using CargoWise.ComponentModel;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(JobWorkCollection))]
sealed class JobWorkCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<JobWork>
{
	public void TestMaxCountValidation()
	{
		var collection = Factory.New<JobComInvoiceLine>().JobWorks;

		for (var i = 0; i < 99; i++)
		{
			collection.AddNew();
		}

		CombineAssertions(() =>
		{
			AssertEquals("Maximum allowed", false, collection.HasErrors());
			AssertNoRowMessageError(collection.Last(), "The maximum number of 99 Job Works has been exceeded.");

			var constituent = collection.AddNew();
			AssertHasRowMessageError(constituent, "The maximum number of 99 Job Works has been exceeded.");
		});
	}

	protected override CusSupportingInfoCollection<JobWork> GetCusSupportingInfoCollection() => Factory.New<JobComInvoiceLine>().JobWorks;
}
