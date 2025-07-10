using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.Universal.AsycudaUniversalReference;

namespace Enterprise.Customs.AE.Manifest.Business;

public sealed class AsycudaPackValidation : ASYCUDA.Business.AsycudaPackValidation
{
	public AsycudaPackValidation(AsycudaPack parent) : base(parent)
	{
	}

	new AsycudaPack Parent => (AsycudaPack)base.Parent;

	protected override void CheckAPA_PackQty()
	{
		base.CheckAPA_PackQty();
		MandatoryValidation.MessageErrorIfIsZero(Parent.APA_PackQtyInfo);
	}

	protected override bool IsPackQtyRequired() => false;

	protected override void CheckAPA_PackUQ()
	{
		base.CheckAPA_PackUQ();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.APA_PackUQInfo);
		if (!Parent.APA_PackUQ.IsEmpty)
		{
			CusRefPackLoaderHelper.MessageErrorIfNeeded(CountryCodes.UnitedArabEmirates, Parent.APA_PackUQ, Parent.APA_PackUQInfo, Parent.Factory);
		}
	}

	protected override void CheckAPA_WeightUQ()
	{
		base.CheckAPA_WeightUQ();
		CheckIsUNCodeList20Code(Parent.APA_WeightUQInfo);
	}

	protected override void CheckAPA_VolumeUQ()
	{
		base.CheckAPA_VolumeUQ();
		CheckIsUNCodeList20Code(Parent.APA_VolumeUQInfo);
	}

	protected override void CheckAPA_MarksAndNumbers()
	{
		base.CheckAPA_MarksAndNumbers();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.APA_MarksAndNumbersInfo);
	}

	protected override void CheckAPA_GoodsDescription()
	{
		base.CheckAPA_GoodsDescription();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.APA_GoodsDescriptionInfo);
	}

	void CheckIsUNCodeList20Code(ZPropertyInfo targetInfo)
	{
		var targetValue = targetInfo.Value;
		if (targetValue is ZString cw1UQ && !cw1UQ.IsEmpty)
		{
			var customsUQ = AEUniversalLookupsHelper.GetUN20CodeCustomsUQ(Parent.Factory, cw1UQ);
			if (customsUQ.IsEmpty)
			{
				targetInfo.AddMessageError(Res.GetString("0917B990-2162-48AB-82D2-8BAFC42912A0", "Unable to convert UOM to a corresponding UN Code List 20 Code."));
			}
		}
	}
}
