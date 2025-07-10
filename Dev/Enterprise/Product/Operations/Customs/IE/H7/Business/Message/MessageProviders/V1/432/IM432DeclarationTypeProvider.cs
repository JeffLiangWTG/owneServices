using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class IM432DeclarationTypeProvider : IIM432DeclarationType
	{
		readonly AsycudaBill bill;

		public IM432DeclarationTypeProvider(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
		}

		public string LRN => bill.LocalReferenceNumber;

		public ICustomsOffices02 CustomsOffices02 => CachedValueHelper.GetValue(ref customsOffice02Cached, () => new CustomsOffices02Provider(bill.Header));
		CachedValue<ICustomsOffices02> customsOffice02Cached;

		public IIM432PartiesType Parties => CachedValueHelper.GetValue(ref partiesCached, () => IM432PartiesTypeProvider.NewOrNull(bill.Header));
		CachedValue<IIM432PartiesType> partiesCached;
	}
}
