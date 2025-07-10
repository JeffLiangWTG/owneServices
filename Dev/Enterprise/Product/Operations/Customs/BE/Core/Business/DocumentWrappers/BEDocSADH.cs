using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.EU;
using BECusEntryHeader = Enterprise.Customs.BE.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.BE.Business
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Used in documents DataContext")]
	public class BEDocSADH : DocSADH
	{
		BEDocSADH(BECusEntryHeader entryHeader, BusinessObjectFactory factoryToWrap)
			: base(entryHeader, factoryToWrap)
		{ }

		public static BEDocSADH New(BECusEntryHeader entryHeader, BusinessObjectFactory factoryToWrap) => new BEDocSADH(entryHeader, factoryToWrap);

		public new BECusEntryHeader EntryHeader => (BECusEntryHeader)base.EntryHeader;

		protected override IBox16CountryOfOriginEvaluator GetBox16CountryOfOriginEvaluator() => new SingleOrEmptyIfMultipleBox16CountryOfOriginEvaluator(EntryHeader);
	}
}
