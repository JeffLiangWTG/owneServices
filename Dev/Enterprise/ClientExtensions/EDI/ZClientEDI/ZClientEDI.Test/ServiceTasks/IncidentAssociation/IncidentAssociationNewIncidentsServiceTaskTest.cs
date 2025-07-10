using System.Collections.Generic;
using Enterprise.Client.EDI.IncidentManager.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.Test
{
	[TestedType(typeof(IncidentAssociationNewIncidentsServiceTask))]
	public class IncidentAssociationNewIncidentsServiceTaskTest : ServiceTaskTestCase<IncidentAssociationNewIncidentsServiceTask>
	{
		public void TestServiceTask()
		{
			AssertEquals("IAN", IncidentAssociationNewIncidentsServiceTask.Code);
			AssertEquals("Incident_Association_IAN", IncidentAssociationNewIncidentsServiceTask.DbAppLockCode);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						AutoIncidentMain.Schema.TableName,
						null,
						AutoIncidentMain.Schema.IM_IncidentType + "=" + IncidentConstants.IncidentType.SupportIncident),
				};
			}
		}
	}
}
