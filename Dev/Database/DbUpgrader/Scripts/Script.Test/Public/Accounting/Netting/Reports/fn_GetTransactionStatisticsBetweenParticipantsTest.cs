using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Netting.Reports;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Netting.Reports.Testing
{
	[TestedType(typeof(fn_GetTransactionStatisticsBetweenParticipants))]
	class fn_GetTransactionStatisticsBetweenParticipantsTest : DbCreateScriptTest
	{
		//this funtion is tested in the consumers like fn_NVPMGetNumberOfInvoices, fn_NVPMGetValueOfInvoices, fn_NVPMGetGrossValueOfInvoices
	}
}
