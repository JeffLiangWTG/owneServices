using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class PreviousDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestUserControlDataSourceType()
		{
			using (var control = new PreviousDocumentsUserControl())
			{
				AssertEquals(typeof(NctsDepartureCargoDesc), control.BindingSource.DataSourceType);
			}
		}

		public void TestGridColumnSizes()
		{
			using (var control = new PreviousDocumentsUserControl())
			{
				var previousDocumentsGrid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
				CombineAssertions(() =>
				{
					AssertEquals("CSI_Code", 89, previousDocumentsGrid.GetColumnStyle(NctsPreviousDocument.Schema.CSI_Code).Width);
					AssertEquals("CSI_SubType", 58, previousDocumentsGrid.GetColumnStyle(NctsPreviousDocument.Schema.CSI_SubType).Width);
					AssertEquals("CSI_ReferenceNumber", 361, previousDocumentsGrid.GetColumnStyle(NctsPreviousDocument.Schema.CSI_ReferenceNumber).Width);
				});
			}
		}

		public void TestGridColumCharcterCasing()
		{
			using (var control = new PreviousDocumentsUserControl())
			{
				var previousDocumentsGrid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
				CombineAssertions(() =>
				{
					AssertEquals("CSI_Code", CharacterCasing.Upper, previousDocumentsGrid.GetColumnStyle(NctsPreviousDocument.Schema.CSI_Code).CharacterCasing);
					AssertEquals("CSI_SubType", CharacterCasing.Upper, previousDocumentsGrid.GetColumnStyle(NctsPreviousDocument.Schema.CSI_SubType).CharacterCasing);
					AssertEquals("CSI_ReferenceNumber", CharacterCasing.Upper, previousDocumentsGrid.GetColumnStyle(NctsPreviousDocument.Schema.CSI_ReferenceNumber).CharacterCasing);
				});
			}
		}

		public void TestCSI_ReferenceNumberColumStyleType()
		{
			using (var control = new PreviousDocumentsUserControl())
			{
				var previousDocumentsGrid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
				CombineAssertions(() =>
				{
					AssertEquals("CSI_ReferenceNumber columns type should be of type ZMultiControlColumnStyle.", "Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyle", previousDocumentsGrid.GetColumnStyle(NctsPreviousDocument.Schema.CSI_ReferenceNumber).ColumnStyleType.FullName);
				});
			}
		}
	}
}
