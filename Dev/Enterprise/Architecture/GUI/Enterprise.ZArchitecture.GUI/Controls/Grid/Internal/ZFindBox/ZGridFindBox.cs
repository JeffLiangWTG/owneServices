using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.Core.Forms;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	[SuppressFormDesignerAnalysis]
	[ToolboxItem(false)]
	public class ZGridFindBox : ZPopupFindBox.Bare, IGridControl
	{
		public ZGridFindBox()
		{
			TabStop = false;
			PopupButton.Dock = DockStyle.None;
			#if WINZOR
			PopupButton.Text = "...";
			#else
			PopupButton.Text = "";
			#endif
			PopupButton.BackColor = SystemColors.Control;
			PopupButton.Paint += new PaintEventHandler(PopupButton_Paint);

			CodeBox.BorderStyle = BorderStyle.None;
			CodeBox.Dock = DockStyle.None;
			CodeBox.TabStop = false;
			CodeBox.KeyDown += new KeyEventHandler(CodeBox_KeyDown);
			((IIsOnGrid)CodeBox).IsOnGrid = true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "See Issue 00852474., See Issue 00852474. Imperfectly correlated with low available physical || virtual memory.")]
		void PopupButton_Paint(object sender, PaintEventArgs e)
		{
			#if !WINZOR
			// the ellipsis won't render because the button is so small, so we draw it ourselves.
			try
			{
				TextRendererHelper.DrawText(e.Graphics, "...", PopupButton.Font, ControlDpiScalingHelper.NewScaledPoint(2, 2), SystemBrushes.ControlText);
			}
			catch (InvalidOperationException ex) { if (ex.Message != "Object is currently in use elsewhere.") { throw; } ZUserControl.ReportGraphicsDisplayFailure(ex); }
			catch (System.Runtime.InteropServices.ExternalException ex) { if (ex.Message != "A generic error occurred in GDI+." && ex.Message != "External component has thrown an exception.") { throw; } ZUserControl.ReportGraphicsDisplayFailure(ex); }
			#endif
		}

		public ZCodeFindBoxColumnStyle ColumnStyle { get; set; }

		#region IGridControl Members

		int IGridControl.SelectionStart
		{
			get { return CodeBox.SelectionStart; }
			set { CodeBox.SelectionStart = value; }
		}

		int IGridControl.SelectionLength
		{
			get { return CodeBox.SelectionLength; }
			set { CodeBox.SelectionLength = value; }
		}

		string IGridControl.Text
		{
			get { return CodeBox.Text; }
			set { CodeBox.Text = value; }
		}

		event EventHandler IGridControl.TextChanged
		{
			add { CodeBox.TextChanged += value; }
			remove { CodeBox.TextChanged -= value; }
		}

		int IGridControl.ButtonWidth
		{
			get { return PopupButton.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(1); }
		}

		event KeyEventHandler IGridControl.KeyDown
		{
			add { CodeBox.KeyDown += value; }
			remove { CodeBox.KeyDown -= value; }
		}

		int IGridControl.MaxLength
		{
			get { return MaxLength; }
			set { MaxLength = value; }
		}

		void IGridControl.ActivateEditControl()
		{
			CodeBox.Focus();
		}

		bool IGridControl.ShouldHandleKey(Keys keyData)
		{
			return false;
		}

		bool IGridControl.ShownForReadOnly
		{
			get { return false; }
		}

		#endregion

		#region Implementation

		protected override void ShowEditOrViewForm()
		{
			if (ColumnStyle != null && ColumnStyle.HasCustomEditForm)
			{
				ColumnStyle.ShowEditOrViewForm(this, ReadOnly);
			}
			else
			{
				base.ShowEditOrViewForm();
			}
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			if (PopupButton != null && CodeBox != null)
			{
				SuspendLayout();

				try
				{
					base.OnSizeChanged(e);
					SizeChildControls();
				}
				finally
				{
					ResumeLayout();
				}
			}
			else
			{
				base.OnSizeChanged(e);
			}
		}

		protected virtual void SizeChildControls()
		{
			ControlDpiScalingHelper.SetTop(ref PopupButton, 0, true);
			ControlDpiScalingHelper.SetHeight(ref PopupButton, Height + ControlDpiScalingHelper.OnePixel, false); // This was intentionally left on '1' (not scaled), since the control is so tightly placed rounding causes it to overflow the parent
			ControlDpiScalingHelper.SetWidth(ref PopupButton, 18, true);

			ControlDpiScalingHelper.SetLeft(ref CodeBox, 1, true);
			ControlDpiScalingHelper.SetTop(ref CodeBox, 1, true);
			ControlDpiScalingHelper.SetWidth(ref CodeBox, Width - PopupButton.Width - CodeBox.Left, false);
			ControlDpiScalingHelper.SetLeft(ref PopupButton, CodeBox.Right, false);
		}

		protected override bool ProcessKeyPreview(ref Message m)
		{
			return IsTabbingThroughGridColumn(m) || base.ProcessKeyPreview(ref m);
		}

		protected bool IsTabbingThroughGridColumn(Message m)
		{
			return (Keys)(int)m.WParam == Keys.Tab;
		}

		void CodeBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.F2)
			{
				CodeBox.SelectionLength = 0;
				CodeBox.SelectionStart = CodeBox.Text.Length;
			}
		}

		#endregion
	}
}
