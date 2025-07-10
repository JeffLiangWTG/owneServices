using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public class TemporaryStorageLinkPackageCollection<T>
	: EU.Business.CusTempStorage.TemporaryStorageLinkPackageCollection<T>
	where T : TemporaryStorageLinkPackage
{
	public TemporaryStorageLinkPackageCollection(TemporaryStoragePackedItem packedItem)
		: base(packedItem)
	{
	}

	CachedProperty<bool> mismatchingRowsCached;

	public bool HasMismatchingPackUQLinkedRows =>
		Factory.GetValue(ref mismatchingRowsCached, () =>
		{
			var linkedWithPackUQ = this
				.Where(item => item.IsLinked && item.Package?.APA_PackUQ.IsEmpty == false)
				.ToList();

			if (linkedWithPackUQ.Count == 0)
			{
				return false;
			}

			var groups = linkedWithPackUQ
				.GroupBy(item => item.Package.APA_PackUQ)
				.ToList();

			return groups.Count > 1;
		});
}
