using System;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public class ZTreeModelEventArgs<T> : ZTreePathEventArgs<T>
		where T : class, IBusiness
	{
		public ZTreeModelEventArgs(ZTreePath<T> path, object[] children)
			: this(path, null, children)
		{
		}

		public ZTreeModelEventArgs(ZTreePath<T> path, int[] indices, object[] children)
			: base(path)
		{
			if (children == null)
			{
				throw new ArgumentNullException(nameof(children));
			}

			if (indices != null && indices.Length != children.Length)
			{
				throw new ArgumentException("indices and children arrays must have the same length");
			}

			Indices = indices;
			Children = children;
		}

		public readonly object[] Children;
		public readonly int[] Indices;
	}
}
