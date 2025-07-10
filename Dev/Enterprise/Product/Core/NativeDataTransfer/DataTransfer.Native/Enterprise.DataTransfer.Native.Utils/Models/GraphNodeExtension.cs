using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DataTransfer.Native.Utils.Models
{
	public static class GraphNodeExtension
	{
		public static IEnumerable<T> Decendents<T>(this IGraphNode<T> self)
			where T : IGraphNode<T>
		{
			var decendents = new List<T>();
			foreach (var child in self.Children)
			{
				decendents.Add(child);
				decendents.AddRange(child.Decendents());
			}

			return decendents;
		}

		public static IEnumerable<T> DecendentsAndSelf<T>(this IGraphNode<T> self)
			where T : IGraphNode<T>
		{
			var decendents = self.Decendents();
			return decendents.Union(new[] { self.Self });
		}

		public static IEnumerable<T> Relatives<T>(this T self)
			where T : class, IGraphNode<T>
		{
			return self.DepthFirstTraversal();
		}

		public static bool IsRelative<T>(this T start, T end)
			where T : class, IGraphNode<T>
		{
			return start.Relatives().Contains(end);
		}

		#region Depth First Traversal

		public class VisitedNodeList<T> where T : IGraphNode
		{
			readonly IDictionary<Guid, T> dictionary = new Dictionary<Guid, T>();

			public bool ContainsKey(T graphNode)
			{
				return dictionary.ContainsKey(graphNode.ID);
			}

			public ICollection<T> Keys
			{
				get { return dictionary.Values; }
			}

			public void AddValue(T graphNode)
			{
				dictionary[graphNode.ID] = graphNode;
			}
		}

		public static IEnumerable<T> DepthFirstTraversal<T>(this T start, Action<T, T> preAction = null, Action<T, T> midAction = null, Action<T, T> postAction = null)
			where T : class, IGraphNode<T>
		{
			var visited = new VisitedNodeList<T>();

			if (start == null)
			{
				return visited.Keys;
			}

			DepthNestedTraversal(start, null, visited, preAction, midAction, postAction);

			return visited.Keys.Distinct();
		}

		public static void DepthNestedTraversal<T>(T self, T relative, VisitedNodeList<T> visited, Action<T, T> preAction, Action<T, T> midAction, Action<T, T> postAction)
			where T : IGraphNode<T>
		{
			visited.AddValue(self);

			preAction?.Invoke(self, relative);

			foreach (var parent in self.Parents)
			{
				if (!visited.ContainsKey(parent))
				{
					DepthNestedTraversal(parent, self, visited, preAction, midAction, postAction);
				}
			}

			midAction?.Invoke(self, relative);

			foreach (var child in self.Children)
			{
				if (!visited.ContainsKey(child))
				{
					DepthNestedTraversal(child, self, visited, preAction, midAction, postAction);
				}
			}

			postAction?.Invoke(self, relative);
		}

		#endregion

		#region Find Path
		public interface IPathFinder<T> where T : class, IGraphNode<T>
		{
			Stack<T> GetPath(Guid startID, Guid endID, Func<Stack<T>> fallback);
		}

#if DEBUG
		public
#endif
		static class FindPathCache<T>
		{
			[ThreadSafe]
			static readonly Dictionary<(Guid, Guid), Stack<T>> _cachedPaths = [];
			[ThreadSafe]
			static readonly ReaderWriterLockSlim _cachedPathsLock = new ReaderWriterLockSlim();

			public static Stack<T> GetPath(Guid start, Guid end, Func<Stack<T>> calculator)
			{
				try
				{
					_cachedPathsLock.EnterReadLock();

					if (!_cachedPaths.TryGetValue((start, end), out var result))
					{
						_cachedPathsLock.ExitReadLock();
						_cachedPathsLock.EnterWriteLock();

						if (!_cachedPaths.TryGetValue((start, end), out result))
						{
							_cachedPaths[(start, end)] = result = calculator();
						}
					}

					return Clone(result);
				}
				finally
				{
					if (_cachedPathsLock.IsWriteLockHeld)
					{
						_cachedPathsLock.ExitWriteLock();
					}
					else if (_cachedPathsLock.IsReadLockHeld)
					{
						_cachedPathsLock.ExitReadLock();
					}
				}
			}

			static Stack<T> Clone(Stack<T> original)
			{
				var arr = new T[original.Count];
				original.CopyTo(arr, 0);
				Array.Reverse(arr);
				return new Stack<T>(arr);
			}
		}

		class PathFinder<T> : IPathFinder<T> where T : class, IGraphNode<T>
		{
			public Stack<T> GetPath(Guid startID, Guid endID, Func<Stack<T>> fallback)
			{
				return FindPathCache<T>.GetPath(startID, endID, fallback);
			}
		}
		/// <summary>
		/// Find shortest path between start and end node
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="pathFinder"></param>
		/// <returns></returns>

		public static Stack<T> FindPath<T>(this T start, T end, IPathFinder<T> pathFinder = null)
			where T : class, IGraphNode<T>
		{
			if (pathFinder == null)
			{
				pathFinder = new PathFinder<T>();
			}
			return pathFinder.GetPath(start.ID, end.ID, () => !start.IsRelative(end) ? new Stack<T>() : FindPath(start, end, excepted: null));
		}

		public static Stack<T> FindPath<T>(this T start, T end, IEnumerable<T> excepted, int maxDepth = int.MaxValue)
			where T : IGraphNode<T>
		{
			if (start.Equals(end))
			{
				var stack = new Stack<T>();
				stack.Push(start);
				return stack;
			}

			if (maxDepth <= 0)
			{
				return new Stack<T>();
			}

			var collection = start.Children.Union(start.Parents);
			if (excepted != null)
			{
				collection = collection.Except(excepted);
				excepted = excepted.Union(collection);
			}
			else
			{
				excepted = collection.Concat(new[] { start });
			}

			if (collection.Contains(end))
			{
				var stack = new Stack<T>();
				stack.Push(end);
				stack.Push(start);
				return stack;
			}

			Stack<T> shortestPath = null;
			foreach (var t in collection)
			{
				var path = FindPath(t, end, excepted, maxDepth - 1);
				int pathDepth = path.Count;
				if (pathDepth > 0)
				{
					path.Push(start);
					if (shortestPath == null || path.Count < shortestPath.Count)
					{
						shortestPath = path;
					}
					maxDepth = Math.Min(maxDepth, pathDepth);
				}
			}
			return shortestPath ?? new Stack<T>();
		}

		#endregion
	}
}
