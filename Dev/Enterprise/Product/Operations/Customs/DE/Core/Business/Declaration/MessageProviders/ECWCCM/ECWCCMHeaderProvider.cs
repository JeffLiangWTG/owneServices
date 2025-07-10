using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business
{
	public sealed class ECWCCMHeaderProvider : ImportHeaderProvider, IECWCCMHeader
	{
		public ECWCCMHeaderProvider(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		public string CustomsAuthorisationCurrentProcedure => CachedValueHelper.GetValue(ref currentProcedure, GetCW1OrCWPAuthorizationNumberFromDeclarantOrgHeader);
		CachedValue<string> currentProcedure;

		public string RepresentativeRelationshipFlag => Declaration.JE_DeclarantType.MapDeclarantTypeToMessagingWithoutIndirect();

		public IPartyID CustomsAuthorisationOwner => CachedValueHelper.GetValue(ref customsAuthorisationOwner, () => ImportPartyIDProvider.NewOrNull(Declaration.Declarant));
		CachedValue<IPartyID> customsAuthorisationOwner;

		public IPartyID Representative => CachedValueHelper.GetValue(ref representative, GetRepresentative);
		CachedValue<IPartyID> representative;

		IPartyID GetRepresentative()
		{
			IPartyID result = null;
			var declaration = Declaration;
			if (declaration.JE_DeclarantType == RepresentationTypeList.Codes._2Direct)
			{
				result = ImportPartyIDProvider.NewOrNull(declaration.Representative);
			}
			return result;
		}

		public IReadOnlyCollection<IECWCCMLine> Lines => lines ?? (lines = EntryHeader.MergedLines.Cast<CusEntryLine>().Select(l => new ECWCCMLineProvider(l)).ToArray());
		IReadOnlyCollection<IECWCCMLine> lines;
	}
}
