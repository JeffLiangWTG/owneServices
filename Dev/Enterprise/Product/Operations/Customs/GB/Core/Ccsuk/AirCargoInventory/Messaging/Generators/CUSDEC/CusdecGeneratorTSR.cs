using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Edifact.D00A.Elements;
using Enterprise.Edifact.D00A.Messages.CUSDEC;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.CUSDEC_2_912
{
	class CusdecGeneratorTSR : CusdecGeneratorBase
	{
		public CusdecGeneratorTSR(ICcsukCusAwb awb, ErrorCollector ec, CusUnderbond tsrUnderbond)
			: base(awb, ec, tsrUnderbond)
		{
			this.tsrUnderbond = (TranshipmentRemoval)tsrUnderbond;
		}

		protected override string AssociationAssignedCode
		{
			get { return "109604"; }
		}

		protected override string BgmDocumentName
		{
			get { return "TSR"; }
		}

		protected override void MakeGroup6()
		{
			if (!tsrUnderbond.ValueOfGoods.IsEmpty && !tsrUnderbond.CurrencyCode.IsEmpty)
			{
				var grp6 = result.Group6.InstantiateAChildAndAddItToChildrenCollection();
				var moa = grp6.MOA.InstantiateAChildAndAddItToChildrenCollection();
				moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier = MonetaryAmountTypeCodeQualifierList.GetFromString("40");
				moa.MonetaryAmount.MonetaryAmount = tsrUnderbond.ValueOfGoods.ToStringTrimZeros(2);
				moa.MonetaryAmount.CurrencyIdentificationCode = tsrUnderbond.CurrencyCode;
				moa.MonetaryAmount.CurrencyTypeCodeQualifier = CurrencyTypeCodeQualifierList.GetFromString("1");
				moa.MonetaryFunctionQualifier = "14";
			}
		}

		protected override LocationAndRelationsAsASingleElement MakeLOC28DestinationCountry()
		{
			LocationAndRelationsAsASingleElement location = null;
			if (underbond.AirportOrCountryOfDestination.IsEmpty)
			{
				errorCollector.AddError("Destination country/region needed if destination airport is missing", new ErrorInfo("", "mandatory"));
			}
			else
			{
				if (tsrUnderbond.AirportOrCountryOfDestination.Length == 2)
				{
					location = new LocationAndRelationsAsASingleElement();
					location.PlaceLocationQualifier3227 = LocationTypes.Codes.COD;  //28
					location.PlaceLocationIdentification3225 = tsrUnderbond.CountryOfDestination;
				}
			}
			return location;
		}

		protected override LocationAndRelationsAsASingleElement MakeLOC5PortOfShipment()
		{
			if (tsrUnderbond.PortOfShipment.IsEmpty)
			{
				errorCollector.AddError("Port of shipment", new ErrorInfo("", "mandatory"));
			}
			else
			{
				var location5 = new LocationAndRelationsAsASingleElement();
				location5.PlaceLocationQualifier3227 = LocationTypes.Codes.POS;  //5
				location5.PlaceLocationIdentification3225 = tsrUnderbond.PortOfShipment.Right(3);
				location5.CodeListQualifier1131_1 = "139";
				return location5;
			}
			return null;
		}

		protected override void MakeGroup1ReferenceOnwardAwb(SegmentGroup1 grp1)
		{
			if (!tsrUnderbond.OnwardAirWaybillNumber.IsEmpty)
			{
				var rffOnwardAwb = grp1.RFF.InstantiateAChildAndAddItToChildrenCollection();
				rffOnwardAwb.Reference.ReferenceFunctionCodeQualifier = ReferenceFunctionCodeQualifierList.GetFromString(FsaDocumentTypes.Codes.AirWaybill);
				rffOnwardAwb.Reference.ReferenceIdentifier = tsrUnderbond.OnwardAirWaybillNumber.KeepAlphanumericCharacters();
			}
		}

		protected override void MakesGISforLicenceRestricted()
		{
			MakesGISforLicenceRestrictedCore(tsrUnderbond.LicenseRestrictionInd);
		}

		protected override void MakeTDT()
		{
			var tdt = result.TDT.InstantiateAChildAndAddItToChildrenCollection();
			tdt.TransportStageQualifier = TransportStageCodeQualifierList.GetFromString("12");
			tdt.XModeOfTransport = tsrUnderbond.OnwardMode;
			MakeTDTOnwardCarrier(tdt);
		}

		void MakeTDTOnwardCarrier(TDTSegmentWithDatetime tdt)
		{
			CusdecGeneratorIAR.MakeTDTOnwardCarrier(tdt, tsrUnderbond);
		}

		readonly TranshipmentRemoval tsrUnderbond;
	}
}
