using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.Business.Documents.CertificateOfOrigin.Testing
{
	class EURCertificateOfOriginWrapperTest : TestCaseWithFactory
	{
		public void TestReferenceDateFormat()
		{
			AssertEquals("ReferenceDateFormat", "dd-MM-yyyy", certificateOfOriginWrapper.ReferenceDateFormat);
		}

		public void TestTransportDetail()
		{
			AssertType<TransportDetailWrapper>("TransportDetail Type", certificateOfOriginWrapper.TransportDetail);
		}

		public void TestGoodsSummary()
		{
			AssertType<EURGoodsSummaryWrapper>("GoodsSummary Type", certificateOfOriginWrapper.GoodsSummary);
		}

		public void TestCustomsEndorsement()
		{
			AssertType<CustomsEndorsementWrapper>("CustomsEndorsement (from EntryHeader) Type", certificateOfOriginWrapper.CustomsEndorsement);
		}

		public void TestExporterDeclaration()
		{
			AssertType<ExporterDeclarationWrapper>("ExporterDeclaration Type", certificateOfOriginWrapper.ExporterDeclaration);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			certificateOfOriginWrapper = new EURCertificateOfOriginWrapper(entryHeader);
		}

		EU.Business.Documents.CertificateOfOrigin.IEURCertificateOfOrigin certificateOfOriginWrapper;
	}
}
