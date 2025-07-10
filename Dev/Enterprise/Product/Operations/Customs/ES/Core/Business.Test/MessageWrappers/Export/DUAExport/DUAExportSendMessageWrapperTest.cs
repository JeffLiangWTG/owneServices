using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.ES.Business.ExportMessageConstants;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants.RefCusCodeList;

namespace Enterprise.Customs.ES.Business.Testing;

[TestedType(typeof(DUAExportSendMessageWrapper))]
public class DUAExportSendMessageWrapperTest : ExportSendMessageCommonWrapperAbstractTest<DUAExportSendMessageWrapper>
{
	public void TestMessageType()
	{
		AssertEquals("MessageType is" + ExpectedMessageType, ExpectedMessageType, wrapper.MessageType);
	}

	public void TestCustomsProcedureCategory1()
	{
		declaration.JE_MessageSubType = HeaderData.MessageSubType;
		AssertEquals("Expected filled CustomsProcedureCategory1", HeaderData.MessageSubType, wrapper.CustomsProcedureCategory1);
	}

	protected virtual void AssertCustomsProcedureCategory2()
	{
		entryInstruction.CEI_SubStyle = HeaderData.EntryInstructionSubStyle;
		AssertEquals("Expected filled CustomsProcedureCategory2", HeaderData.EntryInstructionSubStyle, wrapper.CustomsProcedureCategory2);
	}

	public void TestCustomsProcedureCategory2()
	{
		AssertCustomsProcedureCategory2();
	}

	public void TestCustomsProcedureCategory3()
	{
		declaration.ZG_CTStatusID = HeaderData.CTStatus;
		AssertEquals("Expected filled CustomsProcedureCategory3", HeaderData.CTStatus, wrapper.CustomsProcedureCategory3);
	}

	public void TestCustomsProcedureCategory4()
	{
		invoiceHeader.JZ_ValuationCode = HeaderData.ValuationCode;
		AssertEquals("Expected filled CustomsProcedureCategory4", HeaderData.ValuationCode, wrapper.CustomsProcedureCategory4);
	}

	protected virtual void AssertCustomsProcedureCategory5()
	{
		declaration.JE_CustomsOffice = HeaderData.CustomsOffice;
		AssertEquals("Expected filled CustomsProcedureCategory5", HeaderData.CustomsOfficeCode, wrapper.CustomsProcedureCategory5);
	}

	public void TestCustomsProcedureCategory5()
	{
		AssertCustomsProcedureCategory5();
	}

	public void TestCountryOfExport()
	{
		declaration.JE_GoodsOrigin = HeaderData.CountryOfExport;
		AssertEquals("Expected filled CountryOfExport", HeaderData.CountryOfExport, wrapper.CountryOfExport);
	}

	public void TestCountryOfDestination()
	{
		declaration.JE_GoodsDestination = HeaderData.CountryOfDestination;
		AssertEquals("Expected filled CountryOfDestination", HeaderData.CountryOfDestination, wrapper.CountryOfDestination);
	}

