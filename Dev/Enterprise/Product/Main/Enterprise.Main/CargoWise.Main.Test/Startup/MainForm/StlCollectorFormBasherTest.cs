using System.Windows.Forms;
using Enterprise.Billing.StlCollector.Retriever;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	[TestedType(typeof(StlCollectorForm))]
	sealed class StlCollectorFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new StlCollectorForm([]);
		}

		#endregion
	}
}
