//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAsycudaContainerBillOrPackageLinkValidation
//
//    This class should be used for overriding validation in AutoAsycudaContainerBillOrPackageLinkValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaContainerBillOrPackageLinkValidation : AutoAsycudaContainerBillOrPackageLinkValidation
	{
		public AsycudaContainerBillOrPackageLinkValidation(AutoAsycudaContainerBillOrPackageLink parent) : base(parent)
		{
		}

		protected override void CheckAPC_ACN_Container()
		{
			base.CheckAPC_ACN_Container();
			if (Parent.APC_ABL_Bill.IsValid && Parent.APC_APA_Pack.IsValid)
			{
				Parent.APC_ACN_ContainerInfo.AddError(Res.GetString("5C4B578E-2063-4F9C-9EEC-CABCCB5DFAC8", "You can't provide a bill and a pack for same container."));
			}
		}
	}
}


