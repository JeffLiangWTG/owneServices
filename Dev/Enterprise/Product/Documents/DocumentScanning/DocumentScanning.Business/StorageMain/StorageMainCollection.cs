using System;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentScanning.Business
{
	public class StorageMainCollection : BusinessObjectCollection<StorageMain>
	{
		/// <summary>
		/// Creates a collection of StorageMain records
		/// </summary>
		/// <param name="factory">MUST be class Enterprise.DocumentScanning.Business.DocumentFactory</param>
		public StorageMainCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		/// <summary>
		/// Creates a collection of StorageMain records with additional filtering
		/// </summary>
		/// <param name="factory">MUST be class Enterprise.DocumentScanning.Business.DocumentFactory</param>
		[Obsolete("Calling this constructor will delete the AdditionalFilter. Please use StorageMainCollection(BusinessObjectFactory Factory) overload, and call LoadWithAdditionalFiltering() to pass your filter in.")]
		public StorageMainCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

#pragma warning disable 0809
		[Obsolete("You should use LoadWithAdditionalFiltering() to avoid deleting the Additional Filter on StorageMainCollection")]
		public override void Load(ZQuery alternativeAdditionalFilter)
		{
			base.Load(alternativeAdditionalFilter);
		}
#pragma warning restore 0809

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery result = base.CreateAdditionalFilter();
			result.AddToFilter(new CountrySpecificRefTypesQuery(), JoinCondition.And);
			return result;
		}

		protected override System.Collections.IComparer GetComparerForSort(System.ComponentModel.PropertyDescriptor property, System.ComponentModel.ListSortDirection direction)
		{
			return new StorageMainComparer(property, direction);
		}
	}
}
