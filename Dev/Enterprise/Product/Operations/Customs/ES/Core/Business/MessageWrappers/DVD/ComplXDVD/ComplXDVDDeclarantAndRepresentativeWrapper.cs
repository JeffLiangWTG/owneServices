using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class ComplXDVDDeclarantAndRepresentativeWrapper : IComplXDVDDeclarantAndRepresentative
	{
		public ComplXDVDDeclarantAndRepresentativeWrapper(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
		}
		readonly JobDeclaration declaration;

		readonly ZString representativeTypeAuthorizationCode = "O";

		public IPartyNameProvider Declarant => CachedValueHelper.GetValue(ref declarant, () => PartyNameWrapper.New(declaration.Declarant));
		CachedValue<IPartyNameProvider> declarant;

		public IPartyNameProvider Representative => CachedValueHelper.GetValue(ref representative, () => PartyNameWrapper.New(declaration.Representative));
		CachedValue<IPartyNameProvider> representative;

		public ZString RepresentativeTypeAuthorization => declaration.ZG_AuthPerDeclaration ? representativeTypeAuthorizationCode : ZString.Empty;
	}
}
