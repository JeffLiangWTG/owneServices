using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	public class CommissionHeaderBuilderTest : TestCaseWithFactory
	{
		#region Create

		public void TestCreate_MultipleStreams()
		{
			var adlStaff = Factory.NewWithValidTestData<GlbStaff>();
			adlStaff.GS_Code = "ADL";
			var scwStaff = Factory.NewWithValidTestData<GlbStaff>();
			scwStaff.GS_Code = "SCW";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();

			var agreementA = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreementForAllItems(opportunity, org);
			agreementA.CA0_CommissionStream = "AAA";
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(agreementA, adlStaff, 10);

			var agreementB = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreementForAllItems(opportunity, org);
			agreementB.CA0_CommissionStream = "BBB";
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(agreementB, scwStaff, 5);

			Factory.Save();

			var invoice = Factory.New<ARInvoice>();

			var createCommissionContext = new CreateCommissionContext();
			var buildItemArgs = new CommissionHeaderBuildItemArgs(invoice, invoice, org.PK, "SHP", "ALL", "ALL", new ZDate(2002, 2, 2), new ZDate(2002, 2, 2), "POS", "", "", "");
			var buildItem = new CommissionHeaderBuildItem(createCommissionContext, buildItemArgs, (header, agreement) =>
				{
					var lineGroup = header.LineGroups.AddNew();
					lineGroup.CLG_TotalCommissionableAmount = 100;
					lineGroup.CLG_RX_NKCommissionCurrency = "AUD";
				});

			var builder = new CommissionHeaderBuilder(Factory, createCommissionContext);
			builder.Create(buildItem);

			var headers = Factory.Load<AccCommissionHeader>(new ZQuery());
			AssertEquals(2, headers.Length);
			AssertEquals("AAA", 1, headers.Count(x => x.CH0_CommissionStream == "AAA"));
			AssertEquals("BBB", 1, headers.Count(x => x.CH0_CommissionStream == "BBB"));
		}

		#endregion

		#region Existing Commission

		public void TestUpdateExistingCommissionsAndGetCommissionTargetsToCreateFor_WithOverwiteOldValues()
		{
			var customer1 = Factory.New<OrgHeader>();
			var customer2 = Factory.New<OrgHeader>();
			var invoice1 = Factory.New<ARInvoice>();
			var invoice2 = Factory.New<ARInvoice>();
			var job1 = Factory.NewJobForTesting<JobHeader>();
			var job2 = Factory.NewJobForTesting<JobHeader>();
			var agreement1 = Factory.New<OrgCommissionAgreement>();
			var agreement2 = Factory.New<OrgCommissionAgreement>();

			var commissionHeaderA = Factory.New<AccCommissionHeader>();
			commissionHeaderA.CH0_CA0 = agreement1.PK;
			commissionHeaderA.CH0_AH_Source = invoice1.PK;
			commissionHeaderA.CH0_GroupingSourceID = job1.PK;
			commissionHeaderA.CH0_GroupingSourceTableCode = job1.TablePrefix;
			commissionHeaderA.CH0_OH_Customer = customer1.PK;
			commissionHeaderA.CH0_Product = "XXX";
			commissionHeaderA.CH0_Service = "XXX";
			commissionHeaderA.CH0_SubModule = "XXX";

			var commissionHeaderB = Factory.New<AccCommissionHeader>();
			commissionHeaderB.CH0_CA0 = agreement1.PK;
			commissionHeaderB.CH0_AH_Source = invoice2.PK;
			commissionHeaderB.CH0_GroupingSourceID = job1.PK;
			commissionHeaderB.CH0_GroupingSourceTableCode = job1.TablePrefix;
			commissionHeaderB.CH0_OH_Customer = customer1.PK;
			commissionHeaderB.CH0_Product = "XXX";
			commissionHeaderB.CH0_Service = "XXX";
			commissionHeaderB.CH0_SubModule = "XXX";

			var commissionHeaderC = Factory.New<AccCommissionHeader>();
			commissionHeaderC.CH0_CA0 = agreement1.PK;
			commissionHeaderC.CH0_AH_Source = invoice1.PK;
			commissionHeaderC.CH0_GroupingSourceID = job2.PK;
			commissionHeaderC.CH0_GroupingSourceTableCode = job2.TablePrefix;
			commissionHeaderC.CH0_OH_Customer = customer1.PK;
			commissionHeaderC.CH0_Product = "XXX";
			commissionHeaderC.CH0_Service = "XXX";
			commissionHeaderC.CH0_SubModule = "XXX";

			var commissionHeaderD = Factory.New<AccCommissionHeader>();
			commissionHeaderD.CH0_CA0 = agreement1.PK;
			commissionHeaderD.CH0_AH_Source = invoice1.PK;
			commissionHeaderD.CH0_GroupingSourceID = job1.PK;
			commissionHeaderD.CH0_GroupingSourceTableCode = job1.TablePrefix;
			commissionHeaderD.CH0_OH_Customer = customer2.PK;
			commissionHeaderD.CH0_Product = "XXX";
			commissionHeaderD.CH0_Service = "XXX";
			commissionHeaderD.CH0_SubModule = "XXX";

			var commissionHeaderE = Factory.New<AccCommissionHeader>();
			commissionHeaderE.CH0_CA0 = agreement1.PK;
			commissionHeaderE.CH0_AH_Source = invoice1.PK;
			commissionHeaderE.CH0_GroupingSourceID = job1.PK;
			commissionHeaderE.CH0_GroupingSourceTableCode = job1.TablePrefix;
			commissionHeaderE.CH0_OH_Customer = customer1.PK;
			commissionHeaderE.CH0_Product = "YYY";
			commissionHeaderE.CH0_Service = "XXX";
			commissionHeaderE.CH0_SubModule = "XXX";

			var commissionHeaderF = Factory.New<AccCommissionHeader>();
			commissionHeaderF.CH0_CA0 = agreement1.PK;
			commissionHeaderF.CH0_AH_Source = invoice1.PK;
			commissionHeaderF.CH0_GroupingSourceID = job1.PK;
			commissionHeaderF.CH0_GroupingSourceTableCode = job1.TablePrefix;
			commissionHeaderF.CH0_OH_Customer = customer1.PK;
			commissionHeaderF.CH0_Product = "XXX";
			commissionHeaderF.CH0_Service = "YYY";
			commissionHeaderF.CH0_SubModule = "XXX";

			var commissionHeaderG = Factory.New<AccCommissionHeader>();
			commissionHeaderG.CH0_CA0 = agreement1.PK;
			commissionHeaderG.CH0_AH_Source = invoice1.PK;
			commissionHeaderG.CH0_GroupingSourceID = job1.PK;
			commissionHeaderG.CH0_GroupingSourceTableCode = job1.TablePrefix;
			commissionHeaderG.CH0_OH_Customer = customer1.PK;
			commissionHeaderG.CH0_Product = "XXX";
			commissionHeaderG.CH0_Service = "XXX";
			commissionHeaderG.CH0_SubModule = "YYY";

			var commissionHeaderH = Factory.New<AccCommissionHeader>();
			commissionHeaderH.CH0_CA0 = agreement1.PK;
			commissionHeaderH.CH0_CommissionStream = "WBP";
			commissionHeaderH.CH0_AH_Source = invoice1.PK;
			commissionHeaderH.CH0_GroupingSourceID = job1.PK;
			commissionHeaderH.CH0_GroupingSourceTableCode = job1.TablePrefix;
			commissionHeaderH.CH0_OH_Customer = customer1.PK;
			commissionHeaderH.CH0_Product = "XXX";
			commissionHeaderH.CH0_Service = "XXX";
			commissionHeaderH.CH0_SubModule = "XXX";

			var commissionHeaderI = Factory.New<AccCommissionHeader>();
			commissionHeaderI.CH0_CA0 = agreement1.PK;
			commissionHeaderI.CH0_AH_Source = invoice1.PK;
			commissionHeaderI.CH0_GroupingSourceID = job1.PK;
			commissionHeaderI.CH0_GroupingSourceTableCode = job1.TablePrefix;
			commissionHeaderI.CH0_OH_Customer = customer1.PK;
			commissionHeaderI.CH0_Product = "XXX";
			commissionHeaderI.CH0_Service = "XXX";
			commissionHeaderI.CH0_SubModule = "XXX";
			commissionHeaderI.CH0_Mode = "SEA";

			var commissionHeaderJ = Factory.New<AccCommissionHeader>();
			commissionHeaderJ.CH0_CA0 = agreement1.PK;
			commissionHeaderJ.CH0_AH_Source = invoice1.PK;
			commissionHeaderJ.CH0_GroupingSourceID = job1.PK;
			commissionHeaderJ.CH0_GroupingSourceTableCode = job1.TablePrefix;
			commissionHeaderJ.CH0_OH_Customer = customer1.PK;
			commissionHeaderJ.CH0_Product = "XXX";
			commissionHeaderJ.CH0_Service = "XXX";
			commissionHeaderJ.CH0_SubModule = "XXX";
			commissionHeaderJ.CH0_Mode = "ALL";
			commissionHeaderJ.CH0_NKOrigin = "UAIEV";
			commissionHeaderJ.CH0_NKDestination = "AUSYD";

			var commissionHeaderK = Factory.New<AccCommissionHeader>();
			commissionHeaderK.CH0_CA0 = agreement1.PK;
			commissionHeaderK.CH0_AH_Source = invoice1.PK;
			commissionHeaderK.CH0_GroupingSourceID = job1.PK;
			commissionHeaderK.CH0_GroupingSourceTableCode = job1.TablePrefix;
			commissionHeaderK.CH0_OH_Customer = customer1.PK;
			commissionHeaderK.CH0_Product = "XXX";
			commissionHeaderK.CH0_Service = "XXX";
			commissionHeaderK.CH0_SubModule = "XXX";
			commissionHeaderK.CH0_Mode = "ALL";
			commissionHeaderK.CH0_NKOrigin = "UA";
			commissionHeaderK.CH0_NKDestination = "AU";

			var context = new CreateCommissionContext();
			context.OverwriteOldValues = true;
			var agreementAndRates = new CommissionAgreementAndRatesForTesting();
			agreementAndRates.CommissionAgreement = agreement2;
			context.AgreementAndRatesOverride = new Dictionary<ZString, ICommissionAgreementAndRates>() { { ZString.Empty, agreementAndRates } };
			context.OnlyCreateForAgreementAndRatesOverrideStreams = true;

			var builder = new CommissionHeaderBuilderForTesting(Factory, context);
			var itemArgs = new CommissionHeaderBuildItemArgs(invoice1, job1, customer1.PK, "XXX", "XXX", "XXX", new ZDate(2002, 2, 2), new ZDate(2002, 2, 2), AccCommissionHeaderSnapshotEventList.Codes.Posted, "ALL", "AUSYD", "USLAX");
			var item = new CommissionHeaderBuildItem(context, itemArgs, (x, y) => { });
			builder.UpdateExistingCommissionsAndGetCommissionTargetsToCreateFor_ExposedForTesting(item);
			AssertEquals("Should not have overriden since trade lane does not match", false, commissionHeaderJ.IsOverriden);
			AssertEquals("Should not have overriden since trade lane does not match", false, commissionHeaderK.IsOverriden);

			itemArgs = new CommissionHeaderBuildItemArgs(invoice1, job1, customer1.PK, "XXX", "XXX", "XXX", new ZDate(2002, 2, 2), new ZDate(2002, 2, 2), AccCommissionHeaderSnapshotEventList.Codes.Posted, "ALL", "", "");
			item = new CommissionHeaderBuildItem(context, itemArgs, (x, y) => { });
			builder.UpdateExistingCommissionsAndGetCommissionTargetsToCreateFor_ExposedForTesting(item);

			AssertEquals("Should have overriden since all customer, product, service, and submodule match", true, commissionHeaderA.IsOverriden);
			AssertEquals("Should not have overriden since for different invoice", false, commissionHeaderB.IsOverriden);
			AssertEquals("Should not have overriden since for different grouping source", false, commissionHeaderC.IsOverriden);
			AssertEquals("Should not have overriden since for different customer", false, commissionHeaderD.IsOverriden);
			AssertEquals("Should not have overriden since for different product", false, commissionHeaderE.IsOverriden);
			AssertEquals("Should not have overriden since for different service", false, commissionHeaderF.IsOverriden);
			AssertEquals("Should not have overriden since for different submodule", false, commissionHeaderG.IsOverriden);
			AssertEquals("Should not have overriden since for different commission stream", false, commissionHeaderH.IsOverriden);
			AssertEquals("Should have overriden since for ALL covers SEA Mode", true, commissionHeaderI.IsOverriden);
			AssertEquals("Should have overriden since all customer, product, service, and submodule match and trade lane is empty", true, commissionHeaderJ.IsOverriden);
			AssertEquals("Should have overriden since all customer, product, service, and submodule match and trade lane is empty", true, commissionHeaderK.IsOverriden);

			invoice2.AH_IsCancelled = true;
			itemArgs = new CommissionHeaderBuildItemArgs(invoice2, job1, customer1.PK, "XXX", "XXX", "XXX", new ZDate(2002, 2, 2), new ZDate(2002, 2, 2), AccCommissionHeaderSnapshotEventList.Codes.Posted, "", "", "");
			item = new CommissionHeaderBuildItem(context, itemArgs, (x, y) => { });
			builder.UpdateExistingCommissionsAndGetCommissionTargetsToCreateFor_ExposedForTesting(item);
			AssertEquals("Should not have overriden since for cancelled invoice", false, commissionHeaderB.IsOverriden);

			itemArgs = new CommissionHeaderBuildItemArgs(invoice1, job1, customer1.PK, "XXX", "XXX", "XXX", new ZDate(2002, 2, 2), new ZDate(2002, 2, 2), AccCommissionHeaderSnapshotEventList.Codes.Posted, "ALL", "UAIEV", "AUSYD");
			item = new CommissionHeaderBuildItem(context, itemArgs, (x, y) => { });
			builder.UpdateExistingCommissionsAndGetCommissionTargetsToCreateFor_ExposedForTesting(item);
			AssertEquals("Should have overriden since trade lane matches", true, commissionHeaderJ.IsOverriden);
			AssertEquals("Should have overriden since trade lane matches", true, commissionHeaderK.IsOverriden);

			itemArgs = new CommissionHeaderBuildItemArgs(invoice1, job1, customer1.PK, "XXX", "XXX", "XXX", new ZDate(2002, 2, 2), new ZDate(2002, 2, 2), AccCommissionHeaderSnapshotEventList.Codes.Posted, "ALL", "UA", "AU");
			item = new CommissionHeaderBuildItem(context, itemArgs, (x, y) => { });
			builder.UpdateExistingCommissionsAndGetCommissionTargetsToCreateFor_ExposedForTesting(item);
			AssertEquals("Should have overriden since trade lane matches", true, commissionHeaderJ.IsOverriden);
			AssertEquals("Should have overriden since trade lane matches", true, commissionHeaderK.IsOverriden);

			itemArgs = new CommissionHeaderBuildItemArgs(invoice1, job1, customer1.PK, "XXX", "XXX", "XXX", new ZDate(2002, 2, 2), new ZDate(2002, 2, 2), AccCommissionHeaderSnapshotEventList.Codes.Posted, "AIR", "", "");
			item = new CommissionHeaderBuildItem(context, itemArgs, (x, y) => { });
			builder.UpdateExistingCommissionsAndGetCommissionTargetsToCreateFor_ExposedForTesting(item);
			AssertEquals("Should not have overriden since for different Mode", true, commissionHeaderI.IsOverriden);
		}

		public void TestUnrelatedAgreementDoesNotOverrideCommission()
		{
			var customer = Factory.New<OrgHeader>();
			var invoice = Factory.New<ARInvoice>();
			var job = Factory.NewJobForTesting<JobHeader>();
			var agreement1 = Factory.New<OrgCommissionAgreement>();
			agreement1.CA0_EffectiveDate = new ZDate(2002, 1, 1);
			var agreement2 = Factory.New<OrgCommissionAgreement>();
			agreement2.CA0_EffectiveDate = new ZDate(2002, 1, 1);

			var commissionHeader1 = Factory.New<AccCommissionHeader>();
			commissionHeader1.CH0_CA0 = agreement1.PK;
			commissionHeader1.CH0_AH_Source = invoice.PK;
			commissionHeader1.CH0_GroupingSourceID = job.PK;
			commissionHeader1.CH0_GroupingSourceTableCode = job.TablePrefix;
			commissionHeader1.CH0_OH_Customer = customer.PK;
			commissionHeader1.CH0_Product = "XXX";
			commissionHeader1.CH0_Service = "XXX";
			commissionHeader1.CH0_SubModule = "XXX";
			commissionHeader1.CH0_CommissionDate = new ZDate(2002, 5, 5);

			var commissionHeader2 = Factory.New<AccCommissionHeader>();
			commissionHeader2.CH0_CA0 = agreement2.PK;
			commissionHeader2.CH0_AH_Source = invoice.PK;
			commissionHeader2.CH0_GroupingSourceID = job.PK;
			commissionHeader2.CH0_GroupingSourceTableCode = job.TablePrefix;
			commissionHeader2.CH0_OH_Customer = customer.PK;
			commissionHeader2.CH0_Product = "XXX";
			commissionHeader2.CH0_Service = "XXX";
			commissionHeader2.CH0_SubModule = "XXX";
			commissionHeader2.CH0_CommissionDate = new ZDate(2001, 10, 10);

			var context = new CreateCommissionContext();
			context.OverwriteOldValues = true;
			context.OnlyCreateForAgreementAndRatesOverrideStreams = true;
			context.AgreementsBeingApproved = new HashSet<OrgCommissionAgreement>(new[] { agreement2 });
			var builder = new CommissionHeaderBuilderForTesting(Factory, context);

			var itemArgs = new CommissionHeaderBuildItemArgs(invoice, job, customer.PK, "XXX", "XXX", "XXX", new ZDate(2002, 2, 2), new ZDate(2002, 2, 2), AccCommissionHeaderSnapshotEventList.Codes.Posted, "ALL", "", "");
			var item = new CommissionHeaderBuildItem(context, itemArgs, (x, y) => { });
			builder.UpdateExistingCommissionsAndGetCommissionTargetsToCreateFor_ExposedForTesting(item);
			AssertEquals("Should not have overriden since unrelated agreement", false, commissionHeader1.IsOverriden);
			AssertEquals("Should not have overriden since correct agreement", true, commissionHeader2.IsOverriden);
		}

		public void TestUpdateExistingMarkOldChecksCoverDate()
		{
			var customer = Factory.New<OrgHeader>();
			var invoice = Factory.New<ARInvoice>();
			var job = Factory.NewJobForTesting<JobHeader>();
			var agreement1 = Factory.New<OrgCommissionAgreement>();
			agreement1.CA0_EffectiveDate = new ZDate(2002, 1, 1);
			var agreement2 = Factory.New<OrgCommissionAgreement>();
			agreement2.CA0_EffectiveDate = new ZDate(2002, 1, 1);

			var commissionHeader1 = Factory.New<AccCommissionHeader>();
			commissionHeader1.CH0_CA0 = agreement1.PK;
			commissionHeader1.CH0_AH_Source = invoice.PK;
			commissionHeader1.CH0_GroupingSourceID = job.PK;
			commissionHeader1.CH0_GroupingSourceTableCode = job.TablePrefix;
			commissionHeader1.CH0_OH_Customer = customer.PK;
			commissionHeader1.CH0_Product = "XXX";
			commissionHeader1.CH0_Service = "XXX";
			commissionHeader1.CH0_SubModule = "XXX";
			commissionHeader1.CH0_CommissionDate = new ZDate(2002, 5, 5);

			var commissionHeader2 = Factory.New<AccCommissionHeader>();
			commissionHeader2.CH0_CA0 = agreement1.PK;
			commissionHeader2.CH0_AH_Source = invoice.PK;
			commissionHeader2.CH0_GroupingSourceID = job.PK;
			commissionHeader2.CH0_GroupingSourceTableCode = job.TablePrefix;
			commissionHeader2.CH0_OH_Customer = customer.PK;
			commissionHeader2.CH0_Product = "YYY";
			commissionHeader2.CH0_Service = "XXX";
			commissionHeader2.CH0_SubModule = "XXX";
			commissionHeader2.CH0_CommissionDate = new ZDate(2001, 10, 10);

			var context = new CreateCommissionContext();
			context.OverwriteOldValues = true;
			var agreementAndRates = new CommissionAgreementAndRatesForTesting();
			agreementAndRates.CommissionAgreement = agreement2;
			context.AgreementAndRatesOverride = new Dictionary<ZString, ICommissionAgreementAndRates>() { { ZString.Empty, agreementAndRates } };
			context.OnlyCreateForAgreementAndRatesOverrideStreams = true;
			context.AgreementsBeingApproved = new HashSet<OrgCommissionAgreement>(new[] { agreement1, agreement2 });
			var builder = new CommissionHeaderBuilderForTesting(Factory, context);

			var itemArgs = new CommissionHeaderBuildItemArgs(invoice, job, customer.PK, "XXX", "XXX", "XXX", new ZDate(2002, 2, 2), new ZDate(2002, 2, 2), AccCommissionHeaderSnapshotEventList.Codes.Posted, "ALL", "", "");
			var item = new CommissionHeaderBuildItem(context, itemArgs, (x, y) => { });
			builder.UpdateExistingCommissionsAndGetCommissionTargetsToCreateFor_ExposedForTesting(item);
			AssertEquals("Should have overriden since covers date", true, commissionHeader1.IsOverriden);
			AssertEquals("Should not have overriden since different Product", false, commissionHeader2.IsOverriden);

			itemArgs = new CommissionHeaderBuildItemArgs(invoice, job, customer.PK, "YYY", "XXX", "XXX", new ZDate(2002, 2, 2), new ZDate(2002, 2, 2), AccCommissionHeaderSnapshotEventList.Codes.Posted, "ALL", "", "");
			item = new CommissionHeaderBuildItem(context, itemArgs, (x, y) => { });
			agreementAndRates.CommissionAgreement = agreement1;
			builder.UpdateExistingCommissionsAndGetCommissionTargetsToCreateFor_ExposedForTesting(item);
			AssertEquals("Should have overriden since different Product", true, commissionHeader1.IsOverriden);
			AssertEquals("Should not have overriden since not covers date", false, commissionHeader2.IsOverriden);
		}

		public void TestUpdateExistingMarkOldChecksCoverDate_NonManualEffectiveDate()
		{
			var customer = Factory.New<OrgHeader>();
			var invoice = Factory.New<ARInvoice>();
			var job = Factory.NewJobForTesting<JobHeader>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MiscServ.OM_CMClientCommenced = new ZDate(2000, 1, 1);

			var agreement1 = Factory.New<OrgCommissionAgreement>();
			agreement1.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.ClientCommencedDate;
			agreement1.CA0_OH_Customer = org.PK;

			var agreement2 = Factory.New<OrgCommissionAgreement>();
			agreement2.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.ClientCommencedDate;
			agreement2.CA0_OH_Customer = org.PK;

			var commissionHeader1 = Factory.New<AccCommissionHeader>();
			commissionHeader1.CH0_CA0 = agreement1.PK;
			commissionHeader1.CH0_AH_Source = invoice.PK;
			commissionHeader1.CH0_GroupingSourceID = job.PK;
			commissionHeader1.CH0_GroupingSourceTableCode = job.TablePrefix;
			commissionHeader1.CH0_OH_Customer = customer.PK;
			commissionHeader1.CH0_Product = "XXX";
			commissionHeader1.CH0_Service = "XXX";
			commissionHeader1.CH0_SubModule = "XXX";
			commissionHeader1.CH0_CommissionDate = new ZDate(2002, 5, 5);

			var commissionHeader2 = Factory.New<AccCommissionHeader>();
			commissionHeader2.CH0_CA0 = agreement1.PK;
			commissionHeader2.CH0_AH_Source = invoice.PK;
			commissionHeader2.CH0_GroupingSourceID = job.PK;
			commissionHeader2.CH0_GroupingSourceTableCode = job.TablePrefix;
			commissionHeader2.CH0_OH_Customer = customer.PK;
			commissionHeader2.CH0_Product = "YYY";
			commissionHeader2.CH0_Service = "XXX";
			commissionHeader2.CH0_SubModule = "XXX";
			commissionHeader2.CH0_CommissionDate = new ZDate(2001, 10, 10);

			var context = new CreateCommissionContext();
			context.OverwriteOldValues = true;
			var agreementAndRates = new CommissionAgreementAndRatesForTesting();
			agreementAndRates.CommissionAgreement = agreement2;
			context.AgreementAndRatesOverride = new Dictionary<ZString, ICommissionAgreementAndRates>() { { ZString.Empty, agreementAndRates } };
			context.OnlyCreateForAgreementAndRatesOverrideStreams = true;
			context.AgreementsBeingApproved = new HashSet<OrgCommissionAgreement>(new[] { agreement1, agreement2 });
			var builder = new CommissionHeaderBuilderForTesting(Factory, context);

			var itemArgs = new CommissionHeaderBuildItemArgs(invoice, job, customer.PK, "XXX", "XXX", "XXX", new ZDate(2002, 2, 2), new ZDate(2002, 2, 2), AccCommissionHeaderSnapshotEventList.Codes.Posted, "ALL", "", "");
			var item = new CommissionHeaderBuildItem(context, itemArgs, (x, y) => { });
			builder.UpdateExistingCommissionsAndGetCommissionTargetsToCreateFor_ExposedForTesting(item);
			AssertEquals("Should have overriden since no effective date", true, commissionHeader1.IsOverriden);
			AssertEquals("Should not have overriden since different Product", false, commissionHeader2.IsOverriden);

			itemArgs = new CommissionHeaderBuildItemArgs(invoice, job, customer.PK, "YYY", "XXX", "XXX", new ZDate(2002, 2, 2), new ZDate(2002, 2, 2), AccCommissionHeaderSnapshotEventList.Codes.Posted, "ALL", "", "");
			item = new CommissionHeaderBuildItem(context, itemArgs, (x, y) => { });
			context.AgreementsBeingApproved = new HashSet<OrgCommissionAgreement>(new[] { agreement2 });
			builder = new CommissionHeaderBuilderForTesting(Factory, context);
			builder.UpdateExistingCommissionsAndGetCommissionTargetsToCreateFor_ExposedForTesting(item);
			AssertEquals("Should have overriden since different Product", true, commissionHeader1.IsOverriden);
			AssertEquals("Should have overriden since effective date is client commenced", true, commissionHeader2.IsOverriden);
		}

		#endregion

		#region Finalize

		public void TestCreate_ShouldNotCreateHeaderWhenNoLines()
		{
			var transactionHeader = Factory.NewWithValidTestData<ARInvoice>();
			var builder = new CommissionHeaderBuilderForTesting(Factory, null);

			var headerWithoutLines = Factory.NewWithValidTestData<AccCommissionHeader>();
			builder.Finalize_ExposedForTesting(headerWithoutLines);
			AssertEquals("Should have deleted header as there are no lines nor line groups", true, headerWithoutLines.IsDeleted);

			var headerWithLines = Factory.NewWithValidTestData<AccCommissionHeader>();
			var line = headerWithLines.Lines.AddNew();
			builder.Finalize_ExposedForTesting(headerWithLines);
			AssertEquals("Should not have deleted header as is has lines", false, headerWithLines.IsDeleted);
		}

		public void TestCreate_ShouldDeleteWhenAllLineGroupsHaveNoLines()
		{
			var transactionHeader = Factory.NewWithValidTestData<ARInvoice>();
			var builder = new CommissionHeaderBuilderForTesting(Factory, null);

			var headerWithEmptyLineGroups = Factory.NewWithValidTestData<AccCommissionHeader>();
			var emtpyLineGroup = headerWithEmptyLineGroups.LineGroups.AddNew();
			builder.Finalize_ExposedForTesting(headerWithEmptyLineGroups);
			AssertEquals("Should have deleted header as there are no lines nor line groups", true, headerWithEmptyLineGroups.IsDeleted);

			var headerWithNonEmptyLineGroup = Factory.NewWithValidTestData<AccCommissionHeader>();
			var lineGroup = headerWithNonEmptyLineGroup.LineGroups.AddNew();
			var line = lineGroup.Lines.AddNew();
			builder.Finalize_ExposedForTesting(headerWithNonEmptyLineGroup);
			AssertEquals("Should not have deleted header as is has lines", false, headerWithNonEmptyLineGroup.IsDeleted);
		}

		[TestDate(2000, 1, 1)]
		public void TestCreate_ShouldUpdateAgreementFirstUsageDate()
		{
			var agreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			Factory.Save();

			var transactionHeader = Factory.NewWithValidTestData<ARInvoice>();
			var builder = new CommissionHeaderBuilderForTesting(Factory, null);

			var headerWithLinesA = Factory.NewWithValidTestData<AccCommissionHeader>();
			headerWithLinesA.CH0_CA0 = agreement.PK;
			var lineA = headerWithLinesA.Lines.AddNew();
			builder.Finalize_ExposedForTesting(headerWithLinesA);
			AssertEquals("Should have updated the first usage date", new ZDateTime(2000, 1, 1), agreement.CA0_FirstUsageDateUtc);

			TestDateAttribute.Date = new DateTime(2022, 2, 2);

			var headerWithLinesB = Factory.NewWithValidTestData<AccCommissionHeader>();
			headerWithLinesB.CH0_CA0 = agreement.PK;
			var lineB = headerWithLinesB.Lines.AddNew();
			builder.Finalize_ExposedForTesting(headerWithLinesB);
			AssertEquals("Should not have updated the first usage date, because already exists", new ZDateTime(2000, 1, 1), agreement.CA0_FirstUsageDateUtc);
		}

		public void TestUpdateExistingCommissionsAndGetCommissionTargetsToCreateFor_ReturnsCorrectTargetsWhenRegenerating()
		{
			var testHelper = new CommissionTestObjectCreator(Factory);
			testHelper.SetupTransactionsAndCommissions(closeJob: true);

			testHelper.Job.Close(null, null);

			var context = new CreateCommissionContext();
			context.RegeneratingCommissions = true;
			var builder = new CommissionHeaderBuilderForTesting(Factory, context);

			var itemArgs = new CommissionHeaderBuildItemArgs(testHelper.JobInvoice, testHelper.Job, testHelper.Org.PK, "SHP", "ALL", "ALL", ZDate.Today, ZDate.Today, AccCommissionHeaderSnapshotEventList.Codes.Posted, "SEA", "", "");
			var item = new CommissionHeaderBuildItem(context, itemArgs, (x, y) => { });
			var result = builder.UpdateExistingCommissionsAndGetCommissionTargetsToCreateFor_ExposedForTesting(item);
			AssertEquals("1 Commission Header should have been returned.", 1, result.Count);

			itemArgs = new CommissionHeaderBuildItemArgs(testHelper.Invoice, testHelper.Invoice, testHelper.Org.PK, "ALL", "ALL", "ALL", ZDate.Today, ZDate.Today, AccCommissionHeaderSnapshotEventList.Codes.Posted, "SEA", "", "");
			item = new CommissionHeaderBuildItem(context, itemArgs, (x, y) => { });
			result = builder.UpdateExistingCommissionsAndGetCommissionTargetsToCreateFor_ExposedForTesting(item);
			AssertEquals("1 Commission Header should have been returned.", 1, result.Count);
		}

		#endregion
	}

	#region Classes

	class CommissionHeaderBuilderForTesting : CommissionHeaderBuilder
	{
		public CommissionHeaderBuilderForTesting(BusinessObjectFactory factory, CreateCommissionContext context, Func<ICommissionAgreementAndRates, bool> additionalPreCreateCheck = null)
			: base(factory, context, additionalPreCreateCheck)
		{
		}

		public Dictionary<ZString, ICommissionAgreementAndRates> UpdateExistingCommissionsAndGetCommissionTargetsToCreateFor_ExposedForTesting(ICommissionHeaderBuildItem headerBuilder)
		{
			return UpdateExistingCommissionsAndGetCommissionTargetsToCreateFor(headerBuilder);
		}

		public void Finalize_ExposedForTesting(AccCommissionHeader commissionHeader)
		{
			Finalize(commissionHeader);
		}
	}

	#endregion
}
