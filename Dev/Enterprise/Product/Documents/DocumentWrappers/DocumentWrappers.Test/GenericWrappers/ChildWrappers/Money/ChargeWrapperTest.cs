using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ChargeWrapper))]
	sealed class ChargeWrapperTest : GenericWrapperTest
	{
		#region Carrier's TransitTime, Frequency and Frequency Unit

		public void TestCarrierTransitTimeFrequencyAndFrequencyUnit_Empty()
		{
			var oneOffQuote = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			oneOffQuote.OH_Carrier = TestObjectCreator.Creditor1.PK;

			var possibleCarrier1 = oneOffQuote.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew();
			possibleCarrier1.TTC_OH_Carrier = TestObjectCreator.Creditor2.PK;

			Factory.Save();

			oneOffQuote.TryLoadOrCreateJob();
			using (var job = (Job)oneOffQuote.Job)
			{
				var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, sellCurrency: TestObjectCreator.USD, osSellAmt: 10m, creditor: TestObjectCreator.Creditor1);
				AssertChargeWrapper(charge1, expectedCarrierTransitTime: "", expectedCarrierFrequency: "0", expectedCarrierFrequencyUnit: "", assertionMessage: "Carrier");

				var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, sellCurrency: TestObjectCreator.USD, osSellAmt: 10m, creditor: TestObjectCreator.Creditor2);
				AssertChargeWrapper(charge2, expectedCarrierTransitTime: "", expectedCarrierFrequency: "0", expectedCarrierFrequencyUnit: "", assertionMessage: "PossibleCarrier");
			}
		}

		void AssertChargeWrapper(Charge charge, string expectedCarrierTransitTime, string expectedCarrierFrequency, string expectedCarrierFrequencyUnit, string assertionMessage = default)
		{
			var chargeWrapper = new ChargeWrapper(charge, Factory);
			CombineAssertions(assertionMessage, () =>
			{
				AssertEquals("TransitTime", expectedCarrierTransitTime, chargeWrapper.CarrierTransitTime);
				AssertEquals("Frequency", expectedCarrierFrequency, chargeWrapper.CarrierFrequency);
				AssertEquals("FrequencyUnit", expectedCarrierFrequencyUnit, chargeWrapper.CarrierFrequencyUnit);
			});
		}

		public void TestCarrierTransitTimeFrequencyAndFrequencyUnit()
		{
			var oneOffQuote = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			oneOffQuote.OH_Carrier = TestObjectCreator.Creditor1.PK;
			oneOffQuote.TransitTime = "10";
			oneOffQuote.Frequency = 1;
			oneOffQuote.FrequencyUnit = "Days";

			var possibleCarrier1 = oneOffQuote.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew();
			possibleCarrier1.TTC_OH_Carrier = TestObjectCreator.Creditor2.PK;
			possibleCarrier1.TTC_TransitTime = "20";
			possibleCarrier1.TTC_Frequency = 2;
			possibleCarrier1.TTC_FrequencyUnit = "Daily";

			var possibleCarrier2 = oneOffQuote.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew();
			possibleCarrier2.TTC_OH_Carrier = TestObjectCreator.Creditor3.PK;
			possibleCarrier2.TTC_TransitTime = "30";
			possibleCarrier2.TTC_Frequency = 3;
			possibleCarrier2.TTC_FrequencyUnit = "Forthnight";

			Factory.Save();

			oneOffQuote.TryLoadOrCreateJob();
			using (var job = (Job)oneOffQuote.Job)
			{
				var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, sellCurrency: TestObjectCreator.USD, osSellAmt: 10m, creditor: TestObjectCreator.Creditor1);
				AssertChargeWrapper(charge1, expectedCarrierTransitTime: "10", expectedCarrierFrequency: "1", expectedCarrierFrequencyUnit: "Days", assertionMessage: "Carrier");

				var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, sellCurrency: TestObjectCreator.USD, osSellAmt: 10m, creditor: TestObjectCreator.Creditor2);
				AssertChargeWrapper(charge2, expectedCarrierTransitTime: "20", expectedCarrierFrequency: "2", expectedCarrierFrequencyUnit: "Daily", assertionMessage: "PossibleCarrier1");

				var charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, sellCurrency: TestObjectCreator.USD, osSellAmt: 10m, creditor: TestObjectCreator.Creditor3);
				AssertChargeWrapper(charge3, expectedCarrierTransitTime: "30", expectedCarrierFrequency: "3", expectedCarrierFrequencyUnit: "Forthnight", assertionMessage: "PossibleCarrier2");
			}
		}

		#endregion

		#region Description

		public void TestDescription()
		{
			var sundry = Factory.New<SundryCharges>();
			var job = new Job.Loader(sundry).TryLoadOrCreate();
			var charge = AddCharge(job, "FRT");

			var wrapper = new ChargeWrapper(charge, Factory);
			AssertEquals("wrapper.Description", "International Freight", wrapper.Description);

			charge.JR_Desc = "Another description";
			AssertEquals("wrapper.Description", "Another description", wrapper.Description);

			using (DocumentsDataRegistry.Instance.ShowCalculationDescriptionOnOneOffQuotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ShowCalculationDescriptionOnOneOffQuotesCode.Yes))
			{
				AssertEquals("GIVEN registry is Yes THEN wrapper.Description should be JR_Desc", "Another description", wrapper.Description);
			}

			using (DocumentsDataRegistry.Instance.ShowCalculationDescriptionOnOneOffQuotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ShowCalculationDescriptionOnOneOffQuotesCode.No))
			{
				AssertEquals("GIVEN registry is No THEN wrapper.Description should be JR_Desc", "Another description", wrapper.Description);
			}

			AssertEquals("Precondition", "International Freight", charge.ChargeCode.AC_Desc);
			using (DocumentsDataRegistry.Instance.ShowCalculationDescriptionOnOneOffQuotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ShowCalculationDescriptionOnOneOffQuotesCode.Charge))
			{
				AssertEquals("GIVEN registry is No THEN wrapper.Description should be AC_Desc", "International Freight", wrapper.Description);
			}
		}

		#region Multilingual

		public void TestDescription_Multilingual_EnableLocalChargeCodeDescriptionDefault()
		{
			AssertDescription_Multilingual
			(
				enableLocalChargeCodeDescriptionDefault: true,
				expectedDescription: "Charge Local Description 1"
			);
		}

		public void TestDescription_Multilingual_DisableLocalChargeCodeDescriptionDefault()
		{
			AssertDescription_Multilingual
			(
				enableLocalChargeCodeDescriptionDefault: false,
				expectedDescription: "Mein Testgebührencode"
			);
		}

		void AssertDescription_Multilingual(bool enableLocalChargeCodeDescriptionDefault, string expectedDescription)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Desc = "Charge Description 1";
			chargeCode.AC_LocalLanguageDescription = "Charge Local Description 1";

			using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, enableLocalChargeCodeDescriptionDefault))
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.German))
			using (var mockRes = Res.UseMockData())
			{
				var charge = CreateCharge(Factory, chargeCode, sellAccount: orgHeader);
				var chargeWrapper = new ChargeWrapper(charge, Factory);
				SetChargeCodeDescriptionMultilingual(chargeCode, mockRes, description: "Charge Description 1", multilingualDescription: "Mein Testgebührencode");
				AssertEquals("wrapper.Description", expectedDescription, chargeWrapper.Description);
			}
		}

		public void TestDescription_Multilingual_EnableLocalChargeCodeDescriptionDefault_ThenDisableLocalChargeCodeDescriptionDefault()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Desc = "Charge Description 1";
			chargeCode.AC_LocalLanguageDescription = "Charge Local Description 1";

			Charge charge = null;

			using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				charge = CreateCharge(Factory, chargeCode, sellAccount: orgHeader);
				AssertEquals("Precondition: charge description", "Charge Local Description 1", charge.JR_Desc);
			}

			using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.German))
			using (var mockRes = Res.UseMockData())
			{
				var chargeWrapper = new ChargeWrapper(charge, Factory);
				SetChargeCodeDescriptionMultilingual(chargeCode, mockRes, description: "Charge Description 1", multilingualDescription: "Mein Testgebührencode");
				AssertEquals
				(
					"WHEN EnableLocalChargeCodeDescriptionDefault then DisableLocalChargeCodeDescriptionDefault THEN should show multilingual description",
					"Mein Testgebührencode",
					chargeWrapper.Description
				);
			}
		}

		public void TestDescription_Multilingual_AddTextToDescription()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Desc = "Charge Description 1";

			using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.German))
			using (var mockRes = Res.UseMockData())
			{
				var charge = CreateCharge(Factory, chargeCode);
				AssertEquals("Precondition: charge description", "Charge Description 1", charge.JR_Desc);

				charge.JR_Desc += " (updated)";

				var chargeWrapper = new ChargeWrapper(charge, Factory);
				SetChargeCodeDescriptionMultilingual(chargeCode, mockRes, description: "Charge Description 1", multilingualDescription: "Mein Testgebührencode");
				AssertEquals
				(
					"WHEN text is added at the end of description, THEN should show multilingual description with the added text",
					"Mein Testgebührencode (updated)",
					chargeWrapper.Description
				);
			}
		}

		static Charge CreateCharge(BusinessObjectFactory factory, AccChargeCode chargeCode, OrgHeader sellAccount = null)
		{
			var sundry = factory.New<SundryCharges>();
			var job = new Job.Loader(sundry).TryLoadOrCreate();

			var charge = AddCharge(job, chargeCode.AC_Code);

			if (sellAccount != null)
			{
				charge.JR_OH_SellAccount = sellAccount.PK;
			}

			return charge;
		}

		static void SetChargeCodeDescriptionMultilingual(AccChargeCode chargeCode, IMockResourceStringCache mockRes, string description, string multilingualDescription)
		{
			var resKey = chargeCode.AC_DescInfo.CustomizableDataResourceStrings.GetMultilingualString(chargeCode, description).ResourceKey;
			mockRes.Put(resKey, new ResourceStringData(resKey, multilingualDescription));
		}

		public void TestDescription_Multilingual_UpdatingDescription()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Desc = "Charge Description 1";

			using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.German))
			using (var mockRes = Res.UseMockData())
			{
				var charge = CreateCharge(Factory, chargeCode, sellAccount: orgHeader);
				AssertEquals("Precondition: charge description", "Charge Description 1", charge.JR_Desc);

				charge.JR_Desc = "User updated description";

				var chargeWrapper = new ChargeWrapper(charge, Factory);
				SetChargeCodeDescriptionMultilingual(chargeCode, mockRes, description: "Charge Description 1", multilingualDescription: "Mein Testgebührencode");
				AssertEquals("wrapper.Description", "User updated description", chargeWrapper.Description);
			}
		}

		public void TestDescription_Multilingual_ShowCalculationDescriptionOnOneOffQuotesIsYes()
		{
			AssertDescription_Multilingual_ShowCalculationDescriptionOnOneOffQuotes
			(
				showCalculationDescriptionOnOneOffQuotes: ShowCalculationDescriptionOnOneOffQuotesCode.Yes,
				expectedDescription: "Mein Testgebührencode"
			);
		}

		public void TestDescription_Multilingual_ShowCalculationDescriptionOnOneOffQuotesIsNo()
		{
			AssertDescription_Multilingual_ShowCalculationDescriptionOnOneOffQuotes
			(
				showCalculationDescriptionOnOneOffQuotes: ShowCalculationDescriptionOnOneOffQuotesCode.No,
				expectedDescription: "Mein Testgebührencode"
			);
		}

		public void TestDescription_Multilingual_ShowCalculationDescriptionOnOneOffQuotesIsCharge()
		{
			AssertDescription_Multilingual_ShowCalculationDescriptionOnOneOffQuotes
			(
				showCalculationDescriptionOnOneOffQuotes: ShowCalculationDescriptionOnOneOffQuotesCode.Charge,
				expectedDescription: "Mein Testgebührencode"
			);
		}

		void AssertDescription_Multilingual_ShowCalculationDescriptionOnOneOffQuotes(string showCalculationDescriptionOnOneOffQuotes, string expectedDescription)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Desc = "Charge Description 1";

			using (DocumentsDataRegistry.Instance.ShowCalculationDescriptionOnOneOffQuotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, showCalculationDescriptionOnOneOffQuotes))
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.German))
			using (var mockRes = Res.UseMockData())
			{
				var charge = CreateCharge(Factory, chargeCode, sellAccount: orgHeader);
				var chargeWrapper = new ChargeWrapper(charge, Factory);
				SetChargeCodeDescriptionMultilingual(chargeCode, mockRes, description: "Charge Description 1", multilingualDescription: "Mein Testgebührencode");
				AssertEquals("wrapper.Description", expectedDescription, chargeWrapper.Description);
			}
		}

		#endregion

		#endregion

		#region Calculation Description

		public void TestCalculationDescription()
		{
			const string calculationDescriptionText = @"ODOC: Base AUD 50.00

Charge located in Company Tariff Level 1 (Base Company Tariff) with the following details: 

Mode:			ALL
Charge Code Group:	ORG
Start Date:		15 December 2010
End Date:		15 June 2011
Origin:			Australia
Cross-Trade Rate:	No
Service Level:		
Commodity Code:	GEN

User:		CargoWise Support
Time:		8/02/2011 8:30:39 AM
";
			SundryCharges sundry = Factory.New<SundryCharges>();
			Job job = new Job.Loader(sundry).TryLoadOrCreate();

			Charge charge = AddCharge(job, "DDOC");
			charge.RevenueCalculationDescription = ORtfTextUtil.TextToRtfBytes(calculationDescriptionText);

			ChargeWrapper wrapper = new ChargeWrapper(charge, Factory);

			using (DocumentsDataRegistry.Instance.ShowCalculationDescriptionOnOneOffQuotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ShowCalculationDescriptionOnOneOffQuotesCode.Yes))
			{
				AssertEquals("GIVEN registry is Yes THEN wrapper.CalculationDescription should be shown", "  Base AUD 50.00", wrapper.CalculationDescription);
			}

			using (DocumentsDataRegistry.Instance.ShowCalculationDescriptionOnOneOffQuotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ShowCalculationDescriptionOnOneOffQuotesCode.No))
			{
				AssertNullOrEmpty("GIVEN registry is No THEN wrapper.CalculationDescription should be empty", wrapper.CalculationDescription);
			}

			using (DocumentsDataRegistry.Instance.ShowCalculationDescriptionOnOneOffQuotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ShowCalculationDescriptionOnOneOffQuotesCode.Charge))
			{
				AssertNullOrEmpty("GIVEN registry is Charge THEN wrapper.CalculationDescription should be empty", wrapper.CalculationDescription);
			}
		}

		public void TestCalculationDescription_QuickCalculator()
		{
			var calculationDescriptionText = @"FRT: 8 CN @
AUD 3000.000/20GP x 8 CN

Sell Amount Entered using Quick Calculator";

			using (DocumentsDataRegistry.Instance.ShowCalculationDescriptionOnOneOffQuotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ShowCalculationDescriptionOnOneOffQuotesCode.Yes))
			{
				var sundry = Factory.New<SundryCharges>();
				var job = new Job.Loader(sundry).TryLoadOrCreate();

				var charge = AddCharge(job, "DDOC");
				charge.RevenueCalculationDescription = ORtfTextUtil.TextToRtfBytes(calculationDescriptionText);

				var wrapper = new ChargeWrapper(charge, Factory);
				AssertEquals(@"  8 CN @ AUD 3000.000/20GP x 8 CN", wrapper.CalculationDescription);
			}
		}

		public void TestCalculationDescription_ParsingEmptyLines()
		{
			var calculationDescriptionText = @"
: Some information";

			using (DocumentsDataRegistry.Instance.ShowCalculationDescriptionOnOneOffQuotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ShowCalculationDescriptionOnOneOffQuotesCode.Yes))
			{
				var sundry = Factory.New<SundryCharges>();
				var job = new Job.Loader(sundry).TryLoadOrCreate();

				var charge = AddCharge(job, "DDOC");
				charge.RevenueCalculationDescription = ORtfTextUtil.TextToRtfBytes(calculationDescriptionText);

				var wrapper = new ChargeWrapper(charge, Factory);
				AssertEquals("  Some information", wrapper.CalculationDescription);
			}
		}

		public void TestCalculationDescription_IgnoreChargeDescriptionRegistryItem()
		{
			SundryCharges sundry = Factory.New<SundryCharges>();
			Job job = new Job.Loader(sundry).TryLoadOrCreate();
			Charge charge = AddCharge(job, "DDOC");

			DocumentsDataRegistry.Instance.ShowCalculationDescriptionOnOneOffQuotes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ShowCalculationDescriptionOnOneOffQuotesCode.Yes);

			charge.JR_Desc = "Description CONTAINS";
			charge.RevenueCalculationDescription = ORtfTextUtil.TextToRtfBytes(": DOESN'T CONTAIN");

			ChargeWrapper wrapper = new ChargeWrapper(charge, Factory);
			AssertEquals("  DOESN'T CONTAIN", wrapper.CalculationDescription);

			charge.RevenueCalculationDescription = ORtfTextUtil.TextToRtfBytes(": CONTAINS");
			wrapper = new ChargeWrapper(charge, Factory);
			AssertEquals(ZString.Empty, wrapper.CalculationDescription);
		}

		#endregion

		public void TestChargeBreakDowns()
		{
			SundryCharges sundry = Factory.New<SundryCharges>();
			Job job = new Job.Loader(sundry).TryLoadOrCreate();

			AccTaxRate rate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			rate.AT_Code = "BOB";
			rate.SetRateNumerator_ForTestOnly(10);

			Charge charge = AddCharge(job, "ODOC");
			charge.JR_AT_CostGSTRate = rate.PK;
			charge.JR_AT_SellGSTRate = rate.PK;
			SetCost(charge, "SGD", 50, 60);
			SetSell(charge, "USD", 70, 80);

			ChargeWrapper wrapper = new ChargeWrapper(charge, Factory);

			CombineAssertions(delegate
			{
				AssertEquals("OS Cost Net", "50.00 SGD", wrapper.OSCost.WithoutTax.ToString());
				AssertEquals("OS Cost Tax", "5.00 SGD", wrapper.OSCost.Tax.ToString());
				AssertEquals("OS Cost Gross", "55.00 SGD", wrapper.OSCost.WithTax.ToString());

				AssertEquals("Local Cost Net", "60.00 ERN", wrapper.LocalCost.WithoutTax.ToString());
				AssertEquals("Local Cost Tax", "6.00 ERN", wrapper.LocalCost.Tax.ToString());
				AssertEquals("Local Cost Gross", "66.00 ERN", wrapper.LocalCost.WithTax.ToString());

				AssertEquals("OS Sell Net", "70.00 USD", wrapper.OSSell.WithoutTax.ToString());
				AssertEquals("OS Sell Tax", "7.00 USD", wrapper.OSSell.Tax.ToString());
				AssertEquals("OS Sell Gross", "77.00 USD", wrapper.OSSell.WithTax.ToString());

				AssertEquals("Local Sell Net", "80.00 ERN", wrapper.LocalSell.WithoutTax.ToString());
				AssertEquals("Local Sell Tax", "8.00 ERN", wrapper.LocalSell.Tax.ToString());
				AssertEquals("Local Sell Gross", "88.00 ERN", wrapper.LocalSell.WithTax.ToString());
			});
		}

		public override void TestWrapperMappingsEmpty()
		{
			SundryCharges sundry = Factory.New<SundryCharges>();
			Job job = new Job.Loader(sundry).TryLoadOrCreate();
			Charge charge = AddCharge(job, "FRT");
			charge.JR_Desc = "Another description";

			ChargeWrapper wrapper = new ChargeWrapper(charge, Factory);

			CombineAssertions(delegate
			{
				AssertEquals("Calculation Description", "", wrapper.CalculationDescription);
				AssertEquals("Charge Code", "FRT - International Freight", wrapper.ChargeCode.ToString());
				AssertEquals("Description", "Another description", wrapper.Description);
				AssertEquals("Local Sell", "", wrapper.LocalSell.ToString());
				AssertEquals("Local Cost", "", wrapper.LocalSell.ToString());
				AssertEquals("OS Sell", "", wrapper.OSSell.ToString());
				AssertEquals("OS Cost", "", wrapper.OSSell.ToString());
			});
		}

		#region Implementation

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
ChargeCode : FRT - International Freight
LocalCost : 408.00 ERN
LocalSell : 400.00 ERN
OSCost : 510.00 SGD
OSSell : 500.00 USD
Registry : (No Default Field Value Available on Registry)";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			SundryCharges sundry = Factory.New<SundryCharges>();
			Job job = new Job.Loader(sundry).TryLoadOrCreate();

			Charge charge = AddCharge(job, "FRT");
			SetSell(charge, "USD", 500m, 400m);
			SetCost(charge, "SGD", 510m, 408m);

			return new ChargeWrapper(charge, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Charge
======================================================================
Name                                    Type
----------------------------------------------------------------------
LocalCost                               Charge Breakdown
LocalSell                               Charge Breakdown
OSCost                                  Charge Breakdown
OSSell                                  Charge Breakdown
ChargeCode                              CodeAndDescription
CalculationDescription                  String
CarrierFrequency                        String
CarrierFrequencyUnit                    String
CarrierTransitTime                      String
Description                             String
SellRate                                String
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			SundryCharges sundry = Factory.New<SundryCharges>();
			Job job = new Job.Loader(sundry).TryLoadOrCreate();

			Charge charge = AddCharge(job, "FRT");
			SetSell(charge, "USD", 500m, 400m);
			SetCost(charge, "SGD", 510m, 408m);

			return new ChargeWrapper(charge, Factory);
		}

		static Charge AddCharge(Job header, string chargeCode)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(AccChargeCodeSchema.AC_Code, chargeCode);
			filter.AddToFilter(AccChargeCodeSchema.AC_GC, header.JH_GC);

			AccChargeCode ac = header.Factory.LoadTop1<AccChargeCode>(filter);

			Charge charge = header.Charges.AddNew();
			charge.JR_AC = ac.PK;

			return charge;
		}

		static void SetSell(Charge charge, string currency, decimal osAmt, decimal localAmt)
		{
			SetExchangeRate((Job)charge.Job, currency, osAmt / localAmt);

			charge.JR_RX_NKSellCurrency = currency;
			charge.JR_OSSellAmt = osAmt;
			charge.JR_LocalSellAmt = localAmt;
		}

		static void SetCost(Charge charge, string currency, decimal osAmt, decimal localAmt)
		{
			SetExchangeRate((Job)charge.Job, currency, osAmt / localAmt);

			charge.JR_RX_NKCostCurrency = currency;
			charge.JR_OSCostAmt = osAmt;
			charge.JR_LocalCostAmt = localAmt;
		}

		static void SetExchangeRate(Job job, string currency, decimal rate)
		{
			foreach (ExchangeRate exrate in job.ExchangeRates)
			{
				if (exrate.JF_RX_NKRateCurrency == currency)
				{
					exrate.JF_BaseRate = rate;
					return;
				}
			}

			{
				ExchangeRate exrate = job.ExchangeRates.AddNew();
				exrate.JF_RX_NKRateCurrency = currency;
				exrate.JF_BaseRate = rate;
			}
		}

		#endregion
	}
}
