using System.Collections.Generic;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.Declaration;
using EUCusEntryLineCollection = Enterprise.Customs.Business.ICusEntryLineCollection<Enterprise.Customs.EU.Business.Declaration.CusEntryLine>;
using FRCusEntryLineCollection = Enterprise.Customs.Business.ICusEntryLineCollection<Enterprise.Customs.FR.Business.Declaration.CusEntryLine>;

namespace Enterprise.Customs.FR.DocumentWrappers.SADH;

public class DocSADHPageCollection : Enterprise.DocumentWrappers.Customs.EU.DocSADHPageCollection
{
	public DocSADHPageCollection(FRCusEntryLineCollection collection, BusinessObjectFactory factory)
		: base(collection, factory)
	{
	}

	protected DocSADHPageCollection(BusinessObjectFactory factory)
		: base(factory)
	{
	}

	protected override void GenerateDocSADHCollection(EUCusEntryLineCollection collection, BusinessObjectFactory factory)
	{
		if (collection.FirstOrDefault() is CusEntryLine fRCusEntryLine)
		{
			var page = NewRegularBox44Page(factory, fRCusEntryLine, isUsedForFirstEntryLine: true);
			Add(page);
			if (page.Line1.NeedASecondPageForBox44)
			{
				Add(NewEnlargedBox44Page(factory, fRCusEntryLine, isUsedForFirstEntryLine: true));
			}
		}

		var tripleEntryLines = collection.Cast<CusEntryLine>().Skip(1).Chunk(3).Select(x => x.ToList()).ToList();
		if (tripleEntryLines.Count > 0)
		{
			tripleEntryLines[tripleEntryLines.Count - 1].AddRange(Enumerable.Repeat<CusEntryLine>(null, 3 - tripleEntryLines.Last().Count));
			foreach (var tripleEntryLine in tripleEntryLines)
			{
				var page = NewPage(factory, tripleEntryLine);
				Add(page);
				if (page.Line1.NeedASecondPageForBox44)
				{
					Add(NewEnlargedBox44Page(factory, tripleEntryLine[0], isUsedForFirstEntryLine: false));
				}
				if (page.Line2?.NeedASecondPageForBox44 ?? false)
				{
					Add(NewEnlargedBox44Page(factory, tripleEntryLine[1], isUsedForFirstEntryLine: false));
				}
				if (page.Line3?.NeedASecondPageForBox44 ?? false)
				{
					Add(NewEnlargedBox44Page(factory, tripleEntryLine[2], isUsedForFirstEntryLine: false));
				}
			}
		}
	}

	DocSADHPage NewRegularBox44Page(BusinessObjectFactory factory, CusEntryLine entryLine, bool isUsedForFirstEntryLine) => DocSADHPage.New(factory, entryLine, false, isUsedForFirstEntryLine);

	DocSADHPage NewEnlargedBox44Page(BusinessObjectFactory factory, CusEntryLine entryLine, bool isUsedForFirstEntryLine) => DocSADHPage.New(factory, entryLine, true, isUsedForFirstEntryLine);

	DocSADHPage NewPage(BusinessObjectFactory factory, IList<CusEntryLine> tripleEntryLines) => DocSADHPage.New(factory, tripleEntryLines);

	public new DocSADHPage AddNew() => (DocSADHPage)base.AddNew();

	public new DocSADHPage this[int index] => (DocSADHPage)base[index];
}
