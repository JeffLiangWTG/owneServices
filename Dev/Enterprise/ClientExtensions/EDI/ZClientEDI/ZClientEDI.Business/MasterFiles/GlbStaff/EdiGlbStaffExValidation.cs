//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiGlbStaffExValidation
//
//    This class should be used for overriding validation in AutoEdiGlbStaffExValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Schema;

	public class EdiGlbStaffExValidation : AutoEdiGlbStaffExValidation
	{
		public EdiGlbStaffExValidation(AutoEdiGlbStaffEx parent) : base(parent)
		{
		}

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
		{
			if (info.Name == EdiGlbStaffExSchema.Constants.GS9_GS)
			{
				return false;
			}
			else
			{
				return base.ShouldValidateFKToCancelledRecord(info);
			}
		}
	}
}

