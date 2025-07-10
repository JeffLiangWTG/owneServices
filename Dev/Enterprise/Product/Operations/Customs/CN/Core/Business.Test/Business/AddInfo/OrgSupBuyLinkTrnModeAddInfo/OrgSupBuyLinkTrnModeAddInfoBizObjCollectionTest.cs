using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(OrgSupBuyLinkTrnModeAddInfoBizObjCollection))]
	class OrgSupBuyLinkTrnModeAddInfoBizObjCollectionTest : NonPersistentBusinessObjectCollectionTestCase<OrgSupBuyLinkTrnModeAddInfoBizObjCollection>
	{
		protected override OrgSupBuyLinkTrnModeAddInfoBizObjCollection GetCollectionToTest() => new OrgSupBuyLinkTrnModeAddInfoBizObjCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new OrgSupBuyLinkTrnModeAddInfoBizObj((OrgSupBuyLinkTrnModeAddInfo)Factory.New<OrgSupBuyLinkTrnMode>().AddInfo);
	}
}
