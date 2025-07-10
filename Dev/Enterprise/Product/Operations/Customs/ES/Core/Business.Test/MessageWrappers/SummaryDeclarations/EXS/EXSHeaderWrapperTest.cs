using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants.RefCusCodeList;

namespace Enterprise.Customs.ES.Business.Testing;

class EXSHeaderWrapperTest : WrapperHelperTest<EXSHeaderWrapper>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null Entry Header", () => GetWrapper(null, "aa"));
			AssertExceptionThrown<ArgumentNullException>("No Declaration", () => GetWrapper(Factory.New<CusEntryHeader>(), "aa"));
		});
	}

	public void TestReferenceNumber()
	{
		entryHeader.CH_BGMReference = HeaderData.MovementReferenceNumber;
		AssertEquals("Expected filled ReferenceNumber", HeaderData.MovementReferenceNumber, wrapper.ReferenceNumber);
	}

		public void TestGoodsLocation()
		{
			CombineAssertions(() =>
			{
				entryInstruction.GoodsLocation.Address.AuthorisationNumber = "AAAA";
				AssertEquals("Expected filled ReferenceNumber short", "AAAA", wrapper.GoodsLocation);
				entryInstruction.GoodsLocation.Address.AuthorisationNumber = "AAAAAAAAAA";
				AssertEquals("Expected filled ReferenceNumber long", "AAAAAAAAAA", wrapper.GoodsLocation);
				entryInstruction.GoodsLocation.Address.AuthorisationNumber = "ES00AAAAAAAAAAA";
				AssertEquals("Expected filled ReferenceNumber long > 10 ", "AAAAAAAAAAA", wrapper.GoodsLocation);
				entryInstruction.GoodsLocation.Address.AuthorisationNumber = "";
				AssertEquals("Expected empty ReferenceNumber when not declared", ZString.Empty, wrapper.GoodsLocation);
			});
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
		Factory.SetBulkTypeHelper();
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
		var package4 = declaration.Packages.AddNew();
		package4.CW_CR_HouseContainer = billPackingGroup.PK;
		package4.CW_PackType = "VG";

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

		var linkPackage4 = collection1.AddNew();
		linkPackage4.Package = package4;
		linkPackage4.IsLinked = true;
		linkPackage4.PackQty = 2;

		AssertEquals("Expected filled TotalPackagesQty with the sum of all packages quantities + the amount of bulk packages", 13, wrapper.TotalPackagesQty);
	}

	public void TestTotalNumberOfPackageElements_WithVehicles()
	{
		Factory.SetBulkTypeHelper();
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
			var package4 = declaration.Packages.AddNew();
			package4.CW_CR_HouseContainer = billPackingGroup.PK;
			package4.CW_PackType = "VG";

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

			var linkPackage4 = collection1.AddNew();
			linkPackage4.Package = package4;
			linkPackage4.IsLinked = true;
			linkPackage4.PackQty = 2;

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

			AssertEquals("Expected filled TotalPackagesQty with the sum of all packages quantities + the amount of bulk packages", 16, wrapper.TotalPackagesQty);
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

	public void TestTotalGrossWeight()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entryHeader1.MergedLines.AddNew();
		var entryLine2 = entryHeader1.MergedLines.AddNew();

		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		invoiceLine1.JI_WeightUQ = Core.Constants.Weight.Grams;
		invoiceLine1.JI_Weight = 100000.00m;
		invoiceLine1.JI_CL = entryLine1.PK;

		var invoiceLine2 = invoice1.InvoiceLines.AddNew();
		invoiceLine2.JI_WeightUQ = Core.Constants.Weight.Milligrams;
		invoiceLine2.JI_Weight = 200000000.00m;
		invoiceLine2.JI_CL = entryLine1.PK;

		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine3 = invoice2.InvoiceLines.AddNew();
		invoiceLine3.JI_WeightUQ = Core.Constants.Weight.Hectograms;
		invoiceLine3.JI_Weight = 2000.00m;
		invoiceLine3.JI_CL = entryLine2.PK;

		var invoiceLine4 = invoice2.InvoiceLines.AddNew();
		invoiceLine4.JI_WeightUQ = Core.Constants.Weight.Kilograms;
		invoiceLine4.JI_Weight = 500.60m;
		invoiceLine4.JI_CL = entryLine2.PK;

		wrapper = GetWrapper(entryHeader1);

		AssertEquals("Expected filled TotalGrossWeight with the sum of all GrossWeight quantities", 1000.6m, wrapper.TotalGrossWeight);
	}

	public void TestDeclarationDate()
	{
		AssertEquals("Expected filled ReferenceNumber", ZDateTime.Now, wrapper.DeclarationDate);
	}

	public void TestDeclarationPlace()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected DeclarationPlace without NKCusAgent", ZString.Empty, wrapper.DeclarationPlace);

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "AAA";
			staff.GS_UserAddress1 = "BBBBBBBBB";
			staff.GS_City = "MADRID";
			declaration.JE_GS_NKCusAgent = staff.GS_Code;
			AssertEquals("Expected DeclarationPlace with NKCusAgent", "MADRID", wrapper.DeclarationPlace);

			declaration.JE_GS_NKCusAgent = "JJ";
			AssertEquals("Expected DeclarationPlace with NKCusAgent is a wrong code", ZString.Empty, wrapper.DeclarationPlace);

			var staff1 = Factory.New<GlbStaff>();
			staff.GS_Code = "BBB";
			declaration.JE_GS_NKCusAgent = staff1.GS_Code;
			AssertEquals("Expected when cusAgent's address is not set", ZString.Empty, wrapper.DeclarationPlace);
		});
	}

	public void TestSpecificCircumstanceInd()
	{
		CombineAssertions(() =>
		{
			declaration.ZG_SpecificCircumstanceIndicator = ZString.Empty;
			AssertEquals("Expected Specific Circumstance Indicator empty", ZString.Empty, wrapper.SpecificCircumstanceInd);
			declaration.ZG_SpecificCircumstanceIndicator = "A";
			AssertEquals("Expected Specific Circumstance Indicator A", "A", wrapper.SpecificCircumstanceInd);
			declaration.ZG_SpecificCircumstanceIndicator = "B";
			AssertEquals("Expected Specific Circumstance Indicator B", "B", wrapper.SpecificCircumstanceInd);
			declaration.ZG_SpecificCircumstanceIndicator = "E";
			AssertEquals("Expected Specific Circumstance Indicator E", "E", wrapper.SpecificCircumstanceInd);
			declaration.ZG_SpecificCircumstanceIndicator = "A20";
			AssertEquals("Expected Specific Circumstance Indicator A20", ZString.Empty, wrapper.SpecificCircumstanceInd);
		});
	}

	public void TestNatSpecificCircumstanceInd()
	{
		AssertEquals("Expected Specific Circumstance Indicator", ZString.Empty, wrapper.NatSpecificCircumstanceInd);
	}

	public void TestDocumentOperationIndicator()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected Document Operation Indicator without subType", ZString.Empty, wrapper.DocumentOperationIndicator);

			wrapper = GetWrapper(entryHeader, messageSubType: DeclarationMessageSubTypeList.Codes.Amendment);
			AssertEquals("Expected Document Operation Indicator with subType" + DeclarationMessageSubTypeList.Codes.Amendment, "M", wrapper.DocumentOperationIndicator);

			wrapper = GetWrapper(entryHeader, messageSubType: DeclarationMessageSubTypeList.Codes.Cancellation);
			AssertEquals("Expected Document Operation Indicator with subType" + DeclarationMessageSubTypeList.Codes.Cancellation, "A", wrapper.DocumentOperationIndicator);
		});
	}

	public void TestDocumentReferenceNumber()
	{
		CombineAssertions(() =>
		{
			entryHeader.MovementReferenceNumber = "ES2233333333355";

			wrapper = GetWrapper(entryHeader, messageSubType: DeclarationMessageSubTypeList.Codes.Amendment);
			AssertEquals("Expected Document Reference Number with subType" + DeclarationMessageSubTypeList.Codes.Amendment, "ES2233333333355", wrapper.DocumentReferenceNumber);

			wrapper = GetWrapper(entryHeader);
			AssertEquals("Expected Document Reference Number without subType", ZString.Empty, wrapper.DocumentReferenceNumber);

			wrapper = GetWrapper(entryHeader, messageSubType: DeclarationMessageSubTypeList.Codes.Cancellation);
			AssertEquals("Expected Document Reference Number with subType" + DeclarationMessageSubTypeList.Codes.Cancellation, "ES2233333333355", wrapper.DocumentReferenceNumber);
		});
	}

	public void TestMethodOfPayment()
	{
		CombineAssertions(() =>
		{
			wrapper = GetWrapper(entryHeader, uniqueDeclareMopInHeader: ZString.Empty);
			AssertEquals("Expected MethodOfPayment when send empty", ZString.Empty, wrapper.MethodOfPayment);
			wrapper = GetWrapper(entryHeader, uniqueDeclareMopInHeader: "A");
			AssertEquals("Expected MethodOfPayment when send A", "A", wrapper.MethodOfPayment);
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
			entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;

		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.InvoiceLines.AddNew();

		invoiceLine.JI_CEI = entryInstruction.PK;

		var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge done", true, mergeResult);

		entryHeader = declaration.CustomsEntryHeaders[0];

		wrapper = GetWrapper(entryHeader);
	}

		protected CusEntryInstruction entryInstruction;
		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
		CusEntryHeader entryHeader;
		EXSHeaderWrapper wrapper;

	EXSHeaderWrapper GetWrapper(CusEntryHeader cusEntryHeader, string uniqueDeclareMopInHeader = null, string messageSubType = null) => new EXSHeaderWrapper(cusEntryHeader, uniqueDeclareMopInHeader, messageSubType);

	protected override EXSHeaderWrapper GetProvider() => wrapper;
}
