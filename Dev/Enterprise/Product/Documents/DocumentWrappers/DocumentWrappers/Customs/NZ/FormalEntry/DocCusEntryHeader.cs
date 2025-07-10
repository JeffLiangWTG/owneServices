
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration.FormalEntry;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.DocumentWrappers.Customs.NZ.FormalEntry
{
	public class DocCusEntryHeader : DocBaseCusEntryHeader
	{
		#region Static and Normal Constructors
		DocCusEntryHeader(CusEntryHeader cusEntryHeader, BusinessObjectFactory factoryToWrap)
			: base(cusEntryHeader, factoryToWrap)
		{
		}

		public static DocCusEntryHeader New(CusEntryHeader cusEntryHeader, BusinessObjectFactory factoryToWrap)
		{
			if (cusEntryHeader == null)
			{
				return null;
			}
			else
			{
				return new DocCusEntryHeader(cusEntryHeader, factoryToWrap);
			}
		}
		#endregion

		#region EntryHeader Implementation
		CusEntryHeader EntryHeader
		{
			get { return (CusEntryHeader)WrappedObject; }
		}

		bool IsExport
		{
			get { return EntryHeader != null && EntryHeader.Declaration != null && EntryHeader.Declaration.IsExport; }
		}
		#endregion

		#region Common Mapped Fields
		public ZInt TotalLines
		{
			get { return EntryHeader.MergedLines.Count; }
		}

		public ZString VFDWholeNZD
		{
			get { return EntryHeader.VFDWholeNZD.ToString(0); }
		}

		public ZString TotalDuty
		{
			get { return "$" + (IsExport ? EntryHeader.DutyCreditAmount.ToString(2) : EntryHeader.DutyAmount.ToString(2)); }
		}

		public ZString TotalGST
		{
			get { return "$" + (IsExport ? EntryHeader.GSTCreditAmount.ToString(2) : EntryHeader.GSTAmount.ToString(2)); }
		}

		public ZString TotalLevies
		{
			get { return "$" + EntryHeader.TotalMisc.ToString(2); }
		}

		public ZString EntryFee
		{
			get { return "$" + EntryFeeAmount.ToString(2); }
		}

		public ZString EntryFeeExGST
		{
			get { return "$" + EntryHeader.EntryFeeAmount.ToString(2); }
		}

		public ZString EntryFeeGST
		{
			get { return "$" + EntryHeader.EntryFeeGST.ToString(2); }
		}

		protected override ZDecimal EntryFeeCore
		{
			get { return EntryHeader.EntryFeeAmount + EntryHeader.EntryFeeGST; }
		}

		public ZString TotalPayableNZD
		{
			get { return "$" + EntryHeader.TotalAmountPayable.ToString(2); }
		}

		public ZString GrandTotalAmount
		{
			get { return GrandTotal.Amount; }
		}

		public ZString GrandTotalDescription
		{
			get { return GrandTotal.Description; }
		}

		#region GrandTotal
		GrandTotalGenerator GrandTotal
		{
			get
			{
				if (fGrandTotal == null)
				{
					fGrandTotal = new GrandTotalGenerator(this);
				}
				return fGrandTotal;
			}
		}
		GrandTotalGenerator fGrandTotal;
		class GrandTotalGenerator
		{
			public GrandTotalGenerator(DocCusEntryHeader entryHeader)
			{
				ZDecimal amountPayable = entryHeader.EntryFeeAmount + (entryHeader.IsExport ? new ZDecimal(-entryHeader.TotalAmountPayable) : entryHeader.TotalAmountPayable);
				if (amountPayable < 0)
				{
					amountPayable = -amountPayable;
					Description = RefundDescription;
				}
				else
				{
					Description = PaymentDescription;
				}
				Amount = "$" + amountPayable.ToString(2);
			}
			public readonly ZString Amount;
			public readonly ZString Description;
		}
		public const string PaymentDescription = "Total Amount Payable";
		public const string RefundDescription = "Total Refund Amount";

		#endregion

		public ZString DepositRefund
		{
			get { return EntryHeader.DepositRefundAmount.IsEmpty ? "" : "$" + EntryHeader.DepositRefundAmount.ToString(2); }
		}
		#endregion
	}
}
