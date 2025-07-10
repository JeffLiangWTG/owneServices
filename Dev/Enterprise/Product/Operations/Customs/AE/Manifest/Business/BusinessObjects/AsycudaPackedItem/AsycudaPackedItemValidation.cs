using CargoWise.EntityFramework;

namespace Enterprise.Customs.AE.Manifest.Business;

public sealed class AsycudaPackedItemValidation : ASYCUDA.Business.AsycudaPackedItemValidation
{
	public AsycudaPackedItemValidation(AsycudaPackedItem parent) : base(parent)
	{
	}

	new AsycudaPackedItem Parent => (AsycudaPackedItem)base.Parent;

	protected override void CheckAPI_Tariff()
	{
		base.CheckAPI_Tariff();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.API_TariffInfo);
	}
}
