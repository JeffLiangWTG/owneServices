using System.Web.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZAddressFindBoxLabelTest : WebControlTest
	{
		#region setup

		DummyWithZAddress Dummy
		{
			get { return (DummyWithZAddress)TestBizO; }
		}

		protected override Control GetNewControl()
		{
			return new ZAddressFindBoxLabel();
		}

		protected override void SetUp()
		{
			base.SetUp();
			Label.BindTo = "Z0_Guid";
			Label.BindToList = "Z0_Guid_ZAddress.OrgAddress_List";
		}

		ZAddressFindBoxLabel Label
		{
			get { return Control as ZAddressFindBoxLabel; }
		}

		protected override DummyEnterpriseBusinessObject GetNewDataSource()
		{
			return Factory.New<DummyWithZAddress>();
		}

		#endregion

		public void TestBinding()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgAddress address = header.Addresses.AddNew();
			Dummy.Z0_Guid = header.Addresses[0].PK;

			Label.Bind(TestBizO);
			AssertNotNullOrEmpty("label text", Label.Text);
		}
	}
}
