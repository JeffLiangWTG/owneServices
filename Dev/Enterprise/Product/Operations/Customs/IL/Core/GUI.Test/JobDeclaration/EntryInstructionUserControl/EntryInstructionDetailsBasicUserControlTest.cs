using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.IL.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IL.GUI.Testing
{
	[TestedType(typeof(EntryInstructionDetailsBasicUserControl))]
	sealed class EntryInstructionDetailsBasicUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			using var control = new EntryInstructionDetailsBasicUserControl();
			AssertEquals("EntryInstructionDetailsBasicUserControl test data source", typeof(CusEntryInstruction), control.BindingSource.DataSourceType);
		}

		public void TestControls() => CombineAssertions(() =>
		{
			using var control = new EntryInstructionDetailsBasicUserControl();

			_ = control.AssertContainsControl<ZAddressControl>("FromWarehouseAddressControl", x => x.WithBindTo("CEI_OA_Warehouse"));
			_ = control.AssertContainsControl<ZAddressControl>("ToWarehouseAddressControl", x => x.WithBindTo("CEI_OA_Warehouse2"));
			_ = control.AssertContainsControl<ZDateEdit>("DateForDutyDateEdit", x => x.WithBindTo("CEI_DateForDuty"));
			_ = control.AssertContainsControl<ZDropEdit>("FormattedProcedureDropEdit", x => x.WithBindTo("CEI_FormattedProcedure"));
			_ = control.AssertContainsControl<ZDropEdit>("AutonomyRegionTypeDropEdit", x => x.WithBindTo("CEI_AutonomyRegionType"));
			_ = control.AssertContainsControl<ZCalcDropEdit>("PackagesQtyCalcDropEdit", x
				=> x.WithBindToAmount("CEI_NumberOfPackages").WithBindToUnit("CEI_CustomsPackType"));
		});
	}
}
