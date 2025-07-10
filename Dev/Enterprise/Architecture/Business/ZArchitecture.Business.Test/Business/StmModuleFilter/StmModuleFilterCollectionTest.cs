using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(StmModuleFilterCollection))]
	sealed class StmModuleFilterCollectionTest : ActiveBusinessObjectCollectionTestCase<StmModuleFilterCollection>
	{
		#region TestSetDefaultsForNewChild

		public void TestSetDefaultsForNewChild()
		{
			AssertEquals(EnvProxy.Instance.CurrentCompany.PK, Collection.AddNew().S9_GC);
			AssertEquals(EnvProxy.Instance.CurrentUser.PK, Collection.AddNew().S9_RelatedEntityID);

			StmModuleFilter layout = Collection.AddNew();
			layout.S9_IsPublished = true;

			AssertEquals("Should not set the S9_RelatedEntityID on a published filter layout.", true, layout.S9_RelatedEntityID.IsEmpty);
		}

		#endregion

		#region TestCollectionLoadsCurrentUserLayoutsAndPublishedLayoutsOnly

		public void TestCollectionLoadsCurrentUserLayoutsAndPublishedLayoutsOnly()
		{
			StmModuleFilter publishedLayout = TestDataHelper.NewPublishedLayout("Published Layout");
			StmModuleFilter scoobyLayoutData = TestDataHelper.NewLayoutWithUserData("Scooby");
			StmModuleFilter shaggyLayoutData = TestDataHelper.NewLayoutWithUserData("Shaggy", ZGuid.NewZGuid(), GlbStaffSchema.Constants.Prefix);

			AssertCollectionContains("Collection should contain published layouts, whether or not the user has data for it", publishedLayout, Collection);
			AssertCollectionContains(scoobyLayoutData, Collection);
			AssertCollectionNotContains("Layout belongs to a different user and should not have been loaded.", shaggyLayoutData, Collection);
		}

		#endregion

		#region TestCollectionLoadsLayoutsForCurrentModuleOnly

		public void TestCollectionLoadsLayoutsForCurrentModuleOnly()
		{
			StmModuleFilter layoutForCurrentModule = TestDataHelper.NewLayoutWithUserData("Layout");
			StmModuleFilter layoutForOtherModule = TestDataHelper.NewLayoutWithUserData("Layout", "Other ModuleID");

			AssertCollectionContains(layoutForCurrentModule, Collection);
			AssertCollectionNotContains(layoutForOtherModule, Collection);
		}

		#endregion

		#region TestCollectionLoadsLayoutsForCurrentOrBlankCompanyOnly

		public void TestCollectionLoadsLayoutsForCurrentOrBlankCompanyOnly()
		{
			StmModuleFilter layoutForCurrentCompany = TestDataHelper.NewLayoutWithUserData("Layout");
			StmModuleFilter layoutForOtherCompany = TestDataHelper.NewLayoutWithUserData("Layout in other Company");
			StmModuleFilter layoutForBlankCompany = TestDataHelper.NewLayoutWithUserData("Layout in blank Company");

			layoutForOtherCompany.S9_GC = Factory.New<IGlbCompany>().PK;
			layoutForBlankCompany.S9_GC = ZGuid.Empty;
			Factory.Save();

			AssertCollectionContains(layoutForCurrentCompany, Collection);
			AssertCollectionNotContains(layoutForOtherCompany, Collection);
			AssertCollectionContains(layoutForBlankCompany, Collection);
		}

		#endregion

		#region Implementation

		new StmModuleFilterCollection Collection
		{
			get { return base.Collection; }
		}

		protected override StmModuleFilterCollection GetCollectionToTest()
		{
			return new StmModuleFilterCollection(Factory, LayoutsTestDataHelper.TestModuleID, new FilterStripLayoutsHelper());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			StmModuleFilter result = Factory.New<StmModuleFilter>();

			result.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
			result.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
			result.S9_ModuleID = LayoutsTestDataHelper.TestModuleID;

			return result;
		}

		LayoutsTestDataHelper TestDataHelper
		{
			get { return fTestDataHelper ?? (fTestDataHelper = new LayoutsTestDataHelper(Factory)); }
		}

		LayoutsTestDataHelper fTestDataHelper;

		#endregion
	}
}
