using System;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting
{
	public partial class CalculationXMLTextForm : KForm, ICaptionRenderingSupport
	{
		public CalculationXMLTextForm(ZString noteText)
			: base()
		{
			InitializeComponent();

			this.Text = AutoRatingRunner.CalculationXML;
			CalculationXMLTextBox.Text = noteText;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			CalculationXMLTextBox.SelectionStart = 0;
			CalculationXMLTextBox.DeselectAll();
		}

		void closeButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void copyToClipboardButton_Click(object sender, EventArgs e)
		{
			SafeClipboard.SetText(CalculationXMLTextBox.Text);
		}

		#region ICaptionRenderingSupport

		bool? ICaptionRenderingSupport.CaptionRenderingEnabled
		{
			get { return true; }
		}

		event EventHandler ICaptionRenderingSupport.CaptionRenderingEnabledChanged
		{
			add { }
			remove { }
		}

		#endregion
	}
}
