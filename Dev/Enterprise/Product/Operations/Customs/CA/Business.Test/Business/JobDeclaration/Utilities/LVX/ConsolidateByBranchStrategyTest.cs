using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ConsolidateByBranchStrategyTest : TestCaseWithFactory
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
			var wrapper = new JobDeclarationConsolidationOptionsWrapper(declaration);
			CACustomsDataRegistry.Instance.ConsolidateByBranch.SetValue(Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty, true);
			Assert("Effective ConsolidateByBranch", new ConsolidateByBranchStrategy(wrapper).HasAcknowledged);
			CACustomsDataRegistry.Instance.ConsolidateByBranch.SetValue(Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty, false);
			Assert("Effective ConsolidateByBranch", !new ConsolidateByBranchStrategy(wrapper).HasAcknowledged);
			declaration.JE_OH_Importer = importer.PK;
			var orgImpAddInfo = OrgImpAddInfo.Get(declaration.Importer);
			orgImpAddInfo.ZO_IsConsolidateByBranch = false;
			Assert("Effective ConsolidateByBranch", !new ConsolidateByBranchStrategy(wrapper).HasAcknowledged);
			CACustomsDataRegistry.Instance.ConsolidateByBranch.SetValue(Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty, true);
			Assert("Effective ConsolidateByBranch", new ConsolidateByBranchStrategy(wrapper).HasAcknowledged);
			orgImpAddInfo.ZO_IsConsolidateByBranch = true;
			CACustomsDataRegistry.Instance.ConsolidateByBranch.SetValue(Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty, true);
			Assert("Effective ConsolidateByBranch", new ConsolidateByBranchStrategy(wrapper).HasAcknowledged);
			CACustomsDataRegistry.Instance.ConsolidateByBranch.SetValue(Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty, false);
			Assert("Effective ConsolidateByBranch", new ConsolidateByBranchStrategy(wrapper).HasAcknowledged);
		}

		public void TestFillDataForNewDeclaration()
		{
			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.GB_Code = "GBA";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = branch.PK;
			var wrapper = new JobDeclarationConsolidationOptionsWrapper(declaration);
			var stratrgy = new ConsolidateByBranchStrategy(wrapper);
			CACustomsDataRegistry.Instance.ConsolidateByBranch.SetValue(Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty, false);
			Assert("Effective ConsolidateByBranch", !new ConsolidateByBranchStrategy(wrapper).HasAcknowledged);
			var newDeclaration = Factory.New<JobDeclaration>();
			newDeclaration.JE_GB = ZGuid.Empty;
			stratrgy.FillDataForNewDeclaration(newDeclaration);
			AssertEquals(GlbBranch.CurrentBranch.PK, newDeclaration.JE_GB);
			CACustomsDataRegistry.Instance.ConsolidateByBranch.SetValue(Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty, true);
			stratrgy = new ConsolidateByBranchStrategy(wrapper);
			Assert("Effective ConsolidateByBranch", stratrgy.HasAcknowledged);
			newDeclaration = Factory.New<JobDeclaration>();
			newDeclaration.JE_GB = ZGuid.Empty;
			stratrgy.FillDataForNewDeclaration(newDeclaration);
			AssertEquals(branch.PK, newDeclaration.JE_GB);
		}
	}
}
