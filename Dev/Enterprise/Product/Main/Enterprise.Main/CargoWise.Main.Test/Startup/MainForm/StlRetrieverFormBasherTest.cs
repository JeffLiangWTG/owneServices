using System.Windows.Forms;
using Enterprise.Billing.StlCollector.Retriever;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	[TestedType(typeof(StlRetrieverForm))]
	sealed class StlRetrieverFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new StlRetrieverForm();
		}

		#endregion
	}
}
