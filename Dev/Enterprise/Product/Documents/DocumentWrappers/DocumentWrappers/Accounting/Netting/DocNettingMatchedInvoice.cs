using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Netting;

namespace Enterprise.DocumentWrappers
{
	public class DocNettingMatchedInvoice : DocBaseWrapper
	{
		protected DocNettingMatchedInvoice(NettingMatchedInvoice nettingMatchedInvoice, BusinessObjectFactory factoryToWrap)
			: base(nettingMatchedInvoice, factoryToWrap)
		{
			Argument.NotNull(nettingMatchedInvoice, "NettingMovement");
		}

		public static DocNettingMatchedInvoice New(NettingMatchedInvoice nettingMatchedInvoice, BusinessObjectFactory factoryToWrap)
		{
			return new DocNettingMatchedInvoice(nettingMatchedInvoice, factoryToWrap);
		}

		NettingMatchedInvoice MatchedInvoice
		{
			get { return (NettingMatchedInvoice)WrappedObject; }
		}

		public ZString Issuer
		{
			get { return MatchedInvoice.Issuer; }
		}

		public ZString Recipient
		{
			get { return MatchedInvoice.Recipient; }
		}

		public ZString ARInvoiceReference
		{
			get { return MatchedInvoice.ARInvoiceReference; }
		}

		public ZString APInvoiceReference
		{
			get { return MatchedInvoice.APInvoiceReference; }
		}
	}
}
