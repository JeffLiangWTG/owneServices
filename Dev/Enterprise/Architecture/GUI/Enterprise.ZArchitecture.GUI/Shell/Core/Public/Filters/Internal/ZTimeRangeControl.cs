using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
#if WINZOR
using Microsoft.AspNetCore.Components;
#endif

namespace Enterprise.ZArchitecture.GUI.Internal
{
	[DefaultDataSourceBindingMember(null)]
	public partial class ZTimeRangeControl : ZUserControl
	{
		#region Construction

		public ZTimeRangeControl()
			: this(null)
		{
		}

		public ZTimeRangeControl(ZFilterStrip parentStrip)
		{
			ParentStrip = parentStrip;
			InitializeComponent();
			InitializeControls();
		}

#if WINZOR
		public override ElementReference? GetFocusElement() => PropertySearchDropEdit.GetFocusElement();
#endif

		protected virtual void InitializeControls()
		{
			ControlDpiScalingHelper.SetLeft(this, 0, true);

			UpdatePropertySearchDropEditLayout();
			PropertySearchDropEdit.FormattingEnabled = true;
			PropertySearchDropEdit.BindTo = "PropertySearch";
			PropertySearchDropEdit.BindToList = "PropertySearch_List";
			PropertySearchDropEdit.TabIndex = 1;

			FromTimeEdit.BindTo = "Property1";
			FromTimeEdit.TabIndex = 2;
			AddTimeEditVisibleChangedEventHandler(FromTimeEdit);

			ToTimeEdit.BindTo = "Property2";
			ToTimeEdit.TabIndex = 3;

			if (ParentStrip != null)
			{
				ControlDpiScalingHelper.SetTop(ref PropertySearchDropEdit, ParentStrip.FilterControlTop, false);
				ControlDpiScalingHelper.SetTop(ref FromTimeEdit, ParentStrip.FilterControlTop, false);
				ControlDpiScalingHelper.SetTop(ref ToTimeEdit, ParentStrip.FilterControlTop, false);
				ControlDpiScalingHelper.SetLeft(ref FromTimeEdit, labelWidth + ParentStrip.FilterControlsBox1Start, true);
				ControlDpiScalingHelper.SetLeft(ref ToTimeEdit, ParentStrip.FilterControlsBox2Start(ToTimeEdit), true);
			}
			else
			{
				ControlDpiScalingHelper.SetTop(ref PropertySearchDropEdit, 0, true);
				ControlDpiScalingHelper.SetTop(ref FromTimeEdit, 0, true);
				ControlDpiScalingHelper.SetTop(ref ToTimeEdit, 0, true);
				ControlDpiScalingHelper.SetLeft(ref FromTimeEdit, ControlDpiScalingHelper.ScaleToCurrentDpiX(labelWidth) + PropertySearchDropEdit.Width, false);
				ControlDpiScalingHelper.SetLeft(ref ToTimeEdit, ControlDpiScalingHelper.ScaleToCurrentDpiX(labelWidth * 2) + PropertySearchDropEdit.Width + FromTimeEdit.Width, false);
			}
		}

		#endregion

		#region Binding

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (Filter != null)
			{
				Filter.PropertySearchInfo.ValueChanged -= new EventHandler(PropertySearchInfo_ValueChanged);
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (Filter != null)
			{
				Filter.PropertySearchInfo.ValueChanged += new EventHandler(PropertySearchInfo_ValueChanged);
				UpdateControls();
			}
		}

		#endregion

		#region Showing / Hiding the date controls

		void PropertySearchInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateControls();
		}

		void UpdateControls()
		{
			var dateFilter = Filter;

			ShowTimeControls = dateFilter != null && (dateFilter.IsPropertySearchUsingSpecifiedTimeRange);
		}

		void AddTimeEditVisibleChangedEventHandler(ZFilterStripTimeEdit dateEdit)
		{
			dateEdit.VisibleChanged += new EventHandler(this.TimeEditVisibleChanged);
		}

		void TimeEditVisibleChanged(object sender, EventArgs e)
		{
			if (((ZFilterStripTimeEdit)sender).Visible)
			{
				((ZFilterStripTimeEdit)sender).Focus();
			}
		}

		protected bool ShowTimeControls
		{
			get { return showTimeControls; }
			set
			{
				showTimeControls = value;
				SuspendLayoutAndSetVisibility(value, FromTimeEdit, ToTimeEdit);
			}
		}

		void SuspendLayoutAndSetVisibility(bool visible, params Control[] controls)
		{
			SuspendLayout();
			try
			{
				foreach (var control in controls)
				{
					control.Visible = visible;
				}

				UpdatePropertySearchDropEditLayout();
			}
			finally
			{
				ResumeLayout(true);
			}
		}

		protected virtual void UpdatePropertySearchDropEditLayout()
		{
			PropertySearchDropEdit.ShowDescriptionBox = !ShowTimeControls;

			if (ShowTimeControls)
			{
				PropertySearchDropEdit.ForeColor = SystemColors.GrayText;
				PropertySearchDropEdit.PreBoundMaxLength = ZFilterStrip.FilterComparisonOperatorBoxPreBoundMaxLength;

				if (ParentStrip != null)
				{
					ControlDpiScalingHelper.SetLeft(ref PropertySearchDropEdit, ParentStrip.FilterComparisonOperatorBoxStart, false);
					ControlDpiScalingHelper.SetLeft(ref ToTimeEdit, ParentStrip.FilterControlsBox2Start(ToTimeEdit), true);
				}
				else
				{
					ControlDpiScalingHelper.SetLeft(ref PropertySearchDropEdit, 0, true);
					ControlDpiScalingHelper.SetLeft(ref FromTimeEdit, ControlDpiScalingHelper.ScaleToCurrentDpiX(labelWidth) + PropertySearchDropEdit.Width, false);
					ControlDpiScalingHelper.SetLeft(ref ToTimeEdit, ControlDpiScalingHelper.ScaleToCurrentDpiX(labelWidth * 2) + PropertySearchDropEdit.Width + FromTimeEdit.Width, false);
				}
			}
			else
			{
				PropertySearchDropEdit.ForeColor = SystemColors.WindowText;
				ControlDpiScalingHelper.SetWidth(PropertySearchDropEdit.CodeBox, ZFilterStrip.DropListDateBoxWidth, true);
				// This is to reposition the selected text so you can read it all, instead of showing the ending part of the text
				PropertySearchDropEdit.CodeBox.SelectionLength = 0;
				PropertySearchDropEdit.CodeBox.SelectAll();
				PropertySearchDropEdit.PreBoundMaxLength = 0;
				PropertySearchDropEdit.CodeBox.UpdateSelectedIndexAndDescription();

				if (ParentStrip != null)
				{
					ControlDpiScalingHelper.SetLeft(ref PropertySearchDropEdit, ParentStrip.FilterControlsBox1Start, true);
					ControlDpiScalingHelper.SetWidth(ref PropertySearchDropEdit, ParentStrip.FilterControlBoxWidth, true);
				}
				else
				{
					ControlDpiScalingHelper.SetLeft(ref PropertySearchDropEdit, 0, true);
					ControlDpiScalingHelper.SetWidth(ref PropertySearchDropEdit, ZFilterStrip.FilterControlBoxMaxWidth, true);
				}
			}
		}

		bool showTimeControls;
		protected ZFilterStrip ParentStrip;

		protected ModuleTimeFilter Filter
		{
			get { return (ModuleTimeFilter)CurrentDataItem; }
		}

		#endregion

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

		const int labelWidth = 100;

		#endregion
	}
}
