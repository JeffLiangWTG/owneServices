using System.Windows.Forms;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.GUI.Testing
{
	[TestedType(typeof(StlPreviewForm))]
	internal sealed class StlPreviewFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new StlPreviewForm(new StlPreview(Factory));
		}

		#endregion
	}
}
