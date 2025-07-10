using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class GuaranteesUserControlTest : TestCaseWithFactory
	{
		public void TestColumns()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(FindColumnByName(guaranteesGrid, GuaranteeForDeclaration.Schema.PW_BondType));
				AssertNotNull(FindColumnByName(guaranteesGrid, GuaranteeForDeclaration.Schema.PW_Password));
				AssertNotNull(FindColumnByName(guaranteesGrid, GuaranteeForDeclaration.Schema.PW_BondNumber));
				AssertNotNull(FindColumnByName(guaranteesGrid, GuaranteeForDeclaration.Schema.PW_HolderIdentification));
				AssertNotNull(FindColumnByName(guaranteesGrid, GuaranteeForDeclaration.Schema.EntryInstructionID));
				AssertNotNull(FindColumnByName(guaranteesGrid, GuaranteeForDeclaration.Schema.PW_BondAmount));
				AssertNotNull(FindColumnByName(guaranteesGrid, GuaranteeForDeclaration.Schema.PW_RX_NKCurrency));
				AssertNotNull(FindColumnByName(guaranteesGrid, GuaranteeForDeclaration.Schema.PW_BondFiledPort));
				AssertNotNull(FindColumnByName(guaranteesGrid, GuaranteeForDeclaration.Schema.PW_BondNumber2));
				AssertNotNull(FindColumnByName(guaranteesGrid, GuaranteeForDeclaration.Schema.PW_ValidityLimitation));
			});
		}

		public void TestPW_HolderIdentification()
		{
			var holderIDStyle = guaranteesGrid.GetColumnStyle(GuaranteeForDeclaration.Schema.PW_HolderIdentification);
			Assert(holderIDStyle is ZDropEditColumnStyleInfo);
		}

		public void TestHidePW_Password()
		{
			CombineAssertions(() =>
			{
				var passwordColumn = (ZMultiControlColumnStyleInfo)guaranteesGrid.GetColumnStyle(GuaranteeForDeclaration.Schema.PW_Password);
				AssertEquals("PasswordChar", '*', passwordColumn.PasswordChar);
				AssertEquals("FieldTypeColumnName", nameof(CommonGuarantee.AccessCodeFieldType), passwordColumn.FieldTypeColumnName);
			});
		}

		ZGridColumnInfo FindColumnByName(ZGrid grid, string columnName)
			=> grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == $"{columnName}");

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new GuaranteesUserControl();
			guaranteesGrid = (ZGrid)userControl.Controls.Find("GuaranteesGrid", true).First();
		}
		GuaranteesUserControl userControl;
		ZGrid guaranteesGrid;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
