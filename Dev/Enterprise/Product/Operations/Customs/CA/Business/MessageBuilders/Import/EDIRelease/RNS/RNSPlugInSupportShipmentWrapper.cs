namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	using System;
	using CargoWise.Common;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.CA.Business.MessageManagers;
	using Enterprise.Customs.Common;
	using Enterprise.Freight.Forwarding.Business;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Business;

	public class RNSPlugInSupportShipmentWrapper : IRNSPlugInSupport
	{
		public RNSPlugInSupportShipmentWrapper(Freight.Business.CommonShipment shipment)
		{
			Argument.NotNull(shipment, "shipment");
			this.shipment = shipment;
		}

		#region Implementation of IRNSRequest

		ZDateTime IRNSRequestData.DateOfArrival
		{
			get { return ZDateTime.Now; }
		}

		ZString IRNSRequestData.CargoControlNumber
		{
			get
			{
				var number = shipment.Numbers.GetFirstReferenceNumberByType(CanadaAdditionalReferenceNumberTypes.Codes.CCN);
				return number != null ? number.CE_EntryNum : ZString.Empty;
			}
		}

		ZString IRNSRequestData.HouseBillNumber
		{
			get { return shipment.JS_HouseBill; }
		}

		ZString IRNSRequestData.TransactionNumber
		{
			get { return ZString.Empty; }
		}

		ZString IRNSRequestData.OfficeCode
		{
			get { return ZString.Empty; }
		}

		ZString IRNSRequestData.SubLocationCode
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region Implementation of IRNSPlugInSupport

		BusinessObject IRNSPlugInSupport.Master
		{
			get { return shipment; }
		}

		Logs IRNSPlugInSupport.Logs
		{
			get { return shipment.Logs; }
		}

		bool IRNSPlugInSupport.ReleaseStatusEventsSupported
		{
			get
			{
				var result = true;
				if (shipment is ForwardingShipment forwardingShipment)
				{
					result = forwardingShipment.GetDeclaration() == null;
				}
				return result;
			}
		}

		bool IRNSPlugInSupport.PlugInVisible
		{
			get { return shipment.IsImport(); }
		}

		event EventHandler IRNSPlugInSupport.PlugInVisibilityDataChanged
		{
			add
			{
				shipment.JS_RL_NKOriginInfo.ValueChanged += value;
				shipment.JS_RL_NKDestinationInfo.ValueChanged += value;
			}
			remove
			{
				shipment.JS_RL_NKOriginInfo.ValueChanged -= value;
				shipment.JS_RL_NKDestinationInfo.ValueChanged -= value;
			}
		}

		RNSMultiMessageManager IRNSPlugInSupport.GetRNSMultiMessageManager()
		{
			throw new NotSupportedException();
		}

		#endregion

		readonly Freight.Business.CommonShipment shipment;
	}
}
