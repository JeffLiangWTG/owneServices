using System;
using CargoWise.Definitions;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business
{
	public class ARComplianceDocumentWorkflowDescriptor : AccComplianceDocumentWorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.ARComplianceDocumentCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("C8B9773D-A933-4493-AED5-5EE40B4CA97D", "AR Compliance Document");

		public override ControllerID ControllerID => ControllerIDs.ARComplianceDocument;

		public override Type WorkflowProviderType => typeof(ARComplianceDocumentHeader);

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new BusinessContext[] { BusinessContext.ARComplianceDocument }; }
		}
	}
}
