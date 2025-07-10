using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine
{
	public class BODocDataProviderCollectionHelper : IBODocDataProviderCollectionHelper
	{
		public BODocDataProviderCollectionHelper(IBusinessObjectCollection collection)
		{
			Argument.NotNull(collection, "collection");
			this.collection = collection;
		}
		readonly IBusinessObjectCollection collection;

		public IBODocDataProvider this[string index]
		{
			get { return GetRowFromStringIndexer(index) ?? GetRowFromIntIndexer(index); }
		}

		public IBODocDataProvider this[int index]
		{
			get { return BODocDataProvider.Get((BusinessObject)collection[index]); }
		}

		public int Count
		{
			get { return collection.Count; }
		}

		IBODocDataProvider GetRowFromStringIndexer(string index)
		{
			using (new ReEntranceStopper(this))
			{
				if (ReEntranceCount == 1)
				{
					Type typeOfCollection = collection.GetType();
					PropertyInfo indexer = typeOfCollection.GetProperty("Item", new Type[] { typeof(string) });
					if (indexer != null && typeof(BusinessObject).IsAssignableFrom(indexer.PropertyType))
					{
						var bo = indexer.GetValue(collection, new object[] { index }) as BusinessObject;
						if (bo != null)
						{
							return BODocDataProvider.Get(bo);
						}
					}
				}
			}
			return null;
		}

		int ReEntranceCount;

		class ReEntranceStopper : IDisposable
		{
			internal ReEntranceStopper(BODocDataProviderCollectionHelper parent)
			{
				this.parent = parent;
				parent.ReEntranceCount++;
			}
			readonly BODocDataProviderCollectionHelper parent;

			public void Dispose()
			{
				parent.ReEntranceCount--;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		IBODocDataProvider GetRowFromIntIndexer(ZString index)
		{
			index = index.ToLower().Replace(" ", "");
			if (index == "first")
			{
				if (Count > 0)
				{
					return BODocDataProvider.Get((BusinessObject)collection[0]);
				}
			}
			else if (index == "last" || index == "count")
			{
				if (Count > 0)
				{
					return BODocDataProvider.Get((BusinessObject)collection[Count - 1]);
				}
			}
			else if (index.IsNumbersOnlyOrEmpty)
			{
				var oneBasedIndex = ZInt.Zero;
				if (ZInt.TryParse(index, out oneBasedIndex))
				{
					if (oneBasedIndex.IsInRange(1, Count))
					{
						return BODocDataProvider.Get((BusinessObject)collection[oneBasedIndex - 1]);
					}
				}
			}
			else if (index.Left(6) == "count-")
			{
				ZString countFromLastAsString = index.SubstringSafe(6);
				if (countFromLastAsString.IsNumbersOnlyOrEmpty)
				{
					var countFromLast = ZInt.Zero;
					if (ZInt.TryParse(countFromLastAsString, out countFromLast))
					{
						ZInt oneBasedIndex = Count - countFromLast;
						if (oneBasedIndex.IsInRange(1, Count))
						{
							return BODocDataProvider.Get((BusinessObject)collection[oneBasedIndex - 1]);
						}
					}
				}
			}
			return null;
		}

		public ZString Format(ZString formatString, ZString delimiter, ZString filterString, ZString groupByString, ZInt maxItems)
		{
			if (filterString.IsEmpty)
			{
				return FormatCore(formatString, delimiter, groupByString, maxItems, collection.Cast<BusinessObject>());
			}
			else
			{
				var dataProviders = new List<BusinessObject>();

				using (var interpreter = new FormatStringInterpreter() as IFormatStringInterpreter)
				{
					foreach (BusinessObject bizObj in collection)
					{
						try
						{
							var expr = interpreter.Format(bizObj, filterString);
							if (ExpressionEvaluator.Evaluate(expr, RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.Value))
							{
								dataProviders.Add(bizObj);
							}
						}
						catch (ExpressionEvaluationException ex)
						{
							throw new BODocDataProviderCollectionFormatException(Res.GetString("f3b53d24-13bf-41b3-a743-37df72c02b4e", "Unable to format collection as {0} is not a valid filter statement.",
								filterString), ex.InnerException);
						}
					}
					return FormatCore(formatString, delimiter, groupByString, maxItems, dataProviders);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		ZString FormatCore(ZString formatString, ZString delimiter, ZString groupByParameters, ZInt maxItems, IEnumerable<BusinessObject> dataProviders)
		{
			string actualDelimiter = ", ";
			switch (delimiter.ToLower())
			{
				case "colon":
					actualDelimiter = " : ";
					break;
				case "dash":
					actualDelimiter = " - ";
					break;
				case "newline":
					actualDelimiter = "\r\n";
					break;
				case "htmlnewline":
					actualDelimiter = "<br />";
					break;
				case "space":
					actualDelimiter = " ";
					break;
			}

			if (groupByParameters.IsEmpty)
			{
				return CombineValuesWithoutGrouping(formatString, actualDelimiter, maxItems, dataProviders);
			}
			else
			{
				return CombineValuesWithGrouping(formatString, actualDelimiter, groupByParameters, maxItems, dataProviders);
			}
		}

		string CombineValuesWithoutGrouping(string formatString, string delimiter, int maxItems, IEnumerable<BusinessObject> dataProviders)
		{
			var result = new ZStringBuilder();

			var itemsToKeep = maxItems > 0 ? maxItems : dataProviders.Count();
			var itemsKept = 0;

			foreach (BusinessObject dataProvider in dataProviders)
			{
				using (var interpreter = new FormatStringInterpreter() as IFormatStringInterpreter)
				{
					var formattedValue = interpreter.Format(dataProvider, formatString);

					if (!string.IsNullOrEmpty(formattedValue))
					{
						result.Append(formattedValue);
						++itemsKept;
						if (itemsKept >= itemsToKeep)
						{
							break;
						}
					}
				}
			}

			return result.ToStringWithDelimiterBetweenAppends(delimiter);
		}

		string CombineValuesWithGrouping(string formatString, string delimiter, string groupByParameters, int maxItems, IEnumerable<BusinessObject> dataProviders)
		{
			string[] groupByPropertyNames = GetGroupByPropertyNames(groupByParameters);
			var groupedObjects = new Dictionary<string, List<BusinessObject>>();

			foreach (BusinessObject dataProvider in dataProviders)
			{
				ZString groupByParametersValues = GetGroupByKey(dataProvider, groupByPropertyNames);

				List<BusinessObject> list;
				if (!groupedObjects.TryGetValue(groupByParametersValues, out list))
				{
					list = new List<BusinessObject>();
					groupedObjects.Add(groupByParametersValues, list);
				}

				list.Add(dataProvider);
			}

			return GetFormattedValues(formatString, groupedObjects, delimiter, maxItems);
		}

		string GetFormattedValues(string formatString, Dictionary<string, List<BusinessObject>> groupedObjects, string delimiter, int maxItems)
		{
			var result = new ZStringBuilder();
			var itemsToKeep = maxItems > 0 ? maxItems : groupedObjects.Count;
			var itemsKept = 0;

			foreach (KeyValuePair<string, List<BusinessObject>> keyValuePair in groupedObjects)
			{
				using (var interpreter = new FormatStringInterpreter() as IFormatStringInterpreter)
				{
					var collectionScope = keyValuePair.Value;

					interpreter.GetCollectionScopeStrategy = () => collectionScope;

					var formattedValue = interpreter.Format(keyValuePair.Value[0], formatString);

					if (!string.IsNullOrEmpty(formattedValue))
					{
						result.Append(formattedValue);
						++itemsKept;
						if (itemsKept >= itemsToKeep)
						{
							break;
						}
					}
				}
			}

			return result.ToStringWithDelimiterBetweenAppends(delimiter);
		}

		string[] GetGroupByPropertyNames(string groupByParameters)
		{
			var result = new List<string>();
			int current = 0;

			while (current < groupByParameters.Length)
			{
				int openBracePos = groupByParameters.IndexOf('{', current);

				if (openBracePos < 0)
				{
					break;
				}
				else
				{
					int closeBracePos = groupByParameters.IndexOf('}', openBracePos);

					if (closeBracePos < 0)
					{
						throw new ArgumentException("Unmatched open brace in the group by format");
					}

					result.Add(groupByParameters.Substring(openBracePos + 1, closeBracePos - openBracePos - 1));

					current = closeBracePos + 1;
				}
			}

			return result.ToArray();
		}

		string GetGroupByKey(BusinessObject bizObj, string[] propertyNames)
		{
			var result = new ZStringBuilder();
			Type type = bizObj.GetType();

			var reflector = new BusinessObjectReflector();
			var dataProvider = new BusinessObjectDataProvider();

			foreach (string propertyName in propertyNames)
			{
				var methodInfoChain = reflector.GetMethodInfoChain(type, bizObj, propertyName);

				var data = dataProvider.GetFieldValueFromMethodInfoChain(bizObj, methodInfoChain);

				if (data != null)
				{
					result.Append(data.ToString());
				}
			}

			return result.ToStringWithDelimiterBetweenAppends("|");
		}

		public BusinessObject[] GetFilteredBusinessObjects(ZString filter)
		{
			var businessObjects = collection.ToArray();
			if (Count > 0 && !filter.Trim().IsEmpty)
			{
				businessObjects = Array.FindAll(businessObjects, item =>
				{
					using (var interpreter = new FormatStringInterpreter() as IFormatStringInterpreter)
					{
						var expression = interpreter.Format(item, filter.Replace('<', '{').Replace('>', '}'));
						return ExpressionEvaluator.Evaluate(expression, RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.Value);
					}
				});
			}

			return businessObjects;
		}

		public PropertyInfo GetPropertyInfo(ZString fieldName)
		{
			if (Count <= 0)
			{
				return null;
			}

			var typeOfObjectContained = collection[0].GetType();
			return typeOfObjectContained.GetProperty(fieldName);
		}

		public object Total(ZString fieldName, ZString decimalPlaces, ZString filter)
		{
			if (Count > 0)
			{
				var property = GetPropertyInfo(fieldName);
				if (property != null)
				{
					var businessObjects = GetFilteredBusinessObjects(filter);
					return GetTotal(businessObjects, property, decimalPlaces);
				}
			}
			return ZString.Empty;
		}

		static object GetTotal(BusinessObject[] businessObjects, PropertyInfo property, ZString decimalPlaces)
		{
			if (property.PropertyType == typeof(ZInt))
			{
				return GetTotalZInt(businessObjects, property.Name);
			}

			if (property.PropertyType == typeof(ZDecimal))
			{
				return GetTotalZDecimal(businessObjects, decimalPlaces, property.Name);
			}

			if (property.PropertyType == typeof(ZLong))
			{
				return GetTotalZLong(businessObjects, property.Name);
			}

			if (typeof(ITotalValueAndUnits).IsAssignableFrom(property.PropertyType))
			{
				return GetITotalValueAndUnits(businessObjects, property, decimalPlaces);
			}

			return ZString.Empty;
		}

		static ZString GetITotalValueAndUnits(BusinessObject[] businessObjects, PropertyInfo property, ZString decimalPlaces)
		{
			ValueAndUnitSelfTotaller result = null;
			foreach (BusinessObject dataProvider in businessObjects)
			{
				ITotalValueAndUnits line = (ITotalValueAndUnits)property.GetValue(dataProvider, null);
				if (result == null)
				{
					result = line.GetNewForTotalling();
				}
				else
				{
					line.AddSelfToResult(result);
				}
			}
			return result == null ? ZString.Empty : result.ToString(decimalPlaces);
		}

		static object GetTotalZDecimal(BusinessObject[] businessObjects, ZString decimalPlaces, ZString fieldName)
		{
			var result = ZDecimal.Zero;

			foreach (var dataProvider in businessObjects)
			{
				result += (ZDecimal)dataProvider[fieldName];
			}

			if (result.IsEmpty)
			{
				return ZString.Empty;
			}

			int decimals;
			return int.TryParse(decimalPlaces, out decimals) ? ZDecimal.Parse(result.ToString(decimals)) : result;
		}

		static object GetTotalZInt(BusinessObject[] businessObjects, ZString fieldName)
		{
			var result = ZInt.Zero;

			foreach (BusinessObject dataProvider in businessObjects)
			{
				result += (ZInt)dataProvider[fieldName];
			}
			return result.IsEmpty ? string.Empty : result;
		}

		static object GetTotalZLong(BusinessObject[] businessObjects, ZString fieldName)
		{
			var result = ZLong.Zero;

			foreach (BusinessObject dataProvider in businessObjects)
			{
				result += (ZLong)dataProvider[fieldName];
			}
			return result.IsEmpty ? string.Empty : result;
		}

		public BusinessObject Find(ZString match)
		{
			BusinessObject result = null;

			if (Count > 0)
			{
				try
				{
					var useJS = RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.Value;

					var clause = ObjectFactory.Get<IUserDefinedConditionSplitter>().Split(match);

					using (var interpreter = new FormatStringInterpreter() as IFormatStringInterpreter)
					using (Res.TemporarilySwitchLanguage(Res.DefaultLanguage))
					{
						result = Array.Find(collection.ToArray(), element =>
						{
							return clause.EvaluateBy((condition, evaluator) =>
							{
								if (evaluator != null)
								{
									return evaluator();
								}

								var expression = interpreter.Format(element, condition);
								return ExpressionEvaluator.Evaluate(expression, useJS);
							});
						});
					}
				}
				catch (ExpressionEvaluationException e)
				{
					throw new BODocDataProviderCollectionFindException(string.Format("Could not evaluate the following match: {0}", match), e);
				}
			}

			return result;
		}
	}
}
