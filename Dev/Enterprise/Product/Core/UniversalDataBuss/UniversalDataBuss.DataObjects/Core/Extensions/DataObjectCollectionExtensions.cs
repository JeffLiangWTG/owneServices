using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public static class DataObjectCollectionExtensions
	{
		public static void AddRange<T>(this Collection<T> existingCollection, IEnumerable<T> content)
		{
			if (content != null)
			{
				foreach (var item in content)
				{
					existingCollection.Add(item);
				}
			}
		}

		public static TResult MergeCollection<TDataObject, TResult>(this TResult existingCollection, IEnumerable<TDataObject> newCollection, bool keepExistingData, Func<TDataObject, TDataObject, bool> isMatched)
			where TDataObject : IDataObject
			where TResult : ICollection<TDataObject>, new()
		{
			if (keepExistingData)
			{
				return MergeCollection<TDataObject, TResult>(existingCollection, newCollection, isMatched);
			}
			else
			{
				return MergeCollection<TDataObject, TResult>(newCollection, existingCollection, isMatched);
			}
		}

		public static TResult MergeCollectionByCandidateKey<TDataObject, TResult>(this TResult existingCollection, IEnumerable<TDataObject> newCollection, bool keepExistingData)
			where TDataObject : IDataObject
			where TResult : ICollection<TDataObject>, new()
		{
			Func<TDataObject, TDataObject, bool> isMatched = IsMatchedByCandidateKey;
			if (keepExistingData)
			{
				return MergeCollection<TDataObject, TResult>(existingCollection, newCollection, isMatched);
			}
			else
			{
				return MergeCollection<TDataObject, TResult>(newCollection, existingCollection, isMatched);
			}
		}

		static TResult MergeCollection<TDataObject, TResult>(IEnumerable<TDataObject> primaryCollection, IEnumerable<TDataObject> secondaryCollection, Func<TDataObject, TDataObject, bool> isMatched)
			where TDataObject : IDataObject
			where TResult : ICollection<TDataObject>, new()
		{
			var result = new TResult();
			foreach (var item in GetMergedItems(primaryCollection, secondaryCollection, isMatched))
			{
				result.Add(item);
			}

			return result;
		}

		static IEnumerable<TDataObject> GetMergedItems<TDataObject>(IEnumerable<TDataObject> primaryCollection, IEnumerable<TDataObject> secondaryCollection, Func<TDataObject, TDataObject, bool> isMatched) where TDataObject : IDataObject
		{
			if (primaryCollection == null && secondaryCollection == null)
			{
				return Enumerable.Empty<TDataObject>();
			}
			if (primaryCollection == null)
			{
				return secondaryCollection;
			}
			if (secondaryCollection == null)
			{
				return primaryCollection;
			}
			return primaryCollection.Concat(secondaryCollection.Where(secondaryItem => !primaryCollection.Any(primaryItem => isMatched(primaryItem, secondaryItem))));
		}

		static bool IsMatchedByCandidateKey<TDataObject>(TDataObject existingValue, TDataObject valueToMatch)
			where TDataObject : IDataObject
		{
			bool result = false;
			if (existingValue != null && valueToMatch != null)
			{
				var candidateKeyValues = GetCandidateKeyValues(valueToMatch);
				result = candidateKeyValues != null && existingValue.IsMatchedByCandidateKey(candidateKeyValues);
			}
			return result;
		}

		public static W GetMatchedByCandidateKey<W>(this List<W> existingList, W valueToMatch)
			where W : IDataObject
		{
			var candidateKeyValues = GetCandidateKeyValues(valueToMatch);
			if (candidateKeyValues != null)
			{
				foreach (var existingValue in existingList)
				{
					if (existingValue.IsMatchedByCandidateKey(candidateKeyValues))
					{
						return existingValue;
					}
				}
			}

			return default(W);
		}

		static Dictionary<PropertyInfo, object> GetCandidateKeyValues<W>(W valueToMatch)
		{
			Dictionary<PropertyInfo, object> candidateKeyValues = null;
			var propertiesWithCandidateKey = GetPropertiesWithCandidateKey(valueToMatch.GetType());
			if (propertiesWithCandidateKey.Count > 0)
			{
				candidateKeyValues = new Dictionary<PropertyInfo, object>();
				foreach (var propertyWithCandidateKey in propertiesWithCandidateKey)
				{
					var candidateKeyValue = propertyWithCandidateKey.GetValue(valueToMatch, null); // Deteremine which to keep
					candidateKeyValues.Add(propertyWithCandidateKey, candidateKeyValue);
				}
			}
			return candidateKeyValues;
		}

		static bool IsMatchedByCandidateKey(this IDataObject existingValue, Dictionary<PropertyInfo, object> candidateKeyValues)
		{
			bool isMatched = true;
			foreach (var pair in candidateKeyValues)
			{
				var existingCandidateKeyValue = pair.Key.GetValue(existingValue, null);
				var codeBasedCandidateKeyValue = existingCandidateKeyValue as ICodeDataObject;
				if (codeBasedCandidateKeyValue != null)
				{
					if (codeBasedCandidateKeyValue.Code.Equals(((ICodeDataObject)pair.Value).Code))
					{
						continue;
					}
				}
				else if (existingCandidateKeyValue == null ? pair.Value == null : existingCandidateKeyValue.Equals(pair.Value))
				{
					continue;
				}
				isMatched = false;
				break;
			}
			return isMatched;
		}

		static List<PropertyInfo> GetPropertiesWithCandidateKey(Type declarationListValueType)
		{
			List<PropertyInfo> propertiesWithCandidateKey = null;
			if (!TypeWithCandidateKeyDictionary.TryGetValue(declarationListValueType, out propertiesWithCandidateKey))
			{
				var declarationListValueProperties = declarationListValueType.GetProperties();
				propertiesWithCandidateKey = new List<PropertyInfo>();
				foreach (var declarationListValueProperty in declarationListValueProperties)
				{
					if (declarationListValueProperty.GetCustomAttributes(typeof(CandidateKeyAttribute), false).Length > 0)
					{
						propertiesWithCandidateKey.Add(declarationListValueProperty);
					}
				}
			}
			return propertiesWithCandidateKey;
		}

		static Dictionary<Type, List<PropertyInfo>> TypeWithCandidateKeyDictionary
		{
			get { return typeWithCandidateKeyDictionary ?? (typeWithCandidateKeyDictionary = new Dictionary<Type, List<PropertyInfo>>()); }
		}
		[ThreadStatic]
		static Dictionary<Type, List<PropertyInfo>> typeWithCandidateKeyDictionary;
	}
}
