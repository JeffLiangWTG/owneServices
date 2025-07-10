using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.EU;
using ITCusEntryLine = Enterprise.Customs.IT.Business.Declaration.CusEntryLine;
using ITCusEntryLineCollection = Enterprise.Customs.Business.ICusEntryLineCollection<Enterprise.Customs.IT.Business.Declaration.CusEntryLine>;

namespace Enterprise.Customs.IT.Business;

public sealed class ITDocSADHLineCollection : DocSADHLineCollection
{
	public ITDocSADHLineCollection(ITCusEntryLineCollection entryLineCollection, BusinessObjectFactory factory)
		: this(entryLineCollection.Cast<ITCusEntryLine>(), factory)
	{
	}

	public ITDocSADHLineCollection(IEnumerable<ITCusEntryLine> entryLineCollection, BusinessObjectFactory factory)
		: base(factory)
	{
		foreach (var entryLine in entryLineCollection)
		{
			Add(ITDocSADHLine.New(entryLine, factory));
		}
	}

	public new ITDocSADHLine AddNew() => (ITDocSADHLine)base.AddNew();

	public new ITDocSADHLine this[int index] => (ITDocSADHLine)base[index];
}
