using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	[TestedType(typeof(TransactionMatchLinkGroup))]
	public class TransactionMatchLinkGroupTest : TransactionMatchLinkCollectionTest
	{
		public void TestCollectionContainsInvoice()
		{
			APInvoice invoice1 = Factory.New<APInvoice>();
			AssertEquals("Contains Invoice 1", false, TestCollection.ContainsInvoice(invoice1));
			TransactionMatchLink matchLink = TestCollection.AddNew();

			matchLink.AP_AH = invoice1.PK;
			AssertEquals("Contains Invoice 1", true, TestCollection.ContainsInvoice(invoice1));

			matchLink.AP_AH = ZGuid.NewZGuid();
			AssertEquals("Contains Invoice 1", false, TestCollection.ContainsInvoice(invoice1));

			APInvoice invoice2 = Factory.New<APInvoice>();
			matchLink.AP_AH = invoice2.PK;
			AssertEquals("Contains Invoice 1", false, TestCollection.ContainsInvoice(invoice1));
			AssertEquals("Contains Invoice 2", true, TestCollection.ContainsInvoice(invoice2));
		}

		public void TestGetBalance_Balanced()
		{
			TransactionMatchLink matchlink1 = TestCollection.AddNew();
			matchlink1.AP_Amount = 90M;
			TransactionMatchLink matchlink2 = TestCollection.AddNew();
			matchlink2.AP_Amount = -90M;
			AssertEquals("Matchlinks should balance to zero", 0M, TestCollection.GetBalance());
		}

		public void TestGetBalance_Caculate180M()
		{
			TransactionMatchLink matchlink1 = TestCollection.AddNew();
			matchlink1.AP_Amount = 90M;
			TransactionMatchLink matchlink2 = TestCollection.AddNew();
			matchlink2.AP_Amount = 90M;
			AssertEquals(180M, TestCollection.GetBalance());
		}

		public void TestCriticalValidation()
		{
			Assert("Supports CriticalValidation", TestCollection is ISupportCriticalValidation);
			AssertNotNull("CriticalValidation", ((ISupportCriticalValidation)TestCollection).CriticalValidation);
			Assert("CriticalValidation is TransactionMatchLinkCollectionCriticalValidation", ((ISupportCriticalValidation)TestCollection).CriticalValidation is TransactionMatchLinkGroupCriticalValidation);
		}

		protected new TransactionMatchLinkGroup TestCollection
		{
			get { return base.TestCollection as TransactionMatchLinkGroup; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			base.TestCollection = new TransactionMatchLinkGroup(Factory);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new TransactionMatchLinkGroup(Factory);
		}
	}
}
