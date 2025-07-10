//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiIdentityCertificateValidation
//
//    This class should be used for overriding validation in AutoEdiIdentityCertificateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.IdentityCertificate.Business
{
	public class EdiIdentityCertificateValidation : AutoEdiIdentityCertificateValidation
	{
		public EdiIdentityCertificateValidation(AutoEdiIdentityCertificate parent) : base(parent)
		{
		}

		EdiIdentityCertificate IdentityCertificate => certificate ??= (EdiIdentityCertificate)Parent;
		EdiIdentityCertificate certificate;

		protected override void CheckICE_CARoot()
		{
			if (IdentityCertificate.Application.IDA_LD.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.ICE_CARootInfo);
			}
			ListValidation.ErrorIfInvalidCode(Parent.ICE_CARootInfo);
			base.CheckICE_CARoot();
		}
	}
}
