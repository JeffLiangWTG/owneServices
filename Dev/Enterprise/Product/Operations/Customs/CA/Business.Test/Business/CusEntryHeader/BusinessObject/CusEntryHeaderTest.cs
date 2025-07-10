using System;
using System.Data;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusEntryHeader))]
	sealed class CusEntryHeaderTest : Customs.Business.Testing.CusEntryHeaderTest
	{
		[TestDate(2025, 5, 28)]
		public void TestCH_EntryStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Today.AddDays(-10);
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			declaration.CA_AccountingAge = 0;
			entry.CH_EntryStatus = EntryStatusList.Codes.Clear;
			AssertEquals("CA_AccountingAge will not update to 6 as entry is not CAD", 0, declaration.CA_AccountingAge);
			entry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			declaration.CA_AccountingAge = 0;
			entry.CH_EntryStatus = EntryStatusList.Codes.Cancelled;
			AssertEquals("CA_AccountingAge will update to 6 as entry is CAD", 6, declaration.CA_AccountingAge);
		}

		[TestDate(2025, 5, 28)]
		public void TestCH_MessageType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Today.AddDays(-10);
			var entry = declaration.CustomsEntryHeaders.AddNew();
			declaration.CA_AccountingAge = 0;
			entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			AssertEquals(0, declaration.CA_AccountingAge);
			entry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			AssertEquals(6, declaration.CA_AccountingAge);
		}

		public void TestIsB3C()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			Assert(!entry.IsB3C);
			entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			Assert(entry.IsB3C);
		}

		public void TestIsCAD()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			Assert(!entry.IsCAD);
			entry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			Assert(entry.IsCAD);
		}

		public void TestDutyFeeChangedSinceLastResponse()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry.MergedLines.AddNew();
			var entryLine2 = entry.MergedLines.AddNew();
			entryLine1.ConfirmedFees.SetAmount(CADDutyTaxFeeTypeCodes.Codes.CUD, 1m);
			entryLine1.ConfirmedFees.SetAmount(CADDutyTaxFeeTypeCodes.Codes.GST, 2m);
			entryLine2.ConfirmedFees.SetAmount(CADDutyTaxFeeTypeCodes.Codes.CUD, 7m);
			entryLine2.ConfirmedFees.SetAmount(CADDutyTaxFeeTypeCodes.Codes.GST, 8m);
			Assert(entry.DutyFeeChangedSinceLastResponse);

			entryLine1.Fees.SetAmount(EntryChargeTypeList.Codes.TotalDutyAmount, 1m);
			entryLine1.Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTAmount, 2m);
			entryLine2.Fees.SetAmount(EntryChargeTypeList.Codes.TotalDutyAmount, 7m);
			Factory.Save();
			Assert(entry.DutyFeeChangedSinceLastResponse);
			entryLine2.Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTAmount, 9m);
			Factory.Save();
			Assert(entry.DutyFeeChangedSinceLastResponse);
			entryLine2.Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTAmount, 8m);
			Factory.Save();
			Assert(!entry.DutyFeeChangedSinceLastResponse);
		}

		[UseSnapshotProtection]
		[TestDate(2021, 05, 25, 15, 39, 42)]
		public void TestPopulateEntryNumberForExpEntryHeader()
		{
			SetProxyOrg(Factory);
			ZString fountainKey = ZDateTime.Now.Year.ToString();
			fountainKey = fountainKey.SubstringSafe(fountainKey.Length - 1, 1);
			fountainKey = "EXPLIC" + fountainKey;

			var fountain = Env.NumberFountains.GetCAEntryNumberGeneratorFountain(fountainKey);
			fountain.SetValues(Factory, minValue: 1, nextValue: 10000, maxValue: 999999999);

			var entry = CreateExportCusEntryHeader(Factory, JobMessageTypeList.Codes.Export);

			Assert("EntryNumber not set yet", entry.EntryNumber.IsEmpty);

			Factory.Save();

			AssertEquals("EntryNumber", ZString.Empty, entry.EntryNumber);
		}

		[UseSnapshotProtection]
		[TestDate(2021, 05, 25, 15, 39, 42)]
		public void TestEntryNumberReGeneratedAfterSaveFailed()
		{
			ZString fountainKey = ZDateTime.Now.Year.ToString();
			fountainKey = fountainKey.SubstringSafe(fountainKey.Length - 1, 1);
			fountainKey = "EXPLIC" + fountainKey;

			var fountain = Env.NumberFountains.GetCAEntryNumberGeneratorFountain(fountainKey);
			fountain.SetValues(Factory, minValue: 1, nextValue: 10000, maxValue: 999999999);

			var factory1 = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory();
			SetProxyOrg(factory1);
			SetProxyOrg(factory2);

			var entry1 = CreateExportCusEntryHeader(factory1, JobMessageTypeList.Codes.Export);

			factory1.Saving += ThrowCannotSaveException;

			AssertExceptionThrown(typeof(ZCannotSaveException), delegate
			{ factory1.Save(); });

			var entry2 = CreateExportCusEntryHeader(factory2, JobMessageTypeList.Codes.Export);
			factory2.Save();

			AssertEquals(ZString.Empty, entry1.EntryNumber);
			AssertEquals(ZString.Empty, entry2.EntryNumber);

			factory1.Saving -= ThrowCannotSaveException;

			factory1.Save();
			AssertEquals(ZString.Empty, entry1.EntryNumber);
		}

		void ThrowCannotSaveException(BusinessObjectFactory factory)
		{
			throw new ZCannotSaveException("can not save", "");
		}

		void SetProxyOrg(BusinessObjectFactory factory)
		{
			var company = GlbCompany.GetCurrentCompany(factory);
			var companyProxy = company.OrgProxy;
			var canada = factory.Load<RefCountry>(Core.Constants.CountryGuids.Canada);
			if (companyProxy.CustomsCodes.GetCustomsRegNo(OrgCusCode.CACodeTypes.ExportLicenceNumber).IsEmpty)
			{
				companyProxy.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.ExportLicenceNumber, "EXPLIC", canada);
			}
			factory.Save();
		}

		CusEntryHeader CreateExportCusEntryHeader(BusinessObjectFactory factory, ZString messageType)
		{
			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = messageType;

			var entry = factory.NewWithValidTestData<CusEntryHeader>();
			entry.CH_JE = declaration.PK;
			declaration.CustomsEntryHeaders.Add(entry);
			return entry;
		}

		public void TestHumanReadableName()
		{
			var entry = Factory.New<CusEntryHeader>();
			entry.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			AssertEquals("Customs Entry (REL) " + entry.CH_BGMReference, entry.HumanReadableName);
			entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			AssertEquals("Customs Entry (B3C) " + entry.CH_BGMReference, entry.HumanReadableName);
			entry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			AssertEquals("Customs Entry (CAD) " + entry.CH_BGMReference, entry.HumanReadableName);
		}

		public void TestEffectivePortOfCLearance_NotSubmitted_ShowNothing()
		{
			var entry = CreateExportCusEntryHeader(Factory, JobMessageTypeList.Codes.Import);
			Assert("Port Of CLearance Override is not set", entry.CA_PortOfClearanceOverride.IsEmpty);
			Assert("Manual Submission not entered yet", entry.CH_EntrySubmittedDate.IsEmpty);
			Assert("EffectivePortOfClearance Show Nothing", entry.EffectivePortOfClearance.IsEmpty);
		}

		public void TestG7XEntryTypeIsEXP()
		{
			var entry = CreateExportCusEntryHeader(Factory, JobMessageTypeList.Codes.Export);
			entry.CH_MessageType = MessageTypeList.Codes.G7Export;
			entry.EntryNumber = "GDS3234";
			var entryNum = entry.CusEntryNumber;
			AssertEquals("entryNum.CE_EntryNum", "GDS3234", entryNum.CE_EntryNum);
			AssertEquals("entryNum.CE_EntryType", JobMessageTypeList.Codes.Export, entryNum.CE_EntryType);
		}

		public void TestEffectivePortOfCLearance_SubmittedWithoutOverride_ShowValueFromHeader()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_CustomsOffice = "TEST";

			var entry = Factory.NewWithValidTestData<CusEntryHeader>();
			entry.CH_JE = declaration.PK;
			declaration.CustomsEntryHeaders.Add(entry);

			entry.CH_EntrySubmittedDate = ZDateTime.Now;

			Assert("Port Of CLearance Override is not set", entry.CA_PortOfClearanceOverride.IsEmpty);
			Assert("Manual Submission has been made", entry.CH_EntrySubmittedDate.IsValid);
			AssertEquals("EffectivePortOfClearance Show header", declaration.JE_CustomsOffice, entry.EffectivePortOfClearance);
		}

		public void TestEffectivePortOfCLearance_SubmittedWithverride_ShowOverride()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_CustomsOffice = "TEST";

			var entry = Factory.NewWithValidTestData<CusEntryHeader>();
			entry.CH_JE = declaration.PK;
			declaration.CustomsEntryHeaders.Add(entry);

			entry.CH_EntrySubmittedDate = ZDateTime.Now;
			entry.CA_PortOfClearanceOverride = "OVER";

			Assert("Port Of CLearance Override is set", !entry.CA_PortOfClearanceOverride.IsEmpty);
			Assert("Manual Submission has been made", entry.CH_EntrySubmittedDate.IsValid);
			AssertEquals("EffectivePortOfClearance Show override", entry.CA_PortOfClearanceOverride, entry.EffectivePortOfClearance);
		}

		public void TestEffectiveValuationDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ServiceOptions.Codes.IID;
			declaration.JE_ValuationDate = new ZDate(2025, 03, 17);
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			AssertNotEquals(declaration.JE_ValuationDate, entryHeader.EffectiveValuationDate);
			entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			AssertEquals(declaration.JE_ValuationDate, entryHeader.EffectiveValuationDate);
		}

		public void TestIsImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var mockEntry = Factory.NewMoq<CusEntryHeader>();
			var entry = mockEntry.Object;
			AssertEquals(false, entry.IsImport);
			entry.CH_JE = declaration.PK;
			AssertEquals(true, entry.IsImport);
		}

		public void TestIM2CustomsCharges()
		{
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest())
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
				declaration.JE_CustomsOffice = "351";
				declaration.TransactionNumber.AccountSecurityCode = "12345";
				declaration.TransactionNumber.SequentialNumber = "00006789";
				var b3Invoice = declaration.Invoices.AddNew();
				b3Invoice.JZ_RX_NKInvoice_Currency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates).RX_Code;
				var b3InvoiceLine = b3Invoice.JobComInvoiceLines.AddNew();
				b3InvoiceLine.JI_Tariff = "123456789";
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();

				var header = declaration.B3EntryHeader;
				var lineOne = header.AllEntryLines.First();
				lineOne.Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.TotalDutyAmount, 100m);
				lineOne.Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.TotalSIMAAmount, 200m);
				lineOne.Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.TotalGSTAmount, 300m);
				Factory.Save();

				var im2 = declaration.GetNewCopyToB2Declaration();
				im2.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				im2.ApportionmentDirty = false;
				im2.DoMerge();

				var line = im2.B3EntryHeader.AllEntryLines.First();
				line.Fees.SetAmount(EntryChargeTypeList.Codes.TotalDutyAmount, 200m);
				line.Fees.SetAmount(EntryChargeTypeList.Codes.TotalSIMAAmount, 300m);
				line.Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTAmount, 400m);
				Factory.Save();

				var entryHeader = declaration.B3EntryHeader as ICustomsChargeEntry;
				var b2EntryHeader = im2.B3EntryHeader as ICustomsChargeEntry;
				AssertNotNull(entryHeader);
				AssertNotNull(b2EntryHeader);
				AssertEquals(100m, entryHeader.GetTotalChargeValueFor(entryHeader.EntryChargeTypeList.OfType<Enterprise.Registry.Business.Customs.EntryChargeType>().FirstOrDefault(x => x.Code == "DTY"), ""));
				AssertEquals(200m, entryHeader.GetTotalChargeValueFor(entryHeader.EntryChargeTypeList.OfType<Enterprise.Registry.Business.Customs.EntryChargeType>().FirstOrDefault(x => x.Code == "SIM"), ""));
				AssertEquals(300m, entryHeader.GetTotalChargeValueFor(entryHeader.EntryChargeTypeList.OfType<Enterprise.Registry.Business.Customs.EntryChargeType>().FirstOrDefault(x => x.Code == "GST"), ""));
				AssertEquals(200m, b2EntryHeader.GetTotalChargeValueFor(entryHeader.EntryChargeTypeList.OfType<Enterprise.Registry.Business.Customs.EntryChargeType>().FirstOrDefault(x => x.Code == "DTY"), ""));
				AssertEquals(300m, b2EntryHeader.GetTotalChargeValueFor(entryHeader.EntryChargeTypeList.OfType<Enterprise.Registry.Business.Customs.EntryChargeType>().FirstOrDefault(x => x.Code == "SIM"), ""));
				AssertEquals(400m, b2EntryHeader.GetTotalChargeValueFor(entryHeader.EntryChargeTypeList.OfType<Enterprise.Registry.Business.Customs.EntryChargeType>().FirstOrDefault(x => x.Code == "GST"), ""));

				AssertEquals(100m, im2.B3EntryHeader.GetIM2TotalChargeValueByDebtorFor(entryHeader.EntryChargeTypeList.OfType<Enterprise.Registry.Business.Customs.EntryChargeType>().FirstOrDefault(x => x.Code == "DTY")).Values.First());
				AssertEquals(100m, im2.B3EntryHeader.GetIM2TotalChargeValueByDebtorFor(entryHeader.EntryChargeTypeList.OfType<Enterprise.Registry.Business.Customs.EntryChargeType>().FirstOrDefault(x => x.Code == "SIM")).Values.First());
				AssertEquals(100m, im2.B3EntryHeader.GetIM2TotalChargeValueByDebtorFor(entryHeader.EntryChargeTypeList.OfType<Enterprise.Registry.Business.Customs.EntryChargeType>().FirstOrDefault(x => x.Code == "GST")).Values.First());
				line.Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTAmount, 200m);
				Factory.Save();
				AssertEquals(0m, im2.B3EntryHeader.GetIM2TotalChargeValueByDebtorFor(entryHeader.EntryChargeTypeList.OfType<Enterprise.Registry.Business.Customs.EntryChargeType>().FirstOrDefault(x => x.Code == "GST")).Values.First());
			}
		}

		public void TestB2B3XCustomsCharges()
		{
			AssertB2B3XCustomsCharges(JobMessageTypeList.Codes.B2Adjustments);
			AssertB2B3XCustomsCharges(JobMessageTypeList.Codes.XTypeEntry);
		}

		void AssertB2B3XCustomsCharges(ZString messageType)
		{
			var b2 = Factory.New<JobDeclaration>();
			b2.JE_MessageType = messageType;
			b2.CA_B2Type = B2TypeList.Codes.Specific;
			var subHeader = b2.B2AsAccountedForInvoices.AddNew();
			subHeader.JZ_InvoiceNumber = "INV1";
			subHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
			var line = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			line.CA_OriginalLineNo = "1";
			line.JI_Description = "SOME DESCRIPTION";
			line.CA_AuthorityNumber = "AUTHO";
			line.JI_Tariff = "2402.10.00 10";
			line.CA_99TariffCode = "9960";
			line.JI_CustomsQuantity = 5m;
			line.JI_CustomsUnitQty = "MIL";
			line.CA_ValueForDutyCode = ValueForDutyCodes.Codes.RelatedFirmsSimilarGoods;
			line.CA_CVforCurrConv = 1000m;

			var sima = line.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ADD);
			sima.C1_Override = true;
			sima.C1_Amount = 500m;
			sima.C1_ExemptCode = SIMACodes.Codes.C31;

			var excise = line.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ExciseTax);
			excise.C1_Override = true;
			excise.C1_Amount = 50m;
			excise.C1_Rate = 5.0m;
			excise.C1_RateType = RateTypes.Codes.Specific;

			var duty = line.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty.C1_Override = true;
			duty.C1_Amount = 300m;
			duty.C1_Rate = 6.5m;
			duty.C1_UnitOfMeasure = "AG";
			duty.C1_RateType = RateTypes.Codes.AdValorem;

			var gst = line.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			gst.C1_Override = true;
			gst.C1_Amount = 60m;
			gst.C1_Rate = 5m;
			gst.C1_RateType = RateTypes.Codes.AdValorem;

			var laimLine = line.CorrespondingAsClaimedForInvoiceLine;
			laimLine.DutiesAndTaxes.DeleteAll();
			var sima2 = laimLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CVD);
			sima2.C1_Override = true;
			sima2.C1_Amount = 400m;
			sima2.C1_ExemptCode = SIMACodes.Codes.C31;

			var excise2 = laimLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ExciseTax);
			excise2.C1_Override = true;
			excise2.C1_Amount = 40m;
			excise2.C1_Rate = 5.0m;
			excise2.C1_RateType = RateTypes.Codes.Specific;

			var duty2 = laimLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty2.C1_Override = true;
			duty2.C1_Amount = 200m;
			duty2.C1_Rate = 6.5m;
			duty2.C1_UnitOfMeasure = "AG";
			duty2.C1_RateType = RateTypes.Codes.AdValorem;

			var gst2 = laimLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			gst2.C1_Override = true;
			gst2.C1_Amount = 60m;
			gst2.C1_Rate = 5m;
			gst2.C1_RateType = RateTypes.Codes.AdValorem;
			b2.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			b2.DoMerge();

			var b2EntryHeader = (CusEntryHeader)b2.ActiveEntryHeaders.First();
			AssertEquals(-100m, b2EntryHeader.GetB2TotalChargeValueByDebtorFor(b2EntryHeader.EntryChargeTypeList.OfType<Enterprise.Registry.Business.Customs.EntryChargeType>().FirstOrDefault(x => x.Code == "SIM")).Values.First());
			AssertEquals(-10m, b2EntryHeader.GetB2TotalChargeValueByDebtorFor(b2EntryHeader.EntryChargeTypeList.OfType<Enterprise.Registry.Business.Customs.EntryChargeType>().FirstOrDefault(x => x.Code == "EXS")).Values.First());
			AssertEquals(-100m, b2EntryHeader.GetB2TotalChargeValueByDebtorFor(b2EntryHeader.EntryChargeTypeList.OfType<Enterprise.Registry.Business.Customs.EntryChargeType>().FirstOrDefault(x => x.Code == "DTY")).Values.First());
			AssertEquals(0m, b2EntryHeader.GetB2TotalChargeValueByDebtorFor(b2EntryHeader.EntryChargeTypeList.OfType<Enterprise.Registry.Business.Customs.EntryChargeType>().FirstOrDefault(x => x.Code == "GST")).Values.First());
		}

		public void TestReferenceNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var mockEntry = Factory.NewMoq<CusEntryHeader>();
			var entry = mockEntry.Object;
			entry.CH_JE = declaration.PK;
			entry.CH_BGMReference = "BGMREF";
			entry.CH_Status = "XXX";
			AssertEquals("BGMREF", entry.ReferenceNumber);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "000";
			AssertEquals(" LVS ID : 000", entry.ReferenceNumber);

			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest())
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				entry.CH_BGMReference = declaration.TransactionNumber.AccountSecurityCode + "000000000";
				Factory.Save();
				Assert(!entry.CH_BGMReference.EndsWith("000000000", StringComparison.OrdinalIgnoreCase));
			}
		}

		public void TestReferenceNumberForB3X()
		{
			var declaration = Factory.New<JobDeclaration>();
			var mockEntry = Factory.NewMoq<CusEntryHeader>();
			var entry = mockEntry.Object;
			entry.CH_JE = declaration.PK;
			entry.CH_BGMReference = "BGMREF";
			entry.CH_Status = "XXX";
			AssertEquals("BGMREF", entry.ReferenceNumber);

			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest())
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
				declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
				entry.CH_BGMReference = declaration.TransactionNumber.AccountSecurityCode + "000000000";
				Factory.Save();
				Assert(!entry.CH_BGMReference.EndsWith("000000000", StringComparison.OrdinalIgnoreCase));
			}
		}

		public void TestCopyingReferenceNumber_TransactionNumberGeneratedAfterMerging()
		{
			using (CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (CACustomsDataRegistry.Instance.ForceManualInputOfTransactionNumber.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest("12345"))
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "12345");

				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

				declaration.Invoices.AddNew().InvoiceLines.AddNew();
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();

				AssertEquals("Precondition: Transaction Number is empty", "12345000000000", declaration.TransactionNumber.ToString());
				AssertEquals("Precondition: B3C CH_BGMReference is empty", "12345000000000", declaration.B3EntryHeader.CH_BGMReference);
				AssertEquals("Precondition: REL CH_BGMReference is empty", "12345000000000", declaration.ReleaseEntryHeader.CH_BGMReference);

				Factory.Save();

				AssertEquals("Transaction Number should be generated", "12345000000012", declaration.FormattedTransactionNumber);
				AssertEquals("B3C CH_BGMReference should copy from Transaction Number", "12345000000012", declaration.B3EntryHeader.CH_BGMReference);
				AssertEquals("REL CH_BGMReference should copy from Transaction Number", "12345000000012", declaration.ReleaseEntryHeader.CH_BGMReference);
			}
		}

		public void TestEntryHeaderStatusDescription()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var mockEntry = Factory.NewMoq<CusEntryHeader>();
			var entry = mockEntry.Object;
			entry.CH_JE = declaration.PK;
			entry.CH_EntryStatus = ZString.Empty;
			AssertEquals(ZString.Empty, entry.EntryHeaderStatusDescription);
		}

		public void TestTotalsProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry.MergedLines.AddNew();
			var entryLine2 = entry.MergedLines.AddNew();
			entryLine1.Fees.SetAmount(EntryChargeTypeList.Codes.TotalDutyAmount, 1m);
			entryLine1.Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTAmount, 2m);
			entryLine1.Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTDirectAmount, 3m);
			entryLine1.Fees.SetAmount(EntryChargeTypeList.Codes.TotalExciseTaxAmount, 4m);
			entryLine1.Fees.SetAmount(EntryChargeTypeList.Codes.TotalNonBillableSIMAAmount, 5m);
			entryLine1.Fees.SetAmount(EntryChargeTypeList.Codes.TotalSIMAAmount, 6m);
			entryLine2.Fees.SetAmount(EntryChargeTypeList.Codes.TotalDutyAmount, 7m);
			entryLine2.Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTAmount, 8m);
			entryLine2.Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTDirectAmount, 9m);
			entryLine2.Fees.SetAmount(EntryChargeTypeList.Codes.TotalExciseTaxAmount, 10m);
			entryLine2.Fees.SetAmount(EntryChargeTypeList.Codes.TotalNonBillableSIMAAmount, 11m);
			entryLine2.Fees.SetAmount(EntryChargeTypeList.Codes.TotalSIMAAmount, 12m);
			AssertEquals("Total SIMA Duty", 34m, entry.TotalSimaDuty);
			AssertEquals("Total Excise Tax", 14m, entry.TotalExciseTax);
			AssertEquals("Total Duty And Tax", 78m, entry.TotalDutyAndTax);
			AssertEquals("Total Payable", 62m, entry.TotalAmountPayable);
			AssertEquals("Total Duty", 8m, entry.TotalDutyAmount);
			AssertEquals("Total GST", 22m, entry.GSTAmount);
		}

		public void TestPositiveAndNegativeTotalAmounts()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse20;

			var invoice = declaration.Invoices.AddNew();

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.SuspendMarkApportionmentDirtyForDutiesAndTaxes();
			var duty = invoiceLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty.C1_Amount = 100m;
			duty.C1_Override = true;
			var tax = invoiceLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ExciseTax);
			tax.C1_Amount = 200m;
			tax.C1_Override = true;
			var sima = invoiceLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ADD);
			sima.C1_Amount = 300m;
			sima.C1_Override = true;
			sima.C1_ExemptCode = "1";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.SuspendMarkApportionmentDirtyForDutiesAndTaxes();
			duty = invoiceLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty.C1_Amount = 1000m;
			duty.C1_Override = true;
			tax = invoiceLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ExciseTax);
			tax.C1_Amount = 2000m;
			tax.C1_Override = true;
			sima = invoiceLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.SUR);
			sima.C1_Amount = 3000m;
			sima.C1_Override = true;
			sima.C1_ExemptCode = "1";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entryHeader = declaration.B3EntryHeader;
			var positiveAmounts = entryHeader.PositiveTotalAmounts;
			AssertEquals("Customs Duty", 1100m, positiveAmounts.TotalCustomsDuty);
			AssertEquals("Tax", 2200m, positiveAmounts.TotalExciseTax);
			AssertEquals("SIMA", 3300m, positiveAmounts.TotalSIMAAssessment);

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Warehouse10;
			positiveAmounts = entryHeader.PositiveTotalAmounts;
			AssertEquals("Customs Duty", 0m, positiveAmounts.TotalCustomsDuty);
			AssertEquals("Tax", 0m, positiveAmounts.TotalExciseTax);
			AssertEquals("SIMA", 0m, positiveAmounts.TotalSIMAAssessment);

			foreach (CusEntryLine entryLine in entryHeader.AllEntryLines)
			{
				entryLine.Fees.SetAmount(EntryChargeTypeList.Codes.CustomsValueInInvoiceCurrency, -1m);
			}

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse20;
			var negativeAmounts = entryHeader.NegativeTotalAmounts;
			AssertEquals("Customs Duty", -1100m, negativeAmounts.TotalCustomsDuty);
			AssertEquals("Tax", -2200m, negativeAmounts.TotalExciseTax);
			AssertEquals("SIMA", 0m, negativeAmounts.TotalSIMAAssessment);

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Warehouse10;
			negativeAmounts = entryHeader.NegativeTotalAmounts;
			AssertEquals("Customs Duty", 0m, negativeAmounts.TotalCustomsDuty);
			AssertEquals("Tax", 0m, negativeAmounts.TotalExciseTax);
			AssertEquals("SIMA", 0m, negativeAmounts.TotalSIMAAssessment);
		}

		public void TestPositiveAndNegativeTotalAmountsForCAD()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;

				var invoice = declaration.Invoices.AddNew();

				var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine1.SuspendMarkApportionmentDirtyForDutiesAndTaxes();
				var duty = invoiceLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
				duty.C1_Amount = 100m;
				duty.C1_Override = true;
				var tax = invoiceLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ExciseTax);
				tax.C1_Amount = 200m;
				tax.C1_Override = true;
				var sima = invoiceLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ADD);
				sima.C1_Amount = 300m;
				sima.C1_Override = true;
				sima.C1_ExemptCode = "1";

				var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine2.SuspendMarkApportionmentDirtyForDutiesAndTaxes();
				duty = invoiceLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
				duty.C1_Amount = 1000m;
				duty.C1_Override = true;
				tax = invoiceLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ExciseTax);
				tax.C1_Amount = 2000m;
				tax.C1_Override = true;
				sima = invoiceLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.SUR);
				sima.C1_Amount = 3000m;
				sima.C1_Override = true;
				sima.C1_ExemptCode = "1";

				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

				var entryHeader = declaration.B3EntryHeader;
				var positiveAmounts = entryHeader.PositiveTotalAmounts;
				AssertEquals("Customs Duty", 1100m, positiveAmounts.TotalCustomsDuty);
				AssertEquals("Tax", 2200m, positiveAmounts.TotalExciseTax);
				AssertEquals("SIMA", 3300m, positiveAmounts.TotalSIMAAssessment);

				declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse101;
				positiveAmounts = entryHeader.PositiveTotalAmounts;
				AssertEquals("Customs Duty", 0m, positiveAmounts.TotalCustomsDuty);
				AssertEquals("Tax", 0m, positiveAmounts.TotalExciseTax);
				AssertEquals("SIMA", 0m, positiveAmounts.TotalSIMAAssessment);

				declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse102;
				positiveAmounts = entryHeader.PositiveTotalAmounts;
				AssertEquals("Customs Duty", 0m, positiveAmounts.TotalCustomsDuty);
				AssertEquals("Tax", 0m, positiveAmounts.TotalExciseTax);
				AssertEquals("SIMA", 0m, positiveAmounts.TotalSIMAAssessment);

				foreach (CusEntryLine entryLine in entryHeader.AllEntryLines)
				{
					entryLine.Fees.SetAmount(EntryChargeTypeList.Codes.CustomsValueInInvoiceCurrency, -1m);
				}

				declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
				var negativeAmounts = entryHeader.NegativeTotalAmounts;
				AssertEquals("Customs Duty", -1100m, negativeAmounts.TotalCustomsDuty);
				AssertEquals("Tax", -2200m, negativeAmounts.TotalExciseTax);
				AssertEquals("SIMA", 0m, negativeAmounts.TotalSIMAAssessment);

				declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse101;
				negativeAmounts = entryHeader.NegativeTotalAmounts;
				AssertEquals("Customs Duty", 0m, negativeAmounts.TotalCustomsDuty);
				AssertEquals("Tax", 0m, negativeAmounts.TotalExciseTax);
				AssertEquals("SIMA", 0m, negativeAmounts.TotalSIMAAssessment);

				declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse102;
				negativeAmounts = entryHeader.NegativeTotalAmounts;
				AssertEquals("Customs Duty", 0m, negativeAmounts.TotalCustomsDuty);
				AssertEquals("Tax", 0m, negativeAmounts.TotalExciseTax);
				AssertEquals("SIMA", 0m, negativeAmounts.TotalSIMAAssessment);
			}
		}

		public void TestEntryNumber()
		{
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest("98765"))
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "98765");
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.CH_MessageType = MessageTypeList.Codes.EDIRelease;
				var transaction = declaration.TransactionNumber;// touch to instantiate
				Factory.Save();
				Assert(!declaration.DeclarationNumber.IsEmpty);
				AssertEquals("Import Entry Number is declaration number", declaration.DeclarationNumber, entry.EntryNumber);
			}
		}

		public void TestGrossWeightForCA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TotalWeight = 20m;
			declaration.JE_TotalWeightUnit = "LB";
			var entry = Factory.New<CusEntryHeader>();
			declaration.CustomsEntryHeaders.Add(entry);
			AssertEquals("Gross weight from declaration", 9.07m, entry.GrossWeight.InKilograms.Round(2));

			var entryLine1 = entry.MergedLines.AddNew();
			var entryLine2 = entry.MergedLines.AddNew();
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoice1.JZ_Weight = 50;
			invoice1.JZ_WeightUQ = "LB";
			invoice2.JZ_Weight = 5;
			invoice2.JZ_WeightUQ = "KG";
			entry.ResetTotalsAndCachedValues();
			AssertEquals("Gross weight from invoices", 27.68m, entry.GrossWeight.InKilograms.Round(2));
		}

		public void TestMessageTypeBooleans()
		{
			var entry = Factory.New<CusEntryHeader>();
			entry.CH_MessageType = MessageTypeList.Codes.G7Export;
			AssertEquals("IsG7ExportDeclaration", true, entry.IsG7ExportDeclaration);
			AssertEquals("IsImportEDIRelease", false, entry.IsImportEDIRelease);
			AssertEquals("IsDataLoadingModule", false, entry.IsDataLoadingModule);

			entry.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			AssertEquals("IsG7ExportDeclaration", false, entry.IsG7ExportDeclaration);
			AssertEquals("IsImportEDIRelease", true, entry.IsImportEDIRelease);
			AssertEquals("IsDataLoadingModule", false, entry.IsDataLoadingModule);

			entry.CH_MessageType = MessageTypeList.Codes.DataLoadingModule;
			AssertEquals("IsG7ExportDeclaration", false, entry.IsG7ExportDeclaration);
			AssertEquals("IsImportEDIRelease", false, entry.IsImportEDIRelease);
			AssertEquals("IsDataLoadingModule", true, entry.IsDataLoadingModule);
		}

		public void TestConveyanceIdentificationNos()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice1 = dec.Invoices.AddNew();
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			var invoice2 = dec.Invoices.AddNew();
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			var line1 = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = line1.PK;
			invoiceLine1.CA_ConveyanceIdentificationNumber = "12345678";
			var line2 = entryHeader.MergedLines.AddNew();
			invoiceLine2.JI_CL = line2.PK;
			invoiceLine2.CA_ConveyanceIdentificationNumber = "87654321";
			AssertEquals("12345678,87654321", entryHeader.ConveyanceIdentificationNos);
		}

		public void TestLicenceAndPermits()
		{
			var company = GlbCompany.GetCurrentCompany(Factory);
			var companyProxy = company.OrgProxy;

			var canada = Factory.Load<RefCountry>(Core.Constants.CountryGuids.Canada);
			companyProxy.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.ExportLicenceNumber, "XLICENCE", canada);
			var declaration = Factory.New<JobDeclaration>();
			var exporter = Factory.New<OrgHeader>();
			var cusCode = exporter.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.AuthorizationID, "ASS312");
			declaration.JE_OH_Supplier = exporter.PK;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var dp1 = declaration.Permits.AddNew();
			dp1.CY_Data = "DP1";
			var dp2 = declaration.Permits.AddNew();
			dp2.CY_Data = "DP2";
			var lp1 = invoiceLine.Permits.AddNew();
			lp1.CY_Data = "LP1";
			var lp2 = invoiceLine.Permits.AddNew();
			lp2.CY_Data = "LP2";
			var lp3 = invoiceLine.Permits.AddNew();
			lp3.CY_Data = "DP1";
			AssertEquals("LicenceAndPermits", "DP1/DP2/LP1/LP2/XLICENCE/Auth.ID:ASS312", entry.LicenceAndPermits);
		}

		public void TestExportLicenceNumber()
		{
			var company = GlbCompany.GetCurrentCompany(Factory);
			var companyProxy = company.OrgProxy;
			var branch = company.Branches.AddNew();
			var branchProxy = branch.OrgProxy;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("ExportLicenceNumber is blank by default", "", entry.ExportLicenceNumber);

			var canada = Factory.Load<RefCountry>(Core.Constants.CountryGuids.Canada);
			companyProxy.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.ExportLicenceNumber, "XLICENCE", canada);
			var query = new ZQuery(ZArchitecture.Schema.OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = new BusinessObjectFactory().LoadTop1<OrgHeader>(query).PK;
			AssertEquals("ExportLicenceNumber", "XLICENCE", entry.ExportLicenceNumber);

			entry.ResetCachedValuesForTest();
			declaration.ResetCachedValuesForTest();
			companyProxy.CustomsCodes.RemoveAndDeleteAll();
			branchProxy.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.ExportLicenceNumber, "YLICENCE", canada);
			AssertEquals("ExportLicenceNumber", "YLICENCE", entry.ExportLicenceNumber);
		}

		public void TestAddStatusCustomsLogIfRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.DataLoadingModule;
			Factory.Save();
			entry.CH_Status = MessageStatusList.Codes.Sent;
			Factory.Save();
			AssertEquals("1 log added on MessageStatus changed for DLM", 1, entry.Logs.GetAllLogs().Count);

			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.G7Export;
			entry.CH_Status = MessageStatusList.Codes.Sent;
			Factory.Save();
			AssertEquals("No log added on MessageStatus changed for other messages", 0, entry.Logs.GetAllLogs().Count);

			entry.CH_EntryStatus = EntryStatusList.Codes.Cancelled;
			Factory.Save();
			AssertEquals("1 log added on EntryStatus changed for other messages", 1, entry.Logs.GetAllLogs().Count);
		}

		public void TestAddCustomsClearedEventWhenB3AcceptedWithAttachLVXJobs()
		{
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest())
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);

				var lvsJob = Factory.New<JobDeclaration>();
				lvsJob.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				var lvxJob = Factory.New<JobDeclaration>();
				lvxJob.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
				LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(lvxJob.LVXInvoiceHeader, lvsJob);
				var trigger = lvxJob.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "trigger auto-rating";
				trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomsEntryStatus.Code;
				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AutoRateCostsAndRevenue;
				Factory.Save();
				var entry1 = lvsJob.CustomsEntryHeaders.AddNew();
				entry1.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				entry1.CH_Status = MessageStatusList.Codes.Sent;
				var entry2 = lvxJob.CustomsEntryHeaders.AddNew();
				entry2.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				Factory.Save();
				AssertNull("No log added to LVS job", entry1.Logs.MostRecentLogByEventTime(AutoEvents.CustomsEntryStatus, "CLR"));
				AssertNull("No log added to attached LVX job", entry2.Logs.MostRecentLogByEventTime(AutoEvents.CustomsEntryStatus, "CLR"));
				entry1.CH_EntryStatus = B3EntryStatusList.Codes.Accepted;
				Factory.Save();
				AssertNotNull("B3 clear generates Customs Entry Status (Cleared) event to LVS job", entry1.Logs.MostRecentLogByEventTime(AutoEvents.CustomsEntryStatus, "CLR"));
				AssertNotNull("B3 clear generates Customs Entry Status (Cleared) event to attached LVX job", entry2.Logs.MostRecentLogByEventTime(AutoEvents.CustomsEntryStatus, "CLR"));
				Assert("Trigger", !trigger.P9_ActualDate.IsEmpty);
			}
		}

		#region TestDeriveDeclarationStatusIsCalledOnSavingWhenMessagesChanged

		public void TestDeriveDeclarationStatusIsCalledOnSavingWhenMessagesChangedExport()
		{
			var declaration = Factory.New<DummyJobDeclaration>();
			declaration.JE_MessageTypeReturns = JobMessageTypeList.Codes.Export;
			var entry = declaration.CustomsEntryHeaders.AddNew();

			declaration.DeriveExportDeclarationStatusCount = 0;
			Factory.Save();
			Assert($"DeriveExportDeclarationStatus should not be called.", declaration.DeriveExportDeclarationStatusCount == 0);

			var message = entry.Messages.AddNew();
			message.EM_ApplicationCode = "A";
			Factory.Save();
			Assert($"DeriveExportDeclarationStatus should have been called at least once.", declaration.DeriveExportDeclarationStatusCount > 0);
		}

		public void TestDeriveDeclarationStatusIsCalledOnSavingWhenMessagesChangedImport()
		{
			var declaration = Factory.New<DummyJobDeclaration>();
			declaration.JE_MessageTypeReturns = JobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();

			declaration.DeriveImportDeclarationStatusCount = 0;
			Factory.Save();
			Assert($"DeriveImportDeclarationStatus should not be called.", declaration.DeriveImportDeclarationStatusCount == 0);

			declaration.DeriveImportDeclarationStatusCount = 0;
			var message = entry.Messages.AddNew();
			message.EM_ApplicationCode = "A";
			Factory.Save();
			Assert($"DeriveImportDeclarationStatus should have been called at least once.", declaration.DeriveImportDeclarationStatusCount > 0);
		}

		sealed class DummyJobDeclaration : JobDeclaration
		{
			public DummyJobDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public ZString JE_MessageTypeReturns { get; set; } = string.Empty;
			public int DeriveExportDeclarationStatusCount { get; set; }
			public int DeriveImportDeclarationStatusCount { get; set; }

			public override ZString JE_MessageType { get => JE_MessageTypeReturns; set => JE_MessageTypeReturns = value; }

			protected override void DeriveExportDeclarationStatus()
			{
				DeriveExportDeclarationStatusCount++;
				base.DeriveExportDeclarationStatus();
			}

			protected override void DeriveImportDeclarationStatus()
			{
				DeriveImportDeclarationStatusCount++;
				base.DeriveImportDeclarationStatus();
			}
		}
		#endregion

		public void TestHasBeenWithdrawn()
		{
			var entry = Factory.New<CusEntryHeader>();
			entry.CH_MessageType = MessageTypeList.Codes.DataLoadingModule;
			AssertEquals("Withdrawn is currently not use", false, entry.HasBeenWithdrawn);

			entry.CH_MessageType = MessageTypeList.Codes.G7Export;
			Assert("Has not been withdrawn", !entry.HasBeenWithdrawn);

			entry.CH_EntryStatus = EntryStatusList.Codes.Cancelled;
			Assert("Has been withdrawn", entry.HasBeenWithdrawn);

			entry.CH_EntryStatus = EntryStatusList.Codes.Clear;
			Assert("Has not been withdrawn", !entry.HasBeenWithdrawn);
		}

		public void TestShouldLogCustomsClearedToDeclarationOrShipment()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entry = declaration.CustomsEntryHeaders.AddNew();

			entry.CH_MessageType = MessageTypeList.Codes.DataLoadingModule;
			AssertEquals("ShouldLogCustomsClearedToDeclarationOrShipment for DLM", false, entry.ShouldLogCustomsClearedToDeclarationOrShipment);

			entry.CH_MessageType = MessageTypeList.Codes.G7Export;
			AssertEquals("ShouldLogCustomsClearedToDeclarationOrShipment for G7Export", true, entry.ShouldLogCustomsClearedToDeclarationOrShipment);

			entry.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			AssertEquals("ShouldLogCustomsClearedToDeclarationOrShipment for release", true, entry.ShouldLogCustomsClearedToDeclarationOrShipment);
		}

		public void TestHasBeenLodgedAtCustoms()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			//DLM
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.DataLoadingModule;
			Factory.Save();
			AssertEquals("HasBeenLodgedAtCustoms", false, entry.HasBeenLodgedAtCustoms);
			entry.CH_Status = MessageStatusList.Codes.Sent;
			Factory.Save();
			AssertEquals("HasBeenLodgedAtCustoms", true, entry.HasBeenLodgedAtCustoms);

			//G7Export
			entry.CH_MessageType = MessageTypeList.Codes.G7Export;
			AssertEquals("HasBeenLodgedAtCustoms", false, entry.HasBeenLodgedAtCustoms);

			entry.CH_EntryStatus = EntryStatusList.Codes.Clear;
			AssertEquals("HasBeenLodgedAtCustoms", true, entry.HasBeenLodgedAtCustoms);

			entry.CH_EntryStatus = EntryStatusList.Codes.Cancelled;
			AssertEquals("HasBeenLodgedAtCustoms", false, entry.HasBeenLodgedAtCustoms);

			//EDIRelease
			entry.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			AssertEquals("HasBeenLodgedAtCustoms", false, entry.HasBeenLodgedAtCustoms);

			entry.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.Y51ReleaseDocumentsRequired;
			AssertEquals("HasBeenLodgedAtCustoms", true, entry.HasBeenLodgedAtCustoms);

			entry.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.Cancelled;
			AssertEquals("HasBeenLodgedAtCustoms", false, entry.HasBeenLodgedAtCustoms);

			entry.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.GoodsReleased;
			AssertEquals("HasBeenLodgedAtCustoms", true, entry.HasBeenLodgedAtCustoms);

			//B3CUSDEC
			entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entry.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.Cancelled;
			AssertEquals("HasBeenLodgedAtCustoms", false, entry.HasBeenLodgedAtCustoms);

			entry.CH_EntryStatus = B3EntryStatusList.Codes.Accepted;
			AssertEquals("HasBeenLodgedAtCustoms", true, entry.HasBeenLodgedAtCustoms);

			entry.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.Cancelled;
			AssertEquals("HasBeenLodgedAtCustoms", false, entry.HasBeenLodgedAtCustoms);

			entry.CH_EntryStatus = B3EntryStatusList.Codes.Confirmed;
			AssertEquals("HasBeenLodgedAtCustoms", true, entry.HasBeenLodgedAtCustoms);

			// CAD
			entry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			entry.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.Cancelled;
			AssertEquals("HasBeenLodgedAtCustoms", false, entry.HasBeenLodgedAtCustoms);

			entry.CH_EntryStatus = CADEntryStatusList.Codes.Approved;
			AssertEquals("HasBeenLodgedAtCustoms", true, entry.HasBeenLodgedAtCustoms);
		}

		public void TestPopulateEntrySubmittedDateIfRequired_WithManualSubmission()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			declaration.JE_CustomsOffice = "351";
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "00006789";
			var b3Invoice = declaration.Invoices.AddNew();
			b3Invoice.JZ_RX_NKInvoice_Currency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates).RX_Code;
			var b3InvoiceLine = b3Invoice.JobComInvoiceLines.AddNew();
			b3InvoiceLine.JI_Tariff = "123456789";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			var cusEntryHeader = declaration.B3EntryHeader;
			var lineOne = cusEntryHeader.AllEntryLines.First();
			lineOne.Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.TotalDutyAmount, 100m);
			lineOne.Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.TotalSIMAAmount, 200m);
			lineOne.Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.TotalGSTAmount, 300m);

			var manualSubmissionDate = new ZDate(2017, 04, 05);

			var manualSubmissionSupport = declaration as Integration.Customs.CA.IManualSubmissionSupport;
			var manualSubmissionBo = new ManualSubmissionBO(declaration, MessageTypeList.Codes.B3CUSDEC, Factory);

			var entrySubmissionBo = manualSubmissionBo.CurrentEntrySubmissionBO;
			entrySubmissionBo.ManualSubmissionDate = manualSubmissionDate;
			manualSubmissionSupport.ManualSubmission(manualSubmissionBo.GetCusEntrySubmissionBOs());
			Factory.Save();

			// manual submission will be overwritten with real electronic submission
			cusEntryHeader.CH_JE = declaration.PK;
			cusEntryHeader.PopulateEntrySubmittedDateIfRequired();
			AssertEquals("CH_EntrySubmittedDate is set to ZDateTime.Now", true, cusEntryHeader.CH_EntrySubmittedDate > ZDateTime.Now.AddSeconds(-10));
			AssertEquals("JE_EntrySubmittedDate is set to ZDateTime.Now", true, declaration.JE_EntrySubmittedDate == cusEntryHeader.CH_EntrySubmittedDate);
		}

		public void TestPopulateEntrySubmittedDateIfRequired_WithOutdatedManualSubmission()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			declaration.JE_CustomsOffice = "351";
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "00006789";
			var b3Invoice = declaration.Invoices.AddNew();
			b3Invoice.JZ_RX_NKInvoice_Currency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates).RX_Code;
			var b3InvoiceLine = b3Invoice.JobComInvoiceLines.AddNew();
			b3InvoiceLine.JI_Tariff = "123456789";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			var cusEntryHeader = declaration.B3EntryHeader;
			var lineOne = cusEntryHeader.AllEntryLines.First();
			lineOne.Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.TotalDutyAmount, 100m);
			lineOne.Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.TotalSIMAAmount, 200m);
			lineOne.Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.TotalGSTAmount, 300m);

			var manualSubmittedDate = new ZDate(2017, 04, 05);
			var submittedDate = manualSubmittedDate.AddDays(1);

			var manualSubmissionSupport = declaration as Integration.Customs.CA.IManualSubmissionSupport;
			var manualSubmissionBo = new ManualSubmissionBO(declaration, MessageTypeList.Codes.B3CUSDEC, Factory);

			var entrySubmissionBo = manualSubmissionBo.CurrentEntrySubmissionBO;
			entrySubmissionBo.ManualSubmissionDate = manualSubmittedDate;
			manualSubmissionSupport.ManualSubmission(manualSubmissionBo.GetCusEntrySubmissionBOs());
			Factory.Save();

			// manual submission had already been be overwritten with an EDI, therefore previous submission date will not be changed
			cusEntryHeader.CH_JE = declaration.PK;
			cusEntryHeader.CH_EntrySubmittedDate = submittedDate;
			cusEntryHeader.PopulateEntrySubmittedDateIfRequired();
			AssertEquals("Previous EDI CH_EntrySubmittedDate is not updated", true, cusEntryHeader.CH_EntrySubmittedDate == submittedDate);
		}

		public void TestPopulateEntrySubmittedDateIfRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			var mockEntry = Factory.NewMoq<CusEntryHeader>();
			var entry = mockEntry.Object;

			// without declaration
			AssertEquals("CH_EntrySubmittedDate.IsEmpty", true, entry.CH_EntrySubmittedDate.IsEmpty);
			AssertEquals("JE_EntrySubmittedDate.IsEmpty", true, declaration.JE_EntrySubmittedDate.IsEmpty);
			entry.PopulateEntrySubmittedDateIfRequired();
			AssertEquals("CH_EntrySubmittedDate is set to ZDateTime.Now", true, entry.CH_EntrySubmittedDate > ZDateTime.Now.AddSeconds(-10));
			AssertEquals("JE_EntrySubmittedDate.IsEmpty", true, declaration.JE_EntrySubmittedDate.IsEmpty);

			// with declaration
			entry.CH_JE = declaration.PK;
			entry.CH_EntrySubmittedDate = ZDateTime.Empty;
			entry.PopulateEntrySubmittedDateIfRequired();
			AssertEquals("CH_EntrySubmittedDate is set to ZDateTime.Now", true, entry.CH_EntrySubmittedDate > ZDateTime.Now.AddSeconds(-10));
			AssertEquals("JE_EntrySubmittedDate is set to ZDateTime.Now", true, declaration.JE_EntrySubmittedDate == entry.CH_EntrySubmittedDate);

			// with already set declaration
			entry.CH_EntrySubmittedDate = ZDateTime.Empty;
			var declarationTime = ZDateTime.Now.AddMinutes(-30);
			declaration.JE_EntrySubmittedDate = declarationTime;
			entry.PopulateEntrySubmittedDateIfRequired();
			AssertEquals("CH_EntrySubmittedDate is set to ZDateTime.Now", true, entry.CH_EntrySubmittedDate > ZDateTime.Now.AddSeconds(-10));
			AssertEquals("JE_EntrySubmittedDate is unchanged", true, declaration.JE_EntrySubmittedDate == declarationTime);
		}

		public void TestPopulateEntrySubmittedDateIfRequiredWithParam()
		{
			var declaration = Factory.New<JobDeclaration>();
			var mockEntry = Factory.NewMoq<CusEntryHeader>();
			var entry = mockEntry.Object;

			var submittedDateTime = ZDateTime.Now.AddMinutes(-15);

			// without declaration
			AssertEquals("CH_EntrySubmittedDate.IsEmpty", true, entry.CH_EntrySubmittedDate.IsEmpty);
			AssertEquals("JE_EntrySubmittedDate.IsEmpty", true, declaration.JE_EntrySubmittedDate.IsEmpty);
			entry.PopulateEntrySubmittedDateIfRequired(submittedDateTime);
			AssertEquals("CH_EntrySubmittedDate is set to ZDateTime.Now", true, entry.CH_EntrySubmittedDate == submittedDateTime);
			AssertEquals("JE_EntrySubmittedDate.IsEmpty", true, declaration.JE_EntrySubmittedDate.IsEmpty);

			// with declaration
			entry.CH_JE = declaration.PK;
			entry.CH_EntrySubmittedDate = ZDateTime.Empty;
			entry.PopulateEntrySubmittedDateIfRequired(submittedDateTime);
			AssertEquals("CH_EntrySubmittedDate is set to ZDateTime.Now", true, entry.CH_EntrySubmittedDate == submittedDateTime);
			AssertEquals("JE_EntrySubmittedDate is set to CH_EntrySubmittedDate", true, declaration.JE_EntrySubmittedDate == entry.CH_EntrySubmittedDate);

			// with already set declaration
			entry.CH_EntrySubmittedDate = ZDateTime.Empty;
			var declarationTime = ZDateTime.Now.AddMinutes(-30);
			declaration.JE_EntrySubmittedDate = declarationTime;
			entry.PopulateEntrySubmittedDateIfRequired(submittedDateTime);
			AssertEquals("CH_EntrySubmittedDate is set to ZDateTime.Now", true, entry.CH_EntrySubmittedDate == submittedDateTime);
			AssertEquals("JE_EntrySubmittedDate is unchanged", true, declaration.JE_EntrySubmittedDate == declarationTime);

			// with already set declaration is newer
			entry.CH_EntrySubmittedDate = ZDateTime.Empty;
			var declarationTime2 = ZDateTime.Now;
			declaration.JE_EntrySubmittedDate = declarationTime2;
			entry.PopulateEntrySubmittedDateIfRequired(submittedDateTime);
			AssertEquals("CH_EntrySubmittedDate is set to ZDateTime.Now", true, entry.CH_EntrySubmittedDate == submittedDateTime);
			AssertEquals("JE_EntrySubmittedDate is set to CH_EntrySubmittedDate", true, declaration.JE_EntrySubmittedDate == entry.CH_EntrySubmittedDate);
		}

		public void TestCustomsClearedEvent()
		{
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest())
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.CH_MessageType = MessageTypeList.Codes.EDIRelease;
				Factory.Save();
				((IEDIReleaseMessageAttachee)entry).SettingReleaseDateWithStatusUpdate = true;
				declaration.JE_EntryAuthorisationDate = new ZDateTime(2011, 1, 2);
				((IEDIReleaseMessageAttachee)entry).SettingReleaseDateWithStatusUpdate = false;
				entry.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.Y51ReleaseDocumentsRequired;
				AssertNull("No declaration event", declaration.Logs.MostRecentLogByEventTime(Events.CustomsCleared));
				AssertNull("No entry event", entry.Logs.MostRecentLogByEventTime(Events.CustomsCleared));
				Factory.Save();
				var clearedEvent = declaration.Logs.MostRecentLogByEventTime(Events.CustomsCleared);
				AssertNotNull("Customs Cleared event should exist on declaration", clearedEvent);
				clearedEvent = entry.Logs.MostRecentLogByEventTime(Events.CustomsCleared);
				AssertNotNull("Customs Cleared event should exist on entry header", clearedEvent);
				AssertEquals("Cleared date", new ZDateTime(2011, 1, 2), clearedEvent.SL_EventTime);
				AssertEquals("Cleared status code", EDIReleaseImportEntryStatusList.Codes.Y51ReleaseDocumentsRequired, clearedEvent.SL_Reference);
				declaration.Logs.CancelAll();
				entry.Logs.CancelAll();
				entry.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.GoodsDetained;
				Factory.Save();
				AssertNull("No declaration event", declaration.Logs.MostRecentLogByEventTime(Events.CustomsCleared));
				AssertNull("No entry event", entry.Logs.MostRecentLogByEventTime(Events.CustomsCleared));
				declaration.Logs.CancelAll();
				entry.Logs.CancelAll();
				entry.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.GoodsReleased;
				Factory.Save();
				AssertNotNull("Declaration event", declaration.Logs.MostRecentLogByEventTime(Events.CustomsCleared));
				AssertNotNull("Entry event", entry.Logs.MostRecentLogByEventTime(Events.CustomsCleared));

				declaration.Logs.CancelAll();
				entry.Logs.CancelAll();
				var entry2 = declaration.CustomsEntryHeaders.AddNew();
				entry2.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				entry2.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.GoodsReleased;
				Factory.Save();
				AssertNull("B3 clear does not generate Declaration event", declaration.Logs.MostRecentLogByEventTime(Events.CustomsCleared));
				AssertNull("B3 clear does not generate Entry event", entry.Logs.MostRecentLogByEventTime(Events.CustomsCleared));
			}
		}

		public void TestIsFeePaidByBroker()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_PaymentMethod = Enterprise.Customs.Business.PaymentPartyCodeDescriptionList.Codes.Default;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			AssertIsFeePaidByBroker(declaration, "No Importer", true, true, true, false, false);
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			declaration.JE_OH_Importer = importer.PK;
			declaration.TransactionNumber.AccountSecurityCode = "40000";
			declaration.TransactionNumber.SequentialNumber = "04228";
			declaration.ImporterAddInfo.ZO_IsImporterDirectPayment = true;
			AssertIsFeePaidByBroker(declaration, "Importer with security", false, false, true, false, false);
			declaration.ImporterAddInfo.ZO_IsGSTDirectPayment = true;
			AssertIsFeePaidByBroker(declaration, "Importer with security and pays GST", false, false, false, false, false);
			declaration.ImporterAddInfo.ZO_IsImporterDirectPayment = false;
			declaration.ImporterAddInfo.ZO_IsGSTDirectPayment = false;
			AssertIsFeePaidByBroker(declaration, "Importer without security", true, true, true, false, false);
			declaration.ImporterAddInfo.ZO_IsGSTDirectPayment = true;
			AssertIsFeePaidByBroker(declaration, "Importer without security but pays GST", true, false, false, false, false);

			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.TransactionNumber.AccountSecurityCode = "40000";
			declaration.TransactionNumber.SequentialNumber = "04228";
			AssertIsFeePaidByBroker(declaration, "No Importer", true, true, true, false, false);
			declaration.JE_OH_Importer = importer.PK;
			declaration.TransactionNumber.AccountSecurityCode = "40000";
			declaration.TransactionNumber.SequentialNumber = "04228";
			declaration.ImporterAddInfo.ZO_IsImporterDirectPayment = true;
			declaration.ImporterAddInfo.ZO_IsGSTDirectPayment = false;
			AssertIsFeePaidByBroker(declaration, "Importer with security", false, false, true, false, false);
			declaration.ImporterAddInfo.ZO_IsGSTDirectPayment = true;
			declaration.ImporterAddInfo.ZO_IsGSTDirectAutoRated = true;
			AssertIsFeePaidByBroker(declaration, "Importer with security and pays GST", false, false, false, false, true);
			declaration.ImporterAddInfo.ZO_IsImporterDirectPayment = false;
			declaration.ImporterAddInfo.ZO_IsGSTDirectPayment = false;
			AssertIsFeePaidByBroker(declaration, "Importer without security", true, true, true, false, false);
			declaration.ImporterAddInfo.ZO_IsGSTDirectPayment = true;
			declaration.ImporterAddInfo.ZO_IsGSTDirectAutoRated = true;
			AssertIsFeePaidByBroker(declaration, "Importer without security but pays GST", true, false, false, true, true);

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Warehouse10;
			foreach (Enterprise.Registry.Business.Customs.EntryChargeType chargeType in EntryChargeTypeList.GetList("CA"))
			{
				Assert("Into W/H transaction", !declaration.ActiveEntryHeaders[0].IsFeePaidByBroker(chargeType.Code, "", null));
			}

			entry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse101;
			foreach (Enterprise.Registry.Business.Customs.EntryChargeType chargeType in EntryChargeTypeList.GetList("CA"))
			{
				Assert("Into W/H transaction", !declaration.ActiveEntryHeaders[0].IsFeePaidByBroker(chargeType.Code, "", null));
			}
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse102;
			foreach (Enterprise.Registry.Business.Customs.EntryChargeType chargeType in EntryChargeTypeList.GetList("CA"))
			{
				Assert("Into W/H transaction", !declaration.ActiveEntryHeaders[0].IsFeePaidByBroker(chargeType.Code, "", null));
			}
		}

		public void TestIsFeePaidByBroker_CADEntry()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_PaymentMethod = Enterprise.Customs.Business.PaymentPartyCodeDescriptionList.Codes.Default;
				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
				AssertIsFeePaidByBroker(declaration, "No Importer", false, false, false, false, false, false);
				var importer = Factory.New<OrgHeader>();
				importer.FillWithValidTestData();
				declaration.JE_OH_Importer = importer.PK;
				declaration.TransactionNumber.AccountSecurityCode = "40000";
				declaration.TransactionNumber.SequentialNumber = "04228";
				declaration.ImporterAddInfo.ZO_IsImporterDirectPayment = true;
				AssertIsFeePaidByBroker(declaration, "CAD BrokerToPay = N, Importer with security", false, false, false, false, false, false);
				declaration.ImporterAddInfo.ZO_IsGSTDirectPayment = true;
				AssertIsFeePaidByBroker(declaration, "CAD BrokerToPay = N, Importer with security and pays GST", false, false, false, false, false, false);

				declaration.ImporterAddInfo.ZO_CADIsBrokerToPay = true;
				declaration.ImporterAddInfo.ZO_IsImporterDirectPayment = false;
				declaration.ImporterAddInfo.ZO_IsGSTDirectPayment = false;
				AssertIsFeePaidByBroker(declaration, "CAD BrokerToPay = Y, Importer without security", true, true, true, true, true, true, true, true, true);

				declaration.ImporterAddInfo.ZO_CADIsBrokerToPay = false;
				AssertIsFeePaidByBroker(declaration, "CAD BrokerToPay = Y, Importer without security", false, false, false, false, false, false, false, false, false);
			}
		}

		void AssertIsFeePaidByBroker(JobDeclaration declaration, string importerStatusComment, bool defaultExpectedDutyResult, bool defaultExpectedGSTResult, bool brokerPaysGSTResult, bool defaultExpectedDirectGSTResult, bool brokerPaysDirectGSTResult,
			bool brokerExpectedDutyResult = true, bool importerExpectedDutyResult = false, bool importerExpectedGSTResult = false, bool importerExpectedGSDResult = false)
		{
			declaration.JE_PaymentMethod = Enterprise.Customs.Business.PaymentPartyCodeDescriptionList.Codes.Default;
			AssertEquals(importerStatusComment + ", JE_PaymentMethod is default", defaultExpectedDutyResult, declaration.ActiveEntryHeaders[0].IsFeePaidByBroker(EntryChargeTypeList.Codes.TotalDutyAmount, "", null));
			AssertEquals(importerStatusComment + ", JE_PaymentMethod is default", defaultExpectedGSTResult, declaration.ActiveEntryHeaders[0].IsFeePaidByBroker(EntryChargeTypeList.Codes.TotalGSTAmount, "", null));
			AssertEquals(importerStatusComment + ", JE_PaymentMethod is default", defaultExpectedDirectGSTResult, declaration.ActiveEntryHeaders[0].IsFeePaidByBroker(EntryChargeTypeList.Codes.TotalGSTDirectAmount, "", null));
			declaration.JE_PaymentMethod = Enterprise.Customs.Business.PaymentPartyCodeDescriptionList.Codes.Broker;
			AssertEquals(importerStatusComment + ", JE_PaymentMethod is broker", brokerExpectedDutyResult, declaration.ActiveEntryHeaders[0].IsFeePaidByBroker(EntryChargeTypeList.Codes.TotalDutyAmount, "", null));
			AssertEquals(importerStatusComment + ", JE_PaymentMethod is broker", brokerPaysGSTResult, declaration.ActiveEntryHeaders[0].IsFeePaidByBroker(EntryChargeTypeList.Codes.TotalGSTAmount, "", null));
			AssertEquals(importerStatusComment + ", JE_PaymentMethod is broker", brokerPaysDirectGSTResult, declaration.ActiveEntryHeaders[0].IsFeePaidByBroker(EntryChargeTypeList.Codes.TotalGSTDirectAmount, "", null));
			declaration.JE_PaymentMethod = Enterprise.Customs.Business.PaymentPartyCodeDescriptionList.Codes.Importer;
			AssertEquals(importerStatusComment + ", JE_PaymentMethod is importer", importerExpectedDutyResult, declaration.ActiveEntryHeaders[0].IsFeePaidByBroker(EntryChargeTypeList.Codes.TotalDutyAmount, "", null));
			AssertEquals(importerStatusComment + ", JE_PaymentMethod is importer", importerExpectedGSTResult, declaration.ActiveEntryHeaders[0].IsFeePaidByBroker(EntryChargeTypeList.Codes.TotalGSTAmount, "", null));
			AssertEquals(importerStatusComment + ", JE_PaymentMethod is importer", importerExpectedGSDResult, declaration.ActiveEntryHeaders[0].IsFeePaidByBroker(EntryChargeTypeList.Codes.TotalGSTDirectAmount, "", null));

			if (declaration.ImporterAddInfo != null)
			{
				declaration.JE_PaymentMethod = Enterprise.Customs.Business.PaymentPartyCodeDescriptionList.Codes.Broker;
				declaration.ImporterAddInfo.ZO_AccountSecurityNumber = "40000";
				AssertEquals("Importer has account security code", importerExpectedDutyResult, declaration.ActiveEntryHeaders[0].IsFeePaidByBroker(EntryChargeTypeList.Codes.TotalDutyAmount, "", null));
				AssertEquals("Importer has account security code", importerExpectedGSTResult, declaration.ActiveEntryHeaders[0].IsFeePaidByBroker(EntryChargeTypeList.Codes.TotalGSTAmount, "", null));
				AssertEquals("Importer has account security code", importerExpectedGSDResult, declaration.ActiveEntryHeaders[0].IsFeePaidByBroker(EntryChargeTypeList.Codes.TotalGSTDirectAmount, "", null));
				declaration.ImporterAddInfo.ZO_AccountSecurityNumber = ZString.Empty;
			}
		}

		public void TestIsFeePaidBrokerWithSuppliedDebtor()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_PaymentMethod = Enterprise.Customs.Business.PaymentPartyCodeDescriptionList.Codes.Default;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var buyer = Factory.New<OrgHeader>();
			buyer.FillWithValidTestData();
			var importerAddInfo = OrgImpAddInfo.Get(buyer);
			importerAddInfo.ZO_IsImporterDirectPayment = true;
			AssertIsFeePaidByBroker(buyer, declaration, "Buyer with security", false, false, true, false, false);
			importerAddInfo.ZO_IsGSTDirectPayment = true;
			AssertIsFeePaidByBroker(buyer, declaration, "Buyer with security and pays GST", false, false, false, false, false);
			importerAddInfo.ZO_IsImporterDirectPayment = false;
			importerAddInfo.ZO_IsGSTDirectPayment = false;
			AssertIsFeePaidByBroker(buyer, declaration, "Buyer without security", true, true, true, false, false);
			importerAddInfo.ZO_IsGSTDirectPayment = true;
			AssertIsFeePaidByBroker(buyer, declaration, "Buyer without security but pays GST", true, false, false, false, false);
			importerAddInfo.ZO_IsImporterDirectPayment = true;
			importerAddInfo.ZO_IsGSTDirectPayment = false;
			AssertIsFeePaidByBroker(buyer, declaration, "Buyer with security", false, false, true, false, false);
			importerAddInfo.ZO_IsGSTDirectPayment = true;
			importerAddInfo.ZO_IsGSTDirectAutoRated = true;
			AssertIsFeePaidByBroker(buyer, declaration, "Buyer with security and pays GST", false, false, false, false, true);
			importerAddInfo.ZO_IsImporterDirectPayment = false;
			importerAddInfo.ZO_IsGSTDirectPayment = false;
			AssertIsFeePaidByBroker(buyer, declaration, "Buyer without security", true, true, true, false, false);
			importerAddInfo.ZO_IsGSTDirectPayment = true;
			importerAddInfo.ZO_IsGSTDirectAutoRated = true;
			AssertIsFeePaidByBroker(buyer, declaration, "Buyer without security but pays GST", true, false, false, true, true);

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Warehouse10;
			foreach (Enterprise.Registry.Business.Customs.EntryChargeType chargeType in EntryChargeTypeList.GetList("CA"))
			{
				Assert("Into W/H transaction", !declaration.ActiveEntryHeaders[0].IsFeePaidByBroker(chargeType.Code, buyer.PK));
			}

			entry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse101;
			foreach (Enterprise.Registry.Business.Customs.EntryChargeType chargeType in EntryChargeTypeList.GetList("CA"))
			{
				Assert("Into W/H transaction", !declaration.ActiveEntryHeaders[0].IsFeePaidByBroker(chargeType.Code, buyer.PK));
			}
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse102;
			foreach (Enterprise.Registry.Business.Customs.EntryChargeType chargeType in EntryChargeTypeList.GetList("CA"))
			{
				Assert("Into W/H transaction", !declaration.ActiveEntryHeaders[0].IsFeePaidByBroker(chargeType.Code, buyer.PK));
			}
		}

		public void TestIsFeePaidBrokerWithSuppliedDebtor_CADEntry()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_PaymentMethod = Enterprise.Customs.Business.PaymentPartyCodeDescriptionList.Codes.Default;
				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
				var buyer = Factory.New<OrgHeader>();
				buyer.FillWithValidTestData();
				var importerAddInfo = OrgImpAddInfo.Get(buyer);
				importerAddInfo.ZO_IsImporterDirectPayment = true;
				AssertIsFeePaidByBroker(buyer, declaration, "CAD BrokerToPay = N, Buyer with security", false, false, false, false, false, false);
				importerAddInfo.ZO_IsGSTDirectPayment = true;
				AssertIsFeePaidByBroker(buyer, declaration, "CAD BrokerToPay = N, Buyer with security and pays GST", false, false, false, false, false, false);

				importerAddInfo.ZO_CADIsBrokerToPay = true;
				importerAddInfo.ZO_IsImporterDirectPayment = false;
				importerAddInfo.ZO_IsGSTDirectPayment = false;
				AssertIsFeePaidByBroker(buyer, declaration, "CAD BrokerToPay = Y, Buyer without security", true, true, true, true, true, true, true, true, true);

				importerAddInfo.ZO_CADIsBrokerToPay = false;
				AssertIsFeePaidByBroker(buyer, declaration, "CAD BrokerToPay = Y, Buyer without security", false, false, false, false, false, false, false, false, false);
			}
		}

		void AssertIsFeePaidByBroker(OrgHeader buyer, JobDeclaration declaration, string importerStatusComment, bool defaultExpectedDutyResult, bool defaultExpectedGSTResult, bool brokerPaysGSTResult, bool defaultExpectedDirectGSTResult, bool brokerPaysDirectGSTResult,
			bool brokerExpectedDutyResult = true, bool importerExpectedDutyResult = false, bool importerExpectedGSTResult = false, bool importerExpectedGSDResult = false)
		{
			declaration.JE_PaymentMethod = Enterprise.Customs.Business.PaymentPartyCodeDescriptionList.Codes.Default;
			AssertEquals(importerStatusComment + ", JE_PaymentMethod is default", defaultExpectedDutyResult, declaration.ActiveEntryHeaders[0].IsFeePaidByBroker(EntryChargeTypeList.Codes.TotalDutyAmount, buyer.PK));
			AssertEquals(importerStatusComment + ", JE_PaymentMethod is default", defaultExpectedGSTResult, declaration.ActiveEntryHeaders[0].IsFeePaidByBroker(EntryChargeTypeList.Codes.TotalGSTAmount, buyer.PK));
			AssertEquals(importerStatusComment + ", JE_PaymentMethod is default", defaultExpectedDirectGSTResult, declaration.ActiveEntryHeaders[0].IsFeePaidByBroker(EntryChargeTypeList.Codes.TotalGSTDirectAmount, buyer.PK));
			declaration.JE_PaymentMethod = Enterprise.Customs.Business.PaymentPartyCodeDescriptionList.Codes.Broker;
			AssertEquals(importerStatusComment + ", JE_PaymentMethod is broker", brokerExpectedDutyResult, declaration.ActiveEntryHeaders[0].IsFeePaidByBroker(EntryChargeTypeList.Codes.TotalDutyAmount, buyer.PK));
			AssertEquals(importerStatusComment + ", JE_PaymentMethod is broker", brokerPaysGSTResult, declaration.ActiveEntryHeaders[0].IsFeePaidByBroker(EntryChargeTypeList.Codes.TotalGSTAmount, buyer.PK));
			AssertEquals(importerStatusComment + ", JE_PaymentMethod is broker", brokerPaysDirectGSTResult, declaration.ActiveEntryHeaders[0].IsFeePaidByBroker(EntryChargeTypeList.Codes.TotalGSTDirectAmount, buyer.PK));
			declaration.JE_PaymentMethod = Enterprise.Customs.Business.PaymentPartyCodeDescriptionList.Codes.Importer;
			AssertEquals(importerStatusComment + ", JE_PaymentMethod is importer", importerExpectedDutyResult, declaration.ActiveEntryHeaders[0].IsFeePaidByBroker(EntryChargeTypeList.Codes.TotalDutyAmount, buyer.PK));
			AssertEquals(importerStatusComment + ", JE_PaymentMethod is importer", importerExpectedGSTResult, declaration.ActiveEntryHeaders[0].IsFeePaidByBroker(EntryChargeTypeList.Codes.TotalGSTAmount, buyer.PK));
			AssertEquals(importerStatusComment + ", JE_PaymentMethod is importer", importerExpectedGSDResult, declaration.ActiveEntryHeaders[0].IsFeePaidByBroker(EntryChargeTypeList.Codes.TotalGSTDirectAmount, buyer.PK));
		}

		public void TestTypeDecider()
		{
			Assert("Update Customs.Business.CusEntryHeader to include a decider for this class", Factory.New(typeof(Customs.Business.CusEntryHeader)).GetType() == GetExpectedBusinessObjectType());
		}

		public void TestMessagesForDisplay()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			var message1 = Factory.New<UniversalEventMessage>();
			message1.EM_LinkedObject = entry;
			var message2 = Factory.New<UniversalEventMessage>();
			var stmAlog = Factory.New<StmALog>();
			using (stmAlog.LockForUpdatingKeyFieldsForTesting())
			{
				stmAlog.SL_Parent = declaration.PK;
				stmAlog.SL_Table = JobDeclaration.Schema.TableName;
			}
			var genPivot = Factory.New<GenPivot>();
			genPivot.XX_Relation1ID = stmAlog.PK;
			genPivot.XX_Relation2ID = message2.PK;
			genPivot.XX_RelationType = Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage;
			var message3 = Factory.New<UniversalEventMessage>();
			stmAlog = shipment.Logs.AddNew();
			genPivot = Factory.New<GenPivot>();
			genPivot.XX_Relation1ID = stmAlog.PK;
			genPivot.XX_Relation2ID = message3.PK;
			genPivot.XX_RelationType = Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage;
			var message4 = Factory.New<UniversalEventMessage>();
			genPivot = Factory.New<GenPivot>();
			genPivot.XX_Relation1ID = stmAlog.PK;
			genPivot.XX_Relation2ID = message4.PK;
			genPivot.XX_RelationType = Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage;
			message4.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message4.EM_MessageSubType = UniversalEventMessageTypes.Codes.IIDResponses;
			message4.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var message5 = Factory.New<UniversalEventMessage>();
			genPivot = Factory.New<GenPivot>();
			genPivot.XX_Relation1ID = stmAlog.PK;
			genPivot.XX_Relation2ID = message5.PK;
			genPivot.XX_RelationType = Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage;
			message5.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message5.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;
			Factory.Save();
			var messages = entry.MessagesForDisplay;
			AssertEquals("Messages Count", 5, messages.Count);
			Assert("Contains Message", messages.Contains(message1));
			Assert("Contains Message", messages.Contains(message2));
			Assert("Contains Message", messages.Contains(message3));
			Assert("Contains IID Message", messages.Contains(message4));
			Assert("Contains D4 Message", messages.Contains(message5));

			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			var message6 = Factory.New<UniversalEventMessage>();
			message6.EM_LinkedObject = entry;
			var message7 = Factory.New<UniversalEventMessage>();
			stmAlog = Factory.New<StmALog>();
			using (stmAlog.LockForUpdatingKeyFieldsForTesting())
			{
				stmAlog.SL_Parent = declaration.PK;
				stmAlog.SL_Table = JobDeclaration.Schema.TableName;
			}
			genPivot = Factory.New<GenPivot>();
			genPivot.XX_Relation1ID = stmAlog.PK;
			genPivot.XX_Relation2ID = message7.PK;
			genPivot.XX_RelationType = Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage;
			var message8 = Factory.New<UniversalEventMessage>();
			stmAlog = Factory.New<StmALog>();
			using (stmAlog.LockForUpdatingKeyFieldsForTesting())
			{
				stmAlog.SL_Parent = shipment.PK;
				stmAlog.SL_Table = ForwardingShipment.Schema.TableName;
			}
			genPivot = Factory.New<GenPivot>();
			genPivot.XX_Relation1ID = stmAlog.PK;
			genPivot.XX_Relation2ID = message8.PK;
			genPivot.XX_RelationType = Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage;
			Factory.Save();
			messages = entry.MessagesForDisplay;
			AssertEquals("Messages Count", 1, messages.Count);
			Assert("Contains Message", messages.Contains(message6));
		}

		public void TestGetUniqueNumberForAccountingIntegration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ServiceOptions.Codes.IID;
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "00050303";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			AssertEquals("Unique Number should be identical with the entry number", "12345000503032", ((IAccInvoiceDataProvider)entryHeader).UniqueNumber);

			using (CACustomsDataRegistry.Instance.SuspendAssignmentOfEntryNumberToDisbursementCharges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Unique Number should be empty when registry is turned on.", ZString.Empty, ((IAccInvoiceDataProvider)entryHeader).UniqueNumber);
			}
		}

		public void TestIsClearedB3C()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			entry.CH_EntryStatus = EntryStatusList.Codes.Clear;
			Assert("IsClearedB3CorCAD", !entry.IsClearedB3CorCAD);
			entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			Assert("IsClearedB3CorCAD", entry.IsClearedB3CorCAD);
			entry.CH_EntryStatus = B3EntryStatusList.Codes.SyntaxError;
			Assert("IsClearedB3CorCAD", !entry.IsClearedB3CorCAD);
			entry.CH_EntryStatus = B3EntryStatusList.Codes.Confirmed;
			Assert("IsClearedB3CorCAD", entry.IsClearedB3CorCAD);
		}

		protected override bool RatesAreReciprocal
		{
			get { return true; }
		}

		protected override BaseJobDeclaration ImportJobDeclaration
		{
			get
			{
				var declaration = (JobDeclaration)base.ImportJobDeclaration;
				declaration.UseBaseMergeStrategyForTesting = true;
				return declaration;
			}
		}

		#region B3Schedule

		public void TestCancelAndDeactivateDeferredB3Message()
		{
			var testHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			var testMessage1 = testHeader.Messages.AddNew();
			testMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			testMessage1.EM_Status = EDIMessage.Status.Queued;
			testMessage1.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			testMessage1.EM_IsActive = true;
			var testMessage2 = testHeader.Messages.AddNew();
			testMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			testMessage2.EM_Status = EDIMessage.Status.Queued;
			testMessage2.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			testMessage2.EM_IsActive = false;
			testMessage2.EM_HeldUntilDate = ZDateTime.UtcNow;
			var testMessage3 = testHeader.Messages.AddNew();
			testMessage3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			testMessage3.EM_Status = EDIMessage.Status.Queued;
			testMessage3.EM_MessageType = MessageTypeList.Codes.EDIRelease;
			testMessage3.EM_IsActive = true;
			testMessage3.EM_HeldUntilDate = ZDateTime.UtcNow;
			var testMessage4 = testHeader.Messages.AddNew();
			testMessage4.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			testMessage4.EM_Status = EDIMessage.Status.Queued;
			testMessage4.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			testMessage4.EM_IsActive = true;
			testMessage4.EM_HeldUntilDate = ZDateTime.UtcNow;
			var testMessage5 = testHeader.Messages.AddNew();
			testMessage5.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			testMessage5.EM_Status = EDIMessage.Status.Sent;
			testMessage5.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			testMessage5.EM_IsActive = true;
			testMessage5.EM_HeldUntilDate = ZDateTime.UtcNow;

			CombineAssertions("not working for non-B3 Header", () =>
			{
				testHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
				testHeader.CancelAndDeactivateDeferredB3Message();
				AssertEquals(EDIMessage.Status.Queued, testMessage1.EM_Status);
				AssertEquals(true, testMessage1.EM_IsActive);
				AssertEquals(EDIMessage.Status.Queued, testMessage2.EM_Status);
				AssertEquals(false, testMessage2.EM_IsActive);
				AssertEquals(EDIMessage.Status.Queued, testMessage3.EM_Status);
				AssertEquals(true, testMessage3.EM_IsActive);
				AssertEquals(EDIMessage.Status.Queued, testMessage4.EM_Status);
				AssertEquals(true, testMessage4.EM_IsActive);
				AssertEquals(EDIMessage.Status.Sent, testMessage5.EM_Status);
				AssertEquals(true, testMessage5.EM_IsActive);
			});

			CombineAssertions("working for B3 Header", () =>
			{
				testHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				testHeader.CancelAndDeactivateDeferredB3Message();
				AssertEquals(EDIMessage.Status.Queued, testMessage1.EM_Status);
				AssertEquals(true, testMessage1.EM_IsActive);
				AssertEquals(EDIMessage.Status.Queued, testMessage2.EM_Status);
				AssertEquals(false, testMessage2.EM_IsActive);
				AssertEquals(EDIMessage.Status.Queued, testMessage3.EM_Status);
				AssertEquals(true, testMessage3.EM_IsActive);
				AssertEquals(EDIMessage.Status.Cancelled, testMessage4.EM_Status);
				AssertEquals(false, testMessage4.EM_IsActive);
				AssertEquals(EDIMessage.Status.Sent, testMessage5.EM_Status);
				AssertEquals(true, testMessage5.EM_IsActive);
			});
		}

		public void TestGetScheduledTime()
		{
			var targetScheduleTime = ZDateTime.UtcNow;
			var testHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			var testMessage1 = testHeader.Messages.AddNew();
			testMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			testMessage1.EM_Status = EDIMessage.Status.Queued;
			testMessage1.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			testMessage1.EM_IsActive = true;
			var testMessage2 = testHeader.Messages.AddNew();
			testMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			testMessage2.EM_Status = EDIMessage.Status.Queued;
			testMessage2.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			testMessage2.EM_IsActive = false;
			testMessage2.EM_HeldUntilDate = targetScheduleTime.AddDays(1);
			var testMessage3 = testHeader.Messages.AddNew();
			testMessage3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			testMessage3.EM_Status = EDIMessage.Status.Queued;
			testMessage3.EM_MessageType = MessageTypeList.Codes.EDIRelease;
			testMessage3.EM_IsActive = true;
			testMessage3.EM_HeldUntilDate = targetScheduleTime.AddDays(2);
			var testMessage4 = testHeader.Messages.AddNew();
			testMessage4.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			testMessage4.EM_Status = EDIMessage.Status.Queued;
			testMessage4.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			testMessage4.EM_IsActive = true;
			testMessage4.EM_HeldUntilDate = targetScheduleTime.AddDays(3);
			var testMessage5 = testHeader.Messages.AddNew();
			testMessage5.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			testMessage5.EM_Status = EDIMessage.Status.Sent;
			testMessage5.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			testMessage5.EM_IsActive = true;
			testMessage5.EM_HeldUntilDate = targetScheduleTime.AddDays(4);

			CombineAssertions("not working for non-B3 Header", () =>
			{
				testHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
				AssertEquals("WrongHeaderType", ZDateTime.Empty, testHeader.DeferredB3MessageTime);
			});

			CombineAssertions("working for B3 Header", () =>
			{
				testHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				AssertEquals("only one schedule message", targetScheduleTime.AddDays(3), testHeader.DeferredB3MessageTime);

				testMessage4.Delete();
				Factory.Save();
				AssertEquals("No eligible schedule message", ZDateTime.Empty, testHeader.DeferredB3MessageTime);

				var testMessage6 = testHeader.Messages.AddNew();
				testMessage6.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				testMessage6.EM_Status = EDIMessage.Status.Queued;
				testMessage6.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
				testMessage6.EM_IsActive = true;
				testMessage6.EM_HeldUntilDate = targetScheduleTime.AddDays(6);
				var testMessage7 = testHeader.Messages.AddNew();
				testMessage7.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				testMessage7.EM_Status = EDIMessage.Status.Queued;
				testMessage7.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
				testMessage7.EM_IsActive = true;
				testMessage7.EM_HeldUntilDate = targetScheduleTime.AddDays(5);
				AssertEquals("Choose the smaller time", targetScheduleTime.AddDays(5), testHeader.DeferredB3MessageTime);
			});
		}

		#endregion

		public void TestReCalculateDeclarationStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			Factory.Save();
			AssertEquals("JE_MessageStatus", "", declaration.JE_MessageStatus);
			AssertEquals("CH_Status", "", entryHeader.CH_Status);
			entryHeader.CH_Status = MessageStatusList.Codes.ClearChange;
			Factory.Save();
			AssertEquals("JE_MessageStatus", MessageStatusList.Codes.ClearChange, declaration.JE_MessageStatus);
			AssertEquals("CH_Status", MessageStatusList.Codes.ClearChange, entryHeader.CH_Status);
		}

		public void TestCancelDeferredB3MessageIfRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			var message = entryHeader.Messages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			message.EM_IsActive = true;
			message.EM_HeldUntilDate = ZDateTime.Now;
			Factory.Save();
			AssertEquals("EM_Status", message.EM_Status, EDIMessage.Status.Queued);
			entryHeader.NeedCancelDeferredB3CADMessage = true;
			Factory.Save();
			AssertEquals("EM_Status", message.EM_Status, EDIMessage.Status.Cancelled);
		}

		public void TestExportDecDocumentHasAutoTickBoxFromMessageStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			Factory.Save();
			AssertEquals("JE_MessageStatus", "", declaration.JE_MessageStatus);
			AssertEquals("CH_Status", "", entryHeader.CH_Status);
			entryHeader.CH_Status = MessageStatusList.Codes.ClearChange;
			Factory.Save();
			AssertEquals("JE_MessageStatus", MessageStatusList.Codes.ClearChange, declaration.JE_MessageStatus);
			AssertEquals("CH_Status", MessageStatusList.Codes.ClearChange, entryHeader.CH_Status);

			AssertEquals("IsG7MessageStatusClearOriginal", false, entryHeader.IsG7MessageStatusClearOriginalOrNotCleared);
			AssertEquals("IsG7MessageStatusClearChange", true, entryHeader.IsG7MessageStatusClearAmendmentOrChange);
			AssertEquals("IsG7MessageStatusClearDelete", false, entryHeader.IsG7MessageStatusClearDelete);

			entryHeader.CH_Status = MessageStatusList.Codes.ClearOriginal;
			AssertEquals("IsG7MessageStatusClearOriginal", true, entryHeader.IsG7MessageStatusClearOriginalOrNotCleared);

			entryHeader.CH_Status = MessageStatusList.Codes.ClearDelete;
			AssertEquals("IsG7MessageStatusClearDelete", true, entryHeader.IsG7MessageStatusClearDelete);

			entryHeader.CH_Status = MessageStatusList.Codes.ClearReplace;
			AssertEquals("IsG7MessageStatusClearAmendment", true, entryHeader.IsG7MessageStatusClearAmendmentOrChange);
		}

		public void TestDocumentGeneratorSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ServiceOptions.Codes.IID;
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "00050303";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var supporter = (Integration.Customs.ICustomsDocumentGeneratorSupporter)entryHeader;
			AssertEquals("B3 (As Accounted)", supporter.GetDocumentName(CusEntryHeader.B3AsAccountedActionCode));
			AssertEquals("CA Customs Invoice", supporter.GetDocumentName(CusEntryHeader.CACustomsInvoiceActioCode));
			AssertExceptionThrown(typeof(InvalidOperationException), () => supporter.GetDocumentName("A"));

			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			var sentMessage = Factory.New<B3Message>();
			sentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sentMessage.EM_Status = EDIMessage.Status.Sent;
			sentMessage.EM_MessageText = @"UNH+2287+CUSDEC:S:99B:UN'BGM+:::AB+454+9'CST++I'LOC+41+497'LOC+11+497'RFF+TN:000001831'RFF+ARA:105759013RM0001'TDT+11++2++9165'DOC+785+803609238364B'DTM+204:20160120:102'MOA+43:4000'UNS+D'DMS+1'NAD+SE++GHJ LTD. INT?'L  ?+?:??@'DOC+935'DTM+129:20160119:102'LOC+27+AU+AU'PAT+1+CONSIGN:::02'MOA+6::CAD'CST+1+POS+1+8544700090+23'MOA+40:400000'MOA+43:400000'MOA+125:400000'RFF+LI:1:0'MOA+38:400000'TAX+7+VAT++5.0'MOA+1:20000'GIR+1+1'MEA+AAR++MTR:2134'TAX+5+++0.00'MOA+155:000'UNS+S'TAX+7+:::K90'MOA+1:20000'TAX+4+:::K90'MOA+176:20000'UNT+37+2287'";
			sentMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(-20);
			entryHeader.Messages.Add(sentMessage);
			var sentInterchange = Factory.New<CAEDIInterchange>();
			sentInterchange.EI_InterchangeNum = "1";
			sentMessage.EM_EI = sentInterchange.PK;
			Factory.Save();
			AssertEquals(false, supporter.GenerateCustomsDocument(CusEntryHeader.B3AsAccountedActionCode));
			AssertEquals("system cannot find the accepted B3 message for this job", supporter.GetReasonForUnableToGenerateCustomsDocument());
			AssertEquals(false, supporter.GenerateCustomsDocument(CusEntryHeader.CACustomsInvoiceActioCode));
			AssertEquals("there is no invoice header found for this job", supporter.GetReasonForUnableToGenerateCustomsDocument());

			var receiveMessage = Factory.New<B3Message>();
			receiveMessage.EM_MessageText = @"UNH+1+CUSRES:S:99B:UN'BGM++484+9'DTM+137:20160428:102'RFF+ABO:12345'ERP+:I99'RFF+ABO:503032'ERC+942992'DOC+961'CST++1+1+0'UNT+9+1'";
			receiveMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			receiveMessage.EM_MessageSubType = B3EntryStatusList.Codes.Accepted;
			receiveMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;
			entryHeader.Messages.Add(receiveMessage);
			Factory.Save();
			AssertEquals(true, supporter.GenerateCustomsDocument(CusEntryHeader.B3AsAccountedActionCode));
			AssertEquals(ZString.Empty, supporter.GetReasonForUnableToGenerateCustomsDocument());
			AssertEquals(false, supporter.GenerateCustomsDocument(CusEntryHeader.CACustomsInvoiceActioCode));
			AssertEquals("there is no invoice header found for this job", supporter.GetReasonForUnableToGenerateCustomsDocument());
			var storageMain = declaration.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(declaration, Core.Constants.DocManagerCodes.JobDeclaration);
			AssertEquals(1, storageMain.Files.Count);
			AssertEquals("B3ImportDocument.pdf", storageMain.Files[0].FileName);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			Factory.Save();
			AssertEquals(true, supporter.GenerateCustomsDocument(CusEntryHeader.CACustomsInvoiceActioCode));
			AssertEquals(ZString.Empty, supporter.GetReasonForUnableToGenerateCustomsDocument());
			storageMain = declaration.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(declaration, Core.Constants.DocManagerCodes.JobDeclaration);
			AssertEquals(2, storageMain.Files.Count);
			AssertEquals("B3ImportDocument.pdf", storageMain.Files[0].FileName);
			AssertEquals("CACustomsInvoice.pdf", storageMain.Files[1].FileName);
		}
	}
}
