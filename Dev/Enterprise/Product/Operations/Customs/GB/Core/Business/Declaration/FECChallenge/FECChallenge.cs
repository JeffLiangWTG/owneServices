using System.Data;
using System.Diagnostics;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class FECChallenge : CusCodeData
	{
		public FECChallenge(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.FEC;

			setDefaultValuesStackTrace = new StackTrace().ToString();
		}

		string setDefaultValuesStackTrace;

		public override void OnSaving()
		{
			base.OnSaving();

			if (!IsInDatabase && CY_ParentTableCode.IsEmpty)
			{
				var savigStackTrace = new StackTrace().ToString();
				savigStackTrace += "Parent ID: " + CY_ParentID + "\r\n";
				savigStackTrace += "Code: " + CY_Code + "\r\n";
				savigStackTrace += "IsDeleted: " + IsDeleted;

				ErrorReporter.ReportOnce("SavingEmptyCusCodeData", $"Saving an empty FECChallenge\r\nSaving stack trace:\r\n{savigStackTrace}\r\nSetDefaultValues stack trace:\r\n{setDefaultValuesStackTrace}");
			}
		}

		public ZBool IsParentEntryHeader => Parent != null && CY_ParentTableCode == CusEntryHeaderSchema.Constants.Prefix;

		public ZBool IsParentEntryLine => Parent != null && CY_ParentTableCode == CusEntryLineSchema.Constants.Prefix;

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(CusEntryHeader), typeof(CusEntryLine));

		protected override CusCodeDataLookups GetNewLookups() => new FECChallengeLookups(this);

		public new FECChallengeLookups Lookups => (FECChallengeLookups)base.Lookups;

		protected override CusCodeDataValidation GetNewValidation() => Declaration?.ApplicationExtender?.GetFECChallengeValidation(this) ?? new FECChallengeValidation(this);

		#endregion

		#region New Properties

		#region New Value

		[ReadOnlyMember(nameof(IsNewValueDisabled))]
		[List(nameof(Lookups) + "." + nameof(FECChallengeLookups.NewValueList))]
		public ZDecimal NewValue_Decimal
		{
			get
			{
				return ZDecimal.ParseSafe(NewValue, 0);
			}
			set
			{
				RelatedFieldInfo?.SetValueFromString(value.ToString());
				NewValue_DecimalInfo?.RefreshBinding();
			}
		}
		public ZPropertyInfo NewValue_DecimalInfo => RelatedFieldInfo != null ? GetWrappedZPropertyInfo(nameof(NewValue_Decimal), x => RelatedFieldInfo) : GetZPropertyInfo(nameof(NewValue_Decimal));

		[ReadOnlyMember(nameof(IsNewValueDisabled))]
		[List(nameof(Lookups) + "." + nameof(FECChallengeLookups.NewValueList))]
		public ZString NewValue
		{
			get
			{
				if (IsForDecimalField && IsParentEntryLine && ((CusEntryLine)Parent).InvoiceLines.Count > 1)
				{
					var propertyName = (new FECChallengeFields()).GetDescriptionFromCode(CY_Code);
					return ((CusEntryLine)Parent).InvoiceLines.Sum(x => (ZDecimal)x[propertyName]).ToString(System.Globalization.CultureInfo.InvariantCulture);
				}
				else
				{
					return RelatedFieldInfo?.Value.ToString() ?? ZString.Empty;
				}
			}
			set
			{
				if ((CY_Code == FECChallengeFields.Codes.JI_NettMassUQ || CY_Code == FECChallengeFields.Codes.JI_SuppUQ) &&
					IsParentEntryLine && ((CusEntryLine)Parent).InvoiceLines.Count > 1)
				{
					var propertyName = (new FECChallengeFields()).GetDescriptionFromCode(CY_Code);
					((CusEntryLine)Parent).InvoiceLines.ForEach(x =>
					{
						x.FindPropertyInfo(propertyName).SetValueFromString(value);
					});
				}
				else
				{
					RelatedFieldInfo?.SetValueFromString(value);
				}
				NewValueInfo?.RefreshBinding();
			}
		}

		public ZPropertyInfo NewValueInfo => RelatedFieldInfo != null ? GetWrappedZPropertyInfo(nameof(NewValue), x => RelatedFieldInfo) : GetZPropertyInfo(nameof(NewValue));

		public ZString NewValueFieldType
		{
			get
			{
				ZString type = ZString.Empty;
				switch (CY_Code)
				{
					case FECChallengeFields.Codes.JE_DSP:
					case FECChallengeFields.Codes.JE_FLG:
					case FECChallengeFields.Codes.JE_DST:
					case FECChallengeFields.Codes.JI_ORG:
						type = nameof(FieldType.TextCodeFindBox);
						break;
					case FECChallengeFields.Codes.JI_NettMass:
					case FECChallengeFields.Codes.JI_Price:
					case FECChallengeFields.Codes.JI_Supp:
						type = nameof(FieldType.Decimal);
						break;
					case FECChallengeFields.Codes.JI_NettMassUQ:
					case FECChallengeFields.Codes.JI_SuppUQ:
						type = nameof(FieldType.TextDropEdit);
						break;
				}
				return type;
			}
		}

		public ZBool IsNewValueDisabled
		{
			get
			{
				return CY_IsOverridden || (RelatedFieldInfo?.ReadOnly ?? false)
					|| (IsParentEntryLine && ((CusEntryLine)Parent).InvoiceLines.Count != 1 && IsForDecimalField);
			}
		}

		#endregion

		#region Related Object Name

		public ZString RelatedObjectName
		{
			get
			{
				if (relatedObjectName.IsEmpty && Parent != null)
				{
					relatedObjectName = Parent.HumanReadableName;
					if (IsParentEntryLine)
					{
						relatedObjectName += "[" + ((CusEntryLine)Parent).CL_LineNumber + "]";
					}
				}
				return relatedObjectName;
			}
		}
		ZString relatedObjectName;
		public ZPropertyInfo RelatedObjectNameInfo => GetZPropertyInfo(nameof(RelatedObjectName));

		#endregion

		#region Related Field Info

		ZPropertyInfo RelatedFieldInfo
		{
			get
			{
				if (relatedFieldInfo == null)
				{
					var fieldName = (new FECChallengeFields()).GetDescriptionFromCode(CY_Code);
					if (IsParentEntryHeader)
					{
						relatedFieldInfo = ((CusEntryHeader)Parent).Declaration?.FindPropertyInfo(fieldName);
					}
					else if (IsParentEntryLine)
					{
						var lines = ((CusEntryLine)Parent)?.InvoiceLines;
						if (lines != null && lines.Count > 0)
						{
							relatedFieldInfo = lines[0].FindPropertyInfo(fieldName);
						}
					}
				}
				return relatedFieldInfo;
			}
		}
		ZPropertyInfo relatedFieldInfo;

		#endregion

		#region Related Object Field Name

		public ZString RelatedObjectFieldName
		{
			get
			{
				if (relatedObjectFieldName.IsEmpty && RelatedFieldInfo != null)
				{
					relatedObjectFieldName = ZPropertyInfo.GetFriendlyColumnNameShared(RelatedFieldInfo.Name);
				}
				return relatedObjectFieldName;
			}
		}
		ZString relatedObjectFieldName;
		public ZPropertyInfo RelatedObjectFieldNameInfo => GetZPropertyInfo(nameof(RelatedObjectFieldName));

		#endregion

		public int RelatedFieldMaxLength => (RelatedFieldInfo != null && RelatedFieldInfo.SupportsMaxLength) ? RelatedFieldInfo.MaxLength : -1;

		public JobDeclaration Declaration
		{
			get
			{
				if (IsParentEntryHeader)
				{
					return ((CusEntryHeader)Parent)?.Declaration;
				}
				if (IsParentEntryLine)
				{
					return ((CusEntryLine)Parent)?.Declaration;
				}
				return null;
			}
		}

		#endregion

		#region Override Properties

		public ZDecimal CY_Data_Decimal => ZDecimal.ParseSafe(CY_Data, 0);

		[MaxLength("RelatedFieldMaxLength")]
		[List(nameof(Lookups) + "." + nameof(FECChallengeLookups.NewValueList))]
		public override ZString CY_Data { get => base.CY_Data; set => base.CY_Data = value; }

		[List(nameof(Lookups) + "." + nameof(FECChallengeLookups.CusCodeDataTypes))]
		public override ZString CY_Type { get => base.CY_Type; set => base.CY_Type = value; }

		[ReadOnlyMember(nameof(CY_IsOverridden_Readonly))]
		public override ZBool CY_IsOverridden { get => base.CY_IsOverridden; set => base.CY_IsOverridden = value; }

		ZBool CY_IsOverridden_Readonly => IsFecConfirmTickboxReadonly || IsNewValueInputed;

		public ZBool IsNewValueInputed
		{
			get
			{
				if (IsForDecimalField)
				{
					return NewValue_Decimal != CY_Data_Decimal;
				}
				else
				{
					return NewValue != CY_Data;
				}
			}
		}

		ZBool IsForDecimalField => CY_Code == FECChallengeFields.Codes.JI_NettMass || CY_Code == FECChallengeFields.Codes.JI_Supp || CY_Code == FECChallengeFields.Codes.JI_Price;

		ZBool IsFecConfirmTickboxReadonly
		{
			get
			{
				ZBool result = false;
				if (Parent is CusEntryLine entryLine && entryLine.Header is CusEntryHeader parentEntryHeader)
				{
					result = parentEntryHeader.Declaration != null;
				}
				else if (Parent is CusEntryHeader entryHeader)
				{
					result = entryHeader.Declaration != null;
				}
				return result;
			}
		}

		#endregion
	}
}
