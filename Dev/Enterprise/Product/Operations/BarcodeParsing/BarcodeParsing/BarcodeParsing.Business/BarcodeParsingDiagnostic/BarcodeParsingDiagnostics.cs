using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BarcodeParsing.Business
{
	public sealed class BarcodeParsingDiagnostics : NonPersistentBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Replacement character")]
		public const string ReplacementCharacterForGS1Terminator = "\u25CF";

		public BarcodeParsingDiagnostics(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			DiagnosticsType = DiagnosticsTypes.Codes.Parsing;
			ModuleCode = BarcodeModuleTypes.Codes.Warehouse;
		}

		#endregion

		#region Barcode

		[ResourceStringData("BarcodeParsingDiagnostics|Barcode", Caption = "Barcode")]
		public ZString Barcode
		{
			get { return barcode; }
			set
			{
				// the GS1 Character will only exist at the beginning and end if we are
				// using a keyboard wedge scanner that was setup for Scan-Pack. We need to
				// strip the pre-amble and post-amble as they are not part of the barcode.
				var sanitisedBarcodeText = value.Trim(ReplacementCharacterForGS1Terminator[0]);

				SetNonPersistentPropertyValue(BarcodeInfo, ref barcode, sanitisedBarcodeText);
			}
		}

		ZString barcode;

		public ZPropertyInfo BarcodeInfo
		{
			get { return GetZPropertyInfo(nameof(Barcode)); }
		}

		#endregion

		#region BuyerPK

		[ResourceStringData("BarcodeParsingDiagnostics|BuyerPK", Caption = "Buyer")]
		[RelatedBusinessObject("Buyer")]
		[List("Lookups.Buyers")]
		public ZGuid BuyerPK
		{
			get { return buyerPK; }
			set
			{
				SetNonPersistentPropertyValue(BuyerPKInfo, ref buyerPK, value);
				ClearOutRelatedEntityIfNotEditable();

				if (!IsValidationSuspended)
				{
					Validation.ValidateBuyerPK();
				}
			}
		}

		ZGuid buyerPK;

		public ZPropertyInfo BuyerPKInfo => GetZPropertyInfo(nameof(BuyerPK));

		public OrgHeader Buyer => Factory.Load<OrgHeader>(BuyerPK);

		void ClearOutRelatedEntityIfNotEditable()
		{
			if (!IsRelatedEntityEditable)
			{
				RelatedEntityPK = ZGuid.Empty;
			}
		}

		bool IsRelatedEntityEditable => GetConsumer().IsRelatedEntityEditable(BuyerPK);

		IBarcodeParsingConsumer GetConsumer() => Factory.GetBarcodeParsingConsumerFromModuleCode(ModuleCode);

		#endregion

		#region ModuleCode

		[ResourceStringData("BarcodeParsingDiagnostics|ModuleCode", Caption = "Module")]
		[List("Lookups.ModuleTypes")]
		public ZString ModuleCode
		{
			get { return moduleCode; }
			set
			{
				if (moduleCode != value)
				{
					RelatedEntityPK = ZGuid.Empty;
				}

				SetNonPersistentPropertyValue(ModuleCodeInfo, ref moduleCode, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateModuleCode();
				}
			}
		}

		ZString moduleCode;

		public ZPropertyInfo ModuleCodeInfo
		{
			get { return GetZPropertyInfo(nameof(ModuleCode)); }
		}

		#endregion

		#region RelatedEntityPK

		[ResourceStringData("BarcodeParsingDiagnostics|RelatedEntityPK", Caption = "Related Entity")]
		[List("Lookups.RelatedEntityList")]
		[ReadOnlyMember(nameof(RelatedEntityPKReadOnly))]
		public ZGuid RelatedEntityPK
		{
			get { return relatedEntityPK; }
			set
			{
				SetNonPersistentPropertyValue(RelatedEntityPKInfo, ref relatedEntityPK, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateRelatedEntityPK();
				}
			}
		}

		ZGuid relatedEntityPK;

		public ZPropertyInfo RelatedEntityPKInfo
		{
			get { return GetZPropertyInfo(nameof(RelatedEntityPK)); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Added this Code after failure in DAT tests through BulkAnalyzer script(WI00610195)")]
		bool RelatedEntityPKReadOnly
		{
			get { return !IsRelatedEntityEditable; }
		}

		#endregion

		#region SupplierPK

		[ResourceStringData("BarcodeParsingDiagnostics|SupplierPK", Caption = "Supplier")]
		[RelatedBusinessObject("Supplier")]
		[List("Lookups.Suppliers")]
		public ZGuid SupplierPK
		{
			get { return supplierPK; }
			set
			{
				SetNonPersistentPropertyValue(SupplierPKInfo, ref supplierPK, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateSupplierPK();
				}
			}
		}
		ZGuid supplierPK;

		public ZPropertyInfo SupplierPKInfo
		{
			get { return GetZPropertyInfo(nameof(SupplierPK)); }
		}

		public OrgHeader Supplier
		{
			get { return Factory.Load<OrgHeader>(SupplierPK); }
		}

		#endregion

		#region IsGS1Barcode

		[ResourceStringData("BarcodeRuleDiagnostic|IsGS1", Caption = "Is GS1")]
		public bool IsGS1Barcode
		{
			get { return isGS1BarcodeOverride || Barcode.Contains(ReplacementCharacterForGS1Terminator, StringComparison.OrdinalIgnoreCase); }
			set { isGS1BarcodeOverride = value; }
		}
		bool isGS1BarcodeOverride;

		#endregion

		#region Types

		[ResourceStringData("BarcodeParsingDiagnostics|DiagnosticsType", Caption = "Type")]
		[List("Lookups.DiagnosticsTypes")]
		public ZString DiagnosticsType
		{
			get { return diagnosticsType; }
			set
			{
				SetNonPersistentPropertyValue(DiagnosticsTypeInfo, ref diagnosticsType, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateDiagnosticsType();
					Validation.ValidateModuleCode();
				}
			}
		}

		ZString diagnosticsType;

		public ZPropertyInfo DiagnosticsTypeInfo => GetZPropertyInfo(nameof(DiagnosticsType));

		public bool IsDiagnosticsTypeParsing => DiagnosticsType == DiagnosticsTypes.Codes.Parsing;

		public bool IsDiagnosticsTypeValidation => DiagnosticsType == DiagnosticsTypes.Codes.Validation;

		#endregion

		#region TargetField

		[ResourceStringData("BarcodeParsingDiagnostics|TargetField", Caption = "Target Field")]
		[List("Lookups.TargetFields")]
		public ZString TargetField
		{
			get { return targetField; }
			set
			{
				SetNonPersistentPropertyValue(TargetFieldInfo, ref targetField, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateTargetField();
				}
			}
		}

		ZString targetField;

		public ZPropertyInfo TargetFieldInfo => GetZPropertyInfo(nameof(TargetField)); 

		#endregion

		#region Criteria availability

		public bool IsRelatedEntityAvailable => GetConsumer().IsRelatedEntityAvailable;

		public bool IsBuyerAvailable => GetConsumer().IsBuyerAvailable;

		public bool IsSupplierAvailable => GetConsumer().IsSupplierAvailable;

		#endregion

		#region Results & Last Matched Rule

		public IBusiness LastMatchedRule { get; private set; }

		[ResourceStringData("BarcodeParsingDiagnostics|Results", Caption = "Results")]
		public ZString Results
		{
			get { return results; }
			set
			{
				SetNonPersistentPropertyValue(ResultsInfo, ref results, value);
				ResultsInfo.RefreshBinding();
			}
		}

		ZString results;

		public ZPropertyInfo ResultsInfo
		{
			get { return GetZPropertyInfo(nameof(Results)); }
		}

		#endregion

		// Calculated

		#region Captions

		public string BuyerCaption => GetConsumer().GetBuyerCaption(BuyerPKInfo.Description);

		public string RelatedEntityCaption => GetConsumer().GetRelatedEntityCaption(RelatedEntityPKInfo.Description);

		public string SupplierCaption => GetConsumer().GetSupplierCaption(SupplierPKInfo.Description);

		#endregion

		public void Run()
		{
			Validation.ValidateAll();
			var consumer = GetConsumer();
			if (!HasErrors && consumer != null)
			{
				var loadMatchingRulesParams = new LoadMatchingRulesParameters(
					ModuleCode,
					BuyerPK,
					SupplierPK,
					relatedEntityPK,
					isBuyerRequiredForRelatedEntity: false,
					IsGS1Barcode);
				var runner = GetRunner();
				var barcodeWithReplacedTerminator = Barcode.Replace(ReplacementCharacterForGS1Terminator, BarcodeRule.GS1Terminator);
				var result = runner.Run(consumer, loadMatchingRulesParams, barcodeWithReplacedTerminator, targetField);
				Results = result.BarcodeRuleProcessingMessage;
				LastMatchedRule = result.MatchedRule;
			}
		}

		IBarcodeDiagnosticsRunner GetRunner() => DiagnosticsType.ToString() switch
		{
			DiagnosticsTypes.Codes.Parsing => new BarcodeParsingDiagnosticsRunner(),
			DiagnosticsTypes.Codes.Validation => new BarcodeValidationDiagnosticsRunner(),
			_ => throw new ArgumentException("Invalid Diagnostics Type", nameof(DiagnosticsType)),
		};

		#region Lookups

		public BarcodeParsingDiagnosticsLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = new BarcodeParsingDiagnosticsLookups(this);
				}
				return lookups;
			}
		}
		BarcodeParsingDiagnosticsLookups lookups;

		#endregion

		#region Validation

		public BarcodeParsingDiagnosticsValidation Validation
		{
			get { return new BarcodeParsingDiagnosticsValidation(this); }
		}

		#endregion
	}
}
