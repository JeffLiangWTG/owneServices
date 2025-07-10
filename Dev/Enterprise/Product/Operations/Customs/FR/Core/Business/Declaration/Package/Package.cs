using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class Package : EU.Business.Declaration.Package, Integration.Customs.FR.IPackage
	{
		public Package(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public new class Schema : BasePackage.Schema
		{
			public const int CW_MarksAndNosMaxLengthForMessage = 42;
		}
		public new PackageValidation Validation => (PackageValidation)base.Validation;
		protected override CusDecHouseContainerPackValidation GetNewValidation() => new PackageValidation(this);
	}
}
