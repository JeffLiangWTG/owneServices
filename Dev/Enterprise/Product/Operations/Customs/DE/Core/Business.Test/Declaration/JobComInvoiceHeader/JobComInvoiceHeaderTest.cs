using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.DE;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants;
using CustomsChargeTypeList = Enterprise.Customs.Business.CustomsChargeTypeList;
using ICommonInvoice = Enterprise.Customs.Business.ICommonInvoice;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceHeader))]
	sealed class JobComInvoiceHeaderTest : EU.Business.Declaration.Testing.JobComInvoiceHeaderTest<JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine>
	{
		[TestDate(2022, 01, 17)]
		public void TestJZ_ValuationDateOverrideIsNotCopied()
		{
			invoiceHeader.JZ_ValuationDateOverride = ZDateTime.Now;
			var clone = (JobComInvoiceHeader)invoiceHeader.Clone();
			AssertNotEquals("Not equals", clone.JZ_ValuationDateOverride, invoiceHeader.JZ_ValuationDateOverride);
		}

		public void TestValidation()
		{
			var header = GetNewInvoiceHeader();

			CombineAssertions(() =>
			{
				header.JobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertType<ExportJobComInvoiceHeaderValidation>("If an invoice header is export, it should return the export validation", header.Validation);

				header.JobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertType<ImportJobComInvoiceHeaderValidation>("If an invoice header is import, it should return the import validation", header.Validation);

				var entryInstruction = header.JobDeclaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;
				AssertType<StockMovementJobComInvoiceHeaderValidation>("If an invoice header is import and all CEI_Style of CustomsEntryInstructions is 'LUZ'", header.Validation);

				header.JobDeclaration.JE_MessageType = DEJobMessageTypeList.Codes.WarehouseAdjustment;
				AssertType<WarehouseAdjustmentJobComInvoiceHeaderValidation>("If an invoice header is warehouseAdjustment, it should return warehouseAdjustment validation", header.Validation);

				header.JobDeclaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
				AssertType<JobComInvoiceHeaderValidation>("Other JE_MessageType, it should return the base validation", header.Validation);
			});
		}

		public void TestIsStockMovement()
		{
			var invoice = GetNewInvoiceHeader();
			invoice.JobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				AssertEquals("JobDeclaration.IsStockMovement is false", ZBool.False, invoice.IsStockMovement);

				var entryInstruction = invoice.JobDeclaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;
				AssertEquals("JobDeclaration.IsStockMovement is true", ZBool.True, invoice.IsStockMovement);
			});
		}

		public void TestNeedAtLeastOneInvoiceSupportingDocument()
		{
			var invoice = GetNewInvoiceHeader();
			invoice.JobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			var supportingDocument = invoice.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = "AAA";
			CombineAssertions(() =>
			{
				AssertEquals("Not StockMovement", ZBool.True, invoice.NeedAtLeastOneInvoiceSupportingDocument);

				var entryInstruction = invoice.JobDeclaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;
				AssertEquals("Is StockMovement", ZBool.False, invoice.NeedAtLeastOneInvoiceSupportingDocument);
			});
		}

		public void TestAdditionalInfos()
		{
			var header = GetNewInvoiceHeader();
			AssertEquals(typeof(AdditionalInfoCollection), header.AdditionalInfos.GetType());
		}

		public void TestJZ_JE()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_UCR = "ABC";
			var invoice = declaration.Invoices.AddNew();
			AssertEquals(declaration.PK, invoice.JZ_JE);
			AssertEquals("ABC", invoice.JZ_UCR);
		}

		public void TestZG_AgreedPlaceCode()
		{
			var header = GetNewInvoiceHeader();
			AddInfoManagerTestHelper.AssertWrappedProperty(header, nameof(header.ZG_AgreedPlaceCode), AutoEUAddInfo.Schema.ZG_AgreedPlaceCode, (ZString)"1");
			AssertEquals(1, header.ZG_AgreedPlaceCodeInfo.MaxLength);
		}

		public void TestJZ_AgreedPlaceCodeDefaultValuesWithIncoTermUpdate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			CombineAssertions(() =>
			{
				invoice.JZ_IncoTerm = ZString.Empty;
				AssertEquals("JZ_IncoTerm = ''", ZString.Empty, invoice.ZG_AgreedPlaceCode);

				invoice.JZ_IncoTerm = IncotermA1840CodeList.Codes.CFR;
				AssertEquals("JZ_IncoTerm = CFR", UniversalReferenceConstants.AgreedPlaceCodes._3, invoice.ZG_AgreedPlaceCode);

				invoice.JZ_IncoTerm = IncotermA1840CodeList.Codes.EXW;
				AssertEquals("JZ_IncoTerm = EXW", UniversalReferenceConstants.AgreedPlaceCodes._1, invoice.ZG_AgreedPlaceCode);

				invoice.JZ_IncoTerm = "ZZZ";
				AssertEquals("Invalid JZ_IncoTerm, ZG_AgreedPlaceCode not updated", UniversalReferenceConstants.AgreedPlaceCodes._1, invoice.ZG_AgreedPlaceCode);

				invoice.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
				AssertEquals("JZ_IncoTerm = XXX", ZString.Empty, invoice.ZG_AgreedPlaceCode);
			});
		}

		public void TestJZ_IncoTerm_DoeNotSetDefaultAgreedPlace()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = IncotermA1840CodeList.Codes.CFR;
			AssertEquals(string.Empty, invoice.ZG_AgreedPlaceCode);
		}

		public void TestDefaultValuesWithMessageType()
		{
			var header = GetNewInvoiceHeader();
			var declaration = header.JobDeclaration;
			declaration.JE_UCR = "ABC";
			AssertEquals(ZString.Empty, header.JZ_UCR);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("ABC", header.JZ_UCR);
		}

		public void TestGroupCharges()
		{
			AssertType<JobComInvApportionedChargeCollection<InvoiceApportionCharge>>(GetNewInvoiceHeader().GroupCharges);
		}

		public void TestCharges()
		{
			AssertType<InvoiceChargeCollection<InvoiceCharge>>(GetNewInvoiceHeader().Charges);
		}

		public override void TestChargeTypeList()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			ICommonInvoice commonInvoice = dec.Invoices.AddNew();

			var chargeTypeList1 = commonInvoice.ChargeTypeList;
			var chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));

			var customsChargeTypeList = new CustomsChargeTypeList();
			customsChargeTypeList.RemoveCode(Common.CustomsChargeTypeList.Codes.OverseasFreight);
			customsChargeTypeList.RemoveCode(CustomsChargeTypeList.Codes.AdditionCharge);
			customsChargeTypeList.RemoveCode(CustomsChargeTypeList.Codes.Commission);
			customsChargeTypeList.RemoveCode(CustomsChargeTypeList.Codes.DeductionCharge);
			customsChargeTypeList.RemoveCode(CustomsChargeTypeList.Codes.ExWorks);
			customsChargeTypeList.RemoveCode(CustomsChargeTypeList.Codes.ForeignInlandFreight);
			customsChargeTypeList.RemoveCode(CustomsChargeTypeList.Codes.LandingCharges);
			customsChargeTypeList.RemoveCode(CustomsChargeTypeList.Codes.OverseasInsurance);
			customsChargeTypeList.RemoveCode(CustomsChargeTypeList.Codes.OtherCharges);
			customsChargeTypeList.RemoveCode(CustomsChargeTypeList.Codes.PackingCost);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._001, ImportChargeCodeList.Descriptions._001);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._002, ImportChargeCodeList.Descriptions._002);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._003, ImportChargeCodeList.Descriptions._003);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._004, ImportChargeCodeList.Descriptions._004);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._005, ImportChargeCodeList.Descriptions._005);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._006, ImportChargeCodeList.Descriptions._006);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._007, ImportChargeCodeList.Descriptions._007);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._008, ImportChargeCodeList.Descriptions._008);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._009, ImportChargeCodeList.Descriptions._009);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._010, ImportChargeCodeList.Descriptions._010);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._011, ImportChargeCodeList.Descriptions._011);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._012, ImportChargeCodeList.Descriptions._012);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._015, ImportChargeCodeList.Descriptions._015);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._016, ImportChargeCodeList.Descriptions._016);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._017, ImportChargeCodeList.Descriptions._017);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._019, ImportChargeCodeList.Descriptions._019);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes.AIR, ImportChargeCodeList.Descriptions.AIR);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes.TCE, ImportChargeCodeList.Descriptions.TCE);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._014, ImportChargeCodeList.Descriptions._014);
			customsChargeTypeList.Sort();
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);

			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			chargeTypeList1 = commonInvoice.ChargeTypeList;
			var exportChargeTypeList = new ChargeCodeList();
			exportChargeTypeList.Sort();
			AssertEquals(exportChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);
		}

		public void TestLookups()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			AssertType<JobComInvoiceHeaderLookups>(invoice.Lookups);
		}

		public void TestJobComInvoiceLines()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			AssertType<JobComInvoiceLineViewCollection>(invoice.JobComInvoiceLines);
		}

		public new void TestInvoiceLines()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			AssertType<JobComInvoiceLineViewCollection>(invoice.InvoiceLines);
		}

		public void TestSupportingDocuments()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			AssertType<SupportingDocumentCollection>(invoice.SupportingDocuments);
		}

		public void TestAddingSupportingDocumentAutomaticallyEnabled()
		{
			var invoice = GetNewInvoiceHeader();
			Assert("AddingSupportingDocumentAutomaticallyEnabled for Import", invoice.AddingSupportingDocumentAutomaticallyEnabled);

			invoice.JobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
			Assert("AddingSupportingDocumentAutomaticallyEnabled for Export", invoice.AddingSupportingDocumentAutomaticallyEnabled);
		}

		public void TestAddingSupportingDocumentAutomaticallyEnabled_InwardProcessingAVABR()
		{
			var invoice = GetNewInvoiceHeader();
			var instruction = invoice.JobDeclaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;

			AssertEquals("AddingSupportingDocumentAutomaticallyEnabled for CEI_Style = AVABR", false, invoice.AddingSupportingDocumentAutomaticallyEnabled);
		}

		public void TestGetStandaloneIncoTermAndChargeFactoryCountryContext()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var header = Factory.New<JobComInvoiceHeaderForTest>();
				header.JZ_JE = declaration.PK;
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertEquals("Import", "DEIMP", header.GetStandaloneIncoTermAndChargeFactoryCountryContext());

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				AssertEquals("Export", "DE", header.GetStandaloneIncoTermAndChargeFactoryCountryContext());
			});
		}

		public override void TestInvoiceLineChargeAffectsBalanceCalculation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 25000m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 10000m;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 20000m;
			declaration.ResumeApportionment();

			CombineAssertions(() =>
			{
				AssertEquals(0m, invoice.JZ_Calc_ChargesExcludedFromITOT);
				AssertNotEquals("Not balanced yet", 0m, invoice.JZ_Calc_Balance);

				var lineCharge = invoiceLine2.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 5000m, declaration.LocalCurrencyCode);
				lineCharge.J7_IsDutiable = false;
				lineCharge.J7_IsGSTApplicable = false;
				lineCharge.J7_Calc_IsIncludedInInvoiceAmount = true;
				declaration.ResumeApportionment();
				Assert("Pre-Req - lineCharge is includedInInvoiceAmount", lineCharge.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("line level Discount", -5000m, invoice.JZ_Calc_ChargesExcludedFromITOT);
				AssertEquals("Now balanced", 0m, invoice.JZ_Calc_Balance);

				var charge = invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 5000m, declaration.LocalCurrencyCode);
				declaration.ResumeApportionment();
				AssertEquals("Invoice Level Discount. Line level Discount is disregarded", -5000m, invoice.JZ_Calc_ChargesExcludedFromITOT);
				AssertEquals("Still balanced", 0m, invoice.JZ_Calc_Balance);
			});
		}

		public void TestDisablePropertiesForJE_MessageType_WAD()
		{
			CombineAssertions(() =>
			{
				var invoice = GetNewInvoiceHeader();
				foreach (var propertyInfo in new[]
				{
					invoice.JZ_InvoiceDateInfo,
					invoice.JZ_InvoiceAmountInfo,
					invoice.JZ_RX_NKInvoice_CurrencyInfo,
					invoice.JZ_FreeOfChargeInfo,
					invoice.JZ_IncoTermInfo,
					invoice.JZ_IncoTermPlaceInfo,
					invoice.JZ_ValuationCodeInfo,
					invoice.JZ_WeightInfo,
					invoice.JZ_WeightUQInfo,
					invoice.JZ_NetWeightInfo,
					invoice.JZ_NetWeightUQInfo,
					invoice.ZG_TransportChargesMethodOfPaymentInfo,
					invoice.JZ_NoOfPacksInfo
				})
				{
					AssertPropertyIsReadOnlyWAD(propertyInfo);
				}

				void AssertPropertyIsReadOnlyWAD(ZPropertyInfo propertyInfo)
				{
					invoice.JobDeclaration.JE_MessageType = DEJobMessageTypeList.Codes.Import;
					AssertEquals($"{propertyInfo.Name}, JE_MessageType IMP", false, propertyInfo.ReadOnly);

					invoice.JobDeclaration.JE_MessageType = DEJobMessageTypeList.Codes.WarehouseAdjustment;
					AssertEquals($"{propertyInfo.Name}, JE_MessageType WAD", true, propertyInfo.ReadOnly);
				}
			});
		}

		public void TestJZ_IncoTermPlace_Caption()
		{
			var invoice = GetNewInvoiceHeader();
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(invoice.JZ_IncoTermPlaceInfo);
			AssertEquals("Agreed Place", resourceStringData.Caption);
		}

		public void TestJZ_IncoTermPlace_Readonly()
		{
			var invoice = GetNewInvoiceHeader();
			var incoTermPlacePropertyInfo = invoice.JZ_IncoTermPlaceInfo;
			CombineAssertions(() =>
			{
				invoice.JobDeclaration.JE_MessageType = DEJobMessageTypeList.Codes.Export;
				invoice.AddInfo.ZG_AgreedPlaceCode = "DE";
				AssertEquals("Not readOnly", false, incoTermPlacePropertyInfo.ReadOnly);

				invoice.JobDeclaration.JE_MessageType = DEJobMessageTypeList.Codes.WarehouseAdjustment;
				AssertEquals("ReadOnly for WarehouseAdjustment", true, incoTermPlacePropertyInfo.ReadOnly);
			});
		}

		public void TestAddInfo()
		{
			var invoice = GetNewInvoiceHeader();
			AssertType<AddInfoJobComInvoiceHeader>(invoice.AddInfo);
		}

		public override void TestUpdateJZ_InvoiceNumber_DefaultDocument()
		{
			SupportingDocumentTestHelper.CreateRefCusCodeWithAttributeToEnsurePropertyEditable(
				Factory,
				UniversalReferenceConstants.RefCusCodeListAttributes.Name.Reference,
				EU.Business.UniversalReferenceConstants.SupportingDocumentTypes.N380,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection);
			base.TestUpdateJZ_InvoiceNumber_DefaultDocument();
		}

		public void TestSupportingDocuments_SequenceNumber()
		{
			var invoice = GetNewInvoiceHeader();
			CombineAssertions(() =>
			{
				var invoiceLine1 = invoice.InvoiceLines.AddNew();
				AssertEquals("InvoiceLine 1", (ZShort)1, invoiceLine1.JI_LineNo);
				var invoiceLine2 = invoice.InvoiceLines.AddNew();
				AssertEquals("InvoiceLine 2", (ZShort)2, invoiceLine2.JI_LineNo);

				var doc1 = invoice.SupportingDocuments.AddNew();
				AssertEquals("Doc 1", (ZInt)1, doc1.CSI_LineNo);
				var doc2 = invoice.SupportingDocuments.AddNew();
				AssertEquals("Doc 2", (ZInt)2, doc2.CSI_LineNo);
				var doc3 = invoice.SupportingDocuments.AddNew();
				AssertEquals("Doc 3", (ZInt)3, doc3.CSI_LineNo);
				invoice.SupportingDocuments.RemoveAndDelete(doc2);
				AssertEquals("Renumbered doc 3 to 2", (ZInt)2, doc3.CSI_LineNo);
			});
		}

		public void TestValuationRefCusCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "TranNature");

			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature,
				"01", "01 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Name.A1150, RefCusCodeListAttributes.Value.Yes);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature,
				"02", "02 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Name.C0091, RefCusCodeListAttributes.Value.Yes);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "03", "03 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature,
				"04", "04 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Name.A1150, RefCusCodeListAttributes.Value.Yes);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			CombineAssertions(() =>
			{
				AssertNull("JZ_ValuationCode is empty", invoice.ValuationRefCusCodeList);

				invoice.JZ_ValuationCode = "01";
				AssertEquals("JZ_ValuationCode = '01'", "01 DESC", invoice.ValuationRefCusCodeList.ZZD_Description);

				invoice.JZ_ValuationCode = "02";
				AssertEquals("JZ_ValuationCode = '02'", "02 DESC", invoice.ValuationRefCusCodeList.ZZD_Description);

				invoice.JZ_ValuationCode = "03";
				AssertNull("JZ_ValuationCode = '03', no attribute", invoice.ValuationRefCusCodeList);

				invoice.JZ_ValuationCode = "04";
				AssertNull("JZ_ValuationCode = '04', from EUN", invoice.ValuationRefCusCodeList);
			});
		}

		public void TestJZ_OA_ConsigneeAddress_GetDefaultAddress()
		{
			var invoice = GetNewInvoiceHeader();
			var orgHeader = Factory.New<OrgHeader>();
			CombineAssertions(() =>
			{
				invoice.JZ_OA_ConsigneeAddress_ZAddress.OrgPK = orgHeader.PK;
				AssertEquals("Not Export", ZGuid.Empty, invoice.JZ_OA_ConsigneeAddress);

				invoice.JZ_OA_ConsigneeAddress_ZAddress.OrgPK = ZGuid.Empty;
				invoice.JobDeclaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
				invoice.JZ_OA_ConsigneeAddress_ZAddress.OrgPK = orgHeader.PK;
				AssertEquals("Export", orgHeader.MainAddress.PK, invoice.JZ_OA_ConsigneeAddress);
			});
		}

		public override void TestCFRCalculationWithFreightAdjustedFlag()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ExportDate = new ZDateTime(2004, 10, 30);
			var invoice = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			invoice.JZ_InvoiceAmount = 10500m;
			invoice.JZ_RX_NKInvoice_Currency = dec.LocalCurrencyCode;

			var oFT = invoice.Charges.AddNew();
			oFT.J7_ChargeType = OverseasFreightCode;
			oFT.J7_Amount = 500m;
			oFT.J7_RX_NKCurrency = dec.LocalCurrencyCode;
			oFT.J7_IsDutiable = true;
			oFT.J7_IsIncludedInITOT = false;

			var adjustedOFT = invoice.Charges.AddNew();
			adjustedOFT.J7_ChargeType = OverseasFreightCode;
			adjustedOFT.J7_Amount = 300m;
			adjustedOFT.J7_RX_NKCurrency = dec.LocalCurrencyCode;
			adjustedOFT.J7_AdjustedCharge = true;
			adjustedOFT.J7_IsDutiable = true;
			adjustedOFT.J7_IsIncludedInITOT = false;

			AssertEquals("InvoiceLineTotal should only take charges without adjusted on", 10000m, invoice.InvoiceLineTotal);
			AssertEquals("FOB", 10500m, invoice.JZ_Calc_FOBAmount);
			AssertEquals("CIF", 10500m, invoice.JZ_Calc_CIFAmount);

			oFT.J7_IsIncludedInITOT = true;
			adjustedOFT.J7_IsIncludedInITOT = true;

			AssertEquals("InvoiceLineTotal should only take charges without adjusted on", 10500m, invoice.InvoiceLineTotal);
			AssertEquals("FOB", 10500m, invoice.JZ_Calc_FOBAmount);
			AssertEquals("CIF", 10500m, invoice.JZ_Calc_CIFAmount);
		}

		public void TestReadOnlyProperties_InwardProcessing_AVABR()
		{
			var invoice = GetNewInvoiceHeader();
			var instruction = invoice.JobDeclaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;

			CombineAssertions(() =>
			{
				AssertEquals("ReadOnly JZ_InvoiceDate", true, invoice.JZ_InvoiceDateInfo.ReadOnly);
				AssertEquals("ReadOnly IsJZ_InvoiceCurrExRateUserEnterable", true, invoice.IsJZ_InvoiceCurrExRateUserEnterableInfo.ReadOnly);
				AssertEquals("ReadOnly JZ_IncoTermPlace", true, invoice.JZ_IncoTermPlaceInfo.ReadOnly);
				AssertEquals("ReadOnly JZ_IncoTermDescription", true, invoice.JZ_IncoTermDescriptionInfo.ReadOnly);
				AssertEquals("ReadOnly ZG_AgreedPlaceCode", true, invoice.ZG_AgreedPlaceCodeInfo.ReadOnly);
				AssertEquals("ReadOnly JZ_ValuationCode", true, invoice.JZ_ValuationCodeInfo.ReadOnly);
				AssertEquals("ReadOnly JZ_Weight", true, invoice.JZ_WeightInfo.ReadOnly);
				AssertEquals("ReadOnly JZ_WeightUQ", true, invoice.JZ_WeightUQInfo.ReadOnly);
				AssertEquals("ReadOnly JZ_NetWeight", true, invoice.JZ_NetWeightInfo.ReadOnly);
				AssertEquals("ReadOnly JZ_NetWeightUQ", true, invoice.JZ_NetWeightUQInfo.ReadOnly);
				AssertEquals("ReadOnly JZ_InvoiceCurrLandedCostExRate", true, invoice.JZ_InvoiceCurrLandedCostExRateInfo.ReadOnly);
				AssertEquals("ReadOnly JZ_NoOfPacks", true, invoice.JZ_NoOfPacksInfo.ReadOnly);

				instruction.CEI_Style = ImportDeclarationTypeList.Codes.AZL;
				invoice.JZ_RX_NKInvoice_Currency = "xy";
				AssertEquals("Not readOnly IsJZ_InvoiceCurrExRateUserEnterable", false, invoice.IsJZ_InvoiceCurrExRateUserEnterableInfo.ReadOnly);
				AssertEquals("Not readOnly JZ_InvoiceDate", false, invoice.JZ_InvoiceDateInfo.ReadOnly);
				AssertEquals("Not readOnly JZ_IncoTermPlace", false, invoice.JZ_IncoTermPlaceInfo.ReadOnly);
				AssertEquals("Not readOnly JZ_IncoTermDescription", false, invoice.JZ_IncoTermDescriptionInfo.ReadOnly);
				AssertEquals("Not readOnly ZG_AgreedPlaceCode", false, invoice.ZG_AgreedPlaceCodeInfo.ReadOnly);
				AssertEquals("Not readOnly JZ_ValuationCode", false, invoice.JZ_ValuationCodeInfo.ReadOnly);
				AssertEquals("Not readOnly JZ_Weight", false, invoice.JZ_WeightInfo.ReadOnly);
				AssertEquals("Not readOnly JZ_WeightUQ", false, invoice.JZ_WeightUQInfo.ReadOnly);
				AssertEquals("Not readOnly JZ_NetWeight", false, invoice.JZ_NetWeightInfo.ReadOnly);
				AssertEquals("Not readOnly JZ_NetWeightUQ", false, invoice.JZ_NetWeightUQInfo.ReadOnly);
				AssertEquals("Not readOnly JZ_InvoiceCurrLandedCostExRate", false, invoice.JZ_InvoiceCurrLandedCostExRateInfo.ReadOnly);
				AssertEquals("Not readOnly JZ_NoOfPacks", false, invoice.JZ_NoOfPacksInfo.ReadOnly);
			});
		}

		public override string GetLocalCurrencyCode() => Core.Constants.CurrencyCodes.Germany;

		protected override string OverseasFreightCode => ChargeCodeList.Codes.OverseasFreight;

		protected override BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();

		protected override Type ExpectedTypeOfGroupCharges => typeof(JobComInvApportionedChargeCollection<InvoiceApportionCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(InvoiceChargeCollection<InvoiceCharge>);

		JobComInvoiceHeader GetNewInvoiceHeader()
		{
			var declaration = (JobDeclaration)GetNewDeclaration();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			return declaration.Invoices.AddNew();
		}
	}

	class JobComInvoiceHeaderForTest : JobComInvoiceHeader
	{
		public JobComInvoiceHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new string GetStandaloneIncoTermAndChargeFactoryCountryContext() => base.GetStandaloneIncoTermAndChargeFactoryCountryContext();
	}
}
