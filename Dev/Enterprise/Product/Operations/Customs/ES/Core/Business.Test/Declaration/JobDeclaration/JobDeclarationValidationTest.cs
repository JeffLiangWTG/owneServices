using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	public class JobDeclarationValidationTest : Customs.Business.Testing.BaseJobDeclarationValidationTest<JobDeclaration>
	{
		public void TestCheckJE_DeclarantType()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_DeclarantType = "AH";
				AssertNoMessageErrorContaining("No error for no empty", declaration.JE_DeclarantTypeInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_DeclarantType = ZString.Empty;
				AssertHasMessageErrorContaining("Error for 0 entry instructions", declaration.JE_DeclarantTypeInfo, MandatoryValidation.YouHaveNotEntered);

				var instruction = declaration.CustomsEntryInstructions.AddNew();
				instruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				declaration.Validation.ValidateJE_DeclarantType();
				AssertHasMessageErrorContaining("Error for not T2C entry instructions", declaration.JE_DeclarantTypeInfo, MandatoryValidation.YouHaveNotEntered);

				instruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				declaration.Validation.ValidateJE_DeclarantType();
				AssertNoMessageErrorContaining("No error for T2C entry instructions", declaration.JE_DeclarantTypeInfo, MandatoryValidation.YouHaveNotEntered);

				var instruction2 = declaration.CustomsEntryInstructions.AddNew();
				instruction2.CEI_SubStyle = EntrySubStyleList.Codes.A;
				declaration.Validation.ValidateJE_DeclarantType();
				AssertHasMessageErrorContaining("Error for not all entry instructions equals T2C", declaration.JE_DeclarantTypeInfo, MandatoryValidation.YouHaveNotEntered);

				instruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				declaration.Validation.ValidateJE_DeclarantType();
				AssertNoMessageErrorContaining("No error for all entry instructions equals T2C", declaration.JE_DeclarantTypeInfo, MandatoryValidation.YouHaveNotEntered);

				instruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				declaration.Validation.ValidateJE_DeclarantType();
				AssertNoMessageErrorContaining("No error for all entry instructions equals T2C or T2L", declaration.JE_DeclarantTypeInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckCEH_CustomsProfile_WithNIF_ForEXS()
		{
			var declaration = SetUpBOWithCertificateData(out var broker, out var authStaff);
			var message = "The selected certificate has not NIF Certificate value so declaration will be rejected.";
			CombineAssertions(() =>
			{
				var dec = GetDeclarationForTesting(declaration, GetDeclarationTypeForTesting.Export);
				dec.JE_CustomsProfile = "TestCert1";
				AssertNoWarningContaining("For entry Export with NIF", dec.JE_CustomsProfileInfo, message);
				dec.JE_CustomsProfile = "TestCert2";
				AssertNoWarningContaining("For entry Export without NIF", dec.JE_CustomsProfileInfo, message);

				dec = GetDeclarationForTesting(declaration, GetDeclarationTypeForTesting.EXS);
				dec.JE_CustomsProfile = "TestCert1";
				AssertNoWarningContaining("For entry EXS with NIF", dec.JE_CustomsProfileInfo, message);
				dec.JE_CustomsProfile = "TestCert2";
				AssertHasWarningContaining("For entry EXS without NIF", dec.JE_CustomsProfileInfo, message);
			});
		}

		public void TestCheckJE_PaymentMethodLogicForEU()
		{
			var jobDeclaration = SetUpBOWithCertificateData(out var broker, out var authStaff);
			CombineAssertions(() =>
			{
				foreach (var paymentMethod in new[]
				{
					DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14,
					DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority,
					DefermentMethodList.Codes.ConsigneesAccountStandingAuthority,
					DefermentMethodList.Codes.ConsigneesAccountConsigneeCompletingTheDeclaration
				})
				{
					jobDeclaration.JE_PaymentMethod = paymentMethod;
					AssertEquals($"Payment method {paymentMethod}", false, jobDeclaration.JE_PaymentMethodInfo.Notifications.Any(x => x.Message.Contains("requires a Deferment Approval Number (DAN) for the relevant country/region")));
				}
			});
		}

		public void TestCheckCEH_CustomsProfile_List()
		{
			var declaration = SetUpBOWithCertificateData(out var broker, out var authStaff);

			CombineAssertions(() =>
			{
				declaration.JE_CustomsProfile = "INVALID";
				AssertHasMessageErrorContaining(declaration.JE_CustomsProfileInfo, ListValidation.InvalidCodeMessageError);

				declaration.JE_CustomsProfile = "TestCert1";
				AssertNoMessageErrorContaining(declaration.JE_CustomsProfileInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckCEH_CustomsProfile_AuthorisedUser()
		{
			var notAuthorisedUserMessage = "You are not authorized to use this certificate. Please ask the Broker to authorize your user on the Staff & Resource module, Brokerage tab.";
			var declaration = SetUpBOWithCertificateData(out var broker, out var authStaff);

			var bpUser = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, "~BP");
			var adUser = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, "~AD");

			CombineAssertions(() =>
			{
				declaration.JE_CustomsProfile = "TestCert1";
				AssertHasMessageErrorContaining("Default Current User is not authorised for cert1", declaration.JE_CustomsProfileInfo, notAuthorisedUserMessage);

				declaration.JE_CustomsProfile = "TestCert2";
				AssertHasMessageErrorContaining("Default Current User is not authorised for cert2", declaration.JE_CustomsProfileInfo, notAuthorisedUserMessage);

				using (Env.SetTemporaryUserContext(new UserContext(authStaff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					declaration.JE_CustomsProfile = "TestCert1";
					AssertNoNotifications("authStaff not authorised for cert1", declaration.JE_CustomsProfileInfo);

					declaration.JE_CustomsProfile = "TestCert2";
					AssertHasMessageErrorContaining("authStaff is not authorised for cert2", declaration.JE_CustomsProfileInfo, notAuthorisedUserMessage);
				}

				using (Env.SetTemporaryUserContext(new UserContext(broker, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					declaration.JE_CustomsProfile = "TestCert1";
					AssertNoNotifications("broker is cert1's owner so is authorised", declaration.JE_CustomsProfileInfo);

					declaration.JE_CustomsProfile = "TestCert2";
					AssertNoNotifications("broker is cert2's owner so is authorised", declaration.JE_CustomsProfileInfo);
				}

				using (Env.SetTemporaryUserContext(new UserContext(bpUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					declaration.JE_CustomsProfile = "TestCert1";
					AssertNoNotifications("bpUser is not configured as authorised for cert1 but his gs_code is ~BP so is authorized.", declaration.JE_CustomsProfileInfo);

					declaration.JE_CustomsProfile = "TestCert2";
					AssertNoNotifications("bpUser is not configured as authorised for cert1 but his gs_code is ~BP so is authorized.", declaration.JE_CustomsProfileInfo);
				}

				using (Env.SetTemporaryUserContext(new UserContext(adUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					declaration.JE_CustomsProfile = "TestCert1";
					AssertNoNotifications("adUser is not configured as authorised for cert1 but his gs_code is ~AD so is authorized.", declaration.JE_CustomsProfileInfo);

					declaration.JE_CustomsProfile = "TestCert2";
					AssertNoNotifications("adUser is not configured as authorised for cert1 but his gs_code is ~AD so is authorized.", declaration.JE_CustomsProfileInfo);
				}
			});
		}

		public void TestCheckCEH_CustomsProfile_Mandatory()
		{
			var declaration = SetUpBOWithCertificateData(out var broker, out var authStaff);

			CombineAssertions(() =>
			{
				declaration.JE_CustomsProfile = "TestCert1";
				AssertNoMessageErrorContaining(declaration.JE_CustomsProfileInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_CustomsProfile = ZString.Empty;
				AssertHasMessageErrorContaining(declaration.JE_CustomsProfileInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		JobDeclaration SetUpBOWithCertificateData(out GlbStaff broker, out GlbStaff authStaff)
		{
			broker = Factory.New<GlbStaff>();
			broker.GS_Code = "AH";
			broker.GS_LoginName = "ahtest";
			broker.StaffPlainTextPassword = "security123";

			var wrapper = GlbStaffWrapper.Get(broker);
			var cert1 = wrapper.ESBPasswordCollection.AddNew();
			cert1.GP_Name = "TestCert1";
			cert1.GP_MailBoxID = "Test";
			cert1.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert1.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

			var cert2 = wrapper.ESBPasswordCollection.AddNew();
			cert2.GP_Name = "TestCert2";
			cert2.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert2.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

			authStaff = Factory.New<GlbStaff>();
			authStaff.GS_Code = "AZ";
			authStaff.GS_LoginName = "aztest";

			var authorisation = Factory.New<GlbExternalPasswordAuthorisation>();
			authorisation.GEA_GP = cert1.PK;
			authorisation.GEA_GS_AuthorisedStaff = authStaff.PK;

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GS_NKCusAgent = broker.GS_Code;

			return declaration;
		}

		enum GetDeclarationTypeForTesting { Export, EXS }

		JobDeclaration GetDeclarationForTesting(JobDeclaration dec, GetDeclarationTypeForTesting declarationType)
		{
			var entryInstruction = dec.CustomsEntryInstructions.AddNew();
			switch (declarationType)
			{
				case GetDeclarationTypeForTesting.Export:
					dec.JE_MessageType = MessageTypeList.Codes.Export;
					entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
					break;
				case GetDeclarationTypeForTesting.EXS:
					dec.JE_MessageType = MessageTypeList.Codes.Export;
					entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
					break;
				default:
					break;
			}
			return dec;
		}
	}
}
