using System;
using System.Web.UI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZButtonColumn : ButtonColumn, IUniqueKeyColumn
	{
		public ZButtonColumn(String headerText, string bindTo)
		{
			this.HeaderText = headerText;
			this.DataTextField = bindTo;
			this.SortExpression = bindTo;
			this.ButtonType = ButtonColumnType.LinkButton;
		}

		#region IUniqueKeyColumn Members

		public int ColumnIndex { get; set; }

		public object ColumnKey { get; set; }

		public int UniqueKey
		{
			get
			{
				if (ColumnKey == null)
				{
					return ColumnIndex;
				}
				return (int)ColumnKey;
			}
		}

		#endregion
	}
}
