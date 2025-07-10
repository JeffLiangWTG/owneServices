using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Moq;
using static Enterprise.Customs.Business.AutoCusSupportingInfo.Schema;
using static Enterprise.Customs.ExitControlBase.Business.AutoCusExitReportItem.Schema;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	sealed class ReportItemsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(ExitControlBase.Business.ICusExitReportCollection<CusExitReport>), userControl.BindingSource.DataSourceType);
		}

		public void TestReportItemsGroupBox()
		{
			var reportItemsGroupBox = userControl.ReportItemsGroupBox;
			CombineAssertions(() =>
			{
				AssertEquals("ReportItemsGrid is within ReportItemsGroupBox", true, reportItemsGroupBox.Controls.Contains(reportItemsGrid));
				AssertEquals("Caption", "Items", reportItemsGroupBox.CaptionResourceString.Caption);
				AssertEquals("Dock", DockStyle.Left, reportItemsGroupBox.Dock);
			});
		}

		public void TestReportItemsGrid_AvailableColumns()
		{
			AssertSequencesEqual("Columns",
				new[] { nameof(CusExitReportItem.ConsignmentItemLineNumber), ERI_GrossMass, ERI_NetMass, nameof(CusExitReportItem.ConsignmentItemUniqueConsignmentReference) },
				reportItemsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		public void TestReportItemsGridColumn_ConsignmentItemLineNumber()
		{
			var columnInfo = reportItemsGrid.GetColumnStyle(nameof(CusExitReportItem.ConsignmentItemLineNumber));
			AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80), columnInfo.Width);
		}

		public void TestReportItemsGridColumn_ERI_GrossMass()
		{
			var columnInfo = reportItemsGrid.GetColumnStyle(ERI_GrossMass);
			AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100), columnInfo.Width);
		}

		public void TestReportItemsGridColumn_ERI_NetMass()
		{
			var columnInfo = reportItemsGrid.GetColumnStyle(ERI_NetMass);
			AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100), columnInfo.Width);
		}

		public void TestReportItemsGridColumn_ConsignmentItemUniqueConsignmentReference()
		{
			CombineAssertions(() =>
			{
				var columnInfo = reportItemsGrid.GetColumnStyle(nameof(CusExitReportItem.ConsignmentItemUniqueConsignmentReference));
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140), columnInfo.Width);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
			});
		}

		public void TestReportItemsGrid_ListManager_CurrentChanged()
		{
			var exitHeader = CreateCusExitHeaderForTest();
			var exitControlLayoutProviders = new KeyObjectHandleDictionaryObject
			{
				{ "Default", new TestObjectHandle(new ExitControlLayoutProviderForReportItemsTest()) }
			};

			using (ObjectFactory.Substitute("ExitControlLayoutProviders", exitControlLayoutProviders))
			using (var form = new ExitControlForm(exitHeader))
			{
				form.Show();

				var exitControlUserControl = form.ExitControlUserControl;
				var reportsTabPage = form.ExitControlUserControl.ReportsTabPage;
				exitControlUserControl.ExitControlTabControl.SelectedTab = reportsTabPage;

				var reportItemsUserControl = reportsTabPage.FindSingle<ReportItemsUserControl>(nameof(ReportItemsUserControl));
				var reportItemsGrid = reportItemsUserControl.ReportItemsGrid;
				var reportItemPackagesGrid = reportItemsUserControl.ReportItemPackagesGrid;
				CombineAssertions(() =>
				{
					reportItemsUserControl.IsPackageRelatedToConsignmentItemCheckBox.Checked = false;
					reportItemsGrid.ListManager.Position = 0;
					reportItemPackagesGrid.SelectAllElements();
					AssertEquals("UnChecked, select first item", 3, reportItemPackagesGrid.SelectedRowCount);

					reportItemsGrid.ListManager.Position = 1;
					reportItemPackagesGrid.SelectAllElements();
					AssertEquals("UnChecked, select second item", 3, reportItemPackagesGrid.SelectedRowCount);

					reportItemsUserControl.IsPackageRelatedToConsignmentItemCheckBox.Checked = true;
					reportItemsGrid.ListManager.Position = 0;
					reportItemPackagesGrid.SelectAllElements();
					AssertEquals("Checked, select first item", 2, reportItemPackagesGrid.SelectedRowCount);

					reportItemsGrid.ListManager.Position = 1;
					reportItemPackagesGrid.SelectAllElements();
					AssertEquals("Checked, select second item", 1, reportItemPackagesGrid.SelectedRowCount);
				});
			}
		}

		public void TestOnExitReportChanged()
		{
			var exitHeader = CreateCusExitHeaderForTest();
			var exitControlLayoutProviders = new KeyObjectHandleDictionaryObject
			{
				{ "Default", new TestObjectHandle(new ExitControlLayoutProviderForReportItemsTest()) }
			};

			using (ObjectFactory.Substitute("ExitControlLayoutProviders", exitControlLayoutProviders))
			using (var form = new ExitControlForm(exitHeader))
			{
				form.Show();

				var exitControlUserControl = form.ExitControlUserControl;
				var reportsTabPage = form.ExitControlUserControl.ReportsTabPage;
				exitControlUserControl.ExitControlTabControl.SelectedTab = reportsTabPage;

				var reportItemsUserControl = reportsTabPage.FindSingle<ReportItemsUserControl>(nameof(ReportItemsUserControl));
				var reportItemAdditionalInfosSplitContainer = reportItemsUserControl.PackagesAndAdditionalDocumentSplitContainer;
				CombineAssertions(() =>
				{
					var exitReport = Factory.New<CusExitReport>();
					exitReport.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport;
					((IReportItemsUserControl)reportItemsUserControl).OnExitReportChanged(exitReport);
					AssertEquals("Additional Infos Panel NOT collapsed for CER_Type = 'ALT", true, reportItemAdditionalInfosSplitContainer.Panel2Collapsed);

					exitReport.CER_Type = ExitReportTypeList.Codes.ExitNotification;
					((IReportItemsUserControl)reportItemsUserControl).OnExitReportChanged(exitReport);
					AssertEquals("Additional Infos Panel collapsed for CER_Type = 'EXT", true, reportItemAdditionalInfosSplitContainer.Panel2Collapsed);

					exitReport.CER_Type = ExitReportTypeList.Codes.Presentation;
					((IReportItemsUserControl)reportItemsUserControl).OnExitReportChanged(exitReport);
					AssertEquals("Additional Infos Panel NOT collapsed for CER_Type = 'PRE", false, reportItemAdditionalInfosSplitContainer.Panel2Collapsed);
				});
			}
		}

		public void TestPackagesAndAdditionalDocumentSplitContainer()
		{
			var exitHeader = CreateCusExitHeaderForTest();
			var exitControlLayoutProviders = new KeyObjectHandleDictionaryObject
			{
				{ "Default", new TestObjectHandle(new ExitControlLayoutProviderForReportItemsTest()) }
			};

			using (ObjectFactory.Substitute("ExitControlLayoutProviders", exitControlLayoutProviders))
			using (var form = new ExitControlForm(exitHeader))
			{
				form.Show();

				var exitControlUserControl = form.ExitControlUserControl;
				var reportsTabPage = form.ExitControlUserControl.ReportsTabPage;
				exitControlUserControl.ExitControlTabControl.SelectedTab = reportsTabPage;

				var reportItemsUserControl = reportsTabPage.FindSingle<ReportItemsUserControl>(nameof(ReportItemsUserControl));
				var reportItemsGrid = reportItemsUserControl.ReportItemsGrid;
				var packagesAndAdditionalDocumentSplitContainer = reportItemsUserControl.PackagesAndAdditionalDocumentSplitContainer;
				CombineAssertions(() =>
				{
					AssertEquals("Dock", DockStyle.Fill, packagesAndAdditionalDocumentSplitContainer.Dock);
					AssertEquals("Orientation", Orientation.Horizontal, packagesAndAdditionalDocumentSplitContainer.Orientation);
				});
			}
		}

		public void TestReportItemPackagesGroupBox()
		{
			var reportItemPackagesGroupBox = userControl.ReportItemPackagesGroupBox;
			CombineAssertions(() =>
			{
				AssertEquals("ReportItemPackagesGrid is within ReportItemPackagesGroupBox", true, reportItemPackagesGroupBox.Controls.Contains(reportItemPackagesGrid));
				AssertEquals("Caption", "Packages", reportItemPackagesGroupBox.CaptionResourceString.Caption);
				AssertEquals("Dock", DockStyle.Fill, reportItemPackagesGroupBox.Dock);
			});
		}

		public void TestReportItemPackagesGrid()
		{
			AssertSequencesEqual("Columns",
				[nameof(CusExitReportItem.SeqNo), nameof(CusExitReportItem.ConsignmentItemLineNumber), ERI_Quantity, nameof(CusExitReportItem.PackageType), nameof(CusExitReportItem.PackageMarksAndNumbers), nameof(CusExitReportItem.PackageContainerOrEquipment)],
				reportItemPackagesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		public void TestReportItemPackagesGridColumn_ConsignmentItemLineNumber()
		{
			var columnInfo = reportItemPackagesGrid.GetColumnStyle(nameof(CusExitReportItem.ConsignmentItemLineNumber));
			AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80), columnInfo.Width);
		}

		public void TestReportItemPackagesGridColumn_ERI_Quantity()
		{
			var columnInfo = reportItemPackagesGrid.GetColumnStyle(ERI_Quantity);
			AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100), columnInfo.Width);
		}

		public void TestReportItemPackagesGridColumn_PackageType()
		{
			var columnInfo = reportItemPackagesGrid.GetColumnStyle(nameof(CusExitReportItem.PackageType));
			AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100), columnInfo.Width);
		}

		public void TestReportItemPackagesGridColumn_PackageSeqNo()
		{
			var columnInfo = reportItemPackagesGrid.GetColumnStyle(nameof(CusExitReportItem.SeqNo));
			AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80), columnInfo.Width);
		}

		public void TestReportItemPackagesGridColumn_PackageMarksAndNumbers()
		{
			var columnInfo = reportItemPackagesGrid.GetColumnStyle(nameof(CusExitReportItem.PackageMarksAndNumbers));
			AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140), columnInfo.Width);
		}

		public void TestReportItemPackagesGridColumn_PackageContainerOrEquipment()
		{
			var columnInfo = reportItemPackagesGrid.GetColumnStyle(nameof(CusExitReportItem.PackageContainerOrEquipment));
			AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140), columnInfo.Width);
		}

		public void TestBottomPanel()
		{
			AssertEquals(DockStyle.Bottom, userControl.BottomPanel.Dock);
		}

		public void TestIsPackageRelatedToConsignmentItemCheckBox()
		{
			var isPackageRelatedToConsignmentItemCheckBox = userControl.IsPackageRelatedToConsignmentItemCheckBox;
			CombineAssertions(() =>
			{
				AssertEquals("IsPackageRelatedToConsignmentItemCheckBox is within BottomPanel", true, userControl.BottomPanel.Controls.Contains(isPackageRelatedToConsignmentItemCheckBox));
				AssertEquals("BindTo", "IsPackageRelatedToConsignmentItem", isPackageRelatedToConsignmentItemCheckBox.BindTo);
			});
		}

		public void TestIsPackageRelatedToConsignmentItemCheckBox_Checked()
		{
			var exitHeader = CreateCusExitHeaderForTest();
			var exitControlLayoutProviders = new KeyObjectHandleDictionaryObject
			{
				{ "Default", new TestObjectHandle(new ExitControlLayoutProviderForReportItemsTest()) }
			};

			using (ObjectFactory.Substitute("ExitControlLayoutProviders", exitControlLayoutProviders))
			using (var form = new ExitControlForm(exitHeader))
			{
				form.Show();

				var exitControlUserControl = form.ExitControlUserControl;
				var reportsTabPage = form.ExitControlUserControl.ReportsTabPage;
				exitControlUserControl.ExitControlTabControl.SelectedTab = reportsTabPage;

				var reportItemsUserControl = reportsTabPage.FindSingle<ReportItemsUserControl>(nameof(ReportItemsUserControl));
				var reportItemsGrid = reportItemsUserControl.ReportItemsGrid;
				var reportItemPackagesGrid = reportItemsUserControl.ReportItemPackagesGrid;
				CombineAssertions(() =>
				{
					reportItemsGrid.ListManager.Position = 0;
					reportItemPackagesGrid.SelectAllElements();
					AssertEquals("Default count", 2, reportItemPackagesGrid.SelectedRowCount);

					reportItemsUserControl.IsPackageRelatedToConsignmentItemCheckBox.Checked = true;
					reportItemPackagesGrid.SelectAllElements();
					AssertEquals("Checked", 2, reportItemPackagesGrid.SelectedRowCount);

					reportItemsUserControl.IsPackageRelatedToConsignmentItemCheckBox.Checked = false;
					reportItemPackagesGrid.SelectAllElements();
					AssertEquals("UnChecked", 3, reportItemPackagesGrid.SelectedRowCount);
				});
			}
		}

		public void TestAdditionalPanelsSplitContainer()
		{
			var exitHeader = CreateCusExitHeaderForTest();
			var exitControlLayoutProviders = new KeyObjectHandleDictionaryObject
			{
				{ "Default", new TestObjectHandle(new ExitControlLayoutProviderForReportItemsTest()) }
			};

			using (ObjectFactory.Substitute("ExitControlLayoutProviders", exitControlLayoutProviders))
			using (var form = new ExitControlForm(exitHeader))
			{
				form.Show();

				var exitControlUserControl = form.ExitControlUserControl;
				var reportsTabPage = form.ExitControlUserControl.ReportsTabPage;
				exitControlUserControl.ExitControlTabControl.SelectedTab = reportsTabPage;

				var reportItemsUserControl = reportsTabPage.FindSingle<ReportItemsUserControl>(nameof(ReportItemsUserControl));
				var reportItemsGrid = reportItemsUserControl.ReportItemsGrid;
				var additionalPanelsSplitContainer = reportItemsUserControl.AdditionalPanelsSplitContainer;
				CombineAssertions(() =>
				{
					AssertEquals("Dock", DockStyle.Fill, additionalPanelsSplitContainer.Dock);
					AssertEquals("Orientation", Orientation.Horizontal, additionalPanelsSplitContainer.Orientation);
				});
			}
		}

		public void TestAdditionalPanelsSplitContainer_Collapsed()
		{
			var mockExitControlLayoutProvider = new Mock<IExitControlLayoutProvider>();
			var exitControlLayoutProviders = new KeyObjectHandleDictionaryObject
			{
				{ Core.Constants.CountryCodes.Lithuania, new TestObjectHandle(mockExitControlLayoutProvider.Object) }
			};

			using (ObjectFactory.Substitute("ExitControlLayoutProviders", exitControlLayoutProviders))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Lithuania))
			{
				var exitHeader = CreateCusExitHeaderForTest();
				CombineAssertions(() =>
				{
					using (var control = new ReportItemsUserControl())
					{
						mockExitControlLayoutProvider.SetupGet(p => p.AuthorizationIsActive).Returns(false);
						control.SetDataBinding(exitHeader, "");
						Assert("Authorization is inactive: panel should be collapsed.", control.AdditionalPanelsSplitContainer.Panel1Collapsed);
					}
					using (var control = new ReportItemsUserControl())
					{
						mockExitControlLayoutProvider.SetupGet(p => p.AuthorizationIsActive).Returns(true);
						control.SetDataBinding(exitHeader, "");
						Assert("Authorization is active: panel should be expanded.", !control.AdditionalPanelsSplitContainer.Panel1Collapsed);
					}
				});
			}
		}

		public void TestReportItemAuthorizationsGroupBox()
		{
			var reportItemAuthorizationsGroupBox = userControl.ReportItemAuthorizationsGroupBox;
			AssertEquals("Caption", "Authorizations", reportItemAuthorizationsGroupBox.CaptionResourceString.Caption);
		}

		public void TestAuthorizationGridColumns()
		{
			var exitHeader = CreateCusExitHeaderForTest();

			using (var form = new ExitControlForm(exitHeader))
			{
				userControl.SetDataBinding(exitHeader, "");
				form.Show();

				AssertSequencesEqual("Columns",
					[CusAuthorizationUsage.Schema.AGC_Code, nameof(CusAuthorizationUsage.CustomsCode), nameof(CusAuthorizationUsage.EffectiveReferenceNumber), CusAuthorizationUsage.Schema.AGC_OH_Owner],
					authorizationsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
			}
		}

		public void TestAuthorizationGriColumn_AGC_Code()
		{
			userControl.SetDataBinding(CreateCusExitHeaderForTest(), "");
			var columnInfo = authorizationsGrid.GetColumnStyle(CusAuthorizationUsage.Schema.AGC_Code);
			AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80), columnInfo.Width);
		}

		public void TestAuthorizationGriColumn_CustomsCode()
		{
			userControl.SetDataBinding(CreateCusExitHeaderForTest(), "");
			var columnInfo = authorizationsGrid.GetColumnStyle(nameof(CusAuthorizationUsage.CustomsCode));
			AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80), columnInfo.Width);
		}

		public void TestAuthorizationGriColumn_EffectiveReferenceNumber()
		{
			userControl.SetDataBinding(CreateCusExitHeaderForTest(), "");
			var columnInfo = authorizationsGrid.GetColumnStyle(nameof(CusAuthorizationUsage.EffectiveReferenceNumber));
			AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80), columnInfo.Width);
		}

		public void TestAuthorizationGriColumn_AGC_OH_Owner()
		{
			userControl.SetDataBinding(CreateCusExitHeaderForTest(), "");
			var columnInfo = authorizationsGrid.GetColumnStyle(CusAuthorizationUsage.Schema.AGC_OH_Owner);
			AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80), columnInfo.Width);
		}

		public void TestAuthorizationsGrid_ColumnsInitializedUsingConfiguredExitControlLayoutProvider()
		{
			const string columnName = "AGC_Code";
			var mockGridColumnLayout = Mock.Of<IGridColumnLayout>(
				c => c.Columns == new[]
				{
					new ZTextBoxColumnStyleInfo(columnName, 80)
				});
			var mockAuthorizationsGridLayout = Mock.Of<IGridColumnLayoutProvider>(p => p.Layout == mockGridColumnLayout);
			var mockExitControlLayoutProvider = Mock.Of<IExitControlLayoutProvider>(
				p => p.AuthorizationGridColumnLayout == mockAuthorizationsGridLayout);

			var exitControlLayoutProviders = new KeyObjectHandleDictionaryObject
			{
				{ Core.Constants.CountryCodes.Lithuania, new TestObjectHandle(mockExitControlLayoutProvider) }
			};

			using (ObjectFactory.Substitute("ExitControlLayoutProviders", exitControlLayoutProviders))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Lithuania))
			{
				var exitHeader = CreateCusExitHeaderForTest();
				using (var control = new ReportItemsUserControl())
				{
					control.SetDataBinding(exitHeader, "");
					var authorizationsGrid = control.AuthorizationsGrid;
					AssertEquals("ColumnStyles Count", 1, authorizationsGrid.ColumnStyles.Count);

					var columnInfo = authorizationsGrid.GetColumnStyle(columnName);

					AssertNotNull("Column Style Info", columnInfo);
					CombineAssertions("Column Info", () =>
					{
						AssertEquals("Column Name", columnName, columnInfo.ColumnName);
						AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80), columnInfo.Width);
					});
				}
			}
		}

		public void TestReportItemAdditionalInfosGroupBox()
		{
			var reportItemAdditionalInfosGroupBox = userControl.ReportItemAdditionalInfosGroupBox;
			AssertEquals("Caption", "Additional Document", reportItemAdditionalInfosGroupBox.CaptionResourceString.Caption);
		}

		public void TestAdditionalDocumentGrid_NoExtraColumns()
		{
			var exitHeader = CreateCusExitHeaderForTest();

			using (var form = new ExitControlForm(exitHeader))
			{
				userControl.SetDataBinding(exitHeader, "");
				form.Show();

				AssertSequencesEqual("Columns", [CSI_SubType, CSI_Code, CSI_ReferenceNumber], additionalDocumentGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
			}
		}

		public void TestAdditionalDocumentGridColumn_CSI_SubType()
		{
			userControl.SetDataBinding(CreateCusExitHeaderForTest(), "");
			var columnInfo = additionalDocumentGrid.GetColumnStyle(CSI_SubType);
			AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80), columnInfo.Width);
		}

		public void TestAdditionalDocumentGridColumn_CSI_Code()
		{
			userControl.SetDataBinding(CreateCusExitHeaderForTest(), "");
			var columnInfo = additionalDocumentGrid.GetColumnStyle(CSI_Code);
			AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80), columnInfo.Width);
		}

		public void TestAdditionalDocumentGridColumn_CSI_ReferenceNumber()
		{
			userControl.SetDataBinding(CreateCusExitHeaderForTest(), "");
			var columnInfo = additionalDocumentGrid.GetColumnStyle(CSI_ReferenceNumber);
			AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(131), columnInfo.Width);
		}

		public void TestAdditionalDocumentsGrid_ColumnsInitializedUsingConfiguredExitControlLayoutProvider()
		{
			const string columnName = "CSI_ReferenceNumber";
			var mockGridColumnLayout = Mock.Of<IGridColumnLayout>(
				c => c.Columns == new[]
				{
					new ZTextBoxColumnStyleInfo(columnName, 150)
				});
			var mockAdditionalDocumentsGridLayout = Mock.Of<IGridColumnLayoutProvider>(p => p.Layout == mockGridColumnLayout);
			var mockExitControlLayoutProvider = Mock.Of<IExitControlLayoutProvider>(
				p => p.ReportItemAdditionalDocumentsGridLayout == mockAdditionalDocumentsGridLayout);

			var exitControlLayoutProviders = new KeyObjectHandleDictionaryObject
			{
				{ "Default", new TestObjectHandle(new ExitControlLayoutProviderForReportItemsTest()) },
				{ Core.Constants.CountryCodes.Italy, new TestObjectHandle(mockExitControlLayoutProvider) }
			};

			using (ObjectFactory.Substitute("ExitControlLayoutProviders", exitControlLayoutProviders))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var exitHeader = CreateCusExitHeaderForTest();
				using (var control = new ReportItemsUserControl())
				{
					control.SetDataBinding(exitHeader, "");
					var additionalInfGrid = control.AdditionalDocumentGrid;
					AssertEquals("ColumnStyles Count", 1, additionalInfGrid.ColumnStyles.Count);

					var columnInfo = additionalInfGrid.GetColumnStyle(columnName);

					AssertNotNull("Column Style Info", columnInfo);
					CombineAssertions("Column Info", () =>
					{
						AssertEquals("Column Name", columnName, columnInfo.ColumnName);
						AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150), columnInfo.Width);
					});
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new ReportItemsUserControl();
			reportItemsGrid = userControl.ReportItemsGrid;
			reportItemPackagesGrid = userControl.ReportItemPackagesGrid;
			authorizationsGrid = userControl.AuthorizationsGrid;
			additionalDocumentGrid = userControl.AdditionalDocumentGrid;
		}

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}

		ReportItemsUserControl userControl;
		ZGrid reportItemsGrid;
		ZGrid reportItemPackagesGrid;
		ZGrid authorizationsGrid;
		ZGrid additionalDocumentGrid;

		CusExitHeader CreateCusExitHeaderForTest()
		{
			var exitHeader = Factory.New<CusExitHeader>();
			var exitConsignment = exitHeader.CusExitConsignments.AddNew();
			var exitConsignmentItem = exitConsignment.CusExitConsignmentItems.AddNew();
			var packagePivot = exitConsignmentItem.CusExitConsignmentPackagePivots.AddNew();
			var packagePivot2 = exitConsignmentItem.CusExitConsignmentPackagePivots.AddNew();
			var exitConsignmentItem2 = exitConsignment.CusExitConsignmentItems.AddNew();
			var packagePivot3 = exitConsignmentItem2.CusExitConsignmentPackagePivots.AddNew();

			var exitReport = exitHeader.CusExitReports.AddNew();
			var exitReportItem = exitReport.CusExitReportItems.AddNew();
			exitReportItem.ERI_CCI_ConsignmentItem = exitConsignmentItem.PK;
			exitReportItem.ERI_CXP_Package = packagePivot.Package.PK;

			var exitReportItem2 = exitReport.CusExitReportItems.AddNew();
			exitReportItem2.ERI_CCI_ConsignmentItem = exitConsignmentItem.PK;
			exitReportItem2.ERI_CXP_Package = packagePivot2.Package.PK;

			var exitReportItem3 = exitReport.CusExitReportItems.AddNew();
			exitReportItem3.ERI_CCI_ConsignmentItem = exitConsignmentItem2.PK;
			exitReportItem3.ERI_CXP_Package = packagePivot3.Package.PK;
			return exitHeader;
		}
	}

	sealed class ExitControlLayoutProviderForReportItemsTest : ExitControlLayoutProvider, IExitControlLayoutProvider
	{
		IReportItemsUserControl IExitControlLayoutProvider.CreateReportItemsUserControl() => new ReportItemsUserControl();
	}
}
