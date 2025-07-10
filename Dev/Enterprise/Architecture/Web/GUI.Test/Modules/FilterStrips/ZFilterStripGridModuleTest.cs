using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.FilterStrips;
using Enterprise.ZArchitecture.Web.GUI.FilterStrips;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	sealed class ZFilterStripGridModuleTest : TestCase
	{
		#region TestFilterControlType

		public void TestFilterControlType()
		{
			AssertEquals(typeof(ZFilterStripControl), Module.FilterControlType);
		}

		#endregion

		#region TestGetNewFilterControlResource

		public void TestGetNewFilterControlResource()
		{
			using (var temp = new TempDirectory())
			{
				Module.FilterControlResource.SetServerMappedPathForTest(temp.DirectoryName);
				Module.FilterControlResource.Extract();
				AssertEquals(true, Module.FilterControlResource.FileName.EndsWith(@"/ZFilterGridModule/ZFilterStripGridModule/ZFilterStripControl.ascx"));
			}
		}

		#endregion

		#region TestFilterStripBusinessObjectIsInitialised

		public void TestFilterStripBusinessObjectIsInitialised()
		{
			DummyZFilterStripGridModule lazyCreateModule = Module;
			Page.OnLoad();
			FilterStripBusinessObject filterStripBizO = (FilterStripBusinessObject)Page.DataSource;

			AssertEquals(true, filterStripBizO.LayoutsHelper is FilterStripLayoutsHelperForWeb);
			AssertEquals(OrgContactSchema.Constants.Prefix, filterStripBizO.LayoutsHelper.CurrentUserTablePrefix);
			AssertEquals(DummyModuleIDs.Dummy.Name, ((IFilterStripBusinessObjectInternals)filterStripBizO).LayoutContext);
		}

		#endregion

		#region TestOverrideFilterStripBusinessObject

		public void TestOverrideFilterStripBusinessObject()
		{
			DummyZFilterStripGridModule lazyCreateModule = Module;
			Page.OnLoad();
			FilterStripBusinessObject filterStripBizOPageDataSource = (FilterStripBusinessObject)Page.DataSource;

			AssertEquals(true, filterStripBizOPageDataSource.LayoutsHelper is FilterStripLayoutsHelperForWeb);
			AssertEquals(OrgContactSchema.Constants.Prefix, filterStripBizOPageDataSource.LayoutsHelper.CurrentUserTablePrefix);
			AssertEquals(DummyModuleIDs.Dummy.Name, ((IFilterStripBusinessObjectInternals)filterStripBizOPageDataSource).LayoutContext);

			FilterStripBusinessObject filterStripBizOToOverride = Page.GetNewDummyFilterStripBizO();

			AssertEquals(false, filterStripBizOToOverride.LayoutsHelper is FilterStripLayoutsHelperForWeb);

			lazyCreateModule.OverrideFilterStripBizO(filterStripBizOToOverride);
			Page.OnLoad();

			AssertEquals(true, filterStripBizOToOverride.LayoutsHelper is FilterStripLayoutsHelperForWeb);
			AssertEquals(OrgContactSchema.Constants.Prefix, filterStripBizOToOverride.LayoutsHelper.CurrentUserTablePrefix);
			AssertEquals(DummyModuleIDs.Dummy.Name, ((IFilterStripBusinessObjectInternals)filterStripBizOToOverride).LayoutContext);
		}

		#endregion

		#region TestCreateModuleWithoutPage

		[ExpectNoExceptions]
		public void TestCanBeCreatedWithoutPage()
		{
			using (DummyZFilterStripGridModule module = new DummyZFilterStripGridModule(new BusinessObjectFactory(), null))
			{
				AssertNotNull("Can be created without page", module);
			}
		}

		#endregion

		public void TestGetEDocsBulkDownloadRelatedPKs()
		{
			var factory = new BusinessObjectFactory();

			var relatedPK = ZGuid.NewZGuid();
			var parentBizo = new DummyNonPersistentBusinessObjectWithIWebDocumentsSupportBase(relatedPK);
			var parentBizo2 = new DummyNonPersistentBusinessObject();

			var dummy = factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_VarCharMax = "ABC";

			var grid = new ZDataGrid();
			grid.Page = new ZTestPage();
			grid.BindTo = "Collection";
			dummy.Collection.Add(parentBizo);
			dummy.Collection.Add(parentBizo2);
			grid.Bind(dummy);

			using (var module = new DummyZFilterStripGridModule(factory, null))
			{
				var actual = module.GetEDocsBulkDownloadRelatedPKs(grid, parentBizo.PK);
				AssertContainsExactElementsInAnyOrder(actual, new[] { parentBizo.PK, relatedPK });

				actual = module.GetEDocsBulkDownloadRelatedPKs(grid, parentBizo2.PK);
				AssertEquals("PK2 Count", 0, actual.Count);
			}
		}

		class DummyNonPersistentBusinessObjectWithIWebDocumentsSupportBase : DummyNonPersistentBusinessObject, IWebDocumentsSupportBase
		{
			public DummyNonPersistentBusinessObjectWithIWebDocumentsSupportBase(ZGuid docRelatedPk)
			{
				this.docRelatedPk = docRelatedPk;
			}

			public ZGuid DocParentPK => throw new NotImplementedException();

			public List<ZGuid> DocRelatedPKs => new ZGuid[] { this.PK, docRelatedPk }.ToList();

			readonly ZGuid docRelatedPk;
		}

		#region Implementation

		protected override void TearDown()
		{
			base.TearDown();
			if (fModule != null)
			{
				fModule.Dispose();
			}
		}

		DummyZFilterStripGridModule Module
		{
			get { return fModule ?? (fModule = new DummyZFilterStripGridModule(new BusinessObjectFactory(), Page)); }
		}

		DummyPage Page
		{
			get { return fPage ?? (fPage = new DummyPage()); }
		}

		DummyZFilterStripGridModule fModule;
		DummyPage fPage;

		#endregion
	}
}
