using System;
using System.Linq;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing;

class ClearanceLineWrapperTest : WrapperHelperTest<ClearanceLineWrapper>
{
	public void Constructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("EntryLine null", () => new ClearanceLineWrapper(null));
			AssertExceptionThrown<ArgumentOutOfRangeException>("Empty InvoiceLines", () => new ClearanceLineWrapper(Factory.New<CusEntryLine>()));
		});
	}

	public void TestLineNumber()
	{
		entryLine.CL_LineNumber = 2;
		AssertEquals("Expected empty LineNumber", 2, wrapper.LineNumber);
	}

	public void TestSummaryDeclaration()
	{
		CombineAssertions(() =>
		{
			var previousDoc = invoiceLine.PreviousDocuments.AddNew();
			previousDoc.CSI_ReferenceNumber = "Reference";
			previousDoc.CSI_Code = "SUM";
			previousDoc.CSI_LineNo = 1;
			wrapper = new ClearanceLineWrapper(entryLine);
			AssertEquals("For SUM previous documents SummaryDeclaration must have line number attached when not 0", "Reference00001", wrapper.SummaryDeclaration);

			previousDoc.CSI_ReferenceNumber = "Reference2";
			previousDoc.CSI_LineNo = 0;
			wrapper = new ClearanceLineWrapper(entryLine);
			AssertEquals("For SUM previous documents SummaryDeclaration must not have line number attached when 0", "Reference2", wrapper.SummaryDeclaration);

			previousDoc.CSI_ReferenceNumber = "Reference3";
			previousDoc.CSI_Code = "IRR";
			previousDoc.CSI_LineNo = 5;
			wrapper = new ClearanceLineWrapper(entryLine);
			AssertEquals("For non SUM previous documents SummaryDeclaration must not have line number attached", "Reference3", wrapper.SummaryDeclaration);
		});
	}
	public void TestLineNumberReferenced()
	{
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		invoiceLine.JI_CEI = entryInstruction.PK;

		entryLine.CL_LineNumber = 30;
		invoiceLine.ZG_T2LItemNumber = 20;

		entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.T2C;
		AssertEquals("Expected filled LineNumberReferenced when entry instructions = T2C", 20, wrapper.LineNumberReferenced);

		entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.T2L;
		AssertEquals("Expected filled LineNumberReferenced when entry instructions = T2L", 30, wrapper.LineNumberReferenced);
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

	public void TestPackageQty_OnlyPackages()
	{
		var pack1 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo1 = Factory.New<Customs.Business.BasePackage>();
		packageInfo1.CW_PackType = InternalPackage1.Type;
		pack1.CHC_CW = packageInfo1.PK;
		pack1.CHC_NumberOfPacks = 4;
		invoiceLine.PackagesPivot.Add(pack1);

		var pack2 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo2 = Factory.New<Customs.Business.BasePackage>();
		packageInfo2.CW_PackType = InternalPackage2.Type;
		pack2.CHC_CW = packageInfo2.PK;
		pack2.CHC_NumberOfPacks = 5;
		invoiceLine.PackagesPivot.Add(pack2);

		AssertEquals("Expected filled PackageQty", 9, wrapper.PackageQty);
	}

	public void TestPackageQty_OnlyVehicles()
	{
		var vehicle = invoiceLine.Vehicles.AddNew();
		vehicle.CVH_VehicleIdentificationNumber = "VinCode";
		var vehicle1 = invoiceLine.Vehicles.AddNew();
		vehicle1.CVH_VehicleIdentificationNumber = "VinCode1";

		var invLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		var vehicle2 = invLine2.Vehicles.AddNew();
		vehicle2.CVH_VehicleIdentificationNumber = "VinCode2";

		var invLine3 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		var vehicle3 = invLine3.Vehicles.AddNew();
		vehicle3.CVH_VehicleIdentificationNumber = "VinCode3";

		var invLine4 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		var vehicle4 = invLine4.Vehicles.AddNew();
		vehicle4.CVH_VehicleIdentificationNumber = "VinCode3";

		AssertEquals("Expected filled PackageQty", 5, wrapper.PackageQty);
	}

	public void TestPackageQty_VehiclesAndPackages()
	{
		var pack1 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo1 = Factory.New<Customs.Business.BasePackage>();
		packageInfo1.CW_PackType = InternalPackage1.Type;
		pack1.CHC_CW = packageInfo1.PK;
		pack1.CHC_NumberOfPacks = 4;
		invoiceLine.PackagesPivot.Add(pack1);

		var pack2 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo2 = Factory.New<Customs.Business.BasePackage>();
		packageInfo2.CW_PackType = InternalPackage2.Type;
		pack2.CHC_CW = packageInfo2.PK;
		pack2.CHC_NumberOfPacks = 5;
		invoiceLine.PackagesPivot.Add(pack2);

		var invLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		var vehicle2 = invLine2.Vehicles.AddNew();
		vehicle2.CVH_VehicleIdentificationNumber = "VinCode";

		var invLine3 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		var vehicle3 = invLine3.Vehicles.AddNew();
		vehicle3.CVH_VehicleIdentificationNumber = "VinCode2";

		var invLine4 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		var vehicle4 = invLine4.Vehicles.AddNew();
		vehicle4.CVH_VehicleIdentificationNumber = "VinCode2";

		AssertEquals("Expected filled PackageQty", 12, wrapper.PackageQty);
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

		wrapper = new ClearanceLineWrapper(entryLine);
		var containers = wrapper.Containers;
		CombineAssertions(() =>
		{
			AssertArrayEqualsByElements("Expected filled Containers", ContainerTags, containers.ToArray());
			AssertSame("Cached Containers", wrapper.Containers, containers);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.FillWithValidTestData();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.JobComInvoiceLines.AddNew();

		var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge done", true, mergeResult);

		entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

		wrapper = new ClearanceLineWrapper(entryLine);
	}

	JobDeclaration declaration;
	JobComInvoiceHeader invoice;
	JobComInvoiceLine invoiceLine;
	CusEntryLine entryLine;
	ClearanceLineWrapper wrapper;

	protected override ClearanceLineWrapper GetProvider() => wrapper;
}
