using CargoWise.EntityFramework;
using Enterprise.Customs.Common.EU;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	[TestedType(typeof(AlternativeEvidence))]
	sealed class AlternativeEvidenceTest : Customs.Business.Testing.CusCodeDataTest<AlternativeEvidence>
	{
		public void TestValidation()
		{
			(var alternativeEvidence, var report, _, _) = GetNewBusinessObject(Factory);
			report.CER_Type = "!@";
			AssertType<AlternativeEvidenceValidation>(alternativeEvidence.Validation);
			report.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport;
			AssertType<InformationOnNonExitedExportAlternativeEvidenceValidation>("InformationOnNonExitedExport", alternativeEvidence.Validation);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory).alternativeEvidence;

		public static (AlternativeEvidence alternativeEvidence, CusExitReport report, CusExitConsignment consignment, CusExitHeader header) GetNewBusinessObject(BusinessObjectFactory factory)
		{
			(var report, var consignment, var header) = CusExitReportTest.GetNewBusinessObject(factory);
			var alternativeEvidence = report.AlternativeEvidences.AddNew();
			return (alternativeEvidence, report, consignment, header);
		}
	}
}
