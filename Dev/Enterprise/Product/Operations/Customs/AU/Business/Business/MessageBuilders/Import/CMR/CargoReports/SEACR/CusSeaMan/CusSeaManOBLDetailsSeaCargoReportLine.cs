using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManOBLDetailsSeaCargoReportLine : ISeaCargoReportLine
	{
		public CusSeaManOBLDetailsSeaCargoReportLine(CusSeaManOBLDetail detail)
		{
			this.detail = detail;
		}
		readonly CusSeaManOBLDetail detail;

		public ZString ContainerMode => detail.BD_LineCargoType;

		public ZString ContainerNumber => detail.IsBulk || detail.IsBreakBulk ? ZString.Empty : detail.BD_ContainerNumber;

		public ZString SealNumber => detail.BD_SealNo;

		public ZString MarksAndNumbers => detail.BD_MarksAndNumbers;

		public bool ShippingOwnedContainerIndicator => detail.BD_ShipperOwnedContainerIndicator;

		public bool FumigationCertificateIndicator => detail.BD_FumigationIndicator;

		public bool HazardousGoodsIndicator => detail.BD_HazardousIndicator;

		public bool DocumentsIndicator => detail.BD_ReportableDocsIndicator;

		public bool SACIndication => detail.BD_SACIndicator;

		public bool PerishableGoodsIndicator => detail.BD_PerishableIndicator;

		public bool TimberIndicator => detail.BD_TimberIndicator;

		public bool PersonalEffectsIndicator => detail.BD_PersonalEffectsIndicator;

		public ZDecimal Volume => detail.BD_CargoVolume;

		public ZDecimal Weight => detail.BD_GrossWeight;

		public ZDecimal NetWeight => detail.BD_GrossWeight;

		public ZString WeightUQ => detail.BD_GrossWeightUM;

		public ZInt PackageCount => detail.BD_NoOfPacks;

		public ZString PackageType => detail.BD_PackType;

		public ZString GoodsDescription => detail.BD_GoodsDescription;

		public ZString ContainerType => detail.BD_TypeOfContainer;

		public ZString ContainerSize => detail.BD_ContainerSizeOrISOCode;

		public ZString ConsignorVendor => ZString.Empty;
	}
}
