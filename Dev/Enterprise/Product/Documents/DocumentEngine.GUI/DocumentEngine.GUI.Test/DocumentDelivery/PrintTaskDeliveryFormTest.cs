using System.Windows.Forms;
using Enterprise.DocumentEngine.GUI.DocumentDelivery;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	[TestedType(typeof(PrintTaskDeliveryForm))]
	sealed class PrintTaskDeliveryFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			PrintTask task = new PrintTask();
			PrintTaskDeliveryForm form = new PrintTaskDeliveryForm(task.TaskSettings);
			return form;
		}
	}
}
