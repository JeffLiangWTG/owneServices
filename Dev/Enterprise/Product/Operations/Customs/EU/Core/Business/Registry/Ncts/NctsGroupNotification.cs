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
	public sealed class NctsGroupNotification : GroupNotification
	{
		public NctsGroupNotification()
		{
		}

		public NctsGroupNotification(ZString sendMode, ZGuid sendGroupPk)
			: base(sendMode, sendGroupPk)
		{
		}

		public NctsGroupNotification(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override CodeDescriptionPairList GetSendModeList()
			=> new CodeDescriptionPairList(OLookUpEditType.EmailTo);

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			=> new NctsGroupNotification(fallbackLevel, factory);
	}
}
