using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.EU;
using ESCusEntryLine = Enterprise.Customs.ES.Business.Declaration.CusEntryLine;
using ESCusEntryLineCollection = Enterprise.Customs.Business.ICusEntryLineCollection<Enterprise.Customs.ES.Business.Declaration.CusEntryLine>;

namespace Enterprise.Customs.ES.DocumentWrappers.SADH
{
	public sealed class ESDocSADHLineCollectionExport : DocSADHLineCollection
	{
		public ESDocSADHLineCollectionExport(ESCusEntryLineCollection entryLineCollection, BusinessObjectFactory factory)
			: this(entryLineCollection.Cast<ESCusEntryLine>(), factory)
		{
		}

		public ESDocSADHLineCollectionExport(IEnumerable<ESCusEntryLine> entryLineCollection, BusinessObjectFactory factory)
			: base(factory)
		{
			foreach (var entryLine in entryLineCollection)
			{
				Add(ESDocSADHLineExport.New(entryLine, factory));
			}
		}

		public new ESDocSADHLineExport AddNew() => (ESDocSADHLineExport)base.AddNew();

		public new ESDocSADHLineExport this[int index] => (ESDocSADHLineExport)base[index];
	}
}
