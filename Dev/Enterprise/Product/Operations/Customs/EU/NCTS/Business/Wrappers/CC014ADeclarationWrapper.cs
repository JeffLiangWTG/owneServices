using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CC014ADeclarationWrapper : DeclarationWrapper, ICC014ADeclaration
	{
		public CC014ADeclarationWrapper(NctsHeader nctsHeader) : base(nctsHeader)
		{ }

		public ZString DeclarantTIN => CachedValueHelper.GetValue(ref declarantTIN, () => nctsHeader.DepartureDeclarantTIN());
		CachedValue<ZString> declarantTIN;

		public ZString PrincipalTIN => CachedValueHelper.GetValue(ref principalTIN, () => nctsHeader.DeparturePrincipalTIN());
		CachedValue<ZString> principalTIN;

		public ZString TypeOfDeclaration => nctsHeader.MovementHeader.BM_InBondEntryType;

		public ZBool IsTIRDeclaration => nctsHeader.MovementHeader.IsTIRDeclaration;
	}
}
