using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.UCC5.Testing
{
	[TestedType(typeof(TS304Provider))]
	sealed class TS304ProviderTest : TestCaseWithFactory
	{
		public void TestMovementReferenceNumber()
		{
			AssertEquals("21IEDUB11A782454R2", provider.MovementReferenceNumber);
		}

		public void TestAmendmentAcceptanceDate()
		{
			AssertEquals(new DateTime(2023, 9, 5), provider.AmendmentAcceptanceDate);
		}

		public void TestRemarks()
		{
			AssertEquals("Test Remarks", provider.Remarks);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new TS304Provider(AISUCC5InterchangeProcessorTestHelper.CreateTS304Object("21IEDUB11A782454R2", new DateTime(2023, 9, 5, 12, 15, 0), "Test Remarks"));
		}
		TS304Provider provider;
	}
}
