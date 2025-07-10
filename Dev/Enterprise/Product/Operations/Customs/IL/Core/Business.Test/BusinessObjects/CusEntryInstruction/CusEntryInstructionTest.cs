using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.FetchStrategies;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(CusEntryInstruction))]
	sealed class CusEntryInstructionTest : Customs.Business.Testing.CusEntryInstructionAbstractTest
	{
		public void TestBaseType()
		{
			AssertEquals(typeof(AutoILCusEntryInstruction), typeof(CusEntryInstruction).BaseType);
		}

		public void TestValidation()
		{
			AssertType<CusEntryInstructionValidation>(instruction.Validation);
			AssertEquals(typeof(AutoILCusEntryInstructionValidation), typeof(CusEntryInstructionValidation).BaseType);
		}

		public void TestLookups()
		{
			AssertType<CusEntryInstructionLookups>(instruction.Lookups);
		}

		public void TestMaxLength()
		{
			AssertEquals("CEI_AutonomyRegionType", 2, instruction.CEI_AutonomyRegionTypeInfo.MaxLength);
			AssertEquals("CEI_FormattedProcedure", 7, instruction.CEI_FormattedProcedureInfo.MaxLength);
			AssertEquals("CEI_CustomsPackType", 2, instruction.CEI_CustomsPackTypeInfo.MaxLength);
		}

		public void TestCaptions()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CEI_AutonomyRegionType", "Autonomy Region", DataBoundResourceStrings.GetDataForProperty(instruction.CEI_AutonomyRegionTypeInfo).Caption);
				AssertEquals("CEI_FormattedProcedure", "Procedure", DataBoundResourceStrings.GetDataForProperty(instruction.CEI_FormattedProcedureInfo).Caption);
				AssertEquals("CEI_OA_Warehouse2", "To Warehouse Address", DataBoundResourceStrings.GetDataForProperty(instruction.CEI_OA_Warehouse2Info).Caption);
				AssertEquals("ToWarehouseOrgPK", "To Warehouse", DataBoundResourceStrings.GetDataForProperty(instruction.ToWarehouseOrgPKInfo).Caption);
				AssertEquals("CEI_OA_Warehouse", "From Warehouse Address", DataBoundResourceStrings.GetDataForProperty(instruction.CEI_OA_WarehouseInfo).Caption);
				AssertEquals("FromWarehouseOrgPK", "From Warehouse", DataBoundResourceStrings.GetDataForProperty(instruction.FromWarehouseOrgPKInfo).Caption);
				AssertEquals("CEI_DateForDuty", "Taxation Date", DataBoundResourceStrings.GetDataForProperty(instruction.CEI_DateForDutyInfo).Caption);
			});
		}

		public void TestListAttributes()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(CusEntryInstruction), "CEI_AutonomyRegionType", false, attrib => attrib.ListDataSourceMember == "Lookups.AutonomyRegionTypeList");
			AssertHasCustomAttribute<ListAttribute>(typeof(CusEntryInstruction), "CEI_FormattedProcedure", false, attrib => attrib.ListDataSourceMember == "Lookups.ProcedureCodeList");
			AssertHasCustomAttribute<ListAttribute>(typeof(CusEntryInstruction), "CEI_OA_Warehouse", false, attrib => attrib.ListDataSourceMember == "Lookups.FromWarehouseBondedWarehouseAddressList");
			AssertHasCustomAttribute<ListAttribute>(typeof(CusEntryInstruction), "FromWarehouseOrgPK", false, attrib => attrib.ListDataSourceMember == "Lookups.BondedWarehouseCollection");
			AssertHasCustomAttribute<ListAttribute>(typeof(CusEntryInstruction), "CEI_OA_Warehouse2", false, attrib => attrib.ListDataSourceMember == "Lookups.ToWarehouseBondedWarehouseAddressList");
			AssertHasCustomAttribute<ListAttribute>(typeof(CusEntryInstruction), "ToWarehouseOrgPK", false, attrib => attrib.ListDataSourceMember == "Lookups.BondedWarehouseCollection");
		}

		public void TestCEI_FormattedProcedure()
		{
			instruction.CEI_FormattedProcedure = "1234567";
			CombineAssertions("When CEI_FormattedProcedure == 1234567", () =>
			{
				AssertEquals("CEI_FormattedProcedure", "1234567", instruction.CEI_FormattedProcedure);
				AssertEquals("CEI_Procedure = value.Left(4)", "1234", instruction.CEI_Procedure);
				AssertEquals("CEI_Style = value.SubstringSafe(4, 3)", "567", instruction.CEI_Style);
			});

			instruction.CEI_FormattedProcedure = "123456";
			CombineAssertions("When CEI_FormattedProcedure == 123456", () =>
			{
				AssertEquals("CEI_FormattedProcedure", "123456", instruction.CEI_FormattedProcedure);
				AssertEquals("CEI_Procedure = value.Left(4)", "1234", instruction.CEI_Procedure);
				AssertEquals("CEI_Style = value.SubstringSafe(4, 3)", "56", instruction.CEI_Style);
			});
		}

		public void TestCEI_Description_WithHebrewCharacters()
		{
			instruction.CEI_Description = "Description with Hebrew characters אבגד";
			Factory.Save();
			AssertEquals("Description that contains Hebrew characters", "Description with Hebrew characters אבגד", instruction.CEI_Description);
		}

		public void TestCEI_NumberOfPackages()
		{
			AssertEntity<CusEntryInstruction>()
			.HasProperty(p => p.CEI_NumberOfPackages)
			.WithCaption("Customs Quantity");
		}

		public void TestCEI_CustomsPackType()
		{
			AssertEntity<CusEntryInstruction>()
			.HasProperty(p => p.CEI_CustomsPackType)
			.WithCaption("Package Type")
			.WithShortCaption("Pack Type")
			.WithList("Lookups.PackageUQList");
		}

		public void TestPreviousDocuments()
		{
			AssertType<PreviousDocumentCollection>(instruction.PreviousDocuments);
		}

		public void TestGetCusSupportingInfoTypes()
		{
			var actualTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)instruction).GetCusSupportingInfoTypes();
			AssertEquals("Previous Document", typeof(PreviousDocument), actualTypes[Common.IL.CusSupportingInfoTypeList.Codes.PreviousDocument]);
		}

		public void TestGetFetchStrategies()
		{
			var expectedTypes = new[] { typeof(CusSupportingInfoTypeSupporterFetchStrategy) };
			var actualTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)instruction).GetFetchStrategies().Select(c => c.GetType());
			AssertContainsExactElementsInAnyOrder(expectedTypes, actualTypes);
		}

		protected override void SetUp()
		{
			base.SetUp();
			instruction = Factory.NewWithValidTestData<CusEntryInstruction>();
		}
		CusEntryInstruction instruction;
	}
}
