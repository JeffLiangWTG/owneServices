using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module.Testing
{
	[TestedType(typeof(UPEOrgMatchApprovalController))]
	public class UPEOrgMatchApprovalControllerTest : OrgMatchApprovalControllerTest
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.OrgMatchApproval;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			DummyBusinessObject dummyParent = Factory.New<DummyBusinessObject>();
			OrgMatchApproval.Loader loader = new OrgMatchApproval.Loader(Factory);
			DummyOrgMatchApproval dummyMatchApproval = (DummyOrgMatchApproval)loader.LoadOrCreate(dummyParent.PK, OrgMatchApprovalType.DummyType);
			Factory.Save();
			return dummyMatchApproval;
		}
	}
}
