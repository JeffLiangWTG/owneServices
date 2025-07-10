using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class UAInvoiceLine : APInvoiceLine
	{
		public UAInvoiceLine(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		public override AccTransactionHeader TransactionHeader => UAInvoice ?? base.TransactionHeader;

		protected override AccTransactionLinesLookups GetNewLookups()
		{
			return new UAInvoiceLineLookups(this);
		}

		UAInvoice UAInvoice => MasterTransactionHeader as UAInvoice;

		protected override ZString LineType
		{
			get { return TransactionLineTypes.UnapprovedCost; }
		}

		protected override AccTransactionLinesValidation GetNewValidationCore()
		{
			return new UAInvoiceLineValidation(this);
		}

		public override void ImportFromApportionSplitCharge(ApportionSplitCharge chargeToImportFrom)
		{
			base.ImportFromApportionSplitCharge(chargeToImportFrom);
			if (UAInvoiceLineValidation != null) {
				UAInvoiceLineValidation.ValidateAL_IsFinalCharge();
			}
		}

		UAInvoiceLineValidation UAInvoiceLineValidation
		{
			get { return Validation as UAInvoiceLineValidation; }
		}

		protected override bool SupportsInputVatRecoverableCore
		{
			get { return true; }
		}

		protected override Security.SecurityCheckpoint OverrideInputVatRecoverableSecurityCheckPoint
		{
			get { return Env.Security.AllowUAInvoiceLineVATRecoverableOverride; }
		}
	}
}
