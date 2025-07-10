using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Messaging
{
	public interface IBodyEad
	{
		ZString BodyRecordUniqueReference { get; set; }

		ZString ExciseProductCode { get; set; }

		ZString CnCode { get; set; }

		ZString Quantity { get; set; }

		ZString GrossWeight { get; set; }

		ZString NetWeight { get; set; }

		ZString FiscalMark { get; set; }

		ZString FiscalMarkUsedFlag { get; set; }

		ZString Density { get; set; }

		ZString CommercialDescription { get; set; }

		ZString BrandNameOfProducts { get; set; }

		IPackage Package { get; set; }
	}
}
