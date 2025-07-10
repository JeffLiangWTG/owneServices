using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Business.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;
using TariffTypes = Enterprise.Customs.Universal.Constants.TariffTypes;
using UniversalConstants = Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLine))]
	sealed class JobComInvoiceLineTest : Customs.Business.Testing.BaseJobComInvoiceLineAbstractTest
	{
		[ExpectNoExceptions]
		public void TestAllAddInfoColumnsAreInModelView()
		{
			var jobComInvoiceLine = Factory.New<JobComInvoiceLine>();
			ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(jobComInvoiceLine,
				"KRJobComInvoiceLine",
				schemaTypeName: nameof(AutoKRJobComInvoiceLine.Schema));
		}

		public override void TestPivot()
		{
			IClassificationTypeProvider classificationTypeProvider = InvoiceLine.GetClassificationTypeProvider();
			ZString hTECode = classificationTypeProvider.HTECode;
			ZString hTICode = classificationTypeProvider.HTICode;

			Declaration.JE_MessageType = DeclarationExportMessageType;

			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PARTNUM";

			OrgPartRelation supplierRelation1 = part.RelatedOrganisations.AddNew();
			supplierRelation1.OU_OH = Factory.New<OrgHeader>().PK;
			supplierRelation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			OrgPartRelation supplierRelation2 = part.RelatedOrganisations.AddNew();
			supplierRelation2.OU_OH = Factory.New<OrgHeader>().PK;
			supplierRelation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			var pivot1 = part.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = hTICode;
			pivot1.CI_OH = supplierRelation1.OU_OH;
			var pivot2 = part.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = hTECode;
			pivot2.CI_OH = supplierRelation1.OU_OH;
			var pivot3 = part.PivotsForBinding.AddNew();
			pivot3.CI_ChildType = hTICode;
			pivot3.CI_OH = supplierRelation2.OU_OH;

			InvoiceLine.InvoiceHeader.JZ_OH_Supplier = supplierRelation1.OU_OH;
			InvoiceLine.JI_PartNo = part.OP_PartNum;

			AssertNotNull(InvoiceLine.Part);
			AssertEquals(pivot2.PK, InvoiceLine.Pivot.PK);

			Declaration.JE_MessageType = DeclarationImportMessageType;
			AssertEquals(pivot1.PK, InvoiceLine.Pivot.PK);

			InvoiceLine.InvoiceHeader.JZ_OH_Supplier = supplierRelation2.OU_OH;
			InvoiceLine.JI_PartNo = part.OP_PartNum;

			AssertNotNull(InvoiceLine.Part);
			AssertEquals(pivot3.PK, InvoiceLine.Pivot.PK);
		}

		public void TestCustomsCountryCode()
		{
			AssertEquals("CustomsCountryCodeCore should be KR", Core.Constants.CountryCodes.KoreaSouth, InvoiceLine.CustomsCountryCode);
		}

		public void TestIInvoiceLinePartDetailsMembers()
		{
			using (Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				IInvoiceLinePartDetails partDetails = Factory.New<JobComInvoiceLine>();
				AssertEquals(Core.Constants.CountryCodes.KoreaSouth, partDetails.CustomsCountryCode);
				AssertEquals(typeof(OrgSupplierPart), partDetails.TypeOfPartUsed);
			}
		}

		public void TestPartType()
		{
			var factory2 = new BusinessObjectFactory();
			var importer = factory2.New<OrgHeader>();
			importer.FillWithValidTestData();
			MasterFiles.Business.OrgSupplierPart product = (MasterFiles.Business.OrgSupplierPart)factory2.New<Integration.Customs.AU.IOrgSupplierPart>();
			product.OP_PartNum = "TestTEST";
			product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			factory2.Save();

			var customsTemplate_Company = Factory.New<GlbCompany>();
			customsTemplate_Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			var customsTemplate_Branch = customsTemplate_Company.Branches.AddNew();
			customsTemplate_Branch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.KoreaSouth)).RL_Code;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_GB = customsTemplate_Branch.PK;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "TestTEST";
			AssertEquals("Product type gets changed depending on who is requesting", typeof(OrgSupplierPart), invoiceLine.Part.GetType());
			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry("AU");
			var factory3 = new BusinessObjectFactory();
			var declarationLoaded = factory3.Load<JobDeclaration>(declaration.PK);
			AssertEquals("product type still the type", typeof(OrgSupplierPart), declarationLoaded.InvoiceLines[0].Part.GetType());
		}

		public void TestCusSupportingInfoIsDeletedWhenInvoiceLineIsDeleted()
		{
			var invoiceLine = (JobComInvoiceLine)InvoiceLine;
			var preApproval = new PreApprovalCollection(invoiceLine).AddNew();
			var gaApproval = new GAApprovalCollection(invoiceLine).AddNew();
			var certificate = new CertificateOfOriginCollection(invoiceLine).AddNew();
			var nonGADetail = new NonGADetailCollection(invoiceLine).AddNew();
			var previousExpDecLine = new PreviousExpDecLineCollection(invoiceLine).AddNew();
			var supprotingDocument = new SupportingDocumentCollection(invoiceLine).AddNew();

			invoiceLine.Delete();
			Assert(preApproval.IsDeleted);
			Assert(gaApproval.IsDeleted);
			Assert(certificate.IsDeleted);
			Assert(nonGADetail.IsDeleted);
			Assert(previousExpDecLine.IsDeleted);
			Assert(supprotingDocument.IsDeleted);
		}

		public void TestTypeDecider()
		{
			Assert("Update Customs.Business.BaseJobComInvoiceLine to include a decider for this class", Factory.New(typeof(BaseJobComInvoiceLine)).GetType() == GetExpectedBusinessObjectType());
		}

		public void TestPreApprovalData()
		{
			var invoiceLine = (JobComInvoiceLine)InvoiceLine;
			AssertEquals(0, invoiceLine.PreApprovalCollection.Count);

			invoiceLine.PRA_ReferenceNumber = "12345";
			AssertNotNull(invoiceLine.PreApprovalCollection);
			AssertEquals("12345", invoiceLine.PRA_ReferenceNumber);

			invoiceLine.PreApprovalCollection.RemoveAndDeleteAll();
			invoiceLine.PRA_DateOfIssue = ZDateTime.Today;
			AssertNotNull(invoiceLine.PreApprovalCollection);
			AssertEquals(ZDateTime.Today, invoiceLine.PRA_DateOfIssue);

			invoiceLine.PreApprovalCollection.RemoveAndDeleteAll();
			invoiceLine.PRA_DateOfExpiry = ZDateTime.Today;
			AssertNotNull(invoiceLine.PreApprovalCollection);
			AssertEquals(ZDateTime.Today, invoiceLine.PRA_DateOfExpiry);
		}

		public void TestEffectiveAssementDate()
		{
			var invoiceLine = (JobComInvoiceLine)InvoiceLine;
			invoiceLine.Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			invoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(ZDateTime.Today, invoiceLine.EffectiveAssessmentDate);

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(invoiceLine.Declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			invoiceLine.Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(ZDateTime.Today, invoiceLine.EffectiveAssessmentDate);

			var exportEntryNum = invoiceLine.CusEntryLine.Header.EntryNumbers.AddNew();
			exportEntryNum.CE_EntryType = KRJobMessageTypeList.Codes.Export;
			AssertEquals(ZDateTime.Today, invoiceLine.EffectiveAssessmentDate);

			invoiceLine.CusEntryLine.Header.CusEntryNumber.CE_IssueDate = new ZDateTime(2021, 7, 20);
			AssertEquals(new ZDateTime(2021, 7, 20), invoiceLine.EffectiveAssessmentDate);

			invoiceLine.Declaration.ThrowAwayMerge();

			invoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(ZDateTime.Today, invoiceLine.EffectiveAssessmentDate);

			invoiceLine.Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(ZDateTime.Today, invoiceLine.EffectiveAssessmentDate);

			var importEntryNum = invoiceLine.CusEntryLine.Header.EntryNumbers.AddNew();
			importEntryNum.CE_EntryType = KRJobMessageTypeList.Codes.Import;
			AssertEquals(ZDateTime.Today, invoiceLine.EffectiveAssessmentDate);

			invoiceLine.CusEntryLine.Header.CusEntryNumber.CE_IssueDate = new ZDateTime(2021, 7, 20);
			AssertEquals(new ZDateTime(2021, 7, 20), invoiceLine.EffectiveAssessmentDate);
		}

		public void TestGetDutyReductionClassificationCodeWhenAorB()
		{
			SetUpDutyReductionExemptionTariffData();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_IsSpecificUseCode = false;
			invoiceLine.JI_SecondaryPreference = "A093000004";
			AssertEquals(ImportDutyReductionClassificationList.Codes.DutyExemption, invoiceLine.DutyReductionClassificationCode);

			invoiceLine.JI_SecondaryPreference = "A1070001";
			AssertEquals(ImportDutyReductionClassificationList.Codes.DutyReduction, invoiceLine.DutyReductionClassificationCode);
		}

		public void TestGetDutyReductionClassificationCodeWhenSpecificUseCodeIsUsed()
		{
			SetUpDutyReductionExemptionTariffData();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_IsSpecificUseCode = true;
			invoiceLine.JI_InstallmentCode = "";
			AssertEquals(ImportDutyReductionClassificationList.Codes.SpecificUseCodeOnly, invoiceLine.DutyReductionClassificationCode);

			invoiceLine.JI_IsSpecificUseCode = false;
			invoiceLine.JI_InstallmentCode = "ABC";
			AssertEquals(ImportDutyReductionClassificationList.Codes.InstallmentPayment, invoiceLine.DutyReductionClassificationCode);

			invoiceLine.JI_IsSpecificUseCode = true;
			invoiceLine.JI_InstallmentCode = "ABC";
			AssertEquals(ImportDutyReductionClassificationList.Codes.SpecificUseCodeAndDutyReductionOrInstallment, invoiceLine.DutyReductionClassificationCode);

			invoiceLine.JI_IsSpecificUseCode = true;
			invoiceLine.JI_InstallmentCode = "";
			invoiceLine.JI_SecondaryPreference = "A1070001";
			AssertEquals(ImportDutyReductionClassificationList.Codes.SpecificUseCodeAndDutyReductionOrInstallment, invoiceLine.DutyReductionClassificationCode);
		}

		public void TestDutyReductionClassificationCodeWhenInvalid()
		{
			SetUpDutyReductionExemptionTariffData();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_IsSpecificUseCode = true;
			invoiceLine.JI_InstallmentCode = "ABC";
			invoiceLine.JI_SecondaryPreference = "A1070001";
			AssertEquals(ImportDutyReductionClassificationList.Codes.Invalid, invoiceLine.DutyReductionClassificationCode);

			invoiceLine.JI_IsSpecificUseCode = false;
			invoiceLine.JI_InstallmentCode = "ABC";
			invoiceLine.JI_SecondaryPreference = "A1070001";
			AssertEquals(ImportDutyReductionClassificationList.Codes.Invalid, invoiceLine.DutyReductionClassificationCode);

			invoiceLine.JI_InstallmentCode = "";
			invoiceLine.JI_SecondaryPreference = "";
			AssertNullOrEmpty(invoiceLine.DutyReductionClassificationCode);
		}

		public void TestReductionRateRegulationGroupNumber()
		{
			var invoiceLine = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			invoiceLine.DutyReductionGroupNumber = "A";
			AssertEquals("A::", invoiceLine.JI_DutyReductionRateRegulationCode);

			invoiceLine.JI_DutyReductionRateRegulationCode = ":001:01";
			AssertEquals("", invoiceLine.DutyReductionGroupNumber);

			invoiceLine.JI_DutyReductionRateRegulationCode = "B::02";
			AssertEquals("B", invoiceLine.DutyReductionGroupNumber);

			invoiceLine.DutyReductionGroupNumber = "C";
			AssertEquals("C::02", invoiceLine.JI_DutyReductionRateRegulationCode);
		}
		public void TestReductionRateRegulationSeqNumber()
		{
			var invoiceLine = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			invoiceLine.DutyReductionSeqNumber = "002";
			AssertEquals(":002:", invoiceLine.JI_DutyReductionRateRegulationCode);

			invoiceLine.JI_DutyReductionRateRegulationCode = "A::02";
			AssertEquals("", invoiceLine.DutyReductionSeqNumber);

			invoiceLine.JI_DutyReductionRateRegulationCode = ":001:02";
			AssertEquals("001", invoiceLine.DutyReductionSeqNumber);

			invoiceLine.DutyReductionSeqNumber = "003";
			AssertEquals(":003:02", invoiceLine.JI_DutyReductionRateRegulationCode);
		}
		public void TestReductionRateRegulationItemNumber()
		{
			var invoiceLine = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			invoiceLine.DutyReductionItemNumber = "01";
			AssertEquals("::01", invoiceLine.JI_DutyReductionRateRegulationCode);

			invoiceLine.JI_DutyReductionRateRegulationCode = "A:001:";
			AssertEquals("", invoiceLine.DutyReductionItemNumber);

			invoiceLine.JI_DutyReductionRateRegulationCode = "A:001:02";
			AssertEquals("02", invoiceLine.DutyReductionItemNumber);

			invoiceLine.DutyReductionItemNumber = "03";
			AssertEquals("A:001:03", invoiceLine.JI_DutyReductionRateRegulationCode);
		}

		void SetUpDutyReductionExemptionTariffData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, ZZ.TariffTypes.DutyReductionExemption);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "A093000004", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "관세법 제93조제4호 해당물품");
			helper.CreateTariffAttribute(Customs.KR.Messaging.Constants.ZZ.TariffAttributes.IsDutyExempt, YesNo.Yes, tariff1);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "A1070001", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "관세법 제107조 제1항 해당물품");
			Factory.Save();
		}

		public void TestJI_TariffMaxLength()
		{
			AssertEquals(10, InvoiceLine.JI_TariffInfo.MaxLength);
		}

		public override void TestJI_FormattedTariff()
		{
			InvoiceLine.JI_Tariff = "123 456 78.900";
			AssertEquals("JI_FormattedTariff", "1234.56-7890", InvoiceLine.JI_FormattedTariff);

			InvoiceLine.JI_FormattedTariff = "987.654.32.100";
			AssertEquals("JI_FormattedTariff", "9876.54-3210", InvoiceLine.JI_FormattedTariff);
			AssertEquals("JI_Tariff", "9876543210", InvoiceLine.JI_Tariff);
			AssertEquals(12, InvoiceLine.JI_FormattedTariffInfo.MaxLength);
		}

		public void TestCustomsUnitsReadOnly()
		{
			AssertEquals(true, InvoiceLine.JI_CustomsUnitQtyInfo.ReadOnly);
			AssertEquals(true, InvoiceLine.JI_CustomsSecondUnitQtyInfo.ReadOnly);
			AssertEquals(true, InvoiceLine.JI_CustomsThirdUnitQtyInfo.ReadOnly);
		}

		public void TestGetTotalAmountInLocalCurrency()
		{
			var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			SetExchangeRate(GlbCompany.CurrentCompany, Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary, ZDateTime.Today, ZDateTime.Today, 1100.5m, usdCurrency);

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var usdRate = entry.CurrencyConverter.GetExchangeRate(usdCurrency);
			AssertEquals(1100.5m, usdRate);

			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;
			invoiceLine.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A114, 80m, Core.Constants.CurrencyCodes.UnitedStates);
			invoiceLine.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A114, 20m, Core.Constants.CurrencyCodes.UnitedStates);
			invoiceLine.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A116, 400m, Core.Constants.CurrencyCodes.UnitedStates);
			invoiceLine.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A116, 600m, Core.Constants.CurrencyCodes.UnitedStates);
			var overseasFreight1 = invoiceLine.GetImportFreightInLocalCurrency(entry.CurrencyConverter, invoice.JZ_ValuationCode);
			AssertEquals("OverseasFreight Charges converted to local currency", 110050m, overseasFreight1);
			var overseasInsurance1 = invoiceLine.GetImportInsuranceInLocalCurrency(entry.CurrencyConverter, invoice.JZ_ValuationCode);
			AssertEquals("OverseasInsurance Charges converted to local currency", 1100500m, overseasInsurance1);

			((IChargeApportionee)invoiceLine).ApportionedCharges.AddNew(ImportChargeMethodOneCodeList.Codes.A114, 70m, Core.Constants.CurrencyCodes.UnitedStates);
			((IChargeApportionee)invoiceLine).ApportionedCharges.AddNew(ImportChargeMethodOneCodeList.Codes.A114, 30m, Core.Constants.CurrencyCodes.UnitedStates);
			((IChargeApportionee)invoiceLine).ApportionedCharges.AddNew(ImportChargeMethodOneCodeList.Codes.A116, 900m, Core.Constants.CurrencyCodes.UnitedStates);
			((IChargeApportionee)invoiceLine).ApportionedCharges.AddNew(ImportChargeMethodOneCodeList.Codes.A116, 100m, Core.Constants.CurrencyCodes.UnitedStates);

			var overseasFreight2 = invoiceLine.GetImportFreightInLocalCurrency(entry.CurrencyConverter, invoice.JZ_ValuationCode);
			AssertEquals("OverseasFreight Charges and ApportionedCharges converted to local currency", 220100m, overseasFreight2);
			var overseasInsurance2 = invoiceLine.GetImportInsuranceInLocalCurrency(entry.CurrencyConverter, invoice.JZ_ValuationCode);
			AssertEquals("OverseasInsurance Charges and ApportionedChargesconverted to local currency", 2201000m, overseasInsurance2);
		}
		void SetExchangeRate(GlbCompany company, ZString rateType, ZDateTime startDate, ZDateTime endDate, ZDecimal rate, RefCurrency foreignCurrency)
		{
			RefExchangeRate result = Factory.New<RefExchangeRate>();

			result.RE_GC = company.PK;
			result.RE_RX_NKExCurrency = foreignCurrency.RX_Code;
			result.RE_StartDate = startDate;
			result.RE_ExpiryDate = endDate;
			result.RE_SellRate = rate;
			result.RE_ExRateType = rateType;

			Factory.Save();
		}
		public override void TestChargeTypeList()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			Customs.Business.ICommonInvoice commonInvoice = dec.Invoices.AddNew().JobComInvoiceLines.AddNew();
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

			customsChargeTypeList = new Customs.Business.CustomsChargeTypeList();
			customsChargeTypeList.Sort();
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);
		}
		public void TestJI_IsSpecificUseCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var universalTariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, TariffTypes.HarmonizedSystem);
			var universalTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, universalTariffType.PK, "1234500000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "관세법 제88조제1항제1호 해당물품");
			var conditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.KoreaSouth, ZZ.RefCusConditionClass.RATE, ZZ.RefCusConditionType.PostClearanceProcedure);
			var preference = helper.CreatePreferenceForCountry("FEU1", "FEU1", Core.Constants.CountryCodes.KoreaSouth);
			var condition = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.KoreaSouth, conditionType.PK, universalTariff.PK, "Post Clearance Procedure", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, preferencePK: preference.PK);
			var condValueType = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.KoreaSouth, ZZ.RefCusConditionType.PostClearanceProcedure);
			helper.CreateOrGetExistingRefCusConditionValue(condValueType.PK, condition.PK, "A");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.JI_Tariff = "0000000000";
			invoiceLine.JI_PrimaryPreference = "FEU2";
			AssertEquals(false, invoiceLine.JI_IsSpecificUseCode);

			invoiceLine.JI_Tariff = "1234500000";
			invoiceLine.JI_PrimaryPreference = "FEU2";
			AssertEquals(false, invoiceLine.JI_IsSpecificUseCode);

			invoiceLine.JI_Tariff = "0000000000";
			invoiceLine.JI_PrimaryPreference = "FEU1";
			AssertEquals(false, invoiceLine.JI_IsSpecificUseCode);

			invoiceLine.JI_Tariff = "1234500000";
			invoiceLine.JI_PrimaryPreference = "FEU1";
			AssertEquals(true, invoiceLine.JI_IsSpecificUseCode);
		}

		public void TestConignee()
		{
			var conignee = Factory.New<OrgHeader>();
			var conigneeAddress = conignee.MainAddress;
			conigneeAddress.OA_Phone = "010-0000-0000";
			conigneeAddress.OA_PostCode = "12345";
			conigneeAddress.OA_Address1 = "Address 1";
			conigneeAddress.OA_Address2 = "Address 2";
			var invoiceLine = (JobComInvoiceLine)InvoiceLine;
			invoiceLine.JI_OA_ConsigneeAddress = conigneeAddress.PK;

			AssertNotNull(invoiceLine.Consignee);
			AssertEquals("010-0000-0000", invoiceLine.Consignee.PhoneNumber);
			AssertEquals("12345", invoiceLine.Consignee.Postcode);
			AssertEquals("Address 1 Address 2", invoiceLine.Consignee.AddressDetails);
		}

		public void TestChangeDecimalPlacesOfUnitPrice()
		{
			var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine.JI_LinePrice = 100;
			invoiceLine.JI_InvoiceQuantity = 3;
			AssertEquals(33.333333m, invoiceLine.UnitPrice);
		}

		public void TestZPropertyInfoChange()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			var line = header.InvoiceLines.AddNew();

			AssertEquals(50, line.JI_ModelInfo.MaxLength);
			AssertEquals(30, line.JI_BrandNameInfo.MaxLength);
			AssertEquals(3, line.JI_PreviousEntryLineNumberInfo.MaxLength);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(35, line.JI_PreviousEntryNumberInfo.MaxLength);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(16, line.JI_PreviousEntryNumberInfo.MaxLength);
		}

		public void TestIInvoiceOrProductIsImplemented()
		{
			var line = Factory.New<JobComInvoiceLine>();
			AssertEquals(1, line.GetType().FindInterfaces(new System.Reflection.TypeFilter((type, criteria) => type.ToString().Equals(criteria.ToString())), "Enterprise.Customs.KR.Business.ILineOrProduct").Length);
		}

		public void TestChangeIsExportForJobComInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			var line = header.InvoiceLines.AddNew();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Assert(line.IsExport);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert(!line.IsExport);

			var header2 = Factory.New<JobComInvoiceHeader>();
			var line2 = header2.InvoiceLines.AddNew();

			header2.JZ_MessageType = JobMessageTypeList.Codes.Export;
			Assert(line2.IsExport);
			header2.JZ_MessageType = JobMessageTypeList.Codes.Import;
			Assert(!line2.IsExport);
		}

		public void TestGetCurrencyFromParent()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(invoiceLine1.CusEntryLine.Header.CurrencyConverter, invoiceLine1.CurrencyConverter);

			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			AssertEquals(invoice2.CurrencyConverter, invoiceLine2.CurrencyConverter);
		}

		public void TestKR_HighestGAApprovalSeqNo()
		{
			var entry = declaration.CustomsEntryHeaders[0];
			var invoiceLine1 = declaration.InvoiceLines[0];
			invoiceLine1.GAApprovalDataCollection.AddNew().CSI_LineNo = 1;
			var invoiceLine2 = declaration.InvoiceLines[1];

			var gaApprovalData1 = invoiceLine2.GAApprovalDataCollection.AddNew();
			gaApprovalData1.CSI_LineNo = 1;
			var gaApprovalData2 = invoiceLine2.GAApprovalDataCollection.AddNew();
			gaApprovalData2.CSI_LineNo = 2;

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			AssertEquals(0u, invoiceLine1.KR_HighestGAApprovalSeqNo);
			AssertEquals(0u, invoiceLine2.KR_HighestGAApprovalSeqNo);
			var genAddOnColumn1 = GetGenAddOnColumn(Factory, invoiceLine1.PK, JobComInvoiceLine.GenAddOnColumnConstants.KR_HighestGAApprovalSeqNo);
			var genAddOnColumn2 = GetGenAddOnColumn(Factory, invoiceLine2.PK, JobComInvoiceLine.GenAddOnColumnConstants.KR_HighestGAApprovalSeqNo);
			AssertNull(genAddOnColumn1);
			AssertNull(genAddOnColumn2);

			CreateSnapshot(entry);
			AssertEquals(0u, invoiceLine1.KR_HighestGAApprovalSeqNo);
			AssertEquals(0u, invoiceLine2.KR_HighestGAApprovalSeqNo);
			genAddOnColumn1 = GetGenAddOnColumn(Factory, invoiceLine1.PK, JobComInvoiceLine.GenAddOnColumnConstants.KR_HighestGAApprovalSeqNo);
			genAddOnColumn2 = GetGenAddOnColumn(Factory, invoiceLine2.PK, JobComInvoiceLine.GenAddOnColumnConstants.KR_HighestGAApprovalSeqNo);
			AssertNull(genAddOnColumn1);
			AssertNull(genAddOnColumn2);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			AssertEquals(1u, invoiceLine1.KR_HighestGAApprovalSeqNo);
			AssertEquals(2u, invoiceLine2.KR_HighestGAApprovalSeqNo);
			genAddOnColumn1 = GetGenAddOnColumn(Factory, invoiceLine1.PK, JobComInvoiceLine.GenAddOnColumnConstants.KR_HighestGAApprovalSeqNo);
			genAddOnColumn2 = GetGenAddOnColumn(Factory, invoiceLine2.PK, JobComInvoiceLine.GenAddOnColumnConstants.KR_HighestGAApprovalSeqNo);
			AssertEquals(invoiceLine1.KR_HighestGAApprovalSeqNo.ToString(), genAddOnColumn1.XA_Data);
			AssertEquals(invoiceLine2.KR_HighestGAApprovalSeqNo.ToString(), genAddOnColumn2.XA_Data);

			invoiceLine1.GAApprovalDataCollection.AddNew().CSI_LineNo = 2;
			invoiceLine2.GAApprovalDataCollection.RemoveAndDelete(gaApprovalData2);
			CreateSnapshot(entry);
			AssertEquals(1u, invoiceLine1.KR_HighestGAApprovalSeqNo);
			AssertEquals(2u, invoiceLine2.KR_HighestGAApprovalSeqNo);
			genAddOnColumn1.Reload();
			genAddOnColumn2.Reload();
			AssertEquals(invoiceLine1.KR_HighestGAApprovalSeqNo.ToString(), genAddOnColumn1.XA_Data);
			AssertEquals(invoiceLine2.KR_HighestGAApprovalSeqNo.ToString(), genAddOnColumn2.XA_Data);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			AssertEquals(2u, invoiceLine1.KR_HighestGAApprovalSeqNo);
			AssertEquals(1u, invoiceLine2.KR_HighestGAApprovalSeqNo);
			genAddOnColumn1.Reload();
			genAddOnColumn2.Reload();
			AssertEquals(invoiceLine1.KR_HighestGAApprovalSeqNo.ToString(), genAddOnColumn1.XA_Data);
			AssertEquals(invoiceLine2.KR_HighestGAApprovalSeqNo.ToString(), genAddOnColumn2.XA_Data);

			invoiceLine1.GAApprovalDataCollection.RemoveAndDeleteAll();
			invoiceLine2.GAApprovalDataCollection.RemoveAndDeleteAll();
			CreateSnapshot(entry);
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			AssertEquals(0u, invoiceLine1.KR_HighestGAApprovalSeqNo);
			AssertEquals(0u, invoiceLine2.KR_HighestGAApprovalSeqNo);
			genAddOnColumn1.Reload();
			genAddOnColumn2.Reload();
			Assert(genAddOnColumn1.IsDeleted);
			Assert(genAddOnColumn2.IsDeleted);
		}

		public void TestKR_HighestVehicleSeqNo()
		{
			var entry = declaration.CustomsEntryHeaders[0];
			var invoiceLine1 = declaration.InvoiceLines[0];
			invoiceLine1.VehicleNumbers.AddNew().CY_Order = 1;
			var invoiceLine2 = declaration.InvoiceLines[1];
			var vehicleNumber1 = invoiceLine2.VehicleNumbers.AddNew();
			vehicleNumber1.CY_Order = 1;
			var vehicleNumber2 = invoiceLine2.VehicleNumbers.AddNew();
			vehicleNumber2.CY_Order = 2;

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			AssertEquals(0u, invoiceLine1.KR_HighestVehicleSeqNo);
			AssertEquals(0u, invoiceLine2.KR_HighestVehicleSeqNo);
			var genAddOnColumn1 = GetGenAddOnColumn(Factory, invoiceLine1.PK, JobComInvoiceLine.GenAddOnColumnConstants.KR_HighestVehicleSeqNo);
			var genAddOnColumn2 = GetGenAddOnColumn(Factory, invoiceLine2.PK, JobComInvoiceLine.GenAddOnColumnConstants.KR_HighestVehicleSeqNo);
			AssertNull(genAddOnColumn1);
			AssertNull(genAddOnColumn2);

			CreateSnapshot(entry);
			AssertEquals(0u, invoiceLine1.KR_HighestVehicleSeqNo);
			AssertEquals(0u, invoiceLine2.KR_HighestVehicleSeqNo);
			genAddOnColumn1 = GetGenAddOnColumn(Factory, invoiceLine1.PK, JobComInvoiceLine.GenAddOnColumnConstants.KR_HighestVehicleSeqNo);
			genAddOnColumn2 = GetGenAddOnColumn(Factory, invoiceLine2.PK, JobComInvoiceLine.GenAddOnColumnConstants.KR_HighestVehicleSeqNo);
			AssertNull(genAddOnColumn1);
			AssertNull(genAddOnColumn2);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			AssertEquals(1u, invoiceLine1.KR_HighestVehicleSeqNo);
			AssertEquals(2u, invoiceLine2.KR_HighestVehicleSeqNo);
			genAddOnColumn1 = GetGenAddOnColumn(Factory, invoiceLine1.PK, JobComInvoiceLine.GenAddOnColumnConstants.KR_HighestVehicleSeqNo);
			genAddOnColumn2 = GetGenAddOnColumn(Factory, invoiceLine2.PK, JobComInvoiceLine.GenAddOnColumnConstants.KR_HighestVehicleSeqNo);
			AssertEquals(invoiceLine1.KR_HighestVehicleSeqNo.ToString(), genAddOnColumn1.XA_Data);
			AssertEquals(invoiceLine2.KR_HighestVehicleSeqNo.ToString(), genAddOnColumn2.XA_Data);

			invoiceLine1.VehicleNumbers.AddNew().CY_Order = 2;
			invoiceLine2.VehicleNumbers.RemoveAndDelete(vehicleNumber2);

			CreateSnapshot(entry);
			AssertEquals(1u, invoiceLine1.KR_HighestVehicleSeqNo);
			AssertEquals(2u, invoiceLine2.KR_HighestVehicleSeqNo);
			genAddOnColumn1.Reload();
			genAddOnColumn2.Reload();
			AssertEquals(invoiceLine1.KR_HighestVehicleSeqNo.ToString(), genAddOnColumn1.XA_Data);
			AssertEquals(invoiceLine2.KR_HighestVehicleSeqNo.ToString(), genAddOnColumn2.XA_Data);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			AssertEquals(2u, invoiceLine1.KR_HighestVehicleSeqNo);
			AssertEquals(1u, invoiceLine2.KR_HighestVehicleSeqNo);
			genAddOnColumn1.Reload();
			genAddOnColumn2.Reload();
			AssertEquals(invoiceLine1.KR_HighestVehicleSeqNo.ToString(), genAddOnColumn1.XA_Data);
			AssertEquals(invoiceLine2.KR_HighestVehicleSeqNo.ToString(), genAddOnColumn2.XA_Data);

			invoiceLine1.VehicleNumbers.RemoveAndDeleteAll();
			invoiceLine2.VehicleNumbers.RemoveAndDeleteAll();
			CreateSnapshot(entry);
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			AssertEquals(0u, invoiceLine1.KR_HighestVehicleSeqNo);
			AssertEquals(0u, invoiceLine2.KR_HighestVehicleSeqNo);
			genAddOnColumn1.Reload();
			genAddOnColumn2.Reload();
			Assert(genAddOnColumn1.IsDeleted);
			Assert(genAddOnColumn2.IsDeleted);
		}
		GenAddOnColumn GetGenAddOnColumn(BusinessObjectFactory factory, ZGuid parentPK, ZString name)
		{
			var query = new ZQuery(GenAddOnColumnSchema.XA_ParentID, parentPK);
			query.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, JobComInvoiceLineSchema.Constants.Prefix);
			query.AddToFilter(GenAddOnColumnSchema.XA_Name, name);
			return factory.Load<GenAddOnColumn>(query).FirstOrDefault();
		}

		public void TestExcludeFromCloning()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.Invoices.AddNew();

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_PackType = "BG";
			var gaApproval1 = invoiceLine1.GAApprovalDataCollection.AddNew();
			var vehicleNumbers1 = invoiceLine1.VehicleNumbers.AddNew();
			var vehicleNumbers2 = invoiceLine1.VehicleNumbers.AddNew();
			AssertEquals(0u, gaApproval1.CSI_LineNo);
			AssertEquals(0u, vehicleNumbers1.CY_Order);
			AssertEquals(0u, vehicleNumbers2.CY_Order);

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_NoOfPacks = 3;
			var gaApproval2 = invoiceLine2.GAApprovalDataCollection.AddNew();
			var gaApproval3 = invoiceLine2.GAApprovalDataCollection.AddNew();
			var vehicleNumbers3 = invoiceLine2.VehicleNumbers.AddNew();
			AssertEquals(0u, gaApproval2.CSI_LineNo);
			AssertEquals(0u, gaApproval3.CSI_LineNo);
			AssertEquals(0u, vehicleNumbers3.CY_Order);

			AssertEquals(0u, invoiceLine1.KR_HighestGAApprovalSeqNo);
			AssertEquals(0u, invoiceLine2.KR_HighestGAApprovalSeqNo);
			AssertEquals(0u, invoiceLine1.KR_HighestVehicleSeqNo);
			AssertEquals(0u, invoiceLine2.KR_HighestVehicleSeqNo);

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1u, gaApproval1.CSI_LineNo);
			AssertEquals(1u, vehicleNumbers1.CY_Order);
			AssertEquals(2u, vehicleNumbers2.CY_Order);

			AssertEquals(1u, gaApproval2.CSI_LineNo);
			AssertEquals(2u, gaApproval3.CSI_LineNo);
			AssertEquals(1u, vehicleNumbers3.CY_Order);

			AssertEquals(0u, invoiceLine1.KR_HighestGAApprovalSeqNo);
			AssertEquals(0u, invoiceLine2.KR_HighestGAApprovalSeqNo);
			AssertEquals(0u, invoiceLine1.KR_HighestVehicleSeqNo);
			AssertEquals(0u, invoiceLine2.KR_HighestVehicleSeqNo);

			var entry = declaration.CustomsEntryHeaders[0];
			CreateSnapshot(entry);
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			AssertEquals(1u, invoiceLine1.KR_HighestGAApprovalSeqNo);
			AssertEquals(2u, invoiceLine1.KR_HighestVehicleSeqNo);
			AssertEquals(2u, invoiceLine2.KR_HighestGAApprovalSeqNo);
			AssertEquals(1u, invoiceLine2.KR_HighestVehicleSeqNo);
			Factory.Save();

			var clonedJobDeclaration = (JobDeclaration)declaration.TemplateCopy();
			var clonedInvoiceLine1 = clonedJobDeclaration.InvoiceLines[0];
			AssertEquals("BG", clonedInvoiceLine1.JI_PackType);
			AssertEquals(0u, clonedInvoiceLine1.KR_HighestGAApprovalSeqNo);
			AssertEquals(0u, clonedInvoiceLine1.KR_HighestVehicleSeqNo);
			var gaApproval4 = clonedInvoiceLine1.GAApprovalDataCollection.AddNew();
			var vehicleNumbers4 = clonedInvoiceLine1.VehicleNumbers.AddNew();
			AssertEquals(0u, gaApproval4.CSI_LineNo);
			AssertEquals(0u, vehicleNumbers4.CY_Order);

			var clonedInvoiceLine2 = clonedJobDeclaration.InvoiceLines[1];
			AssertEquals(3, clonedInvoiceLine2.JI_NoOfPacks);
			AssertEquals(0u, clonedInvoiceLine2.KR_HighestGAApprovalSeqNo);
			AssertEquals(0u, clonedInvoiceLine2.KR_HighestVehicleSeqNo);
			var gaApproval5 = clonedInvoiceLine2.GAApprovalDataCollection.AddNew();
			var vehicleNumbers5 = clonedInvoiceLine2.VehicleNumbers.AddNew();
			AssertEquals(0u, gaApproval5.CSI_LineNo);
			AssertEquals(0u, vehicleNumbers5.CY_Order);

			clonedJobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(2u, gaApproval4.CSI_LineNo);
			AssertEquals(3u, vehicleNumbers4.CY_Order);

			AssertEquals(3u, gaApproval5.CSI_LineNo);
			AssertEquals(2u, vehicleNumbers5.CY_Order);

			AssertEquals(0u, clonedInvoiceLine1.KR_HighestGAApprovalSeqNo);
			AssertEquals(0u, clonedInvoiceLine1.KR_HighestGAApprovalSeqNo);
			AssertEquals(0u, clonedInvoiceLine2.KR_HighestVehicleSeqNo);
			AssertEquals(0u, clonedInvoiceLine2.KR_HighestVehicleSeqNo);

			CreateSnapshot(clonedJobDeclaration.CustomsEntryHeaders[0]);
			clonedJobDeclaration.CustomsEntryHeaders[0].CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			AssertEquals(2u, clonedInvoiceLine1.KR_HighestGAApprovalSeqNo);
			AssertEquals(3u, clonedInvoiceLine1.KR_HighestVehicleSeqNo);
			AssertEquals(3u, clonedInvoiceLine2.KR_HighestGAApprovalSeqNo);
			AssertEquals(2u, clonedInvoiceLine2.KR_HighestVehicleSeqNo);
		}

		void CreateSnapshot(CusEntryHeader entry)
		{
			var header = new ExportEntryHeaderCreator().Create(entry);
			using (var stream = KRXmlObjectSerializer.Serialize(header))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._830, stream);
				Factory.Save();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.InvoiceLines.AddNew();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}
		JobDeclaration declaration;

		public void TestCustomsUnitPrice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, UniversalConstants.TariffTypes.HarmonizedSystem);
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "2402200000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateTariffAttribute(ZZ.TariffAttributes.InvoiceQuantityInCU1, YesNo.Yes, tariff);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");

			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "2402200000";
			invoiceLine1.JI_InvoiceQuantity = 10;
			invoiceLine1.JI_CustomsQuantity = 20;
			invoiceLine1.JI_LinePrice = 1000;

			var invoiceLine2 = invoice1.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "12345600000";
			invoiceLine2.JI_InvoiceQuantity = 10;
			invoiceLine2.JI_CustomsQuantity = 5;
			invoiceLine2.JI_LinePrice = 1000;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals(true, invoiceLine1.HasHSRequiringInvQuantityInCustomsUQ);
			AssertEquals(50m, invoiceLine1.CustomsUnitPrice);

			AssertEquals(false, invoiceLine2.HasHSRequiringInvQuantityInCustomsUQ);
			AssertEquals(200m, invoiceLine2.CustomsUnitPrice);
		}

		OrgSupplierPart SetupProduct()
		{
			var product = (OrgSupplierPart)OrgSupplierPart.New(Factory);
			product.OP_PartNum = "CCC";
			product.OP_Desc = "DESCRIPTION";
			product.OP_Brand = "BRAND";
			product.OP_Model = "MODEL";
			product.RelatedOrganisations.RemoveAndDeleteAll();
			return product;
		}

		CusClassPartPivot SetupPivot(string childType, ZGuid productPk)
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_OP = productPk;
			pivot.CI_ChildType = childType;
			pivot.CI_TariffNum = "1234567890";
			pivot.KRClassification.CKR_Ingredient = childType + " INGREDIENT";
			pivot.CI_RN_NKCountryOfOrigin = "KR";
			pivot.KRClassification.CKR_COOLabelLocation = "B";
			return pivot;
		}

		public void TestSetTariffEtcDataFromProductsPivotCore()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "999";
			var product = SetupProduct();
			product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var impPivot = SetupPivot(ClassificationTypeList.Codes.HTI, product.PK);
			var invImpLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invImpLine.JI_PartNo = product.OP_PartNum;

			CombineAssertions(ClassificationTypeList.Codes.HTI, () =>
			{
				AssertNotEquals("JI_Tariff", "1234567890", invImpLine.JI_Tariff);
				AssertNotEquals("JI_BrandName", "BRAND", invImpLine.JI_BrandName);
				AssertNotEquals("JI_Model", "MODEL", invImpLine.JI_Model);
				AssertNotEquals("JI_Ingredient ", "HTI INGREDIENT", invImpLine.JI_Ingredient);
				AssertNotEquals("JI_CountryOfOrigin", "KR", invImpLine.JI_CountryOfOrigin);
				AssertNotEquals("JI_COOLabelLocation", "B", invImpLine.JI_COOLabelLocation);
			});

			var expPivot = SetupPivot(ClassificationTypeList.Codes.HTE, product.PK);
			var expTariff = "329129312";
			expPivot.CI_TariffNum = expTariff;
			var invExpLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invExpLine.JI_PartNo = product.OP_PartNum;

			CombineAssertions(ClassificationTypeList.Codes.HTE, () =>
			{
				AssertEquals("JI_Tariff", expTariff, invExpLine.JI_Tariff);
				AssertEquals("JI_Description", "DESCRIPTION", invExpLine.JI_Description);
				AssertEquals("JI_BrandName", "BRAND", invExpLine.JI_BrandName);
				AssertEquals("JI_Model", "MODEL", invExpLine.JI_Model);
				AssertEquals("JI_Ingredient ", "HTE INGREDIENT", invExpLine.JI_Ingredient);
				AssertEquals("JI_CountryOfOrigin", "KR", invExpLine.JI_CountryOfOrigin);
				AssertEquals("JI_COOLabelLocation", "B", invExpLine.JI_COOLabelLocation);
			});
		}

		public void TestSetTariffApprovalDocument()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "999";
			var product = SetupProduct();

			product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ZZ.NKCodeType.ENGAR, "Export - Non Government Agency Approval Reason type");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			var testData1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.ENGAR, "69102", "마약류 관리에 관한 법률 제2조 제3호 마목 단서에 따른 신체적 또는 정신적 의존성을 야기하지 아니하는 제제", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeListAttribute(testData1.PK, "MandatoryDocWhenExempt", "향정신성의약품 제외인정 신청서");
			Factory.Save();

			var pivot = SetupPivot(ClassificationTypeList.Codes.HTI, product.PK);
			var approvalDocument = pivot.GAApprovalDataCollection.AddNew();
			approvalDocument.CSI_Procedure = "69";
			approvalDocument.CSI_SubType = RequirementTypeCodeList.Codes._1;
			approvalDocument.CSI_Status = "02";
			approvalDocument.CSI_Code = "A";
			approvalDocument.CSI_Description = "APPROVAL DOCUMENT DESCRIPTION";
			approvalDocument.CSI_AdditionalDescription = "APPROVAL DOCUMENT ADDITIONALDESCRIPTION";

			var invImpLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invImpLine.JI_PartNo = product.OP_PartNum;
			AssertEquals("HTI tariff GAApprovalDataCollection.Count", 0, invImpLine.GAApprovalDataCollection.Count);

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			var invExpLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invExpLine.JI_PartNo = product.OP_PartNum;

			CombineAssertions(ClassificationTypeList.Codes.HTE, () =>
			{
				AssertEquals("GAApprovalDataCollection.Count", 1, invExpLine.GAApprovalDataCollection.Count);
				AssertEquals("CSI_Procedure", "69", invExpLine.GAApprovalDataCollection[0].CSI_Procedure);
				AssertEquals("CSI_SubType", RequirementTypeCodeList.Codes._1, invExpLine.GAApprovalDataCollection[0].CSI_SubType);
				AssertEquals("CSI_Code", "A", invExpLine.GAApprovalDataCollection[0].CSI_Code);
				AssertEquals("CSI_Status", "02", invExpLine.GAApprovalDataCollection[0].CSI_Status);
				AssertEquals("CSI_Description", "APPROVAL DOCUMENT DESCRIPTION", invExpLine.GAApprovalDataCollection[0].CSI_Description);
				AssertEquals("CSI_AdditionalDescription ", "APPROVAL DOCUMENT ADDITIONALDESCRIPTION", invExpLine.GAApprovalDataCollection[0].CSI_AdditionalDescription);
				AssertEquals("향정신성의약품 제외인정 신청서", invExpLine.GAApprovalDataCollection[0].ExportNonGAMandatoryDocument);
			});

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var loadedExpLine = factory2.Load<JobComInvoiceLine>(invExpLine.PK);
			CombineAssertions(ClassificationTypeList.Codes.HTE, () =>
			{
				AssertEquals("GAApprovalDataCollection.Count", 1, loadedExpLine.GAApprovalDataCollection.Count);
				AssertEquals("CSI_Procedure", "69", loadedExpLine.GAApprovalDataCollection[0].CSI_Procedure);
				AssertEquals("CSI_SubType", RequirementTypeCodeList.Codes._1, loadedExpLine.GAApprovalDataCollection[0].CSI_SubType);
				AssertEquals("CSI_Code", "A", loadedExpLine.GAApprovalDataCollection[0].CSI_Code);
				AssertEquals("CSI_Status", "02", loadedExpLine.GAApprovalDataCollection[0].CSI_Status);
				AssertEquals("CSI_Description", "APPROVAL DOCUMENT DESCRIPTION", loadedExpLine.GAApprovalDataCollection[0].CSI_Description);
				AssertEquals("CSI_AdditionalDescription ", "APPROVAL DOCUMENT ADDITIONALDESCRIPTION", loadedExpLine.GAApprovalDataCollection[0].CSI_AdditionalDescription);
				AssertEquals("향정신성의약품 제외인정 신청서", loadedExpLine.GAApprovalDataCollection[0].ExportNonGAMandatoryDocument);
			});
		}

		public void TestMaxLength()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			AssertEquals(70, invoiceLine.JI_IngredientInfo.MaxLength);
			AssertEquals(50, invoiceLine.JI_SerialNumberInfo.MaxLength);
			AssertEquals(400, invoiceLine.JI_DescriptionInfo.MaxLength);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			AssertEquals(15, invoiceLine.JI_IngredientInfo.MaxLength);
			AssertEquals(30, invoiceLine.JI_SerialNumberInfo.MaxLength);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.ValuationDeclaration;
			AssertEquals(90, invoiceLine.JI_DescriptionInfo.MaxLength);
		}

		public override void TestMakeCustomsQuantityReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

			AssertEquals(true, invoiceLine.JI_CustomsQuantityInfo.ReadOnly);
			AssertEquals(true, invoiceLine.JI_CustomsUnitQtyInfo.ReadOnly);

			invoiceLine.JI_CustomsUnitQty = "U";

			AssertEquals(false, invoiceLine.JI_CustomsQuantityInfo.ReadOnly);
			AssertEquals(true, invoiceLine.JI_CustomsUnitQtyInfo.ReadOnly);
		}

		public override void TestCustomsQuantityIsReadonlyWhenUnitQtyEmpty()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

			AssertEquals(true, invoiceLine.JI_CustomsUnitQty.IsEmpty);
			AssertEquals(true, invoiceLine.JI_CustomsQuantityInfo.ReadOnly);
		}

		public void TestReadonlyOfCustomsSecondQuantity()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

			AssertEquals(true, invoiceLine.JI_CustomsSecondQuantityInfo.ReadOnly);
			AssertEquals(true, invoiceLine.JI_CustomsSecondUnitQtyInfo.ReadOnly);

			invoiceLine.JI_CustomsSecondUnitQty = "U";
			AssertEquals(false, invoiceLine.JI_CustomsSecondQuantityInfo.ReadOnly);
			AssertEquals(true, invoiceLine.JI_CustomsSecondUnitQtyInfo.ReadOnly);
		}

		public void TestRenewGAApprovalDataCollectionByTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var ogaCondition = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.KoreaSouth, "CTRL", ZZ.RefCusConditionType.OGA, "Other Government Agency Requirement Details");
			var simpledrawbackCondition = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.KoreaSouth, "CTRL", ZZ.RefCusConditionType.SimpleDrawback, "Simple drawback");
			var regulationNumber = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.KoreaSouth, ZZ.RefCusConditionValueType.OGARegulationNumber, "OGA Regulation Number");
			var documentName = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.KoreaSouth, ZZ.RefCusConditionValueType.OGADocumentName, "OGA Document Name");
			var simpledrawbackRateValueType = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.KoreaSouth, ZZ.RefCusConditionValueType.SimpleDrawbackRate, "Drawback Amount per 10,000 Korean Won");
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, UniversalConstants.TariffTypes.HarmonizedSystem);

			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0101211000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "5305003000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			TestRefConditionSetupHelper.AddRefCusCondition(tariff2, "71", "수출허가서", ogaCondition, regulationNumber, documentName, true, false);
			TestRefConditionSetupHelper.AddRefCusCondition(tariff2, simpledrawbackCondition, true, false, simpledrawbackRateValueType, "1000");

			var tariff3 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "9305101010", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			TestRefConditionSetupHelper.AddRefCusCondition(tariff3, "34", "수출허가서", ogaCondition, regulationNumber, documentName, true, false);
			TestRefConditionSetupHelper.AddRefCusCondition(tariff3, "55", "수출허가증", ogaCondition, regulationNumber, documentName, true, false);
			TestRefConditionSetupHelper.AddRefCusCondition(tariff3, simpledrawbackCondition, true, false, simpledrawbackRateValueType, "25000");

			var tariff4 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "4203301030", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(-1));
			TestRefConditionSetupHelper.AddRefCusCondition(tariff4, "71", "수출허가서", ogaCondition, regulationNumber, documentName, true, false);
			TestRefConditionSetupHelper.AddRefCusCondition(tariff4, simpledrawbackCondition, true, false, simpledrawbackRateValueType, "25000");

			var tariff5 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0101291000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			TestRefConditionSetupHelper.AddRefCusCondition(tariff5, "13", "검역증명서", ogaCondition, regulationNumber, documentName, false, true);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			AssertEquals(invoiceLine.GAApprovalDataCollection.Count, 0);

			CombineAssertions("If the message type is export and OGA data is present, the collection is populated.", () =>
			{
				invoiceLine.JI_Tariff = tariff1.ZZ1_TariffCode;
				AssertEquals("This HS has not OGA data.", 0, invoiceLine.GAApprovalDataCollection.Count);

				invoiceLine.JI_Tariff = tariff2.ZZ1_TariffCode;
				AssertEquals(1, invoiceLine.GAApprovalDataCollection.Count);
				AssertEquals("71", invoiceLine.GAApprovalDataCollection[0].CSI_Procedure);
				AssertEquals("수출허가서", invoiceLine.GAApprovalDataCollection[0].CSI_Description);

				invoiceLine.JI_Tariff = tariff3.ZZ1_TariffCode;
				AssertEquals(2, invoiceLine.GAApprovalDataCollection.Count);
				AssertEquals("34", invoiceLine.GAApprovalDataCollection[0].CSI_Procedure);
				AssertEquals("수출허가서", invoiceLine.GAApprovalDataCollection[0].CSI_Description);
				AssertEquals("55", invoiceLine.GAApprovalDataCollection[1].CSI_Procedure);
				AssertEquals("수출허가증", invoiceLine.GAApprovalDataCollection[1].CSI_Description);

				invoiceLine.JI_Tariff = tariff4.ZZ1_TariffCode;
				AssertEquals("This HS has not OGA valid data.", 0, invoiceLine.GAApprovalDataCollection.Count);

				invoiceLine.JI_Tariff = tariff5.ZZ1_TariffCode;
				AssertEquals("This HS has not EXP OGA data.", 0, invoiceLine.GAApprovalDataCollection.Count);

				declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
				invoiceLine.JI_Tariff = tariff2.ZZ1_TariffCode;
				AssertEquals("It is not imported because the message type is import.", 0, invoiceLine.GAApprovalDataCollection.Count);

				invoiceLine.JI_Tariff = tariff5.ZZ1_TariffCode;
				AssertEquals("It is not imported because the message type is import.", 0, invoiceLine.GAApprovalDataCollection.Count);
			});

			CombineAssertions("When the user enters data and the HS changes", () =>
			{
				declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
				SetGAApprovalData(invoiceLine, "71", "수출허가서", "Test value is not deleted.");
				AssertEquals(1, invoiceLine.GAApprovalDataCollection.Count);

				invoiceLine.JI_Tariff = tariff2.ZZ1_TariffCode;
				AssertEquals(1, invoiceLine.GAApprovalDataCollection.Count);
				AssertEquals("71", invoiceLine.GAApprovalDataCollection[0].CSI_Procedure);
				AssertEquals("수출허가서", invoiceLine.GAApprovalDataCollection[0].CSI_Description);
				AssertEquals("If there is a same procedure and description, nothing will work.", "Test value is not deleted.", invoiceLine.GAApprovalDataCollection[0].CSI_AdditionalDescription);

				SetGAApprovalData(invoiceLine, "34", "수출허가서", "Test value is not deleted.");
				AssertEquals(2, invoiceLine.GAApprovalDataCollection.Count);

				invoiceLine.JI_Tariff = tariff3.ZZ1_TariffCode;
				AssertEquals(2, invoiceLine.GAApprovalDataCollection.Count);
				AssertEquals("34", invoiceLine.GAApprovalDataCollection[0].CSI_Procedure);
				AssertEquals("수출허가서", invoiceLine.GAApprovalDataCollection[0].CSI_Description);
				AssertEquals("Test value is not deleted.", invoiceLine.GAApprovalDataCollection[0].CSI_AdditionalDescription);
				AssertEquals("55", invoiceLine.GAApprovalDataCollection[1].CSI_Procedure);
				AssertEquals("수출허가증", invoiceLine.GAApprovalDataCollection[1].CSI_Description);
				AssertEquals("", invoiceLine.GAApprovalDataCollection[1].CSI_AdditionalDescription);
			});
		}
		void SetGAApprovalData(JobComInvoiceLine invoiceLine, string procedure, string description, string additionalDescription)
		{
			var gAApproval = invoiceLine.GAApprovalDataCollection.AddNew();
			gAApproval.CSI_Procedure = procedure;
			gAApproval.CSI_Description = description;
			gAApproval.CSI_AdditionalDescription = additionalDescription;
		}
		public void TestIsPreapprovalMandatoryByTariff()
		{
			#region setup
			var invoiceLine = (JobComInvoiceLine)InvoiceLine;
			invoiceLine.Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			invoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(invoiceLine.Declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			invoiceLine.Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var exportEntryNum = invoiceLine.CusEntryLine.Header.EntryNumbers.AddNew();
			exportEntryNum.CE_EntryType = KRJobMessageTypeList.Codes.Export;
			invoiceLine.CusEntryLine.Header.CusEntryNumber.CE_IssueDate = ZDateTime.Today;

			var tariffHelper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = tariffHelper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, TariffTypes.HarmonizedSystem);
			var conditionHelper = new TestRefConditionSetupHelper(Factory);
			conditionHelper.Setup();
			#endregion
			tariffHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "7207190001", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			tariffHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0000000000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			var nomenclature = conditionHelper.GenerateNomenclature("7207", "12.34..56.7.8", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			var tradeGroup = conditionHelper.GenerateTradeGroup(Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			conditionHelper.GenerateTradeGroupCountry(tradeGroup, Core.Constants.CountryCodes.UnitedStates, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(1));
			conditionHelper.SetCondition(nomenclature, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			invoiceLine.JI_Tariff = "7207190001";
			invoiceLine.Declaration.JE_GoodsDestination = Core.Constants.CountryCodes.UnitedStates;
			Assert(invoiceLine.IsPreapprovalMandatory);

			invoiceLine.Declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Greece;
			Assert(!invoiceLine.IsPreapprovalMandatory);

			invoiceLine.JI_Tariff = "0000000000";
			invoiceLine.Declaration.JE_GoodsDestination = Core.Constants.CountryCodes.UnitedStates;
			Assert(!invoiceLine.IsPreapprovalMandatory);
		}

		public void TestIsPreapprovalMandatoryByEffectiveAssessmentDate()
		{
			#region setup
			var invoiceLine = (JobComInvoiceLine)InvoiceLine;
			invoiceLine.Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			invoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(invoiceLine.Declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			invoiceLine.Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var exportEntryNum = invoiceLine.CusEntryLine.Header.EntryNumbers.AddNew();
			exportEntryNum.CE_EntryType = KRJobMessageTypeList.Codes.Export;
			invoiceLine.CusEntryLine.Header.CusEntryNumber.CE_IssueDate = ZDateTime.Today;

			var tariffHelper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = tariffHelper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, TariffTypes.HarmonizedSystem);
			var conditionHelper = new TestRefConditionSetupHelper(Factory);
			conditionHelper.Setup();
			#endregion

			tariffHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "7207190001", ZDateTime.Today.AddDays(-30), ZDateTime.Today.AddDays(30));
			var nomenclature = conditionHelper.GenerateNomenclature("7207", "12.34..56.7.8", ZDateTime.Today.AddDays(-2), ZDateTime.MaxSmallDateTime);
			var tradeGroup = conditionHelper.GenerateTradeGroup(Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			conditionHelper.GenerateTradeGroupCountry(tradeGroup, Core.Constants.CountryCodes.UnitedStates, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(1));
			conditionHelper.SetCondition(nomenclature, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today.AddDays(-2), ZDateTime.MaxSmallDateTime);

			invoiceLine.JI_Tariff = "7207190001";
			invoiceLine.Declaration.JE_GoodsDestination = Core.Constants.CountryCodes.UnitedStates;
			Assert(invoiceLine.IsPreapprovalMandatory);

			invoiceLine.CusEntryLine.Header.CusEntryNumber.CE_IssueDate = ZDateTime.Today.AddDays(-10);
			Assert(!invoiceLine.IsPreapprovalMandatory);

			invoiceLine.CusEntryLine.Header.CusEntryNumber.CE_IssueDate = ZDateTime.Today.AddDays(10);
			Assert(!invoiceLine.IsPreapprovalMandatory);

			tradeGroup = conditionHelper.GenerateTradeGroup(Core.Constants.CountryCodes.Greece, ZDateTime.Today.AddDays(-30), ZDateTime.Today.AddDays(-10));
			conditionHelper.GenerateTradeGroupCountry(tradeGroup, Core.Constants.CountryCodes.Greece, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(1));
			conditionHelper.SetCondition(nomenclature, Core.Constants.CountryCodes.Greece, ZDateTime.Today.AddDays(-2), ZDateTime.MaxSmallDateTime);

			invoiceLine.Declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Greece;
			Assert(!invoiceLine.IsPreapprovalMandatory);

			tradeGroup = conditionHelper.GenerateTradeGroup(Core.Constants.CountryCodes.EuropeanUnion, ZDateTime.Today.AddDays(-30), ZDateTime.Today.AddDays(1));
			conditionHelper.GenerateTradeGroupCountry(tradeGroup, Core.Constants.CountryCodes.UnitedKingdom, ZDate.Today.AddDays(-30), ZDate.Today.AddDays(-10));
			conditionHelper.SetCondition(nomenclature, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today.AddDays(-2), ZDateTime.MaxSmallDateTime);

			invoiceLine.Declaration.JE_GoodsDestination = Core.Constants.CountryCodes.UnitedKingdom;
			Assert(!invoiceLine.IsPreapprovalMandatory);

			tariffHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "6344650002", ZDateTime.Today.AddDays(-30), ZDateTime.Today.AddDays(30));
			nomenclature = conditionHelper.GenerateNomenclature("634465", "12.63..44.6.5", ZDateTime.Today.AddDays(-30), ZDateTime.Today.AddDays(-10));
			conditionHelper.SetCondition(nomenclature, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today.AddDays(-2), ZDateTime.MaxSmallDateTime);

			invoiceLine.JI_Tariff = "6344650002";
			invoiceLine.CusEntryLine.Header.CusEntryNumber.CE_IssueDate = ZDateTime.Today;
			Assert(!invoiceLine.IsPreapprovalMandatory);
		}

		public void TestIsEligibleForSimpleDrawback()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, TariffTypes.HarmonizedSystem);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0202201000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0202301000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			var tariff3 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0303230000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			var condtionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.KoreaSouth, "CTRL", ZZ.RefCusConditionType.SimpleDrawback);
			helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.KoreaSouth, condtionType.PK, tariff1.PK, "Valid test value", false, true, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.KoreaSouth, condtionType.PK, tariff2.PK, "Invalid test value", false, true, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(-1));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_IssueDate = ZDateTime.Today;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine.PK;

			invoiceLine1.JI_Tariff = tariff3.ZZ1_TariffCode;
			AssertEquals(false, invoiceLine1.IsEligibleForSimpleDrawback);

			invoiceLine2.JI_Tariff = tariff2.ZZ1_TariffCode;
			AssertEquals(false, invoiceLine2.IsEligibleForSimpleDrawback);

			invoiceLine3.JI_Tariff = tariff1.ZZ1_TariffCode;
			AssertEquals(true, invoiceLine3.IsEligibleForSimpleDrawback);
		}

		public void TestRenewGAApprovalDataCollectionByProduct()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var refCusConditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.KoreaSouth, "CTRL", ZZ.RefCusConditionType.OGA, "Other Government Agency Requirement Details");
			var regulationNumber = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.KoreaSouth, ZZ.RefCusConditionValueType.OGARegulationNumber, "OGA Regulation Number");
			var documentName = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.KoreaSouth, ZZ.RefCusConditionValueType.OGADocumentName, "OGA Document Name");
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, UniversalConstants.TariffTypes.HarmonizedSystem);

			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "9305101010", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			TestRefConditionSetupHelper.AddRefCusCondition(tariff, "34", "수출허가서", refCusConditionType, regulationNumber, documentName, true, false);
			TestRefConditionSetupHelper.AddRefCusCondition(tariff, "55", "수출허가증", refCusConditionType, regulationNumber, documentName, true, false);
			Factory.Save();

			var emptySupplierPart = Factory.New<OrgSupplierPart>();
			emptySupplierPart.OP_PartNum = "Empty Product";
			OrgPartRelation emptyRelation = emptySupplierPart.RelatedOrganisations.AddNew();
			emptyRelation.OU_OH = Factory.New<OrgHeader>().PK;
			emptyRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			var emptyPivot = emptySupplierPart.PivotsForBinding.AddNew();
			emptyPivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals(0, emptyPivot.GAApprovalDataCollection.Count);

			var supplierPart = Factory.New<OrgSupplierPart>();
			supplierPart.OP_PartNum = "Product";
			OrgPartRelation relation = supplierPart.RelatedOrganisations.AddNew();
			relation.OU_OH = Factory.New<OrgHeader>().PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			var pivot = supplierPart.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot.CI_TariffNum = tariff.ZZ1_TariffCode;
			AssertEquals(2, pivot.GAApprovalDataCollection.Count);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(0, invoiceLine.GAApprovalDataCollection.Count);

			invoice.JZ_OH_Supplier = emptyRelation.OU_OH;
			invoiceLine.JI_PartNo = emptySupplierPart.OP_PartNum;
			AssertEquals(0, invoiceLine.GAApprovalDataCollection.Count);

			invoice.JZ_OH_Supplier = relation.OU_OH;
			invoiceLine.JI_PartNo = supplierPart.OP_PartNum;
			AssertEquals(2, invoiceLine.GAApprovalDataCollection.Count);
			AssertEquals("34", invoiceLine.GAApprovalDataCollection[0].CSI_Procedure);
			AssertEquals("수출허가서", invoiceLine.GAApprovalDataCollection[0].CSI_Description);
			AssertEquals("", invoiceLine.GAApprovalDataCollection[0].CSI_AdditionalDescription);
			AssertEquals("55", invoiceLine.GAApprovalDataCollection[1].CSI_Procedure);
			AssertEquals("수출허가증", invoiceLine.GAApprovalDataCollection[1].CSI_Description);
			AssertEquals("", invoiceLine.GAApprovalDataCollection[1].CSI_AdditionalDescription);

			invoiceLine.JI_PartNo = ZString.Empty;
			AssertEquals(2, invoiceLine.GAApprovalDataCollection.Count);

			SetGAApprovalData(invoiceLine, "13", "검역증명서", "It will be updated to an empty value.");
			SetGAApprovalData(invoiceLine, "34", "수출허가서", "It will be updated to an empty value.");
			AssertEquals(4, invoiceLine.GAApprovalDataCollection.Count);
			AssertEquals("34", invoiceLine.GAApprovalDataCollection[0].CSI_Procedure);
			AssertEquals("수출허가서", invoiceLine.GAApprovalDataCollection[0].CSI_Description);
			AssertEquals("", invoiceLine.GAApprovalDataCollection[0].CSI_AdditionalDescription);
			AssertEquals("55", invoiceLine.GAApprovalDataCollection[1].CSI_Procedure);
			AssertEquals("수출허가증", invoiceLine.GAApprovalDataCollection[1].CSI_Description);
			AssertEquals("", invoiceLine.GAApprovalDataCollection[1].CSI_AdditionalDescription);
			AssertEquals("13", invoiceLine.GAApprovalDataCollection[2].CSI_Procedure);
			AssertEquals("검역증명서", invoiceLine.GAApprovalDataCollection[2].CSI_Description);
			AssertEquals("It will be updated to an empty value.", invoiceLine.GAApprovalDataCollection[2].CSI_AdditionalDescription);
			AssertEquals("34", invoiceLine.GAApprovalDataCollection[3].CSI_Procedure);
			AssertEquals("수출허가서", invoiceLine.GAApprovalDataCollection[3].CSI_Description);
			AssertEquals("It will be updated to an empty value.", invoiceLine.GAApprovalDataCollection[3].CSI_AdditionalDescription);

			invoiceLine.JI_PartNo = supplierPart.OP_PartNum;
			AssertEquals(2, invoiceLine.GAApprovalDataCollection.Count);
			AssertEquals("34", invoiceLine.GAApprovalDataCollection[0].CSI_Procedure);
			AssertEquals("수출허가서", invoiceLine.GAApprovalDataCollection[0].CSI_Description);
			AssertEquals("", invoiceLine.GAApprovalDataCollection[0].CSI_AdditionalDescription);
			AssertEquals("55", invoiceLine.GAApprovalDataCollection[1].CSI_Procedure);
			AssertEquals("수출허가증", invoiceLine.GAApprovalDataCollection[1].CSI_Description);
			AssertEquals("", invoiceLine.GAApprovalDataCollection[1].CSI_AdditionalDescription);
		}

		public void TestJI_DutyReductionRateRegulationCode()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals(ZString.Empty, invoiceLine.JI_DutyReductionRateRegulationCode);
			AssertEquals(ZString.Empty, invoiceLine.DutyReductionGroupNumber);
			AssertEquals(ZString.Empty, invoiceLine.DutyReductionSeqNumber);
			AssertEquals(ZString.Empty, invoiceLine.DutyReductionItemNumber);

			invoiceLine.DutyReductionSeqNumber = "001";
			AssertEquals(":001:", invoiceLine.JI_DutyReductionRateRegulationCode);

			invoiceLine.DutyReductionItemNumber = "02";
			AssertEquals(":001:02", invoiceLine.JI_DutyReductionRateRegulationCode);

			invoiceLine.DutyReductionGroupNumber = "01";
			AssertEquals("01:001:02", invoiceLine.JI_DutyReductionRateRegulationCode);

			AssertEquals("01", invoiceLine.DutyReductionGroupNumber);
			AssertEquals("001", invoiceLine.DutyReductionSeqNumber);
			AssertEquals("02", invoiceLine.DutyReductionItemNumber);
		}

		public void TestItemsOfEntryLineFeeAndRate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.KoreaSouth, UniversalConstants.RateTypes.Duty);
			var dtyRateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", cusRateType.PK);
			var tariffTypeHSN = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, TariffTypes.HarmonizedSystem);
			var tariffTypeDRE = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, ZZ.TariffTypes.DutyReductionExemption);

			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffTypeHSN.PK, "1234500000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "관세법 제88조제1항제1호 해당물품");

			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffTypeDRE.PK, "0987654321", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "DutyReductionRate Tariff");
			var cusRateCode = helper.CreateCusRateCode(Factory, ZZ.RateCodes.DutyReductionRate, cusRateType.PK);
			helper.CreateRefCusRate(tariff2.PK, cusRateCode.PK, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "VFD * 0.8", null, "80", "");

			var ldcPreference = helper.CreatePreferenceForCountry("LDC", "LDC", Core.Constants.CountryCodes.KoreaSouth);
			var tradeGroup1 = helper.CreateTradeGroup(Core.Constants.CountryCodes.KoreaSouth, "LDC1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(tradeGroup1, Core.Constants.CountryCodes.Australia, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var tariff3 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffTypeHSN.PK, "1234567890", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "DutyRateTypeDescription Tariff");
			var aLDCRate = helper.CreateRate(tariff3, dtyRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.8", ldcPreference.PK, "80");
			helper.CreateCusApplicability(aLDCRate, tradeGroup1, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_SequenceNumber = 1;
			invoiceLine.JI_IsSpecificUseCode = false;
			invoiceLine.JI_PrimaryPreference = "LDC";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(invoiceLine.Declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			invoiceLine.Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			invoiceLine.JI_SecondaryPreference = "0987654321";
			AssertEquals(80m, invoiceLine.DutyReductionRate);

			invoiceLine.JI_Tariff = "1234500000";
			AssertNull(invoiceLine.UniversalDutyRate);
			AssertNullOrEmpty(invoiceLine.DutyRateCode);
			AssertNullOrEmpty(invoiceLine.DutyRateCodeDescription);

			invoiceLine.JI_Tariff = "1234567890";
			var rateCodeDTA = Factory.New<CusRefRateCodeView>();
			rateCodeDTA.ZY1_RateCode = ZZ.RateCodes.DutyAdValorem;
			invoiceLine.UniversalDutyRate.ZZ2_ZY1_RateCode = rateCodeDTA.PK;

			AssertEquals(DutyRateCodeList.Codes._1, invoiceLine.DutyRateCode);
			AssertEquals(DutyRateCodeList.Descriptions._1, invoiceLine.DutyRateCodeDescription);

			var rateCodeDTS = Factory.New<CusRefRateCodeView>();
			rateCodeDTS.ZY1_RateCode = ZZ.RateCodes.DutySpecific;
			invoiceLine.UniversalDutyRate.ZZ2_ZY1_RateCode = rateCodeDTS.PK;

			AssertEquals(DutyRateCodeList.Codes._3, invoiceLine.DutyRateCode);
			AssertEquals(DutyRateCodeList.Descriptions._3, invoiceLine.DutyRateCodeDescription);
		}

		public void TestDomesticTaxBaseQtyOrPrice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypeDMT = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, ZZ.TariffTypes.DomesticTaxRate);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffTypeDMT.PK, "123456-A", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffTypeDMT.PK, "411000-B", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariffUOM(tariff2, "CU1", DomesticTaxBaseQtyOrPriceCode.UQs.Unit);

			var tariff3 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffTypeDMT.PK, "234567-C", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariffUOM(tariff3, "CU1", DomesticTaxBaseQtyOrPriceCode.UQs.AlcoholContent);

			helper.CreateTariffAttribute(ZZ.TariffAttributes.TaxClassification1, ZZ.TariffAttributes.LiquorTax, tariff1);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_DomesticTaxCode = "123456-A";
			invoiceLine.JI_CustomsUnitQty = DomesticTaxBaseQtyOrPriceCode.UQs.AlcoholContent;
			invoiceLine.JI_CustomsQuantity = 10m;
			AssertEquals(DomesticTaxBaseQtyOrPriceCode.UQs.AlcoholContent, invoiceLine.JI_CustomsUnitQty);
			AssertEquals(10m, invoiceLine.DomesticTaxBaseQtyOrPrice);

			invoiceLine.JI_DomesticTaxCode = "411000-B";
			invoiceLine.JI_CustomsQuantity = 20m;
			AssertEquals(DomesticTaxBaseQtyOrPriceCode.UQs.Unit, invoiceLine.JI_CustomsUnitQty);
			AssertEquals(20m, invoiceLine.DomesticTaxBaseQtyOrPrice);

			invoiceLine.JI_DomesticTaxCode = "234567-C";
			invoiceLine.JI_CustomsUnitQty = DomesticTaxBaseQtyOrPriceCode.UQs.AlcoholContent;
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_CustomsThirdUnitQty = DomesticTaxBaseQtyOrPriceCode.UQs.Minutes;
			invoiceLine.JI_CustomsThirdQuantity = 30m;
			AssertEquals(10m, invoiceLine.DomesticTaxBaseQtyOrPrice);

			invoiceLine.JI_DomesticTaxCode = ZString.Empty;
			invoiceLine.JI_Tariff = DomesticTaxBaseQtyOrPriceCode.HSCodeInMinutes;
			invoiceLine.JI_CustomsThirdUnitQty = DomesticTaxBaseQtyOrPriceCode.UQs.Minutes;
			invoiceLine.JI_CustomsThirdQuantity = 30m;
			AssertEquals(30m, invoiceLine.DomesticTaxBaseQtyOrPrice);

			invoiceLine.JI_CustomsThirdUnitQty = DomesticTaxBaseQtyOrPriceCode.UQs.AlcoholContent;
			invoiceLine.JI_CustomsFourthUnitQty = DomesticTaxBaseQtyOrPriceCode.UQs.Minutes;
			invoiceLine.JI_CustomsFourthQuantity = 40m;
			AssertEquals(40m, invoiceLine.DomesticTaxBaseQtyOrPrice);

			invoiceLine.JI_CustomsUnitQty = DomesticTaxBaseQtyOrPriceCode.UQs.Minutes;
			invoiceLine.JI_CustomsQuantity = 50m;
			AssertEquals(50m, invoiceLine.DomesticTaxBaseQtyOrPrice);
		}

		public void TestPopulateCustomsUnitQty()
		{
			var dataGrouping = Core.Constants.CountryCodes.KoreaSouth;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypeHSN = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, TariffTypes.HarmonizedSystem);
			var tariffTypeDMT = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, ZZ.TariffTypes.DomesticTaxRate);
			var tariffTypeDTE = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, ZZ.TariffTypes.DomesticTaxReductionExemption);

			var tariffHSN = helper.LoadOrCreateNewTariff(dataGrouping, tariffTypeHSN.PK, "2402201000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Filter tip cigarettes");
			helper.CreateTariffUOM(tariffHSN, "CU1", "U");
			helper.CreateTariffUOM(tariffHSN, "CU2", "KG");

			var tariffDMT1 = helper.LoadOrCreateNewTariff(dataGrouping, tariffTypeDMT.PK, "803001-A", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "피우는 담배(제2종 파이프)");
			helper.CreateTariffUOM(tariffDMT1, "CU1", "U");

			var tariffDMT2 = helper.LoadOrCreateNewTariff(dataGrouping, tariffTypeDMT.PK, "803002-B", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "피우는 담배(제2종 파이프)");
			helper.CreateTariffUOM(tariffDMT2, "CU1", "GR");

			var tariffDMT3 = helper.LoadOrCreateNewTariff(dataGrouping, tariffTypeDMT.PK, "803005-C", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "피우는 담배(제2종 파이프)");
			helper.CreateTariffUOM(tariffDMT3, "CU1", "ML");
			Factory.Save();

			var tariffDTE = helper.LoadOrCreateNewTariff(dataGrouping, tariffTypeDTE.PK, "L180131", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "개별소비세법 18조 1항 3호가목의 물품");
			helper.CreateTariffUOM(tariffDTE, "CU1", "U");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_Tariff = "2402201000";
			AssertEquals("U", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("KG", invoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals(ZString.Empty, invoiceLine.JI_CustomsThirdUnitQty);
			AssertEquals(ZString.Empty, invoiceLine.JI_CustomsFourthUnitQty);

			invoiceLine.JI_DomesticTaxCode = "803001-A";
			AssertEquals("U", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("KG", invoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals(ZString.Empty, invoiceLine.JI_CustomsThirdUnitQty);
			AssertEquals(ZString.Empty, invoiceLine.JI_CustomsFourthUnitQty);

			invoiceLine.JI_DomesticTaxCode = "803002-B";
			AssertEquals("U", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("KG", invoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals("GR", invoiceLine.JI_CustomsThirdUnitQty);
			AssertEquals(ZString.Empty, invoiceLine.JI_CustomsFourthUnitQty);

			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_CustomsSecondQuantity = 2000m;
			invoiceLine.JI_CustomsThirdQuantity = 3000m;

			invoiceLine.JI_DomesticTaxCode = "803005-C";
			AssertEquals("U", invoiceLine.JI_CustomsUnitQty);
			AssertEquals(1000m, invoiceLine.JI_CustomsQuantity);
			AssertEquals("KG", invoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals(2000m, invoiceLine.JI_CustomsSecondQuantity);
			AssertEquals("ML", invoiceLine.JI_CustomsThirdUnitQty);
			AssertEquals(0m, invoiceLine.JI_CustomsThirdQuantity);
			AssertEquals(ZString.Empty, invoiceLine.JI_CustomsFourthUnitQty);

			invoiceLine.JI_CustomsThirdQuantity = 4000m;
			invoiceLine.JI_Tariff = "";
			AssertEquals("ML", invoiceLine.JI_CustomsUnitQty);
			AssertEquals(4000m, invoiceLine.JI_CustomsQuantity);
			AssertEquals(ZString.Empty, invoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals(0m, invoiceLine.JI_CustomsSecondQuantity);
			AssertEquals(ZString.Empty, invoiceLine.JI_CustomsThirdUnitQty);
			AssertEquals(0m, invoiceLine.JI_CustomsThirdQuantity);
			AssertEquals(ZString.Empty, invoiceLine.JI_CustomsFourthUnitQty);

			invoiceLine.JI_DomesticTaxExemptionCode = "L180131";
			AssertEquals("ML", invoiceLine.JI_CustomsUnitQty);
			AssertEquals(4000m, invoiceLine.JI_CustomsQuantity);
			AssertEquals("U", invoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals(0m, invoiceLine.JI_CustomsSecondQuantity);
			AssertEquals(ZString.Empty, invoiceLine.JI_CustomsThirdUnitQty);
			AssertEquals(0m, invoiceLine.JI_CustomsThirdQuantity);
			AssertEquals(ZString.Empty, invoiceLine.JI_CustomsFourthUnitQty);
		}

		public void TestAgricultureTaxClassification()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypeDRE = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, ZZ.TariffTypes.DutyReductionExemption);
			var tariffTypeDMT = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, ZZ.TariffTypes.DomesticTaxRate);

			var tariffAGTA1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffTypeDRE.PK, "123456-B", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariffAttribute(ZZ.TariffAttributes.AgricultureTaxAApplies, "A", tariffAGTA1);

			var tariffAGTB1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffTypeDMT.PK, "741258-A", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariffAttribute(ZZ.TariffAttributes.AgricultureTaxBApplies, "B", tariffAGTB1);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_SecondaryPreference = "123456-B";
			AssertEquals("A", invoiceLine.AgricultureTaxClassification);

			invoiceLine.JI_DomesticTaxCode = "741258-A";
			AssertEquals("C", invoiceLine.AgricultureTaxClassification);

			invoiceLine.JI_SecondaryPreference = ZString.Empty;
			AssertEquals("B", invoiceLine.AgricultureTaxClassification);

			invoiceLine.JI_DomesticTaxCode = ZString.Empty;
			AssertEquals(ZString.Empty, invoiceLine.AgricultureTaxClassification);
		}

		public void TestIssuedInThirdCountry()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_RN_NKSecondCommercialInvoiceCountry = ZString.Empty;
			AssertEquals(YesNoList.Codes.No, invoiceLine.IssuedInThirdCountry);

			invoiceLine.JI_RN_NKSecondCommercialInvoiceCountry = "US";
			AssertEquals(YesNoList.Codes.Yes, invoiceLine.IssuedInThirdCountry);
		}

		public void TestCountryOfOriginExporterNumber()
		{
			var invoiceLine_NotAttachedInvoiceHeader = Factory.New<JobComInvoiceLine>();
			Assert(invoiceLine_NotAttachedInvoiceHeader.CountryOfOriginExporterNumber.IsEmpty);

			var invoice = Factory.New<JobComInvoiceHeader>();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			AssertNull(invoice.Supplier);
			Assert(invoiceLine.CountryOfOriginExporterNumber.IsEmpty);

			var supplier = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "레디코리아");
			invoice.JZ_OH_Supplier = supplier.PK;
			AssertNotNull(invoice.Supplier);
			Assert(invoiceLine.CountryOfOriginExporterNumber.IsEmpty);

			var customsCode = supplier.CustomsCodes.AddNew();
			customsCode.OK_CodeType = IdentificationType.CertificateOfOriginExporterNumber;
			customsCode.OK_CustomsRegNo = "1234567890123456789012345";
			AssertEquals("1234567890123456789012345", invoiceLine.CountryOfOriginExporterNumber);
		}

		public void TestIsFTAPreference()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_PrimaryPreference = ZString.Empty;
			Assert(!invoiceLine.IsFTAPreference);

			invoiceLine.JI_PrimaryPreference = "A";
			Assert(!invoiceLine.IsFTAPreference);

			invoiceLine.JI_PrimaryPreference = "F";
			Assert(!invoiceLine.IsFTAPreference);

			invoiceLine.JI_PrimaryPreference = "F1";
			Assert(!invoiceLine.IsFTAPreference);

			invoiceLine.JI_PrimaryPreference = "FUS1";
			Assert(invoiceLine.IsFTAPreference);
		}

		public void TestJI_DutyRateSelection()
		{
			var referenceDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping_KR = referenceDataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth).ZZZ_DataGrouping;
			var dataGrouping_AU = referenceDataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Australia).ZZZ_DataGrouping;
			var tariffType = referenceDataHelper.CreateNewOrGetExistingTariffType(dataGrouping_KR, UniversalConstants.TariffTypes.HarmonizedSystem);
			Factory.Save();

			var tariff_0 = referenceDataHelper.CreateTariff(dataGrouping_KR, tariffType.PK, "1206101000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "우육");

			var tariff_1 = referenceDataHelper.CreateTariff(dataGrouping_KR, tariffType.PK, "2306101000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "치즈");
			var rateType = referenceDataHelper.CreateCusRateType(dataGrouping_KR, UniversalConstants.RateTypes.Duty);
			var rateCodeDTA = referenceDataHelper.CreateCusRateCode(Factory, ZZ.RateCodes.DutyAdValorem, rateType.PK);
			var rateCodeDTS = referenceDataHelper.CreateCusRateCode(Factory, ZZ.RateCodes.DutySpecific, rateType.PK);
			var tradeGroup = referenceDataHelper.CreateTradeGroup("AU", "AU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			referenceDataHelper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Australia);
			referenceDataHelper.CreateTariffUOM(tariff_1, "CU1", "KG");
			var preference_1 = referenceDataHelper.CreatePreferenceForCountry("A1", "기본세율(선택1)", Core.Constants.CountryCodes.KoreaSouth);
			var rateAdvalorem_1 = referenceDataHelper.CreateRate(tariff_1, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.055", dataGrouping: dataGrouping_KR, preferencePk: preference_1.PK);
			var rateSpecific_1 = referenceDataHelper.CreateRate(tariff_1, rateCodeDTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "[KG] * 1600", dataGrouping: dataGrouping_KR, preferencePk: preference_1.PK);
			referenceDataHelper.CreateCusApplicability(rateAdvalorem_1, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			referenceDataHelper.CreateCusApplicability(rateSpecific_1, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			referenceDataHelper.CreateCusApplicability(rateAdvalorem_1, null, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date, additionalCode: ZZ.ApplicabilityAdditionalCodes.Max);
			referenceDataHelper.CreateCusApplicability(rateSpecific_1, null, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date, additionalCode: ZZ.ApplicabilityAdditionalCodes.Max);

			var tariff_2 = referenceDataHelper.CreateTariff(dataGrouping_KR, tariffType.PK, "3706101000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "기타");
			referenceDataHelper.CreateTariffUOM(tariff_2, "CU1", "KG");
			var preference_2 = referenceDataHelper.CreatePreferenceForCountry("C2", "WTO협정세율(선택2)", Core.Constants.CountryCodes.KoreaSouth);
			var rateAdvalorem_2 = referenceDataHelper.CreateRate(tariff_2, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.065", dataGrouping: dataGrouping_KR, preferencePk: preference_2.PK);
			var rateSpecific_2 = referenceDataHelper.CreateRate(tariff_2, rateCodeDTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "[KG] * 1560", dataGrouping: dataGrouping_KR, preferencePk: preference_2.PK);
			referenceDataHelper.CreateCusApplicability(rateAdvalorem_2, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			referenceDataHelper.CreateCusApplicability(rateSpecific_2, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			referenceDataHelper.CreateCusApplicability(rateAdvalorem_2, null, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date, additionalCode: ZZ.ApplicabilityAdditionalCodes.Max);
			referenceDataHelper.CreateCusApplicability(rateAdvalorem_2, null, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date, additionalCode: ZZ.ApplicabilityAdditionalCodes.Min);
			referenceDataHelper.CreateCusApplicability(rateSpecific_2, null, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date, additionalCode: ZZ.ApplicabilityAdditionalCodes.Max);
			referenceDataHelper.CreateCusApplicability(rateSpecific_2, null, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date, additionalCode: ZZ.ApplicabilityAdditionalCodes.Min);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_Tariff = tariff_0.ZZ1_TariffCode;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(true, invoiceLine.JI_DutyRateSelectionInfo.ReadOnly);
			AssertEquals(ZString.Empty, invoiceLine.JI_DutyRateSelection);
			AssertEquals(0, invoiceLine.Lookups.DutyRateSelectionList.Count);

			invoiceLine.JI_Tariff = tariff_1.ZZ1_TariffCode;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(true, invoiceLine.JI_DutyRateSelectionInfo.ReadOnly);
			AssertEquals(ZString.Empty, invoiceLine.JI_DutyRateSelection);
			AssertEquals(0, invoiceLine.Lookups.DutyRateSelectionList.Count);

			invoiceLine.JI_PrimaryPreference = preference_1.ZZS_Preference;
			invoiceLine.JI_CountryOfOrigin = dataGrouping_AU;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(true, invoiceLine.JI_DutyRateSelectionInfo.ReadOnly);
			AssertEquals(ZZ.ApplicabilityAdditionalCodes.Max, invoiceLine.JI_DutyRateSelection);
			AssertEquals(1, invoiceLine.Lookups.DutyRateSelectionList.Count);

			invoiceLine.JI_PrimaryPreference = "XXX";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(true, invoiceLine.JI_DutyRateSelectionInfo.ReadOnly);
			AssertEquals(ZString.Empty, invoiceLine.JI_DutyRateSelection);
			AssertEquals(0, invoiceLine.Lookups.DutyRateSelectionList.Count);

			invoiceLine.JI_Tariff = tariff_2.ZZ1_TariffCode;
			invoiceLine.JI_PrimaryPreference = preference_2.ZZS_Preference;
			invoiceLine.JI_CountryOfOrigin = dataGrouping_AU;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(false, invoiceLine.JI_DutyRateSelectionInfo.ReadOnly);
			AssertEquals(ZString.Empty, invoiceLine.JI_DutyRateSelection);
			AssertEquals(2, invoiceLine.Lookups.DutyRateSelectionList.Count);

			invoiceLine.JI_CountryOfOrigin = "XX";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(true, invoiceLine.JI_DutyRateSelectionInfo.ReadOnly);
			AssertEquals(ZString.Empty, invoiceLine.JI_DutyRateSelection);
			AssertEquals(0, invoiceLine.Lookups.DutyRateSelectionList.Count);
		}

		public void TestInstallationCostControl()
		{
			SetInstallationCostTestData();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_DomesticTaxCode = "123456-A";
			invoiceLine.JI_InstallationCost = 1.12m;
			invoiceLine.JI_DomesticTaxExemptionCode = "D310201";
			invoiceLine.JI_CustomsUnitQty = DomesticTaxBaseQtyOrPriceCode.UQs.AlcoholContent;
			invoiceLine.JI_CustomsQuantity = 10m;
			AssertEquals(true, invoiceLine.JI_InstallationCostInfo.ReadOnly);
			Assert(invoiceLine.JI_InstallationCost.IsEmpty);
			AssertEquals(10m, invoiceLine.DomesticTaxBaseQtyOrPrice);

			invoiceLine.JI_InstallationCost = 12m;
			invoiceLine.JI_DomesticTaxExemptionCode = "L180131";
			AssertEquals(false, invoiceLine.JI_InstallationCostInfo.ReadOnly);
			AssertEquals(12m, invoiceLine.JI_InstallationCost);
			AssertEquals(12m, invoiceLine.DomesticTaxBaseQtyOrPrice);
		}

		public void TestLineOrProductIsValidationEnabled()
		{
			Assert(((ILineOrProduct)InvoiceLine).IsValidationEnabled);
		}

		public void TestJI_DomesticTaxCode()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_DomesticTaxCode = "123456-A";
			AssertEquals("123456", invoiceLine.DomesticTaxCode);
			AssertEquals("A", invoiceLine.DomesticTaxPreference);

			invoiceLine.JI_DomesticTaxCode = "123456-B";
			AssertEquals("123456", invoiceLine.DomesticTaxCode);
			AssertEquals("B", invoiceLine.DomesticTaxPreference);

			invoiceLine.JI_DomesticTaxCode = "987654-C";
			AssertEquals("987654", invoiceLine.DomesticTaxCode);
			AssertEquals("C", invoiceLine.DomesticTaxPreference);
		}

		public void TestJI_DomesticTaxExemptionCode()
		{
			SetInstallationCostTestData();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_DomesticTaxExemptionCode = "L180131";
			AssertEquals("Y", invoiceLine.DomesticTaxExemptionTariff.Attributes.FirstOrDefault(x => x.ZZ3_Name == Messaging.Constants.ZZ.TariffAttributeNames.InvolvesInstallationCost).ZZ3_Value);
			AssertEquals(false, invoiceLine.JI_InstallationCostInfo.ReadOnly);

			invoiceLine.JI_DomesticTaxExemptionCode = "D310201";
			AssertEquals("N", invoiceLine.DomesticTaxExemptionTariff.Attributes.FirstOrDefault(x => x.ZZ3_Name == Messaging.Constants.ZZ.TariffAttributeNames.InvolvesInstallationCost).ZZ3_Value);
			AssertEquals(true, invoiceLine.JI_InstallationCostInfo.ReadOnly);

			invoiceLine.JI_DomesticTaxExemptionCode = "E042001";
			AssertNull(invoiceLine.DomesticTaxExemptionTariff.Attributes.FirstOrDefault(x => x.ZZ3_Name == Messaging.Constants.ZZ.TariffAttributeNames.InvolvesInstallationCost));
			AssertEquals(true, invoiceLine.JI_InstallationCostInfo.ReadOnly);

			invoiceLine.JI_DomesticTaxExemptionCode = "T120104";
			AssertNull(invoiceLine.DomesticTaxExemptionTariff.Attributes.FirstOrDefault(x => x.ZZ3_Name == Messaging.Constants.ZZ.TariffAttributeNames.InvolvesInstallationCost));
			AssertEquals(true, invoiceLine.JI_InstallationCostInfo.ReadOnly);
		}

		public void TestIsSubjectTo5FN()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, ZZ.TariffTypes.DutyReductionExemption);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "A093000004", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "관세법 제93조제4호 해당물품");
			helper.CreateTariffAttribute(Customs.KR.Messaging.Constants.ZZ.TariffAttributes.IsDutyExempt, YesNo.Yes, tariff1);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "A1070001", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "관세법 제107조 제1항 해당물품");
			Factory.Save();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_SequenceNumber = 1;
			invoiceLine.JI_SpecificUseCodeDutyRatePermitNo = "A";
			AssertEquals("CKI_SpecificUseCodeDutyRatePermitNo is not empty", false, invoiceLine.IsSubjectTo5FN);

			invoiceLine.JI_SpecificUseCodeDutyRatePermitNo = "";
			invoiceLine.JI_IsSpecificUseCode = false;
			invoiceLine.JI_SecondaryPreference = "A093000004";
			invoiceLine.JI_InstallmentCode = "";

			AssertEquals(ImportDutyReductionClassificationList.Codes.DutyExemption, invoiceLine.DutyReductionClassificationCode);
			AssertEquals("DutyExemption is A", true, invoiceLine.IsSubjectTo5FN);

			invoiceLine.JI_SecondaryPreference = "A1070001";
			AssertEquals(ImportDutyReductionClassificationList.Codes.DutyReduction, invoiceLine.DutyReductionClassificationCode);
			AssertEquals("DutyExemption is B", true, invoiceLine.IsSubjectTo5FN);

			invoiceLine.JI_SecondaryPreference = "";
			invoiceLine.JI_InstallmentCode = "ABC";
			AssertEquals(ImportDutyReductionClassificationList.Codes.InstallmentPayment, invoiceLine.DutyReductionClassificationCode);
			AssertEquals("DutyExemption is C", true, invoiceLine.IsSubjectTo5FN);

			invoiceLine.JI_IsSpecificUseCode = true;
			invoiceLine.JI_InstallmentCode = "";
			AssertEquals(ImportDutyReductionClassificationList.Codes.SpecificUseCodeOnly, invoiceLine.DutyReductionClassificationCode);
			AssertEquals("DutyExemption is D", true, invoiceLine.IsSubjectTo5FN);

			invoiceLine.JI_InstallmentCode = "ABC";
			AssertEquals(ImportDutyReductionClassificationList.Codes.SpecificUseCodeAndDutyReductionOrInstallment, invoiceLine.DutyReductionClassificationCode);
			AssertEquals("DutyExemption is T", true, invoiceLine.IsSubjectTo5FN);

			invoiceLine.JI_SecondaryPreference = "A1070001";
			AssertEquals(ImportDutyReductionClassificationList.Codes.Invalid, invoiceLine.DutyReductionClassificationCode);
			AssertEquals("DutyExemption is X", false, invoiceLine.IsSubjectTo5FN);
		}

		public void TestProportionOfEntryLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.KoreaRepublicOf;
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 10000m;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.KoreaRepublicOf;
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 5000m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

			entryLine.Fees.GetOrAddFeeByFeeType(ChargeTypeList.Codes.SpecialConsumptionTax).CF_ChargeAmount = 300m;
			entryLine.Fees.GetOrAddFeeByFeeType(ChargeTypeList.Codes.TransportationTax).CF_ChargeAmount = 600m;
			entryLine.Fees.GetOrAddFeeByFeeType(ChargeTypeList.Codes.LiquorTax).CF_ChargeAmount = 900m;
			entryLine.Fees.GetOrAddFeeByFeeType(ChargeTypeList.Codes.EducationTax).CF_ChargeAmount = 1200m;
			entryLine.Fees.GetOrAddFeeByFeeType(ChargeTypeList.Codes.AgricultureTax).CF_ChargeAmount = 1500m;

			AssertEquals(10000m, invoiceLine1.JI_Calc_FOB_InLocalCurrency);
			AssertEquals(5000m, invoiceLine2.JI_Calc_FOB_InLocalCurrency);
			AssertEquals(15000m, entryLine.FOBInLocalCurrency.Amount);

			AssertNotNull("Invoice line 1 is linked to CusEntryLine", invoiceLine1.CusEntryLine);
			AssertNotNull("Invoice line 2 is linked to CusEntryLine", invoiceLine2.CusEntryLine);

			AssertEquals("Calculated SCT Amount for invoice line 1", 200m, invoiceLine1.JI_Calc_SpecialConsumptionTaxIncludingWHEstimate);
			AssertEquals("Calculated SCT Amount for invoice line 2", 100m, invoiceLine2.JI_Calc_SpecialConsumptionTaxIncludingWHEstimate);
			AssertEquals("Calculated TRT Amount for invoice line 1", 400m, invoiceLine1.JI_Calc_TransportationTaxIncludingWHEstimate);
			AssertEquals("Calculated TRT Amount for invoice line 2", 200m, invoiceLine2.JI_Calc_TransportationTaxIncludingWHEstimate);
			AssertEquals("Calculated LQT Amount for invoice line 1", 600m, invoiceLine1.JI_Calc_LiquorTaxIncludingWHEstimate);
			AssertEquals("Calculated LQT Amount for invoice line 2", 300m, invoiceLine2.JI_Calc_LiquorTaxIncludingWHEstimate);
			AssertEquals("Calculated EDT Amount for invoice line 1", 800m, invoiceLine1.JI_Calc_EducationTaxIncludingWHEstimate);
			AssertEquals("Calculated EDT Amount for invoice line 2", 400m, invoiceLine2.JI_Calc_EducationTaxIncludingWHEstimate);
			AssertEquals("Calculated AGT Amount for invoice line 1", 1000m, invoiceLine1.JI_Calc_AgricultureTaxIncludingWHEstimate);
			AssertEquals("Calculated AGT Amount for invoice line 2", 500m, invoiceLine2.JI_Calc_AgricultureTaxIncludingWHEstimate);
		}

		void SetInstallationCostTestData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypeDMT = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, ZZ.TariffTypes.DomesticTaxRate);
			var tariffTypeDTE = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, Messaging.Constants.ZZ.TariffTypes.DomesticTaxReductionExemption);

			var tariffDMT1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffTypeDMT.PK, "123456-A", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingTariffAttribute(ZZ.TariffAttributes.TaxClassification1, ZZ.TariffAttributes.LiquorTax, tariffDMT1);

			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffTypeDTE.PK, "L180131", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffTypeDTE.PK, "D310201", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var tariff3 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffTypeDTE.PK, "E042001", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var tariff4 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffTypeDTE.PK, "T120104", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			helper.CreateNewOrGetExistingTariffAttribute(Messaging.Constants.ZZ.TariffAttributeNames.TaxCode, Messaging.Constants.ZZ.TariffAttributes.SpecialConsumptionTax, tariff1);
			helper.CreateNewOrGetExistingTariffAttribute(Messaging.Constants.ZZ.TariffAttributeNames.InvolvesInstallationCost, YesNo.Yes, tariff1);

			helper.CreateNewOrGetExistingTariffAttribute(Messaging.Constants.ZZ.TariffAttributeNames.TaxCode, Messaging.Constants.ZZ.TariffAttributes.LiquorTax, tariff2);
			helper.CreateNewOrGetExistingTariffAttribute(Messaging.Constants.ZZ.TariffAttributeNames.InvolvesInstallationCost, YesNo.No, tariff2);

			helper.CreateNewOrGetExistingTariffAttribute(Messaging.Constants.ZZ.TariffAttributeNames.TaxCode, Messaging.Constants.ZZ.TariffAttributes.ValueAddedTax, tariff3);
			helper.CreateNewOrGetExistingTariffAttribute(Messaging.Constants.ZZ.TariffAttributeNames.TaxCode, Messaging.Constants.ZZ.TariffAttributes.TransportationTax, tariff4);
			Factory.Save();
		}

		public void TestOverseasInsuranceAndOverseasFreight()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;

				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
				invoice.JZ_InvoiceAmount = 10000m;
				invoice.JZ_IncoTerm = "FOB";

				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 10000m;

				invoice.Charges.RemoveAndDeleteAll();
				invoice.Charges.AddNew(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 10m, declaration.LocalCurrencyCode);
				invoice.Charges.AddNew(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 20m, declaration.LocalCurrencyCode);

				declaration.ResumeApportionment();
				AssertEquals("Line OverseasFreight amount", 10m, invoiceLine.JI_OverseasFreight.Amount);
				AssertEquals("Line OverseasInsurance amount", 20m, invoiceLine.JI_OverseasInsurance.Amount);

				declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

				invoice.Charges.RemoveAndDeleteAll();
				invoice.Charges.AddNew(ImportChargeMethodCodeList.Codes.A114, 10m, declaration.LocalCurrencyCode);
				invoice.Charges.AddNew(ImportChargeMethodCodeList.Codes.A116, 20m, declaration.LocalCurrencyCode);

				invoice.Charges.AddNew(ImportChargeMethodCodeList.Codes.B311, 30m, declaration.LocalCurrencyCode);
				invoice.Charges.AddNew(ImportChargeMethodCodeList.Codes.B313, 40m, declaration.LocalCurrencyCode);

				invoice.Charges.AddNew(ImportChargeMethodCodeList.Codes.B501, 50m, declaration.LocalCurrencyCode);
				invoice.Charges.AddNew(ImportChargeMethodCodeList.Codes.B503, 60m, declaration.LocalCurrencyCode);

				declaration.ResumeApportionment();
				invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;
				AssertEquals("Line OverseasFreight amount", 10m, invoiceLine.JI_OverseasFreight.Amount);
				AssertEquals("Line OverseasInsurance amount", 20m, invoiceLine.JI_OverseasInsurance.Amount);

				invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodTwo;
				AssertEquals("Line OverseasFreight amount", 30m, invoiceLine.JI_OverseasFreight.Amount);
				AssertEquals("Line OverseasInsurance amount", 40m, invoiceLine.JI_OverseasInsurance.Amount);

				invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodThree;
				AssertEquals("Line OverseasFreight amount", 30m, invoiceLine.JI_OverseasFreight.Amount);
				AssertEquals("Line OverseasInsurance amount", 40m, invoiceLine.JI_OverseasInsurance.Amount);

				invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodFourA;
				AssertEquals("Line OverseasFreight amount", 0m, invoiceLine.JI_OverseasFreight.Amount);
				AssertEquals("Line OverseasInsurance amount", 0m, invoiceLine.JI_OverseasInsurance.Amount);

				invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodFourB;
				AssertEquals("Line OverseasFreight amount", 0m, invoiceLine.JI_OverseasFreight.Amount);
				AssertEquals("Line OverseasInsurance amount", 0m, invoiceLine.JI_OverseasInsurance.Amount);

				invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodFive;
				AssertEquals("Line OverseasFreight amount", 50m, invoiceLine.JI_OverseasFreight.Amount);
				AssertEquals("Line OverseasInsurance amount", 60m, invoiceLine.JI_OverseasInsurance.Amount);

				invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodSix;
				AssertEquals("Line OverseasFreight amount", 50m, invoiceLine.JI_OverseasFreight.Amount);
				AssertEquals("Line OverseasInsurance amount", 60m, invoiceLine.JI_OverseasInsurance.Amount);
			}
		}

		public void TestICusCodeDataTypeSupporter()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			Integration.Customs.ICusCodeDataTypeSupporter supporter = invoiceLine;
			AssertEquals(3, supporter.GetCusCodeDataTypes().Count);
			Assert(supporter.GetCusCodeDataTypes().ContainsKey(CusCodeDataTypeList.Codes.VehicleNumber));
			Assert(supporter.GetCusCodeDataTypes().ContainsKey(CusCodeDataTypeList.Codes.ImmediateDelivery));
			Assert(supporter.GetCusCodeDataTypes().ContainsKey(CusCodeDataTypeList.Codes.HsExtensionCode));
		}

		public void TestHSExtensionCollection()
		{
			new TestDataSetupHelper(Factory).SetTariffAdditionalCodes();
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			AssertNull(invoiceLine.UniversalTariff);

			invoiceLine.JI_Tariff = "0301929090";
			AssertNotNull(invoiceLine.UniversalTariff);
			AssertEquals(1, invoiceLine.HSExtensionCodeCollection.Count);

			var mainAdditionalCode = invoiceLine.HSExtensionCodeCollection[0];
			AssertEquals(HsExtensionCodes.Code.Category, mainAdditionalCode.Category);
			AssertEquals(ZString.Empty, mainAdditionalCode.CY_Code);

			CombineAssertions("0301929090 has four categories(01, 02, 03, 04).", () =>
			{
				AssertHSExtensionCodeCollection("01", 3, 2);
				AssertHSExtensionCodeCollection("02", 3, 2);
				AssertHSExtensionCodeCollection("03", 3, 2);
				AssertHSExtensionCodeCollection("04", 2, 1);
			});

			invoiceLine.JI_Tariff = "0105949000";
			AssertNotNull(invoiceLine.UniversalTariff);
			AssertEquals(1, invoiceLine.HSExtensionCodeCollection.Count);

			mainAdditionalCode = invoiceLine.HSExtensionCodeCollection[0];
			CombineAssertions("0105949000 has one category(01).", () =>
			{
				AssertHSExtensionCodeCollection("01", 3, 2);
			});

			void AssertHSExtensionCodeCollection(string category, int collectionCount, int subCategoryCount)
			{
				mainAdditionalCode.CY_Code = category;
				AssertEquals(collectionCount, invoiceLine.HSExtensionCodeCollection.Count);

				var hsExtensionCodeCollection = invoiceLine.HSExtensionCodeCollection;
				AssertEquals(1, hsExtensionCodeCollection.Where(x => x.Category == HsExtensionCodes.Code.Category).Count());
				AssertEquals(subCategoryCount, hsExtensionCodeCollection.Where(x => x.Category == HsExtensionCodes.Code.SubCategory).Count());
			}
		}

		public void TestAdditionalTariffCode()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();

			var hsExtensionCode1 = invoiceLine.HSExtensionCodeCollection.AddNew();
			hsExtensionCode1.CY_Order = 0;
			hsExtensionCode1.CY_Code = "01";
			AssertEquals("01", invoiceLine.AdditionalTariffCode);

			var hsExtensionCode2 = invoiceLine.HSExtensionCodeCollection.AddNew();
			hsExtensionCode2.CY_Order = 1;
			hsExtensionCode2.CY_Code = "1N";
			AssertEquals("01-1N", invoiceLine.AdditionalTariffCode);

			var hsExtensionCode3 = invoiceLine.HSExtensionCodeCollection.AddNew();
			hsExtensionCode3.CY_Order = 2;
			hsExtensionCode3.CY_Code = "2L";
			AssertEquals("01-1N-2L", invoiceLine.AdditionalTariffCode);

			invoiceLine.HSExtensionCodeCollection.FirstOrDefault<HSExtensionCode>(x => x.ClassificationType == HsExtensionCodes.Description.Category).Delete();
			AssertNullOrEmpty(invoiceLine.AdditionalTariffCode);
		}
		public void TestDecialPlace()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();

			AssertHasCustomAttribute<DecimalPlacesAttribute>(invoiceLine.GetType(), "JI_Calc_SpecialConsumptionTaxIncludingWHEstimate", true, attrib => attrib.DecimalPlaces == 0);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(invoiceLine.GetType(), "JI_Calc_TransportationTaxIncludingWHEstimate", true, attrib => attrib.DecimalPlaces == 0);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(invoiceLine.GetType(), "JI_Calc_LiquorTaxIncludingWHEstimate", true, attrib => attrib.DecimalPlaces == 0);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(invoiceLine.GetType(), "JI_Calc_EducationTaxIncludingWHEstimate", true, attrib => attrib.DecimalPlaces == 0);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(invoiceLine.GetType(), "JI_Calc_AgricultureTaxIncludingWHEstimate", true, attrib => attrib.DecimalPlaces == 0);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(invoiceLine.GetType(), "JI_Calc_GSTVATAmountIncludingWHEstimate", true, attrib => attrib.DecimalPlaces == 0);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(invoiceLine.GetType(), "JI_Calc_DutyAmountIncludingWHEstimate", true, attrib => attrib.DecimalPlaces == 0);
		}
		#region Implementation

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			return invoiceLine;
		}

		protected override bool RatesAreReciprocal => true;
		#endregion

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceLineCharge>);
	}
}
