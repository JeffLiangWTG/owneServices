using System.Windows.Forms;
using Enterprise.Client.DHL.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.DHL.GUI.Testing
{
	[TestedType(typeof(FlightBulkUpdateForm))]
	public class FlightBulkUpdateFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			FlightBulkUpdateBusinessObject bizObj = new FlightBulkUpdateBusinessObject();
			return new FlightBulkUpdateForm(bizObj);
		}
	}
}
