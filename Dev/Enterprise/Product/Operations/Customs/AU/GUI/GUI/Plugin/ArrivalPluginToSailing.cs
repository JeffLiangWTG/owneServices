using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.AU.Sailing.GUI
{
	public class ArrivalPluginToSailing : ZAlwaysLoadPlugIn
	{
		public ArrivalPluginToSailing(CustomsJobVoyageWrapper voyageWrapper) : base(voyageWrapper.Voyage)
		{
			businessObject = voyageWrapper;
			Enabled = voyageWrapper.Voyage.IsAir && new CMRUtilities().AreWeRunningInCMR(ZDateTime.Now);
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.AlwaysAllow; }
		}

		public override bool CanDelete
		{
			get { return businessObject.Voyage.Messages.Count == 0; }
		}

		public override void Delete()
		{
			businessObject.Voyage.Delete();
		}

		public override string Name
		{
			get { return "Arrival Reporting"; }
		}

		protected override Control GetNewUserControl()
		{
			return new SailingPluginUserControl();
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		public override string CannotDeleteMessage
		{
			get { return "Cannot delete this record as it has messages associated with it and it needs to be retained for audit trail purposes."; }
		}

		readonly CustomsJobVoyageWrapper businessObject;
		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return businessObject;
		}

		MenuItem topLevelMenu;
		protected override MenuItem GetNewTopLevelMenu()
		{
			if (topLevelMenu == null)
			{
				topLevelMenu = new ArrivalPluginMenu(Manager);
			}
			return topLevelMenu;
		}

		Business.MultiMessageManager manager;
		protected internal Business.MultiMessageManager Manager
		{
			get
			{
				if (manager == null)
				{
					var voyageWrapper = businessObject;
					manager = new JobVoyageMessageManager(voyageWrapper);
				}
				return manager;
			}
		}

		public override ContinueWithSave ShowPreSaveDialogsCore()
		{
			var result = base.ShowPreSaveDialogsCore();
			if (result == ContinueWithSave.Yes)
			{
				result = new SendsMessagesToCustomsGUI().DetermineRequiredMessagesAndSendThem(Manager);
			}
			return result;
		}

		public override void OnUserControlShown()
		{
			base.OnUserControlShown();
			businessObject.Validation.ValidateAll();
		}
	}
}
