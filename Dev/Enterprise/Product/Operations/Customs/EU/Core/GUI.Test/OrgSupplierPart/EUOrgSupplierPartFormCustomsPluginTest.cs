using System.Windows.Forms;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;

namespace Enterprise.Customs.EU.GUI.Testing
{
	public class EUOrgSupplierPartFormCustomsPluginTest : OrgSupplierPartFormCustomsPluginTest
	{
		public override void TestGetNewUserControl()
		{
			using (var plugin = (EUOrgSupplierPartFormCustomsPluginForTesting)GetNewPlugIn(Part))
			{
				using (Control control = plugin.GetNewUserControl_Exposed())
				{
					AssertEquals(typeof(EUOrgSupplierPartFormCustomsControl), control.GetType());
				}
			}
		}

		#region Implementation

		protected override MasterFiles.Business.OrgSupplierPart GetNewPart()
		{
			return (OrgSupplierPart)OrgSupplierPart.New(Factory);
		}

		protected override OrgSupplierPartFormCustomsPlugin GetNewPlugIn(MasterFiles.Business.OrgSupplierPart part)
		{
			return new EUOrgSupplierPartFormCustomsPluginForTesting((OrgSupplierPart)part);
		}

		#endregion

		class EUOrgSupplierPartFormCustomsPluginForTesting : EUOrgSupplierPartFormCustomsPlugin
		{
			public EUOrgSupplierPartFormCustomsPluginForTesting(OrgSupplierPart part)
				: base(part)
			{
			}

			public Control GetNewUserControl_Exposed()
			{
				return base.GetNewUserControl();
			}
		}
	}
}
