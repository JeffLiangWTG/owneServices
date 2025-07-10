using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public abstract class CMRCUSDECMessageTest : CMRMessageTest
	{
		public void TestGetIncomingMessageByBGMRefAndVersion()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_DeclarationReference = "B00122382";
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "B00122382/1";
			CMRIMDMessage iMDMessage = Factory.New<CMRIMDMessage>();
			iMDMessage.EM_MessageText = CMRImportDeclarationTestData.IMD; //IMD+B00122382/1/SYD7:7
			iMDMessage.EM_LinkedObject = entryHeader;
			iMDMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;

			CMRREFACCMessage message1 = Factory.New<CMRREFACCMessage>();
			message1.EM_MessageText = CMRImportDeclarationTestData.REFACC; //RFF+ABO:B00122382/1/SYD1::7
			entryHeader.Messages.Add(message1);
			EDIMessage foundMessage = iMDMessage.GetSingleIncomingMessageByBGMRefVersionAndType(entryHeader.Messages, "REFACC");
			AssertNull("Should not find a message", foundMessage);

			iMDMessage = Factory.New<CMRIMDMessage>();
			iMDMessage.EM_MessageText = CMRImportDeclarationTestData.IMD.Replace("/1/SYD7:7", "/1/SYD1:7");
			iMDMessage.EM_LinkedObject = entryHeader;
			iMDMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			foundMessage = iMDMessage.GetSingleIncomingMessageByBGMRefVersionAndType(entryHeader.Messages, "REFREJ");
			AssertNull("Should not find a message", foundMessage);
			foundMessage = iMDMessage.GetSingleIncomingMessageByBGMRefVersionAndType(entryHeader.Messages, "REFACC");
			AssertNotNull("Found a message", foundMessage);
			AssertEquals("Is the correct message", message1.PK, foundMessage.PK);

			iMDMessage = Factory.New<CMRIMDMessage>();
			iMDMessage.EM_MessageText = CMRImportDeclarationTestData.IMD.Replace("/1/SYD7:7", "/1/SYD1:6");
			iMDMessage.EM_LinkedObject = entryHeader;
			iMDMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			foundMessage = iMDMessage.GetSingleIncomingMessageByBGMRefVersionAndType(entryHeader.Messages, "REFACC");
			AssertNull("Should not find a message", foundMessage);
		}

		public void TestIsWithdrawalMessage()
		{
			CMRCUSDECMessage message = (CMRCUSDECMessage)GetNewBusinessObject();
			message.EM_MessageText = GetSampleWithdrawMessage();
			AssertEquals("IsWithdrawalMessage", true, message.IsWithdrawalMessage);
		}

		public void TestBranchIdentifier()
		{
			var message = (CMRCUSDECMessage)GetNewBusinessObject();
			message.EM_MessageText = CMRImportDeclarationTestData.IMD;
			AssertEquals("BranchIdentifier", "AA33HF", message.BranchIdentifier);
		}

		#region Implementation

		protected abstract ZString GetSampleWithdrawMessage();

		#endregion
	}
}
