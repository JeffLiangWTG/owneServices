using Enterprise.Accounting.Business.GlobalChargeCode;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(GlobalChargeCodeOrganizationForm))]
	public class GlobalChargeCodeOrganizationFormTest : ZFormBasherTest
	{
		#region Implementation

		protected override System.Windows.Forms.Form GetFormToBashCore()
		{
			return new GlobalChargeCodeOrganizationForm(Factory.New<GlobalChargeCodeMapOrganization>());
		}

		#endregion
	}
}
