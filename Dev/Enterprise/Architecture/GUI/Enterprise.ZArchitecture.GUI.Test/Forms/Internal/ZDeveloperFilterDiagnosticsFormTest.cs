using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.DevTools;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using GlowIndexQueryService.Business;

namespace Enterprise.ZArchitecture.GUI.Test.Forms.Internal
{
	internal class ZDeveloperFilterDiagnosticsFormTest : TestCaseWithFactory
	{
		public void TestShowXml()
		{
			const string expectedMessage =
				"Information <?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n" +
				"<ArrayOfFilterStrip xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n" +
				"  <FilterStrip />\r\n" +
				"</ArrayOfFilterStrip>\r\n" +
				"";

			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var popup = new EmbeddedModulePopup(module))
			{
				popup.Show();
				Application.DoEvents();

				using (var diagnosticsForm = new ZDeveloperFilterDiagnosticsForm((ZFilterStripControl)module.EmbeddedControl, module.GridCollection))
				{
					diagnosticsForm.DoShowXml();
				}

				AssertXMLEquals("", expectedMessage, UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestBuildSQL_Union()
		{
			using (SystemDataRegistry.Instance.UnionOrOrFilter.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var popup = new EmbeddedModulePopup(module))
			{
				popup.Show();
				Application.DoEvents();

				var filter = module.FilterBusinessObject.AddTextFilterStrip("Z0_Code", "AAA");
				filter.OrCategory = FilterOrCategory.Red;
				filter.IsActive = true;
				filter = module.FilterBusinessObject.AddTextFilterStrip("Z0_Code", "BBB");
				filter.OrCategory = FilterOrCategory.Red;
				filter.IsActive = true;
				var dateFilter = module.FilterBusinessObject.AddDateFilterStrip("Z0_Date");
				dateFilter.IsActive = true;
				dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				dateFilter.Property1 = new ZDate(2018, 1, 1);
				dateFilter.Property2 = new ZDate(2018, 1, 1);
				var unionFilter = module.FilterBusinessObject.AddFilterStrip<ModuleUnionOrOrFilter>(FilterStripBusinessObject.UnionOrOrDescription);
				unionFilter.Property0 = true;
				unionFilter.IsActive = true;
				using (var diagnosticsForm = new ZDeveloperFilterDiagnosticsForm((ZFilterStripControl)module.EmbeddedControl, module.GridCollection))
				{
					AssertEquals(@"SELECT 
Z0_PK, Z0_AddInfo, Z0_AnotherDate, Z0_AnotherDecimal, Z0_AnotherNumber,
Z0_BitFalse, Z0_BitFiltered, Z0_BitTrue, Z0_Bool, Z0_Byte,
Z0_Code, Z0_Date, Z0_DateOnly, Z0_DateTimeOffset, Z0_Decimal,
Z0_Description, Z0_FK_Code, CAST(Z0_Geography AS varbinary(max)) AS Z0_Geography, Z0_Guid, Z0_IsSystem,
Z0_IsValid, Z0_Long, Z0_Money, Z0_NAddInfo, Z0_Number,
Z0_NVarChar, case when Z0_NVarCharMax is null then null when datalength(Z0_NVarCharMax) < 1024 then Z0_NVarCharMax else char(0) + char(0) end as Z0_NVarCharMax, Z0_Short, Z0_SmallDateTime, Z0_SparseBit,
Z0_SparseByte, Z0_SparseChar, Z0_SparseDate, Z0_SparseDateTime, Z0_SparseDateTimeOffset,
Z0_SparseDecimal, Z0_SparseGuid, Z0_SparseLong, Z0_SparseMoney, Z0_SparseNumber,
Z0_SparseNVarChar, Z0_SparseShort, Z0_SparseSmallDateTime, Z0_SparseTime, case when Z0_SparseVarBinaryMax is null then null when datalength(Z0_SparseVarBinaryMax) < 1024 then Z0_SparseVarBinaryMax else 0x0000 end as Z0_SparseVarBinaryMax,
Z0_SparseVarChar, case when Z0_SparseXml is null then null when datalength(Z0_SparseXml) < 1024 then cast(Z0_SparseXml as nvarchar(max)) else '<?placeholder LazyLoading=""Yes""?>' end as Z0_SparseXml, Z0_Time, case when Z0_VarBinaryMax is null then null when datalength(Z0_VarBinaryMax) < 1024 then Z0_VarBinaryMax else 0x0000 end as Z0_VarBinaryMax, case when Z0_VarCharMax is null then null when datalength(Z0_VarCharMax) < 1024 then Z0_VarCharMax else char(0) + char(0) end as Z0_VarCharMax,
case when Z0_Xml is null then null when datalength(Z0_Xml) < 1024 then cast(Z0_Xml as nvarchar(max)) else '<?placeholder LazyLoading=""Yes""?>' end as Z0_Xml
	FROM dbo.DummyBizo
	WHERE ((Z0_Date >= '2018-01-01 00:00:00.000' and Z0_Date < '2018-01-02 00:00:00.000') and (Z0_Code like 'BBB%' AND Z0_Code >= 'BBB' AND Z0_Code <= 'BBþ'))
union
SELECT 
Z0_PK, Z0_AddInfo, Z0_AnotherDate, Z0_AnotherDecimal, Z0_AnotherNumber,
Z0_BitFalse, Z0_BitFiltered, Z0_BitTrue, Z0_Bool, Z0_Byte,
Z0_Code, Z0_Date, Z0_DateOnly, Z0_DateTimeOffset, Z0_Decimal,
Z0_Description, Z0_FK_Code, CAST(Z0_Geography AS varbinary(max)) AS Z0_Geography, Z0_Guid, Z0_IsSystem,
Z0_IsValid, Z0_Long, Z0_Money, Z0_NAddInfo, Z0_Number,
Z0_NVarChar, case when Z0_NVarCharMax is null then null when datalength(Z0_NVarCharMax) < 1024 then Z0_NVarCharMax else char(0) + char(0) end as Z0_NVarCharMax, Z0_Short, Z0_SmallDateTime, Z0_SparseBit,
Z0_SparseByte, Z0_SparseChar, Z0_SparseDate, Z0_SparseDateTime, Z0_SparseDateTimeOffset,
Z0_SparseDecimal, Z0_SparseGuid, Z0_SparseLong, Z0_SparseMoney, Z0_SparseNumber,
Z0_SparseNVarChar, Z0_SparseShort, Z0_SparseSmallDateTime, Z0_SparseTime, case when Z0_SparseVarBinaryMax is null then null when datalength(Z0_SparseVarBinaryMax) < 1024 then Z0_SparseVarBinaryMax else 0x0000 end as Z0_SparseVarBinaryMax,
Z0_SparseVarChar, case when Z0_SparseXml is null then null when datalength(Z0_SparseXml) < 1024 then cast(Z0_SparseXml as nvarchar(max)) else '<?placeholder LazyLoading=""Yes""?>' end as Z0_SparseXml, Z0_Time, case when Z0_VarBinaryMax is null then null when datalength(Z0_VarBinaryMax) < 1024 then Z0_VarBinaryMax else 0x0000 end as Z0_VarBinaryMax, case when Z0_VarCharMax is null then null when datalength(Z0_VarCharMax) < 1024 then Z0_VarCharMax else char(0) + char(0) end as Z0_VarCharMax,
case when Z0_Xml is null then null when datalength(Z0_Xml) < 1024 then cast(Z0_Xml as nvarchar(max)) else '<?placeholder LazyLoading=""Yes""?>' end as Z0_Xml
	FROM dbo.DummyBizo WHERE ((Z0_Date >= '2018-01-01 00:00:00.000' and Z0_Date < '2018-01-02 00:00:00.000') and (Z0_Code like 'AAA%' AND Z0_Code >= 'AAA' AND Z0_Code <= 'AAþ')) OPTION (RECOMPILE)",
	diagnosticsForm.DoBuildSQL(module.FilterBusinessObject.Filter));
				}
			}
		}

		public void TestBuildSQL_SuccessfulCase()
		{
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.JobShipment))
			using (var popup = new EmbeddedModulePopup(module))
			{
				popup.Show();
				Application.DoEvents();

				using (var diagnosticsForm = new ZDeveloperFilterDiagnosticsForm((ZFilterStripControl)module.EmbeddedControl, module.GridCollection))
				{
					var expectedMessageNotILocation = string.Format("SELECT *\r\nFROM {0}\r\nWHERE\r\n{1}", module.GridCollection.TableName, BusinessObject.GetActiveFilter(module.GridCollection.TypeOfElements).LiteralTextSqlFormatted);

					AssertEquals(expectedMessageNotILocation, diagnosticsForm.DoBuildSQL(new ZQuery()));
				}
			}
		}

