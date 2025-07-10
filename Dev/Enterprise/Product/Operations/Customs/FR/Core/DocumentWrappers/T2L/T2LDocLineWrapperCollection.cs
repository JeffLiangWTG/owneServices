using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.DocumentWrappers;
using FRCusEntryLineCollection = Enterprise.Customs.Business.ICusEntryLineCollection<Enterprise.Customs.FR.Business.Declaration.CusEntryLine>;

namespace Enterprise.Customs.FR.DocumentWrappers.Transit;

public class T2LDocLineWrapperCollection : DocBaseWrapperCollection<T2LDocLineWrapper>
{
	public T2LDocLineWrapperCollection(FRCusEntryLineCollection entryLineCollection, BusinessObjectFactory factory, bool isForT2LF) : this(entryLineCollection.Cast<CusEntryLine>(), factory, isForT2LF)
	{
	}

	public T2LDocLineWrapperCollection(IEnumerable<CusEntryLine> entryLineCollection, BusinessObjectFactory factory, bool isForT2LF) : base(factory)
	{
		foreach (var entryLine in entryLineCollection)
		{
			Add(T2LDocLineWrapper.New(entryLine, factory, isForT2LF: isForT2LF));
		}
	}
}
