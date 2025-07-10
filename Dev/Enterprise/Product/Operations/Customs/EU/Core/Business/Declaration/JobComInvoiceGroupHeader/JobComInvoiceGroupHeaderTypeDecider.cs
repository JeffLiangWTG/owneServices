using System;
using CargoWise.Integration;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class JobComInvoiceGroupHeaderTypeDecider : Customs.Business.BaseJobComInvoiceGroupHeaderTypeDecider
	{
		public override Type GetTypeForBinding()
		{
			var result = base.GetTypeForBinding();
			if (!typeof(JobComInvoiceGroupHeader).IsAssignableFrom(result))
			{
				result = typeof(JobComInvoiceGroupHeader);
			}
			return result;
		}

		protected override Type GetTypeForNewCore(ITypeDeciderContext context)
		{
			var result = base.GetTypeForNewCore(context);
			if (!typeof(JobComInvoiceGroupHeader).IsAssignableFrom(result))
			{
				result = typeof(JobComInvoiceGroupHeader);
			}
			return result;
		}
	}
}