		public void TestBuildSQL_MaximumRows()
		{
			var dummyBusinessObjectCollection = new DummyBusinessObjectCollection(Factory);
			var filterBusinessObject = new DummyFilterBusinessObject();
			using (var diagnosticsForm = new ZDeveloperFilterDiagnosticsForm(filterBusinessObject, dummyBusinessObjectCollection))
			{
				AssertEquals("SELECT TOP 10 *\r\nFROM DummyBizo", diagnosticsForm.DoBuildSQL(new ZQuery { MaximumRows = 10 }));
			}
		}

		public void TestBuildSQL_TableNameNotValid()
		{
			const string invalidTableName = "<UnknownTableName>";

			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.Location))
			using (var popup = new EmbeddedModulePopup(module))
			{
				popup.Show();
				Application.DoEvents();

				using (var diagnosticsForm = new ZDeveloperFilterDiagnosticsForm((ZFilterStripControl)module.EmbeddedControl, module.GridCollection))
				{
					var expectedMessageIsILocation = string.Format("SELECT *\r\nFROM {0}", invalidTableName);

					AssertEquals(expectedMessageIsILocation, diagnosticsForm.DoBuildSQL(new ZQuery()));
				}
			}
		}

		public void TestIndexSearchAnalyzer_WhenModuleSupportsIndexSearch_ShouldBeVisible()
		{
			var oldOperationButtonsGroupBoxHeight = 0;
			var oldClientSizeHeight = 0;
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.Location))
			using (var popup = new EmbeddedModulePopup(module))
			{
				popup.Show();
				Application.DoEvents();

				using (var diagnosticsForm = new ZDeveloperFilterDiagnosticsForm((ZFilterStripControl)module.EmbeddedControl, module.GridCollection))
				{
					diagnosticsForm.Show();
					Application.DoEvents();

					oldOperationButtonsGroupBoxHeight = diagnosticsForm.FindSingle<KGroupBox>("operationButtonsGroupBox").Height;
					oldClientSizeHeight = diagnosticsForm.ClientSize.Height;

					diagnosticsForm.Close();
				}
			}

			using (new GlowIndexQueryEngineMock())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbStaff))
			using (var popup = new EmbeddedModulePopup(module))
			{
				popup.Show();
				Application.DoEvents();

				using (var diagnosticsForm = new ZDeveloperFilterDiagnosticsForm((ZFilterStripControl)module.EmbeddedControl, module.GridCollection))
				{
					diagnosticsForm.Show();
					Application.DoEvents();

					var newOperationButtonsGroupBoxHeight = diagnosticsForm.FindSingle<KGroupBox>("operationButtonsGroupBox").Height;
					var newClientSizeHeight = diagnosticsForm.ClientSize.Height;
					var indexSearchAnalyzer = diagnosticsForm.FindSingle<KRadioButton>("indexSearchAnalyzer");

					Assert("indexSearchAnalyzer should be visible", indexSearchAnalyzer.Visible);
					AssertOperationButtons(diagnosticsForm, module.FilterBusinessObject.SearchType);
					Assert("operationButtonsGroupBox's height should be extended", newOperationButtonsGroupBoxHeight > oldOperationButtonsGroupBoxHeight);
					Assert("Client's height should be extended", newClientSizeHeight > oldClientSizeHeight);
				}
			}
		}

		public void TestIndexSearchAnalyzer_WhenModuleDoesNotHaveSearchFields_ShouldBeHidden()
		{
			var emptySearchFieldCollection = new SearchFieldCollection("IGlbStaff", Array.Empty<SearchField>());
			using (var mocker = new GlowIndexQueryEngineMock(emptySearchFieldCollection))
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbStaff))
			using (var popup = new EmbeddedModulePopup(module))
			{
				popup.Show();
				Application.DoEvents();

				using (var diagnosticsForm = new ZDeveloperFilterDiagnosticsForm((ZFilterStripControl)module.EmbeddedControl, module.GridCollection))
				{
					diagnosticsForm.Show();
					Application.DoEvents();

					var indexSearchAnalyzer = diagnosticsForm.FindSingle<KRadioButton>("indexSearchAnalyzer");
					AssertEquals("indexSearchAnalyzer should be hidden", false, indexSearchAnalyzer.Visible);
				}
			}
		}

		public void TestIndexSearchAnalyzer_WhenGlowIndexSearchAllowedForModuleIsDisabled_ShouldBeHidden()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbStaff))
			using (var popup = new EmbeddedModulePopup(module))
			{
				popup.Show();
				Application.DoEvents();

				using (var diagnosticsForm = new ZDeveloperFilterDiagnosticsForm((ZFilterStripControl)module.EmbeddedControl, module.GridCollection))
				{
					diagnosticsForm.Show();
					Application.DoEvents();

					var indexSearchAnalyzer = diagnosticsForm.FindSingle<KRadioButton>("indexSearchAnalyzer");
					AssertEquals("indexSearchAnalyzer should be hidden", false, indexSearchAnalyzer.Visible);
				}
			}
			IndexSearchFilterHelper.ResetIndexQueryEngine();
		}

		public void TestIndexSearchAnalyzer_WhenClickingShowButton_ShouldPopUpIndexSearchAnalyzerForm()
		{
			using (new GlowIndexQueryEngineMock())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbStaff))
			using (var popup = new EmbeddedModulePopup(module))
			{
				popup.Show();
				Application.DoEvents();

				using (var diagnosticsForm = new ZDeveloperFilterDiagnosticsForm((ZFilterStripControl)module.EmbeddedControl, module.GridCollection))
				{
					diagnosticsForm.Show();
					Application.DoEvents();

					var showButton = diagnosticsForm.FindSingle<KButton>("showButton");
					showButton.PerformClick();
					Application.DoEvents();

					var indexSearchAnalyzerForm = ZApplication.GetOpenForms().OfType<IndexSearchAnalyzerForm>().First();
					AssertNotNull(indexSearchAnalyzerForm);
					indexSearchAnalyzerForm.Close();
				}
			}
		}

		public void TestIndexSearchAnalyzer_WhenIndexSearchIsToggled_ShouldUpdateOperationButtons()
		{
			using (new GlowIndexQueryEngineMock())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbStaff))
			using (var popup = new EmbeddedModulePopup(module))
			{
				popup.Show();
				Application.DoEvents();

				using (var diagnosticsForm = new ZDeveloperFilterDiagnosticsForm((ZFilterStripControl)module.EmbeddedControl, module.GridCollection))
				{
					diagnosticsForm.Show();
					Application.DoEvents();

					AssertEquals(SearchType.Index, module.FilterBusinessObject.SearchType);
					AssertOperationButtons(diagnosticsForm, module.FilterBusinessObject.SearchType);

					module.ToggleIndexSearchFilterMenuItem.PerformClick();
					Application.DoEvents();

					AssertEquals(SearchType.Sql, module.FilterBusinessObject.SearchType);
					AssertOperationButtons(diagnosticsForm, module.FilterBusinessObject.SearchType);
				}
			}
		}

		void AssertOperationButtons(KForm diagnosticsForm, SearchType searchType)
		{
			var isIndexSearchType = searchType == SearchType.Index;
			var isSqlSearchType = searchType == SearchType.Sql;
			var queryAnalyzer = diagnosticsForm.FindSingle<KRadioButton>("queryAnalyzer");
			var indexSearchAnalyzer = diagnosticsForm.FindSingle<KRadioButton>("indexSearchAnalyzer");
			var generatedFilterCheckBox = diagnosticsForm.FindSingle<KCheckBox>("generatedFilterCheckBox");
			var additionalFilterCheckBox = diagnosticsForm.FindSingle<KCheckBox>("additionalFilterCheckBox");
			var relationshipFilterCheckBox = diagnosticsForm.FindSingle<KCheckBox>("relationshipFilterCheckBox");

			AssertEquals(queryAnalyzer.Enabled, isSqlSearchType);
			AssertEquals(queryAnalyzer.Checked, isSqlSearchType);
			AssertEquals(generatedFilterCheckBox.Enabled, isSqlSearchType);
			AssertEquals(additionalFilterCheckBox.Enabled, isSqlSearchType);
			AssertEquals(relationshipFilterCheckBox.Enabled, isSqlSearchType);
			AssertEquals(indexSearchAnalyzer.Enabled, isIndexSearchType);
			AssertEquals(indexSearchAnalyzer.Checked, isIndexSearchType);
		}
	}
}
