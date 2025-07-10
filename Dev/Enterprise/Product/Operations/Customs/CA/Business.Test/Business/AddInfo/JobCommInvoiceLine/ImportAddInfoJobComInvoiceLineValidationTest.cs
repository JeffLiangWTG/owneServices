using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using NUnit.Framework;
using static Enterprise.Customs.CA.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ImportAddInfoJobComInvoiceLineValidationTest : AddInfoJobComInvoiceLineValidationTest
	{
		#region TestCheckCA_ADJValue

		public void TestCheckCA_ADJValue()
		{
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration);
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.WarrantyRepairsRemission;
			invoiceLine.JI_ParentID = ZGuid.NewZGuid();
			invoiceLine.CA_ADJValue = 1m;
			AssertHasMessageError(invoiceLine.CA_ADJValueInfo, ImportJobComInvoiceLineValidation.ValueShouldBeZero);

			invoiceLine.CA_ADJValue = 0m;
			AssertNoMessageError(invoiceLine.CA_ADJValueInfo, ImportJobComInvoiceLineValidation.ValueShouldBeZero);
		}

		#endregion

		#region TestNoNeedValidationForOkaCFIA

		public void TestNoNeedValidationForOkaCFIA()
		{
			invoiceLine.CA_DestinationProvince = ZString.Empty;
			invoiceLine.CA_OGDStatus = AVSStatusList.Codes.WillBeApproved;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_DestinationProvinceInfo);

			invoiceLine.CA_RN_NKCFIAOrigin = "XX";
			invoiceLine.CA_OGDStatus = AVSStatusList.Codes.WillBeApproved;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_RN_NKCFIAOriginInfo);

			invoiceLine.CA_RequirementID = "XXX";
			invoiceLine.CA_OGDStatus = AVSStatusList.Codes.WillBeApproved;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_RequirementIDInfo);

			invoiceLine.CA_RequirementVer = "XXX";
			invoiceLine.CA_OGDStatus = AVSStatusList.Codes.WillBeApproved;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_RequirementVerInfo);

			invoiceLine.CA_AirsCode = "XXX";
			invoiceLine.CA_OGDStatus = AVSStatusList.Codes.WillBeApproved;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_AirsCodeInfo);

			invoiceLine.CA_EndUse = "XXX";
			invoiceLine.CA_OGDStatus = AVSStatusList.Codes.WillBeApproved;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_EndUseInfo);

			invoiceLine.CA_MiscID = "XXX";
			invoiceLine.CA_OGDStatus = AVSStatusList.Codes.WillBeApproved;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_MiscIDInfo);

			invoiceLine.CA_MiscID = "XXX";
			invoiceLine.CA_OGDStatus = AVSStatusList.Codes.WillBeApproved;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_MiscIDInfo);
		}

		#endregion

		#region TestCheckCA_CVforCurrConv

		public void TestCheckCA_CVforCurrConv()
		{
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine.CA_CVforCurrConvOvr = true;
			invoiceLine.CA_CVforCurrConv = 0m;
			AssertHasMessageErrorContaining(invoiceLine.CA_CVforCurrConvInfo, MandatoryValidation.ValueCannotBeZero);
			invoiceLine.CA_CVforCurrConv = -1m;
			AssertHasMessageErrorContaining(invoiceLine.CA_CVforCurrConvInfo, MandatoryValidation.ValueCannotBeNegative);

			invoiceLine.JI_Tariff = JobComInvoiceLine.LuxuryTaxTariffCode;
			invoiceLine.JI_ParentID = Guid.NewGuid();
			invoiceLine.AddInfoValidation.ValidateCA_CVforCurrConv();
			AssertNoMessageErrorContaining(invoiceLine.CA_CVforCurrConvInfo, MandatoryValidation.ValueCannotBeNegative);
			invoiceLine.JI_ParentID = Guid.Empty;
			invoiceLine.JI_Tariff = "";

			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.WarrantyRepairsRemission;
			invoiceLine.JI_ParentID = ZGuid.NewZGuid();
			invoiceLine.CA_CVforCurrConv = 1m;
			AssertHasMessageError(invoiceLine.CA_CVforCurrConvInfo, ImportJobComInvoiceLineValidation.ValueShouldBeZero);

			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			invoiceLine.CA_CVforCurrConv = -1m;
			AssertNoMessageErrorContaining(invoiceLine.CA_CVforCurrConvInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		#endregion

		#region TestCheckCA_AuthorityNumber

		public void TestValidateRemissionEffectiveDate()
		{
			var newFactory = new BusinessObjectFactory();
			var importerOfRecord = newFactory.NewWithValidTestData<OrgHeader>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.ImporterOfRecordAddress.OrganisationPK = importerOfRecord.PK;

			var rulingNoneConfigurations = newFactory.New<Universal.ZZRefCusRulingCombined>();
			rulingNoneConfigurations.ZZX_RulingType = CalculationMethods.Codes.DeliveredDutyPaid;
			rulingNoneConfigurations.ZZX_RulingNumber = "RULING0";
			rulingNoneConfigurations.ZZX_Description = "RULING0xx";
			rulingNoneConfigurations.ZZX_OA_AppliesTo = importerOfRecord.MainAddress.PK;
			rulingNoneConfigurations.ZZX_StartDate = new ZDate(2022, 1, 1);
			rulingNoneConfigurations.ZZX_EndDate = new ZDate(2022, 1, 31);
			rulingNoneConfigurations.ZZX_RN_NKCountryCode = Constants.CountryCodes.Canada;

			var rulingREL = newFactory.New<Universal.ZZRefCusRulingCombined>();
			rulingREL.ZZX_RulingType = CalculationMethods.Codes.DeliveredDutyPaid;
			rulingREL.ZZX_RulingNumber = "RULING1";
			rulingREL.ZZX_Description = "RULING1xx";
			rulingREL.ZZX_OA_AppliesTo = importerOfRecord.MainAddress.PK;
			rulingREL.Configurations.AddNew(Universal.RefCusRulingConfigCategories.Codes.DAT, Universal.RefCusRulingConfigTypes.Codes.REL, ZDecimal.Zero, ZString.Empty);
			rulingREL.ZZX_StartDate = new ZDate(2022, 1, 1);
			rulingREL.ZZX_EndDate = new ZDate(2022, 12, 31);
			rulingREL.ZZX_RN_NKCountryCode = Constants.CountryCodes.Canada;

			var rulingDSD = newFactory.New<Universal.ZZRefCusRulingCombined>();
			rulingDSD.ZZX_RulingType = CalculationMethods.Codes.DeliveredDutyPaid;
			rulingDSD.ZZX_RulingNumber = "RULING2";
			rulingDSD.ZZX_Description = "RULING2xx";
			rulingDSD.ZZX_OA_AppliesTo = importerOfRecord.MainAddress.PK;
			rulingDSD.Configurations.AddNew(Universal.RefCusRulingConfigCategories.Codes.DAT, Universal.RefCusRulingConfigTypes.Codes.DSD, ZDecimal.Zero, ZString.Empty);
			rulingDSD.ZZX_StartDate = new ZDate(2022, 1, 1);
			rulingDSD.ZZX_EndDate = new ZDate(2022, 12, 31);
			rulingDSD.ZZX_RN_NKCountryCode = Constants.CountryCodes.Canada;

			var rulingREL1 = newFactory.New<Universal.ZZRefCusRulingCombined>();
			rulingREL1.ZZX_RulingType = CalculationMethods.Codes.DeliveredDutyPaid;
			rulingREL1.ZZX_RulingNumber = "RULING11";
			rulingREL1.ZZX_Description = "RULING11xx";
			rulingREL1.ZZX_OA_AppliesTo = importerOfRecord.MainAddress.PK;
			rulingREL1.Configurations.AddNew(Universal.RefCusRulingConfigCategories.Codes.DAT, Universal.RefCusRulingConfigTypes.Codes.REL, ZDecimal.Zero, ZString.Empty);
			rulingREL1.ZZX_StartDate = new ZDate(2022, 1, 1);
			rulingREL1.ZZX_EndDate = new ZDate(2022, 1, 31);
			rulingREL1.ZZX_RN_NKCountryCode = Constants.CountryCodes.Canada;

			var rulingDSD1 = newFactory.New<Universal.ZZRefCusRulingCombined>();
			rulingDSD1.ZZX_RulingType = CalculationMethods.Codes.DeliveredDutyPaid;
			rulingDSD1.ZZX_RulingNumber = "RULING21";
			rulingDSD1.ZZX_Description = "RULING21xx";
			rulingDSD1.ZZX_OA_AppliesTo = importerOfRecord.MainAddress.PK;
			rulingDSD1.Configurations.AddNew(Universal.RefCusRulingConfigCategories.Codes.DAT, Universal.RefCusRulingConfigTypes.Codes.DSD, ZDecimal.Zero, ZString.Empty);
			rulingDSD1.ZZX_StartDate = new ZDate(2022, 1, 1);
			rulingDSD1.ZZX_EndDate = new ZDate(2022, 1, 31);
			rulingDSD1.ZZX_RN_NKCountryCode = Constants.CountryCodes.Canada;

			newFactory.Save();
			var emptyDate = ZDateTime.Empty;

			var date0 = new ZDateTime(2022, 10, 10);
			invoiceLine = GetNewInvoiceLine(emptyDate, date0, emptyDate, emptyDate, false);
			invoiceLine.CA_AuthorityNumber = rulingNoneConfigurations.ZZX_RulingNumber;
			AssertHasMessageError(invoiceLine.CA_AuthorityNumberInfo, "Remission Number is on file but is not valid for " + date0.ToShortDateString() + ".");

			var date1 = new ZDateTime(2022, 9, 9);
			invoiceLine = GetNewInvoiceLine(date1, emptyDate, emptyDate, emptyDate, false);
			invoiceLine.CA_AuthorityNumber = rulingDSD.ZZX_RulingNumber;
			AssertNoMessageError(invoiceLine.CA_AuthorityNumberInfo, "Remission Number is on file but is not valid for " + date1.ToShortDateString() + ".");

			invoiceLine = GetNewInvoiceLine(date1, emptyDate, emptyDate, emptyDate, false);
			invoiceLine.CA_AuthorityNumber = rulingDSD1.ZZX_RulingNumber;
			AssertHasMessageError(invoiceLine.CA_AuthorityNumberInfo, "Remission Number is on file but is not valid for " + date1.ToShortDateString() + ".");

			invoiceLine = GetNewInvoiceLine(date1, emptyDate, emptyDate, emptyDate, true);
			invoiceLine.CA_AuthorityNumber = rulingNoneConfigurations.ZZX_RulingNumber;
			AssertHasMessageError(invoiceLine.CA_AuthorityNumberInfo, "Remission Number is on file but is not valid for " + date1.ToShortDateString() + ".");

			var date2 = new ZDateTime(2022, 8, 8);
			invoiceLine = GetNewInvoiceLine(date1, date2, emptyDate, emptyDate, false);
			invoiceLine.CA_AuthorityNumber = rulingREL.ZZX_RulingNumber;
			AssertNoMessageError(invoiceLine.CA_AuthorityNumberInfo, "Remission Number is on file but is not valid for " + date2.ToShortDateString() + ".");

			invoiceLine = GetNewInvoiceLine(date1, date2, emptyDate, emptyDate, false);
			invoiceLine.CA_AuthorityNumber = rulingREL1.ZZX_RulingNumber;
			AssertHasMessageError(invoiceLine.CA_AuthorityNumberInfo, "Remission Number is on file but is not valid for " + date2.ToShortDateString() + ".");

			invoiceLine = GetNewInvoiceLine(date1, date2, emptyDate, emptyDate, true);
			invoiceLine.CA_AuthorityNumber = rulingREL1.ZZX_RulingNumber;
			AssertHasMessageError(invoiceLine.CA_AuthorityNumberInfo, "Remission Number is on file but is not valid for " + date1.ToShortDateString() + ".");

			var date3 = new ZDateTime(2022, 7, 7);
			invoiceLine = GetNewInvoiceLine(date1, date2, date3, emptyDate, false);
			invoiceLine.CA_AuthorityNumber = rulingREL.ZZX_RulingNumber;
			AssertNoMessageError(invoiceLine.CA_AuthorityNumberInfo, "Remission Number is on file but is not valid for " + date3.ToShortDateString() + ".");

			invoiceLine = GetNewInvoiceLine(date1, date2, date3, emptyDate, false);
			invoiceLine.CA_AuthorityNumber = rulingREL1.ZZX_RulingNumber;
			AssertHasMessageError(invoiceLine.CA_AuthorityNumberInfo, "Remission Number is on file but is not valid for " + date3.ToShortDateString() + ".");

			var date4 = new ZDateTime(2022, 6, 6);
			invoiceLine = GetNewInvoiceLine(date1, date2, date3, date4, false);
			invoiceLine.CA_AuthorityNumber = rulingREL.ZZX_RulingNumber;
			AssertNoMessageError(invoiceLine.CA_AuthorityNumberInfo, "Remission Number is on file but is not valid for " + date4.ToShortDateString() + ".");

			invoiceLine = GetNewInvoiceLine(date1, date2, date3, date4, false);
			invoiceLine.CA_AuthorityNumber = rulingREL1.ZZX_RulingNumber;
			AssertHasMessageError(invoiceLine.CA_AuthorityNumberInfo, "Remission Number is on file but is not valid for " + date4.ToShortDateString() + ".");

			JobComInvoiceLine GetNewInvoiceLine(ZDateTime jZ_ValuationDateOverride, ZDateTime jE_DateOfFirstArrival, ZDateTime cA_EstReleaseDate, ZDateTime jE_EntryAuthorisationDate, bool addRulingManully)
			{
				invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.InvoiceHeader.JZ_ValuationDateOverride = jZ_ValuationDateOverride;
				invoiceLine.Declaration.JE_DateOfFirstArrival = jE_DateOfFirstArrival;
				invoiceLine.Declaration.CA_EstReleaseDate = cA_EstReleaseDate;
				invoiceLine.Declaration.JE_EntryAuthorisationDate = jE_EntryAuthorisationDate;
				if (addRulingManully)
				{
					invoiceLine.RulingConfigurations.AddNew(Universal.RefCusRulingConfigCategories.Codes.DAT, Universal.RefCusRulingConfigTypes.Codes.DSD, ZDecimal.Zero, ZString.Empty);
				}

				return invoiceLine;
			}
		}

		[TestDate(2018, 10, 10, 10, 10, 10)]
		public void TestCheckCA_AuthorityNumberAfterMerge()
		{
			using (DutyAndTaxManagerTest.SetReciprocalFlagForCurrentCompany(true))
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				CACustomsDataRegistry.Instance.MinimumVFDForDutyAndTax.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2444.69m);
				var factory = JobComInvoiceLineTestHelper.PopulateDutiesAndTaxesRefFilesForExciseTestReturningFactory();
				var helper = new DeclarationTestHelper(factory, true);
				var declaration = factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "CA-1482";
				invoice.CA_RN_NKExport = Constants.CountryCodes.Japan;
				helper.USD.SetCustomsRate(ZDateTime.Today, ZDateTime.Today, 0.9932);
				invoice.JZ_RX_NKInvoice_Currency = helper.USD.RX_Code;
				invoice.JZ_IncoTerm = Constants.IncoTerms.DeliveredDutyPaid;
				var oft = invoice.Charges.AddNew();
				oft.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				oft.J7_Amount = 100m;
				oft.J7_RX_NKCurrency = "CAD";
				oft.J7_IsIncludedInITOT = true;

				var line = invoice.JobComInvoiceLines.AddNew();
				line.CA_CalculationMethod = CalculationMethods.Codes.DeliveredDutyPaid;
				var simaDuty = line.DutyAndTaxManager.SIMADuties.FirstOrDefault() ?? line.DutiesAndTaxes.AddNew();
				simaDuty.C1_Override = true;
				simaDuty.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
				simaDuty.C1_ExemptCode = SIMACodes.Codes.C31;
				simaDuty.C1_Amount = 20m;
				JobComInvoiceLineTestHelper.FillInvoiceLine(line, 1, 1, "2403.19.00 41", 3000m);
				line.JI_CustomsUnitQty = "NMB";
				line.JI_CustomsQuantity = 20;
				const string warning3 = "The total Value for Duty of this job is less than the minimum but you have not entered an OIC regular remission.";

				var fakeentry = Factory.NewWithValidTestData<CusEntryHeader>();
				fakeentry.CH_JE = declaration.PK;
				declaration.CustomsEntryHeaders.Add(fakeentry);
				fakeentry.AllEntryLines.AddNew();
				line.RunPreSaveValidation();
				AssertHasMessageError("will check AuthorityNumber for validation all on declaration merged", line.CA_AuthorityNumberInfo, warning3);
				declaration.CustomsEntryHeaders.RemoveAndDelete(fakeentry);

				line.AddInfoValidation.ValidateCA_AuthorityNumber();
				AssertNoMessageError(line.CA_AuthorityNumberInfo, warning3);
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				line.AddInfoValidation.ValidateCA_AuthorityNumber();
				AssertNoMessageError(line.CA_AuthorityNumberInfo, warning3);
				var entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
				AssertEquals("MergedLines.Count", 1, entryHeader.MergedLines.Count);
				line.AddInfoValidation.ValidateCA_AuthorityNumber();
				AssertNoMessageError(line.CA_AuthorityNumberInfo, warning3);
				line.JI_CustomsQuantity = 20;
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				line.AddInfoValidation.ValidateCA_AuthorityNumber();
				AssertNoMessageError(line.CA_AuthorityNumberInfo, warning3);
				declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				line.AddInfoValidation.ValidateCA_AuthorityNumber();
				AssertNoMessageError(line.CA_AuthorityNumberInfo, warning3);
				CACustomsDataRegistry.Instance.MinimumVFDForDutyAndTax.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3444.69m);
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				line.AddInfoValidation.ValidateCA_AuthorityNumber();
				AssertHasMessageError(line.CA_AuthorityNumberInfo, warning3);
				line.CA_AuthorityNumber = "test";
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				line.AddInfoValidation.ValidateCA_AuthorityNumber();
				AssertNoMessageError(line.CA_AuthorityNumberInfo, warning3);
			}
		}

		public void TestCheckCA_AuthorityNumber()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var caDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Canada);
			Factory.Save();
			var taxOrFee = helper.CreateTaxOrFee("RT1", 0m, Core.Constants.CountryCodes.Canada, ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1), threshold: 20m);
			Factory.Save();

			const string error = "You should not enter a Special Authority (OIC) on a Total Consolidation (VAR) Type F Declaration";
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			invoiceLine.CA_AuthorityNumber = "123456";
			AssertHasError(invoiceLine.CA_AuthorityNumberInfo, error);

			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			invoiceLine.AddInfoValidation.ValidateCA_AuthorityNumber();
			AssertNoError(invoiceLine.CA_AuthorityNumberInfo, error);

			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			invoiceLine.CA_AuthorityNumber = ZString.Empty;
			AssertNoError(invoiceLine.CA_AuthorityNumberInfo, error);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var invoice2 = declaration2.LVXInvoiceHeader;
			var invoice2Line = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line.CA_AuthorityNumber = "123456";
			AssertNoError(invoice2Line.CA_AuthorityNumberInfo, error);
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(invoice2, declaration);
			invoice2Line.AddInfoValidation.ValidateCA_AuthorityNumber();
			AssertNoError(invoice2Line.CA_AuthorityNumberInfo, error);

			invoice2.JZ_ValuationDateOverride = ZDateTime.Today.AddDays(-2);
			invoice2Line.AddInfoValidation.ValidateCA_AuthorityNumber();
			AssertHasError(invoice2Line.CA_AuthorityNumberInfo, error);

			const string warning2 = "Special Authority Number required with SIMA Code 50";
			const string warning4 = "A Special Authority (OIC) code is required when remission is claimed";
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, invoiceLine.Declaration);
			var dat = invoiceLine.DutiesAndTaxes.AddNew();
			dat.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			dat.C1_ExemptCode = SIMACodes.Codes.C50;
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			declaration.DoMerge();
			invoiceLine.AddInfoValidation.ValidateCA_AuthorityNumber();
			AssertHasMessageError(invoiceLine.CA_AuthorityNumberInfo, warning2);

			dat.C1_TaxType = DutyAndTaxTypes.Codes.SIMADuty;
			dat.C1_ExemptCode = SIMACodes.Codes.C50;
			invoiceLine.AddInfoValidation.ValidateCA_AuthorityNumber();
			AssertHasMessageError(invoiceLine.CA_AuthorityNumberInfo, warning2);
			invoiceLine.JI_FormattedTariff = ZString.Empty;
			invoiceLine.CA_AuthorityNumber = "123456";
			AssertNoMessageError(invoiceLine.CA_AuthorityNumberInfo, warning2);
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			invoiceLine.CA_AuthorityNumber = "";
			AssertNoMessageError(invoiceLine.CA_AuthorityNumberInfo, warning2);
			invoiceLine.AddInfoValidation.ValidateCA_AuthorityNumber();
			AssertNoMessageError(invoiceLine.CA_AuthorityNumberInfo, warning4);
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.NoRemission;
			invoiceLine.AddInfoValidation.ValidateCA_AuthorityNumber();
			AssertNoMessageError(invoiceLine.CA_AuthorityNumberInfo, warning4);
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.OneOneTwentiethRemission;
			invoiceLine.AddInfoValidation.ValidateCA_AuthorityNumber();
			AssertHasMessageError(invoiceLine.CA_AuthorityNumberInfo, warning4);

			var childLine = invoice.JobComInvoiceLines.AddNew();
			childLine.JI_ParentID = invoiceLine.PK;
			childLine.JI_ParentTableCode = invoiceLine.TablePrefix;
			childLine.JI_LineNo = invoiceLine.JI_LineNo + 1;
			childLine.CA_CalculationMethod = CalculationMethods.Codes.RepairsRemission;
			childLine.CA_AuthorityNumber = "";
			childLine.RepairLineSynchroniser.Synchronise(true);
			AssertNoMessageError(childLine.CA_AuthorityNumberInfo, warning4);

			invoice.JZ_IncoTerm = Constants.IncoTerms.DeliveredDutyPaid;
			AssertEquals(CalculationMethods.Codes.DeliveredDutyPaid, invoiceLine.CA_CalculationMethod);
			AssertNoMessageError(invoiceLine.CA_AuthorityNumberInfo, warning4);
		}

		public void TestCheckCA_AuthorityNumber_RemissionRepairLine()
		{
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, invoiceLine.Declaration);
			var dat = invoiceLine.DutiesAndTaxes.AddNew();
			dat.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			dat.C1_ExemptCode = SIMACodes.Codes.C50;
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.RepairsRemission;

			declaration.DoMerge();
			const string warning = "Special Authority Number required with SIMA Code 50";
			AssertHasMessageError(invoiceLine.CA_AuthorityNumberInfo, warning);

			invoiceLine.JI_ParentID = ZGuid.NewZGuid();
			invoiceLine.AddInfoValidation.ValidateCA_AuthorityNumber();
			AssertNoMessageError("Repair remission line should not validate CA_AuthorityNumber", invoiceLine.CA_AuthorityNumberInfo, warning);
		}

		public void TestCheckCA_AuthorityNumberForStandaloneInvoice()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			Assert("Standalone Invoice", !invoice.IsAttachedToPersistentDeclaration);
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var faker = new FakeDeclarationCreatorForInvoice(invoice);
			var declaration = (JobDeclaration)faker.HeaderData;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			const string warning1 = "Special Authority Number required with SIMA Code 50";
			const string warning3 = "A Special Authority (OIC) code is required when remission is claimed";
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, invoiceLine.Declaration);
			var dat = invoiceLine.DutiesAndTaxes.AddNew();
			dat.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			dat.C1_ExemptCode = SIMACodes.Codes.C50;
			invoiceLine.CA_AuthorityNumber = "";
			AssertNoMessageError(invoiceLine.CA_AuthorityNumberInfo, warning1);

			dat.C1_TaxType = DutyAndTaxTypes.Codes.SIMADuty;
			dat.C1_ExemptCode = SIMACodes.Codes.C50;
			invoiceLine.CA_AuthorityNumber = "";
			invoiceLine.AddInfoValidation.ValidateCA_AuthorityNumber();
			AssertNoMessageError(invoiceLine.CA_AuthorityNumberInfo, warning1);
			invoiceLine.JI_FormattedTariff = ZString.Empty;
			invoiceLine.AddInfoValidation.ValidateCA_AuthorityNumber();
			AssertNoMessageError(invoiceLine.CA_AuthorityNumberInfo, warning3);
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.NoRemission;
			invoiceLine.AddInfoValidation.ValidateCA_AuthorityNumber();
			AssertNoMessageError(invoiceLine.CA_AuthorityNumberInfo, warning3);
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.OneOneTwentiethRemission;
			invoiceLine.AddInfoValidation.ValidateCA_AuthorityNumber();
			AssertHasMessageError(invoiceLine.CA_AuthorityNumberInfo, warning3);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice.JZ_JE = declaration2.PK;
			Assert("Invoice attach to declaration", invoice.IsAttachedToPersistentDeclaration);

			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration2);

			var dat2 = invoiceLine2.DutiesAndTaxes.AddNew();
			dat2.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			dat2.C1_ExemptCode = SIMACodes.Codes.C50;
			invoiceLine2.RefreshAggregatedFields();
			invoiceLine2.AddInfoValidation.ValidateCA_AuthorityNumber();
			AssertHasMessageError(invoiceLine2.CA_AuthorityNumberInfo, warning1);
		}

		[ExpectNoExceptions]
		public void TestCheckCA_AuthorityNumber_InvoiceHeaderIsNull()
		{
			invoiceLine.JI_JZ = ZGuid.Empty;
			invoiceLine.CA_AuthorityNumber = "123456";
		}

		public void TestCheckCA_AuthorityNumber_Ruling()
		{
			var warning = "Special Authority Number not found in Rulings table. Either add a new Ruling (F3) or select a valid Ruling (F4).";
			var error = "Special Authority Number is found but is associated with another organization and cannot be used with this organization.";

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var importerOfRecord = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusRuling1 = helper.CreateOrGetRefCusRuling(Core.Constants.CountryCodes.Canada, "C001", "2", ZDate.Today, ZDate.Today.AddDays(1));
			cusRuling1.ZZX_OA_AppliesTo = importer.MainAddress.PK;
			var cusRuling2 = helper.CreateOrGetRefCusRuling(Core.Constants.CountryCodes.Canada, "C002", "2", ZDate.Today, ZDate.Today.AddDays(1));
			cusRuling2.ZZX_OA_AppliesTo = importerOfRecord.MainAddress.PK;

			Factory.Save();

			invoiceLine.CA_AuthorityNumber = "~";
			AssertHasWarning(invoiceLine.CA_AuthorityNumberInfo, warning);

			invoiceLine.CA_AuthorityNumber = "C001";
			AssertNoWarning(invoiceLine.CA_AuthorityNumberInfo, warning);
			AssertNoError(invoiceLine.CA_AuthorityNumberInfo, error);

			invoiceLine.CA_AuthorityNumber = "C002";
			AssertHasError(invoiceLine.CA_AuthorityNumberInfo, error);

			declaration.ImporterOfRecordAddress.OrganisationPK = importerOfRecord.PK;
			invoiceLine.AddInfoValidation.ValidateCA_AuthorityNumber();
			AssertNoError(invoiceLine.CA_AuthorityNumberInfo, error);
		}

		#endregion

		#region TestValidateBlankODGCFIA

		public void TestValidateBlankODGCFIA()
		{
			invoiceLine.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.CFIA);
			invoiceLine.Declaration.CA_OGDCFIA = true;
			AssertHasRowMessageError(invoiceLine, ImportAddInfoJobComInvoiceLineValidation.NoCFIAData);

			invoiceLine.CA_DestinationProvince = CanadianProvinceList.Codes.YukonTerritory;
			invoiceLine.AddInfoValidation.ValidateAll();
			AssertNoRowMessageError(invoiceLine, ImportAddInfoJobComInvoiceLineValidation.NoCFIAData);
		}

		#endregion

		#region TestCheckCA_DestinationProvince

		public void TestCheckCA_DestinationProvince()
		{
			invoiceLine.Declaration.CA_OGDCFIA = true;
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.CA_DestinationProvinceInfo, InvalidCode1, CanadianProvinceList.Codes.YukonTerritory);
			invoiceLine.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.CFIA);
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.CA_DestinationProvinceInfo, InvalidCode1, CanadianProvinceList.Codes.YukonTerritory);

			invoiceLine.Declaration.CA_OGDCFIA = false;
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.CA_DestinationProvinceInfo, InvalidCode1, CanadianProvinceList.Codes.YukonTerritory);
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_DestinationProvinceInfo);
		}

		#endregion

		#region TestCheckCA_RN_NKCFIAOrigin

		public void TestCheckCA_RN_NKCFIAOrigin()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.CA_RN_NKCFIAOriginInfo, InvalidCode2, Constants.CountryCodes.UnitedStates);
		}

		#endregion

		#region TestCheckCA_CFIAUSStateOfOrigin

		public void TestCheckCA_CFIAUSStateOfOrigin()
		{
			invoiceLine.CA_RN_NKCFIAOrigin = Constants.CountryCodes.Canada;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_CFIAUSStateOfOriginInfo);

			invoiceLine.CA_RN_NKCFIAOrigin = Constants.CountryCodes.UnitedStates;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceLine.CA_CFIAUSStateOfOriginInfo, InvalidCode1, USStatesList.Codes.Wyoming);
		}

		#endregion

		#region TestCheckCA_RN_NKExport

		public void TestCheckCA_RN_NKExport()
		{
			const string messageError = "Country/Region of Export must be specified for LVS when tariff treatment code other than 02 and 10.";

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			invoiceLine = (JobComInvoiceLine)declaration.Invoices[0].InvoiceLines.AddNew();
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.CA_RN_NKExportInfo, InvalidCode1, Constants.CountryCodes.UnitedStates);

			invoiceLine.CA_RN_NKExport = ZString.Empty;
			invoiceLine.CA_TreatmentCode = TariffTreatmentCodes.Codes.Norway;
			AssertHasMessageError(invoiceLine.CA_RN_NKExportInfo, messageError);

			invoiceLine.CA_RN_NKExport = Constants.CountryCodes.Canada;
			AssertNoMessageError(invoiceLine.CA_RN_NKExportInfo, messageError);
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.CA_RN_NKExportInfo, InvalidCode1, Constants.CountryCodes.UnitedStates);

			invoiceLine.CA_RN_NKExport = ZString.Empty;
			invoiceLine.CA_TreatmentCode = TariffTreatmentCodes.Codes.UnitedStates;
			AssertNoMessageError(invoiceLine.CA_RN_NKExportInfo, messageError);
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.CA_RN_NKExportInfo, InvalidCode1, Constants.CountryCodes.UnitedStates);

			invoiceLine.CA_RN_NKExport = ZString.Empty;
			invoiceLine.CA_TreatmentCode = ZString.Empty;
			invoice.CA_TreatmentCode = TariffTreatmentCodes.Codes.Norway;
			AssertHasMessageError(invoiceLine.CA_RN_NKExportInfo, messageError);
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.CA_RN_NKExportInfo, InvalidCode1, Constants.CountryCodes.UnitedStates);

			invoiceLine.CA_RN_NKExport = ZString.Empty;
			invoice.CA_TreatmentCode = TariffTreatmentCodes.Codes.UnitedStates;
			AssertNoMessageError(invoiceLine.CA_RN_NKExportInfo, messageError);
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.CA_RN_NKExportInfo, InvalidCode1, Constants.CountryCodes.UnitedStates);

			invoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.CA_RN_NKExportInfo, InvalidCode1, Constants.CountryCodes.UnitedStates);
		}

		#endregion

		#region TestCA_USStateOfExport

		public void TestCA_USStateOfExport()
		{
			invoiceLine.CA_RN_NKExport = Constants.CountryCodes.Canada;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_USStateOfExportInfo);

			invoiceLine.CA_RN_NKExport = Constants.CountryCodes.UnitedStates;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceLine.CA_USStateOfExportInfo, InvalidCode1, USStatesList.Codes.Wyoming);
		}

		#endregion

		#region TestCheckCA_TIIN

		public void TestCheckCA_TIIN()
		{
			invoiceLine.AddInfoValidation.ValidateCA_TIIN();
			AssertNoMessageErrors(invoiceLine.CA_TIINInfo);

			invoiceLine.Declaration.CA_OGDTC = true;
			invoiceLine.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.TC);
			invoiceLine.CA_ImportReasonCode = ImportReasonCodes.Codes.Retread;
			invoiceLine.JI_CountryOfOrigin = Constants.CountryCodes.Ukraine;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.CA_TIINInfo);

			invoiceLine.JI_CountryOfOrigin = Constants.CountryCodes.UnitedStates;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_TIINInfo);

			invoiceLine.CA_ImportReasonCode = ImportReasonCodes.Codes.Export;
			invoiceLine.JI_CountryOfOrigin = Constants.CountryCodes.Ukraine;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_TIINInfo);

			invoiceLine.Declaration.CA_OGDTC = false;
			invoiceLine.CA_ImportReasonCode = ImportReasonCodes.Codes.Retread;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_TIINInfo);
		}

		#endregion

		#region TestCheckCA_TreatmentCode

		public void TestCheckCA_TreatmentCodeWithoutDelcaration()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_StandAloneInvoiceDirection = JobMessageTypeList.Codes.Import;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_TreatmentCode = "AA";
			var validation = invoiceLine.AddInfoValidation;
			AssertNoExceptionThrown(() =>
			{
				validation.ValidateCA_TreatmentCode();
			});
		}

		public void TestCheckCA_TreatmentCodeMatchToProduct()
		{
			var msg = "Treatment Code does not match product code file.";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var importer = Factory.New<OrgHeader>();
			importer.OH_IsConsignee = true;
			declaration.JE_OH_Importer = importer.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PARTNUM";
			var supRelation = part.RelatedOrganisations.AddSupplier(Factory.New<OrgHeader>());
			var importClassification = Factory.New<BaseCusClassification>();
			importClassification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			var importPivot = part.PivotsForBinding.AddNew();
			importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			importPivot.CI_CC = importClassification.PK;

			invoiceLine.InvoiceHeader.JZ_OH_Supplier = supRelation.OU_OH;
			invoiceLine.JI_PartNo = part.OP_PartNum;

			using (CustomsDataRegistry.Instance.SeverityLevelOfInvoiceLineValidationAgainstProductData.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, ProductAuditActions.Codes.AddWarningValidation))
			{
				importPivot.CCA_TreatmentCode = "12";
				invoice.CA_TreatmentCode = "";
				invoiceLine.CA_TreatmentCode = "";
				AssertEquals("", invoiceLine.CA_TreatmentCode);
				AssertEquals("", invoiceLine.CA_TreatmentCodeInfo.Value);
				invoice.CA_TreatmentCode = "12";
				invoiceLine.CA_TreatmentCode = "";
				AssertEquals("12", invoiceLine.CA_TreatmentCode);
				AssertNoWarning(invoiceLine.CA_TreatmentCodeInfo, msg);

				importPivot.CCA_TreatmentCode = "13";
				invoiceLine.AddInfoValidation.ValidateCA_TreatmentCode();
				AssertHasWarning(invoiceLine.CA_TreatmentCodeInfo, msg);

				importPivot.CCA_TreatmentCode = "12";
				invoiceLine.AddInfoValidation.ValidateCA_TreatmentCode();
				AssertNoWarning(invoiceLine.CA_TreatmentCodeInfo, msg);
			}

			using (CustomsDataRegistry.Instance.SeverityLevelOfInvoiceLineValidationAgainstProductData.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, ProductAuditActions.Codes.AddMessageErrorValidation))
			{
				invoiceLine.AddInfoValidation.ValidateCA_TreatmentCode();
				AssertNoMessageError(invoiceLine.CA_TreatmentCodeInfo, msg);

				importPivot.CCA_TreatmentCode = "12";
				invoice.CA_TreatmentCode = "";
				invoiceLine.CA_TreatmentCode = "";
				AssertEquals("", invoiceLine.CA_TreatmentCode);
				AssertEquals("", invoiceLine.CA_TreatmentCodeInfo.Value);
				invoice.CA_TreatmentCode = "12";
				invoiceLine.CA_TreatmentCode = "";
				AssertEquals("12", invoiceLine.CA_TreatmentCode);
				AssertNoMessageError(invoiceLine.CA_TreatmentCodeInfo, msg);

				invoiceLine.CA_TreatmentCode = "12";
				AssertNoMessageError(invoiceLine.CA_TreatmentCodeInfo, msg);

				importPivot.CCA_TreatmentCode = "13";
				invoiceLine.AddInfoValidation.ValidateCA_TreatmentCode();
				AssertHasMessageError(invoiceLine.CA_TreatmentCodeInfo, msg);

				importPivot.CCA_TreatmentCode = string.Empty;
				invoiceLine.AddInfoValidation.ValidateCA_TreatmentCode();
				AssertNoMessageError(invoiceLine.CA_TreatmentCodeInfo, msg);
			}
		}

		[TestDate(2020, 03, 31)]
		public void TestCheckCA_TreatmentCode()
		{
			var classHeader = Factory.New<CACClassHeader>();
			classHeader.ZA_EffectiveDate = ZDateTime.Now.AddDays(-10);
			classHeader.ZA_ExpiryDate = ZDateTime.Now.AddDays(10);
			classHeader.ZA_ClassificationNumber = "1212121212";
			classHeader.ZA_AreaCode = "XXX";
			var rateHeader = classHeader.ClassRates.AddNew();
			rateHeader.ZB_EffectiveDate = ZDateTime.Now.AddDays(-5);
			rateHeader.ZB_ExpiryDate = ZDateTime.Now.AddDays(5);
			var rate = rateHeader.Rates.AddNew();
			rate.ZC_TreatmentCode = "12";
			Factory.Save();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreatePreferenceForCountry(TariffTreatmentCodes.Codes.UnitedStates, TariffTreatmentCodes.Codes.UnitedStates, Constants.CountryCodes.Canada);
			const string invalidCode = "A";
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration);
			ValidationTestHelper.AssertErrorIfNotEntered(invoiceLine.CA_TreatmentCodeInfo);
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.CA_TreatmentCodeInfo, invalidCode, TariffTreatmentCodes.Codes.UnitedStates);
			invoice.CA_TreatmentCode = TariffTreatmentCodes.Codes.UnitedStates;
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.CA_TreatmentCodeInfo, invalidCode, TariffTreatmentCodes.Codes.UnitedStates);

			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			invoice.CA_TreatmentCode = ZString.Empty;
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.CA_TreatmentCodeInfo, invalidCode, TariffTreatmentCodes.Codes.UnitedStates);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			ValidationTestHelper.AssertErrorIfNotEntered(invoiceLine.CA_TreatmentCodeInfo);
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.CA_TreatmentCodeInfo, invalidCode, TariffTreatmentCodes.Codes.UnitedStates);
			invoice.CA_TreatmentCode = TariffTreatmentCodes.Codes.UnitedStates;
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.CA_TreatmentCodeInfo, invalidCode, TariffTreatmentCodes.Codes.UnitedStates);

			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Now;
			invoiceLine.JI_Tariff = "1212121212";
			invoiceLine.CA_TreatmentCode = "12";
			AssertNoMessageErrorContaining(invoiceLine.CA_TreatmentCodeInfo, "No duty rate has been found for classification tariff 1212.12.12 12");
			invoiceLine.CA_TreatmentCode = "13";
			AssertHasMessageErrorContaining(invoiceLine.CA_TreatmentCodeInfo, "No duty rate has been found for classification tariff 1212.12.12 12");
			invoiceLine.JI_Tariff = tariffCode2;
			invoiceLine.AddInfoValidation.ValidateCA_TreatmentCode();
			AssertHasMessageErrorContaining(invoiceLine.CA_TreatmentCodeInfo, "No duty rate has been found");
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.JI_Tariff = "1212121212";
			AssertNoMessageErrorContaining(invoiceLine.CA_TreatmentCodeInfo, "No duty rate has been found for classification tariff 1212.12.12 12");

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.CA_TreatmentCode = TariffTreatmentCodes.Codes.General;
			AssertHasMessageErrorContaining(invoiceLine.CA_TreatmentCodeInfo, "General Rate of Duty (TT 03) is not applicable to US origin shipments");
			invoiceLine.CA_TreatmentCode = TariffTreatmentCodes.Codes.Australia;
			AssertNoMessageErrorContaining(invoiceLine.CA_TreatmentCodeInfo, "General Rate of Duty (TT 03) is not applicable to US origin shipments");

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration1.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var invoice2 = declaration2.LVXInvoiceHeader;
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(invoice2, declaration1);
			var invoiceline2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceline2.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceline2.CA_TreatmentCode = TariffTreatmentCodes.Codes.General;
			AssertHasMessageErrorContaining(invoiceline2.CA_TreatmentCodeInfo, "General Rate of Duty (TT 03) is not applicable to US origin shipments");
		}

		public void TestCheckCA_TreatmentCode_CertificateOfOrigin()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var importer = Factory.New<OrgHeader>();
			importer.OH_IsConsignee = true;
			declaration.JE_OH_Importer = importer.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var part = Factory.New<OrgSupplierPart>();
			invoiceLine.SetPartForTesting(part);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codes = new TariffTreatmentCodes().GetAllCodes().Except(new List<string> { TariffTreatmentCodes.Codes.MostFavouredNation, TariffTreatmentCodes.Codes.General });
			foreach (var item in codes)
			{
				helper.CreatePreferenceForCountry(item, item, Constants.CountryCodes.Canada);
			}

			foreach (var ttCode in codes)
			{
				var doc = Factory.New<JobRequiredDocument>();
				doc.EQ_DocType = Core.Constants.RefDocTypes.CertificateOfOrigin;
				var attr = doc.Attributes.AddNew();
				attr.D0_AttribName = JobRequiredDocAttribTypeList.Codes.TradePreferenceCode;
				attr.D0_AttribValue = ttCode;
				invoiceLine.CA_TreatmentCode = ttCode;

				using (CACustomsDataRegistry.Instance.SeverityLevelForCertificateOfOriginValidations.SetTemporaryValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, ProductAuditActions.Codes.NoAction))
				{
					invoiceLine.AddInfoValidation.ValidateCA_TreatmentCode();
					AssertNoMessageError(invoiceLine.CA_TreatmentCodeInfo, ImportAddInfoJobComInvoiceLineValidation.CheckCertificateOfOrigin);
				}

				using (CACustomsDataRegistry.Instance.SeverityLevelForCertificateOfOriginValidations.SetTemporaryValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, ProductAuditActions.Codes.AddWarningValidation))
				{
					invoiceLine.AddInfoValidation.ValidateCA_TreatmentCode();
					AssertHasWarning(invoiceLine.CA_TreatmentCodeInfo, ImportAddInfoJobComInvoiceLineValidation.CheckCertificateOfOrigin);
				}

				using (CACustomsDataRegistry.Instance.SeverityLevelForCertificateOfOriginValidations.SetTemporaryValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, ProductAuditActions.Codes.AddMessageErrorValidation))
				{
					invoiceLine.AddInfoValidation.ValidateCA_TreatmentCode();
					AssertHasMessageError(invoiceLine.CA_TreatmentCodeInfo, ImportAddInfoJobComInvoiceLineValidation.CheckCertificateOfOrigin);

					part.RequiredDocuments.Add(doc);
					invoiceLine.AddInfoValidation.ValidateCA_TreatmentCode();
					AssertNoMessageError(invoiceLine.CA_TreatmentCodeInfo, ImportAddInfoJobComInvoiceLineValidation.CheckCertificateOfOrigin);

					part.RequiredDocuments.RemoveAll();
					invoiceLine.AddInfoValidation.ValidateCA_TreatmentCode();
					AssertHasMessageError(invoiceLine.CA_TreatmentCodeInfo, ImportAddInfoJobComInvoiceLineValidation.CheckCertificateOfOrigin);

					importer.RequiredDocuments.Add(doc);
					invoiceLine.AddInfoValidation.ValidateCA_TreatmentCode();
					AssertNoMessageError(invoiceLine.CA_TreatmentCodeInfo, ImportAddInfoJobComInvoiceLineValidation.CheckCertificateOfOrigin);

					doc.EQ_ValidToDate = invoiceLine.EffectiveDateForDutyRate.AddDays(-1);
					invoiceLine.AddInfoValidation.ValidateCA_TreatmentCode();
					AssertHasMessageError(invoiceLine.CA_TreatmentCodeInfo, ImportAddInfoJobComInvoiceLineValidation.CheckCertificateOfOrigin);
				}
			}

			invoiceLine.CA_TreatmentCode = TariffTreatmentCodes.Codes.MostFavouredNation;

			using (CACustomsDataRegistry.Instance.SeverityLevelForCertificateOfOriginValidations.SetTemporaryValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, ProductAuditActions.Codes.AddWarningValidation))
			{
				invoiceLine.AddInfoValidation.ValidateCA_TreatmentCode();
				AssertNoWarning(invoiceLine.CA_TreatmentCodeInfo, ImportAddInfoJobComInvoiceLineValidation.CheckCertificateOfOrigin);
			}

			using (CACustomsDataRegistry.Instance.SeverityLevelForCertificateOfOriginValidations.SetTemporaryValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, ProductAuditActions.Codes.AddMessageErrorValidation))
			{
				invoiceLine.AddInfoValidation.ValidateCA_TreatmentCode();
				AssertNoMessageError(invoiceLine.CA_TreatmentCodeInfo, ImportAddInfoJobComInvoiceLineValidation.CheckCertificateOfOrigin);
			}
		}

		#endregion

		#region TestCheckCA_RequirementID

		public void TestCheckCA_RequirementID()
		{
			invoiceLine.Declaration.CA_OGDCFIA = true;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_RequirementIDInfo);

			invoiceLine.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.CFIA);
			invoiceLine.Declaration.CA_OGDCFIA = false;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_RequirementIDInfo);

			invoiceLine.CA_RequirementID = "XX";
			AssertHasErrorContaining(invoiceLine.CA_RequirementIDInfo, "digits");
			invoiceLine.CA_RequirementID = "11";
			AssertNoErrorContaining(invoiceLine.CA_RequirementIDInfo, "digits");
		}

		#endregion

		#region TestCheckCA_RequirementVer

		public void TestCheckCA_RequirementVer()
		{
			invoiceLine.Declaration.CA_OGDCFIA = true;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_RequirementVerInfo);

			invoiceLine.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.CFIA);
			invoiceLine.Declaration.CA_OGDCFIA = false;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_RequirementVerInfo);

			invoiceLine.CA_RequirementVer = "XX";
			AssertHasErrorContaining(invoiceLine.CA_RequirementVerInfo, "digits");
			invoiceLine.CA_RequirementVer = "11";
			AssertNoErrorContaining(invoiceLine.CA_RequirementVerInfo, "digits");
		}

		#endregion

		#region TestCheckCA_AirsCode

		public void TestCheckCA_AirsCode()
		{
			invoiceLine.Declaration.CA_OGDCFIA = true;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_AirsCodeInfo);
			invoiceLine.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.CFIA);

			invoiceLine.Declaration.CA_OGDCFIA = false;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_AirsCodeInfo);

			invoiceLine.Declaration.CA_OGDCFIA = true;
			invoiceLine.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.CFIA);
			invoiceLine.CA_AirsCode = "1";
			AssertNoMessageError(invoiceLine.CA_AirsCodeInfo, ImportAddInfoJobComInvoiceLineValidation.NoCFIATariffMessage);

			invoiceLine.JI_Tariff = "000000000";
			invoiceLine.AddInfoValidation.ValidateCA_AirsCode();
			AssertHasMessageError(invoiceLine.CA_AirsCodeInfo, ImportAddInfoJobComInvoiceLineValidation.NoCFIATariffMessage);

			invoiceLine.CA_AirsCode = ZString.Empty;
			AssertNoMessageError(invoiceLine.CA_AirsCodeInfo, ImportAddInfoJobComInvoiceLineValidation.NoCFIATariffMessage);

			invoiceLine.CA_AirsCode = "XX";
			AssertHasErrorContaining(invoiceLine.CA_AirsCodeInfo, "digits");
			invoiceLine.CA_AirsCode = "11";
			AssertNoErrorContaining(invoiceLine.CA_AirsCodeInfo, "digits");
		}

		#endregion

		#region TestCheckCA_TradeNameForNRCan

		public void TestCheckCA_TradeNameForNRCan()
		{
			invoiceLine.CA_NRCanInd = "Y";
			var nrCanHeader = invoiceLine.NRCanPGAHeader;
			nrCanHeader.CA_EEFProgramInd = "Y";
			nrCanHeader.CA_Category = NRCanIntendedUseCodes.Codes.NR02;
			nrCanHeader.CA_EXPProgramInd = "N";
			invoiceLine.CA_TradeName = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.CA_TradeNameInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.CA_TradeName = "TRADEName";
			AssertNoMessageError(invoiceLine.CA_TradeNameInfo, MandatoryValidation.YouHaveNotEntered);

			nrCanHeader.CA_EXPProgramInd = "Y";
			nrCanHeader.CA_AuthorizedParty = "IMP";
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			invoiceLine.CA_TradeName = "";
			AssertHasMessageErrorContaining(invoiceLine.CA_TradeNameInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.CA_TradeName = "TRADEName";
			AssertNoMessageError(invoiceLine.CA_TradeNameInfo, MandatoryValidation.YouHaveNotEntered);

			nrCanHeader.CA_AuthorizedParty = string.Empty;
			invoiceLine.AddInfoValidation.ValidateCA_TradeName();
			AssertHasMessageErrorContaining(invoiceLine.CA_TradeNameInfo, "Trade Name should keep empty as MFG/Authorized Party is not entered in PGA: Natural Resources Canada.");
		}

		#endregion

		#region TestCheckCA_EndUse

		public void TestCheckCA_EndUse()
		{
			invoiceLine.Declaration.CA_OGDCFIA = true;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_EndUseInfo);

			invoiceLine.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.CFIA);
			invoiceLine.Declaration.CA_OGDCFIA = false;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_EndUseInfo);

			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.CA_EndUseInfo, "??", "01");
		}

		#endregion

		#region TestCheckCA_MiscID

		public void TestCheckCA_MiscID()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("CFIAM", "CFIA Misc Codes");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Canada, "CFIAM", "1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.CA_MiscIDInfo, "XX", "1");
		}

		#endregion

		#region TestCheckCA_CustomsValue

		public void TestCheckCA_CustomsValue()
		{
			const string messageError = "At least one Customs Duty Rate must be specified.";
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Warehouse10;
			invoiceLine.AddInfoValidation.ValidateCA_CustomsValue();
			AssertHasMessageError(invoiceLine.CA_CustomsValueInfo, messageError);

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse22;
			invoiceLine.AddInfoValidation.ValidateCA_CustomsValue();
			AssertNoMessageError(invoiceLine.CA_CustomsValueInfo, messageError);

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Warehouse10;
			var duty = invoiceLine.DutiesAndTaxes.AddNew();
			duty.C1_Override = true;
			duty.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			AssertNoMessageError(invoiceLine.CA_CustomsValueInfo, messageError);

			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.WarrantyRepairsRemission;
			invoiceLine.JI_ParentID = ZGuid.NewZGuid();
			invoiceLine.CA_CustomsValue = 1m;
			AssertHasMessageError(invoiceLine.CA_CustomsValueInfo, ImportJobComInvoiceLineValidation.ValueShouldBeZero);

			invoiceLine.CA_CustomsValue = 0m;
			AssertNoMessageError(invoiceLine.CA_CustomsValueInfo, ImportJobComInvoiceLineValidation.ValueShouldBeZero);

			var cadEntry = declaration.ActiveEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;

			invoiceLine.DutiesAndTaxes.DeleteAll();
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse101;
			invoiceLine.AddInfoValidation.ValidateCA_CustomsValue();
			AssertHasMessageError(invoiceLine.CA_CustomsValueInfo, messageError);

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse102;
			invoiceLine.AddInfoValidation.ValidateCA_CustomsValue();
			AssertHasMessageError(invoiceLine.CA_CustomsValueInfo, messageError);

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse101;
			duty = invoiceLine.DutiesAndTaxes.AddNew();
			duty.C1_Override = true;
			duty.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			invoiceLine.AddInfoValidation.ValidateCA_CustomsValue();
			AssertNoMessageError(invoiceLine.CA_CustomsValueInfo, messageError);

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse102;
			invoiceLine.AddInfoValidation.ValidateCA_CustomsValue();
			AssertNoMessageError(invoiceLine.CA_CustomsValueInfo, messageError);
		}

		#endregion

		#region TestCheckCA_99TariffCode

		public void TestCheckCA_99TariffCode()
		{
			var messageError = "Tariff code does not match pattern '99XX' or '00XX'";
			invoiceLine.CA_99TariffCode = "2321";
			AssertHasMessageError(invoiceLine.CA_99TariffCodeInfo, messageError);
			invoiceLine.CA_99TariffCode = "9924";
			AssertNoMessageError(invoiceLine.CA_99TariffCodeInfo, messageError);
			invoiceLine.CA_99TariffCode = "0031";
			AssertNoMessageError(invoiceLine.CA_99TariffCodeInfo, messageError);
			invoiceLine.CA_99TariffCode = "";
			AssertNoMessageError(invoiceLine.CA_99TariffCodeInfo, messageError);

			var warning = "A Tariff Code may be required with 1/60 Remission calculation method.";
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.OneSixtiethRemission;
			AssertHasWarning(invoiceLine.CA_99TariffCodeInfo, warning);

			warning = "A Tariff Code is required with 1/120 Remission calculation method.";
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.OneOneTwentiethRemission;
			AssertHasWarning(invoiceLine.CA_99TariffCodeInfo, warning);

			invoiceLine.CA_99TariffCode = "9924";
			AssertNoWarning(invoiceLine.CA_99TariffCodeInfo, warning);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var invoice = declaration.LVXInvoiceHeader;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.CA_99TariffCode = "2321";
			AssertNoMessageError(invoiceLine2.CA_99TariffCodeInfo, messageError);
		}

		#endregion

		#region TestCheckCA_CalculationMethod

		public void TestCheckCA_CalculationMethod()
		{
			const string error = "You may not claim remission on a Total Consolidation (VAR) Type F Declaration";
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.OneSixtiethRemission;
			AssertHasError(invoiceLine.CA_CalculationMethodInfo, error);

			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			invoiceLine.AddInfoValidation.ValidateCA_CalculationMethod();
			AssertNoError(invoiceLine.CA_CalculationMethodInfo, error);

			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			invoiceLine.CA_CalculationMethod = ZString.Empty;
			AssertNoError(invoiceLine.CA_CalculationMethodInfo, error);

			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.NoRemission;
			AssertNoError(invoiceLine.CA_CalculationMethodInfo, error);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var invoice2 = declaration2.LVXInvoiceHeader;
			var invoice2Line = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line.CA_CalculationMethod = CalculationMethods.Codes.OneOneTwentiethRemission;
			AssertNoError(invoice2Line.CA_CalculationMethodInfo, error);
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(invoice2, declaration);
			invoice2Line.AddInfoValidation.ValidateCA_CalculationMethod();
			AssertHasError(invoice2Line.CA_CalculationMethodInfo, error);

			invoice2.JZ_IncoTerm = Constants.IncoTerms.DeliveredDutyPaid;
			AssertEquals(CalculationMethods.Codes.DeliveredDutyPaid, invoice2Line.CA_CalculationMethod);
			AssertNoError(invoice2Line.CA_CalculationMethodInfo, error);
		}

		#endregion

		#region TestCheckCA_ValueForDutyCode

		public void TestCheckCA_ValueForDutyCode()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.CA_ValueForDutyCodeInfo, "99", "28");
			invoiceLine.InvoiceHeader.CA_ValueForDutyCode = ZString.Empty;
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageErrorForValidationType(invoiceLine.CA_ValueForDutyCodeInfo, ValidateForMessageType.B3CUSDEC, declaration);
			invoiceLine.JI_Tariff = tariffCode1;
			invoiceLine.AddInfoValidation.ValidateCA_ValueForDutyCode();
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_ValueForDutyCodeInfo);
			invoiceLine.JI_Tariff = "";
			invoiceLine.AddInfoValidation.ValidateCA_ValueForDutyCode();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageErrorForValidationType(invoiceLine.CA_ValueForDutyCodeInfo, ValidateForMessageType.B3CUSDEC, declaration);
			invoiceLine.JI_ParentID = Guid.NewGuid();
			invoiceLine.JI_Tariff = JobComInvoiceLine.LuxuryTaxTariffCode;
			invoiceLine.AddInfoValidation.ValidateCA_ValueForDutyCode();
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_ValueForDutyCodeInfo);
			invoiceLine.JI_ParentID = Guid.Empty;
			invoiceLine.JI_Tariff = "";
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_ValueForDutyCodeInfo);
			invoiceLine.InvoiceHeader.CA_ValueForDutyCode = "28";
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration);
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_ValueForDutyCodeInfo);
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_ValueForDutyCodeInfo);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			invoiceLine.InvoiceHeader.CA_ValueForDutyCode = ZString.Empty;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_ValueForDutyCodeInfo);
		}

		public void TestCheckCA_ValueForDutyCode_WarrantyLine()
		{
			invoiceLine.CA_ValueForDutyCode = ValueForDutyCodes.Codes.RelatedFirmsComputedValue;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.WarrantyRepairsRemission;
			AssertEquals(2, invoice.InvoiceLines.Count);

			var childLine = (JobComInvoiceLine)invoice.InvoiceLines[1];
			childLine.CA_ValueForDutyCode = ValueForDutyCodes.Codes.RelatedFirmsDeductiveValue;
			AssertHasMessageError(childLine.CA_ValueForDutyCodeInfo, ImportAddInfoJobComInvoiceLineValidation.VFDCodeShouldBe29);

			childLine.CA_ValueForDutyCode = ValueForDutyCodes.Codes.RelatedFirmsResidualMethodValue;
			AssertNoMessageError(childLine.CA_ValueForDutyCodeInfo, ImportAddInfoJobComInvoiceLineValidation.VFDCodeShouldBe29);

			invoiceLine.CA_ValueForDutyCode = ValueForDutyCodes.Codes.UnrelatedFirmsPaidPayableWithoutAdjustments;
			childLine.CA_ValueForDutyCode = ValueForDutyCodes.Codes.UnrelatedFirmsPaidPayableWithoutAdjustments;
			AssertHasMessageError(childLine.CA_ValueForDutyCodeInfo, ImportAddInfoJobComInvoiceLineValidation.VFDCodeShouldBe19);

			childLine.CA_ValueForDutyCode = ValueForDutyCodes.Codes.UnrelatedFirmsResidualMethodValue;
			AssertNoMessageError(childLine.CA_ValueForDutyCodeInfo, ImportAddInfoJobComInvoiceLineValidation.VFDCodeShouldBe19);
		}

		#endregion

		#region TestCheckCA_PageNumber

		public void TestCheckCA_PageNumber()
		{
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration);
			AssertEquals("PageNumber default 0", 0, invoiceLine.CA_PageNumber);
			invoiceLine.AddInfoValidation.ValidateCA_PageNumber();
			AssertHasMessageErrorContaining(invoiceLine.CA_PageNumberInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_ParentID = Guid.NewGuid();
			invoiceLine.JI_Tariff = JobComInvoiceLine.LuxuryTaxTariffCode;
			invoiceLine.AddInfoValidation.ValidateCA_PageNumber();
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_PageNumberInfo);
			invoiceLine.JI_ParentID = Guid.Empty;
			invoiceLine.JI_Tariff = "";
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_PageNumberInfo);

			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.CA_PageNumberInfo);
		}

		#endregion

		#region TestCheckCA_CasualImportCommodity

		public void TestCheckCA_CasualImportCommodity()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Canada, RefCusCodeListType.Codes.CasualImportCommodity,
				"Cigarettes", "Cigarettes", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Names.Type, CasualImportConstants.CasualImpCommodityType.Tobacco);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Canada, RefCusCodeListType.Codes.CasualImportCommodity,
				"Cigars", "Cigars", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Names.Type, CasualImportConstants.CasualImpCommodityType.Tobacco);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Canada, RefCusCodeListType.Codes.CasualImportCommodity,
				"Wine", "Wine", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Names.Type, CasualImportConstants.CasualImpCommodityType.Alcohol);
			Factory.Save();
			ValidationTestHelper.AssertErrorIfInvalidCode(invoiceLine.CA_CasualImportCommodityInfo, InvalidCode1, "Cigarettes");

			var warning = "There are no appropriate units for this commodity on Duty & Tax Tab page.";
			var tariffCode = "5402.39.00";
			var unit1 = "LTR";
			var unit2 = "NMB";
			var unit3 = "GRM";
			var commodity1 = "Cigars";
			var commodity2 = "Wine";

			invoiceLine.JI_FormattedTariff = tariffCode;
			invoiceLine.JI_CustomsSecondUnitQty = "";
			invoiceLine.JI_CustomsThirdUnitQty = "";
			invoiceLine.CA_CasualImportCommodity = commodity1;
			AssertHasWarning(invoiceLine.CA_CasualImportCommodityInfo, warning);
			invoiceLine.CA_CasualImportCommodity = commodity2;
			AssertHasWarning(invoiceLine.CA_CasualImportCommodityInfo, warning);

			invoiceLine.JI_CustomsSecondUnitQty = unit1;
			invoiceLine.CA_CasualImportCommodity = commodity1;
			AssertHasWarning(invoiceLine.CA_CasualImportCommodityInfo, warning);
			invoiceLine.CA_CasualImportCommodity = commodity2;
			AssertNoWarning(invoiceLine.CA_CasualImportCommodityInfo, warning);

			invoiceLine.JI_CustomsSecondUnitQty = unit2;
			invoiceLine.CA_CasualImportCommodity = commodity1;
			AssertNoWarning(invoiceLine.CA_CasualImportCommodityInfo, warning);
			invoiceLine.CA_CasualImportCommodity = commodity2;
			AssertHasWarning(invoiceLine.CA_CasualImportCommodityInfo, warning);

			invoiceLine.JI_CustomsSecondUnitQty = unit3;
			invoiceLine.CA_CasualImportCommodity = commodity1;
			AssertNoWarning(invoiceLine.CA_CasualImportCommodityInfo, warning);
			invoiceLine.CA_CasualImportCommodity = commodity2;
			AssertNoWarning(invoiceLine.CA_CasualImportCommodityInfo, warning);
		}

		#endregion

		#region TestCheckCA_CasualImportDestinationProvince

		public void TestCheckCA_CasualImportDestinationProvince()
		{
			ValidationTestHelper.AssertErrorIfInvalidCode(invoiceLine.CA_CasualImportDestinationProvinceInfo, InvalidCode1, CanadianProvinceList.Codes.Alberta);

			invoiceLine.CA_IsCasualImport = false;
			invoice.CA_IsCasualImport = false;
			invoiceLine.CA_CasualImportDestinationProvince = ZString.Empty;
			invoice.CA_CasualImportDestinationProvince = ZString.Empty;
			invoiceLine.AddInfoValidation.ValidateCA_CasualImportDestinationProvince();
			AssertNoError(invoiceLine.CA_CasualImportDestinationProvinceInfo, ImportAddInfoJobComInvoiceLineValidation.NoCasualImportDestinationProvinceMessage);

			invoice.CA_IsCasualImport = true;
			invoiceLine.AddInfoValidation.ValidateCA_CasualImportDestinationProvince();
			AssertHasError(invoiceLine.CA_CasualImportDestinationProvinceInfo, ImportAddInfoJobComInvoiceLineValidation.NoCasualImportDestinationProvinceMessage);
			invoice.CA_CasualImportDestinationProvince = CanadianProvinceList.Codes.Alberta;
			invoiceLine.AddInfoValidation.ValidateCA_CasualImportDestinationProvince();
			AssertNoError(invoiceLine.CA_CasualImportDestinationProvinceInfo, ImportAddInfoJobComInvoiceLineValidation.NoCasualImportDestinationProvinceMessage);
			invoice.CA_CasualImportDestinationProvince = ZString.Empty;
			invoiceLine.CA_CasualImportDestinationProvince = CanadianProvinceList.Codes.Alberta;
			invoiceLine.AddInfoValidation.ValidateCA_CasualImportDestinationProvince();
			AssertNoError(invoiceLine.CA_CasualImportDestinationProvinceInfo, ImportAddInfoJobComInvoiceLineValidation.NoCasualImportDestinationProvinceMessage);

			invoiceLine.CA_IsCasualImport = true;
			invoiceLine.CA_CasualImportDestinationProvince = ZString.Empty;
			AssertHasError(invoiceLine.CA_CasualImportDestinationProvinceInfo, ImportAddInfoJobComInvoiceLineValidation.NoCasualImportDestinationProvinceMessage);
			invoice.CA_CasualImportDestinationProvince = CanadianProvinceList.Codes.Alberta;
			invoiceLine.AddInfoValidation.ValidateCA_CasualImportDestinationProvince();
			AssertNoError(invoiceLine.CA_CasualImportDestinationProvinceInfo, ImportAddInfoJobComInvoiceLineValidation.NoCasualImportDestinationProvinceMessage);
			invoice.CA_CasualImportDestinationProvince = ZString.Empty;
			invoiceLine.CA_CasualImportDestinationProvince = CanadianProvinceList.Codes.Alberta;
			invoiceLine.AddInfoValidation.ValidateCA_CasualImportDestinationProvince();
			AssertNoError(invoiceLine.CA_CasualImportDestinationProvinceInfo, ImportAddInfoJobComInvoiceLineValidation.NoCasualImportDestinationProvinceMessage);

			invoice.CA_IsCasualImport = false;
			invoiceLine.CA_CasualImportDestinationProvince = ZString.Empty;
			AssertHasError(invoiceLine.CA_CasualImportDestinationProvinceInfo, ImportAddInfoJobComInvoiceLineValidation.NoCasualImportDestinationProvinceMessage);
			invoice.CA_CasualImportDestinationProvince = CanadianProvinceList.Codes.Alberta;
			invoiceLine.AddInfoValidation.ValidateCA_CasualImportDestinationProvince();
			AssertNoError(invoiceLine.CA_CasualImportDestinationProvinceInfo, ImportAddInfoJobComInvoiceLineValidation.NoCasualImportDestinationProvinceMessage);
			invoice.CA_CasualImportDestinationProvince = ZString.Empty;
			invoiceLine.CA_CasualImportDestinationProvince = CanadianProvinceList.Codes.Alberta;
			invoiceLine.AddInfoValidation.ValidateCA_CasualImportDestinationProvince();
			AssertNoError(invoiceLine.CA_CasualImportDestinationProvinceInfo, ImportAddInfoJobComInvoiceLineValidation.NoCasualImportDestinationProvinceMessage);
		}

		public void TestCheckCA_CasualImportDestinationProvinceForDoesNotMatch()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.MainAddress.OA_RL_NKRelatedPortCode = "CAYUL";
			consignee.MainAddress.OA_City = "MONTREAL";
			consignee.MainAddress.OA_State = "QC";

			var consignee1 = Factory.New<OrgHeader>();
			consignee1.MainAddress.OA_RL_NKRelatedPortCode = "CAVAN";
			consignee1.MainAddress.OA_City = "VANCOUVER";
			consignee1.MainAddress.OA_State = "BC";

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = dec.Invoices.AddNew();
			invoice.JZ_OH_Consignee = consignee.PK;
			invoice.CA_IsCasualImport = true;
			AssertEquals("QC", invoice.CA_CasualImportDestinationProvince);

			var invoiceLine = dec.FilteredInvoiceLines.AddNew();
			invoiceLine.CA_IsCasualImport = true;
			AssertEquals("QC", invoiceLine.CA_CasualImportDestinationProvince);
			AssertNoWarning(invoiceLine.CA_CasualImportDestinationProvinceInfo, ImportAddInfoJobComInvoiceHeaderValidation.DestinationProvinceDoesNotMatch);
			invoiceLine.CA_CasualImportDestinationProvince = "ON";
			AssertHasWarning(invoiceLine.CA_CasualImportDestinationProvinceInfo, ImportAddInfoJobComInvoiceHeaderValidation.DestinationProvinceDoesNotMatch);

			invoiceLine.CA_CasualImportDestinationProvince = ZString.Empty;
			invoiceLine.JI_OA_ConsigneeAddress = consignee1.MainAddress.PK;
			AssertEquals("BC", invoiceLine.CA_CasualImportDestinationProvince);
			AssertNoWarning(invoiceLine.CA_CasualImportDestinationProvinceInfo, ImportAddInfoJobComInvoiceHeaderValidation.DestinationProvinceDoesNotMatch);
			invoiceLine.CA_CasualImportDestinationProvince = "ON";
			AssertHasWarning(invoiceLine.CA_CasualImportDestinationProvinceInfo, ImportAddInfoJobComInvoiceHeaderValidation.DestinationProvinceDoesNotMatch);
		}

		#endregion

		#region TestCheckCA_CFIACountryOfSource

		public void TestCheckCA_CFIACountryOfSource()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.CA_CFIACountryOfSourceInfo, "??", Constants.CountryCodes.Canada);
		}

		#endregion

		#region TestCheckCA_CFIAStateOfSource

		public void TestCheckCA_CFIAStateOfSource()
		{
			var countryCode1 = Constants.CountryCodes.UnitedStates;
			var countryCode2 = Constants.CountryCodes.Canada;
			var countryCode3 = Constants.CountryCodes.Albania;
			var usState = USStatesList.Codes.Alabama;
			var invalidState = "??";
			invoiceLine.CA_CFIACountryOfSource = countryCode1;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceLine.CA_CFIAStateOfSourceInfo, invalidState, usState);
			invoiceLine.CA_CFIACountryOfSource = countryCode2;
			ValidationTestHelper.AssertIfIsEnteredMessageError(invoiceLine.CA_CFIAStateOfSourceInfo);
			invoiceLine.CA_CFIACountryOfSource = countryCode3;
			ValidationTestHelper.AssertIfIsEnteredMessageError(invoiceLine.CA_CFIAStateOfSourceInfo);
		}

		#endregion

		#region TestCheckCA_TradeName

		public void TestCheckCA_TradeName()
		{
			invoiceLine.CA_DFOInd = YesNoList.Codes.Yes;

			var pgaHeader = invoiceLine.DFOPGAHeader;
			pgaHeader.CA_ABIProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_HasGeneticModification = true;

			invoiceLine.AddInfoValidation.ValidateCA_TradeName();
			AssertHasMessageError(invoiceLine.CA_TradeNameInfo, "You have not entered a value.");
		}

		public void TestCheckCA_TradeName_CPRProgram()
		{
			invoiceLine.CA_HCInd = YesNoList.Codes.Yes;

			var pgaHeader = invoiceLine.HCPGAHeader;
			pgaHeader.CA_CPRProgramInd = YesNoList.Codes.Yes;

			invoiceLine.AddInfoValidation.ValidateCA_TradeName();
			AssertHasWarning(invoiceLine.CA_TradeNameInfo, ImportAddInfoJobComInvoiceLineValidation.CPR_TradeNameShouldNotBeEmptyMessage);

			invoiceLine.CA_TradeName = "ABC";
			AssertNoWarning(invoiceLine.CA_TradeNameInfo, ImportAddInfoJobComInvoiceLineValidation.CPR_TradeNameShouldNotBeEmptyMessage);
		}

		#endregion

		#region TestCheckCA_ExpiryDate

		public void TestCheckCA_ExpiryDate()
		{
			invoiceLine.CA_HCInd = YesNoList.Codes.Yes;

			var pgaHeader = invoiceLine.HCPGAHeader;
			pgaHeader.CA_CTOProgramInd = YesNoList.Codes.Yes;

			pgaHeader.CA_CategoryCTO = HCCategories.Codes.HC27;
			invoiceLine.AddInfoValidation.ValidateCA_ExpiryDate();
			AssertHasWarning(invoiceLine.CA_ExpiryDateInfo, "It is strongly recommended to provide the Expiry Date for Tissues.");

			pgaHeader.CA_CategoryCTO = HCCategories.Codes.HC26;
			invoiceLine.AddInfoValidation.ValidateCA_ExpiryDate();
			AssertNoWarning(invoiceLine.CA_ExpiryDateInfo, "It is strongly recommended to provide the Expiry Date for Tissues.");

			invoiceLine.Declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 10, 26);
			invoiceLine.CA_ExpiryDate = new ZDateTime(2017, 10, 24);

			AssertHasMessageError(invoiceLine.CA_ExpiryDateInfo, "Expiry Date is in the past before release is obtained.");
		}

		#endregion

		#region TestCheckCA_RN_NKSource

		public void TestCheckCA_RN_NKSource()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.CA_RN_NKSourceInfo, "~", "US");
		}

		#endregion

		#region TestCheckCA_StateOfSource

		public void TestCheckCA_StateOfSource()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertNoMessageErrorContaining(invoiceLine.CA_StateOfSourceInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.CA_RN_NKSource = "US";
			AssertNoMessageErrorContaining(invoiceLine.CA_StateOfSourceInfo, MandatoryValidation.YouHaveNotEntered);

			var cfiaHeader = invoiceLine.CFIAPGAHeader;
			cfiaHeader.CA_AllProgramInd = YesNoList.Codes.Yes;

			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.CA_StateOfSourceInfo, "~", "NY");

			invoiceLine.CA_StateOfSource = string.Empty;
			AssertHasMessageErrorContaining(invoiceLine.CA_StateOfSourceInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.CA_StateOfSource = "NY";
			AssertNoMessageErrorContaining(invoiceLine.CA_StateOfSourceInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.CA_RN_NKSource = "CA";
			invoiceLine.CA_StateOfSource = string.Empty;
			AssertNoMessageErrorContaining(invoiceLine.CA_StateOfSourceInfo, MandatoryValidation.YouHaveNotEntered);
		}

		#endregion

		#region TestCheckCA_ProductionDate

		public void TestCheckCA_ProductionDate()
		{
			invoiceLine.CA_HCInd = YesNoList.Codes.Yes;

			var pgaHeader = invoiceLine.HCPGAHeader;
			pgaHeader.CA_APIProgramInd = YesNoList.Codes.Yes;
			invoiceLine.AddInfoValidation.ValidateCA_ProductionDate();
			AssertHasMessageErrorContaining(invoiceLine.CA_ProductionDateInfo, MandatoryValidation.YouHaveNotEntered);

			pgaHeader.CA_HDRProgramInd = YesNoList.Codes.Yes;
			invoiceLine.AddInfoValidation.ValidateCA_ProductionDate();
			AssertHasMessageErrorContaining(invoiceLine.CA_ProductionDateInfo, MandatoryValidation.YouHaveNotEntered);

			pgaHeader.CA_NHPProgramInd = YesNoList.Codes.Yes;
			invoiceLine.AddInfoValidation.ValidateCA_ProductionDate();
			AssertHasMessageErrorContaining(invoiceLine.CA_ProductionDateInfo, MandatoryValidation.YouHaveNotEntered);

			pgaHeader.CA_VETProgramInd = YesNoList.Codes.Yes;
			invoiceLine.AddInfoValidation.ValidateCA_ProductionDate();
			AssertHasMessageErrorContaining(invoiceLine.CA_ProductionDateInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.CA_ProductionDate = ZDateTime.Now;
			AssertNoMessageErrorContaining(invoiceLine.CA_ProductionDateInfo, MandatoryValidation.YouHaveNotEntered);

			pgaHeader.CA_CPRProgramInd = YesNoList.Codes.Yes;
			invoiceLine.AddInfoValidation.ValidateCA_ProductionDate();
		}

		public void TestCheckCA_ProductionDate_CPRProgram()
		{
			invoiceLine.CA_HCInd = YesNoList.Codes.Yes;
			var pgaHeader = invoiceLine.HCPGAHeader;
			pgaHeader.CA_CPRProgramInd = YesNoList.Codes.Yes;

			invoiceLine.AddInfoValidation.ValidateCA_ProductionDate();
			AssertHasWarning(invoiceLine.CA_ProductionDateInfo, ImportAddInfoJobComInvoiceLineValidation.CPR_ProductionDateShouldNotBeEmptyMessage);

			invoiceLine.CA_ProductionDate = ZDateTime.Now;
			AssertNoWarning(invoiceLine.CA_ProductionDateInfo, ImportAddInfoJobComInvoiceLineValidation.CPR_ProductionDateShouldNotBeEmptyMessage);
		}

		#endregion

		#region TestCheckCA_VINNumber

		public void TestCheckCA_VINNumberForTC()
		{
			invoiceLine.CA_TCInd = YesNoList.Codes.Yes;
			var availabePrograms = new[]
			{
				TCPGAVehicleProgramCodes.Codes.PIL,
				TCPGAVehicleProgramCodes.Codes.VCC,
				TCPGAVehicleProgramCodes.Codes.VFS,
				TCPGAVehicleProgramCodes.Codes.VFC,
				TCPGAVehicleProgramCodes.Codes.VAE,
				TCPGAVehicleProgramCodes.Codes.VCR,
				TCPGAVehicleProgramCodes.Codes.VUV,
				TCPGAVehicleProgramCodes.Codes.VVP
			};

			AssertMandatoryProperty(invoiceLine.CA_VINNumberInfo, availabePrograms);

			invoiceLine.TCPGAHeader.CA_VPRProgramInd = YesNoList.Codes.Yes;
			invoiceLine.TCPGAHeader.CA_SubProgram = TCPGAVehicleProgramCodes.Codes.VFS;

			AssertCA_VINNumberFormatIsRight();
		}

		public void TestCheckCA_VINNumberForECCC()
		{
			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;
			AssertCA_VINNumberFormatIsRight();
		}

		void AssertCA_VINNumberFormatIsRight()
		{
			var modelYearsUseCommonFormat = new ZString[] { "1981", ZString.Empty, "Test" };

			foreach (var year in modelYearsUseCommonFormat)
			{
				invoiceLine.CA_ModelYear = year;

				invoiceLine.CA_VINNumber = "0123456789123456~";
				AssertHasMessageErrorContaining(invoiceLine.CA_VINNumberInfo, "VIN should be 17 alphanumeric.");

				invoiceLine.CA_VINNumber = "0123456789123456";
				AssertHasMessageErrorContaining(invoiceLine.CA_VINNumberInfo, "VIN should be 17 alphanumeric.");

				invoiceLine.CA_VINNumber = "01234567891234567";
				AssertNoMessageErrorContaining(invoiceLine.CA_VINNumberInfo, "VIN should be 17 alphanumeric.");
			}

			invoiceLine.CA_ModelYear = "1980";
			invoiceLine.CA_VINNumber = "012345~";
			AssertHasMessageErrorContaining(invoiceLine.CA_VINNumberInfo, "VIN should be 5-13 alphanumeric.");

			invoiceLine.CA_VINNumber = "0123";
			AssertHasMessageErrorContaining(invoiceLine.CA_VINNumberInfo, "VIN should be 5-13 alphanumeric.");

			invoiceLine.CA_VINNumber = "01234";
			AssertNoMessageErrorContaining(invoiceLine.CA_VINNumberInfo, "VIN should be 5-13 alphanumeric.");

			invoiceLine.CA_VINNumber = "0123456789123";
			AssertNoMessageErrorContaining(invoiceLine.CA_VINNumberInfo, "VIN should be 5-13 alphanumeric.");
		}

		#endregion

		#region TestCheckCA_ModelYear

		public void TestCheckCA_ModelYear()
		{
			invoiceLine.CA_TCInd = YesNoList.Codes.Yes;
			var availabePrograms = new[]
			{
				TCPGAVehicleProgramCodes.Codes.PIL,
				TCPGAVehicleProgramCodes.Codes.VCC,
				TCPGAVehicleProgramCodes.Codes.VFS,
				TCPGAVehicleProgramCodes.Codes.VFC,
				TCPGAVehicleProgramCodes.Codes.VAE,
				TCPGAVehicleProgramCodes.Codes.VCR,
				TCPGAVehicleProgramCodes.Codes.VVP
			};

			AssertMandatoryProperty(invoiceLine.CA_ModelYearInfo, availabePrograms);
			AssertListValidationProperty(invoiceLine.CA_ModelYearInfo, availabePrograms.Append(TCPGAVehicleProgramCodes.Codes.VUV));
		}

		#endregion

		public void TestCheckCA_ModelYear_ForECCC()
		{
			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;
			var header = invoiceLine.ECCCPGAHeader;
			CombineAssertions(() =>
			{
				header.CA_ProcessCode = ProcessCodes.Codes.XE04;
				header.InvoiceLine.AddInfoValidation.ValidateCA_ModelYear();
				AssertHasMessageErrorContaining("XE04", header.InvoiceLine.CA_ModelYearInfo, MandatoryValidation.YouHaveNotEntered);
				header.CA_ProcessCode = ProcessCodes.Codes.XE03;
				header.InvoiceLine.AddInfoValidation.ValidateCA_ModelYear();
				AssertNoMessageErrorContaining("Not XE04", header.InvoiceLine.CA_ModelYearInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckCA_RemissionType()
		{
			invoiceLine.CA_RemissionType = "ABC";
			AssertHasMessageErrorContaining(invoiceLine.CA_RemissionTypeInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.CA_RemissionType = RemissionTypeList.Codes.ComplianceCaseNumber;
			AssertNoMessageErrorContaining(invoiceLine.CA_RemissionTypeInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			invoiceLine.CA_AuthorityNumber = "5299";
			invoiceLine.CA_RemissionType = ZString.Empty;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.CA_RemissionTypeInfo);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			invoiceLine.AddInfoValidation.ValidateCA_RemissionType();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.CA_RemissionTypeInfo);
		}

		#region Implementation

		void AssertMandatoryProperty(ZPropertyInfo propertyInfo, IEnumerable<string> availablePrograms, string messageError = "You have not entered")
		{
			CombineAssertions(() =>
			{
				foreach (var programCode in invoiceLine.TCPGAHeader.AddInfoLookups.ProgramCodesList.GetAllCodes())
				{
					if (programCode == TCPGADepartmentCodes.Codes.TPR)
					{
						invoiceLine.TCPGAHeader.CA_TPRProgramInd = YesNoList.Codes.Yes;
					}
					else
					{
						invoiceLine.TCPGAHeader.CA_VPRProgramInd = YesNoList.Codes.Yes;
						invoiceLine.TCPGAHeader.CA_SubProgram = programCode;
					}

					invoiceLine.AddInfoValidation.ValidateAll();
					AssertEquals($"propertyInfo:{propertyInfo.Name}, programCode:{programCode}, isMandatoryAvailabe:{availablePrograms.Contains(programCode)}"
						, availablePrograms.Contains(programCode)
						, propertyInfo.Notifications.GetMessageErrors().ContainsNotificationContaining(messageError));

					if (programCode == TCPGADepartmentCodes.Codes.TPR)
					{
						invoiceLine.TCPGAHeader.CA_TPRProgramInd = YesNoList.Codes.No;
					}
					else
					{
						invoiceLine.TCPGAHeader.CA_VPRProgramInd = YesNoList.Codes.No;
						invoiceLine.TCPGAHeader.CA_SubProgram = string.Empty;
					}
				}
			});
		}

		void AssertListValidationProperty(ZPropertyInfo propertyInfo, IEnumerable<string> availablePrograms)
		{
			CombineAssertions(() =>
			{
				foreach (var programCode in invoiceLine.TCPGAHeader.AddInfoLookups.ProgramCodesList.GetAllCodes())
				{
					if (programCode == TCPGADepartmentCodes.Codes.TPR)
					{
						invoiceLine.TCPGAHeader.CA_TPRProgramInd = YesNoList.Codes.Yes;
					}
					else
					{
						invoiceLine.TCPGAHeader.CA_VPRProgramInd = YesNoList.Codes.Yes;
						invoiceLine.TCPGAHeader.CA_SubProgram = programCode;
					}

					propertyInfo.Value = (ZString)"~";

					invoiceLine.AddInfoValidation.ValidateAll();
					AssertEquals($"propertyInfo:{propertyInfo.Name}, programCode:{programCode}, isListAvailabe:{availablePrograms.Contains(programCode)}"
						, availablePrograms.Contains(programCode)
						, propertyInfo.Notifications.GetMessageErrors().ContainsNotificationContaining(ListValidation.InvalidCodeMessageError.ToString()));

					if (programCode == TCPGADepartmentCodes.Codes.TPR)
					{
						invoiceLine.TCPGAHeader.CA_TPRProgramInd = YesNoList.Codes.No;
					}
					else
					{
						invoiceLine.TCPGAHeader.CA_VPRProgramInd = YesNoList.Codes.No;
						invoiceLine.TCPGAHeader.CA_SubProgram = string.Empty;
					}
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
		}

		const string InvalidCode1 = "12";
		const string InvalidCode2 = "07";
		const string tariffCode1 = "0000999907";
		const string tariffCode2 = "0000999900";

		#endregion
	}
}
