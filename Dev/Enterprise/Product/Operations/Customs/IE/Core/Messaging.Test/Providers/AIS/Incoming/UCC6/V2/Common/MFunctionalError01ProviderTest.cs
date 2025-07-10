using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	sealed class MFunctionalError01ProviderTest : TestCaseWithFactory
	{
		public void TestSequenceNumber()
		{
			AssertEquals("1", provider.SequenceNumber);
		}

		public void TestErrorPointer()
		{
			AssertEquals("ErrorPointer001", provider.ErrorPointer);
		}

		public void TestErrorCode()
		{
			AssertEquals("13", provider.ErrorCode);
		}

		public void TestErrorReason()
		{
			AssertEquals("ER1", provider.ErrorReason);
		}

		public void TestRemarks()
		{
			AssertEquals("Functional Error Remarks 1", provider.Remarks);
		}

		public void TestOriginalAttributeValue()
		{
			AssertEquals("Original Attribute Value 1", provider.OriginalAttributeValue);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new MFunctionalError01Provider(new MFunctionalErrorType01
			{
				SequenceNumber = "1",
				ErrorPointer = "ErrorPointer001",
				ErrorCode = "13",
				ErrorReason = "ER1",
				Remarks = "Functional Error Remarks 1",
				OriginalAttributeValue = "Original Attribute Value 1",
			});
		}
		MFunctionalError01Provider provider;
	}
}
