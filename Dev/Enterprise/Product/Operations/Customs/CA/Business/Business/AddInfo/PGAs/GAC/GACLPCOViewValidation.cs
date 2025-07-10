using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class GACLPCOViewValidation : LPCOViewValidation
	{
		public GACLPCOViewValidation(LPCOView parent, IPGAHeader pgaHeader)
			: base(parent, pgaHeader)
		{
		}

		protected override void CheckCLP_AlternativeQuotaUQ()
		{
			base.CheckCLP_AlternativeQuotaUQ();
			var invoiceLine = (PGAHeader as GACPGAHeader).InvoiceLine;
			if (invoiceLine != null && !Parent.CLP_AlternativeQuotaUQ.IsEmpty)
			{
				var units = new List<ZString>() { invoiceLine.JI_CustomsUnitQty, invoiceLine.JI_CustomsSecondUnitQty, invoiceLine.JI_CustomsThirdUnitQty };
				if (!units.Any(x => !x.IsEmpty && x == Parent.CLP_AlternativeQuotaUQ))
				{
					Parent.CLP_AlternativeQuotaUQInfo.AddMessageError(Res.GetString("ae8df52a-e699-4054-8136-dd0779507509", "Unit of Measure must match the Customs Unit of Measure."));
				}
			}
		}

		protected override void CheckCLP_RefNo()
		{
			base.CheckCLP_RefNo();
			switch (Parent.CLP_Type)
			{
				case LPCODocumentTypeQualifier.Codes._2004:
					if (!Regex.IsMatch(Parent.CLP_RefNo, @"^GIP[0-9]{1,5}$"))
					{
						Parent.CLP_RefNoInfo.AddMessageError(Res.GetString("34ABBA91-D84C-45A5-AC29-62EFB85E63FE", "Ref No should be \"GIP\" followed by 1-5 digits"));
					}
					break;
				case LPCODocumentTypeQualifier.Codes._2006:
					if (!Regex.IsMatch(Parent.CLP_RefNo, @"^GIP8[01]$"))
					{
						Parent.CLP_RefNoInfo.AddMessageError(Res.GetString("B4E8AE51-B2D8-427F-9274-92769A86831E", "Ref No should be \"GIP80\" or \"GIP81\""));
					}
					break;
			}
		}

		protected override void CheckCLP_RN_NKSmeltAndPourCountryCode()
		{
			base.CheckCLP_RN_NKSmeltAndPourCountryCode();

			var lpco = Parent;
			if (lpco.CLP_RN_NKSmeltAndPourCountryCode.IsEmpty && lpco.CLP_Type == LPCODocumentTypeQualifier.Codes._2006 && Regex.IsMatch(lpco.CLP_RefNo, @"^GIP8[01]$"))
			{
				if (ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.GACMLT, Core.Constants.CountryCodes.Canada, ZDateTime.Today))
				{
					MandatoryValidation.MessageErrorIfNotEntered(lpco.CLP_RN_NKSmeltAndPourCountryCodeInfo);
				}
				else
				{
					MandatoryValidation.WarnIfNotEntered(lpco.CLP_RN_NKSmeltAndPourCountryCodeInfo);
				}
			}
		}
	}
}
