using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.PlugIn;

public partial class EntryAmendmentForm : ZChildForm
{
	public EntryAmendmentForm(AutoEntryAmendmentHandler manualSet) : base(manualSet)
	{
		InitializeComponent();
	}

	public new AutoEntryAmendmentHandler BusinessEntity => (AutoEntryAmendmentHandler)base.BusinessEntity;

	public override string FormHeading => Res.GetString("6F272AD8-177A-4272-8A6D-A821A7AE25D0", "Entry Amendment");

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);

		this.totalEntryLinesCalcEdit.Visible = this.BusinessEntity?.ShowTotalEntryLines ?? false;
	}

	void OkButton_Click(object sender, EventArgs e)
	{
		PerformValidation();
		if (BusinessEntity.HasErrors())
		{
			ShowErrorsDialog();
		}
		else
		{
			var result = PromptUserHelper.ShowEntryAmendmentConfirmation();
			if (result == ZDialogResult.OK)
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}
	}

	void AbortButton_Click(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Cancel;
		Close();
	}
}
