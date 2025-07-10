using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Testing
{
	sealed class ScehemeCollectionTest : TestCaseWithFactory
	{
		public void TestFindSchemeOnCollection()
		{
			var schemes = new SchemeCollection(Factory);

			var scheme1 = Factory.New<GridColourScheme>();
			scheme1.S9_FilterName = "i";
			var scheme2 = Factory.New<GridColourScheme>();

			schemes.Add(scheme1);
			schemes.Add(scheme2);

			AssertEquals(scheme1, schemes.FindByPK(scheme1.PK));
			AssertEquals(scheme2, schemes.FindByPK(scheme2.PK));

			AssertEquals(scheme1, schemes.GetSchemes(new ZQuery(StmModuleFilterSchema.S9_FilterName, "i"))[0]);
			AssertEquals(scheme1, schemes.GetScheme(new ZQuery(StmModuleFilterSchema.S9_FilterName, "i")));
		}
	}
}
