using System;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class ZRulingFindBoxTest : BaseZCodeFindBoxTest
	{
		public void TestShowFormForCreatingNewRuling_EmptyRulingNumber()
		{
			ShowFormForCreatingNewRuling(string.Empty);
		}

		public void TestShowFormForCreatingNewRuling_ValidRulingNumber()
		{
			ShowFormForCreatingNewRuling("RULING001");
		}

		void ShowFormForCreatingNewRuling(string rulingNumber)
		{
			var importerOfRecord = Factory.NewWithValidTestData<OrgHeader>();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.ImporterOfRecordAddress.OrganisationPK = importerOfRecord.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();

			void HookMessageBoxForRuling(object sender, EventArgs e)
			{
				var messageBox = sender as MessageBoxForRuling;
				if (messageBox != null)
				{
					messageBox.Load += (o, args) =>
					{
						messageBox.WithImporterRadioButton.Checked = false;
						messageBox.WithoutImporterRadioButton.Checked = true;

						messageBox.DialogResult = DialogResult.Yes;
					};
				}
			}

			using (var frm = new ZForm(declaration))
			{
				var findBox = new ZRulingFindBox
				{
					BindTo = "FilteredInvoiceLines.CA_AuthorityNumber"
				};

				frm.Controls.Add(findBox);
				frm.Show();

				Application.DoEvents();

				try
				{
					KForm.FormCreated += HookMessageBoxForRuling;

					findBox.CodeBox.Text = rulingNumber;

					KeySender.SendKeyDownToProcessCmdKey(findBox.CodeBox, (int)Keys.F3);
					Application.DoEvents();

					var rulingCombined = (ZFormModaliser.LastFormShownForTest as ZForm)?.BusinessEntityForPersistingForm as ZZRefCusRulingCombined;
					AssertNotNull("Should create a new ZZRefCusRulingCombined.", rulingCombined);

					CombineAssertions(() =>
					{
						AssertEquals("ZZX_RulingNumber", rulingNumber, rulingCombined.ZZX_RulingNumber);
						AssertEquals("ZZX_OA_AppliesTo", ZGuid.Empty, rulingCombined.ZZX_OA_AppliesTo);
						AssertEquals("ZZX_OA_AppliesTo_ZAddress.OrgPK", ZGuid.Empty, rulingCombined.ZZX_OA_AppliesTo_ZAddress.OrgPK);
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
