using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.MX;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.Universal.Constants.ProfileQuestion;

namespace Enterprise.Customs.MX.Business
{
	public class Identifier : CusSupportingInfo
	{
		public new class Schema : CusSupportingInfo.Schema
		{
			public const string Applicability = "Applicability";
			public const string FillGuidance1 = "FillGuidance1";
			public const string FillGuidance2 = "FillGuidance2";
			public const string FillGuidance3 = "FillGuidance3";
		}

		public Identifier(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool SupportsNotes => false;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.Identifier;
			CSI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
		}

		[MaxLength(2)]
		[ResourceStringData("Enterprise.Customs.MX.Business.Identifier|CSI_Code", Caption = "Code")]
		[List(nameof(Lookups) + "." + nameof(IdentifierLookups.CodeList))]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set
			{
				var oldValue = base.CSI_Code;
				base.CSI_Code = value;

				if (oldValue != CSI_Code)
				{
					CSI_ReferenceNumber = ZString.Empty;
					CSI_ReferenceNumber2 = ZString.Empty;
					CSI_Description = ZString.Empty;
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.MX.Business.Identifier|CSI_ReferenceNumber", Caption = "Complement 1")]
		[List(nameof(Lookups) + "." + nameof(IdentifierLookups.Complement1List))]
		[ReadOnlyMember(nameof(CSI_ReferenceNumber_ReadOnly))]
		public override ZString CSI_ReferenceNumber { get => GetComplement(base.CSI_ReferenceNumber, Complement1DataType); set => base.CSI_ReferenceNumber = GetValueForSettingComplement(value, Complement1DataType, Complement1DecimalPlaces); }

		[ResourceStringData("Enterprise.Customs.MX.Business.Identifier|CSI_ReferenceNumber2", Caption = "Complement 2")]
		[List(nameof(Lookups) + "." + nameof(IdentifierLookups.Complement2List))]
		[ReadOnlyMember(nameof(CSI_ReferenceNumber2_ReadOnly))]
		public override ZString CSI_ReferenceNumber2 { get => GetComplement(base.CSI_ReferenceNumber2, Complement2DataType); set => base.CSI_ReferenceNumber2 = GetValueForSettingComplement(value, Complement2DataType, Complement2DecimalPlaces); }

		[ResourceStringData("Enterprise.Customs.MX.Business.Identifier|CSI_Description", Caption = "Complement 3")]
		[List(nameof(Lookups) + "." + nameof(IdentifierLookups.Complement3List))]
		[ReadOnlyMember(nameof(CSI_Description_ReadOnly))]
		public override ZString CSI_Description { get => GetComplement(base.CSI_Description, Complement3DataType); set => base.CSI_Description = GetValueForSettingComplement(value, Complement3DataType, Complement3DecimalPlaces); }

		[ResourceStringData("Enterprise.Customs.MX.Business.Identifier|Applicability", Caption = "Applicability")]
		public ZString Applicability => Complement1Question?.XQ2_Text ?? ZString.Empty;

		[ResourceStringData("Enterprise.Customs.MX.Business.Identifier|FillGuidance1", Caption = "Fill Guidance", FullDescription = "The Complement 1 Fill Guidance.")]
		public ZString FillGuidance1 => Complement1Question?.XQ2_Note ?? ZString.Empty;

		[ResourceStringData("Enterprise.Customs.MX.Business.Identifier|FillGuidance2", Caption = "Fill Guidance", FullDescription = "The Complement 2 Fill Guidance.")]
		public ZString FillGuidance2 => Complement2Question?.XQ2_Note ?? ZString.Empty;

		[ResourceStringData("Enterprise.Customs.MX.Business.Identifier|FillGuidance3", Caption = "Fill Guidance", FullDescription = "The Complement 3 Fill Guidance.")]
		public ZString FillGuidance3 => Complement3Question?.XQ2_Note ?? ZString.Empty;

		public new IdentifierLookups Lookups => (IdentifierLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups() => new IdentifierLookups(this);

		protected override CusSupportingInfoValidation GetNewValidation() => new IdentifierValidation(this);

		public IEnumerable<RefCusProfileQuestion> Questions => MXRefCusProfile.GetProfileQuestions(Factory, CSI_Code, (Parent as JobComInvoiceLine)?.EffectiveAssessmentDate ?? ZDateTime.Today);

		public RefCusProfileQuestion Complement1Question => Questions.FindByQuestionCode(Constants.Profile.Types.CO1);

		public RefCusProfileQuestion Complement2Question => Questions.FindByQuestionCode(Constants.Profile.Types.CO2);

		public RefCusProfileQuestion Complement3Question => Questions.FindByQuestionCode(Constants.Profile.Types.CO3);

		ZString GetComplement(ZString content, ZString complementDataType)
		{
			if (complementDataType == AnswerDataTypes.Number && !content.IsEmpty)
			{
				content = (ZDecimal.TryParse(content, out var decimalValue) ? decimalValue : 0).ToString("G", Culture.CurrentCompanyCountryCulture);
			}
			return content;
		}

		ZString GetValueForSettingComplement(ZString value, ZString complementDataType, ZInt decimalMaxLength)
		{
			if (complementDataType == AnswerDataTypes.Number)
			{
				value = ZDecimal.TryParse(value, Culture.CurrentCompanyCountryCulture, out var decimalValue) ? decimalValue.Truncate(decimalMaxLength).ToString() : string.Empty;
			}
			return value;
		}

		ZBool CSI_ReferenceNumber_ReadOnly => Complement1Question == null;

		ZBool CSI_ReferenceNumber2_ReadOnly => Complement2Question == null;

		ZBool CSI_Description_ReadOnly => Complement3Question == null;

		public ZInt Complement1DecimalPlaces => Complement1Question?.XQ2_AnswerDecimalPlaces ?? ZInt.Zero;

		public ZInt Complement1MaxLength => Complement1Question?.XQ2_AnswerMaxLength ?? ZInt.Zero;

		public ZInt Complement2DecimalPlaces => Complement2Question?.XQ2_AnswerDecimalPlaces ?? ZInt.Zero;

		public ZInt Complement2MaxLength => Complement2Question?.XQ2_AnswerMaxLength ?? ZInt.Zero;

		public ZInt Complement3DecimalPlaces => Complement3Question?.XQ2_AnswerDecimalPlaces ?? ZInt.Zero;

		public ZInt Complement3MaxLength => Complement3Question?.XQ2_AnswerMaxLength ?? ZInt.Zero;

		public ZString Complement1FieldType => EvaluateDataType(Complement1DataType);

		ZString Complement1DataType => Complement1Question?.XQ2_AnswerDataType ?? ZString.Empty;

		public ZString Complement2FieldType => EvaluateDataType(Complement2DataType);

		ZString Complement2DataType => Complement2Question?.XQ2_AnswerDataType ?? ZString.Empty;

		public ZString Complement3FieldType => EvaluateDataType(Complement3DataType);

		ZString Complement3DataType => Complement3Question?.XQ2_AnswerDataType ?? ZString.Empty;

		ZString EvaluateDataType(ZString answerDataType)
		{
			return (string)answerDataType switch
			{
				AnswerDataTypes.List => nameof(FieldType.TextDropEdit),
				_ => nameof(FieldType.Text),
			};
		}
	}
}
