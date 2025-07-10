//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAURecommendationLetterAddInfoValidation
//
//    This class should be used for overriding validation in AutoAURecommendationLetterAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AURecommendationLetterAddInfoValidation : AutoAURecommendationLetterAddInfoValidation
	{
		public AURecommendationLetterAddInfoValidation(AutoAURecommendationLetterAddInfo parent) : base(parent)
		{
		}

		protected override void CheckZA_LetterNumber()
		{
			base.CheckZA_LetterNumber();

			var header = ((RecommendationLetter)Parent.Parent).Header;
			if (header != null && header.QH_ProduceType != EXDOCCommodityCodes.Codes.Meat && !Parent.ZA_LetterNumber.IsEmpty)
			{
				Parent.ZA_LetterNumberInfo.AddMessageError(LetterDetailsShouldNotBeEntered);
			}
			else
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ZA_LetterNumberInfo);
			}

			ValidateZA_LetterDate();
		}

		protected override void CheckZA_LetterDate()
		{
			base.CheckZA_LetterDate();

			if (!Parent.ZA_LetterDate.IsEmpty)
			{
				var header = ((RecommendationLetter)Parent.Parent).Header;
				if (header != null && header.QH_ProduceType != EXDOCCommodityCodes.Codes.Meat)
				{
					Parent.ZA_LetterDateInfo.AddMessageError(LetterDetailsShouldNotBeEntered);
				}
				else if (Parent.ZA_LetterNumber.IsEmpty)
				{
					Parent.ZA_LetterDateInfo.AddMessageError(LetterNumberRequired);
				}
			}
		}

		internal const string LetterNumberRequired = "Letter Date should only be entered when a corresponding Letter Number exists.";
		internal const string LetterDetailsShouldNotBeEntered = "Recommendation Letter details are only required for MEAT.";
	}
}
