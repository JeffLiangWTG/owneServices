using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.DataTransfer.SystemMerge.GUI;

namespace Enterprise.Accounting.Module
{
	public class SysMergeJournalXmlDataTransferDirector : SysMergeXmlDataTransferDirector
	{
		public SysMergeJournalXmlDataTransferDirector()
			: base(new ARAPJournalDataAdapter())
		{
		}
	}
}