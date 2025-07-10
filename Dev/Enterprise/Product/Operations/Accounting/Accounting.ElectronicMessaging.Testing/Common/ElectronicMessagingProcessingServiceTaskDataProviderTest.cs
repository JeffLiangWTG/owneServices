using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	internal class ElectronicMessagingProcessingServiceTaskDataProviderTest : TestCaseWithFactory
	{
		public virtual ElectronicMessagingProcessingServiceTaskDataProvider GetProvider() => new ElectronicMessagingProcessingServiceTaskDataProvider();

		#region Test GetPKsOfCompaniesWithQueuedTransactions()

		public void TestGetPKsOfCompaniesWithQueuedTransactions_WithZeroQueuedTransactions()
		{
			var nonCurrentCompanyPK = Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, Env.CurrentCompanyPK))[0].PK.ToGuid();
			AssertNotEquals("Precondition", nonCurrentCompanyPK, Env.CurrentCompanyPK);

			InsertAccEInvoicingTransactionPivots(1, Env.CurrentCompanyPK, "AA", status: EInvoicingPivotState.Batched);
			InsertAccEInvoicingTransactionPivots(1, nonCurrentCompanyPK, "AA", status: EInvoicingPivotState.BatchedWithError);
			InsertAccEInvoicingTransactionPivots(1, Env.CurrentCompanyPK, "AA", status: EInvoicingPivotState.Delivered);
			InsertAccEInvoicingTransactionPivots(1, Env.CurrentCompanyPK, "AA", status: EInvoicingPivotState.Discarded);
			InsertAccEInvoicingTransactionPivots(1, Env.CurrentCompanyPK, "AA", status: EInvoicingPivotState.Failed);
			InsertAccEInvoicingTransactionPivots(1, Env.CurrentCompanyPK, "AA", status: EInvoicingPivotState.Pending);
			InsertAccEInvoicingTransactionPivots(1, Env.CurrentCompanyPK, "AA", status: EInvoicingPivotState.Sent);
			InsertAccEInvoicingTransactionPivots(1, Env.CurrentCompanyPK, "AA", status: EInvoicingPivotState.Succeed);

			var companyPKs = GetProvider().GetPKsOfCompaniesWithQueuedTransactions("AA");

			AssertContainsExactElementsInAnyOrder("No PKs because no Queued status pivots", Array.Empty<ZGuid>(), companyPKs);
		}

		public void TestGetPKsOfCompaniesWithQueuedTransactions_WithFiveQueuedTransactionsInOneCompany()
		{
			var nonCurrentCompanyPK = Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, Env.CurrentCompanyPK))[0].PK.ToGuid();
			AssertNotEquals("Precondition", nonCurrentCompanyPK, Env.CurrentCompanyPK);

			InsertAccEInvoicingTransactionPivots(5, Env.CurrentCompanyPK, "AA", status: EInvoicingPivotState.Queued);
			InsertAccEInvoicingTransactionPivots(5, nonCurrentCompanyPK, "BB", status: EInvoicingPivotState.Queued);

			var companyPKs = GetProvider().GetPKsOfCompaniesWithQueuedTransactions("AA");

			AssertContainsExactElementsInAnyOrder("One company for country code AA", new ZGuid[] { Env.CurrentCompanyPK }, companyPKs);
		}

		public void TestGetPKsOfCompaniesWithQueuedTransactions_WithFiveQueuedTransactionsInTwoCompanies()
		{
			var nonCurrentCompanyPK = Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, Env.CurrentCompanyPK))[0].PK.ToGuid();
			AssertNotEquals("Precondition", nonCurrentCompanyPK, Env.CurrentCompanyPK);

			InsertAccEInvoicingTransactionPivots(5, Env.CurrentCompanyPK, "AA", status: EInvoicingPivotState.Queued);
			InsertAccEInvoicingTransactionPivots(5, nonCurrentCompanyPK, "AA", status: EInvoicingPivotState.Queued);

			var companyPKs = GetProvider().GetPKsOfCompaniesWithQueuedTransactions("AA");

			AssertContainsExactElementsInAnyOrder("Two companies for country code AA", new ZGuid[] { Env.CurrentCompanyPK, nonCurrentCompanyPK }, companyPKs);
		}

		public void TestGetPKsOfCompaniesWithQueuedTransactions_WithFiveQueuedComplianceDocsInCountry()
		{
			var nonCurrentCompanyPK = Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, Env.CurrentCompanyPK))[0].PK.ToGuid();
			AssertNotEquals("Precondition", nonCurrentCompanyPK, Env.CurrentCompanyPK);

			InsertAccEInvoicingTransactionPivots(5, Env.CurrentCompanyPK, "AA", status: EInvoicingPivotState.Queued, parentTableCode: "ADH");
			InsertAccEInvoicingTransactionPivots(5, nonCurrentCompanyPK, "BB", status: EInvoicingPivotState.Queued, parentTableCode: "ADH");

			var companyPKs = GetProvider().GetPKsOfCompaniesWithQueuedTransactions("AA");

			AssertContainsExactElementsInAnyOrder("One company for country code AA; parent table code makes no difference to query", new ZGuid[] { Env.CurrentCompanyPK }, companyPKs);
		}

		public void TestGetPKsOfCompaniesWithQueuedTransactions_UsesDatabaseIndex()
		{
			var manyCountries = Country.LicenceKeyBuilderSupportedCountryCodes.Take(20);
			var companies = new List<GlbCompany>();
			foreach (var countryCode in manyCountries)
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_Code = "~" + countryCode;
				company.GC_RN_NKCountryCode = countryCode;
				companies.Add(company);
			}
			Factory.Save();

			foreach (var company in companies)
			{
				// 20 * 50 = 1000 records in non-QUE status.
				InsertAccEInvoicingTransactionPivots(50, company.PK.ToGuid(), company.GC_RN_NKCountryCode, status: EInvoicingPivotState.Succeed);
			}

			var randomCompany = companies.OrderBy(c => c.PK).First();
			InsertAccEInvoicingTransactionPivots(2, randomCompany.PK.ToGuid(), randomCompany.GC_RN_NKCountryCode, status: EInvoicingPivotState.Queued);

			TestConnection.ExecuteNonQuery($"UPDATE STATISTICS AccEInvoicingTransactionPivot WITH FULLSCAN");

			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538
			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				var companyPKs = GetProvider().GetPKsOfCompaniesWithQueuedTransactions(randomCompany.GC_RN_NKCountryCode);

				AssertContainsExactElementsInAnyOrder("One company has Queued pivot", new ZGuid[] { randomCompany.PK }, companyPKs);

				var plans = TestConnection.ExecutedCommandsAndQueryPlans.Single(t => t.Item1.Contains(AccEInvoicingTransactionPivotSchema.Constants.TableName));
				var planalyzer = new QueryPlanalyzer(plans.Item2.Single());
				CombineAssertions("Index NR_RX__AIP_RN_NKCountryCode_AIP_GC should be used for finding Queued pivots", () =>
				{
					AssertContainsExactElementsInAnyOrder(
						Enumerable.Empty<string>(),
						planalyzer.TableScans.Select(x => x.TableName)
					);

					AssertContainsExactElementsInAnyOrder(
						Enumerable.Empty<string>(),
						planalyzer.IndexScans.Select(x => x.TableName)
					);

					AssertContainsExactElementsInAnyOrder(
						new[] { AccEInvoicingTransactionPivotSchema.Constants.Indexes.NR_RX__AIP_RN_NKCountryCode_AIP_GC },
						planalyzer.IndexSeeks.Select(x => x.IndexName)
					);
				});
			}
		}

		#endregion

		#region Test GetPKsOfCompaniesThatEnabledEInvoicing()

		public void TestGetPKsOfCompaniesThatEnabledEInvoicing_WithZeroEnabledCompanies()
		{
			var companyAA = Factory.NewWithValidTestData<GlbCompany>();
			companyAA.GC_Code = "~AA";
			companyAA.GC_RN_NKCountryCode = "AA";

			var companyBB = Factory.NewWithValidTestData<GlbCompany>();
			companyBB.GC_Code = "~BB";
			companyBB.GC_RN_NKCountryCode = "BB";

			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(companyAA.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(companyBB.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var companyPKs = GetProvider().GetPKsOfCompaniesThatEnabledEInvoicing("AA");

				AssertContainsExactElementsInAnyOrder("No companies are enabled for AA country code", Array.Empty<ZGuid>(), companyPKs);
			}
		}

		public void TestGetPKsOfCompaniesThatEnabledEInvoicing_WithTwoEnabledCompanies()
		{
			var companyA1 = Factory.NewWithValidTestData<GlbCompany>();
			companyA1.GC_Code = "~A1";
			companyA1.GC_RN_NKCountryCode = "AA";

			var companyA2 = Factory.NewWithValidTestData<GlbCompany>();
			companyA2.GC_Code = "~A2";
			companyA2.GC_RN_NKCountryCode = "AA";

			var companyBB = Factory.NewWithValidTestData<GlbCompany>();
			companyBB.GC_Code = "~BB";
			companyBB.GC_RN_NKCountryCode = "BB";

			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(companyA1.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(companyA2.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(companyBB.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var companyPKs = GetProvider().GetPKsOfCompaniesThatEnabledEInvoicing("AA");

				AssertContainsExactElementsInAnyOrder("Two companies are enabled for AA country code", new ZGuid[] { companyA1.PK, companyA2.PK }, companyPKs);
			}
		}

		public void TestGetPKsOfCompaniesThatEnabledEInvoicing_AlsoForPayables()
		{
			var companyA1 = Factory.NewWithValidTestData<GlbCompany>();
			companyA1.GC_Code = "~A1";
			companyA1.GC_RN_NKCountryCode = "AA";

			var companyA2 = Factory.NewWithValidTestData<GlbCompany>();
			companyA2.GC_Code = "~A2";
			companyA2.GC_RN_NKCountryCode = "AA";

			var companyBB = Factory.NewWithValidTestData<GlbCompany>();
			companyBB.GC_Code = "~BB";
			companyBB.GC_RN_NKCountryCode = "BB";

			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(companyA1.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(companyA2.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(companyBB.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(companyBB.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var companyPKs = GetProvider().GetPKsOfCompaniesThatEnabledEInvoicing("AA");
				AssertContainsExactElementsInAnyOrder("Two companies are enabled for AA country code", new ZGuid[] { companyA1.PK, companyA2.PK }, companyPKs);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(companyA1.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(companyA2.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(companyA2.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(companyBB.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(companyBB.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var companyPKs = GetProvider().GetPKsOfCompaniesThatEnabledEInvoicing("AA");
				AssertContainsExactElementsInAnyOrder("Only one company is enabled for AA country code", new ZGuid[] { companyA2.PK }, companyPKs);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(companyA1.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(companyA2.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(companyA1.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(companyA2.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(companyBB.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(companyBB.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var companyPKs = GetProvider().GetPKsOfCompaniesThatEnabledEInvoicing("AA");
				AssertContainsExactElementsInAnyOrder("Two companies are enabled for AA country code", new ZGuid[] { companyA1.PK, companyA2.PK }, companyPKs);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(companyA1.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(companyA2.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(companyA1.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(companyA2.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(companyBB.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(companyBB.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var dataProvider = GetProvider();
				var companyPKs = dataProvider.GetPKsOfCompaniesThatEnabledEInvoicing("AA");
				AssertContainsExactElementsInAnyOrder("No company is enabled for AA country code", Array.Empty<ZGuid>(), companyPKs);

				companyPKs = dataProvider.GetPKsOfCompaniesThatEnabledEInvoicing("BB");
				AssertContainsExactElementsInAnyOrder("Only one company is enabled for BB country code", new ZGuid[] { companyBB.PK }, companyPKs);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(companyA1.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(companyA2.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(companyA1.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(companyA2.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(companyBB.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(companyBB.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var dataProvider = GetProvider();
				var companyPKs = dataProvider.GetPKsOfCompaniesThatEnabledEInvoicing("AA");
				AssertContainsExactElementsInAnyOrder("Two companies are enabled for AA country code", new ZGuid[] { companyA1.PK, companyA2.PK }, companyPKs);

				companyPKs = dataProvider.GetPKsOfCompaniesThatEnabledEInvoicing("BB");
				AssertContainsExactElementsInAnyOrder("Only one company is enabled for BB country code", new ZGuid[] { companyBB.PK }, companyPKs);
			}
		}

		#endregion

		#region Helpers

		void InsertAccEInvoicingTransactionPivots(int count, Guid companyPK, string countryCode, string parentTableCode = "AH", string status = "QUE")
		{
			for (int i = 0; i < count; i++)
			{
				var sQL = @"INSERT INTO dbo.AccEInvoicingTransactionPivot (AIP_PK, AIP_GC, AIP_RN_NKCountryCode, AIP_ParentID, AIP_ParentTableCode, AIP_Status, AIP_ActionType)
								VALUES (@PK, @Company, @CountryCode, @ParentID, @ParentTableCode, @Status, @ActionType)";
				using (var cmd = TestConnection.Command(sQL))
				{
					cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
					cmd.AddParameter("@Company", SqlDbType.UniqueIdentifier, companyPK);
					cmd.AddParameterBasedOnDbColumn("@CountryCode", countryCode, AccEInvoicingTransactionPivotSchema.AIP_RN_NKCountryCode);
					cmd.AddParameter("@ParentID", SqlDbType.UniqueIdentifier, Guid.NewGuid());
					cmd.AddParameterBasedOnDbColumn("@ParentTableCode", parentTableCode, AccEInvoicingTransactionPivotSchema.AIP_ParentTableCode);
					cmd.AddParameterBasedOnDbColumn("@Status", status, AccEInvoicingTransactionPivotSchema.AIP_Status);
					cmd.AddParameterBasedOnDbColumn("@ActionType", EInvoicingPivotActionType.Submit, AccEInvoicingTransactionPivotSchema.AIP_ActionType);
					cmd.ExecuteNonQuery();
				}
			}
		}

		#endregion
	}
}
