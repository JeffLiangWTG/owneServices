using System.Collections;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CFSShipmentWrapper : SeaCargoDepotShipment
		, IObsoleteValidation
		, ICusUnderbondDependentCollectionParent
		, IAUCusUnderbondUnionCollectionParent
		, ISeaOutturnReportHeaderInformationProvider
		, IOutturnableLine
		, Customs.Business.IMessageManageableBizObj
	{
		protected CFSShipmentWrapper(CFSShipment shipment)
			: base(shipment)
		{
		}

		public static new CFSShipmentWrapper Load(CFSShipment parent)
		{
			return (CFSShipmentWrapper)Load(typeof(CFSShipmentWrapper), parent);
		}

		public ZString UnderbondHumanReadableName
		{
			get { return "Shipment: " + Shipment.JS_HouseBill; }
		}

		#region Related Business Objects

		Customs.Business.CusUnderbondCollection ICusUnderbondDependentCollectionParent.Underbonds => Underbonds;
		[ChildEditable(true)]
		public CusUnderbondCollection Underbonds
		{
			get
			{
				if (fUnderbonds == null)
				{
					fUnderbonds = new CusUnderbondCollection(this);
					fUnderbonds.Load();
					RegisterEditableChildObject(fUnderbonds);
				}
				return fUnderbonds;
			}
		}
		CusUnderbondCollection fUnderbonds;

		public CFSShipmentWrapperDepotCusOutturnCollection Outturns
		{
			get
			{
				if (outturns == null)
				{
					outturns = new CFSShipmentWrapperDepotCusOutturnCollection(this);
					outturns.Load();
					RegisterEditableChildObject(outturns);
				}
				return outturns;
			}
		}
		CFSShipmentWrapperDepotCusOutturnCollection outturns;

		public DepotCusOutturn Outturn
		{
			get
			{
				DepotCusOutturn result = null;
				if (Outturns.Count > 0)
				{
					result = Outturns[0];

					if (Outturns.Count > 1)
					{
						ErrorReporter.ReportOnce("cfsshipmentwrapperauoutturnproperty", "More than one depot cus outturn is linked to Shipment: " + Shipment.JS_UniqueConsignRef);
					}
				}
				return result;
			}
		}

		#endregion

		#region Old Logging Stuff

		public override ZGuid IMPMessage
		{
			get
			{
				return base.IMPMessage;
			}
			set
			{
				base.IMPMessage = value;
				var depotHouse = Factory.Load<CusSCADepotHouse>(value);
				if (depotHouse != null)
				{
					for (int i = depotHouse.Underbonds.Count - 1; i >= 0; i--)
					{
						CusUnderbond underbond = depotHouse.Underbonds[i];
						depotHouse.Underbonds.Remove(underbond);
						Underbonds.Add(underbond);
						AllUnderbonds.Load();
					}
					StmALog lastEvent = depotHouse.Logs.MostRecentLogByEventTimeExcludingEstimated(Events.SeaCargoDepotEvent);
					if (lastEvent != null)
					{
						Shipment.Logs.AddNew(Events.SeaCargoDepotEvent, lastEvent.SL_Reference, lastEvent.SL_EventTimeOffset);
					}
				}
			}
		}

		public override ZGuid CSAMessage
		{
			get
			{
				return base.CSAMessage;
			}
			set
			{
				base.CSAMessage = value;
				var depotHouse = Factory.Load<CusSCADepotHouse>(value);
				if (depotHouse != null)
				{
					StmALog lastEvent = depotHouse.Logs.MostRecentLogByEventTimeExcludingEstimated(Events.SeaCargoDepotEvent);
					if (lastEvent != null)
					{
						Shipment.Logs.AddNew(Events.SeaCargoDepotEvent, lastEvent.SL_Reference, lastEvent.SL_EventTimeOffset);
					}
				}
			}
		}

		public override CusSCADepotHouseList IMPMessage_List
		{
			get
			{
				CusSCADepotHouseList result = new CusSCADepotHouseList(Factory);
				result.MessageType = CMRMessage.CMRMessageTypes.UBMREQR;
				return result;
			}
		}

		public override CusSCADepotHouseList CSAMessage_List
		{
			get
			{
				CusSCADepotHouseList result = new CusSCADepotHouseList(Factory);
				result.MessageType = CMRMessage.CMRMessageTypes.CARST;
				return result;
			}
		}

		#endregion

		#region IOutturnableLine Members

		bool IOutturnableLine.IsDeleted
		{
			get { return Shipment.IsDeleted; }
		}

		ZString IOutturnableLine.CargoStatus
		{
			get { return Shipment.JS_ShipmentStatus; }
		}

		ZInt IOutturnableLine.PackagesManifested
		{
			get { return Shipment.JS_OuterPacks; }
		}

		#endregion

		#region ICusUnderbondDependentCollectionParent Members

		ZString ICusUnderbondDependentCollectionParent.Details
		{
			get { return "Shipment: " + Shipment.JS_HouseBill; }
		}

		IOutturnableLine[] ICusUnderbondDependentCollectionParent.OutturnableLines
		{
			get
			{
				ArrayList result = new ArrayList();
				result.Add(this);
				return (IOutturnableLine[])result.ToArray(typeof(IOutturnableLine));
			}
		}

		bool ICusUnderbondDependentCollectionParent.CanSendWithoutDelay
		{
			get { return true; }
		}

		bool ICusUnderbondDependentCollectionParent.UsesTranshipmentPortOnUnderbond
		{
			get { return false; }
		}

		ZString ICusUnderbondDependentCollectionParent.DefaultTranshipmentPort
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region ICusUnderbondUnionCollectionParent Members

		Customs.Business.CusUnderbondUnionCollection ICusUnderbondUnionCollectionParent.AllUnderbonds => AllUnderbonds;

		public CusUnderbondUnionCollection AllUnderbonds
		{
			get
			{
				if (fAllUnderbonds == null)
				{
					fAllUnderbonds = new CusUnderbondUnionCollection(this);
					fAllUnderbonds.Load();
				}
				return fAllUnderbonds;
			}
		}
		CusUnderbondUnionCollection fAllUnderbonds;

		public ICusUnderbondDependentCollectionParent[] GetAllPossibleCollectionProviders()
		{
			return new ICusUnderbondDependentCollectionParent[] { this };
		}

		bool ICusUnderbondUnionCollectionParent.IsForAirCargo
		{
			get { return false; }
		}

		#endregion

		#region ISeaOutturnReportHeaderInformationProvider Members

		public ISeaOutturnReportHeaderInformation GetHeader(CusUnderbond underbond)
		{
			if (underbond != null)
			{
				var outturnHeader = Factory.Load<CusOutturnHeader>(underbond.C4_C6);
				if (outturnHeader != null)
				{
					return new DepotCusOutturnHeaderOutturnReportHeaderInformation(outturnHeader);
				}
			}
			return null;
		}

		#endregion

		#region Create And Link Outturn

		internal void CreateAndLinkOutturnIfMissing(CusOutturnHeader header)
		{
			if (Outturn == null)
			{
				DepotCusOutturnLoaderOrCreator creator = new DepotCusOutturnLoaderOrCreator(Shipment, header);

				DepotCusOutturn result = creator.FindMatchingOutturn()
					?? creator.CreateOutturn();

				Outturns.Add(result);
			}
		}

		#endregion

		#region IMessageManageableBizObj Members

		Customs.Business.IMessageManager Customs.Business.IMessageManageableBizObj.GetMessageManagerForAmendmentDetection()
		{
			return new SeaCargoDepotMultiMessageManager(this);
		}

		bool Customs.Business.IMessageManageableBizObj.IsInAStatusAmendmentSendable
		{
			get { return true; }
		}

		Customs.Business.ContinueWithDetection Customs.Business.IMessageManageableBizObj.ProcessBeforeDetectingAmendmentAndContinue()
		{
			return Customs.Business.ContinueWithDetection.Yes;
		}

		#endregion
	}
}
