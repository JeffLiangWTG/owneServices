using System;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(BMNCNShapeData),
	Constants.DocManagerCodes.NetworkDiagram)]
namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	class BMNCNShapeData : AssemblyData
	{
		public override Type BusinessObjectType
		{
			get { return typeof(BMNCNShape); }
		}

		protected override Type CollectionType
		{
			get { return typeof(BMNCNShapeCollection); }
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new BMNCNShapeCollection(factory);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.NetworkDiagram; }
		}

		public override string ReferenceType
		{
			get { return Constants.ReferenceTypes.BusinessEntityProcessWorkflow; }
		}

		public override MultilingualString HumanReadableName
		{
			get { return ResString.GetMultilingualString("11f2fc42-099a-4c85-8009-f3a061b4bb4a", "Network Diagram"); }
		}

		public override bool IsAllowedForUnallocatedeDocs
		{
			get { return true; }
		}
	}
}
