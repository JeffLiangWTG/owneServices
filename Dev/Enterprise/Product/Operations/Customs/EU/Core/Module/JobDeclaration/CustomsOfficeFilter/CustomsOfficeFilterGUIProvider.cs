using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Module
{
	public static class CustomsOfficeFilterGUIProvider
	{
		public static Control[] GetCustomsOfficeFilterControls(ZFilterStrip filterStripControl, ZBindingSource bindingSource)
		{
			ZDropEdit operatorDropEdit = new ZDropEdit();
			operatorDropEdit.Name = "operatorDropEdit";
			operatorDropEdit.CharacterCasing = CharacterCasing.Lower;
			operatorDropEdit.CodeBox.TextAlign = HorizontalAlignment.Center;
			operatorDropEdit.CodeBox.Font = filterStripControl.ComparisonOperatorFont;
			operatorDropEdit.PreBoundMaxLength = ZFilterStrip.FilterComparisonOperatorBoxPreBoundMaxLength;
			operatorDropEdit.ShowDescriptionBox = false;
			ControlDpiScalingHelper.SetTop(ref operatorDropEdit, filterStripControl.FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref operatorDropEdit, ControlDpiScalingHelper.ScaleToCurrentDpiX(filterStripControl.FilterControlsBox1Start - ZFilterStrip.SpaceBetweenLabelAndControl) - operatorDropEdit.Width, false);
			operatorDropEdit.BindToList = "ComparisonOperator_List";
			operatorDropEdit.TabIndex = 0;
			bindingSource.SetBindingMember(operatorDropEdit, "ComparisonOperator");

			ZDropEdit purposePropertyDropEdit = new ZDropEdit();
			purposePropertyDropEdit.Name = "typeDropEdit";
			purposePropertyDropEdit.CharacterCasing = CharacterCasing.Upper;
			purposePropertyDropEdit.PreBoundMaxLength = 3;
			purposePropertyDropEdit.ShowDescriptionBox = false;
			ControlDpiScalingHelper.SetTop(ref purposePropertyDropEdit, filterStripControl.FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref purposePropertyDropEdit, filterStripControl.FilterControlsBox2Start(purposePropertyDropEdit), true);
			purposePropertyDropEdit.TabIndex = 3;
			bindingSource.SetBindingMember(purposePropertyDropEdit, "Purpose");

			ZLabel purposeLabel = new ZLabel();
			purposeLabel.Name = "purposeLabel";
			purposeLabel.Text = Res.GetString("69258FD0-5037-4D02-9956-AEB614A84A02", "Purpose:");
			purposeLabel.Size = purposeLabel.PreferredSize;
			ControlDpiScalingHelper.SetWidth(purposeLabel, purposeLabel.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(1), false);
			ControlDpiScalingHelper.SetTop(ref purposeLabel, filterStripControl.LabelTop, false);
			ControlDpiScalingHelper.SetLeft(ref purposeLabel, filterStripControl.Label2Start(purposeLabel, purposePropertyDropEdit), false);
			purposeLabel.TabIndex = 2;

			ZTextBox officeTextBox = new ZTextBox();
			officeTextBox.Name = "officeTextBox";
			ControlDpiScalingHelper.SetTop(ref officeTextBox, filterStripControl.FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref officeTextBox, filterStripControl.FilterControlsBox1Start, true);
			ControlDpiScalingHelper.SetWidth(ref officeTextBox, purposeLabel.Left - officeTextBox.Left - ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.SpaceBetweenLabelAndControl), false);
			officeTextBox.TabIndex = 1;
			bindingSource.SetBindingMember(officeTextBox, "Property");

			return new Control[]
			{
				operatorDropEdit,
				officeTextBox,
				purposeLabel,
				purposePropertyDropEdit,
			};
		}
	}
}
