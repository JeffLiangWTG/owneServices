using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants.RefCusCodeList;

namespace Enterprise.Customs.ES.Business.Testing;

class ExpeditionHeaderWrapperTest : WrapperHelperTest<ExpeditionHeaderWrapper>
{
	public void ExpeditionHeaderWrapperConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => GetWrapper(null));
	}

	public void TestExpeditionCustomsOffice()
	{
		declaration.JE_CustomsOffice = HeaderData.ExpeditionCustomsOffice;
		AssertEquals("Expected filled ExpeditionCustomsOffice", HeaderData.ExpeditionCustomsOfficeCode, wrapper.ExpeditionCustomsOffice);
	}

	public void TestExpeditionCountry()
	{
		declaration.JE_RL_NKOrigin = HeaderData.ExpeditionOrigin;
		AssertEquals("Expected filled ExpeditionCountry", HeaderData.ExpeditionOriginCountry, wrapper.ExpeditionCountry);
	}

	public void TestDestinationCountry()
	{
		declaration.JE_RL_NKFinalDestination = HeaderData.ExpeditionDestination;
		AssertEquals("Expected filled DestinationCountry", HeaderData.ExpeditionDestinationCountry, wrapper.DestinationCountry);
	}

	public void TestTotalLinesNum()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected 1 TotalLinesNum (mandatory at least one)", 1, wrapper.TotalLinesNum);

			entryHeader.MergedLines.AddNew();
			entryHeader.MergedLines.AddNew();
			AssertEquals("Expected 3 TotalLinesNum", 3, wrapper.TotalLinesNum);
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

		AssertEquals("Expected filled TotalPackagesQty with the sum of all packages quantities and pieces quantities", 12, wrapper.TotalPackagesQty);
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

	public void TestConveyanceId()
	{
		CombineAssertions(() =>
		{
			declaration.JE_VesselName = "4456BGT";
			AssertEquals("Transport code ID for other", "4456BGT", wrapper.ConveyanceId);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_VesselName = "VESSEL NAME";
			AssertEquals("Transport code ID for sea", "VESSEL NAME", wrapper.ConveyanceId);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_VoyageFlightNo = "Flight";
			AssertEquals("Transport code ID for air", "Flight", wrapper.ConveyanceId);
		});
	}

	public void TestNullSender()
	{
		AssertExceptionThrown<NullReferenceException>(() => wrapper.Sender.ToString());
	}

	public void TestSender()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		declaration.SupplierDocumentaryAddress.OrganisationPK = orgHeader.PK;
		var sender = wrapper.Sender;
		CombineAssertions(() =>
		{
			AssertNotNull("Expected filled Sender", sender);
			AssertSame("Cached Sender", wrapper.Sender, sender);
		});
	}

	public void TestNullConsignee()
	{
		AssertExceptionThrown<NullReferenceException>(() => wrapper.Consignee.ToString());
	}

	public void TestConsignee()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		declaration.ImporterDocumentaryAddress.OrganisationPK = orgHeader.PK;
		var consignee = wrapper.Consignee;
		CombineAssertions(() =>
		{
			AssertNotNull("Expected filled Consignee", consignee);
			AssertSame("Cached Consignee", wrapper.Consignee, consignee);
		});
	}

	public void TestNullDeclarant()
	{
		declaration.Declarant.OA_OH = ZGuid.Empty;
		AssertExceptionThrown<NullReferenceException>(() => wrapper.Declarant.ToString());
	}

	public void TestDeclarant()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		declaration.Declarant.OA_OH = orgHeader.PK;
		var declarant = wrapper.Declarant;
		CombineAssertions(() =>
		{
			AssertNotNull("Expected filled Declarant", declarant);
			AssertSame("Cached Declarant", wrapper.Declarant, declarant);
		});
	}

	public void TestCommunications()
	{
		CombineAssertions(() =>
		{
			var communications = wrapper.Communications;
			AssertNotNull("Expected not null", communications);
			AssertSame("Cached", wrapper.Communications, communications);
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

		wrapper = GetWrapper(entryHeader);
	}

	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine;
	CusEntryHeader entryHeader;
	ExpeditionHeaderWrapper wrapper;

	protected virtual ExpeditionHeaderWrapper GetWrapper(CusEntryHeader cusEntryHeader) => new ExpeditionHeaderWrapper(cusEntryHeader);

	protected override ExpeditionHeaderWrapper GetProvider() => wrapper;
}
