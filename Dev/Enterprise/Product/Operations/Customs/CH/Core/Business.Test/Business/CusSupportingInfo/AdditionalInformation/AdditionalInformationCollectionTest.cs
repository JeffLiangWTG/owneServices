using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(AdditionalInformationCollection))]
public class AdditionalInformationCollectionTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		return Factory.New<JobComInvoiceLine>().AdditionalInformations;
	}
}
