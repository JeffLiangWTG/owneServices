
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class MercosulForeignDeclarationLookups : Customs.Business.CusSupportingInfoLookups
	{
		public MercosulForeignDeclarationLookups(MercosulForeignDeclaration parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList MercosulCountriesList => Factory.GetCachedValue<MercosulCountriesList>();
	}
}
