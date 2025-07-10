using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.EU;
using ESCusEntryLine = Enterprise.Customs.ES.Business.Declaration.CusEntryLine;
using ESCusEntryLineCollection = Enterprise.Customs.Business.ICusEntryLineCollection<Enterprise.Customs.ES.Business.Declaration.CusEntryLine>;

namespace Enterprise.Customs.ES.DocumentWrappers.SADH
{
	public sealed class ESDocSADHLineCollectionImport : DocSADHLineCollection
	{
		public ESDocSADHLineCollectionImport(ESCusEntryLineCollection entryLineCollection, BusinessObjectFactory factory)
			: this(entryLineCollection.Cast<ESCusEntryLine>(), factory)
		{
		}

		public ESDocSADHLineCollectionImport(IEnumerable<ESCusEntryLine> entryLineCollection, BusinessObjectFactory factory)
			: base(factory)
		{
			foreach (var entryLine in entryLineCollection)
			{
				Add(ESDocSADHLineImport.New(entryLine, factory));
			}
		}

		public new ESDocSADHLineImport AddNew() => (ESDocSADHLineImport)base.AddNew();

		public new ESDocSADHLineImport this[int index] => (ESDocSADHLineImport)base[index];
	}
}
