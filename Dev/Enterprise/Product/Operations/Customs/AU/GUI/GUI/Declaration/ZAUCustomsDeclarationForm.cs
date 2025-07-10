using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public class ZAUCustomsDeclarationForm : BaseJobDeclarationForm
	{
		protected ZAUCustomsDeclarationForm()
		{
		}

		public ZAUCustomsDeclarationForm(JobDeclaration declaration)
			: base(declaration)
		{
			DataContext = Core.Constants.DataContext.AUCustoms;
			declaration.DocumentRequestedWithNoCusEntryHeaders += new EventHandler(OnDocumentRequestedWithNoCusEntryHeaders);
		}

		protected void OnDocumentRequestedWithNoCusEntryHeaders(object sender, EventArgs e)
		{
			Globals.Message.ShowError("You must enter Community Protection details before viewing this document.");
		}

		public new AUBrokerageUserControl CustomsBrokerageUserControl
		{
			get { return (AUBrokerageUserControl)fCustomsBrokerageUserControl; }
		}

		protected override BaseCustomsBrokerageUserControl GetBrokerageUserControl()
		{
			return new AUBrokerageUserControl();
		}

		protected override IEDIMenu GetNewTopLevelMenuCore()
		{
			return EDIMenu.New();
		}

		#region Implementation

		protected bool sendAmendment;
		readonly System.ComponentModel.Container components;

		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)fJobDeclaration; }
		}

		protected override ZTabControl TopLevelTabControl
		{
			get { return fCustomsBrokerageUserControl != null ? CustomsBrokerageUserControl.MainTabControl : null; }
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}

				Declaration.DocumentRequestedWithNoCusEntryHeaders -= new EventHandler(OnDocumentRequestedWithNoCusEntryHeaders);
			}
			base.Dispose(disposing);
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			ContinueWithSave result = base.ShowPreSaveDialogs();
			if (result == ContinueWithSave.Yes)
			{
				//only for export jobs. CMR Import jobs are handled by Customs GUI
				result = Declaration.CanContinueWithSaveSendingAnAmendmentIfNeeded() ? result : ContinueWithSave.No;
			}
			return result;
		}

		protected override Customs.GUI.SendsMessagesToCustomsGUI GetNewMessagingActionsController()
		{
			return new MessagingActionsController(Declaration);
		}

		#endregion

	}
}
