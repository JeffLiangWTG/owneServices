using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing;

[TestedType(typeof(ZAUCustomsDeclarationForm))]
public class ImportAUCustomsDeclarationFormTest_ForWhenDeclarationCancelled : Customs.GUI.Testing.BaseCustomsDeclarationFormTest_ForWhenDeclarationCancelled<JobDeclaration>
{
	public override ZString MessageTypeForFormBashing => Common.Shared.SharedJobMessageTypeList.Codes.Import;

	protected override BaseJobComInvoiceLine CreateInvoiceLineForPerformanceTest(BaseJobComInvoiceHeader invoiceHeader, int index, int invoiceIndex)
	{
		var invoiceLine = (JobComInvoiceLine)invoiceHeader.JobComInvoiceLines.AddNew();
		var tariff = Tariffs[(index + (invoiceIndex * 10)) % Tariffs.Length];
		invoiceLine.JI_Tariff = tariff.SC_TariffClassificationNumber + tariff.SC_StatisticalClassificationCode;
		invoiceLine.AddInfo.ZA_PST = "GEN";
		return invoiceLine;
	}

	CMRStatisticalClassificationPeriodSnapshot[] Tariffs => tariffs ?? (tariffs = Factory.Load<CMRStatisticalClassificationPeriodSnapshot>(new ZQuery(CMRStatisticalClassificationPeriodSnapshotSchema.SC_TariffClassificationNumber, AUCustomsDeclarationFormTest.TariffsToLoad)));
	CMRStatisticalClassificationPeriodSnapshot[] tariffs;
}
