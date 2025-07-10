using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(typeof(UnallocatedData), Enterprise.Core.Constants.DocManagerCodes.Unallocated)]

namespace Enterprise.DocumentScanning.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	public class UnallocatedData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(Enterprise.DocumentScanning.Business.StorageDocsUnallocated); } }
		protected override Type CollectionType
		{
			get { return typeof(NonPersistentUnallocatedObjectCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new NonPersistentUnallocatedObjectCollection((DocumentFactory)factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.NotAssigned; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.Unallocated; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("9a5b1c2f-ea34-41b8-a85f-0d50fa5bea88", "Unallocated"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
