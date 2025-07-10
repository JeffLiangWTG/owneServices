using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageManagers;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	internal class CloseReportTreeNode : HouseTreeNode
	{
		internal CloseReportTreeNode(ACIForwarderCloseMessageManager closeMessageManager)
			: base(closeMessageManager)
		{
			if (closeMessageManager.ActionPurpose != ActionPurpose.Withdraw)
			{
				foreach (var houseCCN in closeMessageManager.AllWrappedCCN)
				{
					this.Nodes.Add(new CloseReportHouseTreeNode(houseCCN));
				}
			}
		}

		internal static ZString CaptionStringCloseReport
		{
			get { return Res.GetString("08f10509-9469-4f5a-8578-0c293d745273", "Close Reports"); }
		}
	}
}
