using System.Linq;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business
{
	public abstract class SingleDecHeaderProvider : ImportDecHeaderProvider, ISingleDecHeader
	{
		public SingleDecHeaderProvider(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		public bool DeclarantIsConsigneeFlag => Declaration.Declarant.Header.PK == Declaration.ImporterDocumentaryAddress.OrganisationPK;

		public string DeliveryTermsPlace => RandomInvoiceHeader?.JZ_IncoTermPlace;

		public string DeliveryTermsKey => RandomInvoiceHeader?.ZG_AgreedPlaceCode;

		public IMoney PaymentTransaction => CachedValueHelper.GetValue(ref paymentTransaction, () =>
		{
			IMoney result = null;
			var valuationCode = RandomInvoiceHeader?.JZ_ValuationCode ?? ZString.Empty;
			var foc = valuationCode != "23" && valuationCode != "24";
			if (foc)
			{
				result = new MoneyProviderCalculated(RandomInvoiceHeader?.JZ_RX_NKInvoice_Currency ?? ZString.Empty,
					EntryInstruction.InvoiceLines.Sum(l => l.JI_LinePrice));
			}
			return result;
		});
		CachedValue<IMoney> paymentTransaction;

		public string ForeignTradeStatisticsGoodsStatus => Declaration.JE_StatisticStatus;

		public string ForeignTradeStatisticsDestinationCountry => Declaration.JE_GoodsDestination;

		public string ForeignTradeStatisticsDestinationFederalState => CachedValueHelper.GetValue(ref foreignTradeStatisticsDestinationFederalState, () => ImportMappingHelper.GetDestinationFederalState(Declaration, ForeignTradeStatisticsDestinationCountry));
		CachedValue<string> foreignTradeStatisticsDestinationFederalState;

		public string EntryCustomsOfficeReferenceNumber => Declaration.CustomsOffices.Cast<DEOfficeCode>().FirstOrDefault(co => co.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent)?.CY_Data;
	}
}
