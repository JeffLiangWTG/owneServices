using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants.RefCusCodeList;

namespace Enterprise.Customs.ES.Business.Testing;

public class ReceptionHeaderWrapperTest : WrapperHelperTest<ReceptionHeaderWrapper>
{
	public void TestReceptionCustomsOffice()
	{
		declaration.JE_CustomsOffice = HeaderData.ExpeditionCustomsOffice;
		AssertEquals("Expected filled ExpeditionCustomsOffice", HeaderData.ExpeditionCustomsOfficeCode, wrapper.ReceptionCustomsOffice);

		declaration.JE_CustomsOffice = "ES00";
		AssertEquals("Expected 4 characters ExpeditionCustomsOffice", "ES00", wrapper.ReceptionCustomsOffice);
	}

	public void TestReceptionT2LReference()
	{
		entryHeader.MovementReferenceNumber = "Test";
		AssertEquals("Expected filled ReceptionT2LReference", "Test", wrapper.ReceptionT2LReference);
	}

	public void TestExpeditionDate()
	{
		entryHeader.MovementReferenceNumberIssueDate = ZDateTime.Today;
		AssertEquals("Expected filled ExpeditionDate", ZDateTime.Today.Date, wrapper.ExpeditionDate);
	}

	public void TestExpeditionCountry()
	{
		declaration.JE_RL_NKOrigin = "ESMAD";
		AssertEquals("Expected filled ExpeditionCountry", "ES", wrapper.ExpeditionCountry);
	}

	public void TestTotalLinesNum()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected 1 TotalNumberOfGoods (mandatory at least one)", 1, wrapper.TotalLinesNum);

			entryHeader.MergedLines.AddNew();
			entryHeader.MergedLines.AddNew();
			AssertEquals("Expected 3 TotalNumberOfGoods", 3, wrapper.TotalLinesNum);
		});
	}

	public void TestTotalPackagesQty()
	{
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

		AssertEquals("Expected filled TotalNumberOfPackageElements with full packages", 12, wrapper.TotalPackagesQty);
	}

	public void TestTotalNumberOfPackageElements_WithVehicles()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty TotalNumberOfPackageElements", 0, wrapper.TotalPackagesQty);

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

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var vehicle2 = invoiceLine2.Vehicles.AddNew();
			invoiceLine2.JI_Tariff = "2203001011";
			vehicle2.CVH_VehicleIdentificationNumber = "VIN1";

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			var vehicle3 = invoiceLine3.Vehicles.AddNew();
			invoiceLine3.JI_Tariff = "2203001012";
			vehicle3.CVH_VehicleIdentificationNumber = "VIN2";

			var invoiceLine4 = declaration.InvoiceLines.AddNew();
			var vehicle4 = invoiceLine4.Vehicles.AddNew();
			invoiceLine4.JI_Tariff = "2203001012";
			vehicle4.CVH_VehicleIdentificationNumber = "VIN2";

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge", true, mergeResult);

			AssertEquals("Expected filled TotalPackagesQty with the sum of all packages quantities + the amount of bulk packages", 12, wrapper.TotalPackagesQty);
		});
	}

	public void TestTotalNumberOfPackageElements_OnlyVehicles()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty TotalNumberOfPackageElements", 0, wrapper.TotalPackagesQty);

			invoiceLine.JI_Tariff = "2203001011";
			var vehicle = invoiceLine.Vehicles.AddNew();
			vehicle.CVH_VehicleIdentificationNumber = "VIN1";

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var vehicle2 = invoiceLine2.Vehicles.AddNew();
			invoiceLine2.JI_Tariff = "2203001011";
			vehicle2.CVH_VehicleIdentificationNumber = "VIN2";

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			var vehicle3 = invoiceLine3.Vehicles.AddNew();
			invoiceLine3.JI_Tariff = "2203001012";
			vehicle3.CVH_VehicleIdentificationNumber = "VIN3";

			var invoiceLine4 = declaration.InvoiceLines.AddNew();
			var vehicle4 = invoiceLine4.Vehicles.AddNew();
			invoiceLine4.JI_Tariff = "2203001012";
			vehicle4.CVH_VehicleIdentificationNumber = "VIN3";

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge", true, mergeResult);

			AssertEquals("Expected filled TotalNumberOfPackageElements with full packages", 4, wrapper.TotalPackagesQty);
		});
	}

	public void TestContainersIndicator()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected false ContainersIndicator", false, wrapper.ContainersIndicator);

			var containerTag = "CONTAINER";
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = containerTag;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(containerTag).IsForInvoiceLine = true;
			AssertEquals("Expected true ContainersIndicator", true, wrapper.ContainersIndicator);
		});
	}

	public void TestNullDeclarant()
	{
		declaration.Declarant.OA_OH = ZGuid.Empty;
		AssertExceptionThrown<NullReferenceException>(() => wrapper.Declarant.ToString());
	}

	public void TestDeclarant()
	{
		CombineAssertions(() =>
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			declaration.Declarant.OA_OH = orgHeader.PK;
			var declarant = wrapper.Declarant;
			AssertNotNull("Expected filled Declarant", declarant);
			AssertSame("Cached Declarant", wrapper.Declarant, declarant);
		});
	}

	public void TestConstructorValidation()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown("Constructor Throws Exception if entryHeader is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","entryHeader"), () => new ReceptionHeaderWrapper(null));

			var entryHeader = Factory.New<CusEntryHeader>();
			AssertExceptionThrown("Constructor Throws Exception if jobDeclaration is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","Declaration"), () => new ReceptionHeaderWrapper(entryHeader));
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

		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.InvoiceLines.AddNew();

		var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge done", true, mergeResult);

		entryHeader = declaration.CustomsEntryHeaders[0];

		wrapper = new ReceptionHeaderWrapper(entryHeader);
	}

	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine;
	CusEntryHeader entryHeader;
	ReceptionHeaderWrapper wrapper;

	protected override ReceptionHeaderWrapper GetProvider() => wrapper;
}
