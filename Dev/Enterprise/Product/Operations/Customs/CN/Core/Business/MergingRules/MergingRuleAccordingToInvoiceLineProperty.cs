using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public abstract class MergingRuleAccordingToInvoiceLineProperty
	{
		protected MergingRuleAccordingToInvoiceLineProperty(ZString propertyName)
		{
			this.propertyName = propertyName;
		}

		protected ZString propertyName;

		public IEnumerable<IZType> GetKeysForLine(JobComInvoiceLine invoiceLine)
		{
			yield return (IZType)invoiceLine[propertyName];
		}
	}
}
