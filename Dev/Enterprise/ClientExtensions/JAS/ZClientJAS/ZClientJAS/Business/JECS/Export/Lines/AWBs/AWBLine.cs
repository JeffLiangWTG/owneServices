using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.JAS.Business.AWB;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public abstract class AWBLine : MessageLine
	{
		public AWBLine(ExportAWBHeader awbHeader)
		{
			this.AWBHeader = awbHeader;
			this.type = awbHeader.GetType();
			this.parentID = awbHeader.EH_ParentID;
			this.parentTable = awbHeader.EH_Table;
			this.pk = awbHeader.PK;
			this.factory = awbHeader.Factory;
		}

		#region Field Positions

		protected abstract JXCConstants.AWBFieldPositions NewFieldPositions();

		protected JXCConstants.AWBFieldPositions FieldPositions
		{
			get
			{
				if (fFieldPositions == null)
				{
					fFieldPositions = NewFieldPositions();
				}

				return fFieldPositions;
			}
		}

		JXCConstants.AWBFieldPositions fFieldPositions;

		#endregion

		#region Set Field Values

		protected override void SetFieldValues(JXCFlatFileDataRow dataRow)
		{
			if (AWBHeader == null || AWBHeader.IsDeleted)
			{
				AWBHeader = ReloadAWBHeader();
				if (AWBHeader == null)
				{
					return;
				}
			}

			SetHeaderFields(dataRow);
			SetShipperFields(dataRow);
			SetConsigneeFields(dataRow);
			SetCarrierFields(dataRow);
			SetAgentFields(dataRow);
			SetAccountingInfoFields(dataRow);
			SetRoutingFields(dataRow);
			SetFreightDeclarationFields(dataRow);
			SetFlightInfoFields(dataRow);
			SetHandlingInfoFields(dataRow);
			SetFreightInfoFields(dataRow);
			SetChargesFields(dataRow);
			SetFooterFields(dataRow);
		}

		ExportAWBHeader ReloadAWBHeader()
		{
			var query = new ZDBOnlyQuery(type);
			query.AddToFilter(ExportAWBHeaderSchema.EH_ParentID, parentID);
			query.AddToFilter(ExportAWBHeaderSchema.EH_Table, parentTable);
			query.AddToFilter(ExportAWBHeaderSchema.PK, SQLComparisonOperator.NotEqual, pk);

			return (ExportAWBHeader)factory.LoadTop1(type, query);
		}

		#region Header Fields

		protected virtual void SetHeaderFields(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(FieldPositions.TypeOfRecord, JXCConstants.TypeOfRecord.New);
			dataRow.SetField(FieldPositions.OriginCityCode, AWBHeader.EH_AWBOriginCode);
			dataRow.SetField(FieldPositions.AirlinePrefix, AWBHeader.EH_AirlinePrefix);
			dataRow.SetField(FieldPositions.MAWBSerialNo, AWBHeader.EH_AWBSerialNo);
		}

		#endregion

		#region Shipper Fields

		protected abstract int ShipperAddressMaxLength { get; }

		protected virtual void SetShipperFields(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(FieldPositions.ShipperName, AWBHeader.EH_ShipperName);
			dataRow.SetField(FieldPositions.ShipperAddress1, AWBHeader.EH_ShipperAddress, ShipperAddressMaxLength);
			dataRow.SetField(FieldPositions.ShipperAddress2, AWBHeader.EH_ShipperAddress2, ShipperAddressMaxLength);
			dataRow.SetField(FieldPositions.ShipperCity, AWBHeader.EH_ShipperPlace);
			dataRow.SetField(FieldPositions.ShipperPostCode, AWBHeader.EH_ShipperPostCode);
			dataRow.SetField(FieldPositions.ShipperState, AWBHeader.EH_ShipperState);
			dataRow.SetField(FieldPositions.ShipperCountryCode, AWBHeader.EH_ShipperCountryCode);
			dataRow.SetField(FieldPositions.ShipperAccountNo, ((IJASExportAWBHeader)AWBHeader).ShipperAccountForJXC);
		}

		#endregion

		#region Consignee Fields

		protected abstract int ConsigneeAddressMaxLength { get; }

		protected virtual void SetConsigneeFields(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(FieldPositions.ConsigneeName, AWBHeader.EH_ConsigneeName);
			dataRow.SetField(FieldPositions.ConsigneeAddress1, AWBHeader.EH_ConsigneeAddress, ConsigneeAddressMaxLength);
			dataRow.SetField(FieldPositions.ConsigneeAddress2, AWBHeader.EH_ConsigneeAddress2, ConsigneeAddressMaxLength);
			dataRow.SetField(FieldPositions.ConsigneeCity, AWBHeader.EH_ConsigneePlace);
			dataRow.SetField(FieldPositions.ConsigneePostCode, AWBHeader.EH_ConsigneePostCode);
			dataRow.SetField(FieldPositions.ConsigneeState, AWBHeader.EH_ConsigneeState);
			dataRow.SetField(FieldPositions.ConsigneeCountryCode, AWBHeader.EH_ConsigneeCountryCode);
			dataRow.SetField(FieldPositions.ConsigneeAccountNo, ((IJASExportAWBHeader)AWBHeader).ConsigneeAccountForJXC);
		}

		#endregion

		#region Carrier Fields

		protected abstract int CarrierAddressMaxLength { get; }

		void SetCarrierFields(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(FieldPositions.CarrierName, AWBHeader.EH_IssuingAgentName);
			dataRow.SetField(FieldPositions.CarrierAddress1, AWBHeader.EH_IssuingAgentAddress1, CarrierAddressMaxLength);
			dataRow.SetField(FieldPositions.CarrierAddress2, AWBHeader.EH_IssuingAgentAddress2, CarrierAddressMaxLength);
		}

		#endregion

		#region Agent Fields

		void SetAgentFields(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(FieldPositions.AgentName, AWBHeader.EH_AgentName);
			dataRow.SetField(FieldPositions.AgentAddress1, AWBHeader.EH_AgentPlace, JXCConstants.AWBFieldBoundaries.AgentAddressMaxLength);
			dataRow.SetField(FieldPositions.AgentAddress2, AWBHeader.EH_AgentPlace, JXCConstants.AWBFieldBoundaries.AgentAddressMaxLength);
			dataRow.SetField(FieldPositions.AgentAddress3, AWBHeader.EH_AgentPlace, JXCConstants.AWBFieldBoundaries.AgentPlaceMaxLength);
			dataRow.SetField(FieldPositions.AgentIATACode, AWBHeader.EH_AgentIATACodeFormatted);
			dataRow.SetField(FieldPositions.AgentAccountNo, AWBHeader.EH_AgentAccountNo);
		}

		#endregion

		#region Accounting Info Fields

		void SetAccountingInfoFields(JXCFlatFileDataRow dataRow)
		{
			int noOfAccountingInfoFields = 7;

			for (int i = 0; i < noOfAccountingInfoFields; i++)
			{
				if (AWBHeader.AWBAccountingInformations.Count > i)
				{
					int currentFieldPosIndex = FieldPositions.AccountingInfo1 + i;
					dataRow.SetField(currentFieldPosIndex, AWBHeader.AWBAccountingInformations[i].EA_Information, JXCConstants.AWBFieldBoundaries.AccountingInfoMaxLength);
				}
			}
		}

		#endregion

		#region Routing Fields

		void SetRoutingFields(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(FieldPositions.To1st, AWBHeader.EH_To1st);
			dataRow.SetField(FieldPositions.By1st, AWBHeader.EH_By1st);
			dataRow.SetField(FieldPositions.To2nd, AWBHeader.EH_To2nd);
			dataRow.SetField(FieldPositions.By2nd, AWBHeader.EH_By2nd);
			dataRow.SetField(FieldPositions.To3rd, AWBHeader.EH_To3rd);
			dataRow.SetField(FieldPositions.By3rd, AWBHeader.EH_By3rd);
		}

		#endregion

		#region Freight Declaration Fields

		protected abstract ZString DeclaredValueCurrency { get; }
		protected abstract ZString CustomsValueCurrency { get; }

		void SetFreightDeclarationFields(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(FieldPositions.ChargeCode, AWBHeader.EH_ChargesCode);
			dataRow.SetField(FieldPositions.WeightPPDorCOL, AWBHeader.EH_WeightVPPDCOL.Left(1));
			dataRow.SetField(FieldPositions.OtherPPDorCOL, AWBHeader.EH_OtherPPDCOL.Left(1));
			dataRow.SetField(FieldPositions.DeclaredValue, AWBHeader.EH_DeclaredValue);
			dataRow.SetField(FieldPositions.CurrencyCodeForDeclaredValue, DeclaredValueCurrency);
			dataRow.SetField(FieldPositions.CustomsValue, AWBHeader.EH_CustomsValue);
			dataRow.SetField(FieldPositions.CurrencyCodeForCustomsValue, CustomsValueCurrency);
			dataRow.SetField(FieldPositions.Currency, AWBHeader.EH_Currency);
		}

		#endregion

		#region Flight Info Fields

		void SetFlightInfoFields(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(FieldPositions.AirportOfDeparture, AWBHeader.EH_AirportOfDepartureAndRequestRouteText);
			dataRow.SetField(FieldPositions.AirportOfDestination, AWBHeader.EH_AirportOfDestinationText);

			ZString flightNo1 = AWBHeader.EH_Booking1stCarrier + AWBHeader.EH_Booking1stFlight;
			dataRow.SetField(FieldPositions.FlightNo1, flightNo1);
			dataRow.SetField(FieldPositions.FlightDate1, AWBHeader.Booking1stFlightDate);

			ZString flightNo2 = AWBHeader.EH_Booking2ndCarrier + AWBHeader.EH_Booking2ndFlight;
			dataRow.SetField(FieldPositions.FlightNo2, flightNo2);
			dataRow.SetField(FieldPositions.FlightDate2, AWBHeader.Booking2ndFlightDate);

			dataRow.SetField(FieldPositions.Insurance, AWBHeader.EH_InsuranceValue);
		}

		#endregion

		#region Handling Info Fields

		void SetHandlingInfoFields(JXCFlatFileDataRow dataRow)
		{
			ZString handlingInfo1 = AWBHeader.EH_HandlingInformation.SubstringSafe(0, 60);
			ZString handlingInfo2 = AWBHeader.EH_HandlingInformation.SubstringSafe(60, 60);
			ZString handlingInfo3 = AWBHeader.EH_HandlingInformation.SubstringSafe(120, 60);

			dataRow.SetField(FieldPositions.HandlingInfo1, handlingInfo1);
			dataRow.SetField(FieldPositions.HandlingInfo2, handlingInfo2);
			dataRow.SetField(FieldPositions.HandlingInfo3, handlingInfo3);
		}

		#endregion

		#region Freight Info Fields

		void SetFreightInfoFields(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(FieldPositions.TotalNoOfPieces, AWBHeader.EH_TotalNoOfPieces);
			dataRow.SetField(FieldPositions.TotalGrossWeight, AWBHeader.EH_TotalGrossWeight);
			dataRow.SetField(FieldPositions.WeightUnit, GetFirstRateLineWeightUnit());
			dataRow.SetField(FieldPositions.Total, AWBHeader.EH_TotalLineTotals);
		}

		ZString GetFirstRateLineWeightUnit()
		{
			ExportAWBRateLine rateLine = AWBHeader.AWBRateLines[ExportAWBRateLineSchema.Constants.ER_LineCount, (ZByte)1];
			return (rateLine != null) ? rateLine.ER_WeightInLBsOrKGs : ZString.Empty;
		}

		#endregion

		#region Charges Fields

		void SetChargesFields(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(FieldPositions.PrepaidWeightCharge, AWBHeader.EH_TotalWeightPPD);
			dataRow.SetField(FieldPositions.CollectWeightCharge, AWBHeader.EH_TotalWeightCOL);
			dataRow.SetField(FieldPositions.PrepaidValuationCharge, AWBHeader.EH_ValuationPPD);
			dataRow.SetField(FieldPositions.CollectValuationCharge, AWBHeader.EH_ValuationCOL);
			dataRow.SetField(FieldPositions.PrepaidTax, AWBHeader.EH_TaxesPPD);
			dataRow.SetField(FieldPositions.CollectTax, AWBHeader.EH_TaxesCOL);
			dataRow.SetField(FieldPositions.PrepaidOtherChargesDueAgent, AWBHeader.EH_OtherChargesDueAgentPPD);
			dataRow.SetField(FieldPositions.CollectOtherChargesDueAgent, AWBHeader.EH_OtherChargesDueAgentCOL);
			dataRow.SetField(FieldPositions.PrepaidOtherChargesDueCarrier, AWBHeader.EH_OtherChargesDueCarrierPPD);
			dataRow.SetField(FieldPositions.CollectOtherChargesDueCarrier, AWBHeader.EH_OtherChargesDueCarrierCOL);
			dataRow.SetField(FieldPositions.TotalPrepaid, AWBHeader.EH_TotalPPD);
			dataRow.SetField(FieldPositions.TotalCollect, AWBHeader.EH_TotalCOL);
		}

		#endregion

		#region Footer Fields

		void SetFooterFields(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(FieldPositions.SignatureOfShipperOrAgent, AWBHeader.EH_ShippersSignature);
			dataRow.SetField(FieldPositions.DateOfIssue, AWBHeader.EH_AWBIssueDate);
			dataRow.SetField(FieldPositions.PlaceOfIssue, AWBHeader.EH_AWBIssuePlace);
			dataRow.SetField(FieldPositions.SignatureOfCarrierOrAgent, AWBHeader.EH_AWBAgentsSignature);
		}

		#endregion

		#endregion

		protected ExportAWBHeader AWBHeader;
		protected readonly Type type;
		protected readonly ZGuid parentID;
		protected readonly ZString parentTable;
		protected readonly ZGuid pk;
		protected readonly BusinessObjectFactory factory;
	}
}
