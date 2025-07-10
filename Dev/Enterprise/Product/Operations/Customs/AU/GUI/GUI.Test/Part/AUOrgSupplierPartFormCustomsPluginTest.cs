using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class AUOrgSupplierPartFormCustomsPluginTest : Customs.GUI.Testing.OrgSupplierPartFormCustomsPluginTest
	{
		public override void TestGetNewUserControl()
		{
			using (AUOrgSupplierPartFormCustomsPlugin plugin = (AUOrgSupplierPartFormCustomsPlugin)GetNewPlugIn(Part))
			{
				AssertType<AUOrgSupplierPartFormCustomsControl>(plugin.UserControl);
			}
		}

		protected override MasterFiles.Business.OrgSupplierPart GetNewPart() => Factory.New<AUOrgSupplierPart>();

		protected override OrgSupplierPartFormCustomsPlugin GetNewPlugIn(MasterFiles.Business.OrgSupplierPart part) => new AUOrgSupplierPartFormCustomsPlugin((AUOrgSupplierPart)part);
	}
}
