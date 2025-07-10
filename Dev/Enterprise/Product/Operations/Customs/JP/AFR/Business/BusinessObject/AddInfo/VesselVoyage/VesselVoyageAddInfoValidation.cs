//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoVesselVoyageAddInfoValidation
//
//    This class should be used for overriding validation in AutoVesselVoyageAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.JP.AFR.Business
{
	public class VesselVoyageAddInfoValidation : AutoVesselVoyageAddInfoValidation
	{
		public VesselVoyageAddInfoValidation(AutoVesselVoyageAddInfo parent) : base(parent)
		{
		}
	}
}
