using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Moq;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class JobDeclarationRatingAdapterTest : TestCaseWithFactory
	{
		public void TestOnAutoRated_LVSForConsolidation_AddReferenceToInvoiceLineDescription()
		{
			var mockEntry = Factory.NewMoq<CusEntryHeader>();
			var entry = mockEntry.Object;
			entry.CH_JE = declaration.PK;
			entry.CH_BGMReference = "BGMREF";
			entry.CH_Status = "XXX";

			var autoRatedCharge = new Mock<IAutoRatedCharge>();
			autoRatedCharge.Setup(c => c.InvoiceLineDescription).Returns("McLaren");

			var adapter = (JobDeclarationRatingAdapter<JobDeclaration>)declaration.RatingAdapter;
			adapter.OnAutoRated(new[] { autoRatedCharge.Object });
			autoRatedCharge.VerifySet(c => c.InvoiceLineDescription = It.IsAny<ZString>(), Times.Never);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "000";

			adapter = (JobDeclarationRatingAdapter<JobDeclaration>)declaration.RatingAdapter;
			adapter.OnAutoRated(new[] { autoRatedCharge.Object });
			autoRatedCharge.VerifySet(c => c.InvoiceLineDescription = "McLaren - LVS ID : 000", Times.Once);

			Assert(true);
		}

		public void TestAutoRating_CustomsInfo()
		{
			var entry1 = declaration.ActiveEntryHeaders.AddNew();
			var entry2 = declaration.ActiveEntryHeaders.AddNew();
			var entry3 = declaration.ActiveEntryHeaders.AddNew();
			entry1.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entry2.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			entry3.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var rating1 = (IAutoRatingCustomsInfo)declaration.RatingAdapter;
			AssertEquals(1, rating1.Entries.Count);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				var rating2 = (IAutoRatingCustomsInfo)declaration.RatingAdapter;
				AssertEquals(1, rating2.Entries.Count);
			}
		}

		public void TestAutoRatingSubHeaderCount()
		{
			var notifier = new SendsMessagesToCustomsShutterUpperer();
			var invoice = declaration.Invoices.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
			JobComInvoiceLineTestHelper.FillInvoiceLine((JobComInvoiceLine)invoice.InvoiceLines.AddNew(), 100);
			JobComInvoiceLineTestHelper.FillInvoiceLine((JobComInvoiceLine)invoice.InvoiceLines.AddNew(), 100);
			JobComInvoiceLineTestHelper.FillInvoiceLine((JobComInvoiceLine)invoice2.InvoiceLines.AddNew(), 100);
			JobComInvoiceLineTestHelper.FillInvoiceLine((JobComInvoiceLine)invoice2.InvoiceLines.AddNew(), 100);
			declaration.CA_MergeBy = B3MergeByList.Codes.ClassificationOrTariffOverMultipleInvoices;
			declaration.DoMerge(notifier);
			Factory.Save();
			var rating = (IAutoRatingCustomsInfo)declaration.RatingAdapter;
			AssertEquals(1, rating.SubHeaderCount);
		}

		public void TestShipmentCount_Import()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			SetupHouseBillsForRatingShipmentCount();
			AssertEquals("Import Shipment Count", 2m, ((RateableMeasureSet)declaration.RatingAdapter.RateableMeasures).Shipments);
		}

		public void TestShipmentCount_LVSForConsolidation()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			SetupHouseBillsForRatingShipmentCount();
			AssertEquals("LVX Shipment Count", 1m, ((RateableMeasureSet)declaration.RatingAdapter.RateableMeasures).Shipments);
		}

		public void TestShipmentCount_B2Adjustments()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			SetupHouseBillsForRatingShipmentCount();
			AssertEquals("B2 Shipment Count", 1m, ((RateableMeasureSet)declaration.RatingAdapter.RateableMeasures).Shipments);
		}

		public void TestShipmentCount_XTypeEntry()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			SetupHouseBillsForRatingShipmentCount();
			AssertEquals("B3X Shipment Count", 1m, ((RateableMeasureSet)declaration.RatingAdapter.RateableMeasures).Shipments);
		}

		public void TestFreightMode()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("Base Freight Mode", FreightMode.LCL, declaration.RatingAdapter.FreightMode);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			AssertEquals("LVX Freight Mode", FreightMode.ROA, declaration.RatingAdapter.FreightMode);
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			AssertEquals("B2 Freight Mode", FreightMode.ROA, declaration.RatingAdapter.FreightMode);
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			AssertEquals("B3X Freight Mode", FreightMode.ROA, declaration.RatingAdapter.FreightMode);
		}

		public void TestJobDirection()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Base Job Direction", Directions.Export, declaration.RatingAdapter.JobDirection);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			AssertEquals("LVX Job Direction", Directions.Import, declaration.RatingAdapter.JobDirection);
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			AssertEquals("B2 Job Direction", Directions.Import, declaration.RatingAdapter.JobDirection);
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			AssertEquals("B3X Job Direction", Directions.Import, declaration.RatingAdapter.JobDirection);
		}

		public void TestDates()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfArrival = new ZDateTime(2015, 1, 5);
			declaration.JE_ExportDate = new ZDateTime(2015, 1, 3);
			declaration.CA_K84AccountingDate = new ZDateTime(2015, 1, 8);
			AssertEquals("Base Earliest Date", new ZDateTime(2015, 1, 3), declaration.RatingAdapter.JobDatesProvider.EarliestPossibleDate);
			AssertEquals("Base Latest Date", new ZDateTime(2015, 1, 5), declaration.RatingAdapter.JobDatesProvider.LatestPossibleDate);
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			AssertEquals("B2 Earliest Date", new ZDateTime(2015, 1, 8), declaration.RatingAdapter.JobDatesProvider.EarliestPossibleDate);
			AssertEquals("B2 Latest Date", new ZDateTime(2015, 1, 8), declaration.RatingAdapter.JobDatesProvider.LatestPossibleDate);
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			AssertEquals("B3X Earliest Date", new ZDateTime(2015, 1, 8), declaration.RatingAdapter.JobDatesProvider.EarliestPossibleDate);
			AssertEquals("B3X Latest Date", new ZDateTime(2015, 1, 8), declaration.RatingAdapter.JobDatesProvider.LatestPossibleDate);
		}

		public void TestGetMeasurementsForAutoRating_OGD_Import()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			SetupOGDInvoiceLinesForRatingMeasures();
			var rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			CombineAssertions(() =>
			{
				AssertEquals("No CFIA measure", false, rateableMeasures.HasMeasureType(MeasureType.CFIALine));
				AssertEquals("No NR measure", false, rateableMeasures.HasMeasureType(MeasureType.NRCANLine));
				AssertEquals("No TC measure", false, rateableMeasures.HasMeasureType(MeasureType.TCLine));
				AssertEquals("No IC measure", false, rateableMeasures.HasMeasureType(MeasureType.SITTLine));

				declaration.CA_OGDCFIA = true;
				declaration.CA_OGDIC = true;
				declaration.CA_OGDNR = true;
				declaration.CA_OGDTC = true;

				rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
				AssertEquals("CFIA count", 4m, rateableMeasures.GetActual(MeasureType.CFIALine));
				AssertEquals("NR count", 3m, rateableMeasures.GetActual(MeasureType.NRCANLine));
				AssertEquals("TC count", 2m, rateableMeasures.GetActual(MeasureType.TCLine));
				AssertEquals("IC count", 1m, rateableMeasures.GetActual(MeasureType.SITTLine));
			});
		}

		public void TestGetMeasurementsForAutoRating_OGD_SwitchToExport()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			SetupOGDInvoiceLinesForRatingMeasures();

			declaration.CA_OGDCFIA = true;
			declaration.CA_OGDIC = true;
			declaration.CA_OGDNR = true;
			declaration.CA_OGDTC = true;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			CombineAssertions("OGD Rating Doesn't occur for Export", () =>
			{
				AssertEquals("No CFIA measure", false, rateableMeasures.HasMeasureType(MeasureType.CFIALine));
				AssertEquals("No NR measure", false, rateableMeasures.HasMeasureType(MeasureType.NRCANLine));
				AssertEquals("No TC measure", false, rateableMeasures.HasMeasureType(MeasureType.TCLine));
				AssertEquals("No IC measure", false, rateableMeasures.HasMeasureType(MeasureType.SITTLine));
			});
		}

		public void TestGetMeasurementsForAutoRating_IID()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.Invoices.AddNew();
			var line = declaration.InvoiceLines.AddNew();

			CombineAssertions(() =>
			{
				SetLineIndicators(YesNoList.Codes.Yes);
				var rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
				AssertEquals("CFIA count", 1m, rateableMeasures.GetActual(MeasureType.CFIALine));
				AssertEquals("NR count", 1m, rateableMeasures.GetActual(MeasureType.NRCANLine));
				AssertEquals("TC count", 1m, rateableMeasures.GetActual(MeasureType.TCLine));
				AssertEquals("HC count", 1m, rateableMeasures.GetActual(MeasureType.HCLine));
				AssertEquals("PHAC count", 1m, rateableMeasures.GetActual(MeasureType.PHACLine));
				AssertEquals("ECCC count", 1m, rateableMeasures.GetActual(MeasureType.ECCCLine));
				AssertEquals("DFO count", 1m, rateableMeasures.GetActual(MeasureType.DFOLine));
				AssertEquals("CNSC count", 1m, rateableMeasures.GetActual(MeasureType.CNSCLine));
				AssertEquals("GAC count", 1m, rateableMeasures.GetActual(MeasureType.GACLine));

				SetLineIndicators(YesNoList.Codes.No);
				rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
				AssertEquals("No CFIA measure", false, rateableMeasures.HasMeasureType(MeasureType.CFIALine));
				AssertEquals("No NRCAN measure", false, rateableMeasures.HasMeasureType(MeasureType.NRCANLine));
				AssertEquals("No TC measure", false, rateableMeasures.HasMeasureType(MeasureType.TCLine));
				AssertEquals("No HC measure", false, rateableMeasures.HasMeasureType(MeasureType.HCLine));
				AssertEquals("No PHAC measure", false, rateableMeasures.HasMeasureType(MeasureType.PHACLine));
				AssertEquals("No ECCC measure", false, rateableMeasures.HasMeasureType(MeasureType.ECCCLine));
				AssertEquals("No DFO measure", false, rateableMeasures.HasMeasureType(MeasureType.DFOLine));
				AssertEquals("No CNSC measure", false, rateableMeasures.HasMeasureType(MeasureType.CNSCLine));
				AssertEquals("No GAC measure", false, rateableMeasures.HasMeasureType(MeasureType.GACLine));
			});

			void SetLineIndicators(ZString value)
			{
				line.CA_CFIAInd = value;
				line.CA_NRCanInd = value;
				line.CA_TCInd = value;
				line.CA_HCInd = value;
				line.CA_PHACInd = value;
				line.CA_ECCCInd = value;
				line.CA_DFOInd = value;
				line.CA_CNSCInd = value;
				line.CA_GACInd = value;
			}
		}

		public void TestGetAmountOfPGALinesForAutoRating_IID()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew() as JobComInvoiceLine;

			var rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.PGALines));

			invoiceLine1.CA_CFIAInd = YesNoList.Codes.Yes;
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(1m, rateableMeasures.GetActual(MeasureType.PGALines));

			invoiceLine1.CA_CFIAInd = YesNoList.Codes.No;
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.PGALines));

			invoiceLine1.CA_GACInd = YesNoList.Codes.Yes;
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(1m, rateableMeasures.GetActual(MeasureType.PGALines));

			var invoiceLine2 = invoice1.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine2.CA_CFIAInd = YesNoList.Codes.Yes;
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(2m, rateableMeasures.GetActual(MeasureType.PGALines));

			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine3 = invoice2.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine3.CA_CFIAInd = YesNoList.Codes.Yes;
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(3m, rateableMeasures.GetActual(MeasureType.PGALines));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(false, rateableMeasures.HasMeasureType(MeasureType.PGALines));
		}

		public void TestGetAmountOfPGALinesForAutoRating_NotIIDAndNotLVS()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.CSA;
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew() as JobComInvoiceLine;

			var rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.PGALines));

			declaration.CA_OGDCFIA = true;
			invoiceLine1.CA_AirsCode = "xxx";
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(1m, rateableMeasures.GetActual(MeasureType.PGALines));

			declaration.CA_OGDCFIA = false;
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.PGALines));

			declaration.CA_OGDCFIA = true;
			declaration.CA_OGDNR = true;
			invoiceLine1.CA_ImportReasonCode = "xx";

			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			ZString tariffCode1 = "0000000000";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusTariffType1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, PGACodes.Codes.NRCan);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, cusTariffType1.PK, tariffCode1, startDate, endDate);
			invoiceLine1.JI_Tariff = tariffCode1;

			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(2m, rateableMeasures.GetActual(MeasureType.PGALines));

			ZString tariffCode2 = "0000000001";
			var cusTariffType2 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, PGACodes.Codes.TC);
			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, cusTariffType2.PK, tariffCode2, startDate, endDate);
			invoiceLine1.JI_Tariff = tariffCode2;
			declaration.CA_OGDTC = true;

			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(2m, rateableMeasures.GetActual(MeasureType.PGALines));

			invoiceLine1.JI_Tariff = null;
			declaration.CA_OGDIC = true;
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(2m, rateableMeasures.GetActual(MeasureType.PGALines));

			declaration.CA_OGDIC = false;
			declaration.CA_OGDNR = true;
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(2m, rateableMeasures.GetActual(MeasureType.PGALines));

			declaration.CA_OGDIC = false;
			declaration.CA_OGDNR = false;
			declaration.CA_OGDTC = true;
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(2m, rateableMeasures.GetActual(MeasureType.PGALines));

			var invoiceLine2 = invoice1.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine2.CA_ImportReasonCode = "xx";
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(3m, rateableMeasures.GetActual(MeasureType.PGALines));

			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine3 = invoice2.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine3.CA_ImportReasonCode = "xx";
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(4m, rateableMeasures.GetActual(MeasureType.PGALines));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(false, rateableMeasures.HasMeasureType(MeasureType.PGALines));
		}

		public void TestAutoRatingStatusInfo()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertAutoRatingStatusInfo(declaration, false, "Merge has not occurred");
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			declaration.JE_MessageStatus = MessageStatusList.Codes.AwaitingOriginal;
			Factory.Save();
			AssertAutoRatingStatusInfo(declaration, true, "The Declaration is waiting for a response");
			declaration.JE_MessageStatus = MessageStatusList.Codes.ClearOriginal;
			AssertAutoRatingStatusInfo(declaration, true, string.Empty);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var lvxJob = Factory.New<JobDeclaration>();
			lvxJob.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var lvxInvoiceHeader = lvxJob.LVXInvoiceHeader;
			lvxInvoiceHeader.InvoiceLines.AddNew();
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(lvxInvoiceHeader, declaration);
			AssertAutoRatingStatusInfo(declaration, false, "Courier LVS Declaration jobs attached to this Consolidated LVS Declaration");
		}

		public void TestAutoRatingStatusInfo_B2Adjustments()
		{
			AssertAutoRatingStatusInfoForB2OrB3X(JobMessageTypeList.Codes.B2Adjustments);
		}

		public void TestAutoRatingStatusInfo_XTypeEntry()
		{
			AssertAutoRatingStatusInfoForB2OrB3X(JobMessageTypeList.Codes.XTypeEntry);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
		JobDeclaration declaration;

		void AssertAutoRatingStatusInfoForB2OrB3X(ZString messageType)
		{
			declaration.JE_MessageType = messageType;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			AssertAutoRatingStatusInfo(declaration, false, "Merge has not occurred");

			var header = declaration.Invoices.AddNew();
			header.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
			var line = header.AsAccountForFilteredInvoiceLines.AddNew();
			line.CA_OriginalLineNo = "1";
			AssertAutoRatingStatusInfo(declaration, true, string.Empty);
		}

		static void AssertAutoRatingStatusInfo(JobDeclaration declaration, bool expectedCanExecute, string expectedMessage)
		{
			var autoRating = declaration.RatingAdapter;
			AssertEquals("CanExecute", expectedCanExecute, autoRating.StatusInformation.CanExecute);
			if (string.IsNullOrEmpty(expectedMessage))
			{
				Assert("Message", string.IsNullOrEmpty(autoRating.StatusInformation.Message));
			}
			else
			{
				Assert("Message", autoRating.StatusInformation.Message.Contains(expectedMessage));
			}
		}

		void SetupHouseBillsForRatingShipmentCount()
		{
			declaration.JE_HouseBill = "HBL1";
			var newBill = declaration.Bills.AddNew();
			newBill.CU_BillType = BillTypeList.Codes.HouseBill;
			newBill.CU_BillNum = "HBL2";
		}

		void SetupOGDInvoiceLinesForRatingMeasures()
		{
			CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.TC, "4011100000", "TPR");
			CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.NRCan, "7321111000", "EEF");
			declaration.CA_ServiceOption = ZString.Empty;
			declaration.Invoices.AddNew();
			var line1 = declaration.InvoiceLines.AddNew();
			line1.CA_AirsCode = "1";
			var line2 = declaration.InvoiceLines.AddNew();
			line2.CA_AirsCode = "1";
			var line3 = declaration.InvoiceLines.AddNew();
			line3.CA_AirsCode = "1";
			var line4 = declaration.InvoiceLines.AddNew();
			line4.CA_AirsCode = "1";
			declaration.InvoiceLines.AddNew();
			var line6 = declaration.InvoiceLines.AddNew();
			line6.CA_ImportReasonCode = "1";
			line6.JI_Tariff = "7321111000";
			var line7 = declaration.InvoiceLines.AddNew();
			line7.CA_ImportReasonCode = "1";
			line7.JI_Tariff = "7321111000";
			var line8 = declaration.InvoiceLines.AddNew();
			line8.CA_ImportReasonCode = "1";
			line8.JI_Tariff = "7321111000";
			var line9 = declaration.InvoiceLines.AddNew();
			line9.CA_ImportReasonCode = "1";
			line9.JI_Tariff = "4011100000";
			var line10 = declaration.InvoiceLines.AddNew();
			line10.CA_ImportReasonCode = "1";
			line10.JI_Tariff = "4011100000";
			var line11 = declaration.InvoiceLines.AddNew();
			line11.CA_ImportReasonCode = "1";
		}
	}
}
