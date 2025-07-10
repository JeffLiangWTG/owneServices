using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(VoyageDestinationActualArrivalStatusCalculator))]
	sealed class VoyageDestinationActualArrivalStatusCalculatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestStatusInfo()
		{
			AssertEquals("StatusInfo.Name", "Code", Calculator.StatusInfo.Name);
		}

		public void TestInterestedMessageTypes()
		{
			AssertEquals("InterestedMessageTypes.Length", 1, Calculator.InterestedMessageTypes.Length);
			AssertEquals("InterestedMessageTypes[0]", CMRMessage.CMRMessageTypes.AIRAAR, Calculator.InterestedMessageTypes[0]);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var voyage = Factory.New<JobVoyage>();
			var destination = voyage.Destinations.AddNew();
			var jobVoyageWrapper = new CustomsJobVoyageWrapper(voyage);
			var destinationWrapper = new CustomsVoyageDestinationWrapper(jobVoyageWrapper, destination);
			return new VoyageDestinationActualArrivalStatusCalculator(destinationWrapper);
		}

		VoyageDestinationActualArrivalStatusCalculator calculator;
		VoyageDestinationActualArrivalStatusCalculator Calculator => calculator ?? (calculator = (VoyageDestinationActualArrivalStatusCalculator)GetNewBusinessObject());
	}
}
