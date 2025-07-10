using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CO.Manifest.Business
{
	public class AsycudaContainerValidation : ASYCUDA.Business.AsycudaContainerValidation
	{
		public AsycudaContainerValidation(AsycudaContainer parent)
			: base(parent)
		{
		}

		protected new AsycudaContainer Parent => (AsycudaContainer)base.Parent;

		protected override void CheckACN_RC_ContainerType()
		{
			base.CheckACN_RC_ContainerType();

			if (Parent.Header.AMA_ContainerMode == Core.Constants.ContainerModes.Containerised && !Parent.ACN_RC_ContainerType.IsEmpty)
			{
				var codeMapCollection = Parent.ContainerType?.CodeMapCollection.Cast<RefContainerCodeMap>();
				if (codeMapCollection != null && !codeMapCollection.Any(x => x.RCM_RN_NKCountry == Core.Constants.CountryCodes.Colombia))
				{
					Parent.ACN_RC_ContainerTypeInfo.AddMessageError(Res.GetString("C9AEF235-62C2-4890-804C-D17232F9998E", "The Container Type selected should have a Customs Container Code entered"));
				}
			}
		}
	}
}
