using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Utilities;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Utilities.Testing
{
	[TestedType(typeof(JobRevRecognitionDates))]
	class JobRevRecognitionDatesTest : EdwHashTest
	{
		protected override string expectedMainDbFunctionHash => "6CDB5BD53094BF94B2C7EE30650FD4D23B656AF95E49E4B890538870876ED744";
		protected override string expectedEdwDbFunctionHash => "ECAF078FFFC0AD0B7FFA8C1DA370D76A6F7DCE8971DE39CC047F64E69A5AE5F1";

		protected override string edwScriptPath => "ReportFunctions/Accounting/Utilities/JobRevRecognitionDates.sql";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new JobRevRecognitionDates();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Accounting.Utilities.JobRevRecognitionDates();
		}
	}
}

