using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocGLJournal : DocTransactionHeader
	{
		DocGLJournal(GLJournal gLJournal, BusinessObjectFactory factoryToWrap)
			: base(gLJournal, factoryToWrap)
		{
		}

		public static DocGLJournal New(GLJournal gLJournal, BusinessObjectFactory factoryToWrap)
		{
			if (gLJournal == null)
			{
				return null;
			}
			else
			{
				return new DocGLJournal(gLJournal, factoryToWrap);
			}
		}

		GLJournal GLJournal
		{
			get { return (GLJournal)WrappedObject; }
		}

		public override string ToString()
		{
			return TransactionNumber;
		}

		public ZDecimal LocalExTaxAmount
		{
			get { return GLJournal.AH_LocalExTaxAmount; }
		}

		public ZDecimal LocalGstAmount
		{
			get { return GLJournal.AH_LocalTaxAmount; }
		}

		public ZDecimal OverseasTotal
		{
			get { return GLJournal.AH_OSTotalAmount; }
		}

		public new ZDecimal GSTAmount
		{
			get { return GLJournal.AH_GSTAmount; }
		}

		public new ZDecimal InvoiceAmount
		{
			get { return GLJournal.AH_InvoiceAmount; }
		}

		public new ZDecimal OSTotal
		{
			get { return GLJournal.AH_OSTotal; }
		}

		public ZBool PostToGL
		{
			get { return GLJournal.AH_PostToGL == "Y"; }
		}

		protected DocTransactionLineCollection fLines;
		public DocTransactionLineCollection Lines
		{
			get
			{
				if (fLines == null)
				{
					fLines = new DocTransactionLineCollection(GLJournal.Factory);
					foreach (GLJournalLine line in GLJournal.Lines)
					{
						fLines.Add(DocGLJournalLine.New(line, Factory));
					}
				}
				return fLines;
			}
		}

		protected override DocTransactionLineCollection GetLinesCore()
		{
			return Lines;
		}

		protected override ZBool GetHasForeignCurrencyLines()
		{
			return GLJournal.Lines.Cast<DependentTransactionLine>().Any(x => x.AL_RX_NKTransactionCurrency != TransactionHeader.AH_RX_NKTransactionCurrency);
		}

		#region Approval

		protected override ZString GetRequestedByCore()
		{
			return GLJournal.LastPostedRequest_RequesterFullName;
		}

		protected override ZDateTime GetRequestedTimeCore()
		{
			return GLJournal.LastPostedRequest_RequestedTime;
		}

		protected override ZString GetApprovedByCore()
		{
			return GLJournal.LastPostedRequest_ApproverFullName;
		}

		protected override ZDateTime GetApprovedTimeCore()
		{
			return GLJournal.LastPostedRequest_ApprovedTime;
		}

		protected override ZString GetOriginalRequesterCore()
		{
			return GLJournal.OriginalRequest_RequesterFullName;
		}

		protected override ZString GetOriginalApproverCore()
		{
			return GLJournal.OriginalPostedRequest_ApproverFullName;
		}

		#endregion

		#region Custom Fields

		protected override ZDecimal GetTotalDebitAmount()
		{
			ZDecimal result = 0M;
			foreach (DocTransactionLine line in Lines)
			{
				result += line.DebitAmountDecimal;
			}

			return result;
		}

		protected override ZDecimal GetTotalCreditAmount()
		{
			ZDecimal result = 0M;
			foreach (DocTransactionLine line in Lines)
			{
				result += line.CreditAmountDecimal;
			}

			return result;
		}

		public ZString PeriodDisplay
		{
			get
			{
				switch (TransactionType)
				{
					case ZArchitecture.Core.TransactionTypes.GLAutoJournal:
						return Res.GetString("e370d441-262c-44bb-80f4-56d9b7adc2a7", "ENDING PERIOD");
					case ZArchitecture.Core.TransactionTypes.GLReversingJournal:
						return Res.GetString("c045ec8a-49cc-4715-8a6b-9b3044221080", "REVERSED IN");
					default:
						return "";
				}
			}
		}

		protected override ZString GetPeriodDisplayCore()
		{
			return PeriodDisplay;
		}

		public ZString AgePeriodDisplay
		{
			get
			{
				ZInt period = 0;
				if (TransactionType == ZArchitecture.Core.TransactionTypes.GLReversingJournal || TransactionType == ZArchitecture.Core.TransactionTypes.GLAutoJournal)
				{
					period = AgePeriod;
				}
				return (period == 0) ? "" : period.ToString();
			}
		}

		protected override ZString GetAgePeriodDisplayCore()
		{
			return AgePeriodDisplay;
		}

		public ZString JournalType
		{
			get
			{
				if (TransactionType == ZArchitecture.Core.TransactionTypes.GLAutoJournal)
				{
					return Res.GetString("ac28a483-bf33-49fb-84c8-aa7f587554bd", "Auto") + " ";
				}
				else if (TransactionType == ZArchitecture.Core.TransactionTypes.GLReversingJournal)
				{
					return Res.GetString("29557424-8f39-4baf-a7ed-aa4435d12ce5", "Reversing") + " ";
				}
				else
				{
					return "";
				}
			}
		}

		protected override ZString GetJournalTypeCore()
		{
			return JournalType;
		}

		#endregion
	}
}
