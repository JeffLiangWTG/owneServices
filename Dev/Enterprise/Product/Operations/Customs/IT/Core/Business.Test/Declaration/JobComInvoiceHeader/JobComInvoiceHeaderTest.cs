using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(JobComInvoiceHeader))]
sealed class JobComInvoiceHeaderTest : EU.Business.Declaration.Testing.JobComInvoiceHeaderTest<JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine>
{
	public void TestIsValidToDefaultIncoTermFromSupplier()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeaderForTest>();
		invoiceHeader.JZ_IncoTerm = ZString.Empty;
		AssertEquals("IncoTerm empty, so now is valid to take default IncoTerm from supplier", true, invoiceHeader.IsValidToDefaultIncoTermFromSupplierExposed);

		invoiceHeader.JZ_IncoTerm = "FOB";
		AssertEquals("IncoTerm is already set, so now is not valid to take default IncoTerm from supplier", false, invoiceHeader.IsValidToDefaultIncoTermFromSupplierExposed);
	}

	public void TestIncoTermNotChangedWhenAddingInvoiceLine_ExportCase()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";

		var supplier = Factory.New<OrgHeader>();
		supplier.MiscServ.OM_EXDefaultIncoTerm = "DAP";
		supplier.OH_Code = "AA";

		declaration.JE_OH_Supplier = supplier.PK;

		var invoiceHeader = declaration.Invoices.AddNew();
		AssertEquals("IncoTerm", "DAP", invoiceHeader.JZ_IncoTerm);

		invoiceHeader.JZ_IncoTerm = "CIF";
		invoiceHeader.InvoiceLines.AddNew();

		AssertEquals("IncoTerm", "CIF", invoiceHeader.JZ_IncoTerm);
	}

	public void TestIncoTermNotChangedWhenAddingInvoiceLine_ImportCase()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";

		var supplier = Factory.New<OrgHeader>();
		supplier.MiscServ.OM_EXDefaultIncoTerm = "DAP";
		supplier.OH_Code = "AA";

		declaration.JE_OH_Supplier = supplier.PK;

		var invoiceHeader = declaration.Invoices.AddNew();
		AssertEquals("IncoTerm", "DAP", invoiceHeader.JZ_IncoTerm);

		invoiceHeader.JZ_IncoTerm = "CIF";
		invoiceHeader.InvoiceLines.AddNew();

		AssertEquals("IncoTerm", "CIF", invoiceHeader.JZ_IncoTerm);
	}

	public void TestPreviousDocuments()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		AssertType<PreviousDocumentCollection>(invoice.PreviousDocuments);
	}

	public void TestSupportingDocuments()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		AssertType<SupportingDocumentCollection>(invoice.SupportingDocuments);
	}

	public void TestUpdateDefaultSupportingDocumentCountryToSupplierCountry()
	{
		var supplierFromES = Factory.NewWithValidTestData<OrgHeader>();
		supplierFromES.MainAddress.OA_RN_NKCountryCode = "ES";

		var supplierFromDE = Factory.NewWithValidTestData<OrgHeader>();
		supplierFromDE.MainAddress.OA_RN_NKCountryCode = "DE";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = "BLT";
		declaration.JE_OH_Supplier = supplierFromES.PK;
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_InvoiceNumber = "123";
		invoice.JZ_InvoiceDate = ZDateTime.Today;
		var supportingDocumentN380 = invoice.SupportingDocuments[0];
		var supportingDocumentOther = invoice.SupportingDocuments.AddNew();
		supportingDocumentOther.CSI_Code = "ABC";
		supportingDocumentOther.CSI_RN_NKCountryCode = "IN";

		CombineAssertions("Declaration Supplier with ES country and Invoice header supplier empty", () =>
		{
			AssertEquals("for N380 Supporting doc Country code", "ES", supportingDocumentN380.CSI_RN_NKCountryCode);
			AssertEquals("for Other Supporting doc Country code", "IN", supportingDocumentOther.CSI_RN_NKCountryCode);
		});

		CombineAssertions("Declaration Supplier with DE country and Invoice header supplier empty", () =>
		{
			invoice.JZ_OH_Supplier = ZGuid.Empty;
			declaration.JE_OH_Supplier = supplierFromDE.PK;
			AssertEquals("for N380 Supporting doc Country code", "DE", supportingDocumentN380.CSI_RN_NKCountryCode);
			AssertEquals("for Other Supporting doc Country code", "IN", supportingDocumentOther.CSI_RN_NKCountryCode);
		});

		CombineAssertions("Without Supplier at Declaration and Invoice header", () =>
		{
			invoice.JZ_OH_Supplier = ZGuid.Empty;
			declaration.JE_OH_Supplier = ZGuid.Empty;
			AssertEquals("No change for N380 Supporting doc Country code", "DE", supportingDocumentN380.CSI_RN_NKCountryCode);
			AssertEquals("No change for Other Supporting doc Country code", "IN", supportingDocumentOther.CSI_RN_NKCountryCode);
		});

		CombineAssertions("InvoiceHeader Supplier with ES country", () =>
		{
			invoice.JZ_OH_Supplier = supplierFromES.PK;
			AssertEquals("for N380 Supporting doc Country code", "ES", supportingDocumentN380.CSI_RN_NKCountryCode);
			AssertEquals("for Other Supporting doc Country code", "IN", supportingDocumentOther.CSI_RN_NKCountryCode);
		});

		CombineAssertions("InvoiceHeader Supplier with DE country", () =>
		{
			invoice.JZ_OH_Supplier = supplierFromDE.PK;
			AssertEquals("for N380 Supporting doc Country code", "DE", supportingDocumentN380.CSI_RN_NKCountryCode);
			AssertEquals("for Other Supporting doc Country code", "IN", supportingDocumentOther.CSI_RN_NKCountryCode);
		});

		CombineAssertions("Without Supplier at InvoiceHeader", () =>
		{
			invoice.JZ_OH_Supplier = ZGuid.Empty;
			AssertEquals("No change for N380 Supporting doc Country code", "DE", supportingDocumentN380.CSI_RN_NKCountryCode);
			AssertEquals("No change for Other Supporting doc Country code", "IN", supportingDocumentOther.CSI_RN_NKCountryCode);
		});

		CombineAssertions("Declaration Supplier with ES country and InvoiceHeader Supplier empty", () =>
		{
			declaration.JE_OH_Supplier = supplierFromES.PK;
			AssertEquals("for N380 Supporting doc Country code", "ES", supportingDocumentN380.CSI_RN_NKCountryCode);
			AssertEquals("for Other Supporting doc Country code", "IN", supportingDocumentOther.CSI_RN_NKCountryCode);
		});
	}

	public void TestCreateNewJobComInvoiceLineCollection()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		CombineAssertions(() =>
		{
			AssertType<JobComInvoiceLineViewCollection>("JobComInvoiceLines", invoiceHeader.JobComInvoiceLines);
			AssertType<JobComInvoiceLineViewCollection>("InvoiceLines", invoiceHeader.InvoiceLines);
		});
	}

	public void TestCreateNewInvoiceLineCollectionWhenDeclarationIsNull()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		AssertType<JobComInvoiceLineViewCollection>("JobComInvoiceLines", invoiceHeader.JobComInvoiceLines);
	}

	public void TestEffectiveValuationDate()
	{
		var date1 = new ZDate(2019, 11, 15);

		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction1.CEI_DateForDuty = date1;
		entryInstruction1.CEI_SubStyle = "B";

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction1.PK;

		AssertEquals("EffectiveValuationDate should be", date1, invoice.EffectiveValuationDate);

		invoiceLine1.JI_CEI = ZGuid.Empty;
		AssertEquals("When an Invoice has no entry instructions related the EffectiveValuationDate should be", ZDate.Today, invoice.EffectiveValuationDate);

		var date2 = new ZDate(2019, 11, 16);
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction2.CEI_SubStyle = "A";
		entryInstruction2.CEI_DateForDuty = date2;

		invoiceLine1.JI_CEI = entryInstruction1.PK;
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction2.PK;

		AssertEquals("When an Invoice has two ore more related entry instructions, the EffectiveValuationDate should be", date2, invoice.EffectiveValuationDate);
	}

	public void TestGetNewValidation()
	{
		var standaloneInvoiceHeader = Factory.New<JobComInvoiceHeader>();
		AssertType<JobComInvoiceHeaderValidation>(standaloneInvoiceHeader.Validation);

		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeaderUnderDeclaration = declaration.Invoices.AddNew();
		AssertType<JobComInvoiceHeaderValidation>(invoiceHeaderUnderDeclaration.Validation);
	}

	public void TestZG_AgreedPlaceCode()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		invoiceHeader.ZG_AgreedPlaceCode = "1";
		Factory.Save();
		AssertContains("AgreedPlaceCode=1", invoiceHeader.JZ_AddInfo);
	}

	public void TestLookupsType()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		AssertType<JobComInvoiceHeaderLookups>(invoiceHeader.Lookups);
	}

	public void TestAddInfoValidation()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		AssertType<AddInfoJobComInvoiceHeaderValidation>("JobComInvoiceHeader.AddInfoValidation type should be", invoiceHeader.AddInfoValidation);
	}

	public void TestCusEntryInstructions()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction1.PK;
		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction2.PK;

		AssertEquals("InvoiceHeader.CusEntryInstructions count should be", 2, invoiceHeader.CusEntryInstructions.Count());
		AssertType<CusEntryInstruction>("InvoiceHeader.CusEntryInstructions type should be", invoiceHeader.CusEntryInstructions.ElementAt(0));
	}

	public void TestAeoCertificatesManager()
	{
		var invoice = Factory.New<JobDeclaration>().Invoices.AddNew();

		var aeoCertificateManager = invoice.AeoCertificateManager;
		AssertNotNull("AeoCertificateManager must be never null", aeoCertificateManager);
		AssertSame("AeoCertificateManager must be cached", aeoCertificateManager, invoice.AeoCertificateManager);

		var anotherDeclaration = Factory.New<JobDeclaration>();
		invoice.JZ_JE = anotherDeclaration.PK;

		AssertEquals("AeoCertificateManager must be reset when parent Declaration change", true, aeoCertificateManager != invoice.AeoCertificateManager);
	}

	public void TestSupplierDocumentaryAddress()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		var supplier1 = GetSupplier();

		AssertNotNull("SupplierDocumentaryAddress must be never null", invoiceHeader.SupplierDocumentaryAddress);

		AssertSupplierDocumentaryAddressHasDefaultedWithOrganisationData();
		AssertSupplierDocumentaryAddressIsStillSyncronizedWithOrganisation();
		AssertSupplierDocumentaryDoesNotChangeWhenOverrideFlagIsTrue();

		#region Local Methods

		void AssertSupplierDocumentaryAddressHasDefaultedWithOrganisationData()
		{
			var supplier1DocumentaryAddress = invoiceHeader.SupplierDocumentaryAddress;
			supplier1DocumentaryAddress.E2_OA_Address = supplier1.MainAddress.PK;

			Factory.Save();

			AssertSupplierDocumentaryAddress("Assert related JobDocAddress has defaulted with same data from organisation",
				"IKEA",
				"MAIN",
				"ADDRESS",
				"ABCEXPMEL",
				"4000",
				"ZA",
				invoiceHeader.SupplierDocumentaryAddress);
		}

		void AssertSupplierDocumentaryAddressIsStillSyncronizedWithOrganisation()
		{
			var otherFactory = ChangeOrganisationInAnotherFactory(supplier1.PK, "SECOND ADDRESS", "", "4000", "MILAN", "IT");
			var reloadedInvoiceHeader = otherFactory.Load<JobComInvoiceHeader>(invoiceHeader.PK);

			AssertSupplierDocumentaryAddress("Assert related JobDocAddress has not been changed",
				"IKEA",
				"SECOND ADDRESS",
				"",
				"MILAN",
				"4000",
				"IT",
				reloadedInvoiceHeader.SupplierDocumentaryAddress);
		}

		void AssertSupplierDocumentaryDoesNotChangeWhenOverrideFlagIsTrue()
		{
			var otherFactory = new BusinessObjectFactory();
			invoiceHeader = otherFactory.Load<JobComInvoiceHeader>(invoiceHeader.PK);
			invoiceHeader.SupplierDocumentaryAddress.E2_AddressOverride = true;
			otherFactory.Save();

			var anotherFactory = ChangeOrganisationInAnotherFactory(supplier1.PK, "THIRD ADDRESS", "3", "5000", "ROME", "IT");
			var reloadedInvoiceHeader = anotherFactory.Load<JobComInvoiceHeader>(invoiceHeader.PK);

			AssertSupplierDocumentaryAddress("Assert related JobDocAddress has not been changed",
				"IKEA",
				"SECOND ADDRESS",
				"",
				"MILAN",
				"4000",
				"IT",
				reloadedInvoiceHeader.SupplierDocumentaryAddress);
		}

		BusinessObjectFactory ChangeOrganisationInAnotherFactory(ZGuid organisationPk, string address1, string address2, string postCode, string city, string countryCode)
		{
			var factory = new BusinessObjectFactory();

			supplier1 = factory.Load<OrgHeader>(organisationPk);
			var address = supplier1.MainAddress;
			address.Address1 = address1;
			address.Address2 = address2;
			address.Postcode = postCode;
			address.City = city;
			address.OA_RN_NKCountryCode = countryCode;

			factory.Save();
			return factory;
		}

		#endregion
	}

	public void TestSupplierDocumentaryAddressDefaultSupplier()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		var supplier1 = GetSupplier();

		AssertEquals("[PRE-CONDITION] JZ_OH_Supplier", ZGuid.Empty, invoiceHeader.JZ_OH_Supplier);

		invoiceHeader.SupplierDocumentaryAddress.E2_OA_Address = supplier1.MainAddress.PK;
		AssertEquals("JZ_OH_Supplier", supplier1.PK, invoiceHeader.JZ_OH_Supplier);

		invoiceHeader.SupplierDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
		AssertEquals("JZ_OH_Supplier", ZGuid.Empty, invoiceHeader.JZ_OH_Supplier);
	}

	public void TestDefaultSupplierDocumentaryAddress()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var supplier1 = GetSupplier();

		AssertSupplierDocumentaryAddress("[PRE-CONDITION] Assert SupplierDocumentaryAddress is Empty", "", "", "", "", "", "", invoiceHeader.SupplierDocumentaryAddress);

		invoiceHeader.JZ_OH_Supplier = supplier1.PK;
		AssertSupplierDocumentaryAddress("Assert JobDoSupplierDocumentaryAddresscAddress has defaulted with same data from organisation",
			"IKEA",
			"MAIN",
			"ADDRESS",
			"ABCEXPMEL",
			"4000",
			"ZA",
			invoiceHeader.SupplierDocumentaryAddress);

		invoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
		AssertSupplierDocumentaryAddress("Assert SupplierDocumentaryAddress is Empty", "", "", "", "", "", "", invoiceHeader.SupplierDocumentaryAddress);
	}

	public void TestDefaultSupplierOrgPkOnJZ_OH_SupplierSet()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();

		var supplier = GetSupplier();
		invoiceHeader.JZ_OH_Supplier = supplier.PK;
		AssertEquals("SupplierOrgPK", supplier.PK, invoiceHeader.SupplierOrgPK);
	}

	public void TestDefaultSupplierOrgPkOnInvoiceLoaded()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();

		var supplier = GetSupplier();
		invoiceHeader.JZ_OH_Supplier = supplier.PK;
		AssertEquals("PRE-CONDITION: SupplierOrgPK", supplier.PK, invoiceHeader.SupplierOrgPK);
		Factory.Save();

		var reloadedInvoice = new BusinessObjectFactory().Load<JobComInvoiceHeader>(invoiceHeader.PK);
		AssertEquals("POST-CONDITION: SupplierOrgPK", supplier.PK, reloadedInvoice.SupplierOrgPK);
	}

	public void TestSettingJZ_OA_SupplierAddressDoNotSetJZ_OH_Supplier()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();

		var supplier = GetSupplier();
		invoiceHeader.JZ_OH_Supplier = supplier.PK;
		invoiceHeader.JZ_OA_SupplierAddress = supplier.MainAddress.PK;

		invoiceHeader.JZ_OA_SupplierAddress = ZGuid.Empty;
		AssertEquals("JZ_OH_Supplier", supplier.PK, invoiceHeader.JZ_OH_Supplier);
	}

	public void TestSupplierPiggyBackedDocAddressValidation()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();

		var supplier = GetSupplier();
		invoiceHeader.SupplierDocumentaryAddress.E2_OA_Address = supplier.MainAddress.PK;

		declaration.JE_MessageType = "EXP";
		AssertPiggyBackedDocAddressValidation();

		declaration.JE_MessageType = "IMP";
		AssertNull("When declaration is Import, PiggyBackedDocAddressValidation the return type", invoiceHeader.PiggyBackedDocAddressValidation(invoiceHeader.SupplierDocumentaryAddress));

		void AssertPiggyBackedDocAddressValidation()
		{
			invoiceHeader.InvoiceLines.AddNew().JI_CEI = entryInstruction1.PK;

			CombineAssertions($"Assert PiggyBackedDocAddressValidation return object type for {declaration.JE_MessageType} declaration", () =>
			{
				entryInstruction1.ZG_ParticipantType = ParticipantTypeList.Codes.BuyersConsolManySuppliersOneImporter;
				AssertType<TraderJobDocAddressValidation>("When Entry Instructions is BuyersConsol, PiggyBackedDocAddressValidation the return type", invoiceHeader.PiggyBackedDocAddressValidation(invoiceHeader.SupplierDocumentaryAddress));

				entryInstruction1.ZG_ParticipantType = ParticipantTypeList.Codes.Triangulation;
				AssertNull("When Entry Instructions is not BuyersConsol, BuyersConsolManySuppliersOneImporter, PiggyBackedDocAddressValidation the return type", invoiceHeader.PiggyBackedDocAddressValidation(invoiceHeader.SupplierDocumentaryAddress));

				invoiceHeader.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.JI_CEI = ZGuid.Empty);
				AssertNull("When has no entry instructions, PiggyBackedDocAddressValidation return type", invoiceHeader.PiggyBackedDocAddressValidation(invoiceHeader.SupplierDocumentaryAddress));

				invoiceHeader.InvoiceLines.RemoveAndDeleteAll();
				AssertNull("When has no invoice lines, PiggyBackedDocAddressValidation return type", invoiceHeader.PiggyBackedDocAddressValidation(invoiceHeader.SupplierDocumentaryAddress));
			});
		}
	}

	public void TestIsBuyersConsol()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();

		var invoiceHeader = declaration.Invoices.AddNew();
		AssertEquals("When Invoice header has no entry instructions", false, invoiceHeader.IsBuyersConsol);

		invoiceHeader.InvoiceLines.AddNew().JI_CEI = entryInstruction1.PK;

		entryInstruction1.ZG_ParticipantType = ParticipantTypeList.Codes.BuyersConsolManySuppliersOneImporter;
		AssertEquals("When Invoice has one Entry Instruction with ParticipantType is BuyersConsol", true, invoiceHeader.IsBuyersConsol);

		entryInstruction1.ZG_ParticipantType = ParticipantTypeList.Codes.Triangulation;
		AssertEquals("When Invoice has one Entry Instruction with ParticipantType not BuyersConsol", false, invoiceHeader.IsBuyersConsol);

		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		invoiceHeader.InvoiceLines.AddNew().JI_CEI = entryInstruction2.PK;

		entryInstruction1.ZG_ParticipantType = ParticipantTypeList.Codes.BuyersConsolManySuppliersOneImporter;
		entryInstruction2.ZG_ParticipantType = ParticipantTypeList.Codes.Triangulation;
		AssertEquals("When Invoice has multiple Entry Instructions with different Participant Types", false, invoiceHeader.IsBuyersConsol);

		entryInstruction2.ZG_ParticipantType = ParticipantTypeList.Codes.BuyersConsolManySuppliersOneImporter;
		AssertEquals("When Invoice has multiple Entry Instructions with same Participant Type", true, invoiceHeader.IsBuyersConsol);
	}

	public void TestEntryInstructionsHaveDifferentParticipantTypes()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();

		invoiceLine1.JI_CEI = entryInstruction1.PK;
		invoiceLine2.JI_CEI = entryInstruction2.PK;

		entryInstruction1.ZG_ParticipantType = ParticipantTypeList.Codes.BuyersConsolManySuppliersOneImporter;
		entryInstruction2.ZG_ParticipantType = ParticipantTypeList.Codes.Triangulation;
		AssertEquals("EntryInstructionsHaveDifferentParticipantTypes", true, invoiceHeader.EntryInstructionsHaveDifferentParticipantTypes);

		entryInstruction2.ZG_ParticipantType = ParticipantTypeList.Codes.BuyersConsolManySuppliersOneImporter;
		AssertEquals("EntryInstructionsHaveDifferentParticipantTypes", false, invoiceHeader.EntryInstructionsHaveDifferentParticipantTypes);
	}

	public void TestJZ_UCRMaxLenght()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		AssertEquals("JZ_UCR MaxLength", 35, invoiceHeader.JZ_UCRInfo.MaxLength);
	}

	public void TestFetchStrategy()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		AssertType<JobComInvoiceHeaderFetchStrategy>("FetchStrategy", invoiceHeader.FetchStrategy);
	}

	public void TestDocAddressesType()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		AssertType<JobComInvoiceHeaderDocAddressDependentCollection>("DocAddresses Type", invoiceHeader.DocAddresses);
	}

	public void TestJZ_IncoTermCaption()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		AssertEquals("JZ_IncoTerm Caption", "[20.1] INCO term", DataBoundResourceStrings.GetDataForProperty(invoiceHeader.JZ_IncoTermInfo)?.Caption);
	}

	public void TestAdditionalInfosType()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		AssertType<InvoiceHeaderAdditionalInfoCollection>("AdditionalInfos Type", invoiceHeader.AdditionalInfos);
	}

	public void TestGetCusSupportingInfoTypes()
	{
		var supportingInfoTypeSupporter = (Integration.Customs.ICusSupportingInfoTypeSupporter)Factory.New<JobComInvoiceHeader>();
		var supportingInfoTypes = supportingInfoTypeSupporter.GetCusSupportingInfoTypes();
		AssertEquals("SupportingInfoTypes Count", 3, supportingInfoTypes.Count);
		AssertEquals("AdditionalInfo Type", typeof(InvoiceHeaderAdditionalInfo), supportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
		AssertEquals("SupportingDocument Type", typeof(SupportingDocument), supportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument]);
		AssertEquals("PreviousDocument Type", typeof(PreviousDocument), supportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument]);
	}

	public void TestJZ_AdditionalTermsMaxLenght()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		AssertEquals("MaxLength", 512, invoiceHeader.JZ_AdditionalTermsInfo.MaxLength);
	}

	public void TestJZ_AdditionalTermsWipedOnIncotermSet()
	{
		var declaration = Factory.New<JobDeclaration>();
		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_AdditionalTerms = "TRM";
			invoiceHeader.JZ_IncoTerm = "XXX";
			AssertEquals("JZ_AdditionalTerms", "TRM", invoiceHeader.JZ_AdditionalTerms);

			invoiceHeader.JZ_IncoTerm = "FOB";
			AssertEquals("JZ_AdditionalTerms", "", invoiceHeader.JZ_AdditionalTerms);
		}
	}

	public void TestAdditionalTermsSupport()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();

		CombineAssertions("When job is not UCC6", () =>
		{
			using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				declaration.JE_MessageType = "IMP";
				invoice.JZ_IncoTerm = "XXX";
				AssertEquals("When MessageType is IMP and IncoTerm is XXX", false, invoice.AdditionalTermsSupport);

				invoice.JZ_IncoTerm = "FOB";
				AssertEquals("When MessageType is IMP and IncoTerm is FOB", false, invoice.AdditionalTermsSupport);

				declaration.JE_MessageType = "EXP";
				invoice.JZ_IncoTerm = "XXX";
				AssertEquals("When MessageType is EXP and IncoTerm is XXX", false, invoice.AdditionalTermsSupport);

				invoice.JZ_IncoTerm = "FOB";
				AssertEquals("When MessageType is EXP and IncoTerm is FOB", false, invoice.AdditionalTermsSupport);
			}
		});

		CombineAssertions("When job is UCC6", () =>
		{
			using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = "IMP";
				invoice.JZ_IncoTerm = "XXX";
				AssertEquals("When MessageType is IMP and IncoTerm is XXX", false, invoice.AdditionalTermsSupport);

				invoice.JZ_IncoTerm = "FOB";
				AssertEquals("When MessageType is IMP and IncoTerm is FOB", false, invoice.AdditionalTermsSupport);

				declaration.JE_MessageType = "EXP";
				invoice.JZ_IncoTerm = "XXX";
				AssertEquals("When MessageType is EXP and IncoTerm is XXX", true, invoice.AdditionalTermsSupport);

				invoice.JZ_IncoTerm = "FOB";
				AssertEquals("When MessageType is EXP and IncoTerm is FOB", false, invoice.AdditionalTermsSupport);
			}
		});
	}

	public void TestAgreedPlaceCodeSupportAndVisible()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		invoice.JZ_IncoTerm = IncoTerms.Other;
		AssertEquals("When Import and IncoTerm is XXX", ZBool.True, invoice.AgreedPlaceCodeSupportAndVisible);

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		invoice.JZ_IncoTerm = IncoTerms.Other;
		AssertEquals("When Export not UCC6 and IncoTerm is XXX", ZBool.False, invoice.AgreedPlaceCodeSupportAndVisible);

		using (TemporarilyClearDeclarationConfigurationAndThenSetAgreedPlaceCodeSupport(declaration, true))
		{
			invoice.JZ_IncoTerm = IncoTerms.CarriageAndInsurancePaidTo;
			AssertEquals("When Export not UCC6 and IncoTerm is not XXX and 'AgreedPlaceCodeSupportCore' is ON", ZBool.True, invoice.AgreedPlaceCodeSupportAndVisible);

			invoice.JZ_IncoTerm = IncoTerms.Other;
			AssertEquals("When Export not UCC6 and IncoTerm is XXX and 'AgreedPlaceCodeSupportCore' is ON", ZBool.False, invoice.AgreedPlaceCodeSupportAndVisible);
		}
	}

	public override string GetLocalCurrencyCode() => Core.Constants.CurrencyCodes.Italy;

	protected override CodeDescriptionPairList GetExpectedCustomsChargeTypeList()
	{
		var customsChargeTypeList = new UCCCustomsChargeTypeList();
		customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.Additions71Charge);
		customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge);
		customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.TransportCostsCharge);
		customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.IndirectAndOtherPaymentsCharge);
		customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge);
		customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.Deductions71Charge);
		customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.RightToReproduceCharge);
		customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge);
		customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.AdjustmentCharge);
		customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge);
		customsChargeTypeList.Sort();
		return customsChargeTypeList;
	}

	protected override BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();

	protected override bool ShouldBOGetSavedWithDetachedInvoiceHeader(BaseJobComInvoiceHeader invoice, Type invoiceLineAddInfoChildType, BusinessObject bO)
	{
		return (bO is GenAddOnColumn genAddOnColumn && genAddOnColumn.XA_Name == JobDeclaration.GenAddOnColumnConstants.MessageVersionColumnName) || base.ShouldBOGetSavedWithDetachedInvoiceHeader(invoice, invoiceLineAddInfoChildType, bO);
	}

	protected override void SetupCountrySpecificDataForBondedWarehousingInvoiceWithDifferentSupplierTest(BaseJobDeclaration jobDeclaration, BaseJobComInvoiceHeader invoiceHeader, BaseJobComInvoiceLine invoiceLine, OrgHeader supplier)
	{
		var entryInstruction = (CusEntryInstruction)jobDeclaration.CustomsEntryInstructions.AddNew();
		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.BuyersConsolManySuppliersOneImporter;
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceHeader.JZ_OH_Supplier = supplier.PK;
	}

	protected override Type ExpectedTypeOfGroupCharges => typeof(JobComInvApportionedChargeCollection<InvoiceApportionCharge>);

	protected override Type ExpectedTypeOfCharges => typeof(InvoiceChargeCollection<InvoiceCharge>);

	OrgHeader GetSupplier()
	{
		var supplier = Factory.New<OrgHeader>();
		supplier.OH_Code = "IK1";
		supplier.OH_FullName = "IKEA";
		var address = supplier.MainAddress;
		address.CompanyName = "IKEA";
		address.Address1 = "MAIN";
		address.Address2 = "ADDRESS";
		address.Postcode = "4000";
		address.City = "ABCEXPMEL";
		address.OA_RN_NKCountryCode = "ZA";
		return supplier;
	}

	void AssertSupplierDocumentaryAddress(string assertionMessage, ZString expectedCompanyName, ZString expectedAddress1, ZString expectedAddress2, ZString expectedCity, ZString expectedPostCode, ZString expectedCountryCode, JobDocAddress supplierDocumentaryAddress)
	{
		CombineAssertions(assertionMessage, () =>
		{
			AssertEquals("DocAddressType", DocAddressType.SupplierDocumentaryAddress, supplierDocumentaryAddress.DocAddressType);
			AssertEquals("JobDocAddress.E2_CompanyName", expectedCompanyName, supplierDocumentaryAddress.E2_CompanyName);
			AssertEquals("JobDocAddress.E2_Address1", expectedAddress1, supplierDocumentaryAddress.E2_Address1);
			AssertEquals("JobDocAddress.E2_Address2", expectedAddress2, supplierDocumentaryAddress.E2_Address2);
			AssertEquals("JobDocAddress.E2_City", expectedCity, supplierDocumentaryAddress.E2_City);
			AssertEquals("JobDocAddress.E2_Postcode", expectedPostCode, supplierDocumentaryAddress.E2_Postcode);
			AssertEquals("JobDocAddress.OA_RN_NKCountryCode", expectedCountryCode, supplierDocumentaryAddress.E2_RN_NKCountryCode);
		});
	}

	IDisposable TemporarilyClearDeclarationConfigurationAndThenSetAgreedPlaceCodeSupport(JobDeclaration declaration, bool configurationValue) => EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetAgreedPlaceCodeSupport(declaration, configurationValue);

	class JobComInvoiceHeaderForTest : JobComInvoiceHeader
	{
		public JobComInvoiceHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool IsValidToDefaultIncoTermFromSupplierExposed => base.IsValidToDefaultIncoTermFromSupplier;
	}
}
