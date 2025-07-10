using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Netting
{
	public class NettingSystemExchangeRate : AutoNettingSystemExchangeRate
	{
		public NettingSystemExchangeRate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			NER_Rate = 1M;
			NER_RateType = "NET";
			NER_RX_NKCurrency = "XXX";
		}
#endif
	}
}
