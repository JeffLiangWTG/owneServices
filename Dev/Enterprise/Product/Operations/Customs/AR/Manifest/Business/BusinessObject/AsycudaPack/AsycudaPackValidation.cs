using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.AR.Manifest.Business
{
	public class AsycudaPackValidation : ASYCUDA.Business.AsycudaPackValidation
	{
		public AsycudaPackValidation(AsycudaPack parent)
			: base(parent)
		{
		}

		protected new AsycudaPack Parent => (AsycudaPack)base.Parent;

		protected override bool IsPackQtyRequired() => true;

		protected override void CheckAPA_PackUQ()
		{
			base.CheckAPA_PackUQ();
			if (Parent.APA_PackQty > 0)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.APA_PackUQInfo);
			}
		}

		protected override bool IsWeightRequired() => true;

		protected override bool IsVolumeRequired() => true;

		protected override void CheckAPA_VolumeUQ()
		{
			base.CheckAPA_VolumeUQ();
			if (Parent.APA_Volume > 0)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.APA_VolumeUQInfo);
			}
		}

		protected override void CheckAPA_CommodityCode()
		{
			base.CheckAPA_CommodityCode();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.APA_CommodityCodeInfo);
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
	}
}
