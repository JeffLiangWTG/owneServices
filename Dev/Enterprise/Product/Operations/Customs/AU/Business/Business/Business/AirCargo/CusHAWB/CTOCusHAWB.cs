using System.Collections;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[DependentBusinessObject(typeof(CTOCusMAWB), "ChildBills")]
	public class CTOCusHAWB : CusHAWBBase,
		ISendersMessageReferenceProvider,
		ICMRMessageRespondee,
		IUnderbondMovementRequestHeaderProvider,
		IUnderbondDefaultValueProvider,
		IEDIMessageCollectionProvider,
		IDetailsTabPageHeadingProvider,
		IJobInvoicingPlugIn,
		IEDocsProvider,
		Integration.Customs.AU.ICTOCusHAWB,
		IDataExportCSVFileNameProvider
	{
		public CTOCusHAWB(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CTOCusHAWB LoadFromSendersReference(BusinessObjectFactory factory, ZString sendersReference)
		{
			return (CTOCusHAWB)CusHAWBBase.LoadFromQuery(new ZQuery(CusHAWBSchema.CS_MessageReference, sendersReference), factory);
		}

		public static CTOCusHAWB Load(BusinessObjectFactory factory, ICTOCusHAWBInformationProvider info)
		{
			if (info.ArrivalDate.IsEmpty || !info.ArrivalDate.IsValid)
			{
				return null;
			}

			foreach (CTOCusMAWB cTOMAWB in new CusMAWBBase.Loader(factory).FindMatchingCTOMAWBs(info.FlightNumber, info.ArrivalDate))
			{
				foreach (CTOCusHAWB hAWB in cTOMAWB.ChildBills)
				{
					if (hAWB.CS_HAWB == info.MAWB)
					{
						return hAWB;
					}
				}
			}
			return null;
		}

		public new CTOCusMAWB MAWB
		{
			get { return (CTOCusMAWB)base.MAWB; }
		}

		[RelatedBusinessObject(nameof(MAWB))]
		public override ZGuid CS_CM
		{
			get { return base.CS_CM; }
			set { base.CS_CM = value; }
		}

		protected override ZString UnderbondHumanReadableNameCore
		{
			get { return "MasterBill" + (CS_HAWB.IsEmpty ? "" : (" " + CS_HAWB)); }
		}

		protected override ZString DetailsCore
		{
			get { return ZString.Empty; }
		}

		public override void OnSaving()
		{
			DeactivateJobHeaderWhenIsCancelled();
			base.OnSaving();
			PopulateSendersReferenceIfNeeded();
		}

		void DeactivateJobHeaderWhenIsCancelled()
		{
			if (IsCancelled && IsCancelledHasChanged)
			{
				JobHeader.DeactivateAllJobs(this, true);
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!saveSucceeded && !IsInDatabase)
			{
				CS_MessageReference = ZString.Empty;
			}
		}

		#region IUnderbondDefaultValueProvider Members

		void IUnderbondDefaultValueProvider.SetUnderbondDefaultValues(CusUnderbond underbond)
		{
			underbond.C4_FlightNo = MAWB.CM_FlightNo;
			underbond.C4_PiecesManifested = CS_PiecesManifested;
		}

		#endregion

		#region IUnderbondMovementRequestHeaderProvider Members

		IUnderbondMovementRequestHeader IUnderbondMovementRequestHeaderProvider.GetHeader(CusUnderbond underbond)
		{
			return new CTOCusHAWBUnderbondMovementRequestHeader(underbond, this);
		}

		public bool IsBureau
		{
			get { return false; }
		}

		#endregion

		#region GetAllPossibleCollectionProviders

		protected override ICusUnderbondDependentCollectionParent[] GetAllPossibleCollectionProvidersCore()
		{
			ArrayList result = new ArrayList();
			result.Add(this);
			result.AddRange(PartShips);
			return (ICusUnderbondDependentCollectionParent[])result.ToArray(typeof(ICusUnderbondDependentCollectionParent));
		}

		#endregion

		protected override Customs.Business.CusHAWBValidation GetNewValidation()
		{
			return new CTOCusHAWBValidation(this);
		}

		#region ISendersMessageReferenceProvider Members

		public ZString SendersReference
		{
			get { return CS_MessageReference; }
		}

		public void PopulateSendersReferenceIfNeeded()
		{
			if (!IsInDatabase)
			{
				PopulateNumberPropertyIfRequired(CS_MessageReferenceInfo, GetNewMessageReference);
			}
			else if (CS_MessageReference.IsEmpty)
			{
				CS_MessageReference = GetNewMessageReference(Factory);
			}
		}

		ZString GetNewMessageReference(BusinessObjectFactory factory)
		{
			return Env.NumberFountains.CTOCusHAWBNumber.GetNextFormatted(factory);
		}

		#endregion

		#region ICMRMessageRespondee Members

		public ZString Details
		{
			get
			{
				return "Flight: " + MAWB.CM_FlightNo + "\r\n" +
					"ETA: " + MAWB.CM_ArrivalDate.ToString() + "\r\n" +
					"MAWB: " + CS_HAWB + "\r\n";
			}
		}

		#endregion

		string IDetailsTabPageHeadingProvider.Heading
		{
			get { return UnderbondHumanReadableNameCore; }
		}

		#region IJobNumber

		string IJobNumber.JobNumber
		{
			get { return "N" + CS_MessageReference; }
		}

		#endregion

		#region IJobHeaderParent Members

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
			PopulateSendersReferenceIfNeeded();
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

		CTOCusHAWBInvoicingSupporter fInvoicingSupporter;
		public IJobInvoicingSupporter InvoicingSupporter
		{
			get { return fInvoicingSupporter ?? (fInvoicingSupporter = new CTOCusHAWBInvoicingSupporter(this)); }
		}

		#endregion

		#region IEDocsProvider Members

		public EDocsProviderSupporter GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get { return new DocManagerInfo(this, Core.Constants.DocManagerCodes.AirCargoHouse); }
		}

		#endregion

		#region IDataExportCSVFileNameProvider Members

		ZString IDataExportCSVFileNameProvider.FileNameSuffix
		{
			get { return CS_HAWB; }
		}

		#endregion
	}
}
