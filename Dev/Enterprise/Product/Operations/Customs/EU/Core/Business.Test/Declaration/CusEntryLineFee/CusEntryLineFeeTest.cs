using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public abstract class CusEntryLineFeeTest<TDeclaration, TEntryLine, TEntryLineFee> : Customs.Business.Testing.CusEntryLineFeeTest
		where TDeclaration : JobDeclaration
		where TEntryLine : CusEntryLine
		where TEntryLineFee : CusEntryLineFee
	{
		public void TestTypeDecider()
		{
			var entryLineFee = SetEntryLineFeeData().entryLineFee;
			entryLineFee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.Vat;
			entryLineFee.CF_ChargeAmount = 10m;
			Factory.Save();
			AssertEquals("Update Customs.Business.CusEntryLineFee to include a decider for this class", GetExpectedBusinessObjectType(), new BusinessObjectFactory().Load<Customs.Business.CusEntryLineFee>(entryLineFee.PK).GetType());
		}

		#region Implementation

		protected virtual (TDeclaration declaration, TEntryLine entryLine, TEntryLineFee entryLineFee) SetEntryLineFeeData()
		{
			var declaration = Factory.New<TDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var entryLine = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First().MergedLines.Cast<TEntryLine>()
				.First();
			var entryLineFee = (TEntryLineFee)entryLine.Fees.AddNew();
			return (declaration, entryLine, entryLineFee);
		}

		public void TestChargeAmountRounder()
		{
			var entryLineFee = SetEntryLineFeeData().entryLineFee;

			AssertNotNull("ChargeAmountRounder", entryLineFee.ChargeAmountRounder);

			var chargeAmountRounder = entryLineFee.ChargeAmountRounder;
			AssertSame("ChargeAmountRounder should be cached", chargeAmountRounder, entryLineFee.ChargeAmountRounder);
		}

		#endregion
	}

	[TestedType(typeof(CusEntryLineFee))]
	sealed class CusEntryLineFee_DoNotInheritTest : CusEntryLineFeeTest<JobDeclaration, CusEntryLine, CusEntryLineFee>
	{
		public void TestUserEnteredStashSource()
		{
			var entryLineFee = SetEntryLineFeeData().entryLineFee;
			AssertType<CusEntryLineFeeUserEnteredStashSource>(entryLineFee.UserEnteredStashSource);
		}

		public void TestIncludeForVatCalculation()
		{
			var entryLineFee = SetEntryLineFeeData().entryLineFee;
			AssertEquals("IncludeForVatCalculation should always be tue", true, entryLineFee.IncludeForVatCalculation);
		}

		public void TestShouldResetDataOnMergingCore()
		{
			var dec = Factory.New<JobDeclaration>();
			var header = dec.CustomsEntryHeaders.AddNew();
			var line = header.MergedLines.AddNew();
			var fee = line.Fees.AddNew();
			fee.CF_ChargeType = "DTY";
			fee.CF_ChargeAmount = 69m;
			Factory.Save();
			AssertEquals(69m, fee.CF_ChargeAmount);
			dec.ZG_VATDeferType = "A"; // make any change
			Factory.Save();
			AssertNotNull(fee);
			AssertEquals("Editing dec and hence re-merger shoudl not have clobbered the fees", 69m, fee.CF_ChargeAmount);
		}

		public void TestReadOnlyFields()
		{
			var entryLineFee = SetEntryLineFeeData().entryLineFee;
			entryLineFee.G4_RateOverride = "";

			AssertEquals("G4_MethodOfPayment isn't ReadOnly", false, entryLineFee.G4_MethodOfPaymentInfo.ReadOnly);
			AssertEquals("G4_RateOverride isn't ReadOnly", false, entryLineFee.G4_RateOverrideInfo.ReadOnly);

			AssertEquals("G4_RateDuty is ReadOnly", true, entryLineFee.G4_RateDutyInfo.ReadOnly);
			AssertEquals("G4_Type is ReadOnly", true, entryLineFee.G4_TypeInfo.ReadOnly);
			AssertEquals("G4_Amount is ReadOnly", true, entryLineFee.G4_AmountInfo.ReadOnly);
			AssertEquals("G4_BaseAmount is ReadOnly", true, entryLineFee.G4_BaseAmountInfo.ReadOnly);
			AssertEquals("G4_RateSuspension is ReadOnly", true, entryLineFee.G4_RateSuspensionInfo.ReadOnly);

			entryLineFee.G4_RateOverride = "ADD";
			AssertEquals("G4_MethodOfPayment isn't ReadOnly", false, entryLineFee.G4_MethodOfPaymentInfo.ReadOnly);
			AssertEquals("G4_RateOverride isn't ReadOnly", false, entryLineFee.G4_RateOverrideInfo.ReadOnly);
			AssertEquals("G4_RateDuty isn't ReadOnly", false, entryLineFee.G4_RateDutyInfo.ReadOnly);
			AssertEquals("G4_Type isn't ReadOnly", false, entryLineFee.G4_TypeInfo.ReadOnly);
			AssertEquals("G4_BaseAmount isn't ReadOnly", false, entryLineFee.G4_BaseAmountInfo.ReadOnly);
			AssertEquals("G4_RateSuspension isn't ReadOnly", false, entryLineFee.G4_RateSuspensionInfo.ReadOnly);
			AssertEquals("G4_Amount is ReadOnly", false, entryLineFee.G4_AmountInfo.ReadOnly);
		}

		public void TestTotalAmount_AutomaticallyCalculatedWhenActionAddOrOvr()
		{
			var entrylineFee = SetEntryLineFeeData().entryLineFee;
			entrylineFee.CF_BaseValue = 1000;
			entrylineFee.CF_MethodOfCalculation = ZString.Empty;
			AssertEquals("0.00", entrylineFee.G4_Amount);
			entrylineFee.CF_MethodOfCalculation = Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage;
			entrylineFee.CF_Rate = 25;
			entrylineFee.G4_RateOverride = RateOverrideReasonList.Codes.Additional;
			AssertEquals("250.00", entrylineFee.G4_Amount);
		}

		public void TestCountryCode()
		{
			var entryLineFee = SetEntryLineFeeData().entryLineFee;
			AssertEquals("CusEntryLineFee CountryCode is the CurrentCompany CountryCode", Env.CurrentCompany.Country.Code, entryLineFee.CountryCode);
		}

		public void TestImportExportParent()
		{
			(_, CusEntryLine entryLine, CusEntryLineFee entryLineFee) = SetEntryLineFeeData();
			AssertEquals("ImportExportParent is the EntryLine of the entryLineFee", entryLine, entryLineFee.ImportExportParent);
		}

		public void TestMethodOfPayment()
		{
			var entryLineFee = SetEntryLineFeeData().entryLineFee;
			entryLineFee.CF_MethodOfPayment = ZString.Empty;
			entryLineFee.G4_MethodOfPayment = "MoP";
			AssertEquals("MoP", entryLineFee.G4_MethodOfPayment);
			AssertEquals("MoP", entryLineFee.CF_MethodOfPayment);
		}

		public void TestMethodOfCalculation()
		{
			var entryLineFee = SetEntryLineFeeData().entryLineFee;
			entryLineFee.CF_MethodOfCalculation = ZString.Empty;
			entryLineFee.G4_RateDuty = "123";
			AssertEquals("123", entryLineFee.G4_RateDuty);
			AssertEquals("123", entryLineFee.CF_MethodOfCalculation);
		}

		public void TestRateOverrideReasonCode()
		{
			var entryLineFee = SetEntryLineFeeData().entryLineFee;
			entryLineFee.CF_RateOverrideReasonCode = ZString.Empty;
			entryLineFee.G4_RateOverride = "123";
			AssertEquals("123", entryLineFee.G4_RateOverride);
			AssertEquals("123", entryLineFee.CF_RateOverrideReasonCode);
		}

		public void TestChargeTypeDescription()
		{
			var entryLineFee = SetEntryLineFeeData().entryLineFee;
			var code = entryLineFee.Lookups.ChargeTypeList.GetAllCodes().First();
			var description = entryLineFee.Lookups.ChargeTypeList.GetDescriptionFromCode(code);
			entryLineFee.CF_ChargeType = code;
			AssertEquals(description, entryLineFee.ChargeTypeDescription);
		}

		public void TestChargeType()
		{
			var entryLineFee = SetEntryLineFeeData().entryLineFee;
			entryLineFee.CF_ChargeType = ZString.Empty;
			entryLineFee.G4_Type = "D";
			AssertEquals("D", entryLineFee.G4_Type);
			AssertEquals("D", entryLineFee.CF_ChargeType);
		}

		public void TestChargeAmount()
		{
			var entryLineFee = SetEntryLineFeeData().entryLineFee;
			entryLineFee.CF_ChargeAmount = 20;
			entryLineFee.G4_Amount = "2.00";
			AssertEquals(nameof(CusEntryLineFee.G4_Amount), "2.00", entryLineFee.G4_Amount);
			AssertEquals(nameof(CusEntryLineFee.CF_ChargeAmount), (ZDecimal)2.00, entryLineFee.CF_ChargeAmount);

			entryLineFee.CF_ChargeAmount = 20.1234;
			AssertEquals(nameof(CusEntryLineFee.CF_ChargeAmount), (ZDecimal)20.1234, entryLineFee.CF_ChargeAmount);

			entryLineFee.CF_ChargeAmount = 20.49998;
			AssertEquals($"Default rounding for {nameof(CusEntryLineFee.CF_ChargeAmount)}", (ZDecimal)20.5000, entryLineFee.CF_ChargeAmount);
		}

		public void TestBaseValue()
		{
			var entryLineFee = SetEntryLineFeeData().entryLineFee;
			entryLineFee.CF_BaseValue = 0;
			entryLineFee.G4_BaseAmount = 1.1;
			AssertEquals((ZDecimal)1.1, entryLineFee.G4_BaseAmount);
			AssertEquals((ZDecimal)1.1, entryLineFee.CF_BaseValue);
		}

		public void TestBaseValueDecimalPlaces()
		{
			var expectedMessageError = "Base Amount allows only 2 decimal places.";
			using (ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, true))
			{
				var entryLineFee = SetEntryLineFeeData().entryLineFee;
				entryLineFee.G4_BaseAmount = 5.123456;
				AssertHasMessageErrorContaining("Decimal places message error for wrong base value with UseUniversalFeeCalculation true", entryLineFee.G4_BaseAmountInfo, expectedMessageError);
			}
		}

		public void TestRate()
		{
			var entryLineFee = SetEntryLineFeeData().entryLineFee;
			entryLineFee.CF_Rate = 0;
			entryLineFee.G4_RateSuspension = "1.1";
			AssertEquals("1.100000", entryLineFee.G4_RateSuspension);
			AssertEquals((ZDecimal)1.1, entryLineFee.CF_Rate);
		}

		public void TestRateDecimalPlaces()
		{
			var entryLineFee = SetEntryLineFeeData().entryLineFee;

			entryLineFee.CF_Rate = 0.06;
			AssertEquals("G4_RateSuspension", "0.060000", entryLineFee.G4_RateSuspension);
			entryLineFee.CF_Rate = 5.1234567;
			AssertEquals("G4_RateSuspension", "5.1234567", entryLineFee.G4_RateSuspension);

			var expectedMessageError = "Tax Rate allows only 6 decimal places.";
			using (ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, true))
			{
				entryLineFee = SetEntryLineFeeData().entryLineFee;
				entryLineFee.G4_RateSuspension = "5.1234567";
				AssertHasMessageErrorContaining("Decimal places message error for wrong rate with UseUniversalFeeCalculation true", entryLineFee.G4_RateSuspensionInfo, expectedMessageError);
			}
		}

		public void TestChargeAmountRounderType()
		{
			var entryLineFee = SetEntryLineFeeData().entryLineFee;
			AssertType<FeeNoRounder>($"{nameof(CusEntryLineFee.ChargeAmountRounder)} type", entryLineFee.ChargeAmountRounder);
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestG4_CalculatedPercentage()
		{
			var entryLineFee = Factory.New<CusEntryLineFee>();
			var exception = entryLineFee.G4_CalculatedPercentage;
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestG4_BaseQuantity()
		{
			var entryLineFee = Factory.New<CusEntryLineFee>();
			var exception = entryLineFee.G4_BaseQuantity;
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestG4_BaseQuantityUQ()
		{
			var entryLineFee = Factory.New<CusEntryLineFee>();
			var exception = entryLineFee.G4_BaseQuantityUQ;
		}

		public void TestEventsAreUnhookedOnDispose()
		{
			ChargeAmountRefresherForTest refresher;
			using (var cusEntryLineFee = Factory.New<CusEntryLineFeeForTestWithCustomRefresher>())
			{
				refresher = (ChargeAmountRefresherForTest)cusEntryLineFee.ChargeAmountRefresher;
				Assert("Events are hooked", !refresher.UnhookedEvents);
			}

			Assert("UnHookedEvents", refresher.UnhookedEvents);
		}

		public void TestIDocSADHLineTaxBoxSupporterMembers()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_PaymentMethod = "R";
			var header = dec.CustomsEntryHeaders.AddNew();
			var line = header.MergedLines.AddNew();
			var fee = line.Fees.AddNew();
			fee.CF_ChargeType = "A00";
			fee.CF_BaseValue = 122.12m;
			fee.CF_Rate = 22.00m;
			fee.CF_ChargeAmount = 99.99m;
			fee.CF_MethodOfPayment = "A";
			fee.NationalFeeTypeCode = "A445";
			CombineAssertions(() =>
			{
				var taxBoxSupporter = (IDocSADHLineTaxBoxSupporter)fee;
				AssertEquals("AmountInDeclarationCurrency", "99.99", taxBoxSupporter.AmountInDeclarationCurrency);
				AssertEquals("MethodOfPayment", "A", taxBoxSupporter.MethodOfPayment);
				AssertEquals("Rate", "22.00", taxBoxSupporter.Rate);
				AssertEquals("RateDuty", "", taxBoxSupporter.RateDuty);
				AssertEquals("RateOverride", "", taxBoxSupporter.RateOverride);
				AssertEquals("TaxBase", "122.12", taxBoxSupporter.TaxBase);
				AssertEquals("Type", "A00", taxBoxSupporter.Type);
				AssertEquals("NationalFeeTypeCode", "A445", taxBoxSupporter.NationalFeeTypeCode);
				AssertEquals("DeclarationMethodOfPayment", "R", taxBoxSupporter.DeclarationMethodOfPayment);
			});
		}

		public void TestDefaultMethodOfPaymentIfEmpty()
		{
			var entryLineFee = Factory.New<CusEntryLineFeeForTest>();
			entryLineFee.DefaultMethodOfPaymentIfEmpty();
			AssertEquals("Default Method of Payment", "X", entryLineFee.CF_MethodOfPayment);

			entryLineFee.CF_MethodOfPayment = "A1";
			entryLineFee.DefaultMethodOfPaymentIfEmpty();
			AssertEquals("Default Method of Payment", "A1", entryLineFee.CF_MethodOfPayment);
		}

		public void TestAllowNegativeAmount()
		{
			var entryLineFee = Factory.New<CusEntryLineFeeForTest>();
			Assert("AllowNegativeAmount default value should be FALSE", !entryLineFee.AllowNegativeAmount);
		}

		public void TestAllowZeroOrEmptyAmount()
		{
			var entryLineFee = Factory.New<CusEntryLineFeeForTest>();
			Assert("Zero Amount is not allowed", !entryLineFee.AllowZeroOrEmptyAmount);
		}

		public void TestChargeAmountRefresher()
		{
			var lineFee = Factory.New<CusEntryLineFee>();
			AssertType<ChargeAmountRefresher>("ChargeAmountRefresher Type", lineFee.ChargeAmountRefresher);
		}

		public void TestSetCF_ChargeAmount()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var line = entryHeader.AllEntryLines.AddNew();
			var fee = line.Fees.AddNew();
			fee.CF_ChargeType = "B00";
			fee.CF_ChargeAmount = 12m;
			AssertEquals(12m, entryHeader.CH_TotalPaid);
		}

		class CusEntryLineFeeForTest : CusEntryLineFee
		{
			public CusEntryLineFeeForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override ZString GetDefaultMethodOfPaymentValue() => "X";
		}

		class CusEntryLineFeeForTestWithCustomRefresher : CusEntryLineFee
		{
			public CusEntryLineFeeForTestWithCustomRefresher(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override ChargeAmountRefresher GetNewChargeAmountRefresher()
			{
				return new ChargeAmountRefresherForTest(this);
			}
		}

		class ChargeAmountRefresherForTest : ChargeAmountRefresher
		{
			public ChargeAmountRefresherForTest(CusEntryLineFee lineFee) : base(lineFee)
			{
			}

			protected override void UnhookEventsCore()
			{
				base.UnhookEventsCore();
				UnhookedEvents = true;
			}

			public bool UnhookedEvents;
		}
	}
}
