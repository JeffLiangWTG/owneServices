using System.Windows.Forms;
using Enterprise.Customs.DE.Business;

namespace Enterprise.Customs.DE.GUI.Testing
{
	public class OrgSupplierPartFormCustomsPluginTest : Customs.GUI.Testing.OrgSupplierPartFormCustomsPluginTest
	{
		public override void TestGetNewUserControl()
		{
			using (var plugin = (OrgSupplierPartFormCustomsPluginForTesting)GetNewPlugIn(Part))
			{
				using (var control = plugin.GetNewUserControl_Exposed())
				{
					AssertEquals(typeof(OrgSupplierPartFormCustomsControl), control.GetType());
				}
			}
		}

		#region Implementation

		protected override MasterFiles.Business.OrgSupplierPart GetNewPart()
		{
			return (OrgSupplierPart)OrgSupplierPart.New(Factory);
		}

		protected override Customs.GUI.OrgSupplierPartFormCustomsPlugin GetNewPlugIn(MasterFiles.Business.OrgSupplierPart part)
		{
			return new OrgSupplierPartFormCustomsPluginForTesting((OrgSupplierPart)part);
		}

		#endregion

		class OrgSupplierPartFormCustomsPluginForTesting : OrgSupplierPartFormCustomsPlugin
		{
			public OrgSupplierPartFormCustomsPluginForTesting(OrgSupplierPart part)
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
