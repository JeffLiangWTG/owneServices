using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	public class InvoiceLineViewCollection : InvoiceLineViewCollection<JobComInvoiceLine>
	{
		public InvoiceLineViewCollection(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
		}

		protected override System.Collections.IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			if (property.Name == JobComInvoiceLine.Schema.JI_B3LineNumber)
			{
				return new CAInvoiceLineComparer(direction == ListSortDirection.Descending, JobComInvoiceLine.Schema.JI_B3LineNumber);
			}
			return base.GetComparerForSort(property, direction);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			using (child.GetValidationSuspender())
			{
				var line = (JobComInvoiceLine)child;
				var declaration = line.Declaration;
				if (Count > 0)
				{
					var previousLine = this[Count - 1];
					line.CA_PageNumber = previousLine.CA_PageNumber;
					if (declaration != null && declaration.IsLVS)
					{
						line.CA_TreatmentCode = previousLine.CA_TreatmentCode;
						line.JI_CountryOfOrigin = previousLine.JI_CountryOfOrigin;
						line.JI_StateOrRegionOfOrigin = previousLine.JI_StateOrRegionOfOrigin;
						line.CA_RN_NKExport = previousLine.CA_RN_NKExport;
						line.CA_USStateOfExport = previousLine.CA_USStateOfExport;
					}
				}
				else
				{
					line.CA_PageNumber = 1;
				}

				var header = line.InvoiceHeader;
				if (header != null && header.JZ_IncoTerm == Core.Constants.IncoTerms.DeliveredDutyPaid)
				{
					line.CA_CalculationMethod = CalculationMethods.Codes.DeliveredDutyPaid;
				}

				line.SetAVSStatusIfNeeded();

				if (declaration != null && declaration.IsPersistent && declaration.IsIID)
				{
					var packages = declaration.Packages.LowestPackages;
					if (packages.Count() == 1)
					{
						var package = packages.First();
						if (line.InvoiceHeader.PackagesPivot.Cast<InvoiceHeaderPackagePivot>().All(p => p.Package.PK != package.PK))
						{
							line.ToggleLinkageWithPackage(package, true);
						}
					}
				}
			}
		}

		protected override BusinessObject AddNewCore()
		{
			var newLine = base.AddNewCore();
			SetDefaultsForNewChild(newLine);
			return newLine;
		}

		protected override void CopyLastLineDetailsToNewLinesIfEnabled(JobComInvoiceLine newLine, JobComInvoiceLine previousLine)
		{
			base.CopyLastLineDetailsToNewLinesIfEnabled(newLine, previousLine);
			if (CopyLastLineDetailsToNewLines)
			{
				CAPGADetailsCopyHelper.CopyPGADetails(newLine, previousLine);
			}
		}

		public override IDisposable SuspendAdditionallyForImport()
		{
			return new CAInvoiceLineImportSuspender(Declaration);
		}

		protected class CAInvoiceLineImportSuspender : InvoiceLineImportSuspender
		{
			public CAInvoiceLineImportSuspender(BaseJobDeclaration jobDeclaration)
				: base(jobDeclaration)
			{
			}

			protected override void DisposeCore(BaseJobDeclaration jobDeclaration)
			{
				base.DisposeCore(jobDeclaration);
				PageNumberCalculator.RecalculatePageNumber((JobDeclaration)jobDeclaration, 0);
			}
		}
	}
}
