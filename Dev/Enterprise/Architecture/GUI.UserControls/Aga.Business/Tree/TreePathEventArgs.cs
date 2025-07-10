using System;

namespace Aga.Business.Tree
{
	public class TreePathEventArgs : EventArgs
	{
		private readonly TreePath _path;
		public TreePath Path
		{
			get { return _path; }
		}

		public TreePathEventArgs()
		{
			_path = new TreePath();
		}

		public TreePathEventArgs(TreePath path)
		{
			if (path == null)
			{
				throw new ArgumentNullException();
			}

			_path = path;
		}
	}
}
