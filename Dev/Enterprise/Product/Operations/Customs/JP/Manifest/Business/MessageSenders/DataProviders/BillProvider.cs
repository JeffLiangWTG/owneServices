using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Customs.JP.MessageDefinitions.Outbound;
using Enterprise.Customs.JP.Common;
using static CargoWise.Customs.JP.MessageDefinitions.Outbound.ICHA;

namespace Enterprise.Customs.JP.Manifest.Business
{
	public class BillProvider : IHCH01Bills, IHDF01Bills, INVC01Bills, ICHABills
	{
		public BillProvider(ManifestMessageSendingObject messageSendingObject)
		{
			this.messageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
			this.bill = Argument.NotNull(messageSendingObject.Bill, nameof(bill));
			temporaryLandingInfo = bill.TemporaryLandingInfo;
		}

		readonly AsycudaBill bill;
		readonly ManifestMessageSendingObject messageSendingObject;
		readonly TemporaryLandingInfo temporaryLandingInfo;

		public string HAWB => bill.ABL_BillNumber;

		public int TotalCount => bill.ABL_ManifestQty;

		public IMeasurement TotalWeight => TryGetMeasurementProvider(bill, nameof(TotalWeight));

		public string GoodsDescription => bill.ABL_GoodsDescription;

		public string SpecialCargoCode => bill.ABL_SpecialCargoCode;

		public string FinalDestination => bill.FinalDestination?.RL_IATA;

		public string GoodsLocation => bill.ABL_GoodsLocation;

		public string PortOfDischarge => bill.ABL_GoodsLocation;

		public IAddress Shipper => TryGetAddressProvider(bill, nameof(Shipper), nameof(IHCH01Bills));

		public IAddress Consignee => TryGetAddressProvider(bill, nameof(Consignee), nameof(IHCH01Bills));

		public string Action => messageSendingObject.Action;

		public decimal Weight => bill.CustomsWeight;

		public string CargoType => bill.ABL_CargoType;

		public string HouseBillNumbers => bill.ABL_BillNumber;

		string INVC01Bills.GoodsDescription => bill.ABL_GoodsDescription;

		ILocation INVC01Bills.FinalDestination => TryGetLocationProvider(bill, nameof(FinalDestination));

		public ILocation PlaceOfDelivery => TryGetLocationProvider(bill, nameof(PlaceOfDelivery));

		IWesternAddress INVC01Bills.Shipper => TryGetAddressProvider(bill, nameof(Shipper), nameof(INVC01Bills));

		IWesternAddress INVC01Bills.Consignee => TryGetAddressProvider(bill, nameof(Consignee), nameof(INVC01Bills));

		public IEnumerable<IWesternAddress> Notifier
		{
			get
			{
				yield return TryGetAddressProvider(bill, nameof(Notifier), nameof(INVC01Bills));
			}
		}

		public string Tariff
		{
			get
			{
				var allDigits = Regex.Replace(bill.ABL_Tariff, @"\D", "");
				return allDigits.Length > 6 ? allDigits.Substring(0, 6) : allDigits;
			}
		}

		public string MarksAndNumbers => bill.ABL_MarksAndNumbers;

		public IMeasurement Quantity => TryGetMeasurementProvider(bill, nameof(Quantity));

		public IMeasurement NetWeight => TryGetMeasurementProvider(bill, nameof(NetWeight));

		public IMeasurement Volume => TryGetMeasurementProvider(bill, nameof(Volume));

		public string GoodsOfOrigin => bill.ABL_CountryOfOrigin;

		public IMoney FreightValue => TryGetMoneyProvider(bill, nameof(FreightValue));

		public IMoney GoodsValue => TryGetMoneyProvider(bill, nameof(GoodsValue));

		public string IsTemporaryLanding => (!temporaryLandingInfo.CSI_Code.IsEmpty && temporaryLandingInfo.CSI_ItemNumber > 0) ? Constants.Message.ManifestNatureCodes.Transhipment28 : string.Empty;

		public string ReasonCodeForTemporaryLanding => temporaryLandingInfo.CSI_Code;

		public short? NumberOfDaysForTemporaryLanding => (short?)temporaryLandingInfo.CSI_ItemNumber;

		public DateTime? StartDateForTemporaryLanding => temporaryLandingInfo.CSI_DateOfIssue.GetDateTime();

		public DateTime? EndDateForTemporaryLanding => temporaryLandingInfo.CSI_DateOfExpiry.GetDateTime();

		public string TransportationCodeDuringTemporaryLanding => temporaryLandingInfo.CSI_ReferenceNumber;

		public string PortOfDischargeName => string.Empty;

		public IEnumerable<string> CodeForVerification => bill.OtherLawsandRegulations.Select(cre => cre.CFR_Reference.ToString());

		public string Notes => bill.ABL_Remarks;

		public string Reason => messageSendingObject.Reason;

		AddressProvider TryGetAddressProvider(AsycudaBill bill, string type, string messageType) => new AddressProvider(bill, type, messageType);

		MeasurementProvider TryGetMeasurementProvider(AsycudaBill bill, string type) => new MeasurementProvider(bill, type);

		LocationProvider TryGetLocationProvider(AsycudaBill bill, string type) => new LocationProvider(bill, type);

		MoneyProvider TryGetMoneyProvider(AsycudaBill bill, string type) => new MoneyProvider(bill, type);
	}
}
