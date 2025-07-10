using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class CusSealValidation : Customs.Business.CusSealValidation
	{
		public CusSealValidation(AutoCusSeal parent) : base(parent)
		{
		}

		protected override void CheckBK_SealNumber()
		{
			base.CheckBK_SealNumber();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.BK_SealNumberInfo);
		}

		protected override void CheckBK_SealType()
		{
			base.CheckBK_SealType();

			var parent = Parent;
			MandatoryValidation.MessageErrorIfNotEntered(parent.BK_SealTypeInfo);
			ListValidation.MessageErrorIfInvalidCode(parent.BK_SealTypeInfo);
		}

		protected override void CheckBK_UnloadingState()
		{
			base.CheckBK_UnloadingState();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.BK_UnloadingStateInfo);
		}

		protected override void CheckBK_SealingPartyType()
		{
			base.CheckBK_SealingPartyType();
			ListValidation.MessageErrorIfInvalidCode(Parent.BK_SealingPartyTypeInfo);
		}
	}
}
