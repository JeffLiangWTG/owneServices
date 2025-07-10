using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.MasterFiles;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	[TestedType(typeof(SupportingDocument))]
	sealed class SupportingDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<SupportingDocument>
	{
		protected override IEnumerable<SupportingDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			yield return declaration.SupportingDocuments.AddNew();
			var invoice = declaration.Invoices.AddNew();
			yield return invoice.SupportingDocuments.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			yield return invoiceLine.SupportingDocuments.AddNew();
			var product = factory.New<OrgSupplierPart>();
			product.OP_PartNum = "POOPY";
			var relationship = product.RelatedOrganisations.AddNew();
			relationship.OU_Relationship = "BTH";
			relationship.OU_OH = Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = Enterprise.Customs.Business.BaseCusClassification.ClassificationType.Both;
			yield return pivot.SupportingDocuments.AddNew();
		}

		public void TestIsLine()
		{
			Assert("IsLine should be always true", supportingDocument.IsLine);
		}

		public void TestIsLineOnly()
		{
			Assert("IsLineOnly should be always true", supportingDocument.IsLineOnly);
		}

		public void TestCSI_RN_NKCountryCode()
		{
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForProperty(Factory.New<SupportingDocument>().CSI_RN_NKCountryCodeInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "CCI Validate Country", resourceStringDataAttribute.Caption);
				AssertEquals("MediumCaption", "CCI Valid. Country", resourceStringDataAttribute.MediumCaption);
				AssertEquals("ShortCaption", "CCI Country", resourceStringDataAttribute.ShortCaption);
				AssertEquals("FullDescription", "Only used in CCI. Identify the country who will validate the data", resourceStringDataAttribute.FullDescription);
			});
		}

		public void TestCSI_PackQty()
		{
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForProperty(Factory.New<SupportingDocument>().CSI_PackQtyInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Number of Packages", resourceStringDataAttribute.Caption);
				AssertEquals("MediumCaption", "Pack Qty", resourceStringDataAttribute.MediumCaption);
				AssertEquals("ShortCaption", "#Pkgs.", resourceStringDataAttribute.ShortCaption);
				AssertEquals("FullDescription", "Number of Packages for this document.", resourceStringDataAttribute.FullDescription);
			});
		}

		public void TestCSI_PackType()
		{
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForProperty(Factory.New<SupportingDocument>().CSI_PackTypeInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Type of Packages", resourceStringDataAttribute.Caption);
				AssertEquals("MediumCaption", "Pack Type", resourceStringDataAttribute.MediumCaption);
				AssertEquals("ShortCaption", "Pack Type", resourceStringDataAttribute.ShortCaption);
				AssertEquals("FullDescription", "Type of Packages for this document.", resourceStringDataAttribute.FullDescription);
			});
		}

		public void TestCopyFrom()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();

			var supDoc1 = GetSupportingDoc(1, "REF111");
			declaration.SupportingDocuments.Add(supDoc1);
			var readOnlySupportingDocumentCollection = new ReadOnlySupportingDocumentCollection(entryLine);
			readOnlySupportingDocumentCollection.LoadNew();
			var readOnlySupportingDocument = readOnlySupportingDocumentCollection.Cast<ReadOnlySupportingDocument>().First();

			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("Null readonlysupdoc", () => SupportingDocument.CopyFrom(null));

				var supDoc = SupportingDocument.CopyFrom(readOnlySupportingDocument);
				AssertEquals("CSI_Code", readOnlySupportingDocument.CSI_Code, supDoc.CSI_Code);
				AssertEquals("CSI_ReferenceNumber", readOnlySupportingDocument.CSI_ReferenceNumber, supDoc.CSI_ReferenceNumber);
				AssertEquals("CSI_Quantity", readOnlySupportingDocument.CSI_Quantity, supDoc.CSI_Quantity);
				AssertEquals("CSI_UnitOfQuantity", readOnlySupportingDocument.CSI_UnitOfQuantity, supDoc.CSI_UnitOfQuantity);
				AssertEquals("CSI_Quantity2", readOnlySupportingDocument.CSI_Quantity2, supDoc.CSI_Quantity2);
				AssertEquals("CSI_UnitOfQuantity2", readOnlySupportingDocument.CSI_UnitOfQuantity2, supDoc.CSI_UnitOfQuantity2);
				AssertEquals("CSI_Value", readOnlySupportingDocument.CSI_Value, supDoc.CSI_Value);
				AssertEquals("CSI_RX_NKCurrency", readOnlySupportingDocument.CSI_RX_NKCurrency, supDoc.CSI_RX_NKCurrency);
				AssertEquals("CSI_DateOfIssue", readOnlySupportingDocument.CSI_DateOfIssue, supDoc.CSI_DateOfIssue);
				AssertEquals("CSI_DateOfExpiry", readOnlySupportingDocument.CSI_DateOfExpiry, supDoc.CSI_DateOfExpiry);
				AssertEquals("CSI_Procedure", readOnlySupportingDocument.CSI_Procedure, supDoc.CSI_Procedure);
				AssertEquals("CSI_AdditionalDescription", readOnlySupportingDocument.CSI_AdditionalDescription, supDoc.CSI_AdditionalDescription);
				AssertEquals("CSI_ItemNumber", readOnlySupportingDocument.CSI_ItemNumber, supDoc.CSI_ItemNumber);
				AssertEquals("CSI_DataModel", readOnlySupportingDocument.CSI_DataModel, supDoc.CSI_DataModel);
			});
		}

		public void TestMatchesPreviouslySentDocument()
		{
			var supDoc = GetSupportingDoc(1, "REF111");
			var supDocEqual = GetSupportingDoc(1, "REF111");
			CombineAssertions(() =>
			{
				AssertEquals("Object is equal", true, supDoc.MatchesPreviouslySentDocument(supDocEqual));

				var supDocDiff = GetSupportingDoc(1, "REF111");
				supDocDiff.CSI_Code = "T";
				AssertEquals("CSI_Code: Object is not equal", false, supDoc.MatchesPreviouslySentDocument(supDocDiff));

				supDocDiff = GetSupportingDoc(1, "REF111");
				supDocDiff.CSI_ReferenceNumber = "T";
				AssertEquals("CSI_ReferenceNumber: Object is not equal", false, supDoc.MatchesPreviouslySentDocument(supDocDiff));

				supDocDiff = GetSupportingDoc(1, "REF111");
				supDocDiff.CSI_Quantity = 0;
				AssertEquals("CSI_Quantity: Object is equal since we are not checking this field", true, supDoc.MatchesPreviouslySentDocument(supDocDiff));

				supDocDiff = GetSupportingDoc(1, "REF111");
				supDocDiff.CSI_DateOfExpiry = new ZDateTime(2020, 12, 01);
				AssertEquals("CSI_DateOfExpiry: Object is not equal", false, supDoc.MatchesPreviouslySentDocument(supDocDiff));

				supDocDiff = GetSupportingDoc(1, "REF111");
				supDocDiff.CSI_Quantity2 = 0;
				AssertEquals("CSI_Quantity2: Object is equal since we are not checking this field", true, supDoc.MatchesPreviouslySentDocument(supDocDiff));

				supDocDiff = GetSupportingDoc(1, "REF111");
				supDocDiff.CSI_DateOfIssue = new ZDateTime(2020, 12, 01);
				AssertEquals("CSI_DateOfIssue: Object is not equal", false, supDoc.MatchesPreviouslySentDocument(supDocDiff));

				supDocDiff = GetSupportingDoc(1, "REF111");
				supDocDiff.CSI_RX_NKCurrency = "T";
				AssertEquals("CSI_RX_NKCurrency: Object is equal since we are not checking this field", true, supDoc.MatchesPreviouslySentDocument(supDocDiff));

				supDocDiff = GetSupportingDoc(1, "REF111");
				supDocDiff.CSI_Procedure = "T";
				AssertEquals("CSI_Procedure: Object is not equal", false, supDoc.MatchesPreviouslySentDocument(supDocDiff));

				supDocDiff = GetSupportingDoc(1, "REF111");
				supDocDiff.CSI_UnitOfQuantity = "T";
				AssertEquals("CSI_UnitOfQuantity: Object is equal since we are not checking this field", true, supDoc.MatchesPreviouslySentDocument(supDocDiff));

				supDocDiff = GetSupportingDoc(1, "REF111");
				supDocDiff.CSI_AdditionalDescription = "AddInfo2";
				AssertEquals("CSI_AdditionalDescription: Object is not equal", false, supDoc.MatchesPreviouslySentDocument(supDocDiff));

				supDocDiff = GetSupportingDoc(1, "REF111");
				supDocDiff.CSI_Value = 0;
				AssertEquals("CSI_Value: Object is equal since we are not checking this field", true, supDoc.MatchesPreviouslySentDocument(supDocDiff));

				supDocDiff = GetSupportingDoc(1, "REF111");
				supDocDiff.CSI_ItemNumber = 5;
				AssertEquals("CSI_ItemNumber: Object is not equal", false, supDoc.MatchesPreviouslySentDocument(supDocDiff));

				supDocDiff = GetSupportingDoc(1, "REF111");
				supDocDiff.CSI_UnitOfQuantity2 = "T";
				AssertEquals("CSI_UnitOfQuantity2: Object is equal since we are not checking this field", true, supDoc.MatchesPreviouslySentDocument(supDocDiff));
			});
		}

		public void TestMarkEntryHeaderAsNeedingValidation()
		{
			var markedAsNeedingValidationCount = 0;
			Factory.MarkedAsNeedingValidation += (obj) =>
			{
				if (obj is CusEntryHeader)
				{
					markedAsNeedingValidationCount++;
				}
			};

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var invoiceHeaderSupportingDocument = invoiceHeader.SupportingDocuments.AddNew();
			invoiceHeaderSupportingDocument.CSI_Code = "XXX";
			AssertEquals("Setting CSI_Code should not trigger MarkAsNeedingValidation() on CusEntryHeader as parent is JobComInvoiceHeader", 0, markedAsNeedingValidationCount);

			invoiceHeaderSupportingDocument.Delete();
			AssertEquals($"Deleting {nameof(invoiceHeaderSupportingDocument)} should not trigger MarkAsNeedingValidation() on CusEntryHeader as parent is JobComInvoiceHeader", 0, markedAsNeedingValidationCount);

			var invoiceLineSupportingDocument = invoiceLine.SupportingDocuments.AddNew();
			invoiceLineSupportingDocument.CSI_Code = "XXX";
			AssertEquals("Setting CSI_Code should trigger MarkAsNeedingValidation() on CusEntryHeader as parent is JobComInvoiceLine", 1, markedAsNeedingValidationCount);

			invoiceLineSupportingDocument.Delete();
			AssertEquals($"Deleting {nameof(invoiceLineSupportingDocument)} should trigger MarkAsNeedingValidation() on CusEntryHeader as parent is JobComInvoiceHeader", 2, markedAsNeedingValidationCount);
		}

		public void TestCSI_Procedure_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;

			var entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryHeader1.CH_CEI_Instruction = entryInstruction1.PK;
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.B;
			entryHeader1.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;

			var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;
			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.A;
			entryHeader2.CH_EntryStatus = EntryStatusCodes.Cleared;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction2.PK;

			var supportingDocumentOnInvoiceLine1 = invoiceLine1.SupportingDocuments.AddNew();
			var supportingDocumentOnInvoiceLine2 = invoiceLine2.SupportingDocuments.AddNew();
			var supportingDocumentOnInvoice1 = invoice.SupportingDocuments.AddNew();
			var supportingDocumentOnDeclaration = declaration.SupportingDocuments.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("supportingDocumentOnInvoiceLine1.CSI_Procedure is NOT readonly", false, supportingDocumentOnInvoiceLine1.CSI_ProcedureInfo.ReadOnly);
				AssertEquals("supportingDocumentOnInvoiceLine2.CSI_Procedure is readonly", true, supportingDocumentOnInvoiceLine2.CSI_ProcedureInfo.ReadOnly);
				AssertEquals("supportingDocumentOnInvoice1.CSI_Procedure is NOT readonly", false, supportingDocumentOnInvoice1.CSI_ProcedureInfo.ReadOnly);
				AssertEquals("supportingDocumentOnDeclaration.CSI_Procedure is NOT readonly", false, supportingDocumentOnDeclaration.CSI_ProcedureInfo.ReadOnly);

				invoiceLine1.JI_CEI = entryInstruction2.PK;
				AssertEquals("supportingDocumentOnInvoice1.CSI_Procedure is readonly", true, supportingDocumentOnInvoice1.CSI_ProcedureInfo.ReadOnly);
			});
		}

		public void TestCloneSupportingDocument()
		{
			var supportingDoc = GetSupportingDoc(1, "REF");
			CombineAssertions(() =>
			{
				AssertEquals("Test CSI_Procedure of the original document", "A", supportingDoc.CSI_Procedure);
				var cloneSupportingDoc = (SupportingDocument)supportingDoc.Clone();
				AssertEquals("Test for no Clone CSI_Procedure when cloning document", ZString.Empty, cloneSupportingDoc.CSI_Procedure);
			});
		}

		public void TestGetHashCodeError()
		{
			ErrorReporter.Clear();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var supDoc1 = declaration.SupportingDocuments.AddNew();
			supDoc1.CSI_ReferenceNumber = "AAA";
			var supDoc2 = declaration.SupportingDocuments.AddNew();
			supDoc2.CSI_ReferenceNumber = "BBB";
			var supDoc3 = declaration.SupportingDocuments.AddNew();
			supDoc3.CSI_ReferenceNumber = "CCC";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var declarationInAnotherFactory = newFactory.Load<JobDeclaration>(declaration.PK);
			var supDocsInAnotherFactory = declarationInAnotherFactory.SupportingDocuments;
			newFactory.Save();
			AssertEquals(3, supDocsInAnotherFactory.Count);
			supDocsInAnotherFactory.RemoveAndDeleteAll();

			supDoc3.Delete();
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("MessageReported doesn't have the expected error.", false, ErrorReporter.LastMessageReported.Contains("Should not be accessing a property on a deleted business object"));
				var exceptionMessage = ErrorReporter.LastExceptionReported?.Message ?? ZString.Empty;
				AssertEquals("ExceptionReported  doesn't have the expected error", false, exceptionMessage.Contains("Deleted row information cannot be accessed through the row."));
				ErrorReporter.Clear();
			});
		}

		public void TestPackageCodeList()
		{
			RefCusCodeList Create_CusCodeList_RefData(UniversalReferenceTestDataHelper refHelper, string countryCode, string refDataType, string code)
			{
				var dateMIN = ZDateTime.Today.AddMonths(-1);
				var dateMAX = ZDateTime.Today.AddMonths(3);

				return refHelper.CreateCusCodeList(countryCode, refDataType, code, dateMIN, dateMAX);
			}

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var unitedNationsPackageTypes = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes;
			var packageTypes = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes;
			var eun = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, "UnitedNationsRecommendations", helper.CreateNewOrGetExistingDataGrouping(eun, "EUN"));
			helper.CreateNewOrGetExistingDataGrouping(CountryCodes.Spain, "ES");
			helper.CreateNewOrGetExistingCusCodeType(unitedNationsPackageTypes, "UNPKG");
			helper.CreateNewOrGetExistingCusCodeType(packageTypes, "PKG");
			Create_CusCodeList_RefData(helper, eun, unitedNationsPackageTypes, "1A");
			Create_CusCodeList_RefData(helper, eun, packageTypes, "1B");
			Create_CusCodeList_RefData(helper, CountryCodes.Spain, unitedNationsPackageTypes, "1C");
			Create_CusCodeList_RefData(helper, eun, unitedNationsPackageTypes, "1D");
			Create_CusCodeList_RefData(helper, eun, unitedNationsPackageTypes, "1E");

			Factory.Save();

			var lookups = new PreviousDocumentLookups(Factory.New<PreviousDocument>());
			var packageCodeList = lookups.PackageCodeList;

			CombineAssertions(() =>
			{
				AssertEquals("Codes", "1A, 1D, 1E", packageCodeList.CodesAsString);
				AssertSame("Cached", lookups.PackageCodeList, packageCodeList);
			});
		}

		SupportingDocument GetSupportingDoc(int i, ZString refNumber, decimal qty3 = 10.0m)
		{
			var supDoc = Factory.New<SupportingDocument>();
			supDoc.SuspendValidation();

			supDoc.CSI_Code = "1234";
			supDoc.CSI_ReferenceNumber = refNumber;

			supDoc.CSI_Quantity = i * 10;
			supDoc.CSI_UnitOfQuantity = "BAG";
			supDoc.CSI_Quantity2 = i * 10.1;
			supDoc.CSI_UnitOfQuantity2 = "PKT";
			supDoc.CSI_Value = i * 1000;
			supDoc.CSI_RX_NKCurrency = "GBP";
			supDoc.CSI_DateOfIssue = new ZDateTime(2020, 01, 01);
			supDoc.CSI_DateOfExpiry = new ZDateTime(2020, 12, 31);
			supDoc.CSI_Type = "SUP";
			supDoc.CSI_Procedure = "A";
			supDoc.CSI_AdditionalDescription = "AddInfo";
			supDoc.CSI_ItemNumber = 2;
			supDoc.CSI_DataModel = Core.Constants.CountryCodes.Spain;

			return supDoc;
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();
			supportingDocument = invoiceHeader.SupportingDocuments.AddNew();
		}

		SupportingDocument supportingDocument;
		JobComInvoiceHeader invoiceHeader;
		JobDeclaration declaration;
	}
}
