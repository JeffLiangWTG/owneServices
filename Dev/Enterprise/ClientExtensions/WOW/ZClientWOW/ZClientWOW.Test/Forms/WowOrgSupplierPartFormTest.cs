using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	[TestedType(typeof(WowOrgSupplierPartForm))]
	public class WowOrgSupplierPartFormTest : OrgSupplierPartFormTest
	{
		#region Implementation
		protected override Form GetFormToBashCore()
		{
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			return new WowOrgSupplierPartForm(part);
		}
		#endregion
	}
}
