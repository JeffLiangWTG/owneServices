using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Module.Testing
{
	[TestedType(typeof(DocumentListMessagesModule))]
	public class DocumentListMessagesModuleTest : ZModuleBasherWithFetchHintsTest
	{
		protected override ZFilterModule CreateModuleForFetchHintsTest() => new DocumentListMessagesModule();
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.KR.DocumentListMessages;

		protected override void SetupDataForFetchHintsTest()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSOF", "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType("CUSDP", "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, "CUSOF", "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, "CUSDP", "20", "내륙기지통관과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			for (int idx = 0; idx < 20; idx++)
			{
				CreateDocumentListMessageForFetchHintTest(idx);
			}
			Factory.Save();
		}
		// 2022-11-21, To be changed in WI00550695.
		void CreateDocumentListMessageForFetchHintTest(int idx)
		{
			var interchange = Factory.NewWithValidTestData<Enterprise.Messaging.Business.EDIInterchange>();
			interchange.EI_InterchangeNum = idx.ToString();
			interchange.EI_SystemCreateTimeUtc = ZDateTime.Now;
			interchange.EI_From = "EDITST" + idx;
			interchange.EI_To = "TSTEDI" + idx;
			interchange.EI_Status = Enterprise.Messaging.Business.EDIInterchange.Status.Queued;
			interchange.EI_SessionGUID = ZGuid.NewZGuid();

			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_MessageType = "DLT";
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			message.EM_ApplicationReference = "1" + idx;
			message.EM_MessageNum = idx.ToString();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageSubType = "XXX";
			message.EM_Status = "RCV";
			message.EM_SystemCreateTimeUtc = ZDateTime.Now;
			message.EM_EI = interchange.PK;

			var transactionID = "20200409095703202004" + idx.ToString("00") + "-ELI-" + ZGuid.NewZGuid().ToString();
			message.EM_MessageText = transactionID + ",GOVCBR5AF";

			var cusPollingTransaction = Factory.NewWithValidTestData<CusPollingTransaction>();
			cusPollingTransaction.CPT_ParentID = message.PK;
			cusPollingTransaction.CPT_ApplicationCode = "KRC";
			cusPollingTransaction.CPT_NumberOfAttempts = 1;
			cusPollingTransaction.CPT_Reference = "5AF";
			cusPollingTransaction.CPT_Status = "OPN";
			cusPollingTransaction.CPT_TransactionID = transactionID;
		}
	}
}
