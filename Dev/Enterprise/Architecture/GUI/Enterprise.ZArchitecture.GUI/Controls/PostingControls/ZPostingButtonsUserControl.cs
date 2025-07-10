using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.Core.Forms
{
	public partial class ZPostingButtonsUserControl : ZUserControl, IPostOrCancel // This is an architecture control
	{
		public ZPostingButtonsUserControl()
		{
			InitializeComponent();
			TabStop = false;
			toolStrip.FocusOnClick = true;
			ForceRepositioning = true;
			toolStrip.Parent = this;

			CloseButton = AddButton(Res.GetString("1ec7a87a-96dd-44b8-8944-199142ef674f", "&Close"),
				Icons.GetImage(IconTypes.BlackWhite_Close));
			SaveAndCloseButton = AddButton(Res.GetString("64d471e0-01b9-418e-8301-2fc7e502e730", "S&ave && Close"),
				Icons.GetImage(IconTypes.BlackWhite_SaveClose));
			SaveButton = AddButton(Res.GetString("0274bace-f9ad-46fb-8e3e-4d13858ecdba", "&Save"),
				Icons.GetImage(IconTypes.NewButtonRest));
		}

		public ZToolStripButton InsertAdditionalButton(string name, string text, Image image, int index)
		{
			var button = new ZToolStripButton
			{
				Name = name,
				Image = image,
				ImageScaling = ToolStripItemImageScaling.SizeToFit,
				Text = text,
				Margin = ControlDpiScalingHelper.NewScaledPadding(5, 0, 5, 0),
				AutoToolTip = false,
				Alignment = ToolStripItemAlignment.Right
			};
			toolStrip.Items.Insert(index, button);

			return button;
		}

		ZToolStripButton AddButton(string text, Image image)
		{
			var button = new ZToolStripButton
			{
				Image = image,
				ImageScaling = ToolStripItemImageScaling.SizeToFit,
				Text = text,
				Margin = ControlDpiScalingHelper.NewScaledPadding(5, 0, 5, 0),
				AutoToolTip = false,
				Alignment = ToolStripItemAlignment.Right
			};
			toolStrip.Items.Add(button);

			return button;
		}

		[DefaultValue(false)]
		public new bool TabStop
		{
			get { return base.TabStop; }
			set { base.TabStop = value; }
		}

		[DefaultValue(true), Browsable(false)]
		public bool ForceRepositioning
		{
			get;
			set;
		}

		[DefaultValue(false), Browsable(false)]
		public bool RequiresHacks
		{
			get;
			set;
		}

		[DefaultValue(true), Browsable(false)]
		public new bool AutoSize
		{
			get { return base.AutoSize; }
			set { base.AutoSize = value; }
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Filthy hacks and magic numbers inside! Beware!")]
		protected override void OnLayout(LayoutEventArgs e)
		{
			base.OnLayout(e);

			if (RequiresHacks)
			{
				var kpanelParent = Parent as KPanel;
				if (kpanelParent != null && kpanelParent.AutoSize &&
					kpanelParent.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(100) >
					this.Width) //JobDeclarationForm's containing panel becomes super-inflated for some reason.
				{
					this.Location = new Point(0, this.Location.Y);
					this.Parent.Width = this.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(8);
				}
			}

			if (ForceRepositioning && toolStrip.Width < this.Width)
			{
				toolStrip.Location =
					new Point(this.Width - toolStrip.Width,
						toolStrip.Location
							.Y); //This is the only fix that works for both English and German and also having less buttons.
			}

#if WINZOR
			// PositionButtonsUserControls normarly positioned relative to the forms' ClientSize, but Winzor has different ClientSize, so it's missaligned on Winzor.
			// Override the position to be on top of the statusbar or on the bottom.
			if (Parent is ZForm && !(Parent is ZChildForm))
			{
				var bottom =
 (Parent is IStatusBarProvider statusBarProvider && statusBarProvider.ShowStatusBar) ? statusBarProvider.MainStatusBar.Top : Parent.Height;
				ControlDpiScalingHelper.SetTop(this, bottom - this.Height, false);
			}
#endif
		}

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

