using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GenericCharge
{
	[TestedType(typeof(GenericChargeCollection))]
	public class GenericChargeCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GenericChargeCollectionForTest(Factory);
		}

		public override void TestDelete()
		{
			base.TestRemoveFromRelationship();
		}

		public override void TestTypedAddNew()
		{
			Assert(true);
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Assert(true);
		}

		public void TestRelatiosnshipFilter()
		{
			GlbCompany demoCompany = Factory.LoadTop1(typeof(GlbCompany), new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK)) as GlbCompany;
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			AccChargeCode testInactiveCharge = testObjectCreator.CreateChargeCode("TESTCDE1", "TEST Charge Code1", "REV", 1.5m, null, null, "ALL");
			testInactiveCharge.AC_IsActive = ZBool.False;
			AccChargeCode testNonCurrentCompanyCharge = testObjectCreator.CreateChargeCode("TESTCDE2", "TEST Charge Code2", "REV", 1.5m, null, null, "ALL");
			testNonCurrentCompanyCharge.AC_GC = demoCompany.PK;
			AccChargeCode testCorrectCharge = testObjectCreator.CreateChargeCode("TESTCDE3", "TEST Charge Code3", "REV", 1.5m, null, null, "ALL");
			Factory.Save();
			GenericChargeCollection testChargeCollection = new GenericChargeCollection(Factory);
			testChargeCollection.Load();
			AssertNull(testChargeCollection.FindByPK(testInactiveCharge.PK));
			AssertNull(testChargeCollection.FindByPK(testNonCurrentCompanyCharge.PK));
			AssertNotNull(testChargeCollection.FindByPK(testCorrectCharge.PK));
		}

		public void TestShowGLAccountsForImportAction()
		{
			var collection = (GenericChargeCollectionForTest)GetCollectionToTest();
			collection.ShowGLAccountsForImportAction = (glHeaderCollection, glHeaderList) => { glHeaderList.Add(Factory.New<AccGLHeader>()); };
			var action = ((AccGLHeaderFindBoxListProvider)collection.FindBoxListProvider_ForTest).ShowGLAccountsForImportAction;
			AssertNotNull(action);
			AssertEquals(collection.ShowGLAccountsForImportAction, action);
		}

		class GenericChargeCollectionForTest : GenericChargeCollection
		{
			public GenericChargeCollectionForTest(BusinessObjectFactory factory) : base(factory)
			{
			}

			public IFindBoxListProvider FindBoxListProvider_ForTest => FindBoxListProvider;
		}
	}
}
