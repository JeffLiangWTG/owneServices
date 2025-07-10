using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BarcodeParsing.GUI
{
	/// <summary>
	/// This class is required to override the max length in the GUI. This is because
	/// the terminator gets wrapped in double quotes, so allowing them to enter in the full amount
	/// will cause an exception as the wrapped value will be 2 more than the allowed max length.
	/// </summary>
	public class ZTextBoxColumnStyleWithMaxLengthInfo : ZTextBoxColumnStyleInfo
	{
		public ZTextBoxColumnStyleWithMaxLengthInfo()
		{
			// -1 means not set.
			MaxLength = -1;
		}

		public override Type ColumnStyleType
		{
			get { return typeof(ZTextBoxColumnStyleWithMaxLength); }
		}

		// so the designer doesn't serialise this value when opening a form designer
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int MaxLength
		{
			get;
			set;
		}
	}

	public class ZTextBoxColumnStyleWithMaxLength : ZTextBoxColumnStyle
	{
		public ZTextBoxColumnStyleWithMaxLength(ZTextBoxColumnStyleWithMaxLengthInfo info)
			: base(info)
		{
		}

		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellVisible)
		{
			base.Edit(source, rowNum, bounds, readOnly, instantText, cellVisible);

			// this is what the base code does to set the max length, essentially we just override with the new value
			var maxLength = MaxLength;
			if (maxLength > -1 && CanEdit && source.Position > -1 && source.Count > 0)
			{
				var current = source.GetCurrent() as BusinessObject;
				if (current != null)
				{
					TextBox.MaxLength = maxLength;
				}
			}
		}

		int MaxLength
		{
			get { return ((ZTextBoxColumnStyleWithMaxLengthInfo)ColumnInfo).MaxLength; }
		}
	}
}
