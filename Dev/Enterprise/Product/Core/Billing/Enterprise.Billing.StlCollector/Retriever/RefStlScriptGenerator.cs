using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Billing.Collectors;
using Enterprise.Billing.StlCollector.Retriever.Scripts;
using Enterprise.Integration.Billing;

namespace Enterprise.Billing.StlCollector.Retriever
{
	public static class RefStlScriptGenerator
	{
		public static IEnumerable<IStlScript> TestInstancesOfAllScripts => ObjectFactory.Get<ListObject>("StlDynamicCollectorsList").Cast<IRefStlScript>().Select(s => new RefStlScriptRetriever(s));

		public static string GenerateInsertScript(IStlScript[] stlScripts)
		{
#pragma warning disable CW1053

			var scriptBuilder = new StringBuilder(@"USE [CW-RefDatabase]
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
");

#pragma warning restore CW1053

			for (int i = 0; i < stlScripts.Length; ++i)
			{
				var s = stlScripts[i];
				scriptBuilder.AppendLine($"           {(i > 0 ? "," : "")}(NEWID()");
				AppendValue(scriptBuilder, s.Code);
				AppendValue(scriptBuilder, s.Role);
				AppendValue(scriptBuilder, s.Module);
				AppendValue(scriptBuilder, s.Function);
				AppendValue(scriptBuilder, s.Feature);
				AppendValue(scriptBuilder, RefStlScriptHelper.DataGrainToCode(s.StlGrain));
				AppendValue(scriptBuilder, s.Company);
				AppendValue(scriptBuilder, s.Branch);
				AppendValue(scriptBuilder, s.TransactionDateUtc);
				AppendValue(scriptBuilder, s.User);
				AppendValue(scriptBuilder, s.GuidReference);
				AppendValue(scriptBuilder, s.Reference1);
				AppendValue(scriptBuilder, s.Reference2);
				AppendValue(scriptBuilder, s.Reference3);
				AppendValue(scriptBuilder, s.Reference4);
				AppendValue(scriptBuilder, s.AdditionalRefs);
				AppendValue(scriptBuilder, s.BillableCount);
				AppendValue(scriptBuilder, s.Preparation);
				AppendValue(scriptBuilder, s.From);
				AppendValue(scriptBuilder, s.Where);
				AppendValue(scriptBuilder, s.WithRecompile);
				AppendValue(scriptBuilder, s.IsMandatoryForMilestones);
				AppendValue(scriptBuilder, RefActiveOn.All);
				AppendValue(scriptBuilder, RefStlScriptHelper.DateTypeToCode(s.DateType));
				AppendValue(scriptBuilder, s.MinCW1Version);
				AppendValue(scriptBuilder, s.MaxCW1Version);
				AppendValue(scriptBuilder, RefStlScriptHelper.DateToCode(s.CollectionStartDateUtc), closeBracket: true);
			}

			scriptBuilder.Append("GO");
			return scriptBuilder.ToString();
		}

#nullable enable
		static void AppendValue(StringBuilder builder, string? value, bool closeBracket = false)
		{
			var builderValue = value == null ? "NULL" : $"\'{value.Replace("'", "''")}\'";
			builder.AppendLine($"           ,{builderValue}{(closeBracket ? ")" : "")}");
		}
#nullable disable

		static void AppendValue(StringBuilder builder, bool value, bool closeBracket = false)
		{
			builder.AppendLine($"           ,{(value ? "1" : "0")}{(closeBracket ? ")" : "")}");
		}
	}
}
