using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class Package : EU.Business.Declaration.Package, Integration.Customs.DE.IPackage
	{
		public Package(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new PackageValidation Validation => (PackageValidation)GetNewValidation();

		protected override CusDecHouseContainerPackValidation GetNewValidation() => new PackageValidation(this);
	}
}
