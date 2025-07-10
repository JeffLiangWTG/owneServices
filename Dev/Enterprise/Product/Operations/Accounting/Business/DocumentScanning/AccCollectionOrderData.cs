using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(CollectionOrderData),
	Enterprise.Core.Constants.DocManagerCodes.CollectionOrder)]

namespace Enterprise.Accounting.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.Accounting.Business.Riba;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;

	public class CollectionOrderData : AssemblyData
	{
		public override Type BusinessObjectType
		{
			get
			{
				return typeof(AccCollectionOrder);
			}
		}

		protected override Type CollectionType
		{
			get
			{
				return typeof(AccCollectionOrderCollection);
			}
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new AccCollectionOrderCollection(factory);
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.AccCollectionOrder;
			}
		}

		public override string ReferenceType
		{
			get
			{
				return Core.Constants.ReferenceTypes.Accounting;
			}
		}

		public override MultilingualString HumanReadableName
		{
			get
			{
				return ResString.GetMultilingualString("0DFDA68B-FCFE-4BBB-A38A-8B4538242243", "Collection Order");
			}
		}

		public override bool IsAllowedForUnallocatedeDocs
		{
			get
			{
				return true;
			}
		}
	}
}
