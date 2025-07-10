using Enterprise.Customs.BE.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.NCTS.GUI;

public class PreviousDocumentLayoutBuilder<T> : ColumnLayoutBuilder<T, PreviousDocumentControlBag> where T : EU.Business.Declaration.MultiLineAddInfos.PreviousDocument
{
	public override PreviousDocumentControlBag CommonBag => PreviousDocumentControlBag.Instance;

	protected override int MaxColumns => 1;

	protected override void SetDefaultVisibilities()
	{
		base.SetDefaultVisibilities();
		SetVisibility(EU.NCTS.GUI.PreviousDocumentControlBag.Instance.ReferenceNumberTextBox, x => x.CSI_Code != Constants.PreviousDocumentTypes.CargoManifest, x => x.CSI_CodeInfo);
		SetVisibility(CommonBag.ReferenceNumberN785UserControl, x => x.CSI_Code == Constants.PreviousDocumentTypes.CargoManifest, x => x.CSI_CodeInfo);
	}
}
