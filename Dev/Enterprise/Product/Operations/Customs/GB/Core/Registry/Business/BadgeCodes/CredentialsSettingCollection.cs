using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.GB.Registry.XmlSerializers")]
	public class CredentialsSettingCollection : RegistryBusinessObjectCollectionTemplate
	{
		public CredentialsSettingCollection()
			: base()
		{
		}

		public CredentialsSettingCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new CredentialsSetting this[int i]
		{
			get { return (CredentialsSetting)Elements[i]; }
		}

		public new CredentialsSetting AddNew()
		{
			return (CredentialsSetting)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CredentialsSettingCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CredentialsSetting(CurrentFallbackLevel, CurrentFactory);
		}

		public CredentialsSetting FindByBadgeCode(ZString badge)
		{
			if (!badge.IsEmpty)
			{
				foreach (CredentialsSetting credentialsSetting in this)
				{
					if (credentialsSetting.BadgeCode == badge)
					{
						return credentialsSetting;
					}
				}
			}
			return null;
		}
	}
}
