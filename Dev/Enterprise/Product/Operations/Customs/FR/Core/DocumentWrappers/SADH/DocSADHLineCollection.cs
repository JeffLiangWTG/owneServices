using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.Declaration;
using FRCusEntryLineCollection = Enterprise.Customs.Business.ICusEntryLineCollection<Enterprise.Customs.FR.Business.Declaration.CusEntryLine>;

namespace Enterprise.Customs.FR.DocumentWrappers.SADH;

public sealed class DocSADHLineCollection : Enterprise.DocumentWrappers.Customs.EU.DocSADHLineCollection
{
	public DocSADHLineCollection(FRCusEntryLineCollection entryLineCollection, BusinessObjectFactory factory)
		: this(entryLineCollection.Cast<CusEntryLine>(), factory)
	{
	}

	public DocSADHLineCollection(IEnumerable<CusEntryLine> entryLineCollection, BusinessObjectFactory factory)
		: base(factory)
	{
		foreach (var entryLine in entryLineCollection)
		{
			Add(DocSADHLine.New(entryLine, factory));
		}
	}

	public new DocSADHLine AddNew() => (DocSADHLine)base.AddNew();

	public new DocSADHLine this[int index] => (DocSADHLine)base[index];
}
