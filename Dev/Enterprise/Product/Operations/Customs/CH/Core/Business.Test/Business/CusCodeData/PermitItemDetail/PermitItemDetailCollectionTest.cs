using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(PermitItemDetailCollection))]
class PermitItemDetailCollectionTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var permit = Factory.New<Permit>();
		return permit.PermitItemDetails;
	}
}
