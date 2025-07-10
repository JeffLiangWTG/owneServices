using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Organisation;
using Enterprise.Customs.GB.CDS.Organisation;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(OrgHeaderCheckerSendingManager))]
	sealed class OrgHeaderCheckerSendingManagerTest : TestCaseWithFactory
	{
		public void TestConstructor_Null()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new OrgHeaderCheckerSendingManager(null));
		}

		public void TestSend_WhenOrgCusCodesAreMissing()
		{
			var (orgHeader, _, _) = SetupOrgHeader();
			var checker = new NonPersistentOrgHeaderChecker(Factory, orgHeader);
			checker.VerifyVAT = true;
			checker.VerifyEORI = true;

			AssertEquals("Pre-Condition VAT", string.Empty, orgHeader.GetVATRegistrationNumber(Core.Constants.CountryCodes.UnitedKingdom));
			AssertEquals("Pre-Condition EORI", string.Empty, orgHeader.GetEuIdentificationNumber(Core.Constants.CountryCodes.UnitedKingdom));

			var sendingManager = new OrgHeaderCheckerSendingManager(checker);
			sendingManager.Send();

			var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, orgHeader.PK).AddToFilter(EDIMessageSchema.EM_LinkTable, OrgHeader.Schema.TableName)).ToArray();
			AssertEquals(0, messages.Length);
		}

		public void TestSend_WhenNothingIsSelected()
		{
			var (orgHeader, _, _) = SetupOrgHeader("54321", "GB12345");
			var checker = new NonPersistentOrgHeaderChecker(Factory, orgHeader);
			checker.VerifyVAT = false;
			checker.VerifyEORI = false;

			AssertEquals("Pre-Condition VAT", "54321", orgHeader.GetVATRegistrationNumber(Core.Constants.CountryCodes.UnitedKingdom));
			AssertEquals("Pre-Condition EORI", "GB12345", orgHeader.GetEuIdentificationNumber(Core.Constants.CountryCodes.UnitedKingdom));

			var sendingManager = new OrgHeaderCheckerSendingManager(checker);
			sendingManager.Send();

			var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, orgHeader.PK).AddToFilter(EDIMessageSchema.EM_LinkTable, OrgHeader.Schema.TableName)).ToArray();
			AssertEquals(0, messages.Length);
		}

		public void TestSend_ForVAT()
		{
			using (Factory.AddDisposableService())
			{
				var (orgHeader, vatOrgCusCode, _) = SetupOrgHeader("54321", "GB12345");
				var checker = new NonPersistentOrgHeaderChecker(Factory, orgHeader);
				checker.VerifyVAT = true;
				checker.VerifyEORI = false;
				var sendingManager = new OrgHeaderCheckerSendingManager(checker);
				sendingManager.Send();

				var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, orgHeader.PK).AddToFilter(EDIMessageSchema.EM_LinkTable, OrgHeader.Schema.TableName)).ToArray();
				AssertEquals(1, messages.Length);
				Assert("VAT", vatOrgCusCode, messages[0]);
			}
		}

		public void TestSend_ForEORI()
		{
			using (Factory.AddDisposableService())
			{
				var (orgHeader, _, eoriOrgCusCode) = SetupOrgHeader("54321", "GB12345");
				var checker = new NonPersistentOrgHeaderChecker(Factory, orgHeader);
				checker.VerifyVAT = false;
				checker.VerifyEORI = true;
				var sendingManager = new OrgHeaderCheckerSendingManager(checker);
				sendingManager.Send();

				var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, orgHeader.PK).AddToFilter(EDIMessageSchema.EM_LinkTable, OrgHeader.Schema.TableName)).ToArray();
				AssertEquals(1, messages.Length);
				Assert("EORI", eoriOrgCusCode, messages[0]);
			}
		}

		public void TestSend_ForEORI_NOP()
		{
			using (Factory.AddDisposableService())
			{
				var (orgHeader, _, eoriOrgCusCode) = SetupOrgHeader("54321", "XI12345");
				var checker = new NonPersistentOrgHeaderChecker(Factory, orgHeader);
				checker.VerifyVAT = false;
				checker.VerifyEORI = true;
				var sendingManager = new OrgHeaderCheckerSendingManager(checker);
				sendingManager.Send();

				var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, orgHeader.PK).AddToFilter(EDIMessageSchema.EM_LinkTable, OrgHeader.Schema.TableName)).ToArray();
				AssertEquals(1, messages.Length);
				Assert("EORI", eoriOrgCusCode, messages[0]);
			}
		}

		public void TestSend_ForEORIAndVAT()
		{
			using (Factory.AddDisposableService())
			{
				var (orgHeader, vatOrgCusCode, eoriOrgCusCode) = SetupOrgHeader("54321", "GB12345");
				var checker = new NonPersistentOrgHeaderChecker(Factory, orgHeader);
				checker.VerifyVAT = true;
				checker.VerifyEORI = true;
				var sendingManager = new OrgHeaderCheckerSendingManager(checker);
				sendingManager.Send();

				var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, orgHeader.PK).AddToFilter(EDIMessageSchema.EM_LinkTable, OrgHeader.Schema.TableName)).ToArray();
				AssertEquals(2, messages.Length);

				var vatMessage = messages.SingleOrDefault(m => m.EM_MessageText.Contains("<Value>VAT</Value>"));
				Assert("VAT", vatOrgCusCode, vatMessage);

				var eoriMessage = messages.SingleOrDefault(m => m.EM_MessageText.Contains("<Value>EORI</Value>"));
				Assert("EORI", eoriOrgCusCode, eoriMessage);
			}
		}

		(OrgHeader OrgHeader, OrgCusCode VATOrgCusCode, OrgCusCode EORIOrgCusCode) SetupOrgHeader(string vat = null, string eori = null)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ABC123";

			OrgCusCode vatOrgCusCode = null;

			if (vat != null)
			{
				vatOrgCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, vat, Core.Constants.CountryCodes.UnitedKingdom);
			}

			OrgCusCode eoriOrgCusCode = null;

			if (eori != null)
			{
				eoriOrgCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eori, Core.Constants.CountryCodes.UnitedKingdom);
			}

			Factory.Save();

			return (orgHeader, vatOrgCusCode, eoriOrgCusCode);
		}

		void Assert(string messagePrefix, OrgCusCode orgCusCode, EDIMessage message)
		{
			AssertNotNull($"{messagePrefix} Message", message);
			AssertEquals($"{messagePrefix} Message EM_ApplicationReference", orgCusCode.PK.ToString(), message.EM_ApplicationReference);
			AssertEquals($"{messagePrefix} OrgCusCodeValidity VerificationStatus", OrgConstants.CusCodeValidityVerification.NotVerified, orgCusCode.OrgCusCodeValidity.VerificationStatus);
		}
	}
}
