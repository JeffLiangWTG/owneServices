using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class JobDeclarationCustomsChargesTest : TestCaseWithFactory
	{
		public void TestGSTAmountForCasualImport()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "ABC";
			importer.OH_FullName = "ABC INC.";
			var importerAddInfo = OrgImpAddInfo.Get(importer);
			importerAddInfo.ZO_CADIsBrokerToPay = true;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			var line = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			line.CA_IsCasualImport = true;
			line.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST).C1_Amount = 800m;
			line.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CPT).C1_Amount = 500m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var charges = ServiceLocator.GetService<ICustomsCharges>(declaration).GetCustomsCharges(null);
			AssertCustomsCharge(charges[0], 1300, "Total GST Amount");
		}

		public void TestLVSCustomsCharges()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Canada);
			RatingDataRegistry.Instance.IncludeEntryHeaderReferenceInCustomsDisbursementCharges.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			RatingDataRegistry.Instance.IncludeCustomDeferredChargeInInvoicing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			CACustomsDataRegistry.Instance.DefaultExciseTaxFromCustomsTariff.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var factory = JobComInvoiceLineTestHelper.PopulateDutiesAndTaxesRefFilesReturningFactory();
			var helper = new DeclarationTestHelper(factory, true);
			var canada = factory.Load<RefCountry>(Constants.CountryGuids.Canada);
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForLowValueShipments, "9874", canada);
			var query = new ZQuery(ZArchitecture.Schema.OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = new BusinessObjectFactory().LoadTop1<OrgHeader>(query).PK; //Some company from db
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForLowValueShipments, "6543", canada);
			declaration.JE_CustomsOffice = "351";
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "00006789";

			//lvs id 1
			var lvsID1 = declaration.Invoices.AddNew();
			helper.USD.SetCustomsRate(ZDateTime.Today, ZDateTime.Today, 1.25);
			lvsID1.JZ_RX_NKInvoice_Currency = helper.USD.RX_Code;
			lvsID1.JZ_InvoiceNumber = "LVSID1";
			var buyer1 = helper.CreateOrganisation("IMP", "IMPORTER1 NAME", "CATOR", "IMPORTER1 ADDRESS", "IMPORTER1 CITY", "123 4567");
			var billTo = helper.CreateOrganisation("IMP", "BILLING PARTY NAME", "CATOR", "IMPORTER1 ADDRESS", "IMPORTER1 CITY", "123 4567");
			buyer1.SetRelatedParty(billTo, RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.All, ZString.Empty);
			//((OrgImpAddInfo)importer.CountryData.ImpAddInfo).ZO_IsImporterDirectPayment = true;
			lvsID1.JZ_OH_Buyer = buyer1.PK;
			JobComInvoiceLineTestHelper.FillInvoiceLine(lvsID1.JobComInvoiceLines.AddNew(), 1, 1, 80, 11, 12, 13, Constants.Weight.Kilograms, 16);
			JobComInvoiceLineTestHelper.FillInvoiceLine(lvsID1.JobComInvoiceLines.AddNew(), 1, 1, 80, 11, 12, 13, Constants.Weight.Kilograms, 16);

			//lvs id 2
			var lvsID2 = declaration.Invoices.AddNew();
			lvsID2.JZ_RX_NKInvoice_Currency = helper.USD.RX_Code;
			lvsID2.JZ_InvoiceNumber = "LVSID2";
			lvsID2.JZ_OH_Buyer = buyer1.PK;
			JobComInvoiceLineTestHelper.FillInvoiceLine(lvsID2.JobComInvoiceLines.AddNew(), 1, 1, 80, 11, 12, 13, Constants.Weight.Kilograms, 16);

			//lvs id 3
			var lvsID3 = declaration.Invoices.AddNew();
			lvsID3.JZ_RX_NKInvoice_Currency = helper.USD.RX_Code;
			lvsID3.JZ_InvoiceNumber = "LVSID3";
			var buyer2 = helper.CreateOrganisation("IMP", "IMPORTER2 NAME", "CATOR", "IMPORTER2 ADDRESS", "IMPORTER2 CITY", "123 4567");
			((OrgImpAddInfo)buyer2.CountryData.ImpAddInfo).ZO_IsGSTDirectPayment = true;
			lvsID3.JZ_OH_Buyer = buyer2.PK;
			var lvsID3Line = lvsID3.JobComInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(lvsID3Line, 1, 1, 80, 11, 12, 13, Constants.Weight.Kilograms, 16);
			var simaDuty = lvsID3Line.DutiesAndTaxes[0];
			simaDuty.C1_ExemptCode = SIMACodes.Codes.C32;

			//lvs id 4
			var lvsID4 = declaration.Invoices.AddNew();
			lvsID4.JZ_RX_NKInvoice_Currency = helper.USD.RX_Code;
			lvsID4.JZ_InvoiceNumber = "LVSID4";
			var buyer3 = helper.CreateOrganisation("IMP", "IMPORTER3 NAME", "CATOR", "IMPORTER3 ADDRESS", "IMPORTER3 CITY", "123 4567");
			var impAddInfo = ((OrgImpAddInfo)buyer3.CountryData.ImpAddInfo);
			impAddInfo.ZO_IsGSTDirectPayment = true;
			impAddInfo.ZO_IsGSTDirectAutoRated = true;
			lvsID4.JZ_OH_Buyer = buyer3.PK;
			JobComInvoiceLineTestHelper.FillInvoiceLine(lvsID4.JobComInvoiceLines.AddNew(), 1, 1, 80, 11, 12, 13, Constants.Weight.Kilograms, 0);

			declaration.ResumeApportionment();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			var decAsCustomsCharges = ServiceLocator.GetService<ICustomsCharges>(declaration);
			CustomsCharge[] charges = decAsCustomsCharges.GetCustomsCharges(null);
			Array.Sort(charges, new Customs.Business.Testing.CustomsChargeComparerForTest());
			AssertEquals(8, charges.Length);
			AssertCustomsCharge(charges[0], 16m, buyer2.PK, "Total Non-Billable SIMA Amount", "12345000067897\r\n  LVS IDs: LVSID3", true, false);
			AssertCustomsCharge(charges[1], 48m, billTo.PK, "Total SIMA Amount", "12345000067897\r\n  LVS IDs: LVSID1, LVSID2", false, true);
			AssertCustomsCharge(charges[2], 48m, buyer2.PK, "Total GST Direct Amount", "12345000067897\r\n  LVS IDs: LVSID3", true, false);
			AssertCustomsCharge(charges[3], 48m, buyer3.PK, "Total GST Direct Amount", "12345000067897\r\n  LVS IDs: LVSID4", false, true);
			AssertCustomsCharge(charges[4], 151.68m, billTo.PK, "Total GST Amount", "12345000067897\r\n  LVS IDs: LVSID1, LVSID2", false, true);
			AssertCustomsCharge(charges[5], 200m, buyer2.PK, "Total Duty Amount", "12345000067897\r\n  LVS IDs: LVSID3", false, true);
			AssertCustomsCharge(charges[6], 200m, buyer3.PK, "Total Duty Amount", "12345000067897\r\n  LVS IDs: LVSID4", false, true);
			AssertCustomsCharge(charges[7], 600m, billTo.PK, "Total Duty Amount", "12345000067897\r\n  LVS IDs: LVSID1, LVSID2", false, true);
		}

		void AssertCustomsCharge(CustomsCharge charge, ZDecimal expectedAmount, ZGuid expectedDebbtorPK, ZString expectedDescription, ZString expectedRef, bool expectedIsInfo, bool expectedIsBrokerPay)
		{
			AssertEquals("Invalid Amount", expectedAmount, charge.Amount);
			AssertEquals("Invalid Description", expectedDescription, charge.Description);
			AssertEquals("Invalid DebtorPK", expectedDebbtorPK, charge.DebtorPK);
			AssertEquals("Invalid EntryReference", expectedRef, charge.EntryReference);
			AssertEquals("Invalid IsInformationOnly", expectedIsInfo, charge.IsInformationOnly);
			AssertEquals("Invalid IsPaidByBroker", expectedIsBrokerPay, charge.IsPaidByBroker);
		}

		public void TestB2B3XCustomsCharges()
		{
			AssertB2B3XCustomsCharges(JobMessageTypeList.Codes.B2Adjustments);
			AssertB2B3XCustomsCharges(JobMessageTypeList.Codes.XTypeEntry);
		}

		void AssertB2B3XCustomsCharges(ZString messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			declaration.CA_B2Type = B2TypeList.Codes.Specific;
			var subHeader = declaration.B2AsAccountedForInvoices.AddNew();
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
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			var decAsCustomsCharges = ServiceLocator.GetService<ICustomsCharges>(declaration);
			CustomsCharge[] charges = decAsCustomsCharges.GetCustomsCharges(null);
			Array.Sort(charges, new Customs.Business.Testing.CustomsChargeComparerForTest());
			AssertEquals(3, charges.Length);
			AssertCustomsCharge(charges[0], -100m, "Total Duty Amount");
			AssertCustomsCharge(charges[1], -100m, "Total SIMA Amount");
			AssertCustomsCharge(charges[2], -10m, "Total Excise Tax Amount");
		}

		void AssertCustomsCharge(CustomsCharge charge, ZDecimal expectedAmount, ZString expectedDescription)
		{
			AssertEquals("Invalid Amount", expectedAmount, charge.Amount);
			AssertEquals("Invalid Description", expectedDescription, charge.Description);
			AssertEquals("Invalid DebtorPK", ZGuid.Empty, charge.DebtorPK);
			AssertEquals("Invalid EntryReference", "", charge.EntryReference);
			AssertEquals("Invalid IsInformationOnly", false, charge.IsInformationOnly);
			AssertEquals("Invalid IsPaidByBroker", true, charge.IsPaidByBroker);
		}
	}
}
