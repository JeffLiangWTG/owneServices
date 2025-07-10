using Enterprise.CustomerService.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(IncidentApprovalData),
	Enterprise.Core.Constants.DocManagerCodes.IncidentApproval)]

namespace Enterprise.CustomerService.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	public class IncidentApprovalData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(IncidentApproval); } }
		protected override Type CollectionType
		{
			get { return typeof(IncidentApprovalCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new IncidentApprovalCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.ServiceRequest; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.BusinessEntityProcessWorkflow; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("b932352a-fcf6-4bae-b7f9-f4e1106b6992", "eRequest"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
