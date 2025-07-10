using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.IL.Manifest.Module.Testing
{
	sealed class ManifestInboundInterchangeImporterTest : TestCaseWithFactory
	{
		public void TestMenuItem_ShouldBeVisible_WhenCurrentUserIsCWSupport()
		{
			CombineAssertions(() =>
			{
				Assert("Prerequisite: Current user is CWSupport by default.", GlbStaff.CurrentUser.IsSupportUser);
				Assert("The action of adding IL Manifest response interchange should be visible when current user is CWSupport.", new ManifestInboundInterchangeImporter(Factory).Precondition());
			});
		}

		public void TestMenuItem_ShouldBeInvisible_WhenCurrentUserIsNotCWSupport()
		{
			GlbStaff.CurrentUser.GS_LoginName = "Dummy";
			CombineAssertions(() =>
			{
				Assert("Prerequisite: Current user is not CWSupport.", !GlbStaff.CurrentUser.IsSupportUser);
				Assert("The action of adding IL Manifest response interchange should be invisible when current user is not CWSupport.", !new ManifestInboundInterchangeImporter(Factory).Precondition());
			});
		}

		public void TestEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked()
		{
			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			using (var stream = resourceRetriever.GetStream("Enterprise.Customs.IL.Manifest.Module.Testing.TestFiles.Manifest_1171.xml"))
			{
				var importerMock = new Mock<ManifestInboundInterchangeImporter>(Factory);
				importerMock.Protected().Setup<Stream>("GetXmlFileStream").Returns(stream);
				var addPntsResponseInterchangeActionMenuItem = (MenuItem)importerMock.Object.GetNewMenuItem();
				addPntsResponseInterchangeActionMenuItem.PerformClick();
				CombineAssertions(() =>
				{
					var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.ILCustoms));
					AssertNotNull("An interchange of the xml uploaded should be created.", interchange);
					AssertEquals("EI_BodyText", expectedInterchangeText, interchange.EI_BodyText);
					AssertEquals("EI_ReceiveTransmit should be RCV.", EDIInterchange.Direction.Receive, interchange.EI_ReceiveTransmit);
					AssertEquals("EI_Status should be QUE.", EDIInterchange.Status.Queued, interchange.EI_Status);
					AssertEquals("EI_InterchangeType should be ILC.", "MAN", interchange.EI_InterchangeType);
					AssertEquals("EI_From should be IL Customs Test", "ILCustomsTEST", interchange.EI_From);
					AssertEquals("EI_To should be currentComany.LicenceKeyIdentifier", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_To);
					AssertEquals("EI_TransportType should be xT", EDIInterchangeTransportTypeList.Codes.xT, interchange.EI_TransportType);
					AssertNotEquals("EI_SessionGUID should not be empty", ZGuid.Empty, interchange.EI_SessionGUID);
				});
			}
		}

		public void TestEDIInterchangeShouldBeSuccessfullyCreated_WhenToolStripItemIsClicked()
		{
			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			using (var stream = resourceRetriever.GetStream("Enterprise.Customs.IL.Manifest.Module.Testing.TestFiles.Manifest_1171.xml"))
			{
				var importerMock = new Mock<ManifestInboundInterchangeImporter>(Factory);
				importerMock.Protected().Setup<Stream>("GetXmlFileStream").Returns(stream);
				var addPntsResponseInterchangeActionMenuItem = (MenuItem)importerMock.Object.GetNewMenuItem();
				var toolStripItem = MenuItemToToolStripItemConverter.ConvertMenuItemToToolStripItem(addPntsResponseInterchangeActionMenuItem);
				toolStripItem.PerformClick();
				CombineAssertions(() =>
				{
					var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.ILCustoms));
					AssertNotNull("An interchange of the xml uploaded should be created.", interchange);
					AssertEquals("EI_BodyText", expectedInterchangeText, interchange.EI_BodyText);
					AssertEquals("EI_ReceiveTransmit should be RCV.", EDIInterchange.Direction.Receive, interchange.EI_ReceiveTransmit);
					AssertEquals("EI_Status should be QUE.", EDIInterchange.Status.Queued, interchange.EI_Status);
					AssertEquals("EI_InterchangeType should be ILC.", "MAN", interchange.EI_InterchangeType);
					AssertEquals("EI_From should be IL Customs Test", "ILCustomsTEST", interchange.EI_From);
					AssertEquals("EI_To should be currentComany.LicenceKeyIdentifier", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_To);
					AssertEquals("EI_TransportType should be xT", EDIInterchangeTransportTypeList.Codes.xT, interchange.EI_TransportType);
					AssertNotEquals("EI_SessionGUID should not be empty", ZGuid.Empty, interchange.EI_SessionGUID);
				});
			}
		}

		readonly string expectedInterchangeText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<MN_MSG4_SendManifestFeedBack_Message xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <ResponseContentHeader xmlns=""http://malam.com/customs/CargoControl/MN_MSG1_MANIFEST"">
    <TransmitionDateTime xmlns=""http://malam.com/customs/EAICommon.xsd"">2024-02-28T12:26:05.3437188+02:00</TransmitionDateTime>
    <ApplicationID xmlns=""http://malam.com/customs/EAICommon.xsd"">1655738</ApplicationID>
    <Remark xsi:nil=""true"" xmlns=""http://malam.com/customs/EAICommon.xsd"" />
  </ResponseContentHeader>
  <Response xmlns=""urn:wco:datamodel:WCO:RES:1"">
    <IssueDateTime>2024-02-28T12:26:05</IssueDateTime>
    <FunctionCode>90</FunctionCode>
    <FunctionalReferenceID>241169</FunctionalReferenceID>
    <Error>
      <ValidationCode listName=""Integrity check failed - container type : G0 in serial : 1 is not the same in sub-transaction : I025307A56 and transaction : I025544300"" listVersionID=""1"" name=""בדיקת שלמות נכשלה - אין התאמה בסוג מכולה : G0. בסידורי : 1 , בעסקת הבן : I025307A56 , מול עסקת אב : I025544300"">4852</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>28A</DocumentSectionCode>
        <SequenceNumeric>6556</SequenceNumeric>
      </Pointer>
    </Error>
    <Error>
      <ValidationCode listName=""Integrity check failed - container type : G0 in serial : 2 is not the same in sub-transaction : I025307A56 and transaction : I025544300"" listVersionID=""1"" name=""בדיקת שלמות נכשלה - אין התאמה בסוג מכולה : G0. בסידורי : 2 , בעסקת הבן : I025307A56 , מול עסקת אב : I025544300"">4852</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>28A</DocumentSectionCode>
        <SequenceNumeric>6556</SequenceNumeric>
      </Pointer>
    </Error>
    <Status>
      <NameCode>1</NameCode>
      <Pointer>
        <DocumentSectionCode>28A</DocumentSectionCode>
        <SequenceNumeric>6556</SequenceNumeric>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>29A</DocumentSectionCode>
        <SequenceNumeric>1</SequenceNumeric>
      </Pointer>
    </Status>
    <Status>
      <NameCode>1</NameCode>
      <Pointer>
        <DocumentSectionCode>28A</DocumentSectionCode>
        <SequenceNumeric>6556</SequenceNumeric>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>29A</DocumentSectionCode>
        <SequenceNumeric>2</SequenceNumeric>
      </Pointer>
    </Status>
    <Status>
      <NameCode>1</NameCode>
      <Pointer>
        <DocumentSectionCode>28A</DocumentSectionCode>
        <SequenceNumeric>6556</SequenceNumeric>
      </Pointer>
    </Status>
    <Declaration xmlns=""urn:wco:datamodel:WCO:CRI:1"">
      <ID>241169</ID>
      <TypeCode>785</TypeCode>
      <AdditionalInformation>
        <StatementCode>1</StatementCode>
        <StatementTypeCode>1</StatementTypeCode>
      </AdditionalInformation>
      <AdditionalInformation>
        <StatementCode>1</StatementCode>
        <StatementTypeCode>1</StatementTypeCode>
      </AdditionalInformation>
      <Consignment>
        <GrossVolumeMeasure>0</GrossVolumeMeasure>
        <SequenceNumeric>6556</SequenceNumeric>
        <TotalPackageQuantity>2</TotalPackageQuantity>
        <AcceptancePlace>
          <Name>KOREA</Name>
        </AcceptancePlace>
        <Consignee>
          <ID>520043027</ID>
          <Name>Elbit Systems Import Department Ltd</Name>
          <Address>
            <CityName>HAIFA</CityName>
            <CountryCode>IL</CountryCode>
            <Line>HAIFA</Line>
            <PostcodeID>3100401</PostcodeID>
          </Address>
          <Communication>
            <ID>04-9951467 ,052-6021960</ID>
            <TypeID>AL</TypeID>
          </Communication>
        </Consignee>
        <ConsignmentItem>
          <GoodsStatusCode>N</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AdditionalInformation>
            <Content>0</Content>
            <StatementTypeCode>11</StatementTypeCode>
          </AdditionalInformation>
          <AdditionalInformation>
            <Content>D5</Content>
            <StatementTypeCode>18</StatementTypeCode>
          </AdditionalInformation>
          <Commodity>
            <CargoDescription>
              STC;
              74 PALLETS
              CONTAINER PA116L
              P.O. . 4F0000284 REV.002
              INV# BK-231228
            </CargoDescription>
            <Classification>
              <ID>7326</ID>
              <IdentificationTypeCode>HS</IdentificationTypeCode>
            </Classification>
          </Commodity>
          <GoodsMeasure>
            <GrossMassMeasure>9205</GrossMassMeasure>
          </GoodsMeasure>
          <GovernmentProcedure>
            <CurrentCode>4000000</CurrentCode>
          </GovernmentProcedure>
          <TransportEquipment>
            <CharacteristicCode>42G0</CharacteristicCode>
            <FullnessCode>5</FullnessCode>
            <ID>TCKU4367675</ID>
            <Seal>
              <SequenceNumeric>1</SequenceNumeric>
              <ID>A4231361778</ID>
            </Seal>
            <SupplierPartyTypeCode>2</SupplierPartyTypeCode>
          </TransportEquipment>
        </ConsignmentItem>
        <ConsignmentItem>
          <GoodsStatusCode>N</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <AdditionalInformation>
            <Content>0</Content>
            <StatementTypeCode>11</StatementTypeCode>
          </AdditionalInformation>
          <AdditionalInformation>
            <Content>D5</Content>
            <StatementTypeCode>18</StatementTypeCode>
          </AdditionalInformation>
          <Commodity>
            <CargoDescription>
              STC;
              74 PALLETS
              CONTAINER PA116L
              P.O. . 4F0000284 REV.002
              INV# BK-231228
            </CargoDescription>
            <Classification>
              <ID>7326</ID>
              <IdentificationTypeCode>HS</IdentificationTypeCode>
            </Classification>
          </Commodity>
          <GoodsMeasure>
            <GrossMassMeasure>9205</GrossMassMeasure>
          </GoodsMeasure>
          <GovernmentProcedure>
            <CurrentCode>4000000</CurrentCode>
          </GovernmentProcedure>
          <TransportEquipment>
            <CharacteristicCode>42G0</CharacteristicCode>
            <FullnessCode>5</FullnessCode>
            <ID>TGHU5228752</ID>
            <Seal>
              <SequenceNumeric>1</SequenceNumeric>
              <ID>A4231337765</ID>
            </Seal>
            <SupplierPartyTypeCode>2</SupplierPartyTypeCode>
          </TransportEquipment>
        </ConsignmentItem>
        <Consignor>
          <ID>128942</ID>
          <Name>Kudan company.(IMI)</Name>
          <Address>
            <CountryCode>TW</CountryCode>
            <Line>101-1303 KOLONG DEP.</Line>
          </Address>
          <Communication>
            <ID>82-2-7902731</ID>
            <TypeID>AL</TypeID>
          </Communication>
        </Consignor>
        <Freight>
          <PaymentMethodCode>CC</PaymentMethodCode>
        </Freight>
        <GoodsConsignedPlace>
          <ID>KRPUS</ID>
        </GoodsConsignedPlace>
        <GoodsReceiptPlace>
          <ID>ILHBT</ID>
        </GoodsReceiptPlace>
        <GovernmentAgencyGoodsItem>
          <AdditionalInformation>
            <Content>520043027</Content>
            <StatementTypeCode>2</StatementTypeCode>
          </AdditionalInformation>
          <AdditionalInformation>
            <StatementCode>1</StatementCode>
            <StatementTypeCode>5</StatementTypeCode>
          </AdditionalInformation>
        </GovernmentAgencyGoodsItem>
        <LoadingLocation>
          <ID>KRPUS</ID>
        </LoadingLocation>
        <TransportContractDocument>
          <ConditionCode>27</ConditionCode>
          <ID>I025307A56</ID>
          <TypeCode>IL1</TypeCode>
        </TransportContractDocument>
        <TransportContractDocument>
          <ID>SE-L0392838</ID>
          <TypeCode>705</TypeCode>
        </TransportContractDocument>
        <TransportContractDocument>
          <ID>ZIMUSEL71082056</ID>
          <TypeCode>704</TypeCode>
        </TransportContractDocument>
        <TransportContractDocument>
          <ID>I025544300</ID>
          <IssueLocation>Pusan</IssueLocation>
          <TypeCode>IL2</TypeCode>
        </TransportContractDocument>
        <UnloadingLocation>
          <ArrivalDateTime>2024-02-28T00:00:00</ArrivalDateTime>
          <ID>ILHBT</ID>
        </UnloadingLocation>
      </Consignment>
      <Submitter>
        <ID>513094649</ID>
      </Submitter>
    </Declaration>
  </Response>
</MN_MSG4_SendManifestFeedBack_Message>";
	}
}
