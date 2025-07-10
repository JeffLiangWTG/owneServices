using System;
using System.Windows.Forms;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.Visualisation
{
	partial class VisualiserForm : ZChildForm, IVisualizerView
	{
		public VisualiserForm()
		{
			InitializeComponent();

#if DEBUG
			// These buttons click events are hooked outside of this form, mark them as hooked to prevent being reported by basher test.
			discardButton.ClickHasBeenHooked = true;
			resetButton.ClickHasBeenHooked = true;
			saveButton.ClickHasBeenHooked = true;
#endif
		}

		public event EventHandler DiscardButtonClicked
		{
			add { discardButton.Click += value; }
			remove { discardButton.Click -= value; }
		}

		public event EventHandler ResetButtonClicked
		{
			add { resetButton.Click += value; }
			remove { resetButton.Click -= value; }
		}

		public event EventHandler SaveButtonClicked
		{
			add { saveButton.Click += value; }
			remove { saveButton.Click -= value; }
		}

		public event EventHandler ViewClosing
		{
			add
			{
				viewClosing += value;
				FormClosing += VisualiserForm_FormClosing;
			}
			remove
			{
				viewClosing -= value;
				FormClosing -= VisualiserForm_FormClosing;
			}
		}

		void VisualiserForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			viewClosing?.Invoke(sender, EventArgs.Empty);
		}

		event EventHandler viewClosing;

		public IVisualizedReportView GetNewReportView()
		{
			return new VisualiserTabPage();
		}

		public void AddReportView(IVisualizedReportView view)
		{
			visualiserTabControl.TabPages.Add((TabPage)view);
		}

		public bool ShowConfirmation(string message, string caption)
		{
			return Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes;
		}
	}
}
