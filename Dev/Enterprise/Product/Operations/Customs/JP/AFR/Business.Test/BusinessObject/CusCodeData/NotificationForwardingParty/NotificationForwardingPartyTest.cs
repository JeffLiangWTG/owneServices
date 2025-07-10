using CargoWise.Common;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(NotificationForwardingParty))]
	class NotificationForwardingPartyTest : Customs.Business.Testing.CusCodeDataTest<NotificationForwardingParty>
	{
		public void TestGetHumanReadableName()
		{
			AssertEquals("Notification Forwarding Party", Factory.New<NotificationForwardingParty>().HumanReadableName);
		}

		public void TestValidation()
		{
			var nfp = Factory.New<NotificationForwardingParty>();
			AssertEquals("Validation", typeof(NotificationForwardingPartyValidation), nfp.Validation.GetType());
		}

		public void TestSetDefaultValues()
		{
			var nfp = Factory.New<NotificationForwardingParty>();
			AssertEquals(NotificationForwardingParty.NFPType, nfp.CY_Type);
		}

		public void TestCY_DataMaxLength()
		{
			var nfp = Factory.New<NotificationForwardingParty>();
			AssertEquals(5, nfp.CY_DataInfo.MaxLength);
		}

		public void TestParent()
		{
			var nfp = Factory.New<NotificationForwardingParty>();
			nfp.CY_ParentID = bill.PK;
			nfp.CY_ParentTableCode = bill.TablePrefix;
			AssertEquals(bill, nfp.Parent);
		}

		public override void TestSettingValueCallsRefreshBinding()
		{
			base.TestSettingValueCallsRefreshBinding();
			if (ErrorReporter.LastKeyReported == "Enterprise.Customs.JP.AFR.Business.NotificationForwardingParty.CY_Type Invalid Setting")
			{
				ErrorReporter.Clear();
			}
		}

		public void TestReOrdering()
		{
			var nfp1 = bill.NotificationForwardingParties.AddNewIfNotExist("C1");
			nfp1.CY_Order = 1;
			var nfp2 = bill.NotificationForwardingParties.AddNewIfNotExist("C2");
			nfp2.CY_Order = 1;
			AssertEquals((short)2, nfp1.CY_Order);
			AssertEquals((short)1, nfp2.CY_Order);
			var nfp3 = bill.NotificationForwardingParties.AddNewIfNotExist("C3");
			nfp3.CY_Order = 2;
			AssertEquals((short)3, nfp1.CY_Order);
			AssertEquals((short)1, nfp2.CY_Order);
			AssertEquals((short)2, nfp3.CY_Order);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			var nfp = factory.New<NotificationForwardingParty>();
			nfp.Parent = bill;
			return nfp;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var nfp = Factory.New<NotificationForwardingParty>();
			nfp.Parent = bill;
			return nfp;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<JPAFRHeader>();
			bill = header.Bills.AddNew();
		}
		JPAFRBills bill;
	}
}
