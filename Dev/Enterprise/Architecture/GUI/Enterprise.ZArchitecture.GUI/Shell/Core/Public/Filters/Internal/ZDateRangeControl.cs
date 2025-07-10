using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
#if WINZOR
using Microsoft.AspNetCore.Components;
#endif

namespace Enterprise.ZArchitecture.GUI.Internal
{
	[DefaultDataSourceBindingMember(null)]
	public partial class ZDateRangeControl : ZUserControl
	{
		#region Construction

		public ZDateRangeControl()
			: this(null)
		{
		}

		public ZDateRangeControl(ZFilterStrip parentStrip)
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

			FromDateEdit.BindTo = "Property1";
			FromDateEdit.TabIndex = 2;
			AddDateEditVisibleChangedEventHandler(FromDateEdit);

			ToDateEdit.BindTo = "Property2";
			ToDateEdit.TabIndex = 3;

			FromOffsetHoursTimeEditEx.BindTo = "Property1";
			FromOffsetHoursTimeEditEx.TabIndex = 2;

			ToOffsetHoursTimeEditEx.BindTo = "Property2";
			ToOffsetHoursTimeEditEx.TabIndex = 3;

			FromOffsetDaysCalcEdit.BindTo = nameof(Filter.PropertyDecimal1);
			FromOffsetDaysCalcEdit.TabIndex = 2;

			ToOffsetDaysCalcEdit.BindTo = nameof(Filter.PropertyDecimal2);
			ToOffsetDaysCalcEdit.TabIndex = 3;

			FilterOptionDropEdit.BindTo = "FilterOption";
			FilterOptionDropEdit.TabIndex = 4;

			if (ParentStrip != null)
			{
				ControlDpiScalingHelper.SetTop(ref PropertySearchDropEdit, ParentStrip.FilterControlTop, false);
				ControlDpiScalingHelper.SetTop(ref FromDateEdit, ParentStrip.FilterControlTop, false);
				ControlDpiScalingHelper.SetTop(ref ToDateEdit, ParentStrip.FilterControlTop, false);
				ControlDpiScalingHelper.SetLeft(ref FromDateEdit, labelWidth + ParentStrip.FilterControlsBox1Start, true);
				ControlDpiScalingHelper.SetLeft(ref ToDateEdit, ParentStrip.FilterControlsBox2Start(ToDateEdit), true);
			}
			else
			{
				ControlDpiScalingHelper.SetTop(ref PropertySearchDropEdit, 0, true);
				ControlDpiScalingHelper.SetTop(ref FromDateEdit, 0, true);
				ControlDpiScalingHelper.SetTop(ref ToDateEdit, 0, true);
				ControlDpiScalingHelper.SetLeft(ref FromDateEdit, ControlDpiScalingHelper.ScaleToCurrentDpiX(labelWidth) + PropertySearchDropEdit.Width, false);
				ControlDpiScalingHelper.SetLeft(ref ToDateEdit, ControlDpiScalingHelper.ScaleToCurrentDpiX(labelWidth * 2) + PropertySearchDropEdit.Width + FromDateEdit.Width, false);
			}

			SetOffsetControlLocations(FromOffsetHoursTimeEditEx, ToOffsetHoursTimeEditEx);
			SetOffsetControlLocations(FromOffsetDaysCalcEdit, ToOffsetDaysCalcEdit);

			FromOffsetDaysCalcEdit.MaxValue = ModuleDateFilter.MaxValueForDecimalProperties;
			ToOffsetDaysCalcEdit.MaxValue = ModuleDateFilter.MaxValueForDecimalProperties;

			ControlDpiScalingHelper.SetTop(ref FilterOptionDropEdit, ToOffsetHoursTimeEditEx.Top, false);
			ControlDpiScalingHelper.SetLeft(ref FilterOptionDropEdit, ToOffsetHoursTimeEditEx.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(65), false);
		}

		void SetOffsetControlLocations(Control fromControl, Control toControl)
		{
			ControlDpiScalingHelper.SetTop(ref fromControl, FromDateEdit.Top, false);
			ControlDpiScalingHelper.SetLeft(ref fromControl, FromDateEdit.Left, false);

			ControlDpiScalingHelper.SetTop(ref toControl, fromControl.Top, false);
			ControlDpiScalingHelper.SetLeft(ref toControl, fromControl.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(50), false);
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

			ShowDateControls = dateFilter != null && (dateFilter.IsPropertySearchUsingSpecifiedDateRange || dateFilter.IsPropertySearchUsingSpecifiedDateTimeRange);
			ShowHourOffsetControls = !ShowDateControls && dateFilter != null && (dateFilter.IsPropertySearchUsingSpecifiedHourOffsetRange || dateFilter.IsPropertySearchUsingSpecifiedWorkHourOffsetRange);
			ShowDayOffsetControls = !ShowDateControls && !ShowHourOffsetControls && dateFilter != null && dateFilter.IsPropertySearchUsingSpecifiedDayOffsetRange;
		}

