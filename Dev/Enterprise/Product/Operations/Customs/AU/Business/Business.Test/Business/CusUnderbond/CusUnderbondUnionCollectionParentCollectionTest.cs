using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusUnderbondUnionCollectionParentCollection))]
	class CusUnderbondUnionCollectionParentCollectionTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusUnderbondUnionCollectionParentCollection(Factory, typeof(CusHAWB));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CusHAWB>();
		}
	}
}
