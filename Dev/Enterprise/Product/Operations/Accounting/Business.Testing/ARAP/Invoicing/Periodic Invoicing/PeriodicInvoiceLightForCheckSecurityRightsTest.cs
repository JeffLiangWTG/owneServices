using CargoWise.Types;
using NUnit.Framework;
using static Enterprise.Accounting.Business.ARAP.Invoicing.PeriodicInvoiceBulkPoster;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(PeriodicInvoiceLightForCheckSecurityRights))]
	public class PeriodicInvoiceLightForCheckSecurityRightsTest : PeriodicInvoiceLightTest
	{
		public override void TestRunPreSaveValidationCore()
		{
			Assert("PreSaveValidation is not applicable for PeriodicInvoiceLightForCheckSecurityRights", true);
		}

		public override void TestLoadMiscInvoicesWithOtherTaxes()
		{
			Assert("This is not applicable for PeriodicInvoiceLightForCheckSecurityRights", true);
		}

		public override void TestPeriodicInvoiceDoesNotLoadMiscInvoicesWithOtherTaxes()
		{
			Assert("This is not applicable for PeriodicInvoiceLightForCheckSecurityRights", true);
		}

		public override void TestReloadChargesByJob_ExcludeAutoJRJ()
		{
			Assert("This is not applicable for PeriodicInvoiceLightForCheckSecurityRights", true);
		}

		public override void TestRemoveJobsWhichAllChargesShouldPostAutoJRJ_AllChargesShouldNotPost()
		{
			Assert("This is not applicable for PeriodicInvoiceLightForCheckSecurityRights", true);
		}

		public override void TestRemoveJobsWhichAllChargesShouldPostAutoJRJ_PartOfChargesShouldPost()
		{
			Assert("This is not applicable for PeriodicInvoiceLightForCheckSecurityRights", true);
		}

		public override void TestRemoveJobsWhichAllChargesShouldPostAutoJRJ_AllOfChargesShouldPost()
		{
			Assert("This is not applicable for PeriodicInvoiceLightForCheckSecurityRights", true);
		}

		protected override void SetupPeriodicInvoiceWithJobsAndMiscInvoices(PeriodicInvoiceBase invoice, ZGuid[] jobPks, ZGuid[] chargePks, ZGuid[] invoicePks)
		{
			var invoiceInfo = PrepareInvoiceInfo(invoice, jobPks, chargePks, invoicePks);
			(testPeriodicInvoice_internalValue as PeriodicInvoiceLightForCheckSecurityRights)?.InitalizeFromInvoiceInfo(invoiceInfo);
		}

		protected override void Initialize(InvoiceInfo invoiceInfo)
		{
			(testPeriodicInvoice_internalValue as PeriodicInvoiceLightForCheckSecurityRights)?.InitalizeFromInvoiceInfo(invoiceInfo);
		}
	}
}
