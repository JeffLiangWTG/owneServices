using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Messaging
{
	public interface ICC014ADeclaration : IDeclaration
	{
		ZString DeclarantTIN { get; }
		ZString PrincipalTIN { get; }
		ZBool IsTIRDeclaration { get; }
	}
}
