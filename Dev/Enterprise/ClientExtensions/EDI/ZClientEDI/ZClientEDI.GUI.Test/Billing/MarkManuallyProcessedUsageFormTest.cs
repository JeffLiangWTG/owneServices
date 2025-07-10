using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.GUI.Testing
{
	[TestedType(typeof(MarkManuallyProcessedUsageForm))]
	class MarkManuallyProcessedUsageFormTest : ZFormBasherTest
	{
		// Disables critical validation "Missing Reversing Transaction for this canceled transaction".
		[SuspendCriticalValidation]
		public void TestMarkNonInvoiced()
		{
			var month1 = new ZDateTime(2015, 1, 1);
			var month2 = new ZDateTime(2015, 2, 1);
			var month3 = new ZDateTime(2015, 3, 1);
			var month4 = new ZDateTime(2015, 4, 1);
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var lic2 = BillingTestHelper.CreateLicence(Factory, "BBB");

			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = lic1.Company.LC_OH;
			invoice.AH_TransactionType = TransactionTypes.Invoice;

			ARInvoice invoiceCancelled = Factory.NewWithValidTestData<ARInvoice>();
			invoiceCancelled.AH_OH = lic1.Company.LC_OH;
			invoiceCancelled.AH_TransactionType = TransactionTypes.Invoice;
			invoiceCancelled.AH_IsCancelled = true;

			var u1 = BillingTestHelper.CreateChargeableUsage(Factory, "HOS", "", month1, lic1, 11);
			var u2 = BillingTestHelper.CreateChargeableUsage(Factory, "HOS", "", month2, lic1, 11);
			var u3 = BillingTestHelper.CreateChargeableUsage(Factory, "HOS", "", month3, lic1, 11);
			var u4 = BillingTestHelper.CreateChargeableUsage(Factory, "HOS", "", month4, lic1, 11);

			var u5 = BillingTestHelper.CreateChargeableUsage(Factory, "HRD", "", month1, lic1, 11);
			var u6 = BillingTestHelper.CreateChargeableUsage(Factory, "HRD", "", month2, lic1, 11);
			var u7 = BillingTestHelper.CreateChargeableUsage(Factory, "HRD", "", month3, lic1, 11);
			var u8 = BillingTestHelper.CreateChargeableUsage(Factory, "HRD", "", month4, lic1, 11);

			var u9 = BillingTestHelper.CreateChargeableUsage(Factory, "HOS", "", month1, lic2, 11);
			var u10 = BillingTestHelper.CreateChargeableUsage(Factory, "HOS", "", month2, lic2, 11);
			var u11 = BillingTestHelper.CreateChargeableUsage(Factory, "HOS", "", month3, lic2, 11);
			var u12 = BillingTestHelper.CreateChargeableUsage(Factory, "HOS", "", month4, lic2, 11);

			var u13 = BillingTestHelper.CreateChargeableUsage(Factory, "HRD", "", month1, lic2, 11);
			var u14 = BillingTestHelper.CreateChargeableUsage(Factory, "HRD", "", month2, lic2, 11);
			var u15 = BillingTestHelper.CreateChargeableUsage(Factory, "HRD", "", month3, lic2, 11);
			var u16 = BillingTestHelper.CreateChargeableUsage(Factory, "HRD", "", month4, lic2, 11);

			u2.U1_AH_Invoice = invoice.PK;
			u3.U1_AH_Invoice = invoiceCancelled.PK;
			u6.U1_LCC = ZGuid.Empty;
			u6.U1_LC = ZGuid.Empty;

			Factory.Save();

			int rowsRemaining = 16;
			int rowsAffected = MarkManuallyProcessedUsageForm.MarkNonInvoiced(month2, month3,
				new string[] { "HOS", "HRD" },
				new string[] { lic1.Company.Header.OH_Code });
			AssertEquals(3, rowsAffected);
			rowsRemaining -= rowsAffected;
			var usageList = new BusinessObjectFactory().Load<ClientChargeableUsage>(new ZQuery(ClientChargeableUsageSchema.U1_ManuallyProcessed, false));
			AssertEquals(rowsRemaining, usageList.Length);
			AssertEquals("marked 3, 6, 7", 0, usageList.Count(x => x.PK == u3.PK || x.PK == u6.PK || x.PK == u7.PK));

			rowsAffected = MarkManuallyProcessedUsageForm.MarkNonInvoiced(month1, month4,
				new string[] { "ZZZ" },
				new string[] { lic2.Company.Header.OH_Code });
			AssertEquals("nothing marked since no ZZZ usage", 0, rowsAffected);
			usageList = new BusinessObjectFactory().Load<ClientChargeableUsage>(new ZQuery(ClientChargeableUsageSchema.U1_ManuallyProcessed, false));
			AssertEquals("nothing marked since no ZZZ usage", rowsRemaining, usageList.Length);

			rowsAffected = MarkManuallyProcessedUsageForm.MarkNonInvoiced(month1, month4,
				new string[] { "HOS" },
				new string[] { "ZZZ" });
			AssertEquals("nothing marked since no ZZZ org", 0, rowsAffected);
			usageList = new BusinessObjectFactory().Load<ClientChargeableUsage>(new ZQuery(ClientChargeableUsageSchema.U1_ManuallyProcessed, false));
			AssertEquals("nothing marked since no ZZZ org", rowsRemaining, usageList.Length);

			rowsAffected = MarkManuallyProcessedUsageForm.MarkNonInvoiced(month1, month3,
				new string[] { "HOS" },
				new string[] { lic2.Company.Header.OH_Code });
			AssertEquals("marked", 3, rowsAffected);
			rowsRemaining -= rowsAffected;
			usageList = new BusinessObjectFactory().Load<ClientChargeableUsage>(new ZQuery(ClientChargeableUsageSchema.U1_ManuallyProcessed, false));
			AssertEquals("remaining", rowsRemaining, usageList.Length);
			AssertEquals("marked 9, 10, 11, 12", 0, usageList.Count(x => x.PK == u9.PK || x.PK == u10.PK || x.PK == u11.PK));

			rowsAffected = MarkManuallyProcessedUsageForm.MarkNonInvoiced(month1, month4,
				new string[] { "HOS", "HRD" },
				new string[] { "" });
			AssertEquals("marked", 9, rowsAffected);
			rowsRemaining -= rowsAffected;
			usageList = new BusinessObjectFactory().Load<ClientChargeableUsage>(new ZQuery(ClientChargeableUsageSchema.U1_ManuallyProcessed, false));
			AssertEquals("remaining is invoiced", rowsRemaining, usageList.Length);

			rowsAffected = MarkManuallyProcessedUsageForm.MarkNonInvoiced(month1, month3,
				new string[] { "HOS" },
				new string[] { lic1.Company.Header.OH_Code, lic2.Company.Header.OH_Code });
		}

		public void TestUnmark()
		{
			var month1 = new ZDateTime(2015, 1, 1);
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var lic2 = BillingTestHelper.CreateLicence(Factory, "BBB");

			var u1 = BillingTestHelper.CreateChargeableUsage(Factory, "HOS", "", month1, lic1, 11);
			u1.U1_ManuallyProcessed = true;
			var u2 = BillingTestHelper.CreateChargeableUsage(Factory, "HOS", "", month1, lic2, 11);
			u2.U1_ManuallyProcessed = true;

			Factory.Save();

			using (var form = new MarkManuallyProcessedUsageForm())
			{
				form.OrgTextBox.Text = lic1.Company.Header.OH_Code;
				form.CodeTextBox.Text = "HOS";
				form.StartDateEdit.DateTimeValue = month1;
				form.EndDateEdit.DateTimeValue = month1;
				form.UnmarkButton_Click(form, EventArgs.Empty);
			}

			u1.Reload();
			u2.Reload();
			AssertEquals(false, u1.U1_ManuallyProcessed);
			AssertEquals(true, u2.U1_ManuallyProcessed);
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new MarkManuallyProcessedUsageForm();
		}

		#endregion
	}
}
