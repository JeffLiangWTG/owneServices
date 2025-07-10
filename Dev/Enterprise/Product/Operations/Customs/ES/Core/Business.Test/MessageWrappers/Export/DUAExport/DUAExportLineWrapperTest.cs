using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.ES.Business.Testing;

public class DUAExportLineWrapperTest : WrapperHelperTest<DUAExportLineWrapper>
{
	public void TestGoodsCustomsProcedureCategory1()
	{
		CombineAssertions(() =>
		{
			invoiceLine.JI_Tariff = EntryLineData.Tariff;
			AssertEquals("Expected filled GoodsCustomsProcedureCategory1 with only tariff code", EntryLineData.Tariff, wrapper.GoodsCustomsProcedureCategory1);

			invoiceLine.JI_Tariff = EntryLineData.Tariff;
			invoiceLine.JI_SupplementaryCode1 = EntryLineData.SuppCode1;
			AssertEquals("Expected filled GoodsCustomsProcedureCategory1 with tariff + first supplementary code", EntryLineData.Tariff + EntryLineData.SuppCode1, wrapper.GoodsCustomsProcedureCategory1);

			invoiceLine.JI_Tariff = EntryLineData.Tariff;
			invoiceLine.JI_SupplementaryCode1 = EntryLineData.SuppCode1;
			invoiceLine.JI_SupplementaryCode2 = EntryLineData.SuppCode2;
			AssertEquals("Expected filled GoodsCustomsProcedureCategory1 with tariff + first supplementary code + second supplementary code", EntryLineData.Tariff + EntryLineData.SuppCode1 + EntryLineData.SuppCode2, wrapper.GoodsCustomsProcedureCategory1);
		});
	}

	public void TestGoodsCustomsProcedureCategory2()
	{
		invoiceLine.JI_Procedure = EntryLineData.Procedure;
		AssertEquals("Expected filled GoodsCustomsProcedureCategory2", EntryLineData.CPCCode, wrapper.GoodsCustomsProcedureCategory2);
	}

	protected virtual void AssertGoodsCustomsProcedureCategory3()
	{
		CombineAssertions(() =>
		{
			invoiceLine.JI_Procedure = EntryLineData.Procedure;
			AssertEquals("Expected filled GoodsCustomsProcedureCategory3 with only procedure's concession", EntryLineData.ProcedureConcessionPart, wrapper.GoodsCustomsProcedureCategory3);

			invoiceLine.JI_Procedure = EntryLineData.Procedure;
			invoiceLine.AdditionalProcedureCodes.AddNew(EntryLineData.AddProcedure1);
			AssertEquals("Expected filled GoodsCustomsProcedureCategory3 with procedure's concession + first additional concession", EntryLineData.ProcedureConcessionPart + EntryLineData.Procedure1ConcessionPart, wrapper.GoodsCustomsProcedureCategory3);

			invoiceLine.AdditionalProcedureCodes.AddNew(EntryLineData.AddProcedure2);
			AssertEquals("Expected filled GoodsCustomsProcedureCategory3 with procedure's concession + first additional concession + second additional concession", EntryLineData.ConcessionCodes, wrapper.GoodsCustomsProcedureCategory3);

			invoiceLine.JI_Procedure = EntryLineData.ProcedurePart1;
			invoiceLine.AdditionalProcedureCodes.AddNew(EntryLineData.AddProcedure1);
			invoiceLine.AdditionalProcedureCodes.AddNew(EntryLineData.AddProcedure2);
			AssertEquals("Expected filled GoodsCustomsProcedureCategory3 with procedure < 4 => only first additional concession + second additional concession", EntryLineData.Procedure1ConcessionPart + EntryLineData.Procedure2ConcessionPart, wrapper.GoodsCustomsProcedureCategory3);
		});
	}
	public void TestGoodsCustomsProcedureCategory3()
	{
		AssertGoodsCustomsProcedureCategory3();
	}

	public void TestGoodsCustomsProcedureCategory4()
	{
		CombineAssertions(() =>
		{
			invoiceLine.AdditionalSupplementaryCodes.AddNew(EntryLineData.AddSupplement1);
			AssertEquals("Expected filled GoodsCustomsProcedureCategory4 with first additional supplementary code", EntryLineData.AddSupplement1, wrapper.GoodsCustomsProcedureCategory4);

			invoiceLine.AdditionalSupplementaryCodes.AddNew(EntryLineData.AddSupplement2);
			AssertEquals("Expected filled GoodsCustomsProcedureCategory4 with first additional supplementary code + second additional supplementary code", EntryLineData.AddSupplementsCode, wrapper.GoodsCustomsProcedureCategory4);
		});
	}

