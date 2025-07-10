using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.tcl;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	public class CC037CComprehensiveGuaranteeProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("ComprehensiveGuaranteeType missing", () => new CC037CComprehensiveGuaranteeProvider(null));
			});
		}
		public void TestReferenceAmount()
		{
			AssertEquals("ReferenceAmount", 7M, provider.ReferenceAmount);
		}

		public void TestPercentageOfReferenceAmount()
		{
			AssertEquals("PercentageOfReferenceAmount", "45", provider.PercentageOfReferenceAmount);
		}

		public void TestGuaranteeAmount()
		{
			AssertEquals("GuaranteeAmount", 12M, provider.GuaranteeAmount);
		}

		public void TestCurrency()
		{
			AssertEquals("Currency", "USD", provider.Currency);
		}

		public void TestNumberOfCertificates()
		{
			AssertEquals("NumberOfCertificates", "3", provider.NumberOfCertificates);
		}

		public void TestValidityStartDate()
		{
			AssertEquals("ValidityStartDate", new ZDate(2023, 02, 11), provider.ValidityStartDate);
		}

		public void TestValidityEndDate()
		{
			AssertEquals("ValidityEndDate", new ZDate(2023, 02, 15), provider.ValidityEndDate);
		}

		public void TestInvalidityReasonCode()
		{
			AssertEquals("InvalidityReasonCode", "AB8", provider.InvalidityReasonCode);
		}

		public void TestInvalidityReasonText()
		{
			AssertEquals("InvalidityReasonText", "Some reason text", provider.InvalidityReasonText);
		}

		public void TestLiabilityLiberationDate()
		{
			AssertEquals("LiabilityLiberationDate", new ZDate(2023, 05, 11), provider.LiabilityLiberationDate);
		}

		public void TestRestrictedUseForSuspendedGoods()
		{
			AssertEquals("RestrictedUseForSuspendedGoods", "1", provider.RestrictedUseForSuspendedGoods);
		}

		public void TestValidityLimitation()
		{
			AssertEquals("ValidityLimitation is null", 0, GetEmptyProvider().ValidityLimitations.Count);

			var validityLimitation1 = provider.ValidityLimitations;
			AssertType<CC037CValidityLimitationProvider>(validityLimitation1.ElementAt(0));
			var validityLimitation2 = provider.ValidityLimitations;
			AssertSame("Is cached", validityLimitation1, validityLimitation2);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC037CComprehensiveGuaranteeProvider(new ComprehensiveGuaranteeType
			{
				ReferenceAmount = 7,
				PercentageOfReferenceAmount = "45",
				GuaranteeAmount = 12,
				Currency = "USD",
				NumberOfCertificates = "3",
				ValidityStartDate = new DateTime(2023, 02, 11, 2, 3, 6),
				ValidityEndDate = new DateTime(2023, 02, 15, 2, 4, 6),
				InvalidityReasonCode = "AB8",
				InvalidityReasonText = "Some reason text",
				LiabilityLiberationDate = new DateTime(2023, 05, 11, 2, 4, 6),
				RestrictedUseForSuspendedGoods = Flag.Item1,
				ValidityLimitation = new Collection<ValidityLimitationType>
				{
					new ValidityLimitationType
					{
						SequenceNumber = "1",
						GuaranteeNotValidIn = "AB"
					},
					new ValidityLimitationType
					{
						SequenceNumber = "2",
						GuaranteeNotValidIn = "DE"
					}
				}
			});
		}
		CC037CComprehensiveGuaranteeProvider provider;

		CC037CComprehensiveGuaranteeProvider GetEmptyProvider()
		{
			return new CC037CComprehensiveGuaranteeProvider(new ComprehensiveGuaranteeType());
		}
	}
}
