using System;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;

sealed class CusEntryInstructionCustomsMessageWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new CusEntryInstructionCustomsMessageWrapper(entryInstruction: null));
		AssertExceptionThrown<ArgumentNullException>(() => new CusEntryInstructionCustomsMessageWrapper(Factory.New<CusEntryInstruction>()));
	}

	public void TestAdditionalDeclarationType()
	{
		var wrapper = GetNewWrapper();
		entryInstruction.CEI_SubStyle = "";
		AssertNullOrEmpty(nameof(ICusEntryInstructionCustomsMessageWrapper.AdditionalDeclarationType), wrapper.AdditionalDeclarationType);

		entryInstruction.CEI_SubStyle = "D";
		wrapper = GetNewWrapper();
		AssertEquals(nameof(ICusEntryInstructionCustomsMessageWrapper.AdditionalDeclarationType), "D", wrapper.AdditionalDeclarationType);
	}

	public void TestAuthorizations()
	{
		var wrapper = GetNewWrapper();
		AssertNotNull(nameof(ICusEntryInstructionCustomsMessageWrapper.Authorizations), wrapper.Authorizations);
		AssertEquals($"{nameof(ICusEntryInstructionCustomsMessageWrapper.Authorizations)} count", 0, wrapper.Authorizations.Count);

		entryInstruction.CusAuthorizationUsages.AddNew();
		entryInstruction.CusAuthorizationUsages.AddNew();

		wrapper = GetNewWrapper();
		var authorizations = wrapper.Authorizations;
		AssertEquals($"{nameof(ICusEntryInstructionCustomsMessageWrapper.Authorizations)} count", 2, authorizations.Count);
		AssertSame(nameof(ICusEntryInstructionCustomsMessageWrapper.Authorizations), authorizations, wrapper.Authorizations);
		AssertEquals($"{nameof(ICusEntryInstructionCustomsMessageWrapper.Authorizations)} Type", true, authorizations.All(x => x is AuthorizationWrapper));
	}

	public void TestAdditionalSupplyChainActors()
	{
		var wrapper = GetNewWrapper();

		AssertNotNull(nameof(ICusEntryInstructionCustomsMessageWrapper.AdditionalSupplyChainActors), wrapper.AdditionalSupplyChainActors);
		AssertEquals($"{nameof(ICusEntryInstructionCustomsMessageWrapper.AdditionalSupplyChainActors)} count", 0, wrapper.AdditionalSupplyChainActors.Count);

		entryInstruction.CusSupplyChainActorReferences.AddNew();
		entryInstruction.CusSupplyChainActorReferences.AddNew();

		wrapper = GetNewWrapper();
		AssertEquals($"{nameof(ICusEntryInstructionCustomsMessageWrapper.AdditionalSupplyChainActors)} count", 2, wrapper.AdditionalSupplyChainActors.Count);
		AssertEquals($"{nameof(ICusEntryInstructionCustomsMessageWrapper.AdditionalSupplyChainActors)} type should be {nameof(IH1Header.AdditionalSupplyChainActors)}", true, wrapper.AdditionalSupplyChainActors.All(x => x is AdditionalSupplyChainActorWrapper));
	}

	public void TestWarehouse()
	{
		var wrapper = GetNewWrapper();
		AssertNull(nameof(ICusEntryInstructionCustomsMessageWrapper.Warehouse), wrapper.Warehouse);

		var helper = new ITUniversalReferenceTestDataHelper(Factory);
		var procedure = helper.CreateRefCusProcedure("IT", "X", "18", "00", "", "Test", "IMP");

		var entryLine1 = entryHeader.MergedLines.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_Procedure = "1800";
		invoiceLine1.JI_CL = entryLine1.PK;

		var warehouseAddress = Factory.New<OrgHeader>().Addresses.AddNew();

		entryInstruction.CEI_OA_Warehouse = warehouseAddress.PK;
		entryInstruction.ZG_FromWarehouseID = "63237833";
		entryInstruction.ZG_FromWarehouseType = "R";
		procedure.ZZ6_IntoWarehouse = "N";
		procedure.ZZ6_OutOfWarehouse = "Y";

		wrapper = GetNewWrapper();
		var warehouse = wrapper.Warehouse;
		AssertNotNull(nameof(ICusEntryInstructionCustomsMessageWrapper.Warehouse), warehouse);

		CombineAssertions("From Warehouse", () =>
		{
			AssertType<WarehouseWrapper>(nameof(ICusEntryInstructionCustomsMessageWrapper.Warehouse), warehouse);
			AssertEquals("WarehouseType", "R", warehouse.WarehouseType);
			AssertEquals("IdentificationNumber", "63237833", warehouse.IdentificationNumber);
		});

		entryInstruction.ZG_FromWarehouseID = ZString.Empty;
		entryInstruction.ZG_FromWarehouseType = ZString.Empty;
		wrapper = GetNewWrapper();
		warehouse = wrapper.Warehouse;
		AssertNull("Warehouse Type and ID are empty", warehouse);

		entryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
		entryInstruction.CEI_OA_Warehouse2 = warehouseAddress.PK;
		entryInstruction.ZG_ToWarehouseID = "12345433";
		entryInstruction.ZG_ToWarehouseType = "B";
		procedure.ZZ6_IntoWarehouse = "Y";
		procedure.ZZ6_OutOfWarehouse = "N";

		wrapper = GetNewWrapper();
		warehouse = wrapper.Warehouse;
		AssertNotNull(nameof(ICusEntryInstructionCustomsMessageWrapper.Warehouse), warehouse);

		CombineAssertions("To Warehouse", () =>
		{
			AssertType<WarehouseWrapper>(nameof(ICusEntryInstructionCustomsMessageWrapper.Warehouse), warehouse);
			AssertEquals("WarehouseType", "B", warehouse.WarehouseType);
			AssertEquals("IdentificationNumber", "12345433", warehouse.IdentificationNumber);
		});

		entryInstruction.ZG_ToWarehouseID = ZString.Empty;
		entryInstruction.ZG_ToWarehouseType = ZString.Empty;
		wrapper = GetNewWrapper();
		warehouse = wrapper.Warehouse;
		AssertNull("Warehouse Type and ID are empty", warehouse);
	}

	public void TestAcceptanceDate_WithEmptyOrInvalidValue()
	{
		entryInstruction.CEI_SubStyle = ITEntrySubStyleList.Codes.SupplementaryDeclarationX;
		var wrapper = GetNewWrapper();
		AssertNull(nameof(ICusEntryInstructionCustomsMessageWrapper.AcceptanceDate), wrapper.AcceptanceDate);

		entryInstruction.ZG_SimplifiedDecAcceptanceDate = DateTime.MinValue;
		wrapper = GetNewWrapper();
		AssertNull(nameof(ICusEntryInstructionCustomsMessageWrapper.AcceptanceDate), wrapper.AcceptanceDate);
	}

	public void TestAcceptanceDate()
	{
		var testDate = new DateTime(2022, 01, 01);
		entryInstruction.ZG_SimplifiedDecAcceptanceDate = testDate;
		entryInstruction.CEI_SubStyle = ITEntrySubStyleList.Codes.SupplementaryDeclarationX;
		var wrapper = GetNewWrapper();
		AssertEquals(nameof(ICusEntryInstructionCustomsMessageWrapper.AcceptanceDate), testDate, wrapper.AcceptanceDate);
	}

	public void TestFiscalReferences()
	{
		var wrapper = GetNewWrapper();
		AssertNotNull(nameof(ICusEntryInstructionCustomsMessageWrapper.FiscalReferences), wrapper.FiscalReferences);
		AssertEquals($"{nameof(ICusEntryInstructionCustomsMessageWrapper.FiscalReferences)} count", 0, wrapper.FiscalReferences.Count);

		entryInstruction.FiscalReferences.AddNew();
		entryInstruction.FiscalReferences.AddNew();

		wrapper = GetNewWrapper();
		var fiscalReferences = wrapper.FiscalReferences;
		AssertEquals($"{nameof(ICusEntryInstructionCustomsMessageWrapper.FiscalReferences)} count", 2, fiscalReferences.Count);
		AssertSame(nameof(ICusEntryInstructionCustomsMessageWrapper.FiscalReferences), fiscalReferences, wrapper.FiscalReferences);
	}

	public void TestGuarantees()
	{
		var wrapper = GetNewWrapper();
		AssertNotNull(nameof(ICusEntryInstructionCustomsMessageWrapper.Guarantees), wrapper.Guarantees);
		AssertEquals($"{nameof(ICusEntryInstructionCustomsMessageWrapper.Guarantees)} count", 0, wrapper.Guarantees.Count);

		entryInstruction.Guarantees.AddNew();
		entryInstruction.Guarantees.AddNew();

		wrapper = GetNewWrapper();
		var guaranteesWrappers = wrapper.Guarantees;
		AssertEquals($"{nameof(ICusEntryInstructionCustomsMessageWrapper.Guarantees)} count", 2, guaranteesWrappers.Count);
		AssertSame(nameof(ICusEntryInstructionCustomsMessageWrapper.Guarantees), guaranteesWrappers, wrapper.Guarantees);
	}

	public void TestGuaranteeTypes()
	{
		var wrapper = GetNewWrapper();
		AssertNotNull(nameof(ICusEntryInstructionCustomsMessageWrapper.GuaranteeTypes), wrapper.GuaranteeTypes);
		AssertEquals($"{nameof(ICusEntryInstructionCustomsMessageWrapper.GuaranteeTypes)} count", 0, wrapper.GuaranteeTypes.Count);

		entryInstruction.Guarantees.AddNew().PW_BondType = "A";
		entryInstruction.Guarantees.AddNew().PW_BondType = "B";
		entryInstruction.Guarantees.AddNew().PW_BondType = "B";
		entryInstruction.Guarantees.AddNew().PW_BondType = "C";

		wrapper = GetNewWrapper();
		var guaranteesTypeWrappers = wrapper.GuaranteeTypes;
		AssertEquals($"{nameof(IH3Header.GuaranteeTypes)} count", 3, guaranteesTypeWrappers.Count);
		AssertSame(nameof(IH3Header.GuaranteeTypes), guaranteesTypeWrappers, wrapper.GuaranteeTypes);
		AssertArrayEqualsByElements(nameof(IH3Header.GuaranteeTypes), new string[] { "A", "B", "C" }, guaranteesTypeWrappers.ToArray());
	}

	public void TestGetGuaranteeHolderIdentificationNumber_WithoutDeclarant()
	{
		var wrapper = GetNewWrapper();
		var holderIdentification = wrapper.GetGuaranteeHolderIdentificationNumber(declarant: null);
		AssertNullOrEmpty("HolderIdentification", holderIdentification);

		entryInstruction.Guarantees.AddNew();
		wrapper = GetNewWrapper();
		holderIdentification = wrapper.GetGuaranteeHolderIdentificationNumber(declarant: null);
		AssertNullOrEmpty("HolderIdentification", holderIdentification);

		var guarantee = entryInstruction.Guarantees.AddNew();
		guarantee.PW_HolderIdentification = "IT67231222";
		wrapper = GetNewWrapper();
		holderIdentification = wrapper.GetGuaranteeHolderIdentificationNumber(declarant: null);
		AssertEquals("HolderIdentification", "IT67231222", holderIdentification);
	}

	public void TestGetGuaranteeHolderIdentificationNumber_WithDeclarant()
	{
		var declarantMock = new Mock<IEoriTrader>();
		declarantMock.Setup(t => t.EoriNumber).Returns("IT123123");

		entryInstruction.Guarantees.AddNew();
		var guarantee = entryInstruction.Guarantees.AddNew();
		var wrapper = GetNewWrapper();
		var holderIdentification = wrapper.GetGuaranteeHolderIdentificationNumber(declarantMock.Object);
		AssertNullOrEmpty("HolderIdentification", holderIdentification);

		guarantee.PW_HolderIdentification = "IT67231222";
		wrapper = GetNewWrapper();
		holderIdentification = wrapper.GetGuaranteeHolderIdentificationNumber(declarantMock.Object);
		AssertEquals("HolderIdentification", "IT67231222", holderIdentification);

		guarantee.PW_HolderIdentification = "IT123123";
		wrapper = GetNewWrapper();
		holderIdentification = wrapper.GetGuaranteeHolderIdentificationNumber(declarantMock.Object);
		AssertNullOrEmpty("HolderIdentification", holderIdentification);

		var guaranteeTwo = entryInstruction.Guarantees.AddNew();
		guaranteeTwo.PW_HolderIdentification = "IT6723122211";
		wrapper = GetNewWrapper();
		holderIdentification = wrapper.GetGuaranteeHolderIdentificationNumber(declarantMock.Object);
		AssertEquals("HolderIdentification", "IT6723122211", holderIdentification);
	}

	public void TestGoodsPresentationDateTime()
	{
		var wrapper = GetNewWrapper();
		AssertNull(nameof(ICusEntryInstructionCustomsMessageWrapper.GoodsPresentationDateTime), wrapper.GoodsPresentationDateTime);

		var now = ZDateTime.Now;
		entryInstruction.ZG_PresentationStartDate = now;
		wrapper = GetNewWrapper();
		AssertEquals(nameof(ICusEntryInstructionCustomsMessageWrapper.GoodsPresentationDateTime), now, wrapper.GoodsPresentationDateTime);
	}

	public void TestInlandTransportMode()
	{
		CombineAssertions(() =>
		{
			var wrapper = GetNewWrapper();
			AssertNull(nameof(ICusEntryInstructionCustomsMessageWrapper.InlandTransportMode), wrapper.InlandTransportMode);

			var officePresentation = FindOrAddNewOffice("PRE");
			var officeOfExit = FindOrAddNewOffice("EXT");

			officePresentation.CY_Data = "PRE12345";
			officeOfExit.CY_Data = "EXT9999";
			declaration.JE_EntryStyle = "EX";
			entryInstruction.CEI_SubStyle = "A";
			declaration.JE_TransportModeInland = "AIR";
			wrapper = GetNewWrapper();
			AssertEquals(nameof(ICusEntryInstructionCustomsMessageWrapper.InlandTransportMode), 4, wrapper.InlandTransportMode);

			declaration.JE_TransportModeInland = "XXX";
			wrapper = GetNewWrapper();
			AssertEquals(nameof(ICusEntryInstructionCustomsMessageWrapper.InlandTransportMode), -1, wrapper.InlandTransportMode);

			entryInstruction.CEI_SubStyle = "B";
			wrapper = GetNewWrapper();
			AssertNull(nameof(ICusEntryInstructionCustomsMessageWrapper.InlandTransportMode), wrapper.InlandTransportMode);
		});

		EuOfficeCode FindOrAddNewOffice(string code)
		{
			return declaration.CustomsOffices.Find(x => x.CY_Code == code).SingleOrDefault()
					?? declaration.CustomsOffices.AddNew(code);
		}
	}

	public void TestPreviousDocuments()
	{
		var wrapper = GetNewWrapper();
		AssertNotNull("PreviousDocuments", wrapper.PreviousDocuments);
		AssertEquals("PreviousDocuments Count", 0, wrapper.PreviousDocuments.Count);

		entryInstruction.PreviousDocuments.AddNew();
		entryInstruction.PreviousDocuments.AddNew();

		wrapper = GetNewWrapper();
		var previousDocuments = wrapper.PreviousDocuments;
		AssertEquals("PreviousDocuments Count", 2, wrapper.PreviousDocuments.Count);
		AssertSame("PreviousDocuments Cached", previousDocuments, wrapper.PreviousDocuments);
		AssertEquals("PreviousDocument Wrapper Type", expected: true, previousDocuments.All(x => x is EntryInstructionPreviousDocumentWrapper));
	}

	public void TestSupportingDocuments()
	{
		entryInstruction.ClearanceByEntryLine = true;
		var wrapper = GetNewWrapper();
		AssertNotNull("SupportingDocuments", wrapper.SupportingDocuments);
		AssertEquals("SupportingDocuments Count", 0, wrapper.SupportingDocuments.Count);

		var supportingDocument = entryInstruction.SupportingDocuments.AddNew();
		supportingDocument.CSI_Code = "1001";
		supportingDocument.CSI_YearOfIssue = "2023";
		supportingDocument.CSI_RN_NKCountryCode = "IT";
		supportingDocument.CSI_ReferenceNumber = "12345";
		entryInstruction.SupportingDocuments.AddNew();

		wrapper = GetNewWrapper();
		var supportingDocuments = wrapper.SupportingDocuments;

		AssertEquals("SupportingDocuments Count", 2, wrapper.SupportingDocuments.Count);
		AssertSame("SupportingDocuments Cached", supportingDocuments, wrapper.SupportingDocuments);
		AssertEquals("SupportingDocuments Wrapper Type", expected: true, supportingDocuments.All(x => x is SupportingDocumentWrapper));
		AssertEquals("Reference Number", true, wrapper.SupportingDocuments.Any(x => x.ReferenceNumber == "2023-IT-12345"));
	}

	public void TestSupportingDocuments_SupportingDocument33YYWrapper()
	{
		entryInstruction.ClearanceByEntryLine = false;
		var wrapper = GetNewWrapper();

		var supportingDocuments = wrapper.SupportingDocuments;
		AssertNotNull("SupportingDocuments", wrapper.SupportingDocuments);
		AssertEquals("SupportingDocuments Count", 1, wrapper.SupportingDocuments.Count);
		AssertEquals("SupportingDocuments Wrapper Type", expected: true, supportingDocuments.All(x => x is SupportingDocument33YYWrapper));
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
	}

	ICusEntryInstructionCustomsMessageWrapper GetNewWrapper() => new CusEntryInstructionCustomsMessageWrapper(entryInstruction);

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	CusEntryHeader entryHeader;
}