	public void TestGoodsDescription()
	{
		invoiceLine.JI_Description = EntryLineData.GoodsDescription;
		AssertEquals("Expected filled GoodsDescription", EntryLineData.GoodsDescription, wrapper.GoodsDescription);
	}

	public void TestSpecialConditions()
	{
		var addInfo = declaration.AdditionalInfos.AddNew();
		addInfo.CSI_Code = AddInfos[0].Code;
		addInfo.CSI_Description = AddInfos[0].Desc;
		var specialConditions = wrapper.SpecialConditions;

		CombineAssertions(() =>
		{
			AssertEquals("SpecialConditions wrapped", AddInfos[0].Code, specialConditions.Code1);
			AssertSame("Cached SpecialConditions", wrapper.SpecialConditions, specialConditions);
		});
	}

	public void TestCountryOfOrigin()
	{
		invoiceLine.JI_CountryOfOrigin = EntryLineData.CountryOfOrigin;
		AssertEquals("Expected filled CountryOfOrigin", EntryLineData.CountryOfOrigin, wrapper.CountryOfOrigin);
	}

	public void TestStateOfOrigin()
	{
		invoiceLine.JI_StateOrRegionOfOrigin = EntryLineData.StateOfOrigin;
		AssertEquals("Expected filled StateOfOrigin", EntryLineData.StateOfOrigin, wrapper.StateOfOrigin);
	}

	public void TestSupplementaryUnitsNumber()
	{
		invoiceLine.JI_CustomsSecondQuantity = EntryLineData.SupplQuantity;
		AssertEquals("Expected filled SupplementaryUnitsNumber", EntryLineData.SupplQuantity, wrapper.SupplementaryUnitsNumber);
	}

