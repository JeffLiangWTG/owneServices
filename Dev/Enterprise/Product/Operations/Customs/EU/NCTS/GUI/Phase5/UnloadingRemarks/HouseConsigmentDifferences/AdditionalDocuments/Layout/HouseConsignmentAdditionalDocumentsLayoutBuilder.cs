using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class HouseConsignmentAdditionalDocumentsLayoutBuilder<T> : ColumnLayoutBuilder<T, HouseConsignmentAdditionalDocumentsControlBag> where T : NctsBillAdditionalDocument
	{
		public override HouseConsignmentAdditionalDocumentsControlBag CommonBag => HouseConsignmentAdditionalDocumentsControlBag.Instance;

		protected override int MaxColumns => 1;

		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();
			SetVisibility(CommonBag.ReferenceNumberTextBox, nctsBillAdditionalDocument => nctsBillAdditionalDocument.CSI_SubType != AdditionalInfoSubTypeList.Codes.AdditionalInformation, nctsBillAdditionalDocument => nctsBillAdditionalDocument.CSI_SubTypeInfo);
			SetVisibility(CommonBag.TextTextBox, nctsBillAdditionalDocument => nctsBillAdditionalDocument.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation, nctsBillAdditionalDocument => nctsBillAdditionalDocument.CSI_SubTypeInfo);
		}
	}
}
