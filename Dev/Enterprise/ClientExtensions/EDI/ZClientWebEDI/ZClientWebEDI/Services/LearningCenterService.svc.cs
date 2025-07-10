using System;
using System.ServiceModel;
using System.ServiceModel.Activation;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Client.EDI.LearningCenter;
using Enterprise.MarketingManager.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Scheduler.Business;
using static Enterprise.Recruiter.Business.LearningCentreTestResultHelper;

namespace Enterprise.ZClientWebCargoWiseEDI.Services
{
	[ServiceContract(Namespace = "http://schemas.cargowise.com/")]
	public interface ILearningCenterService
	{
		[OperationContract]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
		string GetLearningCenterExamUrl(string examId, Guid contactPK, string countryCode, string jobSkillCode = "", string language = "", string examSettingCode = "");

		[OperationContract]
		string GetLearningCenterExamResult(string examId, Guid contactPK);

		[OperationContract]
		ZDateTime GetNextScheduledServiceTaskDateTime(StmScheduleTask task);

		[OperationContract]
		TestResultDetails GetLearningCenterExamResultDetails(string examId, Guid contactPK, string skillCode, string examSettingCode = "");

		[OperationContract]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
		string GetLearningCenterJobSkillUrl(string staffCode, string jobSkillCode);

		[OperationContract]
		bool ShouldShowPersonalEmailRequestNotification(Guid contactPK);

		[OperationContract]
		JobSkillProgressDetails GetLearningCenterJobSkillProgressDetails(string staffCode, string jobSkillCode);

		[OperationContract]
		JobSkillItem[] GetJobSkillsRequireSkillTest();

		[OperationContract]
		AttemptDetails GetLastAccreditationAttemptDetails(Guid contactPK, string accreditationCode);
	}

	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	[ServiceBehavior(Namespace = "http://schemas.cargowise.com/")]
	public class LearningCenterService : MyAccountWebServiceBase, ILearningCenterService
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
		public string GetLearningCenterExamUrl(string examId, Guid contactPK, string countryCode, string jobSkillCode = "", string language = "", string examSettingCode = "")
		{
			ZString result = ZString.Empty;

			using (Db.DisposableActionForDbConnection())
			{
				try
				{
					ValidateRequestIpAddress();
					result = LearningCentreTestUrlHelper.GetTestUrlForContact(examId, contactPK, countryCode, jobSkillCode, language, examSettingCode);
				}
				catch (Exception ex) when (!ex.IsCriticalException() && !(ex is FaultException<ErrorData>))
				{
					HandleError("Server Error", ex.Message + System.Environment.NewLine + ex.StackTrace);
					ErrorReporter.ReportOnce(ex.Message, ex);
				}

				return result;
			}
		}

		public string GetLearningCenterExamResult(string examId, Guid contactPK)
		{
			ZString result = ZString.Empty;

			try
			{
				ValidateRequestIpAddress();
				result = LearningCentreTestResultHelper.GetTestResult(examId, contactPK);
			}
			catch (Exception ex) when (!ex.IsCriticalException() && !(ex is FaultException<ErrorData>))
			{
				HandleError("Server Error", ex.Message + System.Environment.NewLine + ex.StackTrace);
				ErrorReporter.ReportOnce(ex.Message, ex);
			}

			return result;
		}

		public ZDateTime GetNextScheduledServiceTaskDateTime(StmScheduleTask task)
		{
			var result = ZDateTime.Empty;

			try
			{
				ValidateRequestIpAddress();
				result = ScheduleTaskHelper.GetNextScheduleServiceTaskDateTime(task);
			}
			catch (Exception ex) when (!ex.IsCriticalException() && !(ex is FaultException<ErrorData>))
			{
				HandleError("Server Error", ex.Message + System.Environment.NewLine + ex.StackTrace);
				ErrorReporter.ReportOnce(ex.Message, ex);
			}

			return result;
		}

		public TestResultDetails GetLearningCenterExamResultDetails(string examId, Guid contactPK, string skillCode, string examSettingCode = "")
		{
			TestResultDetails result = new TestResultDetails();

			using (Db.DisposableActionForDbConnection())
			{
				try
				{
					ValidateRequestIpAddress();
					result = GetTestResultDetails(examId, contactPK, skillCode, examSettingCode);
				}
				catch (Exception ex) when (!ex.IsCriticalException() && !(ex is FaultException<ErrorData>))
				{
					HandleError("Server Error", ex.Message + System.Environment.NewLine + ex.StackTrace);
					ErrorReporter.ReportOnce(ex.Message, ex);
				}

				return result;
			}
		}

		public bool ShouldShowPersonalEmailRequestNotification(Guid contactPK)
		{
			var result = false;

			using (Db.DisposableActionForDbConnection())
			{
				try
				{
					ValidateRequestIpAddress();
					result = LearningCentreTestResultHelper.CompletedExamsWithoutPersonalEmailAddress(contactPK);
				}
				catch (Exception ex) when (!ex.IsCriticalException() && !(ex is FaultException<ErrorData>))
				{
					HandleError("Server Error", ex.Message + System.Environment.NewLine + ex.StackTrace);
					ErrorReporter.ReportOnce(ex.Message, ex);
				}

				return result;
			}
		}

		public string GetLearningCenterJobSkillUrl(string staffCode, string jobSkillCode)
		{
			ZString result = ZString.Empty;

			try
			{
				ValidateRequestIpAddress();
				result = LearningCentreJobSkillHelper.GetJobSkillUrl(staffCode, jobSkillCode);
			}
			catch (Exception ex) when (!ex.IsCriticalException() && !(ex is FaultException<ErrorData>))
			{
				HandleError("Server Error", ex.Message + System.Environment.NewLine + ex.StackTrace);
				ErrorReporter.ReportOnce(ex.Message, ex);
			}

			return result;
		}

		public JobSkillProgressDetails GetLearningCenterJobSkillProgressDetails(string staffCode, string jobSkillCode)
		{
			JobSkillProgressDetails result = null;

			try
			{
				ValidateRequestIpAddress();

				result = LearningCentreJobSkillHelper.GetJobSkillProgressDetails(staffCode, jobSkillCode);
			}
			catch (Exception ex) when (!ex.IsCriticalException() && !(ex is FaultException<ErrorData>))
			{
				HandleError("Server Error", ex.Message + System.Environment.NewLine + ex.StackTrace);
				ErrorReporter.ReportOnce(ex.Message, ex);
			}

			return result;
		}

		public JobSkillItem[] GetJobSkillsRequireSkillTest()
		{
			var result = Array.Empty<JobSkillItem>();
			try
			{
				ValidateRequestIpAddress();
			}
			catch (Exception ex) when (!ex.IsCriticalException() && !(ex is FaultException<ErrorData>))
			{
				HandleError("Server Error", ex.Message + System.Environment.NewLine + ex.StackTrace);
				ErrorReporter.ReportOnce(ex.Message, ex);
			}

			return result;
		}

		public AttemptDetails GetLastAccreditationAttemptDetails(Guid contactPK, string accreditationCode)
		{
			using (Db.DisposableActionForDbConnection())
			{
				try
				{
					ValidateRequestIpAddress();
					return LearningCentreTestResultHelper.GetLastAccreditationAttemptDetails(contactPK, accreditationCode);
				}
				catch (Exception ex) when (!ex.IsCriticalException() && !(ex is FaultException<ErrorData>))
				{
					HandleError("Server Error", ex.Message + System.Environment.NewLine + ex.StackTrace);
					ErrorReporter.ReportOnce(ex.Message, ex);
				}

				return new AttemptDetails();
			}
		}
	}
}
