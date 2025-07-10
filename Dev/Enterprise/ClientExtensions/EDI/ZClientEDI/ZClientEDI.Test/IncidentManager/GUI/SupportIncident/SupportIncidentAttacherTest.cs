using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	public class SupportIncidentAttacherTest : TestCaseWithFactory
	{
		public void TestAttachCore()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = "ENT";
			incident.IM_Description = "Something";
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			incident.Factory.Save();
			AssertEquals("PRE:", false, incident.CanCreateWorkItem);
			var findBoxList = new SupportIncidentCollection(Factory);
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var attacher = new SupportIncidentAttacherForTest(workItem.RelatedItems, findBoxList, ClientModuleRegistration.SupportIncident, false);
			attacher.TestAttachCore(incident, new List<BusinessObject>());
			AssertEquals("Message shown when not CanCreateWorkItem", ModuleSelectionControl.YouCannotCreateWorkItem, UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			incident.Factory.Save();
			AssertEquals("PRE:", true, incident.CanCreateWorkItem);
			attacher.TestAttachCore(incident, new List<BusinessObject>());
			AssertEquals("Message not shown", null, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestAttachIncidentAsParentOnWorkItem()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = "ENT";
			incident.IM_Description = "Something";
			incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			incident.Factory.Save();
			AssertEquals("PRE:", true, incident.CanCreateWorkItem);
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			workItem.WKI_WorkItemNumber = "WI00001234";
			var attacher = new SupportIncidentAttacherForTest(workItem.RelatedItems, new SupportIncidentCollection(Factory), ClientModuleRegistration.SupportIncident, true);
			var listToBulkAdd = new List<BusinessObject>();
			attacher.TestAttachCore(incident, listToBulkAdd);
			attacher.AttachItemsCore_Exposed(workItem.RelatedItems, listToBulkAdd);
			Factory.Save();
			AssertEquals(1, incident.ChildrenOnlyRelatedItems.Count);
			AssertEquals("WI00001234", ((NewWorkItem)incident.ChildrenOnlyRelatedItems.First()).WKI_WorkItemNumber);
		}
	}
}
