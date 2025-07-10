using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC043CSupportingDocumentProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("SupportingDocumentType missing", () => new CC043CSupportingDocumentProvider(null));
			});
		}

		public void TestSequenceNumber()
		{
			AssertEquals((ZShort)1, provider.SequenceNumber);
		}

		public void TestType()
		{
			AssertEquals("ABC", provider.Type);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("3412", provider.ReferenceNumber);
		}

		public void TestComplementOfInformation()
		{
			AssertEquals("8765", provider.ComplementOfInformation);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC043CSupportingDocumentProvider(new SupportingDocumentType02
			{
				SequenceNumber = "1",
				Type = "ABC",
				ReferenceNumber = "3412",
				ComplementOfInformation = "8765"
			});
		}
		CC043CSupportingDocumentProvider provider;
	}
}
