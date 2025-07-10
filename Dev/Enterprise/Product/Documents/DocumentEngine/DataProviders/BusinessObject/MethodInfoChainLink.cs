using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.DataProviders
{
	internal class MethodInfoChainLink
	{
		public MethodInfoChainLink(MethodInfo methodInfo, object[] parameters)
			: this(methodInfo)
		{
			Parameters = parameters;
		}

		public MethodInfoChainLink(MethodInfo methodInfo, string index)
			: this(methodInfo)
		{
			Index = index;
		}

		public MethodInfoChainLink(MethodInfo methodInfo)
		{
			MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo), "There's no point having a MethodInfoChainLink without a MethodInfo. As soon as you have a Link with no MethodInfo, the chain used to Reflect out a value is broken.");

			//check to see if this is a getter of a property with [PasswordAttribute] on it - derived from https://stackoverflow.com/questions/7819489/identify-whether-a-methodinfo-instance-is-a-property-accessor
			if (MethodInfo.IsSpecialName && MethodInfo.Name.StartsWith("get_"))
			{
				try
				{
					var prop = MethodInfo.DeclaringType.GetProperty(MethodInfo.Name.Substring(4),
					   BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
					var accessor = prop?.GetGetMethod();
					if (accessor == MethodInfo)
					{
						isPassword = Attribute.GetCustomAttribute(prop, typeof(PasswordAttribute)) != null;
					}
				}
				catch (AmbiguousMatchException)
				{
				}
			}
		}
		internal string PropertyIdentifier { get; set; }

		internal bool ShouldBeIgnoredWhenEvaluating { get; set; }
		public Type TypeToReflect { get; internal set; }

		public readonly MethodInfo MethodInfo;
		public readonly string Index;
		public readonly object[] Parameters;
		readonly bool isPassword;

		public object ReflectOutObject(object parentObject, object topLevelObject)
		{
			var result = ReflectOutObjectCore(parentObject, topLevelObject);

			if (MethodInfo.Name.EndsWith(AuditDetailsColumns.SystemCreateTimeUtc, StringComparison.OrdinalIgnoreCase))
			{
				// HACK: This prevents concurrency related issues as well as makes macros more intuitive.
				// (Specifically, conditional/predicate macro's on objects that have not yet been saved to the database)
				return result is ZDateTime dateTime && !dateTime.IsEmpty ? dateTime : ZDateTime.UtcNow;
			}
			else if (MethodInfo.Name.EndsWith(AuditDetailsColumns.SystemCreateUser, StringComparison.OrdinalIgnoreCase))
			{
				if (result == null && MethodInfo.ReturnType == typeof(GlbStaff))
				{
					return Env.CurrentUser;
				}
				else if (result is ZString user && user.IsEmpty)
				{
					return Env.CurrentUser.Initials;
				}
			}

			return result;
		}

		object ReflectOutObjectCore(object parentObject, object topLevelObject)
		{
			try
			{
				return ReflectOutObjectCore(parentObject);
			}
			catch (ApplicationException exception)
			{
				if (exception.IsCriticalException())
				{
					throw;
				}

				var innerException = exception.InnerException;
				if (innerException != null
					&& (ReportProcessingErrorSeverityExtensions.ExceptionTypesToBeShownToUser.Any(typeToBeShown => typeToBeShown.IsInstanceOfType(innerException))
						|| parentObject is CompositeDataSourceFieldValueProvider))
				{
					throw innerException;
				}

				var exceptionToThrow = new InvalidOperationException(
					FormattableString.Invariant(
						$"Error reflecting property [{MethodInfo.Name}] from parent type [{MethodInfo.ReflectedType.FullName}] {(string.IsNullOrEmpty(PropertyIdentifier) ? string.Empty : $"with property identifier [{PropertyIdentifier}] ")}: {exception.InnerException?.Message ?? string.Empty}"),
					exception)
				{
					Source = topLevelObject?.GetType().FullName ?? string.Empty
				};

				throw exceptionToThrow;
			}
		}

		object ReflectOutObjectCore(object parentObject)
		{
			if (isPassword)
			{
				return "***";
			}

			if (Index != null && typeof(IBODocDataProviderCollection).IsAssignableFrom(MethodInfo.ReturnType))
			{
				var collection = (IBODocDataProviderCollection)MethodInfo.Invoke(parentObject, null);
				return collection == null ? null : collection[Index];
			}
			else if (Index != null)
			{
				if (typeof(IBusinessObjectCollection).IsAssignableFrom(MethodInfo.ReturnType))
				{
					var helper = parentObject as BODocDataProviderCollectionHelper;
					if (helper == null)
					{
						var collection = (IBusinessObjectCollection)MethodInfo.Invoke(parentObject, null);
						if (collection != null)
						{
							helper = new BODocDataProviderCollectionHelper(collection);
						}
					}
					return helper == null ? null : helper[Index];
				}
				else if (MethodInfo.ReturnType == typeof(ZString[]))
				{
					return ((ZString[])MethodInfo.Invoke(parentObject, null))[int.Parse(Index, CultureInfo.InvariantCulture)];
				}
			}
			var parentCollection = parentObject as IBusinessObjectCollection;
			if (typeof(BODocDataProviderCollectionHelper).IsAssignableFrom(MethodInfo.ReflectedType)
				&& Parameters != null
				&& parentCollection != null)
			{
				return MethodInfo.Invoke(new BODocDataProviderCollectionHelper(parentCollection), Parameters);
			}
			else if (Parameters != null)
			{
				return MethodInfo.IsStatic ? MethodInfo.Invoke(null, Prepend(parentObject, Parameters)) : MethodInfo.Invoke(parentObject, Parameters);
			}
			else if (typeof(IBODocDataProviderCollection).IsAssignableFrom(MethodInfo.ReflectedType) && MethodInfo.Name == CollectionIndexerMethodName)
			{
				return parentObject is IBODocDataProviderCollection collection && collection.Count > 0 ? collection[0] : null;
			}
			if (parentCollection != null
				&& MethodInfo.Name == CollectionIndexerMethodName
				&& (typeof(IBusinessObjectCollection).IsAssignableFrom(MethodInfo.ReflectedType)
					|| (typeof(BODocDataProviderCollectionHelper).IsAssignableFrom(MethodInfo.ReflectedType))))
			{
				var collection = new BODocDataProviderCollectionHelper(parentCollection);
				return collection.Count > 0 ? collection[0] : null;
			}
			else
			{
				return MethodInfo.Invoke(parentObject, null);
			}
		}

		static object[] Prepend(object newItem, object[] array)
		{
			var result = new object[array.Length + 1];
			array.CopyTo(result, 1);
			result[0] = newItem;
			return result;
		}

		const string CollectionIndexerMethodName = "get_Item";
	}
}
