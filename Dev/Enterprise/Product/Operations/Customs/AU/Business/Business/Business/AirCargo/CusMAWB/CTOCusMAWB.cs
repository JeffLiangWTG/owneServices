using System.Collections;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[CodeProperty(CTOCusMAWB.Schema.CM_MAWB), DescriptionProperty(CTOCusMAWB.Schema.CM_Description)]
	public class CTOCusMAWB :
		CusMAWBBase,
		ICusUnderbondDependentCollectionParent,
		IAirOutturnReportHeaderInformationProvider,
		IUnderbondDefaultValueProvider,
		IJobInvoicingPlugIn,
		IDocumentSupportable,
		IEDocsProvider,
		Customs.Business.IMessageManageableBizObj,
		Integration.Customs.AU.ICTOCusMAWB,
		IDataExportCSVFileNameProvider,
		IDocManagerSupportIncudingRelatedObjects
	{
		public CTOCusMAWB(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override void OnLoaded()
		{
			if (!CM_IsCTOMAWB)
			{
				ErrorReporter.ReportOnce("Attempting to load a CTOCusMAWB from a record that is not flagged CM_IsCTOMAWB", "Attempting to load a CTOCusMAWB from a record that is not flagged CM_IsCTOMAWB"); // Column names are in a string, which is okay
			}
			base.OnLoaded();
		}

		public new class Schema : CusMAWBBase.Schema
		{
			public const string CM_Description = "CM_Description";
		}

		public override ZString CM_RL_NKDischargePort
		{
			get { return base.CM_RL_NKDischargePort; }
			set
			{
				bool isDiff = base.CM_RL_NKDischargePort != value;
				base.CM_RL_NKDischargePort = value;
				if (isDiff)
				{
					ChildBills.MarkAsNeedingValidation();
				}
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CM_IsCTOMAWB = true;
		}

		public ZString CM_Description
		{
			get { return CM_FlightNo + " : " + CM_ArrivalDate.ToShortDateString(); }
		}

		public override ZDateTime CM_ArrivalDate
		{
			get { return base.CM_ArrivalDate; }
			set
			{
				bool hasChanges = value != CM_ArrivalDate;
				base.CM_ArrivalDate = value;
				if (hasChanges)
				{
					Underbonds.MarkAsNeedingValidation();
					foreach (CusUnderbond underbond in Underbonds)
					{
						underbond.Outturns.MarkAsNeedingValidation();
					}
				}
			}
		}

		#region Saving

		public override void OnSaving()
		{
			DeactivateJobHeaderWhenIsCancelled();
			base.OnSaving();
		}

		void DeactivateJobHeaderWhenIsCancelled()
		{
			if (!this.HasContext(BusinessContext.InvoicingPlugInGUI) && IsCancelled && IsCancelledHasChanged)
			{
				JobHeader.DeactivateAllJobs(this, true);
			}
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerIncludingRelatedObjectsInfo(this, Core.Constants.DocManagerCodes.AirCTO);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region Related objects

		[ChildEditable(true)]
		public new CTOCusHAWBCollection ChildBills
		{
			get { return (CTOCusHAWBCollection)base.ChildBills; }
		}

		protected override Customs.Business.CusHAWBDependentCollection GetNewChildBillsCollection()
		{
			return new CTOCusHAWBCollection(this);
		}

		protected override void LoadNewChildBillsCollection(Customs.Business.CusHAWBDependentCollection childBills)
		{
			((CTOCusHAWBCollection)childBills).HookCollectionChanged(AllChildBills);
			base.LoadNewChildBillsCollection(childBills);
		}

		CTOCusHAWBAndPartShipCollection fAllChildBills;
		public CTOCusHAWBAndPartShipCollection AllChildBills
		{
			get
			{
				if (fAllChildBills == null)
				{
					fAllChildBills = new CTOCusHAWBAndPartShipCollection(this);
					fAllChildBills.HookCollectionChanged(ChildBills);

					fAllChildBills.Load();
				}
				return fAllChildBills;
			}
		}

		CusUnderbond fakeFlightOuturnUnderbond;
		public CusUnderbond FakeFlightOuturnUnderbond
		{
			get
			{
				if (fakeFlightOuturnUnderbond == null)
				{
					ZQuery underbondFilter = new ZQuery(Enterprise.ZArchitecture.Schema.CusUnderbondSchema.C4_ParentID, PK);
					fakeFlightOuturnUnderbond = Factory.LoadTop1<CusUnderbond>(underbondFilter);
					if (fakeFlightOuturnUnderbond == null)
					{
						fakeFlightOuturnUnderbond = Factory.New<CusUnderbond>();
						using (fakeFlightOuturnUnderbond.SuspendSettingHasChanges())
						{
							fakeFlightOuturnUnderbond.LinkedObject = this;
						}
					}
					RegisterEditableChildObject(fakeFlightOuturnUnderbond);
				}
				return fakeFlightOuturnUnderbond;
			}
		}

		#endregion

		#region GetAllPossibleCollectionProvidersCore Override

		protected override ICusUnderbondDependentCollectionParent[] GetAllPossibleCollectionProvidersCore()
		{
			ArrayList result = new ArrayList();
			result.AddRange(ChildBills);
			foreach (CTOCusHAWB hAWB in ChildBills)
			{
				foreach (CusPartShip partShip in hAWB.PartShips)
				{
					result.Add(partShip);
				}
			}
			return (ICusUnderbondDependentCollectionParent[])result.ToArray(typeof(ICusUnderbondDependentCollectionParent));
		}

		#endregion

		#region Implementation

		protected override bool IsStandAloneCore
		{
			get { return true; }
		}

		public override void Delete()
		{
			ChildBills.RemoveAndDeleteAll();
			base.Delete();
		}

		#endregion

		#region ICusUnderbondDependentCollectionParent Members

		public IOutturnableLine[] OutturnableLines
		{
			get
			{
				return (IOutturnableLine[])AllChildBills.ToArray(typeof(IOutturnableLine));
			}
		}

		Customs.Business.CusUnderbondCollection ICusUnderbondDependentCollectionParent.Underbonds => Underbonds;
		public CusUnderbondCollection Underbonds
		{
			get
			{
				if (fUnderbonds == null)
				{
					fUnderbonds = new CusUnderbondCollection(this);
					fUnderbonds.Load();
					fUnderbonds.IsManagedForDataRefresh = true;
				}
				return fUnderbonds;
			}
		}
		CusUnderbondCollection fUnderbonds;

		ZString IOutturnableLine.CargoStatus
		{
			get { return ZString.Empty; }
		}

		ZInt IOutturnableLine.PackagesManifested
		{
			get { return 0; }
		}

		bool ICusUnderbondDependentCollectionParent.CanSendWithoutDelay
		{
			get { return true; }
		}

		public ZString UnderbondHumanReadableName
		{
			get { return ZString.Empty; }
		}

		public ZString Details
		{
			get { return ZString.Empty; }
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

		#region IAirOutturnReportHeaderInformationProvider Members

		IAirOutturnReportHeaderInformation IAirOutturnReportHeaderInformationProvider.GetHeader(CusUnderbond underbond)
		{
			return new CTOCusMAWBOutturnReportHeaderInformation(this, underbond);
		}

		protected override Customs.Business.CusMAWBValidation GetNewValidation()
		{
			return new CTOCusMAWBValidation(this);
		}

		#endregion

		#region IUnderbondDefaultValueProvider Members

		void IUnderbondDefaultValueProvider.SetUnderbondDefaultValues(CusUnderbond underbond)
		{
			underbond.C4_FlightNo = CM_FlightNo;
			ZShort packageCount = 0;
			foreach (CTOCusHAWB hAWB in ChildBills)
			{
				if ((int)packageCount + (int)hAWB.CS_PiecesManifested > short.MaxValue)
				{
					underbond.C4_PiecesManifested = 0;
					return;
				}
				packageCount += hAWB.CS_PiecesManifested;
			}
			underbond.C4_PiecesManifested = packageCount;
		}

		#endregion

		#region IJobNumber

		string JobNumber => GetFormattedJobNumber(CM_FlightNo, CM_RL_NKFirstArrivalPort, CM_ArrivalDate);

		internal string GetFormattedJobNumber(ZString flightNo, ZString firstArrivalPort, ZDateTime arrivalDate)
			=> "G" + (firstArrivalPort.IsEmpty ? flightNo : flightNo.SubstringSafe(0, 8)) + arrivalDate.ToString("yyMMdd") + firstArrivalPort;

		string IJobNumber.JobNumber
		{
			get { return JobNumber; }
		}

		#endregion

		#region IJobHeaderParent Members

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
		}

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobCreated(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
		}

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return true; }
		}

		#endregion

		#region IJobInvoicingPlugIn Members

		CTOCusMAWBInvoicingSupporter fInvoicingSupporter;
		public IJobInvoicingSupporter InvoicingSupporter
		{
			get { return fInvoicingSupporter ?? (fInvoicingSupporter = new CTOCusMAWBInvoicingSupporter(this)); }
		}

		#endregion

		#region IDocumentSupportable Members

		public new DocumentSupporter DocumentSupporter
		{
			get { return new CTOCusMAWBDocumentSupporter(this); }
		}

		#endregion

		#region IEDocsProvider Members

		public EDocsProviderSupporter GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region IMessageManageableBizObj Members

		Customs.Business.IMessageManager Customs.Business.IMessageManageableBizObj.GetMessageManagerForAmendmentDetection()
		{
			return new CTOCusMAWBMessageManager(delegate
			{ return this; });
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

		#region IDataExportCSVFileNameProvider Members

		ZString IDataExportCSVFileNameProvider.FileNameSuffix
		{
			get { return CM_MAWB; }
		}

		#endregion

		#region IDocManagerSupportIncudingRelatedObjects Members

		BusinessObject IDocManagerSupportIncudingRelatedObjects.SelfReference
		{
			get { return this; }
		}

		System.Collections.Generic.IEnumerable<BusinessObject> IDocManagerSupportIncudingRelatedObjects.GetRelatedBusinessObjects()
		{
			foreach (var underbond in AllUnderbonds)
			{
				yield return underbond;
			}
		}

		#endregion
	}
}
