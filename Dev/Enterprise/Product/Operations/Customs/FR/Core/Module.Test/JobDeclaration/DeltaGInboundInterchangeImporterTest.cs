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

namespace Enterprise.Customs.FR.Module.Declaration.Testing
{
	sealed class DeltaGInboundInterchangeImporterTest : TestCaseWithFactory
	{
		public void TestMenuItem_ShouldBeVisible_WhenCurrentUserIsCWSupport()
		{
			CombineAssertions(() =>
			{
				Assert("Prerequisite: Current user is CWSupport by default.", GlbStaff.CurrentUser.IsSupportUser);
				Assert("The action of adding delta G response interchange should be visible when current user is CWSupport.", DeltaGInboundInterchangeImporter.IsMenuItemVisible());
			});
		}

		public void TestMenuItem_ShouldBeInvisible_WhenCurrentUserIsNotCWSupport()
		{
			GlbStaff.CurrentUser.GS_LoginName = "Dummy";
			CombineAssertions(() =>
			{
				Assert("Prerequisite: Current user is not CWSupport.", !GlbStaff.CurrentUser.IsSupportUser);
				Assert("The action of adding delta G response interchange should be invisible when current user is not CWSupport.", !DeltaGInboundInterchangeImporter.IsMenuItemVisible());
			});
		}

		public void TestEDIInterchangeShouldBeSuccessfullyCreated_WhenMenuItemIsClicked()
		{
			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			using (var stream = resourceRetriever.GetStream("Enterprise.Customs.FR.Module.Testing.TestFiles.DeltaCImportINVResponseMessageWithREF.xml"))
			{
				var importerMock = new Mock<DeltaGInboundInterchangeImporter>(Factory);
				importerMock.Protected().Setup<Stream>("GetXmlFileStream").Returns(stream);
				var addDeltaGResponseInterchangeActionMenuItem = importerMock.Object.GetNewMenuItem();
				addDeltaGResponseInterchangeActionMenuItem.PerformClick();
				CombineAssertions(() =>
				{
					var interchange = Factory.LoadTop1<FRInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.GenericMessageDelivery));
					AssertNotNull("An interchange of the xml uploaded should be created.", interchange);
					AssertEquals("EI_BodyText", expectedInterchangeText, interchange.EI_BodyText);
					AssertEquals("EI_ReceiveTransmit should be RCV.", EDIInterchange.Direction.Receive, interchange.EI_ReceiveTransmit);
					AssertEquals("EI_Status should be QUE.", EDIInterchange.Status.Queued, interchange.EI_Status);
					AssertEquals("EI_InterchangeType should be FRC.", GenericMessageDeliveryInterchangeTypeList.Codes.FRCustoms, interchange.EI_InterchangeType);
					AssertEquals("EI_From should be configured in FRCustomsDataRegistry.RecipientID", FRCustomsDataRegistry.Instance.RecipientID.Value, interchange.EI_From);
					AssertEquals("EI_To should be currentComany.LicenceKeyIdentifier", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_To);
				});
			}
		}
		public void TestEDIInterchangeShouldBeSuccessfullyCreated_WhenToolStripItemIsClicked()
		{
			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			using (var stream = resourceRetriever.GetStream("Enterprise.Customs.FR.Module.Testing.TestFiles.DeltaCImportINVResponseMessageWithREF.xml"))
			{
				var importerMock = new Mock<DeltaGInboundInterchangeImporter>(Factory);
				importerMock.Protected().Setup<Stream>("GetXmlFileStream").Returns(stream);
				var addDeltaGResponseInterchangeActionMenuItem = importerMock.Object.GetNewMenuItem();
				var toolStripItem = MenuItemToToolStripItemConverter.ConvertMenuItemToToolStripItem(addDeltaGResponseInterchangeActionMenuItem);
				toolStripItem.PerformClick();
				CombineAssertions(() =>
				{
					var interchange = Factory.LoadTop1<FRInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.GenericMessageDelivery));
					AssertNotNull("An interchange of the xml uploaded should be created.", interchange);
					AssertEquals("EI_BodyText", expectedInterchangeText, interchange.EI_BodyText);
					AssertEquals("EI_ReceiveTransmit should be RCV.", EDIInterchange.Direction.Receive, interchange.EI_ReceiveTransmit);
					AssertEquals("EI_Status should be QUE.", EDIInterchange.Status.Queued, interchange.EI_Status);
					AssertEquals("EI_InterchangeType should be FRC.", GenericMessageDeliveryInterchangeTypeList.Codes.FRCustoms, interchange.EI_InterchangeType);
					AssertEquals("EI_From should be configured in FRCustomsDataRegistry.RecipientID", FRCustomsDataRegistry.Instance.RecipientID.Value, interchange.EI_From);
					AssertEquals("EI_To should be currentComany.LicenceKeyIdentifier", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_To);
				});
			}
		}

		readonly string expectedInterchangeText = @"<?xml version=""1.0"" encoding=""iso-8859-1"" standalone=""yes""?>
<Message>
  <EnveloppeMessage>
    <schemaID>MessageReponseCDecImp</schemaID>
    <schemaVersion>18122012</schemaVersion>
    <partyId>40218856900048</partyId>
    <transactionId>AAAAAAAAA+BBB+0000000001</transactionId>
    <numseq>2</numseq>
  </EnveloppeMessage>
  <ReponseDeclaration>
    <Entete>
      <refdec>1901204207</refdec>
      <refdos>9000-B00177613</refdos>
      <dateValidationDec>13/02/2019</dateValidationDec>
    </Entete>
    <ReponseDatas>
      <Notification>
        <Etat>
          <etat>REF</etat>
          <etatDate>01/09/2023</etatDate>
          <etatHeure>10:25</etatHeure>
          <etatprec>DIN</etatprec>
          <etatprecDate>13/02/2019</etatprecDate>
          <etatprecHeure>09:02</etatprecHeure>
          <evenement>invalidation d'une dÃ©claration refusÃ©e par la douane</evenement>
          <ReponseDemande>
            <Motivation>
              <motiv>test ai2</motiv>
              <justifreg>Article 148 du délégué du CDU</justifreg>
              <nouvelledest>Aucune déclaration</nouvelledest>
            </Motivation>
            <motivservice>test ko</motivservice>
            <numdemande>2300000452</numdemande>
            <bureauagent>FR002300</bureauagent>
            <typedemande>INV</typedemande>
          </ReponseDemande>
        </Etat>
      </Notification>
    </ReponseDatas>
  </ReponseDeclaration>
</Message>";
	}
}
