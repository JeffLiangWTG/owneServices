using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.Client.WLG
{
	public class WLGARInvoice : ARInvoice
	{
		public WLGARInvoice(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		DocumentSupporter fDocumentSupporter;
		public override DocumentSupporter DocumentSupporter
		{
			get { return (fDocumentSupporter) ?? (fDocumentSupporter = base.DocumentSupporter); }
		}
	}
}
