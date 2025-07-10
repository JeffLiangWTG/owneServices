using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	[TestedType(typeof(PreviousDocument))]
	sealed class PreviousDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<PreviousDocument>
	{
		public void TestParentIsDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			var previousDocument = declaration.PreviousDocuments.AddNew();

			AssertEquals(true, previousDocument.ParentIsDeclaration);
			AssertEquals(false, previousDocument.ParentIsEntryInstruction);
			AssertEquals(false, previousDocument.ParentIsJobComInvoiceHeader);
			AssertEquals(false, previousDocument.ParentIsJobComInvoiceLine);
		}

		public void TestParentIsEntryInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var previousDocument = entryInstruction.PreviousDocuments.AddNew();

			AssertEquals(false, previousDocument.ParentIsDeclaration);
			AssertEquals(true, previousDocument.ParentIsEntryInstruction);
			AssertEquals(false, previousDocument.ParentIsJobComInvoiceHeader);
			AssertEquals(false, previousDocument.ParentIsJobComInvoiceLine);
		}

		public void TestParentIsJobComInvoiceHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var previousDocument = invoice.PreviousDocuments.AddNew();

			AssertEquals(false, previousDocument.ParentIsDeclaration);
			AssertEquals(false, previousDocument.ParentIsEntryInstruction);
			AssertEquals(true, previousDocument.ParentIsJobComInvoiceHeader);
			AssertEquals(false, previousDocument.ParentIsJobComInvoiceLine);
		}

		public void TestParentIsJobComInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var previousDocument = invoiceLine.PreviousDocuments.AddNew();

			AssertEquals(false, previousDocument.ParentIsDeclaration);
			AssertEquals(false, previousDocument.ParentIsEntryInstruction);
			AssertEquals(false, previousDocument.ParentIsJobComInvoiceHeader);
			AssertEquals(true, previousDocument.ParentIsJobComInvoiceLine);
		}

		public void TestCSI_LineNo_MaxLength()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invLine = declaration.InvoiceLines.AddNew();
			var previousDocument = invLine.PreviousDocuments.AddNew();
			AssertEquals(5, previousDocument.CSI_LineNoInfo.MaxLength);

			var invHeaderPreviousDocument = (PreviousDocument)GetNewBusinessObject();
			AssertEquals(5, invHeaderPreviousDocument.CSI_LineNoInfo.MaxLength);
		}

		public void TestCSI_PackQty()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invLine = declaration.InvoiceLines.AddNew();
			var previousDocument = invLine.PreviousDocuments.AddNew();
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForProperty(previousDocument.CSI_PackQtyInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Number of Packages", resourceStringDataAttribute.Caption);
				AssertEquals("MediumCaption", "Pack Qty", resourceStringDataAttribute.MediumCaption);
				AssertEquals("ShortCaption", "#Pkgs.", resourceStringDataAttribute.ShortCaption);
				AssertEquals("CSI_PackQtyMaxLength: MaxLength", 8, previousDocument.CSI_PackQtyInfo.GetAttribute<MaxLengthAttribute>().MaxLength);
			});
		}

		public void TestCSI_PackType()
		{
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForProperty(Factory.New<PreviousDocument>().CSI_PackTypeInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Type of Packages", resourceStringDataAttribute.Caption);
				AssertEquals("MediumCaption", "Pack Type", resourceStringDataAttribute.MediumCaption);
				AssertEquals("ShortCaption", "Pack Type", resourceStringDataAttribute.ShortCaption);
			});
		}

		public void TestCSI_RN_NKCountryCode()
		{
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForProperty(Factory.New<PreviousDocument>().CSI_RN_NKCountryCodeInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "CCI Validate Country", resourceStringDataAttribute.Caption);
				AssertEquals("MediumCaption", "CCI Valid. Country", resourceStringDataAttribute.MediumCaption);
				AssertEquals("ShortCaption", "CCI Country", resourceStringDataAttribute.ShortCaption);
				AssertEquals("FullDescription", "Only used in CCI. Identify the country who will validate the data", resourceStringDataAttribute.FullDescription);
			});
		}

		public void TestCopyFrom()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "BLT";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var prevDoc1 = Factory.New<PreviousDocument>();
			prevDoc1.CSI_Code = "1234";
			prevDoc1.CSI_SubType = "Y";
			prevDoc1.CSI_ReferenceNumber = "REFERENCE";
			prevDoc1.CSI_DateOfIssue = new ZDateTime(2020, 01, 01);
			prevDoc1.CSI_LineNo = 2;
			prevDoc1.CSI_Quantity = 20;
			prevDoc1.CSI_UnitOfQuantity = "BAG";
			prevDoc1.CSI_PackQty = 10;
			prevDoc1.CSI_PackType = "TYP";
			prevDoc1.CSI_DataModel = Core.Constants.CountryCodes.Spain;
			declaration.PreviousDocuments.Add(prevDoc1);
			var readOnlyPreviousDocumentCollection = new ReadOnlyPreviousDocumentCollection(entryLine);
			readOnlyPreviousDocumentCollection.LoadNew();
			var readOnlyPreviousDocument = readOnlyPreviousDocumentCollection.Cast<ReadOnlyPreviousDocument>().First();

			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("Null readonlyprevdoc", () => PreviousDocument.CopyFrom(entryLine, null));

				var prevDoc = PreviousDocument.CopyFrom(entryLine, readOnlyPreviousDocument);
				AssertEquals("CSI_Code", readOnlyPreviousDocument.CSI_Code, prevDoc.CSI_Code);
				AssertEquals("CSI_SubType", readOnlyPreviousDocument.CSI_SubType, prevDoc.CSI_SubType);
				AssertEquals("CSI_ReferenceNumber", readOnlyPreviousDocument.CSI_ReferenceNumber, prevDoc.CSI_ReferenceNumber);
				AssertEquals("CSI_DateOfIssue", readOnlyPreviousDocument.CSI_DateOfIssue, prevDoc.CSI_DateOfIssue);
				AssertEquals("CSI_LineNo", readOnlyPreviousDocument.CSI_LineNo, prevDoc.CSI_LineNo);
				AssertEquals("CSI_Quantity", readOnlyPreviousDocument.CSI_Quantity, prevDoc.CSI_Quantity);
				AssertEquals("CSI_UnitOfQuantity", readOnlyPreviousDocument.CSI_UnitOfQuantity, prevDoc.CSI_UnitOfQuantity);
				AssertEquals("CSI_DataModel", readOnlyPreviousDocument.CSI_DataModel, prevDoc.CSI_DataModel);
			});

			using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(true))
			{
				prevDoc1.Declaration.JE_MessageType = MessageTypeList.Codes.Import;
				CombineAssertions("Should copy CSI_PackQty and CSI_PackType only when declaration is ImportH1.", () =>
				{
					var prevDoc = PreviousDocument.CopyFrom(entryLine, readOnlyPreviousDocument);
					AssertEquals("CSI_PackType", readOnlyPreviousDocument.CSI_PackType, prevDoc.CSI_PackType);
					AssertEquals("CSI_PackQty", readOnlyPreviousDocument.CSI_PackQty, prevDoc.CSI_PackQty);
				});

				prevDoc1.Declaration.JE_MessageType = MessageTypeList.Codes.Export;
				CombineAssertions("Should not copy CSI_PackQty and CSI_PackType when declaration is other than ImportH1.", () =>
				{
					var prevDoc = PreviousDocument.CopyFrom(entryLine, readOnlyPreviousDocument);
					AssertNotEquals("CSI_PackType", readOnlyPreviousDocument.CSI_PackType, prevDoc.CSI_PackType);
					AssertNotEquals("CSI_PackQty", readOnlyPreviousDocument.CSI_PackQty, prevDoc.CSI_PackQty);
				});
			}

			using (RegistryTemporarySetterHelper.SetESImportMessageVersion(IMPORTVersionNumberList.Codes.Ics))
			{
				prevDoc1.Declaration.JE_MessageType = MessageTypeList.Codes.Import;
				CombineAssertions("Should not copy CSI_PackQty and CSI_PackType when declaration is Non UCC6.", () =>
				{
					var prevDoc = PreviousDocument.CopyFrom(entryLine, readOnlyPreviousDocument);
					AssertNotEquals("CSI_PackType", readOnlyPreviousDocument.CSI_PackType, prevDoc.CSI_PackType);
					AssertNotEquals("CSI_PackQty", readOnlyPreviousDocument.CSI_PackQty, prevDoc.CSI_PackQty);
				});

				prevDoc1.Declaration.JE_MessageType = MessageTypeList.Codes.Export;
				CombineAssertions("Should not copy CSI_PackQty and CSI_PackType when declaration is export.", () =>
				{
					var prevDoc = PreviousDocument.CopyFrom(entryLine, readOnlyPreviousDocument);
					AssertNotEquals("CSI_PackType", readOnlyPreviousDocument.CSI_PackType, prevDoc.CSI_PackType);
					AssertNotEquals("CSI_PackQty", readOnlyPreviousDocument.CSI_PackQty, prevDoc.CSI_PackQty);
				});
			}
		}

		public void TestMatchesPreviouslySentDocument()
		{
			var preDoc = GetPreviousDocuments(1, "REF111");
			var previouslySentPreDoc = GetPreviousDocuments(1, "REF111");

			CombineAssertions(() =>
			{
				AssertEquals("Object is equal", true, preDoc.MatchesPreviouslySentPreviousDocument(previouslySentPreDoc));

				previouslySentPreDoc.CSI_Code = "T";
				AssertEquals("CSI_Code: Object is not equal", false, preDoc.MatchesPreviouslySentPreviousDocument(previouslySentPreDoc));
				previouslySentPreDoc.CSI_Code = preDoc.CSI_Code;
				AssertEquals("CSI_Code: Object is equal", true, preDoc.MatchesPreviouslySentPreviousDocument(previouslySentPreDoc));

				previouslySentPreDoc.CSI_SubType = "AA";
				AssertEquals("CSI_SubType: Object is not equal", false, preDoc.MatchesPreviouslySentPreviousDocument(previouslySentPreDoc));
				previouslySentPreDoc.CSI_SubType = preDoc.CSI_SubType;
				AssertEquals("CSI_SubType: Object is equal", true, preDoc.MatchesPreviouslySentPreviousDocument(previouslySentPreDoc));

				previouslySentPreDoc.CSI_ReferenceNumber = "T";
				AssertEquals("CSI_ReferenceNumber: Object is not equal", false, preDoc.MatchesPreviouslySentPreviousDocument(previouslySentPreDoc));
				previouslySentPreDoc.CSI_ReferenceNumber = preDoc.CSI_ReferenceNumber;
				AssertEquals("CSI_ReferenceNumber: Object is equal", true, preDoc.MatchesPreviouslySentPreviousDocument(previouslySentPreDoc));

				previouslySentPreDoc.CSI_DateOfIssue = new ZDateTime(2020, 12, 01);
				AssertEquals("CSI_DateOfIssue: Object is not equal", false, preDoc.MatchesPreviouslySentPreviousDocument(previouslySentPreDoc));
				previouslySentPreDoc.CSI_DateOfIssue = preDoc.CSI_DateOfIssue;
				AssertEquals("CSI_DateOfIssue: Object is equal", true, preDoc.MatchesPreviouslySentPreviousDocument(previouslySentPreDoc));

				previouslySentPreDoc.CSI_LineNo = 0;
				AssertEquals("CSI_LineNo: Object is not equal", false, preDoc.MatchesPreviouslySentPreviousDocument(previouslySentPreDoc));
				previouslySentPreDoc.CSI_LineNo = preDoc.CSI_LineNo;
				AssertEquals("CSI_LineNo: Object is equal", true, preDoc.MatchesPreviouslySentPreviousDocument(previouslySentPreDoc));

				previouslySentPreDoc.CSI_UnitOfQuantity = "T";
				AssertEquals("CSI_UnitOfQuantity: Object is not equal", false, preDoc.MatchesPreviouslySentPreviousDocument(previouslySentPreDoc));
				previouslySentPreDoc.CSI_UnitOfQuantity = preDoc.CSI_UnitOfQuantity;
				AssertEquals("CSI_UnitOfQuantity: Object is equal", true, preDoc.MatchesPreviouslySentPreviousDocument(previouslySentPreDoc));

				previouslySentPreDoc.CSI_Quantity = 0;
				AssertEquals("CSI_Quantity: Object is not equal", false, preDoc.MatchesPreviouslySentPreviousDocument(previouslySentPreDoc));
				previouslySentPreDoc.CSI_Quantity = preDoc.CSI_Quantity;
				AssertEquals("CSI_Quantity: Object is equal", true, preDoc.MatchesPreviouslySentPreviousDocument(previouslySentPreDoc));
			});

			using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(true))
			{
				preDoc.Declaration.JE_MessageType = MessageTypeList.Codes.Import;
				CombineAssertions("Should compare CSI_PackQty and CSI_PackType only when declaration is ImportH1.", () =>
				{
					previouslySentPreDoc.CSI_PackQty = 0;
					AssertEquals("CSI_PackQty: Object is not equal", false, preDoc.MatchesPreviouslySentPreviousDocument(previouslySentPreDoc));
					previouslySentPreDoc.CSI_PackQty = preDoc.CSI_PackQty;
					AssertEquals("CSI_PackQty: Object is equal", true, preDoc.MatchesPreviouslySentPreviousDocument(previouslySentPreDoc));

					previouslySentPreDoc.CSI_PackType = "";
					AssertEquals("CSI_PackType: Object is not equal", false, preDoc.MatchesPreviouslySentPreviousDocument(previouslySentPreDoc));
					previouslySentPreDoc.CSI_PackType = preDoc.CSI_PackType;
					AssertEquals("CSI_PackType: Object is equal", true, preDoc.MatchesPreviouslySentPreviousDocument(previouslySentPreDoc));
				});

				preDoc.Declaration.JE_MessageType = MessageTypeList.Codes.Export;
				CombineAssertions("Should not compare CSI_PackQty and CSI_PackType when declaration is other than ImportH1.", () =>
				{
					previouslySentPreDoc.CSI_PackQty = 0;
					AssertEquals("CSI_PackQty: Object is equal", true, preDoc.MatchesPreviouslySentPreviousDocument(previouslySentPreDoc));

					previouslySentPreDoc.CSI_PackType = "";
					AssertEquals("CSI_PackType: Object is equal", true, preDoc.MatchesPreviouslySentPreviousDocument(previouslySentPreDoc));
				});
			}

			using (RegistryTemporarySetterHelper.SetESImportMessageVersion(IMPORTVersionNumberList.Codes.Ics))
			{
				preDoc.Declaration.JE_MessageType = MessageTypeList.Codes.Import;
				CombineAssertions("Should Not compare CSI_PackQty and CSI_PackType when declaration is ICS.", () =>
				{
					previouslySentPreDoc.CSI_PackQty = 0;
					AssertEquals("CSI_PackQty: Object is equal", true, preDoc.MatchesPreviouslySentPreviousDocument(previouslySentPreDoc));

					previouslySentPreDoc.CSI_PackType = "";
					AssertEquals("CSI_PackType: Object is equal", true, preDoc.MatchesPreviouslySentPreviousDocument(previouslySentPreDoc));
				});

				preDoc.Declaration.JE_MessageType = MessageTypeList.Codes.Export;
				CombineAssertions("Should Not compare CSI_PackQty and CSI_PackType when declaration is export.", () =>
				{
					previouslySentPreDoc.CSI_PackQty = 0;
					AssertEquals("CSI_PackQty: Object is equal", true, preDoc.MatchesPreviouslySentPreviousDocument(previouslySentPreDoc));

					previouslySentPreDoc.CSI_PackType = "";
					AssertEquals("CSI_PackType: Object is equal", true, preDoc.MatchesPreviouslySentPreviousDocument(previouslySentPreDoc));
				});
			}
		}

		PreviousDocument GetPreviousDocuments(int i, ZString refNumber)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var preDoc = declaration.PreviousDocuments.AddNew();
			preDoc.SuspendValidation();

			preDoc.CSI_Code = "1234";
			preDoc.CSI_SubType = "SUP";
			preDoc.CSI_ReferenceNumber = refNumber;
			preDoc.CSI_DateOfIssue = new ZDateTime(2020, 01, 01);
			preDoc.CSI_LineNo = 2;
			preDoc.CSI_UnitOfQuantity = "BAG";
			preDoc.CSI_Quantity = i * 10;
			preDoc.CSI_PackType = "PAC";
			preDoc.CSI_PackQty = i * 10;

			return preDoc;
		}

		protected override IEnumerable<PreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			yield return invoice.PreviousDocuments.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			yield return invoiceLine.PreviousDocuments.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject() => Factory.New<JobDeclaration>().Invoices.AddNew().PreviousDocuments.AddNew();
	}
}
