using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.GB.DocumentWrappers
{
	public class DocSADH : Enterprise.DocumentWrappers.Customs.EU.DocSADH
	{
		protected DocSADH(CusEntryHeader entryHeader, BusinessObjectFactory factoryToWrap)
			: base(entryHeader, factoryToWrap)
		{
		}

		public static new DocSADH New(CusEntryHeader entryHeader, BusinessObjectFactory factoryToWrap)
		{
			return new DocSADH(entryHeader, factoryToWrap);
		}

		protected override Enterprise.DocumentWrappers.Customs.EU.DocSADHPageCollection GetPagesCore()
		{
			return new DocSADHPageCollection(EntryHeader.MergedLines, Factory);
		}

		protected override Enterprise.DocumentWrappers.Customs.EU.DocSADHLineCollection GetLinesCore()
		{
			return new DocSADHLineCollection(EntryHeader.MergedLines, Factory);
		}
	}
}
