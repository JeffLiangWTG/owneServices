using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocStatementSummaryLineCollection))]
	public class DocStatementSummaryLineCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocStatementSummaryLineCollection>
	{
		public void TestGetLineFromOrgCode()
		{
			DocStatementSummaryLineCollection collection = new DocStatementSummaryLineCollection(Factory);
			OrgHeader organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			organisation1.OH_Code = "PLNTEXPRSS";
			ARInvoice header1 = Factory.NewWithValidTestData<ARInvoice>();
			header1.AH_OH = organisation1.PK;
			DocTransactionHeader docHeader1 = DocTransactionHeader.New(header1, Factory);
			OrgHeader organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			organisation2.OH_Code = "CRSTYCRB";
			ARInvoice header2 = Factory.NewWithValidTestData<ARInvoice>();
			header2.AH_OH = organisation2.PK;
			DocTransactionHeader docHeader2 = DocTransactionHeader.New(header2, Factory);
			DocStatementSummaryLine line1 = new DocStatementSummaryLine(null, docHeader1);
			DocStatementSummaryLine line2 = new DocStatementSummaryLine(null, docHeader2);
			collection.Add(line1);
			collection.Add(line2);
			AssertEquals(line2, collection.GetLineFromOrgCode("CRSTYCRB"));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			ARInvoice header = Factory.NewWithValidTestData<ARInvoice>();
			DocTransactionHeader docHeader = DocTransactionHeader.New(header, Factory);
			return new DocStatementSummaryLine(null, docHeader);
		}

		protected override DocStatementSummaryLineCollection GetCollectionToTest()
		{
			return new DocStatementSummaryLineCollection(Factory);
		}
	}
}
