using System;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.ElectronicPayment.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;
using ProviderCodes = Enterprise.MasterFiles.Business.EPaymentProviderCodes.Codes;

namespace Enterprise.Accounting.ElectronicPayment.Testing.Common
{
	public class AccEPaymentBeneficiaryRequestToGEPConverterTest : TestCaseWithFactory
	{
		[TestDate(2021, 4, 9)]
		public void TestConvertBeneficiaryRequestToGEP_SBNMessageType_LastUpdatedDateIsIncludedInPayload()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company1 = objectCreator.CreateCompanyAndBranch("AUMEL");
			company1.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;
			var beneficiaryRequestCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(company1.PK);
			var staffToken = testHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.UtcNow.AddHours(2), company1.PK, beneficiaryRequestCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToken.TK_AccountName = "This name is provided by OFX.";
			Factory.Save();

			AccEPaymentBeneficiaryRequest firstRequest;
			using (Env.SetTemporaryUserContext(beneficiaryRequestCreatingUser.PK.ToGuid(), company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				firstRequest = objectCreator.CreateValidEPaymentBeneficiaryRequestForStatus(EPaymentStatusCodes.BeneficiaryRequest.Queued);
				Factory.Save();
			}

			var converter = new AccEPaymentBeneficiaryRequestToGEPConverter();
			var firstRequestPayload = GetPayloadForRequest(firstRequest);
			AssertContains("Last Updated Date should be empty for first request in a company.", @"""updatedDateFrom"":""""", firstRequestPayload);

			AccEPaymentBeneficiaryRequest secondRequest;
			using (Env.SetTemporaryUserContext(beneficiaryRequestCreatingUser.PK.ToGuid(), company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				secondRequest = objectCreator.CreateValidEPaymentBeneficiaryRequestForStatus(EPaymentStatusCodes.BeneficiaryRequest.Queued);
				Factory.Save();
			}

			var secondRequestPayload = GetPayloadForRequest(secondRequest);
			AssertContains("Last Updated Date should be empty for second request because first request is in QUE status.", @"""updatedDateFrom"":""""", secondRequestPayload);

			firstRequest.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Requested;
			Factory.Save();

			secondRequestPayload = GetPayloadForRequest(secondRequest);
			AssertContains("Last Updated Date should be empty for second request because first request is in REQ status.", @"""updatedDateFrom"":""""", secondRequestPayload);

			firstRequest.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Error;
			firstRequest.ABR_LastResponseReceivedUtc = ZDateTime.UtcNow;
			Factory.Save();

			secondRequestPayload = GetPayloadForRequest(secondRequest);
			AssertContains("Last Updated Date should be empty for second request because first request is in ERR status.", @"""updatedDateFrom"":""""", secondRequestPayload);

			firstRequest.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Partial;
			Factory.Save();

			secondRequestPayload = GetPayloadForRequest(secondRequest);
			AssertContains("Last Updated Date should be empty for second request because first request is in PAR status.", @"""updatedDateFrom"":""""", secondRequestPayload);

			firstRequest.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Received;
			Factory.Save();

			secondRequestPayload = GetPayloadForRequest(secondRequest);
			AssertContains("Last Updated Date should not be empty for second request because first request is in RCV status.", @"""updatedDateFrom"":""2021-04-09 00:00:00""", secondRequestPayload);

			string GetPayloadForRequest(AccEPaymentBeneficiaryRequest request)
			{
				var gepMessage = converter.ConvertBeneficiaryRequestToGEP(request);
				return Encoding.UTF8.GetString(Convert.FromBase64String(gepMessage.Payload));
			}
		}

		#region Beneficiary Request

		[TestDate(2021, 4, 9)]
		public void TestConvertBeneficiaryRequestToGEP_SBNMessageType_Successful()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company1 = objectCreator.CreateCompanyAndBranch("AUMEL");
			company1.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;
			var beneficiaryRequestCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(company1.PK);
			var staffToken = testHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.UtcNow.AddHours(2), company1.PK, beneficiaryRequestCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToken.TK_AccountName = "This name is provided by OFX.";
			Factory.Save();

			AccEPaymentBeneficiaryRequest request;
			using (Env.SetTemporaryUserContext(beneficiaryRequestCreatingUser.PK.ToGuid(), company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				request = objectCreator.CreateValidEPaymentBeneficiaryRequestForStatus(EPaymentStatusCodes.BeneficiaryRequest.Queued);
				Factory.Save();
			}

			var converter = new AccEPaymentBeneficiaryRequestToGEPConverter();
			var ePayment = converter.ConvertBeneficiaryRequestToGEP(request);

			AssertEquals("MessagingSystem", ProviderCodes.OFX, ePayment.Header.ElectronicPaymentRequest.MessagingSystem);
			AssertEquals("MessageType", GEPProviderAPICommandList.Codes.SearchBeneficiary, ePayment.Header.ElectronicPaymentRequest.MessageType);
			AssertEquals("CompanyCode", company1.GC_Code, ePayment.Header.ElectronicPaymentRequest.CompanyCode);
			AssertEquals("BranchCode", company1.FirstActiveBranch.GB_Code, ePayment.Header.ElectronicPaymentRequest.BranchCode);
			AssertEquals("UserPk", beneficiaryRequestCreatingUser.PK.ToString(), ePayment.Header.ElectronicPaymentRequest.UserPk);
			AssertEquals("UserCode", beneficiaryRequestCreatingUser.GS_Code, ePayment.Header.ElectronicPaymentRequest.UserCode);
			AssertEquals("UserAccountName", staffToken.TK_AccountName, ePayment.Header.ElectronicPaymentRequest.UserAccountName);
			AssertEquals("IsProductionSystem", false, ePayment.Header.ElectronicPaymentRequest.IsProductionSystem);

			var expectedPayload = @"{""requestReference"":""" + request.ABR_InternalReference + @""",""bankAccountCode"":""EPA"",""updatedDateFrom"":"""",""startPageNumber"":1,""maxNumberOfRecordInHttpResponse"":100,""maxNumberOfRecordInXUE"":500}";

			var payLoad = Encoding.UTF8.GetString(Convert.FromBase64String(ePayment.Payload));
			AssertEquals("JSON payload", expectedPayload, payLoad);
		}

		[TestDate(2021, 4, 9)]
		public void TestConvertBeneficiaryRequestToGEP_SBNMessageType_CreatingUserDoesNotHaveToken()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company1 = objectCreator.CreateCompanyAndBranch("AUMEL");
			company1.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;
			var beneficiaryRequestCreatingUser = objectCreator.CreateStaff("YOU");
			Factory.Save();

			AccEPaymentBeneficiaryRequest request;
			using (Env.SetTemporaryUserContext(beneficiaryRequestCreatingUser.PK.ToGuid(), company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var bankAccount = objectCreator.CreateEPaymentBankAccount();
				var staffToken = objectCreator.CreateEPaymentStaffToken(bankAccount.PK, DateTime.UtcNow.AddDays(-1), beneficiaryRequestCreatingUser.GS_Code, "ATH");
				request = objectCreator.CreateValidEPaymentBeneficiaryRequestForStatus(EPaymentStatusCodes.BeneficiaryRequest.Queued);
				Factory.Save();
			}

			AssertExceptionThrown<GEPMessageCreationException>("OFX User account information is missing.", () =>
			{
				var converter = new AccEPaymentBeneficiaryRequestToGEPConverter();
				converter.ConvertBeneficiaryRequestToGEP(request);
			});
		}

		[TestDate(2021, 4, 9)]
		public void TestConvertBeneficiaryRequestToGEP_SBNMessageType_CreatingUserIsNotAuthorized()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company1 = objectCreator.CreateCompanyAndBranch("AUMEL");
			company1.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;
			var beneficiaryRequestCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(company1.PK);
			var staffToken = testHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.Empty, company1.PK, beneficiaryRequestCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.NotAuthorised);
			staffToken.TK_AccountName = "This name is provided by OFX.";
			Factory.Save();

