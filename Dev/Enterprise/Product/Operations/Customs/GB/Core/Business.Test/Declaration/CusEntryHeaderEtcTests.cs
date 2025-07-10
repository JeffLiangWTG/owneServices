using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.Customs.GB.CDS;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.Integration.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;
using static Enterprise.Integration.Customs;
using CustomsChargeTypeList = Enterprise.Customs.Business.CustomsChargeTypeList;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryHeader))]
	class CusEntryHeaderTest : EU.Business.Declaration.Testing.CusEntryHeaderTest<CusEntryHeader>
	{
		public override void TestAdditionalInfos()
		{
			Assert(true);
		}

		public override void TestComplementaryJob()
		{
			Assert(true);
		}

		public override void TestOfficeOfExit()
		{
			var declaration = (JobDeclaration)GetNewDeclaration();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsOffices.RemoveAndDeleteAll();
			declaration.JE_CustomsOffice = "LV001000";
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, "LV002000");
			AssertEquals("LV001000", declaration.OfficeOfExit);
			AssertEquals("LV001000", entry.OfficeOfExit);
		}

		public override void TestOfficeOfEntry()
		{
			var declaration = (JobDeclaration)GetNewDeclaration();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsOffices.RemoveAndDeleteAll();
			declaration.JE_CustomsOffice = "LV001000";
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, "LV002000");
			AssertEquals("", declaration.OfficeOfEntry);
			AssertEquals("", entry.OfficeOfEntry);
		}

		public void TestMasterUCR()
		{
			var declaration = (JobDeclaration)GetNewDeclaration();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_MasterUCR = "ABC";
			Factory.Save();
			AssertEquals("ABC", entry.CH_MasterUCR);
			var cen = Factory.LoadTop1<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, entry.PK));
			AssertEquals(false, cen.CE_EntryIsSystemGenerated);
		}

		public void TestIsOriginAndDestinationRequiredInItinerary()
		{
			var decCds = Factory.New<JobDeclaration>();
			decCds.JE_ApplicationCode = "CDS";
			var entryCds = (CusEntryHeader)decCds.ActiveEntryHeaders.AddNew();
			AssertEquals("CDS - required ", true, entryCds.IsOriginAndDestinationRequiredInItinerary_ForTest);
		}

		public void TestGetDefaultStatusDescription()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_ApplicationCode = "CDS";
			var entry1 = (CusEntryHeader)dec1.ActiveEntryHeaders.AddNew();
			AssertEquals("Not Sent", entry1.DefaultStatusDescription);
			entry1.MovementReferenceNumberSetter("MRN123", ZDateTime.Today);
			AssertEquals("Not Accepted", entry1.DefaultStatusDescription);
		}

		public void TestGetCDSChargeDeductions()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = "CDS";
			var entry = (CusEntryHeader)dec.ActiveEntryHeaders.AddNew();
			var entryLine1 = entry.AllEntryLines.AddNew();
			var entryLine2 = entry.AllEntryLines.AddNew();

			var invoice1 = dec.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "GBP";
			var invoice1Line1 = invoice1.InvoiceLines.AddNew();
			invoice1Line1.JI_CL = entryLine1.PK;
			invoice1Line1.JI_LinePrice = 50;
			invoice1.JZ_InvoiceAmount = 50;

			var invoice2 = dec.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = "GBP";
			var invoice2Line1 = invoice2.InvoiceLines.AddNew();
			invoice2Line1.JI_CL = entryLine2.PK;
			invoice2Line1.JI_LinePrice = 50;
			invoice2.JZ_InvoiceAmount = 100;

			dec.TopGroupInvoice.Charges.RemoveAndDeleteAll();

			var charge1 = invoice1.Charges.AddNew();
			var charge2 = invoice1.Charges.AddNew();
			var charge3 = invoice1.Charges.AddNew();
			var charge4 = invoice1.Charges.AddNew();
			var charge5 = invoice2.Charges.AddNew();
			var charge6 = invoice2.Charges.AddNew();
			var charge7 = invoice2.Charges.AddNew();
			var charge8 = invoice2.Charges.AddNew();
			var charge9 = dec.TopGroupInvoice.Charges.AddNew();
			var charge10 = dec.TopGroupInvoice.Charges.AddNew();

			InvChargeTestHelper.SetUpCharge(charge1, "CBR", false, 1.1m, string.Empty, true, 1m);//AC Item
			InvChargeTestHelper.SetUpCharge(charge2, "CBR", false, 1.1m, string.Empty, false, 2m);//AC Item
			InvChargeTestHelper.SetUpCharge(charge3, "OFT", true, 0m, "VAL", false, 3m);//AP Header
			InvChargeTestHelper.SetUpCharge(charge4, "OFT", true, 0m, "VAL", false, 4m);//AP Header
			InvChargeTestHelper.SetUpCharge(charge5, "OFT", true, 0m, "VAL", false, 5m);//AP Header
			InvChargeTestHelper.SetUpCharge(charge6, "CEA", false, 0m, string.Empty, false, 6m);//BB Item
			charge6.J7_IsGSTApplicable = true;
			InvChargeTestHelper.SetUpCharge(charge7, "CEA", false, 0m, string.Empty, false, 7m);//BB Item
			charge7.J7_IsStatisticalValueApplicable = true;
			InvChargeTestHelper.SetUpCharge(charge8, "OFT", true, 0m, "VAL", false, 5m);//AP Header
			charge8.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.China;
			InvChargeTestHelper.SetUpCharge(charge9, "OFT", true, 0m, "VAL", false, 6m);//AP Header
			charge9.J7_IsGSTApplicable = false;
			InvChargeTestHelper.SetUpCharge(charge10, "OFT", true, 0m, "VAL", false, 7m);//AP Header
			charge10.J7_IsStatisticalValueApplicable = false;

			dec.ResumeApportionment();

			var deductions = entry.CDSChargeDeductions;
			AssertEquals(1, deductions.Count);
			AssertEquals((ZDecimal)27.99, deductions["AP.GBP"].Amount);
		}

		public void TestLookups()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			AssertType<CusEntryHeaderLookups>(entryHeader.Lookups);
		}

		public void TestIrcInventoryReturnCodeAndRouteOfEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			declaration.SingleEntry.CH_AddInfo = "RouteOfEntry=RR*IrcInventoryReturnCode=II*ImportClearanceStatusICS=6*StyleOfEntrySOE=2";

			AssertEquals("RR", declaration.SingleEntry.CH_RouteOfEntry);
			AssertEquals("II", declaration.SingleEntry.CH_IrcInventoryReturnCode);
			Assert(declaration.SingleEntry.CH_AddInfo.Contains("RouteOfEntry=RR"));
			Assert(declaration.SingleEntry.CH_AddInfo.Contains("IrcInventoryReturnCode=II"));
			AssertEquals("6", declaration.SingleEntry.CH_ImportClearanceStatusICS);
			AssertEquals("2", declaration.SingleEntry.CH_StyleOfEntrySOE);

			AssertEquals(declaration.JE_GBIrcInventoryReturnCode, declaration.SingleEntry.CH_IrcInventoryReturnCode);
			AssertEquals(declaration.JE_GBRouteOfEntry, declaration.SingleEntry.CH_RouteOfEntry);
			AssertEquals(declaration.ZG_StyleOfEntrySOE, declaration.SingleEntry.CH_StyleOfEntrySOE);
			AssertEquals(declaration.ZG_ImportClearanceStatusICS, declaration.SingleEntry.CH_ImportClearanceStatusICS);
		}

		public void TestIsCancelledWithCustoms()
		{
			var decCDS = Factory.New<JobDeclaration>();
			decCDS.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entryCDS = decCDS.CustomsEntryHeaders.AddNew();
			Assert(!entryCDS.IsCancelledWithCustoms);
			entryCDS.CH_EntryStatus = "DJC";
			Assert(!entryCDS.IsCancelledWithCustoms);
			entryCDS.CH_EntryStatus = GBCommonConstants.EntryStatusCodes.DeclarationCancelled;
			Assert(!entryCDS.IsCancelledWithCustoms);
			entryCDS.CH_EntryStatus = Customs.Common.EU.EntryStatusList.Codes.Cancelled;
			Assert(entryCDS.IsCancelledWithCustoms);

			var decCHIEF = Factory.New<JobDeclaration>();
			decCHIEF.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			var entryChief = decCHIEF.CustomsEntryHeaders.AddNew();
			Assert(!entryChief.IsCancelledWithCustoms);
			entryChief.CH_EntryStatus = "DJC";
			Assert(!entryChief.IsCancelledWithCustoms);
			entryChief.CH_EntryStatus = GBCommonConstants.EntryStatusCodes.DeclarationCancelled;
			Assert(!entryChief.IsCancelledWithCustoms);
			entryChief.CH_EntryStatus = Customs.Common.EU.EntryStatusList.Codes.Cancelled;
			Assert(entryChief.IsCancelledWithCustoms);
		}

		public void TestLRNGenerated_CDS()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "HYE";
			registrationKey.ServerCodeForTest = "CMT";

			TestConnection.BeginTransaction(); // Updating next number fountain value for the test
			try
			{
				Env.NumberFountains.GBCDSEntryLocalReferenceNumber.SetNext(Factory, 6789);
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = "CDS";
				var entry = declaration.CustomsEntryHeaders.AddNew();
				Factory.Save();
				var lrn = entry.LRN;
				AssertEquals("HYEEDICMT0000000006789", lrn);
				AssertEquals(22, lrn.Length);
				Assert(lrn.Right(13).IsLettersAndNumbersOnlyOrEmpty);
				AssertEquals("HYEEDICMT", lrn.Left(9));
				var internalLrnEntryNumber = CusEntryNumber.Load(entry, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
				AssertNotNull(internalLrnEntryNumber);
				AssertEquals(CusEntryNumber.Categories.CustomsPermitClearanceNumber, internalLrnEntryNumber.CE_Category);
				Assert(internalLrnEntryNumber.CE_EntryIsSystemGenerated);
			}
			finally
			{
				TestConnection.RollbackTransaction(); // Updating next number fountain value for the test
			}
		}

		public void TestIsUniqueEntryLocalReferenceNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "CDS";
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.LRN = "LRNLRN123";
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.LRN = "LRNLRN123";
			Factory.Save();

			Assert(!CusEntryHeader.IsUniqueEntryLocalReferenceNumber(Factory, "LRNLRN123"));
			Assert(CusEntryHeader.IsUniqueEntryLocalReferenceNumber(Factory, "LRNLRN123456"));
		}

		public void TestMessages()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			AssertType<GbEDIMessageCollection>(entryHeader.Messages);
		}

		public void TestGetMethodsOfPaymentThatCanInfluenceAutoRatingCore()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = "CDS";
			var ceh = (ICustomsChargeEntry)dec.ActiveEntryHeaders.AddNew();
			AssertMethodOfPaymentCodesEmpty(ceh);

			dec.JE_ApplicationCode = "CHF";
			var ceh2 = (ICustomsChargeEntry)dec.ActiveEntryHeaders.AddNew();
			AssertMethodOfPaymentCodesEmpty(ceh2);

			var newFactory = new BusinessObjectFactory();
			CreateRatingRefZZRecords(newFactory);

			dec = newFactory.New<JobDeclaration>();
			dec.JE_ApplicationCode = "CDS";
			var ceh3 = (ICustomsChargeEntry)dec.ActiveEntryHeaders.AddNew();
			newFactory.Save();
			AssertMethodOfPaymentCodePopulated(ceh3, new string[] { "A", "B" });

			newFactory = new BusinessObjectFactory();
			dec = newFactory.New<JobDeclaration>();
			dec.JE_ApplicationCode = "CHF";
			var ceh4 = (ICustomsChargeEntry)dec.ActiveEntryHeaders.AddNew();
			newFactory.Save();
			AssertMethodOfPaymentCodePopulated(ceh4, new string[] { "C", "D" });
		}

		public class CusEntryHeaderWithPublicPropertiesAndFunkyPaymentCodes : CusEntryHeader
		{
			public CusEntryHeaderWithPublicPropertiesAndFunkyPaymentCodes(BusinessObjectFactory factory, System.Data.DataRow row)
				: base(factory, row)
			{ }

			public new ZDecimal GetTotalChargeValueFor(EntryChargeType chargeTypeElement, ZString methodOfPaymentCode)
			{
				return base.GetTotalChargeValueFor(chargeTypeElement, methodOfPaymentCode);
			}
		}

		public void TestGetAmountsForDifferentMethodsOfPayment()
		{
			CreateRatingRefZZRecords(Factory);

			var ceh = Factory.New<CusEntryHeaderWithPublicPropertiesAndFunkyPaymentCodes>();
			var entryLine1 = ceh.MergedLines.AddNew();
			var entryLine2 = ceh.MergedLines.AddNew();

			var fee1A00X = AddNewFee(entryLine1, "A00", "X", 111m);
			var fee1A00Y = AddNewFee(entryLine1, "A00", "Y", 222m);
			var fee1B00X = AddNewFee(entryLine1, "B00", "X", 333m);
			var fee1B00Y = AddNewFee(entryLine1, "B00", "Y", 444m);
			var fee2A00X = AddNewFee(entryLine2, "A00", "X", 1000m);
			var fee2A00Y = AddNewFee(entryLine2, "A00", "Y", 2000m);
			var fee2B00X = AddNewFee(entryLine2, "B00", "X", 3000m);
			var fee2B00Y = AddNewFee(entryLine2, "B00", "Y", 4000m);

			var a00Charge = new EntryChargeType(null, "A00", "", true, "");
			var b00Charge = new EntryChargeType(null, "B00", "", true, "");
			AssertEquals(111m + 1000m, ceh.GetTotalChargeValueFor(a00Charge, "X"));
			AssertEquals(222m + 2000m, ceh.GetTotalChargeValueFor(a00Charge, "Y"));
			AssertEquals(333m + 3000m, ceh.GetTotalChargeValueFor(b00Charge, "X"));
			AssertEquals(444m + 4000m, ceh.GetTotalChargeValueFor(b00Charge, "Y"));

			AssertEquals(111m + 1000m + 222m + 2000m, ceh.GetTotalChargeValueFor(a00Charge, ""));
			AssertEquals(333m + 3000m + 444m + 4000m, ceh.GetTotalChargeValueFor(b00Charge, ""));
		}

		public void TestGetAmountsForVAT()
		{
			CreateRatingRefZZRecords(Factory);

			var ceh = Factory.New<CusEntryHeaderWithPublicPropertiesAndFunkyPaymentCodes>();
			var entryLine1 = ceh.MergedLines.AddNew();
			var entryLine2 = ceh.MergedLines.AddNew();

			const string mop = "E";
			AddNewFee(entryLine1, "A00", mop, 200m);
			AddNewFee(entryLine1, "B00", mop, 100m);
			AddNewFee(entryLine1, "B05", mop, 50m);
			AddNewFee(entryLine2, "A00", mop, 20m);
			AddNewFee(entryLine2, "B00", mop, 10m);
			AddNewFee(entryLine2, "B05", mop, 5m);

			var vatEntryChargeType = new EntryChargeType(null, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Vat, string.Empty, true, string.Empty);
			var totalVatCharges = ceh.GetTotalChargeValueFor(vatEntryChargeType, mop);
			AssertEquals(100m + 50m + 10m + 5m, totalVatCharges);
		}

		public void TestGetAmountsWithConfirmedFees()
		{
			CreateRatingRefZZRecords(Factory);

			var header = Factory.New<CusEntryHeaderWithPublicPropertiesAndFunkyPaymentCodes>();
			var entryLine1 = header.MergedLines.AddNew();
			var entryLine2 = header.MergedLines.AddNew();
			entryLine1.Fees.AddOrUpdate("A00", 111m);
			entryLine1.Fees.AddOrUpdate("B00", 222m);
			entryLine2.Fees.AddOrUpdate("A00", 1000m);
			entryLine2.Fees.AddOrUpdate("B00", 1100m);
			entryLine1.ConfirmedFees.AddOrUpdate("A00", 111.11m);
			entryLine1.ConfirmedFees.AddOrUpdate("B00", 222.22m);
			entryLine2.ConfirmedFees.AddOrUpdate("A00", 1001.10m);
			entryLine2.ConfirmedFees.AddOrUpdate("B00", 1100.01m);

			var a00Charge = new EntryChargeType(null, "A00", "", true, "");
			var b00Charge = new EntryChargeType(null, "B00", "", true, "");

			AssertEquals(111.11m + 1001.10m, header.GetTotalChargeValueFor(a00Charge, ""));
			AssertEquals(222.22m + 1100.01m, header.GetTotalChargeValueFor(b00Charge, ""));

			AssertEquals(111.11m + 1001.10m, header.Duty);
			AssertEquals(222.22m + 1100.01m, header.VAT);
		}

		public void TestNIDutyAndTaxIncluded()
		{
			var refDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var dtyRateType = refDataHelper.CreateNewOrGetExistingRateType(dataGroupingCode: "CDS", rateType: Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, description: "Customs duties on industrial products");
			refDataHelper.CreateCusRateCode(Factory, zy1RateCode: GBCommonConstants.NorthernIrelandDutyCodes.CustomsDuty, dtyRateType.PK);
			var gbDataGrouping = refDataHelper.CreateNewOrGetExistingDataGrouping("GB", "United Kingdom");
			refDataHelper.CreateNewOrGetExistingDataGrouping("CDS", "", parent: gbDataGrouping);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var ceh = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = ceh.MergedLines.AddNew();
			var entryLine2 = ceh.MergedLines.AddNew();

			var fee1A00 = AddNewFee(entryLine1, "A00", "A", 111m);
			var fee1A50 = AddNewFee(entryLine1, "A50", "A", 222m);
			var fee1ANotNI = AddNewFee(entryLine1, "A01", "A", 11m);
			var fee1B00 = AddNewFee(entryLine1, "B00", "A", 666m);
			var fee1B05 = AddNewFee(entryLine1, "B05", "E", 777m);
			var fee1BNotNI = AddNewFee(entryLine1, "B01", "A", 22m);

			CombineAssertions(() =>
			{
				AssertEquals("CustomsEntryHeader.Duty", 111m + 222m, ceh.Duty);
				AssertEquals("CustomsEntryHeader.VAT", 666m + 777m, ceh.VAT);
				AssertEquals("CustomsEntryHeader.DutyImmediate", 333m, ceh.DutyImmediate);
				AssertEquals("CustomsEntryHeader.DutyDeferred", 0m, ceh.DutyDeferred);
				AssertEquals("CustomsEntryHeader.VATImmediate", 666m, ceh.VATImmediate);
				AssertEquals("CustomsEntryHeader.VATDeferred", 777m, ceh.VATDeferred);
				AssertEquals("CustomsEntryHeader.AllOtherFeesImmediate", 11m + 22m, ceh.AllOtherFeesImmediate);
				AssertEquals("CustomsEntryHeader.AllOtherFeesDeferred", 0m, ceh.AllOtherFeesDeferred);
			});
		}

		public void TestEntryHeaderSummaryConfirmedFees()
		{
			var refDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var dtyRateType = refDataHelper.CreateNewOrGetExistingRateType(dataGroupingCode: "CDS", rateType: Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, description: "Customs duties on industrial products");
			refDataHelper.CreateCusRateCode(Factory, zy1RateCode: GBCommonConstants.NorthernIrelandDutyCodes.CustomsDuty, dtyRateType.PK);
			var gbDataGrouping = refDataHelper.CreateNewOrGetExistingDataGrouping("GB", "United Kingdom");
			refDataHelper.CreateNewOrGetExistingDataGrouping("CDS", "", parent: gbDataGrouping);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var ceh = declaration.CustomsEntryHeaders.AddNew();
			var line = ceh.MergedLines.AddNew();

			AddNewFee(line, "A00", "A", 111m);
			AddNewFee(line, "A50", "A", 222m);
			AddNewFee(line, "A01", "A", 11m);
			AddNewFee(line, "B00", "A", 666m);
			AddNewFee(line, "B05", "E", 777m);
			AddNewFee(line, "B01", "A", 22m);
			line.ConfirmedFees.AddOrUpdate("A00", 111.11m).CF_MethodOfPayment = "A";
			line.ConfirmedFees.AddOrUpdate("A50", 222.22m).CF_MethodOfPayment = "A";
			line.ConfirmedFees.AddOrUpdate("A01", 11.01m).CF_MethodOfPayment = "A";
			line.ConfirmedFees.AddOrUpdate("B00", 666.66m).CF_MethodOfPayment = "A";
			line.ConfirmedFees.AddOrUpdate("B05", 777.77m).CF_MethodOfPayment = "E";
			line.ConfirmedFees.AddOrUpdate("B01", 22.02m).CF_MethodOfPayment = "A";

			CombineAssertions(() =>
			{
				AssertEquals("CustomsEntryHeader.Duty", 333.33m, ceh.Duty);
				AssertEquals("CustomsEntryHeader.VAT", 1444.43m, ceh.VAT);
				AssertEquals("CustomsEntryHeader.DutyImmediate", 333.33m, ceh.DutyImmediate);
				AssertEquals("CustomsEntryHeader.VATImmediate", 666.66m, ceh.VATImmediate);
				AssertEquals("CustomsEntryHeader.VATDeferred", 777.77m, ceh.VATDeferred);
				AssertEquals("CustomsEntryHeader.AllOtherFeesImmediate", 33.03m, ceh.AllOtherFeesImmediate);
				AssertEquals("CustomsEntryHeader.AllOtherFeesDeferred", 0m, ceh.AllOtherFeesDeferred);
			});
		}

		public void TestTaxFeePaymentCodeIsDeferred()
		{
			var refDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var dtyRateType = refDataHelper.CreateNewOrGetExistingRateType(dataGroupingCode: "CDS", rateType: Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, description: "Customs duties on industrial products");
			refDataHelper.CreateCusRateCode(Factory, zy1RateCode: GBCommonConstants.NorthernIrelandDutyCodes.CustomsDuty, dtyRateType.PK);
			var gbDataGrouping = refDataHelper.CreateNewOrGetExistingDataGrouping("GB", "United Kingdom");
			refDataHelper.CreateNewOrGetExistingDataGrouping("CDS", "", parent: gbDataGrouping);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var ceh = declaration.CustomsEntryHeaders.AddNew();

			var entryLine1 = ceh.MergedLines.AddNew();
			var entryLine2 = ceh.MergedLines.AddNew();

			var fee1B00E1 = AddNewFee(entryLine1, "B00", "E", 333m);
			var fee1B00E2 = AddNewFee(entryLine1, "B00", "E", 555m);
			var fee1B00A = AddNewFee(entryLine1, "B00", "A", 666m);

			CombineAssertions("CDS", () =>
			{
				AssertEquals("CustomsEntryHeader.VATDeferred", 888m, ceh.VATDeferred);
				AssertEquals("CustomsEntryHeader.VATImmediate", 666m, ceh.VATImmediate);
				AssertEquals("CustomsEntryHeader.AllOtherFeesImmediate", 0m, ceh.AllOtherFeesImmediate);
				AssertEquals("CustomsEntryHeader.AllOtherFeesDeferred", 0m, ceh.AllOtherFeesDeferred);
			});

			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;

			var fee1B00F1 = AddNewFee(entryLine1, "B00", "F", 101m);
			var fee1B00F2 = AddNewFee(entryLine1, "B00", "F", 202m);

			CombineAssertions("CHIEF", () =>
			{
				AssertEquals("CustomsEntryHeader.VATDeferred", 303m, ceh.VATDeferred);
				AssertEquals("CustomsEntryHeader.VATImmediate", 666m, ceh.VATImmediate);
				AssertEquals("CustomsEntryHeader.AllOtherFeesImmediate", 0m, ceh.AllOtherFeesImmediate);
				AssertEquals("CustomsEntryHeader.AllOtherFeesDeferred", 0m, ceh.AllOtherFeesDeferred);
			});
		}

		public void TestEntryNumberType()
		{
			//IMP
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "IMS213";
			Factory.InvalidateCachedProperties();
			AssertEquals("Entry Type", Customs.Business.JobMessageTypeList.Codes.Import, entryHeader.CusEntryNumber.CE_EntryType);
			AssertEquals("Entry Number", "IMS213", entryHeader.CusEntryNumber.CE_EntryNum);

			//Based on JE_MessageType
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.CustomsEntryHeaders.Add(entryHeader);
			entryHeader.EntryNumber = "IMS213";
			Factory.InvalidateCachedProperties();
			AssertEquals("Entry Type", Customs.Business.JobMessageTypeList.Codes.Export, entryHeader.CusEntryNumber.CE_EntryType);
			AssertEquals("Entry Number", "IMS213", entryHeader.CusEntryNumber.CE_EntryNum);

			//NOT Based on CH_MessageType
			entryHeader.CusEntryNumber.Delete();
			entryHeader.CH_MessageType = "GBG";
			entryHeader.EntryNumber = "IMS213";
			Factory.InvalidateCachedProperties();
			AssertEquals("Entry Type", Customs.Business.JobMessageTypeList.Codes.Export, entryHeader.CusEntryNumber.CE_EntryType);
			AssertEquals("Entry Number", "IMS213", entryHeader.CusEntryNumber.CE_EntryNum);
		}

		public void TestEntryNumberTypeForApplicationTypes()
		{
			// Chief
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "123";
			entryHeader.CH_MessageType = "EXP";
			CusEntryNumber entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryNum = "123";
			entryNumber.Parent = entryHeader;
			entryNumber.CE_EntryType = "EXP";
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			AssertEquals("123", declaration.DeclarationNumber);

			// CDS
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "456";
			entryHeader.CH_MessageType = Customs.Common.CusEntryNumberTypes.Standard.MovementReferenceNumber;
			entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryNum = "456";
			entryNumber.Parent = entryHeader;
			entryNumber.CE_EntryType = "EXP";
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			AssertEquals("456", declaration.DeclarationNumber);

			// Invalid Application Code
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = "Bad";
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "789";
			entryHeader.CH_MessageType = Customs.Common.CusEntryNumberTypes.Standard.MovementReferenceNumber;
			entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryNum = "789";
			entryNumber.Parent = entryHeader;
			entryNumber.CE_EntryType = "IMP";
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			AssertEquals("789", declaration.DeclarationNumber);

			// Null Declaration
			entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.EntryNumber = "789";
			entryHeader.CH_MessageType = Customs.Common.CusEntryNumberTypes.Standard.MovementReferenceNumber;
			entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryNum = "789";
			entryNumber.Parent = entryHeader;
			entryNumber.CE_EntryType = "IMP";
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			AssertEquals("789", declaration.DeclarationNumber);
		}

		public void TestICusAddInfoTypeSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			ICusAddInfoTypeSupporter supporter = entry;
			supporter.AssertType(typeof(CusAddInfo<MaritimeUcnThatIsHeld>), CusAddInfoTypeAttribute.Codes.GBMaritimeUCNThatIsHeld);
			supporter.AssertType(null, "ZZ!");

			var addInfo = entry.MaritimeUcnsThatAreHeld.AddNew();
			addInfo.Data.NW_UCN = "2345678901234";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var addInfoInDiffFactory = newFactory.Load<CusAddInfo>(addInfo.PK);
			AssertEquals(typeof(CusAddInfo<MaritimeUcnThatIsHeld>), addInfoInDiffFactory.GetType());
		}

		public void TestMaritimeUcnsThatAreHeld()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(0, entry.MaritimeUcnsThatAreHeld.Count);
			var ucn = entry.MaritimeUcnsThatAreHeld.AddNew();
			AssertEquals(1, entry.MaritimeUcnsThatAreHeld.Count);
			var ucn2 = entry.MaritimeUcnsThatAreHeld.AddNew();
			AssertEquals(2, entry.MaritimeUcnsThatAreHeld.Count);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var entryReloaded = newFactory.Load<CusEntryHeader>(entry.PK);
			AssertEquals(2, entryReloaded.MaritimeUcnsThatAreHeld.Count);
		}

		public void TestPackageCount_ForCDS()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			dec.ZG_ShipmentType = ShipmentTypeList.Codes.BasicDirect;
			dec.CustomsEntryInstructions.RemoveAndDeleteAll();
			var cei = dec.CustomsEntryInstructions.AddNew();
			cei.CEI_PackageCount = 30;
			var cei2 = dec.CustomsEntryInstructions.AddNew();
			cei2.CEI_PackageCount = 20;

			var invoice = dec.Invoices.AddNew();
			var invLine = invoice.InvoiceLines.AddNew();
			invLine.JI_CEI = cei.PK;
			var invLine2 = invoice.InvoiceLines.AddNew();
			invLine2.JI_CEI = cei2.PK;

			var merger = new LineMerger(dec);
			merger.DoMerge();

			AssertEquals("dec.ActiveEntryHeaders.Count", 2, dec.ActiveEntryHeaders.Count);

			AssertEquals(30, cei.CEI_PackageCount);
			AssertEquals(20, cei2.CEI_PackageCount);
			AssertEquals(30, dec.ActiveEntryHeaders[0].PackagesCount);
			AssertEquals(20, dec.ActiveEntryHeaders[1].PackagesCount);

			invLine2.JI_CEI = ZGuid.Empty;
			merger = new LineMerger(dec);
			merger.DoMerge();
			AssertEquals(0, dec.ActiveEntryHeaders[1].PackagesCount);
		}

		public void TestCH_CEIIsAssignedAfterMergingForCDS()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			dec.ZG_ShipmentType = ShipmentTypeList.Codes.BasicDirect;
			dec.CustomsEntryInstructions.RemoveAndDeleteAll();
			var cei = dec.CustomsEntryInstructions.AddNew();
			cei.CEI_PackageCount = 30;
			var cei2 = dec.CustomsEntryInstructions.AddNew();
			cei2.CEI_PackageCount = 20;

			var invoice = dec.Invoices.AddNew();
			var invLine = invoice.InvoiceLines.AddNew();
			invLine.JI_CEI = cei.PK;
			var invLine2 = invoice.InvoiceLines.AddNew();
			invLine2.JI_CEI = cei2.PK;

			var merger = new LineMerger(dec);
			merger.DoMerge();

			AssertEquals("CH AND CEI linked together after merging", dec.ActiveEntryHeaders[0].CH_CEI_Instruction, cei.PK);
			AssertEquals("CH AND CEI linked together after merging", dec.ActiveEntryHeaders[1].CH_CEI_Instruction, cei2.PK);
		}

		public void TestCanLoadEntryInstructionAfterMergingForCDS()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			dec.ZG_ShipmentType = ShipmentTypeList.Codes.BasicDirect;
			dec.CustomsEntryInstructions.RemoveAndDeleteAll();
			var cei = dec.CustomsEntryInstructions.AddNew();
			cei.CEI_PackageCount = 30;
			var cei2 = dec.CustomsEntryInstructions.AddNew();
			cei2.CEI_PackageCount = 20;

			var invoice = dec.Invoices.AddNew();
			var invLine = invoice.InvoiceLines.AddNew();
			invLine.JI_CEI = cei.PK;
			var invLine2 = invoice.InvoiceLines.AddNew();
			invLine2.JI_CEI = cei2.PK;

			var merger = new LineMerger(dec);
			merger.DoMerge();

			AssertEquals("CH AND CEI linked together after merging", dec.ActiveEntryHeaders[0].EntryInstruction, cei);
			AssertEquals("CH AND CEI linked together after merging", dec.ActiveEntryHeaders[1].EntryInstruction, cei2);
		}

		public void TestSupportingDocuments()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			SupportingDocument aSuppDoc = invoice.SupportingDocuments.AddNew();
			aSuppDoc.CSI_Code = "123";

			aSuppDoc = invoice.SupportingDocuments.AddNew();
			aSuppDoc.CSI_Code = "456";

			aSuppDoc = declaration.SupportingDocuments.AddNew();
			aSuppDoc.CSI_Code = "789";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			entryHeader = declaration.CustomsEntryHeaders[0].MergedLines[0].Header;
			IEnumerator<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument> aSuppDocToTest = entryHeader.SupportingDocuments.GetEnumerator();

			aSuppDocToTest.MoveNext();
			AssertEquals("aSuppDocToTest.Current.CSI_Code = 789", aSuppDocToTest.Current.CSI_Code, "789");
			aSuppDocToTest.MoveNext();
			AssertEquals("aSuppDocToTest.Current.CSI_Code = 123", aSuppDocToTest.Current.CSI_Code, "123");
			aSuppDocToTest.MoveNext();
			AssertEquals("aSuppDocToTest.Current.CSI_Code = 456", aSuppDocToTest.Current.CSI_Code, "456");
		}

		public void TestAdditionalInfosTestAdditionalInfosSomeMore()
		{
			var uniHelper = new UniversalReferenceTestDataHelper(Factory);
			var addInfCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation;
			var attributeNameValuePairs = new Dictionary<string, string[]>();

			attributeNameValuePairs.Clear();
			attributeNameValuePairs.Add("Level", new string[] { "HEADER" });
			uniHelper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.UnitedKingdom, new string[] { addInfCodeType }, "123", "123", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			uniHelper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.UnitedKingdom, new string[] { addInfCodeType }, "456", "456", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			uniHelper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.UnitedKingdom, new string[] { addInfCodeType }, "789", "789", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			Factory.Save();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo addInfo = invoice.AdditionalInfos.AddNew();
			addInfo.CSI_Code = "123";

			addInfo = invoice.AdditionalInfos.AddNew();
			addInfo.CSI_Code = "456";

			addInfo = declaration.AdditionalInfos.AddNew();
			addInfo.CSI_Code = "789";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			entryHeader = declaration.CustomsEntryHeaders[0].MergedLines[0].Header;
			IEnumerator<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo> addInfoToTest = entryHeader.AdditionalInfos.GetEnumerator();

			addInfoToTest.MoveNext();
			AssertEquals("addInfoToTest.Current.CSI_Code = 789", "789", addInfoToTest.Current.CSI_Code);
			addInfoToTest.MoveNext();
			AssertEquals("addInfoToTest.Current.CSI_Code = 123", "123", addInfoToTest.Current.CSI_Code);
			addInfoToTest.MoveNext();
			AssertEquals("addInfoToTest.Current.CSI_Code = 456", "456", addInfoToTest.Current.CSI_Code);

			addInfoToTest.Dispose();
		}

		public void TestAmendmentReasonCodeIsReadOnlyForCHIEFAndCDS()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			Assert("Amendment reason code should be read only when application code is not CDS", entryHeader.ZG_AmendmentReasonCodeInfo.ReadOnly);

			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			Assert("Amendment reason code should be editable when application code is CDS", entryHeader.ZG_AmendmentReasonCodeInfo.ReadOnly);
		}

		public void TestCustomsMessageRemarksIsReadOnlyWhenCDS()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			Assert("Amendment reason code should be read only when application code is CDS", entryHeader.CH_CustomsMessageRemarksInfo.ReadOnly);
		}

		public void TestAmendmentReasonCodeValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CustomsMessageRemarks = "Comment";

			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			entryHeader.AddInfoValidation.ValidateZG_AmendmentReasonCode();
			AssertNoNotifications(entryHeader.ZG_AmendmentReasonCodeInfo);

			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			entryHeader.AddInfoValidation.ValidateZG_AmendmentReasonCode();
			AssertNoNotifications(entryHeader.ZG_AmendmentReasonCodeInfo);

			entryHeader.CH_CustomsMessageRemarks = string.Empty;
			entryHeader.AddInfoValidation.ValidateZG_AmendmentReasonCode();
			AssertNoNotifications(entryHeader.ZG_AmendmentReasonCodeInfo);
		}

		public void TestSplitReferenceFromCEIGoesIntoMucrForMultiEntryCDSDeclaration_Basic()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_MasterUCR = "HBAC11122222222";
			var cei1 = declaration.CustomsEntryInstructions.AddNew();
			cei1.CEI_Style = "A1";
			cei1.CEI_SplitReference = "01";
			var cei2 = declaration.CustomsEntryInstructions.AddNew();
			cei2.CEI_Style = "A2";
			cei2.CEI_SplitReference = "02";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = cei1.PK;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = cei2.PK;

			SendsMessagesToCustomsShutterUpperer shutup = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.DoMerge(shutup);
			Factory.Save();

			AssertEquals(2, declaration.ActiveEntryHeaders.Count);

			var ceh1 = (CusEntryHeader)invoice.FirstEntryHeader;
			var ceh2 = (CusEntryHeader)invoice.FirstEntryHeader;
			Assert(declaration.ActiveEntryHeaders.OfType<CusEntryHeader>().Any(x => x.CH_MasterUCR.Equals("HBAC11122222222        01")));
			Assert(declaration.ActiveEntryHeaders.OfType<CusEntryHeader>().Any(x => x.CH_MasterUCR.Equals("HBAC11122222222        02")));
		}

		public void TestProcedureCodeWithoutConcession()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoiceLine1 = dec.Invoices.AddNew().InvoiceLines.AddNew();
			var invoiceLine2 = dec.Invoices[0].InvoiceLines.AddNew();
			var entryHeader = dec.ActiveEntryHeaders.AddNew() as CusEntryHeader;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_Procedure = "4000123";
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_Procedure = "4000456";
			AssertEquals("4000", entryHeader.ProcedureCodeWithoutConcession);

			invoiceLine2.JI_Procedure = "9876456";
			AssertEquals(ZString.Empty, entryHeader.ProcedureCodeWithoutConcession);

			invoiceLine1.JI_Procedure = "9876123";
			AssertEquals("9876", entryHeader.ProcedureCodeWithoutConcession);
		}

		public void TestEntryTypeFriendlyName()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_EntryStyle = "IM";
			var invoiceLine1 = dec.Invoices.AddNew().InvoiceLines.AddNew();
			var invoiceLine2 = dec.Invoices[0].InvoiceLines.AddNew();
			dec.CustomsEntryInstructions.RemoveAndDeleteAll();
			var cei = dec.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "H1";
			cei.CEI_SubStyle = "A";

			var entryHeader = dec.ActiveEntryHeaders.AddNew() as CusEntryHeader;
			entryHeader.CH_CEI_Instruction = cei.PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_Procedure = "4000123";
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_Procedure = "9876123";
			AssertEquals("H1 (IMA)", entryHeader.EntryTypeFriendlyName);

			invoiceLine2.JI_Procedure = "4000456";
			AssertEquals("H1 (IMA / 4000)", entryHeader.EntryTypeFriendlyName);
		}

		public void TestIsPreLodgedOrLodgedWithCustoms()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_EntryStyle = "IM";
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			var entry = dec.ActiveEntryHeaders.AddNew() as CusEntryHeader;
			entry.CH_EntryStatus = "123";
			Assert(entry.EntryNumber.IsEmpty);
			Assert(!entry.IsPreLodgedOrLodgedWithCustoms);

			entry.CH_EntryStatus = GBCommonConstants.EntryStatusCodes.DeclarationAccepted;
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryNum = "123";
			entryNumber.Parent = entry;
			entryNumber.CE_EntryType = "IMP";
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			entry.CH_EntryStatus = "ACC";
			Assert(entry.IsPreLodgedOrLodgedWithCustoms);

			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			entry.CH_EntryStatus = "123";
			Assert(!entry.IsPreLodgedOrLodgedWithCustoms);
			entry.CH_EntryStatus = "ACC";
			Assert(entry.IsPreLodgedOrLodgedWithCustoms);
			entry.CH_EntryStatus = string.Empty;
			Assert(!entry.IsPreLodgedOrLodgedWithCustoms);
			entry.CH_EntryStatus = "CLE";
			Assert(entry.IsPreLodgedOrLodgedWithCustoms);
			entry.CH_EntryStatus = "CLR";
			Assert(entry.IsPreLodgedOrLodgedWithCustoms);

			entry.CH_EntryStatus = string.Empty;
			Assert(!entry.IsPreLodgedOrLodgedWithCustoms);
			entry.CH_EntryStatus = CDS.Constants.ThreeCharFunctionCodes.MessageRegistered;
			Assert(entry.IsPreLodgedOrLodgedWithCustoms);
		}

		public void TestFiscalReferences()
		{
			var declaration = Factory.New<JobDeclaration>();

			CommonTestData.CreateFiscalReferenceData(declaration);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entryHeader = declaration.CustomsEntryHeaders[0];

			var refs = entryHeader.FiscalReferences.ToList();

			AssertEquals(1, refs.Count);

			AssertNotNull(refs.FirstOrDefault(x => x.CFR_Code == "FR6" && x.CFR_Reference == "GB66666666"));
		}

		public void TestSettingEntryStatusForCDSDeclarationUpdatesHighestLineNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "CDS";
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entry.CH_HighestLineNumber = 1;

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			AssertEquals(1, (int)entry.CH_HighestLineNumber);

			entry.CH_EntryStatus = "ACC";
			AssertEquals(2, (int)entry.CH_HighestLineNumber);

			var entryLine3 = entry.MergedLines.AddNew();
			entryLine3.CL_LineNumber = 4;
			entry.CH_EntryStatus = "CLE";
			AssertEquals(2, (int)entry.CH_HighestLineNumber);

			entry.CH_EntryStatus = "ACC";
			AssertEquals(4, (int)entry.CH_HighestLineNumber);
		}

		public void TestARAutoPostingDoesNotOccurWhenNotCustomsCleared()
		{
			var invoiceTestHelper = SetupForAutoRatingTest();

			var declaration = RunJobForAutoBillingTest(invoiceTestHelper, "69");
			var job = new JobHeader.Loader(declaration).Load();
			AssertNull("Account job not created", job);
		}

		public void TestARAutoPosting()
		{
			var invoiceTestHelper = SetupForAutoRatingTest();
			var declaration = RunJobForAutoBillingTest(invoiceTestHelper);
			var job = new JobHeader.Loader(declaration).Load();
			AssertNotNull("Account job created", job);
			var charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertEquals("One charge item has been created", 1, charges.Length);
			AssertEquals("Charge amount", 250m, charges[0].JR_LocalCostAmt);
			AssertEquals("Charge type", "DSB", charges[0].JR_ChargeType);
		}

		public void TestARAutoPostingUsingRegistryOverrideWithInvalidCodeForAutoBilling()
		{
			var invoiceTestHelper = SetupForAutoRatingTest();

			var options = CustomsDataRegistry.Instance.EnableAccountingIntegration.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			options.CDSCustomsStatusCodes = "02,03";
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, options);

			var declaration = RunJobForAutoBillingTest(invoiceTestHelper);
			var job = new JobHeader.Loader(declaration).Load();
			AssertNull("Account job created", job);
		}

		public void TestARAutoPostingUsingRegistryOverrideWithValidCodeForAutoBilling()
		{
			var invoiceTestHelper = SetupForAutoRatingTest();

			var options = CustomsDataRegistry.Instance.EnableAccountingIntegration.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			options.CDSCustomsStatusCodes = "01, 02 ,03";
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, options);

			var declaration = RunJobForAutoBillingTest(invoiceTestHelper);
			var job = new JobHeader.Loader(declaration).Load();
			AssertNotNull("Account job created", job);
			var charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertEquals("One charge item has been created", 1, charges.Length);
			AssertEquals("Charge amount", 250m, charges[0].JR_LocalCostAmt);
			AssertEquals("Charge type", "DSB", charges[0].JR_ChargeType);
		}

		public void TestARAutoPostingUsingRegistryOverrideWithValidCodeForAutoBilling09()
		{
			var invoiceTestHelper = SetupForAutoRatingTest();

			var options = CustomsDataRegistry.Instance.EnableAccountingIntegration.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			options.CDSCustomsStatusCodes = ",09  ,";
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, options);

			var declaration = RunJobForAutoBillingTest(invoiceTestHelper, "09");
			var job = new JobHeader.Loader(declaration).Load();
			AssertNotNull("Account job created", job);
			var charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertEquals("One charge item has been created", 1, charges.Length);
			AssertEquals("Charge amount", 250m, charges[0].JR_LocalCostAmt);
			AssertEquals("Charge type", "DSB", charges[0].JR_ChargeType);
		}

		InvoicingTestHelper SetupForAutoRatingTest()
		{
			var cdsResponseHelper = new CDSResponseStatusTestDataHelper(Factory);
			cdsResponseHelper.CreateCDSCustomsStatuses();

			CreateRatingRefZZRecords(Factory);
			var testHelper = new InvoicingTestHelper(Factory);
			testHelper.SetUpDisbursementCreditorAndChargeCode();
			var group = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			group.Staff[0].GS_EmailAddress = "test@cargowise.com";
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new AutoBillingGroupNotification());
			var option = CustomsDataRegistry.Instance.EnableAccountingIntegration.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			option.EnableAccountingIntegration = true;
			option.APPostDSB = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, option);
			return testHelper;
		}

		JobDeclaration RunJobForAutoBillingTest(InvoicingTestHelper invoiceTestHelper, string functionCode = "01")
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_OH_Importer = invoiceTestHelper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_PaymentMethod = "A";
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = "CDS";
			entry.LRN = "LRN123456789000-B0001000";
			var entryLine = entry.MergedLines.AddNew();
			var fee = entryLine.Fees.AddOrUpdate("B00", 250m);
			fee.CF_MethodOfPayment = "A";

			var declarationMessage = Factory.New<CDSNewDeclarationEDIMessage>();
			entry.Messages.Add(declarationMessage);
			entry.CH_Status = Customs.Common.Shared.MessageStatusList.Codes.AwaitingOriginal;
			Factory.Save();
			AssertNull("No invoicing job should have been created as not lodged yet", new JobHeader.Loader(declaration).Load());

			var ediMessage = Factory.New<CDSResponseEDIMessage>();
			ediMessage.EM_IsActive = true;
			ediMessage.EM_IsTestMessage = true;
			ediMessage.EM_ApplicationCode = "CDS";
			ediMessage.EM_ReceiveTransmit = "RCV";
			ediMessage.EM_Status = "QUE";
			ediMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			ediMessage.EM_MessageText = $@"<Response xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
	<FunctionCode>" + functionCode + @"</FunctionCode>
	<FunctionalReferenceID>{entry.PK}</FunctionalReferenceID>
	<IssueDateTime>
		<DateTimeString formatCode=""304"" xmlns = ""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20190704110702Z</DateTimeString>
	</IssueDateTime>
	<Declaration>
		<AcceptanceDateTime>
			<DateTimeString formatCode = ""304"" xmlns = ""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20190704110702Z</DateTimeString>
		</AcceptanceDateTime>
		<FunctionalReferenceID>LRN123456789000-B0001000</FunctionalReferenceID>
		<ID>c41cb7554783489c94e24939cb1ccf51</ID>
		<VersionID>1</VersionID>
	</Declaration>
