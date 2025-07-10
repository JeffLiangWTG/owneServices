using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	class GsumMessageExporterTest : JXCMessageExporterTestCase
	{
		public void TestCheckAndCreateGsumLinesToBeSent_EnsureExistingEventsAreCleared()
		{
			AssertEquals("Pre-condition", 0, Exporter.GsumLinesToBeSentForTesting.Count);
			Exporter.GsumLinesToBeSentForTesting.Add(new GSUMLine(new GsumShipmentWrapper(Shipment), "XXX", ZDateTime.Now));
			AssertEquals(1, Exporter.GsumLinesToBeSentForTesting.Count);
			Exporter.CheckAndCreateGsumLinesToBeSent();
			AssertExportedMessage(Exporter);
		}

		public void TestCheckAndCreateGsumLinesToBeSent_GoodsPickedUpFromShipper()
		{
			ToggleShipmentDirection(false);
			Shipment.JS_A_RCV = new ZDateTime(2007, 1, 1, 20, 20, 30);
			SetExpectedMessageLines(new GSUMLine(ShipmentWrapper, "RSH", Shipment.JS_A_RCV));
			Exporter.CheckAndCreateGsumLinesToBeSent();
			AssertExportedMessage(Exporter);
			Shipment.DocsAndCartage.JP_PickupCartageCompleted = new ZDateTime(2006, 2, 2, 10, 22, 33);
			Exporter.CheckAndCreateGsumLinesToBeSent();
			AssertExportedMessage(Exporter);
			Shipment.JS_A_RCV = ZDateTime.Empty;
			Shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.Empty;
			SetExpectedMessageLines();
			Exporter.CheckAndCreateGsumLinesToBeSent();
			AssertExportedMessage(Exporter);
		}

		public void TestCheckAndCreateGsumLinesToBeSent_GoodsReceivedFromShipper()
		{
			ToggleShipmentDirection(false);
			Shipment.DocsAndCartage.JP_PickupCartageCompleted = new ZDateTime(2007, 1, 1, 20, 20, 30);
			SetExpectedMessageLines(new GSUMLine(ShipmentWrapper, "PUP", Shipment.DocsAndCartage.JP_PickupCartageCompleted));
			Exporter.CheckAndCreateGsumLinesToBeSent();
			AssertExportedMessage(Exporter);
			Shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.Empty;
			SetExpectedMessageLines();
			Exporter.CheckAndCreateGsumLinesToBeSent();
			AssertExportedMessage(Exporter);
		}

		public void TestCheckAndCreateGsumLinesToBeSent_HasExportEventsButNonExportShipment()
		{
			ToggleShipmentDirection(true);
			Shipment.DocsAndCartage.JP_PickupCartageCompleted = new ZDateTime(2007, 1, 1, 20, 20, 30);
			Exporter.CheckAndCreateGsumLinesToBeSent();
			AssertExportedMessage(Exporter);
		}

		public void TestCheckAndCreateGsumLinesToBeSent_GoodsOutForDeliveryToConsignee()
		{
			ToggleShipmentDirection(true);
			Shipment.DocsAndCartage.JP_DeliveryCartageAdvised = new ZDateTime(2007, 1, 1, 20, 20, 30);
			SetExpectedMessageLines(new GSUMLine(ShipmentWrapper, "OFD", Shipment.DocsAndCartage.JP_DeliveryCartageAdvised));
			Exporter.CheckAndCreateGsumLinesToBeSent();
			AssertExportedMessage(Exporter);
			Shipment.DocsAndCartage.JP_DeliveryCartageAdvised = ZDateTime.Empty;
			SetExpectedMessageLines();
			Exporter.CheckAndCreateGsumLinesToBeSent();
			AssertExportedMessage(Exporter);
		}

		public void TestCheckAndCreateGsumLinesToBeSent_GoodsDeliveredToConsignee()
		{
			ToggleShipmentDirection(true);
			Shipment.DocsAndCartage.JP_DeliveryCartageCompleted = new ZDateTime(2007, 1, 1, 20, 20, 30);
			SetExpectedMessageLines(new GSUMLine(ShipmentWrapper, "POD", Shipment.DocsAndCartage.JP_DeliveryCartageCompleted));
			Exporter.CheckAndCreateGsumLinesToBeSent();
			AssertExportedMessage(Exporter);
			Shipment.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Empty;
			SetExpectedMessageLines();
			Exporter.CheckAndCreateGsumLinesToBeSent();
			AssertExportedMessage(Exporter);
		}

		public void TestCheckAndCreateGsumLinesToBeSent_DocumentTurnedOverToBroker_InhouseBroker()
		{
			ToggleShipmentDirection(true);
			JASOrgHeader importBroker = Factory.New<JASOrgHeader>();
			importBroker.OfficeCode = "USATL";
			importBroker.NettingCode = "USATL";
			Shipment.JS_OH_ImportBroker = importBroker.PK;
			Exporter.CheckAndCreateGsumLinesToBeSent();
			AssertExportedMessage(Exporter);
		}

		[TestDate(2005, 1, 2, 10, 33, 20)]
		public void TestCheckAndCreateGsumLinesToBeSent_DocumentTurnedOverToBroker_ThirdPartyBroker()
		{
			ToggleShipmentDirection(true);
			Shipment.JS_OH_ImportBroker = Factory.New<JASOrgHeader>().PK;
			Exporter.CheckAndCreateGsumLinesToBeSent();
			SetExpectedMessageLines(new GSUMLine(ShipmentWrapper, "DTO", ZDateTime.Now));
			AssertExportedMessage(Exporter);
			Shipment.JS_OH_ImportBroker = ZGuid.Empty;
			Exporter.CheckAndCreateGsumLinesToBeSent();
			SetExpectedMessageLines();
			AssertExportedMessage(Exporter);
		}

		public void TestCheckAndCreateGsumLinesToBeSent_CustomsEntryMade()
		{
			ToggleShipmentDirection(true);
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_JS = Shipment.PK;
			declaration.CustomsEntryHeaders.AddNew();
			declaration.LogCustomsCommencedIfNeeded();
			Exporter.CheckAndCreateGsumLinesToBeSent();
			SetExpectedMessageLines(new GSUMLine(ShipmentWrapper, "CUS", declaration.LogsOfDeclarationOrShipment.MostRecentLogByEventTime(Events.CustomsCommenced).SL_EventTime));
			AssertExportedMessage(Exporter);
		}

		public void TestCheckAndCreateGsumLinesToBeSent_CustomsCleared()
		{
			ToggleShipmentDirection(true);
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_JS = Shipment.PK;
			declaration.CustomsEntryHeaders.AddNew();
			declaration.LogCustomsClearedIfNeeded();
			Exporter.CheckAndCreateGsumLinesToBeSent();
			SetExpectedMessageLines(new GSUMLine(ShipmentWrapper, "CLR", declaration.LogsOfDeclarationOrShipment.MostRecentLogByEventTime(Events.CustomsCleared).SL_EventTime));
			AssertExportedMessage(Exporter);
		}

		[TestDate(2005, 1, 2, 10, 33, 20)]
		public void TestCheckAndCreateGsumLinesToBeSent_MultipleImportGsumEvents()
		{
			ToggleShipmentDirection(true);
			Shipment.DocsAndCartage.JP_DeliveryCartageAdvised = new ZDateTime(2007, 1, 1, 20, 20, 30);
			Shipment.JS_OH_ImportBroker = Factory.New<JASOrgHeader>().PK;
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_JS = Shipment.PK;
			declaration.CustomsEntryHeaders.AddNew();
			declaration.LogCustomsCommencedIfNeeded();
			declaration.LogCustomsClearedIfNeeded();
			Exporter.CheckAndCreateGsumLinesToBeSent();
			SetExpectedMessageLines(new GSUMLine[] { new GSUMLine(ShipmentWrapper, "OFD", Shipment.DocsAndCartage.JP_DeliveryCartageAdvised), new GSUMLine(ShipmentWrapper, "DTO", ZDateTime.Now), new GSUMLine(ShipmentWrapper, "CUS", declaration.LogsOfDeclarationOrShipment.MostRecentLogByEventTime(Events.CustomsCommenced).SL_EventTime), new GSUMLine(ShipmentWrapper, "CLR", declaration.LogsOfDeclarationOrShipment.MostRecentLogByEventTime(Events.CustomsCleared).SL_EventTime) });
			AssertExportedMessage(Exporter);
		}

		public void TestCheckAndCreateGsumLinesToBeSent_HasImportEventsButNonImportShipment()
		{
			ToggleShipmentDirection(false);
			Shipment.DocsAndCartage.JP_DeliveryCartageAdvised = new ZDateTime(2007, 1, 1, 20, 20, 30);
			Shipment.JS_OH_ImportBroker = Factory.New<JASOrgHeader>().PK;
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = Shipment.PK;
			declaration.CustomsEntryHeaders.AddNew();
			declaration.LogCustomsCommencedIfNeeded();
			declaration.LogCustomsClearedIfNeeded();
			Exporter.CheckAndCreateGsumLinesToBeSent();
			AssertExportedMessage(Exporter);
		}

		public void TestCheckAndCreateGsumLinesToBeSent_NewImportShipment()
		{
			JASForwardingShipment shipment = CreateTestShipment();
			ToggleShipmentDirection(shipment, true);
			shipment.DocsAndCartage.JP_DeliveryCartageAdvised = new ZDateTime(2007, 1, 1, 20, 20, 30);
			shipment.JS_OH_ImportBroker = Factory.New<JASOrgHeader>().PK;
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;
			declaration.CustomsEntryHeaders.AddNew();
			declaration.LogCustomsCommencedIfNeeded();
			declaration.LogCustomsClearedIfNeeded();
			GsumShipmentWrapper shipmentWrapper = new GsumShipmentWrapper(shipment);
			GsumMessageExporter exporter = new GsumMessageExporter(shipment);
			exporter.CheckAndCreateGsumLinesToBeSent();
			SetExpectedMessageLines(new GSUMLine[] { new GSUMLine(shipmentWrapper, "OFD", shipment.DocsAndCartage.JP_DeliveryCartageAdvised), new GSUMLine(shipmentWrapper, "DTO", ZDateTime.Now), new GSUMLine(shipmentWrapper, "CUS", declaration.LogsOfDeclarationOrShipment.MostRecentLogByEventTime(Events.CustomsCommenced).SL_EventTime), new GSUMLine(shipmentWrapper, "CLR", declaration.LogsOfDeclarationOrShipment.MostRecentLogByEventTime(Events.CustomsCleared).SL_EventTime) });
			AssertExportedMessage(exporter);
		}

		public void TestCheckAndCreateGsumLinesToBeSent_NewExportShipment()
		{
			JASForwardingShipment shipment = CreateTestShipment();
			ToggleShipmentDirection(shipment, false);
			shipment.JS_A_RCV = new ZDateTime(2007, 1, 1, 20, 20, 30);
			GsumShipmentWrapper shipmentWrapper = new GsumShipmentWrapper(shipment);
			SetExpectedMessageLines(new GSUMLine(shipmentWrapper, "RSH", shipment.JS_A_RCV));
			GsumMessageExporter exporter = new GsumMessageExporter(shipment);
			exporter.CheckAndCreateGsumLinesToBeSent();
			AssertExportedMessage(exporter);
		}

		void ToggleShipmentDirection(bool import)
		{
			ToggleShipmentDirection(Shipment, import);
		}

		void ToggleShipmentDirection(JASForwardingShipment shipment, bool import)
		{
			if (import)
			{
				shipment.JS_RL_NKOrigin = "SGSIN";
				shipment.JS_RL_NKDestination = "AUBNE";
				ExpectedMessageExporter.HeaderData.FreightDest = "AUBNE";
			}
			else
			{
				shipment.JS_RL_NKOrigin = "AUBNE";
				shipment.JS_RL_NKDestination = "SGSIN";
				ExpectedMessageExporter.HeaderData.FreightDest = "SGSIN";
			}
		}

		void SetExpectedMessageLines(params GSUMLine[] lines)
		{
			if (lines.Length > 0)
			{
				ExpectedMessageExporter.ExpectedMessageFileNamesAndContentLines = new JXCMessageExporter.MessageFileNameAndContents[] { new JXCMessageExporter.MessageFileNameAndContents("HB1234.txt", lines) };
			}
			else
			{
				ExpectedMessageExporter.ExpectedMessageFileNamesAndContentLines = System.Array.Empty<JXCMessageExporter.MessageFileNameAndContents>();
			}
		}

		ZGuid initialProxyOrgPK;
		protected override void SetUp()
		{
			initialProxyOrgPK = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			base.SetUp();
			ExpectedMessageExporter.HeaderData.FreightDest = "AUBNE";
			ExpectedMessageExporter.HeaderData.SetSendingForwarder("USATL", "USATL");
			ExpectedMessageExporter.HeaderData.SetDestinationForwarder("AUMEL", "AUMEL");
			SetExpectedMessageLines();
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "1234567890123456789012345678901234567890";
			org.MainAddress.OA_Address1 = "2345678901234567890123456789012345678901";
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = org.PK;
			JASOrgHeader currentOrgProxy = (JASOrgHeader)org;
			currentOrgProxy.OfficeCode = "USATL";
			currentOrgProxy.NettingCode = "USATL";
			object lazyLoadShipment = Shipment;
			Factory.Save();
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = initialProxyOrgPK;
		}

		public void TestClearCurrentGsumEvents()
		{
			AssertEquals("Pre-condition", 0, Exporter.GsumLinesToBeSentForTesting.Count);
			Exporter.GsumLinesToBeSentForTesting.Add(new GSUMLine(new GsumShipmentWrapper(Shipment), "XXX", ZDateTime.Now));
			AssertEquals(1, Exporter.GsumLinesToBeSentForTesting.Count);
			Exporter.ClearCurrentGsumEvents();
			AssertEquals("Should be cleared", 0, Exporter.GsumLinesToBeSentForTesting.Count);
		}

		GsumMessageExporter Exporter
		{
			get
			{
				if (fExporter == null)
				{
					fExporter = new GsumMessageExporter(Shipment);
				}

				return fExporter;
			}
		}

		GsumShipmentWrapper ShipmentWrapper
		{
			get
			{
				if (fShipmentWrapper == null)
				{
					fShipmentWrapper = new GsumShipmentWrapper(Shipment);
				}

				return fShipmentWrapper;
			}
		}

		JASForwardingShipment CreateTestShipment()
		{
			JASForwardingShipment result = Factory.NewWithValidTestData<JASForwardingShipment>();
			result.JS_RL_NKDestination = "AUBNE";
			result.JS_HouseBill = "HB 1234%$$%$% ";
			JASForwardingConsol consol = (JASForwardingConsol)result.Consols.AddNew();
			consol.FillWithValidTestData();
			consol.SetDefaultSendingForwarderAddress(Factory.NewWithValidTestData<JASOrgHeader>());
			consol.SendingForwarder.NettingCode = "USATL";
			consol.SendingForwarder.OfficeCode = "USATL";
			consol.SetDefaultReceivingForwarderAddress(Factory.NewWithValidTestData<JASOrgHeader>());
			consol.ReceivingForwarder.NettingCode = "AUMEL";
			consol.ReceivingForwarder.OfficeCode = "AUMEL";
			return result;
		}

		JASForwardingShipment Shipment
		{
			get
			{
				if (fShipment == null)
				{
					fShipment = CreateTestShipment();
				}

				return fShipment;
			}
		}

		GsumMessageExporter fExporter;
		GsumShipmentWrapper fShipmentWrapper;
		JASForwardingShipment fShipment;
	}
}
