using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	[TestedType(typeof(TS304Provider))]
	sealed class TS304ProviderTest : TestCaseWithFactory
	{
		public void TestMovementReferenceNumber()
		{
			AssertEquals("21IEDUB11A782454R2", provider.MovementReferenceNumber);
		}

		public void TestDateOfAcceptance()
		{
			AssertEquals(new DateTime(2023, 09, 5, 12, 15, 30), provider.DateOfAcceptance);
		}

		public void TestRemarks()
		{
			AssertEquals("Test Remarks", provider.Remarks);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new TS304Provider(AISInterchangeProcessorTestHelper.CreateTS304Object("21IEDUB11A782454R2", new DateTime(2023, 9, 5, 12, 15, 30), "Test Remarks"));
		}
		TS304Provider provider;
	}
}
