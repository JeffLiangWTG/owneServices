using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class BusinessObjectAfterSaveNotificationTest : TestCaseWithFactory
	{
		public void TestFactorySaveUpdatesMessageAndDeallocatesBusinessObject()
		{
			var log = Factory.NewWithValidTestData<StmALog>();
			var saveNotificationInfo = new BusinessObjectAfterSaveNotificationForTest(log);

			ZGuid originalPk = saveNotificationInfo.PK;

			AssertNotNull("Business entity is not null", saveNotificationInfo.BusinessEntity);
			Factory.Save();

			ZGuid afterSavePk = saveNotificationInfo.PK;

			AssertEquals(originalPk, afterSavePk);
			AssertNotNull(log);
			AssertNull("BusinessEntity should be null after saving", saveNotificationInfo.businessEntity);
		}
	}
}
