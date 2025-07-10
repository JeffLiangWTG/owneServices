using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.CDS.Messaging.Wrappers;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class CDSInventoryLinkingRequestMessageBuilderTests : TestCaseWithFactory
	{
		[TestDate(2018, 6, 7)]
		public void TestNewInventoryLinkingRequest()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Port");
			var codelist = helper.CreateNewOrGetExistingCusCodeList(Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "LOCLOCSHE", "LOCLOCSHE", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeListAttribute(codelist.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Facility, "AU");
			// Do not add qualifier QF, thus the CcsUkToCdsLocationConverter will give "GBAU LOCLOCSHE" and we put this into the message as "GBAULOCLOCSHE"
			Factory.Save();

			var str = ZDateTime.Now.ToString("o");
			var timeZonePostfix = str.Substring(str.IndexOf("+", StringComparison.OrdinalIgnoreCase));

			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings { CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields });
			var consol = GbCDSConsolIntegrationWrapperTests.CreateSampleWrapper(Factory);
			var entry = GbCDSExportEntryHeaderWrapperTests.CreateSampleEntryHeader(Factory);
			var entryProvider = new GbCDSExportEntryHeaderWrapper(entry);
			var consolProvider = new CdsExportConsolIntegrationWrapperToIUkCinvWrapper(new CustomsExportConsolIntegrationWrapper(consol, null));

			CombineAssertions(() =>
			{
				AssertXMLEquals("entry/MucrAssociate", @"<inventoryLinkingConsolidationRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
  <messageCode>EAC</messageCode>
  <masterUCR>MUCR</masterUCR>
  <ucrBlock>
    <ucr>DUCR</ucr>
    <ucrPartNo>1</ucrPartNo>
    <ucrType>D</ucrType>
  </ucrBlock>
</inventoryLinkingConsolidationRequest>"
				, ReplacePlaceHolder(CDSInventoryLinkingRequestMessageBuilder.NewMessageText(entryProvider, new GbDes242MessageFunction.MucrAssociate()), entry));

				AssertXMLEquals("entry/MucrDisAssociate", @"<inventoryLinkingConsolidationRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
  <messageCode>EAC</messageCode>
  <ucrBlock>
    <ucr>DUCR</ucr>
    <ucrPartNo>1</ucrPartNo>
    <ucrType>D</ucrType>
  </ucrBlock>
</inventoryLinkingConsolidationRequest>"
				, ReplacePlaceHolder(CDSInventoryLinkingRequestMessageBuilder.NewMessageText(entryProvider, new GbDes242MessageFunction.MucrDisAssociate()), entry));

				AssertXMLEquals("entry/MucrClose", @"<inventoryLinkingConsolidationRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
  <messageCode>CST</messageCode>
  <masterUCR>MUCR</masterUCR>
</inventoryLinkingConsolidationRequest>"
					, CDSInventoryLinkingRequestMessageBuilder.NewMessageText(entryProvider, new GbDes242MessageFunction.MucrClose()));

				AssertXMLEquals("entry/ArrivalActual/Declaration", @"<inventoryLinkingMovementRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
  <messageCode>EAL</messageCode>
  <ucrBlock>
    <ucr>DUCR</ucr>
    <ucrPartNo>1</ucrPartNo>
    <ucrType>D</ucrType>
  </ucrBlock>
  <goodsLocation>GBAUQFLOCATION</goodsLocation>
  <goodsArrivalDateTime>2018-06-06T00:00:00+08:00</goodsArrivalDateTime>
  <shedOPID>SUB</shedOPID>
  <masterUCR xsi:nil=""true"" />
  <movementReference>06Jun0000</movementReference>
  <transportDetails>
    <transportID>Flight</transportID>
    <transportMode>4</transportMode>
    <transportNationality>GB</transportNationality>
  </transportDetails>
</inventoryLinkingMovementRequest>".Replace("+08:00", timeZonePostfix)
					, ReplacePlaceHolder(CDSInventoryLinkingRequestMessageBuilder.NewMessageText(entryProvider, new GbInventoryManagementMessageFunction.ArrivalActual(GbInventoryManagementMessageFunction.MasterOrDeclaration.Declaration)), entry));

				AssertXMLEquals("entry/ArrivalAnticipated/Declaration", @"<inventoryLinkingMovementRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
  <messageCode>EAA</messageCode>
  <ucrBlock>
    <ucr>DUCR</ucr>
    <ucrPartNo>1</ucrPartNo>
    <ucrType>D</ucrType>
  </ucrBlock>
  <goodsLocation>GBAUQFLOCATION</goodsLocation>
  <shedOPID>SUB</shedOPID>
  <masterUCR xsi:nil=""true"" />
  <movementReference>06Jun0000</movementReference>
  <transportDetails>
    <transportID>Flight</transportID>
    <transportMode>4</transportMode>
    <transportNationality>GB</transportNationality>
  </transportDetails>
</inventoryLinkingMovementRequest>".Replace("+08:00", timeZonePostfix)
					, ReplacePlaceHolder(CDSInventoryLinkingRequestMessageBuilder.NewMessageText(entryProvider, new GbInventoryManagementMessageFunction.ArrivalAnticipated(GbInventoryManagementMessageFunction.MasterOrDeclaration.Declaration)), entry));

				AssertXMLEquals("entry/Departure/Declaration", @"<inventoryLinkingMovementRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
  <messageCode>EDL</messageCode>
  <ucrBlock>
    <ucr>DUCR</ucr>
    <ucrPartNo>1</ucrPartNo>
    <ucrType>D</ucrType>
  </ucrBlock>
  <goodsLocation>GBAUQFLOCATION</goodsLocation>
  <goodsDepartureDateTime>2018-06-07T00:00:00+08:00</goodsDepartureDateTime>
  <shedOPID>SUB</shedOPID>
  <masterUCR xsi:nil=""true"" />
  <movementReference>06Jun0000</movementReference>
  <transportDetails>
    <transportID>Flight</transportID>
    <transportMode>4</transportMode>
    <transportNationality>GB</transportNationality>
  </transportDetails>
</inventoryLinkingMovementRequest>".Replace("+08:00", timeZonePostfix)
					, ReplacePlaceHolder(CDSInventoryLinkingRequestMessageBuilder.NewMessageText(entryProvider, new GbInventoryManagementMessageFunction.Departure(GbInventoryManagementMessageFunction.MasterOrDeclaration.Declaration)), entry));

				AssertXMLEquals("entry/ArrivalActual/Master", @"<inventoryLinkingMovementRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
  <messageCode>EAL</messageCode>
  <ucrBlock>
    <ucr>MUCR</ucr>
    <ucrType>M</ucrType>
  </ucrBlock>
  <goodsLocation>GBAUQFLOCATION</goodsLocation>
  <goodsArrivalDateTime>2018-06-06T00:00:00+08:00</goodsArrivalDateTime>
  <shedOPID>SUB</shedOPID>
  <masterUCR>MUCR</masterUCR>
  <movementReference>06Jun0000</movementReference>
  <transportDetails>
    <transportID>Flight</transportID>
    <transportMode>4</transportMode>
    <transportNationality>GB</transportNationality>
  </transportDetails>
</inventoryLinkingMovementRequest>".Replace("+08:00", timeZonePostfix)
				, CDSInventoryLinkingRequestMessageBuilder.NewMessageText(entryProvider, new GbInventoryManagementMessageFunction.ArrivalActual(GbInventoryManagementMessageFunction.MasterOrDeclaration.Master)));

				AssertXMLEquals("entry/ArrivalAnticipated/Master", @"<inventoryLinkingMovementRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
  <messageCode>EAA</messageCode>
  <ucrBlock>
    <ucr>MUCR</ucr>
    <ucrType>M</ucrType>
  </ucrBlock>
  <goodsLocation>GBAUQFLOCATION</goodsLocation>
  <shedOPID>SUB</shedOPID>
  <masterUCR>MUCR</masterUCR>
  <movementReference>06Jun0000</movementReference>
  <transportDetails>
    <transportID>Flight</transportID>
    <transportMode>4</transportMode>
    <transportNationality>GB</transportNationality>
  </transportDetails>
</inventoryLinkingMovementRequest>".Replace("+08:00", timeZonePostfix)
				, CDSInventoryLinkingRequestMessageBuilder.NewMessageText(entryProvider, new GbInventoryManagementMessageFunction.ArrivalAnticipated(GbInventoryManagementMessageFunction.MasterOrDeclaration.Master)));

				AssertXMLEquals("entry/Departure/Master", @"<inventoryLinkingMovementRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
  <messageCode>EDL</messageCode>
  <ucrBlock>
    <ucr>MUCR</ucr>
    <ucrType>M</ucrType>
  </ucrBlock>
  <goodsLocation>GBAUQFLOCATION</goodsLocation>
  <goodsDepartureDateTime>2018-06-07T00:00:00+08:00</goodsDepartureDateTime>
  <shedOPID>SUB</shedOPID>
  <masterUCR>MUCR</masterUCR>
  <movementReference>06Jun0000</movementReference>
  <transportDetails>
    <transportID>Flight</transportID>
    <transportMode>4</transportMode>
    <transportNationality>GB</transportNationality>
  </transportDetails>
</inventoryLinkingMovementRequest>".Replace("+08:00", timeZonePostfix)
					, CDSInventoryLinkingRequestMessageBuilder.NewMessageText(entryProvider, new GbInventoryManagementMessageFunction.Departure(GbInventoryManagementMessageFunction.MasterOrDeclaration.Master)));

				AssertXMLEquals("entry/QueryMasterDEC", @"<inventoryLinkingQueryRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
  <queryUCR>
    <ucr>MUCR</ucr>
    <ucrType>M</ucrType>
  </queryUCR>
</inventoryLinkingQueryRequest>"
				, CDSInventoryLinkingRequestMessageBuilder.NewMessageText(entryProvider, new GbDes242MessageFunction.QueryMasterDEC()));

				AssertXMLEquals("consol/MucrAssociate", @"<inventoryLinkingConsolidationRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
  <messageCode>EAC</messageCode>
  <masterUCR>A:MUCR1234567</masterUCR>
  <ucrBlock>
    <ucr />
    <ucrType>D</ucrType>
  </ucrBlock>
</inventoryLinkingConsolidationRequest>"
					, CDSInventoryLinkingRequestMessageBuilder.NewMessageText(consolProvider, new GbDes242MessageFunction.MucrAssociate()));

				AssertXMLEquals("consol/MucrAssociate", @"<inventoryLinkingConsolidationRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
  <messageCode>EAC</messageCode>
  <masterUCR>A:MUCR1234567</masterUCR>
  <ucrBlock>
    <ucr>UCR-123</ucr>
    <ucrPartNo>456</ucrPartNo>
    <ucrType>D</ucrType>
  </ucrBlock>
</inventoryLinkingConsolidationRequest>"
					, CDSInventoryLinkingRequestMessageBuilder.NewMessageText(consolProvider, new GbDes242MessageFunction.MucrAssociate { ChildUCRToBeAddedToMasterUCR = "UCR-123/456" }));

				AssertXMLEquals("consol/MucrDisAssociate", @"<inventoryLinkingConsolidationRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
  <messageCode>EAC</messageCode>
  <ucrBlock>
    <ucr />
    <ucrType>D</ucrType>
  </ucrBlock>
</inventoryLinkingConsolidationRequest>"
					, CDSInventoryLinkingRequestMessageBuilder.NewMessageText(consolProvider, new GbDes242MessageFunction.MucrDisAssociate()));

				AssertXMLEquals("consol/MucrDisAssociate", @"<inventoryLinkingConsolidationRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
  <messageCode>EAC</messageCode>
  <ucrBlock>
    <ucr>UCR-123</ucr>
    <ucrPartNo>456</ucrPartNo>
    <ucrType>D</ucrType>
  </ucrBlock>
</inventoryLinkingConsolidationRequest>"
					, CDSInventoryLinkingRequestMessageBuilder.NewMessageText(consolProvider, new GbDes242MessageFunction.MucrDisAssociate { ChildUCRToBeAddedToMasterUCR = "UCR-123/456" }));

				AssertXMLEquals("consol/MucrClose", @"<inventoryLinkingConsolidationRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
  <messageCode>CST</messageCode>
  <masterUCR>A:MUCR1234567</masterUCR>
</inventoryLinkingConsolidationRequest>"
					, CDSInventoryLinkingRequestMessageBuilder.NewMessageText(consolProvider, new GbDes242MessageFunction.MucrClose()));

				AssertXMLEquals("consol/ArrivalActual/Declaration", @"<inventoryLinkingMovementRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
  <messageCode>EAL</messageCode>
  <ucrBlock>
    <ucr />
    <ucrType>D</ucrType>
  </ucrBlock>
  <goodsLocation>GBAULOCLOCSHE</goodsLocation>
  <goodsArrivalDateTime>2018-06-06T00:00:00+08:00</goodsArrivalDateTime>
  <shedOPID>She</shedOPID>
  <masterUCR xsi:nil=""true"" />
  <masterOpt>X</masterOpt>
  <movementReference>06Jun0000PART</movementReference>
  <transportDetails>
    <transportID>TransportId</transportID>
    <transportMode>4</transportMode>
    <transportNationality>GB</transportNationality>
  </transportDetails>
</inventoryLinkingMovementRequest>".Replace("+08:00", timeZonePostfix)
					, CDSInventoryLinkingRequestMessageBuilder.NewMessageText(consolProvider, new GbInventoryManagementMessageFunction.ArrivalActual(GbInventoryManagementMessageFunction.MasterOrDeclaration.Declaration)));

				AssertXMLEquals("consol/ArrivalAnticipated/Declaration", @"<inventoryLinkingMovementRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
  <messageCode>EAA</messageCode>
  <ucrBlock>
    <ucr />
    <ucrType>D</ucrType>
  </ucrBlock>
  <goodsLocation>GBAULOCLOCSHE</goodsLocation>
  <shedOPID>She</shedOPID>
  <masterUCR xsi:nil=""true"" />
  <masterOpt>X</masterOpt>
  <movementReference>06Jun0000PART</movementReference>
  <transportDetails>
    <transportID>TransportId</transportID>
    <transportMode>4</transportMode>
    <transportNationality>GB</transportNationality>
  </transportDetails>
</inventoryLinkingMovementRequest>".Replace("+08:00", timeZonePostfix)
					, CDSInventoryLinkingRequestMessageBuilder.NewMessageText(consolProvider, new GbInventoryManagementMessageFunction.ArrivalAnticipated(GbInventoryManagementMessageFunction.MasterOrDeclaration.Declaration)));

				AssertXMLEquals("consol/Departure/Declaration", @"<inventoryLinkingMovementRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
  <messageCode>EDL</messageCode>
  <ucrBlock>
    <ucr />
    <ucrType>D</ucrType>
  </ucrBlock>
  <goodsLocation>GBAULOCLOCSHE</goodsLocation>
  <goodsDepartureDateTime>2018-06-06T00:00:00+08:00</goodsDepartureDateTime>
  <shedOPID>She</shedOPID>
  <masterUCR xsi:nil=""true"" />
  <masterOpt>X</masterOpt>
  <movementReference>06Jun0000PART</movementReference>
  <transportDetails>
    <transportID>TransportId</transportID>
    <transportMode>4</transportMode>
    <transportNationality>GB</transportNationality>
  </transportDetails>
</inventoryLinkingMovementRequest>".Replace("+08:00", timeZonePostfix)
					, CDSInventoryLinkingRequestMessageBuilder.NewMessageText(consolProvider, new GbInventoryManagementMessageFunction.Departure(GbInventoryManagementMessageFunction.MasterOrDeclaration.Declaration)));

				AssertXMLEquals("consol/ArrivalActual/Master", @"<inventoryLinkingMovementRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
  <messageCode>EAL</messageCode>
  <ucrBlock>
    <ucr>A:MUCR1234567</ucr>
    <ucrType>M</ucrType>
  </ucrBlock>
  <goodsLocation>GBAULOCLOCSHE</goodsLocation>
  <goodsArrivalDateTime>2018-06-06T00:00:00+08:00</goodsArrivalDateTime>
  <shedOPID>She</shedOPID>
  <masterUCR>A:MUCR1234567</masterUCR>
  <masterOpt>X</masterOpt>
  <movementReference>06Jun0000PART</movementReference>
  <transportDetails>
    <transportID>TransportId</transportID>
    <transportMode>4</transportMode>
    <transportNationality>GB</transportNationality>
  </transportDetails>
</inventoryLinkingMovementRequest>".Replace("+08:00", timeZonePostfix)
					, CDSInventoryLinkingRequestMessageBuilder.NewMessageText(consolProvider, new GbInventoryManagementMessageFunction.ArrivalActual(GbInventoryManagementMessageFunction.MasterOrDeclaration.Master)));

				AssertXMLEquals("consol/ArrivalAnticipated/Master", @"<inventoryLinkingMovementRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
  <messageCode>EAA</messageCode>
  <ucrBlock>
    <ucr>A:MUCR1234567</ucr>
    <ucrType>M</ucrType>
  </ucrBlock>
  <goodsLocation>GBAULOCLOCSHE</goodsLocation>
  <shedOPID>She</shedOPID>
  <masterUCR>A:MUCR1234567</masterUCR>
  <masterOpt>X</masterOpt>
  <movementReference>06Jun0000PART</movementReference>
  <transportDetails>
    <transportID>TransportId</transportID>
    <transportMode>4</transportMode>
    <transportNationality>GB</transportNationality>
  </transportDetails>
</inventoryLinkingMovementRequest>".Replace("+08:00", timeZonePostfix)
				, CDSInventoryLinkingRequestMessageBuilder.NewMessageText(consolProvider, new GbInventoryManagementMessageFunction.ArrivalAnticipated(GbInventoryManagementMessageFunction.MasterOrDeclaration.Master)));

				AssertXMLEquals("consol/Departure/Master", @"<inventoryLinkingMovementRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
  <messageCode>EDL</messageCode>
  <ucrBlock>
    <ucr>A:MUCR1234567</ucr>
    <ucrType>M</ucrType>
  </ucrBlock>
  <goodsLocation>GBAULOCLOCSHE</goodsLocation>
  <goodsDepartureDateTime>2018-06-06T00:00:00+08:00</goodsDepartureDateTime>
  <shedOPID>She</shedOPID>
  <masterUCR>A:MUCR1234567</masterUCR>
  <masterOpt>X</masterOpt>
  <movementReference>06Jun0000PART</movementReference>
  <transportDetails>
    <transportID>TransportId</transportID>
    <transportMode>4</transportMode>
    <transportNationality>GB</transportNationality>
  </transportDetails>
</inventoryLinkingMovementRequest>".Replace("+08:00", timeZonePostfix)
				, CDSInventoryLinkingRequestMessageBuilder.NewMessageText(consolProvider, new GbInventoryManagementMessageFunction.Departure(GbInventoryManagementMessageFunction.MasterOrDeclaration.Master)));

				AssertXMLEquals("consol/QueryMasterDEC", @"<inventoryLinkingQueryRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
  <queryUCR>
    <ucr>A:MUCR1234567</ucr>
    <ucrType>M</ucrType>
  </queryUCR>
</inventoryLinkingQueryRequest>"
				, CDSInventoryLinkingRequestMessageBuilder.NewMessageText(consolProvider, new GbDes242MessageFunction.QueryMasterDEC()));
			});

			entry = GbCDSExportEntryHeaderWrapperTests.CreateSampleEntryHeaderWithBGMReference(Factory, "AR1/999");
			entryProvider = new GbCDSExportEntryHeaderWrapper(entry);

			CombineAssertions(() =>
			{
				AssertXMLEquals("entryWithBGM/MucrAssociate", @"<inventoryLinkingConsolidationRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
  <messageCode>EAC</messageCode>
  <masterUCR>MUCR</masterUCR>
  <ucrBlock>
    <ucr>AR1</ucr>
    <ucrPartNo>999</ucrPartNo>
    <ucrType>D</ucrType>
  </ucrBlock>
</inventoryLinkingConsolidationRequest>"
				, ReplacePlaceHolder(CDSInventoryLinkingRequestMessageBuilder.NewMessageText(entryProvider, new GbDes242MessageFunction.MucrAssociate()), entry));
			});

			entry = GbCDSExportEntryHeaderWrapperTests.CreateSampleEntryHeaderWithBGMReference(Factory, "AR1");
			entryProvider = new GbCDSExportEntryHeaderWrapper(entry);

			CombineAssertions(() =>
			{
				AssertXMLEquals("entry/MucrDisAssociate", @"<inventoryLinkingConsolidationRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
  <messageCode>EAC</messageCode>
  <ucrBlock>
    <ucr>AR1</ucr>
    <ucrType>D</ucrType>
  </ucrBlock>
</inventoryLinkingConsolidationRequest>"
				, ReplacePlaceHolder(CDSInventoryLinkingRequestMessageBuilder.NewMessageText(entryProvider, new GbDes242MessageFunction.MucrDisAssociate()), entry));
			});
		}

		ZString ReplacePlaceHolder(ZString message, CusEntryHeader entry)
		{
			return message.Replace(EU.Business.Declaration.CusEntryHeader.UCRReferencePlaceHolderXmlFriendly, entry.DeclarationUCR)
				.Replace(EU.Business.Declaration.CusEntryHeader.UCRPartPlaceHolderXmlFriendly, entry.DeclarationUCRPartSuffix);
		}
	}
}
