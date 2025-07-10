using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class ClientHookLoaderTest : TestCase
	{
		public void TestClient()
		{
			AssertEquals("Should NOT load Client Hook", Clients.None, Loader.Client);

			using (Loader.OverrideClientAssemblyForTest(Clients.TNT))
			{
				AssertEquals("Should load TNT Client Hook", Clients.TNT, Loader.Client);
			}

			AssertEquals("Should NOT load Client Hook", Clients.None, Loader.Client);
		}

		public void TestGetClientCode()
		{
			AssertEquals("EDI", ClientHookLoader.Instance.GetClientCode("ZClientEDI"));
			AssertEquals("EDI", ClientHookLoader.Instance.GetClientCode("ZClientEDI.dll"));
			AssertEquals("EDI", ClientHookLoader.Instance.GetClientCode("ZClientEDI.Test.dll"));
			AssertEquals("EDI", ClientHookLoader.Instance.GetClientCode("ZClientEDI.Test"));
			AssertEquals("EDI", ClientHookLoader.Instance.GetClientCode("ZClientEDI.Business"));
			AssertEquals("EDI", ClientHookLoader.Instance.GetClientCode("ZClientWebEDI.dll"));
			AssertEquals("EDI", ClientHookLoader.Instance.GetClientCode("ZClientWebEDI"));
			AssertEquals("YAS", ClientHookLoader.Instance.GetClientCode("ZClientYAS"));
			AssertEquals("YAS", ClientHookLoader.Instance.GetClientCode("ZClientYAS.dll"));
			AssertEquals("YAS", ClientHookLoader.Instance.GetClientCode("ZClientYAS.Test.dll"));
			AssertEquals("YAS", ClientHookLoader.Instance.GetClientCode("ZClientWebYAS.dll"));
			AssertEquals("YAS", ClientHookLoader.Instance.GetClientCode("ZClientWebYAS"));
			AssertNull(ClientHookLoader.Instance.GetClientCode("ZClient12"));
			AssertNull(ClientHookLoader.Instance.GetClientCode("Enterprise.dll"));
		}

		public void TestIsClientOverrideAssembly()
		{
			AssertEquals("ZClient4PL does not have a valid EnterpriseCode", false, Loader.IsClientOverrideAssembly("ZClient4PL,"));
			AssertEquals("ZClientwow does not have a valid EnterpriseCode", false, Loader.IsClientOverrideAssembly("ZClientwow,"));
			AssertEquals("ZClientCGX has a valid EnterpriseCode", true, Loader.IsClientOverrideAssembly("ZClientCGX,"));
			AssertEquals("ZClientCG2 has a valid EnterpriseCode", true, Loader.IsClientOverrideAssembly("ZClientCG2,"));

			// Current assemblies.
			AssertEquals("ZClientEDI is a client override", true, Loader.IsClientOverrideAssembly(Loader.FindAssemblyForTest(Clients.EDI)));
			AssertEquals("ZClientM1A is a client override", true, Loader.IsClientOverrideAssembly(Loader.FindAssemblyForTest(Clients.M1A)));
			AssertEquals("ZClientUPE is a client override", true, Loader.IsClientOverrideAssembly(Loader.FindAssemblyForTest("UPE")));
			AssertEquals("ZModules is not a client override", false, Loader.IsClientOverrideAssembly(GetType().Assembly));

			var webAssembly = Assembly.Load("ZClientWebEDI");
			AssertNotNull("Should have been able to load ZClientWebEDI", webAssembly);
			AssertEquals("ZClientWebEDI is not a client override", false, Loader.IsClientOverrideAssembly(webAssembly));
		}

		public void TestFindAssemblyLoadZClientEDI()
		{
			AssertNotNull("ZClientEDI can be loaded", Loader.FindAssemblyForTest(Clients.EDI));
		}

		public void TestGetAllClientSpecificAssemblyFileNames()
		{
			var fileNames = Loader.GetAllClientSpecificAssemblyFileNames();
			AssertNotNull("Client Assemblies should not be null", fileNames);
			Assert("Client Assemblies should has at least 40", fileNames.Length > 40);
			AssertEquals("No file names are duplicated", fileNames.Length, fileNames.Select(Path.GetFileName).Distinct().Count());
		}

		public void TestGetAllClientSpecificAssemblyFileNamesNetCoreFolderFirst()
		{
			var fileNames = ClientHookLoader.GetFilesFromBin("ZClient???.dll", takeNetCoreFilesFirst: true);
			AssertNotNull("Client Assemblies should not be null", fileNames);
			Assert("Client Assemblies should has at least 40", fileNames.Length > 40);
			AssertEquals("No file names are duplicated", fileNames.Length, fileNames.Select(Path.GetFileName).Distinct().Count());
			Assert("Files from NET8 folder", fileNames.Any(f => f.EndsWith("net8.0\\ZClientAUS.dll")));
		}

		public void TestIsAnyClientOverrideAssembly()
		{
			AssertIsAnyClientOverrideAssembly(false, "ZClientUEID");
			AssertIsAnyClientOverrideAssembly(true, "ZClientUPE.Test");
			AssertIsAnyClientOverrideAssembly(true, "ZClientEDI.Business");
			AssertIsAnyClientOverrideAssembly(true, "ZClientEDI.Business.Test");
			AssertIsAnyClientOverrideAssembly(false, "ZClientEDIATest");

			// Current assemblies.
			AssertIsAnyClientOverrideAssembly(true, Loader.FindAssemblyForTest(Clients.UPE).FullName);
			AssertIsAnyClientOverrideAssembly(true, Loader.GetAssemblyFromFileName(Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "ZClientUPE.Test.dll")).FullName);
			AssertIsAnyClientOverrideAssembly(false, GetType().Assembly.FullName);

			var webAssembly = Assembly.Load("ZClientWebEDI");
			AssertNotNull("Should have been able to load ZClientWebEDI", webAssembly);
			AssertIsAnyClientOverrideAssembly(true, webAssembly.FullName);
		}

		void AssertIsAnyClientOverrideAssembly(bool expectedResult, string name)
		{
			AssertEquals(name, expectedResult, Loader.IsAnyClientOverrideAssembly(name));
		}

		public void TestIsAnyNonWebClientOverrideAssembly()
		{
			AssertIsAnyNonWebClientOverrideAssembly("ZClientUEID", false, false);
			AssertIsAnyNonWebClientOverrideAssembly("ZClientEDI.Business", true, true);
			AssertIsAnyNonWebClientOverrideAssembly("ZClientEDI.Business.Test", true, false);
			AssertIsAnyNonWebClientOverrideAssembly("ZClientEDI.Business.Test.DLL", true, false);
			AssertIsAnyNonWebClientOverrideAssembly("ZClientEDI", true, true);
			AssertIsAnyNonWebClientOverrideAssembly("ZClientEDI.DLL", true, true);
			AssertIsAnyNonWebClientOverrideAssembly("ZClientWebEDI", false, false);
			AssertIsAnyNonWebClientOverrideAssembly("ZClientWebEDI.DLL", false, false);
			AssertIsAnyNonWebClientOverrideAssembly("ZClientWebEDI.Test", false, false);
			AssertIsAnyNonWebClientOverrideAssembly("ZClientWebEDI.Test.DLL", false, false);
			AssertIsAnyNonWebClientOverrideAssembly("Enteprise.Masterfiles", false, false);
			AssertIsAnyNonWebClientOverrideAssembly("Enteprise.Masterfiles.DLL", false, false);
			AssertIsAnyNonWebClientOverrideAssembly("Enteprise.Masterfiles.Test", false, false);
			AssertIsAnyNonWebClientOverrideAssembly("Enteprise.Masterfiles.Test.DLL", false, false);
		}

		void AssertIsAnyNonWebClientOverrideAssembly(string name, bool expectedResultIfIncludingTestAssemblies, bool expectedResultIfNotIncludingTestAssemblies)
		{
			AssertEquals(name, expectedResultIfIncludingTestAssemblies, Loader.IsAnyNonWebClientOverrideAssembly(name, true));
			AssertEquals(name, expectedResultIfNotIncludingTestAssemblies, Loader.IsAnyNonWebClientOverrideAssembly(name, false));
		}

		public void TestInitialiseUninitialise()
		{
			var clientHook1 = new TestClientHook();
			AssertEquals("Precondition", false, clientHook1.IsInitialised);

			ClientHook clientHook2;

			using (Loader.OverrideClientAssemblyForTest(Clients.UPE))
			using (Loader.OverrideClientHookForTest(clientHook1))
			{
				AssertEquals("Client Hook should be set up", clientHook1, Loader.ClientHook);
				AssertEquals("The loader should initialise the client hook after loaded", true, clientHook1.IsInitialised);

				using (Loader.OverrideClientAssemblyForTest(Clients.TNT))
				{
					AssertEquals("The loader should uninitialise the client hook after changing assemblies", false, clientHook1.IsInitialised);

					clientHook2 = Loader.ClientHook;
					AssertEquals("The loader should initialise the new client hook after loaded", true, clientHook2.IsInitialised);
				}
			}

			AssertEquals("System should beck to initial state", false, clientHook1.IsInitialised);
			AssertEquals("System should beck to initial state", false, clientHook2.IsInitialised);
		}

		public void TestFindAssembly()
		{
			AssertNotNull("ZClientEDI Assembly should be found, Please build and run again", Loader.FindAssemblyForTest(Clients.EDI));
			AssertNull("ZClientNone Assembly should NOT be found", Loader.FindAssemblyForTest(Clients.None));
		}

		public void TestClientAssembly()
		{
			AssertEquals("Should NOT load Client Hook", Clients.None, Loader.Client);

			var clientAssembly = Loader.FindAssemblyForTest(Clients.TNT);
			AssertNotNull("ZClientTNT Assembly should be found, Please build and run again", clientAssembly);

			using (Loader.OverrideClientAssemblyForTest(clientAssembly))
			{
				AssertEquals("ClientHook should be set to a client hook", Clients.TNT, Loader.Client);
			}

			using (Loader.OverrideClientAssemblyFromCodeForTest(null))
			{
				AssertNull("ClientHook should be set to null", Loader.ClientHook);
				AssertEquals("Client should be set to None", Clients.None, Loader.Client);
			}
		}

		public void TestGetDbSchemaUpgradeInfo()
		{
			var clientAssembly = Loader.FindAssemblyForTest(Clients.UPE);
			AssertNotNull("GetDbSchemaUpgradeInfo", Loader.GetDbSchemaExtensionObjects(clientAssembly));
		}

		#region ITableSchemaSource

		public void TestTableSchemas()
		{
			using (Loader.OverrideClientHookForTest(new TestClientHook()))
			{
				var tableSchemaSource = (ITableSchemaSource)Loader;
				AssertEquals("TableSchemas", 1, tableSchemaSource.TableSchemas.Length);
			}
		}

		public void TestGetTableSchema()
		{
			AssertEquals("Accessing schema for table when no client hook is loaded", null, ObjectFactory.Get<IApplicationSchemaResolver>().GetPkColumnSafe("ClientTestTable"));

			using (Loader.OverrideClientHookForTest(new TestClientHook()))
			{
				AssertEquals("Schema for client table should be accessible", "T9_PK", ObjectFactory.Get<IApplicationSchemaResolver>().GetPkColumn("ClientTestTable").Name);
				AssertEquals("Accessing schema for non-existant table while client hook is loaded", null, ObjectFactory.Get<IApplicationSchemaResolver>().GetPkColumnSafe("NonExistantTable"));
			}
		}

		public void TestGetTableSchemaFromColumnNamePrefix()
		{
			AssertEquals("Accessing schema for table when no client hook is loaded", null, EnterpriseSchema.GetTableSchemaFromColumnNamePrefix("T9"));

			using (Loader.OverrideClientHookForTest(new TestClientHook()))
			{
				AssertEquals("Getting schema for client table from column name prefix", "ClientTestTable", EnterpriseSchema.GetTableSchemaFromColumnNamePrefix("T9").TableName);
				AssertEquals("Accessing schema for non-existant column name prefix while client hook is loaded", null, EnterpriseSchema.GetTableSchemaFromColumnNamePrefix("T2"));
			}
		}

		#endregion

		#region Implementation

		ClientHookLoader Loader
		{
			get { return ClientHookLoader.Instance; }
		}

		#endregion //Implementation
	}
}
