using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class JobDeclarationQuarantineTest : TestCaseWithFactory
	{
		public void TestQuarantineInvoice()
		{
			declaration.Invoices.AddNew();
			AssertNull("Declaration is not quarantine", declaration.QuarantineInvoice);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			AssertNotNull("Declaration is quarantine", declaration.QuarantineInvoice);
		}

		public void TestIsExport()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("Declaration is not Export", !declaration.IsExport);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			Assert("Declaration is Export", declaration.IsExport);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExportDeclarationByExternalBroker;
			Assert("Declaration is Export", declaration.IsExport);
		}

		public void TestIsQuarantine()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Assert("Declaration is not Quarantine", !declaration.IsQuarantine);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			Assert("Declaration is Quarantine", declaration.IsQuarantine);
		}

		public void TestIsImport()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert(testDec.IsImport);
			testDec.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			Assert(testDec.IsImport);
			testDec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			Assert(testDec.IsImport);
			testDec.JE_MessageType = JobMessageTypeList.Codes.ImportDeclarationByExternalBroker;
			Assert(testDec.IsImport);
			testDec.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			Assert(!testDec.IsImport);
		}

		public void TestIsImporterDiplomat()
		{
			OrgHeader importer = Factory.New<OrgHeader>();
			var cusCode = importer.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.Diplomat;
			declaration.JE_OH_Importer = importer.PK;
			Assert("Importer is a diplomat", declaration.IsImporterDiplomat);
		}

		public void TestIsImportOrDrawback()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert(testDec.IsImportOrDrawback);
			testDec.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			Assert(testDec.IsImportOrDrawback);
			testDec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			Assert(testDec.IsImportOrDrawback);
			testDec.JE_MessageType = JobMessageTypeList.Codes.ImportDeclarationByExternalBroker;
			Assert(testDec.IsImportOrDrawback);
			testDec.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			Assert(testDec.IsImportOrDrawback);
			testDec.JE_MessageType = JobMessageTypeList.Codes.Export;
			Assert(!testDec.IsImportOrDrawback);
		}

		public void TestValidationType()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			Assert("Import validation", declaration.Validation is QuarantineJobDeclarationValidation);
		}

		public void TestContainersAlwaysRequired()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			Assert("Containers Required", declaration.ContainersRequired);
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			Assert("Containers Not Required", !declaration.ContainersRequired);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			Assert("Containers Required", declaration.ContainersRequired);
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			Assert("Containers Required", declaration.ContainersRequired);
		}

		public void TestIsNonTransportDeclarationType()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Assert("Export is TrasportDeclarationType", !declaration.IsNonTransportDeclarationType);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("Import is TrasportDeclarationType", !declaration.IsNonTransportDeclarationType);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			Assert("Drawback is NonTrasportDeclarationType", declaration.IsNonTransportDeclarationType);
		}

		public void TestJE_ToOrder()
		{
			declaration.JE_ToOrder = false;
			declaration.JE_OH_Importer = ZGuid.NewZGuid();
			declaration.JE_ToOrder = true;
			Assert("Importer has been cleared", declaration.JE_OH_Importer.IsEmpty);
		}

		public void TestJE_ToOrderComment()
		{
			declaration.JE_ToOrder = false;
			Assert("Importer City is empty", declaration.JE_ToOrderComment.IsEmpty);
			declaration.JE_ToOrder = true;
			Assert("Importer City is empty", declaration.JE_ToOrderComment == "UNKNOWN");
			declaration.JE_ToOrderComment = "Consignee City to be announced";
			declaration.JE_ToOrder = false;
			Assert("Importer City is empty", declaration.JE_ToOrderComment.IsEmpty);
		}

		public void TestEmailSentToCustomsWhenFreightChangesDeliveryAddress()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			group.GG_Code = "TG";
			GlbStaff gStaff1 = Factory.New<GlbStaff>();
			gStaff1.GS_Code = "G1";
			gStaff1.GS_LoginName = "G1";
			gStaff1.GS_EmailAddress = "g1@edi.com.au";
			GlbGroupLink link1 = Factory.New<GlbGroupLink>();
			link1.GK_GG = group.PK;
			link1.GK_GS = gStaff1.PK;
			GlbStaff gStaff2 = Factory.New<GlbStaff>();
			gStaff2.GS_Code = "G2";
			gStaff1.GS_LoginName = "G2";
			gStaff2.GS_EmailAddress = "g2@edi.com.au";
			GlbGroupLink link2 = Factory.New<GlbGroupLink>();
			link2.GK_GG = group.PK;
			link2.GK_GS = gStaff2.PK;
			Env.Registry.AUCustoms.EdificeSendAcknowledgementsToGroup = group.PK.ToGuid();
			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_Code = "CODE";
			OrgAddress mainAddress = header.MainAddress;
			mainAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			OrgAddress deliveryAddress = header.Addresses.AddNew();
			deliveryAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery);
			deliveryAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Delivery);
			deliveryAddress.OA_Address1 = "Address1";
			deliveryAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDeliveryAddress.OrganisationPK = header.PK;
			declaration.JE_JS = shipment.PK;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "12345";
			EDIMessage message = entryHeader.Messages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Received;
			GlbStaff newStaff = Factory.New<GlbStaff>();
			newStaff.GS_Code = "FB";
			newStaff.GS_LoginName = "FB";
			newStaff.GS_EmailAddress = "test1@edi.com.au";
			message.EM_SystemCreateUser = "FB";
			Factory.Save();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Sent;
			Factory.Save();
			AssertEquals("Pre-condition", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			shipment.UserHasEnteredDeliverAddressControl = true;
			shipment.ConsigneeDeliveryAddress.E2_Address1 = "Changed Address";
			shipment.UserHasEnteredDeliverAddressControl = false;
			Factory.Save();
			AssertEquals("1 email sent", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			Assert("Notification Email Body", Env.OutgoingCustomsMailManager.EmailsCreated[0].Body.Contains("You should send an amendment message to Customs to reflect this change at Customs."));
			Assert("Last message user", Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Contains("test1@edi.com.au"));
			Assert("Group user", Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Contains("g1@edi.com.au"));
			Assert("Group user", Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.Contains("g2@edi.com.au"));
		}

		public void TestDeliveryAddressPostCodeMessages()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;

			var refTestHelper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = new ZDateTime(2020, 1, 1);
			var endDate = new ZDateTime(2076, 6, 6);
			var postcode1 = refTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, "AUPC", "123X", "Postcode", startDate, endDate);
			refTestHelper.CreateNewOrGetExistingCusCodeListAttribute(postcode1.PK, "PostcodeDeliveryClassification", "METRO");
			var postcode2 = refTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, "AUPC", "080X", "Postcode", startDate, endDate);
			refTestHelper.CreateNewOrGetExistingCusCodeListAttribute(postcode2.PK, "PostcodeDeliveryClassification", "SPLIT");
			var postcode3 = refTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, "AUPC", "111X", "Postcode", startDate, endDate);
			refTestHelper.CreateNewOrGetExistingCusCodeListAttribute(postcode3.PK, "PostcodeDeliveryClassification", "RURAL");
			Factory.Save();

			ZString postCodeError;
			ZString postCodeWarning;
			declaration.DeliveryAddressPostCodeMessages(out postCodeError, out postCodeWarning);
			Assert("DeliveryAddressPostCodeMessages-no importer", postCodeError.IsEmpty);
			Assert("DeliveryAddressPostCodeMessages-should be no warning", postCodeWarning.IsEmpty);

			OrgHeader importer = OrgHeader.New(Factory);
			importer.OH_IsConsignee = true;
			declaration.ImporterDeliveryAddress.OrganisationPK = importer.PK;
			declaration.ImporterDeliveryAddress.E2_AddressOverride = false;

			declaration.DeliveryAddressPostCodeMessages(out postCodeError, out postCodeWarning);
			Assert("DeliveryAddressPostCodeMessages-no importer delivery address", postCodeError.IsEmpty);
			Assert("DeliveryAddressPostCodeMessages-should be no warning", postCodeWarning.IsEmpty);

			OrgAddress address = importer.Addresses.AddNew();
			declaration.ImporterDeliveryAddress.E2_OA_Address = address.PK;

			declaration.DeliveryAddressPostCodeMessages(out postCodeError, out postCodeWarning);
			Assert("DeliveryAddressPostCodeMessages-no importer delivery address post code", postCodeError.IsEmpty);
			Assert("DeliveryAddressPostCodeMessages-should be no warning", postCodeWarning.IsEmpty);

			address.OA_PostCode = "123X";
			declaration.DeliveryAddressPostCodeMessages(out postCodeError, out postCodeWarning);
			Assert("DeliveryAddressPostCodeMessages-metro post code", postCodeError.IsEmpty);
			Assert("DeliveryAddressPostCodeMessages-should be no warning", postCodeWarning.IsEmpty);

			address.OA_PostCode = "111X";
			declaration.DeliveryAddressPostCodeMessages(out postCodeError, out postCodeWarning);
			Assert("DeliveryAddressPostCodeMessages-rural post code", postCodeError.Contains("Delivery address post code is listed as RURAL"));
			Assert("DeliveryAddressPostCodeMessages-should be no warning", postCodeWarning.IsEmpty);

			address.OA_PostCode = "222X";
			declaration.DeliveryAddressPostCodeMessages(out postCodeError, out postCodeWarning);
			Assert("DeliveryAddressPostCodeMessages-rural post code", postCodeError.Contains("Delivery address post code is listed as RURAL"));
			Assert("DeliveryAddressPostCodeMessages-should be no warning", postCodeWarning.IsEmpty);

			address.OA_PostCode = "080X";
			declaration.DeliveryAddressPostCodeMessages(out postCodeError, out postCodeWarning);
			Assert("DeliveryAddressPostCodeMessages-split post code", postCodeError.Contains("Delivery address post code is listed as SPLIT"));
			Assert("DeliveryAddressPostCodeMessages-should be no warning", postCodeWarning.IsEmpty);

			address.OA_PostCode = "80X";
			declaration.DeliveryAddressPostCodeMessages(out postCodeError, out postCodeWarning);
			Assert("DeliveryAddressPostCodeMessages-short post code", postCodeError.Contains("Delivery address post code is listed as SPLIT"));
			Assert("DeliveryAddressPostCodeMessages-should be no warning", postCodeWarning.IsEmpty);

			declaration.ImporterDeliveryAddress.E2_AddressOverride = true;

			declaration.ImporterDeliveryAddress.E2_Postcode = ZString.Empty;
			declaration.DeliveryAddressPostCodeMessages(out postCodeError, out postCodeWarning);
			Assert("DeliveryAddressPostCodeMessages-no override post code", postCodeError.IsEmpty);
			Assert("DeliveryAddressPostCodeMessages-should be no warning", postCodeWarning.IsEmpty);

			declaration.ImporterDeliveryAddress.E2_Postcode = "123X";
			declaration.DeliveryAddressPostCodeMessages(out postCodeError, out postCodeWarning);
			Assert("DeliveryAddressPostCodeMessages-override metro post code", postCodeError.IsEmpty);
			Assert("DeliveryAddressPostCodeMessages-should be no warning", postCodeWarning.IsEmpty);

			declaration.ImporterDeliveryAddress.E2_Postcode = "111X";
			declaration.DeliveryAddressPostCodeMessages(out postCodeError, out postCodeWarning);
			Assert("DeliveryAddressPostCodeMessages-override rural post code", postCodeError.Contains("Delivery address post code is listed as RURAL"));
			Assert("DeliveryAddressPostCodeMessages-should be no warning", postCodeWarning.IsEmpty);

			declaration.ImporterDeliveryAddress.E2_Postcode = "222X";
			declaration.DeliveryAddressPostCodeMessages(out postCodeError, out postCodeWarning);
			Assert("DeliveryAddressPostCodeMessages-override rural post code", postCodeError.Contains("Delivery address post code is listed as RURAL"));
			Assert("DeliveryAddressPostCodeMessages-should be no warning", postCodeWarning.IsEmpty);

			declaration.ImporterDeliveryAddress.E2_Postcode = "080X";
			declaration.DeliveryAddressPostCodeMessages(out postCodeError, out postCodeWarning);
			Assert("DeliveryAddressPostCodeMessages-override split post code", postCodeError.Contains("Delivery address post code is listed as SPLIT"));
			Assert("DeliveryAddressPostCodeMessages-should be no warning", postCodeWarning.IsEmpty);

			declaration.ImporterDeliveryAddress.E2_Postcode = "80X";
			declaration.DeliveryAddressPostCodeMessages(out postCodeError, out postCodeWarning);
			Assert("DeliveryAddressPostCodeMessages-override short post code", postCodeError.Contains("Delivery address post code is listed as SPLIT"));
			Assert("DeliveryAddressPostCodeMessages-should be no warning", postCodeWarning.IsEmpty);

			declaration.AddInfo.ZA_AQISInspectLocation_Hidden = "INSPECTION LOCATION";

			declaration.ImporterDeliveryAddress.E2_Postcode = "111X";
			declaration.DeliveryAddressPostCodeMessages(out postCodeError, out postCodeWarning);
			Assert("DeliveryAddressPostCodeMessages-still should be message error", postCodeError.Contains("Delivery address post code is listed as RURAL"));
			Assert("DeliveryAddressPostCodeMessages-should be no warning", postCodeWarning.IsEmpty);

			declaration.ImporterDeliveryAddress.E2_Postcode = "222X";
			declaration.DeliveryAddressPostCodeMessages(out postCodeError, out postCodeWarning);
			Assert("DeliveryAddressPostCodeMessages-still should be message error", postCodeError.Contains("Delivery address post code is listed as RURAL"));
			Assert("DeliveryAddressPostCodeMessages-should be no warning", postCodeWarning.IsEmpty);

			AQISConcernType otherConcernType = declaration.AQISConcernTypes.AddNew();
			otherConcernType.Code = "OTHR";
			declaration.DeliveryAddressPostCodeMessages(out postCodeError, out postCodeWarning);
			Assert("DeliveryAddressPostCodeMessages-still should be message error", postCodeError.Contains("Delivery address post code is listed as RURAL"));
			Assert("DeliveryAddressPostCodeMessages-should be no warning", postCodeWarning.IsEmpty);

			AQISConcernType ruralConcernType = declaration.AQISConcernTypes.AddNew();
			ruralConcernType.Code = "RURL";
			declaration.DeliveryAddressPostCodeMessages(out postCodeError, out postCodeWarning);
			Assert("DeliveryAddressPostCodeMessages-now no message error", postCodeError.IsEmpty);
			Assert("DeliveryAddressPostCodeMessages-but is warning", postCodeWarning.Contains("Delivery address post code is listed as RURAL, tailgate inspection is required"));

			declaration.ImporterDeliveryAddress.E2_Postcode = "111X";
			declaration.DeliveryAddressPostCodeMessages(out postCodeError, out postCodeWarning);
			Assert("DeliveryAddressPostCodeMessages-now no message error", postCodeError.IsEmpty);
			Assert("DeliveryAddressPostCodeMessages-but is warning", postCodeWarning.Contains("Delivery address post code is listed as RURAL, tailgate inspection is required"));

			declaration.ImporterDeliveryAddress.E2_Postcode = "080X";
			declaration.DeliveryAddressPostCodeMessages(out postCodeError, out postCodeWarning);
			Assert("DeliveryAddressPostCodeMessages-now no message error", postCodeError.IsEmpty);
			Assert("DeliveryAddressPostCodeMessages-but is warning", postCodeWarning.Contains("Delivery address post code is listed as SPLIT, tailgate inspection is required if the address is Rural."));

			declaration.ImporterDeliveryAddress.E2_Postcode = "222X";
			declaration.AddInfo.ZA_AQISInspectLocation_Hidden = ZString.Empty;
			declaration.DeliveryAddressPostCodeMessages(out postCodeError, out postCodeWarning);
			Assert("DeliveryAddressPostCodeMessages-message error should be back", postCodeError.Contains("Delivery address post code is listed as RURAL"));
			Assert("DeliveryAddressPostCodeMessages-but warning gone", postCodeWarning.IsEmpty);
		}

		public void TestQuarantineHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			var docHeader = declaration.QuarantineInvoice?.QuarantineExDocHeader;

			Assert("QuarantineExDocHeader exists", docHeader != null);
			docHeader.Validation.ValidateAll();
			AssertHasMessageErrorContaining(docHeader.QH_ProduceTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(docHeader.QH_AQISRegionInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(docHeader.QH_CertificateRequiredLocationInfo, "Health certificate print location is required");
			AssertHasMessageError(docHeader.QH_AuthorisationEstablishmentInfo, "Authorization Establishment must be entered.");

			docHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			docHeader.QH_AQISRegion = ZString.Empty;
			AssertEquals(true, docHeader.IsNEXDOCSActive);
			AssertNoMessageErrorContaining(docHeader.QH_AQISRegionInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			docHeader.Validation.ValidateAll();
			AssertNoMessageErrorContaining(docHeader.QH_ProduceTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(docHeader.QH_AQISRegionInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(docHeader.QH_CertificateRequiredLocationInfo, "Health certificate print location is required");
			AssertNoMessageError(docHeader.QH_AuthorisationEstablishmentInfo, "Authorization Establishment must be entered.");

			Factory.Save();
			Assert("Quarantine Invoice does not exists", declaration.QuarantineInvoice == null);
		}

		public void TestResetARPATDJobDocAddressCache_WhenMessageTypeChange()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.Invoices.AddNew();
			var quarantineInvoice = declaration.QuarantineInvoice;
			var aQISResponsiblePerson = quarantineInvoice.AQISResponsiblePerson;
			var aQISTransitDestination = quarantineInvoice.AQISTransitDestination;
			AssertEquals(aQISResponsiblePerson, invoice.AQISResponsiblePerson);
			AssertEquals(aQISTransitDestination, invoice.AQISTransitDestination);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Factory.Save();
			AssertNull(declaration.QuarantineInvoice);
			AssertNull(Factory.Load<JobDocAddress>(aQISResponsiblePerson.PK));
			AssertNull(Factory.Load<JobDocAddress>(aQISTransitDestination.PK));
			AssertNotEquals(aQISResponsiblePerson, invoice.AQISResponsiblePerson);
			AssertNotEquals(aQISTransitDestination, invoice.AQISTransitDestination);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}

		JobDeclaration declaration;

		#endregion
	}
}
