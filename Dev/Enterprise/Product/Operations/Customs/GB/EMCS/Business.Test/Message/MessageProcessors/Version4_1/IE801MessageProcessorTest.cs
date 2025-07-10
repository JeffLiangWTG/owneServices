using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie801;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing.Version4_1
{
	[TestedType(typeof(IE801MessageProcessor))]
	sealed class IE801MessageProcessorTest : IE801MessageProcessorAbstractTest<Ie801Type>
	{
		protected override ZString MessageText => EMCSMessageProcessorTestHelper.GetStandardIE801Text();

		protected override void AssertLineUpdateResult(EMCSJobDeclaration declaration)
		{
			var invoiceLine = declaration.FilteredInvoiceLines[0];
			AssertEquals("JI_LineNo", (ZShort)1, invoiceLine.JI_LineNo);
			AssertEquals("ZG_ExciseProductCode", "W200", invoiceLine.ZG_ExciseProductCode);
			AssertEquals("JI_Tariff", "22084011", invoiceLine.JI_Tariff);
			AssertEquals("ZG_FiscalMarkUsed", true, invoiceLine.ZG_FiscalMarkUsed);
			AssertEquals("ZG_FiscalMark", "FM001", invoiceLine.ZG_FiscalMark);
			AssertEquals("JI_Origin", "CN", invoiceLine.ZG_Origin);
			AssertEquals("JI_NDescription", "CD001", invoiceLine.JI_NDescription);
			AssertEquals("JI_BrandName", "BP001", invoiceLine.JI_BrandName);
			AssertEquals("JI_CustomsQuantity", 2m, invoiceLine.JI_CustomsQuantity);
			AssertEquals("ZG_AddInfo/DeclaredValue", 2m, invoiceLine.ZG_DeclaredValue);
			AssertEquals("JI_Weight", 3m, invoiceLine.JI_Weight);
			AssertEquals("JI_NetWeight", 4m, invoiceLine.JI_NetWeight);
			AssertEquals("ZG_AlcoholicStrength", 5m, invoiceLine.ZG_AlcoholicStrength);
			AssertEquals("ZG_DegreePlato", 6m, invoiceLine.ZG_DegreePlato);
			AssertEquals("ZG_Density", 8m, invoiceLine.ZG_Density);
			AssertEquals("ZG_SizeOfProducer", 7m, invoiceLine.ZG_SizeOfProducer);
			AssertEquals("ZG_GrowingZone", "1", invoiceLine.ZG_GrowingZone);
			AssertEquals("ZG_WineCategory", "1", invoiceLine.ZG_WineCategory);
			AssertEquals("ZG_WineCountryOrigin", "GB", invoiceLine.ZG_WineCountryOrigin);
			AssertEquals("JI_WineDetailsComments", "WPOI001", invoiceLine.JI_WineDetailsComments);
			AssertEquals("OperationCodeData1.CY_Code", "WO001", invoiceLine.OperationCodeDataCollection[0].CY_Code);
			AssertEquals("OperationCodeData2.CY_Code", "WO002", invoiceLine.OperationCodeDataCollection[1].CY_Code);
			AssertEquals("ZG_MaturationPeriodOrAgeOfProducts", "10Months", invoiceLine.ZG_MaturationPeriodOrAgeOfProducts);
			AssertEquals("ZG_IndependentSmallProducersDeclaration", "Independent Small Producers Declaration", invoiceLine.ZG_IndependentSmallProducersDeclaration);

			var package1 = invoiceLine.EMCSPackagePivots.Cast<NonPersistentPackagePivot>().FirstOrDefault(p => p.IsForInvoiceLine);
			AssertEquals("Package 1 UQ", "CT", package1.UnitType);
			AssertEquals("Package 1 Quantity", 5, package1.UnitCount);
			AssertEquals("B5_SealNumber", "SN001", package1.SealNumber);
			AssertEquals("B5_SealComment", "SC001", package1.SealComment);
			AssertEquals("B5_MarksAndNumbers", "Shipping Marks 1", package1.MarksAndNumbers);
			AssertEquals("Linked to the line", true, package1.IsForInvoiceLine);
			AssertEquals("IsMainPack", true, invoiceLine.ZG_IsMainPack);
		}

		public void TestPackagesLinkedCorrectlyWhenNotUnique()
		{
			declaration = Factory.New<EMCSJobDeclaration>();
			declaration.JE_GB = Branch.PK;
			declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
			declaration.JE_DeclarationReference = "E00000810";
			CreateNewMrnNumber(declaration, "MRN1234567", "1");
			outgoingMessage = CreateNewOutgoingMessage();
			declaration.Messages.Add(outgoingMessage);

			var incomingMessage = Factory.New<EMCSInboundEDIMessage>();
			incomingMessage.EM_ApplicationReference = TransactionID;
			incomingMessage.EM_MessageType = MessageType;
			incomingMessage.EM_MessageText = exampleMessage;

			TestEndToEndProcessing(incomingMessage);
			AssertEquals("Pre-requisite: message was processed", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);

			AssertEquals("declaration.InvoiceHeader.InvoiceLines.Count", 5, declaration.InvoiceHeader.InvoiceLines.Count);
			CombineAssertions(() =>
			{
				AssertPackageLink(1, [(8, "BO", string.Empty)]);
				AssertPackageLink(2, [(4, "BO", string.Empty)]);
				AssertPackageLink(3, [(4, "BO", string.Empty)]);
				AssertPackageLink(4, [(4, "BO", "Mk.II")]);
				AssertPackageLink(5, [(4, "CT", string.Empty)]);
			});
		}

		void AssertPackageLink(int sequenceNumber, IEnumerable<(ZLong numberOfPackages, ZString unitType, ZString marks)> packages)
		{
			var invoiceLine = declaration.InvoiceHeader.InvoiceLines.Cast<EMCSJobComInvoiceLine>().SingleOrDefault(x => x.JI_LineNo == sequenceNumber);
			AssertNotNull($"Could not find invoice line with number {sequenceNumber}", invoiceLine);

			var packagePivots = invoiceLine.EMCSPackagePivots.Where(x => x.IsForInvoiceLine).Select(x => (x.Package.B5_UnitCount, x.Package.B5_UnitType, x.Package.B5_MarksAndNumbers));
			AssertContainsExactElementsInAnyOrder($"Line {sequenceNumber} packages", packages, packagePivots);
			AssertEquals($"Line {sequenceNumber} IsMainPack", expected: packages.Any(), invoiceLine.ZG_IsMainPack);
		}

		static readonly string exampleMessage = @"<ie801:IE801 xmlns=""http://www.govtalk.gov.uk/taxation/InternationalTrade/Excise/MovementForTraderData/3"" xmlns:doc=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:DOC:V3.13"" xmlns:emcs=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:EMCS:V3.13"" xmlns:euc=""http://www.govtalk.gov.uk/taxation/InternationalTrade/Excise/EmcsUkCodes/3"" xmlns:ie0=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE880:V3.13"" xmlns:ie1=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE825:V3.13"" xmlns:ie2=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE717:V3.13"" xmlns:ie3=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE815:V3.13"" xmlns:ie=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE934:V3.13"" xmlns:ie704uk=""http://www.govtalk.gov.uk/taxation/InternationalTrade/Excise/ie704uk/3"" xmlns:ie801=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE801:V3.13"" xmlns:ie802=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE802:V3.13"" xmlns:ie803=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE803:V3.13"" xmlns:ie807=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE807:V3.13"" xmlns:ie810=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE810:V3.13"" xmlns:ie813=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE813:V3.13"" xmlns:ie818=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE818:V3.13"" xmlns:ie819=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE819:V3.13"" xmlns:ie829=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE829:V3.13"" xmlns:ie837=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE837:V3.13"" xmlns:ie839=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE839:V3.13"" xmlns:ie840=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE840:V3.13"" xmlns:ie871=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE871:V3.13"" xmlns:ie881=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE881:V3.13"" xmlns:ie905=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE905:V3.13"" xmlns:tcl=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TCL:V3.13"" xmlns:tms=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13"" xmlns:tns4=""http://www.govtalk.gov.uk/taxation/InternationalTrade/Common/ControlDocument"" xmlns:tns5=""http://www.govtalk.gov.uk/taxation/InternationalTrade/Excise/MovementForTraderData/3"" xmlns:tns=""http://www.govtalk.gov.uk/taxation/InternationalTrade/Excise/NewMessagesData/3"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
<ie801:Header>
	<tms:MessageSender>NDEA.IT</tms:MessageSender>
	<tms:MessageRecipient>NDEA.GB</tms:MessageRecipient>
	<tms:DateOfPreparation>2020-01-29</tms:DateOfPreparation>
	<tms:TimeOfPreparation>08:33:29.881</tms:TimeOfPreparation>
	<tms:MessageIdentifier>EF713C93-57CD-42D9-9D7B-951E3BDAD549</tms:MessageIdentifier>
</ie801:Header>
<ie801:Body>
	<ie801:EADESADContainer>
		<ie801:ConsigneeTrader>
			<ie801:Traderid>GB001</ie801:Traderid>
			<ie801:TraderName>Consignee Party 1</ie801:TraderName>
			<ie801:StreetName>CP Address</ie801:StreetName>
			<ie801:Postcode>0001</ie801:Postcode>
			<ie801:City>London</ie801:City>
		</ie801:ConsigneeTrader>
		<ie801:ExciseMovement>
			<ie801:AdministrativeReferenceCode>MRN1234567</ie801:AdministrativeReferenceCode>
			<ie801:DateAndTimeOfValidationOfEadEsad>2020-01-29T08:33:00.0000</ie801:DateAndTimeOfValidationOfEadEsad>
		</ie801:ExciseMovement>
		<ie801:ConsignorTrader>
			<ie801:TraderExciseNumber>GB002</ie801:TraderExciseNumber>
			<ie801:TraderName>Consignor Party 1</ie801:TraderName>
			<ie801:StreetName>CRP Address</ie801:StreetName>
			<ie801:Postcode>0001</ie801:Postcode>
			<ie801:City>London</ie801:City>
		</ie801:ConsignorTrader>
		<ie801:PlaceOfDispatchTrader>
			<ie801:ReferenceOfTaxWarehouse>GBW001</ie801:ReferenceOfTaxWarehouse>
			<ie801:TraderName>PartyPlaceOfDispatch Party 1</ie801:TraderName>
			<ie801:StreetName>PRD Address</ie801:StreetName>
			<ie801:Postcode>0001</ie801:Postcode>
			<ie801:City>London</ie801:City>
		</ie801:PlaceOfDispatchTrader>
		<ie801:CompetentAuthorityDispatchOffice>
			<ie801:ReferenceNumber>IT051999</ie801:ReferenceNumber>
		</ie801:CompetentAuthorityDispatchOffice>
		<ie801:FirstTransporterTrader>
			<ie801:TraderName>FirstTransporter Party 1</ie801:TraderName>
			<ie801:StreetName>FTP Address</ie801:StreetName>
			<ie801:StreetNumber>1</ie801:StreetNumber>
			<ie801:Postcode>0001</ie801:Postcode>
			<ie801:City>London</ie801:City>
		</ie801:FirstTransporterTrader>
		<ie801:EadEsad>
			<ie801:LocalReferenceNumber>B000222547896254786321</ie801:LocalReferenceNumber>
			<ie801:InvoiceNumber>1</ie801:InvoiceNumber>
			<ie801:InvoiceDate>2020-01-29</ie801:InvoiceDate>
			<ie801:OriginTypeCode>1</ie801:OriginTypeCode>
			<ie801:DateOfDispatch>2020-01-29</ie801:DateOfDispatch>
			<ie801:TimeOfDispatch>12:30:00</ie801:TimeOfDispatch>
		</ie801:EadEsad>
		<ie801:HeaderEadEsad>
			<ie801:SequenceNumber>1</ie801:SequenceNumber>
			<ie801:DateAndTimeOfUpdateValidation>2020-01-29T08:33:00.0000</ie801:DateAndTimeOfUpdateValidation>
			<ie801:DestinationTypeCode>4</ie801:DestinationTypeCode>
			<ie801:JourneyTime>H12</ie801:JourneyTime>
			<ie801:TransportArrangement>2</ie801:TransportArrangement>
		</ie801:HeaderEadEsad>
		<ie801:TransportMode>
			<ie801:TransportModeCode>4</ie801:TransportModeCode>
		</ie801:TransportMode>
		<ie801:MovementGuarantee>
			<ie801:GuarantorTypeCode>12</ie801:GuarantorTypeCode>
		</ie801:MovementGuarantee>
		<ie801:BodyEadEsad>
			<ie801:BodyRecordUniqueReference>1</ie801:BodyRecordUniqueReference>
			<ie801:ExciseProductCode>W200</ie801:ExciseProductCode>
			<ie801:CnCode>22042183</ie801:CnCode>
			<ie801:Quantity>6</ie801:Quantity>
			<ie801:GrossMass>8.8</ie801:GrossMass>
			<ie801:NetMass>6</ie801:NetMass>
			<ie801:AlcoholicStrengthByVolumeInPercentage>13</ie801:AlcoholicStrengthByVolumeInPercentage>
			<ie801:FiscalMarkUsedFlag>0</ie801:FiscalMarkUsedFlag>
			<ie801:CommercialDescription>DRINK</ie801:CommercialDescription>
			<ie801:Package>
				<ie801:KindOfPackages>BO</ie801:KindOfPackages>
				<ie801:NumberOfPackages>8</ie801:NumberOfPackages>
			</ie801:Package>
		</ie801:BodyEadEsad>
		<ie801:BodyEadEsad>
			<ie801:BodyRecordUniqueReference>2</ie801:BodyRecordUniqueReference>
			<ie801:ExciseProductCode>W200</ie801:ExciseProductCode>
			<ie801:CnCode>22042183</ie801:CnCode>
			<ie801:Quantity>3</ie801:Quantity>
			<ie801:GrossMass>4.4</ie801:GrossMass>
			<ie801:NetMass>3</ie801:NetMass>
			<ie801:AlcoholicStrengthByVolumeInPercentage>13</ie801:AlcoholicStrengthByVolumeInPercentage>
			<ie801:FiscalMarkUsedFlag>0</ie801:FiscalMarkUsedFlag>
			<ie801:CommercialDescription>DRINK</ie801:CommercialDescription>
			<ie801:Package>
				<ie801:KindOfPackages>BO</ie801:KindOfPackages>
				<ie801:NumberOfPackages>4</ie801:NumberOfPackages>
			</ie801:Package>
		</ie801:BodyEadEsad>
		<ie801:BodyEadEsad>
			<ie801:BodyRecordUniqueReference>3</ie801:BodyRecordUniqueReference>
			<ie801:ExciseProductCode>W200</ie801:ExciseProductCode>
			<ie801:CnCode>22042166</ie801:CnCode>
			<ie801:Quantity>3</ie801:Quantity>
			<ie801:GrossMass>5</ie801:GrossMass>
			<ie801:NetMass>3</ie801:NetMass>
			<ie801:AlcoholicStrengthByVolumeInPercentage>13.5</ie801:AlcoholicStrengthByVolumeInPercentage>
			<ie801:FiscalMarkUsedFlag>0</ie801:FiscalMarkUsedFlag>
			<ie801:CommercialDescription>DRINK</ie801:CommercialDescription>
			<ie801:Package>
				<ie801:KindOfPackages>BO</ie801:KindOfPackages>
				<ie801:NumberOfPackages>4</ie801:NumberOfPackages>
			</ie801:Package>
		</ie801:BodyEadEsad>
		<ie801:BodyEadEsad>
			<ie801:BodyRecordUniqueReference>4</ie801:BodyRecordUniqueReference>
			<ie801:ExciseProductCode>W200</ie801:ExciseProductCode>
			<ie801:CnCode>22042166</ie801:CnCode>
			<ie801:Quantity>3</ie801:Quantity>
			<ie801:GrossMass>5.4</ie801:GrossMass>
			<ie801:NetMass>3</ie801:NetMass>
			<ie801:AlcoholicStrengthByVolumeInPercentage>14</ie801:AlcoholicStrengthByVolumeInPercentage>
			<ie801:FiscalMarkUsedFlag>0</ie801:FiscalMarkUsedFlag>
			<ie801:CommercialDescription>DRINK</ie801:CommercialDescription>
			<ie801:Package>
				<ie801:KindOfPackages>BO</ie801:KindOfPackages>
				<ie801:NumberOfPackages>4</ie801:NumberOfPackages>
				<ie801:ShippingMarks>Mk.II</ie801:ShippingMarks>
			</ie801:Package>
		</ie801:BodyEadEsad>
		<ie801:BodyEadEsad>
			<ie801:BodyRecordUniqueReference>5</ie801:BodyRecordUniqueReference>
			<ie801:ExciseProductCode>W200</ie801:ExciseProductCode>
			<ie801:CnCode>22042166</ie801:CnCode>
			<ie801:Quantity>3</ie801:Quantity>
			<ie801:GrossMass>5.4</ie801:GrossMass>
			<ie801:NetMass>3</ie801:NetMass>
			<ie801:AlcoholicStrengthByVolumeInPercentage>14</ie801:AlcoholicStrengthByVolumeInPercentage>
			<ie801:FiscalMarkUsedFlag>0</ie801:FiscalMarkUsedFlag>
			<ie801:CommercialDescription>DRINK</ie801:CommercialDescription>
			<ie801:Package>
				<ie801:KindOfPackages>CT</ie801:KindOfPackages>
				<ie801:NumberOfPackages>4</ie801:NumberOfPackages>
			</ie801:Package>
		</ie801:BodyEadEsad>
		<ie801:TransportDetails>
			<ie801:TransportUnitCode>2</ie801:TransportUnitCode>
			<ie801:IdentityOfTransportUnits>ABC123</ie801:IdentityOfTransportUnits>
			<ie801:ComplementaryInformation>INFO</ie801:ComplementaryInformation>
		</ie801:TransportDetails>
	</ie801:EADESADContainer>
</ie801:Body>
</ie801:IE801>";
	}
}
