using System.Reflection;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting.Testing
{
	[TestedType(typeof(CalculationXMLTextForm))]
	public class CalculationXMLTextFormTest : BasherTest
	{
		#region ShowDialog

		public void TestShowDialog()
		{
			using (var auditLogTextForm = new CalculationXMLTextForm("Note Text"))
			{
				ZFormModaliser.ShowDialogAndDispose(auditLogTextForm);
				var dialog = (CalculationXMLTextForm)ZFormModaliser.LastFormShownDialogForTest;
				string logText = GetControl<ZTextBox>(dialog, "CalculationXMLTextBox").Text;

				AssertEquals("Note Text", logText);
			}
		}

		#endregion

		#region Implementation

		T GetControl<T>(CalculationXMLTextForm dialog, string name)
		{
			return (T)typeof(CalculationXMLTextForm).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(dialog);
		}

		public override Form GetFormToBash()
		{
			return new CalculationXMLTextForm("Text");
		}

		#endregion
	}
}
