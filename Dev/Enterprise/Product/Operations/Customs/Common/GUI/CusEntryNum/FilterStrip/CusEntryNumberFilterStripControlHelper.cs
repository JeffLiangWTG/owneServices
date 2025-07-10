using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.Common.Module
{
	public static class CusEntryNumberFilterStripControlHelper
	{
		public static bool IsModuleFilterSupported(ModuleFilter moduleFilter)
		{
			return moduleFilter is CusEntryNumDateFilter || moduleFilter is CusEntryNumTextFilter;
		}

		public static Control[] GetFilterControls(ZFilterStrip filterStrip, ModuleFilter moduleFilter, ZBindingSource bindingSource)
		{
			Control[] result = null;
			if (moduleFilter is CusEntryNumDateFilter)
			{
				result = GetCusEntryNumberDateFilterControls(filterStrip, bindingSource);
			}
			else if (moduleFilter is CusEntryNumTextFilter)
			{
				result = GetCusEntryNumberStringFilterControls(filterStrip, bindingSource);
			}

			return result;
		}

		static Control[] GetCusEntryNumberStringFilterControls(ZFilterStrip filterStrip, ZBindingSource bindingSource)
		{
			ZDropEdit operatorDropEdit = new ZDropEdit();
			operatorDropEdit.CharacterCasing = CharacterCasing.Lower;
			operatorDropEdit.CodeBox.TextAlign = HorizontalAlignment.Center;
			operatorDropEdit.CodeBox.Font = filterStrip.ComparisonOperatorFont;
			operatorDropEdit.ShowDescriptionBox = false;
			ControlDpiScalingHelper.SetTop(ref operatorDropEdit, filterStrip.FilterControlTop, false);
			operatorDropEdit.PreBoundMaxLength = ZFilterStrip.FilterComparisonOperatorBoxPreBoundMaxLength;
			ControlDpiScalingHelper.SetLeft(ref operatorDropEdit, filterStrip.FilterControlsBox1Start, true);
			operatorDropEdit.BindToList = "ComparisonOperator_List";
			operatorDropEdit.TabIndex = 1;
			bindingSource.SetBindingMember(operatorDropEdit, "ComparisonOperator");

			ZTextBox textBox = new ZTextBox();
			ControlDpiScalingHelper.SetTop(ref textBox, filterStrip.FilterControlTop, false);
			ControlDpiScalingHelper.SetWidth(ref textBox, filterStrip.FilterControlBoxWidth - ControlDpiScalingHelper.UnscaleFromCurrentDpiX(operatorDropEdit.Width) - ZFilterStrip.SpaceBetweenLabelAndControl * 2, true);
			ControlDpiScalingHelper.SetLeft(ref textBox, filterStrip.FilterControlsBox2Start(textBox), true);
			textBox.TabIndex = 2;
			textBox.BindTo = "Property";
			bindingSource.SetBindingMember(textBox, "Property");

			ZFilterStripDropEdit entryTypeDropEdit = GetEntryTypeDropEditControl(filterStrip);
			bindingSource.SetBindingMember(entryTypeDropEdit, entryTypeDropEdit.BindTo);

			return new Control[] { operatorDropEdit, textBox, entryTypeDropEdit };
		}

		static Control[] GetCusEntryNumberDateFilterControls(ZFilterStrip filterStrip, ZBindingSource bindingSource)
		{
			CusEntryNumberDateFilterControl result = new CusEntryNumberDateFilterControl(filterStrip);
			bindingSource.SetBindingMember(result, ".");
			return new Control[] { result };
		}

		static ZFilterStripDropEdit GetEntryTypeDropEditControl(ZFilterStrip filterStrip)
		{
			ZFilterStripDropEdit entryTypeDropEdit = new ZFilterStripDropEdit();
			entryTypeDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			ControlDpiScalingHelper.SetWidth(ref entryTypeDropEdit, ZFilterStrip.DropListCodeBoxWidth, true);
			ControlDpiScalingHelper.SetWidth(entryTypeDropEdit.Controls["CodeBox"], ZFilterStrip.DropListCodeBoxWidth, true);
			ControlDpiScalingHelper.SetTop(ref entryTypeDropEdit, filterStrip.FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref entryTypeDropEdit, filterStrip.FilterControlsBox1Start - ZFilterStrip.DropListCodeBoxWidth - 8, true);
			entryTypeDropEdit.Name = "EntryTypeDropEdit";
			entryTypeDropEdit.MaxItemsToShowInDropDown = 28;
			entryTypeDropEdit.ShowDescriptionBox = false;
			entryTypeDropEdit.TabIndex = 0;
			entryTypeDropEdit.BindTo = "EntryType";
			entryTypeDropEdit.BindToList = "AdditionalReferenceNumberTypes";
			return entryTypeDropEdit;
		}

		[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
		class CusEntryNumberDateFilterControl : ZDateRangeControl
		{
			public CusEntryNumberDateFilterControl(ZFilterStrip parentStrip)
				: base(parentStrip)
			{
				this.Controls.Add(GetEntryTypeDropEditControl(parentStrip));
			}

			protected override void InitializeControls()
			{
				base.InitializeControls();
				ControlDpiScalingHelper.SetWidth(ref FromDateEdit, FromDateEdit.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(13), false);
				ControlDpiScalingHelper.SetWidth(ref ToDateEdit, ToDateEdit.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(13), false);
				ControlDpiScalingHelper.SetLeft(ref FromDateEdit, ControlDpiScalingHelper.ScaleToCurrentDpiX(ParentStrip.FilterControlsBox1Start + 4) + PropertySearchDropEdit.Width, false);
				ControlDpiScalingHelper.SetLeft(ref ToDateEdit, ParentStrip.FilterControlsBox2Start(ToDateEdit) + 0, true);
			}

			protected override void UpdatePropertySearchDropEditLayout()
			{
				base.UpdatePropertySearchDropEditLayout();
				if (ShowDateControls)
				{
					ControlDpiScalingHelper.SetLeft(ref PropertySearchDropEdit, ParentStrip.FilterControlsBox1Start, true);
					PropertySearchDropEdit.PreBoundMaxLength = ZFilterStrip.FilterComparisonOperatorBoxPreBoundMaxLength;
					ControlDpiScalingHelper.SetLeft(ref FromDateEdit, PropertySearchDropEdit.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(4), false);
					if (FromDateEdit.Right > ToDateEdit.Left)
					{
						int overlap = FromDateEdit.Right - ToDateEdit.Left;
						ControlDpiScalingHelper.SetWidth(FromDateEdit, FromDateEdit.Width - (overlap / 2 + ControlDpiScalingHelper.ScaleToCurrentDpiX(1)), false);
						ControlDpiScalingHelper.SetWidth(ToDateEdit, ToDateEdit.Width - (overlap / 2 + ControlDpiScalingHelper.ScaleToCurrentDpiX(1)), false);
						ControlDpiScalingHelper.SetLeft(ToDateEdit, ToDateEdit.Left + overlap / 2 + ControlDpiScalingHelper.ScaleToCurrentDpiX(2), false);
					}
				}
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
			public static PropertyDescriptor[] GetPropertyDescriptors()
			{
				return new ControlPropertyDescriptorBuilder<CusEntryNumberDateFilterControl>().Result;
			}
		}
	}
}
