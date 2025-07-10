using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.DE.EMCS.Messaging
{
	public interface IED801Line : IInboundProvider
	{
		ZInt BodyRecordUniqueReference { get; }
		ZString ExciseProductCode { get; }
		ZString CnCode { get; }
		ZBool FiscalMarkUsedFlag { get; }
		ZString FiscalMark { get; }
		ZString DesignationOfOrigin { get; }
		ZString CommercialDescription { get; }
		ZString BrandNameOfProducts { get; }
		ZDecimal Quantity { get; }
		ZDecimal GrossWeight { get; }
		ZDecimal NetWeight { get; }
		ZDecimal AlcoholicStrength { get; }
		ZDecimal DegreePlato { get; }
		ZDecimal SizeOfProducer { get; }
		ZDecimal Density { get; }
		IReadOnlyCollection<IEMCSPackageInComing> Packages { get; }
		ZString WineGrowingZoneCode { get; }
		ZString WineProductCategory { get; }
		ZString WineProductThirdCountryOfOrigin { get; }
		ZString WineProductOtherInfo { get; }
		IReadOnlyCollection<ZString> WineOperationCodes { get; }
		ZString MaturationPeriodOrAgeOfProducts { get; }
	}
}
