using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public partial class LoadDataForUnloadingSelectorForm : ZChildForm
	{
		public LoadDataForUnloadingSelectorForm()
		{
			InitializeComponent();
			MessageLabel.Text = messageLabelText;
			ExtraMessageLabel.Text = extraMessageLabelText;
		}

		public (ZBool isAnswerOK, ZBool departureSelected) GetResultFromSelector()
		{
			var answer = ZFormModaliser.ShowDialogWithoutDispose(this);
			return (answer == DialogResult.OK, DepartureRadioButton.Checked);
		}

		public override string FormVerb => string.Empty;

		readonly string messageLabelText = ResString.GetMultilingualString("3D669A4A-EDE9-4F26-B2C3-B4F21512FE07", "A Departure for that MRN already exists in the system.\nPlease, select the source of the data to be loaded:");

		readonly string extraMessageLabelText = ResString.GetMultilingualString("E2396CFB-3DC0-45AB-A4B8-51F0E2CCBC4A", "Please note existing data in Unloading Remarks could be overwritten.");
	}
}
