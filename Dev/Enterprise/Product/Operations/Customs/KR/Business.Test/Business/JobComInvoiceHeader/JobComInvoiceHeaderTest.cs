using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Business.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.KR.Messaging.Constants;
using CustomsChargeTypeList = Enterprise.Customs.Business.CustomsChargeTypeList;
using ICommonInvoice = Enterprise.Customs.Business.ICommonInvoice;

namespace Enterprise.Customs.KR.Business.Testing
{
	#region JobComInvoiceHeaderTest
	[TestedType(typeof(JobComInvoiceHeader))]
	sealed class JobComInvoiceHeaderTest : Customs.Business.Testing.BaseJobComInvoiceHeaderTest<JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine>
	{
		protected override (BaseJobDeclaration, BaseJobDeclaration) GetDeclarationsForAttach()
		{
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			return base.GetDeclarationsForAttach();
		}

		public void TestTypeDecider()
		{
			Assert("Update BaseJobComInvoiceHeaderTypeDecider to include a decider for this class", Factory.New(typeof(BaseJobComInvoiceHeader)).GetType() == GetExpectedBusinessObjectType());
		}

		public void TestCertificateOfOrigin()
		{
			var invoice = Factory.New<JobDeclaration>().Invoices.AddNew();
			invoice.CertificateOfOriginCollection.AddNew().CSI_ReferenceNumber = "Ref1";
			Factory.Save();

			var query = new ZDBOnlyQuery(typeof(CusSupportingInfo));
			query.AddToFilter(CusSupportingInfoSchema.CSI_ParentID, invoice.PK);
			query.AddToFilter(CusSupportingInfoSchema.CSI_Type, CusSupportingInfoTypeList.Codes.CertificateOfOrigin);
			var cusSupportingInfo = Factory.Load(typeof(CusSupportingInfo), query);
			AssertEquals(1, cusSupportingInfo.Length);
			AssertEquals("Ref1", ((CusSupportingInfo)cusSupportingInfo[0]).CSI_ReferenceNumber);
		}

		public void TestIncoTermAndChargeFactory()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			AssertType<ExportIncoTermAndCustomsChargeFactory>(invoice.IncoTermAndChargeFactory);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			AssertType<LocalExportIncoTermAndCustomsChargeFactory>(invoice.IncoTermAndChargeFactory);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			AssertType<ImportIncoTermAndCustomsChargeFactory>(invoice.IncoTermAndChargeFactory);
		}

