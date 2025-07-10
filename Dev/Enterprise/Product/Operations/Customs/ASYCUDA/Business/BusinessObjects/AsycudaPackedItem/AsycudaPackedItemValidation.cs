using CargoWise.EntityFramework;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaPackedItemValidation : ManifestBase.AsycudaPackedItemValidation
	{
		public AsycudaPackedItemValidation(AsycudaPackedItem parent)
			: base(parent)
		{
		}

		protected new AsycudaPackedItem Parent => (AsycudaPackedItem)base.Parent;

		protected override void CheckAPI_RN_NKGoodsOrigin()
		{
			base.CheckAPI_RN_NKGoodsOrigin();
			ListValidation.MessageErrorIfInvalidCode(Parent.API_RN_NKGoodsOriginInfo);
		}

		protected override void CheckAPI_CustomsQty()
		{
			base.CheckAPI_CustomsQty();
			if (Parent.API_CustomsQty < 0)
			{
				Parent.API_CustomsQtyInfo.AddError(NegativeAmountNotAllowed);
			}
		}

		internal const string NegativeAmountNotAllowed = "Please enter a non-negative value.";

		protected override void CheckAPI_CustomsUQ()
		{
			base.CheckAPI_CustomsUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.API_CustomsUQInfo);
			CheckMandatoryAPI_CustomsUQ();
		}

		protected virtual void CheckMandatoryAPI_CustomsUQ()
		{
			if (!Parent.API_CustomsQty.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.API_CustomsUQInfo);
			}
		}

		protected override void CheckAPI_NetWeight()
		{
			base.CheckAPI_NetWeight();
			if (Parent.API_NetWeight < 0)
			{
				Parent.API_NetWeightInfo.AddError(NegativeAmountNotAllowed);
			}
		}

		protected override void CheckAPI_NetWeightUQ()
		{
			base.CheckAPI_NetWeightUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.API_NetWeightUQInfo);
			if (!Parent.API_NetWeight.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.API_NetWeightUQInfo);
			}
		}

		protected override void CheckAPI_CustomsValue()
		{
			base.CheckAPI_CustomsValue();
			if (Parent.API_CustomsValue < 0)
			{
				Parent.API_CustomsValueInfo.AddError(NegativeAmountNotAllowed);
			}
		}

		protected override void CheckAPI_GrossWeight()
		{
			base.CheckAPI_GrossWeight();
			if (Parent.API_GrossWeight < 0)
			{
				Parent.API_GrossWeightInfo.AddError(NegativeAmountNotAllowed);
			}
		}

		protected override void CheckAPI_GrossWeightUQ()
		{
			base.CheckAPI_GrossWeightUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.API_GrossWeightUQInfo);
			if (!Parent.API_GrossWeight.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.API_GrossWeightUQInfo);
			}
		}

		protected override void CheckAPI_GoodsValue()
		{
			base.CheckAPI_GoodsValue();
			if (Parent.API_GoodsValue < 0)
			{
				Parent.API_GoodsValueInfo.AddError(NegativeAmountNotAllowed);
			}
		}

		protected override void CheckAPI_RX_NKGoodsValueCurrency()
		{
			base.CheckAPI_RX_NKGoodsValueCurrency();
			ValidateGoodsValueCurrencyCore();
			if (!Parent.API_GoodsValue.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.API_RX_NKGoodsValueCurrencyInfo);
			}
		}

		protected virtual void ValidateGoodsValueCurrencyCore() => ListValidation.MessageErrorIfInvalidCode(Parent.API_RX_NKGoodsValueCurrencyInfo);

		protected override void CheckAPI_Tariff()
		{
			base.CheckAPI_Tariff();
			TariffListValidationCore();
		}

		protected virtual void TariffListValidationCore()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.API_TariffInfo);
		}

		protected override void CheckAPI_TaxAmount()
		{
			base.CheckAPI_TaxAmount();
			if (Parent.API_TaxAmount < 0)
			{
				Parent.API_TaxAmountInfo.AddError(NegativeAmountNotAllowed);
			}
		}
	}
}
