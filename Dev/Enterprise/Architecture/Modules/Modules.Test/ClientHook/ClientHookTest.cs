using System;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class ClientHookTest : TestCase
	{
		#region UniqueId

		public void TestUniqueId()
		{
			var hook = new TestClientHook();
			AssertEquals("", hook.UniqueId);

			hook.ClientOverride = Clients.EDI;
			AssertEquals("EDI", hook.UniqueId);
		}

		#endregion

		#region Initialize / Uninitialize

		[ExpectException(typeof(InvalidOperationException))]
		public void TestInitializeCanOnlyBeCalledOnce()
		{
			var hook = new TestClientHook();
			hook.Initialise();
			hook.Initialise();
		}

		[ExpectException(typeof(InvalidOperationException))]
		public void TestUninitializeCanOnlyBeCalledOnce()
		{
			var hook = new TestClientHook();
			hook.Initialise();
			hook.Uninitialise();
			hook.Uninitialise();
		}

		#endregion

		#region NewClientControllers

		public void TestNewClientControllers()
		{
			var mockHook = new Mock<ClientHook>();
			mockHook.Protected()
				.Setup<ControllerInfo[]>("NewClientControllersCore")
				.Returns<ControllerInfo[]>(null);

			var hook = mockHook.Object;
			AssertNull("NewClientControllers", hook.NewClientControllers);

			var iD = new ControllerID("ControllerID");
			var info = new ControllerInfo(iD, typeof(TestClientHook));
			var infos = new ControllerInfo[] { info };

			mockHook.Protected()
				.Setup<ControllerInfo[]>("NewClientControllersCore")
				.Returns(infos);

			var hookInfos = hook.NewClientControllers;
			AssertEquals("NewClientControllers length", 1, hookInfos.Length);
			AssertEquals("NewClientControllers", info, hookInfos[0]);

			mockHook.VerifyAll();
		}

		#endregion

		#region ChildTableToParentTableDictionary

		public void TestChildTableToParentTableDictionary()
		{
			var clientHook = new TestClientHook();
			clientHook.AddChildToParentTableMapping("child", "parent");

			AssertEquals("Parent table for child table", "parent", clientHook.GetParentTableName("child"));

			clientHook.RemoveChildToParentTableMapping("child");
			AssertEquals("Parent table for removed child table", null, clientHook.GetParentTableName("child"));
		}
		#endregion

		#region NewClientModules

		public void TestNewClientModules()
		{
			var mockHook = new Mock<ClientHook>();
			mockHook.Protected()
				.Setup<NewClientModuleInfo[]>("NewClientModulesCore")
				.Returns<NewClientModuleInfo>(null);

			var hook = mockHook.Object;
			AssertNull("NewClientModules", hook.NewClientModules);

			var info = new ModuleInfo(null, typeof(TestClientHook));
			var module = new NewClientModuleInfo("CategoryName", "SectionName", info);
			var modules = new NewClientModuleInfo[] { module };

			mockHook.Protected()
				.Setup<NewClientModuleInfo[]>("NewClientModulesCore")
				.Returns(modules);

			var hookModules = hook.NewClientModules;
			AssertEquals("NewClientModules length", 1, hookModules.Length);
			AssertEquals("NewClientModules", module, hookModules[0]);

			mockHook.VerifyAll();
		}

		#endregion

		#region NewModuleSectionsToAddForClient

		public void TestNewModuleSectionsToAddForClient()
		{
			var mockHook = new Mock<ClientHook>();
			mockHook.Protected()
				.Setup<IModuleSectionAddOn[]>("NewModuleSectionsToAddForClientCore")
				.Returns<IModuleSectionAddOn[]>(null);

			var hook = mockHook.Object;
			AssertNull("NewModuleSectionsToAddForClient", hook.NewModuleSectionsToAddForClient);

			var section = new TestModuleSectionAddOn();
			var sections = new TestModuleSectionAddOn[] { section };

			mockHook.Protected()
				.Setup<IModuleSectionAddOn[]>("NewModuleSectionsToAddForClientCore")
				.Returns(sections);

			var hookSections = hook.NewModuleSectionsToAddForClient;
			AssertEquals("NewModuleSectionsToAddForClient length", 1, hookSections.Length);
			AssertEquals("NewModuleSectionsToAddForClient", section, hookSections[0]);

			mockHook.VerifyAll();
		}

		#endregion

		#region ITableSchemaSource

		public void TestGetTableSchema()
		{
			AssertEquals("Accessing schema for table when no client hook is loaded", null, ObjectFactory.Get<IApplicationSchemaResolver>().GetPkColumnSafe("ClientTestTable"));

			using (ClientHookLoader.Instance.OverrideClientHookForTest(new TestClientHook()))
			{
				AssertEquals("Schema for client table should be accessible", "T9_PK", ObjectFactory.Get<IApplicationSchemaResolver>().GetPkColumn("ClientTestTable").Name);
				AssertEquals("Accessing schema for non-existant table while client hook loaded", null, ObjectFactory.Get<IApplicationSchemaResolver>().GetPkColumnSafe("NonExistantTable"));
			}
		}

		#endregion

		#region DocumentEngine

		public void TestDocumentEngineCollectionProviders()
		{
			AssertNull("Must be null in ZModule and be overriden later on", TestClientHook.Instance.DocumentEngineCollectionProviders);
		}

		public void TestDocumentEngineCodeDescriptionPairProviders()
		{
			AssertNull("Must be null in ZModule and be overriden later on", TestClientHook.Instance.DocumentEngineCodeDescriptionPairProviders);
		}

		#endregion

		#region Test Classes

		class TestModuleSectionAddOn : IModuleSectionAddOn
		{
			#region IModuleSectionAddOn Members

			public string CategoryName
			{
				get { return "TestIoduleSectionAddOn"; }
			}

			#endregion
		}

		#endregion
	}
}
