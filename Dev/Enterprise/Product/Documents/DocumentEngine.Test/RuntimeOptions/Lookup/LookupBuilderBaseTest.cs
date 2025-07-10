using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class LookupBuilderBaseTest : FilterBuilderTest
	{
		protected override FilterBuilder GetFilterBuilderToTest()
		{
			return new LookupBuilder(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
		}

		public void TestSetupListAndModuleIDs()
		{
			LookupBuilderBaseForTesting builder = new LookupBuilderBaseForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			LookupField filterField = new LookupField(Factory);
			AssertNull("FilterField BindToList is null", filterField.BindToList);
			AssertNull("FilterField ModuleID is null", filterField.ModuleID);

			builder.SetupListAndModuleIDs(CollectionProviderTypeCodeDescriptionList.Codes.Organisation, filterField);

			AssertEquals("FilterField's BindToList is now orgs", typeof(OrgHeaderCollection), filterField.BindToList.GetType());
			AssertEquals("FilterField's ModuleID is now orgs", ModuleIDs.Organisation, filterField.ModuleID);
		}

		[NUnit.Framework.ExpectException(typeof(TemplateDefinitionException))]
		public void TestSetupListAndModuleIDsWithInvalidCode()
		{
			LookupBuilderBaseForTesting builder = new LookupBuilderBaseForTesting(new ValidatorPack(), Factory, new MatchEvaluator(DummyEvaluator));
			StringTreeNode orgNode = new StringTreeNode();
			orgNode.Value = "organisation lookup";

			StringTreeNode typeNode = new StringTreeNode();
			typeNode.Value = "type";
			typeNode.Children.Add(orgNode);

			StringTreeNode tree = new StringTreeNode();
			tree.Children.Add(typeNode);

			builder.FilterTree = orgNode;
			LookupField filterField = new LookupField(Factory);

			builder.SetupListAndModuleIDs("blah", filterField); // should throw an exception when given an invalid code
		}

		public void TestDoCustomBuilding()
		{
			LookupBuilder builder = new LookupBuilder(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			LookupField filterField = new LookupField(Factory);

			AssertNull("FilterField BindToList is null", filterField.BindToList);
			AssertNull("FilterField ModuleID is null", filterField.ModuleID);

			StringTreeNode orgNode = new StringTreeNode();
			orgNode.Value = "organisation lookup";

			StringTreeNode typeNode = new StringTreeNode();
			typeNode.Value = "type";
			typeNode.Children.Add(orgNode);

			StringTreeNode tree = new StringTreeNode();
			tree.Value = "test";
			tree.Children.Add(typeNode);

			builder.DoCustomBuilding(tree, filterField);

			AssertEquals("FilterField's BindToList is now orgs", typeof(OrgHeaderCollection), filterField.BindToList.GetType());
			AssertEquals("FilterField's ModuleID is now orgs", ModuleIDs.Organisation, filterField.ModuleID);
		}

		[NUnit.Framework.ExpectException(typeof(TemplateDefinitionException))]
		public void TestGetLookupTypeNameWithoutTypeNode()
		{
			StringTreeNode orgNode = new StringTreeNode();
			orgNode.Value = "organisation tester";

			StringTreeNode typeNode = new StringTreeNode();
			typeNode.Value = "abc";
			typeNode.Children.Add(orgNode);

			StringTreeNode tree = new StringTreeNode();
			tree.Children.Add(typeNode);

			LookupBuilderBaseForTesting builder = new LookupBuilderBaseForTesting(new ValidatorPack(), Factory, new MatchEvaluator(DummyEvaluator));
			builder.GetLookupTypeName(tree);
		}

		public void TestGetLookupTypeNameWithTypeNode()
		{
			LookupBuilderBaseForTesting builder = new LookupBuilderBaseForTesting(new ValidatorPack(), Factory, new MatchEvaluator(DummyEvaluator));
			StringTreeNode orgNode = new StringTreeNode();
			orgNode.Value = "organisation tester";

			StringTreeNode typeNode = new StringTreeNode();
			typeNode.Value = "type";
			typeNode.Children.Add(orgNode);

			StringTreeNode tree = new StringTreeNode();
			tree.Children.Add(typeNode);

			AssertEquals("lookup type should return 'organisation', it is now under the 'type' node", CollectionProviderTypeCodeDescriptionList.Codes.Organisation, builder.GetLookupTypeName(tree));
		}

		public void TestAddValidationAndDefaultWasCalled()
		{
			StringTreeNode lookupNode = new StringTreeNode();
			lookupNode.Value = "dummy tester";

			StringTreeNode typeNode = new StringTreeNode();
			typeNode.Value = "type";
			typeNode.Children.Add(lookupNode);

			StringTreeNode filterTreeNode = new StringTreeNode();
			filterTreeNode.Value = "Hello";
			filterTreeNode.Children.Add(typeNode);

			LookupBuilderBaseForTesting builder = new LookupBuilderBaseForTesting(new ValidatorPack(), Factory, new MatchEvaluator(DummyEvaluator));
			LookupField filterField = new LookupField(Factory);
			CollectionAndModuleIDBuilder.SetLookupForTest("dummy", typeof(DummyCollectionProvider));

			builder.DoCustomBuilding(filterTreeNode, filterField);
			AssertEquals("AddValidationAndDefault was called", true, DummyCollectionProvider.AddValidationAndDefaultWasCalled);
		}

		#region Implementation

		public class LookupBuilderBaseForTesting : LookupBuilderBase
		{
			public LookupBuilderBaseForTesting(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues)
				: base(validators, businessObjectFactory, evaluatorForDefaultValues, ReportRunningType.Report)
			{
			}

			public LookupBuilderBaseForTesting(ValidatorPack validators, BusinessObjectFactory businessObjectFactory,
				MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
				: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
			{
			}

			protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
			{
				throw new NotImplementedException();
			}

			protected override string RegularExpressionToMatchFilterType
			{
				get { return @"tester$"; }
			}

			protected internal new StringTreeNode FilterTree
			{
				set => base.FilterTree = value;
			}

			public override bool CanBuild(string filterType)
			{
				return false;
			}

			protected override FilterField GetFilterField()
			{
				return null;
			}

			protected internal new void SetupListAndModuleIDs(string lookupTypeName, FilterField newField, StringTreeNode fieldTree = null) => base.SetupListAndModuleIDs(lookupTypeName, newField, fieldTree);

			protected internal new string GetLookupTypeName(StringTreeNode fieldTree) => base.GetLookupTypeName(fieldTree);
		}

		class DummyCollectionProvider : CollectionProvider
		{
			public DummyCollectionProvider(BusinessObjectFactory businessObjectFactory)
				: base(businessObjectFactory)
			{
				AddValidationAndDefaultWasCalled = false;
			}

			protected override IBusinessObjectCollection CreateCollection()
			{
				return null;
			}

			public override ModuleIdentifier ModuleID
			{
				get { return null; }
			}

			public override void AddValidationAndDefault(FilterField parentFilterField, ValidatorPack validatorPack)
			{
				AddValidationAndDefaultWasCalled = true;
			}

			public static bool AddValidationAndDefaultWasCalled;
		}

		#endregion
	}
}
