using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.JobDeclarationForms.Testing
{
	public class GBGuaranteesUserControlTest : TestCaseWithFactory
	{
		public void TestColumns()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			using (var form = new ZForm(declaration))
			using (var control = new GBGuaranteesUserControlForTesting())
			{
				form.Controls.Add(control);
				form.Show();
				var grid = control.GuaranteesGrid_Exposed;

				AssertColumnInGrid(grid, Enterprise.Customs.EU.Business.Declaration.GuaranteeForDeclaration.Schema.PW_Password, true, typeof(ZDropEditColumnStyle), 0);
				AssertColumnInGrid(grid, Enterprise.Customs.EU.Business.Declaration.GuaranteeForDeclaration.Schema.PW_HolderIdentification, true, typeof(ZDropEditColumnStyle), 1);
				AssertColumnInGrid(grid, Enterprise.Customs.EU.Business.Declaration.GuaranteeForDeclaration.Schema.PW_BondNumber2, true, typeof(ZTextBoxColumnStyle), 2);
				AssertColumnInGrid(grid, Enterprise.Customs.EU.Business.Declaration.GuaranteeForDeclaration.Schema.PW_BondNumber, true, typeof(ZMultiControlColumnStyle), 3);

				AssertNotNull(FindColumnByName(control.GuaranteesGrid_Exposed, Enterprise.Customs.EU.Business.Declaration.GuaranteeForDeclaration.Schema.PW_Password));
				AssertNotNull(FindColumnByName(control.GuaranteesGrid_Exposed, Enterprise.Customs.EU.Business.Declaration.GuaranteeForDeclaration.Schema.PW_BondNumber));
				AssertNotNull(FindColumnByName(control.GuaranteesGrid_Exposed, Enterprise.Customs.EU.Business.Declaration.GuaranteeForDeclaration.Schema.PW_HolderIdentification));
				AssertNotNull(FindColumnByName(control.GuaranteesGrid_Exposed, Enterprise.Customs.GB.Business.Declaration.GBGuarantee.Schema.EntryInstructionID));

				Assert(control.GuaranteesGrid_Exposed.GetColumnStyle(Enterprise.Customs.EU.Business.Declaration.GuaranteeForDeclaration.Schema.PW_BondNumber).IsVisible);
				Assert(control.GuaranteesGrid_Exposed.GetColumnStyle(Enterprise.Customs.EU.Business.Declaration.GuaranteeForDeclaration.Schema.PW_BondNumber2).IsVisible);

				Assert(!control.GuaranteesGrid_Exposed.GetColumnStyle(Enterprise.Customs.EU.Business.Declaration.GuaranteeForDeclaration.Schema.PW_BondType).IsVisible);
				Assert(!control.GuaranteesGrid_Exposed.GetColumnStyle(Enterprise.Customs.EU.Business.Declaration.GuaranteeForDeclaration.Schema.PW_BondAmount).IsVisible);
				Assert(!control.GuaranteesGrid_Exposed.GetColumnStyle(Enterprise.Customs.EU.Business.Declaration.GuaranteeForDeclaration.Schema.PW_RX_NKCurrency).IsVisible);
				Assert(!control.GuaranteesGrid_Exposed.GetColumnStyle(Enterprise.Customs.EU.Business.Declaration.GuaranteeForDeclaration.Schema.PW_BondFiledPort).IsVisible);
				Assert(!control.GuaranteesGrid_Exposed.GetColumnStyle(Enterprise.Customs.EU.Business.Declaration.GuaranteeForDeclaration.Schema.PW_ValidityLimitation).IsVisible);

				Assert(!FindColumnByName(control.GuaranteesGrid_Exposed, Enterprise.Customs.GB.Business.Declaration.GBGuarantee.Schema.EntryInstructionID).IsUnavailable);

				var style = control.GuaranteesGrid_Exposed.GetColumnStyle(Enterprise.Customs.EU.Business.Declaration.GuaranteeForDeclaration.Schema.PW_Password);
				Assert(style is ZDropEditColumnStyleInfo);

				AssertEquals("[UCC 8/2] Code", control.GuaranteesGrid_Exposed.GetColumnCaption(Enterprise.Customs.EU.Business.Declaration.GuaranteeForDeclaration.Schema.PW_Password));
				AssertEquals("[UCC 8/3] GRN", control.GuaranteesGrid_Exposed.GetColumnCaption(Enterprise.Customs.EU.Business.Declaration.GuaranteeForDeclaration.Schema.PW_BondNumber));
				AssertEquals("[UCC 8/3] Other Guarantee Reference", control.GuaranteesGrid_Exposed.GetColumnCaption(Enterprise.Customs.EU.Business.Declaration.GuaranteeForDeclaration.Schema.PW_BondNumber2));
				AssertEquals("[UCC 8/3] Holder Identification", control.GuaranteesGrid_Exposed.GetColumnCaption(Enterprise.Customs.EU.Business.Declaration.GuaranteeForDeclaration.Schema.PW_HolderIdentification));
			}
		}

		ZGridColumnInfo FindColumnByName(ZGrid grid, string columnName)
		{
			return grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == $"{columnName}");
		}

		class GBGuaranteesUserControlForTesting : GBGuaranteesUserControl
		{
			public ZGrid GuaranteesGrid_Exposed => GuaranteesGrid;
		}

		void AssertColumnInGrid(ZGrid grid, string columnName, bool available, Type type, int index)
		{
			var columnStyle = grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == $"{columnName}");
			CombineAssertions(columnName, () =>
			{
				AssertEquals(columnName + " available", available, columnStyle != null);
				if (available)
				{
					AssertEquals(columnName + " column type", type, columnStyle.ColumnStyleType);
					AssertEquals(columnName + " column index", index, grid.ColumnStyles.IndexOf(columnStyle));
				}
			});
		}
	}
}
