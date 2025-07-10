using System.Reflection;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.JobRequiredDocument;

namespace Enterprise.Accounting.Export.Business.Testing
{
	class JobRequiredDocumentLookupsTest : TestCaseWithFactory
	{
		public void TestAllCategoryType_List()
		{
			var jobRequiredDocument = Factory.NewWithValidTestData<JobRequiredDocument>();
			var lookup = new MasterFiles.Business.JobRequiredDocumentLookups(jobRequiredDocument);
			var allCategoriesInMasterfiles = lookup.AllCategoryType_List_ForTest;
			foreach (CodeDescriptionPair categoryInMasterfiles in allCategoriesInMasterfiles)
			{
				var categoryDescriptionInAccounting = Enterprise.Accounting.Export.Business.JobRequiredDocumentLookups.AllCategoryType_List.GetDescriptionFromCode(categoryInMasterfiles.Code);
				Assert(string.Format("Category code {0} missing in Enterprise.Accounting.Export.Business.JobRequiredDocumentLookups.AllCategoryType_List. Please add this code.", categoryInMasterfiles.Code), !string.IsNullOrWhiteSpace(categoryDescriptionInAccounting));
				AssertEquals(string.Format("Description for code {0} is different in Enterprise.Accounting.Export.Business.JobRequiredDocumentLookups.AllCategoryType_List than in Masterfiles.Business.JobRequiredDocumentLookups.AllCategoryType_List. Please update the description.", categoryInMasterfiles.Code), categoryInMasterfiles.Description, categoryDescriptionInAccounting);
			}

			var allCategoriesInAccounting = Enterprise.Accounting.Export.Business.JobRequiredDocumentLookups.AllCategoryType_List;

			AssertEquals("Number of codes in Masterfiles.Business.JobRequiredDocumentLookups.AllCategoryType_List is less than codes in Enterprise.Accounting.Export.Business.JobRequiredDocumentLookups.AllCategoryType_List. Both these lists should be the same.", allCategoriesInMasterfiles.Count, allCategoriesInAccounting.Count);
		}

		public void TestDocUsage_List()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var jobRequiredDocument = Factory.NewWithValidTestData<JobRequiredDocument>();

			var fields = typeof(DocUsage).GetFields(BindingFlags.Public | BindingFlags.Static);
			foreach (var field in fields)
			{
				var usageCodeInMasterfiles = (string)field.GetValue(null);

				var usageDescriptionInAccounting = Enterprise.Accounting.Export.Business.JobRequiredDocumentLookups.DocUsage_List.GetDescriptionFromCode(usageCodeInMasterfiles);
				Assert(string.Format("Usage code {0} missing in Enterprise.Accounting.Export.Business.JobRequiredDocumentLookups.DocUsage_List. Please add this code.", usageCodeInMasterfiles), !string.IsNullOrWhiteSpace(usageDescriptionInAccounting));

				jobRequiredDocument.ParentType = typeof(OrgHeader);
				jobRequiredDocument.EQ_ParentID = testObjectCreator.ABIGAS.PK;
				jobRequiredDocument.EQ_ParentTableCode = "OH";

				switch (usageCodeInMasterfiles)
				{
					case DocUsage.Broker:
					case DocUsage.Carrier:
					case DocUsage.Competitor:
					case DocUsage.Creditor:
					case DocUsage.Debtor:
					case DocUsage.ForwarderAgent:
					case DocUsage.ImporterConsignee:
					case DocUsage.Services:
					case DocUsage.SupplierConsignor:
					case DocUsage.TransportClient:
					case DocUsage.Warehouse:
						jobRequiredDocument.EQ_DocCategory = "CSR";
						break;
				}

				var lookup = new MasterFiles.Business.JobRequiredDocumentLookups(jobRequiredDocument);
				var usageDescriptionInMasterfiles = lookup.DocUsage_List.GetDescriptionFromCode(usageCodeInMasterfiles);
				if (usageCodeInMasterfiles.ToUpper() == "ALL")
				{
					usageDescriptionInMasterfiles = "ALL";
				}
				AssertEquals(string.Format("Description for code {0} is different in Enterprise.Accounting.Export.Business.JobRequiredDocumentLookups.DocUsage_List than in Masterfiles.Business.JobRequiredDocumentLookups.DocUsage_List. Please update the description.", usageCodeInMasterfiles)
					, usageDescriptionInMasterfiles, usageDescriptionInAccounting);
			}

			AssertEquals("Number of codes in Masterfiles.Business.JobRequiredDocument.DocUsage is less than codes in Enterprise.Accounting.Export.Business.JobRequiredDocumentLookups.DocUsage_List. Both these lists should be the same."
				, fields.Length, Enterprise.Accounting.Export.Business.JobRequiredDocumentLookups.DocUsage_List.Count);
		}
	}
}
