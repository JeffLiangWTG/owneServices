using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.EDI.Modules;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.MarketingManager.GUI
{
	public class ContactsIncidentsFilter : ModuleGuidForeignCollectionFilterAlternativeParentWorkflowProvider
	{
		protected ContactsIncidentsFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public ContactsIncidentsFilter(ZString description, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn foreignKeyColumn, IBusinessObjectCollection list, Type parentBusinessObjectType)
			: base(description, ClientModuleRegistration.SupportIncident, primaryKeyColumn, foreignKeyColumn, list, parentBusinessObjectType)
		{
		}

		public ContactsIncidentsFilter(ZString description, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn foreignKeyColumn, GetList listDelegate, Type parentBusinessObjectType)
			: base(description, ClientModuleRegistration.SupportIncident, primaryKeyColumn, foreignKeyColumn, listDelegate, parentBusinessObjectType)
		{
		}
	}
}
