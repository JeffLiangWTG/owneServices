using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public partial class JobComInvoiceGroupHeader : EU.Business.Declaration.JobComInvoiceGroupHeader, Integration.Customs.FR.IJobComInvoiceGroupHeader
	{
		public JobComInvoiceGroupHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewDirectInvoiceHeaderCollection() => new InvoiceHeaderActiveCollection(this, true);

		public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;
		
		public new InvoiceHeaderActiveCollection JobComInvoiceHeaders => (InvoiceHeaderActiveCollection)base.JobComInvoiceHeaders;

		protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection() => new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(this);

		public new GroupInvoiceChargeCollection<GroupInvoiceCharge> Charges => (GroupInvoiceChargeCollection<GroupInvoiceCharge>)base.Charges;

		protected override IJobComInvChargeCollection<BaseGroupInvoiceCharge> CreateGroupInvoiceChargeCollection() => new GroupInvoiceChargeCollection<GroupInvoiceCharge>(this);

		public void UpdateCharges()
		{
			var declaration = JobDeclaration;

			if ((declaration.IsImport && IncoTermAndChargeFactory is ExportIncoTermAndCustomsChargeFactory) || (!declaration.IsImport && !(IncoTermAndChargeFactory is ExportIncoTermAndCustomsChargeFactory)))
			{
				declaration.RefreshIncotermAndChargeFactory();
			}

			var dictionary = declaration.IsImport ? ((IncoTermAndCustomsChargeFactory)IncoTermAndChargeFactory).CustomsChargeCodeDictionary : ((ExportIncoTermAndCustomsChargeFactory)IncoTermAndChargeFactory).CustomsChargeCodeDictionary;

			var requiredCharges = new HashSet<MessageChargeKey>();
			foreach (JobComInvoiceHeader invoice in declaration.Invoices)
			{
				var chargeConfigKey = new IncoTermChargesConfigurationKey(invoice);
				if (dictionary != null && dictionary.TryGetValue(chargeConfigKey, out var chargeCodeList))
				{
					requiredCharges.UnionWith(chargeCodeList.Select(c => c.MessageChargeKey));
				}
			}

			var chargesToRemove = Charges.OfType<BaseGroupInvoiceCharge>().Where(c => c.J7_Amount == 0 && !c.MessageChargeKey.In(requiredCharges)).ToList();
			foreach (var charge in chargesToRemove)
			{
				Charges.RemoveAndDelete(charge);
			}

			foreach (var requiredCharge in requiredCharges)
			{
				if (!Charges.OfType<BaseGroupInvoiceCharge>().Any(c => c.MessageChargeKey.Equals(requiredCharge)))
				{
					var newCharge = Charges.AddNew();
					newCharge.J7_ChargeType = requiredCharge.ChargeCode;
					newCharge.J7_IsDutiable = requiredCharge.IsDutiable;
					newCharge.J7_IsStatisticalValueApplicable = requiredCharge.IsStatisticalValueApplicable;
					newCharge.J7_IsGSTApplicable = requiredCharge.IsVATible;
					newCharge.J7_IsIncludedInITOT = requiredCharge.IsIncludedInITOT;
				}
			}
		}

		protected override string GetStandaloneIncoTermAndChargeFactoryCountryContext() => base.GetStandaloneIncoTermAndChargeFactoryCountryContext() + this.GetIncoTermChargeFactoryCacheKey();

		SupplierBuyerLinkGroupChargesPopulator chargesPopulator;
		internal SupplierBuyerLinkGroupChargesPopulator ChargesPopulator => chargesPopulator ?? (chargesPopulator = new SupplierBuyerLinkGroupChargesPopulator(this));

		public void PopulateCharges() => ChargesPopulator.PopulateCharges();
	}
}
