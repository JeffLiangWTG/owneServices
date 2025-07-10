using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.KR.Business
{
	public class NonGADetail : CusSupportingInfo
	{
		public NonGADetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public const int NGA_CodeMaxLength = 1;
			public const int NGA_ProcedureMaxLength = 2;
			public const int NGA_DescriptionMaxLength = 60;
			public const int NonGAReasonTypeMaxLength = 5;
		}

		[ReadOnly(true)]
		[ResourceStringData("33FA7170-6A2C-4BC9-98CD-3F0F2DFC8F1C", Caption = "Seq #")]
		public override ZInt CSI_LineNo
		{
			get => base.CSI_LineNo;
			set => base.CSI_LineNo = value;
		}

		[MaxLength(Schema.NGA_CodeMaxLength)]
		[ResourceStringData("26FDA78A-E5B8-4103-B31B-4F75A570791F", Caption = "Non Req. Type ")]
		[List(nameof(Lookups) + "." + nameof(NonGADetailLookups.CodeList))]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set => base.CSI_Code = value;
		}

		[MaxLength(Schema.NGA_ProcedureMaxLength)]
		[List(nameof(Lookups) + "." + nameof(NonGADetailLookups.OGARegulationCategoryList))]
		[ResourceStringData("A6BCF79F-CAEF-43EF-82F6-1848E644F80D", Caption = "Regulation Category")]
		public override ZString CSI_Procedure
		{
			get => base.CSI_Procedure;
			set => base.CSI_Procedure = value;
		}

		[MaxLength(Schema.NGA_DescriptionMaxLength)]
		[ResourceStringData("0E179A40-BCAB-4847-A8C3-E8EBC18F4CB2", Caption = "Additional Reason For Approval Exemption")]
		public override ZString CSI_Description
		{
			get => base.CSI_Description;
			set => base.CSI_Description = value;
		}

		[MaxLength(Schema.NonGAReasonTypeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(NonGADetailLookups.NonGAReasonTypeList))]
		[ReadOnlyMember(nameof(NonGAReasonType_ReadOnly))]
		[BusinessObjectTestExclude]
		[ResourceStringData("7453346C-0158-4C09-B071-80B7EC1ACCD1", Caption = "Non-GA Reason Type")]
		public ZString NonGAReasonType
		{
			get
			{
				var result = ZString.Empty;
				if (!(CSI_Procedure.IsEmpty || CSI_Code.IsEmpty || CSI_Status.IsEmpty))
				{
					result = CSI_Procedure + CSI_Code + CSI_Status;
				}
				return result;
			}
			set
			{
				CheckMaximumLength(NonGAReasonTypeInfo, value);

				CSI_Status = value.Length == Schema.NonGAReasonTypeMaxLength ? value.Right(2) : ZString.Empty;

				NonGAReasonTypeInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo NonGAReasonTypeInfo => GetZPropertyInfo(nameof(NonGAReasonType));
		bool NonGAReasonType_ReadOnly => CSI_Procedure.IsEmpty || CSI_Code.IsEmpty;

		public ZZRefCusCodeListCombined ImportNonGAReasonCode
		{
			get
			{
				return MessageFunctions.GetRefCusCodeList(Factory, NonGAReasonType, Messaging.Constants.ZZ.NKCodeType.INGAR);
			}
		}

		[ResourceStringData("30B25F75-E1BE-4729-A029-DD2A9D771A86", Caption = "Non-GA Mandatory Doc.")]
		[MaxLength(255)]
		public ZString ImportNonGAMandatoryDocument => ImportNonGAReasonCode?.GetAttribute(Messaging.Constants.ZZ.CodeListAttributeNames.MandatoryDocWhenExemptCode) ?? ZString.Empty;

		protected override CusSupportingInfoValidation GetNewValidation() => new NonGADetailValidation(this);
		public new NonGADetailValidation Validation => (NonGADetailValidation)base.Validation;
		public new ILineOrProduct Parent => base.Parent as ILineOrProduct;
		protected override bool IsValidationEnabledCore(ZPropertyInfo propertyInfo)
		{
			return Parent?.IsValidationEnabled ?? true;
		}

		public new NonGADetailLookups Lookups => (NonGADetailLookups)base.Lookups;
		protected override CusSupportingInfoLookups GetNewLookups()
		{
			return new NonGADetailLookups(this);
		}

		public IEnumerable<IZType> KeyFields => new List<IZType>() { CSI_Procedure, CSI_Code, CSI_Status };

		public bool HasSameKey(NonGADetail other)
		{
			return KeyFields.SequenceEqual(other.KeyFields);
		}
	}
}
