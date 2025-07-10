using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public interface ICustomsEntryLine : ICusCommonLine, ICIQDataLine
	{
		ZShort EntryLineNo { get; }
		ZString NameOfGoods { get; }
		ZString GoodsSpecModel { get; }
		ZString OriginStateCode { get; }
		ZString DomesticDistrictCode { get; }
		ZString DomesticRegionCode { get; }

		ZString CertOfOriginNumber { get; }
		ZString TradeAgreementCode { get; }
		ZString CertOfOriginCountry { get; }
		ZInt ItemNoOnCertOfOrigin { get; }
		ZString CertOfOriginType { get; }
	}
}
