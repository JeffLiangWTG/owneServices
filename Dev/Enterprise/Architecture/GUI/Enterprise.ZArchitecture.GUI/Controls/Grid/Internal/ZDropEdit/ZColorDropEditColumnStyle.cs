using System;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	#region ZColorDropEditColumnStyleInfo

	public class ZColorDropEditColumnStyleInfo : ZDropEditColumnStyleInfo
	{
		public ZColorDropEditColumnStyleInfo(string columnName, int width, int maxDropDownItems) : base(columnName, width, maxDropDownItems) { }

		public ZColorDropEditColumnStyleInfo(string columnName, int width) : this(columnName, width, DefaultMaxDropDownItems) { }

		public ZColorDropEditColumnStyleInfo() { } // required for ZGrid column designer

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType
		{
			get { return typeof(ZColorDropEditColumnStyle); }
		}
	}

	#endregion

	#region ZColorDropEditColumnStyle

	public class ZColorDropEditColumnStyle : ZDropEditColumnStyle
	{
		public ZColorDropEditColumnStyle(ZDropEditColumnStyleInfo columnInfo) : this(columnInfo, () => new ZGridDropEdit())
		{
			DropEdit.ShowColorInDropDown = true;
			DropEdit.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode;
		}

		public ZColorDropEditColumnStyle(ZDropEditColumnStyleInfo columnInfo, Func<Control> editControl) : base(columnInfo, editControl) { }
	}

	#endregion
}
