using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class IMDJobComInvoiceLineValidationTest : ImportJobComInvoiceLineValidationTest
	{
		public void TestLineValuationBasisList()
		{
			AssertEquals("6 elements in the list", 6, testInvoiceLineValidation.LineValuationBasisList.Count);
		}

		public void TestEmptyTariffIsIfTobaccoOrAlcoholIsDeclared()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			CMRCusEntryCPDec question = entryHeader.Questions.AddNew();
			question.ON_CPDecNum = 18;
			question.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;

			invoiceLine.JI_Tariff = "";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertEquals("Tariff is mandatory for tobacco or alcohol", true, invoiceLine.JI_TariffInfo.HasMessageErrors());

			testDec.JE_MessageType = "FRM";
			invoiceLine.JI_Tariff = "";
			AssertEquals("Tariff is mandatory", true, invoiceLine.JI_TariffInfo.HasMessageErrors());
		}

		public void TestInstrumentCodeListForCMR()
		{
			jobDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			var expiredInstrument = CMRInstrument.New(Factory);
			expiredInstrument.IN_Type = "XX";
			expiredInstrument.IN_Number = "EXP";
			expiredInstrument.IN_StartDate = new ZDateTime(2002, 12, 31);
			expiredInstrument.IN_EndDate = new ZDateTime(2003, 04, 05);

			var revokedInstrument = CMRInstrument.New(Factory);
			revokedInstrument.IN_Type = "XX";
			revokedInstrument.IN_Number = "RVK";
			revokedInstrument.IN_StartDate = new ZDateTime(2002, 12, 31);
			revokedInstrument.IN_RevocationDate = new ZDateTime(2003, 04, 06);

			testInvoiceLine.AddInfo.ZA_InstrumentType_Hidden = "XX";

			invoiceHeader.AddInfo.ZA_EFD = "050403";
			var instrumentList = testInvoiceLine.InstrumentCodeList;
			AssertNotNull("Instrument list", instrumentList);
			AssertNotNull("Expired", instrumentList.GetDescriptionFromCode("EXP"));
			AssertNotNull("Revoked", instrumentList.GetDescriptionFromCode("RVK"));

			invoiceHeader.AddInfo.ZA_EFD = "060403";
			instrumentList = testInvoiceLine.InstrumentCodeList;
			AssertNotNull("Instrument list", instrumentList);
			AssertNull("Expired", instrumentList.GetDescriptionFromCode("EXP"));
			AssertNotNull("Revoked", instrumentList.GetDescriptionFromCode("RVK"));

			invoiceHeader.AddInfo.ZA_EFD = "070403";
			instrumentList = testInvoiceLine.InstrumentCodeList;
			AssertNotNull("Instrument list", instrumentList);
			AssertNull("Expired", instrumentList.GetDescriptionFromCode("EXP"));
			AssertNull("Revoked", instrumentList.GetDescriptionFromCode("RVK"));
		}

		public override void TestValidateDescriptionIsNotTooLong()
		{
			testInvoiceLine.JI_Description = new string('a', 250);
			Assert("!HasWarnings", !testInvoiceLine.JI_DescriptionInfo.HasWarnings());
			testInvoiceLine.JI_Description = new string('a', 251);
			Assert("HasWarnings", testInvoiceLine.JI_DescriptionInfo.HasWarnings());
		}

		public void TestOriginValidation()
		{
			testInvoiceLine.JI_CountryOfOrigin = ZString.Empty;
			AssertEquals("Country/Region of Origin has a message error", true, testInvoiceLine.JI_CountryOfOriginInfo.HasMessageErrors());

			testInvoiceLine.JI_CountryOfOrigin = "AU";
			AssertEquals("Country/Region of Origin doesn't have a message error", false, testInvoiceLine.JI_CountryOfOriginInfo.HasMessageErrors());

			testInvoiceLine.JI_CountryOfOrigin = ZString.Empty;
			AssertEquals("Country/Region of Origin has a message error", true, testInvoiceLine.JI_CountryOfOriginInfo.HasMessageErrors());

			testInvoiceLine.InvoiceHeader.AddInfo.ZA_ORG = "AU";
			testInvoiceLine.Validation.ValidateJI_CountryOfOrigin();
			AssertEquals("Country/Region of Origin doesn't have a message error", false, testInvoiceLine.JI_CountryOfOriginInfo.HasMessageErrors());
		}

		public void TestCustomsQty()
		{
			const string MessageError = "Customs Quantity must be 1 when Luxury Car Tax Payable (LCTI) is set to \"Y\".";
			testInvoiceLine.JI_CustomsUnitQty = "KG";
			testInvoiceLine.AddInfo.ZA_LCTI = ZString.Empty;
			testInvoiceLineValidation.ValidateJI_CustomsQuantity();
			AssertNoMessageError(testInvoiceLine.JI_CustomsQuantityInfo, MessageError);

			testInvoiceLine.AddInfo.ZA_LCTI = "Y";
			testInvoiceLineValidation.ValidateJI_CustomsQuantity();
			AssertHasMessageError(testInvoiceLine.JI_CustomsQuantityInfo, MessageError);

			testInvoiceLine.JI_CustomsQuantity = 2;
			AssertHasMessageError(testInvoiceLine.JI_CustomsQuantityInfo, MessageError);

			testInvoiceLine.JI_CustomsQuantity = 1;
			AssertNoMessageError(testInvoiceLine.JI_CustomsQuantityInfo, MessageError);

			testInvoiceLine.AddInfo.ZA_LCTI = ZString.Empty;
			testInvoiceLine.JI_CustomsQuantity = 9999999999.99999m;
			AssertNoMessageErrorContaining(testInvoiceLine.JI_CustomsQuantityInfo, "Customs Quantity must not be > 9,999,999,999.99999");
			testInvoiceLine.JI_CustomsQuantity = 10000000000m;
			AssertHasMessageErrorContaining(testInvoiceLine.JI_CustomsQuantityInfo, "Customs Quantity must not be > 9,999,999,999.99999");

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Import);
				var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, tariffType.PK, "11111111", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Tariff Code Description", taxOrFeeCode: "GST");
				helper.CreateTariffUOM(tariff1, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "NR");
				var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, tariffType.PK, "22222222", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Tariff Code Description", taxOrFeeCode: "GST");
				helper.CreateTariffUOM(tariff2, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "ERR");
				Factory.Save();

				testInvoiceLine.Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				testInvoiceLine.JI_Tariff = "1111.11.11";
				testInvoiceLine.AddInfo.ZA_LCTI = "Y";
				testInvoiceLine.JI_CustomsQuantity = 2;
				AssertNoMessageErrors("There should be no message errors when tariff's UOM is 'NR'", testInvoiceLine.JI_CustomsQuantityInfo);
				testInvoiceLine.JI_CustomsQuantity = 10000000000m;
				AssertNoMessageErrors("There should be no message errors when tariff's UOM is 'NR'", testInvoiceLine.JI_CustomsQuantityInfo);

				testInvoiceLine.JI_Tariff = "2222.22.22";
				testInvoiceLine.AddInfo.ZA_LCTI = "Y";
				testInvoiceLine.JI_CustomsQuantity = 2;
				AssertNoMessageErrors("There should be no message errors when tariff's UOM is 'ERR'", testInvoiceLine.JI_CustomsQuantityInfo);
				testInvoiceLine.JI_CustomsQuantity = 10000000000m;
				AssertNoMessageErrors("There should be no message errors when tariff's UOM is 'ERR'", testInvoiceLine.JI_CustomsQuantityInfo);
			}
		}

		public void TestTariffValidation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var impTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Import);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "2203006920", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Tariff Code Description", taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "LA");
			helper.CreateTariffUOM(tariff1, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.AdditionalUOMType, "L");
			Factory.Save();

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 1, 1);

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "00000000 00";

				AssertEquals("Is SOFA declaration", false, declaration.IsSOFADeclaration);
				AssertNoWarning(invoiceLine.JI_TariffInfo, CMRImportJobComInvoiceLineValidation.TariffMayNotQualifyForSOFA);

				declaration.AddInfo.ZA_SOFAIndicator_Hidden = true;
				invoiceLine.JI_Tariff = "2203.00.69 20";
				AssertHasWarning(invoiceLine.JI_TariffInfo, CMRImportJobComInvoiceLineValidation.TariffMayNotQualifyForSOFA);
				AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, "Invalid Tariff/Stat code combination for ");

				invoiceLine.JI_Tariff = "9999.30.11 05";
				AssertHasWarning(invoiceLine.JI_TariffInfo, CMRImportJobComInvoiceLineValidation.TariffMayNotQualifyForSOFA);
				AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "Invalid Tariff/Stat code combination for ");
			}
		}

		public void TestTariffValidation_AUCAHECC_WithoutStatClassification()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType("AU", Enterprise.Customs.Universal.Constants.TariffTypes.Import);
			tariffType.ZZI_ZZ9_NKNomenclatureGroupType = "AU";
			helper.LoadOrCreateNewTariff("AU", tariffType.PK, "1234567899", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "test description 1");

			var newFactory = new BusinessObjectFactory();
			var auTariff = newFactory.New<AUCClass>();
			auTariff.UJ_Code = "1234.56.78 90";
			newFactory.Save();

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 1, 1);

				var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "1234.56.78 90";
				AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "Invalid Tariff/Stat code combination for ");

				using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				{
					invoiceLine.JI_Tariff = "1234.56.78 99";
					AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, "Invalid Tariff/Stat code combination for ");
				}
			}
		}

		public void TestTariffValidation_AUCAHECC()
		{
			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				TestCaseHelper.ClearTable(CMRStatisticalClassificationPeriodSnapshot.Schema.TableName);

				invoiceHeader.AddInfo.ZA_EFD = "030305";

				var tariff = CMRStatisticalClassificationPeriodSnapshot.New(Factory);
				tariff.SC_TariffClassificationNumber = "01011000";
				tariff.SC_StatisticalClassificationCode = "02";
				tariff.SC_StartDate = new ZDateTime(2005, 03, 03);

				var expiredTariff = CMRStatisticalClassificationPeriodSnapshot.New(Factory);
				expiredTariff.SC_TariffClassificationNumber = "01011000";
				expiredTariff.SC_StatisticalClassificationCode = "25";
				expiredTariff.SC_EndDate = new ZDateTime(2004, 01, 01);

				testInvoiceLine.Validation.ValidateJI_Tariff();
				AssertHasMessageErrors("Tariff", testInvoiceLine.JI_TariffInfo);

				testInvoiceLine.JI_Tariff = "01011000";
				AssertHasMessageErrors("Tariff", testInvoiceLine.JI_TariffInfo);

				testInvoiceLine.JI_Tariff = "01011000 25";
				AssertHasMessageErrors("Tariff", testInvoiceLine.JI_TariffInfo);

				testInvoiceLine.JI_Tariff = "01011000 2";
				AssertHasMessageErrors("Tariff", testInvoiceLine.JI_TariffInfo);

				testInvoiceLine.JI_Tariff = "01011000 02";
				AssertNoMessageErrors("Tariff", testInvoiceLine.JI_TariffInfo);
			}
		}

		public void TestJI_OH_Supplier()
		{
			testInvoiceLine.Validation.ValidateJI_OH_Supplier();
			AssertNoWarning(testInvoiceLine.JI_OH_SupplierInfo, IMDJobComInvoiceLineValidation.SupplierRequiresCIDWarning);

			var orgHeader = Factory.New<OrgHeader>();
			testInvoiceLine.JI_OH_Supplier = orgHeader.PK;
			AssertNoWarning(testInvoiceLine.JI_OH_SupplierInfo, IMDJobComInvoiceLineValidation.SupplierRequiresCIDWarning);

			testInvoiceLine.Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			testInvoiceLine.Validation.ValidateJI_OH_Supplier();
			AssertHasWarning(testInvoiceLine.JI_OH_SupplierInfo, IMDJobComInvoiceLineValidation.SupplierRequiresCIDWarning);

			orgHeader.CustomsClientID = "AAA111";
			var cusCode = orgHeader.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia);
			testInvoiceLine.Validation.ValidateJI_OH_Supplier();
			AssertNoWarning(testInvoiceLine.JI_OH_SupplierInfo, IMDJobComInvoiceLineValidation.SupplierRequiresCIDWarning);

			cusCode.OK_OA_PremisesAddress = Factory.New<OrgAddress>().PK;
			testInvoiceLine.Validation.ValidateJI_OH_Supplier();
			AssertHasWarning(testInvoiceLine.JI_OH_SupplierInfo, IMDJobComInvoiceLineValidation.SupplierRequiresCIDWarning);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			testInvoiceLine.Declaration.JE_DateAtFinalDestination = new ZDateTime(2010, 01, 01);
			testInvoiceLine.Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			CMRInstrument instrument = CMRInstrument.New(Factory);
			instrument.IN_Type = "BL";
			instrument.IN_StartDate = new ZDateTime(2004, 01, 01);
			instrument.IN_Number = "Test";
		}

		protected override JobComInvoiceLineValidation GetNewValidationProvider(JobComInvoiceLine invoiceLine)
		{
			return new IMDJobComInvoiceLineValidation(invoiceLine);
		}

		#endregion
	}
}
