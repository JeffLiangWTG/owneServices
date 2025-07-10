using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocCYDYardUnitState))]
	internal class DocCYDYardUnitStateTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { unitStateWrapper };
		}

		public void TestSize()
		{
			unitState.Container.RC_Length = 20.000;
			AssertEquals("20", unitStateWrapper.Size);
		}

		CYDYardUnitState unitState;

		DocCYDYardUnitState unitStateWrapper;

		protected override void SetUp()
		{
			var container = Factory.NewWithValidTestData<RefContainer>();

			var delivery = Factory.NewWithValidTestData<CYDDelivery>();
			delivery.UnitLineItem.YLI_RC_ContainerType = container.PK;

			unitState = Factory.New<CYDYardUnitState>();
			unitState.YUS_YDL_Delivery = delivery.PK;

			unitStateWrapper = DocCYDYardUnitState.New(unitState, Factory);

			base.SetUp();
		}
	}
}
