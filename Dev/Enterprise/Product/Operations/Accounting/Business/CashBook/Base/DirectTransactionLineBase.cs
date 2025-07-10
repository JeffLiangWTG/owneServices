using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.CashBook
{
	public abstract partial class DirectTransactionLineBase : DependentTransactionLine
	{
		public DirectTransactionLineBase(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override sealed AccTransactionLinesValidation GetNewValidation()
		{
			if (IsValidationSuspended) //keep this IF the first always to avoid any any db hits and calculations for a case when validation will not be used.
			{
				return GetEmptyValidation();//Return empty validation as just some not null value as actual validation calls will be skipped anyway.
			}

			AccTransactionLinesValidation result = null;

			if (ParentTransactionHeader != null)
			{
				if (ParentTransactionHeader.IsDeleted || ((IReversing)ParentTransactionHeader).IsReversed)
				{
					result = GetEmptyValidation();
				}
			}

			if (result == null)
			{
				result = base.GetNewValidation();
			}

			return result;
		}

		protected override AccTransactionLinesLookups GetNewLookups()
		{
			return new DirectTransactionLineBaseLookups(this);
		}

		public DirectTransactionHeaderBase ParentTransactionHeader
		{
			get { return (DirectTransactionHeaderBase)MasterTransactionHeader; }
		}

		public override ZDecimal AL_ExchangeRate
		{
			get { return base.AL_ExchangeRate; }
			set
			{
				base.AL_ExchangeRate = value;

				if (!IsLocalAmountRecalculationSuspended)
				{
					RecalculateLocalAmounts();
					RecalculateTaxAmounts();
				}
			}
		}

		public override ZGuid AL_AG
		{
			get { return base.AL_AG; }
			set
			{
				base.AL_AG = value;
				if (GLHeader != null)
				{
					AL_Desc = GLHeader.AG_DescriptionMultilingual;
				}
			}
		}

		protected override bool AL_AG_ReadOnly
		{
			get { return ReadOnlyIfAL_AGIsValidAndNotMisc && AL_AG.IsValid && !IsParentReceiptTypeMisc; }
		}

		public bool ReadOnlyIfAL_AGIsValidAndNotMisc { get; set; }

		public bool IsParentReceiptTypeMisc { get; set; }

		[ReadOnly(true)]
		public override ZDecimal AL_OverseasTotal
		{
			get { return base.AL_OverseasTotal; }
			set { base.AL_OverseasTotal = value; }
		}

		public override ZGuid AL_AT
		{
			get { return base.AL_AT; }

			set
			{
				base.AL_AT = value;
				UpdateAL_A9_VatClassFromTaxRate();
			}
		}

		protected override void OnRateChangedCore()
		{
			UpdateAL_OSTaxAmount();
		}

		public override bool IsGovtChargeCodeApplicable
		{
			get { return true; }
		}

		protected bool AL_GovtChargeCode_ReadOnly
		{
			get { return false; }
		}

		protected override bool IsMultiSubAccountsSupportedCore => true;
	}
}
