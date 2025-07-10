using CargoWise.Application;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.Business
{
	class BMSQLFunctionHelper : IBMSQLFunctionHelper
	{
		bool IgnoreIterations => ObjectFactory.Get<IBMSRegistry>().IgnoreIterationsWhenCalculatingStartability || !ObjectFactory.Get<IBMSRegistry>().IsPlanningManagementEnabled;
		string IBMSQLFunctionHelper.GetCurrentTasks => IgnoreIterations ? BMGlobalConstants.CurrentTasksIgnoringIterationsSQLFunctionText : BMGlobalConstants.CurrentTasksSQLFunctionText;
		string IBMSQLFunctionHelper.GetCurrentTasksInWorkflows => IgnoreIterations ? BMGlobalConstants.CurrentTasksInWorkflowsIgnoringIterationsSQLFunctionText : BMGlobalConstants.CurrentTasksInWorkflowsSQLFunctionText;
	}
}
