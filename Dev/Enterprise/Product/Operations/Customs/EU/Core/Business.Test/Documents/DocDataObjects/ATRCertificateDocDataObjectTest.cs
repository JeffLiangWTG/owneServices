using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Customs.EU.Business.Documents.DocDataObjects.Testing.DocDataObjectTestUtility;

namespace Enterprise.Customs.EU.Business.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(ATRCertificateDocDataObject))]
	class ATRCertificateDocDataObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			declaration.JE_TransportMode = "AIR";
			declaration.JE_VesselName = "ARGO";

			AssertExceptionThrown<ArgumentNullException>("Exception expected when wrapper is null", () => new ATRCertificateDocDataObject(null, declaration.Factory));
			AssertExceptionThrown<ArgumentNullException>("Exception expected when factory is null", () => new ATRCertificateDocDataObject(new ATRCertificateOfOriginWrapper(entryHeader), null));
		}

		[ExpectNoExceptions]
		public void TestSetATRCertificateDefaultData()
		{
			var exporter = Factory.New<OrgHeader>();
			exporter.OH_FullName = "Test Exporter Company";
			exporter.MainAddress.Address1 = "72 Grand St";
			var exporterAddress = exporter.Addresses.AddNew();
			exporterAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			exporterAddress.OA_Address1 = "Test Exporter Address Line";
			exporterAddress.City = "Sydney";
			exporterAddress.OA_RN_NKCountryCode = "AU";
			exporterAddress.Postcode = "2015";

			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "Test Importer Company";
			importer.MainAddress.Address1 = "15 Prince St";
			var importerAddress = importer.Addresses.AddNew();
			importerAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			importerAddress.OA_Address1 = "Test Importer Address Line";
			importerAddress.City = "Seoul";
			importerAddress.OA_RN_NKCountryCode = "KR";
			importerAddress.Postcode = "58321";

			declaration.JE_OH_Supplier = exporter.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_GoodsOrigin = "ES";
			declaration.JE_RL_NKFinalDestination = "FRLIO";
			declaration.JE_GoodsDestination = "TR";

			var atrCertificateDocDataObject = new ATRCertificateDocDataObject(new ATRCertificateOfOriginWrapper(entryHeader), entryHeader.Factory);
			NUnit.Framework.Assert.That(atrCertificateDocDataObject.Exporter, NUnit.Framework.Is.EqualTo("TEST EXPORTER COMPANY\nTEST EXPORTER ADDRESS LINE\nSYDNEY 2015\nAUSTRALIA").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(atrCertificateDocDataObject.Importer, NUnit.Framework.Is.EqualTo("TEST IMPORTER COMPANY\nTEST IMPORTER ADDRESS LINE\nSEOUL 58321\nKOREA, REPUBLIC OF").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(atrCertificateDocDataObject.ExportationCountry, NUnit.Framework.Is.EqualTo("SPAIN").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(atrCertificateDocDataObject.DestinationCountry, NUnit.Framework.Is.EqualTo("TURKEY").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(atrCertificateDocDataObject.Url, NUnit.Framework.Is.EqualTo(ZString.Empty), "Url always empty in EU");
		}

		[ExpectNoExceptions]
		public void TestMaxLengthExporterInDocument()
		{
			var exporter = Factory.New<OrgHeader>();
			exporter.OH_FullName = ATRCertificateOfOriginConstants.Length.FieldForMaxLengthTest100;
			exporter.MainAddress.Address1 = ATRCertificateOfOriginConstants.Length.FieldForMaxLengthTest100.Substring(0, 50);
			exporter.MainAddress.Address2 = ATRCertificateOfOriginConstants.Length.FieldForMaxLengthTest100.Substring(0, 50);
			var exporterAddress = exporter.Addresses.AddNew();
			exporterAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			exporterAddress.OA_Address1 = ATRCertificateOfOriginConstants.Length.FieldForMaxLengthTest100.Substring(0, 50);
			exporterAddress.OA_Address2 = ATRCertificateOfOriginConstants.Length.FieldForMaxLengthTest100.Substring(0, 50);
			exporterAddress.PrimaryOrgAddressAdditionalInfoDetail = ATRCertificateOfOriginConstants.Length.FieldForMaxLengthTest100.Substring(0, 50);
			exporterAddress.City = ATRCertificateOfOriginConstants.Length.FieldForMaxLengthTest100.Substring(0, 50);
			exporterAddress.OA_RN_NKCountryCode = "AU";
			exporterAddress.Postcode = "2015";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = exporter.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var atrDataObject = new ATRCertificateDocDataObject(new ATRCertificateOfOriginWrapper(entryHeader), entryHeader.Factory);

			NUnit.Framework.Assert.That(atrDataObject.Exporter.Length, NUnit.Framework.Is.EqualTo(ATRCertificateOfOriginConstants.Length.MaxLinesExporterInDocument * ATRCertificateOfOriginConstants.Length.MaxLengthLineExporterInDocument), "The max length of Exporter should be " + (ATRCertificateOfOriginConstants.Length.MaxLinesExporterInDocument * ATRCertificateOfOriginConstants.Length.MaxLengthLineExporterInDocument));
		}

		[ExpectNoExceptions]
		public void TestMaxLengthCountryOfExportationInDocument()
		{
			atrCertificateDocDataObject.ExportationCountry = ATRCertificateOfOriginConstants.Length.FieldForMaxLengthTest100;
			NUnit.Framework.Assert.That(atrCertificateDocDataObject.ExportationCountry.Length, NUnit.Framework.Is.EqualTo(ATRCertificateOfOriginConstants.Length.MaxLengthCountryOfExportationInDocument), "The max length of Country of Exportation should be " + ATRCertificateOfOriginConstants.Length.MaxLengthCountryOfExportationInDocument);
		}

		[ExpectNoExceptions]
		public void TestMaxLengthCountryOfDestinationInDocument()
		{
			atrCertificateDocDataObject.DestinationCountry = ATRCertificateOfOriginConstants.Length.FieldForMaxLengthTest100;
			NUnit.Framework.Assert.That(atrCertificateDocDataObject.DestinationCountry.Length, NUnit.Framework.Is.EqualTo(ATRCertificateOfOriginConstants.Length.MaxLengthCountryOfDestinationInDocument), "The max length of Country of Destination should be " + ATRCertificateOfOriginConstants.Length.MaxLengthCountryOfDestinationInDocument);
		}

		[ExpectNoExceptions]
		public void TestMaxLengthItemInDocument()
		{
			atrCertificateDocDataObject.Items += "1" + System.Environment.NewLine;
			atrCertificateDocDataObject.Items += "2" + System.Environment.NewLine;
			while (atrCertificateDocDataObject.Items.Length < (ATRCertificateOfOriginConstants.Length.MaxLinesItemsInDocument * ATRCertificateOfOriginConstants.Length.MaxLengthLineItemsInDocument))
			{
				atrCertificateDocDataObject.Items += ATRCertificateOfOriginConstants.Length.FieldForMaxLengthTest100;
			}
			NUnit.Framework.Assert.That(atrCertificateDocDataObject.Items.Length, NUnit.Framework.Is.EqualTo(ATRCertificateOfOriginConstants.Length.MaxLinesItemsInDocument * ATRCertificateOfOriginConstants.Length.MaxLengthLineItemsInDocument), "The max length of Items should be " + (ATRCertificateOfOriginConstants.Length.MaxLinesItemsInDocument * ATRCertificateOfOriginConstants.Length.MaxLengthLineItemsInDocument));
		}

		[ExpectNoExceptions]
		public void TestMaxLengthMarksInDocument()
		{
			atrCertificateDocDataObject.MarksNumbers += "1" + System.Environment.NewLine;
			atrCertificateDocDataObject.MarksNumbers += "2" + System.Environment.NewLine;
			while (atrCertificateDocDataObject.MarksNumbers.Length < (ATRCertificateOfOriginConstants.Length.MaxLinesMarksInDocument * ATRCertificateOfOriginConstants.Length.MaxLengthLineMarksInDocument))
			{
				atrCertificateDocDataObject.MarksNumbers += ATRCertificateOfOriginConstants.Length.FieldForMaxLengthTest100;
			}
			NUnit.Framework.Assert.That(atrCertificateDocDataObject.MarksNumbers.Length, NUnit.Framework.Is.EqualTo(ATRCertificateOfOriginConstants.Length.MaxLinesMarksInDocument * ATRCertificateOfOriginConstants.Length.MaxLengthLineMarksInDocument), "The max length of Marks and Numbers should be " + (ATRCertificateOfOriginConstants.Length.MaxLinesMarksInDocument * ATRCertificateOfOriginConstants.Length.MaxLengthLineMarksInDocument) + System.Environment.NewLine + atrCertificateDocDataObject.MarksNumbers);
		}

		[ExpectNoExceptions]
		public void TestMaxLengthGrossWeightInDocument()
		{
			atrCertificateDocDataObject.GrossWeight += "1" + System.Environment.NewLine;
			while (atrCertificateDocDataObject.GrossWeight.Length < (ATRCertificateOfOriginConstants.Length.MaxLinesGrossWeightInDocument * ATRCertificateOfOriginConstants.Length.MaxLengthLineGrossWeightInDocument))
			{
				atrCertificateDocDataObject.GrossWeight += ATRCertificateOfOriginConstants.Length.FieldForMaxLengthTest100;
			}
			NUnit.Framework.Assert.That(atrCertificateDocDataObject.GrossWeight.Length, NUnit.Framework.Is.EqualTo(ATRCertificateOfOriginConstants.Length.MaxLinesGrossWeightInDocument * ATRCertificateOfOriginConstants.Length.MaxLengthLineGrossWeightInDocument), "The max length of Gross Weight should be " + (ATRCertificateOfOriginConstants.Length.MaxLinesGrossWeightInDocument * ATRCertificateOfOriginConstants.Length.MaxLengthLineGrossWeightInDocument));
		}

		[ExpectNoExceptions]
		public void TestMaxLengthCustomsEndorsementInDocument()
		{
			var box12CustomsEndorsement = atrCertificateDocDataObject.Box12CustomsEndorsement;

			CombineAssertions(() =>
		{
			box12CustomsEndorsement.Form = ATRCertificateOfOriginConstants.Length.FieldForMaxLengthTest100;
			box12CustomsEndorsement.FormNo = ATRCertificateOfOriginConstants.Length.FieldForMaxLengthTest100;
			box12CustomsEndorsement.CustomsOffice = ATRCertificateOfOriginConstants.Length.FieldForMaxLengthTest100;
			box12CustomsEndorsement.IssuingCountry = ATRCertificateOfOriginConstants.Length.FieldForMaxLengthTest100;
			box12CustomsEndorsement.Place = ATRCertificateOfOriginConstants.Length.FieldForMaxLengthTest100;
			NUnit.Framework.Assert.That(box12CustomsEndorsement.Form.Length, NUnit.Framework.Is.EqualTo(ATRCertificateOfOriginConstants.Length.MaxLength12FormInDocument), "The max length of (12)Form should be " + ATRCertificateOfOriginConstants.Length.MaxLength12FormInDocument);
			NUnit.Framework.Assert.That(box12CustomsEndorsement.FormNo.Length, NUnit.Framework.Is.EqualTo(ATRCertificateOfOriginConstants.Length.MaxLength12AATR1InDocument), "The max length of (12)FormNo should be " + ATRCertificateOfOriginConstants.Length.MaxLength12AATR1InDocument);
			NUnit.Framework.Assert.That(box12CustomsEndorsement.CustomsOffice.Length, NUnit.Framework.Is.EqualTo(ATRCertificateOfOriginConstants.Length.MaxLength12CustomsInDocument), "The max length of (12)CustomsOffice should be " + ATRCertificateOfOriginConstants.Length.MaxLength12CustomsInDocument);
			NUnit.Framework.Assert.That(box12CustomsEndorsement.IssuingCountry.Length, NUnit.Framework.Is.EqualTo(ATRCertificateOfOriginConstants.Length.MaxLength12IssuingInDocument), "The max length of (12)IssuingCountry should be " + ATRCertificateOfOriginConstants.Length.MaxLength12IssuingInDocument);
			NUnit.Framework.Assert.That(box12CustomsEndorsement.Place.Length, NUnit.Framework.Is.EqualTo(ATRCertificateOfOriginConstants.Length.MaxLength12PlaceInDocument), "The max length of (12)Place should be " + ATRCertificateOfOriginConstants.Length.MaxLength12PlaceInDocument);
		});
		}

		[ExpectNoExceptions]
		public void TestMaxLengthDeclarationByExporterInDocument()
		{
			var box13DeclarationExporter = atrCertificateDocDataObject.Box13DeclarationExporter;

			CombineAssertions(() =>
			{
				box13DeclarationExporter.Place = ATRCertificateOfOriginConstants.Length.FieldForMaxLengthTest100;
				box13DeclarationExporter.SupplierDetails = ATRCertificateOfOriginConstants.Length.FieldForMaxLengthTest100;
				NUnit.Framework.Assert.That(ATRCertificateOfOriginConstants.Length.MaxLength13PlaceInDocument, NUnit.Framework.Is.EqualTo(14));
				NUnit.Framework.Assert.That(box13DeclarationExporter.Place.Length, NUnit.Framework.Is.EqualTo(ATRCertificateOfOriginConstants.Length.MaxLength13PlaceInDocument), "The max length of (13)Place should be " + ATRCertificateOfOriginConstants.Length.MaxLength13PlaceInDocument);
				NUnit.Framework.Assert.That(ATRCertificateOfOriginConstants.Length.MaxLength13SupplierDetailsInDocument, NUnit.Framework.Is.EqualTo(66));
				NUnit.Framework.Assert.That(box13DeclarationExporter.SupplierDetails.Length, NUnit.Framework.Is.EqualTo(ATRCertificateOfOriginConstants.Length.MaxLength13SupplierDetailsInDocument), "The max length of (13)Supplier Details should be " + ATRCertificateOfOriginConstants.Length.MaxLength13SupplierDetailsInDocument);
			});
		}

		[ExpectNoExceptions]
		public void TestRemarks()
		{
			var atrDocDataObject = (ATRCertificateDocDataObject)GetNewBusinessObject();
			NUnit.Framework.Assert.That(atrDocDataObject.Remarks, NUnit.Framework.Is.EqualTo(ZString.Empty), "Remarks always empty in EU");
		}

		[ExpectNoExceptions]
		public void TestItemDetails()
		{
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 100m;
			invoiceLine1.JI_InvoiceUQ = "BAG";
			invoiceLine1.JI_Description = "Books";
			invoiceLine1.JI_Weight = 10m;
			invoiceLine1.JI_WeightUQ = "KG";

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_WeightUQ = "LT";
			invoiceLine2.JI_VolumeUQ = "M3";
			invoiceLine2.JI_InvoiceUQ = "BBK";
			invoiceLine2.JI_Description = "TV";

			var warehouse = Factory.New<OrgHeader>();
			declaration.WarehouseDocAddress.OrganisationPK = warehouse.PK;

			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;

			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine2.JI_CL = entryLine2.PK;

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_WeightUQ = "KG";
			invoiceLine3.JI_VolumeUQ = "DD";
			invoiceLine3.JI_InvoiceUQ = "DDJ";
			invoiceLine3.JI_Description = "LN";

			CombineAssertions("Assert Item Details", () =>
			{
				var items = @"1
2
";
				AssertDataObjectPropertyIsAlterable(atrCertificateDocDataObject, items, nameof(atrCertificateDocDataObject.Items));

				var marksNumbers = @"BOOKS
TV";
				AssertDataObjectPropertyIsAlterable(atrCertificateDocDataObject, marksNumbers, nameof(atrCertificateDocDataObject.MarksNumbers));

				var grossWeight = @"10 KG
";
				AssertDataObjectPropertyIsAlterable(atrCertificateDocDataObject, grossWeight, nameof(atrCertificateDocDataObject.GrossWeight));
			});
		}

		[ExpectNoExceptions]
		public void TestATRBox12CustomsEndorsement()
		{
			NUnit.Framework.Assert.That(atrCertificateDocDataObject.Box12CustomsEndorsement, NUnit.Framework.Is.Not.EqualTo(default(CustomsEndorsement)), "nameof(ATRCertificateDocDataObject.Box12CustomsEndorsement) - should not be [null]");
			var box12CustomsEndorsement = atrCertificateDocDataObject.Box12CustomsEndorsement;
			NUnit.Framework.Assert.That(atrCertificateDocDataObject.Box12CustomsEndorsement, NUnit.Framework.Is.SameAs(box12CustomsEndorsement), $"{nameof(ATRCertificateDocDataObject.Box12CustomsEndorsement)} must be cached");
		}

		[ExpectNoExceptions]
		public void TestBox13DeclarationExporter()
		{
			var atrDocDataObject = (ATRCertificateDocDataObject)GetNewBusinessObject();

			NUnit.Framework.Assert.That(atrDocDataObject.Box13DeclarationExporter, NUnit.Framework.Is.Not.EqualTo(default(DeclarationExporter)), "nameof(ATRCertificateDocDataObject.Box13DeclarationExporter) - should not be [null]");
			var box13DeclarationExporter = atrDocDataObject.Box13DeclarationExporter;

			CombineAssertions($"Assert {nameof(ATRCertificateDocDataObject.Box13DeclarationExporter)}", () =>
			{
				NUnit.Framework.Assert.That(box13DeclarationExporter, NUnit.Framework.Is.TypeOf<DeclarationExporter>(), $"{nameof(ATRCertificateDocDataObject.Box13DeclarationExporter)} type");
				NUnit.Framework.Assert.That(atrDocDataObject.Box13DeclarationExporter, NUnit.Framework.Is.SameAs(box13DeclarationExporter), $"{nameof(ATRCertificateDocDataObject.Box13DeclarationExporter)} must be cached");
			});
		}

		[ExpectNoExceptions]
		public void TestVoyageFlight()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_VesselName = "Vessel 1";
			declaration.JE_VoyageFlightNo = "Voyage 1";
			NUnit.Framework.Assert.That(atrCertificateDocDataObject.TransportDetails, NUnit.Framework.Is.EqualTo("VESSEL 1 VOYAGE 1").Using(CustomComparers.TypeComparison), $"When Transport mode is SEA, {nameof(ATRCertificateDocDataObject.TransportDetails)}");

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_VoyageFlightNo = "az123";
			atrCertificateDocDataObject = new ATRCertificateDocDataObject(new ATRCertificateOfOriginWrapper(entryHeader), entryHeader.Factory);
			NUnit.Framework.Assert.That(atrCertificateDocDataObject.TransportDetails, NUnit.Framework.Is.EqualTo("AZ123").Using(CustomComparers.TypeComparison), $"When Transport mode is AIR, {nameof(ATRCertificateDocDataObject.TransportDetails)}");

			declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
			declaration.JE_VesselName = "rail";
			atrCertificateDocDataObject = new ATRCertificateDocDataObject(new ATRCertificateOfOriginWrapper(entryHeader), entryHeader.Factory);
			NUnit.Framework.Assert.That(atrCertificateDocDataObject.TransportDetails, NUnit.Framework.Is.EqualTo("RAIL").Using(CustomComparers.TypeComparison), $"When Transport mode is \"Other transports (RAIL, ROA, ...)\", {nameof(ATRCertificateDocDataObject.TransportDetails)}");

			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			declaration.JE_VesselName = "road";
			atrCertificateDocDataObject = new ATRCertificateDocDataObject(new ATRCertificateOfOriginWrapper(entryHeader), entryHeader.Factory);
			NUnit.Framework.Assert.That(atrCertificateDocDataObject.TransportDetails, NUnit.Framework.Is.EqualTo("ROAD").Using(CustomComparers.TypeComparison), $"When Transport mode is \"Other transports (RAIL, ROA, ...)\", {nameof(ATRCertificateDocDataObject.TransportDetails)}");
		}

		[ExpectNoExceptions]
		public void TestReferenceDateFormat()
		{
			var atrDocDataObject = (ATRCertificateDocDataObject)GetNewBusinessObject();
			NUnit.Framework.Assert.That(atrDocDataObject.ReferenceDateFormat, NUnit.Framework.Is.EqualTo("dd/MM/yyyy"), "Reference Date Format");
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			return new ATRCertificateDocDataObject(new ATRCertificateOfOriginWrapper(entryHeader), entryHeader.Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			atrCertificateDocDataObject = new ATRCertificateDocDataObject(new ATRCertificateOfOriginWrapper(entryHeader), entryHeader.Factory);
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		ATRCertificateDocDataObject atrCertificateDocDataObject;
	}
}
