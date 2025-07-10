using System.Linq;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class JobComInvoiceLineLookups : EU.Business.Declaration.JobComInvoiceLineLookups
	{
		public JobComInvoiceLineLookups(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList StateIslandCodesList => CustomsFiscalTerritoriesList.GetSpainFullList(Parent.Factory);

		public override CodeDescriptionPairList TaxOrFeeCodeList
		{
			get
			{
				var returnList = new CodeDescriptionPairList();

				var vatCodePrefix = InvoiceLine.DestinationStateIsCanaryIsland ? UniversalReferenceConstants.RefCusTaxOrFee.CanaryIslandVATPrefix : UniversalReferenceConstants.RefCusTaxOrFee.VatPrefix;
				foreach (var vatCodeDescriptionPair in base.TaxOrFeeCodeList.OfType<CodeDescriptionPair>().Where(x => x.Code.StartsWith(vatCodePrefix)))
				{
					returnList.Add(vatCodeDescriptionPair);
				}

				returnList.AddPair("EX", Res.GetString("D33CBE1D-80CF-4BA4-A6E0-D6353A0A69A6", "Exemption"));

				return returnList;
			}
		}

		public new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

		protected new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;
	}
}
