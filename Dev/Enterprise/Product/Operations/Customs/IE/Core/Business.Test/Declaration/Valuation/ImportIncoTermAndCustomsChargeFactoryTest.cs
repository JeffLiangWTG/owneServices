using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class ImportIncoTermAndCustomsChargeFactoryTest : EUIncoTermAndCustomsChargeFactoryTest
	{
		public override void TestFactoryType()
		{
			AssertEquals(typeof(ImportIncoTermAndCustomsChargeFactory), incoTermAndChargeFactory.GetType());
		}

		public override void TestGetAllIncoTerms()
		{
			AssertEquals("Count", 16, incoTermAndChargeFactory.GetAllIncoTerms().Length);
		}

		public override void TestGetAllCharges()
		{
			var expectedCharges = new string[]
			{
				"AB", "AD", "AE", "AF", "AG", "AH", "AI", "AJ", "AK", "ONS" ,"OFT", "AL", "AN", "BA", "BB", "BC", "BD", "BE", "BF", "BG", "1X", "2X"
			};

			AssertContainsExactElementsInAnyOrder(expectedCharges, incoTermAndChargeFactory.GetAllCharges().Select(_ => _.Code));
		}

		public override void TestGetCharge()
		{
			AssertGetCharge(AISChargeCodeList.Codes.AB, new CustomsChargeCode(AISChargeCodeList.Codes.AB, AISChargeCodeList.Descriptions.AB));
			AssertGetCharge(AISChargeCodeList.Codes.AD, new CustomsChargeCode(AISChargeCodeList.Codes.AD, AISChargeCodeList.Descriptions.AD));
			AssertGetCharge(AISChargeCodeList.Codes.AE, new CustomsChargeCode(AISChargeCodeList.Codes.AE, AISChargeCodeList.Descriptions.AE));
			AssertGetCharge(AISChargeCodeList.Codes.AF, new CustomsChargeCode(AISChargeCodeList.Codes.AF, AISChargeCodeList.Descriptions.AF));
			AssertGetCharge(AISChargeCodeList.Codes.AG, new CustomsChargeCode(AISChargeCodeList.Codes.AG, AISChargeCodeList.Descriptions.AG));
			AssertGetCharge(AISChargeCodeList.Codes.AH, new CustomsChargeCode(AISChargeCodeList.Codes.AH, AISChargeCodeList.Descriptions.AH));
			AssertGetCharge(AISChargeCodeList.Codes.AI, new CustomsChargeCode(AISChargeCodeList.Codes.AI, AISChargeCodeList.Descriptions.AI));
			AssertGetCharge(AISChargeCodeList.Codes.AJ, new CustomsChargeCode(AISChargeCodeList.Codes.AJ, AISChargeCodeList.Descriptions.AJ));
			AssertGetCharge(AISChargeCodeList.Codes.AK, new CustomsChargeCode(AISChargeCodeList.Codes.AK, AISChargeCodeList.Descriptions.AK));
			AssertGetCharge(AISChargeCodeList.Codes.AL, new CustomsChargeCode(AISChargeCodeList.Codes.AL, AISChargeCodeList.Descriptions.AL));
			AssertGetCharge(AISChargeCodeList.Codes.AN, new CustomsChargeCode(AISChargeCodeList.Codes.AN, AISChargeCodeList.Descriptions.AN));
			AssertGetCharge(AISChargeCodeList.Codes.BA, new CustomsChargeCode(AISChargeCodeList.Codes.BA, AISChargeCodeList.Descriptions.BA));
			AssertGetCharge(AISChargeCodeList.Codes.BB, new CustomsChargeCode(AISChargeCodeList.Codes.BB, AISChargeCodeList.Descriptions.BB));
			AssertGetCharge(AISChargeCodeList.Codes.BC, new CustomsChargeCode(AISChargeCodeList.Codes.BC, AISChargeCodeList.Descriptions.BC));
			AssertGetCharge(AISChargeCodeList.Codes.BD, new CustomsChargeCode(AISChargeCodeList.Codes.BD, AISChargeCodeList.Descriptions.BD));
			AssertGetCharge(AISChargeCodeList.Codes.BE, new CustomsChargeCode(AISChargeCodeList.Codes.BE, AISChargeCodeList.Descriptions.BE));
			AssertGetCharge(AISChargeCodeList.Codes.BF, new CustomsChargeCode(AISChargeCodeList.Codes.BF, AISChargeCodeList.Descriptions.BF));
			AssertGetCharge(AISChargeCodeList.Codes.BG, new CustomsChargeCode(AISChargeCodeList.Codes.BG, AISChargeCodeList.Descriptions.BG));
			AssertGetCharge(AISChargeCodeList.Codes._1X, new CustomsChargeCode(AISChargeCodeList.Codes._1X, AISChargeCodeList.Descriptions._1X));
			AssertGetCharge(AISChargeCodeList.Codes._2X, new CustomsChargeCode(AISChargeCodeList.Codes._2X, AISChargeCodeList.Descriptions._2X));
		}

		protected override string GetCountryContext() => Core.Constants.CountryCodes.Ireland + IEJobMessageTypeList.Codes.Import;

		protected override string FreightToEUBorderCode => AISChargeCodeList.Codes.AK;

		protected override string FreightAfterEUBorderCode => AISChargeCodeList.Codes._1X;

		protected override string IncoTermAndCustomsChargeConfigurationFilename =>
			throw new InvalidOperationException("Using overridden TestIncoTermAndCustomsChargeConfiguration which reads texts from EmbeddedResource");

		#region IncoTermAndCustomsChargeConfiguration

		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestIncoTermAndCustomsChargeConfiguration()
		{
			var resultData = new ZStringBuilder(DataHeading);
			var incoTermFactory = (ImportIncoTermAndCustomsChargeFactory)IncoTermAndCustomsChargeFactory.GetByCountryCode(GetCountryContext());
			var allCharges = incoTermFactory.GetAllCharges();
			foreach (var incoTerm in incoTermFactory.GetAllIncoTerms())
			{
				foreach (var charge in allCharges)
				{
					var chargeCode = charge.Code;
					resultData.Append(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16}",
						incoTerm, chargeCode,
						charge.IsDutiable,
						charge.IsDutiableDeemedForThisCharge,
						charge.IsStatisticalValueApplicable,
						charge.IsStatisticalValueApplicableDeemed,
						charge.IsVATible,
						charge.IsVATibleDeemedForThisCharge,
						charge.IsIncludedInITOTDeemedForThisCharge,
						charge.IsIncludedInITOTIfDeemed,
						charge.IsIncoTermNeutral,
						charge.IsPercentageApplicable,
						incoTermFactory.GetDefaultIsIncludedInInvoice(incoTerm, charge),
						incoTermFactory.IsIncludedInInvoiceAmountFixed(incoTerm, charge),
						incoTermFactory.IsThisChargeRecommendedForThisIncoTerm(incoTerm, chargeCode),
						incoTermFactory.IsThisChargeMandatory(incoTerm, chargeCode),
						incoTermFactory.CanThisIncoTermHaveThisChargeForValidation(incoTerm, charge)
						));
				}
			}
			var expectedConfigText = GetExpectedConfigurationText();
			var actualConfigText = GetActualConfigText();

			AssertMultilineASCIIEquals("", expectedConfigText, actualConfigText);
		}
		const string DataHeading = "IncoTerm,CustomsCharge,IsDutiable,IsDutiableDeemedForThisCharge,IsStatisticalValueApplicable,IsStatisticalValueApplicableDeemed,IsVATible,IsVATibleDeemedForThisCharge,IsIncludedInITOTDeemedForThisCharge,IsIncludedInITOTIfDeemed,IsIncoTermNeutral,IsPercentageApplicable,IsIncludedInInvoice,IsIncludedInInvoiceAmount,IsThisChargeRecommendedForThisIncoTerm,IsThisChargeMandatory,CanThisIncoTermHaveThisChargeForValidation";

		string GetActualConfigText()
		{
			var resultData = new ZStringBuilder(DataHeading);
			var incoTermFactory = (ImportIncoTermAndCustomsChargeFactory)IncoTermAndCustomsChargeFactory.GetByCountryCode(GetCountryContext());
			var allCharges = incoTermFactory.GetAllCharges();
			foreach (var incoTerm in incoTermFactory.GetAllIncoTerms())
			{
				foreach (var charge in allCharges)
				{
					var chargeCode = charge.Code;
					resultData.Append(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16}",
						incoTerm, chargeCode,
						charge.IsDutiable,
						charge.IsDutiableDeemedForThisCharge,
						charge.IsStatisticalValueApplicable,
						charge.IsStatisticalValueApplicableDeemed,
						charge.IsVATible,
						charge.IsVATibleDeemedForThisCharge,
						charge.IsIncludedInITOTDeemedForThisCharge,
						charge.IsIncludedInITOTIfDeemed,
						charge.IsIncoTermNeutral,
						charge.IsPercentageApplicable,
						incoTermFactory.GetDefaultIsIncludedInInvoice(incoTerm, charge),
						incoTermFactory.IsIncludedInInvoiceAmountFixed(incoTerm, charge),
						incoTermFactory.IsThisChargeRecommendedForThisIncoTerm(incoTerm, chargeCode),
						incoTermFactory.IsThisChargeMandatory(incoTerm, chargeCode),
						incoTermFactory.CanThisIncoTermHaveThisChargeForValidation(incoTerm, charge)
						));
				}
			}
			return resultData.ToStringWithNewLineBetweenAppends();
		}

		string GetExpectedConfigurationText()
		{
			var executingAssembly = Assembly.GetExecutingAssembly();
			var path = executingAssembly.GetManifestResourceNames().First(name => name.EndsWith(ExpectedConfigurationTextFileName));
			using (var stream = executingAssembly.GetManifestResourceStream(path))
			{
				using (var reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
		}
		protected virtual string ExpectedConfigurationTextFileName => "ImportIncoTermAndCustomsChargeFactory.csv";

		#endregion

		public override void TestSetupToEUBorderCharge()
		{
			var chargeToCompare = Factory.New<JobComInvoiceHeader>().Charges.AddNew(AISChargeCodeList.Codes.AK);

			var chargeToTest = Factory.New<InvoiceCharge>();
			((ImportIncoTermAndCustomsChargeFactory)incoTermAndChargeFactory).SetupToEUBorderCharge(chargeToTest, 1153, "EUR");

			CombineAssertions("SetupToEUBorderCharge", () =>
			{
				AssertEquals("J7_ChargeType, should have been set as expected.", AISChargeCodeList.Codes.AK, chargeToTest.J7_ChargeType);
				AssertEquals("J7_Amount, should have been set as expected.", 1153m, chargeToTest.J7_Amount);
				AssertEquals("J7_RX_NKCurrency, should have been set as expected.", "EUR", chargeToTest.J7_RX_NKCurrency);

				AssertEquals("J7_IsDutiable, should be same as before.", chargeToCompare.J7_IsDutiable, chargeToTest.J7_IsDutiable);
				AssertEquals("J7_IsGSTApplicable, should be same as before.", chargeToCompare.J7_IsGSTApplicable, chargeToTest.J7_IsGSTApplicable);
				AssertEquals("J7_IsStatisticalValueApplicable, should be same as before.", chargeToCompare.J7_IsStatisticalValueApplicable, chargeToTest.J7_IsStatisticalValueApplicable);
			});
		}

		public override void TestSetupAfterEUBorderCharge()
		{
			var chargeToCompare = Factory.New<JobComInvoiceHeader>().Charges.AddNew(AISChargeCodeList.Codes._1X);

			var chargeToTest = Factory.New<InvoiceCharge>();
			((ImportIncoTermAndCustomsChargeFactory)incoTermAndChargeFactory).SetupAfterEUBorderCharge(chargeToTest, 1153, "EUR");

			CombineAssertions("SetupAfterEUBorderCharge", () =>
			{
				AssertEquals("J7_ChargeType, should have been set as expected.", AISChargeCodeList.Codes._1X, chargeToTest.J7_ChargeType);
				AssertEquals("J7_Amount, should have been set as expected.", 1153m, chargeToTest.J7_Amount);
				AssertEquals("J7_RX_NKCurrency, should have been set as expected.", "EUR", chargeToTest.J7_RX_NKCurrency);

				AssertEquals("J7_IsDutiable, should be same as before.", chargeToCompare.J7_IsDutiable, chargeToTest.J7_IsDutiable);
				AssertEquals("J7_IsGSTApplicable, should be same as before.", chargeToCompare.J7_IsGSTApplicable, chargeToTest.J7_IsGSTApplicable);
				AssertEquals("J7_IsStatisticalValueApplicable, should be same as before.", chargeToCompare.J7_IsStatisticalValueApplicable, chargeToTest.J7_IsStatisticalValueApplicable);
			});
		}
	}
}
