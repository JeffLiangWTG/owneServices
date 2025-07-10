using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging.Testing
{
	class TP5MessageSendingObjectValidationTest : TestCaseWithFactory
	{
		public void TestCheckJustification()
		{
			var expectedMessage = "Justification is mandatory";
			CombineAssertions(() =>
			{
				messageSendingObject.MessageType = TP5MessageTypeList.Codes.CC014C;
				messageSendingObject.Validation.ValidateJustification();
				AssertHasError("Justification must be not empty when message is 014", messageSendingObject.JustificationInfo, expectedMessage);
				messageSendingObject.MessageType = TP5MessageTypeList.Codes.CC015C;
				messageSendingObject.Validation.ValidateJustification();
				AssertNoErrorContaining("Justification can be empty when message is not 014", messageSendingObject.JustificationInfo, expectedMessage);
			});
		}

		public void TestCheckJustificationCode()
		{
			var expectedMessage = "Regular Justification Code is mandatory";
			CombineAssertions(() =>
			{
				messageSendingObject.MessageType = TP5MessageTypeList.Codes.CC014C;
				messageSendingObject.Validation.ValidateJustificationCode();
				AssertHasError("JustificationCode must be not empty when message is 014", messageSendingObject.JustificationCodeInfo, expectedMessage);
				messageSendingObject.MessageType = TP5MessageTypeList.Codes.CC015C;
				messageSendingObject.Validation.ValidateJustificationCode();
				AssertNoErrorContaining("JustificationCode can be empty when message is not 014", messageSendingObject.JustificationCodeInfo, expectedMessage);
			});
		}

		public void TestCheckQueryInformation()
		{
			var expectedMessage = "Query Information is mandatory";
			CombineAssertions(() =>
			{
				messageSendingObject.MessageType = TP5MessageTypeList.Codes.CC141C;
				messageSendingObject.Validation.ValidateQueryInformation();
				AssertHasError("Query Information must not be empty when message is 141", messageSendingObject.QueryInformationInfo, expectedMessage);
				messageSendingObject.QueryInformation = "Query Information Text";
				AssertNoErrorContaining("No error when query information is not empty and message is 141", messageSendingObject.QueryInformationInfo, expectedMessage);

				messageSendingObject.MessageType = TP5MessageTypeList.Codes.CC015C;
				messageSendingObject.Validation.ValidateQueryInformation();
				AssertNoErrorContaining("Query Information can be empty when message is not 141", messageSendingObject.QueryInformationInfo, expectedMessage);
			});
		}

		public void TestCheckQueryIdentifier()
		{
			TP5MessageSendingObjectLookupsTest.SetUpCusCodeList_CL054(Factory);
			var expectedMessage = "Query Identifier is a mandatory value";
			CombineAssertions(() =>
			{
				messageSendingObject.MessageType = TP5MessageTypeList.Codes.CC034C;
				messageSendingObject.QueryIdentifier = ZString.Empty;
				messageSendingObject.Validation.ValidateQueryIdentifier();
				AssertHasMessageErrorContaining("Query Identifier must not be empty when message is 034", messageSendingObject.QueryIdentifierInfo, expectedMessage);
				messageSendingObject.QueryIdentifier = "Identifier1";
				AssertNoMessageErrorContaining("No message error when Query Identifier is not empty and message is 034", messageSendingObject.QueryIdentifierInfo, expectedMessage);
				ValidationTestHelper.AssertInvalidCodeMessageError(messageSendingObject.QueryIdentifierInfo, "XXXXX", "Identifier1");

				messageSendingObject.MessageType = TP5MessageTypeList.Codes.CC015C;
				messageSendingObject.QueryIdentifier = ZString.Empty;
				messageSendingObject.Validation.ValidateQueryIdentifier();
				AssertNoMessageErrorContaining("Query Identifier can be empty when message is not 034", messageSendingObject.QueryIdentifierInfo, expectedMessage);
			});
		}

		public void TestCheckPeriodFrom()
		{
			CombineAssertions(() =>
			{
				messageSendingObject.MessageType = TP5MessageTypeList.Codes.CC034C;

				messageSendingObject.Validation.ValidatePeriodFrom();
				AssertNoErrors("Period From can be empty", messageSendingObject.PeriodFromInfo);

				messageSendingObject.PeriodFrom = ZDateTime.Invalid;
				AssertHasErrorContaining("Period From must be valid when it is not empty", messageSendingObject.PeriodFromInfo, "Please enter a valid date.");

				messageSendingObject.PeriodTo = new ZDateTime(2025, 1, 5);
				messageSendingObject.PeriodFrom = new ZDateTime(2025, 1, 6);
				AssertHasErrorContaining("When both Period From and Period To are filled in, there should be validation that Period From is before Period To", messageSendingObject.PeriodFromInfo, "Period From must be older than Period To.");

				messageSendingObject.PeriodFrom = new ZDateTime(2025, 1, 4);
				AssertNoErrors(messageSendingObject.PeriodFromInfo);
			});
		}

		public void TestCheckPeriodTo()
		{
			CombineAssertions(() =>
			{
				messageSendingObject.MessageType = TP5MessageTypeList.Codes.CC034C;

				messageSendingObject.Validation.ValidatePeriodTo();
				AssertNoErrors("Period To can be empty", messageSendingObject.PeriodToInfo);

				messageSendingObject.PeriodTo = ZDateTime.Invalid;
				AssertHasErrorContaining("Period To must be valid when it is not empty", messageSendingObject.PeriodToInfo, "Please enter a valid date.");

				messageSendingObject.PeriodFrom = new ZDateTime(2025, 1, 5);
				messageSendingObject.PeriodTo = new ZDateTime(2025, 1, 4);
				AssertHasErrorContaining("When both Period From and Period To are filled in, there should be validation that Period From is before Period To", messageSendingObject.PeriodToInfo, "Period From must be older than Period To.");

				messageSendingObject.PeriodTo = new ZDateTime(2025, 1, 6);
				AssertNoErrors(messageSendingObject.PeriodToInfo);
			});
		}

		public void TestCheckRequesterRole()
		{
			TP5MessageSendingObjectLookupsTest.SetUpCusCodeList_CL156(Factory);
			var expectedMessage = "Role is a mandatory value";
			CombineAssertions(() =>
			{
				messageSendingObject.MessageType = TP5MessageTypeList.Codes.CC034C;
				messageSendingObject.RequesterRole = ZString.Empty;
				messageSendingObject.Validation.ValidateRequesterRole();
				AssertHasMessageErrorContaining("Role must not be empty when message is 034", messageSendingObject.RequesterRoleInfo, expectedMessage);
				messageSendingObject.RequesterRole = "Role1";
				AssertNoMessageErrorContaining("No message error when Role is not empty and message is 034", messageSendingObject.RequesterRoleInfo, expectedMessage);
				ValidationTestHelper.AssertInvalidCodeMessageError(messageSendingObject.RequesterRoleInfo, "XXXXX", "Role1");

				messageSendingObject.MessageType = TP5MessageTypeList.Codes.CC015C;
				messageSendingObject.RequesterRole = ZString.Empty;
				messageSendingObject.Validation.ValidateRequesterRole();
				AssertNoMessageErrorContaining("Role can be empty when message is not 034", messageSendingObject.RequesterRoleInfo, expectedMessage);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			messageSendingObject = new TP5MessageSendingObject(nctsHeader);
		}

		TP5MessageSendingObject messageSendingObject;
	}
}
