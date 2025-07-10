using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.CA.Business.MessageBuilders.Testing
{
	sealed class B3ImportMessageWrapperTest : TestCaseWithFactory
	{
		public void TestSetB3SubHeaderNumbers()
		{
			AssertEquals(true, declaration.InvoiceLines.Cast<JobComInvoiceLine>().All(x => x.B3SubHeaderNumberForLVX.IsEmpty));

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, false))
			{
				var lvx = Factory.New<JobDeclaration>();
				lvx.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
				var lvxInvoice = lvx.Invoices.AddNew();
				var lvxInvoiceLine01 = lvxInvoice.JobComInvoiceLines.AddNew();
				var lvxInvoiceLine02 = lvxInvoice.JobComInvoiceLines.AddNew();
				AssertEquals(true, lvx.InvoiceLines.Cast<JobComInvoiceLine>().All(x => x.B3SubHeaderNumberForLVX.IsEmpty));

				lvx.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				lvx.DoMerge();
				var lvxEntry = lvx.B3EntryHeader;
				var lvxWrapper = new B3ImportMessageWrapper(lvxEntry);
				AssertEquals(false, lvx.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.B3SubHeaderNumberForLVX.IsEmpty));

				var num = lvxInvoiceLine01.B3SubHeaderNumberForLVX;
				lvxInvoiceLine01.B3SubHeaderNumberForLVX = num + 1;
				lvxWrapper = new B3ImportMessageWrapper(lvxEntry);
				AssertEquals(num, lvxInvoiceLine01.B3SubHeaderNumberForLVX);

				lvxInvoiceLine01.B3SubHeaderNumberForLVX = 0;
				lvxWrapper = new B3ImportMessageWrapper(lvxEntry);
				AssertEquals(num, lvxInvoiceLine01.B3SubHeaderNumberForLVX);

				var lvxInvoiceLine03 = lvxInvoice.JobComInvoiceLines.AddNew();
				lvx.Factory.Save();
				lvxWrapper = new B3ImportMessageWrapper(lvxEntry);
				AssertEquals(false, lvx.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.B3SubHeaderNumberForLVX.IsEmpty));
			}
		}

		public void TestRefreshCachedValues()
		{
			IB3Header b3Header = new B3ImportMessageWrapper(entryHeader);
			AssertEquals("B3BInputReleases Count", 2, b3Header.B3BInputReleases.Count());

			var b3PosSubHeaders = new List<IB3SubHeader>(b3Header.PositiveB3SubHeaders);
			AssertEquals("PositiveB3SubHeaders count", 2, b3PosSubHeaders.Count);

			var number1 = declaration.AdditionalReferenceNumbers.AddNew();
			number1.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number1.CE_EntryNum = "1111111";

			AssertEquals("B3BInputReleases Count", 2, b3Header.B3BInputReleases.Count());

			var posClassificationLines = new List<IClassificationLine1>(b3Header.PositiveClassificationLines);
			AssertEquals("PositiveClassificationLines count", 3, posClassificationLines.Count);

			var newline = declaration.InvoiceLines.AddNew();
			newline.CA_B3SubHeaderNumber = 99;
			var newEntry = entryHeader.AllEntryLines.AddNew();
			newEntry.CL_LineNumber = 99;
			b3PosSubHeaders = new List<IB3SubHeader>(b3Header.PositiveB3SubHeaders);
			AssertEquals("PositiveB3SubHeaders count", 2, b3PosSubHeaders.Count);

			posClassificationLines = new List<IClassificationLine1>(b3Header.PositiveClassificationLines);
			AssertEquals("PositiveClassificationLines count", 3, posClassificationLines.Count);

			b3Header.RefreshCachedValues();

			AssertEquals("B3BInputReleases Count", 3, b3Header.B3BInputReleases.Count());

			b3PosSubHeaders = new List<IB3SubHeader>(b3Header.PositiveB3SubHeaders);
			AssertEquals("PositiveB3SubHeaders count", 3, b3PosSubHeaders.Count);

			posClassificationLines = new List<IClassificationLine1>(b3Header.PositiveClassificationLines);
			AssertEquals("PositiveClassificationLines count", 4, posClassificationLines.Count);
		}

		public void TestPopulateEntrySubmittedDateIfRequiredWithParam()
		{
			var declaration = this.declaration;
			var entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);

			// with not set declaration
			var submittedDateTime1 = ZDateTime.Now.AddMinutes(-15);
			entryHeader.CH_EntrySubmittedDate = ZDateTime.Empty;
			declaration.JE_EntrySubmittedDate = ZDateTime.Empty;
			var wrapper = new B3ImportMessageWrapper(entryHeader);
			wrapper.PopulateEntrySubmittedDateIfRequired(submittedDateTime1);
			AssertEquals("CH_EntrySubmittedDate is set to ZDateTime.Now", true, entryHeader.CH_EntrySubmittedDate == submittedDateTime1);
			AssertEquals("JE_EntrySubmittedDate is set to ZDateTime.Now", true, declaration.JE_EntrySubmittedDate == entryHeader.CH_EntrySubmittedDate);

			// with already set declaration
			var submittedDateTime2 = ZDateTime.Now.AddMinutes(-30);
			entryHeader.CH_EntrySubmittedDate = ZDateTime.Empty;
			var declarationTime = ZDateTime.Now.AddMinutes(-45);
			declaration.JE_EntrySubmittedDate = declarationTime;
			wrapper = new B3ImportMessageWrapper(entryHeader);
			wrapper.PopulateEntrySubmittedDateIfRequired(submittedDateTime2);
			AssertEquals("CH_EntrySubmittedDate is set to ZDateTime.Now", true, entryHeader.CH_EntrySubmittedDate == submittedDateTime2);
			AssertEquals("JE_EntrySubmittedDate is unchanged", true, declaration.JE_EntrySubmittedDate == declarationTime);
		}

		#region TestIB3HeaderProperties

		[ExpectNoExceptions]
		public void TestNoExceptionsThrownIfInvalidPorts()
		{
			declaration.JE_CustomsOffice = "AAA";
			declaration.CA_UnladingOffice = "BBB";
			var messageBuilder = new B3CusdecMessageBuilder<B3Message>(new B3ImportMessageWrapper(entryHeader), MessageSubTypes.Create);
			messageBuilder.PopulateMessages();
		}

		public void TestIB3HeaderProperties()
		{
			IB3Header b3Header = new B3ImportMessageWrapper(entryHeader);
			AssertEquals("BatchNumber", "<<UNIQUE BATCH NUMBER PLACE HOLDER>>", b3Header.BatchNumber);
			AssertEquals("B3TypeCode", declaration.JE_MessageSubType, b3Header.B3TypeCode);

			AssertEquals("PaymentCode", "I", b3Header.PaymentCode);
			declaration.ImporterAddInfo.ZO_IsImporterDirectPayment = false;
			AssertEquals("PaymentCode", string.Empty, b3Header.PaymentCode);
			AssertEquals("CBSAOffice", "351", b3Header.CBSAOffice);
			AssertEquals("PortOfUnlading", "423", b3Header.PortOfUnlading);
			AssertEquals("WarehouseNumber", ZString.Empty, b3Header.WarehouseNumber);
			AssertEquals("AccountSecurityCode", "12345", b3Header.AccountSecurityCode);
			AssertEquals("TransactionNumber", "000067897", b3Header.TransactionNumber);

			AssertEquals("BusinessNumber", "3021", b3Header.BusinessNumber);
			DeleteCusCode(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, declaration.Importer);
			b3Header = new B3ImportMessageWrapper(entryHeader);
			AssertEquals("BusinessNumber", "1234", b3Header.BusinessNumber);
			AssertEquals("Importer", "IMPORTER NAME", b3Header.Importer.E2_CompanyName);

			AssertEquals("GSTNumber", "0987654321", b3Header.GSTNumber);
			DeleteCusCode(OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial, declaration.Importer);
			b3Header = new B3ImportMessageWrapper(entryHeader);
			AssertEquals("BusinessNumber", string.Empty, b3Header.BusinessNumber);
			AssertEquals("GSTNumber", "0987654321", b3Header.GSTNumber);

			AssertEquals("TransportMode", "9", b3Header.TransportMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.NoCarrier;
			AssertEquals("TransportMode", "8", b3Header.TransportMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			AssertEquals("TransportMode", "7", b3Header.TransportMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			AssertEquals("TransportMode", "6", b3Header.TransportMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			AssertEquals("TransportMode", "2", b3Header.TransportMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("TransportMode", "1", b3Header.TransportMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			AssertEquals("TransportMode", "5", b3Header.TransportMode);

			AssertEquals("B3BInputReleases Count", 2, b3Header.B3BInputReleases.Count());
			var release1 = b3Header.B3BInputReleases.First();
			var release2 = b3Header.B3BInputReleases.ElementAt(1);
			AssertEquals("CargoControlNumber", "212112345678987654321", release1.CargoControlNumber);
			AssertEquals("DateOfRelease", declaration.JE_EntryAuthorisationDate, release1.DateOfRelease);
			AssertEquals("CargoControlNumber", "123456789321566549877", release2.CargoControlNumber);
			AssertEquals("DateOfRelease", ZDateTime.Empty, release2.DateOfRelease);

			var number1 = declaration.AdditionalReferenceNumbers.AddNew();
			number1.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number1.CE_EntryNum = "1111111";
			b3Header = new B3ImportMessageWrapper(entryHeader);

			AssertEquals("B3BInputReleases Count", 3, b3Header.B3BInputReleases.Count());
			release1 = b3Header.B3BInputReleases.First();
			release2 = b3Header.B3BInputReleases.ElementAt(1);
			var release3 = b3Header.B3BInputReleases.ElementAt(2);
			AssertEquals("CargoControlNumber", "212112345678987654321", release1.CargoControlNumber);
			AssertEquals("DateOfRelease", declaration.JE_EntryAuthorisationDate, release1.DateOfRelease);
			AssertEquals("CargoControlNumber", "123456789321566549877", release2.CargoControlNumber);
			AssertEquals("DateOfRelease", ZDateTime.Empty, release2.DateOfRelease);
			AssertEquals("CargoControlNumber", "1111111", release3.CargoControlNumber);
			AssertEquals("DateOfRelease", ZDateTime.Empty, release3.DateOfRelease);

			AssertEquals("CarrierCodeAtImportation", "2121", b3Header.CarrierCodeAtImportation);
			AssertEquals("TotalValueForDuty", 3488.89m, b3Header.TotalValueForDuty);
			AssertEquals("B3Comments", "Note to be printed on B3", b3Header.B3Comments);

			var b3SubHeaders = new List<IB3SubHeader>(b3Header.PositiveB3SubHeaders);
			AssertEquals("PositiveB3SubHeaders count", 2, b3SubHeaders.Count);
			declaration.Invoices.ApplySort(JobComInvoiceHeader.Schema.JZ_InvoiceNumber, ListSortDirection.Ascending);
			for (var j = 0; j < b3SubHeaders.Count; j++)
			{
				AssertEquals("B3SubHeaderNumber", declaration.Invoices[j].JZ_InvoiceNumber, b3SubHeaders[j].B3SubHeaderNumber.ToString());
			}

			var classLines = new List<IClassificationLine1>(b3Header.PositiveClassificationLines);
			AssertEquals("ClassificationLines count", 3, classLines.Count);
			entryHeader.AllEntryLines.Sort(CusEntryLine.Schema.CL_LineNumber);
			for (var j = 0; j < classLines.Count; j++)
			{
				AssertEquals("B3LineNumber", entryHeader.AllEntryLines[j].CL_LineNumber, classLines[j].B3LineNumber);
			}

			AssertTotalAmounts(b3Header.PositiveTotalAmounts, 0m, 2632.46m, 492m, 12472m, 15596.46m);
			//TODO: Test negative amounts
			//AssertTotalAmounts(b3Header.NegativeTotalAmounts, -75m, -150m, 0, -4459m, -4684m);

			var firstInvoice = declaration.Invoices[0];
			foreach (JobComInvoiceLine invoiceLine in firstInvoice.InvoiceLines)
			{
				var simaDuty = invoiceLine.DutiesAndTaxes[0];
				simaDuty.C1_ExemptCode = SIMACodes.Codes.C52;
			}
			b3Header = new B3ImportMessageWrapper(entryHeader);
			AssertTotalAmounts(b3Header.PositiveTotalAmounts, 0m, 2632.46m, 90m, 12472m, 15194.46m);
			foreach (JobComInvoiceLine invoiceLine in firstInvoice.InvoiceLines)
			{
				var simaDuty = invoiceLine.DutiesAndTaxes[0];
				simaDuty.C1_ExemptCode = SIMACodes.Codes.C40;
			}
			b3Header = new B3ImportMessageWrapper(entryHeader);
			AssertTotalAmounts(b3Header.PositiveTotalAmounts, 0m, 2632.46m, 90m, 12472m, 15194.46m);
			foreach (JobComInvoiceLine invoiceLine in firstInvoice.InvoiceLines)
			{
				var simaDuty = invoiceLine.DutiesAndTaxes[0];
				simaDuty.C1_ExemptCode = SIMACodes.Codes.C51;
			}
			b3Header = new B3ImportMessageWrapper(entryHeader);
			AssertTotalAmounts(b3Header.PositiveTotalAmounts, 0m, 2632.46m, 492m, 12472m, 15596.46m);

			Assert("Calculations done", b3Header.IsCalculationsDone);
			var classLine = entryHeader.AllEntryLines[0];
			AssertNotNull(classLine);
			classLine.RandomLine.DutiesAndTaxes.Find(tax => tax.C1_TaxType == DutyAndTaxTypes.Codes.GST).First().Delete();
			b3Header = new B3ImportMessageWrapper(entryHeader);
			Assert("Calculations not done", !b3Header.IsCalculationsDone);

			declaration.ImporterOfRecordAddress.OrganisationPK = importerOrRecord.PK;

			AssertEquals("BusinessNumber", "4044", b3Header.BusinessNumber);
			DeleteCusCode(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, importerOrRecord);
			b3Header = new B3ImportMessageWrapper(entryHeader);
			AssertEquals("BusinessNumber", "9876", b3Header.BusinessNumber);
			AssertEquals("Importer", "IMPORTER OF RECORD NAME", b3Header.Importer.E2_CompanyName);

			AssertEquals("GSTNumber", "1234567890", b3Header.GSTNumber);
			DeleteCusCode(OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial, importerOrRecord);
			b3Header = new B3ImportMessageWrapper(entryHeader);
			AssertEquals("BusinessNumber", string.Empty, b3Header.BusinessNumber);
			AssertEquals("GSTNumber", "1234567890", b3Header.GSTNumber);
		}

		public void TestIsCalculationsDone()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			cusEntryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			cusEntryHeader.CH_JE = declaration.PK;
			IB3Header b3Header = new B3ImportMessageWrapper(cusEntryHeader);
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
			var universalHelper = new UniversalReferenceTestDataHelper(factory);
			var taxOrFee = universalHelper.CreateTaxOrFee(Universal.Constants.RefCusTaxOrFeeTypes.Deminimus, 2500, Core.Constants.CountryCodes.Canada, new ZDateTime(2013, 01, 07), new ZDateTime(2079, 06, 06));
			factory.Save();

			var helper = new DeclarationTestHelper(factory, true);
			var canada = factory.Load<RefCountry>(Constants.CountryGuids.Canada);

			var jobDeclaration = factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.JE_OH_Importer = helper.CreateOrganisation("SUP", "SUPPLIER NAME", "AUMEL", "SUPPLIER ADDRESS", "MELBOURNE", "VIC", "3000", "123 4567").PK;
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;

			var entry = jobDeclaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entry.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted;

			IB3Header b3Header = new B3ImportMessageWrapper(entry);
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2013, 5, 1);
			var line = entry.MergedLines.AddNew();
			line.CL_CustomsValue = 5000m;
			Assert("Is HVS", !jobDeclaration.IsLowValueNormalReleaseJob);

			jobDeclaration.ImporterAddInfo.ZO_IsImporterDirectPayment = false;
			jobDeclaration.ImporterAddInfo.ZO_IsGSTDirectPayment = false;
			jobDeclaration.ImporterAddInfo.ZO_IsLVSImporterDirectPayment = false;
			AssertEquals("PaymentCode", ZString.Empty, b3Header.PaymentCode);

			jobDeclaration.ImporterAddInfo.ZO_IsImporterDirectPayment = true;
			jobDeclaration.ImporterAddInfo.ZO_IsLVSImporterDirectPayment = false;
			jobDeclaration.ImporterAddInfo.ZO_IsGSTDirectPayment = false;
			AssertEquals("PaymentCode", "I", b3Header.PaymentCode);

			jobDeclaration.ImporterAddInfo.ZO_IsImporterDirectPayment = false;
			jobDeclaration.ImporterAddInfo.ZO_IsLVSImporterDirectPayment = true;
			jobDeclaration.ImporterAddInfo.ZO_IsGSTDirectPayment = false;
			AssertEquals("PaymentCode", ZString.Empty, b3Header.PaymentCode);

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
			AssertEquals("PaymentCode", "I", b3Header.PaymentCode);

			jobDeclaration.ImporterAddInfo.ZO_IsImporterDirectPayment = false;
			jobDeclaration.ImporterAddInfo.ZO_IsLVSImporterDirectPayment = true;
			jobDeclaration.ImporterAddInfo.ZO_IsGSTDirectPayment = true;
			AssertEquals("PaymentCode", "G", b3Header.PaymentCode);

			jobDeclaration = factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.JE_OH_Importer = helper.CreateOrganisation("SUP", "SUPPLIER NAME", "AUMEL", "SUPPLIER ADDRESS", "MELBOURNE", "VIC", "3000", "123 4567").PK;
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;

			entry = jobDeclaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entry.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted;

			b3Header = new B3ImportMessageWrapper(entry);
			line = entry.MergedLines.AddNew();
			line.CL_CustomsValue = 2400m;
			invoiceHeader = jobDeclaration.Invoices.AddNew();
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2013, 5, 1);
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			jobDeclaration.ResumeApportionment();
			Assert("Is LVS", jobDeclaration.IsLowValueNormalReleaseJob);

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
		}

		[TestDate(2016, 1, 1)]
		public void TestPaymentCode2()
		{
			CACustomsDataRegistry.Instance.DefaultExciseTaxFromCustomsTariff.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var factory = JobComInvoiceLineTestHelper.PopulateDutiesAndTaxesRefFilesReturningFactory();
			var universalHelper = new UniversalReferenceTestDataHelper(factory);
			var taxOrFee = universalHelper.CreateTaxOrFee(Enterprise.Customs.Universal.Constants.RefCusTaxOrFeeTypes.Deminimus, 2500, Core.Constants.CountryCodes.Canada, new ZDateTime(2013, 01, 07), new ZDateTime(2079, 06, 06));
			factory.Save();
			var helper = new DeclarationTestHelper(factory, true);
			var canada = factory.Load<RefCountry>(Constants.CountryGuids.Canada);

			var jobDeclaration = factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.JE_OH_Importer = helper.CreateOrganisation("SUP", "SUPPLIER NAME", "AUMEL", "SUPPLIER ADDRESS", "MELBOURNE", "VIC", "3000", "123 4567").PK;
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;

			var entry = jobDeclaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entry.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted;

			IB3Header b3Header = new B3ImportMessageWrapper(entry);
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2013, 5, 1);
			var line = entry.MergedLines.AddNew();
			line.CL_CustomsValue = 5000m;
			Assert("Is HVS", !jobDeclaration.IsLowValueNormalReleaseJob);

			jobDeclaration.ImporterAddInfo.ZO_IsImporterDirectPayment = false;
			jobDeclaration.ImporterAddInfo.ZO_IsGSTDirectPayment = false;
			jobDeclaration.ImporterAddInfo.ZO_IsLVSImporterDirectPayment = false;
			AssertEquals("PaymentCode", ZString.Empty, b3Header.PaymentCode);

			jobDeclaration.ImporterAddInfo.ZO_IsImporterDirectPayment = true;
			jobDeclaration.ImporterAddInfo.ZO_IsLVSImporterDirectPayment = false;
			jobDeclaration.ImporterAddInfo.ZO_IsGSTDirectPayment = false;
			AssertEquals("PaymentCode", "I", b3Header.PaymentCode);

			jobDeclaration.ImporterAddInfo.ZO_IsImporterDirectPayment = false;
			jobDeclaration.ImporterAddInfo.ZO_IsLVSImporterDirectPayment = true;
			jobDeclaration.ImporterAddInfo.ZO_IsGSTDirectPayment = false;
			AssertEquals("PaymentCode", ZString.Empty, b3Header.PaymentCode);

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
			AssertEquals("PaymentCode", "I", b3Header.PaymentCode);

			jobDeclaration.ImporterAddInfo.ZO_IsImporterDirectPayment = false;
			jobDeclaration.ImporterAddInfo.ZO_IsLVSImporterDirectPayment = true;
			jobDeclaration.ImporterAddInfo.ZO_IsGSTDirectPayment = true;
			AssertEquals("PaymentCode", "G", b3Header.PaymentCode);

			jobDeclaration = factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.JE_OH_Importer = helper.CreateOrganisation("SUP", "SUPPLIER NAME", "AUMEL", "SUPPLIER ADDRESS", "MELBOURNE", "VIC", "3000", "123 4567").PK;
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;

			entry = jobDeclaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entry.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted;

			b3Header = new B3ImportMessageWrapper(entry);
			line = entry.MergedLines.AddNew();
			line.CL_CustomsValue = 2400m;
			invoiceHeader = jobDeclaration.Invoices.AddNew();
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2013, 5, 1);
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			jobDeclaration.ResumeApportionment();
			Assert("Is LVS", jobDeclaration.IsLowValueNormalReleaseJob);

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
		}

		internal static void AssertTotalAmounts(ITotalAmounts amounts, decimal excise, decimal gst, decimal sima, decimal duty, decimal all)
		{
			AssertEquals("TotalExciseTax", excise, amounts.TotalExciseTax);
			AssertEquals("TotalGST", gst, amounts.TotalGST);
			AssertEquals("TotalSIMAAssessment", sima, amounts.TotalSIMAAssessment);
			AssertEquals("TotalCustomsDuty", duty, amounts.TotalCustomsDuty);
			AssertEquals("TotalAllDutyAndTaxes", all, amounts.TotalAllDutyAndTaxes);
		}

		public void TestClassificationLines_CS00165970()
		{
			invoice.JobComInvoiceLines[0].CA_PageNumber = 0;
			invoice.JobComInvoiceLines[1].CA_PageNumber = 1;

			IB3Header b3Header = new B3ImportMessageWrapper(entryHeader);

			AssertNoExceptionThrown(delegate
			{ var accessed = b3Header.PositiveClassificationLines; });
		}

		#endregion

		#region TestB3SubHeaderProperties

		public void TestB3SubHeaderProperties()
		{
			invoice = declaration.Invoices[0];
			var line = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(line, 3, 1, 1000, 1000, 666, 333, Constants.Weight.Kilograms);
			line.CA_TreatmentCode = TariffTreatmentCodes.Codes.Mexico;

			line = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(line, 4, 1, 1000, 1000, 666, 333, Constants.Weight.Kilograms);
			line.JI_CountryOfOrigin = Constants.CountryCodes.Canada;

			line = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(line, 5, 1, 1000, 1000, 666, 333, Constants.Weight.Kilograms);
			line.CA_ValueForDutyCode = "12";

			invoice.JobComInvoiceLines[0].Charges.AddNew(CAChargeTypeList.Codes.OverseasFreight, 10m, Constants.CurrencyCodes.Canada);
			invoice.JobComInvoiceLines[1].Charges.AddNew(CAChargeTypeList.Codes.OverseasFreight, 20m, Constants.CurrencyCodes.Canada);
			invoice.JobComInvoiceLines[3].Charges.AddNew(CAChargeTypeList.Codes.OverseasFreight, 70m, Constants.CurrencyCodes.Canada);
			invoice.JobComInvoiceLines[4].Charges.AddNew(CAChargeTypeList.Codes.OverseasFreight, 130m, Constants.CurrencyCodes.Canada);

			var invoice1 = declaration.Invoices[1];
			invoice1.JobComInvoiceLines[0].Charges.AddNew(CAChargeTypeList.Codes.OverseasFreight, 100m, Constants.CurrencyCodes.Canada);
			invoice1.JobComInvoiceLines[1].Charges.AddNew(CAChargeTypeList.Codes.OverseasFreight, 150m, Constants.CurrencyCodes.Canada);
			declaration.DoMerge();

			IB3Header b3Header = new B3ImportMessageWrapper(declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC));
			var subHeaders = b3Header.PositiveB3SubHeaders.ToList();
			AssertEquals("PositiveB3SubHeaders Count", 4, subHeaders.Count);

			//Sub Header 1
			var expectedB3SubHeader =
				new ExpectedB3SubHeaderForTesting
				{
					B3SubHeaderNumber = 1,
					Vendor = invoice.Supplier.MainAddress,
					CountryOfOrigin = Constants.CountryCodes.Japan,
					PlaceOfExport = invoice.CA_TradeZone,
					TariffTreatmentCode = TariffTreatmentCodes.Codes.MostFavouredNation,
					USPortOfExit = invoice.CA_USPortOfExit,
					DateOfDirectShipment = invoice.EffectiveValuationDate,
					CurrencyCode = invoice.JZ_RX_NKInvoice_Currency,
					TimeLimitUnit = invoice.CA_TimeLimitCode,
					B3TimeLimits = invoice.CA_TimeLimit,
					FreightCharges = 160,
					InvoiceNumber = invoice.JZ_InvoiceNumber,
					ExchangeRate = invoice.EffectiveExchangeRateForInvoiceCurr,
					TradeZone = invoice.CA_TradeZone
				};

			AssertB3SubHeader(expectedB3SubHeader, subHeaders[0]);

			invoice.JZ_OH_Supplier = ZGuid.Empty;
			invoice.CA_TradeZone = ZString.Empty;
			expectedB3SubHeader.Vendor = declaration.Supplier.MainAddress;
			expectedB3SubHeader.PlaceOfExport = "UIL";

			AssertB3SubHeader(expectedB3SubHeader, subHeaders[0]);

			invoice.CA_RN_NKExport = Constants.CountryCodes.Canada;
			expectedB3SubHeader.PlaceOfExport = invoice.CA_RN_NKExport;
			expectedB3SubHeader.USPortOfExit = string.Empty;

			AssertB3SubHeader(expectedB3SubHeader, subHeaders[0]);

			//Sub Header 2
			expectedB3SubHeader.B3SubHeaderNumber = 2;
			expectedB3SubHeader.FreightCharges = 0;
			expectedB3SubHeader.TariffTreatmentCode = TariffTreatmentCodes.Codes.Mexico;
			AssertB3SubHeader(expectedB3SubHeader, subHeaders[1]);

			//Sub Header 3
			expectedB3SubHeader.B3SubHeaderNumber = 3;
			expectedB3SubHeader.FreightCharges = 70;
			expectedB3SubHeader.CountryOfOrigin = Constants.CountryCodes.Canada;
			expectedB3SubHeader.TariffTreatmentCode = TariffTreatmentCodes.Codes.MostFavouredNation;
			AssertB3SubHeader(expectedB3SubHeader, subHeaders[2]);

			//Sub Header 4
			expectedB3SubHeader.B3SubHeaderNumber = 4;
			expectedB3SubHeader.FreightCharges = 250;
			expectedB3SubHeader.CountryOfOrigin = Constants.CountryCodes.Japan;
			expectedB3SubHeader.Vendor = invoice1.Supplier.MainAddress;
			expectedB3SubHeader.PlaceOfExport = invoice1.CA_RN_NKExport;
			expectedB3SubHeader.USPortOfExit = invoice1.CA_USPortOfExit;
			expectedB3SubHeader.CurrencyCode = invoice1.JZ_RX_NKInvoice_Currency;
			expectedB3SubHeader.TimeLimitUnit = invoice1.CA_TimeLimitCode;
			expectedB3SubHeader.B3TimeLimits = invoice1.CA_TimeLimit;
			expectedB3SubHeader.InvoiceNumber = invoice1.JZ_InvoiceNumber;
			expectedB3SubHeader.ExchangeRate = invoice1.EffectiveExchangeRateForInvoiceCurr;
			AssertB3SubHeader(expectedB3SubHeader, subHeaders[3]);

			var collection = new DefaultFreightPercentageCollection();
			var item = collection.AddNew();
			item.ModeofTransport = "ROA";
			item.FreightPercentage = 20m;
			CACustomsDataRegistry.Instance.DefaultFreightPercentages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			declaration.JE_TransportMode = "ROA";
			foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
			{
				invoiceLine.JI_LinePrice = 0;
				invoiceLine.Charges.RemoveAndDeleteAll();
			}
			declaration.DoMerge();
			entryHeader.MergedLines[0].CL_CustomsValue = 100m;
			entryHeader.ResetTotalsAndCachedValues();
			b3Header = new B3ImportMessageWrapper(declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC));
			subHeaders = b3Header.PositiveB3SubHeaders.ToList();
			AssertEquals("FreightCharges", 0m, subHeaders[0].FreightCharges);
			AssertEquals("FreightCharges", 0m, subHeaders[1].FreightCharges);
			AssertEquals("FreightCharges", 0m, subHeaders[2].FreightCharges);
			AssertEquals("FreightCharges", 0m, subHeaders[3].FreightCharges);

			invoice.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
			declaration.DoMerge();
			entryHeader.MergedLines[0].CL_CustomsValue = 2500;
			entryHeader.ResetTotalsAndCachedValues();
			b3Header = new B3ImportMessageWrapper(declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC));
			subHeaders = b3Header.PositiveB3SubHeaders.ToList();
			AssertEquals("FreightCharges", 500m, subHeaders[0].FreightCharges);
			AssertEquals("FreightCharges", 0m, subHeaders[1].FreightCharges);
			AssertEquals("FreightCharges", 0m, subHeaders[2].FreightCharges);
			AssertEquals("FreightCharges", 0m, subHeaders[3].FreightCharges);

			invoice.CA_RN_NKExport = Core.Constants.CountryCodes.PuertoRico;
			declaration.DoMerge();
			entryHeader.MergedLines[0].CL_CustomsValue = 2500;
			entryHeader.ResetTotalsAndCachedValues();
			b3Header = new B3ImportMessageWrapper(declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC));
			subHeaders = b3Header.PositiveB3SubHeaders.ToList();
			AssertEquals("FreightCharges", 500m, subHeaders[0].FreightCharges);

			invoice.CA_RN_NKExport = Core.Constants.CountryCodes.VirginIslands;
			declaration.DoMerge();
			entryHeader.MergedLines[0].CL_CustomsValue = 2500;
			entryHeader.ResetTotalsAndCachedValues();
			b3Header = new B3ImportMessageWrapper(declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC));
			subHeaders = b3Header.PositiveB3SubHeaders.ToList();
			AssertEquals("FreightCharges", 500m, subHeaders[0].FreightCharges);

			invoice.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStatesMinorIslands;
			declaration.DoMerge();
			entryHeader.MergedLines[0].CL_CustomsValue = 2500;
			entryHeader.ResetTotalsAndCachedValues();
			b3Header = new B3ImportMessageWrapper(declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC));
			subHeaders = b3Header.PositiveB3SubHeaders.ToList();
			AssertEquals("FreightCharges", 500m, subHeaders[0].FreightCharges);
			CACustomsDataRegistry.Instance.DefaultFreightPercentages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DefaultFreightPercentageCollection());
		}

		internal static void AssertB3SubHeader(IB3SubHeader expectedB3SubHeader, IB3SubHeader b3SubHeader)
		{
			AssertEquals("B3SubHeaderNumber", expectedB3SubHeader.B3SubHeaderNumber, b3SubHeader.B3SubHeaderNumber);
			AssertEquals("Vendor Name", expectedB3SubHeader.Vendor.E2_CompanyName, b3SubHeader.Vendor.E2_CompanyName);
			AssertEquals("Vendor Country", expectedB3SubHeader.Vendor.CountryCode, b3SubHeader.Vendor.CountryCode);
			AssertEquals("Vendor State Code", expectedB3SubHeader.Vendor.E2_State, b3SubHeader.Vendor.E2_State);
			AssertEquals("Vendor Zip Code", expectedB3SubHeader.Vendor.E2_Postcode, b3SubHeader.Vendor.E2_Postcode);
			AssertEquals("CountryOfOrigin", expectedB3SubHeader.CountryOfOrigin, b3SubHeader.CountryOfOrigin);
			AssertEquals("PlaceOfExport", expectedB3SubHeader.PlaceOfExport, b3SubHeader.PlaceOfExport);
			AssertEquals("TariffTreatmentCode", expectedB3SubHeader.TariffTreatmentCode, b3SubHeader.TariffTreatmentCode);
			AssertEquals("USPortOfExit", expectedB3SubHeader.USPortOfExit, b3SubHeader.USPortOfExit);
			AssertEquals("DateOfDirectShipment", expectedB3SubHeader.DateOfDirectShipment, b3SubHeader.DateOfDirectShipment);
			AssertEquals("CurrencyCode", expectedB3SubHeader.CurrencyCode, b3SubHeader.CurrencyCode);
			AssertEquals("TimeLimitUnit", expectedB3SubHeader.TimeLimitUnit, b3SubHeader.TimeLimitUnit);
			AssertEquals("B3TimeLimits", expectedB3SubHeader.B3TimeLimits, b3SubHeader.B3TimeLimits);
			AssertEquals("FreightCharges", expectedB3SubHeader.FreightCharges, b3SubHeader.FreightCharges);
		}

		#endregion

		#region TestB3MessageContent

		public void TestB3MessageContainsWarehouseNumber()
		{
			entryHeader.Declaration.JE_MessageSubType = B3EntryTypeList.Codes.Warehouse10;
			var messageBuilder = new B3CusdecMessageBuilder<B3Message>(new B3ImportMessageWrapper(entryHeader), MessageSubTypes.Create);
			var message = messageBuilder.PopulateMessages().GetBuilderResults().First().Message;
			Assert("message contains warehouse number", message.EM_MessageText.Contains("LOC+41+351'LOC+11+423'LOC+18+12345'RFF+TN:000067897'RFF+ARA:3021'"));
			Assert("interpretation contains warehouse number", message.EM_MessageInterpretation.Replace("<tr><td style=", "\r\n<tr><td style=").Contains(@"
<tr><td style=""word-break:break-all"" rowspan=""1"">LOC+11+423&#39;</td><td>Port Of Unlading</td><td>423</td></tr>
<tr><td style=""word-break:break-all"" rowspan=""1"">LOC+18+12345&#39;</td><td>Warehouse Number</td><td>12345</td></tr>
<tr><td style=""word-break:break-all"" rowspan=""1"">RFF+TN:000067897&#39;</td><td>Transaction Number</td><td>000067897</td></tr>"));
		}

		#region Expected Message

		internal static void AppendClassificationLine2(ZStringBuilder builder, int num, ZString uom, ZString qty, ZString weight, decimal rate, decimal amount, bool addTranNum)
		{
			AppendClassificationLine2(builder, num, uom, qty, weight, string.Format("{0}.00", rate), amount, addTranNum);
		}

		internal static void AppendClassificationLine2(ZStringBuilder builder, int num, ZString uom, ZString qty, ZString weight, string rate, decimal amount, bool addTranNum)
		{
			/*B3 Line Number*/
			builder.Append("GIR+1+" + num);
			/*Class. Line Quantity*/
			if (qty != "")
			{
				builder.Append(string.Format("MEA+AAR++{0}:{1}", uom, qty));
			}
			/*Weight in KGM*/
			if (weight != "")
			{
				builder.Append("MEA+AAA++KGM:" + weight);
			}
			/*Customs Duty Rate*/
			builder.Append("TAX+5+++" + rate);
			/*Customs Duty Amount*/
			builder.Append("MOA+155:" + (amount * 100).ToString("000"));
			//TODO: Test Previous Trans. Number when we start to calulate it.
			/*Previous Trans. Number*/
			//if(addTranNum) builder.Append("DOC+998+123::1");
		}

		internal static void AppendTotalAmounts(ZStringBuilder builder, bool isPositive, decimal excise, decimal gst, decimal sima, decimal duty, decimal all)
		{
			//const string format = "0.00##";
			var type = isPositive ? "K90" : "K92";
			/*Customs Duty*/
			builder.Append("TAX+5+:::" + type);
			/*Total Customs Duty*/
			builder.Append("MOA+155:" + (duty * 100).ToString("000"));
			if (isPositive)
			{
				/*Other Charges*/
				builder.Append("TAX+1+:::" + type);
				/*Total SIM	A Assessment*/
				builder.Append("MOA+105:" + (sima * 100).ToString("000"));
			}
			/*Excise Tax*/
			builder.Append("TAX+3+:::" + type);
			/*Total Excise Tax*/
			builder.Append("MOA+4:" + (excise * 100).ToString("000"));
			/*GST*/
			builder.Append("TAX+7+:::" + type);
			/*Total GST*/
			builder.Append("MOA+1:" + (gst * 100).ToString("000"));
			/*Total All*/
			builder.Append("TAX+4+:::" + type);
			/*Total All Duty/Taxes*/
			builder.Append("MOA+176:" + (all * 100).ToString("000"));
		}

		#endregion

		#endregion

		#region TestWarehouseEntryTotals

		public void TestIntoWarehouseEntry()
		{
			entryHeader.Declaration.JE_MessageSubType = B3EntryTypeList.Codes.Warehouse10;
			IB3Header b3Header = new B3ImportMessageWrapper(entryHeader);
			AssertTotalAmounts(b3Header.PositiveTotalAmounts, 0m, 0m, 0m, 0m, 0m);
			AssertEquals("WarehouseNumber", "12345", b3Header.WarehouseNumber);
		}

		public void TestExWarehouseEntry()
		{
			entryHeader.Declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse20;
			IB3Header b3Header = new B3ImportMessageWrapper(entryHeader);
			AssertTotalAmounts(b3Header.PositiveTotalAmounts, 0m, 2632.46m, 492m, 12472m, 15596.46m);
			AssertEquals("WarehouseNumber", "12345", b3Header.WarehouseNumber);
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

		#endregion

		public void TestSetPageRelativeLineNumbers()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var lines = invoice.JobComInvoiceLines;
			var lines2 = invoice2.JobComInvoiceLines;

			const string classNum1 = "8201.10.00 10";
			const string classNum2 = "8201.40.10 00";

			JobComInvoiceLineTestHelper.FillInvoiceLine(lines.AddNew(), 1, 1, classNum1, 110m);
			JobComInvoiceLineTestHelper.FillInvoiceLine(lines.AddNew(), 2, 1, classNum1, 100m);
			JobComInvoiceLineTestHelper.FillInvoiceLine(lines.AddNew(), 3, 2, classNum1, 120m);
			JobComInvoiceLineTestHelper.FillInvoiceLine(lines.AddNew(), 4, 2, classNum1, 130m);
			JobComInvoiceLineTestHelper.FillInvoiceLine(lines.AddNew(), 5, 3, classNum1, 150m);
			JobComInvoiceLineTestHelper.FillInvoiceLine(lines.AddNew(), 6, 3, classNum2, 140m);
			JobComInvoiceLineTestHelper.FillInvoiceLine(lines.AddNew(), 7, 3, classNum2, 160m);

			JobComInvoiceLineTestHelper.FillInvoiceLine(lines2.AddNew(), 1, 4, classNum2, 110m);
			JobComInvoiceLineTestHelper.FillInvoiceLine(lines2.AddNew(), 2, 4, classNum2, 100m);
			JobComInvoiceLineTestHelper.FillInvoiceLine(lines2.AddNew(), 3, 5, classNum1, 120m);
			JobComInvoiceLineTestHelper.FillInvoiceLine(lines2.AddNew(), 4, 5, classNum1, 130m);
			JobComInvoiceLineTestHelper.FillInvoiceLine(lines2.AddNew(), 5, 6, classNum1, 150m);
			JobComInvoiceLineTestHelper.FillInvoiceLine(lines2.AddNew(), 6, 6, classNum1, 140m);
			JobComInvoiceLineTestHelper.FillInvoiceLine(lines2.AddNew(), 7, 6, classNum2, 160m);

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			var entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
			var wrapper = new B3ImportMessageWrapper(entryHeader);

			var classificationLines = wrapper.PositiveClassificationLines.ToList();
			AssertEquals("ClassificationLines.Count", 4, classificationLines.Count);

			AssertCA_PageRelativeLineNumber(1, 1, lines);
			AssertCA_PageRelativeLineNumber(2, 2, lines);
			AssertCA_PageRelativeLineNumber(1, 3, lines);
			AssertCA_PageRelativeLineNumber(2, 4, lines);
			AssertCA_PageRelativeLineNumber(1, 5, lines);
			AssertCA_PageRelativeLineNumber(2, 6, lines);
			AssertCA_PageRelativeLineNumber(3, 7, lines);

			AssertCA_PageRelativeLineNumber(1, 1, lines2);
			AssertCA_PageRelativeLineNumber(2, 2, lines2);
			AssertCA_PageRelativeLineNumber(1, 3, lines2);
			AssertCA_PageRelativeLineNumber(2, 4, lines2);
			AssertCA_PageRelativeLineNumber(1, 5, lines2);
			AssertCA_PageRelativeLineNumber(2, 6, lines2);
			AssertCA_PageRelativeLineNumber(3, 7, lines2);

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse20;
			lines[0].CA_PageRelativeLineNumber = 20;
			lines[1].CA_PageRelativeLineNumber = 21;
			lines[2].CA_PageRelativeLineNumber = 22;

			lines2[0].CA_PageRelativeLineNumber = 20;
			lines2[1].CA_PageRelativeLineNumber = 21;
			lines2[2].CA_PageRelativeLineNumber = 22;
			classificationLines = wrapper.PositiveClassificationLines.ToList();

			AssertCA_PageRelativeLineNumber(20, 1, lines);
			AssertCA_PageRelativeLineNumber(21, 2, lines);
			AssertCA_PageRelativeLineNumber(22, 3, lines);
			AssertCA_PageRelativeLineNumber(2, 4, lines);
			AssertCA_PageRelativeLineNumber(1, 5, lines);
			AssertCA_PageRelativeLineNumber(2, 6, lines);
			AssertCA_PageRelativeLineNumber(3, 7, lines);

			AssertCA_PageRelativeLineNumber(20, 1, lines2);
			AssertCA_PageRelativeLineNumber(21, 2, lines2);
			AssertCA_PageRelativeLineNumber(22, 3, lines2);
			AssertCA_PageRelativeLineNumber(2, 4, lines2);
			AssertCA_PageRelativeLineNumber(1, 5, lines2);
			AssertCA_PageRelativeLineNumber(2, 6, lines2);
			AssertCA_PageRelativeLineNumber(3, 7, lines2);
		}

		void AssertCA_PageRelativeLineNumber(int expectedNumber, int invoiceNo, JobComInvoiceLineViewCollection lines)
		{
			AssertEquals($"line{invoiceNo}.CA_PageRelativeLineNumber", expectedNumber, lines.Cast<JobComInvoiceLine>().First(x => x.JI_LineNo == invoiceNo).CA_PageRelativeLineNumber);
		}

		[ExpectNoExceptions]
		public void TestInvoiceLineHasNoB3EntryLine_WI00351451()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			cusEntryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			cusEntryHeader.CH_JE = declaration.PK;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var b3MessageWrapper = new B3ImportMessageWrapper(cusEntryHeader);
			_ = b3MessageWrapper.PositiveClassificationLines;
		}

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
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Supplier = helper.CreateOrganisation("SUP", "SUPPLIER NAME", "AUMEL", "SUPPLIER ADDRESS", "MELBOURNE", "VIC", "3000", "123 4567").PK;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.WarehouseDocAddress.OrganisationPK = helper.CreateOrganisation("WAREHOUSE NAME", "CATOR").PK;
			declaration.WarehouseDocAddress.Organisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "12345", canada);
			var importer = helper.CreateOrganisation("IMP", "IMPORTER NAME", "CATOR", "IMPORTER ADDRESS", "IMPORTER CITY", "123 4567");
			importer.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = ZString.Empty;
			((OrgImpAddInfo)importer.CountryData.ImpAddInfo).ZO_IsImporterDirectPayment = true;
			declaration.JE_OH_Importer = importer.PK;
			declaration.Importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "3021", canada);
			declaration.Importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial, "1234", canada);
			declaration.Importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForGoodsServicesHarmonizedSalesTax, "0987654321", canada);
			declaration.JE_CustomsOffice = "351";
			declaration.CA_UnladingOffice = "423";
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "00006789";
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2010, 4, 30, 12, 41, 25);
			declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "212112345678987654321";
			declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = ZString.Empty;
			declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "123456789321566549877";
			declaration.JE_OH_ShippingLine = helper.ShippingLine.PK;
			declaration.JE_OH_Forwarder = helper.ExportForwarder.PK;
			declaration.ShippingLine.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "2121", canada);
			declaration.Forwarder.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "1212", canada);

			//Invoice 1
			var supplier1 = helper.CreateOrganisation("SUP", "SUPPLIER NAME", "AUMEL", "SUPPLIER ADDRESS", "MELBOURNE", "VIC", "3000", "123 4567").PK;
			invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1";
			invoice.Charges.AddNew(CAChargeTypeList.Codes.OverseasFreight, 100m);
			invoice.JZ_OH_Supplier = supplier1;
			invoice.ExporterDocumentaryAddress.OrganisationPK = helper.CreateOrganisation("SUP", "EXPORTER NAME", "USCHI", "EXPORTER ADDRESS", "CHICARGO", "IL", "12321", "123 4567").PK;
			invoice.CA_RN_NKExport = Constants.CountryCodes.UnitedStates;
			invoice.CA_USStateOfExport = USStatesList.Codes.NewYork;
			invoice.CA_TradeZone = "168B";
			invoice.CA_USPortOfExit = "2813";
			invoice.CA_TreatmentCode = TariffTreatmentCodes.Codes.MostFavouredNation;
			invoice.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Day;
			invoice.CA_TimeLimit = 20;
			helper.USD.SetCustomsRate(ZDateTime.Today, ZDateTime.Today, 1.111111);
			invoice.JZ_RX_NKInvoice_Currency = helper.USD.RX_Code;
			invoice.JZ_Weight = 250;
			invoice.JZ_WeightUQ = "LB";

			//Invoice Lines
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(invoiceLine, 1, 1, 1000, 1000, 666, 333, Constants.Weight.Kilograms, 222);
			JobComInvoiceLineTestHelper.AddTariffRecord(invoiceLine);
			JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 2, 1, 700, 500, 250, 100, Constants.Weight.Tonnes, 180);

			//Invoice 2
			var supplier2 = helper.CreateOrganisation("SUP", "SUPPLIER NAME2", "USNYC", "SUPPLIER ADDRESS2", "NEW YOUK", "NY", "12345", "123 4567").PK;
			invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "2";
			invoice.Charges.AddNew(CAChargeTypeList.Codes.OverseasFreight, 0.01m);
			invoice.JZ_OH_Supplier = supplier2;
			invoice.CA_RN_NKExport = Constants.CountryCodes.NewZealand;
			invoice.CA_USPortOfExit = "3216";
			invoice.CA_TreatmentCode = TariffTreatmentCodes.Codes.MostFavouredNation;
			invoice.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Month;
			invoice.CA_TimeLimit = 2;
			invoice.JZ_RX_NKInvoice_Currency = helper.CAD.RX_Code;
			invoice.CA_TradeZone = "";
			invoice.JZ_Weight = 50;
			invoice.JZ_WeightUQ = "LB";

			//Invoice Lines
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(invoiceLine, 3, 2, 1000, 1000, 666, 333, Constants.Weight.Kilograms);
			invoiceLine.JI_Tariff = "0301104567";
			invoiceLine.JI_CustomsQuantity = 1000;
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Litre;
			JobComInvoiceLineTestHelper.AddTariffRecord(invoiceLine);

			//TODO: Make this line negative.
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(invoiceLine, 4, 2, 600, 100, 90, 80, Constants.Weight.Kilograms, 90);
			invoiceLine.JI_Tariff = "0301104567";
			invoiceLine.JI_CustomsQuantity = 100;
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Litre;

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);

			importerOrRecord = helper.CreateOrganisation("IMPORTER OF RECORD NAME", "NZAKL", "IMPORTER OF RECORD ADDRESS", "AUCKLAND", "123 4567");
			importerOrRecord.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "4044", canada);
			importerOrRecord.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial, "9876", canada);
			importerOrRecord.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForGoodsServicesHarmonizedSalesTax, "1234567890", canada);
		}

		internal static void DeleteCusCode(string code, OrgHeader orgHeader)
		{
			orgHeader.CustomsCodes.RemoveAndDelete(orgHeader.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(code, Constants.CountryCodes.Canada));
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		JobComInvoiceHeader invoice;
		OrgHeader importerOrRecord;

		#endregion
	}
}
