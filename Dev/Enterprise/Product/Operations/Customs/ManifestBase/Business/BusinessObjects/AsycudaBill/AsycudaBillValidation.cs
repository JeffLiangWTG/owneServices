//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAsycudaBillValidation
//
//    This class should be used for overriding validation in AutoAsycudaBillValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaBillValidation : AutoAsycudaBillValidation
	{
		public AsycudaBillValidation(AutoAsycudaBill parent) : base(parent)
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

		protected override void CheckABL_NetWeightUQ()
		{
			base.CheckABL_NetWeightUQ();
			if (IsMandatoryForNetWeightUQ)
			{
				MandatoryValidation.CheckUnitEntered(Parent.ABL_NetWeightUQInfo, Parent.ABL_NetWeightInfo);
			}
		}

		protected override void CheckABL_RX_NKGoodsValueCurrency()
		{
			base.CheckABL_RX_NKGoodsValueCurrency();
			var info = Parent.ABL_RX_NKGoodsValueCurrencyInfo;

			ListValidation.ErrorIfInvalidCode(info);
			if (!Parent.ABL_GoodsValue.IsEmpty)
			{
				MandatoryValidation.CheckEntered(info);
			}
		}

		protected new AsycudaBill Parent => (AsycudaBill)base.Parent;

		protected virtual bool IsMandatoryForNetWeightUQ => true;
	}
}

