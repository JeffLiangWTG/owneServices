#if DEBUG

using CargoWise.Types;
using Enterprise.DocumentEngine;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public partial class JobDocumentPrinter
	{
		public string JobProfitDocumentMenuName_ForTestOnly => JobProfitDocumentMenuName;

		public PrintTask GetPrintTask_ForTestOnly(ZGuid docCommandPk)
		{
			return GetPrintTask(docCommandPk);
		}

		public DocumentCommand FindJobDocumentCommand_ForTestOnly(JobDocumentPrintItem jobDocumentPrintItem = null)
		{
			return FindJobDocumentCommand(jobDocumentPrintItem);
		}

		public DocumentPack GetDocumentPack_ForTestOnly()
		{
			return GetDocumentPack();
		}

		public DeliveryInstructions GetPrintDeliveryInstructions_ForTestOnly(DocumentPack docPack)
		{
			return GetPrintDeliveryInstructions(docPack);
		}
	}
}

#endif
