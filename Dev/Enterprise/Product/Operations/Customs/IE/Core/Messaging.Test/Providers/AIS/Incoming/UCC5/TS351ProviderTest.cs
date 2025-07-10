using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.UCC5.Testing
{
	[TestedType(typeof(TS351Provider))]
	sealed class TS351ProviderTest : TestCaseWithFactory
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
			AssertEquals(new DateTime(2023, 9, 5), provider.ControlDate);
		}

		public void TestControlResultRemarks()
		{
			AssertEquals("control remarks", provider.ControlResultRemarks);
		}

		public void TestRemarks()
		{
			AssertEquals("Test Remarks", provider.Remarks);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new TS351Provider(AISUCC5InterchangeProcessorTestHelper.CreateTS351Object("21IEDUB11A782454R2", "LRN123", new DateTime(2023, 9, 5, 12, 15, 0), "Test Remarks"));
		}
		TS351Provider provider;
	}
}
