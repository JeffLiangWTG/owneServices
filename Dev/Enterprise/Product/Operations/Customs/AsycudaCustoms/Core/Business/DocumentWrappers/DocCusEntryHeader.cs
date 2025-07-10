using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class DocCusEntryHeader : DocBaseCusEntryHeader
	{
		DocCusEntryHeader(CusEntryHeader cusEntryHeader, BusinessObjectFactory factoryToWrap)
			: base(cusEntryHeader, factoryToWrap)
		{
		}

		public static DocCusEntryHeader New(CusEntryHeader cusEntryHeader, BusinessObjectFactory factoryToWrap) => new DocCusEntryHeader(cusEntryHeader, factoryToWrap);
	}
}
