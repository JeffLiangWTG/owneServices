using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BarcodeParsingEngine;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BarcodeParsing.Business
{
	public class BarcodeValidationRule : AutoBarcodeValidationRule, IBarcodeRuleWithLength, IBarcodeValidationRule
	{
		public BarcodeValidationRule(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Schema

		public new abstract class Schema : AutoBarcodeValidationRule.Schema
		{
			public const string LengthTypeForBinding = "LengthTypeForBinding";
		}

		#endregion

		#region RuleSet

		public BarcodeRuleSet RuleSet => Factory.Load<BarcodeRuleSet>(BVR_BRS_RuleSet);

		#endregion

		#region BVR_BRS_RuleSet

		[RelatedBusinessObject("RuleSet")]
		public override ZGuid BVR_BRS_RuleSet
		{
			get { return base.BVR_BRS_RuleSet; }
			set { base.BVR_BRS_RuleSet = value; }
		}

		#endregion

		[ReadOnlyMember(nameof(IsDateFormat))]
		public override ZString BVR_Prefix
		{
			get => base.BVR_Prefix;
			set => base.BVR_Prefix = value;
		}

		#region LengthTypeForBinding

		[MaxLength(1)]
		[BusinessObjectMaxLengthTestExclude] // Test will fail as setter takes code, getter returns description
		[List("Lookups.LengthTypes")]
		[ReadOnlyMember(nameof(IsDateFormat))]
		[ResourceStringData("BarcodeValidationRule|LengthTypeForBinding", Caption = "Length Type", ShortCaption = "Len. Type")]
		public ZString LengthTypeForBinding
		{
			get { return Lookups.LengthTypes.GetDescriptionFromCode(LengthType); }
			set
			{
				if (Lookups.LengthTypes.ContainsCode(value))
				{
					bool valueHasChanged = SetNonPersistentPropertyValue(LengthTypeForBindingInfo, ref lengthType, value);
					if (valueHasChanged)
					{
						this.SetMinAndMaxLength();
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateBVR_MinLength();
						Validation.ValidateBVR_MaxLength();
					}
				}
				else
				{
					LengthTypeForBindingInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo LengthTypeForBindingInfo => GetZPropertyInfo(Schema.LengthTypeForBinding);

		#region LengthType

		public ZString LengthType => lengthType ?? this.CalculateLengthType(IsInDatabase);

		ZString? lengthType;

		#endregion

		#endregion

		#region BVR_MinLength

		[ReadOnlyMember(nameof(IsLengthReadOnly))]
		public override ZShort BVR_MinLength
		{
			get { return base.BVR_MinLength; }
			set
			{
				var previousValue = BVR_MinLength;
				base.BVR_MinLength = value;

				if (previousValue != BVR_MinLength)
				{
					this.SetLengthForLengthTypeFixed(value);
					Validation.ValidateBVR_MaxLength();
				}
			}
		}

		#endregion

		#region BVR_MaxLength

		[ReadOnlyMember(nameof(IsLengthReadOnly))]
		public override ZShort BVR_MaxLength
		{
			get { return base.BVR_MaxLength; }
			set
			{
				var previousValue = BVR_MaxLength;
				base.BVR_MaxLength = value;

				if (previousValue != BVR_MaxLength)
				{
					this.SetLengthForLengthTypeFixed(value);
					Validation.ValidateBVR_MinLength();
				}
			}
		}

		#endregion

		#region BVR_Format

		[List("Lookups.FormatTypes")]
		public override ZString BVR_Format
		{
			get { return base.BVR_Format; }
			set
			{
				base.BVR_Format = value;
				if (IsDateFormat)
				{
					LengthTypeForBinding = LengthTypes.Codes.Fixed;
					BVR_Prefix = string.Empty;
					BVR_MinLength = LengthOfDateFormat;
				}
			}
		}

		public bool IsDateFormat => BarcodeFormatHelper.IsDateFormat(BVR_Format);

		ZShort LengthOfDateFormat => BarcodeFormatHelper.LengthOfDateFormat(BVR_Format);

		public FormatType Format => BarcodeFormatHelper.ParseFormat(BVR_Format);

		#endregion

		#region BVR_TargetField

		[List("Lookups.TargetFields")]
		public override ZString BVR_TargetField => base.BVR_TargetField;

		#endregion

		#region TargetFieldDescription

		[ResourceStringData("BarcodeValidationRule|TargetFieldDescription", Caption = "Target Field Description", ShortCaption = "Description")]
		public ZString TargetFieldDescription => Lookups.TargetFields.GetDescriptionFromCode(BVR_TargetField);

		#endregion

		#region Module

		public ZString Module => RuleSet?.BRS_Module ?? ZString.Empty;

		#endregion

		#region IsLengthReadOnly

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Added this Code after failure in DAT tests through BulkAnalyzer script(WI00610195)")]
		bool IsLengthReadOnly => IsDateFormat || this.IsLengthTypeAny();

		#endregion

		#region ReadOnly

		public override bool ReadOnly
		{
			get { return base.ReadOnly || IsRuleSetReadOnly; }
			set { base.ReadOnly = value; }
		}

		bool IsRuleSetReadOnly => RuleSet != null && RuleSet.ReadOnly;

		#endregion

		#region LoadMatchingRules

		public static IEnumerable<BarcodeValidationRule> LoadMatchingRules(BusinessObjectFactory factory, ZString moduleCode, ZString buyerCode, ZString supplierCode, ZGuid relatedEntityPK)
		{
			return LoadMatchingRulesHelper.LoadMatchingRules<BarcodeValidationRule>(BuildLoadMatchingRulesQuery, factory, moduleCode, buyerCode, supplierCode, relatedEntityPK, isGS1: false);
		}

		public static IEnumerable<BarcodeValidationRule> LoadMatchingRules(BusinessObjectFactory factory, ZString moduleCode, ZGuid buyerPK, ZGuid supplierPK, ZGuid relatedEntityPK)
		{
			return LoadMatchingRulesHelper.LoadMatchingRules<BarcodeValidationRule>(BuildLoadMatchingRulesQuery, factory, moduleCode, buyerPK, supplierPK, relatedEntityPK, isGS1: false);
		}

		static ZDBOnlyQuery BuildLoadMatchingRulesQuery(ZDBOnlySubQuery subQuery, LoadMatchingRulesParameters parameters)
		{
			var ruleQuery = new ZDBOnlyQuery(typeof(BarcodeValidationRule));
			ruleQuery.AddSubQuery(BarcodeValidationRuleSchema.BVR_BRS_RuleSet, subQuery, JoinCondition.And);

			return ruleQuery;
		}

		#endregion

		#region Cloning support

		protected override bool SupportsCloneCore() => true;

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clone = (BarcodeValidationRule)base.CloneInternal(args);
			clone.LengthTypeForBinding = LengthType;

			return clone;
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning() => new[] { BarcodeValidationRuleSchema.Constants.BVR_BRS_RuleSet };

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore => Res.GetString("4a8856d8-1fec-4eb6-9745-c7786c60cbfd", "Barcode Validation Rule");

		#endregion

		#region IBarcodeRuleWithLength

		ZShort IBarcodeRuleWithLength.MinLength
		{
			get => BVR_MinLength;
			set => BVR_MinLength = value;
		}

		ZShort IBarcodeRuleWithLength.MaxLength
		{
			get => BVR_MaxLength;
			set => BVR_MaxLength = value;
		}

		ZPropertyInfo IBarcodeRuleWithLength.MinLengthInfo
		{
			get => BVR_MinLengthInfo;
		}

		ZPropertyInfo IBarcodeRuleWithLength.MaxLengthInfo
		{
			get => BVR_MaxLengthInfo;
		}

		#endregion

		#region IBarcodeValidaionRule

		public string Prefix => BVR_Prefix;

		public short MinLength => BVR_MinLength;

		public short MaxLength => BVR_MaxLength;

		public string TargetField => BVR_TargetField;

		#endregion
	}
}
