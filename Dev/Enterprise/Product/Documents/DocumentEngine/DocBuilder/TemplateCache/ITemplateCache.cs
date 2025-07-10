using System;
using Enterprise.DocumentEngine.FlexCelInterface;

namespace Enterprise.DocumentEngine.DocBuilder
{
	public interface ITemplateCache
	{
		void Clear();
		void PurgeOldRecords();
		ExcelInterface Get(TemplateKey key, Func<ExcelInterface> fileGeneratorMethod);
		TimeSpan CacheTimeout { get; }
	}
}
