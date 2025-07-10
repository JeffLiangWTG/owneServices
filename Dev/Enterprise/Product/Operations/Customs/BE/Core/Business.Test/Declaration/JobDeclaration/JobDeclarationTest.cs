using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.BE.Business.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BE;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

[TestedType(typeof(JobDeclaration))]
sealed class JobDeclarationTest : EU.Business.Declaration.Testing.JobDeclarationAbstractTest<JobDeclaration>
{
	public void TestJE_DefermentAccountNumber()
	{
		const string importerDAN = "12345678";
		const string userDAN = "abcdef78901234567";
		var declaration = Factory.New<JobDeclaration>();
		declaration.SetImport();
		var testImporter = Factory.NewWithValidTestData<OrgHeader>();
		testImporter.OH_FullName = "Singapore Test Importer Pte. Ltd.";
		testImporter.MainAddress.OA_Address1 = "Changi Airport";
		testImporter.MainAddress.OA_Address2 = "Building 3C";
		testImporter.OH_Code = "TEST";
		testImporter.OH_RL_NKClosestPort = "FRPAR";
		testImporter.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, importerDAN, Core.Constants.CountryCodes.Belgium);

		CombineAssertions(() =>
		{
			AssertEquals("Approval Defer No.", DataBoundResourceStrings.GetDataForProperty(declaration.JE_DefermentAccountNumberInfo).Caption);

			AssertEquals("JE_DefermentAccountNumber = '' when Importer is null, JE_PaymentMethod = ''", ZString.Empty, declaration.JE_DefermentAccountNumber);

			declaration.JE_OH_Importer = testImporter.PK;
			AssertEquals("JE_DefermentAccountNumber = '' when Importer is not null, JE_PaymentMethod = ''", ZString.Empty, declaration.JE_DefermentAccountNumber);

			declaration.JE_PaymentMethod = PaymentMethodList.Codes.Deferral;
			AssertEquals("JE_DefermentAccountNumber = Importer's DAN when Importer is not null, JE_PaymentMethod = 'E'", importerDAN, declaration.JE_DefermentAccountNumber);

			declaration.JE_DefermentAccountNumber = userDAN;
			AssertEquals("JE_DefermentAccountNumber is manually updated", userDAN, declaration.JE_DefermentAccountNumber);
		});
	}

	public void TestJE_MessageSubType_Caption()
	{
		AssertEquals("JE_MessageSubType: Caption", "[1a] Entry Style", DataBoundResourceStrings.GetDataForProperty(typeof(JobDeclaration), nameof(JobDeclaration.JE_MessageSubType)).Caption);
	}

	public void TestJE_DeclarantType_Caption()
	{
		AssertEquals("JE_DeclarantType: Full Description", "[UCC 3/21] Rep. Type", DataBoundResourceStrings.GetDataForProperty(typeof(JobDeclaration), nameof(JobDeclaration.JE_DeclarantType)).FullDescription);
		AssertEquals("JE_DeclarantType: Caption", "Rep. Type", DataBoundResourceStrings.GetDataForProperty(typeof(JobDeclaration), nameof(JobDeclaration.JE_DeclarantType)).Caption);
	}

	public void TestJE_OA_SellerAddressLabel()
	{
		var declaration = Factory.New<JobDeclaration>();
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("IMP", "[UCC 3/24] Seller", declaration.JE_OA_SellerAddressLabel.Caption);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("EXP", "[2] Subcontractor", declaration.JE_OA_SellerAddressLabel.Caption);
		});
	}

	public void TestJE_OH_ControllingCustomerLabel()
	{
		AssertEquals("JE_ControllingCustomer: Caption", "Controlling Customer", DataBoundResourceStrings.GetDataForProperty(typeof(JobDeclaration), nameof(JobDeclaration.JE_OH_ControllingCustomer)).Caption);
	}

	public void TestJE_OA_ConsigneeAddress_Caption()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals("JE_OA_ConsigneeAddress: Caption", "[UCC 3/26] Buyer", declaration.JE_OA_ConsigneeAddressInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
	}

	public override void TestJE_TransportMode_Caption()
	{
		AssertCaptionAndFullDescription(Factory.New<JobDeclaration>().JE_TransportModeInfo, "Transport", "[UCC 7/4] Transport");
	}

	public void TestJE_ShipmentIncoTerm_Caption()
	{
		AssertEquals("JE_ShipmentIncoTerm: Caption", "[UCC 4/1] Incoterm", DataBoundResourceStrings.GetDataForProperty(typeof(JobDeclaration), nameof(JobDeclaration.JE_ShipmentIncoTerm)).Caption);
	}

	public void TestJE_UCR_Caption()
	{
		AssertEquals("JE_UCR: Caption", "[UCC 2/4] DUCR", DataBoundResourceStrings.GetDataForProperty(typeof(JobDeclaration), nameof(JobDeclaration.JE_UCR)).Caption);
	}

	public void TestJE_RN_NKTransportNationality_Caption()
	{
		AssertCaptionAndFullDescription(Factory.New<JobDeclaration>().JE_RN_NKTransportNationalityInfo, "Nationality", "[UCC 7/8] Nationality");
	}

	public void TestZG_SpecificCircumstanceIndicator_Caption()
	{
		AssertCaptionAndFullDescription(Factory.New<JobDeclaration>().ZG_SpecificCircumstanceIndicatorInfo, "Circumstance", "[UCC 1/7] Circumstance");
	}

	public void TestJE_VesselName_Caption()
	{
		AssertEquals("JE_VesselName: Caption", "[UCC 7/7] Vessel", DataBoundResourceStrings.GetDataForProperty(typeof(JobDeclaration), nameof(JobDeclaration.JE_VesselName)).Caption);
	}

	public void TestValidationType()
	{
		var declaration = Factory.New<JobDeclaration>();
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertType<ImportJobDeclarationValidation>("IMP", declaration.Validation);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertType<ExportJobDeclarationValidation>("EXP", declaration.Validation);

			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertType<JobDeclarationValidation>("MSC", declaration.Validation);
		});
	}

	public override void TestLocalCurrencyCoreOverride()
	{
		AssertEquals(Core.Constants.CurrencyCodes.Belgium, Factory.New<JobDeclaration>().LocalCurrencyCode);
	}

	public override void TestAreMultipleEntryInstructionsAllowed()
	{
		var declaration = Factory.New<JobDeclaration>();
		Assert(declaration.AreMultipleEntryInstructionsAllowed);
	}

	public void TestDV1DefaultingBuyerSeller()
	{
		var declaration = Factory.New<JobDeclaration>();

		OrgHeader testImporter = Factory.NewWithValidTestData<OrgHeader>();
		testImporter.OH_FullName = "Singapore Test Importer Pte. Ltd.";
		testImporter.MainAddress.OA_Address1 = "Changi Airport";
		testImporter.MainAddress.OA_Address2 = "Building 3C";
		testImporter.OH_Code = "TEST";
		testImporter.PrimaryRegistrationNumber.Number = string.Empty;

		var importerAddress = Factory.New<OrgAddress>();
		importerAddress.OA_OH = testImporter.PK;
		importerAddress.OA_Address1 = "Eugene Leroy Street ";
		importerAddress.CompanyName = "test Declarant";

		OrgHeader testSupplier = Factory.NewWithValidTestData<OrgHeader>();
		testSupplier.OH_FullName = "Singapore Test Importer Pte. Ltd.";
		testSupplier.MainAddress.OA_Address1 = "Changi Airport";
		testSupplier.MainAddress.OA_Address2 = "Building 3C";
		testSupplier.OH_Code = "TEST";

		var supplierAddress = Factory.New<OrgAddress>();
		supplierAddress.OA_OH = testSupplier.PK;
		supplierAddress.OA_Address1 = "Eugene Leroy Street ";
		supplierAddress.CompanyName = "test Declarant";

		declaration.ImporterDocumentaryAddress.E2_OA_Address = importerAddress.PK;
		declaration.SupplierDocumentaryAddress.E2_OA_Address = supplierAddress.PK;
		declaration.ZG_IsHighValueOvrd = true;

		Assert(!declaration.JE_OA_ConsigneeAddress.IsEmpty);
		Assert(!declaration.JE_OA_SellerAddress.IsEmpty);
	}

	public void TestIncoTermAndChargeFactoryType()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertType(typeof(IncoTermAndCustomsChargeFactory), declaration.IncoTermAndChargeFactory);
	}

	public override void TestGetCustomsEntryInstructionProviderCore()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertType(typeof(EntryInstructionProvider), declaration.CustomsEntryInstructionProvider);
	}

	public void TestCustomsEntryInstructions()
	{
		var dec = Factory.New<JobDeclaration>();
		AssertType<CusEntryInstructionCollection>(dec.CustomsEntryInstructions);
	}

	public void TestJE_LocationOfGoods()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals("Lookups.BELocationOfGoodsList", declaration.JE_LocationOfGoodsInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
	}

	public void TestJE_CustomsOffice()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals("Lookups.JE_CustomsOfficeList", declaration.JE_CustomsOfficeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
	}

	public override void TestCustomsOfficeRequirementHelper()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertType(typeof(JobDeclarationCustomsOfficeRequirementHelper), declaration.CustomsOfficeRequirementHelper);
	}

	public override void TestCustomsOfficeOfEntry()
	{
		using (ConfigurationTestHelper.TemporarilySetDeclarationConfiguration(Factory, "IsUCC6Core", false))
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.CustomsOffices.RemoveAndDeleteAll();
			declaration.JE_CustomsOffice = "LV001000";
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, "LV002000");
			AssertEquals("LV002000", declaration.OfficeOfEntry);
		}
	}

	public void TestIsInventorySelectionEnabled()
	{
		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType("BE", "BE");
		helper.CreateRefCusProcedure("BE", "", "40", "", "", "desc", "IMP", intoWarehouse: true, outOfWarehouse: true, group: "IMP");
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader = declaration.ActiveEntryHeaders.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var organisation = Factory.NewWithValidTestData<OrgHeader>();
		var organisationAddress = organisation.Addresses.AddNew();
		var cusEntryLine = entryHeader.MergedLines.AddNew();

		AssertEquals(false, declaration.IsOutwardBondedWarehousingEnabledForSingleOrMultipleEntry);
		AssertEquals(false, declaration.IsInventorySelectionEnabled);

		cusEntryLine.InvoiceLines.Add(invoiceLine);
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_JZ = invoiceHeader.PK;
		invoiceLine.JI_Procedure = "40";
		organisationAddress.OA_OH = organisation.PK;
		organisation.OH_IsWarehouseClient = true;
		organisation.CompanyData.OB_IMUsedBondedWhs = true;
		declaration.JE_MessageType = "IMP";
		entryInstruction.CEI_OA_Warehouse2 = organisationAddress.PK;

		AssertEquals(true, declaration.IsOutwardBondedWarehousingEnabledForSingleOrMultipleEntry);
		AssertEquals(true, declaration.IsInventorySelectionEnabled);
	}

	public void TestPresentationStartDate()
	{
		var resourceString = Factory.New<JobDeclaration>().ZG_PresentationStartDateInfo.GetAttribute<ResourceStringDataAttribute>();
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Presentation Date", resourceString.Caption);
			AssertEquals("Full Description", "[15 08 001 000] Date and Time of the presentation of the goods to customs", resourceString.FullDescription);
		});
	}

	public void TestJE_LocationQualifier()
	{
		const string validCodeNotDALocation = "BEANR433A";
		const string validCodeAndDALocation = "BEANR444A";
		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities");
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, validCodeNotDALocation, "433A 433A 2030 ANTWERPEN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, UniversalReferenceConstants.Type, UniversalReferenceConstants.Pakhuis);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, validCodeAndDALocation, "444A 444A 2030 ANTWERPEN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, UniversalReferenceConstants.Type, UniversalReferenceConstants.DALocatie);
		Factory.Save();

		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_TransportModeInland = UniversalReferenceConstants.TransportModeAir;
		jobDeclaration.Lookups.BELocationOfGoodsList.Reload(true);

		CombineAssertions(() =>
		{
			jobDeclaration.JE_LocationOfGoods = ZString.Empty;
			AssertEquals("LocationOfGoods is empty", ZString.Empty, jobDeclaration.JE_LocationQualifier);

			jobDeclaration.JE_LocationOfGoods = "ERR";
			AssertEquals("LocationOfGoods is invalid", ZString.Empty, jobDeclaration.JE_LocationQualifier);

			jobDeclaration.JE_LocationOfGoods = validCodeNotDALocation;
			AssertEquals("LocationOfGoods is valid but no D&A Location", Constants.LocationQualifiers.C, jobDeclaration.JE_LocationQualifier);

			jobDeclaration.JE_LocationOfGoods = validCodeAndDALocation;
			AssertEquals("LocationOfGoods is valid and a D&A Location", Constants.LocationQualifiers.A, jobDeclaration.JE_LocationQualifier);
		});
	}

	public void TestJE_MessageTypeChangedExportToImport()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.JZ_UCR = "ucr";
		invoiceHeader.ZG_TransportChargesMethodOfPayment = "C";

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		CombineAssertions(() =>
		{
			AssertEquals("InvoiceHeader JZ_UCR should be empty", ZString.Empty, invoiceHeader.JZ_UCR);
			AssertEquals("InvoiceHeader ZG_TransportChargesMethodOfPayment should be empty", ZString.Empty, invoiceHeader.ZG_TransportChargesMethodOfPayment);
		});
	}

	public void TestJE_MessageTypeChangedImportToExport()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var previousDocument = invoiceLine.PreviousDocuments.AddNew();
		previousDocument.CSI_Quantity2 = new ZDecimal(100);
		previousDocument.CSI_UnitOfQuantity2 = "1A";

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		CombineAssertions(() =>
		{
			AssertEquals("PreviousDocument CSI_Quantity2 should be 0", ZDecimal.Zero, previousDocument.CSI_Quantity2);
			AssertEquals("PreviousDocument CSI_UnitOfQuantity2 should be empty", ZString.Empty, previousDocument.CSI_UnitOfQuantity2);
		});
	}

	public void TestLookups()
	{
		var declaration = Factory.New<JobDeclaration>();
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertType<ImportJobDeclarationLookups>("IMP", declaration.Lookups);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertType<ExportJobDeclarationLookups>("EXP", declaration.Lookups);
			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertType<JobDeclarationLookups>("MSC", declaration.Lookups);
		});
	}

	public void TestFilteredInvoiceLines()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertType<EU.Business.Declaration.InvoiceLineViewCollection<JobComInvoiceLine>>(declaration.FilteredInvoiceLines);
	}

	protected override EUCommonConstants.TransportModeSource ExpectedTransportMeansDependencyForImport => EUCommonConstants.TransportModeSource.InlandTransportMode;
	protected override EUCommonConstants.TransportModeSource ExpectedTransportMeansDependencyForExport => EUCommonConstants.TransportModeSource.InlandTransportMode;
	protected override EUCommonConstants.TransportModeSource ExpectedTransportMeansDependencyForMiscellaneousCustoms => EUCommonConstants.TransportModeSource.InlandTransportMode;

	public void TestDefaultRegionOfDestinationWhenMessageTypeChanges()
	{
		var declaration = Factory.New<JobDeclaration>();
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_MessageType = BEJobMessageTypeList.Codes.Import;
			declaration.ZG_RegionOfDestination = "A";

			AssertEquals("When JE_MessageType is Import ZG_RegionOfDestination can be present", "A", declaration.ZG_RegionOfDestination);

			declaration.JE_MessageType = BEJobMessageTypeList.Codes.Export;

			AssertEquals("When JE_MessageType is no longer import ZG_RegionOfDestination should be empty", string.Empty, declaration.ZG_RegionOfDestination);
		}
	}

	public void TestIsExitSummary() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = BEJobMessageTypeList.Codes.ExitSummary;
		AssertEquals("IsExitSummary", true, declaration.IsExitSummary);
		declaration.JE_MessageType = ZString.Empty;
		AssertEquals("Empty", false, declaration.IsExitSummary);
	});

	public void TestIsReExport() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = BEJobMessageTypeList.Codes.ReExport;
		AssertEquals("IsReExport", true, declaration.IsReExport);
		declaration.JE_MessageType = ZString.Empty;
		AssertEquals("Empty", false, declaration.IsReExport);
	});

	public void TestIsExport() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = BEJobMessageTypeList.Codes.Export;
		AssertEquals("IsExport for Export", true, declaration.IsExport);
		declaration.JE_MessageType = BEJobMessageTypeList.Codes.ReExport;
		AssertEquals("IsExport for ReExport", true, declaration.IsExport);
		declaration.JE_MessageType = BEJobMessageTypeList.Codes.ExitSummary;
		AssertEquals("IsExport for ExitSummary", true, declaration.IsExport);
		declaration.JE_MessageType = ZString.Empty;
		AssertEquals("Empty", false, declaration.IsExport);
	});

	void AssertCaptionAndFullDescription(ZPropertyInfo propertyInfo, string expectedCaption, string expectedFullDescription, string messageType = "")
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(propertyInfo, null, [messageType == "IMP" ? JobDeclaration.CaptionKeyImportUCC6 : messageType == "EXP" ? JobDeclaration.CaptionKeyExportUCC6 : null]);

		CombineAssertions(() =>
		{
			AssertEquals($"{propertyInfo.Name} {messageType} Caption", expectedCaption, resourceStringData.Caption);
			AssertEquals($"{propertyInfo.Name} {messageType} FullDescription", expectedFullDescription, resourceStringData.FullDescription);
		});
	}

	protected override Type ExpectedDeclarationLevelPackageCollectionType => typeof(BaseDeclarationLevelPackageCollection<Package>);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.New<JobDeclaration>();

	protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

	protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest) => new LightValidationTesterExcludingJobDocAddress(bizObjToTest);
}

class LightValidationTesterExcludingJobDocAddress : LightValidationTester
{
	public LightValidationTesterExcludingJobDocAddress(BusinessObject bo) : base(bo)
	{
	}

	protected override bool ShouldTestProperty(ZPropertyInfo info)
	{
		if (info.BizObj.GetType() == typeof(JobDocAddress))
		{
			return false;
		}
		return base.ShouldTestProperty(info);
	}
}
