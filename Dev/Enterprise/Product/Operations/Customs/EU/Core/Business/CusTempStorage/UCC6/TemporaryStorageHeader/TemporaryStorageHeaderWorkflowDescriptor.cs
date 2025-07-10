using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageHeaderWorkflowDescriptor : WorkflowDescriptor
	{
		#region Identification

		const string workflowTypeCode = "TSH";

		public override string Code => workflowTypeCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("2DAD5843-8A6D-4711-AF1C-0EA8EF18AFE4", "UCC6 Temporary Storage Header");

		public override ControllerID ControllerID => ControllerIDs.Customs.EU.UCC6TemporaryStorage;

		public override System.Type WorkflowProviderType => typeof(TemporaryStorageHeader);

		#endregion

		#region Capapibilities

		public override bool RequiresClient => false;

		public override bool RequiresBranch => true;

		public override bool RequiresDepartment => true;

		public override bool SupportsEventTracking => true;

		public override bool SupportsBufferManagement => false;

		public override bool SupportsUniversalTemplates => false;

		#endregion
	}
}
