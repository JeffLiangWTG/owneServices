using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.MasterFiles.Business.AutoCusBondDetail.Schema;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class GuaranteesUserControlTest : TestCaseWithFactory
	{
		public void TestAvailableColumns()
		{
			AssertSequencesEqual("GuaranteesUserControl available Columns", new[] { PW_BondType, PW_BondNumber, PW_Password, PW_BondAmount, PW_BondNumber2, PW_SuretyCode }, guaranteesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Where(col => !col.IsUnavailable).Select(x => x.ColumnName));
		}

		public void TestAllColumns()
		{
			AssertSequencesEqual("GuaranteesUserControl all Columns", new[] { PW_BondType, PW_BondNumber, PW_Password, PW_BondAmount, PW_RX_NKCurrency, PW_BondFiledPort, PW_BondNumber2, PW_ValidityLimitation, PW_HolderIdentification, PW_SuretyCode, PW_Override }, guaranteesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		public void TestCheckPW_OverrideVisibility_WithConfiguration()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			using (NctsConfigurationTestHelper.TemporarilySetGuaranteeOverrideSupportConfiguration(Factory, true))
			{
				AssertEquals("OverrideSupport modified", true, nctsHeader.Configuration.GuaranteeConfiguration.OverrideSupport(nctsHeader));
				userControl.SetDataBinding(nctsHeader, ZString.Empty);

				var expectedColumns = new[] { PW_BondType, PW_BondNumber, PW_Password, PW_BondAmount, PW_BondNumber2, PW_SuretyCode, PW_Override };
				AssertSequencesEqual("GuaranteesUserControl available Columns, with Override support", expectedColumns, guaranteesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Where(col => !col.IsUnavailable).Select(x => x.ColumnName));
			}
		}

		public void TestCheckPW_OverrideVisibility_withWrongBinding()
		{
			userControl.SetDataBinding("xyz", ZString.Empty);

			var expectedColumns = new[] { PW_BondType, PW_BondNumber, PW_Password, PW_BondAmount, PW_BondNumber2, PW_SuretyCode };
			AssertSequencesEqual("GuaranteesUserControl available Columns, with wrong binding", expectedColumns, guaranteesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Where(col => !col.IsUnavailable).Select(x => x.ColumnName));
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new GuaranteesUserControl();
			guaranteesGrid = userControl.FindSingle<ZGrid>("GuaranteesGrid");
		}

		ZGrid guaranteesGrid;
		GuaranteesUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
