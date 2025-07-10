using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AsycudaBillScreeningValidation : ASYCUDA.Business.AsycudaBillScreeningValidation
	{
		public AsycudaBillScreeningValidation(AutoAsycudaBillScreening parent)
			: base(parent)
		{
		}

		protected new AsycudaBillScreening Parent => (AsycudaBillScreening)base.Parent;

		protected override void CheckASR_Result()
		{
			base.CheckASR_Result();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ASR_ResultInfo);
		}

		protected override void CheckASR_PER_AuthorizedPerson()
		{
			base.CheckASR_PER_AuthorizedPerson();

			if (!Parent.ASR_AuthorizedPersonType.IsEmpty && Parent.ASR_AuthorizedPersonType != EUICS2ScreeningAuthorizedPersonTypes.Codes.AP3)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ASR_PER_AuthorizedPersonInfo);
			}
		}

		protected override void CheckASR_AuthorizedPersonType()
		{
			base.CheckASR_AuthorizedPersonType();

			if (!CheckIfAuthorizedPersonFieldsAreAllPopulatedOrAllEmpty())
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ASR_AuthorizedPersonTypeInfo);
			}
		}

		protected override void CheckASR_AuthorizedPersonName()
		{
			base.CheckASR_AuthorizedPersonName();

			if (!CheckIfAuthorizedPersonFieldsAreAllPopulatedOrAllEmpty())
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ASR_AuthorizedPersonNameInfo);
			}
		}

		protected override void CheckASR_AuthorizedPersonIdentifier()
		{
			base.CheckASR_AuthorizedPersonIdentifier();

			if (!CheckIfAuthorizedPersonFieldsAreAllPopulatedOrAllEmpty())
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ASR_AuthorizedPersonIdentifierInfo);
			}
		}

		bool CheckIfAuthorizedPersonFieldsAreAllPopulatedOrAllEmpty()
		{
			var allPopulated = !Parent.ASR_AuthorizedPersonType.IsEmpty && !Parent.ASR_AuthorizedPersonName.IsEmpty && !Parent.ASR_AuthorizedPersonIdentifier.IsEmpty;
			var allEmpty = Parent.ASR_AuthorizedPersonType.IsEmpty && Parent.ASR_AuthorizedPersonName.IsEmpty && Parent.ASR_AuthorizedPersonIdentifier.IsEmpty;
			return allPopulated || allEmpty;
		}

		internal void ValidateAuthorizedPersonFieldsRequirement()
		{
			ValidateASR_PER_AuthorizedPerson();
			ValidateASR_AuthorizedPersonType();
			ValidateASR_AuthorizedPersonName();
			ValidateASR_AuthorizedPersonIdentifier();
		}

		protected override void CheckASR_TransportNumberType()
		{
			var parent = Parent;
			ListValidation.MessageErrorIfInvalidCode(parent.ASR_TransportNumberTypeInfo);
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(parent.ASR_TransportNumberTypeInfo, parent.ASR_TransportNumberInfo);
		}

		protected override void CheckASR_TransportNumber()
		{
			var parent = Parent;
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(parent.ASR_TransportNumberInfo, parent.ASR_TransportNumberTypeInfo);
		}
	}
}
