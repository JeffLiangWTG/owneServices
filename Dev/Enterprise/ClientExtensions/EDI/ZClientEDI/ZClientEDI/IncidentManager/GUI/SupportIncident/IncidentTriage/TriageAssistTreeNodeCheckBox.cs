using Aga.Controls.Tree;
using Aga.Controls.Tree.NodeControls;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.Business;
#if WINZOR
using System.Net;
using System.Web;
using Microsoft.AspNetCore.Components;
#endif

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public class TriageAssistTreeNodeCheckBox : NodeCheckBox
	{
#if WINZOR
		public override MarkupString GetRenderedContent(TreeNodeAdv node, DrawContext context)
		{
			if (IsTriageWrapperNode(node))
			{
				return base.GetRenderedContent(node, context);
			}
			return new MarkupString();
		}
#else
		public override void Draw(TreeNodeAdv node, DrawContext context)
		{
			if (IsTriageWrapperNode(node))
			{
				base.Draw(node, context);
			}
		}
#endif

		static bool IsTriageWrapperNode(TreeNodeAdv node) => node.Tag is ZNode<TriageAssistTreeBizObjWrapper> tag && tag.BizObj is TriageAssistTreeTriageWrapper;
	}
}
