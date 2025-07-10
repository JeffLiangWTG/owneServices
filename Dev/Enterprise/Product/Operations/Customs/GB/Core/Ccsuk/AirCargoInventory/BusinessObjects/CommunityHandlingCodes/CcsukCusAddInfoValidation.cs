//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCcsukCusAddInfoValidation
//
//    This class should be used for overriding validation in AutoCcsukCusAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	using CargoWise.EntityFramework;

	public class CcsukCusAddInfoValidation : AutoCcsukCusAddInfoValidation
	{
		public CcsukCusAddInfoValidation(AutoCcsukCusAddInfo parent) : base(parent)
		{
		}

		protected override void CheckC4_CommunityHandlingCode()
		{
			base.CheckC4_CommunityHandlingCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.C4_CommunityHandlingCodeInfo);
			ValidateC4_SplitReferenceToWhichThisPertains();
		}

		protected override void CheckC4_SplitReferenceToWhichThisPertains()
		{
			base.CheckC4_SplitReferenceToWhichThisPertains();
			if (!Parent.C4_SplitReferenceToWhichThisPertainsInfo.ReadOnly)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.C4_SplitReferenceToWhichThisPertainsInfo);
				if (Parent.C4_SplitReferenceToWhichThisPertains.IsEmpty)
				{
					Parent.C4_SplitReferenceToWhichThisPertainsInfo.AddWarning("This CHC will pertain to the whole (H)AWB, not an individual split");
				}
			}
		}
	}
}
