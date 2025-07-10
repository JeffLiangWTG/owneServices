using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.DocumentWrappers
{
	public class DocSADHPage : Enterprise.DocumentWrappers.Customs.EU.DocSADHPage
	{
		protected DocSADHPage(BusinessObjectFactory factory, DocSADHLine line1, DocSADHLine line2, DocSADHLine line3)
			: base(factory, line1, line2, line3)
		{
		}

		public new static DocSADHPage New(BusinessObjectFactory factory, EU.Business.Declaration.CusEntryLine entryLine1)
		{
			return new DocSADHPage(factory, DocSADHLine.New(entryLine1, factory), null, null);
		}

		public new static DocSADHPage New(BusinessObjectFactory factory, ICusEntryLineCollection<EU.Business.Declaration.CusEntryLine> entryLines, int startFrom)
		{
			var line1 = GetElementSafe(entryLines, startFrom, factory);
			var line2 = GetElementSafe(entryLines, startFrom + 1, factory);
			var line3 = GetElementSafe(entryLines, startFrom + 2, factory);
			return new DocSADHPage(factory, line1, line2, line3);
		}

		static DocSADHLine GetElementSafe(ICusEntryLineCollection<EU.Business.Declaration.CusEntryLine> entryLines, int index, BusinessObjectFactory factory)
		{
			var entryLine = index < entryLines.Count ? entryLines[index] : null;
			return DocSADHLine.New(entryLine, factory);
		}
	}
}
