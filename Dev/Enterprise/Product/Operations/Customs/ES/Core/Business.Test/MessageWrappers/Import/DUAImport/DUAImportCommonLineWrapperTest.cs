using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using EntrySubStyleList = Enterprise.Customs.ES.Business.Declaration.EntrySubStyleList;

namespace Enterprise.Customs.ES.Business.Testing;

public class DUAImportCommonLineWrapperTest : WrapperHelperTest<DUAImportCommonLineWrapper>
{
	public void TestExternalPackagingType()
	{
		CombineAssertions(() =>
		{
			invoiceLine.ContainersPivot.RemoveAndDeleteAll();
			AssertEquals("Expected empty ExternalPackagingType when there are no containers", ZString.Empty, wrapper.ExternalPackagingType);

			var containerPivot = invoiceLine.ContainersPivot.AddNew();
			AssertEquals("Expected filled ExternalPackagingType when there are containers", BusinessQuantityUnit.Container, wrapper.ExternalPackagingType);

			var container = declaration.CusContainers.AddNew();
			containerPivot.C2_CO = container.PK;

			var containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "TEST";
			container.CO_RC = containerType.PK;
			AssertEquals("Expected filled ExternalPackagingType when container type hasn't ES Code", BusinessQuantityUnit.Container, wrapper.ExternalPackagingType);

			var codeMap = containerType.CodeMapCollection.AddNew();
			codeMap.RCM_RN_NKCountry = CountryCodes.Spain;
			codeMap.RCM_Code = "7";
			AssertEquals("Expected filled ExternalPackagingType when container type has ES Code", "7", wrapper.ExternalPackagingType);

			codeMap.RCM_RN_NKCountry = CountryCodes.Eritrea;
			AssertEquals("Expected filled ExternalPackagingType when container type hasn't ES Code", BusinessQuantityUnit.Container, wrapper.ExternalPackagingType);
		});
	}

