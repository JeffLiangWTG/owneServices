using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public interface ITaxFrameworkBOLoader
	{
		IEnumerable<TransactionLineForOtherTaxesDisplay> LoadTransactionLineForOtherTaxesDisplay(BusinessObjectFactory factory, ZQuery query);
		ITaxRecordParent LoadTaxRecordParent(BusinessObjectFactory factory, ZGuid pk);
	}
}
