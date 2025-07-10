using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.PlugIn;

public partial class EntryManualReleaseForm : ZChildForm
{
	public EntryManualReleaseForm(EntryManualReleaseHandler manualRelease)
		: base(manualRelease)
	{
		InitializeComponent();
	}

	public new EntryManualReleaseHandler BusinessEntity => (EntryManualReleaseHandler)base.BusinessEntity;

	public override string FormHeading => Res.GetString("B2994294-FB7E-46B0-A8FD-066521F90E6B", "Manual Release");

	void OkButton_Click(object sender, System.EventArgs e)
	{
		PerformValidation();
		if (BusinessEntity.HasErrors())
		{
			ShowErrorsDialog();
		}
		else
		{
			DialogResult = DialogResult.OK;
			Close();
		}
	}

	void AbortButton_Click(object sender, System.EventArgs e)
	{
		DialogResult = DialogResult.Cancel;
		Close();
	}
}
