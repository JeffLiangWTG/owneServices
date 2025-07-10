using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobRevenueJournalLineValidation : DependentTransactionLineValidation
	{
		public JobRevenueJournalLineValidation(JobRevenueJournalLine parent, IClosedJobReopener closedJobReopener)
			: base(parent)
		{
			ClosedJobReopener = closedJobReopener;
		}

		readonly IClosedJobReopener ClosedJobReopener;

		JobRevenueJournalLine ParentLine
		{
			get { return (JobRevenueJournalLine)Parent; }
		}

		public override void ValidateAll()
		{
			IsAddRowWarningIfTheSameLineIsExistSuspended = true;
			try
			{
				base.ValidateAll();

				ValidateDebitCreditSign();
				ValidateOSUnsignedLineAmount();
				ValidateLocalUnsignedLineAmount();
				ValidateCostRevenueType();
			}
			finally
			{
				IsAddRowWarningIfTheSameLineIsExistSuspended = false;
			}

			AddRowWarningIfTheSameLineIsExist();
		}

		protected override void CheckAL_JH()
		{
			base.CheckAL_JH();
			MandatoryValidation.CheckEntered(ParentLine.AL_JHInfo);
			ListValidation.ErrorIfInvalidPK(ParentLine.AL_JHInfo);

			if (ParentLine.Job != null && ParentLine.Job.IsReadyForFinancialClosureWithoutPostSecurity)
			{
				ParentLine.AL_JHInfo.AddError(Res.GetString("9018F3D5-CF1E-477D-BF7F-3D301D43454F", @"Cannot post this charge for this job, because it has Ready For Financial Closure status."));
			}

			AddRowWarningIfTheSameLineIsExist();

			if (!Parent.AL_JHInfo.HasErrors() && Parent.AL_JH.IsValid)
			{
				ValidateRevenueRecognition(Res.GetString("484005A8-64CD-4DE6-99F4-7DE6EE775369", "This journal cannot be posted until the {0:G} for this job is recorded. This job and charge code combination requires this date for revenue recognition purposes."));
			}

			ClosedJobReopener?.ValidateClosedJob(ParentLine.AL_JHInfo, ParentLine.InvoicingJob);
		}

		protected override void CheckAL_Desc()
		{
			base.CheckAL_Desc();
			MandatoryValidation.CheckEntered(Parent.AL_DescInfo);
		}

		protected override void CheckAL_ExchangeRate()
		{
			base.CheckAL_ExchangeRate();

			MandatoryValidation.CheckEntered(Parent.AL_ExchangeRateInfo);
		}

		protected override void CheckAL_AG()
		{
			//empty validation because user can't change AL_AG value and so can't fix errors here 
		}

		protected override void CheckAL_SupplyType()
		{
			//empty validation because Job Revenue Journal Line don't have supply type
		}

		protected override void CheckAL_AC()
		{
			MandatoryValidation.CheckEntered(ParentLine.AL_ACInfo);

			string error = ParentLine.ErrorMessageIfInvalidAL_AC();
			if (!string.IsNullOrEmpty(error))
			{
				ParentLine.AL_ACInfo.AddError(error);
			}

			AddRowWarningIfTheSameLineIsExist();
		}

		#region ValidateDebitCreditSign

		public void ValidateDebitCreditSign()
		{
			ValidateCalculatedProperty(ParentLine.DebitCreditSignInfo);
		}

		protected void CheckDebitCreditSign()
		{
			MandatoryValidation.CheckEntered(ParentLine.DebitCreditSignInfo);
			ListValidation.ErrorIfInvalidCode(ParentLine.DebitCreditSignInfo, ParentLine.DebitCreditSignList);

			AddRowWarningIfTheSameLineIsExist();
		}

		#endregion

		#region ValidateCostRevenueType

		public void ValidateCostRevenueType()
		{
			ValidateCalculatedProperty(ParentLine.CostRevenueTypeInfo);
		}

		protected void CheckCostRevenueType()
		{
			MandatoryValidation.CheckEntered(ParentLine.CostRevenueTypeInfo);
			ListValidation.ErrorIfInvalidCode(ParentLine.CostRevenueTypeInfo, ParentLine.CostRevenueTypeList);
			if (ParentLine.ParentJournal != null && !ParentLine.ParentJournal.IsReverseTransaction)
			{
				var registrySetting = AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				if (registrySetting != AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code && ParentLine.CostRevenueType != registrySetting)
				{
					ParentLine.CostRevenueTypeInfo.AddError(Res.GetString("08cfbd3c-78bd-4e3d-b094-098e0e422642", "When '{0}' Registry is set to {1}, Line Cost/Revenue Type must be {1}.",
						AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.Caption, registrySetting));
				}
			}
		}

		#endregion

		#region ValidateUnsignedLineAmount

		public void ValidateOSUnsignedLineAmount()
		{
			ValidateCalculatedProperty(ParentLine.OSUnsignedLineAmountInfo);
		}

		protected void CheckOSUnsignedLineAmount()
		{
			MandatoryValidation.CheckEntered(ParentLine.OSUnsignedLineAmountInfo);

			AddRowWarningIfTheSameLineIsExist();
		}

		public void ValidateLocalUnsignedLineAmount()
		{
			ValidateCalculatedProperty(ParentLine.LocalUnsignedLineAmountInfo);
		}

		protected void CheckLocalUnsignedLineAmount()
		{
			MandatoryValidation.CheckEntered(ParentLine.LocalUnsignedLineAmountInfo);

			AddRowWarningIfTheSameLineIsExist();

			if (!AccountingMasterFilesUtils.IsForeignAndLocalAmountSameWhenUsingLocalCurrency(Parent.AL_RX_NKTransactionCurrency, ParentLine.OSUnsignedLineAmount, ParentLine.LocalUnsignedLineAmount))
			{
				ParentLine.LocalUnsignedLineAmountInfo.AddError(Res.GetString("e0c5fbbb-7679-46c2-bc03-28c3a35c2c61", "The Local Amount should be equal to OS Amount when Local Currency is used"));
			}
		}

		#endregion

		protected override void CheckAL_GB()
		{
			base.CheckAL_GB();

			AddRowWarningIfTheSameLineIsExist();
		}

		protected override void CheckAL_GE()
		{
			base.CheckAL_GE();

			AddRowWarningIfTheSameLineIsExist();

			if (ParentLine.AL_GE.IsValid && !ParentLine.AL_GE.IsEmpty)
			{
				GlbDepartment dept = ParentLine.Factory.Load<GlbDepartment>(ParentLine.AL_GE);
				if (dept != null && dept.GE_Misc && ParentLine.ParentJournal != null && !ParentLine.ParentJournal.IsReverseTransaction)
				{
					ParentLine.AL_GEInfo.AddError(Res.GetString("9679EEC3-F37F-4D94-9D05-04F6FB7636D1", "Cannot issue job charges for a miscellaneous department."));
				}
			}
		}

		protected override void CheckAL_RX_NKTransactionCurrency()
		{
			base.CheckAL_RX_NKTransactionCurrency();

			AddRowWarningIfTheSameLineIsExist();
		}

		protected override void CheckAL_LocalTaxAmount()
		{
			//Journal lines do not have tax
		}

		protected override void CheckAL_LocalExTaxAmount()
		{
			//Journal Lines don't use AL_LocalExTaxAmount in GUI Form, they use UnsignedLocalLineAmount
		}

		void AddRowWarningIfTheSameLineIsExist()
		{
			if (!IsAddRowWarningIfTheSameLineIsExistSuspended)
			{
				IsAddRowWarningIfTheSameLineIsExistSuspended = true;
				try
				{
					Notification warning = new Notification(CargoWise.EntityFramework.NotificationType.Warning, Res.GetString("CD643911-B085-4AEE-8CB0-113F98AE04A4", "This line is possibly wrong because line with the same data already exist in this journal."));
					ParentLine.RemoveRowNotification(warning);

					if (!ParentLine.IsInDatabase && ParentLine.ParentJournal != null &&
						!ParentLine.AL_ACInfo.HasErrors() &&
						!ParentLine.AL_JHInfo.HasErrors() &&
						!ParentLine.AL_GBInfo.HasErrors() &&
						!ParentLine.AL_GEInfo.HasErrors() &&
						!ParentLine.AL_RX_NKTransactionCurrencyInfo.HasErrors() &&
						!ParentLine.OSUnsignedLineAmountInfo.HasErrors() &&
						!ParentLine.LocalUnsignedLineAmountInfo.HasErrors() &&
						!ParentLine.DebitCreditSignInfo.HasErrors())
					{
						JobRevenueJournalLine equalLine = (from JobRevenueJournalLine line in ParentLine.ParentJournal.Lines
																							 where line.PK != ParentLine.PK &&
																								 line.AL_AC == ParentLine.AL_AC &&
																								 line.AL_JH == ParentLine.AL_JH &&
																								 line.AL_GB == ParentLine.AL_GB &&
																								 line.AL_GE == ParentLine.AL_GE &&
																								 line.AL_RX_NKTransactionCurrency == ParentLine.AL_RX_NKTransactionCurrency &&
																								 line.OSUnsignedLineAmount == ParentLine.OSUnsignedLineAmount &&
																								 line.LocalUnsignedLineAmount == ParentLine.LocalUnsignedLineAmount &&
																								 line.DebitCreditSign != ParentLine.DebitCreditSign
																							 select line).FirstOrDefault();

						if (equalLine != null)
						{
							ParentLine.AddRowNotification(warning);
						}
					}
				}
				finally
				{
					IsAddRowWarningIfTheSameLineIsExistSuspended = false;
				}
			}
		}

		bool IsAddRowWarningIfTheSameLineIsExistSuspended;
	}
}