</Response>";
			ediMessage.EM_LinkTable = "CusEntryHeader";
			ediMessage.EM_LinkUniqueID = entry.PK;
			Factory.Save();

			var processor = new CDSIncomingMessageProcessor(new LoggingInformation());
			processor.ExecuteBatch();
			return declaration;
		}

		public override void TestIsIndirectExport()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var dec = Factory.New<JobDeclaration>();
				var entry = dec.CustomsEntryHeaders.AddNew();

				dec.JE_MessageType = "EXP";
				dec.JE_CustomsOffice = "FR00001";
				Assert("EXP GB->FR", !entry.IsIndirectExport);

				dec.JE_CustomsOffice = "GB00001";
				Assert("EXP GB->GB", !entry.IsIndirectExport);

				dec.JE_MessageType = "IMP";
				dec.JE_CustomsOffice = "FR00001";
				Assert("IMP", !entry.IsIndirectExport);

				dec.JE_MessageType = "EXP";
				dec.JE_CustomsOffice = "";
				Assert("EXP GB->(blank)", !entry.IsIndirectExport);

				var belfast = new RefUNLOCO.Loader(Factory).Load("GBBEL");
				if (!belfast.IsInNorthernIreland)
				{
					var ni = Factory.NewWithValidTestData<RefCountryStates>();
					ni.RW_RegionName = RefUNLOCO.Regions.NorthernIreland;
					belfast.RL_RW = ni.PK;
				}
				Assert("Pre-requisite: GBBEL must be InNorthernIreland!", belfast.IsInNorthernIreland);

				dec.JE_RL_NKOrigin = belfast.RL_Code;
				dec.JE_CustomsOffice = "IE00001";
				Assert("EXP XI->IE", entry.IsIndirectExport);

				dec.JE_CustomsOffice = "GB00001";
				Assert("EXP XI->GB", !entry.IsIndirectExport);
			}
		}

		public void TestIsFSD()
		{
			var dec = Factory.New<JobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();
			dec.JE_MessageType = "IMP";
			entry.CH_CEI_Instruction = dec.CustomsEntryInstructions[0].PK;
			entry.EntryInstruction.CEI_Style = ImportSADDeclarationTypeList.Codes.ImportSupplementaryDeclaration; //ISD
			entry.EntryInstruction.CEI_SubStyle = CodeDescriptionPairLists.EntrySubStyleListImport.Codes.SdpSupplementaryDeclarationOrSdpAndOrLcpEidrFinalSupplementaryDeclaration;
			var line1 = dec.InvoiceLines.AddNew();
			line1.JI_Procedure = JobComInvoiceLine.CfspFsdCPCCode;

			Assert("Should be IsFSD", entry.IsFSD);
		}

		public void TestICanBeImportOrExport()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			var ioe = dec.CustomsEntryHeaders.AddNew() as EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport;

			AssertEquals(dec.CountryCode, ioe.TrueCountryCode);
			AssertEquals(dec.CountryCode, ioe.DataGroupingCode);
		}

		public void TestDateOfLegalAcceptance()
		{
			var dec = Factory.New<JobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();

			AssertEquals("DateOfLegalAcceptance with empty log", ZDate.Empty, entry.DateOfLegalAcceptance);

			var log1 = entry.Logs.AddNew(Events.CustomsEntryStatus, ThreeCharFunctionCode.Codes.ACC);
			var date1 = new ZDateTime(2023, 5, 22);
			using (log1.LockForUpdatingKeyFieldsForTesting())
			{
				log1.SL_EventTime = date1;
			}

			AssertEquals("DateOfLegalAcceptance / one log", date1, entry.DateOfLegalAcceptance);

			var log2 = entry.Logs.AddNew(Events.CustomsEntryStatus, ThreeCharFunctionCode.Codes.ACC);
			var date2 = new ZDateTime(2023, 5, 21);
			using (log2.LockForUpdatingKeyFieldsForTesting())
			{
				log2.SL_EventTime = date2;
			}

			AssertEquals("DateOfLegalAcceptance / two log", date2, entry.DateOfLegalAcceptance);

			var log3 = entry.Logs.AddNew(Events.CustomsEntryStatus, ThreeCharFunctionCode.Codes.ACC);
			var date3 = new ZDateTime(2023, 5, 23);
			using (log3.LockForUpdatingKeyFieldsForTesting())
			{
				log3.SL_EventTime = date3;
			}

			AssertEquals("DateOfLegalAcceptance / three log", date2, entry.DateOfLegalAcceptance);

			var log4 = entry.Logs.AddNew(Events.CustomsEntryStatus, ThreeCharFunctionCode.Codes.EXT);
			var date4 = new ZDateTime(2023, 5, 20);
			using (log4.LockForUpdatingKeyFieldsForTesting())
			{
				log4.SL_EventTime = date4;
			}

			AssertEquals("DateOfLegalAcceptance / four log", date2, entry.DateOfLegalAcceptance);
		}

		public void TestClearanceDateFromCLE()
		{
			var dec = Factory.New<JobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();

			AssertEquals("ClearanceDateFromCLE with empty log", ZDate.Empty, entry.ClearanceDateFromCLE);

			var log1 = entry.Logs.AddNew(Events.CustomsEntryStatus, ThreeCharFunctionCode.Codes.CLE);
			var date1 = new ZDateTime(2023, 6, 2);
			using (log1.LockForUpdatingKeyFieldsForTesting())
			{
				log1.SL_EventTime = date1;
			}

			AssertEquals("ClearanceDateFromCLE / one log", date1, entry.ClearanceDateFromCLE);

			var log2 = entry.Logs.AddNew(Events.CustomsEntryStatus, ThreeCharFunctionCode.Codes.CLE);
			var date2 = new ZDateTime(2023, 5, 3);
			using (log2.LockForUpdatingKeyFieldsForTesting())
			{
				log2.SL_EventTime = date2;
			}

			AssertEquals("ClearanceDateFromCLE / two log", date2, entry.ClearanceDateFromCLE);

			var log3 = entry.Logs.AddNew(Events.CustomsEntryStatus, ThreeCharFunctionCode.Codes.CLE);
			var date3 = new ZDateTime(2023, 7, 1);
			using (log3.LockForUpdatingKeyFieldsForTesting())
			{
				log3.SL_EventTime = date3;
			}

			AssertEquals("ClearanceDateFromCLE / three log", date2, entry.ClearanceDateFromCLE);

			var log4 = entry.Logs.AddNew(Events.CustomsEntryStatus, ThreeCharFunctionCode.Codes.ACC);
			var date4 = new ZDateTime(2023, 4, 4);
			using (log4.LockForUpdatingKeyFieldsForTesting())
			{
				log4.SL_EventTime = date4;
			}

			AssertEquals("ClearanceDateFromCLE / four log", date2, entry.ClearanceDateFromCLE);
		}

		public void TestEntryHeaderStatusDescription()
		{
			var dec = Factory.New<JobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();

			dec.JE_ApplicationCode = "CHF";
			dec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			entry.CH_EntryStatus = "TAX";
			AssertEquals("Unknown", entry.EntryHeaderStatusDescription);

			dec.JE_ApplicationCode = "CDS";
			AssertEquals("Duties and taxes have been calculated and are due", entry.EntryHeaderStatusDescription);
		}

		public void TestInitialiseMasterUCR_Imp_IsMucrSetWhenSingleNonCanceled()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			dec.JE_MasterUCR = "TEST123";
			var entryCanceled = dec.CustomsEntryHeaders.AddNew();
			var entryValid = dec.CustomsEntryHeaders.AddNew();
			Factory.Save();
			entryCanceled.CH_EntryStatus = Customs.Common.EU.EntryStatusList.Codes.Cancelled;
			Factory.Save();
			AssertEquals("TEST123", entryValid.CH_MasterUCR);
		}

		public void TestInitialiseMasterUCR_Imp_IsMucrSetWhenDuplicatingDucrAndSingleEntry()
		{
			var dec1 = Factory.NewWithValidTestData<JobDeclaration>();
			dec1.JE_MessageType = MessageTypeList.Codes.Import;
			dec1.JE_MasterUCR = "TEST123";
			dec1.CustomsEntryHeaders.AddNew();
			Factory.Save();

			var dec2 = Factory.NewWithValidTestData<JobDeclaration>();
			dec2.JE_MessageType = MessageTypeList.Codes.Import;
			dec2.JE_MasterUCR = "TEST234";
			dec2.ClientReferenceForDucr = dec1.JE_DeclarationReference;
			var entry2 = dec2.CustomsEntryHeaders.AddNew();
			Factory.Save();

			Assert(entry2.CH_BGMReference.EndsWith("/1"));
			AssertEquals("TEST234", entry2.CH_MasterUCR);
		}

		public void TestInitialiseMasterUCR_Exp_IsMucrSetIgnoringSlashCancelled()
		{
			var dec1 = Factory.NewWithValidTestData<JobDeclaration>();
			dec1.JE_MessageType = MessageTypeList.Codes.Export;
			dec1.JE_MasterUCR = "TEST123";
			var ceh1 = dec1.CustomsEntryHeaders.AddNew();
			Factory.Save();
			var ceh2 = dec1.CustomsEntryHeaders.AddNew();
			Factory.Save();

			AssertEquals("TEST123", ceh1.CH_MasterUCR);
			AssertEquals("TEST123", ceh2.CH_MasterUCR);
		}

		public void TestSavingLRN_NoDuplicateReferenceException()
		{
			try
			{
				TestingState.IsRunningTests = false;
				AssertEquals("Pre-Requisite: Must simulate runtime", expected: false, TestingState.IsRunningTests);

				var declaration1 = Factory.New<JobDeclaration>();
				declaration1.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				var header = declaration1.CustomsEntryHeaders.AddNew();
				var dbConnection = ((CargoWise.Data.IDbConnected)Factory).Connection;
				header.PopulateLocalReferenceNumberIfNeed();
				var jobReference = header.LRN;
				dbConnection.RollbackTransaction();

				var newFactory = new BusinessObjectFactory();
				var declaration2 = newFactory.New<JobDeclaration>();
				declaration2.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				var header2 = declaration2.CustomsEntryHeaders.AddNew();
				newFactory.Save();
				AssertEquals(jobReference, header2.LRN);

				dbConnection.BeginTransaction();
				AssertEquals(jobReference, header.LRN);
				Factory.Save();
				AssertNotEquals(jobReference, header.LRN);
			}
			finally
			{
				TestingState.IsRunningTests = true;
			}
		}

		public void TestSupplierEoriOfMainOffice()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			var supplier = Factory.New<OrgHeader>();
			declaration.SupplierDocumentaryAddress.OrganisationPK = supplier.PK;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CombineAssertions(() =>
			{
				var customsCode = supplier.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "A12345678", "ES");
				customsCode.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 10);
				AssertEquals("Expected ES EORI", "ESA12345678", entryHeader.SupplierEoriOfMainOffice);

				var customCode2 = supplier.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "XI123456789A", "GB");
				customCode2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 11);
				AssertEquals("Expected XI EORI when there are multiple EORIs", "XI123456789A", entryHeader.SupplierEoriOfMainOffice);

				var customCode3 = supplier.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "987654321", "GB");
				customCode3.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 09);
				AssertEquals("Expected GB EORI when there are multiple EORIs", "GB987654321", entryHeader.SupplierEoriOfMainOffice);

				declaration.SupplierDocumentaryAddress.E2_AddressOverride = ZBool.True;
				declaration.SupplierDocumentaryAddress.E2_GovRegNum = "RR000000A";
				AssertEquals("Expected GovRegNum", "RR000000A", entryHeader.SupplierEoriOfMainOffice);
			});
		}

		public void TestImporterEoriOfMainOffice()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			var importer = Factory.New<OrgHeader>();
			declaration.ImporterDocumentaryAddress.OrganisationPK = importer.PK;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CombineAssertions(() =>
			{
				var customsCode = importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "A12345678", "ES");
				customsCode.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 10);
				AssertEquals("Expected ES EORI", "ESA12345678", entryHeader.ImporterEoriOfMainOffice);

				var customCode2 = importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "XI123456789A", "GB");
				customCode2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 11);
				AssertEquals("Expected XI EORI when there are multiple EORIs", "XI123456789A", entryHeader.ImporterEoriOfMainOffice);

				var customCode3 = importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "987654321", "GB");
				customCode3.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 09);
				AssertEquals("Expected GB EORI when there are multiple EORIs", "GB987654321", entryHeader.ImporterEoriOfMainOffice);

				declaration.ImporterDocumentaryAddress.E2_AddressOverride = ZBool.True;
				declaration.ImporterDocumentaryAddress.E2_GovRegNum = "RR000000A";
				AssertEquals("Expected GovRegNum", "RR000000A", entryHeader.ImporterEoriOfMainOffice);
			});
		}

		protected override (ZString OverseasFreightChargeCode, ZString OverseasInsuranceChargeCode, ZString NotIncludedChargeCode) GetChargeCodesForTotalTAndI()
			=> (ChargesProvider.AirFreightCode, CustomsChargeTypeList.Codes.OverseasInsurance, CustomsChargeTypeList.Codes.PackingCost);

		protected override IChargesCurrencyTestSetup GetChargesCurrencyTestSetup() => new ChargesCurrencyTestSetup();

		void AssertMethodOfPaymentCodePopulated(ICustomsChargeEntry ceh3, string[] expectedCodes)
		{
			var codes = ceh3.GetMethodsOfPaymentThatCanInfluenceAutoRating();
			AssertEquals(2, codes.Length);
			AssertEquals(expectedCodes[0], codes[0]);
			AssertEquals(expectedCodes[1], codes[1]);
		}

		ZString[] AssertMethodOfPaymentCodesEmpty(ICustomsChargeEntry ceh)
		{
			var codes = ceh.GetMethodsOfPaymentThatCanInfluenceAutoRating();
			AssertEquals("When no GB data in RefZZ, use base value of one empty string", 1, codes.Length);
			AssertEquals("", codes[0]);
			return codes;
		}

		EU.Business.Declaration.CusEntryLineFee AddNewFee(EU.Business.Declaration.CusEntryLine entryLine, string tty, string mop, decimal amount)
		{
			var fee = entryLine.Fees.AddNew();
			fee.CF_MethodOfPayment = mop;
			fee.CF_ChargeType = tty;
			fee.CF_ChargeAmount = amount;
			return fee;
		}

		void CreateRatingRefZZRecords(BusinessObjectFactory newFactory)
		{
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			var groupingGB = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom");
			var groupingCDS = helper.CreateNewOrGetExistingDataGrouping("CDS", "Customs Declaration Service");
			newFactory.Save();

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "Method of Payment");
			var mop1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "C", "Anything one", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(mop1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.AutoRating, "Doesn't matter");
			var mop2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "D", "Anything two", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(mop2.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.AutoRating, "Still doesn't matter");
			helper.CreateNewOrGetExistingCusCodeListAttribute(mop2.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.AutoRating, "");

			var mop3 = helper.CreateNewOrGetExistingCusCodeList("CDS", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "A", "Anything three", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(mop3.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.AutoRating, "Doesn't matter");
			var mop4 = helper.CreateNewOrGetExistingCusCodeList("CDS", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "B", "Anything four", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(mop4.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.AutoRating, "Still doesn't matter");
			helper.CreateNewOrGetExistingCusCodeListAttribute(mop4.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.AutoRating, "");

			var a00 = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedKingdom, "A00");
			helper.LoadOrCreateNewCusRateCode(newFactory, "A00", a00.PK);
			var b00 = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedKingdom, "B00");
			helper.LoadOrCreateNewCusRateCode(newFactory, "B00", b00.PK);
			newFactory.Save();
		}

		public void TestSupportModificationState()
		{
			AssertEquals(true, ((Customs.Business.WarehouseExtensions.IWarehouseIntegrationSupporter)Factory.New<CusEntryHeader>()).SupportModificationState);
		}

		public override void TestTotalDutyAmount()
		{
			RunTestForCDS();
		}

		void RunTestForCDS()
		{
			var entry = RunTotalDutyAmountTest("CDS", ApplicationCodeList.Codes.GbCustomsDeclarationServices, "E");

			CombineAssertions(() =>
			{
				AssertEquals("Total Duty Amount", 700m, entry.TotalDutyAmount);
				AssertEquals("Duty Amount", 700m, entry.Duty);
				AssertEquals("VAT", 70m, entry.VAT);

				AssertEquals("B00", entry.TaxCode);
				AssertEquals("A00", entry.DutyCode);

				AssertEquals("DutyImmediate", 400m, entry.DutyImmediate);
				AssertEquals("DutyDeferred", 100m, entry.DutyDeferred);
				AssertEquals("VATImmediate", 40m, entry.VATImmediate);
				AssertEquals("VATDeferred", 10m, entry.VATDeferred);
				AssertEquals("AllOtherFeesDeferred", 40m, entry.AllOtherFeesDeferred);
				AssertEquals("AllOtherFeesImmediate", 10m, entry.AllOtherFeesImmediate);
				AssertEquals("AllOtherFees", 180m, entry.AllOtherFees);
			});
		}

		CusEntryHeader RunTotalDutyAmountTest(ZString dataGrouping, ZString appCode, ZString deferredPaymentMethod)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			Factory.Save();

			var dut = helper.CreateNewOrGetExistingRateType(dataGrouping, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, "Duty");
			var vat = helper.CreateNewOrGetExistingRateType(dataGrouping, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Vat, "VAT");

			helper.LoadOrCreateNewCusRateCode(Factory, RefCusRateCodes.CustomsDutyOnIndustrialProducts, dut.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "A05", dut.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, RefCusRateCodes.ProvisionalAntiDumpingDuty, vat.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, RefCusRateCodes.Vat, vat.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, RefCusRateCodes.VatOnAdditionalDutiesForNorthernIreland, vat.PK);
			Factory.Save();

			var entry = SetupEntryForFeesTest(appCode, RefCusRateCodes.Vat, RefCusRateCodes.CustomsDutyOnIndustrialProducts, deferredPaymentMethod);

			return entry;
		}

		CusEntryHeader SetupEntryForFeesTest(ZString appCode, ZString chargeType1, ZString chargeType2, ZString deferredPaymentMethod)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = appCode;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.ZG_MethodOfPayment = "E";

			var entry = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			CreateFees(entryLine, chargeType1, 1, deferredPaymentMethod);
			CreateFees(entryLine, chargeType2, 10, deferredPaymentMethod);

			var fee1 = entryLine.Fees.AddNew();
			fee1.CF_ChargeType = "ABC";
			fee1.CF_ChargeAmount = 10m;
			fee1.CF_MethodOfPayment = "P";

			var fee2 = entryLine.Fees.AddNew();
			fee2.CF_ChargeType = "DEF";
			fee2.CF_ChargeAmount = 20m;
			fee2.CF_MethodOfPayment = "Y";

			var fee3 = entryLine.Fees.AddNew();
			fee3.CF_ChargeType = "GHI";
			fee3.CF_ChargeAmount = 40m;
			fee3.CF_MethodOfPayment = "E";

			var fee4 = entryLine.Fees.AddNew();
			fee4.CF_ChargeType = "JKL";
			fee4.CF_ChargeAmount = 50m;
			fee4.CF_MethodOfPayment = "F";

			var fee5 = entryLine.Fees.AddNew();
			fee5.CF_ChargeType = "MNO";
			fee5.CF_ChargeAmount = 60m;
			fee5.CF_MethodOfPayment = "D";

			Factory.Save();

			return entry;
		}

		void CreateFees(CusEntryLine entryLine, ZString chargeType, int factor, ZString deferredPaymentMethod)
		{
			var fee1 = entryLine.Fees.AddNew();
			fee1.CF_ChargeType = chargeType;
			fee1.CF_ChargeAmount = 10m * factor;
			fee1.CF_MethodOfPayment = deferredPaymentMethod;
			var fee2 = entryLine.Fees.AddNew();
			fee2.CF_ChargeType = chargeType;
			fee2.CF_ChargeAmount = 20m * factor;
			fee2.CF_MethodOfPayment = "G";
			var fee3 = entryLine.Fees.AddNew();
			fee3.CF_ChargeType = chargeType;
			fee3.CF_ChargeAmount = 40m * factor;
			fee3.CF_MethodOfPayment = "A";
		}

		sealed class ChargesCurrencyTestSetup : IChargesCurrencyTestSetup
		{
			void IChargesCurrencyTestSetup.SetupJobDecWithOFTAndCIFCharges(BaseJobDeclaration declaration, ZString currencyCode)
			{
				declaration.AutoCreateChargesBasedOnIncoTerm = false;

				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_InvoiceAmount = 10000m;
				invoiceHeader.JZ_RX_NKInvoice_Currency = currencyCode;
				invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 10000m;

				var nonDutiableCharge = invoiceHeader.Charges.AddNew();
				nonDutiableCharge.J7_ChargeType = CustomsChargeTypeList.Codes.DeductionCharge;
				nonDutiableCharge.J7_Amount = 200m;
				nonDutiableCharge.J7_IsDutiable = false;
				nonDutiableCharge.J7_IsIncludedInITOT = true;

				var oft = invoiceHeader.Charges.AddNew();
				oft.J7_ChargeType = ChargesProvider.AirFreightCode;
				oft.J7_Amount = 500m;
				oft.J7_RX_NKCurrency = invoiceHeader.Invoice_Currency.RX_Code;
			}

			ZDecimal IChargesCurrencyTestSetup.ExpectedFOB => 10300m;
			ZDecimal IChargesCurrencyTestSetup.ExpectedCIF => 10800m;
		}

		protected override EU.Business.Declaration.JobDeclaration GetNewDeclarationForTesting()
		{
			var dec = base.GetNewDeclarationForTesting();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			return dec;
		}

		protected override BaseJobDeclaration ImportJobDeclaration
		{
			get
			{
				var dec = Factory.New<JobDeclaration>();
				dec.JE_MessageType = ImportJobMessage;
				dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
				return dec;
			}
		}
	}

	static class CommonTestData
	{
		public static void CreateFiscalReferenceData(JobDeclaration declaration)
		{
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLineA = invoice.JobComInvoiceLines.AddNew();
			var invoiceLineB = invoice.JobComInvoiceLines.AddNew();
			var invoiceLineC = invoice.JobComInvoiceLines.AddNew();

			var fr1 = invoiceLineB.FiscalReferences.AddNew();
			fr1.CFR_Code = "FR1";
			fr1.CFR_Reference = "GB11111111";

			var fr2 = invoiceLineB.FiscalReferences.AddNew();
			fr2.CFR_Code = "FR2";
			fr2.CFR_Reference = "GB22222222";

			var fr3 = invoiceLineC.FiscalReferences.AddNew();
			fr3.CFR_Code = "FR1";
			fr3.CFR_Reference = "GB11111111";

			var fr4 = invoiceLineC.FiscalReferences.AddNew();
			fr4.CFR_Code = "FR2";
			fr4.CFR_Reference = "GB33333333";

			var fr5 = invoiceLineC.FiscalReferences.AddNew();
			fr5.CFR_Code = "FR4";
			fr5.CFR_Reference = "GB44444444";

			var fr6 = declaration.CusEntryInstruction.FiscalReferences.AddNew();
			fr6.CFR_Code = "FR6";
			fr6.CFR_Reference = "GB66666666";
		}
	}
}
