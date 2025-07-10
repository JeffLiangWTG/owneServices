using System;
using System.Linq;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing;

class ExpeditionLineWrapperTest : WrapperHelperTest<ExpeditionLineWrapper>
{
	public void ExpeditionLineWrapperConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("EntryLine null", () => new ExpeditionLineWrapper(null));
			AssertExceptionThrown<ArgumentOutOfRangeException>("Empty InvoiceLines", () => new ExpeditionLineWrapper(Factory.New<CusEntryLine>()));
		});
	}

	public void TestLineNumber()
	{
		entryLine.CL_LineNumber = 2;
		AssertEquals("Expected empty LineNumber", 2, wrapper.LineNumber);
	}

	public void TestGoodsCode()
	{
		invoiceLine.JI_Tariff = EntryLineData.Tariff;
		AssertEquals("Expected filled GoodsCode", EntryLineData.TariffShort, wrapper.GoodsCode);
	}

	public void TestGoodsDescription()
	{
		invoiceLine.JI_Description = EntryLineData.GoodsDescription;
		AssertEquals("Expected filled GoodsDescription", EntryLineData.GoodsDescription, wrapper.GoodsDescription);
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

	public void TestPackages()
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

		var packages = wrapper.Packages;
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled Packages", 2, packages.Count);
			AssertSame("Cached Packages", wrapper.Packages, packages);
		});
	}

	public void TestPackagesWhenVehicles()
	{
		var vehicle = invoiceLine.Vehicles.AddNew();
		vehicle.CVH_VehicleIdentificationNumber = EntryLineData.Vin;
		var invLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		var vehicle2 = invLine2.Vehicles.AddNew();
		vehicle2.CVH_VehicleIdentificationNumber = EntryLineData.Vin;

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

		var packages = wrapper.Packages;
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled Packages with vehicles and packages", 3, packages.Count);
			AssertSame("Cached Packages", wrapper.Packages, packages);
		});
	}

	public void TestPackagesWhenSamePackageType()
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

		var packages = wrapper.Packages;
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled Packages for packages with the same type only 1", 1, packages.Count);
			AssertSame("Cached Packages", wrapper.Packages, packages);
		});
	}

	public void TestContainers()
	{
		var invoiceLine2 = invoice.InvoiceLines.AddNew();

		foreach (var tag in ContainerTags)
		{
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = tag;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(tag).IsForInvoiceLine = true;
			invoiceLine2.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(tag).IsForInvoiceLine = true;
		}

		var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge done", true, mergeResult);

		entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

		wrapper = new ExpeditionLineWrapper(entryLine);
		var containers = wrapper.Containers;
		CombineAssertions(() =>
		{
			AssertArrayEqualsByElements("Expected filled Containers", ContainerTags, containers.ToArray());
			AssertSame("Cached Containers", wrapper.Containers, containers);
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

	public void TestDocumentsSubmitted()
	{
		SetSupportingDocumentsRefData();

		declaration.SupportingDocuments.Add(GetSupportingDoc("AAA", "REF111"));
		declaration.SupportingDocuments.Add(GetSupportingDoc("N380", "REF222"));
		entryInstruction.SupportingDocuments.Add(GetSupportingDoc("D005", "REF333"));
		entryInstruction.SupportingDocuments.Add(GetSupportingDoc("D008", "REF444"));
		invoice.SupportingDocuments.Add(GetSupportingDoc("D005", "REF555"));
		invoice.SupportingDocuments.Add(GetSupportingDoc("D008", "REF666"));
		invoiceLine.SupportingDocuments.Add(GetSupportingDoc("N380", "REF777"));
		invoiceLine.SupportingDocuments.Add(GetSupportingDoc("N380", "REF888"));

		var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge done", true, mergeResult);

		entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
		wrapper = new ExpeditionLineWrapper(entryLine);
		var documentsSubmitted = wrapper.DocumentsSubmitted;

		CombineAssertions(() =>
		{
			AssertEquals("Expected filled DocumentsSubmitted", 8, documentsSubmitted.Count);
			AssertSame("Cached DocumentsSubmitted", wrapper.DocumentsSubmitted, documentsSubmitted);
		});
	}

	public void TestDocumentsSubmittedRepeated()
	{
		SetSupportingDocumentsRefData();

		CombineAssertions(() =>
		{
			AssertEquals("Expected empty Documents list", 0, wrapper.DocumentsSubmitted.Count);

			invoiceLine.SupportingDocuments.Add(GetSupportingDoc("N380", "REF111"));
			invoiceLine.SupportingDocuments.Add(GetSupportingDoc("N380", "REF111"));

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge", true, mergeResult);

			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			wrapper = new ExpeditionLineWrapper(entryLine);
			var documents = wrapper.DocumentsSubmitted;

			AssertEquals("Expected filled DocumentsSubmitted", 1, documents.Count);
			AssertSame("Cached DocumentsSubmitted", wrapper.DocumentsSubmitted, documents);
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
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.JobComInvoiceLines.AddNew();

		var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge done", true, mergeResult);

		entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

		wrapper = new ExpeditionLineWrapper(entryLine);
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	JobComInvoiceHeader invoice;
	JobComInvoiceLine invoiceLine;
	CusEntryLine entryLine;
	ExpeditionLineWrapper wrapper;

	protected override ExpeditionLineWrapper GetProvider() => wrapper;
}
