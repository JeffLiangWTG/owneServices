using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
#if NETCOREAPP
using System.Runtime.Loader;
#endif
using CargoWise.Bi.Common;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.SqlClr.Registration;
using CargoWise.Data.Testing;
using CargoWise.IO;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Microsoft.CSharp;

namespace Enterprise.DbUpgrader.Assemblies.Testing
{
	sealed class DatabaseAssembliesUpgraderTest : BaseUpgraderTestCase
	{
		public void TestMainDatabasesHasClrObjects()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var assemblyList = GetAssemblyList(connection);

				var x = RegisteredAssemblies.MainClrObjects.Select(o => o.ToString()).ToArray();
				AssertContainsExactElementsInAnyOrder(string.Format("CLR objects not deployed correctly on {0}.", Db.DatabaseName), x, assemblyList.ToArray());
			}
		}

		public void TestAuditDatabaseHasClrObjects()
		{
			string auditServer = null;
			using (var conn = Db.NewAdminConnection())
			{
				auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(conn);
			}

			using (var biConnection = Db.NewAdminConnection(auditServer, Db.AuditDatabaseName))
			{
				var assemblyList = GetAssemblyList(biConnection);

				AssertContainsExactElementsInAnyOrder(string.Format("CLR objects not deployed correctly on {0}.", Db.AuditDatabaseName), RegisteredAssemblies.AuditClrObjects.Select(o => o.ToString()).ToArray(), assemblyList.ToArray());
			}
		}

		public void TestEdwDatabaseHasClrObjects()
		{
			string dataWarehouseServer = null;
			using (var conn = Db.NewAdminConnection())
			{
				dataWarehouseServer = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(conn);
			}

			using (var biConnection = Db.NewAdminConnection(dataWarehouseServer, Db.EdwDatabaseName))
			{
				var assemblyList = GetAssemblyList(biConnection);

				AssertContainsExactElementsInAnyOrder(string.Format("CLR objects not deployed correctly on {0}.", Db.EdwDatabaseName), RegisteredAssemblies.EdwClrObjects.Select(o => o.ToString()).ToArray(), assemblyList.ToArray());
			}
		}

		List<string> GetAssemblyList(AdminConnection connection)
		{
			var assemblyList = new List<string>();
			var sqlText = @"
SELECT asm.name, asmd.assembly_class, asmd.assembly_method
FROM sys.assemblies as asm
INNER JOIN sys.assembly_modules as asmd ON asm.assembly_id = asmd.assembly_id
WHERE asm.is_visible = 1";
			using (var reader = connection.Command(sqlText).ExecuteReader())
			{
				while (reader.Read())
				{
					var assemblyName = reader.GetString(0);
					var className = reader.GetString(1);
					var memberName = reader.IsDBNull(2) ? null : reader.GetString(2);

					var assembly = assemblyName + "." + className + (memberName == null ? "" : "." + memberName);

					assemblyList.Add(assembly);
				}
			}
			return assemblyList;
		}

		public void TestClrsAreRegisteredInitially()
		{
			// Arrange
			AssertNoSqlAssemblies();
			Compile("Testing.Foo.Bar.Test", "Enterprise.DbUpgrader.Assemblies.Testing.TestFiles.Foo.Bar.Test.0.cs");

			try
			{
				// Act
				var output = UpgradeInSecondAppDomain(GetNewClrObjectInfo(ClrObjectType.Function, "Testing.Foo.Bar.Test", "Foo.Bar", "Test"));

				AssertOutput(output,
					new AssemblyChange("Testing.Foo.Bar.Test", DatabaseAction.Create),
					new ClrObjectChange("Test", DatabaseAction.Create));

				// Assert
				Assert("HasStoredAssemblies",
					new DatabaseAssembliesUpgraderForTest(new UpgradeManagerForTestWithOutputBuffer(), Db.Connection, null, new VersionLabel(0, 0)).HasStoredAssemblies);
				AssertEquals("sql function ouput", "zero", Db.Connection.ExecuteScalar("select dbo.Test()"));
			}
			finally
			{
				DeleteAssembly("Testing.Foo.Bar.Test");
			}
		}

		public void TestClrsAreDeleted()
		{
			// Arrange
			AssertNoSqlAssemblies();
			Compile("Testing.Foo.Bar.Test", "Enterprise.DbUpgrader.Assemblies.Testing.TestFiles.Foo.Bar.Test.0.cs");
			List<string> output;

			try
			{
				UpgradeInSecondAppDomain(new VersionLabel(-1, -1), GetNewClrObjectInfo(ClrObjectType.Function, "Testing.Foo.Bar.Test", "Foo.Bar", "Test"));
			}
			finally
			{
				DeleteAssembly("Testing.Foo.Bar.Test");
			}

			// Act
			output = UpgradeInSecondAppDomain();

			// Assert
			AssertOutput(output,
				new AssemblyChange("Testing.Foo.Bar.Test", DatabaseAction.Drop),
				new ClrObjectChange("Test", DatabaseAction.Drop));
			AssertNoSqlAssemblies();
		}

		public void TestClrsAreNotAlteredIfVersionNotChanged()
		{
			// Arrange
			AssertNoSqlAssemblies();
			Compile("Testing.Foo.Bar.Test", "Enterprise.DbUpgrader.Assemblies.Testing.TestFiles.Foo.Bar.Test.0.cs");

			try
			{
				UpgradeInSecondAppDomain(GetNewClrObjectInfo(ClrObjectType.Function, "Testing.Foo.Bar.Test", "Foo.Bar", "Test"));

				// Act
				var output = UpgradeInSecondAppDomain(SqlClrAssembliesVersion.Application, GetNewClrObjectInfo(ClrObjectType.Function, "Testing.Foo.Bar.Test", "Foo.Bar", "Test"));

				// Assert
				AssertOutput(output);
			}
			finally
			{
				DeleteAssembly("Testing.Foo.Bar.Test");
			}
		}

		public void TestClrsAreDroppedAndCreatedIfVersionChanged()
		{
			// Arrange
			AssertNoSqlAssemblies();
			Compile("Testing.Foo.Bar.Test", "Enterprise.DbUpgrader.Assemblies.Testing.TestFiles.Foo.Bar.Test.0.cs");

			try
			{
				UpgradeInSecondAppDomain(GetNewClrObjectInfo(ClrObjectType.Function, "Testing.Foo.Bar.Test", "Foo.Bar", "Test"));

				// Act
				var output = UpgradeInSecondAppDomain(new VersionLabel(-1, -1), GetNewClrObjectInfo(ClrObjectType.Function, "Testing.Foo.Bar.Test", "Foo.Bar", "Test"));

				// Assert
				AssertOutput(output,
					new AssemblyChange("Testing.Foo.Bar.Test", DatabaseAction.Drop),
					new ClrObjectChange("Test", DatabaseAction.Drop),
					new AssemblyChange("Testing.Foo.Bar.Test", DatabaseAction.Create),
					new ClrObjectChange("Test", DatabaseAction.Create));
			}
			finally
			{
				DeleteAssembly("Testing.Foo.Bar.Test");
			}
		}

		public void TestClrsAreDroppedAndCreatedIfClrsIsMissingRegardlessOfVersion()
		{
			AssertNoSqlAssemblies();
			Compile("Testing.Foo.Bar.Test", "Enterprise.DbUpgrader.Assemblies.Testing.TestFiles.Foo.Bar.Test.0.cs");

			try
			{
				UpgradeInSecondAppDomain(GetNewClrObjectInfo(ClrObjectType.Function, "Testing.Foo.Bar.Test", "Foo.Bar", "Test"));
				AssertEquals("sql function ouput", "zero", Db.Connection.ExecuteScalar("select dbo.Test()"));

				Db.Connection.ExecuteScalar("DROP FUNCTION dbo.Test");
				var output = UpgradeInSecondAppDomain(SqlClrAssembliesVersion.Application, GetNewClrObjectInfo(ClrObjectType.Function, "Testing.Foo.Bar.Test", "Foo.Bar", "Test"));

				AssertOutput(output,
					new AssemblyChange("Testing.Foo.Bar.Test", DatabaseAction.Drop),
					new AssemblyChange("Testing.Foo.Bar.Test", DatabaseAction.Create),
					new ClrObjectChange("Test", DatabaseAction.Create));
				AssertEquals("sql function ouput", "zero", Db.Connection.ExecuteScalar("select dbo.Test()"));
			}
			finally
			{
				DeleteAssembly("Testing.Foo.Bar.Test");
			}
		}

		public void TestTypes()
		{
			AssertNoSqlAssemblies();

			Compile("Testing.SqlAsmTypesTest", "Enterprise.DbUpgrader.Assemblies.Testing.TestFiles.SqlAsmTypesTests.cs");
			try
			{
				var output = UpgradeInSecondAppDomain(
					GetNewClrObjectInfo(ClrObjectType.Function, "Testing.SqlAsmTypesTest", "SqlAsmTypesTest", "TestBinToGuid"),
					GetNewClrObjectInfo(ClrObjectType.Function, "Testing.SqlAsmTypesTest", "SqlAsmTypesTest", "TestGuidToBin"),
					GetNewClrObjectInfo(ClrObjectType.Function, "Testing.SqlAsmTypesTest", "SqlAsmTypesTest", "TestInts"),
					GetNewClrObjectInfo(ClrObjectType.Function, "Testing.SqlAsmTypesTest", "SqlAsmTypesTest", "TestDateTime"),
					GetNewClrObjectInfo(ClrObjectType.Function, "Testing.SqlAsmTypesTest", "SqlAsmTypesTest", "TestDecimalBoolean"),
					GetNewClrObjectInfo(ClrObjectType.Function, "Testing.SqlAsmTypesTest", "SqlAsmTypesTest", "TestDouble"));
				AssertOutput(output,
					new AssemblyChange("Testing.SqlAsmTypesTest", DatabaseAction.Create),
					new ClrObjectChange("TestBinToGuid", DatabaseAction.Create),
					new ClrObjectChange("TestGuidToBin", DatabaseAction.Create),
					new ClrObjectChange("TestInts", DatabaseAction.Create),
					new ClrObjectChange("TestDateTime", DatabaseAction.Create),
					new ClrObjectChange("TestDecimalBoolean", DatabaseAction.Create),
					new ClrObjectChange("TestDouble", DatabaseAction.Create));

				var guid = new Guid();
				using (var cmd = Db.Connection.Command("select dbo.TestBinToGuid(@Input)"))
				{
					cmd.AddParameter("Input", SqlDbType.VarBinary, guid.ToByteArray());
					AssertEquals(cmd.CommandText, guid, cmd.ExecuteScalar());
				}

				using (var cmd = Db.Connection.Command("select dbo.TestGuidToBin(@Input)"))
				{
					cmd.AddParameter("Input", SqlDbType.UniqueIdentifier, guid);
					AssertEquals(cmd.CommandText, guid, new Guid((byte[])cmd.ExecuteScalar()));
				}

				using (var cmd = Db.Connection.Command("select dbo.TestInts(@oneInt, @twoInt)"))
				{
					cmd.AddParameter("oneInt", SqlDbType.SmallInt, short.MaxValue);
					cmd.AddParameter("twoInt", SqlDbType.Int, int.MaxValue);
					AssertEquals(cmd.CommandText, (short.MaxValue + (long)int.MaxValue) * 2, cmd.ExecuteScalar());
				}

				using (var cmd = Db.Connection.Command("select dbo.TestDateTime('2013-02-26 16:16:00')"))
				{
					AssertEquals(cmd.CommandText, new DateTime(2013, 02, 26), cmd.ExecuteScalar());
				}

				using (var cmd = Db.Connection.Command("select dbo.TestDecimalBoolean(1.61803399, @golden)"))
				{
					cmd.AddParameter("golden", SqlDbType.Decimal, 1.61803399);
					Assert(cmd.CommandText, (bool)cmd.ExecuteScalar());
				}

				using (var cmd = Db.Connection.Command("select dbo.TestDecimalBoolean(3.14159, @golden)"))
				{
					cmd.AddParameter("golden", SqlDbType.Decimal, 1.61803399);
					Assert(cmd.CommandText, !(bool)cmd.ExecuteScalar());
				}

				using (var cmd = Db.Connection.Command("select dbo.TestDouble(1, 2)"))
				{
					AssertEquals(cmd.CommandText, 0.5, cmd.ExecuteScalar());
				}
			}
			finally
			{
				DeleteAssembly("Testing.SqlAsmTypesTest");
			}
		}

		public void TestChangeAssembly()
		{
			AssertNoSqlAssemblies();

			Compile("Testing.Foo.Bar.Test", "Enterprise.DbUpgrader.Assemblies.Testing.TestFiles.Foo.Bar.Test.0.cs");
			try
			{
				var output = UpgradeInSecondAppDomain(GetNewClrObjectInfo(ClrObjectType.Function, "Testing.Foo.Bar.Test", "Foo.Bar", "Test"));
				AssertOutput(output,
					new AssemblyChange("Testing.Foo.Bar.Test", DatabaseAction.Create),
					new ClrObjectChange("Test", DatabaseAction.Create));
				AssertEquals("sql function ouput", "zero", Db.Connection.ExecuteScalar("select dbo.Test()"));
			}
			finally
			{
				DeleteAssembly("Testing.Foo.Bar.Test");
			}

			Compile("Testing.Foo.Bar.Test.New", "Enterprise.DbUpgrader.Assemblies.Testing.TestFiles.Foo.Bar.Test.0.cs");
			try
			{
				var output = UpgradeInSecondAppDomain(GetNewClrObjectInfo(ClrObjectType.Function, "Testing.Foo.Bar.Test.New", "Foo.Bar", "Test"));
				AssertOutput(output,
					new AssemblyChange("Testing.Foo.Bar.Test", DatabaseAction.Drop),
					new AssemblyChange("Testing.Foo.Bar.Test.New", DatabaseAction.Create),
					new ClrObjectChange("Test", DatabaseAction.Drop),
					new ClrObjectChange("Test", DatabaseAction.Create));
				AssertEquals("sql function ouput", "zero", Db.Connection.ExecuteScalar("select dbo.Test()"));
			}
			finally
			{
				DeleteAssembly("Testing.Foo.Bar.Test.New");
			}
		}

		public void TestInvalidRegisteredSqlFunctionExceptions()
		{
			AssertNoSqlAssemblies();

			Compile("Testing.SqlAsmExceptions", "Enterprise.DbUpgrader.Assemblies.Testing.TestFiles.SqlAsmExceptionsTest.cs", "Enterprise.DbUpgrader.Assemblies.Testing.TestFiles.Foo.Bar.Test.0.cs");
			try
			{
				var noFail = GetNewClrObjectInfo(ClrObjectType.Function, "Testing.SqlAsmExceptions", "Foo.Bar", "Test");
				var fail = GetNewClrObjectInfo(ClrObjectType.Function, "Testing.SqlAsmExceptions", "No.Such.Class", "Function");
				AssertInvalidRegisteredSqlFunction(fail, "Type not found.", noFail, fail);

				fail = GetNewClrObjectInfo(ClrObjectType.Function, "Testing.SqlAsmExceptions", "Foo.Bar", "NoSuchFunction");
				AssertInvalidRegisteredSqlFunction(fail, "Method not found.", noFail, fail);

				fail = GetNewClrObjectInfo(ClrObjectType.Function, "Testing.SqlAsmExceptions", "SqlAsmExceptionsTest", "NoSqlFunctionAttribute");
				AssertInvalidRegisteredSqlFunction(fail, "Registered SQL functions must have one and only one SqlFunctionAttribute.", noFail, fail);

				fail = GetNewClrObjectInfo(ClrObjectType.Function, "Testing.SqlAsmExceptions", "SqlAsmExceptionsTest", "NoSqlFunctionAttributeName");
				AssertInvalidRegisteredSqlFunction(fail, "Registered SQL functions must define a SqlFunctionAttribute.Name value.", noFail, fail);

				fail = GetNewClrObjectInfo(ClrObjectType.Function, "Testing.SqlAsmExceptions", "SqlAsmExceptionsTest", "TestAmbiguousMatch");
				AssertInvalidRegisteredSqlFunction(fail, "Ambiguous match found.", noFail, fail);

				fail = GetNewClrObjectInfo(ClrObjectType.Function, "Testing.SqlAsmExceptions", "SqlAsmExceptionsTest", "TestTypeMapping");
				AssertInvalidRegisteredSqlFunction(fail, "No defined mapping of the type 'System.Object' to an SqlDbType", noFail, fail);
			}
			finally
			{
				DeleteAssembly("Testing.SqlAsmExceptions");
			}
		}

		public void TestAssemblyFunctionDependencies()
		{
			// Arrange
			AssertNoSqlAssemblies();

			Compile("Testing.Foo.Bar.Test", "Enterprise.DbUpgrader.Assemblies.Testing.TestFiles.Foo.Bar.Test.0.cs");
			List<string> output;
			try
			{
				output = UpgradeInSecondAppDomain(new VersionLabel(-1, -1), GetNewClrObjectInfo(ClrObjectType.Function, "Testing.Foo.Bar.Test", "Foo.Bar", "Test"));
				AssertOutput(output,
					new AssemblyChange("Testing.Foo.Bar.Test", DatabaseAction.Create),
					new ClrObjectChange("Test", DatabaseAction.Create));
				Assert("HasStoredAssemblies",
					new DatabaseAssembliesUpgraderForTest(new UpgradeManagerForTestWithOutputBuffer(), Db.Connection, null, new VersionLabel(0, 0)).HasStoredAssemblies);

				Db.Connection.ExecuteNonQuery("CREATE FUNCTION dbo.TestDependentFunction() RETURNS nvarchar(max) WITH SCHEMABINDING AS BEGIN RETURN dbo.Test() END");

				AssertEquals("sql function ouput", "zero", Db.Connection.ExecuteScalar("select dbo.Test()"));

				Compile("Testing.Foo.Bar.Test", "Enterprise.DbUpgrader.Assemblies.Testing.TestFiles.Foo.Bar.Test.1.cs");

				// Act
				output = UpgradeInSecondAppDomain(GetNewClrObjectInfo(ClrObjectType.Function, "Testing.Foo.Bar.Test", "Foo.Bar", "Test"));

				// Assert
				AssertOutput(output,
					new AssemblyChange("Testing.Foo.Bar.Test", DatabaseAction.Drop),
					new ClrObjectChange("Test", DatabaseAction.Drop),
					new AssemblyChange("Testing.Foo.Bar.Test", DatabaseAction.Create),
					new ClrObjectChange("Test", DatabaseAction.Create));
				AssertEquals("sql function ouput", "one", Db.Connection.ExecuteScalar("select dbo.Test()"));
			}
			finally
			{
				DeleteAssembly("Testing.Foo.Bar.Test");
			}
		}

		public void TestLoadDbAssembliesErrorMessage()
		{
			messages = null;
			AssertNoSqlAssemblies();

			Compile("Testing.Foo.Bar.Test", "Enterprise.DbUpgrader.Assemblies.Testing.TestFiles.Foo.Bar.Test.0.cs");
			try
			{
				_ = UpgradeInSecondAppDomain(new VersionLabel(-1, -1), sqlAssembliesErrorTest: true, GetNewClrObjectInfo(ClrObjectType.Function, "Testing.Foo.Bar.Test", "Foo.Bar", "Test"));
				AssertNotNull(messages);
				Assert(messages.Contains(DatabaseAssembliesUpgrader.lockRequestTimeOutString));
			}
			finally
			{
				DeleteAssembly("Testing.Foo.Bar.Test");
			}
		}

		public List<string> messages;

		#region Implementation

		SqlAssemblyClrObjectInfo GetNewClrObjectInfo(ClrObjectType dbObjectType, string assemblyName, string classFullName, string memberName)
		{
			return SqlAssemblyClrObjectInfo.New(dbObjectType, assemblyName, AssemblyPermission.SAFE, classFullName, memberName);
		}

		void AssertOutput(List<string> output, params Change[] expectedChanges)
		{
			var dropStart = output.IndexOf("Dropping out of date assembly CLR Objects");
			var asmStart = output.IndexOf("Modifying assemblies");
			var createStart = output.IndexOf("Creating new assembly CLR Objects");
			var end = output.IndexOf("SQL Assemblies Upgrade (database: TestForAssembliesUpgrader) completed.");
			var notRequired = output.IndexOf("SQL Assemblies Upgrade (database: TestForAssembliesUpgrader) not required.");

			if (expectedChanges == null || expectedChanges.Length == 0)
			{
				Assert("Unexpected ouput: Dropping out of date assembly CLR Objects", dropStart == -1);
				Assert("Unexpected ouput: Modifying assemblies", asmStart == -1);
				Assert("Unexpected ouput: Creating new assembly CLR Objects", createStart == -1);
				Assert("Unexpected output: SQL Assemblies Upgrade completed.", end == -1);
				Assert("Unexpected output: SQL Assemblies Upgrade not required.", notRequired > -1);
			}
			else
			{
				Assert("Expected ouput: Dropping out of date assembly CLR Objects", dropStart > -1);
				Assert("Expected ouput: Modifying assemblies", asmStart > -1);
				Assert("Expected ouput: Creating new assembly CLR Objects", createStart > -1);
				Assert("Expected output: SQL Assemblies Upgrade completed.", end > -1);
				Assert("Unexpected output: SQL Assemblies Upgrade not required.", notRequired == -1);

				var actualDrop = new List<string>();
				for (var i = dropStart + 1; i < asmStart; i++)
				{
					actualDrop.Add(output[i]);
				}

				var actualAsm = new List<string>();
				for (var i = asmStart + 1; i < createStart; i++)
				{
					actualAsm.Add(output[i]);
				}

				var actualCreate = new List<string>();
				for (var i = createStart + 1; i < end - 1; i++)
				{
					actualCreate.Add(output[i]);
				}

				var expectedDrop = new List<string>();
				var expectedAsm = new List<string>();
				var expectedCreate = new List<string>();
				foreach (var change in expectedChanges)
				{
					if (change is AssemblyChange)
					{
						expectedAsm.Add(DatabaseAssembliesUpgrader.ChangeIndicator(change.action) + " Assembly " + change.Name);
					}
					else
					{
						if (change.action == DatabaseAction.Alter || change.action == DatabaseAction.Drop)
						{
							expectedDrop.Add(DatabaseAssembliesUpgrader.ChangeIndicator(change.action) + " CLR Object " + change.Name);
						}

						if (change.action == DatabaseAction.Alter || change.action == DatabaseAction.Create)
						{
							expectedCreate.Add(DatabaseAssembliesUpgrader.ChangeIndicator(change.action) + " CLR Object " + (((ClrObjectChange)change).newName ?? change.Name));
						}
					}
				}

				AssertContainsExactElementsInAnyOrder("Dropping out of date assembly CLR Objects", expectedDrop, actualDrop);
				AssertContainsExactElementsInAnyOrder("Modifying assemblies", expectedAsm, actualAsm);
				AssertContainsExactElementsInAnyOrder("Creating new assembly CLR Objects", expectedCreate, actualCreate);
			}
		}

		class Change
		{
			internal Change(string name, DatabaseAction action)
			{
				Name = name;
				this.action = action;
			}

			internal readonly DatabaseAction action;

			internal readonly string Name;
		}

		class AssemblyChange : Change
		{
			internal AssemblyChange(string name, DatabaseAction action)
				: base(name, action)
			{
			}
		}

		class ClrObjectChange : Change
		{
			internal ClrObjectChange(string name, DatabaseAction action)
				: base(name, action)
			{
			}

			internal ClrObjectChange(string name, string newName, DatabaseAction action)
				: base(name, action)
			{
				this.newName = newName;
			}

			internal readonly string newName;
		}

		void AssertNoSqlAssemblies()
		{
			Assert("HasStoredAssemblies",
				!new DatabaseAssembliesUpgraderForTest(new UpgradeManagerForTestWithOutputBuffer(), Db.Connection, null, new VersionLabel(0, 0)).HasStoredAssemblies);
		}

		void AssertInvalidRegisteredSqlFunction(SqlAssemblyClrObjectInfo expectedToFail, string expectedReason, params SqlAssemblyClrObjectInfo[] functions)
		{
			Exception exception = null;
			try
			{
				UpgradeInSecondAppDomain(functions);
			}
			catch (Exception ex)
			{
				exception = ex;
			}

			AssertNotNull("Exception", exception);
			AssertNotNull("Exception.InnerException", exception.InnerException);
			AssertType(typeof(InvalidRegisteredSqlClrObjectException), exception.InnerException);
			AssertEquals("Exception.InnerException.Message", string.Format("Invalid registered SQL Object {0}. {1}", expectedToFail.ToString(), expectedReason), exception.InnerException.Message);
			AssertNoSqlAssemblies();
		}

		void Compile(string outputAssembly, params string[] embeddedResources)
		{
			var embeddedResourcesLength = embeddedResources.Length;
			var codeFiles = new string[embeddedResourcesLength];
			var assembly = Assembly.GetCallingAssembly();
			if (!Directory.Exists(DirectoryPath.Value))
			{
				Directory.CreateDirectory(DirectoryPath.Value);
			}
			for (var i = 0; i < embeddedResourcesLength; i++)
			{
				codeFiles[i] = SaveResourceToFile(assembly, embeddedResources[i]);
			}

			var compiler = new CSharpCodeProvider(new Dictionary<string, string> { { "CompilerVersion", "v3.5" } });
			var options = new CompilerParameters();
			options.ReferencedAssemblies.Add("System.dll");
			options.ReferencedAssemblies.Add("System.Data.dll");
			options.ReferencedAssemblies.Add("System.Xml.dll");
			options.OutputAssembly = GetAssemblyFilePath(outputAssembly);
			var result = compiler.CompileAssemblyFromFile(options, codeFiles);
			var output = new string[result.Output.Count];
			result.Output.CopyTo(output, 0);
			AssertEquals(string.Join("\r\n", output), 0, result.Errors.Count);
		}

		string SaveResourceToFile(Assembly assembly, string resourceName)
		{
			var filePath = Path.Combine(DirectoryPath.Value, resourceName);
			using (var fileStream = File.Create(filePath))
			{
				assembly.GetManifestResourceStream(resourceName).CopyTo(fileStream);
			}
			return filePath;
		}

		static string GetAssemblyFilePath(string assemblyName)
		{
			return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), assemblyName + ".dll");
		}

		static void DeleteAssembly(string assemblyName)
		{
			if (File.Exists(GetAssemblyFilePath(assemblyName)))
			{
				File.Delete(GetAssemblyFilePath(assemblyName));
			}
		}

		List<string> UpgradeInSecondAppDomain(params SqlAssemblyClrObjectInfo[] registeredAssemblyClrObjects)
		{
			return UpgradeInSecondAppDomain(new VersionLabel(0, 0), registeredAssemblyClrObjects);
		}

		List<string> UpgradeInSecondAppDomain(VersionLabel version, params SqlAssemblyClrObjectInfo[] registeredAssemblyClrObjects)
		{
			return UpgradeInSecondAppDomain(version, sqlAssembliesErrorTest: false, registeredAssemblyClrObjects);
		}

		List<string> UpgradeInSecondAppDomain(VersionLabel version, bool sqlAssembliesErrorTest, params SqlAssemblyClrObjectInfo[] registeredAssemblyClrObjects)
		{
#if NETCOREAPP
			var context = new AssemblyLoadContext("AssembliesUpgraderTest", isCollectible: true);
			try
			{
				var dbServerName = Db.ServerName;
				var dbDatabaseName = Db.DatabaseName;
				var callBackClass = new CallBackClass(version.Major, version.Minor, dbServerName, dbDatabaseName, testDbName, sqlAssembliesErrorTest, registeredAssemblyClrObjects);

				callBackClass.CallBack();

				if (sqlAssembliesErrorTest)
				{
					messages = callBackClass.Messages;
				}

				return callBackClass.UpgradeOutput;
			}
			finally
			{
				context.Unload();
			}
#else
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var appDomain = AppDomain.CreateDomain("AssembliesUpgraderTest");
			try
			{
				appDomain.SetData("DbServerName", Db.ServerName);
				appDomain.SetData("DbDatabaseName", Db.DatabaseName);
				appDomain.SetData("testDbName", testDbName);
				appDomain.SetData("registeredAssemblyClrObjects", registeredAssemblyClrObjects);
				appDomain.SetData("sqlAssembliesBool", sqlAssembliesErrorTest);
				var callBackClass = new CallBackClass(version.Major, version.Minor);
				appDomain.DoCallBack(callBackClass.CallBack);
				if (sqlAssembliesErrorTest)
				{
					messages = (List<string>)appDomain.GetData("messages");
				}

				return (List<string>)appDomain.GetData("upgradeOutput");
			}
			finally
			{
				AppDomain.Unload(appDomain);
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
#endif
		}

		[Serializable]
		class CallBackClass
		{
#if NETCOREAPP
			public List<string> Messages { get; private set; }
			public List<string> UpgradeOutput { get; private set; }

			public CallBackClass(int versionMajor, int versionMinor, string dbServerName, string dbDatabaseName, string testDbName, bool sqlAssembliesErrorTest, params SqlAssemblyClrObjectInfo[] registeredAssemblyClrObjects)
			{
				this.dbServerName = dbServerName;
				this.dbDatabaseName = dbDatabaseName;
				databaseName = testDbName;
				sqlAssembliesBool = sqlAssembliesErrorTest;
				this.registeredAssemblyClrObjects = registeredAssemblyClrObjects;
				this.versionMajor = versionMajor;
				this.versionMinor = versionMinor;
			}

			public void CallBack()
			{
				Db.InitializeDatabaseDetails(dbServerName, dbDatabaseName);

				var upg = new UpgradeManagerForTestWithOutputBuffer();

				using (Db.DisableSchemaVersionCheck())
				using (var targetConnection = Db.NewAdminConnection())
				{
					((ICurrentDbControl)Db.Connection).UseDatabase(databaseName);
					((ICurrentDbControl)targetConnection).UseDatabase(databaseName);
					var upgrader = new DatabaseAssembliesUpgraderForTest(upg, Db.Connection, null, new VersionLabel(versionMajor, versionMinor), registeredAssemblyClrObjects);
					upgrader.sqlAssembliesErrorTest = sqlAssembliesBool;
					try
					{
						upgrader.RunUpgrade();
					}
					catch (Exception ex)
					{
						if (sqlAssembliesBool && ex.InnerException is SqlException)
						{
							Messages = upgrader.ManagerExposedForTest.OutputTextCollection;
						}
						else
						{
							throw;
						}
					}
				}
				UpgradeOutput = upg.OutputTextCollection;
			}

			readonly int versionMajor;
			readonly int versionMinor;
			readonly string databaseName;
			readonly string dbServerName;
			readonly string dbDatabaseName;
			readonly bool sqlAssembliesBool;
			readonly SqlAssemblyClrObjectInfo[] registeredAssemblyClrObjects;
#else
			public CallBackClass(int versionMajor, int versionMinor)
			{
				this.versionMajor = versionMajor;
				this.versionMinor = versionMinor;
			}

			public void CallBack()
			{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
				dbServerName = (string)AppDomain.CurrentDomain.GetData("DbServerName");
				dbDatabaseName = (string)AppDomain.CurrentDomain.GetData("DbDatabaseName");
				databaseName = (string)AppDomain.CurrentDomain.GetData("testDbName");
				sqlAssembliesBool = (bool)AppDomain.CurrentDomain.GetData("sqlAssembliesBool");
				registeredAssemblyClrObjects = (SqlAssemblyClrObjectInfo[])AppDomain.CurrentDomain.GetData("registeredAssemblyClrObjects");
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.

				Db.InitializeDatabaseDetails(dbServerName, dbDatabaseName);

				var upg = new UpgradeManagerForTestWithOutputBuffer();

				using (Db.DisableSchemaVersionCheck())
				using (var targetConnection = Db.NewAdminConnection())
				{
					((ICurrentDbControl)Db.Connection).UseDatabase(databaseName);
					((ICurrentDbControl)targetConnection).UseDatabase(databaseName);
					var upgrader = new DatabaseAssembliesUpgraderForTest(upg, Db.Connection, null, new VersionLabel(versionMajor, versionMinor), registeredAssemblyClrObjects);
					upgrader.sqlAssembliesErrorTest = sqlAssembliesBool;
					try
					{
						upgrader.RunUpgrade();
					}
					catch (Exception ex)
					{
						if (sqlAssembliesBool && ex.InnerException is SqlException)
						{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
							AppDomain.CurrentDomain.SetData("messages", upgrader.ManagerExposedForTest.OutputTextCollection);
						}
						else
						{
							throw;
						}
					}
				}
				AppDomain.CurrentDomain.SetData("upgradeOutput", upg.OutputTextCollection);
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
			}

			readonly int versionMajor;
			readonly int versionMinor;
			string databaseName;
			string dbServerName;
			string dbDatabaseName;
			bool sqlAssembliesBool;
			SqlAssemblyClrObjectInfo[] registeredAssemblyClrObjects;
#endif
		}

		protected override void SetUp()
		{
			using (var conn = Db.NewAdminConnection())
			{
				conn.ExecuteNonQuery(string.Format("IF EXISTS (SELECT name FROM sys.databases WHERE name = '{0}') DROP DATABASE {0}", testDbName));
				conn.CreateDatabase(
					testDbName,
					actionToPerformAfterCreatingDb: dbName =>
					{
						DataUtils.AlterDbAuthorisation(conn, dbName);
						DataUtils.SetTrustworthyOn(conn, dbName);
					});
				CreateStmDataTable(conn, testDbName);
			}

			usingDatabase = ((ICurrentDbControl)Db.Connection).UseDatabase(testDbName);
		}

		void CreateStmDataTable(AdminConnection conn, string dbName)
		{
			var sql = string.Format(@"
					-- StmData
					if (OBJECT_ID('[{0}].dbo.StmData', 'U') is NULL)
					begin
						CREATE TABLE [{0}].dbo.StmData
						(
							  SD_PK UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
							  SD_Name VARCHAR(300) NULL,
							  SD_Owner UNIQUEIDENTIFIER NULL,
							  SD_DepartmentGuid UNIQUEIDENTIFIER NULL,
							  SD_Type CHAR(3) NULL,
							  SD_BinaryValue VARBINARY(MAX) NULL,
							  SD_GuidValue UNIQUEIDENTIFIER NULL,
						);
					end",
				testDbName);

			using (var cmd = conn.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
		}

		protected override void TearDown()
		{
			if (usingDatabase != null)
			{
				usingDatabase.Dispose();
			}

			using (DbConnection conn = Db.NewAdminConnection())
			{
				conn.ExecuteNonQuery(string.Format("DROP DATABASE {0}", testDbName));
			}

			if (DirectoryPath.IsValueCreated && Directory.Exists(DirectoryPath.Value))
			{
				Directory.Delete(DirectoryPath.Value, recursive: true);
			}
		}

		public override BaseUpgrader GetNewUpgrader(BaseUpgraderUpgradeManagerForTesting dummyUpgradeManager)
		{
			AssertNoSqlAssemblies();

			Compile("Testing.Foo.Bar.Test", "Enterprise.DbUpgrader.Assemblies.Testing.TestFiles.Foo.Bar.Test.0.cs");

			var registeredClrObjects = GetNewClrObjectInfo(ClrObjectType.Function, "Testing.Foo.Bar.Test", "Foo.Bar", "Test");
			return new AppDomainAssembliesUpgraderWrapper(dummyUpgradeManager, Db.Connection, new VersionLabel(0, 0), registeredClrObjects);
		}

		const string testDbName = "TestForAssembliesUpgrader";
		IDisposable usingDatabase;
		readonly Lazy<string> DirectoryPath = new Lazy<string>(() => Path.Combine(Temp.TempPath, Guid.NewGuid().ToString()));

		class DatabaseAssembliesUpgraderForTest : DatabaseAssembliesUpgrader
		{
			public DatabaseAssembliesUpgraderForTest(IUpgradeManager manager, DbConnection upgConnection, DbConnection targetConnection, VersionLabel versionBeforeUpgrade, params SqlAssemblyClrObjectInfo[] registeredClrObjects)
				: base(manager, upgConnection, versionBeforeUpgrade, registeredClrObjects)
			{
				this.targetConnection = targetConnection;
			}

			protected override void DoUpgrade()
			{
				if (targetConnection == null)
				{
					base.DoUpgrade();
				}
				else
				{
					UpgradeSpecificDatabase(DbAssemblies, targetConnection);
				}
			}

			private protected override SqlAssemblies LoadDbAssemblies()
			{
				return targetConnection == null
					? base.LoadDbAssemblies()
					: LoadModel(upgConnection, RegisteredClrObjects);
			}

			private protected override SqlAssemblies LoadModel(DbConnection connection, List<SqlAssemblyClrObjectInfo> clrObjects)
			{
				if (sqlAssembliesErrorTest)
				{
					throw SqlExceptionBuilder.CreateSqlException(1222, "Lock request time out period exceeded.");
				}
				return base.LoadModel(upgConnection, RegisteredClrObjects);
			}

			public UpgradeManagerForTestWithOutputBuffer ManagerExposedForTest => (UpgradeManagerForTestWithOutputBuffer)Manager;

			readonly DbConnection targetConnection;
			public bool sqlAssembliesErrorTest;
		}

		class AppDomainAssembliesUpgraderWrapper : BaseUpgrader
		{
			public AppDomainAssembliesUpgraderWrapper(IUpgradeManager manager, DbConnection upgConnection, VersionLabel versionBeforeUpgrade,
				params SqlAssemblyClrObjectInfo[] registeredClrObjects)
				: base(manager, upgConnection, versionBeforeUpgrade)
			{
				this.manager = manager;
				this.registeredClrObjects = registeredClrObjects;
			}

			DatabaseAssembliesUpgrader ReferenceUpgrader => referenceUpgrader ?? (referenceUpgrader = new DatabaseAssembliesUpgraderForTest(manager, upgConnection, null, versionBeforeUpgrade, registeredClrObjects));

			public override int EstimatedNumberOfTasks => ReferenceUpgrader.EstimatedNumberOfTasks;

			public override string Name => ReferenceUpgrader.Name;

			public override IEnumerable<string> SecondaryDatabasesToUpgrade => ReferenceUpgrader.SecondaryDatabasesToUpgrade;

			protected override VersionLabel LatestVersion => new VersionLabel(0, 0);

			protected override void DoUpgrade()
			{
#if NETCOREAPP
				var context = new AssemblyLoadContext("AssembliesUpgraderTest", isCollectible: true);
				try
				{
					var dbServerName = Db.ServerName;
					var dbDatabaseName = Db.DatabaseName;
					var callBackClass = new UpgradeCallbackClass(dbServerName, dbDatabaseName, testDbName, registeredClrObjects);

					callBackClass.CallBack();

					foreach (var t in callBackClass.Tasks)
					{
						((BaseUpgraderUpgradeManagerForTesting)manager).tasks.Add(t);
					}
				}
				finally
				{
					context.Unload();
				}
#else
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
				var appDomain = AppDomain.CreateDomain("AssembliesUpgraderTest");
				try
				{
					appDomain.SetData("DbServerName", Db.ServerName);
					appDomain.SetData("DbDatabaseName", Db.DatabaseName);
					appDomain.SetData("testDbName", "TestForAssembliesUpgrader");
					appDomain.SetData("registeredAssemblyClrObjects", registeredClrObjects);
					var callbackClass = new UpgradeCallbackClass();
					appDomain.DoCallBack(callbackClass.CallBack);

					foreach (var t in (string[])appDomain.GetData("tasks"))
					{
						((BaseUpgraderUpgradeManagerForTesting)manager).tasks.Add(t);
					}
				}
				finally
				{
					AppDomain.Unload(appDomain);
					DeleteAssembly("Testing.Foo.Bar.Test");
				}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
#endif
			}

			[Serializable]
			class UpgradeCallbackClass
			{
#if NETCOREAPP
				public List<string> Tasks { get; private set; }

				public UpgradeCallbackClass(string dbServerName, string dbDatabaseName, string testDbName, params SqlAssemblyClrObjectInfo[] registeredAssemblyClrObjects)
				{
					this.dbServerName = dbServerName;
					this.dbDatabaseName = dbDatabaseName;
					databaseName = testDbName;
					this.registeredAssemblyClrObjects = registeredAssemblyClrObjects;
				}

				public void CallBack()
				{
					Db.InitializeDatabaseDetails(dbServerName, dbDatabaseName);

						var localManager = new BaseUpgraderUpgradeManagerForTesting();

						using (Db.DisableSchemaVersionCheck())
						using (var targetConnection = Db.NewAdminConnection())
						using (((ICurrentDbControl)targetConnection).UseDatabase(databaseName))
						{
							((ICurrentDbControl)Db.Connection).UseDatabase(databaseName);
							var upgrader = new DatabaseAssembliesUpgraderForTest(localManager, Db.Connection, targetConnection, new VersionLabel(0, 0), registeredAssemblyClrObjects);
							upgrader.RunUpgrade();
						}
						Tasks = localManager.tasks.ToList();
					}

									readonly string databaseName;
				readonly string dbServerName;
				readonly string dbDatabaseName;
				readonly SqlAssemblyClrObjectInfo[] registeredAssemblyClrObjects;
#else
				public void CallBack()
				{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
					dbServerName = (string)AppDomain.CurrentDomain.GetData("DbServerName");
					dbDatabaseName = (string)AppDomain.CurrentDomain.GetData("DbDatabaseName");
					databaseName = (string)AppDomain.CurrentDomain.GetData("testDbName");
					registeredAssemblyClrObjects = (SqlAssemblyClrObjectInfo[])AppDomain.CurrentDomain.GetData("registeredAssemblyClrObjects");
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
					Db.InitializeDatabaseDetails(dbServerName, dbDatabaseName);

					var localManager = new BaseUpgraderUpgradeManagerForTesting();

					using (Db.DisableSchemaVersionCheck())
					using (var targetConnection = Db.NewAdminConnection())
					using (((ICurrentDbControl)targetConnection).UseDatabase(databaseName))
					{
						((ICurrentDbControl)Db.Connection).UseDatabase(databaseName);
						var upgrader = new DatabaseAssembliesUpgraderForTest(localManager, Db.Connection, targetConnection, new VersionLabel(0, 0), registeredAssemblyClrObjects);
						upgrader.RunUpgrade();
					}
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
					AppDomain.CurrentDomain.SetData("tasks", localManager.tasks.ToArray());
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
				}

				string databaseName;
				string dbServerName;
				string dbDatabaseName;
				SqlAssemblyClrObjectInfo[] registeredAssemblyClrObjects;
#endif
			}

			public override void RunUpgrade()
			{
				Manager.ShowInfoMessage(".");

				try
				{
					if (upgConnection.IsInTransaction || !RequiresTransaction)
					{
						DoUpgrade();
					}
					else
					{
						DoUpgradeInTransaction();
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					throw new Exception(Name + " failed.\r\n" + ex.Message, ex);
				}
			}

			readonly IUpgradeManager manager;
			DatabaseAssembliesUpgrader referenceUpgrader;
			readonly SqlAssemblyClrObjectInfo[] registeredClrObjects;
		}

		#endregion
	}
}
