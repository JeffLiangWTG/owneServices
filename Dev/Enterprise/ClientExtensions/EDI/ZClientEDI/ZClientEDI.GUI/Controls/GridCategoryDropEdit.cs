using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Client.EDI.Billing.GUI
{
	/// <summary>
	/// Shows a drop down list split into categories.
	/// Category items are bold and unselectable...
	/// CATEGORY A
	///		Item1
	///		Item2
	///	CATEGORY B
	///		Item3
	///		Item4
	/// </summary>
	public class GridCategoryDropEdit : ZGridDropEdit
	{
		protected override ZDropCodeBox NewCodeBox()
		{
			return new ZDropCodeBox.Bare();
		}

		protected override ZDropButton NewDropButton()
		{
			return new GridCategoryDropButton();
		}

		protected override bool IsItemValidForAutoCompleteCore(ICodeDescription item)
		{
			return !(item is CategoryCodeDescriptionPair);
		}
	}

	public class GridCategoryDropButton : ZGridDropButton
	{
		protected override ZDropForm NewDropForm()
		{
			return new CategoryDropForm(ParentDropEdit);
		}
	}

	public class CategoryDropEdit : ZDropEdit
	{
		protected override ZDropCodeBox NewCodeBox()
		{
			return new ZDropCodeBox.Bare();
		}

		protected override ZDropButton NewDropButton()
		{
			return new CategoryDropButton();
		}

		protected override bool IsItemValidForAutoCompleteCore(ICodeDescription item)
		{
			return !(item is CategoryCodeDescriptionPair);
		}
	}

	public class CategoryDropButton : ZDropButton
	{
		protected override ZDropForm NewDropForm()
		{
			return new CategoryDropForm(ParentDropEdit);
		}
	}

	public class CategoryDropForm : ZDropForm
	{
		public CategoryDropForm(ZDropEdit parentDropEdit)
			: base(parentDropEdit)
		{
		}

		public override bool IsItemSelectable(ICodeDescription item)
		{
			return item != null && !(item is CategoryCodeDescriptionPair) && item.Code.Length != 0;
		}

		protected override int NextItemIndex
		{
			get
			{
				int result = base.NextItemIndex;

				while (result < List.Count - 1 && !IsItemSelectable((ICodeDescription)List[result]))
				{
					result++;
				}

				if (!IsItemSelectable((ICodeDescription)List[result]))
				{
					result = HighlightedItem;   // Don't move past last visible item
				}

				return result;
			}
		}

		protected override int PreviousItemIndex
		{
			get
			{
				int result = base.PreviousItemIndex;

				while (result > 0 && !IsItemSelectable((ICodeDescription)List[result]))
				{
					result--;
				}

				return result;
			}
		}

		#region Painting an Item

		protected override int DescriptionBackgroundOverlap
		{
			get { return 11; } // help separate the line from the codes when the codebox is small
		}

		protected override bool IncludeItemInCodeWidthCalculation(ICodeDescription item)
		{
			return !(item is CategoryCodeDescriptionPair);
		}

#if !WINZOR
		protected override void PaintItemText(ICodeDescription item, ZString text, Graphics graphics, Font font, Brush textBrush, Rectangle codeRectangle)
		{
			int leftIndent = ControlDpiScalingHelper.ScaleToCurrentDpiX(8);
			ControlDpiScalingHelper.SetX(ref codeRectangle, codeRectangle.X + leftIndent, false);
			ControlDpiScalingHelper.SetWidth(ref codeRectangle, codeRectangle.Width - leftIndent, false);

			if (item is CategoryCodeDescriptionPair)
			{
				font = CategoryFont;

				float textWidth = graphics.MeasureString(item.Code, font).Width;
				float textStart = codeRectangle.X;

				// draw line 1

				int x1 = ControlDpiScalingHelper.ScaleToCurrentDpiX(5);
				int y1 = (int)(codeRectangle.Y + codeRectangle.Height / 2.0f - ControlDpiScalingHelper.ScaleToCurrentDpiY(3));
				int w1 = (int)textStart - x1;
				int h1 = ControlDpiScalingHelper.ScaleToCurrentDpiY(2);
				ControlPaint.DrawBorder(graphics, ControlDpiScalingHelper.NewScaledRectangle(x1, y1, w1, h1, false), SystemColors.ControlDark, ButtonBorderStyle.Dotted);

				// draw line 2

				int x2 = (int)(textStart + textWidth) - ControlDpiScalingHelper.ScaleToCurrentDpiX(2);
				int y2 = y1;
				int w2 = codeRectangle.Right - ScrollBarWidth - x2 - x1;
				int h2 = h1;
				ControlPaint.DrawBorder(graphics, ControlDpiScalingHelper.NewScaledRectangle(x2, y2, w2, h2, false), SystemColors.ControlDark, ButtonBorderStyle.Dotted);

				ControlDpiScalingHelper.SetY(ref codeRectangle, codeRectangle.Y - ControlDpiScalingHelper.ScaleToCurrentDpiY(3), false);
				ControlDpiScalingHelper.SetX(ref codeRectangle, (int)textStart, false);

				// cover the dotted separator that is displayed when Descriptions are visible
				//if (ParentDropEdit.ShowDescriptionInDropDown)
				//{
				//	graphics.FillRectangle(SystemBrushes.ControlLight,
				//		DottedLineBounds.X, codeRectangle.Y, DottedLineBounds.Width, codeRectangle.Height);
				//}
			}

			base.PaintItemText(item, text, graphics, font, textBrush, codeRectangle);
		}

		int ScrollBarWidth
		{
			get { return (ScrollBar != null) ? ScrollBar.Width : 0; }
		}

		Font CategoryFont
		{
			get
			{
				if (categoryFont == null)
				{
					categoryFont = new Font("Arial", 9.0f, FontStyle.Bold);
				}
				return categoryFont;
			}
		}

		Font categoryFont;

#endif

		#endregion
	}

	public class CategoryDropEditColumnStyleInfo : ZDropEditColumnStyleInfo
	{
		public CategoryDropEditColumnStyleInfo(string columnName, int width, int maxDropDownItems)
			: base(columnName, width, maxDropDownItems)
		{ }

		public CategoryDropEditColumnStyleInfo(string columnName, int width)
			: base(columnName, width)
		{ }

		public CategoryDropEditColumnStyleInfo() { } // required for ZGrid column designer

		public override Type ColumnStyleType
		{
			get { return typeof(CategoryDropEditColumnStyle); }
		}
	}

	public class CategoryDropEditColumnStyle : ZDropEditColumnStyle
	{
		public CategoryDropEditColumnStyle(CategoryDropEditColumnStyleInfo columnInfo)
			: this(columnInfo, () => new GridCategoryDropEdit())
		{
		}

		public CategoryDropEditColumnStyle(CategoryDropEditColumnStyleInfo columnInfo, Func<Control> editControl)
			: base(columnInfo, editControl)
		{
		}
	}
}
