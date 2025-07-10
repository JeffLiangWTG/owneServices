using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class UACreditNoteLine : APCreditNoteLine
	{
		public UACreditNoteLine(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		public override AccTransactionHeader TransactionHeader => UACreditNote ?? base.TransactionHeader;

		protected override AccTransactionLinesLookups GetNewLookups()
		{
			return new UACreditNoteLineLookups(this);
		}

		UACreditNote UACreditNote => MasterTransactionHeader as UACreditNote;

		protected override ZString LineType
		{
			get { return TransactionLineTypes.UnapprovedCost; }
		}

		protected override AccTransactionLinesValidation GetNewValidationCore()
		{
			return new UACreditNoteLineValidation(this);
		}

		protected override bool SupportsInputVatRecoverableCore
		{
			get { return true; }
		}

		protected override Security.SecurityCheckpoint OverrideInputVatRecoverableSecurityCheckPoint
		{
			get { return Env.Security.AllowUACreditNoteLineVATRecoverableOverride; }
		}
	}
}
