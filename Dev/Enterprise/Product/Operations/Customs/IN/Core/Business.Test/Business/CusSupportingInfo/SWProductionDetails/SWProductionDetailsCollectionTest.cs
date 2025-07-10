using CargoWise.ComponentModel;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(SWProductionDetailsCollection))]
sealed class SWProductionDetailsCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<SWProduction>
{
	public void TestMaxCountValidation()
	{
		var collection = Factory.New<JobComInvoiceLine>().SWProductions;

		for (var i = 0; i < 9999; i++)
		{
			collection.AddNew();
		}

		CombineAssertions(() =>
		{
			AssertEquals("Maximum allowed", false, collection.HasErrors());
			AssertNoRowMessageError(collection.Last(), "The maximum number of 9999 SW Production Details has been exceeded.");

			var productions = collection.AddNew();
			AssertHasRowMessageError(productions, "The maximum number of 9999 SW Production Details has been exceeded.");
		});
	}

	protected override CusSupportingInfoCollection<SWProduction> GetCusSupportingInfoCollection() => Factory.New<JobComInvoiceLine>().SWProductions;
}
