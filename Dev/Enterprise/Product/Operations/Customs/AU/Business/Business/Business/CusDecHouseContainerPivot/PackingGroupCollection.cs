using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class PackingGroupCollection : BaseDeclarationLevelPackingGroupCollection
	{
		public PackingGroupCollection(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
		}

		public new PackingGroup this[int index]
		{
			get { return (PackingGroup)Elements[index]; }
		}

		public new PackingGroup AddNew()
		{
			return (PackingGroup)base.AddNew();
		}

		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		#region TotalNumberOfPackages

		public ZInt TotalNumberOfPackages => Factory.GetValue(ref totalNumberOfPackagesCache, GetTotalNumberOfPackages);

		CachedProperty<ZInt> totalNumberOfPackagesCache;

		ZInt GetTotalNumberOfPackages()
		{
			ZInt result = 0;

			try
			{
				foreach (PackingGroup currentPack in this)
				{
					result += currentPack.TotalNumberOfPackages;
				}
			}
			catch (OverflowException)
			{
				result = int.MaxValue;
			}

			return result;
		}

		#endregion

		#region TotalWarehouseNumberOfPackages

		public ZInt TotalWarehouseNumberOfPackages => Factory.GetValue(ref totalWarehouseNumberOfPackagesCache, GetTotalWarehouseNumberOfPackages);

		CachedProperty<ZInt> totalWarehouseNumberOfPackagesCache;

		ZInt GetTotalWarehouseNumberOfPackages()
		{
			ZInt result = 0;

			foreach (PackingGroup currentPack in this)
			{
				result += currentPack.WarehouseNumberOfPackages;
			}
			return result;
		}

		#endregion

		#region TotalPackingUnitCount

		public ZInt TotalPackingUnitCount => Factory.GetValue(ref totalPackingUnitCountCache, GetTotalPackingUnitCount);
		CachedProperty<ZInt> totalPackingUnitCountCache;

		ZInt GetTotalPackingUnitCount()
		{
			ZInt result = 0;

			foreach (PackingGroup currentPack in this)
			{
				result += currentPack.OuterPackingUnitCount;
			}
			return result;
		}

		#endregion
	}
}
