using System;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(BMControlCustomisationData),
	Enterprise.Core.Constants.DocManagerCodes.BMControlCustomisation)]

namespace Enterprise.BufferManagement.Business
{
	class BMControlCustomisationData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(BMControlCustomisation); } }
		protected override Type CollectionType
		{
			get { return typeof(BMControlCustomisationCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new BMControlCustomisationCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.BMControlCustomisation; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.BusinessEntityProcessWorkflow; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("1d9a3e44-c7bd-4c9d-8e2a-1566c56615c8", "Control Customization"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
