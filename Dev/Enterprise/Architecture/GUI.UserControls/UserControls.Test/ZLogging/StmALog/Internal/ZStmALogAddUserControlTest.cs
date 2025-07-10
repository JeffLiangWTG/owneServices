using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZStmALogAddUserControlTest : TestCaseWithFactory
	{
		public void TestEventReferenceForm()
		{
			var eventReference = new EventReference(Events.CustomisableEvent00Code, ZString.Empty);

			using (var referenceForm = new EventReferenceForm(eventReference))
			{
				referenceForm.Show();

				eventReference.FreeText = "Test Free Text";

				var testParam1 = new Parameter(Events.CustomisableEvent00Code, "LOC", "AUSYD");
				var testParam2 = new Parameter(Events.CustomisableEvent00Code, "LOC", "AUSYD");
				eventReference.ParameterCollection.Add(testParam1);
				eventReference.ParameterCollection.Add(testParam2);

				((ZButton)referenceForm.Controls["ConfimButton"]).PerformClick();

				AssertEquals("Please fix all the errors before proceeding.", UnitTestUserNotification.Instance.LastMessage.Text);

				testParam1.Code = "FAC";
				eventReference.FreeText = new string('A', 1000);
				testParam1.ParamValue = "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB";
				((ZButton)referenceForm.Controls["ConfimButton"]).PerformClick();

				AssertEquals("The event reference generated cannot be more than 1024 characters.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
