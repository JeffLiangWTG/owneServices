using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Eu = Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Declaration
{
	public class CDSFiscalRepresentativeDefaulter : Eu.FiscalRepresentativeDefaulter
	{
		protected override string GetFiscalReferenceDefaultCodeCore() => FiscalReferenceCodeList.Codes.FR1_Importer;

		protected override (OrgHeader, ZString) GetFiscalReferenceRelatedPartyCore(Eu.CusEntryInstruction cei)
		{
			OrgHeader owner = null;
			ZString ownerReference = ZString.Empty;
			if (cei.JobDeclaration is JobDeclaration declaration)
			{
				var importer = declaration.Importer;
				var cpvRelatedParty = importer?.AllRelatedParties.Cast<OrgRelatedParty>()
					.FirstOrDefault(orp => orp.PR_PartyType == RelatedPartyTypeList.Codes.CustomsPostponedVatAccounting);

				owner = cpvRelatedParty?.RelatedParty ?? importer;
				ownerReference = owner.GetVATRegistrationNumberWithCountryCodePrefix(Core.Constants.CountryCodes.UnitedKingdom);
			}
			return (owner, ownerReference);
		}

		protected override bool CheckFiscalReferenceCreationEnabledCore(Eu.JobDeclaration declaration)
		{
			return (declaration as JobDeclaration)?.ZG_UsePostponedVatAccounting ?? false;
		}
	}
}
