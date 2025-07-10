using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ISeaCargoReportLine
	{
		ZString ContainerMode { get; }
		ZString ContainerNumber { get; }
		ZString SealNumber { get; }
		ZString MarksAndNumbers { get; }
		bool ShippingOwnedContainerIndicator { get; }
		bool FumigationCertificateIndicator { get; }
		bool HazardousGoodsIndicator { get; }
		bool DocumentsIndicator { get; }
		bool SACIndication { get; }
		bool PerishableGoodsIndicator { get; }
		bool TimberIndicator { get; }
		bool PersonalEffectsIndicator { get; }
		ZDecimal Volume { get; }
		ZDecimal NetWeight { get; }
		ZDecimal Weight { get; }
		ZString WeightUQ { get; }
		ZInt PackageCount { get; }
		ZString PackageType { get; }
		ZString GoodsDescription { get; }
		ZString ContainerSize { get; }
		ZString ContainerType { get; }
		ZString ConsignorVendor { get; }
	}
}
