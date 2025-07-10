namespace Enterprise.DocumentEngine.GUI.Visualisation
{
	using System;
	using System.Drawing;
	using System.Windows.Forms;
	using CargoWise.Windows.UI;
	using Enterprise.DocumentEngine.Visualisation;
	using Enterprise.ZArchitecture.GUI;

	class VisualiserTabPage : ZTabPage, IVisualizedReportView
	{
		internal VisualiserTabPage()
			: base()
		{
			AutoScroll = true;
			BackColor = Color.White;
		}

		public event EventHandler FirstShown;

		public bool ShouldBeReadOnly
		{
			get
			{
				return ShouldBeReadOnlyInViewMode;
			}
			set
			{
				ShouldBeReadOnlyInViewMode = value;
			}
		}

		bool isVisibleAtLeastOnce;
		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			if (!isVisibleAtLeastOnce && TabVisible)
			{
				isVisibleAtLeastOnce = true;

				UpdateBounds();
				AddScrollHandlers();

				OnFirstShown(this, EventArgs.Empty);

				if (ShouldBeReadOnly)
				{
					this.SetReadOnlyIncludingChildren();
				}
			}
		}

		protected virtual void OnFirstShown(object sender, EventArgs e)
		{
			if (FirstShown != null)
			{
				FirstShown(sender, e);
			}
		}

		IVisualiserDrawer controlDrawer;
		public IVisualiserDrawer ControlDrawer
		{
			get { return controlDrawer ?? (controlDrawer = new VisualiserComponentsToVisualControlsConverter(this)); }
		}

		IVisualiserDrawer borderDrawer;
		public IVisualiserDrawer BorderDrawer
		{
			get { return borderDrawer ?? (borderDrawer = new VisualiserComponentBorderToGraphicsConverter(this)); }
		}

		#region Scroll

		void AddScrollHandlers()
		{
			this.MouseClick += new MouseEventHandler(VisualiserTabPage_MouseClick);

			foreach (Control control in Controls)
			{
				if (!control.CanSelect)
				{
					control.MouseClick += new MouseEventHandler(UnselectableControl_MouseClick);
				}
				else
				{
					control.KeyPress += new KeyPressEventHandler(SelectableControl_KeyPress);

					if (!(control is DataGridView))
					{
						control.MouseWheel += new MouseEventHandler(Control_MouseWheel);
					}
				}
			}
		}

		void RemoveScrollHandlers()
		{
			this.MouseClick -= new MouseEventHandler(VisualiserTabPage_MouseClick);

			foreach (Control control in Controls)
			{
				control.MouseClick -= new MouseEventHandler(UnselectableControl_MouseClick);
				control.KeyPress -= new KeyPressEventHandler(SelectableControl_KeyPress);
				control.MouseWheel -= new MouseEventHandler(Control_MouseWheel);
			}
		}

		#region Event Handlers

		void VisualiserTabPage_MouseClick(object sender, MouseEventArgs e)
		{
			this.Focus();
		}

		void UnselectableControl_MouseClick(object sender, MouseEventArgs e)
		{
			this.Focus();
		}

		void SelectableControl_KeyPress(object sender, KeyPressEventArgs e)
		{
			this.ScrollControlIntoView((Control)sender);
		}

		void Control_MouseWheel(object sender, MouseEventArgs e)
		{
			this.AutoScrollPosition = ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(-this.AutoScrollPosition.X), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(-this.AutoScrollPosition.Y - e.Delta));
		}

		#endregion

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				foreach (Control control in Controls)
				{
					if (!control.IsDisposed)
					{
						var pictureBox = control as PictureBox;
						if (pictureBox != null && pictureBox.Image != null)
						{
							pictureBox.Image.Dispose();
						}
					}
				}
			}

			RemoveScrollHandlers();

			base.Dispose(disposing);
		}
	}
}
