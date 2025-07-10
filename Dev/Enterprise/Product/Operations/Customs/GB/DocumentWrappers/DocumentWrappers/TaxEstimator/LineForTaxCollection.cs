using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.GB.DocumentWrappers
{
	public sealed class LineForTaxCollection : DocBaseWrapperCollection<LineForTax>
	{
		public LineForTaxCollection(Customs.Business.ICusEntryLineCollection<CusEntryLine> collection, BusinessObjectFactory factory)
			: base(factory)
		{
			foreach (CusEntryLine entryLine in collection)
			{
				Add(new LineForTax(entryLine, factory));
			}
		}
	}
}
