using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.GUI
{
	public partial class ChooseCDSoftwareForm : ZChildForm
	{
		public ChooseCDSoftwareForm(ChooseCDSoftwareManager manager)
			: base(manager)
		{
			InitializeComponent();
			CDSoftwareManager.DisableOptionsNotAvailable();

			FourthLabel.Text = Res.GetString("ChooseCDSoftwareForm|FourthLabel", "Once complete, the CD will contain an Excel index file of all the documents it contains. When a client inserts the CD into their CD-ROM drive, it will automatically open the Excel index file and allow them to sort and view the documents on the CD.");
			ThirdLabel.Text = Res.GetString("ChooseCDSoftwareForm|ThirdLabel", "Start up your CD Burning software (e.g. NERO) and use the contents of the folder to create a new CD. You can usually drag-and-drop the contents of the folder into your CD burning software, or specify the path of the folder to burn to CD. If you are using Windows XP or Windows Server 2003, you can use the inbuilt Windows CD burning software supplied on your computer.");
			SecondLabel.Text = Res.GetString("ChooseCDSoftwareForm|SecondLabel", "After copying is complete, a Windows Explorer window will pop up displaying the contents of the folder. This folder will contain all the documents to copy to the CD.");
			FirstLabel.Text = Res.GetString("ChooseCDSoftwareForm|FirstLabel", "This wizard will now copy the selected eDocs to a folder on your computer. This may take a minute or two.");
		}

		public override string FormVerb
		{
			get { return ZString.Empty; }
		}

		protected ChooseCDSoftwareManager CDSoftwareManager
		{
			get { return (ChooseCDSoftwareManager)BusinessEntity; }
		}

		#region Buttons

#if DEBUG
		internal
#endif
		void OKButton_Click(object sender, System.EventArgs e)
		{
			if (CDSoftwareManager == null)
			{
				return;
			}

			if (CDSoftwareManager.SelectedSoftware.IsUserControlled)
			{
				CDSoftwareManager.SelectedSoftware.Burn();
			}
			CDSoftwareManager.DisableSoftwareNotUsed();
			Close();
		}

		void CancelCDButton_Click(object sender, System.EventArgs e)
		{
			if (Globals.Message.Show(Res.GetString("0d51b500-0be7-4a79-8f54-ece03f8f835b", "Are you sure you want to cancel burning this CD?"), Res.GetString("8012b327-bed4-42bb-9ddb-a2eb3ca167dd", "Cancel Burning"), MessageBoxButtons.YesNo, MessageBoxIcon.None) == DialogResult.Yes)
			{
				CDSoftwareManager.SelectedSoftware.CancelBurn();
				Close();
			}
		}

		#endregion
	}
}
