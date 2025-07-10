using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.Customs.FR.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.FR.Module.Declaration.Testing
{
	sealed class DeltaIEInboundInterchangeImporterTest : TestCaseWithFactory
	{
		public void TestMenuItem_ShouldBeVisible_WhenCurrentUserIsCWSupport()
		{
			CombineAssertions(() =>
			{
				Assert("Prerequisite: Current user is CWSupport by default.", GlbStaff.CurrentUser.IsSupportUser);
				Assert("The action of adding delta IE response interchange should be visible when current user is CWSupport.", DeltaIEInboundInterchangeImporter.IsMenuItemVisible());
			});
		}

		public void TestMenuItem_ShouldBeInvisible_WhenCurrentUserIsNotCWSupport()
		{
			GlbStaff.CurrentUser.GS_LoginName = "Dummy";
			CombineAssertions(() =>
			{
				Assert("Prerequisite: Current user is not CWSupport.", !GlbStaff.CurrentUser.IsSupportUser);
				Assert("The action of adding delta IE response interchange should be invisible when current user is not CWSupport.", !DeltaIEInboundInterchangeImporter.IsMenuItemVisible());
			});
		}

		public void TestSubMenuItems_ShouldBeEquivalentToAllDeltaIEResponseMessageSubTypeList()
		{
			var deltaIEInterchangeImporter = new DeltaIEInboundInterchangeImporter(Factory);
			var newMenuItem = deltaIEInterchangeImporter.GetNewMenuItem();
			CombineAssertions(() =>
			{
				AssertEquals("Caption of the Delta IE interchange adding action menu item.", "Add Delta I/E Inbound Interchange (CWSupport Only)", newMenuItem.Text);

				var expectedMenuItemTags = new DeltaIEResponseMessageSubTypeList().GetAllCodes().Select(x => DeltaIEMessageSubTypeAndSchemaIdConverter.ConvertMessageSubTypeToSchemaId(x));
				var actualMenuItemTags = newMenuItem.MenuItems.Cast<MenuItem>().Select(x => x.Tag);
				AssertContainsExactElementsInAnyOrder("The menu items should be all implemented Delta IE Response message types.", expectedMenuItemTags, actualMenuItemTags);
			});
		}

		public void TestEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked_IE456()
		{
			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			using (var stream = resourceRetriever.GetStream("Enterprise.Customs.FR.Module.Testing.TestFiles.DeltaIE_IE456ResponseMessage.json"))
			{
				var importerMock = new Mock<DeltaIEInboundInterchangeImporter>(Factory);
				importerMock.Protected().Setup<Stream>("GetJsonFileStream").Returns(stream);
				var addDeltaIEResponseInterchangeActionMenuItem = importerMock.Object.GetNewMenuItem();
				var ie456InterchangeCreateActionMenuItem = addDeltaIEResponseInterchangeActionMenuItem.MenuItems.Cast<MenuItem>().ToList().Find(x => x.Tag.ToString() == "IE456");
				ie456InterchangeCreateActionMenuItem.PerformClick();
				CombineAssertions(() =>
				{
					var interchange = Factory.LoadTop1<FRInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.GenericMessageDelivery));
					AssertNotNull("An interchange of the json uploaded should be created.", interchange);
					AssertEquals("EI_BodyText", expectedInterchangeText, interchange.EI_BodyText);
					AssertEquals("EI_ReceiveTransmit should be RCV.", EDIInterchange.Direction.Receive, interchange.EI_ReceiveTransmit);
					AssertEquals("EI_Status should be QUE.", EDIInterchange.Status.Queued, interchange.EI_Status);
					AssertEquals("EI_InterchangeType should be FRI.", GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsDeltaIE, interchange.EI_InterchangeType);
					AssertEquals("EI_From should be configured in FRCustomsDataRegistry.RecipientID", FRCustomsDataRegistry.Instance.RecipientID.Value, interchange.EI_From);
					AssertEquals("EI_To should be currentComany.LicenceKeyIdentifier", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_To);
				});
			}
		}
		public void TestEDIInterchangeShouldBeSuccessfullyCreated_WhenToolStripItemIsClicked_IE456()
		{
			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			using (var stream = resourceRetriever.GetStream("Enterprise.Customs.FR.Module.Testing.TestFiles.DeltaIE_IE456ResponseMessage.json"))
			{
				var importerMock = new Mock<DeltaIEInboundInterchangeImporter>(Factory);
				importerMock.Protected().Setup<Stream>("GetJsonFileStream").Returns(stream);
				var addDeltaIEResponseInterchangeActionMenuItem = importerMock.Object.GetNewMenuItem();
				var ie456InterchangeCreateActionMenuItem = addDeltaIEResponseInterchangeActionMenuItem.MenuItems.Cast<MenuItem>().ToList().Find(x => x.Tag.ToString() == "IE456");
				var toolStripItem = MenuItemToToolStripItemConverter.ConvertMenuItemToToolStripItem(ie456InterchangeCreateActionMenuItem);
				toolStripItem.PerformClick();
				CombineAssertions(() =>
				{
					var interchange = Factory.LoadTop1<FRInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.GenericMessageDelivery));
					AssertNotNull("An interchange of the json uploaded should be created.", interchange);
					AssertEquals("EI_BodyText", expectedInterchangeText, interchange.EI_BodyText);
					AssertEquals("EI_ReceiveTransmit should be RCV.", EDIInterchange.Direction.Receive, interchange.EI_ReceiveTransmit);
					AssertEquals("EI_Status should be QUE.", EDIInterchange.Status.Queued, interchange.EI_Status);
					AssertEquals("EI_InterchangeType should be FRI.", GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsDeltaIE, interchange.EI_InterchangeType);
					AssertEquals("EI_From should be configured in FRCustomsDataRegistry.RecipientID", FRCustomsDataRegistry.Instance.RecipientID.Value, interchange.EI_From);
					AssertEquals("EI_To should be currentComany.LicenceKeyIdentifier", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_To);
				});
			}
		}

		readonly string expectedInterchangeText = @"{
  ""SchemaId"": ""IE456"",
  ""TransactionId"": """",
  ""MessageJson"": {
    ""ImportOperation"": [
      {
        ""LRN"": ""IE456000001"",
        ""customsRegistrationNumber"": ""CRN099999999"",
        ""MRN"": ""MRN099999999"",
        ""businessRejectionType"": ""415"",
        ""rejectionDateAndTime"": ""2021-05-01T12:34:56Z"",
        ""rejectionCode"": ""6"",
        ""rejectionReason"": ""Reason""
      }
    ],
    ""SupervisingCustomsOffice"": {
      ""referenceNumber"": ""BE212000""
    },
    ""Declarant"": {
      ""identificationNumber"": ""BE0999999999"",
      ""name"": ""Declarant name"",
      ""Address"": {
        ""streetAndNumber"": ""abc"",
        ""postcode"": ""123456"",
        ""city"": ""Paris"",
        ""country"": ""FR""
      },
      ""ContactPerson"": {
        ""name"": ""ContactPerson"",
        ""phoneNumber"": ""123456"",
        ""eMailAddress"": ""aa@aa.com""
      }
    },
    ""Representative"": {
      ""identificationNumber"": ""BE0999999999"",
      ""status"": ""3"",
      ""ContactPerson"": {
        ""name"": ""Representative name"",
        ""phoneNumber"": ""123456"",
        ""eMailAddress"": ""aa@aa.com""
      }
    },
    ""DeclarationStatus"": {
      ""state"": ""vBlXY1lPX0"",
      ""stateDateTime"": ""8973-04-89T23:98:87"",
      ""event"": ""abc""
    },
    ""FunctionalError"": [
      {
        ""sequenceNumber"": ""2"",
        ""errorPointer"": ""declarationDate"",
        ""errorCode"": ""99"",
        ""errorReason"": ""BER0071"",
        ""remarks"": ""The Declaration date is not valid."",
        ""originalAttributeValue"": ""originalAttributeValue""
      }
    ]
  }
}";
	}
}
