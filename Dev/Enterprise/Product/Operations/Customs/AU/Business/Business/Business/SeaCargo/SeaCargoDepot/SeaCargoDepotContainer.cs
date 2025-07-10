using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SeaCargoDepotContainer : SeaCargoDepotBusinessObject
		, IOutturnableLine
		, ICusUnderbondDependentCollectionParent
		, IAUCusUnderbondUnionCollectionParent
		, ISeaOutturnReportHeaderInformationProvider
		, Customs.Business.IMessageManageableBizObj
	{
		public new abstract class Schema : SeaCargoDepotBusinessObject.Schema
		{
			public const string ContainerNumber = "ContainerNumber";
			public const string ResponsibleParty = "ResponsibleParty";
			public const string ResponsiblePartyCMP = "ResponsiblePartyCMP";
			public const string IMPMessage = "IMPMessage";
			public const string CSAMessage = "CSAMessage";
		}

		public static SeaCargoDepotContainer Load(CFSContainer container)
		{
			return (SeaCargoDepotContainer)Load(typeof(SeaCargoDepotContainer), container);
		}

		protected SeaCargoDepotContainer(CFSContainer container)
			: base(container)
		{
			fContainer = container;
		}

		#region Properties

		public override CFSLoadListConsol ParentConsol
		{
			get
			{
				CFSLoadListConsol result = null;
				if (Container != null)
				{
					result = Container.Consol;
				}
				return result;
			}
		}

		public CFSContainer Container
		{
			get { return fContainer; }
		}

		public CusSCADepotContainerCollection ReportedContainers
		{
			get
			{
				if (fReportedContainers == null)
				{
					fReportedContainers = new CusSCADepotContainerCollection(Container, Factory);
					fReportedContainers.Load();
				}
				return fReportedContainers;
			}
		}

		#region Container Number

		[CargoWise.ComponentModel.MaxLength(12)]
		public ZString ContainerNumber
		{
			get
			{
				ZString result = "";
				if (Container != null)
				{
					result = Container.JC_ContainerNum;
				}
				return result;
			}
		}

		public ZPropertyInfo ContainerNumberInfo
		{
			get { return GetZPropertyInfo(Schema.ContainerNumber); }
		}

		#endregion

		#region ResponsibleParty

		public ZGuid ResponsibleParty
		{
			get
			{
				ZGuid result = ZGuid.Empty;
				if (Container != null)
				{
					result = Container.JC_OH_CFSClient;
				}
				return result;
			}
		}

		public ZPropertyInfo ResponsiblePartyInfo
		{
			get { return GetZPropertyInfo(Schema.ResponsibleParty); }
		}

		public BusinessObjectCollection ClientList
		{
			get
			{
				return new ForwarderCollection(Factory);
			}
		}
		#endregion

		#region ResponsiblePartyCMP

		[CargoWise.ComponentModel.MaxLength(35)]
		public ZString ResponsiblePartyCMP
		{
			get
			{
				ZString result = "";
				if (Container != null && Container.CFSClient != null)
				{
					result = Container.CFSClient.LocalManifestID;
				}
				return result;
			}
		}

		public ZPropertyInfo ResponsiblePartyCMPInfo
		{
			get { return GetZPropertyInfo(Schema.ResponsiblePartyCMP); }
		}

		#endregion

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
					ZQuery impendingArrivalDepotContainerFilter = new ZQuery(CusSCADepotContainerSchema.CJ_JC, Container.PK);
					impendingArrivalDepotContainerFilter.AddToFilter(CusSCADepotContainerSchema.CJ_Status, SeaCargoMessageTypes.ImpendingArrival);
					CusSCADepotContainer[] impendingArrivalContainers = (CusSCADepotContainer[])Factory.Load(typeof(CusSCADepotContainer), impendingArrivalDepotContainerFilter);
					CusSCADepotContainer mostRecentImpendingArrival = null;
					foreach (CusSCADepotContainer impendingArrivalMessage in impendingArrivalContainers)
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
						var currentContainer = Factory.Load<CusSCADepotContainer>(fIMPMessage);
						if (currentContainer != null)
						{
							currentContainer.CJ_JC = ZGuid.Empty;
						}
					}
					if (value != ZGuid.Empty)
					{
						var newContainer = Factory.Load<CusSCADepotContainer>(value);
						if (newContainer != null)
						{
							newContainer.CJ_JC = Container.PK;
							StmALog lastEvent = Container.Logs.MostRecentLogByEventTime(Events.SeaCargoDepotEvent);
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

		public virtual CusSCADepotContainerList IMPMessage_List
		{
			get
			{
				CusSCADepotContainerList result = new CusSCADepotContainerList(Factory, SeaCargoMessageTypes.ImpendingArrival);
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
					ZQuery cargoStatusAdviceDepotContainerFilter = new ZQuery(CusSCADepotContainerSchema.CJ_JC, Container.PK);
					cargoStatusAdviceDepotContainerFilter.AddToFilter(CusSCADepotContainerSchema.CJ_Status, SeaCargoMessageTypes.CargoStatusAdvice);
					CusSCADepotContainer[] cargoStatusAdviceContainers = (CusSCADepotContainer[])Factory.Load(typeof(CusSCADepotContainer), cargoStatusAdviceDepotContainerFilter);
					CusSCADepotContainer mostRecentCargoStatusAdvice = null;
					foreach (CusSCADepotContainer cargoStatusAdviceMessage in cargoStatusAdviceContainers)
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
						var currentContainer = Factory.Load<CusSCADepotContainer>(fCSAMessage);
						if (currentContainer != null)
						{
							currentContainer.CJ_JC = ZGuid.Empty;
						}
					}
					if (value != ZGuid.Empty)
					{
						var newContainer = Factory.Load<CusSCADepotContainer>(value);
						if (newContainer != null)
						{
							newContainer.CJ_JC = Container.PK;
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
		}

		public ZPropertyInfo CSAMessageInfo
		{
			get { return GetZPropertyInfo(Schema.CSAMessage); }
		}

		public virtual CusSCADepotContainerList CSAMessage_List
		{
			get
			{
				CusSCADepotContainerList result = new CusSCADepotContainerList(Factory, SeaCargoMessageTypes.CargoStatusAdvice);
				return result;
			}
		}

		#endregion

		#endregion

		#region Implementation

		readonly CFSContainer fContainer;
		CusSCADepotContainerCollection fReportedContainers;

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			DepotLog.Load();
			Messages.Load();
		}

		#endregion

		#region IOutturnableLine Members

		public ZString UnderbondHumanReadableName
		{
			get { return "Container: " + Container.JC_ContainerNum; }
		}

		ZString IOutturnableLine.CargoStatus
		{
			get { return ZString.Empty; }
		}

		ZInt IOutturnableLine.PackagesManifested
		{
			get { return Container.JC_Calc_TotalPackages; }
		}

		bool IOutturnableLine.IsDeleted
		{
			get { return Container.IsDeleted; }
		}

		#endregion

		#region ICusUnderbondDependentCollectionParent Members

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

		ZString ICusUnderbondDependentCollectionParent.Details
		{
			get
			{
				return "Container: " + Container.JC_ContainerNum;
			}
		}

		IOutturnableLine[] ICusUnderbondDependentCollectionParent.OutturnableLines => Factory.GetValue(ref outturnableLines, GetOutturnableLines);

		CachedProperty<IOutturnableLine[]> outturnableLines;

		IOutturnableLine[] GetOutturnableLines()
		{
			List<IOutturnableLine> result = new List<IOutturnableLine>();
			result.Add(this);
			foreach (CFSShipment shipment in Container.PackUnpackShipments)
			{
				result.Add(CFSShipmentWrapper.Load(shipment));
			}
			return result.ToArray();
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
					RegisterEditableChildObject(fAllUnderbonds);
				}
				return fAllUnderbonds;
			}
		}
		CusUnderbondUnionCollection fAllUnderbonds;

		public ICusUnderbondDependentCollectionParent[] GetAllPossibleCollectionProviders()
		{
			ArrayList result = new ArrayList();
			foreach (CFSPackLine packLine in Container.PackLines)
			{
				if (packLine.Shipment != null)
				{
					result.Add(CFSShipmentWrapper.Load(packLine.Shipment));
				}
			}
			result.Add(this);
			return (ICusUnderbondDependentCollectionParent[])result.ToArray(typeof(ICusUnderbondDependentCollectionParent));
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
