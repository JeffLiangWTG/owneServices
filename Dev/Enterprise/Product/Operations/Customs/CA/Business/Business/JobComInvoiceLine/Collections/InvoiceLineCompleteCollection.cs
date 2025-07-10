using System;
using System.Linq;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	public class InvoiceLineCompleteCollection : TypeSafeInvoiceLineCompleteCollection
	{
		public InvoiceLineCompleteCollection(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
			AllowRebuildSubCollection = true;
		}

		public void RunCFIAValidation()
		{
			foreach (JobComInvoiceLine line in this)
			{
				((ImportAddInfoJobComInvoiceLineValidation)line.AddInfoValidation).ValidateCFIA();
			}
		}

		public void RunSITTValidation()
		{
			foreach (JobComInvoiceLine line in this)
			{
				((ImportAddInfoJobComInvoiceLineValidation)line.AddInfoValidation).ValidateSITT();
				((ImportJobComInvoiceLineValidation)line.Validation).ValidateJI_BrandName();
			}
		}

		public void RunNRCANValidation()
		{
			foreach (JobComInvoiceLine line in this)
			{
				((ImportAddInfoJobComInvoiceLineValidation)line.AddInfoValidation).ValidateNRCAN();
				((ImportJobComInvoiceLineValidation)line.Validation).ValidateJI_BrandName();
			}
		}

		public void RunTiresValidation()
		{
			foreach (JobComInvoiceLine line in this)
			{
				((ImportAddInfoJobComInvoiceLineValidation)line.AddInfoValidation).ValidateTires();
			}
		}

		public void LoadWithoutRebuildSubCollection()
		{
			using (SuspendRebuildSubCollection())
			{
				Load();
			}
		}

		public bool AllowRebuildSubCollection { get; private set; }

		protected override void OnRemoving(BusinessObject bizO)
		{
			base.OnRemoving(bizO);
			if (((JobComInvoiceLine)bizO).IsRegulatedByCFIA)
			{
				JobDeclaration.AVSEventRequired = true;
			}
		}

		protected override bool ShouldAutoAllocatePackageToInvoiceLine(BaseJobComInvoiceLine invoiceLine)
		{
			var result = base.ShouldAutoAllocatePackageToInvoiceLine(invoiceLine);
			if (result && JobDeclaration.Packages.Count == 1)
			{
				var package = JobDeclaration.Packages[0];
				result = invoiceLine.InvoiceHeader?.PackagesPivot.Cast<InvoiceHeaderPackagePivot>().All(p => p.Package.PK != package.PK) ?? false;
			}

			return result;
		}

		protected override void SetDefaultForCommonInvoiceLine(BaseJobComInvoiceLine invoiceLine)
		{
			base.SetDefaultForCommonInvoiceLine(invoiceLine);
			if (JobDeclaration != null && !JobDeclaration.IsIID && JobDeclaration.CA_OGDCFIA)
			{
				((JobComInvoiceLine)invoiceLine).AddDefaultCFIARegistrationNumber();
			}
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			if (JobDeclaration.IsImport)
			{
				var header = (bizO as JobComInvoiceLine).InvoiceHeader;
				PageNumberCalculator.RecalculatePageNumber(JobDeclaration, header?.JZ_InvoiceDisplaySequence ?? 0);
			}
		}

		protected sealed class SubCollectionRebuildSuspender : IDisposable
		{
			internal SubCollectionRebuildSuspender(InvoiceLineCompleteCollection collection)
			{
				this.collection = collection;
				collection.AllowRebuildSubCollection = false;
				DisposableLeakListener.Instance.RegisterDisposable(this);
			}

			bool disposed;
			readonly InvoiceLineCompleteCollection collection;

			public void Dispose()
			{
				if (!disposed)
				{
					collection.AllowRebuildSubCollection = true;
					DisposableLeakListener.Instance.UnRegisterDisposable(this);
					disposed = true;
				}
			}
		}

		IDisposable SuspendRebuildSubCollection() => new SubCollectionRebuildSuspender(this);
	}
}