			AccEPaymentBeneficiaryRequest request;
			using (Env.SetTemporaryUserContext(beneficiaryRequestCreatingUser.PK.ToGuid(), company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				request = objectCreator.CreateValidEPaymentBeneficiaryRequestForStatus(EPaymentStatusCodes.BeneficiaryRequest.Queued);
				Factory.Save();
			}

			AssertExceptionThrown<GEPMessageCreationException>("OFX User account information is missing.", () =>
			{
				var converter = new AccEPaymentBeneficiaryRequestToGEPConverter();
				converter.ConvertBeneficiaryRequestToGEP(request);
			});
		}

		[TestDate(2021, 4, 9)]
		public void TestConvertBeneficiaryRequestToGEP_SBNMessageType_CreatingUserTokenIsExpired()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company1 = objectCreator.CreateCompanyAndBranch("AUMEL");
			company1.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;
			var beneficiaryRequestCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(company1.PK);
			var staffToken = testHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.UtcNow.AddMinutes(-20), company1.PK, beneficiaryRequestCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToken.TK_AccountName = "This name is provided by OFX.";
			Factory.Save();

			AccEPaymentBeneficiaryRequest request;
			using (Env.SetTemporaryUserContext(beneficiaryRequestCreatingUser.PK.ToGuid(), company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				request = objectCreator.CreateValidEPaymentBeneficiaryRequestForStatus(EPaymentStatusCodes.BeneficiaryRequest.Queued);
				Factory.Save();
			}

			AssertExceptionThrown<GEPMessageCreationException>("OFX User account information is missing.", () =>
			{
				var converter = new AccEPaymentBeneficiaryRequestToGEPConverter();
				converter.ConvertBeneficiaryRequestToGEP(request);
			});
		}

		[TestDate(2021, 4, 9)]
		public void TestConvertBeneficiaryRequestToGEP_SBNMessageType_CreatingUserTokenExpiryIsLessThanOneHour()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company1 = objectCreator.CreateCompanyAndBranch("AUMEL");
			company1.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;
			var beneficiaryRequestCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(company1.PK);
			var staffToken = testHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.UtcNow.AddMinutes(20), company1.PK, beneficiaryRequestCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToken.TK_AccountName = "This name is provided by OFX.";
			Factory.Save();

			AccEPaymentBeneficiaryRequest request;
			using (Env.SetTemporaryUserContext(beneficiaryRequestCreatingUser.PK.ToGuid(), company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				request = objectCreator.CreateValidEPaymentBeneficiaryRequestForStatus(EPaymentStatusCodes.BeneficiaryRequest.Queued);
				Factory.Save();
			}

			AssertExceptionThrown<GEPMessageCreationException>("OFX User account information is missing.", () =>
			{
				var converter = new AccEPaymentBeneficiaryRequestToGEPConverter();
				converter.ConvertBeneficiaryRequestToGEP(request);
			});
		}

		#endregion
	}
}
