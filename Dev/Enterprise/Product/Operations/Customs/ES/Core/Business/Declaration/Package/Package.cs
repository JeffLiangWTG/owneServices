using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class Package : EU.Business.Declaration.Package, Integration.Customs.ES.IPackage
	{
		public Package(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override CusDecHouseContainerPackValidation GetNewValidation() => new PackageValidation(this);
	}
}
