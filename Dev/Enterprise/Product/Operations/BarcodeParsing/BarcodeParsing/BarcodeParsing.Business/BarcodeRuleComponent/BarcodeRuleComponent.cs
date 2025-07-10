using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BarcodeParsingEngine;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BarcodeParsing.Business
{
	public class BarcodeRuleComponent : AutoBarcodeRuleComponent, IBarcodeRuleComponent, IBarcodeRuleWithLength
	{
		public BarcodeRuleComponent(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new abstract class Schema : AutoBarcodeRuleComponent.Schema
		{
			public const string DelimiterForBinding = "DelimiterForBinding";
			public const string HasDelimiter = "HasDelimiter";
			public const string IsDelimiterMultiComponent = "IsDelimiterMultiComponent";
			public const string LengthTypeForBinding = "LengthTypeForBinding";
		}

		#endregion

		#region Related Entities

		#region Rule

		public BarcodeRule Rule
		{
			get { return Factory.Load<BarcodeRule>(BRC_BRU_Rule); }
		}

		#endregion

		public void RefreshSampleBarcodeInfo()
		{
			if (Rule != null)
			{
				Rule.SampleBarcodeInfo.RefreshBinding();
			}
		}

		#endregion

		#region Properties

		// persistent

		#region BRC_Sequence

		[ReadOnlyMember(nameof(IsPartialRule))]
		public override ZShort BRC_Sequence
		{
			get { return base.BRC_Sequence; }
			set { base.BRC_Sequence = value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Added this Code after failure in DAT tests through BulkAnalyzer script(WI00610195)")]
		bool IsPartialRule
		{
			get
			{
				var rule = Rule;
				return rule != null && rule.IsPartialRule;
			}
		}

		#endregion

		#region BRC_ApplicationID

		[List("Lookups.ApplicationIdentifiers")]
		public override ZString BRC_ApplicationID
		{
			get { return base.BRC_ApplicationID; }
			set
			{
				base.BRC_ApplicationID = value;

				if (IsGS1)
				{
					TryToAutoFillPropertiesForAppIdFromRegistry();
				}

				RefreshSampleBarcodeInfo();
			}
		}

		void TryToAutoFillPropertiesForAppIdFromRegistry()
		{
			var appIdFromRegistry = WarehouseDataRegistry.Instance.ApplicationIdentifiers.Value[BRC_ApplicationID] ?? WarehouseDataRegistry.Instance.ApplicationIdentifiers.Value[BRC_ApplicationID.ToLower()];
			if (appIdFromRegistry != null)
			{
				// if min length is zero and max length is greater than zero, then the minimum must be 1.
				var minLength = appIdFromRegistry.MinFieldLength == 0 && appIdFromRegistry.MaxFieldLength > 0
					? (ZShort)1
					: (ZShort)appIdFromRegistry.MinFieldLength;

				var maxLength = (ZShort)appIdFromRegistry.MaxFieldLength;
				lengthType = BarcodeFormatHelper.GetLengthTypeBasedOnMinAndMaxLength(minLength, maxLength);
				BRC_MinLength = minLength;
				BRC_MaxLength = maxLength;
				BRC_Format = appIdFromRegistry.DataType;

				if (GS1DateAppIds.Contains(BRC_ApplicationID))
				{
					// set to GS1 date format if is not overriden
					if (BRC_MinLength == 6 && BRC_MaxLength == 6 && BRC_Format == GS1DataFormatTypes.Codes.Digit)
					{
						BRC_Format = GS1DataFormatTypes.Codes.YYMMDD;
					}
				}

				// validate min & max length to clear errors
				Validation.ValidateBRC_MinLength();
				Validation.ValidateBRC_MaxLength();
			}
		}

		readonly ZString[] GS1DateAppIds = { "11", "12", "13", "15", "17" };

		#endregion

		#region BRC_BRU_Rule

		[RelatedBusinessObject("Rule")]
		public override ZGuid BRC_BRU_Rule
		{
			get { return base.BRC_BRU_Rule; }
			set { base.BRC_BRU_Rule = value; }
		}

		#endregion

		#region BRC_Delimiter

		public override ZByte BRC_Delimiter
		{
			get { return base.BRC_Delimiter; }
			set
			{
				base.BRC_Delimiter = value;
				DelimiterForBindingInfo.RefreshBinding();
				RefreshSampleBarcodeInfo();
			}
		}

		#endregion

		#region BRC_Format

		[List("Lookups.FormatTypes")]
		public override ZString BRC_Format
		{
			get { return base.BRC_Format; }
			set
			{
				base.BRC_Format = value;

				// Set Fixed Length for Date Formats
				if (IsDateFormat)
				{
					LengthTypeForBinding = LengthTypes.Codes.Fixed;
					BRC_MinLength = LengthOfDateFormat;
				}

				RefreshSampleBarcodeInfo();
			}
		}

		public bool IsDateFormat => BarcodeFormatHelper.IsDateFormat(BRC_Format);

		ZShort LengthOfDateFormat => BarcodeFormatHelper.LengthOfDateFormat(BRC_Format);

		#endregion

		#region Lengths

		#region BRC_MaxLength

		[ReadOnlyMember(nameof(IsLengthReadOnly))]
		public override ZShort BRC_MaxLength
		{
			get { return base.BRC_MaxLength; }
			set
			{
				var previousValue = BRC_MaxLength;
				base.BRC_MaxLength = value;

				if (previousValue != BRC_MaxLength)
				{
					this.SetLengthForLengthTypeFixed(value);
					Validation.ValidateBRC_MinLength(); // tested in BarcodeRuleComponentValidationTest
				}

				RefreshSampleBarcodeInfo();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Added this Code after failure in DAT tests through BulkAnalyzer script(WI00610195)")]
		bool IsLengthReadOnly
		{
			get { return IsDateFormat || this.IsLengthTypeAny(); }
		}

		#endregion

		#region BRC_MinLength

		[ReadOnlyMember(nameof(IsLengthReadOnly))]
		public override ZShort BRC_MinLength
		{
			get { return base.BRC_MinLength; }
			set
			{
				var previousValue = BRC_MinLength;
				base.BRC_MinLength = value;

				if (previousValue != BRC_MinLength)
				{
					this.SetLengthForLengthTypeFixed(value);
					Validation.ValidateBRC_MaxLength(); // tested in BarcodeRuleComponentValidationTest
				}

				RefreshSampleBarcodeInfo();
			}
		}

		#endregion

		#endregion

		#region BRC_TargetField

		[List("Lookups.TargetFields")]
		public override ZString BRC_TargetField
		{
			get { return base.BRC_TargetField; }
			set
			{
				base.BRC_TargetField = value;
				RefreshSampleBarcodeInfo();
			}
		}

		#endregion

		// calculated

		#region ApplicationIDDataFieldType

		public ZString ApplicationIDDataFieldType
		{
			get { return IsGS1 ? nameof(FieldType.TextDropEdit) : nameof(FieldType.Text); }
		}

		#endregion

		#region DelimiterForBinding

		[MaxLength(1)]
		[ReadOnlyMember(nameof(DelimiterReadOnly))]
		[BusinessObjectMaxLengthTestExclude] // Test will fail as value is wrapped in getter
		[ResourceStringData("BarcodeRuleComponent|Delimiter", Caption = "Delimiter", ShortCaption = "Del.")]
		public ZString DelimiterForBinding
		{
			get
			{
				return HasDelimiter
					? new ZString(((IBarcodeRuleComponent)this).Delimiter).WrapInDoubleQuotes()
					: ZString.Empty;
			}
			set
			{
				var trimmedValue = value.TrimOneDoubleQuoteFromEachEnd();
				// Compare to ZString.Empty instead of using .IsEmpty as .IsEmpty will return true when containing a space.
				// If trimmed value is empty but original value isn't, it can only mean the original value was made up of double quotes
				var valueToSet = trimmedValue == ZString.Empty && value != ZString.Empty ? (ZString)"\"" : trimmedValue;
				CheckMaximumLength(DelimiterForBindingInfo, valueToSet); // Necessary for BizOTestCase

				// Compare to ZString.Empty instead of using .IsEmpty as .IsEmpty will return true when containing a space.
				BRC_Delimiter = valueToSet == ZString.Empty ? (byte)0 : (byte)valueToSet[0];
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Added this Code after failure in DAT tests through BulkAnalyzer script(WI00610195)")]
		bool DelimiterReadOnly
		{
			get { return !HasDelimiter; }
		}

		public ZPropertyInfo DelimiterForBindingInfo
		{
			get { return GetZPropertyInfo(Schema.DelimiterForBinding); }
		}

		#endregion

		#region IsDelimiterMultiComponent

		[ResourceStringData("BarcodeRuleComponent|IsDelimiterMultiComponent", Caption = "Is Del. Multi-Component", MediumCaption = "Is Multi-Component", ShortCaption = "Is Multi-Comp.")]
		public ZBool IsDelimiterMultiComponent
		{
			get { return HasDelimiter && IsDelimiterMultiComponentForRule; }
			set
			{
				var rule = Rule;
				if (rule != null)
				{
					rule.BRU_IsDelimiterMultiComponent = value;

					if (value)
					{
						RemoveDelimiterFromOtherComponents(rule);
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateIsDelimiterMultiComponent();
				}

				IsDelimiterMultiComponentInfo.RefreshBinding();
				RefreshSampleBarcodeInfo();
			}
		}

		void RemoveDelimiterFromOtherComponents(BarcodeRule rule)
		{
			byte? delimiter = null;
			foreach (var component in rule.Components)
			{
				if (component != this && component.HasDelimiter)
				{
					if (!delimiter.HasValue)
					{
						delimiter = component.BRC_Delimiter;
					}
					else
					{
						delimiter = DefaultDelimiter;
					}

					component.HasDelimiter = false; // make the current component the owner of the Multi-Component Delimiter
				}
			}

			if (!HasDelimiter)
			{
				BRC_Delimiter = delimiter ?? DefaultDelimiter;
			}
		}

		bool IsDelimiterMultiComponentForRule
		{
			get
			{
				var rule = Rule;
				return rule != null && rule.BRU_IsDelimiterMultiComponent;
			}
		}

		public ZPropertyInfo IsDelimiterMultiComponentInfo
		{
			get { return GetZPropertyInfo(Schema.IsDelimiterMultiComponent); }
		}

		#endregion

		#region LengthTypeForBinding

		[MaxLength(1)]
		[BusinessObjectMaxLengthTestExclude] // Test will fail as setter takes code, getter returns description
		[List("Lookups.LengthTypes")]
		[ReadOnlyMember(nameof(IsDateFormat))]
		[ResourceStringData("BarcodeRuleComponent|LengthTypeForBinding", Caption = "Length Type", ShortCaption = "Len. Type")]
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
						// tested in BarcodeRuleComponentValidationTest
						Validation.ValidateBRC_MinLength();
						Validation.ValidateBRC_MaxLength();
					}
				}
				else
				{
					LengthTypeForBindingInfo.RefreshBinding();
				}

				Validation.ValidateLengthType();
				RefreshSampleBarcodeInfo();
			}
		}

		public ZPropertyInfo LengthTypeForBindingInfo
		{
			get { return GetZPropertyInfo(Schema.LengthTypeForBinding); }
		}

		#region LengthType

		/// <summary>
		/// This is the internal Code Value for the LengthType, LengthTypeForBinding will use it to
		/// get the Description. Use this value for checking what length type the user has selected.
		/// </summary>
		public ZString LengthType => lengthType ?? this.CalculateLengthType(IsInDatabase);

		ZString? lengthType;

		#endregion

		#endregion

		#region Module

		public ZString Module
		{
			get
			{
				var rule = Rule;
				return rule != null ? rule.Module : ZString.Empty;
			}
		}

		#endregion

		#region ReadOnly

		public override bool ReadOnly
		{
			get { return base.ReadOnly || IsRuleReadOnly; }
			set { base.ReadOnly = value; }
		}

		bool IsRuleReadOnly
		{
			get
			{
				var rule = Rule;
				return rule != null && rule.ReadOnly;
			}
		}

		#endregion

		#region TargetFieldDescription

		[ResourceStringData("BarcodeRuleComponent|TargetFieldDescription", Caption = "Target Field Description", ShortCaption = "Description")]
		public ZString TargetFieldDescription
		{
			get { return Lookups.TargetFields.GetDescriptionFromCode(BRC_TargetField); }
		}

		#endregion

		#endregion

		#region Flags

		#region HasDelimiter

		public const byte DefaultDelimiter = (byte)',';

		[ResourceStringData("BarcodeRuleComponent|HasDelimiter", Caption = "Has Delimiter", ShortCaption = "Has Del.")]
		public ZBool HasDelimiter
		{
			get { return this.HasDelimiter(); }
			set
			{
				if (!value)
				{
					BRC_Delimiter = 0;
				}
				else if (!HasDelimiter)
				{
					BRC_Delimiter = DefaultDelimiter;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateHasDelimiter();
				}

				HasDelimiterInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo HasDelimiterInfo
		{
			get { return GetZPropertyInfo(Schema.HasDelimiter); }
		}

		#endregion

		#region IsApplicationIdentifierBoundToList

		public bool IsApplicationIdentifierBoundToList
		{
			get { return ApplicationIDDataFieldType.EqualsIgnoringCase(nameof(FieldType.TextDropEdit)); }
		}

		#endregion

		#region IsGroupedWithMultiComponentDelimiter

		public bool IsGroupedWithMultiComponentDelimiter
		{
			get
			{
				var rule = Rule;
				return rule != null && !rule.IsPartialRule && rule.Components.Any(c => c.BRC_Sequence <= BRC_Sequence && c.IsDelimiterMultiComponent);
			}
		}

		#endregion

		#region IsGS1

		public bool IsGS1
		{
			get
			{
				var rule = Rule;
				return rule != null && rule.IsGS1;
			}
		}

		#endregion

		#region IsLastComponent

		public bool IsLastComponent
		{
			get
			{
				var rule = Rule;
				return rule != null && rule.Components.Any() && rule.Components.Max(c => c.BRC_Sequence) == BRC_Sequence;
			}
		}

		#endregion

		#endregion

		#region Cloning support

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clone = (BarcodeRuleComponent)base.CloneInternal(args);

			// LengthTypeForBinding takes the code in the setter.
			clone.LengthTypeForBinding = LengthType;
			return clone;
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			return new[] { BarcodeRuleComponentSchema.Constants.BRC_BRU_Rule };
		}

		#endregion

		// interfaces

		#region IBarcodeRuleComponent Members

		string IBarcodeRuleComponent.ApplicationIdentifier
		{
			get { return BRC_ApplicationID; }
		}

		char IBarcodeRuleComponent.Delimiter
		{
			get { return (char)(byte)BRC_Delimiter; }
		}

		FormatType IBarcodeRuleFormat.Format => BarcodeFormatHelper.ParseFormat(BRC_Format);

		bool IBarcodeRuleComponent.IsDelimiterMultiComponent
		{
			get { return IsDelimiterMultiComponent; }
		}

		short IBarcodeRuleFormat.MaxLength
		{
			get { return BRC_MaxLength; }
		}

		short IBarcodeRuleFormat.MinLength
		{
			get { return BRC_MinLength; }
		}

		short IBarcodeRuleComponent.Sequence
		{
			get { return BRC_Sequence; }
		}

		string IBarcodeRuleFormat.TargetField
		{
			get { return BRC_TargetField; }
		}

		#endregion

		#region IBarcodeRuleWithLength

		ZShort IBarcodeRuleWithLength.MinLength
		{
			get => BRC_MinLength;
			set => BRC_MinLength = value;
		}

		ZShort IBarcodeRuleWithLength.MaxLength
		{
			get => BRC_MaxLength;
			set => BRC_MaxLength = value;
		}

		ZPropertyInfo IBarcodeRuleWithLength.MinLengthInfo
		{
			get => BRC_MinLengthInfo;
		}

		ZPropertyInfo IBarcodeRuleWithLength.MaxLengthInfo
		{
			get => BRC_MaxLengthInfo;
		}

		#endregion

		#region Testing
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			BRC_Sequence = 1;
		}

#endif
		#endregion
	}
}
