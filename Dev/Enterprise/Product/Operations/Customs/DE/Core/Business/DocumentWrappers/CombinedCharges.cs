using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.DE.Business.DocumentWrappers
{
	public class CombinedCharges : DocBaseWrapper
	{
		CombinedCharges(BaseJobComInvHeaderCharge[] combinedValuesForCharges, BusinessObjectFactory factory) : base(combinedValuesForCharges, factory)
		{
		}

		public static CombinedCharges New(BaseJobComInvHeaderCharge[] combinedValuesForCharges, BusinessObjectFactory factoryToWrap) => combinedValuesForCharges is null ? null : new CombinedCharges(combinedValuesForCharges, factoryToWrap);

		//Art
		public ZString Type => WrappedObject[0].J7_ChargeType;

		//Bezeichnung
		public ZString Description => WrappedObject[0].Lookups.ChargeTypeList.GetMultilingualDescriptionFromCode(WrappedObject[0].J7_ChargeType)?.ToString(Core.SharedConstants.Languages.German);

		//Betrag in
		public ZString Amount => amount.ToStringRounded(2);

		ZDecimal amount => WrappedObject.Sum(c => c.J7_Amount);

		//Währung
		public ZString Currency => WrappedObject[0].J7_RX_NKCurrency;

		//Betrag in EUR
		public ZString AmountEUR
		{
			get
			{
				var result = ZDecimal.Zero;
				if (rate > ZDecimal.Zero)
				{
					result = amount / rate;
				}
				return result.ToStringRounded(2);
			}
		}

		//Kurs
		public ZString Rate => rate.ToStringRounded(6);

		ZDecimal rate => WrappedObject[0].J7_ExchangeRate;

		//Datum
		public ZString RateDate => WrappedObject[0].J7_ExchangeRateDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);

		new BaseJobComInvHeaderCharge[] WrappedObject => (BaseJobComInvHeaderCharge[])base.WrappedObject;
	}
}
