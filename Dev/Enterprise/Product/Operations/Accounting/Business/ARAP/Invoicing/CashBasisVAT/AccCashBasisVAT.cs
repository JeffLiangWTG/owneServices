using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class AccCashBasisVAT : AutoAccCashBasisVAT, ISupportCriticalValidation, IHaveConstructorStackTrace
	{
		public AccCashBasisVAT(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			YC_GC = GlbCompany.CurrentCompany.PK;
		}

		public sealed override void OnSaving()
		{
			base.OnSaving();
			OnSavingCore();
			((ISupportCriticalValidation)this).CriticalValidation.RegisterOnSavingCheck();
		}

		protected virtual void OnSavingCore()
		{
		}

		public sealed override void Delete()
		{
			if (!IsInDatabase)
			{
				base.Delete();
			}
			else
			{
				throw new NotSupportedException("You cannot delete Cash VAT record in Database.");
			}
		}

		public int LocalDecimals => Company.GetLocalDecimals();

		[DecimalPlaces(nameof(LocalDecimals))]
		public override ZDecimal YC_TaxBaseAmount
		{
			get => base.YC_TaxBaseAmount;
			set => base.YC_TaxBaseAmount = value;
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public override ZDecimal YC_TaxAmount
		{
			get => base.YC_TaxAmount;
			set => base.YC_TaxAmount = value;
		}

		#region ISupportCriticalValidation

		ICriticalValidation ISupportCriticalValidation.CriticalValidation
		{
			get { return new AccCashBasisVATCriticalValidation(this); }
		}

		void IConflictWithCriticalFields.SetConflictWithCriticalFieldsBusinessContext()
		{
			CriticalValidationHelpers.SetConflictWithCriticalFieldsBusinessContext(this);
		}

		#endregion

		#region IHaveConstructorStackTrace member

		StackTrace IHaveConstructorStackTrace.ConstructorStackTrace { get; set; }

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if ((kind & TestBusinessObjectKind.PopulateDependentCollections) != 0)
			{
				var testObjectCreator = new TestObjectCreator(Factory);

				var invoice = Factory.New<APInvoice>();
				invoice.AH_OH = testObjectCreator.Creditor1.PK;
				var line = testObjectCreator.CreateCashVATLines(invoice, 100, 20, false);
				var line2 = testObjectCreator.CreateCashVATLines(invoice, -100, -20, false); // Needed to avoid error: Cash Basis Tax Recognition record with an empty Match Group number can only be created for zero value transactions
				YC_AL_TransactionLine = invoice.Lines[0].PK;
				YC_TaxBaseAmount = 100;
				YC_TaxAmount = 20;
				YC_PostDate = ZDateTime.Today;
			}
			else
			{
				base.FillWithValidTestDataCore(kind, propertyPath);
			}
		}
#endif
	}
}
