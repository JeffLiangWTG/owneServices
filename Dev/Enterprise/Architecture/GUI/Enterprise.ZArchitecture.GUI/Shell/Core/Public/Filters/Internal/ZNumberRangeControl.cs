using System;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	[DefaultDataSourceBindingMember(null)]
	public partial class ZNumberRangeControl : ZUserControl
	{
		#region Construction

		public ZNumberRangeControl()
			: this(null)
		{
		}

		public ZNumberRangeControl(ZFilterStrip parentStrip)
		{
			ParentStrip = parentStrip;
			InitializeComponent();
			InitializeControls();
			UpButton.Click += UpButton_Click;
			DownButton.Click += DownButton_Click;
		}

		public void SetShowEmptyStringForEmptyValue(bool showEmpty)
		{
			ToCalcEdit.ShowEmptyStringForEmptyValue = showEmpty;
			FromCalcEdit.ShowEmptyStringForEmptyValue = showEmpty;
		}

		void DownButton_Click(object sender, EventArgs e)
		{
			Filter.ClickDownArrow();
		}

		void UpButton_Click(object sender, EventArgs e)
		{
			Filter.ClickUpArrow();
		}

		int FirstCalcLeft { get; set; }
		int SecondCalcLeft { get; set; }

		void InitializeControls()
		{
			ControlDpiScalingHelper.SetLeft(this, 0, true);

			PropertySearchDropEdit.BindTo = "PropertySearch";
			PropertySearchDropEdit.BindToList = "PropertySearch_List";
			PropertySearchDropEdit.TabIndex = 1;
			PropertySearchDropEdit.ShowDescriptionBox = false;

			FromCalcEdit.BindTo = "Property1";
			FromCalcEdit.TabIndex = 2;
			AddCalcEditVisibleChangedEventHandler(FromCalcEdit);

			ToCalcEdit.BindTo = "Property2";
			ToCalcEdit.TabIndex = 3;
			AddCalcEditVisibleChangedEventHandler(ToCalcEdit);

			FromCalcEdit.AllowOverlap(ToCalcEdit);

			var top = ParentStrip?.FilterControlTop ?? 0;
			var topIsOnStandardDpi = (ParentStrip != null);
			ControlDpiScalingHelper.SetTop(ref PropertySearchDropEdit, top, topIsOnStandardDpi);
			ControlDpiScalingHelper.SetTop(ref FromCalcEdit, top, topIsOnStandardDpi);
			ControlDpiScalingHelper.SetTop(ref ToCalcEdit, top, topIsOnStandardDpi);
			ControlDpiScalingHelper.SetTop(ref UpButton, top, topIsOnStandardDpi);
			ControlDpiScalingHelper.SetTop(ref DownButton, top, topIsOnStandardDpi);

			if (ParentStrip != null)
			{
				ControlDpiScalingHelper.SetLeft(ref PropertySearchDropEdit, ParentStrip.FilterComparisonOperatorBoxStart, false);
				ControlDpiScalingHelper.SetLeft(ref FromCalcEdit, PropertySearchDropEdit.Right, false);
				ControlDpiScalingHelper.SetLeft(ref ToCalcEdit, ParentStrip.FilterControlsBox2Start(ToCalcEdit), true);
				ControlDpiScalingHelper.SetLeft(ref UpButton, ParentStrip.FilterControlsBox2Start(ToCalcEdit) - 20, true);
				ControlDpiScalingHelper.SetLeft(ref DownButton, ParentStrip.FilterControlsBox2Start(ToCalcEdit) + 20, true);
			}
			else
			{
				const int distance = 50;
				ControlDpiScalingHelper.SetLeft(ref PropertySearchDropEdit, 230 + 0, true);
				ControlDpiScalingHelper.SetLeft(ref FromCalcEdit, PropertySearchDropEdit.Right, false);
				ControlDpiScalingHelper.SetLeft(ref ToCalcEdit, FromCalcEdit.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(distance), false);
				ControlDpiScalingHelper.SetLeft(ref UpButton, FromCalcEdit.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(distance - 20), false);
				ControlDpiScalingHelper.SetLeft(ref DownButton, FromCalcEdit.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(distance + 20), false);
			}

			var andLabelTop = FromCalcEdit.Top + (FromCalcEdit.Height - AndLabel.Height) / 2;
			var andLabelLeft = FromCalcEdit.Right + (ToCalcEdit.Left - FromCalcEdit.Right - AndLabel.Width) / 2;
			ControlDpiScalingHelper.SetTop(ref AndLabel, andLabelTop, false);
			ControlDpiScalingHelper.SetLeft(ref AndLabel, andLabelLeft, false);

			FirstCalcLeft = FromCalcEdit.Left;
			SecondCalcLeft = ToCalcEdit.Left;
		}

		#endregion

		#region Binding

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			if (Filter != null)
			{
				Filter.PropertySearchInfo.ValueChanged -= new EventHandler(PropertySearchInfo_ValueChanged);
				Filter.Property1Changed -= Filter_Property1Changed;
				Filter.MinValueChanged -= Filter_MinValueChanged;
				Filter.MaxValueChanged -= Filter_MaxValueChanged;
				Filter.PropertySearchChanged -= Filter_PropertySearchChanged;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (Filter != null)
			{
				Filter.PropertySearchInfo.ValueChanged += new EventHandler(PropertySearchInfo_ValueChanged);
				UpdateControls();
				Filter.Property1Changed += Filter_Property1Changed;
				Filter.MinValueChanged += Filter_MinValueChanged;
				Filter.MaxValueChanged += Filter_MaxValueChanged;
				Filter.PropertySearchChanged += Filter_PropertySearchChanged;
				CheckArrowButtonsAvailability();
			}
		}

		void PropertySearchInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateControls();
		}

		#endregion

		#region Showing / Hiding the calc controls

		void Filter_PropertySearchChanged(object sender, EventArgs e)
		{
			CheckArrowButtonsAvailability();
		}

		void Filter_MinValueChanged(object sender, EventArgs e)
		{
			CheckArrowButtonsAvailability();
		}

		void Filter_MaxValueChanged(object sender, EventArgs e)
		{
			CheckArrowButtonsAvailability();
		}

		void Filter_Property1Changed(object sender, EventArgs e)
		{
			CheckArrowButtonsAvailability();
		}

		void CheckArrowButtonsAvailability()
		{
			if (Filter != null && Filter.ShowUpAndDownArrows)
			{
				if (Filter.Property1 > Filter.MaxValue ||
					Filter.Property1 < Filter.MinValue ||
					(Filter.Property1 == Filter.MinValue && Filter.Property1 == Filter.MaxValue))
				{
					UpButton.Enabled = false;
					DownButton.Enabled = false;
				}
				else if (Filter.Property1 == Filter.MaxValue)
				{
					UpButton.Enabled = false;
					DownButton.Enabled = true;
				}
				else if (Filter.Property1 == Filter.MinValue)
				{
					UpButton.Enabled = true;
					DownButton.Enabled = false;
				}
				else
				{
					UpButton.Enabled = true;
					DownButton.Enabled = true;
				}
			}
		}

		void UpdateControls()
		{
			SuspendLayout();

			try
			{
				var propertyType = ZCalcEditPropertyTypeConverter.Convert(Filter.PropertyType);

				FromCalcEdit.Core.SetBindToType(propertyType);
				FromCalcEdit.Decimals = Filter.Decimals;
				ToCalcEdit.Core.SetBindToType(propertyType);
				ToCalcEdit.Decimals = Filter.Decimals;

				ControlDpiScalingHelper.SetLeft(ref ToCalcEdit, (Filter.IsLessThanOrEqualToSearch ? FirstCalcLeft : SecondCalcLeft), false);

				FromCalcEdit.Visible = Filter.IsBetweenSearch || Filter.IsGreaterThanOrEqualToSearch || Filter.IsEqualToSearch || Filter.IsGreaterThanSearch;
				ToCalcEdit.Visible = Filter.IsBetweenSearch || Filter.IsLessThanOrEqualToSearch;
				AndLabel.Visible = Filter.IsBetweenSearch;
				UpButton.Visible = Filter.IsEqualToSearch && Filter.ShowUpAndDownArrows;
				DownButton.Visible = Filter.IsEqualToSearch && Filter.ShowUpAndDownArrows;
			}
			finally
			{
				ResumeLayout(true);
			}
		}

		void AddCalcEditVisibleChangedEventHandler(ZCalcEdit calcEdit)
		{
			calcEdit.VisibleChanged += new EventHandler(this.CalcEditVisibleChanged);
		}

		void CalcEditVisibleChanged(object sender, EventArgs e)
		{
			if (FromCalcEdit.Visible && ToCalcEdit.Visible)
			{
				FromCalcEdit.Focus();
			}
			else
			{
				var calcEdit = sender as ZCalcEdit;
				if (calcEdit != null && calcEdit.Visible)
				{
					calcEdit.Focus();
				}
			}
		}

		ZFilterStrip ParentStrip { get; set; }

		ModuleNumberRangeFilter Filter
		{
			get { return (ModuleNumberRangeFilter)CurrentDataItem; }
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

		#endregion
	}
}
