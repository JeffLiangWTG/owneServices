using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class TCLPCOViewValidation : LPCOViewValidation
	{
		public TCLPCOViewValidation(LPCOView parent, IPGAHeader pgaHeader)
			: base(parent, pgaHeader)
		{
		}

		TCPGAHeader Header => PGAHeader as TCPGAHeader;

		protected override void CheckCLP_RN_NKAuthorizationCountry()
		{
			base.CheckCLP_RN_NKAuthorizationCountry();

			if (Header.IsVPR && Parent.CLP_Type == LPCODocumentTypeQualifier.Codes._4004)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CLP_RN_NKAuthorizationCountryInfo);
			}
		}
	}
}
