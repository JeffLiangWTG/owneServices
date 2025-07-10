using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using MailManager;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Mail.Business.Test
{
	[TestedType(typeof(EDIMailItem))]
	class EDIMailItemTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			MailItem result = (MailItem)base.GetNewBusinessObjectForDeleteTest(factory);
			result.MI_Direction = DirectionList.Codes.Receive;
			result.MI_LastAttemptDateTime = ZDateTime.UtcNow;
			result.MI_ReceivedDateTime = ZDateTime.UtcNow;
			result.MI_SendDateTime = ZDateTime.UtcNow;
			return result;
		}

		public void TestTypeDecider()
		{
			MailItem mailItem = Factory.NewWithValidTestData<MailItem>();
			AssertEquals(typeof(EDIMailItem), mailItem.GetType());
			mailItem.MI_Direction = DirectionList.Codes.Receive;
			Factory.Save();
			AssertEquals(typeof(EDIMailItem), new BusinessObjectFactory().Load<MailItem>(mailItem.PK).GetType());
		}

		public void TestLastStatusChangedBy()
		{
			EDIMailItem mailItem = Factory.NewWithValidTestData<EDIMailItem>();
			AssertEquals("", mailItem.LastStatusChangedBy);

			mailItem.MI_Status = "PRS";
			mailItem.MI_Direction = DirectionList.Codes.Receive;
			Factory.Save();
			AssertEquals(ZString.Empty, mailItem.LastStatusChangedBy);
			mailItem.MI_Application = EDIMailApplication.CustomerService;
			mailItem.MI_Status = "XXX";
			Factory.Save();
			AssertContains(GlbStaff.CurrentUser.GS_FullName, mailItem.LastStatusChangedBy);
			AssertContains(GlbStaff.CurrentUser.GS_FullName, new BusinessObjectFactory().Load<EDIMailItem>(mailItem.PK).LastStatusChangedBy);
		}
	}
}
