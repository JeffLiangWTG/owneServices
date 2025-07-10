using System;
using System.Data;

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.JAS.Business.Invoicing
{
	public class JASARAdjustmentNote : ARAdjustmentNote, IJASInvoicingBase
	{
		public JASARAdjustmentNote(BusinessObjectFactory factory, DataRow row)
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
			get { return typeof(JASARAdjustmentNote); }
		}

		protected override Type TypeOfTransaction
		{
			get { return typeof(JASARAdjustmentNote); }
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
			get { return (AH_OSTotal > 0) ? ZArchitecture.Core.TransactionTypes.Invoice : ZArchitecture.Core.TransactionTypes.CreditNote; }
		}

		InvoicingBase IJASInvoicingBase.InvoicingBase
		{
			get { return this; }
		}
		#endregion
	}
}
