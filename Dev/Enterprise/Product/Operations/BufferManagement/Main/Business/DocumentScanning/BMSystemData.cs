using System;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(BMSystemData),
	Enterprise.Core.Constants.DocManagerCodes.BMSystem)]

namespace Enterprise.BufferManagement.Business
{
	class BMSystemData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(BMSystem); } }
		protected override Type CollectionType
		{
			get { return typeof(BMSystemCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new BMSystemCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.BMSystems; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.BusinessEntityProcessWorkflow; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("0af7d844-f219-41b6-a8ac-57e6d525357c", "Buffer Management System"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
