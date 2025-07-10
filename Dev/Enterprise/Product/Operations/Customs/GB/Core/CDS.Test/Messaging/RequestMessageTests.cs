using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.GB.Business.Declaration.Testing;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.Customs.GB.CDS.CodeDescriptionPairLists;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders;
using Enterprise.Customs.GB.CDS.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Customs.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.GB.CDS.Messaging.Testing
{
	public class RequestMessageTests : TestCaseWithFactory
	{
		CusEntryHeader GetEntryHeader_ForExportsMessageTesting(string declarationType, string messageType = MessageTypeList.Codes.Export, bool assertWareHousePreRequisites = true)
		{
			CreateReferenceData_ForExportsMessageTesting();

			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			AmendTransportForTest(entryHeader, transportMode: Enterprise.Customs.Business.TransportTypeList.Codes.Sea);
			entryHeader.CH_MasterUCR = "1234567890";
			entryHeader.RandomHeader.ZG_TransportChargesMethodOfPayment = "Y";

			var declaration = entryHeader.Declaration;
			declaration.JE_MessageType = messageType;
			declaration.JE_DeclarationType = declarationType;
			declaration.JE_MessageSubType = "EX";
			declaration.ZG_SpecificCircumstanceIndicator = "A";

			var decSuppDoc = declaration.SupportingDocuments[0];
			decSuppDoc.CSI_DateOfIssue = new ZDateTime(2019, 3, 3, 3, 3, 3);
			decSuppDoc.CSI_UnitOfQuantity = "VIC";
			decSuppDoc.CSI_ReferenceNumber2 = "CSI2";
			decSuppDoc.CSI_UnitOfQuantity = "KGM";
			decSuppDoc.CSI_Quantity = 13m;

			var frL1 = declaration.InvoiceLines[0].FiscalReferences.AddNew();
			frL1.CFR_Code = "FR1";
			frL1.CFR_Reference = "GB11111111";

			var frL2 = entryHeader.Declaration.InvoiceLines[0].FiscalReferences.AddNew();
			frL2.CFR_Code = "FR2";
			frL2.CFR_Reference = "GB22222222";

			var frL3 = entryHeader.Declaration.InvoiceLines[1].FiscalReferences.AddNew();
			frL3.CFR_Code = "FR3";
			frL3.CFR_Reference = "GB33333333";

			var frL4 = entryHeader.Declaration.InvoiceLines[1].FiscalReferences.AddNew();
			frL4.CFR_Code = "FR4";
			frL4.CFR_Reference = "GB44444444";

			var frH1 = declaration.CusEntryInstruction.FiscalReferences.AddNew();
			frH1.CFR_Code = "FR1";
			frH1.CFR_Reference = "GB55555555";

			var frH2 = declaration.CusEntryInstruction.FiscalReferences.AddNew();
			frH2.CFR_Code = "FR2";
			frH2.CFR_Reference = "GB66666666";

			#region Customs Warehouse

			const string CountryCodeUK = Core.Constants.CountryCodes.UnitedKingdom;
			const string CCP = OrgCusCode.CodeTypes.ControlledPremisesID;
			const string customsControlledPremisisCode = "CCP123";

			var warehouse = Factory.New<OrgHeader>();
			warehouse.OH_Code = "WH123";
			warehouse.CompanyData.OB_IMUsedBondedWhs = true;
			var cusCode = warehouse.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(CCP, customsControlledPremisisCode, CountryCodeUK);

			var addrWH = warehouse.Addresses.AddNew();
			addrWH.Address1 = "21 Long Street";
			cusCode.OK_OA_PremisesAddress = addrWH.PK;

			declaration.CusEntryInstruction.CEI_OA_Warehouse = addrWH.PK;

			#endregion

			#region Use Customs Procedure for Exports

			var invLines = entryHeader.InvoiceLines.ToArray();
			var invLine = invLines[0];
			invLine.JI_Procedure = "3171000";
			invLine.JI_CEI = declaration.CusEntryInstruction.PK;
			invLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Finland;
			invLines[1].JI_CountryOfOrigin = ZString.Empty;

			entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;

			entryHeader.EntryInstruction.CEI_Style = declarationType;
			entryHeader.EntryInstruction.CEI_SubStyle = "A";

			if (assertWareHousePreRequisites)
			{
				Assert("Invoice line must have a customs procedure that deals with a customs warehouse.", invLine.HasOutOfWarehouseProcedure);
				var assertMsg = "The customs warehouse code on the customs entry instruction must be setup correctly.";
				AssertEquals(assertMsg, customsControlledPremisisCode, ((CusEntryInstruction)invLine.EntryInstruction).WarehouseIDFor27);
				AssertEquals(assertMsg, customsControlledPremisisCode, entryHeader.EntryInstruction.WarehouseIDFor27);
			}
			#endregion
			entryHeader.Declaration.JE_RL_NKPortOfLoading = "GBLHR";
			entryHeader.Declaration.JE_RL_NKPortOfArrival = "AUSYD";
			return entryHeader;
		}

		void CreateReferenceData_ForExportsMessageTesting()
		{
			const string CDS = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(CDS, "65", "31", "71", "000", "Goods re exported from a customs warehouse.", "EXP", calculateDuty: false, landedCostOnly: false, intoWarehouse: false, outOfWarehouse: true, group: "B1, B2, B4, C1");
			helper.CreateCusMapType(RefCusMapTypeList.Codes.EUCTY, MapDirectionList.Codes.BTH, "Description", false);
			helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "PR", "US", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "CDS");
			helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "MC", "FR", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "CDS");
			helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "RS", "XS", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "CDS");
			helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "SJ", "NO", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "CDS");
			Factory.Save();
		}

		ZString GetXmlMessage_ForExportsMessageTesting(CusEntryHeader entryHeader)
		{
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var msgSendingObj = MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader);
			var errorCollector = new ErrorCollector();
			var msgFuncNew = new Customs.Business.CusdecMessageFunction.New();
			var messageBuilder = messageBuilderManager.NewMessageBuilder(msgSendingObj, errorCollector, msgFuncNew);
			var xml = messageBuilder.Build();
			return xml;
		}

		public void TestMessageBuilder_MarksNumbersID_HasNoNewlineChars()
		{
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var messageBuilderManager = new MessageBuilderManager();
			var marksWithNewlines = "THIS\r\nis\rNOT\nA LOVE SONG 567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123XXXXX";
			entryHeader.Declaration.Bills[1].PackingGroups[0].Packages[1].CW_MarksAndNos = marksWithNewlines;
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();
			var marksWithoutNewlines_TrimmedTo512Chars = "THIS is NOT A LOVE SONG 56789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012";
			AssertXMLContains("<MarksNumbersID>" + marksWithoutNewlines_TrimmedTo512Chars + "</MarksNumbersID>", xml);
			AssertEquals(512, marksWithoutNewlines_TrimmedTo512Chars.Length);
		}

		public void TestMessageBuilder_AddInfo_HasNoNewLineChars()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "test", dataGrouping: "CDS");
			var code1 = helper.CreateCusCodeList("CDS", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "ADD1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(code1.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.Header);
			var code2 = helper.CreateCusCodeList("CDS", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "ADD2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(code2.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.Item);

			var entryHeader = CreateSampleEntryHeaderForH1(Factory);

			var addInfoDec = entryHeader.Declaration.AdditionalInfos.AddNew();
			addInfoDec.CSI_Code = "ADD1";
			addInfoDec.CSI_Description = "\r\nYou\r\nare\r\nnot\r\nprepared";
			addInfoDec.CSI_ReferenceNumber = "123";

			var addInfoInvHead = entryHeader.InvoiceHeaders.FirstOrDefault()?.AdditionalInfos.AddNew();
			addInfoInvHead.CSI_Code = "ADD1";
			addInfoInvHead.CSI_Description = "\r\nYou\r\nshall\r\nnot\r\npass\r\n";
			addInfoInvHead.CSI_ReferenceNumber = "123";

			var addInfoInvLine = entryHeader.InvoiceHeaders.FirstOrDefault()?.InvoiceLines[0].AdditionalInfos.AddNew();
			addInfoInvLine.CSI_Code = "ADD2";
			addInfoInvLine.CSI_Description = "You\r\nrequire\r\nmore\r\nvespene\r\ngas\r\n";
			addInfoInvLine.CSI_ReferenceNumber = "123";

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			CombineAssertions("Addinfo should not contain newline chars", () =>
			{
				AssertXMLContains(@"You are not prepared", xml);
				AssertXMLContains(@"You shall not pass", xml);
				AssertXMLContains(@"You require more vespene gas", xml);
			});
		}

		public void TestB1Message()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			const string B1 = ExportDeclarationTypeList.Codes.DeclarationForExport;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(B1);
			entryHeader.Declaration.Transports.RemoveAndDeleteAll(); // no transports at all, but should still see origin/dest in the itinerary (GB-->AU)
			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedB1.xml"), xml);

			SetDataForTotalPackageQuantityTesting(entryHeader, 0, true);
			xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			var embeddedResource = EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedB1_WhenTotalPackageQuantityIsZero.xml");
			AssertXMLContains(embeddedResource, xml);
		}

		public void TestB1Message_CDSAutomation_NotForImports()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.NotForImports
				});

			const string B1 = ExportDeclarationTypeList.Codes.DeclarationForExport;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(B1);
			entryHeader.Declaration.Transports.RemoveAndDeleteAll(); // no transports at all, but should still see origin/dest in the itinerary (GB-->AU)
			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedB1NotForImports.xml"), xml);
		}

		public void TestB1Message_CDSAutomation_SendOnlyASingleEntryReference()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SendOnlyASingleEntryReference
				});

			const string B1 = ExportDeclarationTypeList.Codes.DeclarationForExport;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(B1);
			entryHeader.Declaration.Transports.RemoveAndDeleteAll(); // no transports at all, but should still see origin/dest in the itinerary (GB-->AU)
			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedB1SendOnlyASingleEntryReference.xml"), xml);
		}

		public void TestB1MessageWithInvoicesWithDifferentCurrency()
		{
			const string B1 = ExportDeclarationTypeList.Codes.DeclarationForExport;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(B1);
			entryHeader.Declaration.Transports.RemoveAndDeleteAll();
			var invoice = entryHeader.Declaration.Invoices[0];
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(@"<InvoiceAmount currencyID=""GBP"">1219.42</InvoiceAmount>", xml);
			AssertXMLContains(@"<ItemChargeAmount currencyID=""GBP"">500.00</ItemChargeAmount>", xml);
			AssertXMLContains(@"<ItemChargeAmount currencyID=""GBP"">71.94</ItemChargeAmount>", xml);
		}

		public void TestB1MessageWithInvoicesWithSameCurrency()
		{
			const string B1 = ExportDeclarationTypeList.Codes.DeclarationForExport;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(B1);
			entryHeader.Declaration.Transports.RemoveAndDeleteAll();
			entryHeader.Declaration.Invoices.ToList().ForEach(x => x.JZ_RX_NKInvoice_Currency = "USD");
			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(@"<InvoiceAmount currencyID=""USD"">1500.00</InvoiceAmount>", xml);
			AssertXMLContains(@"<ItemChargeAmount currencyID=""USD"">100.00</ItemChargeAmount>", xml);
			AssertXMLContains(@"<ItemChargeAmount currencyID=""USD"">500.00</ItemChargeAmount>", xml);
		}

		public void TestB1MessageWithInvoicesWithSameCurrencyLocal()
		{
			const string B1 = ExportDeclarationTypeList.Codes.DeclarationForExport;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(B1);
			entryHeader.Declaration.Transports.RemoveAndDeleteAll();
			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(@"<InvoiceAmount currencyID=""GBP"">1500.00</InvoiceAmount>", xml);
			AssertXMLContains(@"<ItemChargeAmount currencyID=""GBP"">100.00</ItemChargeAmount>", xml);
			AssertXMLContains(@"<ItemChargeAmount currencyID=""GBP"">500.00</ItemChargeAmount>", xml);
		}

		public void TestB1Message_Seals()
		{
			const string B1 = ExportDeclarationTypeList.Codes.DeclarationForExport;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(B1);
			var container1 = entryHeader.Declaration.CusContainers[0];
			var container2 = entryHeader.Declaration.CusContainers[1];

			container1.CO_Seal = "CONT1SEAL1";
			container1.CO_SecondSeal = "";
			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(@"</GoodsLocation>
        <TransportEquipment>
          <SequenceNumeric>1</SequenceNumeric>
          <Seal>
            <SequenceNumeric>1</SequenceNumeric>
            <ID>CONT1SEAL1</ID>
          </Seal>
        </TransportEquipment>
      </Consignment>", xml);

			container1.CO_Seal = "";
			container1.CO_SecondSeal = "CONT1SEAL2";
			xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(@"</GoodsLocation>
        <TransportEquipment>
          <SequenceNumeric>1</SequenceNumeric>
          <Seal>
            <SequenceNumeric>1</SequenceNumeric>
            <ID>CONT1SEAL2</ID>
          </Seal>
        </TransportEquipment>
      </Consignment>", xml);

			container1.CO_Seal = "CONT1SEAL1";
			xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(@"</GoodsLocation>
        <TransportEquipment>
          <SequenceNumeric>1</SequenceNumeric>
          <Seal>
            <SequenceNumeric>1</SequenceNumeric>
            <ID>CONT1SEAL1</ID>
          </Seal>
          <Seal>
            <SequenceNumeric>2</SequenceNumeric>
            <ID>CONT1SEAL2</ID>
          </Seal>
        </TransportEquipment>
      </Consignment>", xml);

			container2.CO_Seal = "CONT2SEAL1";
			xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(@"</GoodsLocation>
        <TransportEquipment>
          <SequenceNumeric>1</SequenceNumeric>
          <Seal>
            <SequenceNumeric>1</SequenceNumeric>
            <ID>CONT1SEAL1</ID>
          </Seal>
          <Seal>
            <SequenceNumeric>2</SequenceNumeric>
            <ID>CONT1SEAL2</ID>
          </Seal>
          <Seal>
            <SequenceNumeric>3</SequenceNumeric>
            <ID>CONT2SEAL1</ID>
          </Seal>
        </TransportEquipment>
      </Consignment>", xml);

			container1.CO_Seal = "";
			container1.CO_SecondSeal = "";
			container2.CO_Seal = "CONT2SEAL1";
			xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(@"</GoodsLocation>
        <TransportEquipment>
          <SequenceNumeric>1</SequenceNumeric>
          <Seal>
            <SequenceNumeric>1</SequenceNumeric>
            <ID>CONT2SEAL1</ID>
          </Seal>
        </TransportEquipment>
      </Consignment>", xml);

			container2.CO_Seal = "";
			xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(@"</GoodsLocation>
        <TransportEquipment>
          <SequenceNumeric>1</SequenceNumeric>
          <Seal>
            <SequenceNumeric>1</SequenceNumeric>
            <ID>NOSEALS</ID>
          </Seal>
        </TransportEquipment>
      </Consignment>", xml);
		}

		public void TestB1MessageSupplementaryCode()
		{
			const string B1 = ExportDeclarationTypeList.Codes.DeclarationForExport;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(B1);
			entryHeader.Declaration.Transports.RemoveAndDeleteAll(); // no transports at all, but should still see origin/dest in the itinerary (GB-->AU)
			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			var declaration = entryHeader.Declaration;
			var invoice1 = declaration.Invoices[0];
			var invoiceLine1 = invoice1.InvoiceLines[0];
			invoiceLine1.JI_SupplementaryCode1 = "1981";
			invoiceLine1.JI_SupplementaryCode2 = "2017";

			xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(@"          <Classification>
            <ID>1981</ID>
            <IdentificationTypeCode>TRA</IdentificationTypeCode>
          </Classification>
          <Classification>
            <ID>2017</ID>
            <IdentificationTypeCode>TRA</IdentificationTypeCode>
          </Classification>", xml);
		}

		public void TestB1MessageNoOriginWhenNotSpecified()
		{
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(ExportDeclarationTypeList.Codes.DeclarationForExport);
			var invLine = entryHeader.InvoiceLines.First();
			invLine.JI_CountryOfOrigin = ZString.Empty;

			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertNotContains("<Origin>", xml);
		}

		public void TestB2Message()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			const string B2 = ExportDeclarationTypeList.Codes.DeclarationForOutwardProcessing;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(B2);
			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedB2.xml"), xml);

			SetDataForTotalPackageQuantityTesting(entryHeader, 0, true);
			xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedB2_WhenTotalPackageQuantityIsZero.xml"), xml);
		}

		public void TestB2Message_CDSDUCRAutomation_NotForImports()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.NotForImports
				});

			const string B2 = ExportDeclarationTypeList.Codes.DeclarationForOutwardProcessing;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(B2);
			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedB2NotForImports.xml"), xml);
		}

		public void TestB2Message_CDSDUCRAutomation_SendOnlyASingleEntryReference()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SendOnlyASingleEntryReference
				});

			const string B2 = ExportDeclarationTypeList.Codes.DeclarationForOutwardProcessing;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(B2);
			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedB2SendOnlyASingleEntryReference.xml"), xml);
		}

		public void TestB4Message()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			const string B4 = ExportDeclarationTypeList.Codes.DeclarationForDispatchOfGoods;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(B4);
			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedB4.xml"), xml);

			SetDataForTotalPackageQuantityTesting(entryHeader, 55, false);
			xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedB4_WhenTotalPackageQuantityIsNonZeroThroughNonInventoryLocation.xml"), xml);

			SetDataForTotalPackageQuantityTesting(entryHeader, 0, true);
			xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedB4_WhenTotalPackageQuantityIsZeroThroughInventoryLocation.xml"), xml);
		}

		public void TestB4Message_CDSDUCRAutomation_NotForImports()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.NotForImports
				});

			const string B4 = ExportDeclarationTypeList.Codes.DeclarationForDispatchOfGoods;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(B4);
			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedB4NotForImports.xml"), xml);
		}

		public void TestB4Message_CDSDUCRAutomation_SendOnlyASingleEntryReference()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SendOnlyASingleEntryReference
				});

			const string B4 = ExportDeclarationTypeList.Codes.DeclarationForDispatchOfGoods;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(B4);
			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedB4SendOnlyASingleEntryReference.xml"), xml);
		}

		public void TestC1Message()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			const string C1 = ExportDeclarationTypeList.Codes.SimplifiedDeclarationForExport;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(C1);
			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedC1.xml"), xml);

			SetDataForTotalPackageQuantityTesting(entryHeader, 55, false);
			xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedC1_WhenTotalPackageQuantityIsNonZeroThroughNonInventoryLocation.xml"), xml);

			SetDataForTotalPackageQuantityTesting(entryHeader, 0, true);
			xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedC1_WhenTotalPackageQuantityIsZeroThroughInventoryLocation.xml"), xml);
		}

		public void TestC1Message_CDSAutomation_SendOnlyASingleEntryReference()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SendOnlyASingleEntryReference
				});

			const string C1 = ExportDeclarationTypeList.Codes.SimplifiedDeclarationForExport;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(C1);
			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedC1SendOnlyASingleEntryReference.xml"), xml);
		}

		public void TestC1Message_CDSAutomation_NotForImports()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.NotForImports
				});

			const string C1 = ExportDeclarationTypeList.Codes.SimplifiedDeclarationForExport;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(C1);
			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedC1NotForImports.xml"), xml);
		}

		public void TestMessageCarrierFor_B1_ExportsMessage()
		{
			const string B1 = ExportDeclarationTypeList.Codes.DeclarationForExport;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(B1);
			entryHeader.Declaration.Transports.RemoveAndDeleteAll();
			entryHeader.Declaration.ShippingLine.CustomsCodes.RemoveAndDeleteAll();
			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);

			AssertXMLContains(@"<Consignment>
      <Carrier>
        <Name>MR Shipping Line</Name>
        <Address>
          <CityName>Sydney</CityName>
          <CountryCode>AU</CountryCode>
          <Line>Shipping Line Address 1 Shipping Li</Line>
          <PostcodeID>POSTCODE</PostcodeID>
        </Address>
      </Carrier>", xml);

			entryHeader.Declaration.ShippingLine.SetCustomsCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, RefCountry.LoadFromCountryCode(Factory, "GB"), "GB123456789000");
			xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);

			AssertXMLContains(@"<Consignment>
      <Carrier>
        <ID>GB123456789000</ID>
      </Carrier>", xml);
		}

		public void TestBorderTransportMeansFor_B1_ExportsMessage_for_Sea()
		{
			const string B1 = ExportDeclarationTypeList.Codes.DeclarationForExport;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(B1);

			entryHeader.Declaration.Transports.RemoveAndDeleteAll();
			entryHeader.Declaration.JE_TransportMode = Enterprise.Customs.Business.TransportTypeList.Codes.Sea;
			entryHeader.Declaration.JE_VesselName = "TITANIC";
			entryHeader.Declaration.JE_VoyageFlightNo = "VOY123";
			entryHeader.Declaration.JE_RN_NKTransportNationality = "GB";

			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.BorderTransportMeansForExport.ExpectedB1ForSea.xml"), xml);
		}

		public void TestBorderTransportMeansFor_B1_ExportsMessage_for_InlandWaterwayTransport()
		{
			const string B1 = ExportDeclarationTypeList.Codes.DeclarationForExport;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(B1);

			entryHeader.Declaration.JE_TransportMode = Enterprise.Customs.Business.TransportTypeList.Codes.InlandWaterwayTransport;
			entryHeader.Declaration.ZG_Box18TransportID = "TxId01";
			entryHeader.Declaration.JE_RN_NKTransportNationality = "DE";

			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.BorderTransportMeansForExport.ExpectedB1ForInlandWaterway.xml"), xml);
		}

		public void TestBorderTransportMeansFor_B1_ExportsMessage_for_OwnPropulsion()
		{
			const string B1 = ExportDeclarationTypeList.Codes.DeclarationForExport;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(B1);

			entryHeader.Declaration.JE_TransportMode = Enterprise.Customs.Business.TransportTypeList.Codes.OwnPropulsion;
			entryHeader.Declaration.ZG_Box18TransportID = "TxId02";
			entryHeader.Declaration.JE_RN_NKTransportNationality = "FR";

			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.BorderTransportMeansForExport.ExpectedB1ForOwnPropulsion.xml"), xml);
		}

		public void TestBorderTransportMeansFor_B2_ExportsMessage_for_Road()
		{
			const string B2 = ExportDeclarationTypeList.Codes.DeclarationForOutwardProcessing;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(B2);

			entryHeader.Declaration.JE_TransportMode = Enterprise.Customs.Business.TransportTypeList.Codes.Road;
			entryHeader.Declaration.ZG_Box18TransportID = "TxId03";
			entryHeader.Declaration.JE_RN_NKTransportNationality = "US";

			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.BorderTransportMeansForExport.ExpectedB2ForRoad.xml"), xml);
		}

		public void TestBorderTransportMeansFor_B2_ExportsMessage_for_RORO()
		{
			const string B2 = ExportDeclarationTypeList.Codes.DeclarationForOutwardProcessing;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(B2);
			entryHeader.Declaration.JE_TransportMode = GBTransportTypeList.Codes.ROR;
			entryHeader.Declaration.ZG_Box18TransportID = "TxId03";
			entryHeader.Declaration.JE_RN_NKTransportNationality = "US";
			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);

			AssertXMLContains(@"<BorderTransportMeans>
      <ID>TxId03</ID>
      <IdentificationTypeCode>30</IdentificationTypeCode>
      <RegistrationNationalityCode>US</RegistrationNationalityCode>
      <ModeCode>6</ModeCode>
    </BorderTransportMeans>", xml);

			AssertXMLContains(@"<DepartureTransportMeans>
          <ID>TxId03</ID>
          <IdentificationTypeCode>30</IdentificationTypeCode>
          <ModeCode>3</ModeCode>
        </DepartureTransportMeans>", xml);
		}

		public void TestBorderTransportMeansFor_B4_ExportsMessage_for_Air()
		{
			const string B4 = ExportDeclarationTypeList.Codes.DeclarationForDispatchOfGoods;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(B4);

			entryHeader.Declaration.Transports.RemoveAndDeleteAll();
			entryHeader.Declaration.JE_TransportMode = Enterprise.Customs.Business.TransportTypeList.Codes.Air;
			entryHeader.Declaration.JE_VoyageFlightNo = "BA163";
			entryHeader.Declaration.JE_RN_NKTransportNationality = "GB";

			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.BorderTransportMeansForExport.ExpectedB4ForAir.xml"), xml);
		}
		public void TestBorderTransportMeansFor_C1_ExportsMessage_for_Rail()
		{
			const string C1 = ExportDeclarationTypeList.Codes.SimplifiedDeclarationForExport;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(C1);

			entryHeader.Declaration.JE_TransportMode = Enterprise.Customs.Business.TransportTypeList.Codes.Rail;
			entryHeader.Declaration.ZG_Box18TransportID = "TxId04";

			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.BorderTransportMeansForExport.ExpectedC1ForRail.xml"), xml);
		}

		#region C21E tests

		readonly XmlDocument xmlDoc = new XmlDocument();
		readonly XmlDocument xmlDoc2 = new XmlDocument();
		const string messageType = "C21E";
		const string nodeNotFound = "NODE NOT FOUND";

		public void TestC21EMessage()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});
			const string c21E = ExportDeclarationTypeList.Codes.ExportClearanceRequestC21E;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(c21E);
			SetDataForTotalPackageQuantityTesting(entryHeader, 55, true);
			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedC21E.xml"), xml);

			SetDataForTotalPackageQuantityTesting(entryHeader, 55, false);
			xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedC21E_WhenTotalPackageQuantityIsNonZeroThroughNonInventoryLocation.xml"), xml);

			SetDataForTotalPackageQuantityTesting(entryHeader, 0, true);
			xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedC21E_WhenTotalPackageQuantityIsZeroThroughInventoryLocation.xml"), xml);
		}

		public void TestC21EMessage_CDSDUCRAutomation_NotForImports()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.NotForImports
				});
			const string c21E = ExportDeclarationTypeList.Codes.ExportClearanceRequestC21E;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(c21E);
			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedC21ENotForImports.xml"), xml);
		}

		public void TestC21EMessage_CDSDUCRAutomation_SendOnlyASingleEntryReference()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SendOnlyASingleEntryReference
				});
			const string c21E = ExportDeclarationTypeList.Codes.ExportClearanceRequestC21E;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(c21E);
			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedC21ESendOnlyASingleEntryReference.xml"), xml);
		}

		string ConvertToXPathWithLocalName(string xmlPath)
		{
			const StringSplitOptions options = new StringSplitOptions();
			var delimiters = new[] { "/" };
			var arrS = xmlPath.Split(delimiters, options);
			var sb = new StringBuilder("/");

			foreach (var str in arrS)
			{
				sb.Append($"/*[local-name()='{str}']");
			}

			return sb.ToString();
		}

		public void AssertXmlContainsElementsIfDataSupplied(string xPath, string expected, bool addLocalNameToXPath = true)
		{
			const string ns = "urn:wco:datamodel:WCO:DEC-DMS:2";
			var modifiedXPath = AddLocalNameToXPath(xPath, addLocalNameToXPath);
			var actual = xmlDoc.SelectSingleNode(modifiedXPath)?.OuterXml ?? ZString.Empty;
			actual = actual.Replace(" xmlns=\"" + ns + "\"", "");
			AssertXMLEquals(messageType + " element should have data if set: " + xPath, expected, actual);
		}

		public void AssertXmlDoesNotContainElementsIfDataNotSupplied(string xPath, string expected, bool addLocalNameToXPath = true)
		{
			var modifiedXPath = AddLocalNameToXPath(xPath, addLocalNameToXPath);
			var actual = xmlDoc2.SelectSingleNode(modifiedXPath)?.OuterXml ?? nodeNotFound;
			AssertEquals(messageType + " element should not be output to XML if not supplied (O/D): " + xPath, nodeNotFound, actual);
		}

		public void AssertXmlDoesNotContainElementsIfDataIsNotApplicable(string valueFromBusinessObject, string xPath, bool addLocalNameToXPath = true)
		{
			AssertEquals("Expect a value from the business object that we don't want to output to XML", false, string.IsNullOrEmpty(valueFromBusinessObject));
			var modifiedXPath = AddLocalNameToXPath(xPath, addLocalNameToXPath);
			var actual = xmlDoc2.SelectSingleNode(modifiedXPath)?.OuterXml ?? nodeNotFound;
			AssertEquals(messageType + " element should not be output to XML as it is not applicable (NA) : " + xPath, nodeNotFound, actual);
		}

		string AddLocalNameToXPath(string xPath, bool addLocalNameToXPath)
		{
			var modifiedXPath = xPath;
			if (addLocalNameToXPath)
			{
				modifiedXPath = ConvertToXPathWithLocalName(xPath);
			}
			return modifiedXPath;
		}

		public void TestC21E_MandatoryElements()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			const string c21E = ExportDeclarationTypeList.Codes.ExportClearanceRequestC21E;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(c21E);
			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			xmlDoc.LoadXml(xml);

			CombineAssertions("C21E Mandatory elements", () =>
			{
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/TypeCode", @"<TypeCode>EXA</TypeCode>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/GoodsItemQuantity", @"<GoodsItemQuantity>5</GoodsItemQuantity>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/GoodsShipment/PreviousDocument", @"<PreviousDocument><CategoryCode>Z</CategoryCode><ID>UCRREFERENCEPLACEHOLDER3A3C3059CEBF44D08B4E1DE9048AA854</ID><TypeCode>DCR</TypeCode></PreviousDocument>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/FunctionalReferenceID", @"<FunctionalReferenceID>LRNREFERENCEPLACEHOLDERF6AB79A7845D49FA82919554DED4F991</FunctionalReferenceID>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/Exporter", @"<Exporter><Name>00200</Name><Address><CityName>00200</CityName><CountryCode>HU</CountryCode><Line>00200</Line><PostcodeID>00200</PostcodeID></Address></Exporter>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/Declarant", @"<Declarant><ID>GB584361816000</ID></Declarant>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/Declarant/ID", @"<ID>GB584361816000</ID>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/ExitOffice/ID", @"<ID>P.Office</ID>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/GoodsShipment/Consignment/GoodsLocation", @"<GoodsLocation><Name>CW1234567</Name><TypeCode>B</TypeCode><Address><TypeCode>Y</TypeCode><CountryCode>GB</CountryCode></Address></GoodsLocation>");

				AssertXmlContainsElementsIfDataSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/SequenceNumeric", @"<SequenceNumeric>1</SequenceNumeric>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/GovernmentProcedure", @"<GovernmentProcedure><CurrentCode>31</CurrentCode><PreviousCode>71</PreviousCode></GovernmentProcedure>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/GovernmentProcedure/CurrentCode", @"<CurrentCode>31</CurrentCode>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/GovernmentProcedure/PreviousCode", @"<PreviousCode>71</PreviousCode>");
				AssertXmlContainsElementsIfDataSupplied(@"//*[local-name()='Declaration']/*[local-name()='GoodsShipment']/*[local-name()='GovernmentAgencyGoodsItem']/*[local-name()='GovernmentProcedure'][2]/*[local-name()='CurrentCode']", @"<CurrentCode>000</CurrentCode>", false);
			});
		}

		public void TestC21E_OptionalOrDependentElements()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			const string c21E = ExportDeclarationTypeList.Codes.ExportClearanceRequestC21E;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(c21E);

			var helper = new DeclarationTestHelper();
			var consignor = helper.CreateConsignor();
			consignor.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB9876543210", Core.Constants.CountryCodes.UnitedKingdom);
			consignor.Factory.Save();
			entryHeader.Declaration.JE_OA_ShipperAddress = consignor.MainAddress.PK;

			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			xmlDoc.LoadXml(xml);

			var entryHeader2 = helper.CreateBasicEntry(c21E, MessageTypeList.Codes.Export);
			var xml2 = GetXmlMessage_ForExportsMessageTesting(entryHeader2);
			xmlDoc2.LoadXml(xml2);

			//the commented out lines need to be set up in the bo/wrapper/builder so that the data comes out in the xml
			CombineAssertions("C21E Optional/Dependent elements should be in the XML if there is data", () =>
			{
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/SpecificCircumstancesCodeCode", @"<SpecificCircumstancesCodeCode>A20</SpecificCircumstancesCodeCode>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/GoodsShipment/PreviousDocument", @"<PreviousDocument><CategoryCode>Z</CategoryCode><ID>UCRREFERENCEPLACEHOLDER3A3C3059CEBF44D08B4E1DE9048AA854</ID><TypeCode>DCR</TypeCode></PreviousDocument>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/PreviousDocument", @"<PreviousDocument><CategoryCode>Z</CategoryCode><ID>MYINVOICE</ID><TypeCode>380</TypeCode></PreviousDocument>");
				//AssertXmlContainsElementsIfSupplied(@"Declaration/AdditionalInformation/StatementCode", @"");
				//AssertXmlContainsElementsIfSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/AdditionalInformation/StatementCode", @"");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/AdditionalDocument", @"<AdditionalDocument><CategoryCode>C</CategoryCode><EffectiveDateTime><DateTimeString formatCode=""102"" xmlns=""urn:wco:datamodel:WCO:Declaration_DS:DMS:2"">20190303</DateTimeString></EffectiveDateTime><ID>GBEIRGB077440010006</ID><TypeCode>514</TypeCode><Submitter><Name>CSI2</Name></Submitter><WriteOff><QuantityQuantity unitCode=""KGM"">13</QuantityQuantity></WriteOff></AdditionalDocument>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/GoodsShipment/UCR/TraderAssignedReferenceID", @"<TraderAssignedReferenceID>BGMREFERENCEPLACEHOLDER415A6A0C9690475B8AA2D2AE9D237BE4</TraderAssignedReferenceID>");
				//AssertXmlContainsElementsIfSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/UCR/TraderAssignedReferenceID", @"");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/FunctionalReferenceID", @"<FunctionalReferenceID>LRNREFERENCEPLACEHOLDERF6AB79A7845D49FA82919554DED4F991</FunctionalReferenceID>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/GoodsShipment/Warehouse", @"<Warehouse><ID>CP123</ID><TypeCode>C</TypeCode></Warehouse>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/Consignment/Consignor", @"<Consignor><ID>GB9876543210</ID></Consignor>");
				//AssertXmlContainsElementsIfSupplied(@"Declaration/Consignment/ConsignmentItem/Consignor", @"");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/GoodsShipment/Consignee", @"<Consignee><Name>Test Importer Name which is really </Name><Address><CountryCode>GB</CountryCode><Line>Test Importer Address Line which is</Line><PostcodeID>NA</PostcodeID></Address></Consignee>");
				//AssertXmlContainsElementsIfSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Consignee", @"");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/Agent", @"<Agent><FunctionCode>2</FunctionCode></Agent>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/Agent/FunctionCode", @"<FunctionCode>2</FunctionCode>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/Consignment/Carrier", @"<Carrier><ID>GB123456789000</ID></Carrier>");
				//AssertXmlContainsElementsIfSupplied(@"Declaration/GoodsShipment/AEOMutualRecognitionParty", @"");
				//AssertXmlContainsElementsIfSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/AEOMutualRecognitionParty", @"");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/AuthorisationHolder", @"<AuthorisationHolder><ID>GB123</ID><CategoryCode>ACE</CategoryCode></AuthorisationHolder>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/GoodsShipment/DomesticDutyTaxParty", @"<DomesticDutyTaxParty><ID>GB55555555</ID><RoleCode>FR1</RoleCode></DomesticDutyTaxParty>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/DomesticDutyTaxParty", @"<DomesticDutyTaxParty><ID>GB11111111</ID><RoleCode>FR1</RoleCode></DomesticDutyTaxParty>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/Consignment/Freight/PaymentMethodCode", @"<PaymentMethodCode>Y</PaymentMethodCode>");
				//AssertXmlContainsElementsIfSupplied(@"Declaration/Consignment/ConsignmentItem/Freight/PaymentMethodCode", @"");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/InvoiceAmount", @"<InvoiceAmount currencyID=""GBP"">1500.00</InvoiceAmount>");
				//AssertXmlContainsElementsIfSupplied(@"Declaration/CurrencyExchange/RateNumeric", @"");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/GoodsShipment/Destination/CountryCode", @"<CountryCode>HU</CountryCode>");
				//AssertXmlContainsElementsIfSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Destination/CountryCode", @"");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/ExitOffice/ID", @"<ID>P.Office</ID>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/GoodsShipment/ExportCountry/ID", @"<ID>HU</ID>");
				//AssertXmlContainsElementsIfSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Origin", @"");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/Consignment/Itinerary", @"<Itinerary><SequenceNumeric>1</SequenceNumeric><RoutingCountryCode>GB</RoutingCountryCode></Itinerary>");
				//AssertXmlContainsElementsIfSupplied(@"Declaration/PresentationOffice/ID", @"");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/SupervisingOffice/ID", @"<ID>GBBEL001</ID>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Commodity/GoodsMeasure/NetNetWeightMeasure", @"<NetNetWeightMeasure>100</NetNetWeightMeasure>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Commodity/GoodsMeasure/TariffQuantity", @"<TariffQuantity>0</TariffQuantity>");
				//AssertXmlContainsElementsIfSupplied(@"Declaration/TotalGrossMassMeasure", @"");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Commodity/GoodsMeasure/GrossMassMeasure", @"<GrossMassMeasure unitCode=""KGM"">0</GrossMassMeasure>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Commodity/Description", @"<Description>PARTS OF RAT</Description>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Packaging/TypeCode", @"<TypeCode>BX</TypeCode>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Packaging/QuantityQuantity", @"<QuantityQuantity>10</QuantityQuantity>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Packaging/MarksNumbersID", @"<MarksNumbersID>AS ABOVE</MarksNumbersID>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Commodity/DangerousGoods/UNDGID", @"<UNDGID>1234A</UNDGID>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Commodity/Classification", @"<Classification><ID>42021219</ID><IdentificationTypeCode>TSP</IdentificationTypeCode></Classification>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/TotalPackageQuantity", @"<TotalPackageQuantity>55</TotalPackageQuantity>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/GoodsShipment/Consignment/ContainerCode", @"<ContainerCode>1</ContainerCode>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/BorderTransportMeans/ModeCode", @"<ModeCode>1</ModeCode>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/GoodsShipment/Consignment/DepartureTransportMeans/ModeCode", @"<ModeCode>3</ModeCode>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/GoodsShipment/Consignment/DepartureTransportMeans", @"<DepartureTransportMeans><ID>BOATY MCBOATFACE</ID><IdentificationTypeCode>11</IdentificationTypeCode><ModeCode>3</ModeCode></DepartureTransportMeans>");
				//AssertXmlContainsElementsIfSupplied(@"Declaration/GoodsShipment/Consignment/TransportEquipment/ID", @"");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Commodity/TransportEquipment/ID", @"<ID>CNT123</ID>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/BorderTransportMeans", @"<BorderTransportMeans><ID>BOATY MCBOATFACE</ID><IdentificationTypeCode>11</IdentificationTypeCode><RegistrationNationalityCode>CN</RegistrationNationalityCode><ModeCode>1</ModeCode></BorderTransportMeans>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/BorderTransportMeans/RegistrationNationalityCode", @"<RegistrationNationalityCode>CN</RegistrationNationalityCode>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/GoodsShipment/Consignment/TransportEquipment/Seal", @"<Seal><SequenceNumeric>1</SequenceNumeric><ID>123</ID></Seal>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/ObligationGuarantee/SecurityDetailsCode", @"<SecurityDetailsCode>ACE</SecurityDetailsCode>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/ObligationGuarantee", @"<ObligationGuarantee><AmountAmount currencyID="""">0.00</AmountAmount><ID>456</ID><ReferenceID>789</ReferenceID><SecurityDetailsCode>ACE</SecurityDetailsCode><AccessCode>ACE</AccessCode></ObligationGuarantee>");
				//AssertXmlContainsElementsIfSupplied(@"Declaration/GoodsShipment/TransactionNatureCode", @"");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/TransactionNatureCode", @"<TransactionNatureCode>XX</TransactionNatureCode>");
				AssertXmlContainsElementsIfDataSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/StatisticalValueAmount", @"<StatisticalValueAmount currencyID=""GBP"">0.00</StatisticalValueAmount>");
				//AssertXmlContainsElementsIfSupplied(@"Declaration/AcceptanceDateTime", @"");
			});

			//the commented out lines asserts fail because entryHeader2 (empty data) contains data that causes elements to be be generated in the xml
			CombineAssertions("C21E Optional/Dependent elements should not be in the XML if there is no data", () =>
			{
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/SpecificCircumstancesCodeCode", @"");
				//AssertXmlDoesNotContainElementsIfNotSupplied(@"Declaration/GoodsShipment/PreviousDocument", @"<PreviousDocument><CategoryCode>Z</CategoryCode><ID>UCRREFERENCEPLACEHOLDER3A3C3059CEBF44D08B4E1DE9048AA854</ID><TypeCode>DCR</TypeCode></PreviousDocument>");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/PreviousDocument", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/AdditionalInformation/StatementCode", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/AdditionalInformation/StatementCode", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/AdditionalDocument", @"");
				//AssertXmlDoesNotContainElementsIfNotSupplied(@"Declaration/GoodsShipment/UCR/TraderAssignedReferenceID", @"<TraderAssignedReferenceID>BGMREFERENCEPLACEHOLDER415A6A0C9690475B8AA2D2AE9D237BE4</TraderAssignedReferenceID>");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/UCR/TraderAssignedReferenceID", @"");
				//AssertXmlDoesNotContainElementsIfNotSupplied(@"Declaration/FunctionalReferenceID", @"<FunctionalReferenceID>LRNREFERENCEPLACEHOLDERF6AB79A7845D49FA82919554DED4F991</FunctionalReferenceID>");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/GoodsShipment/Warehouse", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/Consignment/Consignor", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/Consignment/ConsignmentItem/Consignor", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/GoodsShipment/Consignee", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Consignee", @"");
				//AssertXmlDoesNotContainElementsIfNotSupplied(@"Declaration/Agent", @"<Agent><FunctionCode>2</FunctionCode></Agent>");
				//AssertXmlDoesNotContainElementsIfNotSupplied(@"Declaration/Agent/FunctionCode", @"<FunctionCode>2</FunctionCode>");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/Consignment/Carrier", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/GoodsShipment/AEOMutualRecognitionParty", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/AEOMutualRecognitionParty", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/AuthorisationHolder", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/GoodsShipment/DomesticDutyTaxParty", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/DomesticDutyTaxParty", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/Consignment/Freight/PaymentMethodCode", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/Consignment/ConsignmentItem/Freight/PaymentMethodCode", @"");
				//AssertXmlDoesNotContainElementsIfNotSupplied(@"Declaration/InvoiceAmount", @"<InvoiceAmount currencyID="">0.00</InvoiceAmount>");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/CurrencyExchange/RateNumeric", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/GoodsShipment/Destination/CountryCode", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Destination/CountryCode", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/ExitOffice/ID", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/GoodsShipment/ExportCountry/ID", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Origin", @"");
				//AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/Consignment/Itinerary", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/PresentationOffice/ID", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/SupervisingOffice/ID", @"");
				//AssertXmlDoesNotContainElementsIfNotSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Commodity/GoodsMeasure/NetNetWeightMeasure", @"<NetNetWeightMeasure>0</NetNetWeightMeasure>");
				//AssertXmlDoesNotContainElementsIfNotSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Commodity/GoodsMeasure/TariffQuantity", @"<TariffQuantity>0</TariffQuantity>");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/TotalGrossMassMeasure", @"");
				//AssertXmlDoesNotContainElementsIfNotSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Commodity/GoodsMeasure/GrossMassMeasure", @"<GrossMassMeasure unitCode=""KGM"">0</GrossMassMeasure>");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Commodity/Description", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Packaging/TypeCode", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Packaging/QuantityQuantity", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Packaging/MarksNumbersID", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Commodity/DangerousGoods/UNDGID", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Commodity/Classification", @"");
				//AssertXmlDoesNotContainElementsIfNotSupplied(@"Declaration/TotalPackageQuantity", @"<TotalPackageQuantity>0</TotalPackageQuantity>");
				//AssertXmlDoesNotContainElementsIfNotSupplied(@"Declaration/GoodsShipment/Consignment/ContainerCode", @"<ContainerCode>0</ContainerCode>");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/BorderTransportMeans/ModeCode", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/GoodsShipment/Consignment/DepartureTransportMeans/ModeCode", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/GoodsShipment/Consignment/DepartureTransportMeans", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/GoodsShipment/Consignment/TransportEquipment/ID", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Commodity/TransportEquipment/ID", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/BorderTransportMeans", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/BorderTransportMeans/RegistrationNationalityCode", @"");
				//AssertXmlDoesNotContainElementsIfNotSupplied(@"Declaration/GoodsShipment/Consignment/TransportEquipment/Seal", @"<Seal><SequenceNumeric>1</SequenceNumeric><ID>NOSEALS</ID></Seal>");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/ObligationGuarantee/SecurityDetailsCode", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/ObligationGuarantee", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/GoodsShipment/TransactionNatureCode", @"");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/TransactionNatureCode", @"");
				//AssertXmlDoesNotContainElementsIfNotSupplied(@"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/StatisticalValueAmount", @"<StatisticalValueAmount currencyID="GBP">0.00</StatisticalValueAmount>");
				AssertXmlDoesNotContainElementsIfDataNotSupplied(@"Declaration/AcceptanceDateTime", @"");
			});
		}

		public void TestC21E_NotApplicableElements()
		{
			const string c21E = ExportDeclarationTypeList.Codes.ExportClearanceRequestC21E;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(c21E);
			var guarantee2 = entryHeader.Declaration.Guarantees.AddNew();
			guarantee2.PW_BondType = GuaranteeTypeList.Codes.Guarantee;
			guarantee2.PW_Password = "BADA";
			guarantee2.PW_HolderIdentification = "BING";
			guarantee2.PW_BondNumber = "666";
			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			xmlDoc.LoadXml(xml);

			CombineAssertions("C21E Not Applicable (NA) elements", () =>
			{
				AssertXmlDoesNotContainElementsIfDataIsNotApplicable("Not set by BO/Wrapper but check it just incase", "Declaration/GoodsShipment/Consignment/GoodsLocation/Address/Line");
				AssertXmlDoesNotContainElementsIfDataIsNotApplicable("Not set by BO/Wrapper but check it just incase", "Declaration/GoodsShipment/Consignment/GoodsLocation/Address/PostcodeID");
				AssertXmlDoesNotContainElementsIfDataIsNotApplicable("Not set by BO/Wrapper but check it just incase", "Declaration/GoodsShipment/Consignment/GoodsLocation/Address/CityName");
				AssertXmlDoesNotContainElementsIfDataIsNotApplicable(entryHeader.Declaration.Guarantees[0].PW_BondNumber, "//*[local-name()='Declaration']/*[local-name()='ObligationGuarantee'][0]/*[local-name()='ReferenceID']", false);
				AssertXmlDoesNotContainElementsIfDataIsNotApplicable(entryHeader.Declaration.Guarantees[1].PW_BondNumber, "//*[local-name()='Declaration']/*[local-name()='ObligationGuarantee'][1]/*[local-name()='ReferenceID']", false);
			});
		}

		#endregion

		public void TestWarehouse()
		{
			OrgHeader warehouseOUTOF;
			JobDeclaration dec;
			CusEntryInstruction cei4071, cei4000, cei7100, cei7171, cei7200;
			TestDataHelper.CreateInstructionsForWarehouseAndGoodsLocationTest(Factory, out warehouseOUTOF, out dec, out cei4071, out cei4000, out cei7100, out cei7171, out cei7200);
			dec.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			dec.DoMerge();
			var ceh4071 = (CusEntryHeader)cei4071.EntryHeader;
			var ceh4000 = (CusEntryHeader)cei4000.EntryHeader;
			var ceh7100 = (CusEntryHeader)cei7100.EntryHeader;
			var ceh7171 = (CusEntryHeader)cei7171.EntryHeader;
			var ceh7200 = (CusEntryHeader)cei7200.EntryHeader;
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(dec);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, ceh4071), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml4071 = Regex.Replace(messageBuilder.Build(), @"\s+", "");
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, ceh4000), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml4000 = Regex.Replace(messageBuilder.Build(), @"\s+", "");
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, ceh7100), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml7100 = Regex.Replace(messageBuilder.Build(), @"\s+", "");
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, ceh7171), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml7171 = Regex.Replace(messageBuilder.Build(), @"\s+", "");
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, ceh7200), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml7200 = Regex.Replace(messageBuilder.Build(), @"\s+", "");

			// Ex-Warehouse into free circ
			AssertContains("<Warehouse><ID>7654321OUT</ID><TypeCode>U</TypeCode></Warehouse>", xml4071);

			// Free circ at frontier
			AssertNotContains("<Warehouse>", xml4000);

			// Frontier into warehouse
			AssertContains("<Warehouse><ID>1234567INN</ID><TypeCode>U</TypeCode></Warehouse>", xml7100);

			// Cross-warehouse
			AssertContains("<Warehouse><ID>1234567INN</ID><TypeCode>U</TypeCode></Warehouse>", xml7171);

			AssertNotContains("<Warehouse>", xml7200);
		}

		public void TestH2Message()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			entryHeader.CH_MasterUCR = "1234567890";
			var declaration = entryHeader.Declaration;
			declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.DeclarationForCustomsWarehousing;  // H2
			declaration.JE_MessageSubType = "IM";
			entryHeader.EntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.DeclarationForCustomsWarehousing;
			entryHeader.EntryInstruction.CEI_SubStyle = "Y";
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();
			AssertXMLEquals(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH2.xml"), xml);

			using (GBCustomsDataRegistry.Instance.SendNetMassForH2Declarations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var xml2 = messageBuilder.Build();
				AssertXMLEquals(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH2_WithNoNetMass61.xml"), xml2);
			}

			SetDataForTotalPackageQuantityTesting(entryHeader, 0, true);
			decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			xml = messageBuilder.Build();
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH2_WhenTotalPackageQuantityIsZero.xml"), xml);
		}

		public void TestH3Message()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			entryHeader.CH_MasterUCR = "1234567890";
			var declaration = entryHeader.Declaration;
			declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.DeclarationForTemporaryAdmission;  // H3
			declaration.JE_MessageSubType = "IM";
			entryHeader.EntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.DeclarationForTemporaryAdmission;
			entryHeader.EntryInstruction.CEI_SubStyle = "Y";
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLEquals(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH3.xml"), xml);
			using (GBCustomsDataRegistry.Instance.SendNetMassForH3Declarations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var xml2 = messageBuilder.Build();
				AssertXMLEquals(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH3_WithNoNetMass61.xml"), xml2);
			}

			SetDataForTotalPackageQuantityTesting(entryHeader, 0, true);
			decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			xml = messageBuilder.Build();
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH3_WhenTotalPackageQuantityIsZero.xml"), xml);
		}

		public void TestH4Message()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			entryHeader.CH_MasterUCR = "1234567890";
			var declaration = entryHeader.Declaration;
			declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.DeclarationForInwardProcessing;  // H4
			declaration.JE_MessageSubType = "IM";
			entryHeader.EntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.DeclarationForInwardProcessing;
			entryHeader.EntryInstruction.CEI_SubStyle = "Y";
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLEquals(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH4.xml"), xml);

			SetDataForTotalPackageQuantityTesting(entryHeader, 0, true);
			decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			xml = messageBuilder.Build();
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH4_WhenTotalPackageQuantityIsZero.xml"), xml);
		}

		public void TestC21IMessage()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			entryHeader.CH_MasterUCR = "1234567890";
			var declaration = entryHeader.Declaration;
			declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.ImportClearanceRequestC21I;  // 21I
			declaration.JE_MessageSubType = "IM";
			entryHeader.EntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.ImportClearanceRequestC21I;
			entryHeader.EntryInstruction.CEI_SubStyle = "Y";
			declaration.JE_GoodsOrigin = "LL";
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLEquals(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedC21I.xml"), xml);

			var longMarksAndNumbers = string.Join("", Enumerable.Repeat("0123456789ABCDEF", 40));
			var package = entryHeader.Declaration.Bills[1].PackingGroups[0].Packages[1];
			package.CW_MarksAndNos = longMarksAndNumbers;
			var messageBuilder2 = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml2 = messageBuilder2.Build();

			var expectedMarksAndNumbers = longMarksAndNumbers.Substring(0, 512);
			AssertXMLContains($"<MarksNumbersID>{expectedMarksAndNumbers}</MarksNumbersID>", xml2);

			SetDataForTotalPackageQuantityTesting(entryHeader, 55, false);
			decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			xml = messageBuilder.Build();
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedC21I_WhenTotalPackageQuantityIsNonZeroThroughNonInventoryLocation.xml"), xml);

			SetDataForTotalPackageQuantityTesting(entryHeader, 0, true);
			decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			xml = messageBuilder.Build();
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedC21I_WhenTotalPackageQuantityIsZeroThroughInventoryLocation.xml"), xml);
		}

		public void TestC21IMessage_Exporter()
		{
			const string c21I = ImportDeclarationTypeList.Codes.ImportClearanceRequestC21I;
			var helper = new DeclarationTestHelper();
			var entryHeader = helper.CreateBasicEntry(c21I, MessageTypeList.Codes.Import);
			var declaration = entryHeader.Declaration;
			var org2 = helper.MakeOrganisation2();
			org2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB025115100006", Core.Constants.CountryCodes.UnitedKingdom);
			declaration.SupplierDocumentaryAddress.OrganisationPK = org2.PK;
			entryHeader.EntryInstruction.CEI_Style = c21I;

			var org = declaration.SupplierDocumentaryAddress.Organisation;
			AssertEquals("Pre-req exporter id", "GB025115100006", org.GetEuIdentificationNumber());
			AssertEquals("Pre-req exporter name", "Daniel", org.OH_FullName);
			var address = org.MainAddress;
			AssertEquals("Pre-req exporter address 1", "High St", address.OA_Address1);
			AssertEquals("Pre-req exporter address 2", "New Bradwell", address.OA_Address2);
			AssertEquals("Pre-req exporter city", "Milton Keynes", address.OA_City);
			AssertEquals("Pre-req exporter post code", "MK1", address.OA_PostCode);

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();
			xmlDoc.LoadXml(xml);

			//Assert No Id just name and address
			AssertContains(@"<Exporter>
      <Name>Daniel</Name>
      <Address>
        <CityName>Milton Keynes</CityName>
        <CountryCode>GB</CountryCode>
        <Line>High St New Bradwell</Line>
        <PostcodeID>MK1</PostcodeID>
      </Address>
    </Exporter>", xml);
		}

		public void TestC21IMessage_DomesticDutyTaxParty()
		{
			const string c21I = ImportDeclarationTypeList.Codes.ImportClearanceRequestC21I;
			var helper = new DeclarationTestHelper();
			var entryHeader = helper.CreateBasicEntry(c21I, MessageTypeList.Codes.Import);
			var declaration = entryHeader.Declaration;

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();
			xmlDoc.LoadXml(xml);

			AssertNotContains(@"No DomesticDutyTaxParty elements block", xml);

			entryHeader = GetEntryHeader_ForExportsMessageTesting(c21I);
			declaration = entryHeader.Declaration;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			AssertEquals("pre-req: there are fiscal references", true, entryHeader.FiscalReferences.Any());

			decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			xml = messageBuilder.Build();
			xmlDoc.LoadXml(xml);

			AssertContains(@"<DomesticDutyTaxParty>
        <ID>GB55555555</ID>
        <RoleCode>FR1</RoleCode>
      </DomesticDutyTaxParty>", xml);
		}

		public void TestC21IMessage_InvoiceAmount()
		{
			const string c21I = ImportDeclarationTypeList.Codes.ImportClearanceRequestC21I;
			var helper = new DeclarationTestHelper();
			var entryHeader = helper.CreateBasicEntry(c21I, MessageTypeList.Codes.Import);
			var declaration = entryHeader.Declaration;

			AssertEquals("Pre-req total amount", 1234.56m, declaration.TotalInvoiceAmount.Amount);
			AssertEquals("Pre-req amount currency", "GBP", declaration.TotalInvoiceAmount.Currency.Code);

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();
			xmlDoc.LoadXml(xml);

			AssertNotContains(@"<DeclarationInvoiceAmount>", xml);
		}

		public void TestC21IMessage_BorderTransportMeans()
		{
			const string c21I = ImportDeclarationTypeList.Codes.ImportClearanceRequestC21I;
			var helper = new DeclarationTestHelper();
			var entryHeader = helper.CreateBasicEntry(c21I, MessageTypeList.Codes.Import);
			var declaration = entryHeader.Declaration;
			declaration.JE_TransportMode = "AIR";
			declaration.JE_VoyageFlightNo = "BA123";
			declaration.JE_RN_NKTransportNationality = "GB";

			AssertEquals("Pre-req RegistrationNationalityCode", "GB", declaration.JE_RN_NKTransportNationality);

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();
			xmlDoc.LoadXml(xml);

			AssertNotContains("Expect no RegistrationNationalityCode in XML", "RegistrationNationalityCode", xml);
			AssertContains(@"<BorderTransportMeans>
      <IdentificationTypeCode>40</IdentificationTypeCode>
      <ModeCode>4</ModeCode>
    </BorderTransportMeans>", xml);
		}

		public void TestC21IMessage_DeclarationGoodsShipmentConsignmentArrivalTransportMeans()
		{
			const string c21I = ImportDeclarationTypeList.Codes.ImportClearanceRequestC21I;
			var helper = new DeclarationTestHelper();
			var entryHeader = helper.CreateBasicEntry(c21I, MessageTypeList.Codes.Import);
			var declaration = entryHeader.Declaration;
			entryHeader.Declaration.JE_TransportMode = "AIR";

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();
			xmlDoc.LoadXml(xml);

			AssertNotContains("No DeclarationGoodsShipmentConsignmentArrivalTransportMeans if no data", "ArrivalTransportMeans", xml);

			declaration.JE_TransportMode = "AIR";
			declaration.JE_VoyageFlightNo = "BA123";
			declaration.JE_RN_NKTransportNationality = "GB";

			decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			xml = messageBuilder.Build();
			xmlDoc.LoadXml(xml);

			AssertContains(@"<ArrivalTransportMeans>
          <ID>BA123</ID>
          <IdentificationTypeCode>40</IdentificationTypeCode>
        </ArrivalTransportMeans>", xml);
		}

		public void TestC21IMessageWithEmptyInvoiceCurrencySkipsInvoiceLine()
		{
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var declaration = entryHeader.Declaration;
			declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.ImportClearanceRequestC21I;
			declaration.JE_MessageSubType = "IM";
			entryHeader.EntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.ImportClearanceRequestC21I;
			entryHeader.EntryInstruction.CEI_SubStyle = "J";

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());

			var invoice = entryHeader.Declaration.Invoices[0];
			invoice.InvoiceLines[3].JI_LinePrice = 0.00;
			var xml = messageBuilder.Build();

			AssertXMLContains(@"<SequenceNumeric>4</SequenceNumeric>
        <AdditionalDocument>
          <CategoryCode>C</CategoryCode>
          <ID>GBEIRGB077440010006</ID>
          <TypeCode>514</TypeCode>
        </AdditionalDocument>
        <Commodity>
          <InvoiceLine>
            <ItemChargeAmount currencyID=""GBP"">0.00</ItemChargeAmount>
          </InvoiceLine>
        </Commodity>
        <Consignor>
          <Name>Daniel</Name>
          <ID>GB696969699000</ID>
          <Address>
            <CityName>Milton Keynes</CityName>
            <CountryCode>GB</CountryCode>
            <Line>High St New Bradwell</Line>
            <PostcodeID>MK1</PostcodeID>
          </Address>
        </Consignor>", xml);

			invoice.JZ_RX_NKInvoice_Currency = "";
			invoice.InvoiceLines[3].JI_LinePrice = 123.45;
			xml = messageBuilder.Build();

			AssertXMLContains(@"<SequenceNumeric>4</SequenceNumeric>
        <AdditionalDocument>
          <CategoryCode>C</CategoryCode>
          <ID>GBEIRGB077440010006</ID>
          <TypeCode>514</TypeCode>
        </AdditionalDocument>
        <Consignor>
          <Name>Daniel</Name>
          <ID>GB696969699000</ID>
          <Address>
            <CityName>Milton Keynes</CityName>
            <CountryCode>GB</CountryCode>
            <Line>High St New Bradwell</Line>
            <PostcodeID>MK1</PostcodeID>
          </Address>
        </Consignor>", xml);
		}

		public void TestC21IMessageWithForeignEORI()
		{
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var declaration = entryHeader.Declaration;
			declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.ImportClearanceRequestC21I;
			declaration.JE_MessageSubType = "IM";
			entryHeader.EntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.ImportClearanceRequestC21I;
			entryHeader.EntryInstruction.CEI_SubStyle = "J";

			var invoice = entryHeader.Declaration.Invoices[0];
			var helper = new DeclarationTestHelper(Factory);
			var deSupplier = helper.GetNewOrganisation("", "DDE", "German", "Main St", "", "Munich", "NA", "1234", "DEMUN", "12345", "654987", "696969699000", Core.Constants.CountryCodes.Germany);
			deSupplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "DE696969699000", Core.Constants.CountryCodes.Germany);
			invoice.JZ_OA_SupplierAddress = deSupplier.Addresses[0].PK;
			invoice.JZ_OA_SellerAddress = deSupplier.Addresses[0].PK;

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			invoice.InvoiceLines[3].JI_LinePrice = 0.00;
			var xml = messageBuilder.Build();

			AssertXMLContains(@"<SequenceNumeric>4</SequenceNumeric>
        <AdditionalDocument>
          <CategoryCode>C</CategoryCode>
          <ID>GBEIRGB077440010006</ID>
          <TypeCode>514</TypeCode>
        </AdditionalDocument>
        <Commodity>
          <InvoiceLine>
            <ItemChargeAmount currencyID=""GBP"">0.00</ItemChargeAmount>
          </InvoiceLine>
        </Commodity>
        <Consignor>
          <Name>German</Name>
          <Address>
            <CityName>Munich</CityName>
            <CountryCode>DE</CountryCode>
            <Line>Main St</Line>
            <PostcodeID>1234</PostcodeID>
          </Address>
        </Consignor>", xml);
		}

		public void TestC21NMessage()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var declaration = entryHeader.Declaration;
			declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.ImportClearanceRequestC21N;
			declaration.JE_MessageSubType = "IM";
			entryHeader.EntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.ImportClearanceRequestC21N;
			entryHeader.EntryInstruction.CEI_SubStyle = "Y";
			entryHeader.CH_MasterUCR = "1234567890";

			var orgA = CreateRepresentativeForAgentRepresentationTests("ACME AR1", "ACME", "Copse lane 1", "Hampshire", "Southampton", "England", "SO16");
			orgA.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORIAR1", "GB");
			var addressA = orgA.Addresses[0];
			entryHeader.Declaration.JE_OA_Representative = addressA.PK;
			entryHeader.Declaration.RepresentativeDocAddress.OrganisationPK = orgA.PK;

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedC21N.xml"), xml);

			SetDataForTotalPackageQuantityTesting(entryHeader, 55, false);
			decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			xml = messageBuilder.Build();
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedC21N_WhenTotalPackageQuantityIsNonZeroThroughNonInventoryLocation.xml"), xml);

			SetDataForTotalPackageQuantityTesting(entryHeader, 0, true);
			decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			xml = messageBuilder.Build();
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedC21N_WhenTotalPackageQuantityIsZeroThroughInventoryLocation.xml"), xml);
		}

		public void TestC21BMessage()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			entryHeader.EntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.BulkImportReducedDataSet;
			entryHeader.EntryInstruction.CEI_SubStyle = "J";
			entryHeader.InvoiceHeaders[1].JZ_OH_Supplier = entryHeader.InvoiceHeaders[0].JZ_OH_Supplier;

			var declaration = entryHeader.Declaration;
			declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.BulkImportReducedDataSet;
			declaration.JE_MessageSubType = "IM";

			var orgImporter = CreateOrgHeaderWithEori("234", "GB");
			entryHeader.Declaration.ImporterDocumentaryAddress.OrganisationPK = orgImporter.PK;

			var orgAgent = CreateRepresentativeForAgentRepresentationTests("ACME AR1", "ACME", "Copse lane 1", "Hampshire", "Southampton", "England", "SO16");
			orgAgent.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORIAR1", "GB");
			entryHeader.Declaration.JE_OA_Representative = orgAgent.Addresses[0].PK;
			entryHeader.Declaration.RepresentativeDocAddress.OrganisationPK = orgAgent.PK;

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLEquals(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedC21B.xml"), xml);
		}

		public void TestC21BMessageWithEmptyInvoiceCurrencySkipsInvoiceLine()
		{
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var declaration = entryHeader.Declaration;
			declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.BulkImportReducedDataSet;
			declaration.JE_MessageSubType = "IM";
			entryHeader.EntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.BulkImportReducedDataSet;
			entryHeader.EntryInstruction.CEI_SubStyle = "J";

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());

			var invoice = entryHeader.Declaration.Invoices[0];
			invoice.InvoiceLines[3].JI_LinePrice = 0.00;
			var xml = messageBuilder.Build();

			AssertXMLContains(@"<SequenceNumeric>4</SequenceNumeric>
        <AdditionalDocument>
          <CategoryCode>C</CategoryCode>
          <ID>GBEIRGB077440010006</ID>
          <TypeCode>514</TypeCode>
        </AdditionalDocument>
        <Commodity>
          <GoodsMeasure>
            <GrossMassMeasure>0</GrossMassMeasure>
            <TariffQuantity>0</TariffQuantity>
          </GoodsMeasure>
          <InvoiceLine>
            <ItemChargeAmount currencyID=""GBP"">0.00</ItemChargeAmount>
          </InvoiceLine>
        </Commodity>", xml);

			invoice.JZ_RX_NKInvoice_Currency = "";
			invoice.InvoiceLines[3].JI_LinePrice = 123.45;
			xml = messageBuilder.Build();

			AssertXMLContains(@"<SequenceNumeric>4</SequenceNumeric>
        <AdditionalDocument>
          <CategoryCode>C</CategoryCode>
          <ID>GBEIRGB077440010006</ID>
          <TypeCode>514</TypeCode>
        </AdditionalDocument>
        <Commodity>
          <GoodsMeasure>
            <GrossMassMeasure>0</GrossMassMeasure>
            <TariffQuantity>0</TariffQuantity>
          </GoodsMeasure>
        </Commodity>", xml);
		}

		public void TestH5Message_HasNoDomesticDutyTaxParty()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			const string h5 = ImportDeclarationTypeList.Codes.DeclarationForGoodsFromTheSpecialFiscalTerritories;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(h5, MessageTypeList.Codes.Import, false);
			var declaration = entryHeader.Declaration;

			AssertEquals("pre-req: there are fiscal references", true, entryHeader.FiscalReferences.Any());

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();
			xmlDoc.LoadXml(xml);

			AssertNotContains(@"<DomesticDutyTaxParty>", xml);
		}

		public void TestC21IMessage_With0008()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			CreateProcedures(Factory, new ZString[] { "0008000", "0009000" });
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			entryHeader.CH_MasterUCR = "1234567890";
			var declaration = entryHeader.Declaration;
			declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.ImportClearanceRequestC21I;  // 21I
			declaration.JE_MessageSubType = "IM";
			entryHeader.EntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.ImportClearanceRequestC21I;
			entryHeader.EntryInstruction.CEI_SubStyle = "Y";
			declaration.JE_GoodsOrigin = "LL";
			declaration.InvoiceLines[0].JI_Procedure = "0008000";
			declaration.InvoiceLines[1].JI_Procedure = "0008000";
			declaration.InvoiceLines[2].JI_Procedure = "0009000";
			declaration.InvoiceLines[3].JI_Procedure = "0009000";
			declaration.InvoiceLines[4].JI_Procedure = "0009000";
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml("Messaging.ExpectedC21I_0008.xml"), xml);
		}

		public void TestC21IMessage_OmitAgentWithProcedureCode0009Or0019()
		{
			CreateProcedures(Factory, new ZString[] { "0008000", "0009000", "0019000" });
			var agent = CreateRepresentativeForAgentRepresentationTests("MY CORP PTY", "MYCORP", "21 Long str", "Buckingham", "London", "England", "1234", true);
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			entryHeader.CH_MasterUCR = "1234567890";
			entryHeader.EntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.ImportClearanceRequestC21I;
			entryHeader.EntryInstruction.CEI_SubStyle = "Y";
			var declaration = entryHeader.Declaration;
			declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.ImportClearanceRequestC21I;
			declaration.JE_MessageSubType = "IM";
			declaration.JE_OA_Representative = agent.Addresses[0].PK;
			declaration.RepresentativeDocAddress.OrganisationPK = agent.PK;

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var sendingObject = MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader);
			var newMessageFunction = new Customs.Business.CusdecMessageFunction.New();
			var agentXml = "<Agent>\r\n      <ID>GBEORI12</ID>\r\n      <FunctionCode>2</FunctionCode>\r\n    </Agent>";

			declaration.InvoiceLines[0].JI_Procedure = "0008000";
			var messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject, new ErrorCollector(), newMessageFunction);
			var xml = messageBuilder.Build();
			AssertContains(agentXml, xml);

			declaration.InvoiceLines[0].JI_Procedure = "0009000";
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject, new ErrorCollector(), newMessageFunction);
			xml = messageBuilder.Build();
			AssertNotContains(agentXml, xml);

			declaration.InvoiceLines[0].JI_Procedure = "0019000";
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject, new ErrorCollector(), newMessageFunction);
			xml = messageBuilder.Build();
			AssertNotContains(agentXml, xml);
		}

		public void TestH5Message()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			entryHeader.CH_MasterUCR = "1234567890";

			var declaration = entryHeader.Declaration;
			declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.DeclarationForGoodsFromTheSpecialFiscalTerritories;  // H5
			declaration.JE_MessageSubType = "IM";
			entryHeader.EntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.DeclarationForGoodsFromTheSpecialFiscalTerritories;
			entryHeader.EntryInstruction.CEI_SubStyle = "Y";
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLEquals(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH5.xml"), xml);

			SetDataForTotalPackageQuantityTesting(entryHeader, 0, true);
			decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			xml = messageBuilder.Build();
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH5_WhenTotalPackageQuantityIsZero.xml"), xml);
		}

		public void TestH5MessageWithDUCRSuffix()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			entryHeader.CH_MasterUCR = "1234567890";
			entryHeader.CH_BGMReference += @"/2";

			var declaration = entryHeader.Declaration;
			declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.DeclarationForGoodsFromTheSpecialFiscalTerritories;  // H5
			declaration.JE_MessageSubType = "IM";
			entryHeader.EntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.DeclarationForGoodsFromTheSpecialFiscalTerritories;
			entryHeader.EntryInstruction.CEI_SubStyle = "Y";
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertContains(@"<PreviousDocument>
        <CategoryCode>Z</CategoryCode>
        <ID>UCRREFERENCEPLACEHOLDER3A3C3059CEBF44D08B4E1DE9048AA854</ID>
        <TypeCode>DCR</TypeCode>
      </PreviousDocument>
      <PreviousDocument>
        <CategoryCode>Z</CategoryCode>
        <ID>2</ID>
        <TypeCode>DCS</TypeCode>
        <LineNumeric>1</LineNumeric>
      </PreviousDocument>", xml);
		}

		public void TestMessageForGuaranteeRelatedEntryInstruction_H1()
		{
			var entryHeader = CreateSampleEntryHeaderForGuaranteeRelatedEntryInstruction(Factory);
			var declaration = entryHeader.Declaration;
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertContains(
@"</TotalPackageQuantity>
    <Agent>
      <FunctionCode>2</FunctionCode>
    </Agent>
    <AuthorisationHolder>
      <ID>GB123</ID>
      <CategoryCode>ACE</CategoryCode>
    </AuthorisationHolder>
    <AuthorisationHolder>
      <ID>GB456</ID>
      <CategoryCode>SDE</CategoryCode>
    </AuthorisationHolder>
    <BorderTransportMeans>",
				xml);
		}

		public void TestI1Message()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var declaration = entryHeader.Declaration;
			declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.ImportSimplifiedDeclaration;
			declaration.JE_MessageSubType = "IM";
			entryHeader.EntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.ImportSimplifiedDeclaration;
			entryHeader.EntryInstruction.CEI_SubStyle = "Y";
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLEquals(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedI1.xml"), xml);

			SetDataForTotalPackageQuantityTesting(entryHeader, 0, false);
			decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			xml = messageBuilder.Build();
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedI1_WhenTotalPackageQuantityIsZero.xml"), xml);
		}

		public void TestI1MessageWithEntryLineUsingControlledGoodsProcedure()
		{
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var declaration = entryHeader.Declaration;
			declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.ImportSimplifiedDeclaration;
			declaration.JE_MessageSubType = "IM";
			entryHeader.EntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.ImportSimplifiedDeclaration;
			entryHeader.EntryInstruction.CEI_SubStyle = "Y";
			var invoice = entryHeader.Declaration.Invoices[0];
			invoice.InvoiceLines[3].JI_Procedure = "01001CD";

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLContains(@"<DutyTaxFee>
            <DutyRegimeCode>100</DutyRegimeCode>
            <QuotaOrderID>123456</QuotaOrderID>
            <Payment>
              <MethodCode>E</MethodCode>
            </Payment>
          </DutyTaxFee>
          <DutyTaxFee>
            <SpecificTaxBaseQuantity unitCode=""HLT"">60</SpecificTaxBaseQuantity>
          </DutyTaxFee>
          <DutyTaxFee>
            <SpecificTaxBaseQuantity unitCode=""DTN#E"">5</SpecificTaxBaseQuantity>
          </DutyTaxFee>", xml);
		}

		public void TestI1MessageWithEmptyInvoiceCurrencyOrZeroPriceSkipsInvoiceLine()
		{
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var declaration = entryHeader.Declaration;
			declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.ImportSimplifiedDeclaration;
			declaration.JE_MessageSubType = "IM";
			entryHeader.EntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.ImportSimplifiedDeclaration;
			entryHeader.EntryInstruction.CEI_SubStyle = "Y";

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());

			var invoice = entryHeader.Declaration.Invoices[0];
			invoice.InvoiceLines[3].JI_LinePrice = 0.00;
			var xml = messageBuilder.Build();

			AssertXMLContains(@"<SequenceNumeric>4</SequenceNumeric>
        <AdditionalDocument>
          <CategoryCode>C</CategoryCode>
          <ID>GBEIRGB077440010006</ID>
          <TypeCode>514</TypeCode>
        </AdditionalDocument>
        <Commodity>
          <GoodsMeasure>
            <GrossMassMeasure>0</GrossMassMeasure>
            <NetNetWeightMeasure>0</NetNetWeightMeasure>
            <TariffQuantity>0</TariffQuantity>
          </GoodsMeasure>
        </Commodity>", xml);

			invoice.JZ_RX_NKInvoice_Currency = "";
			invoice.InvoiceLines[3].JI_LinePrice = 123.45;
			xml = messageBuilder.Build();

			AssertXMLContains(@"<SequenceNumeric>4</SequenceNumeric>
        <AdditionalDocument>
          <CategoryCode>C</CategoryCode>
          <ID>GBEIRGB077440010006</ID>
          <TypeCode>514</TypeCode>
        </AdditionalDocument>
        <Commodity>
          <GoodsMeasure>
            <GrossMassMeasure>0</GrossMassMeasure>
            <NetNetWeightMeasure>0</NetNetWeightMeasure>
            <TariffQuantity>0</TariffQuantity>
          </GoodsMeasure>
        </Commodity>", xml);
		}

		public void TestI1MessageWithQuotaSendsInvoiceLine()
		{
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var declaration = entryHeader.Declaration;
			declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.ImportSimplifiedDeclaration;
			declaration.JE_MessageSubType = "IM";
			entryHeader.EntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.ImportSimplifiedDeclaration;
			entryHeader.EntryInstruction.CEI_SubStyle = "Y";

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());

			var invoice = entryHeader.Declaration.Invoices[0];
			invoice.InvoiceLines[3].JI_LinePrice = 0.00;
			invoice.InvoiceLines[3].JI_ConcessionOrder = "THING";
			var xml = messageBuilder.Build();

			AssertXMLContains(@"<SequenceNumeric>4</SequenceNumeric>
        <AdditionalDocument>
          <CategoryCode>C</CategoryCode>
          <ID>GBEIRGB077440010006</ID>
          <TypeCode>514</TypeCode>
        </AdditionalDocument>
        <Commodity>
          <DutyTaxFee>
            <QuotaOrderID>THING </QuotaOrderID>
          </DutyTaxFee>
          <GoodsMeasure>
            <GrossMassMeasure>0</GrossMassMeasure>
            <NetNetWeightMeasure>0</NetNetWeightMeasure>
            <TariffQuantity>0</TariffQuantity>
          </GoodsMeasure>
          <InvoiceLine>
            <ItemChargeAmount currencyID=""GBP"">0.00</ItemChargeAmount>
          </InvoiceLine>
        </Commodity>", xml);

			invoice.JZ_RX_NKInvoice_Currency = "";
			invoice.InvoiceLines[3].JI_LinePrice = 123.45;
			xml = messageBuilder.Build();

			AssertXMLContains(@"<SequenceNumeric>4</SequenceNumeric>
        <AdditionalDocument>
          <CategoryCode>C</CategoryCode>
          <ID>GBEIRGB077440010006</ID>
          <TypeCode>514</TypeCode>
        </AdditionalDocument>
        <Commodity>
          <DutyTaxFee>
            <QuotaOrderID>THING </QuotaOrderID>
          </DutyTaxFee>
          <GoodsMeasure>
            <GrossMassMeasure>0</GrossMassMeasure>
            <NetNetWeightMeasure>0</NetNetWeightMeasure>
            <TariffQuantity>0</TariffQuantity>
          </GoodsMeasure>
          <InvoiceLine>
            <ItemChargeAmount currencyID="""">123.45</ItemChargeAmount>
          </InvoiceLine>
        </Commodity>", xml);
		}

		public void TestI1MessageControlledGoodsWithE01orE02SkipsInvoiceLine()
		{
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var declaration = entryHeader.Declaration;
			declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.ImportSimplifiedDeclaration;
			declaration.JE_MessageSubType = "IM";
			entryHeader.EntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.ImportSimplifiedDeclaration;
			entryHeader.EntryInstruction.CEI_SubStyle = "Y";
			var invoice = entryHeader.Declaration.Invoices[0];
			invoice.InvoiceLines[3].JI_Procedure = "01001CD";
			var code = invoice.InvoiceLines[3].AdditionalProcedureCodes.AddNew("E01");
			invoice.InvoiceLines[3].JI_LinePrice = 0.00;

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLContains(@"<SequenceNumeric>4</SequenceNumeric>
        <AdditionalDocument>
          <CategoryCode>C</CategoryCode>
          <ID>GBEIRGB077440010006</ID>
          <TypeCode>514</TypeCode>
        </AdditionalDocument>
        <Commodity>
          <GoodsMeasure>
            <GrossMassMeasure>0</GrossMassMeasure>
            <NetNetWeightMeasure>0</NetNetWeightMeasure>
            <TariffQuantity>0</TariffQuantity>
          </GoodsMeasure>
        </Commodity>", xml);

			invoice.InvoiceLines[3].JI_LinePrice = 123.45;
			xml = messageBuilder.Build();

			AssertXMLContains(@"<SequenceNumeric>4</SequenceNumeric>
        <AdditionalDocument>
          <CategoryCode>C</CategoryCode>
          <ID>GBEIRGB077440010006</ID>
          <TypeCode>514</TypeCode>
        </AdditionalDocument>
        <Commodity>
          <GoodsMeasure>
            <GrossMassMeasure>0</GrossMassMeasure>
            <NetNetWeightMeasure>0</NetNetWeightMeasure>
            <TariffQuantity>0</TariffQuantity>
          </GoodsMeasure>
          <InvoiceLine>
            <ItemChargeAmount currencyID=""GBP"">123.45</ItemChargeAmount>
          </InvoiceLine>
        </Commodity>", xml);

			code.CY_Code = "E02";
			xml = messageBuilder.Build();

			AssertXMLContains(@"<SequenceNumeric>4</SequenceNumeric>
        <AdditionalDocument>
          <CategoryCode>C</CategoryCode>
          <ID>GBEIRGB077440010006</ID>
          <TypeCode>514</TypeCode>
        </AdditionalDocument>
        <Commodity>
          <GoodsMeasure>
            <GrossMassMeasure>0</GrossMassMeasure>
            <NetNetWeightMeasure>0</NetNetWeightMeasure>
            <TariffQuantity>0</TariffQuantity>
          </GoodsMeasure>
          <InvoiceLine>
            <ItemChargeAmount currencyID=""GBP"">123.45</ItemChargeAmount>
          </InvoiceLine>
        </Commodity>", xml);

			invoice.InvoiceLines[3].JI_LinePrice = 0m;
			xml = messageBuilder.Build();

			AssertXMLContains(@"<SequenceNumeric>4</SequenceNumeric>
        <AdditionalDocument>
          <CategoryCode>C</CategoryCode>
          <ID>GBEIRGB077440010006</ID>
          <TypeCode>514</TypeCode>
        </AdditionalDocument>
        <Commodity>
          <GoodsMeasure>
            <GrossMassMeasure>0</GrossMassMeasure>
            <NetNetWeightMeasure>0</NetNetWeightMeasure>
            <TariffQuantity>0</TariffQuantity>
          </GoodsMeasure>
        </Commodity>", xml);
		}

		public void TestI1MessageControlledGoodsWithoutE01orE02SendsInvoiceLine()
		{
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var declaration = entryHeader.Declaration;
			declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.ImportSimplifiedDeclaration;
			declaration.JE_MessageSubType = "IM";
			entryHeader.EntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.ImportSimplifiedDeclaration;
			entryHeader.EntryInstruction.CEI_SubStyle = "Y";
			var invoice = entryHeader.Declaration.Invoices[0];
			invoice.InvoiceLines[3].JI_Procedure = "01001CD";
			invoice.InvoiceLines[3].JI_LinePrice = 0.00;

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLContains(@"<SequenceNumeric>4</SequenceNumeric>
        <AdditionalDocument>
          <CategoryCode>C</CategoryCode>
          <ID>GBEIRGB077440010006</ID>
          <TypeCode>514</TypeCode>
        </AdditionalDocument>
        <Commodity>
          <GoodsMeasure>
            <GrossMassMeasure>0</GrossMassMeasure>
            <NetNetWeightMeasure>0</NetNetWeightMeasure>
            <TariffQuantity>0</TariffQuantity>
          </GoodsMeasure>
          <InvoiceLine>
            <ItemChargeAmount currencyID=""GBP"">0.00</ItemChargeAmount>
          </InvoiceLine>
        </Commodity>", xml);

			invoice.InvoiceLines[3].JI_LinePrice = 123.45;
			xml = messageBuilder.Build();

			AssertXMLContains(@"<SequenceNumeric>4</SequenceNumeric>
        <AdditionalDocument>
          <CategoryCode>C</CategoryCode>
          <ID>GBEIRGB077440010006</ID>
          <TypeCode>514</TypeCode>
        </AdditionalDocument>
        <Commodity>
          <GoodsMeasure>
            <GrossMassMeasure>0</GrossMassMeasure>
            <NetNetWeightMeasure>0</NetNetWeightMeasure>
            <TariffQuantity>0</TariffQuantity>
          </GoodsMeasure>
          <InvoiceLine>
            <ItemChargeAmount currencyID=""GBP"">123.45</ItemChargeAmount>
          </InvoiceLine>
        </Commodity>", xml);
		}

		public void TestH1MessageWithTwoFeesOnlyShowsOnePreference()
		{
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var entryLine1 = entryHeader.MergedLines[0];
			var feeWithePercentage = entryLine1.Fees.Cast<CusEntryLineFee>().FirstOrDefault(x => x.CF_MethodOfCalculation == MethodOfCalculation.Percentage);
			entryLine1.Fees.RemoveAndDeleteAll();
			var fee1 = entryLine1.Fees.AddNew();
			fee1.CF_ChargeType = "A50";
			fee1.CF_MethodOfCalculation = MethodOfCalculation.Percentage;
			fee1.CF_BaseValue = 17500m;
			var fee2 = entryLine1.Fees.AddNew();
			fee2.CF_ChargeType = "A70";
			fee2.CF_MethodOfCalculation = MethodOfCalculation.Percentage;
			fee2.CF_BaseValue = 17500m;
			entryLine1.RandomLine.ZG_MethodOfPayment = "";
			entryLine1.RandomLine.JI_ConcessionOrder = "";

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLContains(@"</Classification>
          <DutyTaxFee>
            <DutyRegimeCode>100</DutyRegimeCode>
            <Payment>
              <MethodCode>E</MethodCode>
            </Payment>
          </DutyTaxFee>
          <DutyTaxFee>
            <Payment>
              <MethodCode>E</MethodCode>
            </Payment>
          </DutyTaxFee>
          <GoodsMeasure>", xml);
		}

		public void TestH1MessageWithNoPercentageCalculationMethod()
		{
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var entryLine1 = entryHeader.MergedLines[0];
			var feeWithePercentage = entryLine1.Fees.Cast<CusEntryLineFee>().FirstOrDefault(x => x.CF_MethodOfCalculation == MethodOfCalculation.Percentage);
			entryLine1.Fees.RemoveAndDelete(feeWithePercentage);

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLContains(@"</Classification>
          <DutyTaxFee>
            <DutyRegimeCode>100</DutyRegimeCode>
            <SpecificTaxBaseQuantity unitCode=""HLT"">60</SpecificTaxBaseQuantity>
            <QuotaOrderID>123456</QuotaOrderID>
            <Payment>
              <MethodCode>E</MethodCode>
            </Payment>
          </DutyTaxFee>
          <DutyTaxFee>
            <SpecificTaxBaseQuantity unitCode=""DTN#E"">5</SpecificTaxBaseQuantity>
          </DutyTaxFee>
          <GoodsMeasure>", xml);
		}

		public void TestMessageBuilderWith_EmptyTaxAssessedAmount_And_EmptyPaymentAmount()
		{
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var entryLine1 = entryHeader.MergedLines[0];
			var feeWithePercentage = entryLine1.Fees.Cast<CusEntryLineFee>().FirstOrDefault(x => x.CF_MethodOfCalculation == MethodOfCalculation.Percentage);
			entryLine1.Fees.RemoveAndDelete(feeWithePercentage);

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLContains(@"</Classification>
          <DutyTaxFee>
            <DutyRegimeCode>100</DutyRegimeCode>
            <SpecificTaxBaseQuantity unitCode=""HLT"">60</SpecificTaxBaseQuantity>
            <QuotaOrderID>123456</QuotaOrderID>
            <Payment>
              <MethodCode>E</MethodCode>
            </Payment>
          </DutyTaxFee>
          <DutyTaxFee>
            <SpecificTaxBaseQuantity unitCode=""DTN#E"">5</SpecificTaxBaseQuantity>
          </DutyTaxFee>
          <GoodsMeasure>", xml);
		}

		public void TestH7Message()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var declaration = entryHeader.Declaration;
			declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.SuperReducedDataSetDeclaration;  // H7
			declaration.JE_MessageSubType = "IM";
			entryHeader.EntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.SuperReducedDataSetDeclaration;
			entryHeader.EntryInstruction.CEI_SubStyle = "Y";
			var messageBuilderManager = new MessageBuilderManager();
			SetDataForTotalPackageQuantityTesting(entryHeader, 55, true);
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLEquals(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH7.xml"), xml);

			SetDataForTotalPackageQuantityTesting(entryHeader, 55, false);
			decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			xml = messageBuilder.Build();
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH7_WhenTotalPackageQuantityIsNonZeroThroughNonInventoryLocation.xml"), xml);

			SetDataForTotalPackageQuantityTesting(entryHeader, 0, true);
			decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			xml = messageBuilder.Build();
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH7_WhenTotalPackageQuantityIsZeroThroughInventoryLocation.xml"), xml);
		}

		public void TestH7MessageCustomsValuationFreightChargeAmount()
		{
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var declaration = entryHeader.Declaration;
			declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.SuperReducedDataSetDeclaration;  // H7
			declaration.JE_MessageSubType = "IM";
			entryHeader.EntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.SuperReducedDataSetDeclaration;
			entryHeader.EntryInstruction.CEI_SubStyle = "Y";
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);

			var groupHeaderCharges = declaration.JobComInvoiceGroupHeaders[0].Charges;
			groupHeaderCharges.RemoveAndDeleteAll();

			var charge1 = groupHeaderCharges.AddNew("AFT", 10m, Core.Constants.CurrencyCodes.EuropeanUnion);
			charge1.J7_FullOrPartialApportionment = "FUA";
			var charge2 = groupHeaderCharges.AddNew("OFT", 10m, Core.Constants.CurrencyCodes.EuropeanUnion);
			charge2.J7_FullOrPartialApportionment = "FUA";
			var charge3 = groupHeaderCharges.AddNew("ONS", 10m, Core.Constants.CurrencyCodes.EuropeanUnion);
			charge3.J7_FullOrPartialApportionment = "FUA";
			var charge4 = groupHeaderCharges.AddNew("CPA", 10m, Core.Constants.CurrencyCodes.EuropeanUnion);
			charge4.J7_FullOrPartialApportionment = "FUA";
			var charge5 = groupHeaderCharges.AddNew("MAC", 10m, Core.Constants.CurrencyCodes.EuropeanUnion);
			charge5.J7_FullOrPartialApportionment = "FUA";
			var charge6 = groupHeaderCharges.AddNew("ONS", 10m, Core.Constants.CurrencyCodes.EuropeanUnion);
			charge6.J7_FullOrPartialApportionment = "PAA";

			var sendingObject = MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject, new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());

			var xml = messageBuilder.Build();
			AssertXMLContains(@"<FreightChargeAmount currencyID=""EUR"">40.00</FreightChargeAmount>", xml);

			var charge7 = groupHeaderCharges.AddNew("ONS", 10m, Core.Constants.CurrencyCodes.UnitedKingdom);
			charge7.J7_FullOrPartialApportionment = "FUA";
			xml = messageBuilder.Build();
			AssertXMLContains(@"<FreightChargeAmount currencyID=""GBP"">", xml);
		}

		public void TestH8Message()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var declaration = entryHeader.Declaration;
			declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.ReducedDataSetDeclaration; // H8
			declaration.JE_MessageSubType = "IM";

			#region Customs Warehouse

			const string CountryCodeUK = Core.Constants.CountryCodes.UnitedKingdom;
			const string CCP = OrgCusCode.CodeTypes.ControlledPremisesID;
			const string customsControlledPremisisCode = "CCP123";

			var warehouse = Factory.New<OrgHeader>();
			warehouse.OH_Code = "WH123";
			warehouse.CompanyData.OB_IMUsedBondedWhs = true;
			var cusCode = warehouse.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(CCP, customsControlledPremisisCode, CountryCodeUK);

			var addrWH = warehouse.Addresses.AddNew();
			addrWH.Address1 = "21 Long Street";
			cusCode.OK_OA_PremisesAddress = addrWH.PK;

			declaration.CusEntryInstruction.CEI_OA_Warehouse = addrWH.PK;

			#endregion

			var invLine = entryHeader.InvoiceLines.ToArray()[0];
			invLine.JI_CEI = declaration.CusEntryInstruction.PK;

			entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;

			entryHeader.EntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.ReducedDataSetDeclaration;
			entryHeader.EntryInstruction.CEI_SubStyle = "Y";

			var orgA = CreateRepresentativeForAgentRepresentationTests("ACME AR1", "ACME", "Copse lane 1", "Hampshire", "Southampton", "England", "SO16");
			orgA.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORIAR1", "GB");
			var addressA = orgA.Addresses[0];
			entryHeader.Declaration.JE_OA_Representative = addressA.PK;
			entryHeader.Declaration.RepresentativeDocAddress.OrganisationPK = orgA.PK;
			SetDataForTotalPackageQuantityTesting(entryHeader, 55, true);

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLEquals(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH8.xml"), xml);

			SetDataForTotalPackageQuantityTesting(entryHeader, 55, false);
			decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			xml = messageBuilder.Build();
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH8_WhenTotalPackageQuantityIsNonZeroThroughNonInventoryLocation.xml"), xml);

			SetDataForTotalPackageQuantityTesting(entryHeader, 0, true);
			decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			xml = messageBuilder.Build();
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH8_WhenTotalPackageQuantityIsZeroThroughInventoryLocation.xml"), xml);
		}

		public void TestC21EEIDRNOPMessage()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			var entryHeader = GetEntryHeader_ForExportsMessageTesting(ExportDeclarationTypeList.Codes.ExportClearanceRequestC21EEIDRNOP);
			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLEquals(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedC21EEIDRNOP.xml"), xml);

			SetDataForTotalPackageQuantityTesting(entryHeader, 55, false);
			xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedCEN_WhenTotalPackageQuantityIsNonZeroThroughNonInventoryLocation.xml"), xml);

			SetDataForTotalPackageQuantityTesting(entryHeader, 0, true);
			xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedCEN_WhenTotalPackageQuantityIsZeroThroughInventoryLocation.xml"), xml);
		}

		public void TestFSMessage()
		{
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var declaration = entryHeader.Declaration;
			declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.FinalSupplementaryDeclaration;
			declaration.JE_MessageSubType = "IM";
			entryHeader.EntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.FinalSupplementaryDeclaration;
			entryHeader.EntryInstruction.CEI_SubStyle = "Q";
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedFS.xml"), xml);
		}

		public void TestFSMessageSendRepresentative()
		{
			var orgA = CreateRepresentativeForAgentRepresentationTests("ACME CORP", "ACME", "21 Long str", "Buckingham", "London", "England", "1234", addEORI: true);
			var addressA = orgA.Addresses[0];

			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var declaration = entryHeader.Declaration;
			declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.FinalSupplementaryDeclaration;
			declaration.JE_MessageSubType = "IM";
			entryHeader.EntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.FinalSupplementaryDeclaration;
			entryHeader.EntryInstruction.CEI_SubStyle = "Q";

			declaration.JE_OA_Representative = addressA.PK;
			declaration.RepresentativeDocAddress.OrganisationPK = orgA.PK;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			AssertEquals("pre-req", RepresentationTypeList.Codes._2Direct, entryHeader.Declaration.JE_DeclarantType);
			AssertEquals("pre-req", "GBEORI12", orgA.GetEuIdentificationNumber());

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();
			AssertXMLContains(@"<Agent>
      <ID>GBEORI12</ID>
      <FunctionCode>2</FunctionCode>
    </Agent>", xml);

			declaration.JE_OA_Representative = ZGuid.Empty;
			decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			xml = messageBuilder.Build();
			CombineAssertions(() =>
			{
				AssertXMLContains(@"<Agent>
      <FunctionCode>2</FunctionCode>
    </Agent>", xml);
				AssertNotContains(@"<ID>GBEORI12</ID>", xml);
			});

			declaration.JE_OA_Representative = addressA.PK;
			declaration.JE_DeclarantType = ZString.Empty;
			decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			xml = messageBuilder.Build();
			CombineAssertions(() =>
			{
				AssertXMLContains(@"<Agent>
      <ID>GBEORI12</ID>
    </Agent>", xml);
				AssertNotContains(@"<FunctionCode>2</FunctionCode>", xml);
			});

			declaration.JE_OA_Representative = ZGuid.Empty;
			declaration.JE_DeclarantType = ZString.Empty;
			decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			xml = messageBuilder.Build();
			AssertNotContains(@"<Agent>", xml);
		}

		public void TestH1MessageWithOverrideFees()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var entryLine1 = entryHeader.MergedLines[0];
			entryLine1.Fees.RemoveAndDeleteAll();

			var fee1 = entryLine1.Fees.AddNew();
			fee1.CF_ChargeType = "447";
			fee1.CF_MethodOfCalculation = "ASVX";
			fee1.CF_BaseValue = 351.5476;
			fee1.CF_ChargeAmount = 2000.5476;
			fee1.CF_Rate = 0;
			fee1.CF_RateOverrideReasonCode = "OVR";
			fee1.CF_MethodOfPayment = "E";

			var fee2 = entryLine1.Fees.AddNew();
			fee2.CF_ChargeType = "A00";
			fee2.CF_MethodOfCalculation = "%";
			fee2.CF_BaseValue = 30000.5476;
			fee2.CF_ChargeAmount = 3000.5476;
			fee2.CF_Rate = 10;
			fee2.CF_RateOverrideReasonCode = "OVR";
			fee2.CF_MethodOfPayment = "R";

			var fee3 = entryLine1.Fees.AddNew();
			fee3.CF_ChargeType = "";
			fee3.CF_MethodOfCalculation = "";
			fee3.CF_BaseValue = 0;
			fee3.CF_ChargeAmount = 0;
			fee3.CF_Rate = 0;
			fee3.CF_RateOverrideReasonCode = "OVR";
			fee3.CF_MethodOfPayment = "E";

			var fee4 = entryLine1.Fees.AddNew();
			fee4.CF_ChargeType = "B00";
			fee4.CF_MethodOfCalculation = "";
			fee4.CF_BaseValue = 40000;
			fee4.CF_ChargeAmount = 4000;
			fee4.CF_Rate = 0;
			fee4.CF_RateOverrideReasonCode = "ADD";
			fee4.CF_MethodOfPayment = "E";

			var fee5 = entryLine1.Fees.AddNew();
			fee5.CF_ChargeType = "A02";
			fee5.CF_MethodOfCalculation = "%";
			fee5.CF_BaseValue = 50000.123456;
			fee5.CF_ChargeAmount = 5000.12;
			fee5.CF_Rate = 10;
			fee5.CF_RateOverrideReasonCode = "OVR";
			fee5.CF_MethodOfPayment = "R";

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLContains(@"</Classification>
          <DutyTaxFee>
            <DutyRegimeCode>100</DutyRegimeCode>
            <SpecificTaxBaseQuantity unitCode=""ASV#X"">351.5476</SpecificTaxBaseQuantity>
            <TypeCode>447</TypeCode>
            <Payment>
              <MethodCode>E</MethodCode>
              <TaxAssessedAmount currencyID=""GBP"">14001.20</TaxAssessedAmount>
              <PaymentAmount currencyID=""GBP"">2000.54</PaymentAmount>
            </Payment>
          </DutyTaxFee>
          <DutyTaxFee>
            <SpecificTaxBaseQuantity unitCode=""GBP"">30000.5476</SpecificTaxBaseQuantity>
            <TypeCode>A00</TypeCode>
            <Payment>
              <MethodCode>R</MethodCode>
              <TaxAssessedAmount currencyID=""GBP"">0.00</TaxAssessedAmount>
              <PaymentAmount currencyID=""GBP"">3000.54</PaymentAmount>
            </Payment>
          </DutyTaxFee>
          <DutyTaxFee>
            <Payment>
              <MethodCode>E</MethodCode>
              <TaxAssessedAmount currencyID=""GBP"">0.00</TaxAssessedAmount>
              <PaymentAmount currencyID=""GBP"">0.00</PaymentAmount>
            </Payment>
          </DutyTaxFee>
          <DutyTaxFee>
            <SpecificTaxBaseQuantity unitCode=""GBP"">40000</SpecificTaxBaseQuantity>
            <TypeCode>B00</TypeCode>
            <Payment>
              <MethodCode>E</MethodCode>
            </Payment>
          </DutyTaxFee>
          <DutyTaxFee>
            <SpecificTaxBaseQuantity unitCode=""GBP"">50000.123456</SpecificTaxBaseQuantity>
            <TypeCode>A02</TypeCode>
            <Payment>
              <MethodCode>R</MethodCode>
              <TaxAssessedAmount currencyID=""GBP"">0.00</TaxAssessedAmount>
              <PaymentAmount currencyID=""GBP"">5000.12</PaymentAmount>
            </Payment>
          </DutyTaxFee>
          <GoodsMeasure>", xml);
		}

		ZString ArrangeAndActForCDSSuppressSendingASVXFor3XXTaxTypesTests()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var entryLine1 = entryHeader.MergedLines[0];
			entryLine1.Fees.RemoveAndDeleteAll();

			var fee1 = entryLine1.Fees.AddNew();
			fee1.CF_ChargeType = "321";
			fee1.CF_MethodOfCalculation = "ASVX";
			fee1.CF_BaseValue = 50.00m;
			var fee2 = entryLine1.Fees.AddNew();
			fee2.CF_ChargeType = "321";
			fee2.CF_MethodOfCalculation = "ASV";
			fee2.CF_BaseValue = 5.0m;

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			return xml;
		}

		public void TestSuppressSendingASVXFor3XXTaxTypes_True()
		{
			using (GBCustomsDataRegistry.Instance.CDSSuppressSendingASVXFor3XXTaxTypes.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, true))
			{
				var xml = ArrangeAndActForCDSSuppressSendingASVXFor3XXTaxTypesTests();
				AssertNotContains("<SpecificTaxBaseQuantity unitCode=\"ASV#X\">50.00</SpecificTaxBaseQuantity>", xml);
				AssertContains("<SpecificTaxBaseQuantity unitCode=\"ASV\">5.0</SpecificTaxBaseQuantity>", xml);
			}
		}

		public void TestSuppressSendingASVXFor3XXTaxTypes_False()
		{
			using (GBCustomsDataRegistry.Instance.CDSSuppressSendingASVXFor3XXTaxTypes.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, false))
			{
				var xml = ArrangeAndActForCDSSuppressSendingASVXFor3XXTaxTypesTests();
				AssertContains("<SpecificTaxBaseQuantity unitCode=\"ASV#X\">50.00</SpecificTaxBaseQuantity>", xml);
				AssertContains("<SpecificTaxBaseQuantity unitCode=\"ASV\">5.0</SpecificTaxBaseQuantity>", xml);
			}
		}

		public void TestH1Message()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLEquals(GetExpectedH1(), xml);
		}

		public void TestH1Message_WhenTotalPackageQuantityIsZero()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			SetDataForTotalPackageQuantityTesting(entryHeader, 0, true);
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH1_WhenTotalPackageQuantityIsZero.xml"), xml);
		}

		public void TestH1Message_SendOnlySingleEntryReference()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SendOnlyASingleEntryReference
				});
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLEquals(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH1_Automation_SendOnlyASingleEntryReference.xml"), xml);
		}

		public void TestH1Message_NotForImport()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.NotForImports
				});
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLEquals(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH1_Automation_NotForImports.xml"), xml);
		}

		public void TestH1MessageWithInvoicesWithDifferentCurrencyForItemChargeAmount()
		{
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var declaration = entryHeader.Declaration;
			var invoice2 = declaration.Invoices[1];
			invoice2.JZ_RX_NKInvoice_Currency = "USD";

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLContains(@"<ItemChargeAmount currencyID=""GBP"">359.71</ItemChargeAmount>", xml);
		}

		public void TestH1MessageWithInvoicesWithSameCurrencyOtherThanLocalCurrencyForItemChargeAmount()
		{
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var declaration = entryHeader.Declaration;
			var invoice1 = declaration.Invoices[0];
			invoice1.JZ_RX_NKInvoice_Currency = "USD";
			var invoice2 = declaration.Invoices[1];
			invoice2.JZ_RX_NKInvoice_Currency = "USD";

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLContains(@"<ItemChargeAmount currencyID=""USD"">100.00</ItemChargeAmount>", xml);
			AssertXMLContains(@"<ItemChargeAmount currencyID=""USD"">500.00</ItemChargeAmount>", xml);
		}

		public void TestH1MessageWithInvoicesWithDifferentCurrencyForStatisticalValueAmount()
		{
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var declaration = entryHeader.Declaration;
			var invoice1 = declaration.Invoices[0];
			invoice1.JZ_RX_NKInvoice_Currency = "USD";

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLContains(@"<StatisticalValueAmount currencyID=""GBP"">158.54</StatisticalValueAmount>", xml);
		}

		public void TestH1WithPercentageFees()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			foreach (var line in entryHeader.MergedLines)
			{
				line.Fees.RemoveAndDeleteAll();
			}
			foreach (var invoice in entryHeader.InvoiceHeaders)
			{
				invoice.Charges.RemoveAndDeleteAll();
				invoice.GroupCharges.RemoveAndDeleteAll();
			}
			var declaration = entryHeader.Declaration;

			declaration.TopGroupInvoice.Charges.RemoveAndDeleteAll();
			var charge1 = declaration.TopGroupInvoice.Charges.AddNew();
			var charge2 = declaration.TopGroupInvoice.Charges.AddNew();
			var charge3 = declaration.TopGroupInvoice.Charges.AddNew();

			InvChargeTestHelper.SetUpCharge(charge1, UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge, true, 1m, string.Empty, false, 0m);
			InvChargeTestHelper.SetUpCharge(charge2, UCCCustomsChargeTypeList.Codes.EngineeringDevelopmentArtworkCharge, true, 2m, string.Empty, false, 0m);
			InvChargeTestHelper.SetUpCharge(charge3, UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge, true, 3m, string.Empty, false, 0m);
			declaration.ResumeApportionment();

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLContains(@"<ChargeDeduction>
            <ChargesTypeCode>AB</ChargesTypeCode>
            <OtherChargeDeductionAmount currencyID=""GBP"">5.00</OtherChargeDeductionAmount>
          </ChargeDeduction>", xml);
			AssertXMLContains(@"<ChargeDeduction>
            <ChargesTypeCode>AH</ChargesTypeCode>
            <OtherChargeDeductionAmount currencyID=""GBP"">10.00</OtherChargeDeductionAmount>
          </ChargeDeduction>", xml);
			AssertXMLContains(@"<ChargeDeduction>
            <ChargesTypeCode>AI</ChargesTypeCode>
            <OtherChargeDeductionAmount currencyID=""GBP"">15.00</OtherChargeDeductionAmount>
          </ChargeDeduction>", xml);

			AssertNotContains("Message should not contain AC fee type", "<ChargesTypeCode>AC</ChargesTypeCode>", xml);
			AssertNotContains("Message should not contain AZ fee type", "<ChargesTypeCode>AZ</ChargesTypeCode>", xml);
			AssertNotContains("Message should not contain AM fee type", "<ChargesTypeCode>AM</ChargesTypeCode>", xml);
		}

		public void TestAgentRepresentation_Scenario_1st()
		{
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			entryHeader.Declaration.JE_OA_Representative = ZGuid.Empty;
			var expectedXMLPath = @"Messaging.ExpectedAgentRep1.xml";
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLContains(GetExpectedXMLForAgentRep(expectedXMLPath), xml);
		}

		public void TestH1MessageWithInvoicesWithDifferentSellers()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var declaration = entryHeader.Declaration;
			var invoice1 = declaration.Invoices[0];
			var invoice2 = declaration.Invoices[1];
			var helper = new DeclarationTestHelper(Factory);
			var org1 = helper.MakeOrganisation1();
			var org2 = helper.MakeOrganisation2();
			org2.CustomsCodes.RemoveAndDeleteAll();
			invoice1.JZ_OA_SellerAddress = org1.Addresses[0].PK;
			invoice2.JZ_OA_SellerAddress = org2.Addresses[0].PK;

			var expectedXMLPath = @"Messaging.ExpectedH1_WithInvoicesWithDifferentSellers.xml";
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLEquals(GetExpectedXMLForAgentRep(expectedXMLPath), xml);
		}
		public void TestH1MessageWithInvoicesWithDifferentBuyers()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var declaration = entryHeader.Declaration;
			var invoice1 = declaration.Invoices[0];
			var invoice2 = declaration.Invoices[1];
			var helper = new DeclarationTestHelper(Factory);
			var org1 = helper.MakeOrganisation1();
			var org2 = helper.MakeOrganisation2();
			org2.CustomsCodes.RemoveAndDeleteAll();
			invoice1.JZ_OH_Buyer = org1.PK;
			invoice1.JZ_OA_BuyerAddress = org1.Addresses[0].PK;
			invoice2.JZ_OH_Buyer = org2.PK;
			invoice2.JZ_OA_BuyerAddress = org2.Addresses[0].PK;

			var expectedXMLPath = @"Messaging.ExpectedH1_WithInvoicesWithDifferentBuyers.xml";
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLEquals(GetExpectedXMLForAgentRep(expectedXMLPath), xml);
		}

		public void TestH1MessageMultipleExporters()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var declaration = entryHeader.Declaration;
			declaration.JE_RL_NKOrigin = "USATL";
			declaration.JE_GoodsOrigin = "US";
			var invoice1 = declaration.Invoices[0];
			var invoice2 = declaration.Invoices[1];
			var helper = new DeclarationTestHelper(Factory);
			var deSupplier = helper.GetNewOrganisation("", "DDE", "German", "Main St", "", "Munich", "NA", "1234", "DEMUN", "12345", "654987", "");
			var krSupplier = helper.GetNewOrganisation("", "DKR", "Korea", "Main St", "", "Seoul", "NA", "1234", "KRSEO", "12345", "654987", "");
			invoice1.JZ_OA_SellerAddress = deSupplier.Addresses[0].PK;
			invoice1.JZ_OA_SupplierAddress = deSupplier.Addresses[0].PK;
			invoice2.JZ_OA_SellerAddress = krSupplier.Addresses[0].PK;
			invoice2.JZ_OA_SupplierAddress = krSupplier.Addresses[0].PK;

			var expectedXMLPath = "Messaging.ExpectedH1_WithMultipleExporters.xml";
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();
			AssertXMLEquals(GetExpectedXMLForAgentRep(expectedXMLPath), xml);
		}

		public void TestH1MessageSingleExporter()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var declaration = entryHeader.Declaration;
			declaration.JE_RL_NKOrigin = "USATL";
			declaration.JE_GoodsOrigin = "US";
			var invoice1 = declaration.Invoices[0];
			var invoice2 = declaration.Invoices[1];
			var helper = new DeclarationTestHelper(Factory);
			var deSupplier = helper.GetNewOrganisation("", "DDE", "German", "Main St", "", "Munich", "NA", "1234", "DEMUN", "12345", "654987", "");
			invoice1.JZ_OA_SellerAddress = deSupplier.Addresses[0].PK;
			invoice1.JZ_OA_SupplierAddress = deSupplier.Addresses[0].PK;
			invoice2.JZ_OA_SellerAddress = deSupplier.Addresses[0].PK;
			invoice2.JZ_OA_SupplierAddress = deSupplier.Addresses[0].PK;

			var expectedXMLPath = "Messaging.ExpectedH1_WithSingleExporter.xml";
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();
			AssertXMLEquals(GetExpectedXMLForAgentRep(expectedXMLPath), xml);
		}

		public void TestAgentRepresentation_Scenario_2nd()
		{
			var orgA = CreateRepresentativeForAgentRepresentationTests("MY CORP PTY", "MYCORP", "21 Long str", "Buckingham", "London", "England", "1234");
			var addressA = orgA.Addresses[0];
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			entryHeader.Declaration.JE_OA_Representative = addressA.PK;
			entryHeader.Declaration.RepresentativeDocAddress.OrganisationPK = orgA.PK;
			var expectedXMLPath = @"Messaging.ExpectedAgentRep2.xml";
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLContains(GetExpectedXMLForAgentRep(expectedXMLPath), xml);
		}

		public void TestAgentRepresentation_Scenario_3rd()
		{
			var rep = CreateRepresentativeForAgentRepresentationTests("MY CORP PTY", "MYCORP", "21 Long str", "Buckingham", "London", "England", "1234");
			var addressA = rep.Addresses[0];
			var dec = CreateDeclarantForAgentRepresentationTests("ACME CORP", "ACME", "44 Short str", "WestEnd", "Heathrow", "Scotland", "9876");
			var addressB = dec.Addresses[0];
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			entryHeader.Declaration.JE_OA_Representative = addressA.PK;
			entryHeader.Declaration.RepresentativeDocAddress.OrganisationPK = rep.PK;
			entryHeader.Declaration.JE_OA_DeclarantAddress = addressB.PK;
			entryHeader.Declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			var expectedXMLPath = @"Messaging.ExpectedAgentRep3.xml";
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLContains(GetExpectedXMLForAgentRep(expectedXMLPath), xml);
		}

		public void TestAgentRepresentation_Scenario_4th()   //NoEORI
		{
			var orgA = CreateRepresentativeForAgentRepresentationTests("MY CORP PTY", "MYCORP", "21 Long str", "Buckingham", "London", "England", "1234");
			var addressA = orgA.Addresses[0];
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			entryHeader.Declaration.JE_OA_Representative = addressA.PK;
			entryHeader.Declaration.RepresentativeDocAddress.OrganisationPK = orgA.PK;
			var expectedXMLPath = @"Messaging.ExpectedAgentRep4.xml";
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLContains(GetExpectedXMLForAgentRep(expectedXMLPath), xml);
		}

		public void TestAgentRepresentation_Scenario_5th()  //WithEORI
		{
			var orgA = CreateRepresentativeForAgentRepresentationTests("MY CORP PTY", "MYCORP", "21 Long str", "Buckingham", "London", "England", "1234", addEORI: true);
			var addressA = orgA.Addresses[0];
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			entryHeader.Declaration.JE_OA_Representative = addressA.PK;
			entryHeader.Declaration.RepresentativeDocAddress.OrganisationPK = orgA.PK;
			entryHeader.Declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			var expectedXMLPath = @"Messaging.ExpectedAgentRep5.xml";

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLContains(GetExpectedXMLForAgentRep(expectedXMLPath), xml);
		}

		OrgHeader CreateDeclarantForAgentRepresentationTests(ZString fullName, ZString code, ZString address1, ZString address2, ZString city, ZString state, ZString postCode, bool addEORI = false)
		{
			var orgA = Factory.New<OrgHeader>();
			orgA.OH_FullName = fullName;
			orgA.OH_Code = code;
			var addressA = orgA.MainAddress;
			addressA.OA_Address1 = address1;
			addressA.OA_Address2 = address2;
			addressA.OA_City = city;
			addressA.OA_State = state;
			addressA.OA_PostCode = postCode;

			if (addEORI)
			{
				const string EORI = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				orgA.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(EORI, "EORI35", "GB");
			}

			return orgA;
		}

		OrgHeader CreateRepresentativeForAgentRepresentationTests(ZString fullName, ZString code, ZString address1, ZString address2, ZString city, ZString state, ZString postCode, bool addEORI = false)
		{
			var orgA = Factory.New<OrgHeader>();
			orgA.OH_FullName = fullName;
			orgA.OH_Code = code;
			var addressA = orgA.MainAddress;
			addressA.OA_Address1 = address1;
			addressA.OA_Address2 = address2;
			addressA.OA_City = city;
			addressA.OA_State = state;
			addressA.OA_PostCode = postCode;

			if (addEORI)
			{
				const string EORI = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				orgA.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(EORI, "EORI12", "GB");
			}

			return orgA;
		}

		public void TestMessageDeclarant()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());

			var xml = messageBuilder.Build();
			AssertXMLEquals(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH1Declarant.xml"), xml);
		}

		public void TestIdentificationTypeCode_ForRORO()
		{
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			entryHeader.Declaration.ZG_Box18TransportID = "TxId03";
			entryHeader.Declaration.JE_TransportMode = GBTransportTypeList.Codes.ROR;
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLContains(@"<BorderTransportMeans>
      <IdentificationTypeCode>30</IdentificationTypeCode>
      <RegistrationNationalityCode>AU</RegistrationNationalityCode>
      <ModeCode>6</ModeCode>
    </BorderTransportMeans>", xml);

			AssertXMLContains(@"<ArrivalTransportMeans>
          <ID>TxId03</ID>
          <IdentificationTypeCode>30</IdentificationTypeCode>
        </ArrivalTransportMeans>", xml);
		}

		public void TestH1MessageWithPreviousDocumentInSameOrder()
		{
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var prevDoc1 = entryHeader.Declaration.PreviousDocuments.AddNew();
			prevDoc1.CSI_Code = "A";
			prevDoc1.CSI_SubType = "1";
			prevDoc1.CSI_ReferenceNumber = "A1";
			var prevDoc2 = entryHeader.Declaration.PreviousDocuments.AddNew();
			prevDoc2.CSI_Code = "B";
			prevDoc2.CSI_SubType = "2";
			prevDoc2.CSI_ReferenceNumber = "B2";
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLContains(@"<PreviousDocument>
        <CategoryCode>1</CategoryCode>
        <ID>A1</ID>
        <TypeCode>A</TypeCode>
      </PreviousDocument>
      <PreviousDocument>
        <CategoryCode>2</CategoryCode>
        <ID>B2</ID>
        <TypeCode>B</TypeCode>
      </PreviousDocument>", xml);

			entryHeader.Declaration.PreviousDocuments.RemoveAndDeleteAll();
			prevDoc1 = entryHeader.Declaration.PreviousDocuments.AddNew();
			prevDoc1.CSI_Code = "B";
			prevDoc1.CSI_SubType = "2";
			prevDoc1.CSI_ReferenceNumber = "B2";
			prevDoc2 = entryHeader.Declaration.PreviousDocuments.AddNew();
			prevDoc2.CSI_Code = "A";
			prevDoc2.CSI_SubType = "1";
			prevDoc2.CSI_ReferenceNumber = "A1";

			xml = messageBuilder.Build();

			AssertXMLContains(@"<PreviousDocument>
        <CategoryCode>1</CategoryCode>
        <ID>A1</ID>
        <TypeCode>A</TypeCode>
      </PreviousDocument>
      <PreviousDocument>
        <CategoryCode>2</CategoryCode>
        <ID>B2</ID>
        <TypeCode>B</TypeCode>
      </PreviousDocument>", xml);
		}

		public void TestH1MessageWithWritingOff()
		{
			string countryCodeUK = Core.Constants.CountryCodes.UnitedKingdom;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCodeUK))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var eun = helper.CreateNewOrGetExistingDataGrouping(countryCodeUK, "United Kingdom");

				var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
				var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
				var attributeNameValuePairs = new Dictionary<string, string[]>();
				attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
				helper.CreateCusCodeListsForMultipleTypesWithAttributes(countryCodeUK,
					new string[] { importCodeType, exportCodeType }, "C601", "C601 - Description", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

				Factory.Save();

				var entryHeader = CreateSampleEntryHeaderForH1(Factory);

				var suppDoc = entryHeader.MergedLines[1].InvoiceLines.Cast<JobComInvoiceLine>().ToList()[0].SupportingDocuments[0];
				suppDoc.CSI_ReferenceNumber2 = "Issuing Authority XYZ";
				suppDoc.CSI_DateOfIssue = new ZDateTime(2019, 01, 18, 12, 17, 23);
				suppDoc.CSI_Quantity = 1488;
				suppDoc.CSI_UnitOfQuantity = "KGM";
				suppDoc.CSI_UnitOfQuantity2 = "G";

				var messageBuilderManager = new MessageBuilderManager();
				var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
				var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
				var xml = messageBuilder.Build();

				AssertXMLContains(GetExpectedH1WithWritingOff(), xml);

				suppDoc.CSI_Quantity = 0;
				suppDoc.CSI_UnitOfQuantity = "GBP";
				suppDoc.CSI_UnitOfQuantity2 = "";
				messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
				xml = messageBuilder.Build();

				AssertXMLContains(GetExpectedH1WithWritingOffZero(), xml);
			}
		}

		public void TestH5MessageWithWritingOff()
		{
			string countryCodeUK = Core.Constants.CountryCodes.UnitedKingdom;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCodeUK))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var eun = helper.CreateNewOrGetExistingDataGrouping(countryCodeUK, "United Kingdom");

				var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
				var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
				var attributeNameValuePairs = new Dictionary<string, string[]>();
				attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
				helper.CreateCusCodeListsForMultipleTypesWithAttributes(countryCodeUK,
					new string[] { importCodeType, exportCodeType }, "C601", "C601 - Description", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

				Factory.Save();

				var entryHeader = CreateSampleEntryHeaderForH1(Factory);
				var declaration = entryHeader.Declaration;
				declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.DeclarationForGoodsFromTheSpecialFiscalTerritories;  // H5

				var suppDoc = entryHeader.MergedLines[1].InvoiceLines.Cast<JobComInvoiceLine>().ToList()[0].SupportingDocuments[0];
				suppDoc.CSI_DateOfIssue = new ZDateTime(2019, 01, 18, 12, 17, 23);
				suppDoc.CSI_Quantity = 1488;
				suppDoc.CSI_UnitOfQuantity = "KGM";
				suppDoc.CSI_UnitOfQuantity2 = "G";

				var messageBuilderManager = new MessageBuilderManager();
				var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
				var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
				var xml = messageBuilder.Build();

				AssertXMLContains(GetExpectedH5WithWritingOff(), xml);

				suppDoc.CSI_Quantity = 0;
				suppDoc.CSI_UnitOfQuantity = "GBP";
				suppDoc.CSI_UnitOfQuantity2 = "";
				messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
				xml = messageBuilder.Build();

				AssertXMLContains(GetExpectedH5WithWritingOffZero(), xml);
			}
		}

		public void TestH4MessageWithWritingOff()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			string countryCodeUK = Core.Constants.CountryCodes.UnitedKingdom;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCodeUK))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var eun = helper.CreateNewOrGetExistingDataGrouping(countryCodeUK, "United Kingdom");

				var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
				var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
				var attributeNameValuePairs = new Dictionary<string, string[]>();
				attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
				helper.CreateCusCodeListsForMultipleTypesWithAttributes(countryCodeUK,
					new string[] { importCodeType, exportCodeType }, "C601", "C601 - Description", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

				Factory.Save();

				var entryHeader = CreateSampleEntryHeaderForH1(Factory);
				var declaration = entryHeader.Declaration;
				declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.DeclarationForInwardProcessing;  // H4

				var suppDoc = entryHeader.MergedLines[1].InvoiceLines.Cast<JobComInvoiceLine>().ToList()[0].SupportingDocuments[0];
				suppDoc.CSI_DateOfIssue = new ZDateTime(2019, 01, 18, 12, 17, 23);
				suppDoc.CSI_Quantity = 1488;
				suppDoc.CSI_UnitOfQuantity = "KGM";
				suppDoc.CSI_UnitOfQuantity2 = "G";

				var messageBuilderManager = new MessageBuilderManager();
				var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
				var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.H4ForbidWritingOff, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, true))
				{
					var xml = messageBuilder.Build();
					AssertNotContains(GetExpectedH4WithWritingOff(), xml);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.H4ForbidWritingOff, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, false))
				{
					var xml = messageBuilder.Build();
					AssertXMLContains(GetExpectedH4WithWritingOff(), xml);
				}

				suppDoc.CSI_Quantity = 0;
				suppDoc.CSI_UnitOfQuantity = "GBP";
				suppDoc.CSI_UnitOfQuantity2 = "";
				messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.H4ForbidWritingOff, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, true))
				{
					var xml = messageBuilder.Build();
					AssertNotContains(GetExpectedH4WithWritingOffZero(), xml);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.H4ForbidWritingOff, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, false))
				{
					var xml = messageBuilder.Build();
					AssertXMLContains(GetExpectedH4WithWritingOffZero(), xml);
				}
			}
		}

		public void TestH1MessageWithDestinationCountryForItems()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			var entryHeader = CreateSampleEntryHeaderForH1(Factory);

			entryHeader.Declaration.InvoiceLines[1].ZG_CountryOfDestination = "FR";

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLEquals(GetExpectedH1WithItemDestinations(), xml);
		}
		public void TestMessageWithTwoEntriesH1AndH2()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			var entryHeader = CreateSampleEntryHeaderForH1(Factory);

			var declaration = entryHeader.Declaration;
			declaration.Invoices[0].JZ_Weight = 60;
			declaration.Invoices[1].JZ_Weight = 40;

			var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
			var invoiceLine = declaration.Invoices[0].InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "4012123";
			invoiceLine.JI_FormattedTariff = "4202.12.1900";
			invoiceLine.JI_ZZF_NKTaxType = "673";
			entryHeader2.CH_BGMReference = "8GB123456789000-S0001000/1";
			var cei = declaration.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = ImportDeclarationTypeList.Codes.DeclarationForCustomsWarehousing;
			entryHeader.EntryInstruction.CEI_SubStyle = "Y";
			invoiceLine.JI_CEI = cei.PK;
			var entryLine = entryHeader2.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var ucrnumber = Factory.New<CusEntryNumber>();
			ucrnumber.CE_ParentID = entryHeader2.PK;
			ucrnumber.CE_EntryType = CusEntryNumberTypes.Standard.UniqueConsignementReference;
			ucrnumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			ucrnumber.CE_EntryNum = "DUCR/2";
			entryHeader2.CH_CEI_Instruction = cei.PK;
			declaration.JE_GoodsLocation = "1234567";

			AssertEquals(2, entryHeader.Containers.Length);
			AssertEquals(0, entryHeader2.Containers.Length);

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLEquals(GetExpectedH1(), xml);

			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader2), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			xml = messageBuilder.Build();

			AssertXMLContains("<ContainerCode>0</ContainerCode>", xml);
			AssertNotContains("<ID>CNT123</ID>", xml);
		}

		public void TestMessageWithInvalidEntries()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var declaration = entryHeader.Declaration;
			entryHeader.EntryInstruction.CEI_Style = null;
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var errorCollector = new ErrorCollector();
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), errorCollector, new Customs.Business.CusdecMessageFunction.New());
			AssertContains("Entry Instruction is missing a Declaration Type. Please select from dropdown menu.", errorCollector.GetErrorsAsString());

			errorCollector.WipeErrors();
			entryHeader.EntryInstruction.CEI_Style = "XX";
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), errorCollector, new Customs.Business.CusdecMessageFunction.New());
			AssertContains("CW1 does not support building message type XX", errorCollector.GetErrorsAsString());

			errorCollector.WipeErrors();
			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), errorCollector, new Customs.Business.CusdecMessageFunction.New());
			AssertContains("Cannot find any Entry Instructions; this may be because some Commercial Invoice Lines do not cite an Entry Instruction.", errorCollector.GetErrorsAsString());

			errorCollector.WipeErrors();
			declaration.CustomsEntryInstructions.AddNew().CEI_Style = ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse;
			declaration.Invoices.RemoveAll();
			var inv = declaration.Invoices.AddNew();
			var invLine = inv.JobComInvoiceLines.AddNew();
			entryHeader.CH_CEI_Instruction = invLine.EntryInstruction.PK;
			decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), errorCollector, new Customs.Business.CusdecMessageFunction.New());
			AssertEquals(0, errorCollector.ErrorCount);

			errorCollector.WipeErrors();
			invLine.JI_CEI = ZGuid.Empty;
			entryHeader.CH_CEI_Instruction = ZGuid.Empty;
			decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), errorCollector, new Customs.Business.CusdecMessageFunction.New());
			AssertContains("Cannot find any Entry Instructions; this may be because some Commercial Invoice Lines do not cite an Entry Instruction.", errorCollector.GetErrorsAsString());
		}

		public void TestH1MessageWithoutSupervisingOffice()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			// Set supervising office to be null
			entryHeader.Declaration.SupervisingOfficeDocAddress.E2_OA_Address = ZGuid.Empty;
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLEquals(GetExpectedH1WithoutSupervisingOffice(), xml);
		}

		public void TestH1Message_ItemConsignor()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			entryHeader.Declaration.Invoices[0].JZ_OH_Supplier = ZGuid.Empty;
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH1 - Item Consignor.xml"), xml);
		}

		public void TestH1MessageWithMergedGrossWeight()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			PrepareUniversalData_ToTestMergeOperation();
			var dec = PrepareMergeScenario_ToTestGrossWeight();

			var shutterUpperer = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			bool mergeResult = dec.DoMerge(shutterUpperer);
			Assert("Check Merge Result", mergeResult);
			AssertEquals("pre-req: one entry", 1, dec.ActiveEntryHeaders.Count);
			AssertEquals("pre-req: one merged entry line", 1, dec.ActiveEntryHeaders[0].MergedLines.Count);

			var entryHeader = dec.ActiveEntryHeaders[0];
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(dec);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();
			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH1GrossMassMeasure.xml"), xml);
		}
		JobDeclaration PrepareMergeScenario_ToTestGrossWeight()
		{
			const string CDS = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			const string IMP = MessageTypeList.Codes.Import;

			var dec = CreateJobDeclaration_ToTestMergeOperation(IMP, CDS, 2);

			var invoice = dec.Invoices[0];
			var invLine1 = invoice.InvoiceLines[0];
			invLine1.JI_Procedure = "111";
			invLine1.JI_Description = "XXX";
			invLine1.JI_Weight = 999999.999;
			invLine1.JI_WeightUQ = "KT";
			var invLine2 = invoice.InvoiceLines[1];
			invLine2.JI_Procedure = "111";
			invLine2.JI_Description = "XXX";
			invLine2.JI_Weight = 123456;
			invLine2.JI_WeightUQ = "MG";

			return dec;
		}

		public void TestH1MessageWithAdditionalInfo_Scenario_01()
		{
			PrepareUniversalData_ToTestMergeOperation();
			var dec = PrepareMergeScenario_01_ToTestMergeOperation();

			var shutterUpperer = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			bool mergeResult = dec.DoMerge(shutterUpperer);
			Assert("Check Merge Result", mergeResult);

			var entryHeader = dec.ActiveEntryHeaders[0];
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(dec);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLContains(GetExpectedH1WithAdditionalInfo_Scenario_01_Entry_01_Line_01(), xml);
			AssertXMLContains(GetExpectedH1WithAdditionalInfo_Scenario_01_Entry_01_Line_02(), xml);
		}

		public void TestH1MessageWithAdditionalInfo_Scenario_02()
		{
			PrepareUniversalData_ToTestMergeOperation();
			var dec = PrepareMergeScenario_02_ToTestMergeOperation();

			var shutterUpperer = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			bool mergeResult = dec.DoMerge(shutterUpperer);
			Assert("Check Merge Result", mergeResult);

			var entryHeader = FindEntryHeader_ToTestMergeOperation(dec.ActiveEntryHeaders, "-11-12");
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(dec);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLContains(GetExpectedH1WithAdditionalInfo_Scenario_02_Entry_01_Line_01(), xml);
			AssertXMLContains(GetExpectedH1WithAdditionalInfo_Scenario_02_Entry_01_Line_02(), xml);

			entryHeader = FindEntryHeader_ToTestMergeOperation(dec.ActiveEntryHeaders, "-24-25-26");
			messageBuilderManager = new MessageBuilderManager();
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			xml = messageBuilder.Build();

			AssertXMLContains(GetExpectedH1WithAdditionalInfo_Scenario_02_Entry_02_Line_01(), xml);
			AssertXMLContains(GetExpectedH1WithAdditionalInfo_Scenario_02_Entry_02_Line_02(), xml);
			AssertXMLContains(GetExpectedH1WithAdditionalInfo_Scenario_02_Entry_02_Line_03(), xml);
		}

		public void TestH1MessageWithAdditionalInfo_Scenario_03()
		{
			PrepareUniversalData_ToTestMergeOperation();
			var dec = PrepareMergeScenario_03_ToTestMergeOperation();

			var shutterUpperer = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			bool mergeResult = dec.DoMerge(shutterUpperer);
			Assert("Check Merge Result", mergeResult);

			var entryHeader = FindEntryHeader_ToTestMergeOperation(dec.ActiveEntryHeaders, "-11");
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(dec);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLContains(GetExpectedH1WithAdditionalInfo_Scenario_03_Entry_01_Line_01(), xml);

			entryHeader = FindEntryHeader_ToTestMergeOperation(dec.ActiveEntryHeaders, "-24-25");
			messageBuilderManager = new MessageBuilderManager();
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			xml = messageBuilder.Build();

			AssertXMLContains(GetExpectedH1WithAdditionalInfo_Scenario_03_Entry_02_Line_01(), xml);
			AssertXMLContains(GetExpectedH1WithAdditionalInfo_Scenario_03_Entry_02_Line_02(), xml);

			entryHeader = FindEntryHeader_ToTestMergeOperation(dec.ActiveEntryHeaders, "-37-38-39");
			messageBuilderManager = new MessageBuilderManager();
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			xml = messageBuilder.Build();

			AssertXMLContains(GetExpectedH1WithAdditionalInfo_Scenario_03_Entry_03_Line_01(), xml);
			AssertXMLContains(GetExpectedH1WithAdditionalInfo_Scenario_03_Entry_03_Line_02(), xml);
			AssertXMLContains(GetExpectedH1WithAdditionalInfo_Scenario_03_Entry_03_Line_03(), xml);
		}

		public void TestH1MessageWithAdditionalInfo_Scenario_04()
		{
			PrepareUniversalData_ToTestMergeOperation();
			var dec = PrepareMergeScenario_04_ToTestMergeOperation();

			var shutterUpperer = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			bool mergeResult = dec.DoMerge(shutterUpperer);
			Assert("Check Merge Result", mergeResult);

			var entryHeader = dec.ActiveEntryHeaders[0];
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(dec);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLContains(GetExpectedH1WithAdditionalInfo_Scenario_04_Entry_01_Line_01(), xml);
			AssertXMLContains(GetExpectedH1WithAdditionalInfo_Scenario_04_Entry_01_Line_02(), xml);
		}

		CusEntryHeader FindEntryHeader_ToTestMergeOperation(Customs.Business.ActiveCusEntryHeaderCollection col, string entryId)
		{
			var entryLookup = new Dictionary<string, CusEntryHeader>();
			var list = col.ToList();

			foreach (var obj in list)
			{
				var cusEntryHeader = obj as CusEntryHeader;
				if (cusEntryHeader != null)
				{
					string key = DetermineEntryKey(cusEntryHeader);
					entryLookup.Add(key, cusEntryHeader);
				}
			}

			CusEntryHeader entryHeader = null;
			entryLookup.TryGetValue(entryId, out entryHeader);
			return entryHeader;
		}

		string DetermineEntryKey(CusEntryHeader entryHeader)
		{
			StringBuilder sb = new StringBuilder();
			var list = entryHeader.MergedLines.ToArray();

			foreach (var obj in list)
			{
				var entryLine = obj as CusEntryLine;
				if (entryLine != null)
				{
					sb.Append("-");
					sb.Append(entryLine.CustomsQuantity);
				}
			}

			string key = sb.ToString();
			return key;
		}

		public void TestDeclarationMessageFunctionNewAmendDelete()
		{
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var messageBuilderManager = new MessageBuilderManager();
			var codesToTest = new CDSDeclarationFunctionCode().GetAllCodes();
			foreach (var funcCode in codesToTest.Union(new[] { "Q" }))  // Q is any duff value
			{
				var builder = new H1MessageBuilder(entryHeader, new ErrorCollector(), funcCode);
				var xml = ((IGbCDSMessageBuilder)builder).Build();
				AssertContains(@"<FunctionCode>" + funcCode + @"</FunctionCode>
    <FunctionalReferenceID>", xml);
			}
		}

		public void TestOriginAndDestinationArePopulatedFromTheRightFields()
		{
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var declaration = entryHeader.Declaration;
			declaration.JE_RL_NKOrigin = "GBLHR";
			declaration.JE_RL_NKFinalDestination = "AUSYD";
			declaration.JE_GoodsOrigin = "YY";
			declaration.JE_GoodsDestination = "ZZ";

			var messageBuilderManager = new MessageBuilderManager();
			var codesToTest = new CDSDeclarationFunctionCode().GetAllCodes();
			foreach (var funcCode in codesToTest.Union(new[] { "Q" }))  // Q is any duff value
			{
				var builder = new H1MessageBuilder(entryHeader, new ErrorCollector(), funcCode);
				var xml = ((IGbCDSMessageBuilder)builder).Build();
				AssertContains(@"<Exporter>
      <Name>00200</Name>
      <Address>
        <CityName>00200</CityName>
        <CountryCode>YY</CountryCode>
        <Line>00200</Line>
        <PostcodeID>00200</PostcodeID>
      </Address>
    </Exporter>", xml);
				AssertContains(@"<Destination>
        <CountryCode>ZZ</CountryCode>
      </Destination>", xml);
			}
		}

		public void TestBorderTransportMeansNode()
		{
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var messageBuilderManager = new MessageBuilderManager();
			AmendTransportNationalityForTest(entryHeader);

			// H1
			AmendDeclarationTypeForTest(entryHeader, ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse);
			GenerateMessageAndAssertXml(entryHeader, messageBuilderManager, "</AuthorisationHolder><BorderTransportMeans><RegistrationNationalityCode>CN</RegistrationNationalityCode><ModeCode>1</ModeCode></BorderTransportMeans><Declarant>");

			// H2
			AmendDeclarationTypeForTest(entryHeader, ImportDeclarationTypeList.Codes.DeclarationForCustomsWarehousing);
			GenerateMessageAndAssertXml(entryHeader, messageBuilderManager, "</AuthorisationHolder><BorderTransportMeans><ModeCode>1</ModeCode></BorderTransportMeans><Declarant>");

			// H3
			AmendDeclarationTypeForTest(entryHeader, ImportDeclarationTypeList.Codes.DeclarationForTemporaryAdmission);
			GenerateMessageAndAssertXml(entryHeader, messageBuilderManager, "</AuthorisationHolder><BorderTransportMeans><RegistrationNationalityCode>CN</RegistrationNationalityCode><ModeCode>1</ModeCode></BorderTransportMeans><Declarant>");

			// H4
			AmendDeclarationTypeForTest(entryHeader, ImportDeclarationTypeList.Codes.DeclarationForInwardProcessing);
			GenerateMessageAndAssertXml(entryHeader, messageBuilderManager, "</AuthorisationHolder><BorderTransportMeans><RegistrationNationalityCode>CN</RegistrationNationalityCode><ModeCode>1</ModeCode></BorderTransportMeans><Declarant>");

			// H5
			AmendDeclarationTypeForTest(entryHeader, ImportDeclarationTypeList.Codes.DeclarationForGoodsFromTheSpecialFiscalTerritories);
			GenerateMessageAndAssertXml(entryHeader, messageBuilderManager, "</AuthorisationHolder><BorderTransportMeans><RegistrationNationalityCode>CN</RegistrationNationalityCode><ModeCode>1</ModeCode></BorderTransportMeans><Declarant>");

			// I1
			AmendDeclarationTypeForTest(entryHeader, ImportDeclarationTypeList.Codes.ImportSimplifiedDeclaration);
			GenerateMessageAndAssertXml(entryHeader, messageBuilderManager, "</AuthorisationHolder><BorderTransportMeans><ModeCode>1</ModeCode></BorderTransportMeans><Declarant>");
		}

		public void TestArrivalTransportMeansNode()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var messageBuilderManager = new MessageBuilderManager();

			// Empty nodes
			AmendTransportForTest(entryHeader, transportMode: Enterprise.Customs.Business.TransportTypeList.Codes.Sea, "", "", "", "");
			GenerateMessageAndAssertXml(entryHeader, messageBuilderManager, "<Consignment><ContainerCode>1</ContainerCode><GoodsLocation>");

			// H1/SEA
			AmendTransportForTest(entryHeader, transportMode: Enterprise.Customs.Business.TransportTypeList.Codes.Sea);
			GenerateMessageAndAssertXml(entryHeader, messageBuilderManager, "<Consignment><ContainerCode>1</ContainerCode><ArrivalTransportMeans><ID>BOATYMCBOATFACE</ID><IdentificationTypeCode>11</IdentificationTypeCode><ModeCode>3</ModeCode></ArrivalTransportMeans><GoodsLocation>");

			// H3/AIR
			AmendTransportForTest(entryHeader, transportMode: Enterprise.Customs.Business.TransportTypeList.Codes.Air);
			AmendDeclarationTypeForTest(entryHeader, ImportDeclarationTypeList.Codes.DeclarationForTemporaryAdmission);
			GenerateMessageAndAssertXml(entryHeader, messageBuilderManager, "<Consignment><ContainerCode>1</ContainerCode><ArrivalTransportMeans><ID>BA123</ID><IdentificationTypeCode>40</IdentificationTypeCode><ModeCode>3</ModeCode></ArrivalTransportMeans><GoodsLocation>");

			// H4/ROA
			AmendTransportForTest(entryHeader, transportMode: Enterprise.Customs.Business.TransportTypeList.Codes.Road);
			AmendDeclarationTypeForTest(entryHeader, ImportDeclarationTypeList.Codes.DeclarationForInwardProcessing);
			GenerateMessageAndAssertXml(entryHeader, messageBuilderManager, "<Consignment><ContainerCode>1</ContainerCode><ArrivalTransportMeans><ID>RS52FMS</ID><IdentificationTypeCode>30</IdentificationTypeCode><ModeCode>3</ModeCode></ArrivalTransportMeans><GoodsLocation>");

			// H5/RAI
			AmendTransportForTest(entryHeader, transportMode: Enterprise.Customs.Business.TransportTypeList.Codes.Rail, transportId: "CLARABEL");
			AmendDeclarationTypeForTest(entryHeader, ImportDeclarationTypeList.Codes.DeclarationForGoodsFromTheSpecialFiscalTerritories);
			GenerateMessageAndAssertXml(entryHeader, messageBuilderManager, "<Consignment><ContainerCode>1</ContainerCode><ArrivalTransportMeans><ID>CLARABEL</ID><IdentificationTypeCode>20</IdentificationTypeCode><ModeCode>3</ModeCode></ArrivalTransportMeans><GoodsLocation>");

			// ArrivalTransportMeans node with just ModeCode for H2
			AmendTransportForTest(entryHeader, transportMode: Enterprise.Customs.Business.TransportTypeList.Codes.Road);
			AmendDeclarationTypeForTest(entryHeader, ImportDeclarationTypeList.Codes.DeclarationForCustomsWarehousing);
			GenerateMessageAndAssertXml(entryHeader, messageBuilderManager, "<Consignment><ContainerCode>1</ContainerCode><ArrivalTransportMeans><ModeCode>3</ModeCode></ArrivalTransportMeans><GoodsLocation>");

			// ArrivalTransportMeans node not applicable to I1
			AmendTransportForTest(entryHeader, transportMode: Enterprise.Customs.Business.TransportTypeList.Codes.Road);
			AmendDeclarationTypeForTest(entryHeader, ImportDeclarationTypeList.Codes.ImportSimplifiedDeclaration);
			GenerateMessageAndAssertXml(entryHeader, messageBuilderManager, "<Consignment><ContainerCode>1</ContainerCode><GoodsLocation>");

			// ArrivalTransportMeans ID and IdentificationTypeCode not required for MAIL Transport Mode
			AmendTransportForTest(entryHeader, transportMode: Enterprise.Customs.Business.TransportTypeList.Codes.Mail);
			AmendDeclarationTypeForTest(entryHeader, ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse);
			GenerateMessageAndAssertXml(entryHeader, messageBuilderManager, "<Consignment><ContainerCode>1</ContainerCode><ArrivalTransportMeans><ModeCode>3</ModeCode></ArrivalTransportMeans><GoodsLocation>");

			// ArrivalTransportMeans ID and IdentificationTypeCode not required for FIX Transport Mode
			AmendTransportForTest(entryHeader, transportMode: Enterprise.Customs.Business.TransportTypeList.Codes.FixedTransportInstallations);
			AmendDeclarationTypeForTest(entryHeader, ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse);
			GenerateMessageAndAssertXml(entryHeader, messageBuilderManager, "<Consignment><ContainerCode>1</ContainerCode><ArrivalTransportMeans><ModeCode>3</ModeCode></ArrivalTransportMeans><GoodsLocation>");

			// ArrivalTransportMeans ID and IdentificationTypeCode required for OWN Transport Mode
			// NOTE: IdentificationTypeCode is 41 because we don't know what type to set it to
			AmendTransportForTest(entryHeader, transportId: "Quinjet NRW001", transportMode: Enterprise.Customs.Business.TransportTypeList.Codes.OwnPropulsion);
			AmendDeclarationTypeForTest(entryHeader, ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse);
			GenerateMessageAndAssertXml(entryHeader, messageBuilderManager, "<Consignment><ContainerCode>1</ContainerCode><ArrivalTransportMeans><ID>QuinjetNRW001</ID><IdentificationTypeCode>41</IdentificationTypeCode><ModeCode>3</ModeCode></ArrivalTransportMeans><GoodsLocation>");
		}

		public void TestInvoiceLineCountryOfOriginAndOriginOverride()
		{
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var messageBuilderManager = new MessageBuilderManager();

			entryHeader.Declaration.InvoiceLines[0].JI_CountryOfOrigin = "MD";
			entryHeader.Declaration.InvoiceLines[0].ZG_CountryOfSupply = "DM";
			entryHeader.Declaration.InvoiceLines[0].JI_PrimaryPreference = "100";

			GenerateMessageAndAssertXml(entryHeader, messageBuilderManager, "</GovernmentProcedure><Origin><CountryCode>MD</CountryCode><TypeCode>1</TypeCode></Origin><Packaging>");

			entryHeader.Declaration.InvoiceLines[0].JI_PrimaryPreference = "400";

			GenerateMessageAndAssertXml(entryHeader, messageBuilderManager, "</GovernmentProcedure><Origin><CountryCode>MD</CountryCode><TypeCode>2</TypeCode></Origin><Origin><CountryCode>DM</CountryCode><TypeCode>1</TypeCode></Origin><Packaging>");
		}

		void AmendTransportForTest(CusEntryHeader entryHeader
			, string transportMode = "SEA"
			, string transportModeInland = "ROA"
			, string transportId = "RS52FMS"
			, string voyageFlightNo = "BA123"
			, string vessel = "BOATY MCBOATFACE"
			, string transportNationality = "CN")
		{
			var declaration = entryHeader.Declaration;
			declaration.JE_TransportMode = transportMode;
			declaration.JE_TransportModeInland = transportModeInland;
			declaration.ZG_Box18TransportID = transportId;
			declaration.JE_VoyageFlightNo = voyageFlightNo;
			declaration.JE_VesselName = vessel;
			declaration.JE_RN_NKTransportNationality = transportNationality;
		}

		void AmendTransportNationalityForTest(CusEntryHeader entryHeader
			, string transportNationality = "CN")
		{
			var declaration = entryHeader.Declaration;
			declaration.JE_RN_NKTransportNationality = transportNationality;
		}

		void AmendDeclarationTypeForTest(CusEntryHeader entryHeader, string ceiDeclarationType = "H1", string ceiSubStyle = "Y")
		{
			entryHeader.Declaration.JE_DeclarationType = ceiDeclarationType;
			var cei = entryHeader.EntryInstruction;
			cei.CEI_Style = ceiDeclarationType;
			cei.CEI_SubStyle = ceiSubStyle;
		}

		void GenerateMessageAndAssertXml(CusEntryHeader entryHeader, MessageBuilderManager messageBuilderManager, string xmlExpected)
		{
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = Regex.Replace(messageBuilder.Build(), @"\s+", "");
			AssertXMLContains(xmlExpected, xml);
		}

		public void TestFiscalReferenceMessages()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			var messageBuilderManager = new MessageBuilderManager();
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			ZString xml;
			IGbCDSMessageBuilder messageBuilder;

			var frL1 = entryHeader.Declaration.InvoiceLines[0].FiscalReferences.AddNew();
			frL1.CFR_Code = "FR1";
			frL1.CFR_Reference = "GB11111111";
			var frL2 = entryHeader.Declaration.InvoiceLines[0].FiscalReferences.AddNew();
			frL2.CFR_Code = "FR2";
			frL2.CFR_Reference = "GB22222222";
			var frL3 = entryHeader.Declaration.InvoiceLines[1].FiscalReferences.AddNew();
			frL3.CFR_Code = "FR3";
			frL3.CFR_Reference = "GB33333333";
			var frL4 = entryHeader.Declaration.InvoiceLines[1].FiscalReferences.AddNew();
			frL4.CFR_Code = "FR4";
			frL4.CFR_Reference = "GB44444444";

			var frH1 = entryHeader.Declaration.CusEntryInstruction.FiscalReferences.AddNew();
			frH1.CFR_Code = "FR1";
			frH1.CFR_Reference = "GB55555555";
			var frH2 = entryHeader.Declaration.CusEntryInstruction.FiscalReferences.AddNew();
			frH2.CFR_Code = "FR2";
			frH2.CFR_Reference = "GB66666666";

			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			xml = messageBuilder.Build();
			AssertXMLEquals(GetExpectedH1FiscalReference(), xml);

			// Switch to I1
			var declaration = entryHeader.Declaration;
			declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.ImportSimplifiedDeclaration;
			declaration.JE_MessageSubType = "IM";
			entryHeader.EntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.ImportSimplifiedDeclaration;
			entryHeader.EntryInstruction.CEI_SubStyle = "Y";

			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			xml = messageBuilder.Build();
			AssertXMLEquals(GetExpectedI1FiscalReference(), xml);

			// Switch to H2
			entryHeader.CH_MasterUCR = "1234567890";
			declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.DeclarationForCustomsWarehousing;  // H2
			declaration.JE_MessageSubType = "IM";
			entryHeader.EntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.DeclarationForCustomsWarehousing;
			entryHeader.EntryInstruction.CEI_SubStyle = "Y";

			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			xml = messageBuilder.Build();
			AssertXMLEquals(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH2.xml"), xml);
		}

		public void TestTradeTermsFromEntryOrDeclaration()
		{
			var messageBuilderManager = new MessageBuilderManager();
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			ZString xml;
			IGbCDSMessageBuilder messageBuilder;

			entryHeader.RandomHeader.JZ_IncoTerm = "";
			entryHeader.RandomHeader.JZ_IncoTermPlace = "";
			entryHeader.Declaration.JE_ShipmentIncoTerm = "";
			entryHeader.Declaration.JE_ShipmentIncoTermPlace = "";

			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			xml = messageBuilder.Build();
			AssertNotContains("<TradeTerms>", xml);

			entryHeader.RandomHeader.JZ_IncoTerm = "CIF";
			entryHeader.RandomHeader.JZ_IncoTermPlace = "SOMEPLACE";
			xml = messageBuilder.Build();
			AssertXMLContains(GetExpectedTradeTerms("1"), xml);

			entryHeader.RandomHeader.JZ_IncoTerm = "";
			entryHeader.RandomHeader.JZ_IncoTermPlace = "";
			entryHeader.Declaration.JE_ShipmentIncoTerm = "FOB";
			entryHeader.Declaration.JE_ShipmentIncoTermPlace = "ANOTHERPLACE";
			xml = messageBuilder.Build();
			AssertXMLContains(GetExpectedTradeTerms("2"), xml);

			entryHeader.RandomHeader.JZ_IncoTerm = "CIF";
			entryHeader.RandomHeader.JZ_IncoTermPlace = "PRIORITYTEST";
			entryHeader.Declaration.JE_ShipmentIncoTerm = "FOB";
			entryHeader.Declaration.JE_ShipmentIncoTermPlace = "ANOTHERPLACE";
			xml = messageBuilder.Build();
			AssertXMLContains(GetExpectedTradeTerms("3"), xml);

			entryHeader.RandomHeader.JZ_IncoTerm = "";
			entryHeader.RandomHeader.JZ_IncoTermPlace = "MIXEDTEST";
			entryHeader.Declaration.JE_ShipmentIncoTerm = "FOB";
			entryHeader.Declaration.JE_ShipmentIncoTermPlace = "";
			xml = messageBuilder.Build();
			AssertXMLContains(GetExpectedTradeTerms("4"), xml);
		}

		public void TestTransportChargesMethodOfPayment()
		{
			var messageBuilderManager = new MessageBuilderManager();
			const string B1 = ExportDeclarationTypeList.Codes.DeclarationForExport;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(B1);
			ZString xml;
			IGbCDSMessageBuilder messageBuilder;

			var declaration = entryHeader.Declaration;

			declaration.Invoices.DeleteAll();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.ZG_TransportChargesMethodOfPayment = "X";
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_Procedure = "4000001";
			invoiceLine1.JI_Description = "Item 1";
			invoiceLine1.JI_LinePrice = 111.11;
			var pack1 = invoiceLine1.PackagesForInvoiceLinesForBindingOnly[1];
			pack1.IsLinked = true;
			pack1.PackQty = 11;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.ZG_TransportChargesMethodOfPayment = "Y";
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_Procedure = "4000002";
			invoiceLine2.JI_Description = "Item 2";
			invoiceLine2.JI_LinePrice = 222.22;
			var pack2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly[1];
			pack2.IsLinked = true;
			pack2.PackQty = 22;

			var invoice3 = declaration.Invoices.AddNew();
			invoice3.ZG_TransportChargesMethodOfPayment = ""; // Should be ignored
			var invoiceLine3 = invoice3.InvoiceLines.AddNew();
			invoiceLine3.JI_Procedure = "4000003";
			invoiceLine3.JI_Description = "Item 3";
			invoiceLine3.JI_LinePrice = 333.33;
			var pack3 = invoiceLine3.PackagesForInvoiceLinesForBindingOnly[1];
			pack3.IsLinked = true;
			pack3.PackQty = 33;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();

			declaration.DoMerge();

			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEquals(3, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			declaration.JE_EntryAuthorisationDate = new ZDateTime(1987, 12, 11, 1, 2, 3);

			var errors = new ErrorCollector();

			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), errors, new Customs.Business.CusdecMessageFunction.New());
			xml = messageBuilder.Build();
			CombineAssertions("Checking for Consignment Items", () =>
			{
				//Check for ConsignmentItem in Consignment with first sequence
				AssertXMLContains(@"<Consignment>
      <Carrier>
        <ID>GB123456789000</ID>
      </Carrier>
      <ConsignmentItem>
        <SequenceNumeric>1</SequenceNumeric>", xml);

				//Check for 2 Sequence numbers
				AssertXMLContains(@"<ConsignmentItem>
        <SequenceNumeric>1</SequenceNumeric>
        <Freight>
          <PaymentMethodCode>", xml);
				AssertXMLContains(@"<ConsignmentItem>
        <SequenceNumeric>2</SequenceNumeric>
        <Freight>
          <PaymentMethodCode>", xml);
				AssertNotContains(@"<ConsignmentItem>
        <SequenceNumeric>3</SequenceNumeric>", xml);

				//Check for each payment method
				AssertXMLContains(@"<Freight>
          <PaymentMethodCode>X</PaymentMethodCode>
        </Freight>", xml);
				AssertXMLContains(@"<Freight>
          <PaymentMethodCode>Y</PaymentMethodCode>
        </Freight>", xml);
			});

			invoice2.ZG_TransportChargesMethodOfPayment = "X";
			declaration.DoMerge();

			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEquals(3, declaration.CustomsEntryHeaders[0].MergedLines.Count);
			errors.WipeErrors();

			decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), errors, new Customs.Business.CusdecMessageFunction.New());
			xml = messageBuilder.Build();
			AssertXMLContains(@"<Consignment>
      <Carrier>
        <ID>GB123456789000</ID>
      </Carrier>
      <Freight>
        <PaymentMethodCode>X</PaymentMethodCode>
      </Freight>
      <Itinerary>
        <SequenceNumeric>1</SequenceNumeric>", xml);
		}

		public void TestDecimalPlacesIncludeTrailingZeros()
		{
			const string B1 = ExportDeclarationTypeList.Codes.DeclarationForExport;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(B1);

			entryHeader.Declaration.Invoices[0].JZ_InvoiceAmount = 111;
			entryHeader.Declaration.Invoices[1].JZ_InvoiceAmount = 111;
			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(@"<InvoiceAmount currencyID=""GBP"">222.00</InvoiceAmount>", xml);

			entryHeader.Declaration.Invoices[0].JZ_InvoiceAmount = 111.1;
			entryHeader.Declaration.Invoices[1].JZ_InvoiceAmount = 111.1;
			xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains(@"<InvoiceAmount currencyID=""GBP"">222.20</InvoiceAmount>", xml);
		}

		public void TestExportsMessageMarksAndNumbersMaximumLength()
		{
			const string B4 = ExportDeclarationTypeList.Codes.DeclarationForDispatchOfGoods;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(B4);
			var longMarksAndNumbers = string.Join("", Enumerable.Repeat("0123456789ABCDEF", 5));
			var package = entryHeader.Declaration.Bills[1].PackingGroups[0].Packages[1];
			package.CW_MarksAndNos = longMarksAndNumbers;
			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);

			var expectedMarksAndNumbers = longMarksAndNumbers.Substring(0, 42);
			AssertXMLContains($"<MarksNumbersID>{expectedMarksAndNumbers}</MarksNumbersID>", xml);
		}

		public void TestImporterIdOutputsForOverriddenAddress()
		{
			var messageBuilderManager = new MessageBuilderManager();
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);

			var orgHeader = CreateOrgHeaderWithEori("123", "GB");
			entryHeader.Declaration.ImporterDocumentaryAddress.OrganisationPK = orgHeader.PK;

			ZString xml;
			IGbCDSMessageBuilder messageBuilder;
			entryHeader.Declaration.ImporterDocumentaryAddress.E2_AddressOverride = ZBool.True;
			SetupAddress(entryHeader.Declaration.ImporterDocumentaryAddress, "1");
			entryHeader.Declaration.ImporterDocumentaryAddress.E2_GovRegNum = "777";
			entryHeader.Declaration.ImporterDocumentaryAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;

			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			xml = messageBuilder.Build();
			AssertXMLContains(@"<Importer>
        <Name>Company Name 1</Name>
        <ID>GB777</ID>
        <Address>
          <CityName>City 1</CityName>
          <CountryCode>GB</CountryCode>
          <Line>Address1 1</Line>
          <PostcodeID>Post 1</PostcodeID>
        </Address>
      </Importer>", xml);
		}

		public void TestExporterCountryCodeOutputsForNonOverriddenAddress()
		{
			var messageBuilderManager = new MessageBuilderManager();
			var entryHeader = CreateSampleEntryHeaderForGuaranteeRelatedEntryInstruction(Factory);

			var orgHeader = CreateOrgHeaderWithEori("123", "GB");
			orgHeader.CustomsCodes.RemoveAndDeleteAll();
			entryHeader.Declaration.SupplierDocumentaryAddress.OrganisationPK = orgHeader.PK;

			ZString xml;
			IGbCDSMessageBuilder messageBuilder;

			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			xml = messageBuilder.Build();
			AssertXMLContains(@"<Exporter>
      <Name>Company Name 1</Name>
      <Address>
        <CityName>City 1</CityName>
        <CountryCode>GB</CountryCode>
        <Line>Address1 1</Line>
        <PostcodeID>Post 1</PostcodeID>
      </Address>
    </Exporter>", xml);
		}

		public void TestExporterCountryCodeOutputsForOverriddenAddress()
		{
			var messageBuilderManager = new MessageBuilderManager();
			var entryHeader = CreateSampleEntryHeaderForGuaranteeRelatedEntryInstruction(Factory);

			var orgHeader = CreateOrgHeaderWithEori("123", "GB");
			entryHeader.Declaration.SupplierDocumentaryAddress.OrganisationPK = orgHeader.PK;

			ZString xml;
			IGbCDSMessageBuilder messageBuilder;
			entryHeader.Declaration.SupplierDocumentaryAddress.E2_AddressOverride = ZBool.True;
			SetupAddress(entryHeader.Declaration.SupplierDocumentaryAddress, "1");
			entryHeader.Declaration.SupplierDocumentaryAddress.E2_GovRegNum = "777";
			entryHeader.Declaration.SupplierDocumentaryAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;

			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			xml = messageBuilder.Build();
			AssertXMLContains(@"<Exporter>
      <Name>Company Name 1</Name>
      <ID>GB777</ID>
      <Address>
        <CityName>City 1</CityName>
        <CountryCode>GB</CountryCode>
        <Line>Address1 1</Line>
        <PostcodeID>Post 1</PostcodeID>
      </Address>
    </Exporter>", xml);
		}

		public void TestFallbackDefaultCountryCodeInExporter()
		{
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var defaultSupplier = entryHeader.InvoiceHeaders[0].JZ_OH_Supplier;
			entryHeader.InvoiceHeaders[0].JZ_OH_Supplier = ZGuid.Empty;
			var invoice = entryHeader.Declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = defaultSupplier;
			entryHeader.InvoiceHeaders[0].InvoiceLines[1].JI_JZ = invoice.PK;

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();
			AssertXMLContains($@"<Exporter>
      <Name>00200</Name>
      <Address>
        <CityName>00200</CityName>
        <CountryCode>HU</CountryCode>
        <Line>00200</Line>
        <PostcodeID>00200</PostcodeID>
      </Address>
    </Exporter>", xml);
		}

		public void TestFullWidthXmlSpecialCharactersAreEscaped()
		{
			var orgHeader = CreateOrgHeaderWithEori("123", "GB");
			orgHeader.MainAddress.CompanyName = "＂FANCY＂ Company ＆ Co.＇ Ltd. ＜UK＞";
			orgHeader.CustomsCodes.RemoveAndDeleteAll();

			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			entryHeader.InvoiceHeaders[1].JZ_OH_Supplier = entryHeader.InvoiceHeaders[0].JZ_OH_Supplier;
			entryHeader.Declaration.SupplierDocumentaryAddress.OrganisationPK = orgHeader.PK;

			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);
			AssertXMLContains("<Name>\"FANCY\" Company &amp; Co.' Ltd. &lt;UK&gt;</Name>", xml);
		}

		OrgHeader CreateOrgHeaderWithEori(string eoriCode, string countryCode)
		{
			var result = Factory.New<OrgHeader>();
			result.OH_Code = "ImporterA";
			result.OH_RL_NKClosestPort = "GBLHR";
			result.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode, countryCode);

			SetupAddress(result.MainAddress, "1");

			return result;
		}

		void SetupAddress(OrgAddress address, ZString suffix)
		{
			address.CompanyName = "Company Name " + suffix;
			address.Address1 = "Address1 " + suffix;
			address.City = "City " + suffix;
			address.Postcode = "Post " + suffix;
			address.OA_RN_NKCountryCode = "GB";
		}

		void SetupAddress(JobDocAddress address, ZString suffix)
		{
			address.E2_CompanyName = "Company Name " + suffix;
			address.E2_Address1 = "Address1 " + suffix;
			address.E2_City = "City " + suffix;
			address.E2_Postcode = "Post " + suffix;
		}

		public static ZString GetExpectedH1()
		{
			return EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH1.xml");
		}
		public static ZString GetExpectedH1WithItemLevelExportCountry()
		{
			return EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH1_SendingExportCountryAtItem.xml");
		}

		public static ZString GetExpectedXMLForAgentRep(ZString sPath)
		{
			return EmbeddedResource.GetExpectedMessageXml(sPath);
		}

		public static ZString GetExpectedH1FiscalReference()
		{
			return EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH1-FiscalReference.xml");
		}
		public static ZString GetExpectedI1FiscalReference()
		{
			return EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedI1-FiscalReference.xml");
		}

		public static ZString GetExpectedH1WithoutSupervisingOffice()
		{
			return EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH1WithoutSupervisingOffice.xml");
		}

		public static ZString GetExpectedH1WithItemDestinations()
		{
			return EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH1WithItemDestinations.xml");
		}

		public static ZString GetExpectedH1WithWritingOff()
		{
			return EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH1WithWritingOff.xml");
		}

		public static ZString GetExpectedH1WithWritingOffZero()
		{
			return EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH1WithWritingOffZero.xml");
		}

		public static ZString GetExpectedH5WithWritingOff()
		{
			return EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH5WithWritingOff.xml");
		}

		public static ZString GetExpectedH5WithWritingOffZero()
		{
			return EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH5WithWritingOffZero.xml");
		}

		public static ZString GetExpectedH4WithWritingOff()
		{
			return EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH4WithWritingOff.xml");
		}

		public static ZString GetExpectedH4WithWritingOffZero()
		{
			return EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH4WithWritingOffZero.xml");
		}

		public static ZString GetExpectedH1WithAdditionalInfo_Scenario_01_Entry_01_Line_01()
		{
			return EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH1WithAdditionalInfo_Scenario_01_Entry_01_Line_01.xml");
		}

		public static ZString GetExpectedH1WithAdditionalInfo_Scenario_01_Entry_01_Line_02()
		{
			return EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH1WithAdditionalInfo_Scenario_01_Entry_01_Line_02.xml");
		}

		public static ZString GetExpectedH1WithAdditionalInfo_Scenario_02_Entry_01_Line_01()
		{
			return EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH1WithAdditionalInfo_Scenario_02_Entry_01_Line_01.xml");
		}

		public static ZString GetExpectedH1WithAdditionalInfo_Scenario_02_Entry_01_Line_02()
		{
			return EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH1WithAdditionalInfo_Scenario_02_Entry_01_Line_02.xml");
		}

		public static ZString GetExpectedH1WithAdditionalInfo_Scenario_02_Entry_02_Line_01()
		{
			return EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH1WithAdditionalInfo_Scenario_02_Entry_02_Line_01.xml");
		}

		public static ZString GetExpectedH1WithAdditionalInfo_Scenario_02_Entry_02_Line_02()
		{
			return EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH1WithAdditionalInfo_Scenario_02_Entry_02_Line_02.xml");
		}

		public static ZString GetExpectedH1WithAdditionalInfo_Scenario_02_Entry_02_Line_03()
		{
			return EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH1WithAdditionalInfo_Scenario_02_Entry_02_Line_03.xml");
		}

		public static ZString GetExpectedH1WithAdditionalInfo_Scenario_03_Entry_01_Line_01()
		{
			return EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH1WithAdditionalInfo_Scenario_03_Entry_01_Line_01.xml");
		}

		public static ZString GetExpectedH1WithAdditionalInfo_Scenario_03_Entry_02_Line_01()
		{
			return EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH1WithAdditionalInfo_Scenario_03_Entry_02_Line_01.xml");
		}

		public static ZString GetExpectedH1WithAdditionalInfo_Scenario_03_Entry_02_Line_02()
		{
			return EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH1WithAdditionalInfo_Scenario_03_Entry_02_Line_02.xml");
		}

		public static ZString GetExpectedH1WithAdditionalInfo_Scenario_03_Entry_03_Line_01()
		{
			return EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH1WithAdditionalInfo_Scenario_03_Entry_03_Line_01.xml");
		}

		public static ZString GetExpectedH1WithAdditionalInfo_Scenario_03_Entry_03_Line_02()
		{
			return EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH1WithAdditionalInfo_Scenario_03_Entry_03_Line_02.xml");
		}

		public static ZString GetExpectedH1WithAdditionalInfo_Scenario_03_Entry_03_Line_03()
		{
			return EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH1WithAdditionalInfo_Scenario_03_Entry_03_Line_03.xml");
		}

		public static ZString GetExpectedH1WithAdditionalInfo_Scenario_04_Entry_01_Line_01()
		{
			return EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH1WithAdditionalInfo_Scenario_04_Entry_01_Line_01.xml");
		}

		public static ZString GetExpectedH1WithAdditionalInfo_Scenario_04_Entry_01_Line_02()
		{
			return EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedH1WithAdditionalInfo_Scenario_04_Entry_01_Line_02.xml");
		}

		public static ZString GetExpectedTradeTerms(ZString key)
		{
			return EmbeddedResource.GetExpectedMessageXml($"Messaging.ExpectedTradeTerms_Partial{key}.xml");
		}

		CusEntryHeader CreateSampleEntryHeaderForGuaranteeRelatedEntryInstruction(BusinessObjectFactory factory, bool setDucrExplicitly = true)
		{
			CreateProcedures(factory, new ZString[] { "4012123", "4012456", "4012ABC" });
			factory.Save();
			var facility1_FlipThePort = EU.Business.Testing.ShedTest.CreateShed(factory, "GB", "AAABBB", "Daniel Test", chiefPort: "CCC");
			var facility2_FlipTheShed = EU.Business.Testing.ShedTest.CreateShed(factory, "GB", "XXXYYY", "Daniel Test", chiefShed: "ZZZ");
			var facility3_Verbatim = EU.Business.Testing.ShedTest.CreateShed(factory, "GB", "MMMNNN", "Daniel Test");
			factory.Save();

			var helper = new DeclarationTestHelper(factory);
			var org1 = helper.MakeOrganisation1();
			org1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB025115100006", Core.Constants.CountryCodes.UnitedKingdom);
			org1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CustomsClientCode, "GBBEL001", Core.Constants.CountryCodes.UnitedKingdom);
			var org2 = helper.MakeOrganisation2();
			var declaration = helper.CreateExportAirDeclaration();
			GBCustomsDataRegistry.Instance.BadgeCodes.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new BadgeCodeSettingCollection
				{
					new BadgeCodeSetting
					{
						BadgeCode = "DSK",
						RL_PortCode = "GBLBA",
						Direction = "IMP",
						CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW,
						MasterUcrCalculationMode = Registry.MucrGenerationStyles.Codes.Ccsuk,
						ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services,
					}
				});
			GBCustomsDataRegistry.Instance.Credentials.SetTemporaryValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, new CredentialsSettingCollection
				{
					new CredentialsSetting
					{
						BadgeCode = "DSK",
						Printer = "Location",
						Company = "Role",
						Username = "Username",
						Password = "Password"
					}
				});
			declaration.JE_CustomsProfile = "DSK";
			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			var cei1 = factory.New<CusEntryInstruction>();
			cei1.CEI_JE = declaration.PK;
			cei1.CEI_PackageCount = 55;
			cei1.CEI_Style = ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse;
			cei1.CEI_SubStyle = "Y";

			var cei2 = factory.New<CusEntryInstruction>();
			cei2.CEI_JE = declaration.PK;
			cei2.CEI_PackageCount = 66;
			cei2.CEI_Style = ImportDeclarationTypeList.Codes.DeclarationForGoodsFromTheSpecialFiscalTerritories;
			cei2.CEI_SubStyle = "Y";

			declaration.JE_TransportMode = "SEA";
			declaration.JE_CustomsOffice = "P.Office";
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			declaration.Declarant.Header.OH_FullName = "DJC CUSTOMS BROKERS";  // avoiding stupid rule CW1012
			declaration.JE_MessageSubType = "IM";
			declaration.ZG_OthChgAmt = 6m;
			declaration.ZG_RX_NKOthChg = "B67";

			var guarantee1 = declaration.Guarantees.AddNew();
			guarantee1.PW_BondType = GuaranteeTypeList.Codes.Guarantee;
			guarantee1.PW_Password = guarantee1.Lookups.AuthorisationTypeList[0].Code;
			guarantee1.PW_HolderIdentification = "111";
			guarantee1.EntryInstructionID = cei1.PK;

			var guarantee2 = declaration.Guarantees.AddNew();
			guarantee2.PW_BondType = GuaranteeTypeList.Codes.Guarantee;
			guarantee2.PW_Password = guarantee2.Lookups.AuthorisationTypeList[0].Code;
			guarantee2.PW_HolderIdentification = "222";
			guarantee2.EntryInstructionID = cei2.PK;

			var guarantee3 = declaration.Guarantees.AddNew();
			guarantee3.PW_BondType = GuaranteeTypeList.Codes.Guarantee;
			guarantee3.PW_Password = guarantee2.Lookups.AuthorisationTypeList[0].Code;
			guarantee3.PW_HolderIdentification = "333";

			var invoice1 = declaration.Invoices[0];
			var invoiceLine1 = invoice1.InvoiceLines[0];
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = cei1.PK;
			if (setDucrExplicitly)
			{
				entryHeader.CH_BGMReference = "8GB123456789000-S0001000";
			}
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;

			var authOwner = factory.New<OrgHeader>();
			authOwner.CustomsCodes.AddNew("EOR", "123");
			authOwner.OH_Code = "123";

			var authorisation1 = entryHeader.EntryInstruction.CusAuthorizationUsages.AddNew();
			authorisation1.AGC_Code = "ACE";
			authorisation1.AGC_Number = "GB123";
			authorisation1.AGC_OH_Owner = authOwner.PK;

			var authOwner2 = factory.New<OrgHeader>();
			authOwner2.CustomsCodes.AddNew("EOR", "456");
			authOwner2.OH_Code = "456";

			var authorisation2 = entryHeader.EntryInstruction.CusAuthorizationUsages.AddNew();
			authorisation2.AGC_Code = "SDE";
			authorisation2.AGC_Number = "GB456";
			authorisation2.AGC_OH_Owner = authOwner2.PK;

			return entryHeader;
		}

		public static CusEntryHeader CreateSampleEntryHeaderForH1(BusinessObjectFactory factory, bool setDucrExplicitly = true)
		{
			var subs = factory.New<UNDGSubstance>();
			subs.DG_UNNO = "1234";
			subs.DG_Variant = "A";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs.DG_LQMaxAmt = 250;
			subs.DG_LQMaxAmtUQ = "L";

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "HYE";
			registrationKey.ServerCodeForTest = "CMT";

			CreateProcedures(factory, new ZString[] { "4012123", "4012456", "4012ABC" });
			factory.Save();
			var facility1_FlipThePort = EU.Business.Testing.ShedTest.CreateShed(factory, "GB", "AAABBB", "Daniel Test", chiefPort: "CCC");
			var facility2_FlipTheShed = EU.Business.Testing.ShedTest.CreateShed(factory, "GB", "XXXYYY", "Daniel Test", chiefShed: "ZZZ");
			var facility3_Verbatim = EU.Business.Testing.ShedTest.CreateShed(factory, "GB", "MMMNNN", "Daniel Test");
			factory.Save();

			var helper = new DeclarationTestHelper(factory);
			var org1 = helper.MakeOrganisation1();
			org1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB025115100006", Core.Constants.CountryCodes.UnitedKingdom);
			org1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CustomsClientCode, "GBBEL001", Core.Constants.CountryCodes.UnitedKingdom);
			var org2 = helper.MakeOrganisation2();
			var declaration = helper.CreateExportAirDeclaration();
			GBCustomsDataRegistry.Instance.BadgeCodes.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new BadgeCodeSettingCollection
				{
					new BadgeCodeSetting
					{
						BadgeCode = "DSK",
						RL_PortCode = "GBLBA",
						Direction = "IMP",
						CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW,
						MasterUcrCalculationMode = Registry.MucrGenerationStyles.Codes.Ccsuk,
						ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services,
					}
				});
			GBCustomsDataRegistry.Instance.Credentials.SetTemporaryValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, new CredentialsSettingCollection
				{
					new CredentialsSetting
					{
						BadgeCode = "DSK",
						Printer = "Location",
						Company = "Role",
						Username = "Username",
						Password = "Password"
					}
				});
			declaration.JE_CustomsProfile = "DSK";
			declaration.ZG_Gateway = GatewayList.Codes.Pentant;

			factory.Save();

			declaration.JE_TransportMode = "SEA";
			declaration.JE_CustomsOffice = "P.Office";

			declaration.ShippingLine.CustomsCodes.AddNew("EOR", "123456789000");
			declaration.SupervisingOfficeDocAddress.E2_OA_Address = org1.MainAddress.PK;
			declaration.InvoiceLines[0].JI_OA_SupervisingOffice = org1.MainAddress.PK;
			declaration.BuyerDocAddress.OrganisationPK = org2.PK;
			declaration.BuyerDocAddress.ClosestPort = org2.OH_RL_NKClosestPort;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();

			var instruction1 = factory.New<CusEntryInstruction>();
			instruction1.CEI_JE = declaration.PK;
			instruction1.CEI_PackageCount = 55;
			if (!declaration.CustomsEntryInstructions.Any())
			{
				declaration.CustomsEntryInstructions.Add(instruction1);
			}
			factory.Save();

			var representative = declaration.DocAddresses.AddNew(DocAddressType.Representative);
			representative.OrganisationPK = org1.PK;
			declaration.ImporterDocumentaryAddress.OrganisationPK = org1.PK;
			declaration.SupplierDocumentaryAddress.OrganisationPK = org1.PK;
			declaration.SupplierDocumentaryAddress.ClosestPort = org1.OH_RL_NKClosestPort;
			declaration.JE_OA_SellerAddress = org2.MainAddress.PK;
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_MessageType = "IMP";
			declaration.JE_RL_NKOrigin = org1.OH_RL_NKClosestPort;
			instruction1.CEI_Style = ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse;  // H1 for CDS (EntryInstruction)
			instruction1.CEI_SubStyle = "Y";  // H1 for CDS (EntryInstruction)
			declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse;  // H1 for CHIEF
			declaration.Declarant.Header.OH_FullName = "DJC CUSTOMS BROKERS";  // avoiding stupid rule CW1012
			declaration.ZG_OthChgAmt = 6m;
			declaration.ZG_RX_NKOthChg = "B67";
			var decSuppDoc = declaration.SupportingDocuments.AddNew();
			decSuppDoc.CSI_Code = "C514";
			decSuppDoc.CSI_ReferenceNumber = "GBEIRGB077440010006";

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CNT123";
			container.CO_Seal = "123";
			container.CO_SecondSeal = "456";

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CNT456";

			var authOwner = factory.New<OrgHeader>();
			authOwner.CustomsCodes.AddNew("EOR", "123");
			authOwner.OH_Code = "123";

			var authorisation1 = instruction1.CusAuthorizationUsages.AddNew();
			authorisation1.AGC_Code = "ACE";
			authorisation1.AGC_Number = "GB123";
			authorisation1.AGC_OH_Owner = authOwner.PK;

			var guarantee1 = declaration.Guarantees.AddNew();
			guarantee1.PW_BondType = GuaranteeTypeList.Codes.Guarantee;
			guarantee1.PW_Password = "ACE";
			guarantee1.PW_HolderIdentification = "456";
			guarantee1.PW_BondNumber = "789";

			var exporter = factory.New<OrgHeader>();
			exporter.OH_Code = "EXP";
			var importer = factory.New<OrgHeader>();
			importer.OH_Code = "IMP";

			declaration.JE_TotalNoOfPacks = 55;
			declaration.JE_OH_Exporter = exporter.PK;
			declaration.JE_OH_Importer = importer.PK;

			importer.OH_FullName = "Test Importer Name which is really big and is more than seventy characters in length";
			importer.MainAddress.Address1 = "Test Importer Address Line which is over 35 chars";
			var importerAddress = importer.Addresses.AddNew();
			importerAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			importerAddress.OA_Address1 = "Test Importer Address Line which is over 35 chars";

			var invoice1 = declaration.Invoices[0];
			invoice1.JZ_OH_Supplier = org2.PK;
			invoice1.JZ_RX_NKInvoice_Currency = "GBP";
			var invoiceLine1 = invoice1.InvoiceLines[0];
			invoiceLine1.ZG_MethodOfPayment = Constants.MethodOfPayment.DeferredPayment;
			invoiceLine1.JI_Procedure = "4012123";
			invoiceLine1.JI_FormattedTariff = "4202.12.1900";
			invoiceLine1.JI_CustomsUnitQty = "KGM";
			invoiceLine1.JI_ZZF_NKTaxType = "673";
			invoiceLine1.JI_SupplementaryCode1 = "123";
			invoiceLine1.JI_SupplementaryCode2 = "456";
			invoiceLine1.ZG_CountryOfSupply = "NZ";
			invoiceLine1.AdditionalProcedureCodes.AddNew().CY_Code = "4012456";
			invoiceLine1.AdditionalProcedureCodes.AddNew().CY_Code = "4012ABC";
			invoiceLine1.ZG_CountryOfDestination = "GB";
			invoiceLine1.JI_LinePrice = 100;
			var undg = invoiceLine1.UNDGs.AddNew();
			undg.DI_DG = subs.PK;
			undg.LinkDefault(subs);
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction1.PK;
			if (setDucrExplicitly)
			{
				entryHeader.CH_BGMReference = "8GB123456789000-S0001000";
			}
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			var entryLine3 = entryHeader.AllEntryLines.AddNew();
			var entryLine4 = entryHeader.AllEntryLines.AddNew();
			var entryLine5 = entryHeader.AllEntryLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			var fee0 = entryLine1.Fees.AddNew();
			fee0.CF_ChargeType = "B00";
			fee0.CF_ChargeType = "B00";
			var fee1 = entryLine1.Fees.AddNew();
			fee1.CF_ChargeType = "A00";
			fee1.CF_MethodOfCalculation = "HLT";
			fee1.CF_BaseValue = 60m;
			fee1.CF_ChargeAmount = 7260m;
			fee1.CF_Rate = 121m;
			var fee2 = entryLine1.Fees.AddNew();
			fee2.CF_ChargeType = "A00";
			fee2.CF_MethodOfCalculation = "%";
			fee2.CF_BaseValue = 1000m;
			fee2.CF_ChargeAmount = 400m;
			fee2.CF_Rate = 40m;
			var fee3 = entryLine1.Fees.AddNew();
			fee3.CF_ChargeType = "411";
			fee3.CF_MethodOfPayment = "E";
			invoiceLine1.JI_CustomsThirdUnitQty = "ABC";
			invoiceLine1.JI_ConcessionOrder = "123456";
			var fee4 = entryLine1.Fees.AddNew();
			fee4.CF_ChargeType = "A00";
			fee4.CF_MethodOfCalculation = "DTNE";
			fee4.CF_BaseValue = 5m;
			fee4.CF_ChargeAmount = 103m;
			fee4.CF_Rate = 20m;
			var invoiceLine2 = invoice1.InvoiceLines[1];
			invoiceLine2.ZG_MethodOfPayment = Constants.MethodOfPayment.DeferredPayment;
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_ZZF_NKTaxType = "650";
			invoiceLine2.JI_LinePrice = 200;

			var invoiceLine3 = invoiceLine1.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine3.JI_ZZF_NKTaxType = "654";
			invoiceLine3.JI_CL = entryLine3.PK;
			invoiceLine3.JI_LinePrice = 300;

			var invoiceLine4 = invoiceLine1.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine4.JI_ZZF_NKTaxType = "666";
			invoiceLine4.JI_CL = entryLine4.PK;
			invoiceLine4.JI_LinePrice = 400;
			declaration.CusEntryInstruction.CEI_PackageCount = 55;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_OH_Supplier = org1.PK;
			invoice2.JZ_RX_NKInvoice_Currency = "GBP";
			var invoice2Line1 = invoice2.InvoiceLines.AddNew();
			invoice2Line1.JI_CL = entryLine5.PK;
			invoice2Line1.JI_LinePrice = 500;

			invoice1.JZ_InvoiceAmount = 1000;
			invoice2.JZ_InvoiceAmount = 500;

			var ucrnumber = factory.New<CusEntryNumber>();
			ucrnumber.CE_ParentID = entryHeader.PK;
			ucrnumber.CE_ParentTable = entryHeader.TableName;
			ucrnumber.CE_EntryType = CusEntryNumberTypes.Standard.UniqueConsignementReference;
			ucrnumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			ucrnumber.CE_EntryNum = "DUCR/1";
			entryHeader.CH_CEI_Instruction = instruction1.PK;

			invoice1.RelatedIndicator = false;
			invoice1.RelatedIndicator2 = false;
			invoice1.RelatedIndicator3 = false;
			invoice1.RelatedIndicator4 = false;

			invoiceLine1.JI_ValuationCode = "1";
			invoiceLine1.RelatedIndicator = false;
			invoiceLine1.RelatedIndicator2 = true;
			invoiceLine1.RelatedIndicator3 = false;
			invoiceLine1.RelatedIndicator4 = false;

			invoiceLine2.JI_ValuationCode = "1";
			invoiceLine2.RelatedIndicator = false;
			invoiceLine2.RelatedIndicator2 = false;
			invoiceLine2.RelatedIndicator3 = false;
			invoiceLine2.RelatedIndicator4 = true;

			invoiceLine3.JI_ValuationCode = "2";

			invoiceLine4.JI_ValuationCode = "1";
			invoiceLine4.RelatedIndicator = true;
			invoiceLine4.RelatedIndicator2 = true;
			invoiceLine4.RelatedIndicator3 = false;
			invoiceLine4.RelatedIndicator4 = true;

			entryHeader.RandomHeader.JZ_ValuationCode = "XX";

			declaration.JE_Calc_LocationOtherInformationCountry = "GB";
			declaration.JE_Calc_LocationOtherInformationType = "BY";
			declaration.JE_LocationQualifier = "CW";
			declaration.JE_GoodsLocation = "1234567";

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2019, 3, 3, 3, 3, 3, 3, DateTimeKind.Utc);
			declaration.JE_DefermentAccountNumber = "111111";
			declaration.ZG_VATDeferNumber = "222222";

			var pivot = declaration.InvoiceLines[0].PackagesForInvoiceLinesForBindingOnly[1];
			pivot.IsLinked = false;
			pivot.IsLinked = true;

			declaration.Invoices[0].InvoiceLines[0].ContainersForInvoiceLinesForBindingOnly[1].IsForInvoiceLine = true;

			declaration.TopGroupInvoice.Charges.RemoveAndDeleteAll();
			var charge1 = invoice1.Charges.AddNew();
			var charge2 = declaration.TopGroupInvoice.Charges.AddNew();
			var charge3 = declaration.TopGroupInvoice.Charges.AddNew();
			InvChargeTestHelper.SetUpCharge(charge1, "INT", false, 0m, "VAL", false, 50m);//AC Item
			InvChargeTestHelper.SetUpCharge(charge2, "ONS", true, 0m, "VAL", false, 100m);//AK Header
			InvChargeTestHelper.SetUpCharge(charge3, "ADD", true, 0m, "VAL", false, 10m);//AT Item

			var lineCharge1 = invoiceLine1.Charges.AddNew();
			InvChargeTestHelper.SetUpCharge(lineCharge1, "ADD", false, 0m, string.Empty, false, 1m);//AT Item

			declaration.Transports.RemoveAndDeleteAll();

			var sanJuan = declaration.Transports.AddNew();
			var mexico = declaration.Transports.AddNew();
			var sauPaulo = declaration.Transports.AddNew();
			var sydney = declaration.Transports.AddNew();
			sanJuan.JW_RL_NKDiscPort = "PRSJU";
			mexico.JW_RL_NKDiscPort = "MXMEX";
			sauPaulo.JW_RL_NKDiscPort = "BRSAO";
			sydney.JW_RL_NKDiscPort = "AUSYD";

			declaration.JE_MessageSubType = "IM";
			declaration.ResumeApportionment();

			return entryHeader;
		}

		public static void CreateProcedures(BusinessObjectFactory factory, IEnumerable<ZString> codes)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			foreach (var c in codes)
			{
				helper.CreateOrFindExistingRefCusProcedure("CDS", "X", c.Left(2), c.SubstringSafe(2, 2), c.Right(3), "Test " + c, "IMP", ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse);
			}
		}

		void PrepareUniversalData_ToTestMergeOperation()
		{
			const string countryCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			_ = helper.CreateNewOrGetExistingDataGrouping(countryCode, "To test Merge operation");

			const string ADDIN = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation;
			const string IMPORT = "IMPORT";
			const string EXPORT = "EXPORT";
			const string HEADER = "HEADER";
			const string ITEM = "ITEM";

			helper.CreateNewOrGetExistingCusCodeType(ADDIN, "AdditionalInformation");

			CreateAdditionalInfo(helper, countryCode, "IMH01", IMPORT, HEADER);
			CreateAdditionalInfo(helper, countryCode, "IMH02", IMPORT, HEADER);
			CreateAdditionalInfo(helper, countryCode, "IMH03", IMPORT, HEADER);
			CreateAdditionalInfo(helper, countryCode, "IMI01", IMPORT, ITEM);
			CreateAdditionalInfo(helper, countryCode, "IMI02", IMPORT, ITEM);
			CreateAdditionalInfo(helper, countryCode, "IMI03", IMPORT, ITEM);

			CreateAdditionalInfo(helper, countryCode, "EXH01", EXPORT, HEADER);
			CreateAdditionalInfo(helper, countryCode, "EXH02", EXPORT, HEADER);
			CreateAdditionalInfo(helper, countryCode, "EXH03", EXPORT, HEADER);
			CreateAdditionalInfo(helper, countryCode, "EXI01", EXPORT, ITEM);
			CreateAdditionalInfo(helper, countryCode, "EXI02", EXPORT, ITEM);
			CreateAdditionalInfo(helper, countryCode, "EXI03", EXPORT, ITEM);

			CreateAdditionalInfo(helper, countryCode, "00500", IMPORT, ITEM);

			Factory.Save();
		}

		void CreateAdditionalInfo(UniversalReferenceTestDataHelper helper, string countryCode, string code, string directionValue, string levelValue)
		{
			const string ADDIN = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation;
			const string Direction = RefCusCodeListAttributeTypes.Codes.Direction;
			const string Level = RefCusCodeListAttributeTypes.Codes.Level;

			var dtMIN = ZDateTime.MinSmallDateTimeValue;
			var dtMAX = ZDateTime.MaxSmallDateTimeValue;

			var addInfo = helper.CreateCusCodeList(countryCode, ADDIN, code, code + " - Description", dtMIN, dtMAX);
			AddAttributesAndNames(helper, addInfo, new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>(Direction, directionValue),
				new KeyValuePair<string, string>(Level, levelValue),
			});
		}

		JobDeclaration CreateJobDeclaration_ToTestMergeOperation(string messageType, string applicationCode, int invLinesOnInvoice1, int invLinesOnInvoice2 = 0, int invLinesOnInvoice3 = 0)
		{
			Assert("From 0 to 3 invoice lines are allowed.", (invLinesOnInvoice1 >= 0) && (invLinesOnInvoice1 <= 3));
			Assert("From 0 to 3 invoice lines are allowed.", (invLinesOnInvoice2 >= 0) && (invLinesOnInvoice2 <= 3));
			Assert("From 0 to 3 invoice lines are allowed.", (invLinesOnInvoice3 >= 0) && (invLinesOnInvoice3 <= 3));

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = messageType;
			dec.JE_ApplicationCode = applicationCode;
			dec.JE_TotalNoOfPacks = 5;
			dec.JE_MasterBill = "MB111111112";
			dec.JE_VesselName = "TITANIC";

			JobComInvoiceHeader invoice = null;
			JobComInvoiceLine invLine;
			PreviousDocument prevDoc;

			if (invLinesOnInvoice1 > 0)
			{
				invoice = dec.Invoices.AddNew();
				invoice.ZG_HouseSplitReference = "AA";

				invLine = invoice.InvoiceLines.AddNew();
				invLine.JI_Procedure = "P11";
				invLine.JI_Description = "Invoice line desc 1-1";
				invLine.JI_LinePrice = 11;
				invLine.JI_CustomsQuantity = 11;
				invLine.JI_CustomsUnitQty = "KGM";
				invLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;

				prevDoc = invLine.PreviousDocuments.AddNew();
				prevDoc.CSI_Code = "110";
				prevDoc.CSI_SubType = "Z";
				prevDoc.CSI_ReferenceNumber = "PrevDoc";

				if (invLinesOnInvoice1 > 1)
				{
					invLine = invoice.InvoiceLines.AddNew();
					invLine.JI_Procedure = "P12";
					invLine.JI_Description = "Invoice line desc 1-2";
					invLine.JI_LinePrice = 12;
					invLine.JI_CustomsQuantity = 12;
					invLine.JI_CustomsUnitQty = "KGM";
					invLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;

					prevDoc = invLine.PreviousDocuments.AddNew();
					prevDoc.CSI_Code = "120";
					prevDoc.CSI_SubType = "Z";
					prevDoc.CSI_ReferenceNumber = "PrevDoc";

					if (invLinesOnInvoice1 > 2)
					{
						invLine = invoice.InvoiceLines.AddNew();
						invLine.JI_Procedure = "P13";
						invLine.JI_Description = "Invoice line desc 1-3";
						invLine.JI_LinePrice = 13;
						invLine.JI_CustomsQuantity = 13;
						invLine.JI_CustomsUnitQty = "KGM";
						invLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;

						prevDoc = invLine.PreviousDocuments.AddNew();
						prevDoc.CSI_Code = "130";
						prevDoc.CSI_SubType = "Z";
						prevDoc.CSI_ReferenceNumber = "PrevDoc";
					}
				}
			}

			if (invLinesOnInvoice2 > 0)
			{
				invoice = dec.Invoices.AddNew();
				invoice.ZG_HouseSplitReference = "BB";

				invLine = invoice.InvoiceLines.AddNew();
				invLine.JI_Procedure = "P21";
				invLine.JI_Description = "Invoice line desc 2-1";
				invLine.JI_LinePrice = 24;
				invLine.JI_CustomsQuantity = 24;
				invLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;

				prevDoc = invLine.PreviousDocuments.AddNew();
				prevDoc.CSI_Code = "210";
				prevDoc.CSI_SubType = "Z";
				prevDoc.CSI_ReferenceNumber = "PrevDoc";

				if (invLinesOnInvoice2 > 1)
				{
					invLine = invoice.InvoiceLines.AddNew();
					invLine.JI_Procedure = "P22";
					invLine.JI_Description = "Invoice line desc 2-2";
					invLine.JI_LinePrice = 25;
					invLine.JI_CustomsQuantity = 25;
					invLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;

					prevDoc = invLine.PreviousDocuments.AddNew();
					prevDoc.CSI_Code = "220";
					prevDoc.CSI_SubType = "Z";
					prevDoc.CSI_ReferenceNumber = "PrevDoc";

					if (invLinesOnInvoice2 > 2)
					{
						invLine = invoice.InvoiceLines.AddNew();
						invLine.JI_Procedure = "P23";
						invLine.JI_Description = "Invoice line desc 2-3";
						invLine.JI_LinePrice = 26;
						invLine.JI_CustomsQuantity = 26;
						invLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;

						prevDoc = invLine.PreviousDocuments.AddNew();
						prevDoc.CSI_Code = "230";
						prevDoc.CSI_SubType = "Z";
						prevDoc.CSI_ReferenceNumber = "PrevDoc";
					}
				}
			}

			if (invLinesOnInvoice3 > 0)
			{
				invoice = dec.Invoices.AddNew();
				invoice.ZG_HouseSplitReference = "CC";

				invLine = invoice.InvoiceLines.AddNew();
				invLine.JI_Procedure = "P31";
				invLine.JI_Description = "Invoice line desc 3-1";
				invLine.JI_LinePrice = 37;
				invLine.JI_CustomsQuantity = 37;
				invLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;

				prevDoc = invLine.PreviousDocuments.AddNew();
				prevDoc.CSI_Code = "310";
				prevDoc.CSI_SubType = "Z";
				prevDoc.CSI_ReferenceNumber = "PrevDoc";

				if (invLinesOnInvoice3 > 1)
				{
					invLine = invoice.InvoiceLines.AddNew();
					invLine.JI_Procedure = "P32";
					invLine.JI_Description = "Invoice line desc 3-2";
					invLine.JI_LinePrice = 38;
					invLine.JI_CustomsQuantity = 38;
					invLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;

					prevDoc = invLine.PreviousDocuments.AddNew();
					prevDoc.CSI_Code = "320";
					prevDoc.CSI_SubType = "Z";
					prevDoc.CSI_ReferenceNumber = "PrevDoc";

					if (invLinesOnInvoice3 > 2)
					{
						invLine = invoice.InvoiceLines.AddNew();
						invLine.JI_Procedure = "P33";
						invLine.JI_Description = "Invoice line desc 3-3";
						invLine.JI_LinePrice = 39;
						invLine.JI_CustomsQuantity = 39;
						invLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;

						prevDoc = invLine.PreviousDocuments.AddNew();
						prevDoc.CSI_Code = "330";
						prevDoc.CSI_SubType = "Z";
						prevDoc.CSI_ReferenceNumber = "PrevDoc";
					}
				}
			}

			return dec;
		}

		JobDeclaration PrepareMergeScenario_01_ToTestMergeOperation()
		{
			const string CDS = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			const string IMP = MessageTypeList.Codes.Import;

			var dec = CreateJobDeclaration_ToTestMergeOperation(IMP, CDS, 2);

			var invoice = dec.Invoices[0];
			invoice.AdditionalInfos.Add(CreateAdditionalInfo("IMH01"));

			var invLine = invoice.InvoiceLines[0];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("IMI01"));

			return dec;
		}

		JobDeclaration PrepareMergeScenario_02_ToTestMergeOperation()
		{
			const string CDS = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			const string IMP = MessageTypeList.Codes.Import;

			var dec = CreateJobDeclaration_ToTestMergeOperation(IMP, CDS, 2, 3);

			dec.AdditionalInfos.Add(CreateAdditionalInfo("IMH01"));
			dec.AdditionalInfos.Add(CreateAdditionalInfo("IMI01"));

			var invoice = dec.Invoices[0];
			invoice.AdditionalInfos.Add(CreateAdditionalInfo("IMH02"));

			var invLine = invoice.InvoiceLines[0];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("IMI02"));

			invLine = invoice.InvoiceLines[1];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("IMI03"));

			invoice = dec.Invoices[1];
			invoice.AdditionalInfos.Add(CreateAdditionalInfo("IMH03"));

			invLine = invoice.InvoiceLines[2];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("IMI01"));
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("IMI02"));

			return dec;
		}

		JobDeclaration PrepareMergeScenario_03_ToTestMergeOperation()
		{
			const string CDS = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			const string EXP = MessageTypeList.Codes.Export;

			var dec = CreateJobDeclaration_ToTestMergeOperation(EXP, CDS, 1, 2, 3);
			dec.AdditionalInfos.Add(CreateAdditionalInfo("EXH03"));

			var invoice = dec.Invoices[0];
			invoice.AdditionalInfos.Add(CreateAdditionalInfo("EXH01"));

			var invLine = invoice.InvoiceLines[0];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("EXI02"));

			invoice = dec.Invoices[1];
			invoice.AdditionalInfos.Add(CreateAdditionalInfo("EXH02"));

			invLine = invoice.InvoiceLines[0];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("EXI01"));
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("EXI03"));

			invLine = invoice.InvoiceLines[1];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("EXI02"));

			invoice = dec.Invoices[2];
			invoice.AdditionalInfos.Add(CreateAdditionalInfo("EXH03"));

			invLine = invoice.InvoiceLines[0];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("EXI02"));

			invLine = invoice.InvoiceLines[1];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("EXI03"));

			invLine = invoice.InvoiceLines[2];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("EXI02"));
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("EXI01"));

			return dec;
		}

		JobDeclaration PrepareMergeScenario_04_ToTestMergeOperation()
		{
			const string CDS = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			const string IMP = MessageTypeList.Codes.Import;

			var dec = CreateJobDeclaration_ToTestMergeOperation(IMP, CDS, 2);

			var invoice = dec.Invoices[0];
			var invLine = invoice.InvoiceLines[0];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("IMI02"));

			invLine = invoice.InvoiceLines[1];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("IMI01"));

			return dec;
		}

		AdditionalInfo CreateAdditionalInfo(string code)
		{
			var addInfo = Factory.New<AdditionalInfo>();
			addInfo.CSI_Code = code;
			return addInfo;
		}

		void AddAttributesAndNames(UniversalReferenceTestDataHelper helper, Universal.RefCusCodeList cusCodeList, params KeyValuePair<string, string>[] namesAndValues)
		{
			foreach (var nameValue in namesAndValues)
			{
				var name = nameValue.Key;
				var value = nameValue.Value;
				var key = string.Join("|", name.ToUpper(), cusCodeList.ZZD_ZZK_NKCodeType, cusCodeList.ZZD_ZZZ_NKDataGrouping);

				if (!ExistingNames.Contains(key))
				{
					ExistingNames.Add(key);
					helper.CreateNewOrGetExistingRefCusCodeListAttributeName(name, value, cusCodeList.ZZD_ZZK_NKCodeType, cusCodeList.ZZD_ZZZ_NKDataGrouping);
				}
				cusCodeList.Attributes.AddNew(name, value);
			}
		}

		public void TestAgentRepresentation_NoRepresentative_ButHasARepresentativeDocAddress()
		{
			var orgA = CreateRepresentativeForAgentRepresentationTests("MY CORP PTY", "MYCORP", "21 Long str", "Buckingham", "London", "England", "1234", true);
			var addressA = orgA.Addresses[0];
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var declaration = entryHeader.Declaration;
			declaration.RepresentativeDocAddress.OrganisationPK = orgA.PK;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;

			AssertEquals("pre-req", RepresentationTypeList.Codes._2Direct, entryHeader.Declaration.JE_DeclarantType);
			AssertEquals("pre-req", ZGuid.Empty, declaration.JE_OA_Representative);

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLContains(@"<Agent>
      <FunctionCode>2</FunctionCode>
    </Agent>", xml);
		}

		public void TestAgentRepresentation_WithEORI()
		{
			var orgA = CreateRepresentativeForAgentRepresentationTests("MY CORP PTY", "MYCORP", "21 Long str", "Buckingham", "London", "England", "1234", true);
			var addressA = orgA.Addresses[0];
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var declaration = entryHeader.Declaration;
			declaration.JE_OA_Representative = addressA.PK;
			declaration.RepresentativeDocAddress.OrganisationPK = orgA.PK;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;

			AssertEquals("pre-req", RepresentationTypeList.Codes._2Direct, entryHeader.Declaration.JE_DeclarantType);
			AssertEquals("pre-req", "GBEORI12", orgA.GetEuIdentificationNumber());

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLContains(@"<Agent>
      <ID>GBEORI12</ID>
      <FunctionCode>2</FunctionCode>
    </Agent>", xml);
		}

		public void TestAgentRepresentation_NoEORI()
		{
			var orgA = CreateRepresentativeForAgentRepresentationTests("MY CORP PTY", "MYCORP", "21 Long str", "Buckingham", "London", "England", "1234");
			var addressA = orgA.Addresses[0];
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var declaration = entryHeader.Declaration;
			declaration.JE_OA_Representative = addressA.PK;
			declaration.RepresentativeDocAddress.OrganisationPK = orgA.PK;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;

			AssertEquals("pre-req", RepresentationTypeList.Codes._2Direct, entryHeader.Declaration.JE_DeclarantType);
			AssertEquals("pre-req", "", orgA.GetEuIdentificationNumber());

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertXMLContains(@"<Agent>
      <Name>MY CORP PTY</Name>
      <FunctionCode>2</FunctionCode>
      <Address>
        <CityName>London</CityName>
        <CountryCode>GB</CountryCode>
        <Line>21 Long str Buckingham</Line>
        <PostcodeID>1234</PostcodeID>
      </Address>
    </Agent>", xml);
		}

		public void TestAgentRepresentation_NoDeclarantType()
		{
			var orgA = CreateRepresentativeForAgentRepresentationTests("MY CORP PTY", "MYCORP", "21 Long str", "Buckingham", "London", "England", "1234", true);
			var addressA = orgA.Addresses[0];
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var declaration = entryHeader.Declaration;
			declaration.JE_OA_Representative = addressA.PK;
			declaration.RepresentativeDocAddress.OrganisationPK = orgA.PK;
			declaration.JE_DeclarantType = "";

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			AssertNotContains(@"<Agent>", xml);
		}

		public void TestMessageBuilder_CleansAndTrimsFieldsToSchemaLength_Import()
		{
			var entryHeader = CreateSampleEntryHeaderForH1(Factory);
			var declaration = entryHeader.Declaration;

			var decSuppDoc = declaration.SupportingDocuments.AddNew();
			decSuppDoc.CSI_Code = "C514";
			decSuppDoc.CSI_ReferenceNumber = "Long Additional Document ID with\r\nline breaks";
			decSuppDoc.CSI_Description = "Long Additional Document Name with\r\nline breaks";
			decSuppDoc.CSI_ReferenceNumber2 = "Long Additional Document Submitter Name with\r\nline breaks\r\n";

			entryHeader.Declaration.Bills[1].PackingGroups[0].Packages[1].CW_MarksAndNos = new string('A', 255) + "\r\n\r\n" + new string('A', 255) + "Z";

			EU.Business.Declaration.MultiLineAddInfos.PreviousDocument previousDocument1 = entryHeader.InvoiceHeaders.FirstOrDefault()?.InvoiceLines[0].PreviousDocuments.AddNew();
			previousDocument1.CSI_Code = "703";
			previousDocument1.CSI_SubType = "Z";
			previousDocument1.CSI_ReferenceNumber = "Long Previous Document ID with\r\nline\r\n breaks";

			declaration.CusContainers[0].CO_ContainerNumber = "CNT\r\n1234567898765";
			declaration.CusContainers[0].CO_Seal = "SealNo\r\n1234";
			declaration.CusContainers[0].CO_SecondSeal = "SecondSealNo\r\n1234";
			declaration.JE_VesselName = "Transport Means ID with\r\nbreaks";

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.New());
			var xml = messageBuilder.Build();

			CombineAssertions(() =>
			{
				AssertXMLContains(@"<ID>Long Additional Document ID with li</ID>", xml);
				AssertXMLContains(@"<Name>Long Additional Document Name with </Name>", xml);
				AssertXMLContains(@"<Name>Long Additional Document Submitter Name with line breaks</Name>", xml);
				AssertXMLContains(@"<MarksNumbersID>" + new string('A', 255) + "  " + new string('A', 255) + "</MarksNumbersID>", xml);
				AssertXMLContains(@"<ID>Long Previous Document ID with lin</ID>", xml);
				AssertXMLContains(@"<ID>CNT 1234567898765</ID>", xml);
				AssertXMLContains(@"<ID>Transport Means ID with bre</ID>", xml);
			});
		}

		public void TestMessageBuilder_CleansAndTrimsFieldsToSchemaLength_Export()
		{
			const string B1 = ExportDeclarationTypeList.Codes.DeclarationForExport;
			var entryHeader = GetEntryHeader_ForExportsMessageTesting(B1);
			entryHeader.Declaration.CusContainers[0].CO_Seal = "SealNo\r\n1234";
			entryHeader.Declaration.CusContainers[0].CO_SecondSeal = "SecondSealNo\r\n1234";

			var xml = GetXmlMessage_ForExportsMessageTesting(entryHeader);

			CombineAssertions(() =>
			{
				AssertXMLContains("<ID>SEALNO 1234</ID>", xml);
				AssertXMLContains("<ID>SECONDSEALNO 1234</ID>", xml);
			});
		}

		void SetDataForTotalPackageQuantityTesting(CusEntryHeader entryHeader, int totalPackageQuantity, bool isMovementThroughAnInventoryLocation)
		{
			entryHeader.Declaration.CustomsEntryInstructions[0].CEI_PackageCount = totalPackageQuantity;
			if (isMovementThroughAnInventoryLocation)
			{
				entryHeader.CH_MasterUCR = "1234567890";
			}
			else
			{
				entryHeader.CH_MasterUCR = "";
			}
		}

		HashSet<string> ExistingNames => existingNames ?? (existingNames = new HashSet<string>());
		HashSet<string> existingNames;
	}
}

