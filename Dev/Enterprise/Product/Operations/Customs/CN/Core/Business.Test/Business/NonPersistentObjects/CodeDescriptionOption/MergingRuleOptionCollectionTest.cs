using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(MergingRuleOptionCollection))]
	class MergingRuleOptionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MergingRuleOptionCollection>
	{
		protected override MergingRuleOptionCollection GetCollectionToTest()
		{
			return Factory.New<JobDeclaration>().MergingRuleOptions;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return GetCollectionToTest().FirstOrDefault();
		}

		public void TestRefreshMergeRuleSelectionsByOptions()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			Factory.Save();
			declaration.MergingRuleOptions[1].Selected = true;
			declaration.MergingRuleOptions[1].HasChanges = true;
			declaration.MergingRuleOptions[3].Selected = true;
			declaration.MergingRuleOptions[3].HasChanges = true;
			Factory.Save();
			AssertEquals("MergingRules should have 2 items", 2, declaration.MergingRules.Count);
			AssertMergingRulesHaveCode(declaration, "TUP");
			AssertMergingRulesHaveCode(declaration, "CIR");
		}

		void AssertMergingRulesHaveCode(JobDeclaration declaration, ZString code)
		{
			Assert("MergingRules should contain item with code " + code, declaration.MergingRules.Cast<MergingRule>().Any(item => item.CY_Code == code));
		}

		public void TestRefreshMergeRuleOptionsBySelections()
		{
			var declaration = Factory.New<JobDeclaration>();
			var code1 = declaration.MergingRules.AddNew();
			code1.CY_Code = "TUP";
			code1.CY_Type = Constants.CusCodeDataTypes.Codes.MergingRule;
			var code2 = declaration.MergingRules.AddNew();
			code2.CY_Code = "CIR";
			code2.CY_Type = Constants.CusCodeDataTypes.Codes.MergingRule;
			Factory.Save();
			declaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertEquals("There should be 2 items selected", 2, declaration.MergingRuleOptions.Cast<CodeDescriptionOption>().Count(option => option.Selected));
			AssertMergingRuleOptionsCodeSelected(declaration, "TUP");
			AssertMergingRuleOptionsCodeSelected(declaration, "CIR");
		}

		void AssertMergingRuleOptionsCodeSelected(JobDeclaration declaration, ZString code)
		{
			Assert("Option " + code + " should be selected", declaration.MergingRuleOptions.Cast<CodeDescriptionOption>().Any(option => option.Selected && option.Code == code));
		}

		public void TestJE_MergeByInfo_ValueChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			var options = declaration.MergingRuleOptions.Cast<CodeDescriptionOption>().ToArray();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			options[0].Selected = true;
			options[2].Selected = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			foreach (var option in options)
			{
				Assert(!option.Selected);
				Assert(option.ReadOnly);
			}

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			foreach (var option in options)
			{
				Assert(!option.ReadOnly);
			}
		}
	}
}
