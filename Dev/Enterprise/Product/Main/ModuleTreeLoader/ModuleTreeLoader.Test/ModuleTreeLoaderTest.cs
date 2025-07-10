using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core.Modules;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Recruitment.Registry;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs.Manifest;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Security.Testing;
using Enterprise.TransportCommon.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;
using static Enterprise.Registry.Business.RatingFeatureHelper.CarrierConnect;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Main.ModuleTreeLoader.Test
{
	sealed class ModuleTreeLoaderTest : BaseModuleTreeTest
	{
		#region Utility
		(string id, IMainFormModule mainForm)[] FindModulesById(params ModuleIdentifier[] moduleIdentifiers)
		{
			if (moduleIdentifiers == null || moduleIdentifiers.Length == 0)
			{
				throw new ArgumentException("Module Identifiers cannot be empty.", nameof(moduleIdentifiers));
			}

			var allModules = Tree.Categories.Values.Cast<ModuleCategory>()
				.SelectMany(category => category.Sections.Values.Cast<ModuleSection>())
				.SelectMany(section => section.Modules.Values.Cast<IMainFormModule>());

			var results = moduleIdentifiers.Select(id =>
			{
				var module = allModules.FirstOrDefault(m => string.Equals(m.ID, id.ToString(), StringComparison.InvariantCultureIgnoreCase));

				return (id.ToString(), module);
			});

			return results.ToArray();
		}
		#endregion

		#region Module Integrity Verification

		public void TestPreBoardingNotificationAreAdded()
		{
			ManifestCustomsDataRegistry.Instance.PreBoardingNotificationManifestEnabled.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				Loader.LoadModules();
				var moduleIDs = LoadModuleIDs(ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain);
				AssertEquals("GVMS should be loaded", "Goods vehicle movement system (GVMS)", moduleIDs.FirstOrDefault(p => p.Name == "PreBoardingNotification")?.Description);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				Loader.LoadModules();
				var moduleIDs = LoadModuleIDs(ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain);
				AssertEquals("PBN should be loaded", "PBN-Pre Boarding Notification", moduleIDs.FirstOrDefault(p => p.Name == "PreBoardingNotification")?.Description);
			}
		}

		IEnumerable<ModuleIdentifier> LoadModuleIDs(ModuleTreeLoaderConstant.Entry category, ModuleTreeLoaderConstant.Entry section)
		{
			var targetCategory = Tree.Categories[category.Name];
			var targetSection = targetCategory.Sections[section.Name];
			if (targetSection is null)
			{
				return Enumerable.Empty<ModuleIdentifier>();
			}
			else
			{
				return targetSection.Modules.Values.Cast<IMainFormModule>().Select(module => module.ModuleID);
			}
		}

		public void TestCountrySpecificModulesAreAdded()
		{
			Loader.LoadModules();

			var moduleIds =
				from category in Tree.Categories.ValuesIncludingHidden
				from section in category.Sections.ValuesIncludingHidden
				from module in section.Modules.ValuesIncludingHidden
				select module.ModuleID;

			AssertCollectionContains("ModuleIDs.Customs.EU.GB.GbCcsukReports", ModuleIDs.Customs.EU.GB.GbCcsukReports, moduleIds);
		}

		public void TestAllModuleNodesAreAddedProperly()
		{
			Loader.LoadModules();

			var fails = new List<string>();
			var allCategoriesIncludeHidden = Tree.Categories.ValuesIncludingHidden;
			var allCategoryNamesIncludeHidden = allCategoriesIncludeHidden.Select(category => category.Name);
			foreach (var categoryProperties in typeof(ModuleTreeLoaderConstant.Category).GetProperties(BindingFlags.Public | BindingFlags.Static).Where(prop => prop.PropertyType == typeof(ModuleTreeLoaderConstant.Entry)))
			{
				var category = (ModuleTreeLoaderConstant.Entry)categoryProperties.GetValue(null, null);
				if (!allCategoryNamesIncludeHidden.Contains(category.Name))
				{
					fails.Add(string.Format("ModuleTreeLoaderConstant.Category.{0}", categoryProperties.Name));
				}
			}

			var allSectionsIncludeHidden = allCategoriesIncludeHidden.SelectMany(category => category.Sections.ValuesIncludingHidden);
			var allSectionNamesIncludeHidden = allSectionsIncludeHidden.Select(section => section.Name);
			foreach (var sectionProperties in typeof(ModuleTreeLoaderConstant.Section).GetProperties(BindingFlags.Public | BindingFlags.Static).Where(prop => prop.PropertyType == typeof(ModuleTreeLoaderConstant.Entry)))
			{
				var section = (ModuleTreeLoaderConstant.Entry)sectionProperties.GetValue(null, null);
				if (!allSectionNamesIncludeHidden.Contains(section.Name))
				{
					fails.Add(string.Format("ModuleTreeLoaderConstant.Section.{0}", sectionProperties.Name));
				}
			}

			if (fails.Count > 0)
			{
				Fail(@"All categories, sections and modules should be added through the 'AddIf' methods.
This is required for the Customer Service modules which need to access a list of every module node - even if the module is currently not displayed for the user.

The following fail:
" + string.Join("\r\n", fails));
			}
			else
			{
				Assert(true);
			}
		}

		static bool IsUserDefinedModule(IMainFormModule module)
		{
			if (module == null)
			{
				return false;
			}

			var section = module.ParentSection;
			if (section == null)
			{
				return false;
			}

			var category = section.ParentCategory;
			if (category == null)
			{
				return false;
			}

			if (category.Name != ModuleTreeLoaderConstant.Category.Jump.Name)
			{
				return false;
			}

			if (section.Name == ModuleTreeLoaderConstant.Section.Favorites.Name)
			{
				return true;
			}

			if (section.Name == ModuleTreeLoaderConstant.Section.RecentModules.Name)
			{
				return true;
			}

			return false;
		}

		public void TestLoadedModulesHaveUniqueModuleTreeIDs()
		{
			Loader.LoadModules();

			var idsUsedByMultipleModules =
				from ModuleCategory category in Tree.Categories.ValuesIncludingHidden
				from ModuleSection section in category.Sections.ValuesIncludingHidden
				from IMainFormModule module in section.Modules.ValuesIncludingHidden
				where !IsUserDefinedModule(module)
				group module by module.ModuleTreeID into moduleGroup
				select new { ModuleTreeID = moduleGroup.Key, Modules = moduleGroup.GroupBy(module => module.ParentSection).Select(grp => grp.First()) };

			idsUsedByMultipleModules = idsUsedByMultipleModules.Where(x => x.Modules.Count() > 1);

			idsUsedByMultipleModules = idsUsedByMultipleModules.Where(item =>
			{
				// Transport and PortTransport sections are mutally exclusive so its ok for their modules to have same ids
				if (item.Modules.All(module =>
						module.ParentSection.Name == ModuleTreeLoaderConstant.Section.Transport.Name ||
						module.ParentSection.Name == ModuleTreeLoaderConstant.Section.PortTransport.Name))
				{
					return false;
				}

				return true;
			});

			if (idsUsedByMultipleModules.Any())
			{
				CombineAssertions(() =>
				{
					foreach (var idUsedByMultipleModules in idsUsedByMultipleModules)
					{
						Fail(string.Format("ModuleTreeID [{0}] is not unique. The following modules have this ID: {1}", idUsedByMultipleModules.ModuleTreeID, string.Join(", ", idUsedByMultipleModules.Modules.Select(m => string.Format("[{0}][{1}][{2}]", m.ParentSection.ParentCategory.DisplayText, m.ParentSection.DisplayText, m.Description)))));
					}
				});
			}
			else
			{
				Assert("Loaded modules have unique ModuleTreeIDs", condition: true);
			}
		}

		public void TestModuleTree()
		{
			Loader.LoadModules();
			foreach (ModuleCategory category in Tree.Categories.Values)
			{
				foreach (ModuleSection section in category.Sections.Values)
				{
					foreach (MainFormModule module in section.Modules.Values)
					{
						using var zmodule = module.CreateZModule();
						AssertNotNull("Should have a Module for this module ID: " + module.ID, zmodule);
					}
				}
			}
		}

		public void TestAllSecurityItemsHaveParentConsistentWithModuleParent()
		{
			Loader.LoadModules();

			var errorList = new StringCollectionX();

			foreach (ModuleCategory category in Tree.Categories.Values)
			{
				foreach (ModuleSection section in category.Sections.Values)
				{
					foreach (MainFormModule module in section.Modules.Values)
					{
						using var zmodule = module.CreateZModule();
						if (zmodule.SecurityCheckpoint == null)
						{
							_ = errorList.Add(module.Description + " SecurityCheckpoint is null ");
						}
						else if (zmodule.SecurityCheckpoint == Env.Security.None)
						{
							// do nothing
						}
						else if (zmodule.SecurityCheckpoint.Parent == null)
						{
							_ = errorList.Add(module.Description + "'s SecurityCheckpoint " + zmodule.SecurityCheckpoint.Code + "'s Parent CheckPoint is null" + System.Environment.NewLine);
						}
						else if (!zmodule.BypassParentSecurityCheckpointVerification && section.SecurityCheckpoint != null && section.SecurityCheckpoint.Code != zmodule.SecurityCheckpoint.Parent.Code)
						{
							_ = errorList.Add(module.Description + "'s Parent Section's SecurityCheckpoint is " + section.SecurityCheckpoint.Code + " but it's own SecurityCheckpoint's Parent CheckPoint is " + zmodule.SecurityCheckpoint.Parent.Code + System.Environment.NewLine);
						}
					}
				}
			}

			if (errorList.Count == 0)
			{
				Assert(true);
			}
			else
			{
				Fail("The following modules' SecurityCheckpoint's parent is not the parent section's SecurityCheckpoint. This is inconsistent. This needs to be fixed by allocating the correct parent for the SecurityCheckpoint for the module." + System.Environment.NewLine + errorList.ToString());
			}
		}

		public void TestFavoriteSection_SecurityCheckPointDisplayTextShouldBeSameAsModuleInfoDescription()
		{
			_ = RecentItemManager.Instance.AddToFavoriteModules(new LinkWrapper(ModuleIDs.UnapprovedIntercompanyTransaction.Name, Guid.NewGuid(), null, ModuleIDs.UnapprovedIntercompanyTransaction.Description));
			Loader.LoadModules();
			var favoriteModules = Tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name].Sections[ModuleTreeLoaderConstant.Section.Favorites.Name].Modules.Values.ToList<MainFormModule>();
			var module = favoriteModules.First(x => Equals(x.ModuleID, ModuleIDs.UnapprovedIntercompanyTransaction));
			AssertEquals("should be same", module.SecurityCheckpoint.DisplayText, ModuleIDs.UnapprovedIntercompanyTransaction.Description);

			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			_ = RecentItemManager.Instance.AddToFavoriteModules(new LinkWrapper(ModuleIDs.UnapprovedTransaction.Name, Guid.NewGuid(), null, ModuleIDs.UnapprovedTransaction.Description));
			Loader.LoadModules();
			favoriteModules = Tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name].Sections[ModuleTreeLoaderConstant.Section.Favorites.Name].Modules.Values.ToList<MainFormModule>();
			module = favoriteModules.First(x => Equals(x.ModuleID, ModuleIDs.UnapprovedTransaction));
			AssertEquals("should be same", module.SecurityCheckpoint.DisplayText, ModuleIDs.UnapprovedTransaction.Description);
		}

		public void TestRecentItems_SecurityCheckPointDisplayTextShouldBeSameAsModuleInfoDescription()
		{
			RecentItemManager.Instance.AddOrUpdateRecentItems(string.Empty, new LinkWrapper(ModuleIDs.UnapprovedIntercompanyTransaction.Name, Guid.NewGuid(), null, ModuleIDs.UnapprovedIntercompanyTransaction.Description));
			Loader.LoadModules();
			var recentItemsModules = Tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name].Sections[ModuleTreeLoaderConstant.Section.RecentItems.Name].Modules.Values.ToList<MainFormModule>();
			var module = recentItemsModules.First(x => Equals(x.ModuleID, ModuleIDs.UnapprovedIntercompanyTransaction));
			AssertEquals("should be same", module.SecurityCheckpoint.DisplayText, ModuleIDs.UnapprovedIntercompanyTransaction.Description);

			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			RecentItemManager.Instance.AddOrUpdateRecentItems(string.Empty, new LinkWrapper(ModuleIDs.UnapprovedTransaction.Name, Guid.NewGuid(), null, ModuleIDs.UnapprovedTransaction.Description));
			Loader.LoadModules();
			recentItemsModules = Tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name].Sections[ModuleTreeLoaderConstant.Section.RecentItems.Name].Modules.Values.ToList<MainFormModule>();
			module = recentItemsModules.First(x => Equals(x.ModuleID, ModuleIDs.UnapprovedTransaction));
			AssertEquals("should be same", module.SecurityCheckpoint.DisplayText, ModuleIDs.UnapprovedTransaction.Description);
		}

		public void TestLoadModulesWithEmptyCountry()
		{
			var factory = new BusinessObjectFactory();

			var company = factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = string.Empty;

			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();

			factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertNoExceptionThrown("Loader should load all modules without any errors when the current company doesn't have a valid country code.", () => Loader.LoadModules());
			}
		}

		#endregion

		#region System

		public void TestSystemModules()
		{
			Loader.LoadModules();

			var modules = FindModulesById(
				ModuleIDs.GlbCompany,
				ModuleIDs.GlbBranch,
				ModuleIDs.GlbDepartment,
				ModuleIDs.GlbGroup,
				ModuleIDs.GlbStaff,
				ModuleIDs.Registry,
				ModuleIDs.UserAdminReports,
				ModuleIDs.Messaging.EDIInterchange,
				ModuleIDs.Messaging.EDIMessage,
				ModuleIDs.ActiveUsers,
				ModuleIDs.ScheduledReports,
				ModuleIDs.LicenceUsage,
				ModuleIDs.PrintJob,
				ModuleIDs.PrintQueue,
				ModuleIDs.DocumentSigningJob,
				ModuleIDs.SystemReports,
				ModuleIDs.StmUpgrade,
				ModuleIDs.MailItem,
				ModuleIDs.MailItemTemplate);

			CombineAssertions(() =>
			{
				foreach (var (id, mainForm) in modules)
				{
					AssertNotNull(id, mainForm);
				}

				AssertNull(ModuleIDs.StmFeatureTest.ToString(), Tree.FindByID(ModuleIDs.StmFeatureTest.ToString()));
			});
		}

		const string GlowRestrictedModulesOverrideRegistryName = "GlowRestrictedModulesOverride";

		public void TestHRMSNotVisibleWhenNotEnabled()
		{
			_ = Db.Connection.ExecuteNonQuery(@"DELETE FROM dbo.StmData WHERE SD_Name=@name", p => p.AddParameter("@name", System.Data.SqlDbType.VarChar, GlowRestrictedModulesOverrideRegistryName));

			Loader.LoadModules();
			AssertNull(Tree.FindByID(ModuleIDs.GlowHRMS.ToString()));
		}

		public void TestHRMSVisibleWhenEnabled()
		{
			// Taken directly from my local db
			const string serialisedValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ArrayOfstring xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\"><string>HRM</string></ArrayOfstring>";

			_ = Db.Connection.ExecuteNonQuery(@"DELETE FROM dbo.StmData WHERE SD_Name=@name", p => p.AddParameter("@name", System.Data.SqlDbType.VarChar, GlowRestrictedModulesOverrideRegistryName));

			// GLOW doesn't use compression here, we need UTF-8 stored as binary blob
			_ = Db.Connection.ExecuteNonQuery(@"INSERT INTO dbo.StmData (SD_Name, SD_BinaryValue, SD_PK) VALUES (@name, CONVERT(varbinary(max), @value), NEWID())", p =>
			{
				p.AddParameter("@name", System.Data.SqlDbType.VarChar, GlowRestrictedModulesOverrideRegistryName);
				p.AddParameter("@value", System.Data.SqlDbType.VarChar, serialisedValue);
			});

			Loader.LoadModules();
			AssertNotNull(Tree.FindByID(ModuleIDs.GlowHRMS.ToString()));
		}

		public void TestSystemFeatureTestAppearsWhenEnabled()
		{
			DataRegistry.Instance.FeatureTestModeEnabled = true;

			Loader.LoadModules();

			AssertNotNull("StmFeatureTest", Tree.FindByID(ModuleIDs.StmFeatureTest.ToString()));
		}

		public void TestErrorReporting_DoesNotAppear_ForRegularUser()
		{
			GlbStaff.CurrentUser.GS_LoginName = "test_notsupport";
			GlbStaff.CurrentUser.GS_IsController = false;

			Assert("Sanity check - should not be support user", !Env.CurrentUser.IsSupportUser);
			Assert("Sanity check - should not be controller", !Env.CurrentUser.IsController);

			Loader.LoadModules();

			AssertNull("ErrorReporting", Tree.FindByID(ModuleIDs.ErrorReporting.ToString()));
		}

		public void TestErrorReporting_DoesNotAppear_ForControllerUser()
		{
			GlbStaff.CurrentUser.GS_LoginName = "test_notsupport";
			GlbStaff.CurrentUser.GS_IsController = true;

			Assert("Sanity check - should not be support user", !Env.CurrentUser.IsSupportUser);
			Assert("Sanity check - should be controller", Env.CurrentUser.IsController);

			Loader.LoadModules();

			AssertNull("ErrorReporting", Tree.FindByID(ModuleIDs.ErrorReporting.ToString()));
		}

		public void TestErrorReporting_Appears_ForSupportUser()
		{
			Assert("Sanity check - should be support user", Env.CurrentUser.IsSupportUser);

			Loader.LoadModules();

			AssertNotNull("ErrorReporting", Tree.FindByID(ModuleIDs.ErrorReporting.ToString()));
		}

		public void TestErrorReportingAppearsForRegularUserOnEDISystem()
		{
			GlbStaff.CurrentUser.GS_LoginName = "test_notsupport";
			GlbStaff.CurrentUser.GS_IsController = false;

			Assert("Sanity check - should not be support user", !Env.CurrentUser.IsSupportUser);
			Assert("Sanity check - should not be controller", !Env.CurrentUser.IsController);

			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				Loader.LoadModules();
			}

			AssertNotNull("ErrorReporting", Tree.FindByID(ModuleIDs.ErrorReporting.ToString()));
		}

		public void TestModuleNameEquivalence()
		{
			var securityInstance = Env.Security;

			AssertEquals("Operations." + ModuleTreeLoaderConstant.Section.Schedules.Name, securityInstance.Schedules.ToString());
			AssertEquals("Operations." + ModuleTreeLoaderConstant.Section.Forwarding.Name, securityInstance.Forwarding.ToString());
			AssertEquals("Operations." + ModuleTreeLoaderConstant.Section.CustomsMain.Name, securityInstance.CustomsMain.ToString());
			AssertEquals("Operations." + ModuleTreeLoaderConstant.Section.OrderManager.Name, securityInstance.OrderManager.ToString());
			AssertEquals("Operations." + ModuleTreeLoaderConstant.Section.Transport.Name, securityInstance.Transport.ToString());
			AssertEquals("Operations." + ModuleTreeLoaderConstant.Section.CFSCTO.Name, securityInstance.CFSCTO.ToString());
			AssertEquals("Operations." + ModuleTreeLoaderConstant.Section.Warehouse.Name, securityInstance.Warehouse.ToString());
			AssertEquals("Operations." + ModuleTreeLoaderConstant.Section.TransitWarehouse.Name, securityInstance.TransitWarehouse.ToString());
			AssertEquals("Operations." + ModuleTreeLoaderConstant.Section.LinerAndAgency.Name, securityInstance.LinerAndAgency.ToString());
			AssertEquals("Operations." + ModuleTreeLoaderConstant.Section.StampDutyMain.Name, securityInstance.StampDutyMain.ToString());
			AssertEquals("Manage." + ModuleTreeLoaderConstant.Section.DocManager.Name, securityInstance.DocManager.ToString());
			AssertEquals("Manage." + ModuleTreeLoaderConstant.Section.Receivables.Name, securityInstance.Receivables.ToString());
			AssertEquals("Manage." + ModuleTreeLoaderConstant.Section.Payables.Name, securityInstance.Payables.ToString());
			AssertEquals("Manage." + ModuleTreeLoaderConstant.Section.CashBook.Name, securityInstance.CashBook.ToString());
			AssertEquals("Manage." + ModuleTreeLoaderConstant.Section.JobCosting.Name, securityInstance.JobCosting.ToString());
			AssertEquals("Manage." + ModuleTreeLoaderConstant.Section.GeneralLedger.Name, securityInstance.GeneralLedger.ToString());
			AssertEquals("Manage." + ModuleTreeLoaderConstant.Section.ClientRelationshipManagement.Name, securityInstance.ClientRelationshipManagement.ToString());
			AssertEquals("Manage.BudgetsSection." + ModuleTreeLoaderConstant.Section.Budgets.Name, securityInstance.Budgets.ToString());

			AssertEquals("Config." + ModuleTreeLoaderConstant.Section.References.Name, securityInstance.References.ToString());
			AssertEquals("Config." + ModuleTreeLoaderConstant.Section.RelationshipManagerConfig.Name, securityInstance.RelationshipManagerConfig.ToString());
			AssertEquals("Config." + ModuleTreeLoaderConstant.Section.Location.Name, securityInstance.Location.ToString());
			AssertEquals("Config." + ModuleTreeLoaderConstant.Section.Account.Name, securityInstance.Account.ToString());
			AssertEquals("Config." + ModuleTreeLoaderConstant.Section.CustomsFiles.Name, securityInstance.CustomsFiles.ToString());
			AssertEquals("Config." + ModuleTreeLoaderConstant.Section.WhsConfig.Name, securityInstance.WhsConfig.ToString());
			AssertEquals("Config." + ModuleTreeLoaderConstant.Section.PeopleOperations.Name, securityInstance.PeopleOperations.ToString());
			AssertEquals("Config." + ModuleTreeLoaderConstant.Section.LearningDevelopment.Name, securityInstance.LearningDevelopment.ToString());
			AssertEquals("Config." + ModuleTreeLoaderConstant.Section.HRRecruiter.Name, securityInstance.HRRecruiter.ToString());
			AssertEquals("Config." + ModuleTreeLoaderConstant.Section.System.Name, securityInstance.System.ToString());
		}

		public void TestAllSectionsHaveSubcategories()
		{
			var countryList = new List<string>();
			countryList.AddRange(Constants.CountryCodes.EuCommonTransitCountries);
			countryList.AddRange(ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers());
			countryList.AddRange(new string[]
			{
				Constants.CountryCodes.Australia,
				Constants.CountryCodes.Canada,
				Constants.CountryCodes.China,
				Constants.CountryCodes.Indonesia,
				Constants.CountryCodes.Japan,
				Constants.CountryCodes.Mexico,
				Constants.CountryCodes.Peru,
				Constants.CountryCodes.Singapore,
				Constants.CountryCodes.SouthAfrica,
				Constants.CountryCodes.Taiwan,
				Constants.CountryCodes.UnitedStates,
			});

			foreach (var country in countryList)
			{
				AssertAllSectionsHaveSubcategories(country);
			}
		}

		#endregion

		#region Warehouse

		public void TestWhsModulesCore()
		{
			using (WarehouseDataRegistry.Instance.EnableDynamicWorkOrder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Loader.LoadModules();
			}

			// operations
			var operationModules = FindModulesById(
				ModuleIDs.WhsReceive,
				ModuleIDs.WhsOrder,
				ModuleIDs.WhsWorkOrder,
				ModuleIDs.WhsDynamicWorkOrder,
				ModuleIDs.WhsVASOrder,
				ModuleIDs.WhsPicking,
				ModuleIDs.Packing,
				ModuleIDs.WhsRelease,
				ModuleIDs.WhsTransfer,
				ModuleIDs.WhsAdjustment,
				ModuleIDs.WhsStocktake,
				ModuleIDs.WhsInventory,
				ModuleIDs.WhsInvoicing,
				ModuleIDs.WhsReport,
				ModuleIDs.WhsAdHocServiceJob,
				ModuleIDs.WhsProductWarehousePortal);

			// config
			var configModules = FindModulesById(
				ModuleIDs.WhsConfigWarehouse,
				ModuleIDs.WhsConfigRow,
				ModuleIDs.WhsConfigArea,
				ModuleIDs.WhsConfigProduct,
				ModuleIDs.WhsConfigProductStyle,
				ModuleIDs.WhsCartonSize,
				ModuleIDs.WhsCartonGroup,
				ModuleIDs.WhsConfigLocationType,
				ModuleIDs.WhsConfigPutawayGroup,
				ModuleIDs.WhsSalesChannel,
				ModuleIDs.WhsConfigPickFaces);

			CombineAssertions(() =>
			{
				// Operations
				AssertNotNull(Tree.Categories[ModuleTreeLoaderConstant.Category.Operations.Name].Sections[ModuleTreeLoaderConstant.Section.Warehouse.Name]);
				foreach (var (id, mainForm) in operationModules)
				{
					AssertNotNull(id, mainForm);
					_ = AssertModuleAdded($"{id} should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.Warehouse, id);
				}

				// Config
				AssertNotNull(Tree.Categories[ModuleTreeLoaderConstant.Category.Admin.Name].Sections[ModuleTreeLoaderConstant.Section.WhsConfig.Name]);
				foreach (var (id, mainForm) in configModules)
				{
					AssertNotNull(mainForm);
					_ = AssertModuleAdded($"{id} should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.WhsConfig, id);
				}
			});
		}

		public void TestWhsModulesCore_RegistryDisabled()
		{
			using (WarehouseDataRegistry.Instance.EnableDynamicWorkOrder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Loader.LoadModules();
			}

			// operations
			var operationsModules = FindModulesById(
				ModuleIDs.WhsReceive,
				ModuleIDs.WhsOrder,
				ModuleIDs.WhsWorkOrder,
				ModuleIDs.WhsVASOrder,
				ModuleIDs.WhsPicking,
				ModuleIDs.Packing,
				ModuleIDs.WhsRelease,
				ModuleIDs.WhsTransfer,
				ModuleIDs.WhsAdjustment,
				ModuleIDs.WhsStocktake,
				ModuleIDs.WhsInventory,
				ModuleIDs.WhsInvoicing,
				ModuleIDs.WhsReport,
				ModuleIDs.WhsAdHocServiceJob,
				ModuleIDs.WhsProductWarehousePortal);

			// config
			var configModules = FindModulesById(
				ModuleIDs.WhsConfigWarehouse,
				ModuleIDs.WhsConfigRow,
				ModuleIDs.WhsConfigArea,
				ModuleIDs.WhsConfigProduct,
				ModuleIDs.WhsConfigProductStyle,
				ModuleIDs.WhsCartonSize,
				ModuleIDs.WhsCartonGroup,
				ModuleIDs.WhsConfigLocationType,
				ModuleIDs.WhsConfigPutawayGroup,
				ModuleIDs.WhsSalesChannel,
				ModuleIDs.WhsConfigPickFaces,
				ModuleIDs.WhsConfigDynamicPickFaces);

			CombineAssertions(() =>
			{
				// Operations
				AssertNotNull(Tree.Categories[ModuleTreeLoaderConstant.Category.Operations.Name].Sections[ModuleTreeLoaderConstant.Section.Warehouse.Name]);
				foreach (var (id, mainForm) in operationsModules)
				{
					AssertNotNull(mainForm);
					_ = AssertModuleAdded($"{id} should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.Warehouse, id);
				}

				// Config
				AssertNotNull(Tree.Categories[ModuleTreeLoaderConstant.Category.Admin.Name].Sections[ModuleTreeLoaderConstant.Section.WhsConfig.Name]);
				foreach (var (id, mainForm) in configModules)
				{
					AssertNotNull(mainForm);
					_ = AssertModuleAdded($"{id} should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.WhsConfig, id);
				}
			});
		}

		public void TestWhsModules_PickFacesDoesAppear_ForRegularUser()
		{
			GlbStaff.CurrentUser.GS_LoginName = "test_notsupport";
			GlbStaff.CurrentUser.GS_IsController = false;

			Assert("Sanity check - should not be support user", !Env.CurrentUser.IsSupportUser);
			Assert("Sanity check - should not be controller", !Env.CurrentUser.IsController);

			Loader.LoadModules();

			AssertNotNull("ErrorReporting", Tree.FindByID(ModuleIDs.WhsConfigPickFaces.ToString()));
		}

		public void TestWhsModules_DynamicPickFacesDoesAppear_ForRegularUser()
		{
			GlbStaff.CurrentUser.GS_LoginName = "test_notsupport";
			GlbStaff.CurrentUser.GS_IsController = false;

			Assert("Sanity check - should not be support user", !Env.CurrentUser.IsSupportUser);
			Assert("Sanity check - should not be controller", !Env.CurrentUser.IsController);

			Loader.LoadModules();

			AssertNotNull("ErrorReporting", Tree.FindByID(ModuleIDs.WhsConfigDynamicPickFaces.ToString()));
		}

		public void TestWhsModules_LoadPlanningModule()
		{
			Loader.LoadModules();
			AssertNotNull(Tree.FindByID(ModuleIDs.WhsLoad.ToString()));
		}

		#endregion

		#region TransitWarehouse

		public void TestTransitWarehouse()
		{
			Loader.LoadModules();

			AssertNotNull(Tree.Categories[ModuleTreeLoaderConstant.Category.Operations.Name].Sections[ModuleTreeLoaderConstant.Section.TransitWarehouse.Name]);
			var modules = FindModulesById(
				ModuleIDs.TransitWarehousePortal,
				ModuleIDs.WhsItemReceiveTransportationUnit,
				ModuleIDs.WhsTransitReceiveConsignment,
				ModuleIDs.WhsTransitDispatchConsignment,
				ModuleIDs.WhsTransitReport,
				ModuleIDs.WhsItemReceiveASN,
				ModuleIDs.WhsItemDispatchTransportationUnit,
				ModuleIDs.TransitHandlingUnit,
				ModuleIDs.WhsItemDispatchLoadList,
				ModuleIDs.WhsItemTransferHeader);

			CombineAssertions(() =>
			{
				foreach (var (id, mainForm) in modules)
				{
					AssertNotNull(id, mainForm);
				}
			});
		}

		#endregion

		#region ContainerYard

		public void TestWhenContainYardRegistryIsEnabled_ThenContainerYardModuleSectionIsVisible()
		{
			using (WarehouseDataRegistry.Instance.EnableContainerYard.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Loader.LoadModules();
				var modules = FindModulesById(
					ModuleIDs.CYDReceiveAdvice,
					ModuleIDs.CYDReleaseAdvice,
					ModuleIDs.ContainerYardPortal,
					ModuleIDs.CYDTransportationUnit,
					ModuleIDs.CYDYardUnitState,
					ModuleIDs.CYDYardReport);

				CombineAssertions(() =>
				{
					foreach (var (id, mainForm) in modules)
					{
						AssertNotNull(id, mainForm);
					}
				});
			}
		}

		public void TestWhenContainYardRegistryIsNotEnabled_ThenContainerYardModuleSectionIsNotVisible()
		{
			using (WarehouseDataRegistry.Instance.EnableContainerYard.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Loader.LoadModules();
				var modules = FindModulesById(
					ModuleIDs.CYDReceiveAdvice,
					ModuleIDs.CYDReleaseAdvice,
					ModuleIDs.ContainerYardPortal,
					ModuleIDs.CYDTransportationUnit,
					ModuleIDs.CYDYardUnitState);

				CombineAssertions(() =>
				{
					foreach (var (id, mainForm) in modules)
					{
						AssertNull(id, mainForm);
					}
				});
			}
		}

		#endregion

		#region GateManagement

		public void TestWhenGateManagementRegistryIsEnabled_ThenGateManagementModuleSectionIsVisible()
		{
			using (WarehouseDataRegistry.Instance.EnableGateManagement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Loader.LoadModules();
				AssertNotNull(Tree.FindByID(ModuleIDs.GteBooking.ToString()));
				AssertNotNull(Tree.FindByID(ModuleIDs.GteGateMovementBooking.ToString()));
				AssertNotNull(Tree.FindByID(ModuleIDs.GteVehicleMovement.ToString()));
				AssertNotNull(Tree.FindByID(ModuleIDs.GteGateMovement.ToString()));
				AssertNotNull(Tree.FindByID(ModuleIDs.GateManagementPortal.ToString()));
			}
		}

		public void TestWhenGateManagementRegistryIsNotEnabled_ThenGateManagementModuleSectionIsNotVisible()
		{
			using (WarehouseDataRegistry.Instance.EnableGateManagement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Loader.LoadModules();
				AssertNull(Tree.FindByID(ModuleIDs.GteBooking.ToString()));
				AssertNull(Tree.FindByID(ModuleIDs.GteGateMovementBooking.ToString()));
				AssertNull(Tree.FindByID(ModuleIDs.GteVehicleMovement.ToString()));
				AssertNull(Tree.FindByID(ModuleIDs.GteGateMovement.ToString()));
				AssertNull(Tree.FindByID(ModuleIDs.GateManagementPortal.ToString()));
			}
		}

		#endregion

		#region LocalTransport

		public void TestLocalTransport()
		{
			Loader.LoadModules();

			AssertNotNull(Tree.Categories[ModuleTreeLoaderConstant.Category.Operations.Name].Sections[ModuleTreeLoaderConstant.Section.Transport.Name]);

			var modules = FindModulesById(
				ModuleIDs.Cartage,
				ModuleIDs.CartageWorkSheet,
				ModuleIDs.CartageRunSheetDashboard,
				ModuleIDs.CartageLeg,
				ModuleIDs.CartageLegPlanner,
				ModuleIDs.TransportReports,
				ModuleIDs.CartageType);

			CombineAssertions(() =>
			{
				foreach (var (id, mainForm) in modules)
				{
					AssertNotNull(id, mainForm);
				}
			});
		}

		#endregion

		#region TransportConsignment

		public void TestTransportConsignmentWhenEnabledLandTransport()
		{
			ObjectFactory.Get<ITransportRegistry>().EnableLandTransport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Loader.LoadModules();

			var modules = FindModulesById(
				ModuleIDs.DtbConsignment,
				ModuleIDs.DtbConsignmentRunSheet,
				ModuleIDs.DtbReports,
				ModuleIDs.DtbConsignmentWebPortal,
				ModuleIDs.DtbConsignmentRunSheetWebPortal);

			CombineAssertions(() =>
			{
				AssertNotNull(Tree.Categories[ModuleTreeLoaderConstant.Category.Operations.Name].Sections[ModuleTreeLoaderConstant.Section.TransportConsignment.Name]);
				foreach (var (id, mainForm) in modules)
				{
					AssertNotNull(id, mainForm);
				}
			});
		}

		public void TestTransportConsignmentWhenDisabledLandTransport()
		{
			ObjectFactory.Get<ITransportRegistry>().EnableLandTransport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			Loader.LoadModules();
			CombineAssertions(() =>
			{
				AssertNotNull(Tree.Categories[ModuleTreeLoaderConstant.Category.Operations.Name].Sections[ModuleTreeLoaderConstant.Section.TransportConsignment.Name]);

				var enableModules = FindModulesById(
					ModuleIDs.DtbConsignment,
					ModuleIDs.DtbConsignmentRunSheet,
					ModuleIDs.DtbReports);
				foreach (var (id, mainForm) in enableModules)
				{
					AssertNotNull(id, mainForm);
				}

				var disabledModules = FindModulesById(
					ModuleIDs.DtbConsignmentWebPortal,
					ModuleIDs.DtbConsignmentRunSheetWebPortal);
				foreach (var (id, mainForm) in disabledModules)
				{
					AssertNull(id, mainForm);
				}
			});
		}

		#endregion

		#region Admin

		public void TestBarcodeParsing()
		{
			Loader.LoadModules();

			AssertNotNull(Tree.FindByID(ModuleIDs.BarcodeParsing.ToString()));
		}

		public void TestBarcodeValidation()
		{
			Loader.LoadModules();

			AssertNotNull(Tree.FindByID(ModuleIDs.BarcodeValidation.ToString()));
		}

		public void TestWhsInventoryHeldCodes()
		{
			Loader.LoadModules();

			AssertNotNull(Tree.FindByID(ModuleIDs.WhsInventoryHeldCodes.ToString()));
		}

		public void TestPremisesGateCodes()
		{
			Loader.LoadModules();
			var refPremisesGateCodes = Tree.FindByID(ModuleIDs.RefPremisesGateCode.ToString()) != null;
			Assert("Premises Gate Codes Module not loaded", refPremisesGateCodes);
		}

		public void TestNMFCModuleIsAddedForUSOnly()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.UnitedStates);
			var moduleName = ModuleIDs.RefNMFC.Name;
			Loader.LoadModules();
			_ = AssertModuleAdded("NMFC Module should be loaded if the country is US.", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.References, moduleName);

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);
			Loader.LoadModules();
			_ = AssertModuleAdded("NMFC Module should NOT be loaded if the country is not US.", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.References, moduleName);
		}

		public void TestRecruiterModules()
		{
			Loader.LoadModules();

			CombineAssertions(() =>
			{
				var modules = FindModulesById(
				ModuleIDs.HRJobOpenings,
				ModuleIDs.HRJobApplicant,
				ModuleIDs.HRJobApplication,
				ModuleIDs.HRJobRole,
				ModuleIDs.HRReports,
				ModuleIDs.HREmails);

				foreach (var (id, mainForm) in modules)
				{
					AssertNotNull(id, mainForm);
				}
			});

			AssertRecruitmentModuleEnabledState(ModuleIDs.RecruitmentCandidateManagement, moduleShouldExist: false);

			ObjectFactory.Get<IRecruitmentRegistry>().RecruitmentModuleEnabled = true;
			Loader.LoadModules();

			AssertRecruitmentModuleEnabledState(ModuleIDs.RecruitmentCandidateManagement, moduleShouldExist: true);

			void AssertRecruitmentModuleEnabledState(ModuleIdentifier moduleId, bool moduleShouldExist)
				=> AssertModuleAdded(
					$"{moduleId} should {(moduleShouldExist ? string.Empty : "not ")}be loaded if RecruitmentModuleEnabled is {moduleShouldExist}.",
					expectModuleToExist: moduleShouldExist,
					ModuleTreeLoaderConstant.Category.Admin,
					ModuleTreeLoaderConstant.Section.HRRecruiter,
					moduleId.ToString());
		}

		public void TestLocationModules()
		{
			Loader.LoadModules();

			var modules = FindModulesById(
				ModuleIDs.RefCountry,
				ModuleIDs.RefCountryStates,
				ModuleIDs.RefCityTown,
				ModuleIDs.RefPostCode,
				ModuleIDs.GenShapeGeography,
				ModuleIDs.RefUNLOCO,
				ModuleIDs.InternationalZone,
				ModuleIDs.RefTimeZoneSet,
				ModuleIDs.RateTransportProvider,
				ModuleIDs.TradeLane,
				ModuleIDs.GlbPortDeliveryTime,
				ModuleIDs.PortHubSelection,
				ModuleIDs.RefTransitTime,
				ModuleIDs.LocationsReports);

			CombineAssertions(() =>
			{
				foreach (var (id, mainForm) in modules)
				{
					AssertNotNull(id, mainForm);
				}
			});

			AssertModuleNullability(ModuleIDs.RefShippingLine, ReferenceFilesDataRegistry.Instance.EnableShippingLineReferenceFile, shouldBeNull: true, registryValue: false);
			AssertModuleNullability(ModuleIDs.RefShippingLine, ReferenceFilesDataRegistry.Instance.EnableShippingLineReferenceFile, shouldBeNull: false, registryValue: true);
			AssertModuleNullability(ModuleIDs.PortDepotCarrierSelection, HVLVDataRegistry.Instance.PortCarrierDepotSelectionModule, shouldBeNull: false, registryValue: true);

			AssertNotNull(Tree.FindByID(ModuleIDs.RefComplianceCommodityAlert.ToString()));
			AssertNotNull(Tree.FindByID(ModuleIDs.RefComplianceList.ToString()));

			AssertModuleNullability(ModuleIDs.RouteSegments, OceanCarrierDataRegistry.Instance.EnableOceanCarrierSolution, shouldBeNull: true, registryValue: false);
			AssertModuleNullability(ModuleIDs.RouteSegments, OceanCarrierDataRegistry.Instance.EnableOceanCarrierSolution, shouldBeNull: false, registryValue: true);

			void AssertModuleNullability(ModuleIdentifier moduleIdentifier, RegistryItemWrapper registryItem, bool shouldBeNull, bool registryValue)
			{
				using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
				{
					Loader.LoadModules();
					if (shouldBeNull)
					{
						AssertNull(Tree.FindByID(moduleIdentifier.ToString()));
					}
					else
					{
						AssertNotNull(Tree.FindByID(moduleIdentifier.ToString()));
					}
				}
			}
		}

		#endregion

		#region Booking

		public void TestSchedulesSection()
		{
			Loader.LoadModules();
			var section = Tree.Categories[ModuleTreeLoaderConstant.Category.Operations.Name].Sections[ModuleTreeLoaderConstant.Section.Schedules.Name];
			var modules = section.Modules.Values.ToArray();

			AssertEquals("Expected 8 modules", 8, modules.Length);
			AssertEquals(ModuleIDs.JobSeaSailing, modules[0].ModuleID);
			AssertEquals(ModuleIDs.JobAirSailing, modules[1].ModuleID);
			AssertEquals(ModuleIDs.JobRailSailing, modules[2].ModuleID);
			AssertEquals(ModuleIDs.JobRoadSailing, modules[3].ModuleID);
			AssertEquals(ModuleIDs.SailingDataVendorImporting, modules[4].ModuleID);
			AssertEquals(ModuleIDs.OnlineSailingSchedules, modules[5].ModuleID);
			AssertEquals(ModuleIDs.RoutingLookups, modules[6].ModuleID);
			AssertEquals(ModuleIDs.BookingsReports, modules[7].ModuleID);
		}

		public void TestScheduleSection_OnlineSailingSchedulesDisabled()
		{
			using (FreightDataRegistry.Instance.EnableScheduleFeedService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Loader.LoadModules();
				var section = Tree.Categories[ModuleTreeLoaderConstant.Category.Operations.Name].Sections[ModuleTreeLoaderConstant.Section.Schedules.Name];
				var modules = section.Modules.Values.ToArray();

				AssertEquals("Expected 7 modules", 7, modules.Length);
				AssertEquals(ModuleIDs.JobSeaSailing, modules[0].ModuleID);
				AssertEquals(ModuleIDs.JobAirSailing, modules[1].ModuleID);
				AssertEquals(ModuleIDs.JobRailSailing, modules[2].ModuleID);
				AssertEquals(ModuleIDs.JobRoadSailing, modules[3].ModuleID);
				AssertEquals(ModuleIDs.SailingDataVendorImporting, modules[4].ModuleID);
				AssertEquals(ModuleIDs.RoutingLookups, modules[5].ModuleID);
				AssertEquals(ModuleIDs.BookingsReports, modules[6].ModuleID);
			}
		}

		#endregion

		#region Forwarding

		public void TestForwardingModules()
		{
			Loader.LoadModules();

			var modules = FindModulesById(
				ModuleIDs.QuotedBookings,
				ModuleIDs.JobShipment,
				ModuleIDs.JobConsol,
				ModuleIDs.ConsolPlanningBoard,
				ModuleIDs.Containers,
				ModuleIDs.ConsolidatedTransportBooking,
				ModuleIDs.ForwardingReport);

			CombineAssertions(() =>
			{
				foreach (var (id, mainForm) in modules)
				{
					AssertNotNull(id, mainForm);
				}
			});
		}

		public void TestDocumentTrackingModuleIsAddedForChinaOnly()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);
			using (ZModule testModule = new Freight.Forwarding.Module.DocumentTrackingModule())
			{
				var moduleName = testModule.ID.ToString();
				Loader.LoadModules();

				_ = AssertModuleAdded("Document Tracking Module should be loaded if the country is China.", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.Forwarding, moduleName);
			}

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);
			using (ZModule testModule = new Freight.Forwarding.Module.DocumentTrackingModule())
			{
				var moduleName = testModule.ID.ToString();
				Loader.LoadModules();

				_ = AssertModuleAdded("Document Tracking Module should NOT be loaded if the country is not China.", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.Forwarding, moduleName);
			}
		}

		public void TestHVLVBookingHeaderAndConsignmentModules_Loaded()
		{
			Loader.LoadModules();
			_ = AssertModuleAdded("ETail enabled, HVLV Booking Header module should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.Forwarding, ModuleIDs.HVLVBookingHeader.Name);
			_ = AssertModuleAdded("ETail enabled, HVLV Consignment module should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.Forwarding, ModuleIDs.HVLVConsignment.Name);
		}

		public void TestHVLVOriginLoadListTestingMode_LoadedWhenRegistryEnabled()
		{
			using (HVLVDataRegistry.Instance.HVLVOriginLoadListTestingMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("Registry setting 'HVLVOriginLoadListTestingMode' is enabled, HVLV Origin LoadList module should be loaded",
					expectModuleToExist: true,
					ModuleTreeLoaderConstant.Category.Operations,
					ModuleTreeLoaderConstant.Section.Forwarding,
					ModuleIDs.HVLVOriginLoadList.Name);
			}

			Loader.LoadModules();
			_ = AssertModuleAdded("Registry setting 'HVLVOriginLoadListTestingMode' is disabled by default, HVLV Origin LoadList module should not be loaded",
				expectModuleToExist: false,
				ModuleTreeLoaderConstant.Category.Operations,
				ModuleTreeLoaderConstant.Section.Forwarding,
				ModuleIDs.HVLVOriginLoadList.Name);
		}

		public void TestPortDepotCarrierSelectionModule_LoadedWhenRegistryEnabled()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = false;
			using (HVLVDataRegistry.Instance.PortCarrierDepotSelectionModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("PortDepotCarrierSelection loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.Location, ModuleIDs.PortDepotCarrierSelection.Name);
				_ = AssertModuleAdded("PortHubSelection not loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.Location, ModuleIDs.PortHubSelection.Name);
			}
		}

		public void TestPortHubSelectionModule_LoadedWhenRegistryDisabled()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = false;
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("PortHubSelection loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.Location, ModuleIDs.PortHubSelection.Name);
				_ = AssertModuleAdded("PortDepotCarrierSelection not loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.Location, ModuleIDs.PortDepotCarrierSelection.Name);
			}
		}

		public void TestCarrierAndClientContractModules_LoadedWhenRegistryEnabled()
		{
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("CarrierContracts loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.Forwarding, ModuleIDs.CarrierContractAndAllocations.Name);
				_ = AssertModuleAdded("ClientContracts loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.Forwarding, ModuleIDs.ClientContractAndAllocations.Name);
			}
		}

		public void TestCarrierAndClientContractModules_NotLoadedWhenRegistryDisabled()
		{
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("CarrierContracts NOT loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.Forwarding, ModuleIDs.CarrierContractAndAllocations.Name);
				_ = AssertModuleAdded("ClientContracts NOT loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.Forwarding, ModuleIDs.ClientContractAndAllocations.Name);
			}
		}

		public void TestCO2eDashboardModule()
		{
			var mockCO2eFeatureControl = new Mock<ICO2eFeatureControlHelper>();
			mockCO2eFeatureControl.Setup(m => m.DashboardEnabled).Returns(true);

			using (ObjectFactory.Substitute(mockCO2eFeatureControl.Object))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("CO2eDashboard loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.Forwarding, ModuleIDs.CO2eDashboard.Name);
			}

			mockCO2eFeatureControl.Setup(m => m.DashboardEnabled).Returns(false);
			using (ObjectFactory.Substitute(mockCO2eFeatureControl.Object))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("CO2eDashboard NOT loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.Forwarding, ModuleIDs.CO2eDashboard.Name);
			}
		}

		public void TestMarketIntelligenceAndAnalyticsModule()
		{
			var mockMarketIntelligenceAndAnalyticsFeatureControl = new Mock<IMarketIntelligenceAndAnalyticsFeatureControlHelper>();
			mockMarketIntelligenceAndAnalyticsFeatureControl.Setup(m => m.Enabled).Returns(true);

			using (ObjectFactory.Substitute(mockMarketIntelligenceAndAnalyticsFeatureControl.Object))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("Market Intelligence And Analytics loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.Forwarding, ModuleIDs.MarketIntelligenceAndAnalytics.Name);
			}

			mockMarketIntelligenceAndAnalyticsFeatureControl.Setup(m => m.Enabled).Returns(false);
			using (ObjectFactory.Substitute(mockMarketIntelligenceAndAnalyticsFeatureControl.Object))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("Market Intelligence And Analytics NOT loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.Forwarding, ModuleIDs.MarketIntelligenceAndAnalytics.Name);
			}
		}

		#endregion

		#region Ocean Carrier

		public void TestOceanCarrierModulesShouldBeDisabledWhenItIsTurnedOffInTheRegistry()
		{
			using (OceanCarrierDataRegistry.Instance.EnableOceanCarrierSolution.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Loader.LoadModules();

				var modules = FindModulesById(
					ModuleIDs.OceanCarrierPortal,
					ModuleIDs.CarrierServices,
					ModuleIDs.CarrierShipmentHeader,
					ModuleIDs.EquipmentManagementPortal);

				CombineAssertions(() =>
				{
					foreach (var (id, mainForm) in modules)
					{
						AssertNull(id, mainForm);
					}
				});
			}
		}

		public void TestOceanCarrierModulesShouldBeEnabledWhenItIsTurnedOnInTheRegistry()
		{
			using (OceanCarrierDataRegistry.Instance.EnableOceanCarrierSolution.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Loader.LoadModules();

				var modules = FindModulesById(
					ModuleIDs.OceanCarrierPortal,
					ModuleIDs.CarrierServices,
					ModuleIDs.CarrierShipmentHeader,
					ModuleIDs.EquipmentManagementPortal);

				CombineAssertions(() =>
				{
					foreach (var (id, mainForm) in modules)
					{
						AssertNotNull(id, mainForm);
					}
				});
			}
		}

		#endregion

		#region Customs

		public void TestLoadCustomsRulesDependsOnRegistry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("Rules module added", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.CustomsRules.Name);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.PuertoRico))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("Rules module added", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.CustomsRules.Name);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("Rules module added", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.CustomsRules.Name);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("Rules module not added", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.CustomsRules.Name);
			}
		}

		public void TestCusCalculationRulesModule()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("Customs Calculation Rules module added", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.CusCalculationRules.Name);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("Customs Calculation Rules module not added", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.CusCalculationRules.Name);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Latvia))  // EU Test
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("Customs Calculation Rules module added", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.CusCalculationRules.Name);
			}
		}

		public void TestCusPackingListModule()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				var twRegistry = ObjectFactory.Get<Enterprise.Integration.Customs.TW.ITWCustomsRegistry>();
				twRegistry.CustomsPackingListEnable.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				Loader.LoadModules();
				_ = AssertModuleAdded("Customs Packing List", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.CusPackingList.Name);

				twRegistry.CustomsPackingListEnable.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

				Loader.LoadModules();
				_ = AssertModuleAdded("Customs Packing List", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.CusPackingList.Name);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				var twRegistry = ObjectFactory.Get<Enterprise.Integration.Customs.TW.ITWCustomsRegistry>();
				twRegistry.CustomsPackingListEnable.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				Loader.LoadModules();
				_ = AssertModuleAdded("Customs Packing List", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.CusPackingList.Name);
			}
		}

		public void TestTWBriefCustomsDeclarations()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				var twRegistry = ObjectFactory.Get<Enterprise.Integration.Customs.TW.ITWCustomsRegistry>();
				twRegistry.EnableBriefCustomsDeclaration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				Loader.LoadModules();
				_ = AssertModuleAdded("TW.BriefCustomsDeclarations should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.TW.BriefCustomsDeclarations.Name);

				twRegistry.EnableBriefCustomsDeclaration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

				Loader.LoadModules();
				_ = AssertModuleAdded("TW.BriefCustomsDeclarations should be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.TW.BriefCustomsDeclarations.Name);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				var twRegistry = ObjectFactory.Get<Enterprise.Integration.Customs.TW.ITWCustomsRegistry>();
				twRegistry.EnableBriefCustomsDeclaration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				Loader.LoadModules();
				_ = AssertModuleAdded("TW.BriefCustomsDeclarations should be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.TW.BriefCustomsDeclarations.Name);
			}
		}

		[SelfManagedTariffCountries(Core.Constants.CountryCodes.Congo)]
		public void TestTradeGroupsModuleIsRegistryDependent()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("Trade Groups module should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.TradeGroups.Name);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Congo))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("Trade Groups module should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.TradeGroups.Name);
			}
		}

		[SelfManagedTariffCountries(Core.Constants.CountryCodes.Congo)]
		public void TestPreferenceCodeModule()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Congo))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("Preference Code module should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.CusRefPreference.Name);
				_ = AssertModuleAdded("Preference Code module should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.CusRefPreference.Name);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("Preference Code module should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.CusRefPreference.Name);
				_ = AssertModuleAdded("Preference Code module should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.CusRefPreference.Name);
			}
		}

		[SelfManagedTariffCountries(Core.Constants.CountryCodes.Congo)]
		public void TestCusRefRateCodeModule()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Congo))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("CusRefRateCode module should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.CusRefRateCode.Name);
				_ = AssertModuleAdded("CusRefRateCode module should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.CusRefRateCode.Name);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("CusRefRateCode module should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.CusRefRateCode.Name);
				_ = AssertModuleAdded("CusRefRateCode module should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.CusRefRateCode.Name);
			}
		}

		[SelfManagedTariffCountries(Core.Constants.CountryCodes.Congo)]
		public void TestCusRefTariffVersionModule()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Congo))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("CusRefTariffVersion module should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.CusRefTariffVersion.Name);
				_ = AssertModuleAdded("CusRefTariffVersion module should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.CusRefTariffVersion.Name);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("CusRefTariffVersion module should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.CusRefTariffVersion.Name);
				_ = AssertModuleAdded("CusRefTariffVersion module should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.CusRefTariffVersion.Name);
			}
		}

		public void TestEntryHeaderModules()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Switzerland);
			Loader.LoadModules();
			_ = AssertModuleAdded($"EntryHeader for {GlbCompany.CurrentCompany.GC_RN_NKCountryCode}", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EntryHeader.Name);

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Japan);
			Loader.LoadModules();
			_ = AssertModuleAdded($"EntryHeader for {GlbCompany.CurrentCompany.GC_RN_NKCountryCode}", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EntryHeader.Name);

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Singapore);
			Loader.LoadModules();
			_ = AssertModuleAdded($"EntryHeader for {GlbCompany.CurrentCompany.GC_RN_NKCountryCode}", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EntryHeader.Name);

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.UnitedStates);
			Loader.LoadModules();
			_ = AssertModuleAdded($"EntryHeader for {GlbCompany.CurrentCompany.GC_RN_NKCountryCode}", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EntryHeader.Name);

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.UnitedKingdom);
			Loader.LoadModules();
			_ = AssertModuleAdded($"EntryHeader for {GlbCompany.CurrentCompany.GC_RN_NKCountryCode}", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EntryHeader.Name);

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.SouthAfrica);
			Loader.LoadModules();
			_ = AssertModuleAdded($"EntryHeader for {GlbCompany.CurrentCompany.GC_RN_NKCountryCode}", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EntryHeader.Name);

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);
			Loader.LoadModules();
			_ = AssertModuleAdded($"EntryHeader for {GlbCompany.CurrentCompany.GC_RN_NKCountryCode}", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EntryHeader.Name);

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.France);
			Loader.LoadModules();
			_ = AssertModuleAdded($"EntryHeader for {GlbCompany.CurrentCompany.GC_RN_NKCountryCode}", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EntryHeader.Name);

			var euCustomsMemberProvider = ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>();
			foreach (var euCountry in euCustomsMemberProvider.GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers()
					.Where(x => euCustomsMemberProvider.IsInEuropeanCustomsUnion(x)))
			{
				GlbCompany.CurrentCompany.SetCountry(euCountry);
				Loader.LoadModules();
				_ = AssertModuleAdded($"EntryHeader for {euCountry}", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EntryHeader.Name);
			}

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Italy);
			Loader.LoadModules();
			_ = AssertModuleAdded($"EntryHeader for {GlbCompany.CurrentCompany.GC_RN_NKCountryCode}", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EntryHeader.Name);

			var asycudaCustomsType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Asycuda;
			var zzDataGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping;
			var factory = new BusinessObjectFactory();
			var helper = new Enterprise.Customs.Universal.Testing.UniversalReferenceTestDataHelper(factory);
			_ = helper.CreateNewOrGetExistingCusCodeType(asycudaCustomsType, "Asycuda country");
			_ = helper.CreateNewOrGetExistingCusCodeList(zzDataGrouping, asycudaCustomsType, "NA", "Namibia", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();

			ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().ResetCachingForTest();
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Namibia);
			Loader.LoadModules();
			_ = AssertModuleAdded($"EntryHeader for {GlbCompany.CurrentCompany.GC_RN_NKCountryCode}", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EntryHeader.Name);
		}

		public void TestSGAccessModule()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Singapore))
			{
				var sgRegistry = ObjectFactory.Get<Enterprise.Integration.Customs.SG.ISGCustomsRegistry>();
				sgRegistry.ACCESSEnable.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				Loader.LoadModules();
				_ = AssertModuleAdded("SGAccess.Manifest should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsGlobal, ModuleIDs.Customs.ASYCUDA.SGAccess.Manifest.Name);
				_ = AssertModuleAdded("SGAccess.ManifestBill should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsGlobal, ModuleIDs.Customs.ASYCUDA.SGAccess.ManifestBill.Name);

				sgRegistry.ACCESSEnable.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

				Loader.LoadModules();
				_ = AssertModuleAdded("SGAccess.Manifest should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsGlobal, ModuleIDs.Customs.ASYCUDA.SGAccess.Manifest.Name);
				_ = AssertModuleAdded("SGAccess.ManifestBill should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsGlobal, ModuleIDs.Customs.ASYCUDA.SGAccess.ManifestBill.Name);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				var sgRegistry = ObjectFactory.Get<Enterprise.Integration.Customs.SG.ISGCustomsRegistry>();
				sgRegistry.ACCESSEnable.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				Loader.LoadModules();
				_ = AssertModuleAdded("SGAccess.Manifest should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsGlobal, ModuleIDs.Customs.ASYCUDA.SGAccess.Manifest.Name);
				_ = AssertModuleAdded("SGAccess.ManifestBill should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsGlobal, ModuleIDs.Customs.ASYCUDA.SGAccess.ManifestBill.Name);

				sgRegistry.ACCESSEnable.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

				Loader.LoadModules();
				_ = AssertModuleAdded("SGAccess.Manifest should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsGlobal, ModuleIDs.Customs.ASYCUDA.SGAccess.Manifest.Name);
				_ = AssertModuleAdded("SGAccess.ManifestBill should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsGlobal, ModuleIDs.Customs.ASYCUDA.SGAccess.ManifestBill.Name);
			}
		}

		public void TestGBSpecificModules()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("ZZ RefCusCodeList for GB - for PIMAs", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.Universal.ZZRefCusCodeList.Name);
				_ = AssertModuleAdded("CCSUK Module should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.Ccsuk, ModuleIDs.Customs.EU.GB.CcsukAirInventory.Name);

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CHIEF_SUNSET_EXP, Constants.CountryCodes.UnitedKingdom, ZDateTime.Now, value: true))
				{
					Loader.LoadModules();
					var customsGBSectionExists = Tree.Categories[ModuleTreeLoaderConstant.Category.Admin.Name].Sections.Values.Cast<ModuleSection>().Any(m => m.CustomerServiceMenuSectionCode == ModuleTreeCustomerServiceMenuSectionList.Codes.CustomsGb);
					Assert("Customs (GB) section should not be loaded", !customsGBSectionExists);
				}
			}
		}

		public void TestGBH7Module()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			{
				MockH7FeatureControlManagerAndAssertModuleVisibility(ModuleIDs.Customs.EU.EUH7.Name);
			}
		}

		public void TestGBH7BillModule()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			{
				MockH7FeatureControlManagerAndAssertModuleVisibility(ModuleIDs.Customs.EU.EUH7Bill.Name);
			}
		}

		public void TestEUH7Module()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Latvia))
			{
				MockH7FeatureControlManagerAndAssertModuleVisibility(ModuleIDs.Customs.EU.EUH7.Name);
			}
		}

		public void TestEUH7BillModule()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Latvia))
			{
				MockH7FeatureControlManagerAndAssertModuleVisibility(ModuleIDs.Customs.EU.EUH7Bill.Name);
			}
		}

		void MockH7FeatureControlManagerAndAssertModuleVisibility(string moduleID)
		{
			var h7FeatureCode = CargoWise.Definitions.LicenceFeatureCodeList.Codes.EcommerceH7Feature;
			var mockIFeatureControlManager = new Mock<IFeatureControlManager>();
			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				MockIFeatureControlManager(null);
				AssertModuleVisible("Module disabled by default if feature data is null", isVisible: false);

				MockIFeatureControlManager(GetMockFeatureData(h7FeatureCode));
				AssertModuleVisible("Module enabled when Feature Data contains code but no parameter", isVisible: true);

				var parameters = "{\"AuthorizedCountries\": [],  \"AuthorizedCompanies\": []}";
				MockIFeatureControlManager(GetMockFeatureData(h7FeatureCode, parameters));
				AssertModuleVisible("Module disabled if parameter is not empty but both AuthorizedCountries and AuthorizedCompanies are empty", isVisible: false);

				parameters = $"{{\"AuthorizedCountries\": [\"$$\", \"{GlbCompany.CurrentCompany.GC_RN_NKCountryCode}\"],  \"AuthorizedCompanies\": []}}";
				MockIFeatureControlManager(GetMockFeatureData(h7FeatureCode, parameters));
				AssertModuleVisible("Module enabled if parameter is not empty and AuthorizedCountries contains the current login country", isVisible: true);

				parameters = $"{{\"AuthorizedCountries\": [],  \"AuthorizedCompanies\": [{{\"CountryCode\": \"{GlbCompany.CurrentCompany.GC_RN_NKCountryCode}\",\"CompanyCodes\": [\"{GlbCompany.CurrentCompany.GC_Code}\"]}}]}}";
				MockIFeatureControlManager(GetMockFeatureData(h7FeatureCode, parameters));
				AssertModuleVisible("Module enabled if parameter is not empty AuthorizedCompanies contains the current login company", isVisible: true);

				parameters = $"{{\"AuthorizedCountries\": [],  \"AuthorizedCompanies\": [{{\"CountryCode\": \"{GlbCompany.CurrentCompany.GC_RN_NKCountryCode}\",\"CompanyCodes\": [\"$$$\"]}}]}}";
				MockIFeatureControlManager(GetMockFeatureData(h7FeatureCode, parameters));
				AssertModuleVisible("Module disabled if parameter is not empty and neither AuthorizedCountries nor AuthorizedCompanies contains the current login country and company", isVisible: false);
			}

			void AssertModuleVisible(string message, bool isVisible)
			{
				Loader.LoadModules();
				_ = AssertModuleAdded(message, isVisible, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, moduleID);
			}

			IFeatureData GetMockFeatureData(string featureControlCode, string parameters = default)
			{
				var featureControlRule = new FeatureControlRule
				{
					FCM_FeatureControlCode = featureControlCode,
					FCR_Parameters = parameters
				};

				return new FeatureData(featureControlRule);
			}

			void MockIFeatureControlManager(IFeatureData featureData)
			{
				mockIFeatureControlManager.Setup(x => x.GetFeatureDataAsync(h7FeatureCode, CancellationToken.None)).Returns(Task.FromResult(featureData));
			}
		}

		public void TestEuModules()
		{
			var euCountries = new[] { Constants.CountryCodes.UnitedKingdom, Constants.CountryCodes.France, Constants.CountryCodes.Martinique, Constants.CountryCodes.Croatia };

			foreach (var country in euCountries)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				{
					Loader.LoadModules();
					_ = AssertModuleAdded("EU Permits for " + country, expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.Permits.Name);
					_ = AssertModuleAdded("EU Authorisation for " + country, expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.CusAuthorisations.Name);
				}
			}

			var currentCompanyCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			Loader.LoadModules();
			_ = AssertModuleAdded("EU Authorisation for " + currentCompanyCountry, expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.CusAuthorisations.Name);
			_ = AssertModuleAdded("EU Permits for " + currentCompanyCountry, expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.Permits.Name);
		}

		public void TestEuEmcsModule()
		{
			var registry = ObjectFactory.Get<Integration.Customs.EUEMCS.IEmcsCustomsDataRegistry>();

			var euCountries = new[] { Constants.CountryCodes.Latvia, Constants.CountryCodes.UnitedKingdom };
			foreach (var country in euCountries)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				{
					CombineAssertions(() =>
					{
						AssertModuleVisible("Module disabled by default", isVisible: false);

						using (registry.EnableEmcsFunctions.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
						{
							AssertModuleVisible("Module enabled at company level", isVisible: true);
						}
					});
				}
			}

			void AssertModuleVisible(string message, bool isVisible)
			{
				Loader.LoadModules();
				_ = AssertModuleAdded(message, isVisible, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EU.EMCS.Name);
			}
		}

		public void TestEuUCC6TemporaryStorageModuleForCountriesUnderFrenchJurisdiction()
		{
			var registry_PNTSEnabled = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().PNTSEnabled;
			var registry_PNTSEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().PNTSEnabledDeveloperOnly;

			foreach (var country in Core.Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				{
					CombineAssertions(() =>
					{
						AssertModuleVisible("Module disabled by default", isVisible: false);

						using (registry_PNTSEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
						{
							AssertModuleVisible($"Module should be enabled at company level for {country}", isVisible: true);
						}

						using (registry_PNTSEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
						{
							AssertModuleVisible($"Module should be enabled at company level for {country}", true);
						}
					});
				}
			}

			void AssertModuleVisible(string message, bool isVisible)
			{
				Loader.LoadModules();
				_ = AssertModuleAdded(message, isVisible, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EU.UCC6TemporaryStorage.Name);
			}
		}

		public void TestEmcsModuleForCountriesUnderFrenchJurisdiction()
		{
			var registry = ObjectFactory.Get<Integration.Customs.EUEMCS.IEmcsCustomsDataRegistry>();

			foreach (var country in Core.Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Latvia))
				{
					CombineAssertions(() =>
					{
						AssertModuleVisible("Module disabled by default", isVisible: false);

						using (registry.EnableEmcsFunctions.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
						{
							AssertModuleVisible("Module enabled at company level", isVisible: true);
						}
					});
				}
			}

			void AssertModuleVisible(string message, bool isVisible)
			{
				Loader.LoadModules();
				_ = AssertModuleAdded(message, isVisible, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EU.EMCS.Name);
			}
		}

		public void TestEuExitControlModule()
		{
			var registry = ObjectFactory.Get<Integration.Customs.EUExitControl.IExitControlCustomsDataRegistry>();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Latvia))
			{
				CombineAssertions(() =>
				{
					AssertModuleVisible("Module disabled by default", isVisible: false);

					using (registry.EnableExitControlModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					{
						AssertModuleVisible("Module enabled at company level", isVisible: true);
					}
				});
			}

			void AssertModuleVisible(string message, bool isVisible)
			{
				Loader.LoadModules();
				_ = AssertModuleAdded(message, isVisible, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EU.ExitControl.Name);
			}
		}

		public void TestEuExitControlReportModule()
		{
			var registry = ObjectFactory.Get<Integration.Customs.EUExitControl.IExitControlCustomsDataRegistry>();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Latvia))
			{
				CombineAssertions(() =>
				{
					AssertModuleVisible("Module disabled by default", isVisible: false);

					using (registry.EnableExitControlModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					{
						AssertModuleVisible("Module enabled at company level", isVisible: true);
					}
				});
			}

			void AssertModuleVisible(string message, bool isVisible)
			{
				Loader.LoadModules();
				_ = AssertModuleAdded(message, isVisible, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EU.ExitControlReport.Name);
			}
		}

		public void TestIntrastatReportsModule()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Latvia))
			{
				var mock = new Mock<Integration.Customs.EU.IEUIntrastatCustomsRegistry>();
				using (ObjectFactory.Substitute(mock.Object))
				{
					_ = mock.Setup(m => m.IsIntrastatEnabled).Returns(true);
					Loader.LoadModules();
					_ = AssertModuleAdded("Intrastat module enabled",
						expectModuleToExist: true,
						ModuleTreeLoaderConstant.Category.Operations,
						ModuleTreeLoaderConstant.Section.CustomsMain,
						ModuleIDs.Customs.EU.IntrastatReports.Name);

					_ = mock.Setup(m => m.IsIntrastatEnabled).Returns(false);
					Loader.LoadModules();
					_ = AssertModuleAdded("Intrastat module disabled",
						expectModuleToExist: false,
						ModuleTreeLoaderConstant.Category.Operations,
						ModuleTreeLoaderConstant.Section.CustomsMain,
						ModuleIDs.Customs.EU.IntrastatReports.Name);
				}
			}
		}

		public void TestUSModules()
		{
			var usCountries = new[] { Constants.CountryCodes.UnitedStates, Constants.CountryCodes.PuertoRico };

			foreach (var country in usCountries)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				{
					Loader.LoadModules();
					_ = AssertModuleAdded($"US Import Classification for {country}", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.US.USImportClassification.Name);
					_ = AssertModuleAdded($"US Export Classification for {country}", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.US.USExportClassification.Name);
				}
			}
		}

		public void TestDETemporaryStorage()
		{
			CombineAssertions(() =>
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany))
				{
					Loader.LoadModules();
					_ = AssertModuleAdded("SumA should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.TemporaryStorage.Name);
					_ = AssertModuleAdded("SumA Register should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.EU.DE.SumARegister.Name);
					_ = AssertModuleAdded("SumA Register ReadOnly should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EU.DE.SumARegisterReadOnly.Name);
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Poland))
				{
					Loader.LoadModules();
					_ = AssertModuleAdded("SumA Register should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.EU.DE.SumARegister.Name);
					_ = AssertModuleAdded("SumA Register ReadOnly should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EU.DE.SumARegisterReadOnly.Name);
				}
			});
		}

		public void TestDEExportStatusRequest()
		{
			CombineAssertions(() =>
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany))
				{
					Loader.LoadModules();
					_ = AssertModuleAdded("ExportStatusRequest should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EU.DE.ExportStatusRequest.Name);
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Mexico))
				{
					Loader.LoadModules();
					_ = AssertModuleAdded("ExportStatusRequest should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EU.DE.ExportStatusRequest.Name);
				}
			});
		}

		public void TestDEMonthlyClosing()
		{
			var deRegistry = ObjectFactory.Get<Enterprise.Integration.Customs.DE.IDECustomsRegistry>();
			CombineAssertions(() =>
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany))
				{
					deRegistry.ShowMonthlyClosing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
					Loader.LoadModules();
					_ = AssertModuleAdded("MonthlyClosing should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EU.DE.MonthlyClosing.Name);
					deRegistry.ShowMonthlyClosing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
					Loader.LoadModules();
					_ = AssertModuleAdded("MonthlyClosing should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EU.DE.MonthlyClosing.Name);
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Mexico))
				{
					deRegistry.ShowMonthlyClosing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
					Loader.LoadModules();
					_ = AssertModuleAdded("MonthlyClosing should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EU.DE.MonthlyClosing.Name);
					deRegistry.ShowMonthlyClosing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
					Loader.LoadModules();
					_ = AssertModuleAdded("MonthlyClosing should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EU.DE.MonthlyClosing.Name);
				}
			});
		}

		public void TestDETaxChangeAssessment()
		{
			var deRegistry = ObjectFactory.Get<Integration.Customs.DE.IDECustomsRegistry>();
			CombineAssertions(() =>
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany))
				{
					Loader.LoadModules();
					_ = AssertModuleAdded("TaxChangeAssessment should be loaded for DE", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EU.DE.TaxChangeAssessment.Name);
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Mexico))
				{
					Loader.LoadModules();
					_ = AssertModuleAdded("TaxChangeAssessment should not be loaded for MX", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EU.DE.TaxChangeAssessment.Name);
				}
			});
		}

		public void TestAUNEXDOCNotificationModule()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("NexDocNotifications should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.AU.NexDocNotifications.Name);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("NexDocNotifications should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.AU.NexDocNotifications.Name);
			}
		}

		public void TestAUHouseSeaCargoModule()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("HouseSeaCargo should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.AU.HouseSeaCargo.Name);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("HouseSeaCargo should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.AU.HouseSeaCargo.Name);

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.SeaCargoHouse, Constants.CountryCodes.Australia, ZDateTime.Now, value: true))
				{
					Loader.LoadModules();
					_ = AssertModuleAdded("FUNC turned on, HouseSeaCargo should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.AU.HouseSeaCargo.Name);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.PilotSeaCargoHouse, Constants.CountryCodes.Australia, ZDateTime.Now, value: true))
				{
					Loader.LoadModules();
					_ = AssertModuleAdded("PFUNC turned on, HouseSeaCargo should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.AU.HouseSeaCargo.Name);
				}
			}
		}

		public void TestUniversalModules_EU()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Spain))
			{
				Loader.LoadModules();
				CombineAssertions(() =>
				{
					_ = AssertModuleAdded("ModuleIDs.Customs.Universal.RefCusTariff", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.Universal.RefCusTariff.Name);
					_ = AssertModuleAdded("ModuleIDs.Customs.Universal.ZZRefCusMap", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.Universal.ZZRefCusMap.Name);
					_ = AssertModuleAdded("ModuleIDs.Customs.Universal.ZZRefCusCodeList", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.Universal.ZZRefCusCodeList.Name);
					_ = AssertModuleAdded("ModuleIDs.Customs.Universal.ZZRefCarrier", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.Universal.ZZRefCarrier.Name);
					_ = AssertModuleAdded("ModuleIDs.Customs.Universal.RefHarbourRate", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.Universal.RefHarbourRate.Name);
				});
			}
		}

		public void TestUniversalModules_NonEU()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SouthAfrica))
			{
				Loader.LoadModules();
				CombineAssertions(() =>
				{
					_ = AssertModuleAdded("ModuleIDs.Customs.Universal.RefCusTariff", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.Universal.RefCusTariff.Name);
					_ = AssertModuleAdded("ModuleIDs.Customs.Universal.ZZRefCusMap", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.Universal.ZZRefCusMap.Name);
					_ = AssertModuleAdded("ModuleIDs.Customs.Universal.ZZRefCusCodeList", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.Universal.ZZRefCusCodeList.Name);
					_ = AssertModuleAdded("ModuleIDs.Customs.Universal.ZZRefCarrier", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.Universal.ZZRefCarrier.Name);
					_ = AssertModuleAdded("ModuleIDs.Customs.Universal.RefHarbourRate", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.Universal.RefHarbourRate.Name);
				});
			}
		}

		public void TestNctsModuleVisiblity_GB()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			{
				AssertNctsModuleVisibility(Constants.CountryCodes.UnitedKingdom, expectedVisibility: true);
			}
		}

		public void TestNctsModuleVisiblity_ES()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Spain))
			{
				AssertNctsModuleVisibility(Constants.CountryCodes.Spain, expectedVisibility: true);
			}
		}

		public void TestNctsModuleVisiblity_FR()
		{
			foreach (var country in Enterprise.Core.Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				{
					if (country == Constants.CountryCodes.France)
					{
						AssertNctsModuleVisibility(country, expectedVisibility: true);
					}
					else
					{
						AssertNctsModuleVisibility(country, expectedVisibility: false);
					}
				}
			}
		}

		public void TestNctsModuleVisiblity_AU()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				AssertNctsModuleVisibility(Constants.CountryCodes.Australia, expectedVisibility: false);
			}
		}

		public void TestUSLowValueShipmentModule()
		{
			var countryCodes = new string[] { Constants.CountryCodes.UnitedStates, Constants.CountryCodes.PuertoRico };
			foreach (var countryCode in countryCodes)
			{
				GlbCompany.CurrentCompany.SetCountry(countryCode);
				Loader.LoadModules();

				_ = AssertModuleAdded("USLowValueShipment is turned on", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.US.USLowValueEntries.Name);
				_ = AssertModuleAdded("Low Value Entries by Bill module is turned on", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.US.USLowValueEntriesBill.Name);
			}
		}

		public void TestUSSpecificModules()
		{
			var countryCodes = new string[] { Constants.CountryCodes.UnitedStates, Constants.CountryCodes.PuertoRico };

			foreach (var countryCode in countryCodes)
			{
				GlbCompany.CurrentCompany.SetCountry(countryCode);
				Loader.LoadModules();
				_ = AssertModuleAdded("USCustomsStatement", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.US.USCustomsStatement.Name);
				_ = AssertModuleAdded("AMSBrokerDownloadMessage", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.US.AMSBrokerDownloadMessage.Name);
				_ = AssertModuleAdded("BorderLineReleaseMessage", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.US.BorderLineReleaseMessage.Name);
				_ = AssertModuleAdded("USInBond", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.US.InBond.Name);
				_ = AssertModuleAdded("USInBondMoveHeader", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.US.InBondMoveHeader.Name);
				_ = AssertModuleAdded("QueryMessage", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.US.QueryMessage.Name);
				_ = AssertModuleAdded("CourtesyNoticesOfLiquidationMessage", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.US.CourtesyNoticesOfLiquidation.Name);
				_ = AssertModuleAdded("Reconciliation", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.US.Reconciliation.Name);
				_ = AssertModuleAdded("Protest", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.US.Protest.Name);
				_ = AssertModuleAdded("Drawback", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.US.Drawback.Name);
				_ = AssertModuleAdded("eManifest", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.US.eManifest.Name);
				_ = AssertModuleAdded("eManifest International", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsGlobal, ModuleIDs.Customs.US.eManifestIntl.Name);
				_ = AssertModuleAdded("AMS", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsGlobal, ModuleIDs.Customs.US.AMS.Name);
				_ = AssertModuleAdded("AMS Bill", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsGlobal, ModuleIDs.Customs.US.AMSBill.Name);
				_ = AssertModuleAdded("Global Manifest", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsGlobal, ModuleIDs.Customs.ASYCUDA.Manifest.Name);

				_ = AssertModuleAdded("USTariffBulkChange", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.US.USTariffBulkChange.Name);
				_ = AssertModuleAdded("USInBondNumber", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.US.InBondNumber.Name);

				_ = AssertModuleAdded("USCACCase", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsUS, ModuleIDs.Customs.US.USCACCase.Name);
				_ = AssertModuleAdded("AffirmationOfCompliance", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsUS, ModuleIDs.Customs.US.AffirmationOfCompliance.Name);

				_ = AssertModuleAdded("Carriers", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsUS, ModuleIDs.Customs.US.Carrier.Name);

				_ = AssertModuleAdded("Country", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsUS, ModuleIDs.Customs.US.Country.Name);
				_ = AssertModuleAdded("Data Version", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsUS, ModuleIDs.Customs.US.USCDataVersion.Name);

				_ = AssertModuleAdded("FIRMS", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsUS, ModuleIDs.Customs.US.FIRMS.Name);

				_ = AssertModuleAdded("Quota", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsUS, ModuleIDs.Customs.US.Quota.Name);

				_ = AssertModuleAdded("Tariff", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsUS, ModuleIDs.Customs.US.Tariff.Name);

				_ = AssertModuleAdded("Rules", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsUS, ModuleIDs.Customs.US.USCRule.Name);
				_ = AssertModuleAdded("Tariff Rules", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsUS, ModuleIDs.Customs.US.USCTariffRule.Name);
				_ = AssertModuleAdded("TeamSpecialist", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsUS, ModuleIDs.Customs.US.TeamSpecialist.Name);
				_ = AssertModuleAdded("Visa", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsUS, ModuleIDs.Customs.US.Visa.Name);
				_ = AssertModuleAdded("TariffRequiringVisa", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsUS, ModuleIDs.Customs.US.TariffRequiringVisa.Name);

				_ = AssertModuleAdded("Permits", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.Permits.Name);
			}

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);
			Loader.LoadModules();
			_ = AssertModuleAdded("USCustomsStatement", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.US.USCustomsStatement.Name);
			_ = AssertModuleAdded("AMSBrokerDownloadMessage", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.US.AMSBrokerDownloadMessage.Name);
			_ = AssertModuleAdded("BorderLineReleaseMessage", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.US.BorderLineReleaseMessage.Name);
			_ = AssertModuleAdded("USInBond", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.US.InBond.Name);
			_ = AssertModuleAdded("USInBondMoveHeader", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.US.InBondMoveHeader.Name);
			_ = AssertModuleAdded("QueryMessage", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.US.QueryMessage.Name);
			_ = AssertModuleAdded("Reconciliation", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.US.Reconciliation.Name);
			_ = AssertModuleAdded("Protest", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.US.Protest.Name);
			_ = AssertModuleAdded("Drawback", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.US.Drawback.Name);
			_ = AssertModuleAdded("eManifest", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.US.eManifest.Name);
			_ = AssertModuleAdded("Global Manifest", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsGlobal, ModuleIDs.Customs.ASYCUDA.Manifest.Name);

			_ = AssertModuleAdded("USInBondNumber", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.US.InBondNumber.Name);

			_ = AssertModuleAdded("USCACCase", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsUS, ModuleIDs.Customs.US.USCACCase.Name);
			_ = AssertModuleAdded("AffirmationOfCompliance", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsUS, ModuleIDs.Customs.US.AffirmationOfCompliance.Name);

			_ = AssertModuleAdded("Carriers", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsUS, ModuleIDs.Customs.US.Carrier.Name);

			_ = AssertModuleAdded("Country", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsUS, ModuleIDs.Customs.US.Country.Name);
			_ = AssertModuleAdded("Data Version", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsUS, ModuleIDs.Customs.US.USCDataVersion.Name);

			_ = AssertModuleAdded("FIRMS", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsUS, ModuleIDs.Customs.US.FIRMS.Name);

			_ = AssertModuleAdded("Quota", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsUS, ModuleIDs.Customs.US.Quota.Name);

			_ = AssertModuleAdded("Tariff", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsUS, ModuleIDs.Customs.US.Tariff.Name);

			_ = AssertModuleAdded("Rules", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsUS, ModuleIDs.Customs.US.USCRule.Name);
			_ = AssertModuleAdded("Tariff Rules", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsUS, ModuleIDs.Customs.US.USCTariffRule.Name);
			_ = AssertModuleAdded("TeamSpecialist", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsUS, ModuleIDs.Customs.US.TeamSpecialist.Name);
			_ = AssertModuleAdded("Visa", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsUS, ModuleIDs.Customs.US.Visa.Name);
			_ = AssertModuleAdded("TariffRequiringVisa", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsUS, ModuleIDs.Customs.US.TariffRequiringVisa.Name);
			_ = AssertModuleAdded("USCForeignAndRegionPort", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsUS, ModuleIDs.Customs.US.USCForeignAndRegionPort.Name);
			_ = AssertModuleAdded("USCCarrierAndFIRMS", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsUS, ModuleIDs.Customs.US.USCCarrierAndFIRMS.Name);
		}

		public void TestTWSpecificModules()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("Transhipment", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.TW.Transhipment.Name);
			}
		}

		public void TestTRETradeModules()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.SouthAfrica);
			Loader.LoadModules();
			_ = AssertModuleAdded("TR.ETrade should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.TR.ETrade.Name);

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Turkey);
			Loader.LoadModules();
			_ = AssertModuleAdded("TR.ETrade should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.TR.ETrade.Name);

			ObjectFactory.Get<Enterprise.Integration.Customs.TR.ITRCustomsDataRegistry>().ExposeETradeModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Loader.LoadModules();
			_ = AssertModuleAdded("TR.ETrade should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.TR.ETrade.Name);
		}

		public void TestTRSimplifiedProcedureTransitSystemModules()
		{
			CombineAssertions(() =>
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
				{
					ObjectFactory.Get<Integration.Customs.TR.ITRCustomsDataRegistry>().ExposeSimplifiedProcedureTransitSystemModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
					Loader.LoadModules();
					_ = AssertModuleAdded("SimplifiedProcedureTransitSystem not valid for UK", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.EuNcts, ModuleIDs.Customs.TR.SimplifiedProcedureTransitSystem.Name);
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
				{
					ObjectFactory.Get<Integration.Customs.TR.ITRCustomsDataRegistry>().ExposeSimplifiedProcedureTransitSystemModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
					Loader.LoadModules();
					_ = AssertModuleAdded("Turkey but registry not enabled", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.EuNcts, ModuleIDs.Customs.TR.SimplifiedProcedureTransitSystem.Name);

					ObjectFactory.Get<Integration.Customs.TR.ITRCustomsDataRegistry>().ExposeSimplifiedProcedureTransitSystemModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
					Loader.LoadModules();
					_ = AssertModuleAdded("Turkey and registry enabled", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.EuNcts, ModuleIDs.Customs.TR.SimplifiedProcedureTransitSystem.Name);
				}
			});
		}

		public void TestTRStatementsStampDuty()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.SouthAfrica);
			Loader.LoadModules();
			_ = AssertModuleAdded("TR.StatementsStampDuty should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.StampDutyMain, ModuleIDs.Customs.TR.StatementsStampDuty.Name);

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Turkey);
			Loader.LoadModules();
			_ = AssertModuleAdded("TR.StatementsStampDuty should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.StampDutyMain, ModuleIDs.Customs.TR.StatementsStampDuty.Name);
		}

		public void TestJPSpecificModules()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Canada);
			Loader.LoadModules();
			_ = AssertModuleAdded("AFR", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsGlobal, ModuleIDs.Customs.JP.AFR.Name);
			_ = AssertModuleAdded("AFR Bill", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsGlobal, ModuleIDs.Customs.JP.AFRBill.Name);
		}

		public void TestCASpecificModules()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Canada);
			Loader.LoadModules();
			_ = AssertModuleAdded("CAQueryMessages should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.CA.CAQueryMessages.Name);
			_ = AssertModuleAdded("CAReleaseNotifications should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.CA.CAReleaseNotifications.Name);
			_ = AssertModuleAdded("K84Reports should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.CA.K84Reports.Name);
			_ = AssertModuleAdded("CA Daily Notice Reconciliation should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.CA.CADailyNoticeReconciliation.Name);
			_ = AssertModuleAdded("CA ARL Statement of Account should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.CA.CAARLStatementOfAccount.Name);
			_ = AssertModuleAdded("CA CSA Revenue Summary Form should not be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.CA.CACSARevenueSummaryForm.Name);
			Loader.LoadModules();
			_ = AssertModuleAdded("CA CSA Revenue Summary Form should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.CA.CACSARevenueSummaryForm.Name);
			_ = AssertModuleAdded("Forwarded Manifests should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.CA.CAManifestForward.Name);
			_ = AssertModuleAdded("B2Adjustments should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.CA.B2Adjustments.Name);
			_ = AssertModuleAdded("ExportClassification should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.CA.CAExportClassification.Name);
			_ = AssertModuleAdded("HTSClassification should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.CA.HTSClassification.Name);
			_ = AssertModuleAdded("ExportTariff Module should be loaded if the country is CA", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsCA, ModuleIDs.Customs.CA.ExportTariff.Name);
			_ = AssertModuleAdded("SubLocation should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsCA, ModuleIDs.Customs.CA.SubLocation.Name);
			_ = AssertModuleAdded("e-Manifest (US) should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsGlobal, ModuleIDs.Customs.US.eManifestIntl.Name);
			_ = AssertModuleAdded("Transaction Number Setting should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsCA, ModuleIDs.Customs.CA.CATransactionNumberSetting.Name);
			_ = AssertModuleAdded("CA house bill in Global section", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsGlobal, ModuleIDs.Customs.CA.CAHouseBilleManifest.Name);
			_ = AssertModuleAdded("Global Manifest", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsGlobal, ModuleIDs.Customs.ASYCUDA.Manifest.Name);

			Loader.LoadModules();
			_ = AssertModuleAdded("Permits", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.Permits.Name);
			_ = AssertModuleAdded("CACusRuling", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.CA.CACusRuling.Name);

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);
			Loader.LoadModules();
			_ = AssertModuleAdded("CAQueryMessages should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.CA.CAQueryMessages.Name);
			_ = AssertModuleAdded("CAReleaseNotifications should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.CA.CAReleaseNotifications.Name);
			_ = AssertModuleAdded("K84Reports should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.CA.K84Reports.Name);
			_ = AssertModuleAdded("CA Daily Notice Reconciliation should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.CA.CADailyNoticeReconciliation.Name);
			_ = AssertModuleAdded("CA ARL Statement of Account should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.CA.CAARLStatementOfAccount.Name);
			_ = AssertModuleAdded("CA CSA Revenue Summary Form should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.CA.CACSARevenueSummaryForm.Name);
			_ = AssertModuleAdded("Forwarded Manifests should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.CA.CAManifestForward.Name);
			_ = AssertModuleAdded("B2Adjustment should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.CA.B2Adjustments.Name);
			_ = AssertModuleAdded("ExportClassification should be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.CA.CAExportClassification.Name);
			_ = AssertModuleAdded("HTSClassification should be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.CA.HTSClassification.Name);
			_ = AssertModuleAdded("ExportTariff Module should NOT be loaded if the country is not CA", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsCA, ModuleIDs.Customs.CA.ExportTariff.Name);
			_ = AssertModuleAdded("SubLocation should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsCA, ModuleIDs.Customs.CA.SubLocation.Name);
			_ = AssertModuleAdded("e-Manifest (US) should  not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsGlobal, ModuleIDs.Customs.US.eManifestIntl.Name);
			_ = AssertModuleAdded("Transaction Number Setting should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.CA.CATransactionNumberSetting.Name);
			_ = AssertModuleAdded("CA house bill in Global section", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsGlobal, ModuleIDs.Customs.CA.CAHouseBilleManifest.Name);
			_ = AssertModuleAdded("URN Number Setting should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsCA, ModuleIDs.Customs.CA.CATransactionNumberSetting.Name);
			_ = AssertModuleAdded("Global Manifest", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsGlobal, ModuleIDs.Customs.ASYCUDA.Manifest.Name);

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Mexico);
			Loader.LoadModules();
			_ = AssertModuleAdded("e-Manifest (US) should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsGlobal, ModuleIDs.Customs.US.eManifestIntl.Name);
		}

		public void TestCALVXJobsModule()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Mexico);
			Loader.LoadModules();
			_ = AssertModuleAdded("CALVXJobs should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.CA.CALVXJobs.Name);

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Canada);
			Loader.LoadModules();
			_ = AssertModuleAdded("CALVXJobs should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.CA.CALVXJobs.Name);
		}

		public void TestCHSpecificModules()
		{
			var registryDeclarationActivationEnabled = ObjectFactory.Get<Integration.Customs.CH.ICHCustomsRegistry>().DeclarationActivationEnabled;

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Switzerland);

			using (registryDeclarationActivationEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertModules(expectDeclarationActivationVisible: true);
			}

			using (registryDeclarationActivationEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				AssertModules(expectDeclarationActivationVisible: false);
			}

			void AssertModules(bool expectDeclarationActivationVisible)
			{ 
				Loader.LoadModules();
				_ = AssertModuleAdded("Permits", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.Permits.Name);
				_ = AssertModuleAdded("CustomsSummary", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.CH.CustomsSummary.Name);
				_ = AssertModuleAdded("DeclarationActivation", expectModuleToExist: expectDeclarationActivationVisible, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.CH.DeclarationActivation.Name);
			}
		}

		public void TestBRModules()
		{
			var mock = new Mock<Integration.Customs.BR.IBRCustomsDataRegistry>();
			using (ObjectFactory.Substitute(mock.Object))
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Brazil))
				{
					_ = mock.Setup(m => m.EnableImportLicense).Returns(false);
					_ = mock.Setup(m => m.EnableLPCO).Returns(false);
					_ = mock.Setup(m => m.EnableCatalogModule).Returns(true);
					Loader.LoadModules();
					_ = AssertModuleAdded("GoodsCatalog should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.GoodsCatalog.Name);

					_ = AssertModuleAdded("LPCODeclaration should NOT be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.BR.LPCODeclaration.Name);
					_ = AssertModuleAdded("LPCOEntryHeader should NOT be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.BR.LPCOEntryHeader.Name);
					_ = AssertModuleAdded("Licenses should NOT be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.BR.License.Name);
					_ = AssertModuleAdded("LicenseEntryHeader should NOT be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.BR.LicenseEntryHeader.Name);
					_ = AssertModuleAdded("LPCO should NOT be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.BR.LPCO.Name);

					_ = mock.Setup(m => m.EnableCatalogModule).Returns(false);
					Loader.LoadModules();
					_ = AssertModuleAdded("GoodsCatalog should NOT be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.GoodsCatalog.Name);

					_ = mock.Setup(m => m.EnableImportLicense).Returns(false);
					_ = mock.Setup(m => m.EnableLPCO).Returns(true);
					Loader.LoadModules();
					_ = AssertModuleAdded("LPCODeclaration should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.BR.LPCODeclaration.Name);
					_ = AssertModuleAdded("LPCOEntryHeader should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.BR.LPCOEntryHeader.Name);
					_ = AssertModuleAdded("License should NOT be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.BR.License.Name);
					_ = AssertModuleAdded("LicenseEntryHeader NOT should be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.BR.LicenseEntryHeader.Name);
					_ = AssertModuleAdded("LPCO should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.BR.LPCO.Name);

					_ = mock.Setup(m => m.EnableImportLicense).Returns(true);
					_ = mock.Setup(m => m.EnableLPCO).Returns(false);
					Loader.LoadModules();
					_ = AssertModuleAdded("LPCODeclaration should NOT be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.BR.LPCODeclaration.Name);
					_ = AssertModuleAdded("LPCOEntryHeader should NOT be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.BR.LPCOEntryHeader.Name);
					_ = AssertModuleAdded("License should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.BR.License.Name);
					_ = AssertModuleAdded("LicenseEntryHeader should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.BR.LicenseEntryHeader.Name);
					_ = AssertModuleAdded("LPCO should NOT be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.BR.LPCO.Name);

					_ = mock.Setup(m => m.EnableImportLicense).Returns(true);
					_ = mock.Setup(m => m.EnableLPCO).Returns(true);
					Loader.LoadModules();
					_ = AssertModuleAdded("LPCODeclaration should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.BR.LPCODeclaration.Name);
					_ = AssertModuleAdded("LPCOEntryHeader should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.BR.LPCOEntryHeader.Name);
					_ = AssertModuleAdded("License should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.BR.License.Name);
					_ = AssertModuleAdded("LicenseEntryHeader should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.BR.LicenseEntryHeader.Name);
					_ = AssertModuleAdded("LPCO should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.BR.LPCO.Name);
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SvalbardAndJanMayen))
				{
					Loader.LoadModules();
					_ = AssertModuleAdded("LPCODeclaration should NOT be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.BR.LPCODeclaration.Name);
					_ = AssertModuleAdded("LPCOEntryHeader should NOT be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.BR.LPCOEntryHeader.Name);
					_ = AssertModuleAdded("License should NOT be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.BR.License.Name);
					_ = AssertModuleAdded("LicenseEntryHeader should NOT be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.BR.LicenseEntryHeader.Name);
					_ = AssertModuleAdded("LPCO should NOT be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.BR.LPCO.Name);
					_ = AssertModuleAdded("GoodsCatalog should NOT be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.GoodsCatalog.Name);
				}

				mock.VerifyAll();
			}
		}

		public void TestBRForeignOperatorModules()
		{
			var mock = new Mock<Integration.Customs.BR.IBRCustomsDataRegistry>();
			using (ObjectFactory.Substitute(mock.Object))
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Brazil))
				{
					_ = mock.Setup(m => m.EnableForeignOperator).Returns(false);
					Loader.LoadModules();
					_ = AssertModuleAdded("ForeignOperator should NOT be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.BR.ForeignOperator.Name);

					_ = mock.Setup(m => m.EnableForeignOperator).Returns(true);
					Loader.LoadModules();
					_ = AssertModuleAdded("ForeignOperator should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.BR.ForeignOperator.Name);
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SvalbardAndJanMayen))
				{
					Loader.LoadModules();
					_ = AssertModuleAdded("ForeignOperator should NOT be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.BR.ForeignOperator.Name);
				}

				mock.VerifyAll();
			}
		}

		[TestDate(2018, 5, 23)]
		public void TestESTemporaryStorageModule()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Spain))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("Temporary Storage should be hidden", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.TemporaryStorage.Name);

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.TemporaryStorage, Constants.CountryCodes.Spain, ZDateTime.Now, value: true))
				{
					Loader.LoadModules();
					_ = AssertModuleAdded("Temporary Storage should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.TemporaryStorage.Name);
				}
			}
		}

		public void TestESUCC6TemporaryStorageModule()
		{
			var registryPNTSEnabled = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().PNTSEnabled;
			var registryPNTSEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().PNTSEnabledDeveloperOnly;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Spain))
			{
				CombineAssertions(() =>
				{
					using (registryPNTSEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
					using (registryPNTSEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
					{
						AssertModuleVisible("When PNTSEnabled = false and PNTSEnabledDeveloperOnly = false, module is not enabled", false);
					}

					using (registryPNTSEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					using (registryPNTSEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
					{
						AssertModuleVisible("When PNTSEnabled = true and PNTSEnabledDeveloperOnly = false, module is enabled", true);
					}

					using (registryPNTSEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
					using (registryPNTSEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					{
						AssertModuleVisible("When PNTSEnabled = false and PNTSEnabledDeveloperOnly = true, module is enabled", true);
					}

					using (registryPNTSEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					using (registryPNTSEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					{
						AssertModuleVisible("When PNTSEnabled = true and PNTSEnabledDeveloperOnly = true, module is enabled", true);
					}
				});
			}

			void AssertModuleVisible(string message, bool isVisible)
			{
				Loader.LoadModules();
				_ = AssertModuleAdded(message, isVisible, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EU.UCC6TemporaryStorage.Name);
			}
		}

		[TestDate(2018, 5, 23)]
		public void TestESTemporaryStorageRegisterModule()
		{
			var registryRegisterEnabled = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabled;
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Spain))
			{
				CombineAssertions(() =>
				{
					using (registryRegisterEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
					using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
					{
						AssertModuleVisible("When RegisterEnabled = false and RegisterEnabledDeveloperOnly = false, module is not enabled", false);
					}

					using (registryRegisterEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
					{
						AssertModuleVisible("When RegisterEnabled = true and RegisterEnabledDeveloperOnly = false, module is enabled", true);
					}

					using (registryRegisterEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
					using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					{
						AssertModuleVisible("When RegisterEnabled = false and RegisterEnabledDeveloperOnly = true, module is enabled", true);
					}

					using (registryRegisterEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					{
						AssertModuleVisible("When RegisterEnabled = true and RegisterEnabledDeveloperOnly = true, module is enabled", true);
					}
				});
			}

			void AssertModuleVisible(string message, bool isVisible)
			{
				Loader.LoadModules();
				_ = AssertModuleAdded(message, isVisible, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.EU.ES.TemporaryStorageRegister.Name);
			}
		}

		[TestDate(2018, 5, 23)]
		public void TestESTemporaryStoragePremisesModule()
		{
			var registryRegisterEnabled = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabled;
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Spain))
			{
				CombineAssertions(() =>
				{
					using (registryRegisterEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
					using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
					{
						AssertModuleVisible("When RegisterEnabled = false and RegisterEnabledDeveloperOnly = false, module is not enabled", false);
					}

					using (registryRegisterEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
					{
						AssertModuleVisible("When RegisterEnabled = true and RegisterEnabledDeveloperOnly = false, module is enabled", true);
					}

					using (registryRegisterEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
					using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					{
						AssertModuleVisible("When RegisterEnabled = false and RegisterEnabledDeveloperOnly = true, module is enabled", true);
					}

					using (registryRegisterEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					{
						AssertModuleVisible("When RegisterEnabled = true and RegisterEnabledDeveloperOnly = true, module is enabled", true);
					}
				});
			}

			void AssertModuleVisible(string message, bool isVisible)
			{
				Loader.LoadModules();
				_ = AssertModuleAdded(message, isVisible, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.EU.TempStoragePremises.Name);
			}
		}

		public void TestNZSpecificModules()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.NewZealand))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("NZCUSCAR", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.NZ.CUSCAR.Name);
				_ = AssertModuleAdded("NZECIWriteOffManifesting", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.NZ.ECIWriteOffManifesting.Name);
				_ = AssertModuleAdded("NZExpressECI", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.NZ.ExpressECI.Name);
				_ = AssertModuleAdded("NZSeaCargoICR should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.NZ.SeaCargoICR.Name);
			}
		}

		public void TestKRSpecificModules()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.KoreaSouth);
			Loader.LoadModules();
			_ = AssertModuleAdded("CustomsStatement", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.KR.CustomsStatement.Name);
			_ = AssertModuleAdded("MiscRequestMessages", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.KR.MiscRequestMessages.Name);
			_ = AssertModuleAdded("DocumentListMessages", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.KR.DocumentListMessages.Name);
			_ = AssertModuleAdded("CusReconDeclaration", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.KR.DocumentListMessages.Name);
		}

		public void TestFRCustomsStatementModuleForCountriesUnderFrenchJurisdiction()
		{
			Loader.LoadModules();
			_ = AssertModuleAdded("Customs Statement should be hidden.", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EU.FR.CustomsStatement.Name);
			foreach (var country in Core.Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				{
					Loader.LoadModules();
					_ = AssertModuleAdded("Customs Statement should be loaded.", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EU.FR.CustomsStatement.Name);
				}
			}
		}

		public void TestFRTempStorageRegisterModuleLoadedForCountriesUnderFrenchJurisdiction()
		{
			foreach (var country in Core.Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				{
					Loader.LoadModules();
					_ = AssertModuleAdded("French Temp. Storage Register should be loaded.", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.EU.TempStorageRegister.Name);
				}
			}
		}

		public void TestPLTemporaryStorageModule()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Poland))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("Temporary Storage should be visble", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.TemporaryStorage.Name);
			}
		}

		public void TestCDSCashPaymentsModule()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("Module enabled by default", true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EU.GB.CDSCashPayments.Name);
			}
		}

		public void TestCODocumentIDsModule()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Uruguay);
			Loader.LoadModules();
			_ = AssertModuleAdded("CO.DocumentIDs should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsCO, ModuleIDs.Customs.CO.DocumentIDs.Name);

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Colombia);
			Loader.LoadModules();
			_ = AssertModuleAdded("CO.DocumentIDs should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsCO, ModuleIDs.Customs.CO.DocumentIDs.Name);
		}

		public void TestIECustomsAndExciseReports()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Latvia);
			Loader.LoadModules();
			AssertModuleAdded("IE.CustomsAndExciseReports should not be loaded", false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EU.IE.CustomsAndExciseReports.Name);

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Ireland);
			Loader.LoadModules();
			AssertModuleAdded("IE.CustomsAndExciseReports should be loaded", true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EU.IE.CustomsAndExciseReports.Name);
		}

		public void TestIntrastatModules()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Germany);
			var mock = new Mock<Integration.Customs.EU.IEUIntrastatCustomsRegistry>();
			using (ObjectFactory.Substitute(mock.Object))
			{
				_ = mock.Setup(m => m.IsIntrastatEnabled).Returns(true);
				Loader.LoadModules();
				_ = AssertModuleAdded("Intrastat module enabled", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EU.IntrastatTransactions.Name);

				_ = mock.Setup(m => m.IsIntrastatEnabled).Returns(false);
				Loader.LoadModules();
				_ = AssertModuleAdded("Intrastat module disabled", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EU.IntrastatTransactions.Name);
			}
		}

		public void TestTemporaryStorageRegisterModuleForNorway()
		{
			var mock = new Mock<Integration.Customs.NO.INOTemporaryStorageRegistry>();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Norway))
			using (ObjectFactory.Substitute(mock.Object))
			{
				_ = mock.Setup(m => m.IsTemporaryStorageRegisterEnabled).Returns(true);
				Loader.LoadModules();
				CombineAssertions("When TemporaryStorageRegister Registry is enabled", () => AssertTemporaryStorageModules(true));

				_ = mock.Setup(m => m.IsTemporaryStorageRegisterEnabled).Returns(false);
				Loader.LoadModules();
				CombineAssertions("When TemporaryStorageRegister Registry is disabled", () => AssertTemporaryStorageModules(false));
			}

			void AssertTemporaryStorageModules(bool expectModuleToExist)
			{
				_ = AssertModuleAdded("Temporary Storage Register", expectModuleToExist: expectModuleToExist, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.NO.TemporaryStorageRegister.Name);
				_ = AssertModuleAdded("Temporary Storage Register ReadOnly", expectModuleToExist: expectModuleToExist, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.NO.TemporaryStorageRegisterReadOnly.Name);
			}
		}

		public void TestTemporaryStorageModulesForItaly()
		{
			var settingsMock = new Mock<Integration.Customs.Shared.ITemporaryStorageSettings>();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			using (ObjectFactory.Substitute(settingsMock.Object))
			{
				CombineAssertions("When registry item is enabled", () => AssertTemporaryStorageModules(registryEnabled: true));
				CombineAssertions("When registry item is disabled", () => AssertTemporaryStorageModules(registryEnabled: false));
			}

			void AssertTemporaryStorageModules(bool registryEnabled)
			{
				settingsMock.Setup(m => m.IsUsingUCC6).Returns(registryEnabled);

				Loader.LoadModules();
				_ = AssertModuleAdded("Temporary Storage Register Module Enabled?", expectModuleToExist: registryEnabled, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleIDs.Customs.EU.TempStorageRegister.Name);
				_ = AssertModuleAdded("Temporary Storage Module Enabled?", expectModuleToExist: registryEnabled, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.Customs.EU.UCC6TemporaryStorage.Name);
			}
		}

		#endregion

		#region OrderManager

		public void TestOrderManager()
		{
			AdvOrmFeatureHelper.RunTestWith(true, action: () =>
			{
				System.Environment.SetEnvironmentVariable("CTRLTOWER_Enabled", "tRuE");

				Loader.LoadModules();

				AssertNotNull(Tree.Categories[ModuleTreeLoaderConstant.Category.Operations.Name].Sections[ModuleTreeLoaderConstant.Section.OrderManager.Name]);
				var mainModules = FindModulesById(
					ModuleIDs.JobShipmentPreplanning,
					ModuleIDs.Orders,
					ModuleIDs.OrderLine,
					ModuleIDs.OrdersReport);
				var aDVORMModules = FindModulesById(
					ModuleIDs.SupplierBooking,
					ModuleIDs.ContainerLoadList,
					ModuleIDs.OrdersWebPortal,
					ModuleIDs.OrderLinesWebPortal,
					ModuleIDs.ContainerLoadPlan);
				var controlTowerModules = FindModulesById(ModuleIDs.ControlTower);
				CombineAssertions(() =>
				{
					foreach (var (id, mainForm) in mainModules)
					{
						AssertNotNull(id, mainForm);
					}
					foreach (var (id, mainForm) in aDVORMModules)
					{
						AssertNotNull(id, mainForm);
					}
					foreach (var (id, mainForm) in controlTowerModules)
					{
						AssertNotNull(id, mainForm);
					}
				});
			});

			AdvOrmFeatureHelper.RunTestWith(false, action: () =>
			{
				System.Environment.SetEnvironmentVariable("CTRLTOWER_Enabled", "FalSe");

				Loader.LoadModules();

				AssertNotNull(Tree.Categories[ModuleTreeLoaderConstant.Category.Operations.Name].Sections[ModuleTreeLoaderConstant.Section.OrderManager.Name]);
				var mainModules = FindModulesById(
					ModuleIDs.JobShipmentPreplanning,
					ModuleIDs.Orders,
					ModuleIDs.OrderLine,
					ModuleIDs.OrdersReport);
				var aDVORMModules = FindModulesById(
					ModuleIDs.SupplierBooking,
					ModuleIDs.ContainerLoadList,
					ModuleIDs.OrdersWebPortal,
					ModuleIDs.OrderLinesWebPortal,
					ModuleIDs.ContainerLoadPlan);
				var controlTowerModules = FindModulesById(ModuleIDs.ControlTower);
				CombineAssertions(() =>
				{
					foreach (var (id, mainForm) in mainModules)
					{
						AssertNotNull(id, mainForm);
					}
					foreach (var (id, mainForm) in aDVORMModules)
					{
						AssertNull(id, mainForm);
					}
					foreach (var (id, mainForm) in controlTowerModules)
					{
						AssertNull(id, mainForm);
					}
				});
			});
		}

		#endregion

		#region Accounting

		public void TestAssetManagementModuleEnablingByRegistry()
		{
			AssertAssetManagementModuleEnablingByRegistry(ModuleIDs.AssetManagementPortal);
		}

		public void TestAssetManagementReportsModuleEnablingByRegistry()
		{
			AssertAssetManagementModuleEnablingByRegistry(ModuleIDs.AssetManagementReports);
		}

		public void AssertAssetManagementModuleEnablingByRegistry(ModuleIdentifier moduleID)
		{
			var registry = AccountingMasterFilesRegistry.Instance.EnableAssetManagementFunctionality;

			using (registry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Loader.LoadModules();
				AssertNull(Tree.FindByID(moduleID.ToString()));
			}

			using (registry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Loader.LoadModules();
				AssertNotNull(Tree.FindByID(moduleID.ToString()));
			}
		}

		public void TestComPayRegisteredOrganisations()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);
			using (ZModule testModule = new Enterprise.Accounting.Module.ComPayRegisteredOrganisationsModule())
			{
				var moduleName = testModule.ID.ToString();
				Loader.LoadModules();

				_ = AssertModuleAdded("ComPayRegisteredOrganisationsModule should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.Account, moduleName);
			}

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);
			using (ZModule testModule = new Enterprise.Accounting.Module.ComPayRegisteredOrganisationsModule())
			{
				var moduleName = testModule.ID.ToString();
				Loader.LoadModules();

				_ = AssertModuleAdded("ComPayRegisteredOrganisationsModule should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.Account, moduleName);
			}

			var mock = new Mock<IAccounting>();
			_ = mock.Setup(m => m.IsENettOrganisation(It.IsAny<ZGuid>())).Returns(false);
			_ = mock.Setup(m => m.EnableBulkDisbursementJobsClosure).Returns(true);
			using (ObjectFactory.Substitute<IAccounting>(mock.Object))
			{
				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);
				using (ZModule testModule = new Enterprise.Accounting.Module.ComPayRegisteredOrganisationsModule())
				{
					var moduleName = testModule.ID.ToString();
					Loader.LoadModules();

					_ = AssertModuleAdded("ComPayRegisteredOrganisationsModule should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.Account, moduleName);
				}

				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);
				using (ZModule testModule = new Enterprise.Accounting.Module.ComPayRegisteredOrganisationsModule())
				{
					var moduleName = testModule.ID.ToString();
					Loader.LoadModules();

					_ = AssertModuleAdded("ComPayRegisteredOrganisationsModule should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.Account, moduleName);
				}
			}
			mock.VerifyAll();
		}

		public void TestAccCollectionBatch()
		{
			using ZModule testModule = new Enterprise.Accounting.Module.AccCollectionBatchModule();
			var moduleName = testModule.ID.ToString();
			Loader.LoadModules();
			_ = AssertModuleAdded("Accounting Collection Batch Module should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.Receivables, moduleName);
		}

		public void TestAccCollectionOrder()
		{
			using ZModule testModule = new Enterprise.Accounting.Module.AccCollectionOrderModule();
			var moduleName = testModule.ID.ToString();
			Loader.LoadModules();
			_ = AssertModuleAdded("Accounting Collection Order Module should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.Receivables, moduleName);
		}

		public void TestARCashAdvanceWhenRegistryAccessEnabled()
		{
			using ZModule testModule = new Enterprise.Accounting.Module.ARCashAdvanceModule();
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var moduleName = testModule.ID.ToString();
				Loader.LoadModules();
				_ = AssertModuleAdded("Accounting AR Cash Advance Module should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.Receivables, moduleName);
			}
		}

		public void TestARCashAdvanceWhenRegistryAccessDisabled()
		{
			using ZModule testModule = new Enterprise.Accounting.Module.ARCashAdvanceModule();
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var moduleName = testModule.ID.ToString();
				Loader.LoadModules();
				_ = AssertModuleAdded("Accounting AR Cash Advance Module should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.Receivables, moduleName);
			}
		}

		public void TestAPCashAdvance_WhenRegistryAccessEnabled_ModuleShouldBeLoaded()
		{
			using ZModule testModule = new Enterprise.Accounting.Module.APCashAdvanceModule();
			using (AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var moduleName = testModule.ID.ToString();
				Loader.LoadModules();
				_ = AssertModuleAdded("Accounting AP Cash Advance Module should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.Payables, moduleName);
			}
		}

		public void TestAPCashAdvance_WhenRegistryAccessDisabled_ModuleShouldNotBeLoaded()
		{
			using ZModule testModule = new Enterprise.Accounting.Module.APCashAdvanceModule();
			using (AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var moduleName = testModule.ID.ToString();
				Loader.LoadModules();
				_ = AssertModuleAdded("Accounting AP Cash Advance Module should not be loaded", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.Payables, moduleName);
			}
		}

		public void TestAccComplianceSequence()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Peru))
			{
				Loader.LoadModules();
				AssertNotNull(Tree.FindByID(ModuleIDs.AccComplianceSequence.ToString()));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Indonesia))
			{
				Loader.LoadModules();
				AssertNotNull(Tree.FindByID(ModuleIDs.AccComplianceSequence.ToString()));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Poland))
			{
				Loader.LoadModules();
				AssertNotNull(Tree.FindByID(ModuleIDs.AccComplianceSequence.ToString()));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Mexico))
			{
				Loader.LoadModules();
				AssertNotNull(Tree.FindByID(ModuleIDs.AccComplianceSequence.ToString()));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				Loader.LoadModules();
				AssertNull(Tree.FindByID(ModuleIDs.AccComplianceSequence.ToString()));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				Loader.LoadModules();
				AssertNull(Tree.FindByID(ModuleIDs.AccComplianceSequence.ToString()));
			}
		}

		public void TestAccComplianceSequenceForHasAccComplianceSequence()
		{
			var fields = typeof(Constants.CountryCodes).GetFields(BindingFlags.Public | BindingFlags.Static);
			foreach (var field in fields)
			{
				if (field == null || field.GetValue(null) is not string countryCode)
				{
					Fail($"Unable to cast field to string value; either null or invalid:{System.Environment.NewLine}{field}.");
					return;
				}

				var result = Db.Connection.ExecuteScalar(@"SELECT 1 FROM dbo.RefCountry WHERE RN_Code = @CountryCode", p =>
				{
					p.AddParameter("@CountryCode", System.Data.SqlDbType.VarChar, countryCode);
				});

				if (result != null)
				{
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
					{
						AssertEquals(GlbCompany.CurrentCompany.Country.SupportComplianceSubType && countryCode != Constants.CountryCodes.China, GlbCompany.CurrentCompany.Country.HasAccComplianceSequence);

						Loader.LoadModules();

						if (GlbCompany.CurrentCompany.Country.HasAccComplianceSequence)
						{
							AssertNotNull(Tree.FindByID(ModuleIDs.AccComplianceSequence.ToString()));
						}
						else
						{
							AssertNull(Tree.FindByID(ModuleIDs.AccComplianceSequence.ToString()));
						}
					}
				}
			}
		}

		public void TestAccComplianceSequence_DeleteCountry()
		{
			var factory = new BusinessObjectFactory();
			var country = factory.New<RefCountry>();
			country.Code = "AA";
			factory.Save();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AA"))
			{
				country.Delete();
				factory.Save();
				Loader.LoadModules();
				AssertNull(Tree.FindByID(ModuleIDs.AccComplianceSequence.ToString()));
			}
		}

		public void TestAccCompliancedocument()
		{
			Assert("No Compliance Document Configurations", !AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value);
			Loader.LoadModules();
			AssertNull(Tree.FindByID(ModuleIDs.ARComplianceDocument.ToString()));
			AssertNull(Tree.FindByID(ModuleIDs.APComplianceDocument.ToString()));

			AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			Assert("Has Compliance Report Configuration", AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value);
			Loader.LoadModules();
			AssertNotNull(Tree.FindByID(ModuleIDs.APComplianceDocument.ToString()));
			AssertNotNull(Tree.FindByID(ModuleIDs.ARComplianceDocument.ToString()));
		}

		public void TestAccPaymentBatch()
		{
			Loader.LoadModules();
			AssertNotNull(Tree.FindByID(ModuleIDs.PaymentBatch.ToString()));
		}

		void AssertModuleAccOrgTaxConfigurationTemplate(bool hasAnyAccTaxConfiguration,
			bool shouldContainModuleAccOrgTaxConfigurationTemplate,
			Action<Mock<ITaxFrameworkConfigurationHelper>, Mock<IAccountingMasterFilesDependencyFactory>> verifyAction)
		{
			var mockITaxFrameworkConfigurationHelper = new Mock<ITaxFrameworkConfigurationHelper>();
			var mockIAccountingMasterFilesDependencyFactory = new Mock<IAccountingMasterFilesDependencyFactory>();
			_ = mockITaxFrameworkConfigurationHelper.Setup(x => x.HasAnyAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>())).Returns(hasAnyAccTaxConfiguration);
			_ = mockIAccountingMasterFilesDependencyFactory.Setup(x => x.GetTaxFrameworkConfigurationHelper()).Returns(mockITaxFrameworkConfigurationHelper.Object);

			using (ObjectFactory.Substitute<IAccountingMasterFilesDependencyFactory>(mockIAccountingMasterFilesDependencyFactory.Object))
			{
				Loader.LoadModules();
				verifyAction.Invoke(mockITaxFrameworkConfigurationHelper, mockIAccountingMasterFilesDependencyFactory);
				AssertEquals(shouldContainModuleAccOrgTaxConfigurationTemplate, Tree.FindByID(ModuleIDs.AccOrgTaxConfigurationTemplate.ToString()) != null);
			}
		}

		public void TestAccOrgTaxConfigurationTemplate()
		{
			AssertModuleAccOrgTaxConfigurationTemplate(hasAnyAccTaxConfiguration: true, shouldContainModuleAccOrgTaxConfigurationTemplate: true, (mockITaxFrameworkConfigurationHelper, mockIAccountingMasterFilesDependencyFactory) =>
			{
				mockITaxFrameworkConfigurationHelper.Verify(x => x.HasAnyAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.Is<GlbCompany>(company => company.PK == GlbCompany.CurrentCompany.PK)), Times.Once);
				mockIAccountingMasterFilesDependencyFactory.Verify(x => x.GetTaxFrameworkConfigurationHelper(), Times.Once);
			});

			AssertModuleAccOrgTaxConfigurationTemplate(hasAnyAccTaxConfiguration: false, shouldContainModuleAccOrgTaxConfigurationTemplate: false, (mockITaxFrameworkConfigurationHelper, mockIAccountingMasterFilesDependencyFactory) =>
			{
				mockITaxFrameworkConfigurationHelper.Verify(x => x.HasAnyAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.Is<GlbCompany>(company => company.PK == GlbCompany.CurrentCompany.PK)), Times.Once);
				mockIAccountingMasterFilesDependencyFactory.Verify(x => x.GetTaxFrameworkConfigurationHelper(), Times.Once);
			});
		}

		public void TestAccComplianceReport()
		{
			AssertEquals("No Compliance Report Configurations", expected: false, AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value.Count > 0);
			Loader.LoadModules();
			AssertNull(Tree.FindByID(ModuleIDs.AccComplianceReport.ToString()));

			var reports = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var report = reports.AddNew();
			report.Country = "AU";
			report.ReportCode = "TST";
			report.TaxRegistrationType = "ABN";
			report.ReportBaseTablePrefix = "AH";
			report.ReportPeriodicity = CusPermitHeaderApplicationCodeList.Codes.Permit;
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, reports);

			Assert("Has Compliance Report Configuration", AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value.Count > 0);
			Loader.LoadModules();
			AssertNotNull(Tree.FindByID(ModuleIDs.AccComplianceReport.ToString()));
		}

		public void TestChequeModuleItems()
		{
			Loader.LoadModules();

			var modules = FindModulesById(
				ModuleIDs.Cheque,
				ModuleIDs.ChequeTransaction);

			CombineAssertions(() =>
			{
				foreach (var (id, mainForm) in modules)
				{
					AssertNull(id, mainForm);
				}
			});

			AccountingConfigurationRegistry.Instance.EnableChequeManagementFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Loader.LoadModules();

			modules = FindModulesById(
				ModuleIDs.Cheque,
				ModuleIDs.ChequeTransaction);

			CombineAssertions(() =>
			{
				foreach (var (id, mainForm) in modules)
				{
					AssertNotNull(id, mainForm);
				}
			});
		}

		public void TestCN2004DataInterface()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);
			Assert("Current company's country is China", GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.China);
			GlbStaff.CurrentUser.GS_LoginName = "CWSupport";
			Assert("Sanity check - should be support user", GlbStaff.CurrentUser.IsSupportUser);
			AccountingMasterFilesRegistry.Instance.ShowChinaGBTDataInterfaceMenus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert("ShowChinaGBTDataInterfaceMenus is true", AccountingMasterFilesRegistry.Instance.ShowChinaGBTDataInterfaceMenus.Value);
			Loader.LoadModules();
			AssertNotNull(Tree.FindByID(ModuleIDs.CN2004DataInterface.ToString()));

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.UnitedStates);
			Assert("Current company's country is not China", GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Constants.CountryCodes.China);
			Loader.LoadModules();
			AssertNull(Tree.FindByID(ModuleIDs.CN2004DataInterface.ToString()));
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);

			GlbStaff.CurrentUser.GS_LoginName = "test_notsupport";
			Assert("Sanity check - should not be support user", !GlbStaff.CurrentUser.IsSupportUser);
			Loader.LoadModules();
			AssertNull(Tree.FindByID(ModuleIDs.CN2004DataInterface.ToString()));
			GlbStaff.CurrentUser.GS_LoginName = "CWSupport";

			AccountingMasterFilesRegistry.Instance.ShowChinaGBTDataInterfaceMenus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Assert("ShowChinaGBTDataInterfaceMenus is false", !AccountingMasterFilesRegistry.Instance.ShowChinaGBTDataInterfaceMenus.Value);
			Loader.LoadModules();
			AssertNull(Tree.FindByID(ModuleIDs.CN2004DataInterface.ToString()));
			AccountingMasterFilesRegistry.Instance.ShowChinaGBTDataInterfaceMenus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Loader.LoadModules();
			AssertNotNull(Tree.FindByID(ModuleIDs.CN2004DataInterface.ToString()));
		}

		public void TestCNDataInterface()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);
			Assert("Current company's country is China", GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.China);
			Env.Security.ChinaDataInterface.IsAllowed = true;
			Assert("Env.Security.ChinaDataInterface.IsAllowed is true", Env.Security.ChinaDataInterface.IsAllowed);
			AccountingMasterFilesRegistry.Instance.ShowChinaGBTDataInterfaceMenus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert("ShowChinaGBTDataInterfaceMenus is true", AccountingMasterFilesRegistry.Instance.ShowChinaGBTDataInterfaceMenus.Value);
			Loader.LoadModules();
			AssertNotNull(Tree.FindByID(ModuleIDs.CNDataInterface.ToString()));

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.UnitedStates);
			Assert("Current company's country is not China", GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Constants.CountryCodes.China);
			Loader.LoadModules();
			AssertNull(Tree.FindByID(ModuleIDs.CNDataInterface.ToString()));
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);

			Env.Security.ChinaDataInterface.IsAllowed = false;
			Assert("Env.Security.ChinaDataInterface.IsAllowed is false", !Env.Security.ChinaDataInterface.IsAllowed);
			Loader.LoadModules();
			AssertNull(Tree.FindByID(ModuleIDs.CNDataInterface.ToString()));
			Env.Security.ChinaDataInterface.IsAllowed = true;

			AccountingMasterFilesRegistry.Instance.ShowChinaGBTDataInterfaceMenus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Assert("ShowChinaGBTDataInterfaceMenus is false", !AccountingMasterFilesRegistry.Instance.ShowChinaGBTDataInterfaceMenus.Value);
			Loader.LoadModules();
			AssertNull(Tree.FindByID(ModuleIDs.CNDataInterface.ToString()));
			AccountingMasterFilesRegistry.Instance.ShowChinaGBTDataInterfaceMenus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Loader.LoadModules();
			AssertNotNull(Tree.FindByID(ModuleIDs.CNDataInterface.ToString()));
		}

		public void TestAccPlaceOfSupplyChargeCodeGroup()
		{
			Assert("No POS Type ticked in the current company configurations",
				!AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.Value.OfType<CodeDescriptionBool>().Any(x => x.Bool));
			Assert("POS functionaality is not enabled", !PlaceOfSupplyHelper.IsPlaceOfSupplyEnabled(GlbCompany.CurrentCompany));
			Loader.LoadModules();
			AssertNull(Tree.FindByID(ModuleIDs.AccPlaceOfSupplyChargeCodeGroup.ToString()));

			var newValue = AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.Value;
			newValue[0].Bool = true;
			using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, newValue))
			{
				Assert("POS functionaality is enabled", PlaceOfSupplyHelper.IsPlaceOfSupplyEnabled(GlbCompany.CurrentCompany));

				Loader.LoadModules();
				AssertNotNull(Tree.FindByID(ModuleIDs.AccPlaceOfSupplyChargeCodeGroup.ToString()));
			}
		}

		public void TestAccountingVoucherModuleIsAddedForChinaCompany()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);
			using ZModule testModule = new Enterprise.Accounting.Module.AccountingVoucherModule();
			var moduleName = testModule.ID.ToString();
			Loader.LoadModules();

			_ = AssertModuleAdded("Accounting Voucher Module should be loaded if the country is China.", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.GeneralLedger, moduleName);
		}

		public void TestAccountingVoucherModuleIsAddedForTaiwanCompany()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Taiwan);
			using ZModule testModule = new Enterprise.Accounting.Module.AccountingVoucherModule();
			var moduleName = testModule.ID.ToString();
			Loader.LoadModules();

			_ = AssertModuleAdded("Accounting Voucher Module should be loaded if the country is Taiwan.", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.GeneralLedger, moduleName);
		}

		public void TestAccountingVoucherModuleIsNotAddedForAusralia()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);
			using ZModule testModule = new Enterprise.Accounting.Module.AccountingVoucherModule();
			var moduleName = testModule.ID.ToString();
			Loader.LoadModules();

			_ = AssertModuleAdded("Accounting Voucher Module should not be loaded if the country is Australia.", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.GeneralLedger, moduleName);
		}

		public void TestChinaJournalListingModuleIsAddedForChinaCompany()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);
			using ZModule testModule = new Enterprise.Accounting.Module.ChinaJournalListingModule();
			var moduleName = testModule.ID.ToString();
			Loader.LoadModules();

			_ = AssertModuleAdded("China Journal Listing Module should be loaded if the country is China.", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.GeneralLedger, moduleName);
		}

		public void TestChinaJournalListingModuleIsNotAddedForAusralia()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);
			using ZModule testModule = new Enterprise.Accounting.Module.ChinaJournalListingModule();
			var moduleName = testModule.ID.ToString();
			Loader.LoadModules();

			_ = AssertModuleAdded("China Journal Listing Module should not be loaded if the country is Australia.", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.GeneralLedger, moduleName);
		}

		public void TestAccGeneralLedgerDataModule()
		{
			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using ZModule testModule = new Enterprise.Accounting.Module.AccGeneralLedgerDataModule();
			var moduleName = testModule.ID.ToString();
			Loader.LoadModules();
			AssertModuleAdded(string.Format("{0} should not be loaded if GenerateJournalEntriesForPostedAccountingTransactions is false.", moduleName), expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.GeneralLedger, moduleName);

			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var mockIFeatureData = new Mock<IFeatureData>();
			var generalLedgerFeatureControlData = new GeneralLedgerDataFeatureControlModel() { EnableAccountingJournalsModule = false };
			var mockIFeatureControlManager = new Mock<IFeatureControlManager>();
			mockIFeatureControlManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingGeneralLedgerDataFeature, CancellationToken.None)).Returns(Task.FromResult(mockIFeatureData.Object));
			mockIFeatureData.Setup(x => x.TryDeserializeParameterAsJson(out generalLedgerFeatureControlData)).Returns(true);
			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				Loader.LoadModules();
				AssertModuleAdded(string.Format("{0} should not be loaded if EnableAccountingJournalsModule is false.", moduleName), expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.GeneralLedger, moduleName);
			}

			generalLedgerFeatureControlData.EnableAccountingJournalsModule = true;
			mockIFeatureData.Setup(x => x.TryDeserializeParameterAsJson(out generalLedgerFeatureControlData)).Returns(true);
			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				Loader.LoadModules();
				AssertModuleAdded(string.Format("{0} should be loaded if EnableAccountingJournalsModule is true.", moduleName), expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.GeneralLedger, moduleName);
			}
		}

		#region TestXmlJournalExportAndImportModule

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "WTG3101:Do not nest regions.", Justification = "<Pending>")]
		public void TestXmlJournalExportAndImportModule()
		{
			using (ZModule testModule = new Enterprise.Accounting.Module.XmlJournalExportModule())
			{
				AssertXmlJournalModule(testModule.ID.ToString());
			}
			using (ZModule testModule = new Enterprise.Accounting.Module.XmlJournalImportModule())
			{
				AssertXmlJournalModule(testModule.ID.ToString());
			}
		}

		void AssertXmlJournalModule(string moduleName)
		{
			SystemDataRegistry.Instance.ActivateSystemMergeDataInterface.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Loader.LoadModules();
			_ = AssertModuleAdded(string.Format("{0} should not be loaded if ActivateSystemMergeDataInterface is false.", moduleName), expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.GeneralLedger, moduleName);

			SystemDataRegistry.Instance.ActivateSystemMergeDataInterface.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Loader.LoadModules();
			_ = AssertModuleAdded(string.Format("{0} should be loaded if ActivateSystemMergeDataInterface is true.", moduleName), expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.GeneralLedger, moduleName);
		}

		#endregion

		public void TestAccTaxOverrideGroupModule()
		{
			var moduleName = ModuleIDs.AccTaxOverrideGroup.Name;

			var currentCompanyInLocalFactory = new BusinessObjectFactory().Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			currentCompanyInLocalFactory.GC_IsGSTRegistered = false;
			currentCompanyInLocalFactory.Factory.Save();

			using (currentCompanyInLocalFactory.Branches[0].SetAsTemporaryContext())
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("Accounting TaxOverrideGroup Module should be loaded if the country is not GST registered.", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.Account, moduleName);
			}

			currentCompanyInLocalFactory.GC_IsGSTRegistered = true;
			currentCompanyInLocalFactory.Factory.Save();

			using (currentCompanyInLocalFactory.Branches[0].SetAsTemporaryContext())
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
				Loader.LoadModules();
				_ = AssertModuleAdded("Accounting TaxOverrideGroup Module should not be loaded if the country is GST registered.", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.Account, moduleName);
			}

			currentCompanyInLocalFactory.SetCountry(Constants.CountryCodes.Malaysia);
			using (currentCompanyInLocalFactory.Branches[0].SetAsTemporaryContext())
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
				Loader.LoadModules();
				_ = AssertModuleAdded("Accounting TaxOverrideGroup Module should be loaded if the country is GST registered.", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.Account, moduleName);
			}

			using (currentCompanyInLocalFactory.Branches[0].SetAsTemporaryContext())
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
				Loader.LoadModules();
				_ = AssertModuleAdded("Accounting TaxOverrideGroup Module should not be loaded if the country is GST registered.", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.Account, moduleName);
			}

			var factory = new BusinessObjectFactory();
			var org = factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			var customCode = org.CustomsCodes.AddNew();
			customCode.OK_CodeType = "GST";
			customCode.OK_CustomsRegNo = "1111111";
			factory.Save();

			using (currentCompanyInLocalFactory.Branches[0].SetAsTemporaryContext())
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("Accounting TaxOverrideGroup Module should be loaded if the country is GST registered.", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.Account, moduleName);
			}
		}

		public void TestAccInvMsgModule()
		{
			var moduleName = ModuleIDs.AccInvMsg.Name;

			var currentCompanyInLocalFactory = new BusinessObjectFactory().Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			currentCompanyInLocalFactory.GC_IsGSTRegistered = false;
			currentCompanyInLocalFactory.Factory.Save();

			using (currentCompanyInLocalFactory.Branches[0].SetAsTemporaryContext())
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("Invoice Tax Messages Module should not be loaded if the country is not GST registered.", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.Account, moduleName);
			}

			currentCompanyInLocalFactory.GC_IsGSTRegistered = true;
			currentCompanyInLocalFactory.Factory.Save();

			using (currentCompanyInLocalFactory.Branches[0].SetAsTemporaryContext())
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
				Loader.LoadModules();
				_ = AssertModuleAdded("Invoice Tax Messages Module should be loaded if the country is GST registered.", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.Account, moduleName);
			}

			currentCompanyInLocalFactory.SetCountry(Constants.CountryCodes.Malaysia);
			using (currentCompanyInLocalFactory.Branches[0].SetAsTemporaryContext())
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
				Loader.LoadModules();
				_ = AssertModuleAdded("Invoice Tax Messages Module should be loaded if the country is GST registered.", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.Account, moduleName);
			}

			using (currentCompanyInLocalFactory.Branches[0].SetAsTemporaryContext())
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
				Loader.LoadModules();
				_ = AssertModuleAdded("Invoice Tax Messages Module should not be loaded if the country is GST registered.", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.Account, moduleName);
			}

			var factory = new BusinessObjectFactory();
			var org = factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			var customCode = org.CustomsCodes.AddNew();
			customCode.OK_CodeType = "GST";
			customCode.OK_CustomsRegNo = "1111111";
			factory.Save();

			using (currentCompanyInLocalFactory.Branches[0].SetAsTemporaryContext())
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("Invoice Tax Messages Module should be loaded if the country is GST registered.", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.Account, moduleName);
			}
		}

		public void TestAccTaxRateModule()
		{
			var moduleName = ModuleIDs.AccTaxRate.Name;

			var currentCompanyInLocalFactory = new BusinessObjectFactory().Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			currentCompanyInLocalFactory.GC_IsGSTRegistered = false;
			currentCompanyInLocalFactory.Factory.Save();

			using (currentCompanyInLocalFactory.Branches[0].SetAsTemporaryContext())
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("Tax Rates Module should not be loaded if the country is not GST registered.", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.Account, moduleName);
			}

			currentCompanyInLocalFactory.GC_IsGSTRegistered = true;
			currentCompanyInLocalFactory.Factory.Save();

			using (currentCompanyInLocalFactory.Branches[0].SetAsTemporaryContext())
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
				Loader.LoadModules();
				_ = AssertModuleAdded("Tax Rates Module should be loaded if the country is GST registered.", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.Account, moduleName);
			}

			currentCompanyInLocalFactory.SetCountry(Constants.CountryCodes.Malaysia);
			using (currentCompanyInLocalFactory.Branches[0].SetAsTemporaryContext())
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
				Loader.LoadModules();
				_ = AssertModuleAdded("Tax Rates Module should be loaded if the country is GST registered.", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.Account, moduleName);
			}

			using (currentCompanyInLocalFactory.Branches[0].SetAsTemporaryContext())
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
				Loader.LoadModules();
				_ = AssertModuleAdded("Tax Rates Module should not be loaded if the country is GST registered.", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.Account, moduleName);
			}

			var factory = new BusinessObjectFactory();
			var org = factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			var customCode = org.CustomsCodes.AddNew();
			customCode.OK_CodeType = "GST";
			customCode.OK_CustomsRegNo = "1111111";
			factory.Save();

			using (currentCompanyInLocalFactory.Branches[0].SetAsTemporaryContext())
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("Tax Rates Module should be loaded if the country is GST registered.", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.Account, moduleName);
			}
		}

		public void TestAPInvoiceApprovalModule()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Loader.LoadModules();
			var moduleId = ModuleIDs.APInvoiceApproval.ToString();
			_ = AssertModuleAdded(string.Format("{0} should not be loaded if EnableAPInvoiceApproval is false.", moduleId), expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.Payables, moduleId);
			moduleId = ModuleIDs.UnapprovedTransaction.ToString();
			_ = AssertModuleAdded(string.Format("{0} should be loaded if EnableAPInvoiceApproval is false.", moduleId), expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.Payables, moduleId);
			moduleId = ModuleIDs.UnapprovedIntercompanyTransaction.ToString();
			_ = AssertModuleAdded(string.Format("{0} should not be loaded if EnableAPInvoiceApproval is false.", moduleId), expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.Payables, moduleId);

			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Loader.LoadModules();
			moduleId = ModuleIDs.APInvoiceApproval.ToString();
			_ = AssertModuleAdded(string.Format("{0} should be loaded if EnableAPInvoiceApproval is false.", moduleId), expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.Payables, moduleId);
			moduleId = ModuleIDs.UnapprovedTransaction.ToString();
			_ = AssertModuleAdded(string.Format("{0} should not be loaded if EnableAPInvoiceApproval is false.", moduleId), expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.Payables, moduleId);
			moduleId = ModuleIDs.UnapprovedIntercompanyTransaction.ToString();
			_ = AssertModuleAdded(string.Format("{0} should be loaded if EnableAPInvoiceApproval is false.", moduleId), expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.Payables, moduleId);
		}

		public void TestTransactionsPendingAllocationModule()
		{
			AccountingMasterFilesRegistry.Instance.EnableTransactionPendingAllocationApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Loader.LoadModules();
			var moduleId = ModuleIDs.TransactionsPendingAllocationApproval.ToString();
			_ = AssertModuleAdded(string.Format("{0} should not be loaded if EnableTransactionPendingAllocationApproval is false.", moduleId), expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.Payables, moduleId);

			AccountingMasterFilesRegistry.Instance.EnableTransactionPendingAllocationApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Loader.LoadModules();
			moduleId = ModuleIDs.TransactionsPendingAllocationApproval.ToString();
			_ = AssertModuleAdded(string.Format("{0} should be loaded if EnableTransactionPendingAllocationApproval is true.", moduleId), expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.Payables, moduleId);
		}

		public void TestPayableOrderModule()
		{
			Loader.LoadModules();
			var moduleId = ModuleIDs.AccPayableOrder.ToString();
			_ = AssertModuleAdded(string.Format("{0} should be loaded", moduleId), expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.Payables, moduleId);
		}

		public void TestGLJournalApprovalModule()
		{
			Loader.LoadModules();
			var moduleId = ModuleIDs.GLJournalApproval.ToString();
			_ = AssertModuleAdded(string.Format("{0} should be loaded", moduleId), expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.GeneralLedger, moduleId);
		}

		public void TestTransactionsExportModule()
		{
			Loader.LoadModules();
			var moduleId = ModuleIDs.TransactionsExport.ToString();
			_ = AssertModuleAdded(string.Format("{0} should not be loaded if HasInterfaceConnector is false.", moduleId), expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.GeneralLedger, moduleId);

			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			_ = eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			Loader.LoadModules();
			moduleId = ModuleIDs.TransactionsExport.ToString();
			_ = AssertModuleAdded(string.Format("{0} should be loaded if HasInterfaceConnector is true.", moduleId), expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.GeneralLedger, moduleId);
			tomorrow = new InterfaceConnectorTemporarilyEnabledUntil { EnabledUntil = ZDateTime.Empty };
			_ = eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
		}

		public void TestImportXMLTransactions()
		{
			Loader.LoadModules();
			var moduleId = ModuleIDs.XmlTransactionsImport.ToString();
			_ = AssertModuleAdded(string.Format("{0} should not be loaded if HasInterfaceConnector is false.", moduleId), expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.GeneralLedger, moduleId);

			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			_ = eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			Loader.LoadModules();
			moduleId = ModuleIDs.XmlTransactionsImport.ToString();
			_ = AssertModuleAdded(string.Format("{0} should be loaded if HasInterfaceConnector is true.", moduleId), expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.GeneralLedger, moduleId);
			tomorrow = new InterfaceConnectorTemporarilyEnabledUntil { EnabledUntil = ZDateTime.Empty };
			_ = eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
		}

		public void TestPayablesInvoiceProcessingPortalModule()
		{
			var invoiceProcessingPortalModuleId = ModuleIDs.APInvoiceProcessingPortal.ToString();
			using (AccountingConfigurationRegistry.Instance.EnablePayablesInvoiceProcessingPortal.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded($"{invoiceProcessingPortalModuleId} should be loaded when EnablePayablesInvoiceProcessingPortal = true.", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.Payables, invoiceProcessingPortalModuleId);
			}

			using (AccountingConfigurationRegistry.Instance.EnablePayablesInvoiceProcessingPortal.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded($"{invoiceProcessingPortalModuleId} should not be loaded when EnablePayablesInvoiceProcessingPortal = false.", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.Payables, invoiceProcessingPortalModuleId);
			}
		}

		public void TestReportingBookSection()
		{
			Loader.LoadModules();
			AssertNull("ReportingBooks section", Tree.Categories[ModuleTreeLoaderConstant.Category.Admin.Name].Sections[ModuleTreeLoaderConstant.Section.ReportingBooks.Name]);
			AssertNull("ReportingBooks section", Tree.Categories[ModuleTreeLoaderConstant.Category.Manage.Name].Sections[ModuleTreeLoaderConstant.Section.GLReportingBooks.Name]);

			using (AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Loader.LoadModules();
				AssertNotNull("ReportingBooks section", Tree.Categories[ModuleTreeLoaderConstant.Category.Admin.Name].Sections[ModuleTreeLoaderConstant.Section.ReportingBooks.Name]);
				var modules = Tree.Categories[ModuleTreeLoaderConstant.Category.Admin.Name].Sections[ModuleTreeLoaderConstant.Section.ReportingBooks.Name].Modules;
				AssertEquals("Number of ReportingBookSection", 3, modules.Count);
				var chartModuleID = ModuleIDs.AlternateChartofAccounts.ToString();
				var alternateGLAccountModuleID = ModuleIDs.AlternateGLAccounts.ToString();
				var reportingBookModuleId = ModuleIDs.AccReportingBook.ToString();
				_ = AssertModuleAdded($"{chartModuleID} should be loaded when EnableReportingBooksFeature = true.", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.ReportingBooks, chartModuleID);
				_ = AssertModuleAdded($"{alternateGLAccountModuleID} should be loaded when EnableReportingBooksFeature = true.", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.ReportingBooks, alternateGLAccountModuleID);
				_ = AssertModuleAdded($"{reportingBookModuleId} should be loaded when EnableReportingBooksFeature = true.", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.ReportingBooks, reportingBookModuleId);

				AssertNotNull("GLReportingBooks section", Tree.Categories[ModuleTreeLoaderConstant.Category.Manage.Name].Sections[ModuleTreeLoaderConstant.Section.GLReportingBooks.Name]);
				modules = Tree.Categories[ModuleTreeLoaderConstant.Category.Manage.Name].Sections[ModuleTreeLoaderConstant.Section.GLReportingBooks.Name].Modules;
				AssertEquals("Number of GLReportingBookSection", 1, modules.Count);
				var gLReportingBooksReportModuleId = ModuleIDs.GLReportingBooksReport.ToString();
				_ = AssertModuleAdded($"{gLReportingBooksReportModuleId} should be loaded when EnableReportingBooksFeature = true.", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.GLReportingBooks, gLReportingBooksReportModuleId);
			}
		}

		#endregion

		#region Rating

		public void TestGlowRateSelector()
		{
			var featureControlMock = new Mock<IFeatureControlManager>();
			featureControlMock.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.CargoWiseCarrierConnect, CancellationToken.None)).Returns(Task.FromResult((IFeatureData)null));

			using (ObjectFactory.Substitute(featureControlMock.Object))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("CarrierConnect", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.WiseRates, ModuleIDs.CarrierConnect.ToString());
			}

			var cWCarrierConnectFeatureRule = new CWCarrierConnectFeatureRule { Enabled = true };
			var featureDataMock = new Mock<IFeatureData>();
			featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out cWCarrierConnectFeatureRule)).Returns(true);
			featureControlMock.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.CargoWiseCarrierConnect, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));

			using (ObjectFactory.Substitute(featureControlMock.Object))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("CarrierConnect", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.WiseRates, ModuleIDs.CarrierConnect.ToString());
			}
		}

		#endregion

		#region Client Specific

		ModuleSectionAddOn Section1;
		ModuleSectionAddOn Section2;
		ModuleSectionAddOn Section3;

		TestClientHook GetClientSpecificHook()
		{
			var hook = new TestClientHook();

			Section1 = new ModuleSectionAddOn(ModuleTreeLoaderConstant.Category.Manage.Name, "Section1", (NoResString)"SDisplayText1", null, Env.Security.None, IconTypes.None, IconTypes.None, ModuleTreeLoaderConstant.Subcategory.Account);
			Section2 = new ModuleSectionAddOn(ModuleTreeLoaderConstant.Category.Manage.Name, "Section2", (NoResString)"SDisplayText2", null, Env.Security.None, IconTypes.None, IconTypes.None, ModuleTreeLoaderConstant.Subcategory.Account);
			Section3 = new ModuleSectionAddOn(ModuleTreeLoaderConstant.Category.Admin.Name, "Section3", (NoResString)"SDisplayText3", null, Env.Security.None, IconTypes.None, IconTypes.None, ModuleTreeLoaderConstant.Subcategory.Account);
			var sections = new ModuleSectionAddOn[] { Section1, Section2, Section3 };

			var info1 = new ModuleInfo(ModuleID1, typeof(DummyModule1));
			var module1 = new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Manage.Name, Section1.Name, info1);

			var info2 = new ModuleInfo(ModuleID2, typeof(DummyModule2));
			var module2 = new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Manage.Name, Section2.Name, info2);

			var info3 = new ModuleInfo(ModuleID3, typeof(DummyModule3));
			var module3 = new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Admin.Name, Section3.Name, info3);

			var info4 = new ModuleInfo(ModuleID4, typeof(DummyModule4));
			var module4 = new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Admin.Name, Section3.Name, info4);

			var info5 = new ModuleInfo(ModuleID5, typeof(DummyModule4));
			var module5 = new NewClientModuleInfo(info5);

			var modules = new NewClientModuleInfo[] { module1, module2, module3, module4, module5 };

			Loader.LoadModules();

			var retrievedModules = FindModulesById(
				ModuleID1,
				ModuleID2,
				ModuleID3,
				ModuleID4,
				ModuleID5);

			CombineAssertions(() =>
			{
				foreach (var (id, mainForm) in retrievedModules)
				{
					AssertNull(id, mainForm);
				}
			});

			hook.NewModuleSectionsToAddForClientForTest = sections;
			hook.NewClientModulesForTest = modules;

			return hook;
		}

		public void TestLoadClientOverrideAndAddOn()
		{
			var hook = GetClientSpecificHook();
			using (ClientHookLoader.Instance.OverrideClientHookForTest(hook))
			{
				Loader.LoadModules();

				ModuleSection firstSection = null;
				var firstSectionFound = false;
				foreach (ModuleSection section in Tree.Categories["Manage"].Sections.Values)
				{
					if (firstSectionFound)
					{
						break;
					}

					firstSectionFound = true;

					firstSection = section;
					break;
				}

				AssertEquals("Client section should NOT be first section by default", expected: false, Section1.Name == firstSection.Name);

				AssertModuleExist(ModuleID1.ToString(), Section1);
				AssertModuleExist(ModuleID2.ToString(), Section2);
				AssertModuleExist(ModuleID3.ToString(), Section3);
				AssertModuleExist(ModuleID4.ToString(), Section3);
				AssertNull("ModuleID5 should not exist in the tree - it exists outside the tree", Tree.FindByID(ModuleID5.ToString()));
			}

			Loader.LoadModules();

			var retrievedModules = FindModulesById(
				ModuleID1,
				ModuleID2,
				ModuleID3,
				ModuleID4,
				ModuleID5);

			CombineAssertions(() =>
			{
				foreach (var (id, mainForm) in retrievedModules)
				{
					AssertNull(id, mainForm);
				}
			});
		}

		public void TestLoadClientOverrideAndAddOnTopOfTree()
		{
			var hook = GetClientSpecificHook();
			hook.SetAddNewModuleSectionsAtTopOfTreeIsSet(true);
			using (ClientHookLoader.Instance.OverrideClientHookForTest(hook))
			{
				Loader.LoadModules();
				var firstSection = GetFirstSectionFromTree("Manage");
				AssertEquals("Client section should be first section", Section1.Name, firstSection.Name);

				AssertModuleExist(ModuleID1.ToString(), Section1);
				AssertModuleExist(ModuleID2.ToString(), Section2);
				AssertModuleExist(ModuleID3.ToString(), Section3);
				AssertModuleExist(ModuleID4.ToString(), Section3);
			}

			Loader.LoadModules();

			var retrievedModules = FindModulesById(
				ModuleID1,
				ModuleID2,
				ModuleID3,
				ModuleID4);

			CombineAssertions(() =>
			{
				foreach (var (id, mainForm) in retrievedModules)
				{
					AssertNull(id, mainForm);
				}
			});
		}

		public void TestAddNewModuleSectionsAtTopOfTreeAndAdditionalModulesSomewhereElse()
		{
			var hook = new TestClientHook();
			hook.NewModuleSectionsToAddForClientForTest = new ModuleSectionAddOn[]
			{
				(Section1 = new ModuleSectionAddOn(ModuleTreeLoaderConstant.Category.Operations.Name, "ClientOperations", (NoResString)"Client Operations", null, Env.Security.None, IconTypes.None, IconTypes.None, ModuleTreeLoaderConstant.Subcategory.Account)),
				(Section2 = new ModuleSectionAddOn(ModuleTreeLoaderConstant.Category.Manage.Name, "ClientAccounts", (NoResString)"Client Accounts", null, Env.Security.None, IconTypes.None, IconTypes.None, ModuleTreeLoaderConstant.Subcategory.Account)),
				(Section3 = new ModuleSectionAddOn(ModuleTreeLoaderConstant.Category.Admin.Name, "ClientAdmin", (NoResString)"Client Admin", null, Env.Security.None, IconTypes.None, IconTypes.None, ModuleTreeLoaderConstant.Subcategory.Account))
			};
			hook.NewClientModulesForTest = new NewClientModuleInfo[]
			{
				new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations.Name, "ClientOperations", new ModuleInfo(ModuleID1, typeof(DummyModule1))),
				new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Manage.Name, "ClientAccounts", new ModuleInfo(ModuleID2, typeof(DummyModule2))),
				new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Admin.Name, "ClientAdmin", new ModuleInfo(ModuleID3, typeof(DummyModule3))),
				new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Admin.Name, ModuleTreeLoaderConstant.Section.RelationshipManagerConfig.Name, new ModuleInfo(ModuleID4, typeof(DummyModule4)))
			};

			hook.SetAddNewModuleSectionsAtTopOfTreeIsSet(true);
			using (ClientHookLoader.Instance.OverrideClientHookForTest(hook))
			{
				Loader.LoadModules();

				AssertEquals("Should be added to the top of the the tree", "ClientOperations", GetFirstSectionFromTree(ModuleTreeLoaderConstant.Category.Operations.Name).Name);
				AssertEquals("Should be added to the top of the the tree", "ClientAccounts", GetFirstSectionFromTree(ModuleTreeLoaderConstant.Category.Manage.Name).Name);
				AssertEquals("Should be added to the top of the the tree", "ClientAdmin", GetFirstSectionFromTree(ModuleTreeLoaderConstant.Category.Admin.Name).Name);

				AssertModuleExist(ModuleID1.ToString(), Section1);
				AssertModuleExist(ModuleID2.ToString(), Section2);
				AssertModuleExist(ModuleID3.ToString(), Section3);
				var relationshipManagerConfigSection = Tree.Categories[ModuleTreeLoaderConstant.Category.Admin.Name].Sections[ModuleTreeLoaderConstant.Section.RelationshipManagerConfig.Name];
				AssertModuleExist(ModuleID4.ToString(), relationshipManagerConfigSection);
			}

			Loader.LoadModules();

			var retrievedModules = FindModulesById(
				ModuleID1,
				ModuleID2,
				ModuleID3,
				ModuleID4);

			CombineAssertions(() =>
			{
				foreach (var (id, mainForm) in retrievedModules)
				{
					AssertNull(id, mainForm);
				}
			});
		}

		ModuleSection GetFirstSectionFromTree(string category)
		{
			foreach (ModuleSection section in Tree.Categories[category].Sections.Values)
			{
				return section;
			}

			return null;
		}

		void AssertModuleExist(string iD, ModuleSection expectedSection)
		{
			var formModule = Tree.FindByID(iD);
			AssertNotNull("FormModule should exist", formModule);
			AssertEquals("Section", expectedSection, formModule.ParentSection);
		}

		[ExpectNoExceptions]
		public void TestHandlingMissingCategoryAndSection()
		{
			var hook = new TestClientHook();
			using (ClientHookLoader.Instance.OverrideClientHookForTest(hook))
			{
				var section1 = new ModuleSectionAddOn("CategoryDummy", "Section1", (NoResString)"SDisplayText1", null, Env.Security.None, IconTypes.None, IconTypes.None, ModuleTreeLoaderConstant.Subcategory.Account);
				var section2 = new ModuleSectionAddOn(ModuleTreeLoaderConstant.Category.Manage.Name, "Section2", (NoResString)"SDisplayText2", null, Env.Security.None, IconTypes.None, IconTypes.None, ModuleTreeLoaderConstant.Subcategory.Account);

				var info2 = new ModuleInfo(ModuleID2, typeof(DummyModule2));
				var module2 = new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Manage.Name, section2.Name, info2);

				Loader.LoadModules();

				hook.NewModuleSectionsToAddForClientForTest = new ModuleSectionAddOn[] { section1 };
				Loader.LoadModules();

				hook.NewModuleSectionsToAddForClientForTest = null;
				hook.NewClientModulesForTest = new NewClientModuleInfo[] { module2 };
				Loader.LoadModules();

				hook.NewModuleSectionsToAddForClientForTest = new ModuleSectionAddOn[] { section2 };

				Loader.LoadModules();
			}
		}

		public enum ClientModuleIDs
		{
			ClientModuleID1,
			ClientModuleID2,
			ClientModuleID3,
			ClientModuleID4,
			ClientModuleID5
		}

		readonly static ClientModuleIdentifier ModuleID1 = new ClientModuleIdentifier(ClientModuleIDs.ClientModuleID1, "ClientModuleID1");
		readonly static ClientModuleIdentifier ModuleID2 = new ClientModuleIdentifier(ClientModuleIDs.ClientModuleID2, "ClientModuleID2");
		readonly static ClientModuleIdentifier ModuleID3 = new ClientModuleIdentifier(ClientModuleIDs.ClientModuleID3, "ClientModuleID3");
		readonly static ClientModuleIdentifier ModuleID4 = new ClientModuleIdentifier(ClientModuleIDs.ClientModuleID4, "ClientModuleID4");
		readonly static ClientModuleIdentifier ModuleID5 = new ClientModuleIdentifier(ClientModuleIDs.ClientModuleID5, "ClientModuleID5");

		class DummyModule1 : DummyModule
		{
			public override Enterprise.ZArchitecture.Modules.ModuleIdentifier ID
			{
				get { return ModuleID1; }
			}
		}

		class DummyModule2 : DummyModule
		{
			public override Enterprise.ZArchitecture.Modules.ModuleIdentifier ID
			{
				get { return ModuleID2; }
			}
		}

		class DummyModule3 : DummyModule
		{
			public override Enterprise.ZArchitecture.Modules.ModuleIdentifier ID
			{
				get { return ModuleID3; }
			}
		}

		class DummyModule4 : DummyModule
		{
			public override Enterprise.ZArchitecture.Modules.ModuleIdentifier ID
			{
				get { return ModuleID4; }
			}
		}

		[ExpectNoExceptions]
		[SnailTest]
		public void TestCanLoadTreeTwiceForAllClientDlls()
		{
			foreach (var clientDll in Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ZClient*"))
			{
				Assembly clientAssembly = null;
				try
				{
					clientAssembly = Assembly.LoadFile(clientDll);
				}
				catch
				{ }
				if (clientAssembly != null)
				{
					foreach (var type in clientAssembly.GetTypes())
					{
						if (typeof(ClientHook).IsAssignableFrom(type))
						{
							try
							{
								var clientHook = (ClientHook)Activator.CreateInstance(type, nonPublic: true);
								using (ClientHookLoader.Instance.OverrideClientHookForTest(clientHook))
								{
									for (var i = 0; i < 2; i++)
									{
										var loader = new ModuleTreeLoader();
										loader.Initialise(new ModuleTree(), Env.Security);
										loader.LoadModules();
									}
								}

								break;
							}
							catch (Exception ex)
							{
								throw new Exception("ModuleTreeLoader failed for " + clientDll, ex);
							}
						}
					}
				}
			}
		}

		#endregion

		#region Workflow

		#region Tag Rules

		public void TestTagRuleModule_WhenPlanningManagementEnabled_ShouldBeVisible()
		{
			ObjectFactory.Get<IBMTestHelper>().SetWorkflowManagementModeInRegistry("PLN");
			Loader.LoadModules();

			AssertEquals(ModuleTreeLoaderConstant.Section.BufferManagementConfig.Name, Tree.FindByID(ModuleIDs.BMTagRule.ToString()).ParentSection.Name);
		}

		public void TestTagRuleModule_WhenBufferManagementWorkflowModeEnabled_ShouldBeVisible()
		{
			ObjectFactory.Get<IBMTestHelper>().SetWorkflowManagementModeInRegistry("BUF");
			Loader.LoadModules();

			AssertEquals(ModuleTreeLoaderConstant.Section.BufferManagementConfig.Name, Tree.FindByID(ModuleIDs.BMTagRule.ToString()).ParentSection.Name);
		}

		public void TestTagRuleModule_WhenEnhancedWorkflowManagementEnabled_ShouldNotBeVisible()
		{
			ObjectFactory.Get<IBMTestHelper>().SetWorkflowManagementModeInRegistry("EWF");
			Loader.LoadModules();

			AssertNull(Tree.FindByID(ModuleIDs.BMTagRule.ToString()));
		}

		public void TestTagRuleModule_WhenBasicWorkflowManagementEnabled_ShouldNotBeVisible()
		{
			ObjectFactory.Get<IBMTestHelper>().SetWorkflowManagementModeInRegistry("BWF");
			Loader.LoadModules();

			AssertNull(Tree.FindByID(ModuleIDs.BMTagRule.ToString()));
		}

		#endregion

		#region AcceptabilityBand

		public void TestAcceptabilityBandModule_WhenPlanningManagementEnabled_ShouldBeVisible()
		{
			ObjectFactory.Get<IBMTestHelper>().SetWorkflowManagementModeInRegistry("PLN");
			Loader.LoadModules();

			AssertEquals(ModuleTreeLoaderConstant.Section.BufferManagementConfig.Name, Tree.FindByID(ModuleIDs.AcceptabilityBand.ToString()).ParentSection.Name);
		}

		public void TestAcceptabilityBandModule_WhenBufferManagementWorkflowModeEnabled_ShouldBeVisible()
		{
			ObjectFactory.Get<IBMTestHelper>().SetWorkflowManagementModeInRegistry("BUF");
			Loader.LoadModules();

			AssertEquals(ModuleTreeLoaderConstant.Section.BufferManagementConfig.Name, Tree.FindByID(ModuleIDs.AcceptabilityBand.ToString()).ParentSection.Name);
		}

		public void TestAcceptabilityBandModule_WhenEnhancedWorkflowManagementEnabled_ShouldNotBeVisible()
		{
			ObjectFactory.Get<IBMTestHelper>().SetWorkflowManagementModeInRegistry("EWF");
			Loader.LoadModules();

			AssertNull(Tree.FindByID(ModuleIDs.AcceptabilityBand.ToString()));
		}

		public void TestAcceptabilityBandModule_WhenBasicWorkflowManagementEnabled_ShouldNotBeVisible()
		{
			ObjectFactory.Get<IBMTestHelper>().SetWorkflowManagementModeInRegistry("BWF");
			Loader.LoadModules();

			AssertNull(Tree.FindByID(ModuleIDs.AcceptabilityBand.ToString()));
		}

		#endregion

		#region MENT

		public void TestMENTModule_WhenPlanningManagementEnabled_ShouldBeVisible()
		{
			ObjectFactory.Get<IBMTestHelper>().SetWorkflowManagementModeInRegistry("PLN");
			Loader.LoadModules();

			AssertEquals(ModuleTreeLoaderConstant.Section.BufferManagementConfig.Name, Tree.FindByID(ModuleIDs.MENTAgedScoreQuery.ToString()).ParentSection.Name);
		}

		public void TestMENTModule_WhenBufferManagementWorkflowModeEnabled_ShouldBeVisible()
		{
			ObjectFactory.Get<IBMTestHelper>().SetWorkflowManagementModeInRegistry("BUF");
			Loader.LoadModules();

			AssertEquals(ModuleTreeLoaderConstant.Section.BufferManagementConfig.Name, Tree.FindByID(ModuleIDs.MENTAgedScoreQuery.ToString()).ParentSection.Name);
		}

		public void TestMENTModule_WhenEnhancedWorkflowManagementEnabled_ShouldNotBeVisible()
		{
			ObjectFactory.Get<IBMTestHelper>().SetWorkflowManagementModeInRegistry("EWF");
			Loader.LoadModules();

			AssertNull(Tree.FindByID(ModuleIDs.MENTAgedScoreQuery.ToString()));
		}

		public void TestMENTModule_WhenBasicWorkflowManagementEnabled_ShouldNotBeVisible()
		{
			ObjectFactory.Get<IBMTestHelper>().SetWorkflowManagementModeInRegistry("BWF");
			Loader.LoadModules();

			AssertNull(Tree.FindByID(ModuleIDs.MENTAgedScoreQuery.ToString()));
		}

		#endregion

		public void TestWorkflowModules()
		{
			System.Environment.SetEnvironmentVariable("EXTREQ_Enabled", "tRUe");

			Loader.LoadModules();

			var tasks = Tree.FindByID(ModuleIDs.ProcessTasks.ToString());
			var workflowExceptions = Tree.FindByID(ModuleIDs.WorkflowExceptions.ToString());
			var reports = Tree.FindByID(ModuleIDs.ProcessMgrReports.ToString());
			var externalRequests = Tree.FindByID(ModuleIDs.ExternalRequests.ToString());
			var templates = Tree.FindByID(ModuleIDs.ProcessTemplates.ToString());
			var events = Tree.FindByID(ModuleIDs.Events.ToString());
			var addons = Tree.FindByID(ModuleIDs.GenCustomAddOnRule.ToString());

			AssertEquals(ModuleTreeLoaderConstant.Section.ManageWorkflowSection.Name, tasks.ParentSection.Name);
			AssertEquals(ModuleTreeLoaderConstant.Section.ManageWorkflowSection.Name, workflowExceptions.ParentSection.Name);
			AssertEquals(ModuleTreeLoaderConstant.Section.ManageWorkflowSection.Name, reports.ParentSection.Name);
			AssertEquals(ModuleTreeLoaderConstant.Section.ManageWorkflowSection.Name, externalRequests.ParentSection.Name);
			AssertEquals(ModuleTreeLoaderConstant.Section.WorkflowSection.Name, templates.ParentSection.Name);
			AssertEquals(ModuleTreeLoaderConstant.Section.WorkflowSection.Name, events.ParentSection.Name);
			AssertEquals(ModuleTreeLoaderConstant.Section.WorkflowSection.Name, addons.ParentSection.Name);

			var externalRequestModules = FindModulesById(
				ModuleIDs.ExternalRequestTypes,
				ModuleIDs.ExternalRequestInfoTemplate,
				ModuleIDs.ExternalRequests);

			CombineAssertions(() =>
			{
				foreach (var (id, mainForm) in externalRequestModules)
				{
					AssertNotNull(id, mainForm);
				}
			});

			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = false;
			System.Environment.SetEnvironmentVariable("EXTREQ_Enabled", "fALse");

			Loader.LoadModules();

			var retrievedModules = FindModulesById(
				ModuleIDs.BMSystems,
				ModuleIDs.BMBoard,
				ModuleIDs.BMBoardSlideshow,
				ModuleIDs.BMControlCustomisation,
				ModuleIDs.BMTagDefinition,
				ModuleIDs.BMTagRule,
				ModuleIDs.NetworkDiagram,
				ModuleIDs.WorkQueues,
				ModuleIDs.ProcessHeader,
				ModuleIDs.MENTAgedScoreQuery);
			externalRequestModules = FindModulesById(
				ModuleIDs.ExternalRequestTypes,
				ModuleIDs.ExternalRequestInfoTemplate,
				ModuleIDs.ExternalRequests);

			CombineAssertions(() =>
			{
				foreach (var (id, mainForm) in retrievedModules)
				{
					AssertNull(id, mainForm);
				}
				foreach (var (id, mainForm) in externalRequestModules)
				{
					AssertNull(id, mainForm);
				}
			});

			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;

			Loader.LoadModules();

			AssertEquals(ModuleTreeLoaderConstant.Section.BufferManagementConfig.Name, Tree.FindByID(ModuleIDs.BMSystems.ToString()).ParentSection.Name);
			AssertEquals(ModuleTreeLoaderConstant.Section.BufferManagementConfig.Name, Tree.FindByID(ModuleIDs.BMBoard.ToString()).ParentSection.Name);
			AssertEquals(ModuleTreeLoaderConstant.Section.BufferManagementConfig.Name, Tree.FindByID(ModuleIDs.BMBoardSlideshow.ToString()).ParentSection.Name);
			AssertEquals(ModuleTreeLoaderConstant.Section.BufferManagementConfig.Name, Tree.FindByID(ModuleIDs.BMControlCustomisation.ToString()).ParentSection.Name);
			AssertEquals(ModuleTreeLoaderConstant.Section.BufferManagementConfig.Name, Tree.FindByID(ModuleIDs.BMTagDefinition.ToString()).ParentSection.Name);
			AssertEquals(ModuleTreeLoaderConstant.Section.BufferManagementConfig.Name, Tree.FindByID(ModuleIDs.BMTagRule.ToString()).ParentSection.Name);
			AssertEquals(ModuleTreeLoaderConstant.Section.BufferManagementConfig.Name, Tree.FindByID(ModuleIDs.MENTAgedScoreQuery.ToString()).ParentSection.Name);
			AssertEquals(ModuleTreeLoaderConstant.Section.ManageWorkflowSection.Name, Tree.FindByID(ModuleIDs.ProcessHeader.ToString()).ParentSection.Name);

			AssertEquals(ModuleTreeLoaderConstant.Section.ProductivitySection.Name, Tree.FindByID(ModuleIDs.WorkItem.ToString()).ParentSection.Name);
			AssertEquals(ModuleTreeLoaderConstant.Section.ProductivitySection.Name, Tree.FindByID(ModuleIDs.Project.ToString()).ParentSection.Name);
			AssertEquals(ModuleTreeLoaderConstant.Section.ProductivitySection.Name, Tree.FindByID(ModuleIDs.CustomerServiceTicket.ToString()).ParentSection.Name);

			AssertEquals(ModuleTreeLoaderConstant.Section.WorkflowPlanning.Name, Tree.FindByID(ModuleIDs.NetworkDiagram.ToString()).ParentSection.Name);
			AssertEquals(ModuleTreeLoaderConstant.Section.WorkflowPlanning.Name, Tree.FindByID(ModuleIDs.WorkQueues.ToString()).ParentSection.Name);
			AssertNull(Tree.FindByID(ModuleIDs.BMReleaseSequence.ToString()));

			ObjectFactory.Get<IBMSRegistry>().ReleaseSequencesModuleEnabled = true;

			Loader.LoadModules();

			AssertEquals(ModuleTreeLoaderConstant.Section.WorkflowPlanning.Name, Tree.FindByID(ModuleIDs.BMReleaseSequence.ToString()).ParentSection.Name);
		}

		public void TestWorkflowPlanningSection_WhenBasicWorkflowManagementEnabled_ShouldNotBeIncluded()
		{
			ObjectFactory.Get<IBMTestHelper>().SetWorkflowManagementModeInRegistry("BWF");
			ObjectFactory.Get<IBMSRegistry>().ReleaseSequencesModuleEnabled = true;

			Loader.LoadModules();

			var retrievedModules = FindModulesById(
				ModuleIDs.NetworkDiagram,
				ModuleIDs.WorkQueues,
				ModuleIDs.BMReleaseSequence);

			CombineAssertions(() =>
			{
				foreach (var (id, mainForm) in retrievedModules)
				{
					AssertNull(id, mainForm);
				}
			});
		}

		public void TestWorkflowPlanningSection_WhenEnhancedWorkflowManagementEnabled_ShouldNotBeIncluded()
		{
			ObjectFactory.Get<IBMTestHelper>().SetWorkflowManagementModeInRegistry("EWF");
			ObjectFactory.Get<IBMSRegistry>().ReleaseSequencesModuleEnabled = true;

			Loader.LoadModules();

			var retrievedModules = FindModulesById(
				ModuleIDs.NetworkDiagram,
				ModuleIDs.WorkQueues,
				ModuleIDs.BMReleaseSequence);

			CombineAssertions(() =>
			{
				foreach (var (id, mainForm) in retrievedModules)
				{
					AssertNull(id, mainForm);
				}
			});
		}

		public void TestWorkflowPlanningSection_WhenBufferManagementWorkflowModeEnabled_ShouldNotBeIncluded()
		{
			ObjectFactory.Get<IBMTestHelper>().SetWorkflowManagementModeInRegistry("BUF");
			ObjectFactory.Get<IBMSRegistry>().ReleaseSequencesModuleEnabled = true;

			Loader.LoadModules();

			var retrievedModules = FindModulesById(
				ModuleIDs.NetworkDiagram,
				ModuleIDs.WorkQueues,
				ModuleIDs.BMReleaseSequence);

			CombineAssertions(() =>
			{
				foreach (var (id, mainForm) in retrievedModules)
				{
					AssertNull(id, mainForm);
				}
			});
		}

		public void TestWorkflowPlanningSection_WhenPlanningManagementEnabled_ShouldBeIncluded()
		{
			ObjectFactory.Get<IBMTestHelper>().SetWorkflowManagementModeInRegistry("PLN");
			ObjectFactory.Get<IBMSRegistry>().ReleaseSequencesModuleEnabled = true;

			Loader.LoadModules();

			var retrievedModules = FindModulesById(
				ModuleIDs.NetworkDiagram,
				ModuleIDs.WorkQueues,
				ModuleIDs.BMReleaseSequence);

			CombineAssertions(() =>
			{
				foreach (var (id, mainForm) in retrievedModules)
				{
					AssertEquals(ModuleTreeLoaderConstant.Section.WorkflowPlanning.Name, mainForm.ParentSection.Name);
				}
			});
		}

		#region

		public void TestComponentRelationships_WhenPlanningManagementEnabled_ShouldBeIncluded()
		{
			ObjectFactory.Get<IBMTestHelper>().SetWorkflowManagementModeInRegistry("PLN");
			Loader.LoadModules();

			AssertEquals(ModuleTreeLoaderConstant.Section.BufferManagementConfig.Name, Tree.FindByID(ModuleIDs.ComponentRelationship.ToString()).ParentSection.Name);
		}

		public void TestComponentRelationships_WhenBufferManagementWorkflowModeEnabled_ShouldBeIncluded()
		{
			ObjectFactory.Get<IBMTestHelper>().SetWorkflowManagementModeInRegistry("BUF");
			Loader.LoadModules();

			AssertEquals(ModuleTreeLoaderConstant.Section.BufferManagementConfig.Name, Tree.FindByID(ModuleIDs.ComponentRelationship.ToString()).ParentSection.Name);
		}

		public void TestComponentRelationships_WhenEnhancedWorkflowManagementEnabled_ShouldNotBeIncluded()
		{
			ObjectFactory.Get<IBMTestHelper>().SetWorkflowManagementModeInRegistry("EWF");
			Loader.LoadModules();

			AssertNull(Tree.FindByID(ModuleIDs.ComponentRelationship.ToString()));
		}

		public void TestComponentRelationships_WhenBasicWorkflowManagementEnabled_ShouldNotBeIncluded()
		{
			ObjectFactory.Get<IBMTestHelper>().SetWorkflowManagementModeInRegistry("BWF");
			Loader.LoadModules();

			AssertNull(Tree.FindByID(ModuleIDs.ComponentRelationship.ToString()));
		}

		#endregion

		#endregion

		#region Reference Files

		public void TestReferenceFiles()
		{
			Loader.LoadModules();
			AssertNotNull(Tree.FindByID(ModuleIDs.RefOrgPartCategory.ToString()));
			AssertNotNull(Tree.FindByID(ModuleIDs.UNDGCommonData.ToString()));
			AssertEquals(ModuleIDs.UNDGCommonData.Description, Tree.FindByID(ModuleIDs.UNDGCommonData.ToString()).SecurityCheckpoint.DisplayText);
			AssertEquals("References", Tree.FindByID(ModuleIDs.UNDGCommonData.ToString()).ParentSection.Name);

			AssertNotNull(Tree.FindByID(ModuleIDs.RefAccessorial.ToString()));

			using (ObjectFactory.Get<ITransportRegistry>().EnableBookingWithCarrierMessagingBuss.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Loader.LoadModules();
				var refMessagingBussCarrierInfoModule = Tree.FindByID(ModuleIDs.RefMessagingBussCarrierInfo.ToString());
				AssertNotNull(refMessagingBussCarrierInfoModule);

				DataRegistry.Instance.ProductivityWiseModeEnabled = true;
				Loader.LoadModules();
				AssertNull(Tree.FindByID(ModuleIDs.RefMessagingBussCarrierInfo.ToString()));
				AssertNull(Tree.FindByID(ModuleIDs.RefAccessorial.ToString()));
				AssertNull(Tree.FindByID(ModuleIDs.RefJobEquipment.ToString()));
			}

			using (ObjectFactory.Get<ITransportRegistry>().EnableEquipmentCombination.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				DataRegistry.Instance.ProductivityWiseModeEnabled = false;
				Loader.LoadModules();
				var refquipmentCombinationModule = Tree.FindByID(ModuleIDs.RefJobEquipment.ToString());
				AssertNotNull(refquipmentCombinationModule);

				DataRegistry.Instance.ProductivityWiseModeEnabled = true;
				Loader.LoadModules();
				AssertNull(Tree.FindByID(ModuleIDs.RefJobEquipment.ToString()));
			}
		}

		#endregion

		#region Archive Manager

		public void TestArchiveManager()
		{
			Loader.LoadModules();

			var moduleId = ModuleIDs.ArchivedRecords.ToString();
			_ = AssertModuleAdded(string.Format("{0} should be loaded.", moduleId), expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.ArchiveManagerSection, moduleId);
			var module = Tree.FindByID(moduleId);
			var section = module.ParentSection;
			AssertEquals(ModuleTreeLoaderConstant.Section.ArchiveManagerSection.Name, section.Name);

			moduleId = ModuleIDs.ArchiveSchedule.ToString();
			_ = AssertModuleAdded(string.Format("{0} should be loaded.", moduleId), expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.ArchiveManagerSection, moduleId);
			module = Tree.FindByID(moduleId);
			section = module.ParentSection;
			AssertEquals(ModuleTreeLoaderConstant.Section.ArchiveManagerSection.Name, section.Name);

			moduleId = ModuleIDs.ArchiveReports.ToString();
			_ = AssertModuleAdded(string.Format("{0} should be loaded.", moduleId), expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.ArchiveManagerSection, moduleId);
			module = Tree.FindByID(moduleId);
			section = module.ParentSection;
			AssertEquals(ModuleTreeLoaderConstant.Section.ArchiveManagerSection.Name, section.Name);
		}

		#endregion

		#region Master Data

		public void TestMasterData()
		{
			SystemDataRegistry.Instance.PersonIntelligenceModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Loader.LoadModules();

			var moduleId = ModuleIDs.Organisation.ToString();
			_ = AssertModuleAdded(string.Format("{0} should be loaded.", moduleId), expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.MasterData, moduleId);
			var module = Tree.FindByID(moduleId);
			var section = module.ParentSection;
			AssertEquals(ModuleTreeLoaderConstant.Section.MasterData.Name, section.Name);

			moduleId = ModuleIDs.AdministrationPanel.ToString();
			_ = AssertModuleAdded(string.Format("{0} should be loaded.", moduleId), expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.MasterData, moduleId);
			module = Tree.FindByID(moduleId);
			section = module.ParentSection;
			AssertEquals(ModuleTreeLoaderConstant.Section.MasterData.Name, section.Name);

			moduleId = ModuleIDs.MasterDataReports.ToString();
			_ = AssertModuleAdded(string.Format("{0} should be loaded.", moduleId), expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.MasterData, moduleId);
			module = Tree.FindByID(moduleId);
			section = module.ParentSection;
			AssertEquals(ModuleTreeLoaderConstant.Section.MasterData.Name, section.Name);

			moduleId = ModuleIDs.GlbPerson.ToString();
			_ = AssertModuleAdded(string.Format("{0} should be loaded.", moduleId), expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.MasterData, moduleId);
			module = Tree.FindByID(moduleId);
			section = module.ParentSection;
			AssertEquals(ModuleTreeLoaderConstant.Section.MasterData.Name, section.Name);

			var subcategory = Tree.Categories[ModuleTreeLoaderConstant.Category.Admin.Name].Sections[ModuleTreeLoaderConstant.Section.MasterData.Name].Subcategory;
			AssertEquals(ModuleTreeLoaderConstant.Subcategory.MasterData.Name, subcategory.Name);
		}

		public void TestMdmAdministrationPanel()
		{
			Loader.LoadModules();
			AssertNotNull(Tree.FindByID(ModuleIDs.AdministrationPanel.ToString()));

			SystemDataRegistry.Instance.MdmAdministrationPanelEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Loader.LoadModules();
			AssertNull(Tree.FindByID(ModuleIDs.AdministrationPanel.ToString()));
		}

		public void TestOrgContacts()
		{
			Loader.LoadModules();
			AssertNotNull(Tree.FindByID(ModuleIDs.OrgContacts.ToString()));
		}

		#endregion

		#region Jump Category

		public void TestJumpCategory()
		{
			Loader.LoadModules();
			AssertEquals("Number of sections", 3, Tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name].Sections.Count);
			AssertNotNull(Tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name].Sections[ModuleTreeLoaderConstant.Section.Favorites.Name]);
			AssertNotNull(Tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name].Sections[ModuleTreeLoaderConstant.Section.RecentItems.Name]);
			AssertNotNull(Tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name].Sections[ModuleTreeLoaderConstant.Section.RecentModules.Name]);
		}

		public void TestFavoritesSection()
		{
			PopulateFavoritesAndRecent();
			Loader.LoadModules();
			AssertEquals("Number of favorites", 2, Tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name].Sections[ModuleTreeLoaderConstant.Section.Favorites.Name].Modules.Count);

			var i = 0;
			foreach (MainFormModule module in Tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name].Sections[ModuleTreeLoaderConstant.Section.Favorites.Name].Modules.Values)
			{
				AssertShortcutSameAsModule(new LinkWrapper(RecentItemManager.Instance.FavoriteModules[i++]), module);
			}
		}

		[ExpectNoExceptions]
		public void TestFavoritesWithDuplicates()
		{
			AssertEquals("Number of favorites before test", 0, RecentItemManager.Instance.FavoriteModules.Count);
			PopulateFavoritesWithDuplicates();
			AssertEquals("Number of favorites before loading", 2, RecentItemManager.Instance.FavoriteModules.Count);
			Loader.LoadModules();
			AssertEquals("Number of favorites", 1, Tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name].Sections[ModuleTreeLoaderConstant.Section.Favorites.Name].Modules.Count);
		}

		public void TestFavoritesWithBadModules()
		{
			AssertEquals("Number of favorites before test", 0, RecentItemManager.Instance.FavoriteModules.Count);
			PopulateFavoritesWithBadModules();
			AssertEquals("Number of favorites before loading", 3, RecentItemManager.Instance.FavoriteModules.Count);
			Loader.LoadModules();
			AssertEquals("Number of favorites", 2, Tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name].Sections[ModuleTreeLoaderConstant.Section.Favorites.Name].Modules.Count);
			AssertEquals("Number of favorites according to favorites list", 2, RecentItemManager.Instance.FavoriteModules.Count);
		}

		public void TestFavoritesAndRecentItemsWithModulesNotInTree()
		{
			AssertEquals("Number of favorites before test", 0, RecentItemManager.Instance.FavoriteModules.Count);
			AssertEquals("Number of recent items before test", 0, RecentItemManager.Instance.GetRecentItems(string.Empty).Count);

			var expectedItemsCount = PopulateFavoritesAndRecentItemsWithModulesNotInTree(true);
			AssertEquals("Number of favorites before loading", expectedItemsCount, RecentItemManager.Instance.FavoriteModules.Count);
			AssertEquals("Number of recent items before loading", expectedItemsCount, RecentItemManager.Instance.GetRecentItems(string.Empty).Count);

			Loader.LoadModules();

			AssertEquals("Number of favorites", expectedItemsCount, Tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name].Sections[ModuleTreeLoaderConstant.Section.Favorites.Name].Modules.Count);
			AssertEquals("Number of favorites according to favorites list", expectedItemsCount, RecentItemManager.Instance.FavoriteModules.Count);
			AssertEquals("Number of recent items", expectedItemsCount, Tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name].Sections[ModuleTreeLoaderConstant.Section.RecentItems.Name].Modules.Count);
		}

		public void TestFavoritesAndRecentItemsWithModulesNotInTree_RecordsOnly()
		{
			AssertEquals("Number of favorites before test", 0, RecentItemManager.Instance.FavoriteModules.Count);
			AssertEquals("Number of recent items before test", 0, RecentItemManager.Instance.GetRecentItems(string.Empty).Count);

			var expectedItemsCount = PopulateFavoritesAndRecentItemsWithModulesNotInTree(false);
			AssertEquals("Number of favorites before loading", expectedItemsCount, RecentItemManager.Instance.FavoriteModules.Count);
			AssertEquals("Number of recent items before loading", expectedItemsCount, RecentItemManager.Instance.GetRecentItems(string.Empty).Count);

			Loader.LoadModules();

			AssertEquals("Number of favorites", expectedItemsCount, Tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name].Sections[ModuleTreeLoaderConstant.Section.Favorites.Name].Modules.Count);
			AssertEquals("Number of favorites according to favorites list", expectedItemsCount, RecentItemManager.Instance.FavoriteModules.Count);
			AssertEquals("Number of recent items", expectedItemsCount, Tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name].Sections[ModuleTreeLoaderConstant.Section.RecentItems.Name].Modules.Count);
		}

		public void TestRecentItemsSection()
		{
			PopulateFavoritesAndRecent();
			Loader.LoadModules();
			AssertEquals("Number of recent items", 1, Tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name].Sections[ModuleTreeLoaderConstant.Section.RecentItems.Name].Modules.Count);

			var recent = RecentItemManager.Instance.GetRecentItems(string.Empty);
			var i = 0;
			foreach (MainFormModule module in Tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name].Sections[ModuleTreeLoaderConstant.Section.RecentItems.Name].Modules.Values)
			{
				AssertShortcutSameAsModule(new LinkWrapper(recent[i++]), module);
			}
		}

		[ExpectNoExceptions]
		public void TestRecentItemsWithDuplicates()
		{
			AssertEquals("Number of recent items before test", 0, RecentItemManager.Instance.GetRecentItems(string.Empty).Count);
			PopulateRecentItemsWithDuplicates();
			AssertEquals("Number of recent items before loading", 2, RecentItemManager.Instance.GetRecentItems(string.Empty).Count);
			Loader.LoadModules();
			AssertEquals("Number of recent items", 1, Tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name].Sections[ModuleTreeLoaderConstant.Section.RecentItems.Name].Modules.Count);
		}

		public void TestRecentModulesSection()
		{
			PopulateFavoritesAndRecent();
			Loader.LoadModules();
			AssertEquals("Number of recent modules", 1, Tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name].Sections[ModuleTreeLoaderConstant.Section.RecentModules.Name].Modules.Count);

			var i = 0;
			foreach (MainFormModule module in Tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name].Sections[ModuleTreeLoaderConstant.Section.RecentModules.Name].Modules.Values)
			{
				AssertShortcutSameAsModule(new LinkWrapper(RecentItemManager.Instance.RecentModules[i++]), module);
			}
		}

		[ExpectNoExceptions]
		public void TestRecentModulesWithDuplicates()
		{
			AssertEquals("Number of recent items before test", 0, RecentItemManager.Instance.RecentModules.Count);
			PopulateRecentModulesWithDuplicates();
			AssertEquals("Number of recent items before loading", 2, RecentItemManager.Instance.RecentModules.Count);
			Loader.LoadModules();
			AssertEquals("Number of recent items", 1, Tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name].Sections[ModuleTreeLoaderConstant.Section.RecentModules.Name].Modules.Count);
		}

		public void TestFavoritesWithDummyModules()
		{
			PopulateFavoritesAndRecentWithDummyModules();
			Loader.LoadModules();
			AssertEquals("Number of favorites", 0, Tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name].Sections[ModuleTreeLoaderConstant.Section.Favorites.Name].Modules.Count);
		}

		void AssertShortcutSameAsModule(LinkWrapper shortcut, MainFormModule module)
		{
			AssertEquals(shortcut.ModuleName, module.ModuleID.Name);
			AssertEquals(shortcut.RecordKey, module.RecordKey);
			AssertEquals(shortcut.RecordUrl, module.RecordUrl);
			if (!shortcut.IsModule)
			{
				AssertEquals(shortcut.RecordDescription, module.RecordDescription);
			}
		}

		void PopulateFavoritesAndRecent(string module = "JobShipment")
		{
			_ = RecentItemManager.Instance.AddToFavoriteModules(new LinkWrapper(module));
			_ = RecentItemManager.Instance.AddToFavoriteModules(new LinkWrapper(module, Guid.NewGuid(), "http://", "description"));
			RecentItemManager.Instance.AddOrUpdateRecentItems(string.Empty, new LinkWrapper(module, Guid.NewGuid(), "http://", "description"));
			RecentItemManager.Instance.AddOrUpdateRecentModules(new LinkWrapper(module));
		}

		void PopulateFavoritesAndRecentWithDummyModules()
		{
			PopulateFavoritesAndRecent("Dummy");
		}

		void PopulateFavoritesWithDuplicates(string module = "JobShipment")
		{
			RecentItemManager.Instance.AddToFavoriteForTest(new LinkWrapper(module));
			RecentItemManager.Instance.AddToFavoriteForTest(new LinkWrapper(module));
		}

		void PopulateFavoritesWithBadModules()
		{
			RecentItemManager.Instance.AddToFavoriteForTest(new LinkWrapper("JobShipment"));
			RecentItemManager.Instance.AddToFavoriteForTest(new LinkWrapper("BadJobShipment"));
			RecentItemManager.Instance.AddToFavoriteForTest(new LinkWrapper("JobConsol"));
		}

		int PopulateFavoritesAndRecentItemsWithModulesNotInTree(bool linkDirectlyToModule)
		{
			foreach (var module in ValidModulesNotInTree)
			{
				var link = linkDirectlyToModule ? new LinkWrapper(module.Name) : new LinkWrapper(module.Name, Guid.NewGuid(), "edient://", "Some PAVE board");

				RecentItemManager.Instance.AddToFavoriteForTest(link);
				RecentItemManager.Instance.AddToRecentItemsForTest(string.Empty, link);
			}

			return ValidModulesNotInTree.Length;
		}

		static ModuleIdentifier[] ValidModulesNotInTree
		{
			get
			{
				return new[]
				{
					ModuleIDs.VisualBoard,
				};
			}
		}

		void PopulateRecentItemsWithDuplicates(string module = "JobShipment")
		{
			var guid = Guid.NewGuid();
			RecentItemManager.Instance.AddToRecentItemsForTest(string.Empty, new LinkWrapper(module, guid, "http://", "description"));
			RecentItemManager.Instance.AddToRecentItemsForTest(string.Empty, new LinkWrapper(module, guid, "http://", "description"));
		}

		void PopulateRecentModulesWithDuplicates(string module = "JobShipment")
		{
			RecentItemManager.Instance.AddToRecentModulesForTest(new LinkWrapper(module));
			RecentItemManager.Instance.AddToRecentModulesForTest(new LinkWrapper(module));
		}

		#endregion

		#region Business Intelligence

		public void TestBusinessIntelligenceModules()
		{
			using (SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Db.ServerName))
			using (SystemDataRegistry.Instance.BiReportUserCredential.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, BiReportCredentialRegistryItem.BiReportCredentialTestValue()))
			using (SystemDataRegistry.Instance.BiAnalysisServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
			using (SystemDataRegistry.Instance.BiAuditServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
			using (SystemDataRegistry.Instance.BiPowerBiWebPortalUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
			{
				Loader.LoadModules();

				CombineAssertions(() =>
				{
					AssertNotNull("BiManager", Tree.FindByID(ModuleIDs.BiManager.ToString()));
					AssertNull("AnalyticsReports", Tree.FindByID(ModuleIDs.AnalyticsReports.ToString()));
				});
			}

			using (SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Db.ServerName))
			using (SystemDataRegistry.Instance.BiReportUserCredential.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, BiReportCredentialRegistryItem.BiReportCredentialTestValue()))
			using (SystemDataRegistry.Instance.BiAnalysisServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Db.ServerName))
			using (SystemDataRegistry.Instance.BiAuditServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Db.ServerName))
			using (SystemDataRegistry.Instance.BiPowerBiWebPortalUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
			{
				Loader.LoadModules();

				CombineAssertions(() =>
				{
					AssertNotNull("BiManager", Tree.FindByID(ModuleIDs.BiManager.ToString()));
					AssertNull("AnalyticsReports", Tree.FindByID(ModuleIDs.AnalyticsReports.ToString()));
				});
			}

			using (SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Db.ServerName))
			using (SystemDataRegistry.Instance.BiReportUserCredential.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, BiReportCredentialRegistryItem.BiReportCredentialTestValue()))
			using (SystemDataRegistry.Instance.BiAnalysisServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Db.ServerName))
			using (SystemDataRegistry.Instance.BiAuditServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Db.ServerName))
			using (SystemDataRegistry.Instance.BiPowerBiWebPortalUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "PowerBiReports"))
			{
				Loader.LoadModules();

				CombineAssertions(() =>
				{
					AssertNotNull("BiManager", Tree.FindByID(ModuleIDs.BiManager.ToString()));
					AssertNotNull("AnalyticsReports", Tree.FindByID(ModuleIDs.AnalyticsReports.ToString()));
				});
			}
		}

		#endregion

		#region ProductivityWise

		public void TestProductivityToolsSubtree_ShouldAlwaysExist_ShouldContainDesiredModules()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = false;
			AssertProductivityModulesExistAndAreCorrect();

			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			AssertProductivityModulesExistAndAreCorrect();
		}

		void AssertProductivityModulesExistAndAreCorrect()
		{
			Loader.LoadModules();
			var security = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);

			AssertProductivityModuleIsCorrect(ModuleIDs.CustomerServiceTicket.ToString(), security.FindCheckPoint(security.CustomerServiceTicket.Code));
			AssertProductivityModuleIsCorrect(ModuleIDs.Project.ToString(), security.FindCheckPoint(security.Project.Code));
			AssertProductivityModuleIsCorrect(ModuleIDs.WorkItem.ToString(), security.FindCheckPoint(security.WorkItem.Code));
		}

		void AssertProductivityModuleIsCorrect(string moduleID, ISecurityCheckpoint expectedSecurity)
		{
			var productivityModuleOfChoice = Tree.FindByID(moduleID);

			AssertNotNull("We expect " + moduleID + " to exist, but instead...", productivityModuleOfChoice);
			AssertEquals("We expect " + moduleID + " to have " + moduleID + " Security rights, but instead...", expectedSecurity.ToString(), productivityModuleOfChoice.SecurityCheckpoint.ToString());
			AssertEquals("We expect " + moduleID + " to exist within ProductivityTools, but instead...", productivityModuleOfChoice.ParentSection.DisplayText, "Productivity Tools");
		}

		public void TestProductivityWiseModuleTreeSections_WhenBufferManagementDisabled_ShouldContainNonFreightForwardingModulesOnly()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = false;
			Loader.LoadModules();

			AssertModuleCategorySections(Tree, ModuleTreeLoaderConstant.Category.Jump.Name, new[]
			{
				ModuleTreeLoaderConstant.Section.Favorites.Name,
				ModuleTreeLoaderConstant.Section.RecentItems.Name,
				ModuleTreeLoaderConstant.Section.RecentModules.Name
			});

			AssertModuleCategorySections(Tree, ModuleTreeLoaderConstant.Category.Operations.Name, new[]
			{
				ModuleTreeLoaderConstant.Section.ProductivitySection.Name
			});

			// Manage
			AssertModuleCategorySections(Tree, ModuleTreeLoaderConstant.Category.Manage.Name, new[]
			{
				ModuleTreeLoaderConstant.Section.ClientRelationshipManagement.Name,
				ModuleTreeLoaderConstant.Section.BusinessIntelligenceAndAnalytics.Name,
				ModuleTreeLoaderConstant.Section.ManageWorkflowSection.Name,
				ModuleTreeLoaderConstant.Section.DocManager.Name,
				ModuleTreeLoaderConstant.Section.Receivables.Name,
				ModuleTreeLoaderConstant.Section.Payables.Name,
				ModuleTreeLoaderConstant.Section.CashBook.Name,
				ModuleTreeLoaderConstant.Section.JobCosting.Name,
				ModuleTreeLoaderConstant.Section.GeneralLedger.Name,
				ModuleTreeLoaderConstant.Section.GLConsolidations.Name,
				ModuleTreeLoaderConstant.Section.Budgets.Name
			});

			// Maintain
			AssertModuleCategorySections(Tree, ModuleTreeLoaderConstant.Category.Admin.Name, new[]
			{
				ModuleTreeLoaderConstant.Section.MasterData.Name,
				ModuleTreeLoaderConstant.Section.References.Name,
				ModuleTreeLoaderConstant.Section.RelationshipManagerConfig.Name,
				ModuleTreeLoaderConstant.Section.Location.Name,
				ModuleTreeLoaderConstant.Section.Account.Name,
				ModuleTreeLoaderConstant.Section.PeopleOperations.Name,
				ModuleTreeLoaderConstant.Section.LearningDevelopment.Name,
				ModuleTreeLoaderConstant.Section.HRRecruiter.Name,
				ModuleTreeLoaderConstant.Section.WorkflowSection.Name,
				ModuleTreeLoaderConstant.Section.EDIMessaging.Name,
				ModuleTreeLoaderConstant.Section.ArchiveManagerSection.Name,
				ModuleTreeLoaderConstant.Section.UserAdmin.Name,
				ModuleTreeLoaderConstant.Section.System.Name,
				ModuleTreeLoaderConstant.Section.PrintingSection.Name,
				ModuleTreeLoaderConstant.Section.EmailSection.Name,
				ModuleTreeLoaderConstant.Section.ReportSection.Name
			});
		}

		public void TestEAdaptorNextFeatureModule()
		{
			var ediCommunicationsModeModule = ModuleIDs.Messaging.EDICommunicationsMode;
			var ediCommunicationParty = ModuleIDs.Messaging.EDICommunicationParty;

			Loader.LoadModules();
			AssertModuleAdded("Module should not be loaded if EAdaptorNextFeature is not active.", false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.EDIMessaging, ediCommunicationsModeModule.Name);
			AssertModuleAdded("Module should not be loaded if EAdaptorNextFeature is not active.", false, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.EDIMessaging, ediCommunicationParty.Name);

			var mockIFeatureData = new Mock<IFeatureData>();
			var mockIFeatureControlManager = new Mock<IFeatureControlManager>();
			mockIFeatureControlManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.EAdaptorNextFeature, CancellationToken.None)).Returns(Task.FromResult(mockIFeatureData.Object));

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				Loader.LoadModules();
				AssertModuleAdded("Module should be loaded if EAdaptorNextFeature is active.", true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.EDIMessaging, ediCommunicationsModeModule.Name);
				AssertModuleAdded("Module should be loaded if EAdaptorNextFeature is active.", true, ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.EDIMessaging, ediCommunicationParty.Name);
			}
		}

		public void TestProductivityWiseCompleteModuleTree_ShouldContainNonFreightForwardingModules_AndThoseRequiredToSupportProductivityModules()
		{
			System.Environment.SetEnvironmentVariable("EXTREQ_Enabled", "TRUE");

			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;

			Loader.LoadModules();

			var treeInTextFormat = GetModuleTreeInTextFormat();

			#region AssertProductivityWiseCompleteModuleTree

			AssertMultilineASCIIEquals("This defines the list of modules allowed for clients who are not freight forwarding or adjacent businesses. Only modules which are or support productivity tools should be included in this list. If in doubt, check with the PAVE team.",
$@"Category: Jump
	Section: Favorites

	Section: Recent Items

	Section: Recent Modules


Category: Operate
	Section: Productivity Tools
		Work Items
		Projects
		Customer Service Tickets


Category: Manage
	Section: Client Relationship Management
		Dashboard
		Inquiry Manager
		Commission Management
		Communication Manager
		Client Intelligence
		Campaign Management
		Opportunity Management
		Competitor Intelligence
		News and Announcements
		Reports

	Section: Business Intelligence && Analytics
		BI Manager

	Section: Planning
		Network Diagrams
		Work Queues

	Section: Workflow && Process
		Task List
		Exceptions
		Reports
		Job Workflows
		Requests

	Section: DocManager
		Allocate eDocs
		Merge eDocs Databases
		eDocs Database Manager
		Create eDocs CD
		Reports

	Section: Receivables
		Receivables Transactions
		Payment Processing
		Match Transactions
		Claims and Queries
		Collection Calls
		Receivables Enquiries
		Credit Note Approval
		Credit Controlled Documents Approval
		Print Invoice
		Invoice Batch
		Statements
		Reports
		Collection Batch
		Collection Orders

	Section: Payables
		Payables Transactions
		Hot Check
		Payment Processing
		Match Transactions
		Claims and Queries
		Payables Enquiries
		Invoice Approval
		Intercompany Transaction Approval
		Transactions Pending Allocation
		CASS Cost File Import
		Incomplete Invoices
		Reports
		Transactions Pending Allocation Approval
		Purchase Orders
		Payment Batches

	Section: Cash Book
		Cashbook Transactions
		Deposit Batches
		Direct Debit Batches
		Bank Reconciliations
		Reports

	Section: Job Costing
		WIPs and Accruals
		Job Management
		Job Revenue Journals
		Reports

	Section: General Ledger
		Journals
		Period Management
		Import CSV Transactions
		Transaction Search
		Reports
		Journals Awaiting Approval

	Section: GL Consolidations
		Consolidation Groups

	Section: Budgets
		Budgets
		Reports


Category: Maintain
	Section: Master Data
		Organization
		Organization Contacts
		Reports

	Section: Reference Files
		Document Organization Registration Mapping
		Document Types
		Document Sources
		Reports

	Section: Sales && Marketing
		Sales Products
		Sales Teams

	Section: Buffer Management
		Buffer Management Systems
		Visual Boards
		Visual Board Slide Shows
		Reports
		Customized Visual Layouts
		Tag Groups
		Tag Rules
		Acceptability Bands
		Measurement of Employee Normalized Throughput
		Component Relationships

	Section: Locations
		Countries/Regions
		States
		Cities/Towns
		Post Codes
		Geography
		UNLOCO
		Time Zones
		Reports
		Holidays

	Section: Account
		Sales/Expense Groups
		Tax ID
		Invoice Tax Messages
		Charge Codes
		Global Charge Codes
		Tax Override Groups
		Intercompany Charge Code Mappings
		Organization Charge Code Mappings
		Apportionment Templates
		Bank Accounts
		Check Books
		GL Accounts
		GL Multi-Language Mapping
		Creditor Groups
		Debtor Groups
		Import Chart of Accounts
		Import Data
		Reports
		Job Billing Exchange Rate Configuration

	Section: People Operations
		Reports
		HR Campaign Management

	Section: Learning && Development

	Section: Recruiter
		Job Openings
		Job Applicant
		Job Application
		Job Role
		HR Emails

	Section: Workflow Manager
		Exception Types
		Workflow Templates
		Template Company Rules
		Events
		Add On Rules
		Field Change Events
		Request Types
		Request Template

	Section: EDI Messaging
		EDI Interchange
		EDI Message
		Purpose Code
		EDI Message Profile
		Additional Event Context
		Business Rule Engine

	Section: Archive Manager
		Archive Schedule
		Archived Records
		Reports

	Section: User Admin
		Active Users
		Branches
		Companies
		Departments
		Group
		Staff and Resources
		Resource Capabilities
		Dialog Default Options
		Reports

	Section: System
		Customer Service (Legacy)
		Universal Copy Schedules
		License Usage
		Registry
		Service Tasks
		Process Controllers
		Upgrades
		Update Notes Portal
		Local Languages
		Translation Feedback
		Resource Strings
	Section: Printing
		Print Jobs
		Print Queues
		Document Signing Job
	Section: Email
		Emails
		Email Templates
	Section: Reports
		Reports
		Report Statistics
		Report Management
		Scheduled Reports
		Error Reports
", treeInTextFormat);

			#endregion
		}

		public void TestProductivityWiseCompleteModuleTree_WhenBufferManagementDisabled_ShouldContainNonFreightForwardingModules_AndThoseRequiredToSupportProductivityModules_ButNoPaveModules()
		{
			System.Environment.SetEnvironmentVariable("EXTREQ_Enabled", "TRUE");

			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = false;

			Loader.LoadModules();

			var treeInTextFormat = GetModuleTreeInTextFormat();

			#region AssertProductivityWiseCompleteModuleTree

			AssertMultilineASCIIEquals("This defines the list of modules allowed for clients who are not freight forwarding or adjacent businesses. Only modules which are or support productivity tools should be included in this list. If in doubt, check with the PAVE team.",
@"Category: Jump
	Section: Favorites

	Section: Recent Items

	Section: Recent Modules


Category: Operate
	Section: Productivity Tools
		Work Items
		Projects
		Customer Service Tickets


Category: Manage
	Section: Client Relationship Management
		Dashboard
		Inquiry Manager
		Commission Management
		Communication Manager
		Client Intelligence
		Campaign Management
		Opportunity Management
		Competitor Intelligence
		News and Announcements
		Reports

	Section: Business Intelligence && Analytics
		BI Manager

	Section: Workflow && Process
		Task List
		Exceptions
		Reports
		Requests

	Section: DocManager
		Allocate eDocs
		Merge eDocs Databases
		eDocs Database Manager
		Create eDocs CD
		Reports

	Section: Receivables
		Receivables Transactions
		Payment Processing
		Match Transactions
		Claims and Queries
		Collection Calls
		Receivables Enquiries
		Credit Note Approval
		Credit Controlled Documents Approval
		Print Invoice
		Invoice Batch
		Statements
		Reports
		Collection Batch
		Collection Orders

	Section: Payables
		Payables Transactions
		Hot Check
		Payment Processing
		Match Transactions
		Claims and Queries
		Payables Enquiries
		Invoice Approval
		Intercompany Transaction Approval
		Transactions Pending Allocation
		CASS Cost File Import
		Incomplete Invoices
		Reports
		Transactions Pending Allocation Approval
		Purchase Orders
		Payment Batches

	Section: Cash Book
		Cashbook Transactions
		Deposit Batches
		Direct Debit Batches
		Bank Reconciliations
		Reports

	Section: Job Costing
		WIPs and Accruals
		Job Management
		Job Revenue Journals
		Reports

	Section: General Ledger
		Journals
		Period Management
		Import CSV Transactions
		Transaction Search
		Reports
		Journals Awaiting Approval

	Section: GL Consolidations
		Consolidation Groups

	Section: Budgets
		Budgets
		Reports


Category: Maintain
	Section: Master Data
		Organization
		Organization Contacts
		Reports

	Section: Reference Files
		Document Organization Registration Mapping
		Document Types
		Document Sources
		Reports

	Section: Sales && Marketing
		Sales Products
		Sales Teams

	Section: Locations
		Countries/Regions
		States
		Cities/Towns
		Post Codes
		Geography
		UNLOCO
		Time Zones
		Reports
		Holidays

	Section: Account
		Sales/Expense Groups
		Tax ID
		Invoice Tax Messages
		Charge Codes
		Global Charge Codes
		Tax Override Groups
		Intercompany Charge Code Mappings
		Organization Charge Code Mappings
		Apportionment Templates
		Bank Accounts
		Check Books
		GL Accounts
		GL Multi-Language Mapping
		Creditor Groups
		Debtor Groups
		Import Chart of Accounts
		Import Data
		Reports
		Job Billing Exchange Rate Configuration

	Section: People Operations
		Reports
		HR Campaign Management

	Section: Learning && Development

	Section: Recruiter
		Job Openings
		Job Applicant
		Job Application
		Job Role
		HR Emails

	Section: Workflow Manager
		Exception Types
		Workflow Templates
		Template Company Rules
		Events
		Add On Rules
		Field Change Events
		Request Types
		Request Template

	Section: EDI Messaging
		EDI Interchange
		EDI Message
		Purpose Code
		EDI Message Profile
		Additional Event Context
		Business Rule Engine

	Section: Archive Manager
		Archive Schedule
		Archived Records
		Reports

	Section: User Admin
		Active Users
		Branches
		Companies
		Departments
		Group
		Staff and Resources
		Resource Capabilities
		Dialog Default Options
		Reports

	Section: System
		Customer Service (Legacy)
		Universal Copy Schedules
		License Usage
		Registry
		Service Tasks
		Process Controllers
		Upgrades
		Update Notes Portal
		Local Languages
		Translation Feedback
		Resource Strings
	Section: Printing
		Print Jobs
		Print Queues
		Document Signing Job
	Section: Email
		Emails
		Email Templates
	Section: Reports
		Reports
		Report Statistics
		Report Management
		Scheduled Reports
		Error Reports
", treeInTextFormat);

			#endregion
		}

		public void TestNonProductivityWiseTree_ShouldContainProductivityToolsBranch()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = false;

			Loader.LoadModules();

			var treeInTextFormat = GetModuleTreeInTextFormat();

			AssertContains("We expect the non-ProductivityWise module tree to contain our ProductivityTools branch exactly as expected, but instead...",
				@"
	Section: Productivity Tools
		Work Items
		Projects
		Customer Service Tickets",
			treeInTextFormat);
		}

		public void TestCashBookTree_ShouldContainChequeModuleItemsWhenRegistryIsEnabled()
		{
			AccountingConfigurationRegistry.Instance.EnableChequeManagementFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Loader.LoadModules();

			var treeInTextFormat = GetModuleTreeInTextFormat();

			AssertContains("We expect the Cash Book module tree to contain Cheque Module items",
				@"
	Section: Cash Book
		Cashbook Transactions
		Deposit Batches
		Direct Debit Batches
		Bank Reconciliations
		Reports
		Cheques
		Cheque Transactions",
			treeInTextFormat);
		}

		public void TestNonProductivityWiseTree_ShouldContainProductivityToolsBranchWhichShouldAppearLastInOperateCategory()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = false;

			Loader.LoadModules();

			var treeInTextFormat = GetModuleTreeInTextFormat();

			AssertContains("We expect the non-ProductivityWise module tree to contain our ProductivityTools branch exactly as expected, but instead...",
				@"

	Section: Liner && Agency
		Customs Export Manifest
		Customs Import Manifest
		Bookings
		Bills of Lading
		Bill of Lading Containers
		Container Manager
		Container Movements
		Container Detention
		Voyage Accounting
		Sundry Charges
		Reports

	Section: Productivity Tools
		Work Items
		Projects
		Customer Service Tickets",
			treeInTextFormat);
		}

		public void TestModuleSectionsWithServiceCodes()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = false;
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;

			Loader.LoadModules();
			var treeInTextFormat = GetModuleSectionsWithServiceCodesInTextFormat();

			AssertEquals(
			@"Category: Jump
Section:  - Favorites
Section:  - Recent Items
Section:  - Recent Modules
Category: Operate
Section: SCH - Schedules
Section: FOR - Forwarding
Section: CUS - Customs
Section: CUS - Customs Global
Section: ORD - Order Manager
Section: TBK - Transport Booking
Section: LTN - Land Transport
Section: LOC - Port Transport
Section: CFS - CFS/CTO
Section: WAR - Product Warehouse
Section: TWH - Transit Warehouse
Section: SHM - Liner && Agency
Section: PRT - Productivity Tools
Category: Manage
Section: CRM - Client Relationship Management
Section: TAR - Tariffs && Rates
Section: WRS - Rates Service
Section: BNI - Business Intelligence && Analytics
Section: BUF - Planning
Section: WRK - Workflow && Process
Section: DCM - DocManager
Section: RCB - Receivables
Section: PAY - Payables
Section: CBK - Cash Book
Section: JCT - Job Costing
Section: GLG - General Ledger
Section: GLC - GL Consolidations
Section: BUG - Budgets
Category: Maintain
Section: MDM - Master Data
Section: REF - Reference Files
Section: SAL - Sales && Marketing
Section: TAR - Tariffs && Rates
Section: BUF - Buffer Management
Section: LCT - Locations
Section: ACC - Account
Section: CUS - Customs Files
Section: CUS - Customs (US)
Section: WAR - Warehouse
Section: HRM - People Operations
Section: LAD - Learning && Development
Section: REC - Recruiter
Section: PRO - Workflow Manager
Section: EDI - EDI Messaging
Section: ARM - Archive Manager
Section: USA - User Admin
Section: SYS - System
Section: PTS - Printing
Section: EMS - Email
Section: RPS - Reports
",
			treeInTextFormat);

			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			Loader.LoadModules();
			treeInTextFormat = GetModuleSectionsWithServiceCodesInTextFormat();

			AssertMultilineASCIIEquals(
			@"Category: Jump
Section:  - Favorites
Section:  - Recent Items
Section:  - Recent Modules
Category: Operate
Section: PRT - Productivity Tools
Category: Manage
Section: CRM - Client Relationship Management
Section: BNI - Business Intelligence && Analytics
Section: BUF - Planning
Section: WRK - Workflow && Process
Section: DCM - DocManager
Section: RCB - Receivables
Section: PAY - Payables
Section: CBK - Cash Book
Section: JCT - Job Costing
Section: GLG - General Ledger
Section: GLC - GL Consolidations
Section: BUG - Budgets
Category: Maintain
Section: MDM - Master Data
Section: REF - Reference Files
Section: SAL - Sales && Marketing
Section: BUF - Buffer Management
Section: LCT - Locations
Section: ACC - Account
Section: HRM - People Operations
Section: LAD - Learning && Development
Section: REC - Recruiter
Section: PRO - Workflow Manager
Section: EDI - EDI Messaging
Section: ARM - Archive Manager
Section: USA - User Admin
Section: SYS - System
Section: PTS - Printing
Section: EMS - Email
Section: RPS - Reports
",
			treeInTextFormat);
		}

		string GetModuleTreeInTextFormat()
		{
			var treeBuilder = new StringBuilder();

			foreach (ModuleCategory category in Tree.Categories.Values)
			{
				_ = treeBuilder.Append("Category: ");
				_ = treeBuilder.AppendLine(category.DisplayText);

				foreach (ModuleSection section in category.Sections.Values)
				{
					_ = treeBuilder.Append("\tSection: ");
					_ = treeBuilder.AppendLine(section.DisplayText);

					foreach (IMainFormModule module in section.Modules.Values)
					{
						_ = treeBuilder.Append("\t\t");
						_ = treeBuilder.AppendLine(module.Description);
					}

					_ = treeBuilder.AppendLine();
				}

				_ = treeBuilder.AppendLine();
			}

			return treeBuilder.ToString();
		}

		string GetModuleSectionsWithServiceCodesInTextFormat()
		{
			var treeBuilder = new StringBuilder();

			foreach (ModuleCategory category in Tree.Categories.Values)
			{
				_ = treeBuilder.AppendLine($"Category: {category.DisplayText}");

				foreach (ModuleSection section in category.Sections.Values)
				{
					_ = treeBuilder.AppendLine($"Section: {section.CustomerServiceMenuSectionCode} - {section.DisplayText}");
				}
			}

			return treeBuilder.ToString();
		}

		#endregion

		#region ComponentRelationshipEnabled

		public void TestComponentRelationshipEnabledModuleTree()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			Loader.LoadModules();
			AssertEquals(
				"WHEN BufferManagementEnabled=true THEN should show",
				ModuleTreeLoaderConstant.Section.BufferManagementConfig.Name,
				Tree.FindByID(ModuleIDs.ComponentRelationship.ToString()).ParentSection.Name);

			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = false;
			Loader.LoadModules();
			AssertNull(
				"WHEN BufferManagementEnabled=false THEN should not show",
				Tree.FindByID(ModuleIDs.ComponentRelationship.ToString()));
		}

		#endregion

		#region Dummy

		public void TestDummyModuleListingSubsetRegistration()
		{
			DummyModuleListingSubset.InitializeDummyModule.Value = true;
			Loader.LoadModules();
			var module = Tree.FindByID(DummyModuleIDs.Dummy.ToString());
			AssertNotNull(module);
			CombineAssertions(() =>
			{
				AssertEquals("Dummy", module.ID);
				AssertEquals("Dummy", module.ParentSection.Name);
			});
		}

		#endregion

		#region Implementation

		void AssertAllSectionsHaveSubcategories(string countryCode)
		{
			GlbCompany.CurrentCompany.SetCountry(countryCode);
			Loader.LoadModules();

			foreach (ModuleCategory category in Tree.Categories.Values)
			{
				if (category.Name != ModuleTreeLoaderConstant.Category.Jump.Name)
				{
					foreach (ModuleSection section in category.Sections.Values)
					{
						AssertNotNull(string.Format("Subcategory must be set {0} ({1})", section.Name, countryCode), section.Subcategory);
					}
				}
			}
		}

		static void AssertModuleCategorySections(ModuleTree tree, string categoryName, string[] expectedSectionNames)
		{
			var category = tree.Categories[categoryName];
			var sectionNames = category.Sections.Values.Cast<ModuleSection>().Select(s => s.Name).ToArray();

			AssertSequencesEqual(categoryName + " category sections", expectedSectionNames, sectionNames);
		}

		void AssertNctsModuleVisibility(string country, bool expectedVisibility)
		{
			Loader.LoadModules();
			_ = AssertModuleAdded("NCTS movement module visibility for " + country, expectedVisibility, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.EuNcts, ModuleIDs.Customs.EU.NctsMovementModule.Name);
			_ = AssertModuleAdded("NCTS reports visibility for " + country, expectedVisibility, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.EuNcts, ModuleIDs.Customs.EU.NctsReportsModule.Name);
		}

		#endregion

		#region GlbPerson

		public void TestGlbPersonPanel()
		{
			Loader.LoadModules();
			AssertNull(Tree.FindByID(ModuleIDs.GlbPerson.ToString()));

			SystemDataRegistry.Instance.PersonIntelligenceModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Loader.LoadModules();
			AssertNotNull(Tree.FindByID(ModuleIDs.GlbPerson.ToString()));
		}

		#endregion
	}
}
