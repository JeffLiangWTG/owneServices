using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business
{
	public interface ICsvUsageReportWriter
	{
		void WriteCsvUsageReport(ZDateTime usageTime, ZString company, ZString branch, ZString staff, ZString reference,
			ZString priceCode, ZString priceItemDescription, ZInt unitCount, ZDecimal? adjustedUnitCount = null, ZString? tenantID = null);
	}
}

