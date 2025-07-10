using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Web;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using EventSortOrderList = Enterprise.Registry.Business.Web.EventSortOrderList;
using SharedGlowRegistry = CargoWise.Definitions.GlowRegistry;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(WebDataRegistry))]
	class WebDataRegistryTest : RegistryItemSetTestCaseWithFactory<WebDataRegistry>
	{
		#region Web Services

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Testing")]
		public void TestRootServicesUri()
		{
			var registration = ObjectFactory.Get<IProductRegistration>();
			TestRegistryItem(
				ItemSet.RootServicesUri,
				SharedGlowRegistry.EnterpriseServicesRootUriKey,
				WebDataRegistry.WebServicesCategory,
				$"{Core.Constants.ProductName} Services Root URL",
				$"The URL to the place where {Core.Constants.ProductName} Services are installed.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.PreserveTestValue,
				TextEditorType.TextBox,
				expectedDefaultValue: SharedGlowRegistry.CalculateEnterpriseServicesDefaultUri(registration.Key.EnterpriseCode, registration.Key.ServerCode, registration.IsWiseTechGlobalInternalSystem()).AbsoluteUri,
				testValueToSetAndRead: "https://localhost/");

			AssertType<UriRegistryDataType>(ItemSet.RootServicesUri.DataType);

			var dataType = (UriRegistryDataType)ItemSet.RootServicesUri.DataType;
			AssertEquals(false, dataType.AllowAutoProtocolPrefixing);
		}

		public void TestRootServicesUri_DefaultValue_InDemoCompany()
		{
			using (Env.SetTemporaryUserContext(User.SupportUserName, GetFirstDemoCompanyBranchPK(), Guid.Empty))
			{
				var defaultValue = ItemSet.RootServicesUri.DefaultValue;
				AssertEquals("https://svc-edidat.sand.wtg.zone/Services/", defaultValue);
			}
		}

		internal static Guid GetFirstDemoCompanyBranchPK()
		{
			using (var command = Db.Connection.Command("SELECT TOP(1) GB_PK FROM dbo.GlbBranch JOIN dbo.GlbCompany ON GB_GC = GC_PK WHERE GC_Code = @code"))
			{
				command.AddParameterBasedOnDbColumn("@code", "DEM", GlbCompanySchema.GC_Code);
				return (Guid)command.ExecuteScalar();
			}
		}

		public void TestRootServicesUri_DataType()
		{
			AssertType(typeof(UriRegistryDataType), ItemSet.RootServicesUri.DataType);

			var dataType = (UriRegistryDataType)ItemSet.RootServicesUri.DataType;
			AssertEquals(false, dataType.AllowAutoProtocolPrefixing);
		}

		#endregion

		public void TestNonCachedItemsValueSharedBetweenThreads()
		{
			using ((Db.Instance as IDbConnectionMultiThreadControl).UseMainConnectionAcrossAllThreadsForTests())
			{
				WebDataRegistryTest.ReadyForTests = false;
				WebDataRegistryTest.StartTests = false;

				var cts = new CancellationTokenSource();
				var task = Task.Factory.StartNew(() => { ThreadTestingProcess(cts.Token); }, cts.Token);
				while (true)
				{
					if (WebDataRegistryTest.ReadyForTests)
					{
						AssertEquals("ShowDetailedInventory should be false", false, WebDataRegistry.Instance.GetShowDetailedInventory(WebDataRegistryTest.RegistryOwnerPK));
						AssertEquals("ShowDetailedOrderLine should be false", false, WebDataRegistry.Instance.GetShowDetailedOrderLines(WebDataRegistryTest.RegistryOwnerPK));

						WebDataRegistry.Instance.SetShowDetailedInventory(WebDataRegistryTest.RegistryOwnerPK, true);
						WebDataRegistry.Instance.SetShowDetailedOrderLines(WebDataRegistryTest.RegistryOwnerPK, true);

						AssertEquals("ShowDetailedInventory should be true", true, WebDataRegistry.Instance.GetShowDetailedInventory(WebDataRegistryTest.RegistryOwnerPK));
						AssertEquals("ShowDetailedOrderLine should be true", true, WebDataRegistry.Instance.GetShowDetailedOrderLines(WebDataRegistryTest.RegistryOwnerPK));

						WebDataRegistryTest.StartTests = true;

						while (!task.IsCompleted)
						{
							if (!string.IsNullOrEmpty(ExceptionMessage))
							{
								cts.Cancel();
								break;
							}
						}
						break;
					}
				}

				Assert(ExceptionMessage, string.IsNullOrEmpty(ExceptionMessage));
			}

			var count = ExceptionReporterTestListener.Instance.Count;
			AssertEquals("There should only be the thread sentry error in the exception reporter", 1, count);
			var exception = ExceptionReporterTestListener.Instance[0];
			AssertEquals(typeof(CrossThreadAccessException), exception.GetBaseException().GetType());
			ExceptionReporterTestListener.Instance.Clear();
		}

		#region Properties for Thread Testing

		public static string ExceptionMessage
		{
			get
			{
				lock (staticLock)
				{
					return exceptionMessage;
				}
			}
			set
			{
				lock (staticLock)
				{
					exceptionMessage = value;
				}
			}
		}
		static string exceptionMessage = "";

		public static bool ReadyForTests
		{
			get
			{
				lock (staticLock)
				{
					return readyForTests;
				}
			}
			set
			{
				lock (staticLock)
				{
					readyForTests = value;
				}
			}
		}
		static bool readyForTests;

		public static bool StartTests
		{
			get
			{
				lock (staticLock)
				{
					return startTests;
				}
			}
			set
			{
				lock (staticLock)
				{
					startTests = value;
				}
			}
		}
		static bool startTests;

		public static ZGuid RegistryOwnerPK
		{
			get
			{
				lock (staticLock)
				{
					return new ZGuid("ACCE4D7F-CD5D-4377-9BF0-A7A515CB10E4");
				}
			}
		}

		static readonly object staticLock = new object();

		#endregion

		protected static void ThreadTestingProcess(CancellationToken token)
		{
			WebDataRegistry.Instance.SetShowDetailedInventory(WebDataRegistryTest.RegistryOwnerPK, false);
			WebDataRegistry.Instance.SetShowDetailedOrderLines(WebDataRegistryTest.RegistryOwnerPK, false);

			if (WebDataRegistry.Instance.GetShowDetailedInventory(WebDataRegistryTest.RegistryOwnerPK))
			{
				WebDataRegistryTest.ExceptionMessage = "ShowDetailedInventory should be false";
			}
			if (WebDataRegistry.Instance.GetShowDetailedOrderLines(WebDataRegistryTest.RegistryOwnerPK))
			{
				WebDataRegistryTest.ExceptionMessage = "ShowDetailedOrderLine should be false";
			}

			WebDataRegistryTest.ReadyForTests = true;

			while (!token.IsCancellationRequested)
			{
				if (WebDataRegistryTest.StartTests)
				{
					if (!WebDataRegistry.Instance.GetShowDetailedInventory(WebDataRegistryTest.RegistryOwnerPK))
					{
						WebDataRegistryTest.ExceptionMessage = "ShowDetailedInventory should be true";
					}
					if (!WebDataRegistry.Instance.GetShowDetailedOrderLines(WebDataRegistryTest.RegistryOwnerPK))
					{
						WebDataRegistryTest.ExceptionMessage = "ShowDetailedOrderLine should be true";
					}

					break;
				}
			}
		}

		public void TestStandardItems()
		{
			TestRegistryItem(ItemSet.AllowToAddNewOrganisation, "AllowToAddNewOrganisation", WebDataRegistry.WebCategory, "Allow creation of new organizations", WebDataRegistry.HintForAllowToAddNewOrganisation, sysFlags, true);
			TestRegistryItem(ItemSet.ChoiceOfPasswordSetAndResetProcessFlowEnabled, "ChoiceOfPasswordSetAndResetProcessFlowEnabled", WebDataRegistry.WebCategory, "Enable Choice of Password Set/Reset Process Flow", @"When the ""Send Password Instructions"" button on the Organization > Contact > Web Security form is pressed, the user will be offered a choice of sending the Contact user to either WebTracker or to CargoWise Web Portals in order to complete the set or reset of their password. Following a successful change of password, the Contact user will then be directed to WebTracker or the CargoWise Web Portal catalog for subsequent login.", sysFlags, RegistryOptions.PreserveTestValue, true);
			TestGenericRegistryItem(ItemSet.DefaultCargoWiseWebPortal, "DefaultCargoWiseWebPortal", WebDataRegistry.WebCategory, "Default CargoWise Web Portal", WebDataRegistry.HintForDefaultCargoWiseWebPortal, RegistryStorageFlags.System);

			DefaultFilterLayoutTest(ItemSet.DefaultFilterLayoutForwardingOrders, "Forwarding Orders", WebDataRegistry.ForwardingOrdersCategory, "DefaultFilterLayoutForwardingOrders", "TrackingOrders", RegistryOptions.NotCached);
			DefaultFilterLayoutTest(ItemSet.DefaultFilterLayoutForwardingOrderLines, "Forwarding Order Lines", WebDataRegistry.ForwardingOrderLinesCategory, "DefaultFilterLayoutForwardingOrderLines", "TrackingOrderLines", RegistryOptions.NotCached);
			DefaultFilterLayoutTest(ItemSet.DefaultFilterLayoutForwardingBookings, "Forwarding Bookings", WebDataRegistry.ForwardingBookingsCategory, "DefaultFilterLayoutForwardingBookings", "TrackingBookings", RegistryOptions.NotCached);
			DefaultFilterLayoutTest(ItemSet.DefaultFilterLayoutISF, "Importer Security Filing", WebDataRegistry.CustomsISFCategory, "DefaultFilterLayoutISF", "TrackingImporterSecurityFiling", RegistryOptions.NotCached);
			DefaultFilterLayoutTest(ItemSet.DefaultFilterLayoutDeclaration, "Declaration", WebDataRegistry.CustomsDeclarationCategory, "DefaultFilterLayoutDeclaration", "TrackingDeclarations", RegistryOptions.NotCached);

			DefaultFilterLayoutTest(ItemSet.DefaultFilterLayoutForwardingShipments, "Forwarding Shipments", WebDataRegistry.ForwardingShipmentsCategory, "DefaultFilterLayoutForwardingShipments", "TrackingShipments", RegistryOptions.NotCached);
			DefaultFilterLayoutTest(ItemSet.DefaultFilterLayoutCFSShipments, "CFS Shipments", WebDataRegistry.CFSShipmentsCategory, "DefaultFilterLayoutCFSShipments", "CFSShipments", RegistryOptions.Default);
			DefaultFilterLayoutTest(ItemSet.DefaultFilterLayoutForwardingContainers, "Forwarding Containers", WebDataRegistry.ForwardingContainersCategory, "DefaultFilterLayoutForwardingContainers", "TrackingContainers", RegistryOptions.NotCached);
			DefaultFilterLayoutTest(ItemSet.DefaultFilterLayoutLinerAndAgencyContainers, "Liner & Agency Containers", WebDataRegistry.LinerAndAgencyContainersCategory, "DefaultFilterLayoutLinerAndAgencyContainers", "LinerAndAgencyContainers", RegistryOptions.NotCached);
			DefaultFilterLayoutTest(ItemSet.DefaultFilterLayoutForwardingFlightSchedules, "Forwarding Flight Schedules", WebDataRegistry.ForwardingFlightsCategory, "DefaultFilterLayoutForwardingFlightSchedules", "TrackingFlightSchedules", RegistryOptions.NotCached);
			DefaultFilterLayoutTest(ItemSet.DefaultFilterLayoutForwardingSailingSchedules, "Forwarding Sailing Schedules", WebDataRegistry.ForwardingSailingsCategory, "DefaultFilterLayoutForwardingSailingSchedules", "TrackingSailingSchedules", RegistryOptions.NotCached);
			DefaultFilterLayoutTest(ItemSet.DefaultFilterLayoutForwardingRoadSchedules, "Forwarding Road Schedules", WebDataRegistry.ForwardingRoadCategory, "DefaultFilterLayoutForwardingRoadSchedules", "TrackingRoadSchedules", RegistryOptions.NotCached);
			DefaultFilterLayoutTest(ItemSet.DefaultFilterLayoutForwardingRailSchedules, "Forwarding Rail Schedules", WebDataRegistry.ForwardingRailCategory, "DefaultFilterLayoutForwardingRailSchedules", "TrackingRailSchedules", RegistryOptions.NotCached);
			DefaultFilterLayoutTest(ItemSet.DefaultFilterLayoutLinerAndAgencyBookings, "Liner & Agency Bookings", WebDataRegistry.LinerAndAgencyBookingsCategory, "DefaultFilterLayoutShippingBookings", "ShippingBookings", RegistryOptions.NotCached);
			DefaultFilterLayoutTest(ItemSet.DefaultFilterLayoutLinerAndAgencyBillsOfLading, "Liner & Agency Bills of Lading", WebDataRegistry.LinerAndAgencyBillsOfLadingCategory, "DefaultFilterLayoutShippingBillsOfLading", "ShippingBillsOfLading", RegistryOptions.NotCached);
			DefaultFilterLayoutTest(ItemSet.DefaultFilterLayoutWarehouses, "Warehouses", WebDataRegistry.WarehousesCategory, "DefaultFilterLayoutWarehouses", "TrackingWarehouse", RegistryOptions.NotCached);
			DefaultFilterLayoutTest(ItemSet.DefaultFilterLayoutWarehouseProducts, "Warehouse Products", WebDataRegistry.WarehouseProductsCategory, "DefaultFilterLayoutWarehouseProducts", "OrgSupplierPartWeb", RegistryOptions.NotCached);
			DefaultFilterLayoutTest(ItemSet.DefaultFilterLayoutWarehouseReceipts, "Warehouse Receipts", WebDataRegistry.WarehouseReceiptsCategory, "DefaultFilterLayoutWarehouseReceipts", "TrackingWarehouseReceive", RegistryOptions.NotCached);
			DefaultFilterLayoutTest(ItemSet.DefaultFilterLayoutWarehouseOrders, "Warehouse Orders", WebDataRegistry.WarehouseOrdersCategory, "DefaultFilterLayoutWarehouseOrders", "TrackingWarehouseOrders", RegistryOptions.NotCached);
			DefaultFilterLayoutTest(ItemSet.DefaultFilterLayoutWarehouseInventory, "Warehouse Inventory", WebDataRegistry.WarehouseInventoryCategory, "DefaultFilterLayoutWarehouseInventory", "TrackingInventory", RegistryOptions.NotCached);
			DefaultFilterLayoutTest(ItemSet.DefaultFilterLayoutForwardingQuotes, "Forwarding Quotes", WebDataRegistry.ForwardingQuotesCategory, "DefaultFilterLayoutForwardingQuotes", "TrackingQuotations", RegistryOptions.NotCached);
			DefaultFilterLayoutTest(ItemSet.DefaultFilterLayoutTrackingCartage, "Transport Jobs", WebDataRegistry.TransportJobsCategory, "DefaultFilterLayoutTrackingCartage", "TrackingCartage", RegistryOptions.NotCached);
			DefaultFilterLayoutTest(ItemSet.DefaultFilterLayoutMAWB, "MAWBs", WebDataRegistry.ForwardingMAWBCategory, "DefaultFilterLayoutMAWB", "TrackingMAWB", RegistryOptions.IsOnlyForDevelopers);
			DefaultFilterLayoutTest(ItemSet.DefaultFilterLayoutHAWB, "HAWBs", WebDataRegistry.ForwardingHAWBCategory, "DefaultFilterLayoutHAWB", "TrackingHAWB", RegistryOptions.IsOnlyForDevelopers);

			NotificationOptionsTest(WebDataRegistry.Instance.BookingNotificationOptions, "Forwarding Bookings", WebDataRegistry.ForwardingBookingsCategory, "BookingNotificationOptions");
			NotificationOptionsTest(WebDataRegistry.Instance.ISFNotificationOptions, "Importer Security Filing", WebDataRegistry.CustomsISFCategory, "ISFNotificationOptions");
			NotificationOptionsTest(WebDataRegistry.Instance.OrderNotificationOptions, "Forwarding Orders", WebDataRegistry.ForwardingOrdersCategory, "OrderNotificationOptions");
			NotificationOptionsTest(WebDataRegistry.Instance.QuoteNotificationOptions, "Forwarding Quotes", WebDataRegistry.ForwardingQuotesCategory, "QuoteNotificationOptions");
			NotificationOptionsTest(WebDataRegistry.Instance.ContainerNotificationOptions, "Forwarding Containers", WebDataRegistry.ForwardingContainersCategory, "ContainerNotificationOptions");
			NotificationOptionsTest(WebDataRegistry.Instance.LinerAndAgencyContainerNotificationOptions, "Liner & Agency Containers", WebDataRegistry.LinerAndAgencyContainersCategory, "LinerAndAgencyContainerNotificationOptions");
			NotificationOptionsTest(WebDataRegistry.Instance.LinerAndAgencyBookingNotificationOptions, "Liner & Agency Bookings", WebDataRegistry.LinerAndAgencyBookingsCategory, "ShippingBookingNotificationOptions");
			NotificationOptionsTest(WebDataRegistry.Instance.LinerAndAgencyBillOfLadingNotificationOptions, "Liner & Agency Forwarding Instructions", WebDataRegistry.LinerAndAgencyBillsOfLadingCategory, "ShippingBillOfLadingNotificationOptions");
			NotificationOptionsTest(WebDataRegistry.Instance.WarehouseOrdersNotificationOptions, "Warehouse Orders", WebDataRegistry.WarehouseOrdersCategory, "WarehouseOrdersNotificationOptions");
			NotificationOptionsTest(WebDataRegistry.Instance.WarehouseReceiptsNotificationOptions, "Warehouse Receipts", WebDataRegistry.WarehouseReceiptsCategory, "WarehouseReceiptsNotificationOptions");
			NotificationOptionsTest(WebDataRegistry.Instance.ShipmentNotificationOptions, "Forwarding Shipments", WebDataRegistry.ForwardingShipmentsCategory, "ShipmentNotificationOptions");
			NotificationOptionsTest(WebDataRegistry.Instance.TrackingCartageNotificationOptions, "Transport Jobs", WebDataRegistry.TransportJobsCategory, "TrackingCartageNotificationOptions");

			NotificationEmailGroupTest(WebDataRegistry.Instance.BookingNotificationEmailGroup, "Forwarding Bookings", WebDataRegistry.ForwardingBookingsCategory, "BookingNotificationEmailGroup", RegistryOptions.IsValueMandatory | RegistryOptions.PreserveTestValue);
			NotificationEmailGroupTest(WebDataRegistry.Instance.ISFNotificationEmailGroup, "Importer Security Filing", WebDataRegistry.CustomsISFCategory, "ISFNotificationEmailGroup", RegistryOptions.IsValueMandatory | RegistryOptions.PreserveTestValue);
			NotificationEmailGroupTest(WebDataRegistry.Instance.OrderNotificationEmailGroup, "Forwarding Orders", WebDataRegistry.ForwardingOrdersCategory, "OrderNotificationEmailGroup", RegistryOptions.IsValueMandatory | RegistryOptions.PreserveTestValue);
			NotificationEmailGroupTest(WebDataRegistry.Instance.QuoteNotificationEmailGroup, "Forwarding Quotes", WebDataRegistry.ForwardingQuotesCategory, "QuoteNotificationEmailGroup", RegistryOptions.IsValueMandatory | RegistryOptions.PreserveTestValue);
			NotificationEmailGroupTest(WebDataRegistry.Instance.ContainerNotificationEmailGroup, "Forwarding Containers", WebDataRegistry.ForwardingContainersCategory, "ContainerNotificationEmailGroup", RegistryOptions.IsValueMandatory | RegistryOptions.PreserveTestValue);
			NotificationEmailGroupTest(WebDataRegistry.Instance.LinerAndAgencyContainerNotificationEmailGroup, "Liner & Agency Containers", WebDataRegistry.LinerAndAgencyContainersCategory, "LinerAndAgencyContainerNotificationEmailGroup", RegistryOptions.IsValueMandatory | RegistryOptions.PreserveTestValue);
			NotificationEmailGroupTest(WebDataRegistry.Instance.LinerAndAgencyBookingNotificationEmailGroup, "Liner & Agency Bookings", WebDataRegistry.LinerAndAgencyBookingsCategory, "ShippingBookingNotificationEmailGroup", RegistryOptions.IsValueMandatory | RegistryOptions.PreserveTestValue);
			NotificationEmailGroupTest(WebDataRegistry.Instance.LinerAndAgencyBillOfLadingNotificationEmailGroup, "Liner & Agency Forwarding Instructions", WebDataRegistry.LinerAndAgencyBillsOfLadingCategory, "ShippingBillOfLadingNotificationEmailGroup", RegistryOptions.IsValueMandatory | RegistryOptions.PreserveTestValue);
			NotificationEmailGroupTest(WebDataRegistry.Instance.WarehouseOrdersNotificationEmailGroup, "Warehouse Orders", WebDataRegistry.WarehouseOrdersCategory, "WarehouseOrdersNotificationEmailGroup", RegistryOptions.IsValueMandatory | RegistryOptions.PreserveTestValue);
			NotificationEmailGroupTest(WebDataRegistry.Instance.WarehouseReceiptsNotificationEmailGroup, "Warehouse Receipts", WebDataRegistry.WarehouseReceiptsCategory, "WarehouseReceiptsNotificationEmailGroup", RegistryOptions.IsValueMandatory | RegistryOptions.PreserveTestValue);
			NotificationEmailGroupTest(WebDataRegistry.Instance.ShipmentNotificationEmailGroup, "Forwarding Shipments", WebDataRegistry.ForwardingShipmentsCategory, "ShipmentNotificationEmailGroup", RegistryOptions.IsValueMandatory | RegistryOptions.PreserveTestValue);
			NotificationEmailGroupTest(WebDataRegistry.Instance.TrackingCartageNotificationEmailGroup, "Transport Jobs", WebDataRegistry.TransportJobsCategory, "TrackingCartageNotificationEmailGroup", RegistryOptions.IsValueMandatory | RegistryOptions.PreserveTestValue);

			OrgsRolePropertySuppressionRegistryItemTest(WebDataRegistry.Instance.ISFOrgsRolePropertySuppression, "Importer Security Filing", WebDataRegistry.CustomsISFCategory, "ISFOrgsRolePropertySuppression", typeof(ISFAccessRules));

			StaffRolesToNotifyTest(WebDataRegistry.Instance.ISFNotificationStaffRoles, "Importer Security Filing", WebDataRegistry.CustomsISFCategory, "ISFNotificationStaffRoles", new[] { StaffAssignmentRoles.Codes.CustomsAgent, StaffAssignmentRoles.Codes.CustomerServiceRep });
			StaffRolesToNotifyTest(WebDataRegistry.Instance.BookingsNotificationStaffRoles, "Forwarding Bookings", WebDataRegistry.ForwardingBookingsCategory, "BookingsNotificationStaffRoles", StaffAssignmentRoles.Codes.SalesRep);
			StaffRolesToNotifyTest(WebDataRegistry.Instance.OrderNotificationStaffRoles, "Forwarding Orders", WebDataRegistry.ForwardingOrdersCategory, "OrderNotificationStaffRoles", StaffAssignmentRoles.Codes.CartageCoordinator);
			StaffRolesToNotifyTest(WebDataRegistry.Instance.QuoteNotificationStaffRoles, "Forwarding Quotes", WebDataRegistry.ForwardingQuotesCategory, "QuoteNotificationStaffRoles", StaffAssignmentRoles.Codes.SalesRep);
			StaffRolesToNotifyTest(WebDataRegistry.Instance.ContainerNotificationStaffRoles, "Forwarding Containers", WebDataRegistry.ForwardingContainersCategory, "ContainerNotificationStaffRoles", StaffAssignmentRoles.Codes.CartageCoordinator);
			StaffRolesToNotifyTest(WebDataRegistry.Instance.LinerAndAgencyContainerNotificationStaffRoles, "Liner & Agency Containers", WebDataRegistry.LinerAndAgencyContainersCategory, "LinerAndAgencyContainerNotificationStaffRoles", StaffAssignmentRoles.Codes.CartageCoordinator);
			StaffRolesToNotifyTest(WebDataRegistry.Instance.LinerAndAgencyBookingNotificationStaffRoles, "Liner & Agency Bookings", WebDataRegistry.LinerAndAgencyBookingsCategory, "ShippingBookingNotificationStaffRoles", StaffAssignmentRoles.Codes.SalesRep);
			StaffRolesToNotifyTest(WebDataRegistry.Instance.LinerAndAgencyBillOfLadingNotificationStaffRoles, "Liner & Agency Forwarding Instructions", WebDataRegistry.LinerAndAgencyBillsOfLadingCategory, "ShippingBillOfLadingNotificationStaffRoles", StaffAssignmentRoles.Codes.SalesRep);
			StaffRolesToNotifyTest(WebDataRegistry.Instance.WarehouseOrdersNotificationStaffRoles, "Warehouse Orders", WebDataRegistry.WarehouseOrdersCategory, "WarehouseOrdersNotificationStaffRoles", StaffAssignmentRoles.Codes.CartageCoordinator);
			StaffRolesToNotifyTest(WebDataRegistry.Instance.WarehouseReceiptsNotificationStaffRoles, "Warehouse Receipts", WebDataRegistry.WarehouseReceiptsCategory, "WarehouseReceiptsNotificationStaffRoles", StaffAssignmentRoles.Codes.CartageCoordinator);
			StaffRolesToNotifyTest(WebDataRegistry.Instance.ShipmentNotificationStaffRoles, "Forwarding Shipments", WebDataRegistry.ForwardingShipmentsCategory, "ShipmentNotificationStaffRoles", new[] { StaffAssignmentRoles.Codes.CartageCoordinator, StaffAssignmentRoles.Codes.CustomerServiceRep });

			SuppressionItemTest(WebDataRegistry.Instance.SuppressFlightDetailsForExport, "Suppress Export Flight Details", "SuppressFlightDetailsForExport", "export", RegistryOptions.PreserveTestValue);
			SuppressionItemTest(WebDataRegistry.Instance.SuppressFlightDetailsForImport, "Suppress Import Flight Details", "SuppressFlightDetailsForImport", "import");
			SuppressionItemTest(WebDataRegistry.Instance.SuppressFlightDetailsForDomestic, "Suppress Domestic Flight Details", "SuppressFlightDetailsForDomestic", "domestic");
			SuppressionItemTest(WebDataRegistry.Instance.SuppressFlightDetailsForForeign, "Suppress Foreign Flight Details", "SuppressFlightDetailsForForeign", "foreign");

			UseModuleTest(WebDataRegistry.Instance.UseWebAccountsModule, "Accounts", WebDataRegistry.AccountsCategory, "UseWebAccountsModule");
			UseModuleTest(WebDataRegistry.Instance.UseWebForwardingBookingsModule, "Forwarding Bookings", WebDataRegistry.ForwardingBookingsCategory, "UseWebForwardingBookingsModule");
			UseModuleTest(WebDataRegistry.Instance.UseWebISFModule, "Importer Security Filing", WebDataRegistry.CustomsISFCategory, "UseWebISFModule");
			UseModuleTest(WebDataRegistry.Instance.UseWebDeclarationModule, "Declaration", WebDataRegistry.CustomsDeclarationCategory, "UseWebDeclarationModule");
			UseModuleTest(WebDataRegistry.Instance.UseWebForwardingContainersModule, "Forwarding Containers", WebDataRegistry.ForwardingContainersCategory, "UseWebForwardingContainersModule");
			UseModuleTest(WebDataRegistry.Instance.UseWebLinerAndAgencyContainersModule, "Liner & Agency Containers", WebDataRegistry.LinerAndAgencyContainersCategory, "UseWebLinerAndAgencyContainersModule");
			UseModuleTest(WebDataRegistry.Instance.UseWebForwardingOrdersModule, "Forwarding Orders", WebDataRegistry.ForwardingOrdersCategory, "UseWebForwardingOrdersModule");
			UseModuleTest(WebDataRegistry.Instance.UseWebForwardingQuotesModule, "Forwarding Quotes", WebDataRegistry.ForwardingQuotesCategory, "UseWebForwardingQuotesModule");

			UseModuleTest(WebDataRegistry.Instance.UseWebForwardingReportsModule, "Forwarding Reports", WebDataRegistry.ForwardingReportsCategory, "UseWebForwardingReportsModule");
			UseModuleTest(WebDataRegistry.Instance.UseWebCustomsReportsModule, "Customs Reports", WebDataRegistry.CustomsReportsCategory, "UseWebCustomsReportsModule");
			UseModuleTest(WebDataRegistry.Instance.UseWebWarehouseReportsModule, "Warehouse Reports", WebDataRegistry.WarehouseReportsCategory, "UseWebWarehouseReportsModule", true);
			UseModuleTest(WebDataRegistry.Instance.UseWebLinerAndAgencyReportsModule, "Liner & Agency Reports", WebDataRegistry.LinerAndAgencyReportsCategory, "UseWebShippingReportsModule");

			UseModuleTest(WebDataRegistry.Instance.UseWebCartageModule, "Transport Jobs", WebDataRegistry.TransportJobsCategory, "UseWebCartageModule");
			UseModuleTest(WebDataRegistry.Instance.UseWebForwardingShipmentsModule, "Forwarding Shipments", WebDataRegistry.ForwardingShipmentsCategory, "UseWebForwardingShipmentsModule");
			UseModuleTest(WebDataRegistry.Instance.UseWebCFSShipmentsModule, "CFS Shipments", WebDataRegistry.CFSShipmentsCategory, "UseWebCFSShipmentsModule", _default, false);

			UseModuleTest(WebDataRegistry.Instance.UseWebLinerAndAgencyBookingsModule, "Liner & Agency Bookings", WebDataRegistry.LinerAndAgencyBookingsCategory, "UseWebShippingBookingsModule");
			UseModuleTest(WebDataRegistry.Instance.UseWebWarehouseInventoryModule, "Warehouse Inventory", WebDataRegistry.WarehouseInventoryCategory, "UseWebWarehouseInventoryModule", true);
			UseModuleTest(WebDataRegistry.Instance.UseWebWarehouseOrdersModule, "Warehouse Orders", WebDataRegistry.WarehouseOrdersCategory, "UseWebWarehouseOrdersModule", true);
			UseModuleTest(WebDataRegistry.Instance.UseWebWarehouseProductsModule, "Warehouse Products", WebDataRegistry.WarehouseProductsCategory, "UseWebWarehouseProductsModule", true);
			UseModuleTest(WebDataRegistry.Instance.UseWebWarehouseReceiptsModule, "Warehouse Receipts", WebDataRegistry.WarehouseReceiptsCategory, "UseWebWarehouseReceiptsModule", true);
			UseModuleTest(WebDataRegistry.Instance.UseWebForwardingFlightsModule, "Forwarding Flight Schedules", WebDataRegistry.ForwardingFlightsCategory, "UseWebForwardingFlightsModule");
			UseModuleTest(WebDataRegistry.Instance.UseWebForwardingSailingsModule, "Forwarding Sailing Schedules", WebDataRegistry.ForwardingSailingsCategory, "UseWebForwardingSailingsModule");
			UseModuleTest(WebDataRegistry.Instance.UseWebForwardingRoadModule, "Forwarding Road Schedules", WebDataRegistry.ForwardingRoadCategory, "UseWebForwardingRoadModule");
			UseModuleTest(WebDataRegistry.Instance.UseWebForwardingRailModule, "Forwarding Rail Schedules", WebDataRegistry.ForwardingRailCategory, "UseWebForwardingRailModule");
			UseModuleTest(WebDataRegistry.Instance.UseWebMAWBModule, "MAWBs", WebDataRegistry.ForwardingMAWBCategory, "UseWebMAWBModule", RegistryOptions.IsOnlyForDevelopers, false);
			UseModuleTest(WebDataRegistry.Instance.UseWebHAWBModule, "HAWBs", WebDataRegistry.ForwardingHAWBCategory, "UseWebHAWBModule", RegistryOptions.IsOnlyForDevelopers, false);
		}

		public void TestEventRegistryItems()
		{
			TestRegistryItem(ItemSet.ApplyEventVisibilityOverrideRulesToMilestones, "ApplyEventVisibilityOverrideRulesToMilestones", WebDataRegistry.NeoCategory, "Apply Event Visibility Override Rules to Milestones", "Controls the application of 'Event Visibility – Overrides' rules to Milestones. Default value is ‘No’, this setting prevents ‘Event Visibility – Overrides’ rules from applying to Milestones. When set to ‘Yes’, ‘Event Visibility – Overrides’ rules will be applied to Milestones.", sysFlags, _default, false);
			TestRegistryItem(ItemSet.EventSortOrder, "EventSortOrder", WebDataRegistry.WebTrackerCategory, "Event Sort Order", "Controls published events sort order in the events grids for WebTracker.", sysFlags, _default, new EventSortOrderList(), EventSortOrderList.Codes.Chronological);
			TestRegistryItem(ItemSet.EventSortOrderOfNeo, "EventSortOrderOfNeo", WebDataRegistry.NeoCategory, "Event Sort Order", "Event Sort Order controls event sort order in the tracking events grid of Neo.", sysFlags, _default, new EventSortOrderList(), EventSortOrderList.Codes.ReversedChronological);
			TestRegistryItem(ItemSet.EventIncludeRelated, "EventIncludeRelated", WebDataRegistry.WebTrackerCategory, "Include Related Events", "Display related events in WebTracker.", sysFlags, _default, true);
			TestRegistryItem(ItemSet.EventIncludeEstimates, "EventIncludeEstimates", WebDataRegistry.WebTrackerCategory, "Include Estimates", "Display estimated events in WebTracker.", sysFlags, _default, false);
			TestRegistryItem(ItemSet.EventVisibility, "EventVisibility", WebDataRegistry.WebTrackerCategory, "Event Visibility", "List event codes to be shown in WebTracker.", sysFlags, GetExpectedEventVisibilityDefaultValue());
		}

		EventVisibilityCollection GetExpectedEventVisibilityDefaultValue()
		{
			var defaultValue = new EventVisibilityCollection();
			defaultValue.AddNew(Events.AllExportDocumentsReceivedCode);
			defaultValue.AddNew(Events.AllImportDocumentsReceivedCode);
			defaultValue.AddNew(Events.ArrivalCode);
			defaultValue.AddNew(Events.CargoReceivedAtDepotCode);
			defaultValue.AddNew(Events.CargoAvailableCode);
			defaultValue.AddNew(Events.CustomsCommencedCode);
			defaultValue.AddNew(Events.CustomsEntryStatusCode);
			defaultValue.AddNew(Events.CustomsClearedCode);
			defaultValue.AddNew(Events.DeliveryCartageAdvisedCode);
			defaultValue.AddNew(Events.DeliveryCartageCompleteFinalisedCode);
			defaultValue.AddNew(Events.DepartureCode);
			defaultValue.AddNew(Events.DeliveredCode);
			defaultValue.AddNew(Events.DestinationReadyForDeliveryCode);
			defaultValue.AddNew(Events.ExportCustomsClearedCode);
			defaultValue.AddNew(Events.ExportCustomsCommencedCode);
			defaultValue.AddNew(Events.ExWorksCode);
			defaultValue.AddNew(Events.ItemDocumentJobFinalisedCode);
			defaultValue.AddNew(Events.FreightLoadedCode);
			defaultValue.AddNew(Events.FreightUnloadedCode);
			defaultValue.AddNew(Events.GateInCode);
			defaultValue.AddNew(Events.HoldAwaitingCode);
			defaultValue.AddNew(Events.IntermediateTranshipmentArrivalCode);
			defaultValue.AddNew(Events.MessageStatusChangeCode);
			defaultValue.AddNew(Events.OrderConfirmedCode);
			defaultValue.AddNew(Events.PickupCartageAdvisedCode);
			defaultValue.AddNew(Events.PickupCartageCompleteFinalisedCode);
			defaultValue.AddNew(Events.PickedUpCode);
			defaultValue.AddNew(Events.ReleaseRequestedCode);
			defaultValue.AddNew(Events.ReleasedCode);
			defaultValue.AddNew(Events.WarehouseReceiptArrivedCode);
			defaultValue.AddNew(Events.WarehouseJobEnteredCode);
			defaultValue.AddNew(Events.WarehouseOrderPickingCode);
			defaultValue.AddNew(Events.WarehouseReceiptPuttingAwayCode);
			defaultValue.AddNew(Events.WarehouseReceiptETANotificationCode);
			defaultValue.AddNew(Events.WarehouseReceiptUnloadedCode);
			return defaultValue;
		}

		public void TestEventVisibilityOverrideRegistryItem()
		{
			var registryItem = ItemSet.EventVisibilityOverride;
			TestGenericRegistryItem(
				registryItem,
				"EventVisibilityOverride",
				WebDataRegistry.NeoCategory,
				"Event Visibility – Overrides",
				@"List of Events to be shown in Neo per workflow process type. When Include Related Events is enabled, related events will be displayed in Neo.

Event descriptions can be overridden to suit the business.

Event Details can be hidden.

When there are duplicate events against the same workflow process type the first, last or all events can be shown.

When Quick View is enabled listed Events will be visible to anonymous trackers (no login required).

When Include Estimates is enabled, estimated events will be displayed in Neo.",
				sysFlags);

			var expectedWorkflowCodes = new ZString[]
			{
				"BRK",
				"CLH",
				"CNT",
				"CON",
				"HVC",
				"HVH",
				"HVO",
				"ISF",
				"ORD",
				"SBK",
				"SHP",
				"TCW"
			};

			var defaultValue = registryItem.DefaultValue;
			var emptyDescription = (new EventVisibilityOverride()).Description;
			var actualWorkflowCodes = defaultValue.Select(x => ((EventVisibilityOverride)x).Code);
			var actualDescriptions = defaultValue.Select(x => ((EventVisibilityOverride)x).Description);

			AssertContainsExactElementsInExactOrder("The registry item should contain the correct default workflow codes", expectedWorkflowCodes, actualWorkflowCodes);
			AssertCollectionNotContains("None of the descriptions should still be the default value", emptyDescription, actualDescriptions);
		}

		public void TestMilestoneRegistryItems()
		{
			TestRegistryItem(ItemSet.MilestoneSortOrder, "MilestoneSortOrder", WebDataRegistry.MilestonesCategory, "Milestone Sort Order", "Controls published milestones sort order in the milestones grids for WebTracker. This also affects the Order Tracking Dates for WebTracker.", sysFlags, _default, new MilestoneSortOrderList(), MilestoneSortOrderList.Codes.Chronological);
			TestRegistryItem(ItemSet.MilestoneVisibility, "MilestoneVisibility", WebDataRegistry.MilestonesCategory, "Milestone Visibility", "Controls when published milestones appear in the milestones grids for WebTracker. This also affects the Order Tracking Dates for WebTracker.", sysFlags, _default, new MilestoneVisibilityList(), MilestoneVisibilityList.Codes.All);
			TestRegistryItem(ItemSet.MilestoneDatesVisibility, "MilestoneDatesVisibility", WebDataRegistry.MilestonesCategory, "Milestone Dates", "Controls the display of the dates within the milestones grids for WebTracker. This also affects the Order Tracking Dates for WebTracker.", sysFlags, _default, new MilestoneDatesVisibilityList(), MilestoneDatesVisibilityList.Codes.All);
			TestRegistryItem(ItemSet.MilestoneStatusVisibility, "MilestoneStatusVisibility", WebDataRegistry.MilestonesCategory, "Milestone Status", "Controls the display of the milestone status within the milestones grids for WebTracker. This also affects the Order Tracking Dates for WebTracker.", sysFlags, _default, new MilestoneStatusVisibilityList(), MilestoneStatusVisibilityList.Codes.All);

			#region Milestone Event Updates

			#region Warehouse

			TestRegistryItem(ItemSet.WarehouseOrderMilestoneEventUpdates, "WarehouseOrderMilestoneEventUpdates", WebDataRegistry.WarehouseOrdersCategory, "Milestone Updates", "Specify event codes that can be updated by parties to a Warehouse Order.", sysFlags, GetExpectedWarehouseOrderMilestoneEventUpdatesDefaultValue());
			TestRegistryItem(ItemSet.WarehouseReceiveMilestoneEventUpdates, "WarehouseReceiveMilestoneEventUpdates", WebDataRegistry.WarehouseReceiptsCategory, "Milestone Updates", "Specify event codes that can be updated by parties to a Warehouse Receipt.", sysFlags, GetExpectedWarehouseReceiveMilestoneEventUpdatesDefaultValue());

			#endregion

			#region Transport

			TestRegistryItem(ItemSet.CartageMilestoneEventUpdates, "CartageMilestoneEventUpdates", WebDataRegistry.TransportJobsCategory, "Milestone Updates", "Specify event codes that can be updated by parties to a Port Transport Job.", sysFlags, GetExpectedCartageMilestoneEventUpdatesDefaultValue());

			#endregion

			#region LinerAndAgency

			TestRegistryItem(ItemSet.LinerAndAgencyBookingMilestoneEventUpdates, "ShippingBookingMilestoneEventUpdates", WebDataRegistry.LinerAndAgencyBookingsCategory, "Milestone Updates", "Specify event codes that can be updated by parties to a Liner & Agency Booking.", sysFlags, GetExpectedLinerAndAgencyBookingMilestoneEventUpdatesDefaultValue());
			TestRegistryItem(ItemSet.BillOfLadingMilestoneEventUpdates, "BillOfLadingMilestoneEventUpdates", WebDataRegistry.LinerAndAgencyBillsOfLadingCategory, "Milestone Updates", "Specify event codes that can be updated by parties to a Liner & Agency Bill of Lading.", sysFlags, GetExpectedBillOfLadingMilestoneEventUpdatesDefaultValue());
			TestRegistryItem(ItemSet.LinerAndAgencyContainerMilestoneEventUpdates, "LinerAndAgencyContainerMilestoneEventUpdates", WebDataRegistry.LinerAndAgencyContainersCategory, "Milestone Updates", "Specify event codes that can be updated by parties to a Container Movement.", sysFlags, GetExpectedLinerAndAgencyContainerMilestoneEventUpdatesDefaultValue());

			#endregion

			#region Customs

			TestRegistryItem(ItemSet.ISFMilestoneEventUpdates, "ISFMilestoneEventUpdates", WebDataRegistry.CustomsISFCategory, "Milestone Updates", "Specify event codes that can be updated by parties to an Importer Security Filing.", sysFlags, GetExpectedISFMilestoneEventUpdatesDefaultValue());
			TestRegistryItem(ItemSet.DeclarationMilestoneEventUpdates, "DeclarationMilestoneEventUpdates", WebDataRegistry.CustomsDeclarationCategory, "Milestone Updates", "Specify event codes that can be updated by parties to a Customs Declaration.", sysFlags, GetExpectedDeclarationMilestoneEventUpdatesDefaultValue());

			#endregion

			#region Forwarding

			TestRegistryItem(ItemSet.OrderMilestoneEventUpdates, "OrderMilestoneEventUpdates", WebDataRegistry.ForwardingOrdersCategory, "Milestone Updates", "Specify event codes that can be updated by parties to an Order.", sysFlags, GetExpectedOrderMilestoneEventUpdatesDefaultValue());
			TestRegistryItem(ItemSet.ContainerMilestoneEventUpdates, "ContainerMilestoneEventUpdates", WebDataRegistry.ForwardingContainersCategory, "Milestone Updates", "Specify event codes that can be updated by parties to a Container Movement.", sysFlags, GetExpectedContainerMilestoneEventUpdatesDefaultValue());
			TestRegistryItem(ItemSet.BookingMilestoneEventUpdates, "BookingMilestoneEventUpdates", WebDataRegistry.ForwardingBookingsCategory, "Milestone Updates", "Specify event codes that can be updated by parties to a Booking.", sysFlags, GetExpectedBookingMilestoneEventUpdatesDefaultValue());
			TestRegistryItem(ItemSet.ShipmentMilestoneEventUpdates, "ShipmentMilestoneEventUpdates", WebDataRegistry.ForwardingShipmentsCategory, "Milestone Updates", "Specify event codes that can be updated by parties to a Shipment.", sysFlags, GetExpectedShipmentMilestoneEventUpdatesDefaultValue());

			#endregion

			#endregion
		}

		ShipmentMilestoneEventUpdatesCollection GetExpectedShipmentMilestoneEventUpdatesDefaultValue()
		{
			ShipmentMilestoneEventUpdatesCollection defaultValue = new ShipmentMilestoneEventUpdatesCollection();
			defaultValue.AddNew("DCA", WebPartyType.DeliveryAgent, WebPartyType.ReceivingAgent);
			defaultValue.AddNew("PCF", WebPartyType.SendingAgent);
			defaultValue.AddNew("GIW", WebPartyType.SendingAgent);
			defaultValue.AddNew("AED", WebPartyType.SendingAgent);
			defaultValue.AddNew("CAD", WebPartyType.SendingAgent);
			defaultValue.AddNew("ECM", WebPartyType.ExportBroker, WebPartyType.SendingAgent);
			defaultValue.AddNew("ECC", WebPartyType.ExportBroker, WebPartyType.SendingAgent);
			defaultValue.AddNew("DEP", WebPartyType.SendingAgent);
			defaultValue.AddNew("AID", WebPartyType.ImportBroker, WebPartyType.ReceivingAgent);
			defaultValue.AddNew("CCC", WebPartyType.ImportBroker, WebPartyType.ReceivingAgent);
			defaultValue.AddNew("CLR", WebPartyType.ImportBroker, WebPartyType.ReceivingAgent);
			defaultValue.AddNew("ARV", WebPartyType.DeliveryAgent, WebPartyType.ReceivingAgent);
			defaultValue.AddNew("PCA", WebPartyType.SendingAgent);
			defaultValue.AddNew("DCF", WebPartyType.DeliveryAgent, WebPartyType.ReceivingAgent);
			defaultValue.AddNew("DLV", WebPartyType.Consignee, WebPartyType.DeliveryAgent, WebPartyType.ReceivingAgent);
			defaultValue.AddNew("PUP", WebPartyType.SendingAgent);
			return defaultValue;
		}

		BookingMilestoneEventUpdatesCollection GetExpectedBookingMilestoneEventUpdatesDefaultValue()
		{
			BookingMilestoneEventUpdatesCollection defaultValue = new BookingMilestoneEventUpdatesCollection();
			return defaultValue;
		}

		ContainerMilestoneEventUpdatesCollection GetExpectedContainerMilestoneEventUpdatesDefaultValue()
		{
			ContainerMilestoneEventUpdatesCollection defaultValue = new ContainerMilestoneEventUpdatesCollection();
			return defaultValue;
		}

		LinerAndAgencyContainerMilestoneEventUpdatesCollection GetExpectedLinerAndAgencyContainerMilestoneEventUpdatesDefaultValue()
		{
			var defaultValue = new LinerAndAgencyContainerMilestoneEventUpdatesCollection();
			return defaultValue;
		}

		OrderMilestoneEventUpdatesCollection GetExpectedOrderMilestoneEventUpdatesDefaultValue()
		{
			OrderMilestoneEventUpdatesCollection defaultValue = new OrderMilestoneEventUpdatesCollection();
			defaultValue.AddNew("OCF", WebPartyType.OrderedBy);
			defaultValue.AddNew("EXW", WebPartyType.SendingAgent, WebPartyType.Supplier);
			defaultValue.AddNew("GIW", WebPartyType.SendingAgent);
			defaultValue.AddNew("DEP", WebPartyType.SendingAgent);
			defaultValue.AddNew("ARV", WebPartyType.ReceivingAgent);
			defaultValue.AddNew("CCC", WebPartyType.ReceivingAgent);
			defaultValue.AddNew("CLR", WebPartyType.ReceivingAgent);
			defaultValue.AddNew("CAV", WebPartyType.ReceivingAgent);
			defaultValue.AddNew("DCA", WebPartyType.OrderedBy, WebPartyType.ReceivingAgent);
			defaultValue.AddNew("DCF", WebPartyType.OrderedBy, WebPartyType.ReceivingAgent);
			return defaultValue;
		}

		DeclarationMilestoneEventUpdatesCollection GetExpectedDeclarationMilestoneEventUpdatesDefaultValue()
		{
			DeclarationMilestoneEventUpdatesCollection defaultValue = new DeclarationMilestoneEventUpdatesCollection();
			defaultValue.AddNew("CCC", WebPartyType.Carrier, WebPartyType.ExternalBroker, WebPartyType.Forwarder);
			defaultValue.AddNew("CLR", WebPartyType.Carrier, WebPartyType.ExternalBroker, WebPartyType.Forwarder);
			defaultValue.AddNew("ECM", WebPartyType.Carrier, WebPartyType.ExternalBroker, WebPartyType.Forwarder);
			defaultValue.AddNew("ECC", WebPartyType.Carrier, WebPartyType.ExternalBroker, WebPartyType.Forwarder);
			return defaultValue;
		}

		ISFMilestoneEventUpdatesCollection GetExpectedISFMilestoneEventUpdatesDefaultValue()
		{
			ISFMilestoneEventUpdatesCollection defaultValue = new ISFMilestoneEventUpdatesCollection();
			return defaultValue;
		}

		BillOfLadingMilestoneEventUpdatesCollection GetExpectedBillOfLadingMilestoneEventUpdatesDefaultValue()
		{
			BillOfLadingMilestoneEventUpdatesCollection defaultValue = new BillOfLadingMilestoneEventUpdatesCollection();
			return defaultValue;
		}

		ShippingBookingMilestoneEventUpdatesCollection GetExpectedLinerAndAgencyBookingMilestoneEventUpdatesDefaultValue()
		{
			var defaultValue = new ShippingBookingMilestoneEventUpdatesCollection();
			return defaultValue;
		}

		CartageMilestoneEventUpdatesCollection GetExpectedCartageMilestoneEventUpdatesDefaultValue()
		{
			CartageMilestoneEventUpdatesCollection defaultValue = new CartageMilestoneEventUpdatesCollection();
			return defaultValue;
		}

		WarehouseReceiveMilestoneEventUpdatesCollection GetExpectedWarehouseReceiveMilestoneEventUpdatesDefaultValue()
		{
			WarehouseReceiveMilestoneEventUpdatesCollection defaultValue = new WarehouseReceiveMilestoneEventUpdatesCollection();
			defaultValue.AddNew("WHE", WebPartyType.Client);
			defaultValue.AddNew("WHT", WebPartyType.Supplier, WebPartyType.Transport);
			defaultValue.AddNew("WHA", WebPartyType.Transport);
			defaultValue.AddNew("WHU", WebPartyType.Transport);
			defaultValue.AddNew("WHP");
			defaultValue.AddNew("FIN");
			return defaultValue;
		}

		WarehouseOrderMilestoneEventUpdatesCollection GetExpectedWarehouseOrderMilestoneEventUpdatesDefaultValue()
		{
			WarehouseOrderMilestoneEventUpdatesCollection defaultValue = new WarehouseOrderMilestoneEventUpdatesCollection();
			defaultValue.AddNew("WHE", WebPartyType.Client, WebPartyType.GoodsBilledTo);
			defaultValue.AddNew("WHI");
			defaultValue.AddNew("FIN");
			return defaultValue;
		}

		public void TestComponentsURLsRegistryItems()
		{
			string category = WebDataRegistry.ComponentsURLsCategory;
			TestRegistryItem(ItemSet.WebTrackerUrl, "WebTrackerUrl", category, "WebTracker URL", WebDataRegistry.GetWebUrlHint((NoResString)"WebTracker Site"), RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, TextEditorType.TextBox, string.Empty, "https://wisetechglobal.com/Webtracker");
			TestRegistryItem(ItemSet.WebCampaignUrl, "WebCampaignUrl", category, "WebCampaign URL", WebDataRegistry.GetWebUrlHint((NoResString)"Web Voting / Exam / Survey Campaign Site"), RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue | RegistryOptions.CannotCallParameterlessValueGetter, TextEditorType.TextBox, string.Empty, "https://wisetechglobal.com/Webcampaign");
			TestRegistryItem(ItemSet.WebCertificationUrl, "WebCertificationUrl", category, "WebLearningCentre URL", WebDataRegistry.GetWebUrlHint((NoResString)"WebLearningCentre Site"), sysFlags, RegistryOptions.IsOnlyForSupport, TextEditorType.TextBox, string.Empty, "https://wisetechglobal.com/Webcertification");
			TestRegistryItem(ItemSet.CargoWiseUserPortalUrl, "CargoWiseUserPortalUrl", category, "CargoWise User Portal URL", "Specify the root URL for CargoWise User Portal Site", sysFlags, RegistryOptions.IsOnlyForCargoWise | RegistryOptions.IsOnlyForSupport, TextEditorType.TextBox, "https://myaccount-portal.cargowise.com/myaccount", "https://wisetechglobal.com/myaccount");
			TestRegistryItem(ItemSet.WebCFSUrl, "WebCFSUrl", category, "WebCFS URL", WebDataRegistry.GetWebUrlHint((NoResString)"WebCFS Site"), RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, TextEditorType.TextBox, string.Empty, "https://wisetechglobal.com/WebCFS");

			AssertType<WebsiteRegistryDataType>(WebDataRegistry.Instance.WebTrackerUrl.DataType);
			AssertType<WebsiteRegistryDataType>(WebDataRegistry.Instance.WebCampaignUrl.DataType);
			AssertType<WebsiteRegistryDataType>(WebDataRegistry.Instance.WebCertificationUrl.DataType);
			AssertType<WebsiteRegistryDataType>(WebDataRegistry.Instance.CargoWiseUserPortalUrl.DataType);
			AssertType<WebsiteRegistryDataType>(WebDataRegistry.Instance.WebCFSUrl.DataType);
		}

		public void TestTrustedMessagingItems()
		{
			string category = WebDataRegistry.TrustedMessagingCategory;
			TestRegistryItem(ItemSet.EnableTrustedMessaging, "EnableTrustedMessaging", category, "Enable Trusted Messaging", "Enable Trusted Messaging between Central System and Client System", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, true);
			TestRegistryItem(ItemSet.TrustedMessagingCentralSystemCertificate, "TrustedMessagingCentralSystemCertificate", category, "Central System Certificate", "Specify the Central System Certificate file", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, Array.Empty<byte>());
			TestRegistryItem(ItemSet.TrustedMessagingClientSystemCertificate, "TrustedMessagingClientSystemCertificate", category, "Client System Certificate", "Specify the Client System Certificate file", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, Array.Empty<byte>());
			TestRegistryItem(ItemSet.TrustedMessagingClientSystemCertificatePassword, "TrustedMessagingClientSystemCertificatePassword", category, "Client System Certificate Password", "Specify the Password for Client System Certificate", RegistryStorageFlags.System, TextEditorType.Password, RegistryOptions.IsOnlyForSupport, "");
			TestRegistryItem(ItemSet.TrustedMessagingSecretKey, "TrustedMessagingSecretKey", category, "Trusted Messaging Secret Key", "Trusted Messaging Secret Key", RegistryStorageFlags.System, TextEditorType.TextBox, RegistryOptions.IsHidden | RegistryOptions.NotCached, "");
		}

		public void TestForwardingRegistryItems()
		{
			string category = WebDataRegistry.ForwardingBookingsCategory;

			TestRegistryItem(ItemSet.DisableBookingContainersGrid, "DisableBookingContainersGrid", category, "Hide Booking Containers Grid", "Change to 'Yes' this Registry item value to hide Containers Grid on the Booking page.", sysFlags, _default, false);
			TestRegistryItem(ItemSet.DisableBookingGoodsPacksGrid, "DisableBookingGoodsPacksGrid", category, "Hide Booking Goods / Packs Grid", "Change to 'Yes' this Registry item value to hide Goods / Packs Grid on Booking page.", sysFlags, _default, false);
			TestRegistryItem(ItemSet.BookingOrderGridsEnableExtraRowMode, "BookingOrderGridsEnableExtraRowMode", category, "Enable Extra Row Mode on Order / Order Line Grids", "Change to 'Yes' this Registry item value to always display an extra row for Attached Orders Grid and Product Order Lines Grid on the Booking page.", sysFlags, RegistryOptions.IsOnlyForDevelopers, false);
			TestRegistryItem(ItemSet.BookingAttachOrdersWithoutSupplier,
				"BookingAttachOrdersWithoutSupplier",
				category,
				"Attach Orders Without Supplier",
@"Controls whether orders where no Supplier is specified can be attached to a booking.

The default is 'No' - Only orders where the logged in organization is a Supplier, Buyer or Carrier can be attached.

If overridden to 'Yes' - Orders where the Supplier is not specified, and the Buyer matches Booking Consignee, can be attached.",
			sysFlags,
			_default,
			false);
			TestRegistryItem(ItemSet.DisableBookingInsuranceValue, "DisableBookingInsuranceValue", category, "Hide Booking Insurance Value", "Change this Registry item to 'Yes' to hide Insurance Value on the Booking page.", sysFlags, _default, false);

			var lookupList = FreightDataRegistry.Instance.HouseBillOfLadingTypesForSea.Value.GetCodeDescriptionPairList();
			string defaultValue = lookupList[0].Code;
			TestRegistryItem(ItemSet.BookingDefaultHouseBillType, "BookingDefaultHouseBillType", category, "Default House Bill Type", WebDataRegistry.HintForBookingDefaultHouseBillType, sysFlags, _default, lookupList, defaultValue);

			TestRegistryItem(ItemSet.BookingPackLinesPageSize,
				"BookingPackLinesPageSize",
				category,
				"Goods/Packs Page Size",
				WebDataRegistry.HintForBookingPackLinesPageSize,
				sysFlags,
				RegistryOptions.Default,
				0m,
				0m,
				99m);

			TestRegistryItem(ItemSet.BookingTermsAndConditions, "BookingTermsAndConditions", category, "Terms and Conditions for Bookings", WebDataRegistry.HintForBookingTermsAndConditions, sysFlags, RegistryOptions.Default, TextEditorType.HTML, string.Empty);
			TestRegistryItem(ItemSet.PrintAddressesOnFreightLabels, "PrintAddressesOnFreightLabels", category, "Print Addresses on Freight Labels", WebDataRegistry.HintForPrintAddressesOnFreightLabels, sysFlags, _default, true);

			TestRegistryItem(ItemSet.UseShipmentPageUnShippedOrders,
							 "UseShipmentPageUnShippedOrders",
							 WebDataRegistry.ForwardingShipmentsCategory,
							 "Show Un-shipped Orders",
											 @"This Registry item controls whether the ‘Show Un-shipped Orders’ option is available on the Shipments page of WebTracker.

You can show an additional grid underneath the main Shipments grid that will show all Orders that are not attached to Shipments (allowing your clients to see all of their current jobs on one page).

The default is ‘NO’ - web users will see a single grid for Shipments on the Shipments page (they will not have an option to show the un-shipped orders grid).

If overridden to ‘YES’ - your web users will see a link at the top of the Shipments page.
They can click this link to show or hide the Un-shipped Orders grid at the bottom of the Shipments page.",
											 sysFlags,
											 _default,
											 false);

			TestRegistryItem(ItemSet.GetQuotesBasedOnPostalCodes, "GetQuotesBasedOnPostalCodes", WebDataRegistry.ForwardingQuotesCategory, "Get quotes based on postal/zip codes", WebDataRegistry.HintForGetQuotesBasedOnPostalCodes, sysFlags, _default, false);

			TestRegistryItem(ItemSet.SaveQuotesWithoutRates, "SaveQuotesWithoutRates", WebDataRegistry.ForwardingQuotesCategory, "Save Quotes without Rates", WebDataRegistry.HintForSaveQuotesWithoutRates, sysFlags, false);

			TestRegistryItem(ItemSet.DisableQuoteInsuranceValue, "DisableQuoteInsuranceValue", WebDataRegistry.ForwardingQuotesCategory, "Hide Quote Insurance Value", "Change to 'Yes' this Registry item value to hide Insurance Value on the Quote page.", sysFlags, _default, false);

			TestRegistryItem(ItemSet.UseHVLVBookingHeadersAndConsignments,
							 "UseHVLVBookingHeadersAndConsignments",
							 WebDataRegistry.ForwardingHVLVCategory,
							 "Show HVLV Booking Headers & Consignments",
											 @"Controls the display of the eCommerce tab in Neo.

The default is ‘No’ – The eCommerce tab will not be shown in Neo and will be disabled for all users.

If overridden to ‘Yes’ – The eCommerce tab will be shown in Neo.",
											 sysFlags,
											 _default,
											 false);
		}

		public void TestWarhouseInventoryRegistryItems()
		{
			string category = WebDataRegistry.WarehouseInventoryCategory;

			TestRegistryItem(ItemSet.WarehouseInventoryDetailsPageSize,
				"WarehouseInventoryDetailsPageSize",
				category,
				"Inventory Details Page Size",
				WebDataRegistry.HintForWarehouseInventoryDetailsPageSize,
				sysFlags,
				RegistryOptions.Default,
				50m,
				0m,
				99m);
		}

		public void TestLoginQuickViewRegistryItems()
		{
			string category = WebDataRegistry.LoginQuickViewCategory;
			TestRegistryItem(ItemSet.WebTrackerAutoLoginRequiresPassword, "WebTrackerAutoLoginRequiresPassword", category, "Auto Login Requires Password", "This determines if the WebTracker auto-login link requires user to type in the password. By default password is not required.", sysFlags, RegistryOptions.PreserveTestValue, false);
			TestRegistryItem(ItemSet.WebTrackerLoginPageInstruction, "WebTrackerLoginPageInstruction", category, "Login Page Instructions", WebDataRegistry.HintForWebTrackerLoginPageInstruction, sysFlags, RegistryOptions.Default, TextEditorType.Memo, string.Empty);
			TestRegistryItem(ItemSet.WebTrackerLoginRequiresCompanyCode, "WebTrackerLoginRequiresCompanyCode", category, "Login requires Company Code", WebDataRegistry.HintForWebTrackerLoginRequiresCompanyCode, sysFlags, RegistryOptions.Default, true);
			TestRegistryItem(ItemSet.WebTrackerLocalChargesOnShipmentQuickView, "WebTrackerLocalChargesOnShipmentQuickView", category, "Show invoicing details in Shipment Quick View.", WebDataRegistry.HintForWebTrackerLocalChargesOnShipmentQuickView, sysFlags, RegistryOptions.PreserveTestValue, false);
			TestRegistryItem(ItemSet.WebTrackerShipmentQuickView, "WebTrackerShipmentQuickView", category, "Show Shipment Quick View", WebDataRegistry.HintForWebTrackerShipmentQuickView, sysFlags, RegistryOptions.PreserveTestValue, true);
			TestRegistryItem(ItemSet.WebTrackerContainerQuickView, "WebTrackerContainerQuickView", category, "Show Container Tracking Quick View", WebDataRegistry.HintForWebTrackerContainerQuickView, sysFlags, RegistryOptions.Default, false);
			TestRegistryItem(ItemSet.WebTrackerSiteTermsAndConditions, "WebTrackerSiteTermsAndConditions", category, "Site Terms and Conditions", WebDataRegistry.HintForWebTrackerSiteTermsAndConditions, sysFlags, RegistryOptions.PreserveTestValue, TextEditorType.HTML, string.Empty);
			TestRegistryItem(ItemSet.StoreFailedLogins, "StoreFailedLogins", category, "Store info about failed logins in the logs for the web branch", "Failed login info will be stored as a new log record on the web branch.", sysFlags, RegistryOptions.IsOnlyForDevelopers, false);
			TestRegistryItem(ItemSet.WebTrackerQuickViewByAdditionalReferences, "WebTrackerQuickViewByAdditionalReferences", category, "Use Additional References in Quick View", WebDataRegistry.HintForQuickViewByAdditionalReferences, sysFlags, RegistryOptions.Default, false);
			TestRegistryItem(ItemSet.WebTrackerUseCanadianReferencesQuickView, "WebTrackerUseCanadianReferencesQuickView", WebDataRegistry.LoginQuickViewCanadaCategory, "Use CCN and Transaction # in Quick View", WebDataRegistry.HintForWebTrackerUseCanadianReferencesQuickView, sysFlags, RegistryOptions.Default, false);
		}

		public void TestPasswordResetRegistryItems()
		{
			var category = WebDataRegistry.PasswordResetCategory;
			TestGenericRegistryItem(ItemSet.PasswordResetEmailFooter, "PasswordReminderEmailFooter", category, "Password Reset Email Footer", WebDataRegistry.GetHintForPasswordResetEmailFooter(WebDataRegistry.GetDefaultValueForPasswordResetEmailFooter(Env.Registry.MailboxDisplayName)), RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default, WebDataRegistry.GetDefaultValueForPasswordResetEmailFooter(Env.Registry.MailboxDisplayName));
			TestGenericRegistryItem(ItemSet.PasswordResetSuccessfullyEmailTemplate, "PasswordResetSuccessfullyEmailTemplate", category, "Password Reset Successfully Email Template", "Template that will be used in reset confirmation emails message.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default);
			TestGenericRegistryItem(ItemSet.PasswordResetEmailTemplate, "PasswordResetEmailTemplate", category, "Password Reset Email Template", "Template that will be used in Password Reset emails.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default);
			TestGenericRegistryItem(ItemSet.MasterPasswordResetSuccessfullyEmailTemplate, "MasterPasswordResetSuccessfullyEmailTemplate", category, "Master Password Reset Successfully Email Template", "Template that will be used in reset master password confirmation emails message.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default);
		}

		public void TestPasswordSetRegistryItems()
		{
			string category = WebDataRegistry.PasswordSetCategory;
			TestGenericRegistryItem(ItemSet.PasswordSetEmailFooter, "PasswordSetReminderEmailFooter", category, "Password Set Email Footer", WebDataRegistry.GetHintForPasswordSetEmailFooter(WebDataRegistry.GetDefaultValueForPasswordSetEmailFooter(Env.Registry.MailboxDisplayName)), RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default, WebDataRegistry.GetDefaultValueForPasswordSetEmailFooter(Env.Registry.MailboxDisplayName));
			TestGenericRegistryItem(ItemSet.PasswordSetEmailTemplate, "PasswordSetEmailTemplate", category, "Password Set Email Template", "Template that will be used in Password Set Emails.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default);
			TestRegistryItem(ItemSet.PasswordSetEmailUnsuccessfulMessage, "PasswordSetReminderEmailUnsuccessfulMessage", category, "Password Set Unsuccessful Message", WebDataRegistry.HintForPasswordSetEmailUnsuccessfulMessage, sysFlags, RegistryOptions.Default, TextEditorType.TextBox, WebDataRegistry.DefaultValueForPasswordSetEmailUnsuccessfulMessage);
			TestGenericRegistryItem(ItemSet.PasswordSetSuccessfullyEmailTemplate, "PasswordSetSuccessfullyEmailTemplate", category, "Password Set Successfully Email Template", "Template that will be used in set password confirmation emails message.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default);
			TestGenericRegistryItem(ItemSet.MasterPasswordSetSuccessfullyEmailTemplate, "MasterPasswordSetSuccessfullyEmailTemplate", category, "Master Password Set Successfully Email Template", "Template that will be used in set master password confirmation emails message.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default);
		}

		public void TestWeAdminsNOtificationGroup()
		{
			TestRegistryItem(ItemSet.WebAdminsEmailNotificationGroup, "WebAdminsEmailNotificationGroup", WebDataRegistry.WebCategory, "Web Site Administrators Email Notification Group", "The group that will be sent email notifications for any WebTracker management events.", sysFlags, _default | RegistryOptions.IsValueMandatory, RegistryFindBoxCollection.GlbGroup, RegistryFactory.Instance.GetGroupPK("ALL"));
		}

		public void TestPerformanceAndAppearanceRegistryItems()
		{
			var category = WebDataRegistry.PerformanceAndAppearanceCategory;
			TestRegistryItem(ItemSet.PageSize, "PageSize", category, "Page Size for Search Screen", "The number of records to show per page on a search screen. You can also define the maximum number of records to show on a search screen.", sysFlags, RegistryOptions.PreserveTestValue, 25);
			TestRegistryItem(ItemSet.DateFormat, "DateFormat", category, "Date Format", WebDataRegistry.HintForDateFormat, sysFlags, _default, new DateFormatCodeDescriptionPairList(), "STD");
			TestRegistryItem(ItemSet.WebActivityLogging, "WebActivityLogging", category, "Web Activity Logging", "Set this to 'Yes' to activate logging user actions in WebTracker.", sysFlags, false);
			TestRegistryItem(ItemSet.MaxFilteredRecords, "MaxFilteredRecords", category, "Max No. of Records to Show", WebDataRegistry.HintForMaxFilteredRecords, sysFlags, RegistryOptions.PreserveTestValue, 1000);
			TestRegistryItem(ItemSet.MaxFilteredRecordsForExportToExcel, "MaxFilteredRecordsForExportToExcel", category, "Max No. of Records to Export to Excel", WebDataRegistry.HintForMaxFilteredRecordsForExportToExcel, sysFlags, _default, 10000);
			TestRegistryItem(ItemSet.RequestTimeout, "RequestTimeout", category, "Request Timeout", WebDataRegistry.HintForRequestTimeout, sysFlags, 300);

			category = WebDataRegistry.WebTrackerThemeCategory;
			TestGenericRegistryItem(ItemSet.WebTrackerTheme, "WebTrackerTheme", category, "Theme", WebDataRegistry.HintForWebTrackerTheme, RegistryStorageFlags.System, RegistryOptions.PreserveTestValue);
			TestGenericRegistryItem(ItemSet.WebTrackerCustomCss, "WebTrackerCustomCss", category, "Theme CSS", WebDataRegistry.HintForWebTrackerCustomCss, RegistryStorageFlags.System, RegistryOptions.PreserveTestValue);
			TestGenericRegistryItem(ItemSet.WebTrackerCustomImages, "WebTrackerCustomImages", category, "Theme Images", WebDataRegistry.HintForWebTrackerCustomImages, RegistryStorageFlags.System, RegistryOptions.PreserveTestValue);
			TestGenericRegistryItem(ItemSet.WebTrackerUrls, "WebTrackerUrls", category, "URLs", WebDataRegistry.HintForWebTrackerUrls, RegistryStorageFlags.System, RegistryOptions.PreserveTestValue);

			category = WebDataRegistry.WebCFSThemeCategory;
			TestGenericRegistryItem(ItemSet.WebCFSTheme, "WebCFSTheme", category, "Theme", WebDataRegistry.HintForWebCFSTheme, RegistryStorageFlags.System, RegistryOptions.PreserveTestValue);
			TestGenericRegistryItem(ItemSet.WebCFSCustomCss, "WebCFSCustomCss", category, "Theme CSS", WebDataRegistry.HintForWebCFSCustomCss, RegistryStorageFlags.System, RegistryOptions.PreserveTestValue);
			TestGenericRegistryItem(ItemSet.WebCFSCustomImages, "WebCFSCustomImages", category, "Theme Images", WebDataRegistry.HintForWebCFSCustomImages, RegistryStorageFlags.System, RegistryOptions.PreserveTestValue);
			TestGenericRegistryItem(ItemSet.WebCFSUrls, "WebCFSUrls", category, "URLs", WebDataRegistry.HintForWebCFSUrls, RegistryStorageFlags.System, RegistryOptions.Default);

			category = WebDataRegistry.WebCampaignThemeCategory;
			TestGenericRegistryItem(ItemSet.WebCampaignCustomTheme, "WebCampaignCustomTheme", category, "Theme", WebDataRegistry.HintForWebCampaignTheme, RegistryStorageFlags.System, RegistryOptions.PreserveTestValue);
			TestGenericRegistryItem(ItemSet.WebCampaignCustomThemeUrl, "WebCampaignCustomThemeUrl", category, "Theme URL", WebDataRegistry.HintForWebCampaignUrls, RegistryStorageFlags.System);
			AssertEquals(WebThemeCustomObject.Schema.DefaultThemeName, ItemSet.WebCampaignCustomTheme.DefaultValue[0].ThemeName);
			AssertEquals(WebThemeCustomObject.Schema.DefaultThemeName, ItemSet.WebCampaignCustomThemeUrl.DefaultValue[0].ThemeName);
		}

		public void TestGetWebTheme()
		{
			var url1 = "red.webtracker.com";
			var url2 = "blu.webtracker.com";
			var urlx = "grn.webtracker.com";

			var urlcfs1 = "red.webcfs.com";
			var urlcfs2 = "blu.webcfs.com";
			var urlcfsx = "grn.webcfs.com";

			var data0 = "CLS";
			var data1 = "CUS";
			var data2 = "STD";

			WebDataRegistry.Instance.WebTrackerTheme.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebTrackerTheme[]
				{
					new WebTrackerTheme("", data0),
					new WebTrackerTheme(url1, data1),
					new WebTrackerTheme(url2, data2)
				});

			AssertEquals(data1, WebDataRegistry.Instance.GetWebTheme(WebDataRegistry.Instance.WebTrackerTheme, url1));
			AssertEquals(data2, WebDataRegistry.Instance.GetWebTheme(WebDataRegistry.Instance.WebTrackerTheme, url2));
			AssertEquals(data0, WebDataRegistry.Instance.GetWebTheme(WebDataRegistry.Instance.WebTrackerTheme, urlx));

			WebDataRegistry.Instance.WebTrackerTheme.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebTrackerTheme[]
			{
				new WebTrackerTheme(null, data0),
				new WebTrackerTheme(url1, data1),
				new WebTrackerTheme(url2, data2)
			});

			AssertEquals(data1, WebDataRegistry.Instance.GetWebTheme(WebDataRegistry.Instance.WebTrackerTheme, url1));
			AssertEquals(data2, WebDataRegistry.Instance.GetWebTheme(WebDataRegistry.Instance.WebTrackerTheme, url2));
			AssertEquals(data0, WebDataRegistry.Instance.GetWebTheme(WebDataRegistry.Instance.WebTrackerTheme, urlx));

			WebDataRegistry.Instance.WebCFSTheme.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebTrackerTheme[]
				{
					new WebTrackerTheme("", data0),
					new WebTrackerTheme(urlcfs1, data1),
					new WebTrackerTheme(urlcfs2, data2)
				});
			AssertEquals(data1, WebDataRegistry.Instance.GetWebTheme(WebDataRegistry.Instance.WebCFSTheme, urlcfs1));
			AssertEquals(data2, WebDataRegistry.Instance.GetWebTheme(WebDataRegistry.Instance.WebCFSTheme, urlcfs2));
			AssertEquals(data0, WebDataRegistry.Instance.GetWebTheme(WebDataRegistry.Instance.WebCFSTheme, urlcfsx));
		}

		public void TestGetWebCustomImage()
		{
			var name = "image.jpg";

			var url0 = "";
			var url1 = "red.webtracker.com";
			var url2 = "blu.webtracker.com";
			var urlx = "grn.webtracker.com";

			var urlcfs1 = "red.webcfs.com";
			var urlcfs2 = "blu.webcfs.com";
			var urlcfsx = "grn.webcfs.com";

			var data0 = new byte[] { 0, 1, 2 };
			var data1 = new byte[] { 3, 4, 5 };
			var data2 = new byte[] { 6, 7, 8 };

			WebDataRegistry.Instance.WebTrackerCustomImages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebTrackerCustomImage[]
				{
					new WebTrackerCustomImage(name, url0, data0),
					new WebTrackerCustomImage(name, "Http://" + url1, data1),
					new WebTrackerCustomImage(name, "HTTPS://" + url2, data2)
				});
			AssertEquals(data1, WebDataRegistry.Instance.GetWebCustomImage(WebDataRegistry.Instance.WebTrackerCustomImages, url1, name));
			AssertEquals(data2, WebDataRegistry.Instance.GetWebCustomImage(WebDataRegistry.Instance.WebTrackerCustomImages, url2, name));
			AssertEquals(data0, WebDataRegistry.Instance.GetWebCustomImage(WebDataRegistry.Instance.WebTrackerCustomImages, urlx, name));
			AssertEquals(data0, WebDataRegistry.Instance.GetWebCustomImage(WebDataRegistry.Instance.WebTrackerCustomImages, urlx, "ImAgE.JpG"));

			WebDataRegistry.Instance.WebCFSCustomImages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebTrackerCustomImage[]
				{
					new WebTrackerCustomImage(name, url0, data0),
					new WebTrackerCustomImage(name, urlcfs1, data1),
					new WebTrackerCustomImage(name, urlcfs2, data2)
				});
			AssertEquals(data1, WebDataRegistry.Instance.GetWebCustomImage(WebDataRegistry.Instance.WebCFSCustomImages, urlcfs1, name));
			AssertEquals(data2, WebDataRegistry.Instance.GetWebCustomImage(WebDataRegistry.Instance.WebCFSCustomImages, urlcfs2, name));
			AssertEquals(data0, WebDataRegistry.Instance.GetWebCustomImage(WebDataRegistry.Instance.WebCFSCustomImages, urlcfsx, name));
			AssertEquals(data0, WebDataRegistry.Instance.GetWebCustomImage(WebDataRegistry.Instance.WebCFSCustomImages, urlcfsx, "ImAgE.JpG"));
		}

		public void TestGetWebCustomCss()
		{
			var url0 = "";

			var url1 = "red.webtracker.com";
			var url2 = "blu.webtracker.com";
			var urlx = "grn.webtracker.com";

			var urlcfs1 = "red.webcfs.com";
			var urlcfs2 = "blu.webcfs.com";
			var urlcfsx = "grn.webcfs.com";

			var data0 = "body { color: black; }";
			var data1 = "body { color: red; }";
			var data2 = "body { color: blue; }";

			WebDataRegistry.Instance.WebTrackerCustomCss.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebTrackerCustomCss[]
				{
					new WebTrackerCustomCss(url0, data0),
					new WebTrackerCustomCss(url1, data1),
					new WebTrackerCustomCss(url2, data2)
				});
			AssertEquals(data1, WebDataRegistry.Instance.GetWebCustomCss(WebDataRegistry.Instance.WebTrackerCustomCss, url1));
			AssertEquals(data2, WebDataRegistry.Instance.GetWebCustomCss(WebDataRegistry.Instance.WebTrackerCustomCss, url2));
			AssertEquals(data0, WebDataRegistry.Instance.GetWebCustomCss(WebDataRegistry.Instance.WebTrackerCustomCss, urlx));

			WebDataRegistry.Instance.WebCFSCustomCss.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebTrackerCustomCss[]
				{
					new WebTrackerCustomCss(url0, data0),
					new WebTrackerCustomCss(urlcfs1, data1),
					new WebTrackerCustomCss(urlcfs2, data2)
				});
			AssertEquals(data1, WebDataRegistry.Instance.GetWebCustomCss(WebDataRegistry.Instance.WebCFSCustomCss, urlcfs1));
			AssertEquals(data2, WebDataRegistry.Instance.GetWebCustomCss(WebDataRegistry.Instance.WebCFSCustomCss, urlcfs2));
			AssertEquals(data0, WebDataRegistry.Instance.GetWebCustomCss(WebDataRegistry.Instance.WebCFSCustomCss, urlcfsx));
		}

		public void TestContainsWebUrl()
		{
			var urls = new[] { "https://WEB1.com", "https://www.WEB1.com/", "http://WEB2.com/", "http://www.WEB2.com/", "//Web3.com/" };
			Assert("contains w/ https prefix", WebDataRegistry.ContainsWebUrl(urls, "web1.com"));
			Assert("contains w/ https prefix and trailing slash", WebDataRegistry.ContainsWebUrl(urls, "www.web1.com"));
			Assert("contains w/ http prefix", WebDataRegistry.ContainsWebUrl(urls, "web2.com"));
			Assert("contains w/ http prefix and trailing slash", WebDataRegistry.ContainsWebUrl(urls, "www.web2.com"));
			Assert("contains w/ //", WebDataRegistry.ContainsWebUrl(urls, "web3.com"));
		}

		public void TestDataTypes()
		{
			var type = ItemSet.PageSize.DataType as IntRegistryDataType;
			AssertNotNull(type);

			AssertEquals(type.LowerBound, ((double)1));
			AssertEquals(type.UpperBound, ((double)Int32.MaxValue));

			type = ItemSet.MaxFilteredRecords.DataType as IntRegistryDataType;
			AssertNotNull(type);
			AssertEquals(type.LowerBound, ((double)0));
			AssertEquals(type.UpperBound, ((double)Int32.MaxValue));
		}

		public void TestWebServicesRegistryItems()
		{
			string category = WebDataRegistry.WebServicesCategory;
			TestRegistryItem(ItemSet.WebServiceUsername, "WebServiceUsername", category, "Web Service User Login", "Enter the username that will be used when accessing web services.", sysFlags, TextEditorType.TextBox, RegistryOptions.PreserveTestValue, string.Empty);
			TestRegistryItem(ItemSet.WebServicePassword, "WebServicePassword", category, "Web Service User Password", "Enter the password that will be used when accessing web services.", sysFlags, TextEditorType.Password, RegistryOptions.PreserveTestValue | RegistryOptions.IsPasswordVisibleForControllerUser, string.Empty);
			TestRegistryItem(ItemSet.WebServiceAlternativeCredentials, "WebServiceAlternativeCredentials", category, "Web Service Alternative Credentials", "List of alternative credentials to access supported web services.\r\nCurrently supported web services: Remote Printing.\r\nAfter editing credentials, you need to restart web server(s), or wait for period of time specified in registry item 'Web Service Credentials Cache Time'.", sysFlags, RegistryOptions.PreserveTestValue, 50, 0);
			TestRegistryItem(ItemSet.WebServiceChargeCodes, "WebServiceChargeCodes", category, "Charge Codes", "Transactions with Charge Codes defined in this Registry will be excluded from Web Service Data Export.", RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, Guid.Empty.ToString(), RegistryFindBoxFilter.MrgDsbOrMjaChargeCode);
		}

		public void TestWebSiteIntegrationRegistryItems()
		{
			TestRegistryItem(ItemSet.CustomLoginPageURL,
											 "CustomLoginPageURL",
											 WebDataRegistry.WebSiteIntegrationCategory,
											 "Custom Login Page URL",
							 @"Use this setting to specify the URL of your Custom Login Page for WebTracker.
This page will be used instead of the standard Login page and should implement special functionality to pass the login details to your Web Tracking site.
This page should also be able to receive and show possible login errors.",
											 sysFlags,
											 TextEditorType.TextBox,
											 ZString.Empty);

			TestRegistryItem(ItemSet.CustomQuickViewPageURL,
							 "CustomQuickViewPageURL",
											 WebDataRegistry.WebSiteIntegrationCategory,
											 "Custom Quick View Page URL",
							 @"Use this setting to specify the URL of your Custom Quick View page for WebTracker.
This page should implement special functionality to pass the Quick View details to WebTracker.
This page should also be able to receive and show possible errors as a result of trying to Quick View.

NOTE: You do not need to specify anything in this setting if your Custom Login Page also handles the Quick View functionality.
You only need to override this setting if you have a separate page for the Custom Quick View functionality.",
											 sysFlags,
											 TextEditorType.TextBox,
											 ZString.Empty);
		}

		public void TestMobileServicesUriRegistryItem()
		{
			TestRegistryItem(
				ItemSet.MobileServicesUriRegistryItem,
				"MobileServicesUri",
				WebDataRegistry.WebServicesCategory,
				"Mobile Services URL",
				"The URL to the WiseTech Global mobile services",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.PreserveTestValue,
				TextEditorType.TextBox,
				expectedDefaultValue: "https://mobile.cargowise.net/",
				testValueToSetAndRead: "https://localhost/");
		}

		public void TestShipmentPageCustomisation()
		{
			TestGenericRegistryItem(ItemSet.ShipmentPageCustomisation, "ShipmentPageCustomisation",
				WebDataRegistry.ForwardingShipmentsCategory,
				"Customize Shipment Details page",
				"Use this Registry settings to hide some elements on the Shipment Details web page.",
				sysFlags, _default);

			CodeDescriptionBoolDisallowNewCollection elements = new CodeDescriptionBoolDisallowNewCollection
						{
							{ WebDataRegistry.ShipmentDetailsPageElements.EtdAndEta, (NoResString)"ETD and ETA", false },
							{ WebDataRegistry.ShipmentDetailsPageElements.StorageCommencesDate, (NoResString)"Storage Commences Date", false },
							{ WebDataRegistry.ShipmentDetailsPageElements.CartageAdvisedDate, (NoResString)"Cartage Advised Date", false },
							{ WebDataRegistry.ShipmentDetailsPageElements.TransportGrid, (NoResString)"Transport Grid", false },
							{ WebDataRegistry.ShipmentDetailsPageElements.GoodsPacksGrid, (NoResString)"Goods / Packs Grid", false },
							{ WebDataRegistry.ShipmentDetailsPageElements.OrdersGrid, (NoResString)"Orders Grid", false },
							{ WebDataRegistry.ShipmentDetailsPageElements.ContainersGrid, (NoResString)"Containers Grid", false }
						};
			AssertEquals("Should be equal number of elements", elements.Count, ItemSet.ShipmentPageCustomisation.Value.Count);

			for (int i = 0; i < ItemSet.ShipmentPageCustomisation.Value.Count; i++)
			{
				AssertEquals(string.Format("Codes of elements in position {0} should match, but are {1} and {2}", i, elements[i].Code, ItemSet.ShipmentPageCustomisation.Value[i].Code),
					elements[i].Code, ItemSet.ShipmentPageCustomisation.Value[i].Code);
				AssertEquals(string.Format("Element {0} should have Bool equal false", i), false, ItemSet.ShipmentPageCustomisation.Value[i].Bool);
			}

			CodeDescriptionBool element = elements[0];
			element.Bool = true;

			ItemSet.ShipmentPageCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, elements);

			AssertNotNull("ShipmentPageCustomisation.Value should not be null", ItemSet.ShipmentPageCustomisation.Value);
			AssertEquals("Should be seven elements", 7, ItemSet.ShipmentPageCustomisation.Value.Count);
			AssertEquals("Should not allow new elements to be added", false, ItemSet.ShipmentPageCustomisation.Value.AllowNew);
			AssertEquals("The element's Code", element.Code, ItemSet.ShipmentPageCustomisation.Value[0].Code);
			AssertEquals("The element's Bool should be True", true, ItemSet.ShipmentPageCustomisation.Value[0].Bool);

			for (int i = 1; i < ItemSet.ShipmentPageCustomisation.Value.Count; i++)
			{
				AssertEquals(string.Format("Element {0} should have Bool equal false", i), false, ItemSet.ShipmentPageCustomisation.Value[i].Bool);
			}
		}

		public void TestCFSShipmentPageCustomisation()
		{
			TestGenericRegistryItem(ItemSet.CFSShipmentPageCustomisation, "CFSShipmentPageCustomisation",
				WebDataRegistry.CFSShipmentsCategory,
				"Customize CFS Shipment Details page",
				"Use this Registry settings to hide some elements on the CFS Shipment Details web page.",
				sysFlags, RegistryOptions.Default);

			CodeDescriptionBoolCollection elements = new CodeDescriptionBoolCollection
						{
							{ WebDataRegistry.CFSShipmentDetailsPageElements.EtdAndEta, (NoResString)"ETD and ETA", false },
							{ WebDataRegistry.CFSShipmentDetailsPageElements.StorageCommencesDate, (NoResString)"Storage Commences Date", false },
							{ WebDataRegistry.CFSShipmentDetailsPageElements.CartageAdvisedDate, (NoResString)"Cartage Advised Date", false },
							{ WebDataRegistry.CFSShipmentDetailsPageElements.TransportGrid, (NoResString)"Transport Grid", false },
							{ WebDataRegistry.CFSShipmentDetailsPageElements.GoodsPacksGrid, (NoResString)"Goods / Packs Grid", false },
							{ WebDataRegistry.CFSShipmentDetailsPageElements.DeliveryGrid, (NoResString)"Delivery Grid", false },
						};
			AssertEquals("Should be equal number of elements", elements.Count, ItemSet.CFSShipmentPageCustomisation.Value.Count);

			for (int i = 0; i < ItemSet.CFSShipmentPageCustomisation.Value.Count; i++)
			{
				AssertEquals(string.Format("Codes of elements in position {0} should match, but are {1} and {2}", i, elements[i].Code, ItemSet.CFSShipmentPageCustomisation.Value[i].Code),
					elements[i].Code, ItemSet.CFSShipmentPageCustomisation.Value[i].Code);
				AssertEquals(string.Format("Element {0} should have Bool equal false", i), false, ItemSet.CFSShipmentPageCustomisation.Value[i].Bool);
			}

			CodeDescriptionBool element = elements[0];
			element.Bool = true;

			ItemSet.CFSShipmentPageCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, elements);

			AssertNotNull("CFSShipmentPageCustomisation.Value should not be null", ItemSet.CFSShipmentPageCustomisation.Value);
			AssertEquals("Should be seven elements", 6, ItemSet.CFSShipmentPageCustomisation.Value.Count);
			AssertEquals("The element's Code", element.Code, ItemSet.CFSShipmentPageCustomisation.Value[0].Code);
			AssertEquals("The element's Bool should be True", true, ItemSet.CFSShipmentPageCustomisation.Value[0].Bool);

			for (int i = 1; i < ItemSet.CFSShipmentPageCustomisation.Value.Count; i++)
			{
				AssertEquals(string.Format("Element {0} should have Bool equal false", i), false, ItemSet.CFSShipmentPageCustomisation.Value[i].Bool);
			}
		}

		public void TestWebEDocsBulkDownload()
		{
			TestGenericRegistryItem(ItemSet.WebEDocsBulkDownload, "WebEDocsBulkDownload",
									WebDataRegistry.WebCategory,
									"eDocs Bulk Download",
									"eDocs with the following document types will be available to download directly from the search screen. When accessing a module in Web Tracker, if that module does not have configuration for eDocs Bulk Download, it will default to the configuration from 'All Modules'.",
									RegistryStorageFlags.System | RegistryStorageFlags.Company);

			var docTypes = new RefDocTypeEntryCollection();
			docTypes.AddNew().RefDocTypePK = Factory.LoadTop1<IRefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "ACV")).PK;
			docTypes.AddNew().RefDocTypePK = Factory.LoadTop1<IRefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "MSC")).PK;
			var modules = new WebEDocsDownloadEntryDictionary();
			modules.Add("AllModules", new WebEDocsDownloadEntry(true, docTypes));

			ItemSet.WebEDocsBulkDownload.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, modules);

			AssertNotNull("WebEDocsBulkDownload.Value should not be null", ItemSet.WebEDocsBulkDownload.Value);
			AssertEquals("Should be 1 module", 1, ItemSet.WebEDocsBulkDownload.Value.Count);
			AssertEquals("Should be true", true, ItemSet.WebEDocsBulkDownload.Value["AllModules"].AllowAllDocTypes);
			AssertEquals("Should be 2 elements", 2, ItemSet.WebEDocsBulkDownload.Value["AllModules"].DocTypeCollection.Count);
			AssertEquals("The element's RefDocTypePK", docTypes[0].RefDocTypePK, ItemSet.WebEDocsBulkDownload.Value["AllModules"].DocTypeCollection[0].RefDocTypePK);
			AssertEquals("The element's RefDocTypePK", docTypes[1].RefDocTypePK, ItemSet.WebEDocsBulkDownload.Value["AllModules"].DocTypeCollection[1].RefDocTypePK);
			AssertEquals("The element's RefDocTypePK", docTypes[0].RefDocTypeName, ItemSet.WebEDocsBulkDownload.Value["AllModules"].DocTypeCollection[0].RefDocTypeName);
			AssertEquals("The element's RefDocTypePK", docTypes[1].RefDocTypeName, ItemSet.WebEDocsBulkDownload.Value["AllModules"].DocTypeCollection[1].RefDocTypeName);
		}

		public void TestServiceLevelVisibility()
		{
			AssertEquals(ItemSet.ServiceLevelVisibility.Name, "ServiceLevelVisibility");
			AssertEquals(ItemSet.ServiceLevelVisibility.Category, WebDataRegistry.ForwardingCategory);
			AssertEquals(ItemSet.ServiceLevelVisibility.Caption, "Service Levels Visibility");
			AssertEquals(ItemSet.ServiceLevelVisibility.Hint, @"This registry item controls which service levels are available to clients in WebTracker.
These settings will be the default for all organizations. You can override these defaults for each organization by changing settings in each organization's Shipper/Consignor, Service Levels tab.");
			AssertEquals(ItemSet.ServiceLevelVisibility.Storage, RegistryStorageFlags.System);
			AssertEquals(ItemSet.ServiceLevelVisibility.Options, RegistryOptions.Default);

			Assert("This system defined record should exist by default", ItemSet.ServiceLevelVisibility.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ContainsCode("D2D"));

			RegistryServiceLevelCollection testCollection = new RegistryServiceLevelCollection();
			RegistryServiceLevel testLevel = testCollection.Add(new ZGuid(), "TST", (NoResString)"testDescription", true);
			ItemSet.ServiceLevelVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testCollection);
			AssertCollectionNotContains("Only values from dbo.RefServiceLevel table can exist", testLevel, ItemSet.ServiceLevelVisibility.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestWebServiceChargeGuids()
		{
			Guid new1 = Guid.NewGuid();
			Guid new2 = Guid.NewGuid();
			ItemSet.WebServiceChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new1 + "," + new2);
			Guid[] chargeCodes = ItemSet.GetWebServiceChargeGuids(Env.CurrentCompany.PK);
			AssertEquals("Chargecodes[0]", new1, chargeCodes[0]);
			AssertEquals("Chargecodes[1]", new2, chargeCodes[1]);

			chargeCodes = ItemSet.GetWebServiceChargeGuids(Guid.NewGuid());
			AssertEquals("The chargecode list is empty", 0, chargeCodes.Length);
		}

		public void TestShowDetailedInventoryRegistryItem()
		{
			TestRegistryItem(ItemSet.ShowDetailedInventory, "ShowDetailedInventory", "", "", "", RegistryStorageFlags.CompanyDepartment, RegistryOptions.IsHidden | RegistryOptions.NotCached, true);
		}

		public void TestShowDetailedOrderLinesRegistryItem()
		{
			TestRegistryItem(ItemSet.ShowDetailedOrderLines, "ShowDetailedOrderLines", "", "", "", RegistryStorageFlags.CompanyDepartment, RegistryOptions.IsHidden | RegistryOptions.NotCached, true);
		}

		public void TestAllowedLanguages()
		{
			var testLanguage = Factory.New<IRefLocalLanguage>();
			testLanguage.RA_Code = "EN";
			testLanguage.RA_RN_NKCountryCode = "NZ";
			testLanguage.RA_Description = "A custom language";
			Factory.Save();
			var expected = new CodeDescriptionPairList(OLookUpEditType.Language);
			AssertEquals("Custom language should be included", "A custom language", ItemSet.AllowedLanguages.Value.Codes.GetDescriptionFromCode("EN-NZ"));
			AssertContainsExactElementsInAnyOrder(expected, ItemSet.AllowedLanguages.Value.Codes);
			AssertContainsExactElementsInAnyOrder(expected, ItemSet.AllowedLanguages.DefaultValue);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Testing")]
		public void TestEmailPassword()
		{
			AssertVisible(ItemSet.EmailPasswordTemplate);

			AssertEquals("Name", "EmailPasswordTemplate", ItemSet.EmailPasswordTemplate.Name);
			AssertEquals("Category", WebDataRegistry.WebCategory, ItemSet.EmailPasswordTemplate.Category);
			AssertEquals("Caption", "Email Password Template", ItemSet.EmailPasswordTemplate.Caption);
			AssertEquals("Hint", "Template that will be used in Web Access Password Emails.", ItemSet.EmailPasswordTemplate.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.EmailPasswordTemplate.Storage);
			AssertNotEquals("DocSourceType", typeof(DocumentWrappers.IDocContactPasswordEmail), ItemSet.EmailPasswordTemplate.DefaultValue.DocSourceType);
			Type expectedDocSourceType = ObjectFactory.GetType<DocumentWrappers.IDocContactPasswordEmail>();
			AssertEquals("DocSourceType - successful lookup", expectedDocSourceType, ItemSet.EmailPasswordTemplate.DefaultValue.DocSourceType);

			AssertEquals("Default", $"{Core.Constants.ProductName} Web Access Password", ItemSet.EmailPasswordTemplate.Value.EmailSubject);
			AssertEquals("Default", ItemSet.EmailPasswordTemplateDefaultBody, ItemSet.EmailPasswordTemplate.Value.EmailBody);

			ItemSet.EmailPasswordTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate(expectedDocSourceType, "ediEnterprise Web Access Password", "Email body template"));
			AssertEquals("ediEnterprise Web Access Password", ItemSet.EmailPasswordTemplate.Value.EmailSubject);
			AssertEquals("Email body template", ItemSet.EmailPasswordTemplate.Value.EmailBody);
		}

		public void TestCheckHealthIPWhitelist()
		{
			EnvProxy.SetHostedLocationForTest("");

			var newItemSet = GetNewItemSet();
			TestGenericRegistryItem(
				newItemSet.HealthCheckAccessIPWhitelistRegistryItem,
				"HealthCheckAccessIPWhitelist",
				"Web and Visibility",
				"Health Check Access IP White list",
				@"Only the IP addresses in this list can access the health check page. This white list supports IP subnets, IP ranges and single addresses. e.g. 192.168.0.0/24 or 192.168.1.1-192.168.1.20 or 192.168.3.1 or 2001:0001::/64",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController);

			AssertEquals(2, newItemSet.HealthCheckAccessIPWhitelistRegistryItem.DefaultValue.Count);

			var defaultRule1 = newItemSet.HealthCheckAccessIPWhitelistRegistryItem.DefaultValue[0];
			var defaultRule2 = newItemSet.HealthCheckAccessIPWhitelistRegistryItem.DefaultValue[1];

			AssertEquals("127.0.0.1/8", defaultRule1.Text);
			Assert(defaultRule1.Enabled);

			AssertEquals("::1", defaultRule2.Text);
			Assert(defaultRule2.Enabled);

			EnvProxy.SetHostedLocationForTest("SYD");
			newItemSet = GetNewItemSet();
			RegistryItemDictionary.Instance.PurgeAll();
			AssertEquals(RegistryOptions.IsOnlyEditableBySupportIfHosted, newItemSet.HealthCheckAccessIPWhitelistRegistryItem.Options);
		}

		public void TestShowNewTrackingPortal()
		{
			TestRegistryItem(ItemSet.ShowNewTrackingPortal, "ShowNewTrackingPortal", WebDataRegistry.WebCategory, "Show Link to the New Tracking Portal", "Set this to 'Yes' to display a link inviting WebTracker users to try the new tracking portal.", sysFlags, RegistryOptions.IsHidden, false);
		}

		public void TestWebtrackerPreloadModules()
		{
			var registryItem = ItemSet.WebTrackerPreloadModules;

			AssertEquals("WebTrackerPreloadModules", registryItem.Name);
			AssertEquals(WebDataRegistry.PerformanceAndAppearanceCategory, registryItem.Category);
			AssertEquals("WebTracker Modules to Pre-load", registryItem.Caption);
			AssertEquals("WebTracker modules that should be immediately loaded after an upgrade instead of on-demand.", registryItem.Hint);
			AssertEquals(RegistryStorageFlags.System, registryItem.Storage);
			AssertEquals(RegistryOptions.IsOnlyEditableBySupportIfHosted, registryItem.Options);

			var defaultValue = ItemSet.WebTrackerPreloadModules.DefaultValue;
			var defaultShipmentsValue = defaultValue.OfType<CodeDescriptionBool>().FirstOrDefault(x => x.Code == WebTrackerPreloadModulesList.Codes.Shipments);
			Assert(defaultShipmentsValue.Bool);
		}

		public void TestWebLoginAttempts()
		{
			TestRegistryItem(
				ItemSet.WebLoginAttempts,
				nameof(ItemSet.WebLoginAttempts),
				WebDataRegistry.PasswordControlCategory,
				"Login Attempts",
				"Number of failed login attempts before web user is locked out. (0 = Do not lockout).",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				0);

			var type = ItemSet.WebLoginAttempts.DataType as IntRegistryDataType;
			AssertNotNull(type);

			AssertEquals(type.LowerBound, ((double)0));
			AssertEquals(type.UpperBound, ((double)Int32.MaxValue));
		}

		public void TestWebLoginLockoutMinutes()
		{
			TestRegistryItem(
				ItemSet.WebLoginLockoutMinutes,
				nameof(ItemSet.WebLoginLockoutMinutes),
				WebDataRegistry.PasswordControlCategory,
				"Lockout Minutes",
				"Number of minutes to lockout web user after failed login attempts.(0 = Requires manual reset)",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				15);

			var type = ItemSet.WebLoginLockoutMinutes.DataType as IntRegistryDataType;
			AssertNotNull(type);

			AssertEquals(type.LowerBound, ((double)0));
			AssertEquals(type.UpperBound, ((double)Int32.MaxValue));
		}

		public void TestWebPasswordMinLength()
		{
			TestRegistryItem(
				ItemSet.WebPasswordMinLength,
				nameof(ItemSet.WebPasswordMinLength),
				WebDataRegistry.PasswordControlCategory,
				"Minimum Length",
				"Minimum length of password.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				12,
				8,
				15);
		}

		public void TestWebPasswordHistoryCount()
		{
			TestRegistryItem(
				ItemSet.WebPasswordHistoryCount,
				nameof(ItemSet.WebPasswordHistoryCount),
				WebDataRegistry.PasswordControlCategory,
				"History Count",
				"Number of previously used passwords to remember.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				0,
				0,
				15);
		}

		public void TestEnableWebPasswordComplexityRules()
		{
			TestRegistryItem(
				ItemSet.EnableWebPasswordComplexityRules,
				nameof(ItemSet.EnableWebPasswordComplexityRules),
				WebDataRegistry.PasswordControlCategory,
				"Enable Password Complexity Rules",
				"The rules will be enforced when a user sets a new password. Password must contain at least three of the following: uppercase letters, lowercase letters, numbers, symbols, and non-European alphabet characters.",
				RegistryStorageFlags.System,
				RegistryOptions.Default, false);
		}

		public void TestPasswordRotation()
		{
			TestRegistryItem(
				ItemSet.WebPasswordRotationDays,
				nameof(ItemSet.WebPasswordRotationDays),
				WebDataRegistry.PasswordControlCategory,
				"Password Change Days",
				"Force password rotation every (n) days. Set to 0 to disable password rotation.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				0,
				0,
				Int32.MaxValue);

			TestGenericRegistryItem(ItemSet.WebPasswordRotationEffectiveDate, nameof(ItemSet.WebPasswordRotationEffectiveDate),
				WebDataRegistry.PasswordControlCategory, "Password Rotation Effective Date",
				"Effective Date when the Password Rotation comes into effect. \r\nFor example, if a last password change date is known, that can be used to determine whether a password has expired. If a password has never been changed, this date is used as a fallback in the calculation to determine whether a password has expired.", RegistryStorageFlags.System, RegistryOptions.Default, DateTime.MinValue);
		}

		public void TestSendPasswordInstructions()
		{
			TestGenericRegistryItem(ItemSet.ContactCreatedStartFromDate, nameof(ItemSet.ContactCreatedStartFromDate),
				WebDataRegistry.WebCategory, "Send Password Instructions",
				"Overriding this registry setting will enable the SPI Service Task. \r\n\r\nThe default value is No - Do not run the Service Task. \r\n\r\nIf overridden, the SPI service task will be enabled and run for Organization Contacts that fulfill all of the following conditions: \r\n- The Contact has been created since the specified Date \r\n- Password Instructions have never been sent to the Contact \r\n- The Contact has Web Access enabled \r\n- The Contact is Active.", RegistryStorageFlags.System, RegistryOptions.Default, DateTime.MinValue);
		}

		public void TestContactRedirectionExpiry()
		{
			AssertEquals("ContactRedirectionExpiryDays.DefaultValue", 30, ItemSet.ContactRedirectionExpiryDays.DefaultValue);

			ItemSet.ContactRedirectionExpiryDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			AssertEquals("Should be set", 1, ItemSet.ContactRedirectionExpiryDays.Value);

			AssertEquals("Expected error", "Value must be greater than or equal to the minimum (1)",
				ItemSet.ContactRedirectionExpiryDays.GetValidationErrorMessage(0, Guid.Empty, Guid.Empty, Guid.Empty));

			AssertEquals("Expected error", "Value must be less than or equal to the maximum (1000)",
				ItemSet.ContactRedirectionExpiryDays.GetValidationErrorMessage(1001, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestPasswordBannedWordList()
		{
			TestGenericRegistryItem(
				ItemSet.PasswordBannedWordList,
				"PasswordBannedWordList",
				WebDataRegistry.PasswordControlCategory,
				"Password Banned Word List",
				"Use this registry item to store the list of words (or string values) that cannot be used by users when they set or reset passwords.\r\nThese values could be text contained within a password, for example, the word 'not' in 'donotuse'.\r\nPassword checks against this list are not case sensitive. For example, 'Word', 'WoRd' or 'WORD' will be assessed as the same word.",
				RegistryStorageFlags.System,
				RegistryOptions.Default);
		}

		public void TestEnableWebPasswordNameAndEmailValidation()
		{
			TestRegistryItem(
				ItemSet.EnableWebPasswordNameAndEmailValidation,
				nameof(ItemSet.EnableWebPasswordNameAndEmailValidation),
				WebDataRegistry.PasswordControlCategory,
				"Enable Web Password Name And Email Validation",
				"When this registry item is enabled, the name and email address password policy rule will be enforced for web portal users when they set a new password.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				true);
		}

		public void TestLoginFailureAttemptSecretKey()
		{
			var registryItem = ItemSet.LoginFailureAttemptSecretKey;

			AssertEquals("LoginFailureAttemptSecretKey", registryItem.Name);
			AssertEquals(WebDataRegistry.WebCategory, registryItem.Category);
			AssertEquals("Login Failure Attempt Secret Key", registryItem.Caption);
			AssertEquals("Key used for hash of My Account and WebTracker login failure. The key should be 64 bytes long.", registryItem.Hint);
			AssertEquals(RegistryStorageFlags.System, registryItem.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, registryItem.Options);
			Assert("Default value is empty", string.IsNullOrEmpty(registryItem.DefaultValue));
		}

		public void TestFeatureControlRuleContent()
		{
			var registryItem = ItemSet.FeatureControlRuleContent;
			AssertEquals("FeatureControlRuleContent", registryItem.Name);
			AssertEquals(WebDataRegistry.TrustedMessagingCategory, registryItem.Category);
			AssertEquals("Feature Control Rule Content", registryItem.Caption);
			AssertEquals("Feature Control Rule Content", registryItem.Hint);
			AssertEquals(RegistryStorageFlags.System, registryItem.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport | RegistryOptions.NotCached, registryItem.Options);
			Assert("Default value is empty", string.IsNullOrEmpty(registryItem.DefaultValue));
			AssertEquals(true, ((StringRegistryDataType)registryItem.DataType).IsEncrypted);
		}

		#region Implementation

		const RegistryOptions _default = RegistryOptions.Default;
		const RegistryStorageFlags sysFlags = RegistryStorageFlags.System;

		void DefaultFilterLayoutTest(FilterLayoutCodePairRegistryItem item, string caption, string category, string registryItemName, string moduleName, RegistryOptions options)
		{
			TestRegistryItem(item,
							 registryItemName,
							 category,
							 "Default Filter Layout",
							 WebDataRegistry.GetHintForDefaultFilterLayoutRegistryItems((NoResString)caption),
							 RegistryStorageFlags.Company,
							 options
							);
		}

		void NotificationOptionsTest(CodePairRegistryItem item, string caption, string category, string registryItemName)
		{
			TestRegistryItem(item,
							 registryItemName,
							 category,
							 "Notification Options",
							WebDataRegistry.GetHintForNotificationOptions((NoResString)caption),
							 sysFlags,
							 _default,
							 new SendingRuleCodeDescriptionPairList(),
							 "ALL");
		}

		void UseModuleTest(BooleanRegistryItem item, string caption, string category, string registryItemName, RegistryOptions options, bool defaultValue, bool isWarehouse = false)
		{
			TestRegistryItem(item,
							registryItemName,
							category,
							string.Format("Show {0} tab", caption),
							isWarehouse ? WebDataRegistry.GetHintForUseModuleRegistryItemsForWarehouse((NoResString)caption) : WebDataRegistry.GetHintForUseModuleRegistryItems((NoResString)caption),
							 sysFlags,
							 options,
							 defaultValue);
		}

		void UseModuleTest(BooleanRegistryItem item, string caption, string category, string registryItemName, bool isWarehouse = false)
		{
			UseModuleTest(item, caption, category, registryItemName, _default, true, isWarehouse);
		}

		void SuppressionItemTest(CodeDescriptionBoolRegistryItem item, string caption, string registryItemName, string hint, RegistryOptions options = RegistryOptions.Default)
		{
			TestRegistryItem(item, registryItemName, WebDataRegistry.SuppressFlightDetailsCategory, caption, RegistrySuppressionHelper.GetWebHint((NoResString)hint), RegistryStorageFlags.Company | RegistryStorageFlags.Branch, options, "Suppress", false, Enum.GetNames(typeof(SuppressFields)).Length, RegistrySuppressionHelper.GetDefaultFields(Env.CurrentBranch.PK).ToArray());
		}

		void StaffRolesToNotifyTest(CodeDescriptionBoolRegistryItem item, string caption, string category, string registryItemName, params string[] staffAssignments)
		{
			AssertNotNull("Value should not be null", item.Value);

			AssertEquals(registryItemName, item.Name);
			AssertEquals(category, item.Category);
			AssertEquals("Notification Staff Roles", item.Caption);
			AssertEquals(string.Format("The staff roles that will be sent email notifications for {0} additions or modifications made via WebTracker.", caption), item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, item.Storage);
			AssertEquals("Send Notification To", ((CodeDescriptionBoolRegistryEditorInfo)item.EditorInfo).BoolColumnCaption);
			Assert("Bool column should be visible", ((CodeDescriptionBoolRegistryEditorInfo)item.EditorInfo).IsBoolColumnVisible);
			Assert("Only Bool Column should be editable", ((CodeDescriptionBoolRegistryEditorInfo)item.EditorInfo).IsOnlyBoolColumnEditable);

			StaffRolesNotificationTestHelper.AssertDefaultRoles(item.Value);
			StaffRolesNotificationTestHelper.AssertBoolValue(item.Value, true, true, staffAssignments);
			StaffRolesNotificationTestHelper.AssertBoolValue(item.Value, false, false, staffAssignments);

			CodeDescriptionBoolDisallowNewCollection newRoles = new CodeDescriptionBoolDisallowNewCollection();
			CodeDescriptionBoolDisallowNew role = newRoles.AddNew();
			role.Code = "TST";
			role.Bool = true;

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newRoles);

			AssertEquals("Should be one element", 1, item.Value.Count);
			AssertEquals("The element's Code should be TST", "TST", item.Value[0].Code);
			AssertEquals("The element's Bool should be True", true, item.Value[0].Bool);
		}

		void OrgsRolePropertySuppressionRegistryItemTest(AccessControlRegistryItem item, string caption, string category, string registryItemName, Type typeOfAccessRules)
		{
			AssertNotNull("Value should not be null", item.Value);
			AssertEquals(registryItemName, item.Name);
			AssertEquals(category, item.Category);
			AssertEquals("Access Control", item.Caption);
			AssertEquals(WebDataRegistry.GetHintForOrgsRolePropertySuppressionRegistryItems((NoResString)caption), item.Hint);
			AssertEquals(typeOfAccessRules, item.accessRules.GetType());
		}

		void NotificationEmailGroupTest(GuidRegistryItem item, string caption, string category, string registryItemName, RegistryOptions options = RegistryOptions.IsValueMandatory)
		{
			TestRegistryItem(item,
							 registryItemName,
							 category,
							 "Notification Group",
							 string.Format("Send email notifications to this group when additions or modifications are made in the {0} tab.", caption),
							 RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							 options,
							 RegistryFindBoxCollection.GlbGroup,
							 RegistryFactory.Instance.GetGroupPK("ALL"));

			Guid allUsersGroup = Factory.LoadTop1<IGlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, "ALL")).PK.ToGuid();
			AssertEquals("DefaultValue", allUsersGroup, item.DefaultValue);

			Guid newGuid = Guid.NewGuid();
			WebDataRegistry.Instance.BookingNotificationEmailGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);
			AssertEquals("Value", newGuid, WebDataRegistry.Instance.BookingNotificationEmailGroup.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		protected override IEnumerable<string> ConditionallyVisibleRegistryItems
		{
			get
			{
				return new[]
				{
					"UseWebMAWBModule",
					"DefaultFilterLayoutMAWB",
					"UseWebHAWBModule",
					"DefaultFilterLayoutHAWB",
					"Theme",
					"WebCertificationUrl",
					"EventVisibilityOverride",
					"EventSortOrderOfNeo",
					nameof(WebDataRegistry.LoginFailureAttemptSecretKey),
				};
			}
		}

		#endregion
	}
}
