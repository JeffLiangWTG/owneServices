using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	sealed class DeclarationOrganizationsUserControlTest : TestCase
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(EMCSJobDeclaration), control.BindingSource.DataSourceType);
		}

		public void TestConsignorDocAddressControl()
		{
			AssertType<ZDocAddressControl>(control.ConsignorDocAddressControl);
		}

		public void TestConsigneeDocAddressControl()
		{
			AssertType<ZDocAddressControl>(control.ConsigneeDocAddressControl);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new DeclarationOrganizationsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		DeclarationOrganizationsUserControl control;
	}
}
