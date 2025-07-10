using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	public class CC228CGuaranteeReferenceProviderTest : TestCaseWithFactory
	{
		public void TestConstructor_Null()
		{
			AssertExceptionThrown<ArgumentException>("Should throw ArgumentException when calling constructor passing a null input", () => new CC228CGuaranteeReferenceProvider(null));
		}

		public void TestConstructorAndFieldsWithEmptyInputs()
		{
			CombineAssertions("Should not throw exception when creating using empty input and accessing fields", () =>
			{
				CC228CGuaranteeReferenceProvider testItem = null;
				AssertNoExceptionThrown("Should not throw exception when calling constructor passing an ", () => testItem = new CC228CGuaranteeReferenceProvider(new GuaranteeReferenceType10()));
				AssertEquals("GRN empty", ZString.Empty, testItem.GRN);
				AssertEquals("Currency empty", ZString.Empty, testItem.Currency);
				AssertEquals("GuaranteeAmount empty", ZDecimal.Zero, testItem.GuaranteeAmount);
				AssertEquals("InvalidityDate empty", ZDateTime.Empty, testItem.InvalidityDate);
				AssertEquals("CustomOfficeOfGuaranteeReferenceNumber empty", ZString.Empty, testItem.CustomOfficeOfGuaranteeReferenceNumber);
				AssertEquals("LiabilityLiberationDate empty", ZDateTime.Empty, testItem.LiabilityLiberationDate);
			});
		}

		public void TestFieldsWithValidInputs()
		{
			var testProvider = new CC228CGuaranteeReferenceProvider(CreateStandardProvider());

			CombineAssertions("CC228CGuaranteeReferenceProvider fields", () =>
			{
				AssertEquals("SequenceNumber", 1, testProvider.SequenceNumber);
				AssertEquals("GRN", "12GRNCC055C012345A678901", testProvider.GRN);
				AssertEquals("Currency", "GBP", testProvider.Currency);
				AssertEquals("GuaranteeAmount", 234m, testProvider.GuaranteeAmount);
				AssertEquals("InvalidityDate", new DateTime(2023, 7, 26, 11, 21, 31), testProvider.InvalidityDate);
				AssertEquals("CustomOfficeOfGuaranteeReferenceNumber", "RNCC037C", testProvider.CustomOfficeOfGuaranteeReferenceNumber);
				AssertEquals("LiabilityLiberationDate", new DateTime(2023, 7, 26, 14, 52, 18), testProvider.LiabilityLiberationDate);
			});
		}

		public static GuaranteeReferenceType10 CreateStandardProvider() => new GuaranteeReferenceType10
		{
			SequenceNumber = "1",
			Grn = "12GRNCC055C012345A678901",
			Currency = "GBP",
			GuaranteeAmount = 234m,
			InvalidityDate = new DateTime(2023, 7, 26, 11, 21, 31),
			CustomsOfficeOfGuarantee = new CustomsOfficeOfGuaranteeType02 { ReferenceNumber = "RNCC037C" },
			LiabilityLiberationDate = new DateTime(2023, 7, 26, 14, 52, 18),
		};
	}
}
