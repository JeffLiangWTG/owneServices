using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public class ReadOnlySupportingDocument : NonPersistentBusinessObject, ISupportingDocumentEqualityKey
	{
		public ReadOnlySupportingDocument(SupportingDocument supportingDocument)
			: base(supportingDocument?.Factory)
		{
			this.supportingDocument = Argument.NotNull(supportingDocument, nameof(supportingDocument));
		}

		readonly SupportingDocument supportingDocument;

		public static class Schema
		{
			public const string CSI_Code = "CSI_Code";
			public const string CSI_ReferenceNumber = "CSI_ReferenceNumber";
			public const string CSI_Status = "CSI_Status";
			public const string CSI_Quantity = "CSI_Quantity";
			public const string CSI_UnitOfQuantity = "CSI_UnitOfQuantity";
			public const string CSI_Quantity2 = "CSI_Quantity2";
			public const string CSI_UnitOfQuantity2 = "CSI_UnitOfQuantity2";
			public const string CSI_Value = "CSI_Value";
			public const string CSI_RX_NKCurrency = "CSI_RX_NKCurrency";
			public const string CSI_DateOfIssue = "CSI_DateOfIssue";
			public const string CSI_DateOfExpiry = "CSI_DateOfExpiry";
			public const string CSI_Procedure = "CSI_Procedure";
			public const string CSI_CodeDescription = "CSI_CodeDescription";
			public const string CSI_SubType = "CSI_SubType";
			public const string CSI_AdditionalDescription = "CSI_AdditionalDescription";
			public const string CSI_ItemNumber = "CSI_ItemNumber";
			public const string CSI_DataModel = "CSI_DataModel";
			public const string IsDocumentHeader = "IsDocumentHeader";
		}

		[ResourceStringData("e4144755-6770-4377-a951-268a895c5386", Caption = "Type")]
		public ZString CSI_Code => supportingDocument.CSI_Code;
		public ZPropertyInfo CSI_CodeInfo => GetZPropertyInfo(Schema.CSI_Code);

		[ResourceStringData("feca62da-2efa-46ae-af0d-194e44d6c7cc", Caption = "Reference")]
		public ZString CSI_ReferenceNumber => supportingDocument.CSI_ReferenceNumber;
		public ZPropertyInfo CSI_ReferenceNumberInfo => GetZPropertyInfo(Schema.CSI_ReferenceNumber);

		[ResourceStringData("3d619207-208d-4272-b0ee-d9659c589185", Caption = "Availability")]
		public virtual ZString CSI_Status => supportingDocument.CSI_Status;
		public virtual ZPropertyInfo CSI_StatusInfo => GetZPropertyInfo(Schema.CSI_Status);

		[ResourceStringData("78072ed9-560a-4694-a608-9e2ed982c165", Caption = "Quantity", ShortCaption = "Qty")]
		public ZDecimal CSI_Quantity => supportingDocument.CSI_Quantity;
		public ZPropertyInfo CSI_QuantityInfo => GetZPropertyInfo(Schema.CSI_Quantity);

		[ResourceStringData("96a92b56-5e6d-438f-9adb-29d16c3554e0", Caption = "Unit Of Quantity", ShortCaption = "UOM")]
		public ZString CSI_UnitOfQuantity => supportingDocument.CSI_UnitOfQuantity;
		public ZPropertyInfo CSI_UnitOfQuantityInfo => GetZPropertyInfo(Schema.CSI_UnitOfQuantity);

		[ResourceStringData("b5a7680e-e714-4703-a80b-1a6d2e2161ff", Caption = "2nd/Estimated Quantity", ShortCaption = "2nd/Est. Qty")]
		public ZDecimal CSI_Quantity2 => supportingDocument.CSI_Quantity2;
		public ZPropertyInfo CSI_Quantity2Info => GetZPropertyInfo(Schema.CSI_Quantity2);

		[ResourceStringData("ceba9635-c224-4b61-88e9-c5698ea8d9d1", Caption = "2nd/Estimated UQ", ShortCaption = "2nd/Est. UQ")]
		public ZString CSI_UnitOfQuantity2 => supportingDocument.CSI_UnitOfQuantity2;
		public ZPropertyInfo CSI_UnitOfQuantity2Info => GetZPropertyInfo(Schema.CSI_UnitOfQuantity2);

		[ResourceStringData("70f8d4a9-ad02-4fb7-8ce8-6785fad617d8", Caption = "Value")]
		public ZDecimal CSI_Value => supportingDocument.CSI_Value;
		public ZPropertyInfo CSI_ValueInfo => GetZPropertyInfo(Schema.CSI_Value);

		[ResourceStringData("d97c6068-401a-4fa9-95ea-87121ef0d09a", Caption = "Currency")]
		public ZString CSI_RX_NKCurrency => supportingDocument.CSI_RX_NKCurrency;
		public ZPropertyInfo CSI_RX_NKCurrencyInfo => GetZPropertyInfo(Schema.CSI_RX_NKCurrency);

		[ResourceStringData("aa901930-83eb-4fdd-8ea4-eee60dc905de", Caption = "Date of Issue")]
		public ZDateTime CSI_DateOfIssue => supportingDocument.CSI_DateOfIssue;
		public ZPropertyInfo CSI_DateOfIssueInfo => GetZPropertyInfo(Schema.CSI_DateOfIssue);

		[ResourceStringData("9cf1ce6a-20e2-4513-a39a-af9a8e474de6", Caption = "Date of Expiry")]
		public ZDateTime CSI_DateOfExpiry => supportingDocument.CSI_DateOfExpiry;
		public ZPropertyInfo CSI_DateOfExpiryInfo => GetZPropertyInfo(Schema.CSI_DateOfExpiry);

		public virtual ZString CSI_Procedure => supportingDocument.CSI_Procedure;
		public virtual ZPropertyInfo CSI_ProcedureInfo => GetZPropertyInfo(Schema.CSI_Procedure);

		[ResourceStringData("971B1436-939A-45EF-8961-20B1C3EAD1AA", Caption = "Description", ShortCaption = "Desc.")]
		public ZString CSI_CodeDescription => supportingDocument.CSI_CodeDescription;
		public ZPropertyInfo CSI_CodeDescriptionInfo => GetZPropertyInfo(Schema.CSI_CodeDescription);

		[ResourceStringData("ED09C6F9-3E02-4D54-B8C4-76847070570B", Caption = "Sub Type")]
		public ZString CSI_SubType => supportingDocument.CSI_SubType;
		public ZPropertyInfo CSI_SubTypeInfo => GetZPropertyInfo(Schema.CSI_SubType);

		public virtual ZString CSI_AdditionalDescription => supportingDocument.CSI_AdditionalDescription;
		public ZPropertyInfo CSI_AdditionalDescriptionInfo => GetZPropertyInfo(Schema.CSI_AdditionalDescription);

		public virtual ZInt CSI_ItemNumber => supportingDocument.CSI_ItemNumber;
		public ZPropertyInfo CSI_ItemNumberInfo => GetZPropertyInfo(Schema.CSI_ItemNumber);

		public virtual ZString CSI_DataModel => supportingDocument.CSI_DataModel;
		public ZPropertyInfo CSI_DataModelInfo => GetZPropertyInfo(Schema.CSI_DataModel);

		public virtual ZBool IsDocumentHeader => false;
		public ZPropertyInfo IsDocumentHeaderInfo => GetZPropertyInfo(Schema.IsDocumentHeader);

		public SupportingDocument GetSupportingDocument() => supportingDocument;
	}
}
