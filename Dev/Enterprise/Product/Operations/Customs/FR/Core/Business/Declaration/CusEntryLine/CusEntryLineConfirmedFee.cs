using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class CusEntryLineConfirmedFee : NonPersistentBusinessObject
	{
		public CusEntryLineConfirmedFee(ZString description, ZDecimal chargeAmount, ZString chargeCurrency)
		{
			Description = description;
			Amount = chargeAmount;
			Currency = chargeCurrency;
		}

		public ZDecimal Amount { get; set; }

		public ZString Description { get; set; }

		public ZString Currency { get; set; }
	}
}
