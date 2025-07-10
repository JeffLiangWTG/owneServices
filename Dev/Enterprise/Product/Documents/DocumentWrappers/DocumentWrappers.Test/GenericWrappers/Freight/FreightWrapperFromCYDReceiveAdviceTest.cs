using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Yard.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromCYDReceiveAdvice))]
	sealed class FreightWrapperFromCYDReceiveAdviceTest : FreightWrapperTest
	{
		public void TestJobNumber()
		{
			var advice = Factory.New<CYDReceiveAdvice>();
			advice.YRA_JobNumber = "D001234";
			var wrapper = new FreightWrapperFromCYDReceiveAdvice(advice, Factory);
			AssertEquals("JobNumber", "D001234", wrapper.JobNumber);
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<CYDReceiveAdvice>();
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			CYDReceiveAdvice advice = Factory.New<CYDReceiveAdvice>();
			return new FreightWrapperFromCYDReceiveAdvice(advice, Factory);
		}
	}
}
