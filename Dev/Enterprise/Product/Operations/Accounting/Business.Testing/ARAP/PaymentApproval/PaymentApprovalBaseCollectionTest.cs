using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	public abstract class PaymentApprovalBaseCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestIndexer()
		{
			var obj1 = Factory.New(ExpectedElementType);
			TestCollection.Add(obj1);
			AssertEquals("Index 0", obj1, TestCollection[0]);

			var obj2 = Factory.New(ExpectedElementType);
			TestCollection.Add(obj2);
			AssertEquals("Index 1", obj2, TestCollection[1]);
		}

		public void TestAllowNew()
		{
			Assert("AllowNew should be set to false", !TestCollection.AllowNew);
		}

		public void TestAllowRemove()
		{
			Assert("AllowRemove should be set to false", !TestCollection.AllowRemove);
		}

		public void TestRegisterEditableMatchingObjectsForAllPayments()
		{
			var obj1 = Factory.NewWithValidTestData(ExpectedElementType) as PaymentApprovalBase;
			obj1.AV_AH = TestOrg.PK;
			obj1.InitializeForPaymentBatch(() => true);
			obj1.MatchingBaseObject.MatchedTransactions.RemoveAllFromAllCollections();
			TestCollection.Add(obj1);
			AssertEquals("Index 0", obj1, TestCollection[0]);

			var obj2 = Factory.NewWithValidTestData(ExpectedElementType) as PaymentApprovalBase;
			obj2.AV_AH = TestOrg.PK;
			obj2.InitializeForPaymentBatch(() => true);
			obj2.MatchingBaseObject.MatchedTransactions.RemoveAllFromAllCollections();
			TestCollection.Add(obj2);
			AssertEquals("Index 1", obj2, TestCollection[1]);

			TestCollection.RegisterEditableMatchingObjectsForAllPayments();

			TestCollection.HasChanges = false;
			obj1.MatchingBaseObject.HasChanges = false;
			obj2.MatchingBaseObject.HasChanges = false;
			Assert("Collection should not have any changes", !TestCollection.HasChanges);
			Assert("Obj1 should not have any changes", !obj1.HasChanges);
			Assert("Obj2 should not have any changes", !obj2.HasChanges);

			obj1.PaymentMatchingBaseObject.HasChanges = true;
			Assert("Collection should have changes", TestCollection.HasChanges);
			Assert("Obj1 should have changes", obj1.HasChanges);
			Assert("Obj2 should not have any changes", !obj2.HasChanges);

			obj1.PaymentMatchingBaseObject.HasChanges = false;
			Assert("Collection should not have any changes", !TestCollection.HasChanges);
			Assert("Obj1 should not have any changes", !obj1.HasChanges);
			Assert("Obj2 should not have any changes", !obj2.HasChanges);

			obj2.PaymentMatchingBaseObject.HasChanges = true;
			Assert("Collection should have changes", TestCollection.HasChanges);
			Assert("Obj1 should not have any changes", !obj1.HasChanges);
			Assert("Obj2 should have changes", obj2.HasChanges);
		}

		public void TestGetTypeOfElementsFromPK()
		{
			var payment1 = Factory.NewWithValidTestData(ExpectedElementType) as PaymentApprovalBase;
			payment1.AV_AH = TestOrg.PK;
			payment1.InitializeForPaymentBatch(() => true);
			payment1.MatchingBaseObject.MatchedTransactions.RemoveAllFromAllCollections();
			payment1.AV_Status = PaymentApprovalStatus.Cancelled;

			var payment2 = Factory.NewWithValidTestData(ExpectedElementType) as PaymentApprovalBase;
			payment2.AV_AH = TestOrg.PK;
			payment2.InitializeForPaymentBatch(() => true);
			payment2.MatchingBaseObject.MatchedTransactions.RemoveAllFromAllCollections();
			payment2.AV_Status = PaymentApprovalStatus.Posted;

			TestCollection.Add(payment1);
			TestCollection.Add(payment2);

			AssertEquals(ExpectedElementType, TestCollection.GetTypeOfElementsFromPK(payment1.PK));

			TestCollection.Sort(PaymentApprovalBase.Schema.AV_Calc_Sequence);
			AssertEquals(payment2, TestCollection[0]);
			AssertEquals(payment1, TestCollection[1]);
		}

		[ExpectNoExceptions]
		public void TestUnRegisterEditableMatchingObjectForAPayment()
		{
			TestCollection.UnRegisterEditableMatchingObjectForAPayment_ForTestOnly(null);//Should not bring an error
			var obj1 = Factory.NewWithValidTestData(ExpectedElementType) as PaymentApprovalBase;
			obj1.AV_AH = TestOrg.PK;
			obj1.InitializeForPaymentBatch(() => true);
			obj1.MatchingBaseObject.MatchedTransactions.RemoveAllFromAllCollections();
			TestCollection.Add(obj1);
			AssertEquals("Index 0", obj1, TestCollection[0]);

			TestCollection.RegisterEditableMatchingObjectsForAllPayments();

			TestCollection.HasChanges = false;
			obj1.MatchingBaseObject.HasChanges = false;
			Assert("Collection should not have any changes", !TestCollection.HasChanges);
			Assert("Obj1 should not have any changes", !obj1.HasChanges);

			obj1.PaymentMatchingBaseObject.HasChanges = true;
			Assert("Collection should have changes", TestCollection.HasChanges);
			Assert("Obj1 should have changes", obj1.HasChanges);

			TestCollection.HasChanges = false;
			TestCollection.UnRegisterEditableMatchingObjectForAPayment_ForTestOnly(obj1);
			obj1.PaymentMatchingBaseObject.HasChanges = true;
			Assert("Obj1 should not have any changes", !obj1.HasChanges);
			Assert("Obj1.PaymentMatchingBaseObject should have changes", obj1.PaymentMatchingBaseObject.HasChanges);
		}

		#region Implementation

		PaymentApprovalBaseCollection TestCollection;
		OrgHeader TestOrg;

		protected Type ExpectedElementType => TestCollection.TypeOfElements;

		protected override void SetUp()
		{
			base.SetUp();
			TestOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			TestCollection = GetCollectionToTest() as PaymentApprovalBaseCollection;
		}

		#endregion
	}
}
