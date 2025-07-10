using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants.RefCusCodeList;

namespace Enterprise.Customs.ES.Business.Testing;

public class DUAImportCommonHeaderWrapperTest : WrapperHelperTest<DUAImportCommonHeaderWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("No Header", () => GetWrapper(null));
	}

	public void TestCustomsOfficeOfDestination()
	{
		declaration.JE_CustomsOffice = HeaderData.CustomsOffice;
		AssertEquals("Expected filled CustomsOfficeOfDestination", HeaderData.CustomsOfficeCode, wrapper.CustomsOfficeOfDestination);
	}

	public void TestShipmentType()
	{
		declaration.JE_MessageSubType = HeaderData.MessageSubType;
		AssertEquals("Expected filled ShipmentType", HeaderData.MessageSubType, wrapper.ShipmentType);
	}

	public void TestProcedure()
	{
		entryInstruction.CEI_SubStyle = HeaderData.EntryInstructionSubStyle;
		AssertEquals("Expected filled Procedure", HeaderData.EntryInstructionSubStyle, wrapper.Procedure);
	}

	public void TestTotalLinesNum()
	{
		entryHeader.MergedLines.AddNew();
		entryHeader.MergedLines.AddNew();
		AssertEquals("Expected 2 TotalLinesNum", 3, wrapper.TotalLinesNum);
	}

	public void TestTotalPackagesNum_NoVehicles()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty TotalPackagesNum", 0, wrapper.TotalPackagesNum);

			var declarationBill = declaration.Bills.AddNew();
			var billPackingGroup = (EU.Business.Declaration.PackingGroup)declarationBill.PackingGroups.AddNew();
			var package1 = declaration.Packages.AddNew();
			package1.CW_CR_HouseContainer = billPackingGroup.PK;
			package1.CW_PackType = "BX";
			var package2 = declaration.Packages.AddNew();
			package2.CW_CR_HouseContainer = billPackingGroup.PK;
			package2.CW_PackType = EU.Business.UniversalReferenceConstants.RefCusCodeUnPackedPackageUnitType.Unpacked;
			var package3 = declaration.Packages.AddNew();
			package3.CW_CR_HouseContainer = billPackingGroup.PK;
			package3.CW_PackType = PackageType.Frame;

			var collection1 = (InvoiceLineCusLinkPackageCollection)invoiceLine.PackagesForInvoiceLinesForBindingOnly;

			var linkPackage1 = collection1.AddNew();
			linkPackage1.Package = package1;
			linkPackage1.IsLinked = true;
			linkPackage1.PackQty = 5;

			var linkPackage2 = collection1.AddNew();
			linkPackage2.Package = package2;
			linkPackage2.IsLinked = true;
			linkPackage2.PackQty = 4;

			var linkPackage3 = collection1.AddNew();
			linkPackage3.Package = package3;
			linkPackage3.IsLinked = true;
			linkPackage3.PackQty = 3;

			AssertEquals("Expected filled TotalPackagesNum with full packages", 12, wrapper.TotalPackagesNum);
		});
	}

	public void TestTotalPackagesNum_WithVehicles()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty TotalPackagesNum", 0, wrapper.TotalPackagesNum);

			var declarationBill = declaration.Bills.AddNew();
			var billPackingGroup = (EU.Business.Declaration.PackingGroup)declarationBill.PackingGroups.AddNew();
			var package1 = declaration.Packages.AddNew();
			package1.CW_CR_HouseContainer = billPackingGroup.PK;
			package1.CW_PackType = "BX";
			var package2 = declaration.Packages.AddNew();
			package2.CW_CR_HouseContainer = billPackingGroup.PK;
			package2.CW_PackType = EU.Business.UniversalReferenceConstants.RefCusCodeUnPackedPackageUnitType.Unpacked;
			var package3 = declaration.Packages.AddNew();
			package3.CW_CR_HouseContainer = billPackingGroup.PK;
			package3.CW_PackType = PackageType.Frame;

			var collection1 = (InvoiceLineCusLinkPackageCollection)invoiceLine.PackagesForInvoiceLinesForBindingOnly;

			var linkPackage1 = collection1.AddNew();
			linkPackage1.Package = package1;
			linkPackage1.IsLinked = true;
			linkPackage1.PackQty = 5;

			var linkPackage2 = collection1.AddNew();
			linkPackage2.Package = package2;
			linkPackage2.IsLinked = true;
			linkPackage2.PackQty = 4;

			var linkPackage3 = collection1.AddNew();
			linkPackage3.Package = package3;
			linkPackage3.IsLinked = true;
			linkPackage3.PackQty = 0;

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			var vehicle2 = invoiceLine2.Vehicles.AddNew();
			invoiceLine2.JI_Tariff = "2203001011";
			vehicle2.CVH_VehicleIdentificationNumber = "VIN1";

			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			var vehicle3 = invoiceLine3.Vehicles.AddNew();
			invoiceLine3.JI_Tariff = "2203001012";
			vehicle3.CVH_VehicleIdentificationNumber = "VIN2";

			var invoiceLine4 = invoiceHeader.InvoiceLines.AddNew();
			var vehicle4 = invoiceLine4.Vehicles.AddNew();
			invoiceLine4.JI_Tariff = "2203001012";
			vehicle4.CVH_VehicleIdentificationNumber = "VIN2";

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge", true, mergeResult);

			var entryHeader = declaration.CustomsEntryHeaders[0];
			wrapper = GetWrapper(entryHeader);

			AssertEquals("Expected filled TotalPackagesNum with full packages", 12, wrapper.TotalPackagesNum);
		});
	}

	public void TestTotalPackagesNum_OnlyVehicles()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty TotalPackagesNum", 0, wrapper.TotalPackagesNum);

			invoiceLine.JI_Tariff = "2203001011";
			var vehicle = invoiceLine.Vehicles.AddNew();
			vehicle.CVH_VehicleIdentificationNumber = "VIN1";

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			var vehicle2 = invoiceLine2.Vehicles.AddNew();
			invoiceLine2.JI_Tariff = "2203001011";
			vehicle2.CVH_VehicleIdentificationNumber = "VIN2";

			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			var vehicle3 = invoiceLine3.Vehicles.AddNew();
			invoiceLine3.JI_Tariff = "2203001012";
			vehicle3.CVH_VehicleIdentificationNumber = "VIN3";

			var invoiceLine4 = invoiceHeader.InvoiceLines.AddNew();
			var vehicle4 = invoiceLine4.Vehicles.AddNew();
			invoiceLine4.JI_Tariff = "2203001012";
			vehicle4.CVH_VehicleIdentificationNumber = "VIN3";

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge", true, mergeResult);

			var entryHeader = declaration.CustomsEntryHeaders[0];
			wrapper = GetWrapper(entryHeader);

			AssertEquals("Expected filled TotalPackagesNum with full packages", 4, wrapper.TotalPackagesNum);
		});
	}

	public void TestCommercialReference()
	{
		declaration.JE_OwnerRef = HeaderData.OwnerRef;
		AssertEquals("Expected filled CommercialReference", HeaderData.OwnerRef, wrapper.CommercialReference);
	}

	public void TestNullImporter()
	{
		AssertExceptionThrown<NullReferenceException>(() => wrapper.Importer.ToString());
	}

	public void TestImporter()
	{
		var orgHeader = Factory.New<OrgHeader>();
		var orgAddressMain = orgHeader.MainAddress;
		orgAddressMain.OA_Address1 = "Address Main";

		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address Other";

		declaration.ImporterDocumentaryAddress.E2_OA_Address = orgAddress.PK;
		declaration.ImporterDocumentaryAddress.OrganisationPK = orgHeader.PK;

		var importer = wrapper.Importer;
		CombineAssertions(() =>
		{
			AssertNotNull("Expected filled Importer", importer);
			AssertSame("Cached Importer", wrapper.Importer, importer);

			AssertEquals("Expected filled Address correctly", "Address Other", importer.Address);

			declaration.ImporterDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
			declaration.JE_OH_Importer = orgHeader.PK;
			wrapper = GetWrapper(entryHeader);
			AssertEquals("Expected filled Address correctly whena ddress not specified", "Address Main", wrapper.Importer.Address);
		});
	}

	public void TestNullDeclarant()
	{
		declaration.Declarant.OA_OH = ZGuid.Empty;
		AssertExceptionThrown<NullReferenceException>(() => wrapper.Declarant.ToString());
	}

	public void TestDeclarant()
	{
		var orgHeader = Factory.New<OrgHeader>();
		declaration.Declarant.OA_OH = orgHeader.PK;
		var declarant = wrapper.Declarant;
		CombineAssertions(() =>
		{
			AssertNotNull("Expected filled Declarant", declarant);
			AssertSame("Cached Declarant", wrapper.Declarant, declarant);
		});
	}

	public void TestDeclarationEmail_CustomsClearanceEmailRecipient()
	{
		const string clearanceEmail = "mail1.mail@mail.com";
		const string mailboxEmail = "mail2.mail@mail.com";

		using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(clearanceEmail))
		using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(mailboxEmail))
		{
			AssertEquals("Expected filled DeclarationEmail with clearance email recipient when filled", clearanceEmail, wrapper.DeclarationEmail);
		}
	}

	public void TestDeclarationEmail_MailboxEmailAddress()
	{
		const string mailboxEmail = "mail2.mail@mail.com";

		using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(string.Empty))
		using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(mailboxEmail))
		{
			AssertEquals("Expected filled DeclarationEmail with mailbox email address when filled and clearance email recipient is empty", mailboxEmail, wrapper.DeclarationEmail);
		}
	}

	public void TestDeclarationEmail_Empty()
	{
		using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(string.Empty))
		using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(string.Empty))
		{
			AssertEquals("Expected empty DeclarationEmail when clearance email recipient and mailbox email address are empty", ZString.Empty, wrapper.DeclarationEmail);
		}
	}

	public void TestOtherEmail()
	{
		declaration.ZG_OtherEmailAddr = HeaderData.OtherEmail;
		AssertEquals("Expected filled OtherEmail", HeaderData.OtherEmail, wrapper.OtherEmail);
	}

	public void TestOriginCountry()
	{
		declaration.JE_GoodsOrigin = HeaderData.CountryOfOrigin;
		AssertEquals("Expected filled OriginCountry", HeaderData.CountryOfOrigin, wrapper.OriginCountry);
	}

	public void TestIsContainerised()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected false IsContainerised", false, wrapper.IsContainerised);

			var containerTag = "CONTAINER";
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = containerTag;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(containerTag).IsForInvoiceLine = true;
			AssertEquals("Expected true IsContainerised", true, wrapper.IsContainerised);
		});
	}

	public void TestCurrencyCode()
	{
		invoiceHeader.JZ_RX_NKInvoice_Currency = HeaderData.TotalAmountCurrency;
		AssertEquals("Expected filled CurrencyCode", HeaderData.TotalAmountCurrency, wrapper.CurrencyCode);
	}

	public void TestGoodsLocation()
	{
		CombineAssertions(() =>
		{
			entryInstruction.GoodsLocation.Address.AuthorisationNumber = "9999000002";
			AssertEquals("Expected filled GoodsLocation", "ES009999000002", wrapper.GoodsLocation);

			entryInstruction.GoodsLocation.Address.AuthorisationNumber = ZString.Empty;
			AssertEquals("Expected empty GoodsLocation", ZString.Empty, wrapper.GoodsLocation);

			entryInstruction.GoodsLocation.Address.AuthorisationNumber = "JPB009999000002";
			AssertEquals("Expected filled GoodsLocation Long", "JPB009999000002", wrapper.GoodsLocation);
		});
	}

	public void TestTotalTributesAmount()
	{
		var entryLine = entryHeader.MergedLines[0];
		var fee1 = entryLine.Fees.AddNew();
		fee1.CF_ChargeAmount = HeaderData.TotalTributesAmount / 4;
		var fee2 = entryLine.Fees.AddNew();
		fee2.CF_ChargeAmount = HeaderData.TotalTributesAmount / 4;

		entryHeader.MergedLines.AddNew();
		var fee3 = entryLine.Fees.AddNew();
		fee3.CF_ChargeAmount = HeaderData.TotalTributesAmount / 4;
		var fee4 = entryLine.Fees.AddNew();
		fee4.CF_ChargeAmount = HeaderData.TotalTributesAmount / 4;

		AssertEquals("Expected filled TotalTributesAmount", HeaderData.TotalTributesAmount, wrapper.TotalTributesAmount);
	}

	public void TestPaymentMode()
	{
		invoiceLine.ZG_MethodOfPayment = HeaderData.PaymentMode;
		AssertEquals("Expected filled PaymentMode", HeaderData.PaymentMode, wrapper.PaymentMode);
	}

	public void TestClearanceGuarantee()
	{
		CombineAssertions(() =>
		{
			entryInstruction.CEI_SubStyle = HeaderData.EntryInstructionSubStyle;

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = HeaderData.EntryInstructionSubStyle2;

			var guarantee = declaration.Guarantees.AddNew();
			guarantee.EntryInstructionID = entryInstruction2.PK;
			guarantee.PW_BondNumber = HeaderData.GuaranteeReferenceClearance;

			AssertEquals("Expected ClearanceGuarantee is empty when guarantee entry instruction is not the same as the entry instruction in invoice lines", ZString.Empty, wrapper.ClearanceGuarantee);

			guarantee.EntryInstructionID = entryInstruction.PK;
			AssertEquals("Expected ClearanceGuarantee is empty when guarantee entry instruction is the same as the entry instruction in invoice lines but guarantees type is not A", ZString.Empty, wrapper.ClearanceGuarantee);

			guarantee.PW_BondType = "A";
			AssertEquals("Expected ClearanceGuarantee is filled when guarantee entry instruction is the same as the entry instruction in invoice lines and guarantees type is A", HeaderData.GuaranteeReferenceClearance, wrapper.ClearanceGuarantee);

			var guarantee2 = declaration.Guarantees.AddNew();
			guarantee2.EntryInstructionID = entryInstruction2.PK;
			AssertEquals("Expected ClearanceGuarantee is filled when guarantee entry instruction is the same as the entry instruction in invoice lines and guarantees type is A (when there are more than one guarantees)", HeaderData.GuaranteeReferenceClearance, wrapper.ClearanceGuarantee);
		});
	}

	public void TestPendenciesGuarantee()
	{
		CombineAssertions(() =>
		{
			entryInstruction.CEI_SubStyle = HeaderData.EntryInstructionSubStyle;

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = HeaderData.EntryInstructionSubStyle2;

			var guarantee = declaration.Guarantees.AddNew();
			guarantee.EntryInstructionID = entryInstruction2.PK;
			guarantee.PW_BondNumber = HeaderData.GuaranteeReferencePendencies;

			AssertEquals("Expected ClearanceGuarantee is empty when guarantee entry instruction is not the same as the entry instruction in invoice lines", ZString.Empty, wrapper.PendenciesGuarantee);

			guarantee.EntryInstructionID = entryInstruction.PK;
			AssertEquals("Expected ClearanceGuarantee is empty when guarantee entry instruction is the same as the entry instruction in invoice lines but guarantees type is not P", ZString.Empty, wrapper.PendenciesGuarantee);

			guarantee.PW_BondType = "P";
			AssertEquals("Expected ClearanceGuarantee is filled when guarantee entry instruction is the same as the entry instruction in invoice lines and guarantees type is P", HeaderData.GuaranteeReferencePendencies, wrapper.PendenciesGuarantee);

			var guarantee2 = declaration.Guarantees.AddNew();
			guarantee2.EntryInstructionID = entryInstruction2.PK;
			AssertEquals("Expected ClearanceGuarantee is filled when guarantee entry instruction is the same as the entry instruction in invoice lines and guarantees type is P (when there are more than one guarantees)", HeaderData.GuaranteeReferencePendencies, wrapper.PendenciesGuarantee);
		});
	}

	public void TestGRNGuarantees()
	{
		CombineAssertions(() =>
		{
			entryInstruction.CEI_SubStyle = HeaderData.EntryInstructionSubStyle;

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = HeaderData.EntryInstructionSubStyle2;

			var guarantee = declaration.Guarantees.AddNew();
			guarantee.EntryInstructionID = entryInstruction2.PK;
			guarantee.PW_BondNumber = GRNGuaranteesCanCodes[0];
			guarantee.PW_BondType = "A";

			wrapper = GetWrapper(entryHeader);
			AssertEquals("Expected GRNGuarantees is empty when guarantee entry instruction is not the same as the entry instruction in invoice lines", 0, wrapper.GRNGuarantees.Count);

			guarantee.EntryInstructionID = entryInstruction.PK;
			wrapper = GetWrapper(entryHeader);
			AssertEquals("Expected GRNGuarantees is empty when guarantee entry instruction is the same as the entry instruction in invoice lines but guarantees type is not empty", 0, wrapper.GRNGuarantees.Count);

			guarantee.PW_BondType = ZString.Empty;
			wrapper = GetWrapper(entryHeader);
			AssertEquals("No empty GRNGuarantees when guarantee entry instruction is the same as the entry instruction in invoice lines and guarantees type is empty and guarantees reference doesn't have A in the 5th character", 0, wrapper.GRNGuarantees.Count);

			guarantee.PW_BondNumber = GRNGuaranteesCodes[0];
			wrapper = GetWrapper(entryHeader);
			AssertEquals("No empty GRNGuarantees when guarantee entry instruction is the same as the entry instruction in invoice lines and guarantees type is empty and guarantees reference has A in the 5th character", GRNGuaranteesCodes[0], wrapper.GRNGuarantees.ToArray()[0]);

			declaration.Guarantees.RemoveAndDeleteAll();

			foreach (var guaranteeCode in GRNGuaranteesCodes)
			{
				var gua = declaration.Guarantees.AddNew();
				gua.EntryInstructionID = entryInstruction.PK;
				gua.PW_BondNumber = guaranteeCode;
			}

			foreach (var guaranteeCanCode in GRNGuaranteesCanCodes)
			{
				var gua = declaration.Guarantees.AddNew();
				gua.EntryInstructionID = entryInstruction.PK;
				gua.PW_BondNumber = guaranteeCanCode;
			}

			wrapper = GetWrapper(entryHeader);
			var grnGuarantees = wrapper.GRNGuarantees;

			AssertArrayEqualsByElements("No empty GRNGuarantees when guarantee entry instruction is the same as the entry instruction in invoice lines and guarantees type is empty and guarantees reference has A in the 5th character", GRNGuaranteesCodes.ToArray(), grnGuarantees.ToArray());
			AssertSame("Cached GRNGuarantees", wrapper.GRNGuarantees, grnGuarantees);
		});
	}

	public void TestPaymentModeCan()
	{
		var helper = new ESUniversalReferenceTestDataHelper(Factory);
		helper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, "61", "Test 61");

		declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];
		invoiceLine.ZG_MethodOfPayment2 = HeaderData.PaymentMode;
		AssertEquals("Expected filled PaymentModeCan", HeaderData.PaymentMode, wrapper.PaymentModeCan);

		declaration.ZG_DestinationState = "ZZ";
		invoiceLine.ZG_MethodOfPayment2 = HeaderData.PaymentMode;
		AssertEquals("Expected empty PaymentModeCan when DestinationState is not CanaryIsland", ZString.Empty, wrapper.PaymentModeCan);
	}

	public void TestClearanceGuaranteeCan()
	{
		CombineAssertions(() =>
		{
			entryInstruction.CEI_SubStyle = HeaderData.EntryInstructionSubStyle;

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = HeaderData.EntryInstructionSubStyle2;

			var guarantee = declaration.Guarantees.AddNew();
			guarantee.EntryInstructionID = entryInstruction2.PK;
			guarantee.PW_BondNumber = HeaderData.GuaranteeReferenceClearanceCan;

			AssertEquals("Expected ClearanceGuaranteeCan is empty when guarantee entry instruction is not the same as the entry instruction in invoice lines", ZString.Empty, wrapper.ClearanceGuaranteeCan);

			guarantee.EntryInstructionID = entryInstruction.PK;
			AssertEquals("Expected ClearanceGuaranteeCan is empty when guarantee entry instruction is the same as the entry instruction in invoice lines but guarantees type is not C", ZString.Empty, wrapper.ClearanceGuaranteeCan);

			guarantee.PW_BondType = "C";
			AssertEquals("Expected ClearanceGuaranteeCan is filled when guarantee entry instruction is the same as the entry instruction in invoice lines and guarantees type is C", HeaderData.GuaranteeReferenceClearanceCan, wrapper.ClearanceGuaranteeCan);

			var guarantee2 = declaration.Guarantees.AddNew();
			guarantee2.EntryInstructionID = entryInstruction2.PK;
			AssertEquals("Expected ClearanceGuaranteeCan is filled when guarantee entry instruction is the same as the entry instruction in invoice lines and guarantees type is C (when there are more than one guarantees)", HeaderData.GuaranteeReferenceClearanceCan, wrapper.ClearanceGuaranteeCan);
		});
	}

	public void TestPendenciesGuaranteeCan()
	{
		CombineAssertions(() =>
		{
			entryInstruction.CEI_SubStyle = HeaderData.EntryInstructionSubStyle;

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = HeaderData.EntryInstructionSubStyle2;

			var guarantee = declaration.Guarantees.AddNew();
			guarantee.EntryInstructionID = entryInstruction2.PK;
			guarantee.PW_BondNumber = HeaderData.GuaranteeReferencePendenciesCan;

			AssertEquals("Expected ClearanceGuaranteeCan is empty when guarantee entry instruction is not the same as the entry instruction in invoice lines", ZString.Empty, wrapper.PendenciesGuaranteeCan);

			guarantee.EntryInstructionID = entryInstruction.PK;
			AssertEquals("Expected ClearanceGuaranteeCan is empty when guarantee entry instruction is the same as the entry instruction in invoice lines but guarantees type is not T", ZString.Empty, wrapper.PendenciesGuaranteeCan);

			guarantee.PW_BondType = "T";
			AssertEquals("Expected ClearanceGuaranteeCan is filled when guarantee entry instruction is the same as the entry instruction in invoice lines and guarantees type is T", HeaderData.GuaranteeReferencePendenciesCan, wrapper.PendenciesGuaranteeCan);

			var guarantee2 = declaration.Guarantees.AddNew();
			guarantee2.EntryInstructionID = entryInstruction2.PK;
			AssertEquals("Expected ClearanceGuaranteeCan is filled when guarantee entry instruction is the same as the entry instruction in invoice lines and guarantees type is T (when there are more than one guarantees)", HeaderData.GuaranteeReferencePendenciesCan, wrapper.PendenciesGuaranteeCan);
		});
	}

	public void TestGRNGuaranteesCan()
	{
		CombineAssertions(() =>
		{
			entryInstruction.CEI_SubStyle = HeaderData.EntryInstructionSubStyle;

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = HeaderData.EntryInstructionSubStyle2;

			var guarantee = declaration.Guarantees.AddNew();
			guarantee.EntryInstructionID = entryInstruction2.PK;
			guarantee.PW_BondNumber = GRNGuaranteesCodes[0];
			guarantee.PW_BondType = "A";

			wrapper = GetWrapper(entryHeader);
			AssertEquals("Expected GRNGuaranteesCan is empty when guarantee entry instruction is not the same as the entry instruction in invoice lines", 0, wrapper.GRNGuaranteesCan.Count);

			guarantee.EntryInstructionID = entryInstruction.PK;
			wrapper = GetWrapper(entryHeader);
			AssertEquals("Expected GRNGuaranteesCan is empty when guarantee entry instruction is the same as the entry instruction in invoice lines but guarantees type is not empty", 0, wrapper.GRNGuaranteesCan.Count);

			guarantee.PW_BondType = ZString.Empty;
			wrapper = GetWrapper(entryHeader);
			AssertEquals("No empty GRNGuaranteesCan when guarantee entry instruction is the same as the entry instruction in invoice lines and guarantees type is empty and guarantees reference doesn't have C in the 5th character", 0, wrapper.GRNGuaranteesCan.Count);

			guarantee.PW_BondNumber = GRNGuaranteesCanCodes[0];
			wrapper = GetWrapper(entryHeader);
			AssertEquals("No empty GRNGuaranteesCan when guarantee entry instruction is the same as the entry instruction in invoice lines and guarantees type is empty and guarantees reference has C in the 5th character", GRNGuaranteesCanCodes[0], wrapper.GRNGuaranteesCan.ToArray()[0]);

			declaration.Guarantees.RemoveAndDeleteAll();

			foreach (var guaranteeCode in GRNGuaranteesCodes)
			{
				var gua = declaration.Guarantees.AddNew();
				gua.EntryInstructionID = entryInstruction.PK;
				gua.PW_BondNumber = guaranteeCode;
			}

			foreach (var guaranteeCanCode in GRNGuaranteesCanCodes)
			{
				var gua = declaration.Guarantees.AddNew();
				gua.EntryInstructionID = entryInstruction.PK;
				gua.PW_BondNumber = guaranteeCanCode;
			}

			wrapper = GetWrapper(entryHeader);
			var grnGuaranteesCan = wrapper.GRNGuaranteesCan;

			AssertArrayEqualsByElements("No empty GRNGuaranteesCan when guarantee entry instruction is the same as the entry instruction in invoice lines and guarantees type is empty and guarantees reference has C in the 5th character", GRNGuaranteesCanCodes.ToArray(), grnGuaranteesCan.ToArray());
			AssertSame("Cached GRNGuaranteesCan", wrapper.GRNGuaranteesCan, grnGuaranteesCan);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.FillWithValidTestData();
		declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.InvoiceLines.AddNew();

		var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge", true, mergeResult);

		entryHeader = declaration.CustomsEntryHeaders[0];

		wrapper = GetWrapper(entryHeader);
	}

	protected JobDeclaration declaration;
	protected CusEntryInstruction entryInstruction;
	protected JobComInvoiceHeader invoiceHeader;
	protected JobComInvoiceLine invoiceLine;
	protected CusEntryHeader entryHeader;
	DUAImportCommonHeaderWrapper wrapper;

	protected virtual DUAImportCommonHeaderWrapper GetWrapper(CusEntryHeader cusEntryHeader) => new DUAImportCommonHeaderWrapper(cusEntryHeader);

	protected override DUAImportCommonHeaderWrapper GetProvider() => wrapper;
}
