using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers
{
	public class DocSailing : DocBaseWrapper
	{
		DocSailing(JobSailing jobSailing, BusinessObjectFactory factoryToWrap)
			: base(jobSailing, factoryToWrap) { }

		public static DocSailing New(JobSailing jobSailing, BusinessObjectFactory factoryToWrap)
		{
			if (jobSailing == null)
			{
				return null;
			}
			else
			{
				return new DocSailing(jobSailing, factoryToWrap);
			}
		}

		JobSailing JobSailing
		{
			get { return (JobSailing)WrappedObject; }
		}

		public override string ToString()
		{
			return "";
		}
		public ZDecimal TotalWeight
		{
			get { return JobSailing.TotalWeight; }
		}

		public ZString TotalWeightUnit
		{
			get { return JobSailing.TotalWeightUnit; }
		}

		public ZDecimal TotalVolume
		{
			get { return JobSailing.TotalVolume; }
		}

		public ZString TotalVolumeUnit
		{
			get { return JobSailing.TotalVolumeUnit; }
		}

		public ZString VoyageFlight
		{
			get { return JobSailing.JX_JV_VoyageFlight; }
		}
		public DocVessel Vessel
		{
			get { return DocVessel.New(JobSailing.Vessel, Factory); }
		}

		public DocVoyage Voyage
		{
			get { return DocVoyage.New(JobSailing.Voyage, Factory); }
		}

		public ZString VoyageNo
		{
			get { return (Voyage != null) ? Voyage.VoyageFlight : ZString.Empty; }
		}

		public ZDateTime LCLCutOff
		{
			get { return JobSailing.JX_DepotCutOff; }
		}

		public ZDateTime LCLReceivalCommences
		{
			get { return JobSailing.JX_DepotReceivalCommences; }
		}

		public ZDateTime FCLCutOff
		{
			get { return JobSailing.JX_JA_CTOCutOff; }
		}

		public ZDateTime FCLReceivalCommences
		{
			get { return JobSailing.JX_JA_CTOReceivalCommences; }
		}

		public ZDateTime HazardousCutOff
		{
			get { return JobSailing.JX_JA_DGFCLCutOff; }
		}

		public ZDateTime HazardousReceivalCommences
		{
			get { return JobSailing.JX_JA_DGFCLReceivalCommences; }
		}

		public ZString ReservedMasterBill
		{
			get { return JobSailing.JX_ReservedMasterBill; }
		}

		public ZDateTime LCLAvailabilityDate
		{
			get { return JobSailing.JX_DepotAvailabilityDate; }
		}

		public ZDateTime LCLStorageDate
		{
			get { return JobSailing.JX_DepotStorageDate; }
		}

		public ZDateTime DocsCutOff
		{
			get { return JobSailing.JX_JA_DocumentaryCutoff; }
		}

		public ZDateTime AvailabilityDate
		{
			get { return JobSailing.JX_JB_CTOAvailabilityDate; }
		}

		public ZDateTime StorageDate
		{
			get { return JobSailing.JX_JB_CTOStorageDate; }
		}

		#region Departure Fields

		public DocVoyageOrigin VoyageOrigin
		{
			get { return DocVoyageOrigin.New(JobSailing.Origin, Factory); }
		}

		public ZDateTime ETD
		{
			get { return JobSailing.JX_JA_E_DEP; }
		}

		public DocUNLOCO PortOfLoading
		{
			get
			{
				var uNLOCO = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, JobSailing.JX_JA_RL_NKPortOfLoading);
				return DocUNLOCO.New(uNLOCO, Factory);
			}
		}

		public DocAddress DepartureCTO
		{
			get { return DocAddress.New(JobSailing.DepartureCTOAddress, Factory); }
		}

		public ZString DepartureReference
		{
			get { return JobSailing.Origin != null ? JobSailing.Origin.JA_DepartReference : ZString.Empty; }
		}

		#endregion

		#region Arrival Fields

		public DocVoyageDestination VoyageDestination
		{
			get { return JobSailing.JX_JB.IsValid ? DocVoyageDestination.New(JobSailing.Factory, JobSailing.JX_JB) : null; }
		}

		public ZDateTime ETA
		{
			get { return JobSailing.JX_JB_E_ARV; }
		}

		public DocUNLOCO PortOfDischarge
		{
			get
			{
				var uNLOCO = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, JobSailing.JX_JB_RL_NKPortOfDischarge);
				return DocUNLOCO.New(uNLOCO, Factory);
			}
		}

		public ZString ArrivalReference
		{
			get { return JobSailing.Destination != null ? JobSailing.Destination.JB_ArrivalReference : ZString.Empty; }
		}

		public DocAddress ArrivalCTO
		{
			get { return DocAddress.New(JobSailing.ArrivalCTOAddress, Factory); }
		}

		#endregion

		#region Notes
		public ZString LoadListInstructions
		{
			get { return GetNotes(PredefinedNoteTypes.Instance.LoadListInstructions.Description, JobSailing); }
		}
		#endregion

		#region Load List Fields
		protected DocPackLinesCollection UnAllocatedPackLines
		{
			get
			{
				DocPackLinesCollection result = new DocPackLinesCollection(JobSailing.Factory);
				if (JobSailing.Voyage != null)
				{
					UnAllocatedPackLinesForSailing allUnpackedPacks = JobSailing.UnAllocatedPackLines;
					allUnpackedPacks.ShowOnlyThisSailing = ZBool.True;
					allUnpackedPacks.ShowOnlyReceived = ZBool.False;
					allUnpackedPacks.ShowOnlyNonTranship = ZBool.True;
					foreach (PackLine unpacked in allUnpackedPacks)
					{
						result.Add(DocPackLines.New(unpacked, Factory));
					}
				}
				return result;
			}
		}

		protected DocPackLinesCollection AllPackLines
		{
			get
			{
				DocPackLinesCollection result = new DocPackLinesCollection(JobSailing.Factory);

				foreach (CommonContainer container in JobSailing.Containers)
				{
					foreach (PackLine packed in container.PackLines)
					{
						DocPackLines packLine = DocPackLines.New(packed, Factory);

						CommonContainer freightContainer = (CommonContainer)Factory.Load(typeof(CommonContainer), container.PK);
						packLine.ContainerForLoadList = DocContainer.New(freightContainer, Factory);
						result.Add(packLine);
					}
				}
				result.AddRange(UnAllocatedPackLines);
				return result;
			}
		}

		public DocLoadListPackLineCollection PackLines
		{
			get { return new DocLoadListPackLineCollection(null, AllPackLines, JobSailing.Factory); }
		}

		public ZString Context
		{
			get { return "SAILING"; }
		}
		public DocOrganisation ShippingLine
		{
			get { return (Voyage != null) ? Voyage.Line : null; }
		}
		#endregion
	}
}
