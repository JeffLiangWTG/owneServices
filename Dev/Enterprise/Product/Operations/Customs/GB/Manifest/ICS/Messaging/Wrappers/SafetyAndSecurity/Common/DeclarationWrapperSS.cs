using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.Interfaces.SafetyAndSecurity;
using CargoWise.Customs.GB.MessageContracts.SafetyAndSecurity;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.ICS;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.SafetyAndSecurity.Messaging
{
	public abstract class DeclarationWrapperSS
	{
		protected DeclarationWrapperSS(AsycudaManifestHeaderBase manifest, string messageType)
		{
			this.manifest = Argument.NotNull(manifest, nameof(manifest));
			this.messageType = Argument.NotNullOrEmpty(messageType, nameof(messageType));
			utcDateTime = ZDateTime.UtcNow.ToDateTime();
		}

		readonly string messageType;
		protected readonly AsycudaManifestHeaderBase manifest;
		protected readonly ZDateTime utcDateTime;

		protected OrgHeader OrgProxy => manifest.Branch.OrgProxy;

		protected ZString EORI => EuEoriProviderAndValidator.GetRegoCodeOfThisOrg(OrgProxy, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

		protected ZString EORIBranchSuffix => EuEoriProviderAndValidator.GetRegoCodeOfThisOrg(OrgProxy, OrgCusCode.UnitedKingdomCodeTypes.EoriBranchSuffix);
		ZString NormalizeEORIBranchSuffix(ZString suffix) => Regex.Replace(suffix, @"[^\d]", string.Empty).PadLeft(10, '0');

		public string MessageSender => $"{EORI}/{NormalizeEORIBranchSuffix(EORIBranchSuffix)}";

		public string MessageRecipient => string.Empty; // Do not send.

		public string DateOfPreparation => utcDateTime.ToString("yyMMdd");

		public string TimeOfPreparation => utcDateTime.ToString("HHmm");

		public string Priority => string.Empty; // Do not send.

		public string MessageIdentification => IcsSsGreatBritainEDIMessage.MessageNumberPlaceHolderXml;

		public string CorrelationIdentifier => IcsSsGreatBritainEDIMessage.MessageNumberPlaceHolderXml;

		public string MessageType => messageType;

		#region Consignor
		public ITrader Consignor => consignor ??= GetConsignor();

		ITrader GetConsignor()
		{
			var masterBill = manifest.MasterBill;
			return new TraderWrapper(masterBill.ABL_ShipperName,
							masterBill.ABL_ShipperStreet1,
							masterBill.ABL_ShipperPostcode,
							masterBill.ABL_ShipperCity,
							masterBill.ABL_RN_NKShipperCountry,
							ZString.Empty,
							masterBill.ABL_ShipperRegNo);
		}
		ITrader consignor;
		#endregion

		#region Consignee
		public ITrader Consignee => consignee ??= GetConsignee();

		ITrader GetConsignee()
		{
			var masterBill = manifest.MasterBill;
			return new TraderWrapper(masterBill.ABL_ConsigneeName,
							masterBill.ABL_ConsigneeStreet1,
							masterBill.ABL_ConsigneePostcode,
							masterBill.ABL_ConsigneeCity,
							masterBill.ABL_RN_NKConsigneeCountry,
							ZString.Empty,
							masterBill.ABL_ConsigneeRegNo);
		}
		ITrader consignee;
		#endregion

		#region NotifyParty
		public ITrader NotifyParty => notifyParty ??= GetNotifyParty();

		ITrader GetNotifyParty()
		{
			var masterBill = manifest.MasterBill;
			return new TraderWrapper(masterBill.ABL_NotifyPartyName,
							masterBill.ABL_NotifyPartyStreet1,
							masterBill.ABL_NotifyPartyPostcode,
							masterBill.ABL_NotifyPartyCity,
							masterBill.ABL_RN_NKNotifyPartyCountry,
							ZString.Empty,
							masterBill.ABL_NotifyPartyRegNo);
		}
		ITrader notifyParty;
		#endregion

		public ITrader Representative => null; // Do not send.

		#region GoodsItems
		public IEnumerable<IGoodsItem> GoodsItems => goodsItems ??= GetGoodsItems();

		IReadOnlyCollection<IGoodsItem> GetGoodsItems() => manifest.Bills.Select(x => new GoodsItemWrapper(x)).ToList();
		IReadOnlyCollection<IGoodsItem> goodsItems;
		#endregion

		#region Itineraries
		public IEnumerable<IItinerary> Itineraries => itineraries ??= GetItineraries();

		IReadOnlyCollection<IItinerary> GetItineraries() => manifest.Itinerary.Select(x => new ItineraryWrapper(x.CY_Data)).ToList();
		IReadOnlyCollection<IItinerary> itineraries;
		#endregion

		#region LodgementCustomsOffice 
		public CargoWise.Customs.GB.MessageContracts.SafetyAndSecurity.ICustomsOffice LodgementCustomsOffice => lodgementCustomsOffice ??= new CustomsOfficeWrapper(manifest.AMA_CustomsOffice);
		CargoWise.Customs.GB.MessageContracts.SafetyAndSecurity.ICustomsOffice lodgementCustomsOffice;
		#endregion

		#region LodgingSummaryDeclarationPerson
		public ITrader LodgingSummaryDeclarationPerson => lodgingSummaryDeclarationPerson ??= GetLodgingSummaryDeclarationPerson(); //manifest.Branch.pos

		ITrader GetLodgingSummaryDeclarationPerson()
		{
			var person = GlbStaff.CurrentUser;
			return new TraderWrapper(person.GS_FullName,
							person.Address1,
							person.Postcode,
							person.City,
							person.Country?.Code ?? ZString.Empty,
							ZString.Empty,
							EORI,
							sendNameAndAddressOrEori: true);
		}
		ITrader lodgingSummaryDeclarationPerson;
		#endregion

		#region SealsIDs
		public IEnumerable<ISealsID> SealsIds => sealsIds ??= GetSealsIds();

		IReadOnlyCollection<ISealsID> GetSealsIds() =>
			manifest.Containers
				.Select(container => new[] { container.ACN_Seal1, container.ACN_Seal2, container.ACN_Seal3 })
				.SelectMany(seal => seal).Where(seal => !seal.IsEmpty)
				.Select(seal => new SealsIDWrapper(seal))
				.ToList();
		IReadOnlyCollection<ISealsID> sealsIds;
		#endregion

		#region FirstEntry

		public IFirstEntryCustomsOffice FirstEntry => firstEntry ??= GetFirstEntry();

		IFirstEntryCustomsOffice GetFirstEntry() => manifest.FirstEntries.Select(x => new FirstEntryCustomsOfficeWrapper(x.Data, x.Date)).FirstOrDefault();
		IFirstEntryCustomsOffice firstEntry;
		#endregion

		#region SubsequentEntries
		public IEnumerable<CargoWise.Customs.GB.MessageContracts.SafetyAndSecurity.ICustomsOffice> SubsequentEntries => subsequentEntries ??= GetSubsequentEntries();

		IReadOnlyCollection<CargoWise.Customs.GB.MessageContracts.SafetyAndSecurity.ICustomsOffice> GetSubsequentEntries() => manifest.SubsequentEntries.Select(x => new CustomsOfficeWrapper(x.Data)).ToList();
		IReadOnlyCollection<CargoWise.Customs.GB.MessageContracts.SafetyAndSecurity.ICustomsOffice> subsequentEntries;
		#endregion

		#region EntryCarrier
		public ITrader EntryCarrier => entryCarrier ??= GetEntryCarrier();

		ITrader GetEntryCarrier()
		{
			ITrader carrierTrader = null;
			var carrier = manifest?.Carrier;
			if (carrier != null)
			{
				var carrierGbEORI = carrier.GetEORI(Core.Constants.CountryCodes.UnitedKingdom, ignoreCountryOfIssuanceIfNotMatched: true);
				carrierTrader = new TraderWrapper(carrier.CompanyName,
														carrier.Address1,
														carrier.Postcode,
														carrier.City,
														carrier.Country.Code,
														ZString.Empty,
														carrierGbEORI,
														sendNameAndAddressOrEori: carrierGbEORI.StartsWith(Core.Constants.CountryCodes.UnitedKingdom));
			}
			return carrierTrader;
		}
		ITrader entryCarrier;
		#endregion
	}
}
