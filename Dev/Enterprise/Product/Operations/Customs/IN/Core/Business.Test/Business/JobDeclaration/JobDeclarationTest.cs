using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.Common.IN;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(JobDeclaration))]
sealed class JobDeclarationTest : BaseJobDeclarationTest<JobDeclaration>
{
	public new void TestPopulateCommercialInvoice()
	{
		var shipment = Factory.New<ForwardingShipment>();
		var line1 = shipment.OuterPackLines.AddNew();
		var line2 = shipment.OuterPackLines.AddNew();
		var line3 = shipment.OuterPackLines.AddNew();

		line2.JL_HarmonisedCode = "2022.22";
		line3.JL_HarmonisedCode = "3033.33";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_OH_Importer = Factory.New<OrgHeader>().PK;
		CreatePartAndClassification("AB0035X", "2605.00", declaration.Importer);

		line1.Products.PackProductManager.Value = "AB0035X";
		line3.Products.PackProductManager.Value = "H15-394885J9";

		((Freight.Integration.Forwarding.ICommercialInvoice)declaration).PopulateCommercialInvoice(shipment.OuterPackLines);

		CombineAssertions(() =>
		{
			AssertEquals("Has new header", 1, declaration.Invoices.Count);
			AssertEquals("Should have 3 lines", 3, declaration.Invoices[0].JobComInvoiceLines.Count);
			AssertEquals("260500", declaration.Invoices[0].JobComInvoiceLines[0].JI_Tariff.Replace(".", ""));
			AssertEquals("202222", declaration.Invoices[0].JobComInvoiceLines[1].JI_Tariff.Replace(".", ""));
			AssertEquals("303333", declaration.Invoices[0].JobComInvoiceLines[2].JI_Tariff.Replace(".", ""));
			AssertEquals("AB0035X", declaration.Invoices[0].JobComInvoiceLines[0].JI_PartNo);
			AssertEquals("", declaration.Invoices[0].JobComInvoiceLines[1].JI_PartNo);
			AssertEquals("H15-394885J9", declaration.Invoices[0].JobComInvoiceLines[2].JI_PartNo);
		});
	}

	public void TestMergeManager()
	{
		AssertEquals(typeof(MergeManager), Factory.New<JobDeclaration>().MergeManager.GetType());
	}

	public override void TestAreMultipleEntryInstructionsAllowed()
	{
		AssertEquals(false, Declaration.AreMultipleEntryInstructionsAllowed);
	}

	public void TestSupportsChcPivotBetweenInvoiceLineAndPacking()
	{
		AssertEquals(true, Declaration.SupportsChcPivotBetweenInvoiceLineAndPacking);
	}

	public void TestJE_RW_NKOriginState_Caption()
	{
		var originStateInfo = DataBoundResourceStrings.GetDataForProperty(Declaration.JE_RW_NKOriginStateInfo);
		AssertEquals("Origin State", originStateInfo.Caption);
		AssertEquals("State", originStateInfo.MediumCaption);
		AssertEquals("State", originStateInfo.ShortCaption);
	}

	public void TestJE_RW_NKOriginState_MaxLength()
	{
		AssertEquals("Max Length", 2, Declaration.JE_RW_NKOriginStateInfo.MaxLength);
	}

	public void TestExportIECCode()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var originStateInfo = DataBoundResourceStrings.GetDataForProperty(declaration.IECCodeInfo);

		var importer = Factory.New<OrgHeader>();
		declaration.JE_OH_Importer = importer.PK;
		var cusCode = importer.CustomsCodes.AddNew();
		cusCode.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.IEC;
		cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.India;
		cusCode.OK_CustomsRegNo = "1234567890";

