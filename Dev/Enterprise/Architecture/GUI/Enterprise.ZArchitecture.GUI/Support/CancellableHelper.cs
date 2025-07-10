using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI.Support
{
	class CancellableHelper
	{
		public static ICancellable GetICancellable(IBusiness businessEntity)
		{
			if (ZFilterGridModule.IsTemplateRecord(businessEntity)
				&& businessEntity is ITemplateRecordProvider templateRecordProvider
				&& templateRecordProvider.TemplateRecord != null)
			{
				return templateRecordProvider.TemplateRecord as ICancellable;
			}

			return businessEntity as ICancellable;
		}
	}
}
