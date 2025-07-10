using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Documents.CMR.Testing
{
	sealed class CMRWayBillWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestSupplierAddress()
		{
			var wrapper = GetNewWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.SupplierAddress, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "SupplierAddress");

			var supplier = Factory.New<OrgHeader>();
			declaration.JE_OH_Supplier = supplier.PK;
			supplier.OH_FullName = "Freds Supply Co";
			supplier.MainAddress.OA_Address1 = "367 George St";
			supplier.MainAddress.OA_City = "Sydney";
			supplier.MainAddress.OA_State = "NSW";
			supplier.MainAddress.OA_PostCode = "2000";
			supplier.MainAddress.OA_RN_NKCountryCode = "AU";
			declaration.JE_OH_Supplier = supplier.PK;

			wrapper = GetNewWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.SupplierAddress, NUnit.Framework.Is.EqualTo("FREDS SUPPLY CO\r\n367 GEORGE ST\r\nSYDNEY NSW 2000\r\nAUSTRALIA").Using(CustomComparers.TypeComparison), "SupplierAddress");
		}

		[ExpectNoExceptions]
		public void TestImporterAddress()
		{
			var wrapper = GetNewWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.ImporterAddress, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "ImporterAddress");

			var importer = Factory.New<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			importer.OH_FullName = "Freds Supply Co";
			importer.MainAddress.OA_Address1 = "367 George St";
			importer.MainAddress.OA_City = "Sydney";
			importer.MainAddress.OA_State = "NSW";
			importer.MainAddress.OA_PostCode = "2000";
			importer.MainAddress.OA_RN_NKCountryCode = "AU";
			declaration.JE_OH_Importer = importer.PK;

			wrapper = GetNewWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.ImporterAddress, NUnit.Framework.Is.EqualTo("FREDS SUPPLY CO\r\n367 GEORGE ST\r\nSYDNEY NSW 2000\r\nAUSTRALIA").Using(CustomComparers.TypeComparison), "ImporterAddress");
		}

		[ExpectNoExceptions]
		public void TestInternationalConsignementNote()
		{
			declaration.JE_MasterBill = "THEMASTERBILL";
			declaration.JE_VesselName = "THEVESSELNAME";

			declaration.JE_TransportMode = string.Empty;
			var wrapper = GetNewWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.InternationalConsignmentNote, NUnit.Framework.Is.EqualTo("THEVESSELNAME").Using(CustomComparers.TypeComparison), "When JE_TransportMode is empty InternationalConsignementNote is taken from JE_VesselName");

			declaration.JE_TransportMode = "ROA";
			declaration.JE_MasterBill = "THEMASTERBILL";
			declaration.JE_VesselName = "THEVESSELNAME";
			wrapper = GetNewWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.InternationalConsignmentNote, NUnit.Framework.Is.EqualTo("THEVESSELNAME").Using(CustomComparers.TypeComparison), "When transport mode is not AIR or SEA, InternationalConsignementNote is taken from JE_VesselName");

			declaration.JE_TransportMode = "SEA";
			declaration.JE_MasterBill = "THEMASTERBILL";
			declaration.JE_VesselName = "THEVESSELNAME";
			wrapper = GetNewWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.InternationalConsignmentNote, NUnit.Framework.Is.EqualTo("THEMASTERBILL").Using(CustomComparers.TypeComparison), "When transport mode is AIR or SEA, InternationalConsignementNote is taken from JE_MasterBIll");
		}

		[ExpectNoExceptions]
		public void TestPlaceOfDelivery()
		{
			var wrapper = GetNewWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.PlaceOfDelivery, NUnit.Framework.Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "Place of Delivery at this stage");

			var importer = Factory.New<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			importer.OH_FullName = "Freds Supply Co";
			importer.MainAddress.OA_Address1 = "367 George St";
			importer.MainAddress.OA_City = "Sydney";
			importer.MainAddress.OA_State = "NSW";
			importer.MainAddress.OA_PostCode = "2000";
			importer.MainAddress.OA_RN_NKCountryCode = "AU";
			declaration.JE_OH_Importer = importer.PK;

			wrapper = GetNewWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.PlaceOfDelivery, NUnit.Framework.Is.EqualTo("2000 SYDNEY AU").Using(CustomComparers.TypeComparison), "Place of delivery must be filled");
		}

		[ExpectNoExceptions]
		public void TestGoodsTakingOverPlaceAndDate()
		{
			var wrapper = GetNewWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.CityCountryDateOfGoodsTakingOver, NUnit.Framework.Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "When no address has been provided, wrapper is empty");

			var supplier = Factory.New<OrgHeader>();
			var supplierAddress = supplier.Addresses.AddNew();
			supplierAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);

			declaration.JE_OH_Supplier = supplier.PK;

			supplierAddress.City = "Sydney";
			supplierAddress.OA_RN_NKCountryCode = string.Empty;
			wrapper = GetNewWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.CityCountryDateOfGoodsTakingOver, NUnit.Framework.Is.EqualTo("SYDNEY").Using(CustomComparers.TypeComparison), "When city has been provided to supplier, place is available in wrapper");

			var today = ZDate.Today;
			var todayAsString = today.ToString("dd/MM/yyyy");
			supplierAddress.City = string.Empty;
			declaration.JE_DateAtFinalDestination = today;
			wrapper = GetNewWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.CityCountryDateOfGoodsTakingOver, NUnit.Framework.Is.EqualTo(todayAsString).Using(CustomComparers.TypeComparison), "When JE_DateAtFinalDestination has been provided to declaration, date is available in wrapper");

			supplierAddress.OA_RN_NKCountryCode = "AU";
			declaration.JE_DateAtFinalDestination = ZDate.Empty;
			wrapper = GetNewWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.CityCountryDateOfGoodsTakingOver, NUnit.Framework.Is.EqualTo("AU").Using(CustomComparers.TypeComparison), "When Country has been provided to declaration, it is available in wrapper");

			supplierAddress.City = "Sydney";
			declaration.JE_DateAtFinalDestination = today;
			wrapper = GetNewWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.CityCountryDateOfGoodsTakingOver, NUnit.Framework.Is.EqualTo($"SYDNEY AU {todayAsString}").Using(CustomComparers.TypeComparison), "When Country City and JE_DateAtFinalDestination have been provided to declaration, they are available in wrapper");
		}

		[ExpectNoExceptions]
		public void TestCarrierAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			var wrapper = GetNewWrapper(declaration);

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "The Best Carrier Company";
			var carrierAddress = carrier.Addresses.AddNew();
			carrierAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			carrierAddress.OA_Address1 = "109 Main St";
			carrierAddress.City = "Sydney";
			carrierAddress.OA_RN_NKCountryCode = "AU";
			declaration.JE_OH_ShippingLine = carrier.PK;

			NUnit.Framework.Assert.That(wrapper.CarrierAddress, NUnit.Framework.Is.EqualTo("THE BEST CARRIER COMPANY\r\n109 MAIN ST\r\nSYDNEY - AUSTRALIA").Using(CustomComparers.TypeComparison), "Carrier Address");
		}

		[ExpectNoExceptions]
		public void TestGoodsAttachedDocuments()
		{
			var declaration = Factory.New<JobDeclaration>();
			var wrapper = GetNewWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.GoodsAttachedDocuments, NUnit.Framework.Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "No supporting document expeted at this stage");

			var supportingDocument1 = declaration.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = "XXX";
			supportingDocument1.CSI_ReferenceNumber = "No1";
			var supportingDocument2 = declaration.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Code = "N380";
			supportingDocument2.CSI_ReferenceNumber = "Y1";

			var invoiceHeader = declaration.Invoices.AddNew();
			var supportingDocument3 = invoiceHeader.SupportingDocuments.AddNew();
			supportingDocument3.CSI_Code = "YYY";
			supportingDocument3.CSI_ReferenceNumber = "No2";
			var supportingDocument4 = invoiceHeader.SupportingDocuments.AddNew();
			supportingDocument4.CSI_Code = "N380";
			supportingDocument4.CSI_ReferenceNumber = "Y2";

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			var supportingDocument5 = invoiceLine1.SupportingDocuments.AddNew();
			supportingDocument5.CSI_Code = "ZZZ";
			supportingDocument5.CSI_ReferenceNumber = "No3";
			var supportingDocument6 = invoiceLine1.SupportingDocuments.AddNew();
			supportingDocument6.CSI_Code = "N380";
			supportingDocument6.CSI_ReferenceNumber = "Y3";

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			var supportingDocument7 = invoiceLine2.SupportingDocuments.AddNew();
			supportingDocument7.CSI_Code = "QQQ";
			supportingDocument7.CSI_ReferenceNumber = "No4";
			var supportingDocument8 = invoiceLine2.SupportingDocuments.AddNew();
			supportingDocument8.CSI_Code = "N380";
			supportingDocument8.CSI_ReferenceNumber = "Y4";

			wrapper = GetNewWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.GoodsAttachedDocuments, NUnit.Framework.Is.EqualTo("Y1, Y2, Y3, Y4").Using(CustomComparers.TypeComparison), "When supporting documents have been provided, goods attachment documents expected in wrapper");
		}

		[ExpectNoExceptions]
		public void TestLineDetailsBox6_7_8_9()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var wrapper = GetNewWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.LineDetailsBox6_7_8_9, NUnit.Framework.Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "When no package info has been provided, empty string is expected");

			var invoiceHeader = declaration.Invoices.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			var entryLine2 = entryHeader.MergedLines.AddNew();
			var entryLine3 = entryHeader.MergedLines.AddNew();

			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			entryLine1.EffectiveDescription = "THIS IS THE DESCRIPTION FOR ENTRY LINE 1 WHICH WILL BE CUT HERE --->THIS PART OF THE STRING WILL NOT BE SHOWN  FINE ANIMAL HAIR, KNITTED OR CROCHETED (EXCL. GRADUATED COMPRESSION HOSIERY, PANTYHOSE AND TIGHTS, WOMEN''S FULL-LENGTH OR KNEE-LENGTH STOCKINGS, MEASURING PER SINGLE YARN < 67 DECITEX, AND HOSIERY FOR BABIES)";
			invoiceLine1.JI_CL = entryLine1.PK;
			AddPackageInfoToInvoiceLine(invoiceLine1, 1, "AA", "ar01");

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			entryLine2.EffectiveDescription = "THIS IS THE DESCRIPTION FOR LINE 2 WITH DOUBLE PACKAGE";
			invoiceLine2.JI_CL = entryLine2.PK;
			AddPackageInfoToInvoiceLine(invoiceLine2, 1, "GG", "ar02");
			AddPackageInfoToInvoiceLine(invoiceLine2, 1, "XX", "ar03");

			var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			entryLine3.EffectiveDescription = "THIS IS THE DESCRIPTION FOR LINE 3 WITH NO PACKAGE";
			invoiceLine3.JI_CL = entryLine3.PK;

			wrapper = GetNewWrapper(declaration);
			var expectedLineDescription = @"ar01, 1 AA; THIS IS THE DESCRIPTION FOR ENTRY LINE 1 WHICH WILL BE CUT HERE --->
