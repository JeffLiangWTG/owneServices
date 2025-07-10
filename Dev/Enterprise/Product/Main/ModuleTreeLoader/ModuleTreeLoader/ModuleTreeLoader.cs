using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core.Environment;
using Enterprise.Core.Modules;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.Recruitment.Registry;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs.Manifest;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ResourceStrings.Business;
using Enterprise.Security;
using Enterprise.TransportCommon.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.ZArchitecture.Modules.ModuleTreeLoaderHelpers;
using Constants = Enterprise.Core.Constants;
using GlowRegistry = Enterprise.Registry.Business.GlowRegistry;

namespace Enterprise.Main.ModuleTreeLoader
{
	public class ModuleTreeLoader : IModuleTreeLoader
	{
		#region Initialisation

		public void Initialise(ModuleTree treeToLoad, IZSecurity securityInstance)
		{
			Initialise(treeToLoad, securityInstance, null);
		}

		public void Initialise(ModuleTree treeToLoad, IZSecurity securityInstance, ClientHook clientHook)
		{
			this.TreeToLoad = treeToLoad;
			this.SecurityInstance = (SecurityCore)securityInstance;
			this.clientHook = clientHook;
		}

		SecurityCore SecurityInstance;
		ModuleTree TreeToLoad;

		ClientHook clientHook;
		ClientHook ClientHook
		{
			get { return clientHook ?? ClientHookLoader.Instance.ClientHook; }
		}

		public void LoadModules()
		{
			ModuleCategory jumpCategory = new ModuleCategory(ModuleTreeLoaderConstant.Category.Jump, null);
			ModuleCategory operateCategory = new ModuleCategory(ModuleTreeLoaderConstant.Category.Operations, SecurityInstance.Operations);
			ModuleCategory manageCategory = new ModuleCategory(ModuleTreeLoaderConstant.Category.Manage, SecurityInstance.Manage);
			ModuleCategory maintainCategory = new ModuleCategory(ModuleTreeLoaderConstant.Category.Admin, SecurityInstance.Maintain);

			TreeToLoad.Categories.Clear();
			TreeToLoad.Categories.Add(jumpCategory);
			TreeToLoad.Categories.Add(operateCategory);
			TreeToLoad.Categories.Add(manageCategory);
			TreeToLoad.Categories.Add(maintainCategory);

			if (ClientHook != null && ClientHook.AddNewModuleSectionsAtTopOfTree)
			{
				LoadClientSections();
			}

			InitialiseOperateCategory(operateCategory);
			InitialiseManageCategory(manageCategory);
			InitialiseMaintainCategory(maintainCategory);

			if (ClientHook != null)
			{
				if (!ClientHook.AddNewModuleSectionsAtTopOfTree)
				{
					LoadClientSections();
				}
				LoadClientModules();
			}

			InitialiseJumpCategory(jumpCategory);
			ModuleListingSubsetRegister.InitializeModuleTree(new(jumpCategory, operateCategory, manageCategory, maintainCategory), SecurityInstance);
		}

		#endregion

		#region Modules

		#region Client Specific

		void LoadClientSections()
		{
			IModuleSectionAddOn[] newSections = ClientHook.NewModuleSectionsToAddForClient;
			if (newSections != null)
			{
				foreach (ModuleSectionAddOn sectionAddOn in newSections)
				{
					ModuleCategory category = TreeToLoad.Categories[sectionAddOn.CategoryName];
					if (category != null)
					{
						category.Sections.Add(sectionAddOn);
					}
				}
			}
		}

		void LoadClientModules()
		{
			NewClientModuleInfo[] newModules = ClientHook.NewClientModules;
			if (newModules != null)
			{
				foreach (NewClientModuleInfo clientModuleInfo in newModules)
				{
					bool includeInMainMenu = !string.IsNullOrEmpty(clientModuleInfo.CategoryName) && !string.IsNullOrEmpty(clientModuleInfo.SectionName);
					if (includeInMainMenu)
					{
						ModuleCategory category = TreeToLoad.Categories[clientModuleInfo.CategoryName];
						if (category != null)
						{
							ModuleSection section = category.Sections[clientModuleInfo.SectionName];
							if (section != null)
							{
								AddClientSpecificModule(section, clientModuleInfo.ID);
							}
						}
					}
				}
			}
		}

		#endregion

		#region Standard Modules

		#region Jump

		void InitialiseJumpCategory(ModuleCategory jumpCategory)
		{
			var favoriteSection = new ModuleSection(ModuleTreeLoaderConstant.Section.Favorites, null, null, IconTypes.Phone, IconTypes.Phone20x16);
			jumpCategory.Sections.Add(favoriteSection);
			InitializeFavoriteSection(favoriteSection);

			var recentItemsSection = new ModuleSection(ModuleTreeLoaderConstant.Section.RecentItems, null, null, IconTypes.Phone, IconTypes.Phone20x16);
			jumpCategory.Sections.Add(recentItemsSection);
			InitializeRecentItemsSection(recentItemsSection);

			var recentModulesSection = new ModuleSection(ModuleTreeLoaderConstant.Section.RecentModules, null, null, IconTypes.Phone, IconTypes.Phone20x16);
			jumpCategory.Sections.Add(recentModulesSection);
			InitializeRecentModulesSection(recentModulesSection);
		}

		void InitializeFavoriteSection(ModuleSection favoriteSection)
		{
			var modules = RecentItemManager.Instance.FavoriteModules;
			if (modules != null)
			{
				foreach (var module in modules.ToArray())
				{
					var removeModule = false;
					var shortcut = new LinkWrapper(module);
					var shortcutIsModule = shortcut.IsModule;

					var moduleId = FindModuleIdentifierInTreeOrNonExposedModules(shortcutIsModule ? shortcut.ModuleName : module.STL_ModuleID.ToString());
					if (moduleId != null)
					{
						try
						{
							AddModule(favoriteSection, moduleId, m =>
							{
								if (!shortcutIsModule)
								{
									m.RecordUrl = shortcut.RecordUrl;
									m.RecordDescription = shortcut.RecordDescription;
									m.RecordKey = shortcut.RecordKey;
								}
							});
						}
						catch (ModuleAlreadyExistsException)
						{
							removeModule = true;
						}
					}
					else
					{
						removeModule = true;
					}

					if (removeModule)
					{
						RecentItemManager.Instance.RemoveFromFavoriteModules(shortcut);
					}
				}
			}
		}

		void InitializeRecentItemsSection(ModuleSection recentSection)
		{
			var modules = RecentItemManager.Instance.GetRecentItems(string.Empty);
			if (modules != null)
			{
				foreach (var module in modules.ToArray())
				{
					var shortcut = new LinkWrapper(module);

					var moduleId = FindModuleIdentifierInTreeOrNonExposedModules(shortcut.ModuleName);
					if (moduleId != null)
					{
						try
						{
							AddModule(recentSection, moduleId, m =>
							{
								m.RecordUrl = shortcut.RecordUrl;
								m.RecordDescription = shortcut.RecordDescription;
								m.RecordKey = shortcut.RecordKey;
							});
						}
						catch (ModuleAlreadyExistsException)
						{
							RecentItemManager.Instance.RemoveFromRecentItems(string.Empty, shortcut);
						}
					}
				}
			}
		}

		void InitializeRecentModulesSection(ModuleSection recentSection)
		{
			var modules = RecentItemManager.Instance.RecentModules;
			if (modules != null)
			{
				foreach (var module in modules.ToArray())
				{
					var mainFormModule = TreeToLoad.FindByID(module.STL_ModuleID);
					if (mainFormModule != null)
					{
						_ = mainFormModule.SecurityCheckpoint;
						try
						{
							AddModule(recentSection, mainFormModule.ModuleID);
						}
						catch (ModuleAlreadyExistsException)
						{
							RecentItemManager.Instance.RemoveFromRecentModules(new LinkWrapper(module));
						}
					}
				}
			}
		}

		ModuleIdentifier FindModuleIdentifierInTreeOrNonExposedModules(string moduleID)
		{
			var namedModule = TreeToLoad.FindByID(moduleID);
			if (namedModule != null)
			{
				_ = namedModule.SecurityCheckpoint;
				return namedModule.ModuleID;
			}
			else
			{
				return ValidModulesNotInTree.FirstOrDefault(id => id.Name == moduleID);
			}
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

		#endregion

		#region Operate

		#region Initialisation

		void InitialiseOperateCategory(ModuleCategory operationsCategory)
		{
			operationsCategory.Sections.AddIf(!ProductivityWiseModeEnabled, () =>
			{
				var section = new ModuleSection(ModuleTreeLoaderConstant.Section.Schedules, ModuleTreeCustomerServiceMenuSectionList.Codes.Schedules, SecurityInstance.Schedules, IconTypes.Phone, IconTypes.Phone20x16, ModuleTreeLoaderConstant.Subcategory.Schedules);
				InitialiseSchedulesSection(section);

				return section;
			});

			operationsCategory.Sections.AddIf(!ProductivityWiseModeEnabled, () =>
			{
				var section = new ModuleSection(ModuleTreeLoaderConstant.Section.Forwarding, ModuleTreeCustomerServiceMenuSectionList.Codes.Forwarding, SecurityInstance.Forwarding, IconTypes.Box, IconTypes.Box20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);
				InitialiseForwardingSection(section);

				return section;
			});

			AddCcsukIfInUnitedKingdom(operationsCategory);
			AddNctsIfInContractingCountry(operationsCategory);

			operationsCategory.Sections.AddIf(!ProductivityWiseModeEnabled, () =>
			{
				var section = new ModuleSection(ModuleTreeLoaderConstant.Section.CustomsMain, ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, SecurityInstance.CustomsMain, IconTypes.Customs, IconTypes.Customs20x16, ModuleTreeLoaderConstant.Subcategory.Customs);
				InitialiseCustomsMain(section);

				return section;
			});

			operationsCategory.Sections.AddIf(!ProductivityWiseModeEnabled, () =>
			{
				var section = new ModuleSection(ModuleTreeLoaderConstant.Section.CustomsGlobal, ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, SecurityInstance.CustomsGlobal, IconTypes.Customs, IconTypes.Customs20x16, ModuleTreeLoaderConstant.Subcategory.Customs);
				InitialiseCustomsGlobal(section);

				return section;
			});

			operationsCategory.Sections.AddIf(!ProductivityWiseModeEnabled, () =>
			{
				var section = new ModuleSection(ModuleTreeLoaderConstant.Section.OrderManager, ModuleTreeCustomerServiceMenuSectionList.Codes.OrderManager, SecurityInstance.OrderManager, IconTypes.Paper, IconTypes.Paper20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);
				InitialiseOrderMgtSection(section);

				return section;
			});

			operationsCategory.Sections.AddIf(!ProductivityWiseModeEnabled, () =>
			{
				var section = new ModuleSection(ModuleTreeLoaderConstant.Section.TransportBooking, ModuleTreeCustomerServiceMenuSectionList.Codes.TransportBooking, SecurityInstance.DtbTransportBookingsOperations, IconTypes.Phone, IconTypes.Phone20x16, ModuleTreeLoaderConstant.Subcategory.Transport);
				InitialiseTransportBookingSection(section);

				return section;
			});

			operationsCategory.Sections.AddIf(!ProductivityWiseModeEnabled, () =>
			{
				var transportConsignmentSection = new ModuleSection(ModuleTreeLoaderConstant.Section.TransportConsignment, ModuleTreeCustomerServiceMenuSectionList.Codes.LandTransport, SecurityInstance.DtbLandTransportOperations, IconTypes.Phone, IconTypes.Phone20x16, ModuleTreeLoaderConstant.Subcategory.Transport);
				InitialiseTransportConsignmentSection(transportConsignmentSection);
				return transportConsignmentSection;
			});

			operationsCategory.Sections.AddIf(!ProductivityWiseModeEnabled, () =>
			{
				var section = new ModuleSection(ModuleTreeLoaderConstant.Section.PortTransport, ModuleTreeCustomerServiceMenuSectionList.Codes.LocalTransport, SecurityInstance.Transport, IconTypes.Truck, IconTypes.Truck20x16, ModuleTreeLoaderConstant.Subcategory.Transport);
				InitialiseTransportSection(section);

				return section;
			});

			operationsCategory.Sections.AddIf(!ProductivityWiseModeEnabled, () =>
			{
				var section = new ModuleSection(ModuleTreeLoaderConstant.Section.CFSCTO, ModuleTreeCustomerServiceMenuSectionList.Codes.CfsCto, SecurityInstance.CFSCTO, IconTypes.CFS, IconTypes.CFS20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);
				InitialiseCFSCTOSection(section);

				return section;
			});

			operationsCategory.Sections.AddIf(!ProductivityWiseModeEnabled, () =>
			{
				var section = new ModuleSection(ModuleTreeLoaderConstant.Section.Warehouse, ModuleTreeCustomerServiceMenuSectionList.Codes.Warehouse, SecurityInstance.Warehouse, IconTypes.Forklift, IconTypes.Forklift20x16, ModuleTreeLoaderConstant.Subcategory.Warehouse);
				InitialiseWarehouseSection(section);

				return section;
			});

			operationsCategory.Sections.AddIf(!ProductivityWiseModeEnabled, () =>
			{
				var section = new ModuleSection(ModuleTreeLoaderConstant.Section.TransitWarehouse, ModuleTreeCustomerServiceMenuSectionList.Codes.TransitWarehouse, SecurityInstance.TransitWarehouse, IconTypes.Forklift, IconTypes.Forklift20x16, ModuleTreeLoaderConstant.Subcategory.Warehouse);
				InitialiseTransitWarehouseSection(section);

				return section;
			});

			operationsCategory.Sections.AddIf(!ProductivityWiseModeEnabled && WarehouseDataRegistry.Instance.EnableContainerYard.Value, () =>
			{
				var section = new ModuleSection(ModuleTreeLoaderConstant.Section.ContainerYard, ModuleTreeCustomerServiceMenuSectionList.Codes.ContainerYard, SecurityInstance.ContainerYard, IconTypes.Container, IconTypes.Container20x16, ModuleTreeLoaderConstant.Subcategory.Warehouse);
				InitialiseContainerYardSection(section);

				return section;
			});

			operationsCategory.Sections.AddIf(!ProductivityWiseModeEnabled && WarehouseDataRegistry.Instance.EnableGateManagement.Value, () =>
			{
				var section = new ModuleSection(ModuleTreeLoaderConstant.Section.GateManagement, ModuleTreeCustomerServiceMenuSectionList.Codes.GateManagement, SecurityInstance.GateManagement, IconTypes.Truck, IconTypes.Truck20x16, ModuleTreeLoaderConstant.Subcategory.Warehouse);
				InitialiseGateManagementSection(section);

				return section;
			});

			operationsCategory.Sections.AddIf(!ProductivityWiseModeEnabled, () =>
			{
				var section = new ModuleSection(ModuleTreeLoaderConstant.Section.LinerAndAgency, ModuleTreeCustomerServiceMenuSectionList.Codes.LinerAndAgency, SecurityInstance.LinerAndAgency, IconTypes.Ship, IconTypes.Ship20x16, ModuleTreeLoaderConstant.Subcategory.LinerAndAgency);
				InitialiseShippingSection(section);

				return section;
			});

			operationsCategory.Sections.AddIf(!ProductivityWiseModeEnabled && OceanCarrierDataRegistry.Instance.EnableOceanCarrierSolution.Value, () =>
			{
				var section = new ModuleSection(ModuleTreeLoaderConstant.Section.OceanCarrier, ModuleTreeCustomerServiceMenuSectionList.Codes.OceanCarrier, SecurityInstance.LinerAndAgency, IconTypes.Ship, IconTypes.Ship20x16, ModuleTreeLoaderConstant.Subcategory.OceanCarrier);
				AddModule(section, ModuleIDs.OceanCarrierPortal);
				AddModule(section, ModuleIDs.CarrierServices);
				AddModule(section, ModuleIDs.CarrierShipmentHeader);

				return section;
			});

			operationsCategory.Sections.AddIf(!ProductivityWiseModeEnabled && OceanCarrierDataRegistry.Instance.EnableOceanCarrierSolution.Value, () =>
			{
				var section = new ModuleSection(ModuleTreeLoaderConstant.Section.EquipmentManagement, ModuleTreeCustomerServiceMenuSectionList.Codes.EquipmentManagement, SecurityInstance.LinerAndAgency, IconTypes.Container, IconTypes.Container20x16, ModuleTreeLoaderConstant.Subcategory.OceanCarrier);
				AddModule(section, ModuleIDs.EquipmentManagementPortal);

				return section;
			});

			AddStampDutyForTurkey(operationsCategory);

			var productivityToolsSection = new ModuleSection(ModuleTreeLoaderConstant.Section.ProductivitySection, ModuleTreeCustomerServiceMenuSectionList.Codes.ProductivityTools, SecurityInstance.ProductivityTools, IconTypes.Processes, IconTypes.Processes20x16, ModuleTreeLoaderConstant.Subcategory.ProductivityTools);

			AddProductivityToolsModules(productivityToolsSection);

			operationsCategory.Sections.Add(productivityToolsSection);
		}

		#endregion

		#region Bookings

		void InitialiseSchedulesSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.JobSeaSailing);
			AddModule(section, ModuleIDs.JobAirSailing);
			AddModule(section, ModuleIDs.JobRailSailing);
			AddModule(section, ModuleIDs.JobRoadSailing);
			AddModule(section, ModuleIDs.SailingDataVendorImporting);

