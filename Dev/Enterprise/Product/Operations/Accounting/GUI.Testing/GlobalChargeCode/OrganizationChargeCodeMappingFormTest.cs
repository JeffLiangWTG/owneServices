using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(OrganizationChargeCodeMappingForm))]
	public class OrganizationChargeCodeMappingFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new OrganizationChargeCodeMappingForm(Factory);
		}
	}
}
