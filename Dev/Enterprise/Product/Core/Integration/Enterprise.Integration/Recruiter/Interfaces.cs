using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration.Recruiter
{
	public interface ILearningCentreCampaign
	{
	}
	public interface ILearningCentreCampaignCollection { }
	public interface ILearningCentreCampaignItem
	{
		void ReloadSkillRatingTestHistory();
	}
	public interface ILearningCentreQuestion { }

	public interface IHRGlbCompanyCampaign { }

	public interface IHRCampaignProcessTasks { }

	public interface IHRJobApplicant
	{
		ZString HA_Title { get; set; }
		ZString HA_Gender { get; set; }
		ZString HA_FullName { get; set; }
		ZString HA_NameSuffix { get; set; }
		ZString HA_UserAddress1 { get; set; }
		ZString HA_UserAddress2 { get; set; }
		ZString HA_City { get; set; }
		ZString HA_Postcode { get; set; }
		ZString HA_State { get; set; }
		ZString HA_RN_NKCountry { get; set; }
		ZDate HA_Birthdate { get; set; }
		ZString HA_FaxNum { get; set; }
		ZString HA_HomePhone { get; set; }
		ZString HA_MobilePhone { get; set; }
		ZPropertyInfo HA_MobilePhoneInfo { get; }
		ZString HA_RN_NKNationalityCodeISO { get; set; }
		ZString HA_Passport { get; set; }
		ZString HA_DriversLicenseNumber { get; set; }
		ZString HA_EmailAddress { get; set; }
		ZGuid HA_PER { get; set; }
		ZString HA_WorkPhone { get; set; }
		ZDateTime HA_SystemCreateTimeUtc { get; }
		ZBool IsLearningCenterUser { get; }
		ZGuid PK { get; }
		IActiveBusinessObjectCollection CertificatesBizoCollection { get; }
		bool IsInDatabase { get; }
		void UpdateEmailAddress(ZString newEmailAddress);
		void UpdateFromPerson(IGlbPerson person);
		void CancelChanges();
		IGlbPerson Person { get; }
		ZPropertyInfo HA_EmailAddressInfo { get; }
	}

	public interface IHRJobApplicantCollection
	{
	}

	public interface IHRJobApplication
	{
		ZGuid HP_HA { get; set; }
	}

	public interface IHRRecruitmentJobCampaign
	{ }

	public interface IHRRecruitmentJobCampaignProcessTask
	{ }

	public interface IExamSetting : IBusiness
	{
		ZString EXS_Code { get; set; }
		ZShort EXS_MaximumAskedQuestionsPerExam { get; set; }
		ZShort EXS_TestResultsExpireAfterHours { get; set; }
		ZShort EXS_ExamExpiryTimeInMinutes { get; set; }
		ZGuid EXS_G0 { get; set; }
		ZString EXS_ExamVersion { get; set; }
		ZGuid PK { get; }
	}

	public interface IGlbAccreditationJobSkillGroupCollection : IBusinessObjectCollection
	{
		new IGlbAccreditationJobSkillGroup this[int i] { get; }
		new IGlbAccreditationJobSkillGroup AddNew();
		void Reload();
	}

	public interface IGlbAccreditation : IBusiness
	{
		ZString HAC_Code { get; set; }
		ZString HAC_Description { get; set; }
		ZString HAC_CertificateCode { get; set; }
		ZBool HAC_IsRefresher { get; set; }
		ZString HAC_RefresherCertificateExpiryType { get; set; }
		IGlbAccreditationJobSkillGroupCollection Groups { get; }
		IGlbAccreditationTreeModel GlbAccreditationTreeModel { get; }
		IGlbAccreditationDependantCollection Requirements { get; }
		ZGuid PK { get; }
	}

	public interface IGlbAccreditationCollection : IBusinessObjectCollection { }

	public interface IGlbAccreditationDependantCollection : IBusinessObjectCollection
	{
		new IGlbAccreditation this[int i] { get; }
	}

	public interface IAccreditationPersonProxyCollection : IBusinessObjectCollection
	{
		new IAccreditationPersonProxy this[int i] { get; }
	}

	public interface IAccreditationPersonProxy : IBusiness
	{
		IGlbAccreditation Accreditation { get; }
		IGlbAccreditationAttempt Attempt { get; }
	}

	public interface IGlbAccreditationTreeModel
	{
		IGlbAccreditation Accreditation { get; }
	}

	public interface IGlbAccreditationAttempt : IBusiness
	{
		ZGuid HAA_HAC { get; set; }
		ZBool IsStarted { get; }
		ZBool IsCompleted { get; }
		ZBool CheckIsExpired(ZDate checkDate);
		ZString AccreditationCode { get; }
		ZString AccreditationDescription { get; }
		ZString Status { get; }
		ZString Progress { get; }
		ZDate HAA_CommencementDate { get; set; }
		ZDate HAA_CompletionDueDate { get; set; }
		bool HAA_CompletionDueDate_ReadOnly { get; }
		ZDate HAA_CompletionDate { get; set; }
		ZDate HAA_ExpiryDate { get; set; }
		IGlbAccreditation Accreditation { get; }
		IGlbAccreditationTreeModel TreeModel { get; }
		ZString CertificateCode { get; }
		ZString CertificateNumber { get; }
		ZDate CertificateIssueDate { get; }
		ZDate CertificateExpiryDate { get; }
		ZDecimal AverageWeightedScore { get; }
		IAccreditationPersonProxyCollection RequirementsProxyCollection { get; }
		ZGuid PK { get; }
		ZGuid HAA_PER { get; set; }
		ZDate EarliestAttemptCommencementDate { get; }
		IEnumerable<IPersonAccreditationAttempt> ChainOfPreRequesiteAttempts { get; }
	}

	public interface IPersonAccreditationAttempt
	{
		Guid HAA_PK { get; }
		Guid HAA_HAC { get; }
		ZDate HAA_CommencementDate { get; }
		ZDate HAA_CompletionDate { get; }
		ZDate HAA_ExpiryDate { get; }
		bool HAC_IsRefresher { get; }
		string HAC_Code { get; }
		string HAC_CertificateCode { get; }
	}

	[SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	public interface IGlbAccreditationAttemptForm
	{
	}

	[SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	public interface IGlbAccreditationJobSkillGroup : IBusiness
	{
		bool IsDeleted { get; }
		ZDateTime GetCompletedDateForPerson(IGlbPerson person, IGlbAccreditationAttempt attempt, bool useCache);
		IGlbAccreditationJobSkillPivotCollection SkillPivots { get; }
		ZString HJG_Description { get; set; }
		ZShort HJG_Threshold { get; }
		ZString HJG_ParentTableCode { get; set; }
		ZGuid HJG_ParentID { get; set; }
		IGlbAccreditationJobSkillGroupCollection Groups { get; }
		ZGuid PK { get; }
	}
	[SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	public interface IGlbAccreditationJobSkillPivot : IBusiness
	{
		ZGuid HAJ_HS { get; set; }
		ZGuid HAJ_HJG { get; set; }
		ZGuid PK { get; }
	}

	[SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	public interface IGlbAccreditationJobSkillPivotCollection : IBusinessObjectCollection
	{
		new IGlbAccreditationJobSkillPivot this[int i] { get; }
		void Reload();
	}

	[SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	public interface IGlbAccreditationRequirementPivot
	{
		ZGuid HAR_HAC { get; set; }
		ZGuid HAR_HAC_Parent { get; set; }
	}

	public interface IGlbAccreditationAttemptCollection : IBusinessObjectCollection
	{
		new IGlbAccreditationAttempt this[int i] { get; }
		void Reload();
	}

	public interface IAccreditationUpdaterForPerson
	{
		void Run();
		void Cancel();
		void DeleteAttemptsAndCertificates(IGlbAccreditation targetAccreditation = null);
		event EventHandler RunBegin;
		event EventHandler RunEnd;
		event ExamAttemptProcessedEventHandler ExamAttemptProcessed;
		void SetPersons(IGlbPerson[] persons);
		ZBool DeleteExistingCertificates { get; set; }
	}

	[SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	public interface IAccreditationUpdaterForm
	{
	}

	[SuppressMessage("Microsoft.Design", "CA1003:UseGenericEventHandlerInstances")]
	public delegate void ExamAttemptProcessedEventHandler(object sender, ExamAttemptProcessedEventArgs e);

	public class ExamAttemptProcessedEventArgs : EventArgs
	{
		public ExamAttemptProcessedEventArgs(long applicantsProcessed, long applicantsTotal, long examsProcessed, long examsTotal)
		{
			ApplicantsProcessed = applicantsProcessed;
			ApplicantsTotal = applicantsTotal;
			ExamsProcessed = examsProcessed;
			ExamsTotal = examsTotal;
		}

		public long ApplicantsProcessed;
		public long ApplicantsTotal;
		public long ExamsProcessed;
		public long ExamsTotal;
	}

	public interface IExamSettingCodeHelper
	{
		string GetExamVersion(string examSettingsCode, ZGuid campaignItemPk, BusinessObjectFactory factory);
	}

	[SuppressMessage("Microsoft.Design", "CA1003:UseGenericEventHandlerInstances")]
	public delegate void PersonProcessedEventHandler(object sender, PersonProcessedEventArgs e);

	public class PersonProcessedEventArgs : EventArgs
	{
		public PersonProcessedEventArgs(long personsProcessed, long personsTotal)
		{
			PersonsProcessed = personsProcessed;
			PersonsTotal = personsTotal;
		}

		public long PersonsProcessed;
		public long PersonsTotal;
	}

	public interface IAccreditationMerger
	{
		void Merge(BusinessObjectFactory factory);
	}

	public interface IHRApplicationDocument
	{
		ZGuid PK { get; }
		ZGuid HPD_HP { get; set; }
	}

	public interface IExamAttempt
	{
		ZGuid PK { get; }
		ZGuid EXA_G8 { get; set; }
		ZByte EXA_Score { get; set; }
		ZDateTime EXA_TestCommencedUtc { get; set; }
		ZDateTime EXA_TestCompletedUtc { get; set; }
		ZString EXA_Version { get; set; }
		ZString EXA_Status { get; set; }
	}
}
