using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class JobDeclarationMiscMessageSendingObjectCoreLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAmendmentReasonCodeList()
		{
			var messageSendingObject = new JobDeclarationMiscMessageSendingObject(Factory.New<CusEntryHeader>(), ElectronicDocumentTypeList.Codes._5AS, MessageFunctions.MessageFunctionCode.Extend, x => true);
			AssertEquals("11, 12, 13, 14, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 90", messageSendingObject.Lookups.AmendmentReasonCodeList.CodesAsString);

			messageSendingObject = new JobDeclarationMiscMessageSendingObject(Factory.New<CusEntryHeader>(), ElectronicDocumentTypeList.Codes._5DR, MessageFunctions.MessageFunctionCode.Amendment, x => true);
			AssertEquals("1, 2, 9", messageSendingObject.Lookups.AmendmentReasonCodeList.CodesAsString);

			messageSendingObject = new JobDeclarationMiscMessageSendingObject(Factory.New<CusEntryHeader>(), ElectronicDocumentTypeList.Codes._5DR, MessageFunctions.MessageFunctionCode.Cancellation, x => true);
			AssertEquals("1, 2, 9", messageSendingObject.Lookups.AmendmentReasonCodeList.CodesAsString);

			messageSendingObject = new JobDeclarationMiscMessageSendingObject(Factory.New<CusEntryHeader>(), ElectronicDocumentTypeList.Codes._5FE, MessageFunctions.MessageFunctionCode.Cancellation, x => true);
			AssertEquals("01, 02, 03, 04, 05, 06, 07, 08, 09, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 99", messageSendingObject.Lookups.AmendmentReasonCodeList.CodesAsString);
		}

		public void TestAmendmentFaultPartyList()
		{
			var messageSendingObject = new JobDeclarationMiscMessageSendingObject(Factory.New<CusEntryHeader>(), ElectronicDocumentTypeList.Codes._5AS, MessageFunctions.MessageFunctionCode.Extend, x => true);
			AssertEquals("A, B, C, D, E, F, G, H, Z", messageSendingObject.Lookups.FaultPartyList.CodesAsString);

			messageSendingObject = new JobDeclarationMiscMessageSendingObject(Factory.New<CusEntryHeader>(), ElectronicDocumentTypeList.Codes._5FE, MessageFunctions.MessageFunctionCode.Extend, x => true);
			AssertEquals("01, 02, 03, 04, 07, 08, 09, 99", messageSendingObject.Lookups.FaultPartyList.CodesAsString);
		}
	}
}
