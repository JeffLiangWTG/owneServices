using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BarcodeParsing.Business
{
	public class BarcodeRuleSet : AutoBarcodeRuleSet
	{
		public BarcodeRuleSet(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Entities

		#region RelatedEntity

		public BusinessObject RelatedEntity
		{
			get { return Factory.Load(BRS_RelatedEntityTableCode, BRS_RelatedEntityId); }
		}

		#endregion

		#region Rules

		[ChildEditable]
		public BarcodeRuleCollection Rules
		{
			get
			{
				if (rules == null)
				{
					rules = new BarcodeRuleCollection(this);
					RegisterEditableChildObject(rules);
				}

				return rules;
			}
		}

		BarcodeRuleCollection rules;

		#endregion

		#region ValidationRules

		[ChildEditable]
		public BarcodeValidationRuleCollection ValidationRules
		{
			get
			{
				if (validationRules == null)
				{
					validationRules = new BarcodeValidationRuleCollection(this);
					RegisterEditableChildObject(validationRules);
				}

				return validationRules;
			}
		}

		BarcodeValidationRuleCollection validationRules;

		#endregion

		#endregion

		#region Properties

		// persistent

		#region BRS_IsSystem

		[ReadOnly(true)]
		public override ZBool BRS_IsSystem
		{
			get { return base.BRS_IsSystem; }
			set { base.BRS_IsSystem = value; }
		}

		#endregion

		#region BRS_Module

		[List("Lookups.ModuleTypes")]
		public override ZString BRS_Module
		{
			get { return base.BRS_Module; }
			set
			{
				var previousValue = BRS_Module;
				base.BRS_Module = value;

				if (previousValue != BRS_Module)
				{
					BRS_RelatedEntityId = ZGuid.Empty;
				}
				ClearOutBuyerIfNotAvailable();
				ClearOutSupplierIfNotAvailable();

				// tested in TestBarcodeRuleSetValidation
				if (!IsValidationSuspended)
				{
					Validation.ValidateBRS_OH_Buyer();
					Validation.ValidateBRS_OH_Supplier();
					Validation.ValidateBRS_RelatedEntityId();
				}
			}
		}

		#endregion

		#region BRS_OH_Buyer

		public override ZGuid BRS_OH_Buyer
		{
			get { return base.BRS_OH_Buyer; }
			set
			{
				base.BRS_OH_Buyer = value;
				ClearOutRelatedEntityIfNotEditable();

				// tested in TestBarcodeRuleSetValidation
				if (!IsValidationSuspended)
				{
					Validation.ValidateBRS_Module();
					Validation.ValidateBRS_OH_Supplier();
					Validation.ValidateBRS_RelatedEntityId();
				}
			}
		}

		void ClearOutRelatedEntityIfNotEditable()
		{
			if (!IsRelatedEntityEditable)
			{
				BRS_RelatedEntityId = ZGuid.Empty;
			}
		}

		void ClearOutBuyerIfNotAvailable()
		{
			if (!IsBuyerAvailable)
			{
				BRS_OH_Buyer = ZGuid.Empty;
			}
		}

		void ClearOutSupplierIfNotAvailable()
		{
			if (!IsSupplierAvailable)
			{
				BRS_OH_Supplier = ZGuid.Empty;
			}
		}
		public override ZPropertyInfo BRS_OH_BuyerInfo
		{
			get { return GetZPropertyInfo(BarcodeRuleSetSchema.Constants.BRS_OH_Buyer, BuyerCaption); }
		}

		#endregion

		#region BRS_OH_Supplier

		public override ZGuid BRS_OH_Supplier
		{
			get { return base.BRS_OH_Supplier; }
			set
			{
				base.BRS_OH_Supplier = value;
				ClearOutRelatedEntityIfNotEditable();

				// tested in TestBarcodeRuleSetValidation
				if (!IsValidationSuspended)
				{
					Validation.ValidateBRS_Module();
					Validation.ValidateBRS_OH_Buyer();
					Validation.ValidateBRS_RelatedEntityId();
				}
			}
		}

		public override ZPropertyInfo BRS_OH_SupplierInfo
		{
			get { return GetZPropertyInfo(BarcodeRuleSetSchema.Constants.BRS_OH_Supplier, SupplierCaption); }
		}

		#endregion

		#region BRS_RelatedEntityId

		[List("Lookups.RelatedEntityList")]
		[ReadOnlyMember(nameof(BRS_RelatedEntityIdReadOnly))]
		public override ZGuid BRS_RelatedEntityId
		{
			get { return base.BRS_RelatedEntityId; }
			set
			{
				if (BRS_RelatedEntityId != value)
				{
					// need to set table prefix first so that validation will work (Validation loads the related entity with the table prefix).
					var entity = value.IsValid && Lookups.RelatedEntityList != null ? Factory.Load(Lookups.RelatedEntityList.TypeOfElements, value) : null;
					BRS_RelatedEntityTableCode = entity != null ? entity.TablePrefix : "";
				}

				base.BRS_RelatedEntityId = value;

				// tested in TestBarcodeRuleSetValidation
				if (!IsValidationSuspended)
				{
					Validation.ValidateBRS_Module();
					Validation.ValidateBRS_OH_Buyer();
					Validation.ValidateBRS_OH_Supplier();
				}
			}
		}

		public override ZPropertyInfo BRS_RelatedEntityIdInfo
		{
			get { return GetZPropertyInfo(BarcodeRuleSetSchema.Constants.BRS_RelatedEntityId, RelatedEntityCaption); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Added this Code after failure in DAT tests through BulkAnalyzer script(WI00610195)")]
		bool BRS_RelatedEntityIdReadOnly
		{
			get { return !IsRelatedEntityEditable; }
		}

		#endregion

		// calculated

		#region Captions

		public string BuyerCaption => GetConsumer().GetBuyerCaption(base.BRS_OH_BuyerInfo.Description);

		public string RelatedEntityCaption => GetConsumer().GetRelatedEntityCaption(base.BRS_RelatedEntityIdInfo.Description);

		public string SupplierCaption => GetConsumer().GetSupplierCaption(base.BRS_OH_SupplierInfo.Description);

		#endregion

		bool IsRelatedEntityEditable => GetConsumer().IsRelatedEntityEditable(BRS_OH_Buyer);

		IBarcodeParsingConsumer GetConsumer() => Factory.GetBarcodeParsingConsumerFromModuleCode(BRS_Module);

		#endregion

		#region Delete

		public override void Delete()
		{
			base.Delete();
			Rules.DeleteAll(); // tested by SaveAndDeleteBusinessObject
			ValidationRules.DeleteAll();
		}

		#endregion

		#region Flags

		#region IsRelatedEntityAvailable

		public bool IsRelatedEntityAvailable => GetConsumer().IsRelatedEntityAvailable;

		#endregion

		#region IsBuyerAvailable

		public bool IsBuyerAvailable
		{
			get { return Factory.GetBarcodeParsingConsumerFromModuleCode(BRS_Module).IsBuyerAvailable; }
		}

		#endregion

		#region IsSupplierAvailable

		public bool IsSupplierAvailable
		{
			get { return Factory.GetBarcodeParsingConsumerFromModuleCode(BRS_Module).IsSupplierAvailable; }
		}

		#endregion

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("f8908560-c3ce-4818-8c7d-ec6f8801729f", "Barcode Rule Set"); }
		}

		#endregion

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region ReadOnly

		public override bool ReadOnly
		{
			get { return base.ReadOnly || BRS_IsSystem; }
			set { base.ReadOnly = value; }
		}

		#endregion

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BRS_Module = BarcodeModuleTypes.Codes.Warehouse;
		}

		#endregion

		public int TotalRules => (rules?.Count ?? 0) + (validationRules?.Count ?? 0);
	}
}