	public void TestCustomsOfficeofExitCountryCode()
	{
		CombineAssertions(() =>
		{
			var officeOfExit = declaration.CustomsOffices.AddNew();
			officeOfExit.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit;
			officeOfExit.CY_Data = HeaderData.CustomsOfficeOfExit;
			wrapper = GetWrapper(entryHeader, Certificate);

			AssertEquals("Expected filled CustomsOfficeofExitCountryCode with OfficeOfExit country code", HeaderData.CustomsOfficeOfExitCountry, wrapper.CustomsOfficeofExitCountryCode);

			officeOfExit.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExport;
			officeOfExit.CY_Data = HeaderData.CustomsOfficeOfExport;
			wrapper = GetWrapper(entryHeader, Certificate);

			AssertEquals("Expected filled CustomsOfficeofExitCountryCode with OfficeOfExport country code", HeaderData.CustomsOfficeOfExportCountry, wrapper.CustomsOfficeofExitCountryCode);

			officeOfExit.CY_Data = ZString.Empty;
			declaration.JE_CustomsOffice = HeaderData.CustomsOffice;
			wrapper = GetWrapper(entryHeader, Certificate);

			AssertEquals("Expected filled CustomsOfficeofExitCountryCode with CustomsOffice country code when declared Empty OfficeOfExport", HeaderData.CustomsOfficeCountry, wrapper.CustomsOfficeofExitCountryCode);

			officeOfExit.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit;
			declaration.JE_CustomsOffice = HeaderData.CustomsOffice;
			wrapper = GetWrapper(entryHeader, Certificate);

			AssertEquals("Expected filled CustomsOfficeofExitCountryCode with CustomsOffice country code when declared Empty OfficeOfExit", HeaderData.CustomsOfficeCountry, wrapper.CustomsOfficeofExitCountryCode);

			officeOfExit.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit;
			officeOfExit.CY_Data = ZString.Empty;
			var officeOfExport = declaration.CustomsOffices.AddNew();
			officeOfExport.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExport;
			officeOfExport.CY_Data = HeaderData.CustomsOfficeOfExport;
			wrapper = GetWrapper(entryHeader, Certificate);

			AssertEquals("Expected filled CustomsOfficeofExitCountryCode with OfficeOfExport country code when declared Empty OfficeOfExit", HeaderData.CustomsOfficeOfExportCountry, wrapper.CustomsOfficeofExitCountryCode);

			declaration.CustomsOffices.RemoveAndDeleteAll();
			declaration.JE_CustomsOffice = HeaderData.CustomsOffice;
			wrapper = GetWrapper(entryHeader, Certificate);

			AssertEquals("Expected filled CustomsOfficeofExitCountryCode with CustomsOffice country code when no other customsOffice is declared", HeaderData.CustomsOfficeCountry, wrapper.CustomsOfficeofExitCountryCode);
		});
	}

	public void TestCustomsOfficeofExit()
	{
		CombineAssertions(() =>
		{
			var officeOfExit = declaration.CustomsOffices.AddNew();
			officeOfExit.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit;
			officeOfExit.CY_Data = HeaderData.CustomsOfficeOfExit;
			wrapper = GetWrapper(entryHeader, Certificate);

			AssertEquals("Expected filled CustomsOfficeofExit with OfficeOfExit office code", HeaderData.CustomsOfficeOfExitCode, wrapper.CustomsOfficeofExit);

			officeOfExit.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExport;
			officeOfExit.CY_Data = HeaderData.CustomsOfficeOfExport;
			wrapper = GetWrapper(entryHeader, Certificate);

			AssertEquals("Expected filled CustomsOfficeofExit with OfficeOfExport office code", HeaderData.CustomsOfficeOfExportCode, wrapper.CustomsOfficeofExit);

			officeOfExit.CY_Data = ZString.Empty;
			declaration.JE_CustomsOffice = HeaderData.CustomsOffice;
			wrapper = GetWrapper(entryHeader, Certificate);

			AssertEquals("Expected filled CustomsOfficeofExit with CustomsOffice office code when declared Empty OfficeOfExport", HeaderData.CustomsOfficeCodeComplete, wrapper.CustomsOfficeofExit);

			officeOfExit.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit;
			declaration.JE_CustomsOffice = HeaderData.CustomsOffice;
			wrapper = GetWrapper(entryHeader, Certificate);

			AssertEquals("Expected filled CustomsOfficeofExit with CustomsOffice office code when declared Empty OfficeOfExit", HeaderData.CustomsOfficeCodeComplete, wrapper.CustomsOfficeofExit);

			officeOfExit.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit;
			officeOfExit.CY_Data = ZString.Empty;
			var officeOfExport = declaration.CustomsOffices.AddNew();
			officeOfExport.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExport;
			officeOfExport.CY_Data = HeaderData.CustomsOfficeOfExport;
			wrapper = GetWrapper(entryHeader, Certificate);

			AssertEquals("Expected filled CustomsOfficeofExit with OfficeOfExport office code when declared Empty OfficeOfExit", HeaderData.CustomsOfficeOfExportCode, wrapper.CustomsOfficeofExit);

			declaration.CustomsOffices.RemoveAndDeleteAll();
			declaration.JE_CustomsOffice = HeaderData.CustomsOffice;
			wrapper = GetWrapper(entryHeader, Certificate);

			AssertEquals("Expected filled CustomsOfficeofExit with CustomsOffice office code when no other customsOffice is declared", HeaderData.CustomsOfficeCodeComplete, wrapper.CustomsOfficeofExit);
		});
	}

