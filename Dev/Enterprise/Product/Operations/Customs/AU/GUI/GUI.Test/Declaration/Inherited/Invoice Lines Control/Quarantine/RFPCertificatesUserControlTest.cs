using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class RFPCertificatesUserControlTest : TestCaseWithFactory
	{
		public void TestGroupBoxCaptions()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();

			using (var testForm = new ZForm(invoiceLine))
			using (var control = new RFPCertificatesUserControl())
			{
				testForm.Controls.Add(control);
				testForm.Show();
				var productDescriptionGroupBox = control.FindSingle<ZGroupBox>("ProductDescriptionGroupBox");
				AssertEquals("Product Description", productDescriptionGroupBox.Text);
				var certificatetGroupBox = control.FindSingle<ZGroupBox>("CertificateGroupBox");
				AssertEquals("Certificate", certificatetGroupBox.Text);
			}
		}

		public void TestManualCertificateProductDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Description = "BAD MEAT";
			var quarantineExDocLine = invoiceLine.QuarantineExDocLine;
			quarantineExDocLine.QL_MeatInspectionDescription = "GOOD MEAT";

			using (var testForm = new ZForm(invoiceLine))
			{
				using (var control = new RFPCertificatesUserControl())
				{
					testForm.Controls.Add(control);
					testForm.Show();

					var meatInspectionDescriptionTextBox = control.FindSingle<ZTextBox>("QL_MeatInspectionDescriptionTextBox");
					AssertEquals("GOOD MEAT", meatInspectionDescriptionTextBox.Text);

					var buttonControl = control.FindSingle<ZButton>("CopyMeatInspectionDescriptionFromLineButton");
					buttonControl.PerformClick();
					AssertEquals("BAD MEAT", meatInspectionDescriptionTextBox.Text);
				}
			}
		}

		public void TestSetupVisibility()
		{
			using (var control = new RFPCertificatesUserControl())
			{
				control.SetupVisibility(true);
				Assert("QL_CommercialProductDescriptionTextBox should not be visible when NEXDOCSActive is true.", !control.QL_CommercialProductDescriptionTextBox.Visible);
				Assert("QL_HealthCertificateDescriptionTextBox should not be visible when NEXDOCSActive is true.", !control.QL_HealthCertificateDescriptionTextBox.Visible);
				Assert("QL_ImportAuthorityCodeTextBox should not be visible when NEXDOCSActive is true.", !control.QL_ImportAuthorityCodeTextBox.Visible);
				Assert("QL_SendHCDescCheckBox should not be visible when NEXDOCSActive is true.", !control.QL_SendHCDescCheckBox.Visible);
				Assert("CopyMeatInspectionDescriptionFromLineButton should be visible when NEXDOCSActive is true.", control.CopyMeatInspectionDescriptionFromLineButton.Visible);

				control.SetupVisibility(false);
				Assert("QL_CommercialProductDescriptionTextBox should be visible when NEXDOCSActive is false.", control.QL_CommercialProductDescriptionTextBox.Visible);
				Assert("QL_HealthCertificateDescriptionTextBox should be visible when NEXDOCSActive is false.", control.QL_HealthCertificateDescriptionTextBox.Visible);
				Assert("QL_ImportAuthorityCodeTextBox should be visible when NEXDOCSActive is false.", control.QL_ImportAuthorityCodeTextBox.Visible);
				Assert("QL_SendHCDescCheckBox should be visible when NEXDOCSActive is false.", control.QL_SendHCDescCheckBox.Visible);
				Assert("CopyMeatInspectionDescriptionFromLineButton should not be visible when NEXDOCSActive is false.", !control.CopyMeatInspectionDescriptionFromLineButton.Visible);
			}
		}
	}
}
