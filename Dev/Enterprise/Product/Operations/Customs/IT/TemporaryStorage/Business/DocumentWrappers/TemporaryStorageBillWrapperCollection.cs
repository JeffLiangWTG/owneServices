using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public class TemporaryStorageBillWrapperCollection : DocBaseWrapperCollection<DocumentWrappers.Customs.EU.TemporaryStorage.TemporaryStorageBillWrapper>
{
	public TemporaryStorageBillWrapperCollection(IEnumerable<TemporaryStorageBill> bills, BusinessObjectFactory factory) : base(factory)
	{
		foreach (var item in bills)
		{
			Add(TemporaryStorageBillWrapper.New(item, factory));
		}
	}
}
