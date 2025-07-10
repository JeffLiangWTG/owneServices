using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public class AUBrokeragePluginToFreight : BrokeragePlugInOneToOne
	{
		public AUBrokeragePluginToFreight(ForwardingShipment shipment) : base(shipment)
		{
		}

		protected override Customs.Business.CreateDeclarationHelper GetCreateDeclarationHelperCore()
		{
			return new CreateDeclarationHelper();
		}

		public override ContinueWithSave ShowPreSaveDialogsCore()
		{
			ContinueWithSave result = base.ShowPreSaveDialogsCore();
			if (result == ContinueWithSave.Yes)
			{
				Customs.Business.BaseJobDeclaration declarationFromLocalCache = GetDeclarationFromShipmentFromLocalCache();
				if (declarationFromLocalCache != null && declarationFromLocalCache.HasChanges)
				{
					//only for export. CMR import jobs are handled by base Customs GUI
					if (!JobDeclaration.CanContinueWithSaveSendingAnAmendmentIfNeeded())
					{
						result = ContinueWithSave.No;
					}
				}
			}
			return result;
		}

		protected override Customs.GUI.SendsMessagesToCustomsGUI GetNewMessagingActionsController()
		{
			return new MessagingActionsController(JobDeclaration);
		}

		#region IZPlugIn Members

		protected override MenuItem GetNewTopLevelMenuCore()
		{
			if (mainMenuItem == null)
			{
				mainMenuItem = EDIMenu.New();
			}
			return mainMenuItem;
		}

		public override bool CanDelete
		{
			get { return JobDeclaration == null || JobDeclaration.CanDelete; }
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (fUserControlCreated)
			{
				((AUBrokerageUserControl)UserControl).JobDeclaration = JobDeclaration;
			}
		}

		protected override BaseCustomsBrokerageUserControl CreateBrokerageUserControl()
		{
			fUserControlCreated = true;
			return new AUBrokerageUserControl();
		}

		public override string Name
		{
			get { return "Brokerage"; }
		}

		protected override void OnJobDeclarationSet()
		{
			base.OnJobDeclarationSet();
			if (JobDeclaration != null)
			{
				JobDeclaration.DocumentRequestedWithNoCusEntryHeaders += OnDocumentRequestedWithNoCusEntryHeaders;
			}
		}

		#endregion

		#region Implementation

		EDIMenu mainMenuItem;
		bool fUserControlCreated;

		public new JobDeclaration JobDeclaration
		{
			get { return base.JobDeclaration as JobDeclaration; }
		}

		protected virtual void OnDocumentRequestedWithNoCusEntryHeaders(object sender, EventArgs e)
		{
			Globals.Message.ShowError("You must Do Community Protection before viewing this document.");
		}

		public override void OnSaveCompletedOrAborted(bool saved)
		{
			base.OnSaveCompletedOrAborted(saved);
			if (!saved && JobDeclaration != null)
			{
				JobDeclaration.RecoverFromUnsuccessfulSave();
				JobDeclaration.DeleteAnyNewMessages();
				foreach (CusEntryHeader entryHeader in JobDeclaration.CustomsEntryHeaders)
				{
					entryHeader.RecoverFromUnsuccessfulSave();
					entryHeader.DeleteAnyNewMessages();
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (HasJobDeclarationBeenLoaded)
				{
					var declaration = JobDeclaration;
					if (declaration != null)
					{
						declaration.DocumentRequestedWithNoCusEntryHeaders -= OnDocumentRequestedWithNoCusEntryHeaders;
					}
				}
			}

			base.Dispose(disposing);
		}

		#endregion

	}
}
