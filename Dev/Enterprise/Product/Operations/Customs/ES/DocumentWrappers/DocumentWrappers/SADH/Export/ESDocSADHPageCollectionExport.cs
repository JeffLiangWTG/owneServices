using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.DocumentWrappers.Customs.EU;
using ESCusEntryLine = Enterprise.Customs.ES.Business.Declaration.CusEntryLine;
using ESCusEntryLineCollection = Enterprise.Customs.Business.ICusEntryLineCollection<Enterprise.Customs.ES.Business.Declaration.CusEntryLine>;
using EUCusEntryLineCollection = Enterprise.Customs.Business.ICusEntryLineCollection<Enterprise.Customs.EU.Business.Declaration.CusEntryLine>;

namespace Enterprise.Customs.ES.DocumentWrappers.SADH
{
	public sealed class ESDocSADHPageCollectionExport : DocSADHPageCollection
	{
		public ESDocSADHPageCollectionExport(ESCusEntryLineCollection collection, BusinessObjectFactory factory)
			: base(collection, factory)
		{
		}

		protected override DocSADHPage NewFirstPage(BusinessObjectFactory factory, CusEntryLine entryLine) => ESDocSADHPageExport.New(factory, (ESCusEntryLine)entryLine);

		public new ESDocSADHPageExport AddNew() => (ESDocSADHPageExport)base.AddNew();

		public new ESDocSADHPageExport this[int index] => (ESDocSADHPageExport)base[index];

		protected override void AddLinePages(EUCusEntryLineCollection collection, BusinessObjectFactory factory)
		{
			var lines = GetESDocSADHLinesExportFromEntryLines((ESCusEntryLineCollection)collection, factory);
			for (int index = 1; index < lines.Count; index += 3)
			{
				Add(NewPageWithUpToThreeLinesFromESDocSADHLines(factory, lines, index));
			}
		}

		List<ESDocSADHLineExport> GetESDocSADHLinesExportFromEntryLines(ESCusEntryLineCollection collection, BusinessObjectFactory factory)
		{
			var lines = new List<ESDocSADHLineExport>();
			bool isFirstEntryLine = true;
			foreach (var entryLine in collection)
			{
				lines.Add(ESDocSADHLineExport.New(entryLine, factory));
				SeparateBoxesHelper.GetExportGetExtraLineBox31AndBox44(entryLine, factory, isFirstEntryLine).ForEach(lines.Add);
				if (isFirstEntryLine)
				{
					isFirstEntryLine = false;
				}
			}
			return lines;
		}

		DocSADHPage NewPageWithUpToThreeLinesFromESDocSADHLines(BusinessObjectFactory factory, List<ESDocSADHLineExport> lines, int startLineIndex) => ESDocSADHPageExport.New(factory, lines, startLineIndex);
	}
}
