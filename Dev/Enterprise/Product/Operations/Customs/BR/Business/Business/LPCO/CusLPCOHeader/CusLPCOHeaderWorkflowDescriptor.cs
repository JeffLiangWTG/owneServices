using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.BR.Business
{
	public class CusLPCOHeaderWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.CusBRLPCOHeaderWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("CusBRLPCOHeaderWorkflowDescriptor", "BR LPCO");

		public override Type WorkflowProviderType => typeof(CusLPCOHeader);

		public override ControllerID ControllerID => ControllerIDs.Customs.BR.LPCO;

		public override bool SupportsBufferManagement => false;

		public override bool SupportsEventTracking => true;

		public override bool SupportsUniversalTemplates => false;
	}
}
