using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using PNTSMessageTypeList = Enterprise.Customs.EU.Business.PNTSMessageTypeList;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing;

sealed class TemporaryStorageHeaderValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckAMA_OA_Declarant()
	{
		var tempHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		tempHeader.AMA_OA_Declarant = ZGuid.Empty;
		tempHeader.Validation.ValidateAll();
		AssertHasMessageError("Declarant is mandatory.", tempHeader.AMA_OA_DeclarantInfo, "You have not entered a Declarant.");

		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgHeader.CustomsCodes.RemoveAll();
		tempHeader.AMA_OA_Representative = orgAddress.PK;
		tempHeader.AMA_OA_Declarant = orgAddress.PK;
		AssertHasMessageError("An EORI # should exist for Declarant.", tempHeader.AMA_OA_DeclarantInfo, "An EORI # should exist for Declarant.");
		AssertHasMessageError(tempHeader.AMA_OA_DeclarantInfo, "Declarant must not be the same as the Representative.");

		var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
		var orgAddressWithEORI = Factory.NewWithValidTestData<OrgAddress>();
		orgAddressWithEORI.OA_OH = orgHeader2.PK;
		var orgCusCode = orgHeader2.CustomsCodes.AddNew();
		orgCusCode.OK_CodeType = "EOR";
		orgCusCode.OK_CustomsRegNo = "ES0001";
		tempHeader.AMA_OA_Declarant = orgAddressWithEORI.PK;
		AssertNoErrors(tempHeader.AMA_OA_DeclarantInfo);

		orgCusCode.OK_CodeType = "PAS";
		tempHeader.Validation.ValidateAMA_OA_Declarant();
		AssertNoErrors(tempHeader.AMA_OA_DeclarantInfo);
	}

	public void TestCheckAMA_OA_Carrier()
	{
		var tempHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		tempHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PresentationNotification;
		tempHeader.Validation.ValidateAll();
		AssertNoNotifications("When message mode is not TS, Carrier is mandatory.", tempHeader.AMA_OA_CarrierInfo);

		tempHeader.AMA_MessageType = PNTSMessageTypeList.Codes.CombinedTemporaryStorage;
		tempHeader.Validation.ValidateAMA_OA_Carrier();
		AssertNoNotifications("Removed error when message mode is not TS, Carrier is mandatory.", tempHeader.AMA_OA_CarrierInfo);

		tempHeader.AMA_MessageType = ZString.Empty;
		tempHeader.Validation.ValidateAMA_OA_Carrier();
		AssertNoNotifications("Removed error When message mode is not TS, Carrier is mandatory.", tempHeader.AMA_OA_CarrierInfo);

		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgHeader.CustomsCodes.RemoveAll();
		tempHeader.AMA_OA_Carrier = orgAddress.PK;
		tempHeader.Validation.ValidateAMA_OA_Declarant();
		AssertNoNotifications("Removed error: An EORI # should exist for Carrier.", tempHeader.AMA_OA_CarrierInfo);
	}

	public void TestCheckAMA_CustomsProfile_List()
	{
		var header = SetUpBOWithCertificateData(out var broker, out var authStaff);

		CombineAssertions(() =>
		{
			header.AMA_CustomsProfile = "INVALID";
			AssertHasMessageErrorContaining(header.AMA_CustomsProfileInfo, ListValidation.InvalidCodeMessageError);

			header.AMA_CustomsProfile = "TestCert1";
			AssertNoMessageErrorContaining(header.AMA_CustomsProfileInfo, ListValidation.InvalidCodeMessageError);
		});
	}

	public void TestCheckAMA_CustomsProfile_AuthorisedUser()
	{
		var notAuthorisedUserMessage = "You are not authorized to use this certificate. Please ask the Broker to authorize your user on the Staff & Resource module, Brokerage tab.";
		var header = SetUpBOWithCertificateData(out var broker, out var authStaff);

		CombineAssertions(() =>
		{
			header.AMA_CustomsProfile = "TestCert1";
			AssertHasMessageErrorContaining("Default Current User is not authorised for cert1", header.AMA_CustomsProfileInfo, notAuthorisedUserMessage);

			header.AMA_CustomsProfile = "TestCert2";
			AssertHasMessageErrorContaining("Default Current User is not authorised for cert2", header.AMA_CustomsProfileInfo, notAuthorisedUserMessage);

			using (Env.SetTemporaryUserContext(new UserContext(authStaff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				header.AMA_CustomsProfile = "TestCert1";
				AssertNoNotifications("authStaff not authorised for cert1", header.AMA_CustomsProfileInfo);

				header.AMA_CustomsProfile = "TestCert2";
				AssertHasMessageErrorContaining("authStaff is not authorised for cert2", header.AMA_CustomsProfileInfo, notAuthorisedUserMessage);
			}

			using (Env.SetTemporaryUserContext(new UserContext(broker, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				header.AMA_CustomsProfile = "TestCert1";
				AssertNoNotifications("broker is cert1's owner so is authorised", header.AMA_CustomsProfileInfo);

				header.AMA_CustomsProfile = "TestCert2";
				AssertNoNotifications("broker is cert2's owner so is authorised", header.AMA_CustomsProfileInfo);
			}
		});
	}

	public void TestCheckAMA_CustomsProfile_Mandatory()
	{
		var header = SetUpBOWithCertificateData(out var broker, out var authStaff);

		CombineAssertions(() =>
		{
			header.AMA_CustomsProfile = "TestCert1";
			AssertNoMessageErrorContaining("Error is not shown cause Certificate is not empty", header.AMA_CustomsProfileInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_CustomsProfile = ZString.Empty;
			AssertHasMessageErrorContaining("Error is shown cause Certificate is empty", header.AMA_CustomsProfileInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckAMA_TransportMode_Empty()
	{
		var header = Factory.New<TemporaryStorageHeader>();

		CombineAssertions(() =>
		{
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			header.AMA_TransportMode = "AIR";
			AssertNoMessageErrorContaining("Error is not shown when Transport Mode is not empty", header.AMA_TransportModeInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_TransportMode = ZString.Empty;
			AssertHasMessageErrorContaining("Error is shown when Transport Mode is empty and messagetype is G5X", header.AMA_TransportModeInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			header.Validation.ValidateAMA_TransportMode();
			AssertNoMessageErrorContaining("Error is not shown when Transport Mode is empty but messagetype is not G5X or G5P (LAM)", header.AMA_TransportModeInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
			header.Validation.ValidateAMA_TransportMode();
			AssertHasMessageErrorContaining("Error is shown when Transport Mode is empty and messagetype is G5P", header.AMA_TransportModeInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			header.Validation.ValidateAMA_TransportMode();
			AssertNoMessageErrorContaining("Error is not shown when Transport Mode is empty but messagetype is not G5X or G5P (TSM)", header.AMA_TransportModeInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckTransportType_Empty()
	{
		var header = Factory.New<TemporaryStorageHeader>();

		CombineAssertions(() =>
		{
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			header.TransportType = "41";
			AssertNoMessageErrorContaining("Error is not shown when Transport Type is not empty", header.AMA_TransportModeInfo, MandatoryValidation.YouHaveNotEntered);

			header.TransportType = ZString.Empty;
			header.Validation.ValidateTransportType();
			AssertHasMessageErrorContaining("Error is shown when Transport Type is empty and messagetype is G5X", header.TransportTypeInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			header.Validation.ValidateTransportType();
			AssertNoMessageErrorContaining("Error is not shown when Transport Type is empty but messagetype is not G5X or G5P (LAM)", header.TransportTypeInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
			header.Validation.ValidateTransportType();
			AssertHasMessageErrorContaining("Error is shown when Transport Type is empty and messagetype is G5P", header.TransportTypeInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			header.Validation.ValidateTransportType();
			AssertNoMessageErrorContaining("Error is not shown when Transport Type is empty but messagetype is not G5X or G5P (TSM)", header.TransportTypeInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckDestinationCustomsOffice_Empty()
	{
		var header = Factory.New<TemporaryStorageHeader>();

		CombineAssertions(() =>
		{
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			header.DestinationCustomsOffice = "ESD28600";
			AssertNoMessageErrorContaining("Error is not shown when Destination Customs Office is not empty", header.DestinationCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);

			header.DestinationCustomsOffice = ZString.Empty;
			AssertHasMessageErrorContaining("Error is shown when Destination Customs Office is empty and messagetype is G5X", header.DestinationCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			header.Validation.ValidateDestinationCustomsOffice();
			AssertNoMessageErrorContaining("Error is not shown when Destination Customs Office is empty but messagetype is not G5X or G5P (LAM)", header.DestinationCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
			header.Validation.ValidateDestinationCustomsOffice();
			AssertHasMessageErrorContaining("Error is shown when Destination Customs Office is empty and messagetype is G5P", header.DestinationCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			header.Validation.ValidateDestinationCustomsOffice();
			AssertNoMessageErrorContaining("Error is not shown when Destination Customs Office is empty but messagetype is not G5X or G5P (TSM)", header.DestinationCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckDestinationGoodsLocationDescription_Empty()
	{
		var header = Factory.New<TemporaryStorageHeader>();

		CombineAssertions(() =>
		{
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			header.DestinationGoodsLocation.CGL_Qualifier = "U";
			header.DestinationGoodsLocation.CGL_Type = "A";
			AssertNoMessageErrorContaining("Error is not shown when Destination Goods Location is not empty", header.DestinationGoodsLocationDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			header.DestinationGoodsLocation.CGL_Qualifier = ZString.Empty;
			header.DestinationGoodsLocation.CGL_Type = ZString.Empty;
			header.Validation.ValidateDestinationGoodsLocationDescription();
			AssertHasMessageErrorContaining("Error is shown when Destination Goods Location is empty and messagetype is G5X", header.DestinationGoodsLocationDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			header.Validation.ValidateDestinationGoodsLocationDescription();
			AssertHasMessageErrorContaining("Error is shown when Destination Goods Location is empty and messagetype is LAM", header.DestinationGoodsLocationDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
			header.Validation.ValidateDestinationGoodsLocationDescription();
			AssertHasMessageErrorContaining("Error is shown when Destination Goods Location is empty and messagetype is G5P", header.DestinationGoodsLocationDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			header.Validation.ValidateDestinationGoodsLocationDescription();
			AssertHasMessageErrorContaining("Error is shown when Destination Goods Location is empty and messagetype is TSM", header.DestinationGoodsLocationDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_MessageType = "AAA";
			header.Validation.ValidateDestinationGoodsLocationDescription();
			AssertNoMessageErrorContaining("Error is not shown when messagetype is AAA", header.DestinationGoodsLocationDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckAMA_MessageType_EmptyPacks()
	{
		var messageError = "You have not entered a Pack.";

		var header = Factory.New<TemporaryStorageHeader>();
		header.Bills.RemoveAndDeleteAll();
		var bill = header.Bills.AddNew();

		CombineAssertions(() =>
		{
			_ = bill.Packs.AddNew();
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			AssertNoMessageErrorContaining("Error is not shown when Packs is not empty", header.AMA_MessageTypeInfo, messageError);

			bill.Packs.RemoveAndDeleteAll();
			header.Validation.ValidateAMA_MessageType();
			AssertHasMessageErrorContaining("Error is shown when Packs is empty and messagetype is G5X", header.AMA_MessageTypeInfo, messageError);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			AssertHasMessageErrorContaining("Error is shown when Packs is empty and messagetype is LAM", header.AMA_MessageTypeInfo, messageError);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
			AssertHasMessageErrorContaining("Error is shown when Packs is empty and messagetype is G5P", header.AMA_MessageTypeInfo, messageError);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			AssertHasMessageErrorContaining("Error is shown when Packs is empty and messagetype is TSM", header.AMA_MessageTypeInfo, messageError);

			header.AMA_MessageType = "AAA";
			AssertNoMessageErrorContaining("Error is not shown when messagetype is AAA", header.AMA_MessageTypeInfo, messageError);
		});
	}

	public void TestCheckAMA_MessageType_EmptyPackedItems()
	{
		var messageError = "You have not entered an Item.";

		var header = Factory.New<TemporaryStorageHeader>();
		header.Bills.RemoveAndDeleteAll();
		var bill = header.Bills.AddNew();

		CombineAssertions(() =>
		{
			_ = bill.PackedItems.AddNew();
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			AssertNoMessageErrorContaining("Error is not shown when PackedItems is not empty", header.AMA_MessageTypeInfo, messageError);

			bill.PackedItems.RemoveAndDeleteAll();
			header.Validation.ValidateAMA_MessageType();
			AssertHasMessageErrorContaining("Error is shown when PackedItems is empty and messagetype is G5X", header.AMA_MessageTypeInfo, messageError);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			AssertHasMessageErrorContaining("Error is shown when PackedItems is empty and messagetype is LAM", header.AMA_MessageTypeInfo, messageError);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
			AssertHasMessageErrorContaining("Error is shown when PackedItems is empty and messagetype is G5P", header.AMA_MessageTypeInfo, messageError);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			AssertHasMessageErrorContaining("Error is shown when PackedItems is empty and messagetype is TSM", header.AMA_MessageTypeInfo, messageError);

			header.AMA_MessageType = "AAA";
			AssertNoMessageErrorContaining("Error is not shown when messagetype is AAA", header.AMA_MessageTypeInfo, messageError);
		});
	}

	public void TestCheckAMA_MessageType_EmptyPreviousDocs()
	{
		var messageError = "You have not entered a Previous Document.";

		var header = Factory.New<TemporaryStorageHeader>();
		header.Bills.RemoveAndDeleteAll();
		var bill = header.Bills.AddNew();

		CombineAssertions(() =>
		{
			_ = bill.PreviousDocuments.AddNew();
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			AssertNoMessageErrorContaining("Error is not shown when Bill PreviousDocuments is not empty", header.AMA_MessageTypeInfo, messageError);

			bill.PreviousDocuments.RemoveAndDeleteAll();
			header.Validation.ValidateAMA_MessageType();
			AssertHasMessageErrorContaining("Error is shown when Bill PreviousDocuments is empty and messagetype is G5X", header.AMA_MessageTypeInfo, messageError);

			var item1 = bill.PackedItems.AddNew();
			_ = item1.PreviousDocuments.AddNew();
			var item2 = bill.PackedItems.AddNew();
			_ = item2.PreviousDocuments.AddNew();
			header.Validation.ValidateAMA_MessageType();
			AssertNoMessageErrorContaining("Error is not shown when Bill PreviousDocuments is empty but all PakcedItems PreviousDocuments are not empty", header.AMA_MessageTypeInfo, messageError);

			item2.PreviousDocuments.RemoveAndDeleteAll();
			header.Validation.ValidateAMA_MessageType();
			AssertHasMessageErrorContaining("Error is shown when Bill PreviousDocuments is empty, at least one of PackedItems PreviousDocuments is empty and messagetype is G5X", header.AMA_MessageTypeInfo, messageError);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			AssertNoMessageErrorContaining("Error is not shown when Bill PreviousDocuments is empty, at least one of PackedItems PreviousDocuments is empty but messagetype is not G5X or G5P (LAM)", header.AMA_MessageTypeInfo, messageError);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
			AssertHasMessageErrorContaining("Error is shown when Bill PreviousDocuments is empty, at least one of PackedItems PreviousDocuments is empty and messagetype is G5P", header.AMA_MessageTypeInfo, messageError);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			AssertNoMessageErrorContaining("Error is not shown when Bill PreviousDocuments is empty, at least one of PackedItems PreviousDocuments is empty but messagetype is not G5X or G5P (TSM)", header.AMA_MessageTypeInfo, messageError);
		});
	}

	public void TestCheckDsdtMrnNumber()
	{
		CombineAssertions(() =>
		{
			var messageError = "You have not entered a DSDT Number. Please enter DSDT (SD Format) in the maritime format.";

			var header = Factory.New<TemporaryStorageHeader>();
			header.DsdtMrnNumber = ZString.Empty;

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			header.Validation.ValidateDsdtMrnNumber();
			AssertNoMessageErrorContaining("Error is not shown when DsdtMrnNumber is empty for G5X", header.DsdtMrnNumberInfo, messageError);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			header.Validation.ValidateDsdtMrnNumber();
			AssertNoMessageErrorContaining("Error is not shown when DsdtMrnNumber is empty for LAM", header.DsdtMrnNumberInfo, messageError);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
			header.Validation.ValidateDsdtMrnNumber();
			AssertNoMessageErrorContaining("Error is not shown when DsdtMrnNumber is empty for G5P", header.DsdtMrnNumberInfo, messageError);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			header.Validation.ValidateDsdtMrnNumber();
			AssertHasMessageErrorContaining("Error is shown when DsdtMrnNumber is empty for TSM", header.DsdtMrnNumberInfo, messageError);

			header.UnionGoods = true;
			header.Validation.ValidateDsdtMrnNumber();
			AssertNoMessageErrorContaining("Error is not shown when DsdtMrnNumber is empty for TSM but is Union Goods true", header.DsdtMrnNumberInfo, messageError);

			header.UnionGoods = false;
			header.DsdtMrnNumber = "ZZ";
			AssertNoMessageErrorContaining("Error is not shown when DsdtMrnNumber is not empty for TSM", header.DsdtMrnNumberInfo, messageError);
		});
	}

	TemporaryStorageHeader SetUpBOWithCertificateData(out GlbStaff broker, out GlbStaff authStaff)
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

		var header = Factory.New<TemporaryStorageHeader>();
		header.AMA_GS_NKCustomsAgent = broker.GS_Code;

		return header;
	}
}
