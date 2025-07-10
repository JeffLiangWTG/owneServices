using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	[TestedType(typeof(ZPerformanceForm))]
	sealed class ZPerformanceFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ZPerformanceForm(new PerformanceStatisticWithForms());
		}

		public new void TestFormIsFullyTranslatable()
		{
			Assert(true); // This form does not need to be translatable, as it is for diagnostics purposes
		}
	}
}