			if (FreightDataRegistry.Instance.EnableScheduleFeedService.Value)
			{
				AddModule(section, ModuleIDs.OnlineSailingSchedules);
			}

			AddModule(section, ModuleIDs.RoutingLookups);
			AddModule(section, ModuleIDs.BookingsReports);
		}

		#endregion

		#region Forwarding

		void InitialiseForwardingSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.QuotedBookings);
			AddModuleIf(ObjectFactory.Get<IContractPermissions>().IsCarrierAndClientContractModulesEnabled(), section, ModuleIDs.CarrierContractAndAllocations);
			AddModuleIf(ObjectFactory.Get<IContractPermissions>().IsCarrierAndClientContractModulesEnabled(), section, ModuleIDs.ClientContractAndAllocations);
			AddModule(section, ModuleIDs.JobShipment);
			AddModule(section, ModuleIDs.JobConsol);
			AddModule(section, ModuleIDs.ConsolPlanningBoard);

			AddModuleIf(CurrentCountry == Constants.CountryCodes.China, section,
				ModuleIDs.DocumentTracking);

			AddModule(section, ModuleIDs.Containers);
			AddModule(section, ModuleIDs.ConsolidatedTransportBooking);
			AddModule(section, ModuleIDs.OceanCarrierBookingAnalysisReport);

			AddModuleIf(ObjectFactory.Get<IMarketIntelligenceAndAnalyticsFeatureControlHelper>().Enabled, section, ModuleIDs.MarketIntelligenceAndAnalytics);

			AddModule(section, ModuleIDs.HVLVBookingHeader);
			AddModule(section, ModuleIDs.HVLVConsignment);
			AddModuleIf(HVLVDataRegistry.Instance.HVLVOriginLoadListTestingMode.Value, section, ModuleIDs.HVLVOriginLoadList);

			AddModule(section, ModuleIDs.ForwardingReport);
			AddModuleIf(ObjectFactory.Get<ICO2eFeatureControlHelper>().DashboardEnabled, section, ModuleIDs.CO2eDashboard);
		}

		#endregion

		#region Customs

		void InitialiseCustomsGlobal(ModuleSection section)
		{
			AddModule(section, ModuleIDs.Customs.CA.CAHouseBilleManifest);
			AddModule(section, ModuleIDs.Customs.ASYCUDA.Manifest);
			AddModule(section, ModuleIDs.Customs.ASYCUDA.ManifestBill);
			AddModule(section, ModuleIDs.Customs.JP.AFR);
			AddModule(section, ModuleIDs.Customs.JP.AFRBill);
			AddModuleIf(CurrentCountry == Constants.CountryCodes.Canada || CurrentCountry == Constants.CountryCodes.Mexico, section,
				ModuleIDs.Customs.US.eManifestIntl);

			if (CurrentCountry == Constants.CountryCodes.Singapore && IsAccessEnable)
			{
				AddModule(section, ModuleIDs.Customs.ASYCUDA.SGAccess.Manifest);
				AddModule(section, ModuleIDs.Customs.ASYCUDA.SGAccess.ManifestBill);
			}

			AddModule(section, ModuleIDs.Customs.US.AMS);
			AddModule(section, ModuleIDs.Customs.US.AMSBill);
			AddModule(section, ModuleIDs.ImporterSecurityFiling);
			AddModule(section, ModuleIDs.CustomsGlobalReport);
		}

		bool IsAccessEnable => (bool)ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable.Value;

		bool IsCurrentCountryUnderUSCustomsOfJurisdiction => !string.IsNullOrWhiteSpace(CurrentCountry) && Constants.CountryCodes.GetCustomsCountryOfJurisdiction(CurrentCountry) == Constants.CountryCodes.UnitedStates;

		bool IsTemporaryStorageEnabled => ObjectFactory.Get<Integration.Customs.Shared.ITemporaryStorageSettings>().IsUsingUCC5;

		bool IsTemporaryStorageRegisterEnabled => ObjectFactory.Get<Integration.Customs.Shared.ITemporaryStorageSettings>().IsUsingTSRegister;

		bool IsShowEntryModule =>
			CurrentCountry == Constants.CountryCodes.SouthAfrica ||
			CurrentCountry == Constants.CountryCodes.China ||
			CurrentCountry == Constants.CountryCodes.UnitedKingdom ||
			CurrentCountry == Constants.CountryCodes.Japan ||
			CurrentCountry == Constants.CountryCodes.Switzerland ||
			ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnion(CurrentCountry) ||
			ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().IsAsycudaCustomsCountry(CurrentCountry);

		bool IsShowCusPackingListModule => (bool)ObjectFactory.Get<Integration.Customs.TW.ITWCustomsRegistry>().CustomsPackingListEnable.Value;

		bool IsShowBriefCustomsDeclarationsModule => (bool)ObjectFactory.Get<Integration.Customs.TW.ITWCustomsRegistry>().EnableBriefCustomsDeclaration.Value;

		void InitialiseCustomsMain(ModuleSection section)
		{
			AddModule(section, ModuleIDs.Customs.JobDeclaration);
			AddModule(section, ModuleIDs.CommercialInvoice);
			AddModulesIf(IsShowEntryModule, section, ModuleIDs.Customs.EntryHeader);
			AddModuleIf(RawDataRegistry.Instance.EnableConsolidatedEntries.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty), section, ModuleIDs.Customs.ConsolidatedDeclaration);

			var isCurrentCountryUnderUSCustomsOfJurisdiction = IsCurrentCountryUnderUSCustomsOfJurisdiction;

			AddModulesIf(isCurrentCountryUnderUSCustomsOfJurisdiction, section,
				ModuleIDs.Customs.US.eManifest,
				ModuleIDs.Customs.US.InBond,
				ModuleIDs.Customs.US.InBondMoveHeader);

			AddModuleIf(CurrentCountry != Constants.CountryCodes.UnitedKingdom && ManifestCustomsDataRegistry.Instance.PreBoardingNotificationManifestEnabled.Value, section, ModuleIDs.Customs.ASYCUDA.PreBoardingNotification);

			#region US

			AddModule(section, ModuleIDs.Customs.US.Protest);
			AddModule(section, ModuleIDs.Customs.US.Reconciliation);
			AddModule(section, ModuleIDs.Customs.US.AMSBrokerDownloadMessage);
			AddModule(section, ModuleIDs.Customs.US.USCustomsStatement);
			AddModule(section, ModuleIDs.Customs.US.BorderLineReleaseMessage);
			AddModule(section, ModuleIDs.Customs.US.QueryMessage);
			AddModule(section, ModuleIDs.Customs.US.CourtesyNoticesOfLiquidation);
			AddModule(section, ModuleIDs.Customs.US.Drawback);
			AddModuleIf(
				isCurrentCountryUnderUSCustomsOfJurisdiction,
				section,
				ModuleIDs.Customs.US.USLowValueEntries);
			AddModuleIf(
				isCurrentCountryUnderUSCustomsOfJurisdiction,
				section,
				ModuleIDs.Customs.US.USLowValueEntriesBill);

			#endregion

			#region NZ

			AddModule(section, ModuleIDs.Customs.NZ.CUSCAR);
			AddModule(section, ModuleIDs.Customs.NZ.ECIWriteOffManifesting);
			AddModule(section, ModuleIDs.Customs.NZ.ExpressECI);
			AddModule(section, ModuleIDs.Customs.NZ.SeaCargoICR);

			#endregion

			#region AU

			AddModule(section, ModuleIDs.Customs.AU.AirCargo);
			AddModule(section, ModuleIDs.Customs.AU.HouseAirCargo);
			AddModule(section, ModuleIDs.Customs.AU.SeaCargo);
			AddModuleIf(CurrentCountry == Constants.CountryCodes.Australia && ObjectFactory.Get<Integration.Customs.Shared.ICustomsDataRegistry>().IsAUSeaCargoHouseEnabled
				, section, ModuleIDs.Customs.AU.HouseSeaCargo);
			AddModuleIf(CurrentCountry == Constants.CountryCodes.Australia, section, ModuleIDs.Customs.AU.NexDocNotifications);

			#endregion

			#region CA

			AddModule(section, ModuleIDs.Customs.CA.CAQueryMessages);
			AddModule(section, ModuleIDs.Customs.CA.CAReleaseNotifications);
			AddModule(section, ModuleIDs.Customs.CA.K84Reports);
			AddModule(section, ModuleIDs.Customs.CA.CADailyNoticeReconciliation);
			AddModule(section, ModuleIDs.Customs.CA.CAARLStatementOfAccount);
			AddModule(section, ModuleIDs.Customs.CA.B2Adjustments);
			AddModule(section, ModuleIDs.Customs.CA.CAManifestForward);
			AddModule(section, ModuleIDs.Customs.CA.CALVXJobs);
			AddModule(section, ModuleIDs.Customs.CA.CACSARevenueSummaryForm);

			#endregion

			#region EU

			var isCurrentCountryUnderJusrisdictionOfAnEUMember = ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsMemberOfEU(Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(CurrentCountry));
			var isUserOfEMCSH7 = isCurrentCountryUnderJusrisdictionOfAnEUMember || CurrentCountry == Constants.CountryCodes.UnitedKingdom;
			AddModuleIf(isUserOfEMCSH7 && (bool)ObjectFactory.Get<Integration.Customs.EUEMCS.IEmcsCustomsDataRegistry>().EnableEmcsFunctions.Value, section, ModuleIDs.Customs.EU.EMCS);
			AddModuleIf(isCurrentCountryUnderJusrisdictionOfAnEUMember && ObjectFactory.Get<Integration.Customs.EUExitControl.IExitControlCustomsDataRegistry>().IsExitControlModuleEnabledForCurrentCompany, section, ModuleIDs.Customs.EU.ExitControl);
			AddModuleIf(isCurrentCountryUnderJusrisdictionOfAnEUMember && ObjectFactory.Get<Integration.Customs.EUExitControl.IExitControlCustomsDataRegistry>().IsExitControlModuleEnabledForCurrentCompany, section, ModuleIDs.Customs.EU.ExitControlReport);
			AddModuleIf(isCurrentCountryUnderJusrisdictionOfAnEUMember && ObjectFactory.Get<Integration.Customs.Shared.ITemporaryStorageSettings>().IsUsingUCC6, section, ModuleIDs.Customs.EU.UCC6TemporaryStorage);
			AddModuleIf(isUserOfEMCSH7 && ObjectFactory.Get<Integration.Customs.EUH7.IH7FeatureControlProvider>().IsAuthorized(CurrentCountry, GlbCompany.CurrentCompany.GC_Code), section, ModuleIDs.Customs.EU.EUH7);
			AddModuleIf(isUserOfEMCSH7 && ObjectFactory.Get<Integration.Customs.EUH7.IH7FeatureControlProvider>().IsAuthorized(CurrentCountry, GlbCompany.CurrentCompany.GC_Code), section, ModuleIDs.Customs.EU.EUH7Bill);
			AddModuleIf(IsTemporaryStorageEnabled, section, ModuleIDs.Customs.TemporaryStorage);
			AddModuleIf(isCurrentCountryUnderJusrisdictionOfAnEUMember && ObjectFactory.Get<Integration.Customs.EU.IEUIntrastatCustomsRegistry>().IsIntrastatEnabled, section, ModuleIDs.Customs.EU.IntrastatReports);
			AddModuleIf(isCurrentCountryUnderJusrisdictionOfAnEUMember && ObjectFactory.Get<Integration.Customs.EU.IEUIntrastatCustomsRegistry>().IsIntrastatEnabled, section, ModuleIDs.Customs.EU.IntrastatTransactions);

			#endregion

			#region DE

			AddModuleIf(CurrentCountry == Constants.CountryCodes.Germany, section, ModuleIDs.Customs.EU.DE.ExportStatusRequest);
			AddModuleIf(CurrentCountry == Constants.CountryCodes.Germany && IsDEMonthlyClosingEnabled, section, ModuleIDs.Customs.EU.DE.MonthlyClosing);
			AddModuleIf(CurrentCountry == Constants.CountryCodes.Germany, section, ModuleIDs.Customs.EU.DE.TaxChangeAssessment);
			AddModuleIf(CurrentCountry == Constants.CountryCodes.Germany && IsTemporaryStorageEnabled, section, ModuleIDs.Customs.EU.DE.SumARegisterReadOnly);

			#endregion

			#region FR

			AddModule(section, ModuleIDs.Customs.EU.FR.CustomsStatement);

			#endregion

			#region TW

			AddModulesIf(CurrentCountry == Constants.CountryCodes.Taiwan, section, ModuleIDs.Customs.TW.Transhipment);
			AddModulesIf(CurrentCountry == Constants.CountryCodes.Taiwan && IsShowCusPackingListModule, section, ModuleIDs.Customs.CusPackingList);
			AddModulesIf(CurrentCountry == Constants.CountryCodes.Taiwan && IsShowBriefCustomsDeclarationsModule, section, ModuleIDs.Customs.TW.BriefCustomsDeclarations);

			#endregion

			#region GB

			AddModuleIf(CurrentCountry == Constants.CountryCodes.UnitedKingdom, section,
				ModuleIDs.Customs.EU.GB.DLUMessage);
			AddModuleIf(CurrentCountry == Constants.CountryCodes.UnitedKingdom, section,
				ModuleIDs.Customs.EU.GB.CDSDISQuery);

			AddModuleIf(CurrentCountry == Constants.CountryCodes.UnitedKingdom, section, ModuleIDs.Customs.EU.GB.CDSCashPayments);

			AddModuleIf(CurrentCountry == Constants.CountryCodes.UnitedKingdom && ManifestCustomsDataRegistry.Instance.PreBoardingNotificationManifestEnabled.Value, section, ModuleIDs.Customs.EU.GB.PreBoardingNotification);

			#endregion

			AddModule(section, ModuleIDs.CustomsReport);

			#region NO

			AddModuleIf(CurrentCountry == Constants.CountryCodes.Norway && IsTemporaryStorageEnabledForNorway, section, ModuleIDs.Customs.NO.TemporaryStorageRegisterReadOnly);

			#endregion

			#region TR
			AddModuleIf(CurrentCountry == Constants.CountryCodes.Turkey && (bool)ObjectFactory.Get<Integration.Customs.TR.ITRCustomsDataRegistry>().ExposeETradeModule.Value, section, ModuleIDs.Customs.TR.ETrade);
			#endregion

			#region KR
			AddModule(section, ModuleIDs.Customs.KR.CustomsStatement);
			AddModule(section, ModuleIDs.Customs.KR.MiscRequestMessages);
			AddModule(section, ModuleIDs.Customs.KR.DocumentListMessages);
			AddModule(section, ModuleIDs.Customs.KR.CusReconDeclaration);
			#endregion

			#region BR

			var enableBRLPCOModules = CurrentCountry == Constants.CountryCodes.Brazil && ObjectFactory.Get<Integration.Customs.BR.IBRCustomsDataRegistry>().EnableLPCO;
			AddModuleIf(enableBRLPCOModules, section, ModuleIDs.Customs.BR.LPCODeclaration);
			AddModuleIf(enableBRLPCOModules, section, ModuleIDs.Customs.BR.LPCOEntryHeader);
			var enableBRLICModules = CurrentCountry == Constants.CountryCodes.Brazil && ObjectFactory.Get<Integration.Customs.BR.IBRCustomsDataRegistry>().EnableImportLicense;
			AddModuleIf(enableBRLICModules, section, ModuleIDs.Customs.BR.License);
			AddModuleIf(enableBRLICModules, section, ModuleIDs.Customs.BR.LicenseEntryHeader);

			#endregion

			#region IE

			AddModuleIf(CurrentCountry == Constants.CountryCodes.Ireland, section, ModuleIDs.Customs.EU.IE.CustomsAndExciseReports);

			#endregion

			#region CH

			AddModuleIf(CurrentCountry == Constants.CountryCodes.Switzerland, section, ModuleIDs.Customs.CH.CustomsSummary);
			AddModuleIf(CurrentCountry == Constants.CountryCodes.Switzerland && (bool)ObjectFactory.Get<Integration.Customs.CH.ICHCustomsRegistry>().DeclarationActivationEnabled.Value, section, ModuleIDs.Customs.CH.DeclarationActivation);

			#endregion
		}

		bool IsDEMonthlyClosingEnabled => (bool)ObjectFactory.Get<Integration.Customs.DE.IDECustomsRegistry>().ShowMonthlyClosing.Value;

		void AddNctsIfInContractingCountry(ModuleCategory operationsCategory)
		{
			var nctsSettings = ObjectFactory.Get<Integration.Customs.Shared.INctsSettings>();
			var isNctsEnabled = nctsSettings.IsNctsEnabled;

			var isSptsEnabled = CurrentCountry == Constants.CountryCodes.Turkey && (bool)ObjectFactory.Get<Integration.Customs.TR.ITRCustomsDataRegistry>().ExposeSimplifiedProcedureTransitSystemModule.Value;
			var nctsModuleSectionRequired = isNctsEnabled || isSptsEnabled;
			operationsCategory.Sections.AddIf(!ProductivityWiseModeEnabled && nctsModuleSectionRequired, () =>
			{
				var ncts = new ModuleSection(ModuleTreeLoaderConstant.Section.EuNcts, ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, SecurityInstance.EuNcts, IconTypes.Box, IconTypes.Tick, ModuleTreeLoaderConstant.Subcategory.Customs);
				AddModuleIf(isNctsEnabled, ncts, ModuleIDs.Customs.EU.NctsMovementModule);
				AddModuleIf(isNctsEnabled, ncts, ModuleIDs.Customs.EU.NctsReportsModule);
				AddModuleIf(isSptsEnabled, ncts, ModuleIDs.Customs.TR.SimplifiedProcedureTransitSystem);
				return ncts;
			});
		}

		void AddCcsukIfInUnitedKingdom(ModuleCategory operationsCategory)
		{
			operationsCategory.Sections.AddIf(!ProductivityWiseModeEnabled && CurrentCountry == Constants.CountryCodes.UnitedKingdom, () =>
			{
				ModuleSection ccsuk = new ModuleSection(ModuleTreeLoaderConstant.Section.Ccsuk, ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, SecurityInstance.AirCcsuk, IconTypes.Box, IconTypes.Tick, ModuleTreeLoaderConstant.Subcategory.Customs);
				AddModule(ccsuk, ModuleIDs.Customs.EU.GB.CcsukMasterAndHouseCombined);
				AddModule(ccsuk, ModuleIDs.Customs.EU.GB.CcsukAirInventory);
				AddModule(ccsuk, ModuleIDs.Customs.EU.GB.CcsukAirInventoryHouse);
				AddModule(ccsuk, ModuleIDs.Customs.EU.GB.CcsukGenralMessage);
				AddModule(ccsuk, ModuleIDs.Customs.EU.GB.CcsukStandAloneFsrEnquiry);
				AddModule(ccsuk, ModuleIDs.Customs.EU.GB.GbCcsukReports);

				return ccsuk;
			});
		}

		void AddStampDutyForTurkey(ModuleCategory operationsCategory)
		{
			operationsCategory.Sections.AddIf(!ProductivityWiseModeEnabled && CurrentCountry == Constants.CountryCodes.Turkey, () =>
			{
				ModuleSection section = new ModuleSection(ModuleTreeLoaderConstant.Section.StampDutyMain, ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, SecurityInstance.StampDutyMain, IconTypes.Customs, IconTypes.Customs20x16, ModuleTreeLoaderConstant.Subcategory.Customs);
				AddModule(section, ModuleIDs.Customs.TR.StatementsStampDuty);

				return section;
			});
		}

		#endregion

		#region Order Manager

		void InitialiseOrderMgtSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.JobShipmentPreplanning);
			AddModule(section, ModuleIDs.Orders);
			AddModule(section, ModuleIDs.OrderLine);
			AddModule(section, ModuleIDs.OrdersReport);

			AddModuleIf(AdvOrmFeatureHelper.IsEnabled, section, ModuleIDs.SupplierBooking);
			AddModuleIf(AdvOrmFeatureHelper.IsEnabled, section, ModuleIDs.ContainerLoadList);
			AddModuleIf(AdvOrmFeatureHelper.IsEnabled, section, ModuleIDs.OrdersWebPortal);
			AddModuleIf(AdvOrmFeatureHelper.IsEnabled, section, ModuleIDs.OrderLinesWebPortal);
			AddModuleIf(AdvOrmFeatureHelper.IsEnabled, section, ModuleIDs.ContainerLoadPlan);
			AddModuleIf(ControlTowerFeatureHelper.IsControlTowerEnabled(), section, ModuleIDs.ControlTower);
		}

		#endregion

		#region Transport Booking

		void InitialiseTransportBookingSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.DtbBooking);
			AddModule(section, ModuleIDs.DtbBookingConsolidation);
			AddModule(section, ModuleIDs.DtbBookingReports);
		}

		#endregion

		#region Land Transport

		void InitialiseTransportConsignmentSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.DtbConsignmentRunSheet);
			AddModule(section, ModuleIDs.DtbConsignment);
			AddModule(section, ModuleIDs.DtbReports);
			AddModuleIf(ObjectFactory.Get<ITransportRegistry>().EnableLandTransport.Value, section, ModuleIDs.DtbConsignmentWebPortal);
			AddModuleIf(ObjectFactory.Get<ITransportRegistry>().EnableLandTransport.Value, section, ModuleIDs.DtbConsignmentRunSheetWebPortal);
		}

		#endregion

		#region Port Transport

		void InitialiseTransportSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.Cartage);
			AddModule(section, ModuleIDs.CartageWorkSheet);
			AddModule(section, ModuleIDs.CartageRunSheetDashboard);
			AddModule(section, ModuleIDs.CartageLeg);
			AddModule(section, ModuleIDs.CartageLegPlanner);
			AddModule(section, ModuleIDs.TransportReports);
			AddModule(section, ModuleIDs.TelematicsPreDriveChecklists);
		}

		#endregion

		#region CFS / CTO

		void InitialiseCFSCTOSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.LoadListConsol);
			AddModule(section, ModuleIDs.ShipmentReceival);
			AddModule(section, ModuleIDs.PackContainerRegistration);
			AddModule(section, ModuleIDs.ManifestTally);
			AddModule(section, ModuleIDs.ShipmentGatePass);
			AddModule(section, ModuleIDs.Customs.AU.AirCTOImport);
			AddModule(section, ModuleIDs.Customs.AU.AirCTOExport);
			AddModule(section, ModuleIDs.Customs.AU.AirCargoOutturnBills);
			AddModule(section, ModuleIDs.Customs.AU.SeaCargoDepot);
			AddModule(section, ModuleIDs.Customs.AU.SeaCargoOutturnBills);
			AddModule(section, ModuleIDs.Customs.AU.AirCargoDepot);
			AddModule(section, ModuleIDs.CFSCTOReports);
			AddModule(section, ModuleIDs.GateBooking);
			AddModule(section, ModuleIDs.GateControl);
		}

		#endregion

		#region Warehouse

		protected void InitialiseWarehouseSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.WhsReceive);
			AddModule(section, ModuleIDs.WhsOrder);
			AddModule(section, ModuleIDs.WhsWorkOrder);
			AddModuleIf(WarehouseDataRegistry.Instance.EnableDynamicWorkOrder.Value, section, ModuleIDs.WhsDynamicWorkOrder);
			AddModule(section, ModuleIDs.WhsVASOrder);
			AddModule(section, ModuleIDs.WhsPicking);
			AddModule(section, ModuleIDs.Packing);
			AddModule(section, ModuleIDs.WhsRelease);
			AddModule(section, ModuleIDs.WhsTransfer);
			AddModule(section, ModuleIDs.WhsAdjustment);
			AddModule(section, ModuleIDs.WhsHandlingUnit);
			AddModule(section, ModuleIDs.WhsInventory);
			AddModule(section, ModuleIDs.WhsStocktake);
			AddModule(section, ModuleIDs.WhsInvoicing);
			AddModule(section, ModuleIDs.WhsReport);
			AddModule(section, ModuleIDs.WhsAdHocServiceJob);
			AddModule(section, ModuleIDs.WhsProductWarehousePortal);
			AddModule(section, ModuleIDs.WhsLoad);
		}

		#endregion

		#region TransitWarehouse

		protected void InitialiseTransitWarehouseSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.TransitWarehousePortal);
			AddModule(section, ModuleIDs.WhsItemReceiveTransportationUnit);
			AddModule(section, ModuleIDs.WhsTransitReceiveConsignment);
			AddModule(section, ModuleIDs.WhsTransitDispatchConsignment);
			AddModule(section, ModuleIDs.WhsTransitReport);
			AddModule(section, ModuleIDs.WhsItemReceiveASN);
			AddModule(section, ModuleIDs.WhsItemDispatchTransportationUnit);
			AddModule(section, ModuleIDs.TransitHandlingUnit);
			AddModule(section, ModuleIDs.WhsItemDispatchLoadList);
			AddModule(section, ModuleIDs.WhsItemTransferHeader);
		}

		#endregion

		#region ContainerYard

		protected void InitialiseContainerYardSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.CYDAdHocServiceOrder);
			AddModule(section, ModuleIDs.CYDDeliveryHeader);
			AddModule(section, ModuleIDs.CYDPickupHeader);
			AddModule(section, ModuleIDs.CYDReceiveAdvice);
			AddModule(section, ModuleIDs.CYDReleaseAdvice);
			AddModule(section, ModuleIDs.ContainerYardPortal);
			AddModule(section, ModuleIDs.CYDTransportationUnit);
			AddModule(section, ModuleIDs.CYDYardUnitState);
			AddModule(section, ModuleIDs.CYDYardReport);
			AddModule(section, ModuleIDs.MNRWorkOrder);
			AddModuleIf(false, section, ModuleIDs.MNRSurvey);
			AddModule(section, ModuleIDs.CYDPeriodicInvoicing);
		}

		#endregion

		#region GateManagement

		protected void InitialiseGateManagementSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.GteBooking);
			AddModule(section, ModuleIDs.GteGateMovementBooking);
			AddModule(section, ModuleIDs.GteVehicleMovement);
			AddModule(section, ModuleIDs.GteGateMovement);
			AddModule(section, ModuleIDs.GateManagementPortal);
		}

		#endregion

		#region Sales and Marketing

		void InitialiseClientRelationshipManagementSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.SalesDashboard);
			AddModule(section, ModuleIDs.SalesEnquiry);
			AddModule(section, ModuleIDs.Commission);
			AddModule(section, ModuleIDs.Communication);
			AddModule(section, ModuleIDs.ClientIntelligence);
			AddModule(section, ModuleIDs.GlbCompanyCampaign);
			AddModule(section, ModuleIDs.Opportunity);
			AddModule(section, ModuleIDs.CompetitorIntelligence);
			AddModule(section, ModuleIDs.NewsAndAnnouncement);
			AddModule(section, ModuleIDs.SalesMgrReports);
		}

		#endregion

		#region LinerAndAgency

		void InitialiseShippingSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.Customs.AU.ExportCustomsManifest);
			AddModule(section, ModuleIDs.Customs.AU.VoyageManifest);
			AddModule(section, ModuleIDs.AgencyBooking);
			AddModule(section, ModuleIDs.AgencyBillOfLading);
			AddModule(section, ModuleIDs.AgencyBillContainers);
			AddModule(section, ModuleIDs.AgencyContainerManager);
			AddModule(section, ModuleIDs.AgencyContainerMove);
			AddModule(section, ModuleIDs.AgencyContainerDetention);
			AddModule(section, ModuleIDs.AgencyVoyageAccounting);
			AddModule(section, ModuleIDs.AgencySundryCharges);
			AddModule(section, ModuleIDs.AgencyReports);
		}

		#endregion

		#region Doc Manager

		void InitialiseDocMgtSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.DocumentAllocation);
			AddModule(section, ModuleIDs.DocumentDbMerger);
			AddModule(section, ModuleIDs.DocumentDbManager);
			AddModule(section, ModuleIDs.ArchiveEDocs);
			AddModule(section, ModuleIDs.DocManagerReports);
		}

		#endregion

		#region Productivity Tools

		void AddProductivityToolsModules(ModuleSection section)
		{
			AddModule(section, ModuleIDs.WorkItem);
			AddModule(section, ModuleIDs.Project);
			AddModule(section, ModuleIDs.CustomerServiceTicket);
		}

		#endregion

		#endregion

		#region Manage

		#region Initialisation

		void InitialiseManageCategory(ModuleCategory category)
		{
			var clientRelationshipManagementSection = new ModuleSection(ModuleTreeLoaderConstant.Section.ClientRelationshipManagement, ModuleTreeCustomerServiceMenuSectionList.Codes.ClientRelationManagement, SecurityInstance.ClientRelationshipManagement, IconTypes.Contacts, IconTypes.Contacts20x16, ModuleTreeLoaderConstant.Subcategory.SalesAndMarketing);
			category.Sections.Add(clientRelationshipManagementSection);
			InitialiseClientRelationshipManagementSection(clientRelationshipManagementSection);

			category.Sections.AddIf(!ProductivityWiseModeEnabled, () =>
			{
				var tariffsAndRates = new ModuleSection(ModuleTreeLoaderConstant.Section.TariffsAndRates, ModuleTreeCustomerServiceMenuSectionList.Codes.TariffsRates, SecurityInstance.TariffsAndRates, IconTypes.Processes, IconTypes.Processes20x16, ModuleTreeLoaderConstant.Subcategory.TariffsAndRates);
				InitialiseTariffsAndRatesSection(tariffsAndRates);
				return tariffsAndRates;
			});

			category.Sections.AddIf(!ProductivityWiseModeEnabled, () =>
			{
				var wiseRates = new ModuleSection(ModuleTreeLoaderConstant.Section.WiseRates, ModuleTreeCustomerServiceMenuSectionList.Codes.WiseRates, SecurityInstance.WiseRatesSection, IconTypes.Processes, IconTypes.Processes20x16, ModuleTreeLoaderConstant.Subcategory.TariffsAndRates);
				InitialiseWiseRatesSection(wiseRates);
				return wiseRates;
			});

			AddBusinessIntelligenceCategoryIfApplicable(category);
			AddManageWorkflowSection(category);

			var documentMgt = new ModuleSection(ModuleTreeLoaderConstant.Section.DocManager, ModuleTreeCustomerServiceMenuSectionList.Codes.DocManager, SecurityInstance.DocManager, IconTypes.Scanner, IconTypes.Scanner20x16, ModuleTreeLoaderConstant.Subcategory.DocManager);
			category.Sections.Add(documentMgt);
			InitialiseDocMgtSection(documentMgt);

			var receivablesSection = new ModuleSection(ModuleTreeLoaderConstant.Section.Receivables, ModuleTreeCustomerServiceMenuSectionList.Codes.Receivables, SecurityInstance.Receivables, IconTypes.Receivables, IconTypes.Receivables20x16, ModuleTreeLoaderConstant.Subcategory.Receivables);
			category.Sections.Add(receivablesSection);
			InitialiseReceivablesSection(receivablesSection);

			var payablesSection = new ModuleSection(ModuleTreeLoaderConstant.Section.Payables, ModuleTreeCustomerServiceMenuSectionList.Codes.Payables, SecurityInstance.Payables, IconTypes.Payables, IconTypes.Payables20x16, ModuleTreeLoaderConstant.Subcategory.Payables);
			category.Sections.Add(payablesSection);
			InitialisePayablesSection(payablesSection);

			var cashBook = new ModuleSection(ModuleTreeLoaderConstant.Section.CashBook, ModuleTreeCustomerServiceMenuSectionList.Codes.CashBooks, SecurityInstance.CashBook, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.CashBook);
			category.Sections.Add(cashBook);
			InitialiseCashBookSection(cashBook);

			var jobCosting = new ModuleSection(ModuleTreeLoaderConstant.Section.JobCosting, ModuleTreeCustomerServiceMenuSectionList.Codes.JobCosting, SecurityInstance.JobCosting, IconTypes.JobCosting, IconTypes.JobCosting20x16, ModuleTreeLoaderConstant.Subcategory.JobCosting);
			category.Sections.Add(jobCosting);
			InitialiseJobCostingSection(jobCosting);

			var generalLedger = new ModuleSection(ModuleTreeLoaderConstant.Section.GeneralLedger, ModuleTreeCustomerServiceMenuSectionList.Codes.GeneralLedger, SecurityInstance.GeneralLedger, IconTypes.Book2, IconTypes.Book2_20x16, ModuleTreeLoaderConstant.Subcategory.GeneralLedger);
			category.Sections.Add(generalLedger);
			InitialiseGeneralLedgerJournalSection(generalLedger);

			var glConsolidations = new ModuleSection(ModuleTreeLoaderConstant.Section.GLConsolidations, ModuleTreeCustomerServiceMenuSectionList.Codes.GlConsolidations, SecurityInstance.GLConsolidations, IconTypes.Book, IconTypes.Book2_20x16, ModuleTreeLoaderConstant.Subcategory.GeneralLedger);
			category.Sections.Add(glConsolidations);
			InitialiseGLConsolidationsSection(glConsolidations);

			category.Sections.AddIf(!ProductivityWiseModeEnabled && AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.Value, () =>
			{
				var reportingBooks = new ModuleSection(ModuleTreeLoaderConstant.Section.GLReportingBooks, ModuleTreeCustomerServiceMenuSectionList.Codes.ReportingBooks, SecurityInstance.GLReportingBooks, IconTypes.SomeBooks, IconTypes.SomeBooks20x16, ModuleTreeLoaderConstant.Subcategory.GeneralLedger);
				InitialiseGeneralLedgerReportingBooksSection(reportingBooks);
				return reportingBooks;
			});

			var budgets = new ModuleSection(ModuleTreeLoaderConstant.Section.Budgets, ModuleTreeCustomerServiceMenuSectionList.Codes.Budgets, SecurityInstance.BudgetsSection, IconTypes.Dollar, IconTypes.Dollar20x16, ModuleTreeLoaderConstant.Subcategory.Budgets);
			category.Sections.Add(budgets);
			InitialiseGeneralLedgerBudgetSection(budgets);

			category.Sections.AddIf(NettingEnabled, () =>
			{
				var netting = new ModuleSection(ModuleTreeLoaderConstant.Section.Netting, ModuleTreeCustomerServiceMenuSectionList.Codes.Netting, SecurityInstance.NettingSection, IconTypes.Dollar, IconTypes.Dollar20x16, ModuleTreeLoaderConstant.Subcategory.Netting);
				InitialiseNettingSection(netting);
				return netting;
			});

			category.Sections.AddIf(AccountingMasterFilesRegistry.Instance.EnableAssetManagementFunctionality.Value, () =>
			{
				var section = new ModuleSection(ModuleTreeLoaderConstant.Section.AssetManagement, ModuleTreeCustomerServiceMenuSectionList.Codes.AssetManagement, SecurityInstance.AssetManagement, IconTypes.Tag, IconTypes.Tag, ModuleTreeLoaderConstant.Subcategory.AssetManagement);
				AddModule(section, ModuleIDs.AssetManagementPortal);
				AddModule(section, ModuleIDs.AssetManagementReports);
				return section;
			});
		}

		void AddManageWorkflowSection(ModuleCategory category)
		{
			category.Sections.AddIf(IsPlanningManagementEnabled, () =>
			{
				var section = new ModuleSection(ModuleTreeLoaderConstant.Section.WorkflowPlanning, ModuleTreeCustomerServiceMenuSectionList.Codes.BufferManagement, SecurityInstance.WorkflowPlanningSection, IconTypes.Processes, IconTypes.Processes20x16, ModuleTreeLoaderConstant.Subcategory.WorkflowAndProcess);
				InitialiseWorkflowPlanningSection(section);

				return section;
			});

			var workflowProcess = new ModuleSection(ModuleTreeLoaderConstant.Section.ManageWorkflowSection, ModuleTreeCustomerServiceMenuSectionList.Codes.WorkflowAndProcesses, SecurityInstance.WorkflowOperationsSection, IconTypes.Processes, IconTypes.Processes20x16, ModuleTreeLoaderConstant.Subcategory.WorkflowAndProcess);
			category.Sections.Add(workflowProcess);

			InitialiseOperationsWorkflowSection(workflowProcess);
		}

		#endregion

		#region Receivables

		void InitialiseReceivablesSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.ARTransaction);
			AddModule(section, ModuleIDs.ARPaymentProcessing);
			AddModule(section, ModuleIDs.ZARMatching);
			AddModule(section, ModuleIDs.ARAccQueryClaim);
			AddModule(section, ModuleIDs.OrgCollectionCalls);
			AddModule(section, ModuleIDs.AREnquiry);
			AddModule(section, ModuleIDs.ARCreditNoteApproval);
			AddModule(section, ModuleIDs.CreditControlledDocumentsApproval);
			AddModule(section, ModuleIDs.InvoicePrinting);
			AddModule(section, ModuleIDs.InvoiceBatch);
			AddModule(section, ModuleIDs.Statement);
			AddModule(section, ModuleIDs.ReceivReports);
			AddModule(section, ModuleIDs.AccCollectionBatch);
			AddModule(section, ModuleIDs.AccCollectionOrder);

			if (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.Value)
			{
				AddModule(section, ModuleIDs.ARCashAdvance);
			}

			if (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value)
			{
				AddModule(section, ModuleIDs.ARComplianceDocument);
			}
		}

		#endregion

		#region Payables

		void InitialisePayablesSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.APTransaction);
			AddModule(section, ModuleIDs.AccHotCheque);
			AddModule(section, ModuleIDs.APPaymentProcessing);
			AddModule(section, ModuleIDs.ZAPMatching);
			AddModule(section, ModuleIDs.APAccQueryClaim);
			AddModule(section, ModuleIDs.APEnquiry);
			if (AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.Value)
			{
				AddModule(section, ModuleIDs.APInvoiceApproval);
				AddModule(section, ModuleIDs.UnapprovedIntercompanyTransaction);
			}
			else
			{
				AddModule(section, ModuleIDs.UnapprovedTransaction);
			}
			AddModule(section, ModuleIDs.TransactionsPendingAllocation);
			AddModule(section, ModuleIDs.CASSCostFileImport);
			AddModule(section, ModuleIDs.APIncompleteInvoices);
			AddModule(section, ModuleIDs.PayablesReports);
			if (AccountingMasterFilesRegistry.Instance.EnableTransactionPendingAllocationApproval.Value)
			{
				AddModule(section, ModuleIDs.TransactionsPendingAllocationApproval);
			}
			AddModule(section, ModuleIDs.AccPayableOrder);
			if (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value)
			{
				AddModule(section, ModuleIDs.APComplianceDocument);
			}

			if (AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.Value)
			{
				AddModule(section, ModuleIDs.APCashAdvance);
			}

			AddModule(section, ModuleIDs.PaymentBatch);

			AddModuleIf(AccountingConfigurationRegistry.Instance.EnablePayablesInvoiceProcessingPortal.Value, section, ModuleIDs.APInvoiceProcessingPortal);
		}

		#endregion

		#region Cashbook

		void InitialiseCashBookSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.CashbookTransaction);
			AddModule(section, ModuleIDs.DepositBatch);
			AddModule(section, ModuleIDs.DirectDebitFile);
			AddModule(section, ModuleIDs.BankReconcilliation);
			AddModule(section, ModuleIDs.CashBookReports);
			AddModuleIf(AccountingConfigurationRegistry.Instance.EnableChequeManagementFunctionality.Value, section, ModuleIDs.Cheque);
			AddModuleIf(AccountingConfigurationRegistry.Instance.EnableChequeManagementFunctionality.Value, section, ModuleIDs.ChequeTransaction);
		}

		#endregion

		#region Job Costing

		void InitialiseJobCostingSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.WIPAccruals);
			AddModule(section, ModuleIDs.JobManagement);
			AddModule(section, ModuleIDs.JobRevenueJournal);
			AddModule(section, ModuleIDs.JobCostingReport);
			AddModuleIf(ObjectFactory.Get<IAccounting>().EnableBulkDisbursementJobsClosure, section, ModuleIDs.BulkDSBJobCloseBatchApproval);
		}

		#endregion

		#region General Ledger Journal

		void InitialiseGeneralLedgerJournalSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.GLJournal);
			AddModule(section, ModuleIDs.PeriodManagement);
			AddModuleIf(eHubMessagingRegistry.Instance.HasInterfaceConnector, section, ModuleIDs.TransactionsExport);
			AddModuleIf(eHubMessagingRegistry.Instance.HasInterfaceConnector, section, ModuleIDs.XmlTransactionsImport);
			AddModule(section, ModuleIDs.CsvTransactionsImport);

			AddModulesIf(SystemDataRegistry.Instance.ActivateSystemMergeDataInterface.Value, section,
				ModuleIDs.OustandingJournalsImport,
				ModuleIDs.OustandingJournalsExport);

			AddModule(section, ModuleIDs.GenericTransaction);
			AddModule(section, ModuleIDs.GLReports);

			AddModuleIf(CurrentCountry == Constants.CountryCodes.China && GlbStaff.CurrentUser.IsSupportUser && AccountingMasterFilesRegistry.Instance.ShowChinaGBTDataInterfaceMenus.Value,
						section,
						ModuleIDs.CN2004DataInterface);
			AddModuleIf(CurrentCountry == Constants.CountryCodes.China && Env.Security.ChinaDataInterface.IsAllowed && AccountingMasterFilesRegistry.Instance.ShowChinaGBTDataInterfaceMenus.Value,
						section,
						ModuleIDs.CNDataInterface);
			AddModuleIf(CurrentCountry == Constants.CountryCodes.China && Env.Security.ChinaReconciliationExport.IsAllowed, section, ModuleIDs.CNReconciliationExport);
			AddModuleIf(CurrentCountry == Constants.CountryCodes.China || CurrentCountry == Constants.CountryCodes.Taiwan, section, ModuleIDs.AccountingVoucher);
			AddModuleIf(CurrentCountry == Constants.CountryCodes.China, section, ModuleIDs.ChinaJournalListing);

			AddModule(section, ModuleIDs.GLJournalApproval);

			AddModuleIf(AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value.Any(), section, ModuleIDs.AccComplianceReport);

			if (AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.Value)
			{
				var featureData = ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingGeneralLedgerDataFeature);

				if (featureData != null && featureData.TryDeserializeParameterAsJson<GeneralLedgerDataFeatureControlModel>(out var generalLedgerDataFeatureControlModel))
				{
					AddModuleIf(generalLedgerDataFeatureControlModel.EnableAccountingJournalsModule, section, ModuleIDs.AccGeneralLedgerData);
				}
			}
		}

		#endregion

		#region General Ledger Consolidations

		void InitialiseGLConsolidationsSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.GLConsolidationGroups);
		}

		#endregion

		#region General Ledger Budget

		void InitialiseGeneralLedgerBudgetSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.GLBudget);
			AddModule(section, ModuleIDs.BudgetReports);
		}

		#endregion

		#region General Ledger Reporting Books

		void InitialiseGeneralLedgerReportingBooksSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.GLReportingBooksReport);
		}

		#endregion

		#region Tariffs & Rates

		void InitialiseTariffsAndRatesSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.GlobalRates);
			AddModule(section, ModuleIDs.ClientRates);
			AddModule(section, ModuleIDs.Costing);
			AddModule(section, ModuleIDs.IntercompanyTariffs);
			AddModule(section, ModuleIDs.ProfitShare);
			AddModule(section, ModuleIDs.Quotations);
			AddModule(section, ModuleIDs.OneOffQuotes);
			AddModule(section, ModuleIDs.TariffRateReports);
		}

		void InitialiseWiseRatesSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.WiseRatesCargoguide);
			AddModule(section, ModuleIDs.WiseRatesCargoSphere);
			AddModule(section, ModuleIDs.WiseRates);
			AddModuleIf(RatingFeatureHelper.CarrierConnect.IsFeatureEnabled(), section, ModuleIDs.CarrierConnect);
		}

		#endregion

		#region Business Intelligence & Analytics

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decrutification", "WTG3012:AvoidBoolLiteralsInLargerBoolExpressions", Justification = "#if directive in expression")]
		void AddBusinessIntelligenceCategoryIfApplicable(ModuleCategory category)
		{
			bool shouldAddBiSection =
#if DEBUG
				true ||
#endif
				!string.IsNullOrEmpty(SystemDataRegistry.Instance?.BiAuditServer?.Value) ||
				!string.IsNullOrEmpty(SystemDataRegistry.Instance?.BiDataWarehouseServer?.Value);
			category.Sections.AddIf(shouldAddBiSection, GetBusinessIntelligenceModuleSection);
		}

		ModuleSection GetBusinessIntelligenceModuleSection()
		{
			var businessIntelligenceAndAnalytics = new ModuleSection(
				entry: ModuleTreeLoaderConstant.Section.BusinessIntelligenceAndAnalytics,
				customerServiceMenuSectionCode: ModuleTreeCustomerServiceMenuSectionList.Codes.BusinessIntelligence,
				checkPoint: SecurityInstance.BusinessIntelligenceAndAnalytics,
				icon: IconTypes.Processes,
				groupImage: IconTypes.Processes20x16,
				subcategory: ModuleTreeLoaderConstant.Subcategory.BusinessIntelligenceAndAnalytics);

			AddGeneralModulesToBusinessIntelligenceSection(businessIntelligenceAndAnalytics);
			AddOlapSpecificModulesToBusinessIntelligenceSection(businessIntelligenceAndAnalytics);

			return businessIntelligenceAndAnalytics;
		}

		void AddGeneralModulesToBusinessIntelligenceSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.BiManager);
		}

		void AddOlapSpecificModulesToBusinessIntelligenceSection(ModuleSection section)
		{
			bool isPowerBiReportsSet = !(string.IsNullOrEmpty(SystemDataRegistry.Instance?.BiPowerBiWebPortalUrl?.Value));
			AddModuleIf(isPowerBiReportsSet, section, ModuleIDs.AnalyticsReports);
		}

		#endregion

		#region Netting

		void InitialiseNettingSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.NettingStatement);
			AddModule(section, ModuleIDs.NettingReports);
		}

		#endregion

		#endregion

		#region Maintain

		#region Initialisation

		void InitialiseMaintainCategory(ModuleCategory category)
		{
			var masterData = new ModuleSection(ModuleTreeLoaderConstant.Section.MasterData, ModuleTreeCustomerServiceMenuSectionList.Codes.MasterData, SecurityInstance.MasterData, IconTypes.Book3, IconTypes.Book3_20x16, ModuleTreeLoaderConstant.Subcategory.MasterData);
			category.Sections.Add(masterData);
			InitialiseMasterDataSection(masterData);

			var reference = new ModuleSection(ModuleTreeLoaderConstant.Section.References, ModuleTreeCustomerServiceMenuSectionList.Codes.ReferenceFiles, SecurityInstance.References, IconTypes.Book3, IconTypes.Book3_20x16, ModuleTreeLoaderConstant.Subcategory.ReferenceFiles);
			category.Sections.Add(reference);
			InitialiseReferenceSection(reference);

			var relationshipManagerSection = new ModuleSection(ModuleTreeLoaderConstant.Section.RelationshipManagerConfig, ModuleTreeCustomerServiceMenuSectionList.Codes.SalesMarketing, SecurityInstance.RelationshipManagerConfig, IconTypes.Contacts, IconTypes.Contacts20x16, ModuleTreeLoaderConstant.Subcategory.SalesAndMarketing);
			category.Sections.Add(relationshipManagerSection);
			IntialiseRelationshipManagerConfigSection(relationshipManagerSection);

			category.Sections.AddIf(!ProductivityWiseModeEnabled, () =>
			{
				var tariffsAndRates = new ModuleSection(ModuleTreeLoaderConstant.Section.TariffsAndRates, ModuleTreeCustomerServiceMenuSectionList.Codes.TariffsRates, SecurityInstance.TariffsAndRatesConfig, IconTypes.Processes, IconTypes.Processes20x16, ModuleTreeLoaderConstant.Subcategory.TariffsAndRates);
				InitialiseTariffsAndRatesConfigSection(tariffsAndRates);

				return tariffsAndRates;
			});

			category.Sections.AddIf(IsAnyLevelOfBufferManagementEnabled, () =>
			{
				var bufferManagementSection = new ModuleSection(ModuleTreeLoaderConstant.Section.BufferManagementConfig, ModuleTreeCustomerServiceMenuSectionList.Codes.BufferManagement, SecurityInstance.BufferManagementConfig, IconTypes.None, IconTypes.None, ModuleTreeLoaderConstant.Subcategory.PerformanceManagement);
				InitialiseBufferManagementConfigSection(bufferManagementSection);
				return bufferManagementSection;
			});

			var locations = new ModuleSection(ModuleTreeLoaderConstant.Section.Location, ModuleTreeCustomerServiceMenuSectionList.Codes.Locations, SecurityInstance.Location, IconTypes.Globe, IconTypes.Globe20x16, ModuleTreeLoaderConstant.Subcategory.Locations);
			category.Sections.Add(locations);
			InitialiseLocationsSection(locations);

			var account = new ModuleSection(ModuleTreeLoaderConstant.Section.Account, ModuleTreeCustomerServiceMenuSectionList.Codes.Account, SecurityInstance.Account, IconTypes.SomeBooks, IconTypes.SomeBooks20x16, ModuleTreeLoaderConstant.Subcategory.Account);
			category.Sections.Add(account);
			InitialiseAccountConfigSection(account);

			category.Sections.AddIf(!ProductivityWiseModeEnabled && AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.Value, () =>
			{
				var reportingBooks = new ModuleSection(ModuleTreeLoaderConstant.Section.ReportingBooks, ModuleTreeCustomerServiceMenuSectionList.Codes.ReportingBooks, SecurityInstance.ReportingBooksSection, IconTypes.SomeBooks, IconTypes.SomeBooks20x16, ModuleTreeLoaderConstant.Subcategory.Account);
				InitialiseReportingBooksConfigSection(reportingBooks);
				return reportingBooks;
			});

			category.Sections.AddIf(!ProductivityWiseModeEnabled, () =>
			{
				var customs = new ModuleSection(ModuleTreeLoaderConstant.Section.CustomsFiles, ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, SecurityInstance.CustomsFiles, IconTypes.Customs, IconTypes.Customs20x16, ModuleTreeLoaderConstant.Subcategory.Customs);
				InitialiseCustomsConfigSection(customs);

				return customs;
			});

			category.Sections.AddIf(!ProductivityWiseModeEnabled, () =>
			{
				var uSCustoms = new ModuleSection(ModuleTreeLoaderConstant.Section.CustomsUS, ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, SecurityInstance.MaintainUSCustoms, IconTypes.Customs, IconTypes.Customs20x16, ModuleTreeLoaderConstant.Subcategory.Customs);
				InitialiseUSCustomsMain(uSCustoms);

				return uSCustoms;
			});

			category.Sections.AddIf(!ProductivityWiseModeEnabled && CurrentCountry == Constants.CountryCodes.Canada, () =>
			{
				var cACustoms = new ModuleSection(ModuleTreeLoaderConstant.Section.CustomsCA, ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, SecurityInstance.MaintainCACustoms, IconTypes.Customs, IconTypes.Customs20x16, ModuleTreeLoaderConstant.Subcategory.Customs);
				InitialiseCACustomsMain(cACustoms);
				return cACustoms;
			});

			category.Sections.AddIf(!ProductivityWiseModeEnabled && CurrentCountry == Constants.CountryCodes.Colombia, () =>
			{
				var cOCustoms = new ModuleSection(ModuleTreeLoaderConstant.Section.CustomsCO, ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, SecurityInstance.CustomsMain, IconTypes.Customs, IconTypes.Customs20x16, ModuleTreeLoaderConstant.Subcategory.Customs);
				InitialiseCOCustomsMain(cOCustoms);
				return cOCustoms;
			});

			category.Sections.AddIf(!ProductivityWiseModeEnabled, () =>
			{
				var warehouseConfigSection = new ModuleSection(ModuleTreeLoaderConstant.Section.WhsConfig, ModuleTreeCustomerServiceMenuSectionList.Codes.Warehouse, SecurityInstance.WhsConfig, IconTypes.Forklift, IconTypes.Forklift20x16, ModuleTreeLoaderConstant.Subcategory.Warehouse);
				InitialiseWarehouseConfigSection(warehouseConfigSection);

				return warehouseConfigSection;
			});

			var peopleOperationsSection = new ModuleSection(ModuleTreeLoaderConstant.Section.PeopleOperations, ModuleTreeCustomerServiceMenuSectionList.Codes.PeopleOperations, SecurityInstance.PeopleOperations, IconTypes.Contacts, IconTypes.Contacts20x16, ModuleTreeLoaderConstant.Subcategory.HRM);
			category.Sections.Add(peopleOperationsSection);
			InitalisePeopleOperationsSection(peopleOperationsSection);

			var learningDevelopmentSection = new ModuleSection(ModuleTreeLoaderConstant.Section.LearningDevelopment, ModuleTreeCustomerServiceMenuSectionList.Codes.LearningDevelopment, SecurityInstance.LearningDevelopment, IconTypes.Contacts, IconTypes.Contacts20x16, ModuleTreeLoaderConstant.Subcategory.HRM);
			category.Sections.Add(learningDevelopmentSection);
			InitaliseLearningDevelopmentSection(learningDevelopmentSection);

			var hrRecruiterSection = new ModuleSection(ModuleTreeLoaderConstant.Section.HRRecruiter, ModuleTreeCustomerServiceMenuSectionList.Codes.Recruiter, SecurityInstance.HRRecruiter, IconTypes.Contacts, IconTypes.Contacts20x16, ModuleTreeLoaderConstant.Subcategory.HRM);
			category.Sections.Add(hrRecruiterSection);
			InitaliseRecruiterConfigSection(hrRecruiterSection);

			var workflowManager = new ModuleSection(ModuleTreeLoaderConstant.Section.WorkflowSection, ModuleTreeCustomerServiceMenuSectionList.Codes.WorkflowManager, SecurityInstance.WorkflowSection, IconTypes.Processes, IconTypes.Processes20x16, ModuleTreeLoaderConstant.Subcategory.ProcessManager);
			category.Sections.Add(workflowManager);
			InitialiseWorkflowSection(workflowManager);

			var ediMessaging = new ModuleSection(ModuleTreeLoaderConstant.Section.EDIMessaging, ModuleTreeCustomerServiceMenuSectionList.Codes.EDIMessaging, SecurityInstance.EDIMessagingSection, IconTypes.Processes, IconTypes.Processes20x16, ModuleTreeLoaderConstant.Subcategory.EDIMessaging);
			category.Sections.Add(ediMessaging);
			InitialiseEDIMessagingSection(ediMessaging);

			var archiveManager = new ModuleSection(ModuleTreeLoaderConstant.Section.ArchiveManagerSection, ModuleTreeCustomerServiceMenuSectionList.Codes.ArchiveManager, SecurityInstance.ArchiveManagerSection, IconTypes.Blank, IconTypes.Blank, ModuleTreeLoaderConstant.Subcategory.ArchiveManager);
			category.Sections.Add(archiveManager);
			InitialiseArchiveManagerSection(archiveManager);

			var userSection = new ModuleSection(ModuleTreeLoaderConstant.Section.UserAdmin, ModuleTreeCustomerServiceMenuSectionList.Codes.UserAdmin, SecurityInstance.UserAdmin, IconTypes.Folders, IconTypes.Folders20x16, ModuleTreeLoaderConstant.Subcategory.UserAdmin);
			category.Sections.Add(userSection);
			InitialiseUserSection(userSection);

			var systemSection = new ModuleSection(ModuleTreeLoaderConstant.Section.System, ModuleTreeCustomerServiceMenuSectionList.Codes.System, SecurityInstance.System, IconTypes.Folders, IconTypes.Folders20x16, ModuleTreeLoaderConstant.Subcategory.System);
			category.Sections.Add(systemSection);
			InitialiseSystemSection(systemSection);

			var sysPrintingSection = new ModuleSection(ModuleTreeLoaderConstant.Section.PrintingSection, ModuleTreeCustomerServiceMenuSectionList.Codes.PrintingSection, SecurityInstance.PrintingSection, IconTypes.Folders, IconTypes.Folders20x16, ModuleTreeLoaderConstant.Subcategory.System);
			category.Sections.Add(sysPrintingSection);
			InitialisePrintingSection(sysPrintingSection);

			var sysEmailSection = new ModuleSection(ModuleTreeLoaderConstant.Section.EmailSection, ModuleTreeCustomerServiceMenuSectionList.Codes.EmailSection, SecurityInstance.EmailSection, IconTypes.Folders, IconTypes.Folders20x16, ModuleTreeLoaderConstant.Subcategory.System);
			category.Sections.Add(sysEmailSection);
			InitialiseEmailSection(sysEmailSection);

			var sysReportSection = new ModuleSection(ModuleTreeLoaderConstant.Section.ReportSection, ModuleTreeCustomerServiceMenuSectionList.Codes.ReportSection, SecurityInstance.ReportSection, IconTypes.Folders, IconTypes.Folders20x16, ModuleTreeLoaderConstant.Subcategory.System);
			category.Sections.Add(sysReportSection);
			InitialiseReportSection(sysReportSection);
		}

		#endregion

		#region Reference Files

		void InitialiseReferenceSection(ModuleSection section)
		{
			AddModuleIf(!ProductivityWiseModeEnabled, section, ModuleIDs.RefAirline);

			AddModuleIf(!ProductivityWiseModeEnabled, section, ModuleIDs.BarcodeParsing);
			AddModuleIf(!ProductivityWiseModeEnabled, section, ModuleIDs.BarcodeValidation);
			AddModuleIf(!ProductivityWiseModeEnabled, section, ModuleIDs.WhsInventoryHeldCodes);

			AddModuleIf(!ProductivityWiseModeEnabled, section, ModuleIDs.RefCommodityCode);
			AddModuleIf(!ProductivityWiseModeEnabled, section, ModuleIDs.RefContainer);
			AddModuleIf(!ProductivityWiseModeEnabled, section, ModuleIDs.RefCurrency);

			AddModuleIf(!ProductivityWiseModeEnabled, section, ModuleIDs.RefOrgPartCategory);

			AddModuleIf(!ProductivityWiseModeEnabled, section, ModuleIDs.UNDGSubstance);
			AddModuleIf(!ProductivityWiseModeEnabled, section, ModuleIDs.UNDGCommonData);

			AddModuleIf(!ProductivityWiseModeEnabled, section, ModuleIDs.ExchangeRate);
			AddModuleIf(!ProductivityWiseModeEnabled, section, ModuleIDs.ProductionRulesPortal);

			AddModule(section, ModuleIDs.RefDocOrgCusCode);
			AddModule(section, ModuleIDs.RefDocType);
			AddModule(section, ModuleIDs.RefDocSource);
			AddModuleIf(!ProductivityWiseModeEnabled, section, ModuleIDs.RefEquipment);
			AddModuleIf(!ProductivityWiseModeEnabled, section, ModuleIDs.CartageType);

			//Carrier Messaging Buss
			AddModuleIf(!ProductivityWiseModeEnabled, section, ModuleIDs.RefAccessorial);
			AddModuleIf(!ProductivityWiseModeEnabled && ObjectFactory.Get<ITransportRegistry>().EnableBookingWithCarrierMessagingBuss.Value, section, ModuleIDs.RefMessagingBussCarrierInfo);

			//Equipment Combination
			AddModuleIf(!ProductivityWiseModeEnabled && ObjectFactory.Get<ITransportRegistry>().EnableEquipmentCombination.Value, section, ModuleIDs.RefJobEquipment);

			// Domestic Transport Bookings
			AddModuleIf(!ProductivityWiseModeEnabled, section, ModuleIDs.DtbBookingTmpl);

			AddModuleIf(!ProductivityWiseModeEnabled, section, ModuleIDs.JobMawb);

			AddModuleIf(!ProductivityWiseModeEnabled && CurrentCountry == Constants.CountryCodes.UnitedStates, section, ModuleIDs.RefNMFC);
			AddModuleIf(!ProductivityWiseModeEnabled && CurrentCountry == Constants.CountryCodes.Mexico, section, ModuleIDs.RefNMFC);
			AddModuleIf(!ProductivityWiseModeEnabled && CurrentCountry == Constants.CountryCodes.Canada, section, ModuleIDs.RefNMFC);

			AddModuleIf(!ProductivityWiseModeEnabled && ReferenceFilesDataRegistry.Instance.EnableShippingLineReferenceFile.Value, section, ModuleIDs.RefShippingLine);
			AddModuleIf(!ProductivityWiseModeEnabled, section, ModuleIDs.RefComplianceList);
			AddModuleIf(!ProductivityWiseModeEnabled && ComplianceRiskHelper.IsMasterEnabledComplianceWise, section, ModuleIDs.RefComplianceCommodityAlert);
			AddModuleIf(!ProductivityWiseModeEnabled, section, ModuleIDs.RefPackType);
			AddModuleIf(!ProductivityWiseModeEnabled, section, ModuleIDs.RefPremisesGateCode);
			AddModuleIf(!ProductivityWiseModeEnabled, section, ModuleIDs.ServiceLevel);
			AddModuleIf(!ProductivityWiseModeEnabled, section, ModuleIDs.RefVessel);
			AddModuleIf(!ProductivityWiseModeEnabled, section, ModuleIDs.RefCarrierConsortium);
			AddModuleIf(!ProductivityWiseModeEnabled, section, ModuleIDs.RefFacility);

			AddModule(section, ModuleIDs.RefFilesReports);

			AddModuleIf(!ProductivityWiseModeEnabled, section, ModuleIDs.LDaaSDevices);
			AddModuleIf(!ProductivityWiseModeEnabled, section, ModuleIDs.TelematicsPreDriveChecklistTemplates);
		}

		#endregion

		#region Master Data

		void InitialiseMasterDataSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.Organisation);
			AddModule(section, ModuleIDs.OrgContacts);
			AddModulesIf(!ProductivityWiseModeEnabled && SystemDataRegistry.Instance.MdmAdministrationPanelEnabled.Value, section, ModuleIDs.AdministrationPanel);
			AddModule(section, ModuleIDs.MasterDataReports);
			AddModulesIf(SystemDataRegistry.Instance.PersonIntelligenceModuleEnabled.Value, section,
				ModuleIDs.GlbPerson);
		}

		#endregion

		#region Sales Manager

		void IntialiseRelationshipManagerConfigSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.SalesProduct);
			AddModule(section, ModuleIDs.SalesTeam);
		}

		#endregion

		#region Buffer Management

		void InitialiseBufferManagementConfigSection(ModuleSection section)
		{
			var isBufferManagementWorkflowModeOrBetterEnabled = IsBufferManagementWorkflowModeOrBetterEnabled;
			var displayResponsiveReleaseGateUiSettings = ObjectFactory.Get<IBMSRegistry>().DisplayResponsiveReleaseGateUiSettings;

			AddModule(section, ModuleIDs.BMSystems);
			AddModule(section, ModuleIDs.BMBoard);
			AddModule(section, ModuleIDs.BMBoardSlideshow);
			AddModule(section, ModuleIDs.BMReports);
			AddModule(section, ModuleIDs.BMControlCustomisation);
			AddModule(section, ModuleIDs.BMTagDefinition);
			AddModuleIf(isBufferManagementWorkflowModeOrBetterEnabled, section, ModuleIDs.BMTagRule);
			AddModuleIf(isBufferManagementWorkflowModeOrBetterEnabled, section, ModuleIDs.AcceptabilityBand);
			AddModuleIf(isBufferManagementWorkflowModeOrBetterEnabled, section, ModuleIDs.MENTAgedScoreQuery);
			AddModuleIf(isBufferManagementWorkflowModeOrBetterEnabled, section, ModuleIDs.ComponentRelationship);
			AddModuleIf(displayResponsiveReleaseGateUiSettings, section, ModuleIDs.BMBufferTimespan);
		}

		#endregion

		#region Locations

		void InitialiseLocationsSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.RefCountry);
			AddModule(section, ModuleIDs.RefCountryStates);
			AddModule(section, ModuleIDs.RefCityTown);
			AddModule(section, ModuleIDs.RefPostCode);
			AddModuleIf(!ProductivityWiseModeEnabled && OceanCarrierDataRegistry.Instance.EnableOceanCarrierSolution.Value, section, ModuleIDs.RouteSegments);
			AddModule(section, ModuleIDs.GenShapeGeography);
			AddModule(section, ModuleIDs.RefUNLOCO);
			AddModuleIf(!ProductivityWiseModeEnabled, section, ModuleIDs.InternationalZone);
			AddModule(section, ModuleIDs.RefTimeZoneSet);
			AddModuleIf(!ProductivityWiseModeEnabled, section, ModuleIDs.RateTransportProvider);
			AddModuleIf(!ProductivityWiseModeEnabled, section, ModuleIDs.TradeLane);

			AddModuleIf(!ProductivityWiseModeEnabled && (CurrentCountry == Constants.CountryCodes.UnitedStates || CurrentCountry == Constants.CountryCodes.Canada), section,
				ModuleIDs.RefDomesticCartageZone);

			AddModuleIf(!ProductivityWiseModeEnabled, section, ModuleIDs.GlbPortDeliveryTime);
			AddModuleIf(!ProductivityWiseModeEnabled && HVLVDataRegistry.Instance.PortCarrierDepotSelectionModule.Value, section, ModuleIDs.PortDepotCarrierSelection);
			AddModuleIf(!ProductivityWiseModeEnabled && !HVLVDataRegistry.Instance.PortCarrierDepotSelectionModule.Value, section, ModuleIDs.PortHubSelection);

			AddModuleIf(!ProductivityWiseModeEnabled, section, ModuleIDs.RefTransitTime);
			AddModule(section, ModuleIDs.LocationsReports);
			AddModule(section, ModuleIDs.CountryStatesGlbHoliday);
		}

		#endregion

		#region Accounts

		protected virtual bool IsCurrentCompanyGSTRegistered
		{
			get { return Env.CurrentCompany.IsGSTRegistered; }
		}

		protected virtual bool IsCurrentCompanyWHTRegistered
		{
			get { return Env.CurrentCompany.IsWHTRegistered; }
		}

		void InitialiseAccountConfigSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.AccGroups);

			AddModulesIf(IsCurrentCompanyGSTRegistered, section,
				ModuleIDs.AccTaxRate,
				ModuleIDs.AccInvMsg);

			AddModuleIf(IsCurrentCompanyWHTRegistered, section,
				ModuleIDs.AccWithholding);

			AddModule(section, ModuleIDs.AccChargeCode);
			AddModule(section, ModuleIDs.AccGlobalChargeCode);

			AddModuleIf(ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetTaxFrameworkConfigurationHelper().HasAnyAccTaxConfiguration(new BusinessObjectFactory(), GlbCompany.CurrentCompany),
				section,
				ModuleIDs.AccOrgTaxConfigurationTemplate);

			AddModuleIf(IsCurrentCompanyGSTRegistered, section,
				ModuleIDs.AccTaxOverrideGroup);
			AddModule(section, ModuleIDs.GlobalChargeCodeIntercompany);
			AddModule(section, ModuleIDs.GlobalChargeCodeOrganization);
			AddModule(section, ModuleIDs.AccApportionmentTemplate);
			AddModule(section, ModuleIDs.AccBankAccount);
			AddModule(section, ModuleIDs.AccChequeBook);

			var currentCompanyCountry = GlbCompany.CurrentCompany.Country;
			AddModuleIf(currentCompanyCountry != null && currentCompanyCountry.HasAccComplianceSequence, section,
				ModuleIDs.AccComplianceSequence);

			AddModuleIf(!ObjectFactory.Get<IAccounting>().IsENettOrganisation(ZGuid.Empty), section,
				ModuleIDs.ComPayRegisteredOrganisations);

			AddModule(section, ModuleIDs.AccGLHeader);
			AddModule(section, ModuleIDs.AccGLAccountDescriptor);
			AddModule(section, ModuleIDs.OrgCreditorGroup);
			AddModule(section, ModuleIDs.OrgDebtorGroup);
			AddModule(section, ModuleIDs.CsvAccountsImport);
			AddModule(section, ModuleIDs.ImportAccountingData);
			AddModule(section, ModuleIDs.AccountReports);
			AddModule(section, ModuleIDs.JobBillingExRateSysConfig);

			AddModuleIf(PlaceOfSupplyHelper.IsPlaceOfSupplyEnabled(GlbCompany.CurrentCompany), section,
				ModuleIDs.AccPlaceOfSupplyChargeCodeGroup);
		}

		void InitialiseReportingBooksConfigSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.AlternateChartofAccounts);
			AddModule(section, ModuleIDs.AlternateGLAccounts);
			AddModule(section, ModuleIDs.AccReportingBook);
		}

		#endregion

		#region Customs

		void InitialiseCustomsConfigSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.Customs.SG.SG4Classification);

			AddModuleIf(
				CurrentCountry != Constants.CountryCodes.Singapore &&
					CurrentCountry != Constants.CountryCodes.Australia &&
					CurrentCountry != Constants.CountryCodes.Canada &&
					CurrentCountry != Constants.CountryCodes.UnitedStates &&
					CurrentCountry != Constants.CountryCodes.China,
				section,
				ModuleIDs.SingleTariffClassification);

			// For Multi Tariff Customs
			AddModulesIf(Constants.CountryCodes.GetCustomsCountryOfJurisdiction(CurrentCountry) == Constants.CountryCodes.UnitedStates, section,
				ModuleIDs.Customs.US.USImportClassification,
				ModuleIDs.Customs.US.USExportClassification);
			AddModulesIf(Constants.CountryCodes.GetCustomsCountryOfJurisdiction(CurrentCountry) != Constants.CountryCodes.UnitedStates, section,
				ModuleIDs.ImportClassification,
				ModuleIDs.ExportClassification);

			AddModule(section, ModuleIDs.ExportTariffBulkChange);
			AddModule(section, ModuleIDs.ImportTariffBulkChange);
			AddModuleIf(IsSelfManagedTariffCountry, section, ModuleIDs.Customs.TradeGroups);

			// General Customs + Overloads
			AddModule(section, ModuleIDs.Customs.RefPacks);
			AddModuleIf(CurrentCountry == Constants.CountryCodes.Australia, section, ModuleIDs.Customs.CusCalculationRules);
			AddModule(section, ModuleIDs.Customs.Guarantees);
			AddModuleIf((Constants.CountryCodes.GetCustomsCountryOfJurisdiction(CurrentCountry) == Constants.CountryCodes.UnitedStates || CurrentCountry == Constants.CountryCodes.Canada), section, ModuleIDs.Customs.CustomsRules);
			AddModule(section, ModuleIDs.SupplierPart);
			AddModule(section, ModuleIDs.TariffBulkChange);
			AddModule(section, ModuleIDs.SendTestCustomsMessage);
			AddModuleIf(CurrentCountry == Constants.CountryCodes.SouthAfrica, section, ModuleIDs.Customs.Permits);
			AddModuleIf(CurrentCountry == Constants.CountryCodes.Canada, section, ModuleIDs.Customs.Permits);
			AddModuleIf(CurrentCountry == Constants.CountryCodes.Switzerland, section, ModuleIDs.Customs.Permits);
			AddModuleIf((Constants.CountryCodes.GetCustomsCountryOfJurisdiction(CurrentCountry)) == Constants.CountryCodes.UnitedStates, section, ModuleIDs.Customs.Permits);
			AddModuleIf(ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(Constants.CountryCodes.GetCustomsCountryOfJurisdiction(CurrentCountry)), section, ModuleIDs.Customs.Permits);
			AddModuleIf(IsSelfManagedTariffCountry, section, ModuleIDs.Customs.CusRefPreference);
			AddModuleIf(IsSelfManagedTariffCountry, section, ModuleIDs.Customs.CusRefRateCode);

			// CA
			AddModule(section, ModuleIDs.Customs.CA.HTSTariffBulkChange);
			AddModule(section, ModuleIDs.Customs.CA.CAExportClassification);
			AddModule(section, ModuleIDs.Customs.CA.HTSClassification);
			AddModule(section, ModuleIDs.Customs.CA.CACusRuling);

			// US
			AddModule(section, ModuleIDs.Customs.US.InBondNumber);
			AddModule(section, ModuleIDs.Customs.US.USTariffBulkChange);

			//NO
			AddModuleIf(CurrentCountry == Constants.CountryCodes.Norway && IsTemporaryStorageEnabledForNorway, section, ModuleIDs.Customs.NO.TemporaryStorageRegister);

			// NZ
			AddModule(section, ModuleIDs.Customs.NZ.Concession);

			// EU
			if (ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsCountryEuOrCtCountry(Constants.CountryCodes.GetCustomsCountryOfJurisdiction(CurrentCountry)))
			{
				AddModule(section, ModuleIDs.Customs.CusAuthorisations);
				AddModule(section, ModuleIDs.Customs.CusCalculationRules);
			}

			// ES
			AddModuleIf(CurrentCountry == Constants.CountryCodes.Spain && IsTemporaryStorageRegisterEnabled, section, ModuleIDs.Customs.EU.ES.TemporaryStorageRegister);
			AddModuleIf(CurrentCountry == Constants.CountryCodes.Spain && IsTemporaryStorageRegisterEnabled, section, ModuleIDs.Customs.EU.TempStoragePremises);

			// DE
			AddModuleIf(CurrentCountry == Constants.CountryCodes.Germany, section, ModuleIDs.Customs.EU.DE.SumARegister);

			// FR
			AddModuleIf(Core.Constants.CountryCodes.IsUnderFrenchCustomsJurisdiction(CurrentCountry), section, ModuleIDs.Customs.EU.TempStorageRegister);

			//IT
			AddModuleIf(CurrentCountry == Constants.CountryCodes.Italy && ObjectFactory.Get<Integration.Customs.Shared.ITemporaryStorageSettings>().IsUsingUCC6, section, ModuleIDs.Customs.EU.TempStorageRegister);

			//BR
			AddModuleIf(CurrentCountry == Constants.CountryCodes.Brazil && ObjectFactory.Get<Integration.Customs.BR.IBRCustomsDataRegistry>().EnableLPCO, section, ModuleIDs.Customs.BR.LPCO);
			AddModuleIf(CurrentCountry == Constants.CountryCodes.Brazil && ObjectFactory.Get<Integration.Customs.BR.IBRCustomsDataRegistry>().EnableCatalogModule, section, ModuleIDs.Customs.GoodsCatalog);
			AddModuleIf(CurrentCountry == Constants.CountryCodes.Brazil && ObjectFactory.Get<Integration.Customs.BR.IBRCustomsDataRegistry>().EnableForeignOperator, section, ModuleIDs.Customs.BR.ForeignOperator);

			// General
			AddModuleIf(GlbStaff.CurrentUser.GS_IsDeveloper, section, ModuleIDs.Customs.ImportCustomsFilesData);

			AddModule(section, ModuleIDs.CustFilesReports);
			AddModule(section, ModuleIDs.Customs.Universal.RefCusTariff);
			AddModuleIf(IsSelfManagedTariffCountry, section, ModuleIDs.Customs.CusRefTariffVersion);

			AddModule(section, ModuleIDs.Customs.Universal.ZZRefCusMap);

			AddModule(section, ModuleIDs.Customs.Universal.ZZRefCusCodeList);
			AddModule(section, ModuleIDs.Customs.Universal.ZZRefCarrier);
			AddModule(section, ModuleIDs.Customs.Universal.RefHarbourRate);
		}

		bool IsSelfManagedTariffCountry => ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().IsSelfManagedTariffCountry(CurrentCountry);

		bool IsTemporaryStorageEnabledForNorway => ObjectFactory.Get<Integration.Customs.NO.INOTemporaryStorageRegistry>().IsTemporaryStorageRegisterEnabled;

		#endregion

		#region CA Customs

		void InitialiseCACustomsMain(ModuleSection section)
		{
			AddModule(section, ModuleIDs.Customs.CA.ExportTariff);
			AddModule(section, ModuleIDs.Customs.CA.SubLocation);
			AddModule(section, ModuleIDs.Customs.CA.CATransactionNumberSetting);
		}

		#endregion

		#region US Customs

		void InitialiseUSCustomsMain(ModuleSection section)
		{
			bool isCurrentCountryUnderUSCustomsOfJurisdiction = IsCurrentCountryUnderUSCustomsOfJurisdiction;

			AddModulesIf(isCurrentCountryUnderUSCustomsOfJurisdiction, section,
				ModuleIDs.Customs.US.USCACCase,
				ModuleIDs.Customs.US.AffirmationOfCompliance);

			AddModule(section, ModuleIDs.Customs.US.Carrier);

			AddModulesIf(isCurrentCountryUnderUSCustomsOfJurisdiction, section,
				ModuleIDs.Customs.US.Country,
				ModuleIDs.Customs.US.USCDataVersion);

			AddModule(section, ModuleIDs.Customs.US.FIRMS);

			AddModulesIf(isCurrentCountryUnderUSCustomsOfJurisdiction, section,
				ModuleIDs.Customs.US.Quota);

			AddModule(section, ModuleIDs.Customs.US.Tariff);

			AddModulesIf(isCurrentCountryUnderUSCustomsOfJurisdiction, section,
				ModuleIDs.Customs.US.USCRule,
				ModuleIDs.Customs.US.USCTariffRule,
				ModuleIDs.Customs.US.TeamSpecialist,
				ModuleIDs.Customs.US.Visa,
				ModuleIDs.Customs.US.TariffRequiringVisa);
		}

		#endregion

		#region CO Customs

		void InitialiseCOCustomsMain(ModuleSection section)
		{
			AddModule(section, ModuleIDs.Customs.CO.DocumentIDs);
		}

		#endregion

		#region Warehouse

		protected void InitialiseWarehouseConfigSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.WhsConfigWarehouse);
			AddModule(section, ModuleIDs.WhsConfigRow);
			AddModule(section, ModuleIDs.WhsConfigArea);
			AddModule(section, ModuleIDs.WhsConfigProduct);
			AddModule(section, ModuleIDs.WhsCartonSize);
			AddModule(section, ModuleIDs.WhsCartonGroup);
			AddModule(section, ModuleIDs.WhsConfigProductStyle);
			AddModule(section, ModuleIDs.WhsConfigLocationType);

			AddModule(section, ModuleIDs.WhsConfigDynamicPickFaces);
			AddModule(section, ModuleIDs.WhsConfigPickFaces);
			AddModule(section, ModuleIDs.WhsConfigPutawayGroup);
			AddModule(section, ModuleIDs.WhsSalesChannel);
			AddModule(section, ModuleIDs.WhsProductionRulesPortal);
		}

		#endregion

		#region Human Resources

		protected void InitalisePeopleOperationsSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.HRReports);
			AddModule(section, ModuleIDs.HRGlbCompanyCampaign);
			AddModuleIf(IsHRMSEnabled, section, ModuleIDs.GlowHRMS);
		}

		protected void InitaliseLearningDevelopmentSection(ModuleSection section)
		{
			AddModulesIf(IsExamsModuleEnabled, section, ModuleIDs.ExamSetting);
			AddModulesIf(IsAccreditationModuleEnabled, section, ModuleIDs.GlbAccreditation, ModuleIDs.GlbAccreditationAttempt, ModuleIDs.GlbAccreditationGroup);
			AddModulesIf(IsLearningCentreModuleEnabled, section, ModuleIDs.LearningCentreCampaign);
		}

		protected void InitaliseRecruiterConfigSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.HRJobOpenings);
			AddModule(section, ModuleIDs.HRJobApplicant);
			AddModule(section, ModuleIDs.HRJobApplication);
			AddModule(section, ModuleIDs.HRJobRole);
			AddModule(section, ModuleIDs.HREmails);

			AddModuleIf(RecruitmentModuleEnabled, section, ModuleIDs.RecruitmentCandidateManagement);
		}

		#endregion

		#region Workflow

		void InitialiseOperationsWorkflowSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.ProcessTasks);
			AddModule(section, ModuleIDs.WorkflowExceptions);
			AddModule(section, ModuleIDs.ProcessMgrReports);
			AddModuleIf(IsAnyLevelOfBufferManagementEnabled, section, ModuleIDs.ProcessHeader);
			AddModuleIf(ExternalRequestFeatureHelper.IsExternalRequestEnabled(), section, ModuleIDs.ExternalRequests);
		}

		void InitialiseWorkflowPlanningSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.NetworkDiagram);
			AddModule(section, ModuleIDs.WorkQueues);
			AddModuleIf(ReleaseSequencesModuleEnabled, section, ModuleIDs.BMReleaseSequence);
		}

		void InitialiseWorkflowSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.WorkflowExceptionTypes);
			AddModule(section, ModuleIDs.ProcessTemplates);
			AddModule(section, ModuleIDs.ProcessCompanyLinkRule);
			AddModule(section, ModuleIDs.Events);
			AddModule(section, ModuleIDs.GenCustomAddOnRule);
			AddModule(section, ModuleIDs.ProcessFieldChangeRule);
			AddModuleIf(ExternalRequestFeatureHelper.IsExternalRequestEnabled(), section, ModuleIDs.ExternalRequestTypes);
			AddModuleIf(ExternalRequestFeatureHelper.IsExternalRequestEnabled(), section, ModuleIDs.ExternalRequestInfoTemplate);
		}

		void InitialiseEDIMessagingSection(ModuleSection section)
		{
			AddModuleIf(ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(CargoWise.Definitions.LicenceFeatureCodeList.Codes.EAdaptorNextFeature) != null, section, ModuleIDs.Messaging.EDICommunicationsMode);
			AddModule(section, ModuleIDs.Messaging.EDIInterchange);
			AddModule(section, ModuleIDs.Messaging.EDIMessage);
			AddModule(section, ModuleIDs.Messaging.EDIMessagePurpose);
			AddModule(section, ModuleIDs.Messaging.EDIMessageContentFilter);
			AddModule(section, ModuleIDs.Messaging.EDIMessageDeliveryContext);
			AddModuleIf(ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(CargoWise.Definitions.LicenceFeatureCodeList.Codes.EAdaptorNextFeature) != null, section, ModuleIDs.Messaging.EDICommunicationParty);
			AddModuleIf(OrganisationsDataRegistry.Instance.EnableEDICodeMappingModule.Value, section, ModuleIDs.Messaging.EDICodeMapping);
			AddModule(section, ModuleIDs.Messaging.UniversalValidationRule);
		}

		#endregion

		#region ArchiveManager

		void InitialiseArchiveManagerSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.ArchiveSchedule);
			AddModule(section, ModuleIDs.ArchivedRecords);
			AddModule(section, ModuleIDs.ArchiveReports);
		}

		#endregion

		#region System

		void InitialisePrintingSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.PrintJob);
			AddModule(section, ModuleIDs.PrintQueue);
			AddModule(section, ModuleIDs.DocumentSigningJob);
		}

		void InitialiseEmailSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.MailItem);
			AddModule(section, ModuleIDs.MailItemTemplate);
		}

		void InitialiseReportSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.SystemReports);
			AddModule(section, ModuleIDs.ReportStatistics);
			AddModule(section, ModuleIDs.ReportManagement);
			AddModule(section, ModuleIDs.ScheduledReports);
			AddModuleIf(IsSupportOrClientEDI, section, ModuleIDs.ErrorReporting);
		}

		void InitialiseSystemSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.ServiceRequest);
			AddModule(section, ModuleIDs.UniversalCopySchedule);
			AddModule(section, ModuleIDs.LicenceUsage);
			AddModule(section, ModuleIDs.Registry);
			AddModule(section, ModuleIDs.StmServiceTask);
			AddModule(section, ModuleIDs.ProcessController);
			AddModule(section, ModuleIDs.StmUpgrade);
			AddModuleIf(IsFeatureTestingEnabled, section, ModuleIDs.StmFeatureTest);
			AddModule(section, ModuleIDs.UpdateNotesPortal);

			AddModule(section, ModuleIDs.LocalLanguages);
			AddModule(section, ModuleIDs.TranslationFeedback);
			AddModuleIf(Globals.IsDebugMode || TranslationFeedbackConfiguration.IsMasterDatabase, section, ModuleIDs.ResourceStrings);
		}

		void InitialiseUserSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.ActiveUsers);
			AddModule(section, ModuleIDs.GlbBranch);
			AddModule(section, ModuleIDs.GlbCompany);
			AddModule(section, ModuleIDs.GlbDepartment);
			AddModule(section, ModuleIDs.GlbGroup);
			AddModule(section, ModuleIDs.GlbStaff);
			AddModule(section, ModuleIDs.GlbCapability);
			AddModule(section, ModuleIDs.DialogDefault);
			AddModule(section, ModuleIDs.UserAdminReports);
		}

		#endregion

		#region Tariffs & Rates

		void InitialiseTariffsAndRatesConfigSection(ModuleSection section)
		{
			AddModule(section, ModuleIDs.RateAttachmentSet);
		}

		#endregion

		#endregion

		#region Implementation

		static bool IsHRMSEnabled => GlowRegistry.GetRestrictedPortalsOverride().Contains("HRM", StringComparer.OrdinalIgnoreCase);

		static bool IsFeatureTestingEnabled => DataRegistry.Instance.FeatureTestModeEnabled;
		static bool IsAnyLevelOfBufferManagementEnabled => ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled;

		static bool IsPlanningManagementEnabled => ObjectFactory.Get<IBMSRegistry>().IsPlanningManagementEnabled;

		static bool IsBufferManagementWorkflowModeOrBetterEnabled => ObjectFactory.Get<IBMSRegistry>().IsBufferManagementWorkflowModeOrBetterEnabled;

		static bool ProductivityWiseModeEnabled => DataRegistry.Instance.ProductivityWiseModeEnabled;

		static bool RecruitmentModuleEnabled => ObjectFactory.Get<IRecruitmentRegistry>().RecruitmentModuleEnabled;

		static bool IsExamsModuleEnabled => ObjectFactory.Get<IRecruitmentRegistry>().IsExamsModuleEnabled;

		static bool IsAccreditationModuleEnabled => ObjectFactory.Get<IRecruitmentRegistry>().IsAccreditationModuleEnabled;

		static bool IsLearningCentreModuleEnabled => ObjectFactory.Get<IRecruitmentRegistry>().IsLearningCentreModuleEnabled;

		static bool ReleaseSequencesModuleEnabled => ObjectFactory.Get<IBMSRegistry>().ReleaseSequencesModuleEnabled;

		static bool NettingEnabled
		{
			get { return AccountingMasterFilesRegistry.Instance.EnableNetting.Value && (bool)ObjectFactory.Get<IAccounting>().Registry.IsNettingSystem.Value; }
		}

		static bool IsSupportOrClientEDI =>
			// EDISupport has no support user, so it must be visible here.
			(Env.CurrentUser != null && Env.CurrentUser.IsSupportUser)
			|| ClientHookLoader.Instance.Client == Clients.EDI;

		#endregion

		#endregion

		#endregion
	}
}
