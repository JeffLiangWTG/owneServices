using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	[TestedType(typeof(CC043CPreviousDocumentProvider))]
	sealed class CC043CPreviousDocumentProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("previousDocument missing", () => new CC043CPreviousDocumentProvider(null));
			});
		}

		public void TestComplementOfInformation()
		{
			AssertEquals("Complement of Information", "Test", provider.ComplementOfInformation);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("Reference Number", "12345", provider.ReferenceNumber);
		}

		public void TestType()
		{
			AssertEquals("Type", "TRA", provider.Type);
		}

		public void TestSequenceNumber()
		{
			AssertEquals("Sequence Number", (CargoWise.Types.ZShort)1, provider.SequenceNumber);
		}

		protected override void SetUp()
		{
			provider = new CC043CPreviousDocumentProvider(new PreviousDocumentType06()
			{
				SequenceNumber = "1",
				Type = "TRA",
				ReferenceNumber = "12345",
				ComplementOfInformation = "Test",
			});
		}
		CC043CPreviousDocumentProvider provider;
	}
}
