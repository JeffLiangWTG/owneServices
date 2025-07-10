using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.DocWrappers.Testing
{
	sealed class DocWrapperCopyInfoTest : TransactionedTestCase
	{
		public void TestTitleCopyCountPair()
		{
			DocWrapperCopyInfoForTesting info = new DocWrapperCopyInfoForTesting();
			TitleCopyCountPair pair = info.TitleCopyCountPair;
			AssertEquals("title default", "Document", pair.Title);
			AssertEquals("CopyCount default", (short)1, pair.CopyCount);
		}

		public void TestName()
		{
			DocWrapperCopyInfoForTesting info = new DocWrapperCopyInfoForTesting();
			AssertEquals("'Document' by default", "Document", info.Name);

			info.Name = (NoResString)"test";
			AssertEquals("Should now be set to test", "test", info.Name);

			info.Name = (NoResString)"hello";
			AssertEquals("Should now be set to hello", "hello", info.Name);
		}

		public void TestCopyCount()
		{
			DocWrapperCopyInfoForTesting info = new DocWrapperCopyInfoForTesting();
			AssertEquals("default", (short)1, info.CopyCount);

			info.CopyCount = 2;
			AssertEquals("Should now be set to 2", (short)2, info.CopyCount);

			info.CopyCount = 5;
			AssertEquals("Should now be set to 5", (short)5, info.CopyCount);
		}

		public void TestDeliveryMethod()
		{
			DocWrapperCopyInfoForTesting info = new DocWrapperCopyInfoForTesting();
			AssertEquals("default DeliveryMethod", PrintCopyType.ALL, info.DeliveryMethod);

			info.DeliveryMethod = PrintCopyType.PRN;
			AssertEquals("should now be PRN", PrintCopyType.PRN, info.DeliveryMethod);

			info.DeliveryMethod = PrintCopyType.EML;
			AssertEquals("Should now be EML", PrintCopyType.EML, info.DeliveryMethod);
		}
	}
}
