using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.BR.Business
{
	public class CusBRForeignOperatorWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.ForeignOperatorWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("CusBRForeignOperatorWorkflowDescriptor", "Foreign Operator");

		public override Type WorkflowProviderType => typeof(CusBRForeignOperator);

		public override ControllerID ControllerID => ControllerIDs.Customs.BR.ForeignOperator;

		public override bool SupportsBufferManagement => false;

		public override bool SupportsEventTracking => true;

		public override bool SupportsUniversalTemplates => false;
	}
}

