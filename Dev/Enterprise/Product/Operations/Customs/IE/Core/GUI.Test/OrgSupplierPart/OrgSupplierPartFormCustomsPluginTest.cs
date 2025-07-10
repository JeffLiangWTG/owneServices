using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business.MasterFiles;

namespace Enterprise.Customs.IE.GUI.Testing
{
	public class OrgSupplierPartFormCustomsPluginTest : TestCaseWithFactory
	{
		public void TestGetNewUserControl()
		{
			var part = Factory.New<OrgSupplierPart>();
			using (var plugIn = new OrgSupplierPartFormCustomsPluginForTest(part))
			using (var control = plugIn.GetNewUserControl())
			{
				AssertType<OrgSupplierPartFormCustomsControl>(control);
			}
		}
	}

	class OrgSupplierPartFormCustomsPluginForTest : OrgSupplierPartFormCustomsPlugin
	{
		public OrgSupplierPartFormCustomsPluginForTest(OrgSupplierPart part) : base(part)
		{
		}

		public new Control GetNewUserControl() => base.GetNewUserControl();
	}
}
