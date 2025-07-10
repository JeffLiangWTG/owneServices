using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.IO;
using CargoWise.BuildTools.Testing;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Database.Shared;
using CargoWise.DbUpgrader.Scripts.Abstractions;
using NUnit.Framework;

namespace Enterprise.Builder.Generator.Testing
{
	[UseSnapshotProtection]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
	sealed class NoTransactionSingleBizObjGeneratorTest : TestCase
	{
		public void TestGenerateWithClientSpecificScripts()
		{
			var generator = new SingleBizObjGenerator(BaseSourcePath + @"Enterprise\ClientExtensions\UPE\ZClientUPE\AutoClientPWSCharge.cs", outputDirectory);
			AssertNoExceptionThrown(() => generator.Generate());
		}

		public void TestGenerateAll_HRMSchema()
		{
			var generator = new SingleBizObjGenerator(@"AutoGlbStaffRemuneration.cs", outputDirectory, false);
			generator.GenerateAll(outputDirectory);
			AssertEquals(true, File.Exists(Path.Combine(outputDirectory.CWSharedOutputDirectory, @"CargoWise.DbUpgrader\src\Database\CargoWise.Odyssey.Schema\CargoWise.Odyssey.Schema\Schemas\GlbStaffRemunerationSchema.cs")));
			AssertEquals(false, File.Exists(Path.Combine(outputDirectory.DevOutputDirectory, @"Enterprise\Product\Operations\MasterFilters\Business\Global\Staff\AutoGlbStaffRemuneration.cs")));
		}

		public void TestClientTablesCreatedAndDropped()
		{
			TestSingleBizObjGenerator generator = new TestSingleBizObjGenerator(BaseSourcePath + @"Enterprise\ClientExtensions\UPE\ZClientUPE\AutoClientTestBusinessObject.cs", outputDirectory);

			AssertEquals("Client tables shouldn't exist before generation", false, SingleBizObjGeneratorTest.GetTableNamesFromDb().Contains("ClientBISIShipmentHeader"));
			generator.Generate();
			AssertEquals("Client tables should exist while generating source code", true, generator.TablesNamesInDbDuringGeneration.Contains("ClientBISIShipmentHeader"));
			AssertEquals("Client tables shouldn't exist after generation", false, SingleBizObjGeneratorTest.GetTableNamesFromDb().Contains("ClientBISIShipmentHeader"));
		}

		public void TestClientTablesDroppedOnError()
		{
			TestSingleBizObjGenerator generator = new TestSingleBizObjGenerator(BaseSourcePath + @"Enterprise\ClientExtensions\UPE\ZClientUPE\AutoClientTestBusinessObject.cs", outputDirectory);

			AssertEquals("Client tables shouldn't exist before generation", false, SingleBizObjGeneratorTest.GetTableNamesFromDb().Contains("ClientBISIShipmentHeader"));
			generator.ThrowDuringGenerateAll = true;
			try
			{
				generator.Generate();
			}
			catch (InvalidOperationException)
			{
			}
			AssertEquals("Client tables should exist while generating source code", true, generator.TablesNamesInDbDuringGeneration.Contains("ClientBISIShipmentHeader"));
			AssertEquals("Client tables shouldn't exist after generation, even if an error occurred", false, SingleBizObjGeneratorTest.GetTableNamesFromDb().Contains("ClientBISIShipmentHeader"));
		}

