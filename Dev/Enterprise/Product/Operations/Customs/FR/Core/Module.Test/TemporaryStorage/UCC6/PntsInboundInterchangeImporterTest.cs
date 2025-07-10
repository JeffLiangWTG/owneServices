using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.Customs.FR.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.FR.Module.TempStorage.Testing
{
	sealed class PntsInboundInterchangeImporterTest : TestCaseWithFactory
	{
		public void TestMenuItem_ShouldBeVisible_WhenCurrentUserIsCWSupport()
		{
			CombineAssertions(() =>
			{
				Assert("Prerequisite: Current user is CWSupport by default.", GlbStaff.CurrentUser.IsSupportUser);
				Assert("The action of adding PNTS response interchange should be visible when current user is CWSupport.", PntsInboundInterchangeImporter.IsMenuItemVisible());
			});
		}

		public void TestMenuItem_ShouldBeInvisible_WhenCurrentUserIsNotCWSupport()
		{
			GlbStaff.CurrentUser.GS_LoginName = "Dummy";
			CombineAssertions(() =>
			{
				Assert("Prerequisite: Current user is not CWSupport.", !GlbStaff.CurrentUser.IsSupportUser);
				Assert("The action of adding PNTS response interchange should be invisible when current user is not CWSupport.", !PntsInboundInterchangeImporter.IsMenuItemVisible());
			});
		}

		public void TestEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked()
		{
			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			using (var stream = resourceRetriever.GetStream("Enterprise.Customs.FR.Module.Testing.TestFiles.IETS016_Feedback_Invalid_IETS115_V0.2.0.xml"))
			{
				var importerMock = new Mock<PntsInboundInterchangeImporter>(Factory);
				importerMock.Protected().Setup<Stream>("GetXmlFileStream").Returns(stream);
				var addPntsResponseInterchangeActionMenuItem = importerMock.Object.GetNewMenuItem();
				addPntsResponseInterchangeActionMenuItem.PerformClick();
				CombineAssertions(() =>
				{
					var interchange = Factory.LoadTop1<FRInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.GenericMessageDelivery));
					AssertNotNull("An interchange of the xml uploaded should be created.", interchange);
					AssertEquals("EI_BodyText", expectedInterchangeText, interchange.EI_BodyText);
					AssertEquals("EI_ReceiveTransmit should be RCV.", EDIInterchange.Direction.Receive, interchange.EI_ReceiveTransmit);
					AssertEquals("EI_Status should be QUE.", EDIInterchange.Status.Queued, interchange.EI_Status);
					AssertEquals("EI_InterchangeType should be FRS.", GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsPNTS, interchange.EI_InterchangeType);
					AssertEquals("EI_From should be configured in FRCustomsDataRegistry.RecipientID", FRCustomsDataRegistry.Instance.RecipientID.Value, interchange.EI_From);
					AssertEquals("EI_To should be currentComany.LicenceKeyIdentifier", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_To);
				});
			}
		}

		public void TestEDIInterchangeShouldBeSuccessfullyCreated_WhenToolStripItemIsClicked()
		{
			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			using (var stream = resourceRetriever.GetStream("Enterprise.Customs.FR.Module.Testing.TestFiles.IETS016_Feedback_Invalid_IETS115_V0.2.0.xml"))
			{
				var importerMock = new Mock<PntsInboundInterchangeImporter>(Factory);
				importerMock.Protected().Setup<Stream>("GetXmlFileStream").Returns(stream);
				var addPntsResponseInterchangeActionMenuItem = importerMock.Object.GetNewMenuItem();
				var toolStripItem = MenuItemToToolStripItemConverter.ConvertMenuItemToToolStripItem(addPntsResponseInterchangeActionMenuItem);
				toolStripItem.PerformClick();
				CombineAssertions(() =>
				{
					var interchange = Factory.LoadTop1<FRInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.GenericMessageDelivery));
					AssertNotNull("An interchange of the xml uploaded should be created.", interchange);
					AssertEquals("EI_BodyText", expectedInterchangeText, interchange.EI_BodyText);
					AssertEquals("EI_ReceiveTransmit should be RCV.", EDIInterchange.Direction.Receive, interchange.EI_ReceiveTransmit);
					AssertEquals("EI_Status should be QUE.", EDIInterchange.Status.Queued, interchange.EI_Status);
					AssertEquals("EI_InterchangeType should be FRS.", GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsPNTS, interchange.EI_InterchangeType);
					AssertEquals("EI_From should be configured in FRCustomsDataRegistry.RecipientID", FRCustomsDataRegistry.Instance.RecipientID.Value, interchange.EI_From);
					AssertEquals("EI_To should be currentComany.LicenceKeyIdentifier", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_To);
				});
			}
		}

		readonly string expectedInterchangeText = @"<IETS016>
	<messageHeader>
		<sender>TSD@BE</sender>
		<recipient>system@BE0999999999</recipient>
		<messageTimestamp>2021-05-01T12:34:56.123456789Z</messageTimestamp>
		<messageId>4c577c952370-4715-9f0b-fab9bcaaec1e</messageId>
		<refToMessageId>6fbe80fe-dfeb-4dd8-be69-8d8be24a8f07.example.com</refToMessageId>
		<correlationId>a96bb508f0a4-4fc3-8c9f-0badba0d8d0e</correlationId>
		<languageCode>EN</languageCode>
	</messageHeader>
	<lrn>IETS115INVALIDMESSAGE</lrn>
	<notificationDate>2021-05-01T12:34:56Z</notificationDate>
	<businessValidationType>115</businessValidationType>
	<declarant>
		<identificationNumber>BE0999999999</identificationNumber>
		<name>Declarant name</name>
	</declarant>
	<representative>
		<identificationNumber>BE0999999999</identificationNumber>
		<name>Representative name</name>
		<status>2</status>
	</representative>
	<personPresentingTheGoods>
		<identificationNumber>BE0999999999</identificationNumber>
	</personPresentingTheGoods>
	<supervisingCustomsOffice>
		<referenceNumber>BE212000</referenceNumber>
	</supervisingCustomsOffice>
	<customsOfficeOfPresentation>
		<referenceNumber>BE212000</referenceNumber>
	</customsOfficeOfPresentation>
	<error>
		<sequenceNumber>1</sequenceNumber>
		<errorPointer>dateAndTimeOfPresentationOfTheGoods</errorPointer>
		<errorCode>99</errorCode>
		<errorReason>BER0069</errorReason>
		<remarks>The Date and time of presentation of the goods is not valid.</remarks>
	</error>
	<error>
		<sequenceNumber>2</sequenceNumber>
		<errorPointer>declarationDate</errorPointer>
		<errorCode>99</errorCode>
		<errorReason>BER0071</errorReason>
		<remarks>The Declaration date is not valid.</remarks>
	</error>
</IETS016>";
	}
}
