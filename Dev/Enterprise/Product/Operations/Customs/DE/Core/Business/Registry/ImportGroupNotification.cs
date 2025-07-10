using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.DE.Business.XmlSerializers")]
	public class ImportGroupNotification : GroupNotification
	{
		public ImportGroupNotification()
		{
		}

		public ImportGroupNotification(ZString sendMode, ZGuid sendGroupPK) : base(sendMode, sendGroupPK)
		{
		}

		public ImportGroupNotification(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override CodeDescriptionPairList GetSendModeList() => new CodeDescriptionPairList(OLookUpEditType.EmailTo);

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new ImportGroupNotification(fallbackLevel, factory);
	}
}
