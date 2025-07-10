using System.Windows.Forms;
using Enterprise.Billing.StlCollector.Retriever;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	[TestedType(typeof(StlRetrieverCollectedDataForm))]
	sealed class StlRetrieverCollectedDataFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new StlRetrieverCollectedDataForm([]);
		}

		#endregion
	}
}
