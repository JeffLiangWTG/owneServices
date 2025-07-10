using System.Collections;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[CodeProperty(CusSeaManTranHead.Schema.BT_VoyageNum)]
	public class CusSeaManTranHead : Customs.Business.CusSeaManTranHead
		, IAUCusUnderbondUnionCollectionParent
		, ICMRMessageRespondee
		, Customs.Business.IMessageManageableBizObj
		, Integration.Customs.AU.ICusSeaManTranHead
		, IDataExportCSVFileNameProvider
		, IDocManagerSupportIncudingRelatedObjects
	{
		public CusSeaManTranHead(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			Calculator = new CusSeaManTranHeadStatusCalculator(this);
		}

		public readonly CusSeaManTranHeadStatusCalculator Calculator;

		#region Static

		public static CusSeaManTranHead LoadByVoyageNumberVessel(BusinessObjectFactory factory, ZString voyageNumber, ZString vesselName)
		{
			ZQuery filter = new ZQuery(CusSeaManTranHeadSchema.BT_VoyageNum, voyageNumber);
			filter.AddToFilter(CusSeaManTranHeadSchema.BT_VesselName, vesselName);
			return factory.LoadTop1<CusSeaManTranHead>(filter);
		}

		#endregion

		#region Schema

		public new abstract class Schema : Customs.Business.CusSeaManTranHead.Schema
		{
			public const string ImpendingArrivalResponseStatus = "ImpendingArrivalResponseStatus";
		}

		#endregion

		#region Overrides

		#region OceanBills

		[ChildEditable(true)]
		public new CusSeaManOBLHeaderCollection OceanBills
		{
			get { return (CusSeaManOBLHeaderCollection)base.OceanBills; }
		}

		protected override Customs.Business.CusSeaManOBLHeaderCollection GetNewOceanBills()
		{
			return new CusSeaManOBLHeaderCollection(this);
		}

		public new CusSeaManOBLHeaderCollectionView OceanBillsView
		{
			get { return (CusSeaManOBLHeaderCollectionView)base.OceanBillsView; }
		}

		protected override Customs.Business.CusSeaManOBLHeaderCollectionView GetNewOceanBillsView()
		{
			return new CusSeaManOBLHeaderCollectionView(OceanBills);
		}

		#endregion

		#region ArrivalPorts

		[ChildEditable(true)]
		public new CusSeaManArrivalPortCollection Arrivals
		{
			get { return (CusSeaManArrivalPortCollection)base.Arrivals; }
		}

		protected override Customs.Business.CusSeaManArrivalPortCollection GetNewArrivals()
		{
			return new CusSeaManArrivalPortCollection(this);
		}

		#endregion

		#region SlotCharterers

		[ChildEditable(true)]
		public new CusSeaManSlotOrgCollection SlotCharterers
		{
			get { return (CusSeaManSlotOrgCollection)base.SlotCharterers; }
		}

		protected override Customs.Business.CusSeaManSlotOrgCollection GetNewSlotCharterers()
		{
			return new CusSeaManSlotOrgCollection(this);
		}

		#endregion

		#region Lookups

		protected override Customs.Business.CusSeaManTranHeadLookups GetNewLookups()
		{
			return new CusSeaManTranHeadLookups(this);
		}

		public new CusSeaManTranHeadLookups Lookups
		{
			get { return (CusSeaManTranHeadLookups)base.Lookups; }
		}

		#endregion

		#region Validation

		protected override Customs.Business.CusSeaManTranHeadValidation GetNewValidation()
		{
			return new CusSeaManTranHeadValidation(this);
		}

		#endregion

		#region Vessel / Lloyds

		public override ZString BT_VesselName
		{
			get
			{
				return base.BT_VesselName;
			}
			set
			{
				bool hasChanges = base.BT_VesselName != value;
				base.BT_VesselName = value;
				if (hasChanges && !IsCopying)
				{
					base.BT_LloydsIMO = ZString.Empty;
					if (!base.BT_VesselName.IsEmpty && Vessel != null)
					{
						DefaultLloydsIMO();
					}
				}
			}
		}

		public override ZString BT_LloydsIMO
		{
			get { return base.BT_LloydsIMO; }
			set
			{
				bool hasChanges = base.BT_LloydsIMO != value;
				base.BT_LloydsIMO = value;
				if (hasChanges && !IsCopying && !BT_LloydsIMO.IsEmpty)
				{
					var vessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, value));
					if (vessel != null && vessel.RV_Code != BT_VesselName)
					{
						base.BT_VesselName = vessel.RV_Code;
					}
				}
			}
		}

		void DefaultLloydsIMO()
		{
			var lloydsNumber = Vessel?.RV_LloydsNumber ?? ZString.Empty;
			if (!lloydsNumber.IsEmpty && lloydsNumber != BT_LloydsIMO)
			{
				BT_LloydsIMO = lloydsNumber;
				Validation.ValidateBT_VesselName();
			}
		}

		#region Vessel

		public RefVessel Vessel => Vessels.Length == 1 ? Vessels[0] : null;

		public bool VesselHasDuplicates => Vessels.Length > 1;

		RefVessel[] Vessels => Factory.GetValue(ref fVesselsCached, () => LoadVesselsCore());
		CachedProperty<RefVessel[]> fVesselsCached;

		protected virtual RefVessel[] LoadVesselsCore()
		{
			var vesselQuery = new ZQuery(RefVesselSchema.RV_Code, BT_VesselName);
			vesselQuery.IgnoreActiveFilter = true;
			if (!BT_LloydsIMO.IsEmpty)
			{
				vesselQuery.AddToFilter(RefVesselSchema.RV_LloydsNumber, BT_LloydsIMO);
			}

			return Factory.Load<RefVessel>(vesselQuery);
		}

		#endregion

		#endregion

		#region BT_RL_NKPortOfLastForeignPort

		[List(nameof(Lookups) + "." + nameof(CusSeaManTranHeadLookups.PortOfLastForeignPorts))]
		public override ZString BT_RL_NKPortOfLastForeignPort { get => base.BT_RL_NKPortOfLastForeignPort; set => base.BT_RL_NKPortOfLastForeignPort = value; }

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			if (!GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number.IsEmpty)
			{
				BT_ResponsiblePartyID = GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number.Left(BT_ResponsiblePartyIDInfo.MaxLength);
				BT_PrincipalID = GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number.Left(BT_PrincipalIDInfo.MaxLength);
			}
		}

		protected override List<BusinessObject> GetBusinessObjectsWithRelatedLogsCore()
		{
			List<BusinessObject> result = base.GetBusinessObjectsWithRelatedLogsCore();

			foreach (CusSeaManArrivalPort port in Arrivals)
			{
				result.AddRange(port.CargoLines);
			}

			result.AddRange(AllUnderbonds);

			return result;
		}

		#endregion

		#region Properties

		ZString ICMRMessageRespondee.Details
		{
			get
			{
				ZString result = ZString.Empty;
				RefVessel vessel = Vessel;
				result += "Vessel: " + (vessel != null ? vessel.RV_Code : ZString.Empty) + "\r\n";
				result += "Voyage: " + BT_VoyageNum + "\r\n";
				return result;
			}
		}

		ZString ICMRMessageRespondee.ShortDescription
		{
			get { return "Voyage: " + BT_VoyageNum; }
		}

		#region ImpendingArrivalResponseStatus

		public CusEntryNumStatus ImpendingArrivalResponseStatus
		{
			get
			{
				if (fImpendingArrivalResponseStatus == null)
				{
					fImpendingArrivalResponseStatus = new CusEntryNumStatus(this, new CMRBaseStatuses(), CMRBaseStatuses.Codes.NotSent, CusEntryNumber.EntryType.ImpendingArrivalResponseStatus, Core.Constants.CountryCodes.Australia);
				}

				return fImpendingArrivalResponseStatus;
			}
		}

		CusEntryNumStatus fImpendingArrivalResponseStatus;

		#endregion

		public override ZString BT_ResponsiblePartyID
		{
			get
			{
				return base.BT_ResponsiblePartyID;
			}
			set
			{
				base.BT_ResponsiblePartyID = value.Replace(" ", "");
			}
		}

		public override ZString BT_PrincipalID
		{
			get
			{
				return base.BT_PrincipalID;
			}
			set
			{
				base.BT_PrincipalID = value.Replace(" ", "");
			}
		}

		public ZDateTime DateTimeOfDepartureUTC
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (PortOfLastForeignPort != null && !BT_PortOfLastForeignPortATD.IsEmpty && BT_PortOfLastForeignPortATD.IsValid)
				{
					result = PortOfLastForeignPort.TimeZoneSet.GetCalculationTimeZone().ToUniversalTime(BT_PortOfLastForeignPortATD.ToDateTime());
				}
				return result;
			}
		}

		#endregion

		#region ICusUnderbondUnionCollectionParent Members

		Customs.Business.CusUnderbondUnionCollection ICusUnderbondUnionCollectionParent.AllUnderbonds => AllUnderbonds;

		public CusUnderbondUnionCollection AllUnderbonds
		{
			get
			{
				if (allUnderbonds == null)
				{
					allUnderbonds = new CusUnderbondUnionCollection(this);
					allUnderbonds.Load();
				}
				return allUnderbonds;
			}
		}
		CusUnderbondUnionCollection allUnderbonds;

		ICusUnderbondDependentCollectionParent[] ICusUnderbondUnionCollectionParent.GetAllPossibleCollectionProviders()
		{
			ArrayList result = new ArrayList();
			foreach (CusSeaManOBLHeader header in OceanBills)
			{
				result.AddRange(header.Details);
			}
			return (ICusUnderbondDependentCollectionParent[])result.ToArray(typeof(ICusUnderbondDependentCollectionParent));
		}

		bool ICusUnderbondUnionCollectionParent.IsForAirCargo
		{
			get { return false; }
		}

		#endregion

		#region IMessageManageableBizObj Members

		Customs.Business.IMessageManager Customs.Business.IMessageManageableBizObj.GetMessageManagerForAmendmentDetection()
		{
			return new CusSeaManTranHeadMessageManager(this);
		}

		bool Customs.Business.IMessageManageableBizObj.IsInAStatusAmendmentSendable
		{
			get { return true; }
		}

		Customs.Business.ContinueWithDetection Customs.Business.IMessageManageableBizObj.ProcessBeforeDetectingAmendmentAndContinue()
		{
			return Enterprise.Customs.Business.ContinueWithDetection.Yes;
		}

		#endregion

		#region IDataExportCSVFileNameProvider Members

		ZString IDataExportCSVFileNameProvider.FileNameSuffix
		{
			get
			{
				var builder = new ZStringBuilder();
				builder.Append(BT_VesselName);
				builder.Append(BT_VoyageNum);
				builder.Append(BT_RL_NKPortOfLastForeignPort);
				var arvTime = BT_PortOfLastForeignPortATD;
				builder.Append(arvTime.IsValid ? (ZString)arvTime.ToString("yyMMdd") : ZString.Empty);
				return builder.ToStringWithDelimiterBetweenAppends("_");
			}
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return fDocManagerInfo ?? (fDocManagerInfo = new DocManagerIncludingRelatedObjectsInfo(this, Core.Constants.DocManagerCodes.VoyageManifest)); }
		}
		DocManagerInfo fDocManagerInfo;

		#endregion

		#region IDocManagerSupportIncudingRelatedObjects Members

		BusinessObject IDocManagerSupportIncudingRelatedObjects.SelfReference
		{
			get { return this; }
		}

		IEnumerable<BusinessObject> IDocManagerSupportIncudingRelatedObjects.GetRelatedBusinessObjects()
		{
			foreach (var underbond in AllUnderbonds)
			{
				yield return underbond;
			}
		}

		#endregion
	}
}
