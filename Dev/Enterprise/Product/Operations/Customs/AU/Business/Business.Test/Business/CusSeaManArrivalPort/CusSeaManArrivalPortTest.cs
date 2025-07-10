using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSeaManArrivalPort))]
	public class CusSeaManArrivalPortTest : Customs.Business.Testing.CusSeaManArrivalPortTest
	{
		public void TestLookups()
		{
			AssertEquals(typeof(CusSeaManArrivalPortLookups), port.Lookups.GetType());
		}

		public void TestIsFirstArrivalExclusive()
		{
			CusSeaManTranHead header = Factory.New<CusSeaManTranHead>();
			CusSeaManArrivalPort port = header.Arrivals.AddNew();
			CusSeaManArrivalPort port2 = header.Arrivals.AddNew();

			AssertEquals("by default Port value", false, port.BA_IsFirstArrival);
			AssertEquals("by default Port2 value", false, port2.BA_IsFirstArrival);

			port2.BA_IsFirstArrival = true;

			AssertEquals("when Port 2 set true - Port value", false, port.BA_IsFirstArrival);
			AssertEquals("when Port 2 set true - Port2 value", true, port2.BA_IsFirstArrival);

			CusSeaManArrivalPort port3 = header.Arrivals.AddNew();

			AssertEquals("when Port 2 set true and port3 added - Port3 value", false, port3.BA_IsFirstArrival);

			port3.BA_IsFirstArrival = true;

			AssertEquals("when Port 3 also set true - Port value", false, port.BA_IsFirstArrival);
			AssertEquals("when Port 3 also set true - Port2 value", false, port2.BA_IsFirstArrival);
			AssertEquals("when Port 3 also set true - Port3 value", true, port3.BA_IsFirstArrival);

			port3.BA_IsFirstArrival = false;

			AssertEquals("when Port 2 set false - Port value", false, port.BA_IsFirstArrival);
			AssertEquals("when Port 2 set false - Port2 value", false, port2.BA_IsFirstArrival);
			AssertEquals("when Port 2 set false - Port3 value", false, port3.BA_IsFirstArrival);
		}

		public void TestValidation()
		{
			AssertEquals("expected derived type", typeof(CusSeaManArrivalPortValidation), port.Validation.GetType());
		}

		public void TestActualArrivalResponseStatus()
		{
			port.ActualArrivalResponseStatus.Code = CMRBaseStatuses.Codes.OriginalAccepted;

			AssertEquals("ActualArrivalResponseStatus.StatusCode should be same as CMRAcceptedRejectedList.Codes.Accepted", CMRBaseStatuses.Codes.OriginalAccepted, port.ActualArrivalResponseStatus.Code);
			AssertEquals("", port.Lookups.ActualArrivalStatusList.GetDescriptionFromCode(CMRBaseStatuses.Codes.OriginalAccepted), port.ActualArrivalResponseStatus.Description);
		}

		public void TestActualArrivalStatusCalculator()
		{
			AssertNotNull("Calculator", port.ActualArrivalStatusCalculator);
		}

		public void TestCargoListStatusCalculator()
		{
			AssertNotNull("Calculator", port.CargoListStatusCalculator);
		}

		public void TestCargoListStatus()
		{
			AssertNotNull(port.CargoListStatus);
		}

		public void TestActualArrivalStatusStartsAsNotSent()
		{
			AssertEquals("Status Code", CMRBaseStatuses.Codes.NotSent, port.ActualArrivalResponseStatus.Code);
		}

		public void TestCargoListStatusStartsAsNotSent()
		{
			AssertEquals("Status Code", CMRBaseStatuses.Codes.NotSent, port.CargoListStatus.Code);
		}

		public void TestICMRMessageRespondeeDetails()
		{
			transportHeader.BT_VoyageNum = "12345";
			transportHeader.BT_VesselName = "ADMIRALENGRACHT";
			port.BA_RL_NKArrivalPort = "AUSYD";
			AssertEquals("Details", "Vessel: ADMIRALENGRACHT\r\nVoyage: 12345\r\nArrival Port: AUSYD\r\n", ((ICMRMessageRespondee)port).Details);
		}

		public void TestICMRMessageRespondeeShortDescription()
		{
			transportHeader.BT_VoyageNum = "12345";
			port.BA_RL_NKArrivalPort = "AUSYD";
			AssertEquals("ShortDescription", "Voyage: 12345 Arrival Port: AUSYD", ((ICMRMessageRespondee)port).ShortDescription);
		}

		public void TestCargoLines()
		{
			AssertNotNull(port.CargoLines);
			AssertEquals("type", typeof(CusSeaManOBLHeaderCargoLineCollection), port.CargoLines.GetType());
		}

		public void TestCargoLinesReadOnly()
		{
			AssertEquals("cargolines readonly by default", false, port.CargoLines.ReadOnly);

			port.BA_RL_NKArrivalPort = "AUSYD";
			AssertEquals("when set", false, port.CargoLines.ReadOnly);

			port.BA_RL_NKArrivalPort = ZString.Empty;
			AssertEquals("when set blank", false, port.CargoLines.ReadOnly);

			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CusSeaManArrivalPort loadedPort = factory2.Load<CusSeaManArrivalPort>(port.PK);
			AssertEquals("when loaded", false, loadedPort.CargoLines.ReadOnly);
		}

		public void TestArrivalPort()
		{
			AssertEquals("by default", ZString.Empty, port.BA_RL_NKArrivalPort);

			port.BA_RL_NKArrivalPort = "AUSYD";
			AssertEquals("when set", "AUSYD", port.BA_RL_NKArrivalPort);

			CusSeaManOBLHeaderCargoLine line = port.CargoLines.AddNew();
			AssertEquals("on added cargo line", "AUSYD", line.BO_RL_NKDischargePort);

			port.BA_RL_NKArrivalPort = ZString.Empty;
			AssertEquals("blanked with cargolines", ZString.Empty, port.BA_RL_NKArrivalPort);
			AssertEquals("blanked with cargolines on cargoline", ZString.Empty, line.BO_RL_NKDischargePort);

			port.BA_RL_NKArrivalPort = "AUMEL";
			AssertEquals("when changed with cargolines", "AUMEL", port.BA_RL_NKArrivalPort);
			AssertEquals("when changed with cargolines on cargoline", "AUMEL", line.BO_RL_NKDischargePort);
		}

		public new void TestHeader()
		{
			AssertEquals(typeof(CusSeaManTranHead), port.Header.GetType());
			AssertEquals(transportHeader, port.Header);
		}

		public void TestCargoLinesAreRegisteredEditableChildren()
		{
			port.CargoLines.AddNew();
			Assert("CargoLines should be registered editable", port.IsRegisteredEditableChildObject(port.CargoLines));
		}

		public void TestStevedoreID()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			port.BA_OA_CTOAddress = header.Addresses[0].PK;
			port.CTOAddress.Header.PrimaryRegistrationNumber.Number = "54321";
			AssertEquals("StevadoreID", "54321", port.StevedoreID);
		}

		public void TestStevedoreIDInfo()
		{
			AssertNotNull("nullness", port.StevedoreIDInfo);
			AssertEquals("name", CusSeaManArrivalPort.Schema.StevedoreID, port.StevedoreIDInfo.Name);
		}

		public void TestCTOEstablishmentID()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			port.BA_OA_CTOAddress = header.Addresses[0].PK;
			port.CTOAddress.LocalControlledPremisesID = "12345";
			AssertEquals("DischargeCTOID", "12345", port.CTOEstablishmentID);
		}

		public void TestCTOEstablishmentIDInfo()
		{
			AssertNotNull("nullness", port.CTOEstablishmentIDInfo);
			AssertEquals("name", CusSeaManArrivalPort.Schema.CTOEstablishmentID, port.CTOEstablishmentIDInfo.Name);
		}

		public void TestCTOAddressDataRefresh()
		{
			OrgHeader party = OrgHeader.LoadFromCode(Factory, "ABABEU");
			party.PrimaryRegistrationNumber.Number = "54321";
			AssertNotNull("PreCondition: this party exists", party);

			var partyAddress = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.OA_OH, party.PK));
			AssertNotNull("PreCondition: this party address exists", partyAddress);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CusSeaManTranHead tranHead = factory2.NewWithValidTestData<CusSeaManTranHead>();
			TestHelper port = factory2.New<TestHelper>();
			tranHead.Arrivals.Add(port);
			AssertEquals("precondition hitcount", 0, port.HitCount);

			port.BA_OA_CTOAddress = partyAddress.PK;
			AssertEquals("initial abn", "54321", port.CTOAddress.Header.PrimaryRegistrationNumber.Number);
			AssertEquals("initial hitcount", 0, port.HitCount);

			party.PrimaryRegistrationNumber.Number = "12345";
			Factory.Save();
			AssertEquals("abn change", 2, port.HitCount);
		}

		public void TestActualArrivalMessages()
		{
			AssertNotNull(port.ActualArrivalMessages);
			AssertEquals(typeof(EDIMessageCollectionView), port.ActualArrivalMessages.GetType());

			EDIMessage testMessage = port.Messages.AddNew();
			testMessage.EM_MessageType = CMRMessage.CMRMessageTypes.SEAAAR;
			EDIMessage testMessage2 = port.Messages.AddNew();
			testMessage2.EM_MessageType = CMRMessage.CMRMessageTypes.CARLST;

			AssertCollectionContains(testMessage, port.ActualArrivalMessages);
			AssertCollectionNotContains(testMessage2, port.ActualArrivalMessages);
		}

		public void TestCargoListAndLineMessagesCombined()
		{
			var seaaarMessage = Factory.New<CMRSEAAARRMessage>();
			port.Messages.Add(seaaarMessage);

			var carlstMessage = Factory.New<CMRCARLSTRMessage>();
			port.Messages.Add(carlstMessage);

			var cargoLine1 = port.CargoLines.AddNew();
			var lineMessage1 = Factory.New<CMRCARSTMessage>();
			cargoLine1.Messages.Add(lineMessage1);

			var cargoLine2 = port.CargoLines.AddNew();
			var lineMessage2 = Factory.New<CMRCARSTMessage>();
			cargoLine2.Messages.Add(lineMessage2);
			Factory.Save();

			AssertCollectionNotContains(seaaarMessage, port.CargoListAndLineMessagesCombined);
			AssertCollectionContains(carlstMessage, port.CargoListAndLineMessagesCombined);
			AssertCollectionContains(lineMessage1, port.CargoListAndLineMessagesCombined);
			AssertCollectionContains(lineMessage2, port.CargoListAndLineMessagesCombined);
		}

		public void TestMultiplePortsCanCoexist()
		{
			CusSeaManArrivalPort port1 = port;
			CusSeaManArrivalPort port2 = transportHeader.Arrivals.AddNew();

			port1.BA_RL_NKArrivalPort = "AUSYD";
			port2.BA_RL_NKArrivalPort = "AUSYD";

			AssertEquals("AUSYD", port1.BA_RL_NKArrivalPort);
			AssertEquals("AUSYD", port2.BA_RL_NKArrivalPort);
		}

		public void TestDateTimeOfArrivalUTC()
		{
			port.BA_ArrivalPortETA = new ZDateTime(2005, 6, 5, 16, 52, 0);
			port.BA_ArrivalPortATA = new ZDateTime(2005, 6, 7, 17, 53, 0);
			port.BA_RL_NKArrivalPort = "AUSYD";
			AssertEquals("EstimatedDateTimeOfArrivalUTC", new ZDateTime(2005, 6, 5, 6, 52, 0), port.EstimatedDateTimeOfArrivalUTC);
			AssertEquals("ActualDateTimeOfArrivalUTC", new ZDateTime(2005, 6, 7, 7, 53, 0), port.ActualDateTimeOfArrivalUTC);
			port.BA_RL_NKArrivalPort = ZString.Empty;
			AssertEquals("EstimatedDateTimeOfArrivalUTC", ZDateTime.Empty, port.EstimatedDateTimeOfArrivalUTC);
			AssertEquals("ActualDateTimeOfArrivalUTC", ZDateTime.Empty, port.ActualDateTimeOfArrivalUTC);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<CusSeaManTranHead>().Arrivals.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();

			transportHeader = Factory.New<CusSeaManTranHead>();
			port = transportHeader.Arrivals.AddNew();
		}

		CusSeaManTranHead transportHeader;
		CusSeaManArrivalPort port;

		#region TestHelper

		class TestHelper : CusSeaManArrivalPort
		{
			public TestHelper(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void CTOAddressUpdatedHandler(object @object, EventArgs args)
			{
				HitCount++;
			}

			public int HitCount;
		}

		#endregion

		#endregion
	}
}
