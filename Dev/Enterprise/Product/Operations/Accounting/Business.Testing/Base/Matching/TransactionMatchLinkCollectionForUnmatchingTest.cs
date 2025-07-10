using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	[TestedType(typeof(TransactionMatchLinkCollectionForUnmatching))]
	public class TransactionMatchLinkCollectionForUnmatchingTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new TransactionMatchLinkCollectionForUnmatching(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData(typeof(TransactionMatchLink));
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestLoadNotSupported()
		{
			Collection.Load();
		}
	}
}
