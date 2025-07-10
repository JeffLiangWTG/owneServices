using System.Collections.Generic;
using Enterprise.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class MalaysiaIeInvoicingDiscardAdditionalPiviotActionTypesProvider : IEInvoicingDiscardAdditionalPiviotActionTypesProvider
	{
		public IEnumerable<string> GetAdditionalPivotActionTypes(string actionType)
		{
			var additionalPivotActionTypes = new List<string>();
			if (actionType == Constants.EInvoicingPivotActionType.StatusCheck)
			{
				additionalPivotActionTypes.Add(Constants.EInvoicingPivotActionType.DocumentDetail);
			}

			return additionalPivotActionTypes;
		}
	}
}
