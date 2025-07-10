using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.IN.Business;

[WTG.StaticAnalysis.Annotation.CodeAlive("Used in documents DataContext")]
public class DocCusEntryHeader : DocBaseCusEntryHeader
{
	DocCusEntryHeader(CusEntryHeader cusEntryHeader, BusinessObjectFactory factoryToWrap)
		: base(cusEntryHeader, factoryToWrap)
	{
	}

	public static DocCusEntryHeader New(CusEntryHeader cusEntryHeader, BusinessObjectFactory factoryToWrap)
	{
		return (cusEntryHeader == null) ? null : new DocCusEntryHeader(cusEntryHeader, factoryToWrap);
	}
}
