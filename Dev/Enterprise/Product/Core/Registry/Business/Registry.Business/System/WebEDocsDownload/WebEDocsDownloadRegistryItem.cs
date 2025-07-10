using Enterprise.Integration;
using Enterprise.Integration.Web;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class WebEDocsDownloadRegistryItem : StronglyTypedRegistryItem<WebEDocsDownloadEntryDictionary>
	{
		IWebEDocsDownloadModulesList WebEDocsDownloadModulesList
		{
			get
			{
				return webEDocsDownloadModulesList ?? (webEDocsDownloadModulesList = CargoWise.Application.ObjectFactory.Get<IWebEDocsDownloadModulesList>("IWebEDocsDownloadModulesList"));
			}
		}
		IWebEDocsDownloadModulesList webEDocsDownloadModulesList;

		public WebEDocsDownloadRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new WebEDocsDownloadRegistryDataType(), storage, options))
		{
		}

		public WebEDocsDownloadEntry GetModule(string moduleName)
		{
			if (Value.ContainsKey(moduleName))
			{
				var module = Value[moduleName];
				if (module.DocTypeCollection.Count > 0 || module.AllowAllDocTypes)
				{
					return module;
				}
			}

			if (Value.ContainsKey(WebEDocsDownloadModulesList.AllModules.Code))
			{
				var allModules = Value[WebEDocsDownloadModulesList.AllModules.Code];
				if (allModules.DocTypeCollection.Count > 0 || allModules.AllowAllDocTypes)
				{
					return allModules;
				}
			}

			return null;
		}

		public new WebEDocsDownloadRegistryDataType DataType
		{
			get { return (WebEDocsDownloadRegistryDataType)base.DataType; }
			set { base.DataType = value; }
		}
	}
}
