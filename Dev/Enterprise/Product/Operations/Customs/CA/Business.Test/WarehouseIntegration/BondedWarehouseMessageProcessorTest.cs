using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class BondedWarehouseMessageProcessorTest : TestCaseWithFactory
	{
		public void TestReferenceDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000001";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "12345000000000011";
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			var message = Factory.New<B3Message>();
			message.EM_LinkedObject = entryHeader;
			declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreationHeld;
			Factory.Save();
			EmailDef createdEmail = null;
			var processor = new BondedWarehouseMessageProcessor(message.PK, null, (email) => { createdEmail = email; });
			processor.ProcessAfterSaved(true);
			AssertEquals("Failed to create Stock Levels Update for Declaration Reference: B00000001", createdEmail.Subject);
			AssertContains("Job Number : " + EmailDefBuilder.GetJobLink(declaration, declaration.JE_DeclarationReference), createdEmail.Body);
			AssertContains("Reference Number : 12345000000000011", createdEmail.Body);
		}
	}
}
