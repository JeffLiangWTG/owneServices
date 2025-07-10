using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class ReadOnlyPreviousDocument : NonPersistentBusinessObject, IPreviousDocumentEqualityKey
	{
		public ReadOnlyPreviousDocument(PreviousDocument previousDocument, string calculatedUOM = "", decimal calculatedQuantity = 0)
			: base(previousDocument?.Factory)
		{
			this.previousDocument = Argument.NotNull(previousDocument, nameof(previousDocument));
			this.calculatedUOM = calculatedUOM;
			this.calculatedQuantity = calculatedQuantity;
		}

		readonly PreviousDocument previousDocument;
		protected readonly ZString calculatedUOM;
		protected readonly ZDecimal calculatedQuantity;

		public static class Schema
		{
			public const string CSI_Code = "CSI_Code";
			public const string CSI_SubType = "CSI_SubType";
			public const string CSI_ReferenceNumber = "CSI_ReferenceNumber";
			public const string CSI_DateOfIssue = "CSI_DateOfIssue";
			public const string CSI_LineNo = "CSI_LineNo";
			public const string CSI_UnitOfQuantity = "CSI_UnitOfQuantity";
			public const string CSI_Quantity = "CSI_Quantity";
			public const string CSI_Status = "CSI_Status";
			public const string CSI_PackQty = "CSI_PackQty";
			public const string CSI_PackType = "CSI_PackType";
			public const string CSI_DataModel = "CSI_DataModel";
		}

		[ResourceStringData("AC128918-7F75-4DFB-9C25-2818DC45D8F0", Caption = "Type")]
		public ZString CSI_Code => previousDocument.CSI_Code;
		public ZPropertyInfo CSI_CodeInfo => GetZPropertyInfo(Schema.CSI_Code);

		[ResourceStringData("D5222988-0A85-4C6D-8AA5-282D3BD32F1A", Caption = "Class")]
		public ZString CSI_SubType => previousDocument.CSI_SubType;
		public ZPropertyInfo CSI_SubTypeInfo => GetZPropertyInfo(Schema.CSI_SubType);

		[ResourceStringData("08B945F4-8E90-4B55-8A8D-D09A5F04091E", Caption = "Reference")]
		public ZString CSI_ReferenceNumber => previousDocument.CSI_ReferenceNumber;
		public ZPropertyInfo CSI_ReferenceNumberInfo => GetZPropertyInfo(Schema.CSI_ReferenceNumber);

		[ResourceStringData("758A518F-BFCF-4AD3-9665-5EA73FB23C7C", Caption = "Date Of Issue")]
		public ZDateTime CSI_DateOfIssue => previousDocument.CSI_DateOfIssue;
		public ZPropertyInfo CSI_DateOfIssueInfo => GetZPropertyInfo(Schema.CSI_DateOfIssue);

		[ResourceStringData("5BCEF9DF-01E4-4D50-8537-FA0E9CF2A72F", Caption = "Line No.")]
		public ZInt CSI_LineNo => previousDocument.CSI_LineNo;
		public ZPropertyInfo CSI_LineNoInfo => GetZPropertyInfo(Schema.CSI_LineNo);

		[ResourceStringData("0D40161E-08DF-45DD-B7CA-E6DD60DE2692", Caption = "Unit Of Quantity", ShortCaption = "UOM")]
		public ZString CSI_UnitOfQuantity => calculatedUOM.IsEmpty ? previousDocument.CSI_UnitOfQuantity : calculatedUOM;
		public ZPropertyInfo CSI_UnitOfQuantityInfo => GetZPropertyInfo(Schema.CSI_UnitOfQuantity);

		[ResourceStringData("D08D3772-6536-4392-86BB-83C512ECB858", Caption = "Quantity", ShortCaption = "Qty")]
		public ZDecimal CSI_Quantity => calculatedQuantity.IsEmpty ? previousDocument.CSI_Quantity : calculatedQuantity;
		public ZPropertyInfo CSI_QuantityInfo => GetZPropertyInfo(Schema.CSI_Quantity);

		[ResourceStringData("70EB4C09-015D-4AA9-84CA-65E4D51220A1", Caption = "Status")]
		public ZString CSI_Status => previousDocument.CSI_Status;
		public ZPropertyInfo CSI_StatusInfo => GetZPropertyInfo(Schema.CSI_Status);

		[ResourceStringData("45C83EAA-2E08-431E-B5E6-EEEF8DFD098C", Caption = "Number of Packages", MediumCaption = "Pack Qty", ShortCaption = "#Pkgs.")]
		public ZInt CSI_PackQty => previousDocument.CSI_PackQty;
		public ZPropertyInfo CSI_PackQtyInfo => GetZPropertyInfo(Schema.CSI_PackQty);

		[ResourceStringData("F474DFE1-40AD-4871-991A-EF39258A2BF5", Caption = "Type of Packages", MediumCaption = "Pack Type", ShortCaption = "Pack Type")]
		public ZString CSI_PackType => previousDocument.CSI_PackType;
		public ZPropertyInfo CSI_PackTypeInfo => GetZPropertyInfo(Schema.CSI_PackType);

		public ZString CSI_DataModel => previousDocument.CSI_DataModel;
		public ZPropertyInfo CSI_DataModelInfo => GetZPropertyInfo(Schema.CSI_DataModel);

		public PreviousDocument GetPreviousDocument() => previousDocument;
	}
}
