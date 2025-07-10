using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Business.Test;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Testing;

[TestedType(typeof(SupportIncident))]
public class SupportIncidentTest : IncidentMainBaseTestCase
{
	[GuiTest]
	public void TestCloseIncidentTasks()
	{
		var incident = Factory.New<Enterprise.Client.EDI.IncidentManager.Business.Test.SupportIncidentTest.SupportIncidentForTest>();
		var task1 = incident.WorkflowItems.AddNew();
		var task2 = incident.WorkflowItems.AddNew();
		var task3 = incident.WorkflowItems.AddNew();

		task1.P9_Sequence = 10;
		task2.P9_Sequence = 20;
		task3.P9_Sequence = 30;

		task1.P9_Status = "CLS";
		AssertEquals(false, ZFormModaliser.LastFormShownDialogForTest is ICloseIncidentPopupForm);

		task3.P9_Status = "CLS";
		AssertEquals(false, ZFormModaliser.LastFormShownDialogForTest is ICloseIncidentPopupForm);

		task2.P9_Status = "CLS";
		AssertEquals(true, ZFormModaliser.LastFormShownDialogForTest is ICloseIncidentPopupForm);
		ZFormModaliser.LastFormShownDialogForTest.Dispose();
	}
}
