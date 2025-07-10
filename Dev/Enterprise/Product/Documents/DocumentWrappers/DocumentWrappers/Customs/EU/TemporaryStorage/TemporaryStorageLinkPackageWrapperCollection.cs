using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.DocumentWrappers.Customs.EU.TemporaryStorage;

public class TemporaryStorageLinkPackageWrapperCollection : DocBaseWrapperCollection<TemporaryStorageLinkPackageWrapper>
{
	public TemporaryStorageLinkPackageWrapperCollection(IEnumerable<TemporaryStorageLinkPackage> linkPackages, BusinessObjectFactory factory) : base(factory)
	{
		_ = Argument.NotNull(linkPackages, nameof(linkPackages));
		_ = Argument.NotNull(factory, nameof(factory));
		linkPackages.ForEach(package => Add(TemporaryStorageLinkPackageWrapper.New(package, factory)));
	}
}
