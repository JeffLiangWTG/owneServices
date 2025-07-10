using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public class StatusRequestLookups : EDIMessageLookups
	{
		public StatusRequestLookups(StatusRequest parent) : base(parent)
		{
		}

		protected new StatusRequest Parent => (StatusRequest)base.Parent;

		public CodeDescriptionPairList ModuleList => Factory.GetCachedValue<ExportStatusRequestModuleCodeList>();

		public CodeDescriptionPairList RoleList
		{
			get
			{
				return Parent.Module == ExportStatusRequestModuleCodeList.Codes.NCTS
					? Factory.GetCachedValue<ExportStatusRequestNCTSRoleList>()
					: Factory.GetCachedValue<ExportStatusRequestAESRoleList>();
			}
		}

		public OrgHeaderCollection Identifications => new OrgHeaderCollection(Factory);
	}
}
