using System;
using System.Linq;
using System.Reflection;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(SupportIncidentController))]
	public class SupportIncidentControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return Modules.ClientControllerRegistration.SupportIncident;
		}

		public void TestShowEditForm()
		{
			ZString originalCompanyCode = GlbCompany.CurrentCompany.GC_Code;
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();
			try
			{
				GlbCompany.CurrentCompany.GC_Code = "EDI";
				AssertNull("Last Shown form by default should be null", Controller.LastShownForm);
				Controller.ShowEditForm(incident);
				AssertEquals("The edit form should be opened and should be editable", ODisplayMode.Browse, Controller.LastShownForm.DisplayMode);
				Controller.LastShownForm.Dispose();
				GlbCompany.CurrentCompany.GC_Code = "UKR";
				Controller.ShowEditForm(incident);
				AssertEquals("The edit form should be opened and should be editable", ODisplayMode.Browse, Controller.LastShownForm.DisplayMode);
				Controller.LastShownForm.Dispose();
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_Code = originalCompanyCode;
			}
		}

		public void TestShowControlledIncidentEditForm()
		{
			var registryCollection = new IncidentGroupTypeCollection();
			registryCollection.RemoveAll();

			var groupType = registryCollection.AddNew();
			groupType.GroupType = "MIM";
			groupType.IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV").ControlIncidents = true;

			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryCollection);
			Factory.Save();

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			var incident = link.SupportIncident;
			var group = link.IncidentManagementGroup;

			group.ING_Type = "MIM";
			group.ING_Status = "INV";

			Factory.Save();

			var incidentController = (SupportIncidentController)Controller;
			var showFormMethod = typeof(SupportIncidentController).GetMethod("ShowEditFormAccordingToDialogResult", BindingFlags.NonPublic | BindingFlags.Instance);

			link.INL_IsGroupControlled = false;
			Assert("Incident should not be controlled", !link.IsControlled);
			Factory.Save();

			showFormMethod.Invoke(incidentController, new object[] { incident, ControlledSupportIncidentDialog.Result.View });
			AssertEquals("If incident is not controlled, The incident form should be opened and should be editable, instead of read-only", ODisplayMode.Browse, incidentController.LastShownForm.DisplayMode);
			incidentController.LastShownForm.Dispose();

			link.INL_IsGroupControlled = true;
			Assert("Incident should be controlled", link.IsControlled);
			Factory.Save();

			showFormMethod.Invoke(incidentController, new object[] { incident, ControlledSupportIncidentDialog.Result.View });
			AssertEquals("The incident form should be opened and should be read only", ODisplayMode.ReadOnly, incidentController.LastShownForm.DisplayMode);
			incidentController.LastShownForm.Dispose();

			showFormMethod.Invoke(incidentController, new object[] { incident, ControlledSupportIncidentDialog.Result.Edit });
			AssertEquals("The incident form should be opened and should be editable", ODisplayMode.Browse, incidentController.LastShownForm.DisplayMode);
			incidentController.LastShownForm.Dispose();

			var groupController = (IncidentManagementGroupController)typeof(SupportIncidentController).GetProperty("IncidentGroupController", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(incidentController);
			AssertNull("No group form should be created.", groupController.LastShownForm);

			showFormMethod.Invoke(incidentController, new object[] { incident, ControlledSupportIncidentDialog.Result.OpenGroup });
			AssertNotNull("The last shown form should be IncidentManagementGroupForm.", groupController.LastShownForm);
			AssertEquals("The data source of group form is not expected.", group.PK, ((IncidentManagementGroup)groupController.LastShownForm.BusinessEntityForPersistingForm).PK);

			AssertEquals("The incident management group form should be opened and should be editable", ODisplayMode.Browse, groupController.LastShownForm.DisplayMode);
			groupController.LastShownForm.Dispose();
		}

		public void TestMakeUrlsOnlyOpenableForCurrentCompany()
		{
			SupportIncidentController controller = new SupportIncidentController();
			Assert("MakeUrlsOnlyOpenableForCurrentCompany should be false - enable open WorkItem links without needing to log into a specific branch", !controller.MakeUrlsOnlyOpenableForCurrentCompany);
		}
	}
}
