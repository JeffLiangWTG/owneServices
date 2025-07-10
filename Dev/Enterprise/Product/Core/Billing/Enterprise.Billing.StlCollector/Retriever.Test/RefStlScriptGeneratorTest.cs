using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Billing;
using Moq;

namespace Enterprise.Billing.StlCollector.Retriever.Testing
{
	sealed class RefStlScriptGeneratorTest : TestCaseWithFactory
	{
		public void TestGenerateInsertScript()
		{
			var stlScript1 = CreateStubScript(true, "S2D", "ROLE", "MODULE", "FUNCTION", "FEATURE", "COMPANY", "BRANCH", "DATEUTC", "REF1", "REF2", "REF3", "REF4", "GREF", "ADDREF", "USER", "PREP", "FROM WITH 'QUOTES'", "WHERE", "BILL", true, "ConversionTestClass", StlDataGrain.Transactional, StlDateType.DateTime, "ALL", new DateTime(2024, 3, 27, 1, 2, 1), "21.10.26.0", "22.5.17.0");
			var stlScript2 = CreateStubScript(false, "GIS", "ROLE2", "MODULE2", "FUNCTION2", "FEATURE2", "COMPANY2", "BRANCH2", "DATEUTC2", "REF1-2", "REF2-2", "REF3-2", "REF4-2", "GREF2", "ADDREF2", "USER2", "PREP2", "FROM2", "WHERE2", "BILL2", false, "ScriptGeneratorTestClass", StlDataGrain.MonthlyAllowHistoricalData, StlDateType.DateTimeOffset, "TST", DateTime.MinValue, "20.5.13.0", "21.11.5.0");
			var output = RefStlScriptGenerator.GenerateInsertScript(new[] { stlScript1, stlScript2 });
			AssertEquals(@"USE [CW-RefDatabase]
GO

INSERT INTO [dbo].[RefStlScript]
           ([STL_PK]
           ,[STL_FeatureCode]
           ,[STL_RoleName]
           ,[STL_ModuleName]
           ,[STL_FunctionName]
           ,[STL_FeatureName]
           ,[STL_DataGranularity]
           ,[STL_CompanyCode]
           ,[STL_BranchCode]
           ,[STL_TransactionDateUtc]
           ,[STL_CreatingUserCode]
           ,[STL_GuidReference]
           ,[STL_BillingReference1]
           ,[STL_BillingReference2]
           ,[STL_BillingReference3]
           ,[STL_BillingReference4]
           ,[STL_AdditionalRefs]
           ,[STL_TransactionCount]
           ,[STL_PreparationScript]
           ,[STL_FromClause]
           ,[STL_WhereClause]
           ,[STL_WithOptionRecompile]
           ,[STL_UsedInBilling]
           ,[STL_ActiveOn]
           ,[STL_DateType]
           ,[STL_MinCW1Version]
           ,[STL_MaxCW1Version]
		   ,[STL_CollectionStartDateUtc])
     VALUES
           (NEWID()
           ,'S2D'
           ,'ROLE'
           ,'MODULE'
           ,'FUNCTION'
           ,'FEATURE'
           ,'TRN'
           ,'COMPANY'
           ,'BRANCH'
           ,'DATEUTC'
           ,'USER'
           ,'GREF'
           ,'REF1'
           ,'REF2'
           ,'REF3'
           ,'REF4'
           ,'ADDREF'
           ,'BILL'
           ,'PREP'
           ,'FROM WITH ''QUOTES'''
           ,'WHERE'
           ,1
           ,1
           ,'ALL'
           ,'DTE'
           ,'21.10.26.0'
           ,'22.5.17.0'
           ,'2024-03-27 01:02:01.000')
           ,(NEWID()
           ,'GIS'
           ,'ROLE2'
           ,'MODULE2'
           ,'FUNCTION2'
           ,'FEATURE2'
           ,'MAH'
           ,'COMPANY2'
           ,'BRANCH2'
           ,'DATEUTC2'
           ,'USER2'
           ,'GREF2'
           ,'REF1-2'
           ,'REF2-2'
           ,'REF3-2'
           ,'REF4-2'
           ,'ADDREF2'
           ,'BILL2'
           ,'PREP2'
           ,'FROM2'
           ,'WHERE2'
           ,0
           ,0
           ,'ALL'
           ,'DTO'
           ,'20.5.13.0'
           ,'21.11.5.0'
           ,NULL)
GO", output);
		}

		static IStlScript CreateStubScript(
				bool isMandatoryForMilestones, string code, string role, string module, string function, string feature, string company, string branch, string transactionDateUtc, string reference1, string reference2, string reference3,
				string reference4, string guidReference, string additionalRefs, string user, string preparation, string from, string where, string billableCount, bool withRecompile, string name, StlDataGrain dataGrain,
				StlDateType dateType, string activeOn, DateTime collectionStartDateUtc, string minVersion = null, string maxVersion = null)
		{
			var stlScript = new Mock<IStlScript>();
			stlScript.Setup(m => m.IsMandatoryForMilestones).Returns(isMandatoryForMilestones);
			stlScript.Setup(m => m.Code).Returns(code);
			stlScript.Setup(m => m.Role).Returns(role);
			stlScript.Setup(m => m.Module).Returns(module);
			stlScript.Setup(m => m.Function).Returns(function);
			stlScript.Setup(m => m.Feature).Returns(feature);
			stlScript.Setup(m => m.Company).Returns(company);
			stlScript.Setup(m => m.Branch).Returns(branch);
			stlScript.Setup(m => m.TransactionDateUtc).Returns(transactionDateUtc);
			stlScript.Setup(m => m.Reference1).Returns(reference1);
			stlScript.Setup(m => m.Reference2).Returns(reference2);
			stlScript.Setup(m => m.Reference3).Returns(reference3);
			stlScript.Setup(m => m.Reference4).Returns(reference4);
			stlScript.Setup(m => m.GuidReference).Returns(guidReference);
			stlScript.Setup(m => m.AdditionalRefs).Returns(additionalRefs);
			stlScript.Setup(m => m.User).Returns(user);
			stlScript.Setup(m => m.Preparation).Returns(preparation);
			stlScript.Setup(m => m.From).Returns(from);
			stlScript.Setup(m => m.Where).Returns(where);
			stlScript.Setup(m => m.BillableCount).Returns(billableCount);
			stlScript.Setup(m => m.WithRecompile).Returns(withRecompile);
			stlScript.Setup(m => m.Name).Returns(name);
			stlScript.Setup(m => m.StlGrain).Returns(dataGrain);
			stlScript.Setup(m => m.DateType).Returns(dateType);
			stlScript.Setup(m => m.ActiveOn).Returns(activeOn);
			stlScript.Setup(m => m.MinCW1Version).Returns(minVersion);
			stlScript.Setup(m => m.MaxCW1Version).Returns(maxVersion);
			stlScript.Setup(m => m.CollectionStartDateUtc).Returns(collectionStartDateUtc);
			return stlScript.Object;
		}
	}
}
