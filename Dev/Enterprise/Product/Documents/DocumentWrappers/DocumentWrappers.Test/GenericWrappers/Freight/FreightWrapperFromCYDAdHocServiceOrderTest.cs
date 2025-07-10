using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Yard.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromCYDAdHocServiceOrder))]
	sealed class FreightWrapperFromCYDAdHocServiceOrderTest : FreightWrapperTest
	{
		CYDAdHocServiceOrder adHocServiceOrder;
		FreightWrapperFromCYDAdHocServiceOrder freightWrapperFromCYDAdHocServiceOrder;

		protected override void SetUp()
		{
			base.SetUp();
			adHocServiceOrder = GetNewBusinessObjectToWrap() as CYDAdHocServiceOrder;
			freightWrapperFromCYDAdHocServiceOrder = GetSetupWrapperForDefaultFormatting() as FreightWrapperFromCYDAdHocServiceOrder;
		}

		public void TestJobNumber()
		{
			adHocServiceOrder.YAO_JobNumber = "D001234";
			AssertEquals("JobNumber", "D001234", freightWrapperFromCYDAdHocServiceOrder.JobNumber);
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<CYDAdHocServiceOrder>();
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new FreightWrapperFromCYDAdHocServiceOrder(adHocServiceOrder, Factory);
		}
	}
}
