using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Res = Enterprise.Customs.Common.GUI.Res;

namespace Enterprise.Customs.Common
{
	public static class EntryStatusFilterGUIProvider
	{
		public static Control[] GetEntryStatusFilterControls(ZFilterStrip filterStripControl, ZBindingSource bindingSource, bool useComparisonOperator, bool useFilterType)
		{
			var operatorDropEdit = new ZDropEdit();
			operatorDropEdit.Name = "operatorDropEdit";
			operatorDropEdit.CharacterCasing = CharacterCasing.Lower;
			operatorDropEdit.CodeBox.TextAlign = HorizontalAlignment.Center;
			operatorDropEdit.CodeBox.Font = filterStripControl.ComparisonOperatorFont;
			operatorDropEdit.PreBoundMaxLength = ZFilterStrip.FilterComparisonOperatorBoxPreBoundMaxLength;
			operatorDropEdit.ShowDescriptionBox = false;
			operatorDropEdit.Visible = useComparisonOperator;
			ControlDpiScalingHelper.SetTop(ref operatorDropEdit, filterStripControl.FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref operatorDropEdit, ControlDpiScalingHelper.ScaleToCurrentDpiX(filterStripControl.FilterControlsBox1Start) - ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.SpaceBetweenLabelAndControl) - operatorDropEdit.Width, false);
			operatorDropEdit.BindToList = "ComparisonOperator_List";
			operatorDropEdit.TabIndex = 0;
			bindingSource.SetBindingMember(operatorDropEdit, "ComparisonOperator");

			var typePropertyDropEdit = new ZDropEdit();
			typePropertyDropEdit.Name = "typeDropEdit";
			typePropertyDropEdit.CharacterCasing = CharacterCasing.Normal;
			typePropertyDropEdit.ShowDescriptionBox = false;
			typePropertyDropEdit.Visible = useFilterType;
			ControlDpiScalingHelper.SetTop(ref typePropertyDropEdit, filterStripControl.FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref typePropertyDropEdit, filterStripControl.FilterControlsBox2Start(typePropertyDropEdit) + 50, true);
			typePropertyDropEdit.TabIndex = 3;
			bindingSource.SetBindingMember(typePropertyDropEdit, "FilterType");

			var typeLabel = new ZLabel();
			typeLabel.Name = "typeLabel";
			typeLabel.Text = Res.GetString("337B3A20-2208-4291-BE37-8B0D561500AE", "Type");
			typeLabel.Size = typeLabel.PreferredSize;
			typeLabel.Visible = useFilterType;
			ControlDpiScalingHelper.SetTop(ref typeLabel, filterStripControl.LabelTop, false);
			ControlDpiScalingHelper.SetLeft(ref typeLabel, filterStripControl.Label2Start(typeLabel, typePropertyDropEdit) + ControlDpiScalingHelper.ScaleToCurrentDpiX(50), false);
			typeLabel.TabIndex = 2;

			var entryStatusDropEdit = new ZDropEdit();
			entryStatusDropEdit.Name = "entryStatusDropEdit";
			entryStatusDropEdit.CharacterCasing = CharacterCasing.Upper;
			entryStatusDropEdit.ShowDescriptionBox = true;

			var dropEditWidth = useFilterType ? EntryStatusDropEditWidth : ExtendedEntryStatusDropEditWidth;

			ControlDpiScalingHelper.SetWidth(ref entryStatusDropEdit, dropEditWidth, true);
			ControlDpiScalingHelper.SetTop(ref entryStatusDropEdit, filterStripControl.FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref entryStatusDropEdit, filterStripControl.FilterControlsBox1Start, true);
			entryStatusDropEdit.TabIndex = 1;
			bindingSource.SetBindingMember(entryStatusDropEdit, "Property");

			return new Control[] { operatorDropEdit, entryStatusDropEdit, typeLabel, typePropertyDropEdit };
		}

		const int EntryStatusDropEditWidth = 200;
		const int ExtendedEntryStatusDropEditWidth = 300;
	}
}
