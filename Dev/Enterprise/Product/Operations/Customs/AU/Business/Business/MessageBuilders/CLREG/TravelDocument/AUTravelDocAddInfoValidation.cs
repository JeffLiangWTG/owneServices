//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAUTravelDocAddInfoValidation
//
//    This class should be used for overriding validation in AutoAUTravelDocAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUTravelDocAddInfoValidation : AutoAUTravelDocAddInfoValidation
	{
		public AUTravelDocAddInfoValidation(AutoAUTravelDocAddInfo parent) : base(parent)
		{
		}

		protected override void CheckZA_DocumentNo()
		{
			base.CheckZA_DocumentNo();
			if (Parent.ZA_DocumentNo.IsEmpty)
			{
				Parent.ZA_DocumentNoInfo.AddMessageError(DocumentNoRequired);
			}
		}
		internal const string DocumentNoRequired = "Document Number cannot be empty.";

		protected override void CheckZA_Country()
		{
			base.CheckZA_Country();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZA_CountryInfo, Parent.Lookups.Countries);
		}
	}
}
