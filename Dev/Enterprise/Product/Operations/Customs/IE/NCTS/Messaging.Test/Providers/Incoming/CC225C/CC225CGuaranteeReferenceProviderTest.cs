using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.tcl;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	public class CC225CGuaranteeReferenceProviderTest : TestCaseWithFactory
	{
		public void TestConstructor_Null()
		{
			AssertExceptionThrown<ArgumentException>("Should throw ArgumentException when calling constructor passing a null input", () => new CC225CGuaranteeReferenceProvider(null));
		}

		public void TestConstructorAndFieldsWithEmptyInputs()
		{
			CombineAssertions("Should not throw exception when creating using empty input and accessing fields", () =>
			{
				CC225CGuaranteeReferenceProvider testItem = null;
				AssertNoExceptionThrown("Should not throw exception when calling constructor passing an ", () => testItem = new CC225CGuaranteeReferenceProvider(new GuaranteeReferenceType09()));
				AssertEquals("GRN empty", ZString.Empty, testItem.GRN);
				AssertEquals("Currency empty", ZString.Empty, testItem.Currency);
				AssertEquals("ReferenceAmount empty", ZDecimal.Zero, testItem.ReferenceAmount);
				AssertEquals("PercentageOfReferenceAmount empty", ZDecimal.Zero, testItem.PercentageOfReferenceAmount);
				AssertEquals("GuaranteeAmount empty", ZDecimal.Zero, testItem.GuaranteeAmount);
				AssertEquals("NumberOfCertificates empty", ZInt.Zero, testItem.NumberOfCertificates);
				AssertEquals("ValidityDate empty", ZDateTime.Empty, testItem.ValidityDate);
				AssertEquals("InvalidityDate empty", ZDateTime.Empty, testItem.InvalidityDate);
				AssertEquals("InvalidityReasonCode empty", ZString.Empty, testItem.InvalidityReasonCode);
				AssertEquals("RestrictedUseSuspendedGoods empty", false, testItem.RestrictedUseSuspendedGoods);
				AssertEquals("CustomOfficeOfGuaranteeReferenceNumber empty", ZString.Empty, testItem.CustomOfficeOfGuaranteeReferenceNumber);
			});
		}

		public void TestFieldsWithValidInputs()
		{
			var testProvider = new CC225CGuaranteeReferenceProvider(CreateStandardProvider());

			CombineAssertions("CC225CGuaranteeReferenceProvider fields", () =>
			{
				AssertEquals("SequenceNumber", 1, testProvider.SequenceNumber);

				AssertEquals("GRN", "12GRNCC055C012345A678901", testProvider.GRN);
				AssertEquals("Currency", "GBP", testProvider.Currency);
				AssertEquals("ReferenceAmount", 123m, testProvider.ReferenceAmount);
				AssertEquals("PercentageOfReferenceAmount", 20m, testProvider.PercentageOfReferenceAmount);
				AssertEquals("GuaranteeAmount", 234m, testProvider.GuaranteeAmount);

				AssertEquals("NumberOfCertificates", 45, testProvider.NumberOfCertificates);
				AssertEquals("ValidityDate", new DateTime(2023, 7, 25), testProvider.ValidityDate);
				AssertEquals("InvalidityDate", new DateTime(2023, 7, 26), testProvider.InvalidityDate);
				AssertEquals("InvalidityReasonCode", "ABC", testProvider.InvalidityReasonCode);
				AssertEquals("RestrictedUseSuspendedGoods", ZBool.True, testProvider.RestrictedUseSuspendedGoods);

				AssertEquals("CustomOfficeOfGuaranteeReferenceNumber", "RNCC037C", testProvider.CustomOfficeOfGuaranteeReferenceNumber);
			});
		}

		public void TestFieldsWithValidInputs_AlternativeInputs()
		{
			var testProvider = new CC225CGuaranteeReferenceProvider(new GuaranteeReferenceType09
			{
				NumberOfCertificates = "XXX",
				RestrictedUseSuspendedGoods = Flag.Item0,
			});

			AssertEquals("NumberOfCertificates", 0, testProvider.NumberOfCertificates);
			AssertEquals("RestrictedUseSuspendedGoods", ZBool.False, testProvider.RestrictedUseSuspendedGoods);
		}

		public static GuaranteeReferenceType09 CreateStandardProvider() => new GuaranteeReferenceType09
		{
			SequenceNumber = "1",
			Grn = "12GRNCC055C012345A678901",
			Currency = "GBP",
			ReferenceAmount = 123m,
			PercentageOfReferenceAmount = "20",
			GuaranteeAmount = 234m,
			NumberOfCertificates = "45",
			ValidityDate = new DateTime(2023, 7, 25),
			InvalidityDate = new DateTime(2023, 7, 26),
			InvalidityReasonCode = "ABC",
			RestrictedUseSuspendedGoods = Flag.Item1,
			CustomsOfficeOfGuarantee = new CustomsOfficeOfGuaranteeType02 { ReferenceNumber = "RNCC037C" }
		};
	}
}
