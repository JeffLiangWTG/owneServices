using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(CSARevenueSummaryFormModule))]
	public class CSARevenueSummaryFormModuleTest : StatementModuleTest
	{
		public override void TestFilterControl()
		{
			using (var module = GetModule())
			{
				AssertType<CSARevenueSummaryFormStatementFilterControl>(module.EmbeddedControl);
			}
		}

		protected override SecurityCheckpoint ExpectedSecurityCheckpoint => Env.Security.CACSARevenueSummaryForm;

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.CA.CACSARevenueSummaryForm;

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var statementHeader = factory.New<CusStatementHeader>();
			statementHeader.B2_StatementNumber = "TST000";
			statementHeader.B2_IsMonthlyStatement = true;
			statementHeader.Messages.AddNew();
			return statementHeader;
		}

		protected override BusinessObject CreateValidBOForTestSpecifyWorkflowType(Type elementType)
		{
			var result = (CusStatementHeader)base.CreateValidBOForTestSpecifyWorkflowType(elementType);
			result.B2_IsMonthlyStatement = true;
			return result;
		}

		protected override ZFilterModule GetModule() => new CSARevenueSummaryFormModule();
	}
}
