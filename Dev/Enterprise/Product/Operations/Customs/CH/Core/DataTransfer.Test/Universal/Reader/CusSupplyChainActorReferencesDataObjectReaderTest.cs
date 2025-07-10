using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.CH.DataTransfer.Testing;

partial class CombinedDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
{
	public void TestReadSupplyChainActors()
	{
		var existingDeclaration = Factory.New<JobDeclaration>();
		existingDeclaration.JE_MasterBill = "MB123";
		var existingEntryInstruction = existingDeclaration.CustomsEntryInstructions.AddNew();
		existingEntryInstruction.CEI_Style = "A";
		existingEntryInstruction.CEI_Description = "GREETING";

		var cusSupplyChainActorReferences = existingEntryInstruction.SupplyChainActors;
		var existingActor1 = cusSupplyChainActorReferences.AddNew();
		existingActor1.CFR_Code = "CS";
		existingActor1.CFR_Reference = "REF123";
		existingActor1.CFR_OA_Owner = Organisation3.MainAddress.PK;

		var existingActor2 = cusSupplyChainActorReferences.AddNew();
		existingActor2.CFR_Code = "MF";
		existingActor2.CFR_Reference = "REF657";
		existingActor2.CFR_OA_Owner = Organisation2.MainAddress.PK;

		var existingActor3 = cusSupplyChainActorReferences.AddNew();
		existingActor3.CFR_Code = "WH";
		existingActor3.CFR_Reference = "ZZZ999";
		existingActor3.CFR_OA_Owner = Organisation1.MainAddress.PK;

		var customsReferenceCollection = CreateSupplyChainActors();

		CombineAssertions(() =>
		{
			new CusSupplyChainActorReferencesDataObjectReader(new TestErrorLogger(), Factory).Read(cusSupplyChainActorReferences, customsReferenceCollection.GroupBy(x => x.Type.Code.Value).ToDictionary(x => x.Key, y => y.ToList()));
			AssertEquals("Supply chain actor count", 2, cusSupplyChainActorReferences.Count);
			var actor1 = cusSupplyChainActorReferences[0];
			var actor2 = cusSupplyChainActorReferences[1];
			if (existingActor1.CFR_Reference != "REF123")
			{
				actor1 = cusSupplyChainActorReferences[1];
				actor2 = cusSupplyChainActorReferences[0];
			}
			AssertSame(existingActor1, actor1);
			AssertSame(existingActor2, actor2);
			AssertEquals("existingActor3.IsDeleted - not matched based on CFR_Code", true, existingActor3.IsDeleted);

			AssertEquals("Actor 1 Role", "CS", actor1.CFR_Code);
			AssertEquals("Actor 1 Reference", "REF123", actor1.CFR_Reference);
			AssertEquals("Actor 1 Owner", Organisation1.MainAddress.PK, actor1.CFR_OA_Owner);

			AssertEquals("Actor 2 Role", "MF", actor2.CFR_Code);
			AssertEquals("Actor 2 Reference", "REF987", actor2.CFR_Reference);
			AssertEquals("Actor 2 Owner", Organisation2.MainAddress.PK, actor2.CFR_OA_Owner);
		});
	}

	public void TestReadEmptyReferenceCollection()
	{
		var existingDeclaration = Factory.New<JobDeclaration>();
		existingDeclaration.JE_MasterBill = "MB123";
		var existingEntryInstruction = existingDeclaration.CustomsEntryInstructions.AddNew();
		existingEntryInstruction.CEI_Style = "A";
		existingEntryInstruction.CEI_Description = "GREETING";

		var cusSupplyChainActorReferences = existingEntryInstruction.SupplyChainActors;
		var existingActor1 = cusSupplyChainActorReferences.AddNew();
		existingActor1.CFR_Code = "CS";
		existingActor1.CFR_Reference = "REF123";
		existingActor1.CFR_OA_Owner = Organisation3.MainAddress.PK;

		var dictionary = new Dictionary<ZString, List<CustomsReference>>()
				{
					{ (ZString)CusReferenceTypeList.Codes.SupplyChainActor, new List<CustomsReference>(new[] { new CustomsReference { Type = new CodeDescriptionPair() { Code = CusReferenceTypeList.Codes.SupplyChainActor } } }) }
				};

		CombineAssertions(() =>
		{
			new CusSupplyChainActorReferencesDataObjectReader(new TestErrorLogger(), Factory).Read(cusSupplyChainActorReferences, dictionary);
			AssertEquals("Supply chain actor count", 0, cusSupplyChainActorReferences.Count);
			AssertEquals("existingActor1.IsDeleted - not matched based on CFR_Code", true, existingActor1.IsDeleted);
		});
	}

	public void TestReadReferenceCollectionWithoutSupplyChainActors()
	{
		var existingDeclaration = Factory.New<JobDeclaration>();
		existingDeclaration.JE_MasterBill = "MB123";
		var existingEntryInstruction = existingDeclaration.CustomsEntryInstructions.AddNew();
		existingEntryInstruction.CEI_Style = "A";
		existingEntryInstruction.CEI_Description = "GREETING";

		var cusSupplyChainActorReferences = existingEntryInstruction.SupplyChainActors;
		var existingActor1 = cusSupplyChainActorReferences.AddNew();
		existingActor1.CFR_Code = "CS";
		existingActor1.CFR_Reference = "REF123";
		existingActor1.CFR_OA_Owner = Organisation3.MainAddress.PK;

		var dictionary = new Dictionary<ZString, List<CustomsReference>>()
				{
					{ (ZString)CusReferenceTypeList.Codes.FiscalReference, new List<CustomsReference>(new[] { new CustomsReference { Type = new CodeDescriptionPair() { Code = CusReferenceTypeList.Codes.FiscalReference }, Reference = "REF12" } }) }
				};

		CombineAssertions(() =>
		{
			new CusSupplyChainActorReferencesDataObjectReader(new TestErrorLogger(), Factory).Read(cusSupplyChainActorReferences, dictionary);
			AssertEquals("Supply chain actor count", 1, cusSupplyChainActorReferences.Count);
			AssertSame(existingActor1, cusSupplyChainActorReferences[0]);
			AssertEquals("existingActor1.IsDeleted - should not touch since xml doesn't contain data", false, existingActor1.IsDeleted);

			AssertEquals("Actor 1 Role", "CS", existingActor1.CFR_Code);
			AssertEquals("Actor 1 Reference", "REF123", existingActor1.CFR_Reference);
			AssertEquals("Actor 1 Owner", Organisation3.MainAddress.PK, existingActor1.CFR_OA_Owner);
		});
	}
}
