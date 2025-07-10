using System.Collections.Generic;
using System.Linq;
using CargoWise.Integration;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobDeclarationSynchroniserTest : EU.Business.Declaration.Testing.JobDeclarationSynchroniserTest
{
	public void TestSynchroniserBuyersConsolLead()
	{
		var consol = Factory.New<ForwardingConsol>();
		consol.JK_RL_NKLoadPort = "KRANY";
		consol.JK_RL_NKDischargePort = "ITVCE";
		consol.JK_MasterBillNum = "M1234567";

		var leadShipment = consol.Shipments.AddNew();
		leadShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
		leadShipment.JS_ActualWeight = 1000;
		leadShipment.JS_UnitOfWeight = "KG";
		leadShipment.JS_ActualVolume = 5;
		leadShipment.JS_UnitOfVolume = "M3";
		leadShipment.JS_OuterPacks = 100;
		leadShipment.JS_F3_NKPackType = "PLT";

		var subShipment1 = leadShipment.CoLoadShipments.AddNew();
		subShipment1.JS_ActualWeight = 2000;
		subShipment1.JS_UnitOfWeight = "LB";
		subShipment1.JS_ActualVolume = 1.75;
		subShipment1.JS_UnitOfVolume = "CF";
		subShipment1.JS_OuterPacks = 200;
		subShipment1.JS_F3_NKPackType = "PLT";

		var subShipment2 = leadShipment.CoLoadShipments.AddNew();
		subShipment2.JS_ActualWeight = 3000;
		subShipment2.JS_UnitOfWeight = "KG";
		subShipment2.JS_ActualVolume = 3.8;
		subShipment2.JS_UnitOfVolume = "M3";
		subShipment2.JS_OuterPacks = 300;
		subShipment2.JS_F3_NKPackType = "PKG";
		Factory.Save();

		// Lead Shipment Declaration
		var declarationLeadShp = Factory.New<JobDeclaration>();
		declarationLeadShp.JE_JS = leadShipment.PK;
		declarationLeadShp.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
		declarationLeadShp.ShipmentSynchroniser.Synchronise();

		CombineAssertions("Lead Shipment Declaration", () =>
		{
			AssertEquals("JE_TotalWeight", 1000m, declarationLeadShp.JE_TotalWeight);
			AssertEquals("JE_TotalWeightUnit", "KG", declarationLeadShp.JE_TotalWeightUnit);
			AssertEquals("JE_TotalVolume", 5m, declarationLeadShp.JE_TotalVolume);
			AssertEquals("JE_TotalVolumeUnit", "M3", declarationLeadShp.JE_TotalVolumeUnit);
			AssertEquals("JE_TotalNoOfPacks", 100, declarationLeadShp.JE_TotalNoOfPacks);
			AssertEquals("JE_TotalNoOfPacksPackType", "PLT", declarationLeadShp.JE_TotalNoOfPacksPackType);
		});

		// Standard Shipment 1 Declaration
		var declarationSubShp1 = Factory.New<JobDeclaration>();
		declarationSubShp1.JE_JS = subShipment1.PK;
		declarationSubShp1.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
		declarationSubShp1.ShipmentSynchroniser.Synchronise();

		CombineAssertions("Standard Shipment 1 Declaration", () =>
		{
			AssertEquals("JE_TotalWeight", 2000m, declarationSubShp1.JE_TotalWeight);
			AssertEquals("JE_TotalWeightUnit", "LB", declarationSubShp1.JE_TotalWeightUnit);
			AssertEquals("JE_TotalVolume", 1.75m, declarationSubShp1.JE_TotalVolume);
			AssertEquals("JE_TotalVolumeUnit", "CF", declarationSubShp1.JE_TotalVolumeUnit);
			AssertEquals("JE_TotalNoOfPacks", 200, declarationSubShp1.JE_TotalNoOfPacks);
			AssertEquals("JE_TotalNoOfPacksPackType", "PLT", declarationSubShp1.JE_TotalNoOfPacksPackType);
		});

		// Standard Shipment 2 Declaration
		var declarationSubShp2 = Factory.New<JobDeclaration>();
		declarationSubShp2.JE_JS = subShipment2.PK;
		declarationSubShp2.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
		declarationSubShp2.ShipmentSynchroniser.Synchronise();

		CombineAssertions("Standard Shipment 2 Declaration", () =>
		{
			AssertEquals("JE_TotalWeight", 3000m, declarationSubShp2.JE_TotalWeight);
			AssertEquals("JE_TotalWeightUnit", "KG", declarationSubShp2.JE_TotalWeightUnit);
			AssertEquals("JE_TotalVolume", 3.8m, declarationSubShp2.JE_TotalVolume);
			AssertEquals("JE_TotalVolumeUnit", "M3", declarationSubShp2.JE_TotalVolumeUnit);
			AssertEquals("JE_TotalNoOfPacks", 300, declarationSubShp2.JE_TotalNoOfPacks);
			AssertEquals("JE_TotalNoOfPacksPackType", "PKG", declarationSubShp2.JE_TotalNoOfPacksPackType);
		});

		// Changes are propagated
		leadShipment.JS_ActualWeight = 1001;
		subShipment1.JS_ActualVolume = 3;
		subShipment1.JS_UnitOfVolume = "M3";
		subShipment2.JS_OuterPacks = 202;

		CombineAssertions("After changing some shipment values", () =>
		{
			AssertEquals("[Lead Shipment Declaration] JE_TotalWeight", 1001m, declarationLeadShp.JE_TotalWeight);
			AssertEquals("[Lead Shipment Declaration] JE_TotalWeightUnit", "KG", declarationLeadShp.JE_TotalWeightUnit);
			AssertEquals("[Standard Shipment 1 Declaration] JE_TotalVolume", 3m, declarationSubShp1.JE_TotalVolume);
			AssertEquals("[Standard Shipment 1 Declaration] JE_TotalVolumeUnit", "M3", declarationSubShp1.JE_TotalVolumeUnit);
			AssertEquals("[Standard Shipment 2 Declaration] JE_TotalNoOfPacks", 202, declarationSubShp2.JE_TotalNoOfPacks);
			AssertEquals("[Standard Shipment 2 Declaration] JE_TotalNoOfPacksPackType", "PKG", declarationSubShp2.JE_TotalNoOfPacksPackType);
		});
	}

	public void TestSynchroniserCTStatusWhenIsExp()
	{
		var shipment = Factory.New<ForwardingShipment>();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		declaration.JE_JS = shipment.PK;
		var syncronizer = declaration.ShipmentSynchroniser;

		shipment.JS_CommunityTransitStatus = "T2L";
		AssertCollectionContains("[PRE-CONDITION] CommunityTransitStatusIDList", "T2L", GetCommunityTransitStatusIDCodeList());
		syncronizer.Synchronise(force: true);
		AssertEquals("When CommunityTransitStatusIDList contains shipment value, ZG_CTStatusID", "T2L", declaration.ZG_CTStatusID);

		shipment.JS_CommunityTransitStatus = "F";
		AssertCollectionNotContains("[PRE-CONDITION] CommunityTransitStatusIDList", "F", GetCommunityTransitStatusIDCodeList());
		syncronizer.Synchronise(force: true);
		AssertEquals("When CommunityTransitStatusIDList not contains shipment value, ZG_CTStatusID", "", declaration.ZG_CTStatusID);

		IEnumerable<string> GetCommunityTransitStatusIDCodeList() => declaration.AddInfoLookups.CommunityTransitStatusIDList.Cast<ICodeDescription>().Select(x => x.Code);
	}

	public void TestSynchroniserCTStatusWhenIsImp()
	{
		var shipment = Factory.New<ForwardingShipment>();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_JS = shipment.PK;
		declaration.JE_MessageType = "IMP";

		shipment.JS_CommunityTransitStatus = "T2L";
		declaration.ShipmentSynchroniser.Synchronise(force: true);
		AssertEquals("For import do copy anytime, ZG_CTStatusID", "", declaration.ZG_CTStatusID);
	}

	public void TestGetPackingSynchroniser()
	{
		var shipment = Factory.New<ForwardingShipment>();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_JS = shipment.PK;
		var synchroniser = new JobDeclarationSynchroniserForTest(declaration);
		AssertType<PackingSynchroniser>("GetPackingSynchroniser", synchroniser.GetPackingSynchroniserExposed());
	}

	protected override EU.Business.Declaration.JobDeclaration GetDeclaration()
	{
		return Factory.New<JobDeclaration>();
	}

	class JobDeclarationSynchroniserForTest : JobDeclarationSynchroniser
	{
		public JobDeclarationSynchroniserForTest(JobDeclaration destination) : base(destination)
		{
		}

		public Customs.Business.PackingSynchroniser GetPackingSynchroniserExposed() => GetPackingSynchroniser();
	}
}
