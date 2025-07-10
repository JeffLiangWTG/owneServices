using System;
using CargoWise.Definitions;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business
{
	public class APComplianceDocumentWorkflowDescriptor : AccComplianceDocumentWorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.APComplianceDocumentCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("FBABAA98-2FA9-495C-B7F2-A81EB958F49A", "AP Compliance Document");

		public override ControllerID ControllerID => ControllerIDs.APComplianceDocument;

		public override Type WorkflowProviderType => typeof(APComplianceDocumentHeader);

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new BusinessContext[] { BusinessContext.APComplianceDocument }; }
		}
	}
}
