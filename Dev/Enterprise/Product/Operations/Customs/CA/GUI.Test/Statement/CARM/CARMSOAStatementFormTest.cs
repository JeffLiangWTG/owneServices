using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using static Enterprise.Customs.CA.Business.CARMStatementOfAccountStatementTypeList;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(CARMSOAStatementForm))]
	sealed class CARMSOAStatementFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var result = new CARMSOAStatementForm(Factory.New<CusStatementHeader>());
			result.ControllerID = ControllerIDs.Customs.CustomsStatement;
			return result;
		}

		public void TestControls()
		{
			var header = Factory.New<CusStatementHeader>();
			using (var form = new CARMSOAStatementForm(header))
			{
				form.Show();
				AssertEquals(true, form.Controls.Find("StatementHeaderGroupBox", true).First().Visible);
				AssertEquals(true, form.Controls.Find("StatementHeaderDetailsUserControl", true).First().Visible);

				var billingPeriodTabPage = form.Controls.Find("BillingPeriodTabPage", true).First() as ZTabPage;
				AssertEquals(true, billingPeriodTabPage.TabVisible);
				AssertEquals(true, form.Controls.Find("PreviousStatementBalanceCalcEdit", true).First().Visible);
				AssertEquals(true, form.Controls.Find("CorrectionsLastBalanceCalcEdit", true).First().Visible);
				AssertEquals(true, form.Controls.Find("PaymentsAfterLastSOACalcEdit", true).First().Visible);
				AssertEquals(true, form.Controls.Find("DisbursementsCalcEdit", true).First().Visible);
				AssertEquals(true, form.Controls.Find("InterestSumCalcEdit", true).First().Visible);
				AssertEquals(true, form.Controls.Find("DebitsCalcEdit", true).First().Visible);
				AssertEquals(true, form.Controls.Find("CreditsCalcEdit", true).First().Visible);
				AssertEquals(true, form.Controls.Find("TotalCalcEdit", true).First().Visible);
				AssertEquals(true, form.Controls.Find("DutiesCalcEdit", true).First().Visible);
				AssertEquals(true, form.Controls.Find("ExciseCalcEdit", true).First().Visible);
				AssertEquals(true, form.Controls.Find("ExciseDutiesCalcEdit", true).First().Visible);
				AssertEquals(true, form.Controls.Find("SIMACalcEdit", true).First().Visible);
				AssertEquals(true, form.Controls.Find("GSTCalcEdit", true).First().Visible);
				AssertEquals(true, form.Controls.Find("HSTCalcEdit", true).First().Visible);
				AssertEquals(true, form.Controls.Find("PSTCalcEdit", true).First().Visible);
				AssertEquals(true, form.Controls.Find("InterestCalcEdit", true).First().Visible);
				AssertEquals(true, form.Controls.Find("PenaltiesCalcEdit", true).First().Visible);
				AssertEquals(true, form.Controls.Find("PaymentsCalcEdit", true).First().Visible);
				AssertEquals(true, form.Controls.Find("OthersCalcEdit", true).First().Visible);

				var byDayTabPage = form.Controls.Find("ByDayTabPage", true).First() as ZTabPage;
				byDayTabPage.Select();
				AssertEquals(true, byDayTabPage.TabVisible);

				var linesGrid = form.Controls.Find("LinesGrid", true).First() as ZGrid;
				var columnInfos = linesGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				AssertEquals(true, columnInfos.First(x => x.ColumnName == "B3_ImporterCustomsID").IsVisible);
				AssertEquals(true, columnInfos.First(x => x.ColumnName == "B3_DueDate").IsVisible);
				AssertEquals(true, columnInfos.First(x => x.ColumnName == "B3_ScheduledProcessDate").IsVisible);
				AssertEquals(true, columnInfos.First(x => x.ColumnName == "B2_DueDate").IsVisible);
				AssertEquals(true, columnInfos.First(x => x.ColumnName == "B4_CARMDNChargeAmount_Duties").IsVisible);
				AssertEquals(true, columnInfos.First(x => x.ColumnName == "B4_CARMDNChargeAmount_ExciseTax").IsVisible);
				AssertEquals(true, columnInfos.First(x => x.ColumnName == "B4_CARMDNChargeAmount_ExciseDuties").IsVisible);
				AssertEquals(true, columnInfos.First(x => x.ColumnName == "B4_CARMDNChargeAmount_SIMA").IsVisible);
				AssertEquals(true, columnInfos.First(x => x.ColumnName == "B4_CARMDNChargeAmount_GST").IsVisible);
				AssertEquals(true, columnInfos.First(x => x.ColumnName == "B4_CARMDNChargeAmount_HST").IsVisible);
				AssertEquals(true, columnInfos.First(x => x.ColumnName == "B4_CARMDNChargeAmount_PST").IsVisible);
				AssertEquals(true, columnInfos.First(x => x.ColumnName == "B4_CARMDNChargeAmount_Interests").IsVisible);
				AssertEquals(true, columnInfos.First(x => x.ColumnName == "B4_CARMDNChargeAmount_Penalties").IsVisible);
				AssertEquals(true, columnInfos.First(x => x.ColumnName == "B4_CARMDNChargeAmount_Payments").IsVisible);
				AssertEquals(true, columnInfos.First(x => x.ColumnName == "B4_CARMDNChargeAmount_Others").IsVisible);
			}
		}

		public void TestConstructor()
		{
			var header = Factory.New<CusStatementHeader>();
			header.B2_IsMonthlyStatement = true;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Today, true))
			{
				header.B2_StatementType = ShortCodes.LegalEntiry;
				using (var form = new CARMSOAStatementForm(header))
				{
					AssertNotNull("Support DocDataPlugIn for CARM Statement Of Account", form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
				}
			}
		}
	}
}
