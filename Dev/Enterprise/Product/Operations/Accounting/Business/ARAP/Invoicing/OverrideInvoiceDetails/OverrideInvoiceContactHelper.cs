using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class OverrideInvoiceContactHelper : OverrideInvoiceAddressContactHelper
	{
		public OverrideInvoiceContactHelper(BusinessObjectFactory factory, ZGuid contactPK, params ZGuid[] invoicePKs)
			: this(factory, null, contactPK, true, invoicePKs)
		{
		}

		public OverrideInvoiceContactHelper(BusinessObjectFactory formFactory, BusinessObjectFactory parentFactory, ZGuid contactPK, bool canBizOBeSaved, params ZGuid[] invoicePKs)
			: base(formFactory, parentFactory, canBizOBeSaved, invoicePKs)
		{
			OverriddenContactPK = contactPK;
		}

		ZGuid OverriddenContactPK { get; set; }

		protected override string[] ColumnsToOverride()
		{
			return new string[]
			{
				"DisplayInvoiceContactOverride"
			};
		}

		public override void SetDefaultValues()
		{
			foreach (InvoicingBase invoice in WrappedObjects)
			{
				invoice.DisplayInvoiceContactOverride = OverriddenContactPK;
			}
		}

		public override void UpdateRelatedParentBizO()
		{
			base.UpdateRelatedParentBizO();

			foreach (InvoicingBase invoice in WrappedObjects)
			{
				ZDBOnlyQuery invoiceFilter = new ZDBOnlyQuery(typeof(AccTransactionHeader));
				invoiceFilter.AddToFilter(AccTransactionHeaderSchema.PK, invoice.PK);

				AccTransactionHeader invoiceFromDB = ParentFactory.LoadTop1<AccTransactionHeader>(invoiceFilter);
				invoiceFromDB.AH_OC_InvoiceContactOverride = invoice.DisplayInvoiceContactOverride;

				foreach (InvoicingLineBase line in invoice.Lines)
				{
					ZDBOnlyQuery jobChargeFilter = new ZDBOnlyQuery(typeof(JobCharge));
					jobChargeFilter.AddToFilter(JobChargeSchema.JR_AL_ARLine, line.PK);

					var filteredCharges = ParentFactory.Load<JobCharge>(jobChargeFilter);
					foreach (var charge in filteredCharges)
					{
						charge.JR_OC_SellInvoiceContact = invoice.AH_OC_InvoiceContactOverride;
					}
				}
			}
		}
	}
}