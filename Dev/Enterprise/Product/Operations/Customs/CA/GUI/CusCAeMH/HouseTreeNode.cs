using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageManagers;

namespace Enterprise.Customs.CA.GUI;

internal class HouseTreeNode : TreeNode
{
	internal HouseTreeNode(SingleMessageManager singleMessageManager)
		: base(singleMessageManager.MessageFriendlyName)
	{
		this.messageManager = singleMessageManager;
	}

	internal SingleMessageManager MessageManager
	{
		get
		{
			return messageManager;
		}
	}

	readonly SingleMessageManager messageManager;
	internal ZString HouseCCN
	{
		get
		{
			var houseBillMessageManager = MessageManager as ACIHouseBillMessageManager;
			return houseBillMessageManager == null ? ZString.Empty : houseBillMessageManager.HouseBillCCN;
		}
	}
}
