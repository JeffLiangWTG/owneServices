using System;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public class ZTreePathEventArgs<T> : EventArgs
		where T : class, IBusiness
	{
		public ZTreePathEventArgs(ZTreePath<T> path)
		{
			if (path == null)
			{
				throw new ArgumentNullException(nameof(path));
			}

			Path = path;
		}

		public readonly ZTreePath<T> Path;
	}
}
