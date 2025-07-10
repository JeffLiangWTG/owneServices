using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Registry.Business.Web;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Constants = Enterprise.Core.Constants;
using EventSortOrderList = Enterprise.Registry.Business.Web.EventSortOrderList;
using SharedGlowRegistry = CargoWise.Definitions.GlowRegistry;

namespace Enterprise.Registry.Business
{
	public sealed class WebDataRegistry : RegistryItemSet
	{
		#region Construction

		WebDataRegistry()
		{
		}

		public static WebDataRegistry Instance
		{
			get { return fInstance ?? (fInstance = new WebDataRegistry()); }
		}

		[ThreadStatic]
		static WebDataRegistry fInstance;

		T Get<T>(string registryItemName, CreateWebItemDelegate<T> createWebItemDelegate) where T : IRegistryItem
		{
			return GetItem(registryItemName, () => createWebItemDelegate(registryItemName));
		}

		delegate T CreateWebItemDelegate<T>(string name) where T : IRegistryItem;

		#endregion

		public override bool IsForProductivityWise => true;

		#region Categories / Hints

		#region Categories

		internal static MultilingualString WebCategory { get { return RawDataRegistry.Categories.WebAndVisibility; } }

		internal static MultilingualString AccountsCategory { get { return RegistryConstants.GetCategory(WebCategory, ResString.GetMultilingualString("4398471c-e08e-4ad1-9e91-55d3cee8d3cd", "Accounts")); } }

		internal static MultilingualString CustomsCategory { get { return RegistryConstants.GetCategory(WebCategory, ResString.GetMultilingualString("ba611ae6-ad0e-4c72-b625-ddaca6344307", "Customs")); } }
		internal static MultilingualString CustomsISFCategory { get { return RegistryConstants.GetCategory(CustomsCategory, ResString.GetMultilingualString("b86b2448-eb5e-4454-83cd-c95a4b784927", "ISF")); } }
		internal static MultilingualString CustomsDeclarationCategory { get { return RegistryConstants.GetCategory(CustomsCategory, ResString.GetMultilingualString("de96cc9c-ab19-4d2e-b8fb-402007ed0841", "Declaration")); } }
		internal static MultilingualString CustomsUSCForeignPortCategory { get { return RegistryConstants.GetCategory(CustomsCategory, ResString.GetMultilingualString("2fd5d084-ef2e-4d61-b40a-62b30add3bbb", "Schedule K Port Codes")); } }
		internal static MultilingualString CustomsUSCRegionDistrictPortCategory { get { return RegistryConstants.GetCategory(CustomsCategory, ResString.GetMultilingualString("f5bf464f-20f5-46a7-989a-24ec705c8604", "Schedule D Port Codes")); } }
		internal static MultilingualString CustomsReportsCategory { get { return RegistryConstants.GetCategory(CustomsCategory, ResString.GetMultilingualString("e28bfd45-f200-4d2e-96e6-dd38d21ce8b8", "Reports")); } }

		internal static MultilingualString TransportCategory { get { return RegistryConstants.GetCategory(WebCategory, ResString.GetMultilingualString("5d9e1173-5659-4c4e-b916-edd5d850b19a", "Port Transport")); } }
		internal static MultilingualString TransportReportsCategory { get { return RegistryConstants.GetCategory(TransportCategory, ResString.GetMultilingualString("e28bfd45-f200-4d2e-96e6-dd38d21ce8b8", "Reports")); } }
		internal static MultilingualString TransportJobsCategory { get { return RegistryConstants.GetCategory(TransportCategory, ResString.GetMultilingualString("399bed74-fe55-4ef0-a4f4-d094d082345d", "Transport Jobs")); } }
		internal static MultilingualString CFSCategory { get { return RegistryConstants.GetCategory(WebCategory, ResString.GetMultilingualString("8EB6F12D-E701-432C-8334-42A2402E9CB0", "CFS")); } }
		internal static MultilingualString CFSShipmentsCategory { get { return RegistryConstants.GetCategory(CFSCategory, ResString.GetMultilingualString("BF8F2477-C076-4C6C-8F2E-C66B6ADA7DA3", "CFS Shipments")); } }
		internal static MultilingualString ForwardingCategory { get { return RegistryConstants.GetCategory(WebCategory, ResString.GetMultilingualString("7feac298-4850-4497-ae61-2ab2a5ddf23f", "Forwarding")); } }
		internal static MultilingualString ForwardingShipmentsCategory { get { return RegistryConstants.GetCategory(ForwardingCategory, ResString.GetMultilingualString("fe4aa05f-ae2e-495a-aedb-e8dc6398c66d", "Shipments")); } }
		internal static MultilingualString ForwardingBookingsCategory { get { return RegistryConstants.GetCategory(ForwardingCategory, ResString.GetMultilingualString("9d562cea-9b4b-44d5-8ccd-fb63956f6c67", "Bookings")); } }
		internal static MultilingualString ForwardingOrdersCategory { get { return RegistryConstants.GetCategory(ForwardingCategory, ResString.GetMultilingualString("cee5cf86-58b6-4C37-9d94-d7f52c399990", "Orders")); } }
		internal static MultilingualString ForwardingOrderLinesCategory { get { return RegistryConstants.GetCategory(ForwardingCategory, ResString.GetMultilingualString("95ae77be-4345-45d3-a864-493ce79aaa8f", "Order Lines")); } }
		internal static MultilingualString ForwardingContainersCategory { get { return RegistryConstants.GetCategory(ForwardingCategory, ResString.GetMultilingualString("162739d6-3f50-43b2-8513-61fce12efb91", "Containers")); } }
		internal static MultilingualString ForwardingQuotesCategory { get { return RegistryConstants.GetCategory(ForwardingCategory, ResString.GetMultilingualString("8a23668c-5323-4695-83ff-4055c74581a3", "Quotes")); } }
		internal static MultilingualString ForwardingReportsCategory { get { return RegistryConstants.GetCategory(ForwardingCategory, ResString.GetMultilingualString("e28bfd45-f200-4d2e-96e6-dd38d21ce8b8", "Reports")); } }
		internal static MultilingualString ForwardingMAWBCategory { get { return RegistryConstants.GetCategory(ForwardingCategory, ResString.GetMultilingualString("0941df2b-b095-46ae-99b8-8371eb457cc9", "MAWBs")); } }
		internal static MultilingualString ForwardingHAWBCategory { get { return RegistryConstants.GetCategory(ForwardingCategory, ResString.GetMultilingualString("92ced673-4193-4098-876a-6715361c8003", "HAWBs")); } }

		internal static MultilingualString ForwardingFlightsCategory { get { return RegistryConstants.GetCategory(ForwardingCategory, ResString.GetMultilingualString("77cdc929-0199-4d1c-a19d-5d0e12acb01f", "Flights")); } }
		internal static MultilingualString ForwardingSailingsCategory { get { return RegistryConstants.GetCategory(ForwardingCategory, ResString.GetMultilingualString("4f2f4b05-9432-4b4f-bb0e-33ef19c61e46", "Sailings")); } }
		internal static MultilingualString ForwardingRoadCategory { get { return RegistryConstants.GetCategory(ForwardingCategory, ResString.GetMultilingualString("e88a22f8-a635-46a6-b2b0-541f835b0885", "Road")); } }
		internal static MultilingualString ForwardingRailCategory { get { return RegistryConstants.GetCategory(ForwardingCategory, ResString.GetMultilingualString("36be25e8-63b4-4ebd-b374-17f197cf23eb", "Rail")); } }
		internal static MultilingualString ForwardingHVLVCategory { get { return RegistryConstants.GetCategory(ForwardingCategory, ResString.GetMultilingualString("c360fadf-1458-4b6e-8d9a-36286362a527", "HVLV")); } }

		internal static MultilingualString LoginQuickViewCategory { get { return RegistryConstants.GetCategory(WebCategory, ResString.GetMultilingualString("74e294c6-1108-4d33-986d-8526003f795e", "Login and Quick View")); } }
		internal static MultilingualString LoginQuickViewCanadaCategory { get { return RegistryConstants.GetCategory(LoginQuickViewCategory, ResString.GetMultilingualString("8bcd446a-1e7c-4477-b8f4-75cc46003649", "Canada")); } }
		internal static MultilingualString MilestonesCategory { get { return RegistryConstants.GetCategory(WebCategory, ResString.GetMultilingualString("167d7b40-35a4-4e23-b758-578d3efadb22", "Milestones")); } }
		internal static MultilingualString EventsCategory { get { return RegistryConstants.GetCategory(WebCategory, ResString.GetMultilingualString("6d065869-0e4c-4ed0-bae3-c8031697323e", "Events")); } }
		internal static MultilingualString WebTrackerCategory { get { return RegistryConstants.GetCategory(EventsCategory, ResString.GetMultilingualString("584ab4a9-d906-475f-b766-d80e40883a81", "WebTracker")); } }
		internal static MultilingualString NeoCategory { get { return RegistryConstants.GetCategory(EventsCategory, ResString.GetMultilingualString("c58a7c73-c598-4be8-b8ae-0f6d6538afcf", "Neo")); } }
		internal static MultilingualString PasswordControlCategory { get { return RegistryConstants.GetCategory(WebCategory, ResString.GetMultilingualString("12FD2A53-7C9B-4CE4-9381-1B0315AB3D44", "Password Control")); } }
		internal static MultilingualString PasswordResetCategory { get { return RegistryConstants.GetCategory(WebCategory, ResString.GetMultilingualString("488b028d-54c0-4782-a6d7-cfe37c7eac27", "Password Reset")); } }
		internal static MultilingualString PasswordSetCategory { get { return RegistryConstants.GetCategory(WebCategory, ResString.GetMultilingualString("e030b311-9ce0-439c-97ea-d94e87f29678", "Password Set")); } }
		internal static MultilingualString PerformanceAndAppearanceCategory { get { return RegistryConstants.GetCategory(WebCategory, ResString.GetMultilingualString("00a149eb-e823-49d0-86a0-867cadca83bc", "Performance and Appearance")); } }
		internal static MultilingualString WebTrackerThemeCategory { get { return RegistryConstants.GetCategory(PerformanceAndAppearanceCategory, ResString.GetMultilingualString("79f1f116-2309-475e-a20c-9083b127b2c9", "WebTracker Enhanced Theme")); } }
		internal static MultilingualString WebCFSThemeCategory { get { return RegistryConstants.GetCategory(PerformanceAndAppearanceCategory, ResString.GetMultilingualString("7366CB27-E488-4FC4-B7AD-226D5AD9D5EE", "WebCFS Enhanced Theme")); } }
		internal static MultilingualString WebCampaignThemeCategory { get { return RegistryConstants.GetCategory(PerformanceAndAppearanceCategory, ResString.GetMultilingualString("30ba9e5f-5e54-4893-94dc-0ab009dd1c69", "WebCampaign Enhanced Theme")); } }
		internal static MultilingualString ComponentsURLsCategory { get { return RegistryConstants.GetCategory(WebCategory, ResString.GetMultilingualString("9ade1253-54c1-4060-9dc8-786b73bb839a", "Web Component URLs")); } }
		internal static MultilingualString TrustedMessagingCategory { get { return RegistryConstants.GetCategory(WebCategory, ResString.GetMultilingualString("83d1497f-27d7-4a01-82b1-ada06cc03734", "Trusted Messaging")); } }

		internal static MultilingualString LinerAndAgencyCategory { get { return RegistryConstants.GetCategory(WebCategory, ResString.GetMultilingualString("3ac6efbb-ecdf-4007-b2e8-32e87adca724", "Liner & Agency")); } }
		internal static MultilingualString LinerAndAgencyContainersCategory { get { return RegistryConstants.GetCategory(LinerAndAgencyCategory, ResString.GetMultilingualString("FA41B3CE-AF72-4048-950B-6272F3C9677A", "Containers")); } }
		internal static MultilingualString LinerAndAgencyBookingsCategory { get { return RegistryConstants.GetCategory(LinerAndAgencyCategory, ResString.GetMultilingualString("27a8c7a8-9c33-443c-96bd-77cfe37a10e6", "Bookings")); } }
		internal static MultilingualString LinerAndAgencyBillsOfLadingCategory { get { return RegistryConstants.GetCategory(LinerAndAgencyCategory, ResString.GetMultilingualString("7b6e344e-0ccc-4255-bcf3-ec7d032d506c", "Bills of Lading")); } }
		internal static MultilingualString LinerAndAgencyReportsCategory { get { return RegistryConstants.GetCategory(LinerAndAgencyCategory, ResString.GetMultilingualString("e28bfd45-f200-4d2e-96e6-dd38d21ce8b8", "Reports")); } }

		internal static MultilingualString WarehouseCategory { get { return RegistryConstants.GetCategory(WebCategory, ResString.GetMultilingualString("8f998ebb-be11-4bbb-ac58-c1834d088fe3", "Warehouse")); } }
		internal static MultilingualString WarehouseInventoryCategory { get { return RegistryConstants.GetCategory(WarehouseCategory, ResString.GetMultilingualString("49973bd5-0f79-4f32-b5f3-7e5d4fb2cc9d", "Inventory")); } }
		internal static MultilingualString WarehouseOrdersCategory { get { return RegistryConstants.GetCategory(WarehouseCategory, ResString.GetMultilingualString("fef1b09b-03ac-4470-80da-0a76ba9391f6", "Orders")); } }
		internal static MultilingualString WarehouseReceiptsCategory { get { return RegistryConstants.GetCategory(WarehouseCategory, ResString.GetMultilingualString("4e3a9077-9136-48ac-93d8-2a42ee33da81", "Receipts")); } }
		internal static MultilingualString WarehouseProductsCategory { get { return RegistryConstants.GetCategory(WarehouseCategory, ResString.GetMultilingualString("5b0c1d86-1df3-495b-8f9c-cfd7c5470aaa", "Products")); } }
		internal static MultilingualString WarehouseReportsCategory { get { return RegistryConstants.GetCategory(WarehouseCategory, ResString.GetMultilingualString("e28bfd45-f200-4d2e-96e6-dd38d21ce8b8", "Reports")); } }
		internal static MultilingualString WarehousesCategory { get { return RegistryConstants.GetCategory(WarehouseCategory, ResString.GetMultilingualString("16631a50-63e0-4c94-9bf2-bad92c550c79", "Warehouses")); } }

		internal static MultilingualString WebServicesCategory { get { return RegistryConstants.GetCategory(WebCategory, ResString.GetMultilingualString("4d016b13-f4ce-4cc9-898f-512a0d11fd8b", "Web Services")); } }
		internal static MultilingualString WebSiteIntegrationCategory { get { return RegistryConstants.GetCategory(WebCategory, ResString.GetMultilingualString("6c3958b1-a52f-4161-bb8f-385b92056719", "Web Site Integration")); } }

		internal static MultilingualString ModuleSettings { get { return RegistryConstants.GetCategory(WebCategory, ResString.GetMultilingualString("473f25d9-5449-4d11-861e-82a7c1aa404b", "Module Settings")); } }

		#endregion

		#region Hints

		internal static MultilingualString HintForBookingPackLinesPageSize
		{
			get
			{
				return ResString.GetMultilingualString("c86b3a20-c532-4172-ac0d-6ada091bdca5", @"Specify the number of lines per page for the Goods/Packs grid when placing or editing bookings. Enter zero to disable paging.");
			}
		}

		internal static MultilingualString HintForWarehouseInventoryDetailsPageSize => ResString.GetMultilingualString("f5ca9c31-59bc-4435-b5f0-a5df4df5a85e", @"Specify the number of inventory detail lines per page for the Inventory grid. Enter zero to disable paging.");

