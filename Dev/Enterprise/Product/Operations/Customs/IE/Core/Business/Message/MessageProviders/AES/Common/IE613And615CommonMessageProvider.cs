using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AES
{
	public abstract class IE613And615CommonMessageProvider : EntryHeaderMessageProvider
	{
		protected IE613And615CommonMessageProvider(CusEntryHeader entryHeader) : base(entryHeader)
		{
			Consignment = new IE613And615ConsignmentProvider(entryHeaderWrapper);
		}

		public string SpecificCircumstanceIndicator => declaration.ZG_SpecificCircumstanceIndicator;

		public string CustomsOfficeOfExitDeclared => declaration.OfficeOfExitCustomsOffice;

		public string CustomsOfficeOfLodgement => declaration.JE_CustomsOffice;

		public IIE613And615Consignment Consignment { get; }

		public IParty Declarant => CachedValueHelper.GetValue(ref declarantCached, () => PartyProvider.New(declaration.Declarant));
		CachedValue<IParty> declarantCached;

		public IRepresentative Representative => CachedValueHelper.GetValue(ref representativeCached, () => RepresentativeProvider.New(declaration.Representative, declaration.JE_DeclarantType));
		CachedValue<IRepresentative> representativeCached;
	}
}
