using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(CusCodeDataWithSequenceNumberLineCollection<OtherRelevantLaw>))]
	class CusCodeDataWithSequenceNumberLineCollectionTest : CargoWise.EntityFramework.Testing.ActiveBusinessObjectCollectionTestCase<CusCodeDataWithSequenceNumberLineCollection<OtherRelevantLaw>>
	{
		public void TestSetRelationshipDefaults()
		{
			var orl = OtherRelevantLaws.AddNew();
			AssertEquals(Bill.PK, orl.CY_ParentID);
			AssertEquals(JPAFRBillsSchema.Constants.Prefix, orl.CY_ParentTableCode);
		}

		public void TestGetNonEmptyORLInSortOrder()
		{
			var orl1 = OtherRelevantLaws.AddNew();
			orl1.CY_Data = "T5";
			var orl2 = OtherRelevantLaws.AddNew();
			var orl3 = OtherRelevantLaws.AddNew();
			orl3.CY_Data = "T3";
			var orl4 = OtherRelevantLaws.AddNew();
			orl4.CY_Data = "T4";
			orl3.CY_Order = 1;
			orl2.CY_Order = 2;
			orl1.CY_Order = 3;
			orl4.CY_Order = 4;

			var orl5 = new BusinessObjectFactory().New<OtherRelevantLaw>();
			orl5.CY_Order = 3;
			orl5.CY_Data = "T1";
			orl5.CY_ParentID = Bill.PK;
			orl5.CY_ParentTableCode = Bill.TablePrefix;
			orl5.Factory.Save();
			orl5 = Factory.Load<OtherRelevantLaw>(orl5.PK);
			AssertEquals("Precondition", 5, OtherRelevantLaws.Count);
			AssertEquals((short)1, orl3.CY_Order);
			AssertEquals((short)2, orl2.CY_Order);
			AssertEquals((short)3, orl1.CY_Order);
			AssertEquals((short)3, orl5.CY_Order);
			AssertEquals((short)4, orl4.CY_Order);

			var orls = OtherRelevantLaws.GetNonEmptyInSortOrder();
			AssertEquals(4, orls.Length);
			AssertEquals("ORL3", orl3, orls[0]);
			AssertEquals("ORL1", orl1, orls[2]);
			AssertEquals("ORL5", orl5, orls[1]);
			AssertEquals("ORL4", orl4, orls[3]);
		}

		public void TestAddNewIfNotExist()
		{
			AssertEquals(0, OtherRelevantLaws.Count);
			var law1 = OtherRelevantLaws.AddNewIfNotExist("JP");
			AssertEquals(1, OtherRelevantLaws.Count);
			var law2 = OtherRelevantLaws.AddNewIfNotExist("JP");
			AssertEquals(1, OtherRelevantLaws.Count);
			AssertEquals(law1, law2);
			var law3 = OtherRelevantLaws.AddNewIfNotExist("DJ");
			AssertEquals(2, OtherRelevantLaws.Count);
			AssertNotEquals(law1, law3);
		}

		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<OtherRelevantLaw>();
			result.CY_ParentID = Bill.PK;
			result.CY_ParentTableCode = Bill.TablePrefix;
			return result;
		}

		JPAFRHeader Header
		{
			get { return header ?? (header = Factory.New<JPAFRHeader>()); }
		}
		JPAFRHeader header;

		JPAFRBills Bill
		{
			get { return bill ?? (bill = Header.Bills.AddNew()); }
		}
		JPAFRBills bill;

		CusCodeDataWithSequenceNumberLineCollection<OtherRelevantLaw> OtherRelevantLaws
		{
			get { return Bill.OtherRelevantLaws; }
		}

		protected override CusCodeDataWithSequenceNumberLineCollection<OtherRelevantLaw> GetCollectionToTest()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			return new CusCodeDataWithSequenceNumberLineCollection<OtherRelevantLaw>(bill, OtherRelevantLaw.ORLType);
		}

		#endregion
	}
}
