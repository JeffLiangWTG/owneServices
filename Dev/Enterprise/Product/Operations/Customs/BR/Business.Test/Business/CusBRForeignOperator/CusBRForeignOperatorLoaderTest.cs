using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(CusBRForeignOperator.Loader))]
	class CusBRForeignOperatorLoaderTest : LoaderTestCase
	{
		public void TestLoadByOwnerAndAuthorityIdentifier()
		{
			var importer1 = OrgHeader.New(Factory);
			var importer2 = OrgHeader.New(Factory);
			var importer3 = OrgHeader.New(Factory);

			var foreignOperator1 = Factory.New<CusBRForeignOperator>();
			foreignOperator1.BFR_AuthorityIdentifier = "1";
			foreignOperator1.BFR_OH_Owner = importer1.PK;
			foreignOperator1.BFR_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			var foreignOperator2 = Factory.New<CusBRForeignOperator>();
			foreignOperator2.BFR_AuthorityIdentifier = "2";
			foreignOperator2.BFR_OH_Owner = importer1.PK;
			foreignOperator2.BFR_SystemCreateTimeUtc = ZDateTime.UtcNow;

			var foreignOperator3 = Factory.New<CusBRForeignOperator>();
			foreignOperator3.BFR_AuthorityIdentifier = "3";
			foreignOperator3.BFR_OH_Owner = importer1.PK;

			var foreignOperator4 = Factory.New<CusBRForeignOperator>();
			foreignOperator4.BFR_AuthorityIdentifier = "3";
			foreignOperator4.BFR_OH_Owner = importer2.PK;

			var loader = GetNewLoaderToTest() as CusBRForeignOperator.Loader;
			CombineAssertions(() =>
			{
				AssertEquals("Owner=importer1, AuthorityIdentifier='2'", foreignOperator2, loader.LoadByOwnerAndAuthorityIdentifier(importer1, "2"));
				AssertEquals("Owner=importer1, AuthorityIdentifier='3'", foreignOperator3, loader.LoadByOwnerAndAuthorityIdentifier(importer1, "3"));
				AssertEquals("Owner=importer2, AuthorityIdentifier='3'", foreignOperator4, loader.LoadByOwnerAndAuthorityIdentifier(importer2, "3"));
				AssertNull("Owner=importer2, AuthorityIdentifier='4'", loader.LoadByOwnerAndAuthorityIdentifier(importer2, "4"));
				AssertNull("Owner=importer3, AuthorityIdentifier='2'", loader.LoadByOwnerAndAuthorityIdentifier(importer3, "2"));
				AssertNull("Owner=null, AuthorityIdentifier=''", loader.LoadByOwnerAndAuthorityIdentifier(null, string.Empty));
				AssertNull("Owner=null, AuthorityIdentifier='2'", loader.LoadByOwnerAndAuthorityIdentifier(null, "2"));
				AssertNull("Owner=importer2, AuthorityIdentifier=''", loader.LoadByOwnerAndAuthorityIdentifier(importer2, string.Empty));
			});
		}

		public void TestLoadByOwnerAndForeignOperator()
		{
			var importer1 = OrgHeader.New(Factory);
			var importer2 = OrgHeader.New(Factory);

			var foreignOrg1 = OrgHeader.New(Factory);
			var foreignOrg2 = OrgHeader.New(Factory);

			var foreignOperator1 = Factory.New<CusBRForeignOperator>();
			foreignOperator1.BFR_OH_Owner = importer1.PK;
			foreignOperator1.BFR_OH_ForeignOperator = foreignOrg1.PK;

			var foreignOperator2 = Factory.New<CusBRForeignOperator>();
			foreignOperator2.BFR_OH_Owner = importer1.PK;
			foreignOperator2.BFR_OH_ForeignOperator = foreignOrg2.PK;

			var foreignOperator3 = Factory.New<CusBRForeignOperator>();
			foreignOperator3.BFR_OH_Owner = importer2.PK;
			foreignOperator3.BFR_OH_ForeignOperator = foreignOrg2.PK;

			var foreignOperator4 = Factory.New<CusBRForeignOperator>();
			foreignOperator4.BFR_OH_Owner = importer1.PK;

			var loader = GetNewLoaderToTest() as CusBRForeignOperator.Loader;
			CombineAssertions(() =>
			{
				AssertEquals("Owner=importer1, ForeignOperator=foreignOrg1", foreignOperator1, loader.LoadByOwnerAndForeignOperator(importer1.PK, foreignOrg1.PK));
				AssertEquals("Owner=importer1, ForeignOperator=foreignOrg2", foreignOperator2, loader.LoadByOwnerAndForeignOperator(importer1.PK, foreignOrg2.PK));
				AssertEquals("Owner=importer2, ForeignOperator=foreignOrg2", foreignOperator3, loader.LoadByOwnerAndForeignOperator(importer2.PK, foreignOrg2.PK));
				AssertNull("Owner=importer1, ForeignOperator=null", loader.LoadByOwnerAndForeignOperator(importer1.PK, ZGuid.Empty));
				AssertNull("Owner=null, ForeignOperator=foreignOrg1", loader.LoadByOwnerAndForeignOperator(ZGuid.Empty, foreignOrg1.PK));
			});
		}

		protected override BusinessObject.Loader GetNewLoaderToTest() => new CusBRForeignOperator.Loader(Factory);
	}
}
