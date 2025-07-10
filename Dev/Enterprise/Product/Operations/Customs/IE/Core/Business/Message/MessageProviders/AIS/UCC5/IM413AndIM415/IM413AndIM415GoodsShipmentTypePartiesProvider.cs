using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using IParty = CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces.IParty;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM413AndIM415GoodsShipmentTypePartiesProvider : IGoodsShipmentTypeParties
	{
		public IM413AndIM415GoodsShipmentTypePartiesProvider(EntryHeaderWrapper entryHeaderWrapper)
		{
			declaration = entryHeaderWrapper.Declaration;
			instruction = entryHeaderWrapper.Instruction;
		}
		readonly JobDeclaration declaration;
		readonly CusEntryInstruction instruction;

		public IParty Importer => CachedValueHelper.GetValue(ref importerCached, () => declaration.ImporterDocumentaryAddress is JobDocAddress address ? PartyProvider.New(address, GetImporterId(address)) : null);
		CachedValue<IParty> importerCached;

		string GetImporterId(JobDocAddress address)
		{
			var result = ZString.Empty;

			if (!address.E2_AddressOverride)
			{
				var orgAddress = address.Address;
				result = orgAddress.GetEORI();
				if (result.IsEmpty)
				{
					result = MessageProviderHelper.GetPartyRegNo(orgAddress);
					if (result.IsEmpty)
					{
						result = PartyProvider.NoRegNumber;
					}
				}
			}

			return result;
		}

		public IParty Seller => CachedValueHelper.GetValue(ref sellerCached, () => declaration.SellerAddress is OrgAddress address ? PartyProvider.New(address) : null);
		CachedValue<IParty> sellerCached;

		public IParty Buyer => CachedValueHelper.GetValue(ref buyerCached, () => PartyProvider.New(declaration.Buyer?.MainAddress));
		CachedValue<IParty> buyerCached;

		public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActor => additionalSupplyChainActors ?? (additionalSupplyChainActors =
			instruction.CusSupplyChainActorReferences.Cast<EU.Business.Declaration.CusSupplyChainActorReference>()
				.Select(x => new AdditionalSupplyChainActorProvider(x)).ToArray());
		IReadOnlyCollection<IAdditionalSupplyChainActor> additionalSupplyChainActors;
	}
}
