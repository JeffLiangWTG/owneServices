using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.CN.Business
{
	public class CustomsFeeWrapper : DocBaseWrapper
	{
		public CustomsFeeWrapper(CustomsFee fee, ZDateTime dateOfValuation, BusinessObjectFactory factory) : base(fee, factory)
		{
			this.fee = Argument.NotNull(fee, nameof(fee));
			this.dateOfValuation = dateOfValuation;
		}
		readonly CustomsFee fee;
		readonly ZDateTime dateOfValuation;

		public ZDecimal Amount => fee.Amount;

		public CodeAndDescriptionWrapper Currency
		{
			get
			{
				if (fCurrency == null)
				{
					var code = fee.CurrencyCode;
					fCurrency = CodeAndDescriptionWrapper.New(Factory, code, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Currency, dateOfValuation);
				}
				return fCurrency;
			}
		}
		CodeAndDescriptionWrapper fCurrency;

		public CodeAndDescriptionWrapper FeeMark => fFeeMark ?? (fFeeMark = CodeAndDescriptionWrapper.New(fee.MarkCode, Factory.GetCachedValue<FeeMarkTypeList>(), Factory));
		public CodeAndDescriptionWrapper fFeeMark;

		public ZString CurrencyAmountAndMarkCode => fee.IsEmpty || Amount.IsEmpty ? ZString.Empty : ZString.Format("{0}/{1}/{2}", Currency.Code, Amount.ToString(), FeeMark.Code);

		public ZString CurrencyAmountAndMarkDesc => fee.IsEmpty || Amount.IsEmpty ? ZString.Empty : ZString.Format("{0}/{1}/{2}", Currency.Description, Amount.ToString(), FeeMark.Description);
	}
}
