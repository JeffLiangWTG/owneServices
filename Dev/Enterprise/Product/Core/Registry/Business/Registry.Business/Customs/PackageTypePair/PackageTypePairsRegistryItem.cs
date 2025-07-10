using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Customs
{
	public abstract class PackageTypePairsRegistryItem<T> : StronglyTypedRegistryItem<PackageTypePairCollection<T>>
		where T : PackageTypePair
	{
		public PackageTypePairsRegistryItem(IRegistryItem inner)
			: base(inner)
		{
		}

		protected override void SetValueCore(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, object newValue)
		{
			base.SetValueCore(companyOrOwnerPK, branchPK, departmentPK, GetSortedCollection(newValue as PackageTypePairCollection<T>));
		}

		protected override object GetValueWithoutFallbackCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return GetSortedCollection(base.GetValueWithoutFallbackCore(companyPK, branchPK, departmentPK) as PackageTypePairCollection<T>);
		}

		PackageTypePairCollection<T> GetSortedCollection(PackageTypePairCollection<T> collection)
		{
			if (collection != null)
			{
				collection.Sort<T>(new Comparison<T>(
					delegate(T a, T b)
					{
						int result = a.CustomsPackageTypeDescription.CompareTo(b.CustomsPackageTypeDescription);

						if (result == 0)
						{
							result = a.FreightPackageTypeDescription.CompareTo(b.FreightPackageTypeDescription);
						}

						return result;
					}));
			}
			return collection;
		}
	}
}
