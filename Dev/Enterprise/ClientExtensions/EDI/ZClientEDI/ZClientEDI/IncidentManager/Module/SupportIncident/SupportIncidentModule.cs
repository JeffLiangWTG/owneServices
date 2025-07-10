using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.GUI;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class SupportIncidentModule : ZFilterGridModule
	{
		public SupportIncidentModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return Modules.ClientModuleRegistration.SupportIncident; }
		}

		public override bool SupportsConversations => true;

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.AlwaysAllow; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			var controller = ZControllerFactory.Create(Modules.ClientControllerRegistration.SupportIncident) as SupportIncidentController;
			if (newControllerEventHandler != null)
			{
				controller.OnFormShown += newControllerEventHandler;
				newControllerEventHandler = null;
			}
			return controller;
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new SupportIncidentFilterBusinessObject();
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new SupportIncidentCollection(Factory);
		}

#if DEBUG
		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());
			result.Add(new ZMenuItem("Process Web Requests (Test Only)", new EventHandler(ProcessWebRequests)));
			return result.ToArray();
		}

		internal void ProcessWebRequests(object sender, EventArgs e)
		{
			var logger = new TestLogger();
			var processor = new BatchProcessor.SupportRequestProcessor(logger);
			processor.ProcessAllWebUpdates();
			ZArchitecture.Environment.Globals.Message.Show(logger.ToString());
		}

		class TestLogger : ILogger
		{
			public void Log(LogType type, string message)
			{
				Log(type, message, null);
			}

			public void Log(LogType type, string message, Exception ex)
			{
				logLines.Add($"{type}|{message}{(ex != null ? $"|{ex}" : string.Empty)}");
			}

			public override string ToString()
			{
				var builder = new StringBuilder();
				logLines.ForEach(delegate (string line) { builder.AppendLine(line); });
				return builder.ToString();
			}

			public void ClearLog()
			{
				logLines.Clear();
			}

			public string this[int index] => logLines[index];

			public int Count => logLines.Count;

			readonly List<string> logLines = [];
		}
#endif

		#region Internal Incident & Feature Request

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			MenuItem[] menuItems = base.GetNewStandardMenuItems();

			MenuItem supportIncidentMenuItem = new ZMenuItem("Customer Service Incident", delegate { ShowNewForm(); });
			MenuItem internalIncidentMenuItem = new ZMenuItem("Internal Incident", new EventHandler(NewInternalIncident));

			NewMenuItem.MenuItems.Add(supportIncidentMenuItem);
			NewMenuItem.MenuItems.Add(internalIncidentMenuItem);

			return menuItems;
		}

		void NewInternalIncident(object sender, EventArgs e)
		{
			NewInternalIncident(SupportIncidentCategoriesList.Codes.Support, SupportIncident.InternalIncidentComment);
		}

		void NewInternalIncident(string stage, string comment)
		{
			if (EDIDataRegistry.Instance.GlowNewInternalIncident.Value)
			{
				var controller = ZControllerFactory.Create(ControllerIDs.ServiceRequest);
				((IServiceRequestController)controller).ShouldSendERequestDocument = false;
				controller.ShowNewForm();
			}
			else
			{
				newControllerEventHandler = (sender, eventArgs) =>
				{
					var form = sender as SupportIncidentForm;
					form.BusinessEntity.SetupForInternalReportedIncident(stage, comment, GetEdiProdLicence());
				};
				ShowNewForm();
			}
		}
		EventHandler newControllerEventHandler;

		LicenceHeader GetEdiProdLicence()
		{
			return EDIDataRegistry.Instance.InternalIncidentLicenceSettings.Value.EdiProd_Licence;
		}

		#endregion

		protected override IFilterControl GetNewFilterControl()
		{
			return new SupportIncidentFilterControl(GridCollection, FilterBusinessObject);
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return EDISecurityCheckpoints.CustomerServiceIncident; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return EDIJobInvoicingConsumerTypes.Incident.Code; }
		}
	}
}
