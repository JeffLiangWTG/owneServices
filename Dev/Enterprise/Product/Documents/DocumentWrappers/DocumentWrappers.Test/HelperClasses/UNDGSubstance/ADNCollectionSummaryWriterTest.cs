using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class ADNCollectionSummaryWriterTest : TestCaseWithFactory
	{
		public void TestGetSummary_Sea() => TestGetSummary(Constants.TransportModes.Sea);
		public void TestGetSummary_InlandWaterway() => TestGetSummary(Constants.TransportModes.InlandWaterwayTransport);

		void TestGetSummary(string legTransportMode)
		{
			var shipment = Factory.New<ForwardingShipment>();
			AddTransportLegToShipment(shipment, Constants.VesselType.Barge, legTransportMode, adnLoadPort, adnDischargePort);
			CreateUNDGSubstanceADNInFactory("777");

			var undgDataItem = CreateUNDGDataItemFromShipment(shipment, "777", "ADN");
			var wrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);

			var adnUNDGs = new List<UNDGSubstanceWrapper>();
			adnUNDGs.Add(wrapper);

			var writer = new ADNCollectionSummaryWriter() as IUNDGSubstanceCollectionSummaryWriter;
			var summary = writer.GetSummary(adnUNDGs);
			AssertEquals("Should be empty for ADN", ZString.Empty, summary);

			AddNoteToShipment(shipment, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "Hello");
			AddNoteToShipment(shipment, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "World");

			ZString expectedSummary = "Hello" + System.Environment.NewLine + "World";
			summary = writer.GetSummary(adnUNDGs);

			AssertEquals("Should have ADN special handling info", expectedSummary, summary);
		}

		public void TestGetSummaryWithMultipleParentShipments_Sea() => TestGetSummaryWithMultipleParentShipments(Constants.TransportModes.Sea);
		public void TestGetSummaryWithMultipleParentShipments_InlandWaterway() => TestGetSummaryWithMultipleParentShipments(Constants.TransportModes.InlandWaterwayTransport);

		void TestGetSummaryWithMultipleParentShipments(string legTransportMode)
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			AddTransportLegToShipment(shipment1, Constants.VesselType.Barge, "AAA", legTransportMode, adnLoadPort, adnDischargePort);
			CreateUNDGSubstanceADNInFactory("420");
			var undgDataItem1 = CreateUNDGDataItemFromShipment(shipment1, "420", UNDGSubstanceStandardTypes.ADN);

			var shipment2 = Factory.New<ForwardingShipment>();
			AddTransportLegToShipment(shipment2, Constants.VesselType.BulkCarrier, "BBB", legTransportMode, adnLoadPort, adnDischargePort);
			CreateUNDGSubstanceADNInFactory("696");
			var undgDataItem2 = CreateUNDGDataItemFromShipment(shipment2, "696", UNDGSubstanceStandardTypes.ADN);

			var shipment3 = Factory.New<ForwardingShipment>();
			AddTransportLegToShipment(shipment3, Constants.VesselType.Barge, "CCC", legTransportMode, adnLoadPort, adnDischargePort);
			CreateUNDGSubstanceADNInFactory("700");
			var undgDataItem3 = CreateUNDGDataItemFromShipment(shipment3, "700", UNDGSubstanceStandardTypes.ADN);

			var writer = new ADNCollectionSummaryWriter() as IUNDGSubstanceCollectionSummaryWriter;

			var summary = writer.GetSummary(new List<UNDGSubstanceWrapper>());
			AssertEquals("Should be empty when no substances provided.", string.Empty, summary);

			var adnUNDGs = new List<UNDGSubstanceWrapper>
			{
				new UNDGSubstanceWrapper(undgDataItem1, Factory),
				new UNDGSubstanceWrapper(undgDataItem2, Factory),
				new UNDGSubstanceWrapper(undgDataItem3, Factory)
			};

			summary = writer.GetSummary(adnUNDGs);
			AssertEquals("Should be empty when no notes.", string.Empty, summary);

			AddNoteToShipment(shipment2, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "Blebbity");
			summary = writer.GetSummary(adnUNDGs);
			AssertEquals("Notes on shipments that do not meet ADN requirements should be excluded.", string.Empty, summary);

			AddNoteToShipment(shipment1, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "Shlebbity");
			AddNoteToShipment(shipment3, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "Plebbity");
			summary = writer.GetSummary(adnUNDGs);
			AssertEquals("Notes from all valid parent shipments should be included.", "Shlebbity" + System.Environment.NewLine + "Plebbity", summary);
		}

		public void TestGetSummaryWithDuplicateParentShipments_Sea() => TestGetSummaryWithDuplicateParentShipments(Constants.TransportModes.Sea);
		public void TestGetSummaryWithDuplicateParentShipments_InlandWaterway() => TestGetSummaryWithDuplicateParentShipments(Constants.TransportModes.InlandWaterwayTransport);

		void TestGetSummaryWithDuplicateParentShipments(string legTransportMode)
		{
			var shipment = Factory.New<ForwardingShipment>();
			AddTransportLegToShipment(shipment, Constants.VesselType.Barge, "AAA", legTransportMode, adnLoadPort, adnDischargePort);

			CreateUNDGSubstanceADNInFactory("420");
			var undgDataItem1 = CreateUNDGDataItemFromShipment(shipment, "420", UNDGSubstanceStandardTypes.ADN);

			CreateUNDGSubstanceADNInFactory("696");
			var undgDataItem2 = CreateUNDGDataItemFromShipment(shipment, "696", UNDGSubstanceStandardTypes.ADN);

			CreateUNDGSubstanceADNInFactory("700");
			var undgDataItem3 = CreateUNDGDataItemFromShipment(shipment, "700", UNDGSubstanceStandardTypes.ADN);

			var writer = new ADNCollectionSummaryWriter() as IUNDGSubstanceCollectionSummaryWriter;

			var summary = writer.GetSummary(new List<UNDGSubstanceWrapper>());
			AssertEquals("Should be empty when no substances provided.", string.Empty, summary);

			var adnUNDGs = new List<UNDGSubstanceWrapper>
			{
				new UNDGSubstanceWrapper(undgDataItem1, Factory),
				new UNDGSubstanceWrapper(undgDataItem2, Factory),
				new UNDGSubstanceWrapper(undgDataItem3, Factory)
			};

			summary = writer.GetSummary(adnUNDGs);
			AssertEquals("Should be empty when no notes.", string.Empty, summary);

			AddNoteToShipment(shipment, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "Shlebbity");
			summary = writer.GetSummary(adnUNDGs);
			AssertEquals("The shipment's notes should only be included once.", "Shlebbity", summary);
		}

		public void TestGetSummaryReturnEmptyWhenTransportModeIsNotSeaOrIWT()
		{
			var shipment = Factory.New<ForwardingShipment>();
			AddTransportLegToShipment(shipment, Constants.VesselType.Barge, Constants.TransportModes.Rail, adnLoadPort, adnDischargePort);
			CreateUNDGSubstanceADNInFactory("777");

			var undgDataItem = CreateUNDGDataItemFromShipment(shipment, "777", "ADN");
			AddNoteToShipment(shipment, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "Hello");
			AddNoteToShipment(shipment, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "World");

			var adnUNDGs = new List<UNDGSubstanceWrapper>();
			var wrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
			adnUNDGs.Add(wrapper);

			var writer = new ADNCollectionSummaryWriter() as IUNDGSubstanceCollectionSummaryWriter;
			var summary = writer.GetSummary(adnUNDGs);
			AssertEquals("Should be empty for ADN", ZString.Empty, summary);
		}

		public void TestGetSummaryReturnEmptyWhenLoadPortIsNotADN()
		{
			var shipment = Factory.New<ForwardingShipment>();
			AddTransportLegToShipment(shipment, Constants.VesselType.Barge, Constants.TransportModes.Sea, nonADNLoadPort, adnDischargePort);
			CreateUNDGSubstanceADNInFactory("777");

			var undgDataItem0 = CreateUNDGDataItemFromShipment(shipment, "777", "ADN");
			AddNoteToShipment(shipment, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "Hello");

			var adnUNDGs = new List<UNDGSubstanceWrapper>();
			var wrapper = new UNDGSubstanceWrapper(undgDataItem0, Factory);
			adnUNDGs.Add(wrapper);

			var writer = new ADNCollectionSummaryWriter() as IUNDGSubstanceCollectionSummaryWriter;
			var summary = writer.GetSummary(adnUNDGs);
			AssertEquals("Should be empty for ADN", ZString.Empty, summary);
		}

		public void TestGetSummaryReturnEmptyWhenDischargePortIsNotADN()
		{
			var shipment = Factory.New<ForwardingShipment>();
			AddTransportLegToShipment(shipment, Constants.VesselType.Barge, Constants.TransportModes.Sea, adnLoadPort, nonADNDischargePort);
			CreateUNDGSubstanceADNInFactory("777");

			var undgDataItem = CreateUNDGDataItemFromShipment(shipment, "777", "ADN");
			AddNoteToShipment(shipment, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "Hello");

			var adnUNDGs = new List<UNDGSubstanceWrapper>();
			var wrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
			adnUNDGs.Add(wrapper);

			var writer = new ADNCollectionSummaryWriter() as IUNDGSubstanceCollectionSummaryWriter;
			var summary = writer.GetSummary(adnUNDGs);
			AssertEquals("Should be empty for ADN", ZString.Empty, summary);
		}

		public void TestGetSummaryReturnEmptyWhenVesselIsNotBarge()
		{
			var shipment = Factory.New<ForwardingShipment>();
			AddTransportLegToShipment(shipment, Constants.VesselType.BulkCarrier, Constants.TransportModes.Sea, adnLoadPort, adnDischargePort);
			CreateUNDGSubstanceADNInFactory("777");

			var undgDataItem = CreateUNDGDataItemFromShipment(shipment, "777", "ADN");
			AddNoteToShipment(shipment, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "Hello");

			var adnUNDGs = new List<UNDGSubstanceWrapper>();
			var wrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
			adnUNDGs.Add(wrapper);

			var writer = new ADNCollectionSummaryWriter() as IUNDGSubstanceCollectionSummaryWriter;
			var summary = writer.GetSummary(adnUNDGs);

			AssertEquals("Should be empty for ADN", ZString.Empty, summary);
		}

		[ExpectNoExceptions]
		public void TestGetSummaryReturnEmptyWhenVesselIsNull()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var transportLeg = shipment.Transports.AddNew();
			transportLeg.JW_TransportMode = Constants.TransportModes.Sea;
			transportLeg.JW_RL_NKLoadPort = adnLoadPort;
			transportLeg.JW_RL_NKDiscPort = adnDischargePort;

			CreateUNDGSubstanceADNInFactory("777");

			var undgDataItem = CreateUNDGDataItemFromShipment(shipment, "777", "ADN");
			AddNoteToShipment(shipment, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "Hello");

			var adnUNDGs = new List<UNDGSubstanceWrapper>();
			var wrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
			adnUNDGs.Add(wrapper);

			var writer = new ADNCollectionSummaryWriter() as IUNDGSubstanceCollectionSummaryWriter;

			AssertEquals("Should be empty for ADN as the transport had no vessel", ZString.Empty, writer.GetSummary(adnUNDGs));
		}

		public void TestGetSummaryReturnEmptyWhenNoADNSubstance()
		{
			var shipment = Factory.New<ForwardingShipment>();
			AddTransportLegToShipment(shipment, Constants.VesselType.Barge, Constants.TransportModes.Sea, adnLoadPort, adnDischargePort);

			var ridSubstance = Factory.New<UNDGSubstanceRID>();
			ridSubstance.RID_UNNO = "777";
			Factory.Save();

			var undgDataItem = CreateUNDGDataItemFromShipment(shipment, "777", "RID");
			AddNoteToShipment(shipment, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "Hello");

			var adnUNDGs = new List<UNDGSubstanceWrapper>();
			var wrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
			adnUNDGs.Add(wrapper);

			var writer = new ADNCollectionSummaryWriter() as IUNDGSubstanceCollectionSummaryWriter;
			var summary = writer.GetSummary(adnUNDGs);

			AssertEquals("Should be empty for ADN", ZString.Empty, summary);
		}

		public void TestGetSummaryReturnEmptyWhenNoteIsNotDangerousGoodsAdditionalHandlingInformation()
		{
			var shipment = Factory.New<ForwardingShipment>();
			AddTransportLegToShipment(shipment, Constants.VesselType.Barge, Constants.TransportModes.Sea, adnLoadPort, adnDischargePort);
			CreateUNDGSubstanceADNInFactory("777");

			var undgDataItem = CreateUNDGDataItemFromShipment(shipment, "777", "ADN");
			AddNoteToShipment(shipment, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, "Hello");

			var adnUNDGs = new List<UNDGSubstanceWrapper>();
			var wrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
			adnUNDGs.Add(wrapper);

			var writer = new ADNCollectionSummaryWriter() as IUNDGSubstanceCollectionSummaryWriter;
			var summary = writer.GetSummary(adnUNDGs);

			AssertEquals("Should be empty for ADN", ZString.Empty, summary);
		}

		void AddNoteToShipment(ForwardingShipment shipment, ZString description, ZString noteText)
		{
			var note = shipment.Notes.AddNew();
			note.ST_Description = description;
			note.ST_NoteText = noteText;
		}

		UNDGDataItem CreateUNDGDataItemFromShipment(ForwardingShipment shipment, ZString unno, ZString standard)
		{
			var dgSubstance = UNDGSubstanceLoader.LoadSubstances(Factory, unno, standard: standard).First();
			var packline = shipment.OuterPackLines.AddNew();

			var undgDataItem = packline.UNDGs.AddNew();
			undgDataItem.LinkDefault(dgSubstance);
			return undgDataItem;
		}

		void AddTransportLegToShipment(ForwardingShipment shipment, ZString vesselType, ZString transportMode, ZString loadPort, ZString dischargePort)
		{
			AddTransportLegToShipment(shipment, vesselType, "XXX", transportMode, loadPort, dischargePort);
		}

		void AddTransportLegToShipment(ForwardingShipment shipment, ZString vesselType, ZString vesselCode, ZString transportMode, ZString loadPort, ZString dischargePort)
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = vesselCode;
			vessel.RV_VesselType = vesselType;

			var transportLeg = shipment.Transports.AddNew();
			transportLeg.JW_TransportMode = transportMode;
			transportLeg.JW_Vessel = vessel.RV_Code;
			transportLeg.JW_RL_NKLoadPort = loadPort;
			transportLeg.JW_RL_NKDiscPort = dischargePort;

			Factory.Save();
		}

		void CreateUNDGSubstanceADNInFactory(string unno)
		{
			var adnSubstance = Factory.New<UNDGSubstanceADN>();
			adnSubstance.ADN_UNNO = unno;

			Factory.Save();
		}

		readonly string adnLoadPort = Constants.CountryCodes.Austria;
		readonly string adnDischargePort = Constants.CountryCodes.Belgium;
		readonly string nonADNLoadPort = Constants.CountryCodes.Afghanistan;
		readonly string nonADNDischargePort = Constants.CountryCodes.Bahamas;
	}
}
