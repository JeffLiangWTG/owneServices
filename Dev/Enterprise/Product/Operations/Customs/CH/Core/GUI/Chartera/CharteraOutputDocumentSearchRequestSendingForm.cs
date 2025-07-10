using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public partial class CharteraOutputDocumentSearchRequestSendingForm : ZChildForm
{
	public CharteraOutputDocumentSearchRequestSendingForm(CharteraOutputDocumentSearchSendingObject sendingObject) : base(sendingObject)
	{
	}

	public CharteraOutputDocumentSearchRequestSendingForm()
	{
		InitializeComponent();
	}

	protected override void InitialiseForm()
	{
		base.InitialiseForm();
		InitializeComponent();
	}

	void SendButton_Click(object sender, System.EventArgs e)
	{
		if (BusinessEntity != null)
		{
			BusinessEntity.RunPreSaveValidation();
			if (BusinessEntity.Notifications.HasErrors())
			{
				ShowErrorsDialog();
			}
			else
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}
	}

	void CancelButton2_Click(object sender, System.EventArgs e)
	{
		DialogResult = DialogResult.Cancel;
		Close();
	}

	public override string FormHeading => Res.GetString("A5A2D2B7-8AAC-4971-ABAE-0BE622356A1E", "Chartera Output Documents Search Request");
}
