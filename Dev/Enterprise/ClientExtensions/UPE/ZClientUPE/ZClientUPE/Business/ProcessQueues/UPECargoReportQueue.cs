using System;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	/* todo - hide base meaningless field names like P4_CustomDate2 and interface them to meaningfull client specific fields that indicate the actual use 
	 *        of the dam field like say perhaps BisiDownloadDate!!!
	 */

	public partial class UPECargoReportQueue : UPEProcessQueue
	{
		public UPECargoReportQueue(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override UPECusHAWB FirstUPECusHAWB
		{
			get { return (UPECusHAWB)ParentBusinessObject; }
		}

		public override GlbBranch ParentBranch => FirstUPECusHAWB?.MAWB?.Branch;

		#region GetFinalisedCusHAWBs

		public override UPECusHAWB[] GetFinalisedCusHAWBs()
		{
			UPECusHAWB[] result;

			if (FirstUPECusHAWB != null)
			{
				if (AllQueuesCompleted)
				{
					result = new UPECusHAWB[] { FirstUPECusHAWB };
				}
				else
				{
					result = Array.Empty<UPECusHAWB>();
				}
			}
			else
			{
				result = Array.Empty<UPECusHAWB>();
			}

			return result;
		}

		public bool AllQueuesCompleted
		{
			get { return IsCustomsQueueCompleted && IsCommercialQueueCompleted && IsDeclarationQueueCompleted; }
		}

		bool IsDeclarationQueueCompleted
		{
			get { return (FirstUPECusHAWB.Declaration == null && !FirstUPECusHAWB.IsFormalDecRequired) || (FirstUPECusHAWB.Declaration != null && FirstUPECusHAWB.Declaration.CurrentQueue.IsCustomsQueueCompleted); }
		}

		#endregion

		#region Resolution Code

		public ZString ResolutionCode
		{
			get { return (LastLogWithResolutionCode != null) ? LastLogWithResolutionCode.Status : ZString.Empty; }
		}

		public ZString Resolution
		{
			get { return ResolutionList.GetDescriptionFromCode(ResolutionCode); }
		}

		public ZPropertyInfo ResolutionInfo
		{
			get { return GetZPropertyInfo(nameof(Resolution)); }
		}

		public ZDateTime ResolutionUploadDateTime
		{
			get { return (LastLogWithResolutionCode != null) ? LastLogWithResolutionCode.SL_EventTime : ZDateTime.Empty; }
		}

		public ZPropertyInfo ResolutionUploadDateTimeInfo
		{
			get { return GetZPropertyInfo(nameof(ResolutionUploadDateTime)); }
		}

		public void AddResolutionCodeLog()
		{
			if (P4_CustomsStatus != CMRConsolidatedCargoStatuses.Codes.WithdrawnCargoReportHadBeenWithdrawn
			 || (FirstUPECusHAWB.Messages.LastIncomingMessage == null || FirstUPECusHAWB.Messages.LastIncomingMessage.EM_MessageType != EdiMessageTypes.Withdrawn))
			{
				ZString resolutionCodeToUpload = GetResolutionCodeToUpload();

				lastLogWithResolutionCode = (IsCustomsQueueLogsLoaded)
					 ? AddResolutionCodeLogToCollection(resolutionCodeToUpload)
					 : AddResolutionCodeLogDirectlyToFactory(resolutionCodeToUpload);
			}
		}

		public ProcessQueueLog LastLogWithResolutionCode
		{
			get { return lastLogWithResolutionCode ?? (lastLogWithResolutionCode = (IsCustomsQueueLogsLoaded) ? GetLastResolutionCodeLogFromCollection() : GetLastResolutionCodeLogFromDatabase()); }
		}
		ProcessQueueLog lastLogWithResolutionCode;

		ResolutionCodeDescriptionPairList ResolutionList
		{
			get { return resolutionList ?? (resolutionList = new ResolutionCodeDescriptionPairList()); }
		}
		ResolutionCodeDescriptionPairList resolutionList;

		ZQuery ResolutionCodeFilter
		{
			get
			{
				if (resolutionCodeFilter == null)
				{
					resolutionCodeFilter = new ZQuery();
					resolutionCodeFilter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.QueueChanged.Code);
					resolutionCodeFilter.AddToFilter(StmALogSchema.SL_Parent, PK);
					resolutionCodeFilter.AddToFilter(EncodedResolutionCodeReferenceFilter);
					resolutionCodeFilter.OrderBy = StmALogSchema.SL_EventTime.Name + " DESC";
				}
				return resolutionCodeFilter;
			}
		}
		ZQuery resolutionCodeFilter;

		ZQuery EncodedResolutionCodeReferenceFilter
		{
			get
			{
				if (encodedResolutionCodeReferenceFilter == null)
				{
					encodedResolutionCodeReferenceFilter = new ZQuery(StmALogSchema.SL_Reference, Array.ConvertAll(ResolutionList.ToArray(),
						resolution => ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Customs, string.Empty, resolution.Code, string.Empty, string.Empty, string.Empty)));
				}
				return encodedResolutionCodeReferenceFilter;
			}
		}
		ZQuery encodedResolutionCodeReferenceFilter;

		bool IsToBeAbandoned
		{
			get { return P4_QueueName == CommercialQueueCodeDescriptionPairList.Codes.Rebill && P4_Status == ReasonCodeDescriptionPairList.Codes._R1_Abandon; }
		}

		bool IsInCustomsBonding
		{
			get { return FirstUPECusHAWB != null && FirstUPECusHAWB.Declaration != null && FirstUPECusHAWB.Declaration.IsInCustomsBondingQueue; }
		}

		bool HasAlternateBroker
		{
			get { return FirstUPECusHAWB != null && FirstUPECusHAWB.Declaration != null && FirstUPECusHAWB.Declaration.HasAlternateBroker; }
		}

		ProcessQueueLog GetLastResolutionCodeLogFromCollection()
		{
			CustomsQueueLogs.Sort(StmALogSchema.SL_EventTime.Name, ListSortDirection.Descending);
			foreach (ProcessQueueLog log in CustomsQueueLogs)
			{
				bool areLogPropertiesEmpty = log.Queue.IsEmpty && log.SubStatus.IsEmpty && log.Reason.IsEmpty && log.AssignedTo.IsEmpty;
				if (areLogPropertiesEmpty && ResolutionList.ContainsCode((string)log.Status))
				{
					return log;
				}
			}
			return null;
		}

		ProcessQueueLog GetLastResolutionCodeLogFromDatabase()
		{
			return Factory.LoadTop1<UPEProcessQueueLog>(ResolutionCodeFilter);
		}

		ProcessQueueLog AddResolutionCodeLogToCollection(ZString resolutionCode)
		{
			return CustomsQueueLogs.AddNew(string.Empty, resolutionCode, string.Empty, string.Empty, string.Empty);
		}

		ProcessQueueLog AddResolutionCodeLogDirectlyToFactory(ZString resolutionCode)
		{
			ProcessQueueLog result = Factory.New<ProcessQueueLog>();
			result.SL_Table = ProcessQueueSchema.Constants.TableName;
			result.SL_Parent = PK;
			result.SetQueueDetails(ProcessQueueType.Enum.Customs, string.Empty, resolutionCode, string.Empty, string.Empty, string.Empty);
			return result;
		}

		ZString GetResolutionCodeToUpload()
		{
			ZString result;
			if (IsToBeAbandoned)
			{
				result = ResolutionCodeDescriptionPairList.Codes.BZ_Abandoned;
			}
			else if (IsInCustomsBonding)
			{
				result = ResolutionCodeDescriptionPairList.Codes.B7_SeizedByCustoms;
			}
			else if (HasAlternateBroker)
			{
				result = ResolutionCodeDescriptionPairList.Codes.BU_ReleasedByAlternateBroker;
			}
			else
			{
				result = ResolutionCodeDescriptionPairList.Codes.DA_Released;
			}
			return result;
		}

		public static class EdiMessageTypes
		{
			public const string Withdrawn = "WDR";
		}

		internal void ResetResolutionCodeForTest()
		{
			lastLogWithResolutionCode = null;
		}

		#endregion

		#region Overrides

		protected override string UPEProcessQueueType
		{
			get { return this.GetType().Name; }
		}

		public override ZString P4_Status
		{
			get { return base.P4_Status; }
			set
			{
				base.P4_Status = value;
				if (P4_Status == ReasonCodeDescriptionPairList.Codes._R3_RTS && (P4_QueueName == CommercialQueueCodeDescriptionPairList.Codes.Rebill || P4_QueueName == CommercialQueueCodeDescriptionPairList.Codes.Completed))
				{
					P4_CustomDecimal1 = (ZDecimal)4;
				}
				else if (P4_Status == ReasonCodeDescriptionPairList.Codes._R1_Abandon && (P4_QueueName == CommercialQueueCodeDescriptionPairList.Codes.Rebill || P4_QueueName == CommercialQueueCodeDescriptionPairList.Codes.Completed))
				{
					P4_CustomDecimal1 = (ZDecimal)3;
				}
				else
				{
					P4_CustomDecimal1 = (ZDecimal)0;
				}
			}
		}

		public override ZString P4_CustomAttrib4
		{
			get { return base.P4_CustomAttrib4; }
			set
			{
				bool isDiff = base.P4_CustomAttrib4 != value;
				base.P4_CustomAttrib4 = value;
				if (isDiff && FirstUPECusHAWB != null)
				{
					FirstUPECusHAWB.MarkAsNeedingValidation();
				}
			}
		}

		public override ZPropertyInfo P4_GS_NKTaskAssignedToInfo
		{
			get
			{
				ZPropertyInfo result = base.P4_GS_NKTaskAssignedToInfo;
				((IZPropertyInfoObsolete)result).ReadOnly = true;
				return result;
			}
		}

		public override ZPropertyInfo P4_GS_NKCustomsTaskAssignedToInfo
		{
			get
			{
				ZPropertyInfo result = base.P4_GS_NKCustomsTaskAssignedToInfo;
				((IZPropertyInfoObsolete)result).ReadOnly = true;
				return result;
			}
		}

		protected override Type ParentBusinessObjectType
		{
			get { return typeof(UPECusHAWB); }
		}

		public new UPECargoReportQueueLookups Lookups
		{
			get { return (UPECargoReportQueueLookups)base.Lookups; }
		}

		protected override ProcessQueueLookups GetNewLookups()
		{
			return new UPECargoReportQueueLookups(this);
		}

		protected override ProcessQueueValidation GetNewValidation()
		{
			return new UPECargoReportQueueValidation(this);
		}

		#endregion

		public override string ReferenceCode
		{
			get { return referenceCode; }
		}
		const string referenceCode = "COM";
	}
}