	public void TestLocationOfGoodsExamCustomsOffice()
	{
		CombineAssertions(() =>
		{
			declaration.JE_LocationOfGoods = "9999000002";
			wrapper = GetWrapper(entryHeader, Certificate);
			AssertEquals("Expected filled LocationOfGoodsExamCustomsOffice", "9999", wrapper.LocationOfGoodsExamCustomsOffice);

			declaration.JE_LocationOfGoods = ZString.Empty;
			wrapper = GetWrapper(entryHeader, Certificate);
			AssertEquals("Expected empty LocationOfGoodsExamCustomsOffice", ZString.Empty, wrapper.LocationOfGoodsExamCustomsOffice);

			declaration.JE_LocationOfGoods = "ES009999000002";
			wrapper = GetWrapper(entryHeader, Certificate);
			AssertEquals("Expected filled LocationOfGoodsExamCustomsOffice Long", "9999", wrapper.LocationOfGoodsExamCustomsOffice);
		});
	}

	public void TestLocationOfGoodsExam()
	{
		CombineAssertions(() =>
		{
			declaration.JE_LocationOfGoods = "9999000002";
			wrapper = GetWrapper(entryHeader, Certificate);
			AssertEquals("Expected filled LocationOfGoodsExam", "000002", wrapper.LocationOfGoodsExam);

			declaration.JE_LocationOfGoods = ZString.Empty;
			wrapper = GetWrapper(entryHeader, Certificate);
			AssertEquals("Expected empty LocationOfGoodsExam", ZString.Empty, wrapper.LocationOfGoodsExam);

			declaration.JE_LocationOfGoods = "ES009999000002";
			wrapper = GetWrapper(entryHeader, Certificate);
			AssertEquals("Expected filled LocationOfGoodsExam Long", "000002", wrapper.LocationOfGoodsExam);
		});
	}

	public void TestWarehouse()
	{
		var warehouse = Factory.NewWithValidTestData<OrgHeader>();
		warehouse.OH_Code = "CODE";
		warehouse.OH_IsWarehouseClient = true;
		var orgAddressWarehouse = warehouse.Addresses.AddNew();
		orgAddressWarehouse.OA_OH = warehouse.PK;
		orgAddressWarehouse.OA_Address1 = "address";
		orgAddressWarehouse.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Spain;
		var premiseID = warehouse.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, HeaderData.Warehouse, Core.Constants.CountryCodes.Spain);
		premiseID.OK_OA_PremisesAddress = orgAddressWarehouse.PK;
		declaration.WarehouseDocAddress.E2_OA_Address = orgAddressWarehouse.PK;
		entryInstruction.CEI_OA_Warehouse = orgAddressWarehouse.PK;

