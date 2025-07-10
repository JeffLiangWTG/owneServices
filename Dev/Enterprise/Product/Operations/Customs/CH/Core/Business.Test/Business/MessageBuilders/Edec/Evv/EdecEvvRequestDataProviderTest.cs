using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business.Testing;

internal class EdecEvvRequestDataProviderTest : TestCaseWithFactory
{
	public void TestNew()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new EdecEvvRequestDataProvider(null));
	}

	public void TestProvider()
	{
		GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "C123";
		var entryHeader = Factory.New<CusEntryHeader>();
		var sendingObject = new EvvRequestSendingObject(entryHeader, "M100", 3, "DTY");
		var dataProvider = new EdecEvvRequestDataProvider(sendingObject);
		CombineAssertions(() =>
		{
			AssertEquals("RequestorTraderIdentificationNumber", "C123", dataProvider.RequestorTraderIdentificationNumber);
			AssertNull("RequestorCorrelationID", dataProvider.RequestorCorrelationID);
			AssertEquals("CustomsDeclarationNumber", "M100", dataProvider.CustomsDeclarationNumber);
			AssertEquals("CustomsDeclarationVersion", 3, dataProvider.CustomsDeclarationVersion);
			AssertEquals("DocumentType", EvvDocumentType.Codes.TaxationDecisionCustomsDuties, dataProvider.DocumentType);
		});
	}

	public void TestCustomsDeclarationVersion()
	{
		CombineAssertions(() =>
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var sendingObject = new EvvRequestSendingObject(entryHeader, "M100", 0, "DTY");

			var dataProvider = new EdecEvvRequestDataProvider(sendingObject);
			AssertNull($"MovementReferenceNumber={entryHeader.MovementReferenceNumber}", dataProvider.CustomsDeclarationVersion);

			sendingObject = new EvvRequestSendingObject(entryHeader, "M100", 2, "DTY");
			dataProvider = new EdecEvvRequestDataProvider(sendingObject);
			AssertEquals($"MovementReferenceNumber={entryHeader.MovementReferenceNumber}", 2, dataProvider.CustomsDeclarationVersion);
		});
	}
}
