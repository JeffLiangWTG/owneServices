using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(GlobalChargeCodeIntercompanyForm))]
	public class GlobalChargeCodeIntercompanyFormTest : ZFormBasherTest
	{
		#region Implementation

		protected override System.Windows.Forms.Form GetFormToBashCore()
		{
			return new GlobalChargeCodeIntercompanyForm(null);
		}

		#endregion
	}
}
