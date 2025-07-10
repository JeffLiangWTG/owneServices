using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(CusEntryHeader))]
	sealed class CusEntryHeaderTest : Customs.Business.Testing.CusEntryHeaderTest
	{
		public void TestErrorMessageProcessingStrategyParent()
		{
			IErrorMessageProcessingStrategyParent header = Factory.New<CusEntryHeader>();
			AssertType<ErrorMessageProcessingStrategy>(header.ProcessingStrategy);
		}

		public void TestCH_EntryStatusDescription()
		{
			var cusEntryHeader = Factory.New<CusEntryHeader>();

			cusEntryHeader.CH_EntryStatus = CustomsStatusList.Codes.Insufficient;
			AssertEquals(CustomsStatusList.Descriptions.Insufficient, cusEntryHeader.CH_EntryStatusDescription);
		}

		public void TestPhaseDescription()
		{
			var cusEntryHeader = Factory.New<CusEntryHeader>();

			cusEntryHeader.CH_PhaseStatus = CustomsDeclarationPhases.Codes.IDB;
			AssertEquals(CustomsDeclarationPhases.Descriptions.IDB, cusEntryHeader.PhaseDescription);
		}

		public void TestGetCH_StatusDescription()
		{
			var cusEntryHeader = Factory.New<CusEntryHeader>();

			cusEntryHeader.CH_Status = JPMessageStatusList.Codes.Acknowledged;
			AssertEquals(JPMessageStatusList.Descriptions.Acknowledged, cusEntryHeader.CH_StatusDescription);
		}

		public void TestBGMReferenceGenereated()
		{
			CombineAssertions(() =>
			{
				var declaration1 = Factory.New<JobDeclaration>();
				declaration1.JE_TransportMode = "@@@";
				new int[5].ForEach((_) => { declaration1.ActiveEntryHeaders.AddNew(); });
				AssertExceptionThrown<Exception>("Here we need an DB save exception. It is caused by wrong transport mode", () => { Factory.Save(); });
				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var declaration2 = newFactory.New<JobDeclaration>();
				declaration2.ActiveEntryHeaders.AddNew();
				newFactory.Save();
				AssertEquals("0000000006", declaration2.CustomsEntryHeaders[0].CH_BGMReference);
				declaration1.JE_TransportMode = TransportTypeList.Codes.Sea;
				Factory.Save();
				AssertContainsExactElementsInAnyOrder(new[] { "0000000007", "0000000008", "0000000009", "0000000010", "0000000011" }, declaration1.CustomsEntryHeaders.Select(header => header.CH_BGMReference));

				var entryHeaderNoBGMReferenceSimulatingImport = Factory.New<DummyCusEntryHeader>() as Customs.Business.CusEntryHeader;
				entryHeaderNoBGMReferenceSimulatingImport.CH_JE = declaration1.PK;
				Factory.Save();
				entryHeaderNoBGMReferenceSimulatingImport = newFactory.Load<CusEntryHeader>(entryHeaderNoBGMReferenceSimulatingImport.PK);
				AssertNullOrEmpty("A base entry header can be saved without BGM, as happens when importing", entryHeaderNoBGMReferenceSimulatingImport.CH_BGMReference);
				newFactory.Save();
				AssertEquals("Upon factory save, BGM will be assigned", "0000000012", entryHeaderNoBGMReferenceSimulatingImport.CH_BGMReference);
			});
		}

		public new void TestTransactionValueReturnsAggregationOfInvoiceAmountOfMergedHeaders()
		{
			Assert("Invoice headers never merge, therefore no need to test aggregate amounts. See EntryCreationStrategy.", true);
		}

		public void TestCLREventRaised()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
			Factory.Save();

			CombineAssertions(() =>
			{
				entryHeader.CH_EntryStatus = CustomsStatusList.Codes.Cleared;
				Factory.Save();

				var mostRecentLog = declaration.Logs.MostRecentLogByEventTime(Events.CustomsCleared);
				AssertNull("CLR not raised if only part is cleared", mostRecentLog);

				entryHeader2.CH_EntryStatus = CustomsStatusList.Codes.Cleared;
				Factory.Save();

				mostRecentLog = declaration.Logs.MostRecentLogByEventTime(Events.CustomsCleared);
				AssertNotNull("Customs Cleared log added after all is", mostRecentLog);
			});
		}

		public void TestECCEventRaised()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
			Factory.Save();

			CombineAssertions(() =>
			{
				entryHeader.CH_EntryStatus = CustomsStatusList.Codes.Cleared;
				Factory.Save();

				var mostRecentLog = declaration.Logs.MostRecentLogByEventTime(Events.ExportCustomsCleared);
				AssertNull("CLR not raised if only part is cleared", mostRecentLog);

				entryHeader2.CH_EntryStatus = CustomsStatusList.Codes.Cleared;
				Factory.Save();

				mostRecentLog = declaration.Logs.MostRecentLogByEventTime(Events.ExportCustomsCleared);
				AssertNotNull("Customs Cleared log added after all is", mostRecentLog);
			});
		}

		public void TestINACCSStatus()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_Status = JPMessageStatusList.Codes.Acknowledged;
			entryHeader.CH_EntryStatus = CustomsStatusList.Codes.Cleared;
			entryHeader.CH_PhaseStatus = CustomsDeclarationPhases.Codes.IDB;
			CombineAssertions(() =>
			{
				AssertEquals(JPMessageStatusList.Codes.Acknowledged, entryHeader.MessageStatus);
				AssertEquals(CustomsStatusList.Codes.Cleared, entryHeader.CustomsStatus);
				AssertEquals(CustomsDeclarationPhases.Codes.IDB, entryHeader.PhaseStatus);
			});

			entryHeader.MessageStatus = JPMessageStatusList.Codes.Rejected;
			entryHeader.CustomsStatus = CustomsStatusList.Codes.Inspected;
			entryHeader.PhaseStatus = CustomsDeclarationPhases.Codes.EDE;

			CombineAssertions(() =>
			{
				AssertEquals(JPMessageStatusList.Codes.Rejected, entryHeader.CH_Status);
				AssertEquals(CustomsStatusList.Codes.Inspected, entryHeader.CH_EntryStatus);
				AssertEquals(CustomsDeclarationPhases.Codes.EDE, entryHeader.PhaseStatus);
			});
		}

		[TestDate(2007, 12, 12, 12, 12, 12)]
		public override void TestFOBAndCIFFigures()
		{
			if (!IntegratedCountryHelper.CountryHasBuiltInDeclaration(ImportJobDeclaration.CountryCode) || ImportJobDeclaration.IsDeclarationIntegrated)
			{
				Assert("For integrated countries, merge is turned off.", true);
				return;
			}

			var declaration = ImportJobDeclaration;
			var setup = GetChargesCurrencyTestSetup();
			setup.SetupJobDecWithOFTAndCIFCharges(declaration, declaration.LocalCurrencyCode);
			DoMerge(declaration);

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: EntryHeaders.Count", 1, declaration.CustomsEntryHeaders.Count);
				var entryHeader = declaration.CustomsEntryHeaders[0];
				AssertEquals("Overseas Freight for the entry does not include ForeignInlandFreight", 500m, entryHeader.OverseasInsurance.Amount);
				AssertEquals("FOB for the entry", setup.ExpectedFOB, entryHeader.FOB.Amount);
				AssertEquals("CIF for the entry", setup.ExpectedCIF, entryHeader.CIF.Amount);
			});
		}

		public void TestCH_CargoType()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("", entryHeader.CH_CargoType);
				AssertEquals("", entryHeader.CargoTypeDescription);
			});
			
			entryHeader.CH_InspectionStatus = "Z1RT";
			CombineAssertions(() =>
			{
				AssertEquals("Z", entryHeader.CH_CargoType);
				AssertEquals("Damaged Cargo", entryHeader.CargoTypeDescription);
			});
		}

		public void TestCH_InspectionType()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("", entryHeader.CH_InspectionType);
				AssertEquals("", entryHeader.InspectionTypeDescription);
			});
			entryHeader.CH_InspectionStatus = "Z1RT";
			CombineAssertions(() =>
			{
				AssertEquals("1", entryHeader.CH_InspectionType);
				AssertEquals("Simple Inspection", entryHeader.InspectionTypeDescription);
			});
		}

		public void TestCH_InspectionSubType()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("", entryHeader.CH_InspectionSubType);
				AssertEquals("", entryHeader.InspectionSubTypeDescription);
			});
			entryHeader.CH_InspectionStatus = "Z1RT";
			CombineAssertions(() =>
			{
				AssertEquals("R", entryHeader.CH_InspectionSubType);
				AssertEquals("On-Site Inspection", entryHeader.InspectionSubTypeDescription);
			});
		}

		public void TestCH_DocumentRequestType()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("", entryHeader.CH_DocumentRequestType);
				AssertEquals("", entryHeader.DocumentRequestTypeDescription);
			});
			entryHeader.CH_InspectionStatus = "Z1RT";
			CombineAssertions(() =>
			{
				AssertEquals("T", entryHeader.CH_DocumentRequestType);
				AssertEquals("Original Document Request (During Inspection)", entryHeader.DocumentRequestTypeDescription);
			});
		}

		protected override IChargesCurrencyTestSetup GetChargesCurrencyTestSetup()
		{
			return new ChargesCurrencyTestSetup();
		}

		sealed class ChargesCurrencyTestSetup : IChargesCurrencyTestSetup
		{
			ZDecimal IChargesCurrencyTestSetup.ExpectedFOB => 10300m;

			ZDecimal IChargesCurrencyTestSetup.ExpectedCIF => 10800m;

			void IChargesCurrencyTestSetup.SetupJobDecWithOFTAndCIFCharges(BaseJobDeclaration declaration, ZString currencyCode)
			{
				declaration.AutoCreateChargesBasedOnIncoTerm = false;
				var baseJobComInvoiceHeader = declaration.Invoices.AddNew();
				baseJobComInvoiceHeader.JZ_InvoiceAmount = 10000m;
				baseJobComInvoiceHeader.JZ_RX_NKInvoice_Currency = currencyCode;
				baseJobComInvoiceHeader.JZ_IncoTerm = "FOB";
				var baseJobComInvoiceLine = baseJobComInvoiceHeader.JobComInvoiceLines.AddNew();
				baseJobComInvoiceLine.JI_LinePrice = 10000m;
				var baseInvoiceCharge = baseJobComInvoiceHeader.Charges.AddNew();
				baseInvoiceCharge.J7_ChargeType = "FIF";
				baseInvoiceCharge.J7_Amount = 200m;
				baseInvoiceCharge.J7_IsDutiable = false;
				baseInvoiceCharge.J7_IsIncludedInITOT = true;
				var baseInvoiceCharge2 = baseJobComInvoiceHeader.Charges.AddNew();
				baseInvoiceCharge2.J7_ChargeType = "ONS";
				baseInvoiceCharge2.J7_Amount = 500m;
				baseInvoiceCharge2.J7_RX_NKCurrency = baseJobComInvoiceHeader.Invoice_Currency.RX_Code;
			}
		}

		protected override BaseJobDeclaration ImportJobDeclaration
		{
			get
			{
				var declaration = base.ImportJobDeclaration;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				return declaration;
			}
		}

		protected override Type ExpectedChargeCollectionType => typeof(CusEntryHeaderChargesCollection<CusEntryHeaderCharges>);

		protected override Type ExpectedChargeType => typeof(CusEntryHeaderCharges);
	}

	sealed class DummyCusEntryHeader : Customs.Business.CusEntryHeader
	{
		public DummyCusEntryHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
