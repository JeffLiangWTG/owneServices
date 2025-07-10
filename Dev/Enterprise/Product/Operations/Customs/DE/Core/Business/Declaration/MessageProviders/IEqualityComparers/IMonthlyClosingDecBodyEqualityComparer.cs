using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Business
{
	class IMonthlyClosingDecBodyEqualityComparer : IEqualityComparer<IMonthlyClosingDecBody>
	{
		public bool Equals(IMonthlyClosingDecBody px, IMonthlyClosingDecBody py) => ComparerHelper.Compare(px, py, (x, y) =>
			x.ConsigneePK == y.ConsigneePK &&
			string.Equals(x.DeliveryTermsCode, y.DeliveryTermsCode) &&
			string.Equals(x.DeliveryTermsDescription, y.DeliveryTermsDescription) &&
			string.Equals(x.DeliveryTermsKey, y.DeliveryTermsKey) &&
			string.Equals(x.DeliveryTermsPlace, y.DeliveryTermsPlace) &&
			new IMoneyEqualityComparer().Equals(x.PaymentTransaction, y.PaymentTransaction) &&
			string.Equals(x.ForeignTradeStatisticsEntryCustomsOffice, y.ForeignTradeStatisticsEntryCustomsOffice) &&
			new ICustomsValueEqualityComparer().Equals(x.CustomsValue, y.CustomsValue));

		public int GetHashCode(IMonthlyClosingDecBody obj) => 0;
	}
}
