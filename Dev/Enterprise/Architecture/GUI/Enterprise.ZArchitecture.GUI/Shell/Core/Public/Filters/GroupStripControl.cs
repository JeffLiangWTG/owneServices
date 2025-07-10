using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	[ToolboxItem(false)]
	public partial class GroupStripControl : ZUserControl, IStripControl
	{
		public GroupStripControl()
		{
			InitializeComponent();
			FilterCategoriesToolStrip.Renderer = new CargowiseToolStripRenderer();
			InitializeFilterStrip();
			InitializeFilterCategoryMenuItems();

			#region Drag n Drop

			FilterStripGroupBox.AllowDrop = true;
			FilterStripGroupBox.DragEnter += FilterStripGroupBox_DragEnter;
			FilterStripGroupBox.DragDrop += FilterStripGroupBox_DragDrop;

			#endregion

			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			FilterStripGroupBox.AutoSize = true;
			FilterStripGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			BackColor = System.Drawing.Color.Transparent;

			DeleteStripButton.BackgroundImageLayout = ImageLayout.Stretch;
			DeleteStripButton.FlatStyle = FlatStyle.Flat;
			DeleteStripButton.FlatAppearance.BorderSize = 0;
			DeleteStripButton.BackColor = Color.Transparent;

			DeleteStripButton.BackgroundImage = Icons.GetImage(IconTypes.MinusButtonRest);
			DeleteStripButton.MouseEnter += delegate
			{ DeleteStripButton.BackgroundImage = Icons.GetImage(IsDeleteButtonEnabled ? IconTypes.MinusButtonActive : IconTypes.MinusButtonDisabled); };
			DeleteStripButton.MouseLeave += delegate
			{ DeleteStripButton.BackgroundImage = Icons.GetImage(IsDeleteButtonEnabled ? IconTypes.MinusButtonRest : IconTypes.MinusButtonDisabled); };
			DeleteStripButton.EnabledChanged += OnDeleteStripButtonEnabledChanged;

			AddStripButton.BackgroundImageLayout = ImageLayout.Stretch;
			AddStripButton.FlatStyle = FlatStyle.Flat;
			AddStripButton.FlatAppearance.BorderSize = 0;
			AddStripButton.BackColor = Color.Transparent;
			AddStripButton.BackgroundImage = Icons.GetImage(IconTypes.AddButtonRest);
			AddStripButton.MouseEnter += delegate
			{ AddStripButton.BackgroundImage = Icons.GetImage(IsAddButtonEnabled ? IconTypes.AddButtonActive : IconTypes.AddButtonDisabled); };
			AddStripButton.MouseLeave += delegate
			{ AddStripButton.BackgroundImage = Icons.GetImage(IsAddButtonEnabled ? IconTypes.AddButtonRest : IconTypes.AddButtonDisabled); };
			AddStripButton.EnabledChanged += OnAddStripButtonEnabledChanged;

			BindingSource.DefaultBindingMembersEnabled = false;

			MissingResourceStringChecker.ExcludeFromTest(DeleteStripButton);
			MissingResourceStringChecker.ExcludeFromTest(AddStripButton);
		}

		internal void FilterStripGroupBox_DragDrop(object sender, DragEventArgs e)
		{
			var formats = e.Data.GetFormats();
			if (formats.Length > 0)
			{
				if ((typeof(ZFilterStrip).IsAssignableFrom(e.Data.GetData(formats[0]).GetType()) || e.Data.GetData(typeof(ZFilterStrip)) != null) && e.Effect == DragDropEffects.Move)
				{
					var strip = (ZFilterStrip)e.Data.GetData(formats[0]);
					strip.Location = ParentFilterControl.NextStripWithinGroupLocation(GroupName);
					ParentFilterControl.MoveStripBetweenGroups(strip.CurrentDataItem.GroupName, GroupName, strip);

					string originalGroupName = strip.CurrentDataItem.GroupName;

					strip.Parent = (GroupPanel)sender;
					strip.CurrentDataItem.GroupName = GroupName;
					strip.CurrentDataItem.GroupOrCategory = GroupOrCategory;
					strip.CurrentDataItem.AdditionalGroupColourName = AdditionalGroupColourName;

					ParentFilterControl.RefreshStripLayout();
					ParentFilterControl.RefreshGroupLayout();

					ParentFilterControl.RemoveEmptyGroupStripControl(originalGroupName);

					var filterStripCommonControl = ParentFilterStripCommonControl;
					if (filterStripCommonControl != null)
					{
						filterStripCommonControl.ResetFindButton();
					}
				}
			}
		}

		void FilterStripGroupBox_DragEnter(object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.Move;
		}

		void InitializeFilterStrip()
		{
			CurrentFilterControls = new List<Control>();
		}

		void InitializeFilterCategoryMenuItems()
		{
			var imageListCache = ZFilterControlImages.FilterImageList;

			if (imageListCache != null)
			{
				filterCategoriesToolStripMenuItem.Image = imageListCache.Images["FilterCategories"];
			}

			InitializeFilterCategoryMenuItem(noCategoryToolStripMenuItem, "", Color.Transparent, FilterOrCategory.None);
			InitializeFilterCategoryMenuItem(redToolStripMenuItem, "FilterCategoryRed", Color.FromArgb(242, 177, 178), FilterOrCategory.Red);
			InitializeFilterCategoryMenuItem(greenToolStripMenuItem, "FilterCategoryGreen", Color.FromArgb(196, 232, 190), FilterOrCategory.Green);
			InitializeFilterCategoryMenuItem(blueToolStripMenuItem, "FilterCategoryBlue", Color.FromArgb(183, 201, 238), FilterOrCategory.Blue);
			InitializeFilterCategoryMenuItem(brownToolStripMenuItem, "FilterCategoryBrown", Color.FromArgb(226, 202, 176), FilterOrCategory.Brown);
			InitializeFilterCategoryMenuItem(greyToolStripMenuItem, "FilterCategoryGrey", Color.FromArgb(201, 201, 201), FilterOrCategory.Grey);
			InitializeFilterCategoryMenuItem(additionalCategoryToolStripMenuItem, "FilterCategories", Color.Empty, FilterOrCategory.Others);
		}

		void InitializeFilterCategoryMenuItem(ZFilterStripToolStripMenuItem menuItem, string imageKey, Color color, FilterOrCategory groupOrCategory)
		{
			var imageListCache = ZFilterControlImages.FilterImageList;

			if (imageListCache != null)
			{
				menuItem.Image = imageListCache.Images[imageKey];
			}

			menuItem.Color = color;
			menuItem.OrCategory = groupOrCategory;
			menuItem.Click += new EventHandler(toolStripMenuItem_Click);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (GroupName.IsEmpty)
			{
				ControlDpiScalingHelper.SetTop(ref FilterStripGroupBox, 0, true);
				GroupNameLabel.Visible = false;
				ControlDpiScalingHelper.SetTop(ref DeleteStripButton, 18, true);
				ControlDpiScalingHelper.SetTop(ref AddStripButton, 18, true);
				ControlDpiScalingHelper.SetTop(ref FilterCategoriesToolStrip, 18, true);
			}
		}

		#region Clicking a Filter Category MenuItem

		void toolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (((ZFilterStripToolStripMenuItem)sender).OrCategory > FilterOrCategory.Grey)
			{
				ChangeAdditionalFilterOrCategory(((ZFilterStripToolStripMenuItem)sender).OrCategory);
			}
			else
			{
				ChangeFilterOrCategory(((ZFilterStripToolStripMenuItem)sender).OrCategory);
			}

			OnFilterEdited();
		}

		void ChangeAdditionalFilterOrCategory(FilterOrCategory filterOrCategory)
		{
			var additionalOrCategory = GetAdditionalColors();
			ChangeFilterOrCategory(additionalOrCategory);
		}

		FilterOrCategory GetAdditionalColors()
		{
			var existingOrCategory = this.GroupOrCategory;

			var cp = new ColourProperties();

			using (var colorDialog = new ColorDialog())
			{
				colorDialog.CustomColors = ParentFilterControl.ExistingAdditionalColorForGroupStripControl;

				if (colorDialog.ShowDialog() == DialogResult.OK)
				{
					cp.ToKnownColour(colorDialog.Color);
					AdditionalGroupColourName = cp.ColourName;
					return (FilterOrCategory)Enum.Parse(typeof(FilterOrCategory), cp.ColourName);
				}
			}

			return existingOrCategory;
		}

		void ChangeFilterOrCategory(FilterOrCategory orCategory)
		{
			FilterCategoriesToolStripDropDown.SelectCategory(orCategory);

			// colours
			var color = (int)orCategory > (int)FilterOrCategory.Grey ? Color.FromName(Enum.GetName(typeof(FilterOrCategory), orCategory)) : FilterCategoriesToolStripDropDown.CurrentCategoryColor;
			GroupColorLabel.BackColor = color;
			FilterCategoriesToolStrip.BackColor = color;

			ChangeControlColorToMatchCategoryColor(CurrentFilterControls, color);

			GroupOrCategory = orCategory;

			if (ParentFilterControl != null)
			{
				var strips = ParentFilterControl.GetStripsForGroup(GroupName);
				strips.ForEach(s =>
				{
					if (s.CurrentDataItem != null)
					{
						s.CurrentDataItem.GroupName = GroupName;
						s.CurrentDataItem.GroupOrCategory = GroupOrCategory;
						s.CurrentDataItem.AdditionalGroupColourName = AdditionalGroupColourName;
					}
				});
			}
		}

		void ChangeControlColorToMatchCategoryColor(List<Control> controls, Color color)
		{
			foreach (var control in controls)
			{
				if (control is CheckBox || control is RadioButton)
				{
					control.BackColor = color;
				}
				ChangeControlColorToMatchCategoryColor(control.Controls, color);
			}
		}

		void ChangeControlColorToMatchCategoryColor(Control.ControlCollection controls, Color color)
		{
			if (controls != null)
			{
				foreach (Control control in controls)
				{
					if (control is CheckBox || control is RadioButton)
					{
						control.BackColor = color;
					}
					ChangeControlColorToMatchCategoryColor(control.Controls, color);
				}
			}
		}

		void BizO_OrCategoryChanged(object sender, EventArgs e)
		{
			UpdateFilterControlColorsForOrCategory();
			OnFilterOrCategoryChanged();
		}

		#endregion

		List<Control> CurrentFilterControls;

		internal void UpdateFilterControlColorsForOrCategory()
		{
			ChangeFilterOrCategory(GroupOrCategory);
		}

		public event EventHandler FilterOrCategoryChanged;

		protected int PreferredHeight;

		public ZString GroupName
		{
			get;
			set;
		}

		public ZString AdditionalGroupColourName
		{
			get;
			set;
		}

		public FilterOrCategory GroupOrCategory
		{
			get { return groupOrCategory; }
			set
			{
				groupOrCategory = value;
			}
		}

		FilterOrCategory groupOrCategory;

		public event EventHandler FilterEdited;

		void OnFilterEdited()
		{
			if (FilterEdited != null)
			{
				FilterEdited(this, EventArgs.Empty);
			}
		}

		void OnFilterOrCategoryChanged()
		{
			if (FilterOrCategoryChanged != null)
			{
				FilterOrCategoryChanged(this, EventArgs.Empty);
			}
		}

		void DeleteStripButton_Click(object sender, EventArgs e)
		{
			var strips = ParentFilterControl.GetStripsForGroup(GroupName);
			if (strips.Any(s => !s.DeleteStripButton.Visible))
			{
				strips.Where(s => s.DeleteStripButton.Visible).ToList().ForEach(ParentFilterControl.DeleteFilterStrip);
			}
			else
			{
				ParentFilterControl.DeleteGroupFilterStrip(this);
			}

			OnFilterEdited();
		}

		void AddStripButton_Click(object sender, EventArgs e)
		{
			var newFilterStrip = ParentFilterControl.NewZFilterStrip();
			ParentFilterControl.AddFilterStrip(newFilterStrip, ParentFilterControl.FilterBusinessObject.FilterStrips.AddNew(), this);
			newFilterStrip.Focus();
		}

		StripControl ParentFilterControl
		{
			get
			{
				var parent = base.Parent;

				while (parent != null)
				{
					var parentFilterControl = parent as StripControl;
					if (parentFilterControl != null)
					{
						return parentFilterControl;
					}
					parent = parent.Parent;
				}

				return null;
			}
		}

		ZFilterStripCommonControl ParentFilterStripCommonControl
		{
			get
			{
				var parent = base.Parent;

				while (parent != null)
				{
					var parentFilterControl = parent as ZFilterStripCommonControl;
					if (parentFilterControl != null)
					{
						return parentFilterControl;
					}
					parent = parent.Parent;
				}

				return null;
			}
		}

		internal const int MaxFilterStripWidth = 790;
		internal const int MinFilterStripWidth = 534;
		internal const int DeleteButtonDefaultLeft = 740;
		internal const int AddButtonDefaultLeft = 700;
		internal const int FilterButtonDefaultLeft = 768;

		internal void UpdateLayout(int width, bool shouldAlwaysLayoutControls = false)
		{
			SuspendLayout();
			try
			{
				if (width < MinFilterStripWidth)
				{
					width = MinFilterStripWidth;
				}
				if (width > MaxFilterStripWidth)
				{
					width = MaxFilterStripWidth;
				}
				if (shouldAlwaysLayoutControls || (width != Width && CurrentDataItem != null))
				{
					SetWidths(width);
				}
			}
			finally
			{
				ResumeLayout();
			}
		}

		protected virtual void SetWidths(int targetWidth)
		{
			var shift = MaxFilterStripWidth - targetWidth;
			ControlDpiScalingHelper.SetLeft(ref DeleteStripButton, DeleteButtonDefaultLeft - shift, true);
			ControlDpiScalingHelper.SetLeft(ref FilterCategoriesToolStrip, FilterButtonDefaultLeft - shift, true);
			ControlDpiScalingHelper.SetLeft(ref AddStripButton, AddButtonDefaultLeft - shift, true);
			ControlDpiScalingHelper.SetWidth(this, targetWidth, true);
		}

		#region IsDeleteButtonEnabled

		internal bool IsDeleteButtonEnabled
		{
			get { return DeleteStripButton.Enabled; }
			set
			{
				DeleteStripButton.Enabled = value;
			}
		}

		void OnDeleteStripButtonEnabledChanged(object sender, EventArgs e)
		{
			DeleteStripButton.BackgroundImage = Icons.GetImage(DeleteStripButton.Enabled ? IconTypes.MinusButtonRest : IconTypes.MinusButtonDisabled);
		}

		internal bool IsAddButtonEnabled
		{
			get { return AddStripButton.Enabled; }
			set
			{
				AddStripButton.Enabled = value;
			}
		}

		void OnAddStripButtonEnabledChanged(object sender, EventArgs e)
		{
			AddStripButton.BackgroundImage = Icons.GetImage(AddStripButton.Enabled ? IconTypes.AddButtonRest : IconTypes.AddButtonDisabled);
		}

		internal void SetButtonEnabledState(bool enabled)
		{
			IsDeleteButtonEnabled = enabled;
			IsAddButtonEnabled = enabled;
			FilterCategoriesToolStrip.Enabled = enabled;
		}

		#endregion

		#region IsFilterCategoriesToolStripDropDownVisible

		public bool IsFilterCategoriesToolStripDropDownVisible
		{
			get { return FilterCategoriesToolStripDropDown.Visible; }
			set
			{
				FilterCategoriesToolStripDropDown.Visible = value;
			}
		}

		#endregion

		#region IStripControl Members

		void IStripControl.UpdateLayout(int width)
		{
			UpdateLayout(width);
		}

		public FilterOrCategory OrCategory
		{
			get { return GroupOrCategory; }
		}

		#endregion
	}

	[ToolboxItem(false)]
	internal partial class GroupPanel : ZPanel
	{
#if !WINZOR
		const int BORDER_SIZE = 1;
#endif

		readonly IContainer components;

#if !WINZOR

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			ControlPaint.DrawBorder(e.Graphics, ClientRectangle,
				Color.Silver, BORDER_SIZE, ButtonBorderStyle.Solid,
				Color.Silver, BORDER_SIZE, ButtonBorderStyle.Solid,
				Color.Silver, BORDER_SIZE, ButtonBorderStyle.Solid,
				Color.Silver, BORDER_SIZE, ButtonBorderStyle.Solid);
		}

#endif

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