		internal static MultilingualString HintForServiceLevelVisibility
		{
			get
			{
				return ResString.GetMultilingualString("a774ff5a-566e-424a-8b6e-159ce2a5fbf3", @"This registry item controls which service levels are available to clients in WebTracker.
These settings will be the default for all organizations. You can override these defaults for each organization by changing settings in each organization's Shipper/Consignor, Service Levels tab.");
			}
		}

		internal static MultilingualString HintForBookingTermsAndConditions
		{
			get
			{
				return ResString.GetMultilingualString("642a94ec-563e-4b8c-a05c-a1acd0eeba86", @"Booking Terms and Conditions.
When a client makes a Booking in your web tracking system, they must agree to these conditions before they can place a booking.");
			}
		}

		internal static MultilingualString HintForPrintAddressesOnFreightLabels
		{
			get
			{
				return ResString.GetMultilingualString("69d61aef-e968-4a1d-9dff-1007b72850a4", @"Controls printing of Shipper and Consignee addresses on Freight Labels printed from WebTracker.

The default is 'Yes' - the Shipper and Consignee addresses will be shown on the Freight Labels.
If overridden to 'No' - the Shipper and Consignee addresses will not be shown on the Freight Labels.");
			}
		}

		internal static MultilingualString HintForDisableBookingInsuranceValue
		{
			get { return ResString.GetMultilingualString("63aab053-b495-4856-afa8-7c7b5f38004d", "Change this Registry item to 'Yes' to hide Insurance Value on the Booking page."); }
		}

		internal static MultilingualString HintForDisableBookingContainersGrid
		{
			get { return ResString.GetMultilingualString("ce5dfc80-9680-4974-9701-432d9c4cf208", "Change to 'Yes' this Registry item value to hide Containers Grid on the Booking page."); }
		}

		internal static MultilingualString HintForDisableBookingGoodsPacksGrid
		{
			get { return ResString.GetMultilingualString("186ad75d-6aee-4bec-8a64-dee9c329f385", "Change to 'Yes' this Registry item value to hide Goods / Packs Grid on Booking page."); }
		}

		internal static MultilingualString HintBookingAttachOrdersWithoutSupplier
		{
			get
			{
				return ResString.GetMultilingualString("7927999f-108d-44b8-840e-e6688248bccf", @"Controls whether orders where no Supplier is specified can be attached to a booking.

The default is 'No' - Only orders where the logged in organization is a Supplier, Buyer or Carrier can be attached.

If overridden to 'Yes' - Orders where the Supplier is not specified, and the Buyer matches Booking Consignee, can be attached.");
			}
		}

		internal static MultilingualString HintForShowContainerStatusFromShipment
		{
			get
			{
				return ResString.GetMultilingualString("45dc8142-3e45-4fb7-a99a-0d90cb8ca96b", @"Set this to 'Yes' to show a 'Shipment Status' field on the Container Pages in WebTracker.
This 'Shipment Status' field is taken from the related Shipment's 'Custom Text 1'");
			}
		}

		internal static MultilingualString HintForOrderTrackingDatesOrMilestones
		{
			get
			{
				return ResString.GetMultilingualString("9e387b1b-69f5-4067-970b-8e6e1e83ada9", @"This setting allows you to configure your WebTracker system so that it shows Milestones, Order Tracking Dates or both.
Note: The Milestones and Order Tracking Dates show slightly different information - we suggest you use one or the other but not both.");
			}
		}

		internal static MultilingualString HintForUseShipmentPageUnShippedOrders
		{
			get
			{
				return ResString.GetMultilingualString("639bfb23-68d4-4571-8183-1963b7efd1f4", @"This Registry item controls whether the ‘Show Un-shipped Orders’ option is available on the Shipments page of WebTracker.

You can show an additional grid underneath the main Shipments grid that will show all Orders that are not attached to Shipments (allowing your clients to see all of their current jobs on one page).

The default is ‘NO’ - web users will see a single grid for Shipments on the Shipments page (they will not have an option to show the un-shipped orders grid).

If overridden to ‘YES’ - your web users will see a link at the top of the Shipments page.
They can click this link to show or hide the Un-shipped Orders grid at the bottom of the Shipments page.");
			}
		}

		internal static MultilingualString HintForWebTrackerLoginRequiresCompanyCode
		{
			get
			{
				return ResString.GetMultilingualString("03422769-2876-49a1-8755-f4bad890d67b", @"Set WebTracker login to require or not require web user’s Company Code.

The default is 'Yes' – Company Code is required in addition to email address and password.
If overridden to 'No' - only email address and password are required.");
			}
		}

		internal static MultilingualString HintForWebTrackerShipmentQuickView
		{
			get
			{
				return ResString.GetMultilingualString("de99965f-319e-42fb-b4f4-8e0a2904e4f9", @"Controls the use of Shipment Quick View.

The default is 'Yes' - the login page displays an additional box that allows users to enter a HAWB or Shipment number to view Shipment Details without logging in.

If overridden to 'No' - the Quick View box is not shown on the login page and this feature is disabled.");
			}
		}

		internal static MultilingualString HintForWebTrackerContainerQuickView
		{
			get
			{
				return ResString.GetMultilingualString("6625818D-15A0-4BE0-BB79-81E69F9538E3", @"This Setting controls whether Container Tracking Quick View is allowed without logging in.

The default is 'No' - the Container Tracking Quick View box is not shown on the login page and this feature is disabled.

If overridden to 'Yes' - the login page displays an additional box that allows users to enter container number to view its latest movements without logging in.");
			}
		}

		internal static MultilingualString HintForWebTrackerAutoLoginRequiresPassword
		{
			get { return ResString.GetMultilingualString("c91f9a09-7e70-4af8-b5e2-80e5899abbd7", "This determines if the WebTracker auto-login link requires user to type in the password. By default password is not required."); }
		}

		internal static MultilingualString HintForWebTrackerLocalChargesOnShipmentQuickView
		{
			get
			{
				return ResString.GetMultilingualString("697f4769-c860-4ec2-870d-396fe956c970", @"Controls the display of invoicing information (Invoices and Charges) in Shipment Quick View.

The default is 'No' - invoicing information is not shown in Shipment Quick View.

If overridden to 'Yes' - invoicing information will be displayed to users that have not logged in when viewing Shipments using the Shipment Quick View feature.");
			}
		}

		internal static MultilingualString HintForPasswordChangeInstructions
		{
			get
			{
				return ResString.GetMultilingualString("1862dfdb-8b54-41b3-aaab-6d2cf67d4bbb", @"This setting allows you to enter instructions on how your client can change their own WebTracker password.

These instructions are included in a {0} generated email notification once your client has been granted access to WebTracker.", Core.Constants.ProductName);
			}
		}

		internal static MultilingualString HintForQuickViewByAdditionalReferences
		{
			get
			{
				return ResString.GetMultilingualString("60373aeb-23ec-4b74-b8f2-47cee3b1856d", @"This setting controls whether additional reference fields are searched when locating shipment via Quick View.

When set to 'No' (default), only House Bill and Shipment # fields are used.

If overridden to 'Yes', Shipper Reference # and Order # are also searched.

NOTE: When multiple matches are found, only the most recent shipment will be displayed.");
			}
		}

		internal static MultilingualString HintForWebTrackerLoginPageInstruction
		{
			get
			{
				return ResString.GetMultilingualString("b1f954e2-e2aa-4892-a9e4-fc61ba3be930", @"Login Page Instruction Text.

This text is displayed on your WebTracker login page. 
You can use plain text or text with HTML tags for your login page instructions.");
			}
		}

		internal static MultilingualString HintForWebTrackerSiteTermsAndConditions
		{
			get
			{
				return ResString.GetMultilingualString("0a69cf3f-7091-4fd0-8c78-e0b13b3635bb", @"Terms and Conditions for your WebTracker site.

If overridden, each web user must agree to these Terms and Conditions order to log in to your WebTracker site.
When a user has agreed to these terms and conditions, they will not be prompted on subsequent visits.");
			}
		}

		internal static MultilingualString HintForWebTrackerUseCanadianReferencesQuickView
		{
			get
			{
				return ResString.GetMultilingualString("074da76d-3f50-4b55-a43e-0c3a8c7017f2", @"This setting controls whether additional selection criteria, specifically Cargo Control Number (CCN) and Transaction Number, are used when locating Canadian shipments/declarations via Quick View.

When set to 'No' (default), Quick View search is performed as-is.

If overridden to 'Yes', CCN and/or Transaction Number will be used as additional selection criteria to search for eligible shipments/declarations.");
			}
		}

		internal static ResourceString DefaultValueForPasswordChangeInstructions
		{
			get { return ResString.GetMultilingualString("09d5d3cd-3e07-4082-9403-576f684b0b9e", "To change your password, simply login using the details provided in this email.<br />  You can then change your password by selecting User and then Password."); }
		}

		internal static MultilingualString HintForUseHVLVBookingHeadersAndConsignments
		{
			get
			{
				return ResString.GetMultilingualString("4891E6FD-63A0-4A5A-AD11-1369849DB460", @"Controls the display of the eCommerce tab in Neo.

The default is ‘No’ – The eCommerce tab will not be shown in Neo and will be disabled for all users.

If overridden to ‘Yes’ – The eCommerce tab will be shown in Neo.");
			}
		}

		internal static ResourceString GetDefaultValueForPasswordResetEmailSubject(string displayName)
		{
			return ResString.GetMultilingualString("08ae61b9-0713-4e8b-9a08-c0541fd59871", "{0} Website Password Reset", displayName);
		}

		internal static ResourceString GetDefaultValueForPasswordResetEmailFooter(string displayName)
		{
			return ResString.GetMultilingualString("732b18c2-6c21-4e18-826d-30686d4d5e14", @"{0} Web Administrator", displayName);
		}

		internal static MultilingualString GetHintForPasswordResetEmailFooter(MultilingualString caption)
		{
			return ResString.GetMultilingualString("7f0b8ea2-9ede-4b48-9eb7-7eeab358a7ec", "Footer of the password reset email that will be sent to users when they request a password reset.   The email will be sent in HTML format, so you must use the tag '<br />' when you wish to insert a line break.  The default footer is: '{0}'", caption);
		}

		#region Password Set

		internal static MultilingualString GetHintForPasswordSetEmailSubject(MultilingualString caption)
		{
			return ResString.GetMultilingualString("05afc3ba-21cb-49b3-af04-f14fc19a2c28", @"Subject line of the password set email that will be sent to users when they request a password set.
The default header is: '{0}'", caption);
		}

		internal static ResourceString GetDefaultValueForPasswordSetEmailFooter(string displayName)
		{
			return ResString.GetMultilingualString("0104a1f0-0977-4c79-aebd-8a38bf4dcb3f", @"{0} Web Administrator", displayName);
		}

		internal static MultilingualString GetHintForPasswordSetEmailFooter(MultilingualString caption)
		{
			return ResString.GetMultilingualString("9eeccaa1-066d-45a5-ab7b-00278ec5d694", "Footer of the password set email that will be sent to users when they request a password set.   The email will be sent in HTML format, so you must use the tag '<br />' when you wish to insert a line break.  The default footer is: '{0}'", caption);
		}

		internal static ResourceString DefaultValueForPasswordSetEmailUnsuccessfulMessage
		{
			get { return ResString.GetMultilingualString("a903f9ac-4e60-4e99-86fe-de3f09eb5ba3", @"An error was encountered while sending the password set email."); }
		}

		internal static MultilingualString HintForPasswordSetEmailUnsuccessfulMessage
		{
			get
			{
				return ResString.GetMultilingualString("e675b38e-8ee4-4adb-91ad-6d8160719581", @"Message that will be displayed to users if a password set was not successfully sent.
The default message is '{0}'", DefaultValueForPasswordSetEmailUnsuccessfulMessage);
			}
		}

		internal static MultilingualString HintForDefaultCargoWiseWebPortal
		{
			get
			{
				return ResString.GetMultilingualString("4ce4232c-4107-45fc-8f9f-b181e341bd1d", @"Default set/reset password URL that will be sent to users via the Password Set/Reset email. The default portal URL is the Neo Portal. If you would like to send users to the Global Portal Dashboard instead, please set this to GHC");
			}
		}

		#endregion

		internal static MultilingualString GetWebUrlHint(MultilingualString caption)
		{
			return ResString.GetMultilingualString("a9e63e33-a12a-40a4-beab-3c67deef4785", @"Specify the root URL for {0}.
Please keep this in sync with your Web Server settings for {0}.", caption);
		}

		internal static MultilingualString GetHintForNotificationGroupRegistryItems(MultilingualString caption)
		{
			return ResString.GetMultilingualString("eb8cb9f6-98de-429b-9bef-b4d605aea3fe", @"Send email notifications to this group when additions or modifications are made in the {0} tab.", caption);
		}

		internal static MultilingualString GetHintForDefaultFilterLayoutRegistryItems(MultilingualString caption)
		{
			return ResString.GetMultilingualString("C11462D3-BC71-4977-8511-DB7475851C23", @"Controls the default filter layout for the {0} tab in WebTracker. 

The list contains layouts that are published for all users of the fallback company.
The company that the web user logs in under is determined by the controlling branch on their organization record.
Selecting an option below will apply that layout as the default for users logged in this fallback company.", caption);
		}

		internal static MultilingualString GetHintForOrgsRolePropertySuppressionRegistryItems(MultilingualString caption)
		{
			return ResString.GetMultilingualString("60b7385b-8891-4a28-b73e-2c0c3aa875f5", @"Controls the visibility of properties for the {0} tab in WebTracker. 
You can show/hide properties against Web User Organization's roles by checking/unchecking appropriate tick-boxes.", caption);
		}

		internal static MultilingualString GetHintForNotificationOptions(MultilingualString caption)
		{
			return ResString.GetMultilingualString("ccf836ca-08ca-4501-b6a2-84a9947cb3f6", @"Additional sending options for notifications for {0} additions or modifications made via WebTracker.", caption);
		}

		internal static MultilingualString GetHintForUseModuleRegistryItems(MultilingualString caption)
		{
			return ResString.GetMultilingualString("99af1e7d-1e08-4be4-b05b-901fd49b1797", @"Controls the display of the {0} tab in WebTracker.

The default is 'Yes' - The {0} tab will be shown in WebTracker.

If overridden to 'No' - The {0} tab will not be shown in WebTracker and will be disabled for all users.", caption);
		}

		internal static MultilingualString GetHintForUseModuleRegistryItemsForWarehouse(MultilingualString caption)
		{
			var warehouseSepecificString = ResString.GetMultilingualString("cd743721-adb6-4f04-9c4f-7b01ad29556f", @"Please note that the Warehouse tab is only shown if the login organization is a warehouse client. This can be set by selecting ‘Warehouse’ in the Organization Type setting.");

			return MultilingualString.Join("\r\n\r\n", GetHintForUseModuleRegistryItems(caption), warehouseSepecificString);
		}

		internal static MultilingualString HintForOldTheme
		{
			get
			{
				return ResString.GetMultilingualString("ec26bf49-7f9c-4756-8418-7d154669c6cb", @"The theme (look and feel) to use in WebTracker.
This registry item controls which style sheet and images to use for WebTracker.
This will affect the look and feel of your website and should not be changed once you are happy with the font, colors and logos.
Do not use this option if you have already customized the look and feel of WebTracker.");
			}
		}

		internal static MultilingualString HintForWebCFSTheme
		{
			get
			{
				return ResString.GetMultilingualString("4E28A52E-AA45-4C29-B994-B680895366D3",
@"This registry item controls which style sheet and images to use for WebCFS.

The 'Theme' setting can be unique for each WebCFS URL (which need to be listed in the 'URLs' registry item), or for all URLs.

It may take a few minutes for the changes made here to be visible on WebCFS.");
			}
		}

		internal static MultilingualString HintForWebCFSCustomCss
		{
			get
			{
				return ResString.GetMultilingualString(
					"7FC69C41-E3F5-4FF0-A0F1-43D2E5620918",
@"This registry item defines the style sheet to be used for WebCFS.

The 'Theme CSS' setting can be unique for each WebCFS URL (which need to be listed in the 'URLs' registry item), or for all URLs.

It may take a few minutes for the changes made here to be visible on WebCFS.

The style sheet should include the following directive at the top of the file:
{0}",
					@"@import url(""DefaultStyle.css"");");
			}
		}

		internal static MultilingualString HintForWebCFSCustomImages
		{
			get
			{
				return ResString.GetMultilingualString(
					"F3F0D298-6744-440E-853B-D2998D2BE20F",
@"This registry item defines the images to be used for WebCFS. These images are only used if they are referenced from the style sheet under the 'Theme CSS' registry item.

The 'Theme Images' setting can be unique for each WebCFS URL (which need to be listed in the 'URLs' registry item), or for all URLs.

It may take a few minutes for the changes made here to be visible on WebCFS.

To reference theme images from the style sheet, use a relative path. For example, to reference an image named background.png:
{0}",
					@"background-image: url(""Images/background.png"");");
			}
		}

		internal static MultilingualString HintForWebCFSUrls
		{
			get
			{
				return ResString.GetMultilingualString(
					"475336F5-51F2-432A-BF38-ED6FAC8D1365",
@"This registry item defines URLs for WebCFS. Each URL is associated with a 'Theme', and with 'Theme CSS' and 'Theme Images', under the respective registry items. This makes it possible for multiple WebCFS sites at different URLs to have different appearances.

It is important to note that editing or deleting a URL here won't delete CSS or images defined in the respective registry items - they need to be removed manually (by deleting all text or all images).

Each URL should match the host component of the URL that would appear in the header of a HTTP request to the website. For example:
{0}", "wtgsyd.webtracker.wisegrid.net");
			}
		}

		internal static MultilingualString HintForWebTrackerTheme
		{
			get
			{
				return ResString.GetMultilingualString("153ee697-5cc4-44f0-9abc-5f30e56aac28",
@"This registry item controls which style sheet and images to use for WebTracker.

The 'Theme' setting can be unique for each WebTracker URL (which need to be listed in the 'URLs' registry item), or for all URLs.

It may take a few minutes for the changes made here to be visible on WebTracker.");
			}
		}

		internal static MultilingualString HintForWebTrackerCustomCss
		{
			get
			{
				return ResString.GetMultilingualString(
					"1000f6f5-1217-4155-85a1-d0b6488fc09b",
@"This registry item defines the style sheet to be used for WebTracker.

The 'Theme CSS' setting can be unique for each WebTracker URL (which need to be listed in the 'URLs' registry item), or for all URLs.

It may take a few minutes for the changes made here to be visible on WebTracker.

The style sheet should include the following directive at the top of the file:
{0}",
					@"@import url(""DefaultStyle.css"");");
			}
		}

		internal static MultilingualString HintForWebTrackerCustomImages
		{
			get
			{
				return ResString.GetMultilingualString(
					"c6394895-8abc-4b7b-8ee9-2825d5bc416d",
@"This registry item defines the images to be used for WebTracker. These images are only used if they are referenced from the style sheet under the 'Theme CSS' registry item.

The 'Theme Images' setting can be unique for each WebTracker URL (which need to be listed in the 'URLs' registry item), or for all URLs.

It may take a few minutes for the changes made here to be visible on WebTracker.

To reference theme images from the style sheet, use a relative path. For example, to reference an image named background.png:
{0}",
					@"background-image: url(""Images/background.png"");");
			}
		}

		internal static MultilingualString HintForWebTrackerUrls
		{
			get
			{
				return ResString.GetMultilingualString(
					"16b8cf95-ac12-4a30-9a14-40aa41ad9f59",
@"This registry item defines URLs for WebTracker. Each URL is associated with a 'Theme', and with 'Theme CSS' and 'Theme Images', under the respective registry items. This makes it possible for multiple WebTracker sites at different URLs to have different appearances.

It is important to note that editing or deleting a URL here won't delete CSS or images defined in the respective registry items - they need to be removed manually (by deleting all text or all images).

Each URL should match the host component of the URL that would appear in the header of a HTTP request to the website. For example:
{0}", "wtgsyd.webtracker.wisegrid.net");
			}
		}

		internal static MultilingualString HintForWebCampaignTheme
		{
			get
			{
				return ResString.GetMultilingualString("5dc07f00-0990-4ff6-b61d-c5cdaea44347",
@"This registry item controls which style sheet and images to use for WebCampaign.

The 'Theme' setting can be unique for each WebCampaign URL (which need to be listed in the 'Theme URL' registry item), or for all URLs.

It may take a few minutes for the changes made here to be visible on WebCampaign.");
			}
		}

		internal static MultilingualString HintForWebCampaignUrls
		{
			get
			{
				return ResString.GetMultilingualString(
					"22a5a17d-344b-468f-b5b9-9d381cd796fb",
@"This registry item defines URL and company for WebCampaign. Each URL and company combination is associated with a 'Theme', under the respective registry items. This makes it possible for multiple WebCampaign sites at different URLs, or one single WebCampaign site for campaigns created in different companies, to have different appearances.

It is important to note that editing or deleting a URL-Company combination here won't delete CSS or images defined in the respective registry items - they need to be removed manually (by deleting all text or all images).

Each URL should match the WebCampaign URL that would appear in the header of a HTTP request to the website. For example:
{0}", "http://wtgsyd.webtracker.wisegrid.net/campaign/");
			}
		}

		internal static MultilingualString HintForDateFormat
		{
			get
			{
				return ResString.GetMultilingualString("9ab616d5-8591-427f-9b56-58e2d4e743dc", @"This registry item controls how date values are formatted and displayed in WebTracker.");
			}
		}

		internal static MultilingualString HintForCustomLoginPageURL
		{
			get
			{
				return ResString.GetMultilingualString("07dbec2e-38df-4da3-8366-bf6e5796ec23", @"Use this setting to specify the URL of your Custom Login Page for WebTracker.
This page will be used instead of the standard Login page and should implement special functionality to pass the login details to your Web Tracking site.
This page should also be able to receive and show possible login errors.");
			}
		}

		internal static MultilingualString GintForCustomQuickViewPageURL
		{
			get
			{
				return ResString.GetMultilingualString("e3e95970-34d4-43c5-810d-39a8447377cf", @"Use this setting to specify the URL of your Custom Quick View page for WebTracker.
This page should implement special functionality to pass the Quick View details to WebTracker.
This page should also be able to receive and show possible errors as a result of trying to Quick View.

NOTE: You do not need to specify anything in this setting if your Custom Login Page also handles the Quick View functionality.
You only need to override this setting if you have a separate page for the Custom Quick View functionality.");
			}
		}

		internal static MultilingualString HintForWebTrackerSharedSecret
		{
			get
			{
				return ResString.GetMultilingualString("1f7e5b34-a377-4c32-9862-08bc47e8403e", @"Specify the 'Shared Secret' that will be used for communication between WebTracker and your own/external web site.
This is used by the 'Client Home Page' that can be set up for each Organization on the 'Web Security' Tab.");
			}
		}

		internal static MultilingualString HintForPageSize
		{
			get { return ResString.GetMultilingualString("91974fbc-8af7-43ca-8f7e-1ae450e841fd", @"The number of records to show per page on a search screen. You can also define the maximum number of records to show on a search screen."); }
		}

		internal static MultilingualString HintForMaxFilteredRecords
		{
			get
			{
				return ResString.GetMultilingualString("a34d4788-041e-42b5-bde8-fae354aaa4ba", @"The maximum number of records to display on a search screen.
Only this number will be shown even if a search returns more than this number.
This controls the total number of results - you can also define the number of results per page.");
			}
		}

		internal static MultilingualString HintForMaxFilteredRecordsForExportToExcel
		{
			get
			{
				return ResString.GetMultilingualString("efb8c46e-3308-4d1f-a30e-6185eb66626c", @"The maximum number of records to Export to Excel.
Only this number will be exported even if a search returns more than this number.");
			}
		}

		internal static MultilingualString HintForRequestTimeout
		{
			get { return ResString.GetMultilingualString("d738bff7-dd8b-458c-bc0d-effcd76470c9", @"The maximum acceptable time (in seconds) for a page to load (or a report to run) before the request times out. The recommended range is between 90 and 300 seconds (1½ to 5 minutes)."); }
		}

		internal static MultilingualString GetHintForStaffRoles(MultilingualString caption)
		{
			return ResString.GetMultilingualString("3ddc2765-92f9-463e-863f-f3bff3884b58", @"The staff roles that will be sent email notifications for {0} additions or modifications made via WebTracker.", caption);
		}

		internal static MultilingualString HintForAllowToAddNewOrganisation
		{
			get
			{
				return ResString.GetMultilingualString("a8725865-c6ac-4af8-8e13-6f8fe138ecd0", @"Controls the display of the 'Save' option when adding an unknown shipper / consignee in WebTracker.

The default is 'Yes' - The 'Save' option is shown in WebTracker.
You can use web security settings to control access to specific organizations and/or users.

If overridden to 'No' - The 'Save' option will not be shown in WebTracker and will be disabled for all users.");
			}
		}

		internal static MultilingualString HintForGetQuotesBasedOnPostalCodes
		{
			get { return ResString.GetMultilingualString("770f60f1-dfcc-442b-a031-a1608f8502b4", @"The default is 'No'. If overridden to 'Yes' you can get quotes based on postal/zip codes in WebTracker."); }
		}

		internal static MultilingualString HintForSaveQuotesWithoutRates
		{
			get { return ResString.GetMultilingualString("361021e8-aceb-4bbc-8857-ffe396f18904", "Set this to 'Yes' to allow saving quotes without rates in WebTracker."); }
		}

		internal static MultilingualString HintForDisableQuoteInsuranceValue
		{
			get { return ResString.GetMultilingualString("53e2e441-b9a9-490d-b737-3a3d737f3ca9", "Change to 'Yes' this Registry item value to hide Insurance Value on the Quote page."); }
		}

		internal static MultilingualString HintForBookingDefaultHouseBillType
		{
			get { return ResString.GetMultilingualString("1e636757-34da-4e62-97a4-89867e2965cc", "Default House Bill of Lading Type for new Forwarding Bookings created in WebTracker."); }
		}

		internal static MultilingualString HintForISFMilestoneUpdateSettings
		{
			get { return ResString.GetMultilingualString("a913a64f-7c11-4dfe-a28d-e4ba557ec934", "Specify event codes that can be updated by parties to an Importer Security Filing."); }
		}

		internal static MultilingualString HintForWarehouseOrderMilestoneUpdateSettings
		{
			get { return ResString.GetMultilingualString("31af062f-fec8-440c-9f06-62f2583db69a", "Specify event codes that can be updated by parties to a Warehouse Order."); }
		}

		internal static MultilingualString HintForWarehouseReceiveMilestoneUpdateSettings
		{
			get { return ResString.GetMultilingualString("e1ab09d0-ff60-4925-88cf-da256a723b0c", "Specify event codes that can be updated by parties to a Warehouse Receipt."); }
		}

		internal static MultilingualString HintForLocalTransportMilestoneUpdateSettings
		{
			get { return ResString.GetMultilingualString("d27493d9-a298-45a9-a3ec-c321f3b477f0", "Specify event codes that can be updated by parties to a Port Transport Job."); }
		}

		internal static MultilingualString HintForLinerAndAgencyBookingMilestoneUpdateSettings
		{
			get { return ResString.GetMultilingualString("50ac8abe-831e-4d91-8a4e-eadb65f8665f", "Specify event codes that can be updated by parties to a Liner & Agency Booking."); }
		}

		internal static MultilingualString HintForLinerAndAgencyBillOfLadingMilestoneUpdateSettings
		{
			get { return ResString.GetMultilingualString("daa9432a-d618-4351-ac8e-50d7905518d2", "Specify event codes that can be updated by parties to a Liner & Agency Bill of Lading."); }
		}

		internal static MultilingualString HintForDeclarationMilestoneUpdateSettings
		{
			get { return ResString.GetMultilingualString("68e404d3-cd4c-4c21-9034-8bb496a1cf49", "Specify event codes that can be updated by parties to a Customs Declaration."); }
		}

		internal static MultilingualString HintForOrderMilestoneUpdateSettings
		{
			get { return ResString.GetMultilingualString("2f205e6e-c72e-4377-afa8-f7f7812b3add", "Specify event codes that can be updated by parties to an Order."); }
		}

		internal static MultilingualString HintForContainerMilestoneUpdateSettings
		{
			get { return ResString.GetMultilingualString("6bcea9ea-a1ba-479d-b0a5-6d6ec558b596", "Specify event codes that can be updated by parties to a Container Movement."); }
		}

		internal static MultilingualString HintForBookingMilestoneUpdateSettings
		{
			get { return ResString.GetMultilingualString("b8dd7ade-0da3-467f-bdad-f34e5e7fe22a", "Specify event codes that can be updated by parties to a Booking."); }
		}

		internal static MultilingualString HintForShipmentMilestoneUpdateSettings
		{
			get { return ResString.GetMultilingualString("47c378f4-292b-47d9-98d7-925a292044e2", "Specify event codes that can be updated by parties to a Shipment."); }
		}

		#endregion

		#endregion

		#region Organisation

		public BooleanRegistryItem AllowToAddNewOrganisation
		{
			get
			{
				return Get("AllowToAddNewOrganisation", name => new BooleanRegistryItem(name,
					WebCategory,
					ResString.GetMultilingualString("795c3a0f-66ca-476f-9e5e-3a041ce6886c", "Allow creation of new organizations"),
					HintForAllowToAddNewOrganisation,
					RegistryStorageFlags.System,
					true));
			}
		}

		#endregion

		#region Language

		public CodeSelectionCollectionRegistryItem AllowedLanguages
		{
			get
			{
				return Get("AllowedWebLanguages", delegate
				{
					var allLanguages = new CodeDescriptionPairList(OLookUpEditType.Language);
					var webTrackerSupportedLanguages = new CodeDescriptionPairList();
					var codesProvider = new CodeDescriptionPairListProvider(() => webTrackerSupportedLanguages);
					var defaultValue = new CodeSelectionCollection(codesProvider);
					foreach (CodeDescriptionPair language in allLanguages)
					{
						webTrackerSupportedLanguages.Add(language);
						defaultValue.AddNew().Code = language.Code;
					}

					return new CodeSelectionCollectionRegistryItem("AllowedWebLanguages",
						WebCategory,
						ResString.GetMultilingualString("c7e656f1-608d-4f5a-8048-3cea228f078d", "Web Site Languages"),
						ResString.GetMultilingualString("b095d411-ae27-4692-b030-fb70d8097621", "The language of WebTracker interface will be automatically selected from these languages based on the browser settings of the site visitor."),
						RegistryStorageFlags.System,
						codesProvider,
						RegistryOptions.Default,
						defaultValue);
				});
			}
		}

		#endregion

		#region Email Password Template

		public NotificationEmailTemplateRegistryItem EmailPasswordTemplate
		{
			get
			{
				return Get<NotificationEmailTemplateRegistryItem>("EmailPasswordTemplate", delegate
				{
					var registryItem = new NotificationEmailTemplateRegistryItem("EmailPasswordTemplate",
						WebCategory,
						ResString.GetMultilingualString("90D8DF08-8F3C-4AA1-B042-4E0549934880", "Email Password Template"),
						ResString.GetMultilingualString("B6D39311-A8A0-499A-9C81-9E06498FD775", "Template that will be used in Web Access Password Emails."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						ObjectFactory.GetType<Enterprise.Integration.DocumentWrappers.IDocContactPasswordEmail>(),
						Core.Constants.ProductName + (NoResString)" Web Access Password",
						EmailPasswordTemplateDefaultBody);
					return registryItem;
				});
			}
		}

		public ZString EmailPasswordTemplateDefaultBody
		{
			get
			{
				return (NoResString)@"
<br /><br />(*ContactSalutation*),<br /><br />

Your access details to the web site client area at (*WebTrackerURL*) are:<br /><br />

Company Code: (*CompanyCode*)<br />
User Name:   (*UserName*)<br />
Password:   (*Password*)<br /><br />

(*PasswordChangeInstructions*)<br /><br />

Regards,<br /><br />
(*CurrentCompanyName*) Team<br /><br />";
			}
		}

		#endregion

		#region Accounts

		public BooleanRegistryItem UseWebAccountsModule
		{
			get { return GetUseModuleRegistryItem(ResString.GetMultilingualString("4398471c-e08e-4ad1-9e91-55d3cee8d3cd", "Accounts"), AccountsCategory, "UseWebAccountsModule", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		#endregion

		#region Customs

		#region Web Declaration

		public CodePairRegistryItem WebDeclarationNotificationOptions
		{
			get { return GetNotificationOptionsRegistryItem(ResString.GetMultilingualString("4154395e-b5e4-42e5-83d8-306b613ac7c1", "Declaration"), CustomsDeclarationCategory, "WebDeclarationNotificationOptions", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		public GuidRegistryItem WebDeclarationNotificationEmailGroup
		{
			get { return GetNotificationGroupRegistryItem(ResString.GetMultilingualString("4154395e-b5e4-42e5-83d8-306b613ac7c1", "Declaration"), CustomsDeclarationCategory, "WebDeclarationNotificationEmailGroup", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue); }
		}

		public CodeDescriptionBoolRegistryItem WebDeclarationNotificationStaffRoles
		{
			get { return GetStaffRolesRegistryItem(ResString.GetMultilingualString("4154395e-b5e4-42e5-83d8-306b613ac7c1", "Declaration"), CustomsDeclarationCategory, "WebDeclarationNotificationStaffRoles", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, StaffAssignmentRoles.Codes.SalesRep); }
		}

		public BooleanRegistryItem UseWebDeclarationModule
		{
			get { return GetUseModuleRegistryItem(ResString.GetMultilingualString("de96cc9c-ab19-4d2e-b8fb-402007ed0841", "Declaration"), CustomsDeclarationCategory, WebModuleRegistry.Declarations, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		public FilterLayoutCodePairRegistryItem DefaultFilterLayoutDeclaration
		{
			get { return GetDefaultFilterLayoutRegistryItem(ResString.GetMultilingualString("4154395e-b5e4-42e5-83d8-306b613ac7c1", "Declaration"), CustomsDeclarationCategory, "DefaultFilterLayoutDeclaration", "TrackingDeclarations", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.NotCached); }
		}

		#endregion

		#region Web ISF

		public BooleanRegistryItem UseWebISFModule
		{
			get { return GetUseModuleRegistryItem(ResString.GetMultilingualString("1f1a8b53-644b-42ca-9bf9-dd9a1381a8cf", "Importer Security Filing"), CustomsISFCategory, WebModuleRegistry.ISF, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		public FilterLayoutCodePairRegistryItem DefaultFilterLayoutISF
		{
			get { return GetDefaultFilterLayoutRegistryItem(ResString.GetMultilingualString("279daecc-1235-4eac-88e1-b5ce97911b29", "Importer Security Filing"), CustomsISFCategory, "DefaultFilterLayoutISF", "TrackingImporterSecurityFiling", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.NotCached); }
		}

		public CodePairRegistryItem ISFNotificationOptions
		{
			get { return GetNotificationOptionsRegistryItem(ResString.GetMultilingualString("d230f150-f0b5-4ee1-8718-7d8b3405c288", "Importer Security Filing"), CustomsISFCategory, "ISFNotificationOptions", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		public GuidRegistryItem ISFNotificationEmailGroup
		{
			get { return GetNotificationGroupRegistryItem(ResString.GetMultilingualString("16a91aa3-80b6-4c2a-a62b-b759c6232377", "Importer Security Filing"), CustomsISFCategory, "ISFNotificationEmailGroup", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue); }
		}

		public CodeDescriptionBoolRegistryItem ISFNotificationStaffRoles
		{
			get
			{
				return GetStaffRolesRegistryItem(ResString.GetMultilingualString("2373210b-9ea4-4d72-9cb8-6d9d7febb99c", "Importer Security Filing"), CustomsISFCategory, "ISFNotificationStaffRoles", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
					new[] { StaffAssignmentRoles.Codes.CustomsAgent, StaffAssignmentRoles.Codes.CustomerServiceRep });
			}
		}

		public AccessControlRegistryItem ISFOrgsRolePropertySuppression
		{
			get { return GetOrgsRolePropertySuppressionRegistryItem(ResString.GetMultilingualString("d9e2fdba-4427-4208-987e-cb81b1b29821", "Importer Security Filing"), CustomsISFCategory, "ISFOrgsRolePropertySuppression", new ISFAccessRules(), DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		#endregion Web ISF

		#region Web Schedule D Port Codes

		public FilterLayoutCodePairRegistryItem DefaultFilterLayoutUSCRegionDistrictPort
		{
			get { return GetDefaultFilterLayoutRegistryItem(ResString.GetMultilingualString("f5bf464f-20f5-46a7-989a-24ec705c8604", "Schedule D Port Codes"), CustomsUSCRegionDistrictPortCategory, "DefaultFilterLayoutUSCRegionDistrictPort", "TrackingUSCRegionDistrictPort", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.NotCached); }
		}

		#endregion

		#region Web Schedule K Port Codes

		public FilterLayoutCodePairRegistryItem DefaultFilterLayoutUSCForeignPort
		{
			get { return GetDefaultFilterLayoutRegistryItem(ResString.GetMultilingualString("2fd5d084-ef2e-4d61-b40a-62b30add3bbb", "Schedule K Port Codes"), CustomsUSCForeignPortCategory, "DefaultFilterLayoutUSCForeignPort", "TrackingUSCForeignPort", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.NotCached); }
		}

		#endregion

		#endregion

		#region Transport

		#region TransportJobs

		public BooleanRegistryItem TransportShowAllLegs
		{
			get
			{
				return Get("TransportShowAllLegs", name => new BooleanRegistryItem(name,
							TransportJobsCategory,
							ResString.GetMultilingualString("3b19b701-864e-4abc-9946-5c4b82af1cae", "Show All Transport Legs"),
							ResString.GetMultilingualString("716925c8-6d41-4dce-90d9-f794d9d1c033", "This Registry item controls which Transport Legs are shown on the Transport Job details page.  By default, all legs are displayed.  If this is set to 'NO', only legs where one of the parties is the logged in organization are shown."),
							RegistryStorageFlags.System,
							DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
							true));
			}
		}

		public BooleanRegistryItem UseWebCartageModule
		{
			get { return GetUseModuleRegistryItem(ResString.GetMultilingualString("e0040b24-9540-454f-87fc-e8236c32167b", "Transport Jobs"), TransportJobsCategory, "UseWebCartageModule", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		public FilterLayoutCodePairRegistryItem DefaultFilterLayoutTrackingCartage
		{
			get { return GetDefaultFilterLayoutRegistryItem(ResString.GetMultilingualString("e0040b24-9540-454f-87fc-e8236c32167b", "Transport Jobs"), TransportJobsCategory, "DefaultFilterLayoutTrackingCartage", "TrackingCartage", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.NotCached); }
		}

		public CodePairRegistryItem TrackingCartageNotificationOptions
		{
			get { return GetNotificationOptionsRegistryItem(ResString.GetMultilingualString("e0040b24-9540-454f-87fc-e8236c32167b", "Transport Jobs"), TransportJobsCategory, "TrackingCartageNotificationOptions", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		public GuidRegistryItem TrackingCartageNotificationEmailGroup
		{
			get { return GetNotificationGroupRegistryItem(ResString.GetMultilingualString("e0040b24-9540-454f-87fc-e8236c32167b", "Transport Jobs"), TransportJobsCategory, "TrackingCartageNotificationEmailGroup", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue); }
		}

		public CodeDescriptionBoolRegistryItem TrackingCartageNotificationStaffRoles
		{
			get
			{
				return GetStaffRolesRegistryItem(ResString.GetMultilingualString("e0040b24-9540-454f-87fc-e8236c32167b", "Transport Jobs"), TransportJobsCategory, "TrackingCartageNotificationStaffRoles", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
																				 new[] { StaffAssignmentRoles.Codes.CartageCoordinator, StaffAssignmentRoles.Codes.CustomerServiceRep });
			}
		}

		#endregion

		#endregion

		#region Forwarding

		#region Service Levels Visibility

		public ServiceLevelVisibilityRegistryItem ServiceLevelVisibility
		{
			get
			{
				return Get("ServiceLevelVisibility", name => new ServiceLevelVisibilityRegistryItem(name,
																									ForwardingCategory,
																									ResString.GetMultilingualString("7446fb66-bd41-458a-b186-3ae7f83558ec", "Service Levels Visibility"),
																									HintForServiceLevelVisibility,
																									DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default));
			}
		}

		#endregion

		#region Suppress Flight Details

		public static MultilingualString SuppressFlightDetailsCategory { get { return RegistryConstants.GetCategory(ForwardingCategory, ResString.GetMultilingualString("5a5787ee-b6c6-4530-85cf-7d58bf3b3771", "Flight Details Suppression")); } }

		public CodeDescriptionBoolRegistryItem SuppressFlightDetailsForExport
		{
			get
			{
				return GetSuppressFlightDetailsRegistryItem(ResString.GetMultilingualString("7ad0cfc0-7464-4575-8d28-57a96e0b2ba4", "Suppress Export Flight Details"),
																										"SuppressFlightDetailsForExport",
															ResString.GetMultilingualString("35a90464-d87b-4012-bdc0-7669c226d01c", "export"), DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
			}
		}

		public CodeDescriptionBoolRegistryItem SuppressFlightDetailsForImport
		{
			get
			{
				return GetSuppressFlightDetailsRegistryItem(ResString.GetMultilingualString("e3cd1200-1c25-4ca1-ae70-3867768e8e14", "Suppress Import Flight Details"),
																										"SuppressFlightDetailsForImport",
															ResString.GetMultilingualString("15ca60a4-08c2-40fa-bd33-aa7ab0249689", "import"),
															DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
			}
		}

		public CodeDescriptionBoolRegistryItem SuppressFlightDetailsForDomestic
		{
			get
			{
				return GetSuppressFlightDetailsRegistryItem(ResString.GetMultilingualString("6c1ecb14-f020-4c53-ae5c-d6c17ccd49fc", "Suppress Domestic Flight Details"),
															"SuppressFlightDetailsForDomestic",
															ResString.GetMultilingualString("fc73b64d-c01c-4bfb-b4b1-1dc7e69e4fd2", "domestic"),
															DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
			}
		}

		public CodeDescriptionBoolRegistryItem SuppressFlightDetailsForForeign
		{
			get
			{
				return GetSuppressFlightDetailsRegistryItem(ResString.GetMultilingualString("1ed9c6c1-b235-4a0e-bb93-a9b0f6e7486d", "Suppress Foreign Flight Details"),
															"SuppressFlightDetailsForForeign",
															ResString.GetMultilingualString("94f2ca03-01e7-4869-b62b-bb9f9d7e97c3", "foreign"),
															DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
			}
		}

		#endregion

		#region Bookings

		public BooleanRegistryItem UseWebForwardingBookingsModule
		{
			get { return GetUseModuleRegistryItem(ResString.GetMultilingualString("77d346d5-7bcb-4a61-aa6e-c86e468426d0", "Forwarding Bookings"), ForwardingBookingsCategory, WebModuleRegistry.ForwardingBookings, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		public FilterLayoutCodePairRegistryItem DefaultFilterLayoutForwardingBookings
		{
			get { return GetDefaultFilterLayoutRegistryItem(ResString.GetMultilingualString("1d5e1f7c-9ab2-4879-88b8-559397d9b8e6", "Forwarding Bookings"), ForwardingBookingsCategory, "DefaultFilterLayoutForwardingBookings", "TrackingBookings", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.NotCached); }
		}

		public CodePairRegistryItem BookingDefaultHouseBillType
		{
			get
			{
				var lookUpList = FreightDataRegistry.Instance.HouseBillOfLadingTypesForSea.Value.GetCodeDescriptionPairList();
				var defaultValue = lookUpList.Count > 0 ? lookUpList[0].Code : string.Empty;

				return Get("BookingDefaultHouseBillType", name => new CodePairRegistryItem(name,
							ForwardingBookingsCategory,
							ResString.GetMultilingualString("9726ad84-4119-49c9-909e-76f35bf64210", "Default House Bill Type"),
							HintForBookingDefaultHouseBillType,
							new CodeDescriptionPairListProvider(() => lookUpList),
							RegistryStorageFlags.System,
							DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
							defaultValue));
			}
		}

		public CodePairRegistryItem BookingNotificationOptions
		{
			get { return GetNotificationOptionsRegistryItem(ResString.GetMultilingualString("66d9195e-740e-46aa-a1ce-a6b02c146458", "Forwarding Bookings"), ForwardingBookingsCategory, "BookingNotificationOptions", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		public GuidRegistryItem BookingNotificationEmailGroup
		{
			get { return GetNotificationGroupRegistryItem(ResString.GetMultilingualString("27f695e5-aff3-45fa-b3f1-6f214173b5c6", "Forwarding Bookings"), ForwardingBookingsCategory, "BookingNotificationEmailGroup", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue); }
		}

		public CodeDescriptionBoolRegistryItem BookingsNotificationStaffRoles
		{
			get { return GetStaffRolesRegistryItem(ResString.GetMultilingualString("132df4a7-053a-4f77-89c7-3bcbee51f3a3", "Forwarding Bookings"), ForwardingBookingsCategory, "BookingsNotificationStaffRoles", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, StaffAssignmentRoles.Codes.SalesRep); }
		}

		public DecimalRegistryItem BookingPackLinesPageSize
		{
			get
			{
				return Get("BookingPackLinesPageSize", name => new DecimalRegistryItem(name,
					ForwardingBookingsCategory,
					ResString.GetMultilingualString("f75ca96b-3a70-4b0d-ae42-9526d97af354", "Goods/Packs Page Size"),
					HintForBookingPackLinesPageSize,
					new NumericRegistryEditorInfo(decimalPlaces: 0),
					RegistryStorageFlags.System,
					DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
					defaultValue: 0m,
					lowerBound: 0,
					upperBound: 99));
			}
		}

		public MultilingualStringRegistryItem BookingTermsAndConditions => Get("BookingTermsAndConditions",
			name => new MultilingualStringRegistryItem(name,
				ForwardingBookingsCategory,
				ResString.GetMultilingualString("eff98cac-d977-45dd-8f4b-16f5a51c97e4", "Terms and Conditions for Bookings"),
				HintForBookingTermsAndConditions,
				RegistryStorageFlags.System,
				DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default)
			{
				EditorInfo = new TextRegistryEditorInfo(TextEditorType.HTML)
			});

		public BooleanRegistryItem PrintAddressesOnFreightLabels
		{
			get
			{
				return Get("PrintAddressesOnFreightLabels", name => new BooleanRegistryItem(name,
																							ForwardingBookingsCategory,
																							ResString.GetMultilingualString("e61a5910-2726-4a76-a719-cb806e728288", "Print Addresses on Freight Labels"),
																							HintForPrintAddressesOnFreightLabels,
																							RegistryStorageFlags.System,
																							DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
																							true));
			}
		}

		public BooleanRegistryItem DisableBookingInsuranceValue
		{
			get
			{
				return Get("DisableBookingInsuranceValue", name => new BooleanRegistryItem(name,
																							 ForwardingBookingsCategory,
																							 ResString.GetMultilingualString("d9190419-e73f-42d6-bb85-523c062af34f", "Hide Booking Insurance Value"),
																							 HintForDisableBookingInsuranceValue,
																							 RegistryStorageFlags.System,
																							 DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
																							 false));
			}
		}

		public BooleanRegistryItem DisableBookingContainersGrid
		{
			get
			{
				return Get("DisableBookingContainersGrid", name => new BooleanRegistryItem(name,
																							 ForwardingBookingsCategory,
																							 ResString.GetMultilingualString("80e9703b-845b-4d70-9861-a1a53baf1582", "Hide Booking Containers Grid"),
																							 HintForDisableBookingContainersGrid,
																							 RegistryStorageFlags.System,
																							 DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
																							 false));
			}
		}

		public BooleanRegistryItem DisableBookingGoodsPacksGrid
		{
			get
			{
				return Get("DisableBookingGoodsPacksGrid", name => new BooleanRegistryItem(name,
																							 ForwardingBookingsCategory,
																							 ResString.GetMultilingualString("f227712c-94f0-483f-be7e-d611f6f0f083", "Hide Booking Goods / Packs Grid"),
																							 HintForDisableBookingGoodsPacksGrid,
																							 RegistryStorageFlags.System,
																							 DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
																							 false));
			}
		}

		#region SuppressResourceStringsCheckRegion

		public BooleanRegistryItem BookingOrderGridsEnableExtraRowMode
		{
			get
			{
				return Get("BookingOrderGridsEnableExtraRowMode", name => new BooleanRegistryItem(name,
																							 ForwardingBookingsCategory,
																							 (NoResString)"Enable Extra Row Mode on Order / Order Line Grids",
																							 (NoResString)"Change to 'Yes' this Registry item value to always display an extra row for Attached Orders Grid and Product Order Lines Grid on the Booking page.",
																							 RegistryStorageFlags.System,
																							 DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForDevelopers,
																							 false));
			}
		}

		#endregion

		public BooleanRegistryItem BookingAttachOrdersWithoutSupplier
		{
			get
			{
				return Get("BookingAttachOrdersWithoutSupplier", name => new BooleanRegistryItem(name,
																							 ForwardingBookingsCategory,
																							 ResString.GetMultilingualString("f0418cfd-a4f7-4155-bfce-30d3352462d0", "Attach Orders Without Supplier"),
																							 HintBookingAttachOrdersWithoutSupplier,
																							 RegistryStorageFlags.System,
																							 DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
																							 false));
			}
		}

		#endregion

		#region Containers

		public BooleanRegistryItem UseWebForwardingContainersModule
		{
			get { return GetUseModuleRegistryItem(ResString.GetMultilingualString("16c4a6b6-6dd3-4458-ba5b-f6188ce29cc9", "Forwarding Containers"), ForwardingContainersCategory, WebModuleRegistry.ForwardingContainers, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		public FilterLayoutCodePairRegistryItem DefaultFilterLayoutForwardingContainers
		{
			get { return GetDefaultFilterLayoutRegistryItem(ResString.GetMultilingualString("3235fd5e-425a-4f57-84f4-678a386a1d93", "Forwarding Containers"), ForwardingContainersCategory, "DefaultFilterLayoutForwardingContainers", "TrackingContainers", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.NotCached); }
		}

		public CodePairRegistryItem ContainerNotificationOptions
		{
			get { return GetNotificationOptionsRegistryItem(ResString.GetMultilingualString("19c1603b-03a0-4b6f-b46b-129b82482bc9", "Forwarding Containers"), ForwardingContainersCategory, "ContainerNotificationOptions", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		public GuidRegistryItem ContainerNotificationEmailGroup
		{
			get { return GetNotificationGroupRegistryItem(ResString.GetMultilingualString("49b04a25-410a-49fa-9700-2739390632df", "Forwarding Containers"), ForwardingContainersCategory, "ContainerNotificationEmailGroup", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue); }
		}

		public CodeDescriptionBoolRegistryItem ContainerNotificationStaffRoles
		{
			get { return GetStaffRolesRegistryItem(ResString.GetMultilingualString("fcd3da81-0efb-4818-b3cb-323f96b88d6e", "Forwarding Containers"), ForwardingContainersCategory, "ContainerNotificationStaffRoles", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, StaffAssignmentRoles.Codes.CartageCoordinator); }
		}

		public BooleanRegistryItem ShowContainerStatusFromShipment
		{
			get
			{
				return Get("ShowContainerStatusFromShipment", name => new BooleanRegistryItem(name,
																								ForwardingContainersCategory,
																								ResString.GetMultilingualString("a726fb01-00c9-42ff-9672-5451c1cc5f5f", "Show Container Status from Shipments"),
																								HintForShowContainerStatusFromShipment,
																								RegistryStorageFlags.System,
																								DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
																								false));
			}
		}

		#endregion

		#region LinerAndAgencyContainers

		public BooleanRegistryItem UseWebLinerAndAgencyContainersModule
		{
			get { return GetUseModuleRegistryItem(ResString.GetMultilingualString("A31A64F9-E413-461B-92AF-CCCC9732DEDA", "Liner & Agency Containers"), LinerAndAgencyContainersCategory, WebModuleRegistry.LinerAndAgencyContainers, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		public FilterLayoutCodePairRegistryItem DefaultFilterLayoutLinerAndAgencyContainers
		{
			get { return GetDefaultFilterLayoutRegistryItem(ResString.GetMultilingualString("794D92A6-A5AD-46D4-B9AB-3DB6ACFAF345", "Liner & Agency Containers"), LinerAndAgencyContainersCategory, "DefaultFilterLayoutLinerAndAgencyContainers", "LinerAndAgencyContainers", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.NotCached); }
		}

		public CodePairRegistryItem LinerAndAgencyContainerNotificationOptions
		{
			get { return GetNotificationOptionsRegistryItem(ResString.GetMultilingualString("BD723714-FE20-471A-81E9-D4EC8E8A99E8", "Liner & Agency Containers"), LinerAndAgencyContainersCategory, "LinerAndAgencyContainerNotificationOptions", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		public GuidRegistryItem LinerAndAgencyContainerNotificationEmailGroup
		{
			get { return GetNotificationGroupRegistryItem(ResString.GetMultilingualString("B7E77A91-599D-483D-A589-EC8D129A97D3", "Liner & Agency Containers"), LinerAndAgencyContainersCategory, "LinerAndAgencyContainerNotificationEmailGroup", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue); }
		}

		public CodeDescriptionBoolRegistryItem LinerAndAgencyContainerNotificationStaffRoles
		{
			get { return GetStaffRolesRegistryItem(ResString.GetMultilingualString("143F1B66-FF26-49C0-9215-FCED06911703", "Liner & Agency Containers"), LinerAndAgencyContainersCategory, "LinerAndAgencyContainerNotificationStaffRoles", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, StaffAssignmentRoles.Codes.CartageCoordinator); }
		}

		#endregion

		#region Flights

		public BooleanRegistryItem UseWebForwardingFlightsModule
		{
			get { return GetUseModuleRegistryItem(ResString.GetMultilingualString("91cc1161-cbc0-4677-954a-7adb5bf2fdeb", "Forwarding Flight Schedules"), ForwardingFlightsCategory, WebModuleRegistry.FlightSchedules, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		public FilterLayoutCodePairRegistryItem DefaultFilterLayoutForwardingFlightSchedules
		{
			get { return GetDefaultFilterLayoutRegistryItem(ResString.GetMultilingualString("79776b59-76a5-4f0b-8718-6f6ae5a31341", "Forwarding Flight Schedules"), ForwardingFlightsCategory, "DefaultFilterLayoutForwardingFlightSchedules", "TrackingFlightSchedules", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.NotCached); }
		}

		#endregion

		#region Orders

		public BooleanRegistryItem UseWebForwardingOrdersModule
		{
			get { return GetUseModuleRegistryItem(ResString.GetMultilingualString("63b5dff9-e4d5-4fa9-a866-66859c1feb73", "Forwarding Orders"), ForwardingOrdersCategory, WebModuleRegistry.ForwardingOrders, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		public FilterLayoutCodePairRegistryItem DefaultFilterLayoutForwardingOrders
		{
			get { return GetDefaultFilterLayoutRegistryItem(ResString.GetMultilingualString("c83642e1-ff7e-445a-91fa-1d6c6877f1ad", "Forwarding Orders"), ForwardingOrdersCategory, "DefaultFilterLayoutForwardingOrders", "TrackingOrders", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.NotCached); }
		}

		public FilterLayoutCodePairRegistryItem DefaultFilterLayoutForwardingOrderLines
		{
			get { return GetDefaultFilterLayoutRegistryItem(ResString.GetMultilingualString("70d1efee-7a58-4270-974a-e56e642fc7e1", "Forwarding Order Lines"), ForwardingOrderLinesCategory, "DefaultFilterLayoutForwardingOrderLines", "TrackingOrderLines", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.NotCached); }
		}

		public CodePairRegistryItem OrderNotificationOptions
		{
			get { return GetNotificationOptionsRegistryItem(ResString.GetMultilingualString("50f6e307-eac7-443d-8b58-73fc16ad29df", "Forwarding Orders"), ForwardingOrdersCategory, "OrderNotificationOptions", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		public GuidRegistryItem OrderNotificationEmailGroup
		{
			get { return GetNotificationGroupRegistryItem(ResString.GetMultilingualString("3500aaa3-b6b6-4f50-9da5-6c6a7c984af8", "Forwarding Orders"), ForwardingOrdersCategory, "OrderNotificationEmailGroup", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue); }
		}

		public CodeDescriptionBoolRegistryItem OrderNotificationStaffRoles
		{
			get { return GetStaffRolesRegistryItem(ResString.GetMultilingualString("566f48ef-19b9-4a34-889c-83bba0fc17f6", "Forwarding Orders"), ForwardingOrdersCategory, "OrderNotificationStaffRoles", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, StaffAssignmentRoles.Codes.CartageCoordinator); }
		}

		#endregion

		#region Quotes

		public BooleanRegistryItem UseWebForwardingQuotesModule
		{
			get { return GetUseModuleRegistryItem(ResString.GetMultilingualString("839f824f-4d29-4a7d-b998-a48cbcd54fcb", "Forwarding Quotes"), ForwardingQuotesCategory, WebModuleRegistry.ForwardingQuotes, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		public FilterLayoutCodePairRegistryItem DefaultFilterLayoutForwardingQuotes
		{
			get { return GetDefaultFilterLayoutRegistryItem(ResString.GetMultilingualString("77af4e0b-30f0-4fe8-9c77-6c21af31460b", "Forwarding Quotes"), ForwardingQuotesCategory, "DefaultFilterLayoutForwardingQuotes", "TrackingQuotations", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.NotCached); }
		}

		public BooleanRegistryItem GetQuotesBasedOnPostalCodes
		{
			get
			{
				return Get("GetQuotesBasedOnPostalCodes", name => new BooleanRegistryItem(name,
						ForwardingQuotesCategory,
						ResString.GetMultilingualString("75a53fe4-ebb5-4d4b-9acd-f202d930258b", "Get quotes based on postal/zip codes"),
						HintForGetQuotesBasedOnPostalCodes,
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						false));
			}
		}

		public BooleanRegistryItem SaveQuotesWithoutRates
		{
			get
			{
				return Get("SaveQuotesWithoutRates", name => new BooleanRegistryItem(name,
																																				 ForwardingQuotesCategory,
																																				 ResString.GetMultilingualString("cb59a466-57e3-44a4-8070-11674acc5fdd", "Save Quotes without Rates"),
																																				 HintForSaveQuotesWithoutRates,
																																				 RegistryStorageFlags.System,
																																				 DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
																																				 false));
			}
		}

		public CodePairRegistryItem QuoteNotificationOptions
		{
			get { return GetNotificationOptionsRegistryItem(ResString.GetMultilingualString("260b516a-c99b-4101-a625-4af3e55fc92f", "Forwarding Quotes"), ForwardingQuotesCategory, "QuoteNotificationOptions", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		public GuidRegistryItem QuoteNotificationEmailGroup
		{
			get { return GetNotificationGroupRegistryItem(ResString.GetMultilingualString("1e40ad18-c557-432d-a945-c75b03325938", "Forwarding Quotes"), ForwardingQuotesCategory, "QuoteNotificationEmailGroup", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue); }
		}

		public CodeDescriptionBoolRegistryItem QuoteNotificationStaffRoles
		{
			get { return GetStaffRolesRegistryItem(ResString.GetMultilingualString("0842c7f1-ccd5-40d7-85c4-d9b9a6aa17b3", "Forwarding Quotes"), ForwardingQuotesCategory, "QuoteNotificationStaffRoles", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, StaffAssignmentRoles.Codes.SalesRep); }
		}

		public BooleanRegistryItem DisableQuoteInsuranceValue
		{
			get
			{
				return Get("DisableQuoteInsuranceValue", name => new BooleanRegistryItem(name,
																	ForwardingQuotesCategory,
																	ResString.GetMultilingualString("b7018e1d-0dd2-4b6f-a52e-77dfe5c871c8", "Hide Quote Insurance Value"),
																	HintForDisableQuoteInsuranceValue,
																	RegistryStorageFlags.System,
																	DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
																	false));
			}
		}

		#endregion

		#region Reports

		public BooleanRegistryItem UseWebForwardingReportsModule
		{
			get { return GetUseModuleRegistryItem(ResString.GetMultilingualString("f4a34ffe-7fd9-4e13-b9e6-0a43a291757b", "Forwarding Reports"), ForwardingReportsCategory, "UseWebForwardingReportsModule", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		public BooleanRegistryItem UseWebCustomsReportsModule
		{
			get { return GetUseModuleRegistryItem(ResString.GetMultilingualString("49537409-c102-4e64-9f02-1a3503c0ed51", "Customs Reports"), CustomsReportsCategory, "UseWebCustomsReportsModule", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		public BooleanRegistryItem UseWebWarehouseReportsModule
		{
			get { return GetUseModuleRegistryItem(ResString.GetMultilingualString("577B702E-EFA4-4A22-881E-570D09793A30", "Warehouse Reports"), WarehouseReportsCategory, "UseWebWarehouseReportsModule", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, true); }
		}

		public BooleanRegistryItem UseWebTransportReportsModule
		{
			get { return GetUseModuleRegistryItem(ResString.GetMultilingualString("7fdd04ed-f696-4851-adac-4116d64747ab", "Port Transport Reports"), TransportReportsCategory, "UseWebTransportReportsModule", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		public BooleanRegistryItem UseWebLinerAndAgencyReportsModule
		{
			get { return GetUseModuleRegistryItem(ResString.GetMultilingualString("E12CD794-B371-4F20-9910-8CFFFA3A2315", "Liner & Agency Reports"), LinerAndAgencyReportsCategory, "UseWebShippingReportsModule", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		#endregion

		#region Sailings

		public BooleanRegistryItem UseWebForwardingSailingsModule
		{
			get { return GetUseModuleRegistryItem(ResString.GetMultilingualString("8494fcee-fc7a-4f37-ab53-1eb62305bc41", "Forwarding Sailing Schedules"), ForwardingSailingsCategory, WebModuleRegistry.SailingSchedules, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		public FilterLayoutCodePairRegistryItem DefaultFilterLayoutForwardingSailingSchedules
		{
			get { return GetDefaultFilterLayoutRegistryItem(ResString.GetMultilingualString("8c9d948e-4c72-419d-a7fc-7104c18c9415", "Forwarding Sailing Schedules"), ForwardingSailingsCategory, "DefaultFilterLayoutForwardingSailingSchedules", "TrackingSailingSchedules", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.NotCached); }
		}

		#endregion

		#region Shipments

		public WebEDocsDownloadRegistryItem WebEDocsBulkDownload
		{
			get
			{
				return Get("WebEDocsBulkDownload", name => new WebEDocsDownloadRegistryItem(
									"WebEDocsBulkDownload",
									WebCategory,
									ResString.GetMultilingualString("f590f48d-9ed4-4a88-9eb6-9e057b710e2d", "eDocs Bulk Download"),
									ResString.GetMultilingualString("26b0a8f8-606a-4b31-9891-1be78daca0db", "eDocs with the following document types will be available to download directly from the search screen. When accessing a module in Web Tracker, if that module does not have configuration for eDocs Bulk Download, it will default to the configuration from 'All Modules'."),
									RegistryStorageFlags.System | RegistryStorageFlags.Company,
									DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default));
			}
		}

		public BooleanRegistryItem UseWebForwardingShipmentsModule
		{
			get { return GetUseModuleRegistryItem(ResString.GetMultilingualString("fed2f564-1918-4928-85a2-91347cfd7a38", "Forwarding Shipments"), ForwardingShipmentsCategory, WebModuleRegistry.ForwardingShipments, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		public FilterLayoutCodePairRegistryItem DefaultFilterLayoutForwardingShipments
		{
			get { return GetDefaultFilterLayoutRegistryItem(ResString.GetMultilingualString("ffcdfc16-ae48-4490-9616-4df91ebce8a6", "Forwarding Shipments"), ForwardingShipmentsCategory, "DefaultFilterLayoutForwardingShipments", "TrackingShipments", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.NotCached); }
		}

		public CodePairRegistryItem ShipmentNotificationOptions
		{
			get { return GetNotificationOptionsRegistryItem(ResString.GetMultilingualString("e8d3665c-98a0-4bf8-b435-fdda0eff4638", "Forwarding Shipments"), ForwardingShipmentsCategory, "ShipmentNotificationOptions", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		public GuidRegistryItem ShipmentNotificationEmailGroup
		{
			get { return GetNotificationGroupRegistryItem(ResString.GetMultilingualString("ba1b0c85-d3e3-4650-a570-748f3cbcc3e5", "Forwarding Shipments"), ForwardingShipmentsCategory, "ShipmentNotificationEmailGroup", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue); }
		}

		public CodeDescriptionBoolRegistryItem ShipmentNotificationStaffRoles
		{
			get
			{
				return GetStaffRolesRegistryItem(ResString.GetMultilingualString("e1f1d29b-8201-4009-95ae-dd1fb3f3915e", "Forwarding Shipments"),
												 ForwardingShipmentsCategory,
												 "ShipmentNotificationStaffRoles",
												 DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
												new[] { StaffAssignmentRoles.Codes.CartageCoordinator, StaffAssignmentRoles.Codes.CustomerServiceRep });
			}
		}

		public CodeDescriptionBoolRegistryItem ShipmentPageCustomisation
		{
			get
			{
				return Get("ShipmentPageCustomisation", name => new CodeDescriptionBoolDisallowNewRegistryItem(name,
																	 ForwardingShipmentsCategory,
																	 ResString.GetMultilingualString("3e0c4fd7-4aae-40a8-80ce-4b5a76abb874", "Customize Shipment Details page"),
																	 ResString.GetMultilingualString("2012ca3c-babf-4393-82b9-b7e574a85c43", "Use this Registry settings to hide some elements on the Shipment Details web page."),
																	 RegistryStorageFlags.System,
																	 DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
																	 new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("F01692E2-AFD1-4f16-96FF-5F0459E69C83", "Hide"), true, true),
																	 new CodeDescriptionBoolDisallowNewCollection
																	 {
																		{ ShipmentDetailsPageElements.EtdAndEta, ResString.GetMultilingualString("5a77e7bf-4bdb-4416-9552-0d83a157f0db", "ETD and ETA"), false },
																		{ ShipmentDetailsPageElements.StorageCommencesDate, ResString.GetMultilingualString("dfc4f482-0d5a-43b4-a065-9b1230593e33", "Storage Commences Date"), false },
																		{ ShipmentDetailsPageElements.CartageAdvisedDate, ResString.GetMultilingualString("cdf885ca-da68-44f4-b929-933c5a1ab4bd", "Port Transport Advised Date"), false },
																		{ ShipmentDetailsPageElements.TransportGrid, ResString.GetMultilingualString("da745c4f-b6bc-4a66-bfbf-dbf82f378613", "Transport Grid"), false },
																		{ ShipmentDetailsPageElements.GoodsPacksGrid, ResString.GetMultilingualString("f4913d85-da7c-4524-b6b2-d6c83f21e856", "Goods / Packs Grid"), false },
																		{ ShipmentDetailsPageElements.OrdersGrid, ResString.GetMultilingualString("c9d92b1c-fb3f-4ea3-b4d8-1728a14c56e6", "Orders Grid"), false },
																		{ ShipmentDetailsPageElements.ContainersGrid, ResString.GetMultilingualString("083836b2-c1cc-4414-bf0c-b5f4e9675b47", "Containers Grid"), false }
																	 }));
			}
		}

		public BooleanRegistryItem UseShipmentPageUnShippedOrders
		{
			get
			{
				return Get("UseShipmentPageUnShippedOrders", name => new BooleanRegistryItem(name,
														 ForwardingShipmentsCategory,
														 ResString.GetMultilingualString("2e29bda1-130c-4044-94c0-2cc37836fbb6", "Show Un-shipped Orders"),
														 HintForUseShipmentPageUnShippedOrders,
														 RegistryStorageFlags.System,
														 DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
														 false));
			}
		}

		public static class ShipmentDetailsPageElements
		{
			public const string EtdAndEta = "ETD";
			public const string StorageCommencesDate = "STO";
			public const string CartageAdvisedDate = "CAR";
			public const string TransportGrid = "TRA";
			public const string GoodsPacksGrid = "PAC";
			public const string OrdersGrid = "ORD";
			public const string ContainersGrid = "CON";
		}

		#endregion

		#region CFSShipments

		public BooleanRegistryItem UseWebCFSShipmentsModule
		{
			get { return GetUseModuleRegistryItem(ResString.GetMultilingualString("92800E10-6B3A-4EE0-884E-3A2CFAC01E1A", "CFS Shipments"), CFSShipmentsCategory, WebModuleRegistry.CFSShipments, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, false, false); }
		}

		public FilterLayoutCodePairRegistryItem DefaultFilterLayoutCFSShipments
		{
			get { return GetDefaultFilterLayoutRegistryItem(ResString.GetMultilingualString("64697501-9603-4430-BBC6-ADE7EB5D8568", "CFS Shipments"), CFSShipmentsCategory, "DefaultFilterLayoutCFSShipments", "CFSShipments", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		public CodeDescriptionBoolRegistryItem CFSShipmentPageCustomisation
		{
			get
			{
				return Get("CFSShipmentPageCustomisation", name => new CodeDescriptionBoolRegistryItem(name,
																	 CFSShipmentsCategory,
																	 ResString.GetMultilingualString("E9FD92A3-A2E9-4459-A83F-1B6047905E1E", "Customize CFS Shipment Details page"),
																	 ResString.GetMultilingualString("746784A0-721C-4A6D-87CA-F1A4D61CDF35", "Use this Registry settings to hide some elements on the CFS Shipment Details web page."),
																	 RegistryStorageFlags.System,
																	 DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
																	 new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("9484F27E-8241-408A-9D21-F811F3F42632", "Hide"), true, true),
																	 new CodeDescriptionBoolCollection
																	 {
																		{ CFSShipmentDetailsPageElements.EtdAndEta, ResString.GetMultilingualString("0CBC90EF-4D6A-4AFB-A59D-288EF4CFDA34", "ETD and ETA"), false },
																		{ CFSShipmentDetailsPageElements.StorageCommencesDate, ResString.GetMultilingualString("EFB97E00-8A99-4C17-A960-9A15ED6C9CE7", "Storage Commences Date"), false },
																		{ CFSShipmentDetailsPageElements.CartageAdvisedDate, ResString.GetMultilingualString("A483D9CA-5458-405D-9019-8BE263A22A45", "Port Transport Advised Date"), false },
																		{ CFSShipmentDetailsPageElements.TransportGrid, ResString.GetMultilingualString("da745c4f-b6bc-4a66-bfbf-dbf82f378613", "Transport Grid"), false },
																		{ CFSShipmentDetailsPageElements.GoodsPacksGrid, ResString.GetMultilingualString("f4913d85-da7c-4524-b6b2-d6c83f21e856", "Goods / Packs Grid"), false },
																		{ CFSShipmentDetailsPageElements.DeliveryGrid, ResString.GetMultilingualString("8AF66155-91D4-4C28-BFFB-BA167EB84FA2", "Delivery Grid"), false },
																	 }));
			}
		}

		public static class CFSShipmentDetailsPageElements
		{
			public const string EtdAndEta = "ETD";
			public const string StorageCommencesDate = "STO";
			public const string CartageAdvisedDate = "CAR";
			public const string TransportGrid = "TRA";
			public const string GoodsPacksGrid = "PAC";
			public const string DeliveryGrid = "DEL";
		}

		#endregion

		#region Road

		public BooleanRegistryItem UseWebForwardingRoadModule
		{
			get { return GetUseModuleRegistryItem(ResString.GetMultilingualString("d4412450-950a-4d78-b1ac-6b74038a4509", "Forwarding Road Schedules"), ForwardingRoadCategory, WebModuleRegistry.RoadSchedules, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		public FilterLayoutCodePairRegistryItem DefaultFilterLayoutForwardingRoadSchedules
		{
			get { return GetDefaultFilterLayoutRegistryItem(ResString.GetMultilingualString("bfafa886-bb67-4df6-9186-574cfc632442", "Forwarding Road Schedules"), ForwardingRoadCategory, "DefaultFilterLayoutForwardingRoadSchedules", "TrackingRoadSchedules", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.NotCached); }
		}

		#endregion

		#region Rail

		public BooleanRegistryItem UseWebForwardingRailModule
		{
			get { return GetUseModuleRegistryItem(ResString.GetMultilingualString("7109de90-ac5d-4b7f-a8ab-c176e2fe7887", "Forwarding Rail Schedules"), ForwardingRailCategory, WebModuleRegistry.RailSchedules, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		public FilterLayoutCodePairRegistryItem DefaultFilterLayoutForwardingRailSchedules
		{
			get { return GetDefaultFilterLayoutRegistryItem(ResString.GetMultilingualString("d84c359e-7194-429e-9550-f742a94b6357", "Forwarding Rail Schedules"), ForwardingRailCategory, "DefaultFilterLayoutForwardingRailSchedules", "TrackingRailSchedules", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.NotCached); }
		}

		#endregion

		#region SuppressResourceStringsCheckRegion

		#region MAWB

		public BooleanRegistryItem UseWebMAWBModule
		{
			get { return GetUseModuleRegistryItem(ResString.GetMultilingualString("d4401877-9c7f-4496-8e1a-eb3100af5f71", "MAWBs"), ForwardingMAWBCategory, WebModuleRegistry.MAWB, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForDevelopers, false, false); }
		}

		public FilterLayoutCodePairRegistryItem DefaultFilterLayoutMAWB
		{
			get { return GetDefaultFilterLayoutRegistryItem(ResString.GetMultilingualString("d4401877-9c7f-4496-8e1a-eb3100af5f71", "MAWBs"), ForwardingMAWBCategory, "DefaultFilterLayoutMAWB", "TrackingMAWB", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForDevelopers); }
		}

		#endregion

		#region HAWB

		public BooleanRegistryItem UseWebHAWBModule
		{
			get { return GetUseModuleRegistryItem(ResString.GetMultilingualString("70bdf85c-cbce-470d-94e9-c997b7e3868a", "HAWBs"), ForwardingHAWBCategory, WebModuleRegistry.HAWB, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForDevelopers, false, false); }
		}

		public FilterLayoutCodePairRegistryItem DefaultFilterLayoutHAWB
		{
			get { return GetDefaultFilterLayoutRegistryItem(ResString.GetMultilingualString("70bdf85c-cbce-470d-94e9-c997b7e3868a", "HAWBs"), ForwardingHAWBCategory, "DefaultFilterLayoutHAWB", "TrackingHAWB", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForDevelopers); }
		}

		#endregion

		#endregion

		#region HVLV

		public BooleanRegistryItem UseHVLVBookingHeadersAndConsignments
		{
			get
			{
				return Get(WebModuleRegistry.HVLVBookingHeadersAndConsignments, name => new BooleanRegistryItem(name,
														 ForwardingHVLVCategory,
														 ResString.GetMultilingualString("9ba44799-deae-4c43-8b03-67e0c39b758e", "Show HVLV Booking Headers & Consignments"),
														 HintForUseHVLVBookingHeadersAndConsignments,
														 RegistryStorageFlags.System,
														 DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
														 false));
			}
		}

		#endregion

		#endregion

		#region Login and Quick View

		public BooleanRegistryItem StoreFailedLogins
		{
			get
			{
				return Get("StoreFailedLogins", name => new BooleanRegistryItem(name,
					LoginQuickViewCategory,
					(NoResString)"Store info about failed logins in the logs for the web branch",
					(NoResString)"Failed login info will be stored as a new log record on the web branch.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForDevelopers,
					false));
			}
		}

		public BooleanRegistryItem WebTrackerLoginRequiresCompanyCode
		{
			get
			{
				return Get("WebTrackerLoginRequiresCompanyCode", name => new BooleanRegistryItem(name,
						LoginQuickViewCategory,
						ResString.GetMultilingualString("6269e033-fc0f-4937-b45e-ee517e6de520", "Login requires Company Code"),
						HintForWebTrackerLoginRequiresCompanyCode,
						RegistryStorageFlags.System,
						true));
			}
		}

		public BooleanRegistryItem WebTrackerShipmentQuickView
		{
			get
			{
				return Get("WebTrackerShipmentQuickView", name => new BooleanRegistryItem(name,
						LoginQuickViewCategory,
						ResString.GetMultilingualString("29cc52ea-948a-4f95-ac73-e915a6972be1", "Show Shipment Quick View"),
						HintForWebTrackerShipmentQuickView,
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						true));
			}
		}

		public BooleanRegistryItem WebTrackerContainerQuickView
		{
			get
			{
				return Get("WebTrackerContainerQuickView", name => new BooleanRegistryItem(name,
						LoginQuickViewCategory,
						ResString.GetMultilingualString("5CA9DBCC-DE63-4490-A3E5-5A8040B83A76", "Show Container Tracking Quick View"),
						HintForWebTrackerContainerQuickView,
						RegistryStorageFlags.System,
						false));
			}
		}

		public BooleanRegistryItem WebTrackerAutoLoginRequiresPassword
		{
			get
			{
				return Get("WebTrackerAutoLoginRequiresPassword", name => new BooleanRegistryItem(name,
							LoginQuickViewCategory,
							ResString.GetMultilingualString("9a92280c-e026-4b05-b8fb-5b1a0b43c2b4", "Auto Login Requires Password"),
							HintForWebTrackerAutoLoginRequiresPassword,
							RegistryStorageFlags.System,
							RegistryOptions.PreserveTestValue,
							false));
			}
		}

		public BooleanRegistryItem WebTrackerLocalChargesOnShipmentQuickView
		{
			get
			{
				return Get("WebTrackerLocalChargesOnShipmentQuickView", name => new BooleanRegistryItem(name,
							LoginQuickViewCategory,
							ResString.GetMultilingualString("c884b1e8-21d0-4f31-bd8a-32609c67bc6a", "Show invoicing details in Shipment Quick View."),
							HintForWebTrackerLocalChargesOnShipmentQuickView,
							RegistryStorageFlags.System,
							RegistryOptions.PreserveTestValue,
							false));
			}
		}

		public BooleanRegistryItem WebTrackerQuickViewByAdditionalReferences
		{
			get
			{
				return Get("WebTrackerQuickViewByAdditionalReferences", name => new BooleanRegistryItem(name,
						LoginQuickViewCategory,
						ResString.GetMultilingualString("e7aa705f-a868-4d3d-9936-a0c7aab2cf87", "Use Additional References in Quick View"),
						HintForQuickViewByAdditionalReferences,
						RegistryStorageFlags.System,
						false));
			}
		}

		public MultilingualStringRegistryItem WebTrackerLoginPageInstruction
		{
			get
			{
				return Get("WebTrackerLoginPageInstruction", name => new MultilingualStringRegistryItem(name,
						LoginQuickViewCategory,
						ResString.GetMultilingualString("7e59d61b-cc78-4ba8-b3bd-473027c4d55b", "Login Page Instructions"),
						HintForWebTrackerLoginPageInstruction,
						RegistryStorageFlags.System)
				{
					EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo)
				});
			}
		}

		public MultilingualStringRegistryItem WebTrackerSiteTermsAndConditions => Get("WebTrackerSiteTermsAndConditions",
			name => new MultilingualStringRegistryItem(name,
				LoginQuickViewCategory,
				ResString.GetMultilingualString("6de11ac5-740c-4f02-8498-65e2fc64bef8", "Site Terms and Conditions"),
				HintForWebTrackerSiteTermsAndConditions,
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue)
			{
				EditorInfo = new TextRegistryEditorInfo(TextEditorType.HTML)
			});

		public BooleanRegistryItem WebTrackerUseCanadianReferencesQuickView
		{
			get
			{
				return Get("WebTrackerUseCanadianReferencesQuickView", name => new BooleanRegistryItem(name,
						LoginQuickViewCanadaCategory,
						ResString.GetMultilingualString("90d0d22d-2fe7-42df-a684-69528b6c31c8", "Use CCN and Transaction # in Quick View"),
						HintForWebTrackerUseCanadianReferencesQuickView,
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						false));
			}
		}

		#endregion

		#region Events

		public EventVisibilityRegistryItem EventVisibility
		{
			get
			{
				return GetItem<EventVisibilityRegistryItem>("EventVisibility", delegate
				{
					return new EventVisibilityRegistryItem(
						"EventVisibility",
						ResString.GetMultilingualString("8bed975b-0178-4ab7-89d3-75b13e2438de", "Event Visibility"),
						ResString.GetMultilingualString("a38de934-25e6-48e0-a32a-00de7e0db844", "List event codes to be shown in WebTracker."),
						WebTrackerCategory,
						RegistryStorageFlags.System,
						GetEventRegistryItemDefaultValues());
				});
			}
		}

		EventVisibilityCollection GetEventRegistryItemDefaultValues()
		{
			var defaultValue = new EventVisibilityCollection();
			defaultValue.SuspendValidation();
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
			defaultValue.ResumeValidation();
			return defaultValue;
		}

		public BooleanRegistryItem ApplyEventVisibilityOverrideRulesToMilestones
		{
			get
			{
				return Get(TrackableEvents.RegistryNames.ApplyEventVisibilityOverrideRulesToMilestones, name => new BooleanRegistryItem(name,
					NeoCategory,
					ResString.GetMultilingualString("1ec83680-36ed-4e63-a773-5032455ec7c6", "Apply Event Visibility Override Rules to Milestones"),
					ResString.GetMultilingualString("003fe0c9-2584-4f8c-9785-fa2340195c79", "Controls the application of 'Event Visibility – Overrides' rules to Milestones. Default value is ‘No’, this setting prevents ‘Event Visibility – Overrides’ rules from applying to Milestones. When set to ‘Yes’, ‘Event Visibility – Overrides’ rules will be applied to Milestones."),
					RegistryStorageFlags.System,
					false));
			}
		}

		public CodePairRegistryItem EventSortOrder
		{
			get
			{
				return Get("EventSortOrder", name => new CodePairRegistryItem(name,
																				WebTrackerCategory,
																				ResString.GetMultilingualString("5b9fb15b-61da-49f3-9acf-f7e86d5667c8", "Event Sort Order"),
																				ResString.GetMultilingualString("93a91aac-da74-4ba7-b68a-bfd00d31136f", "Controls published events sort order in the events grids for WebTracker."),
																				new CodeDescriptionPairListProvider(() => new EventSortOrderList()),
																				RegistryStorageFlags.System,
																				EventSortOrderList.Codes.Chronological));
			}
		}

		public CodePairRegistryItem EventSortOrderOfNeo
		{
			get
			{
				return Get(TrackableEvents.RegistryNames.EventSortOrderOfNeo, name => new CodePairRegistryItem(name,
																				NeoCategory,
																				ResString.GetMultilingualString("08d72968-ea50-4eed-8b3b-6b754fac1224", "Event Sort Order"),
																				ResString.GetMultilingualString("618f5b94-8248-4877-8407-607b3d5b1763", "Event Sort Order controls event sort order in the tracking events grid of Neo."),
																				new CodeDescriptionPairListProvider(() => new EventSortOrderList()),
																				RegistryStorageFlags.System,
																				EventSortOrderList.Codes.ReversedChronological));
			}
		}

		public BooleanRegistryItem EventIncludeEstimates
		{
			get
			{
				return Get("EventIncludeEstimates", name => new BooleanRegistryItem(name,
																				WebTrackerCategory,
																				ResString.GetMultilingualString("23a7f09e-d813-4280-bb7c-7bb188d2aa18", "Include Estimates"),
																				ResString.GetMultilingualString("0bfdb200-6881-4a3b-81f4-96096dbe366e", "Display estimated events in WebTracker."),
																				RegistryStorageFlags.System,
																				false));
			}
		}

		public BooleanRegistryItem EventIncludeRelated
		{
			get
			{
				return Get("EventIncludeRelated", name => new BooleanRegistryItem(name,
																				WebTrackerCategory,
																				ResString.GetMultilingualString("18d3afdc-fba9-4c86-bb76-d5b641ae7ab0", "Include Related Events"),
																				ResString.GetMultilingualString("50dc1113-485c-40b8-9e80-4aa47a387c3e", "Display related events in WebTracker."),
																				RegistryStorageFlags.System,
																				true));
			}
		}

		public EventVisibilityOverrideRegistryItem EventVisibilityOverride
		{
			get
			{
				return GetItem<EventVisibilityOverrideRegistryItem>("EventVisibilityOverride", delegate
				{
					return new EventVisibilityOverrideRegistryItem(
						TrackableEvents.RegistryNames.EventVisibilityOverride,
						ResString.GetMultilingualString("d73aa591-99fc-4168-8b87-3a526beb3663", "Event Visibility – Overrides"),
						ResString.GetMultilingualString("712ad385-2ac7-461c-9104-2eafb879c574", @"List of Events to be shown in Neo per workflow process type. When Include Related Events is enabled, related events will be displayed in Neo.

Event descriptions can be overridden to suit the business.

Event Details can be hidden.

When there are duplicate events against the same workflow process type the first, last or all events can be shown.

When Quick View is enabled listed Events will be visible to anonymous trackers (no login required).

When Include Estimates is enabled, estimated events will be displayed in Neo."),
						NeoCategory,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						GetEventVisibilityOverrideDefaultValues());
				});
			}
		}

		EventVisibilityOverrideCollection GetEventVisibilityOverrideDefaultValues()
		{
			var defaultWorkflowCodes = TrackableEvents.DefaultWorkflowCodes;

			Array.Sort(defaultWorkflowCodes);

			var defaultValue = new EventVisibilityOverrideCollection();
			defaultValue.SuspendValidation();

			foreach (var workflowCode in defaultWorkflowCodes)
			{
				defaultValue.AddNew(workflowCode);
			}

			defaultValue.ResumeValidation();

			return defaultValue;
		}

		#endregion

		#region Milestones

		#region Milestone Event Updates

		#region Warehouse

		#region Order

		public WarehouseOrderMilestoneEventUpdatesRegistryItem WarehouseOrderMilestoneEventUpdates
		{
			get
			{
				return GetItem<WarehouseOrderMilestoneEventUpdatesRegistryItem>("WarehouseOrderMilestoneEventUpdates", delegate
				{
					return new WarehouseOrderMilestoneEventUpdatesRegistryItem(
						"WarehouseOrderMilestoneEventUpdates",
						ResString.GetMultilingualString("7ab3fad4-e27b-4ae1-8004-9e79e0c047ec", "Milestone Updates"),
						HintForWarehouseOrderMilestoneUpdateSettings,
						WarehouseOrdersCategory,
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						GetWarehouseOrderMilestoneEventUpdatesDefaultValue());
				});
			}
		}

		WarehouseOrderMilestoneEventUpdatesCollection GetWarehouseOrderMilestoneEventUpdatesDefaultValue()
		{
			WarehouseOrderMilestoneEventUpdatesCollection defaultValue = new WarehouseOrderMilestoneEventUpdatesCollection();
			defaultValue.SuspendValidation();
			defaultValue.AddNew("WHE", WebPartyType.Client, WebPartyType.GoodsBilledTo);
			defaultValue.AddNew("WHI");
			defaultValue.AddNew("FIN");
			defaultValue.ResumeValidation();
			return defaultValue;
		}

		#endregion

		#region Receive

		public WarehouseReceiveMilestoneEventUpdatesRegistryItem WarehouseReceiveMilestoneEventUpdates
		{
			get
			{
				return GetItem<WarehouseReceiveMilestoneEventUpdatesRegistryItem>("WarehouseReceiveMilestoneEventUpdates", delegate
				{
					return new WarehouseReceiveMilestoneEventUpdatesRegistryItem(
						"WarehouseReceiveMilestoneEventUpdates",
						ResString.GetMultilingualString("7ab3fad4-e27b-4ae1-8004-9e79e0c047ec", "Milestone Updates"),
						HintForWarehouseReceiveMilestoneUpdateSettings,
						WarehouseReceiptsCategory,
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						GetWarehouseReceiveMilestoneEventUpdatesDefaultValue());
				});
			}
		}

		WarehouseReceiveMilestoneEventUpdatesCollection GetWarehouseReceiveMilestoneEventUpdatesDefaultValue()
		{
			WarehouseReceiveMilestoneEventUpdatesCollection defaultValue = new WarehouseReceiveMilestoneEventUpdatesCollection();
			defaultValue.SuspendValidation();
			defaultValue.AddNew("WHE", WebPartyType.Client);
			defaultValue.AddNew("WHT", WebPartyType.Supplier, WebPartyType.Transport);
			defaultValue.AddNew("WHA", WebPartyType.Transport);
			defaultValue.AddNew("WHU", WebPartyType.Transport);
			defaultValue.AddNew("WHP");
			defaultValue.AddNew("FIN");
			defaultValue.ResumeValidation();
			return defaultValue;
		}

		#endregion

		#endregion

		#region Transport

		#region Cartage

		public CartageMilestoneEventUpdatesRegistryItem CartageMilestoneEventUpdates
		{
			get
			{
				return GetItem<CartageMilestoneEventUpdatesRegistryItem>("CartageMilestoneEventUpdates", delegate
				{
					return new CartageMilestoneEventUpdatesRegistryItem(
						"CartageMilestoneEventUpdates",
						ResString.GetMultilingualString("7ab3fad4-e27b-4ae1-8004-9e79e0c047ec", "Milestone Updates"),
						HintForLocalTransportMilestoneUpdateSettings,
						TransportJobsCategory,
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						GetCartageMilestoneEventUpdatesDefaultValue());
				});
			}
		}

		CartageMilestoneEventUpdatesCollection GetCartageMilestoneEventUpdatesDefaultValue()
		{
			CartageMilestoneEventUpdatesCollection defaultValue = new CartageMilestoneEventUpdatesCollection();
			return defaultValue;
		}

		#endregion

		#endregion

		#region LinerAndAgency

		#region Booking

		public ShippingBookingMilestoneEventUpdatesRegistryItem LinerAndAgencyBookingMilestoneEventUpdates
		{
			get
			{
				return GetItem<ShippingBookingMilestoneEventUpdatesRegistryItem>("ShippingBookingMilestoneEventUpdates", delegate
				{
					return new ShippingBookingMilestoneEventUpdatesRegistryItem(
						"ShippingBookingMilestoneEventUpdates",
						ResString.GetMultilingualString("7ab3fad4-e27b-4ae1-8004-9e79e0c047ec", "Milestone Updates"),
						HintForLinerAndAgencyBookingMilestoneUpdateSettings,
						LinerAndAgencyBookingsCategory,
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						GetLinerAndAgencyBookingMilestoneEventUpdatesDefaultValue());
				});
			}
		}

		ShippingBookingMilestoneEventUpdatesCollection GetLinerAndAgencyBookingMilestoneEventUpdatesDefaultValue()
		{
			var defaultValue = new ShippingBookingMilestoneEventUpdatesCollection();
			return defaultValue;
		}

		#endregion

		#region BillOfLading

		public BillOfLadingMilestoneEventUpdatesRegistryItem BillOfLadingMilestoneEventUpdates
		{
			get
			{
				return GetItem<BillOfLadingMilestoneEventUpdatesRegistryItem>("BillOfLadingMilestoneEventUpdates", delegate
				{
					return new BillOfLadingMilestoneEventUpdatesRegistryItem(
						"BillOfLadingMilestoneEventUpdates",
						ResString.GetMultilingualString("7ab3fad4-e27b-4ae1-8004-9e79e0c047ec", "Milestone Updates"),
						HintForLinerAndAgencyBillOfLadingMilestoneUpdateSettings,
						LinerAndAgencyBillsOfLadingCategory,
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						GetBillOfLadingMilestoneEventUpdatesDefaultValue());
				});
			}
		}

		BillOfLadingMilestoneEventUpdatesCollection GetBillOfLadingMilestoneEventUpdatesDefaultValue()
		{
			BillOfLadingMilestoneEventUpdatesCollection defaultValue = new BillOfLadingMilestoneEventUpdatesCollection();
			return defaultValue;
		}

		#endregion

		#endregion

		#region Customs

		#region ISF

		public ISFMilestoneEventUpdatesRegistryItem ISFMilestoneEventUpdates
		{
			get
			{
				return GetItem<ISFMilestoneEventUpdatesRegistryItem>("ISFMilestoneEventUpdates", delegate
				{
					return new ISFMilestoneEventUpdatesRegistryItem(
						"ISFMilestoneEventUpdates",
						ResString.GetMultilingualString("7ab3fad4-e27b-4ae1-8004-9e79e0c047ec", "Milestone Updates"),
						HintForISFMilestoneUpdateSettings,
						CustomsISFCategory,
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						GetISFMilestoneEventUpdatesDefaultValue());
				});
			}
		}

		ISFMilestoneEventUpdatesCollection GetISFMilestoneEventUpdatesDefaultValue()
		{
			ISFMilestoneEventUpdatesCollection defaultValue = new ISFMilestoneEventUpdatesCollection();
			return defaultValue;
		}

		#endregion

		#region Declaration

		public DeclarationMilestoneEventUpdatesRegistryItem DeclarationMilestoneEventUpdates
		{
			get
			{
				return GetItem<DeclarationMilestoneEventUpdatesRegistryItem>("DeclarationMilestoneEventUpdates", delegate
				{
					return new DeclarationMilestoneEventUpdatesRegistryItem(
						"DeclarationMilestoneEventUpdates",
						ResString.GetMultilingualString("7ab3fad4-e27b-4ae1-8004-9e79e0c047ec", "Milestone Updates"),
						HintForDeclarationMilestoneUpdateSettings,
						CustomsDeclarationCategory,
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						GetDeclarationMilestoneEventUpdatesDefaultValue());
				});
			}
		}

		DeclarationMilestoneEventUpdatesCollection GetDeclarationMilestoneEventUpdatesDefaultValue()
		{
			DeclarationMilestoneEventUpdatesCollection defaultValue = new DeclarationMilestoneEventUpdatesCollection();
			defaultValue.SuspendValidation();
			defaultValue.AddNew("CCC", WebPartyType.Carrier, WebPartyType.ExternalBroker, WebPartyType.Forwarder);
			defaultValue.AddNew("CLR", WebPartyType.Carrier, WebPartyType.ExternalBroker, WebPartyType.Forwarder);
			defaultValue.AddNew("ECM", WebPartyType.Carrier, WebPartyType.ExternalBroker, WebPartyType.Forwarder);
			defaultValue.AddNew("ECC", WebPartyType.Carrier, WebPartyType.ExternalBroker, WebPartyType.Forwarder);
			defaultValue.ResumeValidation();
			return defaultValue;
		}

		#endregion

		#endregion

		#region Forwarding

		#region Order

		public OrderMilestoneEventUpdatesRegistryItem OrderMilestoneEventUpdates
		{
			get
			{
				return GetItem<OrderMilestoneEventUpdatesRegistryItem>("OrderMilestoneEventUpdates", delegate
				{
					return new OrderMilestoneEventUpdatesRegistryItem(
						"OrderMilestoneEventUpdates",
						ResString.GetMultilingualString("7ab3fad4-e27b-4ae1-8004-9e79e0c047ec", "Milestone Updates"),
						HintForOrderMilestoneUpdateSettings,
						ForwardingOrdersCategory,
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						GetOrderMilestoneEventUpdatesDefaultValue());
				});
			}
		}

		OrderMilestoneEventUpdatesCollection GetOrderMilestoneEventUpdatesDefaultValue()
		{
			OrderMilestoneEventUpdatesCollection defaultValue = new OrderMilestoneEventUpdatesCollection();
			defaultValue.SuspendValidation();
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
			defaultValue.ResumeValidation();
			return defaultValue;
		}

		#endregion

		#region Container

		public ContainerMilestoneEventUpdatesRegistryItem ContainerMilestoneEventUpdates
		{
			get
			{
				return GetItem<ContainerMilestoneEventUpdatesRegistryItem>("ContainerMilestoneEventUpdates", delegate
				{
					return new ContainerMilestoneEventUpdatesRegistryItem(
						"ContainerMilestoneEventUpdates",
						ResString.GetMultilingualString("7ab3fad4-e27b-4ae1-8004-9e79e0c047ec", "Milestone Updates"),
						HintForContainerMilestoneUpdateSettings,
						ForwardingContainersCategory,
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						GetContainerMilestoneEventUpdatesDefaultValue());
				});
			}
		}

		ContainerMilestoneEventUpdatesCollection GetContainerMilestoneEventUpdatesDefaultValue()
		{
			ContainerMilestoneEventUpdatesCollection defaultValue = new ContainerMilestoneEventUpdatesCollection();
			return defaultValue;
		}

		#endregion

		#region LinerAndAgencyContainer

		public LinerAndAgencyContainerMilestoneEventUpdatesRegistryItem LinerAndAgencyContainerMilestoneEventUpdates
		{
			get
			{
				return GetItem<LinerAndAgencyContainerMilestoneEventUpdatesRegistryItem>("LinerAndAgencyContainerMilestoneEventUpdates", delegate
				{
					return new LinerAndAgencyContainerMilestoneEventUpdatesRegistryItem(
						"LinerAndAgencyContainerMilestoneEventUpdates",
						ResString.GetMultilingualString("7ab3fad4-e27b-4ae1-8004-9e79e0c047ec", "Milestone Updates"),
						HintForContainerMilestoneUpdateSettings,
						LinerAndAgencyContainersCategory,
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						GetLinerAndAgencyContainerMilestoneEventUpdatesDefaultValue());
				});
			}
		}

		LinerAndAgencyContainerMilestoneEventUpdatesCollection GetLinerAndAgencyContainerMilestoneEventUpdatesDefaultValue()
		{
			var defaultValue = new LinerAndAgencyContainerMilestoneEventUpdatesCollection();
			return defaultValue;
		}

		#endregion

		#region Booking

		public BookingMilestoneEventUpdatesRegistryItem BookingMilestoneEventUpdates
		{
			get
			{
				return GetItem<BookingMilestoneEventUpdatesRegistryItem>("BookingMilestoneEventUpdates", delegate
				{
					return new BookingMilestoneEventUpdatesRegistryItem(
						"BookingMilestoneEventUpdates",
						ResString.GetMultilingualString("7ab3fad4-e27b-4ae1-8004-9e79e0c047ec", "Milestone Updates"),
						HintForBookingMilestoneUpdateSettings,
						ForwardingBookingsCategory,
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						GetBookingMilestoneEventUpdatesDefaultValue());
				});
			}
		}

		BookingMilestoneEventUpdatesCollection GetBookingMilestoneEventUpdatesDefaultValue()
		{
			BookingMilestoneEventUpdatesCollection defaultValue = new BookingMilestoneEventUpdatesCollection();
			return defaultValue;
		}

		#endregion

		#region Shipment

		public ShipmentMilestoneEventUpdatesRegistryItem ShipmentMilestoneEventUpdates
		{
			get
			{
				return GetItem<ShipmentMilestoneEventUpdatesRegistryItem>("ShipmentMilestoneEventUpdates", delegate
				{
					return new ShipmentMilestoneEventUpdatesRegistryItem(
						"ShipmentMilestoneEventUpdates",
						ResString.GetMultilingualString("7ab3fad4-e27b-4ae1-8004-9e79e0c047ec", "Milestone Updates"),
						HintForShipmentMilestoneUpdateSettings,
						ForwardingShipmentsCategory,
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						GetShipmentMilestoneEventUpdatesDefaultValue());
				});
			}
		}

		ShipmentMilestoneEventUpdatesCollection GetShipmentMilestoneEventUpdatesDefaultValue()
		{
			ShipmentMilestoneEventUpdatesCollection defaultValue = new ShipmentMilestoneEventUpdatesCollection();
			defaultValue.SuspendValidation();
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
			defaultValue.ResumeValidation();
			return defaultValue;
		}

		#endregion

		#endregion

		#endregion

		public CodePairRegistryItem MilestoneSortOrder
		{
			get
			{
				return Get("MilestoneSortOrder", name =>
					new CodePairRegistryItem(
						name,
						MilestonesCategory,
						ResString.GetMultilingualString("9757780d-a4d0-4a4a-96e5-5c83fe7ab54e", "Milestone Sort Order"),
						ResString.GetMultilingualString("505d2fa5-9d9e-4156-8f44-322e2b3792b8", "Controls published milestones sort order in the milestones grids for WebTracker. This also affects the Order Tracking Dates for WebTracker."),
						new CodeDescriptionPairListProvider(() => new MilestoneSortOrderList()),
						RegistryStorageFlags.System,
						MilestoneSortOrderList.Codes.Chronological
					)
				);
			}
		}

		public CodePairRegistryItem MilestoneVisibility
		{
			get
			{
				return Get("MilestoneVisibility", name =>
					new CodePairRegistryItem(
						name,
						MilestonesCategory,
						ResString.GetMultilingualString("5018fcee-e65c-45a8-a6a2-ece8c4af0d7f", "Milestone Visibility"),
						ResString.GetMultilingualString("ec1d346f-9bee-4a34-8613-afd681e4bac4", "Controls when published milestones appear in the milestones grids for WebTracker. This also affects the Order Tracking Dates for WebTracker."),
						new CodeDescriptionPairListProvider(() => new MilestoneVisibilityList()),
						RegistryStorageFlags.System,
						MilestoneVisibilityList.Codes.All
					)
				);
			}
		}

		public CodePairRegistryItem MilestoneDatesVisibility
		{
			get
			{
				return Get("MilestoneDatesVisibility", name =>
					new CodePairRegistryItem(
						name,
						MilestonesCategory,
						ResString.GetMultilingualString("eed7f24c-8450-4063-9ca1-6bd1a3b6a0be", "Milestone Dates"),
						ResString.GetMultilingualString("55fa2a41-9a5d-45c3-a97c-b14677cdabf8", "Controls the display of the dates within the milestones grids for WebTracker. This also affects the Order Tracking Dates for WebTracker."),
						new CodeDescriptionPairListProvider(() => new MilestoneDatesVisibilityList()),
						RegistryStorageFlags.System,
						MilestoneDatesVisibilityList.Codes.All
					)
				);
			}
		}

		public CodePairRegistryItem MilestoneStatusVisibility
		{
			get
			{
				return Get("MilestoneStatusVisibility", name =>
					new CodePairRegistryItem(
						name,
						MilestonesCategory,
						ResString.GetMultilingualString("ab405c26-51e7-46db-90a9-06f395d74e1d", "Milestone Status"),
						ResString.GetMultilingualString("da170b71-8e71-41a9-aba7-a11adb071ad6", "Controls the display of the milestone status within the milestones grids for WebTracker. This also affects the Order Tracking Dates for WebTracker."),
						new CodeDescriptionPairListProvider(() => new MilestoneStatusVisibilityList()),
						RegistryStorageFlags.System,
						MilestoneStatusVisibilityList.Codes.All
					)
				);
			}
		}

		#endregion

		#region Notifications

		public GuidRegistryItem WebAdminsEmailNotificationGroup
		{
			get
			{
				return Get("WebAdminsEmailNotificationGroup", name =>
				{
					var item = new GuidRegistryItem(name,
						WebCategory,
						ResString.GetMultilingualString("5ea03981-a0c5-4f1e-9d8f-6a1107b3a95a", "Web Site Administrators Email Notification Group"),
						ResString.GetMultilingualString("9ab52936-fa34-4871-8585-b767e4735c8a", "The group that will be sent email notifications for any WebTracker management events."),
						RegistryStorageFlags.System,
						RegistryFactory.Instance.GetGroupPK("ALL"));
					item.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return item;
				});
			}
		}

		#endregion

		#region Password Reset

		public NotificationEmailTemplateRegistryItem PasswordResetSuccessfullyEmailTemplate
		{
			get
			{
				return Get<NotificationEmailTemplateRegistryItem>("PasswordResetSuccessfullyEmailTemplate", delegate
				{
					var registryItem = new NotificationEmailTemplateRegistryItem("PasswordResetSuccessfullyEmailTemplate",
						PasswordResetCategory,
						ResString.GetMultilingualString("3C03DFA9-A015-452F-AD01-49FB97BE9292", "Password Reset Successfully Email Template"),
						ResString.GetMultilingualString("CEA4DEDD-BEA1-4020-A15C-78DA567710D5", "Template that will be used in reset confirmation emails message."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						ObjectFactory.GetType<Enterprise.Integration.DocumentWrappers.IDocContactPasswordInstructionEmail>(),
						(NoResString)"(*CurrentCompanyName*) Password Reset Complete",
						PasswordResetSuccessfullyEmailTemplateDefaultBody);
					return registryItem;
				});
			}
		}

		public NotificationEmailTemplateRegistryItem PasswordResetEmailTemplate
		{
			get
			{
				return Get<NotificationEmailTemplateRegistryItem>("PasswordResetEmailTemplate", delegate
				{
					var registryItem = new NotificationEmailTemplateRegistryItem("PasswordResetEmailTemplate",
					 PasswordResetCategory,
					 ResString.GetMultilingualString("C140C9B7-0AA4-4615-B03C-49BE17F71BD1", "Password Reset Email Template"),
					 ResString.GetMultilingualString("A10755D3-3D56-41F3-B314-759F7BB4A098", "Template that will be used in Password Reset emails."),
					 RegistryStorageFlags.System | RegistryStorageFlags.Company,
					 RegistryOptions.Default,
					 ObjectFactory.GetType<Enterprise.Integration.DocumentWrappers.IDocContactPasswordInstructionEmail>(),
					 (NoResString)"(*CurrentCompanyName*) Password Reset",
					 PasswordResetEmailTemplateDefaultBody);
					return registryItem;
				});
			}
		}

		public ParameterizedStringRegistryItem PasswordResetEmailFooter
		{
			get
			{
				return Get("PasswordReminderEmailFooter", name => new ParameterizedStringRegistryItem(name,
																						 PasswordResetCategory,
																						 ResString.GetMultilingualString("8768fed5-56d6-41eb-8102-f2e6287980e1", "Password Reset Email Footer"),
																						 GetHintForPasswordResetEmailFooter(GetDefaultValueForPasswordResetEmailFooter(Env.Registry.MailboxDisplayName)),
																						 RegistryStorageFlags.System | RegistryStorageFlags.Company,
																						 RegistryOptions.Default,
																						 GetDefaultValueForPasswordResetEmailFooter(Env.Registry.MailboxDisplayName),
																						 ResString.GetMultilingualString("b0f2032f-7788-4982-8de8-a1113e20a7c8", "Mailbox Display Name")
																						 ));
			}
		}

		public NotificationEmailTemplateRegistryItem MasterPasswordResetSuccessfullyEmailTemplate
		{
			get
			{
				return Get<NotificationEmailTemplateRegistryItem>("MasterPasswordResetSuccessfullyEmailTemplate", delegate
				{
					var registryItem = new NotificationEmailTemplateRegistryItem("MasterPasswordResetSuccessfullyEmailTemplate",
						PasswordResetCategory,
						ResString.GetMultilingualString("972f542d-caae-4423-a0df-b16ea291ff59", "Master Password Reset Successfully Email Template"),
						ResString.GetMultilingualString("88cc973c-93cb-47eb-89f6-887ef4b681a9", "Template that will be used in reset master password confirmation emails message."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						ObjectFactory.GetType<Enterprise.Integration.DocumentWrappers.IDocContactMasterPasswordInstructionEmail>(),
						(NoResString)"(*CurrentCompanyName*) Password Reset Complete",
						MasterPasswordResetSuccessfullyEmailTemplateDefaultBody);
					return registryItem;
				});
			}
		}

		ZString PasswordResetEmailTemplateDefaultBody => @"<font face=""Arial"">
   <div>
      <strong>
      <br>
      </strong>
   </div>
   <div>
      <strong>(*CurrentCompanyName*) Password Reset</strong>
   </div>
   <div>
      <br>
   </div>
   <div>
      <font face=""Arial"" size=""2""></font>
      <font face=""Arial"" size=""2"">Hi (*ContactName*),<br></font>
   </div>
   <div>
      <br>
   </div>
   <div>
      <font face=""Arial"" size=""2""></font>
      <font face=""Arial"" size=""2"">We received your request to have your password reset. If you have lost your password or wish to reset it, click the 'Reset Password' button to set a new password. <br></font>
   </div>
   <div>
      <font face=""Arial"" size=""2"">
      <br>
      </font>
   </div>
   <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
      <tr>
         <td>
            <table border=""0"" cellspacing=""0"" cellpadding=""0"">
               <tr>
                  <td>
                     <a href=""(*PasswordInstructionUrl*)"" target=""_blank"" style=""font-size: 12px; font-family:Arial; color: #ffffff; text-decoration: none; border-radius: 3px; background-color: #371ee1; border-top: 10px solid #371ee1; border-bottom: 10px solid #371ee1; border-right: 22px solid #371ee1; border-left: 22px solid #371ee1; display: inline-block;"">
                        <b>
                           <!--[if mso]>&nbsp;&nbsp;&nbsp;<![endif]-->Reset Password
                        </b>
                        <!--[if mso]>&nbsp;&nbsp;&nbsp;<![endif]-->
                     </a>
                  </td>
               </tr>
            </table>
         </td>
      </tr>
   </table>
   <div>
      <div></div>
      <div><em></em></div>
   </div>
   <Br>
   <font face=""Arial"" size=""2"">
      <strong>Important</strong>:
      <div>
         <font size=""2"">This email is a system generated email, initiated by a password reset request action taken by you, or by (*CurrentCompanyName*).</font>
      </div>
      <div>
         <font size=""2"">
            If you are not expecting a password reset request, please ignore this email and do not click the Reset Password button or link as only a person with access to your email can reset your password.
      </div>
      <div>
      <font face=""Arial"">
      <div>
      <font size=""2"">
      <br>
      </font>
      </div>
      <div>
      <font size=""2"">If you have issues clicking the link, copy and paste the following line into your browser: <br><a href=""(*PasswordInstructionUrl*)"">(*PasswordInstructionUrl*)</a></font>
      </div>
      <div>
      <br>
      </div>
      </font>
      </div>
      <div>
      <div>
      <em>
      <font face=""Arial"" size=""2"">
      <strong>Note</strong>:  This password reset link is only valid for the next 24 hours.   </font>
      </em>
      </div>
      <div>
      <em>
      <br>
      </em>
      </div>
      </div>
      </font>
   </font>
</font>";

		ZString PasswordResetSuccessfullyEmailTemplateDefaultBody => (NoResString)@"<font face=""Arial"">
   <div>
      <strong>
      <br>
      </strong>
   </div>
   <div>
      <strong>(*CurrentCompanyName*) Password Reset Complete</strong>
   </div>
   <div>
      <br>
   </div>
   <div>
      <font face=""Arial"" size=""2""></font>
      <font face=""Arial"" size=""2"">Hi (*ContactName*),<br></font>
   </div>
   <div>
      <br>
      <font face=""Arial"" size=""2""></font>
      <font face=""Arial"" size=""2"">As you requested, your password has successfully been reset and you can now safely use your new password to login to your account. Please record your password in a secure location for future reference.<br></font>
   </div>
   <div>
      <font face=""Arial"" size=""2"">
         <Br>
         <font face=""Arial"" size=""2"">
            <strong>Important</strong>:
            <div>
               <font size=""2"">This email is a system generated email, initiated by a password reset completion action.</font>
            </div>
            <div>
               <font size=""2"">If you are not expecting a password reset completion action, please contact (*CurrentCompanyName*) immediately.</font>
            </div>
         </font>
      </font>
   </div>
</font>";

		ZString MasterPasswordResetSuccessfullyEmailTemplateDefaultBody => (NoResString)@"<font face=""Arial"">
   <div>
      <strong>
      <br>
      </strong>
   </div>
   <div>
      <strong>(*CurrentCompanyName*) Password Reset Complete</strong>
   </div>
   <div>
      <br>
   </div>
   <div>
      <font face=""Arial"" size=""2""></font>
      <font face=""Arial"" size=""2"">Hi (*ContactName*),<br></font>
   </div>
   <div>
      <br>
      <font face=""Arial"" size=""2""></font>
      <font face=""Arial"" size=""2"">As you requested, your password has successfully been reset and you can now safely use your new password to login to your account. Please record your password in a secure location for future reference.<br></font>
   </div>
   <div>
      <font face=""Arial"" size=""2"">
         <Br>
         <font face=""Arial"" size=""2"">
            <strong>Important</strong>:
            <div>
               <font size=""2"">This email is a system generated email, initiated by a password reset completion action.</font>
            </div>
            <div>
               <font size=""2"">If you are not expecting a password reset completion action, please contact (*CurrentCompanyName*) immediately.</font>
            </div>
         </font>
      </font>
   </div>
</font>";

		#endregion

		#region Password Set

		public NotificationEmailTemplateRegistryItem PasswordSetEmailTemplate
		{
			get
			{
				return Get<NotificationEmailTemplateRegistryItem>("PasswordSetEmailTemplate", delegate
				{
					var registryItem = new NotificationEmailTemplateRegistryItem("PasswordSetEmailTemplate",
						PasswordSetCategory,
						ResString.GetMultilingualString("3d59180a-dd58-4e05-8a69-4f4381163135", "Password Set Email Template"),
						ResString.GetMultilingualString("c59608dd-c985-4cb0-98b5-20e8758747b9", "Template that will be used in Password Set Emails."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						ObjectFactory.GetType<Enterprise.Integration.DocumentWrappers.IDocContactPasswordInstructionEmail>(),
						(NoResString)"(*CurrentCompanyName*) Password Set",
						PasswordSetEmailTemplateDefaultBody);
					return registryItem;
				});
			}
		}

		public ZString PasswordSetEmailTemplateDefaultBody => @"<font face=""Arial"">
   <div>
      <strong>
      <br>
      </strong>
   </div>
   <div>
      <strong>(*CurrentCompanyName*) Password Set</strong>
   </div>
   <div>
      <br>
   </div>
   <div>
      <font face=""Arial"" size=""2""></font>
      <font face=""Arial"" size=""2"">Hi (*ContactName*),<br></font>
   </div>
   <div>
      <br>
   </div>
   <div>
      <font face=""Arial"" size=""2""></font>
      <font face=""Arial"" size=""2"">We received your request to have your password set for the first time. Click the 'Password Set' button to set a password for your account. <br></font>
   </div>
   <div>
      <font face=""Arial"" size=""2"">
      <br>
      </font>
   </div>
   <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
      <tr>
         <td>
            <table border=""0"" cellspacing=""0"" cellpadding=""0"">
               <tr>
                  <td>
                     <a href=""(*PasswordInstructionUrl*)"" target=""_blank"" style=""font-size: 12px; font-family:Arial; color: #ffffff; text-decoration: none; border-radius: 3px; background-color: #371ee1; border-top: 10px solid #371ee1; border-bottom: 10px solid #371ee1; border-right: 22px solid #371ee1; border-left: 22px solid #371ee1; display: inline-block;"">
                        <b>
                           <!--[if mso]>&nbsp;&nbsp;&nbsp;<![endif]-->Password Set
                        </b>
                        <!--[if mso]>&nbsp;&nbsp;&nbsp;<![endif]-->
                     </a>
                  </td>
               </tr>
            </table>
         </td>
      </tr>
   </table>
   <div>
      <div></div>
      <div><em></em></div>
   </div>
   <Br>
   <font face=""Arial"" size=""2"">
      <strong>Important</strong>:
      <div>
         <font size=""2"">This email is a system generated email, initiated by a password set request action taken by you, or by (*CurrentCompanyName*).</font>
      </div>
      <div>
         <font size=""2"">
            If you are not expecting a password set request, please ignore this email and do not click the Set Password button or link as only a person with access to your email can set your password.
      </div>
      <div>
      <font face=""Arial"">
      <div>
      <font size=""2"">
      <br>
      </font>
      </div>
      <div>
      <font size=""2"">If you have issues clicking the link, copy and paste the following line into your browser: <br><a href=""(*PasswordInstructionUrl*)"">(*PasswordInstructionUrl*)</a></font>
      </div>
      <div>
      <br>
      </div>
      </font>
      </div>
      <div>
      <div>
      <em>
      <font face=""Arial"" size=""2"">
      <strong>Note</strong>: This password set link is only valid for the next 24 hours.</font>
      </em>
      </div>
      <div>
      <em>
      <br>
      </em>
      </div>
      </div>
      </font>
   </font>
</font>";

		public ParameterizedStringRegistryItem PasswordSetEmailFooter
		{
			get
			{
				return Get("PasswordSetReminderEmailFooter", name => new ParameterizedStringRegistryItem(name,
																						 PasswordSetCategory,
																						 ResString.GetMultilingualString("a1475843-59d6-4ab2-944d-f7ec708c540e", "Password Set Email Footer"),
																						 GetHintForPasswordSetEmailFooter(GetDefaultValueForPasswordSetEmailFooter(Env.Registry.MailboxDisplayName)),
																						 RegistryStorageFlags.System | RegistryStorageFlags.Company,
																						 RegistryOptions.Default,
																						 GetDefaultValueForPasswordSetEmailFooter(Env.Registry.MailboxDisplayName),
																						 ResString.GetMultilingualString("b0f2032f-7788-4982-8de8-a1113e20a7c8", "Mailbox Display Name")
																						 ));
			}
		}

		public MultilingualStringRegistryItem PasswordSetEmailUnsuccessfulMessage
		{
			get
			{
				return Get("PasswordSetReminderEmailUnsuccessfulMessage", name => new MultilingualStringRegistryItem(name,
																										PasswordSetCategory,
																										ResString.GetMultilingualString("5126c207-ec62-43f8-af1b-ef77235df77b", "Password Set Unsuccessful Message"),
																										HintForPasswordSetEmailUnsuccessfulMessage,
																										RegistryStorageFlags.System,
																										DefaultValueForPasswordSetEmailUnsuccessfulMessage)
				{
					EditorInfo = new TextRegistryEditorInfo(TextEditorType.TextBox)
				});
			}
		}

		public NotificationEmailTemplateRegistryItem PasswordSetSuccessfullyEmailTemplate
		{
			get
			{
				return Get<NotificationEmailTemplateRegistryItem>("PasswordSetSuccessfullyEmailTemplate", delegate
				{
					var registryItem = new NotificationEmailTemplateRegistryItem("PasswordSetSuccessfullyEmailTemplate",
						PasswordSetCategory,
						ResString.GetMultilingualString("E4129B50-C762-4F5F-B200-0C1F41274895", "Password Set Successfully Email Template"),
						ResString.GetMultilingualString("425669D1-ED89-425D-8F10-7189D734D656", "Template that will be used in set password confirmation emails message."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						ObjectFactory.GetType<Enterprise.Integration.DocumentWrappers.IDocContactPasswordInstructionEmail>(),
						(NoResString)"(*CurrentCompanyName*) Password Set Complete",
						PasswordSetSuccessfullyEmailTemplateDefaultBody);
					return registryItem;
				});
			}
		}

		ZString PasswordSetSuccessfullyEmailTemplateDefaultBody => (NoResString)@"<font face=""Arial"">
	<div>
		<strong><br></strong>
	</div>
	<div>
		<strong>(*CurrentCompanyName*) Password Set Complete</strong>
	</div>
	<div>
		<br>
	</div>
	<div>
		<font face=""Arial"" size=""2""/>
		<font face=""Arial"" size=""2"">Hi (*ContactName*),<br></font>
	</div>
	<div>
		<br>
			<font face=""Arial"" size=""2""/>
			<font face=""Arial"" size=""2"">As you requested, your password has successfully been set and you can now safely login to your account. Please record your password in a secure location for future reference. <br></font>
	</div>
	<div>
		<font face=""Arial"" size=""2"">
			<Br>
			<font face=""Arial"" size=""2"">
				<strong>Important</strong>:
				<div>
					<font size=""2"">This email is a system generated email, initiated by a password set completion action.</font>
				</div>
				<div>
					<font size=""2"">If you are not expecting a password set completion action, please contact (*CurrentCompanyName*) immediately.</font>
				</div>
			</font>
		</font>
	</div>
</font>";

		public NotificationEmailTemplateRegistryItem MasterPasswordSetSuccessfullyEmailTemplate
		{
			get
			{
				return Get<NotificationEmailTemplateRegistryItem>("MasterPasswordSetSuccessfullyEmailTemplate", delegate
				{
					var registryItem = new NotificationEmailTemplateRegistryItem("MasterPasswordSetSuccessfullyEmailTemplate",
						PasswordSetCategory,
						ResString.GetMultilingualString("308ea6e3-da4b-4a90-810f-93d28c574a01", "Master Password Set Successfully Email Template"),
						ResString.GetMultilingualString("602284c9-7af0-4032-862f-9c4fa6209579", "Template that will be used in set master password confirmation emails message."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						ObjectFactory.GetType<Enterprise.Integration.DocumentWrappers.IDocContactMasterPasswordInstructionEmail>(),
						(NoResString)"(*CurrentCompanyName*) Password Set Complete",
						MasterPasswordSetSuccessfullyEmailTemplateDefaultBody);
					return registryItem;
				});
			}
		}

		ZString MasterPasswordSetSuccessfullyEmailTemplateDefaultBody => (NoResString)@"<font face=""Arial"">
	<div>
		<strong><br></strong>
	</div>
	<div>
		<strong>(*CurrentCompanyName*) Password Set Complete</strong>
	</div>
	<div>
		<br>
	</div>
	<div>
		<font face=""Arial"" size=""2""/>
		<font face=""Arial"" size=""2"">Hi (*ContactName*),<br></font>
	</div>
	<div>
		<br>
			<font face=""Arial"" size=""2""/>
			<font face=""Arial"" size=""2"">As you requested, your password has successfully been set and you can now safely login to your account. Please record your password in a secure location for future reference. <br></font>
	</div>
	<div>
		<font face=""Arial"" size=""2"">
			<Br>
			<font face=""Arial"" size=""2"">
				<strong>Important</strong>:
				<div>
					<font size=""2"">This email is a system generated email, initiated by a password set completion action.</font>
				</div>
				<div>
					<font size=""2"">If you are not expecting a password set completion action, please contact (*CurrentCompanyName*) immediately.</font>
				</div>
			</font>
		</font>
	</div>
</font>";

		#endregion

		#region LinerAndAgency

		#region Booking

		public BooleanRegistryItem UseWebLinerAndAgencyBookingsModule
		{
			get { return GetUseModuleRegistryItem(ResString.GetMultilingualString("dafdc909-05d8-4933-9c48-10c1ce97fcaa", "Liner & Agency Bookings"), LinerAndAgencyBookingsCategory, WebModuleRegistry.LinerAndAgencyBookings, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		public FilterLayoutCodePairRegistryItem DefaultFilterLayoutLinerAndAgencyBookings
		{
			get { return GetDefaultFilterLayoutRegistryItem(ResString.GetMultilingualString("ce416753-e0b4-4a9b-9aa3-63c4c724ce8d", "Liner & Agency Bookings"), LinerAndAgencyBookingsCategory, "DefaultFilterLayoutShippingBookings", "ShippingBookings", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.NotCached); }
		}

		public CodePairRegistryItem LinerAndAgencyBookingNotificationOptions
		{
			get { return GetNotificationOptionsRegistryItem(ResString.GetMultilingualString("57f4a6f4-9156-4940-aaa4-6d793627d289", "Liner & Agency Bookings"), LinerAndAgencyBookingsCategory, "ShippingBookingNotificationOptions", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		public GuidRegistryItem LinerAndAgencyBookingNotificationEmailGroup
		{
			get { return GetNotificationGroupRegistryItem(ResString.GetMultilingualString("ac3580b6-9b84-4f61-a0d1-f27b0ded636a", "Liner & Agency Bookings"), LinerAndAgencyBookingsCategory, "ShippingBookingNotificationEmailGroup", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue); }
		}

		public CodeDescriptionBoolRegistryItem LinerAndAgencyBookingNotificationStaffRoles
		{
			get { return GetStaffRolesRegistryItem(ResString.GetMultilingualString("3635e8d4-cdd8-437c-9d60-03344cce7016", "Liner & Agency Bookings"), LinerAndAgencyBookingsCategory, "ShippingBookingNotificationStaffRoles", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, StaffAssignmentRoles.Codes.SalesRep); }
		}

		#endregion

		#region BillOfLading

		public BooleanRegistryItem UseWebLinerAndAgencyBillsOfLadingModule
		{
			get { return GetUseModuleRegistryItem(ResString.GetMultilingualString("a3db5bed-8c65-4d84-ba41-802ab5ba5d7a", "Liner & Agency Bills of Lading"), LinerAndAgencyBillsOfLadingCategory, WebModuleRegistry.BillsOfLading, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		public FilterLayoutCodePairRegistryItem DefaultFilterLayoutLinerAndAgencyBillsOfLading
		{
			get { return GetDefaultFilterLayoutRegistryItem(ResString.GetMultilingualString("084e8d23-f87d-497f-8ddf-479ba67d3f49", "Liner & Agency Bills of Lading"), LinerAndAgencyBillsOfLadingCategory, "DefaultFilterLayoutShippingBillsOfLading", "ShippingBillsOfLading", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.NotCached); }
		}

		public CodePairRegistryItem LinerAndAgencyBillOfLadingNotificationOptions
		{
			get { return GetNotificationOptionsRegistryItem(ResString.GetMultilingualString("5b683082-a886-42dc-86af-e50f310f845c", "Liner & Agency Forwarding Instructions"), LinerAndAgencyBillsOfLadingCategory, "ShippingBillOfLadingNotificationOptions", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		public GuidRegistryItem LinerAndAgencyBillOfLadingNotificationEmailGroup
		{
			get { return GetNotificationGroupRegistryItem(ResString.GetMultilingualString("6d217953-3665-4801-8f10-5821bd5c3d80", "Liner & Agency Forwarding Instructions"), LinerAndAgencyBillsOfLadingCategory, "ShippingBillOfLadingNotificationEmailGroup", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue); }
		}

		public CodeDescriptionBoolRegistryItem LinerAndAgencyBillOfLadingNotificationStaffRoles
		{
			get { return GetStaffRolesRegistryItem(ResString.GetMultilingualString("b9166026-c2a6-4e54-ad6f-5a454dcb1bcd", "Liner & Agency Forwarding Instructions"), LinerAndAgencyBillsOfLadingCategory, "ShippingBillOfLadingNotificationStaffRoles", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, StaffAssignmentRoles.Codes.SalesRep); }
		}

		#endregion

		#endregion

		#region Web Component URLs

		public StringRegistryItem WebCFSUrl
		{
			get
			{
				return Get("WebCFSUrl", name => new StringRegistryItem(name,
					ComponentsURLsCategory,
					ResString.GetMultilingualString("DC7CA5AF-0FA5-4107-984D-7AEFECE638E9", "WebCFS URL"),
					GetWebUrlHint(ResString.GetMultilingualString("87BAC32F-232C-4A0C-AA42-14908BCAB6A8", "WebCFS Site")),
					RegistryStorageFlags.Company,
					RegistryOptions.PreserveTestValue)
				{
					DataType = new WebsiteRegistryDataType()
				});
			}
		}

		public StringRegistryItem WebTrackerUrl
		{
			get
			{
				return Get("WebTrackerUrl", name => new StringRegistryItem(name,
					ComponentsURLsCategory,
					ResString.GetMultilingualString("3de99c37-c2a1-4491-80c5-b151979918d2", "WebTracker URL"),
					GetWebUrlHint(ResString.GetMultilingualString("5c48b22b-7165-4318-b8db-feb2bf357fd7", "WebTracker Site")),
					RegistryStorageFlags.Company,
					RegistryOptions.PreserveTestValue)
				{
					DataType = new WebsiteRegistryDataType()
				});
			}
		}

		public StringRegistryItem WebCampaignUrl
		{
			get
			{
				return Get("WebCampaignUrl", name => new StringRegistryItem(name,
					ComponentsURLsCategory,
					ResString.GetMultilingualString("e038dca9-72f5-4376-af14-d814a900c509", "WebCampaign URL"),
					GetWebUrlHint(ResString.GetMultilingualString("d5b3e48d-4833-4cdb-8295-04b68340e93d", "Web Voting / Exam / Survey Campaign Site")),
					RegistryStorageFlags.Company,
					RegistryOptions.PreserveTestValue | RegistryOptions.CannotCallParameterlessValueGetter)
				{
					DataType = new WebsiteRegistryDataType()
				});
			}
		}

		public StringRegistryItem WebCertificationUrl
		{
			get
			{
				return Get("WebCertificationUrl", name => new StringRegistryItem(name,
					ComponentsURLsCategory,
					ResString.GetMultilingualString("b0656121-ecd2-4788-ad87-3a5be15ba150", "WebLearningCentre URL"),
					GetWebUrlHint(ResString.GetMultilingualString("f2930c43-56b9-47fb-8666-6afdc9b07356", "WebLearningCentre Site")),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport)
				{
					DataType = new WebsiteRegistryDataType()
				});
			}
		}

		#region SuppressResourceStringsCheckRegion

		public StringRegistryItem CargoWiseUserPortalUrl
		{
			get
			{
				return Get("CargoWiseUserPortalUrl", name => new StringRegistryItem(name,
					ComponentsURLsCategory,
					(NoResString)"CargoWise User Portal URL",
					(NoResString)"Specify the root URL for CargoWise User Portal Site",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForCargoWise | RegistryOptions.IsOnlyForSupport,
					"https://myaccount-portal.cargowise.com/myaccount")
				{
					DataType = new WebsiteRegistryDataType()
				});
			}
		}

		#endregion

		#endregion

		#region Trusted Messaging

		public BooleanRegistryItem EnableTrustedMessaging
		{
			get
			{
				return Get("EnableTrustedMessaging", name => new BooleanRegistryItem
				(
					name,
					TrustedMessagingCategory,
					(NoResString)"Enable Trusted Messaging",
					 (NoResString)"Enable Trusted Messaging between Central System and Client System",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					true)
				);
			}
		}

		public BinaryRegistryItem TrustedMessagingCentralSystemCertificate
		{
			get
			{
				return Get("TrustedMessagingCentralSystemCertificate", name => new BinaryRegistryItem
				(
					name,
					TrustedMessagingCategory,
					(NoResString)"Central System Certificate",
					(NoResString)"Specify the Central System Certificate file",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					Array.Empty<byte>())
				{
					EditorInfo = new FileUpLoaderX509CertificateRegistryEditorInfo(),
				}
				);
			}
		}

		public BinaryRegistryItem TrustedMessagingClientSystemCertificate
		{
			get
			{
				return Get("TrustedMessagingClientSystemCertificate", name => new BinaryRegistryItem
				(
					name,
					TrustedMessagingCategory,
					(NoResString)"Client System Certificate",
					(NoResString)"Specify the Client System Certificate file",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					Array.Empty<byte>())
				{
					EditorInfo = new FileUpLoaderX509CertificateRegistryEditorInfo(TrustedMessagingClientSystemCertificatePassword),
				}
				);
			}
		}

		public StringRegistryItem TrustedMessagingClientSystemCertificatePassword
		{
			get
			{
				return Get("TrustedMessagingClientSystemCertificatePassword", name => new StringRegistryItem
				(
					name,
					TrustedMessagingCategory,
					(NoResString)"Client System Certificate Password",
					(NoResString)"Specify the Password for Client System Certificate",
					new StringRegistryDataType(true),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport)
				{
					EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password),
				}
				);
			}
		}

		public StringRegistryItem TrustedMessagingSecretKey
		{
			get
			{
				return Get("TrustedMessagingSecretKey", name => new StringRegistryItem
				(
					name,
					TrustedMessagingCategory,
					(NoResString)"Trusted Messaging Secret Key",
					(NoResString)"Trusted Messaging Secret Key",
					RegistryStorageFlags.System,
					RegistryOptions.IsHidden | RegistryOptions.NotCached)
				);
			}
		}

		public StringRegistryItem FeatureControlRuleContent
		{
			get
			{
				return Get(FeatureControlRegistryKeys.FeatureControlRuleContentKey, name => new StringRegistryItem
				(
					name,
					TrustedMessagingCategory,
					(NoResString)"Feature Control Rule Content",
					(NoResString)"Feature Control Rule Content",
					new FeatureControlRegistryDataType(isEncrypted: true),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport | RegistryOptions.NotCached) { EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo) });
			}
		}

		#endregion

		#region Warehouse

		#region Inventory

		public BooleanRegistryItem UseWebWarehouseInventoryModule
		{
			get { return GetUseModuleRegistryItem(ResString.GetMultilingualString("d3ad240b-e88a-4197-b37a-cb63b4b2c43f", "Warehouse Inventory"), WarehouseInventoryCategory, WebModuleRegistry.WarehouseInventory, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, true); }
		}

		public FilterLayoutCodePairRegistryItem DefaultFilterLayoutWarehouseInventory
		{
			get { return GetDefaultFilterLayoutRegistryItem(ResString.GetMultilingualString("f6df9a80-d2da-4d6b-b396-10227926eacc", "Warehouse Inventory"), WarehouseInventoryCategory, "DefaultFilterLayoutWarehouseInventory", "TrackingInventory", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.NotCached); }
		}

		internal BooleanRegistryItem ShowDetailedInventory
		{
			get
			{
				return Get("ShowDetailedInventory", name => new BooleanRegistryItem(name,
							null,
							null,
							null,
							RegistryStorageFlags.CompanyDepartment,
							RegistryOptions.IsHidden | RegistryOptions.NotCached,
							true));
			}
		}

		public bool GetShowDetailedInventory(ZGuid webUserPK)
		{
			return ShowDetailedInventory.GetValueWithoutFallback(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, webUserPK.ToGuid());
		}

		public void SetShowDetailedInventory(ZGuid webUserPK, bool show)
		{
			ShowDetailedInventory.SetValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, webUserPK.ToGuid(), show);
		}

		public DecimalRegistryItem WarehouseInventoryDetailsPageSize
		{
			get
			{
				return Get("WarehouseInventoryDetailsPageSize", name => new DecimalRegistryItem(name,
					WarehouseInventoryCategory,
					ResString.GetMultilingualString("ebc5e08c-2824-4557-aa26-fd0db694b050", "Inventory Details Page Size"),
					HintForWarehouseInventoryDetailsPageSize,
					new NumericRegistryEditorInfo(decimalPlaces: 0),
					RegistryStorageFlags.System,
					DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
					defaultValue: 50m,
					lowerBound: 0,
					upperBound: 99));
			}
		}

		#endregion

		#region Orders

		internal BooleanRegistryItem ShowDetailedOrderLines
		{
			get
			{
				return Get("ShowDetailedOrderLines", name => new BooleanRegistryItem(name,
										null,
										null,
										null,
										RegistryStorageFlags.CompanyDepartment,
										RegistryOptions.IsHidden | RegistryOptions.NotCached,
										true));
			}
		}

		public bool GetShowDetailedOrderLines(ZGuid webUserPK)
		{
			return ShowDetailedOrderLines.GetValueWithoutFallback(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, webUserPK.ToGuid());
		}

		public void SetShowDetailedOrderLines(ZGuid webUserPK, bool show)
		{
			ShowDetailedOrderLines.SetValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, webUserPK.ToGuid(), show);
		}

		public CodePairRegistryItem WarehouseOrdersNotificationOptions
		{
			get { return GetNotificationOptionsRegistryItem(ResString.GetMultilingualString("7b143b78-645a-4876-ac90-f5ad78478823", "Warehouse Orders"), WarehouseOrdersCategory, "WarehouseOrdersNotificationOptions", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		public GuidRegistryItem WarehouseOrdersNotificationEmailGroup
		{
			get { return GetNotificationGroupRegistryItem(ResString.GetMultilingualString("12e8e8c2-3bca-4ac2-8ebd-9f36adb119a7", "Warehouse Orders"), WarehouseOrdersCategory, "WarehouseOrdersNotificationEmailGroup", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue); }
		}

		public CodeDescriptionBoolRegistryItem WarehouseOrdersNotificationStaffRoles
		{
			get { return GetStaffRolesRegistryItem(ResString.GetMultilingualString("98c09207-e83f-43a0-9245-72dc8bb50b3f", "Warehouse Orders"), WarehouseOrdersCategory, "WarehouseOrdersNotificationStaffRoles", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, StaffAssignmentRoles.Codes.CartageCoordinator); }
		}

		public BooleanRegistryItem UseWebWarehouseOrdersModule
		{
			get { return GetUseModuleRegistryItem(ResString.GetMultilingualString("a032871e-4300-4f6d-a172-157172bd0f4a", "Warehouse Orders"), WarehouseOrdersCategory, WebModuleRegistry.WarehouseOrders, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, true); }
		}

		public FilterLayoutCodePairRegistryItem DefaultFilterLayoutWarehouseOrders
		{
			get { return GetDefaultFilterLayoutRegistryItem(ResString.GetMultilingualString("582cdcb1-3280-431a-86f9-6dea6392387d", "Warehouse Orders"), WarehouseOrdersCategory, "DefaultFilterLayoutWarehouseOrders", "TrackingWarehouseOrders", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.NotCached); }
		}

		#endregion

		#region Products

		public CodePairRegistryItem WebWarehouseProductsNotificationOptions
		{
			get { return GetNotificationOptionsRegistryItem(ResString.GetMultilingualString("7629c844-cb5a-49b9-a87c-8825aaea3e98", "Warehouse Products"), WarehouseProductsCategory, "WebWarehouseProductsNotificationOptions", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		public GuidRegistryItem WebWarehouseProductsNotificationEmailGroup
		{
			get { return GetNotificationGroupRegistryItem(ResString.GetMultilingualString("7629c844-cb5a-49b9-a87c-8825aaea3e98", "Warehouse Products"), WarehouseProductsCategory, "WebWarehouseProductsNotificationEmailGroup", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue); }
		}

		public CodeDescriptionBoolRegistryItem WebWarehouseProductsNotificationStaffRoles
		{
			get { return GetStaffRolesRegistryItem(ResString.GetMultilingualString("7629c844-cb5a-49b9-a87c-8825aaea3e98", "Warehouse Products"), WarehouseProductsCategory, "WebWarehouseProductsNotificationStaffRoles", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, StaffAssignmentRoles.Codes.SalesRep); }
		}

		public BooleanRegistryItem UseWebWarehouseProductsModule
		{
			get { return GetUseModuleRegistryItem(ResString.GetMultilingualString("7629c844-cb5a-49b9-a87c-8825aaea3e98", "Warehouse Products"), WarehouseProductsCategory, WebModuleRegistry.WarehouseProducts, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, true); }
		}

		public FilterLayoutCodePairRegistryItem DefaultFilterLayoutWarehouseProducts
		{
			get { return GetDefaultFilterLayoutRegistryItem(ResString.GetMultilingualString("851ef165-711f-4eaa-8e9f-f763956fa53a", "Warehouse Products"), WarehouseProductsCategory, "DefaultFilterLayoutWarehouseProducts", "OrgSupplierPartWeb", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.NotCached); }
		}

		#endregion

		#region Receipts

		public CodePairRegistryItem WarehouseReceiptsNotificationOptions
		{
			get { return GetNotificationOptionsRegistryItem(ResString.GetMultilingualString("53263497-e845-4e6c-9597-5d0ab35c9e58", "Warehouse Receipts"), WarehouseReceiptsCategory, "WarehouseReceiptsNotificationOptions", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default); }
		}

		public GuidRegistryItem WarehouseReceiptsNotificationEmailGroup
		{
			get { return GetNotificationGroupRegistryItem(ResString.GetMultilingualString("c1a4863c-7a4c-4e54-8db2-3061cdba8d0e", "Warehouse Receipts"), WarehouseReceiptsCategory, "WarehouseReceiptsNotificationEmailGroup", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue); }
		}

		public CodeDescriptionBoolRegistryItem WarehouseReceiptsNotificationStaffRoles
		{
			get { return GetStaffRolesRegistryItem(ResString.GetMultilingualString("58edbaa7-e3a4-4ee1-b204-fa3d80845565", "Warehouse Receipts"), WarehouseReceiptsCategory, "WarehouseReceiptsNotificationStaffRoles", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, StaffAssignmentRoles.Codes.CartageCoordinator); }
		}

		public BooleanRegistryItem UseWebWarehouseReceiptsModule
		{
			get { return GetUseModuleRegistryItem(ResString.GetMultilingualString("62bbba10-1351-4b36-b01b-4aa69ffbf529", "Warehouse Receipts"), WarehouseReceiptsCategory, WebModuleRegistry.WarehouseReceipts, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, true); }
		}

		public FilterLayoutCodePairRegistryItem DefaultFilterLayoutWarehouseReceipts
		{
			get { return GetDefaultFilterLayoutRegistryItem(ResString.GetMultilingualString("ab75b9dc-6865-4fed-9c5c-8809d331a481", "Warehouse Receipts"), WarehouseReceiptsCategory, "DefaultFilterLayoutWarehouseReceipts", "TrackingWarehouseReceive", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.NotCached); }
		}

		#endregion

		#region Warehouses

		public FilterLayoutCodePairRegistryItem DefaultFilterLayoutWarehouses
		{
			get { return GetDefaultFilterLayoutRegistryItem(ResString.GetMultilingualString("16631a50-63e0-4c94-9bf2-bad92c550c79", "Warehouses"), WarehousesCategory, "DefaultFilterLayoutWarehouses", "TrackingWarehouse", DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.NotCached); }
		}

		#endregion

		#endregion

		#region Web Services

		public StringRegistryItem RootServicesUri
		{
			get
			{
				return GetItem(SharedGlowRegistry.EnterpriseServicesRootUriKey, delegate
				{
					return new StringRegistryItem(
						SharedGlowRegistry.EnterpriseServicesRootUriKey,
						WebServicesCategory,
						ResString.GetMultilingualString("cdcf9ee7-26b6-4f41-8e6b-e85cf7dcccee", "{0} Services Root URL", Core.Constants.ProductName),
						ResString.GetMultilingualString("069e8938-5183-45fb-a12f-b555621c9a39", "The URL to the place where {0} Services are installed.", Core.Constants.ProductName),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.PreserveTestValue,
						defaultValue: GlowRegistry.GetRootGlowUriDefaultValue(CargoWise.Definitions.GlowUriType.Service, (NoResString)"Services"))
					{
						DataType = new UriRegistryDataType(Uri.UriSchemeHttps) { AllowAutoProtocolPrefixing = false },
					};
				});
			}
		}

		public StringRegistryItem WebServiceUsername
		{
			get
			{
				return Get("WebServiceUsername", name => new StringRegistryItem(name,
																				WebServicesCategory,
																				ResString.GetMultilingualString("60bb7a7c-98b3-4925-ad8b-6edc6f642ba7", "Web Service User Login"),
																				ResString.GetMultilingualString("3f60e6d2-125a-4d34-b70f-ba9ff5b7a0d9", "Enter the username that will be used when accessing web services."),
																				RegistryStorageFlags.System,
																				RegistryOptions.PreserveTestValue,
																				string.Empty));
			}
		}

		public StringRegistryItem WebServicePassword
		{
			get
			{
				return Get("WebServicePassword", name => new StringRegistryItem(name,
																				WebServicesCategory,
																				ResString.GetMultilingualString("d376ade9-4fc4-4145-8ed9-13650c762d4c", "Web Service User Password"),
																				ResString.GetMultilingualString("19e68264-937a-4fee-a9f3-3e965aa87f38", "Enter the password that will be used when accessing web services."),
																				RegistryStorageFlags.System,
																				RegistryOptions.PreserveTestValue | RegistryOptions.IsPasswordVisibleForControllerUser,
																				string.Empty)
				{
					EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password)
				});
			}
		}

		public CodeDescriptionPairListRegistryItem WebServiceAlternativeCredentials
		{
			get
			{
				return Get("WebServiceAlternativeCredentials",
					name =>
					{
						var registryItem = new CodeDescriptionPairListRegistryItem(
							name,
							WebServicesCategory,
							ResString.GetMultilingualString("83a54ef6-181e-49bd-8508-39e2c3db76c7", "Web Service Alternative Credentials"),
							ResString.GetMultilingualString("e37e0181-b465-4074-9e96-eb1f061e327b",
								"List of alternative credentials to access supported web services.\r\nCurrently supported web services: Remote Printing.\r\nAfter editing credentials, you need to restart web server(s), or wait for period of time specified in registry item 'Web Service Credentials Cache Time'."),
							50,
							new LoginPasswordPairListEditorInfo(),
							RegistryStorageFlags.System,
							false, // Not localizable registry item - logins and passwords should not be translated to different languages
							RegistryOptions.PreserveTestValue,
							new ReadOnlyCodeDescriptionPairList(),
							false
						);

						((CodeDescriptionPairListRegistryDataType)registryItem.DataType).AllowEmptyCodes = false;
						((CodeDescriptionPairListRegistryDataType)registryItem.DataType).AllowDuplicateCodes = false;
						((CodeDescriptionPairListRegistryDataType)registryItem.DataType).AllowEmptyDescriptions = false;
						((CodeDescriptionPairListRegistryDataType)registryItem.DataType).AllowDuplicateDescriptions = true;
						((CodeDescriptionPairListRegistryDataType)registryItem.DataType).IsEmptyListAllowed = true;

						return registryItem;
					});
			}
		}

		public IntRegistryItem WebServiceCredentialsCacheTime
		{
			get
			{
				return Get("WebServiceCredentialsCacheTime",
					name => new IntRegistryItem(
						name,
						WebServicesCategory,
						ResString.GetMultilingualString("c568037e-e3ee-492e-9c7b-9912dd204c0c", "Web Service Credentials Cache Time"),
						ResString.GetMultilingualString("80723e27-2812-43f9-acf1-c632b4eba000",
							"Time in minutes, web service credentials will be cached on web server before expiring and reloading from database.\r\n\r\nAccepted values are from 0 to 1440 minutes (24 hours), 0 means no time limit."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						15,
						0,
						1440));
			}
		}

		public ChargeCodeListRegistryItem WebServiceChargeCodes
		{
			get
			{
				return Get("WebServiceChargeCodes", name => new ChargeCodeListRegistryItem(name,
																							 WebServicesCategory,
																							 ResString.GetMultilingualString("a8c287ba-aff7-45e2-b205-ecafac75a954", "Charge Codes"),
																							 ResString.GetMultilingualString("e1cf8bba-641c-4581-aa89-83ebe9c562fa", "Transactions with Charge Codes defined in this Registry will be excluded from Web Service Data Export."),
																							 string.Empty,
																							 RegistryFindBoxFilter.MrgDsbOrMjaChargeCode)
				{
					Options = RegistryOptions.PreserveTestValue
				});
			}
		}

		public Guid[] GetWebServiceChargeGuids(Guid companyPK)
		{
			string codes = WebServiceChargeCodes.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
			string[] sGuids = codes.Split(',');
			List<Guid> result = new List<Guid>();
			foreach (string sGuid in sGuids)
			{
				try
				{
					Guid guid = new Guid(sGuid);
					if (guid != Guid.Empty)
					{
						result.Add(guid);
					}
				}
				catch (ArgumentNullException) { }
				catch (FormatException) { }
			}

			return result.ToArray();
		}

		#endregion

		#region Additional Web Service Uris

		public StringRegistryItem AdditionalRootServicesUris
		{
			get
			{
				return GetItem("AdditionalRootServicesUris", delegate
				{
					return new StringRegistryItem(
						"AdditionalRootServicesUris",
						WebServicesCategory,
						ResString.GetMultilingualString("a4bf5b6f-734c-48d5-9fcb-aca0cd1c3831", "Additional {0} Services Root URLs", Constants.ProductName),
						ResString.GetMultilingualString("24ae4ef9-0bed-43c4-822f-ac0860221fa8", "Line separated additional URLs to the place where {0} Services are installed.", Constants.ProductName),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.PreserveTestValue,
						defaultValue: string.Empty)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo)
					};
				});
			}
		}

		#endregion

		#region Web Site Integration

		public StringRegistryItem CustomLoginPageURL
		{
			get
			{
				return Get("CustomLoginPageURL", name => new StringRegistryItem(name,
																				WebSiteIntegrationCategory,
																				ResString.GetMultilingualString("a02ab98a-43d2-4eda-bd3c-96e93f94150f", "Custom Login Page URL"),
																				HintForCustomLoginPageURL,
																				RegistryStorageFlags.System,
																				ZString.Empty));
			}
		}

		public StringRegistryItem CustomQuickViewPageURL
		{
			get
			{
				return Get("CustomQuickViewPageURL", name => new StringRegistryItem(name,
																							WebSiteIntegrationCategory,
																							ResString.GetMultilingualString("144799a7-776a-462d-a6a4-7e3f87ab29d9", "Custom Quick View Page URL"),
																							GintForCustomQuickViewPageURL,
																							RegistryStorageFlags.System,
																							ZString.Empty));
			}
		}

		public StringRegistryItem WebTrackerSharedSecret
		{
			get
			{
				return Get("WebTrackerSharedSecret", name => new StringRegistryItem(name,
																					WebSiteIntegrationCategory,
																					ResString.GetMultilingualString("4866dd24-4f9e-48e1-85cb-760b8cf533a7", "Shared Secret"),
																					HintForWebTrackerSharedSecret,
																					RegistryStorageFlags.System));
			}
		}

		#endregion

		#region Performance and Appearance

		public IntRegistryItem PageSize
		{
			get
			{
				return Get("PageSize", name => new IntRegistryItem(name,
																	 PerformanceAndAppearanceCategory,
																	 ResString.GetMultilingualString("cf5f9a51-d7fa-45ed-92d4-1ebcc0683870", "Page Size for Search Screen"),
																	 HintForPageSize,
																	 RegistryStorageFlags.System,
																	 RegistryOptions.PreserveTestValue,
																	 25)
				{ DataType = new IntRegistryDataType(1, Int32.MaxValue) });
			}
		}

		public IntRegistryItem MaxFilteredRecords
		{
			get
			{
				return Get("MaxFilteredRecords", name => new IntRegistryItem(name,
																			 PerformanceAndAppearanceCategory,
																			 ResString.GetMultilingualString("e2c5f21c-31b5-4f93-b014-9695a81be271", "Max No. of Records to Show"),
																			 HintForMaxFilteredRecords,
																			 RegistryStorageFlags.System,
																			 RegistryOptions.PreserveTestValue,
																			 1000)
				{ DataType = new IntRegistryDataType(0, Int32.MaxValue) });
			}
		}

		public IntRegistryItem MaxFilteredRecordsForExportToExcel
		{
			get
			{
				return Get("MaxFilteredRecordsForExportToExcel", name => new IntRegistryItem(name,
																							 PerformanceAndAppearanceCategory,
																							 ResString.GetMultilingualString("a5d78bbd-8955-492f-985b-38a2d5050554", "Max No. of Records to Export to Excel"),
																							 HintForMaxFilteredRecordsForExportToExcel,
																							 RegistryStorageFlags.System,
																							 10000));
			}
		}

		public IntRegistryItem RequestTimeout
		{
			get
			{
				return Get("RequestTimeout", name => new IntRegistryItem(name,
																		 PerformanceAndAppearanceCategory,
																		 ResString.GetMultilingualString("6269a48c-215b-4df6-9bf1-6fddb38ed39f", "Request Timeout"),
																		 HintForRequestTimeout,
																		 RegistryStorageFlags.System,
																		 300));
			}
		}

		#region SuppressResourceStringsCheckRegion

		public BooleanRegistryItem WebActivityLogging
		{
			get
			{
				return Get("WebActivityLogging", name => new BooleanRegistryItem(name,
																				 PerformanceAndAppearanceCategory,
																				 ResString.GetMultilingualString("a916cda8-6aed-47a9-be97-65746c558a31", "Web Activity Logging"),
																				 ResString.GetMultilingualString("661737cf-50e3-4542-baa6-16be689022e0", "Set this to 'Yes' to activate logging user actions in WebTracker."),
																				 RegistryStorageFlags.System,
																				 false));
			}
		}

		public BooleanRegistryItem AllowInlineFrames
		{
			get
			{
				return Get("AllowInlineFrames", name => new BooleanRegistryItem(name,
																				 PerformanceAndAppearanceCategory,
																				 ResString.GetMultilingualString("a0dbc750-0e70-4c10-9d38-76fc04121361", "Allow In-Line Frames"),
																				 ResString.GetMultilingualString("d2a0f219-395f-44b7-80c8-22ecd10015f8", "Set this to 'Yes' to allow WebTracker pages to work with in-line frames."),
																				 RegistryStorageFlags.System,
																				 false));
			}
		}

		#endregion

		public BooleanRegistryItem AccessFromManagementGroupAndClientControlled
		{
			get
			{
				return Get("AccessFromManagementGroupAndClientControlled", name => new BooleanRegistryItem(name,
																				 LoginQuickViewCategory,
																				 ResString.GetMultilingualString("225b2aeb-d291-415f-b69c-166b2622a753", "Access for Client Controlled By/Management Group"),
																				 ResString.GetMultilingualString("72ac1ae3-4fe6-457e-ae4b-cc314f50d4b7", "Set this to ‘Yes’ to allow organizations nominated as ‘Client Controlled By’ and ‘Management Group’ to access information associated with their child organizations"),
																				 RegistryStorageFlags.System,
																				 false));
			}
		}

		public CodePairRegistryItem DateFormat
		{
			get
			{
				return Get("DateFormat", name => new CodePairRegistryItem(name,
					PerformanceAndAppearanceCategory,
					ResString.GetMultilingualString("d5d01ee6-d143-41ed-a302-01607e25179c", "Date Format"),
					HintForDateFormat,
					new CodeDescriptionPairListProvider(() => new DateFormatCodeDescriptionPairList()),
					RegistryStorageFlags.System,
					"STD"));
			}
		}

		#region WEBCFS Theme

		public WebThemeRegistryItem WebCFSTheme
		{
			get
			{
				return Get("WebCFSTheme", name => new WebThemeRegistryItem(name,
					WebCFSUrls,
					WebCFSThemeCategory,
					ResString.GetMultilingualString("22D3FFDE-5899-4D50-899C-26F39660463C", "Theme"),
					HintForWebCFSTheme,
					RegistryStorageFlags.System,
					RegistryOptions.PreserveTestValue));
			}
		}

		public WebCustomCssRegistryItem WebCFSCustomCss
		{
			get
			{
				return Get("WebCFSCustomCss", name => new WebCustomCssRegistryItem(name,
					WebCFSUrls,
					WebCFSThemeCategory,
					ResString.GetMultilingualString("9D549799-0E20-447C-9201-B8D4C1604ECD", "Theme CSS"),
					HintForWebCFSCustomCss,
					RegistryStorageFlags.System,
					RegistryOptions.PreserveTestValue));
			}
		}

		public WebCustomImagesRegistryItem WebCFSCustomImages
		{
			get
			{
				return Get("WebCFSCustomImages", name => new WebCustomImagesRegistryItem(name,
					WebCFSUrls,
					WebCFSThemeCategory,
					ResString.GetMultilingualString("9f13c172-549f-4d0e-9ff7-add18c464600", "Theme Images"),
					HintForWebCFSCustomImages,
					RegistryStorageFlags.System,
					RegistryOptions.PreserveTestValue));
			}
		}

		public StringArrayRegistryItem WebCFSUrls
		{
			get
			{
				return Get("WebCFSUrls", name => new StringArrayRegistryItem(name,
					WebCFSThemeCategory,
					ResString.GetMultilingualString("d97d4e15-79d8-4d99-8324-394ad5b0e3a8", "URLs"),
					HintForWebCFSUrls,
					RegistryStorageFlags.System));
			}
		}

		#endregion

		#region WebCampaign Theme

		public WebThemeCustomObjectRegistryItem WebCampaignCustomTheme
		{
			get
			{
				var defaultValue = new WebThemeCustomObjectCollection();

				var themeObject = new WebThemeCustomObject();
				themeObject.ThemeName = WebThemeCustomObject.Schema.DefaultThemeName;
				themeObject.CSS = ObjectFactory.Get<IWebCSSImages>().WebStyleSheet;

				var imageList = ObjectFactory.Get<IWebCSSImages>().WebImages;
				var imageCol = new WebCustomThemeImageBusinessObjectCollection();
				foreach (var image in imageList)
				{
					var imageObject = new WebCustomThemeImageBusinessObject();
					imageObject.ImageName = image.Key;
					imageObject.Data = image.Value;
					imageCol.Add(imageObject);
				}

				themeObject.ImageCollection.AddRange(imageCol);
				defaultValue.Add(themeObject);

				return Get("WebCampaignCustomTheme", name => new WebThemeCustomObjectRegistryItem(name,
					WebCampaignThemeCategory,
					ResString.GetMultilingualString("eaaab070-ecf6-41d5-97d2-9b11c69bd0b6", "Theme"),
					HintForWebCampaignTheme,
					RegistryStorageFlags.System,
					RegistryOptions.PreserveTestValue,
					defaultValue));
			}
		}

		public WebThemeUrlRegistryItem WebCampaignCustomThemeUrl
		{
			get
			{
				var defaultValue = new WebThemeUrlCollection();
				defaultValue.Add(new WebThemeUrl()
				{
					ThemeName = WebThemeCustomObject.Schema.DefaultThemeName,
					Url = (NoResString)"localhost"
				});

				defaultValue.SuspendValidation();

				return Get("WebCampaignCustomThemeUrl", name => new WebThemeUrlRegistryItem(name,
					WebCampaignThemeCategory,
					ResString.GetMultilingualString("f8137e76-8c17-4362-b2dc-6f829214af85", "Theme URL"),
					HintForWebCampaignUrls,
					RegistryStorageFlags.System,
					defaultValue));
			}
		}

		#endregion

		#region WebTracker Theme

		public WebThemeRegistryItem WebTrackerTheme
		{
			get
			{
				return Get("WebTrackerTheme", name => new WebThemeRegistryItem(name,
					WebTrackerUrls,
					WebTrackerThemeCategory,
					ResString.GetMultilingualString("16b8494f-43c3-4b69-b05d-437da8d8dfde", "Theme"),
					HintForWebTrackerTheme,
					RegistryStorageFlags.System,
					RegistryOptions.PreserveTestValue));
			}
		}

		// TODO: Remove this in CWO, once all customers have upgraded to use the new registry item
		public CodePairRegistryItem OldTheme
		{
			get
			{
				return Get((NoResString)"Theme", name => new CodePairRegistryItem(name,
					PerformanceAndAppearanceCategory,
					ResString.GetMultilingualString("16b8494f-43c3-4b69-b05d-437da8d8dfde", "Theme"),
					HintForOldTheme,
					new CodeDescriptionPairListProvider(() => new ThemeCodeDescriptionPairList()),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForDevelopers,
					String.Empty));
			}
		}

		public WebCustomCssRegistryItem WebTrackerCustomCss
		{
			get
			{
				return Get("WebTrackerCustomCss", name => new WebCustomCssRegistryItem(name,
					WebTrackerUrls,
					WebTrackerThemeCategory,
					ResString.GetMultilingualString("c70bd9ed-c6b3-43e7-983c-c155276d2acb", "Theme CSS"),
					HintForWebTrackerCustomCss,
					RegistryStorageFlags.System,
					RegistryOptions.PreserveTestValue));
			}
		}

		public WebCustomImagesRegistryItem WebTrackerCustomImages
		{
			get
			{
				return Get("WebTrackerCustomImages", name => new WebCustomImagesRegistryItem(name,
					WebTrackerUrls,
					WebTrackerThemeCategory,
					ResString.GetMultilingualString("9f13c172-549f-4d0e-9ff7-add18c464600", "Theme Images"),
					HintForWebTrackerCustomImages,
					RegistryStorageFlags.System,
					RegistryOptions.PreserveTestValue));
			}
		}

		public StringArrayRegistryItem WebTrackerUrls
		{
			get
			{
				return Get("WebTrackerUrls", name => new StringArrayRegistryItem(name,
					WebTrackerThemeCategory,
					ResString.GetMultilingualString("d97d4e15-79d8-4d99-8324-394ad5b0e3a8", "URLs"),
					HintForWebTrackerUrls,
					RegistryStorageFlags.System,
					RegistryOptions.PreserveTestValue));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "Need too many refactoring")]
		public string GetWebTheme(WebThemeRegistryItem registryItem, string url)
		{
			return registryItem
				.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)
				.Where(x => IsMatchingWebUrl(x.Url, url) || String.IsNullOrEmpty(x.Url))
				.OrderBy(x => String.IsNullOrEmpty(x.Url))
				.Select(x => x.Code)
				.FirstOrDefault();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "Need too many refactoring")]
		public byte[] GetWebCustomImage(WebCustomImagesRegistryItem registryItem, string url, string imageName)
		{
			return registryItem
				.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)
				.Where(x => imageName.Equals(x.Name, StringComparison.OrdinalIgnoreCase) && (IsMatchingWebUrl(x.Url, url) || String.IsNullOrEmpty(x.Url)))
				.OrderBy(x => String.IsNullOrEmpty(x.Url))
				.Select(x => x.Data)
				.FirstOrDefault();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "Need too many refactoring")]
		public string GetWebCustomCss(WebCustomCssRegistryItem registryItem, string url)
		{
			return registryItem
				.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)
				.Where(x => IsMatchingWebUrl(x.Url, url) || String.IsNullOrEmpty(x.Url))
				.OrderBy(x => String.IsNullOrEmpty(x.Url))
				.Select(x => x.Data)
				.FirstOrDefault();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "Not a uri")]
		public static bool ContainsWebUrl(IEnumerable<string> webUrls, string url)
		{
			var cleanedUrls = webUrls.Select(x =>
			{
				if (Uri.TryCreate(x, UriKind.Absolute, out var uri))
				{
					return uri.Host;
				}

				return x?.TrimEnd('/');
			});

			return cleanedUrls.Contains(url?.TrimEnd('/'), StringComparer.OrdinalIgnoreCase);
		}

		bool IsMatchingWebUrl(string registryUrl, string url)
		{
			return ContainsWebUrl(new[] { registryUrl }, url);
		}

		#endregion

		#endregion

		#region Password Set and Reset Process

		public BooleanRegistryItem ChoiceOfPasswordSetAndResetProcessFlowEnabled
		{
			get
			{
				return GetItem("ChoiceOfPasswordSetAndResetProcessFlowEnabled", delegate
				{
					return new BooleanRegistryItem(
						"ChoiceOfPasswordSetAndResetProcessFlowEnabled",
						WebCategory,
						ResString.GetMultilingualString("952234b7-097e-4bab-b695-9e59db87dba9", "Enable Choice of Password Set/Reset Process Flow"),
						ResString.GetMultilingualString("b2a6ac53-9ed9-4331-a945-9cd9b82e3f2b", @"When the ""Send Password Instructions"" button on the Organization > Contact > Web Security form is pressed, the user will be offered a choice of sending the Contact user to either WebTracker or to CargoWise Web Portals in order to complete the set or reset of their password. Following a successful change of password, the Contact user will then be directed to WebTracker or the CargoWise Web Portal catalog for subsequent login."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		public CodePairRegistryItem DefaultCargoWiseWebPortal
		{
			get
			{
				return Get("DefaultCargoWiseWebPortal", name =>
					new CodePairRegistryItem(
						name,
						WebCategory,
						ResString.GetMultilingualString("2c3990a8-026e-4d2b-abc7-04c75435ddf3", "Default CargoWise Web Portal"),
						HintForDefaultCargoWiseWebPortal,
						new CodeDescriptionPairListProvider(() => new DefaultCargoWiseWebPortalCodeList()),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						DefaultCargoWiseWebPortalCodeList.Codes.Default));
			}
		}

		#endregion

		#region Helper Methods

		BooleanRegistryItem GetUseModuleRegistryItem(MultilingualString caption, MultilingualString category, string registryItemName, RegistryOptions options, bool defaultValue, bool isWarehouse = false)
		{
			return Get(registryItemName, name => new BooleanRegistryItem(name,
				category,
				ResString.GetMultilingualString("7997ef55-456c-4f26-85d8-22b8899f5184", "Show {0} tab", caption),
				isWarehouse ? GetHintForUseModuleRegistryItemsForWarehouse(caption) : GetHintForUseModuleRegistryItems(caption),
				RegistryStorageFlags.System,
				options,
				defaultValue));
		}

		BooleanRegistryItem GetUseModuleRegistryItem(MultilingualString caption, MultilingualString category, string registryItemName, RegistryOptions options = RegistryOptions.Default, bool isWarehouse = false)
		{
			return GetUseModuleRegistryItem(caption, category, registryItemName, options, true, isWarehouse);
		}

		FilterLayoutCodePairRegistryItem GetDefaultFilterLayoutRegistryItem(MultilingualString caption, MultilingualString category, string registryItemName, string moduleName, RegistryOptions options = RegistryOptions.NotCached)
		{
			return Get(registryItemName, name =>
			{
				return new FilterLayoutCodePairRegistryItem(name,
					category,
					ResString.GetMultilingualString("680628ee-65a2-4e95-b5a4-fee6e431c6cc", "Default Filter Layout"),
					GetHintForDefaultFilterLayoutRegistryItems(caption),
					true,
					true,
					new ComboBoxFilterLayoutRegistryEditorInfo(moduleName),
					RegistryStorageFlags.Company,
					options,
					(NoResString)"System Default Layout",
					false);
			});
		}

		public CodePairRegistryItem GetNotificationOptionsRegistryItem(MultilingualString caption, MultilingualString category, string registryItemName, RegistryOptions options)
		{
			return Get(registryItemName, name => new CodePairRegistryItem(name,
				category,
				ResString.GetMultilingualString("c3091dc4-bbfb-4f94-bb71-7461aaccc6ba", "Notification Options"),
				GetHintForNotificationOptions(caption),
				new CodeDescriptionPairListProvider(() => new SendingRuleCodeDescriptionPairList()),
				RegistryStorageFlags.System,
				options,
				EmailNotificationSendingRules.ALL));
		}

		GuidRegistryItem GetNotificationGroupRegistryItem(MultilingualString caption, MultilingualString category, string registryItemName, RegistryOptions options)
		{
			return Get(registryItemName, name => new GuidRegistryItem(name,
				category,
				ResString.GetMultilingualString("8b9a3dd5-e89c-490c-85aa-2bb78efa758f", "Notification Group"),
				GetHintForNotificationGroupRegistryItems(caption),
				new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				options,
				RegistryFactory.Instance.GetGroupPK("ALL")));
		}

		CodeDescriptionBoolDisallowNewRegistryItem GetStaffRolesRegistryItem(MultilingualString caption, MultilingualString category, string registryItemName, RegistryOptions options, params string[] defaultStaffAssignments)
		{
			CodeDescriptionBoolDisallowNewCollection roles = StaffRolesNotificationHelper.GetRoles();
			StaffRolesNotificationHelper.SetBoolTo(roles, true, defaultStaffAssignments);

			return Get(registryItemName, name => new CodeDescriptionBoolDisallowNewRegistryItem(name,
				category,
				ResString.GetMultilingualString("65d0b2e1-454b-4a05-8628-0eda9d1e427e", "Notification Staff Roles"),
				GetHintForStaffRoles(caption),
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				options,
				new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("004ccb75-bc1c-4462-b1f8-ea6d463fb0f9", "Send Notification To"), true, true),
				roles));
		}

		AccessControlRegistryItem GetOrgsRolePropertySuppressionRegistryItem(MultilingualString caption, MultilingualString category, string registryItemName, AccessRulesBase accessRules, RegistryOptions options)
		{
			return Get(registryItemName, name => new AccessControlRegistryItem(name,
				category,
				ResString.GetMultilingualString("aaab512d-c651-4332-b783-8838354d4fb4", "Access Control"),
				GetHintForOrgsRolePropertySuppressionRegistryItems(caption),
				accessRules,
				options));
		}

		CodeDescriptionBoolRegistryItem GetSuppressFlightDetailsRegistryItem(MultilingualString caption, string registryItemName, MultilingualString hint, RegistryOptions options = RegistryOptions.Default)
		{
			RegistryItemImplWithDynamicDefaultValue.DefaultValueGetter valueGetter = (companyPK, branchPK, departmentPK) =>
																																							 RegistrySuppressionHelper.GetCollection(null); // We suppress everuthing on the web -- even for US

			return Get(registryItemName, name => new CodeDescriptionBoolRegistryItem(name,
				SuppressFlightDetailsCategory,
				caption,
				RegistrySuppressionHelper.GetWebHint(hint),
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				options,
				new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("83438C29-B285-47FB-AB2D-417F49893024", "Suppress"), true, true),
				valueGetter));
		}

		#endregion

		#region IP Whitelist

		public InternetAddressListRegistryItem HealthCheckAccessIPWhitelistRegistryItem
		{
			get
			{
				return GetItem<InternetAddressListRegistryItem>("HealthCheckAccessIPWhitelist", delegate
				{
					var defaultRuleset = new InternetAddressRuleset();
					var localhostIPV4 = defaultRuleset.AddNew();
					localhostIPV4.Text = "127.0.0.1/8";
					localhostIPV4.Enabled = true;

					var localhostIPV6 = defaultRuleset.AddNew();
					localhostIPV6.Text = "::1";
					localhostIPV6.Enabled = true;

					var option = EnvProxy.IsHostedWithCargowise
						? RegistryOptions.IsOnlyEditableBySupportIfHosted
						: RegistryOptions.IsOnlyForController;

					return new InternetAddressListRegistryItem(
						"HealthCheckAccessIPWhitelist",
						WebCategory,
						ResString.GetMultilingualString("8ecefa89-e2e0-4ecd-855d-a873c3b88d03", "Health Check Access IP White list"),
						ResString.GetMultilingualString("9f8c9295-bc7d-40ed-87d5-bcb044a8812c", "Only the IP addresses in this list can access the health check page. This white list supports IP subnets, IP ranges and single addresses. e.g. 192.168.0.0/24 or 192.168.1.1-192.168.1.20 or 192.168.3.1 or 2001:0001::/64"),
						RegistryStorageFlags.System,
						option,
						defaultRuleset);
				});
			}
		}

		#endregion

		public BooleanRegistryItem ShowNewTrackingPortal
		{
			get
			{
				return Get(nameof(ShowNewTrackingPortal), name => new BooleanRegistryItem(name,
					WebCategory,
					(NoResString)"Show Link to the New Tracking Portal",
					(NoResString)"Set this to 'Yes' to display a link inviting WebTracker users to try the new tracking portal.",
					RegistryStorageFlags.System,
					RegistryOptions.IsHidden,
					false));
			}
		}

		public CodeDescriptionBoolDisallowNewRegistryItem WebTrackerPreloadModules
		{
			get
			{
				return Get(nameof(WebTrackerPreloadModules), name => new CodeDescriptionBoolDisallowNewRegistryItem(name,
					PerformanceAndAppearanceCategory,
					ResString.GetMultilingualString("B92902AB-4727-4646-8A50-F3D2FD2EA0DB", "WebTracker Modules to Pre-load"),
					ResString.GetMultilingualString("C4422E1A-E85E-4BAD-BA10-3E23E6CAFB73", "WebTracker modules that should be immediately loaded after an upgrade instead of on-demand."),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyEditableBySupportIfHosted,
					new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("DF66B47F-3F35-4AE4-A0C2-3FF825104DA6", "Pre-load"), true, true),
					DefaultPreloadModules));
			}
		}

		CodeDescriptionBoolDisallowNewCollection DefaultPreloadModules
		{
			get
			{
				var defaults = new CodeDescriptionBoolDisallowNewCollection(new WebTrackerPreloadModulesList());
				defaults.Set(WebTrackerPreloadModulesList.Codes.Shipments, true);

				return defaults;
			}
		}

		public IntRegistryItem WebLoginAttempts
		{
			get
			{
				return Get(nameof(WebLoginAttempts), name => new IntRegistryItem(
					name,
					PasswordControlCategory,
					ResString.GetMultilingualString("67fded73-9247-4d13-8774-a0d218fe4dcc", "Login Attempts"),
					ResString.GetMultilingualString("d9e749be-4157-4b72-9d5d-2c17db56b680", "Number of failed login attempts before web user is locked out. (0 = Do not lockout)."),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					0)
				{
					DataType = new IntRegistryDataType(0, Int32.MaxValue)
				});
			}
		}

		public IntRegistryItem WebLoginLockoutMinutes
		{
			get
			{
				return Get(nameof(WebLoginLockoutMinutes), name => new IntRegistryItem(
					name,
					PasswordControlCategory,
					ResString.GetMultilingualString("af892537-cf6f-4715-b149-a462ab77858f", "Lockout Minutes"),
					ResString.GetMultilingualString("9eb6588b-4e7f-48c1-b207-e6ec35c610e7", "Number of minutes to lockout web user after failed login attempts.(0 = Requires manual reset)"),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					15)
				{
					DataType = new IntRegistryDataType(0, Int32.MaxValue)
				});
			}
		}

		public IntRegistryItem WebPasswordMinLength
		{
			get
			{
				return Get(nameof(WebPasswordMinLength), name => new IntRegistryItem(
					name,
					PasswordControlCategory,
					ResString.GetMultilingualString("749074e8-ee24-4a14-b9f0-7565ff9eaff8", "Minimum Length"),
					ResString.GetMultilingualString("c2704b04-7f3c-492d-bbc3-960f7e7be68e", "Minimum length of password."),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					12)
				{
					DataType = new IntRegistryDataType(8, 15)
				});
			}
		}

		public IntRegistryItem WebPasswordHistoryCount
		{
			get
			{
				return Get(nameof(WebPasswordHistoryCount), name => new IntRegistryItem(
					name,
					PasswordControlCategory,
					ResString.GetMultilingualString("ec8740ab-037a-4159-a629-98839422af48", "History Count"),
					ResString.GetMultilingualString("f5098827-393f-4407-9eb5-6cf4b31ffb1d", "Number of previously used passwords to remember."),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					0)
				{
					DataType = new IntRegistryDataType(0, 15)
				});
			}
		}

		public BooleanRegistryItem EnableWebPasswordComplexityRules
		{
			get
			{
				return Get(nameof(EnableWebPasswordComplexityRules), name => new BooleanRegistryItem
				(
					name,
					PasswordControlCategory,
					ResString.GetMultilingualString("1c79f35a-5e7c-466f-9acd-b063e46f3fdf", "Enable Password Complexity Rules"),
					ResString.GetMultilingualString("77ae5603-c97b-49f6-8721-a873a6bf42d0", "The rules will be enforced when a user sets a new password. Password must contain at least three of the following: uppercase letters, lowercase letters, numbers, symbols, and non-European alphabet characters."),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					false)
				);
			}
		}

		public IntRegistryItem WebPasswordRotationDays
		{
			get
			{
				return Get(nameof(WebPasswordRotationDays), name => new IntRegistryItem(
					name,
					PasswordControlCategory,
					ResString.GetMultilingualString("f6925ce2-bbdf-4fc7-abe0-dd70c80b1f97", "Password Change Days"),
					ResString.GetMultilingualString("d97ff8f2-d5bc-4691-9d5c-1ec1a1c8fdfd", "Force password rotation every (n) days. Set to 0 to disable password rotation."),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					0)
				{
					DataType = new IntRegistryDataType(0, Int32.MaxValue)
				});
			}
		}

		public DateTimeRegistryItem WebPasswordRotationEffectiveDate
		{
			get
			{
				return Get(nameof(WebPasswordRotationEffectiveDate), name => new DateTimeRegistryItem(
						name,
						PasswordControlCategory,
						ResString.GetMultilingualString("61dbf509-57d1-4437-8896-0f6fb1ca3345", "Password Rotation Effective Date"),
						ResString.GetMultilingualString("141a5499-0381-4781-8bc4-15e8065716df", "Effective Date when the Password Rotation comes into effect. \r\nFor example, if a last password change date is known, that can be used to determine whether a password has expired. If a password has never been changed, this date is used as a fallback in the calculation to determine whether a password has expired."),
						RegistryStorageFlags.System));
			}
		}

		public StringRegistryItem MobileServicesUriRegistryItem
		{
			get
			{
				return GetItem("MobileServicesUri", delegate
				{
					var item = new StringRegistryItem(
						"MobileServicesUri",
						WebServicesCategory,
						ResString.GetMultilingualString("8e97d699-a168-4606-b7af-69eeb420cf60", "Mobile Services URL"),
						ResString.GetMultilingualString("0cf95f19-7670-4042-a25a-637b1f200102", "The URL to the WiseTech Global mobile services"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.PreserveTestValue,
						"https://mobile.cargowise.net/")
					{
						DataType = new UriRegistryDataType(Uri.UriSchemeHttps) { AllowAutoProtocolPrefixing = false }
					};
					return item;
				});
			}
		}

		#region Password Set/Reset Exclude List

		public JsonStringArrayRegistryItem PasswordBannedWordList
		{
			get
			{
				return GetItem("PasswordBannedWordList", delegate
				{
					return new JsonStringArrayRegistryItem(
						"PasswordBannedWordList",
						PasswordControlCategory,
						ResString.GetMultilingualString("63c7fe01-3b59-400b-b87f-03c7f61d86e1", "Password Banned Word List"),
						passwordBannedWordListDescription,
						RegistryStorageFlags.System,
						RegistryOptions.Default);
				});
			}
		}

		readonly ResourceString passwordBannedWordListDescription =
			ResString.GetMultilingualString("79d1db2b-ce0a-46b2-9e22-89952d5e0c10",
				"Use this registry item to store the list of words (or string values) that cannot be used by users when they set or reset passwords.\r\n" +
				"These values could be text contained within a password, for example, the word '{0}' in '{1}'.\r\n" +
				"Password checks against this list are not case sensitive. For example, '{2}', '{3}' or '{4}' will be assessed as the same word.",
				"not", "donotuse", "Word", "WoRd", "WORD");

		#endregion

		public BooleanRegistryItem EnableWebPasswordNameAndEmailValidation
		{
			get
			{
				return Get(nameof(EnableWebPasswordNameAndEmailValidation), name => new BooleanRegistryItem
				(
					name,
					PasswordControlCategory,
					(NoResString)"Enable Web Password Name And Email Validation",
					(NoResString)"When this registry item is enabled, the name and email address password policy rule will be enforced for web portal users when they set a new password.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					true)
				);
			}
		}

		#region Contact Redirection Expiry

		public IntRegistryItem ContactRedirectionExpiryDays
		{
			get
			{
				return GetItem(MyAccountRegistry.MyAccountContactRedirectionExpiryDays, () => new IntRegistryItem(
					MyAccountRegistry.MyAccountContactRedirectionExpiryDays,
					WebCategory,
					ResString.GetMultilingualString("f89cbb45-385e-42f4-bf4c-a51e1b2550fd", "Contact Redirection Expiry Days"),
					ResString.GetMultilingualString("4219bc71-c800-48d6-9e81-775baec7664e", "Set the number of days that a contact will be redirected with web access superseded before web access superseded expires."),
					new NumericRegistryEditorInfo(0),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					30, 1, 1000));
			}
		}

		#endregion

		#region LoginFailureAttemptSecretKey

		public StringRegistryItem LoginFailureAttemptSecretKey
		{
			get
			{
				return GetItem("LoginFailureAttemptSecretKey", delegate
				{
					var result = new StringRegistryItem(
						"LoginFailureAttemptSecretKey",
						WebCategory,
						ResString.GetMultilingualString("8f2b8ac6-7827-430f-a8d7-2893a5b9b0a3", "Login Failure Attempt Secret Key"),
						ResString.GetMultilingualString("3b960b1b-75c4-42e9-a37d-0b5d2c283f55", "Key used for hash of My Account and WebTracker login failure. The key should be 64 bytes long."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport
					);
					result.DataType = new BinaryKeyRegistryDataType(keySize: 64);
					return result;
				});
			}
		}

		#endregion

		#region Send Password Instructions

		public DateTimeRegistryItem ContactCreatedStartFromDate
		{
			get
			{
				return Get("ContactCreatedStartFromDate", name => new DateTimeRegistryItem(
						name,
						WebCategory,
						ResString.GetMultilingualString("26E148F0-7CCF-4F40-93AB-1E6734A7912B", "Send Password Instructions"),
						ResString.GetMultilingualString("CE211504-8583-48AE-B2E5-6DFB05D7377B", "Overriding this registry setting will enable the SPI Service Task. \r\n\r\nThe default value is No - Do not run the Service Task. \r\n\r\nIf overridden, the SPI service task will be enabled and run for Organization Contacts that fulfill all of the following conditions: \r\n- The Contact has been created since the specified Date \r\n- Password Instructions have never been sent to the Contact \r\n- The Contact has Web Access enabled \r\n- The Contact is Active."),
						RegistryStorageFlags.System));
			}
		}

		#endregion
	}
}
