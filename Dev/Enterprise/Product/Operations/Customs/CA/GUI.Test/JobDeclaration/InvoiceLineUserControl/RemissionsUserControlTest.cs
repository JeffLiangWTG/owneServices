using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using JobMessageTypeList = Enterprise.Customs.CA.Business.JobMessageTypeList;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class RemissionsUserControlTest : TestCaseWithFactory
	{
		public void TestOnAfterFirstBinding()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			using (var frm = new ZForm(invoiceLine))
			{
				var ctr = new RemissionsUserControl();
				frm.Controls.Add(ctr);
				frm.Show();

				AssertEquals(true, ctr.Controls.Find("TRSNumberTextBox", true).First().Visible);
				AssertEquals(false, ctr.Controls.Find("RemissionTypeDropEdit", true).First().Visible);
				AssertEquals(true, ((ZRulingFindBox)ctr.Controls.Find("AuthorityNumberCodeFindBox", true).First()).CaptionResourceString.Caption != "DRL/OIC/PMT");
			}

			var cadEntry = declaration.ActiveEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			using (var frm = new ZForm(invoiceLine))
			{
				var ctr = new RemissionsUserControl();
				frm.Controls.Add(ctr);
				frm.Show();

				AssertEquals(false, ctr.Controls.Find("TRSNumberTextBox", true).First().Visible);
				AssertEquals(true, ctr.Controls.Find("RemissionTypeDropEdit", true).First().Visible);
				AssertEquals("DRL/OIC/PMT", ((ZRulingFindBox)ctr.Controls.Find("AuthorityNumberCodeFindBox", true).First()).CaptionResourceString.Caption);
			}
		}

		public void TestSyncConfigurationsFromInvoiceLine()
		{
			var importerOfRecord = Factory.NewWithValidTestData<OrgHeader>();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.ImporterOfRecordAddress.OrganisationPK = importerOfRecord.PK;

			var messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_AuthorityNumber = "RULING001";
			invoiceLine.CA_CalculationMethod = "DD";

			invoiceLine.RulingConfigurations.RemoveAndDeleteAll();

			invoiceLine.RulingConfigurations.AddNew(RefCusRulingConfigCategories.Codes.DTY, RefCusRulingConfigTypes.Codes.Specific, 5, string.Empty);
			invoiceLine.RulingConfigurations.AddNew(RefCusRulingConfigCategories.Codes.EXC, RefCusRulingConfigTypes.Codes.AcceptAmount, 12, "T");

			Factory.Save();

			void HookMessageBoxForRuling(object sender, EventArgs e)
			{
				var messageBox = sender as MessageBoxForRuling;
				if (messageBox != null)
				{
					messageBox.Load += (o, args) =>
					{
						messageBox.DialogResult = DialogResult.Yes;
					};
				}
			}

			using (var frm = new ZForm(invoiceLine))
			{
				var ctr = new RemissionsUserControl();
				var findBox = (ZRulingFindBox)ctr.Controls.Find("AuthorityNumberCodeFindBox", true).First();

				frm.Controls.Add(ctr);
				frm.Show();

				try
				{
					KForm.FormCreated += HookMessageBoxForRuling;

					KeySender.SendKeyDownToProcessCmdKey(findBox.CodeBox, (int)Keys.F3);
					Application.DoEvents();

					var rulingCombined = (ZFormModaliser.LastFormShownForTest as ZForm)?.BusinessEntityForPersistingForm as ZZRefCusRulingCombined;
					AssertNotNull("Should create a new ZZRefCusRulingCombined.", rulingCombined);

					CombineAssertions(() =>
					{
						AssertEquals("ZZX_RulingNumber", "RULING001", rulingCombined.ZZX_RulingNumber);
						AssertEquals("ZZX_RulingType", "DD", rulingCombined.ZZX_RulingType);
						AssertEquals("ZZX_OA_AppliesTo", importerOfRecord.MainAddress.PK, rulingCombined.ZZX_OA_AppliesTo);
						AssertEquals("ZZX_OA_AppliesTo_ZAddress.OrgPK", importerOfRecord.PK, rulingCombined.ZZX_OA_AppliesTo_ZAddress.OrgPK);

						foreach (var config in invoiceLine.RulingConfigurations.Cast<CusRulingConfigCombined>())
						{
							var foundConfig = rulingCombined
								.Configurations
								.Cast<CusRulingConfigCombined>()
								.FirstOrDefault(c => c.ZZY_Category == config.ZZY_Category
										&& c.ZZY_Type == config.ZZY_Type
										&& c.ZZY_Rate == config.ZZY_Rate
										&& c.ZZY_Value == config.ZZY_Value
										&& c.ZZY_JI_InvoiceLine == ZGuid.Empty);

							AssertNotNull("Should sync all config data from the invoice line with all values except the ZZY_JI_InvoiceLine.", foundConfig);
						}
					});
				}
				finally
				{
					KForm.FormCreated -= HookMessageBoxForRuling;
				}
			}
		}
	}
}
