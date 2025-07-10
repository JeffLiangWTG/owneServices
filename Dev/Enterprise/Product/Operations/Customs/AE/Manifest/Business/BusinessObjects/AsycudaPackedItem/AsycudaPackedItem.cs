using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AE.Manifest.Business;

public class AsycudaPackedItem : ASYCUDA.Business.AsycudaPackedItem
{
	public AsycudaPackedItem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new AsycudaPackedItemValidation Validation => (AsycudaPackedItemValidation)base.Validation;

	protected override ManifestBase.AsycudaPackedItemValidation GetNewValidation() => new AsycudaPackedItemValidation(this);

	public override ZString API_Tariff
	{
		get => base.API_Tariff;
		set
		{
			var oldValue = API_Tariff;
			base.API_Tariff = value;
			if (oldValue != API_Tariff && !IsCopying)
			{
				if (UniversalTariff is { } tariff)
				{
					Pack.APA_GoodsDescription = tariff.ZZ1_Description.Left(AsycudaPack.Schema.APA_GoodsDescriptionMaxLength);
				}
			}
		}
	}
}
