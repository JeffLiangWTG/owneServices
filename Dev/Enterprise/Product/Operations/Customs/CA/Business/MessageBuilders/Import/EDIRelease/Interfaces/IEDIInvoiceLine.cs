using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	public interface IEDIInvoiceLineMin
	{
		// Minimum data
		ZString TariffNumber { get; }
		ZDecimal Quantity { get; }
		ZString QuantityUnits { get; }
		ZString CountryOfOrigin { get; }
		ZString ItemDescription { get; }
	}

	public interface IEDIInvoiceLineAQ : IEDIInvoiceLineMin
	{
		// AQ data
		ZInt PageNumber { get; }
		ZInt LineNumber { get; }
		ZDecimal UnitPrice { get; }
		ZDecimal LinePrice { get; }
		ZString LinePriceCurrency { get; }
	}

	public interface IEDIInvoiceLineOGD : IEDIInvoiceLineAQ
	{
		// OGD data
		ZString ImportReasonCode { get; }
		ZString[] RegistrationNumbers { get; }
		ZString[] RegistrationTypes { get; }
		ZBool CompliantCompletionIndicator { get; }
		ZBool CompliantImportDateIndicator { get; }
		ZString TIIN { get; }
		ZString Model { get; }
		ZString ModelNumber { get; }
		ZString BrandName { get; }
		ZString TypeSize { get; }
		ZString RequirementID { get; }
		ZString RequirementVersion { get; }
		ZString DestinationProvince { get; }
		ZString MiscID { get; }
		ZString CFIAOrigin { get; }
		ZString AirsCode { get; }
		ZString EndUse { get; }
		ZString Make { get; }
		ZString VehicleClass { get; }
		ZString[] VIN { get; }
		ZString[] AssemblyMonth { get; }
	}
}
