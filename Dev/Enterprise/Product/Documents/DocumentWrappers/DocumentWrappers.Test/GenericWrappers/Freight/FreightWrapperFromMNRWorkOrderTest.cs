using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Yard.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromMNRWorkOrder))]
	sealed class FreightWrapperFromMNRWorkOrderTest : FreightWrapperTest
	{
		MNRWorkOrderHeader workOrderHeader;
		FreightWrapperFromMNRWorkOrder freightWrapperFromMNRWorkOrder;

		protected override void SetUp()
		{
			base.SetUp();
			workOrderHeader = GetNewBusinessObjectToWrap() as MNRWorkOrderHeader;
			freightWrapperFromMNRWorkOrder = GetSetupWrapperForDefaultFormatting() as FreightWrapperFromMNRWorkOrder;
		}

		public void TestJobNumber()
		{
			workOrderHeader.MWO_JobNumber = "D001234";
			AssertEquals("JobNumber", "D001234", freightWrapperFromMNRWorkOrder.JobNumber);
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<MNRWorkOrderHeader>();
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new FreightWrapperFromMNRWorkOrder(workOrderHeader, Factory);
		}
	}
}
