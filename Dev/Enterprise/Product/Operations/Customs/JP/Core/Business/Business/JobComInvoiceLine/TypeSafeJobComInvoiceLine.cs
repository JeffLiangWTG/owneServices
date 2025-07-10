using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.JP.Business
{
	public partial class JobComInvoiceLine : AutoJPJobComInvoiceLine
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new JobComInvoiceLine Clone()
		{
			return (JobComInvoiceLine)base.Clone();
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

		public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

		[ChildEditable(true)]
		public new JobComInvApportionedChargeCollection<InvoiceLineApportionCharge> ApportionedCharges => (JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>)base.ApportionedCharges;

		[ChildEditable(true)]
		public ICusLineTariffDetailCollection<CusLineTariffDetail> DomesticConsumptionTaxes => (CusLineTariffDetailCollection<CusLineTariffDetail>)base.CusLineTariffDetails;

		#endregion

		#region protected override

		protected override Customs.Business.JobComInvoiceLineLookups GetNewLookups()
		{
			return new JobComInvoiceLineLookups(this);
		}

		protected override Customs.Business.JobComInvoiceLineValidation GetNewValidation()
		{
			return new JobComInvoiceLineValidation(this);
		}

		protected override IJobComInvApportionedChargeCollection<BaseInvoiceLineApportionedCharge> CreateNewInvoiceLineApportionedChargeCollection() => new JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>(this);

		protected override IJobComInvChargeCollection<BaseInvoiceLineCharge> CreateNewInvoiceLineChargeCollection()
		{
			return new JobComInvChargeCollection<InvoiceLineCharge>(this);
		}

		protected override ICusLineTariffDetailCollection<Customs.Business.CusLineTariffDetail> GetCusLineTariffDetails() => new CusLineTariffDetailCollection(this);

		protected override bool SupportsAdditionalTariffs => true;

		#endregion
	}
}
