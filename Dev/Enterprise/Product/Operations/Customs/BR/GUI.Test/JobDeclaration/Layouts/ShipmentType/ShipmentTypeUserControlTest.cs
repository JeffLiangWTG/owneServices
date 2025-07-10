using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class ShipmentTypeUserControlTest : TestCase
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(JobDeclaration), control.BindingSource.DataSourceType);
		}

		public void TestObjectsTypesForComponentsOnUserControl()
		{
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("SpecialTransportDropEdit should be ZDropEdit", control.SpecialTransportDropEdit);
				AssertType<ZCheckBox>("IsMultimodalCheckBox should be ZCheckBox", control.IsMultimodalCheckBox);
				AssertType<ZDropEdit>("DeclarantTypeDropEdit should be ZDropEdit", control.DeclarantTypeDropEdit);
				AssertType<ZDropEdit>("OperationTypeDropEdit should be ZDropEdit", control.OperationTypeDropEdit);
				AssertType<ZDropEdit>("DispatchModalityDropEdit should be ZDropEdit", control.DispatchModalityDropEdit);
				AssertType<ZDropEdit>("BRTransportModeDropEdit should be ZDropEdit", control.BRTransportModeDropEdit);
			});
		}

		ShipmentTypeUserControl control;

		protected override void SetUp()
		{
			base.SetUp();
			control = new ShipmentTypeUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
