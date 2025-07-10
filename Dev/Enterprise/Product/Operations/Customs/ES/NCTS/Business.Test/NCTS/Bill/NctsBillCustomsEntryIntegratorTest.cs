using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants.RefCusCodeList;

namespace Enterprise.Customs.ES.NCTS.Business.Testing;

sealed class NctsBillCustomsEntryIntegratorTest : TestCaseWithFactory
{
	public void TestCreateN830PreviousDocumentAtGoodsItemLevel_InTransitionPeriod_CusEntryLine()
	{
		using (SetTransitionPeriod(isActive: true))
		{
			declaration.JE_MessageType = "EXP";
			entryHeader.MovementReferenceNumberSetter("MyMRN");
			var entryLine0 = entryHeader.MergedLines.AddNew();
			entryLine0.CL_LineNumber = 1;
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 2;

			customsEntryIntegrator.CopyCustomsEntry(entryHeader);
			CombineAssertions(() =>
			{
				var previousDocuments = houseConsignment.GoodsItems[0].PreviousDocuments;
				AssertEquals("PreviousDocuments Count", 1, previousDocuments.Count);

				var previousDocument = previousDocuments[0];
				AssertEquals("CSI_Code", "N830", previousDocument.CSI_Code);
				AssertEquals("CSI_ReferenceNumber", "MyMRN", previousDocument.CSI_ReferenceNumber);
				AssertEquals("CSI_ItemNumber", 0, previousDocument.CSI_ItemNumber);

				previousDocuments = houseConsignment.GoodsItems[1].PreviousDocuments;
				AssertEquals("PreviousDocuments Count", 1, previousDocuments.Count);

				previousDocument = previousDocuments[0];
				AssertEquals("CSI_Code", "N830", previousDocument.CSI_Code);
				AssertEquals("CSI_ReferenceNumber", "MyMRN", previousDocument.CSI_ReferenceNumber);
				AssertEquals("CSI_ItemNumber", 0, previousDocument.CSI_ItemNumber);
			});
		}
	}

	public void TestCopyPackagingDetailsWithoutContainer() => CombineAssertions(() =>
	{
		AddNewBasePackage(declaration, "1A", 1, "JPB001");
		AddNewBasePackage(declaration, "2A", 2, "JPB002");

		var entryLine1 = entryHeader.MergedLines.AddNew();
		var entryLine2 = entryHeader.MergedLines.AddNew();
		var invoice = declaration.Invoices.AddNew();

		var invoiceLine1 = entryLine1.InvoiceLines.AddNew();
		invoiceLine1.JI_JZ = invoice.PK;
		invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;
		invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0].PackQty = 1;
		invoiceLine1.PackagesForInvoiceLinesForBindingOnly[1].IsLinked = false;

		var invoiceLine2 = entryLine2.InvoiceLines.AddNew();
		invoiceLine2.JI_JZ = invoice.PK;
		invoiceLine2.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = false;
		invoiceLine2.PackagesForInvoiceLinesForBindingOnly[1].IsLinked = false;

		var vehicle1 = invoiceLine2.Vehicles.AddNew();
		vehicle1.CVH_VehicleIdentificationNumber = "vin1";
		vehicle1.CVH_BrandName = "peugeot";
		vehicle1.CVH_ModelName = "308";
		var vehicle2 = invoiceLine2.Vehicles.AddNew();
		vehicle2.CVH_VehicleIdentificationNumber = "vin2";
		vehicle2.CVH_BrandName = "opel";
		vehicle2.CVH_ModelName = "corsa";

		customsEntryIntegrator.CopyCustomsEntry(entryHeader);

		var goodItem = houseConsignment.GoodsItems;
		AssertEquals("Expected 1 package in the first Good Item", 1, goodItem[0].Packages.Count);
		AssertEquals("Expected 2 packages in the second Good Item", 2, goodItem[1].Packages.Count);

		AssertPackage(goodItem[0].Packages[0], "1A", 1, "JPB001");
		AssertPackageWithVehicle(goodItem[1].Packages[0], PackageType.Frame, 1, ZString.Empty, "vin1", "peugeot", "308");
		AssertPackageWithVehicle(goodItem[1].Packages[1], PackageType.Frame, 1, ZString.Empty, "vin2", "opel", "corsa");
	});

	static Customs.Business.BasePackage AddNewBasePackage(
			JobDeclaration declaration,
			ZString packType,
			ZInt packQuantity,
			ZString marksAndNos)
	{
		var package = declaration.Packages.AddNew();
		package.CW_PackType = packType;
		package.CW_PackQty = packQuantity;
		package.CW_MarksAndNos = marksAndNos;
		return package;
	}

	void AssertPackageWithVehicle(NctsPackage package, ZString packageType, ZLong packageCount, ZString marksAndNumbers, ZString vin, ZString brandName, ZString modelName)
	{
		AssertPackage(package, packageType, packageCount, marksAndNumbers);
		AssertEquals("VehicleIdentificationNumber", vin, package.B5_PackageID);
		AssertEquals("BrandName", brandName, package.B5_Brand);
		AssertEquals("ModelName", modelName, package.B5_Model);
	}

	void AssertPackage(NctsPackage package, ZString packageType, ZLong packageCount, ZString marksAndNumbers)
	{
		AssertEquals("PackageType", packageType, package.B5_UnitType);
		AssertEquals("PackageCount", packageCount, package.B5_UnitCount);
		AssertEquals("MarksAndNumbers", marksAndNumbers, package.B5_MarksAndNumbers);
	}

	static IDisposable SetTransitionPeriod(bool isActive)
		=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
			Constants.FunctionalityTypes.NCTSTransitionPeriod,
			Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
			ZDate.Today,
			isActive);

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
		houseConsignment = nctsHeader.Bills.AddNew();

		customsEntryIntegrator = new NctsBillCustomsEntryIntegrator(houseConsignment);
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;
	NctsBill houseConsignment;
	EU.NCTS.Business.INctsCustomsEntryIntegrator customsEntryIntegrator;
}
