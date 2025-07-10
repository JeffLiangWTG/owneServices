using System;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Interfaces;

namespace Enterprise.Accounting.Business.Base.Reversing.BadDebtWritingOff
{
	public class BadDebtWritingOffFactory
	{
		public ReversingBase NewWritingOff(IBadDebtWritingOff originalTransaction)
		{
			ReversingBase result = null;
			try
			{
				if (originalTransaction is InvoicingBase)
				{
					result = new InvoicingBaseWritingOff(originalTransaction);
				}
				else if (originalTransaction is IPayablesAndReceivables)
				{
					result = new PayablesAndReceivablesWritingOff(originalTransaction);
				}
			}
			catch (ArgumentNullException)
			{
				result = null;
			}
			return result;
		}
	}
}
