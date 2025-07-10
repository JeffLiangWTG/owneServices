//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAsycudaPackValidation
//
//    This class should be used for overriding validation in AutoAsycudaPackValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaPackValidation : AutoAsycudaPackValidation
	{
		public AsycudaPackValidation(AutoAsycudaPack parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateContainerPK();
		}

		public void ValidateContainerPK()
		{
			ValidateCalculatedProperty(Parent.ContainerPKInfo);
		}

		protected virtual void CheckContainerPK()
		{
			var pivot = Parent?.Pivot;
			if (pivot != null)
			{
				pivot.Validation.ValidateAPC_ACN_Container();
				Parent.ContainerPKInfo.AddAllNotificationsFrom(pivot.APC_ACN_ContainerInfo);
			}
		}

		protected new AsycudaPack Parent => (AsycudaPack)base.Parent;
	}
}
