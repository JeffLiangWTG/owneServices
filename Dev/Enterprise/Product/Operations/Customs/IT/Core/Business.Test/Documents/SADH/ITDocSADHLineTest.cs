using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.EU;
using Enterprise.DocumentWrappers.Customs.EU.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using CusEntryHeader = Enterprise.Customs.IT.Business.Declaration.CusEntryHeader;
using EuAdditionalInfoCollection = Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection;
using ITCusEntryLine = Enterprise.Customs.IT.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.IT.Business.Testing;

[MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Italy)]
sealed class ITDocSADHLineTest : DocSADHLineTest
{
	public void TestBox18IdentityOfTransportAtDepartureLineForIMPUCC6_COM_EXPNonUCC6Declarations()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		var wrapper = ITDocSADHLine.New(entryLine, Factory);

		CombineAssertions("Box18 value", () =>
		{
			AssertBox18IdentityOfTransportAtDepartureForIMPUCC6_COM_EXPNonUCC6Declarations(
				declaration,
				wrapper,
				"IMP",
				"AB 123CD",
				ZString.Empty);

			AssertBox18IdentityOfTransportAtDepartureForIMPUCC6_COM_EXPNonUCC6Declarations(
				declaration,
				wrapper,
				"COM",
				"AB 123CD",
				ZString.Empty);

			AssertBox18IdentityOfTransportAtDepartureForIMPUCC6_COM_EXPNonUCC6Declarations(
				declaration,
				wrapper,
				"EXP",
				"AB 123CD",
				ZString.Empty);
		});
	}

	public void TestBox18IdentityOfTransportAtDepartureLineFormattedForEXPUCC6Declarations()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		var wrapper = ITDocSADHLine.New(entryLine, Factory);

		CombineAssertions("Box18 value for EXP", () =>
		{
			AssertBox18IdentityOfTransportAtDepartureForEXPUCC6Declarations(
				declaration,
				wrapper,
				"AIR",
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty);

			AssertBox18IdentityOfTransportAtDepartureForEXPUCC6Declarations(
				declaration,
				wrapper,
				"AIR",
				"XYZ-10001000101",
				ZString.Empty,
				ZString.Empty,
				"XYZ-10001000101");

			AssertBox18IdentityOfTransportAtDepartureForEXPUCC6Declarations(
				declaration,
				wrapper,
				"RAI",
				"RAI-10001000101",
				ZString.Empty,
				ZString.Empty,
				"RAI-10001000101");

			AssertBox18IdentityOfTransportAtDepartureForEXPUCC6Declarations(
				declaration,
				wrapper,
				"FIX",
				"XYZ-10001000101",
				ZString.Empty,
				ZString.Empty,
				"XYZ-10001000101");

			AssertBox18IdentityOfTransportAtDepartureForEXPUCC6Declarations(
				declaration,
				wrapper,
				"AIR",
				"XYZ-10001000101",
				"KA29",
				ZString.Empty,
				"KA29");

			AssertBox18IdentityOfTransportAtDepartureForEXPUCC6Declarations(
				declaration,
				wrapper,
				"AIR",
				ZString.Empty,
				"KA29",
				ZString.Empty,
				"KA29");

			AssertBox18IdentityOfTransportAtDepartureForEXPUCC6Declarations(
				declaration,
				wrapper,
				"RAI",
				"RAI-10001000101",
				"KA29",
				"RAI29",
				"RAI-10001000101");

			AssertBox18IdentityOfTransportAtDepartureForEXPUCC6Declarations(
				declaration,
				wrapper,
				"RAI",
				ZString.Empty,
				"KA29",
				"RAI29",
				"RAI29");
		});
	}

	void AssertBox18IdentityOfTransportAtDepartureForIMPUCC6_COM_EXPNonUCC6Declarations(
		JobDeclaration declaration,
		ITDocSADHLine wrapper,
		string messageType,
		string box18TransportID,
		ZString expectedValueWhenNoID)
	{
		declaration.JE_MessageType = messageType;
		declaration.ZG_Box18TransportID = box18TransportID;

		AssertEquals(
			$"For {declaration.JE_MessageType} when Box18TransportID available",
			box18TransportID,
			wrapper.Box18IdentityOfTransportAtDepartureLine);

		declaration.ZG_Box18TransportID = "";
		AssertEquals(
			$"For {declaration.JE_MessageType} when Box18TransportID not available",
			expectedValueWhenNoID,
			wrapper.Box18IdentityOfTransportAtDepartureLine);
	}

	void AssertBox18IdentityOfTransportAtDepartureForEXPUCC6Declarations(
		JobDeclaration declaration,
		ITDocSADHLine wrapper,
		ZString transportModeInland,
		ZString transportIDInland,
		ZString aircraftRegistrationInland,
		ZString trailer1RegNo,
		ZString expectedValue)
	{
		declaration.JE_MessageType = "EXP";
		declaration.MessageVersion = "XML";
		declaration.ZG_Box18TransportID = "AB 123CD";
		declaration.JE_TransportModeInland = transportModeInland;
		declaration.JE_TransportIDInland = transportIDInland;
		declaration.JE_AircraftRegistrationInland = aircraftRegistrationInland;
		declaration.JE_Trailer1RegNo = trailer1RegNo;

		AssertEquals(
			$@"When JE_TransportModeInland is {transportModeInland},JE_TransportIDInland is {transportIDInland},
				JE_AircraftRegistrationInland is {aircraftRegistrationInland} and JE_Trailer1RegNo is {trailer1RegNo}",
			expectedValue, wrapper.Box18IdentityOfTransportAtDepartureLine);
	}

	public void TestBox35GrossWeightInKGForIMPDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();

		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_Weight = 100123.456m;
		invoiceLine.JI_WeightUQ = Core.Constants.Weight.Grams;

		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		declaration.JE_MessageType = "IMP";
		var wrapper = ITDocSADHLine.New(entryLine, Factory);
		AssertEquals("For IMP declaration, weight is rounded to 5 decimals", "100.12346", wrapper.Box35GrossWeightInKG);
	}

	public void TestBox35GrossWeightInKGForEXPDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();

		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_Weight = 100123.456m;
		invoiceLine.JI_WeightUQ = Core.Constants.Weight.Grams;

		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		declaration.JE_MessageType = "EXP";
		var wrapper = ITDocSADHLine.New(entryLine, Factory);
		AssertEquals("For EXP declaration, weight is rounded to 5 decimals", "100.12346", wrapper.Box35GrossWeightInKG);
	}

	public void TestBox35GrossWeightInKGForCOMDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();

		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_WeightUQ = Core.Constants.Weight.Grams;
		invoiceLine.JI_Weight = 101123.456m;
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		CombineAssertions("When declaration is COM", () =>
		{
			declaration.JE_MessageType = "COM";
			var wrapper = ITDocSADHLine.New(entryLine, Factory);
			AssertEquals("For COM declaration, weight is rounded to 5 decimals", "101.12346", wrapper.Box35GrossWeightInKG);

			invoiceLine.JI_Weight = 0.00m;
			AssertEquals("For COM declaration, if sum of weights is 0 then print empty string", ZString.Empty, wrapper.Box35GrossWeightInKG);
		});
	}

	public void TestBox38NetWeightInKGForIMPDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();

		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_NetWeight = 100123.456m;
		invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Grams;

		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		declaration.JE_MessageType = "IMP";
		var wrapper = ITDocSADHLine.New(entryLine, Factory);
		AssertEquals("For IMP declaration, weight is rounded to 5 decimals", "100.12346", wrapper.Box38NetWeightInKG);
	}

	public void TestBox38NetWeightInKGForEXPDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();

		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_NetWeight = 100123.456m;
		invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Grams;

		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		declaration.JE_MessageType = "EXP";
		var wrapper = ITDocSADHLine.New(entryLine, Factory);
		AssertEquals("For EXP declaration, weight is rounded to 5 decimals", "100.12346", wrapper.Box38NetWeightInKG);
	}

	public void TestBox38NetWeightInKGForCOMDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();

		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CustomsUnitQty = Core.Constants.Weight.Grams;
		invoiceLine.JI_CustomsQuantity = 101123.456m;
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		CombineAssertions("When declaration is COM Customs Quantity is reported in place of Net Weight", () =>
		{
			declaration.JE_MessageType = "COM";
			var wrapper = ITDocSADHLine.New(entryLine, Factory);
			AssertEquals("For COM declaration, weight is rounded to 5 decimals", "101.12346", wrapper.Box38NetWeightInKG);

			invoiceLine.JI_CustomsQuantity = 0.00m;
			AssertEquals("For COM declaration, if sum of weights is 0 then print empty string", ZString.Empty, wrapper.Box38NetWeightInKG);
		});
	}

	public void TestBox38NetWeightInKGWithCustomsUnitQty()
	{
		TestBox38NetWeightInKGWithCustomsUnitQtyCore("IMP");
		TestBox38NetWeightInKGWithCustomsUnitQtyCore("EXP");
		TestBox38NetWeightInKGWithCustomsUnitQtyCore("COM");
	}

	void TestBox38NetWeightInKGWithCustomsUnitQtyCore(string messageType)
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();

		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		declaration.JE_MessageType = messageType;
		var wrapper = ITDocSADHLine.New(entryLine, Factory);

		invoiceLine.JI_CustomsUnitQty = ZString.Empty;
		invoiceLine.JI_CustomsQuantity = 23.45678m;
		AssertEquals($"For {messageType} declaration, JI_CustomsQuantity with JI_CustomsUnitQty blank", ZString.Empty, wrapper.Box38NetWeightInKG);

		invoiceLine.JI_CustomsUnitQty = Enterprise.Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;
		AssertEquals($"For {messageType} declaration, JI_CustomsQuantity with JI_CustomsUnitQty = KGM", "23.45678", wrapper.Box38NetWeightInKG);
	}

	public void TestGetFirstLineOfBox44()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine.ZG_SteelType = SteelTypeList.Codes._0;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		var wrapper = ITDocSADHLine.New(entryLine, Factory);
		AssertEquals(string.Empty, wrapper.Box44AddInfoAndDocuments);

		invoiceLine.ZG_SteelType = SteelTypeList.Codes._1;
		AssertEquals("AC=1", wrapper.Box44AddInfoAndDocuments);

		invoiceLine.ZG_SteelType = SteelTypeList.Codes._2;
		AssertEquals("AC=2", wrapper.Box44AddInfoAndDocuments);

		invoiceLine.ZG_SteelType = SteelTypeList.Codes._3;
		AssertEquals("AC=3", wrapper.Box44AddInfoAndDocuments);

		invoiceLine.ZG_SteelType = SteelTypeList.Codes._4;
		AssertEquals("AC=4", wrapper.Box44AddInfoAndDocuments);

		invoiceLine.ZG_SteelType = "5";
		AssertEquals("AC=5", wrapper.Box44AddInfoAndDocuments);
	}

	public new void TestBox313ContainerNumbers()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER1";
		declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER2";
		declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER3";
		declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER4";
		declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER5";
		declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER6";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoiceLine1 = entryLine.InvoiceLines.AddNew();
		var invoiceLine2 = entryLine.InvoiceLines.AddNew();
		var invoiceLine3 = entryLine.InvoiceLines.AddNew();
		var lineWrapper = ITDocSADHLine.New(entryLine, Factory);
		AssertEquals("No containers for the entry line", ZString.Empty, lineWrapper.Box31_3ContainerNumbers);

		invoiceLine1.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[0].PK;
		invoiceLine1.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[1].PK;
		invoiceLine2.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[0].PK;
		invoiceLine2.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[1].PK;
		invoiceLine3.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[2].PK;
		lineWrapper = ITDocSADHLine.New(entryLine, Factory);
		AssertEquals("3 containers for the entry line. Threshold not exceeded", "CONTAINER1, CONTAINER2, CONTAINER3", lineWrapper.Box31_3ContainerNumbers);

		invoiceLine3.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[3].PK;
		invoiceLine3.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[4].PK;
		invoiceLine3.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[5].PK;
		lineWrapper = ITDocSADHLine.New(entryLine, Factory);
		AssertEquals("6 containers for the entry line. Threshold exceeded", "VEDI ALLEGATO TOT. CNT 6", lineWrapper.Box31_3ContainerNumbers);
	}

	public void TestBox313ContainerNumbersForEadFirstLine()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER1";
		declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER2";
		declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER3";
		declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER4";
		declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER5";
		declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER6";
		declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER7";
		declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER8";
		declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER9";
		declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER10";
		declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER11";
		declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER12";
		declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER13";

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoiceLine1 = entryLine.InvoiceLines.AddNew();
		var invoiceLine2 = entryLine.InvoiceLines.AddNew();
		var invoiceLine3 = entryLine.InvoiceLines.AddNew();
		entryLine.UseEadAttachmentPrintingSupporter = true;
		entryLine.CL_LineNumber = 1;
		var lineWrapper = ITDocSADHLine.New(entryLine, Factory);
		AssertEquals("No containers for the entry line", ZString.Empty, lineWrapper.Box31_3ContainerNumbers);

		invoiceLine1.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[0].PK;
		invoiceLine1.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[1].PK;
		invoiceLine2.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[0].PK;
		invoiceLine2.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[1].PK;
		invoiceLine3.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[2].PK;
		lineWrapper = ITDocSADHLine.New(entryLine, Factory);
		AssertEquals("3 containers for the entry line. Threshold not exceeded", "CONTAINER1, CONTAINER2, CONTAINER3", lineWrapper.Box31_3ContainerNumbers);

		invoiceLine3.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[3].PK;
		invoiceLine3.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[4].PK;
		invoiceLine3.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[5].PK;
		invoiceLine3.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[6].PK;
		invoiceLine3.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[7].PK;
		invoiceLine3.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[8].PK;
		invoiceLine3.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[9].PK;
		invoiceLine3.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[10].PK;
		invoiceLine3.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[11].PK;
		invoiceLine3.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[12].PK;
		lineWrapper = ITDocSADHLine.New(entryLine, Factory);
		AssertEquals("13 containers for the entry line. Threshold exceeded", "VEDI ALLEGATO TOT. CNT 13", lineWrapper.Box31_3ContainerNumbers);
	}

	public void TestBox313ContainerNumbersForEadGenericLine()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER1";
		declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER2";
		declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER3";
		declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER4";

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoiceLine1 = entryLine.InvoiceLines.AddNew();
		var invoiceLine2 = entryLine.InvoiceLines.AddNew();
		var invoiceLine3 = entryLine.InvoiceLines.AddNew();
		entryLine.UseEadAttachmentPrintingSupporter = true;
		entryLine.CL_LineNumber = 7;
		var lineWrapper = ITDocSADHLine.New(entryLine, Factory);
		AssertEquals("No containers for the entry line", ZString.Empty, lineWrapper.Box31_3ContainerNumbers);

		invoiceLine1.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[0].PK;
		invoiceLine1.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[1].PK;
		invoiceLine2.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[0].PK;
		invoiceLine2.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[1].PK;
		invoiceLine3.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[2].PK;
		lineWrapper = ITDocSADHLine.New(entryLine, Factory);
		AssertEquals("3 containers for the entry line. Threshold not exceeded", "CONTAINER1, CONTAINER2, CONTAINER3", lineWrapper.Box31_3ContainerNumbers);

		invoiceLine3.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[3].PK;
		lineWrapper = ITDocSADHLine.New(entryLine, Factory);
		AssertEquals("4 containers for the entry line. Threshold exceeded", "VEDI ALLEGATO TOT. CNT 4", lineWrapper.Box31_3ContainerNumbers);
	}

	public void TestAttachmentContainerNumbers()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER1";
		declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER2";
		declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER3";
		declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER4";
		declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER5";
		declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER6";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoiceLine1 = entryLine.InvoiceLines.AddNew();
		var invoiceLine2 = entryLine.InvoiceLines.AddNew();
		var invoiceLine3 = entryLine.InvoiceLines.AddNew();
		var lineWrapper = ITDocSADHLine.New(entryLine, Factory);
		AssertEquals("No containers for the entry line. Attachment is empty", ZString.Empty, lineWrapper.AttachmentContainerNumbers);

		invoiceLine1.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[0].PK;
		invoiceLine1.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[1].PK;
		invoiceLine2.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[0].PK;
		invoiceLine2.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[1].PK;
		invoiceLine3.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[2].PK;
		lineWrapper = ITDocSADHLine.New(entryLine, Factory);
		AssertEquals("3 containers for the entry line. Threshold not exceeded. Attachment is empty", ZString.Empty, lineWrapper.AttachmentContainerNumbers);

		invoiceLine3.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[3].PK;
		invoiceLine3.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[4].PK;
		invoiceLine3.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[5].PK;
		lineWrapper = ITDocSADHLine.New(entryLine, Factory);
		AssertEquals("6 containers for the entry line. Threshold exceeded. Attachment is not empty", "CONTAINER1, CONTAINER2, CONTAINER3, CONTAINER4, CONTAINER5, CONTAINER6", lineWrapper.AttachmentContainerNumbers);
	}

	public void TestBox31FreightChargesInfo()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		var lineWrapper = ITDocSADHLine.New(entryLine, Factory);
		AssertEquals(ZString.Empty, lineWrapper.Box31PackagesAndDescriptionOfGoods);

		var invoiceLineCharge1 = invoiceLine.Charges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 50m, CurrencyCodes.EuropeanUnion);
		invoiceLineCharge1.J7_IsDutiable = true;
		invoiceLineCharge1.J7_IsStatisticalValueApplicable = true;
		invoiceLineCharge1.J7_IsGSTApplicable = true;
		AssertEquals("Nolo ExtraCee: 50.00", lineWrapper.Box31PackagesAndDescriptionOfGoods);

		var invoiceLineCharge2 = invoiceLine.Charges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 100m, CurrencyCodes.EuropeanUnion);
		invoiceLineCharge2.J7_IsDutiable = false;
		invoiceLineCharge2.J7_IsStatisticalValueApplicable = true;
		invoiceLineCharge2.J7_IsGSTApplicable = true;
		AssertEquals("Nolo ExtraCee: 50.00, Nolo Cee: 100.00", lineWrapper.Box31PackagesAndDescriptionOfGoods);

		var invoiceLineCharge3 = invoiceLine.Charges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 300m, CurrencyCodes.EuropeanUnion);
		invoiceLineCharge3.J7_IsDutiable = false;
		invoiceLineCharge3.J7_IsStatisticalValueApplicable = false;
		invoiceLineCharge3.J7_IsGSTApplicable = true;
		AssertEquals("Nolo ExtraCee: 50.00, Nolo Cee: 100.00, Nolo Nazionale: 300.00", lineWrapper.Box31PackagesAndDescriptionOfGoods);
	}

	public override void TestBox40Contents()
	{
		JobDeclaration declaration = Factory.New<JobDeclaration>();
		JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
		ITCusEntryLine entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		PreviousDocument documentOnHeader = invoiceHeader.PreviousDocuments.AddNew();
		documentOnHeader.CSI_Procedure = "7";
		documentOnHeader.CSI_SubType = "X";
		documentOnHeader.CSI_Code = "280";
		documentOnHeader.CSI_Status = "G";
		documentOnHeader.CSI_CustomsOffice = "IT279100";
		documentOnHeader.CSI_LineNo = 1;
		documentOnHeader.CSI_ReferenceNumber = "ABCDEFG";

		PreviousDocument documentOnFirstLine = invoiceLine.PreviousDocuments.AddNew();
		documentOnFirstLine.CSI_Procedure = "7";
		documentOnFirstLine.CSI_SubType = Enterprise.Customs.EU.Business.PreviousDocumentClassList.Codes.PreviousDocument;
		documentOnFirstLine.CSI_Code = "380";
		documentOnFirstLine.CSI_ReferenceNumber = "34217890";
		documentOnFirstLine.CSI_DateOfIssue = new ZDateTime(2000, 1, 2);
		documentOnFirstLine.CSI_Status = "G";
		documentOnFirstLine.CSI_CustomsOffice = "IT279100";
		documentOnFirstLine.CSI_LineNo = 1;

		PreviousDocument documentOnSecondLine = invoiceLine.PreviousDocuments.AddNew();
		documentOnSecondLine.CSI_Procedure = "7";
		documentOnSecondLine.CSI_SubType = "Y";
		documentOnSecondLine.CSI_Code = "CLE";
		documentOnSecondLine.CSI_ReferenceNumber2 = "20070701";
		documentOnSecondLine.CSI_DateOfIssue = new ZDateTime(2000, 1, 3);
		documentOnSecondLine.CSI_Status = "G";
		documentOnSecondLine.CSI_CustomsOffice = "IT279100";
		documentOnSecondLine.CSI_LineNo = 1;

		PreviousDocument documentOnGroup = declaration.PreviousDocuments.AddNew();
		documentOnGroup.CSI_Procedure = "7";
		documentOnGroup.CSI_SubType = "A";
		documentOnGroup.CSI_Code = "123";
		documentOnGroup.CSI_ReferenceNumber = "98765432";
		documentOnGroup.CSI_Status = "G";
		documentOnGroup.CSI_CustomsOffice = "IT279100";
		documentOnGroup.CSI_LineNo = 1;

		var line = ITDocSADHLine.New(entryLine, Factory);
		AssertEquals("entryLine.Box40PreviousDocuments", "A-123-7-98765432 G-IT279100-1; X-280-7-ABCDEFG G-IT279100-1; Z-380-7-34217890 G-02/01/2000-IT279100-1; Y-CLE-7-20070701 G-03/01/2000-IT279100-1", line.Box40PreviousDocuments);
	}

	public void TestBox44AddInfoAndDocuments()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine1.JI_CL = entryLine.PK;
		var lineWrapper = ITDocSADHLine.New(entryLine, Factory);
		AssertEquals("No supporting documents for the entry line", ZString.Empty, lineWrapper.Box44AddInfoAndDocuments);

		var declarationSupportingDocument1 = declaration.SupportingDocuments.AddNew();
		declarationSupportingDocument1.CSI_Code = "N380";
		declarationSupportingDocument1.CSI_RN_NKCountryCode = "IT";
		declarationSupportingDocument1.CSI_YearOfIssue = "2020";
		declarationSupportingDocument1.CSI_ReferenceNumber = new string('1', 50);
		declarationSupportingDocument1.CSI_UnitOfQuantity = "KGM";
		declarationSupportingDocument1.CSI_Quantity = 100.1234m;
		var declarationSupportingDocument2 = declaration.SupportingDocuments.AddNew();
		declarationSupportingDocument2.CSI_Code = "N380";
		declarationSupportingDocument2.CSI_RN_NKCountryCode = "IT";
		declarationSupportingDocument2.CSI_YearOfIssue = "2020";
		declarationSupportingDocument2.CSI_ReferenceNumber = new string('2', 10);
		declarationSupportingDocument2.CSI_UnitOfQuantity = "KGM";
		declarationSupportingDocument2.CSI_Quantity = 100.1234m;
		lineWrapper = ITDocSADHLine.New(entryLine, Factory);
		AssertEquals("Threshold not exceeded", "N380-IT-2020-11111111111111111111111111111111111111111111111111-KGM-100.1234;N380-IT-2020-2222222222-KGM-100.1234", lineWrapper.Box44AddInfoAndDocuments);

		var declarationSupportingDocument3 = declaration.SupportingDocuments.AddNew();
		declarationSupportingDocument3.CSI_Code = "N380";
		declarationSupportingDocument3.CSI_RN_NKCountryCode = "IT";
		declarationSupportingDocument3.CSI_YearOfIssue = "2020";
		declarationSupportingDocument3.CSI_ReferenceNumber = new string('3', 50);
		declarationSupportingDocument3.CSI_UnitOfQuantity = "KGM";
		declarationSupportingDocument3.CSI_Quantity = 100.1234m;
		lineWrapper = ITDocSADHLine.New(entryLine, Factory);
		AssertEquals("Threshold exceeded", "VEDI TIPI DOCUMENTI COME DA LISTA ALLEGATA", lineWrapper.Box44AddInfoAndDocuments);
	}

	public void TestBox44AddInfoAndDocuments_WithAdditionalDocuments()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		var lineWrapper = ITDocSADHLine.New(entryLine, Factory);

		AddAdditionalInfo(invoice.AdditionalInfos, "REF", "Y001", "123", "");
		AddAdditionalInfo(invoiceLine.AdditionalInfos, "INF", "30500", "", "456");
		AddAdditionalInfo(invoiceLine.AdditionalInfos, "TRA", "N722", "789", "");
		AssertEquals("Box44AddInfoAndDocuments with AdditionalDocuments", "30500-456; N722-789; Y001-123", lineWrapper.Box44AddInfoAndDocuments);
	}

	public void TestBox44AddInfoAndDocuments_WithRemarks()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CL = entryLine.PK;
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_CL = entryLine.PK;
		var lineWrapper = ITDocSADHLine.New(entryLine, Factory);

		invoiceLine1.Remarks = "Invoice Line1 Remarks\r\nSecond Row Remarks";
		invoiceLine2.Remarks = "Invoice Line2 Remarks";
		AssertEquals("Box44AddInfoAndDocuments with Remarks", "Invoice Line1 Remarks Second Row Remarks; Invoice Line2 Remarks", lineWrapper.Box44AddInfoAndDocuments);
	}

	public void TestBox44AddInfoAndDocumentsNotContainingSupportingDocument9DCR()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "UCR REFERENCE";
		var entryLine = entryHeader.MergedLines.AddNew();
		entryLine.CL_LineNumber = 1;
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		var lineWrapper = ITDocSADHLine.New(entryLine, Factory);
		AssertNotContains("Declaration UCR supporting document is not expected", "9DCR", lineWrapper.Box44AddInfoAndDocuments);
	}

	public void TestAttachmentSupportingDocuments()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine1.JI_CL = entryLine.PK;

		var declarationSupportingDocument1 = declaration.SupportingDocuments.AddNew();
		declarationSupportingDocument1.CSI_Code = "N380";
		declarationSupportingDocument1.CSI_RN_NKCountryCode = "IT";
		declarationSupportingDocument1.CSI_YearOfIssue = "2020";
		declarationSupportingDocument1.CSI_ReferenceNumber = new string('1', 50);
		declarationSupportingDocument1.CSI_UnitOfQuantity = "KGM";
		declarationSupportingDocument1.CSI_Quantity = 100.1234m;
		var lineWrapper = ITDocSADHLine.New(entryLine, Factory);
		AssertEquals("Threshold not exceeded. Attachment is empty", ZString.Empty, lineWrapper.AttachmentSupportingDocuments);

		var declarationSupportingDocument2 = declaration.SupportingDocuments.AddNew();
		declarationSupportingDocument2.CSI_Code = "N380";
		declarationSupportingDocument2.CSI_RN_NKCountryCode = "IT";
		declarationSupportingDocument2.CSI_YearOfIssue = "2020";
		declarationSupportingDocument2.CSI_ReferenceNumber = new string('2', 10);
		declarationSupportingDocument2.CSI_UnitOfQuantity = "KGM";
		declarationSupportingDocument2.CSI_Quantity = 100.1234m;
		lineWrapper = ITDocSADHLine.New(entryLine, Factory);
		AssertEquals("Threshold not exceeded yet. Attachment is empty", ZString.Empty, lineWrapper.AttachmentSupportingDocuments);
		AssertEquals("Threshold not exceeded.", false, lineWrapper.ContainsBox44Attachments);

		var declarationSupportingDocument3 = declaration.SupportingDocuments.AddNew();
		declarationSupportingDocument3.CSI_Code = "N380";
		declarationSupportingDocument3.CSI_RN_NKCountryCode = "IT";
		declarationSupportingDocument3.CSI_YearOfIssue = "2020";
		declarationSupportingDocument3.CSI_ReferenceNumber = new string('3', 50);
		declarationSupportingDocument3.CSI_UnitOfQuantity = "KGM";
		declarationSupportingDocument3.CSI_Quantity = 100.1234m;
		lineWrapper = ITDocSADHLine.New(entryLine, Factory);
		AssertEquals("Threshold exceeded. Attachment is not empty",
			"N380-IT-2020-11111111111111111111111111111111111111111111111111-KGM-100.1234\r\n" +
			"N380-IT-2020-2222222222-KGM-100.1234\r\n" +
			"N380-IT-2020-33333333333333333333333333333333333333333333333333-KGM-100.1234", lineWrapper.AttachmentSupportingDocuments);
		AssertEquals("Threshold exceeded.", true, lineWrapper.ContainsBox44Attachments);
	}

	public void TestAttachmentAdditionalInfos_WhenSAD()
	{
		var factory = Factory;
		var declaration = factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		AddAdditionalInfo(invoice.AdditionalInfos, "REF", "Y001", new string('1', 50), "");

		var lineWrapper = ITDocSADHLine.New(entryLine, factory);
		AssertEquals("Threshold not exceeded", ZString.Empty, lineWrapper.AttachmentSupportingDocuments);
		AssertEquals("Threshold not exceeded.", false, lineWrapper.ContainsBox44Attachments);

		AddAdditionalInfo(invoiceLine.AdditionalInfos, "INF", "30500", "", new string('2', 50));
		AddAdditionalInfo(invoiceLine.AdditionalInfos, "TRA", "N722", new string('3', 50), "");

		lineWrapper = ITDocSADHLine.New(entryLine, factory);
		AssertEquals("Threshold not exceeded", ZString.Empty, lineWrapper.AttachmentSupportingDocuments);
		AssertEquals("Threshold not exceeded.", false, lineWrapper.ContainsBox44Attachments);
	}

	public void TestAttachmentAdditionalInfos_WhenEAD()
	{
		var factory = Factory;
		var declaration = factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		entryLine.UseEadAttachmentPrintingSupporter = true;
		invoiceLine.JI_CL = entryLine.PK;

		AddAdditionalInfo(invoice.AdditionalInfos, "REF", "Y001", new string('1', 50), "");

		var lineWrapper = ITDocSADHLine.New(entryLine, factory);
		AssertEquals("Threshold not exceeded", ZString.Empty, lineWrapper.AttachmentSupportingDocuments);
		AssertEquals("Threshold not exceeded.", false, lineWrapper.ContainsBox44Attachments);

		AddAdditionalInfo(invoiceLine.AdditionalInfos, "INF", "30500", "", new string('2', 50));
		AddAdditionalInfo(invoiceLine.AdditionalInfos, "TRA", "N722", new string('3', 50), "");

		lineWrapper = ITDocSADHLine.New(entryLine, factory);
		AssertEquals("Threshold exceeded. Attachment is not empty",
			 "30500-22222222222222222222222222222222222222222222222222\r\n" +
			 "N722-33333333333333333333333333333333333333333333333333\r\n" +
			 "Y001-11111111111111111111111111111111111111111111111111"
			, lineWrapper.AttachmentAdditionalInfos);
		AssertEquals("Threshold exceeded.", true, lineWrapper.ContainsBox44Attachments);
	}

	public void TestContainsBox44Attachments_WhenEAD()
	{
		var factory = Factory;
		var declaration = factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		entryLine.UseEadAttachmentPrintingSupporter = true;
		invoiceLine.JI_CL = entryLine.PK;

		AddAdditionalInfo(invoice.AdditionalInfos, "REF", "Y001", new string('1', 50), "");

		var lineWrapper = ITDocSADHLine.New(entryLine, factory);
		AssertEquals("Threshold not exceeded.", false, lineWrapper.ContainsBox44Attachments);

		var invoiceLineSupportingDocuments = invoiceLine.SupportingDocuments;
		var invoiceHeaderSupportingDocuments = invoice.SupportingDocuments;

		AddSupportingDocument(invoiceHeaderSupportingDocuments, "N380", "IT", "2021", "A0023", "", 343.43m, "", 12m);
		AddSupportingDocument(invoiceLineSupportingDocuments, "C601", "DE", "2021", "A0050", "KGM", 200.43m, "", 4.56m);
		AddSupportingDocument(invoiceLineSupportingDocuments, "N830", "IE", "2023", "A0040", "KGO", ZDecimal.Zero, "", 0);

		lineWrapper = ITDocSADHLine.New(entryLine, factory);
		AssertEquals("Threshold exceeded.", true, lineWrapper.ContainsBox44Attachments);
	}

	public void TestBox49Warehouse_Export()
	{
		var procedure = new UniversalReferenceTestDataHelper(Factory).CreateOrFindExistingRefCusProcedure("IT", "EX", "10", "71", "F61", "", "EXP", "10P");

		procedure.ZZ6_IntoWarehouse = "Y";
		procedure.ZZ6_OutOfWarehouse = "N";
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		var sadhLine = GetNewDocSADHLine(entryLine);
		AssertEquals("When entry header has not related warehouse", "", sadhLine.Box49Warehouse);

		var warehouse = Factory.New<OrgHeader>();
		var warehouseCustomsCode = warehouse.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "A123", RefCountry.LoadFromCountryCode(Factory, "IT"));
		warehouseCustomsCode.OK_OA_PremisesAddress = warehouse.MainAddress.PK;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_OA_Warehouse2 = warehouse.MainAddress.PK;
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		invoiceLine.JI_CEI = entryInstruction.PK;

		sadhLine = GetNewDocSADHLine(entryLine);
		AssertEquals("When related warehouse has not a valid CustomsRegNo", "123", sadhLine.Box49Warehouse);

		warehouseCustomsCode.OK_CustomsRegNo = "A12345678GB";
		sadhLine = GetNewDocSADHLine(entryLine);
		AssertEquals("1234567", sadhLine.Box49Warehouse);
	}

	public void TestBox49Warehouse_Import()
	{
		var procedure = new UniversalReferenceTestDataHelper(Factory).CreateOrFindExistingRefCusProcedure("IT", "IM", "10", "71", "F61", "", "IMP", "10P");
		procedure.ZZ6_IntoWarehouse = "Y";
		procedure.ZZ6_OutOfWarehouse = "N";
		var procedure2 = new UniversalReferenceTestDataHelper(Factory).CreateOrFindExistingRefCusProcedure("IT", "IM", "42", "71", "C33", "", "IMP", "42P");
		procedure2.ZZ6_IntoWarehouse = "N";
		procedure2.ZZ6_OutOfWarehouse = "Y";
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		var warehouse = Factory.New<OrgHeader>();
		var warehouseCustomsCode = warehouse.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "A123", RefCountry.LoadFromCountryCode(Factory, "IT"));
		warehouseCustomsCode.OK_OA_PremisesAddress = warehouse.MainAddress.PK;
		var warehouse2 = Factory.New<OrgHeader>();
		var warehouseCustomsCode2 = warehouse2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "B321", RefCountry.LoadFromCountryCode(Factory, "IT"));
		warehouseCustomsCode2.OK_OA_PremisesAddress = warehouse2.MainAddress.PK;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_OA_Warehouse = warehouse.MainAddress.PK;
		entryInstruction.CEI_OA_Warehouse2 = warehouse2.MainAddress.PK;
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		invoiceLine.JI_CEI = entryInstruction.PK;

		invoiceLine.JI_Procedure = "1071F61";
		var sadhLine = GetNewDocSADHLine(entryLine);
		AssertEquals("When Has IntoWarehouse Procedure", "B321", sadhLine.Box49Warehouse);

		invoiceLine.JI_Procedure = "4271C33";
		sadhLine = GetNewDocSADHLine(entryLine);
		AssertEquals("When Has OutOfWarehouse Procedure", "A123", sadhLine.Box49Warehouse);

		invoiceLine.JI_Procedure = string.Empty;
		sadhLine = GetNewDocSADHLine(entryLine);
		AssertEquals("When No Procedure", ZString.Empty, sadhLine.Box49Warehouse);
	}

	protected override DocBaseWrapper GetNewDocumentWrapper()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		return ITDocSADHLine.New(entryLine, Factory);
	}

	protected override DocSADHLine GetSADHLineForBox47Taxes()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();

		var feeA00 = entryLine.Fees.AddNew();
		feeA00.CF_ChargeType = "A00";
		feeA00.CF_BaseValue = 1105.89m;
		feeA00.CF_ChargeAmount = 29.85m;
		var feeB00 = entryLine.Fees.AddNew();
		feeB00.CF_ChargeType = "B00";
		feeB00.CF_BaseValue = 1266.03m;
		feeB00.CF_ChargeAmount = 221.55m;
		return ITDocSADHLine.New(entryLine, Factory);
	}

	protected override ZString ExpectedBox49WarehouseInward => "A12345678IT";
	protected override ZString ExpectedBox49WarehouseOutward => "A12345678IT";

	protected override void SetUpForOutwardProcedure()
	{
		zzzDataGrouping = "IT";
		procedureCode = "71";
		previousProcedureCode = "40";
		concession = "C33";
		country = Core.Constants.CountryCodes.Italy;
		customsRegNo = "A12345678IT";
		shipmentType = "IMP";
		group = "IT";
	}

	protected override void SetUpForInwardProcedure()
	{
		zzzDataGrouping = "IT";
		procedureCode = "71";
		previousProcedureCode = "40";
		concession = "C33";
		country = Core.Constants.CountryCodes.Italy;
		customsRegNo = "A12345678IT";
		shipmentType = "IMP";
		group = "IT";
	}

	public void TestBox47TaxesWithPlaceholder()
	{
		var factory = Factory;
		var declaration = factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var lineWrapper = ITDocSADHLine.New(entryLine, factory);
		CombineAssertions("PRE-CONDITION", () =>
		{
			AssertType<ITDocSADHLineTaxCollection>(lineWrapper.Box47Taxes);
			AssertEquals("No entry line fees", 0, entryLine.Fees.Count);
			AssertEquals("No Box47Taxes items", 0, lineWrapper.Box47Taxes.Count);
		});

		for (int i = 0; i < 8; i++)
		{
			entryLine.Fees.AddNew();
		}
		lineWrapper = ITDocSADHLine.New(entryLine, factory);
		CombineAssertions("EntryLine fees fit in Box47Taxes (8 items)", () =>
		{
			AssertEquals("8 entry line fees", 8, entryLine.Fees.Count);
			AssertEquals("8 Box47Taxes items", 8, lineWrapper.Box47Taxes.Count);
		});

		entryLine.Fees.AddNew();
		lineWrapper = ITDocSADHLine.New(entryLine, factory);
		CombineAssertions("EntryLine fees don't fit in Box47Taxes (9 items). Placeholder are added", () =>
		{
			AssertEquals("9 entry line fees", 9, entryLine.Fees.Count);
			AssertEquals("3 Box47Taxes items (placeholder items)", 3, lineWrapper.Box47Taxes.Count);
			AssertEquals("Placeholder[0]", "VEDI", lineWrapper.Box47Taxes[0].Box47b);
			AssertEquals("Placeholder[1]", "LISTA", lineWrapper.Box47Taxes[1].Box47b);
			AssertEquals("Placeholder[2]", "ALLEGATA", lineWrapper.Box47Taxes[2].Box47b);
		});
	}

	public void TestAttachmentFees()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var lineWrapper = ITDocSADHLine.New(entryLine, Factory);
		CombineAssertions("PRE-CONDITION", () =>
		{
			AssertEquals("No entry line fees", 0, entryLine.Fees.Count);
			AssertEquals("No AttachmentFees items", 0, lineWrapper.AttachmentFees.Count);
		});

		for (int i = 0; i < 8; i++)
		{
			entryLine.Fees.AddNew();
		}
		lineWrapper = ITDocSADHLine.New(entryLine, Factory);
		CombineAssertions("EntryLine fees fit in Box47Taxes (8 items)", () =>
		{
			AssertEquals("8 entry line fees", 8, entryLine.Fees.Count);
			AssertEquals("No AttachmentFees items", 0, lineWrapper.AttachmentFees.Count);
		});

		entryLine.Fees.AddNew();
		lineWrapper = ITDocSADHLine.New(entryLine, Factory);
		CombineAssertions("EntryLine fees don't fit in Box47Taxes (9 items). AttachmentFees are added", () =>
		{
			AssertEquals("9 entry line fees", 9, entryLine.Fees.Count);
			AssertEquals("AttachmentFees is filled", 9, lineWrapper.AttachmentFees.Count);
		});
	}

	protected override ZDecimal ExpectedStatisticalValue => 1209.80m;

	public void TestBox44_1ProducedDocumentsCertificatesForEADFirstLine()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		entryLine.UseEadAttachmentPrintingSupporter = true;

		var lineWrapper = ITDocSADHLine.New(entryLine, Factory);
		AssertEquals(nameof(ITDocSADHLine.Box44_1ProducedDocumentsCertificates), "", lineWrapper.Box44_1ProducedDocumentsCertificates);

		var supportingDocument1 = invoiceLine.SupportingDocuments.AddNew();
		var supportingDocument2 = invoiceLine.SupportingDocuments.AddNew();

		supportingDocument1.CSI_Code = "N380";
		supportingDocument1.CSI_ReferenceNumber = "A0023";
		supportingDocument1.CSI_RN_NKCountryCode = "IT";
		supportingDocument1.CSI_YearOfIssue = "2021";
		supportingDocument1.CSI_Quantity = 343.43m;

		supportingDocument2.CSI_Code = "C601";
		supportingDocument2.CSI_ReferenceNumber = "A0050";
		supportingDocument2.CSI_RN_NKCountryCode = "DE";
		supportingDocument2.CSI_YearOfIssue = "2021";
		supportingDocument2.CSI_Quantity = 200.43m;
		supportingDocument2.CSI_UnitOfQuantity = "KGM";

		lineWrapper = ITDocSADHLine.New(entryLine, Factory);
		AssertEquals(nameof(ITDocSADHLine.Box44_1ProducedDocumentsCertificates), "N380-IT-2021-A0023;C601-DE-2021-A0050-KGM-200.43", lineWrapper.Box44_1ProducedDocumentsCertificates);

		var resultinString = ZString.Empty;
		entryLine.CL_LineNumber = 1;

		var i = 0;
		while (resultinString.Length < 351)
		{
			var supportingDocument = invoiceLine.SupportingDocuments.AddNew();

			supportingDocument.CSI_Code = "N38" + i.ToString();
			supportingDocument.CSI_ReferenceNumber = "A0023";
			supportingDocument.CSI_RN_NKCountryCode = "IT";
			supportingDocument.CSI_YearOfIssue = "2021";
			supportingDocument.CSI_Quantity = 343.43m;
			supportingDocument.CSI_UnitOfQuantity = "KGM";
			supportingDocument.CSI_Quantity = i;
			supportingDocument.CSI_RX_NKCurrency = "EUR";
			supportingDocument.CSI_Value = i;

			resultinString += FormatSupportingDocumentInfo(supportingDocument);

			i++;
		}

		lineWrapper = ITDocSADHLine.New(entryLine, Factory);
		AssertEquals(nameof(ITDocSADHLine.Box44_1ProducedDocumentsCertificates), "VEDI TIPI DOCUMENTI COME DA LISTA ALLEGATA", lineWrapper.Box44_1ProducedDocumentsCertificates);
	}

	public override void TestBox44_1ProducedDocumentsCertificates()
	{
		var factory = Factory;
		var declaration = factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		entryLine.CL_LineNumber = 1;
		entryLine.UseEadAttachmentPrintingSupporter = true;
		invoiceLine.JI_CL = entryLine.PK;

		var lineWrapper = ITDocSADHLine.New(entryLine, factory);
		AssertEquals(nameof(ITDocSADHLine.Box44_1ProducedDocumentsCertificates), "", lineWrapper.Box44_1ProducedDocumentsCertificates);

		var invoiceLineSupportingDocuments = invoiceLine.SupportingDocuments;
		var invoiceHeaderSupportingDocuments = invoiceHeader.SupportingDocuments;

		AddSupportingDocument(invoiceHeaderSupportingDocuments, "N380", "IT", "2021", "A0023", "", 343.43m, "", 12m);
		AddSupportingDocument(invoiceLineSupportingDocuments, "C601", "DE", "2021", "A0050", "KGM", 200.43m, "", 4.56m);
		AddSupportingDocument(invoiceLineSupportingDocuments, "N830", "IE", "2023", "A0040", "KGO", ZDecimal.Zero, "", 0);

		lineWrapper = ITDocSADHLine.New(entryLine, factory);
		AssertEquals(nameof(ITDocSADHLine.Box44_1ProducedDocumentsCertificates), "N380-IT-2021-A0023;C601-DE-2021-A0050-KGM-200.43;N830-IE-2023-A0040-KGO-0", lineWrapper.Box44_1ProducedDocumentsCertificates);

		invoiceHeaderSupportingDocuments.RemoveAndDeleteAll();
		invoiceLineSupportingDocuments.RemoveAndDeleteAll();
		AddSupportingDocument(invoiceLineSupportingDocuments, "C601", "DE", "2021", "A0050", "KGM", 200.1234567m, "EUR", 4.5m);
		AddSupportingDocument(invoiceLineSupportingDocuments, "N830", "IE", "2023", "A0040", "KGO", ZDecimal.Zero, "EUR", 0);
		AddSupportingDocument(invoiceHeaderSupportingDocuments, "N380", "IT", "2021", "A0023", "", 343.43m, "USD", 12m);

		lineWrapper = ITDocSADHLine.New(entryLine, factory);
		AssertEquals(nameof(ITDocSADHLine.Box44_1ProducedDocumentsCertificates), "N380-IT-2021-A0023-USD-12.00;C601-DE-2021-A0050-KGM-200.123457-EUR-4.50;N830-IE-2023-A0040-KGO-0-EUR-0.00", lineWrapper.Box44_1ProducedDocumentsCertificates);
	}

	public void TestBox44_1ProducedDocumentsCertificates_AdditionalDocuments()
	{
		var factory = Factory;
		var declaration = factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		entryLine.UseEadAttachmentPrintingSupporter = true;

		var invoiceLineAdditionalInfos = invoiceLine.AdditionalInfos;
		AddAdditionalInfo(invoiceLineAdditionalInfos, "INF", "30500", "", "456");
		AddAdditionalInfo(invoice.AdditionalInfos, "REF", "Y001", "123", "");
		AddAdditionalInfo(invoiceLineAdditionalInfos, "TRA", "N722", "789", "");

		var lineWrapper = ITDocSADHLine.New(entryLine, factory);
		AssertEquals("Box44_1ProducedDocumentsCertificates with AdditionalDocuments", "30500-456;N722-789;Y001-123", lineWrapper.Box44_1ProducedDocumentsCertificates);

		invoiceLineAdditionalInfos.RemoveAndDeleteAll();

		var invoiceHeaderSupportingDocuments = invoice.SupportingDocuments;
		AddSupportingDocument(invoiceHeaderSupportingDocuments, "N380", "IT", "2021", "A0023", "", 343.43m, "", 12m);

		lineWrapper = ITDocSADHLine.New(entryLine, factory);
		AssertEquals(nameof(ITDocSADHLine.Box44_1ProducedDocumentsCertificates), "N380-IT-2021-A0023;Y001-123", lineWrapper.Box44_1ProducedDocumentsCertificates);
	}

	ZString FormatSupportingDocumentInfo(SupportingDocument supportingDocument)
	{
		return new ZStringBuilder()
			.AppendIfNotEmpty(supportingDocument.CSI_Code)
			.AppendIfNotEmpty(supportingDocument.CSI_RN_NKCountryCode)
			.AppendIfNotEmpty(supportingDocument.CSI_YearOfIssue)
			.AppendIfNotEmpty(supportingDocument.CSI_ReferenceNumber)
			.AppendIfNotEmpty(supportingDocument.CSI_UnitOfQuantity)
			.AppendIfNotEmpty(supportingDocument.CSI_Quantity.ToString("#0.#####", CultureInfo.InvariantCulture))
			.ToStringWithDelimiterBetweenAppends("-");
	}

	public void TestBox44_1ProducedDocumentsCertificatesForEADGenericLine()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		entryLine.UseEadAttachmentPrintingSupporter = true;

		var lineWrapper = ITDocSADHLine.New(entryLine, Factory);
		AssertEquals(nameof(ITDocSADHLine.Box44_1ProducedDocumentsCertificates), "", lineWrapper.Box44_1ProducedDocumentsCertificates);

		var supportingDocument1 = invoiceLine.SupportingDocuments.AddNew();
		var supportingDocument2 = invoiceLine.SupportingDocuments.AddNew();

		supportingDocument1.CSI_Code = "N380";
		supportingDocument1.CSI_ReferenceNumber = "A0023";
		supportingDocument1.CSI_RN_NKCountryCode = "IT";
		supportingDocument1.CSI_YearOfIssue = "2021";
		supportingDocument1.CSI_Quantity = 343.43m;

		supportingDocument2.CSI_Code = "C601";
		supportingDocument2.CSI_ReferenceNumber = "A0050";
		supportingDocument2.CSI_RN_NKCountryCode = "DE";
		supportingDocument2.CSI_YearOfIssue = "2021";
		supportingDocument2.CSI_Quantity = 200.43m;
		supportingDocument2.CSI_UnitOfQuantity = "LTR";

		lineWrapper = ITDocSADHLine.New(entryLine, Factory);
		AssertEquals(nameof(ITDocSADHLine.Box44_1ProducedDocumentsCertificates), "N380-IT-2021-A0023;C601-DE-2021-A0050-LTR-200.43", lineWrapper.Box44_1ProducedDocumentsCertificates);

		var resultinString = ZString.Empty;
		entryLine.CL_LineNumber = 6;

		var i = 0;
		while (resultinString.Length < 81)
		{
			var supportingDocument = invoiceLine.SupportingDocuments.AddNew();

			supportingDocument.CSI_Code = "N38" + i.ToString();
			supportingDocument.CSI_ReferenceNumber = "A0023";
			supportingDocument.CSI_RN_NKCountryCode = "IT";
			supportingDocument.CSI_YearOfIssue = "2021";
			supportingDocument.CSI_Quantity = 343.43m;

			resultinString += FormatSupportingDocumentInfo(supportingDocument);

			i++;
		}

		lineWrapper = ITDocSADHLine.New(entryLine, Factory);
		AssertEquals(nameof(ITDocSADHLine.Box44_1ProducedDocumentsCertificates), "VEDI TIPI DOCUMENTI COME DA LISTA ALLEGATA", lineWrapper.Box44_1ProducedDocumentsCertificates);
	}

	public void TestBox44_2SpecialMentions()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		CombineAssertions($"Assert {nameof(ITDocSADHLine.Box44_2SpecialMentions)} in many scenarios", () =>
		{
			var lineWrapper = ITDocSADHLine.New(entryLine, Factory);
			AssertEquals($"When Entry has no Previous Documents, {nameof(ITDocSADHLine.Box44_2SpecialMentions)}", "", lineWrapper.Box44_2SpecialMentions);

			var previousDocumentRp = invoiceLine.PreviousDocuments.AddNew();
			previousDocumentRp.CSI_Procedure = "2";
			previousDocumentRp.CSI_Tariff = "80701010";
			previousDocumentRp.CSI_Quantity = 51.45m;
			previousDocumentRp.CSI_Quantity2 = 5m;
			declaration.ResetApportionedPreviousDocuments();
			lineWrapper = ITDocSADHLine.New(entryLine, Factory);
			AssertEquals($"When Entry has only one RP Previous Document, {nameof(ITDocSADHLine.Box44_2SpecialMentions)}", "DS=80701010-51.45-5", lineWrapper.Box44_2SpecialMentions);

			var previousDocumentPa = invoiceLine.PreviousDocuments.AddNew();
			previousDocumentPa.CSI_Procedure = "MRN";
			declaration.ResetApportionedPreviousDocuments();
			lineWrapper = ITDocSADHLine.New(entryLine, Factory);
			AssertEquals($"When Entry has one RP and one PA Previous Documents, {nameof(ITDocSADHLine.Box44_2SpecialMentions)}", "DS=80701010-51.45-5", lineWrapper.Box44_2SpecialMentions);

			invoiceLine.PreviousDocuments.AddNew().CSI_Procedure = "A3";
			declaration.ResetApportionedPreviousDocuments();
			AssertEquals($"When Entry has more than two Previous Documents, {nameof(ITDocSADHLine.Box44_2SpecialMentions)}", "", lineWrapper.Box44_2SpecialMentions);
		});
	}

	protected override DocSADHLine GetSADHLineForBox47TaxesOrder()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		foreach (var taxType in TaxTypesToTestOrder)
		{
			entryLine.Fees.AddNew().CF_ChargeType = taxType;
		}
		return ITDocSADHLine.New(entryLine, Factory);
	}

	protected override ZString[] TaxTypesToTestOrder => new ZString[] { "407", "165", "A00" };
	protected override ZString[] ExpectedTaxTypesOrder => new ZString[] { "A00", "165", "407" };

	public void TestBox48DeferredPaymentNotContainingMethodOfPayment()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		var lineWrapper = ITDocSADHLine.New(entryLine, Factory);

		var defermentApprovalNumber = "XXXXXX";
		var importer = Factory.New<OrgHeader>();
		importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, defermentApprovalNumber);
		declaration.JE_OH_Importer = importer.PK;
		declaration.JE_PaymentMethod = "B";
		declaration.JE_DefermentAccountNumber = defermentApprovalNumber;
		AssertEquals($"{nameof(lineWrapper.Box48DeferredPayment)} does not contain {nameof(declaration.JE_PaymentMethod)} prefix", defermentApprovalNumber, lineWrapper.Box48DeferredPayment);
	}

	public void TestBox48DeferredPaymentShouldBeDefermentAccountNumber()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var lineWrapper = ITDocSADHLine.New(entryLine, Factory);

		declaration.JE_DefermentAccountNumber = ZString.Empty;

		AssertEquals($"{nameof(lineWrapper.Box48DeferredPayment)} should be empty id {nameof(declaration.JE_DefermentAccountNumber)} is empty", ZString.Empty, lineWrapper.Box48DeferredPayment);

		declaration.JE_DefermentAccountNumber = "ASDFGHJKLXXXXXX";

		AssertEquals($"{nameof(lineWrapper.Box48DeferredPayment)} should be {nameof(declaration.JE_DefermentAccountNumber)}", "ASDFGHJKLXXXXXX", lineWrapper.Box48DeferredPayment);
	}

	protected override DocSADHLine GetNewDocSADHLine(EU.Business.Declaration.CusEntryLine entryLine)
	{
		return ITDocSADHLine.New((ITCusEntryLine)entryLine, Factory);
	}

	protected override EU.Business.Declaration.CusEntryLine SetUpEntryLine(EU.Business.Declaration.CusEntryHeader entryHeader)
	{
		var entryLine = (ITCusEntryLine)entryHeader.MergedLines[0];
		entryLine.ZG_AdjustmentAmount = 12.4m;

		return entryLine;
	}

	protected override ZString ExpectedBox49WarehouseForC88 => "";
	protected override ZString ExpectedGrossMass => "45.35924";

	protected override EU.Business.Declaration.JobDeclaration CreateDeclarationForBox15aExportCountryTest()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		declaration.MessageVersion = "XML";
		return declaration;
	}

	protected override bool GetExportCountryFromInvoiceLine => true;

	SupportingDocument AddSupportingDocument(SupportingDocumentCollection supportingDocumentCollection, ZString typeCode, ZString countryCode, ZString year, ZString referenceNumber, ZString unitOfQuantity, ZDecimal quantity, ZString currency, ZDecimal value)
	{
		var supportingDocument = supportingDocumentCollection.AddNew();

		supportingDocument.CSI_Code = typeCode;
		supportingDocument.CSI_ReferenceNumber = referenceNumber;
		supportingDocument.CSI_RN_NKCountryCode = countryCode;
		supportingDocument.CSI_YearOfIssue = year;
		supportingDocument.CSI_Quantity = quantity;
		supportingDocument.CSI_UnitOfQuantity = unitOfQuantity;

		supportingDocument.CSI_RX_NKCurrency = currency;
		supportingDocument.CSI_Value = value;
		return supportingDocument;
	}

	void AddAdditionalInfo(
			EuAdditionalInfoCollection documentCollection,
			string subType,
			string code,
			string referenceNumber,
			string description)
	{
		var additionalDocument = documentCollection.AddNew();
		additionalDocument.CSI_SubType = subType;
		additionalDocument.CSI_Code = code;
		additionalDocument.CSI_ReferenceNumber = referenceNumber;
		additionalDocument.CSI_Description = description;
	}
}
