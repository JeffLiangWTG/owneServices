using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.DocumentEngineCore.DocWrappers;
	using Enterprise.DocumentWrappers.GenericWrappers.Base;
	using Enterprise.MasterFiles.Business;

	[DefaultField("Type"), WrapperTypeName("ContainerService")]
	public class ServiceWrapper : GenericWrapper
	{
		public ServiceWrapper(JobService jobServiceBO, BusinessObjectFactory factory)
			: base(jobServiceBO, factory)
		{
			JobServiceBO = jobServiceBO ?? factory.GetNull<JobService>();
		}

		public static ServiceWrapper New(BusinessObject jobServiceBO, BusinessObjectFactory factoryToWrap)
		{
			return jobServiceBO != null ? new ServiceWrapper((JobService)jobServiceBO, factoryToWrap) : null;
		}

		readonly JobService JobServiceBO;

		public ZString ServiceDocumentTitle
		{
			get
			{
				var typeDescription = Type.Description;

				if (WrapperForReportName != null)
				{
					var reportName = WrapperForReportName.ReportName;

					if (!string.IsNullOrEmpty(reportName))
					{
						if (reportName.Contains((NoResString)"Request"))
						{
							return Res.GetString("374b2bc7-aa66-496f-9c97-990f44e53101", "Request for {0} Service", typeDescription);
						}
						else if (reportName.Contains((NoResString)"Authorization"))
						{
							return Res.GetString("0f69e891-573b-4f04-a3ff-dd48edba94bb", "Authorization for {0} Service", typeDescription);
						}
					}
				}

				return Res.GetString("7e1b0b3e-a248-4747-a83d-fa03a3f84fee", "{0} Service", typeDescription).Trim();
			}
		}

		internal GenericWrapper WrapperForReportName { get; set; }

		public CodeAndDescriptionWrapper Type
		{
			get { return fType ?? (fType = new CodeAndDescriptionWrapper(JobServiceBO.ES_ServiceCode, JobServiceBO.Lookups.JobServiceType_List, Factory)); }
		}
		CodeAndDescriptionWrapper fType;

		public OrganisationWrapper Contractor
		{
			get { return fContractor ?? (fContractor = new OrganisationWrapper(OrganisationUsageType.Contractor, JobServiceBO.Contractor, ContactType.All, Factory)); }
		}
		OrganisationWrapper fContractor;

		public AddressWrapper LocationAddress
		{
			get
			{
				AddressWrapper locationAddress;

				if (JobServiceBO.ES_OA_Location.IsEmpty && JobServiceBO.Contractor != null)
				{
					locationAddress = new AddressWrapper(Factory.Load<OrgAddress>(JobServiceBO.Contractor.MainAddress.PK), ContactType.All, Factory);
				}
				else
				{
					locationAddress = new AddressWrapper(Factory.Load<OrgAddress>(JobServiceBO.ES_OA_Location), ContactType.All, Factory);
				}

				return locationAddress;
			}
		}

		public ZString ReferenceNumber
		{
			get { return JobServiceBO.ES_References; }
		}

		public ZString ServiceDuration
		{
			get
			{
				TotalHoursHelper time = new TotalHoursHelper();
				return time.GetTextFromTime(JobServiceBO.ES_Duration);
			}
		}

		public ZString ServiceNote
		{
			get { return JobServiceBO.ES_ServiceNote; }
		}

		public ZDateTime DateCompleted
		{
			get { return JobServiceBO.ES_Completed; }
		}

		public ZDateTime DateBooked
		{
			get { return JobServiceBO.ES_Booked; }
		}

		public ZDecimal ServiceCount
		{
			get { return JobServiceBO.ES_ServiceCount; }
		}

		public OrganisationWrapper RequestedBy
		{
			get { return new OrganisationWrapper(OrganisationUsageType.ServiceRequestedBy, (GlbBranch.CurrentBranch.OrgProxy), ContactType.All, Factory); }
		}

		public ZString Note
		{
			get { return JobServiceBO.ES_ServiceNote; }
		}
	}
}
