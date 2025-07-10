using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class ClearanceLocalInvolvedPartyRequirement : JobDocAddressRequirement
	{
		public ClearanceLocalInvolvedPartyRequirement() : base(DocAddressType.ClearanceLocalInvolvedParty)
		{
			GetRegistrationNumberResult = GetRegNumResult;
			LookupsGovRegNumTypes = GetGovRegTypes;
		}

		RegistrationNumberResult GetRegNumResult(JobDocAddress docAddress)
		{
			return new RegistrationNumberResult(docAddress.Factory, true,
				delegate
				{
					var result = new RegistrationNumber() { NumberType = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ };
					if (docAddress != null && !docAddress.IsDeleted
						&& docAddress.Organisation?.PrimaryRegistrationNumber is OrgRegistrationNumber registrationNumber)
					{
						result.NumberType = registrationNumber.NumberType;
						result.Number = registrationNumber.Number;
					}
					return result;
				});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		CodeDescriptionPairList GetGovRegTypes(JobDocAddressLookups lookups)
		{
			var brGovRegType = new CodeDescriptionPairList();
			brGovRegType.AddPair(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "CNPJ Cadastro Nacional da Pessoa Jurídica");
			brGovRegType.AddPair(BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration, "Individual Tax Payer Registration");

			return brGovRegType;
		}
	}
}
