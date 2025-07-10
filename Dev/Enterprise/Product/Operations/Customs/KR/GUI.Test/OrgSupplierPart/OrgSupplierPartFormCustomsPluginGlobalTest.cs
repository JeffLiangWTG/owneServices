using System.Windows.Forms;
using Enterprise.Customs.KR.Business;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class OrgSupplierPartFormCustomsPluginGlobalTest : Customs.GUI.Testing.OrgSupplierPartFormCustomsPluginTest
	{
		public override void TestGetNewUserControl()
		{
			using (OrgSupplierPartFormCustomsPluginGlobalForTest plugin = (OrgSupplierPartFormCustomsPluginGlobalForTest)GetNewPlugIn(Part))
			{
				using (Control control = plugin.GetNewUserControlExposed())
				{
					AssertEquals(typeof(OrgSupplierPartFormCustomsControlGlobal), control.GetType());
				}
			}
		}

		protected override MasterFiles.Business.OrgSupplierPart GetNewPart()
		{
			return Factory.New<OrgSupplierPart>();
		}

		protected override Customs.GUI.OrgSupplierPartFormCustomsPlugin GetNewPlugIn(MasterFiles.Business.OrgSupplierPart part)
		{
			return new OrgSupplierPartFormCustomsPluginGlobalForTest((OrgSupplierPart)part);
		}

		class OrgSupplierPartFormCustomsPluginGlobalForTest : OrgSupplierPartFormCustomsPluginGlobal
		{
			public OrgSupplierPartFormCustomsPluginGlobalForTest(OrgSupplierPart part)
				: base(part)
			{
			}
			public Control GetNewUserControlExposed() => GetNewUserControl();
		}
	}
}
