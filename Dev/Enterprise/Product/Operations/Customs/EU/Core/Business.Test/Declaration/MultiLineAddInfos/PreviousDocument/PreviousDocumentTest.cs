using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing
{
	[TestedType(typeof(PreviousDocument))]
	sealed class PreviousDocumentTest : ImportExportAwareSupportingInfoTest<PreviousDocument>
	{
		public void TestReferenceNumberFieldType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var previousDocument = declaration.PreviousDocuments.AddNew();
			AssertEquals("ReferenceNumber Field should be Text by default", nameof(FieldType.Text), previousDocument.ReferenceNumberFieldType);
			AssertEquals("ReferenceNumber not shown as CodeFindBox by default", false, previousDocument.ShowCodeFindBoxForReferenceNumber);
			AssertEquals("ReferenceNumber shown as TextBox by default", true, previousDocument.ShowTextBoxForReferenceNumber);
		}

		public void TestPreviousDocumentAddedAgainstGroupInvoice()
		{
			var declaration = Factory.New<JobDeclaration>();
			var previousDocument = declaration.PreviousDocuments.AddNew();

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var decLoaded = factory2.Load<JobDeclaration>(declaration.PK);
			var previousDocumentLoaded = factory2.Load<PreviousDocument>(previousDocument.PK);
			AssertEquals(typeof(JobDeclaration), previousDocumentLoaded.Parent.GetType());
		}

		public void TestKeyToDeterimeUniqueness()
		{
			var pd = (PreviousDocument)GetNewBusinessObject();
			pd.CSI_SubType = PreviousDocumentClassList.Codes.PreviousDocument;
			pd.CSI_Code = "380";
			pd.CSI_ReferenceNumber = "FOO";
			AssertEquals("Z380FOO", pd.KeyToDeterimeUniqueness);
		}

		public void TestDateOfIssueInFormat()
		{
			var pd = (PreviousDocument)GetNewBusinessObject();
			pd.CSI_DateOfIssue = new ZDateTime(2000, 1, 1);
			AssertEquals("01/01/2000", pd.DateOfIssueInFormat);
		}

		public void TestSetCSICodeChangeCSIReference() => CombineAssertions(() =>
		{
			var initialValue = "123123";
			var declaration = Factory.New<JobDeclaration>();
			var previousDocument = declaration.PreviousDocuments.AddNew();

			AssertEquals(declaration.PK, previousDocument.Parent.PK);

			declaration.JE_HouseBill = "HouseBill";
			declaration.JE_MasterBill = "MasterBill";

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertDefaultCSI_ReferenceNumber(previousDocument, initialValue, "740", initialValue, "CSI_Reference not equals housebill nor masterbill");
			AssertDefaultCSI_ReferenceNumber(previousDocument, initialValue, "741", initialValue, "CSI_Reference not equals housebill nor masterbill");
			AssertDefaultCSI_ReferenceNumber(previousDocument, initialValue, "704", declaration.JE_HouseBill, "CSI_Reference equals housebill");
			AssertDefaultCSI_ReferenceNumber(previousDocument, initialValue, "705", declaration.JE_MasterBill, "CSI_Reference equals masterbill");
			AssertDefaultCSI_ReferenceNumber(previousDocument, initialValue, "N740", initialValue, "CSI_Reference not equals housebill nor masterbill");
			AssertDefaultCSI_ReferenceNumber(previousDocument, initialValue, "N741", initialValue, "CSI_Reference not equals housebill nor masterbill");
			AssertDefaultCSI_ReferenceNumber(previousDocument, initialValue, "N704", declaration.JE_HouseBill, "CSI_Reference equals housebill");
			AssertDefaultCSI_ReferenceNumber(previousDocument, initialValue, "N705", declaration.JE_MasterBill, "CSI_Reference equals masterbill");

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertDefaultCSI_ReferenceNumber(previousDocument, initialValue, "704", initialValue, "CSI_Reference not equals housebill nor masterbill");
			AssertDefaultCSI_ReferenceNumber(previousDocument, initialValue, "705", initialValue, "CSI_Reference not equals housebill nor masterbill");
			AssertDefaultCSI_ReferenceNumber(previousDocument, initialValue, "740", declaration.JE_HouseBill, "CSI_Reference equals housebill");
			AssertDefaultCSI_ReferenceNumber(previousDocument, initialValue, "741", declaration.JE_MasterBill, "CSI_Reference equals masterbill");
			AssertDefaultCSI_ReferenceNumber(previousDocument, initialValue, "N704", initialValue, "CSI_Reference not equals housebill nor masterbill");
			AssertDefaultCSI_ReferenceNumber(previousDocument, initialValue, "N705", initialValue, "CSI_Reference not equals housebill nor masterbill");
			AssertDefaultCSI_ReferenceNumber(previousDocument, initialValue, "N740", declaration.JE_HouseBill, "CSI_Reference equals housebill");
			AssertDefaultCSI_ReferenceNumber(previousDocument, initialValue, "N741", declaration.JE_MasterBill, "CSI_Reference equals masterbill");
		});

		public void TestQuantityDecimalPlaces()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var previousDocumentOfInvoiceHeader = invoiceHeader.PreviousDocuments.AddNew();
			var previousDocumentOfInvoiceLine = invoiceHeader.InvoiceLines.AddNew().PreviousDocuments.AddNew();

			CombineAssertions("When Export", () =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
				{
					AssertQuantityDecimalPlaces("Previous Document Of Invoice Header and Ucc6", previousDocumentOfInvoiceHeader, 6);
					AssertQuantityDecimalPlaces("Previous Document Of Invoice Line and Ucc6", previousDocumentOfInvoiceLine, 6);
				}

				using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: false))
				{
					AssertQuantityDecimalPlaces("Previous Document Of Invoice Header and Non Ucc6", previousDocumentOfInvoiceHeader, 5);
					AssertQuantityDecimalPlaces("Previous Document Of Invoice Line and Non Ucc6", previousDocumentOfInvoiceLine, 5);
				}
			});

			CombineAssertions("When Import", () =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
				{
					AssertQuantityDecimalPlaces("Previous Document Of Invoice Header and Ucc6", previousDocumentOfInvoiceHeader, 6);
					AssertQuantityDecimalPlaces("Previous Document Of Invoice Line and Ucc6", previousDocumentOfInvoiceLine, 6);
				}

				using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: false))
				{
					AssertQuantityDecimalPlaces("Previous Document Of Invoice Header and Non Ucc6", previousDocumentOfInvoiceHeader, 5);
					AssertQuantityDecimalPlaces("Previous Document Of Invoice Line and Non Ucc6", previousDocumentOfInvoiceLine, 5);
				}
			});

			var orphanPreviousDocument = Factory.New<PreviousDocument>();
			AssertQuantityDecimalPlaces("Without declaration", orphanPreviousDocument, 5);

			var previousDocumentOther = declaration.CustomsEntryInstructions.AddNew().PreviousDocuments.AddNew();
			AssertQuantityDecimalPlaces("When previous Document NOT of Invoice Header or Line", previousDocumentOther, 5);
		}

		public void TestISupportMultipleResourceStringData()
		{
			var previousDocument = (ISupportMultipleResourceStringData)Factory.New<PreviousDocument>();
			AssertCollectionNotContains("When parent declaration is not found, should not contain CaptionKeyInvoiceLineUCC6ExportOrImport", PreviousDocument.CaptionKeyUCC6ExportOrImportInvoiceLine, previousDocument.MultipleKeysToUse);

			var declaration = Factory.New<JobDeclaration>();
			previousDocument = declaration.PreviousDocuments.AddNew();
			AssertContainsExactElementsInAnyOrder("Should contain all MultipleKeysToUse of the original declaration", declaration.MultipleKeysToUse, previousDocument.MultipleKeysToUse);
		}

		public void TestISupportMultipleResourceStringData_Ucc6Export()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			ISupportMultipleResourceStringData supportMultipleResourceStringData = invoiceLine.PreviousDocuments.AddNew();

			AssertCollectionNotContains("For Non Ucc6 Export", supportMultipleResourceStringData.MultipleKeysToUse, PreviousDocument.CaptionKeyUCC6ExportOrImportInvoiceLine);

			CombineAssertions("For Ucc6", () =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
				{
					supportMultipleResourceStringData = invoiceLine.PreviousDocuments.AddNew();
					AssertCollectionContains("Export should contain the CaptionKeyInvoiceLineUCC6ExportOrImport element", PreviousDocument.CaptionKeyUCC6ExportOrImportInvoiceLine, supportMultipleResourceStringData.MultipleKeysToUse);
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
				{
					declaration.JE_MessageType = "~~~";
					supportMultipleResourceStringData = invoiceLine.PreviousDocuments.AddNew();
					AssertCollectionNotContains("Not import nor export should not contain the CaptionKeyInvoiceLineUCC6ExportOrImport element", PreviousDocument.CaptionKeyUCC6ExportOrImportInvoiceLine, supportMultipleResourceStringData.MultipleKeysToUse);
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
				{
					declaration.JE_MessageType = "IMP";
					supportMultipleResourceStringData = invoiceLine.PreviousDocuments.AddNew();
					AssertCollectionContains("Import should contain the CaptionKeyInvoiceLineUCC6ExportOrImport element", PreviousDocument.CaptionKeyUCC6ExportOrImportInvoiceLine, supportMultipleResourceStringData.MultipleKeysToUse);
				}
			});
		}

		public void TestCSI_PackQty_CaptionInvoiceLineUCC6ExportOrImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			var previousDocument = declaration.PreviousDocuments.AddNew();
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(previousDocument.CSI_PackQtyInfo,
				PreviousDocument.CaptionKeyUCC6ExportOrImportInvoiceLine,
				caption: "Number of Packages",
				shortCaption: "Count",
				mediumCaption: "Package Count",
				fullDescription: "[12 01 004 000] Number of Packages: Enter the relevant writing-off number of packages."
			);
		}

		public void TestCSI_PackType_CaptionInvoiceLineUCC6ExportOrImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			var previousDocument = declaration.PreviousDocuments.AddNew();
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(previousDocument.CSI_PackTypeInfo,
				PreviousDocument.CaptionKeyUCC6ExportOrImportInvoiceLine,
				caption: "Type of Packages",
				shortCaption: "Type",
				mediumCaption: "Package Type",
				fullDescription: "[12 01 003 000] Type of Packages: Enter the code specifying the type of package relevant for writing-off the number of packages."
			);
		}

		public void TestCSI_UnitOfQuantity_CaptionDefault()
		{
			var declaration = Factory.New<JobDeclaration>();
			var previousDocument = declaration.PreviousDocuments.AddNew();
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(previousDocument.CSI_UnitOfQuantityInfo,
				declaration.MultipleKeysToUse,
				caption: "Unit Of Quantity",
				shortCaption: "UOM",
				mediumCaption: string.Empty,
				fullDescription: string.Empty
			);
		}

		public void TestCSI_UnitOfQuantity_CaptionInvoiceLineUCC6ExportOrImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			var previousDocument = declaration.PreviousDocuments.AddNew();
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(previousDocument.CSI_UnitOfQuantityInfo,
				PreviousDocument.CaptionKeyUCC6ExportOrImportInvoiceLine,
				caption: "Unit of Quantity",
				shortCaption: "UQ",
				mediumCaption: "Quantity Unit",
				fullDescription: "[12 01 005 000] Measurement Unit and Qualifier (Unit of Quantity): The measurement units laid down in Union legislation, as published in TARIC shall be used. Additional qualifier can be used, where applicable. Enter the relevant writing-off measurement unit and qualifier."
			);
		}

		public void TestCSI_Quantity_CaptionInvoiceLineUCC6ExportOrImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			var previousDocument = declaration.PreviousDocuments.AddNew();
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(previousDocument.CSI_QuantityInfo,
				PreviousDocument.CaptionKeyUCC6ExportOrImportInvoiceLine,
				caption: "Quantity",
				shortCaption: "Qty",
				mediumCaption: string.Empty,
				fullDescription: "[12 01 006 000] Quantity: Enter the relevant writing-off quantity."
			);
		}

		public void TestCSI_Quantity_CaptionDefault()
		{
			var declaration = Factory.New<JobDeclaration>();
			var previousDocument = declaration.PreviousDocuments.AddNew();
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(previousDocument.CSI_QuantityInfo,
				declaration.MultipleKeysToUse,
				caption: "Quantity",
				shortCaption: string.Empty,
				mediumCaption: string.Empty,
				fullDescription: string.Empty
			);
		}

		public void TestCSI_ItemNumber_CaptionInvoiceLineUCC6ExportOrImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			var previousDocument = declaration.PreviousDocuments.AddNew();
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(previousDocument.CSI_ItemNumberInfo,
				PreviousDocument.CaptionKeyUCC6ExportOrImportInvoiceLine,
				caption: "Goods Item Identifier",
				shortCaption: "Item #",
				mediumCaption: "Goods Item ID",
				fullDescription: "[12 01 007 000] Goods Item Identifier: Enter the goods item number as declared in the previous document."
			);
		}

		public void TestCSI_CodeDescription_CaptionInvoiceLineUCC6ExportOrImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			var previousDocument = declaration.PreviousDocuments.AddNew();
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(previousDocument.CSI_CodeDescriptionInfo,
				PreviousDocument.CaptionKeyUCC6ExportOrImportInvoiceLine,
				caption: "Type Description",
				shortCaption: "Desc.",
				mediumCaption: "Description",
				fullDescription: "The description of [12 01 002 000] Type"
			);
		}

		public void TestCSI_LineNo_CaptionInvoiceLineUCC6Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				var previousDocument = declaration.PreviousDocuments.AddNew();
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(previousDocument.CSI_LineNoInfo,
					JobDeclaration.CaptionKeyImportUCC6,
					caption: "Line No.",
					fullDescription: "[12 01 007 000] Previous Documents < Goods Item Identifier"
				);
			}
		}

		public void TestCSI_CodeDescription()
		{
			var countryCode = Core.Constants.CountryCodes.Latvia;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var eunCountryCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
				var eun = helper.CreateNewOrGetExistingDataGrouping(eunCountryCode, "European Union");
				helper.CreateNewOrGetExistingDataGrouping(countryCode, "Latvia", eun);

				var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection;
				helper.CreateCusCodeListsForMultipleTypesWithAttributes(eunCountryCode, new string[] { exportCodeType }, "SD01", "SD01 DES"
				, new Dictionary<string, string[]>(), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				Factory.Save();

				var sd = (PreviousDocument)GetNewBusinessObject();
				sd.CSI_Code = "N123";
				AssertEquals(string.Empty, sd.CSI_CodeDescription);
				sd.CSI_Code = "SD01";
				AssertEquals("SD01 DES", sd.CSI_CodeDescription);
			}
		}

		protected override IEnumerable<PreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			yield return declaration.PreviousDocuments.AddNew();
			var invoice = declaration.Invoices.AddNew();
			yield return invoice.PreviousDocuments.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			yield return invoiceLine.PreviousDocuments.AddNew();
			var product = factory.New<MasterFiles.OrgSupplierPart>();
			product.OP_PartNum = "POOPY";
			var relationship = product.RelatedOrganisations.AddNew();
			relationship.OU_Relationship = "BTH";
			relationship.OU_OH = Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = Enterprise.Customs.Business.BaseCusClassification.ClassificationType.Both;
			yield return pivot.PreviousDocuments.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<JobDeclaration>().Invoices.AddNew().PreviousDocuments.AddNew();
		}

		IDisposable TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(JobDeclaration declaration, bool configurationValue) => ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue);

		void AssertQuantityDecimalPlaces(string message, PreviousDocument previousDocument, int expectedDecimalPlaces)
		{
			AssertHasDecimalPlacesAttribute(message + ", CSI_Quantity", previousDocument.CSI_QuantityInfo, expectedDecimalPlaces);
			AssertHasDecimalPlacesAttribute(message + ", CSI_Quantity2", previousDocument.CSI_Quantity2Info, expectedDecimalPlaces);
			AssertHasDecimalPlacesAttribute(message + ", CSI_Quantity3", previousDocument.CSI_Quantity3Info, expectedDecimalPlaces);
		}

		void AssertDefaultCSI_ReferenceNumber(PreviousDocument previousDocument, string initialValue, string code, string expectedValue, string message)
		{
			previousDocument.CSI_ReferenceNumber = initialValue;
			previousDocument.CSI_Code = code;
			AssertEquals($"Transport Mode {previousDocument.Declaration.JE_TransportMode}, Prev. Doc. code {code}: {message}", expectedValue, previousDocument.CSI_ReferenceNumber);
		}
	}
}
