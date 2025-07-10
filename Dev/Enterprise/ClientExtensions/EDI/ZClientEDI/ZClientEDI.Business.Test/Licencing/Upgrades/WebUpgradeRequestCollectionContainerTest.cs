using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	[TestedType(typeof(WebUpgradeRequestCollectionContainer))]
	public class WebUpgradeRequestCollectionContainerTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			UpgradeRequestCollection upgradeRequestCollection = new UpgradeRequestCollection(Factory);
			return new WebUpgradeRequestCollectionContainer(Factory, upgradeRequestCollection);
		}

		public void TestSendEmailNotificationAutomatically()
		{
			AssertEquals("SendEmailNotificationAutomatically should be true by default", true, BizObj.SendEmailNotificationAutomatically);
		}

		#region Properties

		protected WebUpgradeRequestCollectionContainer BizObj
		{
			get
			{
				if (fBizObj == null)
				{
					fBizObj = (WebUpgradeRequestCollectionContainer)GetNewBusinessObject();
				}

				return fBizObj;
			}
		}

		WebUpgradeRequestCollectionContainer fBizObj;

		#endregion
	}
}
