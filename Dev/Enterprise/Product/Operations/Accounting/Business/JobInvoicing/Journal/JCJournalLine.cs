using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JCJournalLine : DependentTransactionLine
	{
		public new abstract class Schema : DependentTransactionLine.Schema
		{
			public const string AL_CFXControlAccount = "AL_CFXControlAccount";
		}

		public JCJournalLine(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString LineType
		{
			get { return AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue; }
		}

		public void SetCFXValues(Job job, ChargeWithCost chargeWithCost)
		{
			AL_Sequence = (short)ParentHeader.Lines.Count;
			AL_Desc = chargeWithCost.JR_Desc;
			AL_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AL_ExchangeRate = 1M;
			AL_JH = job != null ? job.PK : ZGuid.Empty;
			AL_AC = chargeWithCost.JR_AC;
			AL_GB = chargeWithCost.JR_GB;
			AL_GE = chargeWithCost.JR_GE;

			decimal amount = -chargeWithCost.JR_CFXAmt;
			AL_LineAmount = amount;
			AL_OSAmount = amount;
		}

		protected override bool InvertSigns
		{
			get { return false; }
		}

		#region Public Properties

		[List("Lookups.GLHeaders")]
		public ZGuid AL_CFXControlAccount
		{
			get { return new ZGuid(AccountingConfigurationRegistry.Instance.CFXAccount.GetFallBackValueAtAllLevels(Branch.Company.PK.ToGuid(), Guid.Empty, AL_GE.ToGuid())); }
		}

		public ZPropertyInfo AL_CFXControlAccountInfo
		{
			get { return GetZPropertyInfo(Schema.AL_CFXControlAccount); }
		}

		[List("Jobs")]
		public override ZGuid AL_JH
		{
			get { return base.AL_JH; }
			set { base.AL_JH = value; }
		}

		[List("ChargeCodes")]
		public override ZGuid AL_AC
		{
			get { return base.AL_AC; }
			set { base.AL_AC = value; }
		}

		[List("GLHeaders")]
		public override ZGuid AL_AG
		{
			get { return base.AL_AG; }
			set { base.AL_AG = value; }
		}

		#endregion

		#region Lookups

		#region ChargeCodes

		public AccChargeCodeCollection ChargeCodes
		{
			get { return FindboxLookupCollections.GetChargeCodeCollection(Factory); }
		}

		#endregion

		#region Jobs

		public JobCollection Jobs
		{
			get
			{
				if (fJobs == null)
				{
					fJobs = new JobCollection(Factory);
				}

				return fJobs;
			}
		}

		JobCollection fJobs;

		#endregion

		#region GL Headers

		public AccGLHeaderCollection GLHeaders
		{
			get
			{
				if (fGLHeaders == null)
				{
					fGLHeaders = new AccGLHeaderCollection(Factory);
				}

				return fGLHeaders;
			}
		}

		AccGLHeaderCollection fGLHeaders;

		#endregion

		#endregion

		#region JCJournal Parent

		public JCJournalHeader ParentHeader
		{
			get { return (JCJournalHeader)MasterTransactionHeader; }
		}

		#endregion
	}
}
