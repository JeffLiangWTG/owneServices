using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Client.EDI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class NotificationRolesContactsModule : ZFilterStripGridModule
	{
		public NotificationRolesContactsModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		public override ModuleIdentifier ID => WebModuleIDs.CargoWiseEDINotificationRolesContacts;

		public override Type GridCollectionType => typeof(OrgContactCollection);

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new NotificationRolesContactFilterStripBusinessObject(Factory);

		protected override GridColumnProvider GetColumnProvider() => new NotificationRolesContactsModuleColumnProvider();

		public override SchemaColumn GetBusinessObjectPKColumn(FilterBusinessObject filter) => OrgContactSchema.PK;

		protected override FilterLayoutCodePairRegistryItem DefaultLayoutRegistryItem => EDIDataRegistry.Instance.DefaultFilterLayoutForNotificationRoles;

		protected override SchemaPKColumn RelevantPersistantPKColumn => OrgContactSchema.PK;

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter) => new[] { new ColumnAndSortOrder(OrgContactSchema.OC_ContactName.Name, DefaultSortOrder) };

		protected override ZQuery GetCurrentLoggedInUserFilter(FilterBusinessObject filterBizO) => new ZQuery();
	}
}
