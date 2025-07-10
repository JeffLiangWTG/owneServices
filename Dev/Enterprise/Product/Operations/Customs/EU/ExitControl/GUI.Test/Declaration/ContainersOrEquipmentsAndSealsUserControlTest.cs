using System.Linq;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Moq;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	class ContainersOrEquipmentsAndSealsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(CusExitHeader), userControl.BindingSource.DataSourceType);
		}

		public void TestContainersOrEquipmentsGrid_NoExtraColumns()
		{
			var exitHeader = Factory.New<CusExitHeader>();
			userControl.SetDataBinding(exitHeader, "");

			var containersOrEquipmentsGroupBox = userControl.FindSingle<ZGroupBox>("ContainersOrEquipmentsGroupBox");
			var containersOrEquipmentsGrid = containersOrEquipmentsGroupBox.FindSingle<ZGrid>();
			CombineAssertions(() =>
			{
				AssertEquals("containersOrEquipmentsGrid.ColumnStyles.Count", 2, containersOrEquipmentsGrid.ColumnStyles.Count);
				var containerNumberColumnStyle = containersOrEquipmentsGrid.GetColumnStyle(nameof(CusExitContainer.CXN_ContainerNumber));
				AssertType<ZTextBoxColumnStyleInfo>("containerNumberColumnStyle", containerNumberColumnStyle);
				AssertEquals("containerNumberColumnStyle.CharacterCasing", System.Windows.Forms.CharacterCasing.Upper, containerNumberColumnStyle.CharacterCasing);
				var isEquipmentColumnStyle = containersOrEquipmentsGrid.GetColumnStyle(nameof(CusExitContainer.CXN_IsEquipment));
				AssertType<ZCheckBoxColumnStyleInfo>("isEquipmentColumnStyle", isEquipmentColumnStyle);
			});
		}

		public void TestContainersOrEquipmentsGrid_Columns()
		{
			var provider = new ExitControlLayoutProvider();

			var exitControlLayoutProviders = new KeyObjectHandleDictionaryObject { { "Default", new TestObjectHandle(provider) } };

			using (ObjectFactory.Substitute("ExitControlLayoutProviders", exitControlLayoutProviders))
			{
				var exitHeader = Factory.New<CusExitHeader>();
				userControl.SetDataBinding(exitHeader, "");

				var containersOrEquipmentsGroupBox = userControl.FindSingle<ZGroupBox>("ContainersOrEquipmentsGroupBox");
				var containersOrEquipmentsGrid = containersOrEquipmentsGroupBox.FindSingle<ZGrid>();
				CombineAssertions(() =>
				{
					AssertEquals("containersOrEquipmentsGrid.ColumnStyles.Count", 2, containersOrEquipmentsGrid.ColumnStyles.Count);
					AssertSequencesEqual("Columns", new string[] { nameof(CusExitContainer.CXN_ContainerNumber), nameof(CusExitContainer.CXN_IsEquipment) }, containersOrEquipmentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));

					var containerNumberColumnStyle = containersOrEquipmentsGrid.GetColumnStyle(nameof(CusExitContainer.CXN_ContainerNumber));
					AssertType<ZTextBoxColumnStyleInfo>("containerNumberColumnStyle", containerNumberColumnStyle);
					AssertEquals("containerNumberColumnStyle.CharacterCasing", System.Windows.Forms.CharacterCasing.Upper, containerNumberColumnStyle.CharacterCasing);
					var isEquipmentColumnStyle = containersOrEquipmentsGrid.GetColumnStyle(nameof(CusExitContainer.CXN_IsEquipment));
					AssertType<ZCheckBoxColumnStyleInfo>("isEquipmentColumnStyle", isEquipmentColumnStyle);
				});
			}
		}

		public void TestSealsGrid_NoExtraColumns()
		{
			var exitHeader = Factory.New<CusExitHeader>();
			userControl.SetDataBinding(exitHeader, "");

			var sealsGroupBox = userControl.FindSingle<ZGroupBox>("SealsGroupBox");
			var sealsGrid = sealsGroupBox.FindSingle<ZGrid>();
			CombineAssertions(() =>
			{
				AssertEquals("sealsGrid.ColumnStyles.Count", 1, sealsGrid.ColumnStyles.Count);
				var sealNumberColumnStyle = sealsGrid.GetColumnStyle(nameof(CusExitSeal.BK_SealNumber));
				AssertType<ZTextBoxColumnStyleInfo>("sealNumberColumnStyle", sealNumberColumnStyle);
				AssertEquals("sealNumberColumnStyle.CharacterCasing", System.Windows.Forms.CharacterCasing.Upper, sealNumberColumnStyle.CharacterCasing);
			});
		}

		public void TestSealsGrid_ColumnsInitializedUsingConfiguredExitControlLayoutProvider()
		{
			const string columnName = "BK_UnloadingState";
			var mockGridColumnLayout = Mock.Of<IGridColumnLayout>(
				c => c.Columns == new[]
				{
					new ZDropEditColumnStyleInfo(columnName, 150)
				});
			var mockSealsGridColumnLayout = Mock.Of<IGridColumnLayoutProvider>(p => p.Layout == mockGridColumnLayout);
			var mockExitControlLayoutProvider = Mock.Of<IExitControlLayoutProvider>(
				p => p.SealsGridLayout == mockSealsGridColumnLayout);

			var exitControlLayoutProviders = new KeyObjectHandleDictionaryObject
			{
				{ "Default", new TestObjectHandle(new ExitControlLayoutProviderForReportItemsTest()) },
				{ Core.Constants.CountryCodes.Italy, new TestObjectHandle(mockExitControlLayoutProvider) }
			};

			using (ObjectFactory.Substitute("ExitControlLayoutProviders", exitControlLayoutProviders))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var exitHeader = Factory.New<CusExitHeader>();
				using (var control = new ContainersOrEquipmentsAndSealsUserControl())
				{
					control.SetDataBinding(exitHeader, "");
					var sealsGrid = control.FindSingle<ZGrid>("SealsGrid");
					AssertEquals("ColumnStyles Count", 1, sealsGrid.ColumnStyles.Count);

					var columnInfo = sealsGrid.GetColumnStyle(columnName);

					AssertNotNull("Column Style Info", columnInfo);
					CombineAssertions("Column Info", () =>
					{
						AssertEquals("Column Name", columnName, columnInfo.ColumnName);
						AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150), columnInfo.Width);
					});
				}
			}
		}

		public void TestContainersOrEquipmentsGrid_ColumnsInitializedUsingConfiguredExitControlLayoutProvider()
		{
			const string columnName = "CXN_Status";
			var mockGridColumnLayout = Mock.Of<IGridColumnLayout>(
				c => c.Columns == new[]
				{
					new ZDropEditColumnStyleInfo(columnName, 75)
				});
			var mockContainersOrEquipmentsGridLayout = Mock.Of<IGridColumnLayoutProvider>(p => p.Layout == mockGridColumnLayout);
			var mockExitControlLayoutProvider = Mock.Of<IExitControlLayoutProvider>(
				p => p.ContainersOrEquipmentsGridLayout == mockContainersOrEquipmentsGridLayout);

			var exitControlLayoutProviders = new KeyObjectHandleDictionaryObject
			{
				{ "Default", new TestObjectHandle(new ExitControlLayoutProviderForReportItemsTest()) },
				{ Core.Constants.CountryCodes.Italy, new TestObjectHandle(mockExitControlLayoutProvider) }
			};

			using (ObjectFactory.Substitute("ExitControlLayoutProviders", exitControlLayoutProviders))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var exitHeader = Factory.New<CusExitHeader>();
				using (var control = new ContainersOrEquipmentsAndSealsUserControl())
				{
					control.SetDataBinding(exitHeader, "");
					var containersOrEquipmentsGrid = control.FindSingle<ZGrid>("ContainersOrEquipmentsGrid");
					AssertEquals("ColumnStyles Count", 1, containersOrEquipmentsGrid.ColumnStyles.Count);

					var columnInfo = containersOrEquipmentsGrid.GetColumnStyle(columnName);

					AssertNotNull("Column Style Info", columnInfo);
					CombineAssertions("Column Info", () =>
					{
						AssertEquals("Column Name", columnName, columnInfo.ColumnName);
						AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75), columnInfo.Width);
					});
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new ContainersOrEquipmentsAndSealsUserControl();
		}
		ContainersOrEquipmentsAndSealsUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
