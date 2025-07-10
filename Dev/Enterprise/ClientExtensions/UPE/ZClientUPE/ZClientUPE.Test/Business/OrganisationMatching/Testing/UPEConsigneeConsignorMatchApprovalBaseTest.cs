using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.DataImport.Level1FileFormat;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Client.UPE.Business.Testing
{
	public abstract class UPEConsigneeConsignorMatchApprovalBaseTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParentAccountNumber()
		{
			Level1Record level1Record = new Level1Record();
			SetLevel1RecordOrganisation(level1Record);
			AirCargo.Level1Record = level1Record;
			AssertEquals("A15V04", GetNewOrgMatchApproval().OwnerCode);
		}

		public void TestParentUNLOCO()
		{
			ExpectedParentUNLOCOInfo.Value = (ZString)"AUBNE";
			OrgMatchApproval matchApproval = GetNewOrgMatchApproval();
			UPEOrgHeader newOrganisation = Factory.New<UPEOrgHeader>();
			matchApproval.CopyDetailsToOrganisation(newOrganisation);
			AssertEquals("AUBNE", newOrganisation.OH_RL_NKClosestPort);
		}

		public virtual void TestParentFax()
		{
			Level1Record level1Record = new Level1Record();
			SetLevel1RecordOrganisation(level1Record);
			AirCargo.Level1Record = level1Record;
			AssertEquals("12125630422", GetNewOrgMatchApproval().Fax);
		}

		public void TestCopyDetailsToOrganisation_SwithOffCheckingForDuplicates()
		{
			OrgMatchApproval matchApproval = GetNewOrgMatchApproval();
			UPEOrgHeader newOrganisation = Factory.New<UPEOrgHeader>();
			matchApproval.CopyDetailsToOrganisation(newOrganisation);
			AssertEquals(false, newOrganisation.MustPerformCheckForDuplicateOrganisations);
		}

		protected abstract OrgMatchApproval GetNewOrgMatchApproval();
		protected abstract ZPropertyInfo ExpectedParentUNLOCOInfo { get; }

		protected abstract void SetLevel1RecordOrganisation(Level1Record level1Record);
		#region Implementation
		protected UPECusHAWB AirCargo;
		protected OrgMatchApproval.Loader Loader;
		protected sealed override BusinessObject GetNewBusinessObject()
		{
			return GetNewOrgMatchApproval();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewOrgMatchApproval();
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			AirCargo = Factory.NewWithValidTestData<UPECusHAWB>();
			Loader = new OrgMatchApproval.Loader(Factory);
		}
		#endregion
	}
}
