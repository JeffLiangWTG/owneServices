using System;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Core.Testing;

namespace Enterprise.Customs.EU.DataTransfer.Universal.Testing
{
	class PackagesReaderTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestPackingLinesReadFromUniversalShipment()
		{
			CustomsDataRegistry.Instance.AutoAllocatePackageToInvoiceLines.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false); // so we know that any pivots created were done by XML
			var declarationDataObject = PackagesWritingTest.GetDataObjectWithLineLevelPackages<JobDeclaration, JobComInvoiceLine>(Factory);
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			var packsBOs = declarationBO.InvoiceLines[0].PackagesPivot;
			var packsBO1 = packsBOs[0]; // This shows that the pivots have been created
			var packsBO2 = packsBOs[1];
			AssertEquals("1B", packsBO1.Package.CW_PackType);
			AssertEquals("AE", packsBO2.Package.CW_PackType);
			AssertEquals(1, packsBO1.CHC_NumberOfPacks);
			AssertEquals(2, packsBO2.CHC_NumberOfPacks);
		}

		protected override void SetUp()
		{
			base.SetUp();
			logger = new TestErrorLogger();
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		protected TestErrorLogger logger;
	}
}
