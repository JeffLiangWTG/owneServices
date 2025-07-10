using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.IE.ExitControl.Business.Testing;

namespace Enterprise.Customs.IE.ExitControl.Business.AES.Testing
{
	sealed class AlternativeEvidenceProviderTest : Customs.Business.Testing.DataProviderTestCase<AlternativeEvidenceProvider>
	{
		public void TestType()
		{
			alternateEvidence.CY_Code = "13";
			AssertEquals("Evidence type", "13", Provider.Type);
		}

		public void TestTransportDocument()
		{
			var info1 = alternateEvidence.AdditionalInfos.AddNew();
			var info2 = alternateEvidence.AdditionalInfos.AddNew();
			info1.CSI_Code = "01";
			info1.CSI_ReferenceNumber = "Reference1";
			info2.CSI_Code = "02";
			info2.CSI_ReferenceNumber = "Reference2";
			AssertContainsExactElementsInAnyOrder("Transport document", new ZString[] { "01", "02" }, Provider.TransportDocument.Select(x => x.Type));
			AssertContainsExactElementsInAnyOrder("Transport document", new ZString[] { "Reference1", "Reference2" }, Provider.TransportDocument.Select(x => x.Reference));
		}

		protected override AlternativeEvidenceProvider GetProvider() => new AlternativeEvidenceProvider(alternateEvidence);

		protected override void SetUp()
		{
			base.SetUp();
			(report, _, _) = CusExitReportTest.GetNewBusinessObject(Factory);
			alternateEvidence = report.AlternativeEvidences.AddNew();
		}
		AlternativeEvidence alternateEvidence;
		CusExitReport report;
	}
}
