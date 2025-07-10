using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class InvoiceDependentJob : NonPersistentBusinessObject, IObsoleteValidation
	{
		public InvoiceDependentJob(BusinessObjectFactory factory, InvoicingBase invoice, Job job)
			: base(factory)
		{
			this.invoice = invoice;
			Job = job;
		}

		public Job Job { get; set; }

		#region Variables
		readonly InvoicingBase invoice;
		#endregion

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public abstract class Schema : DependentJob<InvoicingBase>.Schema
		{
			public const string JH_RelatedInvoiceLinesTotalCostInInvoiceCurrency = "JH_RelatedInvoiceLinesTotalCostInInvoiceCurrency";
			public const string JH_RelatedInvoiceLinesCostAmountInInvoiceCurrency = "JH_RelatedInvoiceLinesCostAmountInInvoiceCurrency";
			public const string JH_RelatedInvoiceLinesCostTaxAmountInInvoiceCurrency = "JH_RelatedInvoiceLinesCostTaxAmountInInvoiceCurrency";
			public const string JH_RelatedInvoiceLinesTotalCost = "JH_RelatedInvoiceLinesTotalCost";
			public const string JH_RelatedInvoiceLinesCostAmount = "JH_RelatedInvoiceLinesCostAmount";
			public const string JH_RelatedInvoiceLinesCostTaxAmount = "JH_RelatedInvoiceLinesCostTaxAmount";
			public const string JH_InvoiceCurrencyCode = "JH_InvoiceCurrencyCode";
		}

		#endregion

		#region JH_RelatedInvoiceLinesTotalCostInInvoiceCurrency

		public ZDecimal JH_RelatedInvoiceLinesTotalCostInInvoiceCurrency
		{
			get
			{
				ZDecimal result = 0m;
				foreach (InvoicingLineBase line in RelatedInvoiceLines)
				{
					result -= line.AL_OSAmount;
				}
				return result;
			}
		}

		public ZPropertyInfo JH_RelatedInvoiceLinesTotalCostInInvoiceCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.JH_RelatedInvoiceLinesTotalCostInInvoiceCurrency); }
		}

		#endregion

		#region JH_RelatedInvoiceLinesCostAmountInInvoiceCurrency

		public ZDecimal JH_RelatedInvoiceLinesCostAmountInInvoiceCurrency
		{
			get
			{
				ZDecimal result = 0m;
				foreach (InvoicingLineBase line in RelatedInvoiceLines)
				{
					result += line.AL_OSExTaxAmount;
				}
				return result;
			}
		}

		public ZPropertyInfo JH_RelatedInvoiceLinesCostAmountInInvoiceCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.JH_RelatedInvoiceLinesCostAmountInInvoiceCurrency); }
		}

		#endregion

		#region JH_RelatedInvoiceLinesCostTaxAmountInInvoiceCurrency

		public ZDecimal JH_RelatedInvoiceLinesCostTaxAmountInInvoiceCurrency
		{
			get
			{
				ZDecimal result = 0m;
				foreach (InvoicingLineBase line in RelatedInvoiceLines)
				{
					result += line.AL_OSTaxAmount;
				}
				return result;
			}
		}

		public ZPropertyInfo JH_RelatedInvoiceLinesCostTaxAmountInInvoiceCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.JH_RelatedInvoiceLinesCostTaxAmountInInvoiceCurrency); }
		}

		#endregion

		#region JH_RelatedInvoiceLinesTotalCost

		public ZDecimal JH_RelatedInvoiceLinesTotalCost
		{
			get
			{
				ZDecimal result = 0m;
				foreach (InvoicingLineBase line in RelatedInvoiceLines)
				{
					result += line.AL_LocalTotalAmount;
				}
				return result;
			}
		}

		public ZPropertyInfo JH_RelatedInvoiceLinesTotalCostInfo
		{
			get { return GetZPropertyInfo(Schema.JH_RelatedInvoiceLinesTotalCost); }
		}

		#endregion

		#region JH_RelatedInvoiceLinesCostAmount

		public ZDecimal JH_RelatedInvoiceLinesCostAmount
		{
			get
			{
				ZDecimal result = 0m;
				foreach (InvoicingLineBase line in RelatedInvoiceLines)
				{
					result += line.AL_LocalExTaxAmount;
				}
				return result;
			}
		}

		public ZPropertyInfo JH_RelatedInvoiceLinesCostAmountInfo
		{
			get { return GetZPropertyInfo(Schema.JH_RelatedInvoiceLinesCostAmount); }
		}

		#endregion

		#region JH_RelatedInvoiceLinesCostTaxAmount

		public ZDecimal JH_RelatedInvoiceLinesCostTaxAmount
		{
			get
			{
				ZDecimal result = 0m;
				foreach (InvoicingLineBase line in RelatedInvoiceLines)
				{
					result += line.AL_LocalTaxAmount;
				}
				return result;
			}
		}

		public ZPropertyInfo JH_RelatedInvoiceLinesCostTaxAmountInfo
		{
			get { return GetZPropertyInfo(Schema.JH_RelatedInvoiceLinesCostTaxAmount); }
		}

		#endregion

		#region JH_InvoiceCurrency

		public ZString JH_InvoiceCurrencyCode
		{
			get { return invoice.AH_RX_NKTransactionCurrency; }
		}

		public ZPropertyInfo JH_InvoiceCurrencyCodeInfo
		{
			get { return GetZPropertyInfo(Schema.JH_InvoiceCurrencyCode); }
		}

		#endregion

		#region JH_ParentID
		public ZGuid JH_ParentID
		{
			get
			{
				if (Job != null)
				{
					return Job.JH_ParentID;
				}
				else
				{
					return new ZGuid();
				}
			}
		}

		public ZPropertyInfo JH_ParentIDInfo
		{
			get { return GetZPropertyInfo(Schema.JH_ParentID); }
		}
		#endregion

		#region JH_JobNum
		public ZString JH_JobNum
		{
			get
			{
				if (Job != null)
				{
					return Job.JH_JobNum;
				}
				else
				{
					return string.Empty;
				}
			}
		}

		public ZPropertyInfo JH_JobNumInfo
		{
			get { return GetZPropertyInfo(Schema.JH_JobNum); }
		}
		#endregion

		#region JH_HouseBillNo
		public ZString JH_HouseBillNo
		{
			get
			{
				if (Job != null)
				{
					return Job.JH_HouseBillNo;
				}
				else
				{
					return string.Empty;
				}
			}
		}

		public ZPropertyInfo JH_HouseBillNoInfo
		{
			get { return GetZPropertyInfo(Schema.JH_HouseBillNo); }
		}
		#endregion

		#region Collections

		IEnumerable<InvoicingLineBase> fRelatedInvoiceLines;
		public  IEnumerable<InvoicingLineBase> RelatedInvoiceLines
		{
			get
			{
				fRelatedInvoiceLines = from line in invoice.Lines
									   where ((InvoicingLineBase)line).AL_JH == Job.PK
									   select ((InvoicingLineBase)line);
				return fRelatedInvoiceLines;
			}
#if DEBUG
			set
			{
				fRelatedInvoiceLines = value;
			}
#endif
		}

		#endregion
	}
}
