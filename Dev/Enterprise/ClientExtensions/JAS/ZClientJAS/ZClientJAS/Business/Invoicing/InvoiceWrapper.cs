using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.JAS.Business.JXC;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.Invoicing
{
	public class InvoiceWrapper : NonPersistentBusinessObject, IJXCExportHeader, IObsoleteValidation
	{
		public InvoiceWrapper(IJASInvoicingBase jASInvoicingBase)
			: base(jASInvoicingBase.Factory)
		{
			this.JASInvoicingBase = jASInvoicingBase;
			if (jASInvoicingBase.InvoicingBase == null)
			{
				throw new ArgumentNullException(nameof(jASInvoicingBase), "JASInvoicingBase.InvoicingBase cannot be null.");
			}
			RegisterEditableChildObject(Invoice);
		}

		#region IJXCExportHeader Members

		public JASOrgHeader SendingForwarder
		{
			get { return GlbBranch.CurrentBranch.OrgProxy as JASOrgHeader; }
		}

		public JASOrgHeader ReceivingForwarder
		{
			get { return (JASOrgHeader)Invoice.Header; }
		}

		public ZString FreightDest
		{
			get
			{
				ZString result = "";

				if (Shipment != null)
				{
					result = Shipment.JS_RL_NKDestination;
				}
				else if (Invoice.Header != null)
				{
					result = Invoice.Header.OH_RL_NKClosestPort;
				}

				return result;
			}
		}

		#endregion

		public JASForwardingShipment Shipment
		{
			get
			{
				if (fShipment == null)
				{
					JobHeader job = (Invoice.IsBelongToMultipleJobs || Invoice.IsConsolInvoice)
							? FindJobFromInvoiceLines(Invoice.Lines)
							: Invoice.Job;
					fShipment = GetShipmentFromJob(job);
				}
				return fShipment;
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Invoice.HumanReadableName; }
		}

		JobHeader FindJobFromInvoiceLines(InvoicingLineBaseCollection lines)
		{
			foreach (InvoicingLineBase line in lines)
			{
				if (line.Job != null)
				{
					return line.Job;
				}
			}

			return null;
		}

		JASForwardingShipment GetShipmentFromJob(JobHeader job)
		{
			JASForwardingShipment result = null;

			if (job != null && job.JH_ParentTableCode == JobShipmentSchema.Constants.Prefix)
			{
				result = (JASForwardingShipment)Factory.Load(typeof(JASForwardingShipment), job.JH_ParentID);
			}

			return result;
		}

		public InvoicingBase Invoice
		{
			get { return JASInvoicingBase.InvoicingBase; }
		}

		public readonly IJASInvoicingBase JASInvoicingBase;

		public
 JASForwardingShipment fShipment;
	}
}
