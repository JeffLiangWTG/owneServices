using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.H7.Business
{
	public class AsycudaPackValidation : ASYCUDA.Business.AsycudaPackValidation
	{
		public AsycudaPackValidation(ASYCUDA.Business.AsycudaPack parent) : base(parent)
		{
		}

		protected override void CheckAPA_PackQty()
		{
			var greaterThanZero = Res.GetString("a30e1560-29c7-4383-b8e5-497e9fa68342", "Quantity (on Pack) must be greater than 0.");

			if (Parent.APA_PackQty < 0)
			{
				Parent.APA_PackQtyInfo.AddError(greaterThanZero);
			}
			else if (Parent.APA_PackQty == 0)
			{
				Parent.APA_PackQtyInfo.AddMessageError(greaterThanZero);
			}
		}

		protected override void CheckAPA_PackUQ()
		{
			base.CheckAPA_PackUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.APA_PackUQInfo);
		}

		protected override void CheckAPA_GoodsDescription()
		{
			base.CheckAPA_GoodsDescription();
			var value = Parent.APA_GoodsDescription;
			var propertyInfo = Parent.APA_GoodsDescriptionInfo;

			if (!AllowNonAlphanumericCharactersForDescription && !value.IsLettersAndNumbersOnlyOrEmpty)
			{
				var alphanumericMessageError = Res.GetString("3ccc615f-ae3e-4e5e-9435-4556dd1825f3",
					"Goods Description can only be alphanumeric.");
				propertyInfo.AddMessageError(alphanumericMessageError);
			}

			if (value.Trim().Length > 512)
			{
				var exceedMaxLengthMessageError = Res.GetString("018a85f3-32f6-4224-a161-d16d01f37972",
					"Goods Description has exceeded the max length of 512.");
				propertyInfo.AddMessageError(exceedMaxLengthMessageError);
			}
		}

		protected virtual bool AllowNonAlphanumericCharactersForDescription => false;
	}
}
