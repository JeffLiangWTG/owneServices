using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class TIRSendMessageWrapper : NctsBaseDepartureMessageWrapper, ITIRMessageDataProvider
	{
		public TIRSendMessageWrapper(NctsHeader header, ICertificateProvider certificateData)
			: base(header, certificateData)
		{
		}

		public ZString CountryOfOrigin => nctsHeader.BH_RL_NKImportLoadPort;

		public INctsCustomsTransitOfficeProvider CustomsOfficeOfTransit => customsOfficeOfTransit ?? (customsOfficeOfTransit = new NctsCustomsTransitOfficeWrapper(nctsHeader.DestinationCustomsOfficeCodeForDeparture));
		NctsCustomsTransitOfficeWrapper customsOfficeOfTransit;

		public ZString TIRCarnetNumber => movementHeader.TirCarnetNumber;

		public ZDateTime TIRCarnetExpiryDate => movementHeader.TirCarnetExpiryDate;

		public ITransportMediumInfoCommon LoadingTransport => loadingTransport ?? (loadingTransport = new TransportMediumInfoCommonWrapper(ZString.Empty, movementHeader.BM_TransportAtDeparture, movementHeader.BM_RN_NKTransportAtDepartureCountry));
		TransportMediumInfoCommonWrapper loadingTransport;

		public IPartyProvider Holder => CachedValueHelper.GetValue(ref holder, () => PartyWrapper.New(nctsHeader.Principal));
		CachedValue<IPartyProvider> holder;

		public IReadOnlyCollection<ITIRLine> Lines
		{
			get
			{
				if (lines == null)
				{
					var goodsItems = nctsHeader.IsPhase5 ? nctsHeader.Bills.SelectMany(x => x.GoodsItems) : movementHeader.GoodsItems;
					lines = goodsItems
						.Cast<NctsDepartureCargoDesc>()
						.Select(goodsItem => new TIRLineWrapper(goodsItem))
						.ToList().AsReadOnly();
				}
				return lines;
			}
		}
		IReadOnlyCollection<TIRLineWrapper> lines;
	}
}
