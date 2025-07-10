using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.MasterFiles
{
	public class EdiAddRelatedManagementOrganisationModuleDecisionProvider : AddRelatedManagementOrganisationModuleDecisionProvider
	{
		public EdiAddRelatedManagementOrganisationModuleDecisionProvider(ZNode<Enterprise.MasterFiles.Business.OrgHeader> parentOrganisationNode) : base(parentOrganisationNode)
		{
		}

		protected override bool TryAddChildren(IEnumerable<Enterprise.MasterFiles.Business.OrgHeader> children)
		{
			var factory = parentNode.BizObj.Factory;
			var warningMessage = EDIOrgHeaderValidationHelper.ValidateENTCodeForOrganisationNode(parentNode, children, factory);
			if (!string.IsNullOrEmpty(warningMessage) && Globals.Message.Show(warningMessage, "Warning Message - Valid ENT Code", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
			{
				return false;
			}
			return base.TryAddChildren(children);
		}
	}
}