		void AddDateEditVisibleChangedEventHandler(ZFilterStripDateEdit dateEdit)
		{
			dateEdit.VisibleChanged += new EventHandler(this.DateEditVisibleChanged);
		}

		void DateEditVisibleChanged(object sender, EventArgs e)
		{
			if (((ZFilterStripDateEdit)sender).Visible)
			{
				((ZFilterStripDateEdit)sender).Focus();
			}
		}

		protected bool ShowDateControls
		{
			get { return showDateControls; }
			set
			{
				showDateControls = value;
				SuspendLayoutAndSetVisibility(value, FromDateEdit, ToDateEdit);
			}
		}

		bool ShowHourOffsetControls
		{
			get { return showHourOffsetControls; }
			set
			{
				showHourOffsetControls = value;
				SuspendLayoutAndSetVisibility(value, FromOffsetHoursTimeEditEx, ToOffsetHoursTimeEditEx, FilterOptionDropEdit);

				if (value)
				{
					FilterOptionDropEdit.CaptionResourceString = HourOffsetCaption;
					FilterOptionDropEdit.RefreshCaptionLabel();
				}
			}
		}

		bool ShowDayOffsetControls
		{
			get { return showDayOffsetControls; }
			set
			{
				showDayOffsetControls = value;
				var controlsToChange = new List<Control> { FromOffsetDaysCalcEdit, ToOffsetDaysCalcEdit };

				if (!ShowHourOffsetControls)
				{
					controlsToChange.Add(FilterOptionDropEdit);
				}

				SuspendLayoutAndSetVisibility(value, controlsToChange.ToArray());

				if (value)
				{
					FilterOptionDropEdit.CaptionResourceString = DayOffsetCaption;
					FilterOptionDropEdit.RefreshCaptionLabel();
				}
			}
		}

		protected ResourceStringData HourOffsetCaption => Res.GetData("09479a90-2a1b-4288-a773-87cdd95e838d", "In the");
		protected ResourceStringData DayOffsetCaption => Res.GetData("251c800b-6baf-4f96-9051-f579535fcdba", "Days in the");

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
			PropertySearchDropEdit.ShowDescriptionBox = !ShowDateControls && !ShowHourOffsetControls && !ShowDayOffsetControls;

			if (ShowDateControls || ShowHourOffsetControls || ShowDayOffsetControls)
			{
				PropertySearchDropEdit.ForeColor = SystemColors.GrayText;
				PropertySearchDropEdit.PreBoundMaxLength = ZFilterStrip.FilterComparisonOperatorBoxPreBoundMaxLength;

				if (Filter != null && Filter.IsPropertySearchUsingSpecifiedDateTimeRange)
				{
					FromDateEdit.DateTimeFormat = ZDateTimePickerFormat.Long;
					ToDateEdit.DateTimeFormat = ZDateTimePickerFormat.Long;
				}
				else
				{
					FromDateEdit.DateTimeFormat = ZDateTimePickerFormat.Short;
					ToDateEdit.DateTimeFormat = ZDateTimePickerFormat.Short;
				}

				if (ParentStrip != null)
				{
					ControlDpiScalingHelper.SetLeft(ref PropertySearchDropEdit, ParentStrip.FilterComparisonOperatorBoxStart, false);
					ControlDpiScalingHelper.SetLeft(ref ToDateEdit, ParentStrip.FilterControlsBox2Start(ToDateEdit), true);
				}
				else
				{
					ControlDpiScalingHelper.SetLeft(ref PropertySearchDropEdit, 0, true);
					ControlDpiScalingHelper.SetLeft(ref FromDateEdit, ControlDpiScalingHelper.ScaleToCurrentDpiX(labelWidth) + PropertySearchDropEdit.Width, false);
					ControlDpiScalingHelper.SetLeft(ref ToDateEdit, ControlDpiScalingHelper.ScaleToCurrentDpiX(labelWidth * 2) + PropertySearchDropEdit.Width + FromDateEdit.Width, false);
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

		bool showDateControls;
		bool showHourOffsetControls;
		bool showDayOffsetControls;
		protected ZFilterStrip ParentStrip;

		protected ModuleDateFilter Filter
		{
			get { return (ModuleDateFilter)CurrentDataItem; }
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

		const int labelWidth = 50;

		#endregion
	}
}
