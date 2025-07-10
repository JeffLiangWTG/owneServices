using CargoWise.Types;
using Enterprise.Client.JAS.Business.Utilities;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public class OHBLLine : MessageLine
	{
		public OHBLLine(HouseLevelRecordType houseBillOfLadingType, IJXCExportHeader headerData, ExportHouseBillOfLading houseBillOfLading)
		{
			this.HeaderData = headerData;
			this.HouseBillOfLading = houseBillOfLading;
			this.HouseBillOfLadingType = houseBillOfLadingType;
		}

		#region Overrides

		protected override int FieldCount
		{
			get { return JXCConstants.OHBLFieldCount; }
		}

		protected override ZString LineType
		{
			get
			{
				ZString result;

				switch (HouseBillOfLadingType)
				{
					case HouseLevelRecordType.CoLoad:
						result = JXCConstants.LineTypes.COHB;
						break;

					case HouseLevelRecordType.PreShipment:
						result = JXCConstants.LineTypes.PSBL;
						break;

					default:
						result = JXCConstants.LineTypes.OHBL;
						break;
				}

				return result;
			}
		}

		#endregion

		#region Set Field Values

		protected override void SetFieldValues(JXCFlatFileDataRow dataRow)
		{
			SetHeaderFields(dataRow);
			SetPortFields(dataRow);
			SetCarrierFields(dataRow);
			SetShipperFields(dataRow);
			SetConsigneeFields(dataRow);
			SetDeliveryAgentFields(dataRow);
			SetNotifyPartyFields(dataRow);
			SetFreightChargesFields(dataRow);
			SetVesselAndVoyageFields(dataRow);
			SetSpecialInstructionsFields(dataRow);
			SetFreightInfoFields(dataRow);
			SetAsAgentForJASOceanServicesIncField(dataRow);
			SetDateAndPlaceOfIssue(dataRow);
			SetPlaceOfReceiptAndDeliveryFields(dataRow);
			SetReceivingAgentFields(dataRow);
			SetHandlingInstructionsFields(dataRow);
			SetMarksAndNumbersField(dataRow);
			SetPayableByFields(dataRow);
			SetNoOfOriginalBillOfLadingsField(dataRow);
			SetExcludeCommercialInvoiceField(dataRow);
		}

		#region Header

		void SetHeaderFields(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(JXCConstants.OHBLFieldPositions.TypeOfRecord, "N");
			dataRow.SetField(JXCConstants.OHBLFieldPositions.OriginTrafficFileNo, Shipment.JS_UniqueConsignRef, JXCConstants.OHBLFieldBoundaries.OriginTrafficFileNoMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.OriginOfficeCode, (HeaderData.SendingForwarder != null) ? HeaderData.SendingForwarder.OfficeCode : ZString.Empty);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.HouseBillOfLadingNo, HouseBillOfLading.OceanHouseBillOfLadingNo);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.BillOfLadingTypeCode, "30");
			dataRow.SetField(JXCConstants.OHBLFieldPositions.BillOfLadingNo, HouseBillOfLading.OceanBillOfLadingNo);
		}

		#endregion

		#region Ports

		void SetPortFields(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(JXCConstants.OHBLFieldPositions.PortOfLoadingCode, HouseBillOfLading.PortOfLoadingCode);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.PortOfLoadingName, HouseBillOfLading.PortOfLoadingName, JXCConstants.OHBLFieldBoundaries.PlaceNameMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.PortOfDischargeCode, HouseBillOfLading.PortOfDischargeCode);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.PortOfDischargeName, HouseBillOfLading.PortOfDischargeName, JXCConstants.OHBLFieldBoundaries.PlaceNameMaxLength);

			// TODO: do not hardcode this once we have the data structure ready
			dataRow.SetField(JXCConstants.OHBLFieldPositions.LoadingDateTimeZone, "AEST");
		}

		#endregion

		#region Carrier

		void SetCarrierFields(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetOrganisationNameField(JXCConstants.OHBLFieldPositions.CarrierName, HouseBillOfLading.ShippingLineName, JXCConstants.OHBLFieldBoundaries.CarrierNameMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.CarrierSSLCode, HouseBillOfLading.ShippingLineSSLCode);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.CarrierAddress1, HouseBillOfLading.ShippingLineAddress1, JXCConstants.OHBLFieldBoundaries.CarrierAddressMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.CarrierAddress2, HouseBillOfLading.ShippingLineAddress2, JXCConstants.OHBLFieldBoundaries.CarrierAddressMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.CarrierCity, HouseBillOfLading.ShippingLineCity, JXCConstants.OHBLFieldBoundaries.CarrierCityMaxLength);
		}

		#endregion

		#region Shipper

		void SetShipperFields(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetOrganisationNameField(JXCConstants.OHBLFieldPositions.ShipperName, HouseBillOfLading.ShipperName, JXCConstants.OHBLFieldBoundaries.ShipperNameMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.ShipperAddress1, HouseBillOfLading.ShipperAddress1, JXCConstants.OHBLFieldBoundaries.ShipperAddressMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.ShipperAddress2, HouseBillOfLading.ShipperAddress2, JXCConstants.OHBLFieldBoundaries.ShipperAddressMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.ShipperCity, HouseBillOfLading.ShipperCity, JXCConstants.OHBLFieldBoundaries.ShipperCityMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.ShipperPostCode, HouseBillOfLading.ShipperPostalCode, JXCConstants.OHBLFieldBoundaries.ShipperPostalCodeMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.ShipperState, HouseBillOfLading.ShipperState, JXCConstants.OHBLFieldBoundaries.ShipperStateMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.ShipperCountryCode, HouseBillOfLading.ShipperCountryCode);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.ShipperEmail, HouseBillOfLading.ShipperEmail);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.ShipperPhone, HouseBillOfLading.ShipperPhone, JXCConstants.OHBLFieldBoundaries.ShipperPhoneMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.ShipperAccountNo, HouseBillOfLading.ShipperAccount, JXCConstants.OHBLFieldBoundaries.ShipperAccountMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.UpdateShipper, "N");
		}

		#endregion

		#region Consignee

		void SetConsigneeFields(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetOrganisationNameField(JXCConstants.OHBLFieldPositions.ConsigneeName, HouseBillOfLading.ConsigneeName, JXCConstants.OHBLFieldBoundaries.ConsigneeNameMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.ConsigneeAddress1, HouseBillOfLading.ConsigneeAddress1, JXCConstants.OHBLFieldBoundaries.ConsigneeAddressMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.ConsigneeAddress2, HouseBillOfLading.ConsigneeAddress2, JXCConstants.OHBLFieldBoundaries.ConsigneeAddressMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.ConsigneeCity, HouseBillOfLading.ConsigneeCity, JXCConstants.OHBLFieldBoundaries.ConsigneeCityMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.ConsigneePostCode, HouseBillOfLading.ConsigneePostalCode, JXCConstants.OHBLFieldBoundaries.ConsigneePostalCodeMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.ConsigneeState, HouseBillOfLading.ConsigneeState, JXCConstants.OHBLFieldBoundaries.ConsigneeStateMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.ConsigneeCountryCode, HouseBillOfLading.ConsigneeCountryCode);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.ConsigneeEmail, HouseBillOfLading.ConsigneeEmail);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.ConsigneePhone, HouseBillOfLading.ConsigneePhone, JXCConstants.OHBLFieldBoundaries.ConsigneePhoneMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.ConsigneeAccountNo, HouseBillOfLading.ConsigneeAccount, JXCConstants.OHBLFieldBoundaries.ConsigneeAccountMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.UpdateConsignee, "N");
		}

		#endregion

		#region Delivery Agent

		void SetDeliveryAgentFields(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetOrganisationNameField(JXCConstants.OHBLFieldPositions.DeliveryAgentName, HouseBillOfLading.DeliveryAgentName, JXCConstants.OHBLFieldBoundaries.DeliveryAgentNameMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.DeliveryAgentAddress1, HouseBillOfLading.DeliveryAgentAddress1, JXCConstants.OHBLFieldBoundaries.DeliveryAgentAddressMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.DeliveryAgentAddress2, HouseBillOfLading.DeliveryAgentAddress2, JXCConstants.OHBLFieldBoundaries.DeliveryAgentAddressMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.DeliveryAgentCity, HouseBillOfLading.DeliveryAgentCity, JXCConstants.OHBLFieldBoundaries.DeliveryAgentCityMaxLength);
		}

		#endregion

		#region Notify Party

		void SetNotifyPartyFields(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetOrganisationNameField(JXCConstants.OHBLFieldPositions.NotifyPartyName, HouseBillOfLading.NotifyPartyName, JXCConstants.OHBLFieldBoundaries.NotifyPartyNameMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.NotifyPartyAddress1, HouseBillOfLading.NotifyPartyAddress1, JXCConstants.OHBLFieldBoundaries.NotifyPartyAddressMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.NotifyPartyAddress2, HouseBillOfLading.NotifyPartyAddress2, JXCConstants.OHBLFieldBoundaries.NotifyPartyAddressMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.NotifyPartyCity, HouseBillOfLading.NotifyPartyCity, JXCConstants.OHBLFieldBoundaries.NotifyPartyCityMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.NotifyPartyPhone, HouseBillOfLading.NotifyPartyPhone, JXCConstants.OHBLFieldBoundaries.NotifyPartyPhoneMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.NotifyPartyEmail, HouseBillOfLading.NotifyPartyEmail);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.UpdateNotifyParty, "N");
			dataRow.SetField(JXCConstants.OHBLFieldPositions.UpdateNotifyParty2, "N");
		}

		#endregion

		#region Freight Charges

		void SetFreightChargesFields(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(JXCConstants.OHBLFieldPositions.Currency, HouseBillOfLading.Currency);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.TotalFreightAmount, HouseBillOfLading.TotalFreightAmount);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.PrepaidOrCollect, HouseBillOfLading.PaymentTerm.Left(1));
		}

		#endregion

		#region Vessel

		void SetVesselAndVoyageFields(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(JXCConstants.OHBLFieldPositions.VesselName, HouseBillOfLading.VesselName, JXCConstants.OHBLFieldBoundaries.VesselNameMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.VesselFlagCountry, HouseBillOfLading.VesselCountryCode);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.LloydsCode, HouseBillOfLading.VesselLloydsCode);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.VoyageSSELNumber, HouseBillOfLading.VoyageNumber, JXCConstants.OHBLFieldBoundaries.VoyageSSELNumberMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.OnBoardDate, HouseBillOfLading.OnBoardDate);
		}

		#endregion

		#region Special Instructions

		void SetSpecialInstructionsFields(JXCFlatFileDataRow dataRow)
		{
			ZString instructions1 = HouseBillOfLading.SpecialInstructions.SubstringSafe(0, 60);
			ZString instructions2 = HouseBillOfLading.SpecialInstructions.SubstringSafe(60, 60);
			ZString instructions3 = HouseBillOfLading.SpecialInstructions.SubstringSafe(120, 60);

			dataRow.SetField(JXCConstants.OHBLFieldPositions.SpecialInstructions1, instructions1);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.SpecialInstructions2, instructions2);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.SpecialInstructions3, instructions3);
		}

		#endregion

		#region Freight Info

		void SetFreightInfoFields(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(JXCConstants.OHBLFieldPositions.TotalNoOfPackages, HouseBillOfLading.TotalNoOfPackages);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.PieceTypeCode, PieceTypeCode);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.GrossWeight, HouseBillOfLading.GrossWeightInKilograms);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.KilosOrPounds, "K");
			dataRow.SetField(JXCConstants.OHBLFieldPositions.MeasurementInCBM, HouseBillOfLading.MeasurementInCubicMetres);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.Rate, HouseBillOfLading.FreightRate);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.DescriptionOfPackagesAndGoods1, HouseBillOfLading.GoodsDescription.SubstringSafe(0, 160));
			dataRow.SetField(JXCConstants.OHBLFieldPositions.DescriptionOfPackagesAndGoods2, HouseBillOfLading.GoodsDescription.SubstringSafe(160, 160));
			dataRow.SetField(JXCConstants.OHBLFieldPositions.DescriptionOfPackagesAndGoods3, HouseBillOfLading.GoodsDescription.SubstringSafe(320, 160));
			dataRow.SetField(JXCConstants.OHBLFieldPositions.DescriptionOfPackagesAndGoods4, HouseBillOfLading.GoodsDescription.SubstringSafe(480, 160));
			dataRow.SetField(JXCConstants.OHBLFieldPositions.DescriptionOfPackagesAndGoods5, HouseBillOfLading.GoodsDescription.SubstringSafe(640, 160));
			dataRow.SetField(JXCConstants.OHBLFieldPositions.DescriptionOfPackagesAndGoods6, HouseBillOfLading.GoodsDescription.SubstringSafe(800, 160));
			dataRow.SetField(JXCConstants.OHBLFieldPositions.DescriptionOfPackagesAndGoods7, HouseBillOfLading.GoodsDescription.SubstringSafe(960, 160));
			dataRow.SetField(JXCConstants.OHBLFieldPositions.DescriptionOfPackagesAndGoods8, HouseBillOfLading.GoodsDescription.SubstringSafe(1120, 160));
			dataRow.SetField(JXCConstants.OHBLFieldPositions.HarmonizedCommodityCode, "ZZ");
			dataRow.SetField(JXCConstants.OHBLFieldPositions.TotalNoOfPackagesAndUnitsInWords, HouseBillOfLading.TotalNoOfPackagesAndUnitInWords, JXCConstants.OHBLFieldBoundaries.TotalNoOfPackagesAndUnitInWordsMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.NoOfContainers, HouseBillOfLading.NoOfContainers);
		}

		ZString PieceTypeCode
		{
			get { return new JASUnitConverter().GetJASPackageUnitFromShipmentOuterPacksType(HouseBillOfLading.PackageType); }
		}

		#endregion

		#region AsAgentForJASOceanServicesInc

		void SetAsAgentForJASOceanServicesIncField(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(JXCConstants.OHBLFieldPositions.AsAgentForJASOceanServicesInc, "JAS OCEAN SERVICES INC. AS CARRIER");
		}

		#endregion

		#region Date and Place of Issue

		void SetDateAndPlaceOfIssue(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(JXCConstants.OHBLFieldPositions.DateOfIssue, HouseBillOfLading.DateOfIssue);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.PlaceOfIssue, HouseBillOfLading.PlaceOfIssue, JXCConstants.OHBLFieldBoundaries.PlaceNameMaxLength);
		}

		#endregion

		#region Receiving Agent

		void SetReceivingAgentFields(JXCFlatFileDataRow dataRow)
		{
			if (ReceivingAgent != null)
			{
				dataRow.SetOrganisationNameField(JXCConstants.OHBLFieldPositions.ReceivingAgentName, ReceivingAgent.OH_FullNameTruncated, JXCConstants.OHBLFieldBoundaries.ReceivingAgentNameMaxLength);
				dataRow.SetField(JXCConstants.OHBLFieldPositions.ReceivingAgentAddress, ReceivingAgent.MainAddress.OA_Address1, JXCConstants.OHBLFieldBoundaries.ReceivingAgentAddressMaxLength);
				dataRow.SetField(JXCConstants.OHBLFieldPositions.ReceivingAgentCity, ReceivingAgent.MainAddress.OA_City);
			}
		}

		OrgHeader ReceivingAgent
		{
			get { return (Consol != null) ? Consol.ReceivingForwarder : null; }
		}

		#endregion

		#region Handling Instructions

		void SetHandlingInstructionsFields(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(JXCConstants.OHBLFieldPositions.HandlingInstructions1, HouseBillOfLading.HandlingInstructions.SubstringSafe(0, 35));
			dataRow.SetField(JXCConstants.OHBLFieldPositions.HandlingInstructions2, HouseBillOfLading.HandlingInstructions.SubstringSafe(35, 35));
			dataRow.SetField(JXCConstants.OHBLFieldPositions.HandlingInstructions3, HouseBillOfLading.HandlingInstructions.SubstringSafe(70, 35));
		}

		#endregion

		#region Place of Receipt and Delivery

		void SetPlaceOfReceiptAndDeliveryFields(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(JXCConstants.OHBLFieldPositions.PlaceOfReceipt, HouseBillOfLading.PlaceOfReceipt, JXCConstants.OHBLFieldBoundaries.PlaceNameMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.PlaceOfDelivery, HouseBillOfLading.PlaceOfDelivery, JXCConstants.OHBLFieldBoundaries.PlaceNameMaxLength);
		}

		#endregion

		#region Marks and Numbers

		void SetMarksAndNumbersField(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(JXCConstants.OHBLFieldPositions.MarksAndNumbers, HouseBillOfLading.MarksAndNumbers, JXCConstants.OHBLFieldBoundaries.MarksAndNumbersMaxLength);
		}

		#endregion

		#region Payable By

		void SetPayableByFields(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(JXCConstants.OHBLFieldPositions.FreightPayableBy, HouseBillOfLading.FreightPayableBy, JXCConstants.OHBLFieldBoundaries.PayableByMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.PreCarriagePayableBy, HouseBillOfLading.FreightPayableBy, JXCConstants.OHBLFieldBoundaries.PayableByMaxLength);
			dataRow.SetField(JXCConstants.OHBLFieldPositions.OnCarriagePayableBy, HouseBillOfLading.FreightPayableBy, JXCConstants.OHBLFieldBoundaries.PayableByMaxLength);
		}

		#endregion

		#region No of Original Bill of Ladings

		void SetNoOfOriginalBillOfLadingsField(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(JXCConstants.OHBLFieldPositions.NoOfOriginalBillOfLadings, HouseBillOfLading.NoOfBillOfLadings);
		}

		#endregion

		#region Exclude Commercial Invoice

		void SetExcludeCommercialInvoiceField(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(JXCConstants.OHBLFieldPositions.ExcludeCommercialInvoiceFromReport, "Y");
		}

		#endregion

		#endregion

		JASForwardingShipment Shipment
		{
			get { return HouseBillOfLading.Shipment; }
		}

		JASForwardingConsol Consol
		{
			get { return HouseBillOfLading.Consol; }
		}

		readonly IJXCExportHeader HeaderData;
		readonly ExportHouseBillOfLading HouseBillOfLading;
		readonly HouseLevelRecordType HouseBillOfLadingType;
	}
}
