#if DEBUG

using Enterprise.DataTransfer.Integration;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	public partial class MultipleInvoiceXmlDataTransferDirector
	{
		public Enterprise.DataTransfer.Business.XmlDataImporter NewXmlDataImporter_ForTestOnly()
		{
			return NewXmlDataImporter();
		}

		public IValueObjectDataAdapter Adapter_ForTestOnly => Adapter;
	}
}

#endif