ar02, 1 GG; ar03, 1 XX; THIS IS THE DESCRIPTION FOR LINE 2 WITH DOUBLE PACKAGE
THIS IS THE DESCRIPTION FOR LINE 3 WITH NO PACKAGE";

			NUnit.Framework.Assert.That(wrapper.LineDetailsBox6_7_8_9, NUnit.Framework.Is.EqualTo(expectedLineDescription).Using(CustomComparers.TypeComparison), "Line details");
		}

		[ExpectNoExceptions]
		public void TestLineDetailsTariffCodeBox10()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			var invoiceHeader = declaration.Invoices.AddNew();

			var wrapper = GetNewWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.LineDetailsTariffCodeBox10, NUnit.Framework.Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "When no tariff has been provided, empty string is expected");

			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1234567890";

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1234567891";

			var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "1234567892";

			NUnit.Framework.Assert.That(declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()), NUnit.Framework.Is.True, "Precondition: declaration.DoMerge()");
			wrapper = GetNewWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.LineDetailsTariffCodeBox10, NUnit.Framework.Is.EqualTo("1234567890\r\n1234567891\r\n1234567892").Using(CustomComparers.TypeComparison), "TariffCode");

			invoiceLine2.JI_Tariff = ZString.Empty;
			NUnit.Framework.Assert.That(declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()), NUnit.Framework.Is.True, "Precondition: declaration.DoMerge()");
			wrapper = GetNewWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.LineDetailsTariffCodeBox10, NUnit.Framework.Is.EqualTo("1234567890\r\n\r\n1234567892").Using(CustomComparers.TypeComparison), "TariffCode with empty middle item");

			invoiceLine3.JI_Tariff = ZString.Empty;
			NUnit.Framework.Assert.That(declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()), NUnit.Framework.Is.True, "Precondition: declaration.DoMerge()");
			wrapper = GetNewWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.LineDetailsTariffCodeBox10, NUnit.Framework.Is.EqualTo("1234567890").Using(CustomComparers.TypeComparison), "TariffCode with only first tariff available, no trailing carriage return expected");
		}

		[ExpectNoExceptions]
		public void TestLineDetailsGrossWeightInKGBox11()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MergeBy = "NON";
			declaration.JE_ApplicationCode = "BLT";

			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			var warehouseID = warehouse.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "A1234567GB", RefCountry.LoadFromCountryCode(Factory, GlbCompany.CurrentCompany.Country.Code));
			warehouseID.OK_OA_PremisesAddress = warehouse.MainAddress.PK;

			declaration.WarehouseDocAddress.E2_OA_Address = warehouse.MainAddress.PK;

			var invoiceHeader = declaration.Invoices.AddNew();

			var wrapper = GetNewWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.LineDetailsGrossWeightInKGBox11, NUnit.Framework.Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "When no weight has been provided, empty string is expected");

			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Weight = 1.0m;
			invoiceLine1.JI_WeightUQ = Core.Constants.Weight.Tonnes;

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Weight = 1200m;
			invoiceLine2.JI_WeightUQ = Core.Constants.Weight.Grams;

			var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Weight = 1013m;
			invoiceLine3.JI_WeightUQ = Core.Constants.Weight.Hectograms;

			NUnit.Framework.Assert.That(declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()), NUnit.Framework.Is.True, "Precondition: declaration.DoMerge()");
			wrapper = GetNewWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.LineDetailsGrossWeightInKGBox11, NUnit.Framework.Is.EqualTo("1000.00    \r\n1.2     \r\n101.3     ").Using(CustomComparers.TypeComparison), "Gross Weight in KG");

			invoiceLine2.JI_Weight = ZDecimal.Zero;
			invoiceLine3.JI_Weight = ZDecimal.Zero;
			NUnit.Framework.Assert.That(declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()), NUnit.Framework.Is.True, "Precondition: declaration.DoMerge()");
			wrapper = GetNewWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.LineDetailsGrossWeightInKGBox11, NUnit.Framework.Is.EqualTo("1000.00    ").Using(CustomComparers.TypeComparison), "No trailing carriage returns are expected");
		}

		[ExpectNoExceptions]
		public void TestLineDetailsVolumeInM3Box12()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MergeBy = "NON";
			declaration.JE_ApplicationCode = "BLT";

			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			var warehouseID = warehouse.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "A1234567GB", RefCountry.LoadFromCountryCode(Factory, GlbCompany.CurrentCompany.Country.Code));
			warehouseID.OK_OA_PremisesAddress = warehouse.MainAddress.PK;

			declaration.WarehouseDocAddress.E2_OA_Address = warehouse.MainAddress.PK;

			var invoiceHeader = declaration.Invoices.AddNew();

			var wrapper = GetNewWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.LineDetailsVolumeInM3Box12, NUnit.Framework.Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "When no volume has been provided, empty string is expected");

			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Volume = 1.234568m;
			invoiceLine1.JI_VolumeUQ = Core.Constants.Volume.CubicMetres;

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Volume = 1200000m;
			invoiceLine2.JI_VolumeUQ = Core.Constants.Volume.CubicCentimeters;

			var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Volume = 1013m;
			invoiceLine3.JI_VolumeUQ = Core.Constants.Volume.CubicInches;

			NUnit.Framework.Assert.That(declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()), NUnit.Framework.Is.True, "Precondition: declaration.DoMerge()");
			wrapper = GetNewWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.LineDetailsVolumeInM3Box12, NUnit.Framework.Is.EqualTo("1.235   \r\n1.2     \r\n0.0166  ").Using(CustomComparers.TypeComparison), "Volume in m3");

			invoiceLine2.JI_Volume = ZDecimal.Zero;
			invoiceLine3.JI_Volume = ZDecimal.Zero;
			NUnit.Framework.Assert.That(declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()), NUnit.Framework.Is.True, "Precondition: declaration.DoMerge()");
			wrapper = GetNewWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.LineDetailsVolumeInM3Box12, NUnit.Framework.Is.EqualTo("1.235   ").Using(CustomComparers.TypeComparison), "No trailing carriage return expected");
		}

		[ExpectNoExceptions]
		public void TestIncotermAndTextBox14()
		{
			var declaration = Factory.New<JobDeclaration>();
			var wrapper = GetNewWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.IncotermAndTextBox14, NUnit.Framework.Is.EqualTo(ZString.Empty), "When no Incoterm has been provided, empty string is expected in the wrapper");

			declaration.JE_ShipmentIncoTerm = "FOF";
			wrapper = GetNewWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.IncotermAndTextBox14, NUnit.Framework.Is.EqualTo("FOF - ").Using(CustomComparers.TypeComparison), "When Incoterm has been provided, it must be available in the wrapper");
		}

		[ExpectNoExceptions]
		public void TestTransportIDBox23()
		{
			var declaration = Factory.New<JobDeclaration>();
			var wrapper = GetNewWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.TransportIDBox23, NUnit.Framework.Is.EqualTo(ZString.Empty), "When no Transport ID has been provided, empty string is expected in the wrapper");

			declaration.ZG_Box18TransportID = "12345678901234567890";
			wrapper = GetNewWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.TransportIDBox23, NUnit.Framework.Is.EqualTo("12345678901234567890").Using(CustomComparers.TypeComparison), "When Transport ID has been provided, it must be available in the wrapper");
		}

		[ExpectNoExceptions]
		public void TestJobNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var wrapper = GetNewWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.JobNumber, NUnit.Framework.Is.EqualTo(ZString.Empty), "When no JobNumber has been provided, empty string is expected in the wrapper");

			declaration.JE_DeclarationReference = "12345678901234567890";
			wrapper = GetNewWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.JobNumber, NUnit.Framework.Is.EqualTo("12345678901234567890").Using(CustomComparers.TypeComparison), "When Transport ID has been provided, it must be available in the wrapper");
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
		}
		JobDeclaration declaration;

		ICMRConsignmentNote GetNewWrapper(JobDeclaration declaration) => new CMRWayBillWrapper(declaration);

		void AddPackageInfoToInvoiceLine(JobComInvoiceLine invoiceLine, ZInt packQuantity, string packType, string marksAndNos)
		{
			var package = declaration.Packages.AddNew();
			package.CW_PackQty = packQuantity;
			package.CW_PackType = packType;
			package.CW_MarksAndNos = marksAndNos;

			var packagesPivotCollection = invoiceLine.PackagesPivot;
			var packagePivot = packagesPivotCollection.AddNew();
			packagePivot.CHC_CW = package.PK;
			packagePivot.CHC_NumberOfPacks = 1;
			packagePivot.CHC_JE = invoiceLine.Declaration.PK;
		}
	}
}
