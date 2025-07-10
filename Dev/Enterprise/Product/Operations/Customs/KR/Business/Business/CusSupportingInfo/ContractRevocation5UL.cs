using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ContractRevocation5UL : CusSupportingInfo
	{
		public ContractRevocation5UL(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public const int CR5_CodeMaxLength = 1;
			public const int DisposalNumberMaxLength = 20;
			public const int CR5_AdditionalDescriptionMaxLength = 100;
			public const int CR5_ReferenceNumber2MaxLength = 65;
			public const int CR5_DescriptionMaxLength = 500;
			public const int ExportEntryNumberMaxLength = 15;
			public const int ExportEntryLineNoMaxLength = 3;
		}

		[MaxLength(Schema.CR5_CodeMaxLength)]
		[ReadOnlyMember(nameof(IsRefundTypeContractRevocationReadOnly))]
		[ResourceStringData("0D36D4E4-9EA6-4ABC-AC39-E0EA740D2A8C", Caption = "Cancel Reason")]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set
			{
				var oldValue = CSI_Code;
				base.CSI_Code = value;
				if (oldValue != value)
				{
					CSI_ReferenceNumber = ZString.Empty;
					if (value == CancelReasonCodeList.Codes.A)
					  {
						CSI_DateOfExpiry = ZDateTime.Empty;
						CSI_AdditionalDescription = ZString.Empty;
						CSI_ReferenceNumber2 = ZString.Empty;
						CSI_Description = ZString.Empty;
					}
					else
					{
						CSI_LineNo = ZInt.Zero;
					}
				}
			}
		}

		[ReadOnlyMember(nameof(IsRefundTypeContractRevocationReadOnly))]
		[ResourceStringData("2EE58F42-279E-4076-B198-68922B700F69", Caption = "Disposal Date")]
		public override ZDateTime CSI_DateOfExpiry { get => base.CSI_DateOfExpiry; set => base.CSI_DateOfExpiry = value; }

		[MaxLength(Schema.CR5_AdditionalDescriptionMaxLength)]
		[ReadOnlyMember(nameof(IsRefundTypeContractRevocationReadOnly))]
		[ResourceStringData("62F1B156-9166-4D8F-87D7-AFF85644AD5B", ShortCaption = "Goods Location Desc.", Caption = "Goods Location Description")]
		public override ZString CSI_AdditionalDescription { get => base.CSI_AdditionalDescription; set => base.CSI_AdditionalDescription = value; }

		[MaxLength(Schema.CR5_ReferenceNumber2MaxLength)]
		[ReadOnlyMember(nameof(IsRefundTypeContractRevocationReadOnly))]
		[ResourceStringData("7D922C96-2369-4FA7-8E5F-7A69A753D3A4", ShortCaption = "Residual Substance Desc.", Caption = "Residual Substance Description")]
		public override ZString CSI_ReferenceNumber2 { get => base.CSI_ReferenceNumber2; set => base.CSI_ReferenceNumber2 = value; }

		[MaxLength(Schema.CR5_DescriptionMaxLength)]
		[ReadOnlyMember(nameof(IsRefundTypeContractRevocationReadOnly))]
		[ResourceStringData("E4588C5C-C79E-4903-B5DB-22376A4EDFA9", Caption = "Damage Situation")]
		public override ZString CSI_Description { get => base.CSI_Description; set => base.CSI_Description = value; }

		[MaxLength(Schema.ExportEntryNumberMaxLength)]
		[ReadOnlyMember(nameof(IsReExportReadOnly))]
		[BusinessObjectMaxLengthTestExclude]
		[ResourceStringData("C6A3A29C-9545-4104-9430-367E2DD25E30", ShortCaption = "EXP Entry No.", Caption = "Export Entry Number")]
		public ZString ExportEntryNumber
		{
			get => CSI_Code == CancelReasonCodeList.Codes.A ? MessageFunctions.DeclarationNumberFormat(CSI_ReferenceNumber) : ZString.Empty;
			set
			{
				CSI_ReferenceNumber = value;
				ExportEntryNumberInfo.RefreshBinding();
				CSI_ReferenceNumberInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo ExportEntryNumberInfo => GetZPropertyInfo(nameof(ExportEntryNumber));

		[MaxLength(Schema.DisposalNumberMaxLength)]
		[ReadOnlyMember(nameof(IsDisposalReadOnly))]
		[BusinessObjectMaxLengthTestExclude]
		[ResourceStringData("697D9765-77E2-467D-ADEF-BEDA48A388E3", Caption = "Disposal Number")]
		public ZString DisposalNumber
		{
			get => CSI_Code == CancelReasonCodeList.Codes.B ? CSI_ReferenceNumber : ZString.Empty;
			set
			{
				CSI_ReferenceNumber = value;
				DisposalNumberInfo.RefreshBinding();
				CSI_ReferenceNumberInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo DisposalNumberInfo => GetZPropertyInfo(nameof(DisposalNumber));

		[MaxLength(Schema.ExportEntryLineNoMaxLength)]
		[ReadOnlyMember(nameof(IsReExportReadOnly))]
		[BusinessObjectMaxLengthTestExclude]
		[ResourceStringData("081EE85C-7457-4543-84EF-C6E308AB9D7F", ShortCaption = "EXP Entry Line No.", Caption = "Export Entry Line Number")]
		public ZString FormattedLineNo
		{
			get
			{
				var result = ZString.Empty;
				if (CSI_Code == CancelReasonCodeList.Codes.A)
				{
					result = CSI_LineNo == ZInt.Zero ? ZString.Empty : string.Format("{0:D3}", CSI_LineNo);
				}
				return result;
			}
			set
			{
				CSI_LineNo = ZInt.ParseSafe(value, ZInt.Zero);
				FormattedLineNoInfo.RefreshBinding();
				CSI_LineNoInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo FormattedLineNoInfo => GetZPropertyInfo(nameof(FormattedLineNo));

		bool IsReExportReadOnly
		{
			get
			{
				var result = true;
				if (!IsRefundTypeContractRevocationReadOnly && CSI_Code == CancelReasonCodeList.Codes.A)
				{
					result = false;
				}
				return result;
			}
		}
		bool IsDisposalReadOnly
		{
			get
			{
				var result = true;
				if (!IsRefundTypeContractRevocationReadOnly && CSI_Code == CancelReasonCodeList.Codes.B)
				{
					result = false;
				}
				return result;
			}
		}
		bool IsRefundTypeContractRevocationReadOnly
		{
			get
			{
				var result = true;
				var reconDeclaration = Parent?.Header?.ReconDeclaration;
				if (reconDeclaration != null)
				{
					result = !reconDeclaration.IsRefundTypeContractRevocation;
				}
				return result;
			}
		}
		protected override CusSupportingInfoValidation GetNewValidation() => new ContractRevocation5ULValidation(this);
		public new ContractRevocation5ULValidation Validation => (ContractRevocation5ULValidation)base.Validation;

		public new CusReconEntryLine Parent => (CusReconEntryLine)base.Parent;
		public new ContractRevocation5ULLookups Lookups => (ContractRevocation5ULLookups)base.Lookups;
		protected override CusSupportingInfoLookups GetNewLookups()
		{
			return new ContractRevocation5ULLookups(this);
		}
	}
}
