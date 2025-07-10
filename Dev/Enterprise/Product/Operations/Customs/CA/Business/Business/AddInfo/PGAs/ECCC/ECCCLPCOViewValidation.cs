using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class ECCCLPCOViewValidation : LPCOViewValidation
	{
		public ECCCLPCOViewValidation(LPCOView parent, IPGAHeader pgaHeader)
			: base(parent, pgaHeader)
		{
		}

		protected override void CheckCLP_AlternativeQuotaQuantity()
		{
			base.CheckCLP_AlternativeQuotaQuantity();
			if (PGAHeader.IsProgramEnabled(ECCCPGADepartmentCodes.Codes.ODS))
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.CLP_AlternativeQuotaQuantityInfo);
			}
		}

		protected override void CheckCLP_DIFRefNumberOrLocation()
		{
			base.CheckCLP_DIFRefNumberOrLocation();
			if (PGAHeader.IsProgramEnabled(ECCCPGADepartmentCodes.Codes.WEN))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CLP_DIFRefNumberOrLocationInfo);
			}
		}

		protected override void CheckCLP_EndDate()
		{
			base.CheckCLP_EndDate();
			if (PGAHeader.IsProgramEnabled(ECCCPGADepartmentCodes.Codes.WEN))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CLP_EndDateInfo);
			}
		}

		protected override void CheckCLP_IssueDate()
		{
			base.CheckCLP_IssueDate();
			if (PGAHeader.IsProgramEnabled(ECCCPGADepartmentCodes.Codes.WEN))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CLP_IssueDateInfo);
			}
		}

		protected override void CheckCLP_RefNo()
		{
			base.CheckCLP_RefNo();

			if (PGAHeader.IsProgramEnabled(ECCCPGADepartmentCodes.Codes.ODS) && !Parent.CLP_RefNo.IsEmpty && !IsValidRefNoFormat(Parent.CLP_Type, Parent.CLP_RefNo))
			{
				Parent.CLP_RefNoInfo.AddMessageError(Res.GetString("20e8c4dc-2e77-483c-9ecb-201a8da5e575", @"Please input valid format as ""ODSHA-{0}-YY-###"", ""YY"" represents the two digit calendar year, and ""#"" should be an alpha-numeric.", GetRefNoQualifier(Parent.CLP_Type)));
			}
		}

		bool IsValidRefNoFormat(string documentType, string refNo)
		{
			var type = GetRefNoQualifier(documentType);
			return !type.IsEmpty && Regex.IsMatch(refNo, string.Format(CultureInfo.CurrentCulture, "^ODSHA-{0}-[0-9]{{2}}-[a-zA-Z0-9]{{3}}$", type));
		}

		ZString GetRefNoQualifier(string documentType)
		{
			switch (documentType)
			{
				case LPCODocumentTypeQualifier.Codes._8010:
					return "PER";
				case LPCODocumentTypeQualifier.Codes._8011:
					return "ALL";
				case LPCODocumentTypeQualifier.Codes._8012:
					return "TRA";
				default:
					return ZString.Empty;
			}
		}
	}
}
