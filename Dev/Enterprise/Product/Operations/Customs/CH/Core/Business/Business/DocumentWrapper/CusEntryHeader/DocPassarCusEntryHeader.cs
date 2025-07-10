using CargoWise.EntityFramework;

namespace Enterprise.Customs.CH.Business;

public class DocPassarCusEntryHeader : DocBaseCusEntryHeader
{
	public static new DocPassarCusEntryHeader New(CusEntryHeader cusEntryHeader, BusinessObjectFactory factoryToWrap) => new DocPassarCusEntryHeader(cusEntryHeader, factoryToWrap);

	DocPassarCusEntryHeader(CusEntryHeader cusEntryHeader, BusinessObjectFactory factoryToWrap) : base(cusEntryHeader, factoryToWrap)
	{
	}
}
