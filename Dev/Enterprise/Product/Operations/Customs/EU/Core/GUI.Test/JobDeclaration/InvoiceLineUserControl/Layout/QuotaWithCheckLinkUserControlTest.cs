using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	class QuotaWithCheckLinkUserControlTest : TestCaseWithFactory
	{
		public void TestIExtendedControl()
		{
			using (var control = new QuotaWithCheckLinkUserControl())
			{
				CombineAssertions(() =>
				{
					AssertEquals("Host", control, control.Host);
					AssertType<DefaultControlExtensionCollection>("Extensions", control.Extensions);
				});
			}
		}

		public void TestZLabelCaptionRenderer()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new ZForm(invoiceLine))
			using (var control = new QuotaWithCheckLinkUserControl())
			{
				form.CaptionRenderingEnabled = true;
				form.Controls.Add(control);
				form.Show();

				AssertNotNullOrEmpty(control.GetExtension<ZLabelCaptionRenderer>().Caption);
			}
		}

		public void TestResourceStringBindingMember()
		{
			using (var control = new QuotaWithCheckLinkUserControl())
			{
				AssertEquals(nameof(JobComInvoiceLine.JI_ConcessionOrder), control.ResourceStringBindingMember);
			}
		}

		public void TestQuotaBalanceUrlLinkLabel()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			using (var control = new QuotaWithCheckLinkUserControl())
			{
				var checkQuotaLinkLabel = control.FindSingle<ZLinkLabel>("CheckQuotaBalanceLinkLabel");

				checkQuotaLinkLabel.OnLinkClicked_Exposed(new System.Windows.Forms.LinkLabelLinkClickedEventArgs(null));
				AssertContains("CheckQuotaBalanceLinkLabel", @"https://ec.europa.eu/taxation_customs/dds2/taric/quota_consultation.jsp", WebUrlLauncher.LastUrlLaunched);
			}
		}

		public void TestQuotaBalanceUrlLinkLabelWithQuota()
		{
			using (var control = new QuotaWithCheckLinkUserControl())
			{
				var checkQuotaLinkLabel = control.FindSingle<ZLinkLabel>("CheckQuotaBalanceLinkLabel");
				var quotaDropEdit = control.FindSingle<ZDropEdit>("QuotaDropEdit");
				quotaDropEdit.Text = "098567";

				checkQuotaLinkLabel.OnLinkClicked_Exposed(new System.Windows.Forms.LinkLabelLinkClickedEventArgs(null));
				AssertContains("CheckQuotaBalanceLinkLabel", @"https://ec.europa.eu/taxation_customs/dds2/taric/quota_consultation.jsp", WebUrlLauncher.LastUrlLaunched);
				AssertContains("CheckQuotaBalanceLinkLabel", "Code=098567", WebUrlLauncher.LastUrlLaunched);
			}
		}
	}
}
