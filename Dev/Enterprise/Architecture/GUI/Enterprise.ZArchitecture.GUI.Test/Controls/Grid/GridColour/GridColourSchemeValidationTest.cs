using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Testing
{
	sealed class GridColourSchemeValidationTest : TestCaseWithFactory
	{
		public void TestValidateGridColourStrips()
		{
			var scheme = Factory.New<GridColourScheme>();
			scheme.S9_ModuleID = "MOD";
			scheme.S9_FilterName = "Name";

			using (StripControl)
			{
				var gridColorStrip1 = new GridColourStripBusinessObject(StripControl.FilterBusinessObject, scheme, null);
				gridColorStrip1.RuleName = "rule1";
				scheme.ColourStrips.Add(gridColorStrip1);

				var gridColorStrip2 = new GridColourStripBusinessObject(StripControl.FilterBusinessObject, scheme, null);
				gridColorStrip2.RuleName = "rule2";
				scheme.ColourStrips.Add(gridColorStrip2);

				scheme.Validation.ValidateAll();

				AssertNoErrors(scheme);

				var gridColorStrip3 = new GridColourStripBusinessObject(StripControl.FilterBusinessObject, scheme, null);
				gridColorStrip3.RuleName = "rule3";

				var errorText = "Error message";
				var filter = gridColorStrip3.AddTextFilterStrip("desc");
				filter.IsActive = true;
				filter.Property = string.Empty;
				filter.PropertyValidation = info =>
				{
					if (info.Value.IsEmpty)
					{
						info.AddError(errorText);
					}
				};

				filter.Validation.ValidateProperty();

				AssertHasError(filter.PropertyInfo, errorText);

				scheme.ColourStrips.Add(gridColorStrip3);
				scheme.Validation.ValidateAll();

				AssertHasRowError(scheme, "There are some errors in search rules, please correct these before saving.");
			}
		}

		public void TestCheckIsPublished()
		{
			var scheme1 = Factory.New<GridColourScheme>();
			var scheme2 = Factory.New<GridColourScheme>();

			scheme1.S9_ModuleID = "MOD";
			scheme1.S9_FilterName = "name";
			scheme1.S9_IsPublished = false;

			scheme2.S9_ModuleID = "MOD";
			scheme2.S9_FilterName = "name";
			scheme2.S9_IsPublished = true;

			scheme2.Validation.ValidateAll();
			AssertNoErrors(scheme2.S9_IsPublishedInfo);

			scheme1.S9_IsPublished = true;
			scheme1.Validation.ValidateAll();
			AssertHasErrors(scheme1.S9_IsPublishedInfo);

			scheme2.S9_ModuleID = "X";

			scheme1.Validation.ValidateAll();
			AssertNoErrors(scheme1.S9_IsPublishedInfo);
		}

		public void TestFilterName()
		{
			var scheme = Factory.New<GridColourScheme>();
			scheme.S9_FilterName = "";
			scheme.Validation.ValidateAll();
			AssertHasErrors(scheme.S9_FilterNameInfo);

			scheme.S9_FilterName = "name";
			scheme.S9_ModuleID = "M" + StmModuleFilter.ModuleIdSuffix.GridColorScheme;
			scheme.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
			Factory.Save();

			scheme.Validation.ValidateS9_FilterName();

			AssertHasErrors(scheme.S9_FilterNameInfo);

			using (StripControl)
			{
				scheme.ColourStrips.Add(new GridColourStripBusinessObject(StripControl.FilterBusinessObject, scheme, null));
				scheme.Validation.ValidateAll();

				AssertNoErrors(scheme.S9_FilterNameInfo);
				Factory.Save();

				var scheme2 = Factory.New<GridColourScheme>();

				scheme2.S9_FilterName = "name";
				scheme2.ColourStrips.Add(new GridColourStripBusinessObject(StripControl.FilterBusinessObject, scheme, null));
				scheme2.Validation.ValidateAll();

				AssertNoErrors(scheme2.S9_FilterNameInfo);

				scheme2.S9_ModuleID = "M" + StmModuleFilter.ModuleIdSuffix.GridColorScheme;

				scheme2.Validation.ValidateAll();
				AssertNoErrors(scheme.S9_FilterNameInfo);

				scheme2.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
				scheme2.Validation.ValidateAll();
				AssertHasErrors(scheme2.S9_FilterNameInfo);

				scheme.S9_IsPublished = true;
				scheme2.Validation.ValidateAll();
				AssertHasErrors(scheme2.S9_FilterNameInfo);

				scheme.S9_IsPublished = false;
				scheme.S9_ModuleID = "X" + StmModuleFilter.ModuleIdSuffix.GridColorScheme;
				Factory.Save();
				scheme2.Validation.ValidateAll();
				AssertNoErrors(scheme2.S9_FilterNameInfo);

				scheme.S9_IsSystem = true;
				scheme.S9_ModuleID = "M" + StmModuleFilter.ModuleIdSuffix.GridColorScheme;
				scheme2.Validation.ValidateAll();
				AssertHasErrors(scheme2.S9_FilterNameInfo);

				scheme.S9_IsPublished = false;
				scheme.S9_IsSystem = false;
				scheme.S9_RelatedEntityID = CargoWise.Types.ZGuid.NewZGuid();
				scheme2.Validation.ValidateAll();
				AssertNoErrors(scheme2.S9_FilterNameInfo);

				scheme2.S9_FilterName = "[art";
				scheme2.Validation.ValidateAll();
				AssertHasErrors(scheme2.S9_FilterNameInfo);

				scheme2.S9_FilterName = "art]";
				scheme2.Validation.ValidateAll();
				AssertHasErrors(scheme2.S9_FilterNameInfo);

				scheme2.S9_FilterName = "[art]";
				scheme2.Validation.ValidateAll();
				AssertHasErrors(scheme2.S9_FilterNameInfo);

				scheme2.S9_FilterName = "xyxyxyxy";
				scheme2.Validation.ValidateAll();
				AssertNoErrors(scheme2.S9_FilterNameInfo);

				scheme2.S9_FilterName = "xyxyxyxy*";
				scheme2.Validation.ValidateAll();
				AssertHasErrors(scheme2.S9_FilterNameInfo);
			}
		}

		public void TestCheckS9_FilterName_QueryPerformance()
		{
			var scheme = Factory.New<GridColourScheme>();

			using (Db.Connection.TrackExecutedCommands())
			{
				scheme.S9_FilterName = "Squanch";
				AssertCollectionContains("It's important to add the filter type clause so that the correct index is used. SAD!", Db.Connection.ExecutedCommands, x => x.Contains("S9_FilterType <> 'FRU'"));
			}
		}

		public void TestValidatePublishAcrossAllCompanies()
		{
			var scheme = Factory.New<GridColourScheme>();

			EnvProxy.Instance.Security.PublishGlobalGridColorSchemes.IsAllowedForAllBranches = false;

			scheme.PublishAcrossAllCompanies = false;
			AssertNoErrors(scheme.PublishAcrossAllCompaniesInfo);

			scheme.PublishAcrossAllCompanies = true;
			AssertHasError(scheme.PublishAcrossAllCompaniesInfo, EnvProxy.Instance.Security.PublishGlobalGridColorSchemes.ErrorMessageForNotAllowed);

			EnvProxy.Instance.Security.PublishGlobalGridColorSchemes.IsAllowedForAllBranches = true;

			scheme.PublishAcrossAllCompanies = false;
			AssertNoErrors(scheme.PublishAcrossAllCompaniesInfo);

			scheme.PublishAcrossAllCompanies = true;
			AssertNoErrors(scheme.PublishAcrossAllCompaniesInfo);
		}

		FilterStripControlForTest StripControl
		{
			get
			{
				if (fStripControl == null)
				{
					var bo = new ColorGridFactoryTest.FilterStripBusinessObjectForTest();
					fStripControl = new FilterStripControlForTest(null, bo);
				}
				return fStripControl;
			}
		}

		FilterStripControlForTest fStripControl;
	}
}
