using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.GB.H7.Messaging.Testing
{
	public class GbCDSH7ImportDeclarationWrapperTest : TestCaseWithFactory
	{
		public void TestExporter()
		{
			var shipperOrg = Factory.New<OrgHeader>();
			shipperOrg.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "001", "UK");

			var shipper = shipperOrg.Addresses.AddNew();
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_OA_Shipper = shipper.PK;
			bill.ABL_ShipperStreet1 = "Address1";
			bill.ABL_ShipperStreet2 = "Address2";
			bill.ABL_ShipperCity = "City";
			bill.ABL_RN_NKShipperCountry = "AU";
			bill.ABL_ShipperPostcode = "12345678";
			bill.ABL_ShipperName = "Shipper";

			var messageSendingObject = new MessageSendingObject(bill);

			var wrapper = new GbCDSH7ImportDeclarationWrapper(messageSendingObject);
			var exporter = wrapper.Exporter;
			CombineAssertions("Exporter", () =>
			{
				AssertEquals("Id", ZString.Empty, exporter.ID);
				AssertEquals("Name", "Shipper", exporter.Name);
				AssertEquals("Country code", "AU", exporter.Address.CountryCode);
				AssertEquals("Address line", "Address1Address2", exporter.Address.Line);
				AssertEquals("City", "City", exporter.Address.CityName);
				AssertEquals("Postcode ID", "12345678", exporter.Address.PostcodeID);
			});
		}

		public void TestDeclarant()
		{
			var declarantOrg = Factory.New<OrgHeader>();
			declarantOrg.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "001", "UK");

			var declarant = declarantOrg.Addresses.AddNew();
			declarant.Address1 = "Address1";
			declarant.Address2 = "Address2";
			declarant.City = "City";
			declarant.OA_RN_NKCountryCode = "AU";
			declarant.OA_PostCode = "12345678";
			declarant.CompanyName = "Declarant";

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_OA_Declarant = declarant.PK;
			var bill = header.Bills.AddNew();

			var messageSendingObject = new MessageSendingObject(bill);

			var wrapper = new GbCDSH7ImportDeclarationWrapper(messageSendingObject);
			CombineAssertions("Declarant", () =>
			{
				AssertEquals("Id", "UK001", wrapper.Declarant.ID);
				AssertEquals("Name", "Declarant", wrapper.Declarant.Name);
				AssertEquals("Country code", "AU", wrapper.Declarant.Address.CountryCode);
				AssertEquals("Address line", "Address1 Address2", wrapper.Declarant.Address.Line);
				AssertEquals("City", "City", wrapper.Declarant.Address.CityName);
				AssertEquals("Postcode ID", "12345678", wrapper.Declarant.Address.PostcodeID);
			});
		}

		public void TestAgent()
		{
			var agentOrg = Factory.New<OrgHeader>();
			agentOrg.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "001", "UK");

			var agent = agentOrg.Addresses.AddNew();
			agent.Address1 = "Address1";
			agent.Address2 = "Address2";
			agent.City = "City";
			agent.OA_RN_NKCountryCode = "AU";
			agent.OA_PostCode = "12345678";
			agent.CompanyName = "Agent";

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_OA_Representative = agent.PK;
			header.AMA_AgentType = EU.H7.Business.EUH7AgentTypes.Codes.DIR;
			var bill = header.Bills.AddNew();

			var messageSendingObject = new MessageSendingObject(bill);
			var wrapper = new GbCDSH7ImportDeclarationWrapper(messageSendingObject);
			CombineAssertions("Agent", () =>
			{
				AssertEquals("Id", "UK001", wrapper.Agent.Agent.ID);
				AssertEquals("Name", "Agent", wrapper.Agent.Agent.Name);
				AssertEquals("Country code", "AU", wrapper.Agent.Agent.Address.CountryCode);
				AssertEquals("Address line", "Address1 Address2", wrapper.Agent.Agent.Address.Line);
				AssertEquals("City", "City", wrapper.Agent.Agent.Address.CityName);
				AssertEquals("Postcode ID", "12345678", wrapper.Agent.Agent.Address.PostcodeID);
				AssertEquals("Function code", "2", wrapper.Agent.FunctionCode);
			});
		}

		public void TestAgentFunctionCodeWhenEmptyRepresentative()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_OA_Representative = ZGuid.Empty;
			header.AMA_AgentType = EU.H7.Business.EUH7AgentTypes.Codes.DIR;
			var bill = header.Bills.AddNew();

			var messageSendingObject = new MessageSendingObject(bill);
			var wrapper = new GbCDSH7ImportDeclarationWrapper(messageSendingObject);
			CombineAssertions("Agent", () =>
			{
				AssertEquals("Function code", "2", wrapper.Agent.FunctionCode);
			});

			header.AMA_AgentType = EU.H7.Business.EUH7AgentTypes.Codes.IND;

			messageSendingObject = new MessageSendingObject(bill);
			wrapper = new GbCDSH7ImportDeclarationWrapper(messageSendingObject);
			CombineAssertions("Agent", () =>
			{
				AssertEquals("Function code", "3", wrapper.Agent.FunctionCode);
			});
		}

		public void TestPresentationOffice()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.PresentationOffice = "ABC";
			var bill = header.Bills.AddNew();

			var messageSendingObject = new MessageSendingObject(bill);
			var wrapper = new GbCDSH7ImportDeclarationWrapper(messageSendingObject);
			AssertEquals("ABC", wrapper.PresentationOffice);
		}

		public void TestTotalPackageQuantity_NonBIRDS()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack1 = bill.Packs.AddNew();
			var pack2 = bill.Packs.AddNew();

			pack1.APA_PackQty = 1;
			pack2.APA_PackQty = 9;

			var messageSendingObject = new MessageSendingObject(bill);
			var wrapper = new GbCDSH7ImportDeclarationWrapper(messageSendingObject);
			AssertEquals("Will not provide package quantity for non BIRDS message", 0m, wrapper.TotalPackageQuantity);
		}

		public void TestTotalPackageQuantity_BIRDS()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfDischarge = "AUSYD";
			var bill = header.Bills.AddNew();
			var pack1 = bill.Packs.AddNew();
			var pack2 = bill.Packs.AddNew();

			pack1.APA_PackQty = 1;
			pack2.APA_PackQty = 9;

			var messageSendingObject = new MessageSendingObject(bill);
			var wrapper = new GbCDSH7ImportDeclarationWrapper(messageSendingObject);
			AssertEquals("Will provide package quantity for BIRDS message", 10m, wrapper.TotalPackageQuantity);
		}

		public void TestBorderTransportMeans()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			var messageSendingObject = new MessageSendingObject(bill);
			var wrapper = new GbCDSH7ImportDeclarationWrapper(messageSendingObject);

			CombineAssertions("BorderTransportMeans", () =>
			{
				header.AMA_TransportMode = "SEA";
				AssertEquals("SEA", "1", wrapper.BorderTransportMeans.ModeCode);

				header.AMA_TransportMode = "RAI";
				AssertEquals("RAI", "2", wrapper.BorderTransportMeans.ModeCode);

				header.AMA_TransportMode = "ROA";
				AssertEquals("ROA", "3", wrapper.BorderTransportMeans.ModeCode);

				header.AMA_TransportMode = "AIR";
				AssertEquals("AIR", "4", wrapper.BorderTransportMeans.ModeCode);

				header.AMA_TransportMode = "MAI";
				AssertEquals("MAI", "5", wrapper.BorderTransportMeans.ModeCode);

				header.AMA_TransportMode = "INV";
				AssertEquals("Invalid Transport Mode", "INV", wrapper.BorderTransportMeans.ModeCode);
			});
		}

		public void TestTotalGrossMassMeasure()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_GrossWeight = 100;
			bill.ABL_GrossWeightUQ = "g";

			var messageSendingObject = new MessageSendingObject(bill);
			var wrapper = new GbCDSH7ImportDeclarationWrapper(messageSendingObject);
			AssertEquals(0.1m, wrapper.TotalGrossMassMeasure);
		}

		public void TestFreightChargeAmount()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_TransportValue = 10;
			bill.ABL_RX_NKTransportValueCurrency = "USD";

			var messageSendingObject = new MessageSendingObject(bill);
			var wrapper = new GbCDSH7ImportDeclarationWrapper(messageSendingObject);
			AssertEquals(10m, wrapper.FreightChargeAmount.Amount);
			AssertEquals("USD", wrapper.FreightChargeAmount.Currency);
		}

		public void TestSupervisingOffice()
		{
			var declarantOrg = Factory.New<OrgHeader>();
			var declarant = declarantOrg.Addresses.AddNew();

			var relatedPartyOrg = Factory.New<OrgHeader>();
			var relatedParty = declarantOrg.AllRelatedParties.AddNew();
			relatedParty.PR_OH_RelatedParty = relatedPartyOrg.PK;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.CustomsOffice;
			relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_OA_Declarant = ZGuid.Empty;
			var bill = header.Bills.AddNew();

			var messageSendingObject = new MessageSendingObject(bill);
			var wrapper = new GbCDSH7ImportDeclarationWrapper(messageSendingObject);

			CombineAssertions(() =>
			{
				AssertEquals("Expected empty when Supervising Office is not available", string.Empty, wrapper.SupervisingOffice);

				header.AMA_OA_Declarant = declarant.PK;
				AssertEquals("Expected empty when Supervising Office has no CCD code", string.Empty, wrapper.SupervisingOffice);

				relatedPartyOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "GB001", Core.Constants.CountryCodes.UnitedKingdom);
				AssertEquals("Expected populated value when Supervising Office has CCD code", "GB001", wrapper.SupervisingOffice);
			});
		}

		public void TestAuthorisationHolders()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var messageSendingObject = new MessageSendingObject(bill);
			var wrapper = new GbCDSH7ImportDeclarationWrapper(messageSendingObject);

			CombineAssertions(() =>
			{
				AssertNull("Not a BIRDS message, permit header is null.", wrapper.AuthorisationHolders);

				var refCountry = SetupPortOfDischarge(header);

				AssertNull("Not a BIRDS message(The Port of Discharge is in Northern Ireland), permit header is null.", wrapper.AuthorisationHolders);

				SetupDeclarantAndPermit(header);
				AssertNull("Not a BIRDS message(The Port of Discharge is in Northern Ireland), permit header is not null.", wrapper.AuthorisationHolders);

				refCountry.RW_RegionName = "ENGLAND";

				AssertEquals("It's a BIRDS Message, permit header is not null", 1, wrapper.AuthorisationHolders.Count());
				var authorisationHolder = wrapper.AuthorisationHolders.First();
				AssertEquals("GB001", authorisationHolder.ID);
				AssertEquals("BRD", authorisationHolder.CategoryCode);
			});
		}

		RefCountryStates SetupPortOfDischarge(AsycudaManifestHeader header)
		{
			var refCountry = Factory.New<RefCountryStates>();
			refCountry.RW_RegionName = RefUNLOCO.Regions.NorthernIreland;

			var refUNLOCO = Factory.New<RefUNLOCO>();
			refUNLOCO.RL_Code = "GBAAA";
			refUNLOCO.RL_RW = refCountry.PK;

			header.AMA_RL_NKPortOfDischarge = "GBAAA";

			return refCountry;
		}

		void SetupDeclarantAndPermit(AsycudaManifestHeader header)
		{
			var declarantOrg = Factory.New<OrgHeader>();
			declarantOrg.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "001", "GB");
			var declarantAddress = declarantOrg.Addresses.AddNew();
			declarantAddress.OA_Address1 = "Test Address 1";

			var permit = Factory.New<CusPermitHeader>();
			permit.CPH_OA_AppliesTo = declarantAddress.PK;
			permit.CPH_Type = "BRD";

			header.AMA_OA_Declarant = declarantAddress.PK;
		}
	}
}
