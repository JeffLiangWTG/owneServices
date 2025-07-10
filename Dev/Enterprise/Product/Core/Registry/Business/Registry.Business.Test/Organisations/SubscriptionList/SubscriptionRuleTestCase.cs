using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SubscriptionRule))]
	sealed class SubscriptionRuleTestCase : RegistryBusinessObjectTestCaseBase
	{
		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		public void TestCodeValidation()
		{
			var collection = new SubscriptionRuleCollection();
			var rule_1 = collection.AddNewRule("CD1", (NoResString)"Default Pubished List AAA", false, false, new string[] { "PRINT;SLT30;DESC1;SUM1", "RADIO;PREAP;DESC2;SUM2" });
			var rule_2 = collection.AddNewRule("", (NoResString)"Default Pubished List YYY", false, false, new string[] { "TELEV;EXIST;DESC1;SUM1", "PRINT;EXIST;DESC2;SUM2" });

			rule_1.ValidateCode();
			AssertNoNotifications(rule_1.CodeInfo);

			rule_2.ValidateCode();
			AssertHasError("Empty value error found.", rule_2.CodeInfo, "Please enter a Code.");
		}

		public void TestCampaignTypeValidation()
		{
			var collection = new SubscriptionRuleCollection();
			var rule_1 = collection.AddNewRule("CD1", (NoResString)"Default Published List XXX", false, false, new string[] { "PRINT;SLT30;DESC1;SUM1", "RADIO;PREAP;DESC2;SUM2" });
			var rule_2 = collection.AddNewRule("CD2", (NoResString)"Default Published List YYY", false, true, new string[] { "TELEV;EXIST;DESC1;SUM1", "PRINT;EXIST;DESC2;SUM2" });

			AssertEquals("Added a CRM rule", SubscriptionRuleCampaignTypeList.Codes.ClientRelationshipManagement, rule_1.CampaignType);
			AssertEquals("Added a HRM rule", SubscriptionRuleCampaignTypeList.Codes.HumanResourcesManagement, rule_2.CampaignType);
			AssertNoNotifications(rule_1.CampaignTypeInfo);
			AssertNoNotifications(rule_2.CampaignTypeInfo);

			rule_1.CampaignType = ZString.Empty;
			AssertHasError("Empty value error found.", rule_1.CampaignTypeInfo, "Please enter a value.");

			rule_1.CampaignType = "BLA";
			AssertHasError("Incorrect value error found.", rule_1.CampaignTypeInfo, "Enter a valid selection.");

			rule_1.CampaignType = SubscriptionRuleCampaignTypeList.Codes.HumanResourcesManagement;
			AssertNoNotifications(rule_1.CampaignTypeInfo);
		}

		public void TestCampaignTypeMediaCategoryAndTypeValidation()
		{
			var collectionA = new CodeDescriptionBoolCollection
			{
				{ "AAA", (NoResString)"Category A", true }
			};

			var collectionB = new CodeDescriptionBoolCollection
			{
				{ "BBB", (NoResString)"Category B", true }
			};

			var collectionC = new CodeDescriptionBoolCollection
			{
				{ "CCC", (NoResString)"Category C", true }
			};

			var collectionD = new CodeDescriptionBoolCollection
			{
				{ "DDD", (NoResString)"Category D", true }
			};

			OrganisationsDataRegistry.Instance.CampaignCategory2List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionA);
			OrganisationsDataRegistry.Instance.HRCampaignCategory2List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionB);
			OrganisationsDataRegistry.Instance.CampaignCategory1List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionC);
			OrganisationsDataRegistry.Instance.HRCampaignCategory1List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionD);

			var collection = new SubscriptionRuleCollection();
			var rule_1 = collection.AddNewRule("CD1", (NoResString)"Default Published List XXX", false, false, new string[] { "AAA;CCC;DESC1;SUM1", "AAA;CCC;DESC2;SUM2" });

			AssertEquals("Rule 1 should have 2 nodes", 2, rule_1.Nodes.Count);

			foreach (SubscriptionProperties node in rule_1.Nodes)
			{
				AssertNoNotifications("No notifications since AAA is a CRM value and the campaign type is CRM.", node.MediaCategoryWithAllInfo);
				AssertNoNotifications("No notifications since AAA is a CRM value and the campaign type is CRM.", node.MediaTypeWithAllInfo);
			}

			rule_1.CampaignType = SubscriptionRuleCampaignTypeList.Codes.HumanResourcesManagement;

			foreach (SubscriptionProperties node in rule_1.Nodes)
			{
				AssertHasError("Error should occur since AAA is a CRM value and the campaign type is HRM.", node.MediaCategoryWithAllInfo, "Enter a valid selection.");
				AssertHasError("Error should occur since AAA is a CRM value and the campaign type is HRM.", node.MediaTypeWithAllInfo, "Enter a valid selection.");
			}
		}

		public void TestIsDefaultForBinding_ShouldSetIsDefaultToFalseForOtherItemsOfTheSameCampaignTypeInCollection()
		{
			var collection = new SubscriptionRuleCollection();
			var item1 = collection.AddNew();
			var item2 = collection.AddNew();
			var item3 = collection.AddNew();

			item1.CampaignType = SubscriptionRuleCampaignTypeList.Codes.HumanResourcesManagement;
			item2.CampaignType = SubscriptionRuleCampaignTypeList.Codes.HumanResourcesManagement;
			item3.CampaignType = SubscriptionRuleCampaignTypeList.Codes.ClientRelationshipManagement;
			item1.IsDefaultForBinding = true;
			item2.IsDefaultForBinding = false;
			item3.IsDefaultForBinding = false;
			AssertEquals(true, item1.IsDefault);
			AssertEquals(false, item2.IsDefault);
			AssertEquals(false, item3.IsDefault);

			item2.IsDefaultForBinding = true;
			AssertEquals(false, item1.IsDefault);
			AssertEquals(true, item2.IsDefault);
			AssertEquals(false, item3.IsDefault);

			item3.IsDefaultForBinding = true;
			AssertEquals(false, item1.IsDefault);
			AssertEquals(true, item2.IsDefault);
			AssertEquals(true, item3.IsDefault);
		}

		public void TestIsDefaultValidation_ShouldAllowDefaultsOfDifferentCampaignTypes()
		{
			var collection = new SubscriptionRuleCollection();
			var rule_1 = collection.AddNewRule("CD1", (NoResString)"Default Published List XXX", true, false, new string[] { "PRINT;SLT30;DESC1;SUM1", "RADIO;PREAP;DESC2;SUM2" });
			var rule_2 = collection.AddNewRule("CD2", (NoResString)"Default Published List YYY", true, true, new string[] { "TELEV;EXIST;DESC1;SUM1", "PRINT;EXIST;DESC2;SUM2" });

			AssertEquals("Added a CRM rule", SubscriptionRuleCampaignTypeList.Codes.ClientRelationshipManagement, rule_1.CampaignType);
			AssertEquals("Added a HRM rule", SubscriptionRuleCampaignTypeList.Codes.HumanResourcesManagement, rule_2.CampaignType);
			AssertEquals("CRM rule is default", true, rule_1.IsDefault);
			AssertEquals("HRM rule is default", true, rule_2.IsDefault);
			AssertNoNotifications(rule_1.IsDefaultInfo);
			AssertNoNotifications(rule_2.IsDefaultInfo);

			rule_2.IsDefaultForBinding = false;
			AssertNoNotifications("No notifications since CRM rule is still default", rule_2.IsDefaultInfo);

			rule_1.IsDefaultForBinding = false;
			AssertHasError("There must be at least a default item for CRM or HRM.", rule_1.IsDefaultInfo, "There must be a default item.");

			rule_1.IsDefaultForBinding = true;
			rule_2.IsDefaultForBinding = true;
			AssertEquals("CRM rule is default", true, rule_1.IsDefault);
			AssertEquals("HRM rule is default", true, rule_2.IsDefault);
			AssertNoNotifications(rule_1.IsDefaultInfo);
			AssertNoNotifications(rule_2.IsDefaultInfo);

			rule_2.CampaignType = rule_1.CampaignType;
			AssertHasError("There cannot be more than 1 default per Campaign Type", rule_2.IsDefaultInfo, "There can only be one default per Campaign Type.");
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var collection = new SubscriptionRuleCollection();
			var result = collection.AddNewRule("CD1", (NoResString)"Default Pubished List AAA", false, false, new string[] { "PRINT;SLT30;DESC1;SUM1", "RADIO;PREAP;DESC2;SUM2" });

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return (SubscriptionRule)GetNewBusinessObject();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
