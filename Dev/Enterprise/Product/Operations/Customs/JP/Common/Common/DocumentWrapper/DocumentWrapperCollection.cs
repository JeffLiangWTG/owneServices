using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.JP.Common;

public class DocumentWrapperCollection<T1, T2> : DocumentWrapperCollection<T1>
	where T1 : DocumentWrapper
{
	public DocumentWrapperCollection(IEnumerable<T2> itemProviders, BusinessObjectFactory factory, bool includingIndex = false) : base(factory)
	{
		if (includingIndex)
		{
			itemProviders?.ForEach((item, i) => Add((T1)Activator.CreateInstance(typeof(T1), item, i + 1)));
		}
		else
		{
			itemProviders?.ForEach(item => Add((T1)Activator.CreateInstance(typeof(T1), item)));
		}
	}
}
