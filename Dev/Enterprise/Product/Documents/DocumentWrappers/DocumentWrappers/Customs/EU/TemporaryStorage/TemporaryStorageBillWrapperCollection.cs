using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.DocumentWrappers.Customs.EU.TemporaryStorage;

public class TemporaryStorageBillWrapperCollection : DocBaseWrapperCollection<TemporaryStorageBillWrapper>
{
	public TemporaryStorageBillWrapperCollection(IEnumerable<TemporaryStorageBill> bills, BusinessObjectFactory factory) : base(factory)
	{
		foreach (var item in bills)
		{
			Add(TemporaryStorageBillWrapper.New(item, factory));
		}
	}
}
