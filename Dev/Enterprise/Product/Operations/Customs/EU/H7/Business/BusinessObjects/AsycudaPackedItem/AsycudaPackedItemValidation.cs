using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.H7.Business
{
	public class AsycudaPackedItemValidation : ASYCUDA.Business.AsycudaPackedItemValidation
	{
		public AsycudaPackedItemValidation(ASYCUDA.Business.AsycudaPackedItem parent) : base(parent)
		{
		}

		protected new AsycudaPackedItem Parent => (AsycudaPackedItem)base.Parent;

		protected override void CheckAPI_Tariff()
		{
			base.CheckAPI_Tariff();
			var value = Parent.API_Tariff;
			var propertyInfo = Parent.API_FormattedTariffInfo;
			if (value.IsEmpty)
			{
				var notEnteredMessageError = Res.GetString("b1aed5d8-8ca6-4ac2-976f-e871f008b1eb",
					"You have not entered a Tariff.");
				propertyInfo.AddMessageError(notEnteredMessageError);
				return;
			}

			if (value.Length < 6 || value is { IsLettersAndNumbersOnlyOrEmpty: false })
			{
				var notAlphanumericMessageError = Res.GetString("c8d49f14-5fb3-4e6e-ad66-ef4fe4f8bcba",
					"The Tariff value entered must be at least 6 alphanumeric characters.");
				propertyInfo.AddMessageError(notAlphanumericMessageError);
			}
		}

		protected override void TariffListValidationCore()
		{
			var filter = new ZQuery();
			filter.AddToFilter(Parent.Lookups.TariffList.CompleteFilter);
			filter.AddToFilter(TariffViewSchema.ZZ1_TariffCode, SQLComparisonOperator.StartsWith, Parent.API_Tariff);

			if (Parent.API_Tariff.Length % 2 != 0 || !Parent.Factory.Exists(typeof(TariffView), filter))
			{
				Parent.API_TariffInfo.AddMessageError(TariffListValidationErrorMessage);
			}
		}

		protected override void CheckAPI_CustomsQty2()
		{
			base.CheckAPI_CustomsQty2();
			CompareValidation.CheckNumberNotNegative(Parent.API_CustomsQty2Info);

			var value = Parent.API_CustomsQty2;
			var propertyInfo = Parent.API_CustomsQty2Info;
			int maxDataStringLength = value.DecimalPlaces > 0 ? 17 : 16;
			if (value.ToString().Length > maxDataStringLength)
			{
				var decimalPlacesMessageError = Res.GetString("9c819cbe-685b-441a-b880-b77458358153",
					"The maximum permitted total digits are 16. Out of them, a max of six can be decimal digits.");
				propertyInfo.AddMessageError(decimalPlacesMessageError);
			}
		}

		protected override void CheckAPI_CustomsUQ2()
		{
			base.CheckAPI_CustomsUQ2();
			ListValidation.MessageErrorIfInvalidCode(Parent.API_CustomsUQ2Info);
		}

		protected override void CheckAPI_GoodsDescription()
		{
			base.CheckAPI_GoodsDescription();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.API_GoodsDescriptionInfo);
		}

		protected override void CheckMandatoryAPI_CustomsUQ()
		{
			if (!Parent.API_CustomsQty.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.API_CustomsQtyInfo);
			}
		}

		protected override void ValidateGoodsValueCurrencyCore()
		{
			ListValidation.ErrorIfInvalidCode(
				ResString.GetMultilingualString("7d975f6d-4457-4643-9c7b-4db0a164261e", "Enter a valid Intrinsic Value Currency."),
				Parent.API_RX_NKGoodsValueCurrencyInfo);
			MandatoryValidation.MessageErrorIfNotEntered(Parent.API_RX_NKGoodsValueCurrencyInfo);
		}

		protected override void CheckAPI_GoodsValue()
		{
			base.CheckAPI_GoodsValue();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.API_GoodsValueInfo);

			if (CheckTotalIntrinsicValueOfGoodsIsExceed(out string messageOfValueExceed))
			{
				Parent.API_GoodsValueInfo.AddMessageError(messageOfValueExceed);
			}
		}

		protected virtual bool CheckTotalIntrinsicValueOfGoodsIsExceed(out string messageOfValueExceed)
		{
			var bill = Parent.Bill;
			if (bill.AdditionalProcedureContainsC07() && bill.SumOfGoodsValue > 150m)
			{
				messageOfValueExceed = Res.GetString("7f440484-f7c0-44d3-b1a1-ca155e49f4cc", "Sum of Intrinsic Value (Items) must not exceed EUR 150 when Add. Procedure(s) contains C07.");
				return true;
			}
			else if (bill.ABL_Procedure == EUH7AdditionalProcedureCodeList.Codes.C08 && bill.SumOfGoodsValue > 45m)
			{
				messageOfValueExceed =  Res.GetString("50fa25b2-d302-4c67-91f6-b6b4d868d38a", "Sum of Intrinsic Value (Items) must not exceed EUR 45 when Add. Procedure(s) is C08.");
				return true;
			}

			messageOfValueExceed = string.Empty;
			return false;
		}

		protected virtual string TariffListValidationErrorMessage => ListValidation.InvalidCodeMessageError.ToString();
	}
}
