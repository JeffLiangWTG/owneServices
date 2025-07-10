//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoBarcodeRuleSetValidation
//
//    This class should be used for overriding validation in AutoBarcodeRuleSetValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BarcodeParsing.Business
{
	public class BarcodeRuleSetValidation : AutoBarcodeRuleSetValidation
	{
		public BarcodeRuleSetValidation(AutoBarcodeRuleSet parent)
			: base(parent)
		{
		}

		new BarcodeRuleSet Parent
		{
			get { return (BarcodeRuleSet)base.Parent; }
		}

		#region CheckBRS_Module

		protected override void CheckBRS_Module()
		{
			base.CheckBRS_Module();

			MandatoryValidation.CheckEntered(Parent.BRS_ModuleInfo);
			ListValidation.ErrorIfInvalidCode(Parent.BRS_ModuleInfo);
			CheckModuleBuyerSupplierRelatedEntitiesCombinationIsUnique(Parent.BRS_ModuleInfo);
			CheckModuleSupportValidationRules(Parent.BRS_ModuleInfo);
		}

		void CheckModuleBuyerSupplierRelatedEntitiesCombinationIsUnique(ZPropertyInfo info)
		{
			if (!info.HasErrors()
				&& Parent.Lookups.ModuleTypes.ContainsCode(Parent.BRS_Module)
				&& (Parent.BRS_OH_Buyer.IsEmpty || Parent.BRS_OH_Buyer.IsValid)
				&& (Parent.BRS_OH_Supplier.IsEmpty || Parent.BRS_OH_Supplier.IsValid)
				&& (Parent.BRS_RelatedEntityId.IsEmpty || Parent.BRS_RelatedEntityId.IsValid))
			{
				var query = new ZQuery();
				query.AddToFilter(BarcodeRuleSetSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				query.AddToFilter(BarcodeRuleSetSchema.BRS_Module, Parent.BRS_Module);
				query.AddGuidFilterOrIsNullIfGuidIsEmpty(BarcodeRuleSetSchema.BRS_OH_Buyer, Parent.BRS_OH_Buyer);
				query.AddGuidFilterOrIsNullIfGuidIsEmpty(BarcodeRuleSetSchema.BRS_OH_Supplier, Parent.BRS_OH_Supplier);
				query.AddGuidFilterOrIsNullIfGuidIsEmpty(BarcodeRuleSetSchema.BRS_RelatedEntityId, Parent.BRS_RelatedEntityId);
				query.AddToFilter(BarcodeRuleSetSchema.BRS_IsSystem, Parent.BRS_IsSystem);

				if (Parent.Factory.LoadTop1<BarcodeRuleSet>(query) != null)
				{
					var module = Parent.Lookups.ModuleTypes.GetDescriptionFromCode(Parent.BRS_Module);
					var buyer = Parent.Buyer;
					var supplier = Parent.Supplier;
					var buyerName = buyer != null ? buyer.OH_Code.ToString() : Res.GetString("074da8ca-08c8-4589-b5b5-191e85dbdf9c", "<none>");
					var supplierName = supplier != null ? supplier.OH_Code.ToString() : Res.GetString("074da8ca-08c8-4589-b5b5-191e85dbdf9c", "<none>");

					var errorMessage = Parent.BRS_RelatedEntityId.IsEmpty
						? Res.GetString("09463700-289b-4d79-ab0a-120269ea35f8", "A {0} Rule Set for Buyer ({1}) and Supplier ({2}) already exists.", module, buyerName, supplierName)
						: Res.GetString("15e7e5c1-4766-434d-a681-cf29137a39e2", "A {0} Rule Set for Buyer ({1}), Supplier ({2}) and {3} ({4}) already exists.",
							module, buyerName, supplierName, Parent.RelatedEntityCaption, ((ICodeDescription)Parent.RelatedEntity).Code);

					info.AddError(errorMessage);
				}
			}
		}

		void CheckModuleSupportValidationRules(ZPropertyInfo info)
		{
			var consumer = Parent.Factory.GetBarcodeParsingConsumerFromModuleCode(Parent.BRS_Module);
			if (consumer is not IBarcodeValidationRulesConsumer && Parent.ValidationRules.Count > 0)
			{
				info.AddError(Res.GetString("DBED68EB-F513-4CE2-9E5E-AB7E46CE1C14", "This module does not support validation rules."));
			}
		}

		#endregion

		#region CheckBRS_OH_Buyer

		protected override void CheckBRS_OH_Buyer()
		{
			base.CheckBRS_OH_Buyer();
			CheckModuleBuyerSupplierRelatedEntitiesCombinationIsUnique(Parent.BRS_OH_BuyerInfo);
		}

		#endregion

		#region CheckBRS_RelatedEntityId

		protected override void CheckBRS_RelatedEntityId()
		{
			base.CheckBRS_RelatedEntityId();

			CheckBRS_RelatedEntity_RelatedEntityExistsInLookupList();
			CheckModuleBuyerSupplierRelatedEntitiesCombinationIsUnique(Parent.BRS_RelatedEntityIdInfo);
			AddWarningIfRelatedEntityIsEntered();
		}

		void CheckBRS_RelatedEntity_RelatedEntityExistsInLookupList()
		{
			if (!Parent.BRS_RelatedEntityIdInfo.HasErrors() && Parent.RelatedEntity != null)
			{
				ListValidation.ErrorIfInvalidPK(Parent.BRS_RelatedEntityIdInfo);
			}
		}

		void AddWarningIfRelatedEntityIsEntered()
		{
			if (!Parent.BRS_RelatedEntityIdInfo.HasErrors() && Parent.RelatedEntity != null)
			{
				Parent.BRS_RelatedEntityIdInfo.AddWarning(Res.GetString("52a7693d-0005-4f9e-b416-d39241c4751d",
					"Setting a {0} is not advised, only use this if Parsing Rules will differ per {0}.", Parent.RelatedEntityCaption));
			}
		}

		#endregion

		#region CheckBRS_OH_Supplier

		protected override void CheckBRS_OH_Supplier()
		{
			base.CheckBRS_OH_Supplier();
			CheckModuleBuyerSupplierRelatedEntitiesCombinationIsUnique(Parent.BRS_OH_SupplierInfo);
		}

		#endregion

		#region ValidateBRS_HasAtLeastOneRule

		protected void ValidateBRS_HasAtLeastOneRule()
		{
			var ruleSet = Parent;
			if (ruleSet.TotalRules < 1)
			{
				ruleSet.AddRowError(Res.GetString("83137f42-e047-417e-b539-c11b8957c775", "Barcode Rule Sets require at least One parsing rule or One validation rule."));
			}
			else
			{
				ruleSet.RemoveRowError(Res.GetString("83137f42-e047-417e-b539-c11b8957c775", "Barcode Rule Sets require at least One parsing rule or One validation rule."));
			}
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateBRS_HasAtLeastOneRule();
		}

		#endregion
	}
}
