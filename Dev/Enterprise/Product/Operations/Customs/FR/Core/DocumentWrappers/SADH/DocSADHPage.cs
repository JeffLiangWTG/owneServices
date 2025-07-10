using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.DocumentWrappers.SADH;

[AllowNoStaticNew]
public sealed class DocSADHPage : Enterprise.DocumentWrappers.Customs.EU.DocSADHPage
{
	DocSADHPage(BusinessObjectFactory factory, DocSADHLine line1, DocSADHLine line2, DocSADHLine line3, bool showEnlargedBox44)
		: base(factory, line1, line2, line3)
	{
		ShowEnlargedBox44 = showEnlargedBox44;
	}

	public static DocSADHPage New(BusinessObjectFactory factory, CusEntryLine entryLine, bool showEnlargedBox44, bool isUsedForFirstEntryLine)
	{
		return new DocSADHPage(factory, DocSADHLine.New(entryLine, factory, isUsedForFirstEntryLine), null, null, showEnlargedBox44);
	}

	public static DocSADHPage New(BusinessObjectFactory factory, IList<CusEntryLine> tripleEntryLines)
	{
		return new DocSADHPage(factory
			, DocSADHLine.New(tripleEntryLines[0], factory, false)
			, DocSADHLine.New(tripleEntryLines[1], factory, false)
			, DocSADHLine.New(tripleEntryLines[2], factory, false)
			, false
		);
	}

	public new DocSADHLine Line1 => (DocSADHLine)base.Line1;
	public new DocSADHLine Line2 => (DocSADHLine)base.Line2;
	public new DocSADHLine Line3 => (DocSADHLine)base.Line3;

	public ZBool ShowEnlargedBox44 { get; }
}
