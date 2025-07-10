using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Customs.Universal.Constants.ProfileQuestion;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class AttributeCusCodeData : CusCodeData
	{
		public new class Schema : CusCodeData.Schema
		{
			public const string Example = "Example";
			public const string ConditionDescription = "ConditionDescription";
			public const string Content = "Content";
			public const string FillOrientation = "FillOrientation";
			public const string IsMandatory = "IsMandatory";
			public const string Label = "Label";
			public const string LegalBase = "LegalBase";
			public const string ParentAttributeCode = "ParentAttributeCode";
			public const string TaxType = "TaxType";
			public const string StartDate = "StartDate";
			public const string EndDate = "EndDate";
			public const int AttributeCodeMaxLength = 20;
		}

		public AttributeCusCodeData(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool SupportsNotes => false;

		protected override TypeLoaderCollection parentLoaders => new(typeof(JobComInvoiceLine), typeof(CusClassPartPivot), typeof(CusGoodsCatalog));

		#region Type Safe

		protected override CusCodeDataValidation GetNewValidation() => new AttributeCusCodeDataValidation(this);

		public new AttributeCusCodeDataValidation Validation => (AttributeCusCodeDataValidation)base.Validation;

		public new AttributeCusCodeDataLookups Lookups => (AttributeCusCodeDataLookups)base.Lookups;

		protected override CusCodeDataLookups GetNewLookups() => new AttributeCusCodeDataLookups(this);

		#endregion

		#region CusCodeData Properties

		[MaxLength(Schema.AttributeCodeMaxLength)]
		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.BR.Business.AttributeCusCodeData|Code", Caption = "Code")]
		public override ZString CY_Code { get => base.CY_Code; set => base.CY_Code = value; }

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(Content_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.AttributeCusCodeData|Content", Caption = "Content")]
		[List(nameof(Lookups) + "." + nameof(AttributeCusCodeDataLookups.PossibleValues))]
		public ZString Content
		{
			get
			{
				var content = CY_Data;

				if (!content.IsEmpty)
				{
					if (AnswerDataType == AnswerDataTypes.Boolean)
					{
						content = Lookups.PossibleValues.GetDescriptionFromCode(content.ConvertBoolToYesNo());
					}
					else if (AnswerDataType == AnswerDataTypes.Number)
					{
						content = (ZDecimal.TryParse(content, out var decimalValue) ? decimalValue : 0).ToString("G", Culture.CurrentCompanyCountryCulture);
					}
					else if (AnswerDataType == AnswerDataTypes.Date)
					{
						content = ZDateTime.TryParseExact(content, out var date, Constants.DataFormat) ? date.ToString(DateTimeFormatStrings.ShortDateFormat).ToUpper() : string.Empty;
					}
					else if (AllowMultipleAnswersForString)
					{
						content = string.Join(System.Environment.NewLine, Answers);
					}
					else if (AnswerDataType == AnswerDataTypes.List && !content.IsEmpty && !content.Contains(CodesSeparator) && HasPossibleValues)
					{
						content = $"{content}{CodeDescriptionSeparator}{Lookups.PossibleValues.GetDescriptionFromCode(content)}";
					}
				}
				return content;
			}
			set
			{
				var oldValue = CY_Data;

				if (!value.TrimEndSpaceTab().IsEmpty)
				{
					if (AnswerDataType == AnswerDataTypes.Boolean)
					{
						value = value.ConvertYesNoToBool();
					}
					else if (AnswerDataType == AnswerDataTypes.Number)
					{
						value = ZDecimal.TryParse(value, Culture.CurrentCompanyCountryCulture, out var decimalValue) ? decimalValue.Truncate(CY_DataDecimalPlaces).ToString() : string.Empty;
					}
					else if (AnswerDataType == AnswerDataTypes.Date)
					{
						value = (ZDateTime.TryParseExact(value, out var dateValue, DateTimeFormatStrings.ShortDateFormat) ? dateValue : ZDateTime.Empty).ToString(Constants.DataFormat, CultureInfo.InvariantCulture);
					}
					else if (AllowMultipleAnswersForString)
					{
						var lines = value.Trim().Split(System.Environment.NewLine).Where(x => !x.IsEmpty).ToArray();
						value = lines.FirstOrDefault();
						MultivaluedAttributesLinked.UpdateAll(lines.Skip(1));
					}
					else if (AnswerDataType == AnswerDataTypes.List && value.Contains(CodeDescriptionSeparator) && HasPossibleValues)
					{
						value = value.Split(CodeDescriptionSeparator).FirstOrDefault();
					}
					CY_Data = value;
				}
				else
				{
					CY_Data = ZString.Empty;
					if (AllowMultipleAnswersForString)
					{
						MultivaluedAttributesLinked.RemoveAndDeleteAll();
					}
				}

				if (!IsCopying && oldValue != CY_Data)
				{
					var childAttributes = ChildAttributes?.ToArray();
					if (childAttributes != null)
					{
						foreach (var child in childAttributes)
						{
							if (child.Content_ReadOnly)
							{
								child.Content = ZString.Empty;
							}
							child.ContentInfo.RefreshBinding();
						}
					}
					ContentInfo.RefreshBinding();
				}
			}
		}
		public ZPropertyInfo ContentInfo => GetWrappedZPropertyInfo(Schema.Content, x => CY_DataInfo);

		public IReadOnlyList<ZString> Answers
		{
			get
			{
				if (AllowMultipleAnswersForList)
				{
					return CY_Data.Split(CodesSeparator);
				}
				else if (AllowMultipleAnswersForString)
				{
					return MultivaluedAttributesLinked.Cast<AttributeCusCodeData>().Append(this).OrderBy(x => x.CY_Order).Select(x => x.CY_Data).ToArray();
				}
				else
				{
					return new[] { CY_Data };
				}
			}
		}

		const string CodesSeparator = ",";

		const string CodeDescriptionSeparator = " - ";

		bool Content_ReadOnly => Factory.GetCached(ref cachedContentReadOnly, () => IsCompoundAttribute || !ParentAttributeMeetConditionToProceed());
		CachedProperty<bool> cachedContentReadOnly;

		bool ParentAttributeMeetConditionToProceed()
		{
			var formula = ConditionToProceedFormula;
			return formula.IsEmpty || (ParentAttribute?.MeetConditionToProceed(formula) ?? false);
		}

		bool MeetConditionToProceed(string formula) => Answers.Any(answer => !answer.IsEmpty && new ConditionCalculator(formula, new AttributeConditionCalcData(AnswerDataType, answer)).Evaluate());

		[ResourceStringData("Enterprise.Customs.BR.Business.AttributeCusCodeData|CY_Data", Caption = "Content")]
		public override ZString CY_Data { get => base.CY_Data; set => base.CY_Data = value; }

		public ZString CY_DataFieldType
		{
			get
			{
				if (AllowMultipleAnswersForList)
				{
					return nameof(FieldType.TextCodeFindBox);
				}
				else if (AllowMultipleAnswersForString)
				{
					return nameof(FieldType.TextMultiLine);
				}
				else
				{
					return (string)AnswerDataType switch
					{
						AnswerDataTypes.Boolean => nameof(FieldType.TextDropEdit),
						AnswerDataTypes.List => nameof(FieldType.TextDropEdit),
						AnswerDataTypes.Date => nameof(FieldType.Date),
						_ => nameof(FieldType.Text),
					};
				}
			}
		}

		public bool AllowMultipleAnswersForList => AnswerDataType == AnswerDataTypes.List && AllowMultipleAnswers && HasPossibleValues;

		public bool AllowMultipleAnswersForString => AnswerDataType == AnswerDataTypes.String && AllowMultipleAnswers;

		bool HasPossibleValues => Lookups.PossibleValues.Count > 0;

		#endregion

		#region TariffProfileQuestion Properties

		public TariffProfileQuestion TariffProfileQuestion { get; set; }

		[ResourceStringData("Enterprise.Customs.BR.Business.AttributeCusCodeData|Description", Caption = "Description")]
		public ZString Label => TariffProfileQuestion?.Name ?? ZString.Empty;

		[ResourceStringData("Enterprise.Customs.BR.Business.AttributeCusCodeData|IsMandatory", Caption = "Is Mandatory?")]
		public ZBool IsMandatory => TariffProfileQuestion?.IsAnswerMandatory ?? false;

		[ResourceStringData("Enterprise.Customs.BR.Business.AttributeCusCodeData|FillOrientation", Caption = "Fill Guidance")]
		public ZString FillOrientation => TariffProfileQuestion?.Note ?? ZString.Empty;

		[ResourceStringData("Enterprise.Customs.BR.Business.AttributeCusCodeData|Example", Caption = "Example")]
		public ZString Example => TariffProfileQuestion?.Example ?? ZString.Empty;

		public ZInt MaxSize => TariffProfileQuestion?.AnswerMaxLength ?? 0;

		ZString AnswerDataType => TariffProfileQuestion?.AnswerDataType ?? ZString.Empty;

		bool AllowMultipleAnswers => TariffProfileQuestion?.AllowMultipleAnswers ?? false;

		public bool IsCompoundAttribute => AnswerDataType == AnswerDataTypes.Compound;

		public ZShort CY_DataDecimalPlaces => TariffProfileQuestion?.AnswerDecimalPlaces ?? ZShort.Zero;

		[ResourceStringData("Enterprise.Customs.BR.Business.AttributeCusCodeData|StartDate", Caption = "Start Date")]
		public ZDateTime StartDate => TariffProfileQuestion?.StartDate ?? ZDateTime.Empty;

		[ResourceStringData("Enterprise.Customs.BR.Business.AttributeCusCodeData|EndDate", Caption = "End Date")]
		public ZDateTime EndDate => TariffProfileQuestion?.EndDate ?? ZDateTime.Empty;

		public bool IsEffectiveInFuture => !StartDate.IsEmpty && StartDate.IsInTheFutureDatePartOnly;

		#endregion

		#region TariffProfile Properties

		[ResourceStringData("Enterprise.Customs.BR.Business.AttributeCusCodeData|TaxType", Caption = "Tax Type")]
		public ZString TaxType
		{
			get => taxType;
			private set
			{
				SetNonPersistentPropertyValue(TaxTypeInfo, ref taxType, value);
				TaxTypeInfo.RefreshBinding();
			}
		}
		ZString taxType;

		public ZPropertyInfo TaxTypeInfo => GetZPropertyInfo(Schema.TaxType);

		[ResourceStringData("Enterprise.Customs.BR.Business.AttributeCusCodeData|LegalBase", Caption = "Legal Basis")]
		public ZString LegalBase
		{
			get => legalBase;
			private set
			{
				SetNonPersistentPropertyValue(LegalBaseInfo, ref legalBase, value);
				LegalBaseInfo.RefreshBinding();
			}
		}
		ZString legalBase;

		public ZPropertyInfo LegalBaseInfo => GetZPropertyInfo(Schema.LegalBase);

		internal void SetTaxTypeAndLegalBase(IEnumerable<TariffProfile> profiles)
		{
			string JoinProfile(Func<TariffProfile, string> selector) => profiles != null ? string.Join(", ", profiles.Select(selector)) : string.Empty;

			TaxType = ParentAttribute != null ? ParentAttribute.TaxType : JoinProfile(p => p.TaxType);
			LegalBase = ParentAttribute != null ? ParentAttribute.LegalBase : JoinProfile(p => p.LegalCode);
		}

		#endregion

		#region Question Pathway Properties

		public RefCusProfileQuestionPathway QuestionPathway
		{
			get => questionPathway;
			set
			{
				questionPathway = value;
				QuestionParent = questionPathway?.QuestionParent;
			}
		}
		RefCusProfileQuestionPathway questionPathway;

		RefCusProfileQuestion QuestionParent { get; set; }

		AttributeCusCodeData ParentAttribute => Parent is IAttributeCusCodeDataParent parent && QuestionParent != null ? parent.GetAttributes(CY_Type).GetFirstElementHaving(ParentAttributeCode) : null;

		[ResourceStringData("Enterprise.Customs.BR.Business.AttributeCusCodeData|ParentAttributeCode", Caption = "Parent Attribute")]
		public ZString ParentAttributeCode => QuestionParent?.XQ2_Code ?? ZString.Empty;

		[ResourceStringData("Enterprise.Customs.BR.Business.AttributeCusCodeData|ConditionDescription", Caption = "Condition")]
		public ZString ConditionDescription => QuestionPathway?.XQP_Description ?? ZString.Empty;

		ZString ConditionToProceedFormula => QuestionPathway?.XQP_ConditionToProceedFormula ?? ZString.Empty;

		public bool ParentIsCompoundAttribute => ParentAttribute?.IsCompoundAttribute ?? false;

		public IEnumerable<AttributeCusCodeData> ChildAttributes => (Parent as IAttributeCusCodeDataParent)?.GetAttributes(CY_Type).Where(x => x.ParentAttributeCode == CY_Code);

		#endregion

		[ChildEditable]
		public MultivaluedAttributesCusCodeDataCollection MultivaluedAttributesLinked
		{
			get
			{
				if (fMultivaluedAttributesLinked == null)
				{
					fMultivaluedAttributesLinked = new MultivaluedAttributesCusCodeDataCollection(this);
					fMultivaluedAttributesLinked.Load();
					RegisterEditableChildObject(fMultivaluedAttributesLinked);
				}
				return fMultivaluedAttributesLinked;
			}
		}
		MultivaluedAttributesCusCodeDataCollection fMultivaluedAttributesLinked;

		public void CopyValuesIfEntered(AttributeCusCodeData attribute)
		{
			if (!attribute.Content.IsEmpty)
			{
				Content = attribute.Content;
			}
		}
	}
}