		public override void TestChargeTypeList()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			ICommonInvoice commonInvoice = dec.Invoices.AddNew();
			var chargeTypeList1 = commonInvoice.ChargeTypeList;
			var chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));

			var customsChargeTypeList = new CodeDescriptionPairList();
			customsChargeTypeList.AddRange(new ImportChargeMethodOneCodeList());
			customsChargeTypeList.AddRange(new ImportChargeMethodTwoAndThreeCodeList());
			customsChargeTypeList.AddRange(new ImportChargeMethodFourCodeList());
			customsChargeTypeList.AddRange(new ImportChargeMethodFiveAndSixCodeList());
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);

			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			chargeTypeList1 = commonInvoice.ChargeTypeList;
			chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));

			customsChargeTypeList = new CustomsChargeTypeList();
			customsChargeTypeList.Sort();
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);
		}

		public void TestReadOnlyPropertyChange()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();

			AssertEquals(true, invoice.JZ_InvoiceCurrExRateInfo.ReadOnly);
		}

		public void TestDecimalPlacesChange()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();

			AssertHasCustomAttribute<DecimalPlacesAttribute>(invoice.GetType(), "JZ_InvoiceCurrLandedCostExRate", true, attrib => attrib.DecimalPlaces == 4);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(invoice.GetType(), "JZ_PaymentExRate", true, attrib => attrib.DecimalPlaces == 4);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(invoice.GetType(), "JZ_InvoiceCurrExRate", true, attrib => attrib.DecimalPlaces == 4);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(invoice.GetType(), "JZ_NoOfPacks", true, attrib => attrib.DecimalPlaces == 0);
		}

		public void TestMaxLengthChange()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();

			AssertEquals(2, invoice.JZ_PaymentTermsInfo.MaxLength);
			AssertEquals(1, invoice.JZ_ProvPricingYNInfo.MaxLength);
		}

		public void TestCustomsValues()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;

			RefCurrency usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			RefCurrency jpy = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Japan);

			SettingRate(usd, 0.8m, Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary);
			SettingRate(jpy, 0.5m, Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Japan;
			invHeader.JZ_InvoiceAmount = 161.9m;

			AssertEquals(161.9m, invHeader.JZ_Calc_FOBAmount);
			AssertEquals(80m, invHeader.CustomsValueKRW);
			AssertEquals(100m, invHeader.CustomsValueUSD);

			invHeader.JZ_InvoiceAmount = 159.3m;

			AssertEquals(159.3m, invHeader.JZ_Calc_FOBAmount);
			AssertEquals(79m, invHeader.CustomsValueKRW);
			AssertEquals(99m, invHeader.CustomsValueUSD);
		}

		void SettingRate(RefCurrency currency, ZDecimal rate, string rateType)
		{
			var result = Factory.New<RefExchangeRate>();
			result.RE_GC = GlbCompany.CurrentCompany.PK;
			result.RE_RX_NKExCurrency = currency.RX_Code;
			result.RE_StartDate = ZDateTime.Today;
			result.RE_ExpiryDate = ZDateTime.Today;
			result.RE_SellRate = rate;
			result.RE_ExRateType = rateType;

			Factory.Save();
		}
		public void TestDateOfValuation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(((ICurrencyConverterDataProvider)invoiceLine1.CusEntryLine.Header).DateOfValuation, invoice1.EffectiveValuationDate);

			var invoice2 = Factory.New<JobComInvoiceHeader>();
			AssertEquals(ZDateTime.Today, invoice2.EffectiveValuationDate);
		}
		public void TestKR_DRWApplicantTypeIsReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			AssertEquals(invoice.JZ_DRWApplicantTypeInfo.ReadOnly, false);

			declaration.JE_SimpleDRWApp = ApplicationForSimpleDrawbackCodeList.Codes.AD;
			AssertEquals(invoice.JZ_DRWApplicantTypeInfo.ReadOnly, true);

			declaration.JE_SimpleDRWApp = ApplicationForSimpleDrawbackCodeList.Codes.NO;
			AssertEquals(invoice.JZ_DRWApplicantTypeInfo.ReadOnly, false);
		}

		public void TestTotalInvoiceLinesNetWeight()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			AssertEquals(invoice.TotalInvoiceLinesNetWeightInKG, ZDecimal.Zero);

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_NetWeight = 10;
			invoiceLine1.JI_NetWeightUQ = "XX";
			AssertEquals(invoice.TotalInvoiceLinesNetWeightInKG, ZDecimal.Zero);

			invoiceLine1.JI_NetWeightUQ = "KG";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_NetWeight = 2000;
			invoiceLine2.JI_NetWeightUQ = "G";
			AssertEquals(invoice.TotalInvoiceLinesNetWeightInKG, 12m);
		}
		public void TestManufacturerIPCCodeAndDescription()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.IndustrialParkCode, "Industrial Park Code");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.IndustrialParkCode, "001", "Value", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			var org1 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "KRTES1", "CompanyName1");
			var cusCode = new IDNumberAndType[]
			{
					new IDNumberAndType() { Type = IdentificationType.IndustrialParkCode, Number = "001", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(org1.MainAddress, cusCode);

			var org2 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "KRTES2", "CompanyName2");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.B;
			var invoiceHeader = declaration.Invoices.AddNew();
			AssertEquals("999", invoiceHeader.ManufacturerIPCCode);

			invoiceHeader.JZ_OA_ManufacturerAddress = org1.MainAddress.PK;
			AssertEquals("001", invoiceHeader.ManufacturerIPCCode);

			invoiceHeader.JZ_OA_ManufacturerAddress = org2.MainAddress.PK;
			AssertEquals("999", invoiceHeader.ManufacturerIPCCode);

			declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.E;
			AssertEquals("", invoiceHeader.ManufacturerIPCCode);
		}

		public void TestManufacturerUnipassID()
		{
			var org1 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "KRTES1", "CompanyName1");
			var cusCode = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "12345678901234", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(org1.MainAddress, cusCode);

			var org2 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "KRTES2", "CompanyName2");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.B;
			var invoiceHeader = declaration.Invoices.AddNew();
			AssertEquals("제조미상9999000", invoiceHeader.ManufacturerUnipassID);

			invoiceHeader.JZ_OA_ManufacturerAddress = org1.MainAddress.PK;
			AssertEquals("12345678901234", invoiceHeader.ManufacturerUnipassID);

			invoiceHeader.JZ_OA_ManufacturerAddress = org2.MainAddress.PK;
			AssertEquals("제조미상9999000", invoiceHeader.ManufacturerUnipassID);

			declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.E;
			AssertEquals("", invoiceHeader.ManufacturerUnipassID);
		}

		public void TestSupplierUnipassID()
		{
			var org1 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "KRTES1", "CompanyName1");
			var cusCode = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "12345678901234", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(org1, cusCode);

			var org2 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "KRTES2", "CompanyName2");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			AssertEquals("", invoiceHeader.SupplierUnipassID);

			invoiceHeader.JZ_OH_Supplier = org1.PK;
			AssertEquals("12345678901234", invoiceHeader.SupplierUnipassID);

			invoiceHeader.JZ_OH_Supplier = org2.PK;
			AssertEquals("공급신청자미상9999", invoiceHeader.SupplierUnipassID);
		}

		public void TestBuyerID()
		{
			var org1 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "KRTES1", "CompanyName1");
			var cusCode = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.ForeignCompanyID, Number = "12345678901234", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(org1, cusCode);

			var org2 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "KRTES2", "CompanyName2");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.B;
			var invoiceHeader = declaration.Invoices.AddNew();
			AssertEquals("", invoiceHeader.BuyerID);

			invoiceHeader.JZ_OH_Buyer = org1.PK;
			AssertEquals("12345678901234", invoiceHeader.BuyerID);

			invoiceHeader.JZ_OH_Buyer = org2.PK;
			AssertEquals("ZZZZZZZZ9999A", invoiceHeader.BuyerID);

			declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.E;
			AssertEquals("", invoiceHeader.BuyerID);
		}

		public void TestJZ_OH_Buyer()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			invoiceHeader.JZ_OA_BuyerAddress = org.MainAddress.PK;
			AssertEquals(org.PK, invoiceHeader.JZ_OA_BuyerAddress_ZAddress.OrgPK);
			AssertEquals(org.PK, invoiceHeader.JZ_OH_Buyer);
		}

		public void TestJZ_OA_BuyerAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			invoiceHeader.JZ_OA_BuyerAddress = org.MainAddress.PK;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			invoiceHeader.JZ_OH_Buyer = org2.PK;
			AssertEquals(ZGuid.Empty, invoiceHeader.JZ_OA_BuyerAddress);
		}

		public void TestRefreshCurrencyWhenExRateTypeIsChanged()
		{
			RefCurrency currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);

			SettingRate(currency, 5678m, Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary);
			SettingRate(currency, 1234m, Core.Constants.ExchangeRateTypes.Code.CustomsRate);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = currency.RX_Code;

			AssertEquals(5678m, invoice.JZ_InvoiceCurrExRate);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			AssertEquals(1234m, invoice.JZ_InvoiceCurrExRate);
		}

		public override void TestIWeightApportioneeRoundingIssue()
		{
			var foreignCurrency = RefCurrency.New(Factory);
			foreignCurrency.RX_Code = "XYZ";
			foreignCurrency.SetCustomsRate(ZDateTime.Today, ZDateTime.Today.AddDays(1), RatesAreReciprocal ? 1.231678m : 0.8119m);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.JE_AutoWeightApportion = true;
			declaration.JE_TotalWeight = 10000m;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;
			invoice1.JZ_InvoiceAmount = 8000m;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;
			invoice2.JZ_InvoiceAmount = 2000m;

			AssertEquals("Weight", 8000m, invoice1.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);

			AssertEquals("Weight", 2000m, invoice2.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice2.JZ_WeightUQ);

			invoice1.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice2.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			AssertEquals("Weight", 8000m, invoice1.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);

			AssertEquals("Weight", 2000m, invoice2.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice2.JZ_WeightUQ);

			invoice1.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;
			SetIsJZ_InvoiceCurrExRateUserEnterable(invoice1, true);
			invoice1.JZ_InvoiceCurrExRate = RatesAreReciprocal ? 0.25m : 4m;

			invoice2.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;
			SetIsJZ_InvoiceCurrExRateUserEnterable(invoice2, true);
			invoice2.JZ_InvoiceCurrExRate = RatesAreReciprocal ? 4M : 0.25m;
			AssertEquals("Weight", 2000m, invoice1.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);

			AssertEquals("Weight", 8000m, invoice2.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice2.JZ_WeightUQ);

			SetIsJZ_InvoiceCurrExRateUserEnterable(invoice1, false);
			SetIsJZ_InvoiceCurrExRateUserEnterable(invoice2, false);
			declaration.ApportionInvoiceWeight(null);
			AssertEquals("Weight", 8000m, invoice1.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);

			AssertEquals("Weight", 2000m, invoice2.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice2.JZ_WeightUQ);

			invoice1.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice2.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;
			SetIsJZ_InvoiceCurrExRateUserEnterable(invoice2, true);
			invoice2.JZ_InvoiceCurrExRate = RatesAreReciprocal ? 4M : 0.25m;
			AssertEquals("Weight", 5000m, invoice1.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);

			AssertEquals("Weight", 5000m, invoice2.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice2.JZ_WeightUQ);
		}

		public void TestDefaultSupportingDocumentValueFromJobComInvoiceHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.LocalExport;

			var invoice = declaration.Invoices.AddNew();
			invoice.SupportingDocumentCode = RequirementDocumentTypeCodeList.Codes.E;
			invoice.SupportingDocumentReferenceNumber = "AASS123456789";

			var invoiceLine = invoice.InvoiceLines.AddNew();
			AssertEquals("E", invoice.SupportingDocumentCode);
			AssertEquals("AASS123456789", invoice.SupportingDocumentReferenceNumber);
			AssertEquals("E", invoiceLine.SupportingDocumentCode);
			AssertEquals("AASS123456789", invoiceLine.SupportingDocumentReferenceNumber);

			invoice.SupportingDocumentCode = RequirementDocumentTypeCodeList.Codes.B;
			invoice.SupportingDocumentReferenceNumber = "TEST999900000";
			AssertEquals("B", invoice.SupportingDocumentCode);
			AssertEquals("TEST999900000", invoice.SupportingDocumentReferenceNumber);
			AssertEquals("E", invoiceLine.SupportingDocumentCode);
			AssertEquals("AASS123456789", invoiceLine.SupportingDocumentReferenceNumber);

			invoiceLine.SupportingDocumentCode = RequirementDocumentTypeCodeList.Codes.C;
			invoiceLine.SupportingDocumentReferenceNumber = "ZZZZ000011111";
			AssertEquals("B", invoice.SupportingDocumentCode);
			AssertEquals("TEST999900000", invoice.SupportingDocumentReferenceNumber);
			AssertEquals("C", invoiceLine.SupportingDocumentCode);
			AssertEquals("ZZZZ000011111", invoiceLine.SupportingDocumentReferenceNumber);

			invoiceLine.SupportingDocumentCode = "";
			invoiceLine.SupportingDocumentReferenceNumber = "";
			AssertEquals("B", invoice.SupportingDocumentCode);
			AssertEquals("TEST999900000", invoice.SupportingDocumentReferenceNumber);
			AssertEquals("", invoiceLine.SupportingDocumentCode);
			AssertEquals("", invoiceLine.SupportingDocumentReferenceNumber);

			invoice.SupportingDocumentCode = RequirementDocumentTypeCodeList.Codes.E;
			invoice.SupportingDocumentReferenceNumber = "AASS123456789";
			AssertEquals("E", invoice.SupportingDocumentCode);
			AssertEquals("AASS123456789", invoice.SupportingDocumentReferenceNumber);
			AssertEquals("E", invoiceLine.SupportingDocumentCode);
			AssertEquals("AASS123456789", invoiceLine.SupportingDocumentReferenceNumber);
		}

		public void TestCertificateOfOriginData()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceHeaderCertificate = invoice.CertificateOfOriginCollection.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoice.CertificateOfOriginIssueStatus = CertificateOfOriginIssuedCodeList.Codes.N;
			AssertEquals(invoiceHeaderCertificate.CSI_Code, invoiceLine1.CertificateOfOriginData.CSI_Code);
			invoice.CriteriaForDeterminingCountryOfOrigin = CountryOfOriginDeterminationRuleCodeList.Codes._8;
			AssertEquals(invoiceHeaderCertificate.CSI_SubType, invoiceLine1.CertificateOfOriginData.CSI_SubType);

			invoice.CertificateOfOriginIssueStatus = CertificateOfOriginIssuedCodeList.Codes.Y;
			invoice.CriteriaForDeterminingCountryOfOrigin = CountryOfOriginDeterminationRuleCodeList.Codes._2;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			AssertEquals(CertificateOfOriginIssuedCodeList.Codes.N, invoiceLine1.CertificateOfOriginData.CSI_Code);
			AssertEquals(CountryOfOriginDeterminationRuleCodeList.Codes._8, invoiceLine1.CertificateOfOriginData.CSI_SubType);
			AssertEquals(invoiceHeaderCertificate.CSI_Code, invoiceLine2.CertificateOfOriginData.CSI_Code);
			AssertEquals(invoiceHeaderCertificate.CSI_SubType, invoiceLine2.CertificateOfOriginData.CSI_SubType);
		}

		public void TestICurrencyConverterDataProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			AssertEquals("ICurrencyConverterDataProvider.MaximumDaysToFallback", 0, ((ICurrencyConverterDataProvider)invoice).MaximumDaysToFallback);
		}

		public void TestJZ_CU_RelatedBill()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill = declaration.Bills.AddNew();
			var cargoManagementNum1 = bill.CargoManagementNumbers.AddNew();
			cargoManagementNum1.CY_Data = "TEST1";

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_CU_RelatedHouseBill = bill.PK;

			AssertEquals("TEST1", invoice1.JZ_ImportCargoManagementNumber);

			var cargoManagementNum2 = bill.CargoManagementNumbers.AddNew();
			cargoManagementNum2.CY_Data = "TEST2";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_CU_RelatedHouseBill = bill.PK;

			AssertEquals(ZString.Empty, invoice2.JZ_ImportCargoManagementNumber);
		}

		public void TestIsFreeTrade()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			AssertEquals(false, invoice.IsFreeTrade);

			invoice.JZ_PaymentTerms = InvoicePaymentTermCodeList.Codes.GN;
			AssertEquals(true, invoice.IsFreeTrade);

			invoice.JZ_PaymentTerms = InvoicePaymentTermCodeList.Codes.GO;
			AssertEquals(false, invoice.IsFreeTrade);

			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._71;
			AssertEquals(true, invoice.IsFreeTrade);

			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._80;
			AssertEquals(true, invoice.IsFreeTrade);

			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._81;
			AssertEquals(true, invoice.IsFreeTrade);

			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._82;
			AssertEquals(true, invoice.IsFreeTrade);

			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._83;
			AssertEquals(true, invoice.IsFreeTrade);

			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._84;
			AssertEquals(true, invoice.IsFreeTrade);

			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._85;
			AssertEquals(true, invoice.IsFreeTrade);

			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._86;
			AssertEquals(true, invoice.IsFreeTrade);

			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._87;
			AssertEquals(true, invoice.IsFreeTrade);

			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._88;
			AssertEquals(true, invoice.IsFreeTrade);

			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._89;
			AssertEquals(true, invoice.IsFreeTrade);

			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._90;
			AssertEquals(true, invoice.IsFreeTrade);

			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._91;
			AssertEquals(true, invoice.IsFreeTrade);

			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._92;
			AssertEquals(true, invoice.IsFreeTrade);

			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._93;
			AssertEquals(true, invoice.IsFreeTrade);

			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._94;
			AssertEquals(true, invoice.IsFreeTrade);

			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._95;
			AssertEquals(true, invoice.IsFreeTrade);

			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._96;
			AssertEquals(true, invoice.IsFreeTrade);

			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._97;
			AssertEquals(true, invoice.IsFreeTrade);

			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._100;
			AssertEquals(true, invoice.IsFreeTrade);

			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._101;
			AssertEquals(false, invoice.IsFreeTrade);
		}

		public void TestImportSupplierID_DefaultValue()
		{
			var supplier1 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK1", "Supplier");
			var supplier2 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK2", "Test Supplier");
			supplier2.OH_RL_NKClosestPort = "KESEL";
			var regNo = new IDNumberAndType[]
			{
				new IDNumberAndType { Type = IdentificationType.ForeignCompanyID, Number = "ZZZ1101022931" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(supplier2, regNo);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();

			invoice.JZ_OH_Supplier = supplier1.PK;
			AssertEquals(ZString.Empty, invoice.ImportSupplierID);

			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._91;
			AssertEquals("ZZZZZZZZ9999A", invoice.ImportSupplierID);

			declaration.JE_TradeType = ZString.Empty;
			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._13;
			AssertEquals("ZZZZZZZZ9999A", invoice.ImportSupplierID);

			declaration.JE_ProcedureType = ZString.Empty;
			declaration.JE_MessageSubType = ImportDeclarationTypeCodeList.Codes.C;
			AssertEquals("ZZZZZZZZ9999A", invoice.ImportSupplierID);

			invoice.JZ_OH_Supplier = supplier2.PK;
			AssertEquals("ZZZ1101022931", invoice.ImportSupplierID);
		}

		public void TestImportCopyinvoiceCertificationOfOriginDataToInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoice.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.KoreaSouth;
			invoice.JZ_COOLabelLocation = CountryOfOriginLabelLocationCodeList.Codes.B;
			invoice.JZ_COOLabelType = CountryOfOriginLabelTypeCodeList.Codes.A;
			invoice.JZ_COOExemptionReason = CountryOfOriginExemptionReasonCodeList.Codes._12;
			invoice.CriteriaForDeterminingCountryOfOrigin = CountryOfOriginDeterminationRuleCodeList.Codes._2;
			invoice.CertificateOfOriginIssuingCountry = Core.Constants.CountryCodes.UnitedStates;
			invoice.CertificateOfOriginIssueDate = new ZDateTime(2024, 05, 24);
			invoice.CertificateOfOriginNo = "800324356053";
			invoice.CertificateOfOriginCriteriaCode = CountryOfOriginDeterminationRuleCodeList.Codes.A;
			invoice.CertificateOfOriginAgencyName = "발행기관명";
			invoice.CertificateOfOriginAreaName = "발급지역명";
			invoice.CertificateOfOriginPersonName = "발급담당자명";
			invoice.CertificateOfOriginStatus = Constants.YesNo.Yes;

			AssertEquals(invoiceLine.JI_CountryOfOrigin, invoice.JZ_RN_NKDefaultOrigin);
			AssertEquals(invoiceLine.JI_COOLabelLocation, invoice.JZ_COOLabelLocation);
			AssertEquals(invoiceLine.JI_COOLabelType, invoice.JZ_COOLabelType);
			AssertEquals(invoiceLine.JI_COOExemptionReason, invoice.JZ_COOExemptionReason);
			AssertEquals(invoiceLine.CriteriaForDeterminingCountryOfOrigin, invoice.CriteriaForDeterminingCountryOfOrigin);
			AssertEquals(invoiceLine.CertificateOfOriginIssuingCountry, invoice.CertificateOfOriginIssuingCountry);
			AssertEquals(invoiceLine.CertificateOfOriginIssueDate, invoice.CertificateOfOriginIssueDate);
			AssertEquals(invoiceLine.CertificateOfOriginNo, invoice.CertificateOfOriginNo);
			AssertEquals(invoiceLine.CertificateOfOriginCriteriaCode, invoice.CertificateOfOriginCriteriaCode);
			AssertEquals(invoiceLine.CertificateOfOriginAgencyName, invoice.CertificateOfOriginAgencyName);
			AssertEquals(invoiceLine.CertificateOfOriginAreaName, invoice.CertificateOfOriginAreaName);
			AssertEquals(invoiceLine.CertificateOfOriginPersonName, invoice.CertificateOfOriginPersonName);
			AssertEquals(invoiceLine.CertificateOfOriginStatus, invoice.CertificateOfOriginStatus);
		}

		public void TestPopulateQuestionsAndDeclarationCodes_Is5SM()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			var entry = new TestDataSetupHelper(Factory).GetEntryWithFullGOVCBR934Data();
			var declaration = entry.Declaration;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.ValuationDeclaration;
			var invoice = declaration.Invoices[0];

			invoice.ValuationQuestions.RemoveAndDeleteAll();
			invoice.ValuationDeclarationCodes.RemoveAndDeleteAll();

			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodTwo;
			AssertEquals(0, invoice.ValuationQuestions.Count);
			AssertEquals(12, invoice.ValuationDeclarationCodes.Count);

			AssertEquals(PriceDeclarationItemCodeList.Codes._301, invoice.ValuationDeclarationCodes[0].CY_Code);
			AssertEquals(PriceDeclarationItemCodeList.Codes._302, invoice.ValuationDeclarationCodes[1].CY_Code);
			AssertEquals(PriceDeclarationItemCodeList.Codes._303, invoice.ValuationDeclarationCodes[2].CY_Code);
			AssertEquals(PriceDeclarationItemCodeList.Codes._304, invoice.ValuationDeclarationCodes[3].CY_Code);
			AssertEquals(PriceDeclarationItemCodeList.Codes._305, invoice.ValuationDeclarationCodes[4].CY_Code);
			AssertEquals(PriceDeclarationItemCodeList.Codes._306, invoice.ValuationDeclarationCodes[5].CY_Code);
			AssertEquals(PriceDeclarationItemCodeList.Codes._307, invoice.ValuationDeclarationCodes[6].CY_Code);
			AssertEquals(PriceDeclarationItemCodeList.Codes._401, invoice.ValuationDeclarationCodes[7].CY_Code);
			AssertEquals(PriceDeclarationItemCodeList.Codes._402, invoice.ValuationDeclarationCodes[8].CY_Code);
			AssertEquals(PriceDeclarationItemCodeList.Codes._403, invoice.ValuationDeclarationCodes[9].CY_Code);
			AssertEquals(PriceDeclarationItemCodeList.Codes._404, invoice.ValuationDeclarationCodes[10].CY_Code);
			AssertEquals(PriceDeclarationItemCodeList.Codes._405, invoice.ValuationDeclarationCodes[11].CY_Code);

			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodThree;
			AssertEquals(0, invoice.ValuationQuestions.Count);
			AssertEquals(12, invoice.ValuationDeclarationCodes.Count);

			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodFourA;
			AssertEquals(0, invoice.ValuationQuestions.Count);
			AssertEquals(12, invoice.ValuationDeclarationCodes.Count);

			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodFourB;
			AssertEquals(0, invoice.ValuationQuestions.Count);
			AssertEquals(12, invoice.ValuationDeclarationCodes.Count);

			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodFive;
			AssertEquals(0, invoice.ValuationQuestions.Count);
			AssertEquals(12, invoice.ValuationDeclarationCodes.Count);

			invoice.ProvisionalPricingReason101 = true;
			invoice.ProvisionalPricingReason119 = "TEST";
			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;

			AssertEquals(19, invoice.ValuationQuestions.Count);
			AssertEquals(0, invoice.ValuationDeclarationCodes.Count);
			AssertEquals(PriceQuestionCodeList.Codes._5A, invoice.ValuationQuestions[0].CY_Code);
			AssertEquals(PriceQuestionCodeList.Codes._6A, invoice.ValuationQuestions[1].CY_Code);
			AssertEquals(PriceQuestionCodeList.Codes._6B, invoice.ValuationQuestions[2].CY_Code);
			AssertEquals(PriceQuestionCodeList.Codes._7A, invoice.ValuationQuestions[3].CY_Code);
			AssertEquals(PriceQuestionCodeList.Codes._7B, invoice.ValuationQuestions[4].CY_Code);
			AssertEquals(PriceQuestionCodeList.Codes._8A, invoice.ValuationQuestions[5].CY_Code);
			AssertEquals(PriceQuestionCodeList.Codes._8B, invoice.ValuationQuestions[6].CY_Code);
			AssertEquals(PriceQuestionCodeList.Codes._8C, invoice.ValuationQuestions[7].CY_Code);
			AssertEquals(PriceQuestionCodeList.Codes._8D, invoice.ValuationQuestions[8].CY_Code);
			AssertEquals(PriceQuestionCodeList.Codes._9A, invoice.ValuationQuestions[9].CY_Code);
			AssertEquals(PriceQuestionCodeList.Codes._9B, invoice.ValuationQuestions[10].CY_Code);
			AssertEquals(PriceQuestionCodeList.Codes._10A, invoice.ValuationQuestions[11].CY_Code);
			AssertEquals(PriceQuestionCodeList.Codes._10B, invoice.ValuationQuestions[12].CY_Code);
			AssertEquals(PriceQuestionCodeList.Codes._10C, invoice.ValuationQuestions[13].CY_Code);
			AssertEquals(PriceQuestionCodeList.Codes._10D, invoice.ValuationQuestions[14].CY_Code);
			AssertEquals(PriceQuestionCodeList.Codes._11A, invoice.ValuationQuestions[15].CY_Code);
			AssertEquals(PriceQuestionCodeList.Codes._11B, invoice.ValuationQuestions[16].CY_Code);
			AssertEquals(PriceQuestionCodeList.Codes._11C, invoice.ValuationQuestions[17].CY_Code);
			AssertEquals(PriceQuestionCodeList.Codes._11D, invoice.ValuationQuestions[18].CY_Code);
		}

		public void TestPopulateQuestionsAndDeclarationCodes_IsImport()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			var entry = new TestDataSetupHelper(Factory).GetEntryWithFullGOVCBR934Data();
			var declaration = entry.Declaration;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices[0];

			invoice.ValuationQuestions.RemoveAndDeleteAll();
			invoice.ValuationDeclarationCodes.RemoveAndDeleteAll();

			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodTwo;
			AssertEquals(0, invoice.ValuationQuestions.Count);

			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodThree;
			AssertEquals(0, invoice.ValuationQuestions.Count);

			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodFourA;
			AssertEquals(0, invoice.ValuationQuestions.Count);

			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodFourB;
			AssertEquals(0, invoice.ValuationQuestions.Count);

			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodFive;
			AssertEquals(0, invoice.ValuationQuestions.Count);

			invoice.ProvisionalPricingReason101 = true;
			invoice.ProvisionalPricingReason119 = "TEST";
			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;

			AssertEquals(5, invoice.ValuationQuestions.Count);
			AssertEquals(PriceQuestionCodeList.Codes._7A, invoice.ValuationQuestions[0].CY_Code);
			AssertEquals(PriceQuestionCodeList.Codes._8A, invoice.ValuationQuestions[1].CY_Code);
			AssertEquals(PriceQuestionCodeList.Codes._8B, invoice.ValuationQuestions[2].CY_Code);
			AssertEquals(PriceQuestionCodeList.Codes._9A, invoice.ValuationQuestions[3].CY_Code);
			AssertEquals(PriceQuestionCodeList.Codes._9B, invoice.ValuationQuestions[4].CY_Code);
			AssertEquals(PriceDeclarationItemCodeList.Codes._101, invoice.ValuationDeclarationCodes[0].CY_Code);
			AssertEquals(PriceDeclarationItemCodeList.Codes._119, invoice.ValuationDeclarationCodes[1].CY_Code);
		}

		public void TestValuationQuestionsDefaultValuesFor5SM()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.ValuationDeclaration;

			var invoice = declaration.Invoices.AddNew();
			invoice.ValuationQuestion5A = YesNoList.Codes.Yes;
			AssertEquals(ZString.Empty, invoice.ValuationQuestion5B);
			AssertEquals(YesNoList.Codes.No, invoice.ValuationQuestion5C);
			AssertEquals(YesNoList.Codes.No, invoice.ValuationQuestion5D);
			AssertEquals(ZString.Empty, invoice.ValuationQuestion5EA);
			AssertEquals(ZString.Empty, invoice.ValuationQuestion5EB);
			Assert(!invoice.ValuationQuestion5BInfo.ReadOnly);
			Assert(!invoice.ValuationQuestion5CInfo.ReadOnly);
			Assert(!invoice.ValuationQuestion5DInfo.ReadOnly);
			Assert(!invoice.ValuationQuestion5EAInfo.ReadOnly);

			invoice.ValuationQuestion5B = SpecialRelationshipCodeList.Codes._01;
			invoice.ValuationQuestion5EA = PricingCodeList.Codes._99;
			AssertEquals(SpecialRelationshipCodeList.Codes._01, invoice.ValuationQuestion5B);
			AssertEquals(PricingCodeList.Codes._99, invoice.ValuationQuestion5EA);
			Assert(!invoice.ValuationQuestion5EBInfo.ReadOnly);

			invoice.ValuationQuestion5EB = "기타";
			AssertEquals("기타", invoice.ValuationQuestion5EB);

			invoice.ValuationQuestion5EA = PricingCodeList.Codes._05;
			AssertEquals(ZString.Empty, invoice.ValuationQuestion5EB);
			Assert(invoice.ValuationQuestion5EBInfo.ReadOnly);

			invoice.ValuationQuestion5EA = PricingCodeList.Codes._99;
			invoice.ValuationQuestion5A = YesNoList.Codes.No;
			AssertEquals(ZString.Empty, invoice.ValuationQuestion5B);
			AssertEquals(ZString.Empty, invoice.ValuationQuestion5C);
			AssertEquals(ZString.Empty, invoice.ValuationQuestion5D);
			AssertEquals(ZString.Empty, invoice.ValuationQuestion5EA);
			AssertEquals(ZString.Empty, invoice.ValuationQuestion5EB);
			Assert(invoice.ValuationQuestion5BInfo.ReadOnly);
			Assert(invoice.ValuationQuestion5CInfo.ReadOnly);
			Assert(invoice.ValuationQuestion5DInfo.ReadOnly);
			Assert(invoice.ValuationQuestion5EAInfo.ReadOnly);
			Assert(invoice.ValuationQuestion5EBInfo.ReadOnly);
		}

		public void TestValuationQuestionsDefaultValuesFor934()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.ValuationQuestion7A_IMP = YesNoList.Codes.Yes;
			AssertEquals(ZString.Empty, invoice.ValuationQuestion7B_IMP);
			AssertEquals(YesNoList.Codes.No, invoice.ValuationQuestion7C);
			AssertEquals(YesNoList.Codes.No, invoice.ValuationQuestion7D);
			AssertEquals(ZString.Empty, invoice.ValuationQuestion7EA);
			AssertEquals(ZString.Empty, invoice.ValuationQuestion7EB);
			Assert(!invoice.ValuationQuestion7B_IMPInfo.ReadOnly);
			Assert(!invoice.ValuationQuestion7CInfo.ReadOnly);
			Assert(!invoice.ValuationQuestion7DInfo.ReadOnly);
			Assert(!invoice.ValuationQuestion7EAInfo.ReadOnly);

			invoice.ValuationQuestion7B_IMP = SpecialRelationshipCodeList.Codes._01;
			invoice.ValuationQuestion7EA = PricingCodeList.Codes._99;
			AssertEquals(SpecialRelationshipCodeList.Codes._01, invoice.ValuationQuestion7B_IMP);
			AssertEquals(PricingCodeList.Codes._99, invoice.ValuationQuestion7EA);
			Assert(!invoice.ValuationQuestion7EBInfo.ReadOnly);

			invoice.ValuationQuestion7EB = "기타";
			AssertEquals("기타", invoice.ValuationQuestion7EB);

			invoice.ValuationQuestion7EA = PricingCodeList.Codes._05;
			AssertEquals(ZString.Empty, invoice.ValuationQuestion7EB);
			Assert(invoice.ValuationQuestion7EBInfo.ReadOnly);

			invoice.ValuationQuestion7EA = PricingCodeList.Codes._99;
			invoice.ValuationQuestion7A_IMP = YesNoList.Codes.No;
			AssertEquals(ZString.Empty, invoice.ValuationQuestion7B_IMP);
			AssertEquals(ZString.Empty, invoice.ValuationQuestion7C);
			AssertEquals(ZString.Empty, invoice.ValuationQuestion7D);
			AssertEquals(ZString.Empty, invoice.ValuationQuestion7EA);
			AssertEquals(ZString.Empty, invoice.ValuationQuestion7EB);
			Assert(invoice.ValuationQuestion7B_IMPInfo.ReadOnly);
			Assert(invoice.ValuationQuestion7CInfo.ReadOnly);
			Assert(invoice.ValuationQuestion7DInfo.ReadOnly);
			Assert(invoice.ValuationQuestion7EAInfo.ReadOnly);
			Assert(invoice.ValuationQuestion7EBInfo.ReadOnly);
		}

		public void TestQuestionData()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();

			CreateQuestion(invoice, PriceQuestionCodeList.Codes._5A, YesNo.Yes);
			CreateQuestion(invoice, PriceQuestionCodeList.Codes._5B, "01");
			CreateQuestion(invoice, PriceQuestionCodeList.Codes._5C, YesNo.Yes);
			CreateQuestion(invoice, PriceQuestionCodeList.Codes._5D, YesNo.Yes);
			CreateQuestion(invoice, PriceQuestionCodeList.Codes._5EA, "99");
			CreateQuestion(invoice, PriceQuestionCodeList.Codes._5EB, "기타법률");
			CreateQuestion(invoice, PriceQuestionCodeList.Codes._6A, YesNo.Yes);
			CreateQuestion(invoice, PriceQuestionCodeList.Codes._6B, YesNo.Yes);
			CreateQuestion(invoice, PriceQuestionCodeList.Codes._7A, YesNo.Yes);
			CreateQuestion(invoice, PriceQuestionCodeList.Codes._7B, SpecialRelationshipCodeList.Codes._01);
			CreateQuestion(invoice, PriceQuestionCodeList.Codes._7C, YesNo.No);
			CreateQuestion(invoice, PriceQuestionCodeList.Codes._7D, YesNo.Yes);
			CreateQuestion(invoice, PriceQuestionCodeList.Codes._7EA, PricingCodeList.Codes._02);
			CreateQuestion(invoice, PriceQuestionCodeList.Codes._7EB, "질문사항");
			CreateQuestion(invoice, PriceQuestionCodeList.Codes._8A, YesNo.No);
			CreateQuestion(invoice, PriceQuestionCodeList.Codes._8B, YesNo.Yes);
			CreateQuestion(invoice, PriceQuestionCodeList.Codes._9A, YesNo.No);
			CreateQuestion(invoice, PriceQuestionCodeList.Codes._9B, YesNo.Yes);

			AssertEquals(YesNo.Yes, invoice.ValuationQuestion5A);
			AssertEquals("01", invoice.ValuationQuestion5B);
			AssertEquals(YesNo.Yes, invoice.ValuationQuestion5C);
			AssertEquals(YesNo.Yes, invoice.ValuationQuestion5D);
			AssertEquals("99", invoice.ValuationQuestion5EA);
			AssertEquals("기타법률", invoice.ValuationQuestion5EB);
			AssertEquals(YesNo.Yes, invoice.ValuationQuestion6A);
			AssertEquals(YesNo.Yes, invoice.ValuationQuestion6B);
			AssertEquals(YesNo.Yes, invoice.ValuationQuestion7A_5SM);
			AssertEquals(SpecialRelationshipCodeList.Codes._01, invoice.ValuationQuestion7B_IMP);
			AssertEquals(YesNo.No, invoice.ValuationQuestion7C);
			AssertEquals(YesNo.Yes, invoice.ValuationQuestion7D);
			AssertEquals(PricingCodeList.Codes._02, invoice.ValuationQuestion7EA);
			AssertEquals("질문사항", invoice.ValuationQuestion7EB);
			AssertEquals(YesNo.No, invoice.ValuationQuestion8A);
			AssertEquals(YesNo.Yes, invoice.ValuationQuestion8B);
			AssertEquals(YesNo.No, invoice.ValuationQuestion9A);
			AssertEquals(YesNo.Yes, invoice.ValuationQuestion9B);
		}

		public void TestQuestionDefaultData()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.ValuationDeclaration;
			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;

			AssertEquals(YesNo.No, invoice.ValuationQuestion8A);
			AssertEquals(YesNo.No, invoice.ValuationQuestion8B);
			AssertEquals(YesNo.No, invoice.ValuationQuestion8C);
			AssertEquals(YesNo.No, invoice.ValuationQuestion8D);
			AssertEquals(YesNo.No, invoice.ValuationQuestion9A);
			AssertEquals(YesNo.No, invoice.ValuationQuestion9B);
			AssertEquals(YesNo.No, invoice.ValuationQuestion10A);
			AssertEquals(YesNo.No, invoice.ValuationQuestion10B);
			AssertEquals(YesNo.No, invoice.ValuationQuestion10C);
			AssertEquals(YesNo.No, invoice.ValuationQuestion10D);
			AssertEquals(YesNo.No, invoice.ValuationQuestion11A);
			AssertEquals(YesNo.No, invoice.ValuationQuestion11B);
			AssertEquals(YesNo.No, invoice.ValuationQuestion11C);
			AssertEquals(YesNo.No, invoice.ValuationQuestion11D);

			invoice.ValuationQuestion8A = YesNo.Yes;
			AssertEquals(YesNo.Yes, invoice.ValuationQuestion8A);
		}

		void CreateQuestion(JobComInvoiceHeader invoice, ZString code, ZString data)
		{
			var valuationQuestion = invoice.ValuationQuestions.AddNew();
			valuationQuestion.CY_Code = code;
			valuationQuestion.CY_Data = data;
		}

		public void TestProxyFieldsOfValuationMethodA_ConvertCurrency()
		{
			var usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			SettingRate(usd, 999.67m, Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			var aud = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Australia);
			SettingRate(aud, 999.67m, Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			var jpy = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Japan);
			SettingRate(jpy, 999.67m, Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			var gbp = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedKingdom);
			SettingRate(gbp, 999.67m, Core.Constants.ExchangeRateTypes.Code.CustomsRate);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;
			invoice1.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A102, 111.11m, Core.Constants.CurrencyCodes.UnitedStates);
			invoice1.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A102, 111.11m, Core.Constants.CurrencyCodes.Japan);
			invoice1.GroupCharges.AddNew(ImportChargeMethodOneCodeList.Codes.A102, 111.11m, Core.Constants.CurrencyCodes.Australia);
			invoice1.GroupCharges.AddNew(ImportChargeMethodOneCodeList.Codes.A102, 111.11m, Core.Constants.CurrencyCodes.UnitedKingdom);

			AssertEquals("((111.11 * 999.67) ≈ 111,073) * 4 = 444,292", 444292m, invoice1.IndirectAmount);

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;
			invoice2.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A102, 111.11m, Core.Constants.CurrencyCodes.UnitedStates);
			invoice2.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A102, 111.11m, Core.Constants.CurrencyCodes.UnitedStates);
			invoice2.GroupCharges.AddNew(ImportChargeMethodOneCodeList.Codes.A102, 111.11m, Core.Constants.CurrencyCodes.UnitedStates);
			invoice2.GroupCharges.AddNew(ImportChargeMethodOneCodeList.Codes.A102, 111.11m, Core.Constants.CurrencyCodes.UnitedStates);

			AssertEquals("(111.11 * 4) * 999.67 ≈ 444,293", 444293m, invoice2.IndirectAmount);

			var invoice3 = declaration.Invoices.AddNew();
			invoice3.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;
			invoice3.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A102, 111073.33m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			invoice3.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A102, 111073.33m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			invoice3.GroupCharges.AddNew(ImportChargeMethodOneCodeList.Codes.A102, 111073.33m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			invoice3.GroupCharges.AddNew(ImportChargeMethodOneCodeList.Codes.A102, 111073.33m, Core.Constants.CurrencyCodes.KoreaRepublicOf);

			AssertEquals("111,073.33 * 4 ≈ 444,293", 444293m, invoice3.IndirectAmount);
		}

		public void TestProxyFieldsOfValuationMethodA()
		{
			var usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			SettingRate(usd, 999.67m, Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			var aud = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Australia);
			SettingRate(aud, 891.34m, Core.Constants.ExchangeRateTypes.Code.CustomsRate);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = "CRF";
			invoice.JZ_InvoiceAmount = 100000m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;

			invoice.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A102, 111.11m, Core.Constants.CurrencyCodes.UnitedStates);
			invoice.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A104, 222.22m, Core.Constants.CurrencyCodes.UnitedStates);
			invoice.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A105, 333.33m, Core.Constants.CurrencyCodes.UnitedStates);
			invoice.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A106, 444.44m, Core.Constants.CurrencyCodes.UnitedStates);
			invoice.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A107, 555.55m, Core.Constants.CurrencyCodes.UnitedStates);
			invoice.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A108, 666.66m, Core.Constants.CurrencyCodes.UnitedStates);
			invoice.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A109, 777.77m, Core.Constants.CurrencyCodes.UnitedStates);
			invoice.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A110, 888.88m, Core.Constants.CurrencyCodes.UnitedStates);
			invoice.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A111, 999.99m, Core.Constants.CurrencyCodes.UnitedStates);
			invoice.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A112, 1010.10m, Core.Constants.CurrencyCodes.UnitedStates);
			invoice.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A114, 1111.11m, Core.Constants.CurrencyCodes.UnitedStates);
			invoice.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A115, 1212.12m, Core.Constants.CurrencyCodes.UnitedStates);
			invoice.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A116, 1313.13m, Core.Constants.CurrencyCodes.UnitedStates);
			invoice.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A118, 1414.14m, Core.Constants.CurrencyCodes.UnitedStates);
			invoice.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A119, 1515.15m, Core.Constants.CurrencyCodes.UnitedStates);
			invoice.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A120, 1616.16m, Core.Constants.CurrencyCodes.UnitedStates);
			invoice.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A121, 1717.17m, Core.Constants.CurrencyCodes.UnitedStates);

			invoice.GroupCharges.AddNew(ImportChargeMethodOneCodeList.Codes.A102, 111.11m, Core.Constants.CurrencyCodes.Australia);
			invoice.GroupCharges.AddNew(ImportChargeMethodOneCodeList.Codes.A104, 222.22m, Core.Constants.CurrencyCodes.Australia);
			invoice.GroupCharges.AddNew(ImportChargeMethodOneCodeList.Codes.A105, 333.33m, Core.Constants.CurrencyCodes.Australia);
			invoice.GroupCharges.AddNew(ImportChargeMethodOneCodeList.Codes.A106, 444.44m, Core.Constants.CurrencyCodes.Australia);
			invoice.GroupCharges.AddNew(ImportChargeMethodOneCodeList.Codes.A107, 555.55m, Core.Constants.CurrencyCodes.Australia);
			invoice.GroupCharges.AddNew(ImportChargeMethodOneCodeList.Codes.A108, 666.66m, Core.Constants.CurrencyCodes.Australia);
			invoice.GroupCharges.AddNew(ImportChargeMethodOneCodeList.Codes.A109, 777.77m, Core.Constants.CurrencyCodes.Australia);
			invoice.GroupCharges.AddNew(ImportChargeMethodOneCodeList.Codes.A110, 888.88m, Core.Constants.CurrencyCodes.Australia);
			invoice.GroupCharges.AddNew(ImportChargeMethodOneCodeList.Codes.A111, 999.99m, Core.Constants.CurrencyCodes.Australia);
			invoice.GroupCharges.AddNew(ImportChargeMethodOneCodeList.Codes.A112, 1010.10m, Core.Constants.CurrencyCodes.Australia);
			invoice.GroupCharges.AddNew(ImportChargeMethodOneCodeList.Codes.A114, 1111.11m, Core.Constants.CurrencyCodes.Australia);
			invoice.GroupCharges.AddNew(ImportChargeMethodOneCodeList.Codes.A115, 1212.12m, Core.Constants.CurrencyCodes.Australia);
			invoice.GroupCharges.AddNew(ImportChargeMethodOneCodeList.Codes.A116, 1313.13m, Core.Constants.CurrencyCodes.Australia);
			invoice.GroupCharges.AddNew(ImportChargeMethodOneCodeList.Codes.A118, 1414.14m, Core.Constants.CurrencyCodes.Australia);
			invoice.GroupCharges.AddNew(ImportChargeMethodOneCodeList.Codes.A119, 1515.15m, Core.Constants.CurrencyCodes.Australia);
			invoice.GroupCharges.AddNew(ImportChargeMethodOneCodeList.Codes.A120, 1616.16m, Core.Constants.CurrencyCodes.Australia);
			invoice.GroupCharges.AddNew(ImportChargeMethodOneCodeList.Codes.A121, 1717.17m, Core.Constants.CurrencyCodes.Australia);

			AssertEquals(100000m, invoice.JZ_InvoiceAmount);
			AssertEquals("USD", invoice.JZ_RX_NKInvoice_Currency);
			AssertEquals(999.67m, invoice.JZ_InvoiceCurrExRate);
			AssertEquals("100,000 * 999.67 = 99,967,000", 99967000m, invoice.JZ_InvoiceAmountInLocalCurrency);
			AssertEquals("(111.11 * 999.67 ≈ 111,073) + (111.11 * 891.34 ≈ 99,037) = 210,110", 210110m, invoice.IndirectAmount);
			AssertEquals("(222.22 * 999.67 ≈ 222,147) + (222.22 * 891.34 ≈ 198,074) = 420,221", 420221m, invoice.PurchaseCost);
			AssertEquals("(333.33 * 999.67 ≈ 333,220) + (333.33 * 891.34 ≈ 297,110) = 630,330", 630330m, invoice.BrokerageFee);
			AssertEquals("(444.44 * 999.67 ≈ 444,293) + (444.44 * 891.34 ≈ 396,147) = 840,440", 840440m, invoice.ContainerPackagingCost);
			AssertEquals("(555.55 * 999.67 ≈ 555,367) + (555.55 * 891.34 ≈ 495,184) = 1,050,551", 1050551m, invoice.GoodsCost);
			AssertEquals("(666.66 * 999.67 ≈ 666,440) + (666.66 * 891.34 ≈ 594,221) = 1,260,661", 1260661m, invoice.ProductToolCost);
			AssertEquals("(777.77 * 999.67 ≈ 777,513) + (777.77 * 891.34 ≈ 693,258) = 1,470,771", 1470771m, invoice.CommodityUsageCost);
			AssertEquals("(888.88 * 999.67 ≈ 888,587) + (888.88 * 891.34 ≈ 792,294) = 1,680,881", 1680881m, invoice.ProductDevCost);
			AssertEquals("(999.99 * 999.67 ≈ 999,660) + (999.99 * 891.34 ≈ 891,331) = 1,890,991", 1890991m, invoice.Royalty);
			AssertEquals("(1010.10 * 999.67 ≈ 1,009,767) + (1010.10 * 891.34 ≈ 900,343) = 1,910,110", 1910110m, invoice.ProfitAmount);
			AssertEquals("420,221 + 630,330 + 840,440 + 1,050,551 + 1,260,661 + 1,470,771 + 1,680,881 + 1,890,991 + 1,910,110 = 11,154,956", 11154956m, invoice.ExcludingTransportationCost);
			AssertEquals("(1111.11 * 999.67 ≈ 1,110,743) + (1111.11 * 891.34 ≈ 990,377) = 2,101,120", 2101120m, invoice.Freight);
			AssertEquals("(1212.12 * 999.67 ≈ 1,211,720) + (1212.12 * 891.34 ≈ 1,080,411) = 2,292,131", 2292131m, invoice.UnloadCost);
			AssertEquals("(1313.13 * 999.67 ≈ 1,312,697) + (1313.13 * 891.34 ≈ 1,170,445) = 2,483,142", 2483142m, invoice.Insurance);
			AssertEquals("2,101,120 + 2,292,131 + 2,483,142 = 6,876,393", 6876393m, invoice.TransportationCost);
			AssertEquals("11,154,956 + 6,876,393 = 18,031,349", 18031349m, invoice.TotalAdditionalAmount);
			AssertEquals("(1414.14 * 999.67 ≈ 1,413,673) + (1414.14 * 891.34 ≈ 1,260,480) = 2,674,153", 2674153m, invoice.LocalTransportationCost);
			AssertEquals("(1515.15 * 999.67 ≈ 1,514,650) + (1515.15 * 891.34 ≈ 1,350,514) = 2,865,164", 2865164m, invoice.TechnicalCost);
			AssertEquals("(1616.16 * 999.67 ≈ 1,615,627) + (1616.16 * 891.34 ≈ 1,440,548) = 3,056,175", 3056175m, invoice.OtherCost);
			AssertEquals("(1717.17 * 999.67 ≈ 1,716,603) + (1717.17 * 891.34 ≈ 1,530,582) = 3,247,185", 3247185m, invoice.DiscountAmount);
			AssertEquals("2,674,153 + 2,865,164 + 3,056,175 + 3,247,185 = 11,842,677", 11842677m, invoice.TotalDeductionAmount);
		}

		public void TestValuationMethodBColumnsFiveToSix()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CreateCurrency(CurrencyCodes.UnitedStates, 1300m);
			CreateCurrency(CurrencyCodes.Japan, 900m);
			CreateCurrency(CurrencyCodes.SriLanka, 4m);
			CreateCurrency(CurrencyCodes.UnitedKingdom, 1700m);

			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_InvoiceAmount = 100m;
			invoice.JZ_RX_NKInvoice_Currency = CurrencyCodes.UnitedStates;
			invoice.JZ_InvoiceCurrExRate = 1300m;

			CreateInvoiceCharges("501", CurrencyCodes.UnitedStates, 100m, 1300m);
			CreateInvoiceCharges("502", CurrencyCodes.Japan, 200m, 900m);
			CreateInvoiceCharges("503", CurrencyCodes.SriLanka, 300m, 4m);

			AssertEquals(100m, invoice.JZ_InvoiceAmount);
			AssertEquals("USD", invoice.JZ_RX_NKInvoice_Currency);
			AssertEquals(1300m, invoice.JZ_InvoiceCurrExRate);
			AssertEquals(130000m, invoice.AmountAgreedUponWithCustomsKRW);

			AssertEquals(147000m, invoice.AdditionalCostFreightToArrivalPort);
			AssertEquals(197000m, invoice.AdditionalCostFreightToDeparturePort);
			AssertEquals(18200m, invoice.AdditionalCostInsurance);
			AssertEquals(362200m, invoice.AdditionalCostTotalAdditionalAmount);

			void CreateCurrency(ZString currencyCode, ZDecimal sellRate)
			{
				RefCurrency currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currencyCode);

				var rate = Factory.New<RefExchangeRate>();
				rate.RE_GC = GlbCompany.CurrentCompany.PK;
				rate.RE_RX_NKExCurrency = currency.RX_Code;
				rate.RE_StartDate = ZDateTime.Today;
				rate.RE_ExpiryDate = ZDateTime.Today;
				rate.RE_SellRate = sellRate;
				rate.RE_ExRateType = ExchangeRateTypes.Code.CustomsRate;
			}

			void CreateInvoiceCharges(string chargeType, string currency, ZDecimal amount, ZDecimal rate)
			{
				var charge = invoice.Charges.AddNew();
				charge.J7_ChargeType = chargeType;
				charge.J7_RX_NKCurrency = currency;
				charge.J7_Amount = amount;
				charge.J7_ExchangeRate = rate;

				var groupCharge = invoice.GroupCharges.AddNew();
				groupCharge.J7_ChargeType = chargeType;
				groupCharge.J7_RX_NKCurrency = CurrencyCodes.UnitedKingdom;
				groupCharge.J7_Amount = 10m;
				groupCharge.J7_ExchangeRate = 1700m;
			}
		}

		public void TestValuationMethodBColumnsTwoToThree()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CreateCurrency(CurrencyCodes.UnitedStates, 1400m);
			CreateCurrency(CurrencyCodes.Japan, 9m);
			CreateCurrency(CurrencyCodes.SriLanka, 4m);
			CreateCurrency(CurrencyCodes.Pakistan, 5m);
			CreateCurrency(CurrencyCodes.Australia, 900m);
			CreateCurrency(CurrencyCodes.UnitedKingdom, 1700m);

			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = CurrencyCodes.UnitedStates;
			invoice.JZ_InvoiceCurrExRate = 1400m;

			CreateInvoiceCharges("309", CurrencyCodes.UnitedStates, 100m, 1400m);
			CreateInvoiceCharges("310", CurrencyCodes.Japan, 200m, 9m);
			CreateInvoiceCharges("311", CurrencyCodes.SriLanka, 300m, 4m);
			CreateInvoiceCharges("312", CurrencyCodes.Pakistan, 400m, 5m);
			CreateInvoiceCharges("313", CurrencyCodes.Australia, 500m, 900m);

			CreateInvoiceCharges("303", CurrencyCodes.UnitedStates, 10m, 1400m);
			CreateInvoiceCharges("304", CurrencyCodes.Japan, 20m, 9m);
			CreateInvoiceCharges("305", CurrencyCodes.SriLanka, 30m, 4m);
			CreateInvoiceCharges("306", CurrencyCodes.Pakistan, 40m, 5m);
			CreateInvoiceCharges("307", CurrencyCodes.Australia, 50m, 900m);

			AssertEquals(1000m, invoice.JZ_InvoiceAmount);
			AssertEquals("USD", invoice.JZ_RX_NKInvoice_Currency);
			AssertEquals(1400m, invoice.JZ_InvoiceCurrExRate);
			AssertEquals(1400000m, invoice.ReplacementAmountKRW);

			AssertEquals(157000m, invoice.AdditionalAdjustmentQuantityDiscount);
			AssertEquals(18800m, invoice.AdditionalAdjustmentCommercialAmount);
			AssertEquals(18200m, invoice.AdditionalAdjustmentTransportationCost);
			AssertEquals(19000m, invoice.AdditionalAdjustmentShippingPortCost);
			AssertEquals(467000m, invoice.AdditionalAdjustmentInsurance);
			AssertEquals(680000m, invoice.TotalAdditionalAdjustmentAmount);

			AssertEquals(31000m, invoice.DeductionAdjustmentQuantityDiscount);
			AssertEquals(17180m, invoice.DeductionAdjustmentCommercialAmount);
			AssertEquals(17120m, invoice.DeductionAdjustmentTransportationCost);
			AssertEquals(17200m, invoice.DeductionAdjustmentShippingPortCost);
			AssertEquals(62000m, invoice.DeductionAdjustmentInsurance);
			AssertEquals(144500m, invoice.TotalDeductionAdjustmentAmount);

			void CreateCurrency(ZString currencyCode, ZDecimal sellRate)
			{
				RefCurrency currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currencyCode);

				var rate = Factory.New<RefExchangeRate>();
				rate.RE_GC = GlbCompany.CurrentCompany.PK;
				rate.RE_RX_NKExCurrency = currency.RX_Code;
				rate.RE_StartDate = ZDateTime.Today;
				rate.RE_ExpiryDate = ZDateTime.Today;
				rate.RE_SellRate = sellRate;
				rate.RE_ExRateType = ExchangeRateTypes.Code.CustomsRate;
			}

			void CreateInvoiceCharges(string chargeType, string currency, ZDecimal amount, ZDecimal rate)
			{
				var charge = invoice.Charges.AddNew();
				charge.J7_ChargeType = chargeType;
				charge.J7_RX_NKCurrency = currency;
				charge.J7_Amount = amount;
				charge.J7_ExchangeRate = rate;

				var groupCharge = invoice.GroupCharges.AddNew();
				groupCharge.J7_ChargeType = chargeType;
				groupCharge.J7_RX_NKCurrency = CurrencyCodes.UnitedKingdom;
				groupCharge.J7_Amount = 10m;
				groupCharge.J7_ExchangeRate = 1700m;
			}
		}

		[ExpectNoExceptions]
		public void TestAllAddInfoColumnsAreInModelView()
		{
			ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(GetNewBusinessObject(), "KRJobComInvoiceHeader");
		}

		public void TestValuationMethodBColumnsFour()
		{
			var usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			SettingRate(usd, 999.67m, Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			var jpy = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Japan);
			SettingRate(jpy, 9.45m, Core.Constants.ExchangeRateTypes.Code.CustomsRate);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = "CRF";
			invoice.JZ_InvoiceAmount = 100000m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;

			invoice.Charges.AddNew(ImportChargeMethodFourCodeList.Codes.B404, 111.11m, Core.Constants.CurrencyCodes.UnitedStates);
			invoice.Charges.AddNew(ImportChargeMethodFourCodeList.Codes.B404, 222.22m, Core.Constants.CurrencyCodes.UnitedStates);

			invoice.Charges.AddNew(ImportChargeMethodFourCodeList.Codes.B405, 333.33m, Core.Constants.CurrencyCodes.UnitedStates);
			invoice.Charges.AddNew(ImportChargeMethodFourCodeList.Codes.B405, 444.44m, Core.Constants.CurrencyCodes.Japan);

			invoice.GroupCharges.AddNew(ImportChargeMethodFourCodeList.Codes.B406, 555m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			invoice.GroupCharges.AddNew(ImportChargeMethodFourCodeList.Codes.B406, 666m, Core.Constants.CurrencyCodes.KoreaRepublicOf);

			invoice.Charges.AddNew(ImportChargeMethodFourCodeList.Codes.B407, 777.77m, Core.Constants.CurrencyCodes.UnitedStates);
			invoice.GroupCharges.AddNew(ImportChargeMethodFourCodeList.Codes.B407, 888m, Core.Constants.CurrencyCodes.KoreaRepublicOf);

			invoice.Charges.AddNew(ImportChargeMethodFourCodeList.Codes.B408, 1m, Core.Constants.CurrencyCodes.UnitedStates);

			invoice.Charges.AddNew(ImportChargeMethodFourCodeList.Codes.B409, 2m, Core.Constants.CurrencyCodes.UnitedStates);

			invoice.Charges.AddNew(ImportChargeMethodFourCodeList.Codes.B410, 3m, Core.Constants.CurrencyCodes.UnitedStates);

			invoice.Charges.AddNew(ImportChargeMethodFourCodeList.Codes.B411, 4m, Core.Constants.CurrencyCodes.UnitedStates);

			AssertEquals(100000m, invoice.JZ_InvoiceAmount);
			AssertEquals("USD", invoice.JZ_RX_NKInvoice_Currency);
			AssertEquals(999.67m, invoice.JZ_InvoiceCurrExRate);
			AssertEquals(99967000m, invoice.JZ_InvoiceAmountInLocalCurrency);

			AssertEquals("(111.11 + 222.22) * 999.67", 333220m, invoice.ConsignmentSalesFee);
			AssertEquals("(333.33 * 999.67) + (444.44 * 9.45)", 337420m, invoice.GeneralCost);
			AssertEquals("555 + 666", 1221m, invoice.DeductionTransportationCost);
			AssertEquals("(777.77 * 999.67) + 888", 778401m, invoice.DeductionInsurance);
			AssertEquals("1 * 999.67", 1000m, invoice.DeductionUnloadCost);
			AssertEquals("2 * 999.67", 1999m, invoice.OtherTransportationCosts);
			AssertEquals("3 * 999.67", 2999m, invoice.AdditionalCost);
			AssertEquals("4 * 999.67", 3999m, invoice.Tax);
			AssertEquals("Total", 1460259m, invoice.DeductionCostTotalDeductionAmount);
		}
		public void TestCustomsReferenceNumber()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			AssertEquals(0, invoice.InvoiceHeaderRefs.Count);

			invoice.CustomsReferenceNumber = "ABCDEFG";
			AssertEquals(1, invoice.InvoiceHeaderRefs.Count);
			AssertEquals("ABCDEFG", invoice.InvoiceHeaderRefs[0].J2_ReferenceNumber);
		}

		public override void TestInvoiceLineChargeAffectsBalanceCalculation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 25000m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 10000m;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 20000m;
			declaration.ResumeApportionment();

			AssertEquals(0m, invoice.JZ_Calc_ChargesExcludedFromITOT);
			AssertNotEquals("Not balanced yet", 0m, invoice.JZ_Calc_Balance);

			var lineCharge = invoiceLine2.Charges.AddNew(GetDiscountChargeCodeForTest(), 5000m, declaration.LocalCurrencyCode);
			lineCharge.J7_IsDutiable = false;
			lineCharge.J7_IsGSTApplicable = false;
			lineCharge.J7_ChargeDescription = GetDiscountChargeDescriptionForTest();
			declaration.ResumeApportionment();
			AssertEquals("line level Discount", 0m, invoice.JZ_Calc_ChargesExcludedFromITOT);
			AssertEquals("Now balanced", -5000m, invoice.JZ_Calc_Balance);

			var invoiceCharge = invoice.Charges.AddNew(GetDiscountChargeCodeForTest(), 5000m, declaration.LocalCurrencyCode);
			invoiceCharge.J7_IsDutiable = false;
			invoiceCharge.J7_IsGSTApplicable = false;
			invoiceCharge.J7_ChargeDescription = GetDiscountChargeDescriptionForTest();
			declaration.ResumeApportionment();
			AssertEquals("Invoice Level Discount. Line level Discount is disregarded", 0m, invoice.JZ_Calc_ChargesExcludedFromITOT);
			AssertEquals("Still balanced", -5000m, invoice.JZ_Calc_Balance);
		}

		public void TestIsProvPricingYN()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			AssertEquals(false, invoice.IsProvPricing);

			invoice.JZ_ProvPricingYN = YesNoList.Codes.Yes;
			AssertEquals(true, invoice.IsProvPricing);

			invoice.JZ_ProvPricingYN = YesNoList.Codes.No;
			AssertEquals(false, invoice.IsProvPricing);

			invoice.JZ_ProvPricingYN = ZString.Empty;
			AssertEquals(false, invoice.IsProvPricing);
		}
		#region Implementation

		public override string GetLocalCurrencyCode() => Core.Constants.CurrencyCodes.KoreaRepublicOf;
		protected override bool RatesAreReciprocal => true;

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceCharge>);

		#endregion
	}
	#endregion

	#region JobComInvoiceHeaderFunctionalTest
	sealed class JobComInvoiceHeaderFunctionalTest : Customs.Business.Testing.BaseJobComInvoiceHeaderFunctionalTest
	{
		#region Implementation
		protected override BaseJobDeclaration GetNewDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}
		#endregion
	}
	#endregion

	#region JobComInvoiceHeaderApportionTest
	sealed class JobComInvoiceHeaderApportionTest : Customs.Business.Testing.BaseJobComInvoiceHeaderApportionTest
	{
		#region Implementation
		protected override BaseJobDeclaration GetNewDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}
		#endregion
	}
	#endregion

	#region JobComInvoiceHeaderCalculationTest
	sealed class JobComInvoiceHeaderCalculationTest : Customs.Business.Testing.BaseJobComInvoiceHeaderCalculationTest
	{
		#region Implementation
		protected override BaseJobDeclaration GetNewDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}

		public override void TestCalculateCIFWithCFR()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "VAL"))
			{
				header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
				header.JZ_InvoiceAmount = 10600;
				PrepareCharge(header.Charges.AddNew("EXW", 100));
				PrepareCharge(header.Charges.AddNew("DIS", 1));
				BaseJobComInvHeaderCharge baseJobComInvHeaderCharge = header.Charges.AddNew("OTH", 5);
				PrepareCharge(baseJobComInvHeaderCharge);
				baseJobComInvHeaderCharge.J7_IsDutiable = true;
				baseJobComInvHeaderCharge.J7_IsGSTApplicable = true;
				PrepareCharge(header.Charges.AddNew("OFT", 400));
				header.JZ_IncoTerm = header.IncotermEquivalentToCFRForTesting;
				ZDecimal zDecimal = 10600m;
				declaration.ResumeApportionment();
				Assertion.AssertEquals("CIF value / CFR ", zDecimal, header.JZ_Calc_CIFAmount);
				PrepareCharge(header.Charges.AddNew("ONS", 100, header.JobDeclaration.LocalCurrencyCode));
				declaration.ResumeApportionment();
				zDecimal = 10700m;
				Assertion.AssertEquals("CIF value / CFR ", zDecimal, header.JZ_Calc_CIFAmount);
			}
		}

		public override void TestCalculateCIFWithCIP()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "VAL"))
			{
				header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
				header.JZ_InvoiceAmount = 10600;
				PrepareCharge(header.Charges.AddNew("EXW", 100));
				PrepareCharge(header.Charges.AddNew("DIS", 1));
				BaseJobComInvHeaderCharge baseJobComInvHeaderCharge = header.Charges.AddNew("OTH", 5);
				PrepareCharge(baseJobComInvHeaderCharge);
				baseJobComInvHeaderCharge.J7_IsDutiable = true;
				baseJobComInvHeaderCharge.J7_IsGSTApplicable = true;
				header.Charges.AddNew("ONS", 300);
				header.JZ_IncoTerm = "CIP";
				ZDecimal zDecimal = 10600m;
				declaration.ResumeApportionment();
				Assertion.AssertEquals("CIF value / CIP ", zDecimal, header.JZ_Calc_CIFAmount);
				BaseJobComInvoiceGroupHeader master = header.Master;
				PrepareCharge(master.Charges.AddNew("OFT", 100, header.JobDeclaration.LocalCurrencyCode));
				declaration.ResumeApportionment();
				header.GroupCharges[0].J7_IsIncludedInITOT = false;
				zDecimal = 10700m;
				Assertion.AssertEquals("CIF value / CIP ", zDecimal, header.JZ_Calc_CIFAmount);
			}
		}

		public override void TestCalculateCIFWithDDU()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
			header.JZ_InvoiceAmount = 10600;
			header.Charges.AddNew("PAC", 40);
			header.Charges.AddNew("EXW", 100);
			header.Charges.AddNew("DIS", 1);
			BaseJobComInvHeaderCharge baseJobComInvHeaderCharge = header.Charges.AddNew("OTH", 5);
			baseJobComInvHeaderCharge.J7_IsDutiable = true;
			baseJobComInvHeaderCharge.J7_IsGSTApplicable = true;
			BaseJobComInvHeaderCharge baseJobComInvHeaderCharge2 = header.Charges.AddNew("OTH", 600);
			baseJobComInvHeaderCharge2.J7_IsDutiable = false;
			baseJobComInvHeaderCharge2.J7_IsGSTApplicable = false;
			baseJobComInvHeaderCharge2.J7_IsIncludedInITOT = true;
			header.Charges.AddNew("OFT", 400);
			header.Charges.AddNew("ONS", 300);
			header.Charges.AddNew("LCH", 500);
			header.JZ_IncoTerm = "DDU";
			ZDecimal zDecimal = 9500m;
			Assertion.AssertEquals("CIF value / DDU ", zDecimal, header.JZ_Calc_CIFAmount);
		}

		public override void TestCalculateCIFWithFOB()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "VAL"))
			{
				header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
				header.JZ_InvoiceAmount = 10600;
				PrepareCharge(header.Charges.AddNew("EXW", 100));
				PrepareCharge(header.Charges.AddNew("DIS", 1));
				BaseJobComInvHeaderCharge baseJobComInvHeaderCharge = header.Charges.AddNew("OTH", 5);
				PrepareCharge(baseJobComInvHeaderCharge);
				baseJobComInvHeaderCharge.J7_IsDutiable = true;
				baseJobComInvHeaderCharge.J7_IsGSTApplicable = true;
				header.JZ_IncoTerm = "FOB";
				ZDecimal zDecimal = 10600m;
				Assertion.AssertEquals("CIF / FOB ", zDecimal, header.JZ_Calc_CIFAmount);
				PrepareCharge(header.Charges.AddNew("OFT", 100, header.JobDeclaration.LocalCurrencyCode));
				zDecimal = 10700m;
				declaration.ResumeApportionment();
				Assertion.AssertEquals("CIF / FOB ", zDecimal, header.JZ_Calc_CIFAmount);
			}
		}

		public new void TestBalanceGroupHeaderChargeAndHeaderCharge()
		{
			groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 100, header.JobDeclaration.LocalCurrencyCode);

			header.JZ_InvoiceAmount = 1000;
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 200);
			declaration.ResumeApportionment();
			AssertEquals("The balance should be Invoice Total less cost of its own", new ZDecimal(1000), header.JZ_Calc_Balance);
		}

		public new void TestCalculateBalanceWithInvalidCurrencyOnInlandFreight()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

			var line1 = header.JobComInvoiceLines.AddNew();

			header.JZ_InvoiceAmount = 10000;
			declaration.ResumeApportionment();
			AssertEquals("Initial Balance", new ZDecimal(10000), header.JZ_Calc_Balance);

			header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 1000);
			declaration.ResumeApportionment();
			AssertEquals("Balance after adding Inland Freight (defaulted currency)", new ZDecimal(10000), header.JZ_Calc_Balance);
		}

		public new void TestCalculateRealInvoiceTotal()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 300);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 10);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 1);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Commission, 5);

			//FOB Incoterm
			header.JZ_IncoTerm = "FOB";
			ZDecimal expected = new ZDecimal(10600 - 200 - 300 + 1 - 5);
			AssertEquals("Real Invoice / FOB ", expected, header.InvoiceLineTotal);

			//DDP Incoterm
			header.JZ_IncoTerm = "DDP";
			expected = new ZDecimal(10600 - 200 - 300 - 100 - 10 - 200 + 1 - 5);
			AssertEquals("Real Invoice / DDP ", expected, header.InvoiceLineTotal);

			//CIF Incoterm
			header.JZ_IncoTerm = "CIF";
			expected = 10600 - 200 - 300 - 100 + 1 - 5 - 10;
			AssertEquals("Real Invoice / CIF ", expected, header.InvoiceLineTotal);
		}

		public new void TestChangeIncotermResetIncludedInInvoiceFlagsForInvoiceCharges()
		{
			header.JZ_IncoTerm = "FOB";

			var oFT = header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, declaration.LocalCurrencyCode);
			var oNS = header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, declaration.LocalCurrencyCode);
			var cOM = header.Charges.AddNew(CustomsChargeTypeList.Codes.Commission, 100m, declaration.LocalCurrencyCode);

			var line1 = header.InvoiceLines.AddNew();
			var line1OFT = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 20m, declaration.LocalCurrencyCode);

			var line2 = header.InvoiceLines.AddNew();

			AssertEquals("IsIncludedInInvoice is set for OFT", false, oFT.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("IsIncludedInInvoice is set for ONS", false, oNS.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("IsIncludedInInvoice is set for COM", true, cOM.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("IsIncludedInInvoice is set for Line1 Charge", false, line1OFT.J7_Calc_IsIncludedInInvoiceAmount);

			cOM.J7_Calc_IsIncludedInInvoiceAmount = false;
			header.JZ_IncoTerm = "CIF";

			AssertEquals("IsIncludedInInvoice is set for OFT", true, oFT.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("IsIncludedInInvoice is set for ONS", true, oNS.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("IsIncludedInInvoice for COM stayed as users entered", true, cOM.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("IsIncludedInInvoice is refreshed for Line1 Charge for the incoterm change", true, line1OFT.J7_Calc_IsIncludedInInvoiceAmount);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			groupHeader = (JobComInvoiceGroupHeader)declaration.JobComInvoiceGroupHeaders[0];
			header = declaration.Invoices.AddNew();
		}
		JobDeclaration declaration;
		JobComInvoiceGroupHeader groupHeader;
		JobComInvoiceHeader header;
		#endregion
	}
	#endregion

	#region JobComInvoiceHeaderTestForDocumentWrappert
	sealed class JobComInvoiceHeaderTestForDocumentWrappert : Customs.Business.Testing.BaseJobComInvoiceHeaderTestForDocumentWrapper
	{
		#region Implementation
		protected override BaseJobDeclaration GetNewDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}
		#endregion
	}
	#endregion
}
