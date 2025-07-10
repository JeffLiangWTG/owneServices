using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public class ModuleSelectionControl : RelatedWorkItemModuleButtonGrid
	{
		public event EventHandler NewFormSaved;

		public ModuleSelectionControl()
			: base()
		{
			OnAttach += ModuleSelectionControl_OnAttach;
		}

		SupportIncident Incident
		{
			get { return (SupportIncident)WorkItemSource; }
		}

		public const string YouCannotCreateWorkItem =
			"You cannot create or attach a work item when the incident is in support stage.\r\nPlease escalate the incident via Actions -> Escalate first.";

		protected override IBusiness GetNewBusinessEntity(ZController controller)
		{
			if (!Incident.CanCreateWorkItem)
			{
				Globals.Message.Show(YouCannotCreateWorkItem);
				return null;
			}
			else
			{
				return base.GetNewBusinessEntity(controller);
			}
		}

		protected override void NewForm_Saved(object sender, EventArgs e)
		{
			base.NewForm_Saved(sender, e);
			NewFormSaved?.Invoke(sender, e);
		}

		protected override void AttachButton_Click(object sender, EventArgs e)
		{
			if (!Incident.CanCreateWorkItem)
			{
				Globals.Message.Show(YouCannotCreateWorkItem);
			}
			else
			{
				base.AttachButton_Click(sender, e);
			}
		}

		void ModuleSelectionControl_OnAttach(object sender, ModuleButtonGridOnAttachEventArgs e)
		{
			try
			{
				if (((ZForm)ParentForm).BusinessEntity is SupportIncident incident && e.AttachedBusinessObjects.Any())
				{
					var incidents = e.AttachedBusinessObjects.OfType<SupportIncident>();
					if (incidents.Any())
					{
						RetrospectivelyBroadcastEConversationsHelper.OpenNewForm(incident, incidents.ToList());
					}
					else
					{
						var workitems = e.AttachedBusinessObjects.OfType<NewWorkItem>();
						if (workitems.Any())
						{
							RetrospectivelyBroadcastEConversationsHelper.OpenNewForm(incident, workitems.ToList());
						}
					}
				}
			}
			catch (Exception ex)
			{
				ErrorReporter.ReportOnce("Error creating retrospectively broadcast eConversations form", ex);
			}
		}
	}
}
