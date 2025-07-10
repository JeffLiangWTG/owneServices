using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(DateAcceptabilityLegendForm))]
	class DateAcceptabilityLegendFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new DateAcceptabilityLegendForm();
		}
	}
}
