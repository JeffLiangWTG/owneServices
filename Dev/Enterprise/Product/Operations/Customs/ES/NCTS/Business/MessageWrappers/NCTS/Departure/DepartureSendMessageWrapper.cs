using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class DepartureSendMessageWrapper : NctsBaseDepartureMessageWrapper, IDepartureMessageDataProvider
	{
		public DepartureSendMessageWrapper(NctsHeader header, ICertificateProvider certificateData)
			: base(header, certificateData)
		{
		}

		public ZString CustomsProcedureCategory3 => movementHeader.BM_InBondEntryType;

		public ZString CountryOfDeparture => nctsHeader.BH_RL_NKImportLoadPort;

		public IReadOnlyCollection<INctsCustomsTransitOfficeProvider> CustomsOfficesOfTransit => customsOfficesOfTransit ?? (customsOfficesOfTransit = nctsHeader.TransitCustomsOfficeCodeList.Select(x => new NctsCustomsTransitOfficeWrapper(x.OfficeCode)).ToList().AsReadOnly());
		IReadOnlyCollection<INctsCustomsTransitOfficeProvider> customsOfficesOfTransit;

		public INctsCustomsTransitOfficeProvider CustomsOfficeOfDestination => customsOfficeOfTransit ?? (customsOfficeOfTransit = new NctsCustomsTransitOfficeWrapper(nctsHeader.DestinationCustomsOfficeCodeForDeparture));
		NctsCustomsTransitOfficeWrapper customsOfficeOfTransit;

		public IReadOnlyCollection<IGuaranteeNumber> GuaranteeNumbers => guaranteeNumbers ?? (guaranteeNumbers = nctsHeader.GetEffectiveGuarantees().Cast<NctsGuarantee>().Select(x => new DepartureGuaranteeNumberWrapper(x.PW_BondType + x.PW_BondNumber, x.PW_Password)).ToList().AsReadOnly());
		IReadOnlyCollection<IGuaranteeNumber> guaranteeNumbers;

		public ITransportMediumInfoCommon TransitTransportMedium => transitTransport ?? (transitTransport = new TransportMediumInfoCommonWrapper(ZString.Empty, movementHeader.BM_TransportAtDeparture, movementHeader.BM_RN_NKTransportAtDepartureCountry));
		TransportMediumInfoCommonWrapper transitTransport;

		public IPartyProvider Principal => CachedValueHelper.GetValue(ref principal, () => PartyWrapper.New(nctsHeader.Principal));
		CachedValue<IPartyProvider> principal;

		public IPartyProvider Representative => representative ?? (representative = DepartureRepresentativeWrapper.New(nctsHeader));
		IPartyProvider representative;

		public IReadOnlyCollection<IDepartureLine> Lines
		{
			get
			{
				if (lines == null)
				{
					lines = movementHeader.GoodsItems
						.Cast<NctsDepartureCargoDesc>()
						.Select(goodsItem => new DepartureLineWrapper(goodsItem))
						.ToList().AsReadOnly();
				}
				return lines;
			}
		}
		IReadOnlyCollection<DepartureLineWrapper> lines;

		public ZString NationalSimplificationIndicator => nctsHeader.NationalSimplificatorInd;

		protected override ZString DeclarantIdForUNBSegmentCore
		{
			get
			{
				var representativeId = base.DeclarantIdForUNBSegmentCore;
				var principalId = OrgHeaderExtension.GetIDCode(nctsHeader.Principal?.Address?.Header);
				return representativeId != principalId ? !representativeId.IsEmpty ? representativeId : principalId : principalId;
			}
		}
	}
}
