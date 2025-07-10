using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZNoteDescriptionColumnStyleInfo : ZDropEditColumnStyleInfo
	{
		public override Type ColumnStyleType => typeof(ZNoteDescriptionColumnStyle);
	}

	public class ZNoteDescriptionColumnStyle : ZDropEditColumnStyle
	{
		public ZNoteDescriptionColumnStyle(ZNoteDescriptionColumnStyleInfo columnInfo)
			: base(columnInfo, () => new ZNoteDescriptionDropEdit())
		{
		}

		bool isTextBox;

		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellVisible)
		{
			isTextBox = source.Position >= 0 && source.Position < source.Count && ((StmNote)source.GetCurrent()).ST_IsCustomDescription;

			base.Edit(source, rowNum, bounds, readOnly, instantText, cellVisible);
		}

		protected override Rectangle GetEditControlBounds(Rectangle bounds)
		{
			Rectangle result;
			var dropEdit = (ZGridDropEdit)EditControl;

			dropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;

			if (isTextBox)
			{
				dropEdit.IsDropButtonVisible = false;
				result = bounds;
			}
			else
			{
				dropEdit.IsDropButtonVisible = true;
				result = base.GetEditControlBounds(bounds);
			}

			return result;
		}
	}
}