		var supplier = Factory.New<OrgHeader>();
		declaration.JE_OH_Supplier = supplier.PK;
		var cusCode1 = supplier.CustomsCodes.AddNew();
		cusCode1.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.IEC;
		cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.India;
		cusCode1.OK_CustomsRegNo = "1234567891";
		CombineAssertions(() =>
		{
			AssertEquals("IEC Code", originStateInfo.Caption);
			AssertEquals("Readonly", true, declaration.IECCodeInfo.ReadOnly);
			AssertEquals("Import", "1234567890", declaration.IECCode);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Export", "1234567891", declaration.IECCode);

			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("Not Export and Import", ZString.Empty, declaration.IECCode);
		});
	}

	public void TestExporterClass_Attributes()
	{
		var exporterDescriptionClassInfo = DataBoundResourceStrings.GetDataForProperty(Declaration.ExporterClassDescriptionInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Exporter Class", exporterDescriptionClassInfo.Caption);
			AssertEquals("Exp. Class", exporterDescriptionClassInfo.MediumCaption);
			AssertEquals("Exp. Class", exporterDescriptionClassInfo.ShortCaption);

			Assert("Read Only", Declaration.ExporterClassDescriptionInfo.ReadOnly);
		});
	}

	public void TestExporterClass()
	{
		CombineAssertions(() =>
		{
			AssertEquals("No supplier", string.Empty, Declaration.ExporterClass);
			AssertExporterClassBySupplierCategory(OrgConstants.Category.Business, ExporterClassList.Codes.P);
			AssertExporterClassBySupplierCategory(OrgConstants.Category.NaturalPersonIndividual, ExporterClassList.Codes.P);
			AssertExporterClassBySupplierCategory(OrgConstants.Category.NonGovernmentOrganisation, ExporterClassList.Codes.P);
			AssertExporterClassBySupplierCategory(OrgConstants.Category.Government, ExporterClassList.Codes.G);
		});

		void AssertExporterClassBySupplierCategory(string category, string expectedExporterClass)
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Category = category;
			Declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals($"Supplier category is {category}", expectedExporterClass, Declaration.ExporterClass);
		}
	}

	public void TestExporterClassDescription()
	{
		CombineAssertions(() =>
		{
			AssertEquals("No supplier", ZString.Empty, Declaration.ExporterClassDescription);
			AssertExporterClassBySupplierCategory(OrgConstants.Category.Business);
			AssertExporterClassBySupplierCategory(OrgConstants.Category.NaturalPersonIndividual);
			AssertExporterClassBySupplierCategory(OrgConstants.Category.NonGovernmentOrganisation);
			AssertExporterClassBySupplierCategory(OrgConstants.Category.Government);
		});

		void AssertExporterClassBySupplierCategory(string category)
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Category = category;
			Declaration.JE_OH_Supplier = supplier.PK;
			var expectedExporterClassDescription = $"{Declaration.ExporterClass} - {Factory.GetCachedValue<ExporterClassList>().GetDescriptionFromCode(Declaration.ExporterClass)}";
			AssertEquals($"Supplier category is {category}", expectedExporterClassDescription, Declaration.ExporterClassDescription);
		}
	}

	public void TestAuthorizedDealerCodeCaption_Attributes()
	{
		var authorizedDealerCodeInfo = DataBoundResourceStrings.GetDataForProperty(Declaration.AuthorizedDealerCodeInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Authorized Dealer Code", authorizedDealerCodeInfo.Caption);
			AssertEquals("AD Code", authorizedDealerCodeInfo.MediumCaption);
			AssertEquals("AD Code", authorizedDealerCodeInfo.ShortCaption);

			Assert("Read Only", Declaration.AuthorizedDealerCodeInfo.ReadOnly);
		});
	}

	public void TestGetAuthorizedDealerCode()
	{
		CombineAssertions(() =>
		{
			AssertEquals("No Supplier", ZString.Empty, Declaration.AuthorizedDealerCode);

			var expectedAuthorizedDealerCode = "45435";
			var supplier = Factory.New<OrgHeader>();
			var address = supplier.Addresses.AddNew();
			var cusCode = supplier.CustomsCodes.AddNew(IndiaOrgCusCodeInfo.OrgCusCodes.ADC, expectedAuthorizedDealerCode, Core.Constants.CountryCodes.India);
			cusCode.OK_OA_PremisesAddress = address.PK;
			var supplierDocumentaryAddress = Declaration.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_OA_Address = address.PK;
			AssertEquals("Shipment type is Export", expectedAuthorizedDealerCode, Declaration.AuthorizedDealerCode);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Shipment type is Import", ZString.Empty, Declaration.AuthorizedDealerCode);
		});
	}

	public void TestTranshipperCode()
	{
		AssertEquals("No Transhipper Code", ZString.Empty, Declaration.TranshipperCode);

		var orgHeader = Factory.New<OrgHeader>();
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		orgHeader.CustomsCodes.AddNew(IndiaOrgCusCodeInfo.OrgCusCodes.PAN, "123", Core.Constants.CountryCodes.India);
		var transhipper = Declaration.TranshipperDocAddress;
		transhipper.OrganisationPK = orgHeader.PK;

		AssertEquals("With Transhipper Code", "123", Declaration.TranshipperCode);
	}

	public void TestJE_EPZCode_Attributes()
	{
		var epzCodeInfo = DataBoundResourceStrings.GetDataForProperty(Declaration.JE_EPZCodeInfo);
		CombineAssertions(() =>
		{
			AssertEquals("EPZ Code", epzCodeInfo.Caption);
			AssertEquals("EPZ Code", epzCodeInfo.MediumCaption);
			AssertEquals("EPZ", epzCodeInfo.ShortCaption);

			AssertEquals("Max Length", 1, Declaration.JE_EPZCodeInfo.MaxLength);
		});
	}

	public void TestBranchSerialNumber_Caption()
	{
		var branchSerialNumberInfo = DataBoundResourceStrings.GetDataForProperty(Declaration.BranchSerialNumberInfo);

		CombineAssertions(() =>
		{
			AssertEquals("Branch Serial Number", branchSerialNumberInfo.Caption);
			AssertEquals("Branch Sr. No.", branchSerialNumberInfo.MediumCaption);
			AssertEquals("Br. Sr.", branchSerialNumberInfo.ShortCaption);

			Assert("Read Only", Declaration.BranchSerialNumberInfo.ReadOnly);
		});
	}

	public void TestCommissionerate_Captions()
	{
		CaptionTestHelper.AssertCaptions(Declaration.JE_CommissionerateInfo, "Commissionerate", "Comm.", "Comm.");
	}

	public void TestDivision_Captions()
	{
		CaptionTestHelper.AssertCaptions(Declaration.JE_DivisionInfo, "Division", "Division", "Div.");
	}

	public void TestRange_Captions()
	{
		CaptionTestHelper.AssertCaptions(Declaration.JE_RangeInfo, "Range", "Range", "Range");
	}

	public void TestSealNo_Captions()
	{
		CaptionTestHelper.AssertCaptions(Declaration.JE_SealNoInfo, "Seal No", "Seal No", "Seal No");
	}

	public void TestVerified_Captions()
	{
		CaptionTestHelper.AssertCaptions(Declaration.JE_VerifiedInfo, "Verified?", "Verified?", "Verified?");
	}

	public void TestSampleForwarded_Captions()
	{
		CaptionTestHelper.AssertCaptions(Declaration.JE_SampleForwardedInfo, "Sample forwarded", "Sample fwd.", "Sample fwd.");
	}

	public void TestBranchSerialNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("No Supplier", ZString.Empty, Declaration.BranchSerialNumber);

			ZString expectedBranchSerialNumber = "111";
			var supplier = Factory.New<OrgHeader>();
			var supplierAddress = supplier.Addresses.AddNew();
			supplier.CustomsCodes.AddNew(IndiaOrgCusCodeInfo.OrgCusCodes.BSN, "111", countryCode: Core.Constants.CountryCodes.India)
				.OK_OA_PremisesAddress = supplierAddress.PK;

			Declaration.SupplierDocumentaryAddress.E2_OA_Address = supplier.MainAddress.PK;
			AssertEquals("Supplier Address has no BSN number", ZString.Empty, Declaration.BranchSerialNumber);
			Declaration.SupplierDocumentaryAddress.E2_OA_Address = supplierAddress.PK;
			AssertEquals("Supplier Address has BSN number", expectedBranchSerialNumber, Declaration.BranchSerialNumber);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("No Importer", ZString.Empty, Declaration.BranchSerialNumber);

			expectedBranchSerialNumber = "222";
			var importer = Factory.New<OrgHeader>();
			var importerAddress = importer.Addresses.AddNew();
			importer.CustomsCodes.AddNew(IndiaOrgCusCodeInfo.OrgCusCodes.BSN, expectedBranchSerialNumber, countryCode: Core.Constants.CountryCodes.India)
				.OK_OA_PremisesAddress = importerAddress.PK;

			Declaration.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;
			AssertEquals("Importer Address has no BSN number", ZString.Empty, Declaration.BranchSerialNumber);
			Declaration.ImporterDocumentaryAddress.E2_OA_Address = importerAddress.PK;
			AssertEquals("Importer Address has BSN number", expectedBranchSerialNumber, Declaration.BranchSerialNumber);
		});
	}

	public void TestJE_ExporterType_Attributes()
	{
		var exporterTypeInfo = DataBoundResourceStrings.GetDataForProperty(Declaration.JE_ExporterTypeInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Type Of Exporter", exporterTypeInfo.Caption);
			AssertEquals("Exporter Ty.", exporterTypeInfo.MediumCaption);
			AssertEquals("Exp Ty.", exporterTypeInfo.ShortCaption);

			AssertEquals("Max Length", 1, Declaration.JE_ExporterTypeInfo.MaxLength);
		});
	}

	public void TestJE_ExporterType_Default()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		Declaration.JE_ExporterType = "A";

		GetSupplier(out var supplier1);
		INOrgImpAddInfo.Get(supplier1).ZO_TypeOfExporter = ZString.Empty;
		Declaration.JE_OH_Supplier = supplier1.PK;
		AssertEquals("Organisation with TypeOfExporter empty", "A", Declaration.JE_ExporterType);

		GetSupplier(out var supplier2);
		INOrgImpAddInfo.Get(supplier2).ZO_TypeOfExporter = ExporterTypeList.Codes.MfgExporter;
		Declaration.JE_OH_Supplier = supplier2.PK;
		AssertEquals("Organisation with TypeOfExporter - MfgExporter", ExporterTypeList.Codes.MfgExporter, Declaration.JE_ExporterType);

		void GetSupplier(out OrgHeader supplier)
		{
			supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Category = OrgConstants.Category.Government;
			supplier.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.India;
		}
	}

	public override void TestLocalCurrencyCoreOverride()
	{
		AssertEquals("LocalCurrencyCode", Core.Constants.CurrencyCodes.India, GetJobDeclaration().LocalCurrencyCode);
	}

	public void TestHouseBillsCollectionIsOfRightType()
	{
		AssertType<BillCollection<Bill, JobDeclaration>>(Declaration.Bills);
	}

	public void TestLookupObjectIsCached()
	{
		var firstLookup = Declaration.Lookups;
		var secondLookup = Declaration.Lookups;
		AssertEquals(secondLookup, firstLookup);
	}

	public void TestFilteredInvoiceLines()
	{
		AssertType<InvoiceLineViewCollection<JobComInvoiceLine>>(Declaration.FilteredInvoiceLines);
	}

	public void TestGetCustomsEntryInstructionProviderCore()
	{
		AssertType(typeof(EntryInstructionProvider), Declaration.CustomsEntryInstructionProvider);
	}

	public override void TestIsDeclarationWithEntryInstruction()
	{
		Assert("IN Declaration should support EntryInstructions", !Declaration.CustomsEntryInstructionProvider.IsNoEntryInstruction);
	}

	public void TestIsContainerised()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.Containerised;
			AssertEquals("Containerised", expected: true, Declaration.IsContainerised);
			Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.ContainerisedAndPackaged;
			AssertEquals("Containerised", expected: true, Declaration.IsContainerised);
			Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.BreakBulk;
			AssertEquals("Not Containerised", expected: false, Declaration.IsContainerised);
			Declaration.JE_ContainerMode = ZString.Empty;
			AssertEquals("Not Containerised", expected: false, Declaration.IsContainerised);
		});
	}

	public void TestSupplierDocumentaryAddressChanged()
	{
		var header1 = Factory.New<OrgHeader>();
		var header2 = Factory.New<OrgHeader>();
		var supplierDocumentaryAddress = Declaration.SupplierDocumentaryAddress;
		AssertEquals(ZGuid.Empty, Declaration.JE_OH_Supplier);
		supplierDocumentaryAddress.OrganisationPK = header1.PK;
		AssertEquals(header1.PK, Declaration.JE_OH_Supplier);
		supplierDocumentaryAddress.OrganisationPK = header2.PK;
		AssertEquals(header2.PK, Declaration.JE_OH_Supplier);
	}

	public void TestSupplierDocumentaryAddressChanged_SetValueToEOUAddress()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.Containerised;
			var header = Factory.New<OrgHeader>();
			var supplierDocumentaryAddress = Declaration.SupplierDocumentaryAddress;
			AssertEquals(ZGuid.Empty, Declaration.ExportOrientedUnitsDocAddress.E2_OA_Address);

			Declaration.ExportOrientedUnitsDocAddress.E2_OA_Address = new ZGuid();
			Declaration.JE_StuffingAt = StuffingAtList.Codes.FAC;
			supplierDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, Declaration.ExportOrientedUnitsDocAddress.E2_OA_Address);

			supplierDocumentaryAddress.OrganisationPK = header.PK;
			AssertEquals(header.MainAddress.PK, Declaration.ExportOrientedUnitsDocAddress.E2_OA_Address);
		});
	}

	public void TestFlushSupplierDocumentaryAddressIfBlank()
	{
		var org = Factory.New<OrgHeader>();
		var shipment = Factory.New<ForwardingShipment>();
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		Declaration.JE_JS = shipment.PK;
		Declaration.JE_OH_Supplier = org.PK;
		AssertNoExceptionThrown(() => Declaration.SupplierDocumentaryAddress.E2_AddressOverride = true);
	}

	public void TestImporterDocumentaryAddressChanged()
	{
		var header1 = Factory.New<OrgHeader>();
		var header2 = Factory.New<OrgHeader>();
		var importerDocumentaryAddress = Declaration.ImporterDocumentaryAddress;
		AssertEquals(ZGuid.Empty, Declaration.JE_OH_Importer);
		importerDocumentaryAddress.OrganisationPK = header1.PK;
		AssertEquals(header1.PK, Declaration.JE_OH_Importer);
		importerDocumentaryAddress.OrganisationPK = header2.PK;
		AssertEquals(header2.PK, Declaration.JE_OH_Importer);
	}

	public void TestFlushImporterDocumentaryAddressIfBlank()
	{
		var org = Factory.New<OrgHeader>();
		var shipment = Factory.New<ForwardingShipment>();
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		Declaration.JE_JS = shipment.PK;
		Declaration.JE_OH_Importer = org.PK;
		AssertNoExceptionThrown(() => Declaration.ImporterDocumentaryAddress.E2_AddressOverride = true);
	}

	public void TestJE_CustomsOffice()
	{
		var customsOfficeResourceStringData = DataBoundResourceStrings.GetDataForProperty(Declaration.JE_CustomsOfficeInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Customs House", customsOfficeResourceStringData.Caption);
			AssertEquals("Cus. House.", customsOfficeResourceStringData.MediumCaption);
			AssertEquals("Cus. House.", customsOfficeResourceStringData.ShortCaption);
			AssertEquals("Max Length", 6, declaration.JE_CustomsOfficeInfo.MaxLength);
		});
	}

	public void TestValidation() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		AssertType<ImportJobDeclarationValidation>("ImportJobDeclarationValidation", declaration.Validation);
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		AssertType<ExportJobDeclarationValidation>("ExportJobDeclarationValidation", declaration.Validation);
		declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
		AssertType<JobDeclarationValidation>("JobDeclarationValidation", declaration.Validation);
	});

	public void TestDischargeCountryCode()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_RL_NKPortOfArrival = ZString.Empty;
		CombineAssertions(() =>
		{
			AssertEquals(ZString.Empty, declaration.DischargeCountryCode);
			declaration.JE_RL_NKPortOfArrival = "USLAX";
			AssertEquals("US", declaration.DischargeCountryCode);
		});
	}

	public void TestJE_MergeBy()
	{
		AssertEquals(true, Declaration.JE_MergeByInfo.ReadOnly);
	}

	public void TestJE_CustomsLocationCode()
	{
		var customsLocationResourceStringData = DataBoundResourceStrings.GetDataForProperty(Declaration.JE_CustomsLoadPortInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Customs Location", customsLocationResourceStringData.Caption);
			AssertEquals("CUS. Location", customsLocationResourceStringData.MediumCaption);
			AssertEquals("CUS. Loc.", customsLocationResourceStringData.ShortCaption);
			AssertEquals("Max Length", 6, declaration.JE_CustomsLoadPortInfo.MaxLength);
		});
	}

	public void TestJE_CustomsLocationCode_ForDefault()
	{
		RefDataSetupTestHelper.SetupCustomsLocationeData(Factory);

		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("When TransportMode Sea and IsExport is false, CustomsLoadPort should remain empty", ZString.Empty, Declaration.JE_CustomsLoadPort);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_CustomsOffice = "PQR123";
			AssertEquals("When TransportMode empty, CustomsLoadPort is empty and CustomsOffice is set", ZString.Empty, Declaration.JE_CustomsLoadPort);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			Declaration.JE_CustomsOffice = "PQR123";
			AssertEquals("When TransportMode Road, CustomsLoadPort is empty and CustomsOffice is set", ZString.Empty, Declaration.JE_CustomsLoadPort);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Declaration.JE_CustomsOffice = "ABC123";
			AssertEquals("When TransportMode Sea, CustomsLoadPort is empty and CustomsOffice is set", "ABC123", Declaration.JE_CustomsLoadPort);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			Declaration.JE_CustomsLoadPort = ZString.Empty;
			Declaration.JE_CustomsOffice = "GHI123";
			AssertEquals("When TransportMode Air, CustomsLoadPort is empty and CustomsOffice is set", "GHI123", Declaration.JE_CustomsLoadPort);

			Declaration.JE_CustomsLoadPort = "ABC123";
			Declaration.JE_CustomsOffice = "PQR123";
			AssertEquals("When CustomsLoadPort is not empty and CustomsOffice is set", "ABC123", Declaration.JE_CustomsLoadPort);

			Declaration.JE_CustomsLoadPort = "ABC123";
			Declaration.JE_CustomsOffice = ZString.Empty;
			AssertEquals("Clearing CustomsOffice should not affect CustomsLoadPort value.", "ABC123", Declaration.JE_CustomsLoadPort);
		});
	}

	public void TestJE_SealByCode_Attributes()
	{
		var sealByCodeInfo = DataBoundResourceStrings.GetDataForProperty(Declaration.JE_SealByInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Seal By", sealByCodeInfo.Caption);
			AssertEquals("Seal By", sealByCodeInfo.MediumCaption);
			AssertEquals("Seal By", sealByCodeInfo.ShortCaption);

			AssertEquals("Max Length", 1, Declaration.JE_SealByInfo.MaxLength);
		});
	}

	public void TestExportOrientedUnitsDocAddress()
	{
		var declaration = Factory.New<JobDeclaration>();
		var exportOrientedUnitsDocAddress = declaration.ExportOrientedUnitsDocAddress;

		CombineAssertions(() =>
		{
			AssertNotNull("docAddresses.GetDocAddress(DocAddressType.ExportOrientedUnitsDocAddress)", exportOrientedUnitsDocAddress);

			AssertEquals(DocAddressType.CustomsExportOrientedUnitsAddress, exportOrientedUnitsDocAddress.DocAddressType);
			AssertEquals(ContactType.NoContactType, exportOrientedUnitsDocAddress.DefaultContactType);
			AssertEquals("EOU", exportOrientedUnitsDocAddress.E2_AddressType);
			AssertEquals("JE", exportOrientedUnitsDocAddress.E2_ParentTableCode);
			AssertEquals(false, exportOrientedUnitsDocAddress.CanOverride);
			AssertEquals(declaration.PK, exportOrientedUnitsDocAddress.E2_ParentID);
		});
	}

	public void TestJE_ExaminationDate_Attributes()
	{
		var examinationDateInfo = DataBoundResourceStrings.GetDataForProperty(Declaration.JE_ExaminationDateInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Examination Date", examinationDateInfo.Caption);
			AssertEquals("Exam. Date", examinationDateInfo.MediumCaption);
			AssertEquals("Exam. Dt.", examinationDateInfo.ShortCaption);
		});
	}

	public void TestJE_ExaminingOfficerDesignation_Attributes()
	{
		var examiningOfficerDesignationInfo = DataBoundResourceStrings.GetDataForProperty(Declaration.JE_ExaminingOfficerDesignationInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Examining Officer Designation", examiningOfficerDesignationInfo.Caption);
			AssertEquals("Officer Designation", examiningOfficerDesignationInfo.MediumCaption);
			AssertEquals("Designation", examiningOfficerDesignationInfo.ShortCaption);
		});
	}

	public void TestJE_ExaminingOfficerName_Attributes()
	{
		var examiningOfficerNameInfo = DataBoundResourceStrings.GetDataForProperty(Declaration.JE_ExaminingOfficerNameInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Examining Officer Name", examiningOfficerNameInfo.Caption);
			AssertEquals("Exam. Officer Name", examiningOfficerNameInfo.MediumCaption);
			AssertEquals("Exam. Off. Name", examiningOfficerNameInfo.ShortCaption);
		});
	}

	public void TestJE_SupervisingOfficerDesignation_Attributes()
	{
		var supervisingOfficerDesignationInfo = DataBoundResourceStrings.GetDataForProperty(Declaration.JE_SupervisingOfficerDesignationInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Supervising Officer Designation", supervisingOfficerDesignationInfo.Caption);
			AssertEquals("Sup. Officer Designation", supervisingOfficerDesignationInfo.MediumCaption);
			AssertEquals("Sup. Designation", supervisingOfficerDesignationInfo.ShortCaption);
		});
	}

	public void TestJE_SupervisingOfficerName_Attributes()
	{
		var supervisingOfficerNameInfo = DataBoundResourceStrings.GetDataForProperty(Declaration.JE_SupervisingOfficerNameInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Supervising Officer Name", supervisingOfficerNameInfo.Caption);
			AssertEquals("Sup. Officer Name", supervisingOfficerNameInfo.MediumCaption);
			AssertEquals("Sup. Off. Name", supervisingOfficerNameInfo.ShortCaption);
		});
	}

	public void TestTranshipperDocAddress()
	{
		AssertEquals(DocAddressType.Transhipper, Declaration.TranshipperDocAddress.DocAddressType);
		AssertEquals(false, Declaration.TranshipperDocAddress.CanOverride);
	}

	public void TestImporterExporterCodeOfExportOrientedUnit()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.Containerised;
			Declaration.JE_StuffingAt = StuffingAtList.Codes.FAC;
			AssertEquals("No Supplier", ZString.Empty, Declaration.ImporterExporterCodeOfExportOrientedUnit);

			var expectedCode = "123";
			var supplier = Factory.New<OrgHeader>();
			var address = supplier.Addresses.AddNew();
			AssertEquals("No IEC Customs Codes", ZString.Empty, Declaration.ImporterExporterCodeOfExportOrientedUnit);

			var cusCode = supplier.CustomsCodes.AddNew(IndiaOrgCusCodeInfo.OrgCusCodes.IEC, expectedCode, Core.Constants.CountryCodes.India);
			cusCode.OK_OA_PremisesAddress = address.PK;
			var supplierDocumentaryAddress = Declaration.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_OA_Address = address.PK;
			AssertEquals(expectedCode, Declaration.ImporterExporterCodeOfExportOrientedUnit);
		});
	}

	public void TestBranchSrNumberOfImporterExporter()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.Containerised;
			Declaration.JE_StuffingAt = StuffingAtList.Codes.FAC;
			AssertEquals("No Supplier", ZString.Empty, Declaration.BranchSrNumberOfImporterExporter);

			var expectedBranch = "INC";
			var supplier = Factory.New<OrgHeader>();
			var address = supplier.Addresses.AddNew();
			AssertEquals("No BSN Customs Codes", ZString.Empty, Declaration.BranchSrNumberOfImporterExporter);

			var cusCode = supplier.CustomsCodes.AddNew(IndiaOrgCusCodeInfo.OrgCusCodes.BSN, expectedBranch, Core.Constants.CountryCodes.India);
			cusCode.OK_OA_PremisesAddress = address.PK;
			var supplierDocumentaryAddress = Declaration.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_OA_Address = address.PK;
			AssertEquals(expectedBranch, Declaration.BranchSrNumberOfImporterExporter);
		});
	}

	public void TestGetOrgHeaderList_CustomsExportOrientedUnitsAddress()
	{
		var declaration = Factory.New<JobDeclaration>();
		var iDocAddresses = (IDocAddresses)declaration;
		AssertSame(declaration.Lookups.ExportOrientedUnitsCollection, iDocAddresses.GetOrgHeaderList(DocAddressType.CustomsExportOrientedUnitsAddress));
	}

	public void TestJE_RotationNumber_Attributes()
	{
		var rotationNumberInfo = DataBoundResourceStrings.GetDataForProperty(Declaration.JE_RotationNumberInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Rotation Number", rotationNumberInfo.Caption);
			AssertEquals("Rotation No.", rotationNumberInfo.MediumCaption);
			AssertEquals("Rotation No.", rotationNumberInfo.ShortCaption);

			AssertEquals("Max Length", 7, Declaration.JE_RotationNumberInfo.MaxLength);
		});
	}

	public void TestIsFactoryStuffed()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_StuffingAt = StuffingAtList.Codes.FAC;
			AssertEquals("Factory Stuffed", true, Declaration.IsFactoryStuffed);
			Declaration.JE_StuffingAt = StuffingAtList.Codes.CFS;
			AssertEquals("Not Factory Stuffed", false, Declaration.IsFactoryStuffed);
		});
	}

	public void TestIsSeaAndContainerised()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Not Sea", false, Declaration.IsSeaAndContainerised);
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Declaration.JE_ContainerMode = INContainerModeList.Codes.Liquid;
			AssertEquals("Not Containerised", false, Declaration.IsSeaAndContainerised);
			Declaration.JE_ContainerMode = INContainerModeList.Codes.Containerised;
			AssertEquals("Sea and Containerised", true, Declaration.IsSeaAndContainerised);
		});
	}

	public void TestJE_StuffingAt_Attributes()
	{
		var stuffingAtInfo = DataBoundResourceStrings.GetDataForProperty(Declaration.JE_StuffingAtInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Stuffing At", stuffingAtInfo.Caption);
			AssertEquals("Stuffing At", stuffingAtInfo.MediumCaption);
			AssertEquals("Stuffing", stuffingAtInfo.ShortCaption);
		});
	}

	public void TestJE_SampleAccompanied_Attributes()
	{
		var sampleAccompaniedInfo = DataBoundResourceStrings.GetDataForProperty(Declaration.JE_SampleAccompaniedInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Sample Accompanied", sampleAccompaniedInfo.Caption);
			AssertEquals("Sample Accompanied", sampleAccompaniedInfo.MediumCaption);
			AssertEquals("Sample Accompanied", sampleAccompaniedInfo.ShortCaption);
		});
	}

	public void TestJE_RotationDate_Attributes()
	{
		var rotationDateInfo = DataBoundResourceStrings.GetDataForProperty(Declaration.JE_RotationDateInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Rotation Date", rotationDateInfo.Caption);
			AssertEquals("Rotation Dt.", rotationDateInfo.MediumCaption);
			AssertEquals("Date", rotationDateInfo.ShortCaption);
		});
	}

	public void TestCEI_TotalContainer_OnSaving()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
		Declaration.JE_ContainerMode = INContainerModeList.Codes.ContainerisedAndPackaged;
		Instruction.CEI_TotalContainer = 10;
		AssertEquals("When Containerised and transportmode is sea", 10, Instruction.CEI_TotalContainer);

		Declaration.JE_ContainerMode = INContainerModeList.Codes.Liquid;
		Declaration.OnSaving();
		AssertEquals("When container mode is liquid", 0, Instruction.CEI_TotalContainer);

		Declaration.JE_ContainerMode = INContainerModeList.Codes.Containerised;
		Instruction.CEI_TotalContainer = 10;
		Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
		Declaration.OnSaving();
		AssertEquals("When transportmode is changed to air", 0, Instruction.CEI_TotalContainer);

		Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
		Declaration.JE_ContainerMode = INContainerModeList.Codes.Containerised;
		Instruction.CEI_TotalContainer = 10;
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		Declaration.OnSaving();
		AssertEquals("When message type is import", 0, Instruction.CEI_TotalContainer);
	}

	public void TestCEI_LoosePackages_OnSaving()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
		Declaration.JE_ContainerMode = INContainerModeList.Codes.ContainerisedAndPackaged;
		Instruction.CEI_LoosePackages = 222;
		AssertEquals("When Containerised and transportmode is sea", 222, Instruction.CEI_LoosePackages);

		Declaration.JE_ContainerMode = INContainerModeList.Codes.Liquid;
		Declaration.OnSaving();
		AssertEquals("When container mode is liquid", 0, Instruction.CEI_LoosePackages);

		Declaration.JE_ContainerMode = INContainerModeList.Codes.Containerised;
		Instruction.CEI_LoosePackages = 10;
		Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
		Declaration.OnSaving();
		AssertEquals("When transportmode is changed to air", 0, Instruction.CEI_LoosePackages);

		Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
		Declaration.JE_ContainerMode = INContainerModeList.Codes.Containerised;
		Instruction.CEI_LoosePackages = 101;
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		Declaration.OnSaving();
		AssertEquals("When message type is import", 0, Instruction.CEI_LoosePackages);
	}

	public void TestJE_SealBy_OnSaving()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
		Declaration.JE_SealBy = SealByCodeList.Codes.W;

		AssertEquals("when transport mode is sea", SealByCodeList.Codes.W, Declaration.JE_SealBy);

		Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
		Declaration.OnSaving();

		AssertEquals("When Transport mode changed to air", string.Empty, Declaration.JE_SealBy);

		Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
		Declaration.JE_SealBy = SealByCodeList.Codes.W;
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		Declaration.OnSaving();

		AssertEquals("When Message type changed to Import", string.Empty, Declaration.JE_SealBy);
	}

	public void TestJE_RotaionNumberAndDate_OnSaving()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
		Declaration.JE_RotationDate = new ZDateTime(2025, 2, 08);
		Declaration.JE_RotationNumber = "1234567";

		AssertEquals("when transport mode is sea", new ZDateTime(2025, 2, 08), Declaration.JE_RotationDate);
		AssertEquals("when transport mode is sea", "1234567", Declaration.JE_RotationNumber);

		Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
		Declaration.OnSaving();

		AssertEquals("When Transport mode changed to air", ZDateTime.Empty, Declaration.JE_RotationDate);
		AssertEquals("When Transport mode changed to air", ZString.Empty, Declaration.JE_RotationNumber);

		Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
		Declaration.JE_RotationDate = new ZDateTime(2025, 2, 08);
		Declaration.JE_RotationNumber = "1234567";
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		Declaration.OnSaving();

		AssertEquals("When Message type changed to Import", ZDateTime.Empty, Declaration.JE_RotationDate);
		AssertEquals("When Message type changed to Import", ZString.Empty, Declaration.JE_RotationNumber);
	}

	public void TestJE_StuffingAt_Setter()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.Containerised;
			var header = Factory.New<OrgHeader>();
			var supplierDocumentaryAddress = Declaration.SupplierDocumentaryAddress;
			AssertEquals(ZGuid.Empty, Declaration.ExportOrientedUnitsDocAddress.E2_OA_Address);

			Declaration.ExportOrientedUnitsDocAddress.E2_OA_Address = new ZGuid();
			supplierDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			Declaration.JE_StuffingAt = StuffingAtList.Codes.FAC;
			AssertEquals(ZGuid.Empty, Declaration.ExportOrientedUnitsDocAddress.E2_OA_Address);

			supplierDocumentaryAddress.OrganisationPK = header.PK;
			AssertEquals(header.MainAddress.PK, Declaration.ExportOrientedUnitsDocAddress.E2_OA_Address);

			Declaration.JE_StuffingAt = StuffingAtList.Codes.CFS;
			AssertEquals(header.MainAddress.PK, Declaration.ExportOrientedUnitsDocAddress.E2_OA_Address);
		});
	}

	public void TestJE_MessageType_Setter()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.Containerised;
			var header = Factory.New<OrgHeader>();
			var supplierDocumentaryAddress = Declaration.SupplierDocumentaryAddress;
			AssertEquals(ZGuid.Empty, Declaration.ExportOrientedUnitsDocAddress.E2_OA_Address);

			supplierDocumentaryAddress.OrganisationPK = header.PK;
			Declaration.JE_StuffingAt = StuffingAtList.Codes.FAC;
			AssertNotEquals(header.MainAddress.PK, Declaration.ExportOrientedUnitsDocAddress.E2_OA_Address);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(header.MainAddress.PK, Declaration.ExportOrientedUnitsDocAddress.E2_OA_Address);
		});
	}

	public void TestJE_TransportMode_Setter()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.Containerised;
			var header = Factory.New<OrgHeader>();
			var supplierDocumentaryAddress = Declaration.SupplierDocumentaryAddress;
			AssertEquals(ZGuid.Empty, Declaration.ExportOrientedUnitsDocAddress.E2_OA_Address);

			supplierDocumentaryAddress.OrganisationPK = header.PK;
			Declaration.JE_StuffingAt = StuffingAtList.Codes.FAC;
			AssertNotEquals(header.MainAddress.PK, Declaration.ExportOrientedUnitsDocAddress.E2_OA_Address);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(header.MainAddress.PK, Declaration.ExportOrientedUnitsDocAddress.E2_OA_Address);
		});
	}

	public void TestJE_ContainerMode_Setter()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.Liquid;
			var header = Factory.New<OrgHeader>();
			var supplierDocumentaryAddress = Declaration.SupplierDocumentaryAddress;
			AssertEquals(ZGuid.Empty, Declaration.ExportOrientedUnitsDocAddress.E2_OA_Address);

			supplierDocumentaryAddress.OrganisationPK = header.PK;
			Declaration.JE_StuffingAt = StuffingAtList.Codes.FAC;
			AssertNotEquals(header.MainAddress.PK, Declaration.ExportOrientedUnitsDocAddress.E2_OA_Address);

			Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.Containerised;
			AssertEquals(header.MainAddress.PK, Declaration.ExportOrientedUnitsDocAddress.E2_OA_Address);
		});
	}

	public void TestJE_StuffingAt_OnSaving()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.Containerised;
			Declaration.JE_StuffingAt = StuffingAtList.Codes.CFS;
			Declaration.OnSaving();
			AssertEquals("when transport mode is sea and container mode is containerised", StuffingAtList.Codes.CFS, Declaration.JE_StuffingAt);

			Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.BreakBulk;
			Declaration.OnSaving();
			AssertEquals("when container mode changed to BBK", ZString.Empty, Declaration.JE_StuffingAt);

			Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.Containerised;
			Declaration.JE_StuffingAt = StuffingAtList.Codes.CFS;
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			Declaration.OnSaving();

			AssertEquals("When Transport mode changed to air", ZString.Empty, Declaration.JE_StuffingAt);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.Containerised;
			Declaration.JE_StuffingAt = StuffingAtList.Codes.CFS;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.OnSaving();

			AssertEquals("When Message type changed to Import", ZString.Empty, Declaration.JE_StuffingAt);
		});
	}

	public void TestJE_SampleAccompanied_OnSaving()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.Containerised;
			Declaration.JE_StuffingAt = StuffingAtList.Codes.FAC;
			Declaration.JE_SampleAccompanied = YesNoList.Codes.Yes;
			Declaration.OnSaving();
			AssertEquals("when transport mode is sea and container mode is containerised", YesNoList.Codes.Yes, Declaration.JE_SampleAccompanied);

			Declaration.JE_StuffingAt = StuffingAtList.Codes.CFS;
			Declaration.OnSaving();

			AssertEquals("when stuffing at changed to CFS", ZString.Empty, Declaration.JE_SampleAccompanied);

			Declaration.JE_StuffingAt = StuffingAtList.Codes.FAC;
			Declaration.JE_SampleAccompanied = YesNoList.Codes.Yes;
			Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.BreakBulk;
			Declaration.OnSaving();
			AssertEquals("when container mode changed to BBK", ZString.Empty, Declaration.JE_SampleAccompanied);

			Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.Containerised;
			Declaration.JE_StuffingAt = StuffingAtList.Codes.FAC;
			Declaration.JE_SampleAccompanied = YesNoList.Codes.Yes;
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			Declaration.OnSaving();
			AssertEquals("When Transport mode changed to air", ZString.Empty, Declaration.JE_SampleAccompanied);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.Containerised;
			Declaration.JE_StuffingAt = StuffingAtList.Codes.FAC;
			Declaration.JE_SampleAccompanied = YesNoList.Codes.Yes;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.OnSaving();
			AssertEquals("When Message type changed to Import", ZString.Empty, Declaration.JE_SampleAccompanied);
		});
	}

	public void TestGetIApportionInvoiceHolderCountryContextCore()
	{
		var declaration = Factory.New<JobDeclaration>();
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertType<CommonIncoTermAndCustomsChargeFactory>(declaration.IncoTermAndChargeFactory);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertType<ExportIncoTermAndCustomsChargeFactory>(declaration.IncoTermAndChargeFactory);
		});
	}

	public void TestPiggyBackedDocAddressValidation()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertType<ExportDeclarationJobDocAddressValidation>("Export", Declaration.PiggyBackedDocAddressValidation(declaration.TranshipperDocAddress));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertNull("Import", declaration.PiggyBackedDocAddressValidation(declaration.TranshipperDocAddress));
		});
	}

	protected override string DefaultMergeType => OrgConstants.MergeInvoiceLines.NotMerge;

	CusEntryInstruction Instruction => instruction ??= Declaration.CustomsEntryInstructions.AddNew();
	CusEntryInstruction instruction;

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;
}
