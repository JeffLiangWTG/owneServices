using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public class ZTreePath<T>
		where T : class, IBusiness
	{
		public static ZTreePath<T> Empty()
		{
			return new ZTreePath<T>(System.Array.Empty<ZNode<T>>());
		}

		public ZTreePath(ZNode<T>[] path)
		{
			this.path = path;
		}

		readonly ZNode<T>[] path;

		public ZNode<T>[] FullPath
		{
			get { return path; }
		}

		public ZNode<T> FirstNode
		{
			get
			{
				if (path.Length > 0)
				{
					return path[0];
				}
				else
				{
					return null;
				}
			}
		}

		public bool IsEmpty()
		{
			return (path.Length == 0);
		}
	}
}
