using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocCashAdvanceRequestLine : DocumentWrapper, IGenericTransactionLinePlugIn
	{
		protected DocCashAdvanceRequestLine(AccCashAdvanceRequestLine line, BusinessObjectFactory factoryToWrap)
			: base(line, factoryToWrap)
		{
		}

		protected AccCashAdvanceRequestLine Line
		{
			get { return (AccCashAdvanceRequestLine)WrappedObject; }
		}

		public static DocCashAdvanceRequestLine New(AccCashAdvanceRequestLine line, BusinessObjectFactory factoryToWrap)
		{
			return (line != null) ? new DocCashAdvanceRequestLine(line, factoryToWrap) : null;
		}

		#region IGenericTransactionLinePlugIn members

		GenericTransactionLineSupporter IGenericTransactionLinePlugIn.LineSupporter
		{
			get { return fGenericTransactionSupporter ?? (fGenericTransactionSupporter = new DocCashAdvanceRequestLineGenericTransactionSupporter(this)); }
		}
		DocCashAdvanceRequestLineGenericTransactionSupporter fGenericTransactionSupporter;

		#endregion

		class DocCashAdvanceRequestLineGenericTransactionSupporter : GenericTransactionLineSupporter
		{
			public DocCashAdvanceRequestLineGenericTransactionSupporter(DocCashAdvanceRequestLine parent)
			{
				this.Parent = parent;
			}
			protected readonly DocCashAdvanceRequestLine Parent;

			protected internal override ZDecimal GetOSAmount()
			{
				return Parent.OSAmount;
			}

			protected internal override ZDecimal GetOSPaidAmount()
			{
				return Parent.OSPaidAmount;
			}

			protected internal override ZDecimal GetLocalTotalAmount()
			{
				return Parent.LocalAmount;
			}

			protected internal override ZDecimal GetLocalPaidAmount()
			{
				return Parent.LocalPaidAmount;
			}

			protected internal override ZString GetStatus()
			{
				return Parent.Status;
			}

			protected internal override ZString GetDescription()
			{
				return Parent.Description;
			}
		}

		public override string ToString()
		{
			return ZString.Empty;
		}

		public DocCashAdvanceRequestHeader CashAdvanceRequest
		{
			get { return DocCashAdvanceRequestHeader.New(Line.RequestHeader, Factory); }
		}

		public ZDecimal OSAmount => Line.CAL_OSAmount;
		public ZDecimal OSPaidAmount => Line.CAL_OSPaidAmount;
		public ZDecimal LocalAmount => Line.CAL_LocalAmount;
		public ZDecimal LocalPaidAmount => Line.CAL_LocalPaidAmount;
		public ZString Status => Line.CAL_Status;
		public ZString Description => Line.RelatedChargeDescription;
	}
}
