using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	[TestedType(typeof(AddInfoCusExitDetail))]
	public class AddInfoCusExitDetailTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new AddInfoCusExitDetail(Factory.New<CusExitDetail>().CED_AddInfoInfo);

		public void TestLookups()
		{
			AssertType<AddInfoCusExitDetailLookups>(((AddInfoCusExitDetail)GetNewBusinessObject()).Lookups);
		}
	}
}
