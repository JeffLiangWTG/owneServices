using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.ES.Business.CusTempStorage;

public class TemporaryStoragePackedItemValidation : EU.Business.CusTempStorage.TemporaryStoragePackedItemValidation
{
	public TemporaryStoragePackedItemValidation(AutoAsycudaPackedItem parent) : base(parent)
	{
	}

	const int MaxLineNo = 99999;

	public new TemporaryStoragePackedItem Parent => (TemporaryStoragePackedItem)base.Parent;

	protected override void CheckAPI_LineNo()
	{
		base.CheckAPI_LineNo();
		if (Parent.API_LineNo > MaxLineNo)
		{
			Parent.API_LineNoInfo.AddMessageError(Res.GetString("ADEA1A50-0C51-40CE-A73E-A5625F7CAE6D", "Line Nº should be lower than 99.999."));
		}
	}

	protected override bool ValidateDuplicatedTariff => false;
}
