using System;
using System.Data;

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.JAS.Business.Invoicing
{
	public class JASARCreditNote : ARCreditNote, IJASInvoicingBase
	{
		public JASARCreditNote(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void OnSavingCore()
		{
			base.OnSavingCore();
			ExportHelper.OnSaving();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			ExportHelper.OnSaved(saveSucceeded);
		}

		protected override Type TypeOfReverseTransaction
		{
			get { return typeof(JASARInvoice); }
		}

		protected override Type TypeOfTransaction
		{
			get { return typeof(JASARCreditNote); }
		}

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				if (fNoteTypes == null)
				{
					fNoteTypes = base.NoteTypesCore;
					fNoteTypes.Add(JASPredefinedNoteTypes.Instance.JXCExportLog);
				}
				return fNoteTypes;
			}
		}

		protected override MasterFiles.Business.AccTransactionHeaderValidation GetNewEmptyValidation()
		{
			return IsInJasSpecificNeedsCoreValidation ? GetNewValidationCore() : base.GetNewEmptyValidation();
		}

		public bool IsInJasSpecificNeedsCoreValidation { get; set; }

		FinancialMessageExportHelper ExportHelper
		{
			get
			{
				if (fExportHelper == null)
				{
					fExportHelper = GetNewFinancialMessageExportHelper();
				}
				return fExportHelper;
			}
		}

		protected virtual FinancialMessageExportHelper GetNewFinancialMessageExportHelper()
		{
			return new FinancialMessageExportHelper(this);
		}

		FinancialMessageExportHelper fExportHelper;
		NoteTypeCollection fNoteTypes;

		#region IJASInvoicingBase Members

		ZString IJASInvoicingBase.CreditNoteOrInvoice
		{
			get { return ZArchitecture.Core.TransactionTypes.CreditNote; }
		}

		InvoicingBase IJASInvoicingBase.InvoicingBase
		{
			get { return this; }
		}
		#endregion
	}
}
