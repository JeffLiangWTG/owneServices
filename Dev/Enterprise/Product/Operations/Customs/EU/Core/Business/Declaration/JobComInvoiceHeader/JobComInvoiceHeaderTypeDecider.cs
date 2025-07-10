using System;
using CargoWise.Integration;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class JobComInvoiceHeaderTypeDecider : Customs.Business.BaseJobComInvoiceHeaderTypeDecider
	{
		public override Type GetTypeForBinding()
		{
			var result = base.GetTypeForBinding();
			if (!typeof(JobComInvoiceHeader).IsAssignableFrom(result))
			{
				result = typeof(JobComInvoiceHeader);
			}
			return result;
		}

		protected override Type GetTypeForNewCore(ITypeDeciderContext context)
		{
			var result = base.GetTypeForNewCore(context);
			if (!typeof(JobComInvoiceHeader).IsAssignableFrom(result))
			{
				result = typeof(JobComInvoiceHeader);
			}
			return result;
		}
	}
}
