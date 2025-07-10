using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	class Phase5ArrivalSummaryDeclarationUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsHeader), control.BindingSource.DataSourceType);
		}

		public void TestSummaryTypeDropEdit()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Binding", "ESNctsHeader.CEN_SummaryType", control.SummaryTypeDropEdit.GetBindingMember());
				AssertType<ZDropEdit>("Type", control.SummaryTypeDropEdit);
			});
		}

		public void TestPreviousSummaryDeclarationTextBox()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Binding", "ESNctsHeader.CEN_PreviousSummaryDeclaration", control.PreviousSummaryDeclarationTextBox.GetBindingMember());
				AssertType<ZTextBox>("Type", control.PreviousSummaryDeclarationTextBox);
			});
		}

		public void TestG4PreviousDocumentGroupBox()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "G4", control.G4PreviousDocumentGroupBox.CaptionResourceString.Caption);
				AssertType<ZGroupBox>("Type", control.G4PreviousDocumentGroupBox);
			});
		}

		public void TestG4PreviousDocumentGrid()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Binding", "ArrivalMovementHeader.G4PreviousDocuments", control.G4PreviousDocumentGrid.GetBindingMember());
				AssertType<ZGrid>("Type", control.G4PreviousDocumentGrid);

				var columnNames = control.G4PreviousDocumentGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName);
				AssertSequencesEqual("Columns", new[] { G4PreviousDocument.Schema.CSI_ReferenceNumber }, columnNames);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new Phase5ArrivalSummaryDeclarationUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		Phase5ArrivalSummaryDeclarationUserControl control;
	}
}
