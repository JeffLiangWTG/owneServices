using System;
using System.Windows.Forms;
using Enterprise.Core.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZDataGridTextBoxColumnInfo : ZGridColumnInfo
	{
		public ZDataGridTextBoxColumnInfo(string columnName, int width)
			: base(columnName, width)
		{
		}

		public ZDataGridTextBoxColumnInfo(string columnName, int width, CharacterCasing characterCasing, bool visible, bool mandatory, bool readOnly, bool sortable)
			: base(columnName, width, characterCasing, visible, mandatory, readOnly, sortable)
		{
		}

		public ZDataGridTextBoxColumnInfo()
		{
		}

		public override Type ColumnStyleType
		{
			get { return typeof(ZDataGridTextBoxColumnStyle); }
		}
	}

	public class ZDataGridTextBoxColumnStyle : ZGridColumnStyle
	{
		public ZDataGridTextBoxColumnStyle(ZGridColumnInfo columnInfo)
			: base(columnInfo)
		{
		}
	}
}
