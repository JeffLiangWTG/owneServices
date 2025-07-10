using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class LookupBuilderTest : FilterBuilderTest
	{
		protected override FilterBuilder GetFilterBuilderToTest()
		{
			return new LookupBuilder(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
		}

		public void TestGetFilterField()
		{
			LookupBuilderForTesting filterBuilder = new LookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertEquals(typeof(LookupField), filterBuilder.GetFilterField().GetType());
		}

		public void TestCanBuild()
		{
			LookupBuilderForTesting filterBuilder = new LookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertEquals(true, filterBuilder.CanBuild("lookup"));
		}

		public void TestCanBuildReturnsFalseForAnotherLookups()
		{
			LookupBuilderForTesting filterBuilder = new LookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertEquals("Shouldn't return true for a multiple selection lookup", false, filterBuilder.CanBuild("multipleselectionlookup"));
			AssertEquals("Shouldn't return true for multiple selection lookup with spaces", false, filterBuilder.CanBuild("multiple selection lookup"));
			AssertEquals("Shouldn't return true for multiple selection lookup with spaces", false, filterBuilder.CanBuild("dependence lookup"));
		}

		public void TestDoCustomBuilding_OnlyCurrentCompanyIfSetInCommissionRegistry()
		{
			var validatorPack = new ValidatorPack();
			var filterBuilder = new LookupBuilderForTesting(validatorPack, Factory, DummyEvaluator, ReportRunningType.Report);

			var tree = new StringTreeNode();
			var typeNode = new StringTreeNode();
			typeNode.Value = "Type";
			typeNode.Children.Add(new StringTreeNode() { Value = "company lookup" });
			tree.Children.Add(typeNode);

			var companyCommissionField = new LookupField(Factory);
			companyCommissionField.Validators.Add(validatorPack.OnlyCurrentCompanyIfSetInCommissionRegistry);
			validatorPack.OnlyCurrentCompanyIfSetInCommissionRegistry.Filters.Add(companyCommissionField);

			OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			filterBuilder.DoCustomBuilding(tree, companyCommissionField);
			AssertEquals(ZGuid.Empty, companyCommissionField.ZValue);

			OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			filterBuilder.DoCustomBuilding(tree, companyCommissionField);
			AssertEquals(GlbCompany.CurrentCompany.PK, companyCommissionField.ZValue);
		}

		public void TestRegularExpressionToMatchFilterType()
		{
			LookupBuilderForTesting filterBuilder = new LookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertEquals("Should match string ending in 'lookup'", true, Regex.IsMatch("organisation lookup", filterBuilder.RegularExpressionToMatchFilterType, RegexOptions.IgnoreCase));
		}

		public void TestConstructorRegisteredBindTos()
		{
			LookupBuilderForTesting filterBuilder = new LookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
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
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'shipping provider' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.ShippingProvider));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'forwarder' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Forwarder));
			AssertNotNull("Shouldn't remove existing properties - current reports may break. Expected to find 'dependence' type in the available types", CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Dependence));
		}

		public void TestSetLinkToScheduledReportRecipientForOrganisationIfNeeded()
		{
			var tree = new StringTreeNode();

			var typeNode = new StringTreeNode();
			typeNode.Value = "Type";
			typeNode.Children.Add(new StringTreeNode() { Value = "organisation lookup" });
			tree.Children.Add(typeNode);

			var filterBuilder = new LookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Document);
			var lookupField = filterBuilder.Build(tree, null) as LookupField;
			AssertEquals(false, lookupField.LinkToScheduledReportRecipientForOrganisation);

			filterBuilder = new LookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.DocumentReferenceGuide);
			lookupField = filterBuilder.Build(tree, null) as LookupField;
			AssertEquals(false, lookupField.LinkToScheduledReportRecipientForOrganisation);

			filterBuilder = new LookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.ReportReferenceGuide);
			lookupField = filterBuilder.Build(tree, null) as LookupField;
			AssertEquals(false, lookupField.LinkToScheduledReportRecipientForOrganisation);

			filterBuilder = new LookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			lookupField = filterBuilder.Build(tree, null) as LookupField;
			AssertEquals(false, lookupField.LinkToScheduledReportRecipientForOrganisation);

			filterBuilder = new LookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.NormalScheduledReport);
			lookupField = filterBuilder.Build(tree, null) as LookupField;
			AssertEquals(false, lookupField.LinkToScheduledReportRecipientForOrganisation);

			filterBuilder = new LookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.OneOffScheduledReport);
			tree.Children.Add(new StringTreeNode() { Value = "LinkToScheduledReportRecipientForOrganisation" });
			lookupField = filterBuilder.Build(tree, null) as LookupField;
			AssertEquals(true, lookupField.LinkToScheduledReportRecipientForOrganisation);
			AssertHasWarning(lookupField.ZValueInfo, "This filter value will be overwritten by the recipient's organization value (if valid) when running a scheduled report.");

			filterBuilder = new LookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.NormalScheduledReport);
			lookupField = filterBuilder.Build(tree, null) as LookupField;
			AssertEquals(true, lookupField.LinkToScheduledReportRecipientForOrganisation);
			AssertHasWarning(lookupField.ZValueInfo, "This filter value will be overwritten by the recipient's organization value (if valid) when running a scheduled report.");

			filterBuilder = new LookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			lookupField = filterBuilder.Build(tree, null) as LookupField;
			AssertEquals(true, lookupField.LinkToScheduledReportRecipientForOrganisation);
			AssertHasWarning(lookupField.ZValueInfo, "This filter value will be overwritten by the recipient's organization value (if valid) when running a scheduled report.");

			tree = new StringTreeNode();
			typeNode = new StringTreeNode();
			typeNode.Value = "Type";
			typeNode.Children.Add(new StringTreeNode() { Value = "company lookup" });
			tree.Children.Add(typeNode);
			tree.Children.Add(new StringTreeNode() { Value = "LinkToScheduledReportRecipientForOrganisation" });
			lookupField = filterBuilder.Build(tree, null) as LookupField;
			AssertEquals(false, lookupField.LinkToScheduledReportRecipientForOrganisation);
		}

		public void TestDefaultValue()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var filterBuilder = new LookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);

			var tree = new StringTreeNode();
			var typeNode = new StringTreeNode();
			typeNode.Value = "Type";
			typeNode.Children.Add(new StringTreeNode() { Value = "staff lookup" });
			tree.Children.Add(typeNode);

			var defaultValueNode = new StringTreeNode();
			defaultValueNode.Value = "defaultvalue";
			defaultValueNode.Children.Add(new StringTreeNode() { Value = staff.GS_Code });
			tree.Children.Add(defaultValueNode);

			var staffField = new LookupField(Factory);
			filterBuilder.DoCustomBuilding(tree, staffField);

			AssertEquals("Should set Lookup field value by defaultvalue", staff.PK, staffField.ZValue);
		}

		public class LookupBuilderForTesting : LookupBuilder
		{
			public LookupBuilderForTesting(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle) : base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
			{
			}

			protected internal new string RegularExpressionToMatchFilterType => base.RegularExpressionToMatchFilterType;

			internal new FilterField GetFilterField() => base.GetFilterField();
		}
	}
}
