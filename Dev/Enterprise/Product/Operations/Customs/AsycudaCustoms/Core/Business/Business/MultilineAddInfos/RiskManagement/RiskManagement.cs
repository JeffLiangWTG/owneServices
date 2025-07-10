using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class RiskManagement : CusSupportingInfo
	{
		public RiskManagement(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public new const int CSI_CodeMaxLength = 3;
		}

		[MaxLength(Schema.CSI_CodeMaxLength)]
		[ResourceStringData("eebd17b4-a009-4aa8-a244-ac5ad1a4db9a|CSI_Code", Caption = "Entry/Permit Type")]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set
			{
				var oldValue = CSI_Code;
				base.CSI_Code = value;
				if (!IsCopying && oldValue != CSI_Code)
				{
					DefaultValuesFromEntryIfNeeded();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(RiskManagementLookups.EntryHeaders))]
		[ResourceStringData("ba348705-ca74-46e9-8453-887fe752fb31|CSI_ReferenceNumber", Caption = "Entry/Permit Number")]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set
			{
				var oldValue = CSI_ReferenceNumber;
				base.CSI_ReferenceNumber = value;
				if (!IsCopying && oldValue != CSI_ReferenceNumber)
				{
					DefaultValuesFromEntryIfNeeded();
				}
			}
		}

		public bool IsEntryType => CSI_Code == EntryPermitTypeList.Codes.EntryDeclaration;

		public ZString CSI_ReferenceNumberFieldType
				=> (IsEntryType ? FieldType.TextCodeFindBox : FieldType.Text).ToString();

		public bool HasEntryNumber => IsEntryType && !CSI_ReferenceNumber.IsEmpty;

		[ResourceStringData("10fca41b-de68-4ba6-a68f-ebf509913cc4|CSI_DateOfIssue", Caption = "Date")]
		public override ZDateTime CSI_DateOfIssue
		{
			get => base.CSI_DateOfIssue;
			set => base.CSI_DateOfIssue = value;
		}

		[DecimalPlaces(2)]
		[ResourceStringData("97d39cda-4a80-481f-ac82-6a913658d034|CSI_Value", Caption = "Customs Value")]
		public override ZDecimal CSI_Value
		{
			get => base.CSI_Value;
			set
			{
				var oldValue = CSI_Value;
				base.CSI_Value = value;
				if (!IsCopying && oldValue != CSI_Value)
				{
					Parent?.RemainingCustomsValueInfo.RefreshBinding();
				}
			}
		}

		[DecimalPlaces(3)]
		[ResourceStringData("6d817a89-3fba-4171-b7f8-beb3db5ad330|CSI_Quantity", Caption = "Net Weight in KG")]
		public override ZDecimal CSI_Quantity
		{
			get => base.CSI_Quantity;
			set
			{
				var oldValue = CSI_Quantity;
				base.CSI_Quantity = value;
				if (!IsCopying && oldValue != CSI_Quantity)
				{
					Parent?.RemainingNetWeightKilogramsInfo.RefreshBinding();
				}
			}
		}

		[DecimalPlaces(5)]
		[ResourceStringData("9c9ef55b-f0f6-4414-8d13-2163f40f4e42|CSI_Quantity2", Caption = "Customs Qty")]
		public override ZDecimal CSI_Quantity2
		{
			get => base.CSI_Quantity2;
			set
			{
				var oldValue = CSI_Quantity2;
				base.CSI_Quantity2 = value;
				if (!IsCopying && oldValue != CSI_Quantity2)
				{
					Parent?.RemainingCustomsQuantityInfo.RefreshBinding();
				}
			}
		}

		[ResourceStringData("c874796a-1c7c-41f6-9c41-56756d50822f|CSI_AdditionalDescription", Caption = "Comments")]
		public override ZString CSI_AdditionalDescription
		{
			get => base.CSI_AdditionalDescription;
			set => base.CSI_AdditionalDescription = value;
		}

		public new RiskManagementLookups Lookups => (RiskManagementLookups)base.Lookups;

		public new RiskManagementValidation Validation => (RiskManagementValidation)base.Validation;

		protected override CusSupportingInfoLookups GetNewLookups() => new RiskManagementLookups(this);

		protected override CusSupportingInfoValidation GetNewValidation() => new RiskManagementValidation(this);

		public new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

		void DefaultValuesFromEntryIfNeeded()
		{
			if (HasEntryNumber &&
				Parent?.JobDeclaration is JobDeclaration declaration &&
				new CusEntryHeader.Loader(Factory).FindByEntryNumberAndMessageType(CSI_ReferenceNumber, declaration.JE_MessageType) is CusEntryHeader entry)
			{
				CSI_Value = entry.CustomsValue.Round(2);
				CSI_Quantity = entry.NetWeightKilograms.Round(3);
				CSI_Quantity2 = entry.CustomsQuantity.Round(5);
				CSI_DateOfIssue = entry.CH_EntryReleaseDate.Date;
			}
		}
	}
}
