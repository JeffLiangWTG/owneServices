using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CargoWise.Common.Collections
{
	public interface ISpan<T>
	where T : IComparable<T>
	{
		T Start { get; }
		T End { get; }
	}

	public static class ISpanExtensions
	{
		/// <summary>
		/// Map a function over all of the intersecting spans in a set of spans.
		/// 
		/// For example, if there two spans (1, 3) and (2, 4) there will be three intersections
		/// (1, 2) (2, 3) (3, 4)
		/// </summary>
		/// <typeparam name="TResult">The result of the selector function</typeparam>
		/// <typeparam name="TSpan">A span, consisting of a start and endpoint.</typeparam>
		/// <typeparam name="TSpanNode">The type of the span elements.</typeparam>
		/// <param name="spans">The set of spans</param>
		/// <param name="getStartNode">Func to get the start node</param>
		/// <param name="getEndNode">Func to get the end node</param>
		/// <param name="getResult">Func to get the result</param>
		/// <returns></returns>
		[SuppressMessage("Microsoft.Design", "CA1006:Do not nest generic types in member signatures")]
		public static IEnumerable<TResult> MapOverlap<TResult, TSpan, TSpanNode>(this IEnumerable<TSpan> spans, Func<TSpan, TSpanNode> getStartNode, Func<TSpan, TSpanNode> getEndNode, Func<TSpanNode, TSpanNode, IEnumerable<TSpan>, TResult> getResult)
			where TSpanNode : IComparable<TSpanNode>
		{
			Argument.NotNull(spans, nameof(spans));
			Argument.NotNull(getStartNode, nameof(getStartNode));
			Argument.NotNull(getEndNode, nameof(getEndNode));
			Argument.NotNull(getResult, nameof(getResult));

			var comparer = new SpanKeyComparer<TSpanNode, TSpan>();
			var orderedSpans = new SortedSet<SpanKey<TSpanNode, TSpan>>(comparer);
			var stack = new HashSet<TSpan>();
			var result = new List<TResult>();

			spans = spans.ToArray();

			foreach (var spanGroup in spans.GroupBy(getStartNode))
			{
				orderedSpans.Add(new SpanKey<TSpanNode, TSpan>(spanGroup, true));
			}

			foreach (var spanGroup in spans.GroupBy(getEndNode))
			{
				orderedSpans.Add(new SpanKey<TSpanNode, TSpan>(spanGroup, false));
			}

			if (orderedSpans.Count > 0)
			{
				var startSpan = orderedSpans.First().Key;

				foreach (var group in orderedSpans)
				{
					var endSpan = group.Key;

					if (stack.Any() && startSpan.CompareTo(endSpan) != 0)
					{
						result.Add(getResult(startSpan, endSpan, stack));
					}

					if (group.IsStart)
					{
						foreach (var span in group.Nodes)
						{
							stack.Add(span);
						}
					}
					else
					{
						foreach (var span in group.Nodes)
						{
							stack.Remove(span);
						}
					}

					startSpan = endSpan;
				}
			}

			return result;
		}

		[SuppressMessage("Microsoft.Design", "CA1006:Do not nest generic types in member signatures")]
		public static IEnumerable<TResult> MapOverlap<TResult, TSpan, TSpanNode>(this IEnumerable<TSpan> spans, Func<TSpanNode, TSpanNode, IEnumerable<TSpan>, TResult> getResult)
			where TSpan : ISpan<TSpanNode>
			where TSpanNode : IComparable<TSpanNode>
		{
			return spans.MapOverlap(g => g.Start, g => g.End, getResult);
		}

		/// <summary>
		/// Given a sequence [a, b, c, d, e]
		/// withOverlap: true => will call the `func` with parameters (a, b), (b, c), (c, d), (d, e)
		/// withOverlap: frue => will call the `func` with parameters (a, b), (c, d), (e, f), (g, h)
		/// </summary>
		public static IEnumerable<TResult> SelectSequencedPairs<T, TResult>(this IEnumerable<T> items, Func<T, T, TResult> func, bool withOverlap = true)
		{
			var shouldSkip = !withOverlap;
			var skip = false;
			using (var itr = items.GetEnumerator())
			{
				if (itr.MoveNext())
				{
					var previous = itr.Current;

					while (itr.MoveNext())
					{
						var current = itr.Current;
						if (!skip)
						{
							skip = shouldSkip;
							yield return func(previous, current);
						}
						else
						{
							skip = !shouldSkip;
						}
						previous = current;
					}
				}
			}
		}

		class SpanKey<T, TElement>
			where T : IComparable<T>
		{
			public SpanKey(IGrouping<T, TElement> spanGroup, bool isStart)
			{
				IsStart = isStart;
				Key = spanGroup.Key;
				Nodes = spanGroup;
			}

			public T Key { get; }
			public bool IsStart { get; }
			public IEnumerable<TElement> Nodes { get; }
		}

		class SpanKeyComparer<TKey, TElement> : IComparer<SpanKey<TKey, TElement>>
			where TKey : IComparable<TKey>
		{
			public int Compare(SpanKey<TKey, TElement> x, SpanKey<TKey, TElement> y)
			{
				var result = x.Key.CompareTo(y.Key);
				return result == 0 ? x.IsStart.CompareTo(y.IsStart) * -1 : result;
			}
		}
	}
}
