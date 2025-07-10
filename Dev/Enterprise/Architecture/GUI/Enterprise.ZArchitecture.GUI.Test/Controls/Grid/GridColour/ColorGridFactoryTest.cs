using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Testing
{
	public class ColorGridFactoryTest : TestCaseWithFactory
	{
		public void TestColorSchemeShouldNotApplyIfIsCompanySpecific()
		{
			using (StripControl)
			{
				var scheme = Factory.New<GridColourScheme>();
				scheme.S9_FilterName = "xyz";
				scheme.S9_ModuleID = "shipment_CS";
				scheme.S9_IsPublished = true;
				scheme.PublishAcrossAllCompanies = false;
				var colorData = Factory.New<StmData>();
				colorData.SD_GuidValue = scheme.PK;
				var moduleFilter = Factory.New<StmModuleFilter>();
				moduleFilter.S9_FilterName = "Customized";
				moduleFilter.S9_SaveColumnLayout = true;
				moduleFilter.S9_GridColourLayoutID = scheme.PK;
				moduleFilter.S9_SaveGridColourLayout = true;
				moduleFilter.S9_IsPublished = true;
				moduleFilter.S9_GC = ZGuid.Empty;
				var staff = Factory.New<IGlbStaff>();
				staff.GS_Code = "ABC";
				var company = Factory.New<IGlbCompany>();
				var branch = Factory.New<IGlbBranch>();
				branch.GB_GC = company.PK;
				Factory.Save();

				ColorFactory.Control = StripControl;
				ColorFactory.Grid.CurrentColumnLayout = moduleFilter;
				AssertEquals("Should return scheme if moduleFilter saves customized with colour layout", scheme.PK, ColorFactory.GetLastUsedSchemeForCurrentUser(FilterStrip).PK);

				ColorFactory.ResetSchemes();
				using (EnvProxy.Instance.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					AssertEquals("Should return scheme if the creator of the layout is current user.", scheme.PK, ColorFactory.GetLastUsedSchemeForCurrentUser(FilterStrip).PK);
				}

				ColorFactory.ResetSchemes();
				using (EnvProxy.Instance.SetTemporaryUserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					AssertEquals("Should return null if the color scheme is company specific.", null, ColorFactory.GetLastUsedSchemeForCurrentUser(FilterStrip));
				}
			}
		}

		#region GetLastUsedSchemeForCurrentUser Test

		public void TestGetLastUsedSchemForCurrentUserNewGridColour()
		{
			var zGuid = ZGuid.Empty;

			using (var form = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				var grid = new ZGrid();
				grid.BindTo = "Collection";
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Text", 80));
				form.Controls.Add(grid);
				form.Show();
				Application.DoEvents();

				var colourFacotry = new GridColourFactory(grid);
				var filterStrip = new GridFilterStripBusinessObject(grid);
				((IFilterStripBusinessObjectInternals)filterStrip).LayoutContext = "shipment";

				AssertNull(colourFacotry.GetLastUsedSchemeForCurrentUser(filterStrip));
				AssertEquals("should be false", false, grid.RowColorsAreDataViewOptimisable);

				var scheme = Factory.New<GridColourScheme>();
				scheme.S9_FilterName = "xyz";
				scheme.S9_ModuleID = "shipment_CS";
				var colorData = Factory.New<StmData>();
				colorData.SD_GuidValue = scheme.PK;

				Factory.Save();

				colourFacotry.SetLastUsedSchemeForCurrentUser(filterStrip, scheme);

				AssertEquals("Should return scheme if moduleFilter saves customized with colour layout", scheme.PK, colourFacotry.GetLastUsedSchemeForCurrentUser(filterStrip).PK);
				AssertEquals("should be true", true, grid.RowColorsAreDataViewOptimisable);

				zGuid = scheme.PK;
			}
			using (var form = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				var grid = new ZGrid();
				grid.BindTo = "Collection";
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Text", 80));
				form.Controls.Add(grid);
				form.Show();
				Application.DoEvents();

				var colourFacotry = new GridColourFactory(grid);
				var filterStrip = new GridFilterStripBusinessObject(grid);
				((IFilterStripBusinessObjectInternals)filterStrip).LayoutContext = "shipment";

				AssertEquals("Should return scheme if moduleFilter saves customized with colour layout even the form is reloaded", zGuid, colourFacotry.GetLastUsedSchemeForCurrentUser(filterStrip).PK);
				AssertEquals("should be true", true, grid.RowColorsAreDataViewOptimisable);
			}
		}

		public void TestGetLastUsedSchemeForCurrentUserStandardColourScheme()
		{
			using (StripControl)
			{
				var moduleFilter1 = Factory.New<StmModuleFilter>();
				moduleFilter1.S9_FilterName = "Standard";
				moduleFilter1.S9_ModuleID = "shipment_CS";
				moduleFilter1.S9_SaveColumnLayout = true;
				moduleFilter1.S9_SaveGridColourLayout = true;
				moduleFilter1.S9_GridColourLayoutID = ZGuid.Empty;

				var scheme = Factory.New<GridColourScheme>();
				scheme.S9_FilterName = "xyz";
				scheme.S9_ModuleID = "shipment_CS";

				var colorData = Factory.New<StmData>();
				colorData.SD_GuidValue = scheme.PK;

				var moduleFilter2 = Factory.New<StmModuleFilter>();
				moduleFilter2.S9_FilterName = "Customized";
				moduleFilter2.S9_SaveColumnLayout = true;
				moduleFilter2.S9_GridColourLayoutID = scheme.PK;
				moduleFilter2.S9_SaveGridColourLayout = true;
				Factory.Save();

				ColorFactory.Control = StripControl;

				ColorFactory.Grid.CurrentColumnLayout = moduleFilter1;
				AssertEquals("Should return null if moduleFilter saves standard with empty colour layout", null, ColorFactory.GetLastUsedSchemeForCurrentUser(FilterStrip));

				ColorFactory.Grid.CurrentColumnLayout = moduleFilter2;
				AssertEquals("Should return scheme if moduleFilter saves customized with colour layout", scheme.PK, ColorFactory.GetLastUsedSchemeForCurrentUser(FilterStrip).PK);

				ColorFactory.Grid.CurrentColumnLayout = moduleFilter1;
				AssertEquals("Should return null if moduleFilter saves standard with empty colour layout", null, ColorFactory.GetLastUsedSchemeForCurrentUser(FilterStrip));
			}
		}

		public void TestGetLastUsedSchemeForCurrentUserWhenFilterIsDeleted()
		{
			using (StripControl)
			{
				var scheme = Factory.New<GridColourScheme>();
				scheme.S9_FilterName = "test";
				scheme.S9_ModuleID = "shipment_CS";

				var colorData = Factory.New<StmData>();
				colorData.SD_GuidValue = scheme.PK;

				var moduleFilter = Factory.New<StmModuleFilter>();
				moduleFilter.S9_FilterName = "Customized";
				moduleFilter.S9_GridColourLayoutID = scheme.PK;
				moduleFilter.S9_SaveGridColourLayout = true;
				Factory.Save();

				ColorFactory.Control = StripControl;

				ColorFactory.Grid.CurrentColumnLayout = moduleFilter;
				moduleFilter.Delete();
				AssertEquals("Should return last used scheme without errors", scheme.PK, ColorFactory.GetLastUsedSchemeForCurrentUser(FilterStrip).PK);
			}
		}

		public void TestGetLastUsedSchemeForCurrentUserNoLastUsedGetFirstScheme()
		{
			//no filter or scheme created yet, it should return null
			AssertNull(ColorFactory.GetLastUsedSchemeForCurrentUser(FilterStrip));

			//some scheme exists, it should return it even thou it's not for the active filter.
			var scheme = Factory.New<GridColourScheme>();
			var bo = new FilterStripBusinessObjectForTest();

			using (StripControl)
			{
				var colorStrip = new GridColourStripBusinessObject(StripControl.FilterBusinessObject, scheme, null);
				scheme.ColourStrips.Add(colorStrip);
				scheme.S9_ModuleID = "shipment_CS";
				Factory.Save();
				ColorFactory.Control = StripControl;
				ColorFactory.ResetSchemes();
				AssertEquals("should return the first existing scheme if no last used scheme is found for active strip", scheme.PK, ColorFactory.GetLastUsedSchemeForCurrentUser(FilterStrip).PK);
			}
		}

		public void TestGetLastUsedSchemeForCurrentUserLogicCalledOnlyOnce()
		{
			var scheme = Factory.New<GridColourScheme>();
			scheme.S9_ModuleID = "_CS";
			Factory.Save();
			using (var form = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				var grid = new ZGrid();
				grid.BindTo = "Collection";
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Text", 80));
				form.Controls.Add(grid);
				form.Show();
				Application.DoEvents();
				var oldHits = grid.GridColourSchemeManagerForTest.FilterBusinessObject.Factory.TableSelects
					.FirstOrDefault(ts => ts.TableName == StmModuleFilterSchema.Constants.TableName)
					.Value;
				grid.GridColourSchemeManagerForTest.FilterBusinessObject.Factory.ClearQueryCache();

				var gridColourMenu = grid.ContextMenu.MenuItems.FindByText("Grid Colors", true);
				if (gridColourMenu != null)
				{
					gridColourMenu.PerformSelect();
				}

				var expectedDbHits = new Dictionary<string, int>();
				expectedDbHits.Add(StmDataSchema.Constants.TableName, 1);
				expectedDbHits.Add(StmModuleFilterSchema.Constants.TableName, 1 + oldHits);
				AssertDbHits(expectedDbHits, grid.GridColourSchemeManagerForTest.FilterBusinessObject.Factory);
			}
		}

		public void TestGetLastUsedSchemeForCurrentUserInMultiThread()
		{
			var pauseEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
			Exception threadsException = null;

			var grid = new ZGrid();
			var gridColourFactory = new GridColourFactory(grid);
			var bo = new FilterStripBusinessObjectForTest();
			GridColourStripBusinessObject colorStrip = new GridColourStripBusinessObjectForMultiThreadTest(bo, null, null);

			try
			{
				using (grid)
				{
					var thread1 = new Thread(new ThreadStart(delegate
					{
						try
						{
							using (Db.DisposableActionForDbConnection())
							{
								pauseEvent.WaitOne();

								gridColourFactory.GetLastUsedSchemeForCurrentUser(colorStrip);
							}
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							threadsException = ex;
						}
					}
					));
					thread1.SetApartmentState(ApartmentState.STA);
					thread1.Start();

					var thread2 = new Thread(new ThreadStart(delegate
					{
						try
						{
							using (Db.DisposableActionForDbConnection())
							{
								pauseEvent.WaitOne();

								gridColourFactory.GetLastUsedSchemeForCurrentUser(colorStrip);
							}
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							threadsException = ex;
						}
					}
					));
					thread2.Start();

					pauseEvent.Set();
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				threadsException = ex;
			}

			Thread.Sleep(TimeSpan.FromSeconds(1));
			AssertNull("The exception should not occurs when two threads trying to update cache at the same time", threadsException);
			ErrorReporter.Instance.Clear();
		}

		public void TestGetLastUsedSchemeForCurrentUserLastUsedInStmData()
		{
			//no filter or scheme created yet, it should return null
			AssertNull(ColorFactory.GetLastUsedSchemeForCurrentUser(FilterStrip));

			//last used scheme exists for active filter strip, return it.
			var scheme2 = Factory.New<GridColourScheme>();
			scheme2.S9_FilterName = "xx";
			scheme2.S9_ModuleID = "shipment_CS";

			using (StripControl)
			{
				var colorStrip = new GridColourStripBusinessObject(StripControl.FilterBusinessObject, scheme2, null);

				scheme2.ColourStrips.Add(colorStrip);

				var data = Factory.New<StmData>();
				data.SD_GuidValue = scheme2.PK;
				data.SD_Name = scheme2.S9_ModuleID;
				data.SD_Type = GridColourFactory.ColorSchemeCode;
				data.SD_Owner = EnvProxy.Instance.CurrentUser.PK;
				data.SD_DepartmentGuid = EnvProxy.Instance.CurrentCompany.PK;

				Factory.Save();
				ColorFactory.Control = StripControl;
				ColorFactory.ResetSchemes();
				AssertEquals("should return the matching scheme", scheme2.PK, ColorFactory.GetLastUsedSchemeForCurrentUser(FilterStrip).PK);
			}
		}

		public void TestGetGridLayoutColourScheme()
		{
			var factory = FilterStrip.Factory;
			var scheme1 = factory.New<GridColourScheme>();
			scheme1.S9_FilterName = "xyz";
			scheme1.S9_ModuleID = "shipment_CS";

			var scheme2 = factory.New<GridColourScheme>();
			scheme2.S9_FilterName = "abcd";
			scheme2.S9_ModuleID = "shipment_CS";

			using (StripControl)
			{
				StripControl.FilteredGrid.ColorContextKey = "abcde";
				StripControl.FilteredGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
				StripControl.FilteredGrid.SetDataBinding(new DummyBusinessObjectCollection(factory), "");

				var colorStrip1 = new GridColourStripBusinessObject(StripControl.FilterBusinessObject, scheme1, null);
				scheme1.ColourStrips.Add(colorStrip1);

				var colorStrip2 = new GridColourStripBusinessObject(StripControl.FilterBusinessObject, scheme2, null);
				scheme1.ColourStrips.Add(colorStrip2);

				ColorFactory.Control = StripControl;
				ColorFactory.GetAllSchemesForActiveFilter(FilterStrip);
				ColorFactory.SetLastUsedSchemeForCurrentUser(FilterStrip, scheme1);
				AssertEquals("No grid layout: Should return the last saved colour scheme", scheme1.PK, ColorFactory.GetLastUsedSchemeForCurrentUser(FilterStrip).PK);

				var preConfiguredLayout = factory.New<StmModuleFilter>();
				preConfiguredLayout.S9_SaveGridColourLayout = true;
				preConfiguredLayout.S9_GridColourLayoutID = scheme2.PK;
				factory.Save();

				ColorFactory.Grid.CurrentColumnLayout = preConfiguredLayout;

				factory.Save();
				AssertEquals("With grid layout: Should return the colour scheme saved in the grid layout", scheme2.PK, ColorFactory.GetLastUsedSchemeForCurrentUser(FilterStrip).PK);
			}
		}

		public void TestGetGridLayoutColorSchemeWhenGridCurrentColumnLayoutIsNull()
		{
			var scheme1 = Factory.New<GridColourScheme>();
			scheme1.S9_FilterName = "xyz";
			scheme1.S9_ModuleID = "shipment_CS";

			var scheme2 = Factory.New<GridColourScheme>();
			scheme2.S9_FilterName = "abcd";
			scheme2.S9_ModuleID = "shipment_CS";

			using (StripControl)
			using (var module = ZModuleFactory.Instance.CreateNew(ModuleIDs.JobShipment) as ZFilterModule)
			{
				FilterStrip.ParentModule = module;

				var savedLayout1 = Factory.New<StmModuleFilter>();
				savedLayout1.S9_SaveGridColourLayout = true;
				savedLayout1.S9_FilterName = "layout 1";
				savedLayout1.S9_GridColourLayoutID = scheme1.PK;

				var savedLayout2 = Factory.New<StmModuleFilter>();
				savedLayout2.S9_SaveGridColourLayout = true;
				savedLayout1.S9_FilterName = "layout 2";
				savedLayout2.S9_GridColourLayoutID = scheme2.PK;
				Factory.Save();

				ColorFactory.Control = StripControl;
				FilterStrip.LastUsedLayout = savedLayout1;
				ColorFactory.Grid.CurrentColumnLayout = null;
				AssertEquals("Should return the color scheme 1 saved in the grid layout1", scheme1.PK, ColorFactory.GetLastUsedSchemeForCurrentUser(FilterStrip).PK);

				FilterStrip.LastUsedLayout = savedLayout2;
				ColorFactory.Grid.CurrentColumnLayout = null;
				AssertEquals("Should return the color scheme 2 saved in the grid layout2", scheme2.PK, ColorFactory.GetLastUsedSchemeForCurrentUser(FilterStrip).PK);
			}
		}

		public void TestGetFBOLayoutColourScheme()
		{
			var scheme1 = Factory.New<GridColourScheme>();
			scheme1.S9_FilterName = "xyz";
			scheme1.S9_ModuleID = "shipment_CS";

			var scheme2 = Factory.New<GridColourScheme>();
			scheme2.S9_FilterName = "abcd";
			scheme2.S9_ModuleID = "shipment_CS";

			using (StripControl)
			using (var module = ZModuleFactory.Instance.CreateNew(ModuleIDs.JobShipment) as ZFilterModule)
			{
				FilterStrip.ParentModule = module;
				var preConfiguredLayout = Factory.New<StmModuleFilter>();
				preConfiguredLayout.S9_SaveGridColourLayout = true;
				FilterStrip.LastUsedLayout = preConfiguredLayout;

				var colorStrip1 = new GridColourStripBusinessObject(StripControl.FilterBusinessObject, scheme1, null);
				scheme1.ColourStrips.Add(colorStrip1);

				var colorStrip2 = new GridColourStripBusinessObject(StripControl.FilterBusinessObject, scheme2, null);
				scheme1.ColourStrips.Add(colorStrip2);

				((ZFilterModule)FilterStrip.ParentModule).DoNotCheckOrSaveChanges = false;

				ColorFactory.Grid.CurrentColumnLayout = StmDataGridLayoutStorage.New(Factory.NewWithValidTestData<StmData>());
				preConfiguredLayout.S9_GridColourLayoutID = scheme2.PK;
				Factory.Save();
				AssertEquals("Should return the colour scheme saved in the filter business object", scheme2.PK, ColorFactory.GetLastUsedSchemeForCurrentUser(FilterStrip).PK);

				ColorFactory.Grid.CurrentColumnLayout = StmDataGridLayoutStorage.New(Factory.NewWithValidTestData<StmData>());
				preConfiguredLayout.S9_GridColourLayoutID = scheme1.PK;
				Factory.Save();
				AssertEquals("Should return the colour scheme saved in the filter business object", scheme1.PK, ColorFactory.GetLastUsedSchemeForCurrentUser(FilterStrip).PK);

				((ZFilterModule)FilterStrip.ParentModule).DoNotCheckOrSaveChanges = true;
				ColorFactory.Grid.CurrentColumnLayout = StmDataGridLayoutStorage.New(Factory.NewWithValidTestData<StmData>());
				preConfiguredLayout.S9_GridColourLayoutID = scheme2.PK;
				Factory.Save();
				AssertNotEquals("if the FBO is 'fake', we ignore it", scheme2.PK, ColorFactory.GetLastUsedSchemeForCurrentUser(FilterStrip).PK);
			}
		}

		#endregion

		#region GetAllSchemesForActiveFilter Test

		public void TestGetAllSchemesForActiveFilterLoadAdditionalType()
		{
			// Setup GridColourStrip for Additional Business Object
			// Use StripControl with IAdditionalTypeGridColourSupport

			// Assert that it will load Additional Business Object 
			var scheme = Factory.New<GridColourScheme>();
			scheme.S9_FilterName = "xyz";
			scheme.S9_ModuleID = "shipment_CS";

			using (StripControl)
			{
				var colorStrip = new GridColourStripBusinessObject(StripControl.FilterBusinessObject, scheme, null);
				scheme.ColourStrips.Add(colorStrip);

				var scheme2 = Factory.New<GridColourScheme>();
				scheme2.S9_FilterName = "xx";
				scheme2.S9_ModuleID = "shipment_CS";
				colorStrip = new GridColourStripBusinessObject(StripControl.FilterBusinessObject, scheme, null);
				scheme2.ColourStrips.Add(colorStrip);

				var scheme3 = Factory.New<GridColourScheme>();
				scheme3.S9_FilterName = "xtz";
				scheme3.S9_ModuleID = "shipnt_CS";
				colorStrip = new GridColourStripBusinessObject(StripControl.FilterBusinessObject, scheme, null);
				scheme3.ColourStrips.Add(colorStrip);

				var scheme4 = Factory.New<GridColourScheme>();
				scheme4.S9_FilterName = "sickfly";
				scheme4.S9_ModuleID = "shipment_CS";
				scheme4.S9_IsSystem = true;
				colorStrip = new GridColourStripBusinessObject(StripControl.FilterBusinessObject, scheme, null);
				scheme4.ColourStrips.Add(colorStrip);

				Factory.Save();
				ColorFactory.Control = StripControl;
				AssertEquals("should return all schemes for active strip", 3, ColorFactory.GetAllSchemesForActiveFilter(FilterStrip).Length);

				fFilterStrip.addAdditionalFilterForTesting = new Action<ZQuery, SchemaColumn>((ZQuery query, SchemaColumn column) =>
				{
					query.AddToFilter(JoinCondition.Or, column, SQLComparisonOperator.Equal, "shipnt_CS");
				});

				AssertEquals("should return all schemes for active strip", 4, ColorFactory.GetAllSchemesForActiveFilter(FilterStrip).Length);
			}
		}

		public void TestGetAllSchemesForActiveFilterQuery_NoNotIn()
		{
			using (StripControl)
			{
				ColorFactory.Control = StripControl;
				Assert(!ColorFactory.GetAllSchemesForActiveFilterQuery(FilterStrip).LiteralTextSqlFormatted.Contains("NOT IN"));
			}
		}

		public void TestGetAllSchemesForActiveFilter()
		{
			var scheme = Factory.New<GridColourScheme>();
			scheme.S9_FilterName = "xyz";
			scheme.S9_ModuleID = "shipment_CS";

			using (StripControl)
			{
				var colorStrip = new GridColourStripBusinessObject(StripControl.FilterBusinessObject, scheme, null);
				scheme.ColourStrips.Add(colorStrip);

				var scheme2 = Factory.New<GridColourScheme>();
				scheme2.S9_FilterName = "xx";
				scheme2.S9_ModuleID = "shipment_CS";
				colorStrip = new GridColourStripBusinessObject(StripControl.FilterBusinessObject, scheme, null);
				scheme2.ColourStrips.Add(colorStrip);

				var scheme3 = Factory.New<GridColourScheme>();
				scheme3.S9_FilterName = "xtz";
				scheme3.S9_ModuleID = "shipnt_CS";
				colorStrip = new GridColourStripBusinessObject(StripControl.FilterBusinessObject, scheme, null);
				scheme3.ColourStrips.Add(colorStrip);

				var scheme4 = Factory.New<GridColourScheme>();
				scheme4.S9_FilterName = "sickfly";
				scheme4.S9_ModuleID = "shipment_CS";
				scheme4.S9_IsSystem = true;
				colorStrip = new GridColourStripBusinessObject(StripControl.FilterBusinessObject, scheme, null);
				scheme4.ColourStrips.Add(colorStrip);

				Factory.Save();
				ColorFactory.Control = StripControl;
				AssertEquals("should return all schemes for active strip", 3, ColorFactory.GetAllSchemesForActiveFilter(FilterStrip).Length);
			}
		}

		public void TestGetAllSchemesForActiveFilterSorted()
		{
			using (StripControl)
			{
				const string firstSortedSchemeName = "TestSchemeA";
				const string lastSortedSchemeName = "TestSchemeE";

				var scheme = Factory.New<GridColourScheme>();
				scheme.S9_FilterName = "TestSchemeB";
				scheme.S9_ModuleID = "shipment_CS";

				var colorStrip = new GridColourStripBusinessObject(StripControl.FilterBusinessObject, scheme, null);
				scheme.ColourStrips.Add(new GridColourStripBusinessObject(StripControl.FilterBusinessObject, scheme, null));

				var scheme2 = Factory.New<GridColourScheme>();
				scheme2.S9_FilterName = lastSortedSchemeName;
				scheme2.S9_ModuleID = "shipment_CS";
				scheme2.ColourStrips.Add(colorStrip);

				var scheme3 = Factory.New<GridColourScheme>();
				scheme3.S9_FilterName = firstSortedSchemeName;
				scheme3.S9_ModuleID = "shipment_CS";
				scheme3.ColourStrips.Add(colorStrip);

				var scheme4 = Factory.New<GridColourScheme>();
				scheme4.S9_FilterName = "TestSchemeD";
				scheme4.S9_ModuleID = "shipment_CS";
				scheme4.ColourStrips.Add(colorStrip);

				var scheme5 = Factory.New<GridColourScheme>();
				scheme5.S9_FilterName = "TestSchemeC";
				scheme5.S9_ModuleID = "shipment_CS";
				scheme5.ColourStrips.Add(colorStrip);

				Factory.Save();
				ColorFactory.Control = StripControl;
				var allSchemesSorted = ColorFactory.GetAllSchemesForActiveFilter(FilterStrip);

				AssertEquals("Should return all schemes.", 5, allSchemesSorted.Length);
				AssertEquals("Should return the first scheme name.", firstSortedSchemeName, allSchemesSorted[0].S9_FilterName);
				AssertEquals("Should return the last scheme name.", lastSortedSchemeName, allSchemesSorted[4].S9_FilterName);
			}
		}

		#endregion

		#region SetLastUsedSchemeForCurrentUser Test

		public void TestSetLastUsedSchemeForCurrentUserUsingSchemeName()
		{
			var scheme = Factory.New<GridColourScheme>();
			scheme.S9_FilterName = "xyz";
			scheme.S9_ModuleID = "shipment_CS";
			using (StripControl)
			{
				var colorStrip = new GridColourStripBusinessObject(StripControl.FilterBusinessObject, scheme, null);
				scheme.ColourStrips.Add(colorStrip);
				Factory.Save();
				//need to call get all schemes here to populate the schemes collection in factory.
				ColorFactory.Control = StripControl;
				ColorFactory.GetAllSchemesForActiveFilter(FilterStrip);
				ColorFactory.SetLastUsedSchemeForCurrentUser(FilterStrip, scheme);
				AssertEquals(scheme.PK, ColorFactory.GetLastUsedSchemeForCurrentUser(FilterStrip).PK);

				var stmQuery = ColorFactory.GetLastUsedSchemeQuery(FilterStrip);
				var stmData = Factory.LoadTop1<StmData>(stmQuery);

				AssertNotNull("There should have StmData inserted.", stmData);

				var logQuery = new ZQuery();
				logQuery.AddToFilter(StmALogSchema.SL_Reference, "null -> " + scheme.DisplayName);
				var log = Factory.LoadTop1<StmALog>(logQuery);
				AssertNotNull("There should have StmALog inserted", log);
				AssertEquals("The SL_Table for the log should be StmData", "StmData", log.SL_Table);
				AssertEquals("The SL_Event for the log should be EDT", "EDT", log.SL_SE_NKEvent);
				AssertEquals("The SL_Parent for the log should be the StmData PK", stmData.PK, log.SL_Parent);

				var updateScheme = Factory.New<GridColourScheme>();
				updateScheme.S9_FilterName = "xyz update";
				updateScheme.S9_ModuleID = "shipment_CS";
				updateScheme.ColourStrips.Add(colorStrip);
				Factory.Save();

				ColorFactory.SetLastUsedSchemeForCurrentUser(FilterStrip, updateScheme);
				logQuery = new ZQuery();
				logQuery.AddToFilter(StmALogSchema.SL_Reference, "xyz -> xyz update");
				log = Factory.LoadTop1<StmALog>(logQuery);
				AssertNotNull("There should have new StmALog inserted", log);

				ColorFactory.SetLastUsedSchemeForCurrentUser(FilterStrip, null);
				logQuery = new ZQuery();
				logQuery.AddToFilter(StmALogSchema.SL_Reference, "xyz update -> null");
				log = Factory.LoadTop1<StmALog>(logQuery);
				AssertNotNull("There should have new StmALog inserted", log);
			}
		}

		public void TestSetLastUsedSchemeForCurrentUserUsingGridColorContextKey()
		{
			var scheme = Factory.New<GridColourScheme>();
			scheme.S9_FilterName = "xyz";
			scheme.S9_ModuleID = "shipment_CS";
			using (StripControl)
			{
				StripControl.FilteredGrid.ColorContextKey = "abcde";
				StripControl.FilteredGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
				StripControl.FilteredGrid.SetDataBinding(new DummyBusinessObjectCollection(Factory), "");

				var colorStrip = new GridColourStripBusinessObject(StripControl.FilterBusinessObject, scheme, null);
				scheme.ColourStrips.Add(colorStrip);
				Factory.Save();
				//need to call get all schemes here to populate the schemes collection in factory.
				ColorFactory.Control = StripControl;
				ColorFactory.GetAllSchemesForActiveFilter(FilterStrip);
				ColorFactory.SetLastUsedSchemeForCurrentUser(FilterStrip, scheme);
				AssertEquals(scheme.PK, ColorFactory.GetLastUsedSchemeForCurrentUser(FilterStrip).PK);

				var data = Factory.LoadTop1<StmData>(new ZQuery(StmDataSchema.SD_Name, GridColourFactory.GetGridColorContext(StripControl.FilteredGrid, false)));
				AssertNotNull("Last used schema should have been saved using grid's color context key", data);
				AssertEquals(scheme.PK, data.SD_GuidValue);

				data = Factory.LoadTop1<StmData>(new ZQuery(StmDataSchema.SD_Name, GridColourFactory.GetLayoutContext(FilterStrip)));
				AssertNull("Last used schema should have been saved using grid's color context key", data);
			}
		}

		public void TestSetLastUsedSchemeForCurrentUserUsingActiveStrip()
		{
			var scheme = Factory.New<GridColourScheme>();
			scheme.S9_FilterName = "xyz";
			scheme.S9_ModuleID = "shipment_CS";
			using (StripControlWithNullCollection)
			{
				var colorStrip = new GridColourStripBusinessObject(StripControlWithNullCollection.FilterBusinessObject, scheme, null);
				scheme.ColourStrips.Add(colorStrip);
				Factory.Save();

				var colorFactoryLocal = new GridColourFactory(StripControlWithNullCollection.FilteredGrid);

				colorFactoryLocal.SetLastUsedSchemeForCurrentUser(FilterStrip, scheme);
				AssertEquals(scheme.PK, colorFactoryLocal.GetLastUsedSchemeForCurrentUser(FilterStrip).PK);

				var data = Factory.LoadTop1<StmData>(new ZQuery(StmDataSchema.SD_Name, GridColourFactory.GetGridColorContext(StripControlWithNullCollection.FilteredGrid, false)));
				AssertNull("Last used schema should have been saved using filter strip layout context", data);

				data = Factory.LoadTop1<StmData>(new ZQuery(StmDataSchema.SD_Name, GridColourFactory.GetLayoutContext(FilterStrip)));
				AssertNotNull("Last used schema should have been saved using filter strip layout context", data);
				AssertEquals(scheme.PK, data.SD_GuidValue);
			}
		}

		#endregion

		#region GetGridColorContext

		public void TestGetGridColorContext()
		{
			using (StripControlWithNullCollection)
			{
				StripControlWithNullCollection.FilteredGrid.GridId = "123-456";

				Assert(string.IsNullOrEmpty(GridColourFactory.GetGridColorContext(StripControlWithNullCollection.FilteredGrid, false)));
				Assert(string.IsNullOrEmpty(GridColourFactory.GetGridColorContext(StripControlWithNullCollection.FilteredGrid, true)));

				StripControlWithNullCollection.FilteredGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
				StripControlWithNullCollection.FilteredGrid.SetDataBinding(new DummyBusinessObjectCollection(Factory), "");

				AssertEquals("CargoWise.EntityFramework.Testing.DummyBusinessObject|_CS|123-456", GridColourFactory.GetGridColorContext(StripControlWithNullCollection.FilteredGrid, false));
				AssertEquals("CargoWise.EntityFramework.Testing.DummyBusinessObject|_CS", GridColourFactory.GetGridColorContext(StripControlWithNullCollection.FilteredGrid, true));

				StripControlWithNullCollection.FilteredGrid.ColorContextKey = "abcde";

				AssertEquals("CargoWise.EntityFramework.Testing.DummyBusinessObject|abcde_CS|123-456", GridColourFactory.GetGridColorContext(StripControlWithNullCollection.FilteredGrid, false));
				AssertEquals("CargoWise.EntityFramework.Testing.DummyBusinessObject|abcde_CS", GridColourFactory.GetGridColorContext(StripControlWithNullCollection.FilteredGrid, true));
			}
		}

		#region Uses parent type

		public void TestGetGridColorUsesParentsGridType()
		{
			using (StripControlWithNullCollection)
			{
				StripControlWithNullCollection.FilteredGrid.GridId = "123-456";

				Assert(string.IsNullOrEmpty(GridColourFactory.GetGridColorContext(StripControlWithNullCollection.FilteredGrid, false)));
				Assert(string.IsNullOrEmpty(GridColourFactory.GetGridColorContext(StripControlWithNullCollection.FilteredGrid, true)));

				StripControlWithNullCollection.FilteredGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
				StripControlWithNullCollection.FilteredGrid.SetDataBinding(new DummyBusinessObjectCollectionWithParent(Factory), "");

				AssertEquals("Enterprise.ZArchitecture.GridColourFactory|_CS|123-456", GridColourFactory.GetGridColorContext(StripControlWithNullCollection.FilteredGrid, false));
				AssertEquals("Enterprise.ZArchitecture.GridColourFactory|_CS", GridColourFactory.GetGridColorContext(StripControlWithNullCollection.FilteredGrid, true));

				StripControlWithNullCollection.FilteredGrid.ColorContextKey = "abcde";

				AssertEquals("Enterprise.ZArchitecture.GridColourFactory|abcde_CS|123-456", GridColourFactory.GetGridColorContext(StripControlWithNullCollection.FilteredGrid, false));
				AssertEquals("Enterprise.ZArchitecture.GridColourFactory|abcde_CS", GridColourFactory.GetGridColorContext(StripControlWithNullCollection.FilteredGrid, true));
			}
		}

		class DummyBusinessObjectCollectionWithParent : DummyBusinessObjectCollection, IUseParentGridContext
		{
			public DummyBusinessObjectCollectionWithParent(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public Type ParentType
			{
				get { return typeof(GridColourFactory); }
			}
		}

		#endregion

		public void TestSetLastUsedSchemeForCurrentUserResetsGridColoursByRowCache()
		{
			var scheme = Factory.New<GridColourScheme>();
			scheme.S9_FilterName = "xyz";
			scheme.S9_ModuleID = "shipment_CS";
			using (StripControl)
			{
				var colorStrip = new GridColourStripBusinessObject(StripControl.FilterBusinessObject, scheme, null);
				scheme.ColourStrips.Add(colorStrip);
				Factory.Save();

				var colors1 = StripControl.FilteredGrid.ColorByPK;
				var colors2 = StripControl.FilteredGrid.ColorByPK;
				Assert("Should be same array", object.ReferenceEquals(colors1, colors2));

				ColorFactory.SetLastUsedSchemeForCurrentUser(FilterStrip, scheme);

				colors2 = StripControl.FilteredGrid.ColorByPK;
				Assert("Should be different arrays", !object.ReferenceEquals(colors1, colors2));
			}
		}

		#endregion

		#region Create Standalone Factory Test

		public void TestUseDataSourceFactory()
		{
			var factory = new BusinessObjectFactory();

			using (StripControl)
			{
				StripControl.FilteredGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
				StripControl.FilteredGrid.SetDataBinding(new DummyBusinessObjectCollection(factory), "");
				ColorFactory.Control = StripControl;
				AssertEquals("Factory should be the same as the one of datasource", ColorFactory.Factory, factory);
			}
		}

		public void TestCreateStanaloneFactory()
		{
			var factory = new BusinessObjectFactory();

			using (StripControl)
			{
				StripControl.FilteredGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
				StripControl.FilteredGrid.SetDataBinding(new DummyBusinessObjectCollection(factory), "");
				StripControl.FilteredGrid.IsColourGridFactoryStandAlone = true;
				ColorFactory.Control = StripControl;
				AssertNotEquals("Factory should be different than the one of datasource", ColorFactory.Factory, factory);
			}
		}

		#endregion

		#region TestControlType

		public void TestControlType()
		{
			using (StripControl)
			{
				ColorFactory.Control = StripControl;
				AssertEquals(true, ColorFactory.Control is ZFilterStripCommonControl);
			}
		}

		#endregion

		#region Query Performance

		public void TestGetAllSchemesForActiveFilter_ShouldIncludeS9_FilterTypeInQuery()
		{
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessTasks))
			{
				var factory = new GridColourFactory(module.DisplayGrid);

				using (Db.Connection.TrackExecutedCommands())
				{
					factory.GetAllSchemesForActiveFilter(module.FilterBusinessObject);
					var matchingQueries = Db.Connection.ExecutedCommands.Where(x => x.Contains(ModuleIDs.ProcessTasks.Name)).ToArray();
					AssertEquals(1, matchingQueries.Length);
					AssertContains("It's important to add the filter type clause so that the correct index is used. SAD!", "S9_FilterType <> 'FRU'", matchingQueries.Single());
				}
			}
		}

		#endregion

		#region Implementation

		FilterStripControlForTest StripControl
		{
			get
			{
				if (fStripControl == null)
				{
					var bo = new FilterStripBusinessObjectForTest();
					var collection = new DummyBusinessObjectCollection(Factory);
					fStripControl = new FilterStripControlForTest(collection, bo);
				}
				return fStripControl;
			}
		}
		FilterStripControlForTest fStripControl;

		FilterStripControlForTest StripControlWithNullCollection
		{
			get
			{
				if (fStripControlWithNullCollection == null)
				{
					var bo = new FilterStripBusinessObjectForTest();
					fStripControlWithNullCollection = new FilterStripControlForTest(null, bo);
				}
				return fStripControlWithNullCollection;
			}
		}
		FilterStripControlForTest fStripControlWithNullCollection;

		GridColourFactory ColorFactory
		{
			get { return colorFactory ?? (colorFactory = new GridColourFactory(StripControl.FilteredGrid)); }
		}
		GridColourFactory colorFactory;

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

		public class FilterStripBusinessObjectForTest : FilterStripBusinessObject, IGridColourAdditionalModuleIdFilterSupporter
		{
			public void AddAdditionalFilter(ZQuery moduleIdQuery, SchemaColumn schemaColumn)
			{
				if (addAdditionalFilterForTesting != null)
				{
					addAdditionalFilterForTesting(moduleIdQuery, schemaColumn);
				}
			}

			public Action<ZQuery, SchemaColumn> addAdditionalFilterForTesting;

			protected override ModuleFilterCollection GetModuleFiltersCore()
			{
				var filters = new ModuleFilterCollection();
				filters.AddTextFilter("desc", delegate
				{ return new ZQuery(); });
				return filters;
			}
		}

		public class GridColourStripBusinessObjectForMultiThreadTest : GridColourStripBusinessObject
		{
			public GridColourStripBusinessObjectForMultiThreadTest(FilterStripBusinessObject parentFilterStrip, GridColourScheme colourScheme, Type businessEntityType, bool editable = false) : base(parentFilterStrip, colourScheme, businessEntityType, editable)
			{
			}

			public override string ToString()
			{
				return "FilterStripBusinessObjectForMultiThread";
			}

			public override bool Equals(object obj)
			{
				return true;
			}

			public override int GetHashCode()
			{
				return 1;
			}
		}

		#endregion
	}
}
