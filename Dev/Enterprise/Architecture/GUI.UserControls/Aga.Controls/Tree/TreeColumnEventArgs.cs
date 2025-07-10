using System;

namespace Aga.Controls.Tree
{
	public class TreeColumnEventArgs : EventArgs
	{
		private readonly TreeColumn _column;
		public TreeColumn Column
		{
			get { return _column; }
		}

		public TreeColumnEventArgs(TreeColumn column)
		{
			_column = column;
		}
	}
}
