using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentWrappers;

namespace Enterprise.Client.TIP
{
	public class DocTIPARBatchInvoice : DocARBatchInvoice
	{
		protected DocTIPARBatchInvoice(InvoiceBatchHeader batchInvoice, BusinessObjectFactory factoryToWrap)
			: base(batchInvoice, factoryToWrap, true)
		{
		}

		public new static DocARBatchInvoice New(InvoiceBatchHeader batchInvoice, BusinessObjectFactory factoryToWrap)
		{
			return (batchInvoice != null) ? new DocTIPARBatchInvoice(batchInvoice, factoryToWrap) : null;
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(OverriddenNewMethod);
		}

		static DocARBatchInvoice OverriddenNewMethod(InvoiceBatchHeader batchInvoice, BusinessObjectFactory factoryToWrap)
		{
			return DocTIPARBatchInvoice.New(batchInvoice, factoryToWrap);
		}

		#region Menu Filter Fields

		public override ZBool PrintStandard
		{
			get { return !PrintClientSpecific; }
		}

		public override ZBool PrintClientSpecific
		{
			get { return ZBool.True; }
		}

		#endregion
	}
}
