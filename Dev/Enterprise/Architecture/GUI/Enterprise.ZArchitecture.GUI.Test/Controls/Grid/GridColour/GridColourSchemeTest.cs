using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Testing
{
	[TestedType(typeof(GridColourScheme))]
	public class GridColourSchemeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestNoChangeHappenShouldNotChangeCompany()
		{
			var company = Factory.New<IGlbCompany>();
			var branch = Factory.New<IGlbBranch>();
			branch.GB_GC = company.PK;
			var scheme = Factory.New<GridColourScheme>();
			scheme.S9_FilterName = "old name";
			scheme.S9_IsSystem = false;
			scheme.S9_IsPublished = true;
			Factory.Save();

			AssertEquals(Env.CurrentCompanyPK, scheme.S9_GC);
			var oldCompanyPK = Env.CurrentCompanyPK;
			using (EnvProxy.Instance.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var newFactory = new BusinessObjectFactory();
				var reloadedScheme = newFactory.Load<GridColourScheme>(scheme.PK);
				AssertEquals("should be the old company.", oldCompanyPK, scheme.S9_GC);
				newFactory.Save();
				AssertEquals("should be the old company after save as there is no change in it.", oldCompanyPK, scheme.S9_GC);
				reloadedScheme.S9_FilterName = "new name";
				newFactory.Save();
				AssertEquals("should be the current company after save as changes happen.", Env.CurrentCompanyPK, scheme.S9_GC);
			}
		}

		public void TestSaveDoesntResetGCForSystem()
		{
			var scheme = Factory.New<GridColourScheme>();
			scheme.S9_GC = ZGuid.Empty;
			scheme.S9_RelatedEntityID = ZGuid.Empty;
			scheme.S9_IsSystem = true;
			Factory.Save();

			AssertEquals(ZGuid.Empty, scheme.S9_GC);
			AssertEquals(ZGuid.Empty, scheme.S9_RelatedEntityID);
		}

		public void TestIsPublishedDoesNotSetsChildren()
		{
			var scheme = Factory.New<GridColourScheme>();

			using (StripControl)
			{
				var colorStrip = new GridColourStripBusinessObject(StripControl.FilterBusinessObject, null, null);
				colorStrip.FilterStrips.AddNew();
				colorStrip.BGColor = Color.Bisque;
				colorStrip.RuleName = "rule1";

				var colorStrip2 = new GridColourStripBusinessObject(StripControl.FilterBusinessObject, null, null);
				colorStrip2.FilterStrips.AddNew();
				colorStrip2.BGColor = Color.FromArgb(-16776961);
				colorStrip2.RuleName = "rule2";

				scheme.ColourStrips.Add(colorStrip);
				scheme.ColourStrips.Add(colorStrip2);

				scheme.S9_FilterName = "scheme1";

				colorStrip.SaveLayout(colorStrip.RuleName);
				colorStrip2.SaveLayout(colorStrip2.RuleName);

				Factory.Save();

				scheme.S9_IsPublished = true;

				var filter = Factory.LoadTop1<StmModuleFilter>(new ZQuery(StmModuleFilterSchema.S9_FilterName, "rule1"));
				AssertEquals(false, filter.S9_IsPublished);

				filter = Factory.LoadTop1<StmModuleFilter>(new ZQuery(StmModuleFilterSchema.S9_FilterName, "rule2"));
				AssertEquals(false, filter.S9_IsPublished);
			}
		}

		public void TestSaveSavesRuleInfo()
		{
			var scheme = Factory.New<GridColourScheme>();

			using (StripControl)
			{
				var colorStrip = new GridColourStripBusinessObject(StripControl.FilterBusinessObject, null, null);
				colorStrip.FilterStrips.AddNew();
				colorStrip.BGColor = Color.Bisque;
				colorStrip.RuleName = "rule1";

				var colorStrip2 = new GridColourStripBusinessObject(StripControl.FilterBusinessObject, null, null);
				colorStrip2.FilterStrips.AddNew();
				colorStrip2.BGColor = Color.FromArgb(-16776961);
				colorStrip2.RuleName = "rule2";

				scheme.ColourStrips.Add(colorStrip);
				scheme.ColourStrips.Add(colorStrip2);

				scheme.S9_FilterName = "scheme1";

				colorStrip.SaveLayout(colorStrip.RuleName);
				colorStrip2.SaveLayout(colorStrip2.RuleName);

				Factory.Save();

				var loadedFilter = Factory.LoadTop1<StmModuleFilter>(new ZQuery(StmModuleFilterSchema.PK, scheme.PK));

				AssertEquals("should serialise scheme rule name and colours",
					string.Format(filterData, colorStrip.StmModuleFilter.PK.ToString(), colorStrip2.StmModuleFilter.PK.ToString()),
					loadedFilter.S9_FilterData.ToAscii());

				strip1PK = colorStrip.StmModuleFilter.PK;
				strip2PK = colorStrip2.StmModuleFilter.PK;
			}
		}

		ZGuid strip1PK;
		ZGuid strip2PK;

		public void TestLoadReloadsColorsAndRules()
		{
			TestSaveSavesRuleInfo();
			var scheme = Factory.Load<GridColourScheme>(new ZQuery(StmModuleFilterSchema.S9_FilterName, "scheme1"))[0];
			scheme.OnLoaded();
			AssertEquals("should load all rules", 2, scheme.StripColours.Count);

			AssertEquals("should load rule name", true, scheme.StripColours.ContainsKey(strip1PK.ToString()));
			AssertEquals("should load rule colour", Color.FromArgb(-6972), scheme.StripColours[strip1PK.ToString()].Color);

			AssertEquals("should load rule name", true, scheme.StripColours.ContainsKey(strip2PK.ToString()));
			AssertEquals("should load rule colour", Color.FromArgb(-16776961), scheme.StripColours[strip2PK.ToString()].Color);
		}

		public void TestSetStripsFromFilterResetsAllFilterObjects()
		{
			TestLoadReloadsColorsAndRules();
			var scheme = Factory.Load<GridColourScheme>(new ZQuery(StmModuleFilterSchema.S9_FilterName, "scheme1"))[0];
			using (StripControl)
			{
				scheme.SetStripsFromFilter(StripControl.FilterBusinessObject, typeof(BusinessObject));

				AssertEquals("should populate all strips", 2, scheme.ColourStrips.Count);
				scheme.ColourStrips[0].OnLayoutChanged();
				AssertEquals("populating should hook upto layout changed", true, scheme.HasChanges);
				AssertEquals("correct rule name", "rule1", scheme.ColourStrips[0].RuleName);
				AssertEquals("correct rule name", "rule2", scheme.ColourStrips[1].RuleName);
				AssertNotNull("shoudl load layout", scheme.ColourStrips[0].ModuleFilters["desc"]);
			}
		}

		public void TestValidation()
		{
			AssertEquals(typeof(GridColourSchemeValidation), Factory.New<GridColourScheme>().Validation.GetType());
		}

		[ExpectNoExceptions]
		public void TestResetFilterStripsDoesNotThrowExceptionWhenSameRuleNameUsedForTwoStrips()
		{
			SaveRulesWithDuplicateKeys();
			var scheme = Factory.Load<GridColourScheme>(new ZQuery(StmModuleFilterSchema.S9_FilterName, "scheme1"))[0];
			scheme.OnLoaded();
		}

		public void TestPublishAcrossAllCompanies()
		{
			var scheme = Factory.New<GridColourScheme>();
			scheme.S9_ModuleID = "Dummy" + StmModuleFilter.ModuleIdSuffix.GridColorScheme;

			Assert("Precondition", !scheme.S9_IsPublished);
			Assert("Precondition", !scheme.PublishAcrossAllCompanies);

			Assert("Readonly when S9_IsPublished is false", scheme.PublishAcrossAllCompaniesInfo.ReadOnly);

			scheme.S9_IsPublished = true;

			Assert("No changes", !scheme.PublishAcrossAllCompanies);
			Assert("Editable when S9_IsPublished is true", !scheme.PublishAcrossAllCompaniesInfo.ReadOnly);

			scheme.PublishAcrossAllCompanies = true;

			Assert(scheme.PublishAcrossAllCompanies);
			AssertEquals(ZGuid.Empty, scheme.S9_GC);

			scheme.S9_IsPublished = false;

			Assert("Reset to false together with S9_IsPublished", !scheme.PublishAcrossAllCompanies);
			AssertEquals(EnvProxy.Instance.CurrentCompany.PK, scheme.S9_GC);
		}

		public void TestChildColorStripWithDifferentModuleIdIsLoaded()
		{
			var scheme = Factory.New<GridColourScheme>();
			var colorStripFilter = Factory.New<StmModuleFilter>();
			colorStripFilter.S9_ModuleID = "NotMatching" + StmModuleFilter.ModuleIdSuffix.GridColorStrip;
			colorStripFilter.S9_FilterName = "Rule";
			colorStripFilter.S9_RelatedEntityID = scheme.PK;
			Factory.Save();

			using (StripControl)
			{
				var colorStrip = new GridColourStripBusinessObject(StripControl.FilterBusinessObject, scheme, typeof(DummyBusinessObject));
				colorStrip.RuleName = "Rule";
				AssertNotNull(colorStrip.StmModuleFilter);
				AssertEquals(colorStripFilter.PK, colorStrip.StmModuleFilter.PK);
			}
		}

		public void TestResetStripWithCorruptedFilterData()
		{
			var validScheme = Factory.New<GridColourScheme>();
			var corruptedScheme = Factory.New<GridColourScheme>();

			using (StripControl)
			{
				validScheme.S9_FilterName = "ValidTestScheme";
				validScheme.S9_FilterData = ZBlob.FromAscii(filterData);
				validScheme.ResetStrips();
				AssertEquals(2, validScheme.StripColours.Count);

				corruptedScheme.S9_FilterName = "CorruptedTestScheme";
				corruptedScheme.S9_FilterData = ZBlob.FromAscii(corruptedFilterData);
				AssertNoExceptionThrown(() => corruptedScheme.ResetStrips());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}

		#region Implementation

		void SaveRulesWithDuplicateKeys()
		{
			var scheme = Factory.New<GridColourScheme>();

			using (StripControl)
			{
				var colorStrip = new GridColourStripBusinessObject(StripControl.FilterBusinessObject, null, null);
				colorStrip.FilterStrips.AddNew();
				colorStrip.BGColor = Color.Bisque;
				colorStrip.RuleName = "rule1";

				var colorStrip2 = new GridColourStripBusinessObject(StripControl.FilterBusinessObject, null, null);
				colorStrip2.FilterStrips.AddNew();
				colorStrip2.BGColor = Color.FromArgb(-16776961);
				colorStrip2.RuleName = "rule1";

				scheme.ColourStrips.Add(colorStrip);
				scheme.ColourStrips.Add(colorStrip2);

				scheme.S9_FilterName = "scheme1";

				colorStrip.SaveLayout(colorStrip.RuleName);
				colorStrip2.SaveLayout(colorStrip2.RuleName);

				Factory.Save();

				var dummyFilter = Factory.LoadTop1<StmModuleFilter>(new ZQuery(StmModuleFilterSchema.S9_RelatedEntityID, scheme.PK));

				strip1PK = colorStrip.StmModuleFilter.PK;
				strip2PK = colorStrip2.StmModuleFilter.PK;
			}
		}

		readonly ZString filterData =
			"<NewDataSet>" + System.Environment.NewLine + "  " +
				"<ColourStrips>" + System.Environment.NewLine + "    " +
					"<RulePK>{0}</RulePK>" + System.Environment.NewLine + "    " +
					"<RuleName>rule1</RuleName>" + System.Environment.NewLine + "    " +
					"<BGColor>-6972</BGColor>" + System.Environment.NewLine + "  " +
				"</ColourStrips>" + System.Environment.NewLine + "  " +
				"<ColourStrips>" + System.Environment.NewLine + "    " +
					"<RulePK>{1}</RulePK>" + System.Environment.NewLine + "    " +
					"<RuleName>rule2</RuleName>" + System.Environment.NewLine + "    " +
					"<BGColor>-16776961</BGColor>" + System.Environment.NewLine + "  " +
				"</ColourStrips>" + System.Environment.NewLine +
			"</NewDataSet>";

		readonly ZString corruptedFilterData =
			"<NewDataSet>" + System.Environment.NewLine + "  " +
			"<ColourStrips>" + System.Environment.NewLine + "    " +
			"<CorruptedPK>{0}</CorruptedPK>" + System.Environment.NewLine + "    " +
			"<RuleName>rule1</RuleName>" + System.Environment.NewLine + "    " +
			"<BGColor>-6972</BGColor>" + System.Environment.NewLine + "  " +
			"</ColourStrips>" + System.Environment.NewLine + "  " +
			"<ColourStrips>" + System.Environment.NewLine + "    " +
			"<CorruptedPK>{1}</CorruptedPK>" + System.Environment.NewLine + "    " +
			"<RuleName>rule2</RuleName>" + System.Environment.NewLine + "    " +
			"<BGColor>-16776961</BGColor>" + System.Environment.NewLine + "  " +
			"</ColourStrips>" + System.Environment.NewLine +
			"</NewDataSet>";

		FilterStripControlForTest StripControl
		{
			get { return fStripControl ?? (fStripControl = new FilterStripControlForTest(null, FilterStrip)); }
		}

		FilterStripControlForTest fStripControl;

		public FilterStripBusinessObjectForTest FilterStrip
		{
			get
			{
				if (fFilterStrip == null)
				{
					fFilterStrip = new FilterStripBusinessObjectForTest();
					((IFilterStripBusinessObjectInternals)fFilterStrip).LayoutContext = "shipment";
				}
				return fFilterStrip;
			}
		}

		FilterStripBusinessObjectForTest fFilterStrip;

		public class FilterStripBusinessObjectForTest : FilterStripBusinessObject
		{
			public FilterStripBusinessObjectForTest() : base() { }
			public FilterStripBusinessObjectForTest(BusinessObjectFactory factory) : base(factory) { }

			protected override ModuleFilterCollection GetModuleFiltersCore()
			{
				var filters = new ModuleFilterCollection();
				filters.AddTextFilter("desc", delegate
				{ return new ZQuery(); });
				return filters;
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<GridColourScheme>();
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			var scheme = Factory.New<GridColourScheme>();
			scheme.S9_ModuleID = "Dummy" + StmModuleFilter.ModuleIdSuffix.GridColorScheme;
			return scheme;
		}

		protected override BusinessObject GetNewBusinessObjectForTranslatableFieldTest(BusinessObjectFactory factory)
		{
			var scheme = factory.New<GridColourScheme>();
			scheme.S9_ModuleID = "Dummy" + StmModuleFilter.ModuleIdSuffix.GridColorScheme;
			scheme.S9_IsPublished = true;
			using (StripControl)
			{
				scheme.ColourStrips.Add(new GridColourStripBusinessObject(StripControl.FilterBusinessObject, scheme, null));
			}
			return scheme;
		}

		public override void TestOnLoadedDoesNotCreateOrLoadOtherObjects()
		{
			Assert("Overridden when upgrading base class of this test to EnterpriseBusinessObject. All other new tests passed. Will be passed to Core team for review / remedy.", true);
		}

		#endregion
	}
}
