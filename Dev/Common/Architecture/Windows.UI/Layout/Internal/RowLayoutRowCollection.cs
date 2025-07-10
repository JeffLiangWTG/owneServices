using System.Collections.Generic;
using System.Windows.Forms;

namespace CargoWise.Windows.UI.Layout
{
	internal class RowLayoutRowCollection : IEnumerable<RowLayoutRow>
	{
		public RowLayoutRowCollection()
		{
		}

		public RowLayoutRowCollection(RowLayout owner)
		{
			this.owner = owner;
		}

		public RowLayoutRow this[int i]
		{
			get
			{
				while (i >= List.Count)
				{
					List.Add(new RowLayoutRow(owner));
				}
				return List[i];
			}
		}

		public int Count
		{
			get { return List.Count; }
		}

		public int GetVisibleRowNumber(int row)
		{
			int result = 0;
			if (row == -1)
			{
				result = 0;
			}
			else if (IsDesigning)
			{
				result = row;
			}
			else if (!this[row].Visible)
			{
				result = -1;
			}
			else
			{
				for (int i = 0; i <= row; i++)
				{
					if (i == row)
					{
						break;
					}
					if (this[i].Visible)
					{
						result++;
					}
				}
			}
			return result;
		}

		public int GetRowNumber(int visibleRow)
		{
			int result = visibleRow;
			if (!IsDesigning)
			{
				result = 0;
				int currentVisibleRow = 0;
				for (int i = 0; i < List.Count; i++)
				{
					if (this[i].Visible)
					{
						if (currentVisibleRow++ == visibleRow)
						{
							break;
						}
					}
					result++;
				}
			}
			return result;
		}

		public int GetRowNumberFromControl(Control control)
		{
			for (int i = 0; i < List.Count; i++)
			{
				if (List[i].Controls.Contains(control))
				{
					return i;
				}
			}
			return -1;
		}

		#region IEnumerable Members

		public IEnumerator<RowLayoutRow> GetEnumerator()
		{
			return List.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region Implementation

		readonly RowLayout owner;
		readonly List<RowLayoutRow> List = new List<RowLayoutRow>();

		protected virtual bool IsDesigning
		{
			get { return owner != null && owner.Container != null && owner.Container.Site != null && owner.Container.Site.DesignMode; }
		}

		#endregion
	}
}
