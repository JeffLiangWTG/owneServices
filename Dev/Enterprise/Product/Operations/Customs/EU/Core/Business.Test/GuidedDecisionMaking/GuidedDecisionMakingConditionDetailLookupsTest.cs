using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class GuidedDecisionMakingConditionDetailLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestConditionDetailCodesList()
		{
			var conditionDetailCodesList = lookups.ConditionDetailCodesList;
			AssertType<ZZRefCusCodeListCombinedCollection>(lookups.ConditionDetailCodesList);
			Assert("Pre-requisite: GDMBasic.IsImport is true", gDMBasic.IsImport);
			Assert("Pre-requisite: GDMBasic.IsExport is false", !gDMBasic.IsExport);
			AssertEquals("Code List Type should be SupportingDocumentOfImportDirection", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, conditionDetailCodesList.CodeTypes.First());

			gDMBasic.IsImport = false;
			gDMBasic.IsExport = true;
			Assert("Pre-requisite: GDMBasic.IsImport is false", !gDMBasic.IsImport);
			Assert("Pre-requisite: GDMBasic.IsExport is true", gDMBasic.IsExport);
			conditionDetailCodesList = lookups.ConditionDetailCodesList;
			AssertEquals("Code List Type should be SupportingDocumentOfExportDirection", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, conditionDetailCodesList.CodeTypes.First());
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			gDMBasic = invoiceLine.GetGuidedDecisionMakingBasic();
			var condition = new GuidedDecisionMakingCondition(gDMBasic);
			var conditionDetail = new GuidedDecisionMakingConditionDetail(condition);
			lookups = conditionDetail.Lookups;
		}
		GuidedDecisionMakingBasic gDMBasic;
		GuidedDecisionMakingConditionDetailLookups lookups;
	}
}

