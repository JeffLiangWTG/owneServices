using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders;
using Enterprise.Customs.GB.CDS.Messaging.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.GB.CDS.Testing
{
	class MessageBuilderTest : TestCaseWithFactory
	{
		public void TestAcceptanceDateTime()
		{
			var entryHeader = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new EU.Business.ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			Assert(xml.Contains("20190303030303Z"));
		}

		public void TestImporter()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "test", dataGrouping: "CDS");
			var code1 = helper.CreateCusCodeList("CDS", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "00500", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(code1.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.Header);

			var entryHeader = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);
			var org = OrganisationWrapperTest.CreatePrivateIndividualOrg(Factory);
			entryHeader.Declaration.JE_OH_Importer = org.PK;

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new EU.Business.ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			Assert("Message should contain expected Importer section", xml.Contains(@"<Importer>
        <Name>GB00500 John Smith</Name>
        <Address>
          <CountryCode>GB</CountryCode>
          <Line>John's House</Line>
          <PostcodeID>NA</PostcodeID>
        </Address>
      </Importer>"));

			Assert("Message should contain expected AdditionalInformation section", xml.Contains(@"<AdditionalInformation>
      <StatementCode>00500</StatementCode>
      <StatementDescription>IMPORTER</StatementDescription>
    </AdditionalInformation>"));
		}

		public void TestCountryCodeIsMapped()
		{
			var xs = Core.Constants.CountryCodes.Serbia_ForEUTrading;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.EUCTY, MapDirectionList.Codes.BTH, "Description", isReadonly: false);
			helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, Core.Constants.CountryCodes.Serbia, xs,
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime,
				Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services);

			var serbianOrg = Factory.New<OrgHeader>();
			serbianOrg.OH_FullName = "SLOBODAN";
			serbianOrg.OH_RL_NKClosestPort = "RSBEG";
			serbianOrg.MainAddress.Address1 = "1 STR";
			serbianOrg.MainAddress.Postcode = "12345";
			serbianOrg.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Serbia;
			var entryHeader = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);

			var declaration = entryHeader.Declaration;
			declaration.JE_LocationOtherInformation = Core.Constants.CountryCodes.Serbia;
			declaration.InvoiceLines[1].JI_OA_ExporterAddress = declaration.JE_OA_SellerAddress;
			var representative = declaration.DocAddresses.AddNew(DocAddressType.Representative);
			representative.OrganisationPK = serbianOrg.PK;
			declaration.ImporterDocumentaryAddress.OrganisationPK = serbianOrg.PK;
			declaration.SupplierDocumentaryAddress.OrganisationPK = serbianOrg.PK;
			declaration.SupplierDocumentaryAddress.ClosestPort = serbianOrg.OH_RL_NKClosestPort;
			declaration.JE_OA_DeclarantAddress = serbianOrg.MainAddress.PK;
			declaration.JE_OA_SellerAddress = serbianOrg.MainAddress.PK;
			declaration.JE_OA_Representative = serbianOrg.MainAddress.PK;
			declaration.JE_RL_NKOrigin = serbianOrg.OH_RL_NKClosestPort;

			declaration.Invoices[0].JZ_OH_Supplier = serbianOrg.PK;
			declaration.InvoiceLines[0].JI_OA_ExporterAddress = serbianOrg.MainAddress.PK;
			declaration.InvoiceLines[0].JI_CountryOfOrigin = Core.Constants.CountryCodes.Serbia;
			declaration.JE_MessageType = "IMP";

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new EU.Business.ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			messageBuilder.Build();
			var h1 = new MessageBuilderWrapper(messageBuilder as MessageBuilder);

			CombineAssertions(() =>
			{
				AssertEquals("Agent.Address.CountryCode", xs, h1.Declaration.Agent?.Address?.CountryCode?.Value);
				AssertEquals("Exporter.Address.CountryCode", xs, h1.Declaration.Exporter?.Address?.CountryCode?.Value);
				AssertEquals("Declarant.Address.CountryCode", xs, h1.Declaration.Declarant.Address?.CountryCode?.Value);
				AssertEquals("Importer.Address.CountryCode", xs, h1.Declaration.GoodsShipment.Importer?.Address?.CountryCode?.Value);
				AssertEquals("Destination.CountryCode", xs, h1.Declaration.GoodsShipment.Destination?.CountryCode?.Value);
				AssertEquals("Consignment.GoodsLocation.Address.CountryCode", xs, h1.Declaration.GoodsShipment.Consignment.GoodsLocation?.Address?.CountryCode?.Value);
				AssertEquals("GoodsItems.Origins.CountryCode", xs, string.Join(",", h1.GoodsItems.FirstOrDefault().Origin.Select(x => x.CountryCode.Value)));
				AssertEquals("GoodsItems.ExportCountry.CountryCode", $"{xs},{xs},{xs},{xs},HU", string.Join(",", h1.GoodsItems.Select(x => x.ExportCountry.ID.Value)));
			});
		}

		public void TestAdditionalDocuments()
		{
			var cds = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			_ = helper.CreateNewOrGetExistingDataGrouping(cds);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>();

			attributeNameValuePairs.Clear();
			attributeNameValuePairs.Add("Level", ["ITEM"]);
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(cds, [importCodeType, exportCodeType], "Code1", "Code1 Test", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(cds, [importCodeType, exportCodeType], "Code2", "Code2 Test", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "HYE";
			registrationKey.ServerCodeForTest = "CMT";

			Factory.Save();

			var declaration = Factory.New<Business.Declaration.JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			var instruction = declaration.CustomsEntryInstructions[0];
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			var invoiceLine3 = invoice.InvoiceLines.AddNew();

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;

			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			var entryLine3 = entryHeader.MergedLines.AddNew();
			entryLine3.CL_LineNumber = 3;

			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine3.JI_CL = entryLine3.PK;

			AddSupportingDocument(invoiceLine1, "Code1", "Ref ID 1", "AR1");
			AddSupportingDocument(invoiceLine2, "Code2", "Ref ID 2");
			AddSupportingDocument(invoiceLine3, "Code1", "", "AR1");

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new EU.Business.ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();
			var wrapper = new MessageBuilderWrapper(messageBuilder as MessageBuilder);
			var goodsItems = wrapper.GoodsItems;

			CombineAssertions(() =>
			{
				AssertNotNull(goodsItems);
				AssertEquals("GoodsItems", 3, goodsItems.Count);
				AssertEquals("GoodsItems.AdditionalDocument.ID", "Ref ID 1-AR1", goodsItems[0].AdditionalDocument[0].ID.Value);
				AssertContains("Xml", "<ID>Ref ID 1-AR1</ID>", xml);
				AssertEquals("GoodsItems.AdditionalDocument.ID", "Ref ID 2", goodsItems[1].AdditionalDocument[0].ID.Value);
				AssertContains("Xml", "<ID>Ref ID 2</ID>", xml);
				AssertEquals("GoodsItems.AdditionalDocument.ID", "AR1", goodsItems[2].AdditionalDocument[0].ID.Value);
				AssertContains("Xml", "<ID>AR1</ID>", xml);
			});
		}

		public void TestExportCountryIsMappedFromDeclaration()
		{
			var us = Core.Constants.CountryCodes.UnitedStates;
			var declaration = Factory.New<Business.Declaration.JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_GoodsOrigin = us;

			var instruction = declaration.CustomsEntryInstructions[0];
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;

			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;

			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine2.JI_CL = entryLine2.PK;

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new EU.Business.ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();
			var wrapper = new MessageBuilderWrapper(messageBuilder as MessageBuilder);
			var goodsItems = wrapper.GoodsItems;

			CombineAssertions(() =>
			{
				AssertNotNull(goodsItems);
				AssertEquals("GoodsItems.ExportCountry.CountryCode", $"{us},{us}", string.Join(",", goodsItems.Select(x => x.ExportCountry.ID.Value)));
			});
		}

		public void TestExportCountryIsMappedFromDeclarationAndInvoiceLine()
		{
			var us = Core.Constants.CountryCodes.UnitedStates;
			var ca = Core.Constants.CountryCodes.Canada;
			var declaration = Factory.New<Business.Declaration.JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_GoodsOrigin = us;

			var instruction = declaration.CustomsEntryInstructions[0];
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_RN_NKCountryOfExport = ca;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_RN_NKCountryOfExport = ZString.Empty;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;

			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;

			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine2.JI_CL = entryLine2.PK;

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new EU.Business.ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();
			var wrapper = new MessageBuilderWrapper(messageBuilder as MessageBuilder);
			var goodsItems = wrapper.GoodsItems;

			CombineAssertions(() =>
			{
				AssertNotNull(goodsItems);
				AssertEquals("GoodsItems.ExportCountry.CountryCode", $"{ca},{us}", string.Join(",", goodsItems.Select(x => x.ExportCountry.ID.Value)));
			});
		}

		public void TestExportCountryIsMappedFromInvoiceLine()
		{
			var us = Core.Constants.CountryCodes.UnitedStates;
			var ca = Core.Constants.CountryCodes.Canada;
			var mx = Core.Constants.CountryCodes.Mexico;
			var declaration = Factory.New<Business.Declaration.JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_GoodsOrigin = us;

			var instruction = declaration.CustomsEntryInstructions[0];
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_RN_NKCountryOfExport = ca;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_RN_NKCountryOfExport = mx;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;

			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;

			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine2.JI_CL = entryLine2.PK;

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new EU.Business.ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();
			var wrapper = new MessageBuilderWrapper(messageBuilder as MessageBuilder);
			var goodsItems = wrapper.GoodsItems;

			CombineAssertions(() =>
			{
				AssertNotNull(goodsItems);
				AssertEquals("GoodsItems.ExportCountry.CountryCode", $"{ca},{mx}", string.Join(",", goodsItems.Select(x => x.ExportCountry.ID.Value)));
			});
		}

		public void TestExportCountryForConsignorIsMappedFromInvoiceLine()
		{
			var xs = Core.Constants.CountryCodes.Serbia_ForEUTrading;
			var mx = Core.Constants.CountryCodes.Mexico;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.EUCTY, MapDirectionList.Codes.BTH, "Description", isReadonly: false);
			helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, Core.Constants.CountryCodes.Serbia, xs,
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime,
				Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services);

			var serbianOrg = Factory.New<OrgHeader>();
			serbianOrg.OH_FullName = "SLOBODAN";
			serbianOrg.OH_RL_NKClosestPort = "RSBEG";
			serbianOrg.MainAddress.Address1 = "1 STR";
			serbianOrg.MainAddress.Postcode = "12345";
			serbianOrg.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Serbia;
			var entryHeader = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);

			entryHeader.Declaration.Invoices[0].JZ_OH_Supplier = serbianOrg.PK;
			entryHeader.Declaration.InvoiceLines[0].JI_OA_ExporterAddress = serbianOrg.MainAddress.PK;
			entryHeader.Declaration.InvoiceLines[0].JI_RN_NKCountryOfExport = mx;
			entryHeader.Declaration.InvoiceLines[1].JI_OA_ExporterAddress = serbianOrg.MainAddress.PK;
			entryHeader.Declaration.InvoiceLines[1].JI_RN_NKCountryOfExport = mx;

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new EU.Business.ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			messageBuilder.Build();
			var wrapper = new MessageBuilderWrapper(messageBuilder as MessageBuilder);
			var goodsItems = wrapper.GoodsItems;

			CombineAssertions(() =>
			{
				AssertNotNull(goodsItems);
				AssertEquals("GoodsItems.ExportCountry.CountryCode", $"{mx},{mx},{xs},{xs},HU", string.Join(",", goodsItems.Select(x => x.ExportCountry.ID.Value)));
			});
		}

		void AddSupportingDocument(Business.Declaration.JobComInvoiceLine invoiceLine, string code, string referenceNumber = "", string subType = "")
		{
			var supportingDocument = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = code;
			supportingDocument.CSI_ReferenceNumber = referenceNumber;
			supportingDocument.CSI_SubType = subType;
		}

		void AddPreviousDocument(Business.Declaration.JobComInvoiceLine invoiceLine, string code, string referenceNumber = "", int line_no = 0)
		{
			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			previousDocument.CSI_Code = code;
			previousDocument.CSI_ReferenceNumber = referenceNumber;
			previousDocument.CSI_LineNo = line_no;
		}

		void AddPreviousDocument(Business.Declaration.JobDeclaration declaration, string code, string referenceNumber = "", int line_no = 0)
		{
			var previousDocument = declaration.PreviousDocuments.AddNew();
			previousDocument.CSI_Code = code;
			previousDocument.CSI_ReferenceNumber = referenceNumber;
			previousDocument.CSI_LineNo = line_no;
		}

		public void TestPreviousDocuments()
		{
			var cds = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			_ = helper.CreateNewOrGetExistingDataGrouping(cds);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>();

			attributeNameValuePairs.Clear();
			attributeNameValuePairs.Add("Level", ["ITEM"]);
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(cds, [importCodeType, exportCodeType], "Code1", "Code1 Test", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(cds, [importCodeType, exportCodeType], "Code2", "Code2 Test", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "HYE";
			registrationKey.ServerCodeForTest = "CMT";

			Factory.Save();

			var declaration = Factory.New<Business.Declaration.JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			var instruction = declaration.CustomsEntryInstructions[0];
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse;

			AddPreviousDocument(declaration, "CodeA", "Ref ID A", 1);
			AddPreviousDocument(declaration, "CodeB", "Ref ID B", 0);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			var invoiceLine3 = invoice.InvoiceLines.AddNew();

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;

			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			var entryLine3 = entryHeader.MergedLines.AddNew();
			entryLine3.CL_LineNumber = 3;

			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine3.JI_CL = entryLine3.PK;

			AddPreviousDocument(invoiceLine1, "Code1", "Ref ID 1", 1);
			AddPreviousDocument(invoiceLine2, "Code2", "Ref ID 2", 2);
			AddPreviousDocument(invoiceLine3, "Code3", "Ref ID 3", 0);

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new EU.Business.ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();
			var wrapper = new MessageBuilderWrapper(messageBuilder as MessageBuilder);

			var goodsShipment = wrapper.Declaration.GoodsShipment;

			CombineAssertions(() =>
			{
				AssertNotNull(goodsShipment);
				AssertEquals("GoodsItems.PreviousDocument.ID", "Ref ID A", goodsShipment.PreviousDocument[0].ID.Value);
				AssertEquals("GoodsItems.PreviousDocument.LineNumericSpecified", true, goodsShipment.PreviousDocument[0].LineNumericSpecified);
				AssertContains("Xml", @"<PreviousDocument>
        <ID>Ref ID A</ID>
        <TypeCode>CodeA</TypeCode>
        <LineNumeric>1</LineNumeric>
      </PreviousDocument>", xml);
				AssertEquals("GoodsItems.PreviousDocument.ID", "Ref ID B", goodsShipment.PreviousDocument[1].ID.Value);
				AssertEquals("GoodsItems.PreviousDocument.LineNumericSpecified", false, goodsShipment.PreviousDocument[1].LineNumericSpecified);
				AssertContains("Xml", @"<PreviousDocument>
        <ID>Ref ID B</ID>
        <TypeCode>CodeB</TypeCode>
      </PreviousDocument>", xml);
			});

			var goodsItems = wrapper.GoodsItems;

			CombineAssertions(() =>
			{
				AssertNotNull(goodsItems);
				AssertEquals("GoodsItems", 3, goodsItems.Count);
				AssertEquals("GoodsItems.PreviousDocument.ID", "Ref ID 1", goodsItems[0].PreviousDocument[0].ID.Value);
				AssertEquals("GoodsItems.PreviousDocument.LineNumericSpecified", true, goodsItems[0].PreviousDocument[0].LineNumericSpecified);
				AssertContains("Xml", @"<PreviousDocument>
          <ID>Ref ID 1</ID>
          <TypeCode>Code1</TypeCode>
          <LineNumeric>1</LineNumeric>
        </PreviousDocument>", xml);
				AssertEquals("GoodsItems.PreviousDocument.ID", "Ref ID 2", goodsItems[1].PreviousDocument[0].ID.Value);
				AssertEquals("GoodsItems.PreviousDocument.LineNumericSpecified", true, goodsItems[1].PreviousDocument[0].LineNumericSpecified);
				AssertContains("Xml", @"<PreviousDocument>
          <ID>Ref ID 2</ID>
          <TypeCode>Code2</TypeCode>
          <LineNumeric>2</LineNumeric>
        </PreviousDocument>", xml);
				AssertEquals("GoodsItems.PreviousDocument.ID", "Ref ID 3", goodsItems[2].PreviousDocument[0].ID.Value);
				AssertEquals("GoodsItems.PreviousDocument.LineNumericSpecified", false, goodsItems[2].PreviousDocument[0].LineNumericSpecified);
				AssertContains("Xml", @"<PreviousDocument>
          <ID>Ref ID 3</ID>
          <TypeCode>Code3</TypeCode>
        </PreviousDocument>", xml);
			});
		}

		public void TestDeclarantNameIsSet()
		{
			var entryHeader = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);
			var declaration = entryHeader.Declaration;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = Factory.CreateValidOrgAddress();
			orgAddress.CompanyName = "Declarant";
			orgAddress.OA_OH = orgHeader.PK;
			declaration.JE_OA_DeclarantAddress = orgHeader.MainAddress.PK;
			orgHeader.MainAddress.CompanyName = "Test Declarant Name";
			orgHeader.MainAddress.City = "Test Declarant City";

			var trn = orgHeader.CustomsCodes.AddNew();
			trn.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			trn.OK_CustomsRegNo = "4711";
			var declarantID = declaration.Country.Code + trn.OK_CustomsRegNo;

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);

			using (GBCustomsDataRegistry.Instance.SendCDS317.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				entryHeader.EntryInstruction.CEI_Style = "B1";
				var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new EU.Business.ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
				var b1 = new MessageBuilderWrapper(messageBuilder as MessageBuilder);
				messageBuilder.Build();
				AssertEquals("SendCDS317 = true, CEI_Style = B1, ID", declarantID, b1.Declaration.Declarant.ID.Value);
				AssertEquals("SendCDS317 = true, CEI_Style = B1, Name", orgHeader.MainAddress.CompanyName, b1.Declaration.Declarant.Name.Value);
				AssertEquals("SendCDS317 = true, CEI_Style = B1, Addresss City", orgHeader.MainAddress.City, b1.Declaration.Declarant.Address.CityName.Value);

				entryHeader.EntryInstruction.CEI_Style = "FS";
				messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new EU.Business.ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
				var fsd = new MessageBuilderWrapper(messageBuilder as MessageBuilder);
				messageBuilder.Build();
				AssertEquals("SendCDS317 = true, CEI_Style = FS, ID", declarantID, fsd.Declaration.Declarant.ID.Value);
				AssertNull("SendCDS317 = true, CEI_Style = FS, Name", fsd.Declaration.Declarant.Name);
				AssertNull("SendCDS317 = true, CEI_Style = FS, Address City", fsd.Declaration.Declarant.Address);
			}
			using (GBCustomsDataRegistry.Instance.SendCDS317.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				entryHeader.EntryInstruction.CEI_Style = "B1";
				var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new EU.Business.ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
				var b1 = new MessageBuilderWrapper(messageBuilder as MessageBuilder);
				messageBuilder.Build();
				AssertEquals("SendCDS317 = false, CEI_Style = B1, ID", declarantID, b1.Declaration.Declarant.ID.Value);
				AssertNull("SendCDS317 = false, CEI_Style = B1, Name", b1.Declaration.Declarant.Name);
				AssertNull("SendCDS317 = false, CEI_Style = B1, Address City", b1.Declaration.Declarant.Address);

				entryHeader.EntryInstruction.CEI_Style = "FS";
				messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new EU.Business.ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
				var fsd = new MessageBuilderWrapper(messageBuilder as MessageBuilder);
				messageBuilder.Build();
				AssertEquals("SendCDS317 = false, CEI_Style = FS, ID", declarantID, fsd.Declaration.Declarant.ID.Value);
				AssertNull("SendCDS317 = false, CEI_Style = FS, Name", fsd.Declaration.Declarant.Name);
				AssertNull("SendCDS317 = false, CEI_Style = FS, Address City", fsd.Declaration.Declarant.Address);
			}
		}

		class MessageBuilderWrapper
		{
			public MessageBuilderWrapper(MessageBuilder messageBuilder)
			{
				this.messageBuilder = messageBuilder;
				declarationFieldInfo = typeof(MessageBuilder).GetField("decMessage", BindingFlags.Instance | BindingFlags.NonPublic);
				goodsitemsFieldInfo = typeof(MessageBuilder).GetField("decGoodsItems", BindingFlags.Instance | BindingFlags.NonPublic);
				AssertNotNull("Pre-requisite: field MessageBuilder.decMessage", declarationFieldInfo);
				AssertNotNull("Pre-requisite: property MessageBuilder.GoodsItems", goodsitemsFieldInfo);
			}

			public CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData.Declaration Declaration
			{
				get => (CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData.Declaration)declarationFieldInfo.GetValue(messageBuilder);
			}

			public List<DeclarationGoodsShipmentGovernmentAgencyGoodsItem> GoodsItems
			{
				get => (List<DeclarationGoodsShipmentGovernmentAgencyGoodsItem>)goodsitemsFieldInfo.GetValue(messageBuilder);
			}

			readonly MessageBuilder messageBuilder;
			readonly FieldInfo declarationFieldInfo;
			readonly FieldInfo goodsitemsFieldInfo;
		}
	}
}
