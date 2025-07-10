using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business.Documents.DocDataObjects;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;
using static Enterprise.Customs.ES.Business.ESConstants;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;
using static Enterprise.Customs.EU.Business.TemporaryStorageHelper;
using CusTempStorageRegHeader = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader;
using CusTempStorageRegLine = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine;
using CusTempStorageRegLineItem = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineItem;
using CusTempStorageRegLineItemPivot = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot;
using CusTempStorageRegLineTransaction = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineTransaction;
using CusTempStorageRegLineTransactionStatusList = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionStatusList;
using CusTempStorageRegLineTransactionTypeList = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionTypeList;
using CusTempStorageRegPremises = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegPremises;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;
using PackageType = Enterprise.Customs.ES.Business.UniversalReferenceConstants.RefCusCodeList.PackageType;

namespace Enterprise.Customs.ES.Business.Declaration.Testing;

[TestedType(typeof(CusEntryHeader))]
sealed class CusEntryHeaderTest : EU.Business.Declaration.Testing.CusEntryHeaderTest<CusEntryHeader>
{
	public void TestUpdateHightestLineNumber_HasBeenLodgedAtCustoms()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		var entryLine1 = entryHeader.MergedLines.AddNew();
		entryLine1.CL_LineNumber = 1;
		CombineAssertions(() =>
		{
			entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.AwaitingChange;
			AssertEquals("LockNumberOfEntryLines false", (ZShort)0, entryHeader.CH_HighestLineNumber);

			foreach (var entryStatus in entryHeader.LodgedAtCustomsEntryStatus)
			{
				entryHeader.CH_EntryStatus = entryStatus;
				AssertEquals($"LockNumberOfEntryLines true as HasBeenLodgedAtCustoms; EntryStatus {entryStatus}", (ZShort)1, entryHeader.CH_HighestLineNumber);
			}
		});
	}

	public void TestUpdateHightestLineNumber_IsWaitingForResponse()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		var entryLine1 = entryHeader.MergedLines.AddNew();
		entryLine1.CL_LineNumber = 1;
		CombineAssertions(() =>
		{
			entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.AwaitingChange;
			AssertEquals("LockNumberOfEntryLines false", (ZShort)0, entryHeader.CH_HighestLineNumber);

			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingResponse;
			AssertEquals($"LockNumberOfEntryLines true as IsWaitingForResponse", (ZShort)1, entryHeader.CH_HighestLineNumber);

			entryHeader.CH_Status = MessageStatusList.Codes.OK;
			AssertEquals("LockNumberOfEntryLines false again", (ZShort)0, entryHeader.CH_HighestLineNumber);
		});
	}

	public void TestCommonGoodsItemsIntegratorCore()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		AssertType<ESCommonGoodsItemsIntegrator>(entryHeader.CommonGoodsItemsIntegrator);
	}

	public void TestLookups()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		AssertType<CusEntryHeaderLookups>(entryHeader.Lookups);
	}

	public void TestRandomHeader()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		AssertType<JobComInvoiceHeader>(entryHeader.RandomHeader);
	}

	public void TestShouldAnnexesBeReadOnly()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		entryHeader.ZG_RequestDispatch = Customs.Business.YesNoList.Codes.Yes;
		AssertEquals("ShouldAnnexesBeReadOnly when ZG_RequestDispatch = Yes", true, entryHeader.ShouldAnnexesBeReadOnly);
		entryHeader.ZG_RequestDispatch = Customs.Business.YesNoList.Codes.No;
		AssertEquals("ShouldAnnexesBeReadOnly when ZG_RequestDispatch = No", false, entryHeader.ShouldAnnexesBeReadOnly);
	}

	public void TestTotalGrossWeightInKG()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entryHeader.MergedLines.AddNew();
		var entryLine2 = entryHeader.MergedLines.AddNew();

		var invoiceLine1 = entryLine1.InvoiceLines.AddNew();
		invoiceLine1.JI_WeightUQ = Core.Constants.Weight.Grams;
		invoiceLine1.JI_Weight = 100000.00m;
		invoiceLine1.JI_CL = entryLine1.PK;

		var invoiceLine2 = entryLine1.InvoiceLines.AddNew();
		invoiceLine2.JI_WeightUQ = Core.Constants.Weight.Milligrams;
		invoiceLine2.JI_Weight = 200200000.00m;
		invoiceLine2.JI_CL = entryLine1.PK;

		var invoiceLine3 = entryLine2.InvoiceLines.AddNew();
		invoiceLine3.JI_WeightUQ = Core.Constants.Weight.Hectograms;
		invoiceLine3.JI_Weight = 2002.00m;
		invoiceLine3.JI_CL = entryLine2.PK;

		var invoiceLine4 = entryLine2.InvoiceLines.AddNew();
		invoiceLine4.JI_WeightUQ = Core.Constants.Weight.Kilograms;
		invoiceLine4.JI_Weight = 500.20m;
		invoiceLine4.JI_CL = entryLine2.PK;
		CombineAssertions(() =>
		{
			entryHeader.ZG_UCC6Version = UCC6VersionCodes.NoUCC6;
			AssertEquals("Total Gross Weight must be equal to sum of Merged Entry lines ceiling gross Weight", 1002m, entryHeader.TotalGrossWeightInKG);

			entryHeader.ZG_UCC6Version = UCC6VersionCodes.UCC6;
			AssertEquals("Total Gross Weight must be equal to sum of Merged Entry lines gross Weight", 1000.60m, entryHeader.TotalGrossWeightInKG);
		});
	}

	public void TestInvoiceAmount()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		var invoiceHeader1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();

		var invoiceLine2 = invoiceHeader1.InvoiceLines.AddNew();

		var entryHeader1 = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		var entryLine1 = entryHeader1.AllEntryLines.AddNew();
		entryLine1.CL_LineNumber = 1;
		invoiceLine1.JI_CL = entryLine1.PK;
		invoiceLine2.JI_CL = entryLine1.PK;

		invoiceLine1.JI_LinePrice = 200;
		invoiceLine2.JI_LinePrice = 300;

		AssertEquals("The sum of JI_LinePrice of 2 invLines is expected", new ZDecimal(500), entryHeader1.InvoiceAmount);
	}

	public void TestInvoiceAmountCurrency()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		var invoiceHeader1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
		var entryHeader1 = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		var entryLine1 = entryHeader1.AllEntryLines.AddNew();
		invoiceLine1.JI_CL = entryLine1.PK;

		CombineAssertions(() =>
		{
			invoiceHeader1.JZ_RX_NKInvoice_Currency = "EUR";

			AssertEquals("The InvoiceHeader Currency is expected: EUR", "EUR", entryHeader1.InvoiceAmountCurrency);

			invoiceHeader1.JZ_RX_NKInvoice_Currency = "USD";

			AssertEquals("The InvoiceHeader Currency is expected: USD", "USD", entryHeader1.InvoiceAmountCurrency);
		});
	}

	public void TestShouldLogEntryStatus()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		AssertEquals("ShouldLogEntryStatus", true, entryHeader.ShouldLogEntryStatus);
	}

	public void TestSealCodes()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		var invoiceHeader1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();

		var invoiceHeader2 = declaration.Invoices.AddNew();
		var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
		var invoiceLine3 = invoiceHeader2.InvoiceLines.AddNew();

		var entryHeader1 = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		var entryLine1 = entryHeader1.AllEntryLines.AddNew();
		entryLine1.CL_LineNumber = 1;
		invoiceLine1.JI_CL = entryLine1.PK;

		var entryHeader2 = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		var entryLine2 = entryHeader2.AllEntryLines.AddNew();
		entryLine2.CL_LineNumber = 2;
		invoiceLine2.JI_CL = entryLine2.PK;
		invoiceLine3.JI_CL = entryLine2.PK;

		var container1 = declaration.CusContainers.AddNew();
		container1.CO_ContainerNumber = "CTN1";
		container1.CO_Seal = "S01";
		container1.CO_SecondSeal = "S21";
		container1.AdditionalSeals.AddNew().BK_SealNumber = "A01";
		container1.AdditionalSeals.AddNew().BK_SealNumber = "A02";

		var container2 = declaration.CusContainers.AddNew();
		container2.CO_ContainerNumber = "CTN2";
		container2.CO_Seal = "S02";
		container2.CO_SecondSeal = "S22";
		container2.AdditionalSeals.AddNew().BK_SealNumber = "A03";

		var container3 = declaration.CusContainers.AddNew();
		container3.CO_ContainerNumber = "CTN3";
		container3.CO_Seal = "S03";
		container3.CO_SecondSeal = "S23";
		container3.AdditionalSeals.AddNew().BK_SealNumber = "A04";

		var container4 = declaration.CusContainers.AddNew();
		container4.CO_ContainerNumber = "CTN4";
		container4.CO_Seal = ZString.Empty;
		container4.CO_SecondSeal = ZString.Empty;
		container4.AdditionalSeals.AddNew().BK_SealNumber = "";

		invoiceLine1.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("CTN1").IsForInvoiceLine = true;
		invoiceLine1.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("CTN2").IsForInvoiceLine = true;
		invoiceLine3.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("CTN3").IsForInvoiceLine = true;
		invoiceLine3.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("CTN4").IsForInvoiceLine = true;

		CombineAssertions(() =>
		{
			AssertEquals("Expected 7 SealCodes in entry header 1 when seals declared are in containers", 7, entryHeader1.SealCodes.Count());
			AssertContainsExactElementsInAnyOrder("Seal codes expected in entry header 1", new ZString[] { "S01", "S02", "S21", "S22", "A01", "A02", "A03" }, entryHeader1.SealCodes.ToArray());

			AssertEquals("Expected 3 SealCodes in entry header 2 when seals declared are in containers", 3, entryHeader2.SealCodes.Count());
			AssertContainsExactElementsInAnyOrder("Seal codes expected in entry header 2", new ZString[] { "S03", "S23", "A04" }, entryHeader2.SealCodes.ToArray());

			var packingGroups = declaration.Bills.AddNew().PackingGroups.AddNew();

			var equipment1 = declaration.Equipments.AddNew();
			equipment1.CEQ_IdentificationNumber = "EQUIP1";
			equipment1.Seals.AddNew().BK_SealNumber = "E01";
			var pack1 = packingGroups.Packages.AddNew();
			pack1.CW_PackQty = 1;
			pack1.CW_ContainerNoOrEquipmentNo = equipment1.CEQ_IdentificationNumber;

			var equipment2 = declaration.Equipments.AddNew();
			equipment2.CEQ_IdentificationNumber = "EQUIP2";
			equipment2.Seals.AddNew().BK_SealNumber = "E02";
			equipment2.Seals.AddNew().BK_SealNumber = "E03";
			var pack2 = packingGroups.Packages.AddNew();
			pack2.CW_PackQty = 1;
			pack2.CW_ContainerNoOrEquipmentNo = equipment2.CEQ_IdentificationNumber;

			var equipment3 = declaration.Equipments.AddNew();
			equipment3.CEQ_IdentificationNumber = "EQUIP3";
			equipment3.Seals.AddNew().BK_SealNumber = "";
			var pack3 = packingGroups.Packages.AddNew();
			pack3.CW_PackQty = 1;
			pack3.CW_ContainerNoOrEquipmentNo = equipment3.CEQ_IdentificationNumber;

			invoiceLine1.PackagesPivot.AddPivotFor(pack1);
			invoiceLine2.PackagesPivot.AddPivotFor(pack2);
			invoiceLine2.PackagesPivot.AddPivotFor(pack3);

			AssertEquals("Expected 8 SealCodes in entry header 1 when seals declared are in containers and equipments", 8, entryHeader1.SealCodes.Count());
			AssertContainsExactElementsInAnyOrder("Seal codes expected in entry header 1", new ZString[] { "S01", "S02", "S21", "S22", "A01", "A02", "A03", "E01" }, entryHeader1.SealCodes.ToArray());

			AssertEquals("Expected 5 SealCodes in entry header 2 when seals declared are in containers and equipments", 5, entryHeader2.SealCodes.Count());
			AssertContainsExactElementsInAnyOrder("Seal codes expected in entry header 2", new ZString[] { "S03", "S23", "A04", "E02", "E03" }, entryHeader2.SealCodes.ToArray());
		});
	}

	public override void TestOfficeOfEntry()
	{
		var declaration = (JobDeclaration)GetNewDeclaration();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var entry = declaration.CustomsEntryHeaders.AddNew();
		declaration.CustomsOffices.RemoveAndDeleteAll();
		declaration.JE_CustomsOffice = "LV001000";
		declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, "LV002000");
		AssertEquals("LV002000", declaration.OfficeOfEntry);
		AssertEquals("LV002000", entry.OfficeOfEntry);
	}

	public void TestMRNPropertyIsEditableWhenInstructionIsT2CAndStatusIsEmpty()
	{
		var entryHeader = Factory.New<CusEntryHeader>();

		var entryInstruction = Factory.New<CusEntryInstruction>();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
		entryHeader.CH_EntryStatus = "";
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		CombineAssertions(() =>
		{
			AssertEquals("MRN must be editable when EntryInstruction is T2C", entryHeader.MovementReferenceNumberInfo.ReadOnly, false);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			AssertEquals("MRN must not be editable when EntryInstruction is not T2C", entryHeader.MovementReferenceNumberInfo.ReadOnly, true);
		});
	}

	public void TestMRNPropertyIsEditableWhenInstructionIsT2CAndStatusIsError()
	{
		var entryHeader = Factory.New<CusEntryHeader>();

		var entryInstruction = Factory.New<CusEntryInstruction>();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
		entryHeader.CH_EntryStatus = MessageProcessorConstants.EntryStatusCodes.Error;
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		CombineAssertions(() =>
		{
			AssertEquals("MRN must be editable if EntryInstruction is T2C and status is Error", false, entryHeader.MovementReferenceNumberInfo.ReadOnly);

			entryHeader.CH_EntryStatus = EntryStatusCodeList.Clear;
			AssertEquals("MRN must not be editable if EntryInstruction is T2C and Entry Status in not Error or empty", true, entryHeader.MovementReferenceNumberInfo.ReadOnly);
		});
	}

	public void TestT2CMovementReferenceEntryNumber()
	{
		var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();

		CombineAssertions("Cus Entry number", () =>
		{
			AssertEquals("T2CMovementReferenceNumber should be empty", ZString.Empty, entryHeader.T2CMovementReferenceNumber);

			var cusEntryNumber = Factory.New<CusEntryNumber>();
			cusEntryNumber.CE_EntryType = "TMR";
			cusEntryNumber.CE_ParentID = entryHeader.PK;
			cusEntryNumber.CE_ParentTable = entryHeader.TableName;
			cusEntryNumber.CE_EntryNum = "1234";

			AssertEquals("T2CMovementReferenceNumber should be filled", "1234", entryHeader.T2CMovementReferenceNumber);
		});
	}

	public void TestICusStorageDocPivotTypeSupporter_ReloadCollection()
	{
		var dec = Factory.New<JobDeclaration>();
		var entryHeader = dec.CustomsEntryHeaders.AddNew();
		_ = entryHeader.EDocPivotCollection;
		var provider = (ICusStorageDocPivotTypeSupporter)entryHeader;
		var pivot = Factory.New<CusStorageDocPivot>();
		pivot.CSD_ParentID = entryHeader.PK;
		pivot.CSD_ParentTableCode = entryHeader.TablePrefix;
		provider.ReloadCollection();
		AssertEquals(1, entryHeader.EDocPivotCollection.Count);
	}

	public void TestIsOriginAndDestinationRequiredInItinerary()
	{
		var dec = Factory.New<JobDeclaration>();
		var entryInstruction = dec.CustomsEntryInstructions.AddNew();
		var entryHeader = dec.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		entryHeader.ZG_UCC6Version = 0;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
		AssertEquals("Should origin and dest appear in itinerary no EXS?", false, entryHeader.IsOriginAndDestinationRequiredInItinerary_ForTest);

		entryHeader.ZG_UCC6Version = 1;
		AssertEquals("Should origin and dest appear in itinerary no EXS with AES", true, entryHeader.IsOriginAndDestinationRequiredInItinerary_ForTest);

		dec.JE_MessageType = MessageTypeList.Codes.Import;
		AssertEquals("Should origin and dest appear in itinerary no EXS with AES and Import", false, entryHeader.IsOriginAndDestinationRequiredInItinerary_ForTest);

		entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
		AssertEquals("Should origin and dest appear in itinerary for EXS?", true, entryHeader.IsOriginAndDestinationRequiredInItinerary_ForTest);
	}

	public void TestCanRepeatCountriesInItinerary()
	{
		var dec = Factory.New<JobDeclaration>();
		var entryInstruction = dec.CustomsEntryInstructions.AddNew();
		var entryHeader = dec.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		entryHeader.ZG_UCC6Version = 0;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
		AssertEquals("Can repeat countries in itinerary no EXS?", false, entryHeader.CanRepeatCountriesInItinerary_ForTest);

		entryHeader.ZG_UCC6Version = 1;
		AssertEquals("Can repeat countries in itinerary no EXS with AES", true, entryHeader.CanRepeatCountriesInItinerary_ForTest);

		dec.JE_MessageType = MessageTypeList.Codes.Import;
		AssertEquals("Can repeat countries in itinerary no EXS with AES and Import", false, entryHeader.IsOriginAndDestinationRequiredInItinerary_ForTest);

		entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
		AssertEquals("Can repeat countries in itinerary for EXS?", true, entryHeader.CanRepeatCountriesInItinerary_ForTest);
	}

	public void TestFormattedCircuit()
	{
		var entry = Factory.New<CusEntryHeader>();

		entry.SetMovementReferenceNumberEntryStatus("4");
		AssertEquals("GREEN", entry.FormattedCircuit);
	}

	public void TestFormattedCircuitCan()
	{
		var entry = Factory.New<CusEntryHeader>();

		entry.SetCircuitCan("5");
		AssertEquals("RED", entry.FormattedCircuitCan);
	}

	public void TestFormattedClearanceResult()
	{
		var entry = Factory.New<CusEntryHeader>();

		entry.ZG_ClearanceResult = "A2";
		AssertEquals("[A2] Considered Satisfactory", entry.FormattedClearanceResult);
	}

	public void TestFormattedEADPrint()
	{
		var entry = Factory.New<CusEntryHeader>();

		entry.EUH_EADPrintProcedure = "1";
		AssertEquals("[1] EAD printed by Customs authorities or through the Virtual Office", entry.FormattedEADPrint);
	}

	public void TestTotalPackagesQty()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.FillWithValidTestData();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

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

		var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge", true, mergeResult);

		var entryHeader = declaration.CustomsEntryHeaders[0];

		AssertEquals("TotalPackagesQty is the sum of PackQty for not NE and FR packages", 5, entryHeader.TotalPackagesQty);
	}

	public void TestTotalPiecesQty()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.FillWithValidTestData();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

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

		var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge", true, mergeResult);

		var entryHeader = declaration.CustomsEntryHeaders[0];

		AssertEquals("TotalPiecesQty is the sum of PackQty for only NE packages", 4, entryHeader.TotalPiecesQty);
	}

	public void TestTotalPackagesQtyNonBulk()
	{
		Factory.SetBulkTypeHelper();
		var declaration = Factory.New<JobDeclaration>();
		declaration.FillWithValidTestData();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

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
		var package4 = declaration.Packages.AddNew();
		package4.CW_CR_HouseContainer = billPackingGroup.PK;
		package4.CW_PackType = "VG";

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

		var linkPackage4 = collection1.AddNew();
		linkPackage4.Package = package4;
		linkPackage4.IsLinked = true;
		linkPackage4.PackQty = 2;

		var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge", true, mergeResult);

		var entryHeader = declaration.CustomsEntryHeaders[0];

		AssertEquals("TotalPackagesQtyNonBulk is the sum of PackQty for non bulk (VS, VG, VL, VY,VR, VQ, VO) and no FR packages", 9, entryHeader.TotalPackagesQtyNonBulk);
	}

	public void TestTotalVehiclesQtyNoFR()
	{
		Factory.SetBulkTypeHelper();
		var declaration = Factory.New<JobDeclaration>();
		declaration.FillWithValidTestData();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		var vehicle1 = invoiceLine1.Vehicles.AddNew();
		vehicle1.CVH_VehicleIdentificationNumber = "VIN1";

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

		var invoiceLine5 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine5.JI_Tariff = "2203001013";

		var declarationBill = declaration.Bills.AddNew();
		var billPackingGroup = (EU.Business.Declaration.PackingGroup)declarationBill.PackingGroups.AddNew();

		var package = declaration.Packages.AddNew();
		package.CW_CR_HouseContainer = billPackingGroup.PK;
		package.CW_PackType = PackageType.Frame;

		var collection1 = (InvoiceLineCusLinkPackageCollection)invoiceLine5.PackagesForInvoiceLinesForBindingOnly;

		var linkPackage = collection1.AddNew();
		linkPackage.Package = package;
		linkPackage.IsLinked = true;
		linkPackage.PackQty = 0;

		var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge", true, mergeResult);

		var entryHeader = declaration.CustomsEntryHeaders[0];

		AssertEquals("If line packages FR is 0 TotalVehiclesQty is the sum of vehicles (invoice lines with vehicles)", 4, entryHeader.TotalVehiclesQty);
	}

	public void TestTotalVehiclesQtyFR()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.FillWithValidTestData();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		var vehicle1 = invoiceLine1.Vehicles.AddNew();
		vehicle1.CVH_VehicleIdentificationNumber = "VIN1";

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
		invoiceLine4.JI_Tariff = "2203001011";
		vehicle4.CVH_VehicleIdentificationNumber = "VIN3";

		var invoiceLine5 = invoiceHeader.InvoiceLines.AddNew();
		var vehicle5 = invoiceLine5.Vehicles.AddNew();
		invoiceLine5.JI_Tariff = "2203001013";
		vehicle5.CVH_VehicleIdentificationNumber = "VIN4";

		var declarationBill = declaration.Bills.AddNew();
		var billPackingGroup = (EU.Business.Declaration.PackingGroup)declarationBill.PackingGroups.AddNew();

		var package = declaration.Packages.AddNew();
		package.CW_CR_HouseContainer = billPackingGroup.PK;
		package.CW_PackType = PackageType.Frame;

		var collection1 = (InvoiceLineCusLinkPackageCollection)invoiceLine4.PackagesForInvoiceLinesForBindingOnly;

		var linkPackage = collection1.AddNew();
		linkPackage.Package = package;
		linkPackage.IsLinked = true;
		linkPackage.PackQty = 5;

		var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge", true, mergeResult);

		var entryHeader = declaration.CustomsEntryHeaders[0];

		AssertEquals("TotalVehiclesQty is the sum of Vehicles in Entry lines", 8, entryHeader.TotalVehiclesQty);
	}

	public void TestTotalBulkTypePackages()
	{
		Factory.SetBulkTypeHelper();
		Factory.SetBulkTypeHelper("VY");
		var declaration = Factory.New<JobDeclaration>();
		declaration.FillWithValidTestData();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

		var declarationBill = declaration.Bills.AddNew();
		var billPackingGroup = (EU.Business.Declaration.PackingGroup)declarationBill.PackingGroups.AddNew();
		var package1 = declaration.Packages.AddNew();
		package1.CW_CR_HouseContainer = billPackingGroup.PK;
		package1.CW_PackType = "VY";
		var package2 = declaration.Packages.AddNew();
		package2.CW_CR_HouseContainer = billPackingGroup.PK;
		package2.CW_PackType = EU.Business.UniversalReferenceConstants.RefCusCodeUnPackedPackageUnitType.Unpacked;
		var package3 = declaration.Packages.AddNew();
		package3.CW_CR_HouseContainer = billPackingGroup.PK;
		package3.CW_PackType = "FR";
		var package4 = declaration.Packages.AddNew();
		package4.CW_CR_HouseContainer = billPackingGroup.PK;
		package4.CW_PackType = "VG";

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

		var linkPackage4 = collection1.AddNew();
		linkPackage4.Package = package4;
		linkPackage4.IsLinked = true;
		linkPackage4.PackQty = 2;

		var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge", true, mergeResult);

		var entryHeader = declaration.CustomsEntryHeaders[0];

		AssertEquals("TotalBulkTypePackages is the number bulk (VS, VG, VL, VY,VR, VQ, VO) packages", 2, entryHeader.TotalBulkTypePackages);
	}

	public void TestPackagesCount()
	{
		Factory.SetBulkTypeHelper();
		var declaration = Factory.New<JobDeclaration>();
		declaration.FillWithValidTestData();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var vehicle = invoiceLine.Vehicles.AddNew();
		vehicle.CVH_VehicleIdentificationNumber = "VIN1";
		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		var vehicle2 = invoiceLine2.Vehicles.AddNew();
		vehicle2.CVH_VehicleIdentificationNumber = "VIN2";
		var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
		var invoiceLine4 = invoiceHeader.InvoiceLines.AddNew();
		var vehicle4 = invoiceLine4.Vehicles.AddNew();
		vehicle4.CVH_VehicleIdentificationNumber = "VIN2";

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
		package3.CW_PackType = EU.Business.UniversalReferenceConstants.RefCusCodeBulkPackageUnitType.BulkGas;
		var package4 = declaration.Packages.AddNew();
		package4.CW_CR_HouseContainer = billPackingGroup.PK;
		package4.CW_PackType = PackageType.Frame;

		var collection1 = (InvoiceLineCusLinkPackageCollection)invoiceLine3.PackagesForInvoiceLinesForBindingOnly;

		var linkPackage1 = collection1.AddNew();
		linkPackage1.Package = package1;
		linkPackage1.IsLinked = true;
		linkPackage1.PackQty = 1;

		var linkPackage2 = collection1.AddNew();
		linkPackage2.Package = package2;
		linkPackage2.IsLinked = true;
		linkPackage2.PackQty = 2;

		var linkPackage3 = collection1.AddNew();
		linkPackage3.Package = package3;
		linkPackage3.IsLinked = true;
		linkPackage3.PackQty = 0;

		var linkPackage4 = collection1.AddNew();
		linkPackage4.Package = package4;
		linkPackage4.IsLinked = true;
		linkPackage4.PackQty = 2;

		var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		CombineAssertions(() =>
		{
			AssertEquals("Merge", true, mergeResult);

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			AssertEquals("PackagesCount for Export Declaration", 5, entryHeader.PackagesCount);

			entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
			AssertEquals("PackagesCount for EXS Declaration", 6, entryHeader.PackagesCount);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			AssertEquals("PackagesCount for T2L Expedition Declaration", 5, entryHeader.PackagesCount);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("PackagesCount for T2L Reception Declaration", 5, entryHeader.PackagesCount);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			AssertEquals("PackagesCount for Import Declaration", 5, entryHeader.PackagesCount);
		});
	}

	public void TestTotalAmount()
	{
		var entry = Factory.New<CusEntryHeader>();

		AssertEquals("TotalAmount default is 0", ZDecimal.Zero, entry.TotalAmount);

		entry.TotalAmount = 200.6;
		AssertEquals("TotalAmount", (ZDecimal)200.6, entry.TotalAmount);
	}

	public override void TestEntriesOfHouseBillsRefreshed()
	{
		BaseJobDeclaration declaration = ImportJobDeclaration;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.FillWithValidTestData();

		BaseJobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
		BaseJobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
		SetInvoicesToResultInTwoEntries(invoice1.JobComInvoiceLines.AddNew(), invoice2.JobComInvoiceLines.AddNew());

		Bill bill1 = declaration.Bills.AddNew();
		Bill bill2 = declaration.Bills.AddNew();

		invoice1.JZ_CU_RelatedHouseBill = bill1.PK;
		invoice2.JZ_CU_RelatedHouseBill = bill2.PK;

		DoMerge(declaration);

		CusEntryHeader entry1 = (CusEntryHeader)invoice1.JobComInvoiceLines[0].CusEntryLine.Header;
		CusEntryHeader entry2 = (CusEntryHeader)invoice2.JobComInvoiceLines[0].CusEntryLine.Header;
		AssertEquals(true, entry1 != entry2);
		AssertEquals(true, new List<Customs.Business.CusEntryHeader>(bill1.Entries).Contains(entry1));
		AssertEquals(true, new List<Customs.Business.CusEntryHeader>(bill2.Entries).Contains(entry2));

		invoice1.JZ_CU_RelatedHouseBill = bill2.PK;
		invoice2.JZ_CU_RelatedHouseBill = bill1.PK;

		DoMerge(declaration);

		entry1 = (CusEntryHeader)invoice1.JobComInvoiceLines[0].CusEntryLine.Header;
		entry2 = (CusEntryHeader)invoice2.JobComInvoiceLines[0].CusEntryLine.Header;
		AssertEquals(true, entry1 != entry2);
		AssertEquals(true, new List<Customs.Business.CusEntryHeader>(bill1.Entries).Contains(entry2));
		AssertEquals(true, new List<Customs.Business.CusEntryHeader>(bill2.Entries).Contains(entry1));
	}

	public void TestMRNandIssueDateEditable()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var entry = Factory.New<CusEntryHeader>();
		declaration.CustomsEntryHeaders.Add(entry);

		CombineAssertions(() =>
		{
			AssertEquals("T2LT2 must be read only in this stage", true, entry.MovementReferenceNumberInfo.ReadOnly);
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			entryInstruction.CEI_JE = declaration.PK;
			entry.CH_CEI_Instruction = entryInstruction.PK;
			AssertEquals("T2LT2 must be editable when instruction is T2L", false, entry.MovementReferenceNumberInfo.ReadOnly);
			entry.CH_Status = MessageStatusList.Codes.AwaitingResponse;
			AssertEquals("T2LT2 must be read only when status is Awaiting response", true, entry.MovementReferenceNumberInfo.ReadOnly);
			entry.CH_Status = ZString.Empty;
			entry.CH_EntryStatus = UniversalReferenceConstants.EntryStatusCodeList.Clear;
			AssertEquals("T2LT2 must be read only entry status is CLEAR", true, entry.MovementReferenceNumberInfo.ReadOnly);
			entry.MovementReferenceNumber = "MRN-TEST";
			entry.MovementReferenceNumberIssueDate = ZDateTime.BrettsBirthday;
			Factory.Save();
			var entry2 = Factory.Load<CusEntryHeader>(entry.PK);
			AssertEquals("MRN-TEST", entry2.MovementReferenceNumber);
			AssertEquals("Wrong value for MRN Issue date", ZDateTime.BrettsBirthday, entry2.MovementReferenceNumberIssueDate);
		});
	}

	public void TestLimitDateOfArrivalReadOnly()
	{
		var entry = Factory.New<CusEntryHeader>();
		AssertEquals("LimitDateOfArrival must be read only", true, entry.ZG_LimitDateOfArrivalInfo.ReadOnly);
	}

	public void TestCSVT2LReadOnly()
	{
		var entry = Factory.New<CusEntryHeader>();
		AssertEquals("CSVT2L must be read only", true, entry.ZG_CSVT2LInfo.ReadOnly);
	}

	public void TestTaxFeePaymentCodeIsDeferredCore()
	{
		CombineAssertions(() =>
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			AssertEquals("When method of payment is empty", false, entryHeader.TaxFeePaymentCodeIsDeferred(""));
			var entryLine = entryHeader.AllEntryLines.AddNew();
			AssertEquals("When method of payment is DEF", true, entryHeader.TaxFeePaymentCodeIsDeferred(UniversalReferenceConstants.FeeMethodOfPayment.Deferred));
			AssertEquals("When method of payment is not DEF or NBL", false, entryHeader.TaxFeePaymentCodeIsDeferred("123"));
			AssertEquals("When method of payment is NBL", true, entryHeader.TaxFeePaymentCodeIsDeferred(UniversalReferenceConstants.FeeMethodOfPayment.NonBillableTax));
		});
	}

	public void TestCusStorageDocPivotInterfaces()
	{
		var declaration = Factory.New<JobDeclaration>();
		var shipment = Factory.New<ForwardingShipment>();
		declaration.JE_JS = shipment.PK;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var entry = declaration.ActiveEntryHeaders.AddNew();

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
		entry.CH_CEI_Instruction = entryInstruction.PK;

		var entryLine = entry.AllEntryLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		var eDoc1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
		var eDoc2 = shipment.DocManagerInfo.AddFileOrDocument(new byte[1], "InvoiceShipment.pdf", "CIV");

		CombineAssertions(() =>
		{
			var pivotParent = (ICusStorageDocPivotParent)entry;
			AssertNotNull("EDocPivotCollection is not null", pivotParent.EDocPivotCollection);
			AssertEquals("There are 2 documents in EDocCollections", 2, pivotParent.EDocCollections.Count());
			AssertEquals("ICusStorageDocPivotParent method edoc1", true, pivotParent.EDocCollections.Any(x => x.GetFromUniqueKey(eDoc1.UniqueKey.ToGuid()) != null));
			AssertEquals("ICusStorageDocPivotParent method edoc2", true, pivotParent.EDocCollections.Any(x => x.GetFromUniqueKey(eDoc2.UniqueKey.ToGuid()) != null));

			var newPivot = pivotParent.EDocPivotCollection.AddNew();
			newPivot.CSD_DocType = "T1";
			Factory.Save();

			var loadedPivot = new BusinessObjectFactory().Load<BaseCusStorageDocPivot>(newPivot.PK);
			AssertType("ICusStorageDocPivotTypeSupporter method, should have returned the correct type", typeof(CusStorageDocPivot), loadedPivot);
		});
	}

	public void TestClearOtherObjsWhenDeleting()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entry = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();

		var newPivot = entry.EDocPivotCollection.AddNew();
		newPivot.CSD_DocType = "T1";
		var newPivot2 = entry.EDocPivotCollection.AddNew();
		newPivot2.CSD_DocType = "T2";

		CombineAssertions(() =>
		{
			var pivotParent = (ICusStorageDocPivotParent)entry;
			AssertNotNull("EDocPivotCollection is not null", pivotParent.EDocPivotCollection);
			AssertEquals("There are 2 documents in EDocPivotCollection", 2, pivotParent.EDocPivotCollection.Count);

			entry.Delete();
			pivotParent = entry;
			AssertEquals("EDocPivotCollection should have been deleted. Count should be 0", 0, pivotParent.EDocPivotCollection.Count);
		});
	}

	public void TestIsT2L()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var entry = declaration.CustomsEntryHeaders.AddNew();

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
		entry.CH_CEI_Instruction = entryInstruction.PK;

		CombineAssertions(() =>
		{
			AssertEquals("Entry is t2l", true, entry.IsT2L);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			AssertEquals("Entry is not t2l", false, entry.IsT2L);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			entry.CH_CEI_Instruction = ZGuid.Empty;
			AssertEquals("Entry is not t2l", false, entry.IsT2L);

			entry.CH_CEI_Instruction = entryInstruction.PK;
			entry.Delete();
			AssertEquals("Entry is not t2l", false, entry.IsT2L);
		});
	}

	public void TestIsT2C()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var entry = declaration.CustomsEntryHeaders.AddNew();

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
		entry.CH_CEI_Instruction = entryInstruction.PK;

		CombineAssertions(() =>
		{
			AssertEquals("Entry is T2C", true, entry.IsT2C);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			AssertEquals("Entry is not T2C", false, entry.IsT2C);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			entry.CH_CEI_Instruction = ZGuid.Empty;
			AssertEquals("Entry is not T2C", false, entry.IsT2C);

			entry.CH_CEI_Instruction = entryInstruction.PK;
			entry.Delete();
			AssertEquals("Entry is not T2C", false, entry.IsT2C);
		});
	}

	public void TestIsT2LorT2C()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var entry = declaration.CustomsEntryHeaders.AddNew();

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
		entry.CH_CEI_Instruction = entryInstruction.PK;

		CombineAssertions(() =>
		{
			AssertEquals("SubStyle is t2l", true, entry.IsT2LorT2C);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			AssertEquals("SubStyle is A", false, entry.IsT2LorT2C);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			AssertEquals("SubStyle is T2C", true, entry.IsT2LorT2C);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			entry.CH_CEI_Instruction = ZGuid.Empty;
			AssertEquals("CH_CEI_Instruction is empty", false, entry.IsT2LorT2C);

			entry.CH_CEI_Instruction = entryInstruction.PK;
			entry.Delete();
			AssertEquals("delete entry", false, entry.IsT2LorT2C);
		});
	}

	public void TestIsT2LorT2CorEXS()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var entry = declaration.CustomsEntryHeaders.AddNew();

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
		entry.CH_CEI_Instruction = entryInstruction.PK;

		CombineAssertions(() =>
		{
			AssertEquals("SubStyle is t2l", true, entry.IsT2LorT2CorEXS);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			AssertEquals("SubStyle is A", false, entry.IsT2LorT2CorEXS);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			AssertEquals("SubStyle is T2C", true, entry.IsT2LorT2CorEXS);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
			AssertEquals("SubStyle is B", false, entry.IsT2LorT2CorEXS);

			entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
			AssertEquals("SubStyle is EXS", true, entry.IsT2LorT2CorEXS);
		});
	}

	public void TestIsStyleEmpty()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var entry = declaration.CustomsEntryHeaders.AddNew();

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = "A";
		entry.CH_CEI_Instruction = entryInstruction.PK;

		CombineAssertions(() =>
		{
			AssertEquals("Style is A", false, entry.IsStyleEmpty);

			entryInstruction.CEI_Style = ZString.Empty;
			AssertEquals("Style is empty", true, entry.IsStyleEmpty);

			entryInstruction.CEI_Style = "B";
			AssertEquals("Style is B", false, entry.IsStyleEmpty);
		});
	}

	public void TestIsEXSSubStyle()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var entry = declaration.CustomsEntryHeaders.AddNew();

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("Entry is not EXS cause entryInstruction of the entry is null", false, entry.IsExsSubStyle);

			entryInstruction.CEI_SubStyle = ZString.Empty;
			entry.CH_CEI_Instruction = entryInstruction.PK;
			AssertEquals("Entry is not EXS cause entryInstruction is empty", false, entry.IsExsSubStyle);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			AssertEquals("Entry is not EXS cause entryInstruction is not EXS", false, entry.IsExsSubStyle);

			entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
			AssertEquals("Entry is EXS cause entryInstruction is EXS", true, entry.IsExsSubStyle);
		});
	}

	public void TestIsH2Style()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var entry = declaration.CustomsEntryHeaders.AddNew();

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("Entry is not H2 cause entryInstruction of the entry is null", false, entry.IsH2Style);

			entryInstruction.CEI_Style = ZString.Empty;
			entry.CH_CEI_Instruction = entryInstruction.PK;
			AssertEquals("Entry is not H2 cause entryInstruction is empty", false, entry.IsH2Style);

			entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
			AssertEquals("Entry is H2 cause entryInstruction is H2", true, entry.IsH2Style);

			entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.IM;
			AssertEquals("Entry is not H2 cause entryInstruction is not H2", false, entry.IsH2Style);
		});
	}

	public void TestIsAcceptedExport()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var entry = declaration.CustomsEntryHeaders.AddNew();
		entry.CH_EntryStatus = "CLP";

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
		entry.CH_CEI_Instruction = entryInstruction.PK;

		CombineAssertions(() =>
		{
			AssertEquals("Entry is not an accepted export if declaration is import", false, entry.IsAcceptedExport);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("Entry is an accepted export if declaration is export, entry instruction is in (A, B, C, Z) and entry status is in (CLP, CLR, CDA)", true, entry.IsAcceptedExport);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			AssertEquals("Entry is not an accepted export if declaration is export, entry status is in (CLP, CLR, CDA) but entry instruction is not in (A, B, C, Z)", false, entry.IsAcceptedExport);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
			AssertEquals("Entry is an accepted export if declaration is export, entry instruction is in (A, B, C, Z) and entry status is in (CLP, CLR, CDA)", true, entry.IsAcceptedExport);

			entry.CH_EntryStatus = "AAA";
			AssertEquals("Entry is not an accepted export if declaration is export, entry instruction is in (A, B, C, Z) but entry status is not in (CLP, CLR, CDA)", false, entry.IsAcceptedExport);
		});
	}

	public void TestRequiresH1Annexes()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		declaration.JE_MessageType = MessageTypeList.Codes.Import;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge done", true, mergeResult);
		Factory.Save();

		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.ZG_UCC6Version = 0;
		entryHeader.MovementReferenceNumber = "MRN-TEST";
		CombineAssertions(() =>
		{
			AssertEquals("RequiresH1Annexes is false when UCC6Version = 0", false, entryHeader.RequiresH1Annexes);

			entryHeader.ZG_UCC6Version = 1;
			entryHeader.CH_EntryStatus = EntryStatusCodes.CustomsDeclarationAccepted;
			AssertEquals("RequiresH1Annexes is true when UCC6Version > 0, CH_EntryStatus = CDA and IMP", true, entryHeader.RequiresH1Annexes);

			entryHeader.CH_EntryStatus = "JPB";
			AssertEquals("RequiresH1Annexes is false when UCC6Version > 0, CH_EntryStatus = JPB and IMP", false, entryHeader.RequiresH1Annexes);

			entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingDocuments;
			AssertEquals("RequiresH1Annexes is true when UCC6Version > 0, CH_EntryStatus = CDP and IMP", true, entryHeader.RequiresH1Annexes);

			entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
			AssertEquals("RequiresH1Annexes is false when UCC6Version > 0, CH_EntryStatus = CDP, CEI_Style = H2 and IMP", false, entryHeader.RequiresH1Annexes);

			entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.IM;
			entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingJustificationCertificatesDJP;
			AssertEquals("RequiresH1Annexes is true when UCC6Version > 0, CH_EntryStatus = CLJ and IMP", true, entryHeader.RequiresH1Annexes);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			entryHeader.CH_EntryStatus = EntryStatusCodes.CustomsDeclarationAccepted;
			AssertEquals("RequiresH1Annexes is false when UCC6Version > 0 and CH_EntryStatus = CDA and EXP", false, entryHeader.RequiresH1Annexes);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			entryHeader.CH_EntryStatus = "JPB";

			AssertEquals("RequiresH1Annexes is false when there are not Annexes and IMP", false, entryHeader.RequiresH1Annexes);

			var docPivot = Factory.NewWithValidTestData<CusStorageDocPivot>();
			entryHeader.EDocPivotCollection.Add(docPivot);

			AssertEquals("RequiresH1Annexes is true when there are Annexes and IMP", true, entryHeader.RequiresH1Annexes);
		});
	}

	public void TestRequiresAESAnnexes()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge done", true, mergeResult);
		Factory.Save();

		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.ZG_UCC6Version = 0;
		entryHeader.MovementReferenceNumber = "MRN-TEST";
		CombineAssertions(() =>
		{
			AssertEquals("RequiresAESAnnexes is false when UCC6Version = 0", false, entryHeader.RequiresAESAnnexes);

			entryHeader.ZG_UCC6Version = 1;
			AssertEquals("RequiresAESAnnexes is true when UCC6Version > 0", true, entryHeader.RequiresAESAnnexes);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			AssertEquals("RequiresAESAnnexes is false when CEI_SubStyle is T2l", false, entryHeader.RequiresAESAnnexes);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			AssertEquals("RequiresAESAnnexes is true when CEI_SubStyle is A", true, entryHeader.RequiresAESAnnexes);

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			AssertEquals("RequiresAESAnnexes is false when JE_MessageType is import", false, entryHeader.RequiresAESAnnexes);

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			AssertEquals("RequiresAESAnnexes is true when JE_MessageType is export", true, entryHeader.RequiresAESAnnexes);

			entryHeader.MovementReferenceNumber = ZString.Empty;
			AssertEquals("RequiresAESAnnexes is false when MRN is empty", false, entryHeader.RequiresAESAnnexes);

			entryHeader.MovementReferenceNumber = "MRN-TEST";
			AssertEquals("RequiresAESAnnexes is true when MRN is not empty", true, entryHeader.RequiresAESAnnexes);

			entryHeader.SetCSVClearanceNum("CSV-TEST");
			AssertEquals("RequiresAESAnnexes is false when CLR is not empty and no Annexes", false, entryHeader.RequiresAESAnnexes);

			entryHeader.SetCSVClearanceNum("");
			var docPivot = Factory.NewWithValidTestData<CusStorageDocPivot>();
			entryHeader.EDocPivotCollection.Add(docPivot);
			Factory.Save();
			var message = SetEDIMessageAndGenPivot(entryHeader, docPivot);
			message.EM_Status = EDIMessage.Status.Received;
			Factory.Save();
			entryHeader.SetCSVClearanceNum("CSV-TEST");
			AssertEquals("RequiresAESAnnexes is true when CLR is not empty but there are Annexes with Message Status RCV", true, entryHeader.RequiresAESAnnexes);

			message.EM_Status = EDIMessage.Status.Rejected;
			AssertEquals("RequiresAESAnnexes is true when CLR is not empty and there are Annexes with Message Status REJ", true, entryHeader.RequiresAESAnnexes);

			message.EM_Status = ZString.Empty;
			AssertEquals("RequiresAESAnnexes is true when CLR is not empty and there are Annexes with Message Status empty", true, entryHeader.RequiresAESAnnexes);

			entryHeader.SetCSVClearanceNum("");
			entryHeader.EDocPivotCollection.Add(docPivot);
			Factory.Save();
			message = SetEDIMessageAndGenPivot(entryHeader, docPivot);
			Factory.Save();
			entryHeader.SetCSVClearanceNum("CSV-TEST");
			AssertEquals("RequiresAESAnnexes is true when CLR is not empty but there are Annexes with Message Status SNT", true, entryHeader.RequiresAESAnnexes);
		});
	}

	public void TestRequiresT2LAnnexes()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge done", true, mergeResult);
		Factory.Save();

		var entryHeader = declaration.CustomsEntryHeaders[0];
		CombineAssertions(() =>
		{
			AssertEquals("RequiresT2LAnnexes is false when entry header is not t2l", false, entryHeader.RequiresT2LAnnexes());

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

			AssertEquals("RequiresT2LAnnexes is true when entry header is t2l", true, entryHeader.RequiresT2LAnnexes());

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			var supDoc = invoiceLine.SupportingDocuments.AddNew();
			supDoc.CSI_Code = "9010";
			Factory.Save();

			AssertEquals("RequiresT2LAnnexes is false if the entryHeader is T2L, export and has at least one supporting document with type 9010", false, entryHeader.RequiresT2LAnnexes());
		});
	}

	public void TestRequiresT2LPOUSAnnexes()
	{
		using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.ProofOfUnionStatus))
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);
			Factory.Save();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.MovementReferenceNumber = "MRN-TEST";

			CombineAssertions(() =>
			{
				AssertEquals("RequiresT2LPOUSAnnexes is true when CEI_SubStyle is T2C", true, entryHeader.RequiresT2LPOUSAnnexes);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				AssertEquals("RequiresT2LPOUSAnnexes is false when CEI_SubStyle is A", false, entryHeader.RequiresT2LPOUSAnnexes);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				AssertEquals("RequiresT2LPOUSAnnexes is true when CEI_SubStyle is T2L", true, entryHeader.RequiresT2LPOUSAnnexes);

				entryHeader.MovementReferenceNumber = ZString.Empty;
				AssertEquals("RequiresT2LPOUSAnnexes is false when MRN is empty", false, entryHeader.RequiresT2LPOUSAnnexes);

				entryHeader.MovementReferenceNumber = "MRN-TEST";
				AssertEquals("RequiresT2LPOUSAnnexes is true when MRN is not empty", true, entryHeader.RequiresT2LPOUSAnnexes);

				entryHeader.SetCSVClearanceNum("CSV-TEST");
				AssertEquals("RequiresT2LPOUSAnnexes is false when CLR is not empty and no Annexes", false, entryHeader.RequiresT2LPOUSAnnexes);

				entryHeader.SetCSVClearanceNum("");
				entryHeader.MovementReferenceNumber = "MRN-TEST";
				var docPivot = Factory.NewWithValidTestData<CusStorageDocPivot>();
				entryHeader.EDocPivotCollection.Add(docPivot);
				Factory.Save();
				var message = SetEDIMessageAndGenPivot(entryHeader, docPivot);
				message.EM_Status = EDIMessage.Status.Received;
				Factory.Save();
				entryHeader.SetCSVClearanceNum("CLR1234");
				AssertEquals("RequiresT2LPOUSAnnexes is true when has MRN and Clearance but there are Annexes with Message Status RCV", true, entryHeader.RequiresT2LPOUSAnnexes);

				message.EM_Status = EDIMessage.Status.Rejected;
				AssertEquals("RequiresT2LPOUSAnnexes is false when has MRN and Clearance but there are Annexes with Message Status REJ", true, entryHeader.RequiresT2LPOUSAnnexes);

				entryHeader.SetCSVClearanceNum("");
				AssertEquals("RequiresT2LPOUSAnnexes is true when has MRN and not Clearance but there are Annexes with Message Status REJ", true, entryHeader.RequiresT2LPOUSAnnexes);

				entryHeader.SetCSVClearanceNum("CLR1234");
				message.EM_Status = EDIMessage.Status.Sent;
				AssertEquals("RequiresT2LPOUSAnnexes is true when has MRN and not Clearance but there are Annexes with Message Status SNT", true, entryHeader.RequiresT2LPOUSAnnexes);
			});
		}

		using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.NoProofOfUnionStatus))
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);
			Factory.Save();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.MovementReferenceNumber = "MRN-TEST";

			CombineAssertions(() =>
			{
				AssertEquals("RequiresT2LPOUSAnnexes is true when CEI_SubStyle is T2C", false, entryHeader.RequiresT2LPOUSAnnexes);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				AssertEquals("RequiresT2LPOUSAnnexes is false when CEI_SubStyle is A", false, entryHeader.RequiresT2LPOUSAnnexes);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				AssertEquals("RequiresT2LPOUSAnnexes is true when CEI_SubStyle is T2L", false, entryHeader.RequiresT2LPOUSAnnexes);

				entryHeader.MovementReferenceNumber = ZString.Empty;
				AssertEquals("RequiresT2LPOUSAnnexes is false when MRN is empty", false, entryHeader.RequiresT2LPOUSAnnexes);

				entryHeader.MovementReferenceNumber = "MRN-TEST";
				AssertEquals("RequiresT2LPOUSAnnexes is true when MRN is not empty", false, entryHeader.RequiresT2LPOUSAnnexes);
			});
		}

		using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.RequestJecAndReceptionPous))
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);
			Factory.Save();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.MovementReferenceNumber = "MRN-TEST";

			CombineAssertions(() =>
			{
				AssertEquals("RequiresT2LPOUSAnnexes is true when CEI_SubStyle is T2C", true, entryHeader.RequiresT2LPOUSAnnexes);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				AssertEquals("RequiresT2LPOUSAnnexes is false when CEI_SubStyle is A", false, entryHeader.RequiresT2LPOUSAnnexes);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				AssertEquals("RequiresT2LPOUSAnnexes is true when CEI_SubStyle is T2L", true, entryHeader.RequiresT2LPOUSAnnexes);

				entryHeader.MovementReferenceNumber = ZString.Empty;
				AssertEquals("RequiresT2LPOUSAnnexes is false when MRN is empty", false, entryHeader.RequiresT2LPOUSAnnexes);

				entryHeader.MovementReferenceNumber = "MRN-TEST";
				AssertEquals("RequiresT2LPOUSAnnexes is true when MRN is not empty", true, entryHeader.RequiresT2LPOUSAnnexes);

				entryHeader.SetCSVClearanceNum("CSV-TEST");
				AssertEquals("RequiresT2LPOUSAnnexes is false when CLR is not empty and no Annexes", false, entryHeader.RequiresT2LPOUSAnnexes);

				entryHeader.SetCSVClearanceNum("");
				entryHeader.MovementReferenceNumber = "MRN-TEST";
				var docPivot = Factory.NewWithValidTestData<CusStorageDocPivot>();
				entryHeader.EDocPivotCollection.Add(docPivot);
				Factory.Save();
				var message = SetEDIMessageAndGenPivot(entryHeader, docPivot);
				message.EM_Status = EDIMessage.Status.Received;
				Factory.Save();
				entryHeader.SetCSVClearanceNum("CLR1234");
				AssertEquals("RequiresT2LPOUSAnnexes is true when has MRN and Clearance but there are Annexes with Message Status RCV", true, entryHeader.RequiresT2LPOUSAnnexes);

				message.EM_Status = EDIMessage.Status.Rejected;
				AssertEquals("RequiresT2LPOUSAnnexes is false when has MRN and Clearance but there are Annexes with Message Status REJ", true, entryHeader.RequiresT2LPOUSAnnexes);

				entryHeader.SetCSVClearanceNum("");
				AssertEquals("RequiresT2LPOUSAnnexes is true when has MRN and not Clearance but there are Annexes with Message Status REJ", true, entryHeader.RequiresT2LPOUSAnnexes);

				entryHeader.SetCSVClearanceNum("CLR1234");
				message.EM_Status = EDIMessage.Status.Sent;
				AssertEquals("RequiresT2LPOUSAnnexes is true when has MRN and not Clearance but there are Annexes with Message Status SNT", true, entryHeader.RequiresT2LPOUSAnnexes);
			});
		}
	}

	public void TestHasAnnexes()
	{
		using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.NoProofOfUnionStatus))
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge failed", true, mergeResult);
			Factory.Save();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			CombineAssertions(() =>
			{
				entryHeader.EDocPivotCollection.RemoveAndDeleteAll();
				Factory.Save();
				AssertEquals("HasAnnexes is false when EDocPivotCollection is empty", false, entryHeader.HasAnnexes);

				var docPivot = Factory.NewWithValidTestData<CusStorageDocPivot>();
				entryHeader.EDocPivotCollection.Add(docPivot);
				Factory.Save();
				AssertEquals("HasAnnexes is true when entryHeader has pivots in EDocPivotCollection", true, entryHeader.HasAnnexes);
			});
		}
	}

	public void TestHasAnnexesSentWithoutResponse()
	{
		using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.NoProofOfUnionStatus))
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge failed", true, mergeResult);
			Factory.Save();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			CombineAssertions(() =>
			{
				entryHeader.EDocPivotCollection.RemoveAndDeleteAll();
				Factory.Save();
				AssertEquals("HasAnnexesSentWithoutResponse is false when EDocPivotCollection is empty", false, entryHeader.HasAnnexesSentWithoutResponse());

				var docPivot = Factory.NewWithValidTestData<CusStorageDocPivot>();
				entryHeader.EDocPivotCollection.Add(docPivot);
				Factory.Save();
				AssertEquals("HasAnnexesSentWithoutResponse is false when entryHeader has pivots in EDocPivotCollection but no EDIMessages associated to the pivot", false, entryHeader.HasAnnexesSentWithoutResponse());

				var message = SetEDIMessageAndGenPivot(entryHeader, docPivot);
				Factory.Save();
				AssertEquals("HasAnnexesSentWithoutResponse is true when entryHeader has pivots in EDocPivotCollection and at least one transmit EDIMessage associated to the pivot", true, entryHeader.HasAnnexesSentWithoutResponse());

				message.EM_Status = EDIMessage.Status.Rejected;
				Factory.Save();
				AssertEquals("HasAnnexesSentWithoutResponse is false when entryHeader has pivots in EDocPivotCollection and at least one transmit EDIMessage associated to the pivot but the message is not awaiting response", false, entryHeader.HasAnnexesSentWithoutResponse());
			});
		}
	}

	public void TestHasSendableAnnexesForH1()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		declaration.JE_MessageType = MessageTypeList.Codes.Import;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge failed", true, mergeResult);
		Factory.Save();

		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.ZG_UCC6Version = 1;
		entryHeader.MovementReferenceNumber = "MRN-TEST";
		CombineAssertions(() =>
		{
			entryHeader.CH_EntryStatus = "JPB";
			AssertEquals("HasSendableAnnexesForH1 is false when entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot but entry's CH_EntryStatus is JPB", false, entryHeader.HasSendableAnnexesForH1);

			entryHeader.EDocPivotCollection.RemoveAndDeleteAll();
			Factory.Save();
			AssertEquals("HasSendableAnnexesForH1 is false when EDocPivotCollection is empty", false, entryHeader.HasSendableAnnexesForH1);

			var docPivot = Factory.NewWithValidTestData<CusStorageDocPivot>();
			entryHeader.EDocPivotCollection.Add(docPivot);
			Factory.Save();
			AssertEquals("HasSendableAnnexesForH1 is true when entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot", true, entryHeader.HasSendableAnnexesForH1);

			entryHeader.ZG_UCC6Version = 0;
			AssertEquals("HasSendableAnnexesForH1 is false when entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot but entry's UCC6Version = 0", false, entryHeader.HasSendableAnnexesForH1);

			entryHeader.ZG_UCC6Version = 1;
			AssertEquals("HasSendableAnnexesForH1 is true when entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and entry's UCC6Version = 1", true, entryHeader.HasSendableAnnexesForH1);

			entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
			AssertEquals("HasSendableAnnexesForH1 is false when entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot but entry's CEI_Style is H2", false, entryHeader.HasSendableAnnexesForH1);

			entryHeader.CH_EntryStatus = EntryStatusCodes.CustomsDeclarationAccepted;
			entryInstruction.CEI_Style = ZString.Empty;
			AssertEquals("HasSendableAnnexesForH1 is true when entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and entry's CH_EntryStatus is CDA", true, entryHeader.HasSendableAnnexesForH1);

			entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingDocuments;
			AssertEquals("HasSendableAnnexesForH1 is true when entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and entry's CH_EntryStatus is CDP", true, entryHeader.HasSendableAnnexesForH1);

			entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingJustificationCertificatesDJP;
			AssertEquals("HasSendableAnnexesForH1 is true when entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and entry's CH_EntryStatus is CLJ", true, entryHeader.HasSendableAnnexesForH1);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("HasSendableAnnexesForH1 is false when entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot but JE_MessageType is export", false, entryHeader.HasSendableAnnexesForH1);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var message = SetEDIMessageAndGenPivot(entryHeader, docPivot);
			Factory.Save();
			AssertEquals("HasSendableAnnexesForH1 is false when entryHeader has pivots in EDocPivotCollection and at least one transmit EDIMessage associated to the pivot but the message is awaiting response", false, entryHeader.HasSendableAnnexesForH1);

			message.EM_Status = EDIMessage.Status.Rejected;
			Factory.Save();
			AssertEquals("HasSendableAnnexesForH1 is true when entryHeader has pivots in EDocPivotCollection and at least one transmit EDIMessage associated to the pivot and the message is not awaiting response", true, entryHeader.HasSendableAnnexesForH1);

			message.EM_Status = EDIMessage.Status.Received;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();
			AssertEquals("HasSendableAnnexesForH1 is false when entryHeader has pivots in EDocPivotCollection and at least one receive EDIMessage associated to the pivot but the message has status RCV", false, entryHeader.HasSendableAnnexesForH1);
		});
	}

	public void TestHasSendableAnnexesForAES()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge failed", true, mergeResult);
		Factory.Save();

		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.ZG_UCC6Version = 1;
		entryHeader.MovementReferenceNumber = "MRN-TEST";
		CombineAssertions(() =>
		{
			entryHeader.EDocPivotCollection.RemoveAndDeleteAll();
			Factory.Save();
			AssertEquals("HasSendableAnnexesForAES is false when EDocPivotCollection is empty", false, entryHeader.HasSendableAnnexesForAES);

			var docPivot = Factory.NewWithValidTestData<CusStorageDocPivot>();
			entryHeader.EDocPivotCollection.Add(docPivot);
			Factory.Save();
			AssertEquals("HasSendableAnnexesForAES is true when entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot", true, entryHeader.HasSendableAnnexesForAES);

			entryHeader.ZG_UCC6Version = 0;
			AssertEquals("HasSendableAnnexesForAES is false when entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot but entry's UCC6Version = 0", false, entryHeader.HasSendableAnnexesForAES);

			entryHeader.ZG_UCC6Version = 1;
			AssertEquals("HasSendableAnnexesForAES is true when entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and entry's UCC6Version = 1", true, entryHeader.HasSendableAnnexesForAES);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			AssertEquals("HasSendableAnnexesForAES is false when entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot but entry's CEI_SubStyle is T2C", false, entryHeader.HasSendableAnnexesForAES);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
			AssertEquals("HasSendableAnnexesForAES is true when entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and entry's CEI_SubStyle is B", true, entryHeader.HasSendableAnnexesForAES);

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			AssertEquals("HasSendableAnnexesForAES is false when entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot but JE_MessageType is import", false, entryHeader.HasSendableAnnexesForAES);

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			AssertEquals("HasSendableAnnexesForAES is true when entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and JE_MessageType is export", true, entryHeader.HasSendableAnnexesForAES);

			entryHeader.MovementReferenceNumber = ZString.Empty;
			AssertEquals("HasSendableAnnexesForAES is false when entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot but entry's MRN is empty", false, entryHeader.HasSendableAnnexesForAES);

			entryHeader.MovementReferenceNumber = "MRN-TEST";
			AssertEquals("HasSendableAnnexesForAES is true when entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and entry's MRN is not empty", true, entryHeader.HasSendableAnnexesForAES);

			var message = SetEDIMessageAndGenPivot(entryHeader, docPivot);
			Factory.Save();
			AssertEquals("HasSendableAnnexesForAES is false when entryHeader has pivots in EDocPivotCollection and at least one transmit EDIMessage associated to the pivot but the message is awaiting response", false, entryHeader.HasSendableAnnexesForAES);

			message.EM_Status = EDIMessage.Status.Rejected;
			Factory.Save();
			AssertEquals("HasSendableAnnexesForAES is true when entryHeader has pivots in EDocPivotCollection and at least one transmit EDIMessage associated to the pivot and the message is not awaiting response", true, entryHeader.HasSendableAnnexesForAES);

			message.EM_Status = EDIMessage.Status.Received;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();
			AssertEquals("HasSendableAnnexesForAES is false when entryHeader has pivots in EDocPivotCollection and at least one receive EDIMessage associated to the pivot but the message has status RCV", false, entryHeader.HasSendableAnnexesForAES);
		});
	}

	public void TestHasSendableAnnexesForT2LPOUS()
	{
		using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.ProofOfUnionStatus))
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge failed", true, mergeResult);
			Factory.Save();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.MovementReferenceNumber = "MRN-TEST";

			CombineAssertions(() =>
			{
				entryHeader.EDocPivotCollection.RemoveAndDeleteAll();
				Factory.Save();
				AssertEquals("HasSendableAnnexesForT2LPOUS is false when EDocPivotCollection is empty", false, entryHeader.HasSendableAnnexesForT2LPOUS);

				var docPivot = Factory.NewWithValidTestData<CusStorageDocPivot>();
				entryHeader.EDocPivotCollection.Add(docPivot);
				Factory.Save();
				AssertEquals("HasSendableAnnexesForT2LPOUS is true when entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot", true, entryHeader.HasSendableAnnexesForT2LPOUS);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				AssertEquals("HasSendableAnnexesForT2LPOUS is false when entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot but entry's CEI_SubStyle is A", false, entryHeader.HasSendableAnnexesForT2LPOUS);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				AssertEquals("HasSendableAnnexesForT2LPOUS is true when entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and entry's CEI_SubStyle is T2C", true, entryHeader.HasSendableAnnexesForT2LPOUS);

				entryHeader.MovementReferenceNumber = ZString.Empty;
				AssertEquals("HasSendableAnnexesForT2LPOUS is false when entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot but entry's MRN is empty", false, entryHeader.HasSendableAnnexesForT2LPOUS);

				entryHeader.MovementReferenceNumber = "MRN-TEST";
				AssertEquals("HasSendableAnnexesForT2LPOUS is true when entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and entry's MRN is not empty", true, entryHeader.HasSendableAnnexesForT2LPOUS);

				var message = SetEDIMessageAndGenPivot(entryHeader, docPivot);
				Factory.Save();
				AssertEquals("HasSendableAnnexesForT2LPOUS is false when entryHeader has pivots in EDocPivotCollection and at least one transmit EDIMessage associated to the pivot but the message is awaiting response", false, entryHeader.HasSendableAnnexesForT2LPOUS);

				message.EM_Status = EDIMessage.Status.Rejected;
				Factory.Save();
				AssertEquals("HasSendableAnnexesForT2LPOUS is true when entryHeader has pivots in EDocPivotCollection and at least one transmit EDIMessage associated to the pivot and the message is not awaiting response", true, entryHeader.HasSendableAnnexesForT2LPOUS);

				message.EM_Status = EDIMessage.Status.Received;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				Factory.Save();
				AssertEquals("HasSendableAnnexesForT2LPOUS is false when entryHeader has pivots in EDocPivotCollection and at least one receive EDIMessage associated to the pivot but the message has status RCV", false, entryHeader.HasSendableAnnexesForT2LPOUS);
			});
		}
	}

	public void TestHasSendableAnnexesForT2LPOUS_WhenPOUS2()
	{
		using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.RequestJecAndReceptionPous))
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge failed", true, mergeResult);
			Factory.Save();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.MovementReferenceNumber = "MRN-TEST";

			CombineAssertions(() =>
			{
				entryHeader.EDocPivotCollection.RemoveAndDeleteAll();
				Factory.Save();
				AssertEquals("HasSendableAnnexesForT2LPOUS is false when EDocPivotCollection is empty", false, entryHeader.HasSendableAnnexesForT2LPOUS);

				var docPivot = Factory.NewWithValidTestData<CusStorageDocPivot>();
				entryHeader.EDocPivotCollection.Add(docPivot);
				Factory.Save();
				AssertEquals("HasSendableAnnexesForT2LPOUS is true when entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot", true, entryHeader.HasSendableAnnexesForT2LPOUS);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				AssertEquals("HasSendableAnnexesForT2LPOUS is false when entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot but entry's CEI_SubStyle is A", false, entryHeader.HasSendableAnnexesForT2LPOUS);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				AssertEquals("HasSendableAnnexesForT2LPOUS is true when entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and entry's CEI_SubStyle is T2C", true, entryHeader.HasSendableAnnexesForT2LPOUS);

				entryHeader.MovementReferenceNumber = ZString.Empty;
				AssertEquals("HasSendableAnnexesForT2LPOUS is false when entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot but entry's MRN is empty", false, entryHeader.HasSendableAnnexesForT2LPOUS);

				entryHeader.MovementReferenceNumber = "MRN-TEST";
				AssertEquals("HasSendableAnnexesForT2LPOUS is true when entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and entry's MRN is not empty", true, entryHeader.HasSendableAnnexesForT2LPOUS);

				var message = SetEDIMessageAndGenPivot(entryHeader, docPivot);
				Factory.Save();
				AssertEquals("HasSendableAnnexesForT2LPOUS is false when entryHeader has pivots in EDocPivotCollection and at least one transmit EDIMessage associated to the pivot but the message is awaiting response", false, entryHeader.HasSendableAnnexesForT2LPOUS);

				message.EM_Status = EDIMessage.Status.Rejected;
				Factory.Save();
				AssertEquals("HasSendableAnnexesForT2LPOUS is true when entryHeader has pivots in EDocPivotCollection and at least one transmit EDIMessage associated to the pivot and the message is not awaiting response", true, entryHeader.HasSendableAnnexesForT2LPOUS);

				message.EM_Status = EDIMessage.Status.Received;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				Factory.Save();
				AssertEquals("HasSendableAnnexesForT2LPOUS is false when entryHeader has pivots in EDocPivotCollection and at least one receive EDIMessage associated to the pivot but the message has status RCV", false, entryHeader.HasSendableAnnexesForT2LPOUS);
			});
		}
	}

	public void TestAnnexesRemovedWhenNotRequired_H1()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		declaration.JE_MessageType = MessageTypeList.Codes.Import;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge failed", true, mergeResult);
		Factory.Save();

		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.ZG_UCC6Version = 0;

		var docPivot = Factory.NewWithValidTestData<CusStorageDocPivot>();
		entryHeader.EDocPivotCollection.Add(docPivot);
		Factory.Save();
		CombineAssertions(() =>
		{
			AssertEquals("HasAnnexes is false because is not H1 and has pivots in EDocPivotCollection", false, entryHeader.HasAnnexes);

			docPivot = Factory.NewWithValidTestData<CusStorageDocPivot>();
			entryHeader.EDocPivotCollection.Add(docPivot);
			entryHeader.ZG_UCC6Version = 1;
			Factory.Save();
			AssertEquals("HasAnnexes is true because is H1 and has pivots in EDocPivotCollection", true, entryHeader.HasAnnexes);

			entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
			Factory.Save();
			AssertEquals("HasAnnexes is false because is H1, CEI_Style = H2 and has pivots in EDocPivotCollection", false, entryHeader.HasAnnexes);

			docPivot = Factory.NewWithValidTestData<CusStorageDocPivot>();
			entryHeader.EDocPivotCollection.Add(docPivot);
			entryInstruction.CEI_Style = ZString.Empty;
			Factory.Save();
			AssertEquals("HasAnnexes is true because is H1, CEI_Style = Empty and has pivots in EDocPivotCollection", true, entryHeader.HasAnnexes);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			Factory.Save();
			AssertEquals("HasAnnexes is false because is H1, export and has pivots in EDocPivotCollection", false, entryHeader.HasAnnexes);
		});
	}

	public void TestAnnexesRemovedWhenNotRequired_AES()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		declaration.JE_MessageType = MessageTypeList.Codes.Export;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge failed", true, mergeResult);
		Factory.Save();

		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.ZG_UCC6Version = 0;
		entryHeader.MovementReferenceNumber = "MRN-TEST";

		var docPivot = Factory.NewWithValidTestData<CusStorageDocPivot>();
		entryHeader.EDocPivotCollection.Add(docPivot);
		Factory.Save();
		CombineAssertions(() =>
		{
			AssertEquals("HasAnnexes is false because ZG_UCC6Version = 0 and has pivots in EDocPivotCollection", false, entryHeader.HasAnnexes);

			docPivot = Factory.NewWithValidTestData<CusStorageDocPivot>();
			entryHeader.EDocPivotCollection.Add(docPivot);
			entryHeader.ZG_UCC6Version = 1;
			Factory.Save();
			AssertEquals("HasAnnexes is true because ZG_UCC6Version = 1 and has pivots in EDocPivotCollection", true, entryHeader.HasAnnexes);

			entryInstruction.CEI_SubStyle = "JPB";
			Factory.Save();
			AssertEquals("HasAnnexes is false because ZG_UCC6Version = 1, CEI_SubStyle = T2L and has pivots in EDocPivotCollection", false, entryHeader.HasAnnexes);

			docPivot = Factory.NewWithValidTestData<CusStorageDocPivot>();
			entryHeader.EDocPivotCollection.Add(docPivot);
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			Factory.Save();
			AssertEquals("HasAnnexes is true because ZG_UCC6Version = 1,CEI_SubStyle = A and has pivots in EDocPivotCollection", true, entryHeader.HasAnnexes);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
			Factory.Save();
			AssertEquals("HasAnnexes is false because ZG_UCC6Version = 1, import and has pivots in EDocPivotCollection", false, entryHeader.HasAnnexes);

			docPivot = Factory.NewWithValidTestData<CusStorageDocPivot>();
			entryHeader.EDocPivotCollection.Add(docPivot);
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			Factory.Save();
			AssertEquals("HasAnnexes is true because ZG_UCC6Version = 1, export and has pivots in EDocPivotCollection", true, entryHeader.HasAnnexes);

			entryHeader.MovementReferenceNumber = ZString.Empty;
			Factory.Save();
			AssertEquals("HasAnnexes is false because ZG_UCC6Version = 1, MovementReferenceNumber = Empty and has pivots in EDocPivotCollection", false, entryHeader.HasAnnexes);
		});
	}

	public void TestAnnexesRemovedWhenNotRequired_T2L()
	{
		using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.NoProofOfUnionStatus))
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge failed", true, mergeResult);
			Factory.Save();

			var entryHeader = declaration.CustomsEntryHeaders[0];

			var docPivot = Factory.NewWithValidTestData<CusStorageDocPivot>();
			entryHeader.EDocPivotCollection.Add(docPivot);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("HasAnnexes is true when entryHeader is t2l and has pivots in EDocPivotCollection", true, entryHeader.HasAnnexes);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				Factory.Save();
				AssertEquals("HasAnnexes is false when entry header is no longer t2l", false, entryHeader.HasAnnexes);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				docPivot = Factory.NewWithValidTestData<CusStorageDocPivot>();
				entryHeader.EDocPivotCollection.Add(docPivot);
				Factory.Save();
				AssertEquals("HasAnnexes is true when entryHeader is t2l again and has pivots in EDocPivotCollection", true, entryHeader.HasAnnexes);

				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
				var supDoc = invoiceLine.SupportingDocuments.AddNew();
				supDoc.CSI_Code = "9010";
				Factory.Save();
				AssertEquals("HasAnnexes is false when entry header is t2l, export and has at least one supporting document with type 9010", false, entryHeader.HasAnnexes);
			});
		}
	}

	public void TestGetAllEDocPivotsToSend()
	{
		using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.NoProofOfUnionStatus))
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge failed", true, mergeResult);
			Factory.Save();

			var entryHeader = declaration.CustomsEntryHeaders[0];

			CombineAssertions(() =>
			{
				var pivotList = entryHeader.GetAllSendableEDocPivots();
				AssertEquals("GetAllEDocPivotsToSend returns empty pivotList since there are no pivots in the colection", 0, pivotList.Count);

				var docPivot = Factory.NewWithValidTestData<CusStorageDocPivot>();
				entryHeader.EDocPivotCollection.Add(docPivot);
				Factory.Save();
				pivotList = entryHeader.GetAllSendableEDocPivots();
				AssertEquals("GetAllEDocPivotsToSend returns pivotList with one element since there is only one pivot in the colection", 1, pivotList.Count);

				var docPivot2 = Factory.NewWithValidTestData<CusStorageDocPivot>();
				entryHeader.EDocPivotCollection.Add(docPivot2);
				Factory.Save();
				pivotList = entryHeader.GetAllSendableEDocPivots();
				AssertEquals("GetAllEDocPivotsToSend returns pivotList with two elements since there are two pivots in the colection with empty status", 2, pivotList.Count);

				var docPivot3 = Factory.NewWithValidTestData<CusStorageDocPivot>();
				entryHeader.EDocPivotCollection.Add(docPivot3);
				var docPivot4 = Factory.NewWithValidTestData<CusStorageDocPivot>();
				entryHeader.EDocPivotCollection.Add(docPivot4);
				var docPivot5 = Factory.NewWithValidTestData<CusStorageDocPivot>();
				entryHeader.EDocPivotCollection.Add(docPivot5);

				var message1 = SetEDIMessageAndGenPivot(entryHeader, docPivot2);
				var message2 = SetEDIMessageAndGenPivot(entryHeader, docPivot3);
				var message3 = SetEDIMessageAndGenPivot(entryHeader, docPivot4);
				var message4 = SetEDIMessageAndGenPivot(entryHeader, docPivot5);
				Factory.Save();
				pivotList = entryHeader.GetAllSendableEDocPivots();
				AssertEquals("GetAllEDocPivotsToSend returns pivotList with one element since all pivots in collection are associated to send messages but one (last pivot)", 1, pivotList.Count);

				message1.EM_Status = EDIMessage.Status.Rejected;
				message2.EM_Status = EDIMessage.Status.Failed;
				message3.EM_Status = EDIMessage.Status.Error;
				message4.EM_Status = MessageStatusList.Codes.FailedFromTransmission;

				var docPivot6 = Factory.NewWithValidTestData<CusStorageDocPivot>();
				entryHeader.EDocPivotCollection.Add(docPivot6);
				var message5 = SetEDIMessageAndGenPivot(entryHeader, docPivot6);
				message5.EM_Status = EDIMessage.Status.Received;
				Factory.Save();
				pivotList = entryHeader.GetAllSendableEDocPivots();
				AssertEquals("GetAllEDocPivotsToSend returns pivotList with 5 elements (messages have error/failed/rejected/failedfrontransmission/empty status)", 5, pivotList.Count);
			});
		}
	}

	public void TestHasMRNAndIssueDate()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		var invoice = declaration.Invoices.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = declaration.CustomsEntryHeaders[0];
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("HasMRNAndIssueDate is false because entry header has no mrn nor issue date", false, entryHeader.HasMRNAndIssueDate);

			entryHeader.MovementReferenceNumber = "Test";
			Factory.Save();
			AssertEquals("HasMRNAndIssueDate is false because entry header has no issue date", false, entryHeader.HasMRNAndIssueDate);

			entryHeader.MovementReferenceNumberIssueDate = ZDateTime.BrettsBirthday;
			Factory.Save();
			AssertEquals("HasMRNAndIssueDate is true because entry header has mrn and issue date", true, entryHeader.HasMRNAndIssueDate);
		});
	}

	public void TestCanRequestEffectiveDepartureCertificate()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge done", true, mergeResult);
		Factory.Save();

		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.ZG_UCC6Version = 0;
		entryHeader.CH_EntryStatus = EntryStatusCodes.EffectiveDeparture;
		CombineAssertions(() =>
		{
			AssertEquals("CanRequestEffectiveDepartureCertificate is true when UCC6Version = 0 and entry status is EFD", true, entryHeader.CanRequestEffectiveDepartureCertificate);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			AssertEquals("CanRequestEffectiveDepartureCertificate is false when UCC6Version = 0 and CEI_SubStyle is T2l", false, entryHeader.CanRequestEffectiveDepartureCertificate);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			AssertEquals("CanRequestEffectiveDepartureCertificate is true when UCC6Version = 0 and CEI_SubStyle is A", true, entryHeader.CanRequestEffectiveDepartureCertificate);

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			AssertEquals("CanRequestEffectiveDepartureCertificate is false when UCC6Version = 0 and JE_MessageType is import", false, entryHeader.CanRequestEffectiveDepartureCertificate);

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			AssertEquals("CanRequestEffectiveDepartureCertificate is true when UCC6Version = 0 and JE_MessageType is export", true, entryHeader.CanRequestEffectiveDepartureCertificate);

			entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;
			AssertEquals("CanRequestEffectiveDepartureCertificate is false when UCC6Version = 0 and entry status is not EFD", false, entryHeader.CanRequestEffectiveDepartureCertificate);

			entryHeader.CH_EntryStatus = EntryStatusCodes.EffectiveDeparture;
			entryHeader.ZG_UCC6Version = 1;
			AssertEquals("CanRequestEffectiveDepartureCertificate is true when UCC6Version > 0 and entry status is EFD", true, entryHeader.CanRequestEffectiveDepartureCertificate);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			AssertEquals("CanRequestEffectiveDepartureCertificate is false when UCC6Version > 0 and CEI_SubStyle is T2l", false, entryHeader.CanRequestEffectiveDepartureCertificate);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			AssertEquals("CanRequestEffectiveDepartureCertificate is true when UCC6Version > 0 and CEI_SubStyle is A", true, entryHeader.CanRequestEffectiveDepartureCertificate);

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			AssertEquals("CanRequestEffectiveDepartureCertificate is false when UCC6Version > 0 and JE_MessageType is import", false, entryHeader.CanRequestEffectiveDepartureCertificate);

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			AssertEquals("CanRequestEffectiveDepartureCertificate is true when UCC6Version > 0 and JE_MessageType is export", true, entryHeader.CanRequestEffectiveDepartureCertificate);

			entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;
			AssertEquals("CanRequestEffectiveDepartureCertificate is false when UCC6Version > 0 and entry status is not EFD", false, entryHeader.CanRequestEffectiveDepartureCertificate);
		});
	}

	[TestDate(2022, 3, 12, 4, 0, 0)]
	public override void TestBGMReferencesAreSetWhenSavedAndThenUCRIsChanged()
	{
		var declaration = Factory.New<JobDeclaration>();
		var shipment = Factory.New<ForwardingShipment>();
		declaration.JE_JS = shipment.PK;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var entry = declaration.ActiveEntryHeaders.AddNew();
		var entryLine = entry.AllEntryLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		var registrationMock = new Mock<IProductRegistration>();
		registrationMock.Setup(r => r.Key.EnterpriseCode).Returns("AAA");
		registrationMock.Setup(r => r.Key.ServerCode).Returns("JPB");

		using (ObjectFactory.Substitute(registrationMock.Object))
		using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
		{
			CombineAssertions(() =>
			{
				var registrationMockKey = registrationMock.Object.Key;
				var expectedNumberFountain = Env.NumberFountains.ESBGMReference(ZDate.Today.Year.ToString("0000"), registrationMockKey.EnterpriseCode, registrationMockKey.ServerCode);
				expectedNumberFountain.SetNext(Factory, 1);
				Factory.Save();
				AssertEquals("ES220000001AAAJPB", entry.CH_BGMReference);

				var entry2 = declaration.ActiveEntryHeaders.AddNew();
				Factory.Save();
				AssertEquals("ES220000002AAAJPB", entry2.CH_BGMReference);

				expectedNumberFountain.SetNext(Factory, 13);

				var entry3 = declaration.ActiveEntryHeaders.AddNew();
				Factory.Save();
				AssertEquals("ES220000013AAAJPB", entry3.CH_BGMReference);
				var entry4 = declaration.ActiveEntryHeaders.AddNew();
				Factory.Save();
				AssertEquals("ES220000014AAAJPB", entry4.CH_BGMReference);
			});
		}
	}

	public void TestEntryLines()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		AssertType<CusEntryLineCollection<CusEntryLine>>(entryHeader.MergedLines);
		AssertType<AllCusEntryLineCollection<CusEntryLine>>(entryHeader.AllEntryLines);
	}

	public void TestHasBeenLodgedAtCustoms()
	{
		var entry = Factory.New<CusEntryHeader>();
		CombineAssertions(() =>
		{
			AssertEquals(false, entry.HasBeenLodgedAtCustoms);

			entry.CH_EntryStatus = MessageProcessorConstants.EntryStatusCodes.Cleared;
			AssertEquals(true, entry.HasBeenLodgedAtCustoms);

			entry.CH_EntryStatus = MessageProcessorConstants.EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
			AssertEquals(true, entry.HasBeenLodgedAtCustoms);

			entry.CH_EntryStatus = MessageProcessorConstants.EntryStatusCodes.CustomsDeclarationAccepted;
			AssertEquals(true, entry.HasBeenLodgedAtCustoms);

			entry.CH_EntryStatus = MessageProcessorConstants.EntryStatusCodes.PreDeclarationAccepted;
			AssertEquals(true, entry.HasBeenLodgedAtCustoms);

			entry.CH_EntryStatus = MessageProcessorConstants.EntryStatusCodes.Cancelled;
			AssertEquals(true, entry.HasBeenLodgedAtCustoms);

			entry.CH_EntryStatus = MessageProcessorConstants.EntryStatusCodes.Error;
			AssertEquals(false, entry.HasBeenLodgedAtCustoms);
		});
	}

	public void TestIsWaitingForResponse()
	{
		var entry = Factory.New<CusEntryHeader>();
		CombineAssertions(() =>
		{
			AssertEquals(false, entry.IsWaitingForResponse);

			entry.CH_Status = MessageStatusList.Codes.AwaitingResponse;
			AssertEquals(true, entry.IsWaitingForResponse);
		});
	}

	public void TestCH_Status_ReCalculateStatusDetails_CH_EntryStatus_AwaitingResponse()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		entryHeader.CH_Status = MessageStatusList.Codes.AwaitingResponse;
		CombineAssertions(() =>
		{
			AssertEquals("CH_Status", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
			AssertEquals("CH_EntryStatus", ZString.Empty, entryHeader.CH_EntryStatus);
		});
	}

	public void TestSetAsFailedFromTransmissionDoesNotOverrideEntryStatus()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		entryHeader.CH_Status = "AWO";
		entryHeader.CH_EntryStatus = "NBR";
		declaration.JE_EntryStatus = "NBR";

		entryHeader.SetAsFailedFromTransmission();

		CombineAssertions("SetAsFailedFromTransmission(), Check only CH_Status has been updated", () =>
		{
			AssertEquals("CH_Status", "FFT", entryHeader.CH_Status);
			AssertEquals("CH_EntryStatus", "NBR", entryHeader.CH_EntryStatus);
			AssertEquals("JE_EntryStatus", "NBR", declaration.JE_EntryStatus);
		});
	}

	public void TestIsFailedFromTrasmission()
	{
		CombineAssertions(() =>
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_EntryStatus = "FFT";
			entryHeader.CH_Status = "";
			AssertEquals("When CH_Status is Empty and CH_EntryStatus is FFT, IsFailedFromTrasmission", false, entryHeader.IsFailedFromTransmission);

			entryHeader.CH_EntryStatus = "";
			entryHeader.CH_Status = "FFT";
			AssertEquals("When CH_Status is FFT and CH_EntryStatus is Empty, IsFailedFromTrasmission", true, entryHeader.IsFailedFromTransmission);
		});
	}

	public void TestSetAsFailedFromTransmissionSetsT2IEDIMessageStatus()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		entryHeader.CH_Status = "AWO";
		entryHeader.CH_EntryStatus = "NBR";
		declaration.JE_EntryStatus = "NBR";

		var message1 = entryHeader.Messages.AddNew();
		message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		message1.EM_Status = EDIMessage.Status.Sent;
		message1.EM_MessageType = DeclarationMessageTypeList.Codes.T2lReceptionPous;
		System.Threading.Thread.Sleep(1000);

		var message2 = entryHeader.Messages.AddNew();
		message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		message2.EM_Status = EDIMessage.Status.Sent;
		message2.EM_MessageType = DeclarationMessageTypeList.Codes.T2lReceptionPous;
		System.Threading.Thread.Sleep(1000);

		var message3 = entryHeader.Messages.AddNew();
		message3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		message3.EM_Status = EDIMessage.Status.Sent;
		message3.EM_MessageType = DeclarationMessageTypeList.Codes.T2lReceptionPous;
		System.Threading.Thread.Sleep(1000);

		var message4 = entryHeader.Messages.AddNew();
		message4.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		message4.EM_Status = EDIMessage.Status.Sent;
		message4.EM_MessageType = DeclarationMessageTypeList.Codes.T2lPresentationPous;
		System.Threading.Thread.Sleep(1000);

		var message5 = entryHeader.Messages.AddNew();
		message5.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		message5.EM_Status = EDIMessage.Status.Received;
		message5.EM_MessageType = DeclarationMessageTypeList.Codes.T2lReceptionPous;
		System.Threading.Thread.Sleep(1000);

		var message6 = entryHeader.Messages.AddNew();
		message6.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		message6.EM_Status = EDIMessage.Status.Sent;
		message6.EM_MessageType = DeclarationMessageTypeList.Codes.T2lReceptionPous;
		System.Threading.Thread.Sleep(1000);

		entryHeader.SetAsFailedFromTransmission();

		CombineAssertions(() =>
		{
			AssertEquals("CH_Status", "FFT", entryHeader.CH_Status);
			AssertEquals("CH_EntryStatus", "NBR", entryHeader.CH_EntryStatus);
			AssertEquals("JE_EntryStatus", "NBR", declaration.JE_EntryStatus);

			AssertEquals("message1 has same Status", "SNT", message1.EM_Status);
			AssertEquals("message2 has same Status", "SNT", message2.EM_Status);
			AssertEquals("message3 has new Status sice it is the latest TRX message with type T2I and status SNT", "FFT", message3.EM_Status);
			AssertEquals("message4 has same Status since it has message type not T2I", "SNT", message4.EM_Status);
			AssertEquals("message5 has same Status since it has status not SNT", "RCV", message5.EM_Status);
			AssertEquals("message6 has same Status since it is not TRX", "SNT", message6.EM_Status);
		});
	}

	public void TestIESMessageInfoProvider_Broker()
	{
		CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var esMessageInfoProvider = entryHeader as IESMessageInfoProvider;
			declaration.JE_GS_NKCusAgent = ZString.Empty;
			AssertNull("Broker is null", esMessageInfoProvider.Broker);

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "AZM";
			declaration.JE_GS_NKCusAgent = staff.GS_Code;
			AssertEquals("Broker is not null", staff, esMessageInfoProvider.Broker);
		});
	}

	public void TestIESMessageInfoProvider_EntryReference()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		var esMessageInfoProvider = entryHeader as IESMessageInfoProvider;
		entryHeader.CH_BGMReference = "Reference";
		AssertEquals("EntryReference has the correct value", "Reference", esMessageInfoProvider.EntryReference);
	}

	public void TestIESMessageInfoProvider_MRN()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		var esMessageInfoProvider = entryHeader as IESMessageInfoProvider;
		CombineAssertions(() =>
		{
			AssertEquals("MRN has the correct value (empty when entryHeader has no mrn)", ZString.Empty, esMessageInfoProvider.MRN);

			entryHeader.MovementReferenceNumberSetter("20ES00999830001277", ZDateTime.Today);
			AssertEquals("MRN has the correct value", "20ES00999830001277", esMessageInfoProvider.MRN);
		});
	}

	public void TestIESResponseBusinessObject_BranchPK()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		var esResponseBusinessObject = entryHeader as IESResponseBusinessObject;
		AssertEquals("BranchPK has the correct value", entryHeader.Branch.PK, esResponseBusinessObject.BranchPK);
	}

	public void TestIESResponseBusinessObject_MessageCollection()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		var esResponseBusinessObject = entryHeader as IESResponseBusinessObject;
		entryHeader.Messages.AddNew();
		AssertEquals("MessageCollection has the correct value", entryHeader.Messages, esResponseBusinessObject.MessageCollection);
	}

	public void TestIESResponseBOMessageStatus_MessageStatus()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		var esResponseBusinessObject = entryHeader as IESResponseBOMessageStatus;
		esResponseBusinessObject.MessageStatus = "AAA";
		AssertEquals("MessageStatus has set the correct value", "AAA", entryHeader.CH_Status);
	}

	public void TestIPollingTransactionParent_CertificateName()
	{
		CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var pollingTransactionParent = entryHeader as IPollingTransactionParent;
			declaration.JE_CustomsProfile = ZString.Empty;
			AssertEquals("CertificateName is empty", ZString.Empty, pollingTransactionParent.CertificateName);

			declaration.JE_CustomsProfile = "cert";
			AssertEquals("CertificateName is not empty", "cert", pollingTransactionParent.CertificateName);
		});
	}

	public void TestIPollingTransactionParent_IsTest()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var pollingTransactionParent = entryHeader as IPollingTransactionParent;

		CombineAssertions(() =>
		{
			var registrationMock = new Mock<IProductRegistration>();
			registrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(false);
			registrationMock.Setup(r => r.Key.DatabaseType).Returns(DatabaseTypes.Codes.Production);
			using (ObjectFactory.Substitute(registrationMock.Object))
			{
				AssertEquals("IsTest is false when PRD and external environment", false, pollingTransactionParent.IsTest);
			}

			registrationMock.Setup(r => r.Key.DatabaseType).Returns(DatabaseTypes.Codes.Test);
			using (ObjectFactory.Substitute(registrationMock.Object))
			{
				AssertEquals("IsTest is true when TST and external environment", true, pollingTransactionParent.IsTest);
			}

			registrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(true);
			registrationMock.Setup(r => r.Key.DatabaseType).Returns(DatabaseTypes.Codes.Production);
			using (ObjectFactory.Substitute(registrationMock.Object))
			{
				AssertEquals("IsTest is false when PRD and internal environment", false, pollingTransactionParent.IsTest);
			}

			registrationMock.Setup(r => r.Key.DatabaseType).Returns(DatabaseTypes.Codes.Test);
			using (ObjectFactory.Substitute(registrationMock.Object))
			{
				declaration.ZG_IsTrainingDeclaration = true;
				AssertEquals("IsTest is true when TST and internal environment when flag is checked", true, pollingTransactionParent.IsTest);

				declaration.ZG_IsTrainingDeclaration = false;
				AssertEquals("IsTest is false when TST and internal environment when flag is not checked", false, pollingTransactionParent.IsTest);
			}
		});
	}

	public void TestIPollingTransactionParent_Broker()
	{
		CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var pollingTransactionParent = entryHeader as IPollingTransactionParent;
			declaration.JE_GS_NKCusAgent = ZString.Empty;
			AssertNull("Broker is null", pollingTransactionParent.Broker);

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "AZM";
			declaration.JE_GS_NKCusAgent = staff.GS_Code;
			AssertEquals("Broker is not null", staff, pollingTransactionParent.Broker);
		});
	}

	public void TestIPollingTransactionParent_Declarant()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		var orgProxyAddress = Factory.New<OrgAddress>();
		var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
		orgProxyAddress.OA_OH = orgProxy.PK;
		declaration.Branch.GB_OH_OrgProxy = orgProxy.PK;

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.ZG_UCC6Version = 1;

		CombineAssertions(() =>
		{
			var pollingTransactionParent = entryHeader as IPollingTransactionParent;

			declaration.DeclarantOrgAddress.OA_OH = ZGuid.Empty;
			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_OA_Representative = ZGuid.Empty;
			AssertNull("Declarant is null when exporter, declarant and representative are not declared when export and ZG_UCC6Version is > 0", pollingTransactionParent.Declarant);

			var exporter = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Supplier = exporter.PK;
			AssertEquals("Declarant is not null and filled by exporter when it is declared and declarant and representative are not when export and ZG_UCC6Version is > 0", exporter, pollingTransactionParent.Declarant);

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declaration.DeclarantOrgAddress.OA_OH = declarant.PK;
			AssertEquals("Declarant is not null and filled by declarant when it is declared and representative is not when export and ZG_UCC6Version is > 0", declarant, pollingTransactionParent.Declarant);

			var representativeAddress = Factory.New<OrgAddress>();
			var representative = Factory.NewWithValidTestData<OrgHeader>();
			representativeAddress.OA_OH = representative.PK;
			declaration.JE_OA_Representative = representativeAddress.PK;
			AssertEquals("Declarant is not null and filled by representative when it is declared when export and ZG_UCC6Version is > 0", representative, pollingTransactionParent.Declarant);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Declarant is not null and filled by declarant when it is declared and import, even if representative is declared", declarant, pollingTransactionParent.Declarant);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Declarant is not null and filled by representative when it is declared, when export and ZG_UCC6Version is > 0", representative, pollingTransactionParent.Declarant);

			entryHeader.ZG_UCC6Version = 0;
			AssertEquals("Declarant is not null and filled by declarant when it is declared and export but ZG_UCC6Version is 0, even if representative is declared", declarant, pollingTransactionParent.Declarant);

			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			AssertEquals("Declarant is not null and filled by branch's orgproxy when declarant is not declared and export but ZG_UCC6Version is 0, even if representative is declared", orgProxy, pollingTransactionParent.Declarant);
		});
	}

	public void TestDefaultStatusDescription()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		AssertEquals(ZString.Empty, entryHeader.DefaultStatusDescription);
	}

	public void TestCH_BGMReferenceShouldNotChange()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = GetNewEntryHeaderFromDeclaration(declaration);
		Factory.Save();
		CombineAssertions(() =>
		{
			entryHeader.CH_BGMReference = "Reference1";
			Factory.Save();
			AssertEquals("CH_BGMReference is set to Reference1", "Reference1", entryHeader.CH_BGMReference);
			declaration.ZG_IsTrainingDeclaration = true;
			Factory.Save();
			AssertEquals("CH_BGMReference should not change after change ZG_IsTrainingDeclaration", "Reference1", entryHeader.CH_BGMReference);

			declaration = Factory.New<JobDeclaration>();
			entryHeader = GetNewEntryHeaderFromDeclaration(declaration);
			Factory.Save();
			entryHeader.CH_BGMReference = "Reference2";
			Factory.Save();
			AssertEquals("CH_BGMReference is set to Reference2", "Reference2", entryHeader.CH_BGMReference);
			declaration.JE_UCR = "NewDUCR";
			Factory.Save();
			AssertEquals("CH_BGMReference should not change after change DUCR", "Reference2", entryHeader.CH_BGMReference);
		});
	}

	[TestDate(2021, 3, 12, 4, 0, 0)]
	public void TestFillInBGMOnSaving()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
		var entry = declaration.ActiveEntryHeaders.AddNew();

		var registrationMock = new Mock<IProductRegistration>();
		registrationMock.Setup(r => r.Key.EnterpriseCode).Returns("AAA");
		registrationMock.Setup(r => r.Key.ServerCode).Returns("JPB");

		using (ObjectFactory.Substitute(registrationMock.Object))
		using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
		{
			CombineAssertions(() =>
			{
				AssertEquals("CH_BGMReference is empty before saving", ZString.Empty, entry.CH_BGMReference);

				var registrationMockKey = registrationMock.Object.Key;
				var expectedNumberFountain = Env.NumberFountains.ESBGMReference(ZDate.Today.Year.ToString("0000"), registrationMockKey.EnterpriseCode, registrationMockKey.ServerCode);
				expectedNumberFountain.SetNext(Factory, 1);

				Factory.Save();
				AssertEquals("CH_BGMReference is not empty after saving (it was set because it was not in database and empty)", "ES210000001AAAJPB", entry.CH_BGMReference);

				expectedNumberFountain.SetNext(Factory, 5);

				entry.CH_BGMReference = ZString.Empty;
				Factory.Save();
				AssertEquals("CH_BGMReference is not empty after saving (it was set because it was empty)", "ES210000005AAAJPB", entry.CH_BGMReference);

				expectedNumberFountain.SetNext(Factory, 10);

				var entry2 = declaration.ActiveEntryHeaders.AddNew();
				entry2.CH_BGMReference = "AAAAA";
				Factory.Save();
				AssertEquals("CH_BGMReference is not empty (and NOT changed from set value) after saving (it was not set because is was not in database but declaration in not BLT)", "AAAAA", entry2.CH_BGMReference);

				declaration.JE_ApplicationCode = Enterprise.Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				var entry3 = declaration.ActiveEntryHeaders.AddNew();
				entry3.CH_BGMReference = "BBBBB";
				Factory.Save();
				AssertEquals("CH_BGMReference is not empty (and changed from set value) after saving (it was set because is was not in database and declaration in BLT)", "ES210000010AAAJPB", entry3.CH_BGMReference);
			});
		}
	}

	CusEntryHeader SetUpCompleteDeclaration()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var countryCode = Core.Constants.CountryCodes.Spain;
		var startDate = ZDateTime.Today.AddDays(-1);
		var endDate = ZDateTime.Today.AddDays(1);

		var mapType = helper.CreateCusMapType(UniversalReferenceConstants.RefCusMapType.RetailerFee, Universal.MapDirectionList.Codes.BTH, "Retailer fee Spain", true);
		helper.CreateCusMap(mapType.ZZP_MapType, "IV1", "RQ1", startDate, endDate, countryCode);

		var taxOrFeeType = helper.CreateRefCusTaxOrFeeType("VAT");
		var taxOrFee = helper.CreateTaxOrFee("RQ1", 0.0520, countryCode, startDate, endDate);
		taxOrFee.ZZF_ZX0_NKTaxOrFeeType = taxOrFeeType.ZX0_TaxOrFeeType;
		Factory.Save();

		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

		var importer = Factory.NewWithValidTestData<OrgHeader>();
		var addInfoCusImp = ESOrgImpAddInfo.Get(importer);
		addInfoCusImp.ZO_Retailer = true;
		declaration.JE_OH_Importer = importer.PK;

		var invoice = declaration.Invoices.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_ZZF_NKTaxType = "IV1";

		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction.PK;
		invoiceLine2.JI_ZZF_NKTaxType = "IV1";

		var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge done", true, mergeResult);
		Factory.Save();

		return declaration.CustomsEntryHeaders[0];
	}

	public void TestResetCancelledEntry()
	{
		#region Add data to EntryHeader

		var entryHeader = SetUpCompleteDeclaration();
		entryHeader.CH_BGMReference = "Reference";
		entryHeader.CH_EntryStatus = EntryStatusCodes.Cancelled;
		entryHeader.MovementReferenceNumberSetter("20ES00999830001277", new ZDateTime(2021, 04, 15, 00, 00, 00), CircuitCodeList.Codes.GREEN);
		entryHeader.SetCircuitCan(CircuitCodeList.Codes.RED);
		entryHeader.ZG_ExportMRN = "20EXP0999930006184";
		entryHeader.CH_EntryReleaseDate = new ZDateTime(2020, 10, 01, 00, 00, 00);
		entryHeader.ZG_LimitPaymentDate = new ZDateTime(2022, 05, 15, 00, 00, 00);
		entryHeader.ZG_ATCLimitPaymentDate = new ZDateTime(2023, 05, 15, 00, 00, 00);
		entryHeader.SetCSVClearanceNum("TEST444444444444");
		entryHeader.ZG_CSVImportCertificate = "CRTF444444444444";
		entryHeader.ZG_PaymentProofNumber = "1234";
		entryHeader.ZG_ATCPaymentProofNumber = "4321";
		entryHeader.ZG_Parallel = true;
		entryHeader.ZG_DJPMRN = "20DJP0999930006184";
		entryHeader.ZG_RequestDispatch = "S";
		entryHeader.ZG_LimitDateOfArrival = new ZDateTime(2022, 12, 30, 00, 00, 00);
		entryHeader.EUH_EADPrintProcedure = "1";
		entryHeader.ZG_CSVT2L = "CSVT2L";
		entryHeader.IndirectExport = true;
		entryHeader.ZG_ClearanceResult = "A2";

		var entryLine1 = entryHeader.MergedLines[0];
		var entryLineFee1 = Factory.NewWithValidTestData<CusEntryLineFee>();
		entryLineFee1.CF_CL = entryLine1.PK;
		entryLineFee1.CF_ChargeType = "AAA";
		entryLine1.Fees.Add(entryLineFee1);
		var entryLineFee2 = Factory.NewWithValidTestData<CusEntryLineFee>();
		entryLineFee2.CF_CL = entryLine1.PK;
		entryLineFee2.CF_ChargeType = "BBB";
		entryLine1.Fees.Add(entryLineFee2);

		var entryLine1FeeB00 = Factory.NewWithValidTestData<CusEntryLineFee>();
		entryLine1FeeB00.CF_CL = entryLine1.PK;
		entryLine1FeeB00.CF_ChargeType = "B00";
		entryLine1.Fees.Add(entryLine1FeeB00);

		var suppDoc1 = Factory.New<SupportingDocument>();
		suppDoc1.CSI_Code = "X001";
		suppDoc1.CSI_ReferenceNumber = "ES3600000001";
		suppDoc1.CSI_ParentID = entryLine1.PK;
		suppDoc1.CSI_ParentTableCode = entryLine1.TablePrefix;

		var suppDoc2 = Factory.New<SupportingDocument>();
		suppDoc2.CSI_Code = "X002";
		suppDoc2.CSI_ReferenceNumber = "ES3600000002";
		suppDoc2.CSI_ParentID = entryLine1.PK;
		suppDoc2.CSI_ParentTableCode = entryLine1.TablePrefix;

		var entryLine2 = entryHeader.MergedLines[1];
		var entryLineFee3 = Factory.NewWithValidTestData<CusEntryLineFee>();
		entryLineFee3.CF_CL = entryLine2.PK;
		entryLineFee3.CF_ChargeType = "CCC";
		entryLine2.Fees.Add(entryLineFee3);
		var entryLine2FeeB00 = Factory.NewWithValidTestData<CusEntryLineFee>();
		entryLine2FeeB00.CF_CL = entryLine2.PK;
		entryLine2FeeB00.CF_ChargeType = "B00";
		entryLine2.Fees.Add(entryLine2FeeB00);

		var suppDoc3 = Factory.New<SupportingDocument>();
		suppDoc3.CSI_Code = "X003";
		suppDoc3.CSI_ReferenceNumber = "ES3600000003";
		suppDoc3.CSI_ParentID = entryLine2.PK;
		suppDoc3.CSI_ParentTableCode = entryLine2.TablePrefix;

		var oldMessage1 = Factory.New<ESEDIMessage>();
		oldMessage1.EM_IsActive = true;
		entryHeader.Messages.Add(oldMessage1);
		var oldMessage2 = Factory.New<ESEDIMessage>();
		oldMessage2.EM_IsActive = true;
		entryHeader.Messages.Add(oldMessage2);

		var mergeResult = entryHeader.Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge done", true, mergeResult);

		Factory.Save();

		#endregion

		CombineAssertions("Before calling ResetCancelledEntry", () =>
		{
			entryHeader.Reload();

			AssertEquals("Entry header CH_BGMReference", "Reference", entryHeader.CH_BGMReference);
			AssertEquals("Entry header CH_EntryStatus", EntryStatusCodes.Cancelled, entryHeader.CH_EntryStatus);
			AssertEquals("Entry header MovementReferenceNumber", "20ES00999830001277", entryHeader.MovementReferenceNumber);
			AssertEquals("Entry header MovementReferenceNumberEntryStatus", CircuitCodeList.Codes.GREEN, entryHeader.MovementReferenceNumberEntryStatus);
			AssertEquals("Entry header CircuitCan", CircuitCodeList.Codes.RED, entryHeader.CircuitCan);
			AssertEquals("Entry header ZG_ExportMRN", "20EXP0999930006184", entryHeader.ZG_ExportMRN);
			AssertEquals("Entry header MovementReferenceNumberIssueDate", new ZDateTime(2021, 04, 15, 00, 00, 00), entryHeader.MovementReferenceNumberIssueDate);
			AssertEquals("Entry header CH_EntryReleaseDate", new ZDateTime(2020, 10, 01, 00, 00, 00), entryHeader.CH_EntryReleaseDate);
			AssertEquals("Entry header ZG_LimitPaymentDate", new ZDateTime(2022, 05, 15, 00, 00, 00), entryHeader.ZG_LimitPaymentDate);
			AssertEquals("Entry header ZG_ATCLimitPaymentDate", new ZDateTime(2023, 05, 15, 00, 00, 00), entryHeader.ZG_ATCLimitPaymentDate);
			AssertEquals("Entry header CSVClearance", "TEST444444444444", entryHeader.CSVClearance);
			AssertEquals("Entry header ZG_CSVImportCertificate", "CRTF444444444444", entryHeader.ZG_CSVImportCertificate);
			AssertEquals("Entry header ZG_PaymentProofNumber", "1234", entryHeader.ZG_PaymentProofNumber);
			AssertEquals("Entry header ZG_ATCPaymentProofNumber", "4321", entryHeader.ZG_ATCPaymentProofNumber);
			AssertEquals("Entry header ZG_Parallel", true, entryHeader.ZG_Parallel);
			AssertEquals("Entry header ZG_DJPMRN", "20DJP0999930006184", entryHeader.ZG_DJPMRN);
			AssertEquals("Entry header ZG_RequestDispatch", "S", entryHeader.ZG_RequestDispatch);
			AssertEquals("Entry header ZG_LimitDateOfArrival", new ZDateTime(2022, 12, 30, 00, 00, 00), entryHeader.ZG_LimitDateOfArrival);
			AssertEquals("Entry header EUH_EADPrintProcedure", "1", entryHeader.EUH_EADPrintProcedure);
			AssertEquals("Entry header ZG_CSVT2L", "CSVT2L", entryHeader.ZG_CSVT2L);
			AssertEquals("Entry header IndirectExport", true, entryHeader.IndirectExport);
			AssertEquals("Entry header ZG_ClearanceResult", "A2", entryHeader.ZG_ClearanceResult);

			AssertContainsExactElementsInAnyOrder("Entry header's first entry line fees", new ZString[] { "B01", "AAA", "BBB", "B00" }, entryLine1.Fees.Cast<CusEntryLineFee>().Select(x => x.CF_ChargeType).ToArray());
			AssertContainsExactElementsInAnyOrder("Entry header's second entry line fees", new ZString[] { "B01", "CCC", "B00" }, entryLine2.Fees.Cast<CusEntryLineFee>().Select(x => x.CF_ChargeType).ToArray());

			AssertEquals("There are 2 CL supportingDocuments associated to the entry header's first entry line", 2, entryLine1.GetPreviouslySentSupportingDocuments().Length);
			AssertEquals("There is 1 CL supportingDocument associated to the entry header's second entry line", 1, entryLine2.GetPreviouslySentSupportingDocuments().Length);

			AssertEquals("EntryHeader Messages are active", 2, entryHeader.Messages.Cast<EDIMessage>().Count(x => x.EM_IsActive));
		});

		CombineAssertions("After calling ResetCancelledEntry", () =>
		{
			entryHeader.ResetCancelledEntry();

			AssertEquals("Entry header CH_BGMReference", ZString.Empty, entryHeader.CH_BGMReference);
			AssertEquals("Entry header CH_EntryStatus", ZString.Empty, entryHeader.CH_EntryStatus);
			AssertEquals("Entry header MovementReferenceNumber", ZString.Empty, entryHeader.MovementReferenceNumber);
			AssertEquals("Entry header MovementReferenceNumberEntryStatus", ZString.Empty, entryHeader.MovementReferenceNumberEntryStatus);
			AssertEquals("Entry header CircuitCan", ZString.Empty, entryHeader.CircuitCan);
			AssertEquals("Entry header ZG_ExportMRN", ZString.Empty, entryHeader.ZG_ExportMRN);
			AssertEquals("Entry header MovementReferenceNumberIssueDate", ZDateTime.Empty, entryHeader.MovementReferenceNumberIssueDate);
			AssertEquals("Entry header CH_EntryReleaseDate", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);
			AssertEquals("Entry header ZG_LimitPaymentDate", ZDateTime.Empty, entryHeader.ZG_LimitPaymentDate);
			AssertEquals("Entry header ZG_ATCLimitPaymentDate", ZDateTime.Empty, entryHeader.ZG_ATCLimitPaymentDate);
			AssertEquals("Entry header CSVClearance", ZString.Empty, entryHeader.CSVClearance);
			AssertEquals("Entry header ZG_CSVImportCertificate", ZString.Empty, entryHeader.ZG_CSVImportCertificate);
			AssertEquals("Entry header ZG_PaymentProofNumber", ZString.Empty, entryHeader.ZG_PaymentProofNumber);
			AssertEquals("Entry header ZG_ATCPaymentProofNumber", ZString.Empty, entryHeader.ZG_ATCPaymentProofNumber);
			AssertEquals("Entry header ZG_Parallel", false, entryHeader.ZG_Parallel);
			AssertEquals("Entry header ZG_DJPMRN", ZString.Empty, entryHeader.ZG_DJPMRN);
			AssertEquals("Entry header ZG_RequestDispatch", ZString.Empty, entryHeader.ZG_RequestDispatch);
			AssertEquals("Entry header ZG_LimitDateOfArrival", ZDateTime.Empty, entryHeader.ZG_LimitDateOfArrival);
			AssertEquals("Entry header EUH_EADPrintProcedure", ZString.Empty, entryHeader.EUH_EADPrintProcedure);
			AssertEquals("Entry header ZG_CSVT2L", ZString.Empty, entryHeader.ZG_CSVT2L);
			AssertEquals("Entry header IndirectExport", false, entryHeader.IndirectExport);
			AssertEquals("Entry header ZG_ClearanceResult", ZString.Empty, entryHeader.ZG_ClearanceResult);

			AssertContainsExactElementsInAnyOrder("Entry header's first entry line fees, no B00 to calculate B01", Array.Empty<ZString>(), entryLine1.Fees.Cast<CusEntryLineFee>().Select(x => x.CF_ChargeType).ToArray());
			AssertContainsExactElementsInAnyOrder("Entry header's second entry line fees, no B00 to calculate B01", Array.Empty<ZString>(), entryLine2.Fees.Cast<CusEntryLineFee>().Select(x => x.CF_ChargeType).ToArray());

			AssertEquals("There are no CL supportingDocuments associated to the entry header's first entry line", false, entryLine1.GetPreviouslySentSupportingDocuments().Any());
			AssertEquals("There are no CL supportingDocuments associated to the entry header's second entry line", false, entryLine2.GetPreviouslySentSupportingDocuments().Any());

			AssertEquals("EntryHeader Messages are not active", false, entryHeader.Messages.Cast<EDIMessage>().Any(x => x.EM_IsActive));
		});
	}

	public void TestGetTotalAmountToDeclare()
	{
		var expectedTotalAmount = 1000.200M;

		var entryHeader = Factory.New<CusEntryHeader>();

		var entryLine = entryHeader.MergedLines.AddNew();
		var fee1 = entryLine.Fees.AddNew();
		fee1.CF_ChargeAmount = expectedTotalAmount / 4;
		var fee2 = entryLine.Fees.AddNew();
		fee2.CF_ChargeAmount = expectedTotalAmount / 4;

		var entryLine2 = entryHeader.MergedLines.AddNew();
		var fee3 = entryLine2.Fees.AddNew();
		fee3.CF_ChargeAmount = expectedTotalAmount / 4;
		var fee4 = entryLine2.Fees.AddNew();
		fee4.CF_ChargeAmount = expectedTotalAmount / 4;

		AssertEquals("Expected sum of all CF_CahrgeAmounts in all merged lines in the entry", expectedTotalAmount, entryHeader.GetTotalAmountToDeclare());
	}

	public void TestGetUrlToLaunch_Import_PDI()
	{
		var expectedMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/inwinvoc/es.aeat.advu.jdit.web.cons.DetalleVUAInt?operacion=3000&CABECERA_MRN=" + expectedMRN;

		var entryHeader = SetEntryHeaderForUrlLauncher(MessageTypeList.Codes.Import, EntrySubStyleList.Codes.C);
		entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;

		AssertGetUrl(entryHeader, expectedMRN, expectedUrl);
	}

	public void TestGetUrlToLaunch_Import_NotPDI_Mainland()
	{
		var expectedMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/inwinvoc/es.aeat.dit.adu.adip.inter.cnt.CImporInternet?CABECERA_MRN=" + expectedMRN;

		var entryHeader = SetEntryHeaderForUrlLauncher(MessageTypeList.Codes.Import, EntrySubStyleList.Codes.B);

		AssertGetUrl(entryHeader, expectedMRN, expectedUrl);
	}

	public void TestGetUrlToLaunch_Import_NotPDI_CanaryIsland()
	{
		var expectedMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/inwinvoc/es.aeat.dit.adu.adip.inter.cnt.CImpInternetVexcan?CABECERA_MRN=" + expectedMRN;

		var entryHeader = SetEntryHeaderForUrlLauncher(MessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, true);

		AssertGetUrl(entryHeader, expectedMRN, expectedUrl);
	}

	public void TestGetUrlToLaunch_Import_T2L_NoPOUS()
	{
		var expectedMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/L/inwinvoc/es.aeat.dit.adu.adtl.gestion.cnt.DetExpedicionT2L?retorno=es.aeat.dit.adu.adtl.gestion.qry.QDocsT2LInt&operacion=1001&clave=" + expectedMRN;

		var entryHeader = SetEntryHeaderForUrlLauncher(MessageTypeList.Codes.Import, EntrySubStyleList.Codes.T2L);
		entryHeader.ZG_POUSVersion = POUSVersionCodes.NoPOUS;

		AssertGetUrl(entryHeader, expectedMRN, expectedUrl);
	}

	public void TestGetUrlToLaunch_Import_T2L_POUS1()
	{
		var expectedMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/L/inwinvoc/es.aeat.dit.adu.adtl.gestion.cnt.DetExpedicionT2L?retorno=es.aeat.dit.adu.adtl.gestion.qry.QDocsT2LInt&operacion=1001&clave=" + expectedMRN;

		var entryHeader = SetEntryHeaderForUrlLauncher(MessageTypeList.Codes.Import, EntrySubStyleList.Codes.T2L);
		entryHeader.ZG_POUSVersion = POUSVersionCodes.POUS;

		AssertGetUrl(entryHeader, expectedMRN, expectedUrl);
	}

	public void TestGetUrlToLaunch_Import_T2L_POUS2()
	{
		var expectedT2CMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADTL-JDIT/T2LDetalle?mrn=" + expectedT2CMRN;

		var entryHeader = SetEntryHeaderForUrlLauncher(MessageTypeList.Codes.Import, EntrySubStyleList.Codes.T2L);
		entryHeader.ZG_POUSVersion = POUSVersionCodes.POUS2;

		AssertGetUrlForT2M(entryHeader, expectedT2CMRN, expectedUrl);
	}

	public void TestGetUrlToLaunch_Import_T2C()
	{
		var expectedT2CMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADTL-JDIT/JECDetalle?mrn=" + expectedT2CMRN;

		var entryHeader = SetEntryHeaderForUrlLauncher(MessageTypeList.Codes.Import, EntrySubStyleList.Codes.T2C);

		AssertGetUrlForT2M(entryHeader, expectedT2CMRN, expectedUrl);
	}

	public void TestGetUrlToLaunch_Export()
	{
		var expectedMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADEX-JDIT/AesDetalle?CLAVE=" + expectedMRN;

		var entryHeader = SetEntryHeaderForUrlLauncher(MessageTypeList.Codes.Export, EntrySubStyleList.Codes.C);

		AssertGetUrl(entryHeader, expectedMRN, expectedUrl);
	}

	public void TestGetUrlToLaunch_Export_T2L()
	{
		var expectedMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADTL-JDIT/T2LDetalle?mrn=" + expectedMRN;

		var entryHeader = SetEntryHeaderForUrlLauncher(MessageTypeList.Codes.Export, EntrySubStyleList.Codes.T2L);

		AssertGetUrl(entryHeader, expectedMRN, expectedUrl);
	}

	public void TestGetUrlToLaunch_Export_EXS()
	{
		var expectedMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/inwinvoc/es.aeat.dit.adu.adrx.inter.CtrInternet?operacion=1032&clave=" + expectedMRN;

		var entryHeader = SetEntryHeaderForUrlLauncher(MessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS);

		AssertGetUrl(entryHeader, expectedMRN, expectedUrl);
	}

	void AssertGetUrl(CusEntryHeader entryHeader, string expectedMRN, string expectedUrl)
	{
		CombineAssertions(() =>
		{
			AssertNullOrEmpty("No url was returned when no mrn", entryHeader.GetUrlToLaunch());

			entryHeader.MovementReferenceNumber = expectedMRN;
			AssertEquals("The correct url has been launched", expectedUrl, entryHeader.GetUrlToLaunch());
		});
	}

	void AssertGetUrlForT2M(CusEntryHeader entryHeader, string expectedT2CMRN, string expectedUrl)
	{
		CombineAssertions(() =>
		{
			AssertNullOrEmpty("No url was returned when no mrn", entryHeader.GetUrlToLaunch());

			entryHeader.MovementReferenceNumber = "mrn";
			AssertNullOrEmpty("No url was returned when mrn but no t2c mrn", entryHeader.GetUrlToLaunch());

			var newEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Spain.T2CMovementReferenceNumber, Core.Constants.CountryCodes.Spain);
			newEntryNumber.CE_EntryNum = expectedT2CMRN;
			newEntryNumber.CE_IssueDate = ZDateTime.Today;
			newEntryNumber.CE_EntryIsSystemGenerated = true;
			AssertEquals("The correct url has been launched", expectedUrl, entryHeader.GetUrlToLaunch());
		});
	}

	CusEntryHeader SetEntryHeaderForUrlLauncher(ZString messageType, ZString entryInstructionSubStyle, bool setCanaryIsland = false)
	{
		var testDataHelper = new ESUniversalReferenceTestDataHelper(Factory);
		var canaryIslandCode = "61";
		testDataHelper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, canaryIslandCode, "Test 61");

		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = messageType;
		dec.ZG_DestinationState = setCanaryIsland ? canaryIslandCode : "28";
		var entryInstruction = dec.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = entryInstructionSubStyle;
		var entryHeader = dec.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		return entryHeader;
	}

	public void TestValidationMode()
	{
		var entryHeader = Factory.New<CusEntryHeader>();

		CombineAssertions(() =>
		{
			AssertEquals("Default validation mode is correct", ValidationModes.None, entryHeader.ValidationMode);

			entryHeader.ValidationMode = ValidationModes.PDI;
			AssertEquals("Validation mode is set correctly", ValidationModes.PDI, entryHeader.ValidationMode);
		});
	}

	public void TestSetDefaultValidationMode()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		AssertEquals("Default validation mode is correct", ValidationModes.None, entryHeader.ValidationMode);
	}

	public void TestCheckSupportingDocumentsHaveProcedure_Declaration()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var invoice = declaration.Invoices.AddNew();
		invoice.InvoiceLines.AddNew();

		var supdoc1 = declaration.SupportingDocuments.AddNew();
		supdoc1.CSI_Code = "9001";
		var supdoc2 = declaration.SupportingDocuments.AddNew();
		supdoc2.CSI_Code = "9002";
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = declaration.CustomsEntryHeaders[0];
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("No supporting document has Procedure declared", false, entryHeader.CheckSupportingDocumentsHaveProcedure());

			supdoc1.CSI_Procedure = "A";
			AssertEquals("At least one supporting document has Procedure declared", true, entryHeader.CheckSupportingDocumentsHaveProcedure());
		});
	}

	public void TestCheckSupportingDocumentsHaveProcedure_InvoiceHeader()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var invoice = declaration.Invoices.AddNew();
		invoice.InvoiceLines.AddNew();

		var supdoc1 = invoice.SupportingDocuments.AddNew();
		supdoc1.CSI_Code = "9001";
		var supdoc2 = invoice.SupportingDocuments.AddNew();
		supdoc2.CSI_Code = "9002";
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = declaration.CustomsEntryHeaders[0];
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("No supporting document has Procedure declared", false, entryHeader.CheckSupportingDocumentsHaveProcedure());

			supdoc1.CSI_Procedure = "A";
			AssertEquals("At least one supporting document has Procedure declared", true, entryHeader.CheckSupportingDocumentsHaveProcedure());
		});
	}

	public void TestCheckSupportingDocumentsHaveProcedure_InvoiceLine()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		var supdoc1 = invoiceLine.SupportingDocuments.AddNew();
		supdoc1.CSI_Code = "9001";
		var supdoc2 = invoiceLine.SupportingDocuments.AddNew();
		supdoc2.CSI_Code = "9002";
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = declaration.CustomsEntryHeaders[0];
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("No supporting document has Procedure declared", false, entryHeader.CheckSupportingDocumentsHaveProcedure());

			supdoc1.CSI_Procedure = "A";
			AssertEquals("At least one supporting document has Procedure declared", true, entryHeader.CheckSupportingDocumentsHaveProcedure());
		});
	}

	public void TestCheckPreviousDocumentsExist_Declaration()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var invoice = declaration.Invoices.AddNew();
		invoice.InvoiceLines.AddNew();
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = declaration.CustomsEntryHeaders[0];
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("No previous documents declared", false, entryHeader.CheckPreviousDocumentsExist());

			var prevdoc = declaration.PreviousDocuments.AddNew();
			prevdoc.CSI_Code = "AA";
			AssertEquals("A previous document was declared", true, entryHeader.CheckPreviousDocumentsExist());
		});
	}

	public void TestCheckPreviousDocumentsExist_InvoiceHeader()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var invoice = declaration.Invoices.AddNew();
		invoice.InvoiceLines.AddNew();
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = declaration.CustomsEntryHeaders[0];
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("No previous documents declared", false, entryHeader.CheckPreviousDocumentsExist());

			var prevdoc = invoice.PreviousDocuments.AddNew();
			prevdoc.CSI_Code = "AA";
			AssertEquals("A previous document was declared", true, entryHeader.CheckPreviousDocumentsExist());
		});
	}

	public void TestCheckPreviousDocumentsExist_InvoiceLine()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = declaration.CustomsEntryHeaders[0];
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("No previous documents declared", false, entryHeader.CheckPreviousDocumentsExist());

			var prevdoc = invoiceLine.PreviousDocuments.AddNew();
			prevdoc.CSI_Code = "AA";
			AssertEquals("A previous document was declared", true, entryHeader.CheckPreviousDocumentsExist());
		});
	}

	public void TestCheckAnyEntryLineHasSupportingDocumentsToSend_Declaration()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var invoice = declaration.Invoices.AddNew();
		invoice.InvoiceLines.AddNew();
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = declaration.CustomsEntryHeaders[0];
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("No supporting documents declared", false, entryHeader.CheckAnyEntryLineHasSupportingDocumentsToSend());

			var supdoc = declaration.SupportingDocuments.AddNew();
			supdoc.CSI_Code = "AA";
			AssertEquals("A supporting document was declared to send", true, entryHeader.CheckAnyEntryLineHasSupportingDocumentsToSend());

			var entryLine = entryHeader.MergedLines[0];
			var supdoc2 = Factory.New<SupportingDocument>();
			supdoc2.CSI_ParentID = entryLine.PK;
			supdoc2.CSI_ParentTableCode = entryLine.TablePrefix;
			supdoc2.CSI_Status = "ACC";
			supdoc2.CSI_Code = "AA";
			AssertEquals("A supporting document was declared but it was already sent", false, entryHeader.CheckAnyEntryLineHasSupportingDocumentsToSend());
		});
	}

	public void TestCheckAnyEntryLineHasSupportingDocumentsToSend_InvoiceHeader()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var invoice = declaration.Invoices.AddNew();
		invoice.InvoiceLines.AddNew();
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = declaration.CustomsEntryHeaders[0];
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("No supporting documents declared", false, entryHeader.CheckAnyEntryLineHasSupportingDocumentsToSend());

			var supdoc = invoice.SupportingDocuments.AddNew();
			supdoc.CSI_Code = "AA";
			AssertEquals("A supporting document was declared to send", true, entryHeader.CheckAnyEntryLineHasSupportingDocumentsToSend());

			var entryLine = entryHeader.MergedLines[0];
			var supdoc2 = Factory.New<SupportingDocument>();
			supdoc2.CSI_ParentID = entryLine.PK;
			supdoc2.CSI_ParentTableCode = entryLine.TablePrefix;
			supdoc2.CSI_Status = "ACC";
			supdoc2.CSI_Code = "AA";
			AssertEquals("A supporting document was declared but it was already sent", false, entryHeader.CheckAnyEntryLineHasSupportingDocumentsToSend());
		});
	}

	public void TestCheckAnyEntryLineHasSupportingDocumentsToSend_InvoiceLine()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = declaration.CustomsEntryHeaders[0];
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("No supporting documents declared", false, entryHeader.CheckAnyEntryLineHasSupportingDocumentsToSend());

			var supdoc = invoiceLine.SupportingDocuments.AddNew();
			supdoc.CSI_Code = "AA";
			AssertEquals("A supporting document was declared to send", true, entryHeader.CheckAnyEntryLineHasSupportingDocumentsToSend());

			var entryLine = entryHeader.MergedLines[0];
			var supdoc2 = Factory.New<SupportingDocument>();
			supdoc2.CSI_ParentID = entryLine.PK;
			supdoc2.CSI_ParentTableCode = entryLine.TablePrefix;
			supdoc2.CSI_Status = "ACC";
			supdoc2.CSI_Code = "AA";
			AssertEquals("A supporting document was declared but it was already sent", false, entryHeader.CheckAnyEntryLineHasSupportingDocumentsToSend());
		});
	}

	public void TestAreAllDescriptionLengthCorrectForImport()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = declaration.CustomsEntryHeaders[0];
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("Description for the only invoice line is empty (less than 5 chars)", false, entryHeader.AreAllDescriptionLengthCorrectForImport());

			invoiceLine1.JI_Description = "12345";
			AssertEquals("Description for the only invoice line has 5 chars", true, entryHeader.AreAllDescriptionLengthCorrectForImport());

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			var entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_Description = "123";
			AssertEquals("Description for at least one entry line, the second invoice line, has 3 chars (less than 5 chars)", false, entryHeader.AreAllDescriptionLengthCorrectForImport());

			invoiceLine2.JI_Description = "123456";
			AssertEquals("Description for both invoice lines have more than 5 chars", true, entryHeader.AreAllDescriptionLengthCorrectForImport());
		});
	}

	public void TestSetCSVClearance_Import()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.CH_BGMReference = "ES00001";

		CombineAssertions(() =>
		{
			entryHeader.SetCSVClearance("CLEARANCE1234567");
			AssertEquals("CSV clearance has been changed when the pop up was accepted", "CLEARANCE1234567", entryHeader.CSVClearance);
			AssertEquals("When CSV Clearance is set to a value and entry instruction is A, entry status is set to CLR", EntryStatusCodes.Cleared, entryHeader.CH_EntryStatus);
			AssertEquals("New event in logs", "|NEW=CLEARANCE1234567|RES=Manually Added CSV Clearance Code to Entry ES00001|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
			entryHeader.SetCSVClearance("CLEARANCE1234567");
			AssertEquals("When CSV Clearance is set to the same value entry status is left as it is, even when entry instruction is B", EntryStatusCodes.Cleared, entryHeader.CH_EntryStatus);
			AssertEquals("The Last Event is still the same cause the CSV Clearance does not change", "|NEW=CLEARANCE1234567|RES=Manually Added CSV Clearance Code to Entry ES00001|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);

			// 5ms sleep to ensure second change log's event time is after first change log's time, taking into account SQL datetime precision (3ms).
			Thread.Sleep(5);

			entryHeader.SetCSVClearance("AAAAAAAAAAAAAAAA");
			AssertEquals("CSV clearance has been changed when the pop up was accepted a second time", "AAAAAAAAAAAAAAAA", entryHeader.CSVClearance);
			AssertEquals("When CSV Clearance is set to a new value and entry instruction is B, entry status is set to CLP", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);
			AssertEquals("New event in logs for second change", "|NEW=AAAAAAAAAAAAAAAA|OLD=CLEARANCE1234567|RES=Manually Added CSV Clearance Code to Entry ES00001|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);

			// 5ms sleep to ensure second change log's event time is after first change log's time, taking into account SQL datetime precision (3ms).
			Thread.Sleep(5);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.X;
			entryHeader.SetCSVClearance(ZString.Empty);
			AssertEquals("CSV clearance has been changed when the pop up was accepted a third time", ZString.Empty, entryHeader.CSVClearance);
			AssertEquals("When CSV Clearance is set to empty entry status is left as it is", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);
			AssertEquals("New event in logs for third change", "|NEW=Empty|OLD=AAAAAAAAAAAAAAAA|RES=Manually Added CSV Clearance Code to Entry ES00001|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);

			entryHeader.SetCSVClearance("AAAAAAAAAAAAAAAB");
			AssertEquals("CSV clearance has been changed when the pop up was accepted a fourth time", "AAAAAAAAAAAAAAAB", entryHeader.CSVClearance);
			AssertEquals("When CSV Clearance is set to a new value and entry instruction is X, entry status is set to CLR", EntryStatusCodes.Cleared, entryHeader.CH_EntryStatus);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
			entryHeader.SetCSVClearance("AAAAAAAAAAAAAAAC");
			AssertEquals("CSV clearance has been changed when the pop up was accepted a fifth time", "AAAAAAAAAAAAAAAC", entryHeader.CSVClearance);
			AssertEquals("When CSV Clearance is set to a new value and entry instruction is C, entry status is set to CLP", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Y;
			entryHeader.SetCSVClearance("AAAAAAAAAAAAAAAD");
			AssertEquals("CSV clearance has been changed when the pop up was accepted a sixth time", "AAAAAAAAAAAAAAAD", entryHeader.CSVClearance);
			AssertEquals("When CSV Clearance is set to a new value and entry instruction is Y, entry status is set to CLR", EntryStatusCodes.Cleared, entryHeader.CH_EntryStatus);

			entryHeader.CH_EntryStatus = "AAA";
			AssertEquals("Prereq: Entry status is AAA", "AAA", entryHeader.CH_EntryStatus);
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Z;
			entryHeader.SetCSVClearance("AAAAAAAAAAAAAAAE");
			AssertEquals("CSV clearance has been changed when the pop up was accepted a seventh time", "AAAAAAAAAAAAAAAE", entryHeader.CSVClearance);
			AssertEquals("When CSV Clearance is set to a new value and entry instruction is Z, entry status is set to CLR", EntryStatusCodes.Cleared, entryHeader.CH_EntryStatus);

			entryHeader.CH_EntryStatus = "BBB";
			AssertEquals("Prereq: Entry status is BBB", "BBB", entryHeader.CH_EntryStatus);
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			entryHeader.SetCSVClearance("AAAAAAAAAAAAAAAF");
			AssertEquals("CSV clearance has been changed when the pop up was accepted a eighth time", "AAAAAAAAAAAAAAAF", entryHeader.CSVClearance);
			AssertEquals("When CSV Clearance is set to a new value and entry instruction is T2L, entry status is set to CLR", EntryStatusCodes.Cleared, entryHeader.CH_EntryStatus);

			entryHeader.CH_EntryStatus = "CCC";
			AssertEquals("Prereq: Entry status is CCC", "CCC", entryHeader.CH_EntryStatus);
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			entryHeader.SetCSVClearance("AAAAAAAAAAAAAAAG");
			AssertEquals("CSV clearance has been changed when the pop up was accepted a ninth time", "AAAAAAAAAAAAAAAG", entryHeader.CSVClearance);
			AssertEquals("When CSV Clearance is set to a new value and entry instruction is T2C, entry status is set to CLR", EntryStatusCodes.Cleared, entryHeader.CH_EntryStatus);
		});
	}

	public void TestSetCSVClearance_Export()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.CH_BGMReference = "ES00001";

		CombineAssertions(() =>
		{
			entryHeader.SetCSVClearance("CLEARANCE1234567");
			AssertEquals("CSV clearance has been changed when the pop up was accepted", "CLEARANCE1234567", entryHeader.CSVClearance);
			AssertEquals("When CSV Clearance is set to a value and entry instruction is A, entry status is set to CLR", EntryStatusCodes.Cleared, entryHeader.CH_EntryStatus);
			AssertEquals("New event in logs", "|NEW=CLEARANCE1234567|RES=Manually Added CSV Clearance Code to Entry ES00001|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
			entryHeader.SetCSVClearance("CLEARANCE1234567");
			AssertEquals("When CSV Clearance is set to the same value entry status is left as it is, even when entry instruction is B", EntryStatusCodes.Cleared, entryHeader.CH_EntryStatus);
			AssertEquals("The Last Event is still the same cause the CSV Clearance does not change", "|NEW=CLEARANCE1234567|RES=Manually Added CSV Clearance Code to Entry ES00001|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);

			// 5ms sleep to ensure second change log's event time is after first change log's time, taking into account SQL datetime precision (3ms).
			Thread.Sleep(5);

			entryHeader.SetCSVClearance("AAAAAAAAAAAAAAAA");
			AssertEquals("CSV clearance has been changed when the pop up was accepted a second time", "AAAAAAAAAAAAAAAA", entryHeader.CSVClearance);
			AssertEquals("When CSV Clearance is set to a new value and entry instruction is B, entry status is set to CLP", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);
			AssertEquals("New event in logs for second change", "|NEW=AAAAAAAAAAAAAAAA|OLD=CLEARANCE1234567|RES=Manually Added CSV Clearance Code to Entry ES00001|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);

			// 5ms sleep to ensure second change log's event time is after first change log's time, taking into account SQL datetime precision (3ms).
			Thread.Sleep(5);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.X;
			entryHeader.SetCSVClearance(ZString.Empty);
			AssertEquals("CSV clearance has been changed when the pop up was accepted a third time", ZString.Empty, entryHeader.CSVClearance);
			AssertEquals("When CSV Clearance is set to empty entry status is left as it is", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);
			AssertEquals("New event in logs for third change", "|NEW=Empty|OLD=AAAAAAAAAAAAAAAA|RES=Manually Added CSV Clearance Code to Entry ES00001|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);

			entryHeader.SetCSVClearance("AAAAAAAAAAAAAAAB");
			AssertEquals("CSV clearance has been changed when the pop up was accepted a fourth time", "AAAAAAAAAAAAAAAB", entryHeader.CSVClearance);
			AssertEquals("When CSV Clearance is set to a new value and entry instruction is X, entry status is set to CLR", EntryStatusCodes.Cleared, entryHeader.CH_EntryStatus);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
			entryHeader.SetCSVClearance("AAAAAAAAAAAAAAAC");
			AssertEquals("CSV clearance has been changed when the pop up was accepted a fifth time", "AAAAAAAAAAAAAAAC", entryHeader.CSVClearance);
			AssertEquals("When CSV Clearance is set to a new value and entry instruction is C, entry status is set to CLP", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Y;
			entryHeader.SetCSVClearance("AAAAAAAAAAAAAAAD");
			AssertEquals("CSV clearance has been changed when the pop up was accepted a sixth time", "AAAAAAAAAAAAAAAD", entryHeader.CSVClearance);
			AssertEquals("When CSV Clearance is set to a new value and entry instruction is Y, entry status is set to CLR", EntryStatusCodes.Cleared, entryHeader.CH_EntryStatus);

			entryHeader.CH_EntryStatus = "AAA";
			AssertEquals("Prereq: Entry status is AAA", "AAA", entryHeader.CH_EntryStatus);
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Z;
			entryHeader.SetCSVClearance("AAAAAAAAAAAAAAAE");
			AssertEquals("CSV clearance has been changed when the pop up was accepted a seventh time", "AAAAAAAAAAAAAAAE", entryHeader.CSVClearance);
			AssertEquals("When CSV Clearance is set to a new value and entry instruction is Z, entry status is set to EFD", EntryStatusCodes.EffectiveDeparture, entryHeader.CH_EntryStatus);

			entryHeader.CH_EntryStatus = "BBB";
			AssertEquals("Prereq: Entry status is BBB", "BBB", entryHeader.CH_EntryStatus);
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			entryHeader.SetCSVClearance("AAAAAAAAAAAAAAAF");
			AssertEquals("CSV clearance has been changed when the pop up was accepted a eighth time", "AAAAAAAAAAAAAAAF", entryHeader.CSVClearance);
			AssertEquals("When CSV Clearance is set to a new value and entry instruction is T2L, entry status is set to CLR", EntryStatusCodes.Cleared, entryHeader.CH_EntryStatus);

			entryHeader.CH_EntryStatus = "CCC";
			AssertEquals("Prereq: Entry status is CCC", "CCC", entryHeader.CH_EntryStatus);
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			entryHeader.SetCSVClearance("AAAAAAAAAAAAAAAG");
			AssertEquals("CSV clearance has been changed when the pop up was accepted a ninth time", "AAAAAAAAAAAAAAAG", entryHeader.CSVClearance);
			AssertEquals("When CSV Clearance is set to a new value and entry instruction is T2C, entry status is set to CLR", EntryStatusCodes.Cleared, entryHeader.CH_EntryStatus);

			entryHeader.CH_EntryStatus = "DDD";
			AssertEquals("Prereq: Entry status is DDD", "DDD", entryHeader.CH_EntryStatus);
			entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
			entryHeader.SetCSVClearance("AAAAAAAAAAAAAAAH");
			AssertEquals("CSV clearance has been changed when the pop up was accepted a tenth time", "AAAAAAAAAAAAAAAH", entryHeader.CSVClearance);
			AssertEquals("When CSV Clearance is set to a new value and entry instruction is EXS, entry status is set to CLR", EntryStatusCodes.Cleared, entryHeader.CH_EntryStatus);
		});
	}

	public void TestSetClearanceDate()
	{
		var entryHeader = SetUpCompleteDeclaration();
		entryHeader.CH_BGMReference = "ES00003";

		CombineAssertions(() =>
		{
			entryHeader.SetClearanceDate(new ZDateTime(2023, 06, 14, 11, 12, 13));
			AssertEquals("Entry Release Date has been changed when the pop up was accepted", new ZDateTime(2023, 06, 14, 11, 12, 13), entryHeader.CH_EntryReleaseDate);
			AssertEquals("New event in logs", "|NEW=2023-06-14T11:12:13|RES=Manually Added Clearance Date to Entry ES00003|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);

			entryHeader.SetClearanceDate(new ZDateTime(2023, 06, 14, 11, 12, 13));
			AssertEquals("The Last Event is still the same cause the CSV Exit Certificate does not change", "|NEW=2023-06-14T11:12:13|RES=Manually Added Clearance Date to Entry ES00003|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);

			// 5ms sleep to ensure second change log's event time is after first change log's time, taking into account SQL datetime precision (3ms).
			Thread.Sleep(5);

			entryHeader.SetClearanceDate(new ZDateTime(2024, 04, 28, 10, 09, 08));
			AssertEquals("CSV Exit Certificate has been changed when the pop up was accepted a second time", new ZDateTime(2024, 04, 28, 10, 09, 08), entryHeader.CH_EntryReleaseDate);
			AssertEquals("New event in logs for second change", "|NEW=2024-04-28T10:09:08|OLD=2023-06-14T11:12:13|RES=Manually Added Clearance Date to Entry ES00003|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);

			// 5ms sleep to ensure second change log's event time is after first change log's time, taking into account SQL datetime precision (3ms).
			Thread.Sleep(5);

			entryHeader.SetClearanceDate(ZDateTime.Empty);
			AssertEquals("CSV Exit Certificate has been changed when the pop up was accepted a third time", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);
			AssertEquals("New event in logs for third change", "|NEW=Empty|OLD=2024-04-28T10:09:08|RES=Manually Added Clearance Date to Entry ES00003|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);
		});
	}

	public void TestSetImportCertificate()
	{
		var entryHeader = SetUpCompleteDeclaration();
		entryHeader.CH_BGMReference = "ES00002";

		CombineAssertions(() =>
		{
			entryHeader.SetCSVImportCertificate("ABCDEF1234567");
			AssertEquals("CSV Import Certificate has been changed when the pop up was accepted", "ABCDEF1234567", entryHeader.ZG_CSVImportCertificate);
			AssertEquals("New event in logs", "|NEW=ABCDEF1234567|RES=Manually Added CSV Import Certificate Code to Entry ES00002|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);

			entryHeader.SetCSVImportCertificate("ABCDEF1234567");
			AssertEquals("The Last Event is still the same cause the CSV Import Certificate does not change", "|NEW=ABCDEF1234567|RES=Manually Added CSV Import Certificate Code to Entry ES00002|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);

			// 5ms sleep to ensure second change log's event time is after first change log's time, taking into account SQL datetime precision (3ms).
			Thread.Sleep(5);

			entryHeader.SetCSVImportCertificate("1234567890123456");
			AssertEquals("CSV Import Certificate has been changed when the pop up was accepted a second time", "1234567890123456", entryHeader.ZG_CSVImportCertificate);
			AssertEquals("New event in logs for second change", "|NEW=1234567890123456|OLD=ABCDEF1234567|RES=Manually Added CSV Import Certificate Code to Entry ES00002|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);

			// 5ms sleep to ensure second change log's event time is after first change log's time, taking into account SQL datetime precision (3ms).
			Thread.Sleep(5);

			entryHeader.SetCSVImportCertificate(ZString.Empty);
			AssertEquals("CSV Import Certificate has been changed when the pop up was accepted a third time", ZString.Empty, entryHeader.ZG_CSVImportCertificate);
			AssertEquals("New event in logs for third change", "|NEW=Empty|OLD=1234567890123456|RES=Manually Added CSV Import Certificate Code to Entry ES00002|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);
		});
	}

	public void TestSetT2LClearance()
	{
		var entryHeader = SetUpCompleteDeclaration();
		entryHeader.CH_BGMReference = "ES00003";

		CombineAssertions(() =>
		{
			entryHeader.SetT2LClearance("1234567ABCDEF");
			AssertEquals("CSV T2L has been changed when the pop up was accepted", "1234567ABCDEF", entryHeader.ZG_CSVT2L);
			AssertEquals("New event in logs", "|NEW=1234567ABCDEF|RES=Manually Added CSV T2L Code to Entry ES00003|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);

			entryHeader.SetT2LClearance("1234567ABCDEF");
			AssertEquals("The Last Event is still the same cause the CSV T2L does not change", "|NEW=1234567ABCDEF|RES=Manually Added CSV T2L Code to Entry ES00003|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);

			// 5ms sleep to ensure second change log's event time is after first change log's time, taking into account SQL datetime precision (3ms).
			Thread.Sleep(5);

			entryHeader.SetT2LClearance("ABCDEFGHIJKLMNO");
			AssertEquals("CSV T2L has been changed when the pop up was accepted a second time", "ABCDEFGHIJKLMNO", entryHeader.ZG_CSVT2L);
			AssertEquals("New event in logs for second change", "|NEW=ABCDEFGHIJKLMNO|OLD=1234567ABCDEF|RES=Manually Added CSV T2L Code to Entry ES00003|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);

			// 5ms sleep to ensure second change log's event time is after first change log's time, taking into account SQL datetime precision (3ms).
			Thread.Sleep(5);

			entryHeader.SetT2LClearance(ZString.Empty);
			AssertEquals("CSV T2L has been changed when the pop up was accepted a third time", ZString.Empty, entryHeader.ZG_CSVT2L);
			AssertEquals("New event in logs for third change", "|NEW=Empty|OLD=ABCDEFGHIJKLMNO|RES=Manually Added CSV T2L Code to Entry ES00003|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);
		});
	}

	public void TestSetCSVExitCertificate()
	{
		var entryHeader = SetUpCompleteDeclaration();
		entryHeader.CH_BGMReference = "ES00003";

		CombineAssertions(() =>
		{
			entryHeader.SetCSVExitCertificate("1234567ABCDEF");
			AssertEquals("CSV Exit Certificate has been changed when the pop up was accepted", "1234567ABCDEF", entryHeader.ZG_CSVExitCertificate);
			AssertEquals("New event in logs", "|NEW=1234567ABCDEF|RES=Manually Added CSV Exit Certificate Code to Entry ES00003|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);

			entryHeader.SetCSVExitCertificate("1234567ABCDEF");
			AssertEquals("The Last Event is still the same cause the CSV Exit Certificate does not change", "|NEW=1234567ABCDEF|RES=Manually Added CSV Exit Certificate Code to Entry ES00003|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);

			// 5ms sleep to ensure second change log's event time is after first change log's time, taking into account SQL datetime precision (3ms).
			Thread.Sleep(5);

			entryHeader.SetCSVExitCertificate("ABCDEFGHIJKLMNO");
			AssertEquals("CSV Exit Certificate has been changed when the pop up was accepted a second time", "ABCDEFGHIJKLMNO", entryHeader.ZG_CSVExitCertificate);
			AssertEquals("New event in logs for second change", "|NEW=ABCDEFGHIJKLMNO|OLD=1234567ABCDEF|RES=Manually Added CSV Exit Certificate Code to Entry ES00003|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);

			// 5ms sleep to ensure second change log's event time is after first change log's time, taking into account SQL datetime precision (3ms).
			Thread.Sleep(5);

			entryHeader.SetCSVExitCertificate(ZString.Empty);
			AssertEquals("CSV Exit Certificate has been changed when the pop up was accepted a third time", ZString.Empty, entryHeader.ZG_CSVExitCertificate);
			AssertEquals("New event in logs for third change", "|NEW=Empty|OLD=ABCDEFGHIJKLMNO|RES=Manually Added CSV Exit Certificate Code to Entry ES00003|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);
		});
	}

	public void TestHasNoInvoiceUndeclared()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.SupportingDocuments.Add(Factory.CreateSupportingDocument("N740", "NoInvoice"));
		declaration.SupportingDocuments.Add(Factory.CreateSupportingDocument("N380", "Invoice"));
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		var entryLine = entryHeader.AllEntryLines.AddNew();
		entryLine.AddEntryLineDocument<SupportingDocument>("N740", "NoInvoice");

		CombineAssertions(() =>
		{
			AssertEquals("False cause there is an invoice undeclared", false, entryHeader.HasNoInvoiceUndeclared());

			entryLine.AddEntryLineDocument<SupportingDocument>("N380", "Invoice");
			AssertEquals("True cause all invoice are declared", true, entryHeader.HasNoInvoiceUndeclared());
		});
	}

	public void TestIsContainerised()
	{
		var containerTag = "CONT1";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

		var container = declaration.CusContainers.AddNew();
		container.CO_ContainerNumber = containerTag;

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge done", true, mergeResult);

		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.MergedLines.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("IsContainerised false when no entry line has containers", false, entryHeader.IsContainerised());

			invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(containerTag).IsForInvoiceLine = true;
			AssertEquals("IsContainerised true when at least one entry line has containers (invoice lines have containers)", true, entryHeader.IsContainerised());
		});
	}

	public void TestIs9015SupportingDocumentNeeded()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		declaration.JE_DeclarantType = "DIR";

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		var declarant = Factory.New<OrgHeader>();
		declarant.OH_Code = "AA";
		declaration.Declarant.OA_OH = declarant.PK;
		var authorisation = SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);

		GuaranteesTestHelper.CreateGuaranteesHeaderDetail(Factory, declarant.PK, "Guarantee", false);
		GuaranteesTestHelper.CreateGuaranteeForEntryInstruction(declaration, (entryInstruction.PK, "Guarantee"));

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.ZG_MethodOfPayment = "R";

		Factory.Save();

		declaration.DoMerge();
		var entryHeader = declaration.CustomsEntryHeaders[0];

		CombineAssertions(() =>
		{
			AssertEquals("9015 sup doc is not needed when JE_MessageType is not Import", false, entryHeader.Is9015SupportingDocumentNeeded());

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("9015 sup doc is needed when entry instruction is A, JE_DeclarantType is DIR, line's ZG_MethodOfPayment is R, there is an AEOC authorisation and a guarantee with declarant as holder", true, entryHeader.Is9015SupportingDocumentNeeded());

			declaration.JE_DeclarantType = "3";
			AssertEquals("9015 sup doc is not needed when JE_DeclarantType is not DIR 2 or 5", false, entryHeader.Is9015SupportingDocumentNeeded());

			declaration.JE_DeclarantType = "2";
			AssertEquals("9015 sup doc is needed when entry instruction is A, JE_DeclarantType is 2, line's ZG_MethodOfPayment is R, there is an AEOC authorisation and a guarantee with declarant as holder", true, entryHeader.Is9015SupportingDocumentNeeded());

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			AssertEquals("9015 sup doc is not needed when entry instruction is not A, B, C, X, Y or Z", false, entryHeader.Is9015SupportingDocumentNeeded());

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
			AssertEquals("9015 sup doc is needed when entry instruction is B, JE_DeclarantType is 2, line's ZG_MethodOfPayment is R, there is an AEOC authorisation and a guarantee with declarant as holder", true, entryHeader.Is9015SupportingDocumentNeeded());

			invoiceLine.ZG_MethodOfPayment = "A";
			AssertEquals("9015 sup doc is not needed when line's ZG_MethodOfPayment is not R", false, entryHeader.Is9015SupportingDocumentNeeded());

			invoiceLine.ZG_MethodOfPayment = "R";
			AssertEquals("9015 sup doc is needed when entry instruction is B, JE_DeclarantType is 2, line's ZG_MethodOfPayment is R, there is an AEOC authorisation and a guarantee with declarant as holder", true, entryHeader.Is9015SupportingDocumentNeeded());

			declaration.Declarant.OA_OH = ZGuid.Empty;
			AssertEquals("9015 sup doc is not needed when there is no declarant", false, entryHeader.Is9015SupportingDocumentNeeded());

			declaration.Declarant.OA_OH = declarant.PK;
			AssertEquals("9015 sup doc is needed when entry instruction is B, JE_DeclarantType is 2, line's ZG_MethodOfPayment is R, there is an AEOC authorisation and a guarantee with declarant as holder and declarant is declared", true, entryHeader.Is9015SupportingDocumentNeeded());

			authorisation.CPH_Type = ESCusAuthorisationHeaderTypeList.Codes.ApplicationOrDecisionRelatingToBindingOriginInformation;
			Factory.Save();
			AssertEquals("9015 sup doc is not needed when there is no AEOC, AEOF or AEOS authorisation with declarant as holder", false, entryHeader.Is9015SupportingDocumentNeeded());

			authorisation.CPH_Type = ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplificationsSecurityAndSafety;
			Factory.Save();
			AssertEquals("9015 sup doc is needed when entry instruction is B, JE_DeclarantType is 2, line's ZG_MethodOfPayment is R, there is an AEOF authorisation and a guarantee with declarant as holder", true, entryHeader.Is9015SupportingDocumentNeeded());

			declaration.Guarantees[0].PW_BondNumber = "AAA";
			AssertEquals(1, declaration.Guarantees.Count);
			AssertEquals("9015 sup doc is not needed when ther is no guarantee with declarant as holder declared in the declaration", false, entryHeader.Is9015SupportingDocumentNeeded());
		});
	}

	public void TestEntryHas9015SupportingDocuments_Declaration()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var invoice = declaration.Invoices.AddNew();
		invoice.InvoiceLines.AddNew();
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = declaration.CustomsEntryHeaders[0];
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("No 9015 supporting documents declared", false, entryHeader.EntryHas9015SupportingDocuments());

			SupDocTestHelper.AddSuportingDocumentToDeclaration(declaration, SupportingDocumentType.VATReductionCode);
			AssertEquals("A 9015 supporting document was declared", true, entryHeader.EntryHas9015SupportingDocuments());
		});
	}

	public void TestEntryHas9015SupportingDocuments_InvoiceHeader()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var invoice = declaration.Invoices.AddNew();
		invoice.InvoiceLines.AddNew();
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = declaration.CustomsEntryHeaders[0];
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("No 9015 supporting documents declared", false, entryHeader.EntryHas9015SupportingDocuments());

			SupDocTestHelper.AddSuportingDocumentToInvoiceHeader(invoice, SupportingDocumentType.VATReductionCode);
			AssertEquals("A 9015 supporting document was declared", true, entryHeader.EntryHas9015SupportingDocuments());
		});
	}

	public void TestEntryHas9015SupportingDocuments_InvoiceLine()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = declaration.CustomsEntryHeaders[0];
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("No 9015 supporting documents declared", false, entryHeader.EntryHas9015SupportingDocuments());

			SupDocTestHelper.AddSuportingDocumentToInvoiceLine(invoiceLine, SupportingDocumentType.VATReductionCode);
			AssertEquals("A 9015 supporting document was declared", true, entryHeader.EntryHas9015SupportingDocuments());
		});
	}

	public void TestRemove9015Documents_Declaration()
	{
		var docType9015 = SupportingDocumentType.VATReductionCode;
		var otherDocType = "AAA";

		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var invoice = declaration.Invoices.AddNew();
		invoice.InvoiceLines.AddNew();
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = declaration.CustomsEntryHeaders[0];
		SupDocTestHelper.AddSuportingDocumentToDeclaration(declaration, docType9015);
		SupDocTestHelper.AddSuportingDocumentToDeclaration(declaration, otherDocType);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("Declaration has 1 9015 supporting document before calling method", 1, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType9015));
			AssertEquals("Declaration has 1 AAA supporting document before calling method", 1, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, otherDocType));

			entryHeader.Remove9015Documents();
			AssertEquals("Declaration has no 9015 supporting document after calling method", 0, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType9015));
			AssertEquals("Declaration has 1 AAA supporting document after calling method", 1, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, otherDocType));
		});
	}

	public void TestRemove9015Documents_InvoiceHeader()
	{
		var docType9015 = SupportingDocumentType.VATReductionCode;
		var otherDocType = "AAA";

		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var invoice = declaration.Invoices.AddNew();
		invoice.InvoiceLines.AddNew();
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = declaration.CustomsEntryHeaders[0];
		SupDocTestHelper.AddSuportingDocumentToInvoiceHeader(invoice, docType9015);
		SupDocTestHelper.AddSuportingDocumentToInvoiceHeader(invoice, otherDocType);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("Invoice header has 1 9015 supporting document before calling method", 1, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType9015));
			AssertEquals("Invoice header has 1 AAA supporting document before calling method", 1, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, otherDocType));

			entryHeader.Remove9015Documents();
			AssertEquals("Invoice header has no 9015 supporting document after calling method", 0, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType9015));
			AssertEquals("Invoice header has 1 AAA supporting document after calling method", 1, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, otherDocType));
		});
	}

	public void TestRemove9015Documents_InvoiceLine()
	{
		var docType9015 = SupportingDocumentType.VATReductionCode;
		var otherDocType = "AAA";

		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = declaration.CustomsEntryHeaders[0];
		SupDocTestHelper.AddSuportingDocumentToInvoiceLine(invoiceLine, docType9015);
		SupDocTestHelper.AddSuportingDocumentToInvoiceLine(invoiceLine, otherDocType);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("Invoice line has 1 9015 supporting document before calling method", 1, SupDocTestHelper.GetTypeDocumentCount(invoiceLine.SupportingDocuments, docType9015));
			AssertEquals("Invoice line has 1 AAA supporting document before calling method", 1, SupDocTestHelper.GetTypeDocumentCount(invoiceLine.SupportingDocuments, otherDocType));

			entryHeader.Remove9015Documents();
			AssertEquals("Invoice line has no 9015 supporting document after calling method", 0, SupDocTestHelper.GetTypeDocumentCount(invoiceLine.SupportingDocuments, docType9015));
			AssertEquals("Invoice line has 1 AAA supporting document after calling method", 1, SupDocTestHelper.GetTypeDocumentCount(invoiceLine.SupportingDocuments, otherDocType));
		});
	}

	public void TestRemove9015Documents_AllObjects()
	{
		var docType9015 = SupportingDocumentType.VATReductionCode;

		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = declaration.CustomsEntryHeaders[0];
		SupDocTestHelper.AddSuportingDocumentToDeclaration(declaration, docType9015);
		SupDocTestHelper.AddSuportingDocumentToInvoiceHeader(invoice, docType9015);
		SupDocTestHelper.AddSuportingDocumentToInvoiceLine(invoiceLine, docType9015);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("Declaration has 1 9015 supporting document before calling method", 1, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType9015));
			AssertEquals("Invoice header has 1 9015 supporting document before calling method", 1, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType9015));
			AssertEquals("Invoice line has 1 9015 supporting document before calling method", 1, SupDocTestHelper.GetTypeDocumentCount(invoiceLine.SupportingDocuments, docType9015));

			entryHeader.Remove9015Documents();
			AssertEquals("Declaration has no 9015 supporting document after calling method", 0, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType9015));
			AssertEquals("Invoice header has no 9015 supporting document after calling method", 0, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType9015));
			AssertEquals("Invoice line has no 9015 supporting document after calling method", 0, SupDocTestHelper.GetTypeDocumentCount(invoiceLine.SupportingDocuments, docType9015));
		});
	}

	public void TestRemoveSupportingDocuments()
	{
		var docType = SupportingDocumentType.FluorinatedGases;
		var otherDocType = SupportingDocumentType.REARebate;

		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = "IM";
		entryInstruction.CEI_SubStyle = "A";

		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_RX_NKInvoice_Currency = "EUR";

		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_PrimaryPreference = "100";

		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = declaration.CustomsEntryHeaders[0];

		CombineAssertions(() =>
		{
			SetupSupportingDocuments();
			entryHeader.RemoveSupportingDocuments(docType, removeFromDeclaration: true, removeFromInvoices: false, removeFromInvoiceLines: false, removeFromEntryInstruction: false);
			AssertSupportingDocumentsRemoved(removedFromDeclaration: true, removedFromInvoices: false, removedFromInvoiceLines: false, removedFromEntryInstruction: false);

			SetupSupportingDocuments();
			entryHeader.RemoveSupportingDocuments(docType, removeFromDeclaration: false, removeFromInvoices: true, removeFromInvoiceLines: false, removeFromEntryInstruction: false);
			AssertSupportingDocumentsRemoved(removedFromDeclaration: false, removedFromInvoices: true, removedFromInvoiceLines: false, removedFromEntryInstruction: false);

			SetupSupportingDocuments();
			entryHeader.RemoveSupportingDocuments(docType, removeFromDeclaration: false, removeFromInvoices: false, removeFromInvoiceLines: true, removeFromEntryInstruction: false);
			AssertSupportingDocumentsRemoved(removedFromDeclaration: false, removedFromInvoices: false, removedFromInvoiceLines: true, removedFromEntryInstruction: false);

			SetupSupportingDocuments();
			entryHeader.RemoveSupportingDocuments(docType, removeFromDeclaration: false, removeFromInvoices: false, removeFromInvoiceLines: false, removeFromEntryInstruction: true);
			AssertSupportingDocumentsRemoved(removedFromDeclaration: false, removedFromInvoices: false, removedFromInvoiceLines: false, removedFromEntryInstruction: true);

			SetupSupportingDocuments();
			entryHeader.RemoveSupportingDocuments(docType);
			AssertSupportingDocumentsRemoved(removedFromDeclaration: true, removedFromInvoices: true, removedFromInvoiceLines: true, removedFromEntryInstruction: true);
		});

		entryInstruction.CEI_Style = "";
		entryInstruction.CEI_SubStyle = "";
		invoiceLine.JI_CEI = ZGuid.Empty;

		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		entryHeader = declaration.CustomsEntryHeaders[0];
		AssertNull("Pre-requisite: Entry header has no linked Entry Instruction", entryHeader.EntryInstruction);

		CombineAssertions(() =>
		{
			SetupSupportingDocuments();
			entryHeader.RemoveSupportingDocuments(docType);
			AssertSupportingDocumentsRemoved(removedFromDeclaration: true, removedFromInvoices: true, removedFromInvoiceLines: true, removedFromEntryInstruction: false);
		});

		void SetupSupportingDocuments()
		{
			ClearAllSupportingDocuments(declaration.SupportingDocuments);
			SupDocTestHelper.AddSuportingDocumentToDeclaration(declaration, docType);
			SupDocTestHelper.AddSuportingDocumentToDeclaration(declaration, otherDocType);

			ClearAllSupportingDocuments(invoice.SupportingDocuments);
			SupDocTestHelper.AddSuportingDocumentToInvoiceHeader(invoice, docType);
			SupDocTestHelper.AddSuportingDocumentToInvoiceHeader(invoice, otherDocType);

			ClearAllSupportingDocuments(invoiceLine.SupportingDocuments);
			SupDocTestHelper.AddSuportingDocumentToInvoiceLine(invoiceLine, docType);
			SupDocTestHelper.AddSuportingDocumentToInvoiceLine(invoiceLine, otherDocType);

			ClearAllSupportingDocuments(entryInstruction.SupportingDocuments);
			SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction, docType);
			SupDocTestHelper.AddSuportingDocumentToEntryInstruction(entryInstruction, otherDocType);

			Factory.Save();

			AssertSupportingDocumentsCount($"Pre-requisite: Declaration has {docType} document", 1, declaration.SupportingDocuments, docType);
			AssertSupportingDocumentsCount($"Pre-requisite: Declaration has {otherDocType} document", 1, declaration.SupportingDocuments, otherDocType);
			AssertSupportingDocumentsCount($"Pre-requisite: Invoice has {docType} document", 1, invoice.SupportingDocuments, docType);
			AssertSupportingDocumentsCount($"Pre-requisite: Invoice has {otherDocType} document", 1, invoice.SupportingDocuments, otherDocType);
			AssertSupportingDocumentsCount($"Pre-requisite: Invoice line has {docType} document", 1, invoiceLine.SupportingDocuments, docType);
			AssertSupportingDocumentsCount($"Pre-requisite: Invoice line has {otherDocType} document", 1, invoiceLine.SupportingDocuments, otherDocType);
			AssertSupportingDocumentsCount($"Pre-requisite: Entry instruction line has {docType} document", 1, entryInstruction.SupportingDocuments, docType);
			AssertSupportingDocumentsCount($"Pre-requisite: Entry instruction line has {otherDocType} document", 1, entryInstruction.SupportingDocuments, otherDocType);
		}

		void AssertSupportingDocumentsRemoved(bool removedFromDeclaration, bool removedFromInvoices, bool removedFromInvoiceLines, bool removedFromEntryInstruction)
		{
			string GetExpectedStatus(bool removed) => removed ? "removed" : "retained";
			int GetExpectedCount(bool removed) => removed ? 0 : 1;

			AssertSupportingDocumentsCount($"Declaration {docType} {GetExpectedStatus(removedFromDeclaration)}", GetExpectedCount(removedFromDeclaration), declaration.SupportingDocuments, docType);
			AssertSupportingDocumentsCount($"Declaration {otherDocType} retained", 1, declaration.SupportingDocuments, otherDocType);

			AssertSupportingDocumentsCount($"Invoice {docType} {GetExpectedStatus(removedFromInvoices)}", GetExpectedCount(removedFromInvoices), invoice.SupportingDocuments, docType);
			AssertSupportingDocumentsCount($"Invoice {otherDocType} retained", 1, invoice.SupportingDocuments, otherDocType);

			AssertSupportingDocumentsCount($"Invoice line {docType} {GetExpectedStatus(removedFromInvoiceLines)}", GetExpectedCount(removedFromInvoiceLines), invoiceLine.SupportingDocuments, docType);
			AssertSupportingDocumentsCount($"Invoice line {otherDocType} retained", 1, invoiceLine.SupportingDocuments, otherDocType);

			AssertSupportingDocumentsCount($"Entry instruction {docType} {GetExpectedStatus(removedFromEntryInstruction)}", GetExpectedCount(removedFromEntryInstruction), entryInstruction.SupportingDocuments, docType);
			AssertSupportingDocumentsCount($"Entry instruction {otherDocType} retained", 1, entryInstruction.SupportingDocuments, otherDocType);
		}

		void AssertSupportingDocumentsCount(string message, int expectedCount, EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection documents, string docType)
		{
			AssertEquals(message, expectedCount, documents.Find(x => x.CSI_Code == docType).ToList().Count);
		}

		void ClearAllSupportingDocuments(EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection documents)
		{
			var allDocuments = documents.Find(_ => true).ToList();
			allDocuments.ForEach(x => documents.RemoveAndDelete(x));
		}
	}

	public void TestCountriesOfRouting_ES()
	{
		ForwardingShipment shipment = Enterprise.Freight.Forwarding.Business.Testing.ForwardingShipmentTest.CreateForwardingShipmentWithManyLegsForTesting(Factory);
		JobDeclaration dec = (JobDeclaration)GetNewDeclaration();
		var entry = dec.CustomsEntryHeaders.AddNew();

		dec.JE_JS = shipment.PK;

		dec.JE_RL_NKPortOfLoading = "HKHKG";
		dec.JE_RL_NKPortOfArrival = "DEFRA";
		dec.JE_RL_NKOrigin = "AUSYD";
		dec.JE_RL_NKFinalDestination = "GBLON";
		CombineAssertions(() =>
		{
			AssertEquals("The shipment goes through 5 countries - AU, HK, DE, FR, GB", 5, shipment.CountriesOfRouting.Count);

			List<ZString> routes = entry.CountriesOfRouting;

			AssertEquals("But the entryHeader excludes the origin and dest (not load and arrival ports)", 3, routes.Count);
			AssertEquals("HK", routes[0]);
			AssertEquals("DE", routes[1]);
			AssertEquals("FR", routes[2]);
		});
	}

	public void TestCountriesOfRouting_EXS()
	{
		ForwardingShipment shipment = Enterprise.Freight.Forwarding.Business.Testing.ForwardingShipmentTest.CreateForwardingShipmentWithManyLegsForTesting(Factory);
		JobDeclaration dec = (JobDeclaration)GetNewDeclaration();
		dec.CustomsEntryInstructions.RemoveAndDeleteAll();
		var entryInstruction = dec.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
		var entry = dec.CustomsEntryHeaders.AddNew();
		entry.CH_CEI_Instruction = entryInstruction.PK;

		dec.JE_JS = shipment.PK;

		dec.JE_RL_NKPortOfLoading = "HKHKG";
		dec.JE_RL_NKPortOfArrival = "DEFRA";
		dec.JE_RL_NKOrigin = "AUSYD";
		dec.JE_RL_NKFinalDestination = "GBLON";
		CombineAssertions(() =>
		{
			AssertEquals("The shipment goes through 5 countries - AU, HK, DE, FR, GB", 5, shipment.CountriesOfRouting.Count);

			List<ZString> routes = entry.CountriesOfRouting;

			AssertEquals("But the entryHeader includes the origin and dest and can repeat countries when EXS", 7, routes.Count);
			AssertEquals("AU", routes[0]);
			AssertEquals("AU", routes[1]);
			AssertEquals("HK", routes[2]);
			AssertEquals("DE", routes[3]);
			AssertEquals("FR", routes[4]);
			AssertEquals("GB", routes[5]);
			AssertEquals("GB", routes[6]);
		});
	}

	public void TestCountriesOfRouting_AES()
	{
		ForwardingShipment shipment = Enterprise.Freight.Forwarding.Business.Testing.ForwardingShipmentTest.CreateForwardingShipmentWithManyLegsForTesting(Factory);
		JobDeclaration dec = (JobDeclaration)GetNewDeclaration();
		dec.CustomsEntryInstructions.RemoveAndDeleteAll();

		var entryInstruction = dec.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
		var entry = dec.CustomsEntryHeaders.AddNew();
		entry.CH_CEI_Instruction = entryInstruction.PK;
		entry.ZG_UCC6Version = 1;

		dec.JE_JS = shipment.PK;
		dec.JE_RL_NKPortOfLoading = "HKHKG";
		dec.JE_RL_NKPortOfArrival = "DEFRA";
		dec.JE_RL_NKOrigin = "AUSYD";
		dec.JE_RL_NKFinalDestination = "GBLON";
		CombineAssertions(() =>
		{
			AssertEquals("The shipment goes through 5 countries - AU, HK, DE, FR, GB", 5, shipment.CountriesOfRouting.Count);

			List<ZString> routes = entry.CountriesOfRouting;

			AssertEquals("But the entryHeader includes the origin and dest and can repeat countries when Export and Ucc6", 7, routes.Count);
			AssertEquals("AU", routes[0]);
			AssertEquals("AU", routes[1]);
			AssertEquals("HK", routes[2]);
			AssertEquals("DE", routes[3]);
			AssertEquals("FR", routes[4]);
			AssertEquals("GB", routes[5]);
			AssertEquals("GB", routes[6]);
		});
	}

	public void TestGetTotalChargeValueFor_WithDeferredFees_ES()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var esCode = Core.Constants.CountryCodes.Spain;
		helper.CreateNewOrGetExistingDataGrouping(esCode, "Spain");
		Factory.Save();

		var a00 = helper.CreateNewOrGetExistingRateType(esCode, "A00");
		helper.LoadOrCreateNewCusRateCode(Factory, "A00", a00.PK);
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		var entry = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entry.MergedLines.AddNew();
		var entryLine2 = entry.MergedLines.AddNew();

		AddNewFee(entryLine1, "A00", "X", 111m);
		AddNewFee(entryLine1, "A00", "Y", 222m);
		AddNewFee(entryLine1, "A00", "DEF", 222m);
		AddNewFee(entryLine1, "A00", "NBL", 333m);

		AddNewFee(entryLine2, "A00", "X", 1000m);
		AddNewFee(entryLine2, "A00", "Y", 2000m);
		AddNewFee(entryLine2, "A00", "DEF", 2000m);
		AddNewFee(entryLine2, "A00", "NBL", 3000m);

		var a00Charge = new EntryChargeType(null, "A00", "", true, "");

		var customsChargeEntry = (ICustomsChargeEntry)entry;

		CombineAssertions(() =>
		{
			AssertEquals("Expected TotalChargeValue = 111m + 1000m for A00 X", 1111m, customsChargeEntry.GetTotalChargeValueFor(a00Charge, "X"));
			AssertEquals("Expected TotalChargeValue = 222m + 2000m for A00 Y", 2222m, customsChargeEntry.GetTotalChargeValueFor(a00Charge, "Y"));
			AssertEquals("Expected TotalChargeValue = 111m + 1000m + 222m + 2000m for A00 (without DEF and NBL fees)", 3333m, customsChargeEntry.GetTotalChargeValueFor(a00Charge, ""));
		});
	}

	public void TestIndirectExport_GenAddOnColumn()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = declaration.CustomsEntryHeaders[0];

		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("EntryHeader.IndirectExport should be false by default", false, entryHeader.IndirectExport);

			entryHeader.IndirectExport = true;
			AssertEquals("EntryHeader.IndirectExport should be true", true, entryHeader.IndirectExport);
		});
	}

	public void TestIndirectExportPersistance()
	{
		var filterQuery = new ZQuery(GenAddOnColumnSchema.XA_Name, "ES_IndirectExport");

		var entryHeader = Factory.New<CusEntryHeader>();
		entryHeader.IndirectExport = true;

		CombineAssertions(() =>
		{
			AssertEquals("IndirectExport", true, entryHeader.IndirectExport);
			AssertNotNull("IndirectExport is persisted in GenAddOnColumn", Factory.LoadTop1<GenAddOnColumn>(filterQuery));
		});
	}

	public void TestCSVExitCertificateReadOnly()
	{
		var entry = Factory.New<CusEntryHeader>();
		AssertEquals("CSVExitCertificate must be read only", true, entry.ZG_CSVExitCertificateInfo.ReadOnly);
	}

	public void TestVisualizableDocumentsSupportableAttribute()
	{
		var visualizableDocumentsSupportableAttributes = typeof(CusEntryHeader).GetCustomAttributes(typeof(VisualizableDocumentsSupportableAttribute), false);
		AssertEquals("VisualizableDocumentsSupportableAttribute", 1, visualizableDocumentsSupportableAttributes.Length);

		var visualizableDocumentsSupportableAttribute = (VisualizableDocumentsSupportableAttribute)visualizableDocumentsSupportableAttributes.Single();
		AssertEquals("SupporterType", typeof(CusEntryHeaderESVisualizableDocumentSupporter), visualizableDocumentsSupportableAttribute.SupporterType);
	}

	public void TestIsExportUCC6()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entryHeader = GetNewEntryHeaderFromDeclaration(declaration);
		entryHeader.ZG_UCC6Version = 0;

		CombineAssertions(() =>
		{
			AssertEquals("Is false cause declaration is Import", false, entryHeader.IsExportUCC6);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Is false cause declaration is Export but is not UCC6", false, entryHeader.IsExportUCC6);

			entryHeader.ZG_UCC6Version = 1;
			AssertEquals("Is true cause declaration is Export and UCC6", true, entryHeader.IsExportUCC6);
		});
	}

	public void TestIsExportNoUCC6()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entryHeader = GetNewEntryHeaderFromDeclaration(declaration);
		entryHeader.ZG_UCC6Version = 1;

		CombineAssertions(() =>
		{
			AssertEquals("Is false cause declaration is Import", false, entryHeader.IsExportNoUCC6);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Is false cause declaration is Export and UCC6", false, entryHeader.IsExportNoUCC6);

			entryHeader.ZG_UCC6Version = 0;
			AssertEquals("Is true cause declaration is Export and no UCC6", true, entryHeader.IsExportNoUCC6);
		});
	}

	public void TestIsImportUCC6()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var entryHeader = GetNewEntryHeaderFromDeclaration(declaration);
		entryHeader.ZG_UCC6Version = 0;

		CombineAssertions(() =>
		{
			AssertEquals("Is false cause declaration is Export", false, entryHeader.IsImportUCC6);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Is false cause declaration is Import but is not UCC6", false, entryHeader.IsImportUCC6);

			entryHeader.ZG_UCC6Version = 1;
			AssertEquals("Is true cause declaration is Import and UCC6", true, entryHeader.IsImportUCC6);
		});
	}

	public void TestIsUCC6()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var entryHeader = GetNewEntryHeaderFromDeclaration(declaration);
		entryHeader.ZG_UCC6Version = 0;

		CombineAssertions(() =>
		{
			AssertEquals("Is false cause declaration is Export but ZG_UCC6Version = 0", false, entryHeader.IsUCC6);

			entryHeader.ZG_UCC6Version = 1;
			AssertEquals("Is true cause declaration is Export and ZG_UCC6Version > 0", true, entryHeader.IsUCC6);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			entryHeader.ZG_UCC6Version = 0;
			AssertEquals("Is false cause declaration is Import but ZG_UCC6Version = 0", false, entryHeader.IsUCC6);

			entryHeader.ZG_UCC6Version = 1;
			AssertEquals("Is true cause declaration is Import and ZG_UCC6Version > 0", true, entryHeader.IsUCC6);
		});
	}

	public void TestGetInboxRequestMessageTypesForAES()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryHeader = GetNewEntryHeaderFromDeclaration(declaration);

		CombineAssertions(() =>
		{
			AssertEquals("No message type returned when entry status is empty (not CDA, PDA, PCO, CCO or CLR)", 0, entryHeader.GetInboxRequestMessageTypesForAES().Length);

			entryHeader.CH_EntryStatus = EntryStatusCodes.CustomsDeclarationAccepted;
			AssertContainsExactElementsInAnyOrder("Message types returned are LVE and DIE when entry status is CDA", new ZString[] { "LVE", "DIE" }, entryHeader.GetInboxRequestMessageTypesForAES());

			entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
			AssertContainsExactElementsInAnyOrder("Message type returned is INE when entry status is PDA", new ZString[] { "INE" }, entryHeader.GetInboxRequestMessageTypesForAES());

			entryHeader.CH_EntryStatus = EntryStatusCodes.PendingForEuOffice;
			AssertContainsExactElementsInAnyOrder("Message types returned are LVE, DIE and CCE when entry status is PCO and there is no csv clearance", new ZString[] { "LVE", "DIE", "CCE" }, entryHeader.GetInboxRequestMessageTypesForAES());

			entryHeader.SetCSVClearanceNum("AAAAAAAAAAAAAAAA");
			AssertContainsExactElementsInAnyOrder("Message type returned is INE when entry status is PCO and there is csv clearance is not empty", new ZString[] { "INE" }, entryHeader.GetInboxRequestMessageTypesForAES());

			entryHeader.CH_EntryStatus = EntryStatusCodes.ControlsAtEuOffice;
			AssertContainsExactElementsInAnyOrder("Message types returned are LVE, DIE and CCE when entry status is CCO", new ZString[] { "LVE", "DIE", "CCE" }, entryHeader.GetInboxRequestMessageTypesForAES());

			entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;
			AssertContainsExactElementsInAnyOrder("Message types returned are RES and INE when entry status is CLR", new ZString[] { "RES", "INE" }, entryHeader.GetInboxRequestMessageTypesForAES());
		});
	}

	public void TestSendIQUForGuaranteeWriteOffIfPossible_Validations_CanBeSent()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		declaration.JE_DeclarationReference = "B00169757";

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge done", true, mergeResult);
		Factory.Save();

		var entryHeader = declaration.CustomsEntryHeaders[0];

		var expectedIQUMessageInfo = new IQUMessageInfo()
		{
			MessageCanBeSent = false,
			MessageSentCorrectly = false,
			DeclarationWithBrokerError = ZString.Empty
		};

		CombineAssertions(() =>
		{
			AssertIQUMessageInfo("Message cannot be sent because it is export", expectedIQUMessageInfo, entryHeader.SendIQUForGuaranteeWriteOffIfPossible());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			AssertIQUMessageInfo("Message cannot be sent because it is import but T2L", expectedIQUMessageInfo, entryHeader.SendIQUForGuaranteeWriteOffIfPossible());

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
			AssertIQUMessageInfo("Message cannot be sent because it is import but H2", expectedIQUMessageInfo, entryHeader.SendIQUForGuaranteeWriteOffIfPossible());

			entryInstruction.CEI_Style = ZString.Empty;
			AssertIQUMessageInfo("Message cannot be sent because it is import but without mrn", expectedIQUMessageInfo, entryHeader.SendIQUForGuaranteeWriteOffIfPossible());

			entryHeader.MovementReferenceNumberSetter("MRNCode1", ZDateTime.Today);
			AssertIQUMessageInfo("Message cannot be sent because it is import with mrn but without csv clearance", expectedIQUMessageInfo, entryHeader.SendIQUForGuaranteeWriteOffIfPossible());

			entryHeader.SetCSVClearanceNum("ABCDEFGHIJKLMNOP");
			AssertIQUMessageInfo("Message cannot be sent because it is import with mrn and csv clearance but without guarantees associated", expectedIQUMessageInfo, entryHeader.SendIQUForGuaranteeWriteOffIfPossible());

			GuaranteesTestHelper.CreateGuaranteeForEntryInstruction(declaration, (entryInstruction.PK, "16ESAGL9990000096"));
			AssertIQUMessageInfo("Message cannot be sent because it is import with mrn and csv clearance but without a correct guarantee associated (does not exist in CusGuaranteeHeader)", expectedIQUMessageInfo, entryHeader.SendIQUForGuaranteeWriteOffIfPossible());

			CreateGuaranteeHeaderDetail(Factory, "16ESAGL9990000096", addOBLTransaction: false);
			AssertIQUMessageInfo("Message cannot be sent because it is import with mrn and csv clearance but without a correct guarantee associated (CusGuaranteeHeader does not have OBL transaction)", expectedIQUMessageInfo, entryHeader.SendIQUForGuaranteeWriteOffIfPossible());

			CreateGuaranteeHeaderDetail(Factory, "16ESAGL9990000097", withOldEndDate: true);
			declaration.Guarantees[0].PW_BondNumber = "16ESAGL9990000097";
			AssertEquals(1, declaration.Guarantees.Count);
			AssertIQUMessageInfo("Message cannot be sent because it is import with mrn and csv clearance but without a correct guarantee associated (CusGuaranteeHeader has expired)", expectedIQUMessageInfo, entryHeader.SendIQUForGuaranteeWriteOffIfPossible());

			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingResponse;
			AssertIQUMessageInfo("Message cannot be sent because it is import with mrn and csv clearance and with a correct guarantee associated but has AWR status", expectedIQUMessageInfo, entryHeader.SendIQUForGuaranteeWriteOffIfPossible());
		});
	}

	public void TestSendIQUForGuaranteeWriteOffIfPossible_Validations_BrokerError()
	{
		var staff = Factory.New<GlbStaff>();
		staff.GS_Code = "AH";
		staff.GS_LoginName = "ahtest";
		var wrapper = GlbStaffWrapper.Get(staff);
		var cert = wrapper.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert1";
		cert.GP_MailBoxID = "Test";
		cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		declaration.JE_DeclarationReference = "B00169757";

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		CreateGuaranteeHeaderDetail(Factory, "16ESAGL9990000097");
		GuaranteesTestHelper.CreateGuaranteeForEntryInstruction(declaration, (entryInstruction.PK, "16ESAGL9990000097"));

		var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge done", true, mergeResult);
		Factory.Save();

		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.MovementReferenceNumberSetter("MRNCode1", ZDateTime.Today);
		entryHeader.SetCSVClearanceNum("ABCDEFGHIJKLMNOP");

		var expectedIQUMessageInfo = new IQUMessageInfo()
		{
			MessageCanBeSent = true,
			MessageSentCorrectly = false,
			DeclarationWithBrokerError = "B00169757"
		};

		CombineAssertions(() =>
		{
			AssertIQUMessageInfo("Message cannot be sent because there is no broker declared", expectedIQUMessageInfo, entryHeader.SendIQUForGuaranteeWriteOffIfPossible());

			declaration.JE_GS_NKCusAgent = staff.GS_Code;
			declaration.JE_CustomsProfile = ZString.Empty;
			AssertIQUMessageInfo("Message cannot be sent because there is no certificate declared", expectedIQUMessageInfo, entryHeader.SendIQUForGuaranteeWriteOffIfPossible());

			declaration.JE_CustomsProfile = "INVALID";
			AssertIQUMessageInfo("Message cannot be sent because the certificate declared is not valid", expectedIQUMessageInfo, entryHeader.SendIQUForGuaranteeWriteOffIfPossible());

			declaration.JE_CustomsProfile = cert.GP_Name;
			AssertIQUMessageInfo("Message cannot be sent because current user has no authorisation for certificate declared", expectedIQUMessageInfo, entryHeader.SendIQUForGuaranteeWriteOffIfPossible());
		});
	}

	public void TestSendIQUForGuaranteeWriteOffIfPossible_AEAT()
	{
		var staff = Factory.New<GlbStaff>();
		staff.GS_Code = "AH";
		staff.GS_LoginName = "ahtest";
		var wrapper = GlbStaffWrapper.Get(staff);
		var cert = wrapper.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert1";
		cert.GP_MailBoxID = "Test";
		cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_DeclarationReference = "B00169757";
			declaration.JE_GS_NKCusAgent = staff.GS_Code;
			declaration.JE_CustomsProfile = cert.GP_Name;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			CreateGuaranteeHeaderDetail(Factory, "16ESAGL9990000097");
			GuaranteesTestHelper.CreateGuaranteeForEntryInstruction(declaration, (entryInstruction.PK, "16ESAGL9990000097"));

			var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);
			Factory.Save();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.MovementReferenceNumberSetter("MRNCode1", ZDateTime.Today);
			entryHeader.SetCSVClearanceNum("ABCDEFGHIJKLMNOP");

			AssertSendIQUForGuaranteeWriteOffIfPossible(entryHeader, false);
		}
	}

	public void TestSendIQUForGuaranteeWriteOffIfPossible_ATC()
	{
		var helper = new ESUniversalReferenceTestDataHelper(Factory);
		helper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, BuilderHelperTest.CanaryIslandCode, "Test 61");

		var staff = Factory.New<GlbStaff>();
		staff.GS_Code = "AH";
		staff.GS_LoginName = "ahtest";
		var wrapper = GlbStaffWrapper.Get(staff);
		var cert = wrapper.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert1";
		cert.GP_MailBoxID = "Test";
		cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_DeclarationReference = "B00169757";
			declaration.JE_GS_NKCusAgent = staff.GS_Code;
			declaration.JE_CustomsProfile = cert.GP_Name;
			declaration.ZG_DestinationState = BuilderHelperTest.CanaryIslandCode;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			CreateGuaranteeHeaderDetail(Factory, "16ESAGL9990000097");
			GuaranteesTestHelper.CreateGuaranteeForEntryInstruction(declaration, (entryInstruction.PK, "16ESAGL9990000097"));

			var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);
			Factory.Save();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.MovementReferenceNumberSetter("MRNCode1", ZDateTime.Today);
			entryHeader.SetCSVClearanceNum("ABCDEFGHIJKLMNOP");

			AssertSendIQUForGuaranteeWriteOffIfPossible(entryHeader, true);
		}
	}

	public void TestGetSupportingDocumentsToProcess()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = instruction.PK;
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;
		invoiceLine.JI_CL = entryLine.PK;

		var declarationDoc1 = declaration.SupportingDocuments.AddNew();
		declarationDoc1.CSI_Code = "111";
		declarationDoc1.CSI_ReferenceNumber = "111";

		var declarationDoc2 = declaration.SupportingDocuments.AddNew();
		declarationDoc2.CSI_Code = "111";
		declarationDoc2.CSI_ReferenceNumber = "111";

		var invHeaderDoc1 = invoiceHeader.SupportingDocuments.AddNew();
		invHeaderDoc1.CSI_Code = "222";
		invHeaderDoc1.CSI_ReferenceNumber = "222";

		var instructionDoc1 = instruction.SupportingDocuments.AddNew();
		instructionDoc1.CSI_Code = "333";
		instructionDoc1.CSI_ReferenceNumber = "333";

		var instructionDoc2 = instruction.SupportingDocuments.AddNew();
		instructionDoc2.CSI_Code = "444";
		instructionDoc2.CSI_ReferenceNumber = "444";

		var invLineDoc = invoiceLine.SupportingDocuments.AddNew();
		invLineDoc.CSI_Code = "555";
		invLineDoc.CSI_ReferenceNumber = "555";

		var supportingDocuments = entryHeader.SupportingDocuments.OrderBy(x => x.CSI_Code).ToArray();

		CombineAssertions(() =>
		{
			AssertEquals("Entry Header Doc Count", 3, supportingDocuments.Length);
			AssertEquals("Doc in declaration (without repetitions) CSI_Code = 111", "111", supportingDocuments[0].CSI_Code);
			AssertEquals("First Doc in entryInstruction CSI_Code = 333", "333", supportingDocuments[1].CSI_Code);
			AssertEquals("Second Doc in entryInstruction CSI_Code = 444", "444", supportingDocuments[2].CSI_Code);
		});
	}

	void AssertSendIQUForGuaranteeWriteOffIfPossible(CusEntryHeader entryHeader, bool isATC)
	{
		var expectedIQUMessageInfo = new IQUMessageInfo()
		{
			MessageCanBeSent = true,
			MessageSentCorrectly = true,
			DeclarationWithBrokerError = ZString.Empty
		};

		CombineAssertions(() =>
		{
			AssertIQUMessageInfo("Message was sent correctly", expectedIQUMessageInfo, entryHeader.SendIQUForGuaranteeWriteOffIfPossible());
			AssertEquals("Entry Message Status has changed to awaiting for first entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);

			var messages = entryHeader.Messages;
			AssertEquals("1 EDIMessage was created for the entry", 1, messages.Count);
			var message = messages[0];
			AssertEquals("Sent message has correct EM_ReceiveTransmit", "TRX", message.EM_ReceiveTransmit);
			AssertEquals("Sent message has correct EM_MessageType", "IQU", message.EM_MessageType);
			AssertEquals("Sent message has correct EM_MessageSubType", "GUA", message.EM_MessageSubType);

			if (isATC)
			{
				AssertContains("Sent IQU message is for canary islands", "DatosEnATC", message.EM_MessageText);
			}
			else
			{
				AssertNotContains("Sent IQU message is not for canary islands", "DatosEnATC", message.EM_MessageText);
			}
		});
	}

	void AssertIQUMessageInfo(ZString assertText, IQUMessageInfo expected, IQUMessageInfo returned)
	{
		AssertEquals(assertText + " MessageCanBeSent", expected.MessageCanBeSent, returned.MessageCanBeSent);
		AssertEquals(assertText + " MessageSentCorrectly", expected.MessageSentCorrectly, returned.MessageSentCorrectly);
		AssertEquals(assertText + " DeclarationWithBrokerError", expected.DeclarationWithBrokerError, returned.DeclarationWithBrokerError);
	}

	public void TestMovementReferenceIssueDate()
	{
		var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();

		CombineAssertions("Acceptance Date", () =>
		{
			AssertEquals("MovementReferenceIssueDate should be empty", ZDateTime.Empty, entryHeader.MovementReferenceNumberIssueDate);

			var testDateTime = new ZDateTime(1995, 2, 16);
			entryHeader.MovementReferenceNumberSetter(ZString.Empty, testDateTime);

			AssertEquals("MovementReferenceIssueDate should be filled when value is assing by MovementReferenceNumberSetter", testDateTime, entryHeader.MovementReferenceNumberIssueDate);

			entryHeader.MovementReferenceNumberIssueDate = ZDateTime.BrettsBirthday;

			AssertEquals("MovementReferenceIssueDate should be filled", ZDateTime.BrettsBirthday, entryHeader.MovementReferenceNumberIssueDate);
		});
	}

	public void TestMovementReferenceEntryStatus()
	{
		var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();

		CombineAssertions("Circuit", () =>
		{
			AssertEquals("MovementReferenceEntryStatus should be empty", ZString.Empty, entryHeader.MovementReferenceNumberEntryStatus);

			entryHeader.MovementReferenceNumberSetter(ZString.Empty, entryStatus: "456");

			AssertEquals("MovementReferenceEntryStatus should be filled when value is assing by MovementReferenceNumberSetter", "456", entryHeader.MovementReferenceNumberEntryStatus);

			entryHeader.SetMovementReferenceNumberEntryStatus("123");

			AssertEquals("MovementReferenceEntryStatus should be filled", "123", entryHeader.MovementReferenceNumberEntryStatus);
		});
	}

	public void TestCSVClearance()
	{
		var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();

		CombineAssertions("Clearance Number", () =>
		{
			AssertEquals("CSVClearance should be empty", ZString.Empty, entryHeader.CSVClearance);

			entryHeader.SetCSVClearanceNum("1234");

			AssertEquals("CSVClearance should be filled", "1234", entryHeader.CSVClearance);
		});
	}

	public void TestCircuitCan()
	{
		var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();

		CombineAssertions("Circuit Can", () =>
		{
			AssertEquals("Circuit Can should be empty", ZString.Empty, entryHeader.CircuitCan);

			entryHeader.MovementReferenceNumber = "MRN";
			entryHeader.SetCircuitCan("123");

			AssertEquals("Circuit Can should be filled", "123", entryHeader.CircuitCan);

			var cusEntryNum = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Spain.CircuitCan, entryHeader.CountryCode);
			AssertEquals("Circuit Can should be filled", "MRN", cusEntryNum.CE_EntryNum);
		});
	}

	public void TestSupplierEoriOfMainOffice()
	{
		var declaration = Factory.New<JobDeclaration>();

		var supplier = Factory.New<OrgHeader>();
		declaration.JE_OH_Supplier = supplier.PK;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("Expected empty", ZString.Empty, entryHeader.SupplierEoriOfMainOffice);

			supplier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
			AssertEquals("Expected PAS with country code when no NIF or EORI declared", "GB333333333", entryHeader.SupplierEoriOfMainOffice);

			supplier.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
			supplier.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			AssertEquals("Expected NIF", "NIF22222222", entryHeader.SupplierEoriOfMainOffice);

			supplier.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
			supplier.OH_Category = OrgConstants.Category.Government;
			AssertEquals("Expected Country+NIF for non NAT Organizations", "ESNIF22222222", entryHeader.SupplierEoriOfMainOffice);

			OrgCusCode eoriCusCode = supplier.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
			AssertEquals("Expected EORI with country code", "FR22222222", entryHeader.SupplierEoriOfMainOffice);

			eoriCusCode.OK_CustomsRegNo = "ES22222222";
			eoriCusCode.OK_RN_NKCodeCountry = "ES";
			AssertEquals("Expected EORI with country code not repeated", "ES22222222", entryHeader.SupplierEoriOfMainOffice);

			supplier.CustomsCodes.RemoveAndDeleteAll();
			var customCode = supplier.CustomsCodes.AddNew();

			customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			customCode.OK_CustomsRegNo = "XI123456789A";
			customCode.OK_RN_NKCodeCountry = "GB";
			customCode.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 09);

			AssertEquals("Expected GB EORI when customsCode codeType is EOR, Country is GB and code starts with XI, when there are multiple EORIs", "XI123456789A", entryHeader.SupplierEoriOfMainOffice);

			var customsCode2 = supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "A12345678", "ES");
			customsCode2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 10);
			AssertEquals("Expected ES EORI when there are multiple (one EU and one GB), even when the ES one was added after, when there are multiple EORIs", "ESA12345678", entryHeader.SupplierEoriOfMainOffice);

			customsCode2.OK_RN_NKCodeCountry = "AU";
			customsCode2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 08);
			AssertEquals("Expected GB EORI when there are multiple (one non EU and one GB) and the GB one starts with XI, even when the GB one was added after, when there are multiple EORIs", "XI123456789A", entryHeader.SupplierEoriOfMainOffice);

			customCode.OK_CustomsRegNo = "123456789A";
			AssertEquals("Expected the first EORI found when there are no EU EORIs and the only GB one's code doesn't start with XI, when there are multiple EORIs", "AUA12345678", entryHeader.SupplierEoriOfMainOffice);
		});
	}

	public void TestImporterEoriOfMainOffice()
	{
		var declaration = Factory.New<JobDeclaration>();

		var importer = Factory.New<OrgHeader>();
		declaration.JE_OH_Importer = importer.PK;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("Expected empty", ZString.Empty, entryHeader.ImporterEoriOfMainOffice);

			importer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
			AssertEquals("Expected PAS with country code when no NIF or EORI declared", "GB333333333", entryHeader.ImporterEoriOfMainOffice);

			importer.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
			importer.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			AssertEquals("Expected NIF", "NIF22222222", entryHeader.ImporterEoriOfMainOffice);

			importer.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
			importer.OH_Category = OrgConstants.Category.Government;
			AssertEquals("Expected Country+NIF for non NAT Organizations", "ESNIF22222222", entryHeader.ImporterEoriOfMainOffice);

			OrgCusCode eoriCusCode = importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
			AssertEquals("Expected EORI with country code", "FR22222222", entryHeader.ImporterEoriOfMainOffice);

			eoriCusCode.OK_CustomsRegNo = "ES22222222";
			eoriCusCode.OK_RN_NKCodeCountry = "ES";
			AssertEquals("Expected EORI with country code not repeated", "ES22222222", entryHeader.ImporterEoriOfMainOffice);

			importer.CustomsCodes.RemoveAndDeleteAll();
			var customCode = importer.CustomsCodes.AddNew();

			customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			customCode.OK_CustomsRegNo = "XI123456789A";
			customCode.OK_RN_NKCodeCountry = "GB";
			customCode.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 09);

			AssertEquals("Expected GB EORI when customsCode codeType is EOR, Country is GB and code starts with XI, when there are multiple EORIs", "XI123456789A", entryHeader.ImporterEoriOfMainOffice);

			var customsCode2 = importer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "A12345678", "ES");
			customsCode2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 10);
			AssertEquals("Expected ES EORI when there are multiple (one EU and one GB), even when the ES one was added after, when there are multiple EORIs", "ESA12345678", entryHeader.ImporterEoriOfMainOffice);

			customsCode2.OK_RN_NKCodeCountry = "AU";
			customsCode2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 08);
			AssertEquals("Expected GB EORI when there are multiple (one non EU and one GB) and the GB one starts with XI, even when the GB one was added after, when there are multiple EORIs", "XI123456789A", entryHeader.ImporterEoriOfMainOffice);

			customCode.OK_CustomsRegNo = "123456789A";
			AssertEquals("Expected the first EORI found when there are no EU EORIs and the only GB one's code doesn't start with XI, when there are multiple EORIs", "AUA12345678", entryHeader.ImporterEoriOfMainOffice);
		});
	}

	public override void TestRepresentativeOrDeclarantEoriOfMainOffice()
	{
		var declaration = Factory.New<JobDeclaration>();

		var declarant = Factory.New<OrgHeader>();
		declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("Expected empty", ZString.Empty, entryHeader.RepresentativeOrDeclarantEoriOfMainOffice);

			declarant.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
			AssertEquals("Expected PAS with country code when no NIF or EORI declared", "GB333333333", entryHeader.RepresentativeOrDeclarantEoriOfMainOffice);

			declarant.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
			declarant.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			AssertEquals("Expected NIF", "NIF22222222", entryHeader.RepresentativeOrDeclarantEoriOfMainOffice);

			declarant.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
			declarant.OH_Category = OrgConstants.Category.Government;
			AssertEquals("Expected Country+NIF for non NAT Organizations", "ESNIF22222222", entryHeader.RepresentativeOrDeclarantEoriOfMainOffice);

			OrgCusCode eoriCusCode = declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
			AssertEquals("Expected EORI with country code", "FR22222222", entryHeader.RepresentativeOrDeclarantEoriOfMainOffice);

			eoriCusCode.OK_CustomsRegNo = "ES22222222";
			eoriCusCode.OK_RN_NKCodeCountry = "ES";
			AssertEquals("Expected EORI with country code not repeated", "ES22222222", entryHeader.RepresentativeOrDeclarantEoriOfMainOffice);

			declarant.CustomsCodes.RemoveAndDeleteAll();
			var customCode = declarant.CustomsCodes.AddNew();

			customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			customCode.OK_CustomsRegNo = "XI123456789A";
			customCode.OK_RN_NKCodeCountry = "GB";
			customCode.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 09);

			AssertEquals("Expected GB EORI when customsCode codeType is EOR, Country is GB and code starts with XI, when there are multiple EORIs", "XI123456789A", entryHeader.RepresentativeOrDeclarantEoriOfMainOffice);

			var customsCode2 = declarant.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "A12345678", "ES");
			customsCode2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 10);
			AssertEquals("Expected ES EORI when there are multiple (one EU and one GB), even when the ES one was added after, when there are multiple EORIs", "ESA12345678", entryHeader.RepresentativeOrDeclarantEoriOfMainOffice);

			customsCode2.OK_RN_NKCodeCountry = "AU";
			customsCode2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 08);
			AssertEquals("Expected GB EORI when there are multiple (one non EU and one GB) and the GB one starts with XI, even when the GB one was added after, when there are multiple EORIs", "XI123456789A", entryHeader.RepresentativeOrDeclarantEoriOfMainOffice);

			customCode.OK_CustomsRegNo = "123456789A";
			AssertEquals("Expected the first EORI found when there are no EU EORIs and the only GB one's code doesn't start with XI, when there are multiple EORIs", "AUA12345678", entryHeader.RepresentativeOrDeclarantEoriOfMainOffice);
		});
	}

	#region ConfirmTemporaryStorageGoodsConsumption

	#region ConfirmTemporaryStorageGoodsConsumption Import

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_Import_ShouldntDoAction_TemporaryStorageRegisterNotEnabled()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldntDoAction_TemporaryStorageRegisterNotEnabled(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_Import_ShouldntDoAction_EmptyGoodsLocation()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldntDoAction_EmptyGoodsLocation(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_Import_ShouldntDoAction_LocationNotManagedInPremises()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldntDoAction_LocationNotManagedInPremises(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_Import_ShouldntDoAction_CustomsStatusCDA()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldntDoActionWithCustomsStatus(EntryStatusCodes.CustomsDeclarationAccepted, JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_Import_ShouldntDoAction_CustomsStatusCDP()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldntDoActionWithCustomsStatus(EntryStatusCodes.ClearedWithPendingDocuments, JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_Import_ShouldDoActionWithCustomsStatusPDA_OnlyCancel()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldDoActionWithCustomsStatus_OnlyCancel(EntryStatusCodes.PreDeclarationAccepted, JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_Import_ShouldDoActionWithCustomsStatusCLP_Confirm()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldDoActionWithCustomsStatus(EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, ExpectedCommentPrefixImport);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_Import_ShouldDoActionWithCustomsStatusCLR_Confirm()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldDoActionWithCustomsStatus(EntryStatusCodes.Cleared, JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, ExpectedCommentPrefixImport);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_Import_WriteOff()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_WriteOff(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, "DUA");
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_Import_NoPendingAmount()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_NoPendingAmount(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_Import_PendingAmountPositive()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_PendingAmountPositive(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
	}

	#endregion

	#region ConfirmTemporaryStorageGoodsConsumption Import H2

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ImportH2_ShouldntDoAction_TemporaryStorageRegisterNotEnabled()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldntDoAction_TemporaryStorageRegisterNotEnabled(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle: IMPDeclarationTypeList.Codes.H2, docCode: "337");
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ImportH2_ShouldntDoAction_EmptyGoodsLocation()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldntDoAction_EmptyGoodsLocation(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle: IMPDeclarationTypeList.Codes.H2, docCode: "337");
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ImportH2_ShouldntDoAction_LocationNotManagedInPremises()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldntDoAction_LocationNotManagedInPremises(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle: IMPDeclarationTypeList.Codes.H2, docCode: "337");
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ImportH2_ShouldntDoAction_CustomsStatusCDA()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldntDoActionWithCustomsStatus(EntryStatusCodes.CustomsDeclarationAccepted, JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle: IMPDeclarationTypeList.Codes.H2, docCode: "337");
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ImportH2_ShouldDoActionWithCustomsStatusPDA_OnlyCancel()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldDoActionWithCustomsStatus_OnlyCancel(EntryStatusCodes.PreDeclarationAccepted, JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle: IMPDeclarationTypeList.Codes.H2, docCode: "337");
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ImportH2_ShouldDoActionWithCustomsStatusCLP_Confirm()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldDoActionWithCustomsStatus(EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, ExpectedCommentPrefixImport, entryInstructionStyle: IMPDeclarationTypeList.Codes.H2, docCode: "337");
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ImportH2_ShouldDoActionWithCustomsStatusCLR_Confirm()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldDoActionWithCustomsStatus(EntryStatusCodes.Cleared, JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, ExpectedCommentPrefixImport, entryInstructionStyle: IMPDeclarationTypeList.Codes.H2, docCode: "337");
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ImportH2_WriteOff()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_WriteOff(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, "DVD", entryInstructionStyle: IMPDeclarationTypeList.Codes.H2, docCode: "337");
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ImportH2_NoPendingAmount()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_NoPendingAmount(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle: IMPDeclarationTypeList.Codes.H2, docCode: "337");
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ImportH2_PendingAmountPositive()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_PendingAmountPositive(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle: IMPDeclarationTypeList.Codes.H2, docCode: "337");
	}

	#endregion

	#region ConfirmTemporaryStorageGoodsConsumption EXS

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_EXS_ShouldntDoAction_TemporaryStorageRegisterNotEnabled()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldntDoAction_TemporaryStorageRegisterNotEnabled(JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_EXS_ShouldntDoAction_EmptyGoodsLocation()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldntDoAction_EmptyGoodsLocation(JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_EXS_ShouldntDoAction_LocationNotManagedInPremises()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldntDoAction_LocationNotManagedInPremises(JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_EXS_ShouldntDoAction_CustomsStatusCDA()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldntDoActionWithCustomsStatus(EntryStatusCodes.CustomsDeclarationAccepted, JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_EXS_ShouldntDoAction_CustomsStatusCDP()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldntDoActionWithCustomsStatus(EntryStatusCodes.ClearedWithPendingDocuments, JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_EXS_ShouldntDoAction_CustomsStatusPDA()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldntDoActionWithCustomsStatus(EntryStatusCodes.PreDeclarationAccepted, JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_EXS_ShouldntDoAction_CustomsStatusCLP()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldntDoActionWithCustomsStatus(EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_EXS_ShouldDoActionWithCustomsStatusCLR_Confirm()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldDoActionWithCustomsStatus(EntryStatusCodes.Cleared, JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, ExpectedCommentPrefixExport);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_EXS_WriteOff()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_WriteOff(JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, "EXS");
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_EXS_NoPendingAmount()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_NoPendingAmount(JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_EXS_PendingAmountPositive()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_PendingAmountPositive(JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
	}

	#endregion

	#region ConfirmTemporaryStorageGoodsConsumption Export

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_Export_ShouldntDoAction_TemporaryStorageRegisterNotEnabled()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldntDoAction_TemporaryStorageRegisterNotEnabled(JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, docCode: "1217");
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_Export_ShouldntDoAction_EmptyGoodsLocation()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldntDoAction_EmptyGoodsLocation(JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, docCode: "1217");
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_Export_ShouldntDoAction_LocationNotManagedInPremises()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldntDoAction_LocationNotManagedInPremises(JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, docCode: "1217");
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_Export_ShouldntDoAction_CustomsStatusCDA()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldntDoActionWithCustomsStatus(EntryStatusCodes.CustomsDeclarationAccepted, JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, docCode: "1217");
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_Export_ShouldntDoAction_CustomsStatusPCO()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldntDoActionWithCustomsStatus(EntryStatusCodes.PendingForEuOffice, JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, docCode: "1217");
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_Export_ShouldntDoAction_CustomsStatusCCO()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldntDoActionWithCustomsStatus(EntryStatusCodes.ControlsAtEuOffice, JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, docCode: "1217");
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_Export_ShouldDoActionWithCustomsStatusPDA_OnlyCancel()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldDoActionWithCustomsStatus_OnlyCancel(EntryStatusCodes.PreDeclarationAccepted, JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, docCode: "1217");
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_Export_ShouldDoActionWithCustomsStatusSTD_OnlyCancel()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldDoActionWithCustomsStatus_OnlyCancel(EntryStatusCodes.GoodsStoppedAtDeparture, JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, docCode: "1217");
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_Export_ShouldDoActionWithCustomsStatusINV_OnlyCancel()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldDoActionWithCustomsStatus_OnlyCancel(EntryStatusCodes.Invalidated, JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, docCode: "1217");
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_Export_ShouldDoActionWithCustomsStatusCAN_OnlyCancel()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldDoActionWithCustomsStatus_OnlyCancel(EntryStatusCodes.Cancelled, JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, docCode: "1217");
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_Export_ShouldDoActionWithCustomsStatusCLP_Confirm()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldDoActionWithCustomsStatus(EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, ExpectedCommentPrefixExport, docCode: "1217");
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_Export_ShouldDoActionWithCustomsStatusCLR_Confirm()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldDoActionWithCustomsStatus(EntryStatusCodes.Cleared, JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, ExpectedCommentPrefixExport, docCode: "1217");
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_Export_ShouldDoActionWithCustomsStatusEFD_Confirm()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldDoActionWithCustomsStatus(EntryStatusCodes.EffectiveDeparture, JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, ExpectedCommentPrefixExport, docCode: "1217");
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_Export_NoWriteOff()
	{
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var orgHeader = SetUpOrgHeader();
			var entryHeader = SetUpHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(orgHeader.MainAddress, JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.A, docCode: "1217", isLAME: true);
			var regHeader = SetUpTmpRegHeader();
			var guarantee = SetUpGauranteeForTempStorage(regHeader, -3.0m, orgHeader);
			var regLine = SetUpRegLine(regHeader);

			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);

			var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, packageQty: 2, grossWeight: -2);
			var regLineTransaction2 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, packageQty: 1, grossWeight: -1);

			CombineAssertions(() =>
			{
				AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("[PreReq] Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);
				AssertEquals("[PreReq] Bond Amount transaction2 is default", 0.0m, regLineTransaction2.SRT_BondAmount);

				entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;

				AssertEquals("0 Write off transactions in Guarantee are created", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("Bond Amount transaction1 is not calculated", 0m, regLineTransaction1.SRT_BondAmount);
				AssertEquals("Bond Amount transaction2 is not calculated", 0m, regLineTransaction2.SRT_BondAmount);
			});
		}
	}

	#endregion

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldntDoActionWithCustomsStatusCLR_ImportT2L()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.T2L, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: CH_EntryStatus is empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;

				AssertEquals("CH_EntryStatus is changed to CLR", "CLR", entryHeader.CH_EntryStatus);
				AssertTransactionAfterConfirmAction("regLineTransaction", regLineTransaction, transactionStatus: CusTempStorageRegLineTransactionStatusList.Codes.Pending, referenceType: ZString.Empty, mrnCode: ZString.Empty, comment: ZString.Empty, expectedIssueDate: ZDateTimeOffset.Empty, expectedReleaseDate: ZDateTimeOffset.Empty);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldntDoActionWithCustomsStatusCLR_ImportT2C()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.T2C, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: CH_EntryStatus is empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;

				AssertEquals("CH_EntryStatus is changed to CLR", "CLR", entryHeader.CH_EntryStatus);
				AssertTransactionAfterConfirmAction("regLineTransaction", regLineTransaction, transactionStatus: CusTempStorageRegLineTransactionStatusList.Codes.Pending, referenceType: ZString.Empty, mrnCode: ZString.Empty, comment: ZString.Empty, expectedIssueDate: ZDateTimeOffset.Empty, expectedReleaseDate: ZDateTimeOffset.Empty);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldntDoActionWithCustomsStatusCLR_ExportT2L()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.T2L, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: CH_EntryStatus is empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;

				AssertEquals("CH_EntryStatus is changed to CLR", "CLR", entryHeader.CH_EntryStatus);
				AssertTransactionAfterConfirmAction("regLineTransaction", regLineTransaction, transactionStatus: CusTempStorageRegLineTransactionStatusList.Codes.Pending, referenceType: ZString.Empty, mrnCode: ZString.Empty, comment: ZString.Empty, expectedIssueDate: ZDateTimeOffset.Empty, expectedReleaseDate: ZDateTimeOffset.Empty);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldntDoActionWithCustomsStatusCLR_ExportT2C()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.T2C, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: CH_EntryStatus is empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;

				AssertEquals("CH_EntryStatus is changed to CLR", "CLR", entryHeader.CH_EntryStatus);
				AssertTransactionAfterConfirmAction("regLineTransaction", regLineTransaction, transactionStatus: CusTempStorageRegLineTransactionStatusList.Codes.Pending, referenceType: ZString.Empty, mrnCode: ZString.Empty, comment: ZString.Empty, expectedIssueDate: ZDateTimeOffset.Empty, expectedReleaseDate: ZDateTimeOffset.Empty);
			});
		}
	}

	void AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldntDoAction_TemporaryStorageRegisterNotEnabled(string declarationType, string entryInstructionSubStyle, string internalReferenceType, string entryInstructionStyle = "", string docCode = "SUM")
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		{
			var (entryHeader, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(declarationType, entryInstructionSubStyle, internalReferenceType, entryInstructionStyle: entryInstructionStyle, docCode: docCode);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: CH_EntryStatus is empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;

				AssertEquals("CH_EntryStatus is changed to PDA", "PDA", entryHeader.CH_EntryStatus);
				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	void AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldntDoAction_EmptyGoodsLocation(string declarationType, string entryInstructionSubStyle, string internalReferenceType, string entryInstructionStyle = "", string docCode = "SUM")
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(declarationType, entryInstructionSubStyle, internalReferenceType, locationInEntry: ZString.Empty, entryInstructionStyle: entryInstructionStyle, docCode: docCode);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: CH_EntryStatus is empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;

				AssertEquals("CH_EntryStatus is changed to PDA", "PDA", entryHeader.CH_EntryStatus);
				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	void AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldntDoAction_LocationNotManagedInPremises(string declarationType, string entryInstructionSubStyle, string internalReferenceType, string entryInstructionStyle = "", string docCode = "SUM")
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(declarationType, entryInstructionSubStyle, internalReferenceType, locationInPremises: "9999000005", entryInstructionStyle: entryInstructionStyle, docCode: docCode);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: CH_EntryStatus is empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;

				AssertEquals("CH_EntryStatus is changed to PDA", "PDA", entryHeader.CH_EntryStatus);
				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	void AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldntDoActionWithCustomsStatus(ZString customsStatus, string declarationType, string entryInstructionSubStyle, string internalReferenceType, string entryInstructionStyle = "", string docCode = "SUM")
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(declarationType, entryInstructionSubStyle, internalReferenceType, entryInstructionStyle: entryInstructionStyle, docCode: docCode);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: CH_EntryStatus is empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.CH_EntryStatus = customsStatus;

				AssertEquals("CH_EntryStatus is changed to " + customsStatus, customsStatus, entryHeader.CH_EntryStatus);
				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	void AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldDoActionWithCustomsStatus_OnlyCancel(ZString customsStatus, string declarationType, string entryInstructionSubStyle, string internalReferenceType, string entryInstructionStyle = "", string docCode = "SUM")
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(declarationType, entryInstructionSubStyle, internalReferenceType, entryInstructionStyle: entryInstructionStyle, docCode: docCode);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: CH_EntryStatus is empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.CH_EntryStatus = customsStatus;

				AssertEquals("CH_EntryStatus is changed to " + customsStatus, customsStatus, entryHeader.CH_EntryStatus);
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	void AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_ShouldDoActionWithCustomsStatus(ZString customsStatus, string declarationType, string entryInstructionSubStyle, string internalReferenceType, string commentPrefix, string entryInstructionStyle = "", string docCode = "SUM")
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			SetUpRefData();
			Factory.Save();

			var oldComment = "Extra Old Comment";
			var expectedComment = commentPrefix + ": B00000001";

			var orgHeader = SetUpOrgHeader();
			var entryHeader = SetUpHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(orgHeader.MainAddress, declarationType, entryInstructionSubStyle, entryInstructionStyle: entryInstructionStyle, docCode: docCode, isLAME: internalReferenceType == CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration);

			var regHeader1 = SetUpTmpRegHeader();

			var regLine1 = (CusTempStorageRegLine)regHeader1.CusTempStorageRegLines.AddNew();
			regLine1.SRL_LineNumber = 1;
			regLine1.SRL_PackageType = "NE";
			var regLine2 = (CusTempStorageRegLine)regHeader1.CusTempStorageRegLines.AddNew();
			regLine2.SRL_LineNumber = 2;
			regLine2.SRL_PackageType = "VQ";

			var regLineTransaction1 = SetUpTransaction(regLine1, CusTempStorageRegLineTransactionStatusList.Codes.Pending, internalReferenceType, 10, 6);
			var regLineTransaction2 = SetUpTransaction(regLine1, CusTempStorageRegLineTransactionStatusList.Codes.Pending, internalReferenceType, -10, -2);

			var regLineTransaction3 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Deleted, internalReferenceType, -5, -5);
			var regLineTransaction4 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Pending, internalReferenceType, -5, -6);
			var regLineTransaction5 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, internalReferenceType, 10, 6);

			var regHeader2 = SetUpTmpRegHeader(appCode: "BBB", reference: "reference2");

			var regLine3 = (CusTempStorageRegLine)regHeader2.CusTempStorageRegLines.AddNew();
			regLine3.SRL_LineNumber = 1;
			regLine3.SRL_PackageType = "AA";
			var regLine4 = (CusTempStorageRegLine)regHeader2.CusTempStorageRegLines.AddNew();
			regLine4.SRL_LineNumber = 3;
			regLine4.SRL_PackageType = "VG";

			var regLineTransaction6 = SetUpTransaction(regLine3, CusTempStorageRegLineTransactionStatusList.Codes.Pending, internalReferenceType, 5, 6);
			regLineTransaction6.SRT_Comments = oldComment;
			var regLineTransaction7 = SetUpTransaction(regLine4, CusTempStorageRegLineTransactionStatusList.Codes.Pending, internalReferenceType, -5, -6);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: CH_EntryStatus is empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.CH_EntryStatus = customsStatus;

				AssertEquals("CH_EntryStatus is changed to the correct value", customsStatus, entryHeader.CH_EntryStatus);

				AssertTransactionAfterConfirmAction("regLineTransaction1", regLineTransaction1, expectedComment);
				AssertTransactionAfterConfirmAction("regLineTransaction2", regLineTransaction2, expectedComment);
				AssertEquals("regLineTransaction3 was not changed since it wasn't PND", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction3.SRT_TransactionStatus);
				AssertTransactionAfterConfirmAction("regLineTransaction4", regLineTransaction4, expectedComment);
				AssertEquals("regLineTransaction5 was not changed since it wasn't PND", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction5.SRT_TransactionStatus);
				AssertTransactionAfterConfirmAction("regLineTransaction6", regLineTransaction6, expectedComment + " - " + oldComment);
				AssertTransactionAfterConfirmAction("regLineTransaction7", regLineTransaction7, expectedComment);

				AssertEquals("regLine1 CustomsStatus is changed to CLS because the PackagesRemaining is 0", "CLS", regLine1.SRL_CustomsStatus);
				AssertEquals("regLine2 CustomsStatus is changed to CLS because the RemainingGrossWeight is 0", "CLS", regLine2.SRL_CustomsStatus);
				AssertEquals("regHeader1 Status is changed to CLS because all RegLines associated to it have CustomsStatus CLS", "CLS", regHeader1.SRH_Status);

				AssertEquals("regLine3 CustomsStatus is changed to OPN because the PackagesRemaining is not 0", "OPN", regLine3.SRL_CustomsStatus);
				AssertEquals("regLine4 CustomsStatus is changed to OPN because the RemainingGrossWeight is not 0", "OPN", regLine4.SRL_CustomsStatus);
				AssertEquals("regHeader2 Status is changed to OPN because not all RegLines associated to it have CustomsStatus CLS", "OPN", regHeader2.SRH_Status);
			});
		}
	}

	void AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_WriteOff(string declarationType, string entryInstructionSubStyle, string internalReferenceType, string expectedType, string entryInstructionStyle = "", string docCode = "SUM")
	{
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var orgHeader = SetUpOrgHeader();
			var entryHeader = SetUpHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(orgHeader.MainAddress, declarationType, entryInstructionSubStyle, entryInstructionStyle: entryInstructionStyle, docCode: docCode);
			var regHeader = SetUpTmpRegHeader();
			var guarantee = SetUpGauranteeForTempStorage(regHeader, -3.0m, orgHeader);
			var regLine = SetUpRegLine(regHeader);

			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, internalReferenceType, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);

			var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, internalReferenceType, packageQty: 2, grossWeight: -2);
			var regLineTransaction2 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, internalReferenceType, packageQty: 1, grossWeight: -1);

			CombineAssertions(() =>
			{
				AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("[PreReq] Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);
				AssertEquals("[PreReq] Bond Amount transaction2 is default", 0.0m, regLineTransaction2.SRT_BondAmount);

				entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;

				AssertEquals("2 Write off transactions in Guarantee are created", 2, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("Bond Amount transaction1 is calculated", -2.0m, regLineTransaction1.SRT_BondAmount);
				AssertEquals("Bond Amount transaction2 is calculated", -1.0m, regLineTransaction2.SRT_BondAmount);

				var writeOffTransactions = guarantee.CusGuaranteeLineTransactions.Cast<CusGuaranteeLineTransaction>().Where(x => x.CPL_Comment.StartsWith("Write-off"));
				AssertWriteOffTransaction(writeOffTransactions.ElementAtOrDefault(0), "Write-off transaction 1", FormattedDocReference, expectedType, 2.0m);
				AssertWriteOffTransaction(writeOffTransactions.ElementAtOrDefault(1), "Write-off transaction 2", FormattedDocReference, expectedType, 1.0m);
			});
		}
	}

	void AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_NoPendingAmount(string declarationType, string entryInstructionSubStyle, string internalReferenceType, string entryInstructionStyle = "", string docCode = "SUM")
	{
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var orgHeader = SetUpOrgHeader();
			var entryHeader = SetUpHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(orgHeader.MainAddress, declarationType, entryInstructionSubStyle, entryInstructionStyle: entryInstructionStyle, docCode: docCode);
			var regHeader = SetUpTmpRegHeader();
			var guarantee = SetUpGauranteeForTempStorage(regHeader, 0.0m, orgHeader);
			var regLine = SetUpRegLine(regHeader);

			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, internalReferenceType, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);

			var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, internalReferenceType, packageQty: 3, grossWeight: -3);

			CombineAssertions(() =>
			{
				AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("[PreReq] Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);

				entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;

				AssertEquals("No Write off transactions in Guarantee are created ", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("Bond Amount transaction1 is calculated", -3.0m, regLineTransaction1.SRT_BondAmount);
			});
		}
	}

	void AssertConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction_PendingAmountPositive(string declarationType, string entryInstructionSubStyle, string internalReferenceType, string entryInstructionStyle = "", string docCode = "SUM")
	{
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var orgHeader = SetUpOrgHeader();
			var entryHeader = SetUpHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(orgHeader.MainAddress, declarationType, entryInstructionSubStyle, entryInstructionStyle: entryInstructionStyle, docCode: docCode);
			var regHeader = SetUpTmpRegHeader();
			var guarantee = SetUpGauranteeForTempStorage(regHeader, 3.0m, orgHeader);
			var regLine = SetUpRegLine(regHeader);

			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, internalReferenceType, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);

			var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, internalReferenceType, packageQty: 3, grossWeight: -3);

			CombineAssertions(() =>
			{
				AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("[PreReq] Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);

				entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;

				AssertEquals("No Write off transactions in Guarantee are created ", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("Bond Amount transaction1 is calculated", -3.0m, regLineTransaction1.SRT_BondAmount);

				var expectedError = "|RES=Reference 99994000128 has a positive balance of 3.00 EUR. Please check the existing transactions for this reference and create a manual adjustment if needed.|TYP=TS Guarantee";
				AssertEquals("New event in logs", expectedError, entryHeader.Logs.MostRecentLogByEventTime(Events.ErrorReport).SL_Reference);
			});
		}
	}

	void AssertTransactionAfterConfirmAction(ZString transactionName, CusTempStorageRegLineTransaction transaction, string comment, string transactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, string referenceType = "MRN", string mrnCode = MRNCode, ZDateTimeOffset? expectedIssueDate = null, ZDateTimeOffset? expectedReleaseDate = null)
	{
		AssertEquals(transactionName + "'s SRT_TransactionStatus is correct", transactionStatus, transaction.SRT_TransactionStatus);
		AssertEquals(transactionName + "'s SRT_ReferenceType is correct", referenceType, transaction.SRT_ReferenceType);
		AssertEquals(transactionName + "'s SRT_Reference is correct", mrnCode, transaction.SRT_Reference);
		AssertEquals(transactionName + "'s SRT_Comments is correct", comment, transaction.SRT_Comments);
		AssertEquals(transactionName + "'s SRT_TransactionDate is correct", expectedIssueDate != null ? expectedIssueDate : issueDate.ToOffset(), transaction.SRT_TransactionDate);
		AssertEquals(transactionName + "'s SRT_PhysicalInOutDate is correct", expectedReleaseDate != null ? expectedReleaseDate : releaseDate.ToOffset(), transaction.SRT_PhysicalInOutDate);
	}

	void AssertWriteOffTransaction(CusGuaranteeLineTransaction transaction, ZString transactionName, ZString expectedReference, ZString expectedType, ZDecimal expectedTranValue)
	{
		AssertEquals(transactionName + "'s CPL_TransactionType is TRA", "TRA", transaction.CPL_TransactionType);
		AssertEquals(transactionName + "'s CPL_Reference", expectedReference, transaction.CPL_Reference);
		AssertEquals(transactionName + "'s CPL_TransactionDate is Entry Release Date", releaseDate, transaction.CPL_TransactionDate);
		AssertEquals(transactionName + "'s CPL_Comment is Write-off + reference / Type + MRN", string.Format("Write-off TS {0} / {1} {2}", expectedReference, expectedType, MRNCode), transaction.CPL_Comment);
		AssertEquals(transactionName + "'s CPL_TranValue", expectedTranValue, transaction.CPL_TranValue);
		AssertEquals(transactionName + "'s CPL_Transaction Status is CON", "CON", transaction.CPL_TransactionStatus);
	}

	#endregion

	#region ConfirmTemporaryStorageGoodsConsumptionAfterAddingPNDTransactions

	#region Import

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenCCC_WithPNDTransaction_Import()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenCCC_WithPNDTransaction(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenCCC_WithCONTransaction_Import()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenCCC_WithCONTransaction(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA_Import_DocRefShorterThan18()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA_Import_DocRefLongerThan18_WithoutFormatting()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, regHeaderReference: DocReference, docReference: DocReference);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA_Import_DocRefLongerThan18_WithFormatting()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, regHeaderReference: FormattedDocReference, docReference: DocReference);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR_Import_DocRefShorterThan18()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, ExpectedCommentPrefixImport);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR_Import_DocRefLongerThan18_WithoutFormatting()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, ExpectedCommentPrefixImport, regHeaderReference: DocReference, docReference: DocReference);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR_Import_DocRefLongerThan18_WithFormatting()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, ExpectedCommentPrefixImport, regHeaderReference: FormattedDocReference, docReference: DocReference);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsPDAorEmpty_Import()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsPDAorEmpty(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
	}

	#endregion

	#region Import H2

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenCCC_WithPNDTransaction_ImportH2()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenCCC_WithPNDTransaction(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, docCode: "337", entryInstructionStyle: IMPDeclarationTypeList.Codes.H2);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenCCC_WithCONTransaction_ImportH2()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenCCC_WithCONTransaction(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, docCode: "337", entryInstructionStyle: IMPDeclarationTypeList.Codes.H2);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA_ImportH2_DocRefShorterThan18()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, docCode: "337", entryInstructionStyle: IMPDeclarationTypeList.Codes.H2);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA_ImportH2_DocRefLongerThan18_WithoutFormatting()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, docCode: "337", entryInstructionStyle: IMPDeclarationTypeList.Codes.H2, regHeaderReference: DocReference, docReference: DocReference);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA_ImportH2_DocRefLongerThan18_WithFormatting()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, docCode: "337", entryInstructionStyle: IMPDeclarationTypeList.Codes.H2, regHeaderReference: FormattedDocReference, docReference: DocReference);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR_ImportH2_DocRefShorterThan18()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, ExpectedCommentPrefixImport, docCode: "337", entryInstructionStyle: IMPDeclarationTypeList.Codes.H2);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR_ImportH2_DocRefLongerThan18_WithoutFormatting()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, ExpectedCommentPrefixImport, docCode: "337", entryInstructionStyle: IMPDeclarationTypeList.Codes.H2, regHeaderReference: DocReference, docReference: DocReference);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR_ImportH2_DocRefLongerThan18_WithFormatting()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, ExpectedCommentPrefixImport, docCode: "337", entryInstructionStyle: IMPDeclarationTypeList.Codes.H2, regHeaderReference: FormattedDocReference, docReference: DocReference);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsPDAorEmpty_ImportH2()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsPDAorEmpty(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, docCode: "337", entryInstructionStyle: IMPDeclarationTypeList.Codes.H2);
	}

	#endregion

	#region EXS

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenCCC_WithPNDTransaction_EXS()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenCCC_WithPNDTransaction(JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenCCC_WithCONTransaction_EXS()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenCCC_WithCONTransaction(JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA_EXS_SUMDoc_DocRefShorterThan18()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA(JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA_EXS_SUMDoc_DocRefLongerThan18_WithoutFormatting()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA(JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, regHeaderReference: DocReference, docReference: DocReference);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA_EXS_SUMDoc_DocRefLongerThan18_WithFormatting()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA(JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, regHeaderReference: FormattedDocReference, docReference: DocReference);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA_EXS_XSUMDoc_DocRefShorterThan18()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA(JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, docCode: "XSUM");
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA_EXS_XSUMDoc_DocRefLongerThan18_WithoutFormatting()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA(JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, docCode: "XSUM", regHeaderReference: DocReference, docReference: DocReference);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA_EXS_XSUMDoc_DocRefLongerThan18_WithFormatting()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA(JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, docCode: "XSUM", regHeaderReference: FormattedDocReference, docReference: DocReference);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA_EXS_N337Doc_DocRefShorterThan18()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA(JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, docCode: "N337");
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA_EXS_N337Doc_DocRefLongerThan18_WithoutFormatting()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA(JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, docCode: "N337", regHeaderReference: DocReference, docReference: DocReference);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA_EXS_N337Doc_DocRefLongerThan18_WithFormatting()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA(JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, docCode: "N337", regHeaderReference: FormattedDocReference, docReference: DocReference);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR_EXS_SUMDoc_DocRefShorterThan18()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR(JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, ExpectedCommentPrefixExport);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR_EXS_SUMDoc_DocRefLongerThan18_WithoutFormatting()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR(JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, ExpectedCommentPrefixExport, regHeaderReference: DocReference, docReference: DocReference);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR_EXS_SUMDoc_DocRefLongerThan18_WithFormatting()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR(JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, ExpectedCommentPrefixExport, regHeaderReference: FormattedDocReference, docReference: DocReference);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR_EXS_XSUMDoc_DocRefShorterThan18()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR(JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, ExpectedCommentPrefixExport, docCode: "XSUM");
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR_EXS_XSUMDoc_DocRefLongerThan18_WithoutFormatting()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR(JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, ExpectedCommentPrefixExport, docCode: "XSUM", regHeaderReference: DocReference, docReference: DocReference);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR_EXS_XSUMDoc_DocRefLongerThan18_WithFormatting()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR(JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, ExpectedCommentPrefixExport, docCode: "XSUM", regHeaderReference: FormattedDocReference, docReference: DocReference);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR_EXS_N337Doc_DocRefShorterThan18()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR(JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, ExpectedCommentPrefixExport, docCode: "N337");
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR_EXS_N337Doc_DocRefLongerThan18_WithoutFormatting()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR(JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, ExpectedCommentPrefixExport, docCode: "N337", regHeaderReference: DocReference, docReference: DocReference);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR_EXS_N337Doc_DocRefLongerThan18_WithFormatting()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR(JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, ExpectedCommentPrefixExport, docCode: "N337", regHeaderReference: FormattedDocReference, docReference: DocReference);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsEmpty_EXS()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsPDAorEmpty(JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, shouldSetPDA: false);
	}

	#endregion

	#region Export

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenCCC_WithPNDTransaction_Export()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenCCC_WithPNDTransaction(JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, docCode: "1217");
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenCCC_WithCONTransaction_Export()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenCCC_WithCONTransaction(JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, docCode: "1217");
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA_Export_DocRefShorterThan18()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA(JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, docCode: "1217");
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA_Export_DocRefLongerThan18_WithoutFormatting()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA(JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, docCode: "1217", regHeaderReference: DocReference, docReference: DocReference);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA_Export_DocRefLongerThan18_WithFormatting()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA(JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, docCode: "1217", regHeaderReference: FormattedDocReference, docReference: DocReference);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR_Export_DocRefShorterThan18()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR(JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, ExpectedCommentPrefixExport, docCode: "1217");
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR_Export_DocRefLongerThan18_WithoutFormatting()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR(JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, ExpectedCommentPrefixExport, docCode: "1217", regHeaderReference: DocReference, docReference: DocReference);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR_Export_DocRefLongerThan18_WithFormatting()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR(JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, ExpectedCommentPrefixExport, docCode: "1217", regHeaderReference: FormattedDocReference, docReference: DocReference);
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsPDAorEmpty_Export()
	{
		AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsPDAorEmpty(JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.A, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, docCode: "1217");
	}

	#endregion

	void AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenCCC_WithPNDTransaction(string declarationType, string entryInstructionSubStyle, string internalReferenceType, string entryInstructionStyle = "", string docCode = "SUM")
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(declarationType, entryInstructionSubStyle, internalReferenceType, shouldSetDocuments: true, entryInstructionStyle: entryInstructionStyle, docCode: docCode);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: CH_EntryStatus is empty", ZString.Empty, entryHeader.CH_EntryStatus);
				AssertEquals("Prereq: RegLineTransaction count", 1, regLine.CusTempStorageRegLineTransactions.Count(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				entryHeader.CH_EntryStatus = "CCC";

				AssertEquals("CH_EntryStatus is changed to CCC", "CCC", entryHeader.CH_EntryStatus);
				AssertEquals("new regLineTransaction was not created", 1, regLine.CusTempStorageRegLineTransactions.Count(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
			});
		}
	}

	void AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenCCC_WithCONTransaction(string declarationType, string entryInstructionSubStyle, string internalReferenceType, string entryInstructionStyle = "", string docCode = "SUM")
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(declarationType, entryInstructionSubStyle, internalReferenceType, shouldSetDocuments: true, entryInstructionStyle: entryInstructionStyle, docCode: docCode);
			regLineTransaction.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: CH_EntryStatus is empty", ZString.Empty, entryHeader.CH_EntryStatus);
				AssertEquals("Prereq: RegLineTransaction count", 1, regLine.CusTempStorageRegLineTransactions.Count(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				entryHeader.CH_EntryStatus = "CCC";

				AssertEquals("CH_EntryStatus is changed to CCC", "CCC", entryHeader.CH_EntryStatus);
				AssertEquals("new regLineTransaction was not created", 1, regLine.CusTempStorageRegLineTransactions.Count(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
			});
		}
	}

	void AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCDA(string declarationType, string entryInstructionSubStyle, string internalReferenceType, string entryInstructionStyle = "", string docCode = "SUM", string regHeaderReference = FormattedDocReference, string docReference = FormattedDocReference)
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(declarationType, entryInstructionSubStyle, internalReferenceType, createTransaction: false, shouldSetDocuments: true, entryInstructionStyle: entryInstructionStyle, docCode: docCode, regHeaderReference: regHeaderReference, docReference: docReference);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: CH_EntryStatus is empty", ZString.Empty, entryHeader.CH_EntryStatus);
				AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				entryHeader.CH_EntryStatus = EntryStatusCodes.CustomsDeclarationAccepted;

				AssertEquals("CH_EntryStatus is changed to CDA", EntryStatusCodes.CustomsDeclarationAccepted, entryHeader.CH_EntryStatus);
				AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

				var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
				AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
				AssertEquals("regLineTransaction created with SRT_ReferenceType", ZString.Empty, transaction.SRT_ReferenceType);
				AssertEquals("regLineTransaction created with SRT_Reference", ZString.Empty, transaction.SRT_Reference);
				AssertEquals("regLineTransaction created with SRT_Comments", ZString.Empty, transaction.SRT_Comments);
			});
		}
	}

	void AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsCLR(string declarationType, string entryInstructionSubStyle, string internalReferenceType, string commentPrefix, string entryInstructionStyle = "", string docCode = "SUM", string regHeaderReference = FormattedDocReference, string docReference = FormattedDocReference)
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(declarationType, entryInstructionSubStyle, internalReferenceType, createTransaction: false, shouldSetDocuments: true, entryInstructionStyle: entryInstructionStyle, docCode: docCode, regHeaderReference: regHeaderReference, docReference: docReference);

			var expectedComment = commentPrefix + ": B00000001";

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: CH_EntryStatus is empty", ZString.Empty, entryHeader.CH_EntryStatus);
				AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;

				AssertEquals("CH_EntryStatus is changed to CLR", EntryStatusCodes.Cleared, entryHeader.CH_EntryStatus);
				AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

				var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
				AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
				AssertEquals("regLineTransaction created with SRT_ReferenceType", CusEntryNumberTypes.Standard.MovementReferenceNumber, transaction.SRT_ReferenceType);
				AssertEquals("regLineTransaction created with SRT_Reference", MRNCode, transaction.SRT_Reference);
				AssertEquals("regLineTransaction created with SRT_Comments", expectedComment, transaction.SRT_Comments);
			});
		}
	}

	void AssertConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmptyOrPDA_WhenIsPDAorEmpty(string declarationType, string entryInstructionSubStyle, string internalReferenceType, string entryInstructionStyle = "", string docCode = "SUM", bool shouldSetPDA = true)
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(declarationType, entryInstructionSubStyle, internalReferenceType, locationInEntry: ZString.Empty, createTransaction: false, shouldSetDocuments: true, entryInstructionStyle: entryInstructionStyle, docCode: docCode);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: CH_EntryStatus is empty", ZString.Empty, entryHeader.CH_EntryStatus);
				AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

				if (shouldSetPDA)
				{
					entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
					AssertEquals("CH_EntryStatus is changed to PDA", EntryStatusCodes.PreDeclarationAccepted, entryHeader.CH_EntryStatus);
					AssertEquals("regLineTransaction was not created (PDA)", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				}

				entryHeader.CH_EntryStatus = ZString.Empty;
				AssertEquals("CH_EntryStatus is changed to empty", ZString.Empty, entryHeader.CH_EntryStatus);
				AssertEquals("regLineTransaction was not created (empty)", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
			});
		}
	}

	#endregion

	#region SetUp

	CusEntryHeader SetUpHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(OrgAddress orgAddress, string declarationType, string entryInstructionSubStyle, string locationInEntry = LocationInEntry, string locationInPremises = LocationInEntry
		, string entryInstructionStyle = "", bool shouldSetDocuments = false, string docCode = "SUM", bool isLAME = false, string docReference = FormattedDocReference)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_DeclarationReference = DeclarationReference;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = declarationType;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = entryInstructionSubStyle;
		entryInstruction.CEI_Style = entryInstructionStyle;

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_Weight = 100.4455m;
		invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;

		var packingGroups = declaration.Bills.AddNew().PackingGroups.AddNew();
		var packageInfo = packingGroups.Packages.AddNew();
		packageInfo.CW_PackQty = 9;
		packageInfo.CW_PackType = "CT";
		packageInfo.CW_MarksAndNos = "marks";
		var pack = Factory.New<InvoiceLinePackagePivot>();
		pack.CHC_CW = packageInfo.PK;
		pack.CHC_NumberOfPacks = 9;
		invoiceLine.PackagesPivot.Add(pack);

		if (shouldSetDocuments)
		{
			if (isLAME)
			{
				var supDoc = invoiceLine.SupportingDocuments.AddNew();
				supDoc.CSI_Code = docCode;
				supDoc.CSI_ReferenceNumber = docReference;
			}
			else
			{
				var previousDoc = invoiceLine.PreviousDocuments.AddNew();
				previousDoc.CSI_Code = docCode;
				previousDoc.CSI_ReferenceNumber = docReference;
				previousDoc.CSI_LineNo = 1;
			}
		}

		var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge done", true, mergeResult);
		Factory.Save();

		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.CH_BGMReference = EntryReference;
		entryHeader.MovementReferenceNumberSetter(MRNCode, issueDate);
		entryHeader.CH_EntryReleaseDate = releaseDate;

		declaration.CustomsEntryInstructions[0].GoodsLocation.Address.AuthorisationNumber = locationInEntry;

		var premises = Factory.New<CusTempStorageRegPremises>();
		premises.SRP_Type = isLAME ? "LAM" : "ADT";
		premises.SRP_CustomsLocation = locationInPremises;
		premises.SRP_Code = "X";
		premises.SRP_Description = "DESC";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;

		return entryHeader;
	}

	(CusEntryHeader entryHeader, CusTempStorageRegLineTransaction regLineTransaction, CusTempStorageRegLine regLine) SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction
		(string declarationType, string entryInstructionSubStyle, string internalReferenceType, string locationInEntry = LocationInEntry, string locationInPremises = LocationInEntry, string entryInstructionStyle = ""
		, bool createTransaction = true, bool shouldSetDocuments = false, string docCode = "SUM", string regHeaderReference = FormattedDocReference, string docReference = FormattedDocReference)
	{
		var orgHeader = SetUpOrgHeader();
		var entryHeader = SetUpHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(orgHeader.MainAddress, declarationType, entryInstructionSubStyle, locationInEntry: locationInEntry, locationInPremises: locationInPremises, entryInstructionStyle: entryInstructionStyle, shouldSetDocuments: shouldSetDocuments, docCode: docCode, isLAME: internalReferenceType == CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, docReference: docReference);

		var regHeader = SetUpTmpRegHeader(reference: regHeaderReference);
		var regLine = Factory.New<CusTempStorageRegLine>();
		regLine.SRL_LineNumber = 1;
		regLine.SRL_CustomsStatus = "OPN";
		regLine.SRL_PackageType = "CT";
		regLine.SRL_SRH = regHeader.PK;
		var regLineTransaction = (CusTempStorageRegLineTransaction)null;
		if (createTransaction)
		{
			regLineTransaction = (CusTempStorageRegLineTransaction)regLine.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
			regLineTransaction.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction.SRT_InternalReferenceType = internalReferenceType;
		}

		var regLineItem = Factory.New<CusTempStorageRegLineItem>();
		regLineItem.SRI_GoodsItemNumber = 1;

		var regLineItemPivot = Factory.New<CusTempStorageRegLineItemPivot>();
		regLineItemPivot.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot.SRV_SRL_Line = regLine.PK;

		SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, internalReferenceType, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);
		Factory.Save();

		return (entryHeader, regLineTransaction, regLine);
	}

	CusTempStorageRegLineTransaction SetUpTransaction(CusTempStorageRegLine regLine, ZString transactionStatus, ZString internalReferenceType, int packageQty = 0, decimal grossWeight = 0, string transactionType = "TRN", decimal bondAmount = 0.0m)
	{
		var regLineTransaction = (CusTempStorageRegLineTransaction)regLine.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction.SRT_TransactionType = transactionType;
		regLineTransaction.SRT_TransactionStatus = transactionStatus;
		regLineTransaction.SRT_InternalReferenceType = internalReferenceType;
		regLineTransaction.SRT_PackageQty = packageQty;
		regLineTransaction.SRT_GrossWeight = grossWeight;

		if (transactionType == "OBL")
		{
			regLineTransaction.SRT_BondAmount = bondAmount;
		}
		else
		{
			regLineTransaction.SRT_InternalReferenceNumber = EntryReference;
		}

		return regLineTransaction;
	}

	void SetUpRefData()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UN Package Types");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"VQ", "VQ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"VG", "VG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"NF", "NF", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"AA", "AA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
	}

	CusTempStorageRegHeader SetUpTmpRegHeader(string appCode = "AAA", string reference = FormattedDocReference)
	{
		var regHeader = Factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_AppCode = appCode;
		regHeader.SRH_Reference = reference;

		return regHeader;
	}

	EU.Business.CusGuaranteeHeader SetUpGauranteeForTempStorage(CusTempStorageRegHeader regHeader, ZDecimal value, OrgHeader orgHeader)
	{
		var cusGuarantee = Factory.New<CusGuaranteeHeader>();
		cusGuarantee.CPH_Number = "Test1";
		cusGuarantee.CPH_OH_PermitHolder = orgHeader.PK;
		cusGuarantee.CPH_Type = EUGuaranteeTypeList.Codes.TST;
		cusGuarantee.CPH_SubType = "1";
		cusGuarantee.CPH_StartDate = ZDate.BrettsBirthday;
		cusGuarantee.CPH_Balance = 1000.0m;

		var commonGuarantee = Factory.New<EU.Business.Declaration.CommonGuarantee>();
		commonGuarantee.PW_BondNumber = "Test1";
		commonGuarantee.PW_ParentID = regHeader.PK;
		commonGuarantee.PW_ParentTableCode = CusBondDetailSchema.Constants.Prefix;
		commonGuarantee.PW_CPH_Guarantee = cusGuarantee.PK;

		var guarantee = regHeader.Guarantee.CusGuarantee;
		var guaranteeLineTransaction = guarantee.CusGuaranteeLineTransactions.AddNew();
		guaranteeLineTransaction.CPL_Reference = FormattedDocReference;
		guaranteeLineTransaction.CPL_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		guaranteeLineTransaction.CPL_TranValue = value;

		guarantee.AddTransaction("OPENING", "OPENING", ZString.Empty, ZString.Empty, 1000.0m, 0, transactionType: Customs.Business.PermitTransactionTypeList.Codes.OBL, isAggregated: true);

		return guarantee;
	}

	CusTempStorageRegLine SetUpRegLine(CusTempStorageRegHeader regHeader)
	{
		var regLine = (CusTempStorageRegLine)regHeader.CusTempStorageRegLines.AddNew();
		regLine.SRL_LineNumber = 1;
		regLine.SRL_PackageType = "BX";

		return regLine;
	}

	OrgHeader SetUpOrgHeader()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AAA";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";

		return orgHeader;
	}

	#endregion SetUp

	#region ReserveTemporaryStorageGoods

	public void TestGetEntryLineDataDeclaredToReserveTSGoods()
	{
		Factory.SetBulkTypeHelper();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entryHeader.MergedLines.AddNew();
		var entryLine2 = entryHeader.MergedLines.AddNew();
		var entryLine3 = entryHeader.MergedLines.AddNew();
		var entryLine4 = entryHeader.MergedLines.AddNew();

		CombineAssertions(() =>
		{
			var (declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertEquals("0 documents are returned since there are no documents in the declaration", 0, declarationDataToReserveTSGoodsList.Count());
			AssertEquals("empty string is returned since there are no documents in the declaration", ZString.Empty, messageReturned);

			var invoiceLine1 = (JobComInvoiceLine)entryLine1.InvoiceLines.AddNew();
			var vehicle11 = invoiceLine1.Vehicles.AddNew();
			var vehicle12 = invoiceLine1.Vehicles.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			var previousDoc1 = invoiceLine1.PreviousDocuments.AddNew();
			previousDoc1.CSI_ReferenceNumber = "Reference";
			previousDoc1.CSI_Code = "BBB";
			invoiceLine1.JI_Weight = 200.4455m;
			invoiceLine1.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			vehicle11.CVH_VehicleIdentificationNumber = "VIN1";
			vehicle12.CVH_VehicleIdentificationNumber = "VINX";

			var invoiceLine2 = (JobComInvoiceLine)entryLine1.InvoiceLines.AddNew();
			var vehicle2 = invoiceLine2.Vehicles.AddNew();
			invoiceLine2.JI_CL = entryLine1.PK;
			var previousDoc2 = invoiceLine2.PreviousDocuments.AddNew();
			previousDoc2.CSI_ReferenceNumber = "Reference";
			previousDoc2.CSI_Code = "BBB";
			invoiceLine2.JI_Weight = 2000.4455m;
			invoiceLine2.JI_WeightUQ = Core.Constants.Weight.Grams;
			vehicle2.CVH_VehicleIdentificationNumber = "VIN2";

			var invoiceLine3 = (JobComInvoiceLine)entryLine1.InvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine1.PK;
			var previousDoc3 = invoiceLine3.PreviousDocuments.AddNew();
			previousDoc3.CSI_ReferenceNumber = "Reference";
			previousDoc3.CSI_Code = "BBB";
			invoiceLine3.JI_Weight = 100.4455m;
			invoiceLine3.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			var packageInfo1 = Factory.New<BasePackage>();
			packageInfo1.CW_PackType = "CT";
			packageInfo1.CW_MarksAndNos = "marks";
			var pack1 = Factory.New<InvoiceLinePackagePivot>();
			pack1.CHC_CW = packageInfo1.PK;
			pack1.CHC_NumberOfPacks = 9;
			invoiceLine3.PackagesPivot.Add(pack1);

			var packageInfo2 = Factory.New<BasePackage>();
			packageInfo2.CW_PackType = "NE";
			packageInfo2.CW_MarksAndNos = "marks2";
			var pack2 = Factory.New<InvoiceLinePackagePivot>();
			pack2.CHC_CW = packageInfo2.PK;
			pack2.CHC_NumberOfPacks = 4;
			invoiceLine3.PackagesPivot.Add(pack2);

			var pack3 = Factory.New<InvoiceLinePackagePivot>();
			var packageInfo3 = Factory.New<BasePackage>();
			packageInfo3.CW_PackType = "VG";
			packageInfo3.CW_MarksAndNos = "bulk gas marks";
			pack3.CHC_CW = packageInfo3.PK;
			pack3.CHC_NumberOfPacks = 2;
			invoiceLine3.PackagesPivot.Add(pack3);

			var invoiceLine4 = (JobComInvoiceLine)entryLine2.InvoiceLines.AddNew();
			invoiceLine4.JI_CL = entryLine2.PK;
			var previousDoc4 = invoiceLine4.PreviousDocuments.AddNew();
			previousDoc4.CSI_ReferenceNumber = "Reference2";
			previousDoc4.CSI_Code = "AAA";
			invoiceLine4.JI_Weight = 0.9886m;
			invoiceLine4.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			var packageInfo4 = Factory.New<BasePackage>();
			packageInfo4.CW_PackType = "CT";
			packageInfo4.CW_MarksAndNos = "marks4";
			var pack4 = Factory.New<InvoiceLinePackagePivot>();
			pack4.CHC_CW = packageInfo4.PK;
			pack4.CHC_NumberOfPacks = 8;
			invoiceLine4.PackagesPivot.Add(pack4);

			var invoiceLine5 = (JobComInvoiceLine)entryLine3.InvoiceLines.AddNew();
			invoiceLine5.JI_CL = entryLine3.PK;
			var previousDoc5 = invoiceLine5.PreviousDocuments.AddNew();
			previousDoc5.CSI_ReferenceNumber = "Reference3";
			previousDoc5.CSI_Code = "SUM";
			invoiceLine5.JI_Weight = 300.4455m;
			invoiceLine5.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			var packageInfo5 = Factory.New<BasePackage>();
			packageInfo5.CW_PackType = "BX";
			packageInfo5.CW_MarksAndNos = "marks5";
			var pack5 = Factory.New<InvoiceLinePackagePivot>();
			pack5.CHC_CW = packageInfo5.PK;
			pack5.CHC_NumberOfPacks = 7;
			invoiceLine5.PackagesPivot.Add(pack5);

			var invoiceLine6 = (JobComInvoiceLine)entryLine4.InvoiceLines.AddNew();
			invoiceLine6.JI_CL = entryLine4.PK;
			var previousDoc6 = invoiceLine6.PreviousDocuments.AddNew();
			previousDoc6.CSI_ReferenceNumber = "Reference";
			previousDoc6.CSI_Code = "SUM";
			invoiceLine6.JI_Weight = 400.4455m;
			invoiceLine6.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			var packageInfo6 = Factory.New<BasePackage>();
			packageInfo6.CW_PackType = "VG";
			packageInfo6.CW_MarksAndNos = "marks6";
			var pack6 = Factory.New<InvoiceLinePackagePivot>();
			pack6.CHC_CW = packageInfo6.PK;
			pack6.CHC_NumberOfPacks = 6;
			invoiceLine6.PackagesPivot.Add(pack6);

			var packageInfo7 = Factory.New<BasePackage>();
			packageInfo7.CW_PackType = "CT";
			packageInfo7.CW_MarksAndNos = "marks";
			var pack7 = Factory.New<InvoiceLinePackagePivot>();
			pack7.CHC_CW = packageInfo7.PK;
			pack7.CHC_NumberOfPacks = 6;
			invoiceLine6.PackagesPivot.Add(pack7);

			var expectedPackagesForEntryLine1 = new List<(ZString type, ZInt qty, ZString marksOrVin, ZBool isBulk)>
				{
					("FR", 1, "VIN1", false),
					("FR", 1, "VINX", false),
					("FR", 1, "VIN2", false),
					("CT", 15, "marks", false),
					("NE", 4, "marks2", false),
					("VG", 2, "bulk gas marks", true),
					("VG", 6, "marks6", true)
				};
			var expectedPackagesForEntryLine2 = new List<(ZString type, ZInt qty, ZString marksOrVin, ZBool isBulk)> { ("CT", 8, "marks4", false) };
			var expectedPackagesForEntryLine3 = new List<(ZString type, ZInt qty, ZString marksOrVin, ZBool isBulk)> { ("BX", 7, "marks5", false) };

			(declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertEquals("declarationDataToReserveTSGoodsList has 3 elements", 3, declarationDataToReserveTSGoodsList.Count());
			AssertEquals("empty string is returned even when there are documents in the declaration", ZString.Empty, messageReturned);

			var declarationDataToReserveTSGoods1 = declarationDataToReserveTSGoodsList.First(d => d.TotalGrossWeight == 703.338446m);
			AssertDeclarationDataToReserveTSGoods("declarationDataToReserveTSGoods1", declarationDataToReserveTSGoods1, "Reference", 703.338446m, 302.892446m, expectedPackagesForEntryLine1);

			var declarationDataToReserveTSGoods2 = declarationDataToReserveTSGoodsList.First(d => d.TotalGrossWeight == 0.989m);
			AssertDeclarationDataToReserveTSGoods("declarationDataToReserveTSGoods2", declarationDataToReserveTSGoods2, "Reference2", 0.989m, 0, expectedPackagesForEntryLine2);

			var declarationDataToReserveTSGoods3 = declarationDataToReserveTSGoodsList.First(d => d.TotalGrossWeight == 300.446m);
			AssertDeclarationDataToReserveTSGoods("declarationDataToReserveTSGoods3", declarationDataToReserveTSGoods3, "Reference3", 300.446m, 0, expectedPackagesForEntryLine3);
		});

		void AssertDeclarationDataToReserveTSGoods(ZString message, DeclarationDataToReserveTSGoods declarationData, ZString expectedDocRef, ZDecimal expectedGrossWeight, ZDecimal expectedGrossWeightForVINs, List<(ZString type, ZInt qty, ZString marksOrVin, ZBool isBulk)> expectedPackages)
		{
			AssertEquals(message + " docRef", expectedDocRef, declarationData.Document.CSI_ReferenceNumber);
			AssertEquals(message + " totalGrossWeight", expectedGrossWeight, declarationData.TotalGrossWeight);
			AssertEquals(message + " totalGrossWeightForVINs", expectedGrossWeightForVINs, declarationData.TotalGrossWeightForVINs);
			AssertContainsExactElementsInAnyOrder(message + " packages", expectedPackages, declarationData.Packages);
		}
	}

	public void TestGetInvoiceLineDataDeclaredToReserveTSGoodsForExport_Errors()
	{
		var errorForDocInWrongParent = "ES00001: For managed LAME storages, LAME Reception Certificate (document 1217) must always be declared at Invoice Line level to be able to manage the inventory properly. Please, correct data and send again.";
		var errorForMultiplePackageTypesInOneLine = "ES00001: For managed LAME storages, only one package type per Invoice Line must be entered. If more than one, please, create as many Invoice Lines as package types so the inventory can be managed properly. Please, correct data and send again.";

		var errorForMultipleDocsMoreThanOneLameCertificateGrossWeightInvoice1 = "ES00001 / Invoice 1 / 1: For invoice lines where there is more than one LAME Certificate, it is needed to specify the corresponding Gross Weight for each certificate in column Quantity, setting UOM to KGM, so the inventory can be managed properly. Please, set that data and send again.";
		var errorForMultipleDocsMoreThanOneLameCertificateGrossWeightInvoice2 = "ES00001 / Invoice 2 / 1: For invoice lines where there is more than one LAME Certificate, it is needed to specify the corresponding Gross Weight for each certificate in column Quantity, setting UOM to KGM, so the inventory can be managed properly. Please, set that data and send again.";

		var errorForMultipleDocsMoreThanOneLameCertificatePackQuantityInvoice1 = "ES00001 / Invoice 1 / 1: For invoice lines where there is more than one LAME Certificate, it is needed to specify the corresponding Pack Quantity for each certificate in column Pack Qty, setting Pack Type to the corresponding Type of packages, so the inventory can be managed properly. Please, set that data and send again.";
		var errorForMultipleDocsMoreThanOneLameCertificatePackQuantityInvoice2 = "ES00001 / Invoice 2 / 1: For invoice lines where there is more than one LAME Certificate, it is needed to specify the corresponding Pack Quantity for each certificate in column Pack Qty, setting Pack Type to the corresponding Type of packages, so the inventory can be managed properly. Please, set that data and send again.";

		var errorForMultipleDocsSumGrossWeightGreaterGrossWeightInvoiceLineInvoice1 = "ES00001 / Invoice 1 / 1: The SUM of Gross Weight declared in the LAME Certificates (115) is greater than the Gross Weight declared in the Invoice Line (100). Please, correct those values and send again.";
		var errorForMultipleDocsSumGrossWeightGreaterGrossWeightInvoiceLineInvoice2 = "ES00001 / Invoice 2 / 1: The SUM of Gross Weight declared in the LAME Certificates (105) is greater than the Gross Weight declared in the Invoice Line (50). Please, correct those values and send again.";

		var errorForMultipleDocsSumPackagesGreaterPackagesInvoiceLineInvoice1 = "ES00001 / Invoice 1 / 1: The SUM of Packages declared in the LAME Certificates (30) is greater than the number of packages declared in the Invoice Line (2). Please, correct those values and send again.";
		var errorForMultipleDocsSumPackagesGreaterPackagesInvoiceLineInvoice2 = "ES00001 / Invoice 2 / 1: The SUM of Packages declared in the LAME Certificates (30) is greater than the number of packages declared in the Invoice Line (2). Please, correct those values and send again.";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		var invoiceHeader1 = declaration.Invoices.AddNew();
		invoiceHeader1.JZ_InvoiceNumber = "1";
		var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;

		var invoiceHeader2 = declaration.Invoices.AddNew();
		invoiceHeader2.JZ_InvoiceNumber = "2";
		var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction.PK;

		CombineAssertions(() =>
		{
			var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_BGMReference = EntryReference;

			var (declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertEquals("0 documents are returned since there are no documents in the declaration", 0, declarationDataToReserveTSGoodsList.Count());
			AssertEquals("empty string is returned since there are no documents in the declaration", ZString.Empty, messageReturned);

			var doc1 = declaration.SupportingDocuments.AddNew();
			doc1.CSI_Code = "1217";

			(declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertEquals("0 documents are returned since there are no documents in invoice lines (doc 1217 in declaration)", 0, declarationDataToReserveTSGoodsList.Count());
			AssertEquals("error is returned since there is at least one 1217 document in the declaration", errorForDocInWrongParent, messageReturned);

			doc1.CSI_Code = "BBB";

			(declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertEquals("0 documents are returned since there are no documents in invoice lines (doc BBB in declaration)", 0, declarationDataToReserveTSGoodsList.Count());
			AssertEquals("empty string is returned since there are no 1217 documents in the declaration", ZString.Empty, messageReturned);

			doc1 = invoiceHeader1.SupportingDocuments.AddNew();
			doc1.CSI_Code = "1217";

			(declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertEquals("0 documents are returned since there are no documents in invoice lines (doc 1217 in invoice header)", 0, declarationDataToReserveTSGoodsList.Count());
			AssertEquals("error is returned since there is at least one 1217 document in the invoice header", errorForDocInWrongParent, messageReturned);

			doc1.CSI_Code = "BBB";

			(declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertEquals("0 documents are returned since there are no documents in invoice lines (doc BBBB in invoice header)", 0, declarationDataToReserveTSGoodsList.Count());
			AssertEquals("empty string is returned since there are no 1217 documents in the invoice header", ZString.Empty, messageReturned);

			doc1 = entryInstruction.SupportingDocuments.AddNew();
			doc1.CSI_Code = "1217";

			(declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertEquals("0 documents are returned since there are no documents in invoice lines (doc 1217 in entry instruction)", 0, declarationDataToReserveTSGoodsList.Count());
			AssertEquals("error is returned since there is at least one 1217 document in the entry instruction", errorForDocInWrongParent, messageReturned);

			doc1.CSI_Code = "BBB";

			(declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertEquals("0 documents are returned since there are no documents in invoice lines (doc BBBB in entry instruction)", 0, declarationDataToReserveTSGoodsList.Count());
			AssertEquals("empty string is returned since there are no 1217 documents in the entry instruction", ZString.Empty, messageReturned);

			doc1 = invoiceLine1.SupportingDocuments.AddNew();
			doc1.CSI_ReferenceNumber = "Reference";
			doc1.CSI_Code = "1217";
			doc1.CSI_Quantity = 10m;
			doc1.CSI_UnitOfQuantity = "KGM";

			var doc2 = invoiceLine1.SupportingDocuments.AddNew();
			doc2.CSI_ReferenceNumber = "Reference2";
			doc2.CSI_Code = "1217";
			doc2.CSI_Quantity = 10m;
			doc2.CSI_UnitOfQuantity = "KGM";

			var doc3 = invoiceLine1.SupportingDocuments.AddNew();
			doc3.CSI_ReferenceNumber = "Reference3";
			doc3.CSI_Code = "1217";
			doc3.CSI_UnitOfQuantity = "KGM";

			(declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertEquals("0 documents are returned since there are documents in invoice lines but there is an error (no Quantity in at least one of them)", 0, declarationDataToReserveTSGoodsList.Count());
			AssertEquals("error is returned since there are documents in invoice line 1 but at least one of them is missing Quantity", errorForMultipleDocsMoreThanOneLameCertificateGrossWeightInvoice1, messageReturned);

			invoiceLine1.JI_Weight = 100m;
			invoiceLine1.JI_WeightUQ = "KG";
			doc3.CSI_Quantity = 10m;
			var vehicle1 = invoiceLine1.Vehicles.AddNew();
			vehicle1.CVH_VehicleIdentificationNumber = "VIN1";

			var doc4 = invoiceLine2.SupportingDocuments.AddNew();
			doc4.CSI_ReferenceNumber = "Reference";
			doc4.CSI_Code = "1217";
			doc4.CSI_Quantity = 10m;
			doc4.CSI_UnitOfQuantity = "KGM";

			var doc5 = invoiceLine2.SupportingDocuments.AddNew();
			doc5.CSI_ReferenceNumber = "Reference2";
			doc5.CSI_Code = "1217";
			doc5.CSI_Quantity = 10m;
			doc5.CSI_UnitOfQuantity = "AAA";

			(declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertEquals("0 documents are returned since there are documents in invoice lines but there is an error (UOM is not KGM in at least one of them)", 0, declarationDataToReserveTSGoodsList.Count());
			AssertEquals("error is returned since there are documents in invoice line 2 but at least one of them has the wrong UOM", errorForMultipleDocsMoreThanOneLameCertificateGrossWeightInvoice2, messageReturned);

			doc5.CSI_UnitOfQuantity = "KGM";

			var packageInfo1 = Factory.New<BasePackage>();
			packageInfo1.CW_PackType = "CT";
			packageInfo1.CW_MarksAndNos = "marks";
			var pack1 = Factory.New<InvoiceLinePackagePivot>();
			pack1.CHC_CW = packageInfo1.PK;
			pack1.CHC_NumberOfPacks = 9;
			invoiceLine2.PackagesPivot.Add(pack1);

			var packageInfo2 = Factory.New<BasePackage>();
			packageInfo2.CW_PackType = "NE";
			packageInfo2.CW_MarksAndNos = "marks2";
			var pack2 = Factory.New<InvoiceLinePackagePivot>();
			pack2.CHC_CW = packageInfo2.PK;
			pack2.CHC_NumberOfPacks = 4;
			invoiceLine2.PackagesPivot.Add(pack2);

			(declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertEquals("0 documents are returned since there are documents and packages in invoice lines but there is an error (ther are multiple packages with different types in one line)", 0, declarationDataToReserveTSGoodsList.Count());
			AssertEquals("error is returned since there are documents and packages in invoice lines but there are multiple package types in one invoice line", errorForMultiplePackageTypesInOneLine, messageReturned);

			doc1.CSI_PackType = "KGM";
			doc1.CSI_PackQty = 0;
			doc2.CSI_PackType = "KGM";
			doc2.CSI_PackQty = 0;
			doc3.CSI_PackType = "KGM";
			doc3.CSI_PackQty = 0;
			vehicle1.Delete();

			(declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertEquals("0 documents are returned since there are documents and Pack type is not BULK and not Vehicles for each certificate in column Pack Qty = 0 Or Pack Type is different the invoice line (Invoice Line 1)", 0, declarationDataToReserveTSGoodsList.Count());
			AssertEquals("error is returned since there are documents and Pack type is not BULK and not Vehicles for each certificate in column Pack Qty = 0 Or Pack Type is different the invoice line (Invoice Line 1)", errorForMultipleDocsMoreThanOneLameCertificatePackQuantityInvoice1, messageReturned);

			doc1.CSI_PackQty = 10;
			doc1.CSI_PackType = "CT";
			doc2.CSI_PackQty = 10;
			doc2.CSI_PackType = "CT";
			doc3.CSI_PackQty = 10;
			doc3.CSI_PackType = "CT";

			var packageInfo3 = Factory.New<BasePackage>();
			packageInfo3.CW_PackType = "CT";
			packageInfo3.CW_MarksAndNos = "marks";
			packageInfo3.CW_PackQty = 30;
			var pack3 = Factory.New<InvoiceLinePackagePivot>();
			pack3.CHC_CW = packageInfo3.PK;
			pack3.CHC_NumberOfPacks = 60;
			invoiceLine1.PackagesPivot.Add(pack3);

			doc4.CSI_PackType = "NE";
			doc4.CSI_PackQty = 0;
			doc5.CSI_PackType = "KGM";
			doc5.CSI_PackQty = 0;

			packageInfo2.CW_PackType = "CT";

			(declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertEquals("0 documents are returned since there are documents and Pack type is not BULK and not Vehicles for each certificate in column Pack Qty = 0 Or Pack Type is different the invoice line (Invoice Line 2)", 0, declarationDataToReserveTSGoodsList.Count());
			AssertEquals("error is returned since there are documents and Pack type is not BULK and not Vehicles for each certificate in column Pack Qty = 0 Or Pack Type is different the invoice line (Invoice Line 2)", errorForMultipleDocsMoreThanOneLameCertificatePackQuantityInvoice2, messageReturned);

			doc4.CSI_PackType = "CT";
			doc4.CSI_PackQty = 15;
			doc5.CSI_PackType = "CT";
			doc5.CSI_PackQty = 15;

			doc5.CSI_Quantity = 10m;

			doc3.CSI_UnitOfQuantity = "KGM";
			doc3.CSI_Quantity = 95m;

			(declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertEquals("0 documents are returned since there are documents and Gross Weight for each certificate in column Quantity = 0 and UnitOfQuantity not KGM in invoice (Invoice Line 1)", 0, declarationDataToReserveTSGoodsList.Count());
			AssertEquals("error is returned since there are documents and Gross Weight for each certificate in column Quantity = 0 and UnitOfQuantity not KGM in invoice (Invoice Line 1)", errorForMultipleDocsSumGrossWeightGreaterGrossWeightInvoiceLineInvoice1, messageReturned);

			doc3.CSI_Quantity = 10m;

			invoiceLine2.JI_Weight = 50m;
			invoiceLine2.JI_WeightUQ = "KG";
			doc5.CSI_UnitOfQuantity = "KGM";
			doc5.CSI_Quantity = 95m;

			(declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertEquals("0 documents are returned since there are documents and Gross Weight for each certificate in column Quantity = 0 and UnitOfQuantity not KGM in invoice (Invoice Line 2)", 0, declarationDataToReserveTSGoodsList.Count());
			AssertEquals("error is returned since there are documents and Gross Weight for each certificate in column Quantity = 0 and UnitOfQuantity not KGM in invoice (Invoice Line 2)", errorForMultipleDocsSumGrossWeightGreaterGrossWeightInvoiceLineInvoice2, messageReturned);

			doc5.CSI_Quantity = 10m;
			packageInfo3.CW_PackQty = 2;
			pack3.CHC_NumberOfPacks = 2;

			(declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertEquals("0 documents are returned since there are documents and Pack Weight for each certificate in column Quantity = 0 and UnitOfQuantity not KGM in invoice (Invoice Line 1)", 0, declarationDataToReserveTSGoodsList.Count());
			AssertEquals("error is returned since there are documents and Pack Weight for each certificate in column Quantity = 0 and UnitOfQuantity not KGM in invoice (Invoice Line 1)", errorForMultipleDocsSumPackagesGreaterPackagesInvoiceLineInvoice1, messageReturned);

			packageInfo3.CW_PackQty = 30;
			packageInfo2.CW_PackQty = 2;
			pack3.CHC_NumberOfPacks = 60;
			pack1.CHC_NumberOfPacks = 1;
			pack2.CHC_NumberOfPacks = 1;

			(declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertEquals("0 documents are returned since there are documents and Pack Weight for each certificate in column Quantity = 0 and UnitOfQuantity not KGM in invoice (Invoice Line 2)", 0, declarationDataToReserveTSGoodsList.Count());
			AssertEquals("error is returned since there are documents and Pack Weight for each certificate in column Quantity = 0 and UnitOfQuantity not KGM in invoice (Invoice Line 2)", errorForMultipleDocsSumPackagesGreaterPackagesInvoiceLineInvoice2, messageReturned);
		});
	}

	public void TestGetInvoiceLineDataDeclaredToReserveTSGoodsForExport()
	{
		Factory.SetBulkTypeHelper();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.CH_BGMReference = EntryReference;
		var entryLine1 = entryHeader.MergedLines.AddNew();
		var entryLine2 = entryHeader.MergedLines.AddNew();
		var entryLine3 = entryHeader.MergedLines.AddNew();
		var entryLine4 = entryHeader.MergedLines.AddNew();

		var invoiceHeader1 = declaration.Invoices.AddNew();
		var invoiceHeader2 = declaration.Invoices.AddNew();

		CombineAssertions(() =>
		{
			var (declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertEquals("0 documents are returned since there are no documents in the declaration", 0, declarationDataToReserveTSGoodsList.Count());
			AssertEquals("empty string is returned since there are no documents in the declaration", ZString.Empty, messageReturned);

			var invoiceLine1 = (JobComInvoiceLine)entryLine1.InvoiceLines.AddNew();
			var vehicle11 = invoiceLine1.Vehicles.AddNew();
			var vehicle12 = invoiceLine1.Vehicles.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_JZ = invoiceHeader1.PK;
			invoiceLine1.JI_CEI = entryInstruction.PK;
			var supportingDoc1 = invoiceLine1.SupportingDocuments.AddNew();
			supportingDoc1.CSI_ReferenceNumber = "Reference";
			supportingDoc1.CSI_Code = "1217";
			supportingDoc1.CSI_PackType = "CT";
			supportingDoc1.CSI_PackQty = 90;
			invoiceLine1.JI_Weight = 200.4455m;
			invoiceLine1.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			vehicle11.CVH_VehicleIdentificationNumber = "VIN1";
			vehicle12.CVH_VehicleIdentificationNumber = "VINX";

			var invoiceLine2 = (JobComInvoiceLine)entryLine1.InvoiceLines.AddNew();
			var vehicle2 = invoiceLine2.Vehicles.AddNew();
			invoiceLine2.JI_CL = entryLine1.PK;
			invoiceLine2.JI_JZ = invoiceHeader1.PK;
			invoiceLine2.JI_CEI = entryInstruction.PK;
			var supportingDoc2 = invoiceLine2.SupportingDocuments.AddNew();
			supportingDoc2.CSI_ReferenceNumber = "Reference";
			supportingDoc2.CSI_Code = "1217";
			supportingDoc2.CSI_PackType = "CT";
			supportingDoc2.CSI_PackQty = 90;
			invoiceLine2.JI_Weight = 2000.4455m;
			invoiceLine2.JI_WeightUQ = Core.Constants.Weight.Grams;
			vehicle2.CVH_VehicleIdentificationNumber = "VIN2";

			var invoiceLine3 = (JobComInvoiceLine)entryLine1.InvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine1.PK;
			invoiceLine3.JI_JZ = invoiceHeader1.PK;
			invoiceLine3.JI_CEI = entryInstruction.PK;
			var supportingDoc3 = invoiceLine3.SupportingDocuments.AddNew();
			supportingDoc3.CSI_ReferenceNumber = "Reference";
			supportingDoc3.CSI_Code = "BBB";
			invoiceLine3.JI_Weight = 100.4455m;
			invoiceLine3.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			var packageInfo1 = Factory.New<BasePackage>();
			packageInfo1.CW_PackType = "CT";
			packageInfo1.CW_MarksAndNos = "marks";
			var pack1 = Factory.New<InvoiceLinePackagePivot>();
			pack1.CHC_CW = packageInfo1.PK;
			pack1.CHC_NumberOfPacks = 9;
			invoiceLine3.PackagesPivot.Add(pack1);

			var packageInfo2 = Factory.New<BasePackage>();
			packageInfo2.CW_PackType = "NE";
			packageInfo2.CW_MarksAndNos = "marks2";
			var pack2 = Factory.New<InvoiceLinePackagePivot>();
			pack2.CHC_CW = packageInfo2.PK;
			pack2.CHC_NumberOfPacks = 4;
			invoiceLine3.PackagesPivot.Add(pack2);

			var pack3 = Factory.New<InvoiceLinePackagePivot>();
			var packageInfo3 = Factory.New<BasePackage>();
			packageInfo3.CW_PackType = "VG";
			packageInfo3.CW_MarksAndNos = "bulk gas marks";
			pack3.CHC_CW = packageInfo3.PK;
			pack3.CHC_NumberOfPacks = 2;
			invoiceLine3.PackagesPivot.Add(pack3);

			var invoiceLine4 = (JobComInvoiceLine)entryLine2.InvoiceLines.AddNew();
			invoiceLine4.JI_CL = entryLine2.PK;
			invoiceLine4.JI_JZ = invoiceHeader1.PK;
			invoiceLine4.JI_CEI = entryInstruction.PK;
			var supportingDoc4 = invoiceLine4.SupportingDocuments.AddNew();
			supportingDoc4.CSI_ReferenceNumber = "Reference2";
			supportingDoc4.CSI_Code = "1217";
			supportingDoc4.CSI_PackType = "CT";
			supportingDoc4.CSI_PackQty = 90;
			invoiceLine4.JI_Weight = 0.9886m;
			invoiceLine4.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			var packageInfo4 = Factory.New<BasePackage>();
			packageInfo4.CW_PackType = "CT";
			packageInfo4.CW_MarksAndNos = "marks4";
			var pack4 = Factory.New<InvoiceLinePackagePivot>();
			pack4.CHC_CW = packageInfo4.PK;
			pack4.CHC_NumberOfPacks = 8;
			invoiceLine4.PackagesPivot.Add(pack4);

			var invoiceLine5 = (JobComInvoiceLine)entryLine3.InvoiceLines.AddNew();
			invoiceLine5.JI_CL = entryLine3.PK;
			invoiceLine5.JI_JZ = invoiceHeader2.PK;
			invoiceLine5.JI_CEI = entryInstruction.PK;
			var supportingDoc5 = invoiceLine5.SupportingDocuments.AddNew();
			supportingDoc5.CSI_ReferenceNumber = "Reference3";
			supportingDoc5.CSI_Code = "1217";
			supportingDoc5.CSI_PackType = "BX";
			supportingDoc5.CSI_PackQty = 90;
			invoiceLine5.JI_Weight = 300.4455m;
			invoiceLine5.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			var packageInfo5 = Factory.New<BasePackage>();
			packageInfo5.CW_PackType = "BX";
			packageInfo5.CW_MarksAndNos = "marks5";
			var pack5 = Factory.New<InvoiceLinePackagePivot>();
			pack5.CHC_CW = packageInfo5.PK;
			pack5.CHC_NumberOfPacks = 7;
			invoiceLine5.PackagesPivot.Add(pack5);

			var invoiceLine6 = (JobComInvoiceLine)entryLine4.InvoiceLines.AddNew();
			invoiceLine6.JI_CL = entryLine4.PK;
			invoiceLine6.JI_JZ = invoiceHeader2.PK;
			invoiceLine6.JI_CEI = entryInstruction.PK;
			var supportingDoc6 = invoiceLine6.SupportingDocuments.AddNew();
			supportingDoc6.CSI_ReferenceNumber = "Reference";
			supportingDoc6.CSI_Code = "1217";
			supportingDoc6.CSI_Quantity = 10m;
			supportingDoc6.CSI_UnitOfQuantity = "KGM";
			supportingDoc6.CSI_PackType = "VG";
			supportingDoc6.CSI_PackQty = 2;
			var supportingDoc7 = invoiceLine6.SupportingDocuments.AddNew();
			supportingDoc7.CSI_ReferenceNumber = "Reference2";
			supportingDoc7.CSI_Code = "1217";
			supportingDoc7.CSI_Quantity = 20m;
			supportingDoc7.CSI_UnitOfQuantity = "KGM";
			supportingDoc7.CSI_PackType = "VG";
			supportingDoc7.CSI_PackQty = 2;
			invoiceLine6.JI_Weight = 400.4455m;
			invoiceLine6.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			var packageInfo6 = Factory.New<BasePackage>();
			packageInfo6.CW_PackType = "VG";
			packageInfo6.CW_MarksAndNos = "marks6";
			packageInfo6.CW_PackQty = 40;
			var pack6 = Factory.New<InvoiceLinePackagePivot>();
			pack6.CHC_CW = packageInfo6.PK;
			pack6.CHC_NumberOfPacks = 6;
			invoiceLine6.PackagesPivot.Add(pack6);

			var invoiceLine7 = (JobComInvoiceLine)entryLine4.InvoiceLines.AddNew();
			invoiceLine7.JI_CL = entryLine4.PK;
			invoiceLine7.JI_JZ = invoiceHeader2.PK;
			invoiceLine7.JI_CEI = entryInstruction.PK;
			var supportingDoc8 = invoiceLine7.SupportingDocuments.AddNew();
			supportingDoc8.CSI_ReferenceNumber = "Reference2";
			supportingDoc8.CSI_Code = "1217";
			supportingDoc8.CSI_Quantity = 30m;
			supportingDoc8.CSI_UnitOfQuantity = "KGM";
			supportingDoc8.CSI_PackType = "CT";
			supportingDoc8.CSI_PackQty = 90;
			var packageInfo7 = Factory.New<BasePackage>();
			packageInfo7.CW_PackType = "CT";
			packageInfo7.CW_MarksAndNos = "marks4";
			var pack7 = Factory.New<InvoiceLinePackagePivot>();
			pack7.CHC_CW = packageInfo7.PK;
			pack7.CHC_NumberOfPacks = 6;
			invoiceLine7.PackagesPivot.Add(pack7);

			var expectedPackagesForEntryLine1 = new List<(ZString type, ZInt qty, ZString marksOrVin, ZBool isBulk)>
				{
					("FR", 1, "VIN1", false),
					("FR", 1, "VINX", false),
					("FR", 1, "VIN2", false),
					("VG", 6, "marks6", true)
				};
			var expectedPackagesForEntryLine2 = new List<(ZString type, ZInt qty, ZString marksOrVin, ZBool isBulk)>
				{
					("CT", 14, "marks4", false),
					("VG", 6, "marks6", true)
				};
			var expectedPackagesForEntryLine3 = new List<(ZString type, ZInt qty, ZString marksOrVin, ZBool isBulk)> { ("BX", 7, "marks5", false) };

			(declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertEquals("declarationDataToReserveTSGoodsList has 3 elements", 3, declarationDataToReserveTSGoodsList.Count());
			AssertEquals("empty string is returned even when there are documents in the declaration", ZString.Empty, messageReturned);

			var declarationDataToReserveTSGoods1 = declarationDataToReserveTSGoodsList.First(d => d.TotalGrossWeight == 212.446446m);
			AssertDeclarationDataToReserveTSGoods("declarationDataToReserveTSGoods1", declarationDataToReserveTSGoods1, "Reference", 212.446446m, 202.446446m, expectedPackagesForEntryLine1);

			var declarationDataToReserveTSGoods2 = declarationDataToReserveTSGoodsList.First(d => d.TotalGrossWeight == 50.989m);
			AssertDeclarationDataToReserveTSGoods("declarationDataToReserveTSGoods2", declarationDataToReserveTSGoods2, "Reference2", 50.989m, 0, expectedPackagesForEntryLine2);

			var declarationDataToReserveTSGoods3 = declarationDataToReserveTSGoodsList.First(d => d.TotalGrossWeight == 300.446m);
			AssertDeclarationDataToReserveTSGoods("declarationDataToReserveTSGoods3", declarationDataToReserveTSGoods3, "Reference3", 300.446m, 0, expectedPackagesForEntryLine3);
		});

		void AssertDeclarationDataToReserveTSGoods(ZString message, DeclarationDataToReserveTSGoods declarationData, ZString expectedDocRef, ZDecimal expectedGrossWeight, ZDecimal expectedGrossWeightForVINs, List<(ZString type, ZInt qty, ZString marksOrVin, ZBool isBulk)> expectedPackages)
		{
			AssertEquals(message + " docRef", expectedDocRef, declarationData.Document.CSI_ReferenceNumber);
			AssertEquals(message + " totalGrossWeight", expectedGrossWeight, declarationData.TotalGrossWeight);
			AssertEquals(message + " totalGrossWeightForVINs", expectedGrossWeightForVINs, declarationData.TotalGrossWeightForVINs);
			AssertContainsExactElementsInAnyOrder(message + " packages", expectedPackages, declarationData.Packages);
		}
	}

	public void TestTemporaryStorageTransactionInternalReferenceNumberAndType_Import()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		CombineAssertions(() =>
		{
			var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_BGMReference = EntryReference;

			AssertEquals("TemporaryStorageTransactionInternalReferenceNumber is set to the entryReference when T2L/T2C", EntryReference, entryHeader.TemporaryStorageTransactionInternalReferenceNumber);
			AssertEquals("TemporaryStorageTransactionInternalReferenceType is empty when T2L/T2C", ZString.Empty, entryHeader.TemporaryStorageTransactionInternalReferenceType);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			AssertEquals("TemporaryStorageTransactionInternalReferenceNumber is set to the entryReference when import not H2, not T2L and not T2C", EntryReference, entryHeader.TemporaryStorageTransactionInternalReferenceNumber);
			AssertEquals("TemporaryStorageTransactionInternalReferenceType is set correctly for import not H2, not T2L and not T2C", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, entryHeader.TemporaryStorageTransactionInternalReferenceType);
		});
	}

	public void TestTemporaryStorageTransactionInternalReferenceNumberAndType_EXS()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		CombineAssertions(() =>
		{
			var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_BGMReference = EntryReference;

			AssertEquals("TemporaryStorageTransactionInternalReferenceNumber is set to the entryReference when T2L", EntryReference, entryHeader.TemporaryStorageTransactionInternalReferenceNumber);
			AssertEquals("TemporaryStorageTransactionInternalReferenceType is empty when T2L", ZString.Empty, entryHeader.TemporaryStorageTransactionInternalReferenceType);

			entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
			AssertEquals("TemporaryStorageTransactionInternalReferenceNumber is set to the entryReference when export EXS", EntryReference, entryHeader.TemporaryStorageTransactionInternalReferenceNumber);
			AssertEquals("TemporaryStorageTransactionInternalReferenceType is set correctly for export EXS", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryHeader.TemporaryStorageTransactionInternalReferenceType);
		});
	}

	public void TestTemporaryStorageTransactionInternalReferenceNumberAndType_H2()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = ZString.Empty;

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		CombineAssertions(() =>
		{
			var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_BGMReference = EntryReference;

			AssertEquals("TemporaryStorageTransactionInternalReferenceNumber is set to the entryReference when export", EntryReference, entryHeader.TemporaryStorageTransactionInternalReferenceNumber);
			AssertEquals("TemporaryStorageTransactionInternalReferenceType is set correctly when export", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, entryHeader.TemporaryStorageTransactionInternalReferenceType);

			entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
			AssertEquals("TemporaryStorageTransactionInternalReferenceNumber is set to the entryReference when export H2", EntryReference, entryHeader.TemporaryStorageTransactionInternalReferenceNumber);
			AssertEquals("TemporaryStorageTransactionInternalReferenceType is set correctly when export H2", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, entryHeader.TemporaryStorageTransactionInternalReferenceType);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("TemporaryStorageTransactionInternalReferenceNumber is set to the entryReference when import H2", EntryReference, entryHeader.TemporaryStorageTransactionInternalReferenceNumber);
			AssertEquals("TemporaryStorageTransactionInternalReferenceType is set correctly for import H2", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryHeader.TemporaryStorageTransactionInternalReferenceType);
		});
	}

	public void TestTemporaryStorageTransactionInternalReferenceNumberAndType_Export()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		CombineAssertions(() =>
		{
			var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_BGMReference = EntryReference;

			AssertEquals("TemporaryStorageTransactionInternalReferenceNumber is set to the entryReference when T2L", EntryReference, entryHeader.TemporaryStorageTransactionInternalReferenceNumber);
			AssertEquals("TemporaryStorageTransactionInternalReferenceType is empty when T2L", ZString.Empty, entryHeader.TemporaryStorageTransactionInternalReferenceType);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			AssertEquals("TemporaryStorageTransactionInternalReferenceNumber is set to the entryReference when export", EntryReference, entryHeader.TemporaryStorageTransactionInternalReferenceNumber);
			AssertEquals("TemporaryStorageTransactionInternalReferenceType is set correctly for export not EXS", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, entryHeader.TemporaryStorageTransactionInternalReferenceType);
		});
	}

	#region GetDataToReserveTemporaryStorageGoods

	public void TestGetDataToReserveTemporaryStorageGoods_WithoutSUMdoc()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, false, ZString.Empty, ZString.Empty);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithoutRegHeader()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, regHeaderReference: "reference");

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithoutPremises_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, locationInPremises: "9999000005", docReference: FormattedDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: Goods in TSD Number 99994000128 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithoutPremises_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, locationInPremises: "9999000005", regHeaderReference: DocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: Goods in TSD Number 24ES00999980001282 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithoutPremises_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, locationInPremises: "9999000005");

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: Goods in TSD Number 99994000128 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithoutRegLineItem_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, prevDocLineNo: 2, docReference: FormattedDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is no item line 2 in the Temporary Storage for TSD Number 99994000128. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithoutRegLineItem_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, prevDocLineNo: 2, regHeaderReference: DocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is no item line 2 in the Temporary Storage for TSD Number 24ES00999980001282. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithoutRegLineItem_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, prevDocLineNo: 2);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is no item line 2 in the Temporary Storage for TSD Number 99994000128. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithVINError_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, packageVin: "AAAA", transactionGrossWeight: 5m, docReference: FormattedDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 99994000128, Item 1. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithVINError_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, packageVin: "AAAA", transactionGrossWeight: 5m, regHeaderReference: DocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 24ES00999980001282, Item 1. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithVINError_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, packageVin: "AAAA", transactionGrossWeight: 5m);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 99994000128, Item 1. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, packageQtyNotBulk: 2, transactionGrossWeight: 5m, docReference: FormattedDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 9 BX. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, packageQtyNotBulk: 2, transactionGrossWeight: 5m, regHeaderReference: DocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 24ES00999980001282, Item 1: 9 BX. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, packageQtyNotBulk: 2, transactionGrossWeight: 5m);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 9 BX. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithPackageError_Bulk_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, bulkPackageTypeForRegLine: "V0", transactionGrossWeight: 5m, docReference: FormattedDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 0 VG. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithPackageError_Bulk_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, bulkPackageTypeForRegLine: "V0", transactionGrossWeight: 5m, regHeaderReference: DocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 24ES00999980001282, Item 1: 0 VG. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithPackageError_Bulk_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, bulkPackageTypeForRegLine: "V0", transactionGrossWeight: 5m);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 0 VG. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithGrossWeightError_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transactionGrossWeight: 5m, docReference: FormattedDocReference);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, CusTempStorageRegLine)>()
		{
			(22m, 1, regLine1),
			(22m, 9, regLine3),
			(22m, 0, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 99994000128, Item 1.\nRemaining Gross Weight in the Temporary Storage: 5\n\nDo you want to cancel this declaration to check?", resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithGrossWeightError_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transactionGrossWeight: 5m, regHeaderReference: DocReference);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, CusTempStorageRegLine)>()
		{
			(22m, 1, regLine1),
			(22m, 9, regLine3),
			(22m, 0, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 24ES00999980001282, Item 1.\nRemaining Gross Weight in the Temporary Storage: 5\n\nDo you want to cancel this declaration to check?", resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithGrossWeightError_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transactionGrossWeight: 5m);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, CusTempStorageRegLine)>()
		{
			(22m, 1, regLine1),
			(22m, 9, regLine3),
			(22m, 0, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 99994000128, Item 1.\nRemaining Gross Weight in the Temporary Storage: 5\n\nDo you want to cancel this declaration to check?", resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, docReference: FormattedDocReference);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, CusTempStorageRegLine)>()
		{
			(22m, 1, regLine1),
			(22m, 9, regLine3),
			(22m, 0, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, regHeaderReference: DocReference);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, CusTempStorageRegLine)>()
		{
			(22m, 1, regLine1),
			(22m, 9, regLine3),
			(22m, 0, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, CusTempStorageRegLine)>()
		{
			(22m, 1, regLine1),
			(22m, 9, regLine3),
			(22m, 0, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithMultipleDocs_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, regLine4, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transactionGrossWeight: 5m, docReference: FormattedDocReference, addExtraDoc: true, secondDocRef: FormattedSecondDocRef);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, CusTempStorageRegLine)>()
		{
			(22m, 1, regLine1),
			(22m, 9, regLine3),
			(22m, 0, regLine2),
			(20m, 0, regLine4),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty",
				"ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 99994000128, Item 1.\nRemaining Gross Weight in the Temporary Storage: 5\n\n" +
				"There might not be enough Gross Weight 20 for TSD Number 99984876543, Item 2.\nRemaining Gross Weight in the Temporary Storage: 6\n\n" +
				"Do you want to cancel this declaration to check?", resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithMultipleDocs_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, regLine4, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transactionGrossWeight: 5m, regHeaderReference: DocReference, addExtraDoc: true, secondRegHeaderReference: SecondDocRef);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, CusTempStorageRegLine)>()
		{
			(22m, 1, regLine1),
			(22m, 9, regLine3),
			(22m, 0, regLine2),
			(20m, 0, regLine4),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty",
				"ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 24ES00999980001282, Item 1.\nRemaining Gross Weight in the Temporary Storage: 5\n\n" +
				"There might not be enough Gross Weight 20 for TSD Number 24ES00999898765432, Item 2.\nRemaining Gross Weight in the Temporary Storage: 6\n\n" +
				"Do you want to cancel this declaration to check?", resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithSUMdocWithRegHeaderWithPremises_WithMultipleDocs_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, regLine4, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transactionGrossWeight: 5m, addExtraDoc: true);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, CusTempStorageRegLine)>()
		{
			(22m, 1, regLine1),
			(22m, 9, regLine3),
			(22m, 0, regLine2),
			(20m, 0, regLine4),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty",
				"ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 99994000128, Item 1.\nRemaining Gross Weight in the Temporary Storage: 5\n\n" +
				"There might not be enough Gross Weight 20 for TSD Number 99984876543, Item 2.\nRemaining Gross Weight in the Temporary Storage: 6\n\n" +
				"Do you want to cancel this declaration to check?", resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	#endregion

	#region GetDataToReserveTemporaryStorageGoods ImportDeclaration Amendment

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithoutSUMdoc()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, false, ZString.Empty, ZString.Empty);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithoutRegHeader()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, regHeaderReference: "reference");

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithoutPremises_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, locationInPremises: "9999000005", docReference: FormattedDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: Goods in TSD Number 99994000128 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithoutPremises_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, locationInPremises: "9999000005", regHeaderReference: DocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: Goods in TSD Number 24ES00999980001282 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithoutPremises_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, locationInPremises: "9999000005");

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: Goods in TSD Number 99994000128 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithoutRegLineItem_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, prevDocLineNo: 2, docReference: FormattedDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is no item line 2 in the Temporary Storage for TSD Number 99994000128. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithoutRegLineItem_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, prevDocLineNo: 2, regHeaderReference: DocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is no item line 2 in the Temporary Storage for TSD Number 24ES00999980001282. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithoutRegLineItem_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, prevDocLineNo: 2);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is no item line 2 in the Temporary Storage for TSD Number 99994000128. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithVINError_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, packageVin: "AAAA", transactionGrossWeight: 5m, isForAmendment: true, docReference: FormattedDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 99994000128, Item 1. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithVINError_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, packageVin: "AAAA", transactionGrossWeight: 5m, isForAmendment: true, regHeaderReference: DocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 24ES00999980001282, Item 1. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithVINError_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, packageVin: "AAAA", transactionGrossWeight: 5m, isForAmendment: true);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 99994000128, Item 1. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_WhenCONTransactionExists_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, _, _, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, packageQtyNotBulk: 2, transactionGrossWeight: 5m, docReference: FormattedDocReference);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
		regLineTransaction3.SRT_PackageQty = 1;

		Factory.Save();

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 9 BX. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_WhenCONTransactionExists_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, packageQtyNotBulk: 2, transactionGrossWeight: 5m, regHeaderReference: DocReference);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
		regLineTransaction3.SRT_PackageQty = 1;

		Factory.Save();

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 24ES00999980001282, Item 1: 9 BX. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_WhenCONTransactionExists_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, packageQtyNotBulk: 2, transactionGrossWeight: 5m);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
		regLineTransaction3.SRT_PackageQty = 1;

		Factory.Save();

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 9 BX. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_WhenCONTransactionDoesntExist_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, packageQtyNotBulk: 2, transactionGrossWeight: 5m, docReference: FormattedDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 9 BX. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_WhenCONTransactionDoesntExist_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, packageQtyNotBulk: 2, transactionGrossWeight: 5m, regHeaderReference: DocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 24ES00999980001282, Item 1: 9 BX. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_WhenCONTransactionDoesntExist_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, packageQtyNotBulk: 2, transactionGrossWeight: 5m);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 9 BX. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithPackageError_Bulk_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, bulkPackageTypeForRegLine: "V0", transactionGrossWeight: 5m, docReference: FormattedDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 0 VG. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithPackageError_Bulk_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, bulkPackageTypeForRegLine: "V0", transactionGrossWeight: 5m, regHeaderReference: DocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 24ES00999980001282, Item 1: 0 VG. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithPackageError_Bulk_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, bulkPackageTypeForRegLine: "V0", transactionGrossWeight: 5m);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 0 VG. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithGrossWeightError_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transactionGrossWeight: 5m, docReference: FormattedDocReference);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
		regLineTransaction3.SRT_PackageQty = 1;

		Factory.Save();

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, true, regLine3),
			(22m, 0, 10, true, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 99994000128, Item 1.\nRemaining Gross Weight in the Temporary Storage: 5\n\nDo you want to cancel this declaration to check?", resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithGrossWeightError_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transactionGrossWeight: 5m, regHeaderReference: DocReference);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
		regLineTransaction3.SRT_PackageQty = 1;

		Factory.Save();

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, true, regLine3),
			(22m, 0, 10, true, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 24ES00999980001282, Item 1.\nRemaining Gross Weight in the Temporary Storage: 5\n\nDo you want to cancel this declaration to check?", resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithGrossWeightError_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transactionGrossWeight: 5m);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
		regLineTransaction3.SRT_PackageQty = 1;

		Factory.Save();

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, true, regLine3),
			(22m, 0, 10, true, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 99994000128, Item 1.\nRemaining Gross Weight in the Temporary Storage: 5\n\nDo you want to cancel this declaration to check?", resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_GetDataToReserve_WhenCONTransactionExistsForAll_WithoutDifferences_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transactionGrossWeight: 2m, docReference: FormattedDocReference);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
		regLineTransaction3.SRT_PackageQty = 9;
		regLineTransaction3.SRT_GrossWeight = 19.8m;

		Factory.Save();

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_GetDataToReserve_WhenCONTransactionExistsForAll_WithoutDifferences_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transactionGrossWeight: 2m, regHeaderReference: DocReference);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
		regLineTransaction3.SRT_PackageQty = 9;
		regLineTransaction3.SRT_GrossWeight = 19.8m;

		Factory.Save();

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_GetDataToReserve_WhenCONTransactionExistsForAll_WithoutDifferences_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transactionGrossWeight: 2m);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
		regLineTransaction3.SRT_PackageQty = 9;
		regLineTransaction3.SRT_GrossWeight = 19.8m;

		Factory.Save();

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_GetDataToReserve_WhenCONTransactionExistsForAll_WithDifferences_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, docReference: FormattedDocReference);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
		regLineTransaction3.SRT_PackageQty = 1;

		Factory.Save();

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, true, regLine3),
			(22m, 0, 10, true, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_GetDataToReserve_WhenCONTransactionExistsForAll_WithDifferences_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, regHeaderReference: DocReference);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
		regLineTransaction3.SRT_PackageQty = 1;

		Factory.Save();

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, true, regLine3),
			(22m, 0, 10, true, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_GetDataToReserve_WhenCONTransactionExistsForAll_WithDifferences_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
		regLineTransaction3.SRT_PackageQty = 1;

		Factory.Save();

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, true, regLine3),
			(22m, 0, 10, true, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_GetDataToReserve_WhenCONTransactionDoesntExistForNonBulk_WithoutDifferences_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transactionGrossWeight: 2m, docReference: FormattedDocReference);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = "AA";
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
		regLineTransaction3.SRT_GrossWeight = 22m;

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, false, regLine3),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_GetDataToReserve_WhenCONTransactionDoesntExistForNonBulk_WithoutDifferences_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transactionGrossWeight: 2m, regHeaderReference: DocReference);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = "AA";
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
		regLineTransaction3.SRT_GrossWeight = 22m;

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, false, regLine3),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_GetDataToReserve_WhenCONTransactionDoesntExistForNonBulk_WithoutDifferences_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transactionGrossWeight: 2m);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = "AA";
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
		regLineTransaction3.SRT_GrossWeight = 22m;

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, false, regLine3),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_GetDataToReserve_WhenCONTransactionDoesntExistForNonBulk_WithDifferences_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, docReference: FormattedDocReference);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, false, regLine3),
			(22m, 0, 10, true, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_GetDataToReserve_WhenCONTransactionDoesntExistForNonBulk_WithDifferences_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, regHeaderReference: DocReference);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, false, regLine3),
			(22m, 0, 10, true, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_GetDataToReserve_WhenCONTransactionDoesntExistForNonBulk_WithDifferences_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, false, regLine3),
			(22m, 0, 10, true, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithMultipleDocs_WithoutDifferences_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, regLine4, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transactionGrossWeight: 2m, docReference: FormattedDocReference, addExtraDoc: true, transactionGrossWeightForExtraDoc: 20m, secondDocRef: FormattedSecondDocRef);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
		regLineTransaction3.SRT_PackageQty = 9;
		regLineTransaction3.SRT_GrossWeight = 19.8m;

		Factory.Save();

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithMultipleDocs_WithoutDifferences_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, regLine4, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transactionGrossWeight: 2m, regHeaderReference: DocReference, addExtraDoc: true, transactionGrossWeightForExtraDoc: 20m, secondRegHeaderReference: SecondDocRef);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
		regLineTransaction3.SRT_PackageQty = 9;
		regLineTransaction3.SRT_GrossWeight = 19.8m;

		Factory.Save();

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithMultipleDocs_WithoutDifferences_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, regLine4, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transactionGrossWeight: 2m, addExtraDoc: true, transactionGrossWeightForExtraDoc: 20m);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
		regLineTransaction3.SRT_PackageQty = 9;
		regLineTransaction3.SRT_GrossWeight = 19.8m;

		Factory.Save();

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithMultipleDocs_WithDifferences_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, regLine4, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transactionGrossWeight: 5m, docReference: FormattedDocReference, addExtraDoc: true, secondDocRef: FormattedSecondDocRef);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
		regLineTransaction3.SRT_PackageQty = 1;

		Factory.Save();

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, true, regLine3),
			(22m, 0, 10, true, regLine2),
			(20m, 0, 0, true, regLine4),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty",
				"ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 99994000128, Item 1.\nRemaining Gross Weight in the Temporary Storage: 5\n\n" +
				"There might not be enough Gross Weight 20 for TSD Number 99984876543, Item 2.\nRemaining Gross Weight in the Temporary Storage: 6\n\n" +
				"Do you want to cancel this declaration to check?", resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithMultipleDocs_WithDifferences_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, regLine4, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transactionGrossWeight: 5m, regHeaderReference: DocReference, addExtraDoc: true, secondRegHeaderReference: SecondDocRef);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
		regLineTransaction3.SRT_PackageQty = 1;

		Factory.Save();

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, true, regLine3),
			(22m, 0, 10, true, regLine2),
			(20m, 0, 0, true, regLine4),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty",
				"ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 24ES00999980001282, Item 1.\nRemaining Gross Weight in the Temporary Storage: 5\n\n" +
				"There might not be enough Gross Weight 20 for TSD Number 24ES00999898765432, Item 2.\nRemaining Gross Weight in the Temporary Storage: 6\n\n" +
				"Do you want to cancel this declaration to check?", resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithMultipleDocs_WithDifferences_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, regLine4, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transactionGrossWeight: 5m, addExtraDoc: true);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
		regLineTransaction3.SRT_PackageQty = 1;

		Factory.Save();

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, true, regLine3),
			(22m, 0, 10, true, regLine2),
			(20m, 0, 0, true, regLine4),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty",
				"ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 99994000128, Item 1.\nRemaining Gross Weight in the Temporary Storage: 5\n\n" +
				"There might not be enough Gross Weight 20 for TSD Number 99984876543, Item 2.\nRemaining Gross Weight in the Temporary Storage: 6\n\n" +
				"Do you want to cancel this declaration to check?", resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_RestoreVehicles_WithoutDifferences_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, regLineItem) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transactionGrossWeight: 2m, docReference: FormattedDocReference);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
		regLineTransaction3.SRT_PackageQty = 9;
		regLineTransaction3.SRT_GrossWeight = 19.8m;

		var regLine4 = Factory.New<CusTempStorageRegLine>();
		regLine4.SRL_LineNumber = 5;
		regLine4.SRL_CustomsStatus = "OPN";
		regLine4.SRL_PackageType = "FR";
		regLine4.SRL_PackageMarks = "AAAAAA";
		regLine4.SRL_SRH = regLine1.RegHeader.PK;
		var regLineTransaction1 = regLine4.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction1.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction1.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
		var regLineTransaction2 = regLine4.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		regLineTransaction2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction2.SRT_GrossWeight = 10;

		var regLineItemPivot4 = Factory.New<CusTempStorageRegLineItemPivot>();
		regLineItemPivot4.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

		Factory.Save();

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

			var transaction = regLine4.CusTempStorageRegLineTransactions.FirstOrDefault(t => t.SRT_TransactionStatus == CusTempStorageRegLineTransactionStatusList.Codes.Pending);
			AssertNotNull("regLine4 has new transaction", transaction);
			AssertEquals("regLine4 has new transaction with SRT_TransactionType correct", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
			AssertEquals("regLine4 has new transaction with SRT_TransactionStatus correct", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
			AssertEquals("regLine4 has new transaction with SRT_InternalReferenceNumber correct", EntryReference, transaction.SRT_InternalReferenceNumber);
			AssertEquals("regLine4 has new transaction with SRT_InternalReferenceType correct", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transaction.SRT_InternalReferenceType);
			AssertEquals("regLine4 has new transaction with SRT_PackageQty correct", 1, transaction.SRT_PackageQty);
			AssertEquals("regLine4 has new transaction with SRT_GrossWeight correct", 10m, transaction.SRT_GrossWeight);
			AssertEquals("regLine4 has new transaction with SRT_Comments correct", "Adjustment for Complementary Declaration", transaction.SRT_Comments);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_RestoreVehicles_WithoutDifferences_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, regLineItem) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transactionGrossWeight: 2m, regHeaderReference: DocReference);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
		regLineTransaction3.SRT_PackageQty = 9;
		regLineTransaction3.SRT_GrossWeight = 19.8m;

		var regLine4 = Factory.New<CusTempStorageRegLine>();
		regLine4.SRL_LineNumber = 5;
		regLine4.SRL_CustomsStatus = "OPN";
		regLine4.SRL_PackageType = "FR";
		regLine4.SRL_PackageMarks = "AAAAAA";
		regLine4.SRL_SRH = regLine1.RegHeader.PK;
		var regLineTransaction1 = regLine4.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction1.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction1.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
		var regLineTransaction2 = regLine4.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		regLineTransaction2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction2.SRT_GrossWeight = 10;

		var regLineItemPivot4 = Factory.New<CusTempStorageRegLineItemPivot>();
		regLineItemPivot4.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

		Factory.Save();

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

			var transaction = regLine4.CusTempStorageRegLineTransactions.FirstOrDefault(t => t.SRT_TransactionStatus == CusTempStorageRegLineTransactionStatusList.Codes.Pending);
			AssertNotNull("regLine4 has new transaction", transaction);
			AssertEquals("regLine4 has new transaction with SRT_TransactionType correct", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
			AssertEquals("regLine4 has new transaction with SRT_TransactionStatus correct", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
			AssertEquals("regLine4 has new transaction with SRT_InternalReferenceNumber correct", EntryReference, transaction.SRT_InternalReferenceNumber);
			AssertEquals("regLine4 has new transaction with SRT_InternalReferenceType correct", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transaction.SRT_InternalReferenceType);
			AssertEquals("regLine4 has new transaction with SRT_PackageQty correct", 1, transaction.SRT_PackageQty);
			AssertEquals("regLine4 has new transaction with SRT_GrossWeight correct", 10m, transaction.SRT_GrossWeight);
			AssertEquals("regLine4 has new transaction with SRT_Comments correct", "Adjustment for Complementary Declaration", transaction.SRT_Comments);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_RestoreVehicles_WithoutDifferences_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, regLineItem) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transactionGrossWeight: 2m);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
		regLineTransaction3.SRT_PackageQty = 9;
		regLineTransaction3.SRT_GrossWeight = 19.8m;

		var regLine4 = Factory.New<CusTempStorageRegLine>();
		regLine4.SRL_LineNumber = 5;
		regLine4.SRL_CustomsStatus = "OPN";
		regLine4.SRL_PackageType = "FR";
		regLine4.SRL_PackageMarks = "AAAAAA";
		regLine4.SRL_SRH = regLine1.RegHeader.PK;
		var regLineTransaction1 = regLine4.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction1.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction1.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
		var regLineTransaction2 = regLine4.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		regLineTransaction2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction2.SRT_GrossWeight = 10;

		var regLineItemPivot4 = Factory.New<CusTempStorageRegLineItemPivot>();
		regLineItemPivot4.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

		Factory.Save();

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

			var transaction = regLine4.CusTempStorageRegLineTransactions.FirstOrDefault(t => t.SRT_TransactionStatus == CusTempStorageRegLineTransactionStatusList.Codes.Pending);
			AssertNotNull("regLine4 has new transaction", transaction);
			AssertEquals("regLine4 has new transaction with SRT_TransactionType correct", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
			AssertEquals("regLine4 has new transaction with SRT_TransactionStatus correct", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
			AssertEquals("regLine4 has new transaction with SRT_InternalReferenceNumber correct", EntryReference, transaction.SRT_InternalReferenceNumber);
			AssertEquals("regLine4 has new transaction with SRT_InternalReferenceType correct", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transaction.SRT_InternalReferenceType);
			AssertEquals("regLine4 has new transaction with SRT_PackageQty correct", 1, transaction.SRT_PackageQty);
			AssertEquals("regLine4 has new transaction with SRT_GrossWeight correct", 10m, transaction.SRT_GrossWeight);
			AssertEquals("regLine4 has new transaction with SRT_Comments correct", "Adjustment for Complementary Declaration", transaction.SRT_Comments);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_RestoreVehicles_WithDifferences_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, regLineItem) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, docReference: FormattedDocReference);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
		regLineTransaction3.SRT_PackageQty = 1;

		var regLine4 = Factory.New<CusTempStorageRegLine>();
		regLine4.SRL_LineNumber = 5;
		regLine4.SRL_CustomsStatus = "OPN";
		regLine4.SRL_PackageType = "FR";
		regLine4.SRL_PackageMarks = "AAAAAA";
		regLine4.SRL_SRH = regLine1.RegHeader.PK;
		var regLineTransaction1 = regLine4.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction1.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction1.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
		var regLineTransaction2 = regLine4.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		regLineTransaction2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction2.SRT_GrossWeight = 10;

		var regLineItemPivot4 = Factory.New<CusTempStorageRegLineItemPivot>();
		regLineItemPivot4.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

		Factory.Save();

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, true, regLine3),
			(22m, 0, 10, true, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

			var transaction = regLine4.CusTempStorageRegLineTransactions.FirstOrDefault(t => t.SRT_TransactionStatus == CusTempStorageRegLineTransactionStatusList.Codes.Pending);
			AssertNotNull("regLine4 has new transaction", transaction);
			AssertEquals("regLine4 has new transaction with SRT_TransactionType correct", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
			AssertEquals("regLine4 has new transaction with SRT_TransactionStatus correct", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
			AssertEquals("regLine4 has new transaction with SRT_InternalReferenceNumber correct", EntryReference, transaction.SRT_InternalReferenceNumber);
			AssertEquals("regLine4 has new transaction with SRT_InternalReferenceType correct", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transaction.SRT_InternalReferenceType);
			AssertEquals("regLine4 has new transaction with SRT_PackageQty correct", 1, transaction.SRT_PackageQty);
			AssertEquals("regLine4 has new transaction with SRT_GrossWeight correct", 10m, transaction.SRT_GrossWeight);
			AssertEquals("regLine4 has new transaction with SRT_Comments correct", "Adjustment for Complementary Declaration", transaction.SRT_Comments);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_RestoreVehicles_WithDifferences_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, regLineItem) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, regHeaderReference: DocReference);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
		regLineTransaction3.SRT_PackageQty = 1;

		var regLine4 = Factory.New<CusTempStorageRegLine>();
		regLine4.SRL_LineNumber = 5;
		regLine4.SRL_CustomsStatus = "OPN";
		regLine4.SRL_PackageType = "FR";
		regLine4.SRL_PackageMarks = "AAAAAA";
		regLine4.SRL_SRH = regLine1.RegHeader.PK;
		var regLineTransaction1 = regLine4.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction1.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction1.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
		var regLineTransaction2 = regLine4.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		regLineTransaction2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction2.SRT_GrossWeight = 10;

		var regLineItemPivot4 = Factory.New<CusTempStorageRegLineItemPivot>();
		regLineItemPivot4.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

		Factory.Save();

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, true, regLine3),
			(22m, 0, 10, true, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

			var transaction = regLine4.CusTempStorageRegLineTransactions.FirstOrDefault(t => t.SRT_TransactionStatus == CusTempStorageRegLineTransactionStatusList.Codes.Pending);
			AssertNotNull("regLine4 has new transaction", transaction);
			AssertEquals("regLine4 has new transaction with SRT_TransactionType correct", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
			AssertEquals("regLine4 has new transaction with SRT_TransactionStatus correct", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
			AssertEquals("regLine4 has new transaction with SRT_InternalReferenceNumber correct", EntryReference, transaction.SRT_InternalReferenceNumber);
			AssertEquals("regLine4 has new transaction with SRT_InternalReferenceType correct", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transaction.SRT_InternalReferenceType);
			AssertEquals("regLine4 has new transaction with SRT_PackageQty correct", 1, transaction.SRT_PackageQty);
			AssertEquals("regLine4 has new transaction with SRT_GrossWeight correct", 10m, transaction.SRT_GrossWeight);
			AssertEquals("regLine4 has new transaction with SRT_Comments correct", "Adjustment for Complementary Declaration", transaction.SRT_Comments);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_ImportDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_RestoreVehicles_WithDifferences_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, regLineItem) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
		regLineTransaction3.SRT_PackageQty = 1;

		var regLine4 = Factory.New<CusTempStorageRegLine>();
		regLine4.SRL_LineNumber = 5;
		regLine4.SRL_CustomsStatus = "OPN";
		regLine4.SRL_PackageType = "FR";
		regLine4.SRL_PackageMarks = "AAAAAA";
		regLine4.SRL_SRH = regLine1.RegHeader.PK;
		var regLineTransaction1 = regLine4.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction1.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction1.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration;
		var regLineTransaction2 = regLine4.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		regLineTransaction2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction2.SRT_GrossWeight = 10;

		var regLineItemPivot4 = Factory.New<CusTempStorageRegLineItemPivot>();
		regLineItemPivot4.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

		Factory.Save();

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, true, regLine3),
			(22m, 0, 10, true, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

			var transaction = regLine4.CusTempStorageRegLineTransactions.FirstOrDefault(t => t.SRT_TransactionStatus == CusTempStorageRegLineTransactionStatusList.Codes.Pending);
			AssertNotNull("regLine4 has new transaction", transaction);
			AssertEquals("regLine4 has new transaction with SRT_TransactionType correct", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
			AssertEquals("regLine4 has new transaction with SRT_TransactionStatus correct", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
			AssertEquals("regLine4 has new transaction with SRT_InternalReferenceNumber correct", EntryReference, transaction.SRT_InternalReferenceNumber);
			AssertEquals("regLine4 has new transaction with SRT_InternalReferenceType correct", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transaction.SRT_InternalReferenceType);
			AssertEquals("regLine4 has new transaction with SRT_PackageQty correct", 1, transaction.SRT_PackageQty);
			AssertEquals("regLine4 has new transaction with SRT_GrossWeight correct", 10m, transaction.SRT_GrossWeight);
			AssertEquals("regLine4 has new transaction with SRT_Comments correct", "Adjustment for Complementary Declaration", transaction.SRT_Comments);
		});
	}

	#endregion

	#region GetDataToReserveTemporaryStorageGoods WarehouseDeclaration Compl

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithoutSUMdoc()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, false, ZString.Empty, ZString.Empty);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithoutRegHeader()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, regHeaderReference: "reference");

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithoutPremises_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, locationInPremises: "9999000005", docReference: FormattedDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: Goods in TSD Number 99994000128 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithoutPremises_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, locationInPremises: "9999000005", regHeaderReference: DocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: Goods in TSD Number 24ES00999980001282 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithoutPremises_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, locationInPremises: "9999000005");

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: Goods in TSD Number 99994000128 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithoutRegLineItem_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, prevDocLineNo: 2, docReference: FormattedDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is no item line 2 in the Temporary Storage for TSD Number 99994000128. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithoutRegLineItem_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, prevDocLineNo: 2, regHeaderReference: DocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is no item line 2 in the Temporary Storage for TSD Number 24ES00999980001282. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithoutRegLineItem_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, prevDocLineNo: 2);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is no item line 2 in the Temporary Storage for TSD Number 99994000128. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithVINError_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, packageVin: "AAAA", transactionGrossWeight: 5m, isForAmendment: true, docReference: FormattedDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 99994000128, Item 1. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithVINError_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, packageVin: "AAAA", transactionGrossWeight: 5m, isForAmendment: true, regHeaderReference: DocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 24ES00999980001282, Item 1. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithVINError_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, packageVin: "AAAA", transactionGrossWeight: 5m, isForAmendment: true);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 99994000128, Item 1. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_WhenCONTransactionExists_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, _, _, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, packageQtyNotBulk: 2, transactionGrossWeight: 5m, docReference: FormattedDocReference);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
		regLineTransaction3.SRT_PackageQty = 1;

		Factory.Save();

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 9 BX. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_WhenCONTransactionExists_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, packageQtyNotBulk: 2, transactionGrossWeight: 5m, regHeaderReference: DocReference);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
		regLineTransaction3.SRT_PackageQty = 1;

		Factory.Save();

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 24ES00999980001282, Item 1: 9 BX. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_WhenCONTransactionExists_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, packageQtyNotBulk: 2, transactionGrossWeight: 5m);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
		regLineTransaction3.SRT_PackageQty = 1;

		Factory.Save();

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 9 BX. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_WhenCONTransactionDoesntExist_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, packageQtyNotBulk: 2, transactionGrossWeight: 5m, docReference: FormattedDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 9 BX. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_WhenCONTransactionDoesntExist_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, packageQtyNotBulk: 2, transactionGrossWeight: 5m, regHeaderReference: DocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 24ES00999980001282, Item 1: 9 BX. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk_WhenCONTransactionDoesntExist_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, packageQtyNotBulk: 2, transactionGrossWeight: 5m);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 9 BX. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithPackageError_Bulk_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, bulkPackageTypeForRegLine: "V0", transactionGrossWeight: 5m, docReference: FormattedDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 0 VG. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithPackageError_Bulk_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, bulkPackageTypeForRegLine: "V0", transactionGrossWeight: 5m, regHeaderReference: DocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 24ES00999980001282, Item 1: 0 VG. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithPackageError_Bulk_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, bulkPackageTypeForRegLine: "V0", transactionGrossWeight: 5m);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 0 VG. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithGrossWeightError_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, transactionGrossWeight: 5m, docReference: FormattedDocReference);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
		regLineTransaction3.SRT_PackageQty = 1;

		Factory.Save();

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, true, regLine3),
			(22m, 0, 10, true, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 99994000128, Item 1.\nRemaining Gross Weight in the Temporary Storage: 5\n\nDo you want to cancel this declaration to check?", resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithGrossWeightError_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, transactionGrossWeight: 5m, regHeaderReference: DocReference);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
		regLineTransaction3.SRT_PackageQty = 1;

		Factory.Save();

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, true, regLine3),
			(22m, 0, 10, true, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 24ES00999980001282, Item 1.\nRemaining Gross Weight in the Temporary Storage: 5\n\nDo you want to cancel this declaration to check?", resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithGrossWeightError_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, transactionGrossWeight: 5m);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
		regLineTransaction3.SRT_PackageQty = 1;

		Factory.Save();

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, true, regLine3),
			(22m, 0, 10, true, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 99994000128, Item 1.\nRemaining Gross Weight in the Temporary Storage: 5\n\nDo you want to cancel this declaration to check?", resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_GetDataToReserve_WhenCONTransactionExistsForAll_WithoutDifferences_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, transactionGrossWeight: 2m, docReference: FormattedDocReference);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
		regLineTransaction3.SRT_PackageQty = 9;
		regLineTransaction3.SRT_GrossWeight = 19.8m;

		Factory.Save();

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_GetDataToReserve_WhenCONTransactionExistsForAll_WithoutDifferences_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, transactionGrossWeight: 2m, regHeaderReference: DocReference);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
		regLineTransaction3.SRT_PackageQty = 9;
		regLineTransaction3.SRT_GrossWeight = 19.8m;

		Factory.Save();

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_GetDataToReserve_WhenCONTransactionExistsForAll_WithoutDifferences_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, transactionGrossWeight: 2m);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
		regLineTransaction3.SRT_PackageQty = 9;
		regLineTransaction3.SRT_GrossWeight = 19.8m;

		Factory.Save();

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_GetDataToReserve_WhenCONTransactionExistsForAll_WithDifferences_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, docReference: FormattedDocReference);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
		regLineTransaction3.SRT_PackageQty = 1;

		Factory.Save();

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, true, regLine3),
			(22m, 0, 10, true, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_GetDataToReserve_WhenCONTransactionExistsForAll_WithDifferences_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, regHeaderReference: DocReference);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
		regLineTransaction3.SRT_PackageQty = 1;

		Factory.Save();

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, true, regLine3),
			(22m, 0, 10, true, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_GetDataToReserve_WhenCONTransactionExistsForAll_WithDifferences_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
		regLineTransaction3.SRT_PackageQty = 1;

		Factory.Save();

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, true, regLine3),
			(22m, 0, 10, true, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_GetDataToReserve_WhenCONTransactionDoesntExistForNonBulk_WithoutDifferences_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, transactionGrossWeight: 2m, docReference: FormattedDocReference);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = "AA";
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
		regLineTransaction3.SRT_GrossWeight = 22m;

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, false, regLine3),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_GetDataToReserve_WhenCONTransactionDoesntExistForNonBulk_WithoutDifferences_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, transactionGrossWeight: 2m, regHeaderReference: DocReference);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = "AA";
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
		regLineTransaction3.SRT_GrossWeight = 22m;

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, false, regLine3),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_GetDataToReserve_WhenCONTransactionDoesntExistForNonBulk_WithoutDifferences_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, transactionGrossWeight: 2m);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = "AA";
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
		regLineTransaction3.SRT_GrossWeight = 22m;

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, false, regLine3),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_GetDataToReserve_WhenCONTransactionDoesntExistForNonBulk_WithDifferences_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, docReference: FormattedDocReference);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, false, regLine3),
			(22m, 0, 10, true, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_GetDataToReserve_WhenCONTransactionDoesntExistForNonBulk_WithDifferences_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, regHeaderReference: DocReference);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, false, regLine3),
			(22m, 0, 10, true, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_GetDataToReserve_WhenCONTransactionDoesntExistForNonBulk_WithDifferences_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, false, regLine3),
			(22m, 0, 10, true, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithMultipleDocs_WithoutDifferences_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, regLine4, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, transactionGrossWeight: 2m, docReference: FormattedDocReference, addExtraDoc: true, transactionGrossWeightForExtraDoc: 20m, secondDocRef: FormattedSecondDocRef);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
		regLineTransaction3.SRT_PackageQty = 9;
		regLineTransaction3.SRT_GrossWeight = 19.8m;

		Factory.Save();

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithMultipleDocs_WithoutDifferences_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, regLine4, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, transactionGrossWeight: 2m, regHeaderReference: DocReference, addExtraDoc: true, transactionGrossWeightForExtraDoc: 20m, secondRegHeaderReference: SecondDocRef);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
		regLineTransaction3.SRT_PackageQty = 9;
		regLineTransaction3.SRT_GrossWeight = 19.8m;

		Factory.Save();

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithMultipleDocs_WithoutDifferences_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, regLine4, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, transactionGrossWeight: 2m, addExtraDoc: true, transactionGrossWeightForExtraDoc: 20m);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
		regLineTransaction3.SRT_PackageQty = 9;
		regLineTransaction3.SRT_GrossWeight = 19.8m;

		Factory.Save();

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithMultipleDocs_WithDifferences_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, regLine4, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, transactionGrossWeight: 5m, docReference: FormattedDocReference, addExtraDoc: true, secondDocRef: FormattedSecondDocRef);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
		regLineTransaction3.SRT_PackageQty = 1;

		Factory.Save();

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, true, regLine3),
			(22m, 0, 10, true, regLine2),
			(20m, 0, 0, true, regLine4),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty",
				"ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 99994000128, Item 1.\nRemaining Gross Weight in the Temporary Storage: 5\n\n" +
				"There might not be enough Gross Weight 20 for TSD Number 99984876543, Item 2.\nRemaining Gross Weight in the Temporary Storage: 6\n\n" +
				"Do you want to cancel this declaration to check?", resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithMultipleDocs_WithDifferences_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, regLine4, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, transactionGrossWeight: 5m, regHeaderReference: DocReference, addExtraDoc: true, secondRegHeaderReference: SecondDocRef);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
		regLineTransaction3.SRT_PackageQty = 1;

		Factory.Save();

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, true, regLine3),
			(22m, 0, 10, true, regLine2),
			(20m, 0, 0, true, regLine4),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty",
				"ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 24ES00999980001282, Item 1.\nRemaining Gross Weight in the Temporary Storage: 5\n\n" +
				"There might not be enough Gross Weight 20 for TSD Number 24ES00999898765432, Item 2.\nRemaining Gross Weight in the Temporary Storage: 6\n\n" +
				"Do you want to cancel this declaration to check?", resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_WithMultipleDocs_WithDifferences_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, regLine4, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, transactionGrossWeight: 5m, addExtraDoc: true);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
		regLineTransaction3.SRT_PackageQty = 1;

		Factory.Save();

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, true, regLine3),
			(22m, 0, 10, true, regLine2),
			(20m, 0, 0, true, regLine4),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty",
				"ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 99994000128, Item 1.\nRemaining Gross Weight in the Temporary Storage: 5\n\n" +
				"There might not be enough Gross Weight 20 for TSD Number 99984876543, Item 2.\nRemaining Gross Weight in the Temporary Storage: 6\n\n" +
				"Do you want to cancel this declaration to check?", resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_RestoreVehicles_WithoutDifferences_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, regLineItem) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, transactionGrossWeight: 2m, docReference: FormattedDocReference);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
		regLineTransaction3.SRT_PackageQty = 9;
		regLineTransaction3.SRT_GrossWeight = 19.8m;

		var regLine4 = Factory.New<CusTempStorageRegLine>();
		regLine4.SRL_LineNumber = 5;
		regLine4.SRL_CustomsStatus = "OPN";
		regLine4.SRL_PackageType = "FR";
		regLine4.SRL_PackageMarks = "AAAAAA";
		regLine4.SRL_SRH = regLine1.RegHeader.PK;
		var regLineTransaction1 = regLine4.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction1.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction1.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
		var regLineTransaction2 = regLine4.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		regLineTransaction2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction2.SRT_GrossWeight = 10;

		var regLineItemPivot4 = Factory.New<CusTempStorageRegLineItemPivot>();
		regLineItemPivot4.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

		Factory.Save();

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

			var transaction = regLine4.CusTempStorageRegLineTransactions.FirstOrDefault(t => t.SRT_TransactionStatus == CusTempStorageRegLineTransactionStatusList.Codes.Pending);
			AssertNotNull("regLine4 has new transaction", transaction);
			AssertEquals("regLine4 has new transaction with SRT_TransactionType correct", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
			AssertEquals("regLine4 has new transaction with SRT_TransactionStatus correct", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
			AssertEquals("regLine4 has new transaction with SRT_InternalReferenceNumber correct", EntryReference, transaction.SRT_InternalReferenceNumber);
			AssertEquals("regLine4 has new transaction with SRT_InternalReferenceType correct", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, transaction.SRT_InternalReferenceType);
			AssertEquals("regLine4 has new transaction with SRT_PackageQty correct", 1, transaction.SRT_PackageQty);
			AssertEquals("regLine4 has new transaction with SRT_GrossWeight correct", 10m, transaction.SRT_GrossWeight);
			AssertEquals("regLine4 has new transaction with SRT_Comments correct", "Adjustment for Complementary Declaration", transaction.SRT_Comments);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_RestoreVehicles_WithoutDifferences_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, regLineItem) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, transactionGrossWeight: 2m, regHeaderReference: DocReference);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
		regLineTransaction3.SRT_PackageQty = 9;
		regLineTransaction3.SRT_GrossWeight = 19.8m;

		var regLine4 = Factory.New<CusTempStorageRegLine>();
		regLine4.SRL_LineNumber = 5;
		regLine4.SRL_CustomsStatus = "OPN";
		regLine4.SRL_PackageType = "FR";
		regLine4.SRL_PackageMarks = "AAAAAA";
		regLine4.SRL_SRH = regLine1.RegHeader.PK;
		var regLineTransaction1 = regLine4.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction1.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction1.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
		var regLineTransaction2 = regLine4.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		regLineTransaction2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction2.SRT_GrossWeight = 10;

		var regLineItemPivot4 = Factory.New<CusTempStorageRegLineItemPivot>();
		regLineItemPivot4.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

		Factory.Save();

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

			var transaction = regLine4.CusTempStorageRegLineTransactions.FirstOrDefault(t => t.SRT_TransactionStatus == CusTempStorageRegLineTransactionStatusList.Codes.Pending);
			AssertNotNull("regLine4 has new transaction", transaction);
			AssertEquals("regLine4 has new transaction with SRT_TransactionType correct", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
			AssertEquals("regLine4 has new transaction with SRT_TransactionStatus correct", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
			AssertEquals("regLine4 has new transaction with SRT_InternalReferenceNumber correct", EntryReference, transaction.SRT_InternalReferenceNumber);
			AssertEquals("regLine4 has new transaction with SRT_InternalReferenceType correct", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, transaction.SRT_InternalReferenceType);
			AssertEquals("regLine4 has new transaction with SRT_PackageQty correct", 1, transaction.SRT_PackageQty);
			AssertEquals("regLine4 has new transaction with SRT_GrossWeight correct", 10m, transaction.SRT_GrossWeight);
			AssertEquals("regLine4 has new transaction with SRT_Comments correct", "Adjustment for Complementary Declaration", transaction.SRT_Comments);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_RestoreVehicles_WithoutDifferences_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, regLineItem) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, transactionGrossWeight: 2m);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
		regLineTransaction3.SRT_PackageQty = 9;
		regLineTransaction3.SRT_GrossWeight = 19.8m;

		var regLine4 = Factory.New<CusTempStorageRegLine>();
		regLine4.SRL_LineNumber = 5;
		regLine4.SRL_CustomsStatus = "OPN";
		regLine4.SRL_PackageType = "FR";
		regLine4.SRL_PackageMarks = "AAAAAA";
		regLine4.SRL_SRH = regLine1.RegHeader.PK;
		var regLineTransaction1 = regLine4.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction1.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction1.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
		var regLineTransaction2 = regLine4.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		regLineTransaction2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction2.SRT_GrossWeight = 10;

		var regLineItemPivot4 = Factory.New<CusTempStorageRegLineItemPivot>();
		regLineItemPivot4.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

		Factory.Save();

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

			var transaction = regLine4.CusTempStorageRegLineTransactions.FirstOrDefault(t => t.SRT_TransactionStatus == CusTempStorageRegLineTransactionStatusList.Codes.Pending);
			AssertNotNull("regLine4 has new transaction", transaction);
			AssertEquals("regLine4 has new transaction with SRT_TransactionType correct", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
			AssertEquals("regLine4 has new transaction with SRT_TransactionStatus correct", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
			AssertEquals("regLine4 has new transaction with SRT_InternalReferenceNumber correct", EntryReference, transaction.SRT_InternalReferenceNumber);
			AssertEquals("regLine4 has new transaction with SRT_InternalReferenceType correct", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, transaction.SRT_InternalReferenceType);
			AssertEquals("regLine4 has new transaction with SRT_PackageQty correct", 1, transaction.SRT_PackageQty);
			AssertEquals("regLine4 has new transaction with SRT_GrossWeight correct", 10m, transaction.SRT_GrossWeight);
			AssertEquals("regLine4 has new transaction with SRT_Comments correct", "Adjustment for Complementary Declaration", transaction.SRT_Comments);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_RestoreVehicles_WithDifferences_DocRefShorterThan18()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, regLineItem) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, docReference: FormattedDocReference);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
		regLineTransaction3.SRT_PackageQty = 1;

		var regLine4 = Factory.New<CusTempStorageRegLine>();
		regLine4.SRL_LineNumber = 5;
		regLine4.SRL_CustomsStatus = "OPN";
		regLine4.SRL_PackageType = "FR";
		regLine4.SRL_PackageMarks = "AAAAAA";
		regLine4.SRL_SRH = regLine1.RegHeader.PK;
		var regLineTransaction1 = regLine4.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction1.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction1.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
		var regLineTransaction2 = regLine4.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		regLineTransaction2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction2.SRT_GrossWeight = 10;

		var regLineItemPivot4 = Factory.New<CusTempStorageRegLineItemPivot>();
		regLineItemPivot4.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

		Factory.Save();

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, true, regLine3),
			(22m, 0, 10, true, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

			var transaction = regLine4.CusTempStorageRegLineTransactions.FirstOrDefault(t => t.SRT_TransactionStatus == CusTempStorageRegLineTransactionStatusList.Codes.Pending);
			AssertNotNull("regLine4 has new transaction", transaction);
			AssertEquals("regLine4 has new transaction with SRT_TransactionType correct", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
			AssertEquals("regLine4 has new transaction with SRT_TransactionStatus correct", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
			AssertEquals("regLine4 has new transaction with SRT_InternalReferenceNumber correct", EntryReference, transaction.SRT_InternalReferenceNumber);
			AssertEquals("regLine4 has new transaction with SRT_InternalReferenceType correct", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, transaction.SRT_InternalReferenceType);
			AssertEquals("regLine4 has new transaction with SRT_PackageQty correct", 1, transaction.SRT_PackageQty);
			AssertEquals("regLine4 has new transaction with SRT_GrossWeight correct", 10m, transaction.SRT_GrossWeight);
			AssertEquals("regLine4 has new transaction with SRT_Comments correct", "Adjustment for Complementary Declaration", transaction.SRT_Comments);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_RestoreVehicles_WithDifferences_DocRefLongerThan18_WithoutFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, regLineItem) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, regHeaderReference: DocReference);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
		regLineTransaction3.SRT_PackageQty = 1;

		var regLine4 = Factory.New<CusTempStorageRegLine>();
		regLine4.SRL_LineNumber = 5;
		regLine4.SRL_CustomsStatus = "OPN";
		regLine4.SRL_PackageType = "FR";
		regLine4.SRL_PackageMarks = "AAAAAA";
		regLine4.SRL_SRH = regLine1.RegHeader.PK;
		var regLineTransaction1 = regLine4.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction1.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction1.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
		var regLineTransaction2 = regLine4.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		regLineTransaction2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction2.SRT_GrossWeight = 10;

		var regLineItemPivot4 = Factory.New<CusTempStorageRegLineItemPivot>();
		regLineItemPivot4.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

		Factory.Save();

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, true, regLine3),
			(22m, 0, 10, true, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

			var transaction = regLine4.CusTempStorageRegLineTransactions.FirstOrDefault(t => t.SRT_TransactionStatus == CusTempStorageRegLineTransactionStatusList.Codes.Pending);
			AssertNotNull("regLine4 has new transaction", transaction);
			AssertEquals("regLine4 has new transaction with SRT_TransactionType correct", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
			AssertEquals("regLine4 has new transaction with SRT_TransactionStatus correct", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
			AssertEquals("regLine4 has new transaction with SRT_InternalReferenceNumber correct", EntryReference, transaction.SRT_InternalReferenceNumber);
			AssertEquals("regLine4 has new transaction with SRT_InternalReferenceType correct", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, transaction.SRT_InternalReferenceType);
			AssertEquals("regLine4 has new transaction with SRT_PackageQty correct", 1, transaction.SRT_PackageQty);
			AssertEquals("regLine4 has new transaction with SRT_GrossWeight correct", 10m, transaction.SRT_GrossWeight);
			AssertEquals("regLine4 has new transaction with SRT_Comments correct", "Adjustment for Complementary Declaration", transaction.SRT_Comments);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WarehouseDeclaration_IsAmendment_WithSUMdocWithRegHeaderWithPremises_RestoreVehicles_WithDifferences_DocRefLongerThan18_WithFormatting()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, regLineItem) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);

		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
		regLineTransaction3.SRT_PackageQty = 1;

		var regLine4 = Factory.New<CusTempStorageRegLine>();
		regLine4.SRL_LineNumber = 5;
		regLine4.SRL_CustomsStatus = "OPN";
		regLine4.SRL_PackageType = "FR";
		regLine4.SRL_PackageMarks = "AAAAAA";
		regLine4.SRL_SRH = regLine1.RegHeader.PK;
		var regLineTransaction1 = regLine4.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction1.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction1.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration;
		var regLineTransaction2 = regLine4.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		regLineTransaction2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction2.SRT_GrossWeight = 10;

		var regLineItemPivot4 = Factory.New<CusTempStorageRegLineItemPivot>();
		regLineItemPivot4.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot4.SRV_SRL_Line = regLine4.PK;

		Factory.Save();

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, ZInt, ZBool, CusTempStorageRegLine)>()
		{
			(22m, 9, 10, true, regLine3),
			(22m, 0, 10, true, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, prevDocCode, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, d.TotalEntryPackQty, d.IsAdjustment, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

			var transaction = regLine4.CusTempStorageRegLineTransactions.FirstOrDefault(t => t.SRT_TransactionStatus == CusTempStorageRegLineTransactionStatusList.Codes.Pending);
			AssertNotNull("regLine4 has new transaction", transaction);
			AssertEquals("regLine4 has new transaction with SRT_TransactionType correct", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
			AssertEquals("regLine4 has new transaction with SRT_TransactionStatus correct", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
			AssertEquals("regLine4 has new transaction with SRT_InternalReferenceNumber correct", EntryReference, transaction.SRT_InternalReferenceNumber);
			AssertEquals("regLine4 has new transaction with SRT_InternalReferenceType correct", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, transaction.SRT_InternalReferenceType);
			AssertEquals("regLine4 has new transaction with SRT_PackageQty correct", 1, transaction.SRT_PackageQty);
			AssertEquals("regLine4 has new transaction with SRT_GrossWeight correct", 10m, transaction.SRT_GrossWeight);
			AssertEquals("regLine4 has new transaction with SRT_Comments correct", "Adjustment for Complementary Declaration", transaction.SRT_Comments);
		});
	}

	#endregion

	#region GetDataToReserveTemporaryStorageGoods for Export

	public void TestGetDataToReserveTemporaryStorageGoods_Export_Without1217doc()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, false, ZString.Empty, ZString.Empty, messageType: JobMessageTypeList.Codes.Export);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, supDocCodeExport, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, isLAME: true);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_Export_With1217docInJobDeclaration()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, false, ZString.Empty, ZString.Empty, messageType: JobMessageTypeList.Codes.Export);

		var supDoc = entryHeader.Declaration.SupportingDocuments.AddNew();
		supDoc.CSI_Code = "1217";
		supDoc.CSI_ReferenceNumber = "Reference";

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, supDocCodeExport, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, isLAME: true);

			AssertEquals("resultMessage has error", "ES00001: For managed LAME storages, LAME Reception Certificate (document 1217) must always be declared at Invoice Line level to be able to manage the inventory properly. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_Export_With1217docInEntryInstruction()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, false, ZString.Empty, ZString.Empty, messageType: JobMessageTypeList.Codes.Export);

		var supDoc = entryHeader.EntryInstruction.SupportingDocuments.AddNew();
		supDoc.CSI_Code = "1217";
		supDoc.CSI_ReferenceNumber = "Reference";

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, supDocCodeExport, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, isLAME: true);

			AssertEquals("resultMessage has error", "ES00001: For managed LAME storages, LAME Reception Certificate (document 1217) must always be declared at Invoice Line level to be able to manage the inventory properly. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_Export_With1217docInInvoiceHeader()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, false, ZString.Empty, ZString.Empty, messageType: JobMessageTypeList.Codes.Export);

		var supDoc = entryHeader.InvoiceHeaders.FirstOrDefault().SupportingDocuments.AddNew();
		supDoc.CSI_Code = "1217";
		supDoc.CSI_ReferenceNumber = "Reference";

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, supDocCodeExport, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, isLAME: true);

			AssertEquals("resultMessage has error", "ES00001: For managed LAME storages, LAME Reception Certificate (document 1217) must always be declared at Invoice Line level to be able to manage the inventory properly. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_Export_WithMultiple1217docsWithoutQuantity()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, docCode: "1217", messageType: JobMessageTypeList.Codes.Export);

		var supDoc = entryHeader.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault().SupportingDocuments.AddNew();
		supDoc.CSI_Code = "1217";
		supDoc.CSI_ReferenceNumber = "Reference";
		supDoc.CSI_UnitOfQuantity = "KGM";

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, supDocCodeExport, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, isLAME: true);

			AssertEquals("resultMessage has error", "ES00001 / Invoice 1 / 1: For invoice lines where there is more than one LAME Certificate, it is needed to specify the corresponding Gross Weight for each certificate in column Quantity, setting UOM to KGM, so the inventory can be managed properly. Please, set that data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_Export_WithMultiple1217docsWithoutUnitOfQuantity()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, docCode: "1217", messageType: JobMessageTypeList.Codes.Export);

		var supDoc = entryHeader.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault().SupportingDocuments.AddNew();
		supDoc.CSI_Code = "1217";
		supDoc.CSI_ReferenceNumber = "Reference";
		supDoc.CSI_Quantity = 20m;
		supDoc.CSI_UnitOfQuantity = "AAA";

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, supDocCodeExport, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, isLAME: true);

			AssertEquals("resultMessage has error", "ES00001 / Invoice 1 / 1: For invoice lines where there is more than one LAME Certificate, it is needed to specify the corresponding Gross Weight for each certificate in column Quantity, setting UOM to KGM, so the inventory can be managed properly. Please, set that data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_Export_With1217docWithoutRegHeader()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: FormattedDocReference, messageType: JobMessageTypeList.Codes.Export);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, supDocCodeExport, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, isLAME: true);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_Export_With1217docWithRegHeaderWithoutPremises()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, docCode: "1217", locationInPremises: "9999000005", regHeaderReference: DocReference, messageType: JobMessageTypeList.Codes.Export);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, supDocCodeExport, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, isLAME: true);

			AssertEquals("resultMessage is not empty", "ES00001: Goods in LAME Reception Certificate 24ES00999980001282 are not stored in location 9999000002. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_Export_With1217docWithRegHeaderWithoutPremises_MistakeNumber()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, docCode: "1217", locationInPremises: "9999000005", regHeaderReference: DocReference, messageType: JobMessageTypeList.Codes.Export);
		regLineTransaction.RegLine.RegHeader.SRH_Reference = "AAAA";

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, supDocCodeExport, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, isLAME: true);

			AssertEquals("resultMessage is not empty", "ES00001: There is no record in the Temporary Storage Register for LAME Reception Certificate 24ES00999980001282. You might have mistaken the number.\n\nDo you want to cancel this declaration to check?", resultMessage);

			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_Export_With1217docWithRegHeaderWithPremises_WithMultiplePackageTypesInInvoiceLine()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, docCode: "1217", regHeaderReference: DocReference, messageType: JobMessageTypeList.Codes.Export);

		var packingGroups = entryHeader.Declaration.Bills.Cast<Bill>().FirstOrDefault().PackingGroups.Cast<BasePackingGroup>().FirstOrDefault();
		var invoiceLine = entryHeader.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault(x => x.PackagesPivot.Any());
		var packageInfo = packingGroups.Packages.AddNew();
		packageInfo.CW_PackQty = 20;
		packageInfo.CW_PackType = "CT";
		packageInfo.CW_MarksAndNos = "other marks";
		var pack = Factory.New<InvoiceLinePackagePivot>();
		pack.CHC_CW = packageInfo.PK;
		pack.CHC_NumberOfPacks = 0;
		invoiceLine.PackagesPivot.Add(pack);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, supDocCodeExport, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, isLAME: true);

			AssertEquals("resultMessage has error", "ES00001: For managed LAME storages, only one package type per Invoice Line must be entered. If more than one, please, create as many Invoice Lines as package types so the inventory can be managed properly. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_Export_With1217docWithRegHeaderWithPremises_WithoutRegLineItem()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, docCode: "1217", regHeaderReference: DocReference, regLineItemNo: 2, messageType: JobMessageTypeList.Codes.Export);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, supDocCodeExport, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, isLAME: true);

			AssertEquals("resultMessage is not empty", "ES00001: There is no item line 1 in the Temporary Storage for TSD Number 24ES00999980001282. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_Export_With1217docWithRegHeaderWithPremises_WithVINError()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, docCode: "1217", regHeaderReference: DocReference, packageVin: "AAAA", transactionGrossWeight: 5m, messageType: JobMessageTypeList.Codes.Export);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, supDocCodeExport, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, isLAME: true);

			AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the LAME under Reception Certificate Number 24ES00999980001282. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_Export_With1217docWithRegHeaderWithPremises_WithPackageError_NotBulk_WithPackagesInSupportingDocuments()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, docCode: "1217", regHeaderReference: DocReference, packageQtyNotBulk: 2, transactionGrossWeight: 5m, messageType: JobMessageTypeList.Codes.Export, packageQtyInSupportingDocuments: 3);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, supDocCodeExport, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, isLAME: true);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the LAME for Reception Certificate Number 24ES00999980001282: 3 BX. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_Export_With1217docWithRegHeaderWithPremises_WithPackageError_NotBulk()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, docCode: "1217", regHeaderReference: DocReference, packageQtyNotBulk: 2, transactionGrossWeight: 5m, messageType: JobMessageTypeList.Codes.Export);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, supDocCodeExport, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, isLAME: true);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the LAME for Reception Certificate Number 24ES00999980001282: 9 BX. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_Export_With1217docWithRegHeaderWithPremises_WithPackageError_Bulk()
	{
		var (entryHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, docCode: "1217", regHeaderReference: DocReference, bulkPackageTypeForRegLine: "V0", transactionGrossWeight: 5m, messageType: JobMessageTypeList.Codes.Export);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, supDocCodeExport, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, isLAME: true);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the LAME for Reception Certificate Number 24ES00999980001282: 0 VG. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_Export_With1217docWithRegHeaderWithPremises_WithGrossWeightError()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, docCode: "1217", regHeaderReference: DocReference, transactionGrossWeight: 5m, messageType: JobMessageTypeList.Codes.Export);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, CusTempStorageRegLine)>()
		{
			(33m, 1, regLine1),
			(33m, 9, regLine3),
			(33m, 0, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, supDocCodeExport, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, isLAME: true);

			AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 33 for Reception Certificate Number 24ES00999980001282.\nRemaining Gross Weight in the Temporary Storage: 5\n\nDo you want to cancel this declaration to check?", resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_Export_With1217docWithRegHeaderWithPremises()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, docCode: "1217", regHeaderReference: DocReference, messageType: JobMessageTypeList.Codes.Export);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, CusTempStorageRegLine)>()
		{
			(33m, 1, regLine1),
			(33m, 9, regLine3),
			(33m, 0, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, supDocCodeExport, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, isLAME: true);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_Export_With1217docWithRegHeaderWithPremises_WithMultipleDocs()
	{
		var (entryHeader, regLineTransaction, regLine1, regLine2, regLine3, regLine4, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, docCode: "1217", regHeaderReference: DocReference, transactionGrossWeight: 5m, addExtraDoc: true, secondRegHeaderReference: SecondDocRef, messageType: JobMessageTypeList.Codes.Export);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, CusTempStorageRegLine)>()
		{
			(33m, 1, regLine1),
			(33m, 9, regLine3),
			(33m, 0, regLine2),
			(19m, 0, regLine4),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, entryHeader.CH_BGMReference, supDocCodeExport, LocationInEntry, entryHeader.GetEntryLineDataDeclaredToReserveTSGoods, isLAME: true);

			AssertEquals("resultMessage is not empty",
				"ES00001:\nThere might not be enough Gross Weight 33 for Reception Certificate Number 24ES00999980001282.\nRemaining Gross Weight in the Temporary Storage: 5\n\n" +
				"There might not be enough Gross Weight 19 for Reception Certificate Number 24ES00999898765432.\nRemaining Gross Weight in the Temporary Storage: 6\n\n" +
				"Do you want to cancel this declaration to check?", resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetInvoiceLineDataDeclared_Export_WithMoreThanONE_LAMECertificate_ErrorGrossWeightInvoice()
	{
		var errorForMultipleDocsMoreThanOneLameCertificateGrossWeightInvoice = "ES00001 / Invoice 1 / 1: For invoice lines where there is more than one LAME Certificate, it is needed to specify the corresponding Gross Weight for each certificate in column Quantity, setting UOM to KGM, so the inventory can be managed properly. Please, set that data and send again.";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		var invoiceHeader1 = declaration.Invoices.AddNew();
		invoiceHeader1.JZ_InvoiceNumber = "1";
		var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		invoiceLine1.JI_Weight = 100m;
		invoiceLine1.JI_WeightUQ = "KG";

		CombineAssertions(() =>
		{
			var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_BGMReference = EntryReference;

			var doc1 = invoiceLine1.SupportingDocuments.AddNew();
			doc1.CSI_ReferenceNumber = "Reference";
			doc1.CSI_Code = "1217";
			doc1.CSI_Quantity = 10m;
			doc1.CSI_UnitOfQuantity = "KGM";
			doc1.CSI_PackType = "CT";
			doc1.CSI_PackQty = 10;

			var doc2 = invoiceLine1.SupportingDocuments.AddNew();
			doc2.CSI_ReferenceNumber = "Reference2";
			doc2.CSI_Code = "1217";
			doc2.CSI_Quantity = 10m;
			doc2.CSI_UnitOfQuantity = "KGM";
			doc2.CSI_PackType = "CT";
			doc2.CSI_PackQty = 10;

			var doc3 = invoiceLine1.SupportingDocuments.AddNew();
			doc3.CSI_ReferenceNumber = "Reference3";
			doc3.CSI_Code = "1217";
			doc3.CSI_Quantity = 0m;
			doc3.CSI_UnitOfQuantity = "KGM";
			doc3.CSI_PackType = "CT";
			doc3.CSI_PackQty = 10;

			var packageInfo3 = Factory.New<BasePackage>();
			packageInfo3.CW_PackType = "CT";
			packageInfo3.CW_MarksAndNos = "marks";
			packageInfo3.CW_PackQty = 30;
			var pack3 = Factory.New<InvoiceLinePackagePivot>();
			pack3.CHC_CW = packageInfo3.PK;
			pack3.CHC_NumberOfPacks = 9;
			invoiceLine1.PackagesPivot.Add(pack3);

			var (declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertEquals("0 documents are returned since there are documents in invoice lines but there is an error (no Quantity in at least one of them)", 0, declarationDataToReserveTSGoodsList.Count());
			AssertEquals("error is returned since there are documents in invoice line 1 but at least one of them is missing Quantity", errorForMultipleDocsMoreThanOneLameCertificateGrossWeightInvoice, messageReturned);

			doc3.CSI_Quantity = 10m;

			(declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertNotEquals("Not error is returned (any CSI_Quantity is not empty)", errorForMultipleDocsMoreThanOneLameCertificateGrossWeightInvoice, messageReturned);

			doc2.CSI_UnitOfQuantity = "AAA";

			(declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertEquals("error is returned (CSI_UnitOfQuantity not KGM)", errorForMultipleDocsMoreThanOneLameCertificateGrossWeightInvoice, messageReturned);
		});
	}

	public void TestGetInvoiceLineDataDeclared_Export_WithMoreThanONE_LAMECertificate_ErrorPackQuantityInvoice()
	{
		SetUpRefData();
		Factory.Save();

		var errorForMultipleDocsMoreThanOneLameCertificatePackQuantityInvoice = "ES00001 / Invoice 1 / 1: For invoice lines where there is more than one LAME Certificate, it is needed to specify the corresponding Pack Quantity for each certificate in column Pack Qty, setting Pack Type to the corresponding Type of packages, so the inventory can be managed properly. Please, set that data and send again.";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		var invoiceHeader1 = declaration.Invoices.AddNew();
		invoiceHeader1.JZ_InvoiceNumber = "1";
		var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		invoiceLine1.JI_Weight = 100m;
		invoiceLine1.JI_WeightUQ = "KG";

		CombineAssertions(() =>
		{
			var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_BGMReference = EntryReference;

			var doc1 = invoiceLine1.SupportingDocuments.AddNew();
			doc1.CSI_ReferenceNumber = "Reference";
			doc1.CSI_Code = "1217";
			doc1.CSI_Quantity = 10m;
			doc1.CSI_UnitOfQuantity = "KGM";
			doc1.CSI_PackType = "CT";
			doc1.CSI_PackQty = 0;

			var doc2 = invoiceLine1.SupportingDocuments.AddNew();
			doc2.CSI_ReferenceNumber = "Reference2";
			doc2.CSI_Code = "1217";
			doc2.CSI_Quantity = 10m;
			doc2.CSI_UnitOfQuantity = "KGM";
			doc2.CSI_PackType = "CT";
			doc2.CSI_PackQty = 0;

			var packageInfo = Factory.New<BasePackage>();
			packageInfo.CW_PackType = "CT";
			packageInfo.CW_MarksAndNos = "marks";
			packageInfo.CW_PackQty = 30;
			var pack1 = Factory.New<InvoiceLinePackagePivot>();
			pack1.CHC_CW = packageInfo.PK;
			pack1.CHC_NumberOfPacks = 9;
			invoiceLine1.PackagesPivot.Add(pack1);
			
			var vehicle1 = invoiceLine1.Vehicles.AddNew();
			vehicle1.CVH_VehicleIdentificationNumber = "VIN1";
			var (declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertNotEquals("Not error is returned (with vehicles) since there are documents and Pack type is not BULK and not Vehicles for each certificate in column Pack Qty = 0 Or Pack Type is different the invoice line", errorForMultipleDocsMoreThanOneLameCertificatePackQuantityInvoice, messageReturned);

			vehicle1.Delete();
			(declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertEquals("0 documents are returned since there are documents and Pack type is not BULK and not Vehicles for each certificate in column Pack Qty = 0 Or Pack Type is different the invoice line", 0, declarationDataToReserveTSGoodsList.Count());
			AssertEquals("error is returned since there are documents and Pack type is not BULK and not Vehicles for each certificate in column Pack Qty = 0 Or Pack Type is different the invoice line", errorForMultipleDocsMoreThanOneLameCertificatePackQuantityInvoice, messageReturned);

			doc1.CSI_PackQty = 10;
			doc2.CSI_PackQty = 10;
			(declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertNotEquals("Not error is returned (with CSI_PackQty)", errorForMultipleDocsMoreThanOneLameCertificatePackQuantityInvoice, messageReturned);

			doc1.CSI_PackType = "AA";
			(declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertEquals("Error is returned (with different pack type)", errorForMultipleDocsMoreThanOneLameCertificatePackQuantityInvoice, messageReturned);

			packageInfo.CW_PackType = "VG";
			(declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertNotEquals("Not error is returned (with pack type Bulk)", errorForMultipleDocsMoreThanOneLameCertificatePackQuantityInvoice, messageReturned);

			packageInfo.CW_PackType = PackageType.Frame;
			(declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertNotEquals("Not error is returned (with pack type Frame)", errorForMultipleDocsMoreThanOneLameCertificatePackQuantityInvoice, messageReturned);

			invoiceLine1.PackagesPivot.DeleteAll();
			(declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertEquals("Error is returned (with different pack type, not PackagesPivot)", errorForMultipleDocsMoreThanOneLameCertificatePackQuantityInvoice, messageReturned);
		});
	}

	public void TestGetInvoiceLineDataDeclared_Export_WithMoreThanONE_LAMECertificate_ErrorSumGrossWeightGreaterGrossWeightInvoiceLineInvoice()
	{
		var errorForMultipleDocsSumGrossWeightGreaterGrossWeightInvoiceLineInvoice = "ES00001 / Invoice 1 / 1: The SUM of Gross Weight declared in the LAME Certificates (115) is greater than the Gross Weight declared in the Invoice Line (100). Please, correct those values and send again.";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		var invoiceHeader1 = declaration.Invoices.AddNew();
		invoiceHeader1.JZ_InvoiceNumber = "1";
		var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		invoiceLine1.JI_Weight = 100m;
		invoiceLine1.JI_WeightUQ = "KG";

		CombineAssertions(() =>
		{
			var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_BGMReference = EntryReference;

			var doc1 = invoiceLine1.SupportingDocuments.AddNew();
			doc1.CSI_ReferenceNumber = "Reference";
			doc1.CSI_Code = "1217";
			doc1.CSI_Quantity = 10m;
			doc1.CSI_UnitOfQuantity = "AAA";
			doc1.CSI_PackType = "CT";
			doc1.CSI_PackQty = 10;

			var doc2 = invoiceLine1.SupportingDocuments.AddNew();
			doc2.CSI_ReferenceNumber = "Reference2";
			doc2.CSI_Code = "1217";
			doc2.CSI_Quantity = 10m;
			doc2.CSI_UnitOfQuantity = "AAA";
			doc2.CSI_PackType = "CT";
			doc2.CSI_PackQty = 10;

			var doc3 = invoiceLine1.SupportingDocuments.AddNew();
			doc3.CSI_ReferenceNumber = "Reference3";
			doc3.CSI_Code = "1217";
			doc3.CSI_UnitOfQuantity = "AAA";
			doc3.CSI_PackType = "CT";
			doc3.CSI_PackQty = 10;
			doc3.CSI_Quantity = 10m;

			var packageInfo3 = Factory.New<BasePackage>();
			packageInfo3.CW_PackType = "CT";
			packageInfo3.CW_MarksAndNos = "marks";
			packageInfo3.CW_PackQty = 30;
			var pack3 = Factory.New<InvoiceLinePackagePivot>();
			pack3.CHC_CW = packageInfo3.PK;
			pack3.CHC_NumberOfPacks = 9;
			invoiceLine1.PackagesPivot.Add(pack3);

			var (declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertNotEquals("No error returned (CSI_UnitOfQuantity not KG) since there are documents and Gross Weight for each certificate in column Quantity = 0 and UnitOfQuantity not KGM in invoice", errorForMultipleDocsSumGrossWeightGreaterGrossWeightInvoiceLineInvoice, messageReturned);

			doc1.CSI_UnitOfQuantity = "KGM";
			doc2.CSI_UnitOfQuantity = "KGM";
			doc3.CSI_UnitOfQuantity = "KGM";
			doc3.CSI_Quantity = 95m;

			(declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertEquals("0 documents are returned since there are documents and Gross Weight for each certificate in column Quantity = 0 and UnitOfQuantity not KGM in invoice", 0, declarationDataToReserveTSGoodsList.Count());
			AssertEquals("error is returned since there are documents and Gross Weight for each certificate in column Quantity = 0 and UnitOfQuantity not KGM in invoice", errorForMultipleDocsSumGrossWeightGreaterGrossWeightInvoiceLineInvoice, messageReturned);

			doc3.CSI_Quantity = 10m;
			(declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertNotEquals("No error returned (sum CSI_Quantity > GrossWeightInKG)", errorForMultipleDocsSumGrossWeightGreaterGrossWeightInvoiceLineInvoice, messageReturned);
		});
	}

	public void TestGetInvoiceLineDataDeclared_Export_WithMoreThanONE_LAMECertificate_ErrorSumPackagesGreaterPackagesInvoiceLineInvoice()
	{
		var errorForMultipleDocsSumPackagesGreaterPackagesInvoiceLineInvoice = "ES00001 / Invoice 1 / 1: The SUM of Packages declared in the LAME Certificates (22) is greater than the number of packages declared in the Invoice Line (20). Please, correct those values and send again.";
		var errorForMultipleDocsSumPackagesGreaterPackagesInvoiceLineInvoiceForVehicles = "ES00001 / Invoice 1 / 1: The SUM of Packages declared in the LAME Certificates (3) is greater than the number of packages declared in the Invoice Line (2). Please, correct those values and send again.";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		var invoiceHeader1 = declaration.Invoices.AddNew();
		invoiceHeader1.JZ_InvoiceNumber = "1";
		var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;

		CombineAssertions(() =>
		{
			var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_BGMReference = EntryReference;

			var doc1 = invoiceLine1.SupportingDocuments.AddNew();
			doc1.CSI_ReferenceNumber = "Reference";
			doc1.CSI_Code = "1217";
			doc1.CSI_Quantity = 10m;
			doc1.CSI_UnitOfQuantity = "KGM";
			doc1.CSI_PackType = "BX";
			doc1.CSI_PackQty = 10;

			var doc2 = invoiceLine1.SupportingDocuments.AddNew();
			doc2.CSI_ReferenceNumber = "Reference2";
			doc2.CSI_Code = "1217";
			doc2.CSI_Quantity = 10m;
			doc2.CSI_UnitOfQuantity = "KGM";
			doc2.CSI_PackType = "BX";
			doc2.CSI_PackQty = 12;

			invoiceLine1.JI_Weight = 100m;
			invoiceLine1.JI_WeightUQ = "KG";

			var packageInfo = Factory.New<BasePackage>();
			packageInfo.CW_PackType = "BX";
			packageInfo.CW_MarksAndNos = "marks";
			packageInfo.CW_PackQty = 20;

			var pack3 = Factory.New<InvoiceLinePackagePivot>();
			pack3.CHC_CW = packageInfo.PK;
			pack3.CHC_NumberOfPacks = 20;
			invoiceLine1.PackagesPivot.Add(pack3);

			var (declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertEquals("0 documents are returned since there are documents and Pack Quantity for each certificate", 0, declarationDataToReserveTSGoodsList.Count());
			AssertEquals("error returned (sum CSI_PackQty > sum CHC_NumberOfPacks)", errorForMultipleDocsSumPackagesGreaterPackagesInvoiceLineInvoice, messageReturned);

			packageInfo.CW_PackQty = 23;
			pack3.CHC_NumberOfPacks = 23;
			(declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertNotEquals("No error returned (sum CSI_PackQty > sum CHC_NumberOfPacks)", errorForMultipleDocsSumPackagesGreaterPackagesInvoiceLineInvoice, messageReturned);

			var vehicle11 = invoiceLine1.Vehicles.AddNew();
			vehicle11.CVH_VehicleIdentificationNumber = "VIN1";
			var vehicle12 = invoiceLine1.Vehicles.AddNew();
			vehicle12.CVH_VehicleIdentificationNumber = "VIN2";
			packageInfo.CW_PackType = PackageType.Frame;
			doc1.CSI_PackType = PackageType.Frame;
			doc1.CSI_PackQty = 2;
			doc2.CSI_PackType = PackageType.Frame;
			doc2.CSI_PackQty = 1;
			(declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertEquals("error returned (sum CSI_PackQty > sum grid Vehicles)", errorForMultipleDocsSumPackagesGreaterPackagesInvoiceLineInvoiceForVehicles, messageReturned);

			var vehicle13 = invoiceLine1.Vehicles.AddNew();
			vehicle13.CVH_VehicleIdentificationNumber = "VIN3";
			(declarationDataToReserveTSGoodsList, messageReturned) = entryHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertNotEquals("No error returned (sum CSI_PackQty > sum grid Vehicles)", errorForMultipleDocsSumPackagesGreaterPackagesInvoiceLineInvoiceForVehicles, messageReturned);
		});
	}

	#endregion

	(CusEntryHeader entryHeader, CusTempStorageRegLineTransaction regLineTransaction, CusTempStorageRegLine regLine1, CusTempStorageRegLine regLine2, CusTempStorageRegLine regLine3, CusTempStorageRegLine regLine4, CusTempStorageRegLineItem regLineItem) SetUpDataForForGetDataToReserveTemporaryStorageGoods
		(string internalReferenceType, bool shouldSetDocuments = true, string docCode = "SUM", string docReference = DocReference, int prevDocLineNo = 1, int regLineItemNo = 1,
		string locationInPremises = LocationInEntry, string regHeaderReference = FormattedDocReference, string packageVin = "VIN1", int packageQtyNotBulk = 9, decimal transactionGrossWeight = 40m,
		bool addExtraDoc = false, decimal transactionGrossWeightForExtraDoc = 6m, bool isForAmendment = false, string bulkPackageTypeForRegLine = "VG", string secondDocRef = SecondDocRef, string secondRegHeaderReference = FormattedSecondDocRef, string messageType = JobMessageTypeList.Codes.Import, bool createInTransaction1OBL = false, int packageQtyInSupportingDocuments = 0)
	{
		Factory.SetBulkTypeHelper();
		Factory.SetBulkTypeHelper("VO");

		var isExport = messageType == JobMessageTypeList.Codes.Export;

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = messageType;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
		if (internalReferenceType == CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration)
		{
			entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
		}

		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.JZ_InvoiceNumber = "1";
		var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		var vehicle1 = invoiceLine1.Vehicles.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		invoiceLine1.JI_Tariff = "111";
		invoiceLine1.JI_Weight = isExport ? 10m : 22m;
		invoiceLine1.JI_WeightUQ = Core.Constants.Weight.Kilograms;
		vehicle1.CVH_VehicleIdentificationNumber = "VIN1";

		if (shouldSetDocuments)
		{
			if(isExport)
			{
				var supDoc = invoiceLine1.SupportingDocuments.AddNew();
				supDoc.CSI_Code = docCode;
				supDoc.CSI_ReferenceNumber = docReference;
				supDoc.CSI_Quantity = 21m;
				supDoc.CSI_UnitOfQuantity = "KGM";
				if (packageQtyInSupportingDocuments != 0)
				{
					supDoc.CSI_PackQty = packageQtyInSupportingDocuments;
					supDoc.CSI_PackType = "BX";
				}
			}
			else
			{
				var previousDoc = invoiceLine1.PreviousDocuments.AddNew();
				previousDoc.CSI_Code = docCode;
				previousDoc.CSI_ReferenceNumber = docReference;
				previousDoc.CSI_LineNo = prevDocLineNo;
			}
		}

		var packingGroups = declaration.Bills.AddNew().PackingGroups.AddNew();
		var packageInfo1 = packingGroups.Packages.AddNew();
		packageInfo1.CW_PackQty = 9;
		packageInfo1.CW_PackType = "BX";
		packageInfo1.CW_MarksAndNos = "marks";
		var pack1 = Factory.New<InvoiceLinePackagePivot>();
		pack1.CHC_CW = packageInfo1.PK;
		pack1.CHC_NumberOfPacks = 9;
		invoiceLine1.PackagesPivot.Add(pack1);

		var packageInfo2 = packingGroups.Packages.AddNew();
		packageInfo2.CW_PackQty = 0;
		packageInfo2.CW_PackType = "VG";
		packageInfo2.CW_MarksAndNos = "bulk gas marks";
		var pack2 = Factory.New<InvoiceLinePackagePivot>();
		pack2.CHC_CW = packageInfo2.PK;
		pack2.CHC_NumberOfPacks = 0;
		invoiceLine1.PackagesPivot.Add(pack2);

		if (isExport)
		{
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_Tariff = "111";
			invoiceLine2.JI_Weight = 8m;
			invoiceLine2.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine1.PackagesPivot.Remove(pack1);
			invoiceLine2.PackagesPivot.Add(pack1);
			if (shouldSetDocuments)
			{
				var supDoc = invoiceLine2.SupportingDocuments.AddNew();
				supDoc.CSI_Code = docCode;
				supDoc.CSI_ReferenceNumber = docReference;
			}

			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction.PK;
			invoiceLine3.JI_Tariff = "111";
			invoiceLine3.JI_Weight = 4m;
			invoiceLine3.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine1.PackagesPivot.Remove(pack2);
			invoiceLine3.PackagesPivot.Add(pack2);
			if (shouldSetDocuments)
			{
				var supDoc = invoiceLine3.SupportingDocuments.AddNew();
				supDoc.CSI_Code = docCode;
				supDoc.CSI_ReferenceNumber = docReference;
			}
		}

		if (addExtraDoc)
		{
			var invoiceLine4 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine4.JI_CEI = entryInstruction.PK;
			invoiceLine4.JI_Tariff = "222";
			invoiceLine4.JI_Weight = 20m;
			invoiceLine4.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			if (shouldSetDocuments)
			{
				if (isExport)
				{
					var supDoc2 = invoiceLine4.SupportingDocuments.AddNew();
					supDoc2.CSI_Code = docCode;
					supDoc2.CSI_ReferenceNumber = secondDocRef;
					supDoc2.CSI_Quantity = 19m;
					supDoc2.CSI_UnitOfQuantity = "KGM";
				}
				else
				{
					var previousDoc2 = invoiceLine4.PreviousDocuments.AddNew();
					previousDoc2.CSI_Code = docCode;
					previousDoc2.CSI_ReferenceNumber = secondDocRef;
					previousDoc2.CSI_LineNo = 2;
				}
			}

			var packageInfo3 = packingGroups.Packages.AddNew();
			packageInfo3.CW_PackQty = 0;
			packageInfo3.CW_PackType = "VO";
			packageInfo3.CW_MarksAndNos = "bulk gas marks2";
			var pack3 = Factory.New<InvoiceLinePackagePivot>();
			pack3.CHC_CW = packageInfo3.PK;
			pack3.CHC_NumberOfPacks = 0;
			invoiceLine4.PackagesPivot.Add(pack3);
		}

		var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge done", true, mergeResult);
		Factory.Save();

		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.CH_BGMReference = EntryReference;

		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AAA";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		var premises = Factory.New<CusTempStorageRegPremises>();
		premises.SRP_Type = isExport ? "LAM" : "ADT";
		premises.SRP_CustomsLocation = locationInPremises;
		premises.SRP_Code = "X";
		premises.SRP_Description = "DESC";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;

		var regHeader = Factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_AppCode = "AAA";
		regHeader.SRH_Reference = regHeaderReference;
		regHeader.SRH_SRP_Premises = premises.PK;
		var regLine1 = Factory.New<CusTempStorageRegLine>();
		regLine1.SRL_LineNumber = 1;
		regLine1.SRL_CustomsStatus = "OPN";
		regLine1.SRL_PackageType = isForAmendment ? "CT" : "FR";
		regLine1.SRL_PackageMarks = packageVin;
		regLine1.SRL_SRH = regHeader.PK;
		var regLineTransactionPND = (CusTempStorageRegLineTransaction)regLine1.CusTempStorageRegLineTransactions.AddNew();
		regLineTransactionPND.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransactionPND.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
		regLineTransactionPND.SRT_InternalReferenceNumber = EntryReference;
		regLineTransactionPND.SRT_InternalReferenceType = internalReferenceType;
		var regLineTransaction1 = regLine1.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction1.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction1.SRT_InternalReferenceType = internalReferenceType;
		if (createInTransaction1OBL)
		{
			var regLineTransaction11 = regLine1.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction11.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			regLineTransaction11.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction11.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction11.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements;
			regLineTransaction11.SRT_GrossWeight = transactionGrossWeight;
		}

		var regLine2 = Factory.New<CusTempStorageRegLine>();
		regLine2.SRL_LineNumber = 3;
		regLine2.SRL_CustomsStatus = "OPN";
		regLine2.SRL_PackageType = bulkPackageTypeForRegLine;
		regLine2.SRL_SRH = regHeader.PK;
		var regLineTransaction2 = regLine2.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction2.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction2.SRT_InternalReferenceType = internalReferenceType;
		regLineTransaction2.SRT_GrossWeight = transactionGrossWeight;

		var regLine3 = Factory.New<CusTempStorageRegLine>();
		regLine3.SRL_LineNumber = 4;
		regLine3.SRL_CustomsStatus = "OPN";
		regLine3.SRL_PackageType = "BX";
		regLine3.SRL_SRH = regHeader.PK;
		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = "AAAAA";
		regLineTransaction3.SRT_InternalReferenceType = internalReferenceType;
		regLineTransaction3.SRT_PackageQty = packageQtyNotBulk;

		var regLineItem = Factory.New<CusTempStorageRegLineItem>();
		regLineItem.SRI_GoodsItemNumber = regLineItemNo;

		var regLineItemPivot1 = Factory.New<CusTempStorageRegLineItemPivot>();
		regLineItemPivot1.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot1.SRV_SRL_Line = regLine1.PK;

		var regLineItemPivot2 = Factory.New<CusTempStorageRegLineItemPivot>();
		regLineItemPivot2.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot2.SRV_SRL_Line = regLine2.PK;

		var regLineItemPivot3 = Factory.New<CusTempStorageRegLineItemPivot>();
		regLineItemPivot3.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot3.SRV_SRL_Line = regLine3.PK;

		var regLine4 = (CusTempStorageRegLine)null;
		if (addExtraDoc)
		{
			var regHeader2 = Factory.New<CusTempStorageRegHeader>();
			regHeader2.SRH_AppCode = "BBB";
			regHeader2.SRH_Reference = secondRegHeaderReference;
			regHeader2.SRH_SRP_Premises = premises.PK;

			regLine4 = Factory.New<CusTempStorageRegLine>();
			regLine4.SRL_LineNumber = 5;
			regLine4.SRL_CustomsStatus = "OPN";
			regLine4.SRL_PackageType = "VO";
			regLine4.SRL_SRH = regHeader2.PK;
			var regLineTransaction4 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction4.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction4.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction4.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction4.SRT_InternalReferenceType = internalReferenceType;
			regLineTransaction4.SRT_GrossWeight = transactionGrossWeightForExtraDoc;

			var regLineItemPivot4 = Factory.New<CusTempStorageRegLineItemPivot>();
			regLineItemPivot4.SRV_SRL_Line = regLine4.PK;
			if (isExport)
			{
				regLineItemPivot4.SRV_SRI_Item = regLineItem.PK;
			}
			else
			{
				var regLineItem2 = Factory.New<CusTempStorageRegLineItem>();
				regLineItem2.SRI_GoodsItemNumber = 2;

				regLineItemPivot4.SRV_SRI_Item = regLineItem2.PK;
			}
		}

		Factory.Save();

		return (entryHeader, regLineTransactionPND, regLine1, regLine2, regLine3, regLine4, regLineItem);
	}

	#endregion

	public void TestIsChangingToClearStatusForAccIntegration()
	{
		SetUpRefDataCustomsStatuses();

		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryHeader = Factory.New<CusEntryHeaderForTest>();
		entryHeader.CH_JE = declaration.PK;

		entryHeader.CH_EntryStatus = EntryStatusCodes.CustomsDeclarationAccepted;
		AssertEquals("CDA status should never trigger any billing.", false, entryHeader.IsChangingToClearStatusForAccIntegration);

		Factory.Save();
		entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;
		AssertEquals("Change of status to CLR  should trigger a billing.", true, entryHeader.IsChangingToClearStatusForAccIntegration);
	}

	public void TestIsStatusChangingToCleared_WithNoCustomsValuesFromRegistry()
	{
		SetUpRefDataCustomsStatuses();

		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = Factory.New<CusEntryHeaderForTest>();
		entryHeader.CH_JE = declaration.PK;

		AssertEquals(false, entryHeader.IsStatusChangingToCleared(EntryStatusCodes.CustomsDeclarationAccepted, EntryStatusCodes.ClearedWithPendingComplementaryDeclarations));
		AssertEquals(true, entryHeader.IsStatusChangingToCleared(EntryStatusCodes.CustomsDeclarationAccepted, EntryStatusCodes.Cleared));
		AssertEquals(false, entryHeader.IsStatusChangingToCleared("XYZ", EntryStatusCodes.Cleared));
	}

	public void TestIsStatusChangingToCleared_WithCustomsValuesFromRegistry()
	{
		SetUpRefDataCustomsStatuses();

		var options = CustomsDataRegistry.Instance.EnableAccountingIntegration.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
		options.EUCustomsStatusCodes = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations + ", " + EntryStatusCodes.Cleared;
		CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, options);

		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = Factory.New<CusEntryHeaderForTest>();
		entryHeader.CH_JE = declaration.PK;

		AssertEquals(false, entryHeader.IsStatusChangingToCleared(EntryStatusCodes.CustomsDeclarationAccepted, EntryStatusCodes.PreDeclarationAccepted));
		AssertEquals(true, entryHeader.IsStatusChangingToCleared(EntryStatusCodes.PreDeclarationAccepted, EntryStatusCodes.Cleared));
		AssertEquals(true, entryHeader.IsStatusChangingToCleared("XYZ", EntryStatusCodes.Cleared));
	}

	public void TestIsMrnEntryNumberTheOneWeWantToShow()
	{
		var option = new AccountingIntegrationOptions();
		option.EnableAccountingIntegration = false;
		CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, option);

		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = Factory.New<CusEntryHeaderForTest>();
		entryHeader.CH_JE = declaration.PK;
		AssertEquals("IsMrnEntryNumberTheOneWeWantToShow is false when EnableAccountingIntegration is false", false, entryHeader.IsMrnEntryNumberTheOneWeWantToShow_Exposed);

		option.EnableAccountingIntegration = true;
		CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, option);
		AssertEquals("IsMrnEntryNumberTheOneWeWantToShow is true when EnableAccountingIntegration is true", true, entryHeader.IsMrnEntryNumberTheOneWeWantToShow_Exposed);

		AssertEquals("IsMrnEntryNumberTheOneWeWantToShow is false when header has no declaration associated", false, Factory.New<CusEntryHeaderForTest>().IsMrnEntryNumberTheOneWeWantToShow_Exposed);
	}

	public void TestGetMappedSubTypeForExportPreDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.FillWithValidTestData();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.InvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);

			var entryHeader = declaration.CustomsEntryHeaders[0];

			AssertEquals("Expected D when subStyle declared A", "D", entryHeader.GetMappedSubTypeForExportPreDeclaration());

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
			AssertEquals("Expected E when subStyle declared B", "E", entryHeader.GetMappedSubTypeForExportPreDeclaration());

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
			AssertEquals("Expected F when subStyle declared C", "F", entryHeader.GetMappedSubTypeForExportPreDeclaration());

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.X;
			AssertEquals("Expected X when subStyle declared X", "X", entryHeader.GetMappedSubTypeForExportPreDeclaration());
		});
	}

	public void TestMovementReferenceNumber_Caption()
	{
		var declaration = (JobDeclaration)GetNewDeclaration();
		var entry = declaration.CustomsEntryHeaders.AddNew();

		var resStringData = Customs.Business.ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(entry.MovementReferenceNumberInfo);
		AssertEquals("MRN", resStringData.Caption);
	}

	public void TestLogStatusIfRequired()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.Logs.AddNew(AutoEvents.Transferred, new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, ShipmentTypes.HighVolumeLowValue));

		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();

		CombineAssertions(() =>
		{
			Factory.Save();
			AssertEquals("Should log if when entry is created", 1, declaration.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.ClearanceStatusChangedCode).Count());

			entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingDocuments;
			Factory.Save();
			AssertEquals("Should log if entry status is changed", 2, declaration.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.ClearanceStatusChangedCode).Count());

			entryHeader.CH_EntrySubmittedDate = ZDate.Today;
			entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingDocuments;
			Factory.Save();
			AssertEquals("Should not log if entry status is the same", 2, declaration.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.ClearanceStatusChangedCode).Count());
		});
	}

	void SetUpRefDataCustomsStatuses()
	{
		const string EsCode = CountryCodes.Spain;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");
		var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingDataGrouping(EsCode, parent: grouping);

		AddCustomsStatusCodeList(EntryStatusCodes.CustomsDeclarationAccepted, "Customs Declaration accepted");
		AddCustomsStatusCodeList(EntryStatusCodes.PreDeclarationAccepted, "Customs PDA Status");
		AddCustomsStatusCodeList(EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, "Cleared with pending complementary declarations");
		AddCustomsStatusCodeList("XYZ", "Fake Customs Status XYZ with Excecute Auto Billing Attribute", hasExcecuteAutoBillingAttribute: true);
		AddCustomsStatusCodeList(EntryStatusCodes.Cleared, "Customs Declaration Accepted and Cleared", hasExcecuteAutoBillingAttribute: true);
		Factory.Save();

		void AddCustomsStatusCodeList(string code, string description, bool hasExcecuteAutoBillingAttribute = false)
		{
			var cusCodeList = helper.CreateCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, code, description, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			if (hasExcecuteAutoBillingAttribute)
			{
				helper.CreateNewOrGetExistingRefCusCodeListAttributeName("IExecuteAutoBilling", ZString.Empty, cusCodeList.ZZD_ZZK_NKCodeType, cusCodeList.ZZD_ZZZ_NKDataGrouping);
				cusCodeList.Attributes.AddNew("IExecuteAutoBilling", ZString.Empty);
			}
		}
	}

	void CreateGuaranteeHeaderDetail(BusinessObjectFactory factory, ZString guaranteeCode, bool addOBLTransaction = true, bool withOldEndDate = false)
	{
		var holder = factory.NewWithValidTestData<OrgHeader>();

		var guaranteeHeader = factory.NewWithValidTestData<CusGuaranteeHeader>();
		guaranteeHeader.CPH_Number = guaranteeCode;
		guaranteeHeader.CPH_OH_PermitHolder = holder.PK;
		guaranteeHeader.CPH_StartDate = new ZDate(2020, 7, 15);
		guaranteeHeader.CPH_SystemCreateTimeUtc = new ZDate(2020, 7, 15);
		guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.IMP;
		guaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Spain;

		if (addOBLTransaction)
		{
			AddOBLTransaction(guaranteeHeader, 1000m);
		}

		if (withOldEndDate)
		{
			guaranteeHeader.CPH_EndDate = ZDate.Today.AddDays(-10);
		}
	}

	void AddOBLTransaction(CusGuaranteeHeader guaranteeHeader, ZDecimal value)
	{
		var transaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
		transaction.CPL_Reference = "OPENING";
		transaction.CPL_TranValue = value;
		transaction.CPL_Comment = "OPENING";
		transaction.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
	}

	void AddNewFee(CusEntryLine entryLine, string feeType, string mop, decimal amount)
	{
		var fee = entryLine.Fees.AddNew();
		fee.CF_MethodOfPayment = mop;
		fee.CF_ChargeType = feeType;
		fee.CF_ChargeAmount = amount;
	}

	protected override ZBool IgnoreTestsThatRequireEntryShouldSetUCRinBGMReferenceNumber => true;

	protected override BaseJobDeclaration GetNewDeclaration()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		return dec;
	}

	CusEntryHeader GetNewEntryHeaderFromDeclaration(JobDeclaration declaration)
	{
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		return entryHeader;
	}

	protected override BaseJobDeclaration ImportJobDeclaration
	{
		get
		{
			var declaration = base.ImportJobDeclaration;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			return declaration;
		}
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var declaration = GetNewDeclaration();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
		var entry = declaration.ActiveEntryHeaders.AddNew();
		var entryLine = entry.AllEntryLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
		return entry;
	}

	protected override IChargesCurrencyTestSetup GetChargesCurrencyTestSetup() => new CurrencyTestSetup();

	ESEDIMessage SetEDIMessageAndGenPivot(CusEntryHeader entryHeader, CusStorageDocPivot docPivot)
	{
		var message = Factory.New<ESEDIMessage>();
		entryHeader.Messages.Add(message);
		message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		message.EM_Status = EDIMessage.Status.Sent;
		var messagePivot = Factory.New<GenPivot>();
		messagePivot.XX_RelationType = GenPivotTypes.CusStorageDocPivotEdiMessage;
		messagePivot.XX_Relation1ID = docPivot.PK;
		messagePivot.XX_Relation1TableCode = docPivot.TablePrefix;
		messagePivot.XX_Relation2ID = message.PK;
		messagePivot.XX_Relation2TableCode = message.TablePrefix;

		return message;
	}

	sealed class CurrencyTestSetup : IChargesCurrencyTestSetup
	{
		void IChargesCurrencyTestSetup.SetupJobDecWithOFTAndCIFCharges(BaseJobDeclaration declaration, ZString currencyCode)
		{
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.AutoCreateChargesBasedOnIncoTerm = false;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 10000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = currencyCode;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;

			var nonDutiableCharge = invoiceHeader.Charges.AddNew();
			nonDutiableCharge.J7_ChargeType = Customs.Business.CustomsChargeTypeList.Codes.ForeignInlandFreight;
			nonDutiableCharge.J7_Amount = 200m;
			nonDutiableCharge.J7_IsDutiable = false;
			nonDutiableCharge.J7_IsIncludedInITOT = true;

			var oft = invoiceHeader.Charges.AddNew();
			oft.J7_ChargeType = Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight;
			oft.J7_Amount = 500m;
			oft.J7_RX_NKCurrency = invoiceHeader.Invoice_Currency.RX_Code;
		}

		ZDecimal IChargesCurrencyTestSetup.ExpectedFOB => 10500m;
		ZDecimal IChargesCurrencyTestSetup.ExpectedCIF => 11000m;
	}

	public override void TestShouldCompletelyReassignNumbers()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		AssertEquals(true, entryHeader.ShouldCompletelyReassignNumbers);
		entryHeader.CH_EntryStatus = MessageProcessorConstants.EntryStatusCodes.Cleared;
		AssertEquals(false, entryHeader.ShouldCompletelyReassignNumbers);
	}

	public void TestGetPreviouslySentSupportingDocuments()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "EntryNum";
		var entryLine = entryHeader.AllEntryLines.AddNew();

		var suppDoc1 = declaration.SupportingDocuments.AddNew();
		suppDoc1.CSI_Code = "X001";
		suppDoc1.CSI_ReferenceNumber = "ES3600000001";

		var suppDoc2 = Factory.New<SupportingDocument>();
		suppDoc2.CSI_Code = "X002";
		suppDoc2.CSI_ReferenceNumber = "ES360000000X";
		suppDoc2.CSI_ParentTableCode = entryLine.TablePrefix;
		suppDoc2.CSI_ParentID = entryLine.PK;

		CombineAssertions(() =>
		{
			AssertEquals("There are no entry header SupportingDocuments", 0, entryLine.Header.GetPreviouslySentSupportingDocuments().Length);

			var suppDoc = Factory.New<SupportingDocument>();
			suppDoc.CSI_Code = "X002";
			suppDoc.CSI_ReferenceNumber = "ES3600000002";
			suppDoc.CSI_ParentTableCode = entryLine.Header.TablePrefix;
			suppDoc.CSI_ParentID = entryLine.Header.PK;

			var clSupDocs = entryLine.Header.GetPreviouslySentSupportingDocuments();
			AssertEquals("There is 1 entry header SupportingDocument", 1, clSupDocs.Length);
			AssertEquals("CSI_Code is correct", "X002", clSupDocs[0].CSI_Code);
			AssertEquals("CSI_ReferenceNumber is correct", "ES3600000002", clSupDocs[0].CSI_ReferenceNumber);
		});
	}

	public void TestGetPreviouslySentAdditionalInfos()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("There are no entry header AdditionalInfo Documents", 0, entryHeader.GetPreviouslySentAdditionalInfos().Length);

			var addInfoDoc = Factory.New<AdditionalInfo>();
			addInfoDoc.CSI_Code = "X003";
			addInfoDoc.CSI_ReferenceNumber = "ES3600000003";
			addInfoDoc.CSI_SubType = "REF";
			addInfoDoc.CSI_ParentTableCode = entryHeader.TablePrefix;
			addInfoDoc.CSI_ParentID = entryHeader.PK;

			var chAddInfoDocs = entryHeader.GetPreviouslySentAdditionalInfos();
			AssertEquals("There is 1 entry header AdditionalInfo document.", 1, chAddInfoDocs.Length);
			AssertEquals("CSI_Code is correct", "X003", chAddInfoDocs[0].CSI_Code);
			AssertEquals("CSI_ReferenceNumber is correct", "ES3600000003", chAddInfoDocs[0].CSI_ReferenceNumber);
			AssertEquals("CSI_SubType is correct", "REF", chAddInfoDocs[0].CSI_SubType);
		});
	}

	const string EntryReference = "ES00001";
	const string ExpectedCommentPrefixImport = "IMPORT JOB";
	const string ExpectedCommentPrefixExport = "EXPORT JOB";
	const string DeclarationReference = "B00000001";
	const string MRNCode = "20ES00999930006184";
	const string LocationInEntry = "9999000002";
	const string DocReference = "24ES00999980001282";
	const string FormattedDocReference = "99994000128";
	const string SecondDocRef = "24ES00999898765432";
	const string FormattedSecondDocRef = "99984876543";
	readonly ZDateTime issueDate = new ZDateTime(2024, 06, 10, 11, 11, 11);
	readonly ZDateTime releaseDate = new ZDateTime(2024, 06, 14, 11, 11, 11);
	readonly ZString[] prevDocCode = ["SUM"];
	readonly ZString[] supDocCodeExport = ["1217"];

	sealed class CusEntryHeaderForTest : CusEntryHeader
	{
		public CusEntryHeaderForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		public new ZBool IsStatusChangingToCleared(ZString originalStatus, ZString newStatus) => base.IsStatusChangingToCleared(originalStatus, newStatus);

		public new ZBool IsChangingToClearStatusForAccIntegration => base.IsChangingToClearStatusForAccIntegration;

		public bool IsMrnEntryNumberTheOneWeWantToShow_Exposed => base.IsMrnEntryNumberTheOneWeWantToShow;
	}
}
