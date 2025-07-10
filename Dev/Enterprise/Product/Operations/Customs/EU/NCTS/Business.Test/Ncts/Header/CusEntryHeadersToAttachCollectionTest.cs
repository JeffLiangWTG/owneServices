using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(CusEntryHeadersToAttachCollection))]
	internal class CusEntryHeadersToAttachCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new CusEntryHeadersToAttachCollection(Factory.New<NctsHeader>());

		public void TestRelationshipFilter()
		{
			var branch1 = Factory.LoadTop1<GlbBranch>(new ZQuery());
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Italy);

			var query = new ZDBOnlyQuery(typeof(GlbCompany));
			query.AddToFilter(JoinCondition.And, GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
			var company2 = Factory.LoadTop1<GlbCompany>(query);
			company2.SetCountry(Core.Constants.CountryCodes.Germany);

			var query2 = new ZDBOnlyQuery(typeof(GlbBranch));
			query2.AddToFilter(JoinCondition.And, GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, branch1.PK);
			var branch2 = Factory.LoadTop1<GlbBranch>(query2);
			branch2.GB_GC = company2.PK;

			CombineAssertions("Prerequisite: ", () =>
			{
				Assert("the 2 companies are differents", company2.PK != GlbCompany.CurrentCompany.PK);
				Assert("the 2 branches are differents", branch2.PK != branch1.PK);
			});

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GC = GlbCompany.CurrentCompany.PK;
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_GC = company2.PK;
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			Factory.Save();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_GB = branch1.PK;

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			var collection = new CusEntryHeadersToAttachCollection(nctsHeader);
			collection.Load();
			AssertEquals("collection should contains the entry header which has the same company of the nctsHeader.", true, collection.Contains(factory2.Load<CusEntryHeader>(entry.PK)));
			AssertEquals("collection should not contains the entry header which has a different company from the one of nctsHeader.", false, collection.Contains(factory2.Load<CusEntryHeader>(entry2.PK)));
		}
	}
}