	public void TestSupplementaryUnitsQualifier()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);

		helper.CreateCusMapType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, MapDirectionList.Codes.BTH, "Local Customs Quantity Units", true);
		helper.CreateCusMap(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, EntryLineFeeData.MethodOfCalculationCW1, EntryLineFeeData.MethodOfCalculationCustoms, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), Core.Constants.CountryCodes.Spain);
		Factory.Save();

		CombineAssertions(() =>
		{
			invoiceLine.JI_CustomsSecondUnitQty = EntryLineData.SupplQtyUnitCW1;
			AssertEquals("Expected filled SupplementaryUnitsQualifier with mapped value", EntryLineData.SupplQtyUnitCustoms, wrapper.SupplementaryUnitsQualifier);

			invoiceLine.JI_CustomsSecondUnitQty = EntryLineData.SupplQtyUnitNotMapped;
			AssertEquals("Expected filled SupplementaryUnitsQualifier with original value because the value is not mapped", EntryLineData.SupplQtyUnitNotMapped, wrapper.SupplementaryUnitsQualifier);
		});
	}

	public void TestDangerousGoodsCode()
	{
		var subs = Factory.New<UNDGSubstance>();
		subs.DG_Code = EntryLineData.DangerousGoodsCode;
		subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
		invoiceLine.UNDGs.FirstItemForBinding[0].LinkDefault(subs);
		AssertEquals("Expected filled DangerousGoodsCode", EntryLineData.DangerousGoodsCode, wrapper.DangerousGoodsCode);
	}

	public void TestExternalPackages()
	{
		var invoiceLine2 = invoice.InvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("Expected empty ExternalPackages", ZInt.Zero, wrapper.ExternalPackages.NumberOfPackages);

			foreach (string tag in ContainerTagsWithEmpty)
			{
				var container = declaration.CusContainers.AddNew();
				container.CO_ContainerNumber = tag;
				invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(tag).IsForInvoiceLine = true;
				invoiceLine2.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(tag).IsForInvoiceLine = true;
			}

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);

			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

			wrapper = new DUAExportLineWrapper(entryLine);
			var externalPackages = wrapper.ExternalPackages;

			AssertEquals("Expected filled ExternalPackages", ContainerTags.Length, externalPackages.NumberOfPackages);
			AssertSame("Cached ExternalPackages", wrapper.ExternalPackages, externalPackages);
		});
	}

	public void TestInternalPackages()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty InternalPackages.Packages list", 0, wrapper.InternalPackages.Packages.Count);

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

			wrapper = new DUAExportLineWrapper(entryLine);
			var internalPackages = wrapper.InternalPackages;

			AssertEquals("Expected filled InternalPackages.Packages", 2, internalPackages.Packages.Count);
			AssertSame("Cached InternalPackages", wrapper.InternalPackages, internalPackages);
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
			AssertEquals("Expected filled InternalPackages.Packages with vehicles and packages", 3, internalPackages.Packages.Count);
			AssertSame("Cached Packages", wrapper.InternalPackages, internalPackages);
		});
	}

	public void TestInternalPackagesWhenSamePackageType()
	{
		var pack1 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo1 = Factory.New<Customs.Business.BasePackage>();
		packageInfo1.CW_PackType = InternalPackage1.Type;
		pack1.CHC_CW = packageInfo1.PK;
		invoiceLine.PackagesPivot.Add(pack1);

		var pack2 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo2 = Factory.New<Customs.Business.BasePackage>();
		packageInfo2.CW_PackType = InternalPackage1.Type;
		pack2.CHC_CW = packageInfo2.PK;
		invoiceLine.PackagesPivot.Add(pack2);

		var internalPackages = wrapper.InternalPackages;
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled InternalPackages.Packages for packages with the same type only 1", 1, internalPackages.Packages.Count);
			AssertSame("Cached InternalPackages", wrapper.InternalPackages, internalPackages);
		});
	}

	public void TestInternalPackagesMultipleMergedLines()
	{
		var pack1 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo1 = Factory.New<Customs.Business.BasePackage>();
		packageInfo1.CW_PackType = InternalPackage1.Type;
		pack1.CHC_CW = packageInfo1.PK;
		invoiceLine.PackagesPivot.Add(pack1);
		var invLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		var pack2 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		pack2.CHC_CW = packageInfo1.PK;
		invLine2.PackagesPivot.Add(pack2);

		var pack3 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo2 = Factory.New<Customs.Business.BasePackage>();
		packageInfo2.CW_PackType = InternalPackage2.Type;
		pack3.CHC_CW = packageInfo2.PK;
		var invLine3 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		invLine3.PackagesPivot.Add(pack3);

		var internalPackages = wrapper.InternalPackages;
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled InternalPackages.Packages", 2, internalPackages.Packages.Count);
			AssertSame("Cached Packages", wrapper.InternalPackages, internalPackages);
		});
	}

	public void TestInternalPackagesMultipleMergedLinesWhenVehicles()
	{
		var vehicle = invoiceLine.Vehicles.AddNew();
		vehicle.CVH_VehicleIdentificationNumber = "VINCODE";
		var invLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		var vehicle2 = invLine2.Vehicles.AddNew();
		vehicle2.CVH_VehicleIdentificationNumber = "VINCODE";
		var invLine3 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		var vehicle3 = invLine3.Vehicles.AddNew();
		vehicle3.CVH_VehicleIdentificationNumber = "VINCODE2";

		var internalPackages = wrapper.InternalPackages;
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled InternalPackages.Packages for vehicles 1", 1, internalPackages.Packages.Count);
			AssertSame("Cached Packages", wrapper.InternalPackages, internalPackages);
		});
	}

	public void TestVehiclePackages()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty VehiclePackages.Packages list", 0, wrapper.VehiclePackages.Packages.Count);

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

			wrapper = new DUAExportLineWrapper(entryLine);
			var vehiclePackages = wrapper.VehiclePackages;

			AssertEquals("Expected filled VehiclePackages.Packages", 4, vehiclePackages.Packages.Count);
			AssertSame("Cached VehiclePackages", wrapper.VehiclePackages, vehiclePackages);
		});
	}

	public void TestDocumentReferenceNumber()
	{
		CombineAssertions(() =>
		{
			var previousDoc1 = invoiceLine.PreviousDocuments.AddNew();
			previousDoc1.CSI_ReferenceNumber = "Reference";
			previousDoc1.CSI_Code = "SUM";
			previousDoc1.CSI_LineNo = 1;
			wrapper = new DUAExportLineWrapper(entryLine);
			AssertEquals("For SUM previous documents DocumentReferenceNumber must have line number attached when not 0", "Reference00001", wrapper.DocumentReferenceNumber);

			previousDoc1.CSI_ReferenceNumber = "Reference2";
			previousDoc1.CSI_LineNo = 0;
			wrapper = new DUAExportLineWrapper(entryLine);
			AssertEquals("For SUM previous documents DocumentReferenceNumber must not have line number attached when 0", "Reference2", wrapper.DocumentReferenceNumber);

			previousDoc1.CSI_ReferenceNumber = "Reference3";
			previousDoc1.CSI_Code = "IRR";
			previousDoc1.CSI_LineNo = 5;
			wrapper = new DUAExportLineWrapper(entryLine);
			AssertEquals("For non SUM previous documents DocumentReferenceNumber must not have line number attached", "Reference3", wrapper.DocumentReferenceNumber);
		});
	}

	public void TestDocumentTypeCode()
	{
		var previousDoc1 = invoiceLine.PreviousDocuments.AddNew();
		previousDoc1.CSI_SubType = EntryLineData.PrevDocSubType;
		wrapper = new DUAExportLineWrapper(entryLine);
		AssertEquals("Expected filled DocumentTypeCode", EntryLineData.PrevDocSubType, wrapper.DocumentTypeCode);
	}

	public void TestDocuments()
	{
		SetSupportingDocumentsRefData();

		CombineAssertions(() =>
		{
			AssertEquals("Expected empty Documents list", 0, wrapper.Documents.Count);

			declaration.SupportingDocuments.Add(GetSupportingDoc("AAA", "REF111"));
			declaration.SupportingDocuments.Add(GetSupportingDoc("N380", "REF222"));
			declaration.SupportingDocuments.Add(GetSupportingDoc("N325", "REF333"));
			entryInstruction.SupportingDocuments.Add(GetSupportingDoc("D005", "REF444"));
			entryInstruction.SupportingDocuments.Add(GetSupportingDoc("D008", "REF555"));
			entryInstruction.SupportingDocuments.Add(GetSupportingDoc("N935", "REF666"));
			invoice.SupportingDocuments.Add(GetSupportingDoc("D005", "REF777"));
			invoice.SupportingDocuments.Add(GetSupportingDoc("D008", "REF888"));
			invoice.SupportingDocuments.Add(GetSupportingDoc("N935", "REF999"));
			invoiceLine.SupportingDocuments.Add(GetSupportingDoc("1001", "REF100"));
			invoiceLine.SupportingDocuments.Add(GetSupportingDoc("1003", "REF200"));
			invoiceLine.SupportingDocuments.Add(GetSupportingDoc("1004", "REF300"));

			var supdocEntry1 = GetSupportingDoc("1003", "REF888");
			supdocEntry1.CSI_ParentID = entryLine.PK;
			supdocEntry1.CSI_ParentTableCode = entryLine.TablePrefix;

			wrapper = new DUAExportLineWrapper(entryLine);
			var documents = wrapper.Documents;

			AssertEquals("Expected filled Documents", 12, documents.Count);
			AssertSame("Cached Documents", wrapper.Documents, documents);
		});
	}

	public void TestDocumentsSubmittedRepeated()
	{
		SetSupportingDocumentsRefData();

		CombineAssertions(() =>
		{
			AssertEquals("Expected empty Documents list", 0, wrapper.Documents.Count);

			invoiceLine.SupportingDocuments.Add(GetSupportingDoc("N380", "REF111"));
			invoiceLine.SupportingDocuments.Add(GetSupportingDoc("N380", "REF111"));

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge", true, mergeResult);

			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			wrapper = new DUAExportLineWrapper(entryLine);
			var documents = wrapper.Documents;

			AssertEquals("Expected filled Documents", 1, documents.Count);
			AssertEquals("Expected only document in Documents has merged quantity 20", 20.0M, documents.First().Quantity);
			AssertSame("Cached Documents", wrapper.Documents, documents);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.FillWithValidTestData();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();

		var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge", true, mergeResult);

		entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

		wrapper = GetWrapper(entryLine);
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	JobComInvoiceHeader invoice;
	protected JobComInvoiceLine invoiceLine;
	CusEntryLine entryLine;
	protected DUAExportLineWrapper wrapper;

	protected virtual DUAExportLineWrapper GetWrapper(CusEntryLine entryLine) => new DUAExportLineWrapper(entryLine);

	protected override DUAExportLineWrapper GetProvider() => GetWrapper(entryLine);
}
