using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	public class BarcodeParsingTestHelper
	{
		public BarcodeParsingTestHelper(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		readonly BusinessObjectFactory Factory;

		#region Organisation

		public OrgHeader CreateOrg()
		{
			return CreateOrg("TSTORG");
		}

		public OrgHeader CreateOrg(ZString orgCode)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = orgCode;

			return orgHeader;
		}

		#endregion

		#region OrgSupplierPart

		public OrgSupplierPart CreatePart(ZString productCode, OrgHeader client)
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = productCode;
			part.RelatedOrganisations.AddOwner(client);

			return part;
		}

		#endregion

		#region RuleSet

		public BarcodeRuleSet CreateRuleSet()
		{
			return CreateRuleSet(null, null);
		}

		public BarcodeRuleSet CreateRuleSet(OrgHeader buyer)
		{
			return CreateRuleSet(buyer, null);
		}

		public BarcodeRuleSet CreateRuleSet(OrgHeader buyer, OrgHeader supplier)
		{
			return CreateRuleSet(buyer, supplier, null);
		}

		public BarcodeRuleSet CreateRuleSet(OrgHeader buyer, OrgHeader supplier, BusinessObject relatedEntity)
		{
			var ruleSet = Factory.New<BarcodeRuleSet>();

			if (ruleSet.Lookups.ModuleTypes.ContainsCode(DummyBarcodeParsingConsumer.Module))
			{
				ruleSet.BRS_Module = DummyBarcodeParsingConsumer.Module;
			}

			ruleSet.BRS_IsSystem = false;
			if (buyer != null)
			{
				ruleSet.BRS_OH_Buyer = buyer.PK;
			}

			if (supplier != null)
			{
				ruleSet.BRS_OH_Supplier = supplier.PK;
			}

			if (relatedEntity != null)
			{
				ruleSet.BRS_RelatedEntityId = relatedEntity.PK;
				ruleSet.BRS_RelatedEntityTableCode = relatedEntity.TablePrefix;
			}

			return ruleSet;
		}

		#endregion

		#region Rule

		public BarcodeRule CreateRule()
		{
			return CreateRule("TestRule1");
		}

		public BarcodeRule CreateRule(ZString name)
		{
			return CreateRule(CreateRuleSet(), name);
		}

		public BarcodeRule CreateRule(BarcodeRuleSet ruleSet, bool isGS1 = false)
		{
			return CreateRule(ruleSet, "TestRule1", isGS1);
		}

		public BarcodeRule CreateRule(BarcodeRuleSet ruleSet, ZString name, bool isGS1 = false)
		{
			var rule = ruleSet.Rules.AddNew();
			rule.BRU_Name = name;
			rule.IsGS1 = isGS1;
			return rule;
		}

		public BarcodeRule CreateRule(BarcodeRuleSet ruleSet, ZString name, ZBool isGS1, ZBool isPartialRule)
		{
			return CreateRule(ruleSet, name, isGS1, isPartialRule, string.Empty);
		}

		public BarcodeRule CreateRule(BarcodeRuleSet ruleSet, ZString name, ZBool isGS1, ZBool isPartialRule, string terminator)
		{
			var rule = CreateRule(ruleSet, name, isGS1);
			rule.IsPartialRule = isPartialRule;
			if (!string.IsNullOrEmpty(terminator))
			{
				rule.BRU_Terminator = terminator;
			}
			return rule;
		}

		#endregion

		#region RuleComponent

		public BarcodeRuleComponent CreateRuleComponent(BarcodeRule rule)
		{
			return CreateRuleComponent(rule, 0);
		}

		public BarcodeRuleComponent CreateRuleComponent(BarcodeRule rule, bool hasDelimiter)
		{
			return CreateRuleComponent(rule, 0, 0, hasDelimiter);
		}

		public BarcodeRuleComponent CreateRuleComponent(BarcodeRule rule, short fixedLength)
		{
			return CreateRuleComponent(rule, fixedLength, fixedLength, false);
		}

		public BarcodeRuleComponent CreateRuleComponent(BarcodeRule rule, short minLength, short maxLength)
		{
			return CreateRuleComponent(rule, minLength, maxLength, false);
		}

		public BarcodeRuleComponent CreateRuleComponent(BarcodeRule rule, short minLength, short maxLength, bool hasDelimiter, string format = GS1DataFormatTypes.Codes.AlphaNumericWithSymbols)
		{
			var component = rule.Components.AddNew();
			component.BRC_MinLength = minLength;
			component.BRC_MaxLength = maxLength;
			component.BRC_Format = format;
			component.BRC_TargetField = DummyTargetFields.Codes.TargetField3;
			component.HasDelimiter = hasDelimiter;
			return component;
		}

		public BarcodeRuleComponent CreateRuleComponent(BarcodeRule rule, string targetField, string applicationID, string format = GS1DataFormatTypes.Codes.AlphaNumericWithSymbols)
		{
			var component = CreateRuleComponent(rule);
			component.BRC_Format = format;
			component.BRC_TargetField = targetField;
			component.BRC_ApplicationID = applicationID;
			return component;
		}

		#endregion

		#region BarcodeValidationRule

		public BarcodeValidationRule CreateValidationRule() => CreateValidationRule(CreateRuleSet(), 0, 0, string.Empty);

		public BarcodeValidationRule CreateValidationRule(BarcodeRuleSet ruleSet, short minLength, short maxLength) =>
			CreateValidationRule(ruleSet, minLength, maxLength, string.Empty, GS1DataFormatTypes.Codes.AlphaNumericWithSymbols);

		public BarcodeValidationRule CreateValidationRule(string targetField) => CreateValidationRule(CreateRuleSet(), 0, 0, targetField);

		public BarcodeValidationRule CreateValidationRule(BarcodeRuleSet ruleSet, string targetField, string format = GS1DataFormatTypes.Codes.AlphaNumericWithSymbols) =>
			CreateValidationRule(ruleSet, 0, 0, targetField, format);

		public BarcodeValidationRule CreateValidationRule(BarcodeRuleSet ruleSet, short minLength, short maxLength, string targetField, string format = GS1DataFormatTypes.Codes.AlphaNumericWithSymbols)
		{
			var rule = ruleSet.ValidationRules.AddNew();
			rule.BVR_MinLength = minLength;
			rule.BVR_MaxLength = maxLength;
			rule.BVR_Format = format;
			rule.BVR_TargetField = targetField;
			return rule;
		}

		#endregion
	}
}
