using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Module.Testing
{
	[TestedType(typeof(DocumentListMessagesController))]
	public class DocumentListMessagesControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.KR.DocumentListMessages;

		public override void TestEditForm()
		{
			Assert("Not Implemented", true);
		}

		public override void TestViewForm()
		{
			Assert("Not Implemented", true);
		}

		public override void TestNewForm()
		{
			Assert("Not Implemented", true);
		}

		public override void TestDeleteForm()
		{
			Assert("Not Implemented", true);
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert("Not Implemented", true);
		}

		// 2022-11-21, To be changed in WI00550695.
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var interchange = Factory.NewWithValidTestData<Enterprise.Messaging.Business.EDIInterchange>();
			interchange.EI_InterchangeNum = "11";
			interchange.EI_SystemCreateTimeUtc = new ZDateTime(2022, 10, 01);
			interchange.EI_From = "EDITST1";
			interchange.EI_To = "TSTEDI1";
			interchange.EI_Status = "SNT";
			interchange.EI_SessionGUID = new ZGuid("C214E153-EE8A-4D04-9B36-AF1BBC9483D4");

			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_MessageType = "DLT";
			message.EM_ApplicationCode = "KRC";
			message.EM_ApplicationReference = "123";
			message.EM_MessageNum = "1";
			message.EM_ReceiveTransmit = "RCV";
			message.EM_MessageSubType = "TST";
			message.EM_Status = "RCV";
			message.EM_MessageText = "2020040909570320200409-ELI-edb54e29-d2aa-49ea-849c-87f29bbf1f42,GOVCBR5AF";
			message.EM_SystemCreateTimeUtc = new ZDateTime(2021, 10, 01);
			message.EM_EI = interchange.PK;

			var cusPollingTransaction = Factory.NewWithValidTestData<CusPollingTransaction>();
			cusPollingTransaction.CPT_ParentID = message.PK;
			cusPollingTransaction.CPT_ApplicationCode = "KRC";
			cusPollingTransaction.CPT_Status = "OPN";
			cusPollingTransaction.CPT_NumberOfAttempts = 1;
			cusPollingTransaction.CPT_Reference = "5AF";
			cusPollingTransaction.CPT_TransactionID = "2020040909570320200409-ELI-edb54e29-d2aa-49ea-849c-87f29bbf1f42";
			Factory.Save();

			return message;
		}
	}
}
