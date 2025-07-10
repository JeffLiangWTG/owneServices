using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Testing;

static class MergeTestHelper
{
	public static JobComInvoiceLine GetNewInvoiceLine(JobComInvoiceHeader invoiceHeader, ZString tariff, ZString description)
	{
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine.JI_Description = description;
		invoiceLine.JI_Tariff = tariff;

		return invoiceLine;
	}

	public static InvoiceLinePackagePivot GetNewPackagePivot(JobComInvoiceLine invoiceLine, Customs.Business.BasePackage package, ZInt numberOfPacks)
	{
		var packagesPivotCollection = invoiceLine.PackagesPivot;
		var packagePivot = packagesPivotCollection.AddNew();
		packagePivot.CHC_CW = package.PK;
		packagePivot.CHC_NumberOfPacks = numberOfPacks;
		packagePivot.CHC_JE = invoiceLine.Declaration.PK;

		return packagePivot;
	}
}
