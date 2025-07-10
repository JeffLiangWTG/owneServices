using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	public class EMCSJobDeclarationLookupsTest
		: BusinessObjectLookupsTestCase
	{
		public void TestMessageSubTypeList()
		{
			var list = lookups.MessageSubTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("1, 2, 3, 4, 5, 6, 8, 9, 10, 11, 7", list.CodesAsString);
				AssertSame("Should be cached", list, lookups.MessageSubTypeList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<EMCSJobDeclaration>();
			lookups = new EMCSJobDeclarationLookups(declaration);
		}

		EMCSJobDeclarationLookups lookups;
	}
}
