using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.MasterFiles;

namespace Enterprise.Customs.EU.Business.Declaration
{
	partial class JobComInvoiceLine : AutoJobComInvoiceLine
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new CusClassification Classification => (CusClassification)base.Classification;

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		public new JobComInvoiceLineLookups Lookups => (JobComInvoiceLineLookups)base.Lookups;

		public new JobComInvoiceLineValidation Validation => (JobComInvoiceLineValidation)base.Validation;

		[ChildEditable(true)]
		public new IJobComInvChargeCollection<InvoiceLineCharge> Charges => (IJobComInvChargeCollection<InvoiceLineCharge>)base.Charges;

		[ChildEditable(true)]
		public new IJobComInvApportionedChargeCollection<InvoiceLineApportionCharge> ApportionedCharges => (IJobComInvApportionedChargeCollection<InvoiceLineApportionCharge>)base.ApportionedCharges;

		#endregion

		#region Implementation

		#region Overridden 'CreateNew' methods

		protected override Customs.Business.JobComInvoiceLineLookups GetNewLookups()
		{
			return InvoiceHeader?.JobDeclaration?.GetJobComInvoiceLineLookups(this) ?? new JobComInvoiceLineLookups(this);
		}

		protected override Customs.Business.JobComInvoiceLineValidation GetNewValidation()
		{
			var declaration = InvoiceHeader?.JobDeclaration;
			if (declaration != null)
			{
				return declaration.GetJobComInvoiceLineValidation(this);
			}
			else
			{
				// Stand alones
				return new JobComInvoiceLineValidation(this);
			}
		}

		protected override IJobComInvApportionedChargeCollection<BaseInvoiceLineApportionedCharge> CreateNewInvoiceLineApportionedChargeCollection()
		{
			return new JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>(this);
		}

		protected override IJobComInvChargeCollection<BaseInvoiceLineCharge> CreateNewInvoiceLineChargeCollection()
		{
			return new InvoiceLineChargeCollection<InvoiceLineCharge>(this);
		}

		#endregion

		protected override Customs.Business.TariffFormatter TariffFormatter
		{
			get { return Business.TariffFormatter.New(Declaration?.CountryCode); }
		}
		#endregion
	}
}
