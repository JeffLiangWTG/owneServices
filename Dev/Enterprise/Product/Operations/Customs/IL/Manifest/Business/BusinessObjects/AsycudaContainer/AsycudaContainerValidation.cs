
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaContainerValidation : ASYCUDA.Business.AsycudaContainerValidation
	{
		public AsycudaContainerValidation(ASYCUDA.Business.AsycudaContainer parent) : base(parent)
		{
		}

		protected new AsycudaContainer Parent => (AsycudaContainer)base.Parent;

		protected override void CheckACN_RC_ContainerType()
		{
			base.CheckACN_RC_ContainerType();
			var parent = Parent;
			if (parent.ContainerType is not null
				&& parent.ContainerType.RC_ISOType.IsEmpty)
			{
				parent.ACN_RC_ContainerTypeInfo.AddMessageError(ValidationCaptions.AsycudaContainer.ContainerTypeWithoutISOType);
			}
		}
	}
}
