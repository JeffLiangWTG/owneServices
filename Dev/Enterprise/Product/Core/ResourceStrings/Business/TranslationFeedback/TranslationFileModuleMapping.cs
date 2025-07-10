using System;
using System.Collections.Generic;

namespace Enterprise.ResourceStrings
{
	public class TranslationFileModuleMapping
	{
		public static TranslationFileModuleMapping Instance
		{
			get { return instance ?? (instance = new TranslationFileModuleMapping()); }
		}
		static TranslationFileModuleMapping instance;

		TranslationFileModuleMapping()
		{
			Add(ModuleTypes.GUI, "Rating", "Rating");
			Add(ModuleTypes.GUI, "Documents", "DocumentEngine", "DocumentScanning", "DocumentWrappers");
			Add(ModuleTypes.GUI, "Port Transport", "GPS", "Freight.LocalCartage");
			Add(ModuleTypes.GUI, "Transport Bookings", "TransportBookings");
			Add(ModuleTypes.GUI, "Warehouse", "Warehouse", "Packing");
			Add(ModuleTypes.GUI, "DPS", "DeniedPartyScreening");
			Add(ModuleTypes.GUI, "Accountant", "Accounting");
			Add(ModuleTypes.GUI, "Forwarder", "Freight");
			Add(ModuleTypes.GUI, "CFS Manager", "Freight.CFS");
			Add(ModuleTypes.GUI, "Order Manager", "Freight.Forwarding.Orders");
			Add(ModuleTypes.GUI, "Shipping Manager", "Freight.Agency");
			Add(ModuleTypes.GUI, "MailManager", "MailManager", "BatchProcessor.MailManager");
			Add(ModuleTypes.GUI, "Port Messaging", "Freight.Forwarding.PortMessaging");
			Add(ModuleTypes.Customs, "Customs", "Customs");

			Add(ModuleTypes.WebTracker, "Web Tracker", "ZArchitecture.Web", "Tracking", "WebCFS", "Registry.Web", "DocumentEngine.Web");

			Add(ModuleTypes.DoNotTranslate, "Resource Strings", "ResourceStrings");
			Add(ModuleTypes.DoNotTranslate, "Interface Connector", "DataImport", "DataPurge", "DataTransfer", "MasterFiles.DataTransfer", "Freight.DataTransfer", "Freight.Forwarding.DataTransfer", "Freight.LocalCartage.DataTransfer", "Freight.QuotedBookings.DataTransfer", "Freight.Agency.DataTransfer", "Accounting.DataTransfer", "DocumentScanning.DataTransfer", "Warehouse.DataTransfer", "Warehouse.Transactions.DataTransfer", "Rating.DataTransfer");
			Add(ModuleTypes.DoNotTranslate, "eServices", "eHubMessaging", "Messaging", "DataConverters", "UniversalDataBuss");
			Add(ModuleTypes.DoNotTranslate, "Customs", "ACEManifest", "Customs.AE", "Customs.ASYCUDA", "Customs.AsycudaCustoms", "Customs.ASYCUDAManifest", "Customs.CustomsWare", "Customs.US", "Customs.BR", "Customs.AU", "Customs.GB", "Customs.HK", "Customs.SG",
				"Customs.SGAccess", "Customs.IN", "Customs.ZA", "Customs.ZA.Manifest", "Customs.JP", "Customs.MY", "Customs.NZ", "Customs.US.ACEManifest", "AMS", "Registry.Customs", "Shipnet", "MasterFiles.Customs", "eManifest");
			Add(ModuleTypes.GUI, "Recruiter", "Recruiter");
			Add(ModuleTypes.DoNotTranslate, "Client", "Client", "ClientSharedComponents");
			Add(ModuleTypes.DoNotTranslate, "Certification", "Certification");
			Add(ModuleTypes.DoNotTranslate, "Transit Warehouse", "Registry.Warehouse", "TransportCommon.Registry", "Warehouse.Transit", "Warehouse.Integration");
			Add(ModuleTypes.PAVE, "PAVE", "BufferManagement", "MasterFiles.BufferManagement", "BehaviourManagement", "VisualBoards", "NetworkVisualisation", "ProcessManagement");
			Add(ModuleTypes.DoNotTranslate, "Shpping Instruction", "Freight.Forwarding.ShippingInstruction");
			Add(ModuleTypes.DoNotTranslate, "GLOW", "eTail", "Accounting.Netting", "Accounting.eNett", "eNett_Integration");

			Add(ModuleTypes.Billing, "ZClientEDI", "ZClientEDI", "Client.EDI.Billing");
		}

		public enum ModuleTypes
		{
			DoNotTranslate,
			GUI,
			WebTracker,
			Billing,
			PAVE,
			Customs,
			UpdateNotes,
		}

		void Add(ModuleTypes moduleType, string moduleName, params string[] paths)
		{
			foreach (var path in paths)
			{
				root.Add(moduleName, moduleType, path.Split('.'));
			}
		}

		public Node Lookup(string ns)
		{
			return Lookup(ns.Split('.'));
		}

		public Node Lookup(string[] ns)
		{
			return root.Find(Filter(ns));
		}

		public string[] Filter(string[] ns)
		{
			return Array.FindAll(ns, part => Array.IndexOf(NonContextPaths, part) == -1);
		}

		readonly Node root = new Node() { ModuleName = Core, ModuleType = ModuleTypes.GUI };

		static readonly string[] NonContextPaths = new string[] { "GUI", "Test", "Testing", "Common", "Module", "Business", "UserControls", "Enterprise", "CargoWise" };

		public const string Core = "Core";

		public class Node
		{
			public string ModuleName { get; set; }
			public ModuleTypes ModuleType { get; set; }
			readonly Dictionary<string, Node> paths = new Dictionary<string, Node>(StringComparer.OrdinalIgnoreCase);

			public Node Find(string[] path)
			{
				if (path.Length == 0)
				{
					return this;
				}
				Node match;
				paths.TryGetValue(path[0], out match);
				if (match != null)
				{
					if (path.Length > 1)
					{
						string[] subPath = new string[path.Length - 1];
						Array.Copy(path, 1, subPath, 0, subPath.Length);
						return match.Find(subPath);
					}
					else
					{
						return match;
					}
				}
				else
				{
					return this;
				}
			}

			public void Add(string moduleName, ModuleTypes moduleType, string[] path)
			{
				Node match;
				paths.TryGetValue(path[0], out match);
				if (match == null)
				{
					if (path.Length > 1)
					{
						match = new Node() { ModuleName = this.ModuleName, ModuleType = this.ModuleType };
					}
					else
					{
						match = new Node() { ModuleName = moduleName, ModuleType = moduleType };
					}
					paths.Add(path[0], match);
				}
				if (path.Length > 1)
				{
					string[] subPath = new string[path.Length - 1];
					Array.Copy(path, 1, subPath, 0, subPath.Length);
					match.Add(moduleName, moduleType, subPath);
				}
				else
				{
					match.ModuleName = moduleName;
					match.ModuleType = moduleType;
				}
			}
		}
	}
}
