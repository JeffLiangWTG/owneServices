using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.BISI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public partial class UPEProcessQueueLog : ProcessQueueLog, IShipmentStatusData
	{
		public UPEProcessQueueLog(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override void SetQueueDetails(ZString queueName, ZString status, ZString subStatus, ZString reason, ZString assignedTo)
		{
			if (queueName == CargoReportQueueCodeDescriptionPairList.Codes.Quarantine)
			{
				if (status == ReasonCodeDescriptionPairList.AQUA.Codes.DI_DocInspect)
				{
					base.SetQueueDetails(queueName, ZString.Empty, subStatus, reason, assignedTo);
				}
				else if (status == ReasonCodeDescriptionPairList.AQUA.Codes._05_PkgInspect &&
					(ZString)Master.P4_CustomsStatusInfo.OriginalValue != ReasonCodeDescriptionPairList.AQUA.Codes.DI_DocInspect)
				{
					base.SetQueueDetails(queueName, ZString.Empty, subStatus, reason, assignedTo);
				}
				else
				{
					base.SetQueueDetails(queueName, status, subStatus, reason, assignedTo);
				}
			}
			else
			{
				base.SetQueueDetails(queueName, status, subStatus, reason, assignedTo);
			}
		}

		public UPECusHAWB UPECusHAWB
		{
			get
			{
				if (fUPECusHAWB == null)
				{
					if (ProcessQueue != null)
					{
						fUPECusHAWB = ProcessQueue.FirstUPECusHAWB;
					}

					if (fUPECusHAWB == null)
					{
						fUPECusHAWB = (UPECusHAWB)Factory.GetNull(typeof(UPECusHAWB));
					}
				}
				return fUPECusHAWB;
			}
		}

		#region IShipmentStatusData Members

		ZString IShipmentStatusData.ShipmentStatus
		{
			get { return (IsResolutionCode) ? "04" : "03"; }
		}

		ZString IShipmentStatusData.HoldReasonCode
		{
			get { return (IsResolutionCode) ? ZString.Empty : Status; }
		}

		ZBool IShipmentStatusData.InspectIndicator
		{
			get
			{
				return UPECusHAWB != null
					&& (UPECusHAWB.CurrentQueue.IsInspectIndicator
							|| (UPECusHAWB.Declaration != null && UPECusHAWB.Declaration.CurrentQueue.IsInspectIndicator))
					&& (UPECusHAWB.CurrentQueue.ResolutionCode.IsEmpty);
			}
		}

		ZBool IShipmentStatusData.AddressCorrectionIndicator
		{
			get { return ToBeSentToOPSScan; }
		}

		ZDateTime IShipmentStatusData.ImportReleaseDate
		{
			get { return SL_EventTime; }
		}

		ZString IShipmentStatusData.Remarks
		{
			get { return Reason; }
		}

		ZString IShipmentStatusData.ExceptionStatusCode
		{
			get { return (IsResolutionCode) || SubStatus.StartsWith("_") ? ZString.Empty : SubStatus; }
		}

		ZString IShipmentStatusData.ExceptionResolutionCode
		{
			get { return (IsResolutionCode) ? Status : ZString.Empty; }
		}

		ShipmentStatusType IShipmentStatusData.ShipmentStatusType
		{
			get
			{
				ShipmentStatusType result = ShipmentStatusType.Unknown;

				if (Queue.IsEmpty)
				{
					result = ShipmentStatusType.Special;
				}
				else
				{
					if (QueueTypeString == ProcessQueueType.Commercial)
					{
						result = ShipmentStatusType.Commercial;
					}
					else if (QueueTypeString == ProcessQueueType.Customs && ProcessQueue != null)
					{
						if (ProcessQueue.P4_ParentTableCode == JobDeclarationSchema.Constants.Prefix)
						{
							result = ShipmentStatusType.Declaration;
						}
						else
						{
							result = ShipmentStatusType.CargoReport;
						}
					}
				}

				return result;
			}
		}

		ZDateTime IShipmentStatusData.StatusChangeDate
		{
			get { return SL_EventTime; }
		}

		bool IShipmentStatusData.HasReasonOrResolutionCode
		{
			get { return !Status.IsEmpty && !Status.StartsWith("_"); }
		}

		bool ToBeSentToOPSScan
		{
			get
			{
				return
					(UPECusHAWB.IsHoldForCollection ||
					UPECusHAWB.DoesStatusIndicateTranshipment ||
					UPECusHAWB.IsAbandoned ||
					UPECusHAWB.IsRTS ||
					UPECusHAWB.IsRedirected ||
					UPECusHAWB.IsCOD);
			}
		}

		bool IsResolutionCode
		{
			get { return ResolutionCodeList.ContainsCode(Status); }
		}

		ResolutionCodeDescriptionPairList ResolutionCodeList
		{
			get
			{
				if (fResolutionCodeList == null)
				{
					fResolutionCodeList = new ResolutionCodeDescriptionPairList();
				}
				return fResolutionCodeList;
			}
		}

		ResolutionCodeDescriptionPairList fResolutionCodeList;

		#region Not Required

		ZString IShipmentStatusData.CustomsRefNo
		{
			get { return ""; }
		}

		ZString IShipmentStatusData.BrokerCode
		{
			get { return ""; }
		}

		#endregion

		#endregion

		#region ILineKey Members

		public ZString ShipmentRef
		{
			get { return (LineKey != null) ? LineKey.ShipmentRef : ZString.Empty; }
		}

		public ZString FlightNo
		{
			get { return (LineKey != null) ? LineKey.FlightNo : ZString.Empty; }
		}

		public ZDateTime ImportDate
		{
			get { return (LineKey != null) ? LineKey.ImportDate : ZDateTime.Empty; }
		}

		public ZString ConsigneePostCode
		{
			get { return (LineKey != null) ? LineKey.ConsigneePostCode : ZString.Empty; }
		}

		#endregion

		#region ProcessQueue
		public UPEProcessQueue ProcessQueue
		{
			get
			{
				if (fProcessQueue == null)
				{
					fProcessQueue = (Master != null) ? (UPEProcessQueue)Master : LoadProcessQueue();
				}
				return fProcessQueue;
			}
		}
		UPEProcessQueue fProcessQueue;
		#endregion

		#region Implementation

		UPEProcessQueue LoadProcessQueue()
		{
			return (UPEProcessQueue)Factory.Load<ProcessQueue>(SL_Parent);
		}

		ILineKey LineKey
		{
			get { return ProcessQueue != null && ProcessQueue.P4_ParentTableCode == JobDeclarationSchema.Constants.Prefix && UPECusHAWB.Declaration != null ? UPECusHAWB.Declaration : UPECusHAWB; }
		}

		protected override void BeforeSuccessfulDelete()
		{
		}

		UPECusHAWB fUPECusHAWB;

		#endregion

	}
}
