using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSPackageCollection : CusInvPackCollection<EMCSPackage, EMCSJobDeclaration>
	{
		public EMCSPackageCollection(EMCSJobDeclaration parent) : base(parent)
		{
			this.parent = parent;
			this.EnableMaxCountValidation(99, Res.GetString("165576e6-9f61-41be-ab86-e5ee62b9a9e2", "There are too many Packages. Maximum of 99."), false);
		}

		readonly EMCSJobDeclaration parent;

		public new EMCSJobDeclaration Master => base.Master;

		protected override bool AllowNewCore => !parent.IsMessageStatusSentOrAcknowledged;
	}
}
