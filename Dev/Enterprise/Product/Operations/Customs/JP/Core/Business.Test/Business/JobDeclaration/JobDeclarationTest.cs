using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Common.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.JP.Business.DeclarationCargoTypeList;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.JP.Business.Testing;

[TestedType(typeof(JobDeclaration))]
sealed class JobDeclarationTest : Customs.Business.Testing.BaseJobDeclarationTest<JobDeclaration>
{
	public void TestDepotDocAddressDefaultValue()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var startDate = ZDateTime.Today.AddDays(-1);
		var endDate = ZDateTime.Today.AddDays(1);
		helper.CreateNewOrGetExistingDataGrouping("JP");
		helper.CreateNewOrGetExistingCusCodeType("JPBLC", "JPBLC", Core.Constants.CountryCodes.Japan);
		var refCusCodeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan, "JPBLC", "2HDN8", startDate, endDate);
		refCusCodeList.Attributes.DeleteAll();
		helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeList.PK, RefCusCodeListAttributeTypes.Codes.Type, "洋上");

		Factory.Save();

		var entryInstruction1 = Declaration.CustomsEntryInstructions.AddNew();
		entryInstruction1.CEI_Style = JPImportDeclarationTypeList.Codes.B;
		var entryInstruction2 = Declaration.CustomsEntryInstructions.AddNew();

		var org = Factory.NewWithValidTestData<OrgHeader>();
		var customsCode = org.CustomsCodes.AddNew();
		customsCode.OK_RN_NKCodeCountry = "JP";
		customsCode.OK_CustomsRegNo = "2HDN8";
		customsCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
		customsCode.OK_OA_PremisesAddress = org.MainAddress.PK;
		Declaration.DepotDocAddress.E2_OA_Address = org.MainAddress.PK;

		Assert(entryInstruction1.CEI_Style == JPImportDeclarationTypeList.Codes.B);
		Assert(entryInstruction2.CEI_Style == JPImportDeclarationTypeList.Codes.E);
		Assert(Declaration.JE_RL_NKPortOfLoading == "ZZZ");
	}

	public void TestDocumentSupporterType()
	{
		AssertType<JobDeclarationDocumentSupporter>(Declaration.DocumentSupporter);
	}

	public void TestIsReadyForSending()
	{
		Assert("Default to false", !Declaration.IsReadyForSending);

		var settings = new MailboxAndRemoteWebPrintClientCredentials { LocalComputerAlias = "TEST", DomainName = "TEST", Status = XtCredentialStatusList.Codes.Registered };
		JPRegistry.Instance.MailboxAndRemoteWebPrintClientCredentials.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

		var company = Factory.NewWithValidTestData<GlbCompany>();
		company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;

		var wrapper = GlbCompanyWrapper.GetWrapper<JPGlbCompanyWrapper>(company);

		var mailboxCredential = wrapper.MailboxCredential;
		mailboxCredential.GP_MailBoxID = "TEST";
		mailboxCredential.CurrentDecryptedPassword = "TEST";

		declaration.JE_GC = company.PK;

		Assert("Should be ready for sending to the NACCS as there is a valid MailboxAndRemoteWebPrintClientCredentials and MailboxCredential.", Declaration.IsReadyForSending);
	}

	public void TestDocAddresses()
	{
		AssertType<JPJobDocAddressDependentCollection>(Declaration.DocAddresses);
	}

	public void TestImporterDocumentaryAddressDefaultValue()
	{
		Declaration.JE_MessageType = "IMP";
		var org = Factory.NewWithValidTestData<OrgHeader>();

		org.MainAddress.CustomsCodes.AddNew(OrgCusCode.JapanCodeTypes.LPC, "12345", Core.Constants.CountryCodes.Japan);
		Declaration.ImporterDocumentaryAddress.E2_OA_Address = org.MainAddress.PK;

		AssertEquals(OrgCusCode.JapanCodeTypes.LPC, Declaration.ImporterDocumentaryAddress.E2_GovRegNumType);
		AssertEquals("12345", Declaration.ImporterDocumentaryAddress.E2_GovRegNum);
		AssertEquals("Should sync value to JE_OH_Importer.", org.PK, Declaration.JE_OH_Importer);
		AssertEquals("Should sync value to JE_OA_ImporterAddress .", org.MainAddress.PK, Declaration.JE_OA_ImporterAddress);

		Declaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
		AssertEquals("Should sync value to ImporterDocumentaryAddress.", Declaration.JE_OH_Importer, Declaration.ImporterDocumentaryAddress.OrganisationPK);
	}

	public void TestJE_ArrivalAtLoadingDate()
	{
		var data = DataBoundResourceStrings.GetDataForProperty(Declaration.JE_ArrivalAtLoadingDateInfo);
		CombineAssertions(() =>
		{
			AssertEquals("ShortCaption", "Ent.", data.ShortCaption);
			AssertEquals("Caption", "Date Of Entry", data.Caption);
			AssertEquals("FullDescription", "Entry date at Port of Loading", data.FullDescription);
		});
	}

	public void TestSupplierDocumentaryAddressDefaultValue()
	{
		var org = Factory.NewWithValidTestData<OrgHeader>();

		org.MainAddress.CustomsCodes.AddNew(OrgCusCode.JapanCodeTypes.LPC, "12345", Core.Constants.CountryCodes.Japan);
		Declaration.SupplierDocumentaryAddress.E2_OA_Address = org.MainAddress.PK;

		AssertEquals(OrgCusCode.JapanCodeTypes.LPC, Declaration.SupplierDocumentaryAddress.E2_GovRegNumType);
		AssertEquals("12345", Declaration.SupplierDocumentaryAddress.E2_GovRegNum);
		AssertEquals("Should sync value to JE_OH_Supplier.", org.PK, Declaration.JE_OH_Supplier);
		AssertEquals("Should sync value to JE_OA_SupplierAddress.", org.MainAddress.PK, Declaration.JE_OA_SupplierAddress);

		Declaration.SupplierDocumentaryAddress.E2_OA_Address = ZGuid.Empty;

		Declaration.JE_OH_Supplier = Factory.NewWithValidTestData<OrgHeader>().PK;
		AssertEquals("Should sync value to SupplierDocumentaryAddress.", Declaration.JE_OH_Supplier, Declaration.SupplierDocumentaryAddress.OrganisationPK);
	}

	public void TestDefaultNACCSCredentialForCusAgent()
	{
		var credentialPKs = new DefaultBrokerAndCredentialTestHelper().CreateBrokerStaffAndReturnPK();
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
		Declaration.JE_GS_NKCusAgent = "AN";

		AssertEquals(credentialPKs.credentialPK1, Declaration.JE_NACCSCredential);
	}

	public void TestIsECR()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
		var entryInstruction1 = Declaration.CustomsEntryInstructions.AddNew();
		entryInstruction1.ExportControlNumber = "123";
		Assert(!Declaration.IsECR);

		entryInstruction1.ExportControlNumberInfo.ClearValue();
		Assert(Declaration.IsECR);

		entryInstruction1.ExportControlNumber = "123";
		var entryInstruction2 = Declaration.CustomsEntryInstructions.AddNew();
		entryInstruction2.ExportControlNumber = "";
		Assert(Declaration.IsECR);

		entryInstruction2.ExportControlNumber = "123";
		Assert(!Declaration.IsECR);

		var entryInstruction3 = Declaration.CustomsEntryInstructions.AddNew();
		entryInstruction3.CreateNewEntryNumber(CusEntryNumberTypes.JP.ExportControlNumber, "123", true);
		Assert(Declaration.IsECR);
	}

	public void TestIsBasketRadioCallSign()
	{
		Declaration.JE_RadioCallSign = "1234";
		AssertEquals(false, Declaration.IsBasketRadioCallSign);

		Declaration.JE_RadioCallSign = "9999";
		AssertEquals(true, Declaration.IsBasketRadioCallSign);
	}

	public void TestJE_OwnerSectionCode()
	{
		Declaration.JE_OwnerSectionCode = "南京NJ123";
		AssertEquals("NJ123", Declaration.JE_OwnerSectionCode);

		var data = DataBoundResourceStrings.GetDataForProperty(Declaration.JE_OwnerSectionCodeInfo);
		AssertEquals("Owner Section Code", data.Caption);
		AssertEquals("Owner Sec. Co.", data.ShortCaption);
	}

	public void TestDefaultJE_ACP_POA()
	{
		var supplier = Factory.NewWithValidTestData<OrgHeader>();
		var representative = Factory.NewWithValidTestData<OrgAddress>();
		var eDoc = supplier.RequiredDocuments.AddNew();
		eDoc.EQ_ValidToDate = ZDateTime.Today.AddDays(1);
		eDoc.EQ_DateReceived = ZDateTimeOffset.Today.AddDays(-1);
		eDoc.EQ_DocCategory = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
		eDoc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
		eDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.AttorneyForCustomsProcedures;
		eDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Japan;
		eDoc.EQ_DocNumber = "1234567890";
		eDoc.EQ_OH_DocumentOwner = representative.OA_OH;

		Factory.Save();

		Declaration.JE_OA_Representative = representative.PK;
		Declaration.JE_OH_Supplier = supplier.PK;
		AssertEquals("1234567890", Declaration.JE_ACP_POA);

		Declaration.JE_ACP_POA = ZString.Empty;
		Declaration.JE_OA_Representative = ZGuid.Empty;
		Declaration.JE_OA_Representative = representative.PK;
		AssertEquals("1234567890", Declaration.JE_ACP_POA);
	}

	public void TestShouldShowDescriptionForUnknownPortName()
	{
		var entryInstruction = Declaration.CustomsEntryInstructions.AddNew();
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
		Declaration.JE_RL_NKPortOfLoading = Common.Constants.PortNames.UnknownPortName;
		AssertEquals(true, Declaration.ShouldShowDescriptionForUnknownPortName);

		Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
		AssertEquals(false, Declaration.ShouldShowDescriptionForUnknownPortName);

		entryInstruction.CEI_DeclarationCargoType = MailedCargoList.Codes.H;
		AssertEquals(true, Declaration.ShouldShowDescriptionForUnknownPortName);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals(false, Declaration.ShouldShowDescriptionForUnknownPortName);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		Declaration.JE_RL_NKPortOfLoading = ZString.Empty;
		AssertEquals(false, Declaration.ShouldShowDescriptionForUnknownPortName);
	}

	public void TestPortOfLoadingDescription()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
		Declaration.JE_RL_NKPortOfLoading = Common.Constants.PortNames.UnknownPortName;
		AssertEquals(Common.Constants.PortNames.UnknownPortName, Declaration.PortOfLoadingDescription);

		Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
		AssertEquals(FindBoxMessages.InvalidSelection, Declaration.PortOfLoadingDescription);

		Declaration.JE_RL_NKPortOfLoading = ZString.Empty;
		AssertEquals(FindBoxMessages.NoneSelected, Declaration.PortOfLoadingDescription);

		Declaration.JE_RL_NKPortOfLoading = "JPAAE";
		AssertEquals("Tsubata, Ishikawa", Declaration.PortOfLoadingDescription);

		Declaration.JE_RL_NKPortOfLoading = "TWTPE";
		AssertEquals(FindBoxMessages.InvalidSelection, Declaration.PortOfLoadingDescription);
	}

	public void TestInspectionWitnessAddressAndInspectionWitnessAddressCode()
	{
		var org = Factory.NewWithValidTestData<OrgHeader>();
		Declaration.InspectionWitness.E2_OA_Address = org.MainAddress.PK;

		AssertNullOrEmpty(Declaration.InspectionWitnessCode);
		Assert(!Declaration.InspectionWitnessCodeInfo.ReadOnly);

		var newAddress = org.Addresses.AddNew();
		newAddress.CustomsCodes.AddNew(OrgCusCode.JapanCodeTypes.NUC, "12345", Core.Constants.CountryCodes.Japan);
		Declaration.InspectionWitness.E2_OA_Address = newAddress.PK;

		AssertEquals("12345", Declaration.InspectionWitnessCode);
		Assert(Declaration.InspectionWitnessCodeInfo.ReadOnly);

		Declaration.InspectionWitnessCode = "54321";

		Assert(Declaration.InspectionWitness.E2_AddressOverride);
		Assert(!Declaration.InspectionWitnessCodeInfo.ReadOnly);
	}

	public void TestExternalBrokerAddressAndExternalBrokerCode()
	{
		var org = Factory.NewWithValidTestData<OrgHeader>();
		Declaration.ExternalBrokerAddress.E2_OA_Address = org.MainAddress.PK;

		AssertNullOrEmpty(Declaration.ExternalBrokerCode);
		Assert(!Declaration.ExternalBrokerCodeInfo.ReadOnly);

		var newAddress = org.Addresses.AddNew();
		newAddress.CustomsCodes.AddNew(OrgCusCode.JapanCodeTypes.NUC, "12345", Core.Constants.CountryCodes.Japan);
		Declaration.ExternalBrokerAddress.E2_OA_Address = newAddress.PK;

		AssertEquals("12345", Declaration.ExternalBrokerCode);
		Assert(Declaration.ExternalBrokerCodeInfo.ReadOnly);

		Declaration.ExternalBrokerCode = "54321";

		Assert(Declaration.ExternalBrokerAddress.E2_AddressOverride);
		Assert(!Declaration.ExternalBrokerCodeInfo.ReadOnly);
	}

	public override void TestMasterBillLabel()
	{
		var info = Declaration.JE_MasterBillInfo;
		AssertEquals("Air Caption", "MAWB", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(info, JobDeclaration.AirCaptionKey).Caption);
		AssertEquals("Sea Caption", "Master Bill", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(info, JobDeclaration.SeaCaptionKey).Caption);
	}

	public void TestForwarderAddressAndForwarderCode()
	{
		var org = Factory.NewWithValidTestData<OrgHeader>();
		Declaration.ForwarderAddress.E2_OA_Address = org.MainAddress.PK;

		AssertNullOrEmpty(Declaration.ForwarderCode);
		Assert(!Declaration.ForwarderCodeInfo.ReadOnly);

		var newAddress = org.Addresses.AddNew();
		newAddress.CustomsCodes.AddNew(OrgCusCode.JapanCodeTypes.NUC, "12345", Core.Constants.CountryCodes.Japan);
		Declaration.ForwarderAddress.E2_OA_Address = newAddress.PK;

		AssertEquals("12345", Declaration.ForwarderCode);
		Assert(Declaration.ForwarderCodeInfo.ReadOnly);

		Declaration.ForwarderCode = "54321";

		Assert(Declaration.ForwarderAddress.E2_AddressOverride);
		Assert(!Declaration.ForwarderCodeInfo.ReadOnly);
	}

	public void TestForwarderAddress()
	{
		var forwarderAddress = Declaration.ForwarderAddress;
		AssertNotNull("docAddresses.GetDocAddress(DocAddressType.Forwarder)", forwarderAddress);
		AssertEquals(DocAddressType.Forwarder, forwarderAddress.DocAddressType);
		AssertEquals(ContactType.Administration, forwarderAddress.DefaultContactType);
	}

	public void TestAirCargoAgentAndAirCargoAgentCode()
	{
		var org = Factory.NewWithValidTestData<OrgHeader>();
		Declaration.AirCargoAgent.E2_OA_Address = org.MainAddress.PK;

		AssertNullOrEmpty(Declaration.AirCargoAgentNACCSCode);
		AssertNullOrEmpty(Declaration.AirCargoAgentLocationCode);

		var newAddress = org.Addresses.AddNew();
		newAddress.CustomsCodes.AddNew(OrgCusCode.JapanCodeTypes.NUC, "12345", Core.Constants.CountryCodes.Japan);
		newAddress.CustomsCodes.AddNew(OrgCusCode.JapanCodeTypes.AAL, "ABC", Core.Constants.CountryCodes.Japan);
		Declaration.AirCargoAgent.E2_OA_Address = newAddress.PK;

		AssertEquals("12345", Declaration.AirCargoAgentNACCSCode);
		AssertEquals("ABC", Declaration.AirCargoAgentLocationCode);
		AssertEquals(true, Declaration.AirCargoAgentNACCSCodeInfo.ReadOnly);
		AssertEquals(true, Declaration.AirCargoAgentLocationCodeInfo.ReadOnly);

		Declaration.AirCargoAgentNACCSCode = "54321";
		AssertEquals(true, Declaration.AirCargoAgent.E2_AddressOverride);
		AssertEquals(false, Declaration.AirCargoAgentNACCSCodeInfo.ReadOnly);
		AssertEquals(false, Declaration.AirCargoAgentLocationCodeInfo.ReadOnly);

		Declaration.AirCargoAgent.E2_AddressOverride = false;
		Declaration.AirCargoAgentLocationCode = "CBA";
		AssertEquals(true, Declaration.AirCargoAgent.E2_AddressOverride);
		AssertEquals(false, Declaration.AirCargoAgentNACCSCodeInfo.ReadOnly);
		AssertEquals(false, Declaration.AirCargoAgentLocationCodeInfo.ReadOnly);
	}

	public void TestAirCargoAgent()
	{
		var airCargoAgent = Declaration.AirCargoAgent;
		AssertNotNull("docAddresses.GetDocAddress(DocAddressType.AirCargoAgent)", airCargoAgent);
		AssertEquals(DocAddressType.AirCargoAgent, airCargoAgent.DocAddressType);
		AssertEquals(ContactType.Administration, airCargoAgent.DefaultContactType);
	}

	public void TestPortOfLoadingIATACode()
	{
		Factory.CreateUNLOCOData();

		Declaration.JE_RL_NKPortOfLoading = "JP001";
		AssertEquals(string.Empty, Declaration.PortOfLoadingIATACode);

		Declaration.JE_RL_NKPortOfLoading = "JP002";
		AssertEquals("TKU", Declaration.PortOfLoadingIATACode);

		Declaration.JE_RL_NKPortOfLoading = string.Empty;
		Declaration.PortOfLoadingIATACode = "TKU";
		AssertEquals("JP002", Declaration.JE_RL_NKPortOfLoading);

		Declaration.PortOfLoadingIATACode = "TK";
		AssertEquals("TK", Declaration.PortOfLoadingIATACode);
		AssertEquals("JP002", Declaration.JE_RL_NKPortOfLoading);

		Declaration.PortOfLoadingIATACode = string.Empty;
		AssertEquals("JP002", Declaration.JE_RL_NKPortOfLoading);
	}

	public void TestFinalDestinationIATACode()
	{
		Factory.CreateUNLOCOData();

		Declaration.JE_RL_NKFinalDestination = "JP001";
		AssertEquals(string.Empty, Declaration.FinalDestinationIATACode);

		Declaration.JE_RL_NKFinalDestination = "JP002";
		AssertEquals("TKU", Declaration.FinalDestinationIATACode);

		Declaration.JE_RL_NKFinalDestination = string.Empty;
		Declaration.FinalDestinationIATACode = "TKU";
		AssertEquals("JP002", Declaration.JE_RL_NKFinalDestination);

		Declaration.FinalDestinationIATACode = "TK";
		AssertEquals("TK", Declaration.FinalDestinationIATACode);
		AssertEquals("JP002", Declaration.JE_RL_NKFinalDestination);

		Declaration.FinalDestinationIATACode = string.Empty;
		AssertEquals("JP002", Declaration.JE_RL_NKFinalDestination);
	}

	public override void TestNewOwner()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
		var owner1 = Factory.NewWithValidTestData<OrgHeader>();
		owner1.OH_Code = "X1$1";
		owner1.OH_FullName = owner1.OH_Code;

		var owner2 = Factory.NewWithValidTestData<OrgHeader>();
		owner2.OH_Code = "X2$2";
		owner2.OH_FullName = owner2.OH_Code;

		var instruction1 = Declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
		instruction1.CEI_Style = "G";
		instruction1.CEI_Description = "AA";
		instruction1.CEI_OH_Owner = owner1.PK;

		var instruction2 = Declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
		instruction2.CEI_Style = "D";
		instruction2.CEI_Description = "BB";
		instruction2.CEI_OH_Owner = owner2.PK;

		AssertEquals(instruction1.Owner, Declaration.NewOwner);

		instruction2.CEI_OH_Owner = ZGuid.Empty;
		AssertNotNull(Declaration.NewOwner);
	}

	public void TestJE_MessageType()
	{
		var data = DataBoundResourceStrings.GetDataForProperty(Declaration.JE_MessageTypeInfo);
		AssertEquals("Msg. Type", data.ShortCaption);
		AssertEquals("Msg. Type", data.MediumCaption);
		AssertEquals("Message Type", data.Caption);
		AssertEquals("Indicates the type of customs declaration to be created. When IMP is selected, IDA import customs declaration will be created. When EXP is selected, EDA export customs declaration will be created.", data.FullDescription);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var instruction = Declaration.CustomsEntryInstructions.AddNew();
		instruction.Guarantees.AddNew();
		AssertEquals(1, instruction.Guarantees.Count);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals(0, instruction.Guarantees.Count);
	}

	public void TestDefaultCustomsOfficeDepartmentForCustomsOffice()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType("CUSOF", "Customs Office");
		helper.CreateNewOrGetExistingCusCodeList("JP", "CUSOF", "1Y", "東京税関（本関）山梨政令派出所", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList("JP", "CUSOF", "9J", "沖縄税関支署", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList("JP", "DEPTC", "1Y01", "東京税関*東京税関山梨政令派出所*通関", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList("JP", "DEPTC", "9J01", "沖縄地区税関*沖縄税関支署*通関", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList("JP", "DEPTC", "9J40", "沖縄地区税関*沖縄税関支署*監視", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		Factory.Save();

		AssertEquals(2, Declaration.Lookups.CustomsOfficeList.Count);

		Declaration.JE_CustomsOffice = "1Y";
		AssertEquals("01", Declaration.JE_CustomsOfficeDepartment);

		Declaration.JE_CustomsOffice = "9J";
		AssertEquals("01", Declaration.JE_CustomsOfficeDepartment);

		Declaration.JE_CustomsOffice = "1Y";
		Declaration.JE_CustomsOfficeDepartment = string.Empty;
		Declaration.JE_CustomsOffice = "9J";
		AssertEquals(string.Empty, Declaration.JE_CustomsOfficeDepartment);
	}

	public void TestDefaultValues()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals("ZDate.Today should be the default value for JE_ValuationDate", ZDate.Today, declaration.JE_ValuationDate);
		AssertEquals("JE_PaymentMethod should be set to empty", ZString.Empty, declaration.JE_PaymentMethod);
	}

	public void TestCusAgentAndNACCSCredential()
	{
		var (glbExternalPasswordPK1, glbExternalPasswordPK2, glbExternalPasswordPK3) = new DefaultBrokerAndCredentialTestHelper().CreateBrokerStaffAndReturnPK();

		var item = new DefaultBrokerAndCredential(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), Factory);

		item.DefaultBrokerCode = "AN";
		item.DefaultCredentialSEA = "TEST1001";
		item.DefaultCredentialAIR = "TEST2002";

		JPRegistry.Instance.DefaultBrokerAndCredential.SetValue(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK, item);
		var declaration = Factory.New<JobDeclaration>();

		AssertEquals("AN", declaration.JE_GS_NKCusAgent);
		AssertEquals(glbExternalPasswordPK3, declaration.JE_NACCSCredential);

		declaration.JE_TransportMode = UserCodeSpecificTransportModeList.Codes.SEA;
		AssertEquals(glbExternalPasswordPK1, declaration.JE_NACCSCredential);

		declaration.JE_TransportMode = UserCodeSpecificTransportModeList.Codes.AIR;
		AssertEquals(glbExternalPasswordPK2, declaration.JE_NACCSCredential);
	}

	public void TestIncoTermAndChargeFactory()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		Assert(Declaration.NeedToGetNewIncoTermAndChargeFactory);
		AssertType<ExportIncoTermAndCustomsChargeFactory>(Declaration.IncoTermAndChargeFactory);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		Assert(Declaration.NeedToGetNewIncoTermAndChargeFactory);
		AssertType<ImportIncoTermAndCustomsChargeFactory>(Declaration.IncoTermAndChargeFactory);
	}

	public void TestCustomsOfficeDepartmentReadOnly()
	{
		var targetInfo = Declaration.JE_CustomsOfficeDepartmentInfo;
		AssertEquals(true, targetInfo.ReadOnly);

		Declaration.JE_CustomsOffice = "00";
		AssertEquals(false, targetInfo.ReadOnly);
	}

	public void TestUpdateInvoiceLineJPNACCSCodeIfNeeded()
	{
		var instruction = Declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = Declaration.Invoices.AddNew().InvoiceLines.AddNew();
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		instruction.CEI_Style = JPImportDeclarationTypeList.Codes.Y;

		AssertEquals(ImportNACCSCodeList.Codes.X, invoiceLine.JI_NACCSCode);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		invoiceLine.JI_NACCSCode = ZString.Empty;

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals(ImportNACCSCodeList.Codes.X, invoiceLine.JI_NACCSCode);
	}

	public void TestHouseBillsCollectionIsOfRightType()
	{
		AssertType<BillCollection<Bill, JobDeclaration>>(Declaration.Bills);
	}

	public void TestLookupObjectIsCached()
	{
		AssertSame(Declaration.Lookups, Declaration.Lookups);
	}

	public void TestFilteredInvoiceLines()
	{
		AssertType<InvoiceLineViewCollection<JobComInvoiceLine>>(Declaration.FilteredInvoiceLines);
	}

	public void TestNACCSCredential_ReadOnly()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_GS_NKCusAgent = ZString.Empty;
			AssertEquals(true, Declaration.JE_NACCSCredentialInfo.ReadOnly);
			Declaration.JE_GS_NKCusAgent = "123";
			AssertEquals(false, Declaration.JE_NACCSCredentialInfo.ReadOnly);
		});
	}

	public void TestShowSubmitMenuItem()
	{
		Assert("ShowSubmitMenuItem", !Declaration.ShowSubmitMenuItem);
	}

	public override void TestLocalCurrencyCoreOverride()
	{
		AssertEquals(Core.Constants.CurrencyCodes.Japan, Declaration.LocalCurrencyCode);
	}

	public override void TestIsDeclarationWithEntryInstruction()
	{
		Assert("JP Declaration should support EntryInstructions", !Declaration.CustomsEntryInstructionProvider.IsNoEntryInstruction);
	}

	public void TestDocAddressRequirement()
	{
		var iDocAddresses = (IDocAddresses)Declaration;
		AssertNotNull(iDocAddresses.GetDocAddressRequirement(DocAddressType.ConsigneeAddress));
		AssertNotNull(iDocAddresses.GetDocAddressRequirement(DocAddressType.InspectionWitness));
		AssertNotNull(iDocAddresses.GetDocAddressRequirement(DocAddressType.ExternalBroker));
	}

	public void TestInspectionWitnessDocAddress()
	{
		var inspectionWitness = Declaration.InspectionWitness;
		AssertNotNull("docAddresses.GetDocAddress(DocAddressType.InspectionWitness)", inspectionWitness);
		AssertEquals(DocAddressType.InspectionWitness, inspectionWitness.DocAddressType);
		AssertEquals(ContactType.Administration, inspectionWitness.DefaultContactType);
	}

	public void TestExternalBrokerDocAddress()
	{
		var externalBrokerAddress = Declaration.ExternalBrokerAddress;
		AssertNotNull("docAddresses.GetDocAddress(DocAddressType.ExternalBroker)", externalBrokerAddress);
		AssertEquals(DocAddressType.ExternalBroker, externalBrokerAddress.DocAddressType);
		AssertEquals(ContactType.Administration, externalBrokerAddress.DefaultContactType);
	}

	public void TestDeclarationConsigneeAddress()
	{
		var header = Factory.New<OrgHeader>();
		var declarationConsigneeAddress = Declaration.DeclarationConsigneeAddress;
		AssertNotNull("docAddresses.GetDocAddress(DocAddressType.DeclarationConsigneeAddress)", declarationConsigneeAddress);
		AssertEquals(DocAddressType.ConsigneeAddress, declarationConsigneeAddress.DocAddressType);
		AssertEquals(ContactType.Administration, declarationConsigneeAddress.DefaultContactType);

		Declaration.DeclarationConsigneeAddress.E2_OA_Address = header.MainAddress.PK;
		AssertEquals(header.PK, Declaration.JE_OH_Consignee);
		AssertEquals(header.MainAddress.PK, Declaration.JE_OA_ConsigneeAddress);
	}

	public void TestDeclarationConsignorAddress()
	{
		var declarationConsignorAddress = Declaration.DeclarationConsignorAddress;
		AssertNotNull("docAddresses.GetDocAddress(DocAddressType.DeclarationConsignorAddress)", declarationConsignorAddress);
		AssertEquals(DocAddressType.ConsignorAddress, declarationConsignorAddress.DocAddressType);
		AssertEquals(ContactType.Administration, declarationConsignorAddress.DefaultContactType);
	}

	public void TestAttorneyForCustomsProceduresAddress()
	{
		var header = Factory.New<OrgHeader>();
		var attorneyForCustomsProceduresAddress = Declaration.AttorneyForCustomsProceduresAddress;
		AssertNotNull("docAddresses.GetDocAddress(DocAddressType.AttorneyForCustomsProceduresAddress)", attorneyForCustomsProceduresAddress);
		AssertEquals(DocAddressType.AttorneyForCustomsProceduresAddress, attorneyForCustomsProceduresAddress.DocAddressType);
		AssertEquals(ContactType.Administration, attorneyForCustomsProceduresAddress.DefaultContactType);

		Declaration.AttorneyForCustomsProceduresAddress.E2_OA_Address = header.MainAddress.PK;
		AssertEquals(header.MainAddress.PK, Declaration.JE_OA_Representative);
	}

	public void TestCEI_ECRCargoTypeWhenJE_MessageTypeChanged()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
		var instruction = Declaration.CustomsEntryInstructions.AddNew();
		instruction.CEI_Style = JPExportDeclarationTypeList.Codes.N;
		instruction.CEI_ECRCargoType = ZString.Empty;

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals(ZString.Empty, instruction.CEI_ECRCargoType);

		Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
		AssertEquals(CargoTypeList.Codes.T, instruction.CEI_ECRCargoType);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals(ZString.Empty, instruction.CEI_ECRCargoType);
	}

	protected override System.Collections.Hashtable ExpectedDocAddressTypes
	{
		get
		{
			var result = base.ExpectedDocAddressTypes;
			result[DocAddressTypes.Codes.ConsigneeAddress] = DocAddressType.ConsigneeAddress;
			result[DocAddressTypes.Codes.InspectionWitness] = DocAddressType.InspectionWitness;
			result[DocAddressTypes.Codes.ExportBroker] = DocAddressType.ExportBroker;
			return result;
		}
	}

	public void TestJE_PaymentMethod()
	{
		AssertEquals(1, Declaration.JE_PaymentMethodInfo.MaxLength);
	}

	public void TestJE_DefermentAccountNumber()
	{
		AssertEquals(9, Declaration.JE_DefermentAccountNumberInfo.MaxLength);
	}

	public void TestWarehouseDocAddress()
	{
		var instruction = Declaration.CustomsEntryInstructions.AddNew();
		var bondedWarehouse = Factory.NewWithValidTestData<OrgHeader>();
		bondedWarehouse.OH_FullName = "Test Company";
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		Declaration.WarehouseDocAddress.OrganisationPK = bondedWarehouse.PK;
		Declaration.WarehouseDocAddress.E2_OA_Address = bondedWarehouse.MainAddress.PK;

		AssertEquals("99999", instruction.CEI_BondedLocationCode);
		AssertEquals("Test Company", instruction.CEI_BondedLocationName);

		var bondedWarehouse2 = Factory.NewWithValidTestData<OrgHeader>();
		bondedWarehouse2.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "2HDN8", Core.Constants.CountryCodes.Japan);
		Declaration.WarehouseDocAddress.E2_OA_Address = bondedWarehouse2.MainAddress.PK;

		AssertEquals("2HDN8", instruction.CEI_BondedLocationCode);
		AssertEquals(ZString.Empty, instruction.CEI_BondedLocationName);
	}

	public void TestJE_OH_ShippingLine()
	{
		var airline = Factory.New<RefAirline>();
		airline.RM_TwoCharacterCode = "1A";

		var helper = new UniversalReferenceTestDataHelper(Factory);
		var carrier1 = helper.CreateCarrierCode("2B23", "WG TRUCKING", Core.Constants.CountryCodes.Japan);
		carrier1.ZZ4_IsSea = true;
		var carrier2 = helper.CreateCarrierCode("3C34", "Lame", Core.Constants.CountryCodes.Japan);
		carrier2.ZZ4_IsSea = true;
		Factory.Save();
		var orgForAIR = Factory.NewWithValidTestData<OrgHeader>();
		orgForAIR.OH_IsAirLine = true;
		orgForAIR.MiscServ.OM_RM_Airline = airline.PK;
		var orgForSEA1 = Factory.NewWithValidTestData<OrgHeader>();
		orgForSEA1.OH_IsShippingLine = true;
		var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
		shippingLine.RSL_IsShippingLine = true;
		shippingLine.RSL_StandardCarrierAlphaCode = "2B23";
		orgForSEA1.OH_RSL_ShippingLine = shippingLine.PK;
		orgForSEA1.CustomsCodes.RemoveAndDeleteAll();

		var orgForSEA2 = Factory.NewWithValidTestData<OrgHeader>();
		orgForSEA2.CustomsCodes.AddNew(CodeTypes.CarrierCode, "3C34", Core.Constants.CountryCodes.Japan);
		Factory.Save();

		Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
		Declaration.JE_OH_ShippingLine = orgForAIR.PK;
		AssertEquals("1A", Declaration.JE_CarrierCode);

		Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
		Declaration.JE_OH_ShippingLine = orgForSEA1.PK;
		AssertEquals("2B23", Declaration.JE_CarrierCode);

		Declaration.JE_OH_ShippingLine = orgForSEA2.PK;
		AssertEquals("3C34", declaration.JE_CarrierCode);
	}

	public void TestJE_CarrierCodeChanged()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var carrier = helper.CreateCarrierCode("2B23", "WG TRUCKING", Core.Constants.CountryCodes.Japan);
		carrier.ZZ4_IsSea = true;
		Factory.Save();

		var orgForSEA1 = Factory.NewWithValidTestData<OrgHeader>();
		orgForSEA1.OH_IsShippingLine = true;
		orgForSEA1.OH_IsShippingProvider = true;
		orgForSEA1.OH_IsSeaWholesaler = true;
		var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
		shippingLine.RSL_IsShippingLine = true;
		shippingLine.RSL_StandardCarrierAlphaCode = "2B23";
		orgForSEA1.OH_RSL_ShippingLine = shippingLine.PK;

		var orgForSEA2 = Factory.NewWithValidTestData<OrgHeader>();
		orgForSEA2.OH_IsShippingLine = true;
		orgForSEA2.OH_IsShippingProvider = true;
		orgForSEA2.OH_IsSeaWholesaler = true;
		orgForSEA2.CustomsCodes.AddNew(CodeTypes.CarrierCode, "3C34", Core.Constants.CountryCodes.Japan);

		var orgForAIR = Factory.NewWithValidTestData<OrgHeader>();
		orgForAIR.OH_IsAirLine = true;
		orgForAIR.OH_IsShippingLine = true;
		orgForAIR.OH_IsShippingProvider = true;
		var airline = Factory.NewWithValidTestData<RefAirline>();
		airline.RM_TwoCharacterCode = "VF";
		airline.RM_IsActive = true;
		orgForAIR.MiscServ.OM_RM_Airline = airline.PK;
		Factory.Save();

		Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
		Declaration.JE_CarrierCode = "2B23";
		AssertEquals(orgForSEA1.PK, declaration.JE_OH_ShippingLine);

		Declaration.JE_OH_ShippingLine = ZGuid.Empty;
		Declaration.JE_CarrierCode = "3C34";
		AssertEquals(orgForSEA2.PK, declaration.JE_OH_ShippingLine);

		Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
		Declaration.JE_OH_ShippingLine = ZGuid.Empty;
		Declaration.JE_CarrierCode = "VF";
		AssertEquals(orgForAIR.PK, declaration.JE_OH_ShippingLine);
	}

	public void TestJE_ReceiptMode()
	{
		AssertEquals(2, Declaration.JE_ReceiptModeInfo.MaxLength);
	}

	public void TestJE_DeliveryMode()
	{
		AssertEquals(2, Declaration.JE_DeliveryModeInfo.MaxLength);
	}

	public void TestShipmentSynchroniser()
	{
		Declaration.JE_JS = Factory.New<ForwardingShipment>().PK;
		AssertType<JobDeclarationSynchroniser>(Declaration.ShipmentSynchroniser);
	}

	public void TestMessageSendingInProgress()
	{
		var messageSendingContext = new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.EDA };
		using (Declaration.SetCurrentMessageSendingContext(messageSendingContext))
		{
			CombineAssertions(() =>
			{
				AssertEquals("EDA", true, Declaration.IsEDASendingInProgress);
				AssertEquals("EDA01", false, Declaration.IsEDA01SendingInProgress);
				AssertEquals("IDA", false, Declaration.IsIDASendingInProgress);
				AssertEquals("IDA01", false, Declaration.IsIDA01SendingInProgress);
				AssertEquals("ECR", false, Declaration.IsECRSendingInProgress);
			});
		}

		messageSendingContext.ProcedureCode = JPProcedureCodeList.Codes.EDA01;
		using (Declaration.SetCurrentMessageSendingContext(messageSendingContext))
		{
			AssertEquals("EDA01", true, Declaration.IsEDA01SendingInProgress);
		}

		messageSendingContext.ProcedureCode = JPProcedureCodeList.Codes.IDA;
		using (Declaration.SetCurrentMessageSendingContext(messageSendingContext))
		{
			AssertEquals("IDA", true, Declaration.IsIDASendingInProgress);
		}

		messageSendingContext.ProcedureCode = JPProcedureCodeList.Codes.IDA01;
		using (Declaration.SetCurrentMessageSendingContext(messageSendingContext))
		{
			AssertEquals("IDA01", true, Declaration.IsIDA01SendingInProgress);
		}

		messageSendingContext.ProcedureCode = JPProcedureCodeList.Codes.ECR;
		using (Declaration.SetCurrentMessageSendingContext(messageSendingContext))
		{
			AssertEquals("ECR", true, Declaration.IsECRSendingInProgress);
		}
	}

	public void TestUpdateCEI_GoodsDescriptionWhenJE_GoodsDescriptionChanged()
	{
		var instruction1 = Declaration.CustomsEntryInstructions.AddNew();
		instruction1.CEI_GoodsDescription = "GoodsDescription";
		var instruction2 = Declaration.CustomsEntryInstructions.AddNew();

		Declaration.JE_GoodsDescription = "Desc";
		CombineAssertions(() =>
		{
			AssertEquals("instruction1.CEI_GoodsDescription", "GoodsDescription", instruction1.CEI_GoodsDescription);
			AssertEquals("instruction2.CEI_GoodsDescription", "Desc", instruction2.CEI_GoodsDescription);
		});
	}

	public void TestUpdateRadioCallSignWhenVesselChanged()
	{
		var refVessel1 = Factory.New<RefVessel>();
		refVessel1.RV_Code = "TESTV1";
		var refVessel2 = Factory.New<RefVessel>();
		refVessel2.RV_Code = "TESTV2";

		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateVesselZZ("TESTV1", "LXBH", "CV", "JP");
		Factory.Save();

		Declaration.JE_VesselName = "TESTV1";
		AssertEquals("LXBH", Declaration.JE_RadioCallSign);

		Declaration.JE_RadioCallSign = ZString.Empty;
		Declaration.JE_VesselName = "TESTV2";
		AssertEquals(ZString.Empty, Declaration.JE_RadioCallSign);
	}

	public void TestUpdateVesselWhenRadioCallSignChanged()
	{
		var refVessel = Factory.New<RefVessel>();
		refVessel.RV_Code = "TESTV1";

		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateVesselZZ("TESTV1", "LXBH", "CV", "JP");
		helper.CreateVesselZZ("TESTV2", "OVYQ2", "CV", "JP");
		Factory.Save();

		Declaration.JE_RadioCallSign = "LXBH";
		AssertEquals("TESTV1", Declaration.JE_VesselName);

		Declaration.JE_VesselName = ZString.Empty;
		Declaration.JE_RadioCallSign = "OVYQ2";
		AssertEquals(ZString.Empty, Declaration.JE_VesselName);
	}

	public void TestOverridingSupplierDocumentaryAddressOrImporterDocumentaryAddressDoesNotThrowException()
	{
		var shipment = Factory.New<ForwardingShipment>();
		var helper = new CreateDeclarationHelper();
		var consignor = Factory.NewWithValidTestData<OrgHeader>();
		var consignee = Factory.NewWithValidTestData<OrgHeader>();
		shipment.ConsignorPK = consignor.PK;
		shipment.ConsigneePK = consignee.PK;
		helper.CreateDeclaration(shipment);
		var declaration = JobDeclaration.Load(shipment);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals(declaration.SupplierDocumentaryAddress.OrganisationPK, consignor.PK);
			AssertEquals(declaration.ImporterDocumentaryAddress.OrganisationPK, consignee.PK);
			Assert(!declaration.SupplierDocumentaryAddress.E2_AddressOverride);
			Assert(!declaration.ImporterDocumentaryAddress.E2_AddressOverride);
		});

		AssertNoExceptionThrown(() =>
		{
			declaration.SupplierDocumentaryAddress.E2_AddressOverride = true;
			declaration.SupplierDocumentaryAddress.E2_AddressOverride = false;
			declaration.ImporterDocumentaryAddress.E2_AddressOverride = true;
			declaration.ImporterDocumentaryAddress.E2_AddressOverride = false;
		});
	}

	public void TestDefaultAdditionalReferenceNumbersFromShipment()
	{
		var shipment = Factory.New<ForwardingShipment>();
		var helper = new CreateDeclarationHelper();
		var consol = shipment.Consols.AddNew();
		var bookingReferenceOnConsol = consol.Numbers.AddNew();
		bookingReferenceOnConsol.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;
		bookingReferenceOnConsol.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
		bookingReferenceOnConsol.CE_EntryNum = "00001";

		helper.CreateDeclaration(shipment);
		var loadedDeclaration = JobDeclaration.Load(shipment);
		var bookingReference = loadedDeclaration.AdditionalReferenceNumbers.GetFirstReferenceNumberByTypeAndCountry(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG, Core.Constants.CountryCodes.Japan);
		AssertEquals("00001", bookingReference.CE_EntryNum);

		consol.JK_BookingReference = "00002";
		loadedDeclaration.JE_JS = ZGuid.Empty;
		helper.CreateDeclaration(shipment);
		loadedDeclaration = JobDeclaration.Load(shipment);
		bookingReference = loadedDeclaration.AdditionalReferenceNumbers.GetFirstReferenceNumberByTypeAndCountry(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG, Core.Constants.CountryCodes.Japan);
		AssertEquals("00002", bookingReference.CE_EntryNum);

		var bookingReferenceOnShipment = shipment.Numbers.AddNew();
		bookingReferenceOnShipment.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;
		bookingReferenceOnShipment.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
		bookingReferenceOnShipment.CE_EntryNum = "00003";
		loadedDeclaration.JE_JS = ZGuid.Empty;
		helper.CreateDeclaration(shipment);
		loadedDeclaration = JobDeclaration.Load(shipment);
		bookingReference = loadedDeclaration.AdditionalReferenceNumbers.GetFirstReferenceNumberByTypeAndCountry(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG, Core.Constants.CountryCodes.Japan);
		AssertEquals("00003", bookingReference.CE_EntryNum);
	}

	public void TestIsInvoicesRequiredToBeInSameCurrency()
	{
		AssertEquals(true, Declaration.IsInvoicesRequiredToBeInSameCurrency);
	}

	public void TestIsInvoicesRequiredToBeInSameIncoTerm()
	{
		AssertEquals(true, Declaration.IsInvoicesRequiredToBeInSameIncoTerm);
	}

	protected override BaseJobDeclaration GetJobDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		return declaration;
	}

	JobDeclaration Declaration
	{
		get
		{
			if (declaration == null)
			{
				declaration = Factory.New<JobDeclaration>();
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			}
			return declaration;
		}
	}

	JobDeclaration declaration;
}
