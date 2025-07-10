using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	class ControlDetailsProviderTest : TestCase
	{
		ControlDetailsProvider provider;

		public void TestSequenceNumber()
		{
			provider = new ControlDetailsProvider(new MControlDetailsType { SequenceNumber = "1" });
			AssertEquals("1", provider.SequenceNumber);
		}

		public void TestTypeOfDiscrepancies()
		{
			provider = new ControlDetailsProvider(new MControlDetailsType { TypeOfDiscrepancies = "TD" });
			AssertEquals("TD", provider.TypeOfDiscrepancies);
		}

		public void TestAttributePointer()
		{
			provider = new ControlDetailsProvider(new MControlDetailsType { AttributePointer = "Attribute Pointer" });
			AssertEquals("Attribute Pointer", provider.AttributePointer);
		}

		public void TestCorrectedValue()
		{
			provider = new ControlDetailsProvider(new MControlDetailsType { CorrectedValue = "Corrected Value" });
			AssertEquals("Corrected Value", provider.CorrectedValue);
		}

		public void TestRemarks()
		{
			provider = new ControlDetailsProvider(new MControlDetailsType { Remarks = "Remarks001" });
			AssertEquals("Remarks001", provider.Remarks);
		}
	}
}
