using System;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Modules
{
	public class ClientNumberFountainRegistration
	{
		public static ClientNumberFountainRegistration GetInstance()
		{
			if (fInstance == null)
			{
				fInstance = new ClientNumberFountainRegistration();
			}
			return fInstance;
		}

		[ThreadStatic]
		static ClientNumberFountainRegistration fInstance;

		protected ClientNumberFountainRegistration()
		{
		}

		#region Work Item Sequences - New

		public INumberFountainProxy NewWorkItemNo
		{
			get
			{
				if (fNewWorkItemNo == null)
				{
					FormattedNumberFountainFactory factory = new FormattedNumberFountainFactory("NewWorkItemNo", "WI");
					fNewWorkItemNo = factory.New();
				}

				return fNewWorkItemNo;
			}
		}

		INumberFountainProxy fNewWorkItemNo;

		#endregion

		#region Incident Sequences

		public const string CustomerServiceIncidentNoPrefix = "CS";
		internal const string TelematicsDevicePrefix = "TD";

		#endregion

		#region Professional Services Quote Sequences

		public INumberFountainProxy ProfessionalServicesQuoteNo
		{
			get
			{
				if (fProfessionalServicesQuoteNo == null)
				{
					FormattedNumberFountainFactory factory = new FormattedNumberFountainFactory("ProfessionalServicesQuoteNo", "PSQ");
					fProfessionalServicesQuoteNo = factory.New();
				}
				return fProfessionalServicesQuoteNo;
			}
		}

		INumberFountainProxy fProfessionalServicesQuoteNo;

		#endregion

		#region Incident Management Group

		public INumberFountainProxy IncidentManagementGroupNumber
		{
			get
			{
				if (fIncidentManagementGroupNumber == null)
				{
					var factory = new FormattedNumberFountainFactory("IncidentManagementGroupNumber", "ING");
					fIncidentManagementGroupNumber = factory.New();
				}
				return fIncidentManagementGroupNumber;
			}
		}

		INumberFountainProxy fIncidentManagementGroupNumber;

		#endregion

		#region Incident Triage

		public INumberFountainProxy IncidentTriageNumber
		{
			get
			{
				if (fIncidentTriageNumber == null)
				{
					var factory = new FormattedNumberFountainFactory("IncidentTriageNumber", "TRI");
					fIncidentTriageNumber = factory.New();
				}
				return fIncidentTriageNumber;
			}
		}

		INumberFountainProxy fIncidentTriageNumber;

		#endregion

		#region Incident Triage Checklist Item

		public INumberFountainProxy IncidentTriageChecklistItemNumber
		{
			get
			{
				if (fIncidentTriageChecklistItemNumber == null)
				{
					var factory = new FormattedNumberFountainFactory("IncidentTriageChecklistItemNumber", "TRC");
					fIncidentTriageChecklistItemNumber = factory.New();
				}
				return fIncidentTriageChecklistItemNumber;
			}
		}

		INumberFountainProxy fIncidentTriageChecklistItemNumber;

		#endregion

		#region Investigation Item

		public INumberFountainProxy InvestigationItemNumber
		{
			get
			{
				if (fInvestigationItemNumber == null)
				{
					var factory = new FormattedNumberFountainFactory("InvestigationItemNumber", "INV");
					fInvestigationItemNumber = factory.New();
				}
				return fInvestigationItemNumber;
			}
		}

		INumberFountainProxy fInvestigationItemNumber;

		#endregion

		#region Training Course Booking Sequence

		public INumberFountainProxy TrainingCourseBookingNo
		{
			get
			{
				if (fTrainingCourseBookingNo == null)
				{
					FormattedNumberFountainFactory factory = new FormattedNumberFountainFactory("TrainingCourseBookingNo", "TRN");
					fTrainingCourseBookingNo = factory.New();
				}
				return fTrainingCourseBookingNo;
			}
		}

		INumberFountainProxy fTrainingCourseBookingNo;

		#endregion

		#region Issue Sequences

		public INumberFountainProxy IssueNo
		{
			get
			{
				if (fIssueNo == null)
				{
					FormattedNumberFountainFactory factory = new FormattedNumberFountainFactory("IssueNo");
					fIssueNo = factory.New();
				}
				return fIssueNo;
			}
		}

		INumberFountainProxy fIssueNo;

		#endregion

		#region Licence Numbers

		public INumberFountainProxy LicenceDatabaseNumber
		{
			get
			{
				if (licenceDatabaseNumber == null)
				{
					var factory = new NonFormattedNumberFountainFactory("LicenceDatabaseNumber", minValue: 100);
					licenceDatabaseNumber = factory.New();
				}
				return licenceDatabaseNumber;
			}
		}

		INumberFountainProxy licenceDatabaseNumber;

		public INumberFountainProxy LicenceCompanyNumber
		{
			get
			{
				if (licenceCompanyNumber == null)
				{
					var factory = new NonFormattedNumberFountainFactory("LicenceCompanyNumber", minValue: 1000);
					licenceCompanyNumber = factory.New();
				}
				return licenceCompanyNumber;
			}
		}

		INumberFountainProxy licenceCompanyNumber;

		#endregion

		#region Telematics Device Numbers

		public INumberFountainProxy TelematicsDeviceNumber => telematicsDeviceNumber ?? (telematicsDeviceNumber = new FormattedNumberFountainFactory("TelematicsDeviceNumber", TelematicsDevicePrefix).New());

		INumberFountainProxy telematicsDeviceNumber;

		#endregion

		#region EnterpriseID

		public INumberFountainProxy EnterpriseID
		{
			get
			{
				if (enterpriseID == null)
				{
					var factory = new FormattedNumberFountainFactory("EnterpriseID", "E", formatDigits: 6);
					enterpriseID = factory.New();
				}
				return enterpriseID;
			}
		}

		INumberFountainProxy enterpriseID;

		#endregion

		#region TrustedSystemNumber

		public INumberFountainProxy TrustedSystemNumber
		{
			get
			{
				if (trustedSystemNumber == null)
				{
					var factory = new FormattedNumberFountainFactory("TrustedSystemNumber", "ETS", formatDigits: 6);
					trustedSystemNumber = factory.New();
				}
				return trustedSystemNumber;
			}
		}
		INumberFountainProxy trustedSystemNumber;

		#endregion
	}
}
