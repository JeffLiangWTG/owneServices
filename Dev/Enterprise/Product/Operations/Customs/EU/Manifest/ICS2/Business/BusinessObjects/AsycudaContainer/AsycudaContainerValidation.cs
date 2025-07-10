namespace Enterprise.Customs.EU.Manifest.ICS2.Business;

public class AsycudaContainerValidation : ASYCUDA.Business.AsycudaContainerValidation
{
	public AsycudaContainerValidation(ASYCUDA.Business.AsycudaContainer parent) : base(parent)
	{
	}

	protected override void CheckACN_RC_ContainerType()
	{
		base.CheckACN_RC_ContainerType();

		var parent = Parent;
		var containerType = parent.ContainerType;
		if (containerType != null && containerType.RC_ISOEquipmentSizeTypeCode.IsEmpty)
		{
			parent.ACN_RC_ContainerTypeInfo.AddError(Res.GetString("A10573D2-AFD6-4C55-950B-5B52CC3421B5", "The entered Container Type does not have an Equipment Size Type Code."));
		}
	}
}
