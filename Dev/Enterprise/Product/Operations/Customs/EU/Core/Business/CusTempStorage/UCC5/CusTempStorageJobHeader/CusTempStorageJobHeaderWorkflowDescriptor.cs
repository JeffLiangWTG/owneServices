using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class CusTempStorageJobHeaderWorkflowDescriptor : WorkflowDescriptor
	{
		#region Identification

		const string workflowTypeCode = "CTJ";

		public override string Code => workflowTypeCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("38A9CDD1-C31E-46E5-A757-BE70E406872B", "Customs Temp. Storage Job Header");

		public override ControllerID ControllerID => ControllerIDs.Customs.TemporaryStorage;

		public override System.Type WorkflowProviderType => typeof(CusTempStorageJobHeader);

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
