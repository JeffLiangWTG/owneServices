using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(ChinaAssetsProvision))]
	class ChinaAssetsProvisionTest : EdwHashTest
	{
		[ExpectNoExceptions]
		public void TestTransactionDescPaymentReferenceNumberMaxLength()
		{
			GLTransactionsSPTest.SetUpDataForTestTransactionDescPaymentReferenceNumberMaxLength(TestConnection);

			var template = @"EXEC ChinaAssetsProvision
							@CompanyPK = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC',
							@EndPeriod = 201505,
							@Branch = NULL";

			var result = DataUtils.GetDataTableFromQuery(TestConnection, template);
		}
	
		protected override string expectedMainDbFunctionHash => "FC168A5A483911F8AF23B49DBD3CDDA13D6C7419181D7705EE85F55107519E0B";
		protected override string expectedEdwDbFunctionHash => "66F13218E9D1D16D73F8A38A0A3F36B2DB35B553319FB93448D20182CC41C7C9";

		protected override string edwScriptPath => "Function/Accounting/ChinaAssetsProvision.sql";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new ChinaAssetsProvision();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.ChinaAssetsProvision();
		}
	}
}

