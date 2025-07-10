using CargoWise.Types;
using Enterprise.Customs.AE.Manifest.Business;

namespace Enterprise.Customs.AE.Manifest.GUI;

public class SupportingDocSendingForm : Customs.GUI.SupportingDocSendingForm
{
	public SupportingDocSendingForm(ManifestSupportingDocSendingObjectParent manifestWrapper)
		: base(manifestWrapper)
	{
	}

	protected override void InitialiseForm()
	{
		base.InitialiseForm();
		InitializeColumns();
	}

	void InitializeColumns()
	{
		var cUSRESInfo = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
		cUSRESInfo.ColumnName = nameof(SupportingDocSendingObject.CUSRES);
		cUSRESInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
		MessageSendingObjectsGrid.ColumnStyles.Add(cUSRESInfo);

		var cUSCARInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
		cUSCARInfo.ColumnName = nameof(SupportingDocSendingObject.CUSCAR);
		cUSCARInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
		MessageSendingObjectsGrid.ColumnStyles.Add(cUSCARInfo);

		var eDocFileSizeInMBInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
		eDocFileSizeInMBInfo.ColumnName = nameof(SupportingDocSendingObject.EDocFileSizeInMB);
		eDocFileSizeInMBInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
		eDocFileSizeInMBInfo.Decimals = 2;
		eDocFileSizeInMBInfo.IsMandatory = true;
		MessageSendingObjectsGrid.ColumnStyles.Add(eDocFileSizeInMBInfo);
	}

	protected override ZBool LRNColumnVisible => ZBool.False;
}
