using Enterprise.Customs.JP.Business;

namespace Enterprise.Customs.JP.GUI.Testing
{
	public class OrgSupplierPartFormCustomsPluginTest : Customs.GUI.Testing.OrgSupplierPartFormCustomsPluginTest
	{
		public override void TestGetNewUserControl()
		{
			using (var plugin = GetNewPlugIn(Part))
			{
				AssertType<OrgSupplierPartFormCustomsControlGlobal>(plugin.UserControl);
			}
		}

		protected override MasterFiles.Business.OrgSupplierPart GetNewPart()
		{
			return Factory.New<OrgSupplierPart>();
		}

		protected override Customs.GUI.OrgSupplierPartFormCustomsPlugin GetNewPlugIn(MasterFiles.Business.OrgSupplierPart part)
		{
			return new OrgSupplierPartFormCustomsPlugin((OrgSupplierPart)part);
		}
	}
}
