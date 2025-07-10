using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(StatusRequest))]
	class StatusRequestTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIdentificationOrg()
		{
			var org = Factory.New<OrgHeader>();
			statusRequest.Identification = org.PK;
			AssertEquals(org.PK, statusRequest.IdentificationOrg.PK);
		}

		public void TestValidation()
		{
			AssertType<StatusRequestValidation>(statusRequest.Validation);
		}

		public void TestSetDefaultValues()
		{
			CombineAssertions(() =>
			{
				AssertEquals("EM_ApplicationCode", ApplicationCodes.DECustomsAtlasSystem, statusRequest.EM_ApplicationCode);
				AssertEquals("EM_MessageType", EDIMessageTypeList.Codes.NCTS, statusRequest.EM_MessageType);
				AssertEquals("EM_MessageSubType", NctsMessageSubTypeList.Codes.StatusRequestMessage, statusRequest.EM_MessageSubType);
				AssertEquals("EM_ReceiveTransmit", Direction.Transmit, statusRequest.EM_ReceiveTransmit);
			});
		}

		public void TestModuleNCTS()
		{
			statusRequest.Module = ZString.Empty;
			statusRequest.Module = ExportStatusRequestModuleCodeList.Codes.NCTS;
			CombineAssertions(() =>
			{
				AssertEquals("EM_ApplicationCode", ApplicationCodes.DECustomsAtlasSystem, statusRequest.EM_ApplicationCode);
				AssertEquals("EM_MessageType", EDIMessageTypeList.Codes.NCTS, statusRequest.EM_MessageType);
				AssertEquals("EM_MessageSubType", NctsMessageSubTypeList.Codes.StatusRequestMessage, statusRequest.EM_MessageSubType);
			});
		}

		public void TestModuleAES()
		{
			statusRequest.Module = ExportStatusRequestModuleCodeList.Codes.AES;
			CombineAssertions(() =>
			{
				AssertEquals("EM_ApplicationCode", ApplicationCodes.DECustomsAesSystem, statusRequest.EM_ApplicationCode);
				AssertEquals("EM_MessageType", EDIMessageTypeList.Codes.AES, statusRequest.EM_MessageType);
				AssertEquals("EM_MessageSubType", ExportMessageSubTypeList.Codes.EXQ, statusRequest.EM_MessageSubType);
			});
		}

		public void TestModuleInvalidModuleValue()
		{
			statusRequest.Module = "ZZZZ";
			CombineAssertions(() =>
			{
				AssertEquals("EM_ApplicationCode", ApplicationCodes.DECustomsAesSystem, statusRequest.EM_ApplicationCode);
				AssertEquals("EM_MessageType", EDIMessageTypeList.Codes.AES, statusRequest.EM_MessageType);
				AssertEquals("EM_MessageSubType", ExportMessageSubTypeList.Codes.EXQ, statusRequest.EM_MessageSubType);
			});
		}

		public void TestModuleMaxLength()
		{
			AssertEquals(4, statusRequest.ModuleInfo.MaxLength);
		}

		public void TestMovementReferenceNumberMaxLength()
		{
			AssertEquals(18, statusRequest.MovementReferenceNumberInfo.MaxLength);
		}

		public void TestMovementReferenceNumberStoredInCusEntryNum()
		{
			statusRequest.MovementReferenceNumber = "20DE12345678901234";
			var cusEntryNum = CusEntryNumber.Load(statusRequest, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			AssertEquals("20DE12345678901234", cusEntryNum.CE_EntryNum);
		}

		public void TestMRNCusEntryNumberDeleted()
		{
			statusRequest.MovementReferenceNumber = "20DE12345678901234";
			var cusEntryNum = CusEntryNumber.Load(statusRequest, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			cusEntryNum.Delete();
			AssertEquals(ZString.Empty, statusRequest.MovementReferenceNumber);
		}

		public void TestResponse()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Response?", statusRequest.ResponseInfo.HumanReadableName);
				AssertEquals("Initially No Response", false, statusRequest.Response);
				statusRequest.EM_Status = EDIMessage.Status.Acknowledged;
				AssertEquals("statusRequest.EM_Status = ACK -> Has Response", true, statusRequest.Response);
			});
		}

		public void TestResponseMessages()
		{
			var responseMessage = SetupValidResponseMessage();
			AssertEquals("Have 1 response message.", 1, statusRequest.ResponseMessages.Count());
			AssertEquals("Find response message", responseMessage.PK, statusRequest.ResponseMessages.First().PK);
		}

		public void TestDocManagerInfo()
		{
			AssertType<StatusRequestDocManagerInfo>(statusRequest.DocManagerInfo);
		}

		public void TestRoleMaxLength()
		{
			AssertEquals(1, statusRequest.RoleInfo.MaxLength);
		}

		public void TestLookups()
		{
			CombineAssertions(() =>
			{
				var lookups = statusRequest.Lookups;
				AssertType<StatusRequestLookups>("Type", lookups);
				AssertSame("Cached", lookups, statusRequest.Lookups);
			});
		}

		public void TestNoteTypes()
		{
			AssertEquals(true, statusRequest.NoteTypes.IsPredefinedNoteTypeByDescription(LogbookHelper.LogbookRegistrationNumberNoteDescription));
		}

		protected override bool IsDeleteSupported() => false;

		protected override void SetUp()
		{
			base.SetUp();
			statusRequest = Factory.New<StatusRequest>();
		}
		StatusRequest statusRequest;

		EDIMessage SetupValidResponseMessage()
		{
			var responseMessage = Factory.NewWithValidTestData<EDIMessage>();
			responseMessage.EM_ApplicationCode = statusRequest.EM_ApplicationCode;
			responseMessage.EM_MessageType = statusRequest.EM_MessageType;
			responseMessage.EM_MessageSubType = statusRequest.EM_MessageSubType;
			responseMessage.EM_ReceiveTransmit = Direction.Receive;
			responseMessage.EM_LinkTable = statusRequest.TableName;
			responseMessage.EM_LinkUniqueID = statusRequest.PK;
			return responseMessage;
		}
	}
}
