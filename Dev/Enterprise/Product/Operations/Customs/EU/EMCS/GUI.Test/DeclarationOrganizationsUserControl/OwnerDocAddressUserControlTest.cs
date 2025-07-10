using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	sealed class OwnerDocAddressUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(EMCSJobDeclaration), control.BindingSource.DataSourceType);
		}

		public void TestOwnerDocAddressControl()
		{
			AssertType<ZDocAddressControl>(control.OwnerDocAddressControl);
		}

		public void TestOwnerDocAddressControlEnabled()
		{
			using (var form = new ZForm(declaration))
			using (var control = new OwnerDocAddressUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var ownerDocAddressControl = control.OwnerDocAddressControl;
				CombineAssertions(() =>
				{
					AssertEquals("Is false default", false, ownerDocAddressControl.Enabled);

					declaration.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.OwnerOfTheExciseProducts;
					AssertEquals("Is true when declaration.ZG_GuarantorType is 3", true, ownerDocAddressControl.Enabled);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
			control = new OwnerDocAddressUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		EMCSJobDeclaration declaration;
		OwnerDocAddressUserControl control;
	}
}
