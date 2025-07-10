using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(ARLStatementOfAccountModule))]
	sealed class ARLStatementOfAccountModuleTest : StatementModuleTest
	{
		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var statementHeader = factory.New<CusStatementHeader>();
			statementHeader.B2_StatementNumber = "TST000";
			statementHeader.B2_IsMonthlyStatement = true;
			statementHeader.Messages.AddNew();

			return statementHeader;
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.CA.CAARLStatementOfAccount;

		protected override BusinessObject CreateValidBOForTestSpecifyWorkflowType(Type elementType)
		{
			var result = (CusStatementHeader)base.CreateValidBOForTestSpecifyWorkflowType(elementType);
			result.B2_IsMonthlyStatement = true;

			return result;
		}

		protected override SecurityCheckpoint ExpectedSecurityCheckpoint => Env.Security.CAARLStatementOfAccount;
	}
}
