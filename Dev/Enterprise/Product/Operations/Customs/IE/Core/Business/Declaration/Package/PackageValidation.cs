using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class PackageValidation : EU.Business.Declaration.PackageValidation
	{
		public PackageValidation(Package parent) : base(parent)
		{
		}

		protected new Package Parent => (Package)base.Parent;

		protected override void CheckCW_PackType()
		{
			var package = Package;
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(package.CW_PackTypeInfo, package.PackTypeList);
		}
	}
}
