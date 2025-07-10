using System;
using System.Linq;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.Business.Testing;

public class ComplXDVDLineWrapperTest : WrapperHelperTest<ComplXDVDLineWrapper>
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
		AssertEquals("Expected filled LineNumber", 3, wrapper.LineNumber);
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

			AssertEquals("Expected filled Packages", 3, packages.Count);
			AssertSame("Cached Packages", wrapper.Packages, packages);

			var packagesList = packages.ToList();

			AssertEquals("For first package expected filled PackageType", "CT", packagesList[0].PackageType);
			AssertEquals("For first package expected filled Marks", "marks", packagesList[0].Marks);
			AssertEquals("For first package expected filled NumberOfPackages", 9, packagesList[0].NumberOfPackages);

			AssertEquals("For second package expected filled PackageType", "NE", packagesList[1].PackageType);
			AssertEquals("For second package expected filled Marks", "marks2", packagesList[1].Marks);
			AssertEquals("For second package expected empty NumberOfPackages", 0, packagesList[1].NumberOfPackages);

			AssertEquals("For third package expected filled PackageType", "FR", packagesList[2].PackageType);
			AssertEquals("For third package expected filled Marks", "marks frame", packagesList[2].Marks);
			AssertEquals("For third package expected filled NumberOfPackages", 3, packagesList[2].NumberOfPackages);
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

	public void TestDepositUnitOfMeasureCodeEU()
	{
		invoiceLine.JI_CustomsThirdUnitQty = "KGM";
		AssertEquals("Expected filled DepositUnitOfMeasureCodeEU", "KGM", wrapper.DepositUnitOfMeasureCodeEU);
	}

	public void TestDepositUnitOfMeasureQuantity()
	{
		CombineAssertions(() =>
		{
			invoiceLine.JI_CustomsThirdQuantity = 1.1234m;
			AssertEquals("Expected filled DepositUnitOfMeasureQuantity with 1 invoice line", 1.123m, wrapper.DepositUnitOfMeasureQuantity);

			var invLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			invLine2.JI_CustomsThirdQuantity = 2.3211m;
			AssertEquals("Expected filled DepositUnitOfMeasureQuantity with 2 invoice lines", 3.445m, wrapper.DepositUnitOfMeasureQuantity);

			var invLine3 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			invLine3.JI_CustomsThirdQuantity = 1.0000m;
			AssertEquals("Expected filled DepositUnitOfMeasureQuantity with 3 invoice lines", 4.445m, wrapper.DepositUnitOfMeasureQuantity);
		});
	}

	public void TestGrossMassKg()
	{
		CombineAssertions(() =>
		{
			invoiceLine.JI_Weight = 200.4455M;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals("Weight > 1 not rounded to the upper integer unit but JI_Weight is mark as 3 decimal places", 200.446m, wrapper.GrossMassKg);

			invoiceLine.JI_Weight = 0.9886M;
			AssertEquals("Expected filled GrossMass when weight < 1", 0.989M, wrapper.GrossMassKg);
		});
	}

	public void TestNetMassKg()
	{
		CombineAssertions(() =>
		{
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_NetWeight = 2.3454m;
			invoiceLine.JI_CustomsUnitQty = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_CustomsQuantity = 1.1234m;
			AssertEquals("Expected filled NetMass with 1 invoice line", 1.123m, wrapper.NetMassKg);

			var invLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			invLine2.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invLine2.JI_NetWeight = 2.3454m;
			invLine2.JI_CustomsUnitQty = Core.Constants.Weight.Kilograms;
			invLine2.JI_CustomsQuantity = 2.3211m;
			AssertEquals("Expected filled NetMass with 2 invoice lines", 3.445m, wrapper.NetMassKg);

			var invLine3 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			invLine3.JI_NetWeightUQ = Core.Constants.Weight.Grams;
			invLine3.JI_NetWeight = 2000.000m;
			invLine3.JI_CustomsUnitQty = Core.Constants.Weight.Grams;
			invLine3.JI_CustomsQuantity = 1000.0000m;
			AssertEquals("Expected filled NetMass with 3 invoice lines", 4.445m, wrapper.NetMassKg);
		});
	}

	public void TestSupplementaryQuantity()
	{
		CombineAssertions(() =>
		{
			invoiceLine.JI_CustomsSecondQuantity = 1.1234m;
			AssertEquals("Expected filled SupplementaryQuantity with 1 invoice line", 1.123m, wrapper.SupplementaryQuantity);

			var invLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			invLine2.JI_CustomsSecondQuantity = 2.3211m;
			AssertEquals("Expected filled SupplementaryQuantity with 2 invoice lines", 3.445m, wrapper.SupplementaryQuantity);

			var invLine3 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			invLine3.JI_CustomsSecondQuantity = 1.0000m;
			AssertEquals("Expected filled SupplementaryQuantity with 3 invoice lines", 4.445m, wrapper.SupplementaryQuantity);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		var declaration = Factory.New<JobDeclaration>();
		declaration.FillWithValidTestData();
		declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.InvoiceLines.AddNew();

		var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge", true, mergeResult);

		entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

		wrapper = GetWrapper(entryLine);
	}
	JobComInvoiceLine invoiceLine;
	CusEntryLine entryLine;
	ComplXDVDLineWrapper wrapper;

	ComplXDVDLineWrapper GetWrapper(CusEntryLine entryLine) => new ComplXDVDLineWrapper(entryLine);

	protected override ComplXDVDLineWrapper GetProvider() => wrapper;
}
