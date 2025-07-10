using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CreateIndividualLVSShipmentsStrategyTest : TestCaseWithFactory
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
			CACustomsDataRegistry.Instance.CreateIndividualLVSShipments.SetValue(Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty, true);
			Assert("Effective CreateIndividualLVSShipments", new CreateIndividualLVSShipmentsStrategy(wrapper).HasAcknowledged);
			CACustomsDataRegistry.Instance.CreateIndividualLVSShipments.SetValue(Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty, false);
			Assert("Effective CreateIndividualLVSShipments", !new CreateIndividualLVSShipmentsStrategy(wrapper).HasAcknowledged);
			declaration.JE_OH_Importer = importer.PK;
			var orgImpAddInfo = OrgImpAddInfo.Get(declaration.Importer);
			orgImpAddInfo.ZO_IsCreateIndividualLVS = false;
			Assert("Effective CreateIndividualLVSShipments", !new CreateIndividualLVSShipmentsStrategy(wrapper).HasAcknowledged);
			CACustomsDataRegistry.Instance.CreateIndividualLVSShipments.SetValue(Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty, true);
			Assert("Effective CreateIndividualLVSShipments", new CreateIndividualLVSShipmentsStrategy(wrapper).HasAcknowledged);
			orgImpAddInfo.ZO_IsCreateIndividualLVS = true;
			CACustomsDataRegistry.Instance.CreateIndividualLVSShipments.SetValue(Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty, true);
			Assert("Effective CreateIndividualLVSShipments", new CreateIndividualLVSShipmentsStrategy(wrapper).HasAcknowledged);
			CACustomsDataRegistry.Instance.CreateIndividualLVSShipments.SetValue(Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty, false);
			Assert("Effective CreateIndividualLVSShipments", new CreateIndividualLVSShipmentsStrategy(wrapper).HasAcknowledged);
		}
	}
}
