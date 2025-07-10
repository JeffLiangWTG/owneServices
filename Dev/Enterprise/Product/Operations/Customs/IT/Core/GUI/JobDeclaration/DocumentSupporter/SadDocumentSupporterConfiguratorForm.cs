using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public partial class SadDocumentSupporterConfiguratorForm : ZChildForm
{
	public SadDocumentSupporterConfiguratorForm()
		: base()
	{
	}

	public SadDocumentSupporterConfiguratorForm(JobDeclarationSadDocumentSupporter jobDeclarationSadDocumentSupporter, bool bgmReferenceReadOnly = false)
		: base(jobDeclarationSadDocumentSupporter)
	{
		this.bgmReferenceReadOnly = bgmReferenceReadOnly;
	}
	readonly bool bgmReferenceReadOnly;

	public override string FormHeading
	{
		get { return Res.GetString("81572F32-08B5-48DB-B247-30B85FC7BDB0", "SAD Print"); }
	}

	protected override void InitialiseForm()
	{
		base.InitialiseForm();

		InitializeComponent();
	}

	protected override void JustBeforeFirstTimeVisible()
	{
		base.JustBeforeFirstTimeVisible();

		bGMReferenceToPrintDropEdit.ReadOnly = bgmReferenceReadOnly;
	}

	void AcceptButton_Click(object sender, System.EventArgs e)
	{
		var jobDeclarationSadDocumentSupporter = CurrentDataItem as JobDeclarationSadDocumentSupporter;
		jobDeclarationSadDocumentSupporter.Validation.ValidateAll();
		if (jobDeclarationSadDocumentSupporter.HasErrors)
		{
			ShowErrorsDialog();
		}
		else
		{
			DialogResult = System.Windows.Forms.DialogResult.OK;
			Close();
		}
	}
}
