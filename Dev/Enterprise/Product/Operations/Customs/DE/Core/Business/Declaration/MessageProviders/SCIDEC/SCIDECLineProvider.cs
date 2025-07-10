using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public sealed class SCIDECLineProvider : SingleDecLineProvider, ISCIDECLine
	{
		public SCIDECLineProvider(CusEntryLine entryLine)
			: base(entryLine)
		{
		}

		public string ArticleNumber => RandomInvoiceLine.JI_PartNo;

		public IAmount InwardMovementAmount => CachedValueHelper.GetValue(ref inwardMovementAmountCached, () => new AmountProvider(InvoiceLines.Sum(l => l.JI_BondedWhsQuantity), RandomInvoiceLine.JI_BondedWhsUnitQty));
		CachedValue<IAmount> inwardMovementAmountCached;

		public string EconomicConditions => RandomInvoiceLine.ZG_EconomicConditions;

		public IReadOnlyCollection<ISCIDECLineProduct> Products => products ?? (products = RandomInvoiceLine.InwardProcessingProducts.Cast<InwardProcessingProduct>().Select(p => new SCIDECLineProductProvider(p)).ToArray());
		IReadOnlyCollection<ISCIDECLineProduct> products;

		public IIdentificationMeans IdentificationMeans => CachedValueHelper.GetValue(ref identificationMeansCached, () => new IdentificationMeanProvider(RandomInvoiceLine.ZG_IdentificationMeansType, RandomInvoiceLine.JI_ExtraInfoForClassification));
		CachedValue<IIdentificationMeans> identificationMeansCached;

		public string RequestedPreferentialTreatment => RandomInvoiceLine.JI_PrimaryPreference;

		protected override bool IsHighValueOvrdValidForAssessmentCustomsValue => true;
	}
}
