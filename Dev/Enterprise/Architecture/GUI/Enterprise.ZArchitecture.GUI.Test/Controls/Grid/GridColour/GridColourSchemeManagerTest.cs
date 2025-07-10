using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Testing
{
	sealed class GridColourSchemeManagerTest : TestCaseWithFactory
	{
		public void TestShowManageSchemeFormWithZFormModaliserShowDialogAndDispose()
		{
			ZFormModaliser.LastFormShownDialogForTest = null;
			var fbo = new FilterStripBusinessObjectForTest();
			using (var control = new FilterStripControlForTest(new SchemeCollection(Factory), fbo))
			{
				var man = new GridColourSchemeManager(control.FilteredGrid);
				CreateColorScheme(control.FilterBusinessObject, false, EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentUser.PK);
				man.GridColoursParentMenuItem.PerformSelect();
				man.GridColourManageMenuItem.PerformSelect();
				man.GridColourManageMenuItem.MenuItems[0].PerformSelect();
				man.GridColourManageMenuItem.MenuItems[0].PerformClick();

				AssertNotNull(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

#if !WINZOR
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestGetLatestGridColourSchemeInNewForm()
		{
			var fbo = new FilterStripBusinessObjectForTest();
			using (var control = new FilterStripControlForTest(new SchemeCollection(Factory), fbo))
			{
				var manager = new SchemeManagerForTestWithForm(control.FilteredGrid);
				var scheme = CreateColorScheme(control.FilterBusinessObject, false, EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentUser.PK);
				manager.GridColoursParentMenuItem.PerformSelect();
				manager.GridColourManageMenuItem.PerformSelect();
				manager.GridColourManageMenuItem.MenuItems[0].PerformSelect();
				manager.GridColourManageMenuItem.MenuItems[0].PerformClick();
				manager.customColorForm.Dispose();

				const string updateSql = @"UPDATE dbo.StmModuleFilter SET S9_FilterData = @S9_FilterData WHERE S9_PK = @S9_PK";
				using (var ruleRelationTable = new DataTable("ColourStrips"))
				using (var colourStripsDataSet = new DataSet())
				using (var stream = new MemoryStream())
				using (var command = Db.Connection.Command(updateSql))
				{
					ruleRelationTable.Columns.Add("RulePK", typeof(string));
					ruleRelationTable.Columns.Add("RuleName", typeof(string));
					ruleRelationTable.Columns.Add("BGColor", typeof(int));
					ruleRelationTable.Rows.Add(scheme.PK, "rule2", Color.Red.ToArgb());
					colourStripsDataSet.Locale = CultureInfo.InvariantCulture;
					colourStripsDataSet.Tables.Add(ruleRelationTable);
					colourStripsDataSet.WriteXml(stream, XmlWriteMode.IgnoreSchema);
					stream.Position = 0;

					command.AddParameter("@S9_PK", SqlDbType.UniqueIdentifier, scheme.PK.ToGuid());
					command.AddParameter("@S9_FilterData", SqlDbType.VarBinary, stream.ToArray());
					command.ExecuteNonQuery();
				}

				manager.GridColourManageMenuItem.MenuItems[0].PerformClick();
				AssertEquals("Should get latest Grid Colour Scheme in new form", "rule2", manager.customColorForm.RulesTabControl.TabPages[0].Text);
				manager.customColorForm.Dispose();
			}
		}
#endif

		[ExpectException(typeof(ArgumentNullException))]
		public void TestArgumentNull()
		{
			var man = new GridColourSchemeManager(null);
		}

		public void TestContextMenusAreAddedToGrid()
		{
			var fbo = new FilterStripBusinessObjectForTest();
			using (ZFilterStripControl control = new FilterStripControlTest.DummyZFilterStripControl(new SchemeCollection(Factory), fbo))
			{
				control.FilteredGrid.SetDataBinding(new DummyBusinessObjectCollection(Factory), "");
				AssertNotNull("Should add main grid color menu", control.FilteredGrid.ContextMenu.MenuItems.FindByText("Grid Colors"));
				AssertNotNull("Should add color menu subitms", control.FilteredGrid.ContextMenu.MenuItems.FindByText("Select Color Scheme", true));
				AssertNull("Should not add 'not supported' menu stub", control.FilteredGrid.ContextMenu.MenuItems.FindByText("Custom colors are not supported on this Grid", true));
			}
		}

		public void TestContextMenusAreAddedToGridWithNoFilterStripControl()
		{
			using (var form = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				var grid = new ZGrid { BindTo = "Collection" };
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
				form.Controls.Add(grid);
				form.Show();
				Application.DoEvents();

				AssertNotNull("Should add main grid color menu", grid.ContextMenu.MenuItems.FindByText("Grid Colors"));
				AssertNotNull("Should add color menu sub-items", grid.ContextMenu.MenuItems.FindByText("Select Color Scheme", true));
				AssertNull("Should not add 'not supported' menu stub", grid.ContextMenu.MenuItems.FindByText("Custom colors are not supported on this Grid", true));
			}
		}

		#region TestContextMenusAlsoAddedToGridWithNonPersistentBizo

		public void TestContextMenusAlsoAddedToGridWithNonPersistentBizo()
		{
			using (var form = new ZForm(Factory.New<DummyWithNpBizo>()))
			{
				var grid = new ZGrid { BindTo = "NpBizos" };
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Text", 80));
				form.Controls.Add(grid);
				form.Show();
				Application.DoEvents();

				AssertNotNull("Should add main grid colors menu", grid.ContextMenu.MenuItems.FindByText("Grid Colors"));
				AssertNotNull("Should add color menu sub-items", grid.ContextMenu.MenuItems.FindByText("Select Color Scheme", true));
				AssertNull("Should not add 'not supported' menu stub", grid.ContextMenu.MenuItems.FindByText("Custom colors are not supported on this Grid", true));
			}
		}

		class NpBizo : NonPersistentBusinessObject
		{
			public ZString Text { get; set; }
		}

		class NpBizoCollection : NonPersistentBusinessObjectCollection<NpBizo>
		{
			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				return new NpBizo();
			}
		}

		class DummyWithNpBizo : DummyBusinessObject
		{
			public DummyWithNpBizo(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public NpBizoCollection NpBizos => new NpBizoCollection();
		}

		#endregion

		public void TestColorSchemeNotSetColorStripsAfterPopulated()
		{
			var fbo = new FilterStripBusinessObjectForTest();
			using (var control = new FilterStripControlForTest(new SchemeCollection(Factory), fbo))
			{
				var colorStrip = new GridColourStripBusinessObject(control.FilterBusinessObject, null, null);
				var man = new GridColourSchemeManager(control.FilteredGrid);

				var systemScheme = Factory.New<GridColourScheme>();
				systemScheme.S9_FilterName = "[duduk]";
				systemScheme.S9_ModuleID = "_CS";
				systemScheme.S9_IsSystem = true;
				systemScheme.S9_GC = CargoWise.Types.ZGuid.Empty;
				colorStrip = new GridColourStripBusinessObject(control.FilterBusinessObject, systemScheme, null);
				systemScheme.ColourStrips.Add(colorStrip);

				var userScheme = Factory.New<GridColourScheme>();
				userScheme.S9_FilterName = "user scheme";
				userScheme.S9_ModuleID = "_CS";
				userScheme.S9_IsPublished = false;
				userScheme.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				userScheme.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
				colorStrip = new GridColourStripBusinessObject(control.FilterBusinessObject, userScheme, null);
				colorStrip.BGColor = Color.Red;
				userScheme.ColourStrips.Add(colorStrip);
				Factory.Save();

				man.GridColoursParentMenuItem.PerformSelect();

				var userSchemeMenuItem = man.GridColourSelectMenuItem.MenuItems.FindByText("user scheme");
				AssertNotNull(userSchemeMenuItem);
				Assert(!userSchemeMenuItem.Checked);

				var userSchemePopulated = (GridColourScheme)userSchemeMenuItem.Tag;
				Assert("ColorStrips should not be set if color scheme menu is not checked", !userSchemePopulated.ColourStrips.Any());

				userSchemeMenuItem.PerformSelect();
				userSchemeMenuItem.PerformClick();
				AssertEquals("ColorStrips should be set if color scheme menu is checked", 1, userSchemePopulated.ColourStrips.Count);
				AssertEquals("Strip color should be red", Color.FromArgb(Color.Red.ToArgb()), userSchemePopulated.ColourStrips[0].BGColor);

				var newFactory = new BusinessObjectFactory();
				var schemeReloaded = newFactory.Load<GridColourScheme>(userScheme.PK);
				var anotherColorStrip = new GridColourStripBusinessObject(control.FilterBusinessObject, schemeReloaded, null);
				anotherColorStrip.BGColor = Color.Yellow;
				schemeReloaded.ColourStrips.Add(anotherColorStrip);
				newFactory.Save();

				man.GridColourFactory.ResetSchemes();
				man.GridColoursParentMenuItem.PerformSelect();

				userSchemeMenuItem = man.GridColourSelectMenuItem.MenuItems.FindByText("user scheme");
				userSchemeMenuItem.PerformSelect();
				userSchemeMenuItem.PerformClick();
				userSchemePopulated = (GridColourScheme)userSchemeMenuItem.Tag;
				AssertEquals("Strip color should be updated to yellow", Color.FromArgb(Color.Yellow.ToArgb()), userSchemePopulated.ColourStrips[0].BGColor);
			}
		}

		public void TestColorSchemeManagerNoIndexFiltersWhenIndexSearchIsEnabled()
		{
			var bizO = new FilterStripBusinessObjectForTest();
			var helper = new DummyFilterStripsHelper();
			helper.MockIsApplicableToBizOTypeIsAssignableFrom = true;
			bizO.AddHelperForTest(helper);
			using (var mocker = new GlowIndexQueryEngineMock())
			using (var control = new FilterStripControlForTest(new SchemeCollection(Factory), bizO))
			{
				bizO.IndexSearchFields = GetTestSearchFields();
				bizO.SearchType = SearchType.Index;
				var colorStrip = new GridColourStripBusinessObject(control.FilterBusinessObject, null, null);
				var man = new GridColourSchemeManager(control.FilteredGrid);

				var systemScheme = Factory.New<GridColourScheme>();
				systemScheme.S9_FilterName = "[duduk]";
				systemScheme.S9_ModuleID = "_CS";
				systemScheme.S9_IsSystem = true;
				systemScheme.S9_GC = ZGuid.Empty;
				colorStrip = new GridColourStripBusinessObject(control.FilterBusinessObject, systemScheme, null);
				systemScheme.ColourStrips.Add(colorStrip);

				var publishedScheme = Factory.New<GridColourScheme>();
				publishedScheme.S9_FilterName = "user scheme";
				publishedScheme.S9_ModuleID = "_CS";
				publishedScheme.S9_IsPublished = true;
				publishedScheme.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				publishedScheme.S9_RelatedEntityID = Factory.NewWithValidTestData(ObjectFactory.GetType("IGlbStaff")).PK;
				colorStrip = new GridColourStripBusinessObject(control.FilterBusinessObject, publishedScheme, null);
				publishedScheme.ColourStrips.Add(colorStrip);

				var userScheme = Factory.New<GridColourScheme>();
				userScheme.S9_FilterName = "user scheme";
				userScheme.S9_ModuleID = "_CS";
				userScheme.S9_IsPublished = false;
				userScheme.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				userScheme.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
				colorStrip = new GridColourStripBusinessObject(control.FilterBusinessObject, userScheme, null);
				colorStrip.BGColor = Color.Red;
				userScheme.ColourStrips.Add(colorStrip);
				Factory.Save();

				man.GridColoursParentMenuItem.PerformSelect();

				AssertEquals(null, colorStrip[IndexSearchFilterHelper.DefaultHiddenPrefix + "TestFilter"]);
				AssertEquals(string.Empty, ErrorReporter.LastKeyReported);
				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			}
		}

		SearchFieldCollection GetTestSearchFields()
		{
			var field1 = SearchField.Create(IndexSearchFilterHelper.DefaultHiddenPrefix + "TestFilter", "Test Filter");
			return new SearchFieldCollection("IDummyBusinessObject", new SearchField[] { field1 });
		}

		public void TestToggleAndPopulateGridColourMenuItems()
		{
			var fbo = new FilterStripBusinessObjectForTest();

			using (var control = new FilterStripControlForTest(new SchemeCollection(Factory), fbo))
			{
				var colorStrip = new GridColourStripBusinessObject(control.FilterBusinessObject, null, null);
				var man = new GridColourSchemeManager(control.FilteredGrid);

				var systemScheme = Factory.New<GridColourScheme>();
				systemScheme.S9_FilterName = "[duduk]";
				systemScheme.S9_ModuleID = "_CS";
				systemScheme.S9_IsSystem = true;
				systemScheme.S9_GC = CargoWise.Types.ZGuid.Empty;
				colorStrip = new GridColourStripBusinessObject(control.FilterBusinessObject, systemScheme, null);
				systemScheme.ColourStrips.Add(colorStrip);

				var publishedScheme = Factory.New<GridColourScheme>();
				publishedScheme.S9_FilterName = "user scheme";
				publishedScheme.S9_ModuleID = "_CS";
				publishedScheme.S9_IsPublished = true;
				publishedScheme.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				publishedScheme.S9_RelatedEntityID = Factory.NewWithValidTestData(ObjectFactory.GetType("IGlbStaff")).PK;
				colorStrip = new GridColourStripBusinessObject(control.FilterBusinessObject, publishedScheme, null);
				publishedScheme.ColourStrips.Add(colorStrip);

				var userScheme = Factory.New<GridColourScheme>();
				userScheme.S9_FilterName = "user scheme";
				userScheme.S9_ModuleID = "_CS";
				userScheme.S9_IsPublished = false;
				userScheme.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				userScheme.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
				colorStrip = new GridColourStripBusinessObject(control.FilterBusinessObject, userScheme, null);
				userScheme.ColourStrips.Add(colorStrip);

				var notAScheme = Factory.New<GridColourScheme>();
				notAScheme.S9_FilterName = "not a scheme";
				notAScheme.S9_ModuleID = "_CS";
				notAScheme.S9_IsPublished = true;
				notAScheme.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				notAScheme.S9_RelatedEntityID = ZGuid.NewZGuid();
				colorStrip = new GridColourStripBusinessObject(control.FilterBusinessObject, notAScheme, null);
				notAScheme.ColourStrips.Add(colorStrip);

				Factory.Save();

				man.GridColoursParentMenuItem.PerformSelect();

				AssertEquals(3, man.GridColourManageMenuItem.MenuItems.Count);
				AssertEquals(5, man.GridColourSelectMenuItem.MenuItems.Count);

				AssertNotNull(man.GridColourSelectMenuItem.MenuItems.FindByText("Standard*"));
				AssertNotNull(man.GridColourSelectMenuItem.MenuItems.FindByText("-"));
				AssertNotNull(man.GridColourSelectMenuItem.MenuItems.FindByText("[duduk]"));
				AssertNotNull(man.GridColourSelectMenuItem.MenuItems.FindByText("user scheme"));
				AssertNotNull(man.GridColourSelectMenuItem.MenuItems.FindByText("user scheme*"));

				AssertNotNull(man.GridColourManageMenuItem.MenuItems.FindByText("[duduk]"));
				AssertNotNull(man.GridColourManageMenuItem.MenuItems.FindByText("user scheme"));
				AssertNotNull(man.GridColourManageMenuItem.MenuItems.FindByText("user scheme*"));

				var userSchemeMenuItem = man.GridColourSelectMenuItem.MenuItems.FindByText("user scheme");
				Assert(!userSchemeMenuItem.Checked);
				userSchemeMenuItem.PerformSelect();
				userSchemeMenuItem.PerformClick();
				Assert(userSchemeMenuItem.Checked);
			}
		}

		public void TestToggleAndPopulateGridColourMenuItemsWithNoFilterStripControl()
		{
			using (var form = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				var grid = new ZGrid { BindTo = "Collection" };
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
				form.Controls.Add(grid);
				form.Show();
				Application.DoEvents();

				var man = new GridColourSchemeManager(grid);
				var filterBusinessObject = new GridFilterStripBusinessObject(grid);

				var systemScheme = Factory.New<GridColourScheme>();
				systemScheme.S9_FilterName = "[duduk]";
				systemScheme.S9_ModuleID = "|Collection|DummyBusinessObject_CS";
				systemScheme.S9_IsSystem = true;
				systemScheme.S9_GC = CargoWise.Types.ZGuid.Empty;
				var colorStrip = new GridColourStripBusinessObject(filterBusinessObject, systemScheme, null);
				systemScheme.ColourStrips.Add(colorStrip);

				var publishedScheme = Factory.New<GridColourScheme>();
				publishedScheme.S9_FilterName = "user scheme";
				publishedScheme.S9_ModuleID = "|Collection|DummyBusinessObject_CS";
				publishedScheme.S9_IsPublished = true;
				publishedScheme.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				publishedScheme.S9_RelatedEntityID = Factory.NewWithValidTestData(ObjectFactory.GetType("IGlbStaff")).PK;
				colorStrip = new GridColourStripBusinessObject(filterBusinessObject, publishedScheme, null);
				publishedScheme.ColourStrips.Add(colorStrip);

				var userScheme = Factory.New<GridColourScheme>();
				userScheme.S9_FilterName = "user scheme";
				userScheme.S9_ModuleID = "|Collection|DummyBusinessObject_CS";
				userScheme.S9_IsPublished = false;
				userScheme.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				userScheme.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
				colorStrip = new GridColourStripBusinessObject(filterBusinessObject, userScheme, null);
				userScheme.ColourStrips.Add(colorStrip);

				Factory.Save();

				man.GridColoursParentMenuItem.PerformSelect();

				AssertEquals(3, man.GridColourManageMenuItem.MenuItems.Count);
				AssertEquals(5, man.GridColourSelectMenuItem.MenuItems.Count);

				AssertNotNull(man.GridColourSelectMenuItem.MenuItems.FindByText("Standard*"));
				AssertNotNull(man.GridColourSelectMenuItem.MenuItems.FindByText("-"));
				AssertNotNull(man.GridColourSelectMenuItem.MenuItems.FindByText("[duduk]"));
				AssertNotNull(man.GridColourSelectMenuItem.MenuItems.FindByText("user scheme"));
				AssertNotNull(man.GridColourSelectMenuItem.MenuItems.FindByText("user scheme*"));

				AssertNotNull(man.GridColourManageMenuItem.MenuItems.FindByText("[duduk]"));
				AssertNotNull(man.GridColourManageMenuItem.MenuItems.FindByText("user scheme"));
				AssertNotNull(man.GridColourManageMenuItem.MenuItems.FindByText("user scheme*"));

				var userSchemeMenuItem = man.GridColourSelectMenuItem.MenuItems.FindByText("user scheme");
				Assert(!userSchemeMenuItem.Checked);
				userSchemeMenuItem.PerformSelect();
				userSchemeMenuItem.PerformClick();
				Assert(userSchemeMenuItem.Checked);
			}
		}

		public void TestResetSchemeAfterManageForm()
		{
			var fbo = new FilterStripBusinessObjectForTest();

			using (var control = new FilterStripControlForTest(new SchemeCollection(Factory), fbo))
			{
				var colorStrip = new GridColourStripBusinessObject(control.FilterBusinessObject, null, null);
				var man = new SchemeManagerForTest(control.FilteredGrid);

				var userScheme = Factory.New<GridColourScheme>();
				userScheme.S9_FilterName = "user scheme";
				userScheme.S9_ModuleID = "_CS";
				userScheme.S9_IsPublished = false;
				userScheme.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				userScheme.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
				colorStrip = new GridColourStripBusinessObject(control.FilterBusinessObject, userScheme, null);
				userScheme.ColourStrips.Add(colorStrip);

				Factory.Save();
				man.GridColoursParentMenuItem.PerformSelect();
				man.GridColourManageMenuItem.PerformSelect();
				man.GridColourManageMenuItem.MenuItems[0].PerformSelect();
				man.GridColourManageMenuItem.MenuItems[0].PerformClick();
				AssertEquals(666, ((GridColourScheme)man.GridColourManageMenuItem.MenuItems[0].Tag).StripColours[ZGuid.Empty.ToString()].Color.ToArgb());
			}
		}

		public void TestRemoveRuleAndCancel()
		{
			var fbo = new FilterStripBusinessObjectForTest();

			using (var control = new FilterStripControlForTest(new SchemeCollection(Factory), fbo))
			{
				var man = new SchemeManagerForTestWithForm(control.FilteredGrid);
				CreateColorScheme(control.FilterBusinessObject, false, EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentUser.PK);
				man.GridColoursParentMenuItem.PerformSelect();
				man.GridColourManageMenuItem.PerformSelect();
				man.GridColourManageMenuItem.MenuItems[0].PerformSelect();
				man.GridColourManageMenuItem.MenuItems[0].PerformClick();

				using (var form1 = man.customColorForm)
				{
					AssertEquals(2, form1.RulesTabControl.TabCount);
					AssertEquals("rule1", form1.RulesTabControl.TabPages[0].Text);

					man.customColorForm.ClickRemoveRuleButton();

					AssertEquals("Rule", form1.RulesTabControl.TabPages[0].Text);
					man.customColorForm.CancelButton.PerformClick();
				}

				man.GridColoursParentMenuItem.PerformSelect();
				man.GridColourManageMenuItem.PerformSelect();
				man.GridColourManageMenuItem.MenuItems[0].PerformSelect();
				man.GridColourManageMenuItem.MenuItems[0].PerformClick();

				using (var form2 = man.customColorForm)
				{
					AssertEquals("rule1", form2.RulesTabControl.TabPages[0].Text);
				}
			}
		}

		public void TestQueryHintsAreAddedIfEnabled()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Collection.AddNew().Z0_Code = "TST";
			dummy.Factory.Save();

			var colorScheme = dummy.Factory.New<GridColourScheme>();

			var query = new ZDBOnlyQuery(typeof(DummyChildBusinessObject));
			query.AddFilterAndZSQLParameterCollection("dbo.GetCustomFieldByName(Z0_Code,'Code')='TST'", null);
			colorScheme.ColourStrips.Add(new GridColourStripBizoWithCustomQueryForTest(query) { BGColor = Color.Red });

			using (var form = new ZForm(dummy))
			using (SystemDataRegistry.Instance.EnableQueryHintsForColourSchemeManager.SetTemporaryValue(Guid.Empty,
				Guid.Empty, Guid.Empty, true))
			using (TestConnection.TrackExecutedCommands())
			{
				var grid = new ZGrid { BindTo = "Collection" };
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
				form.Controls.Add(grid);

				grid.SetDataBinding(dummy, "Collection");
				grid.GridColourSchemeManagerForTest.colourFactory.SetLastUsedSchemeForCurrentUser(grid.GridColourSchemeManagerForTest.FilterBusinessObject, colorScheme);

				grid.StartLoadBackgroundColour();
				while (!grid.IsBackgroundColourLoaded)
				{
					Thread.Sleep(100);
					Application.DoEvents();
				}

				AssertEquals("Query should contain WITH (FORCESEEK, NOLOCK, INDEX(PK_UX__Z0_PK))", true, grid.GridColourSchemeManagerForTest.ExecutedCommandsForUT.Any(c => c.Contains("FROM dbo.DummyBizo WITH (FORCESEEK, NOLOCK, INDEX(PK_UX__Z0_PK))")));
			}
		}

		[UseSnapshotProtection(true)]
		public void TestQueryCanReRunWithoutExceptionIfHintsAreIncorrect()
		{
			try
			{
				TypeDecider.AddSubstitution(typeof(DummyChildBusinessObject), typeof(DummyChildBusinessObjectWithPKSchemaColumn));

				var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
				var child = Factory.NewWithValidTestData<DummyChildBusinessObjectWithPKSchemaColumn>();
				child.Z0_Code = "TST";
				dummy.Collection.Add(child);
				dummy.Factory.Save();

				var colorScheme = dummy.Factory.New<GridColourScheme>();

				var query = new ZDBOnlyQuery(typeof(DummyChildBusinessObject));
				query.AddToFilter(DummyBizoSchema.Z0_Code, "TST");
				colorScheme.ColourStrips.Add(new GridColourStripBizoWithCustomQueryForTest(query) { BGColor = Color.Red });

				using (var form = new ZForm(dummy))
				using (SystemDataRegistry.Instance.EnableQueryHintsForColourSchemeManager.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (TestConnection.TrackExecutedCommands())
				{
					var grid = new ZGrid { BindTo = "Collection" };
					grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
					form.Controls.Add(grid);

					grid.SetDataBinding(dummy, "Collection");
					grid.GridColourSchemeManagerForTest.colourFactory.SetLastUsedSchemeForCurrentUser(grid.GridColourSchemeManagerForTest.FilterBusinessObject, colorScheme);

					ErrorReporter.Clear();
					grid.StartLoadBackgroundColour();
					while (!grid.IsBackgroundColourLoaded)
					{
						Thread.Sleep(1000);
						Application.DoEvents();
					}

					var index = grid.GridColourSchemeManagerForTest.ExecutedCommandsForUT.IndexOf(c => c.Contains("FROM dbo.DummyBizo WITH (FORCESEEK, NOLOCK, INDEX(NR_RX__Z0_BitFiltered))\r\n\tWHERE"));
					var nextQuery = grid.GridColourSchemeManagerForTest.ExecutedCommandsForUT.ElementAt(index + 1);

					AssertEquals("Query should contain 'WITH (FORCESEEK, NOLOCK, INDEX(NR_RX__Z0_BitFiltered))'", true, index >= 0);
					AssertEquals(0, ErrorReporter.TotalErrorCount);
					AssertEquals("Because the previous query failed, the next query should be executed without hints", true, nextQuery.Contains("FROM dbo.DummyBizo WITH (NOLOCK)"));
				}
			}
			finally
			{
				TypeDecider.RemoveSubstitution(typeof(DummyChildBusinessObject));
			}
		}

		[UseSnapshotProtection(true)]
		public void TestQueryReRunFailsShouldNotifyUser()
		{
			try
			{
				TypeDecider.AddSubstitution(typeof(DummyChildBusinessObject), typeof(DummyChildBusinessObjectWithPKSchemaColumn));

				var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
				var child = Factory.NewWithValidTestData<DummyChildBusinessObjectWithPKSchemaColumn>();
				child.Z0_Code = "TST";
				dummy.Collection.Add(child);
				dummy.Factory.Save();

				var colorScheme = dummy.Factory.New<GridColourScheme>();

				var query = new ZDBOnlyQuery(typeof(DummyChildBusinessObject));
				query.AddToFilter(DummyBizoSchema.Z0_Code, "TestQueryReRunFailsShouldNotifyUser");
				var colorStrip = new GridColourStripBizoWithCustomQueryForTest(query) { BGColor = Color.Red, RuleName = "Test Rule" };
				colorScheme.ColourStrips.Add(colorStrip);
				colorScheme.S9_FilterName = "Test Scheme";
				var filterStrip = colorStrip.FilterStrips.AddNew();

				using (var form = new ZForm(dummy))
				using (SystemDataRegistry.Instance.EnableQueryHintsForColourSchemeManager.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (TestConnection.TrackExecutedCommands())
				{
					var grid = new ZGrid { BindTo = "Collection" };
					grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
					form.Controls.Add(grid);

					grid.SetDataBinding(dummy, "Collection");
					grid.GridColourSchemeManagerForTest.colourFactory.SetLastUsedSchemeForCurrentUser(grid.GridColourSchemeManagerForTest.FilterBusinessObject, colorScheme);

					ErrorReporter.Clear();
					grid.StartLoadBackgroundColour();
					while (!grid.IsBackgroundColourLoaded)
					{
						Thread.Sleep(1000);
						Application.DoEvents();
					}

					AssertEquals(1, ErrorReporter.TotalErrorCount);
					AssertEquals(true, ErrorReporter.LastMessageReported.Contains("<FilterLayoutValuesSerializer>"));
					AssertEquals(true, ErrorReporter.LastMessageReported.Contains("<FilterLayoutSerializer>"));
					var expectedCallStack = "LoadGridColors";
					AssertContains(expectedCallStack, ErrorReporter.LastExceptionReported.StackTrace);
					ErrorReporter.Clear();

					var expectedMessage = @"Failed to apply color scheme ""Test Scheme"" to grid rows.
Rule Name: Test Rule.
Reason: Query processor could not produce a query plan.

Custom color schemes were temporarily disabled for this grid, please switch to default color scheme and check the current scheme filters and settings for possible errors.";
					AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			finally
			{
				TypeDecider.RemoveSubstitution(typeof(DummyChildBusinessObject));
			}
		}

		[UseSnapshotProtection(true)]
		public void TestQueryHintsAreDisabledIfAddCustomSQLFilter()
		{
			try
			{
				TypeDecider.AddSubstitution(typeof(DummyChildBusinessObject), typeof(DummyChildBusinessObjectWithPKSchemaColumn));

				var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
				var child = Factory.NewWithValidTestData<DummyChildBusinessObjectWithPKSchemaColumn>();
				child.Z0_Code = "TST";
				dummy.Collection.Add(child);
				dummy.Factory.Save();

				var colorScheme = dummy.Factory.New<GridColourScheme>();

				var query = new ZDBOnlyQuery(typeof(DummyChildBusinessObject));
				query.AddToFilter(DummyBizoSchema.Z0_Code, "TST");
				colorScheme.ColourStrips.Add(new GridColourStripBizoWithCustomQueryForTest(query) { BGColor = Color.Red });

				var filter = new ModuleSQLFilter($"Custom SQL ({query})", colorScheme.ColourStrips[0].QueryObjectType)
				{
					Property1 = "Z0_Code = 'hi'",
					Category = FilterCategories.Other,
					IsActive = true
				};
				colorScheme.ColourStrips[0].ModuleFilters.AddFilter(filter);

				ErrorReporter.Clear();

				using (var form = new ZForm(dummy))
				using (SystemDataRegistry.Instance.EnableQueryHintsForColourSchemeManager.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (TestConnection.TrackExecutedCommands())
				{
					var grid = new ZGrid { BindTo = "Collection" };
					grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
					form.Controls.Add(grid);

					grid.SetDataBinding(dummy, "Collection");
					grid.GridColourSchemeManagerForTest.colourFactory.SetLastUsedSchemeForCurrentUser(grid.GridColourSchemeManagerForTest.FilterBusinessObject, colorScheme);

					var eventArgs = new ColourDecidingEventArgs(dummy.Collection[0]);
					grid.GridColourSchemeManagerForTest.grid_CustomRowBackgroundColourDeciding(grid, eventArgs);

					var index = TestConnection.ExecutedCommands.IndexOf(c => c.Contains("FROM dbo.DummyBizo WITH (FORCESEEK, INDEX(NR_RX__Z0_BitFiltered))\r\n\tWHERE"));

					AssertEquals("Query should not contain 'WITH (FORCESEEK, INDEX(NR_RX__Z0_BitFiltered))'", false, index >= 0);
					AssertEquals(0, ErrorReporter.TotalErrorCount);
				}
			}
			finally
			{
				TypeDecider.RemoveSubstitution(typeof(DummyChildBusinessObject));
			}
		}

		[UseSnapshotProtection(true)]
		public void TestQueryCanReRunWithoutExceptionIfParentFilterHintsAreIncorrect()
		{
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.TVPRule = new TVPRule("0");
				try
				{
					TypeDecider.AddSubstitution(typeof(DummyChildBusinessObject), typeof(DummyChildBusinessObjectWithPKSchemaColumn));

					var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
					var child = Factory.NewWithValidTestData<DummyChildBusinessObjectWithPKSchemaColumn>();
					child.Z0_Code = "TST";
					dummy.Collection.Add(child);
					dummy.Factory.Save();

					var colorScheme = dummy.Factory.New<GridColourScheme>();

					var query = new ZDBOnlyQuery(typeof(DummyChildBusinessObject));
					query.AddToFilter(DummyBizoSchema.Z0_Code, "TST");
					var hint = new TableIndexHint(DummyBizoSchema.Constants.Indexes.NR_RX__Z0_BitFiltered);
					query.TableIndexHints.Add(hint);
					query.IsForceSeek = true;
					var subQuery = new ZDBOnlySubQuery(typeof(DummyChildBusinessObject), DummyBizoSchema.PK);
					subQuery.AddToFilter(DummyBizoSchema.Z0_Description, "DESC");
					subQuery.TableIndexHints.Add(new TableIndexHint(DummyBizoSchema.Constants.Indexes.NR_RX__Z0_BitFiltered));
					subQuery.IsForceSeek = true;
					query.AddSubQuery(subQuery, JoinCondition.And);

					colorScheme.ColourStrips.Add(new GridColourStripBizoWithCustomQueryForTest(query) { BGColor = Color.Red });

					using (var form = new ZForm(dummy))
					using (SystemDataRegistry.Instance.EnableQueryHintsForColourSchemeManager.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
					{
						var grid = new ZGrid { BindTo = "Collection" };
						grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
						form.Controls.Add(grid);

						grid.SetDataBinding(dummy, "Collection");
						grid.GridColourSchemeManagerForTest.colourFactory.SetLastUsedSchemeForCurrentUser(grid.GridColourSchemeManagerForTest.FilterBusinessObject, colorScheme);

						grid.StartLoadBackgroundColour();
						while (!grid.IsBackgroundColourLoaded)
						{
							Thread.Sleep(1000);
							Application.DoEvents();
						}

						var index = grid.GridColourSchemeManagerForTest.ExecutedCommandsForUT.IndexOf(c => c.Contains(@"FROM dbo.DummyBizo WITH (FORCESEEK, NOLOCK, INDEX(NR_RX__Z0_BitFiltered, NR_RX__Z0_BitFiltered))
	WHERE (Z0_PK in (SELECT Value FROM @CWO1_)) and (Z0_Code = @CWO2_ and Z0_PK IN (SELECT Z0_PK FROM dbo.DummyBizo WITH (FORCESEEK, INDEX(NR_RX__Z0_BitFiltered)) WHERE Z0_Description = @CWO3_))"));
						var nextQuery = grid.GridColourSchemeManagerForTest.ExecutedCommandsForUT.ElementAt(index + 1);

						AssertEquals("Query should contain tablehint'", true, index >= 0);
						AssertEquals(0, ErrorReporter.TotalErrorCount);
						AssertEquals("Because the previous query failed, the next query should be executed without any hints", true, nextQuery.Contains(@"FROM dbo.DummyBizo WITH (NOLOCK)
	WHERE (Z0_PK in (SELECT Value FROM @CWO1_)) and (Z0_Code = @CWO2_ and Z0_PK IN (SELECT Z0_PK FROM dbo.DummyBizo WHERE Z0_Description = @CWO3_))"));
					}
				}
				finally
				{
					TypeDecider.RemoveSubstitution(typeof(DummyChildBusinessObject));
				}
			}

			ErrorReporter.Clear();
		}

		class DummyChildBusinessObjectWithPKSchemaColumn : DummyChildBusinessObject
		{
			public DummyChildBusinessObjectWithPKSchemaColumn(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override SchemaGuidColumn PKSchemaColumn => new SchemaPKColumn(new DummyChildBizoWithPKSchemaColumnSchema(), DummyBizoSchema.Constants.PK, true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "Testing")]
		class DummyChildBizoWithPKSchemaColumnSchema : ITableSchema
		{
			#region ITableSchema

			string ITableSchema.SqlSchemaName => DummyBizoSchema.Constants.SqlSchemaName;

			string ITableSchema.TableName => DummyBizoSchema.Constants.TableName;

			SchemaPKColumn ITableSchema.PK => DummyBizoSchema.PK;

			string ITableSchema.PkIndexName => "NR_RX__Z0_BitFiltered";

			SchemaColumn ITableSchema.GetSchemaColumn(string columnName)
			{
				return ((ITableSchema)DummyBizoSchema.Instance).GetSchemaColumn(columnName);
			}

			SchemaColumnCollection ITableSchema.All => DummyBizoSchema.All;

			#endregion
		}

		GridColourScheme CreateColorScheme(FilterStripBusinessObject filterBusinessObject, bool isPublished, ZGuid companyPK, ZGuid relatedEntityPK)
		{
			var userScheme = Factory.New<GridColourScheme>();
			userScheme.S9_FilterName = "user scheme";
			userScheme.S9_ModuleID = "_CS";
			userScheme.S9_IsPublished = isPublished;
			userScheme.S9_GC = companyPK;
			userScheme.S9_RelatedEntityID = relatedEntityPK;
			var colorStrip = new GridColourStripBusinessObject(filterBusinessObject, userScheme, null);
			colorStrip.FilterStrips.AddNew();
			colorStrip.RuleName = "rule1";

			var filter1 = Factory.New<StmModuleFilter>();
			filter1.S9_FilterName = "rule1";
			filter1.S9_RelatedEntityID = relatedEntityPK;
			filter1.S9_GC = companyPK;
			filter1.S9_ModuleID = "LC" + GridColourFactory.ColorStripCode;
			colorStrip.StmModuleFilter = filter1;

			userScheme.ColourStrips.Add(colorStrip);

			Factory.Save();

			return userScheme;
		}

		#region TestFilterStripBusinessObjectIsLoadedFromModule

		public void TestFilterStripBusinessObjectIsLoadedFromModule()
		{
			using (var form = new ZForm())
			using (var grid = new ZGrid())
			{
				form.Controls.Add(grid);
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
				grid.SetDataBinding(new DummyBusinessObjectCollection(Factory), "");

				var manager = new GridColourSchemeManager(grid);
				AssertEquals("GridFilterStripBusinessObject", manager.FilterBusinessObject.GetType().Name);
			}

			using (var form = new ZForm())
			using (var grid = new ZGrid())
			{
				form.Controls.Add(grid);
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
				grid.SetDataBinding(new DummyCollectionWithModuleId(Factory), "");

				var manager = new GridColourSchemeManager(grid);
				AssertEquals("GlbStaffFilterBusinessObject", manager.FilterBusinessObject.GetType().Name);
			}
		}

		public void TestGetFilterControlInNonFIlterModuleForm()
		{
			using (var form = new ZForm())
			using (var grid = new ZGrid())
			{
				form.Controls.Add(grid);
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
				grid.SetDataBinding(new DummyCollectionWithModuleId(Factory), "");

				var manager = new GridColourSchemeManager(grid);
				AssertEquals("GlbStaffFilterBusinessObject", manager.FilterBusinessObject.GetType().Name);
				manager.CreateNewGridColourScheme(grid, new EventArgs());
				var colourManagerForm = ZFormModaliser.LastFormShownDialogForTest as IZGridColourCustomiseForm;
				AssertNotNull(colourManagerForm.StripControl);
			}
		}

		public void TestNewlyCreatedColourSchemeIsNotDefaultSelected()
		{
			using (var form = new ZForm())
			using (var grid = new ZGrid())
			{
				form.Controls.Add(grid);
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
				grid.SetDataBinding(new DummyCollectionWithModuleId(Factory), "");

				var manager = new GridColourSchemeManager(grid);
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(delegate(object form)
				{
					var colourForm = (ZForm)form;

					colourForm.Shown += delegate
					{
						colourForm.BusinessEntity.Factory.Save();
					};
				});
				manager.CreateNewGridColourScheme(grid, new EventArgs());

				var query = manager.GridColourFactory.GetLastUsedSchemeQuery(manager.FilterBusinessObject);
				var lastUsedScheme = Factory.LoadTop1<StmData>(query);
				AssertNull("The new colour scheme should not be used", lastUsedScheme);
			}
		}

		[ModuleID(ModuleId.GlbStaff)]
		class DummyCollectionWithModuleId : BusinessObjectCollection<DummyBusinessObject>
		{
			public DummyCollectionWithModuleId(BusinessObjectFactory factory) : base(factory) { }
		}

		#endregion

		#region TestPrecalculateColorsForAllRowsInGrid

		// Test for WI00181871/WI00186582 once we receive additional debugging info (the xml data that causes the problem)
		//
		//internal class GridColourSchemeManagerTestWithDummy : TestCaseWithDummy
		//{

		//	public void TestGrid_CustomRowBackgroundColourDecidingCore()
		//	{
		//		using (ZForm form = new ZForm(Dummy))
		//		{
		//			ZGrid grid = new ZGrid();
		//			grid.BindTo = "Collection";
		//			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
		//			form.Controls.Add(grid);
		//			grid.SetDataBinding(Dummy, "Collection");

		//			var bizO = grid.GridColourSchemeManagerForTest.FilterBusinessObject;
		//			var colorFactory = grid.GridColourSchemeManagerForTest.colourFactory;

		//			bool gotException = false;
		//			try
		//			{
		//				var colourScheme = colorFactory.GetLastUsedSchemeForCurrentUser(bizO);

		//			}
		//			catch (Exception e)
		//			{
		//				Console.WriteLine(e);
		//				gotException = true;
		//			}

		//			Assert(gotException);
		//		}
		//	}
		//}

		public void TestGetPrecalculateColorsForAllRowsInGridQueries_HandlesNullElementTypeError()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Collection.AddNew().Z0_Code = "AAA";
			dummy.Collection.AddNew().Z0_Code = "BBB";

			ErrorReporter.Clear();
			using (var form = new ZForm(dummy))
			using (var grid = new ZGrid { Name = "DummyGrid", BindTo = "Collection" })
			{
				grid.GridColourSchemeManagerForTest = new GridColourSchemeManagerForTest(grid);
				var colourScheme = dummy.Factory.New<GridColourScheme>();
				var gcsName = "testName";
				colourScheme.S9_FilterName = new ZString(gcsName);
				var query = new ZQuery();
				colourScheme.ColourStrips.Add(new GridColourStripBizoWithCustomQueryForTest(query) { BGColor = Color.Red });

				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
				form.Controls.Add(grid);

				grid.SetDataBinding(dummy, "Collection");
				grid.GridColourSchemeManagerForTest.colourFactory.SetLastUsedSchemeForCurrentUser(grid.GridColourSchemeManagerForTest.FilterBusinessObject, colourScheme);
				grid.GridColourSchemeManagerForTest.SimulateNullElementForTest = true;

				grid.GridColourSchemeManagerForTest.GetPrecalculateColorsForAllRowsInGridQueries(colourScheme);

				var lastUsedScheme = grid.GridColourSchemeManagerForTest.colourFactory.GetLastUsedSchemeForCurrentUser(grid.GridColourSchemeManagerForTest.FilterBusinessObject);
				AssertEquals("Color scheme should be reset to standard", null, lastUsedScheme);
			}
			AssertEquals("Types present in grid resulted in null elementType: CargoWise.EntityFramework.Testing.DummyChildBusinessObject", ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}

		public void TestHandleGridColourSchemeError_InvalidColumnName()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Collection.AddNew().Z0_Code = "AAA"; // 1 hit to find 1st colour filter matched
			dummy.Collection.AddNew().Z0_Code = "BBB"; // 2 hits to find 2nd colour filter matched

			using (var form = new ZForm(dummy))
			using (var grid = new ZGrid { Name = "DummyGrid", BindTo = "Collection" })
			{
				var colourScheme = dummy.Factory.New<GridColourScheme>();
				var gcsName = "testName";
				colourScheme.S9_FilterName = new ZString(gcsName);

				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
				form.Controls.Add(grid);

				grid.SetDataBinding(dummy, "Collection");
				grid.GridColourSchemeManagerForTest.colourFactory.SetLastUsedSchemeForCurrentUser(grid.GridColourSchemeManagerForTest.FilterBusinessObject, colourScheme);

				for (var i = 0; i < dummy.Collection.Count; i++)
				{
					var eventArgs = new ColourDecidingEventArgs(dummy.Collection[i]);
					grid.GridColourSchemeManagerForTest.grid_CustomRowBackgroundColourDeciding(grid, eventArgs);

					var ex = SqlExceptionBuilder.CreateSqlException(207, "Invalid column name 'WLV_Depth'.");
					var reason = "test reason";
					var reportOnceKey = "ROK";

					// todo: remove this line once we can replicate the issue via the call above to grid_CustomRowBackgroundColourDeciding
					grid.GridColourSchemeManagerForTest.HandleGridColourSchemeError(ex, eventArgs, reason, reason, reportOnceKey);

					AssertEquals(null, ErrorReporter.LastExceptionReported);
				}
			}
		}

		public void TestHandleGridColourSchemeError()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Collection.AddNew().Z0_Code = "AAA"; // 1 hit to find 1st colour filter matched
			dummy.Collection.AddNew().Z0_Code = "BBB"; // 2 hits to find 2nd colour filter matched

			using (var form = new ZForm(dummy))
			using (var grid = new ZGrid { Name = "DummyGrid", BindTo = "Collection" })
			{
				var colourScheme = dummy.Factory.New<GridColourScheme>();
				var gcsName = "testName";
				colourScheme.S9_FilterName = new ZString(gcsName);

				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
				form.Controls.Add(grid);

				grid.SetDataBinding(dummy, "Collection");
				grid.GridColourSchemeManagerForTest.colourFactory.SetLastUsedSchemeForCurrentUser(grid.GridColourSchemeManagerForTest.FilterBusinessObject, colourScheme);

				for (var i = 0; i < dummy.Collection.Count; i++)
				{
					var eventArgs = new ColourDecidingEventArgs(dummy.Collection[i]);
					grid.GridColourSchemeManagerForTest.grid_CustomRowBackgroundColourDeciding(grid, eventArgs);

					var ex = new Exception("test exception");
					var reason = "test reason";
					var reportOnceKey = "ROK";

					// todo: remove this line once we can replicate the issue via the call above to grid_CustomRowBackgroundColourDeciding
					grid.GridColourSchemeManagerForTest.HandleGridColourSchemeError(ex, eventArgs, reason, reason, reportOnceKey);

					AssertEquals(
							$"Failed to apply colour scheme \"{gcsName}\" to grid DummyGrid: \r\n"
							+ $"Reason: {reason}\r\n"
							+ "Exception info: System.Exception: test exception\r\n"
							+ $"ColourDecidingEventArgs: ObjectAtRow:CargoWise.EntityFramework.Testing.DummyChildBusinessObject Colour:Color [Empty] ReadOnlyColour:Color [Empty] Pk:{dummy.Collection[0].PK}\r\n"
							+ "XML of last layout: \"\"\r\n"
							+ "Original Exception: System.Exception\r\n"
							+ "Original Exception Message: test exception\r\n"
							+ "Original Exception Stack Trace: "
							, ErrorReporter.LastMessageReported);

					AssertEquals(ex.Message, ErrorReporter.LastExceptionReported.Message);
				}
			}
			ErrorReporter.Clear();
		}

		public void TestHandleGridColourSchemeErrorWithSqlExceptionCannotFindEitherColumn()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Collection.AddNew().Z0_Code = "TST";

			foreach (DummyChildBusinessObject childBusinessObject in dummy.Collection)
			{
				childBusinessObject.Z0_Guid = dummy.PK;
			}
			dummy.Factory.Save();

			var colorScheme = dummy.Factory.New<GridColourScheme>();

			var query = new ZDBOnlyQuery(typeof(DummyChildBusinessObject));
			query.AddFilterAndZSQLParameterCollection("dbo.GetCustomFieldByName(Z0_Code,'Code')='TST'", null);
			colorScheme.ColourStrips.Add(new GridColourStripBizoWithCustomQueryForTest(query) { BGColor = Color.Red });

			using (var form = new ZForm(dummy))
			{
				var grid = new ZGrid { BindTo = "Collection" };
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
				form.Controls.Add(grid);

				grid.SetDataBinding(dummy, "Collection");

				grid.GridColourSchemeManagerForTest.AllowPrecalculateColorsForAllRowsInGrid = true;
				grid.GridColourSchemeManagerForTest.colourFactory.SetLastUsedSchemeForCurrentUser(grid.GridColourSchemeManagerForTest.FilterBusinessObject, colorScheme);

				grid.StartLoadBackgroundColour();
				while (!grid.IsBackgroundColourLoaded)
				{
					Thread.Sleep(1000);
					Application.DoEvents();
				}

				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
				AssertEquals("Failed to apply color scheme \"\" to grid rows.\r\nRule Name: Unknown.\r\nReason: Problem in the colour scheme data, to fix this problem it will be necessary delete and create this colour scheme again.\r\n\r\nCustom color schemes were temporarily disabled for this grid, please switch to default color scheme and check the current scheme filters and settings for possible errors.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestHandleGridColourSchemeErrorWithEvaluateException()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Collection.AddNew().Z0_Code = "TEST";

			using (var form = new ZForm(dummy))
			using (var grid = new ZGrid { BindTo = "Collection" })
			{
				var colourScheme = dummy.Factory.New<GridColourScheme>();
				colourScheme.S9_FilterName = "ColourSchemeName";

				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
				form.Controls.Add(grid);

				grid.SetDataBinding(dummy, "Collection");
				grid.GridColourSchemeManagerForTest.colourFactory.SetLastUsedSchemeForCurrentUser(grid.GridColourSchemeManagerForTest.FilterBusinessObject, colourScheme);

				for (var i = 0; i < dummy.Collection.Count; i++)
				{
					const string reason = "Reason";

					var eventArgs = new ColourDecidingEventArgs(dummy.Collection[i]);
					grid.GridColourSchemeManagerForTest.grid_CustomRowBackgroundColourDeciding(grid, eventArgs);
					grid.GridColourSchemeManagerForTest.HandleGridColourSchemeError(new EvaluateException("Test Exception"), eventArgs, reason, string.Empty, string.Empty);

					AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
					AssertEquals(0, ErrorReporter.TotalErrorCount);
				}
			}

			ErrorReporter.Clear();
		}

		public void TestHandleGridColourSchemeErrorWhenLastUsedSchemeIsNull()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Collection.AddNew().Z0_Code = "TEST";

			using (var form = new ZForm(dummy))
			using (var grid = new ZGrid { BindTo = "Collection" })
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
				form.Controls.Add(grid);

				grid.SetDataBinding(dummy, "Collection");

				AssertNoExceptionThrown("NullRef exceptionhandle", () =>
				{
					grid.GridColourSchemeManagerForTest.HandleGridColourSchemeError(new Exception(), new ColourDecidingEventArgs(dummy.Collection[0]), "Reason", string.Empty, string.Empty);
				});
			}

			ErrorReporter.Clear();
		}

		public void TestHandleGridColourSchemeErrorForTimeoutException()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Collection.AddNew().Z0_Code = "TST";
			dummy.Factory.Save();

			var colorScheme = dummy.Factory.New<GridColourScheme>();

			using (var form = new ZForm(dummy))
			using (SystemDataRegistry.Instance.QueryTimeoutForColourSchemeManager.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, 2))
			{
				var grid = new ZGrid { BindTo = "Collection" };
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
				form.Controls.Add(grid);

				var colorManager = new GridColourSchemeManagerForTestShowError(grid);

				grid.SetDataBinding(dummy, "Collection");
				colorManager.colourFactory.SetLastUsedSchemeForCurrentUser(colorManager.FilterBusinessObject, colorScheme);

				var eventArgs = new ColourDecidingEventArgs(dummy.Collection[0]);
				colorManager.grid_CustomRowBackgroundColourDeciding(grid, eventArgs);

				AssertEquals(@"Failed to apply color scheme """" to grid rows.
Rule Name: Unknown.
Reason: The selected Grid Color Scheme could not be applied in a timely manner. Please check if the Grid Color Scheme rules and filters can be made more specific.
The Grid Color Scheme exceeded the timeout value of 2 seconds that is set in the registry at Optimization -> Color Scheme Manager Query Timeout.

Custom color schemes were temporarily disabled for this grid, please switch to default color scheme and check the current scheme filters and settings for possible errors.",
					UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestColorCacheShouldBeExpiredWhenResetEvent()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Collection.AddNew();
			dummy.Collection.AddNew();
			dummy.Collection.AddNew();

			dummy.Factory.Save();
			var colorScheme = dummy.Factory.New<GridColourScheme>();

			using (var form = new ZForm(dummy))
			using (var grid = new ZGrid { BindTo = "Collection" })
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_ChildOnly", 80));
				form.Controls.Add(grid);
				grid.SetDataBinding(dummy, "Collection");

				var gridStrip = new GridFilterStripBusinessObject(grid);

				var colorStrip = new GridColourStripBusinessObject(gridStrip, colorScheme, dummy.GetType()) { BGColor = Color.Red };
				colorStrip.RuleName = "Red";

				var filters = colorStrip.GetModuleFilters();
				var npFilter = ((ModuleNumberRangeFilter)filters["Z0_ChildOnly"]);
				npFilter.Property1 = 4;
				npFilter.Property2 = 6;
				npFilter.IsActive = true;

				var sqlFilter = (ModuleSQLFilter)filters["Custom SQL Filter"];
				sqlFilter.Property1 = "Z0_Code <> 'HAY'";
				sqlFilter.IsActive = true;

				colorScheme.ColourStrips.Add(colorStrip);

				colorStrip = new GridColourStripBusinessObject(gridStrip, colorScheme, dummy.GetType()) { BGColor = Color.Green };
				colorStrip.RuleName = "Green";

				filters = colorStrip.GetModuleFilters();
				npFilter = ((ModuleNumberRangeFilter)filters["Z0_ChildOnly"]);
				npFilter.Property1 = 15;
				npFilter.Property2 = 25;
				npFilter.IsActive = true;

				colorScheme.ColourStrips.Add(colorStrip);
				ZArchitecture.Testing.GridColourSchemeManagerForTest.RecreateGridColourSchemeManager(grid, colorScheme, true);
				Factory.Save();

				grid.StartLoadBackgroundColour();
				while (!grid.IsBackgroundColourLoaded)
				{
					Thread.Sleep(1000);
					Application.DoEvents();
				}

				AssertEquals(false, grid.ColorByPKIsExpired);
				grid.OGrid_ListChanged(form, new ListChangedEventArgs(ListChangedType.Reset, null));
				AssertEquals(true, grid.ColorByPKIsExpired);
				grid.StartLoadBackgroundColour();
				while (!grid.IsBackgroundColourLoaded)
				{
					Thread.Sleep(1000);
					Application.DoEvents();
				}
				AssertEquals(false, grid.ColorByPKIsExpired);
			}
		}

		public void TestHandleGridColourSchemeWithInvalidIndex()
		{
			ErrorReporter.Instance.Clear();
			using (var form = new ZForm(Factory.New<DummyWithDependentsBusinessObject>()))
			{
				var grid = new ZGrid();
				grid.BindTo = "ActiveDependents";
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Text", 80));
				form.Controls.Add(grid);
				form.Show();
				Application.DoEvents();

				var colourFacotry = new GridColourFactory(grid);
				var filterStrip = new GridFilterStripBusinessObject(grid);
				((IFilterStripBusinessObjectInternals)filterStrip).LayoutContext = "dummy";

				var scheme = Factory.New<GridColourScheme>();
				scheme.S9_FilterName = "Z0_Code";
				var colorData = Factory.New<StmData>();
				colorData.SD_GuidValue = scheme.PK;

				Factory.Save();

				colourFacotry.SetLastUsedSchemeForCurrentUser(filterStrip, scheme);
				AssertEquals("should be true", true, grid.RowColorsAreDataViewOptimisable);
				grid.OGrid_ListChanged(form, new ListChangedEventArgs(ListChangedType.ItemChanged, 10));
				AssertEquals(0, ErrorReporter.TotalErrorCount);

				grid.OGrid_ListChanged(form, new ListChangedEventArgs(ListChangedType.ItemChanged, -1));
				AssertEquals(0, ErrorReporter.TotalErrorCount);
			}
		}

		public void TestHandleGridColourSchemeForCalculatedAndDbOnlyQuery()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Collection.AddNew().Z0_ChildOnly = 5; //0
			dummy.Collection.AddNew().Z0_ChildOnly = 10; //1
			dummy.Collection.AddNew().Z0_ChildOnly = 20; //2
			var child = dummy.Collection.AddNew(); //3
			child.Z0_ChildOnly = 5;
			child.Z0_Code = "HAY";

			dummy.Factory.Save();

			var colorScheme = dummy.Factory.New<GridColourScheme>();

			using (var form = new ZForm(dummy))
			using (var grid = new ZGrid { BindTo = "Collection" })
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_ChildOnly", 80));
				form.Controls.Add(grid);

				grid.SetDataBinding(dummy, "Collection");

				var gridStrip = new GridFilterStripBusinessObject(grid);

				var colorStrip = new GridColourStripBusinessObject(gridStrip, colorScheme, dummy.GetType()) { BGColor = Color.Red };
				colorStrip.RuleName = "Red";

				var filters = colorStrip.GetModuleFilters();
				var npFilter = ((ModuleNumberRangeFilter)filters["Z0_ChildOnly"]);
				npFilter.Property1 = 4;
				npFilter.Property2 = 6;
				npFilter.IsActive = true;

				var sqlFilter = (ModuleSQLFilter)filters["Custom SQL Filter"];
				sqlFilter.Property1 = "Z0_Code <> 'HAY'";
				sqlFilter.IsActive = true;

				colorScheme.ColourStrips.Add(colorStrip);

				colorStrip = new GridColourStripBusinessObject(gridStrip, colorScheme, dummy.GetType()) { BGColor = Color.Green };
				colorStrip.RuleName = "Green";

				filters = colorStrip.GetModuleFilters();
				npFilter = ((ModuleNumberRangeFilter)filters["Z0_ChildOnly"]);
				npFilter.Property1 = 15;
				npFilter.Property2 = 25;
				npFilter.IsActive = true;

				colorScheme.ColourStrips.Add(colorStrip);

				colorStrip = new GridColourStripBusinessObject(gridStrip, colorScheme, dummy.GetType()) { BGColor = Color.Blue };
				colorStrip.RuleName = "Blue";

				filters = colorStrip.GetModuleFilters();
				npFilter = ((ModuleNumberRangeFilter)filters["Z0_ChildOnly"]);
				npFilter.Property1 = 1;
				npFilter.Property2 = 99;
				npFilter.IsActive = true;

				colorScheme.ColourStrips.Add(colorStrip);
				ZArchitecture.Testing.GridColourSchemeManagerForTest.RecreateGridColourSchemeManager(grid, colorScheme, true);
				Factory.Save();

				grid.StartLoadBackgroundColour();
				while (!grid.IsBackgroundColourLoaded)
				{
					Thread.Sleep(1000);
					Application.DoEvents();
				}

				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
				AssertEquals(0, ErrorReporter.TotalErrorCount);
				AssertEquals(grid.ColorByPK[dummy.Collection[0].PK], Color.Red);
				AssertEquals(grid.ColorByPK[dummy.Collection[1].PK], Color.Blue);
				AssertEquals(grid.ColorByPK[dummy.Collection[2].PK], Color.Green);
				AssertEquals(grid.ColorByPK[dummy.Collection[3].PK], Color.Blue);
			}
		}

		internal class GridColourSchemeManagerForTestShowError : GridColourSchemeManager
		{
			public GridColourSchemeManagerForTestShowError(ZGrid grid) : base(grid)
			{ }

			protected override void grid_CustomRowBackgroundColourDecidingCore(ColourDecidingEventArgs e)
			{
				base.grid_CustomRowBackgroundColourDecidingCore(e);

				// Mock sql timeout exception
				string sqlText = "SELECT top 1 * FROM dbo.DummyBizo WAITFOR DELAY '00:00:03'";
				Db.Connection.ExecuteNonQuery(sqlText, 1);
			}
		}

		public void TestHandleGridColourSchemeErrorWithUnknownException()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Collection.AddNew().Z0_Code = "TST";
			dummy.Factory.Save();

			var colorScheme = dummy.Factory.New<GridColourScheme>();

			using (var form = new ZForm(dummy))
			using (SystemDataRegistry.Instance.EnableQueryHintsForColourSchemeManager.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.QueryTimeoutForColourSchemeManager.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, 2))
			{
				var grid = new ZGrid { BindTo = "Collection" };
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
				form.Controls.Add(grid);

				var colorManager = new GridColourSchemeManagerForTestShowUnknownError(grid);
				colorManager.ThrownExceptionForTest = new NullReferenceException();

				grid.SetDataBinding(dummy, "Collection");
				colorManager.colourFactory.SetLastUsedSchemeForCurrentUser(colorManager.FilterBusinessObject, colorScheme);

				var eventArgs = new ColourDecidingEventArgs(dummy.Collection[0]);
				colorManager.grid_CustomRowBackgroundColourDeciding(grid, eventArgs);

				AssertEquals("GridColourSchemeManager.grid_CustomRowBackgroundColourDeciding - NullReferenceException", ErrorReporter.LastKeyReported);
				AssertContains("Failed to apply colour scheme \"\" to grid : \r\nReason: Object reference not set to an instance of an object.\r\nMore infos: EnableQueryHintsForColourSchemeManager=True. QueryTimeoutForColourSchemeManager=2", ErrorReporter.LastMessageReported);
				AssertEquals("Failed to apply color scheme \"\" to grid rows.\r\nRule Name: Unknown.\r\nReason: Object reference not set to an instance of an object..\r\n\r\nCustom color schemes were temporarily disabled for this grid, please switch to default color scheme and check the current scheme filters and settings for possible errors.", UnitTestUserNotification.Instance.LastMessage.Text);
				ErrorReporter.Clear();
			}
		}

		public void TestHandleGridColourSchemeErrorWillReportIssueSortByDifferentExceptionType_InvalidOperationException()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Collection.AddNew().Z0_Code = "DND";
			dummy.Factory.Save();

			var colorScheme = dummy.Factory.New<GridColourScheme>();

			using (var form = new ZForm(dummy))
			using (SystemDataRegistry.Instance.EnableQueryHintsForColourSchemeManager.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.QueryTimeoutForColourSchemeManager.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, 2))
			{
				var grid = new ZGrid { BindTo = "Collection" };
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
				form.Controls.Add(grid);

				var colorManager = new GridColourSchemeManagerForTestShowUnknownError(grid);
				colorManager.ThrownExceptionForTest = new InvalidOperationException();

				grid.SetDataBinding(dummy, "Collection");
				colorManager.colourFactory.SetLastUsedSchemeForCurrentUser(colorManager.FilterBusinessObject, colorScheme);

				var eventArgs = new ColourDecidingEventArgs(dummy.Collection[0]);
				colorManager.grid_CustomRowBackgroundColourDeciding(grid, eventArgs);

				AssertEquals("GridColourSchemeManager.grid_CustomRowBackgroundColourDeciding - InvalidOperationException", ErrorReporter.LastKeyReported);
				AssertContains("Failed to apply colour scheme \"\" to grid : \r\nReason: Operation is not valid due to the current state of the object.\r\nMore infos: EnableQueryHintsForColourSchemeManager=True. QueryTimeoutForColourSchemeManager=2", ErrorReporter.LastMessageReported);
				AssertEquals("Failed to apply color scheme \"\" to grid rows.\r\nRule Name: Unknown.\r\nReason: Operation is not valid due to the current state of the object..\r\n\r\nCustom color schemes were temporarily disabled for this grid, please switch to default color scheme and check the current scheme filters and settings for possible errors.", UnitTestUserNotification.Instance.LastMessage.Text);
				ErrorReporter.Clear();
			}
		}

		public void TestHandleGridColourSchemeErrorWillReportIssueSortByDifferentExceptionType_IndexOutOfRangeException()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Collection.AddNew().Z0_Code = "DND-1";
			dummy.Factory.Save();

			var colorScheme = dummy.Factory.New<GridColourScheme>();

			using (var form = new ZForm(dummy))
			using (SystemDataRegistry.Instance.EnableQueryHintsForColourSchemeManager.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.QueryTimeoutForColourSchemeManager.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, 2))
			{
				var grid = new ZGrid { BindTo = "Collection" };
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
				form.Controls.Add(grid);

				var colorManager = new GridColourSchemeManagerForTestShowUnknownError(grid);
				colorManager.ThrownExceptionForTest = new IndexOutOfRangeException();

				grid.SetDataBinding(dummy, "Collection");
				colorManager.colourFactory.SetLastUsedSchemeForCurrentUser(colorManager.FilterBusinessObject, colorScheme);

				var eventArgs = new ColourDecidingEventArgs(dummy.Collection[0]);
				colorManager.grid_CustomRowBackgroundColourDeciding(grid, eventArgs);

				AssertEquals("GridColourSchemeManager.grid_CustomRowBackgroundColourDeciding - IndexOutOfRangeException", ErrorReporter.LastKeyReported);
				AssertContains("Failed to apply colour scheme \"\" to grid : \r\nReason: Index was outside the bounds of the array.\r\nMore infos: EnableQueryHintsForColourSchemeManager=True. QueryTimeoutForColourSchemeManager=2", ErrorReporter.LastMessageReported);
				AssertEquals("Failed to apply color scheme \"\" to grid rows.\r\nRule Name: Unknown.\r\nReason: Index was outside the bounds of the array..\r\n\r\nCustom color schemes were temporarily disabled for this grid, please switch to default color scheme and check the current scheme filters and settings for possible errors.", UnitTestUserNotification.Instance.LastMessage.Text);
				ErrorReporter.Clear();
			}
		}

		class GridColourSchemeManagerForTestShowUnknownError : GridColourSchemeManager
		{
			public GridColourSchemeManagerForTestShowUnknownError(ZGrid grid) : base(grid)
			{ }

			protected override void grid_CustomRowBackgroundColourDecidingCore(ColourDecidingEventArgs e)
			{
				base.grid_CustomRowBackgroundColourDecidingCore(e);

				// Mock CriticalException which has no error handle
				throw ThrownExceptionForTest;
			}

			public Exception ThrownExceptionForTest { get; set; }
		}

		public void TestPrecalculateColorsForAllRowsInGrid()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Collection.AddNew().Z0_Code = "AAA"; // 1 hit to find 1st color filter matched
			dummy.Collection.AddNew().Z0_Code = "BBB"; // 2 hits to find 2nd color filter matched
			dummy.Collection.AddNew().Z0_Code = "CCC"; // 3 hits to find 3rd color filter matched
			dummy.Collection.AddNew().Z0_Code = "AAA"; // 1 hit
			dummy.Collection.AddNew().Z0_Code = "BBB"; // 2 hits
			dummy.Collection.AddNew().Z0_Code = "CCC"; // 3 hits
			foreach (DummyChildBusinessObject childBusinessObject in dummy.Collection)
			{
				childBusinessObject.Z0_Guid = dummy.PK;
			}
			dummy.Factory.Save();
			AssertPrecalculateColorsForAllRowsInGrid(dummy, true, 5); // 3 hits - once for each of 3 color filters + 2 for glb stuff
			AssertPrecalculateColorsForAllRowsInGrid(dummy, false, 14);

			dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Collection.AddNew().Z0_Code = "AAA";
			dummy.Collection.AddNew().Z0_Code = "AAA";
			dummy.Collection.AddNew().Z0_Code = "AAA";
			dummy.Collection.AddNew().Z0_Code = "AAA";
			dummy.Collection.AddNew().Z0_Code = "AAA";
			dummy.Collection.AddNew().Z0_Code = "AAA";
			foreach (DummyChildBusinessObject childBusinessObject in dummy.Collection)
			{
				childBusinessObject.Z0_Guid = dummy.PK;
			}
			dummy.Factory.Save();
			AssertPrecalculateColorsForAllRowsInGrid(dummy, true, 3);
			AssertPrecalculateColorsForAllRowsInGrid(dummy, false, 8); // 6 hits - 1st color filter is checked and mached for each element

			dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Collection.AddNew().Z0_Code = "XXX";
			dummy.Collection.AddNew().Z0_Code = "XXX";
			dummy.Collection.AddNew().Z0_Code = "XXX";
			dummy.Collection.AddNew().Z0_Code = "XXX";
			dummy.Collection.AddNew().Z0_Code = "XXX";
			dummy.Collection.AddNew().Z0_Code = "XXX";
			foreach (DummyChildBusinessObject childBusinessObject in dummy.Collection)
			{
				childBusinessObject.Z0_Guid = dummy.PK;
			}
			dummy.Factory.Save();
			AssertPrecalculateColorsForAllRowsInGrid(dummy, true, 5); // 3 hits - all 3 color filters are checked only once
			AssertPrecalculateColorsForAllRowsInGrid(dummy, false, 20); // 6x3 hits - all 3 color filters are checked for each element
		}

		void AssertPrecalculateColorsForAllRowsInGrid(DummyBusinessObject dummy, bool allowPrecalculateRows, int expectedDbHitCount)
		{
			var localDummy = new BusinessObjectFactory().Load<DummyBusinessObject>(dummy.PK);
			var colorScheme = localDummy.Factory.New<GridColourScheme>();

			var query = new ZDBOnlyQuery(typeof(DummyChildBusinessObject));
			query.AddToFilter(DummyBizoSchema.Z0_Code, "AAA");
			colorScheme.ColourStrips.Add(new GridColourStripBizoWithCustomQueryForTest(query) { BGColor = Color.Red, RuleName = "AAA" });

			query = new ZDBOnlyQuery(typeof(DummyChildBusinessObject));
			query.AddToFilter(DummyBizoSchema.Z0_Code, "BBB");
			colorScheme.ColourStrips.Add(new GridColourStripBizoWithCustomQueryForTest(query) { BGColor = Color.Green, RuleName = "BBB" });

			query = new ZDBOnlyQuery(typeof(DummyChildBusinessObject));
			query.AddToFilter(DummyBizoSchema.Z0_Code, "CCC");
			colorScheme.ColourStrips.Add(new GridColourStripBizoWithCustomQueryForTest(query) { BGColor = Color.Blue, RuleName = "CCC" });

			localDummy.Collection.Load(new ZQuery(DummyBizoSchema.Z0_Guid, localDummy.PK));
			var colors = new Color[localDummy.Collection.Count];
			for (var i = 0; i < localDummy.Collection.Count; i++)
			{
				switch (localDummy.Collection[i].Z0_Code)
				{
					case "AAA":
						colors[i] = Color.Red;
						break;
					case "BBB":
						colors[i] = Color.Green;
						break;
					case "CCC":
						colors[i] = Color.Blue;
						break;
					default:
						colors[i] = Color.Empty;
						break;
				}
			}

			using (var form = new ZForm(localDummy))
			{
				var grid = new ZGrid { BindTo = "Collection" };
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
				form.Controls.Add(grid);

				grid.SetDataBinding(localDummy, "Collection");

				grid.GridColourSchemeManagerForTest.AllowPrecalculateColorsForAllRowsInGrid = allowPrecalculateRows;
				grid.GridColourSchemeManagerForTest.colourFactory.SetLastUsedSchemeForCurrentUser(grid.GridColourSchemeManagerForTest.FilterBusinessObject, colorScheme);

				if (allowPrecalculateRows)
				{
					grid.StartLoadBackgroundColour();
					while (!grid.IsBackgroundColourLoaded)
					{
						Thread.Sleep(1000);
						Application.DoEvents();
					}
				}

				// localDummy.Factory.GetTableHitCount(DummyBizoSchema.Constants.TableName).Value = 0;

				foreach (var strip in colorScheme.ColourStrips)
				{
					Assert(strip.Filter.AddOptionRecompileConditionally);
				}

				for (var i = 0; i < localDummy.Collection.Count; i++)
				{
					var eventArgs = new ColourDecidingEventArgs(localDummy.Collection[i]) { Pk = localDummy.Collection[i].PK };
					grid.GridColourSchemeManagerForTest.grid_CustomRowBackgroundColourDeciding(grid, eventArgs);
					AssertEquals(colors[i], eventArgs.Colour);
				}

				if (!allowPrecalculateRows)
				{
					//we'd check all 3 but only first one might have applied each time
					Assert(!colorScheme.ColourStrips[0].Filter.AddOptionRecompileConditionally);
				}

				AssertEquals(expectedDbHitCount, localDummy.Factory.GetTableHitCount(DummyBizoSchema.Constants.TableName) + grid.GridColourSchemeManagerForTest.DBHitNumberForUT);
			}
		}

		public void TestHandleGridColourSchemeWithReload()
		{
			var collection = new StmALogCollection(Factory);
			var dummy = Factory.NewWithValidTestData<StmALog>();
			using (dummy.LockForUpdatingKeyFieldsForTesting())
			{
				dummy.SL_IsEstimate = false;
			}
			var dummy1 = Factory.NewWithValidTestData<StmALog>();
			using (dummy1.LockForUpdatingKeyFieldsForTesting())
			{
				dummy1.SL_IsEstimate = true;
			}
			Factory.Save();
			collection.Add(dummy);
			collection.Add(dummy1);

			using (var form = new ZForm(collection))
			using (var grid = new ZGrid())
			{
				var colorManager = new GridColourSchemeManagerForTest(grid);

				var colourScheme = dummy.Factory.New<GridColourScheme>();
				colourScheme.S9_FilterName = "ColourSchemeName";

				var dbOnlyquery = new ZDBOnlyQuery(typeof(StmALog));
				dbOnlyquery.AddToFilter(new ZQuery(StmALogSchema.PK, dummy.PK));
				colourScheme.ColourStrips.Add(new GridColourStripBizoWithCustomQueryForTest(dbOnlyquery) { BGColor = Color.Red, RuleName = "AAA" });
				var query = new ZQuery(StmALogSchema.SL_IsEstimate, true);
				colourScheme.ColourStrips.Add(new GridColourStripBizoWithCustomQueryForTest(query) { BGColor = Color.Green, RuleName = "BBB" });

				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("SL_Reference", 80));
				form.Controls.Add(grid);

				grid.SetDataBinding(collection, "");

				colorManager.SetLastUsedSchemeForCurrentUser(colourScheme);

				ZArchitecture.Testing.GridColourSchemeManagerForTest.RecreateGridColourSchemeManager(grid, colourScheme, true);
				Factory.Save();

				grid.StartLoadBackgroundColour();
				while (!grid.IsBackgroundColourLoaded)
				{
					Thread.Sleep(1000);
					Application.DoEvents();
				}

				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
				AssertEquals(0, ErrorReporter.TotalErrorCount);
				AssertEquals(grid.ColorByPK[collection[0].PK], Color.Red);
				AssertEquals(grid.ColorByPK[collection[1].PK], Color.Green);
			}
		}

		public void TestHandleGridColourSchemeErrorWithSqlExceptionSubqueryReturnedMoreThan1Value()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Collection.AddNew().Z0_Code = "HFG1";
			dummy.Collection.AddNew().Z0_Code = "HFG2";

			foreach (DummyChildBusinessObject childBusinessObject in dummy.Collection)
			{
				childBusinessObject.Z0_Guid = dummy.PK;
			}
			dummy.Factory.Save();

			var colorScheme = dummy.Factory.New<GridColourScheme>();

			colorScheme.ColourStrips.Add(new GridColourStripBizoForCustomSqlTest());

			using (var form = new ZForm(dummy))
			using (var grid = new ZGrid { BindTo = "Collection" })
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
				form.Controls.Add(grid);

				grid.SetDataBinding(dummy, "Collection");

				grid.GridColourSchemeManagerForTest.AllowPrecalculateColorsForAllRowsInGrid = true;
				grid.GridColourSchemeManagerForTest.colourFactory.SetLastUsedSchemeForCurrentUser(grid.GridColourSchemeManagerForTest.FilterBusinessObject, colorScheme);

				grid.StartLoadBackgroundColour();
				while (!grid.IsBackgroundColourLoaded)
				{
					Thread.Sleep(1000);
					Application.DoEvents();
				}

				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
				AssertEquals("Failed to apply color scheme \"\" to grid rows.\r\nRule Name: Unknown.\r\nReason: There is a custom SQL error in rule \"\", the error SQL is: Z0_Guid = (SELECT Z0_Guid FROM dbo.DummyBizo WITH (NOLOCK) WHERE Z0_Code LIKE 'HFG%').\r\n\r\nCustom color schemes were temporarily disabled for this grid, please switch to default color scheme and check the current scheme filters and settings for possible errors.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestHandleGridColourSchemeErrorWithOtherCustomSqlException()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Collection.AddNew().Z0_Code = "AAA";

			using (var form = new ZForm(dummy))
			using (var grid = new ZGrid { Name = "DummyGrid", BindTo = "Collection" })
			{
				var colourScheme = dummy.Factory.New<GridColourScheme>();
				var gcsName = "testName";
				colourScheme.S9_FilterName = new ZString(gcsName);
				colourScheme.ColourStrips.Add(new GridColourStripBizoForCustomSqlTest());

				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
				form.Controls.Add(grid);

				grid.SetDataBinding(dummy, "Collection");
				grid.GridColourSchemeManagerForTest.colourFactory.SetLastUsedSchemeForCurrentUser(grid.GridColourSchemeManagerForTest.FilterBusinessObject, colourScheme);

				var eventArgs = new ColourDecidingEventArgs(dummy.Collection[0]);
				grid.GridColourSchemeManagerForTest.grid_CustomRowBackgroundColourDeciding(grid, eventArgs);

				var ex = SqlExceptionBuilder.CreateSqlException(208, "Invalid object name 'dummy'");
				var reason = "test reason";
				var reportOnceKey = "ROK";

				grid.GridColourSchemeManagerForTest.HandleGridColourSchemeError(ex, eventArgs, reason, reason, reportOnceKey);
				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
				AssertEquals("Failed to apply color scheme \"testName\" to grid rows.\r\nRule Name: Unknown.\r\nReason: There is a custom SQL error in rule \"\", the error SQL is: Z0_Guid = (SELECT Z0_Guid FROM dbo.DummyBizo WITH (NOLOCK) WHERE Z0_Code LIKE 'HFG%').\r\n\r\nCustom color schemes were temporarily disabled for this grid, please switch to default color scheme and check the current scheme filters and settings for possible errors.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestHandleGridBackgroundColourCanHandleDbUpgradeExceptionThrown()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Collection.AddNew().Z0_Code = "AAA";

			using (var form = new ZForm(dummy))
			using (var grid = new ZGrid { Name = "DummyGrid", BindTo = "Collection" })
			{
				var colourScheme = dummy.Factory.New<GridColourScheme>();
				var gcsName = "testName";
				colourScheme.S9_FilterName = new ZString(gcsName);
				colourScheme.ColourStrips.Add(new GridColourStripBizoForCustomSqlTest());

				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
				form.Controls.Add(grid);
				grid.SetDataBinding(dummy, "Collection");

				AssertNoExceptionThrown(
					"DatabaseUpgradeInProgressException is swallowed in HandleExceptionWhenDecidingBackgroundColour",
					() => grid.GridColourSchemeManagerForTest.HandleExceptionWhenDecidingBackgroundColour(new DatabaseUpgradeInProgressException(), new ColourDecidingEventArgs(null))
				);
			}
		}

#if !WINZOR
		public void TestHandleGridColourSchemeError_QueryProcessorCouldNotProduceQueryPlanBecauseMinimumWorktableWasTooLong()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Collection.AddNew().Z0_Code = "AAA";

			using (var form = new ZForm(dummy))
			using (var grid = new ZGrid { Name = "DummyGrid", BindTo = "Collection" })
			{
				var colourScheme = dummy.Factory.New<GridColourScheme>();
				var gcsName = "testName";
				colourScheme.S9_FilterName = new ZString(gcsName);

				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 80));
				form.Controls.Add(grid);

				grid.SetDataBinding(dummy, "Collection");
				grid.GridColourSchemeManagerForTest.colourFactory.SetLastUsedSchemeForCurrentUser(grid.GridColourSchemeManagerForTest.FilterBusinessObject, colourScheme);

				var eventArgs = new ColourDecidingEventArgs(dummy.Collection[0]);
				grid.GridColourSchemeManagerForTest.grid_CustomRowBackgroundColourDeciding(grid, eventArgs);

				var ex = SqlExceptionBuilder.CreateSqlException(8618, "The query processor could not produce a query plan because a worktable is required, and its minimum row size exceeds the maximum allowable of 8060 bytes.");
				var reason = "test reason";
				var reportOnceKey = "ROK";

				grid.GridColourSchemeManagerForTest.HandleGridColourSchemeError(ex, eventArgs, reason, reason, reportOnceKey);
				AssertEquals(null, ErrorReporter.LastExceptionReported);
			}
		}
#endif

		protected override void SetUp()
		{
			base.SetUp();

			SystemDataRegistry.Instance.QueryTimeoutForColourSchemeManager.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 60);
		}
	}
	#endregion
}
