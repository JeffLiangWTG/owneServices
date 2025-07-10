using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.Common.Collections
{
	public static class ICollectionExtensions
	{
		public static IReadOnlyCollection<TOut> AsReadOnly<TIn, TOut>(this ICollection<TIn> collection, Func<TIn, TOut> selector)
		{
			Argument.NotNull(collection, nameof(collection));
			Argument.NotNull(selector, nameof(selector));

			return new ReadOnlyCollectionWrapper<TIn, TOut>(collection, selector);
		}

		class ReadOnlyCollectionWrapper<TIn, TOut> : IReadOnlyCollection<TOut>
		{
			readonly ICollection<TIn> collection;
			readonly Func<TIn, TOut> selector;

			public ReadOnlyCollectionWrapper(ICollection<TIn> collection, Func<TIn, TOut> selector)
			{
				Argument.NotNull(collection, nameof(collection));
				Argument.NotNull(selector, nameof(selector));

				this.collection = collection;
				this.selector = selector;
			}

			public IEnumerator<TOut> GetEnumerator()
			{
				return collection.Select(selector).GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public int Count { get { return collection.Count; } }
		}
	}
}
