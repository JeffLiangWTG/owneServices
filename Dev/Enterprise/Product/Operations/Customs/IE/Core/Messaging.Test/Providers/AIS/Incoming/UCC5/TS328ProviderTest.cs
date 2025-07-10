using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.UCC5.Testing
{
	[TestedType(typeof(TS328Provider))]
	sealed class TS328ProviderTest : TestCaseWithFactory
	{
		public void TestMovementReferenceNumber()
		{
			AssertEquals("21IEDUB11A782454R2", provider.MovementReferenceNumber);
		}

		public void TestLRN()
		{
			AssertEquals("LRN123", provider.LRN);
		}

		public void TestAcceptanceDate()
		{
			AssertEquals(new DateTime(2023, 9, 5), provider.AcceptanceDate);
		}

		public void TestResponseDateLimit()
		{
			AssertEquals(new DateTime(2023, 10, 5), provider.ResponseDateLimit);
		}

		public void TestRemarks()
		{
			AssertEquals("remarks", provider.Remarks);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new TS328Provider(AISUCC5InterchangeProcessorTestHelper.CreateTS328Object("21IEDUB11A782454R2", "LRN123", new DateTime(2023, 9, 5, 12, 15, 0)));
		}
		TS328Provider provider;
	}
}
