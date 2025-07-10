using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing;

[TestedType(typeof(ZAUCustomsDeclarationForm))]
public class ExportAUCustomsDeclarationFormTest_AHECC : ExportAUCustomsDeclarationFormTest
{
	protected override BaseJobComInvoiceLine CreateInvoiceLineForPerformanceTest(BaseJobComInvoiceHeader invoiceHeader, int index, int invoiceIndex)
	{
		var invoiceLine = (JobComInvoiceLine)invoiceHeader.JobComInvoiceLines.AddNew();
		var tariff = AHECCs[(index + (invoiceIndex * 10)) % AHECCs.Length];
		invoiceLine.JI_Tariff = tariff.UA_AHECC + tariff.UA_AHECC;
		return invoiceLine;
	}

	AUCAHECC[] AHECCs => aheccs ?? (aheccs = Factory.Load<AUCAHECC>(new ZQuery(AUCAHECCSchema.UA_AHECC, ExportTariffs)));
	AUCAHECC[] aheccs;

	protected override bool UseCustomsReferenceDataValue => false;
}
