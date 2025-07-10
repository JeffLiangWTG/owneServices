using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CMRDeclarationChargeCalculatorTest : TestCaseWithFactory
	{
		public void TestCalculateForNature30()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("DNW", 8.50M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(2009, 1, 2), new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("DNW", 7.50M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(1970, 1, 1), new ZDateTime(2009, 1, 1));
			Factory.Save();

			var lodgeDate = new ZDate(2009, 1, 1);
			AssertN30CustomsCharge(lodgeDate, Deminimus, 7.5m);
			AssertN30CustomsCharge(lodgeDate, 9999m, 7.5m);
			AssertN30CustomsCharge(lodgeDate, 10000m, 7.5m);
			lodgeDate = new ZDate(2009, 1, 2);
			AssertN30CustomsCharge(lodgeDate, Deminimus, 8.5m);
			AssertN30CustomsCharge(lodgeDate, 9999m, 8.5m);
			AssertN30CustomsCharge(lodgeDate, 10000m, 8.5m);
			lodgeDate = new ZDate(2009, 1, 3);
			AssertN30CustomsCharge(lodgeDate, Deminimus, 8.5m);
			AssertN30CustomsCharge(lodgeDate, 9999m, 8.5m);
			AssertN30CustomsCharge(lodgeDate, 10000m, 8.5m);
		}

		void AssertN30CustomsCharge(ZDate lodgeDate, ZDecimal customsValue, ZDecimal expectedResult)
		{
			var dummyEntry = new DummyDeclarationChargeProvider();
			dummyEntry.EffectiveDutyDateExposed = lodgeDate;
			dummyEntry.N10CustomsValueExposed = 0m;
			dummyEntry.N20CustomsValueExposed = 0m;
			dummyEntry.N30CustomsValueExposed = customsValue;

			var testCalculator = new CMRDeclarationChargeCalculatorExposed(dummyEntry);
			AssertEquals("Declaration charge", expectedResult, testCalculator.DeclarationProcessingCharge);
		}

		ZDecimal Deminimus
		{
			get
			{
				return UniversalReferenceHelper.GetDeminimus(new BusinessObjectFactory());
			}
		}

		public void TestDeclarationProcessingChargeForOtherAndMail()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tax1 = helper.CreateTaxOrFee("DPN", 7.50M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FBT", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			tax1.ZZF_Threshold = 10000m;
			var tax2 = helper.CreateTaxOrFee("DPH", 30M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FHT", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			tax2.ZZF_Threshold = 10000m;
			Factory.Save();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 2000m;

			CMRDeclarationChargeCalculator testCalculator = new CMRDeclarationChargeCalculatorExposed(entryHeader);
			AssertEquals("DeclarationProcessingCharge for Mail", 7.50m, testCalculator.DeclarationProcessingCharge);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Other;
			testCalculator = new CMRDeclarationChargeCalculatorExposed(entryHeader);
			AssertEquals("DeclarationProcessingCharge for Other", ZDecimal.Zero, testCalculator.DeclarationProcessingCharge);

			entryLine.CL_CustomsValue = 10000m;
			entryHeader.ResetTotalsAndCachedValues();

			testCalculator = new CMRDeclarationChargeCalculatorExposed(entryHeader);
			AssertEquals("DeclarationProcessingCharge for Other", ZDecimal.Zero, testCalculator.DeclarationProcessingCharge);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
			testCalculator = new CMRDeclarationChargeCalculatorExposed(entryHeader);
			AssertEquals("DeclarationProcessingCharge for Other", 30m, testCalculator.DeclarationProcessingCharge);

			entryLine.CL_CustomsValue = Deminimus;
			entryHeader.ResetTotalsAndCachedValues();

			testCalculator = new CMRDeclarationChargeCalculatorExposed(entryHeader);
			AssertEquals("DeclarationProcessingCharge for Other", 0m, testCalculator.DeclarationProcessingCharge);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
			testCalculator = new CMRDeclarationChargeCalculatorExposed(entryHeader);
			AssertEquals("DeclarationProcessingCharge for Other", 0m, testCalculator.DeclarationProcessingCharge);
		}

		public void TestCalculateForNature10()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tax1 = helper.CreateTaxOrFee("DSN", 17.50M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FBT", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			tax1.ZZF_Threshold = 10000m;
			var tax2 = helper.CreateTaxOrFee("DSH", 40M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FHT", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			tax2.ZZF_Threshold = 10000m;
			var tax3 = helper.CreateTaxOrFee("DAN", 10.50M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FBT", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			tax3.ZZF_Threshold = 10000m;
			var tax4 = helper.CreateTaxOrFee("DAH", 60M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FHT", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			tax4.ZZF_Threshold = 10000m;
			Factory.Save();

			var dummyEntry = new DummyDeclarationChargeProvider();
			dummyEntry.EffectiveDutyDateExposed = new ZDate(2009, 1, 2);
			dummyEntry.transportModeExposed = TransportModeEnum.Sea;
			dummyEntry.N10CustomsValueExposed = Deminimus + 1;
			dummyEntry.N20CustomsValueExposed = 0m;
			dummyEntry.N30CustomsValueExposed = 0m;
			var testCalculator = new CMRDeclarationChargeCalculatorExposed(dummyEntry);
			AssertEquals("Declaration charge for Sea & Non-Nature30", 17.50m, testCalculator.DeclarationProcessingCharge);

			dummyEntry.transportModeExposed = TransportModeEnum.Air;
			testCalculator = new CMRDeclarationChargeCalculatorExposed(dummyEntry);
			AssertEquals("Declaration charge for Air & Non-Nature30", 10.50m, testCalculator.DeclarationProcessingCharge);

			dummyEntry.IsS162ATemporaryImportExposed = true;
			dummyEntry.transportModeExposed = TransportModeEnum.Sea;
			testCalculator = new CMRDeclarationChargeCalculatorExposed(dummyEntry);
			AssertEquals("Declaration charge for Air & Non-Nature30 when S162A applies", 0m, testCalculator.DeclarationProcessingCharge);

			dummyEntry.transportModeExposed = TransportModeEnum.Air;
			testCalculator = new CMRDeclarationChargeCalculatorExposed(dummyEntry);
			AssertEquals("Declaration charge for Air & Non-Nature30 when S162A applies", 0m, testCalculator.DeclarationProcessingCharge);

			dummyEntry.N10CustomsValueExposed = 20000m;
			dummyEntry.IsS162ATemporaryImportExposed = false;

			dummyEntry.transportModeExposed = TransportModeEnum.Sea;
			testCalculator = new CMRDeclarationChargeCalculatorExposed(dummyEntry);
			AssertEquals("Declaration charge for Sea & Non-Nature30 High Value", 40m, testCalculator.DeclarationProcessingCharge);

			dummyEntry.transportModeExposed = TransportModeEnum.Air;
			testCalculator = new CMRDeclarationChargeCalculatorExposed(dummyEntry);
			AssertEquals("Declaration charge for Air & Non-Nature30 High Value", 60m, testCalculator.DeclarationProcessingCharge);

			dummyEntry.IsS162ATemporaryImportExposed = true;
			dummyEntry.transportModeExposed = TransportModeEnum.Sea;
			testCalculator = new CMRDeclarationChargeCalculatorExposed(dummyEntry);
			AssertEquals("Declaration charge for Air & Non-Nature30 when S162A applies High Value", 0m, testCalculator.DeclarationProcessingCharge);

			dummyEntry.transportModeExposed = TransportModeEnum.Air;
			testCalculator = new CMRDeclarationChargeCalculatorExposed(dummyEntry);
			AssertEquals("Declaration charge for Air & Non-Nature30 when S162A applies High Value", 0m, testCalculator.DeclarationProcessingCharge);

			dummyEntry.IsS162ATemporaryImportExposed = false;
			dummyEntry.IsSOFADeclarationExposed = true;
			testCalculator = new CMRDeclarationChargeCalculatorExposed(dummyEntry);
			AssertEquals("Declaration processing charge should not apply for Nature 10 when SOFA applies", 0m, testCalculator.DeclarationProcessingCharge);
		}

		public void TestCalculateForNature1020()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tax1 = helper.CreateTaxOrFee("DSN", 17.50M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FBT", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			tax1.ZZF_Threshold = 10000m;
			var tax2 = helper.CreateTaxOrFee("DSH", 40M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FHT", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			tax2.ZZF_Threshold = 10000m;
			var tax3 = helper.CreateTaxOrFee("DAN", 10.50M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FBT", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			tax3.ZZF_Threshold = 10000m;
			var tax4 = helper.CreateTaxOrFee("DAH", 60M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FHT", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			tax4.ZZF_Threshold = 10000m;
			Factory.Save();

			var dummyEntry = new DummyDeclarationChargeProvider();
			dummyEntry.N10CustomsValueExposed = Deminimus + 1;
			dummyEntry.N20CustomsValueExposed = Deminimus + 1;
			dummyEntry.N30CustomsValueExposed = 0m;
			dummyEntry.EffectiveDutyDateExposed = new ZDate(2009, 1, 2);
			dummyEntry.transportModeExposed = TransportModeEnum.Sea;
			var testCalculator = new CMRDeclarationChargeCalculatorExposed(dummyEntry);
			AssertEquals("Declaration charge for Sea & Non-Nature30", 35m, testCalculator.DeclarationProcessingCharge);

			dummyEntry.transportModeExposed = TransportModeEnum.Air;
			testCalculator = new CMRDeclarationChargeCalculatorExposed(dummyEntry);
			AssertEquals("Declaration charge for Air & Non-Nature30", 21m, testCalculator.DeclarationProcessingCharge);

			dummyEntry.N10CustomsValueExposed = 20000;
			dummyEntry.N20CustomsValueExposed = 20000;

			dummyEntry.transportModeExposed = TransportModeEnum.Sea;
			testCalculator = new CMRDeclarationChargeCalculatorExposed(dummyEntry);
			AssertEquals("Declaration charge for Sea & Non-Nature30", 80m, testCalculator.DeclarationProcessingCharge);

			dummyEntry.transportModeExposed = TransportModeEnum.Air;
			testCalculator = new CMRDeclarationChargeCalculatorExposed(dummyEntry);
			AssertEquals("Declaration charge for Air & Non-Nature30", 120m, testCalculator.DeclarationProcessingCharge);
		}

		public void TestCalculateForLowValue()
		{
			var dummyEntry = new DummyDeclarationChargeProvider();
			dummyEntry.EffectiveDutyDateExposed = new ZDate(2009, 1, 2);
			dummyEntry.transportModeExposed = TransportModeEnum.Sea;
			dummyEntry.N10CustomsValueExposed = Deminimus;
			dummyEntry.N20CustomsValueExposed = Deminimus;
			dummyEntry.N30CustomsValueExposed = 0m;
			CMRDeclarationChargeCalculator testCalculator = new CMRDeclarationChargeCalculatorExposed(dummyEntry);
			AssertEquals("Low value is free of declaration charge", 0m, testCalculator.DeclarationProcessingCharge);
		}

		public void TestCalculateAQISProcessingCharges()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("Q1A", 10, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("Q2A", 15, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("Q1S", 50, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("Q2S", 55, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var dummyEntry = new DummyDeclarationChargeProvider();
			dummyEntry.N10CustomsValueExposed = Deminimus + 1;
			dummyEntry.N20CustomsValueExposed = Deminimus;
			dummyEntry.N30CustomsValueExposed = 0m;
			dummyEntry.EffectiveDutyDateExposed = new ZDate(2009, 1, 2);

			dummyEntry.transportModeExposed = TransportModeEnum.Air;
			AssertEquals("AQIS Processing Charge for air N10", 10m, new CMRDeclarationChargeCalculatorExposed(dummyEntry).AQISProcessingCharge);

			dummyEntry.N10CustomsValueExposed = Deminimus;
			dummyEntry.N20CustomsValueExposed = Deminimus + 1;
			AssertEquals("AQIS Processing Charge for air N20", 15m, new CMRDeclarationChargeCalculatorExposed(dummyEntry).AQISProcessingCharge);

			dummyEntry.N10CustomsValueExposed = Deminimus + 1;
			dummyEntry.N20CustomsValueExposed = Deminimus + 1;
			AssertEquals("AQIS Processing Charge for air N1020", 25m, new CMRDeclarationChargeCalculatorExposed(dummyEntry).AQISProcessingCharge);

			dummyEntry.N10CustomsValueExposed = Deminimus;
			dummyEntry.N20CustomsValueExposed = Deminimus;
			AssertEquals("AQIS Processing Charge for air LowValue", 0m, new CMRDeclarationChargeCalculatorExposed(dummyEntry).AQISProcessingCharge);

			dummyEntry.transportModeExposed = TransportModeEnum.Sea;
			dummyEntry.N10CustomsValueExposed = Deminimus + 1;
			dummyEntry.N20CustomsValueExposed = Deminimus;
			AssertEquals("AQIS Processing Charge for sea N10", 50m, new CMRDeclarationChargeCalculatorExposed(dummyEntry).AQISProcessingCharge);

			dummyEntry.N10CustomsValueExposed = Deminimus;
			dummyEntry.N20CustomsValueExposed = Deminimus + 1;
			AssertEquals("AQIS Processing Charge for sea N20", 55m, new CMRDeclarationChargeCalculatorExposed(dummyEntry).AQISProcessingCharge);

			dummyEntry.N10CustomsValueExposed = Deminimus + 1;
			dummyEntry.N20CustomsValueExposed = Deminimus + 1;
			AssertEquals("AQIS Processing Charge for sea N1020", 105m, new CMRDeclarationChargeCalculatorExposed(dummyEntry).AQISProcessingCharge);

			dummyEntry.N10CustomsValueExposed = 0m;
			dummyEntry.N20CustomsValueExposed = 0m;
			dummyEntry.N30CustomsValueExposed = Deminimus + 1;
			AssertEquals("AQIS Processing Charge for N30", 0m, new CMRDeclarationChargeCalculatorExposed(dummyEntry).AQISProcessingCharge);

			dummyEntry.N10CustomsValueExposed = Deminimus + 1;
			dummyEntry.N20CustomsValueExposed = 0m;
			dummyEntry.N30CustomsValueExposed = 0m;
			dummyEntry.transportModeExposed = TransportModeEnum.Other;
			AssertEquals("AQIS Processing Charge for other", 0m, new CMRDeclarationChargeCalculatorExposed(dummyEntry).AQISProcessingCharge);
		}

		public void TestCalculateAQISContainerCharges()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("Q1F", 100, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("Q2F", 102, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("Q1X", 50, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("Q2X", 52, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("Q1L", 10, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("Q2L", 12, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var dummyEntry = new DummyDeclarationChargeProvider();
			dummyEntry.NumberOfFCLContainersExposed = 1;
			dummyEntry.NumberOfFCXContainersExposed = 3;
			dummyEntry.NumberOfLCLContainersExposed = 7;
			dummyEntry.transportModeExposed = TransportModeEnum.Sea;
			dummyEntry.EffectiveDutyDateExposed = new ZDate(2009, 1, 2);

			dummyEntry.N10CustomsValueExposed = 0m;
			dummyEntry.N20CustomsValueExposed = 0m;
			dummyEntry.N30CustomsValueExposed = Deminimus + 1;
			AssertEquals("Calculated AQIS container charges for sea N30", 0m, new CMRDeclarationChargeCalculatorExposed(dummyEntry).AQISContainerCharge);

			dummyEntry.N10CustomsValueExposed = Deminimus + 1;
			dummyEntry.N20CustomsValueExposed = 0m;
			dummyEntry.N30CustomsValueExposed = 0m;
			AssertEquals("Calculated AQIS container charges for sea N10", 320m, new CMRDeclarationChargeCalculatorExposed(dummyEntry).AQISContainerCharge);

			dummyEntry.N10CustomsValueExposed = 0m;
			dummyEntry.N20CustomsValueExposed = Deminimus + 1;
			AssertEquals("Calculated AQIS container charges for sea N20", 342m, new CMRDeclarationChargeCalculatorExposed(dummyEntry).AQISContainerCharge);

			dummyEntry.N10CustomsValueExposed = Deminimus + 1;
			dummyEntry.N20CustomsValueExposed = Deminimus + 1;
			AssertEquals("Calculated AQIS container charges for sea N1020", 662m, new CMRDeclarationChargeCalculatorExposed(dummyEntry).AQISContainerCharge);

			dummyEntry.N10CustomsValueExposed = Deminimus;
			dummyEntry.N20CustomsValueExposed = 0m;
			dummyEntry.N30CustomsValueExposed = 0m;
			AssertEquals("Calculated AQIS container charges for sea LowValue", 0m, new CMRDeclarationChargeCalculatorExposed(dummyEntry).AQISContainerCharge);

			dummyEntry.N10CustomsValueExposed = Deminimus + 1;
			AssertEquals("Calculated AQIS container charges for sea N10 again", 320m, new CMRDeclarationChargeCalculatorExposed(dummyEntry).AQISContainerCharge);

			dummyEntry.transportModeExposed = TransportModeEnum.Air;
			AssertEquals("Calculated AQIS container charges for air", 0m, new CMRDeclarationChargeCalculatorExposed(dummyEntry).AQISContainerCharge);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TaxOrFeeTestHelper.SetUp();
		}

		class CMRDeclarationChargeCalculatorExposed : CMRDeclarationChargeCalculator
		{
			public CMRDeclarationChargeCalculatorExposed(IDeclarationChargeProvider entry) : base(entry)
			{
			}

			protected override RefCusTaxOrFee.Loader RefCusTaxOrFeeLoader
			{
				get
				{
					var test = new CMRDeclarationChargeCalculatorTest();
					return new RefCusTaxOrFee.Loader(test.Factory);
				}
			}
		}

		class DummyDeclarationChargeProvider : IDeclarationChargeProvider
		{
			public bool IsS162ATemporaryImportExposed;
			public bool IsS162ATemporaryImport
			{
				get { return IsS162ATemporaryImportExposed; }
			}

			public bool IsSOFADeclarationExposed;
			public bool IsSOFADeclaration
			{
				get { return IsSOFADeclarationExposed; }
			}

			public TransportModeEnum transportModeExposed;
			public TransportModeEnum TransportMode
			{
				get { return transportModeExposed; }
			}

			public int NumberOfFCLContainersExposed;
			public int NumberOfFCLContainers
			{
				get { return NumberOfFCLContainersExposed; }
			}

			public int NumberOfFCXContainersExposed;
			public int NumberOfFCXContainers
			{
				get { return NumberOfFCXContainersExposed; }
			}

			public int NumberOfLCLContainersExposed;
			public int NumberOfLCLContainers
			{
				get { return NumberOfLCLContainersExposed; }
			}

			public ZDate EffectiveDutyDateExposed;
			public ZDate EffectiveDutyDate
			{
				get { return EffectiveDutyDateExposed; }
			}

			public ZDecimal N10CustomsValueExposed;
			public ZDecimal N10CustomsValue
			{
				get { return N10CustomsValueExposed; }
			}

			public ZDecimal N20CustomsValueExposed;
			public ZDecimal N20CustomsValue
			{
				get { return N20CustomsValueExposed; }
			}

			public ZDecimal N30CustomsValueExposed;
			public ZDecimal N30CustomsValue
			{
				get { return N30CustomsValueExposed; }
			}

			public bool IsExemptedFromCustomsAndQuarantineFeesExposed;
			public bool IsExemptedFromCustomsAndQuarantineFees
			{
				get { return IsExemptedFromCustomsAndQuarantineFeesExposed; }
			}

			#region IHeaderFeeData Members

			public List<Common.ILineDutyData> DutyDataLines;
			IEnumerable<Common.ILineDutyData> Common.IHeaderFeeData.Lines
			{
				get { return DutyDataLines ?? (DutyDataLines = new List<Common.ILineDutyData>()); }
			}

			void Common.IHeaderFeeData.SetFeeResult(ZString feeType, ZDecimal feeAmount)
			{
			}

			#endregion
		}
	}
}
