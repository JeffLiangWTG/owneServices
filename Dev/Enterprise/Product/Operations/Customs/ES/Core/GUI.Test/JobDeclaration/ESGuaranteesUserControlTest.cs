using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing
{
	class ESGuaranteesUserControlTest : TestCaseWithFactory
	{
		public void TestColumns()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var control = new ESGuaranteesUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var guaranteesGrid = (ZGrid)form.Controls.Find("GuaranteesGrid", true).FirstOrDefault();

				CombineAssertions(() =>
				{
					AssertNotNull(FindColumnByName(guaranteesGrid, ESGuarantee.Schema.PW_BondType));
					AssertNotNull(FindColumnByName(guaranteesGrid, ESGuarantee.Schema.PW_Password));
					AssertNotNull(FindColumnByName(guaranteesGrid, ESGuarantee.Schema.PW_BondNumber));
					AssertNotNull(FindColumnByName(guaranteesGrid, ESGuarantee.Schema.PW_HolderIdentification));
					AssertNotNull(FindColumnByName(guaranteesGrid, ESGuarantee.Schema.EntryInstructionID));
					AssertNotNull(FindColumnByName(guaranteesGrid, "EntryInstruction+CEI_Description"));
					AssertNotNull(FindColumnByName(guaranteesGrid, ESGuarantee.Schema.PW_BondAmount));
					AssertNotNull(FindColumnByName(guaranteesGrid, ESGuarantee.Schema.PW_RX_NKCurrency));
					AssertNotNull(FindColumnByName(guaranteesGrid, ESGuarantee.Schema.PW_BondFiledPort));
					AssertNotNull(FindColumnByName(guaranteesGrid, ESGuarantee.Schema.PW_BondNumber2));
					AssertNotNull(FindColumnByName(guaranteesGrid, ESGuarantee.Schema.PW_ValidityLimitation));

					AssertEquals("PW_BondType column is visible", true, FindColumnByName(guaranteesGrid, ESGuarantee.Schema.PW_BondType).IsVisible);
					AssertEquals("PW_Password column is visible", true, FindColumnByName(guaranteesGrid, ESGuarantee.Schema.PW_Password).IsVisible);
					AssertEquals("PW_BondNumber column is visible", true, FindColumnByName(guaranteesGrid, ESGuarantee.Schema.PW_BondNumber).IsVisible);
					AssertEquals("PW_HolderIdentification column is visible", true, FindColumnByName(guaranteesGrid, ESGuarantee.Schema.PW_HolderIdentification).IsVisible);
					AssertEquals("EntryInstructionID column is visible", true, FindColumnByName(guaranteesGrid, ESGuarantee.Schema.EntryInstructionID).IsVisible);
					AssertEquals("EntryInstruction+CEI_Description column is visible", true, FindColumnByName(guaranteesGrid, "EntryInstruction+CEI_Description").IsVisible);
					AssertEquals("PW_BondAmount column is not visible", false, FindColumnByName(guaranteesGrid, ESGuarantee.Schema.PW_BondAmount).IsVisible);
					AssertEquals("PW_RX_NKCurrency column is not visible", false, FindColumnByName(guaranteesGrid, ESGuarantee.Schema.PW_RX_NKCurrency).IsVisible);
					AssertEquals("PW_BondFiledPort column is not visible", false, FindColumnByName(guaranteesGrid, ESGuarantee.Schema.PW_BondFiledPort).IsVisible);
					AssertEquals("PW_BondNumber2 column is not visible", false, FindColumnByName(guaranteesGrid, ESGuarantee.Schema.PW_BondNumber2).IsVisible);
					AssertEquals("PW_ValidityLimitation column is not visible", false, FindColumnByName(guaranteesGrid, ESGuarantee.Schema.PW_ValidityLimitation).IsVisible);

					AssertEquals("PW_BondType column is not mandatory", false, FindColumnByName(guaranteesGrid, ESGuarantee.Schema.PW_BondType).IsMandatory);
				});
			}
		}

		ZGridColumnInfo FindColumnByName(ZGrid grid, string columnName)
			=> grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == $"{columnName}");
	}
}
