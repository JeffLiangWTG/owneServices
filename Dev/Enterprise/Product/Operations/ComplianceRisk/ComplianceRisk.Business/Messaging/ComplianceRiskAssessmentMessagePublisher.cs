using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.ComplianceRisk.Business.Messaging;

public static class ComplianceRiskAssessmentMessagePublisher
{
	public static void Publish(BusinessObject job)
	{
		var message = job.Factory.New<ComplianceRiskAssessmentEdiMessage>();
		message.EM_LinkTable = job.TableName;
		message.EM_LinkUniqueID = job.PK;
		message.EM_Status = EDIMessageStatusList.Codes.Queued;
		message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
		message.EM_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
	}

	public const string MaterialChange = "MCH";
}
