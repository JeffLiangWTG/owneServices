using System.Collections.Generic;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.CH.DataTransfer.Testing;

partial class CombinedDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
{
	public void TestImportSupplyChainActors()
	{
		var declaration = Factory.New<JobDeclaration>();
		var existingEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
		existingEntryInstruction.CEI_Style = "A";
		existingEntryInstruction.CEI_Description = "GREETING";
		var existingActor1 = existingEntryInstruction.SupplyChainActors.AddNew();
		existingActor1.CFR_Code = "CS";
		existingActor1.CFR_Reference = "REF123";
		existingActor1.CFR_OA_Owner = Organisation3.MainAddress.PK;

		var existingActor2 = existingEntryInstruction.SupplyChainActors.AddNew();
		existingActor2.CFR_Code = "MF";
		existingActor2.CFR_Reference = "REF657";
		existingActor2.CFR_OA_Owner = Organisation2.MainAddress.PK;

		var existingActor3 = existingEntryInstruction.SupplyChainActors.AddNew();
		existingActor3.CFR_Code = "WH";
		existingActor3.CFR_Reference = "ZZZ999";
		existingActor3.CFR_OA_Owner = Organisation1.MainAddress.PK;

		Factory.SaveForTesting();

		CombineAssertions(() =>
		{
			var entryInstructionDataObject = new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Style = "A",
				Description = "GREETING",
				CustomsReferenceCollection = CreateSupplyChainActors(),
			};

			var helper = new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Latvia, Core.Constants.CountryCodes.Latvia);
			var reader = new CustomsEntryInstructionDataObjectReader(entryInstructionDataObject, new TestErrorLogger(), helper, Factory, declaration);
			var entryInstruction = (CusEntryInstruction)reader.ReadIntoBusinessObject();

			AssertSame(existingEntryInstruction, entryInstruction);
			AssertEquals("Supply chain actor count", 2, entryInstruction.SupplyChainActors.Count);
			var actor1 = entryInstruction.SupplyChainActors[0];
			var actor2 = entryInstruction.SupplyChainActors[1];
			if (existingActor1.CFR_Reference != "REF123")
			{
				actor1 = entryInstruction.SupplyChainActors[1];
				actor2 = entryInstruction.SupplyChainActors[0];
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

	List<CustomsReference> CreateSupplyChainActors()
	{
		var result = new List<CustomsReference>();
		var actor1Address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
		actor1Address.OrganizationCode = Organisation1.OH_Code;

		var actor1 = new CustomsReference();
		actor1.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
		actor1.Type = new CodeDescriptionPair() { Code = Customs.Business.CusReferenceTypeList.Codes.SupplyChainActor };
		actor1.SubType = new CodeDescriptionPair35Char() { Code = "CS" };
		actor1.Reference = "REF123";
		actor1.Owner = actor1Address;
		result.Add(actor1);

		var actor2Address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
		actor2Address.OrganizationCode = Organisation2.OH_Code;

		var actor2 = new CustomsReference();
		actor2.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
		actor2.Type = new CodeDescriptionPair() { Code = Customs.Business.CusReferenceTypeList.Codes.SupplyChainActor };
		actor2.SubType = new CodeDescriptionPair35Char() { Code = "MF" };
		actor2.Reference = "REF987";
		actor2.Owner = actor2Address;
		result.Add(actor2);

		return result;
	}

	OrgHeader Organisation1
	{
		get
		{
			if (organisation1 == null)
			{
				organisation1 = Factory.NewWithValidTestData<OrgHeader>();
				organisation1.OH_FullName = "Test Company 1";
				organisation1.OH_Code = "TESTCO1";
				var org1Address = organisation1.MainAddress;
				org1Address.FillWithValidTestData();
				org1Address.Address1 = "123 Test Street";
				org1Address.Postcode = "A12B3C4";
			}
			return organisation1;
		}
	}
	OrgHeader organisation1;

	OrgHeader Organisation2
	{
		get
		{
			if (organisation2 == null)
			{
				organisation2 = Factory.NewWithValidTestData<OrgHeader>();
				organisation2.OH_FullName = "Test Company 2";
				organisation2.OH_Code = "TESTCO2";
				var org2Address = organisation2.MainAddress;
				org2Address.FillWithValidTestData();
				org2Address.Address1 = "456 Test Street";
				org2Address.Postcode = "X98Y7Z6";
			}
			return organisation2;
		}
	}
	OrgHeader organisation2;

	OrgHeader Organisation3
	{
		get
		{
			if (organisation3 == null)
			{
				organisation3 = Factory.NewWithValidTestData<OrgHeader>();
				organisation3.OH_FullName = "Test Company 3";
				organisation3.OH_Code = "TESTCO3";
				var org3Address = organisation3.MainAddress;
				org3Address.FillWithValidTestData();
				org3Address.Address1 = "789 Test Street";
				org3Address.Postcode = "C4DE56";
			}
			return organisation3;
		}
	}
	OrgHeader organisation3;
}
