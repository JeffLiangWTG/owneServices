using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SeaCargoDepotShipment : SeaCargoDepotBusinessObject
	{
		#region Schema

		public new abstract class Schema : SeaCargoDepotBusinessObject.Schema
		{
			public const string HouseBill = "HouseBill";
			public const string IMPMessage = "IMPMessage";
			public const string CSAMessage = "CSAMessage";
		}

		#endregion

		protected SeaCargoDepotShipment(CFSShipment shipment)
			: base(shipment)
		{
			fShipment = shipment;
		}

		public static SeaCargoDepotShipment Load(CFSShipment shipment)
		{
			return (SeaCargoDepotShipment)Load(typeof(SeaCargoDepotShipment), shipment);
		}

		#region Properties

		public override CFSLoadListConsol ParentConsol
		{
			get
			{
				CFSLoadListConsol result = null;
				if (Shipment != null)
				{
					result = Shipment.ArrivalConsol as CFSLoadListConsol;
				}
				return result;
			}
		}

		public CFSShipment Shipment
		{
			get { return fShipment; }
		}

		public CommonContainer Container
		{
			get
			{
				CommonContainer result = null;
				foreach (PackLine packLine in Shipment.OuterPackLines)
				{
					result = packLine.GetContainer(ParentConsol);
					if (result != null)
					{
						break;
					}
				}
				return result;
			}
		}

		public CusSCADepotHouseCollection ReportedHouseBills
		{
			get
			{
				if (fReportedHouseBills == null)
				{
					fReportedHouseBills = new CusSCADepotHouseCollection(Shipment, Factory);
					fReportedHouseBills.Load();
				}
				return fReportedHouseBills;
			}
		}

		[CargoWise.ComponentModel.MaxLength(20)]
		public ZString HouseBill
		{
			get
			{
				ZString result = "";
				if (Shipment != null)
				{
					result = Shipment.JS_HouseBill;
				}
				return result;
			}
		}

		public ZPropertyInfo HouseBillInfo
		{
			get { return GetZPropertyInfo(Schema.HouseBill); }
		}

		#region IMPMessage

		bool iMPMessageChecked;
		ZGuid fIMPMessage;
		public virtual ZGuid IMPMessage
		{
			get
			{
				if (!iMPMessageChecked)
				{
					iMPMessageChecked = true;
					ZQuery impendingArrivalDepotContainerFilter = new ZQuery(CusSCADepotHouseSchema.CX_JS, Shipment.PK);
					impendingArrivalDepotContainerFilter.AddToFilter(CusSCADepotHouseSchema.CX_Status, SeaCargoMessageTypes.ImpendingArrival);
					CusSCADepotHouse[] impendingArrivalContainers = (CusSCADepotHouse[])Factory.Load(typeof(CusSCADepotHouse), impendingArrivalDepotContainerFilter);
					CusSCADepotHouse mostRecentImpendingArrival = null;
					foreach (CusSCADepotHouse impendingArrivalMessage in impendingArrivalContainers)
					{
						if (mostRecentImpendingArrival == null)
						{
							mostRecentImpendingArrival = impendingArrivalMessage;
						}
						else
						{
							StmALog addedEventToCheck = impendingArrivalMessage.Logs.MostRecentLogByEventTime(Events.AddedARecordToTheSystem);
							StmALog currentAddedEvent = mostRecentImpendingArrival.Logs.MostRecentLogByEventTime(Events.AddedARecordToTheSystem);
							if (addedEventToCheck != null && currentAddedEvent != null)
							{
								if (addedEventToCheck.SL_EventTime < currentAddedEvent.SL_EventTime)
								{
									mostRecentImpendingArrival = impendingArrivalMessage;
								}
							}
						}
					}
					if (mostRecentImpendingArrival != null)
					{
						fIMPMessage = mostRecentImpendingArrival.PK;
					}
				}
				return fIMPMessage;
			}
			set
			{
				if (value != fIMPMessage)
				{
					if (fIMPMessage != ZGuid.Empty)
					{
						var currentHouse = Factory.Load<CusSCADepotHouse>(fIMPMessage);
						if (currentHouse != null)
						{
							currentHouse.CX_JS = ZGuid.Empty;
						}
					}
					if (value != ZGuid.Empty)
					{
						var newHouse = Factory.Load<CusSCADepotHouse>(value);
						if (newHouse != null)
						{
							newHouse.CX_JS = Shipment.PK;
							var relatedContainer = Factory.Load<CusSCADepotContainer>(newHouse.CX_CJ);
						}
					}
					SetNonPersistentPropertyValue(IMPMessageInfo, ref fIMPMessage, value);
					if (value == ZGuid.Empty)
					{
						LogCancelImpendingArrival();
					}
					else
					{
						LogImpendingArrival();
					}
				}
			}
		}

		public ZPropertyInfo IMPMessageInfo
		{
			get { return GetZPropertyInfo(Schema.IMPMessage); }
		}

		public virtual CusSCADepotHouseList IMPMessage_List
		{
			get
			{
				CusSCADepotHouseList result = new CusSCADepotHouseList(Factory);
				result.MessageType = SeaCargoMessageTypes.ImpendingArrival;
				return result;
			}
		}

		#endregion

		#region CSAMessage

		bool cSAMessageChecked;
		ZGuid fCSAMessage;
		public virtual ZGuid CSAMessage
		{
			get
			{
				if (!cSAMessageChecked)
				{
					cSAMessageChecked = true;
					ZQuery cargoStatusAdviceDepotContainerFilter = new ZQuery(CusSCADepotHouseSchema.CX_JS, Shipment.PK);
					cargoStatusAdviceDepotContainerFilter.AddToFilter(CusSCADepotHouseSchema.CX_Status, SeaCargoMessageTypes.CargoStatusAdvice);
					CusSCADepotHouse[] cargoStatusAdviceHouseBills = (CusSCADepotHouse[])Factory.Load(typeof(CusSCADepotHouse), cargoStatusAdviceDepotContainerFilter);
					CusSCADepotHouse mostRecentCargoStatusAdvice = null;
					foreach (CusSCADepotHouse cargoStatusAdviceMessage in cargoStatusAdviceHouseBills)
					{
						if (mostRecentCargoStatusAdvice == null)
						{
							mostRecentCargoStatusAdvice = cargoStatusAdviceMessage;
						}
						else
						{
							StmALog addedEventToCheck = cargoStatusAdviceMessage.Logs.MostRecentLogByEventTime(Events.AddedARecordToTheSystem);
							StmALog currentAddedEvent = mostRecentCargoStatusAdvice.Logs.MostRecentLogByEventTime(Events.AddedARecordToTheSystem);
							if (addedEventToCheck != null && currentAddedEvent != null)
							{
								if (addedEventToCheck.SL_EventTime < currentAddedEvent.SL_EventTime)
								{
									mostRecentCargoStatusAdvice = cargoStatusAdviceMessage;
								}
							}
						}
					}
					if (mostRecentCargoStatusAdvice != null)
					{
						fCSAMessage = mostRecentCargoStatusAdvice.PK;
					}
				}
				return fCSAMessage;
			}
			set
			{
				if (value != fCSAMessage)
				{
					if (fCSAMessage != ZGuid.Empty)
					{
						var currentShipment = Factory.Load<CusSCADepotHouse>(fCSAMessage);
						if (currentShipment != null)
						{
							currentShipment.CX_JS = ZGuid.Empty;
						}
					}
					if (value != ZGuid.Empty)
					{
						var newShipment = Factory.Load<CusSCADepotHouse>(value);
						if (newShipment != null)
						{
							newShipment.CX_JS = Shipment.PK;
						}
					}
				}
				SetNonPersistentPropertyValue(CSAMessageInfo, ref fCSAMessage, value);
				if (value == ZGuid.Empty)
				{
					LogCancelCargoStatusAdvice();
				}
				else
				{
					LogCargoStatusAdvice();
				}
			}
		}

		public ZPropertyInfo CSAMessageInfo
		{
			get { return GetZPropertyInfo(Schema.CSAMessage); }
		}

		public virtual CusSCADepotHouseList CSAMessage_List
		{
			get
			{
				CusSCADepotHouseList result = new CusSCADepotHouseList(Factory);
				result.MessageType = SeaCargoMessageTypes.CargoStatusAdvice;
				return result;
			}
		}

		#endregion

		#endregion

		#region Implementation

		readonly CFSShipment fShipment;

		CusSCADepotHouseCollection fReportedHouseBills;

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			DepotLog.Load();
			Messages.Load();
		}

		#endregion

	}
}
