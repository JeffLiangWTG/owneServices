using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;
using MessageTypeList = Enterprise.Customs.EU.Business.MessageTypeList;
using RepresentationTypeList = Enterprise.Customs.EU.Business.RepresentationTypeList;

namespace Enterprise.Customs.FR.DocumentWrappers.SADH.Testing;

sealed class DocSADHTest : Enterprise.DocumentWrappers.Customs.EU.Testing.DocSADHTest
{
	public override int numberOfPages => 2;

	public override void TestTimeLimitDate()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var number = Factory.New<CusEntryNumber>();
		number.CE_IssueDate = new ZDateTime(1987, 12, 11);
		number.CE_ParentID = entryHeader.PK;
		number.CE_ParentTable = CusEntryHeader.Schema.TableName;
		number.CE_EntryType = "EXP";

		var wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("TimeLimitDate should be mapped to date of issue + 90 days", "10/03/1988", wrapper.TimeLimitDate);
	}

	public void TestIssuingDate()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var number = Factory.New<CusEntryNumber>();
		number.CE_IssueDate = new ZDateTime(1987, 12, 11);
		number.CE_ParentID = entryHeader.PK;
		number.CE_ParentTable = CusEntryHeader.Schema.TableName;
		number.CE_EntryType = "EXP";

		var wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("TimeLimitDate should be mapped to date of issue", "11/12/1987", wrapper.IssuingDate);
	}

	public void TestBox54Place()
	{
		var broker = Factory.New<GlbStaff>();
		broker.GS_City = "Lyon";
		broker.GS_Code = "BOK";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_GS_NKCusAgent = broker.GS_Code;
		AssertEquals("Prerequiste", "Brisbane", declaration.Branch.City);

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("Box 54 place should be mapped to declaration branch city.", "Brisbane", wrapper.Box54Place);
	}

	public void TestBox54SignatoryNameAndPosition()
	{
		var broker = Factory.New<GlbStaff>();
		broker.GS_City = "Lyon";
		broker.GS_Code = "BOK";
		broker.GS_FullName = "BOK isATest";
		broker.GS_Title = "Position";

		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("Box 54 SignatoryNameAndPosition should be empty as there is no broker.", ZString.Empty, wrapper.Box54SignatoryNameAndPosition);

		declaration.JE_GS_NKCusAgent = "NOK";
		AssertEquals("Box 54 SignatoryNameAndPosition should be empty as the broker code is wrong.", ZString.Empty, wrapper.Box54SignatoryNameAndPosition);

		declaration.JE_GS_NKCusAgent = broker.GS_Code;
		wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("Box 54 SignatoryNameAndPosition should be mapped to declaration JE_GS_NKCusAgent full name.", "BOK isATest", wrapper.Box54SignatoryNameAndPosition);
	}

	public override void TestBox19HasContainer()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("0", wrapper.Box19HasContainer);
		declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.FCL;
		AssertEquals("0", wrapper.Box19HasContainer);
		var container = declaration.CusContainers.AddNew();
		AssertEquals("0", wrapper.Box19HasContainer);
		container.CO_ContainerNumber = "CNT1";
		AssertEquals("1", wrapper.Box19HasContainer);

		declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.Containerised;
		AssertEquals("1", wrapper.Box19HasContainer);
		declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.ULD;
		AssertEquals("1", wrapper.Box19HasContainer);
		declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.Liquid;
		AssertEquals("1", wrapper.Box19HasContainer);
		declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.BuyersConsol;
		AssertEquals("0", wrapper.Box19HasContainer);
	}

	public void TestBox35GrossWeightInKG()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entryHeader1.MergedLines.AddNew();

		var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
		var entryLine2 = entryHeader2.MergedLines.AddNew();

		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.JZ_Weight = 35.0m;

		var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine1.JI_Weight = 10.0m;
		invoiceLine1.JI_CL = entryLine1.PK;

		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine2.JI_Weight = 20.0m;
		invoiceLine2.JI_CL = entryLine2.PK;

		var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine3.JI_Weight = 5.0m;
		invoiceLine3.JI_CL = entryLine2.PK;

		var wrapper1 = DocSADH.New(entryHeader1, Factory);
		AssertEquals("entryHeader1 Box35GrossWeightInKG should return formatted sum of weight of related invoice lines (10).", "10.000", wrapper1.Box35GrossWeightInKG);

		var wrapper2 = DocSADH.New(entryHeader2, Factory);
		AssertEquals("entryHeader1 Box35GrossWeightInKG should return formatted sum of weight of related invoice lines (20 + 5 = 25).", "25.000", wrapper2.Box35GrossWeightInKG);
	}

	public void TestLines()
	{
		var wrapper = GetNewDocumentWrapper() as DocSADH;
		AssertEquals(typeof(DocSADHLineCollection), wrapper.Lines.GetType());
	}

	public void TestPages()
	{
		var wrapper = GetNewDocumentWrapper() as DocSADH;
		AssertEquals(typeof(DocSADHPageCollection), wrapper.Pages.GetType());
	}

	public void TestBox2Supplier()
	{
		JobDeclaration declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		var supplier = Factory.New<OrgHeader>();
		supplier.OH_Code = "SUPPLIER";
		declaration.JE_OH_Supplier = supplier.PK;

		var supplierAddress = supplier.Addresses.AddNew();
		supplierAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
		supplierAddress.OA_Address1 = "address1";
		supplierAddress.OA_Address2 = "address2";
		supplierAddress.OA_City = "city";
		supplierAddress.OA_State = "state";
		supplierAddress.OA_PostCode = "111";
		supplierAddress.OA_RL_NKRelatedPortCode = "COBOG";
		declaration.SupplierDocumentaryAddress.E2_OA_Address = supplierAddress.PK;
		Factory.Save();

		var wrapper = DocSADH.New(entryHeader, Factory);
		AssertContains("ADDRESS1", wrapper.Box2Supplier.ToString());
		AssertContains("ADDRESS2", wrapper.Box2Supplier.ToString());
		AssertContains("CITY", wrapper.Box2Supplier.ToString());
		AssertContains("111", wrapper.Box2Supplier.ToString());
		AssertContains("STATE", wrapper.Box2Supplier.ToString());
		AssertContains("COLOMBIA", wrapper.Box2Supplier.ToString());

		var euAddInfo = EU.Business.EUOrgImpAddInfo.Get(supplier, Core.Constants.CountryCodes.France);
		euAddInfo.ZO_UseFr3FiscalRepresentation = true;

		var fiscalReferenceOrganisation = Factory.New<OrgHeader>();
		fiscalReferenceOrganisation.OH_Code = "FISCALREP";

		var fiscalReferenceOrganisationAddress = fiscalReferenceOrganisation.Addresses.AddNew();
		fiscalReferenceOrganisationAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
		fiscalReferenceOrganisationAddress.OA_Address1 = "fiscal address1";
		fiscalReferenceOrganisationAddress.OA_Address2 = "fiscal address2";
		fiscalReferenceOrganisationAddress.OA_City = "fiscal city";
		fiscalReferenceOrganisationAddress.OA_PostCode = "333";
		fiscalReferenceOrganisationAddress.OA_RN_NKCountryCode = "FR";

		var fiscalReference = entryInstruction.FiscalReferences.AddNew();
		fiscalReference.CFR_Code = "FR3";
		fiscalReference.CFR_Reference = "FR33562024100133";
		fiscalReference.CFR_OA_Owner = fiscalReferenceOrganisationAddress.PK;
		Factory.Save();
		AssertEquals(true, entryHeader.SupplierUsesFiscalRepresentative);

		wrapper = DocSADH.New(entryHeader, Factory);
		AssertContains("ADDRESS1", wrapper.Box2Supplier.ToString());
		AssertContains("ADDRESS2", wrapper.Box2Supplier.ToString());
		AssertContains("CITY", wrapper.Box2Supplier.ToString());
		AssertContains("111", wrapper.Box2Supplier.ToString());
		AssertContains("STATE", wrapper.Box2Supplier.ToString());
		AssertContains("COLOMBIA", wrapper.Box2Supplier.ToString());
	}

	public void TestBox8Importer()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		var importer = Factory.New<OrgHeader>();
		importer.OH_Code = "IMPORTER";
		declaration.JE_OH_Importer = importer.PK;

		var importerAddress = importer.Addresses.AddNew();
		importerAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
		importerAddress.OA_Address1 = "address1";
		importerAddress.OA_Address2 = "address2";
		importerAddress.OA_City = "city";
		importerAddress.OA_State = "state";
		importerAddress.OA_PostCode = "111";
		importerAddress.OA_RL_NKRelatedPortCode = "COBOG";
		declaration.ImporterDocumentaryAddress.E2_OA_Address = importerAddress.PK;
		Factory.Save();

		var wrapper = DocSADH.New(entryHeader, Factory);
		AssertContains("ADDRESS1", wrapper.Box8Importer.ToString());
		AssertContains("ADDRESS2", wrapper.Box8Importer.ToString());
		AssertContains("CITY", wrapper.Box8Importer.ToString());
		AssertContains("111", wrapper.Box8Importer.ToString());
		AssertContains("STATE", wrapper.Box8Importer.ToString());
		AssertContains("COLOMBIA", wrapper.Box8Importer.ToString());

		var euAddInfo = EU.Business.EUOrgImpAddInfo.Get(importer, Core.Constants.CountryCodes.France);
		euAddInfo.ZO_UseFr3FiscalRepresentation = true;

		var fiscalReferenceOrganisation = Factory.New<OrgHeader>();
		fiscalReferenceOrganisation.OH_Code = "FISCALREP";

		var fiscalReferenceOrganisationAddress = fiscalReferenceOrganisation.Addresses.AddNew();
		fiscalReferenceOrganisationAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
		fiscalReferenceOrganisationAddress.OA_Address1 = "fiscal address1";
		fiscalReferenceOrganisationAddress.OA_Address2 = "fiscal address2";
		fiscalReferenceOrganisationAddress.OA_City = "fiscal city";
		fiscalReferenceOrganisationAddress.OA_PostCode = "333";
		fiscalReferenceOrganisationAddress.OA_RN_NKCountryCode = "FR";

		var fiscalReference = entryInstruction.FiscalReferences.AddNew();
		fiscalReference.CFR_Code = "FR3";
		fiscalReference.CFR_Reference = "FR33562024100133";
		fiscalReference.CFR_OA_Owner = fiscalReferenceOrganisationAddress.PK;
		Factory.Save();
		AssertEquals(true, entryHeader.ImporterUsesFiscalRepresentative);

		wrapper = DocSADH.New(entryHeader, Factory);
		AssertContains("ADDRESS1", wrapper.Box8Importer.ToString());
		AssertContains("ADDRESS2", wrapper.Box8Importer.ToString());
		AssertContains("CITY", wrapper.Box8Importer.ToString());
		AssertContains("111", wrapper.Box8Importer.ToString());
		AssertContains("STATE", wrapper.Box8Importer.ToString());
		AssertContains("COLOMBIA", wrapper.Box8Importer.ToString());
	}

	public void TestBox8Importer_DeclarationHasNonStandardCountryOfDestination()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingDataGrouping("QR", parent: grouping);
		helper.CreateNewOrGetExistingCusCodeType("FR17", "Origin country/territory for entry style EX");
		helper.CreateCusCodeList(Constants.CountryCodes.France, "FR17", "QR", "QR Desc", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		declaration.JE_GoodsDestination = "QR";

		var importer = Factory.New<OrgHeader>();
		importer.OH_Code = "IMPORTER";
		declaration.JE_OH_Importer = importer.PK;

		var importerAddress = importer.Addresses.AddNew();
		importerAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
		importerAddress.OA_Address1 = "address1";
		importerAddress.OA_Address2 = "address2";
		importerAddress.OA_City = "city";
		importerAddress.OA_State = "state";
		importerAddress.OA_PostCode = "111";
		importerAddress.OA_RL_NKRelatedPortCode = "COBOG";
		declaration.ImporterDocumentaryAddress.E2_OA_Address = importerAddress.PK;
		Factory.Save();

		var wrapper = DocSADH.New(entryHeader, Factory);
		AssertContains("ADDRESS1", wrapper.Box8Importer.ToString());
		AssertContains("ADDRESS2", wrapper.Box8Importer.ToString());
		AssertContains("CITY", wrapper.Box8Importer.ToString());
		AssertContains("111", wrapper.Box8Importer.ToString());
		AssertContains("QR DESC", wrapper.Box8Importer.ToString());
	}

	public void TestBox2Supplier_DeclarationHasNonStandardCountryOfOrigin()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingDataGrouping(Constants.CountryCodes.France, parent: grouping);
		helper.CreateNewOrGetExistingCusCodeType("CO15", "Origin country/territory for entry style EX");
		helper.CreateCusCodeListWithAttribute(Constants.CountryCodes.France, "FR15", "QR", "Test ZZ", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, "EXPORT");
		helper.CreateCusCodeListWithAttribute(Constants.CountryCodes.France, "FR15", "FR", "Test FR", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, "IMPORT");
		helper.CreateCusCodeListWithAttribute(Constants.CountryCodes.France, "FR15", "QR", "Test ZZ", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, "IMPORT");
		helper.CreateCusCodeListWithAttribute(Constants.CountryCodes.France, "FR15", "FR", "Test FR", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, "EXPORT");
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		declaration.JE_EntryStyle = EU.Business.EntryStyleListImport.Codes.ImportFromSpecialTerritory;
		declaration.JE_GoodsDestination = "QR";

		var supplier = Factory.New<OrgHeader>();
		supplier.OH_Code = "IMPORTER";
		declaration.JE_OH_Supplier = supplier.PK;

		var supplierAddress = supplier.Addresses.AddNew();
		supplierAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
		supplierAddress.OA_Address1 = "address1";
		supplierAddress.OA_Address2 = "address2";
		supplierAddress.OA_City = "city";
		supplierAddress.OA_State = "state";
		supplierAddress.OA_PostCode = "111";
		supplierAddress.OA_RL_NKRelatedPortCode = "COBOG";
		declaration.SupplierDocumentaryAddress.E2_OA_Address = supplierAddress.PK;

		declaration.JE_GoodsOrigin = "QR";

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = DocSADH.New(entryHeader, Factory);
		AssertContains("ADDRESS1", wrapper.Box2Supplier.ToString());
		AssertContains("ADDRESS2", wrapper.Box2Supplier.ToString());
		AssertContains("CITY", wrapper.Box2Supplier.ToString());
		AssertContains("111", wrapper.Box2Supplier.ToString());
		AssertContains("TEST ZZ", wrapper.Box2Supplier.ToString());
	}

	[TestDate(2020, 01, 01, 12, 0, 0)]
	public void TestFallbackStampTitleAndText()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
		var cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();
		invoiceLine1.JI_CL = cusEntryLine.PK;

		var fallbackSetting = new FallbackSettings();
		fallbackSetting.End = ZDateTime.Empty;
		fallbackSetting.Start = ZDateTime.Today.AddDays(-1);
		FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);

		Factory.Save();

		DocSADH wrapper = DocSADH.New(cusEntryHeader, Factory);
		AssertEquals((ZString)"PROCÉDURE DE SECOURS / FALLBACK PROCEDURE", wrapper.FallbackStampTitle);
		AssertEquals((ZString)"AUCUNE DONNÉE DISPONIBLE DANS LE SYSTÈME\r\nENGAGÉE LE 31/12/2019 A 00:00", wrapper.FallbackStampText);

		fallbackSetting.End = ZDateTime.Empty;
		fallbackSetting.Start = ZDateTime.Today.AddDays(+2);
		FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);

		AssertEquals(ZString.Empty, wrapper.FallbackStampTitle);
		AssertEquals(ZString.Empty, wrapper.FallbackStampText);
	}

	protected override DocBaseWrapper GetNewDocumentWrapper()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		return DocSADH.New(entryHeader, Factory);
	}

	protected override ZString CountrySpecificCurrency { get { return "EUR"; } }

	protected override void SetUp()
	{
		base.SetUp();
		Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.France);
	}

	public override void TestBox14DeclarantRepresentative()
	{
		OrgHeader declarantOrgHeader = Factory.New<OrgHeader>();
		declarantOrgHeader.OH_FullName = "My Test org for Box14";

		OrgAddress declarantAddress = declarantOrgHeader.Addresses.AddNew();
		declarantAddress.OA_Address1 = "Address Line from Address against Org for Box14!";

		JobDeclaration declaration = Factory.New<JobDeclaration>();
		CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
		DocSADH wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("wrapper.Box14DeclarantRepresentative.CompanyName", "EDI", wrapper.Box14DeclarantRepresentative.CompanyName.Left(3));

		declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
		declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
		wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("wrapper.Box14DeclarantRepresentative.CompanyName - rep 2", declarantOrgHeader.OH_FullName, wrapper.Box14DeclarantRepresentative.CompanyName);
		AssertEquals("wrapper.Box14DeclarantRepresentative.Address1 - rep 2", declarantAddress.OA_Address1, wrapper.Box14DeclarantRepresentative.Address1);

		declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
		AssertEquals("wrapper.Box14DeclarantRepresentative.CompanyName - rep 3", declarantOrgHeader.OH_FullName, wrapper.Box14DeclarantRepresentative.CompanyName);
		AssertEquals("wrapper.Box14DeclarantRepresentative.Address1 - rep 3", declarantAddress.OA_Address1, wrapper.Box14DeclarantRepresentative.Address1);

		declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
		AssertNotNull("Box 14 should not be empty for self rep", wrapper.Box14DeclarantRepresentative);
	}

	public override void TestBox24TransactionNature()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.ZG_TransNature = "34";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		var entryLine = entryHeader.MergedLines.AddNew();

		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.JZ_ValuationCode = "12";
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		var wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("Box24TransactionNature is equal to entryInstruction ZG_TransNature instead of invoiceHeader JZ_ValuationCode", "34", wrapper.Box24TransactionNature);
	}

	public void TestBox29ExitOffice_IsUsedForEntryAsWell()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType("CUSOF", "CustomsOffice");
		helper.CreateCusCodeList("FR", "CUSOF", "FR002300", "FR002300 bureau", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		declaration.CustomsOffices.RemoveAndDeleteAll();
		declaration.CustomsOffices.AddNew(EU.Business.EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, "FR002300");

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("FR002300 - FR002300 bureau", wrapper.Box29ExitOffice);
	}

	public override void TestBox14DetailsFooter()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = DocSADH.New(entryHeader, Factory);
		declaration.JE_CustomsProfile = "DGE0023";
		declaration.JE_DeclarantType = Enterprise.Customs.FR.Business.RepresentationTypeList.Codes.IND;
		AssertEquals("N° agrément : DGE0023 - Représentation : 3 Indirect", wrapper.Box14DetailsFooter);
		declaration.JE_DeclarantType = Enterprise.Customs.FR.Business.RepresentationTypeList.Codes.DIR;
		AssertEquals("N° agrément : DGE0023 - Représentation : 2 Direct", wrapper.Box14DetailsFooter);
		declaration.JE_DeclarantType = Enterprise.Customs.FR.Business.RepresentationTypeList.Codes.SEL;
		AssertEquals("N° agrément : DGE0023 - Représentation : 1 Compte propre", wrapper.Box14DetailsFooter);
	}

	public override void TestBox10LabelPart1Caption()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("Box10LabelPart1Caption Test", "Cty.1st.dest", wrapper.Box10LabelPart1Caption);
	}

	public override void TestTitleEUCommunityLabelCaption()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("TitleEUCommunityLabelCaption Test", "EUROPEAN COMMUNITY - DAU", wrapper.TitleEUCommunityLabelCaption);
	}

	[TestDate(2008, 7, 1, 08, 09, 10)]
	public void TestBox54Date()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = DocSADH.New(entryHeader, Factory);

		AssertEquals((ZString)"01-Jul-08 08:09", wrapper.Box54Date);
	}

	public void TestBox54ValidationDate()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("If no event CES with REF 060 or REF 061, Box54ValidationDate should be empty", ZString.Empty, wrapper.Box54ValidationDate);

		entryHeader.Logs.AddNew(Events.CustomsEntryStatus, EntryStatusDescriptionCodeList.Codes.ES060, new ZDateTime(2008, 7, 1, 08, 09, 10).ToOffset());
		entryHeader.Logs.AddNew(Events.CustomsEntryStatus, EntryStatusDescriptionCodeList.Codes.ES060, new ZDateTime(2008, 7, 1, 07, 09, 10).ToOffset());
		wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("Box54ValidationDate should be date of last CES Event with REF 060 or REF 061", (ZString)"01-Jul-08 08:09", wrapper.Box54ValidationDate);

		entryHeader.Logs.AddNew(Events.CustomsEntryStatus, EntryStatusDescriptionCodeList.Codes.ES061, new ZDateTime(2008, 7, 1, 09, 09, 10).ToOffset());
		wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("Box54ValidationDate should be date of last CES Event with REF 060 or REF 061", (ZString)"01-Jul-08 09:09", wrapper.Box54ValidationDate);
	}

	public void TestBox54FirstReleaseDate()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_EntryReleaseDate = ZDateTime.Empty;
		var wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("Box54FirstReleaseDate should be equal to date CH_EntryReleaseDate", ZString.Empty, wrapper.Box54FirstReleaseDate);

		entryHeader.CH_EntryReleaseDate = new ZDateTime(2008, 7, 1, 08, 09, 10);
		wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("Box54FirstReleaseDate should be equal to date CH_EntryReleaseDate", (ZString)"01-Jul-08 08:09", wrapper.Box54FirstReleaseDate);
	}

	public void TestBox54LastAmendmentDate()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("If no event CES with REF 118, Box54LastAmendmentDate should be empty", ZString.Empty, wrapper.Box54LastAmendmentDate);

		entryHeader.Logs.AddNew(Events.CustomsEntryStatus, EntryStatusDescriptionCodeList.Codes.ES118, new ZDateTime(2008, 7, 1, 08, 09, 10).ToOffset());
		entryHeader.Logs.AddNew(Events.CustomsEntryStatus, EntryStatusDescriptionCodeList.Codes.ES118, new ZDateTime(2008, 7, 1, 07, 09, 10).ToOffset());
		wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("Box54LastAmendmentDate should be date of last CES Event with REF 118", (ZString)"01-Jul-08 08:09", wrapper.Box54LastAmendmentDate);
	}

	public void TestBox54InvalidationDate()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("If no event CES with REF 150, Box54InvalidationDate should be empty", ZString.Empty, wrapper.Box54InvalidationDate);

		entryHeader.Logs.AddNew(Events.CustomsEntryStatus, EntryStatusDescriptionCodeList.Codes.ES150, new ZDateTime(2008, 7, 1, 08, 09, 10).ToOffset());
		entryHeader.Logs.AddNew(Events.CustomsEntryStatus, EntryStatusDescriptionCodeList.Codes.ES150, new ZDateTime(2008, 7, 1, 07, 09, 10).ToOffset());
		wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("Box54InvalidationDate should be date of last CES Event with REF 150", (ZString)"01-Jul-08 08:09", wrapper.Box54InvalidationDate);
	}

	[TestDate(2008, 7, 1, 08, 09, 10)]
	public void TestBox54EditionDateLabel()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = DocSADH.New(entryHeader, Factory);

		AssertEquals("Date of Edition : ", wrapper.Box54EditionDateLabel);
	}

	public void TestBox54ValidationDateLabel()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("If no event CES with REF 060, Box54ValidationDate should be empty", ZString.Empty, wrapper.Box54ValidationDate);
		AssertEquals("If Box54ValidationDate is empty, Box54ValidationDateLabel should also be empty.", ZString.Empty, wrapper.Box54ValidationDateLabel);

		entryHeader.Logs.AddNew(Events.CustomsEntryStatus, EntryStatusDescriptionCodeList.Codes.ES060, new ZDateTime(2008, 7, 1, 08, 09, 10).ToOffset());
		entryHeader.Logs.AddNew(Events.CustomsEntryStatus, EntryStatusDescriptionCodeList.Codes.ES060, new ZDateTime(2008, 7, 1, 07, 09, 10).ToOffset());
		wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("Box54ValidationDate should be date of last CES Event with REF 060", (ZString)"01-Jul-08 08:09", wrapper.Box54ValidationDate);
		AssertEquals("If Box54ValidationDate is not empty, Box54ValidationDateLabel should be displayed.", "Date of Validation : ", wrapper.Box54ValidationDateLabel);
	}

	public void TestBox54FirstReleaseDateLabel()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_EntryReleaseDate = ZDateTime.Empty;
		var wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("Box54FirstReleaseDate should be equal to date CH_EntryReleaseDate", ZString.Empty, wrapper.Box54FirstReleaseDate);
		AssertEquals("If Box54FirstReleaseDate is empty, Box54FirstReleaseDateLabel should also be empty.", ZString.Empty, wrapper.Box54FirstReleaseDateLabel);

		entryHeader.CH_EntryReleaseDate = new ZDateTime(2008, 7, 1, 08, 09, 10);
		wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("Box54FirstReleaseDate should be equal to date CH_EntryReleaseDate", (ZString)"01-Jul-08 08:09", wrapper.Box54FirstReleaseDate);
		AssertEquals("If Box54FirstReleaseDate is not empty, Box54FirstReleaseDateLabel should be displayed.", "Date of first Release : ", wrapper.Box54FirstReleaseDateLabel);
	}

	public void TestBox54LastAmendmentDateLabel()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("If no event CES with REF 118, Box54LastAmendmentDate should be empty", ZString.Empty, wrapper.Box54LastAmendmentDate);
		AssertEquals("If Box54LastAmendmentDate is empty, Box54LastAmendmentDateLabel should also be empty.", ZString.Empty, wrapper.Box54LastAmendmentDateLabel);

		entryHeader.Logs.AddNew(Events.CustomsEntryStatus, EntryStatusDescriptionCodeList.Codes.ES118, new ZDateTime(2008, 7, 1, 08, 09, 10).ToOffset());
		entryHeader.Logs.AddNew(Events.CustomsEntryStatus, EntryStatusDescriptionCodeList.Codes.ES118, new ZDateTime(2008, 7, 1, 07, 09, 10).ToOffset());
		wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("Box54LastAmendmentDate should be date of last CES Event with REF 118", (ZString)"01-Jul-08 08:09", wrapper.Box54LastAmendmentDate);
		AssertEquals("If Box54LastAmendmentDate is not empty, Box54LastAmendmentDateLabel should be displayed.", "Date of last Amendment : ", wrapper.Box54LastAmendmentDateLabel);
	}

	public void TestBox54InvalidationDateLabel()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("If no event CES with REF 150, Box54InvalidationDate should be empty", ZString.Empty, wrapper.Box54InvalidationDate);
		AssertEquals("If Box54InvalidationDate is empty, Box54InvalidationDateLabel should also be empty.", ZString.Empty, wrapper.Box54InvalidationDateLabel);

		entryHeader.Logs.AddNew(Events.CustomsEntryStatus, EntryStatusDescriptionCodeList.Codes.ES150, new ZDateTime(2008, 7, 1, 08, 09, 10).ToOffset());
		entryHeader.Logs.AddNew(Events.CustomsEntryStatus, EntryStatusDescriptionCodeList.Codes.ES150, new ZDateTime(2008, 7, 1, 07, 09, 10).ToOffset());
		wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("Box54InvalidationDate should be date of last CES Event with REF 150", (ZString)"01-Jul-08 08:09", wrapper.Box54InvalidationDate);
		AssertEquals("If Box54InvalidationDate is not empty, Box54InvalidationDateLabel should be displayed.", "Date of Invalidation : ", wrapper.Box54InvalidationDateLabel);
	}

	public override void TestShowExportAccompanyingDocument()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals(false, wrapper.ShowExportAccompanyingDocument);
	}

	public override void TestBox15SupplierState()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = DocSADH.New(entryHeader, Factory);

		var customsOffice = Factory.New<ZZRefCusCodeListCombined>();
		customsOffice.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.France;
		customsOffice.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
		customsOffice.ZZD_Code = "FR002300";
		customsOffice.ZZD_StartDate = ZDateTime.Today;
		customsOffice.ZZD_EndDate = ZDateTime.Today.AddYears(1);

		var attribute = customsOffice.Attributes.AddNew();
		attribute.ZZE_Value = "12345";
		attribute.ZZE_ZXE_NKName = "PostCode";

		var unloco1 = Factory.New<RefUNLOCO>();
		unloco1.RL_RN_NKCountryCode = "FR";
		unloco1.RL_Code = "FRCD1";

		var unloco2 = Factory.New<RefUNLOCO>();
		unloco2.RL_RN_NKCountryCode = "IT";
		unloco2.RL_Code = "ITCD1";
		Factory.Save();

		var state = Factory.NewWithValidTestData<RefCountryStates>();
		state.RW_Code = "76";
		state.RW_Description = "Le Havre Port";
		state.RW_RN_NKCountryCode = "FR";

		var state2 = Factory.NewWithValidTestData<RefCountryStates>();
		state2.RW_Code = "13";
		state2.RW_Description = "IT Test";
		state2.RW_RN_NKCountryCode = "IT";

		declaration.JE_CustomsOffice = "FR002300";
		AssertEquals("When JE_RL_NKOrigin is empty, Box15SupplierState should fallback to the 2 first digits of the Customs Office post code.", "12", wrapper.Box15SupplierState);

		declaration.JE_RL_NKOrigin = "ITCD1";
		declaration.Origin.RL_RW = state2.PK;
		AssertEquals("When JE_RL_NKOrigin is not empty but port is not in France, Box15SupplierState should fallback to the 2 first digits of the Customs Office post code.", "12", wrapper.Box15SupplierState);

		declaration.JE_RL_NKOrigin = "FRCD1";
		declaration.Origin.RL_RW = state.PK;
		AssertEquals("Box15SupplierState should match the state code of France if the port is in France.", "76", wrapper.Box15SupplierState);
	}

	public void TestBox17ImporterState()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = DocSADH.New(entryHeader, Factory);
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		declaration.ImporterDeliveryAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.France;
		declaration.ImporterDeliveryAddress.E2_State = "95";
		declaration.ImporterDocumentaryAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
		declaration.ImporterDocumentaryAddress.E2_State = "GTL";
		AssertEquals("95", wrapper.Box17ImporterState);

		declaration.ImporterDeliveryAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
		AssertEquals("GTL", wrapper.Box17ImporterState);

		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		AssertEquals(ZString.Empty, wrapper.Box17ImporterState);
	}

	public void TestBox33ECSupplementCaption()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("1st CACO", wrapper.Box33ECSupplementCaption);
	}

	public void TestBox33ECSupplement2Caption()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("2nd CACO", wrapper.Box33ECSupplement2Caption);
	}

	public override void TestBox47aCaption()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = DocSADH.New(entryHeader, Factory);

		AssertEquals("<Box47aCaption>", "T. Nat/T.Com", wrapper.Box47aCaption);
	}

	public override void TestBox47aMethodPaiement()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = DocSADH.New(entryHeader, Factory);

		AssertEquals("<Box47aMethodPayment>", "Stat./MP", wrapper.Box47eMethodPayment);
	}

	public void TestBoxBAccountingDetailsCore()
	{
		var wrapper = GetDocSADH();

		AssertEquals("<BoxBAccountingDetails>",
			@"Liquidation CE (1) 123456
Liquidation NC (2)
Total à payer
Total AI2 (3)
COD garantie (4) REFA
Taxes non perçues (5)
Autoliquidation (6) ", wrapper.BoxBAccountingDetails);
	}

	public void TestBoxBAccountingDetailsValuesCore()
	{
		var wrapper = GetDocSADH();

		AssertEquals("<BoxBAccountingDetailsValues>",
			@"1
155 555 771
155 555 772
0
0
0
0", wrapper.BoxBAccountingDetailsValues);

		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var line = entryHeader.MergedLines.AddNew();
		var fee = line.ConfirmedFees.AddNew();
		fee.CF_MethodOfPayment = "4";
		fee.CF_ChargeAmount = 3;
		wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("<BoxBAccountingDetailsValues>",
			@"0
0
0
0
3
0
0", wrapper.BoxBAccountingDetailsValues);

		entryHeader.CH_ConfirmedGuaranteeAmount = 10;
		wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("<BoxBAccountingDetailsValues>",
			@"0
0
0
0
10
0
0", wrapper.BoxBAccountingDetailsValues);
	}

	DocSADH GetDocSADH()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var currentCountry = Core.Constants.CountryCodes.France;
		var eunId = helper.CreateNewOrGetExistingDataGrouping(currentCountry);
		Factory.Save();

		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.ZG_VATDeferType = "2";
		declaration.ZG_VATDeferNumber = "0000001";

		declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;

		var declarant = Factory.NewWithValidTestData<OrgHeader>();
		declarant.OH_Code = "NJG";
		SetupAccount(declarant, Enterprise.Customs.FR.Business.OrgCusAccountCodeList.Codes.DGE, OrgCusAccountDeltaGTypeList.Codes.G1, "DGE001", ZString.Empty, ZString.Empty, "1274BC4E");

		var declarantAddress = declarant.Addresses.AddNew();
		declarantAddress.AddressCode = "TestMatchAddress";
		declarantAddress.Address1 = "TestMatchAddress";

		CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "DGUA", declarant.PK, "ADD", "TestMatchAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "REFA", Core.Constants.CountryCodes.France);
		Factory.Save();

		declaration.JE_OA_DeclarantAddress = declarantAddress.PK;

		declaration.JE_DefermentAccountNumber = "123456";

		Factory.Save();

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var charge1 = entryHeader.ConfirmedCharges.AddOrUpdate("B001");
		charge1.C1_MethodOfPayment = "1";
		charge1.C1_ChargeAmount = 1.1m;
		var charge2 = entryHeader.ConfirmedCharges.AddOrUpdate("B002");
		charge2.C1_MethodOfPayment = "2";
		charge2.C1_ChargeAmount = 2.2m;
		var charge3 = entryHeader.ConfirmedCharges.AddOrUpdate("B003");
		charge3.C1_MethodOfPayment = "2";
		charge3.C1_ChargeAmount = 3.3m;
		var charge4 = entryHeader.Charges.AddNew("B004", 44m);
		charge4.C1_MethodOfPayment = "2";
		charge4.C1_ChargeAmount = 11m;

		var entryLine = entryHeader.MergedLines.AddNew();
		var entryLine2 = entryHeader.MergedLines.AddNew();

		var fee = entryLine.ConfirmedFees.AddOrUpdate("GCDP", 10.01); //A00
		fee.CF_MethodOfPayment = "2";
		fee.NationalFeeTypeCode = "GCDP";
		fee.CF_Source = Enterprise.Customs.Business.CusEntryLineFeeSourceCodeList.Codes.CUS;

		var fee1 = entryLine.ConfirmedFees.AddOrUpdate("A435", 20.02); //B00
		fee1.CF_MethodOfPayment = "2";
		fee1.NationalFeeTypeCode = "A435";
		fee1.CF_Source = Enterprise.Customs.Business.CusEntryLineFeeSourceCodeList.Codes.CUS;

		var fee2 = entryLine.ConfirmedFees.AddOrUpdate("N112", 30.03); // DROIT DE PORT DCN
		fee2.CF_MethodOfPayment = "2";
		fee2.NationalFeeTypeCode = "N112";
		fee2.CF_Source = Enterprise.Customs.Business.CusEntryLineFeeSourceCodeList.Codes.CUS;

		var fee3 = entryLine.ConfirmedFees.AddOrUpdate("U235", 40.4); // Antidumping DAD
		fee3.CF_MethodOfPayment = "2";
		fee3.NationalFeeTypeCode = "U235";
		fee3.CF_Source = Enterprise.Customs.Business.CusEntryLineFeeSourceCodeList.Codes.CUS;

		var fee4 = entryLine.ConfirmedFees.AddOrUpdate("K937", 50.2); // OCTROI DE MER ODM
		fee4.CF_MethodOfPayment = "2";
		fee4.NationalFeeTypeCode = "K937";
		fee4.CF_Source = Enterprise.Customs.Business.CusEntryLineFeeSourceCodeList.Codes.CUS;

		var fee5 = entryLine.ConfirmedFees.AddOrUpdate("L295", 60.6); // OTHER
		fee5.CF_MethodOfPayment = "2";
		fee5.NationalFeeTypeCode = "L295";
		fee5.CF_Source = Enterprise.Customs.Business.CusEntryLineFeeSourceCodeList.Codes.CUS;

		var fee6 = entryLine2.ConfirmedFees.AddOrUpdate("GCTG", 155555555.2); //A00
		fee6.CF_MethodOfPayment = "2";
		fee6.NationalFeeTypeCode = "GCTG";
		fee6.CF_Source = Enterprise.Customs.Business.CusEntryLineFeeSourceCodeList.Codes.CUS;

		var merger = new Customs.Business.LineMerger(declaration);
		merger.PopulateTotalAmountPayableForCusEntryHeaders();

		Factory.Save();

		var wrapper = DocSADH.New(entryHeader, Factory);
		return wrapper;
	}

	public static CusGuaranteeHeader CreateGuaranteeHeader(BusinessObjectFactory factory, ZString type, ZString number, ZGuid orgHeaderPK, ZString ruleCode, ZString ruleValue, ZString additionalReferenceCode, ZString additionalreferenceData, ZString countryCode)
	{
		var result = factory.New<CusGuaranteeHeader>();
		result.CPH_OH_PermitHolder = orgHeaderPK;
		result.CPH_RN_NKCountryCode = countryCode;
		result.CPH_StartDate = ZDate.Today;
		result.CPH_Type = type;
		result.CPH_Number = number;
		if (!string.IsNullOrEmpty(ruleCode))
		{
			var rule1 = result.CusGuaranteeRules.AddNew();
			rule1.CPR_RuleCode = ruleCode;
			rule1.CPR_ValueFrom = ruleValue;
		}

		if (type == GuaranteeTypeList.Codes.DEF || type == GuaranteeTypeList.Codes.COD)
		{
			var rule1 = result.CusGuaranteeRules.AddNew();
			rule1.CPR_RuleCode = PermitRuleCodeList.Codes.ENT;
			rule1.CPR_ValueFrom = GuaranteeEntryTypeList.Codes.BTH;
		}

		if (!string.IsNullOrEmpty(additionalReferenceCode))
		{
			var additionalRef = result.AdditionalGuaranteeReferences.AddNew();
			additionalRef.CY_Code = additionalReferenceCode;
			additionalRef.CY_Data = additionalreferenceData;
		}

		return result;
	}

	OrgCusAccount SetupAccount(OrgHeader header, ZString code, ZString type, ZString account, ZString issuer, ZString reportingPeriod, ZString representativeId)
	{
		var orgCusAccount = header.Factory.New<OrgCusAccount>();
		orgCusAccount.CZ_Code = code;
		orgCusAccount.CZ_Type = type;
		orgCusAccount.CZ_OH = header.PK;
		orgCusAccount.CZ_Account = account;
		orgCusAccount.CZ_Issuer = issuer;
		orgCusAccount.CZ_ReportingPeriod = reportingPeriod;
		orgCusAccount.CZ_RepresentativeID = representativeId;
		orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
		return orgCusAccount;
	}

	protected override void SetCustomsOfficeOfExit(EU.Business.Declaration.JobDeclaration declaration, ZString customsOffice)
	{
		declaration.CustomsOffices
			.Cast<EU.Business.EuOfficeCode>()
			.FirstOrDefault(x => x.CY_Code == EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit)
			.CY_Data = customsOffice;
	}

	public void TestShowBoxADetails()
	{
		var fallbackSetting = new FallbackSettings();
		fallbackSetting.Start = DateTime.Today.AddMonths(-1);
		fallbackSetting.End = DateTime.Today.AddMonths(1);
		FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);

		var wrapper = GetDeclaration();
		wrapper.Declaration.JE_LocationOfGoods = "AAAA";
		AssertEquals("<ShowBoxADetails>", true, wrapper.ShowBoxADetails);

		fallbackSetting = (FallbackSettings)FRCustomsDataRegistry.Instance.DeltaGMode.Value;
		fallbackSetting.End = DateTime.Today.AddDays(-1);
		AssertEquals("<ShowBoxADetails>", false, wrapper.ShowBoxADetails);

		fallbackSetting.End = DateTime.Today.AddMonths(1);
		wrapper.Declaration.JE_LocationOfGoods = "";
		AssertEquals("<ShowBoxADetails>", false, wrapper.ShowBoxADetails);

		wrapper.Declaration.JE_LocationOfGoods = "AAAA";
		wrapper.Declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		AssertEquals("<ShowBoxADetails>", false, wrapper.ShowBoxADetails);
	}

	public void TestBoxADetails()
	{
		// Declarant OrgHeader
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "FRD";
		orgHeader.OH_FullName = "France Declarant";

		// Declarant Address :  JE_OA_DeclarantAddress
		var declarantAddress = orgHeader.MainAddress;
		declarantAddress.AddressCode = "FRD Main Address";
		declarantAddress.Address1 = "FRD Main Address";
		declarantAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;

		//DeltaGAccounts : JE_CustomsProfile + JE_DeltaMode
		var orgCusAccount = Factory.New<OrgCusAccount>();
		orgCusAccount.CZ_OH = orgHeader.PK;
		orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGE;
		orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
		orgCusAccount.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G1; // JE_DeltaMode
		orgCusAccount.CZ_Account = "DGE001"; // JE_CustomsProfile

		//Auth : JE_LocationOfGoods
		var auth = Factory.NewWithValidTestData<Customs.Business.CusAuthorisationHeader>();
		auth.CPH_OH_PermitHolder = orgHeader.PK;
		auth.CPH_OA_AppliesTo = orgHeader.MainAddress.PK;
		auth.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
		auth.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation;
		auth.CPH_Number = "CPHNumber0001"; // JE_LocationOfGoods

		// Auth office
		var authRule = auth.CusAuthorisationRules.AddNew();
		authRule.CPR_RuleCode = Enterprise.Customs.FR.Business.CusAuthorisationRuleTypeList.Codes.OFC;
		authRule.CPR_ValueFrom = "221110";

		//Custom code :  auth office
		var cusCode = Factory.New<ZZRefCusCodeListCombined>();
		cusCode.ZZD_Code = authRule.CPR_ValueFrom;
		cusCode.ZZD_Description = "France Declarant Office";
		cusCode.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.France;
		cusCode.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
		cusCode.ZZD_StartDate = DateTime.Now.AddDays(-1);
		cusCode.ZZD_EndDate = DateTime.Now.AddDays(1);
		Factory.Save();

		// Declaration & Entry
		var fallbackSetting = new FallbackSettings();
		fallbackSetting.Start = DateTime.Today.AddMonths(-1);
		fallbackSetting.End = DateTime.Today.AddMonths(1);
		FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);
		var wrapper = GetDeclaration();
		var declaration = wrapper.Declaration;
		var entryHeader = declaration.CustomsEntryHeaders.FirstOrDefault();
		entryHeader.FRCustomsFallbackNumber = "FBN1234";
		declaration.JE_OA_DeclarantAddress = orgHeader.MainAddress.PK;
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
		declaration.JE_CustomsProfile = "DGE001";
		declaration.JE_LocationOfGoods = "CPHNumber0001";
		//modify here
		ZStringBuilder boxADetails = new ZStringBuilder();
		boxADetails.Append($"| {Core.Constants.CountryCodes.France}  |  221110 France Declarant Office\n");
		boxADetails.Append($"| FBN1234  |  {ZDateTime.Today.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture)}\n");
		boxADetails.Append($"| France Declarant  |  CPHNumber0001");
		AssertEquals("BoxADetails When Export , LocationOfGoods and FallBack ", boxADetails.ToString(), wrapper.BoxADetails);

		declaration.JE_LocationOfGoods = "";
		AssertEquals("BoxADetails When not Export , LocationOfGoods and FallBack ", "", wrapper.BoxADetails);
	}
	public void TestIsDeltaG1ExportFallBack()
	{
		var wrapper = GetDeclaration();
		AssertEquals("Is DeltaG1 Export FallBack", true, wrapper.IsDeltaG1ExportFallBack);

		wrapper.Declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		wrapper.Declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
		AssertEquals("Is DeltaG1 Export FallBack", false, wrapper.IsDeltaG1ExportFallBack);

		wrapper.Declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
		AssertEquals("Is DeltaG1 Export FallBack", false, wrapper.IsDeltaG1ExportFallBack);

		wrapper.Declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		wrapper.Declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
		AssertEquals("Is DeltaG1 Export FallBack", false, wrapper.IsDeltaG1ExportFallBack);
	}

	DocSADH GetDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		var wrapper = DocSADH.New(entryHeader, Factory);
		return wrapper;
	}

	public void TestLayoutStyle2()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		var wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("LayoutStyle2 when DeltaG1, Export and FallBack", "3", wrapper.LayoutStyle2);
		wrapper.Declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
		AssertEquals("LayoutStyle2 when not DeltaG1, Export and FallBack", "6", wrapper.LayoutStyle2);
	}

	public void TestLayoutStyle2Description()
	{
		var wrapper = GetDeclaration();
		AssertEquals("LayoutStyle2Description when DeltaG1, Export and FallBack", "EXEMPLAIRE POUR L’EXPIDITEUR/L’EXPORTATEUR", wrapper.LayoutStyle2Description);
		wrapper.Declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
		AssertEquals("LayoutStyle2Description when not DeltaG1, Export and FallBack", "Copy for the country of destination", wrapper.LayoutStyle2Description);
	}

	public override void TestBox17CountryOfDestination()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingDataGrouping(Constants.CountryCodes.France, parent: grouping);
		helper.CreateNewOrGetExistingCusCodeType("FR17", "Origin country/territory for entry style EX");
		helper.CreateCusCodeList(Constants.CountryCodes.France, "FR17", "ZZ", "Test ZZ", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		declaration.JE_EntryStyle = EU.Business.EntryStyleListImport.Codes.ImportFromSpecialTerritory;
		declaration.JE_GoodsDestination = "ZZ";

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = DocSADH.New(entryHeader, Factory);

		AssertEquals("Test Box17CountryOfDestination data", ZString.Empty, wrapper.Box17CountryOfDestination);

		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		AssertEquals("Test Box17CountryOfDestination data", "Test ZZ", wrapper.Box17CountryOfDestination);
	}

	public override void TestBox15CountryOfOrigin()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingDataGrouping(Constants.CountryCodes.France, parent: grouping);
		helper.CreateNewOrGetExistingCusCodeType("CO15", "Origin country/territory for entry style EX");
		helper.CreateCusCodeListWithAttribute(Constants.CountryCodes.France, "CO15", "ZZ", "Test ZZ", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, "EXPORT");
		helper.CreateCusCodeListWithAttribute(Constants.CountryCodes.France, "CO15", "FR", "Test FR", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, "IMPORT");
		helper.CreateCusCodeListWithAttribute(Constants.CountryCodes.France, "CO15", "ZZ", "Test ZZ", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, "IMPORT");
		helper.CreateCusCodeListWithAttribute(Constants.CountryCodes.France, "CO15", "FR", "Test FR", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, "EXPORT");
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		declaration.JE_EntryStyle = EU.Business.EntryStyleListImport.Codes.ImportFromSpecialTerritory;
		declaration.JE_GoodsOrigin = "ZZ";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("Box15CountryOfOrigin is equal to the country description of JE_GoodsOrigin when the country of JE_RL_NKOrigin is not France", "Test ZZ", wrapper.Box15CountryOfOrigin);

		declaration.JE_GoodsOrigin = Core.Constants.CountryCodes.France;
		AssertEquals("Box15CountryOfOrigin should be empty when the country of JE_RL_NKOrigin is France and MessageType is EXP", ZString.Empty, wrapper.Box15CountryOfOrigin);

		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		declaration.JE_EntryStyle = EU.Business.EntryStyleListImport.Codes.ImportFromSpecialTerritory;
		AssertEquals("Box15CountryOfOrigin is equal to the country description of JE_GoodsOrigin when MessageType is IMP", "Test FR", wrapper.Box15CountryOfOrigin);
	}

	public void TestBox15GoodsOrigin()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_GoodsOrigin = Constants.CountryCodes.Germany;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = DocSADH.New(entryHeader, Factory);
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		AssertEquals("Box15GoodsOrigin is equal to JE_GoodsOrigin when JE_GoodsOrigin is not FR", Constants.CountryCodes.Germany, wrapper.Box15GoodsOrigin);

		declaration.JE_GoodsOrigin = Constants.CountryCodes.France;
		AssertEquals("Box15GoodsOrigin should be empty when JE_GoodsOrigin is FR and MessageType is EXP", ZString.Empty, wrapper.Box15GoodsOrigin);

		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		AssertEquals("Box15GoodsOrigin is equal to JE_GoodsOrigin when MessageType is IMP", Constants.CountryCodes.France, wrapper.Box15GoodsOrigin);
	}

	public override void TestBox31PackagesAndDescriptionOfGoodsCaption()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("Box31PackagesAndDescriptionOfGoodsCaption Test", "Marques et numéros - No(s) conteneur(s) - Nombre et nature", wrapper.Box31PackagesAndDescriptionOfGoodsCaption);
	}

	public void TestBox35Are3decimals()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceheader = declaration.Invoices.AddNew();

		var invoiceLine1 = invoiceheader.InvoiceLines.AddNew();
		var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
		var cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();
		invoiceLine1.JI_CL = cusEntryLine.PK;

		invoiceLine1.JI_Weight = 2m;
		var wrapper = DocSADH.New(cusEntryHeader, Factory);
		AssertEquals("Gross weight should be formatted with 3 decimals.", "2.000", wrapper.Box35GrossWeightInKG);

		invoiceLine1.JI_Weight = 2.511999m;
		AssertEquals("Gross weight should be formatted with 3 decimals.", "2.512", wrapper.Box35GrossWeightInKG);
	}

	public override void TestBox54Details_Box54NameOfDeclarantAndRepresentative()
	{
		var broker = CreateBroker("F_N");
		var (declaration, entryHeader) = CreateEntryHeader(broker.GS_Code);

		var wrapper = DocSADH.New((CusEntryHeader)entryHeader, Factory);

		CombineAssertions(() =>
		{
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			AssertEquals("entryHeader.Box54NameOfDeclarantAndRepresentative", "EDI CUSTOMS BROKERS", wrapper.Box54NameOfDeclarantAndRepresentative);

			declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			AssertEquals("entryHeader.Box54NameOfDeclarantAndRepresentative", "Some Big Company Ltd", wrapper.Box54NameOfDeclarantAndRepresentative);

			declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
			AssertEquals("entryHeader.Box54NameOfDeclarantAndRepresentative", "Some Big Company Ltd", wrapper.Box54NameOfDeclarantAndRepresentative);

			var oh = Factory.New<OrgHeader>();
			oh.OH_FullName = "Daniel Declaring party";
			var oa = oh.Addresses.AddNewMainAddress();
			declaration.JE_OA_DeclarantAddress = oa.PK;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			AssertEquals("entryHeader.Box54NameOfDeclarantAndRepresentative", "Daniel Declaring party", wrapper.Box54NameOfDeclarantAndRepresentative);
		});
	}

	public void TestBox54UserDetailsFR()
	{
		var declaration = Factory.New<JobDeclaration>();

		var branch = Factory.New<GlbBranch>();
		branch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.France;
		branch.GB_RL_NKHomePort = "FRMAD";
		branch.GB_BranchName = "Joker";
		branch.GB_Phone = "01 01 01 01 01";
		branch.GB_Fax = "01 01 01 01 02";
		declaration.JE_GB = branch.PK;

		var staff = Factory.New<GlbStaff>();
		staff.GS_Code = "AT";
		staff.GS_FullName = "anonymous Test";
		staff.GS_LoginName = "attest";
		staff.StaffPlainTextPassword = "1234";
		staff.GS_WorkPhone = "01 01 01 01 03";
		staff.GS_RN_NKCountryCode = Core.Constants.CountryCodes.France;

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		CombineAssertions(() =>
		{
			var wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals("entryHeader.Box54SignatoryNameAndPosition: No Message and CusAgent", "", wrapper.Box54SignatoryNameAndPosition);

			wrapper = DocSADH.New(entryHeader, Factory);
			declaration.JE_GS_NKCusAgent = staff.GS_Code;
			AssertEquals("entryHeader.Box54SignatoryNameAndPosition: has CusAgent", "anonymous Test", wrapper.Box54SignatoryNameAndPosition);
		});
	}

	public void TestBoxDBarCode()
	{
		JobDeclaration declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		DocSADH wrapper = DocSADH.New(entryHeader, Factory);

		declaration.JE_GoodsOrigin = Core.Constants.CountryCodes.UnitedKingdom;
		entryHeader.EntryNumber = "071-041987W";
		AssertEquals("wrapper.ENO", "041987W", wrapper.ENO);
		AssertEquals("041987W", wrapper.BoxDBarcode);

		declaration.JE_GoodsOrigin = Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes;
		entryHeader.EntryNumber = "071041987W";
		AssertEquals("wrapper.ENO", "071041987W", wrapper.ENO);
		AssertEquals("071041987W", wrapper.BoxDBarcode);

		declaration.JE_GoodsOrigin = Core.Constants.CountryCodes.Ireland;
		entryHeader.EntryNumber = "071041987W";
		AssertEquals("wrapper.ENO", "071041987W", wrapper.ENO);
		AssertEquals("071041987W", wrapper.BoxDBarcode);

		declaration.JE_GoodsOrigin = Core.Constants.CountryCodes.France;
		entryHeader.EntryNumber = "071041987W";
		AssertEquals("wrapper.ENO", "071041987W", wrapper.ENO);
		AssertEquals("", wrapper.BoxDBarcode);
	}

	public void TestBoxDBarCodeCaption()
	{
		var documentWrapper = (DocSADH)GetNewDocumentWrapper();
		AssertEquals(@"Code Barre à scanner avant embarquement
Barcode to be scanned before boarding", documentWrapper.BoxDBarcodeCaption);
	}

	[TestDate(2008, 6, 8)]
	public override void TestBox22CurrencyAndTotalAmountInvoice()
	{
		RefCurrency usd = RefCurrency.LoadFromCurrencyCode(Factory, Enterprise.Core.Constants.CurrencyCodes.UnitedStates);
		RefCurrency aud = RefCurrency.LoadFromCurrencyCode(Factory, Enterprise.Core.Constants.CurrencyCodes.Australia);

		JobDeclaration declaration = Factory.New<JobDeclaration>();
		CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var entryline1 = entryHeader.MergedLines.AddNew();
		entryline1.CL_InvoiceAmount = 10;
		entryline1.CL_RX_NKInvoiceAmountCurrency = usd.RX_Code;

		var entryline2 = entryHeader.MergedLines.AddNew();
		entryline2.CL_InvoiceAmount = 10;
		entryline2.CL_RX_NKInvoiceAmountCurrency = usd.RX_Code;
		Factory.Save();

		DocSADH wrapper = DocSADH.New(entryHeader, Factory);
		AssertEquals("wrapper.Box22TotalPriceInvoiced", 20.00m, wrapper.Box22TotalPriceInvoiced);
		AssertEquals("wrapper.Box22TotalPriceCurrency", "USD", wrapper.Box22TotalPriceCurrency);

		entryline1.CL_RX_NKInvoiceAmountCurrency = aud.RX_Code;
		Factory.Save();

		AssertEquals("wrapper.Box22TotalPriceInvoiced", 12.09m, wrapper.Box22TotalPriceInvoiced);
		AssertEquals("wrapper.Box22TotalPriceCurrency", CountrySpecificCurrency, wrapper.Box22TotalPriceCurrency);
	}

	[TestDate(2008, 7, 1)]
	public override void TestDOE()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = DocSADH.New(entryHeader, Factory);
		var message = entryHeader.Messages.AddNew();
		message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
		message.EM_SystemCreateTimeUtc = new ZDateTime(2008, 7, 2);
		AssertEquals("If no event CES with REF 060, DOE should be last transmitted message datetime", new ZDateTime(2008, 7, 1), wrapper.DOE);
		message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
		AssertEquals("If no event CES with REF 060, DOE should be last transmitted message datetime", new ZDateTime(2008, 7, 2), wrapper.DOE);

		entryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
		entryHeader.Logs.AddNew(Events.CustomsEntryStatus, EntryStatusDescriptionCodeList.Codes.ES060, new ZDateTime(2022, 10, 19).ToOffset());
		entryHeader.Logs.AddNew(Events.CustomsEntryStatus, EntryStatusDescriptionCodeList.Codes.ES060, new ZDateTime(2022, 10, 20).ToOffset());
		AssertEquals("DOE should be date of last CES Event with REF 060", new ZDateTime(2022, 10, 20), wrapper.DOE);
	}

	public void TestBoxDLabelText()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		Factory.Save();

		var wrapper = DocSADH.New(entryHeader, Factory);

		AssertEquals("DOE should be date of last CES Event with REF 060", "Result:", wrapper.BoxDResultLabel);
		AssertEquals("DOE should be date of last CES Event with REF 060", "Seals affixed: Number", wrapper.BoxDSealsAffixedLabel);
		AssertEquals("DOE should be date of last CES Event with REF 060", "Time limit (date):", wrapper.BoxDTimeLimitLabel);
		AssertEquals("DOE should be date of last CES Event with REF 060", "Identity:", wrapper.BoxDIdentityLabel);
		AssertEquals("DOE should be date of last CES Event with REF 060", "Stamp:", wrapper.BoxDStampLabel);
	}
}
