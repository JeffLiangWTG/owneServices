using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business
{
	public static class JobInvoicingReverserValidationHelper
	{
		static IEnumerable<ZPropertyInfo> GetPropertyInfosNeedToBeValidate(TransactionHeader reversedInvoice)
		{
			return new List<ZPropertyInfo> { reversedInvoice.AH_PostDateInfo };
		}

		public static void Validate(TransactionHeader reversedInvoice)
		{
			var propertyInfos = GetPropertyInfosNeedToBeValidate(reversedInvoice);
			foreach (var info in propertyInfos)
			{
				((IBusinessObjectInternals)reversedInvoice).Validate(info);
			}
		}

		public static bool HasErrors(TransactionHeader reversedInvoice)
		{
			var propertyInfos = GetPropertyInfosNeedToBeValidate(reversedInvoice);
			return propertyInfos.Any(x => x.HasErrors());
		}
	}
}
