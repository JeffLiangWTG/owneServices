using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Interceptors;

namespace Enterprise.DataTransfer.Native.Business.Update.EntityInfos
{
	public class EntityInfoInterceptor : BaseInterceptor
	{
		IEntityInfoHelper entityInfoHelper;
		readonly EntityInfoSetting setting;

		public EntityInfoInterceptor(EntityInfoSetting setting, AncillaryImportServices sessionServices)
			: base(setting, sessionServices)
		{
			entityInfoHelper = new EntityInfoHelper();
			this.setting = setting;
		}

		#region Dependency
#if DEBUG
		internal IEntityInfoHelper EntityInfoHelper
		{
			get { return entityInfoHelper; }
			set { entityInfoHelper = value; }
		}
#endif
		#endregion

		public override void Invoke(IEntitySet entitySet)
		{
			entityInfoHelper.SaveEntityName(setting.EntityInfo, entitySet);
			entityInfoHelper.SaveForeignCode(setting.EntityInfo, entitySet);
			Function(entitySet);
			entityInfoHelper.SaveLocalCode(setting.EntityInfo, entitySet);
			entityInfoHelper.SavePrimaryKey(setting.EntityInfo, entitySet);
		}
	}
}
