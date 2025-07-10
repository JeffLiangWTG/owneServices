using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class CusPersonCountryLookups : Customs.Business.CusPersonCountryLookups
	{
		public CusPersonCountryLookups(CusPersonCountry parent)
			: base(parent)
		{ }

		public virtual CodeDescriptionPairList DataTypes => new CodeDescriptionPairList();

		public virtual CodeDescriptionPairList DataValues => new CodeDescriptionPairList();

		protected new CusPersonCountry Parent => (CusPersonCountry)base.Parent;
	}
}
