using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IssueManager.Business.Test
{
	[TestedType(typeof(HelpErrorLogKey))]
	class HelpErrorLogKeyTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIssue()
		{
			HelpErrorLogKey key = Factory.New<HelpErrorLogKey>();
			AssertNull(key.Issue);

			EdiHelpErrorLog log = Factory.New<EdiHelpErrorLog>();
			key.HK_HE = log.PK;
			AssertEquals(log, key.Issue);
		}

		public void TestGetKeyHashCode()
		{
			AssertEquals(HelpErrorLogKey.GetKeyHashCode("key1"), HelpErrorLogKey.GetKeyHashCode("key1"));
			AssertNotEquals(HelpErrorLogKey.GetKeyHashCode("key1"), HelpErrorLogKey.GetKeyHashCode("key2"));
			AssertEquals(HelpErrorLogKey.GetKeyHashCode("key1"), HelpErrorLogKey.GetKeyHashCode("key1\0has more info"));
		}
	}
}
