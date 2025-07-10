using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class APCreditNoteLine : CreditNoteLine
	{
		public APCreditNoteLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override AccTransactionHeader TransactionHeader => APCreditNote ?? base.TransactionHeader;

		protected override AccTransactionLinesLookups GetNewLookups()
		{
			return new APCreditNoteLineLookups(this);
		}

		APCreditNote APCreditNote => MasterTransactionHeader as APCreditNote;

		protected override bool SupportsInputVatRecoverableCore
		{
			get { return true; }
		}

		protected override Security.SecurityCheckpoint OverrideInputVatRecoverableSecurityCheckPoint
		{
			get { return InvoiceBase != null && InvoiceBase.HasApprovalRequest ? Env.Security.APInvoiceApproval_AllowCrdVATRecoverableOverride : Env.Security.AllowAPCreditNoteLineVATRecoverableOverride; }
		}

		protected override bool InvertSigns
		{
			get { return false; }
		}

		protected override ZString LineType
		{
			get { return TransactionLineTypes.Cost; }
		}

		protected override GenericChargeCollectionBuilder GetGenericChargeCollectionBuilder()
		{
			return new APGenericChargeCollectionBuilder(this);
		}

		protected override AccTransactionLinesValidation GetNewValidationCore()
		{
			if (Factory.HasContext(BusinessContext.SavingIncompleteTransaction))
			{
				return new IncompleteInvoicingLineBaseValidation(this);
			}

			return base.GetNewValidationCore();
		}

		protected override void SetDefaultAL_JHCore(ZGuid jobHeaderGuid)
		{
			SuspendChargePopup();
			this.AL_JH = jobHeaderGuid;
			SetExchangeRate();
			ResumeChargePopup();
		}

		protected override bool AL_ExchangeRate_ReadOnly
		{
			get { return base.AL_ExchangeRate_ReadOnly || !Env.Security.AllowAPCreditNoteLineExchangeRateOverride.IsAllowed; }
		}

		protected override void OnFactorySavingBeforeTransactionCore2()
		{
			base.OnFactorySavingBeforeTransactionCore2();

			APLineRelatedJobOpener.ReopenClosedJobwithSuspendedValidation(InvoiceBase);
		}
	}
}
