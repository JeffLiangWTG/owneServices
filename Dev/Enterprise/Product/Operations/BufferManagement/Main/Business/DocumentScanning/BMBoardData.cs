using System;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(BMBoardData),
	Enterprise.Core.Constants.DocManagerCodes.BMBoard)]

namespace Enterprise.BufferManagement.Business
{
	class BMBoardData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(BMBoard); } }
		protected override Type CollectionType
		{
			get { return typeof(BMBoardCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new BMBoardCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.BMBoard; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.BusinessEntityProcessWorkflow; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("0cf8d70d-dc3d-40e9-a9b2-1b5e6211a00a", "Visual Boards"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
