using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.CommissionManagement.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.CommissionManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	class BillingCommissionHeaderBuildItemTest : TestCaseWithFactory
	{
		public void TestGetExistingCommissionHeaders()
		{
			var ent = Factory.New<LicenceEnterprise>();
			var licDatabase1 = ent.Databases.AddNew();
			var licDatabase2 = ent.Databases.AddNew();
			var company = ent.Companies.AddNew();
			var org = Factory.New<OrgHeader>();
			company.LC_OH = org.PK;
			var clientCompany1A = Factory.New<ClientCompany>();
			clientCompany1A.LCC_LD = licDatabase1.PK;
			var clientCompany1B = Factory.New<ClientCompany>();
			clientCompany1B.LCC_LD = licDatabase1.PK;

			var invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = org.PK;

			var commissionHeader1A = Factory.New<EdiCommissionHeader>();
			commissionHeader1A.CH0_AH_Source = invoice.PK;
			commissionHeader1A.CH0_GroupingSourceID = invoice.PK;
			commissionHeader1A.CH0_GroupingSourceTableCode = invoice.TablePrefix;
			commissionHeader1A.CH0_OH_Customer = org.PK;
			commissionHeader1A.CH0_Product = "AAA";
			commissionHeader1A.CH0_Service = "BBB";
			commissionHeader1A.CH0_SubModule = "CCC";
			var additionalInfo1A = commissionHeader1A.GetOrCreateAdditionalInfo();
			additionalInfo1A.ECH_LCC = clientCompany1A.PK;
			additionalInfo1A.ECH_LD = licDatabase1.PK;

			var commissionHeader1B = Factory.New<EdiCommissionHeader>();
			commissionHeader1B.CH0_AH_Source = invoice.PK;
			commissionHeader1B.CH0_GroupingSourceID = invoice.PK;
			commissionHeader1B.CH0_GroupingSourceTableCode = invoice.TablePrefix;
			commissionHeader1B.CH0_OH_Customer = org.PK;
			commissionHeader1B.CH0_Product = "AAA";
			commissionHeader1B.CH0_Service = "BBB";
			commissionHeader1B.CH0_SubModule = "CCC";
			var additionalInfo1B = commissionHeader1B.GetOrCreateAdditionalInfo();
			additionalInfo1B.ECH_LCC = clientCompany1B.PK;
			additionalInfo1B.ECH_LD = licDatabase1.PK;

			var commissionHeader2 = Factory.New<EdiCommissionHeader>();
			commissionHeader2.CH0_AH_Source = invoice.PK;
			commissionHeader2.CH0_GroupingSourceID = invoice.PK;
			commissionHeader2.CH0_GroupingSourceTableCode = invoice.TablePrefix;
			commissionHeader2.CH0_OH_Customer = org.PK;
			commissionHeader2.CH0_Product = "AAA";
			commissionHeader2.CH0_Service = "BBB";
			commissionHeader2.CH0_SubModule = "CCC";
			var additionalInfo2 = commissionHeader2.GetOrCreateAdditionalInfo();
			additionalInfo2.ECH_LCC = ZGuid.Empty;
			additionalInfo2.ECH_LD = licDatabase2.PK;

			var commissionHeader = Factory.New<EdiCommissionHeader>();
			commissionHeader.CH0_AH_Source = invoice.PK;
			commissionHeader.CH0_GroupingSourceID = invoice.PK;
			commissionHeader.CH0_GroupingSourceTableCode = invoice.TablePrefix;
			commissionHeader.CH0_OH_Customer = org.PK;
			commissionHeader.CH0_Product = "AAA";
			commissionHeader.CH0_Service = "BBB";
			commissionHeader.CH0_SubModule = "CCC";

			var responsibleAgreements = new Dictionary<(string Stream, ZGuid PivotPk), EdiCommissionAgreement>();

			var args = new BillingCommissionHeaderBuildItemArgs(invoice, invoice, org.PK, clientCompany1A.PK, licDatabase1.PK, "AAA", "BBB", "CCC", new ZDate(2002, 2, 2), new ZDateTime(2002, 3, 3), AccCommissionHeaderSnapshotEventList.Codes.Posted);
			var buildItem = new BillingCommissionHeaderBuildItem(new CreateCommissionContext(), args, responsibleAgreements, (x, y) => { });
			AssertContainsExactElementsInAnyOrder(
				new[] { commissionHeader1A },
				buildItem.GetExistingCommissionHeaders());

			args = new BillingCommissionHeaderBuildItemArgs(invoice, invoice, org.PK, clientCompany1B.PK, licDatabase1.PK, "AAA", "BBB", "CCC", new ZDate(2002, 2, 2), new ZDateTime(2002, 3, 3), AccCommissionHeaderSnapshotEventList.Codes.Posted);
			buildItem = new BillingCommissionHeaderBuildItem(new CreateCommissionContext(), args, responsibleAgreements, (x, y) => { });
			AssertContainsExactElementsInAnyOrder(
				new[] { commissionHeader1B },
				buildItem.GetExistingCommissionHeaders());

			args = new BillingCommissionHeaderBuildItemArgs(invoice, invoice, org.PK, ZGuid.Empty, ZGuid.Empty, "AAA", "BBB", "CCC", new ZDate(2002, 2, 2), new ZDateTime(2002, 3, 3), AccCommissionHeaderSnapshotEventList.Codes.Posted);
			buildItem = new BillingCommissionHeaderBuildItem(new CreateCommissionContext(), args, responsibleAgreements, (x, y) => { });
			AssertContainsExactElementsInAnyOrder(
				new[] { commissionHeader },
				buildItem.GetExistingCommissionHeaders());

			args = new BillingCommissionHeaderBuildItemArgs(invoice, invoice, org.PK, ZGuid.Empty, ZGuid.Empty, "ZZZ", "ZZZ", "ZZZ", new ZDate(2002, 2, 2), new ZDateTime(2002, 3, 3), AccCommissionHeaderSnapshotEventList.Codes.Posted);
			buildItem = new BillingCommissionHeaderBuildItem(new CreateCommissionContext(), args, responsibleAgreements, (x, y) => { });
			AssertContainsExactElementsInAnyOrder(
				System.Array.Empty<EdiCommissionHeader>(),
				buildItem.GetExistingCommissionHeaders());
		}

		public void TestNewCommissionHeaderWithLineGroups()
		{
			var ent = Factory.New<LicenceEnterprise>();
			var licDatabase = ent.Databases.AddNew();
			var company = ent.Companies.AddNew();
			var org = Factory.New<OrgHeader>();
			company.LC_OH = org.PK;
			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_LD = licDatabase.PK;

			var commissionAgreement = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreementForAllItems(Factory, org);
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(commissionAgreement, "ADL", 10m);

			var responsibleAgreements = new Dictionary<(string Stream, ZGuid PivotPk), EdiCommissionAgreement>();

			var invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = org.PK;
			var args = new BillingCommissionHeaderBuildItemArgs(invoice, invoice, org.PK, clientCompany.PK, licDatabase.PK, "AAA", "BBB", "CCC", new ZDate(2002, 2, 2), new ZDateTime(2002, 3, 3), AccCommissionHeaderSnapshotEventList.Codes.Posted);
			var context = new CreateCommissionContext();
			var buildItem = new BillingCommissionHeaderBuildItem(context, args, responsibleAgreements,
				(header, rates) =>
					{
						var lineGroup = header.LineGroups.AddNew();
						lineGroup.CLG_TransactionAmount = 100;
						lineGroup.CLG_TotalCommissionableAmount = 100;
					});

			var agreementAndRates = new CommissionAgreementAndRates(commissionAgreement, new ZDate(2002, 2, 2));
			var commissionHeader = (EdiCommissionHeader)buildItem.NewCommissionHeaderWithLineGroups(new KeyValuePair<ZString, ICommissionAgreementAndRates>("WBP", agreementAndRates));
			CombineAssertions(() =>
				{
					AssertEquals("CH0_AH_Source", invoice.PK, commissionHeader.CH0_AH_Source);
					AssertEquals("CH0_GC", invoice.AH_GC, commissionHeader.CH0_GC);
					AssertEquals("CH0_GroupingSourceID", invoice.PK, commissionHeader.CH0_GroupingSourceID);
					AssertEquals("CH0_GroupingSourceTableCode", invoice.TablePrefix, commissionHeader.CH0_GroupingSourceTableCode);
					AssertEquals("CH0_CommissionStream", "WBP", commissionHeader.CH0_CommissionStream);
					AssertEquals("CH0_CA0", commissionAgreement.PK, commissionHeader.CH0_CA0);
					AssertEquals("CH0_OH_Customer", org.PK, commissionHeader.CH0_OH_Customer);
					AssertEquals("CH0_OH_Debtor", org.PK, commissionHeader.CH0_OH_Debtor);
					AssertEquals("CH0_Product", "AAA", commissionHeader.CH0_Product);
					AssertEquals("CH0_Service", "BBB", commissionHeader.CH0_Service);
					AssertEquals("CH0_SubModule", "CCC", commissionHeader.CH0_SubModule);
					AssertEquals("CH0_CommissionDate", new ZDate(2002, 2, 2), commissionHeader.CH0_CommissionDate);
					AssertEquals("CH0_SnapshotDateTime", new ZDateTime(2002, 3, 3), commissionHeader.CH0_SnapshotDateTime);
					AssertEquals("CH0_SnapshotEventCode", AccCommissionHeaderSnapshotEventList.Codes.Posted, commissionHeader.CH0_SnapshotEventCode);

					var additionalInfo = commissionHeader.AdditionalInfo;
					AssertNotNull("AdditionalInfo", additionalInfo);
					if (additionalInfo != null)
					{
						AssertEquals("ECH_LCC", clientCompany.PK, additionalInfo.ECH_LCC);
						AssertEquals("ECH_LD", licDatabase.PK, additionalInfo.ECH_LD);
					}
				});
		}
	}
}
