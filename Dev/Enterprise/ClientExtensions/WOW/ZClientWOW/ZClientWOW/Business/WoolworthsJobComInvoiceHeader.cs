using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Client.Wow
{
	public class WoolworthsJobComInvoiceHeader : JobComInvoiceHeader
	{
		public WoolworthsJobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZGuid JZ_JE
		{
			get { return base.JZ_JE; }
			set
			{
				base.JZ_JE = value;
				MarkAllInvoicesAsNeedingValidation();
			}
		}

		public override ZString JZ_IncoTerm
		{
			get { return base.JZ_IncoTerm; }
			set
			{
				base.JZ_IncoTerm = value;
				MarkAllInvoicesAsNeedingValidation();
			}
		}

		public override ZString JZ_RX_NKInvoice_Currency
		{
			get { return base.JZ_RX_NKInvoice_Currency; }
			set
			{
				base.JZ_RX_NKInvoice_Currency = value;
				MarkAllInvoicesAsNeedingValidation();
			}
		}

		void MarkAllInvoicesAsNeedingValidation()
		{
			if (JobDeclaration != null)
			{
				JobDeclaration.Invoices.MarkAsNeedingValidation();
			}
		}

		protected override Customs.Business.JobComInvoiceHeaderValidation GetNewValidation()
		{
			Customs.Business.JobComInvoiceHeaderValidation result = base.GetNewValidation();
			result.Add(new WoolworthsJobComInvoiceHeaderValidation(this));
			return result;
		}
	}
}
