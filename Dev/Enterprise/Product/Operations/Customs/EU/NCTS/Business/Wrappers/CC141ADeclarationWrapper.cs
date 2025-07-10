using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CC141ADeclarationWrapper : DeclarationWrapper, ICC141ADeclaration
	{
		public CC141ADeclarationWrapper(NctsHeader nctsHeader) : base(nctsHeader)
		{ }

		public ZString DeclarantTIN => CachedValueHelper.GetValue(ref declarantTIN, () => nctsHeader.DepartureDeclarantTIN());
		CachedValue<ZString> declarantTIN;

		public ZString PrincipalTIN => CachedValueHelper.GetValue(ref principalTIN, () => nctsHeader.DeparturePrincipalTIN());
		CachedValue<ZString> principalTIN;

		public ZBool IsTIRDeclaration => nctsHeader.MovementHeader.IsTIRDeclaration;
	}
}
