using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	class APInvoiceDocManagerInfo : InvoicingDocManagerInfo
	{
		public APInvoiceDocManagerInfo(BusinessObject parent, string docManagerCode)
			: base(parent, docManagerCode)
		{
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			return base.GetRelatedObjects()
				.Concat(GetDraftInvoiceEDocs())
				.ToArray();
		}

		AccDraftInvoiceHeader[] GetDraftInvoiceEDocs()
		{
			var query = new ZDBOnlyQuery(typeof(AccDraftInvoiceHeader));

			if (BusinessEntity is InvoicingBase invoicingBase && !invoicingBase.DraftInvoiceHeaderPK.IsEmpty)
			{
				query.AddToFilter(AccDraftInvoiceHeaderSchema.PK, invoicingBase.DraftInvoiceHeaderPK);
			}
			else if (BusinessEntity is AccTransactionHeader transactionHeader)
			{
				query.AddToFilter(AccDraftInvoiceHeaderSchema.AIH_AH_PostedTransactionHeader, transactionHeader?.PK);
			}

			return BusinessEntity.Factory.Load<AccDraftInvoiceHeader>(query);
		}
	}
}
