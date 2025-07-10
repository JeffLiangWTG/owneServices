using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging
{
	public static class XMLMessageBuilderExtension
	{
		public static TResult[] Convert<TInput, TResult>(this IEnumerable<TInput> collection, Func<TInput, TResult> convertMethod) => collection?.WhereNotNull()?.Select(convertMethod).ToArray();

		public static Collection<TResult> ConvertToCollection<TInput, TResult>(this IEnumerable<TInput> collection, Func<TInput, TResult> convertMethod)
		{
			var result = Convert(collection, convertMethod);
			return result != null ? new Collection<TResult>(result) : null;
		}

		public static Collection<string> ConvertToStringCollection(this IEnumerable<ZString> inputList)
		{
			var result = ConvertToArray(inputList);
			return result != null ? new Collection<string>(result) : null;
		}

		public static string[] ConvertToArray(this IEnumerable<ZString> inputList)
		{
			var collectionWithNoEmpty = inputList?.Where(x => !x.IsEmpty).Select(x => x.ToString()) ?? Enumerable.Empty<string>();
			return collectionWithNoEmpty.Any() ? collectionWithNoEmpty.ToArray() : null;
		}

		public static string GetValueOrNull(this ZString field)
		{
			return field.IsEmpty ? null : (string)field;
		}

		public static Collection<int> ConvertToIntCollectionWithZero(this IEnumerable<ZInt> inputList)
		{
			var result = ConvertToArrayWithZero(inputList);
			return result != null ? new Collection<int>(result) : null;
		}

		public static int[] ConvertToArrayWithZero(this IEnumerable<ZInt> inputList)
		{
			var collectionWithNoEmpty = inputList?.Select(x => (int)x) ?? Enumerable.Empty<int>();
			return collectionWithNoEmpty.Any() ? collectionWithNoEmpty.ToArray() : null;
		}
	}
}
