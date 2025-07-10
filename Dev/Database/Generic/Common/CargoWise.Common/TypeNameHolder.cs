using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Common.Design;

namespace CargoWise.Common
{
	/// <summary>
	/// A Type that isn't a real type but is used as a place-holder.
	/// </summary>
	[DebuggerDisplay("{GetType().FullName} FullName={FullName}")]
	[Serializable]
	public class TypeNameHolder : TypeDelegator
#if NETFRAMEWORK
		, System.Runtime.Serialization.ISerializable
#endif
	{
		public TypeNameHolder(string fullName)
			: this(fullName, false)
		{
			Argument.NotNull(fullName, nameof(fullName));
		}

		public TypeNameHolder(string fullName, IServiceProvider serviceProvider)
			: this(fullName, IsTypeExistsInSolution(fullName, serviceProvider))
		{
			Argument.NotNull(fullName, nameof(fullName));
		}

		internal TypeNameHolder(string fullName, bool existsInSolution)
			: base(typeof(void))
		{
			Argument.NotNull(fullName, nameof(fullName));
			this.fullName = fullName;
			this.ExistsThisTypeOrAnyGenericArgumentTypesInSolution = existsInSolution;
		}

#if NETFRAMEWORK
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Contracts", "Requires-32-23")] // Suppressing this message as we add the "FullName" string to info
		protected TypeNameHolder(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: this(info.GetString("FullName"), info.GetBoolean("ExistsInSolution"))
		{
			Argument.NotNull(info, nameof(info)); // Suggested By ReviewBot
		}
#endif

		public bool ExistsThisTypeOrAnyGenericArgumentTypesInSolution { get; private set; }

		public static bool IsTypeFakeAndNotExistsInSolution(Type type)
		{
			TypeNameHolder fakeType = type as TypeNameHolder;
			return fakeType != null && !fakeType.ExistsThisTypeOrAnyGenericArgumentTypesInSolution;
		}

		public static bool IsTypeExistsInSolution(string typeName, IServiceProvider serviceProvider)
		{
			bool result = false;
			if (serviceProvider != null)
			{
				try
				{
					ITypeResolutionService typeResolution = TypeResolutionServiceLocator.Get(serviceProvider);
					Assembly cargowiseDesign = (typeResolution == null) ? Assembly.Load("CargoWise.Design") : typeResolution.GetAssembly(new AssemblyName("CargoWise.Design"));
					if (cargowiseDesign != null)
					{
						Type envdteUtilsType = cargowiseDesign.GetType("CargoWise.Design.DTE.EnvDTEUtil");
						var invokeMember = envdteUtilsType.InvokeMember("IsTypeExistsInSolution", BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, new object[] { serviceProvider, typeName }, CultureInfo.InvariantCulture);
						result = (bool)invokeMember;
					}
				}
				catch (FileNotFoundException)
				{
				}
			}
			return result;
		}

		public override bool Equals(object o)
		{
			TypeNameHolder rhs = o as TypeNameHolder;
			return rhs != null && rhs.FullName == FullName;
		}

		public override string ToString()
		{ return FullName; }

		public override int GetHashCode()
		{ return FullName.GetHashCode(); }

		public override string FullName
		{
			get
			{
				string result = fullName;
				var arguments = this.GetGenericArguments();
				if (arguments.Length > 0)
				{
					if (arguments[0] != null)
					{
						result += "[";
					}
					result += string.Join(",", arguments.Where(x => x != null).Select(x => GetGenericArgumentName(x, arguments.Length > 1)).ToArray());

					if (arguments[arguments.Length - 1] != null)
					{
						result += "]";
					}
				}
				return result;
			}
		}

		public override string Name
		{
			get
			{
				int tick = fullName.IndexOf("[", StringComparison.Ordinal);
				string result = (tick != -1) ? fullName.Substring(0, tick) : fullName;
				int lastDot = result.LastIndexOf(".", StringComparison.Ordinal);
				result = lastDot == -1 ? result : result.Substring(lastDot + 1);
				return result;
			}
		}

		public override string Namespace
		{
			get
			{
				int tick = fullName.IndexOf("[", StringComparison.Ordinal);
				string result = (tick != -1) ? fullName.Substring(0, tick) : fullName;
				int lastDot = result.LastIndexOf(".", StringComparison.Ordinal);
				result = lastDot == -1 ? "" : result.Substring(0, lastDot);
				return result;
			}
		}

		Type[] genericArguments = Array.Empty<Type>();

		public override Type[] GetGenericArguments()
		{
			return genericArguments;
		}

		public void SetGenericArgumentsArraySize(int argumentsCount)
		{
			if (argumentsCount < 0)
			{
				throw new ArgumentException("Invalid argument.", nameof(argumentsCount));
			}

			int argsCount = this.genericArguments.Length + argumentsCount;
			Type[] typeArray = new Type[argsCount];
			Array.Copy(genericArguments, typeArray, genericArguments.Length);
			this.genericArguments = typeArray;
		}

		public static string ChangeTypeNameGenericArgumentCount(string fullName, int count)
		{
			Argument.NotNull(fullName, nameof(fullName)); // Suggested By ReviewBot
			int nameEndIndex = fullName.IndexOfAny(new char[] { '[', '`' });
			int startBracketIndex = fullName.IndexOf('[');
			string result = (nameEndIndex == -1) ? fullName : fullName.Substring(0, nameEndIndex);

			if (count > 0)
			{
				result += "`" + count;
			}
			if (startBracketIndex != -1)
			{
				result += fullName.Substring(startBracketIndex);
			}
			return result;
		}

		public override Type UnderlyingSystemType
		{ get { return this; } }

		#region IsGenericType / IsGenericTypeDefinition / GetGenericTypeDefinition

		public override bool IsGenericType
		{
			get
			{
				bool nameIsGeneric = fullName.Contains("`") && fullName.Contains("[");
				if (nameIsGeneric && this.genericArguments.Length == 0)
				{
					this.genericArguments = new Type[1];
				}
				return nameIsGeneric && this.GetGenericArguments().Length > 0;
			}
		}

		public override bool IsGenericTypeDefinition
		{
			get
			{
				bool nameIsGeneric = fullName.Length > 2 && fullName[fullName.Length - 2] == '`';
				if (nameIsGeneric && this.genericArguments.Length == 0)
				{
					this.genericArguments = new Type[1];
				}
				return nameIsGeneric && this.GetGenericArguments().Length > 0;
			}
		}

		public override Type GetGenericTypeDefinition()
		{
			int paramBracket = FullName.IndexOf("[", StringComparison.Ordinal);
			if (paramBracket == -1)
			{
				throw new InvalidOperationException("Not a generic type");
			}
			var typeNameHolder = new TypeNameHolder(FullName.Substring(0, paramBracket).Trim());
			if (typeNameHolder.GetGenericArguments().Length != this.GetGenericArguments().Length || !typeNameHolder.IsGenericTypeDefinition)
			{
				throw new InvalidOperationException("Not a generic type");
			}
			else
			{
				return typeNameHolder;
			}
		}

		#endregion

		#region MakeGenericType

		public override Type MakeGenericType(params Type[] typeArguments)
		{
			foreach (Type typeArgument in typeArguments)
			{
				if (typeArgument == null)
				{
					throw new ArgumentNullException(nameof(typeArguments), "Array element of typeArguments is null");
				}
			}
			string name = ChangeTypeNameGenericArgumentCount(FullName, typeArguments.Length);
			TypeNameHolder result = new TypeNameHolder(name, ExistsThisTypeOrAnyGenericArgumentTypesInSolution || DoAnyFakeTypesExistInSolution(typeArguments));
			result.genericArguments = typeArguments;
			return result;
		}

		public static Type MakeGenericType(Type genericType, Type[] typeArguments)
		{
			Argument.NotNull(genericType, nameof(genericType)); // Suggested By ReviewBot
			Argument.NotNull(typeArguments, nameof(typeArguments)); // Suggested By ReviewBot
			List<Type> types = new List<Type>();
			types.Add(genericType);
			types.AddRange(typeArguments);

			Type result;
			if (!(genericType is TypeNameHolder) && AreAnyFake(types.ToArray()))
			{
				var typeHolder = new TypeNameHolder(genericType.FullName);
				typeHolder.SetGenericArgumentsArraySize(typeArguments.Length);
				if ((typeHolder.GetGenericArguments() != null && typeHolder.GetGenericArguments().Length != typeArguments.Length) || !typeHolder.IsGenericTypeDefinition)
				{
					throw new ArgumentException("The number of generic arguments does not match the expected amount, or the type is not a generic type definition.", nameof(genericType));
				}
				result = typeHolder.MakeGenericType(typeArguments);
			}
			else
			{
				bool thisTypeExistsInSolution = false;
				try
				{
					if (!(genericType is TypeNameHolder) && !AreAnyFake(types.ToArray()) && genericType.IsGenericTypeDefinition && genericType.GetGenericArguments().Length == typeArguments.Length)
					{
						result = genericType.MakeGenericType(typeArguments);
					}
					else
					{
						thisTypeExistsInSolution = ((TypeNameHolder)genericType).ExistsThisTypeOrAnyGenericArgumentTypesInSolution;
						if ((genericType.GetGenericArguments() != null && genericType.GetGenericArguments().Length != typeArguments.Length) || !genericType.IsGenericTypeDefinition)
						{
							throw new ArgumentException("The number of generic arguments does not match the expected amount, or the type is not a generic type definition.", nameof(genericType));
						}
						result = genericType.MakeGenericType(typeArguments);
					}
				}
				catch (ArgumentException) // when generic constraints aren't satisfied or code contracts requirements are not met
				{
					var typeNameHolder = new TypeNameHolder(genericType.FullName, thisTypeExistsInSolution);
					typeNameHolder.SetGenericArgumentsArraySize(typeArguments.Length);
					result = typeNameHolder.MakeGenericType(typeArguments);
				}
			}
			return result;
		}

		static bool AreAnyFake(Type[] types)
		{
			Argument.NotNull(types, nameof(types)); // Suggested By ReviewBot
			foreach (Type type in types)
			{
				if (type is TypeNameHolder)
				{
					return true;
				}
			}
			return false;
		}

		#endregion

#if NETFRAMEWORK
		#region ISerializable Members

		void System.Runtime.Serialization.ISerializable.GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{ GetObjectData(info, context); }

		protected virtual void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			Argument.NotNull(info, nameof(info)); // Suggested By ReviewBot
			info.AddValue("FullName", fullName);
			info.AddValue("ExistsInSolution", ExistsThisTypeOrAnyGenericArgumentTypesInSolution);
		}

		#endregion
#endif

		#region Implementation

		readonly string fullName;

		static bool DoAnyFakeTypesExistInSolution(Type[] types)
		{
			Argument.NotNull(types, nameof(types)); // Suggested By ReviewBot
			foreach (Type type in types)
			{
				TypeNameHolder fake = type as TypeNameHolder;
				if (fake != null && fake.ExistsThisTypeOrAnyGenericArgumentTypesInSolution)
				{
					return true;
				}
			}
			return false;
		}

		static string GetGenericArgumentName(Type genericArgument, bool hasMoreThanOneGenericArgument)
		{
			Argument.NotNull(genericArgument, nameof(genericArgument)); // Suggested By ReviewBot
			string result = "";
			if (genericArgument is TypeNameHolder)
			{
				result = genericArgument.FullName;
			}
			else
			{
				result = genericArgument.AssemblyQualifiedName;
			}
			if (hasMoreThanOneGenericArgument && result != null && result.Contains(","))
			{
				result = "[" + result + "]";
			}
			return result;
		}

		#endregion
	}
}
