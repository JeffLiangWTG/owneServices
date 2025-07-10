using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Favorites.Testing
{
	[TestedType(typeof(StmLink))]
	sealed class StmLinkTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLinksAreEqual()
		{
			var itemPK = ZGuid.NewZGuid();

			var link1 = Factory.New<StmLink>();
			link1.STL_ItemPK = itemPK;
			link1.STL_ModuleID = "Dummy";
			link1.STL_ItemDescription = "description123";

			var link2 = Factory.New<StmLink>();
			link2.STL_ItemPK = itemPK;
			link2.STL_ModuleID = "Dummy";
			link2.STL_ItemDescription = "description789";

			AssertEquals("Links are treated as equal", link2, link1);

			link2.STL_ModuleID = "Dummy2";
			AssertNotEquals("Links are not treated as equal", link2, link1);

			link2.STL_ModuleID = "Dummy";
			link2.STL_ItemPK = ZGuid.NewZGuid();
			AssertNotEquals("Links are not treated as equal", link2, link1);
		}

		public void TestIsModule()
		{
			link.STL_ModuleID = "Dummy";
			link.STL_ItemPK = ZGuid.Empty;

			Assert("Is a Module", link.IsModule);

			link.STL_ItemPK = ZGuid.NewZGuid();
			Assert("Is Not a Module", !link.IsModule);
		}

		public void TestUniqueKey()
		{
			link.STL_ModuleID = "Dummy";
			link.STL_ItemPK = ZGuid.Empty;

			AssertEquals("Dummy", link.UniqueKey);

			var itemPK = ZGuid.NewZGuid();
			link.STL_ItemPK = itemPK;
			AssertEquals(string.Format("Dummy{0}", itemPK.ToString()), link.UniqueKey);
		}

		[ExpectNoExceptions]
		public void TestHashCodeOnDeletedObject()
		{
			link.Delete();
			var hashCode = link.GetHashCode();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return link;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return link;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return link;
		}

		protected override void SetUp()
		{
			base.SetUp();
			link = Factory.New<StmLink>();
			link.STL_LinkType = StmLinkConstants.Favorites;
			link.STL_LastUsedDateTimeUtc = ZDateTime.UtcNow;
			link.STL_GC_LogonCompany = Factory.LoadTop1<IGlbCompany>(new ZQuery()).PK;
			link.STL_GS_NKUser = "E";
		}

		StmLink link;
	}
}
