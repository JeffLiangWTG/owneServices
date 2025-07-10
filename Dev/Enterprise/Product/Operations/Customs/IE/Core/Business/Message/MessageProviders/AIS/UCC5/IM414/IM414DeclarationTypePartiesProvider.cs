using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM414DeclarationTypePartiesProvider : IIM414DeclarationTypeParties
	{
		readonly JobDeclaration declaration;

		public IM414DeclarationTypePartiesProvider(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}

		public IParty Declarant => CachedValueHelper.GetValue(ref declarantCached, () => PartyProvider.New(declaration.Declarant));
		CachedValue<IParty> declarantCached;

		public IRepresentative Representative => CachedValueHelper.GetValue(ref representativeCached, () => RepresentativeProvider.New(declaration));
		CachedValue<IRepresentative> representativeCached;
	}
}
