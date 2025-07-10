using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM413AndIM415DeclarationTypePartiesProvider : IIM413AndIM415DeclarationTypeParties
	{
		public IM413AndIM415DeclarationTypePartiesProvider(CusEntryHeader entryHeader)
		{
			declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
			instruction = Argument.NotNull(entryHeader.EntryInstruction, nameof(entryHeader.EntryInstruction));
		}
		readonly JobDeclaration declaration;
		readonly CusEntryInstruction instruction;

		public IParty Exporter => CachedValueHelper.GetValue(
			ref exporterCached,
			() =>
			{
				var supplierCustomsCode = declaration.Supplier?.CustomsCodes?.Where(x => x.OK_CodeType == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori).FirstOrDefault();
				if (supplierCustomsCode != null && supplierCustomsCode.OK_RN_NKCodeCountry.EqualsIgnoringCase(Core.Constants.CountryCodes.UnitedKingdom))
				{
					return PartyProvider.NewWithIDNull(declaration.SupplierDocumentaryAddress);
				}
				else
				{
					return PartyProvider.New(declaration.SupplierDocumentaryAddress);
				}
			}
		);
		CachedValue<IParty> exporterCached;

		public IParty Declarant => CachedValueHelper.GetValue(ref declarantCached, () => PartyProvider.New(declaration.Declarant));
		CachedValue<IParty> declarantCached;

		public IRepresentative Representative => CachedValueHelper.GetValue(ref representativeCached, () => RepresentativeProvider.New(declaration));
		CachedValue<IRepresentative> representativeCached;

		public IReadOnlyCollection<IAuthorisationHolder> AuthorisationHolder => authorisationHolderCache ??= instruction.CusAuthorizationUsages.Select(AuthorisationHolderProvider.New).ToArray();
		IReadOnlyCollection<IAuthorisationHolder> authorisationHolderCache;

		public string PersonProvidingGuarantee => GetPersonProvidingGuarantee();

		public string PersonPayingCustomsDuty => declaration.DutyPayer.GetEORI();

		string GetPersonProvidingGuarantee()
		{
			string result = null;
			if (declaration.DefermentPartyDocAddress is JobDocAddress address)
			{
				if (address.E2_AddressOverride)
				{
					result = address.E2_GovRegNum;
				}
				else
				{
					result = address.Address.GetEORI();
				}
			}
			return result;
		}
	}
}
