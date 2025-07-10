using CargoWise.ComponentModel;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class OrgCusCodeValidation : MasterFiles.Business.OrgCusCodeValidation, Integration.Customs.BR.IOrgCusCodeValidation
	{
		public OrgCusCodeValidation(AutoOrgCusCode parent) : base(parent)
		{
		}

		protected override void CheckOK_CodeType()
		{
			base.CheckOK_CodeType();
			var parent = Parent as OrgCusCode;
			var codeType = parent.OK_CodeType;
			var targetInfo = parent.OK_CodeTypeInfo;

			if (!targetInfo.HasErrors() && parent.Organisation is OrgHeader header)
			{
				if (codeType == BrazilOrgCusCodeInfo.OrgCusCodes.ForeignOperatorInternalCode)
				{
					if (header.CountryCode == Enterprise.Core.Constants.CountryCodes.Brazil)
					{
						targetInfo.AddError(Res.GetString("F28A35B0-C66A-486A-BE07-D1BB76D94604", "BR FOI code not necessary for Brazilian companies."));
					}
				}
				else if (codeType == BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ)
				{
					if (header.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Enterprise.Core.Constants.CountryCodes.Brazil, BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ).IsEmpty)
					{
						targetInfo.AddError(Res.GetString("0b0566e4-a8cf-4c72-a5f4-76464c574465", "Root CNPJ cannot be set if no CNPJ has been informed."));
					}
				}
			}
		}

		protected override void CheckOK_CustomsRegNo()
		{
			base.CheckOK_CustomsRegNo();
			var codeType = Parent.OK_CodeType;
			var targetInfo = Parent.OK_CustomsRegNoInfo;

			if (!targetInfo.HasErrors())
			{
				var number = Parent.OK_CustomsRegNo;

				if (codeType == BrazilOrgCusCodeInfo.OrgCusCodes.ForeignOperatorInternalCode)
				{
					if (number.Length > 35)
					{
						targetInfo.AddError(Res.GetString("8C7B23B3-12D8-4505-BCC0-6B56ADD36848", "BR FOI should not have more than 35 characters."));
					}
				}
				else if (codeType == BrazilOrgCusCodeInfo.OrgCusCodes.AEO)
				{
					if (number.Length != 14 || !number.IsNumbersOnlyOrEmpty)
					{
						targetInfo.AddWarning(Res.GetString("99764EC7-2B26-492A-8BCB-CE6062058BCE", "BR AEO should be 14 digits"));
					}
				}
			}
		}
	}
}