		public void TestClientScripts()
		{
			var base_scripts = new List<IDbScript>() { Base_View_Child, Base_View_Parent, };
			var client_scripts = ImmutableArray.Create(Client_View_Child);

			AssertEquals("Precondition: View_Parent", null, GetViewDefinition(View_Parent_Name));
			AssertEquals("Precondition: View_Child", null, GetViewDefinition(View_Child_Name));

			foreach (var script in base_scripts)
			{
				Db.Connection.ExecuteNonQuery(script.Text);
			}

			AssertEquals("View_Parent: Base definition", Base_View_Parent.Text, GetViewDefinition(View_Parent_Name));
			AssertEquals("View_Child: Base definition", Base_View_Child.Text, GetViewDefinition(View_Child_Name));

			var generator = new SingleBizObjGenerator(outputDirectory: null);
			generator.BaseScripts_ForTest = base_scripts;
			var clientInfo = new ExtensionObjects(tableCreationScripts: ImmutableArray<DatabaseObjectCreateScript>.Empty, viewAndRoutineCreationScripts: client_scripts);

			generator.RunClientSpecificTableDropScripts_ForTest(clientInfo, false);
			AssertEquals("View_Parent has been dropped", null, GetViewDefinition(View_Parent_Name));
			AssertEquals("View_Child has been dropped", null, GetViewDefinition(View_Child_Name));

			generator.RunClientSpecificTableCreateScripts_ForTest(clientInfo);
			AssertEquals("View_Parent: Base definition", Base_View_Parent.Text, GetViewDefinition(View_Parent_Name));
			AssertEquals("View_Child: Client definition", Client_View_Child.CreateScript, GetViewDefinition(View_Child_Name));
		}

		#region Implementation

		const string View_Parent_Name = "View_Parent";
		const string View_Child_Name = "View_Child";

		IDbScript Base_View_Parent
		{
			get { return base_View_Parent ?? (base_View_Parent = GetBaseSchemaboundView(View_Parent_Name, 1, View_Child_Name)); }
		}

		IDbScript base_View_Parent;

		IDbScript Base_View_Child
		{
			get { return base_View_Child ?? (base_View_Child = GetBaseSchemaboundView(View_Child_Name, 2)); }
		}

		IDbScript base_View_Child;

		DatabaseViewAndRoutineCreateScript Client_View_Child
		{
			get { return client_View_Child ?? (client_View_Child = GetSchemaboundView(View_Child_Name, 200)); }
		}

		DatabaseViewAndRoutineCreateScript client_View_Child;

		DatabaseViewAndRoutineCreateScript GetSchemaboundView(string name, int value, string childName = null)
		{
			return new DatabaseViewAndRoutineCreateScript(objectName: name
				, createScript: string.Format("CREATE VIEW dbo.{0} WITH SCHEMABINDING AS SELECT value = {1}{2}"
					, name
					, value
					, (string.IsNullOrWhiteSpace(childName)) ? string.Empty : " UNION SELECT value FROM dbo." + childName
					)
				, dropScript: string.Format("DROP VIEW dbo.{0}", name)
				, objectType: DbRoutineType.SqlViewTypeDesc
				);
		}

		IDbScript GetBaseSchemaboundView(string name, int value, string childName = null)
		{
			return new SingleBizObjGenerator.DbScript(Db.SqlDbOwnerSchema, scriptName: name
				, scriptText: string.Format("CREATE VIEW dbo.{0} WITH SCHEMABINDING AS SELECT value = {1}{2}"
					, name
					, value
					, (string.IsNullOrWhiteSpace(childName)) ? string.Empty : " UNION SELECT value FROM dbo." + childName
					)
				, scriptObjectType: DbRoutineType.SqlViewTypeDesc
				);
		}

		string GetViewDefinition(string objectName)
		{
			var full_name = string.Format("[dbo].[{0}]", objectName);
			var sql = "SELECT OBJECT_DEFINITION(OBJECT_ID(@full_name, N'V'));";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@full_name", SqlDbType.NVarChar, 128, full_name);

				var definition = cmd.ExecuteScalar();
				return (definition == null || definition == DBNull.Value)
					? null
					: (string)definition
					;
			}
		}

		GeneratorOutputDirectory outputDirectory;

		protected override void SetUp()
		{
			base.SetUp();
			outputDirectory = new FauxGeneratorOutputDirectory(GeneratorOutputDirectory.SaveMode.CheckOut);
			MockSourceControl.Setup();
		}

		protected override void TearDown()
		{
			base.TearDown();
			outputDirectory.Dispose();
			MockSourceControl.TearDown();
		}

		#endregion // Implementation
	}
}
