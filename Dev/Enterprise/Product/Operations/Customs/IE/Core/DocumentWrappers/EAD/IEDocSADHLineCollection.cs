using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.EU;
using IECusEntryLine = Enterprise.Customs.IE.Business.Declaration.CusEntryLine;
using IECusEntryLineCollection = Enterprise.Customs.Business.ICusEntryLineCollection<Enterprise.Customs.IE.Business.Declaration.CusEntryLine>;

namespace Enterprise.Customs.IE.DocumentWrappers
{
	sealed class IEDocSADHLineCollection : DocSADHLineCollection
	{
		public IEDocSADHLineCollection(IECusEntryLineCollection entryLineCollection, BusinessObjectFactory factory)
			: this(entryLineCollection.Cast<IECusEntryLine>(), factory)
		{
		}

		public IEDocSADHLineCollection(IEnumerable<IECusEntryLine> entryLineCollection, BusinessObjectFactory factory)
			: base(factory)
		{
			foreach (var entryLine in entryLineCollection)
			{
				Add(IEDocSADHLine.New(entryLine, factory));
			}
		}

		public new IEDocSADHLine AddNew() => (IEDocSADHLine)base.AddNew();

		public new IEDocSADHLine this[int index] => (IEDocSADHLine)base[index];
	}
}
