using System.Windows.Forms;
using CargoWise.Types;
using Microsoft.AspNetCore.Components;
using WinzorFramework.Extensions;

namespace Enterprise.ZArchitecture
{
	public partial class ZCheckBoxColumnStyle : ZCustomControlColumnStyle
	{
		protected override string GetCellStyleString(CurrencyManager source, int rowNum)
		{
			var bounds = NotificationProvider.ShrinkBoundsForNotificationIcon(InitialBounds);
			var isFocusedCell = source.Position == rowNum && IsEditControlFocused;
			var customRowBackgroundColour = parentZGrid.GetCustomRowBackgroundColour(rowNum, IsCellReadOnly(source, rowNum));

			if (IsCellReadOnly(source, rowNum) && customRowBackgroundColour.ToArgb() == 0)
			{
				customRowBackgroundColour = parentZGrid.ReadOnlyColorForRowNum(rowNum);
			}

			return $"background-color:{customRowBackgroundColour.GetColorStyleValue()};text-align:center;padding-left:{bounds.Left}px;";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Partial url")]
		protected override MarkupString GetRenderContent(CurrencyManager source, int rowNum, bool alignToRight)
		{
			var isDisabled = IsCurrentCellReadOnly ? " onclick=\"return false;\"" : "";
			var value = new ZBool(GetColumnValueAtRow(source, rowNum));
			var isChecked = value ? " checked" : "";
			if (IsCurrentCellReadOnly)
			{
				return new MarkupString($"<input class=\"checkbox__input checkbox__input--align-middle\" type=\"checkbox\"{isChecked}{isDisabled} tabindex=\"-1\">");
			}
			return new MarkupString($"<input class=\"checkbox__input checkbox__input--align-middle checkbox__input--editable\" type=\"checkbox\"{isChecked}{isDisabled}>");
		}

		protected internal override void HideEditControl()
		{
			base.HideEditControl();
			parentZGrid.InvalidateRow(parentZGrid.CurrentRowIndex);
		}
	}
}
