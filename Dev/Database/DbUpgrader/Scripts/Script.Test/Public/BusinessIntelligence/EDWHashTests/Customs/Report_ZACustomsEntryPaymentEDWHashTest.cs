using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZA;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests.Customs
{
	[TestedType(typeof(Report_ZACustomsEntryPayment))]
	class Report_ZACustomsEntryPaymentEDWHashTest : EdwHashTest
	{
		protected override string edwScriptPath => "ReportFunctions/Customs/Report_ZACustomsEntryPayment.sql";
		protected override string expectedMainDbFunctionHash => "D38EB245FBEE479A4EA222EA4BB52FA2A78282764621103336D44BBC36AA030A";
		protected override string expectedEdwDbFunctionHash => "76F1A0014D057982CDEB00639B4D0BC49331FA7728EE8E6A2980205FB5E96A11";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new Report_ZACustomsEntryPayment();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Customs.Report_ZACustomsEntryPayment();
		}
	}
}

