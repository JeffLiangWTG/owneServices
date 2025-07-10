using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business.MessageManagers
{
	public class CAMessageChooserNonPersistent : MessageChooserNonPersistent
	{
		public CAMessageChooserNonPersistent(SingleMessageManager[] managers, ZString question, ActionPurpose actionPurpose)
			: base(managers, question)
		{
			UpdateCloseReportManagers(actionPurpose);
		}

		public CAMessageChooserNonPersistent(SingleMessageManager[] managers, ZString question, ZString action, ActionPurpose actionPurpose)
			: base(managers, question, action)
		{
			UpdateCloseReportManagers(actionPurpose);
		}
		void UpdateCloseReportManagers(ActionPurpose actionPurpose)
		{
			foreach (SingleMessageManager manager in Managers)
			{
				MessageManagersToSend.Add(manager, false);
				var closeMessageManager = manager as ACIForwarderCloseMessageManager;
				if (closeMessageManager != null)
				{
					closeMessageManager.ActionPurpose = actionPurpose;
					closeMessageManager.isHouseBillIncludedToSend = (houseCCN) => this.SelectedManagers.OfType<ACIHouseBillMessageManager>().Any(houseManager => houseManager.HouseBillCCN == houseCCN);
				}
			}
		}

		#region MessageManagersToSend

		Dictionary<SingleMessageManager, ZBool> messageManagersToSend;
		public Dictionary<SingleMessageManager, ZBool> MessageManagersToSend
		{
			get
			{
				if (messageManagersToSend == null)
				{
					messageManagersToSend = new Dictionary<SingleMessageManager, ZBool>();
				}
				return messageManagersToSend;
			}
		}

		#endregion

		public override SingleMessageManager[] SelectedManagers
		{
			get
			{
				return MessageManagersToSend.Where(toSend => toSend.Value).Select(toSend => toSend.Key).ToArray();
			}
		}

		public override void SelectAll()
		{
		}

		public override void DelselectAll()
		{
		}
	}
}
