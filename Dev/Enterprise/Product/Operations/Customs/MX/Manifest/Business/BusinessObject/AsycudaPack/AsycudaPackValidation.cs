using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class AsycudaPackValidation : ASYCUDA.Business.AsycudaPackValidation
	{
		public AsycudaPackValidation(AsycudaPack parent)
			: base(parent)
		{
		}

		protected new AsycudaPack Parent => (AsycudaPack)base.Parent;

		protected override void CheckAPA_CommodityCode()
		{
			base.CheckAPA_CommodityCode();

			if (Parent.APA_CommodityCode.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.APA_CommodityCodeInfo);
			}
			else
			{
				if (Parent.Bill.IsSea && Parent.APA_CommodityCode.Length != 6)
				{
					Parent.APA_CommodityCodeInfo.AddMessageError(ResString.GetMultilingualString("4C265053-E695-4B63-98C9-DD5CB3823746", "The length of the Commodity Code must be 6."));
				}
			}
		}

		protected override void CheckAPA_GoodsDescription()
		{
			base.CheckAPA_GoodsDescription();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.APA_GoodsDescriptionInfo);
		}

		protected override void CheckAPA_MarksAndNumbers()
		{
			base.CheckAPA_MarksAndNumbers();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.APA_MarksAndNumbersInfo);
		}

		protected override void CheckAPA_PackQty()
		{
			base.CheckAPA_PackQty();
			MandatoryValidation.MessageErrorIfIsZero(Parent.APA_PackQtyInfo);
		}

		protected override void CheckAPA_PackUQ()
		{
			base.CheckAPA_PackUQ();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.APA_PackUQInfo);
		}

		protected override void CheckAPA_WeightUQ()
		{
			base.CheckAPA_WeightUQ();
			if (Parent.APA_Weight > 0)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.APA_WeightUQInfo);
			}
		}

		protected override void CheckLinePriceCurrency()
		{
			base.CheckLinePriceCurrency();

			if (Parent.LinePrice > 0 && Parent.LinePriceCurrency != Core.Constants.CurrencyCodes.UnitedStates)
			{
				Parent.LinePriceCurrencyInfo.AddMessageError(ResString.GetMultilingualString("54B070CE-3AFA-4AB7-8B90-3C0251647A68", "The currency should be USD"));
			}
		}
	}
}
