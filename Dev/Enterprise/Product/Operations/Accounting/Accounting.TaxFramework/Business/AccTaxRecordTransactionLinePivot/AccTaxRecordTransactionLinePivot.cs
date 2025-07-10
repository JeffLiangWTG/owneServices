using System;
using System.ComponentModel;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public class AccTaxRecordTransactionLinePivot : AutoAccTaxRecordTransactionLinePivot
	{
		public AccTaxRecordTransactionLinePivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region ATP_ATT

		[RelatedBusinessObject("TaxTransaction")]
		public override ZGuid ATP_ATT
		{
			get => base.ATP_ATT;
			set => base.ATP_ATT = value;
		}

		public AccTaxTransaction TaxTransaction
		{
			get { return Factory.Load<AccTaxTransaction>(ATP_ATT); }
		}

		#endregion

		public void LinkLine(ITaxableTransactionLineBase line)
		{
			using (ReportLinkingLineSuspender.GetSuspender())
			{
				Argument.NotNull(line, nameof(line));

				if (TaxTransaction == null)
				{
					throw new InvalidOperationException("It is not valid to link TransactionLine to Pivot without setting Tax Record (ATP_ATT) first.");
				}

				ATP_AL_TransactionLine = line.PK;
				BaseOSAmount = TaxTransaction.IsOSTaxCurrencyLocal ? line.LocalAmount : line.BaseOSAmount;
				LocalTaxBaseAmount = line.LocalAmount;
			}
		}

		public override ZGuid ATP_AL_TransactionLine
		{
			get => base.ATP_AL_TransactionLine;
			set
			{
				if (!ReportLinkingLineSuspender.IsSuspended)
				{
					var devErrorMessage = (NoResString)"ATP_AL_TransactionLine is being set on AccTaxRecordTransactionLinePivot without using LinkLine method";
					ErrorReporter.ReportOnce("AccTaxRecordTransactionLinePivot_TransactionLineSetWithoutLinkLine", devErrorMessage);
				}

				base.ATP_AL_TransactionLine = value;
			}
		}

		internal ZDecimal BaseOSAmount { get; private set; }
		internal ZDecimal LocalTaxBaseAmount { get; private set; }

		internal FunctionalitySuspender ReportLinkingLineSuspender
		{
			get { return reportLinkingLineSuspender ?? (reportLinkingLineSuspender = new FunctionalitySuspender()); }
		}
		FunctionalitySuspender reportLinkingLineSuspender;

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			using (ReportLinkingLineSuspender.GetSuspender())
			{
				base.FillWithValidTestDataCore(kind, propertyPath);
			}

			if (TransactionLine != null)
			{
				TransactionLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			}
		}

#endif
	}
}
