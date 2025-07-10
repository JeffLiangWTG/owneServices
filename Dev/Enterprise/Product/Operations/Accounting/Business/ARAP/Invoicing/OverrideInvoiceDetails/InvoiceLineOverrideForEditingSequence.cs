using System;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Integration.Accounting;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class InvoiceLineOverrideForEditingSequence : InvoiceLineOverride
	{
		public InvoiceLineOverrideForEditingSequence(DependentTransactionLine line)
			: base(line)
		{
		}

		#region AL_Sequence

		public ZShort AL_Sequence
		{
			get { return Line.AL_Sequence; }
			set
			{
				if (value != Line.AL_Sequence)
				{
					Line.AL_Sequence = value;

					if (!IsValidationSuspended)
					{
						ValidateAL_Sequence();
					}
				}
				AL_SequenceInfo.RefreshBinding();
			}
		}

		void ValidateAL_Sequence()
		{
			AL_SequenceInfo.ClearAllNotifications();

			if (InvoicingBase != null && !InvoicingBase.IsReverseTransaction)
			{
				InvoicingBase.InitializeDuplicateLinesSequenceLookup();
				MandatoryValidation.CheckNotNegative(AL_SequenceInfo);
				if (!AL_SequenceInfo.HasErrors())
				{
					var key = new Tuple<ZGuid, ZShort>(Line.AL_JH, Line.AL_Sequence);
					if (InvoicingBase.IsDuplicateLineSequenceDetected(key))
					{
						AL_SequenceInfo.AddError(Res.GetString("7E0E0D20-814D-46BA-B061-4E8C962551D1", "The Line Sequence Number must be unique."));
					}
				}

				var overrideTransactionLineSequenceProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Line.Company.GC_RN_NKCountryCode) as IOverrideTransactionLineSequenceProvider;

				if ((overrideTransactionLineSequenceProvider?.CanOverrideTransactionLineSequence(InvoicingBase) ?? false)
					&& Line.AL_LineAmount < 0)
				{
					AL_SequenceInfo.AddWarning(Res.GetString("0DED6BF1-1039-4E01-BB67-4500D47FC506", "The 'discount line' should be recorded immediately after the discounted line in the same invoice group."));
				}
			}
		}

		public ZPropertyInfo AL_SequenceInfo
		{
			get { return this.GetZPropertyInfo(nameof(AL_Sequence)); }
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			ValidateAL_Sequence();
		}
	}
}
