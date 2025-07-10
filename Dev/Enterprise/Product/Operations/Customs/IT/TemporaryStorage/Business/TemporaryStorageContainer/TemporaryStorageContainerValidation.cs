using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public class TemporaryStorageContainerValidation : EU.Business.CusTempStorage.TemporaryStorageContainerValidation
{
	public TemporaryStorageContainerValidation(TemporaryStorageContainer parent) : base(parent)
	{
	}

	protected new TemporaryStorageContainer Parent => (TemporaryStorageContainer)base.Parent;

	protected override void CheckACN_Seal1()
	{
		base.CheckACN_Seal1();

		var parent = Parent;
		if (parent.ACN_Seal1.IsEmpty)
		{
			if (!parent.ACN_ContainerNumber.IsEmpty && parent.ACN_Seal2.IsEmpty && parent.ACN_Seal3.IsEmpty)
			{
				parent.ACN_Seal1Info.AddWarning(ValidationCaptions.TemporaryStorageContainer.NoSealAvailableOrBulkGoods);
			}
		}
		else
		{
			SealNumberValidation.ValidateSealNumber(parent.ACN_Seal1, parent.ACN_Seal1Info);
		}
	}

	protected override void CheckACN_Seal2()
	{
		base.CheckACN_Seal2();

		SealNumberValidation.ValidateSealNumber(Parent.ACN_Seal2, Parent.ACN_Seal2Info);
	}

	protected override void CheckACN_Seal3()
	{
		base.CheckACN_Seal3();

		SealNumberValidation.ValidateSealNumber(Parent.ACN_Seal3, Parent.ACN_Seal3Info);
	}

	protected override void MandatoryValidationOfACN_RC_ContainerTypeCore()
	{
	}
}
