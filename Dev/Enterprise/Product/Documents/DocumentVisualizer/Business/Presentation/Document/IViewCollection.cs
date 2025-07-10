using System.Collections.Generic;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public interface IViewCollection<T> : IEnumerable<T>
	{
		int Count { get; }

		void Add(T view);

		void Remove(T view);

		void Clear();
	}
}