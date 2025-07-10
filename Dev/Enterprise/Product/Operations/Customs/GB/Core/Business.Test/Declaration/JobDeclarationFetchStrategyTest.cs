using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business.Test.Declaration
{
	class JobDeclarationFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForViewJE_DeclarationType()
		{
			for (var i = 1; i < 7; i++)
			{
				var dec = Factory.New<JobDeclaration>();
				dec.JE_DeclarationType = "E" + i.ToString();
				dec.JE_DeclarationReference = "@#$" + i.ToString();
			}
			Factory.Save();
			CombineAssertions(() =>
			{
				var ignoreStackTraceBeforeThis = System.Environment.StackTrace.SplitByLine().Last();
				var newFactory = new BusinessObjectFactory();
				var decs = newFactory.Load<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, SQLComparisonOperator.StartsWith, "@#$"));
				AssertEquals("1Active Fetch Hints for CusEntryInstruction", 0, newFactory.ActiveFetchHintsForTable(CusEntryInstructionSchema.Constants.TableName));
				AssertEquals("2CusEntryInstruction Hits", 0, newFactory.GetTableHitCount(CusEntryInstructionSchema.Constants.TableName));
				decs.ForEach(x =>
				{
					_ = x.JE_DeclarationType;
				});
				AssertEquals("3Active Fetch Hints for CusEntryInstruction", 0, newFactory.ActiveFetchHintsForTable(CusEntryInstructionSchema.Constants.TableName));
				AssertEquals("4CusEntryInstruction Hits", 6, newFactory.GetTableHitCount(CusEntryInstructionSchema.Constants.TableName));

				newFactory = new BusinessObjectFactory();
				decs = newFactory.Load<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, SQLComparisonOperator.StartsWith, "@#$"));
				AssertEquals("5Active Fetch Hints for CusEntryInstruction", 0, newFactory.ActiveFetchHintsForTable(CusEntryInstructionSchema.Constants.TableName));
				AssertEquals("6CusEntryInstruction Hits", 0, newFactory.GetTableHitCount(CusEntryInstructionSchema.Constants.TableName));
				var columns = new[] { new TableColumn(CusEntryInstructionSchema.Constants.TableName, JobDeclaration.Schema.JE_DeclarationType) };
				decs.ForEach(x =>
				{
					x.FetchStrategy.FetchForView(columns);
				});
				AssertEquals("7Active Fetch Hints for CusEntryInstruction", 6, newFactory.ActiveFetchHintsForTable(CusEntryInstructionSchema.Constants.TableName));
				AssertEquals("80CusEntryInstruction Hits", 0, newFactory.GetTableHitCount(CusEntryInstructionSchema.Constants.TableName));
				decs.ForEach(x =>
				{
					_ = x.JE_DeclarationType;
				});
				AssertEquals("9Active Fetch Hints for CusEntryInstruction", 0, newFactory.ActiveFetchHintsForTable(CusEntryInstructionSchema.Constants.TableName));
				AssertEquals("10CusEntryInstruction Hits", 1, newFactory.GetTableHitCount(CusEntryInstructionSchema.Constants.TableName));
			});
		}
	}
}
