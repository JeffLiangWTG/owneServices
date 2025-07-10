using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public abstract class JobDeclarationSynchroniserTest : TestCaseWithFactory
	{
		public void TestPackingModeSynchroniser_Format()
		{
			TestSyncForOneDirection("EXP", Constants.TransportModes.Sea);
			TestSyncForOneDirection("IMP", Constants.TransportModes.Sea);
			TestSyncForOneDirection("EXP", Constants.TransportModes.Air);
			TestSyncForOneDirection("IMP", Constants.TransportModes.Air);
			TestSyncForOneDirection("EXP", Constants.TransportModes.Rail);
			TestSyncForOneDirection("IMP", Constants.TransportModes.Rail);
			TestSyncForOneDirection("EXP", Constants.TransportModes.Road);
			TestSyncForOneDirection("IMP", Constants.TransportModes.Road);

			TestForFakeAustralianContainerModesConsol(Constants.ContainerModes.BuyersConsol);
			TestForFakeAustralianContainerModesConsol(Constants.ContainerModes.AgentConsol);
		}

		void TestForFakeAustralianContainerModesConsol(string shipmentPackingMode)
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "XXYYY";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_PackingMode = shipmentPackingMode;
			var decContModeForBcnAir = GetSynchronisedDec(shipment).JE_ContainerMode;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = shipmentPackingMode;
			var decContModeForBcnSea = GetSynchronisedDec(shipment).JE_ContainerMode;
			AssertEquals("NCT", decContModeForBcnAir);
			AssertEquals("CNT", decContModeForBcnSea);
		}

		void TestSyncForOneDirection(string direction, string transportMode)
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = transportMode;
			if (direction == "IMP")
			{
				shipment.JS_RL_NKOrigin = "XXYYY";
			}
			else
			{
				shipment.JS_RL_NKDestination = "XXYYY";
			}

			Dictionary<ZString, ZString> modesMapingsFromShipmentToDeclaration = new Dictionary<ZString, ZString>();
			modesMapingsFromShipmentToDeclaration.Add(Constants.ContainerModes.FCL, Constants.ContainerModes.FCL);
			modesMapingsFromShipmentToDeclaration.Add(Constants.ContainerModes.LCL, Constants.ContainerModes.LCL);
			modesMapingsFromShipmentToDeclaration.Add(Constants.ContainerModes.BreakBulk, Constants.ContainerModes.BreakBulk);
			modesMapingsFromShipmentToDeclaration.Add(Constants.ContainerModes.Bulk, Constants.ContainerModes.Bulk);
			modesMapingsFromShipmentToDeclaration.Add(Constants.ContainerModes.Liquid, Constants.ContainerModes.Liquid);
			modesMapingsFromShipmentToDeclaration.Add(Constants.ContainerModes.Loose, Constants.ContainerModes.Loose);
			modesMapingsFromShipmentToDeclaration.Add(Constants.ContainerModes.ULD, Constants.ContainerModes.ULD);
			modesMapingsFromShipmentToDeclaration.Add(Constants.ContainerModes.RollOnRollOff, Constants.ContainerModes.RollOnRollOff);
			modesMapingsFromShipmentToDeclaration.Add(Constants.ContainerModes.FTL, Constants.ContainerModes.FTL);
			modesMapingsFromShipmentToDeclaration.Add(Constants.ContainerModes.LTL, Constants.ContainerModes.LTL);
			modesMapingsFromShipmentToDeclaration.Add(Constants.ContainerModes.Containerised, Constants.ContainerModes.Containerised);

			foreach (ZString containerModeInShipment in modesMapingsFromShipmentToDeclaration.Keys)
			{
				shipment.JS_PackingMode = containerModeInShipment;
				ZString containerModeInDeclaration = modesMapingsFromShipmentToDeclaration[containerModeInShipment];
				AssertEquals(string.Format("A shipment of transport mode {0} and container mode {1} should make a  declaration of containerised type {2}", transportMode, containerModeInShipment, containerModeInDeclaration),
								containerModeInDeclaration, GetSynchronisedDec(shipment).JE_ContainerMode);
			}

			// All other modes should always make a NonContainerised dec:
			ZString[] modesThatShouldGiveNCT = new ZString[] { "XXX", "YYY", Constants.ContainerModes.AIR, Constants.ContainerModes.Combination, Constants.ContainerModes.Empty, Constants.ContainerModes.FCLMixedShipper, Constants.ContainerModes.FreightAllKind, Constants.ContainerModes.Groupage, Constants.ContainerModes.Mail, Constants.ContainerModes.OnBoardCourier, Constants.ContainerModes.Other, Constants.ContainerModes.Unaccompanied };
			foreach (ZString containerMode in modesThatShouldGiveNCT)
			{
				shipment.JS_PackingMode = containerMode;
				AssertEquals(string.Format("A shipment of transport mode {0} and containerisation {1} should make a  declaration of packing type NCT", transportMode, containerMode), Constants.ContainerModes.NonContainerised, GetSynchronisedDec(shipment).JE_ContainerMode);
			}
		}

		protected JobDeclaration GetSynchronisedDec(ForwardingShipment shipment, string messageType = "EXP")
		{
			var declaration = GetDeclaration();
			declaration.JE_GB = Factory.NewWithValidTestData<GlbCompany>().Branches.AddNew().PK;
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = messageType;
			var syncher = declaration.ShipmentSynchroniser;
			syncher.Synchronise(new SynchroniseEventArgs(Enterprise.Customs.Business.SynchroniseAction.Force));
			return declaration;
		}

		public void TestAirModeGivesNoMessageErrorOnJE_ContainerMode()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			var declaration = GetDeclaration();
			declaration.JE_JS = shipment.PK;
			var syncher = declaration.ShipmentSynchroniser;
			syncher.Synchronise(new SynchroniseEventArgs(Enterprise.Customs.Business.SynchroniseAction.Force));

			declaration.Validation.ValidateJE_ContainerMode();
			int notificationsCount = 0;
			foreach (object o in declaration.JE_ContainerModeInfo.Notifications)
			{   // Hmmmm, Notifications have a Count property?
				notificationsCount++;
			}
			AssertEquals("JE_ContainerModeInfo ought to have 0 notifications when mode is air", 0, notificationsCount);
		}

		public void TestHouseBills()
		{
			var primaryShipment = Factory.New<ForwardingShipment>();
			primaryShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			primaryShipment.JS_HouseBill = "PRIMARY";
			var secondaryShipment = Factory.New<ForwardingShipment>();
			secondaryShipment.JS_HouseBill = "SECONDARY";
			primaryShipment.CoLoadShipments.Add(secondaryShipment);
			var primaryDeclaration = GetSynchronisedDec(primaryShipment);
			var secondaryDeclaration = GetSynchronisedDec(secondaryShipment);
			AssertEquals("PRIMARY", primaryDeclaration.JE_HouseBill);
			AssertEquals("SECONDARY", secondaryDeclaration.JE_HouseBill);
		}

		public void TestCorrectSynchroniserisReturnedByGetNewShipmentSynchroniser()
		{
			var dec = GetDeclaration();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			dec.JE_JS = shipment.PK;
			var syncher = dec.ShipmentSynchroniser;
			Assert("The JobDeclarationSynchroniser on an EU dec should be an Enterprise.Customs.EU.Business.Declaration.JobDeclarationSynchroniser",
							syncher is JobDeclarationSynchroniser);
		}

		public void TestDeclarantBox14()
		{
			var importBroker = Factory.New<OrgHeader>();
			importBroker.Addresses.AddNewMainAddress();
			var exportBroker = Factory.New<OrgHeader>();
			exportBroker.Addresses.AddNewMainAddress();
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.Addresses.AddNewMainAddress();
			var dec = GetDeclaration();
			dec.Branch.GB_OH_OrgProxy = orgProxy.PK;
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			dec.JE_JS = shipment.PK;
			dec.ShipmentSynchroniser.Synchronise(true);
			AssertEquals(dec.Branch.OrgProxy.MainAddress.PK, dec.Declarant.PK);
			AssertEquals(dec.Branch.OrgProxy.MainAddress.PK, dec.DeclarantOrgAddress.PK);
			shipment.JS_OH_ExportBroker = exportBroker.PK;
			dec.ShipmentSynchroniser.Synchronise(true);
			AssertEquals(exportBroker.MainAddress.PK, dec.Declarant.PK);
			AssertEquals(exportBroker.MainAddress.PK, dec.DeclarantOrgAddress.PK);
			shipment.JS_OH_ImportBroker = importBroker.PK;
			dec.ShipmentSynchroniser.Synchronise(true);
			AssertEquals(exportBroker.MainAddress.PK, dec.Declarant.PK);
			AssertEquals(exportBroker.MainAddress.PK, dec.DeclarantOrgAddress.PK);
			dec.JE_MessageType = "IMP";
			AssertEquals(importBroker.MainAddress.PK, dec.Declarant.PK);
			AssertEquals(importBroker.MainAddress.PK, dec.DeclarantOrgAddress.PK);
			dec.JE_MessageType = "";
			AssertEquals("Changing the message type automatically re-synchs the declarant... to be the orgProxy", orgProxy.MainAddress.PK, dec.Declarant.PK);
			dec.JE_MessageType = "IMP";
			AssertEquals("Changing the message type automatically re-synchs the declarant", importBroker.MainAddress.PK, dec.Declarant.PK);
			dec.JE_MessageType = "EXP";
			AssertEquals("Changing the message type automatically re-synchs the declarant", exportBroker.MainAddress.PK, dec.Declarant.PK);
		}

		public void TestFirstArrivalPortIsCorrectlySetFromRouting()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			{
				ForwardingShipment shipment = Factory.New<ForwardingShipment>();
				ForwardingConsol consol = shipment.Consols.AddNew();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				Transport transportKRSingapore = consol.Transports[0];
				consol.Transports.RemoveAndDeleteAll();
				var transport1 = consol.Transports.AddNew();
				transport1.JW_LegOrder = 1;
				transport1.JW_RL_NKLoadPort = "HKHKG";
				transport1.JW_RL_NKDiscPort = "SGSIN";

				var transport2 = consol.Transports.AddNew();
				transport2.JW_LegOrder = 2;
				transport2.JW_RL_NKLoadPort = "SGSIN";
				transport2.JW_RL_NKDiscPort = "ITBGO";

				var transport3 = consol.Transports.AddNew();
				transport3.JW_LegOrder = 3;
				transport3.JW_RL_NKLoadPort = "ITBGO";
				transport3.JW_RL_NKDiscPort = "DEFRA";

				var transport4 = consol.Transports.AddNew();
				transport4.JW_LegOrder = 4;
				transport4.JW_RL_NKLoadPort = "DEFRA";
				transport4.JW_RL_NKDiscPort = "GBLHR";

				shipment.JS_RL_NKOrigin = "HKHKG";
				shipment.JS_RL_NKDestination = "GBLHR";

				consol.JK_RL_NKLoadPort = "HKHKG";
				consol.JK_RL_NKDischargePort = "GBLHR";

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_JS = shipment.PK;
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

				AssertEquals("First Port of Arrival when not set on consol", "ITBGO", declaration.JE_RL_NKPortOfFirstArrival);

				consol.JK_RL_NKPortOfFirstArrival = "FRPAR";
				declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
				AssertEquals("First Port of Arrival when set to EU port on consol", "FRPAR", declaration.JE_RL_NKPortOfFirstArrival);

				consol.JK_RL_NKPortOfFirstArrival = "SGSIN";
				declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
				AssertEquals("First Port of Arrival when not set to EU port on consol", "ITBGO", declaration.JE_RL_NKPortOfFirstArrival);
			}
		}

		public void TestTotalInnerPackages()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_TotalPackageCount = 18;
			var declaration = GetDeclaration();
			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			var cei1 = declaration.CustomsEntryInstructions.AddNew();
			var cei2 = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_JS = shipment.PK;
			var sync = declaration.ShipmentSynchroniser;
			sync.Synchronise(true);

			CombineAssertions("CEI_TotalInnerPackages of first entry instruction should be synchronized from JS_TotalPackageCount.", () =>
			{
				AssertEquals("First CEI: ", 18, cei1.CEI_TotalInnerPackages);
				AssertEquals("Second CEI: ", 0, cei2.CEI_TotalInnerPackages);
			});
		}

		public void TestItineraryCountriesIsCorrectlySetFromRouting()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.Transports.RemoveAndDeleteAll();

			var transport1 = shipment.TransportsIncludingRelated[0];
			transport1.JW_LegOrder = 1;
			transport1.JW_RL_NKLoadPort = "DEFRA";
			transport1.JW_RL_NKDiscPort = "SGSIN";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "ITBGO";

			var transport3 = shipment.TransportsIncludingRelated.AddNew();
			transport3.JW_LegOrder = 3;
			transport3.JW_RL_NKLoadPort = "ITBGO";
			transport3.JW_RL_NKDiscPort = "USNYC";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			CombineAssertions(() =>
			{
				AssertEquals("Count of Countries", 4, declaration.ItineraryCountries.Count);
				AssertEquals("Itinerary Countries", "DE SG IT US", declaration.ItineraryCountryList);
				AssertEquals("UniqueVoyageIdentifier", "DESGITUS", declaration.UniqueVoyageIdentifier);
				AssertEquals("TransportsIncludingRelated must contain SHP parent type", true, shipment.TransportsIncludingRelated.Cast<Transport>().Select(x => x.JW_ParentType).ToList().Contains("SHP"));
				AssertEquals("TransportsIncludingRelated must contain CON parent type", true, shipment.TransportsIncludingRelated.Cast<Transport>().Select(x => x.JW_ParentType).ToList().Contains("CON"));
			});
		}

		protected virtual JobDeclaration GetDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}
	}
}
