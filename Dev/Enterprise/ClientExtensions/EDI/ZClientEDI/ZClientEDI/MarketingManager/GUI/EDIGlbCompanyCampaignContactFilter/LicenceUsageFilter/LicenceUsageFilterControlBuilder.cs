using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.MarketingManager.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.MarketingManager
{
	public class LicenceUsageFilterControlBuilder : ZFilterStrip.CustomFilterControlsBuilder
	{
		public override bool Handles(ModuleFilter moduleFilter)
		{
			return moduleFilter is LicenceUsageFilter;
		}

		public override Control[] GetFilterControls(ZFilterStrip parentStrip, ModuleFilter moduleFilter, ZBindingSource bindingSource)
		{
			var dateRangeControl = new ZDateRangeControl(parentStrip);
			ControlDpiScalingHelper.SetHeight(ref dateRangeControl, dateRangeControl.FromDateEdit.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(3), false);
			dateRangeControl.TabIndex = 2;
			bindingSource.SetBindingMember(dateRangeControl, ".");

			var dropEdit1 = new ZDropEdit();

			var usageFilter = (LicenceUsageFilter)moduleFilter;
			if (usageFilter.IsBilledFilter)
			{
				dropEdit1.CaptionResourceString = Res.GetData("d03d9c6d-ba5e-4e85-94f2-95d8f18d531a", "System");

				dropEdit1.CharacterCasing = CharacterCasing.Normal;
				ControlDpiScalingHelper.SetTop(ref dropEdit1, dateRangeControl.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(2), false);
				ControlDpiScalingHelper.SetLeft(ref dropEdit1, parentStrip.FilterControlsBox1Start, true);
				ControlDpiScalingHelper.SetWidth(ref dropEdit1, parentStrip.FilterControlBoxWidth, true);
				ControlDpiScalingHelper.SetWidth(dropEdit1.CodeBox, ZFilterStrip.DropListCodeBoxWidth, true);
				dropEdit1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
				dropEdit1.ShowDescriptionBox = true;
				dropEdit1.TabIndex = 3;
				dropEdit1.BindTo = "PriceHeaderCode";
				bindingSource.SetBindingMember(dropEdit1, dropEdit1.BindTo);
			}

			var dropEditPriceItem = new ZDropEdit();
			dropEditPriceItem.CaptionResourceString = Res.GetData("A1DB9CEE-04C8-4041-A29F-32E1D678EC8A", "Usage");
			dropEditPriceItem.CharacterCasing = CharacterCasing.Normal;
			ControlDpiScalingHelper.SetTop(ref dropEditPriceItem, (usageFilter.IsBilledFilter ? dropEdit1.Bottom : dateRangeControl.Bottom) + ControlDpiScalingHelper.ScaleToCurrentDpiY(2), false);
			ControlDpiScalingHelper.SetLeft(ref dropEditPriceItem, parentStrip.FilterControlsBox1Start, true);
			ControlDpiScalingHelper.SetWidth(ref dropEditPriceItem, parentStrip.FilterControlBoxWidth, true);
			ControlDpiScalingHelper.SetWidth(dropEditPriceItem.CodeBox, ZFilterStrip.DropListCodeBoxWidth, true);
			dropEditPriceItem.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
			dropEditPriceItem.ShowDescriptionBox = true;
			dropEditPriceItem.TabIndex = 4;
			dropEditPriceItem.BindTo = "PriceItemCode";
			bindingSource.SetBindingMember(dropEditPriceItem, dropEditPriceItem.BindTo);

			ZCodeFindBox countryCodeFindBox = new ZCodeFindBox();
			countryCodeFindBox.AllowDrop = false;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			countryCodeFindBox.CaptionResourceString = ZClientEDI.Res.GetData("69CA0407-F289-4E07-9876-B3BB3B817074", "Country/Region", "Country/Region of the Company.");
			countryCodeFindBox.Name = "CountryCodeFindBox";

			ControlDpiScalingHelper.SetTop(ref countryCodeFindBox, dropEditPriceItem.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(2), false);
			ControlDpiScalingHelper.SetLeft(ref countryCodeFindBox, parentStrip.FilterControlsBox1Start, true);
			ControlDpiScalingHelper.SetWidth(ref countryCodeFindBox, parentStrip.FilterControlBoxWidth, true);
			ControlDpiScalingHelper.SetWidth(countryCodeFindBox.CodeBox, ZFilterStrip.DropListCodeBoxWidth, true);
			countryCodeFindBox.BindTo = "CountryCode";
			countryCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			countryCodeFindBox.TabIndex = 5;
			bindingSource.SetBindingMember(countryCodeFindBox, countryCodeFindBox.BindTo);

			var dropEdit2 = new ZDropEdit();
			dropEdit2.CaptionResourceString = Res.GetData("be1381b5-6b8c-4504-b886-0ce6f6d096cc", "Usage Count");
			dropEdit2.CharacterCasing = CharacterCasing.Normal;
			ControlDpiScalingHelper.SetTop(ref dropEdit2, countryCodeFindBox.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(2), false);
			ControlDpiScalingHelper.SetLeft(ref dropEdit2, parentStrip.FilterControlsBox1Start, true);
			ControlDpiScalingHelper.SetWidth(ref dropEdit2, parentStrip.FilterControlBoxWidth, true);
			ControlDpiScalingHelper.SetWidth(dropEdit2.CodeBox, ZFilterStrip.DropListCodeBoxWidth * 2, true);
			dropEdit2.ShowDescriptionBox = false;
			dropEdit2.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
			dropEdit2.TabIndex = 6;
			dropEdit2.BindTo = "UsageCountComparisonOperator";
			bindingSource.SetBindingMember(dropEdit2, dropEdit2.BindTo);

			parentStrip.SetHeight(dropEdit2.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(2));

			var calcEdit1 = new ZCalcEdit();
			ControlDpiScalingHelper.SetTop(ref calcEdit1, dropEdit2.Top, false);
			ControlDpiScalingHelper.SetWidth(ref calcEdit1, dateRangeControl.ToDateEdit.Width, false);  //To correctly calculate FilterControlsBox2Start - must be before
			ControlDpiScalingHelper.SetLeft(ref calcEdit1, parentStrip.FilterControlsBox2Start(calcEdit1), true);
			ControlDpiScalingHelper.SetWidth(ref calcEdit1, dateRangeControl.ToDateEdit.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStripDateEdit.CalendarButtonWidthZ), false);
			calcEdit1.DecimalPlaces = 0;
			calcEdit1.TabIndex = 5;
			calcEdit1.BindTo = "UsageCount";
			bindingSource.SetBindingMember(calcEdit1, calcEdit1.BindTo);

			if (!usageFilter.IsBilledFilter)
			{
				dropEdit1.Dispose();
			}

			return usageFilter.IsBilledFilter ? new Control[] { dateRangeControl, dropEdit1, dropEditPriceItem, countryCodeFindBox, dropEdit2, calcEdit1 } :
				new Control[] { dateRangeControl, dropEditPriceItem, countryCodeFindBox, dropEdit2, calcEdit1 };
		}
	}
}
