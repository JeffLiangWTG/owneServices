using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

internal class AdditionalSupplyChainActorDataProviderTest : TestCaseWithFactory
{
	public void TestSequenceNumber()
	{
		EntryInstruction.SupplyChainActors.RemoveAll();
		EntryInstruction.SupplyChainActors.AddNew().CFR_Reference = "SupplyChainActor1";
		EntryInstruction.SupplyChainActors.AddNew().CFR_Reference = "SupplyChainActor2";
		EntryInstruction.SupplyChainActors.AddNew().CFR_Reference = "SupplyChainActor3";

		var dataProviders = AdditionalSupplyChainActorDataProvider.NewCollection(EntryInstruction.SupplyChainActors.Cast<CusSupplyChainActorReference>());

		CombineAssertions(() =>
		{
			AssertEquals("SupplyChainActor1", 1, dataProviders.ElementAt(0).SequenceNumber);
			AssertEquals("SupplyChainActor2", 2, dataProviders.ElementAt(1).SequenceNumber);
			AssertEquals("SupplyChainActor3", 3, dataProviders.ElementAt(2).SequenceNumber);
		});
	}

	public void TestProvider()
	{
		CusSupplyChainActor.CFR_Code = "123";
		CusSupplyChainActor.CFR_Reference = "ref12345";

		var dataProvider = CreateDataProviders().First();

		CombineAssertions(() =>
		{
			AssertEquals("Role", "123", dataProvider.Role);
			AssertEquals("Identification Number", "ref12345", dataProvider.IdentificationNumber);
		});
	}

	CusEntryInstruction EntryInstruction => entryInstruction ?? (entryInstruction = GetEntryInstruction());
	CusEntryInstruction entryInstruction;

	CusSupplyChainActorReference CusSupplyChainActor => cusSupplyChainActorReference ?? (cusSupplyChainActorReference = GetCusSupplyChainActor(EntryInstruction));
	CusSupplyChainActorReference cusSupplyChainActorReference;

	CusEntryInstruction GetEntryInstruction()
	{
		var declaration = Factory.New<JobDeclaration>();
		return entryInstruction = declaration.CustomsEntryInstructions.AddNew();
	}

	CusSupplyChainActorReference GetCusSupplyChainActor(CusEntryInstruction entryInstruction)
	{
		return entryInstruction.SupplyChainActors.AddNew();
	}

	IEnumerable<AdditionalSupplyChainActorDataProvider> CreateDataProviders() => AdditionalSupplyChainActorDataProvider.NewCollection(EntryInstruction.SupplyChainActors.Cast<CusSupplyChainActorReference>());
}
