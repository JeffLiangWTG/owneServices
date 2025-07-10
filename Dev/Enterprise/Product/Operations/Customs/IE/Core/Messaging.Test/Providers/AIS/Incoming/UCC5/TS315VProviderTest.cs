using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.UCC5.Testing
{
	[TestedType(typeof(TS315VProvider))]
	sealed class TS315VProviderTest : TestCaseWithFactory
	{
		public void TestMovementReferenceNumber()
		{
			AssertEquals("21IEDUB11A782454R2", provider.MovementReferenceNumber);
		}

		public void TestLRN()
		{
			AssertEquals("LRN123", provider.LRN);
		}

		public void TestAcknowledgementDate()
		{
			AssertEquals(new DateTime(2023, 9, 5), provider.AcknowledgementDate);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new TS315VProvider(AISUCC5InterchangeProcessorTestHelper.CreateTS315VObject("21IEDUB11A782454R2", "LRN123", new DateTime(2023, 9, 5, 12, 15, 0)));
		}
		TS315VProvider provider;
	}
}
