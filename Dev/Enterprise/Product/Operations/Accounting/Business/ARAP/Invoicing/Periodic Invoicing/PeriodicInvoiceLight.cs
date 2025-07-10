using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Business.ARAP.Invoicing.PeriodicInvoiceBulkPoster;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public abstract class PeriodicInvoiceLight : PeriodicInvoice, ILightValidationInternals
	{
		protected PeriodicInvoiceLight(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected void Initialize(InvoiceInfo invoiceInfo)
		{
			using (ClearJobsSuspender.GetSuspender())
			using (ClearMiscInvoicesSuspender.GetSuspender())
			using (ReloadChargesSuspender.GetSuspender())
			{
				DebtorPK = invoiceInfo.DebtorPK;
				InvoiceType = invoiceInfo.InvoiceType;
				CurrencyNK = invoiceInfo.CurrencyNK;
				InvoiceDate = invoiceInfo.InvoiceDate;
				PostDate = invoiceInfo.PostDate;
				SelectedJobPKs = invoiceInfo.SelectedJobs.ToList();
				ChargePKs = invoiceInfo.Charges.ToList();
				MiscInvoicePKs = invoiceInfo.MiscInvoices.ToList();
				LocalExTaxAmountExpected = invoiceInfo.LocalExTaxAmount;
				LocalTaxAmountExpected = invoiceInfo.LocalTaxAmount;
				SellReference = invoiceInfo.SellReference;
				TaxBranch = invoiceInfo.TaxBranch;
				InvoiceTerm = invoiceInfo.InvoiceTerm;
				InvoiceTermDays = invoiceInfo.InvoiceTermDays;
				DueDate = invoiceInfo.DueDate;
			}
			LoadJobsLight();
			LoadMiscInvoicesLight();
		}

		protected abstract void LoadJobsLight();
		protected abstract void LoadMiscInvoicesLight();

		protected List<ZGuid> SelectedJobPKs
		{
			get { return selectedJobPKs; }
			private set
			{
				selectedJobPKs.Clear();
				selectedJobPKs.AddRange(value);
				ClearJobs();
			}
		}
		readonly List<ZGuid> selectedJobPKs = new List<ZGuid>();

		protected List<ZGuid> ChargePKs
		{
			get { return chargePKs; }
			private set
			{
				chargePKs.Clear();
				chargePKs.AddRange(value);
				ReloadCharges();
			}
		}
		readonly List<ZGuid> chargePKs = new List<ZGuid>();

		protected List<ZGuid> MiscInvoicePKs
		{
			get { return miscInvoicePKs; }
			private set
			{
				miscInvoicePKs.Clear();
				miscInvoicePKs.AddRange(value);
				ClearMiscInvoices();
			}
		}
		readonly List<ZGuid> miscInvoicePKs = new List<ZGuid>();

		protected ZDecimal LocalExTaxAmountExpected
		{
			get;
			private set;
		}

		protected ZDecimal LocalTaxAmountExpected
		{
			get;
			private set;
		}

		protected override void LoadJobsCore()
		{
			Jobs.LoadByPKInBatches<Job>(JobHeaderSchema.PK, SelectedJobPKs);
		}

		protected override void ReloadChargesCore()
		{
			if (Jobs.Count > 0)
			{
				Charges.LoadByPKInBatches(Factory, JobChargeSchema.PK, ChargePKs);
			}
		}

		protected override void LoadMiscInvoicesCore()
		{
			MiscInvoices.LoadByPKInBatches<InvoicingBase>(AccTransactionHeaderSchema.PK, MiscInvoicePKs);
		}

		#region ILightValidationInternals Members

		ZBool ILightValidationInternals.IsValid
		{
			get;
			set;
		}

		bool ILightValidationInternals.IsValidHasChanges
		{
			get { return true; }
		}

		SchemaBoolColumn ILightValidationInternals.IsValidSchemaColumn
		{
			get { return null; }
		}

		#endregion
	}

	public class PeriodicInvoiceLightForCheckSecurityRights : PeriodicInvoiceLight
	{
		public PeriodicInvoiceLightForCheckSecurityRights(BusinessObjectFactory factory)
			: base(factory)
		{ }

		public void InitalizeFromInvoiceInfo(InvoiceInfo invoiceInfo)
		{
			Initialize(invoiceInfo);
		}

		protected override void LoadJobsLight()
		{
			ClearJobs();
			InitializeFilterForJobs();
			using (ReloadChargesSuspender.GetSuspender())
			{
				LoadJobsCore();
			}
		}

		protected override void LoadMiscInvoicesLight()
		{ }

		protected override void RunPreSaveValidationCore()
		{ }
	}

	public class PeriodicInvoiceLightForBulkPosting : PeriodicInvoiceLight
	{
		public PeriodicInvoiceLightForBulkPosting(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public string[] InitalizeAndValidate(InvoiceInfo invoiceInfo)
		{
			Initialize(invoiceInfo);

			((IBusiness)this).ValidateIfQuickAndImprovesPreSaveValidationPerformance();

			return this.NotificationsIncludingChildren
				.GetErrors()
				.GetUniqueMessageList();
		}

		protected override void RunPreSaveValidationCore()
		{
			ClearRowNotifications();

			if (Jobs.Count != SelectedJobPKs.Count)
			{
				AddRowError(Res.GetString("35086280-4217-4E7F-91C9-D8A7184D14D3", "Job count for the invoice was changed from {0} to {1}.", SelectedJobPKs.Count, Jobs.Count));
			}
			if (Charges.Count != ChargePKs.Count)
			{
				AddRowError(Res.GetString("30B87E1C-78AB-4BC8-8ADE-5E8615B24FC2", "Charge count for the invoice was changed from {0} to {1}.", ChargePKs.Count, Charges.Count));
			}
			if (MiscInvoices.Count != MiscInvoicePKs.Count)
			{
				AddRowError(Res.GetString("55a9002f-43f7-4fad-a451-750174195802", "Miscellaneous Invoice count for the invoice was changed from {0} to {1}.", MiscInvoicePKs.Count, MiscInvoices.Count));
			}
			if (LocalExTaxAmount != LocalExTaxAmountExpected)
			{
				AddRowError(Res.GetString("20eab7b9-6e0f-4fb9-9448-3c3eac9943e0", "Total Local Ex Tax Amount of this invoice has changed from {0} to {1}.",
					FormatLocalAmount(LocalExTaxAmountExpected), FormatLocalAmount(LocalExTaxAmount)));
			}
			if (LocalTaxAmount != LocalTaxAmountExpected)
			{
				AddRowError(Res.GetString("696c244d-f535-46c2-b49c-7b13a190ee3e", "Total Local Tax Amount of this invoice has changed from {0} to {1}.",
					FormatLocalAmount(LocalTaxAmountExpected), FormatLocalAmount(LocalTaxAmount)));
			}
		}

		string FormatLocalAmount(ZDecimal localAmount)
		{
			return localAmount.ToString("C", Culture.CurrentCompanyCountryCulture.NumberFormat);
		}

		protected override void LoadJobsLight()
		{
			LoadJobs();
		}

		protected override void LoadMiscInvoicesLight()
		{
			LoadMiscInvoices();
		}
	}
}
