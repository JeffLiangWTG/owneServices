using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.ICS.Messaging.IE315
{
	public class DeclarationWrapper : IDeclaration
	{
		public DeclarationWrapper(AsycudaManifestHeaderBase manifest)
		{
			this.manifest = manifest;
			utcDateTime = ZDateTime.UtcNow.ToDateTime();
		}

		readonly AsycudaManifestHeaderBase manifest;

		public static class Constants
		{
			public const string ICS = "ICS";
			public const string MessageType = "CC315A";
			public const string BOL = "BOL";
		}

		OrgHeader OrgProxy => manifest.Branch.OrgProxy;

		ZString EORI => EuEoriProviderAndValidator.GetRegoCodeOfThisOrg(OrgProxy, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

		ZString EORIBranchSuffix => EuEoriProviderAndValidator.GetRegoCodeOfThisOrg(OrgProxy, OrgCusCode.UnitedKingdomCodeTypes.EoriBranchSuffix);

		ZString TCU => EuEoriProviderAndValidator.GetRegoCodeOfThisOrg(OrgProxy, OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU);

		ZString ConfigCode => EORI.IsEmpty ? TCU : EORI;

		ZString IDeclaration.MessageSender => string.Join("/", EORI, EORIBranchSuffix.IsEmpty ? "00000000" : EORIBranchSuffix.ToString());

		ZString IDeclaration.MessageRecipient => Constants.ICS;

		ZString IDeclaration.DateOfPreparation => utcDateTime.ToString("yyMMdd");

		ZString IDeclaration.TimeOfPreparation => utcDateTime.ToString("HHmm");

		ZString IDeclaration.Priority => ZString.Empty; // Do not send.

		ZBool IDeclaration.TestIndicator => Globals.IsTest;

		ZString IDeclaration.MessageIdentification => OrgProxy.GetEuIdentificationNumber();

		ZString IDeclaration.MessageType => Constants.MessageType;

		ZString IDeclaration.CorrelationIdentifier => ZString.Empty; // Do not send.

		IHeader IDeclaration.Header => header ?? (header = new HeaderWrapper(manifest, utcDateTime));
		IHeader header;

		ITrader IDeclaration.Consignor => consignor ?? (consignor = GetConsignor());

		ITrader GetConsignor()
		{
			var masterBill = manifest.MasterBill;
			return new TraderWrapper(masterBill.ABL_ShipperName, masterBill.ABL_ShipperStreet1,
				masterBill.ABL_ShipperPostcode, masterBill.ABL_ShipperCity, masterBill.ABL_RN_NKShipperCountry, ZString.Empty, ConfigCode);
		}

		ITrader consignor;

		ITrader IDeclaration.Consignee => consignee ?? (consignee = GetConsignee());

		ITrader GetConsignee()
		{
			var masterBill = manifest.MasterBill;
			return new TraderWrapper(masterBill.ABL_ConsigneeName, masterBill.ABL_ConsigneeStreet1,
				masterBill.ABL_ConsigneePostcode, masterBill.ABL_ConsigneeCity, masterBill.ABL_RN_NKConsigneeCountry, ZString.Empty, ConfigCode);
		}

		ITrader consignee;

		ITrader IDeclaration.NotifyParty => notifyParty ?? (notifyParty = GetNotifyParty());

		ITrader GetNotifyParty()
		{
			var masterBill = manifest.MasterBill;
			return new TraderWrapper(masterBill.ABL_NotifyPartyName, masterBill.ABL_NotifyPartyStreet1,
				masterBill.ABL_NotifyPartyPostcode, masterBill.ABL_NotifyPartyCity, masterBill.ABL_RN_NKNotifyPartyCountry, ZString.Empty, ConfigCode);
		}

		ITrader notifyParty;

		IEnumerable<IGoodsItem> IDeclaration.GoodsItems => goodsItems ?? (goodsItems = GetGoodsItems());

		IEnumerable<IGoodsItem> GetGoodsItems()
		{
			var result = new List<IGoodsItem>();

			foreach (AsycudaBill bill in manifest.Bills)
			{
				result.Add(new GoodsItemWrapper(bill, ConfigCode));
			}

			return result;
		}

		IEnumerable<IGoodsItem> goodsItems;

		IEnumerable<IItinerary> IDeclaration.Itineraries => itineraries ?? (itineraries = GetItineraries());

		IEnumerable<IItinerary> GetItineraries()
		{
			var result = new List<IItinerary>();
			foreach (RouteEntry routeEntry in manifest.Itinerary)
			{
				result.Add(new ItineraryWrapper(routeEntry.CY_Data));
			}
			return result;
		}
		IEnumerable<IItinerary> itineraries;

		ICustomsOffice IDeclaration.LodgementCustomsOffice => lodgementCustomsOffice ?? (lodgementCustomsOffice = new CustomsOfficeWrapper(manifest.AMA_CustomsOffice));
		ICustomsOffice lodgementCustomsOffice;

		ITrader IDeclaration.Representative => null; // Do not send.

		ITrader IDeclaration.LodgingSummaryDeclarationPerson => lodgingSummaryDeclarationPerson ?? (lodgingSummaryDeclarationPerson = GetLodgingSummaryDeclarationPerson()); //manifest.Branch.pos

		ITrader GetLodgingSummaryDeclarationPerson()
		{
			var person = GlbStaff.CurrentUser;
			return new TraderWrapper(person.GS_FullName, person.Address1,
				person.Postcode, person.City, person.Country?.Code ?? ZString.Empty, ZString.Empty, EORI);
		}

		ITrader lodgingSummaryDeclarationPerson;

		IEnumerable<ISealsID> IDeclaration.SealsIds => sealsIds ?? (sealsIds = GetSealsIds());

		IEnumerable<ISealsID> GetSealsIds()
		{
			var result = new List<ISealsID>();
			foreach (AsycudaContainer container in manifest.Containers)
			{
				foreach (var seal in new[] { container.ACN_Seal1, container.ACN_Seal2, container.ACN_Seal3 })
				{
					if (!seal.IsEmpty)
					{
						result.Add(new SealsIDWrapper(seal));
					}
				}
			}
			return result;
		}

		IEnumerable<ISealsID> sealsIds;

		IFirstEntryCustomsOffice IDeclaration.FirstEntry => firstEntry ?? (firstEntry = GetFirstEntry());

		IFirstEntryCustomsOffice GetFirstEntry()
		{
			var cusCodeData = manifest.FirstEntries.FirstOrDefault();
			return cusCodeData == null ? null : new FirstEntryCustomsOfficeWrapper(cusCodeData.Data, cusCodeData.Date);
		}

		IFirstEntryCustomsOffice firstEntry;

		IEnumerable<ICustomsOffice> IDeclaration.SubsequentEntries => subsequentEntries ?? (subsequentEntries = GetSubsequentEntries());

		IEnumerable<ICustomsOffice> GetSubsequentEntries() =>
			manifest.SubsequentEntries.Select(x => new CustomsOfficeWrapper(x.Data)).ToList();

		IEnumerable<ICustomsOffice> subsequentEntries;

		ITrader IDeclaration.EntryCarrier => entryCarrier ?? (entryCarrier = GetEntryCarrier());

		ITrader GetEntryCarrier()
		{
			var carrier = manifest.Carrier;
			return carrier == null ? null : new TraderWrapper(carrier.CompanyName, carrier.Street, carrier.Postcode, carrier.City, carrier.Country.Code, ZString.Empty, ConfigCode);
		}

		ITrader entryCarrier;
		readonly ZDateTime utcDateTime;
	}
}

