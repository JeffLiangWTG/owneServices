using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Yard.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromCYDReleaseAdvice))]
	sealed class FreightWrapperFromCYDReleaseAdviceTest : FreightWrapperTest
	{
		public void TestJobNumber()
		{
			var releaseAdvice = Factory.New<CYDReleaseAdvice>();
			releaseAdvice.YRE_JobNumber = "D001234";
			var wrapper = new FreightWrapperFromCYDReleaseAdvice(releaseAdvice, Factory);
			AssertEquals("JobNumber", "D001234", wrapper.JobNumber);
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<CYDReleaseAdvice>();
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var advice = Factory.New<CYDReleaseAdvice>();
			return new FreightWrapperFromCYDReleaseAdvice(advice, Factory);
		}
	}
}
