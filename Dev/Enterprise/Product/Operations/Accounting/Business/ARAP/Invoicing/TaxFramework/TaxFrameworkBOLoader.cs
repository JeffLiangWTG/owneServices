using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.TaxFramework.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework
{
	public class TaxFrameworkBOLoader : ITaxFrameworkBOLoader
	{
		IEnumerable<TransactionLineForOtherTaxesDisplay> ITaxFrameworkBOLoader.LoadTransactionLineForOtherTaxesDisplay(BusinessObjectFactory factory, ZQuery query)
		{
			return factory.Load<TransactionLine>(query).Cast<InvoicingLineBase>().Select(l => TaxFrameworkObjectFactory.GetInvoicingLineBaseForOtherTaxesDisplay(l));
		}

		ITaxRecordParent ITaxFrameworkBOLoader.LoadTaxRecordParent(BusinessObjectFactory factory, ZGuid pk)
		{
			return TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(factory.Load<InvoicingBase>(pk));
		}
	}
}
