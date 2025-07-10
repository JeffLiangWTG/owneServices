using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public sealed partial class SIMADumpingNumberForm : ZChildForm
	{
		public SIMADumpingNumberForm()
		{
			InitializeComponent();
		}

		public SIMADumpingNumberForm(SIMADumpingNumberCollection dumpingNumbers, ZString tariffNumber)
			: base(dumpingNumbers)
		{
			InitializeComponent();
			this.Text = Res.GetString("A99268FE-3BDA-4503-858B-BEB0B3B88FFA", "SIMA Measure Selection - Tariff:{0}", tariffNumber);
		}

		internal SIMADumpingNumber SelectedDumpingNumber
		{
			get { return fSelectedDumpingNumber; }
		}
		SIMADumpingNumber fSelectedDumpingNumber;

		void SelectButton_Click(object sender, System.EventArgs e)
		{
			SIMADumpingNumber selectedNumber = null;
			var listManager = SIMAMeasureItemsGrid.ListManager;
			if (listManager != null)
			{
				selectedNumber = (SIMADumpingNumber)listManager.GetCurrent();
			}

			if (selectedNumber != null)
			{
				fSelectedDumpingNumber = selectedNumber;
				DialogResult = DialogResult.OK;
				Close();
			}
			else
			{
				Globals.Message.ShowInformation(Res.GetString("3F004E9D-CA60-441A-9DEB-64AFF2FEC8ED", "Please selected (highlight) a SIMA measure."), Res.GetString("1B05DC15-EEE2-427E-9875-F7B945995A86", "SIMA measure"));
			}
		}
	}
}
