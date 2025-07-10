using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.DocumentWrappers.Customs.EU.TemporaryStorage;

public class TemporaryStoragePackedItemWrapperCollection : DocBaseWrapperCollection<TemporaryStoragePackedItemWrapper>
{
	public TemporaryStoragePackedItemWrapperCollection(IEnumerable<TemporaryStoragePackedItem> packedItems, BusinessObjectFactory factory) : base(factory)
	{
		_ = Argument.NotNull(packedItems, nameof(packedItems));
		_ = Argument.NotNull(factory, nameof(factory));
		packedItems.ForEach(item => Add(TemporaryStoragePackedItemWrapper.New(item, factory)));
	}
}
