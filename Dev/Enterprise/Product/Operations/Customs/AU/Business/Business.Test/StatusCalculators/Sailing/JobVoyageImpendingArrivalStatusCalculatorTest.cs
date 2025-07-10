using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(JobVoyageImpendingArrivalStatusCalculator))]
	sealed class JobVoyageImpendingArrivalStatusCalculatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestStatusInfo()
		{
			AssertEquals("StatusInfo.Name", "Code", Calculator.StatusInfo.Name);
		}

		public void TestInterestedMessageTypes()
		{
			AssertEquals("InterestedMessageTypes.Length", 1, Calculator.InterestedMessageTypes.Length);
			AssertEquals("InterestedMessageTypes[0]", CMRMessage.CMRMessageTypes.AIRIAR, Calculator.InterestedMessageTypes[0]);
		}

		protected override BusinessObject GetNewBusinessObject() => new JobVoyageImpendingArrivalStatusCalculator(new CustomsJobVoyageWrapper(Factory.New<JobVoyage>()));

		JobVoyageImpendingArrivalStatusCalculator calculator;
		JobVoyageImpendingArrivalStatusCalculator Calculator => calculator ?? (calculator = (JobVoyageImpendingArrivalStatusCalculator)GetNewBusinessObject());
	}
}
