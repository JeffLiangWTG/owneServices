using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Integration;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.GB.CDS.Testing
{
	class CDSDeclarationInfoResponseMessageProcessorTests : TestCaseWithFactory
	{
		public void TestMessageFriendlyName()
		{
			var processor = new CDSDeclarationInfoResponseMessageProcessor(new LoggingInformation());
			AssertEquals("CDS Declaration Query Response Message", processor.MessageFriendlyName);
		}

		public void TestMessageTypesToInclude()
		{
			var processor = new CDSDeclarationInfoResponseMessageProcessor(new LoggingInformation());
			var messageTypesToInclude = processor.MessageTypesToInclude;
			AssertEquals(1, messageTypesToInclude.Count);
			AssertEquals(CDSEDIMessageTypeList.Codes.QueryResponse, messageTypesToInclude[0]);
		}

		public void TestEmailNotifications()
		{
			SetupNotificationGroup(GBCustomsDataRegistry.Instance.NotificationCDS, "Harley.Quinn@DefaultCDS.com", "CDS");

			var message = Factory.New<CDSDeclarationInfoResponseEDIMessage>();

			message.EM_MessageText = CDSDeclarationInfoResponseEDIMessage.Serialize(CDSDeclarationInfoResponseTests.CDSDeclarationInfoResponseXMLForTest);

			new CDSDeclarationInfoResponseMessageProcessor(new LoggingInformation()).ProcessMessage(message);

			var emails = Factory.Load<MailItem>(new ZQuery(MailDBItemsSchema.MI_Direction, MailDirection.Transmit));

			AssertEquals(1, emails.Length);
			AssertContains(CDSDeclarationInfoResponseTests.ExpectedHTMLInterpretation, emails[0].MI_Body);
		}

		public void TestProcessMessageDetails()
		{
			var dec1 = Factory.New<JobDeclaration>();
			var entry1 = dec1.CustomsEntryHeaders.AddNew();
			var dec2 = Factory.New<JobDeclaration>();
			var entry2 = dec2.CustomsEntryHeaders.AddNew();

			entry1.MovementReferenceNumberSetter("MRN0000001", ZDateTime.Today);
			entry2.MovementReferenceNumberSetter("MRN0000002", ZDateTime.Today);

			Factory.Save();

			var message = Factory.New<CDSDeclarationInfoResponseEDIMessage>();

			message.EM_MessageText = @"<p:DeclarationStatusResponse xsi:schemaLocation=""http://gov.uk/customs/declarationInformationRetrieval/status/v2"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:p4=""urn:un:unece:uncefact:data:standard:UnqualifiedDataType:6"" xmlns:p3=""urn:wco:datamodel:WCO:Declaration_DS:DMS:2"" xmlns:p2=""urn:wco:datamodel:WCO:DEC-DMS:2"" xmlns:p1=""urn:wco:datamodel:WCO:Response_DS:DMS:2"" xmlns:p=""http://gov.uk/customs/declarationInformationRetrieval/status/v2"">
	<p:DeclarationStatusDetails>
		<p:Declaration>
			<p:ID>MRN0000001</p:ID>
			<p:VersionID>1</p:VersionID>
			<p:ReceivedDateTime>
				<p:DateTimeString formatCode=""304"">20210325134724Z</p:DateTimeString>
			</p:ReceivedDateTime>
			<p:ROE>H</p:ROE>
			<p:ICS>14</p:ICS>
			<p:IRC>X</p:IRC>
		</p:Declaration>
		<p2:Declaration>
			<p2:FunctionCode>9</p2:FunctionCode>
			<p2:TypeCode>IMD</p2:TypeCode>
			<p2:GoodsItemQuantity>1</p2:GoodsItemQuantity>
			<p2:TotalPackageQuantity>10</p2:TotalPackageQuantity>
			<p2:Submitter>
				<p2:ID>GB159688953432</p2:ID>
			</p2:Submitter>
			<p2:GoodsShipment>
				<p2:PreviousDocument>
					<p2:ID>1GB896458895023-B00031630</p2:ID>
					<p2:TypeCode>DCR</p2:TypeCode>
				</p2:PreviousDocument>
				<p2:UCR>
					<p2:TraderAssignedReferenceID>1GB896458895023-B00031630</p2:TraderAssignedReferenceID>
				</p2:UCR>
			</p2:GoodsShipment>
		</p2:Declaration>
	</p:DeclarationStatusDetails>
	<p:DeclarationStatusDetails>
		<p:Declaration>
			<p:ID>MRN0000002</p:ID>
			<p:VersionID>1</p:VersionID>
			<p:ReceivedDateTime>
				<p:DateTimeString formatCode=""304"">20210325153120Z</p:DateTimeString>
			</p:ReceivedDateTime>
			<p:ROE>H</p:ROE>
			<p:ICS>18</p:ICS>
		</p:Declaration>
		<p2:Declaration>
			<p2:FunctionCode>9</p2:FunctionCode>
			<p2:TypeCode>IMD</p2:TypeCode>
			<p2:GoodsItemQuantity>1</p2:GoodsItemQuantity>
			<p2:TotalPackageQuantity>10</p2:TotalPackageQuantity>
			<p2:Submitter>
				<p2:ID>GB159688953432</p2:ID>
			</p2:Submitter>
			<p2:GoodsShipment>
				<p2:PreviousDocument>
					<p2:ID>1GB896458895023-B00031630</p2:ID>
					<p2:TypeCode>DCR</p2:TypeCode>
				</p2:PreviousDocument>
				<p2:UCR>
					<p2:TraderAssignedReferenceID>1GB896458895023-B00031630/1</p2:TraderAssignedReferenceID>
				</p2:UCR>
			</p2:GoodsShipment>
		</p2:Declaration>
	</p:DeclarationStatusDetails>
</p:DeclarationStatusResponse>";

			new CDSDeclarationInfoResponseMessageProcessor(new LoggingInformation()).ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertEquals("ROE-1", "H", entry1.CH_RouteOfEntry);
				AssertEquals("ICS-1", "14", entry1.CH_ImportClearanceStatusICS);
				AssertEquals("IRC-1", "X", entry1.CH_IrcInventoryReturnCode);
				AssertEquals("ROE-2", "H", entry2.CH_RouteOfEntry);
				AssertEquals("ICS-2", "18", entry2.CH_ImportClearanceStatusICS);
			});
		}

		public void TestProcessDeclarationSearchQuery()
		{
			var message = Factory.New<CDSDeclarationInfoResponseEDIMessage>();

			message.EM_MessageText = @"<p:DeclarationSearchResponse xsi:schemaLocation=""http://gov.uk/customs/declarationInformationRetrieval/declarationSummary/v1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:p4=""urn:un:unece:uncefact:data:standard:UnqualifiedDataType:6"" xmlns:p3=""urn:wco:datamodel:WCO:Declaration_DS:DMS:2"" xmlns:p2=""urn:wco:datamodel:WCO:DEC-DMS:2"" xmlns:p1=""urn:wco:datamodel:WCO:Response_DS:DMS:2"" xmlns:p=""http://gov.uk/customs/declarationInformationRetrieval/declarationSummary/v1"">
    <p:DeclarationSearchDetails>
        <p:Declaration>
            <p:ID>23GBB7KF5523KMKAR1</p:ID>
            <p:ReceivedDateTime>
                <p:DateTimeString formatCode=""304"">20231010102247Z</p:DateTimeString>
            </p:ReceivedDateTime>
            <p:ROE>H</p:ROE>
            <p:ICS>14</p:ICS>
            <p:LRN>HYEDUKCM20000000003421</p:LRN>
        </p:Declaration>
        <p2:Declaration>
            <p2:FunctionCode>9</p2:FunctionCode>
            <p2:TypeCode>IMD</p2:TypeCode>
            <p2:Submitter>
                <p2:ID>GB048834222514</p2:ID>
            </p2:Submitter>
            <p2:Declarant>
                <p2:ID>GB896458895015</p2:ID>
            </p2:Declarant>
            <p2:GoodsShipment>
                <p2:Consignment>
                    <p2:GoodsLocation>
                        <p2:Name>ABDABDABM</p2:Name>
                        <p2:TypeCode>A</p2:TypeCode>
                        <p2:Address>
                            <p2:TypeCode>U</p2:TypeCode>
                            <p2:CountryCode>GB</p2:CountryCode>
                        </p2:Address>
                    </p2:GoodsLocation>
                </p2:Consignment>
                <p2:Importer>
                    <p2:ID>GB896458895015</p2:ID>
                </p2:Importer>
            </p2:GoodsShipment>
        </p2:Declaration>
    </p:DeclarationSearchDetails>
    <p:DeclarationSearchDetails>
        <p:Declaration>
            <p:ID>23GBB7K6WRBIPQDAR8</p:ID>
            <p:ReceivedDateTime>
                <p:DateTimeString formatCode=""304"">20231010101623Z</p:DateTimeString>
            </p:ReceivedDateTime>
            <p:ROE>H</p:ROE>
            <p:ICS>14</p:ICS>
            <p:LRN>HYEDUKCM20000000003420</p:LRN>
        </p:Declaration>
        <p2:Declaration>
            <p2:FunctionCode>9</p2:FunctionCode>
            <p2:TypeCode>IMD</p2:TypeCode>
            <p2:Submitter>
                <p2:ID>GB048834222514</p2:ID>
            </p2:Submitter>
            <p2:Declarant>
                <p2:ID>GB896458895015</p2:ID>
            </p2:Declarant>
            <p2:GoodsShipment>
                <p2:Consignment>
                    <p2:GoodsLocation>
                        <p2:Name>ABDABDABM</p2:Name>
                        <p2:TypeCode>A</p2:TypeCode>
                        <p2:Address>
                            <p2:TypeCode>U</p2:TypeCode>
                            <p2:CountryCode>GB</p2:CountryCode>
                        </p2:Address>
                    </p2:GoodsLocation>
                </p2:Consignment>
                <p2:Importer>
                    <p2:ID>GB896458895015</p2:ID>
                </p2:Importer>
            </p2:GoodsShipment>
        </p2:Declaration>
    </p:DeclarationSearchDetails>
    <p:CurrentPageNumber>1</p:CurrentPageNumber>
    <p:TotalResultsAvailable>2</p:TotalResultsAvailable>
    <p:TotalPagesAvailable>1</p:TotalPagesAvailable>
    <p:NoResultsReturned>false</p:NoResultsReturned>
</p:DeclarationSearchResponse>";

			new CDSDeclarationInfoResponseMessageProcessor(new LoggingInformation()).ProcessMessage(message);

			AssertEquals(Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
		}

		void SetupNotificationGroup(IRegistryItem notificationRegoItem, ZString emailAddress, ZString code)
		{
			var postmasters = Factory.Load<GlbGroup>(Groups.PostMastersGroupPK);
			var postMaster = postmasters.Staff.AddNew();
			postMaster.GS_EmailAddress = "PostMaster@Gallifrey.com";

			if (notificationRegoItem != null)
			{
				var group = Factory.New<GlbGroup>();
				group.GG_Code = code;
				group.GG_Desc = "DCGroup" + code;
				var staff = group.Staff.AddNew();
				staff.GS_Code = code;
				staff.GS_LoginName = "DCUser" + code;
				staff.GS_EmailAddress = emailAddress;
				Factory.Save();
				notificationRegoItem.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, group.PK.ToGuid());
				Factory.Save();
			}
		}
	}
}