		AssertEquals("Expected filled Warehouse", HeaderData.Warehouse, wrapper.Warehouse);
	}

	public void TestDateOfRecap()
	{
		ZDateTime.TryParseExact(HeaderData.DateOfRecap, out var recapDate, CustomsDateTimeExtension.DateFormat);
		declaration.ZG_LCPDepart = recapDate;
		AssertEquals("Expected filled DateOfRecap", recapDate, wrapper.DateOfRecap);
	}

	public void TestGoodsInContainerIndicator()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected false GoodsInContainerIndicator", false, wrapper.GoodsInContainerIndicator);

			var containerTag = "CONTAINER";
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = containerTag;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(containerTag).IsForInvoiceLine = true;
			AssertEquals("Expected true GoodsInContainerIndicator", true, wrapper.GoodsInContainerIndicator);
		});
	}

	public void TestRMTIndicator()
	{
		declaration.ZG_IsRMTApplicable = HeaderData.RMTIndicator;
		AssertEquals("Expected true RMTIndicator", true, wrapper.RMTIndicator);
	}

	public void TestCountryCodes()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty CountryCodes", 0, wrapper.CountryCodes.Count);

			var trans1 = declaration.Transports.AddNew();
			var trans2 = declaration.Transports.AddNew();
			var trans3 = declaration.Transports.AddNew();
			var trans4 = declaration.Transports.AddNew();
			var trans5 = declaration.Transports.AddNew();
			trans1.JW_ETA = ZDate.Today.AddDays(1);
			trans2.JW_ETA = ZDate.Today.AddDays(2);
			trans3.JW_ETA = ZDate.Today.AddDays(3);
			trans4.JW_ETA = ZDate.Today.AddDays(4);
			trans5.JW_ETA = ZDate.Today.AddDays(5);
			trans1.JW_RL_NKLoadPort = "AUSYD";
			trans1.JW_RL_NKDiscPort = "INBOM";
			trans2.JW_RL_NKLoadPort = "INBOM";
			trans2.JW_RL_NKDiscPort = "TRIST";
			trans3.JW_RL_NKLoadPort = "TRIST";
			trans3.JW_RL_NKDiscPort = "DEHAM";
			trans4.JW_RL_NKLoadPort = "DEHAM";
			trans4.JW_RL_NKDiscPort = "GBLBA";
			trans5.JW_RL_NKLoadPort = "GBLBA";
			trans5.JW_RL_NKDiscPort = "ESBCN";
			declaration.JE_RL_NKOrigin = "ESMAD";
			declaration.JE_RL_NKFinalDestination = "FRPAR";

			entryInstruction.IncludeRoutingSecurityData = false;

			wrapper = GetWrapper(entryHeader, Certificate);
			AssertEquals("Expected empty CountryCodes when IncludeRoutingSecurityData is false", 0, wrapper.CountryCodes.Count);

			entryInstruction.IncludeRoutingSecurityData = true;
			wrapper = GetWrapper(entryHeader, Certificate);
			var countryCodes = wrapper.CountryCodes;
			AssertArrayEqualsByElements("Expected filled CountryCodes when IncludeRoutingSecurityData is true", CountriesOfRouting, countryCodes.ToArray());
			AssertSame("Cached CountryCodes", wrapper.CountryCodes, countryCodes);
		});
	}

	public void TestSealCodesOnlyContainersWithSeals()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty SealCodes", 0, wrapper.SealCodes.Count);

			foreach (var seal in ContainerSeals)
			{
				var ctnNumber = "CTN1" + seal;
				var container = declaration.CusContainers.AddNew();
				container.CO_ContainerNumber = ctnNumber;
				container.CO_Seal = seal;
				invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(ctnNumber).IsForInvoiceLine = true;
			}

			wrapper = GetWrapper(entryHeader, Certificate);
			var sealCodes = wrapper.SealCodes;

			AssertArrayEqualsByElements("Expected filled SealCodes", ContainerSeals, sealCodes.ToArray());
			AssertSame("Cached SealCodes", wrapper.SealCodes, sealCodes);
		});
	}

	public void TestSealCodesOnlyContainersWithSecondSeals()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty SealCodes", 0, wrapper.SealCodes.Count);

			foreach (var seal in SecondContainerSeals)
			{
				var ctnNumber = "CTN1" + seal;
				var container = declaration.CusContainers.AddNew();
				container.CO_ContainerNumber = ctnNumber;
				container.CO_SecondSeal = seal;
				invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(ctnNumber).IsForInvoiceLine = true;
			}

			wrapper = GetWrapper(entryHeader, Certificate);
			var sealCodes = wrapper.SealCodes;

			AssertArrayEqualsByElements("Expected filled SealCodes", SecondContainerSeals, sealCodes.ToArray());
			AssertSame("Cached SealCodes", wrapper.SealCodes, sealCodes);
		});
	}

	public void TestSealCodesOnlyContainersWithAdditionalSeals()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty SealCodes", 0, wrapper.SealCodes.Count);

			foreach (var seal in SecondContainerSeals)
			{
				var ctnNumber = "CTN1" + seal;
				var container = declaration.CusContainers.AddNew();
				container.CO_ContainerNumber = ctnNumber;
				container.AdditionalSeals.AddNew().BK_SealNumber = seal;
				invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(ctnNumber).IsForInvoiceLine = true;
			}

			wrapper = GetWrapper(entryHeader, Certificate);
			var sealCodes = wrapper.SealCodes;

			AssertArrayEqualsByElements("Expected filled SealCodes", SecondContainerSeals, sealCodes.ToArray());
			AssertSame("Cached SealCodes", wrapper.SealCodes, sealCodes);
		});
	}

	public void TestSealCodesOnlyEquipmentsWithSeals()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty SealCodes", 0, wrapper.SealCodes.Count);

			var packingGroups = declaration.Bills.AddNew().PackingGroups.AddNew();
			foreach (var seal in ContainerSeals)
			{
				var equNumber = "EQUIP1" + seal;
				var equipment = declaration.Equipments.AddNew();
				equipment.CEQ_IdentificationNumber = equNumber;
				equipment.Seals.AddNew().BK_SealNumber = seal;
				var pack1 = packingGroups.Packages.AddNew();
				pack1.CW_PackQty = 1;
				pack1.CW_ContainerNoOrEquipmentNo = equipment.CEQ_IdentificationNumber;
				invoiceLine.PackagesPivot.AddPivotFor(pack1);
			}

			wrapper = GetWrapper(entryHeader, Certificate);
			var sealCodes = wrapper.SealCodes;

			AssertArrayEqualsByElements("Expected filled SealCodes", ContainerSeals, sealCodes.ToArray());
			AssertSame("Cached SealCodes", wrapper.SealCodes, sealCodes);
		});
	}

	public void TestSealCodes_FromInvoiceLineWithContainersAndEquipment()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;

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

		CombineAssertions(() =>
		{
			var wrapper1 = GetWrapper(entryHeader1, Certificate);
			AssertArrayEqualsByElements("Expected filled SealCodes for first entryHeader", new ZString[] { "S01", "S02", "S21", "S22", "A01", "A02", "A03", "E01" }, wrapper1.SealCodes.ToArray());

			var wrapper2 = GetWrapper(entryHeader2, Certificate);
			AssertArrayEqualsByElements("Expected filled SealCodes for second entryHeader", new ZString[] { "S03", "S23", "A04", "E02", "E03" }, wrapper2.SealCodes.ToArray());
		});
	}

	public void TestTextFunctionCode()
	{
		invoiceHeader.ZG_TransportChargesMethodOfPayment = HeaderData.TransportCharges;
		AssertEquals("Expected filled TextFunctionCode", HeaderData.TransportCharges, wrapper.TextFunctionCode);
	}

	public void TestReferenceNumber()
	{
		declaration.JE_OwnerRef = HeaderData.OwnerRef;
		AssertEquals("Expected filled ReferenceNumber", HeaderData.OwnerRef, wrapper.ReferenceNumber);
	}

	public void TestSpecificCircumstancesIndicator()
	{
		declaration.ZG_SpecificCircumstanceIndicator = HeaderData.SpecificCircumstancesInd;
		AssertEquals("Expected filled SpecificCircumstancesIndicator", HeaderData.SpecificCircumstancesInd, wrapper.SpecificCircumstancesIndicator);
	}

	public void TestBorderTransportMode()
	{
		CombineAssertions(() =>
		{
			var borderTransportMode = wrapper.BorderTransportMode;
			AssertNotNull("Expected not null BorderTransportMode", borderTransportMode);
			AssertSame("Cached BorderTransportMode", wrapper.BorderTransportMode, borderTransportMode);
		});
	}

	public void TestInternalTransportMode()
	{
		declaration.JE_TransportModeInland = HeaderData.InternalTransportMode;
		AssertEquals("Expected filled InternalTransportMode", HeaderData.InternalTransportModeCode, wrapper.InternalTransportMode);
	}

	public void TestTransportModeName()
	{
		declaration.ZG_Box18TransportID = HeaderData.TransportModeId;
		AssertEquals("Expected filled TransportModeName", HeaderData.TransportModeId, wrapper.TransportModeName);
	}

	public void TestNullExporter()
	{
		AssertExceptionThrown<NullReferenceException>(() => wrapper.Exporter.ToString());
	}

	public void TestExporter()
	{
		CombineAssertions(() =>
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = HeaderData.ExporterCode;
			orgHeader.Addresses.AddNew();
			declaration.JE_OH_Supplier = orgHeader.PK;

			var exporter = wrapper.Exporter;

			AssertNotNull("Expected filled Exporter", exporter);
			AssertSame("Cached Exporter", wrapper.Exporter, exporter);
		});
	}

	public void TestNullReceiver()
	{
		AssertExceptionThrown<NullReferenceException>(() => wrapper.Receiver.ToString());
	}

	public void TestReceiver()
	{
		CombineAssertions(() =>
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = HeaderData.ExporterCode;
			orgHeader.Addresses.AddNew();
			declaration.JE_OH_Importer = orgHeader.PK;

			var receiver = wrapper.Receiver;

			AssertNotNull("Expected filled Receiver", receiver);
			AssertSame("Cached Receiver", wrapper.Receiver, receiver);
		});
	}

	public void TestLocationId()
	{
		declaration.ZG_AgreedPlaceCode = HeaderData.LocationId;
		AssertEquals("Expected filled LocationId", HeaderData.LocationId, wrapper.LocationId);
	}

	public void TestIsDeclarationInEuros()
	{
		Assert("Expected true IsDeclarationInEuros", wrapper.IsDeclarationInEuros);
	}

	public void TestLines()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected 1 Line (mandatory at least one)", 1, wrapper.Lines.Count);

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2203001011";

			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "2203001012";

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge", true, mergeResult);

			var entryHeader = declaration.CustomsEntryHeaders[0];
			wrapper = GetWrapper(entryHeader, Certificate);

			var lines = wrapper.Lines;

			AssertEquals("Expected 3 Lines", 3, wrapper.Lines.Count);
			AssertSame("Cached Lines", wrapper.Lines, lines);

			AssertLinesType();
		});
	}

	protected virtual void AssertLinesType()
	{
		AssertEquals(typeof(DUAExportLineWrapper), wrapper.Lines.FirstOrDefault().GetType());
	}

	public void TestTotalNumberOfPackageElements_NoVehicles()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty TotalNumberOfPackageElements", 0, wrapper.TotalNumberOfPackageElements);

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

			AssertEquals("Expected filled TotalNumberOfPackageElements with full packages", 12, wrapper.TotalNumberOfPackageElements);
		});
	}

	public void TestTotalNumberOfPackageElements_WithVehicles()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty TotalNumberOfPackageElements", 0, wrapper.TotalNumberOfPackageElements);

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
			wrapper = GetWrapper(entryHeader, Certificate);

			AssertEquals("Expected filled TotalNumberOfPackageElements with full packages", 12, wrapper.TotalNumberOfPackageElements);
		});
	}

	public void TestTotalNumberOfPackageElements_OnlyVehicles()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty TotalNumberOfPackageElements", 0, wrapper.TotalNumberOfPackageElements);

			var vehicle = invoiceLine.Vehicles.AddNew();
			invoiceLine.JI_Tariff = "2203001011";
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
			wrapper = GetWrapper(entryHeader, Certificate);

			AssertEquals("Expected filled TotalNumberOfPackageElements with full packages", 4, wrapper.TotalNumberOfPackageElements);
		});
	}

	protected virtual ZString ExpectedMessageType => ExportDeclarationWrapperMessageTypeCodeList.ExportDeclaration;

	protected override ZString ExpectedLocalReferenceNumber => "Reference";

	protected override DUAExportSendMessageWrapper GetWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) => new DUAExportSendMessageWrapper(cusEntryHeader, certificateData);

	protected override DUAExportSendMessageWrapper GetProvider() => GetWrapper(entryHeader, Certificate);
}
