using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(OrgCollectionCallsController))]
	public class CollectionCallsControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.OrgCollectionCalls;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_Code = "OrgHeader1";
			header.CompanyData.OB_IsDebtor = true;
			var query = new ZQuery(vw_OrgCollectionCallSchema.CC_OH, header.PK);
			query.AddToFilter(vw_OrgCollectionCallSchema.CC_GC, GlbCompany.CurrentCompany.PK);
			Factory.Save();
			return Factory.LoadTop1<OrgCollectionCall>(query);
		}

		public override void TestNewForm()
		{
			AssertNull(Controller.ShowNewForm());
		}

		public override void TestDeleteForm()
		{
			AssertNull(Controller.ShowDeleteForm(GetBusinessObjectThatIsInTheDatabase()));
		}
	}
}
