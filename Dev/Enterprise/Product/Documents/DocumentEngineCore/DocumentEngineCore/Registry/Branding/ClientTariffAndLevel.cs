using System;
using System.Reflection;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	public class ClientTariffAndLevel : DocumentBrandingBusinessObject
	{
		public ClientTariffAndLevel()
		{
		}

		public ClientTariffAndLevel(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ClientTariffAndLevel(fallbackLevel, factory);
		}

		protected override IRegistryItem BrandingOptionRegistryItem
		{
			get { return DocumentsDataRegistry.Instance.EnableClientBranding; }
		}

		protected override string BrandingOptionTitle
		{
			get { return (NoResString)"Client Branding"; }
		}

		protected override CodeDescriptionPairList GetNewCodeList()
		{
			Assembly assembly = Assembly.Load("Enterprise.MasterFiles.Business");

			Type orgCodeListsType = assembly.GetType("Enterprise.MasterFiles.Business.OrgCodeLists");
			object orgCodeLists = Activator.CreateInstance(orgCodeListsType);

			MethodInfo info = orgCodeListsType.GetMethod("DefaultCompanyTariffLevels_List", BindingFlags.Public | BindingFlags.Instance);
			return (CodeDescriptionPairList)info.Invoke(orgCodeLists, new object[] { CurrentFactory });
		}
	}
}
