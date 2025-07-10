using System;

namespace Enterprise.Customs.IT.Business;

public class SadImIrispMessageSubProcessor : SadIrispMessageSubProcessor, ISadImportExportMessageSubProcessor
{
	public SadImIrispMessageSubProcessor(ISadCustomsLinkedObjectAdapter entryAdapter) : base(entryAdapter)
	{
		if (!entryAdapter.IsImport)
		{
			throw new ArgumentException("Entry must be related to an import job");
		}
	}
}
