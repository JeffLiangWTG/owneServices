using System;
using System.Collections.Generic;
using System.Linq;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ServiceManager.Business
{
	public class ServiceTaskCategoryDescriptor
	{
		public ServiceTaskCategoryDescriptor(string code, string description, bool isShownInProductivityWiseMode)
		{
			Code = code;
			Description = description;
			IsShownInProductivityWiseMode = isShownInProductivityWiseMode;
		}

		public string Code { get; }
		public string Description { get; }
		public bool IsShownInProductivityWiseMode { get; }
	}

	public static class ServiceTaskCategoryDescriptors
	{
		[ThreadSafe]
		static readonly ServiceTaskCategoryDescriptor[] descriptors =
		{
			new ServiceTaskCategoryDescriptor("ACC", Res.GetString("ServiceManager|ServiceTaskFilter|Category|ACC", "Accounting"), true),
			new ServiceTaskCategoryDescriptor("AUC", Res.GetString("ServiceManager|ServiceTaskFilter|Category|AUC", "Australian Customs"), false),
			new ServiceTaskCategoryDescriptor("BI", Res.GetString("ServiceManager|ServiceTaskFilter|Category|BI", "Business Intelligence"), true),
			new ServiceTaskCategoryDescriptor("BMS", Res.GetString("ServiceManager|ServiceTaskFilter|Category|BMS", "Buffer Management"), true),
			new ServiceTaskCategoryDescriptor("BP", Res.GetString("ServiceManager|ServiceTaskFilter|Category|BP", "Batch Processors"), true),
			new ServiceTaskCategoryDescriptor("CAC", Res.GetString("ServiceManager|ServiceTaskFilter|Category|CAC", "Canadian Customs"), false),
			new ServiceTaskCategoryDescriptor("CIM", Res.GetString("ServiceManager|ServiceTaskFilter|Category|CIM", "CargoIMP Messaging"), false),
			new ServiceTaskCategoryDescriptor("CNC", Res.GetString("ServiceManager|ServiceTaskFilter|Category|CNC", "China Customs"), false),
			new ServiceTaskCategoryDescriptor("CSP", Res.GetString("ServiceManager|ServiceTaskFilter|Category|CSP", "Client Specific Function"), false),
			new ServiceTaskCategoryDescriptor("DBM", Res.GetString("ServiceManager|ServiceTaskFilter|Category|DBM", "Database Backup and Maintenance"), true),
			new ServiceTaskCategoryDescriptor("DDC", Res.GetString("ServiceManager|ServiceTaskFilter|Category|DDC", "DocManager Database Creator"), true),
			new ServiceTaskCategoryDescriptor("DEC", Res.GetString("ServiceManager|ServiceTaskFilter|Category|DEC", "Germany Customs"), false),
			new ServiceTaskCategoryDescriptor("DOC", Res.GetString("ServiceManager|ServiceTaskFilter|Category|DOC", "Document Job Manager"), true),
			new ServiceTaskCategoryDescriptor("DOM", Res.GetString("ServiceManager|ServiceTaskFilter|Category|DOM", "Port Transport"), false),
			new ServiceTaskCategoryDescriptor("DPS", Res.GetString("ServiceManager|ServiceTaskFilter|Category|DPS", "Denied Party Screening Automation"), false),
			new ServiceTaskCategoryDescriptor("ESC", Res.GetString("ServiceManager|ServiceTaskFilter|Category|ESC", "Spain Customs"), false),
			new ServiceTaskCategoryDescriptor("ESV", Res.GetString("ServiceManager|ServiceTaskFilter|Category|ESV", "eServices"), true),
			new ServiceTaskCategoryDescriptor("EUC", Res.GetString("ServiceManager|ServiceTaskFilter|Category|EUC", "European Union Customs"), false),
			new ServiceTaskCategoryDescriptor("FRC", Res.GetString("ServiceManager|ServiceTaskFilter|Category|FRC", "France Customs"), false),
			new ServiceTaskCategoryDescriptor("FRT", Res.GetString("ServiceManager|ServiceTaskFilter|Category|FRT", "Freight"), false),
			new ServiceTaskCategoryDescriptor("GBC", Res.GetString("ServiceManager|ServiceTaskFilter|Category|GBC", "United Kingdom Customs"), false),
			new ServiceTaskCategoryDescriptor("GPS", Res.GetString("ServiceManager|ServiceTaskFilter|Category|GPS", "GPS Integration"), false),
			new ServiceTaskCategoryDescriptor("HKC", Res.GetString("ServiceManager|ServiceTaskFilter|Category|HKC", "Hong Kong Customs"), false),
			new ServiceTaskCategoryDescriptor("HRM", Res.GetString("ServiceManager|ServiceTaskFilter|Category|HRM", "Human Resource Management"), false),
			new ServiceTaskCategoryDescriptor("IEC", Res.GetString("ServiceManager|ServiceTaskFilter|Category|IEC", "Ireland Customs"), false),
			new ServiceTaskCategoryDescriptor("ISF", Res.GetString("ServiceManager|ServiceTaskFilter|Category|ISF", "Importer Security Filing"), false),
			new ServiceTaskCategoryDescriptor("ITC", Res.GetString("ServiceManager|ServiceTaskFilter|Category|ITC", "Italian Customs"), false),
			new ServiceTaskCategoryDescriptor("KRC", Res.GetString("ServiceManager|ServiceTaskFilter|Category|KRC", "KR Customs Messaging"), false),
			new ServiceTaskCategoryDescriptor("MAI", Res.GetString("ServiceManager|ServiceTaskFilter|Category|MAI", "E-mail"), true),
			new ServiceTaskCategoryDescriptor("NZC", Res.GetString("ServiceManager|ServiceTaskFilter|Category|NZC", "NZ Customs Messaging"), false),
			new ServiceTaskCategoryDescriptor("PKG", Res.GetString("ServiceManager|ServiceTaskFilter|Category|PKG", "Packing"), false),
			new ServiceTaskCategoryDescriptor("SAL", Res.GetString("ServiceManager|ServiceTaskFilter|Category|SAL", "Sales & Marketing"), true),
			new ServiceTaskCategoryDescriptor("SCM", Res.GetString("ServiceManager|ServiceTaskFilter|Category|SCM", "SG Messaging"), false),
			new ServiceTaskCategoryDescriptor("SYS", Res.GetString("ServiceManager|ServiceTaskFilter|Category|SYS", "System"), true),
			new ServiceTaskCategoryDescriptor("TEL", Res.GetString("ServiceManager|ServiceTaskFilter|Category|TEL", "Telematics"), false),
			new ServiceTaskCategoryDescriptor("USC", Res.GetString("ServiceManager|ServiceTaskFilter|Category|USC", "United States Customs"), false),
			new ServiceTaskCategoryDescriptor("WFL", Res.GetString("ServiceManager|ServiceTaskFilter|Category|WFL", "Workflow Manager"), true),
			new ServiceTaskCategoryDescriptor("WHS", Res.GetString("ServiceManager|ServiceTaskFilter|Category|WHS", "Warehouse"), false),
			new ServiceTaskCategoryDescriptor("ZAC", Res.GetString("ServiceManager|ServiceTaskFilter|Category|ZAC", "ZA Customs Messaging"), false),
			new ServiceTaskCategoryDescriptor("TWC", Res.GetString("ServiceManager|ServiceTaskFilter|Category|TWC", "TW Customs Messaging"), false),
			new ServiceTaskCategoryDescriptor("TRC", Res.GetString("ServiceManager|ServiceTaskFilter|Category|TRC", "TR Customs Messaging"), false),
			new ServiceTaskCategoryDescriptor("UYC", Res.GetString("ServiceManager|ServiceTaskFilter|Category|UYC", "UY Customs Messaging"), false),
			new ServiceTaskCategoryDescriptor("CUS", Res.GetString("ServiceManager|ServiceTaskFilter|Category|CUS", "Customs Shared"), false),
			new ServiceTaskCategoryDescriptor("MXC", Res.GetString("ServiceManager|ServiceTaskFilter|Category|MXC", "MX Customs Messaging"), false),
			new ServiceTaskCategoryDescriptor("CHL", Res.GetString("ServiceManager|ServiceTaskFilter|Category|CHL", "CL Customs Messaging"), false),
			new ServiceTaskCategoryDescriptor("BRC", Res.GetString("ServiceManager|ServiceTaskFilter|Category|BRP", "BR Customs Messaging"), false),
			new ServiceTaskCategoryDescriptor("BEC", Res.GetString("ServiceManager|ServiceTaskFilter|Category|BEC", "BE Customs Messaging"), false),
			new ServiceTaskCategoryDescriptor("NLC", Res.GetString("ServiceManager|ServiceTaskFilter|Category|NLC", "NL Customs Messaging"), false),
			new ServiceTaskCategoryDescriptor("ARC", Res.GetString("ServiceManager|ServiceTaskFilter|Category|ARC", "AR Customs Messaging"), false),
			new ServiceTaskCategoryDescriptor("CHC", Res.GetString("ServiceManager|ServiceTaskFilter|Category|CHC", "CH Customs Messaging"), false),
			new ServiceTaskCategoryDescriptor("ICS", Res.GetString("ServiceManager|ServiceTaskFilter|Category|ICS", "EU ICS2 Messaging"), false),
			new ServiceTaskCategoryDescriptor("PLC", Res.GetString("ServiceManager|ServiceTaskFilter|Category|PLC", "PL Customs Messaging"), false),
			new ServiceTaskCategoryDescriptor("RAT", Res.GetString("ServiceManager|ServiceTaskFilter|Category|RAT", "Rating"), false),
			new ServiceTaskCategoryDescriptor("SCI", Res.GetString("ServiceManager|ServiceTaskFilter|Category|SCI", "Scim"), false),
			new ServiceTaskCategoryDescriptor("ILC", Res.GetString("ServiceManager|ServiceTaskFilter|Category|ILC", "IL Customs Messaging"), false),
			new ServiceTaskCategoryDescriptor("CPW", Res.GetString("ServiceManager|ServiceTaskFilter|Category|CPW", "Compliance Wise"), false),

#if DEBUG
			new ServiceTaskCategoryDescriptor("TST", "Test Only", true),
#endif
		};

		public static ServiceTaskCategoryDescriptor Get(string code)
		{
			return descriptors.SingleOrDefault(x => x.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
		}

		public static IEnumerable<ServiceTaskCategoryDescriptor> Get(Func<ServiceTaskCategoryDescriptor, bool> selector)
		{
			return descriptors.Where(selector);
		}
	}
}

