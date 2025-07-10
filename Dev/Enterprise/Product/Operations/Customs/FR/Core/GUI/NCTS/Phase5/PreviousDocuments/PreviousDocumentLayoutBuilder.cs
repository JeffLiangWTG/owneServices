using Enterprise.Customs.FR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.NCTS
{
	public class PreviousDocumentLayoutBuilder<T> : ColumnLayoutBuilder<T, PreviousDocumentControlBag> where T : EU.Business.Declaration.MultiLineAddInfos.PreviousDocument
	{
		public override PreviousDocumentControlBag CommonBag => PreviousDocumentControlBag.Instance;

		protected override int MaxColumns => 1;

		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();
			SetVisibility(EU.NCTS.GUI.PreviousDocumentControlBag.Instance.ReferenceNumberTextBox, x => x.CSI_Code != UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage, x => x.CSI_CodeInfo);
			SetVisibility(CommonBag.ReferenceNumberCodeFindBox, x => x.CSI_Code == UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage, x => x.CSI_CodeInfo);
		}
	}
}
