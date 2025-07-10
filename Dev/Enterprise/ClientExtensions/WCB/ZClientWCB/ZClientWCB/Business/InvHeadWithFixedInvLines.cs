using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;

namespace Enterprise.Client.WCB
{
	internal class InvHeadWithFixedInvLines : JobComInvoiceHeader
	{
		public InvHeadWithFixedInvLines(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override BaseJobComInvoiceLineViewCollection CreateNewJobComInvoiceLineCollection()
		{
			InvLineFixedCollection result = null;
			if (JobDeclaration != null)
			{
				result = new InvLineFixedCollection(this, JobDeclaration.InvoiceLines);
			}
			return result;
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
			JobDeclaration?.Invoices.MarkAsNeedingValidation();
		}
	}
}
