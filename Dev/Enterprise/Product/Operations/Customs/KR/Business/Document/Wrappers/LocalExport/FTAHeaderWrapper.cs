using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class FTAHeaderWrapper : NonPersistentBusinessObject
	{
		public FTAHeaderWrapper(ImportFTAHeader header, BusinessObjectFactory factory)
			: base(factory)
		{
			Header = header;
		}
		public FTAHeaderWrapper(ImportDHRHeader header, BusinessObjectFactory factory)
			: base(factory)
		{
			Header = header;
			DHRHeader = header;
		}
		public IImportFTAHeader Header { get; }
		public IImportDHRHeader DHRHeader { get; }

		public FTALineWrapperCollection FTALineItems
		{
			get
			{
				if (ftaLineItems == null)
				{
					ftaLineItems = new FTALineWrapperCollection(Header.EntryLines, base.Factory);
				}
				return ftaLineItems;
			}
		}
		FTALineWrapperCollection ftaLineItems;

		public DHRInvoiceLineWrapperCollection DHRInvoiceLineItems
		{
			get
			{
				if (dhrInvoiceLineItems == null)
				{
					dhrInvoiceLineItems = new DHRInvoiceLineWrapperCollection(DHRHeader?.DHRInvoiceLines, base.Factory);
				}
				return dhrInvoiceLineItems;
			}
		}
		DHRInvoiceLineWrapperCollection dhrInvoiceLineItems;
	}
}
