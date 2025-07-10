using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	class InwardProcessingProviderTest : DataProviderTestCase<InwardProcessingProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertNull("NULL", InwardProcessingProvider.NewOrNull(null));
				AssertNotNull("Has PreviousDocuments", InwardProcessingProvider.NewOrNull(previousDocumentMaster));

				entryInstruction.PreviousDocuments.RemoveAndDeleteAll();
				AssertNull("No PreviousDocuments", InwardProcessingProvider.NewOrNull(previousDocumentMaster));
			});
		}

		public void TestGoodsItemQuantity()
		{
			entryInstruction.PreviousDocuments.AddNew();
			AssertEquals(2, Provider.GoodsItemQuantity);
		}

		public void TestProcessingOwnerIdentifier()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", ZString.Empty, Provider.ProcessingOwnerIdentifier);

				previousDocumentMaster.AuthorizationNumber = "AUTH12345";
				AssertEquals("Not empty", "AUTH12345", Provider.ProcessingOwnerIdentifier);
			});
		}

		public void TestSimplifiedGrantAuthorisationFlag()
		{
			CombineAssertions(() =>
			{
				AssertEquals("false", false, Provider.SimplifiedGrantAuthorisationFlag);

				previousDocumentMaster.SimplifiedGrantAuthorizationFlag = true;
				AssertEquals("true", true, Provider.SimplifiedGrantAuthorisationFlag);
			});
		}

		public void TestMonitoringCustomsOfficeReferenceNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", ZString.Empty, Provider.MonitoringCustomsOfficeReferenceNumber);

				previousDocumentMaster.CSI_CustomsOffice = "DE1234";
				AssertEquals("Not empty", "DE1234", Provider.MonitoringCustomsOfficeReferenceNumber);
			});
		}

		public void TestGoodsItems()
		{
			entryInstruction.PreviousDocuments.AddNew();
			AssertEquals(2, Provider.GoodsItems.Count);
		}

		protected override InwardProcessingProvider GetProvider() => InwardProcessingProvider.NewOrNull(previousDocumentMaster);

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.PreviousDocuments.AddNew();
		}
		CusEntryInstruction entryInstruction;
		PreviousDocumentMaster previousDocumentMaster => entryInstruction.PreviousDocumentMaster;
	}
}
