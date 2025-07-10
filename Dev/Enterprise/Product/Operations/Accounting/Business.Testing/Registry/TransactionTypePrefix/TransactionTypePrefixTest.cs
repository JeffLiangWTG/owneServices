using System.Linq;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(TransactionTypePrefix))]
	public class TransactionTypePrefixTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestAutoProcessContra()
		{
			var collection = new TransactionTypePrefixCollection();
			var prefix = collection.AddNew();
			prefix.Ledger = "AR";
			prefix.TransactionType = "CTR";
			prefix.Prefix = "XX";

			AssertEquals(2, collection.Count);
			var autoAddedPrefix = collection.Cast<TransactionTypePrefix>().FirstOrDefault(x => x.Ledger == "AP" && x.TransactionType == "CTR" && x.Prefix == "XX");
			AssertNotNull(autoAddedPrefix);
			Assert(autoAddedPrefix.ReadOnly);

			prefix.Prefix = "AAB";
			AssertEquals("AAB", autoAddedPrefix.Prefix);

			prefix.Ledger = "GL";
			AssertEquals(1, collection.Count);

			prefix.Ledger = "AR";
			AssertEquals(2, collection.Count);

			prefix.TransactionType = "INV";
			AssertEquals(1, collection.Count);

			prefix.Ledger = "AR";
			prefix.TransactionType = "CTR";

			var duplicatePrefix = collection.AddNew();
			duplicatePrefix.Ledger = "AR";
			duplicatePrefix.TransactionType = "CTR";
			AssertEquals(3, collection.Count);

			collection.RemoveAndDelete(duplicatePrefix);
			AssertEquals(2, collection.Count);

			collection.RemoveAndDelete(prefix);
			AssertEquals(0, collection.Count);
		}

		public void TestSetIsAutoAddedWithGetClone()
		{
			var collection = new TransactionTypePrefixCollection();
			var prefix = collection.AddNew();
			prefix.Ledger = "AR";
			prefix.TransactionType = "CTR";
			prefix.Prefix = "XX";

			AssertEquals(2, collection.Count);
			var autoAddedPrefix = collection.Cast<TransactionTypePrefix>().FirstOrDefault(x => x.Ledger == "AP" && x.TransactionType == "CTR" && x.Prefix == "XX");

			AssertNotNull(autoAddedPrefix);
			Assert(autoAddedPrefix.ReadOnly);
			Assert(!prefix.ReadOnly);

			var clonedPrefix = (TransactionTypePrefix)prefix.Clone(null, null);
			var clonedAutoAddedPrefix = (TransactionTypePrefix)autoAddedPrefix.Clone(null, null);

			Assert(clonedAutoAddedPrefix.ReadOnly);
			Assert(!clonedPrefix.ReadOnly);
		}

		public void TestLedgerList()
		{
			AssertEquals("LedgerList.Count", 5, TestBizObj.LedgerList.Count);
			Assert(TestBizObj.LedgerList.ContainsCode("AR"));
			Assert(TestBizObj.LedgerList.ContainsCode("AP"));
			Assert(TestBizObj.LedgerList.ContainsCode("GL"));
			Assert(TestBizObj.LedgerList.ContainsCode("JC"));
			Assert(TestBizObj.LedgerList.ContainsCode("CB"));
		}

		public void TestTransactionTypeList()
		{
			AssertEquals(0, TestBizObj.TransactionTypeList.Count);

			TestBizObj.Ledger = "AR";
			AssertEquals("AR TransactionTypeList.Count", 11, TestBizObj.TransactionTypeList.Count);

			TestBizObj.Ledger = "AP";
			AssertEquals("AP TransactionTypeList.Count", 11, TestBizObj.TransactionTypeList.Count);

			TestBizObj.Ledger = "GL";
			AssertEquals("GL TransactionTypeList.Count", 4, TestBizObj.TransactionTypeList.Count);

			TestBizObj.Ledger = "CB";
			AssertEquals("CB TransactionTypeList.Count", 6, TestBizObj.TransactionTypeList.Count);

			TestBizObj.Ledger = "JC";
			AssertEquals("JC TransactionTypeList.Count", 2, TestBizObj.TransactionTypeList.Count);
		}

		public void TestValidateLedger()
		{
			var collection = new TransactionTypePrefixCollection();
			var prefix1 = collection.AddNew();
			prefix1.Ledger = "AR";
			prefix1.TransactionType = "INV";
			prefix1.Prefix = "A1";

			var prefix2 = collection.AddNew();
			prefix2.Ledger = "AP";
			prefix2.TransactionType = "INV";
			prefix2.Prefix = "A2";

			AssertNoErrors("Ledger should not have errors.", prefix1.LedgerInfo);

			prefix1.Ledger = "";
			AssertHasError(prefix1.LedgerInfo, "Please enter a value.");

			prefix1.Ledger = "A3";
			AssertHasError(prefix1.LedgerInfo, "Enter a valid selection.");

			prefix1.Ledger = "AP";
			AssertHasError(prefix1.LedgerInfo, "Ledger and Transaction Type must be unique.");
		}

		public void TestValidateTransactionType()
		{
			var collection = new TransactionTypePrefixCollection();
			var prefix1 = collection.AddNew();
			prefix1.Ledger = "AP";
			prefix1.TransactionType = "INV";
			prefix1.Prefix = "A1";

			var prefix2 = collection.AddNew();
			prefix2.Ledger = "AP";
			prefix2.TransactionType = "CRD";
			prefix2.Prefix = "A2";

			AssertHasError(prefix1.TransactionTypeInfo, "For INV, CRD and ADJ, you must specify a prefix for these transaction types when a prefix is specified for one of them. The system will enforce this rule for AR and AP ledger separately.");

			var prefix3 = collection.AddNew();
			prefix3.Ledger = "AP";
			prefix3.TransactionType = "ADJ";
			prefix3.Prefix = "A3";

			prefix1.TransactionType = "INV";
			AssertNoErrors("Ledger should not have errors.", prefix1.TransactionTypeInfo);

			prefix1.TransactionType = "";
			AssertHasError(prefix1.TransactionTypeInfo, "Please enter a value.");

			prefix1.TransactionType = "A3";
			AssertHasError(prefix1.TransactionTypeInfo, "Enter a valid selection.");

			prefix1.TransactionType = "CRD";
			AssertHasError(prefix1.TransactionTypeInfo, "Ledger and Transaction Type must be unique.");

			var prefix6 = collection.AddNew();
			prefix6.Ledger = "GL";
			prefix6.TransactionType = "AJL";
			prefix6.Prefix = "A4";

			AssertHasError(prefix6.TransactionTypeInfo, "For GJL, RJL, AJL and NJL, you must specify a prefix for these transaction types when a prefix is specified for one of them.");
		}

		public void TestValidatePrefix()
		{
			TestBizObj.Prefix = "";
			AssertHasError(TestBizObj.PrefixInfo, "Please enter a value.");
		}

		public void TestValidateSamePrefixForCTR()
		{
			var collection = new TransactionTypePrefixCollection();
			var prefix = collection.AddNew();
			prefix.Ledger = "AR";
			prefix.TransactionType = "CTR";
			prefix.Prefix = "XX";

			AssertEquals(2, collection.Count);

			var autoAddedPrefix = collection.Cast<TransactionTypePrefix>().FirstOrDefault(x => x.Ledger == "AP" && x.TransactionType == "CTR" && x.Prefix == "XX");
			autoAddedPrefix.Prefix = "CC";
			prefix.RunPreSaveValidationExcludingChildren();

			AssertHasRowError(prefix, "Prefix of AR/AP Contra must be same.");

			prefix.ClearAllNotifications();
			collection.RemoveAndDelete(autoAddedPrefix);
			prefix.RunPreSaveValidationExcludingChildren();
			AssertHasRowError(prefix, "Prefix of AR/AP Contra must be same.");
		}

		TransactionTypePrefix TestBizObj
		{
			get { return (TransactionTypePrefix)base.BizObj; }
		}

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new TransactionTypePrefix();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion
	}
}
