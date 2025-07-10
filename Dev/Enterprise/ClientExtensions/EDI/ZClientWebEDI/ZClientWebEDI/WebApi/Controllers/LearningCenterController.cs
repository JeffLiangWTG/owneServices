using System;
using System.Web.Http;
using Enterprise.Client.EDI.LearningCenter;
using Enterprise.Recruiter.Business;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[RoutePrefix("api/LearningCenter")]
	public class LearningCenterController : BusinessObjectController
	{
		[HttpGet]
		[Route("GetLearningCenterJobSkillUrl")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
		public string GetLearningCenterJobSkillUrl(string staffCode, string jobSkillCode)
		{
			return LearningCentreJobSkillHelper.GetJobSkillUrl(staffCode, jobSkillCode);
		}

		[HttpGet]
		[Route("HasJobSkill")]
		public bool HasJobSkill(string jobSkillCode, string staffCode)
		{
			return LearningCentreTestResultHelper.HasJobSkill(jobSkillCode, staffCode);
		}

		[HttpGet]
		[Route("ShouldShowPersonalEmailRequestNotification")]
		public bool ShouldShowPersonalEmailRequestNotification(Guid contactPK)
		{
			return LearningCentreTestResultHelper.CompletedExamsWithoutPersonalEmailAddress(contactPK);
		}

		[HttpGet]
		[Route("GetLearningCenterJobSkillProgressDetails")]
		public LearningCentreTestResultHelper.JobSkillProgressDetails GetLearningCenterJobSkillProgressDetails(string staffCode, string jobSkillCode)
		{
			return LearningCentreJobSkillHelper.GetJobSkillProgressDetails(staffCode, jobSkillCode);
		}
	}
}
