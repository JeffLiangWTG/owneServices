using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ConsolidateToOneFTypePerCLVSEntryStrategyTest : TestCaseWithFactory
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
			CACustomsDataRegistry.Instance.ConsolidateToOneFTypePerCLVSEntry.SetValue(Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty, true);
			Assert("Consolidate To one F Type true", new ConsolidateToOneFTypePerCLVSEntryStrategy(wrapper).HasAcknowledged);
			CACustomsDataRegistry.Instance.ConsolidateToOneFTypePerCLVSEntry.SetValue(Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty, false);
			Assert("Consolidate To one F Type false", !new ConsolidateToOneFTypePerCLVSEntryStrategy(wrapper).HasAcknowledged);
			declaration.JE_OH_Importer = importer.PK;
			var orgImpAddInfo = OrgImpAddInfo.Get(declaration.Importer);
			orgImpAddInfo.ZO_IsConsolidateToOneFTypePerCLVSEntry = false;
			Assert("ZO_IsConsolidateToOneFTypePerCLVSEntry = false", !new ConsolidateToOneFTypePerCLVSEntryStrategy(wrapper).HasAcknowledged);
			CACustomsDataRegistry.Instance.ConsolidateToOneFTypePerCLVSEntry.SetValue(Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty, true);
			Assert("ZConsolidateToOneFTypePerCLVSEntry Registry = true", new ConsolidateToOneFTypePerCLVSEntryStrategy(wrapper).HasAcknowledged);
			orgImpAddInfo.ZO_IsConsolidateToOneFTypePerCLVSEntry = true;
			CACustomsDataRegistry.Instance.ConsolidateToOneFTypePerCLVSEntry.SetValue(Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty, true);
			Assert("Both Reg. and Org Flag. true", new ConsolidateToOneFTypePerCLVSEntryStrategy(wrapper).HasAcknowledged);
			CACustomsDataRegistry.Instance.ConsolidateToOneFTypePerCLVSEntry.SetValue(Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty, false);
			Assert("ZO_IsConsolidateToOneFTypePerCLVSEntry = true", new ConsolidateToOneFTypePerCLVSEntryStrategy(wrapper).HasAcknowledged);
		}
	}
}
