using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Messaging.Module
{
	public class EDICommunicationPartyModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Messaging.EDICommunicationParty; }
		}

		public override bool AllowNew
		{
			get { return true; }
		}

		public override bool AllowEdit
		{
			get { return true; }
		}

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Messaging.EDICommunicationParty);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new EDICommunicationPartyFilterControl((EDICommunicationPartyCollection)GridCollection, (EDICommunicationPartyFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new EDICommunicationPartyCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new EDICommunicationPartyFilterBusinessObject();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		// TODO: Menu items are incorrect, to be updated
		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.EDICommunicationParty; }
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());
			if (GlbStaff.CurrentUser.IsSupportUser)
			{
				result.Add(new ZMenuItem(EDIMessageModule.ResetStatusToQueuedMenuName, new EventHandler(ResetToQueued_Click)));
			}

			result.Add(new ZMenuItem((NoResString)"Update Organizations EDI Client", new EventHandler(UpdateOrganizationEDIClient_Click)));

			return result.ToArray();
		}

		protected void ResetToQueued_Click(object sender, EventArgs e)
		{
			EDIMessageModule.ResetToQueuedClickHandler(Grid, Factory);
		}

		protected void UpdateOrganizationEDIClient_Click(object sender, EventArgs e)
		{
			CommunicationModeMigratorForm form = null;
			if (Grid.SelectedElements.Length == 1)
			{
				form = new CommunicationModeMigratorForm(Factory, new CommunicationModeMigratorDataModel(Factory, Grid.SelectedElements[0].PK));
			}
			else
			{
				form = new CommunicationModeMigratorForm(Factory, new CommunicationModeMigratorDataModel(Factory));
			}
			try
			{
				form.Show();
			}
			catch
			{
				try
				{
					form.Dispose();
				}
				catch { }
				throw;
			}
		}
	}
}
