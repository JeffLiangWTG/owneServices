using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.GUI.Testing
{
	internal sealed class StlLicenceControlTestCase : TestCaseWithFactory
	{
		public void TestPriceCurrencyMessageBox()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "CM1", "DB1");
			var lic2 = BillingTestHelper.CreateLicence(Factory, "EN1", "CM2", "DB1");

			var db1 = lic1.Database;
			var db2 = lic2.Database;
			AssertEquals(db1.PK, db2.PK);
			var headers = db1.ActiveLicHeadersForAllCompanies.OfType<LicenceHeader>().ToArray();
			AssertEquals(2, headers.Length);
			var header1 = headers[0];
			var header2 = headers[1];
			db1.LD_IsBilledPerCompany = false;
			header1.LA_RX_NKPriceCurrency = "";
			header2.LA_RX_NKPriceCurrency = "";

			Factory.Save();

			using (var form = new ZForm())
			{
				form.Controls.Add(new StlLicenceControl());
				form.SetDataBinding(lic1, "");
				form.Show();
				db1.LD_IsBilledPerCompany = true;
			}

			AssertEquals(@"The Price Currency entered here will be set to all organizations attached to this database.
Edit the other organizations manually if they require a different currency.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestAddSetting()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "CM1", "DB1");
			var db1 = lic1.Database;
			db1.ShowExpiredLicenceSettings = true;
			var settings = db1.LicenceSettingsForBinding;

			var s1 = Factory.New<BuyingGroupLicenceSetting>();
			s1.LS9_LD = db1.PK;
			var s2 = Factory.New<PriceLicenceSetting>();
			s2.LS9_LD = db1.PK;
			settings.Add(s1);
			settings.Add(s2);
			settings.ApplySort(EdiLicenceSettingSchema.Constants.LS9_Type, System.ComponentModel.ListSortDirection.Descending);

			AssertEquals(2, settings.Count);
			AssertEquals("PRI", settings[0].LS9_Type);
			AssertEquals("BUY", settings[1].LS9_Type);

			using (var form = new ZForm())
			{
				var control = new StlLicenceControlForTest();
				form.Controls.Add(control);
				form.SetDataBinding(lic1, "");
				form.Show();
				Application.DoEvents();

				var item1 = new ZToolStripMenuItem();
				item1.Tag = "CCR";

				var item2 = new ZToolStripMenuItem();
				item2.Tag = "DIS";

				control.AddSetting_Exposed(item1, null);
				control.AddSetting_Exposed(item2, null);

				//new line appears at the bottom of the list and preserve the original list order.
				AssertEquals(4, settings.Count);
				AssertEquals("PRI", settings[0].LS9_Type);
				AssertEquals("BUY", settings[1].LS9_Type);
				AssertEquals("CCR", settings[2].LS9_Type);
				AssertEquals("DIS", settings[3].LS9_Type);

				//no dates are populated 
				AssertEquals(true, settings[2].LS9_ValidFrom.IsEmpty);
				AssertEquals(true, settings[2].LS9_ValidTo.IsEmpty);
				AssertEquals(true, settings[3].LS9_ValidFrom.IsEmpty);
				AssertEquals(true, settings[3].LS9_ValidTo.IsEmpty);
			}
		}

		class StlLicenceControlForTest : StlLicenceControl
		{
			public void AddSetting_Exposed(object sender, System.EventArgs e)
			{
				AddSetting(sender, e);
			}
		}
	}
}
