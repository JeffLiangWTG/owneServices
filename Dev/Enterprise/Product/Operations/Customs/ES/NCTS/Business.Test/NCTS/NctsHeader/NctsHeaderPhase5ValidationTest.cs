using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NctsHeader = Enterprise.Customs.ES.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.ES.Business.Testing
{
	class NctsHeaderPhase5ValidationTest : BusinessObjectValidationTestCase
	{
		public void TestParent()
		{
			var parent = Factory.New<NctsHeader>();
			parent.SetMovementType(NctsMovementType.Codes.Departure);
			AssertEquals(parent.Validation.Parent, parent);
		}

		public void TestCheckBH_CustomsProfile_List()
		{
			var nctsHeader = SetUpBOWithCertificateData(out var broker, out var authStaff);

			CombineAssertions(() =>
			{
				nctsHeader.BH_CustomsProfile = "INVALID";
				AssertHasMessageErrorContaining(nctsHeader.BH_CustomsProfileInfo, ListValidation.InvalidCodeMessageError);

				nctsHeader.BH_CustomsProfile = "TestCert1";
				AssertNoMessageErrorContaining(nctsHeader.BH_CustomsProfileInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckBH_CustomsProfile_AuthorisedUser()
		{
			var notAuthorisedUserMessage = "You are not authorized to use this certificate. Please ask the Broker to authorize your user on the Staff & Resource module, Brokerage tab.";
			var nctsHeader = SetUpBOWithCertificateData(out var broker, out var authStaff);

			CombineAssertions(() =>
			{
				nctsHeader.BH_CustomsProfile = "TestCert1";
				AssertHasMessageErrorContaining("Default Current User is not authorised for cert1", nctsHeader.BH_CustomsProfileInfo, notAuthorisedUserMessage);

				nctsHeader.BH_CustomsProfile = "TestCert2";
				AssertHasMessageErrorContaining("Default Current User is not authorised for cert2", nctsHeader.BH_CustomsProfileInfo, notAuthorisedUserMessage);

				using (Env.SetTemporaryUserContext(new UserContext(authStaff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					nctsHeader.BH_CustomsProfile = "TestCert1";
					AssertNoNotifications("authStaff not authorised for cert1", nctsHeader.BH_CustomsProfileInfo);

					nctsHeader.BH_CustomsProfile = "TestCert2";
					AssertHasMessageErrorContaining("authStaff is not authorised for cert2", nctsHeader.BH_CustomsProfileInfo, notAuthorisedUserMessage);
				}

				using (Env.SetTemporaryUserContext(new UserContext(broker, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					nctsHeader.BH_CustomsProfile = "TestCert1";
					AssertNoNotifications("broker is cert1's owner so is authorised", nctsHeader.BH_CustomsProfileInfo);

					nctsHeader.BH_CustomsProfile = "TestCert2";
					AssertNoNotifications("broker is cert2's owner so is authorised", nctsHeader.BH_CustomsProfileInfo);
				}
			});
		}

		public void TestCheckBH_CustomsProfile_Mandatory()
		{
			var nctsHeader = SetUpBOWithCertificateData(out var broker, out var authStaff);

			CombineAssertions(() =>
			{
				nctsHeader.BH_CustomsProfile = "TestCert1";
				AssertNoMessageErrorContaining("Error is not shown cause Certificate is not empty", nctsHeader.BH_CustomsProfileInfo, MandatoryValidation.YouHaveNotEntered);

				nctsHeader.BH_CustomsProfile = ZString.Empty;
				AssertHasMessageErrorContaining("Error is shown cause Certificate is empty", nctsHeader.BH_CustomsProfileInfo, MandatoryValidation.YouHaveNotEntered);

				nctsHeader.MovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
				nctsHeader.Validation.ValidateBH_CustomsProfile();
				AssertNoMessageErrorContaining("No validation cause is TNN", nctsHeader.BH_CustomsProfileInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckTNNDocumentType()
		{
			var messageWarningTextT = "If TNN Document is 4, MRN number 18 character must be T and characters 3 to 10 must match with departure custom office.";
			var messageWarningTextJ = "If Non Security Data, MRN 17 character must be J.";
			var messageWarningTextK = "If Security Data, MRN 17 character must be K.";

			var nctsHeaderDeparture = Factory.New<NctsHeader>();
			nctsHeaderDeparture.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderDeparture.MovementReferenceEntryNumber.CE_EntryNum = "24ES0005550000000X12";
			nctsHeaderDeparture.MovementHeader.BM_Phase = DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration;

			nctsHeaderDeparture.MovementHeader.CustomsOffices.RemoveAndDeleteAll();
			var customsOffice = nctsHeaderDeparture.MovementHeader.CustomsOffices.AddNew();
			customsOffice.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
			customsOffice.CY_Data = "ES000555";
			nctsHeaderDeparture.TNNDocumentType = ESNCTS5ArrivalTNNTypeList.Codes.OtherThanTadOrWithNoMrn;

			CombineAssertions(() =>
			{
				AssertHasWarningContaining("BM_Phase TNN, Warning displayed when MRN pos 18 not T, Warning displayed", nctsHeaderDeparture.MovementReferenceNumberInfo, messageWarningTextT);

				nctsHeaderDeparture.MovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.DeclarationSent;
				nctsHeaderDeparture.Validation.ValidateAll();
				AssertNoWarningContaining("BM_Phase not TNN, no Warning displayed", nctsHeaderDeparture.MovementReferenceNumberInfo, messageWarningTextT);

				nctsHeaderDeparture.MovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
				nctsHeaderDeparture.TNNDocumentType = ESNCTS5ArrivalTNNTypeList.Codes.TransitAccompanyingDocumentTad;
				AssertNoWarningContaining("BM_Phase TNN and TNNDocumentType not 4, no Warning displayed", nctsHeaderDeparture.MovementReferenceNumberInfo, messageWarningTextT);

				nctsHeaderDeparture.MovementReferenceEntryNumber.CE_EntryNum = "24ES00055X0000000T12";
				nctsHeaderDeparture.TNNDocumentType = ESNCTS5ArrivalTNNTypeList.Codes.OtherThanTadOrWithNoMrn;
				AssertHasWarningContaining("BM_Phase TNN and TNNDocumentType = 4 but Office not equal to code from DES, Warning displayed", nctsHeaderDeparture.MovementReferenceNumberInfo, messageWarningTextT);

				nctsHeaderDeparture.MovementReferenceEntryNumber.CE_EntryNum = "24ES0005550000000T12";
				nctsHeaderDeparture.Validation.ValidateAll();
				AssertNoWarningContaining("BM_Phase TNN and TNNDocumentType = 4 and Office equal to code from DES, no Warning displayed", nctsHeaderDeparture.MovementReferenceNumberInfo, messageWarningTextT);

				nctsHeaderDeparture.MovementReferenceEntryNumber.CE_EntryNum = "24ES000555000000T12";
				nctsHeaderDeparture.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
				nctsHeaderDeparture.Validation.ValidateAll();
				AssertHasWarningContaining("BM_Phase TNN and TNNDocumentType = 4 and Security = NON and MRN pos 17 not J, Warning displayed", nctsHeaderDeparture.MovementReferenceNumberInfo, messageWarningTextJ);

				nctsHeaderDeparture.MovementReferenceEntryNumber.CE_EntryNum = "24ES000555000000J12";
				nctsHeaderDeparture.Validation.ValidateAll();
				AssertNoWarningContaining("BM_Phase TNN and TNNDocumentType = 4 and Security = NON and MRN pos 17 is J, no Warning displayed", nctsHeaderDeparture.MovementReferenceNumberInfo, messageWarningTextJ);

				nctsHeaderDeparture.MovementReferenceEntryNumber.CE_EntryNum = "24ES000555000000T12";
				nctsHeaderDeparture.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
				nctsHeaderDeparture.Validation.ValidateAll();
				AssertHasWarningContaining("BM_Phase TNN and TNNDocumentType = 4 and Security = EXI and MRN pos 17 not K, Warning displayed", nctsHeaderDeparture.MovementReferenceNumberInfo, messageWarningTextK);

				nctsHeaderDeparture.MovementReferenceEntryNumber.CE_EntryNum = "24ES000555000000K12";
				nctsHeaderDeparture.Validation.ValidateAll();
				AssertNoWarningContaining("BM_Phase TNN and TNNDocumentType = 4 and Security = EXI and MRN pos 17 is K, no Warning displayed", nctsHeaderDeparture.MovementReferenceNumberInfo, messageWarningTextK);

				nctsHeaderDeparture.MovementReferenceEntryNumber.CE_EntryNum = ZString.Empty;
				nctsHeaderDeparture.Validation.ValidateAll();
				AssertHasWarningContaining("BM_Phase TNN and TNNDocumentType = 4 and Office equal to code from DES but MRN empty, Warning displayed", nctsHeaderDeparture.MovementReferenceNumberInfo, messageWarningTextT);

				nctsHeaderDeparture.MovementReferenceEntryNumber.CE_EntryNum = "24ES0005550000000T12";
				customsOffice.Delete();
				nctsHeaderDeparture.Validation.ValidateAll();
				AssertHasWarningContaining("BM_Phase TNN and TNNDocumentType = 4 and MRN empty but Office empty, Warning displayed", nctsHeaderDeparture.MovementReferenceNumberInfo, messageWarningTextT);
			});
		}

		public void TestCheckBH_RL_NKImportLoadPort_MustBeFiled()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.BH_RL_NKImportLoadPort = ZString.Empty;
			header.Validation.ValidateBH_RL_NKImportLoadPort();
			AssertNoMessageErrors("Validation not apply in ES", header.BH_RL_NKImportLoadPortInfo);
		}

		NctsHeader SetUpBOWithCertificateData(out GlbStaff broker, out GlbStaff authStaff)
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

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var departureMovement = header.MovementHeader;
			departureMovement.BM_GS_NKCusAgent = broker.GS_Code;

			return header;
		}
	}
}
