using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class OverrideTransactionAgreedPaymentMethodHelper : OverrideInvoiceDetailsHelper
	{
		public OverrideTransactionAgreedPaymentMethodHelper(BusinessObjectFactory factory, params ZGuid[] transactionPKs)
			: this(factory, null, true, transactionPKs)
		{
		}

		public OverrideTransactionAgreedPaymentMethodHelper(BusinessObjectFactory formFactory, BusinessObjectFactory parentFactory, bool canBizOBeSaved, params ZGuid[] transactionPKs)
			: base(formFactory, parentFactory, canBizOBeSaved, transactionPKs)
		{
		}

		protected override string[] ColumnsToOverride()
		{
			var trans = this.WrappedObjects.FirstOrDefault() as AccTransactionHeader;
			if (trans == null || !(trans is InvoicingBase || trans is Journal.Journal))
			{
				return null;
			}
			var ledger = trans.AH_Ledger;

			var columns = new List<string>();

			if (Env.Security.PayablesModifyAgreedPaymentMethodForPosted.IsAllowed && ledger == LedgerTypes.AccountsPayable
			|| Env.Security.ReceivablesModifyAgreedPaymentMethodForPosted.IsAllowed && ledger == LedgerTypes.AccountsReceivable)
			{
				columns.Add(AccTransactionHeaderSchema.AH_AgreedPaymentMethodOverride.Name);
			}

			if (Env.Security.PayablesModifyDueDateForPosted.IsAllowed && ledger == LedgerTypes.AccountsPayable
			|| Env.Security.ReceivablesModifyDueDateForPosted.IsAllowed && ledger == LedgerTypes.AccountsReceivable)
			{
				columns.Add(AccTransactionHeaderSchema.AH_DueDate.Name);
			}

			return columns.ToArray();
		}

		protected override void SetBusinessContext(BusinessObject bizObj)
		{
			bizObj.SetContext(BusinessContext.OverrideTransactionAgreedPaymentMethod);
		}
	}
}
