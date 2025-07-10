using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Core.Constants.Customs.Universal;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class ModuleListTest : TestCaseWithFactory
	{
		public void TestCustomsRulesModule()
		{
			ModuleList moduleList = new ModuleList();
			var module = moduleList[ModuleIDs.Customs.CustomsRules, ""];
			AssertEquals("Rules", module.Description);
			AssertEquals("", module.TableName);
		}

		public void TestModuleMenuNameOverride()
		{
			ModuleList moduleList = new ModuleList();
			ModuleInfo usExportClassification = moduleList[ModuleIDs.ExportClassification, Constants.CountryCodes.UnitedStates];
			AssertEquals("usExportClassification.Description", "Schedule B Lookup Codes", usExportClassification.Description);

			ModuleInfo usImportClassification = moduleList[ModuleIDs.ImportClassification, Constants.CountryCodes.UnitedStates];
			AssertEquals("usImportClassification.Description", "HTS Lookup Codes", usImportClassification.Description);
		}

		public void TestModuleNameIsNotTooLongForDocuments()
		{
			var maxLength = 50 - "Doc".Length - "Documents".Length; // these are the guff that the document engine appends before checking the total is less than 50 chars, pffff
			var moduleList = new ModuleList();
			CombineAssertions(delegate
			{
				foreach (var module in moduleList.All)
				{
					var len = module.ID.Name.Length;
					Assert(string.Format("Module {0} has a name that is too long. It may make the document engine puke when it appends its stuff the the module name. Please shorten it from {1} to {2} characters", module.ID.Name, len, maxLength),
														len <= maxLength);
				}
			});
		}

		public void TestAllModuleExtendedDescriptionsAreUnique()
		{
			CombineAssertions(delegate
			{
				IDictionary<string, string> exclude = new Dictionary<string, string>();

				foreach (Clients clientID in Enum.GetValues(typeof(Clients)))
				{
					var uniqueExtendedDescriptions = new HashSet<string>();

					foreach (var moduleID in new HashSet<ModuleIdentifier>(GetClientModuleIdentifiers(clientID).Concat(ModuleIDs.AllExcludingClientModules)))
					{
						var extendedDescription = moduleID.ExtendedDescription;

						Assert(string.Format(CultureInfo.InvariantCulture,
							   "There is a ModuleIdentifier {0} with Extended Description {1}. This is not unique. Check ModuleRegistration.cs and give all Modules with such a Description an ExtendedDescription that makes it unique. If this is not a standard CW1 module and is instead from a client extension, change it in the ClientModuleRegistration.cs file in that client extension's directory (Current clientID = {2}).",
							   moduleID.Name, extendedDescription, clientID),
							   !assertMethod(uniqueExtendedDescriptions, moduleID));
					}
				}

				bool assertMethod(HashSet<string> uniqueExtendedDescriptions, ModuleIdentifier moduleID)
				{
					bool result = moduleID.ExtendedDescription != moduleID.Description & !uniqueExtendedDescriptions.Add(moduleID.ExtendedDescription);
					if (result)
					{
						try
						{
							exclude.Add(moduleID.Name, moduleID.ExtendedDescription);
						}
						catch (ArgumentException)
						{
							return false;
						}

						return true;
					}
					return false;
				}
			});
		}

		ModuleIdentifier[] GetClientModuleIdentifiers(Clients clientID)
		{
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(clientID))
			{
				if (clientID != Clients.None)
				{
					return ClientHookLoader.Instance.ClientHook?.NewClientModules?.Select(x => x.ID).ToArray() ?? Array.Empty<ModuleIdentifier>();
				}
			}
			return Array.Empty<ModuleIdentifier>();
		}

		public void TestClientModulesAddOn()
		{
			ClientModuleIdentifier moduleID1 = new ClientModuleIdentifier(TestClientModuleId.ClientModuleID1, (NoResString)"ClientModuleID1");
			ModuleInfo info1 = new ModuleInfo(moduleID1, "Enterprise.ZArchitecture.GUI", "Enterprise.ZArchitecture.Modules.Testing.DummyFilterGridModule");
			NewClientModuleInfo clientModuleInfo1 = new NewClientModuleInfo("CategoryName1", "SectionName1", info1);

			ClientModuleIdentifier moduleID2 = new ClientModuleIdentifier(TestClientModuleId.ClientModuleID2, (NoResString)"ClientModuleID2");
			ModuleInfo info2 = new ModuleInfo(moduleID2, "Enterprise.ZArchitecture.GUI", "Enterprise.ZArchitecture.Modules.Testing.DummyModule");
			NewClientModuleInfo clientModuleInfo2 = new NewClientModuleInfo("CategoryName2", "SectionName2", info2);

			NewClientModuleInfo[] clientModuleInfos = new NewClientModuleInfo[] { clientModuleInfo1, clientModuleInfo2 };

			ModuleList list = new ModuleList();
			AssertNotNull("ModuleList", list);
			AssertNull(list[moduleID1, ""]);
			AssertNull(list[moduleID2, ""]);

			TestClientHook.Instance.NewClientModulesForTest = clientModuleInfos;
			using (ClientHookLoader.Instance.OverrideClientHookForTest(TestClientHook.Instance))
			{
				list = new ModuleList();
				AssertNotNull("ModuleList", list);
				ModuleInfo existingInfo = list[moduleID1, ""];
				AssertNotNull(existingInfo);
				AssertEquals(info1, existingInfo);

				existingInfo = list[moduleID2, ""];
				AssertNotNull(existingInfo);
				AssertEquals(info2, existingInfo);
			}

			using (ClientHookLoader.Instance.OverrideClientHookForTest(null))
			{
				list = new ModuleList();
				AssertNotNull("ModuleList", list);
				AssertNull(list[moduleID1, ""]);
				AssertNull(list[moduleID2, ""]);
			}
		}

		public void TestGetRegisteredIdentifierByTableName_And_GetRegisteredIdentifierByTablePrefix()
		{
			var moduleId1 = new ModuleIdentifier(ModuleId.Dummy, (NoResString)"Test Module 1");
			var moduleId2 = new ModuleIdentifier(ModuleId.Dummy2, (NoResString)"Test Module 2");

			const string moduleTable1 = "DummyBizo";
			const string moduleTable2 = "DummyDependentBizo";
			const string moduleTablePrefix1 = "Z0";
			const string moduleTablePrefix2 = "ZD1";

			var moduleList = new ModuleList();
			AssertEquals("Should return null when provided table name is null",
						null, moduleList.GetRegisteredIdentifierByTableName(null));
			AssertEquals("Should return null when provided table name is empty",
						null, moduleList.GetRegisteredIdentifierByTableName(string.Empty));

			//one module

			moduleList = new ModuleList();
			moduleList.ClearForTesting();
			moduleList.Add_ExposedForTesting(new ModuleInfo(moduleId1, "Some.Assembly.Name", "Some.Module.Class.Full.Name", new TableRegistrationInfo(moduleTable1)));
			CombineAssertions("A module is registered without specifying a country code", () =>
			{
				//table
				AssertEquals("Should return registered moduleId for the table name",
						moduleId1, moduleList.GetRegisteredIdentifierByTableName(moduleTable1));
				AssertEquals("Should return registered moduleId with empty country code if there is no registered moduleIds for the provided country code",
						moduleId1, moduleList.GetRegisteredIdentifierByTableName(moduleTable1, Constants.CountryCodes.Australia));
				AssertEquals("Should return null if there is no registered moduleIds for the provided table name",
						null, moduleList.GetRegisteredIdentifierByTableName("NonRegisteredTableName"));
				AssertEquals(@"Should return null if there is no registered moduleIds for the provided table name
					(country code does not matter in this case)",
					null, moduleList.GetRegisteredIdentifierByTableName("NonRegisteredTableName", Constants.CountryCodes.Australia));

				//prefix
				AssertEquals("Should return registered moduleId for the table prefix",
						moduleId1, moduleList.GetRegisteredIdentifierByColumnNamePrefix(moduleTablePrefix1));
				AssertEquals("Should return registered moduleId with empty country code if there is no registered moduleIds for the provided country code",
						moduleId1, moduleList.GetRegisteredIdentifierByColumnNamePrefix(moduleTablePrefix1, Constants.CountryCodes.Australia));
				AssertEquals("Should return null if there is no registered moduleIds for the table with provided table prefix",
						null, moduleList.GetRegisteredIdentifierByColumnNamePrefix("WW"));
				AssertEquals(@"Should return null if there is no registered moduleIds for the table with provided table prefix
					(country code does not matter in this case)",
					null, moduleList.GetRegisteredIdentifierByColumnNamePrefix("WW", Constants.CountryCodes.Australia));
			});

			moduleList = new ModuleList();
			moduleList.ClearForTesting();
			moduleList.Add_ExposedForTesting(new ModuleInfo(moduleId1, "Some.Assembly.Name1", "Some.Module.Class.Full.Name1", Constants.CountryCodes.Australia, new TableRegistrationInfo(moduleTable1)));
			moduleList.Add_ExposedForTesting(new ModuleInfo(moduleId1, "Some.Assembly.Name2", "Some.Module.Class.Full.Name2", Constants.CountryCodes.NewZealand, new TableRegistrationInfo(moduleTable2)));
			CombineAssertions("A module is registered twice with different country codes", () =>
			{
				//table
				AssertEquals("Should return registered moduleId for the provided table name and country code (Australia)",
						moduleId1, moduleList.GetRegisteredIdentifierByTableName(moduleTable1, Constants.CountryCodes.Australia));
				AssertEquals("Should return registered moduleId for the provided table name and country code (New Zealand)",
						moduleId1, moduleList.GetRegisteredIdentifierByTableName(moduleTable2, Constants.CountryCodes.NewZealand));
				AssertEquals(@"Should return null if there is no moduleId registered with the provided table name
						and with either provided country code (Australia) or empty country code (even if it is registered for another country)",
						null, moduleList.GetRegisteredIdentifierByTableName(moduleTable2, Constants.CountryCodes.Australia));
				AssertEquals(@"Should return null if there is no moduleId registered with the provided table name
						and empty country code (even if it is registered for some country)",
						null, moduleList.GetRegisteredIdentifierByTableName(moduleTable1));

				//prefix
				AssertEquals("Should return registered moduleId for the provided table prefix and country code (Australia)",
						moduleId1, moduleList.GetRegisteredIdentifierByColumnNamePrefix(moduleTablePrefix1, Constants.CountryCodes.Australia));
				AssertEquals("Should return registered moduleId for the provided table prefix and country code (New Zealand)",
						moduleId1, moduleList.GetRegisteredIdentifierByColumnNamePrefix(moduleTablePrefix2, Constants.CountryCodes.NewZealand));
				AssertEquals(@"Should return null if there is no moduleId registered for the table with provided table prefix
						and with either provided country code (Australia) or empty country code (even if it is registered for another country)",
						null, moduleList.GetRegisteredIdentifierByColumnNamePrefix(moduleTablePrefix2, Constants.CountryCodes.Australia));
				AssertEquals(@"Should return null if there is no moduleId registered for the table with provided table prefix
						and empty country code (even if it is registered for some country)",
						null, moduleList.GetRegisteredIdentifierByColumnNamePrefix(moduleTablePrefix1));
			});

			moduleList = new ModuleList();
			moduleList.ClearForTesting();
			moduleList.Add_ExposedForTesting(new ModuleInfo(moduleId1, "Some.Assembly.Name1", "Some.Module.Class.Full.Name1", new TableRegistrationInfo(moduleTable1)));
			moduleList.Add_ExposedForTesting(new ModuleInfo(moduleId1, "Some.Assembly.Name2", "Some.Module.Class.Full.Name2", Constants.CountryCodes.NewZealand, new TableRegistrationInfo(moduleTable2)));
			CombineAssertions("A module is registered twice with and without specifying a country code", () =>
			{
				//table
				AssertEquals("Should return registered moduleId for the provided table name and empty country code",
						moduleId1, moduleList.GetRegisteredIdentifierByTableName(moduleTable1));
				AssertEquals("Should return registered moduleId for the provided table name and country code (New Zealand)",
						moduleId1, moduleList.GetRegisteredIdentifierByTableName(moduleTable2, Constants.CountryCodes.NewZealand));
				AssertEquals(@"Should return registered moduleId for the provided table name and some unregistered country code
						if the module was registered with empty country code",
						moduleId1, moduleList.GetRegisteredIdentifierByTableName(moduleTable1, Constants.CountryCodes.Argentina));
				AssertEquals(@"Should return null if there is no moduleId registered with the provided table name
						and with either provided country code (Australia) or empty country code (even if it is registered for another country)",
						null, moduleList.GetRegisteredIdentifierByTableName(moduleTable2, Constants.CountryCodes.Australia));

				//prefix
				AssertEquals("Should return registered moduleId for the provided table prefix and empty country code",
						moduleId1, moduleList.GetRegisteredIdentifierByColumnNamePrefix(moduleTablePrefix1));
				AssertEquals("Should return registered moduleId for the provided table prefix and country code (New Zealand)",
						moduleId1, moduleList.GetRegisteredIdentifierByColumnNamePrefix(moduleTablePrefix2, Constants.CountryCodes.NewZealand));
				AssertEquals(@"Should return registered moduleId for the provided table prefix and some unregistered country code
						if the module was registered with empty country code",
						moduleId1, moduleList.GetRegisteredIdentifierByColumnNamePrefix(moduleTablePrefix1, Constants.CountryCodes.Argentina));
				AssertEquals(@"Should return null if there is no moduleId registered for the table with the provided table prefix
						and with either provided country code (Australia) or empty country code (even if it is registered for another country)",
						null, moduleList.GetRegisteredIdentifierByColumnNamePrefix(moduleTablePrefix2, Constants.CountryCodes.Australia));
			});

			//two modules

			moduleList = new ModuleList();
			moduleList.ClearForTesting();
			moduleList.Add_ExposedForTesting(new ModuleInfo(moduleId1, "Some.Assembly.Name1", "Some.Module.Class.Full.Name1", Constants.CountryCodes.Australia, new TableRegistrationInfo(moduleTable1)));
			moduleList.Add_ExposedForTesting(new ModuleInfo(moduleId2, "Some.Assembly.Name2", "Some.Module.Class.Full.Name2", Constants.CountryCodes.NewZealand, new TableRegistrationInfo(moduleTable2)));
			CombineAssertions("Two different modules are registered with different country codes and table names", () =>
			{
				//table
				AssertEquals("Should return registered moduleId for the provided table name and country code (Australia)",
					moduleId1, moduleList.GetRegisteredIdentifierByTableName(moduleTable1, Constants.CountryCodes.Australia));
				AssertEquals("Should return registered moduleId for the provided table name and country code (New Zealand)",
					moduleId2, moduleList.GetRegisteredIdentifierByTableName(moduleTable2, Constants.CountryCodes.NewZealand));

				//prefix
				AssertEquals("Should return registered moduleId for the provided table prefix and country code (Australia)",
					moduleId1, moduleList.GetRegisteredIdentifierByColumnNamePrefix(moduleTablePrefix1, Constants.CountryCodes.Australia));
				AssertEquals("Should return registered moduleId for the provided table prefix and country code (New Zealand)",
					moduleId2, moduleList.GetRegisteredIdentifierByColumnNamePrefix(moduleTablePrefix2, Constants.CountryCodes.NewZealand));
			});

			moduleList = new ModuleList();
			moduleList.ClearForTesting();
			moduleList.Add_ExposedForTesting(new ModuleInfo(moduleId1, "Some.Assembly.Name1", "Some.Module.Class.Full.Name1", new TableRegistrationInfo(moduleTable1)));
			moduleList.Add_ExposedForTesting(new ModuleInfo(moduleId2, "Some.Assembly.Name2", "Some.Module.Class.Full.Name2", new TableRegistrationInfo(moduleTable1)));
			CombineAssertions("Two different modules are registered with the same table name without specifying country codes", () =>
			{
				//table
				AssertEquals(@"Should return the first registered moduleId for the provided table name
					if there are two modules registered with the same table",
					moduleId1, moduleList.GetRegisteredIdentifierByTableName(moduleTable1));
				AssertEquals(@"Should return the first registered moduleId for the provided table name and empty country code
					if there are two modules registered with the same table and empty country code
					and there is no registered moduleIds for the provided country code",
					moduleId1, moduleList.GetRegisteredIdentifierByTableName(moduleTable1, Constants.CountryCodes.Australia));

				//prefix
				AssertEquals(@"Should return the first registered moduleId for the table with provided table prefix
					if there are two modules registered with the same table",
					moduleId1, moduleList.GetRegisteredIdentifierByColumnNamePrefix(moduleTablePrefix1));
				AssertEquals(@"Should return the first registered moduleId for the table with provided table prefix and empty country code
					if there are two modules registered with the same table and empty country code
					and there is no registered moduleIds for the provided country code",
					moduleId1, moduleList.GetRegisteredIdentifierByColumnNamePrefix(moduleTablePrefix1, Constants.CountryCodes.Australia));
			});

			moduleList = new ModuleList();
			moduleList.ClearForTesting();
			moduleList.Add_ExposedForTesting(new ModuleInfo(moduleId1, "Some.Assembly.Name1", "Some.Module.Class.Full.Name1", Constants.CountryCodes.Australia, new TableRegistrationInfo(moduleTable1)));
			moduleList.Add_ExposedForTesting(new ModuleInfo(moduleId2, "Some.Assembly.Name2", "Some.Module.Class.Full.Name2", Constants.CountryCodes.NewZealand, new TableRegistrationInfo(moduleTable1)));
			CombineAssertions("Two different modules are registered with the same table name, but different country codes", () =>
			{
				//table
				AssertEquals("Should return registered moduleId for the provided table name and country code (Australia)",
					moduleId1, moduleList.GetRegisteredIdentifierByTableName(moduleTable1, Constants.CountryCodes.Australia));
				AssertEquals("Should return registered moduleId for the provided table name and country code (New Zealand)",
					moduleId2, moduleList.GetRegisteredIdentifierByTableName(moduleTable1, Constants.CountryCodes.NewZealand));
				AssertEquals(@"Should return null if there is no moduleId registered with the provided table name
						and empty country code (even if it is registered for some country)",
						null, moduleList.GetRegisteredIdentifierByTableName(moduleTable1));

				//prefix
				AssertEquals("Should return registered moduleId for the provided table prefix and country code (Australia)",
					moduleId1, moduleList.GetRegisteredIdentifierByColumnNamePrefix(moduleTablePrefix1, Constants.CountryCodes.Australia));
				AssertEquals("Should return registered moduleId for the provided table prefix and country code (New Zealand)",
					moduleId2, moduleList.GetRegisteredIdentifierByColumnNamePrefix(moduleTablePrefix1, Constants.CountryCodes.NewZealand));
				AssertEquals(@"Should return null if there is no moduleId registered for the table with provided table prefix
						and empty country code (even if it is registered for some country)",
						null, moduleList.GetRegisteredIdentifierByColumnNamePrefix(moduleTablePrefix1));
			});
		}

		public void TestGetRegisteredIdentifierByTableName_FallbackMechanism()
		{
			var moduleId = new ModuleIdentifier(ModuleId.Dummy, (NoResString)"Test Module 1");
			const string moduleTable = "DummyBizo";

			var moduleList = new ModuleList();
			moduleList.ClearForTesting();
			moduleList.Add_ExposedForTesting(new ModuleInfo(moduleId, "Some.Assembly.Name1", "Some.Module.Class.Full.Name1"));
			CombineAssertions(@"Fallback mechanism: if a table name is not registered, but equals to a module identifier registered without specifying a table name, GetRegisteredIdentifierByTableName() should return the module ID.
				A module is registered without specifying a table name and a country code", () =>
			{
				AssertEquals("Should return registered moduleId for the table name if the table name is not registered, but equals to the module ID",
						moduleId, moduleList.GetRegisteredIdentifierByTableName("Dummy"));
			});

			moduleList = new ModuleList();
			moduleList.ClearForTesting();
			moduleList.Add_ExposedForTesting(new ModuleInfo(moduleId, "Some.Assembly.Name1", "Some.Module.Class.Full.Name1", new TableRegistrationInfo(moduleTable)));
			CombineAssertions(@"Fallback mechanism: if a table name is not registered, but equals to a module identifier registered without specifying a table name, GetRegisteredIdentifierByTableName() should return the module ID.
				A module is registered with specifying a table name only", () =>
			{
				AssertEquals(@"Should return null if there is no registered moduleIds for the provided table name
						and if the module with the ID which equals to the provided table name is registered with another table name",
						null, moduleList.GetRegisteredIdentifierByTableName("Dummy"));
			});

			moduleList = new ModuleList();
			moduleList.ClearForTesting();
			moduleList.Add_ExposedForTesting(new ModuleInfo(moduleId, "Some.Assembly.Name1", "Some.Module.Class.Full.Name1", Constants.CountryCodes.NewZealand));
			CombineAssertions(@"Fallback mechanism: if a table name is not registered, but equals to a module identifier registered without specifying a table name, GetRegisteredIdentifierByTableName() should return the module ID.
				A module is registered with specifying a country code only", () =>
			{
				AssertEquals("Should return registered moduleId for the provided country code and table name if the table name is not registered, but equals to the module ID",
						moduleId, moduleList.GetRegisteredIdentifierByTableName("Dummy", Constants.CountryCodes.NewZealand));
				AssertEquals(@"Should return null if there is no registered moduleIds for the provided table name and empty country code
						and if the module with the ID which equals to the provided table name is registered with a non-empty country code",
						null, moduleList.GetRegisteredIdentifierByTableName("Dummy"));
				AssertEquals(@"Should return null if there is no registered moduleIds for the provided table name and country code
						and if the module with the ID which equals to the provided table name is registered with another country code",
						null, moduleList.GetRegisteredIdentifierByTableName("Dummy", Constants.CountryCodes.Argentina));
			});

			moduleList = new ModuleList();
			moduleList.ClearForTesting();
			moduleList.Add_ExposedForTesting(new ModuleInfo(moduleId, "Some.Assembly.Name1", "Some.Module.Class.Full.Name1", Constants.CountryCodes.NewZealand, new TableRegistrationInfo(moduleTable)));
			CombineAssertions(@"Fallback mechanism: if a table name is not registered, but equals to a module identifier registered without specifying a table name, GetRegisteredIdentifierByTableName() should return the module ID.
				A module is registered with specifying a table name and a country code", () =>
			{
				AssertEquals(@"Should return null if there is no registered moduleIds for the provided table name
						and if the module with the ID which equals to the provided table name is registered with another table name",
						null, moduleList.GetRegisteredIdentifierByTableName("Dummy"));
				AssertEquals(@"Should return null if there is no registered moduleIds for the provided table name
						and if the module with the ID which equals to the provided table name is registered with another table name
						(country code does not matter in this case)",
						null, moduleList.GetRegisteredIdentifierByTableName("Dummy", Constants.CountryCodes.NewZealand));
			});
		}

		enum testModuleId { SupportIncident }

		public void TestGetRegisteredIdentifierByParentModule()
		{
			var moduleId = new ModuleIdentifier(testModuleId.SupportIncident, (NoResString)"Test Module 1");
			const string moduleTable = "IncidentMain";

			var clientHook1 = new TestClientHook();
			AssertEquals("Precondition", false, clientHook1.IsInitialised);

			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			using (ClientHookLoader.Instance.OverrideClientHookForTest(clientHook1))
			{
				clientHook1.AddChildToParentTableMapping("IncidentRequest", "SupportIncident");
				var moduleList = new ModuleList();

				moduleList.ClearForTesting();
				moduleList.Add_ExposedForTesting(new ModuleInfo(moduleId, "Some.Assembly.Name1", "Some.Module.Class.Full.Name1", Constants.CountryCodes.NewZealand, new TableRegistrationInfo(moduleTable)));
				AssertEquals(@"Should return registered moduleId for the provided module and country code which has been mapped to the parentModule",
							moduleId, moduleList.GetRegisteredIdentifierByTableName("IncidentRequest", Constants.CountryCodes.NewZealand));
			}
		}

		public void TestTemporaryStorageIsAvailableForES()
		{
			var list = new ModuleList();
			var temporaryStorage = list[ModuleIDs.Customs.TemporaryStorage, Constants.CountryCodes.Spain];
			AssertNotNull(temporaryStorage);
			AssertEquals("Enterprise.Customs.ES.TemporaryStorage.Module.TemporaryStorageModule", temporaryStorage.ClassFullNameForTest);
			AssertEquals(Constants.CountryCodes.Spain, temporaryStorage.CountryCode);
		}

		public void TestNctsMovementModuleIsAvailableForES()
		{
			var list = new ModuleList();
			var nctsMovementModule = list[ModuleIDs.Customs.EU.NctsMovementModule, Constants.CountryCodes.Spain];
			AssertNotNull(nctsMovementModule);
			AssertEquals("Enterprise.Customs.ES.NCTS.Module.NctsMovementModule", nctsMovementModule.ClassFullNameForTest);
			AssertEquals(Constants.CountryCodes.Spain, nctsMovementModule.CountryCode);
		}

		public void TestJobDeclarationModuleIsAvailableForES()
		{
			var list = new ModuleList();
			var jobDeclarationModule = list[ModuleIDs.Customs.JobDeclaration, Constants.CountryCodes.Spain];
			AssertNotNull(jobDeclarationModule);
			AssertEquals("Enterprise.Customs.ES.Module.JobDeclarationModule", jobDeclarationModule.ClassFullNameForTest);
			AssertEquals(Constants.CountryCodes.Spain, jobDeclarationModule.CountryCode);
		}

		public void TestEntryHeaderModuleIsAvailableForES()
		{
			var list = new ModuleList();
			var entryHeaderModule = list[ModuleIDs.Customs.EntryHeader, Constants.CountryCodes.Spain];
			AssertNotNull(entryHeaderModule);
			AssertEquals("Enterprise.Customs.ES.Module.EntryHeaderModule", entryHeaderModule.ClassFullNameForTest);
			AssertEquals(Constants.CountryCodes.Spain, entryHeaderModule.CountryCode);
		}

		public void TestExitControlIsAvailableForES()
		{
			var list = new ModuleList();
			var exitControl = list[ModuleIDs.Customs.EU.ExitControl, Constants.CountryCodes.Spain];
			AssertNotNull(exitControl);
			AssertEquals("Enterprise.Customs.ES.ExitControl.Module.ExitControlModule", exitControl.ClassFullNameForTest);
			AssertEquals(Constants.CountryCodes.Spain, exitControl.CountryCode);
		}

		public void TestTemporaryStorageRegisterIsAvailableForES()
		{
			var list = new ModuleList();
			var temporaryStorageRegister = list[ModuleIDs.Customs.EU.ES.TemporaryStorageRegister, Constants.CountryCodes.Spain];
			CombineAssertions(() =>
			{
				AssertNotNull(temporaryStorageRegister);
				AssertEquals("Enterprise.Customs.ES.TemporaryStorage.Module.TemporaryStorageRegisterModule", temporaryStorageRegister.ClassFullNameForTest);
				AssertEquals(Constants.CountryCodes.Spain, temporaryStorageRegister.CountryCode);
			});
		}

		public void TestCustomsStatementIsAvailableForFranceAndOverseasDepartmentsUnderItsCustomsJurisdiction()
		{
			var list = new ModuleList();
			foreach (var countryCode in Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
			{
				var customsStatement = list[ModuleIDs.Customs.EU.FR.CustomsStatement, countryCode];
				AssertNotNull(customsStatement);
				AssertEquals("Enterprise.Customs.FR.Module.StatementModule", customsStatement.ClassFullNameForTest);
				AssertEquals(countryCode, customsStatement.CountryCode);
				AssertEquals("Liquidation Statements", customsStatement.Description);
			}
		}

		public void TestTempStorageRegisterIsAvailableForFranceAndOverseasDepartmentsUnderItsCustomsJurisdiction()
		{
			var list = new ModuleList();
			foreach (var countryCode in Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
			{
				var tempStorageRegister = list[ModuleIDs.Customs.EU.TempStorageRegister, countryCode];
				AssertNotNull(tempStorageRegister);
				AssertEquals("Enterprise.Customs.FR.Module.TempStorageRegisterModule", tempStorageRegister.ClassFullNameForTest);
				AssertEquals(countryCode, tempStorageRegister.CountryCode);
				AssertEquals("Temp. Storage Register", tempStorageRegister.Description);
			}
		}

		public void TestTemporaryStorageIsAvailableForPL()
		{
			var list = new ModuleList();
			var temporaryStorage = list[ModuleIDs.Customs.TemporaryStorage, Constants.CountryCodes.Poland];
			AssertNotNull(temporaryStorage);
			AssertEquals("Enterprise.Customs.PL.Module.TemporaryStorageModule", temporaryStorage.ClassFullNameForTest);
			AssertEquals(Constants.CountryCodes.Poland, temporaryStorage.CountryCode);
		}

		public void TestUCC6TemporaryStorageIsAvailableForEUCountries()
		{
			var list = new ModuleList();
			foreach (var euCustomsMember in ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers())
			{
				if (IsFranceOrFranceAndOverseasDepartmentsUnderItsCustomsJurisdiction(euCustomsMember)
					|| euCustomsMember is CountryCodes.Ireland or CountryCodes.Italy)
				{
					continue;
				}
				var temporaryStorage = list[ModuleIDs.Customs.EU.UCC6TemporaryStorage, euCustomsMember];
				AssertNotNull(temporaryStorage);
				CombineAssertions($"When CountryCode is {euCustomsMember}", () =>
				{
					AssertEquals("Enterprise.Customs.EU.TemporaryStorage.Module.UCC6TemporaryStorageModule", temporaryStorage.ClassFullNameForTest);
					AssertEquals(euCustomsMember, temporaryStorage.CountryCode);
				});
			}

			static bool IsFranceOrFranceAndOverseasDepartmentsUnderItsCustomsJurisdiction(string countryCode)
				=> countryCode is CountryCodes.France || CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction.Contains(countryCode);
		}

		public void TestUCC6TemporaryStorageIsAvailableForFranceAndOverseasDepartmentsUnderItsCustomsJurisdiction()
		{
			var list = new ModuleList();
			foreach (var countryCode in Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
			{
				var temporaryStorage = list[ModuleIDs.Customs.EU.UCC6TemporaryStorage, countryCode];
				AssertNotNull(temporaryStorage);
				AssertEquals("Enterprise.Customs.FR.Module.UCC6TemporaryStorageModule", temporaryStorage.ClassFullNameForTest);
				AssertEquals(countryCode, temporaryStorage.CountryCode);
			}
		}

		public void TestTempStoragePremisesIsAvailableForEUCountries()
		{
			var list = new ModuleList();
			foreach (var euCustomsMember in ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers())
			{
				if (euCustomsMember is CountryCodes.Spain)
				{
					continue;
				}
				var temporaryStoragePremises = list[ModuleIDs.Customs.EU.TempStoragePremises, euCustomsMember];
				CombineAssertions(() =>
				{
					AssertNotNull(temporaryStoragePremises);
					AssertEquals("Enterprise.Customs.EU.TemporaryStorage.Module.TempStoragePremisesModule", temporaryStoragePremises.ClassFullNameForTest);
					AssertEquals(euCustomsMember, temporaryStoragePremises.CountryCode);
				});
			}
		}
		public void TestTempStoragePremisesIsAvailableForES()
		{
			var list = new ModuleList();
			var temporaryStoragePremises = list[ModuleIDs.Customs.EU.TempStoragePremises, CountryCodes.Spain];
			CombineAssertions(() =>
			{
				AssertNotNull(temporaryStoragePremises);
				AssertEquals("Enterprise.Customs.ES.TemporaryStorage.Module.TempStoragePremisesModule", temporaryStoragePremises.ClassFullNameForTest);
				AssertEquals(CountryCodes.Spain, temporaryStoragePremises.CountryCode);
			});
		}

		public void TestExitControlReportIsAvailableForIE()
		{
			var list = new ModuleList();
			var exitControlReport = list[ModuleIDs.Customs.EU.ExitControlReport, Constants.CountryCodes.Ireland];
			AssertNotNull(exitControlReport);
			AssertEquals("Enterprise.Customs.IE.ExitControl.Module.ExitControlReportModule", exitControlReport.ClassFullNameForTest);
			AssertEquals(Constants.CountryCodes.Ireland, exitControlReport.CountryCode);
		}

		public void TestUCC6TemporaryStorageModuleIsAvailableForIE()
		{
			AssertUCC6TemporaryStorageModuleIsAvailable(Constants.CountryCodes.Ireland, "Enterprise.Customs.IE.Module.UCC6TemporaryStorageModule");
		}

		public void TestUCC6TemporaryStorageModuleIsAvailableForIT()
		{
			AssertUCC6TemporaryStorageModuleIsAvailable(Constants.CountryCodes.Italy, "Enterprise.Customs.IT.TemporaryStorage.Module.UCC6TemporaryStorageModule");
		}

		void AssertUCC6TemporaryStorageModuleIsAvailable(string countryCode, string expectedClassFullName)
		{
			var list = new ModuleList();
			var temporaryStorage = list[ModuleIDs.Customs.EU.UCC6TemporaryStorage, countryCode];
			AssertNotNull("Module should exist", temporaryStorage);
			AssertEquals("Class full name", expectedClassFullName, temporaryStorage.ClassFullNameForTest);
			AssertEquals("Country code", countryCode, temporaryStorage.CountryCode);
		}

		public void TestTempStorageRegisterIsAvailableForIT()
		{
			var countryCode = Constants.CountryCodes.Italy;
			var list = new ModuleList();

			var tempStorageRegister = list[ModuleIDs.Customs.EU.TempStorageRegister, countryCode];
			AssertNotNull(tempStorageRegister);
			AssertEquals("Enterprise.Customs.IT.TemporaryStorage.Module.TempStorageRegisterModule", tempStorageRegister.ClassFullNameForTest);
			AssertEquals(countryCode, tempStorageRegister.CountryCode);
			AssertEquals("Temp. Storage Register", tempStorageRegister.Description);
		}

		public void TestAllEUModulesAreAvailableForCountriesUnderFrenchJurisdiction()
		{
			var list = new ModuleList();
			foreach (var country in CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
			{
				var temporaryStorage = list[ModuleIDs.Customs.EU.UCC6TemporaryStorage, country];
				AssertNotNull(temporaryStorage);
				AssertEquals("Enterprise.Customs.FR.Module.UCC6TemporaryStorageModule", temporaryStorage.ClassFullNameForTest);
				AssertEquals(country, temporaryStorage.CountryCode);

				var classification = list[ModuleIDs.SingleTariffClassification, country];
				AssertNotNull(classification);
				AssertEquals("Enterprise.Customs.EU.Module.CusClassificationModule", classification.ClassFullNameForTest);
				AssertEquals(country, classification.CountryCode);

				var guarantees = list[ModuleIDs.Customs.Guarantees, country];
				AssertNotNull(guarantees);
				AssertEquals("Enterprise.Customs.EU.Module.GuaranteesModule", guarantees.ClassFullNameForTest);
				AssertEquals(country, guarantees.CountryCode);

				var permit = list[ModuleIDs.Customs.Permits, country];
				AssertNotNull(permit);
				AssertEquals("Enterprise.Customs.EU.Module.CusPermitModule", permit.ClassFullNameForTest);
				AssertEquals(country, permit.CountryCode);
			}
		}

		[ExpectNoExceptions]
		public void TestNoExceptionThrownWhenConstructingModuleList_IfTerritoriesAddedToCustomsUnionAdditionalMembers_FrenchForInstance()
		{
			var helper = ObjectFactory.Get<Enterprise.Integration.Customs.Shared.Universal.IUniversalReferenceTestDataHelper>("Universal.IUniversalReferenceTestDataHelper", Factory);
			helper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
			var tradeGroup = helper.CreateTradeGroup("EUN", RefCusTradeGroup.Codes.EuropeanUnionForCustoms, new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroup, CountryCodes.France, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			var tradeGroupCUAM = helper.CreateTradeGroup("EUN", RefCusTradeGroup.Codes.CustomsUnionAdditionalMembers, new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroupCUAM, CountryCodes.Martinique, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));

			AssertNoExceptionThrown("No Exception thrown even if territories under jurisdiction is added into CustomsUnionAdditionalMembers trade group.", () =>
			{
				var list = new ModuleList();
			});
		}

		public void TestSumARegisterReadOnlyIsAvailableForDE()
		{
			var list = new ModuleList();
			var sumaRegisterReadOnlyModule = list[ModuleIDs.Customs.EU.DE.SumARegisterReadOnly, Constants.CountryCodes.Germany];
			CombineAssertions(() =>
			{
				AssertNotNull("Module exists", sumaRegisterReadOnlyModule);
				AssertEquals("Full Name", "Enterprise.Customs.DE.Module.SumARegisterReadOnlyModule", sumaRegisterReadOnlyModule.ClassFullNameForTest);
				AssertEquals("Country Code", Constants.CountryCodes.Germany, sumaRegisterReadOnlyModule.CountryCode);
			});
		}

		public void TestGoodsCatalogModuleIsAvailableForBR()
		{
			var list = new ModuleList();
			var goodsCatalog = list[ModuleIDs.Customs.GoodsCatalog, Constants.CountryCodes.Brazil];
			AssertNotNull(goodsCatalog);
			AssertEquals("Enterprise.Customs.BR.Module.GoodsCatalogModule", goodsCatalog.ClassFullNameForTest);
			AssertEquals(Constants.CountryCodes.Brazil, goodsCatalog.CountryCode);
		}

		public void TestTemporaryStorageRegisterIsAvailableForNorway()
		{
			var list = new ModuleList();
			AssertModuleInformation(list, ModuleIDs.Customs.NO.TemporaryStorageRegister, "Enterprise.Customs.NO.Module.SumARegisterModule", "Temporary Storage - Register", CountryCodes.Norway);
			AssertModuleInformation(list, ModuleIDs.Customs.NO.TemporaryStorageRegisterReadOnly, "Enterprise.Customs.NO.Module.SumARegisterReadOnlyModule", "Temporary Storage - Register", CountryCodes.Norway);
		}

		public void TestSumARegisterModuleInformation()
		{
			var list = new ModuleList();
			AssertModuleInformation(list, ModuleIDs.Customs.SumARegister, "Enterprise.Customs.EFTA.TemporaryStorageRegister.Module.SumARegisterModule", "SumA Register");
			AssertModuleInformation(list, ModuleIDs.Customs.SumARegisterReadOnly, "Enterprise.Customs.EFTA.TemporaryStorageRegister.Module.SumARegisterReadOnlyModule", "SumA Register Read Only");
		}

		public void TestImportFromTemporaryStorageRegisterModuleInformation()
		{
			var list = new ModuleList();
			AssertModuleInformation(list, ModuleIDs.Customs.ImportFromTemporaryStorageRegister, "Enterprise.Customs.EFTA.TemporaryStorageRegister.Module.ImportFromTemporaryStorageRegisterModule", "Import from Temporary Storage Register");
		}

		public void TestH7BillIsIsSpecilaForES()
		{
			var list = new ModuleList();
			AssertModuleInformation(list, ModuleIDs.Customs.EU.ES.EUH7Bill, "Enterprise.Customs.ES.Manifest.H7.Module.ESH7BillModule", "Low Value (H7) by bill", CountryCodes.Spain);
		}

		static void AssertModuleInformation(ModuleList list, ModuleIdentifier moduleId, string expectedClassFullName, string expectedCaption, string countryCode = "")
		{
			var moduleName = moduleId.Name;
			var module = list[moduleId, countryCode];
			AssertNotNull($"Module: {moduleName}", module);
			CombineAssertions(moduleName, () =>
			{
				AssertEquals("Full Name", expectedClassFullName, module.ClassFullNameForTest);
				AssertEquals("Country Code", countryCode, module.CountryCode);
				AssertEquals("Caption", expectedCaption, module.Description);
			});
		}
	}
}
