using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class InvoiceHeaderActiveCollection : EU.Business.Declaration.InvoiceHeaderActiveCollection
	{
		public InvoiceHeaderActiveCollection(JobDeclaration declaration) : base(declaration)
		{
		}

		public InvoiceHeaderActiveCollection(JobComInvoiceGroupHeader groupInvoice, bool isDirectRelationship)
			: base(groupInvoice, isDirectRelationship)
		{
		}

		public new JobComInvoiceHeader AddNew()
		{
			return (JobComInvoiceHeader)base.AddNew();
		}

		public new JobComInvoiceHeader this[int index] => (JobComInvoiceHeader)(base[index]);

		public override bool HasInvoicesWithRecommendedChargeInGroupCharges(ICustomsChargeCode chargeCode)
		{
			return (JobDeclaration.IsExport)
			? HasInvoicesWithRecommendedChargeInGroupChargesForExportDeclaration(chargeCode)
			: base.HasInvoicesWithRecommendedChargeInGroupCharges(chargeCode);
		}

		bool HasInvoicesWithRecommendedChargeInGroupChargesForExportDeclaration(ICustomsChargeCode chargeCode)
		{
			if (chargeCode != null)
			{
				foreach (BaseJobComInvoiceHeader invoice in this)
				{
					var incoTermAndCharngeFactory = invoice.IncoTermAndChargeFactory;
					var incoTerm = invoice.IncoTerm;
					if (incoTermAndCharngeFactory.IsThisChargeRecommendedForThisIncoTerm(incoTerm, chargeCode.Code)
						&& incoTermAndCharngeFactory.IsThisChargeRecommendedForThisInvoice(invoice, incoTerm, chargeCode.Code))
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
