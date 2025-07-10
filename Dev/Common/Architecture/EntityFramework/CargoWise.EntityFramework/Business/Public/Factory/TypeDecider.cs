using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Integration;

namespace CargoWise.EntityFramework
{
	[WTG.StaticAnalysis.Annotation.Immutable]
	public abstract class TypeDecider : ITypeDecider
	{
		public abstract Type GetTypeForNew();

		public Type GetTypeForNew(ITypeDeciderContext context) => GetTypeForNewCore(context);

		protected virtual Type GetTypeForNewCore(ITypeDeciderContext context) => GetTypeForNew();

		public abstract Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory);

		Type ITypeDecider.GetTypeForLoad(DataRow row, object factory)
		{
			return GetTypeForLoad(row, (BusinessObjectFactory)factory);
		}

		/// <summary>
		/// This is used when determining the type to get the properties from when binding.
		/// 
		/// WARNING: This method should be used when there can only be 1 possible sub-class per row instance. For example,
		/// in a client override you are overriding the type of the object only once and that override stays through the
		/// lifetime of the application.
		/// </summary>
		public abstract Type GetTypeForBinding();

		#region Type Deciding Static Methods

		public static Type GetTypeForBinding(Type initialType)
		{
#if DEBUG
			if (VisualStudioDetector.IsVisualStudio)
			{
				return initialType;
			}
#endif
			Type result = initialType;
			TypeDecider decider = GetTypeDeciderFromType(initialType);
			Type decidedType = null;

			if (decider != null)
			{
				decidedType = decider.GetTypeForBinding();
			}

			if (decidedType != null)
			{
				result = decidedType;
			}

			if (result != null)
			{
				TypeDecider clientDecider = GetClientTypeDeciderFromType(result);
				if (clientDecider != null)
				{
					decidedType = clientDecider.GetTypeForBinding();
				}

				if (decidedType != null)
				{
					result = decidedType;
				}
			}

			if (result != null)
			{
				lock (syncObject)
				{
					if (substitutions.ContainsKey(result))
					{
						result = substitutions[result];
					}
				}
			}

			return result;
		}

		static Type GetTypeToCreate(DataRow row, Type initialType, BusinessObjectFactory factory, ITypeDeciderContext context)
		{
			Type result = null;
			var decider = GetTypeDeciderFromType(initialType);

			if (row.RowState == DataRowState.Detached)
			{
				if (decider != null)
				{
					result = decider.GetTypeForNew(context);
				}

				var clientDecider = GetClientTypeDeciderFromType(result ?? initialType);
				if (clientDecider != null)
				{
					result = clientDecider.GetTypeForNew(context);
				}
			}
			else
			{
				if (decider != null)
				{
					result = decider.GetTypeForLoad(row, factory);
				}

				var clientDecider = GetClientTypeDeciderFromType(result ?? initialType);
				if (clientDecider != null)
				{
					result = clientDecider.GetTypeForLoad(row, factory);
				}
			}
			return result;
		}

		internal static Type GetTypeToCreateFromRow(DataRow row, Type initialType, BusinessObjectFactory factory, ITypeDeciderContext context)
		{
			if (initialType == null)
			{
				throw new ArgumentNullException(nameof(initialType), "Don't pass in a NULL business object type to the Factory, please!");
			}

			Type result = initialType;
			var decidedTypes = new List<Type>();
			do
			{
				decidedTypes.Add(result);
				result = GetTypeToCreate(row, result, factory, context);
			}
			while (result != null && result.IsAbstract && !decidedTypes.Contains(result));

			if (result == null)
			{
				result = initialType;
			}

			lock (syncObject)
			{
				if (substitutions.ContainsKey(result))
				{
					result = substitutions[result];
				}
			}

			if (result.IsAbstract)
			{
				throw new NoConcreteTypeException("Abstract business object type was type decided. (" + initialType + ")");
			}

			return result;
		}

		#endregion

		#region GetTypeDeciderFromType / GetClientTypeDeciderFromType

		public static TypeDecider GetTypeDeciderFromType(Type type)
		{
			TypeDecider result = null;

			lock (syncObject)
			{
				if (!TypeDeciderCache.TryGetValue(type, out result))
				{
					FieldInfo typeDeciderInfo = type.GetField("TypeDecider", BindingFlags.Public | BindingFlags.Static);
					if (typeDeciderInfo != null)
					{
						result = (TypeDecider)typeDeciderInfo.GetValue(null);
					}

					TypeDeciderCache.Add(type, result);
				}
			}

			return result;
		}

		protected static TypeDecider GetClientTypeDeciderFromType(Type type)
		{
			TypeDecider result = null;
			IClientHook clientHook = ClientHookLoader.ClientHook;
			if (clientHook != null && clientHook.ClientTypeDeciders != null)
			{
				result = (TypeDecider)clientHook.ClientTypeDeciders[type];
			}
			return result;
		}

		static IClientHookLoader ClientHookLoader
		{
			get { return clientHookLoader ?? (clientHookLoader = ObjectFactory.Get<IClientHookLoader>()); }
		}
		[ThreadStatic]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "Baseline")]
		static IClientHookLoader clientHookLoader;

		static Dictionary<Type, TypeDecider> TypeDeciderCache
		{
			get
			{
				if (typeDeciderCache == null)
				{
					typeDeciderCache = new Dictionary<Type, TypeDecider>();
					ClientHookLoader.ClientHookChanged += new EventHandler(Instance_ClientHookChanged);
				}
				return typeDeciderCache;
			}
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "Baseline")]
		static Dictionary<Type, TypeDecider> typeDeciderCache;

		static readonly object syncObject = new object();

		#endregion

		#region Substitutions

		public static void AddSubstitution(Type oldType, Type newType)
		{
			lock (syncObject)
			{
				substitutions[oldType] = newType;
			}
		}

		public static void RemoveSubstitution(Type oldType)
		{
			lock (syncObject)
			{
				if (substitutions.ContainsKey(oldType))
				{
					substitutions.Remove(oldType);
				}
			}
		}

		static readonly Dictionary<Type, Type> substitutions = new Dictionary<Type, Type>();

		#endregion

		#region Implementation

		static void Instance_ClientHookChanged(object sender, EventArgs e)
		{
			lock (syncObject)
			{
				TypeDeciderCache.Clear();
			}
		}

		#endregion
	}

	#region TypeDeciderImpl

	[WTG.StaticAnalysis.Annotation.Immutable]
	public class TypeDeciderImpl : TypeDecider
	{
		public TypeDeciderImpl(Type clientType)
		{
			this.ClientType = clientType;
		}

		public readonly Type ClientType;

		public override Type GetTypeForNew()
		{
			return this.ClientType;
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			return this.ClientType;
		}

		public override Type GetTypeForBinding()
		{
			return this.ClientType;
		}
	}

	#endregion
}
