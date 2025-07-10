using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	using System.Collections.Generic;

	public interface IG7ItemLine
	{
		// G08
		// LOC
		ZString CountryOfOrigin { get; }
		ZString ProvinceOfOrigin { get; }

		// RFF
		IEnumerable<ZString> VINs { get; }

		// G10
		// DOC
		IEnumerable<ZString> Permits { get; }

		// G11
		// IMD
		ZString ProductDescription { get; }

		// RFF
		ZInt InvoiceLineNumber { get; }

		// G12
		// CST
		ZString ClassificationNumber { get; }

		// MEA
		ZDecimal Quantity { get; }
		ZString UnitOfMeasure { get; }

		// MOA
		ZDecimal CustomsValue { get; }
		ZString CurrencyCode { get; }
	}
}
