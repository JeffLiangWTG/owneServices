using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.Customs.FR.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.FR.Module.NCTS.Testing
{
	sealed class TP5InboundInterchangeImporterTest : TestCaseWithFactory
	{
		public void TestMenuItem_ShouldBeVisible_WhenCurrentUserIsCWSupport()
		{
			CombineAssertions(() =>
			{
				Assert("Prerequisite: Current user is CWSupport by default.", GlbStaff.CurrentUser.IsSupportUser);
				Assert("The action of adding TP5 response interchange should be visible when current user is CWSupport.", TP5InboundInterchangeImporter.IsMenuItemVisible());
			});
		}

		public void TestMenuItem_ShouldBeInvisible_WhenCurrentUserIsNotCWSupport()
		{
			GlbStaff.CurrentUser.GS_LoginName = "Dummy";
			CombineAssertions(() =>
			{
				Assert("Prerequisite: Current user is not CWSupport.", !GlbStaff.CurrentUser.IsSupportUser);
				Assert("The action of adding TP5 response interchange should be invisible when current user is not CWSupport.", !TP5InboundInterchangeImporter.IsMenuItemVisible());
			});
		}

		public void TestEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked_CC004C()
		{
			AssertEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked("Enterprise.Customs.FR.Module.Testing.TestFiles.NCTS_CC004CResponseMessage.xml");
		}

		public void TestEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked_CC009C()
		{
			AssertEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked("Enterprise.Customs.FR.Module.Testing.TestFiles.NCTS_CC009CResponseMessage.xml");
		}

		public void TestEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked_CC019C()
		{
			AssertEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked("Enterprise.Customs.FR.Module.Testing.TestFiles.NCTS_CC019CResponseMessage.xml");
		}

		public void TestEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked_CC022C()
		{
			AssertEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked("Enterprise.Customs.FR.Module.Testing.TestFiles.NCTS_CC022CResponseMessage.xml");
		}

		public void TestEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked_CC025C()
		{
			AssertEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked("Enterprise.Customs.FR.Module.Testing.TestFiles.NCTS_CC025CResponseMessage.xml");
		}

		public void TestEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked_CC028C()
		{
			AssertEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked("Enterprise.Customs.FR.Module.Testing.TestFiles.NCTS_CC028CResponseMessage.xml");
		}

		public void TestEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked_CC029C()
		{
			AssertEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked("Enterprise.Customs.FR.Module.Testing.TestFiles.NCTS_CC029CResponseMessage.xml");
		}

		public void TestEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked_CC035C()
		{
			AssertEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked("Enterprise.Customs.FR.Module.Testing.TestFiles.NCTS_CC035CResponseMessage.xml");
		}

		public void TestEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked_CC043C()
		{
			AssertEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked("Enterprise.Customs.FR.Module.Testing.TestFiles.NCTS_CC043CResponseMessage.xml");
		}

		public void TestEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked_CC045C()
		{
			AssertEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked("Enterprise.Customs.FR.Module.Testing.TestFiles.NCTS_CC045CResponseMessage.xml");
		}

		public void TestEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked_CC055C()
		{
			AssertEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked("Enterprise.Customs.FR.Module.Testing.TestFiles.NCTS_CC055CResponseMessage.xml");
		}

		public void TestEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked_CC056C()
		{
			AssertEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked("Enterprise.Customs.FR.Module.Testing.TestFiles.NCTS_CC056CResponseMessage.xml");
		}

		public void TestEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked_CC057C()
		{
			AssertEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked("Enterprise.Customs.FR.Module.Testing.TestFiles.NCTS_CC057CResponseMessage.xml");
		}

		public void TestEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked_CC140C()
		{
			AssertEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked("Enterprise.Customs.FR.Module.Testing.TestFiles.NCTS_CC140CResponseMessage.xml");
		}

		public void TestEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked_CC182C()
		{
			AssertEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked("Enterprise.Customs.FR.Module.Testing.TestFiles.NCTS_CC182CResponseMessage.xml");
		}

		public void TestEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked_CD906C()
		{
			AssertEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked("Enterprise.Customs.FR.Module.Testing.TestFiles.NCTS_CD906CResponseMessage.xml");
		}

		public void TestEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked_CC917C()
		{
			AssertEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked("Enterprise.Customs.FR.Module.Testing.TestFiles.NCTS_CC917CResponseMessage.xml");
		}

		public void TestEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked_CC928C()
		{
			AssertEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked("Enterprise.Customs.FR.Module.Testing.TestFiles.NCTS_CC928CResponseMessage.xml");
		}

		public void TestEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked_CCF02C()
		{
			AssertEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked("Enterprise.Customs.FR.Module.Testing.TestFiles.NCTS_CCF02CResponseMessage.xml");
		}

		public void TestEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked_CCF03C()
		{
			AssertEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked("Enterprise.Customs.FR.Module.Testing.TestFiles.NCTS_CCF03CResponseMessage.xml");
		}

		public void AssertEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked(string fileLocation)
		{
			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			var stringResourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
			var expectedInterchangeText = stringResourceRetriever.Value.GetString(fileLocation);

			using (var stream = resourceRetriever.GetStream(fileLocation))
			{
				var importerMock = new Mock<TP5InboundInterchangeImporter>(Factory);
				importerMock.Protected().Setup<Stream>("GetXmlFileStream").Returns(stream);
				var actionMenuItem = importerMock.Object.GetNewMenuItem();
				actionMenuItem.PerformClick();
				CombineAssertions(() =>
				{
					var interchange = Factory.LoadTop1<FRInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.GenericMessageDelivery));
					AssertNotNull("An interchange should be created.", interchange);
					AssertEquals("EI_BodyText", expectedInterchangeText, interchange.EI_BodyText);
					AssertEquals("EI_ReceiveTransmit should be RCV.", EDIInterchange.Direction.Receive, interchange.EI_ReceiveTransmit);
					AssertEquals("EI_Status should be QUE.", EDIInterchange.Status.Queued, interchange.EI_Status);
					AssertEquals("EI_InterchangeType should be TP5.", GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsTP5, interchange.EI_InterchangeType);
					AssertEquals("EI_From should be configured in FRCustomsDataRegistry.RecipientID", FRCustomsDataRegistry.Instance.RecipientID.Value, interchange.EI_From);
					AssertEquals("EI_To should be currentCompany.LicenceKeyIdentifier", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_To);
				});
			}
		}
	}
}
