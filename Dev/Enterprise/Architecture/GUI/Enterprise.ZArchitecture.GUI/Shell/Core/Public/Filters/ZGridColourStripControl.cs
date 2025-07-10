using System.Drawing;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZGridColourStripControl : StripControl
	{
		public ZGridColourStripControl()
		{
			InitializeComponent();
		}

		public ZGridColourStripControl(GridColourStripBusinessObject filter, StripControl baseFilterControl)
			: base(filter)
		{
			InitializeComponent();

			ToolStrip.LocationChanged += delegate { ToolStripLocationChanged(); };

			ToolStripFindDropButton.Visible = false;
			ToolStripSaveLayoutButton.Visible = false;
			ToolStripManageDropButton.Visible = false;

			gridColourStrip = filter;
			ToolStripColourPicker.Visible = true;
			ToolStripBackColourButton.Visible = true;
			ToolStripForeColourButton.Visible = false;

			SetupToolBarButton(ToolStripBackColourButton, IconTypes.EditButtonRest);
			SetupToolBarButton(ToolStripForeColourButton, IconTypes.ClearButtonRest);

			ToolStripBackColourButton.Click += delegate { ChangeBackColour(); };
			myToolStripTop = ToolStrip.Top;
			ToolStripColourPicker.Visible = true;
			UpdateToolStripLayout(Strips);

			OnFilterStripLoaded += delegate { FilterStripLoaded(); };

			this.baseFilterControl = baseFilterControl;
		}

		readonly StripControl baseFilterControl;
		readonly GridColourStripBusinessObject gridColourStrip;

		#region Color

		public Color BGColour
		{
			get { return backColor; }
			set
			{
				backColor = value;
				ToolStripBackColourButton.BackColor = backColor;
				gridColourStrip.OnLayoutChanged();
			}
		}

		Color backColor = Color.WhiteSmoke;

		void ChangeBackColour()
		{
#if WINZOR
			colorDialog = new ColorDialog();
			colorDialog.FullOpen = false;
#endif
			colorDialog.Color = backColor;
			colorDialog.ShowDialog(this);
			BGColour = colorDialog.Color;
			gridColourStrip.BGColor = backColor;
		}

		#endregion

		#region Key Handler

		protected override bool ProcessDialogKey(Keys keyData)
		{
			var result = false;

			switch (keyData)
			{
				case Keys.Control | Keys.E:
					AddStrip();
					result = true;
					break;

				default:
					result = base.ProcessDialogKey(keyData);
					break;
			}

			return result;
		}

		#endregion

		#region Strip Management

		#region Help

		protected override ZString UpdateNoteURL
		{
			get { return "http://www.cargowise.com/Documents/UpdateNotes/ediEnterpriseupdatenote20061128b.pdf"; }
		}

		#endregion

		protected internal override void AddStrip()
		{
			base.AddStrip();
			gridColourStrip.OnLayoutChanged();
		}

		protected override void ResetLayout()
		{
			base.ResetLayout();
			gridColourStrip.OnLayoutChanged();
		}

		protected override void ClearStrips()
		{
			base.ClearStrips();
			gridColourStrip.OnLayoutChanged();
		}

		protected override void FilterStripEdited()
		{
			base.FilterStripEdited();
			gridColourStrip.OnLayoutChanged();
		}

		protected override void AddFilterStrip(ZFilterStrip strip)
		{
			var fs = new FilterStrip(gridColourStrip.ModuleFilters);
			FilterBusinessObject.FilterStrips.Add(fs);
			AddFilterStrip(strip, fs);
		}

		#endregion

		#region Implementation

		protected internal override ZFilterStrip NewZFilterStrip()
			=> baseFilterControl?.NewZFilterStrip() ?? new ZFilterStrip();

		void FilterStripLoaded()
		{
			SelectUserFilter(FilterBusinessObject.LastUsedLayout);
		}

		void SelectUserFilter(StmModuleFilter filter)
		{
			FilterBusinessObject.LoadLayout(filter);
		}

		void ToolStripLocationChanged()
		{
			ControlDpiScalingHelper.SetHeight(this, Height + ToolStrip.Top - myToolStripTop, false);
			myToolStripTop = ToolStrip.Top;
		}

		int myToolStripTop;

		protected override void SetupToolBarButton(ToolStripItem button, IconTypes icon)
		{
			base.SetupToolBarButton(button, icon);
			button.MouseEnter += (o, e) => button.GetCurrentParent().Focus();
		}

		protected override int MaxFilterStripPanelHeight => ControlDpiScalingHelper.ScaleToCurrentDpiY(9999999);

		#endregion
	}
}
