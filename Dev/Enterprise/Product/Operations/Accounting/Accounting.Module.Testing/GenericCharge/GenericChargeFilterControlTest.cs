using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.GenericCharge;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module.Testing
{
	public class GenericChargeFilterControlTest : TestCaseWithFactory
	{
		public void TestShowAlternateGLAccounts_HasGLAccountSelectionAndEntry()
		{
			var chart = TestObjectCreator.CreateAlternateChart("MGT", "Management Reporting", isGlobal: true);
			TestObjectCreator.CreateAccAlternateChartFormat(chart, 1, "X", "tier 1");
			Factory.Save();

			var glHeader = TestObjectCreator.CreateAccGLHeader("1991.01.10", "AS", "BANK ACCOUNT", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Credit);
			var alternateGLAccount = TestObjectCreator.CreateAccAlternateGlAccount(chart.PK, "10.00.1000", "BSH", "DR", 1, "OV", 1, description: "AlternateGLAccount1");
			TestObjectCreator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, glHeader.PK);
			Factory.Save();

			AccountingMasterFilesRegistry.Instance.GLAccountSelectionAndEntry.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chart.PK.ToGuid());

			AssertShowAlternateGLAccounts(true);
		}

		public void TestShowAlternateGLAccounts_NoGLAccountSelectionAndEntry()
		{
			AssertShowAlternateGLAccounts(false);
		}

		void AssertShowAlternateGLAccounts(bool hasGLAccountSelectionAndEntry)
		{
			var journal = Factory.NewWithValidTestData<GLJournal>();
			using (var form = new ZForm(journal))
			{
				var genericCharges = new GenericChargeCollection(Factory);
				var filterBO = new GenericChargeFilterBusinessObject();

				var control = new GenericChargeFilterControl(genericCharges, filterBO);
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				var filteredGrid = form.FindSingleOrDefault<ZGrid>("FilteredGrid");

				var alternateAccounts = filteredGrid.GetColumnStyle("AlternateAccounts");
				AssertNotNull(alternateAccounts);
				AssertEquals(true, hasGLAccountSelectionAndEntry ? alternateAccounts.IsVisible : alternateAccounts.IsUnavailable);
			}
		}

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
		protected TestObjectCreator fTestObjectCreator;
	}
}
