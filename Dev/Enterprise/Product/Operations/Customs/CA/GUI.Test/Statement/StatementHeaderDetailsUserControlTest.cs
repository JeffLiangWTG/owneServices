using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class StatementHeaderDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestControlVisible()
		{
			var header = Factory.New<CusStatementHeader>();
			header.B2_StatementNumber = "DN-123456789RM2345-1";

			void AssertCARMVisible(bool expectedValue)
			{
				using (var form = new ZForm(header))
				using (var ctr = new StatementHeaderDetailsUserControl())
				{
					form.Controls.Add(ctr);
					form.Show();

					Application.DoEvents();

					var cARMGrandTotalGroupBox = ctr.Controls.Find("CARMGrandTotalGroupBox", true).First();
					var cARMDNGSTCalcEdit = ctr.Controls.Find("CARMDNGSTCalcEdit", true).First();
					var cARMDNOthersCalcEdit = ctr.Controls.Find("CARMDNOthersCalcEdit", true).First();
					var cARMDNExciseTaxCalcEdit = ctr.Controls.Find("CARMDNExciseTaxCalcEdit", true).First();
					var cARMDNExciseDutiesCalcEdit = ctr.Controls.Find("CARMDNExciseDutiesCalcEdit", true).First();
					var cARMDNSIMACalcEdit = ctr.Controls.Find("CARMDNSIMACalcEdit", true).First();
					var cARMDNDutiesCalcEdit = ctr.Controls.Find("CARMDNDutiesCalcEdit", true).First();
					var cARMDNInterestsCalcEdit = ctr.Controls.Find("CARMDNInterestsCalcEdit", true).First();
					var cARMDNPaymentsCalcEdit = ctr.Controls.Find("CARMDNPaymentsCalcEdit", true).First();
					var cARMDNDisbursementsCalcEdit = ctr.Controls.Find("CARMDNDisbursementsCalcEdit", true).First();

					AssertEquals("Should only be visible when the statement is CARM DN.", expectedValue, cARMGrandTotalGroupBox.Visible);
					AssertEquals("Should only be visible when the statement is CARM DN.", expectedValue, cARMDNGSTCalcEdit.Visible);
					AssertEquals("Should only be visible when the statement is CARM DN.", expectedValue, cARMDNOthersCalcEdit.Visible);
					AssertEquals("Should only be visible when the statement is CARM DN.", expectedValue, cARMDNExciseTaxCalcEdit.Visible);
					AssertEquals("Should only be visible when the statement is CARM DN.", expectedValue, cARMDNExciseDutiesCalcEdit.Visible);
					AssertEquals("Should only be visible when the statement is CARM DN.", expectedValue, cARMDNSIMACalcEdit.Visible);
					AssertEquals("Should only be visible when the statement is CARM DN.", expectedValue, cARMDNDutiesCalcEdit.Visible);
					AssertEquals("Should only be visible when the statement is CARM DN.", expectedValue, cARMDNInterestsCalcEdit.Visible);
					AssertEquals("Should only be visible when the statement is CARM DN.", expectedValue, cARMDNPaymentsCalcEdit.Visible);
					AssertEquals("Should only be visible when the statement is CARM DN.", expectedValue, cARMDNDisbursementsCalcEdit.Visible);
				}
			}

			void AssertVisible(bool chargeControlVisiable, bool accountingDateDateEditVisiable)
			{
				using (var form = new ZForm(header))
				using (var ctr = new StatementHeaderDetailsUserControl())
				{
					form.Controls.Add(ctr);
					form.Show();

					Application.DoEvents();

					var accountingDateDateEdit = ctr.Controls.Find("AccountingDateDateEdit", true).First();
					var dutiesCalcEdit = ctr.Controls.Find("DutiesCalcEdit", true).First();
					var sIMACalcEdit = ctr.Controls.Find("SIMACalcEdit", true).First();
					var exciseTaxCalcEdit = ctr.Controls.Find("ExciseTaxCalcEdit", true).First();
					var gSTCalcEdit = ctr.Controls.Find("GSTCalcEdit", true).First();
					var othersCalcEdit = ctr.Controls.Find("OthersCalcEdit", true).First();

					AssertEquals("Should only be visible when the statement is DN.", accountingDateDateEditVisiable, accountingDateDateEdit.Visible);
					AssertEquals("Should only be visible when the statement is DN.", chargeControlVisiable, dutiesCalcEdit.Visible);
					AssertEquals("Should only be visible when the statement is DN.", chargeControlVisiable, sIMACalcEdit.Visible);
					AssertEquals("Should only be visible when the statement is DN.", chargeControlVisiable, exciseTaxCalcEdit.Visible);
					AssertEquals("Should only be visible when the statement is DN.", chargeControlVisiable, gSTCalcEdit.Visible);
					AssertEquals("Should only be visible when the statement is DN.", chargeControlVisiable, othersCalcEdit.Visible);
				}
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, false))
			{
				header.B2_IsMonthlyStatement = false;
				AssertVisible(true, true);
				AssertCARMVisible(false);

				header.B2_IsMonthlyStatement = true;
				AssertVisible(false, false);
				AssertCARMVisible(false);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				header.B2_IsMonthlyStatement = false;
				AssertVisible(false, true);
				AssertCARMVisible(true);
			}
		}

		public void TestBusinessNumberLabel()
		{
			var header = Factory.New<CusStatementHeader>();

			void AssertLabel(string label)
			{
				using (var form = new ZForm(header))
				using (var ctr = new StatementHeaderDetailsUserControl())
				{
					form.Controls.Add(ctr);
					form.Show();

					Application.DoEvents();

					var findBox = (ZGuidFindBox)ctr.Controls.Find("ImporterGuidFindBox", true).First();

					AssertEquals("CaptionResourceString", label, findBox.CaptionResourceString.Caption);
				}
			}

			AssertLabel("Importer");

			header.B2_StatementType = "I";
			AssertLabel("Importer");

			header.B2_StatementType = "B";
			AssertLabel("Broker");

			header.B2_StatementType = "C";
			AssertLabel("Importer");

			header.B2_StatementType = "U";
			AssertLabel("Importer");
		}

		public void TestStatementAmountLabel()
		{
			var header = Factory.New<CusStatementHeader>();

			void AssertLabel(string label)
			{
				using (var form = new ZForm(header))
				using (var control = new StatementHeaderDetailsUserControl())
				{
					form.Controls.Add(control);
					form.Show();

					Application.DoEvents();

					var statement = (ZCalcEdit)control.Controls.Find("StatementAmountCalcEdit", true).First();
					AssertEquals("CaptionResourceString", label, statement.CaptionResourceString.Caption);
				}
			}

			header.B2_IsMonthlyStatement = false;
			AssertLabel("Total Due");

			header.B2_IsMonthlyStatement = true;
			AssertLabel("Statement Total");
		}
	}
}
