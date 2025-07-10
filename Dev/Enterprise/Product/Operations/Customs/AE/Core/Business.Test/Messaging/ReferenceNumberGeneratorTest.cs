using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AE.Registry;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class ReferenceNumberGeneratorTest : TestCaseWithFactory
{
	[TestDate(2024, 07, 01, 01, 23, 45)]
	public void TestGenerateMessageReferenceNumber()
	{
		var generator = new ReferenceNumberGenerator(Factory);
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.Throws<ArgumentException>(() => generator.GenerateMessageReferenceNumber(ZString.Empty, "CAR"), "Application Code is empty");
			NUnit.Framework.Assert.Throws<ArgumentException>(() => generator.GenerateMessageReferenceNumber("AEC", ZString.Empty), "Message Type is empty");

			AssertEquals("First Message for AEC, CAR", "012345H0000001", generator.GenerateMessageReferenceNumber("AEC", "CAR"));
			AssertEquals("Same application code and type", "012345H0000002", generator.GenerateMessageReferenceNumber("AEC", "CAR"));
			AssertEquals("Different application code, same type", "012345H0000001", generator.GenerateMessageReferenceNumber("XYZ", "CAR"));
			AssertEquals("Same application code, different type", "012345H0000001", generator.GenerateMessageReferenceNumber("AEC", "XYZ"));
		});
	}

	[TestDate(2024, 07, 01, 01, 23, 45)]
	public void TestGenerateInterchangeReferenceNumber()
	{
		var generator = new ReferenceNumberGenerator(Factory);
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.Throws<ArgumentException>(() => generator.GenerateInterchangeReferenceNumber(ZString.Empty, "CAR"), "Application Code is empty");
			NUnit.Framework.Assert.Throws<ArgumentException>(() => generator.GenerateInterchangeReferenceNumber("AEC", ZString.Empty), "Message Type is empty");

			AssertEquals("First Interchange for AEC, CAR", "012345B0000001", generator.GenerateInterchangeReferenceNumber("AEC", "CAR"));
			AssertEquals("Same application code and type", "012345B0000002", generator.GenerateInterchangeReferenceNumber("AEC", "CAR"));
			AssertEquals("Different application code, same type", "012345B0000001", generator.GenerateInterchangeReferenceNumber("XYZ", "CAR"));
			AssertEquals("Same application code, different type", "012345B0000001", generator.GenerateInterchangeReferenceNumber("AEC", "XYZ"));
		});
	}

	public void TestGenerateDocumentReferenceNumber()
	{
		var generator = new ReferenceNumberGenerator(Factory);
		CombineAssertions(() =>
		{
			using (AECustomsRegistry.Instance.NAICServiceProviderCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SENDER"))
			{
				AssertEquals("First Reference number", "0SENDER0000000000001", generator.GenerateDocumentReferenceNumber());
				AssertEquals("Next Reference number", "0SENDER0000000000002", generator.GenerateDocumentReferenceNumber());
			}

			using (AECustomsRegistry.Instance.NAICServiceProviderCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "NEW_SENDER_ID"))
			{
				AssertEquals("Different Sender Id", "NEW_SEN0000000000001", generator.GenerateDocumentReferenceNumber());
			}
		});
	}
}
