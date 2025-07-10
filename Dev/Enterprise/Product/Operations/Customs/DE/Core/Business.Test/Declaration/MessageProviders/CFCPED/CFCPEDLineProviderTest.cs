using System;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.MonthlyClosing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CFCPEDLineProvider))]
	sealed class CFCPEDLineProviderTest : MonthlyClosingDecLineProviderAbstractTest<CFCPEDLineProvider, ICFCRECHeader, ICFCRECLine>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new CFCPEDLineProvider(null, isModificationMessage: false));
		}

		public void TestCessionManagementFlag_Provider()
		{
			isModificationMessage = true;
			Mock.Get(lineProvider).Setup(l => l.CessionManagementFlag).Returns("X");
			AssertEquals("X", Provider.CessionManagementFlag);
		}

		public void TestCessionManagementFlag_Snapshot()
		{
			isModificationMessage = false;
			snapshot.CessionManagementFlag = "X";
			AssertEquals("X", Provider.CessionManagementFlag);
		}

		public void TestCessionManagementFlag_Snapshot_Null()
		{
			isModificationMessage = false;
			AssertNull(Provider.CessionManagementFlag);
		}

		public void TestPreferentialOriginCountry_Provider()
		{
			isModificationMessage = true;
			Mock.Get(lineProvider).Setup(l => l.PreferentialOriginCountry).Returns("XY");
			AssertEquals("XY", Provider.PreferentialOriginCountry);
		}

		public void TestPreferentialOriginCountry_Snapshot()
		{
			isModificationMessage = false;
			snapshot.PreferentialCountry = "XZ";
			AssertEquals("XZ", Provider.PreferentialOriginCountry);
		}

		public void TestPreferentialOriginCountry_Snapshot_Null()
		{
			isModificationMessage = false;
			AssertNull(Provider.PreferentialOriginCountry);
		}

		public void TestTobaccoRevenueStampNumber()
		{
			Mock.Get(lineProvider).Setup(l => l.TobaccoRevenueStampNumber).Returns("TOBACCO");
			AssertEquals("TOBACCO", Provider.TobaccoRevenueStampNumber);
		}

		public void TestAssessmentOutwardProcessingFee()
		{
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			TestHelper.SetExchangeRates(Factory);
			invoiceLine.Charges.AddNew(ImportChargeCodeList.Codes.OPF, 1.23m, Core.Constants.CurrencyCodes.UnitedStates);
			invoiceLine.Charges.AddNew(ImportChargeCodeList.Codes.TCE, 30m, Core.Constants.CurrencyCodes.Sweden);
			invoiceLine.ApportionedCharges.AddNew(ImportChargeCodeList.Codes.TCE, 3m, Core.Constants.CurrencyCodes.EuropeanUnion);
			invoiceLine.ApportionedCharges.AddNew(ImportChargeCodeList.Codes.OPF, 2.45m, Core.Constants.CurrencyCodes.UnitedStates);
			invoiceLine.ApportionedCharges.AddNew(ImportChargeCodeList.Codes.TCE, 40m, Core.Constants.CurrencyCodes.Sweden);
			invoiceLine.ApportionedCharges.AddNew(ImportChargeCodeList.Codes._014, 459m, Core.Constants.CurrencyCodes.Japan);
			invoiceLine2.Charges.AddNew(ImportChargeCodeList.Codes.OPF, 1.00m, Core.Constants.CurrencyCodes.EuropeanUnion);
			invoiceLine2.Charges.AddNew(ImportChargeCodeList.Codes.TCE, 24.81m, Core.Constants.CurrencyCodes.EuropeanUnion);
			invoiceLine2.ApportionedCharges.AddNew(ImportChargeCodeList.Codes.OPF, 0.50m, Core.Constants.CurrencyCodes.EuropeanUnion);
			invoiceLine2.ApportionedCharges.AddNew(ImportChargeCodeList.Codes.TCE, 31.22m, Core.Constants.CurrencyCodes.EuropeanUnion);
			AssertEquals("EUR", 5.59m, Provider.AssessmentOutwardProcessingFee);
		}

		public void TestAssessmentTaxCosts()
		{
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;

			TestHelper.SetExchangeRates(Factory);
			invoiceLine.Charges.AddNew(ImportChargeCodeList.Codes.OPF, 1.23m, Core.Constants.CurrencyCodes.UnitedStates);
			invoiceLine.Charges.AddNew(ImportChargeCodeList.Codes.TCE, 3.46m, Core.Constants.CurrencyCodes.Sweden);
			invoiceLine.ApportionedCharges.AddNew(ImportChargeCodeList.Codes.TCE, 1.65m, Core.Constants.CurrencyCodes.EuropeanUnion);
			invoiceLine.ApportionedCharges.AddNew(ImportChargeCodeList.Codes.OPF, 2.45m, Core.Constants.CurrencyCodes.UnitedStates);
			invoiceLine.ApportionedCharges.AddNew(ImportChargeCodeList.Codes.TCE, 5.87m, Core.Constants.CurrencyCodes.Sweden);
			invoiceLine.ApportionedCharges.AddNew(ImportChargeCodeList.Codes._014, 459m, Core.Constants.CurrencyCodes.Japan);
			invoiceLine2.ApportionedCharges.AddNew(ImportChargeCodeList.Codes.TCE, 1.00m, Core.Constants.CurrencyCodes.EuropeanUnion);
			invoiceLine2.ApportionedCharges.AddNew(ImportChargeCodeList.Codes.OPF, 0.50m, Core.Constants.CurrencyCodes.EuropeanUnion);
			invoiceLine2.ApportionedCharges.AddNew(ImportChargeCodeList.Codes._014, 2.01m, Core.Constants.CurrencyCodes.EuropeanUnion);
			AssertEquals("EUR", 8.66m, Provider.AssessmentTaxCosts);
		}

		public void TestPreferentialTreatment_Provider()
		{
			isModificationMessage = true;
			Mock.Get(lineProvider).Setup(l => l.PreferentialTreatment).Returns(
				Mock.Of<ILinePreferentialTreatment>(m =>
					m.RequestedPreferentialTreatment == "200" &&
					m.Quantity == Mock.Of<IAmount>(a => a.Quantity == 12344.66m && a.MeasurementUnit == "KGM" && a.Qualifier == String.Empty) &&
					m.ContingentNumber == new string[] { "CONT1", "CONT2" }
				)
			);
			CombineAssertions(() =>
			{
				AssertEquals("200", Provider.PreferentialTreatment.RequestedPreferentialTreatment);
				AssertEquals(12344.66m, Provider.PreferentialTreatment.Quantity.Quantity);
				AssertEquals("KGM", Provider.PreferentialTreatment.Quantity.MeasurementUnit);
				AssertEquals(string.Empty, Provider.PreferentialTreatment.Quantity.Qualifier);
				AssertContainsExactElementsInAnyOrder(new ZString[] { "CONT1", "CONT2" }, Provider.PreferentialTreatment.ContingentNumber);
			});
		}

		public void TestPreferentialTreatment_Snapshot()
		{
			isModificationMessage = false;
			snapshot.PreferentialTreatment = new DEMonthlyClosingEntryLineSnapshotPreferentialTreatment
			{
				Declaration = new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclaration
				{
					Contingent = new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationContingent[]
					{
						new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationContingent { ContingentNumber = "CONT1" },
						new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationContingent { ContingentNumber = "CONT2" },
					},
					PreferentialTreatmentQuantity = new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationPreferentialTreatmentQuantity
					{
						MeasurementUnit = "KGM",
						Quantity = 12344.66m,
						Qualifier = string.Empty
					}
				},
				RequestedPreferentialTreatment = "200"
			};
			CombineAssertions(() =>
			{
				AssertEquals("200", Provider.PreferentialTreatment.RequestedPreferentialTreatment);
				AssertEquals(12344.66m, Provider.PreferentialTreatment.Quantity.Quantity);
				AssertEquals("KGM", Provider.PreferentialTreatment.Quantity.MeasurementUnit);
				AssertEquals(string.Empty, Provider.PreferentialTreatment.Quantity.Qualifier);
				AssertContainsExactElementsInAnyOrder(new ZString[] { "CONT1", "CONT2" }, Provider.PreferentialTreatment.ContingentNumber);
			});
		}

		public void TestPreferentialTreatment_Snapshot_Null()
		{
			isModificationMessage = false;
			AssertNull(Provider.PreferentialTreatment);
		}

		public void TestSpecialCase()
		{
			var tax1 = invoiceLine.Taxes.AddNew();
			tax1.JLT_Type = "100";
			tax1.JLT_Rate = 22.99m;
			tax1.JLT_MethodOfCalculation = "A";
			var tax2 = invoiceLine.Taxes.AddNew();
			tax2.JLT_Type = "200";
			tax2.JLT_Rate = 12.34m;
			tax2.JLT_MethodOfCalculation = "B";
			CombineAssertions(() =>
			{
				AssertEquals("100", Provider.SpecialCase.First().Group);
				AssertEquals(22.99m, Provider.SpecialCase.First().RateOrAmountOrFactor);
				AssertEquals("A", Provider.SpecialCase.First().ApplicationType);
				AssertEquals("200", Provider.SpecialCase.Last().Group);
				AssertEquals(12.34m, Provider.SpecialCase.Last().RateOrAmountOrFactor);
				AssertEquals("B", Provider.SpecialCase.Last().ApplicationType);
			});
		}

		public void TestAssessmentCustomsValue_Provider()
		{
			PrepareForAssessmentCustomsValueCondition();
			isModificationMessage = true;
			Mock.Get(lineProvider).Setup(x => x.AssessmentCustomsValue).Returns(28765.43m);
			AssertEquals(28765.43m, Provider.AssessmentCustomsValue);
		}

		public void TestAssessmentCustomsValue_Snapshot()
		{
			PrepareForAssessmentCustomsValueCondition();
			isModificationMessage = false;
			snapshot.Assessment = new DEMonthlyClosingEntryLineSnapshotAssessment
			{
				CustomsValue = 28765.43m
			};
			AssertEquals(28765.43m, Provider.AssessmentCustomsValue);
		}

		public void TestAssessmentCustomsValue_Snapshot_Null()
		{
			PrepareForAssessmentCustomsValueCondition();
			isModificationMessage = false;
			AssertEquals(0m, Provider.AssessmentCustomsValue);
		}

		public void TestAssessmentCustomsValue_IsDV1()
		{
			Mock.Get(lineProvider).Setup(x => x.AssessmentCustomsValue).Returns(28765.43m);
			declaration.ZG_IsHighValueOvrd = true;
			invoiceLine.JI_Procedure = "1000" + CustomsProcedureCodeList.Import.Concession._8E2;
			AssertEquals(0m, Provider.AssessmentCustomsValue);
		}

		public void TestAssessmentCustomsValue_IsE01_E02()
		{
			Mock.Get(lineProvider).Setup(x => x.AssessmentCustomsValue).Returns(28765.43m);
			CombineAssertions(() =>
			{
				declaration.ZG_IsHighValueOvrd = false;
				invoiceLine.JI_Procedure = "1000" + CustomsProcedureCodeList.Import.Concession._E01;
				AssertEquals(0m, Provider.AssessmentCustomsValue);

				invoiceLine.JI_Procedure = "1000" + CustomsProcedureCodeList.Import.Concession._E02;
				AssertEquals(0m, Provider.AssessmentCustomsValue);
			});
		}
		protected override CFCPEDLineProvider GetProvider() => new CFCPEDLineProviderForTest(reconEntryLine, isModificationMessage, headerProvider, lineProvider, snapshot);

		void PrepareForAssessmentCustomsValueCondition()
		{
			declaration.ZG_IsHighValueOvrd = false;
			invoiceLine.JI_Procedure = "1000" + CustomsProcedureCodeList.Import.Concession._8E2;
		}

		new ICFCPEDLine Provider => base.Provider;

		sealed class CFCPEDLineProviderForTest : CFCPEDLineProvider
		{
			public CFCPEDLineProviderForTest(CusReconEntryLine reconEntryLine, bool isModificationMessage, ICFCRECHeader headerProvider, ICFCRECLine lineProvider, DEMonthlyClosingEntryLineSnapshot snapshot) : base(reconEntryLine, isModificationMessage)
			{
				this.snapshot = snapshot;
				this.lineProvider = lineProvider;
				this.headerProvider = headerProvider;
			}

			protected override IImportDecHeader GetHeaderProvider(CusEntryHeader entryHeader) => headerProvider;

			protected override IImportDecLine GetLineProvider(CusEntryLine entryLine) => lineProvider;

			protected override DEMonthlyClosingEntryLineSnapshot LoadCurrentSnapshot() => snapshot;

			readonly IImportDecHeader headerProvider;
			readonly IImportDecLine lineProvider;
			readonly DEMonthlyClosingEntryLineSnapshot snapshot;
		}
	}
}
