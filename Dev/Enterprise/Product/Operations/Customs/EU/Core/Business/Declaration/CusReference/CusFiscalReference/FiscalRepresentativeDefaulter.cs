using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class FiscalRepresentativeDefaulter
	{
		public void DefaultFiscalReferences(ICusEntryInstructionCollection<CusEntryInstruction> ceiCollection)
		{
			foreach (var entryInstruction in ceiCollection)
			{
				DefaultFiscalReferences(entryInstruction);
			}
		}

		public void DefaultFiscalReferences(CusEntryInstruction cei)
		{
			if (cei is null)
			{
				return;
			}

			var fiscalReferenceCode = GetFiscalReferenceDefaultCode();
			var existingFiscalReference = cei.FiscalReferences.Cast<CusFiscalReference>().Any(x => x.CFR_Code == fiscalReferenceCode);

			if (!existingFiscalReference && cei.JobDeclaration is JobDeclaration declaration && declaration.Configuration.InstructionConfiguration.FiscalReferencesSupport(declaration))
			{
				if (CheckFiscalReferenceCreationEnabled(declaration))
				{
					var (owner, ownerReference) = GetFiscalReferenceRelatedParty(cei);
					if (owner != null && !ownerReference.IsEmpty)
					{
						var fiscalReference = cei.FiscalReferences.AddNew();
						fiscalReference.CFR_Code = fiscalReferenceCode;
						fiscalReference.CFR_Reference = ownerReference;
						fiscalReference.CFR_OA_Owner_ZAddress.OrgPK = owner.PK;
					}
				}
			}
		}

		public static void DefaultFiscalReferences(JobComInvoiceLine invoiceLine, string type)
		{
			if (invoiceLine != null && !string.IsNullOrEmpty(type))
			{
				var existingFiscalReference = invoiceLine.FiscalReferences.Cast<CusFiscalReference>().Any(x => x.CFR_Code == type);
				if (!existingFiscalReference && invoiceLine.Declaration is JobDeclaration declaration && declaration.Configuration.InvoiceLineConfiguration.FiscalReferencesSupport(declaration))
				{
					var (partyPk, vatNumber) = GetVatNumber(declaration.ConsigneeOrgAddress);

					if (vatNumber.IsEmpty)
					{
						(partyPk, vatNumber) = GetVatNumber(declaration.Importer);
					}

					var fiscalReference = invoiceLine.FiscalReferences.AddNew();
					fiscalReference.CFR_Code = type;
					if (!vatNumber.IsEmpty)
					{
						fiscalReference.CFR_Reference = vatNumber;
						fiscalReference.CFR_OA_Owner_ZAddress.OrgPK = partyPk;
					}
				}
			}

			(ZGuid PartyPK, ZString VatNumber) GetVatNumber(OrgHeader party)
			{
				var (partyPK, vatNumber) = (ZGuid.Empty, ZString.Empty);
				if (party != null)
				{
					partyPK = party.PK;
					var countryCode = party.CountryCode;
					if (!countryCode.IsEmpty)
					{
						string codeType = ZArchitecture.Environment.Country.GetConsumptionTaxRegistrationOrgCusCode(countryCode);
						if (!string.IsNullOrWhiteSpace(codeType))
						{
							vatNumber = party.CustomsCodes.GetCustomsRegNo(codeType, countryCode);
						}
					}
				}

				return (partyPK, vatNumber);
			}
		}

		public static void RemoveFiscalReference(BusinessObject parent, string type)
		{
			if (parent != null && !string.IsNullOrEmpty(type))
			{
				var fiscalReferenceToRemove = parent.Factory.Load<CusFiscalReference>(new ZQuery(CusReferenceSchema.CFR_ParentID, parent.PK)).FirstOrDefault(x => x.CFR_Code == type);
				fiscalReferenceToRemove?.Delete();
			}
		}

		public bool CheckFiscalReferenceCreationEnabled(JobDeclaration declaration) => CheckFiscalReferenceCreationEnabledCore(declaration);

		protected virtual bool CheckFiscalReferenceCreationEnabledCore(JobDeclaration declaration)
		{
			var euOrgImpAddInfo = EUOrgImpAddInfo.Get(declaration.Importer, declaration.CountryCode);
			return euOrgImpAddInfo?.ZO_UseFr3FiscalRepresentation ?? false;
		}

		public (OrgHeader, ZString) GetFiscalReferenceRelatedParty(CusEntryInstruction cei) => GetFiscalReferenceRelatedPartyCore(cei);
		protected virtual (OrgHeader, ZString) GetFiscalReferenceRelatedPartyCore(CusEntryInstruction cei)
		{
			OrgHeader owner = null;
			var ownerReference = ZString.Empty;
			if (cei.JobDeclaration is JobDeclaration declaration)
			{
				var importer = declaration.Importer;
				var cpvRelatedParty = importer?.AllRelatedParties.Cast<OrgRelatedParty>()
					.FirstOrDefault(orp => orp.PR_PartyType == RelatedPartyTypeList.Codes.CustomsPostponedVatAccounting);

				owner = cpvRelatedParty?.RelatedParty;
				ownerReference = cei.GetOwnerReference(owner);
			}
			return (owner, ownerReference);
		}

		public string GetFiscalReferenceDefaultCode() => GetFiscalReferenceDefaultCodeCore();
		protected virtual string GetFiscalReferenceDefaultCodeCore() => FiscalReferenceCodeList.Codes.FR3_TaxRepresentative;
	}
}
