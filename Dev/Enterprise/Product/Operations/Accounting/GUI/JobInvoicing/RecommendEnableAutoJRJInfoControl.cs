using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class RecommendEnableAutoJRJInfoControl : ZUserControl
	{
		public RecommendEnableAutoJRJInfoControl()
		{
			InitializeComponent();
		}

		public static DialogDefaultContext DialogDefaultContext
		{
			get
			{
				var result = new DialogDefaultContext(new ZGuid("9ECA40E5-03A4-4787-A11B-AAECCDBCE199"),
												ResString.GetMultilingualString("E96021CF-B72C-465B-BC22-0C70D5458579", "Auto Job Revenue Journal setting"),
												null,
												ZMessageBoxIcon.Information,
												null,
												showCheckboxOnly: true,
												checkBoxCaption: Res.GetData("FF02A220-0F19-4FC0-B1AA-15240C2DB4E8", "Do not show this message again"));
				return result;
			}
		}

		const string UpdateNoteURL = "http://www.cargowise.com/Documents/UpdateNotes/CargoWiseOneUpdateNote20190923.pdf"; // this is an update note URL
		void LearnMoreButton_Click(object sender, System.EventArgs e)
		{
			WebUrlLauncher.Launch(UpdateNoteURL);
		}
	}
}
