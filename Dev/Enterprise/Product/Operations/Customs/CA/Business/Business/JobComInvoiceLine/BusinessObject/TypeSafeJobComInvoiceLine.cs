
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.CA.Business
{
	public partial class JobComInvoiceLine
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new JobComInvoiceLine Clone()
		{
			return (JobComInvoiceLine)base.Clone();
		}

		public new JobComInvoiceHeader InvoiceHeader
		{
			get { return (JobComInvoiceHeader)base.InvoiceHeader; }
		}

		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		public new JobComInvoiceLineLookups Lookups
		{
			get { return (JobComInvoiceLineLookups)base.Lookups; }
		}

		public new JobComInvoiceLineValidation Validation
		{
			get { return (JobComInvoiceLineValidation)base.Validation; }
		}

		[ChildEditable(true)]
		public new JobComInvChargeCollection<InvoiceLineCharge> Charges
		{
			get { return (JobComInvChargeCollection<InvoiceLineCharge>)base.Charges; }
		}

		[ChildEditable(true)]
		public new JobComInvApportionedChargeCollection<InvoiceLineApportionCharge> ApportionedCharges => (JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>)base.ApportionedCharges;

		public new CusClassPartPivot Pivot
		{
			get { return (CusClassPartPivot)base.Pivot; }
		}

		public new OrgSupplierPart Part
		{
			get { return (OrgSupplierPart)base.Part; }
		}

		#endregion

		#region Implementation

		#region protected override

		protected override Customs.Business.JobComInvoiceLineLookups GetNewLookups()
		{
			return new JobComInvoiceLineLookups(this);
		}

		protected override Customs.Business.JobComInvoiceLineValidation GetNewValidation()
		{
			if (InvoiceHeader?.IsImport ?? ZBool.False)
			{
				return new ImportJobComInvoiceLineValidation(this);
			}
			else if (InvoiceHeader?.IsExport ?? ZBool.False)
			{
				return new ExportJobComInvoiceLineValidation(this);
			}
			else if (Declaration != null && (Declaration.IsB2Adjustments || Declaration.IsB3X))
			{
				return new B2JobComInvoiceLineValidation(this);
			}
			else
			{
				return new JobComInvoiceLineValidation(this);
			}
		}

		protected override IJobComInvApportionedChargeCollection<BaseInvoiceLineApportionedCharge> CreateNewInvoiceLineApportionedChargeCollection()
		{
			return new JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>(this);
		}

		protected override IJobComInvChargeCollection<BaseInvoiceLineCharge> CreateNewInvoiceLineChargeCollection()
		{
			return new JobComInvChargeCollection<InvoiceLineCharge>(this);
		}

		protected override Customs.Business.TariffFormatter TariffFormatter
		{
			get { return new TariffFormatter(); }
		}

		#endregion

		#endregion
	}
}
