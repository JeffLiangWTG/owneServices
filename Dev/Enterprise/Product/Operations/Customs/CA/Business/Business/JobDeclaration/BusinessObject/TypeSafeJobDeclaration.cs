using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Integration.Customs.CA;

namespace Enterprise.Customs.CA.Business
{
	public partial class JobDeclaration
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new JobDeclaration ParentRelatedDeclaration
		{
			get { return (JobDeclaration)base.ParentRelatedDeclaration; }
		}
		public new JobDeclaration Clone()
		{
			return (JobDeclaration)base.Clone();
		}

		public new ICusContainerCollection<CusContainer> CusContainers
		{
			get { return (ICusContainerCollection<CusContainer>)base.CusContainers; }
		}

		public new IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader> JobComInvoiceGroupHeaders
		{
			get { return (IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>)base.JobComInvoiceGroupHeaders; }
		}

		public new JobDeclarationValidation Validation
		{
			get { return (JobDeclarationValidation)base.Validation; }
		}

		public new JobDeclarationLookups Lookups
		{
			get { return (JobDeclarationLookups)base.Lookups; }
		}

		public new ICusEntryHeaderCollection<CusEntryHeader> CustomsEntryHeaders => (ICusEntryHeaderCollection<CusEntryHeader>)base.CustomsEntryHeaders;

		public new InvoiceHeaderActiveCollection Invoices
		{
			get { return (InvoiceHeaderActiveCollection)base.Invoices; }
		}

		public new InvoiceLineViewCollection FilteredInvoiceLines
		{
			get
			{
				var result = (InvoiceLineViewCollection)base.FilteredInvoiceLines;
				foreach (var line in from JobComInvoiceLine line in result where line.RepairLineSynchroniser != null select line)
				{
					line.RepairLineSynchroniser.SetEnabled(true, line.RepairLineSynchroniser.DetectEnabled);
				}
				return result;
			}
		}

		[ChildEditable(true)]
		public new InvoiceLineCompleteCollection InvoiceLines
		{
			get { return (InvoiceLineCompleteCollection)base.InvoiceLines; }
		}

		[ChildEditable(true)]
		public new BillCollection Bills
		{
			get { return (BillCollection)base.Bills; }
		}

		BusinessObjectCollection IJobDeclaration.Bills => Bills;

		public new ActiveCusEntryHeaderCollection ActiveEntryHeaders
		{
			get { return (ActiveCusEntryHeaderCollection)base.ActiveEntryHeaders; }
		}

		IActiveCusEntryHeaderCollection IJobDeclaration.ActiveEntryHeaders => ActiveEntryHeaders;

		#endregion

		#region Implementation

		#region Protected override

		protected override ICusContainerCollection<BaseCusContainer> NewCusContainersCollection()
		{
			return new BaseCusContainerCollection<CusContainer>(this, Factory);
		}

		protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection()
		{
			return new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(this);
		}

		protected override Customs.Business.JobDeclarationValidation GetNewValidation()
		{
			JobDeclarationValidation result;
			if (IsImport)
			{
				result = new ImportJobDeclarationValidation(this);
			}
			else if (IsExport)
			{
				result = new ExportJobDeclarationValidation(this);
			}
			else if (IsB2Adjustments || IsB3X)
			{
				result = new B2JobDeclarationValidation(this);
			}
			else
			{
				result = new JobDeclarationValidation(this);
			}
			return result;
		}

		protected override Customs.Business.JobDeclarationLookups GetNewLookups()
		{
			return new JobDeclarationLookups(this);
		}

		protected override ICusEntryHeaderCollection<Customs.Business.CusEntryHeader> NewCustomsEntryHeaders() => new CusEntryHeaderCollection<CusEntryHeader>(this, Factory);

		protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewInvoiceHeaderCollection()
		{
			return new InvoiceHeaderActiveCollection(this, GenPivotTypeDecider.Types.InvoiceRelatedDeclarationGenPivot);
		}

		protected override IInvoiceLineViewCollection<BaseJobComInvoiceLine> GetNewInvoiceLineViewCollection()
		{
			return new InvoiceLineViewCollection(this);
		}

		protected override IBillCollection<Customs.Business.Bill, BaseJobDeclaration> CreateNewBillCollection()
		{
			return new BillCollection(this, Factory);
		}

		protected override Customs.Business.InvoiceLineCompleteCollection GetNewInvoiceLineCompleteCollection()
		{
			return new InvoiceLineCompleteCollection(this);
		}

		protected override Customs.Business.ActiveCusEntryHeaderCollection GetActiveEntryHeaderCollection()
		{
			return new ActiveCusEntryHeaderCollection(this);
		}

		protected override DocumentSupporter CreateNewDocumentSupporter()
		{
			return new JobDeclarationDocumentSupporter(this);
		}

		#endregion

		#endregion
	}
}
