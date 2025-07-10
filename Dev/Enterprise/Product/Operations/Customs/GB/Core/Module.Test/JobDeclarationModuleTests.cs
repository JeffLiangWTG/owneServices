using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Wizards.EIDR;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Module.Testing
{
	class JobDeclarationModuleTestsForMenuItems : EU.Module.Testing.JobDeclarationModuleTestsForMenuItems
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
				using (var module = new JobDeclarationModule())
				{
					module.PerformSearch_ForTest();
					var collection = (BusinessObjectCollection)module.GridCollection;
					var newFactory = collection.Factory;
					var decs = collection.ToArray<JobDeclaration>();
					AssertEquals("1Active Fetch Hints for CusEntryInstruction", 0, newFactory.ActiveFetchHintsForTable(CusEntryInstructionSchema.Constants.TableName));
					AssertEquals("2CusEntryInstruction Hits", 0, newFactory.GetTableHitCount(CusEntryInstructionSchema.Constants.TableName));
					decs.ForEach(x =>
					{
						_ = x.JE_DeclarationType;
					});
					AssertEquals("3Active Fetch Hints for CusEntryInstruction", 0, newFactory.ActiveFetchHintsForTable(CusEntryInstructionSchema.Constants.TableName));
					AssertEquals("4CusEntryInstruction Hits", 6, newFactory.GetTableHitCount(CusEntryInstructionSchema.Constants.TableName));

					module.PerformSearch_ForTest();
					collection = (BusinessObjectCollection)module.GridCollection;
					newFactory = collection.Factory;
					decs = collection.ToArray<JobDeclaration>();
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
				}
			});
		}

		public new void TestGetNewStandardMenuItems()
		{
			using (var module = new JobDeclarationModule())
			{
				module.SetupAndGetGrid();
				using (var menu = module.NewMenuItem)
				{
					AssertContains("EIDR", menu.MenuItems[4].Text);
					AssertContains("CDS FSD", menu.MenuItems[5].Text);
				}
			}
		}

		public void TestNewStandardMenuItemsActuallyDoSomething_EidrWizard()
		{
			RunMenuItemClickTest(4);
		}

		public new void TestNewStandardMenuItemsActuallyDoSomething_NewExportWizard()
		{
			Assert("Tested in EU", true);
		}

		public new void TestNewStandardMenuItemsActuallyDoSomething_NewImportWizard()
		{
			Assert("Tested in EU", true);
		}

		public new void TestNewStandardMenuItemsActuallyDoSomething_SadHWizard()
		{
			Assert("Tested in EU", true);
		}

		protected override EU.Module.JobDeclarationModule GetNewJobDeclarationModule(bool p)
		{
			return new GbJobDeclarationModuleForTest(p);
		}

		class EidrWizardManagerForTest : EidrWizardManager
		{
			public EidrWizardManagerForTest(JobDeclaration declaration, bool simulateUserClickGo)
				: base(declaration)
			{
				ExecutedSuccessfully = simulateUserClickGo;
			}
		}
		protected class GbJobDeclarationModuleForTest : JobDeclarationModule
		{
			public GbJobDeclarationModuleForTest(bool simulateUserClickGo)
			{
				this.simulateUserClickGo = simulateUserClickGo;
			}

			protected override EidrWizardManager GetNewEidrWizardManager(JobDeclaration declaration)
			{
				return new EidrWizardManagerForTest(declaration, simulateUserClickGo);
			}
			readonly bool simulateUserClickGo;
		}
	}
}
