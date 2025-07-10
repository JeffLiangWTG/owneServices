using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing
{
	[TestedType(typeof(ReadOnlySupportingDocument))]
	class ReadOnlySupportingDocumentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var supportingDocument = GetSupportingDoc(1, "REF111");

			var readOnlySupportingDocument = new ReadOnlySupportingDocument(supportingDocument);

			CombineAssertions("ReadOnlySupportingDocument", () =>
			{
				AssertEquals("CSI_Code", supportingDocument.CSI_Code, readOnlySupportingDocument.CSI_Code);
				AssertEquals("CSI_ReferenceNumber", supportingDocument.CSI_ReferenceNumber, readOnlySupportingDocument.CSI_ReferenceNumber);
				AssertEquals("CSI_Status", supportingDocument.CSI_Status, readOnlySupportingDocument.CSI_Status);
				AssertEquals("CSI_Quantity", supportingDocument.CSI_Quantity, readOnlySupportingDocument.CSI_Quantity);
				AssertEquals("CSI_UnitOfQuantity", supportingDocument.CSI_UnitOfQuantity, readOnlySupportingDocument.CSI_UnitOfQuantity);
				AssertEquals("CSI_Quantity2", supportingDocument.CSI_Quantity2, readOnlySupportingDocument.CSI_Quantity2);
				AssertEquals("CSI_UnitOfQuantity2", supportingDocument.CSI_UnitOfQuantity2, readOnlySupportingDocument.CSI_UnitOfQuantity2);
				AssertEquals("CSI_Value", supportingDocument.CSI_Value, readOnlySupportingDocument.CSI_Value);
				AssertEquals("CSI_RX_NKCurrency", supportingDocument.CSI_RX_NKCurrency, readOnlySupportingDocument.CSI_RX_NKCurrency);
				AssertEquals("CSI_DateOfIssue", supportingDocument.CSI_DateOfIssue, readOnlySupportingDocument.CSI_DateOfIssue);
				AssertEquals("CSI_DateOfExpiry", supportingDocument.CSI_DateOfExpiry, readOnlySupportingDocument.CSI_DateOfExpiry);
				AssertEquals("CSI_Procedure", supportingDocument.CSI_Procedure, readOnlySupportingDocument.CSI_Procedure);
				AssertEquals("CSI_CodeDescription", supportingDocument.CSI_CodeDescription, readOnlySupportingDocument.CSI_CodeDescription);
				AssertEquals("CSI_SubType", supportingDocument.CSI_SubType, readOnlySupportingDocument.CSI_SubType);
				AssertEquals("CSI_AdditionalDescription", supportingDocument.CSI_AdditionalDescription, readOnlySupportingDocument.CSI_AdditionalDescription);
				AssertEquals("CSI_ItemNumber", supportingDocument.CSI_ItemNumber, readOnlySupportingDocument.CSI_ItemNumber);
				AssertEquals("CSI_DataModel", supportingDocument.CSI_DataModel, readOnlySupportingDocument.CSI_DataModel);
				AssertEquals("IsDocumentHeader", false, readOnlySupportingDocument.IsDocumentHeader);
			});
		}

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null Supporting Documnet", () => new ReadOnlySupportingDocument(null));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var supportingDocument = GetSupportingDoc(1, "REF111");

			return new ReadOnlySupportingDocument(supportingDocument);
		}

		protected virtual SupportingDocument GetSupportingDoc(int i, ZString refNumber, decimal qty3 = 10.0m)
		{
			var supDoc = Factory.New<SupportingDocument>();
			supDoc.SuspendValidation();

			supDoc.CSI_Code = "1234";
			supDoc.CSI_ReferenceNumber = refNumber;
			supDoc.CSI_SubType = "A"; //Part

			supDoc.CSI_Quantity = i * 10;
			supDoc.CSI_UnitOfQuantity = "BAG";
			supDoc.CSI_Quantity2 = i * 10.1;
			supDoc.CSI_UnitOfQuantity2 = "PKT";
			supDoc.CSI_Value = i * 1000;
			supDoc.CSI_RX_NKCurrency = "GBP";
			supDoc.CSI_DateOfIssue = new ZDateTime(2020, 01, 01);
			supDoc.CSI_DateOfExpiry = new ZDateTime(2020, 12, 31);
			supDoc.CSI_Quantity3 = qty3;

			supDoc.CSI_Description = "Testing"; //reason
			supDoc.CSI_ReferenceNumber2 = "REFNUM2"; //Issueing Authority
			supDoc.CSI_AdditionalDescription = "AddDescr";
			supDoc.CSI_CustomsOffice = "ABC";
			supDoc.CSI_Procedure = "XYZ";
			supDoc.CSI_RN_NKCountryCode = "GB";
			supDoc.CSI_Status = "QWE";
			supDoc.CSI_Tariff = "12345";
			supDoc.CSI_Type = "SUP";
			supDoc.CSI_UnitOfQuantity3 = "U3";

			supDoc.CSI_AdditionalDescription = "AddInfo";
			supDoc.CSI_DataModel = "ES";
			supDoc.CSI_ItemNumber = 2;

			return supDoc;
		}
	}
}
