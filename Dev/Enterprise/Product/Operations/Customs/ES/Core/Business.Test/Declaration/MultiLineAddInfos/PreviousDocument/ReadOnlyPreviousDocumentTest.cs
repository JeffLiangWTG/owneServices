using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(ReadOnlyPreviousDocument))]
	sealed class ReadOnlyPreviousDocumentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null Previous Documnet", () => new ReadOnlyPreviousDocument(null));
		}

		public void TestProperties()
		{
			var previousDocument = GetPreviousDoc(1, "REF111");

			var readOnlyPreviousDocument = new ReadOnlyPreviousDocument(previousDocument);

			CombineAssertions("ReadOnlyPreviousDocument", () =>
			{
				AssertEquals("CSI_Code", previousDocument.CSI_Code, readOnlyPreviousDocument.CSI_Code);
				AssertEquals("CSI_SubType", previousDocument.CSI_SubType, readOnlyPreviousDocument.CSI_SubType);
				AssertEquals("CSI_ReferenceNumber", previousDocument.CSI_ReferenceNumber, readOnlyPreviousDocument.CSI_ReferenceNumber);
				AssertEquals("CSI_DateOfIssue", previousDocument.CSI_DateOfIssue, readOnlyPreviousDocument.CSI_DateOfIssue);
				AssertEquals("CSI_LineNo", previousDocument.CSI_LineNo, readOnlyPreviousDocument.CSI_LineNo);
				AssertEquals("CSI_UnitOfQuantity", previousDocument.CSI_UnitOfQuantity, readOnlyPreviousDocument.CSI_UnitOfQuantity);
				AssertEquals("CSI_Quantity", previousDocument.CSI_Quantity, readOnlyPreviousDocument.CSI_Quantity);
				AssertEquals("CSI_Status", previousDocument.CSI_Status, readOnlyPreviousDocument.CSI_Status);
				AssertEquals("CSI_PactQty", previousDocument.CSI_PackQty, readOnlyPreviousDocument.CSI_PackQty);
				AssertEquals("CSI_Type", previousDocument.CSI_PackType, readOnlyPreviousDocument.CSI_PackType);
				AssertEquals("CSI_DataModel", previousDocument.CSI_DataModel, readOnlyPreviousDocument.CSI_DataModel);
			});
		}

		public void TestCalculatedProperties()
		{
			var previousDocument = GetPreviousDoc(1, "REF111");

			var readOnlyPreviousDocument = new ReadOnlyPreviousDocument(previousDocument, "KGM", 5m);

			CombineAssertions("ReadOnlyPreviousDocument", () =>
			{
				AssertEquals("CSI_UnitOfQuantity", "KGM", readOnlyPreviousDocument.CSI_UnitOfQuantity);
				AssertEquals("CSI_Quantity", 5m, readOnlyPreviousDocument.CSI_Quantity);
				AssertNotEquals("Previous Doc CSI_UnitOfQuantity not equal", previousDocument.CSI_UnitOfQuantity, readOnlyPreviousDocument.CSI_UnitOfQuantity);
				AssertNotEquals("Previous Doc CSI_Quantity not equal", previousDocument.CSI_Quantity, readOnlyPreviousDocument.CSI_Quantity);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var previousDocument = GetPreviousDoc(1, "REF111");

			return new ReadOnlyPreviousDocument(previousDocument);
		}

		PreviousDocument GetPreviousDoc(int i, ZString refNumber)
		{
			var prevDoc = Factory.New<PreviousDocument>();
			prevDoc.SuspendValidation();

			prevDoc.CSI_Code = "1234";
			prevDoc.CSI_SubType = "Y";
			prevDoc.CSI_ReferenceNumber = refNumber;
			prevDoc.CSI_DateOfIssue = new ZDateTime(2020, 01, 01);
			prevDoc.CSI_LineNo = i;
			prevDoc.CSI_Quantity = i * 10;
			prevDoc.CSI_UnitOfQuantity = "BAG";
			prevDoc.CSI_Status = "QWE";
			prevDoc.CSI_PackQty = 2;
			prevDoc.CSI_PackType = "Typ";
			prevDoc.CSI_DataModel = Core.Constants.CountryCodes.Spain;

			return prevDoc;
		}
	}
}
