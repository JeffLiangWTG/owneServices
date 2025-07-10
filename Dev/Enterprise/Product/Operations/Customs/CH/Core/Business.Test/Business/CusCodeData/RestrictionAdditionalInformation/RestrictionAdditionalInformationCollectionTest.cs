using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(RestrictionAdditionalInformationCollection))]
class RestrictionAdditionalInformationCollectionTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var restriction = Factory.New<Restriction>();
		return restriction.AdditionalInformations;
	}
}
