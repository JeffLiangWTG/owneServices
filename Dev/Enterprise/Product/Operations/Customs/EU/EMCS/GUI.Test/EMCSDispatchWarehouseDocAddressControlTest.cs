using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	public class EMCSDispatchWarehouseDocAddressControlTest : TestCaseWithFactory
	{
		public void TestDispatchReferenceTextBoxShouldBindToZG_DispatchReference()
		{
			using (var form = new ZForm(declaration))
			using (var control = new EMCSDispatchWarehouseDocAddressControl())
			{
				form.Controls.Add(control);
				form.Show();
				var dispatchReferenceTextBox = control.FindSingle<ZTextBox>("DispatchReferenceTextBox");
				AssertEquals(EMCSJobDeclaration.Schema.ZG_DispatchReference, control.BindingSource.GetBindingMember(dispatchReferenceTextBox));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<EMCSJobDeclaration>();
		}
		EMCSJobDeclaration declaration;
	}
}