	public void TestInternalPackages()
	{
		var pack1 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo1 = Factory.New<Customs.Business.BasePackage>();
		packageInfo1.CW_PackType = InternalPackage1.Type;
		pack1.CHC_CW = packageInfo1.PK;
		invoiceLine.PackagesPivot.Add(pack1);

		var pack2 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo2 = Factory.New<Customs.Business.BasePackage>();
		packageInfo2.CW_PackType = InternalPackage2.Type;
		pack2.CHC_CW = packageInfo2.PK;
		invoiceLine.PackagesPivot.Add(pack2);

		var internalPackages = wrapper.InternalPackages;
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled InternalPackages", 2, internalPackages.Count);
			AssertSame("Cached Packages", wrapper.InternalPackages, internalPackages);
		});
	}

	public void TestInternalPackagesWhenVehicles()
	{
		var vehicle = invoiceLine.Vehicles.AddNew();
		vehicle.CVH_VehicleIdentificationNumber = WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.Vin;
		var invLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		var vehicle2 = invLine2.Vehicles.AddNew();
		vehicle2.CVH_VehicleIdentificationNumber = WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.Vin;

		var pack1 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo1 = Factory.New<Customs.Business.BasePackage>();
		packageInfo1.CW_PackType = InternalPackage1.Type;
		pack1.CHC_CW = packageInfo1.PK;
		invoiceLine.PackagesPivot.Add(pack1);

		var pack2 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo2 = Factory.New<Customs.Business.BasePackage>();
		packageInfo2.CW_PackType = InternalPackage2.Type;
		pack2.CHC_CW = packageInfo2.PK;
		invoiceLine.PackagesPivot.Add(pack2);

		var internalPackages = wrapper.InternalPackages;
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled InternalPackages with vehicles and packages", 3, internalPackages.Count);
			AssertSame("Cached Packages", wrapper.InternalPackages, internalPackages);
		});
	}

	public void TestInternalPackagesWhenSamePackageType()
	{
		var pack1 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo1 = Factory.New<Customs.Business.BasePackage>();
		packageInfo1.CW_PackType = "AA";
		pack1.CHC_CW = packageInfo1.PK;
		invoiceLine.PackagesPivot.Add(pack1);

		var pack2 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo2 = Factory.New<Customs.Business.BasePackage>();
		packageInfo2.CW_PackType = "AA";
		pack2.CHC_CW = packageInfo2.PK;
		invoiceLine.PackagesPivot.Add(pack2);

		var internalPackages = wrapper.InternalPackages;
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled InternalPackages for packages with the same type only 1", 1, internalPackages.Count);
			AssertSame("Cached Packages", wrapper.InternalPackages, internalPackages);
		});
	}

	public void TestVehicles()
	{
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

		var vehiclesPack = wrapper.Vehicles;
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled Vehicles.Packages", 4, vehiclesPack.Count);
			AssertSame("Cached Vehicles", wrapper.Vehicles, vehiclesPack);
		});
	}

	public void TestOtherMeasurementUnitsCode()
	{
		CombineAssertions(() =>
		{
			invoiceLine.ZG_ExciseCode = "1A0";
			invoiceLine.JI_CustomsThirdUnitQty = WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.ThirdQtyUnitCW1;
			AssertEquals("Expected filled OtherMeasurementUnitsCode with mapped value", WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.ThirdQtyUnitCustoms, wrapper.OtherMeasurementUnitsCode);

			invoiceLine.JI_CustomsThirdUnitQty = WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.ThirdQtyUnitNotMapped;
			AssertEquals("Expected filled OtherMeasurementUnitsCode with original value because the value is not mapped", WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.ThirdQtyUnitNotMapped, wrapper.OtherMeasurementUnitsCode);

			invoiceLine.JI_CustomsThirdUnitQty = "GF";
			AssertEquals("Expected empty OtherMeasurementUnitsCode when ThirdUnitQty is GF", ZString.Empty, wrapper.OtherMeasurementUnitsCode);

			invoiceLine.JI_CustomsThirdUnitQty = "PK";
			AssertEquals("Expected empty OtherMeasurementUnitsCode when ThirdUnitQty is PK", ZString.Empty, wrapper.OtherMeasurementUnitsCode);
		});
	}

	public void TestOtherMeasurementUnitsNumber()
	{
		CombineAssertions(() =>
		{
			invoiceLine.ZG_ExciseCode = "1A0";
			invoiceLine.JI_CustomsThirdQuantity = WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.ThirdQuantity;
			AssertEquals("Expected filled OtherMeasurementUnitsNumber", WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.ThirdQuantity, wrapper.OtherMeasurementUnitsNumber);

			invoiceLine.JI_CustomsThirdUnitQty = "GF";
			AssertEquals("Expected 0 OtherMeasurementUnitsNumber when ThirdUnitQty is GF", ZDecimal.Zero, wrapper.OtherMeasurementUnitsNumber);

			invoiceLine.JI_CustomsThirdUnitQty = "PK";
			AssertEquals("Expected 0 OtherMeasurementUnitsNumber when ThirdUnitQty is PK", ZDecimal.Zero, wrapper.OtherMeasurementUnitsNumber);
		});
	}

	public void TestTariffSupplementaryCodes()
	{
		invoiceLine.JI_SupplementaryCode1 = WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.SuppCode1;
		invoiceLine.JI_SupplementaryCode2 = WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.SuppCode2;

		var tariffSupplementaryCodes = wrapper.TariffSupplementaryCodes;
		CombineAssertions(() =>
		{
			AssertArrayEqualsByElements("Expected filled TariffSupplementaryCodes", new ZString[] { WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.SuppCode1, WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.SuppCode2 }, tariffSupplementaryCodes.ToArray());
			AssertSame("Cached TariffSupplementaryCodes", wrapper.TariffSupplementaryCodes, tariffSupplementaryCodes);
		});
	}

	public void TestProductTitleForSpecialTaxes()
	{
		invoiceLine.ZG_ExciseCode = WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.ExciseCode;
		AssertEquals("Expected filled ProductTitleForSpecialTaxes", WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.ExciseCode, wrapper.ProductTitleForSpecialTaxes);

		invoiceLine.ZG_ExciseCode = "1A0";
		AssertEquals("Expected empty ProductTitleForSpecialTaxes when ExciseCode starts with 1", ZString.Empty, wrapper.ProductTitleForSpecialTaxes);
	}

	public void TestSpecialTaxesIndicator()
	{
		CombineAssertions(() =>
		{
			invoiceLine.ZG_ExciseExemption = "B";
			invoiceLine.ZG_ExciseCode = ZString.Empty;
			AssertEquals("Expected empty SpecialTaxesIndicator when ExciseCode is empty", ZString.Empty, wrapper.SpecialTaxesIndicator);

			invoiceLine.ZG_ExciseCode = "0A0";
			AssertEquals("Expected filled SpecialTaxesIndicator when ExciseCode and ExciseExemption are not empty", "B", wrapper.SpecialTaxesIndicator);

			invoiceLine.ZG_ExciseExemption = ZString.Empty;
			AssertEquals("Expected 0 SpecialTaxesIndicator when ExciseExemption is empty but ExciseCode is not", "0", wrapper.SpecialTaxesIndicator);

			invoiceLine.ZG_ExciseExemption = "B";
			invoiceLine.ZG_ExciseCode = "1A0";
			AssertEquals("Expected empty SpecialTaxesIndicator when ExciseCode starts with 1", ZString.Empty, wrapper.SpecialTaxesIndicator);
		});
	}

	public void TestGrossWeightInKG()
	{
		CombineAssertions(() =>
		{
			invoiceLine.JI_Weight = 200.4455M;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals("Expected filled GrossWeightInKG when weight > 1 rounded to the upper integer unit", 201M, wrapper.GrossWeightInKG);

			invoiceLine.JI_Weight = 0.9886M;
			AssertEquals("Expected filled GrossWeightInKG when weight < 1", 0.989M, wrapper.GrossWeightInKG);
		});
	}

	public void TestPreferenceCode()
	{
		invoiceLine.JI_PrimaryPreference = WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.PrimaryPreferenceCode;
		AssertEquals("Expected filled PreferenceCode", WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.PrimaryPreferenceCodePref, wrapper.PreferenceCode);
	}

	public void TestReductionCode()
	{
		invoiceLine.JI_PrimaryPreference = WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.PrimaryPreferenceCode;
		AssertEquals("Expected filled ReductionCode", WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.PrimaryPreferenceCodeRed, wrapper.ReductionCode);
	}

	public void TestConcessionsCPC()
	{
		CombineAssertions(() =>
		{
			invoiceLine.JI_Procedure = WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.Procedure;
			AssertArrayEqualsByElements("Expected filled ConcessionsCPC with only procedure's concession", new ZString[] { WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.ProcedureConcessionPart }, wrapper.ConcessionsCPC.ToArray());

			invoiceLine.JI_Procedure = WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.Procedure;
			invoiceLine.AdditionalProcedureCodes.AddNew(WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.AddProcedure1);
			wrapper = GetWrapper(entryLine);
			AssertArrayEqualsByElements("Expected filled GoodsCustomsProcedureCategory3 with procedure's concession + first additional concession", new ZString[] { WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.ProcedureConcessionPart, WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.Procedure1ConcessionPart }, wrapper.ConcessionsCPC.ToArray());

			invoiceLine.AdditionalProcedureCodes.AddNew(WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.AddProcedure2);
			wrapper = GetWrapper(entryLine);
			var concessionsCPC = wrapper.ConcessionsCPC;

			AssertArrayEqualsByElements("Expected filled GoodsCustomsProcedureCategory3 with procedure's concession + first additional concession + second additional concession", new ZString[] { WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.ProcedureConcessionPart, WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.Procedure1ConcessionPart, WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.Procedure2ConcessionPart }, concessionsCPC.ToArray());
			AssertSame("Cached ConcessionsCPC", wrapper.ConcessionsCPC, concessionsCPC);

			invoiceLine.JI_Procedure = WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.ProcedurePart1;
			invoiceLine.AdditionalProcedureCodes.AddNew(WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.AddProcedure1);
			invoiceLine.AdditionalProcedureCodes.AddNew(WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.AddProcedure2);
			wrapper = GetWrapper(entryLine);
			AssertArrayEqualsByElements("Expected filled GoodsCustomsProcedureCategory3 with procedure < 4 => only first additional concession + second additional concession", new ZString[] { WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.Procedure1ConcessionPart, WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.Procedure2ConcessionPart }, wrapper.ConcessionsCPC.ToArray());
		});
	}

	public void TestNetWeightInKG()
	{
		CombineAssertions(() =>
		{
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_NetWeight = 2.3454m;
			invoiceLine.JI_CustomsUnitQty = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_CustomsQuantity = 1.1234m;
			AssertEquals("Expected filled NetWeightInKG with 1 invoice line", 1.123m, wrapper.NetWeightInKG);

			var invLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			invLine2.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invLine2.JI_NetWeight = 2.3454m;
			invLine2.JI_CustomsUnitQty = Core.Constants.Weight.Kilograms;
			invLine2.JI_CustomsQuantity = 2.3211m;
			AssertEquals("Expected filled NetWeightInKG with 2 invoice line", 3.445m, wrapper.NetWeightInKG);

			var invLine3 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			invLine3.JI_NetWeightUQ = Core.Constants.Weight.Grams;
			invLine3.JI_NetWeight = 2000.000m;
			invLine3.JI_CustomsUnitQty = Core.Constants.Weight.Grams;
			invLine3.JI_CustomsQuantity = 1000.0000m;
			AssertEquals("Expected filled NetWeightInKG with 3 invoice line", 4.445m, wrapper.NetWeightInKG);
		});
	}

	public void TestContingency()
	{
		invoiceLine.JI_ConcessionOrder = WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.ContingencyCode;
		AssertEquals("Expected filled Contingency", WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.ContingencyCode, wrapper.Contingency);
	}

	public void TestPrecedentDocumentType()
	{
		var previousDoc1 = invoiceLine.PreviousDocuments.AddNew();
		previousDoc1.CSI_SubType = WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.PrevDocSubType;
		wrapper = GetWrapper(entryLine);
		AssertEquals("Expected filled PrecedentDocumentType", WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.PrevDocSubType, wrapper.PrecedentDocumentType);
	}

	public void TestPrecedentDocumentClass()
	{
		var previousDoc1 = invoiceLine.PreviousDocuments.AddNew();
		previousDoc1.CSI_Code = WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.PrevDocCode;

		wrapper = GetWrapper(entryLine);
		AssertEquals("Expected filled PrecedentDocumentClass", WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.PrevDocCode, wrapper.PrecedentDocumentClass);
	}

	public void TestPrecedentDocumentReference()
	{
		CombineAssertions(() =>
		{
			var previousDoc1 = invoiceLine.PreviousDocuments.AddNew();
			previousDoc1.CSI_ReferenceNumber = "Reference";
			previousDoc1.CSI_Code = "SUM";
			previousDoc1.CSI_LineNo = 1;
			wrapper = GetWrapper(entryLine);
			AssertEquals("For SUM previous documents reference number must have line number attached when not 0", "Reference00001", wrapper.PrecedentDocumentReference);

			previousDoc1.CSI_ReferenceNumber = "Reference2";
			previousDoc1.CSI_LineNo = 0;
			wrapper = GetWrapper(entryLine);
			AssertEquals("For SUM previous documents reference number must not have line number attached when 0", "Reference2", wrapper.PrecedentDocumentReference);

			previousDoc1.CSI_ReferenceNumber = "Reference3";
			previousDoc1.CSI_Code = "IRR";
			previousDoc1.CSI_LineNo = 5;
			wrapper = GetWrapper(entryLine);
			AssertEquals("For non SUM previous documents reference number must not have line number attached", "Reference3", wrapper.PrecedentDocumentReference);
		});
	}

	public void TestSupplementaryUnitsCode()
	{
		CombineAssertions(() =>
		{
			invoiceLine.JI_CustomsSecondUnitQty = WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.SupplQtyUnitCW1;
			AssertEquals("Expected filled SupplementaryUnitsCode with mapped value", WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.SupplQtyUnitCustoms, wrapper.SupplementaryUnitsCode);

			invoiceLine.JI_CustomsSecondUnitQty = WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.SupplQtyUnitNotMapped;
			AssertEquals("Expected filled SupplementaryUnitsCode with original value because the value is not mapped", WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.SupplQtyUnitNotMapped, wrapper.SupplementaryUnitsCode);
		});
	}

	public void TestSupplementaryUnitsNumber()
	{
		invoiceLine.JI_CustomsSecondQuantity = WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.SupplQuantity;
		AssertEquals("Expected filled SupplementaryUnitsNumber", WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.SupplQuantity, wrapper.SupplementaryUnitsNumber);
	}

	public void TestInvoiceValue()
	{
		invoiceLine.JI_LinePrice = WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.LinePrice;
		AssertEquals("Expected filled InvoiceValue", WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.LinePrice, wrapper.InvoiceValue);
	}

	public void TestDocumentsAndCertificates()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty DocumentsAndCertificates list", 0, wrapper.DocumentsAndCertificates.Count);

			var supdoc1 = declaration.SupportingDocuments.AddNew();
			supdoc1.CSI_Code = "9001";

			var supdoc2 = entryInstruction.SupportingDocuments.AddNew();
			supdoc2.CSI_Code = "9002";

			var supdoc3 = invoice.SupportingDocuments.AddNew();
			supdoc3.CSI_Code = "9003";

			var supdoc4 = invoiceLine.SupportingDocuments.AddNew();
			supdoc4.CSI_Code = "9004";

			var supdoc5 = invoiceLine.SupportingDocuments.AddNew();
			supdoc5.CSI_Code = "9005";

			entryLine.AddEntryLineDocument<SupportingDocument>("AAA", "10,00", subType: "LIQ", status: "ACC");
			entryLine.AddEntryLineDocument<SupportingDocument>("BBB", "10,00", subType: "LIQ", status: ZString.Empty);
			entryLine.AddEntryLineDocument<SupportingDocument>("CCC", "10,00", subType: "LIQ", status: "REJ");
			entryLine.AddEntryLineDocument<SupportingDocument>("DDD", "10,00", subType: ZString.Empty, status: ZString.Empty);

			wrapper = new DUAImportCommonLineWrapper(entryLine, false);
			var documents = wrapper.DocumentsAndCertificates;

			AssertEquals("Expected filled DocumentsAndCertificates (inlcuded entryLine sup docs with LIQ subtype and not ACC status)", 7, documents.Count);
			AssertSame("Cached DocumentsAndCertificates", wrapper.DocumentsAndCertificates, documents);
		});
	}

	public void TestSpecialInstructions()
	{
		var addInfo = declaration.AdditionalInfos.AddNew();
		addInfo.CSI_Code = WrapperHelperTest<DUAImportCommonLineWrapper>.SpecialInstructionsCodes[0];
		var addInfo2 = declaration.AdditionalInfos.AddNew();
		addInfo2.CSI_Code = WrapperHelperTest<DUAImportCommonLineWrapper>.SpecialInstructionsCodes[1];

		var addInfo3 = invoice.AdditionalInfos.AddNew();
		addInfo3.CSI_Code = WrapperHelperTest<DUAImportCommonLineWrapper>.SpecialInstructionsCodes[1];
		var addInfo4 = invoice.AdditionalInfos.AddNew();
		addInfo4.CSI_Code = WrapperHelperTest<DUAImportCommonLineWrapper>.SpecialInstructionsCodes[2];

		var addInfo5 = invoiceLine.AdditionalInfos.AddNew();
		addInfo5.CSI_Code = WrapperHelperTest<DUAImportCommonLineWrapper>.SpecialInstructionsCodes[2];
		var addInfo6 = invoiceLine.AdditionalInfos.AddNew();
		addInfo6.CSI_Code = WrapperHelperTest<DUAImportCommonLineWrapper>.SpecialInstructionsCodes[0];

		var specialInstructions = wrapper.SpecialInstructions;

		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder("SpecialInstructions wrapped", WrapperHelperTest<DUAImportCommonLineWrapper>.SpecialInstructionsCodes.ToArray(), specialInstructions.ToArray());
			AssertSame("Cached SpecialInstructions", wrapper.SpecialInstructions, specialInstructions);
		});
	}

	public void TestDeclaredTaxes()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty DeclaredTaxes list", 0, wrapper.DeclaredTaxes.Count);

			entryLine.Fees.AddNew();
			entryLine.Fees.AddNew();

			wrapper = GetWrapper(entryLine);
			var declaredTaxes = wrapper.DeclaredTaxes;

			AssertEquals("Expected filled DeclaredTaxes", 2, declaredTaxes.Count);
			AssertSame("Cached DeclaredTaxes", wrapper.DeclaredTaxes, declaredTaxes);
		});
	}

	public void TestTotalValue()
	{
		var fee1 = entryLine.Fees.AddNew();
		fee1.CF_ChargeAmount = WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.FeesTotalChargeAmount / 2;
		var fee2 = entryLine.Fees.AddNew();
		fee2.CF_ChargeAmount = WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.FeesTotalChargeAmount / 2;

		AssertEquals("Expected filled TotalValue", WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.FeesTotalChargeAmount, wrapper.TotalValue);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var helper = new UniversalReferenceTestDataHelper(Factory);
		var cusProcedure1 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Spain, "", WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.ProcedurePart1, WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.ProcedurePart2, WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineData.ProcedureConcessionPart, "AB DESC 1", "EXP", intoWarehouse: true, outOfWarehouse: true, group: "EFD");
		cusProcedure1.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Spain;

		helper.CreateCusMapType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, MapDirectionList.Codes.BTH, "Description", false);
		helper.CreateCusMap(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineFeeData.MethodOfCalculationCW1, WrapperHelperTest<DUAImportCommonLineWrapper>.EntryLineFeeData.MethodOfCalculationCustoms, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), CountryCodes.Spain);
		Factory.Save();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		declaration.FillWithValidTestData();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();

		AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

		entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

		wrapper = GetWrapper(entryLine);
	}

	protected JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	protected JobComInvoiceHeader invoice;
	protected JobComInvoiceLine invoiceLine;
	protected CusEntryLine entryLine;
	DUAImportCommonLineWrapper wrapper;

	protected virtual DUAImportCommonLineWrapper GetWrapper(CusEntryLine cusEntryLine) => new DUAImportCommonLineWrapper(cusEntryLine, false);

	protected override DUAImportCommonLineWrapper GetProvider() => wrapper;
}
