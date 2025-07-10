using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.US;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageBuilders.Testing
{
	sealed class LowValueShipmentsMessageWrapperTest : TestCaseWithFactory
	{
		public void TestTdtSegmentForLVS()
		{
			var lvsjob = Factory.NewWithValidTestData<JobDeclaration>();
			lvsjob.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			lvsjob.Invoices.AddNew().JobComInvoiceLines.AddNew();
			lvsjob.JE_TransportMode = Constants.TransportModes.Air;
			lvsjob.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			lvsjob.DoMerge();

			var lvsEntryHeader = lvsjob.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
			lvsEntryHeader.ResetTotalsAndCachedValues();
			var lvsEntryline = lvsEntryHeader.AllEntryLines.FirstOrDefault();
			lvsEntryline.CL_CustomsValue = 2500;

			var wrapper = new LowValueShipmentsMessageWrapper(lvsEntryHeader);
			var messageBuilder = new B3CusdecMessageBuilder<B3Message>(wrapper, MessageSubTypes.Create);
			var message = messageBuilder.PopulateMessages().GetBuilderResults().First().Message.EM_MessageText.Replace("'", "'\r\n");
			AssertContains("TDT+11++2", message);

			lvsjob.JE_TransportMode = Constants.TransportModes.Road;
			lvsjob.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			lvsjob.DoMerge();
			entryHeader = lvsjob.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
			lvsEntryline = lvsEntryHeader.AllEntryLines.FirstOrDefault();
			lvsEntryline.CL_CustomsValue = 2500;
			wrapper = new LowValueShipmentsMessageWrapper(entryHeader);
			messageBuilder = new B3CusdecMessageBuilder<B3Message>(wrapper, MessageSubTypes.Create);
			message = messageBuilder.PopulateMessages().GetBuilderResults().First().Message.EM_MessageText.Replace("'", "'\r\n");
			AssertContains("TDT+11++2", message);
		}

		#region TestIB3HeaderProperties

		public void TestLVSClassificationLinesDoesNotSetPageRelativeLineNumbers()
		{
			IB3Header b3Header = new LowValueShipmentsMessageWrapper(entryHeader);
			AssertEquals("PositiveClassificationLines count", 5, b3Header.PositiveClassificationLines.Count());

			foreach (JobComInvoiceLine line in declaration.InvoiceLines)
			{
				AssertEquals("CA_PageRelativeLineNumber is not set", 0, line.CA_PageRelativeLineNumber);
			}
		}

		[TestDate(2015, 12, 31)]
		public void TestIB3HeaderProperties()
		{
			IB3Header b3Header = new LowValueShipmentsMessageWrapper(entryHeader);
			AssertEquals("BatchNumber", "<<UNIQUE BATCH NUMBER PLACE HOLDER>>", b3Header.BatchNumber);
			AssertEquals("B3TypeCode", B3EntryTypeList.Codes.LowValueShipments, b3Header.B3TypeCode);
			AssertEquals("PaymentCode", string.Empty, b3Header.PaymentCode);
			AssertEquals("CBSAOffice", "351", b3Header.CBSAOffice);
			AssertEquals("PortOfUnlading", string.Empty, b3Header.PortOfUnlading);
			AssertEquals("WarehouseNumber", string.Empty, b3Header.WarehouseNumber);
			AssertEquals("AccountSecurityCode", "12345", b3Header.AccountSecurityCode);
			AssertEquals("TransactionNumber", "000067897", b3Header.TransactionNumber);
			AssertBusinessNumber(b3Header);

			AssertEquals("GSTNumber", "0987654321", b3Header.GSTNumber);
			declaration.JE_GS_NKCusAgent = broker.GS_Code;
			AssertEquals("BusinessNumber", string.Empty, b3Header.BusinessNumber);
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			AssertEquals("GSTNumber", "0987654321", b3Header.GSTNumber);
			AssertEquals("Importer", "IMPORTER NAME", b3Header.Importer.E2_CompanyName);
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			AssertEquals("GSTNumber", string.Empty, b3Header.GSTNumber);
			AssertEquals("Importer", "Various", b3Header.Importer.E2_CompanyName);
			AssertEquals("ReleaseDate", declaration.JE_EntryAuthorisationDate, b3Header.ReleaseDate);
			AssertEquals("TotalValueForDuty", 3100m, b3Header.TotalValueForDuty);
			AssertEquals("B3Comments", "Note to be printed on B3", b3Header.B3Comments);

			AssertEquals("TransportMode", "2", b3Header.TransportMode);
			entryHeader.AllEntryLines[0].CL_CustomsValue = 0;
			entryHeader.ResetTotalsAndCachedValues();
			AssertEquals("TransportMode", "2", b3Header.TransportMode);

			AssertEquals("B3BInputReleases Count", 1, b3Header.B3BInputReleases.Count());
			AssertEquals("CargoControlNumber 1", ZString.Empty, b3Header.B3BInputReleases.ElementAt(0).CargoControlNumber);
			AssertEquals("DateOfRelease 1", declaration.JE_EntryAuthorisationDate, b3Header.B3BInputReleases.ElementAt(0).DateOfRelease);
			declaration.JE_EntryAuthorisationDate = ZDateTime.Empty;
			var number1 = declaration.AdditionalReferenceNumbers.AddNew();
			number1.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number1.CE_EntryNum = "1111111";
			AssertEquals("B3BInputReleases Count", false, b3Header.B3BInputReleases.Any());

			AssertEquals("CarrierCodeAtImportation", "2121", b3Header.CarrierCodeAtImportation);
			B3ImportMessageWrapperTest.AssertTotalAmounts(b3Header.PositiveTotalAmounts, 0m, 1471.28m, 254, 5841.5m, 7566.78m);
			//TODO: Assert negative
			//B3ImportMessageWrapperTest.AssertTotalAmounts(b3Header.NegativeTotalAmounts, -85, -84, 0, -175, -344);
		}

		public void TestIsCalculationsDone()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			cusEntryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			cusEntryHeader.CH_JE = declaration.PK;
			IB3Header b3Header = new LowValueShipmentsMessageWrapper(cusEntryHeader);
			Assert(b3Header.IsCalculationsDone);
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var entryLine = cusEntryHeader.AllEntryLines.AddNew();
			invoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine);
			var gst = ((JobComInvoiceLine)invoiceLine).DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			gst.C1_Override = true;
			gst.C1_Amount = 60m;
			b3Header.RefreshCachedValues();
			Assert(!b3Header.IsCalculationsDone);
			var gst1 = entryLine.Fees.AddNew();
			gst1.CF_ChargeType = EntryChargeTypeList.Codes.TotalGSTAmount;
			gst1.CF_ChargeAmount = 10m;
			b3Header.RefreshCachedValues();
			Assert(b3Header.IsCalculationsDone);

			gst1.CF_ChargeAmount = 0m;
			b3Header.RefreshCachedValues();
			Assert(!b3Header.IsCalculationsDone);

			gst.C1_ExemptCode = "48";
			b3Header.RefreshCachedValues();
			Assert(b3Header.IsCalculationsDone);
		}

		[TestDate(2015, 12, 31)]
		public void TestPaymentCode1()
		{
			CACustomsDataRegistry.Instance.DefaultExciseTaxFromCustomsTariff.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var factory = JobComInvoiceLineTestHelper.PopulateDutiesAndTaxesRefFilesReturningFactory();
			var helper = new DeclarationTestHelper(factory, true);
			var canada = factory.Load<RefCountry>(Constants.CountryGuids.Canada);

			var jobDeclaration = factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			jobDeclaration.JE_OH_Importer = helper.CreateOrganisation("SUP", "SUPPLIER NAME", "AUMEL", "SUPPLIER ADDRESS", "MELBOURNE", "VIC", "3000", "123 4567").PK;
			jobDeclaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			jobDeclaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;

			var entry = jobDeclaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entry.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted;
			IB3Header b3Header = new LowValueShipmentsMessageWrapper(entry);

			jobDeclaration.ImporterAddInfo.ZO_IsImporterDirectPayment = false;
			jobDeclaration.ImporterAddInfo.ZO_IsGSTDirectPayment = false;
			jobDeclaration.ImporterAddInfo.ZO_IsLVSImporterDirectPayment = false;
			AssertEquals("PaymentCode", ZString.Empty, b3Header.PaymentCode);

			jobDeclaration.ImporterAddInfo.ZO_IsImporterDirectPayment = true;
			jobDeclaration.ImporterAddInfo.ZO_IsLVSImporterDirectPayment = false;
			jobDeclaration.ImporterAddInfo.ZO_IsGSTDirectPayment = false;
			AssertEquals("PaymentCode", ZString.Empty, b3Header.PaymentCode);

			jobDeclaration.ImporterAddInfo.ZO_IsImporterDirectPayment = false;
			jobDeclaration.ImporterAddInfo.ZO_IsLVSImporterDirectPayment = true;
			jobDeclaration.ImporterAddInfo.ZO_IsGSTDirectPayment = false;
			AssertEquals("PaymentCode", "I", b3Header.PaymentCode);

			jobDeclaration.ImporterAddInfo.ZO_IsImporterDirectPayment = false;
			jobDeclaration.ImporterAddInfo.ZO_IsLVSImporterDirectPayment = false;
			jobDeclaration.ImporterAddInfo.ZO_IsGSTDirectPayment = true;
			AssertEquals("PaymentCode", "G", b3Header.PaymentCode);

			jobDeclaration.ImporterAddInfo.ZO_IsImporterDirectPayment = true;
			jobDeclaration.ImporterAddInfo.ZO_IsLVSImporterDirectPayment = true;
			jobDeclaration.ImporterAddInfo.ZO_IsGSTDirectPayment = true;
			AssertEquals("PaymentCode", "I", b3Header.PaymentCode);

			jobDeclaration.ImporterAddInfo.ZO_IsImporterDirectPayment = true;
			jobDeclaration.ImporterAddInfo.ZO_IsLVSImporterDirectPayment = true;
			jobDeclaration.ImporterAddInfo.ZO_IsGSTDirectPayment = false;
			AssertEquals("PaymentCode", "I", b3Header.PaymentCode);

			jobDeclaration.ImporterAddInfo.ZO_IsImporterDirectPayment = true;
			jobDeclaration.ImporterAddInfo.ZO_IsLVSImporterDirectPayment = false;
			jobDeclaration.ImporterAddInfo.ZO_IsGSTDirectPayment = true;
			AssertEquals("PaymentCode", "G", b3Header.PaymentCode);

			jobDeclaration.ImporterAddInfo.ZO_IsImporterDirectPayment = false;
			jobDeclaration.ImporterAddInfo.ZO_IsLVSImporterDirectPayment = true;
			jobDeclaration.ImporterAddInfo.ZO_IsGSTDirectPayment = true;
			AssertEquals("PaymentCode", "I", b3Header.PaymentCode);

			jobDeclaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			jobDeclaration.ImporterAddInfo.ZO_IsImporterDirectPayment = true;
			jobDeclaration.ImporterAddInfo.ZO_IsLVSImporterDirectPayment = true;
			jobDeclaration.ImporterAddInfo.ZO_IsGSTDirectPayment = true;
			AssertEquals("PaymentCode", ZString.Empty, b3Header.PaymentCode);
		}

		[TestDate(2016, 1, 1)]
		public void TestPaymentCode2()
		{
			CACustomsDataRegistry.Instance.DefaultExciseTaxFromCustomsTariff.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var factory = JobComInvoiceLineTestHelper.PopulateDutiesAndTaxesRefFilesReturningFactory();
			var helper = new DeclarationTestHelper(factory, true);
			var canada = factory.Load<RefCountry>(Constants.CountryGuids.Canada);

			var jobDeclaration = factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			jobDeclaration.JE_OH_Importer = helper.CreateOrganisation("SUP", "SUPPLIER NAME", "AUMEL", "SUPPLIER ADDRESS", "MELBOURNE", "VIC", "3000", "123 4567").PK;
			jobDeclaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			jobDeclaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;

			var entry = jobDeclaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entry.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted;
			IB3Header b3Header = new LowValueShipmentsMessageWrapper(entry);

			jobDeclaration.ImporterAddInfo.ZO_IsImporterDirectPayment = false;
			jobDeclaration.ImporterAddInfo.ZO_IsGSTDirectPayment = false;
			jobDeclaration.ImporterAddInfo.ZO_IsLVSImporterDirectPayment = false;
			AssertEquals("PaymentCode", ZString.Empty, b3Header.PaymentCode);

			jobDeclaration.ImporterAddInfo.ZO_IsImporterDirectPayment = true;
			jobDeclaration.ImporterAddInfo.ZO_IsLVSImporterDirectPayment = false;
			jobDeclaration.ImporterAddInfo.ZO_IsGSTDirectPayment = false;
			AssertEquals("PaymentCode", ZString.Empty, b3Header.PaymentCode);

			jobDeclaration.ImporterAddInfo.ZO_IsImporterDirectPayment = false;
			jobDeclaration.ImporterAddInfo.ZO_IsLVSImporterDirectPayment = true;
			jobDeclaration.ImporterAddInfo.ZO_IsGSTDirectPayment = false;
			AssertEquals("PaymentCode", "I", b3Header.PaymentCode);

			jobDeclaration.ImporterAddInfo.ZO_IsImporterDirectPayment = false;
			jobDeclaration.ImporterAddInfo.ZO_IsLVSImporterDirectPayment = false;
			jobDeclaration.ImporterAddInfo.ZO_IsGSTDirectPayment = true;
			AssertEquals("PaymentCode", "G", b3Header.PaymentCode);

			jobDeclaration.ImporterAddInfo.ZO_IsImporterDirectPayment = true;
			jobDeclaration.ImporterAddInfo.ZO_IsLVSImporterDirectPayment = true;
			jobDeclaration.ImporterAddInfo.ZO_IsGSTDirectPayment = true;
			AssertEquals("PaymentCode", "I", b3Header.PaymentCode);

			jobDeclaration.ImporterAddInfo.ZO_IsImporterDirectPayment = true;
			jobDeclaration.ImporterAddInfo.ZO_IsLVSImporterDirectPayment = true;
			jobDeclaration.ImporterAddInfo.ZO_IsGSTDirectPayment = false;
			AssertEquals("PaymentCode", "I", b3Header.PaymentCode);

			jobDeclaration.ImporterAddInfo.ZO_IsImporterDirectPayment = true;
			jobDeclaration.ImporterAddInfo.ZO_IsLVSImporterDirectPayment = false;
			jobDeclaration.ImporterAddInfo.ZO_IsGSTDirectPayment = true;
			AssertEquals("PaymentCode", "G", b3Header.PaymentCode);

			jobDeclaration.ImporterAddInfo.ZO_IsImporterDirectPayment = false;
			jobDeclaration.ImporterAddInfo.ZO_IsLVSImporterDirectPayment = true;
			jobDeclaration.ImporterAddInfo.ZO_IsGSTDirectPayment = true;
			AssertEquals("PaymentCode", "I", b3Header.PaymentCode);

			jobDeclaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			jobDeclaration.ImporterAddInfo.ZO_IsImporterDirectPayment = true;
			jobDeclaration.ImporterAddInfo.ZO_IsLVSImporterDirectPayment = true;
			jobDeclaration.ImporterAddInfo.ZO_IsGSTDirectPayment = true;
			AssertEquals("PaymentCode", ZString.Empty, b3Header.PaymentCode);
		}

		void AssertBusinessNumber(IB3Header b3Header)
		{
			AssertEquals("BusinessNumber from broker home branch", "4561", b3Header.BusinessNumber);
			B3ImportMessageWrapperTest.DeleteCusCode(OrgCusCode.CACodeTypes.BusinessNumberForLowValueShipments, declaration.CusAgent.HomeBranch.OrgProxy);
			AssertEquals("BusinessNumber from broker company", "7894", b3Header.BusinessNumber);
			declaration.CusAgent.GS_GB_HomeBranch = ZGuid.Empty;
			AssertEquals("BusinessNumber from broker without branch", string.Empty, b3Header.BusinessNumber);

			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			AssertEquals("BusinessNumber from importer BRM number", "3021", b3Header.BusinessNumber);
			B3ImportMessageWrapperTest.DeleteCusCode(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, declaration.Importer);
			AssertEquals("BusinessNumber from importer CAI number", "1234", b3Header.BusinessNumber);

			B3ImportMessageWrapperTest.DeleteCusCode(OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial, declaration.Importer);
			declaration.JE_GS_NKCusAgent = ZString.Empty;
			AssertEquals("BusinessNumber from current branch", "6543", b3Header.BusinessNumber);
			B3ImportMessageWrapperTest.DeleteCusCode(OrgCusCode.CACodeTypes.BusinessNumberForLowValueShipments, GlbBranch.CurrentBranch.OrgProxy);
			AssertEquals("BusinessNumber from current company", "9874", b3Header.BusinessNumber);
		}

		#endregion

		#region TestIB3SubHeadersProperties

		public void TestIB3SubHeadersProperties()
		{
			IB3Header b3Header = new LowValueShipmentsMessageWrapper(entryHeader);
			Assert("Pre-condition: TotalValueForDuty greater than 2500", b3Header.TotalValueForDuty > JobComInvoiceHeader.VFDLimit);

			var subHeaders = b3Header.PositiveB3SubHeaders.ToList();
			AssertEquals("PositiveB3SubHeaders Count", 2, subHeaders.Count);

			//Sub Header 1
			var expectedB3SubHeader =
				new ExpectedB3SubHeaderForTesting
				{
					B3SubHeaderNumber = 1,
					Vendor = new DocAddressWrapper("Various")
					{
						CountryCode = "US",
						E2_State = "NY",
						E2_Postcode = "55555"
					},
					CountryOfOrigin = "UNY",
					PlaceOfExport = "UNY",
					TariffTreatmentCode = TariffTreatmentCodes.Codes.MostFavouredNation,
					USPortOfExit = "1001",
					DateOfDirectShipment = ZDateTime.Empty,
					CurrencyCode = Constants.CurrencyCodes.Canada,
					TimeLimitUnit = TimeLimitUnitCodes.Codes.Day,
					B3TimeLimits = 2,
					FreightCharges = 1m,
					InvoiceNumber = ZString.Empty,
					ExchangeRate = ZDecimal.Zero,
					TradeZone = "01"
				};

			B3ImportMessageWrapperTest.AssertB3SubHeader(expectedB3SubHeader, subHeaders[0]);

			//Sub Header 2
			expectedB3SubHeader.B3SubHeaderNumber = 2;
			expectedB3SubHeader.CountryOfOrigin = Constants.CountryCodes.Canada;
			expectedB3SubHeader.PlaceOfExport = "UWY";
			expectedB3SubHeader.TariffTreatmentCode = TariffTreatmentCodes.Codes.Iceland;

			B3ImportMessageWrapperTest.AssertB3SubHeader(expectedB3SubHeader, subHeaders[1]);

			entryHeader.AllEntryLines[0].CL_CustomsValue = 0;
			entryHeader.ResetTotalsAndCachedValues();
			b3Header = new LowValueShipmentsMessageWrapper(entryHeader);
			subHeaders = b3Header.PositiveB3SubHeaders.ToList();

			expectedB3SubHeader.Vendor = new DocAddressWrapper("Various")
			{
				CountryCode = "US",
				E2_State = "NY",
				E2_Postcode = "55555"
			};

			expectedB3SubHeader.USPortOfExit = "1001";
			expectedB3SubHeader.FreightCharges = 1m;
			B3ImportMessageWrapperTest.AssertB3SubHeader(expectedB3SubHeader, subHeaders[1]);
		}

		#endregion

		#region TestIClassificationLine1Properties

		public void TestIClassificationLine1Properties()
		{
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			declaration.DoMerge();

			IB3Header b3Header = new LowValueShipmentsMessageWrapper(entryHeader);
			AssertEquals("PositiveClassificationLines count", 5, b3Header.PositiveClassificationLines.Count());
			AssertSubHeaderAndLineNumbers(b3Header.PositiveClassificationLines.ElementAt(0), 1, 1, 1);
			AssertSubHeaderAndLineNumbers(b3Header.PositiveClassificationLines.ElementAt(1), 1, 2, 1);
			AssertSubHeaderAndLineNumbers(b3Header.PositiveClassificationLines.ElementAt(2), 1, 3, 1);
			AssertSubHeaderAndLineNumbers(b3Header.PositiveClassificationLines.ElementAt(3), 1, 4, 1);
			AssertSubHeaderAndLineNumbers(b3Header.PositiveClassificationLines.ElementAt(4), 2, 5, 1);

			//OIC Line
			var entryLine = entryHeader.AllEntryLines[1];
			var classLine1 = b3Header.PositiveClassificationLines.ElementAt(2);
			AssertEquals("RecordIdentifier", MessageConstants.B3RecordIdentifiers.Positive, classLine1.RecordIdentifier);
			AssertEquals("ClassificationNumber", "8466939090", classLine1.ClassificationNumber);
			AssertEquals("TariffCode", "9960", classLine1.TariffCode);
			AssertEquals("PartNumberDescriptions", entryLine.RandomLine.JI_Description, classLine1.PartNumberDescriptions[0]);
			AssertEquals("AuthorityNumber", "134649987", classLine1.AuthorityNumber);
			AssertEquals("TRSNumber", ZString.Empty, classLine1.TRSNumber);

			AssertEquals("ValueForDutyCode", "13", classLine1.ValueForDutyCode);
			AssertEquals("ValueForCurrency", 400m, classLine1.ValueForCurrency);
			AssertEquals("ValueForDuty", 400m, classLine1.ValueForDuty);
			AssertEquals("ValueForTax", 1120.00m, classLine1.ValueForTax);

			AssertEquals("SIMACode", SIMACodes.Codes.C31, classLine1.SIMACode);
			AssertEquals("SIMAStatementCode", "S", classLine1.SIMAStatementCode);
			AssertEquals("SIMAAssessment", 46m, classLine1.SIMAAssessment);
			entryLine.Fees.SetAmount(EntryChargeTypeList.Codes.TotalSIMAAmount, 0);
			AssertEquals("SIMACode", SIMACodes.Codes.C31, classLine1.SIMACode);
			AssertEquals("SIMAStatementCode", "S", classLine1.SIMAStatementCode);
			AssertEquals("SIMAAssessment", 46m, classLine1.SIMAAssessment);

			AssertEquals("InvoiceCrossReferences count", 1, classLine1.InvoiceCrossReferences.Count());
			var crossReference = classLine1.InvoiceCrossReferences.First();
			AssertEquals("InvoiceLineNumber", 1, crossReference.InvoiceLineNumber);
			AssertEquals("InvoicePageNumber", 1, crossReference.InvoicePageNumber);
			AssertEquals("InvoiceValue", 0.01m, crossReference.InvoiceValue);

			AssertEquals("ExciseExemptionCode", ZString.Empty, classLine1.ExciseExemptionCode);
			AssertEquals("ExciseTaxRate", 0m, classLine1.ExciseTaxRate);
			AssertEquals("ExciseTaxRateToPrint", 0m, classLine1.ExciseTaxRateToPrint);
			AssertEquals("ExciseTaxAmount", 0m, classLine1.ExciseTaxAmount);
			AssertEquals("IsDummyExciseTaxRate", false, classLine1.IsDummyExciseTaxRate);

			AssertEquals("GSTExemptionCode", ZString.Empty, classLine1.GSTExemptionCode);
			AssertEquals("RateOfGST", 16m, classLine1.RateOfGST);
			AssertEquals("GSTAmount", 179.20m, classLine1.GSTAmount);

			AssertEquals("CustomsQuantity", 41m, classLine1.CustomsQuantity);
			AssertEquals("CustomsUnitQty", CustomsUnitOfMeasureList.Codes.Litre, classLine1.CustomsUnitQty);
			AssertEquals("InvoiceQuantity", 55m, classLine1.InvoiceQuantity);
			AssertEquals("InvoiceUQ", CustomsUnitOfMeasureList.Codes.Tube, classLine1.InvoiceUQ);
			AssertEquals("TotalLinePrice", 320m, classLine1.TotalLinePrice.Amount);
			AssertEquals("CustomsValue", 400m, classLine1.CustomsValue.Amount);
			AssertEquals("FOB.Amount", 320m, classLine1.FOB.Amount);
			AssertEquals("FOB.Currency", "USD", classLine1.FOB.Currency.Code);

			Assert("Calculations done", b3Header.IsCalculationsDone);

			classLine1 = b3Header.PositiveClassificationLines.ElementAt(2);
			declaration.ResumeApportionment();
			AssertEquals("AuthorityNumber", "134649987", classLine1.AuthorityNumber);
			AssertEquals("ExciseExemptionCode", ZString.Empty, classLine1.ExciseExemptionCode);
			AssertEquals("ExciseTaxRate", 0m, classLine1.ExciseTaxRate);
			AssertEquals("ExciseTaxRateToPrint", 0m, classLine1.ExciseTaxRateToPrint);
			AssertEquals("ExciseTaxAmount", 0m, classLine1.ExciseTaxAmount);
			AssertEquals("IsDummyExciseTaxRate", false, classLine1.IsDummyExciseTaxRate);

			entryLine.RandomLine.DutiesAndTaxes.Find(tax => tax.C1_TaxType == DutyAndTaxTypes.Codes.GST).First().Delete();
			Assert("Calculations not done", !b3Header.IsCalculationsDone);
		}

		static void AssertSubHeaderAndLineNumbers(IClassificationLine1 classLine, int subHeaderNum, int num, int consolidatedLines)
		{
			AssertEquals("B3SubHeaderNumber", subHeaderNum, classLine.B3SubHeaderNumber);
			AssertEquals("B3LineNumber", num, classLine.B3LineNumber);
			AssertEquals("CountOfConsolidatedLines", consolidatedLines, classLine.CountOfConsolidatedLines);
		}

		public void TestIClassificationLine1Properties_DutiesAndTaxes()
		{
			#region Create Test Data
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			var line1 = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			var line2 = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			line1.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine);
			line2.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine);
			Factory.Save();
			var cpt1 = line1.DutiesAndTaxes.AddNew();
			cpt1.C1_Override = true;
			cpt1.C1_TaxType = DutyAndTaxTypes.Codes.CPT;
			cpt1.C1_Amount = 33m;
			var cta1 = line1.DutiesAndTaxes.AddNew();
			cta1.C1_Override = true;
			cta1.C1_TaxType = DutyAndTaxTypes.Codes.CTA;
			cta1.C1_Amount = 34m;
			var dty1 = line1.DutiesAndTaxes.AddNew();
			dty1.C1_Override = true;
			dty1.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			dty1.C1_Amount = 35m;
			dty1.C1_Code = "AB";
			var exs1 = line1.DutiesAndTaxes.AddNew();
			exs1.C1_Override = true;
			exs1.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			exs1.C1_Amount = 36m;
			exs1.C1_Code = "BC";
			var sur1 = line1.DutiesAndTaxes.AddNew();
			sur1.C1_Override = true;
			sur1.C1_TaxType = DutyAndTaxTypes.Codes.SUR;
			sur1.C1_Amount = 37m;
			sur1.Quantity = 38m;
			sur1.C1_UnitOfMeasure = "M3";
			sur1.C1_Code = "CD";
			sur1.C1_ExemptCode = SIMACodes.Codes.C20;
			var add1 = line1.DutiesAndTaxes.AddNew();
			add1.C1_Override = true;
			add1.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			add1.C1_Amount = 39m;
			add1.Quantity = 40m;
			add1.C1_UnitOfMeasure = "M4";
			add1.C1_Code = "DE";
			var cvd1 = line1.DutiesAndTaxes.AddNew();
			cvd1.C1_Override = true;
			cvd1.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			cvd1.C1_Amount = 43;
			cvd1.Quantity = 44;
			cvd1.C1_UnitOfMeasure = "M5";
			cvd1.C1_Code = "EF";
			var saf1 = line1.DutiesAndTaxes.AddNew();
			saf1.C1_Override = true;
			saf1.C1_TaxType = DutyAndTaxTypes.Codes.SAF;
			saf1.C1_Amount = 45m;
			saf1.C1_Code = "FG";
			saf1.C1_ExemptCode = SIMACodes.Codes.C20;
			var ded1 = line1.Charges.AddNew();
			ded1.J7_ChargeType = Customs.Business.CustomsChargeTypeList.Codes.DeductionCharge;
			ded1.J7_Amount = 46m;
			ded1.J7_RX_NKCurrency = "USD";
			var exd1 = line1.DutiesAndTaxes.AddNew();
			exd1.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			exd1.C1_Amount = 47m;
			exd1.C1_DutyType = DutyAndTaxManager.CombinedDuty.Excise;
			var cpt2 = line2.DutiesAndTaxes.AddNew();
			cpt2.C1_Override = true;
			cpt2.C1_TaxType = DutyAndTaxTypes.Codes.CPT;
			cpt2.C1_Amount = 33m;
			var cta2 = line2.DutiesAndTaxes.AddNew();
			cta2.C1_Override = true;
			cta2.C1_TaxType = DutyAndTaxTypes.Codes.CTA;
			cta2.C1_Amount = 34m;
			var dty2 = line2.DutiesAndTaxes.AddNew();
			dty2.C1_Override = true;
			dty2.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			dty2.C1_Amount = 35m;
			dty2.C1_Code = "AB";
			var exs2 = line2.DutiesAndTaxes.AddNew();
			exs2.C1_Override = true;
			exs2.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			exs2.C1_Amount = 36m;
			exs2.C1_Code = "BC";
			var sur2 = line2.DutiesAndTaxes.AddNew();
			sur2.C1_Override = true;
			sur2.C1_TaxType = DutyAndTaxTypes.Codes.SUR;
			sur2.C1_Amount = 37m;
			sur2.Quantity = 38m;
			sur2.C1_UnitOfMeasure = "M3";
			sur2.C1_Code = "CD";
			var add2 = line2.DutiesAndTaxes.AddNew();
			add2.C1_Override = true;
			add2.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			add2.C1_Amount = 39m;
			add2.Quantity = 40m;
			add2.C1_UnitOfMeasure = "M4";
			add2.C1_Code = "DE";
			var cvd2 = line2.DutiesAndTaxes.AddNew();
			cvd2.C1_Override = true;
			cvd2.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			cvd2.C1_Amount = 43;
			cvd2.Quantity = 44;
			cvd2.C1_UnitOfMeasure = "M5";
			cvd2.C1_Code = "EF";
			var saf2 = line2.DutiesAndTaxes.AddNew();
			saf2.C1_Override = true;
			saf2.C1_TaxType = DutyAndTaxTypes.Codes.SAF;
			saf2.C1_Amount = 45m;
			saf2.C1_Code = "FG";
			saf2.C1_ExemptCode = SIMACodes.Codes.C20;
			var ded2 = line2.Charges.AddNew();
			ded2.J7_ChargeType = Customs.Business.CustomsChargeTypeList.Codes.DeductionCharge;
			ded2.J7_Amount = 46m;
			ded2.J7_RX_NKCurrency = "USD";
			var exd2 = line2.DutiesAndTaxes.AddNew();
			exd2.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			exd2.C1_Amount = 47m;
			exd2.C1_DutyType = DutyAndTaxManager.CombinedDuty.Excise;
			#endregion

			var wrapper = new LowValueShipmentsMessageWrapper(entryHeader);
			var classificationLine1 = entryLine as IClassificationLine1;
			AssertEquals("SalesTaxAmount", 66m, classificationLine1.SalesTaxAmount);
			AssertEquals("CTAAmount", 68m, classificationLine1.CTAAmount);
			AssertEquals("DeductionChargeAmountAndCurrency.Amount", 92m, classificationLine1.DeductionChargeAmountAndCurrency.Amount);
			AssertEquals("DeductionChargeAmountAndCurrency.Currency.RX_Code", "USD", classificationLine1.DeductionChargeAmountAndCurrency.Currency.RX_Code);
			AssertEquals("CustomsDutyCode", "AB", classificationLine1.CustomsDutyCode);
			AssertEquals("ExciseCode", "BC", classificationLine1.ExciseCode);
			AssertEquals("SurtaxQuantity", 76m, classificationLine1.SurtaxQuantity);
			AssertEquals("SurtaxUnitOfMeasure", "M3", classificationLine1.SurtaxUnitOfMeasure);
			AssertEquals("SurtaxCode", "CD", classificationLine1.SurtaxCode);
			AssertEquals("SurtaxStatementCode", "U", classificationLine1.SurtaxStatementCode);
			AssertEquals("HasSurtax", true, classificationLine1.HasSurtax);
			AssertEquals("ADDAmount", 78m, classificationLine1.ADDAmount);
			AssertEquals("ADDQuantity", 80m, classificationLine1.ADDQuantity);
			AssertEquals("ADDUnitOfMeasure", "M4", classificationLine1.ADDUnitOfMeasure);
			AssertEquals("ADDCode", "DE", classificationLine1.ADDCode);
			AssertEquals("ADDIsOverride", true, classificationLine1.ADDIsOverride);
			AssertEquals("HasADD", true, classificationLine1.HasADD);
			AssertEquals("CVDAmount", 86m, classificationLine1.CVDAmount);
			AssertEquals("CVDQuantity", 88m, classificationLine1.CVDQuantity);
			AssertEquals("CVDUnitOfMeasure", "M5", classificationLine1.CVDUnitOfMeasure);
			AssertEquals("CVDCode", "EF", classificationLine1.CVDCode);
			AssertEquals("CVDIsOverride", true, classificationLine1.CVDIsOverride);
			AssertEquals("HasCVD", true, classificationLine1.HasCVD);
			AssertEquals("SafeguardCode", "FG", classificationLine1.SafeguardCode);
			AssertEquals("HasSafeguard", true, classificationLine1.HasSafeguard);
			AssertEquals("SafeguardStatementCode", "U", classificationLine1.SafeguardStatementCode);
			AssertEquals("ExciseDutyAmount", 94m, classificationLine1.ExciseDutyAmount);
		}

		#endregion

		#region TestIClassificationLine2Properties

		public void TestIClassificationLine2Properties()
		{
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			declaration.DoMerge();

			IB3Header b3Header = new LowValueShipmentsMessageWrapper(entryHeader);

			//OIC Line
			var classLine1 = b3Header.PositiveClassificationLines.ElementAt(1);
			var classLine2 = classLine1.ClassificationLines.ElementAt(1);
			AssertEquals("ClassificationLines count", 3, classLine1.ClassificationLines.Count());
			AssertEquals("B3LineNumber", classLine1.B3LineNumber, classLine2.B3LineNumber);

			entryHeader.IsB3GrossWeightSet = true;
			AssertEquals("WeightInKGM", 1m, classLine1.ClassificationLines.First().WeightInKGM);
			entryHeader.IsB3GrossWeightSet = true;
			AssertEquals("WeightInKGM", 0m, classLine2.WeightInKGM);

			AssertEquals("CustomsDutyRate", 1m, classLine2.CustomsDutyRate);
			AssertEquals("CustomsDutyAmount", 322m, classLine2.CustomsDutyAmount);
			AssertEquals("PreviousTransactionNumber", string.Empty, classLine2.PreviousTransactionNumber);
			AssertEquals("PreviousLineNumber", 0, classLine2.PreviousLineNumber);
			AssertEquals("ClassificationLineQuantity", 1m, classLine2.ClassificationLineQuantity);
			AssertEquals("UnitOfMeasureCode", CustomsUnitOfMeasureList.Codes.Number, classLine2.UnitOfMeasureCode);

			////Dummy data for consolidated lines
			foreach (ICodeDescription subType in new LowValueShipmentsTypes())
			{
				declaration.JE_MessageSubType = subType.Code;
				var classLine3 = b3Header.PositiveClassificationLines.First();
				var classLine4 = classLine3.ClassificationLines.ElementAt(1);
				AssertEquals("CustomsDutyRate", 1.0m, classLine4.CustomsDutyRate);
				AssertEquals("ClassificationLineQuantity", 1m, classLine4.ClassificationLineQuantity);
				AssertEquals("UnitOfMeasureCode", CustomsUnitOfMeasureList.Codes.Number, classLine4.UnitOfMeasureCode);
			}
		}

		#endregion

		#region IB3HeaderWithScheduledMessageSupport

		public void TestCleanScheduledB3Messages()
		{
			var testMessage1 = entryHeader.Messages.AddNew();
			testMessage1.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			testMessage1.EM_Status = EDIMessage.Status.Queued;
			testMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			var testMessage2 = entryHeader.Messages.AddNew();
			testMessage2.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			testMessage2.EM_Status = EDIMessage.Status.Queued;
			testMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			testMessage2.EM_HeldUntilDate = ZDateTime.UtcNow;

			var testMessage3 = entryHeader.Messages.AddNew();
			testMessage3.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			testMessage3.EM_Status = EDIMessage.Status.Sent;
			testMessage3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			testMessage3.EM_HeldUntilDate = ZDateTime.UtcNow;

			var testMessage4 = entryHeader.Messages.AddNew();
			testMessage4.EM_MessageType = MessageTypeList.Codes.EDIRelease;
			testMessage4.EM_Status = EDIMessage.Status.Queued;
			testMessage4.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			testMessage4.EM_HeldUntilDate = ZDateTime.UtcNow;

			IB3HeaderWithScheduledMessageSupport b3Header = new B3ImportMessageWrapper(entryHeader);
			b3Header.CancelAndDeactivateScheduledB3Message();

			CombineAssertions(() =>
			{
				AssertEquals("ActiveTest_1", true, testMessage1.EM_IsActive);
				AssertEquals("StatusTest_1", EDIMessage.Status.Queued, testMessage1.EM_Status);
				AssertEquals("ActiveTest_2", false, testMessage2.EM_IsActive);
				AssertEquals("StatusTest_2", EDIMessage.Status.Cancelled, testMessage2.EM_Status);
				AssertEquals("ActiveTest_3", true, testMessage3.EM_IsActive);
				AssertEquals("StatusTest_3", EDIMessage.Status.Sent, testMessage3.EM_Status);
				AssertEquals("ActiveTest_4", true, testMessage4.EM_IsActive);
				AssertEquals("StatusTest_4", EDIMessage.Status.Queued, testMessage4.EM_Status);
			});
		}

		public void TestPopulateEntrySubmittedDateIfRequiredWithParam()
		{
			var declaration = this.declaration;
			var entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);

			// with not set declaration
			var submittedDateTime1 = ZDateTime.Now.AddMinutes(-15);
			entryHeader.CH_EntrySubmittedDate = ZDateTime.Empty;
			declaration.JE_EntrySubmittedDate = ZDateTime.Empty;
			var wrapper = new LowValueShipmentsMessageWrapper(entryHeader);
			wrapper.PopulateEntrySubmittedDateIfRequired(submittedDateTime1);
			AssertEquals("CH_EntrySubmittedDate is set to ZDateTime.Now", true, entryHeader.CH_EntrySubmittedDate == submittedDateTime1);
			AssertEquals("JE_EntrySubmittedDate is set to ZDateTime.Now", true, declaration.JE_EntrySubmittedDate == entryHeader.CH_EntrySubmittedDate);

			// with already set declaration
			var submittedDateTime2 = ZDateTime.Now.AddMinutes(-30);
			entryHeader.CH_EntrySubmittedDate = ZDateTime.Empty;
			var declarationTime = ZDateTime.Now.AddMinutes(-45);
			declaration.JE_EntrySubmittedDate = declarationTime;
			wrapper = new LowValueShipmentsMessageWrapper(entryHeader);
			wrapper.PopulateEntrySubmittedDateIfRequired(submittedDateTime2);
			AssertEquals("CH_EntrySubmittedDate is set to ZDateTime.Now", true, entryHeader.CH_EntrySubmittedDate == submittedDateTime2);
			AssertEquals("JE_EntrySubmittedDate is unchanged", true, declaration.JE_EntrySubmittedDate == declarationTime);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			CACustomsDataRegistry.Instance.DefaultExciseTaxFromCustomsTariff.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var factory = JobComInvoiceLineTestHelper.PopulateDutiesAndTaxesRefFilesReturningFactory();
			var helper = new DeclarationTestHelper(factory, true);
			var canada = factory.Load<RefCountry>(Constants.CountryGuids.Canada);
			declaration = factory.New<JobDeclaration>();
			declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.CustomsMessageToPrintOnB3.Description, "Note to be printed on B3");
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Now;
			declaration.WarehouseDocAddress.OrganisationPK = helper.CreateOrganisation("WAREHOUSE NAME", "CATOR").PK;
			declaration.WarehouseDocAddress.Organisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "12345", canada);
			var importer = helper.CreateOrganisation("IMP", "IMPORTER NAME", "CATOR", "IMPORTER ADDRESS", "IMPORTER CITY", "123 4567");
			importer.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = ZString.Empty;
			((OrgImpAddInfo)importer.CountryData.ImpAddInfo).ZO_IsImporterDirectPayment = true;
			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "3021", canada);
			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial, "1234", canada);
			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForGoodsServicesHarmonizedSalesTax, "0987654321", canada);
			declaration.JE_OH_Importer = importer.PK;
			var brokerCompany = factory.New<GlbCompany>();
			brokerCompany.GC_OH_OrgProxy = helper.CreateOrganisation("BROKER COMPANY ORG", ZString.Empty).PK;
			brokerCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForLowValueShipments, "7894", canada);
			var brokerBranch = brokerCompany.Branches.AddNew();
			brokerBranch.GB_OH_OrgProxy = helper.CreateOrganisation("BROKER BRANCH ORG", ZString.Empty).PK;
			brokerBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForLowValueShipments, "4561", canada);
			broker = factory.New<GlbStaff>();
			broker.GS_Code = "BRK";
			broker.GS_GB_HomeBranch = brokerBranch.PK;
			declaration.JE_GS_NKCusAgent = broker.GS_Code;
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForLowValueShipments, "9874", canada);
			var query = new ZQuery(ZArchitecture.Schema.OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = new BusinessObjectFactory().LoadTop1<OrgHeader>(query).PK; //Some company from db
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForLowValueShipments, "6543", canada);

			declaration.JE_CustomsOffice = "351";
			declaration.CA_UnladingOffice = "423";
			declaration.JE_CarrierCode = "2121";

			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "00006789";

			//Invoice 1
			invoice = declaration.Invoices.AddNew();
			helper.USD.SetCustomsRate(ZDateTime.Today, ZDateTime.Today, 1.25);
			invoice.JZ_RX_NKInvoice_Currency = helper.USD.RX_Code;
			invoice.CA_TimeLimit = 2;
			invoice.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Day;
			invoice.CA_TradeZone = "01";

			//Invoice Lines
			var line = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(line, 1, 1, 80, 11, 12, 13, Constants.Weight.Kilograms, 16);
			JobComInvoiceLineTestHelper.AddTariffRecord(line);
			line.CA_AuthorityNumber = ZString.Empty;

			line = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(line, 2, 1, 160, 21, 22, 23, Constants.Weight.Tonnes, 26);
			line.CA_AuthorityNumber = ZString.Empty;

			oicLine = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(oicLine, 3, 1, 320, 41, 42, 43, Constants.Weight.Kilograms, 46);
			oicLine.CA_AuthorityNumber = "134649987";
			oicLine.JI_InvoiceQuantity = 55;
			oicLine.JI_InvoiceUQ = CustomsUnitOfMeasureList.Codes.Tube;

			line = invoice.JobComInvoiceLines.AddNew();
			//TODO: Make this line negative.
			//There is definitely bug with negative amounts:
			//We decide if CVforCurrConv < 0 then class line NEG else POS.
			//But if we make CV < 0 then all V rates will be negative but S - positive and AllTotal will be rubbish.
			//I think that it should be some check box to indicate class line is POS or NEG. Leon will check.
			JobComInvoiceLineTestHelper.FillInvoiceLine(line, 4, 1, 640, 81, 82, 83, Constants.Weight.Kilograms);
			line.CA_AuthorityNumber = ZString.Empty;
			line.CA_TreatmentCode = TariffTreatmentCodes.Codes.Iceland;
			line.JI_CountryOfOrigin = Constants.CountryCodes.Canada;
			line.CA_RN_NKExport = Constants.CountryCodes.UnitedStates;
			line.CA_USStateOfExport = USStatesList.Codes.Wyoming;

			line = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(line, 5, 1, 1280, 161, 162, 163, Constants.Weight.Kilograms, 166);
			line.CA_AuthorityNumber = ZString.Empty;
			line.JI_Tariff = "0403204599";
			line.JI_CustomsQuantity = 161;
			line.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Litre;
			JobComInvoiceLineTestHelper.AddTariffRecord(line);
			declaration.ResumeApportionment();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			entryHeader = declaration.CustomsEntryHeaders[0];
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		JobComInvoiceHeader invoice;
		GlbStaff broker;
		JobComInvoiceLine oicLine;

		#endregion
	}
}
