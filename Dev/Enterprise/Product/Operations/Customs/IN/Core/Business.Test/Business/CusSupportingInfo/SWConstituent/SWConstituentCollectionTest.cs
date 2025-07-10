using CargoWise.ComponentModel;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(SWConstituentCollection))]
sealed class SWConstituentCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<SWConstituent>
{
	public void TestMaxCountValidation()
	{
		var collection = Factory.New<JobComInvoiceLine>().SWConstituents;

		for (var i = 0; i < 9999; i++)
		{
			collection.AddNew();
		}

		CombineAssertions(() =>
		{
			AssertEquals("Maximum allowed", false, collection.HasErrors());
			AssertNoRowMessageError(collection.Last(), "The maximum number of 9999 Single Window Constituents has been exceeded.");

			var constituent = collection.AddNew();
			AssertHasRowMessageError(constituent, "The maximum number of 9999 Single Window Constituents has been exceeded.");
		});
	}

	protected override CusSupportingInfoCollection<SWConstituent> GetCusSupportingInfoCollection() => Factory.New<JobComInvoiceLine>().SWConstituents;
}
