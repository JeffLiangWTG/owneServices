using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class OverrideInvoiceAddressHelper : OverrideInvoiceAddressContactHelper
	{
		public OverrideInvoiceAddressHelper(BusinessObjectFactory factory, ZGuid addressPK, params ZGuid[] invoicePKs)
			: this(factory, null, addressPK, true, invoicePKs)
		{
		}

		public OverrideInvoiceAddressHelper(BusinessObjectFactory formFactory, BusinessObjectFactory parentFactory, ZGuid addressPK, bool canBizOBeSaved, params ZGuid[] invoicePKs)
			: base(formFactory, parentFactory, canBizOBeSaved, invoicePKs)
		{
			OverriddenAddressPK = addressPK;
		}

		ZGuid OverriddenAddressPK { get; set; }

		protected override string[] ColumnsToOverride()
		{
			return new string[]
			{
				"DisplayInvoiceAddressOverride"
			};
		}

		public override void SetDefaultValues()
		{
			foreach (InvoicingBase invoice in WrappedObjects)
			{
				invoice.DisplayInvoiceAddressOverride = OverriddenAddressPK;
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
				invoiceFromDB.AH_OA_InvoiceAddressOverride = invoice.DisplayInvoiceAddressOverride;

				foreach (InvoicingLineBase line in invoice.Lines)
				{
					ZDBOnlyQuery jobChargeFilter = new ZDBOnlyQuery(typeof(JobCharge));
					jobChargeFilter.AddToFilter(JobChargeSchema.JR_AL_ARLine, line.PK);

					var filteredCharges = ParentFactory.Load<JobCharge>(jobChargeFilter);
					foreach (var charge in filteredCharges)
					{
						charge.JR_OA_SellInvoiceAddress = invoice.AH_OA_InvoiceAddressOverride;
					}
				}
			}
		}
	}
}
