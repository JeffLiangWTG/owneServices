using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.DocumentWrappers.Customs.EU;
using ESCusEntryLine = Enterprise.Customs.ES.Business.Declaration.CusEntryLine;
using ESCusEntryLineCollection = Enterprise.Customs.Business.ICusEntryLineCollection<Enterprise.Customs.ES.Business.Declaration.CusEntryLine>;
using EUCusEntryLineCollection = Enterprise.Customs.Business.ICusEntryLineCollection<Enterprise.Customs.EU.Business.Declaration.CusEntryLine>;

namespace Enterprise.Customs.ES.DocumentWrappers.SADH
{
	public sealed class ESDocSADHPageCollectionImport : DocSADHPageCollection
	{
		public ESDocSADHPageCollectionImport(ESCusEntryLineCollection collection, BusinessObjectFactory factory)
			: base(collection, factory)
		{
		}

		protected override DocSADHPage NewFirstPage(BusinessObjectFactory factory, CusEntryLine entryLine) => ESDocSADHPageImport.New(factory, (ESCusEntryLine)entryLine);

		public new ESDocSADHPageImport AddNew() => (ESDocSADHPageImport)base.AddNew();

		public new ESDocSADHPageImport this[int index] => (ESDocSADHPageImport)base[index];

		protected override void AddLinePages(EUCusEntryLineCollection collection, BusinessObjectFactory factory)
		{
			var lines = GetESDocSADHLinesImportFromEntryLines((ESCusEntryLineCollection)collection, factory);
			for (int index = 1; index < lines.Count; index += 3)
			{
				Add(NewPageWithUpToThreeLinesFromESDocSADHLines(factory, (ESCusEntryLineCollection)collection, lines, index));
			}
		}

		List<ESDocSADHLineImport> GetESDocSADHLinesImportFromEntryLines(ESCusEntryLineCollection collection, BusinessObjectFactory factory)
		{
			var lines = new List<ESDocSADHLineImport>();
			bool isFirstEntryLine = true;
			foreach (var entryLine in collection)
			{
				lines.Add(ESDocSADHLineImport.New(entryLine, factory));
				SeparateBoxesHelper.GetImportGetExtraLineBox31AndBox44(entryLine, factory, isFirstEntryLine).ForEach(lines.Add);
				if (isFirstEntryLine)
				{
					isFirstEntryLine = false;
				}
			}
			return lines;
		}

		DocSADHPage NewPageWithUpToThreeLinesFromESDocSADHLines(BusinessObjectFactory factory, ESCusEntryLineCollection collection, List<ESDocSADHLineImport> lines, int startLineIndex) => ESDocSADHPageImport.New(factory, collection, lines, startLineIndex);
	}
}
