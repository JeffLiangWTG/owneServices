using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(BorderTransportCollection))]
class BorderTransportCollectionTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var parent = Factory.New<JobDeclaration>();
		return new BorderTransportCollection(parent);
	}

	public void TestMaxCount()
	{
		AssertEquals(1, declaration.BorderTransports.MaxCount);
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
	}
	JobDeclaration declaration;
}
