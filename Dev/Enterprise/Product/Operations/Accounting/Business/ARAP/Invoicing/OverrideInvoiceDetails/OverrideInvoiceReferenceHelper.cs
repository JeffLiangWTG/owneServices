using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class OverrideInvoiceReferenceHelper : OverrideInvoiceDetailsHelper
	{
		public OverrideInvoiceReferenceHelper(BusinessObjectFactory factory, params ZGuid[] transactionPKs)
			: this(factory, null, true, transactionPKs)
		{
		}

		public OverrideInvoiceReferenceHelper(BusinessObjectFactory formFactory, BusinessObjectFactory parentFactory, bool canBizOBeSaved, params ZGuid[] transactionPKs)
			: base(formFactory, parentFactory, canBizOBeSaved, transactionPKs)
		{
		}

		protected override string[] ColumnsToOverride()
		{
			ArrayList columns = new ArrayList();

			if (Env.Security.PayablesModifyInvoiceRemittanceReference.IsAllowed)
			{
				columns.Add("InvoiceRemittanceReference");
			}

			if (Env.Security.PayablesModifyInvoiceDateNumOrSupplierCostRef.IsAllowed)
			{
				columns.Add("AH_TransactionNum");
				columns.Add("AH_InvoiceDate");
				columns.Add("AH_ChequeOrReference");
			}

			return (string[])columns.ToArray(typeof(string));
		}

		protected override void SetBusinessContext(BusinessObject bizObj)
		{
			bizObj.SetContext(BusinessContext.OverrideInvoiceReference);
		}
	}
}