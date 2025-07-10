using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(TradeChainPartnerSendingObject))]
	sealed class TradeChainPartnerSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			var tcp = orgImp.TradeChainPartners.AddNew();
			return new TradeChainPartnerSendingObject(tcp, organisation);
		}

		protected override void SetUp()
		{
			base.SetUp();
			this.organisation = CreateNewOrgForTest("TESTORG01", "Test Organisation 01", "Test Organisation 01 Address 01");
			this.orgImp = OrgImpAddInfo.Get(organisation);
			orgImp.TradeChainPartners.AddNew();
		}

		OrgHeader CreateNewOrgForTest(string orgCode, string orgFullName, string orgAddress)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = orgCode;
			var address = org.Addresses.AddNew();
			address.OA_Address1 = orgAddress;
			return org;
		}

		OrgHeader organisation;
		OrgImpAddInfo orgImp;
		#endregion
	}
}
