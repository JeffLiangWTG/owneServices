using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.CH.GUI;

public partial class SupportingDocSendingForm : Customs.GUI.SupportingDocSendingForm
{
	public SupportingDocSendingForm()
	{
	}

	public SupportingDocSendingForm(JobDeclarationSupportingDocSendingObjectParent messageSendingObjectParent)
		: base(messageSendingObjectParent)
	{
	}

	protected override void InitialiseForm()
	{
		base.InitialiseForm();
		InitializeComponent();
		InitializeColumns();
	}

	void InitializeColumns()
	{
		var caseNumberInfo = new ZTextBoxColumnStyleInfo();
		caseNumberInfo.ColumnName = SupportingDocSendingObject.Schema.CaseNumber;
		caseNumberInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
		caseNumberInfo.Caption = Res.GetString("378E0DD2-C3D2-4272-92B8-E799D0A4FDD1", "Reference");
		MessageSendingObjectsGrid.ColumnStyles.Add(caseNumberInfo);

		var eDocFileSizeInMBInfo = new ZCalcEditColumnStyleInfo();
		eDocFileSizeInMBInfo.ColumnName = SupportingDocSendingObject.Schema.EDocFileSizeInMB;
		eDocFileSizeInMBInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
		eDocFileSizeInMBInfo.Decimals = 2;
		MessageSendingObjectsGrid.ColumnStyles.Add(eDocFileSizeInMBInfo);
	}

	public override string FormHeading => Res.GetString("2E410636-3B17-4B3F-A78B-D8236E3C1535", "Send Accompanying Documents to Customs");

	protected override ZString LRNColumnName => Res.GetString("25A93AC6-DDF4-4554-929B-975180F30844", "Entry (MRN)");
}
