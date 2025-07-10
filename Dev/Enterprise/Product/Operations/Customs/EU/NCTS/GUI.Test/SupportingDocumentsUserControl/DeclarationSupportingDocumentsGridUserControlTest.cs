using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using static Enterprise.Customs.Business.AutoCusSupportingInfo.Schema;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class DeclarationSupportingDocumentsGridUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsHeader), userControl.BindingSource.DataSourceType);
			AssertEquals("MovementHeader.SupportingDocuments", supportingDocumentsGrid.GetBindingMember());
		}

		public void TestAvailableColumns()
		{
			AssertSequencesEqual("Columns", new[] { CSI_Code, CSI_ReferenceNumber, CSI_ItemNumber, CSI_ReferenceNumber2 },
					supportingDocumentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			userControl = new DeclarationSupportingDocumentsGridUserControl();
			supportingDocumentsGrid = userControl.SupportingDocumentsGrid;
			userControl.SetDataBinding(nctsHeader, "");
		}
		DeclarationSupportingDocumentsGridUserControl userControl;
		ZGrid supportingDocumentsGrid;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
