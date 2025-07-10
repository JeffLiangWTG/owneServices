using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(InAndOutwardProcessingCollection))]
class InAndOutwardProcessingCollectionTest : SingleCusSupportingInfoCollectionTest<InAndOutwardProcessing>
{
	protected override BusinessObjectCollection GetCollectionToTest() => Factory.New<JobComInvoiceLine>().InAndOutwardProcessings;
}
