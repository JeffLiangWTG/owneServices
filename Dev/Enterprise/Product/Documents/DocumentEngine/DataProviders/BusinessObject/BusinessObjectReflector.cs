using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Application.Exceptions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors;
using Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors.Core;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineIntegration;
using Enterprise.DocumentEngineIntegration.DocumentParsing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.DataProviders
{
	class BusinessObjectReflector : IBusinessObjectReflector
	{
		readonly static ConcurrentDictionary<string, MethodInfoChainLink[]> ReflectorCache = new ConcurrentDictionary<string, MethodInfoChainLink[]>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Gets a series of MethodInfos relative to SourceObject which lead to the property or method identified by Identifier.
		/// </summary>
		/// <param name="SourceType">A type of a IBODocDataProvider or a IBODocDataProviderCollection</param>
		/// <param name="Identifier">A string with a series of property names, e.g. "InvoiceShipmentArrivalConsolNotifyPartyName". Ambiguous names will be resolved in favour of the longest leftmost match. Dots may be used to resolve ambiguities.</param>
		/// <returns>An array of MethodInfos, or null if Identifier does not exist. These should be called in sequence, starting with SourceObject, to get the value out of each property. The final value will be the object idenfied by Identifier for the given SourceObject.</returns>
		public MethodInfoChainLink[] GetMethodInfoChain(Type typeToReflect, object typeValue, string propertyIdentifier)
		{
			string cacheKey = typeToReflect.FullName + "." + propertyIdentifier;
			return ReflectorCache.GetOrAdd(cacheKey, (k) => GetMethodInfoChainInternal(typeToReflect, typeValue, propertyIdentifier));
		}

		public bool IsPropertyAccessible(Type typeToReflect, string propertyIdentifier)
		{
			var methodInfoChainLink = GetMethodInfoChain(typeToReflect, null, propertyIdentifier);
			return methodInfoChainLink != null;
		}

		[ThreadStatic]
		static Regex regex;
		static Regex Regex
		{
			get
			{
				if (regex == null)
				{
					regex = new Regex(
						@"\.(?<PropertyName>[\w]+)(?:\[(?<Index>[^\]]+)])?(?:(?<!\\)\((?<Parameters>(?>(\\.)|[^\(\)""]+|(?<!\\)\((?<Depth>)|(?<!\\)\)(?<-Depth>)|((?<!\\)""([^\\""]|\\.)*(?<!\\)"")|"")*(?(Depth)(?!)))(?<!\\)\))?"
						, RegexOptions.Compiled | RegexOptions.IgnoreCase);
				}

				return regex;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Property Name")]
		const string CountPropertyName = "Count";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Property Name")]
		const string CollectionItemPropertyName = "Item";

		const string PasswordStoredPasswordSaltPropertyName = "PasswordSalt";
		const string PasswordStoredPasswordHashPropertyName = "PasswordHash";

		MethodInfoChainLink[] GetMethodInfoChainInternal(Type typeToReflect, object typeValue, string propertyIdentifier)
		{
			var result = new List<MethodInfoChainLink>();

			if (typeValue is IAllowMacroAccessToAllPublicProperties)
			{
				typeToReflect = typeValue.GetType();
			}

			foreach (Match match in Regex.Matches(PreparePropertyIdentifierForRegex(propertyIdentifier)))
			{
				var isFunction = match.Groups["Parameters"].Success;
				if (isFunction)
				{
					var functionName = match.Groups["PropertyName"].Value;
					var parameters = match.Groups["Parameters"].Value;

					Type functionReturnType = AddFunctionChainLinks(typeToReflect, result, functionName, parameters);

					if (functionReturnType != null)
					{
						typeToReflect = functionReturnType;
					}
					else
					{
						return null;
					}
				}
				else
				{
					var index = GetIndex(match);
					var propertyName = match.Groups["PropertyName"].Value.Trim();

					while (propertyName.Length > 0)
					{
						var propertyInfo = GetValidPropertyInfo(typeToReflect, propertyName);
						if (propertyInfo != null)
						{
							if (!IsCollectionItem(propertyInfo))
							{
								propertyName = propertyName.Remove(startIndex: 0, count: propertyInfo.Name.Length);
							}

							if (index != null && string.IsNullOrEmpty(propertyName) && IsCollection(propertyInfo.PropertyType) && propertyInfo.PropertyType != typeof(ZString[]))
							{
								typeToReflect = BusinessObjectCollection.GetElementTypeFromCollectionType(propertyInfo.PropertyType);
							}
							else
							{
								typeToReflect = propertyInfo.PropertyType;

								if (typeToReflect.IsInterface && Attribute.IsDefined(propertyInfo, typeof(ResolveTypeFromObjectFactoryForDocDataAttribute), false))
								{
									try
									{
										typeToReflect = ObjectFactory.GetType(typeToReflect);
									}
									catch (NoSuchObjectDefinitionException)
									{
									}
								}
							}

							if (typeValue is IAllowMacroAccessToAllPublicProperties parent)
							{
								(typeValue, typeToReflect) = GetConcreteTypeFromInterfaceIfAllowed(parent, typeToReflect, propertyInfo);
							}

							var shouldBeIgnoredWhenEvaluating = ShouldBeIgnoredWhenEvaluating(propertyInfo);
							result.Add(index != null ? new MethodInfoChainLink(propertyInfo.GetGetMethod(), index) { TypeToReflect = typeToReflect, PropertyIdentifier = propertyIdentifier, ShouldBeIgnoredWhenEvaluating = shouldBeIgnoredWhenEvaluating } :
								new MethodInfoChainLink(propertyInfo.GetGetMethod()) { TypeToReflect = typeToReflect, PropertyIdentifier = propertyIdentifier, ShouldBeIgnoredWhenEvaluating = shouldBeIgnoredWhenEvaluating });
						}
						else
						{
							return null;
						}
					}
				}
			}

			if (typeToReflect != null && RequiresSubsequentToString(typeToReflect))
			{
				var toStringMethod = typeToReflect.GetMethod("ToString", Array.Empty<Type>());
				if (toStringMethod != null)
				{
					result.Add(new MethodInfoChainLink(toStringMethod) { PropertyIdentifier = propertyIdentifier });
				}
				else
				{
					return null;
				}
			}

			if (result.Count > 0)
			{
				return result.ToArray();
			}
			else
			{
				return null;
			}
		}

		(object value, Type type) GetConcreteTypeFromInterfaceIfAllowed(IAllowMacroAccessToAllPublicProperties parentObj, Type type, PropertyInfo propertyInfo)
		{
			object value = null;

			try
			{
				value = propertyInfo.GetValue(parentObj);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{ } // couldon't get property value from this object, noting else to do

			if (value is IAllowMacroAccessToAllPublicProperties)
			{
				type = value.GetType();
			}

			return (value, type);
		}

		PropertyInfo GetValidPropertyInfo(Type type, string propertyName)
		{
			var result = type.GetProperties()
				.Where(propertyInfo => IsValidPropertyInfo(propertyInfo, type, propertyName))
				.MinBySafe(n => n, new PropertyComparer());

			if (result == null && IsCollection(type))
			{
				if (propertyName.Equals(CountPropertyName, StringComparison.OrdinalIgnoreCase))
				{
					result = type.GetProperty(CountPropertyName, typeof(int));
				}
				else
				{
					result = type.GetProperty(CollectionItemPropertyName, new Type[] { typeof(int) });
				}
			}

			if (result == null)
			{
				foreach (var interfaceType in type.GetInterfaces())
				{
					var interfaceTypeResult = interfaceType.GetProperties()
						.Where(propertyInfo => IsValidPropertyInfo(propertyInfo, type, propertyName))
						.MinBySafe(n => n, new PropertyComparer());
					if (interfaceTypeResult != null)
					{
						return interfaceTypeResult;
					}
					if (IsCollection(type))
					{
						if (propertyName.Equals(CountPropertyName, StringComparison.OrdinalIgnoreCase))
						{
							result = interfaceType.GetProperty(CountPropertyName, typeof(int));
						}
						else if (propertyName.Equals(CollectionItemPropertyName, StringComparison.OrdinalIgnoreCase))
						{
							result = interfaceType.GetProperty(CollectionItemPropertyName, new Type[] { typeof(int) });
						}

						if (result != null)
						{
							return result;
						}
					}
				}
			}

			return result;
		}

		bool IsValidPropertyInfo(PropertyInfo propertyInfo, Type type, string propertyName)
		{
			return propertyName.StartsWith(propertyInfo.Name, StringComparison.OrdinalIgnoreCase)
				&& (!IsCollection(type) || (propertyInfo.Name != CollectionItemPropertyName))
				&& IsAllowableType(propertyInfo.PropertyType)
				&& propertyInfo.GetGetMethod() != null;
		}

		bool IsCollectionItem(PropertyInfo propertyInfo)
		{
			var indexParameters = propertyInfo.GetIndexParameters();

			return propertyInfo.Name.Equals(CollectionItemPropertyName, StringComparison.OrdinalIgnoreCase) &&
				indexParameters != null &&
				indexParameters.Length == 1 &&
				indexParameters[0].ParameterType.IsAssignableFrom(typeof(int));
		}

		#region SuppressResourceStringsCheckRegion

		readonly static ImmutableDictionary<string, Func<string, BaseFunctionExtractor>> extractors = new Dictionary<string, Func<string, BaseFunctionExtractor>>()
		{
			{ "total", p => new TotalFunctionExtractor(p) },
			{ "format", p => new FormatFunctionExtractor(p) },
			{ "docdatavalue", p => new DocDataFunctionExtractor(p) },
			{ "find", p => new FindFunctionExtractor(p) },
			{ "getcustomfield", p => new GetCustomFieldFunctionExtractor(p) },
			{ "getcustomfieldwithtype", p => new GetCustomFieldWithTypeFunctionExtractor(p) },
			{ "getcustomfieldcodedescription", p => new GetCustomFieldCodeDescriptionFunctionExtractor(p) },
			{ "getcustomfieldcodedescriptionwithtype", p => new GetCustomFieldCodeDescriptionWithTypeFunctionExtractor(p) },
			{ "geteventlastdatetime", p => new GetEventLastDateTimeFunctionExtractor(p) },
			{ "getmetricranking", p => new GetMetricRankingFunctionExtractor(p) },
			{ "getmetrictargetranking", p => new GetMetricTargetRankingFunctionExtractor(p) },
			{ "getmetrictargetrange", p => new GetMetricTargetRangeFunctionExtractor(p) },
			{ "getmetrictargetrangemin", p => new GetMetricTargetRangeMinFunctionExtractor(p) },
			{ "getmetrictargetrangemax", p => new GetMetricTargetRangeMaxFunctionExtractor(p) },
			{ "getdescriptionfromcode", p => new GetDescriptionFromCodeFunctionExtractor(p) }
		}.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);

		#endregion

		Type AddFunctionChainLinks(Type type, List<MethodInfoChainLink> chainLinks, string functionName, string parameters)
		{
			Type result = null;

			if (extractors.TryGetValue(functionName, out var provider))
			{
				result = provider(parameters).AddMethodInfoChainLinks(type, chainLinks);
			}

			return result;
		}

		string GetIndex(Match match)
		{
			string index = null;

			if (match.Groups["Index"].Success)
			{
				index = match.Groups["Index"].Value;
			}

			return index;
		}

		string PreparePropertyIdentifierForRegex(string propertyIdentifier)
		{
			propertyIdentifier = propertyIdentifier.Trim();
			if (!propertyIdentifier.StartsWith("."))
			{
				propertyIdentifier = "." + propertyIdentifier;
			}

			return propertyIdentifier;
		}

		#region Type Checkers

		internal static bool ShouldBeIgnoredWhenEvaluating(PropertyInfo propertyInfo)
		{
			return Attribute.IsDefined(propertyInfo, typeof(DocumentMacroIgnoreAttribute), false) || IsPasswordStoredProperty(propertyInfo);
		}

		static bool IsPasswordStoredProperty(PropertyInfo propertyInfo)
		{
			return typeof(IPasswordStored).IsAssignableFrom(propertyInfo.ReflectedType)
				&& (propertyInfo.Name.Equals(PasswordStoredPasswordHashPropertyName, StringComparison.OrdinalIgnoreCase) || propertyInfo.Name.Equals(PasswordStoredPasswordSaltPropertyName, StringComparison.OrdinalIgnoreCase));
		}

		internal static bool IsAllowableType(Type type)
		{
			return IsRelatedObject(type) || IsCollection(type) || IsField(type);
		}

		internal static bool IsRelatedObject(Type type)
		{
			return IsIBusiness(type) || IsIBODocDataProvider(type) || IsDataModel(type) || type.IsInterface;
		}

		internal static bool IsCollection(Type type)
		{
			return IsIBODocDataProviderCollection(type) || IsIBusinessObjectCollection(type) || type == typeof(ZString[]);
		}

		internal static bool IsField(Type type)
		{
			return IsZType(type) || IsImage(type) || IsEnum(type) || IsBool(type) || IsDynamicObject(type);
		}

		static bool IsIBusiness(Type type)
		{
			return typeof(IBusiness).IsAssignableFrom(type);
		}

		static bool IsIBODocDataProvider(Type type)
		{
			return typeof(IBODocDataProvider).IsAssignableFrom(type);
		}

		static bool IsEnum(Type type)
		{
			return typeof(Enum).IsAssignableFrom(type);
		}

		static bool IsImage(Type type)
		{
			return typeof(System.Drawing.Image).IsAssignableFrom(type);
		}

		static bool IsZType(Type type)
		{
			return typeof(IZType).IsAssignableFrom(type);
		}

		static bool IsIBusinessObjectCollection(Type type)
		{
			return typeof(IBusinessObjectCollection).IsAssignableFrom(type);
		}

		static bool IsIBODocDataProviderCollection(Type type)
		{
			return typeof(IBODocDataProviderCollection).IsAssignableFrom(type);
		}

		static bool IsBool(Type type)
		{
			return typeof(bool).IsAssignableFrom(type);
		}

		static bool IsDynamicObject(Type type)
		{
			return typeof(DynamicObject).IsAssignableFrom(type);
		}

		static bool IsDataModel(Type type)
		{
			return typeof(TriggerDataModel).IsAssignableFrom(type) || typeof(LogEventDataModel).IsAssignableFrom(type);
		}

		static bool RequiresSubsequentToString(Type type)
		{
			return !IsZType(type) && !IsCollection(type) && !IsImage(type) && !typeof(int).IsAssignableFrom(type);
		}

		#endregion

		class PropertyComparer : IComparer<PropertyInfo>
		{
			public int Compare(PropertyInfo x, PropertyInfo y)
			{
				int result = y.Name.Length.CompareTo(x.Name.Length);
				if (result == 0)
				{
					result = y.Name.CompareTo(x.Name);
					if (result == 0)
					{
						if (y.DeclaringType.IsSubclassOf(x.DeclaringType))
						{
							result = 1;
						}
						else if (x.DeclaringType.IsSubclassOf(y.DeclaringType))
						{
							result = -1;
						}
						else if (x.DeclaringType == y.DeclaringType)
						{
							result = 0;
						}
						else
						{
							// Should never happen?
							ErrorReporter.ReportOnce("DocWrapper property comparer", "Two properties have the same name but their DeclaringTypes are not related.");
						}
					}
				}
				return result;
			}
		}

#if DEBUG
		protected virtual void DoSomethingToPropertiesArrayForTesting(PropertyInfo[] properties)
		{
			// by default, do nothing
		}

		public static void ClearCacheForTesting()
		{
			ReflectorCache.Clear();
		}
#endif
	}
}
