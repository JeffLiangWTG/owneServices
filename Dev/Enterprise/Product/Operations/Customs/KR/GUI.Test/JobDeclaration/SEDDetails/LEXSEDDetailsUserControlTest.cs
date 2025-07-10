using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class LEXSEDDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestSEDDetailsGroupBox_StevedoresGroupBox()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.DeclarationTabPage;

				using (var control = brokerageControl.DeclarationUserControl)
				{
					var sEDDetailsUserControl = control.FindSingle<ZUserControl>("LocalExportSEDDetailUserControl");
					var mainPanel = sEDDetailsUserControl.FindSingle<DynamicLayoutPanel>("MainPanel");
					var stevedoresGroupBox = mainPanel.FindSingle<ZGroupBox>("StevedoresGroupBox");
					var grid = stevedoresGroupBox.FindSingle<ZGrid>("StevedoresGrid");

					var index = 0;
					AssertEquals(grid.Columns[index++].ColumnName, CusPerson.Schema.CPN_PER_Person);
					AssertEquals(grid.Columns[index++].ColumnName, nameof(CusPerson.PersonBirthDate));

					AssertEquals(2, grid.ColumnStyles.Count);
					Assert(grid.ColumnStyles[0] is ZGuidFindBoxColumnStyleInfo firstColumn && !firstColumn.IsReadOnly);
					Assert(grid.ColumnStyles[1] is ZTextBoxColumnStyleInfo secondColumn && !secondColumn.IsReadOnly);
				}
			}
		}

		public void TestSEDDetailsGroupBox_OtherTransportMeansGrid()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.LocalExport;

			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
			AssertOtherTransportMeansGroupBoxVisible(declaration, true);

			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._08;
			AssertOtherTransportMeansGroupBoxVisible(declaration, false);

			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._09;
			AssertOtherTransportMeansGroupBoxVisible(declaration, true);

			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._17;
			AssertOtherTransportMeansGroupBoxVisible(declaration, true);

			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._18;
			AssertOtherTransportMeansGroupBoxVisible(declaration, false);
		}

		void AssertOtherTransportMeansGroupBoxVisible(JobDeclaration declaration, bool visible)
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.DeclarationTabPage;

				using (var control = brokerageControl.DeclarationUserControl)
				{
					var sEDDetailsUserControl = control.FindSingle<ZUserControl>("LocalExportSEDDetailUserControl");
					var mainPanel = sEDDetailsUserControl.FindSingle<DynamicLayoutPanel>("MainPanel");
					var stevedoresGroupBox = mainPanel.FindSingle<ZGroupBox>("OtherTransportMeansGroupBox");
					var grid = stevedoresGroupBox.FindSingle<ZGrid>("OtherTransportMeansGrid");

					AssertEquals(visible, grid.Visible);

					if (visible)
					{
						var index = 0;
						AssertEquals(grid.Columns[index++].ColumnName, TransportMeans.Schema.CY_Order);
						AssertEquals(grid.Columns[index++].ColumnName, TransportMeans.Schema.CY_Code);
						AssertEquals(grid.Columns[index++].ColumnName, TransportMeans.Schema.Description);
						AssertEquals(grid.Columns[index++].ColumnName, TransportMeans.Schema.CY_Data);

						AssertEquals(4, grid.ColumnStyles.Count);
						Assert(grid.ColumnStyles[0] is ZCalcEditColumnStyleInfo firstColumn && firstColumn.IsReadOnly);
						Assert(grid.ColumnStyles[1] is ZCodeFindBoxColumnStyleInfo secondColumn && !secondColumn.IsReadOnly);
						Assert(grid.ColumnStyles[2] is ZTextBoxColumnStyleInfo thirdColumn && thirdColumn.IsReadOnly);
						Assert(grid.ColumnStyles[3] is ZTextBoxColumnStyleInfo fouthColumn && !fouthColumn.IsReadOnly);
					}
				}
			}
		}
	}
}
