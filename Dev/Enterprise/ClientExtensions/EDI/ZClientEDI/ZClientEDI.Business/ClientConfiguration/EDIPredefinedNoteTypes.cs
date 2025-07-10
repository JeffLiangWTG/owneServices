using CargoWise.Definitions;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI
{
	public class EDIPredefinedNoteTypes : PredefinedNoteTypes
	{
		protected EDIPredefinedNoteTypes()
		{
		}

		#region Instance

		public new static EDIPredefinedNoteTypes Instance
		{
			get { return (EDIPredefinedNoteTypes)PredefinedNoteTypes.Instance; }
		}

		public static void RegisterThisSubTypeOverride()
		{
			PredefinedNoteTypes.OverrideNewDelegate(New);
		}

		static PredefinedNoteTypes New()
		{
			return new EDIPredefinedNoteTypes();
		}

		#endregion

		#region Note Types

		public PredefinedNoteType IncidentDetail
		{
			get
			{
				if (fIncidentDetail == null)
				{
					fIncidentDetail = new PredefinedNoteType((NoResString)"Incident Detail", StmNoteVisibility.PUB, IsUniqueInCollection, !IsReadOnlyAfterAdd, IsTextOnly, false);
				}
				return fIncidentDetail;
			}
		}
		PredefinedNoteType fIncidentDetail;

		public PredefinedNoteType IncidentResolutionDetail
		{
			get
			{
				if (fIncidentResolutionDetail == null)
				{
					fIncidentResolutionDetail = new PredefinedNoteType((NoResString)"Incident Resolution Detail", StmNoteVisibility.PUB, IsUniqueInCollection, !IsReadOnlyAfterAdd, IsTextOnly, false);
				}
				return fIncidentResolutionDetail;
			}
		}
		PredefinedNoteType fIncidentResolutionDetail;

		public PredefinedNoteType IncidentLog
		{
			get
			{
				if (fIncidentLog == null)
				{
					fIncidentLog = new PredefinedNoteType((NoResString)"Incident Log", StmNoteVisibility.PRV, IsUniqueInCollection, IsReadOnlyAfterAdd, IsTextOnly, false);
				}
				return fIncidentLog;
			}
		}
		PredefinedNoteType fIncidentLog;

		public PredefinedNoteType IncidentComment
		{
			get
			{
				if (incidentComment == null)
				{
					incidentComment = new PredefinedNoteType((NoResString)"Incident Comment", StmNoteVisibility.PUB, IsUniqueInCollection, !IsReadOnlyAfterAdd, IsTextOnly, false);
				}
				return incidentComment;
			}
		}
		PredefinedNoteType incidentComment;

		public PredefinedNoteType IncidentTriagePublishedDescription
		{
			get
			{
				if (incidentTriagePublishedDescription == null)
				{
					incidentTriagePublishedDescription = new PredefinedNoteType((NoResString)"Published Description", StmNoteVisibility.PUB, IsUniqueInCollection, !IsReadOnlyAfterAdd, IsTextOnly, false);
				}
				return incidentTriagePublishedDescription;
			}
		}
		PredefinedNoteType incidentTriagePublishedDescription;

		public PredefinedNoteType IncidentTriageChecklistItemPublishedDescription
		{
			get
			{
				if (incidentTriageChecklistItemPublishedDescription == null)
				{
					incidentTriageChecklistItemPublishedDescription = new PredefinedNoteType((NoResString)"Published Description", StmNoteVisibility.PUB, IsUniqueInCollection, IsReadOnlyAfterAdd, IsTextOnly, false);
				}
				return incidentTriageChecklistItemPublishedDescription;
			}
		}
		PredefinedNoteType incidentTriageChecklistItemPublishedDescription;

		public PredefinedNoteType IncidentTriageSupportNotes
		{
			get
			{
				if (incidentTriageSupportNotes == null)
				{
					incidentTriageSupportNotes = new PredefinedNoteType((NoResString)"Support Notes", StmNoteVisibility.PUB, IsUniqueInCollection, !IsReadOnlyAfterAdd, IsTextOnly, false);
				}
				return incidentTriageSupportNotes;
			}
		}
		PredefinedNoteType incidentTriageSupportNotes;

		public PredefinedNoteType IssueManagerAssignedToHistory
		{
			get
			{
				if (fIssueManagerAssignedTo == null)
				{
					fIssueManagerAssignedTo = new PredefinedNoteType((NoResString)"Assigned To History", StmNoteVisibility.PUB, IsUniqueInCollection, false, IsTextOnly, true);
				}
				return fIssueManagerAssignedTo;
			}
		}
		PredefinedNoteType fIssueManagerAssignedTo;

		public PredefinedNoteType FeatureRequestSoftwareChangeNote
		{
			get
			{
				if (fFeatureRequestSoftwareChangeNote == null)
				{
					fFeatureRequestSoftwareChangeNote = new PredefinedNoteType((NoResString)"Feature Request Software Change", StmNoteVisibility.PUB, IsUniqueInCollection, IsReadOnlyAfterAdd, !IsTextOnly, false);
				}
				return fFeatureRequestSoftwareChangeNote;
			}
		}
		PredefinedNoteType fFeatureRequestSoftwareChangeNote;

		public PredefinedNoteType TrainingCourseReview
		{
			get
			{
				if (fTrainingCourseReview == null)
				{
					fTrainingCourseReview = new PredefinedNoteType((NoResString)"Training Course Review", StmNoteVisibility.PUB, IsUniqueInCollection, !IsReadOnlyAfterAdd, IsTextOnly, TrainingNotesMaxLength, false);
				}
				return fTrainingCourseReview;
			}
		}
		PredefinedNoteType fTrainingCourseReview;

		public PredefinedNoteType TrainingCourseSummary
		{
			get
			{
				if (fTrainingCourseSummary == null)
				{
					fTrainingCourseSummary = new PredefinedNoteType((NoResString)"Training Course Summary", StmNoteVisibility.PUB, IsUniqueInCollection, !IsReadOnlyAfterAdd, IsTextOnly, TrainingNotesMaxLength, false);
				}
				return fTrainingCourseSummary;
			}
		}
		PredefinedNoteType fTrainingCourseSummary;

		public PredefinedNoteType TrainingCourseRecommendations
		{
			get
			{
				if (fTrainingCourseRecommendations == null)
				{
					fTrainingCourseRecommendations = new PredefinedNoteType((NoResString)"Training Course Recommendations", StmNoteVisibility.PUB, IsUniqueInCollection, !IsReadOnlyAfterAdd, IsTextOnly, TrainingNotesMaxLength, false);
				}
				return fTrainingCourseRecommendations;
			}
		}
		PredefinedNoteType fTrainingCourseRecommendations;

		public PredefinedNoteType TrainingCourseInternal
		{
			get
			{
				if (fTrainingCourseInternal == null)
				{
					fTrainingCourseInternal = new PredefinedNoteType((NoResString)"Training Course Internal Notes", StmNoteVisibility.PRV, IsUniqueInCollection, !IsReadOnlyAfterAdd, IsTextOnly, TrainingNotesMaxLength, false);
				}
				return fTrainingCourseInternal;
			}
		}
		PredefinedNoteType fTrainingCourseInternal;

		public PredefinedNoteType TrainingCourseSurveyEmail
		{
			get
			{
				if (fTrainingCourseSurveyEmail == null)
				{
					fTrainingCourseSurveyEmail = new PredefinedNoteType((NoResString)"Training Course Survey Email", StmNoteVisibility.PUB, !IsUniqueInCollection, !IsReadOnlyAfterAdd, IsTextOnly, TrainingNotesMaxLength, false);
				}
				return fTrainingCourseSurveyEmail;
			}
		}
		PredefinedNoteType fTrainingCourseSurveyEmail;

		public PredefinedNoteType FeatureRequestPrerequisites
		{
			get
			{
				if (featureRequestPrerequisites == null)
				{
					featureRequestPrerequisites = new PredefinedNoteType((NoResString)"Feature Request Prerequisites", StmNoteVisibility.PUB, IsUniqueInCollection, !IsReadOnlyAfterAdd, !IsTextOnly, false);
				}
				return featureRequestPrerequisites;
			}
		}
		PredefinedNoteType featureRequestPrerequisites;

		public PredefinedNoteType FeatureRequestInternalNote
		{
			get
			{
				if (featureRequestInternalNote == null)
				{
					featureRequestInternalNote = new PredefinedNoteType((NoResString)"Feature Request Internal Notes", StmNoteVisibility.PUB, IsUniqueInCollection, !IsReadOnlyAfterAdd, !IsTextOnly, false);
				}
				return featureRequestInternalNote;
			}
		}
		PredefinedNoteType featureRequestInternalNote;

		public PredefinedNoteType BusinessRequirements
		{
			get
			{
				if (businessRequirements == null)
				{
					businessRequirements = new PredefinedNoteType((NoResString)"Business Requirements", StmNoteVisibility.PUB, IsUniqueInCollection, !IsReadOnlyAfterAdd, !IsTextOnly, false);
				}
				return businessRequirements;
			}
		}
		PredefinedNoteType businessRequirements;

		public PredefinedNoteType TechnicalSpecification
		{
			get
			{
				if (technicalSpecification == null)
				{
					technicalSpecification = new PredefinedNoteType((NoResString)"Technical Specification", StmNoteVisibility.PUB, IsUniqueInCollection, !IsReadOnlyAfterAdd, !IsTextOnly, false);
				}
				return technicalSpecification;
			}
		}
		PredefinedNoteType technicalSpecification;

		public PredefinedNoteType LicenceDiscrepancy
		{
			get
			{
				if (licenceDiscrepancy == null)
				{
					licenceDiscrepancy = new PredefinedNoteType((NoResString)"Licence Discrepancy", StmNoteVisibility.PUB, IsUniqueInCollection, !IsReadOnlyAfterAdd, IsTextOnly, false);
				}
				return licenceDiscrepancy;
			}
		}
		PredefinedNoteType licenceDiscrepancy;

		public PredefinedNoteType StaffCalendarEmailAddress
		{
			get
			{
				if (staffCalendarEmailAddress == null)
				{
					staffCalendarEmailAddress = new PredefinedNoteType((NoResString)"Staff Calendar Email Address", StmNoteVisibility.PUB, IsUniqueInCollection, !IsReadOnlyAfterAdd, IsTextOnly, false);
				}
				return staffCalendarEmailAddress;
			}
		}
		PredefinedNoteType staffCalendarEmailAddress;

		public PredefinedNoteType ColdCallRegisterAdditionalInfo
		{
			get
			{
				if (coldCallRegisterAdditionalInfo == null)
				{
					coldCallRegisterAdditionalInfo = new PredefinedNoteType((NoResString)"Registrant Additional Detail", StmNoteVisibility.PUB, IsUniqueInCollection, !IsReadOnlyAfterAdd, IsTextOnly, false);
				}
				return coldCallRegisterAdditionalInfo;
			}
		}
		PredefinedNoteType coldCallRegisterAdditionalInfo;

		public PredefinedNoteType OpportunityStatusSummary
		{
			get
			{
				if (opportunityStatusSummary == null)
				{
					opportunityStatusSummary = new PredefinedNoteType((NoResString)"Opportunity Status Summary", StmNoteVisibility.PUB, IsUniqueInCollection, !IsReadOnlyAfterAdd, IsTextOnly, false);
				}
				return opportunityStatusSummary;
			}
		}
		PredefinedNoteType opportunityStatusSummary;

		public PredefinedNoteType LicenceDatabaseRegistrationImportNote
		{
			get
			{
				if (licenceDatabaseRegistrationImportNote == null)
				{
					licenceDatabaseRegistrationImportNote = new PredefinedNoteType((NoResString)"Database Registration Import Note", StmNoteVisibility.PUB, !IsUniqueInCollection, IsReadOnlyAfterAdd, IsTextOnly, false);
				}
				return licenceDatabaseRegistrationImportNote;
			}
		}
		PredefinedNoteType licenceDatabaseRegistrationImportNote;

		public PredefinedNoteType IncidentClosingStaffCode
		{
			get
			{
				if (incidentClosingStaffCode == null)
				{
					incidentClosingStaffCode = new PredefinedNoteType((NoResString)"Closing Staff Code", StmNoteVisibility.PUB, IsUniqueInCollection, !IsReadOnlyAfterAdd, IsTextOnly, false);
				}
				return incidentClosingStaffCode;
			}
		}
		PredefinedNoteType incidentClosingStaffCode;

		public PredefinedNoteType IncidentClosureDate
		{
			get
			{
				if (incidentClosureDate == null)
				{
					incidentClosureDate = new PredefinedNoteType((NoResString)"Closure Date", StmNoteVisibility.PUB, IsUniqueInCollection, !IsReadOnlyAfterAdd, IsTextOnly, false);
				}
				return incidentClosureDate;
			}
		}
		PredefinedNoteType incidentClosureDate;

		public PredefinedNoteType IncidentResolutionComment
		{
			get
			{
				if (incidentResolutionComment == null)
				{
					incidentResolutionComment = new PredefinedNoteType((NoResString)"Resolution Comment", StmNoteVisibility.PUB, IsUniqueInCollection, !IsReadOnlyAfterAdd, IsTextOnly, false);
				}
				return incidentResolutionComment;
			}
		}
		PredefinedNoteType incidentResolutionComment;

		public PredefinedNoteType IncidentDispositionText
		{
			get
			{
				if (incidentDispositionText == null)
				{
					incidentDispositionText = new PredefinedNoteType((NoResString)"Disposition Text", StmNoteVisibility.PUB, IsUniqueInCollection, !IsReadOnlyAfterAdd, IsTextOnly, false);
				}
				return incidentDispositionText;
			}
		}
		PredefinedNoteType incidentDispositionText;

		//

		public PredefinedNoteType LicenceDatabaseRegistrationAdditionalInfoNote
		{
			get
			{
				if (licenceDatabaseRegistrationAdditionalInfoNote == null)
				{
					licenceDatabaseRegistrationAdditionalInfoNote = new PredefinedNoteType((NoResString)"Database Registration Additional Info Note", StmNoteVisibility.PUB, !IsUniqueInCollection, IsReadOnlyAfterAdd, IsTextOnly, false);
				}
				return licenceDatabaseRegistrationAdditionalInfoNote;
			}
		}
		PredefinedNoteType licenceDatabaseRegistrationAdditionalInfoNote;

		public PredefinedNoteType IncidentManagementGroupInitialSymptoms
		{
			get
			{
				if (incidentManagementGroupInitialSymptoms == null)
				{
					incidentManagementGroupInitialSymptoms = new PredefinedNoteType((NoResString)"Initial Symptoms", StmNoteVisibility.PUB, IsUniqueInCollection, !IsReadOnlyAfterAdd, IsTextOnly, false);
				}
				return incidentManagementGroupInitialSymptoms;
			}
		}
		PredefinedNoteType incidentManagementGroupInitialSymptoms;

		public PredefinedNoteType IncidentManagementGroupBusinessImpactDescription
		{
			get
			{
				if (incidentManagementGroupBusinessImpactDescription == null)
				{
					incidentManagementGroupBusinessImpactDescription = new PredefinedNoteType((NoResString)"Business Impact Description", StmNoteVisibility.PUB, IsUniqueInCollection, !IsReadOnlyAfterAdd, IsTextOnly, false);
				}
				return incidentManagementGroupBusinessImpactDescription;
			}
		}
		PredefinedNoteType incidentManagementGroupBusinessImpactDescription;

		public PredefinedNoteType IncidentManagementGroupRootCause
		{
			get
			{
				if (incidentManagementGroupRootCause == null)
				{
					incidentManagementGroupRootCause = new PredefinedNoteType((NoResString)"Root Cause", StmNoteVisibility.PUB, IsUniqueInCollection, !IsReadOnlyAfterAdd, IsTextOnly, false);
				}
				return incidentManagementGroupRootCause;
			}
		}
		PredefinedNoteType incidentManagementGroupRootCause;

		public PredefinedNoteType ResolutionWizardOptionCode
		{
			get
			{
				if (resolutionWizardOptionCode == null)
				{
					resolutionWizardOptionCode = new PredefinedNoteType((NoResString)"Resolution Wizard OptionCode", StmNoteVisibility.PUB, IsUniqueInCollection, !IsReadOnlyAfterAdd, IsTextOnly, false);
				}
				return resolutionWizardOptionCode;
			}
		}
		PredefinedNoteType resolutionWizardOptionCode;

		#endregion

		const int TrainingNotesMaxLength = 2000000;
	}
}
