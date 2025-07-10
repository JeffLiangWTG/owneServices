using CargoWise.Common;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(OtherRelevantLaw))]
	class OtherRelevantLawTest : Customs.Business.Testing.CusCodeDataTest<OtherRelevantLaw>
	{
		public void TestGetHumanReadableName()
		{
			AssertEquals("Other Relevant Law", Factory.New<OtherRelevantLaw>().HumanReadableName);
		}

		public void TestValidation()
		{
			var orl = Factory.New<OtherRelevantLaw>();
			AssertEquals("Validation", typeof(OtherRelevantLawValidation), orl.Validation.GetType());
		}

		public void TestSetDefaultValues()
		{
			var orl = Factory.New<OtherRelevantLaw>();
			AssertEquals(OtherRelevantLaw.ORLType, orl.CY_Type);
		}

		public void TestCY_DataMaxLength()
		{
			var orl = Factory.New<OtherRelevantLaw>();
			AssertEquals(2, orl.CY_DataInfo.MaxLength);
		}

		public void TestParent()
		{
			var orl = Factory.New<OtherRelevantLaw>();
			orl.CY_ParentID = Bill.PK;
			orl.CY_ParentTableCode = Bill.TablePrefix;
			AssertEquals(Bill, orl.Parent);
		}

		public override void TestSettingValueCallsRefreshBinding()
		{
			base.TestSettingValueCallsRefreshBinding();
			if (ErrorReporter.LastKeyReported == "Enterprise.Customs.JP.AFR.Business.OtherRelevantLaw.CY_Type Invalid Setting")
			{
				ErrorReporter.Clear();
			}
		}

		public void TestReOrdering()
		{
			var law1 = Bill.OtherRelevantLaws.AddNewIfNotExist("C1");
			law1.CY_Order = 1;
			var law2 = Bill.OtherRelevantLaws.AddNewIfNotExist("C2");
			law2.CY_Order = 1;
			AssertEquals((short)2, law1.CY_Order);
			AssertEquals((short)1, law2.CY_Order);
			var law3 = Bill.OtherRelevantLaws.AddNewIfNotExist("C3");
			law3.CY_Order = 2;
			AssertEquals((short)3, law1.CY_Order);
			AssertEquals((short)1, law2.CY_Order);
			AssertEquals((short)2, law3.CY_Order);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			var result = factory.New<OtherRelevantLaw>();
			result.Parent = bill;
			return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var orl = Factory.New<OtherRelevantLaw>();
			orl.Parent = Bill;
			return orl;
		}

		JPAFRBills Bill
		{
			get { return bill ?? (bill = Header.Bills.AddNew()); }
		}
		JPAFRBills bill;

		JPAFRHeader Header
		{
			get { return header ?? (header = Factory.New<JPAFRHeader>()); }
		}
		JPAFRHeader header;

		#endregion
	}
}
