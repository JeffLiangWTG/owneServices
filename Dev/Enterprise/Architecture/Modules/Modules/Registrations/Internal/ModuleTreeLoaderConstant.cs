using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Modules
{
	public static class ModuleTreeLoaderConstant
	{
		public class Entry
		{
			public Entry(string name, MultilingualString displayText, string letter)
			{
				this.Name = name;
				this.DisplayText = displayText;
				this.Letter = letter;
			}

			public Entry(string name, MultilingualString displayText)
				: this(name, displayText, null)
			{
			}

			public string Name { get; private set; }
			public MultilingualString DisplayText { get; private set; }
			public string Letter { get; private set; }
		}

		#region SuppressResourceStringsCheckRegion

		public static class Category
		{
			public static Entry Jump { get { return new Entry("Jump", ResString.GetMultilingualString("Category.Jump", "Jump")); } }
			public static Entry Operations { get { return new Entry("Operations", ResString.GetMultilingualString("Category.Operations", "Operate")); } }
			public static Entry Manage { get { return new Entry("Manage", ResString.GetMultilingualString("Category.Manage", "Manage")); } }
			public static Entry Admin { get { return new Entry("Admin", ResString.GetMultilingualString("Category.Admin", "Maintain")); } }
		}

		public static class Subcategory
		{
			public static Entry Favorites { get { return new Entry("Favorites", ResString.GetMultilingualString("Subcategory.Favorites", "Favorites"), "F"); } }
			public static Entry RecentItems { get { return new Entry("RecentItems", ResString.GetMultilingualString("Subcategory.RecentItems", "Recent Items"), "I"); } }
			public static Entry RecentModules { get { return new Entry("RecentModules", ResString.GetMultilingualString("Subcategory.RecentModules", "Recent Modules"), "M"); } }

			public static Entry Schedules { get { return new Entry("Schedules", ResString.GetMultilingualString("Subcategory.Schedules", "Schedules"), "S"); } }
			public static Entry Forwarding { get { return new Entry("Forwarding", ResString.GetMultilingualString("Subcategory.Forwarding", "Forwarding"), "F"); } }
			public static Entry Customs { get { return new Entry("Customs", ResString.GetMultilingualString("Subcategory.Customs", "Customs"), "C"); } }
			public static Entry Transport { get { return new Entry("Transport", ResString.GetMultilingualString("Subcategory.Transport", "Transport"), "T"); } }
			public static Entry Warehouse { get { return new Entry("Warehouse", ResString.GetMultilingualString("Subcategory.Warehouse", "Warehouse"), "W"); } }
			public static Entry LinerAndAgency { get { return new Entry("LinerAndAgency", ResString.GetMultilingualString("Subcategory.LinerAndAgency", "Liner & Agency"), "L"); } }
			public static Entry OceanCarrier => new Entry("OceanCarrier", ResString.GetMultilingualString("Subcategory.OceanCarrier", "Ocean Carrier"), "O");

			public static Entry ProductivityTools { get { return new Entry("ProductivityTools", ResString.GetMultilingualString("Subcategory.ProductivityTools", "Productivity Tools"), "P"); } }
			public static Entry WorkflowAndProcess { get { return new Entry("WorkflowAndProcess", ResString.GetMultilingualString("Subcategory.WorkflowAndProcess", "Workflow & Process"), "W"); } }
			public static Entry DocManager { get { return new Entry("DocManager", ResString.GetMultilingualString("Subcategory.DocManager", "DocManager"), "D"); } }
			public static Entry SalesAndMarketing { get { return new Entry("SalesAndMarketing", ResString.GetMultilingualString("Subcategory.SalesAndMarketing", "Sales & Marketing"), "M"); } }
			public static Entry TariffsAndRates { get { return new Entry("TariffsAndRates", ResString.GetMultilingualString("Subcategory.TariffsAndRates", "Tariffs & Rates"), "T"); } }
			public static Entry Receivables { get { return new Entry("Receivables", ResString.GetMultilingualString("Subcategory.Receivables", "Receivables"), "R"); } }
			public static Entry Payables { get { return new Entry("Payables", ResString.GetMultilingualString("Subcategory.Payables", "Payables"), "P"); } }
			public static Entry CashBook { get { return new Entry("CashBook", ResString.GetMultilingualString("Subcategory.CashBook", "Cash Book"), "C"); } }
			public static Entry JobCosting { get { return new Entry("JobCosting", ResString.GetMultilingualString("Subcategory.JobCosting", "Job Costing"), "J"); } }
			public static Entry GeneralLedger { get { return new Entry("GeneralLedger", ResString.GetMultilingualString("Subcategory.GeneralLedger", "General Ledger"), "G"); } }
			public static Entry Budgets { get { return new Entry("Budgets", ResString.GetMultilingualString("Subcategory.Budgets", "Budgets"), "B"); } }
			public static Entry Netting { get { return new Entry("Netting", ResString.GetMultilingualString("Subcategory.Netting", "Netting"), "N"); } }
			public static Entry AssetManagement => new Entry("Asset Management", ResString.GetMultilingualString("Subcategory.AssetManagement", "Asset Management"), "F");

			public static Entry ReferenceFiles { get { return new Entry("ReferenceFiles", ResString.GetMultilingualString("Subcategory.ReferenceFiles", "Reference Files"), "R"); } }
			public static Entry PerformanceManagement { get { return new Entry("BehaviorManagement", ResString.GetMultilingualString("Subcategory.BehaviorManagement", "Performance Management"), "P"); } }
			public static Entry Locations { get { return new Entry("Locations", ResString.GetMultilingualString("Subcategory.Locations", "Locations"), "L"); } }
			public static Entry Account { get { return new Entry("Account", ResString.GetMultilingualString("Subcategory.Account", "Account"), "A"); } }
			public static Entry HRM { get { return new Entry("HRM", ResString.GetMultilingualString("Subcategory.HRM", "Human Resources"), "H"); } }
			public static Entry ProcessManager { get { return new Entry("ProcessManager", ResString.GetMultilingualString("Subcategory.ProcessManager", "Workflow Manager"), "K"); } }
			public static Entry EDIMessaging { get { return new Entry("EDIMessaging", ResString.GetMultilingualString("Subcategory.EDIMessaging", "EDI Messaging"), "I"); } }
			public static Entry ArchiveManager { get { return new Entry("ArchiveManager", ResString.GetMultilingualString("Subcategory.ArchiveManager", "Archive Manager"), "Z"); } }
			public static Entry System { get { return new Entry("System", ResString.GetMultilingualString("Subcategory.System", "System"), "S"); } }
			public static Entry UserAdmin { get { return new Entry("UserAdmin", ResString.GetMultilingualString("Subcategory.UserAdmin", "User Admin"), "U"); } }
			public static Entry BusinessIntelligenceAndAnalytics { get { return new Entry("BusinessIntelligenceAndAnalytics", ResString.GetMultilingualString("Subcategory.BusinessIntelligenceAndAnalytics", "Business Intelligence & Analytics"), "A"); } }
			public static Entry MasterData { get { return new Entry("MasterData", ResString.GetMultilingualString("Subcategory.MasterData", "Master Data"), "D"); } }
		}

		public static class Section
		{
			// Jump
			public static Entry Favorites { get { return new Entry("Favorites", ResString.GetMultilingualString("Section.Favorites", "Favorites")); } }
			public static Entry RecentModules { get { return new Entry("RecentModules", ResString.GetMultilingualString("Section.RecentModules", "Recent Modules")); } }
			public static Entry RecentItems { get { return new Entry("RecentItems", ResString.GetMultilingualString("Section.RecentItems", "Recent Items")); } }

			// Operations
			public static Entry Schedules { get { return new Entry("Schedules", ResString.GetMultilingualString("Section.Schedules", "Schedules")); } }
			public static Entry Forwarding { get { return new Entry("Forwarding", ResString.GetMultilingualString("Section.Forwarding", "Forwarding")); } }
			public static Entry CustomsMain { get { return new Entry("CustomsMain", ResString.GetMultilingualString("Section.CustomsMain", "Customs")); } }
			public static Entry CustomsGlobal { get { return new Entry("CustomsGlobal", ResString.GetMultilingualString("Section.CustomsGlobal", "Customs Global")); } }
			public static Entry OrderManager { get { return new Entry("OrderManager", ResString.GetMultilingualString("Section.OrderManager", "Order Manager")); } }

			public static Entry PortTransport { get { return new Entry("Transport", ResString.GetMultilingualString("Section.PortTransport", "Port Transport")); } }
			public static Entry TransportConsignment { get { return new Entry("TransportConsignment", ResString.GetMultilingualString("Section.TransportConsignment", "Land Transport")); } }
			public static Entry Transport { get { return new Entry("Transport", ResString.GetMultilingualString("Section.Transport", "Port Transport")); } }

			public static Entry TransportBooking { get { return new Entry("TransportBooking", ResString.GetMultilingualString("Section.TransportBooking", "Transport Booking")); } }
			public static Entry CFSCTO { get { return new Entry("CFSCTO", ResString.GetMultilingualString("Section.CFSCTO", "CFS/CTO")); } }
			public static Entry Ccsuk { get { return new Entry("CCSUK", ResString.GetMultilingualString("Section.CCSUK", "CCS-UK")); } }
			public static Entry EuNcts { get { return new Entry("NCTS", ResString.GetMultilingualString("Section.NCTS", "NCTS")); } }
			public static Entry Warehouse { get { return new Entry("Warehouse", ResString.GetMultilingualString("Section.Warehouse", "Product Warehouse")); } }
			public static Entry TransitWarehouse { get { return new Entry("TransitWarehouse", ResString.GetMultilingualString("Section.TransitWarehouse", "Transit Warehouse")); } }
			public static Entry ContainerYard { get { return new Entry("ContainerYard", ResString.GetMultilingualString("Section.ContainerYard", "Container Yard")); } }
			public static Entry GateManagement { get { return new Entry("GateManagement", ResString.GetMultilingualString("Section.GateManagement", "Gate Management")); } }
			public static Entry TariffsAndRates { get { return new Entry("TariffsAndRates", ResString.GetMultilingualString("Section.TariffsAndRates", "Tariffs && Rates")); } }
			public static Entry WiseRates { get { return new Entry("WiseRates", ResString.GetMultilingualString("Section.WiseRates", "Rates Service")); } }

			public static Entry LinerAndAgency { get { return new Entry("LinerAndAgency", ResString.GetMultilingualString("Section.LinerAndAgency", "Liner && Agency")); } }
			public static Entry OceanCarrier => new Entry("OceanCarrier", ResString.GetMultilingualString("Section.OceanCarrier", "Ocean Carrier"));
			public static Entry ManageWorkflowSection { get { return new Entry("ManageWorkflowSection", ResString.GetMultilingualString("Section.ManageWorkflowSection", "Workflow && Process")); } }
			public static Entry ProductivitySection { get { return new Entry("ProductivitySection", ResString.GetMultilingualString("Section.ProductivitySection", "Productivity Tools")); } }
			public static Entry WorkflowPlanning { get { return new Entry("WorkflowPlanning", ResString.GetMultilingualString("Section.WorkflowPlanning", "Planning")); } }
			public static Entry DocManager { get { return new Entry("DocManager", ResString.GetMultilingualString("Section.DocManager", "DocManager")); } }
			public static Entry StampDutyMain { get { return new Entry("StampDutyMain", ResString.GetMultilingualString("Section.StampDutyMain", "Stamp Duty")); } }
			public static Entry EquipmentManagement { get { return new Entry("EquipmentManagement", ResString.GetMultilingualString("Section.EquipmentManagement", "Equipment Management")); } }

			// Accounts
			public static Entry Receivables { get { return new Entry("Receivables", ResString.GetMultilingualString("Section.Receivables", "Receivables")); } }
			public static Entry Payables { get { return new Entry("Payables", ResString.GetMultilingualString("Section.Payables", "Payables")); } }
			public static Entry CashBook { get { return new Entry("CashBook", ResString.GetMultilingualString("Section.CashBook", "Cash Book")); } }
			public static Entry JobCosting { get { return new Entry("JobCosting", ResString.GetMultilingualString("Section.JobCosting", "Job Costing")); } }
			public static Entry GeneralLedger { get { return new Entry("GeneralLedger", ResString.GetMultilingualString("Section.GeneralLedger", "General Ledger")); } }
			public static Entry GLConsolidations { get { return new Entry("GLConsolidations", ResString.GetMultilingualString("Section.GLConsolidations", "GL Consolidations")); } }
			public static Entry GLReportingBooks { get { return new Entry("GLReportingBooks", ResString.GetMultilingualString("Section.ReportingBooks", "Reporting Books")); } }
			public static Entry Budgets { get { return new Entry("Budgets", ResString.GetMultilingualString("Section.Budgets", "Budgets")); } }
			public static Entry Netting { get { return new Entry("Netting", ResString.GetMultilingualString("Section.Netting", "Netting")); } }
			public static Entry BusinessIntelligenceAndAnalytics { get { return new Entry("BusinessIntelligenceAndAnalytics", ResString.GetMultilingualString("Section.BusinessIntelligenceAndAnalytics", "Business Intelligence && Analytics")); } }
			public static Entry ClientRelationshipManagement { get { return new Entry("ClientRelationshipManagement", ResString.GetMultilingualString("Section.ClientRelationshipManagement", "Client Relationship Management")); } }
			public static Entry AssetManagement => new Entry("AssetManagement", ResString.GetMultilingualString("Section.AssetManagement", "Asset Management"));
			public static Entry ReportingBooks { get { return new Entry("ReportingBooks", ResString.GetMultilingualString("Section.ReportingBooks", "Reporting Books")); } }

			// Admin
			public static Entry References { get { return new Entry("References", ResString.GetMultilingualString("Section.References", "Reference Files")); } }
			public static Entry RelationshipManagerConfig { get { return new Entry("SalesManagerConfig", ResString.GetMultilingualString("Section.SalesManagerConfig", "Sales && Marketing")); } }
			public static Entry BufferManagementConfig { get { return new Entry("BufferManagementConfig", ResString.GetMultilingualString("Section.BufferManagementConfig", "Buffer Management")); } }
			public static Entry Location { get { return new Entry("Location", ResString.GetMultilingualString("Section.Location", "Locations")); } }
			public static Entry Account { get { return new Entry("Account", ResString.GetMultilingualString("Section.Account", "Account")); } }
			public static Entry CustomsFiles { get { return new Entry("CustomsFiles", ResString.GetMultilingualString("Section.Customs", "Customs Files")); } }
			public static Entry CustomsCA { get { return new Entry("Customs CA", ResString.GetMultilingualString("Section.CustomsCA", "Customs (CA)")); } }
			public static Entry CustomsUS { get { return new Entry("Customs US", ResString.GetMultilingualString("Section.CustomsUS", "Customs (US)")); } }
			public static Entry CustomsCO { get { return new Entry("Customs CO", ResString.GetMultilingualString("Section.CustomsCO", "Customs (CO)")); } }
			public static Entry WhsConfig { get { return new Entry("WarehouseConfig", ResString.GetMultilingualString("Section.WarehouseConfig", "Warehouse")); } }
			public static Entry HRRecruiter { get { return new Entry("HRRecruiter", ResString.GetMultilingualString("Section.HRRecruiter", "Recruiter")); } }
			public static Entry PeopleOperations { get { return new Entry("PeopleOperations", ResString.GetMultilingualString("Section.PeopleOperations", "People Operations")); } }
			public static Entry LearningDevelopment { get { return new Entry("LearningDevelopment", ResString.GetMultilingualString("Section.LearningDevelopment", "Learning && Development")); } }
			public static Entry WorkflowSection { get { return new Entry("ProcessManagerSection", ResString.GetMultilingualString("Section.ProcessManagerSection", "Workflow Manager")); } }
			public static Entry EDIMessaging { get { return new Entry("EDIMessagingSection", ResString.GetMultilingualString("Section.EDIMessagingSection", "EDI Messaging")); } }

			public static Entry ArchiveManagerSection { get { return new Entry("ArchiveManagerSection", ResString.GetMultilingualString("Section.ArchiveManagerSection", "Archive Manager")); } }
			public static Entry System { get { return new Entry("System", ResString.GetMultilingualString("Section.System", "System")); } }
			public static Entry UserAdmin { get { return new Entry("UserAdmin", ResString.GetMultilingualString("Section.UserAdmin", "User Admin")); } }
			public static Entry ReportSection { get { return new Entry("Reports", ResString.GetMultilingualString("Section.ReportSection", "Reports")); } }
			public static Entry PrintingSection { get { return new Entry("Printing", ResString.GetMultilingualString("Section.PrintingSection", "Printing")); } }
			public static Entry EmailSection { get { return new Entry("EMail", ResString.GetMultilingualString("Section.EmailSection", "Email")); } }
			public static Entry MasterData { get { return new Entry("MasterData", ResString.GetMultilingualString("Section.MasterData", "Master Data")); } }
		}

		#endregion
	}
}
