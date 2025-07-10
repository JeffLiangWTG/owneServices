using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class MultipleSelectionLookupBuilderTest : FilterBuilderTest
	{
		protected override FilterBuilder GetFilterBuilderToTest()
		{
			return new MultipleSelectionLookupBuilder(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
		}

		public void TestGetFilterField()
		{
			MultipleSelectionLookupBuilderForTesting multipleSelectionLookupBuilder = new MultipleSelectionLookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertEquals(typeof(MultipleSelectionLookup), multipleSelectionLookupBuilder.GetFilterField().GetType());
		}

		public void TestCanBuild()
		{
			MultipleSelectionLookupBuilderForTesting multipleSelectionLookupBuilder = new MultipleSelectionLookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertEquals(true, multipleSelectionLookupBuilder.CanBuild("multiple selection lookup"));
		}

		public void TestRegularExpressionToMatchFilterType()
		{
			MultipleSelectionLookupBuilderForTesting multipleSelectionLookupBuilder = new MultipleSelectionLookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertEquals("Shouldn't match string ending in 'lookup code'", false, Regex.IsMatch("organisation lookup code", multipleSelectionLookupBuilder.RegularExpressionToMatchFilterType, RegexOptions.IgnoreCase));
			AssertEquals("Shouldn't match string ending in 'lookup'", false, Regex.IsMatch("organisation lookup", multipleSelectionLookupBuilder.RegularExpressionToMatchFilterType, RegexOptions.IgnoreCase));
			AssertEquals("Should match string ending in 'multi selection lookup'", true, Regex.IsMatch("organisation multiple selection lookup", multipleSelectionLookupBuilder.RegularExpressionToMatchFilterType, RegexOptions.IgnoreCase));
		}

		public void TestConstructorRegisteredBindTos()
		{
			MultipleSelectionLookupBuilderForTesting filterBuilder = new MultipleSelectionLookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'creditor' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Creditor));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'debtor' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Debtor));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'unloco' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Unloco));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'zone' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Zone));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'country' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Country));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'company' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Company));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'importer' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Importer));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'exporter' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Exporter));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'supplier' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Supplier));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'bank account' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.BankAccount));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'cheque book' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.ChequeBook));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'debtor group' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.DebtorGroup));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'creditor group' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.CreditorGroup));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'currency' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Currency));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'branch' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Branch));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'department' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Department));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'gl account' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.GlAccount));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'local account' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.LocalAccount));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'organisation' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Organisation));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'supplierpart' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Supplierpart));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'staff' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Staff));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'meeting resource' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.MeetingResource));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'vessel' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Vessel));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'carrier' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Carrier));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'groups' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Groups));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'charge code' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.ChargeCode));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'container code' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.ContainerCode));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'service level' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.ServiceLevel));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'commodity code' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.CommodityCode));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'product category' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.ProductCategory));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'settlement code' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.SettlementGroup));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'sending agent' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.SendingAgent));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'forwarder' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Forwarder));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'receiving agent' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.ReceivingAgent));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'shipping provider' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.ShippingProvider));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'transaction department' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.TransactionDepartment));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'transaction branch' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.TransactionBranch));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'overseas agent' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.OverseasAgent));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'transport client' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.TransportClient));
		}

		public void TestDoCustomBuilding_UseCodesForWhereClause()
		{
			var builder = new MultipleSelectionLookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			var filterField = new MultipleSelectionLookup(Factory);

			var lookupNode = new StringTreeNode();
			lookupNode.Value = "Organisation multiple selection lookup";

			var typeNode = new StringTreeNode();
			typeNode.Value = "Type";
			typeNode.Children.Add(lookupNode);

			var tree = new StringTreeNode();
			tree.Value = "test";
			tree.Children.Add(typeNode);

			builder.DoCustomBuilding(tree, filterField);
			Assert("FilterField's UseCodesForWhereClause should be false by default", !filterField.UseCodesForWhereClause);

			var useCodesForWhereClauseNode = new StringTreeNode();
			useCodesForWhereClauseNode.Value = "UseCodesForWhereClause";

			tree.Children.Add(useCodesForWhereClauseNode);
			builder.DoCustomBuilding(tree, filterField);

			Assert("FilterField's UseCodesForWhereClause should be true", filterField.UseCodesForWhereClause);
		}

		public void TestDoCustomBuilding_DefaultCurrentCountryToCountryMultipleSelectionLookup()
		{
			var builder = new MultipleSelectionLookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			var filterField = new MultipleSelectionLookup(Factory);

			var lookupNode = new StringTreeNode();
			lookupNode.Value = "country multiple selection lookup";

			var typeNode = new StringTreeNode();
			typeNode.Value = "Type";
			typeNode.Children.Add(lookupNode);

			var tree = new StringTreeNode();
			tree.Value = "test";
			tree.Children.Add(typeNode);

			CombineAssertions(() =>
			{
				builder.DoCustomBuilding(tree, filterField);
				Assert("not default current country", filterField.BindToList.Count == 0);

				var node = new StringTreeNode();
				node.Value = "DefaultCurrentCountryToCountryMultipleSelectionLookup";
				tree.Children.Add(node);

				builder.DoCustomBuilding(tree, filterField);
				Assert("default current country", filterField.BindToList.Count == 1);
			});
		}

		public void TestDoCustomBuilding_HideIfMeetCondition()
		{
			var builder = new MultipleSelectionLookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluatorForHideIfMeetCondition, ReportRunningType.Report);
			var filterField = new MultipleSelectionLookup(Factory);
			var tree = PrepareTreeNodesForHideIfMeetCondition(builder, "<RegistryItem(AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting)>", isRunPreconditionAssert: true);

			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				builder.DoCustomBuilding(tree, filterField);
				AssertEquals("FilterField's style should be Grid when dependent registry is true", MultipleSelectionLookup.Styles.Grid, filterField.Style);
			}
		}

		public void TestDoCustomBuilding_HideIfMeetConditionWithStyle()
		{
			var builder = new MultipleSelectionLookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluatorForHideIfMeetCondition, ReportRunningType.Report);
			var filterField = new MultipleSelectionLookup(Factory);

			var treeRoot = new StringTreeNode();
			var styleNode = new StringTreeNode();
			styleNode.Value = "Style";
			styleNode.Children.Add(new StringTreeNode() { Value = "Grid" });
			treeRoot.Children.Add(styleNode);

			var tree = PrepareTreeNodesForHideIfMeetCondition(builder, "<RegistryItem(AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting)>", true, treeRoot);

			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				builder.DoCustomBuilding(tree, filterField);
				AssertEquals("FilterField's style should be Grid when dependent registry is true", MultipleSelectionLookup.Styles.Grid, filterField.Style);
			}
		}

		public void TestDoCustomBuilding_HideIfMeetConditionWithoutSecurityRigth()
		{
			var builder = new MultipleSelectionLookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluatorForHideIfMeetCondition, ReportRunningType.Report);
			var filterField = new MultipleSelectionLookup(Factory);
			var tree = PrepareTreeNodesForHideIfMeetCondition(builder, "<RegistryItem(AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting)>", isRunPreconditionAssert: true);

			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Env.Security.ReportsExternalBranchFilter.IsAllowed = false;

				builder.DoCustomBuilding(tree, filterField);
				AssertEquals("FilterField's style should be None when dependent registry is false", MultipleSelectionLookup.Styles.None, filterField.Style);
				AssertEquals(0, filterField.BindToList.Count);
			}
		}

		public void TestDoCustomBuilding_HideIfMeetConditionWithInvalidMacro()
		{
			var builder = new MultipleSelectionLookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			var filterField = new MultipleSelectionLookup(Factory);
			var tree = PrepareTreeNodesForHideIfMeetCondition(builder, null);

			var ex = AssertExceptionThrown<TemplateDefinitionException>(() => builder.DoCustomBuilding(tree, filterField));
			AssertEquals("The condition expression '' is not a proper macro or is not supported.", ex.Message);
		}

		public void TestDoCustomBuilding_HideIfMeetConditionWithInvalidConditionResult()
		{
			var builder = new MultipleSelectionLookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			var filterField = new MultipleSelectionLookup(Factory);
			var tree = PrepareTreeNodesForHideIfMeetCondition(builder, "Invalid Result");

			var ex = AssertExceptionThrown<TemplateDefinitionException>(() => builder.DoCustomBuilding(tree, filterField));
			AssertEquals("The condition expression 'Invalid Result' should have a result either be 'TRUE' or 'FALSE'.", ex.Message);
		}

		public void TestDoCustomBuildingInTaskBuild()
		{
			var builder = new MultipleSelectionLookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			var filterField = new MultipleSelectionLookup(Factory);

			var lookupNode = new StringTreeNode();
			lookupNode.Value = "sea voyage multiple selection lookup";

			var columnsOptions = new StringTreeNode();
			columnsOptions.Value = "ColumnsOptions";
			lookupNode.Children.Add(columnsOptions);

			var option = new StringTreeNode();
			option.Value = "Vessel";
			columnsOptions.Children.Add(option);

			var columnName = new StringTreeNode();
			columnName.Value = "columnname";
			option.Children.Add(columnName);

			var columnNameValue = new StringTreeNode();
			columnNameValue.Value = "test";
			columnName.Children.Add(columnNameValue);

			builder.DoCustomBuildingInTaskBuild(lookupNode, filterField);
			AssertEquals("Vessel", filterField.Columns[0].Caption);
			AssertEquals("test", filterField.Columns[0].ColumnName);
		}

		#region Implementaion

		string DummyEvaluatorForHideIfMeetCondition(Match match)
		{
			if (match.Value == "<RegistryItem(AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting)>")
			{
				return AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.Value.ToString();
			}

			return match.Value;
		}

		StringTreeNode PrepareTreeNodesForHideIfMeetCondition(MultipleSelectionLookupBuilderForTesting builder, string conditionMacro, bool isRunPreconditionAssert = false, StringTreeNode treeRoot = null)
		{
			var lookupNode = new StringTreeNode();
			lookupNode.Value = "Branch Multiple Selection Lookup";

			var typeNode = new StringTreeNode();
			typeNode.Value = "Type";
			typeNode.Children.Add(lookupNode);

			var tree = treeRoot ?? new StringTreeNode();
			tree.Value = "test";
			tree.Children.Add(typeNode);

			var filterField = new MultipleSelectionLookup(Factory);

			if (isRunPreconditionAssert)
			{
				builder.DoCustomBuilding(tree, filterField);
				AssertEquals("FilterField's style should be Grid by default", MultipleSelectionLookup.Styles.Grid, filterField.Style);
			}

			var hideIfMeetConditionNode = new StringTreeNode();
			hideIfMeetConditionNode.Value = "HideIfMeetCondition";
			var registryNameNode = new StringTreeNode();
			registryNameNode.Value = conditionMacro;
			hideIfMeetConditionNode.Children.Add(registryNameNode);
			tree.Children.Add(hideIfMeetConditionNode);

			if (isRunPreconditionAssert)
			{
				builder.DoCustomBuilding(tree, filterField);
				AssertEquals("FilterField's style should be None when dependent registry is false", MultipleSelectionLookup.Styles.None, filterField.Style);
			}

			return tree;
		}

		#endregion

		public class MultipleSelectionLookupBuilderForTesting : MultipleSelectionLookupBuilder
		{
			public MultipleSelectionLookupBuilderForTesting(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle) : base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
			{
			}

			protected internal new string RegularExpressionToMatchFilterType => base.RegularExpressionToMatchFilterType;

			internal new FilterField GetFilterField() => base.GetFilterField();
		}
	}
}
