using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using NUnit.Framework;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants.RefCusCodeList;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(ComplXDVDSendMessageWrapper))]
	public class ComplXDVDSendMessageWrapperTest : DVDCommonSendMessageWrapperAbstractTest<ComplXDVDSendMessageWrapper>
	{
		public void TestMRN()
		{
			entryHeader.MovementReferenceNumber = "MRNNumber";
			AssertEquals("Expected filled MRN with DVD previf", "DVDMRNNumber", wrapper.MRN);
		}

		public void TestTotalPackages_NoVehicles()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty TotalPackages", 0, wrapper.TotalPackages);

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

				AssertEquals("Expected filled TotalPackages with full packages", 12, wrapper.TotalPackages);
			});
		}

		public void TestTotalPackages_WithVehicles()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty TotalPackages", 0, wrapper.TotalPackages);

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

				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				var vehicle2 = invoiceLine2.Vehicles.AddNew();
				invoiceLine2.JI_Tariff = "2203001011";
				vehicle2.CVH_VehicleIdentificationNumber = "VIN1";

				var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
				var vehicle3 = invoiceLine3.Vehicles.AddNew();
				invoiceLine3.JI_Tariff = "2203001012";
				vehicle3.CVH_VehicleIdentificationNumber = "VIN2";

				var invoiceLine4 = invoiceHeader.InvoiceLines.AddNew();
				var vehicle4 = invoiceLine4.Vehicles.AddNew();
				invoiceLine4.JI_Tariff = "2203001012";
				vehicle4.CVH_VehicleIdentificationNumber = "VIN2";

				var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				AssertEquals("Merge", true, mergeResult);

				var entryHeader = declaration.CustomsEntryHeaders[0];
				wrapper = GetWrapper(entryHeader, Certificate);

				AssertEquals("Expected filled TotalPackages with full packages", 15, wrapper.TotalPackages);
			});
		}

		public void TestTotalGrossMass()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
				invoiceLine.JI_Weight = 0.9886M;
				AssertEquals("Expected filled TotalGrossMass when weight < 1", 0.989M, wrapper.TotalGrossMass);

				invoiceLine.JI_Weight = 200.4455m;
				AssertEquals("Weight > 1 not rounded to the upper integer unit but JI_Weight is mark as 3 decimal places", 200.446m, wrapper.TotalGrossMass);

				var entryLine2 = entryHeader.MergedLines.AddNew();
				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_WeightUQ = Core.Constants.Weight.Kilograms;
				invoiceLine2.JI_Weight = 2.3211m;
				invoiceLine2.JI_CL = entryLine2.PK;
				AssertEquals("Expected filled TotalGrossMass when weight > 1 not rounded to the upper integer unit with 2 invoice lines", 202.767m, wrapper.TotalGrossMass);
			});
		}

		public void TestDeclarantAndRepresentative()
		{
			var declarantAndRepresentative = wrapper.DeclarantAndRepresentative;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled DeclarantAndRepresentative", declarantAndRepresentative);
				AssertSame("Cached DeclarantAndRepresentative", wrapper.DeclarantAndRepresentative, declarantAndRepresentative);
			});
		}

		public void TestLines()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected 1 Line (mandatory at least one)", 1, wrapper.Lines.Count);

				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_Tariff = "2203001011";

				var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine3.JI_Tariff = "2203001012";

				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

				entryHeader = declaration.CustomsEntryHeaders[0];
				wrapper = GetWrapper(entryHeader, Certificate);

				var lines = wrapper.Lines;

				AssertEquals("Expected 3 Lines", 3, lines.Count);
				AssertSame("Cached Lines", wrapper.Lines, lines);
			});
		}

		protected override ComplXDVDSendMessageWrapper GetWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) => new ComplXDVDSendMessageWrapper(cusEntryHeader, certificateData);
	}
}
