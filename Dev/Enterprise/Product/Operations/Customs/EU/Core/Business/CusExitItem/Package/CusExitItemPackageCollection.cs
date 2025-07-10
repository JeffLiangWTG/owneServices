using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business
{
	public class CusExitItemPackageCollection : CusInvPackCollection<CusExitItemPackage, CusExitItem>
	{
		public CusExitItemPackageCollection(CusExitItem parent) : base(parent)
		{
		}
	}
}
