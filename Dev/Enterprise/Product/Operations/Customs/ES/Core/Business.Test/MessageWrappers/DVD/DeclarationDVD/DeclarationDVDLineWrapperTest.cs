using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.Business.Testing;

public class DeclarationDVDLineWrapperTest : WrapperHelperTest<DeclarationDVDLineWrapper>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown("Constructor Throws Exception if entryLine is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","entryLine"), () => GetWrapper(null));

			var entryLine = Factory.New<CusEntryLine>();
			AssertExceptionThrown("Constructor Throws Exception if InvoiceLines is null", typeof(ArgumentOutOfRangeException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value '0' cannot be less than or equal to 0.","InvoiceLines"), () => GetWrapper(entryLine));
		});
	}

	public void TestLineNumber()
	{
		entryLine.CL_LineNumber = 3;
		AssertEquals("Expected filled LineNumber", "3", wrapper.LineNumber);
	}

	public void TestSupportingDocuments()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty SupportingDocuments list", 0, wrapper.SupportingDocuments.Count);

			var supdoc1 = declaration.SupportingDocuments.AddNew();
			supdoc1.CSI_Code = "9001";

			var supdoc2 = entryInstruction.SupportingDocuments.AddNew();
			supdoc2.CSI_Code = "9002";

			var supdoc3 = invoiceHeader.SupportingDocuments.AddNew();
			supdoc3.CSI_Code = "9003";

			var supdoc4 = invoiceLine.SupportingDocuments.AddNew();
			supdoc4.CSI_Code = "9004";

			var supdoc5 = invoiceLine.SupportingDocuments.AddNew();
			supdoc5.CSI_Code = "9005";

			var supdoc6 = invoiceLine.SupportingDocuments.AddNew();
			supdoc6.CSI_Code = "9006";

			var invLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			var supdoc7 = invLine2.SupportingDocuments.AddNew();
			supdoc7.CSI_Code = "9004";

			entryLine.AddEntryLineDocument<SupportingDocument>("AAA", "10,00", subType: "LIQ", status: "ACC");
			entryLine.AddEntryLineDocument<SupportingDocument>("BBB", "10,00", subType: "LIQ", status: ZString.Empty);
			entryLine.AddEntryLineDocument<SupportingDocument>("CCC", "10,00", subType: "LIQ", status: "REJ");
			entryLine.AddEntryLineDocument<SupportingDocument>("DDD", "10,00", subType: ZString.Empty, status: ZString.Empty);

			wrapper = GetWrapper(entryLine);
			var documents = wrapper.SupportingDocuments;
			AssertEquals("Expected filled SupportingDocuments (only included those in invoiceHeader, invoiceLine and entryLine not ACC and LIQ)", 6, documents.Count);
			AssertSame("Cached SupportingDocuments", wrapper.SupportingDocuments, documents);
		});
	}

	public void TestAdditionalInfos()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty AdditionalInfos list", 0, wrapper.AdditionalInfos.Count);

			var supdoc1 = declaration.AdditionalInfos.AddNew();
			supdoc1.CSI_Code = "9001";

			var supdoc2 = entryInstruction.AdditionalInfos.AddNew();
			supdoc2.CSI_Code = "9002";

			var supdoc3 = invoiceHeader.AdditionalInfos.AddNew();
			supdoc3.CSI_Code = "9003";

			var supdoc4 = invoiceLine.AdditionalInfos.AddNew();
			supdoc4.CSI_Code = "9004";

			var supdoc5 = invoiceLine.AdditionalInfos.AddNew();
			supdoc5.CSI_Code = "9005";

			wrapper = GetWrapper(entryLine);
			var documents = wrapper.AdditionalInfos;
			AssertEquals("Expected filled AdditionalInfos (only included those in declaration, invoiceHeader and invoiceLine)", 4, documents.Count);
			AssertSame("Cached AdditionalInfos", wrapper.AdditionalInfos, documents);
		});
	}

	public void TestAdditionalSupplyActors()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty AdditionalSupplyActors when no data declared", 0, wrapper.AdditionalSupplyActors.Count);

			entryInstruction.CusSupplyChainActorReferences.AddNew();
			invoiceLine.CusSupplyChainActorReferences.AddNew();
			wrapper = GetWrapper(entryLine, shouldDeclareAddSupplyActorsInLine: true);
			var additionalSupplyActors = wrapper.AdditionalSupplyActors;
			AssertEquals("Expected filled AdditionalSupplyActors when shouldDeclareAddSupplyActorsInLine is true", 2, additionalSupplyActors.Count);
			AssertSame("Cached AdditionalSupplyActors", wrapper.AdditionalSupplyActors, additionalSupplyActors);

			wrapper = GetWrapper(entryLine, shouldDeclareAddSupplyActorsInLine: false);
			AssertEquals("Expected empty AdditionalSupplyActors when shouldDeclareAddSupplyActorsInLine is false", 0, wrapper.AdditionalSupplyActors.Count);
		});
	}

	public void TestDescription()
	{
		invoiceLine.JI_Description = "description";
		AssertEquals("Expected filled Description", "description", wrapper.Description);
	}

	public void TestCusCode()
	{
		invoiceLine.ZG_CusNumber = "0111001-6";
		AssertEquals("Expected filled CusCode", "0111001-6", wrapper.CusCode);
	}

	public void TestTariffCode()
	{
		invoiceLine.JI_Tariff = "2203001011";
		AssertEquals("Expected filled TariffCode", "22030010", wrapper.TariffCode);
	}

	public void TestTariffCodeCombined()
	{
		invoiceLine.JI_Tariff = "2203001011";
		AssertEquals("Expected filled TariffCodeCombined", "11", wrapper.TariffCodeCombined);
	}

	public void TestTariffAdditionalCodes()
	{
		invoiceLine.JI_SupplementaryCode1 = "First";
		invoiceLine.JI_SupplementaryCode2 = "Second";
		invoiceLine.AdditionalSupplementaryCodes.AddNew("AA");
		invoiceLine.AdditionalSupplementaryCodes.AddNew("BB");

		CombineAssertions(() =>
		{
			var tariffAdditionalCodes = wrapper.TariffAdditionalCodes;
			AssertArrayEqualsByElements("Expected filled TariffAdditionalCodes with SupplementatyCodes 1 and 2 when both are filled", new ZString[] { "First", "Second" }, tariffAdditionalCodes.ToArray());
			AssertSame("Cached TariffAdditionalCodes", wrapper.TariffAdditionalCodes, tariffAdditionalCodes);

			invoiceLine.JI_SupplementaryCode2 = ZString.Empty;
			wrapper = GetWrapper(entryLine);
			AssertArrayEqualsByElements("Expected filled TariffAdditionalCodes with SupplementaryCode1 and first additional supplementary code when SupplementaryCode2 is empty", new ZString[] { "First", "AA" }, wrapper.TariffAdditionalCodes.ToArray());

			invoiceLine.JI_SupplementaryCode1 = ZString.Empty;
			wrapper = GetWrapper(entryLine);
			AssertArrayEqualsByElements("Expected filled TariffAdditionalCodes with first and second additional supplementary codes when SupplementatyCodes 1 and 2 are empty", new ZString[] { "AA", "BB" }, wrapper.TariffAdditionalCodes.ToArray());
		});
	}

	public void TestNationalAdditionalCodes()
	{
		invoiceLine.ZG_ExciseCode = "101";
		invoiceLine.ZG_ExciseExemption = "B";

		var nationalAdditionalCodes = wrapper.NationalAdditionalCodes;
		CombineAssertions(() =>
		{
			AssertArrayEqualsByElements("Expected filled NationalAdditionalCodes", new ZString[] { "101B" }, nationalAdditionalCodes.ToArray());
			AssertSame("Cached NationalAdditionalCodes", wrapper.NationalAdditionalCodes, nationalAdditionalCodes);
		});
	}

	public void TestPreferenceCode()
	{
		invoiceLine.JI_PrimaryPreference = "123";
		AssertEquals("Expected filled PreferenceCode", "1", wrapper.PreferenceCode);
	}

	public void TestReductionCode()
	{
		invoiceLine.JI_PrimaryPreference = "123";
		AssertEquals("Expected filled ReductionCode", "23", wrapper.ReductionCode);
	}

	public void TestTaxes()
	{
		entryLine.CL_CustomsValue = 200.45m;
		CombineAssertions(() =>
		{
			var taxes = wrapper.Taxes;
			AssertEquals("Expected filled Taxes with at least one element, even when there are no fees declared", 1, taxes.Count);
			AssertSame("Cached Taxes", wrapper.Taxes, taxes);
			var taxesList = wrapper.Taxes.ToList();
			AssertEquals("Expected first Taxes element to only have BaseAmount", 200.45m, taxesList[0].BaseAmount);
			AssertEquals("Expected first Taxes element to have empty BaseUnit", ZString.Empty, taxesList[0].BaseUnit);
			AssertEquals("Expected first Taxes element to have 0 BaseQuantity", ZDecimal.Zero, taxesList[0].BaseQuantity);

			var fee1 = entryLine.Fees.AddNew();
			fee1.CF_MethodOfCalculation = EntryLineFeeData.MethodOfCalculationPercent;
			fee1.CF_BaseValue = 42.560m;

			var fee2 = entryLine.Fees.AddNew();
			fee2.CF_MethodOfCalculation = "KGM";
			fee2.CF_BaseValue = 25.874m;

			var fee3 = entryLine.Fees.AddNew();
			fee3.CF_MethodOfCalculation = "AAA";
			fee3.CF_BaseValue = 30.963m;

			wrapper = GetWrapper(entryLine);
			taxesList = wrapper.Taxes.ToList();
			AssertEquals("Expected filled Taxes with all fees with method of calculation not % + the baseAmount element", 3, taxesList.Count);
			AssertEquals("Expected first Taxes element to only have BaseAmount", 200.45m, taxesList[0].BaseAmount);
			AssertEquals("Expected first Taxes element to have empty BaseUnit", ZString.Empty, taxesList[0].BaseUnit);
			AssertEquals("Expected first Taxes element to have 0 BaseQuantity", ZDecimal.Zero, taxesList[0].BaseQuantity);

			AssertEquals("Expected second Taxes element to have 0 BaseAmount", ZDecimal.Zero, taxesList[1].BaseAmount);
			AssertEquals("Expected second Taxes element to have filled BaseUnit", "KGM", taxesList[1].BaseUnit);
			AssertEquals("Expected second Taxes element to have filled BaseQuantity", 25.874m, taxesList[1].BaseQuantity);

			AssertEquals("Expected third Taxes element to have 0 BaseAmount", ZDecimal.Zero, taxesList[2].BaseAmount);
			AssertEquals("Expected third Taxes element to have filled BaseUnit", "AAA", taxesList[2].BaseUnit);
			AssertEquals("Expected third Taxes element to have filled BaseQuantity", 30.963m, taxesList[2].BaseQuantity);
		});
	}

	public void TestNetMass()
	{
		CombineAssertions(() =>
		{
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_NetWeight = 2.3454m;
			invoiceLine.JI_CustomsUnitQty = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_CustomsQuantity = 1.1234m;
			AssertEquals("Expected filled NetMass with 1 invoice line", 1.1234m, wrapper.NetMass);

			var invLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			invLine2.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invLine2.JI_NetWeight = 2.3454m;
			invLine2.JI_CustomsUnitQty = Core.Constants.Weight.Kilograms;
			invLine2.JI_CustomsQuantity = 2.3211m;
			AssertEquals("Expected filled NetMass with 2 invoice lines", 3.4445m, wrapper.NetMass);

			var invLine3 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			invLine3.JI_NetWeightUQ = Core.Constants.Weight.Grams;
			invLine3.JI_NetWeight = 2000.000m;
			invLine3.JI_CustomsUnitQty = Core.Constants.Weight.Grams;
			invLine3.JI_CustomsQuantity = 1000.0000m;
			AssertEquals("Expected filled NetMass with 3 invoice lines", 4.4445m, wrapper.NetMass);
		});
	}

	public void TestGrossMass()
	{
		CombineAssertions(() =>
		{
			invoiceLine.JI_Weight = 200.4455M;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals("Weight > 1 not rounded to the upper integer unit but JI_Weight is mark as 3 decimal places", 200.446M, wrapper.GrossMass);

			invoiceLine.JI_Weight = 0.9886M;
			AssertEquals("Expected filled GrossMass when weight < 1", 0.989M, wrapper.GrossMass);
		});
	}

	public void TestSupplementaryUnitsQty()
	{
		CombineAssertions(() =>
		{
			invoiceLine.JI_CustomsSecondQuantity = 1.1234m;
			AssertEquals("Expected filled SupplementaryUnitsQty with 1 invoice line", 1.1234m, wrapper.SupplementaryUnitsQty);

			var invLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			invLine2.JI_CustomsSecondQuantity = 2.3211m;
			AssertEquals("Expected filled SupplementaryUnitsQty with 2 invoice lines", 3.4445m, wrapper.SupplementaryUnitsQty);

			var invLine3 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			invLine3.JI_CustomsSecondQuantity = 1.0000m;
			AssertEquals("Expected filled SupplementaryUnitsQty with 3 invoice lines", 4.4445m, wrapper.SupplementaryUnitsQty);
		});
	}

	public void TestContainers()
	{
		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();

		foreach (var tag in ContainerTagsWithEmpty)
		{
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = tag;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(tag).IsForInvoiceLine = true;
			invoiceLine2.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(tag).IsForInvoiceLine = true;
		}

		var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge done", true, mergeResult);

		entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

		wrapper = GetWrapper(entryLine);
		var containers = wrapper.Containers;
		CombineAssertions(() =>
		{
			AssertArrayEqualsByElements("Expected filled Containers", ContainerTags, containers.ToArray());
			AssertSame("Cached Containers", wrapper.Containers, containers);
		});
	}

	public void TestCountryOfDestination()
	{
		CombineAssertions(() =>
		{
			invoiceLine.ZG_CountryOfDestination = "FR";
			wrapper = GetWrapper(entryLine, shouldDeclareCountryOfDestinationInLine: true);
			AssertEquals("Expected filled CountryOfDestination when shouldDeclareCountryOfDestinationInLine is true", "FR", wrapper.CountryOfDestination);

			wrapper = GetWrapper(entryLine, shouldDeclareCountryOfDestinationInLine: false);
			AssertEquals("Expected empty CountryOfDestination when shouldDeclareCountryOfDestinationInLine is false", ZString.Empty, wrapper.CountryOfDestination);
		});
	}

	public void TestCountryOfExport()
	{
		CombineAssertions(() =>
		{
			invoiceLine.ZG_CountryOfSupply = "ES";
			wrapper = GetWrapper(entryLine, shouldDeclareCountryOfExportInLine: true);
			AssertEquals("Expected filled CountryOfExport when shouldDeclareCountryOfExportInLine is true", "ES", wrapper.CountryOfExport);

			wrapper = GetWrapper(entryLine, shouldDeclareCountryOfExportInLine: false);
			AssertEquals("Expected empty CountryOfExport when shouldDeclareCountryOfExportInLine is false", ZString.Empty, wrapper.CountryOfExport);
		});
	}

	public void TestRequestedCPC()
	{
		invoiceLine.JI_Procedure = "1049123";
		AssertEquals("Expected filled RequestedCPC", "10", wrapper.RequestedCPC);
	}

	public void TestPreviousCPC()
	{
		invoiceLine.JI_Procedure = "1049123";
		AssertEquals("Expected filled PreviousCPC", "49", wrapper.PreviousCPC);
	}

	public void TestAdditionalProcedures()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty AdditionalProcedures when no data declared", 0, wrapper.AdditionalProcedures.Count);

			invoiceLine.JI_Procedure = "1049123";
			wrapper = GetWrapper(entryLine);
			var additionalProcedures = wrapper.AdditionalProcedures;
			AssertEquals("Expected filled AdditionalProcedures with only JI_Procedure declared count 1", 1, additionalProcedures.Count);
			AssertSame("Cached AdditionalProcedures", wrapper.AdditionalProcedures, additionalProcedures);

			var additionalProceduresList = additionalProcedures.ToList();
			AssertEquals("Expected filled AdditionalProcedures with only JI_Procedure declared, EUCode empty when code starts with number", ZString.Empty, additionalProceduresList[0].EUCode);
			AssertEquals("Expected filled AdditionalProcedures with only JI_Procedure declared, NationalCode fileed when code starts with number", "123", additionalProceduresList[0].NationalCode);

			invoiceLine.AdditionalProcedureCodes.AddNew("456F89");
			invoiceLine.AdditionalProcedureCodes.AddNew("789100");
			invoiceLine.AdditionalProcedureCodes.AddNew("789101");
			invoiceLine.AdditionalProcedureCodes.AddNew("789A12");
			invoiceLine.AdditionalProcedureCodes.AddNew("789103");
			wrapper = GetWrapper(entryLine);
			additionalProceduresList = wrapper.AdditionalProcedures.ToList();
			AssertEquals("Expected filled AdditionalProcedures with JI_Procedure and additionalCodes declared count 5", 5, additionalProceduresList.Count);

			AssertEquals("Expected first filled AdditionalProcedures with JI_Procedure and additionalCodes declared, EUCode empty when code starts with number", ZString.Empty, additionalProceduresList[0].EUCode);
			AssertEquals("Expected first filled AdditionalProcedures with JI_Procedure and additionalCodes declared, NationalCode filled when code starts with number", "123", additionalProceduresList[0].NationalCode);

			AssertEquals("Expected second filled AdditionalProcedures with JI_Procedure and additionalCodes declared, EUCode filled when code starts with letter", "F89", additionalProceduresList[1].EUCode);
			AssertEquals("Expected second filled AdditionalProcedures with JI_Procedure and additionalCodes declared, NationalCode empty when code starts with letter", ZString.Empty, additionalProceduresList[1].NationalCode);

			AssertEquals("Expected third filled AdditionalProcedures with JI_Procedure and additionalCodes declared, EUCode empty when code starts with number", ZString.Empty, additionalProceduresList[2].EUCode);
			AssertEquals("Expected third filled AdditionalProcedures with JI_Procedure and additionalCodes declared, NationalCode filled when code starts with number", "100", additionalProceduresList[2].NationalCode);

			AssertEquals("Expected fourth filled AdditionalProcedures with JI_Procedure and additionalCodes declared, EUCode empty when code starts with number", ZString.Empty, additionalProceduresList[3].EUCode);
			AssertEquals("Expected fourth filled AdditionalProcedures with JI_Procedure and additionalCodes declared, NationalCode filled when code starts with number", "101", additionalProceduresList[3].NationalCode);

			AssertEquals("Expected fifth filled AdditionalProcedures with JI_Procedure and additionalCodes declared, EUCode filled when code starts with letter", "A12", additionalProceduresList[4].EUCode);
			AssertEquals("Expected fifth filled AdditionalProcedures with JI_Procedure and additionalCodes declared, NationalCode empty when code starts with letter", ZString.Empty, additionalProceduresList[4].NationalCode);
		});
	}

	public void TestCountryOfOrigin()
	{
		invoiceLine.JI_CountryOfOrigin = "DE";
		AssertEquals("Expected filled CountryOfOrigin", "DE", wrapper.CountryOfOrigin);
	}

	public void TestPackages()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty Packages list", 0, wrapper.Packages.Count);

			var pack1 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
			var packageInfo1 = Factory.New<Customs.Business.BasePackage>();
			packageInfo1.CW_PackType = "CT";
			packageInfo1.CW_MarksAndNos = "marks";
			pack1.CHC_CW = packageInfo1.PK;
			pack1.CHC_NumberOfPacks = 9;
			invoiceLine.PackagesPivot.Add(pack1);

			var pack2 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
			var packageInfo2 = Factory.New<Customs.Business.BasePackage>();
			packageInfo2.CW_PackType = "NE";
			packageInfo2.CW_MarksAndNos = "marks2";
			pack2.CHC_CW = packageInfo2.PK;
			pack2.CHC_NumberOfPacks = 4;
			invoiceLine.PackagesPivot.Add(pack2);

			var pack3 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
			var packageInfo3 = Factory.New<Customs.Business.BasePackage>();
			packageInfo3.CW_PackType = RefCusCodeList.PackageType.Frame;
			packageInfo3.CW_MarksAndNos = "marks frame";
			pack3.CHC_CW = packageInfo3.PK;
			pack3.CHC_NumberOfPacks = 3;
			invoiceLine.PackagesPivot.Add(pack3);

			wrapper = GetWrapper(entryLine);
			var packages = wrapper.Packages;

			AssertEquals("Expected filled Packages (only non FR ones)", 2, packages.Count);
			AssertSame("Cached Packages", wrapper.Packages, packages);

			var packagesList = packages.ToList();

			AssertEquals("For first package expected filled PackageType", "CT", packagesList[0].PackageType);
			AssertEquals("For first package expected filled Marks", "marks", packagesList[0].Marks);
			AssertEquals("For first package expected filled NumberOfPackages", 9, packagesList[0].NumberOfPackages);

			AssertEquals("For second package expected filled PackageType", "NE", packagesList[1].PackageType);
			AssertEquals("For second package expected filled Marks", "marks2", packagesList[1].Marks);
			AssertEquals("For second package expected filled NumberOfPackages", 4, packagesList[1].NumberOfPackages);
		});
	}

	public void TestVehicles()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty Vehicles list", 0, wrapper.Vehicles.Count);

			var vehicle = invoiceLine.Vehicles.AddNew();
			vehicle.CVH_VehicleIdentificationNumber = "VINCODE";
			var vehicleX = invoiceLine.Vehicles.AddNew();
			vehicleX.CVH_VehicleIdentificationNumber = "VINCODEX";

			var invLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			var vehicle2 = invLine2.Vehicles.AddNew();
			vehicle2.CVH_VehicleIdentificationNumber = "VINCODE";

			var invLine3 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			var vehicle3 = invLine3.Vehicles.AddNew();
			vehicle3.CVH_VehicleIdentificationNumber = "VINCODE2";

			wrapper = GetWrapper(entryLine);
			var vehiclesTot = wrapper.Vehicles;

			AssertEquals("Expected filled Vehicles", 4, vehiclesTot.Count);
			AssertSame("Cached Vehicles", wrapper.Vehicles, vehiclesTot);
		});
	}

	public void TestPreviousDocuments()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty PreviousDocuments list", 0, wrapper.PreviousDocuments.Count);

			var prevdoc1 = declaration.PreviousDocuments.AddNew();
			prevdoc1.CSI_Code = "9001";

			var prevdoc2 = invoiceHeader.PreviousDocuments.AddNew();
			prevdoc2.CSI_Code = "9002";

			var prevdoc3 = invoiceLine.PreviousDocuments.AddNew();
			prevdoc3.CSI_Code = "9003";

			var prevdoc4 = invoiceLine.PreviousDocuments.AddNew();
			prevdoc4.CSI_Code = "9004";

			wrapper = GetWrapper(entryLine, shouldDeclarePreviousDocumentsInLine: true);
			var documents = wrapper.PreviousDocuments;
			AssertEquals("Expected filled PreviousDocuments when shouldDeclarePreviousDocumentsInLine is true (all docuemnts)", 4, documents.Count);
			AssertSame("Cached PreviousDocuments", wrapper.PreviousDocuments, documents);

			wrapper = GetWrapper(entryLine, shouldDeclarePreviousDocumentsInLine: false);
			AssertEquals("Expected empty PreviousDocuments when shouldDeclarePreviousDocumentsInLine is false", 0, wrapper.PreviousDocuments.Count);
		});
	}

	public void TestUCRReferenceNumber()
	{
		CombineAssertions(() =>
		{
			invoiceLine.ZG_CommercialReference = "reference";
			AssertEquals("Expected empty UCRReferenceNumber when shouldDeclareUCRInLine flag is false", ZString.Empty, wrapper.UCRReferenceNumber);

			wrapper = GetWrapper(entryLine, shouldDeclareUCRInLine: true);
			AssertEquals("Expected filled UCRReferenceNumber when shouldDeclareUCRInLine flag is true", "reference", wrapper.UCRReferenceNumber);
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
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.InvoiceLines.AddNew();

		var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge", true, mergeResult);

		entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

		wrapper = GetWrapper(entryLine);
	}
	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	JobComInvoiceHeader invoiceHeader;
	JobComInvoiceLine invoiceLine;
	CusEntryLine entryLine;
	DeclarationDVDLineWrapper wrapper;

	DeclarationDVDLineWrapper GetWrapper(CusEntryLine entryLine, bool shouldDeclareUCRInLine = false, bool shouldDeclareAddSupplyActorsInLine = false, bool shouldDeclareCountryOfDestinationInLine = false, bool shouldDeclareCountryOfExportInLine = false, bool shouldDeclarePreviousDocumentsInLine = false) => new DeclarationDVDLineWrapper(entryLine, shouldDeclareUCRInLine, shouldDeclareAddSupplyActorsInLine, shouldDeclareCountryOfDestinationInLine, shouldDeclareCountryOfExportInLine, shouldDeclarePreviousDocumentsInLine);

	protected override DeclarationDVDLineWrapper GetProvider() => wrapper;
}
