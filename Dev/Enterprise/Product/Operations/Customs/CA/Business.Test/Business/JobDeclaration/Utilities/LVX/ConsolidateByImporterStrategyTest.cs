using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ConsolidateByImporterStrategyTest : TestCaseWithFactory
	{
		public void TestHasAcknowledged()
		{
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.GB_Code = "GBA";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = branch.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var wrapper1 = new JobDeclarationConsolidationOptionsWrapper(declaration);
			CACustomsDataRegistry.Instance.ConsolidateByImporter.SetValue(Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty, true);
			Assert("Effective ConsolidateByImporter", new ConsolidateByImporterStrategy(wrapper1).HasAcknowledged);
			CACustomsDataRegistry.Instance.ConsolidateByImporter.SetValue(Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty, false);
			Assert("Effective ConsolidateByImporter", !new ConsolidateByImporterStrategy(wrapper1).HasAcknowledged);
			invoiceLine.CA_AuthorityNumber = "1";
			Assert("Effective ConsolidateByImporter", new ConsolidateByImporterStrategy(wrapper1).HasAcknowledged);
			invoiceLine.CA_AuthorityNumber = "";
			Assert("Effective ConsolidateByImporter", !new ConsolidateByImporterStrategy(wrapper1).HasAcknowledged);
			declaration.JE_OH_Importer = importer.PK;
			var orgImpAddInfo = OrgImpAddInfo.Get(declaration.Importer);
			orgImpAddInfo.ZO_IsLVSConsolidated = false;
			Assert("Effective ConsolidateByImporter", !new ConsolidateByImporterStrategy(wrapper1).HasAcknowledged);
			invoiceLine.CA_AuthorityNumber = "1";
			Assert("Effective ConsolidateByImporter", new ConsolidateByImporterStrategy(wrapper1).HasAcknowledged);
			invoiceLine.CA_AuthorityNumber = "";
			Assert("Effective ConsolidateByImporter", !new ConsolidateByImporterStrategy(wrapper1).HasAcknowledged);
			CACustomsDataRegistry.Instance.ConsolidateByImporter.SetValue(Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty, true);
			Assert("Effective ConsolidateByImporter", new ConsolidateByImporterStrategy(wrapper1).HasAcknowledged);
			orgImpAddInfo.ZO_IsLVSConsolidated = true;
			CACustomsDataRegistry.Instance.ConsolidateByImporter.SetValue(Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty, true);
			Assert("Effective ConsolidateByImporter", new ConsolidateByImporterStrategy(wrapper1).HasAcknowledged);
			CACustomsDataRegistry.Instance.ConsolidateByImporter.SetValue(Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty, false);
			Assert("Effective ConsolidateByImporter", new ConsolidateByImporterStrategy(wrapper1).HasAcknowledged);

			CACustomsDataRegistry.Instance.ConsolidateByImporter.SetValue(Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty, false);
			orgImpAddInfo.ZO_IsLVSConsolidated = false;
			Assert("Effective ConsolidateByImporter", !new ConsolidateByImporterStrategy(wrapper1).HasAcknowledged);
			orgImpAddInfo.ZO_IsImporterDirectPayment = true;
			Assert("Effective ConsolidateByImporter", new ConsolidateByImporterStrategy(wrapper1).HasAcknowledged);
			orgImpAddInfo.ZO_IsImporterDirectPayment = false;
			orgImpAddInfo.ZO_IsLVSImporterDirectPayment = true;
			Assert("Effective ConsolidateByImporter", new ConsolidateByImporterStrategy(wrapper1).HasAcknowledged);
			orgImpAddInfo.ZO_IsLVSImporterDirectPayment = false;
			orgImpAddInfo.ZO_IsGSTDirectPayment = true;
			Assert("Effective ConsolidateByImporter", new ConsolidateByImporterStrategy(wrapper1).HasAcknowledged);

			var simplifiedLVS = new SimplifiedLVS(Factory);
			var wrapper2 = new SimplifiedLVSConsolidationOptionsWrapper(simplifiedLVS);
			Assert("Effective ConsolidateByImporter", !new ConsolidateByImporterStrategy(wrapper2).HasAcknowledged);
			simplifiedLVS.CA_AllowOIC = true;
			Assert("Effective ConsolidateByImporter", new ConsolidateByImporterStrategy(wrapper2).HasAcknowledged);
		}

		public void TestAddMatchingFilter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			var addInfo = OrgImpAddInfo.Get(importer);
			addInfo.ZO_AccountSecurityNumber = "54321";
			addInfo.ZO_AccountSecirityPassword = "12345678";
			declaration.JE_OH_Importer = importer.PK;
			Factory.Save();
			var wrapper = new JobDeclarationConsolidationOptionsWrapper(declaration);
			CACustomsDataRegistry.Instance.ConsolidateByImporter.SetValue(Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty, true);
			var strategy = new ConsolidateByImporterStrategy(wrapper);
			var query = new ZQuery();
			strategy.AddMatchingFilter(query);
			var whereClause = query.GetAsWhereClause(true);
			Assert("ZQuery contains JE_AllowOIC = 0", whereClause.Contains("JE_AllowOIC = 0"));
			Assert("ZQuery contains CE_EntryNum clause", whereClause.Contains("CE_EntryNum Like '54321%'"));

			invoiceLine.CA_AuthorityNumber = "1";
			strategy = new ConsolidateByImporterStrategy(wrapper);
			strategy.AddMatchingFilter(query);
			Assert("ZQuery contains JE_AllowOIC = 1", query.GetAsWhereClause(true).Contains("JE_AllowOIC = 1"));
		}

		public void TestFillDataForNewDeclaration()
		{
			CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			CACustomsDataRegistry.Instance.ConsolidateByImporter.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			var addInfo = OrgImpAddInfo.Get(importer);
			addInfo.ZO_AccountSecurityNumber = "54321";
			addInfo.ZO_AccountSecirityPassword = "12345678";
			declaration.JE_OH_Importer = importer.PK;
			Factory.Save();
			var wrapper = new JobDeclarationConsolidationOptionsWrapper(declaration);
			CACustomsDataRegistry.Instance.ConsolidateByImporter.SetValue(Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty, true);
			var strategy = new ConsolidateByImporterStrategy(wrapper);
			var declaration2 = Factory.New<JobDeclaration>();
			strategy.FillDataForNewDeclaration(declaration2);
			Assert("CA_AllowOIC", !declaration2.CA_AllowOIC);
			AssertEquals("AccountSecurityCode", "54321", declaration2.TransactionNumber.AccountSecurityCode);
			AssertEquals("Importer set from Declaration level", importer.PK, declaration2.JE_OH_Importer);

			invoiceLine.CA_AuthorityNumber = "1";
			strategy = new ConsolidateByImporterStrategy(wrapper);
			strategy.FillDataForNewDeclaration(declaration2);
			Assert("CA_AllowOIC", declaration2.CA_AllowOIC);

			invoice.JZ_OH_Buyer = importer.PK;
			declaration.JE_OH_Importer = ZGuid.Empty;
			wrapper = new JobDeclarationConsolidationOptionsWrapper(declaration);
			strategy = new ConsolidateByImporterStrategy(wrapper);
			strategy.FillDataForNewDeclaration(declaration2);
			AssertEquals("Importer set from Invoice level", importer.PK, declaration2.JE_OH_Importer);
		}
	}
}
