using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

internal class AdditionalSupplyChainActorProviderTest : TestCaseWithFactory
{
	public void TestNewCollection()
	{
		AssertNull("null", CountryOfRoutingOfConsignmentProvider.NewCollection(null));
	}

	public void TestSequenceNumber()
	{
		NctsHeader.CusSupplyChainActors.AddNew().CFR_Reference = "SupplyChainActor1";
		NctsHeader.CusSupplyChainActors.AddNew().CFR_Reference = "SupplyChainActor2";
		NctsHeader.CusSupplyChainActors.AddNew().CFR_Reference = "SupplyChainActor3";
		var dataProviders = AdditionalSupplyChainActorDataProvider.NewCollection(NctsHeader.CusSupplyChainActors);
		CombineAssertions(() =>
		{
			AssertEquals("SupplyChainActor1", 1, dataProviders.ElementAt(0).SequenceNumber);
			AssertEquals("SupplyChainActor2", 2, dataProviders.ElementAt(1).SequenceNumber);
			AssertEquals("SupplyChainActor3", 3, dataProviders.ElementAt(2).SequenceNumber);
		});
	}

	public void TestProvider()
	{
		var cusSupplyChainActor = NctsHeader.CusSupplyChainActors.AddNew();
		cusSupplyChainActor.CFR_Code = "123";
		cusSupplyChainActor.CFR_Reference = "ref12345";
		var dataProvider = CreateDataProviders().First();
		CombineAssertions(() =>
		{
			AssertEquals("Role", "123", dataProvider.Role);
			AssertEquals("Identification Number", "ref12345", dataProvider.IdentificationNumber);
		});
	}

	NctsHeader NctsHeader => nctsHeader ?? (nctsHeader = CreateNctsHeader());
	NctsHeader nctsHeader;

	NctsHeader CreateNctsHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		return nctsHeader;
	}

	IEnumerable<AdditionalSupplyChainActorDataProvider> CreateDataProviders() => AdditionalSupplyChainActorDataProvider.NewCollection((CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>)NctsHeader.CusSupplyChainActors);
}
