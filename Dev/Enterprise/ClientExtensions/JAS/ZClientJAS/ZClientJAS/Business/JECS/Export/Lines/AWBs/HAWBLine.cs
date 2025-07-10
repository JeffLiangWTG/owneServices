using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business.AWB;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public class HAWBLine : AWBLine
	{
		public HAWBLine(IJXCExportHeader jXCHeader, ShipmentExportAWBHeader aWBHeader)
			: this(jXCHeader, aWBHeader, HouseLevelRecordType.Standard)
		{
		}

		public HAWBLine(IJXCExportHeader jXCHeader, ShipmentExportAWBHeader aWBHeader, HouseLevelRecordType hAWBType)
			: base(aWBHeader)
		{
			this.JXCHeader = jXCHeader;
			this.HAWBType = hAWBType;
		}

		#region Overrides

		protected override int FieldCount
		{
			get { return (HAWBType == HouseLevelRecordType.Standard) ? JXCConstants.HAWBFieldCount : JXCConstants.CHABPSABFieldCount; }
		}

		protected override ZString LineType
		{
			get
			{
				ZString result;

				switch (HAWBType)
				{
					case HouseLevelRecordType.CoLoad:
						result = JXCConstants.LineTypes.CHAB;
						break;

					case HouseLevelRecordType.PreShipment:
						result = JXCConstants.LineTypes.PSAB;
						break;

					default:
						result = JXCConstants.LineTypes.HAWB;
						break;
				}

				return result;
			}
		}

		protected override JXCConstants.AWBFieldPositions NewFieldPositions()
		{
			return new JXCConstants.HAWBFieldPositions();
		}

		#endregion

		#region Set Field Values

		protected override void SetFieldValues(JXCFlatFileDataRow dataRow)
		{
			base.SetFieldValues(dataRow);
			SetAlsoNotifyPartyFields(dataRow);
			SetCommercialInvoiceToFollowField(dataRow);
			if (HAWBType == HouseLevelRecordType.Standard)
			{
				SetAMSAgentField(dataRow);
			}
		}

		protected override ZString DeclaredValueCurrency
		{
			get { return AWBHeader.EH_HouseDeclaredValueCurrency; }
		}

		protected override ZString CustomsValueCurrency
		{
			get { return AWBHeader.EH_HouseCustomsValueCurrency; }
		}

		#region Header Fields

		protected override void SetHeaderFields(JXCFlatFileDataRow dataRow)
		{
			base.SetHeaderFields(dataRow);
			ZString sendingOfficeCode = (JXCHeader.SendingForwarder != null) ? JXCHeader.SendingForwarder.OfficeCode : ZString.Empty;
			dataRow.SetField(JXCConstants.HAWBFieldPositions.OriginOfficeCode, sendingOfficeCode);

			if (Shipment != null)
			{
				dataRow.SetField(JXCConstants.HAWBFieldPositions.OriginTrafficFileNo, Shipment.JS_UniqueConsignRef);
				dataRow.SetField(JXCConstants.HAWBFieldPositions.HAWBSerialNo, Shipment.JS_HouseBill);
				if (Shipment.Origin != null && Shipment.Origin.Country != null)
				{
					dataRow.SetField(JXCConstants.HAWBFieldPositions.CountryOfFreightOrigin, Shipment.Origin.RL_RN_NKCountryCode);
				}
			}
		}

		#endregion

		#region Shipper Fields

		protected override void SetShipperFields(JXCFlatFileDataRow dataRow)
		{
			base.SetShipperFields(dataRow);
			dataRow.SetField(JXCConstants.HAWBFieldPositions.UpdateShipper, "N");
			dataRow.SetField(JXCConstants.HAWBFieldPositions.ShipperPhone, AWBHeader.EH_ShipperContactDetail);
		}

		protected override int ShipperAddressMaxLength
		{
			get { return JXCConstants.HAWBFieldBoundaries.ShipperAddressMaxLength; }
		}

		#endregion

		#region Consignee Fields

		protected override void SetConsigneeFields(JXCFlatFileDataRow dataRow)
		{
			base.SetConsigneeFields(dataRow);
			dataRow.SetField(JXCConstants.HAWBFieldPositions.UpdateConsignee, "N");
			dataRow.SetField(JXCConstants.HAWBFieldPositions.ConsigneePhone, AWBHeader.EH_ConsigneeContactDetail);
		}

		protected override int ConsigneeAddressMaxLength
		{
			get { return JXCConstants.HAWBFieldBoundaries.ConsigneeAddressMaxLength; }
		}

		#endregion

		#region Also Notify Party Fields

		void SetAlsoNotifyPartyFields(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(JXCConstants.HAWBFieldPositions.AlsoNotifyPartyName, AWBHeader.EH_AlsoNotifyName);
			dataRow.SetField(JXCConstants.HAWBFieldPositions.AlsoNotifyPartyAddress1, AWBHeader.EH_AlsoNotifyAddress, JXCConstants.HAWBFieldBoundaries.AlsoNotifyAddressMaxLength);
			dataRow.SetField(JXCConstants.HAWBFieldPositions.AlsoNotifyPartyAddress2, AWBHeader.EH_AlsoNotifyAddress2, JXCConstants.HAWBFieldBoundaries.AlsoNotifyAddressMaxLength);
			dataRow.SetField(JXCConstants.HAWBFieldPositions.UpdateAlsoNotifyParty, "N");
			dataRow.SetField(JXCConstants.HAWBFieldPositions.AlsoNotifyPartyPhone, AWBHeader.EH_AlsoNotifyContactDetail);
			dataRow.SetField(JXCConstants.HAWBFieldPositions.AlsoNotifyPartyPlace, AWBHeader.EH_AlsoNotifyPlace);
		}

		#endregion

		#region Commercial Invoice To Follow Field

		void SetCommercialInvoiceToFollowField(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(JXCConstants.HAWBFieldPositions.ExcludeCommercialInvoiceFromReports, "Y");
		}

		#endregion

		#region AMS Agent Field

		void SetAMSAgentField(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(JXCConstants.HAWBFieldPositions.AMSAgent, (IsUSBoundShipment) ? "sectra" : "");
		}

		bool IsUSBoundShipment
		{
			get
			{
				bool shipmentDestIsUS = false;
				if (Shipment != null)
				{
					shipmentDestIsUS = (Shipment.Destination != null && Shipment.Destination.RL_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedStates);
					if (!shipmentDestIsUS)
					{
						foreach (JASForwardingConsol consol in Shipment.Consols)
						{
							if (consol.DischargePort != null && consol.DischargePort.RL_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedStates)
							{
								shipmentDestIsUS = true;
								break;
							}
						}
					}
				}
				return shipmentDestIsUS;
			}
		}

		#endregion

		protected override int CarrierAddressMaxLength
		{
			get { return JXCConstants.HAWBFieldBoundaries.CarrierAddressMaxLength; }
		}

		#endregion

		#region Data Source

		JASForwardingShipment Shipment
		{
			get { return (JASForwardingShipment)AWBHeader.Shipment; }
		}

		new ShipmentExportAWBHeader AWBHeader
		{
			get { return (ShipmentExportAWBHeader)base.AWBHeader; }
		}

		readonly IJXCExportHeader JXCHeader;
		readonly HouseLevelRecordType HAWBType;

		#endregion
	}
}
