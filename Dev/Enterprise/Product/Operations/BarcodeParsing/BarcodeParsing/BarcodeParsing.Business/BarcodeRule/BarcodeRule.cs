using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BarcodeParsingEngine;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BarcodeParsing.Business
{
	public class BarcodeRule : AutoBarcodeRule, IBarcodeRule
	{
		public BarcodeRule(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public const string GS1Terminator = BarcodeCaptureConstants.GS1Terminator;
		public const string EmptyTerminatorWithQuotes = "\"\"";

		#region Schema

		public new abstract class Schema : AutoBarcodeRule.Schema
		{
			public const string IsPartialRule = "IsPartialRule";
			public const string TerminatorType = "TerminatorType";
			public const string SampleBarcode = "SampleBarcode";
			public const string IsGS1 = "IsGS1";
		}

		#endregion

		#region Related Entities

		#region Components

		[ChildEditable]
		public BarcodeRuleComponentCollection Components
		{
			get
			{
				if (components == null)
				{
					components = new BarcodeRuleComponentCollection(this);
					RegisterEditableChildObject(components);
				}

				return components;
			}
		}

		BarcodeRuleComponentCollection components;

		#endregion

		#region RuleSet

		public BarcodeRuleSet RuleSet => Factory.Load<BarcodeRuleSet>(BRU_BRS_RuleSet);

		#endregion

		#endregion

		#region Properties

		// persistent

		#region BRU_Terminator

		[BusinessObjectMaxLengthTestExclude] // Since setter wraps values in quotes, the BizO MaxLength test will fail, GUI will handle max length for this property
		[ReadOnlyMember(nameof(BRU_TerminatorReadOnly))]
		public override ZString BRU_Terminator
		{
			get { return base.BRU_Terminator; }
			set
			{
				// We trim one Double Quote in case the user edits the terminator and removes only one quote.
				// e.g if we had "12345678", and the user removed the last quote the setter would get ["12345678]. Without
				// this change, it would throw a max length exceeded exception as it would wrap to [""12345678"].
				var valueToWrap = (value == EmptyTerminatorWithQuotes) ? value : value.TrimOneDoubleQuoteFromEachEnd();
				bool wrapInDoubleQuotes = value != GS1Terminator;
				base.BRU_Terminator = wrapInDoubleQuotes ? valueToWrap.WrapInDoubleQuotes() : valueToWrap;
				UpdateNonWrappedTerminator();
			}
		}

		void UpdateNonWrappedTerminator()
		{
			Terminator = StripDoubleQuotes(BRU_Terminator);
		}

		static ZString StripDoubleQuotes(ZString value)
		{
			ZString result = value;

			if (IsWrappedInDoubleQuotes(value))
			{
				result = value.Substring(1, value.Length - 2);
			}

			return result;
		}

		static bool IsWrappedInDoubleQuotes(ZString value) => value.Length > 1 && value.StartsWith("\"", StringComparison.OrdinalIgnoreCase) && value.EndsWith("\"", StringComparison.OrdinalIgnoreCase);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Added this Code after failure in DAT tests through BulkAnalyzer script(WI00610195)")]
		bool BRU_TerminatorReadOnly => !TerminatorType.EqualsIgnoringCase(TerminatorTypes.Codes.UserDefined);

		#endregion

		#region BRU_BRS_RuleSet

		[RelatedBusinessObject("RuleSet")]
		public override ZGuid BRU_BRS_RuleSet
		{
			get { return base.BRU_BRS_RuleSet; }
			set { base.BRU_BRS_RuleSet = value; }
		}

		#endregion

		// calculated

		#region Module

		public ZString Module
		{
			get
			{
				var ruleSet = RuleSet;
				return ruleSet != null ? ruleSet.BRS_Module : ZString.Empty;
			}
		}

		#endregion

		#region ReadOnly

		public override bool ReadOnly
		{
			get { return base.ReadOnly || IsRuleSetReadOnly; }
			set { base.ReadOnly = value; }
		}

		bool IsRuleSetReadOnly
		{
			get
			{
				var ruleSet = RuleSet;
				return ruleSet != null && ruleSet.ReadOnly;
			}
		}

		#endregion

		#region SampleBarcode

		[ResourceStringData("BarcodeRule|SampleBarcode", Caption = "Sample Barcode", ShortCaption = "Sample")]
		public ZString SampleBarcode => BarcodeSampler.GetSampleBarcode(this);

		public ZPropertyInfo SampleBarcodeInfo => GetZPropertyInfo(Schema.SampleBarcode);

		#endregion

		#region Terminator

		/// <summary>
		/// The Real Terminator of this Barcode Rule. Do *NOT* set directly.
		/// </summary>
		ZString Terminator
		{
			get { return terminator ?? (terminator = GetTerminator()).Value; }
			set { terminator = value; }
		}

		ZString GetTerminator() => StripDoubleQuotes(BRU_Terminator);

		ZString? terminator;

		#endregion

		#region TerminatorType

		[MaxLength(3)]
		[List("Lookups.TerminatorTypes")]
		[ResourceStringData("BarcodeRule|TerminatorType", Caption = "Terminator Type")]
		public ZString TerminatorType
		{
			get { return terminatorType ?? (terminatorType = GetTerminatorType()).Value; }
			set
			{
				bool valueHasChanged = SetNonPersistentPropertyValue(TerminatorTypeInfo, ref terminatorType, value);
				if (valueHasChanged)
				{
					UpdateTerminator();
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateTerminatorType();
					ValidateAllApplicationIDs(); // tested in BarcodeRuleComponentValidationTest
					ValidateAllHasDelimiters(); // tested in BarcodeRuleComponentValidationTest
				}
			}
		}

		ZString GetTerminatorType() => (Terminator == GS1Terminator) ? TerminatorTypes.Codes.GS1 : TerminatorTypes.Codes.UserDefined;

		/// <summary>
		/// Add the correct terminator (currently only user-defined and GS1 are supported).
		/// </summary>
		void UpdateTerminator()
		{
			BRU_Terminator = IsGS1 ? (ZString)GS1Terminator : BRU_Terminator.Replace(GS1Terminator, "");
		}

		void ValidateAllApplicationIDs()
		{
			foreach (var component in Components)
			{
				component.Validation.ValidateBRC_ApplicationID();
			}
		}

		public ZPropertyInfo TerminatorTypeInfo => GetZPropertyInfo(Schema.TerminatorType);

		ZString? terminatorType;

		#endregion

		#region TerminatorTypeDescription

		[ResourceStringData("BarcodeRule|TerminatorTypeDescription", Caption = "Terminator Type")]
		public ZString TerminatorTypeDescription => Lookups.TerminatorTypes.GetDescriptionFromCode(TerminatorType);

		#endregion

		#endregion

		#region Flags

		#region IsAllComponentsFixedLength

		public bool IsAllComponentsFixedLength => Components.Any() && Components.All(c => c.IsFixedLength());

		#endregion

		#region IsAllComponentsExceptLastFixedLength

		public bool IsAllComponentsExceptLastFixedLength
		{
			get
			{
				var orderedComponents = Components.OrderBy(c => c.BRC_Sequence).ToArray();
				for (int index = 0; index < orderedComponents.Length - 1; index++)
				{
					if (!orderedComponents[index].IsFixedLength())
					{
						return false;
					}
				}

				return orderedComponents.Any();
			}
		}

		#endregion

		#region IsGS1

		[ResourceStringData("BarcodeRule|IsGS1Rule", Caption = "Is GS1 Rule", ShortCaption = "Is GS1")]
		public ZBool IsGS1
		{
			get { return TerminatorType.EqualsIgnoringCase(TerminatorTypes.Codes.GS1); }
			set
			{
				TerminatorType = value ? TerminatorTypes.Codes.GS1 : TerminatorTypes.Codes.UserDefined;
				IsGS1Info.RefreshBinding();

				// tested in BarcodeRuleComponentValidationTest
				if (!IsValidationSuspended)
				{
					ValidateAllHasDelimiters();
					ValidateAllIsDelimiterMultiComponents();
				}
			}
		}

		public ZPropertyInfo IsGS1Info
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.IsGS1); }
		}

		void ValidateAllHasDelimiters()
		{
			foreach (var component in Components)
			{
				component.Validation.ValidateHasDelimiter();
			}
		}

		#endregion

		#region IsPartialRule

		[ResourceStringData("BarcodeRule|IsPartialRule", Caption = "Is Partial Rule", ShortCaption = "Is Partial")]
		public ZBool IsPartialRule
		{
			get { return isPartialRule ?? (isPartialRule = GetIsPartialRule()).Value; }
			set
			{
				bool valueHasChanged = SetNonPersistentPropertyValue(IsPartialRuleInfo, ref isPartialRule, value);
				if (valueHasChanged)
				{
					Components.RefreshBinding(); // Refresh AllowNew on Components collection for the Grid to update
					UpdateSequenceOnAllComponents();
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateIsPartialRule();
					Validation.ValidateBRU_Terminator(); // tested in BarcodeRuleValidationTest
					ValidateAllApplicationIDs(); // tested in BarcodeRuleComponentValidationTest
					ValidateAllIsDelimiterMultiComponents();
				}
			}
		}

		bool GetIsPartialRule() => Components.Count == 1 && Components.Single().BRC_Sequence == 0;

		void UpdateSequenceOnAllComponents()
		{
			short sequenceToSet = 0;

			foreach (var component in Components)
			{
				component.BRC_Sequence = IsPartialRule ? sequenceToSet : ++sequenceToSet;
			}
		}

		void ValidateAllIsDelimiterMultiComponents()
		{
			foreach (var component in Components)
			{
				component.Validation.ValidateIsDelimiterMultiComponent();
			}
		}

		public ZPropertyInfo IsPartialRuleInfo => GetZPropertyInfo(Schema.IsPartialRule);

		ZBool? isPartialRule;

		#endregion

		#endregion

		#region LoadMatchingRules

		public static IEnumerable<BarcodeRule> LoadMatchingRules(BusinessObjectFactory factory, ZString moduleCode, ZString buyerCode, ZString supplierCode, ZGuid relatedEntityPK, bool isGS1)
		{
			return LoadMatchingRulesHelper.LoadMatchingRules<BarcodeRule>(BuildLoadMatchingRulesQuery, factory, moduleCode, buyerCode, supplierCode, relatedEntityPK, isGS1);
		}

		public static IEnumerable<BarcodeRule> LoadMatchingRules(BusinessObjectFactory factory, ZString moduleCode, ZGuid buyerPK, ZGuid supplierPK, ZGuid relatedEntityPK, bool isGS1)
		{
			return LoadMatchingRulesHelper.LoadMatchingRules<BarcodeRule>(BuildLoadMatchingRulesQuery, factory, moduleCode, buyerPK, supplierPK, relatedEntityPK, isGS1);
		}

		static ZDBOnlyQuery BuildLoadMatchingRulesQuery(ZDBOnlySubQuery subQuery, LoadMatchingRulesParameters parameters)
		{
			var ruleQuery = new ZDBOnlyQuery(typeof(BarcodeRule));
			ruleQuery.AddSubQuery(BarcodeRuleSchema.BRU_BRS_RuleSet, subQuery, JoinCondition.And);
			ruleQuery.AddToFilter(BarcodeRuleSchema.BRU_Terminator, parameters.IsGS1 ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual, GS1Terminator);

			return ruleQuery;
		}

		#endregion

		#region Cloning support

		protected override bool SupportsCloneCore() => true;

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clone = (BarcodeRule)base.CloneInternal(args);

			foreach (var component in Components)
			{
				var newComponent = (BarcodeRuleComponent)component.Clone();
				clone.Components.Add(newComponent);
			}

			return clone;
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning() => new[] { BarcodeRuleSchema.Constants.BRU_BRS_RuleSet };

		#endregion

		#region Delete

		public override void Delete()
		{
			base.Delete();
			Components.DeleteAll(); // tested by SaveAndDeleteBusinessObject
		}

		#endregion

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			var row = ((IBusinessObjectInternals)this).Row;
			row[BarcodeRuleSchema.Constants.BRU_Terminator] = EmptyTerminatorWithQuotes;
		}

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore => BRU_Name.IsEmpty ? Res.GetString("2d00207c-02c9-4afb-b067-ded374495182", "Barcode Rule") : Res.GetString("3402a43c-b3a1-4171-bac8-b69c55e1a756", "Barcode Rule {0}", BRU_Name);

		#endregion

		// interfaces

		#region IBarcodeRule Members

		IEnumerable<IBarcodeRuleComponent> IBarcodeRule.Components => Components;

		short IBarcodeRule.RuleNumber => BRU_RuleNumber;

		string IBarcodeRule.RuleName => BRU_Name;

		string IBarcodeRule.Terminator => Terminator;

		bool IBarcodeRule.IsPartialRule => IsPartialRule;

		#endregion

		//

		#region Testing
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			BRU_RuleNumber = 1;
		}

#endif
		#endregion
	}
}
