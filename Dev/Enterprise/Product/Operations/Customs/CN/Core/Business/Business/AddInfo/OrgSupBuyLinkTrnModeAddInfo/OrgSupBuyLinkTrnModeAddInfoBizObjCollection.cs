using CargoWise.EntityFramework;

namespace Enterprise.Customs.CN.Business
{
	public class OrgSupBuyLinkTrnModeAddInfoBizObjCollection : NonPersistentBusinessObjectCollection<OrgSupBuyLinkTrnModeAddInfoBizObj>
	{
		public OrgSupBuyLinkTrnModeAddInfoBizObjCollection(BusinessObjectFactory factory) : base(factory) { }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OrgSupBuyLinkTrnModeAddInfoBizObj(Factory);
		}

		protected override bool AllowNewCore => false;
	}
}
