using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class HCLPCOViewValidation : LPCOViewValidation
	{
		public HCLPCOViewValidation(LPCOView parent, IPGAHeader pgaHeader)
			: base(parent, pgaHeader)
		{
		}

		protected new HCPGAHeader PGAHeader => base.PGAHeader as HCPGAHeader;

		protected override void CheckCLP_RefNo()
		{
			if (Parent.CLP_RefNo.IsEmpty
				&& PGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.BBC)
				&& Parent.CLP_Type == LPCODocumentTypeQualifier.Codes._5003)
			{
				return;
			}

			base.CheckCLP_RefNo();
		}

		protected override void CheckCLP_DIFRefNumberOrLocation()
		{
			if (Parent.CLP_DIFRefNumberOrLocation.IsEmpty
				&& PGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.BBC)
				&& Parent.CLP_Type == LPCODocumentTypeQualifier.Codes._5003)
			{
				Parent.CLP_DIFRefNumberOrLocationInfo.AddWarning(Res.GetString("fa5fdd7c-fc62-46f6-9f13-0399f66fd560", "It is strongly recommended to provide an image of the Proof of Prescription as this will facilitate communication in case of a referral."));
				return;
			}

			if (PGAHeader.IsProgramEnabled(HCPGADepartmentCodes.Codes.DSE))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CLP_DIFRefNumberOrLocationInfo);
			}

			base.CheckCLP_DIFRefNumberOrLocation();
		}
	}
}
