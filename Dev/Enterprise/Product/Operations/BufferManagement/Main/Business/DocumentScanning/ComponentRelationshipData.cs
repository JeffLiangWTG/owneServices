using System;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(ComponentRelationshipData),
	Enterprise.Core.Constants.DocManagerCodes.ComponentRelationship)]

namespace Enterprise.BufferManagement.Business
{
	class ComponentRelationshipData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(ComponentRelationship);

		public override string ReferenceType => Core.Constants.ReferenceTypes.BusinessEntityProcessWorkflow;

		protected override Type CollectionType => typeof(ComponentRelationshipCollection);
		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("9496A411-4FEB-4ECB-9AD0-0338428434C8", "Component Relationship");
	}
}
