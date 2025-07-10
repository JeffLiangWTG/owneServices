using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AES
{
	public abstract class IE570And573CommonMessageProvider : EntryHeaderMessageProvider
	{
		protected IE570And573CommonMessageProvider(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		public string CustomsOfficeOfExitDeclared => declaration.OfficeOfExitCustomsOffice;

		public IParty Declarant => CachedValueHelper.GetValue(ref declarantCached, () => PartyProvider.New(declaration.Declarant));
		CachedValue<IParty> declarantCached;

		public IRepresentative Representative => CachedValueHelper.GetValue(ref representativeCached, () => RepresentativeProvider.New(declaration.Representative, declaration.JE_DeclarantType));
		CachedValue<IRepresentative> representativeCached;
	}
}
