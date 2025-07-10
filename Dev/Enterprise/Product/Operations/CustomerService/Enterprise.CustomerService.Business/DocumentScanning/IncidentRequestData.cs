using Enterprise.CustomerService.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(IncidentRequestData),
	Enterprise.Core.Constants.DocManagerCodes.IncidentRequest)]

namespace Enterprise.CustomerService.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	class IncidentRequestData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(IncidentRequest); } }
		protected override Type CollectionType
		{
			get { return typeof(IncidentRequestCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new IncidentRequestCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.ServiceRequest; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.BusinessEntityProcessWorkflow; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("47A0FF9F-8525-4729-9CAE-0EA105E2402D", "Incident Request"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
