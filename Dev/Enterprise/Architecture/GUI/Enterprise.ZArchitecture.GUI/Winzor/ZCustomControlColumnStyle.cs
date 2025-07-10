using System.Drawing;
using System.Net;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using Microsoft.AspNetCore.Components;
using WinzorFramework.Extensions;

namespace Enterprise.ZArchitecture
{
	public abstract partial class ZCustomControlColumnStyle : ZTextBoxColumnStyle
	{
		protected override string GetEditControlStyleString(CurrencyManager source, int rowNum)
		{
			var editControlStyleString = base.GetEditControlStyleString(source, rowNum);

			if (!IsCellReadOnly(source, rowNum))
			{
				var isFocusedColor = EnterpriseFormLookStrategy.SelectedControlColor;
				editControlStyleString += $"background-color:{isFocusedColor.GetColorStyleValue()};";
			}

			return editControlStyleString;
		}

		protected override MarkupString GetRenderContent(CurrencyManager source, int rowNum, bool alignToRight)
		{
			return (MarkupString)WebUtility.HtmlEncode(ColumnTextAtRow(source, rowNum));
		}

		public virtual Rectangle GetRowCellBoundsWithActualX()
		{
			int cx = 40;
			int? col = parentDataGrid?.CurrentCell.ColumnNumber;
			Rectangle cellBounds = new Rectangle();
			GridColumnStylesCollection columns = parentDataGrid?.TableStyles[0]?.GridColumnStyles;
			if (columns != null && col != null)
			{
				int firstVisibleCol = parentDataGrid.FirstVisibleColumn;
				for (int i = firstVisibleCol; i < col; i++)
				{
					if (columns[i].PropertyDescriptor != null)
					{
						cx += columns[i].Width;
					}
				}

				int borderWidth = parentDataGrid.TableStyles[0]?.GridLineStyle == DataGridLineStyle.Solid ? 1 : 0;
				cellBounds = new Rectangle(cx,
									 ControlDpiScalingHelper.ScaleToCurrentDpiY(0),
									 columns[col.Value].Width - borderWidth,
									 parentDataGrid.PreferredRowHeight - borderWidth);
			}
			return cellBounds;
		}
	}
}
