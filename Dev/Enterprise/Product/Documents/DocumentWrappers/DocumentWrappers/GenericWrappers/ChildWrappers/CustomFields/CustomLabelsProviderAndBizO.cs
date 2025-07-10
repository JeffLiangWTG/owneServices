using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class CustomLabelsProviderAndBizO
	{
		public CustomLabelsProviderAndBizO(IAccessBusinessObject bizOWithCustomFields, ICustomLabelsProvider customLabelsProvider)
		{
			this.bizOWithCustomFields = Argument.NotNull(bizOWithCustomFields, "bizOWithCustomFields");
			this.customLabelsProvider = Argument.NotNull(customLabelsProvider, "customLabelsProvider");
		}

		readonly IAccessBusinessObject bizOWithCustomFields;
		readonly ICustomLabelsProvider customLabelsProvider;

		public IAccessBusinessObject BizOWithCustomFields
		{
			get { return bizOWithCustomFields; }
		}

		public ICustomLabelsProvider CustomLabelsProvider
		{
			get { return customLabelsProvider; }
		}
	}
}
