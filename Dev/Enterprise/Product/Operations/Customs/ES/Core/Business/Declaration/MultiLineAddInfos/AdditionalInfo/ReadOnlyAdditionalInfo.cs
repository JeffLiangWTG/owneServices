using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class ReadOnlyAdditionalInfo : NonPersistentBusinessObject, IAdditionalInfoEqualityKey
	{
		public ReadOnlyAdditionalInfo(AdditionalInfo additionalInfo)
			: base(additionalInfo?.Factory)
		{
			this.additionalInfo = Argument.NotNull(additionalInfo, nameof(additionalInfo));
		}

		readonly AdditionalInfo additionalInfo;

		public static class Schema
		{
			public const string CSI_Code = "CSI_Code";
			public const string CSI_Description = "CSI_Description";
			public const string CSI_SubType = "CSI_SubType";
			public const string CSI_ReferenceNumber = "CSI_ReferenceNumber";
			public const string CSI_ReferenceNumber2 = "CSI_ReferenceNumber2";
			public const string CSI_RX_NKCurrency = "CSI_RX_NKCurrency";
			public const string CSI_Value = "CSI_Value";
			public const string CSI_Status = "CSI_Status";
			public const string CSI_DataModel = "CSI_DataModel";
		}

		[ResourceStringData("DD725B6C-3170-4331-B7DB-5CC7435A0614", Caption = "Full Type")]
		public ZString CSI_Code => additionalInfo.CSI_Code;
		public ZPropertyInfo CSI_CodeInfo => GetZPropertyInfo(Schema.CSI_Code);

		[ResourceStringData("D191CD9E-F199-4346-91A8-6DC76D85D4D8", Caption = "Description")]
		public ZString CSI_Description => additionalInfo.CSI_Description;
		public ZPropertyInfo CSI_DescriptionInfo => GetZPropertyInfo(Schema.CSI_Description);

		[ResourceStringData("98785B3B-AC92-4C17-93D4-B22A522A4253", Caption = "Kind")]
		public ZString CSI_SubType => additionalInfo.CSI_SubType;
		public ZPropertyInfo CSI_SubTypeInfo => GetZPropertyInfo(Schema.CSI_SubType);

		[ResourceStringData("3551F36F-C023-49A1-A5EE-E51155D086A5", Caption = "Reference")]
		public ZString CSI_ReferenceNumber => additionalInfo.CSI_ReferenceNumber;
		public ZPropertyInfo CSI_ReferenceNumberInfo => GetZPropertyInfo(Schema.CSI_ReferenceNumber);

		[ResourceStringData("A0AF6A4E-16D1-482C-9512-9CEF916A383B", Caption = "Detail")]
		public ZString CSI_ReferenceNumber2 => additionalInfo.CSI_ReferenceNumber2;
		public ZPropertyInfo CSI_ReferenceNumber2Info => GetZPropertyInfo(Schema.CSI_ReferenceNumber2);

		[ResourceStringData("304DF9AA-BE57-44AB-91D7-DC1AA9DCE8C0", Caption = "Currency")]
		public ZString CSI_RX_NKCurrency => additionalInfo.CSI_RX_NKCurrency;
		public ZPropertyInfo CSI_RX_NKCurrencyInfo => GetZPropertyInfo(Schema.CSI_RX_NKCurrency);

		[ResourceStringData("865BC386-9AE0-43F0-96BD-F2C3E8EE56A6", Caption = "Amount")]
		public ZDecimal CSI_Value => additionalInfo.CSI_Value;
		public ZPropertyInfo CSI_ValueInfo => GetZPropertyInfo(Schema.CSI_Value);

		[ResourceStringData("987DE083-3AEA-4C0C-81BC-51F3D60D2A38", Caption = "Status")]
		public ZString CSI_Status => additionalInfo.CSI_Status;
		public ZPropertyInfo CSI_StatusInfo => GetZPropertyInfo(Schema.CSI_Status);

		public ZString CSI_DataModel => additionalInfo.CSI_DataModel;
		public ZPropertyInfo CSI_DataModelInfo => GetZPropertyInfo(Schema.CSI_DataModel);

		public AdditionalInfo GetAdditionalInfo() => additionalInfo;
	}
}
