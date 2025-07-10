using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.EU.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.EU.Business.XmlSerializers")]
	public sealed class TemporaryStorageGroupNotification : GroupNotification
	{
		public TemporaryStorageGroupNotification()
		{
		}

		public TemporaryStorageGroupNotification(ZString sendMode, ZGuid sendGroupPK)
			: base(sendMode, sendGroupPK)
		{
		}

		public TemporaryStorageGroupNotification(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override CodeDescriptionPairList GetSendModeList()
		{
			return new CodeDescriptionPairList(OLookUpEditType.EmailTo);
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TemporaryStorageGroupNotification(fallbackLevel, factory);
		}
	}
}
