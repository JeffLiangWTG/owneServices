using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
#if WINZOR
using Microsoft.AspNetCore.Components;
#endif

namespace Enterprise.ZArchitecture.GUI.Internal
{
	[ToolboxItem(false)]
	[DefaultDataSourceBindingMember(null)]
	public partial class ZSingleDateControl : ZUserControl
	{
		#region Construction

		public ZSingleDateControl(ZFilterStrip parentStrip) : this(parentStrip, ZDateTimePickerFormat.Short)
		{
		}

		public ZSingleDateControl(ZFilterStrip parentStrip, ZDateTimePickerFormat dateTimeFormat)
		{
			ParentStrip = parentStrip;
			DateTimeFormat = dateTimeFormat;

			InitializeComponent();
			InitializeControls();
		}

		protected virtual void InitializeControls()
		{
			ControlDpiScalingHelper.SetLeft(this, 0, true);
			ControlDpiScalingHelper.SetTop(ref DateEdit, ParentStrip.FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref DateEdit, ParentStrip.FilterControlsBox1Start, true);
			DateEdit.BindTo = "Property1";
			DateEdit.TabIndex = 3;
			DateEdit.DateTimeFormat = DateTimeFormat;
			LabelCaptionRenderProvider.SetLabelCaptionVisible(DateEdit, false);
		}

		#endregion

		protected ZFilterStrip ParentStrip;
		protected ZDateTimePickerFormat DateTimeFormat;
#if WINZOR
		public override ElementReference? GetFocusElement() => DateEdit.GetFocusElement();
#endif

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
